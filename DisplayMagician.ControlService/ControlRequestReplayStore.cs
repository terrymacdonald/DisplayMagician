using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Reserves local mutations durably and retains results so uncertain work is not repeated.</summary>
public sealed class ControlRequestReplayStore
{
    private static readonly TimeSpan Retention = TimeSpan.FromHours(24);
    private const int MaximumRecords = 1000;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly SemaphoreSlim _executionLock = new SemaphoreSlim(1, 1);
    private readonly string _storagePath;
    private readonly List<ReplayRecord> _records = new List<ReplayRecord>();
    private volatile bool _isAvailable = true;
    public bool IsAvailable => _isAvailable;

    public async Task<ControlResponse> ExecuteAsync(string userSid, ControlEnvelope request, Func<Task<ControlResponse>> execute, CancellationToken cancellationToken)
    {
        bool replayable = IsReplayableMutation(request.MessageType);
        if (replayable) await _executionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!IsAvailable && !IsReadOnlyRequest(request.MessageType)) return UnavailableResponse();
            if (replayable && !TryBegin(userSid, request, DateTime.UtcNow, out ControlResponse existing)) return existing;
            ControlResponse response;
            try { response = await execute().ConfigureAwait(false); }
            catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is UnauthorizedAccessException || ex is InvalidOperationException || ex is TimeoutException)
            {
                _logger.Warn(ex, "ControlRequestReplayStore/ExecuteAsync: The result of request {0} could not be confirmed.", request.RequestId);
                response = new ControlResponse { ErrorCode = IsReadOnlyRequest(request.MessageType) ? ControlErrorCode.AgentUnavailable : ControlErrorCode.OutcomeUnknown,
                    Message = "The command result could not be confirmed. Inspect the current state before retrying a mutation." };
            }
            if (replayable)
            {
                try
                {
                    if (response.IsSuccessful) Store(userSid, request, response, DateTime.UtcNow);
                    else if (response.ErrorCode != ControlErrorCode.OutcomeUnknown) ReleaseFailedRequest(userSid, request);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException)
                {
                    _logger.Error(ex, "ControlRequestReplayStore/ExecuteAsync: Could not commit replay state for request {0}; its reservation is preserved.", request.RequestId);
                    response = new ControlResponse { ErrorCode = ControlErrorCode.OutcomeUnknown,
                        Message = "The command's replay result could not be saved. Its reservation was preserved; inspect the current state before retrying." };
                }
            }
            return response;
        }
        finally { if (replayable) _executionLock.Release(); }
    }

    public ControlRequestReplayStore(StoragePaths storagePaths)
    {
        ArgumentNullException.ThrowIfNull(storagePaths);
        _storagePath = Path.Combine(storagePaths.MachinePath, "ControlRequestReplay.json");
        Load();
    }

    public bool TryGet(string userSid, ControlEnvelope request, DateTime utcNow, out ControlResponse response)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userSid);
        ArgumentNullException.ThrowIfNull(request);
        lock (_syncRoot)
        {
            if (!IsAvailable)
            {
                response = UnavailableResponse();
                return true;
            }
            RemoveExpiredUnsafe(utcNow);
            ReplayRecord? record = _records.FirstOrDefault(candidate => candidate.RequestId == request.RequestId && string.Equals(candidate.UserSid, userSid, StringComparison.OrdinalIgnoreCase));
            if (record == null)
            {
                response = null!;
                return false;
            }

            if (record.MessageType != request.MessageType || !string.Equals(record.PayloadHash, GetPayloadHash(request.Payload), StringComparison.Ordinal))
            {
                response = new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A request ID cannot be reused for a different operation." };
                return true;
            }

            if (record.IsPending)
            {
                response = new ControlResponse { ErrorCode = ControlErrorCode.OutcomeUnknown, Message = "This request was reserved, but its outcome could not be confirmed. It will not be executed again automatically." };
                return true;
            }
            response = JsonSerializer.Deserialize<ControlResponse>(record.ResponsePayload)
                ?? new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ExecutionFailed, Message = "The prior request result could not be read." };
            return true;
        }
    }

    public bool TryBegin(string userSid, ControlEnvelope request, DateTime utcNow, out ControlResponse response)
    {
        lock (_syncRoot)
        {
            if (TryGet(userSid, request, utcNow, out response)) return false;
            if (_records.Count >= MaximumRecords)
            {
                response = new ControlResponse { ErrorCode = ControlErrorCode.ExecutionFailed, Message = "The replay ledger is full. Retry after a completed request expires; existing replay protection was preserved." };
                return false;
            }
            ReplayRecord reservation = new ReplayRecord { SchemaVersion = 2, UserSid = userSid, RequestId = request.RequestId,
                MessageType = request.MessageType, PayloadHash = GetPayloadHash(request.Payload), ReservedUtc = utcNow, IsPending = true };
            _records.Add(reservation);
            try { PersistUnsafe(); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                _records.Remove(reservation);
                response = UnavailableResponse();
                return false;
            }
            response = null!;
            return true;
        }
    }

    public void Store(string userSid, ControlEnvelope request, ControlResponse response, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userSid);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(response);
        lock (_syncRoot)
        {
            if (!IsAvailable) throw new InvalidOperationException("Local replay protection is unavailable.");
            RemoveExpiredUnsafe(utcNow);
            ReplayRecord? existing = _records.FirstOrDefault(candidate => candidate.RequestId == request.RequestId && string.Equals(candidate.UserSid, userSid, StringComparison.OrdinalIgnoreCase));
            string payloadHash = GetPayloadHash(request.Payload);
            string responsePayload = JsonSerializer.Serialize(response);
            if (existing != null && (existing.MessageType != request.MessageType || existing.PayloadHash != payloadHash ||
                (!existing.IsPending && existing.ResponsePayload != responsePayload)))
                throw new InvalidOperationException("A reserved request or completed result cannot be replaced by a different operation.");
            if (existing != null && !existing.IsPending) return;
            if (existing == null && _records.Count >= MaximumRecords) throw new InvalidOperationException("The replay ledger is full.");
            ReplayRecord completed = new ReplayRecord
            {
                SchemaVersion = 2,
                UserSid = userSid,
                RequestId = request.RequestId,
                MessageType = request.MessageType,
                PayloadHash = payloadHash,
                CompletedUtc = utcNow,
                ResponsePayload = responsePayload
            };
            if (existing != null) _records.Remove(existing);
            _records.Add(completed);
            try { PersistUnsafe(); }
            catch
            {
                _records.Remove(completed);
                if (existing != null) _records.Add(existing);
                throw;
            }
        }
    }

    public void ReleaseFailedRequest(string userSid, ControlEnvelope request)
    {
        lock (_syncRoot)
        {
            if (!IsAvailable) throw new InvalidOperationException("Local replay protection is unavailable.");
            ReplayRecord? pending = _records.FirstOrDefault(record => record.IsPending && record.RequestId == request.RequestId &&
                string.Equals(record.UserSid, userSid, StringComparison.OrdinalIgnoreCase) && record.MessageType == request.MessageType && record.PayloadHash == GetPayloadHash(request.Payload));
            if (pending == null) return;
            _records.Remove(pending);
            try { PersistUnsafe(); }
            catch { _records.Add(pending); throw; }
        }
    }

    internal static ControlResponse UnavailableResponse() => new ControlResponse { ErrorCode = ControlErrorCode.ExecutionFailed,
        Message = "Local replay protection is unavailable. Mutating requests are disabled until the replay ledger is repaired and Control Service is restarted." };

    internal static bool IsReadOnlyRequest(ControlMessageType type) => type is
        ControlMessageType.ListProfiles or ControlMessageType.ListGames or ControlMessageType.ListApps or ControlMessageType.ListShortcuts or
        ControlMessageType.GetLoggingSettings or ControlMessageType.ListMessages or ControlMessageType.GetAnonymousMetricsSettings or ControlMessageType.GetActiveWindowPlacementSession or
        ControlMessageType.ListAudioProfiles or ControlMessageType.GetRepositorySnapshot or ControlMessageType.ListOperationDecisions or
        ControlMessageType.GetOperationStatus or ControlMessageType.ListOperationStatuses or ControlMessageType.GetServiceStatus or
        ControlMessageType.GetGatewaySettings or ControlMessageType.GetGatewayIdentity or ControlMessageType.ListDevicePairingRequests or ControlMessageType.ListPairedClients;

    internal static bool IsReplayableMutation(ControlMessageType messageType)
    {
        return messageType is ControlMessageType.ApplyProfile or ControlMessageType.StartShortcut or ControlMessageType.SaveWindowPlacements or ControlMessageType.ClearWindowPlacements or ControlMessageType.CancelOperation or
            ControlMessageType.CreateProfileFromCurrent or ControlMessageType.RenameProfile or ControlMessageType.DeleteProfile or ControlMessageType.UpdateProfileFromCurrent or ControlMessageType.UpdateDisplayProfileSettings or
            ControlMessageType.ApplyAudioProfile or ControlMessageType.CreateAudioProfileFromCurrent or ControlMessageType.RenameAudioProfile or ControlMessageType.DeleteAudioProfile or ControlMessageType.UpdateAudioProfileFromCurrent or
            ControlMessageType.CommitRepositorySnapshot or ControlMessageType.SetMessageReadState or ControlMessageType.ResolveOperationDecision or ControlMessageType.UpdateAnonymousMetricsSettings or
            ControlMessageType.InitializeAnonymousMetrics or ControlMessageType.ForceReleaseDisplayControl or ControlMessageType.RecordRecoveryAdministration or ControlMessageType.RestartUserAgent or
            ControlMessageType.CreateUserSupportBundle or ControlMessageType.SetSharedLogLevel;
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_storagePath))
            {
                ReplayRecord[] records = JsonSerializer.Deserialize<ReplayRecord[]>(File.ReadAllText(_storagePath)) ?? throw new JsonException("Missing replay records.");
                foreach (ReplayRecord record in records)
                {
                    if (record == null || record.SchemaVersion is < 1 or > 2 || string.IsNullOrWhiteSpace(record.UserSid) || record.RequestId == Guid.Empty ||
                        record.PayloadHash == null || record.PayloadHash.Length != 64 || !record.PayloadHash.All(Uri.IsHexDigit) ||
                        (record.IsPending && (record.SchemaVersion != 2 || record.ReservedUtc == default)) ||
                        record.MessageType == ControlMessageType.Unknown || !Enum.IsDefined(record.MessageType) ||
                        (!record.IsPending && (record.CompletedUtc == default || string.IsNullOrWhiteSpace(record.ResponsePayload) || JsonSerializer.Deserialize<ControlResponse>(record.ResponsePayload) == null)))
                        throw new JsonException("The replay ledger contains an invalid record.");
                }
                if (records.GroupBy(record => (record.UserSid.ToUpperInvariant(), record.RequestId)).Any(group => group.Count() > 1))
                    throw new JsonException("The replay ledger contains duplicate request identities.");
                _records.AddRange(records);
            }
            else if (File.Exists(_storagePath + ".bak") || Directory.Exists(_storagePath))
                throw new IOException("Existing replay history is missing or inaccessible.");
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            _logger.Error(ex, "ControlRequestReplayStore/Load: Could not load request replay records from {0}.", _storagePath);
            _isAvailable = false;
        }
    }

    private void RemoveExpiredUnsafe(DateTime utcNow)
    {
        _records.RemoveAll(record => !record.IsPending && utcNow - record.CompletedUtc > Retention);
    }

    private void PersistUnsafe()
    {
        try
        {
            AtomicFileStore.WriteAllText(_storagePath, JsonSerializer.Serialize(_records.OrderBy(record => record.CompletedUtc).ToArray()), $"{_storagePath}.bak");
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            _logger.Error(ex, "ControlRequestReplayStore/PersistUnsafe: Could not persist request replay records to {0}.", _storagePath);
            _isAvailable = false;
            throw;
        }
    }

    private static string GetPayloadHash(string payload)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload ?? string.Empty)));
    }

    private sealed class ReplayRecord
    {
        // Existing unversioned records are completed version-1 entries. Version 2
        // adds reservations; the first atomic write retains the old file as backup.
        public int SchemaVersion { get; set; } = 1;
        public bool IsPending { get; set; }
        public DateTime ReservedUtc { get; set; }
        public string UserSid { get; set; } = string.Empty;
        public Guid RequestId { get; set; }
        public ControlMessageType MessageType { get; set; }
        public string PayloadHash { get; set; } = string.Empty;
        public DateTime CompletedUtc { get; set; }
        public string ResponsePayload { get; set; } = string.Empty;
    }
}
