using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Retains successful mutating client responses so a lost reply cannot repeat an operation.</summary>
public sealed class ControlRequestReplayStore
{
    private static readonly TimeSpan Retention = TimeSpan.FromHours(24);
    private const int MaximumRecords = 1000;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _storagePath;
    private readonly List<ReplayRecord> _records = new List<ReplayRecord>();

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

            response = JsonSerializer.Deserialize<ControlResponse>(record.ResponsePayload)
                ?? new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ExecutionFailed, Message = "The prior request result could not be read." };
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
            RemoveExpiredUnsafe(utcNow);
            _records.RemoveAll(candidate => candidate.RequestId == request.RequestId && string.Equals(candidate.UserSid, userSid, StringComparison.OrdinalIgnoreCase));
            _records.Add(new ReplayRecord
            {
                UserSid = userSid,
                RequestId = request.RequestId,
                MessageType = request.MessageType,
                PayloadHash = GetPayloadHash(request.Payload),
                CompletedUtc = utcNow,
                ResponsePayload = JsonSerializer.Serialize(response)
            });
            if (_records.Count > MaximumRecords)
            {
                _records.RemoveRange(0, _records.Count - MaximumRecords);
            }
            PersistUnsafe();
        }
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_storagePath))
            {
                _records.AddRange(JsonSerializer.Deserialize<ReplayRecord[]>(File.ReadAllText(_storagePath)) ?? Array.Empty<ReplayRecord>());
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            _logger.Error(ex, "ControlRequestReplayStore/Load: Could not load request replay records from {0}.", _storagePath);
        }
    }

    private void RemoveExpiredUnsafe(DateTime utcNow)
    {
        _records.RemoveAll(record => utcNow - record.CompletedUtc > Retention);
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
        }
    }

    private static string GetPayloadHash(string payload)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload ?? string.Empty)));
    }

    private sealed class ReplayRecord
    {
        public string UserSid { get; set; } = string.Empty;
        public Guid RequestId { get; set; }
        public ControlMessageType MessageType { get; set; }
        public string PayloadHash { get; set; } = string.Empty;
        public DateTime CompletedUtc { get; set; }
        public string ResponsePayload { get; set; } = string.Empty;
    }
}
