using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>
/// Central, service-owned operation state for every controller. User Agents publish
/// updates here; local and future remote clients query or subscribe here.
/// </summary>
public sealed class OperationStatusStore
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _storagePath;
    private readonly Dictionary<Guid, OperationStatus> _operations = new Dictionary<Guid, OperationStatus>();
    private readonly Queue<OperationStatus> _notifications = new Queue<OperationStatus>();
    private bool _publishingNotifications;
    private bool _writesEnabled = true;
    private bool _persistenceDirty;
    private readonly Dictionary<Guid, HashSet<Guid>> _acceptedUpdates = new Dictionary<Guid, HashSet<Guid>>();

    public OperationStatusStore(StoragePaths storagePaths)
    {
        ArgumentNullException.ThrowIfNull(storagePaths);
        _storagePath = Path.Combine(storagePaths.MachinePath, "OperationStatuses.json");
        Load();
    }

    public event Action<OperationStatus>? StatusUpdated;

    internal OperationStatus[] GetReplayState()
    {
        lock (_syncRoot) return _operations.Values.Select(Copy).ToArray();
    }

    public OperationStatus Publish(string ownerUserSid, int ownerSessionId, OperationStatusUpdate update, DateTime utcNow)
        => Publish(ownerUserSid, ownerSessionId, update, utcNow, out _);

    public OperationStatus Publish(string ownerUserSid, int ownerSessionId, OperationStatusUpdate update, DateTime utcNow, out bool isPersisted)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        ArgumentNullException.ThrowIfNull(update);
        if (!OperationStatusValidation.IsValidUpdate(update) || ownerSessionId < 0 || utcNow == default)
        {
            throw new ArgumentException("The operation status update is invalid.", nameof(update));
        }

        OperationStatus status;
        lock (_syncRoot)
        {
            if (_operations.TryGetValue(update.OperationId, out OperationStatus? existing))
            {
                if (!string.Equals(existing.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase) || existing.OwnerSessionId != ownerSessionId)
                {
                    throw new InvalidOperationException("An operation can only be updated by its owning User Agent session.");
                }

                if (update.UpdateId != Guid.Empty && (existing.LastUpdateId == update.UpdateId ||
                    (_acceptedUpdates.TryGetValue(update.OperationId, out HashSet<Guid>? knownIds) && knownIds.Contains(update.UpdateId))))
                {
                    isPersisted = _writesEnabled && (!_persistenceDirty || TryPersist());
                    return Copy(existing);
                }

                if (existing.IsTerminal)
                {
                    isPersisted = _writesEnabled && (!_persistenceDirty || TryPersist());
                    return Copy(existing);
                }

                status = Copy(existing);
                status.Sequence++;
            }
            else
            {
                status = new OperationStatus
                {
                    OperationId = update.OperationId,
                    OwnerUserSid = ownerUserSid,
                    OwnerSessionId = ownerSessionId,
                    Sequence = 1,
                    StartedUtc = utcNow
                };
            }

            status.OperationType = update.OperationType;
            status.Phase = update.Phase;
            status.Message = update.Message ?? string.Empty;
            status.UpdatedUtc = utcNow;
            status.IsTerminal = update.IsTerminal;
            status.IsSuccessful = update.IsSuccessful;
            status.ErrorCode = update.ErrorCode;
            status.LastUpdateId = update.UpdateId;
            _operations[status.OperationId] = status;
            if (update.UpdateId != Guid.Empty)
            {
                if (!_acceptedUpdates.TryGetValue(update.OperationId, out HashSet<Guid>? accepted))
                    _acceptedUpdates.Add(update.OperationId, accepted = new HashSet<Guid>());
                accepted.Add(update.UpdateId);
            }
            PruneCompletedOperationsUnsafe(utcNow);
            isPersisted = TryPersist();
            _notifications.Enqueue(Copy(status));
        }

        PublishNotifications();
        return Copy(status);
    }

    public OperationStatus? Get(string ownerUserSid, Guid operationId)
    {
        lock (_syncRoot)
        {
            if (PruneCompletedOperationsUnsafe(DateTime.UtcNow) || _persistenceDirty) TryPersist();
            return _operations.TryGetValue(operationId, out OperationStatus? status) && string.Equals(status.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase)
                ? Copy(status)
                : null;
        }
    }

    public OperationStatus[] GetAll(string ownerUserSid)
    {
        lock (_syncRoot)
        {
            if (PruneCompletedOperationsUnsafe(DateTime.UtcNow) || _persistenceDirty) TryPersist();
            return _operations.Values
                .Where(status => string.Equals(status.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(status => status.UpdatedUtc)
                .Select(Copy)
                .ToArray();
        }
    }

    public OperationStatus[] GetChangedSince(string ownerUserSid, DateTime changedSinceUtc)
    {
        lock (_syncRoot)
        {
            if (PruneCompletedOperationsUnsafe(DateTime.UtcNow) || _persistenceDirty) TryPersist();
            return _operations.Values
                .Where(status => string.Equals(status.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase) && status.UpdatedUtc >= changedSinceUtc.ToUniversalTime())
                .OrderBy(status => status.UpdatedUtc)
                .Select(Copy)
                .ToArray();
        }
    }

    public OperationStatus[] GetActive(string ownerUserSid)
    {
        lock (_syncRoot)
        {
            return _operations.Values
                .Where(status => !status.IsTerminal && string.Equals(status.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(status => status.UpdatedUtc)
                .Select(Copy)
                .ToArray();
        }
    }

    public OperationStatus[] GetActive(string ownerUserSid, int ownerSessionId)
    {
        lock (_syncRoot)
        {
            return _operations.Values
                .Where(status => !status.IsTerminal && string.Equals(status.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase) && status.OwnerSessionId == ownerSessionId)
                .OrderByDescending(status => status.UpdatedUtc)
                .Select(Copy)
                .ToArray();
        }
    }

    public void RefreshAuthority(AgentStatus[] agents)
    {
        ArgumentNullException.ThrowIfNull(agents);
        List<OperationStatus> changedStatuses = new List<OperationStatus>();
        lock (_syncRoot)
        {
            foreach (OperationStatus status in _operations.Values.Where(status => !status.IsTerminal))
            {
                AgentStatus? agent = agents.FirstOrDefault(candidate => candidate.SessionId == status.OwnerSessionId && string.Equals(candidate.UserSid, status.OwnerUserSid, StringComparison.OrdinalIgnoreCase));
                bool isAuthoritative = agent?.IsReady == true && agent.IsHealthy;
                string staleReason = isAuthoritative ? string.Empty : agent == null ? "The User Agent is disconnected." : "The User Agent has not sent a recent heartbeat.";
                if (status.IsAuthoritative == isAuthoritative && status.IsStale == !isAuthoritative && string.Equals(status.StaleReason, staleReason, StringComparison.Ordinal)) continue;
                status.IsAuthoritative = isAuthoritative;
                status.IsStale = !isAuthoritative;
                status.StaleReason = staleReason;
                status.Sequence++;
                changedStatuses.Add(Copy(status));
            }
            if (changedStatuses.Count > 0 || _persistenceDirty)
            {
                TryPersist();
                foreach (OperationStatus status in changedStatuses) _notifications.Enqueue(status);
            }
        }
        PublishNotifications();
    }

    private void PublishNotifications()
    {
        lock (_syncRoot)
        {
            if (_publishingNotifications) return;
            _publishingNotifications = true;
        }
        while (true)
        {
            OperationStatus status;
            lock (_syncRoot)
            {
                if (_notifications.Count == 0)
                {
                    _publishingNotifications = false;
                    return;
                }
                status = _notifications.Dequeue();
            }
            // Preserve commit order without invoking subscribers under the state lock.
            using IDisposable operationScope = SupportLogScope.BeginOperation(status.OperationId);
            foreach (Action<OperationStatus> subscriber in StatusUpdated?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                try { subscriber(Copy(status)); }
                catch (Exception ex) { _logger.Error(ex, "OperationStatusStore/PublishNotifications: Could not notify a subscriber for operation {0} sequence {1}.", status.OperationId, status.Sequence); }
            }
        }
    }

    private void Load()
    {
        try
        {
            string json = File.ReadAllText(_storagePath);
            using JsonDocument document = JsonDocument.Parse(json);
            OperationStatus[] statuses;
            PersistedOperation[] records;
            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                // Legacy version 1 was a plain status array. First replacement backs
                // it up through the existing atomic writer before writing version 2.
                statuses = JsonSerializer.Deserialize<OperationStatus[]>(json) ?? throw new JsonException("The operation-status array is missing.");
                records = Array.Empty<PersistedOperation>();
            }
            else
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object ||
                    !document.RootElement.TryGetProperty("SchemaVersion", out JsonElement version) || version.ValueKind != JsonValueKind.Number || !version.TryGetInt32(out int schema) || schema != 2 ||
                    !document.RootElement.TryGetProperty("Operations", out JsonElement operations) || operations.ValueKind != JsonValueKind.Array)
                    throw new JsonException("The operation-status file is incomplete or uses an unsupported schema.");
                PersistedStatuses stored = JsonSerializer.Deserialize<PersistedStatuses>(json) ?? throw new JsonException("The status document is missing.");
                foreach (JsonElement record in operations.EnumerateArray())
                    if (record.ValueKind != JsonValueKind.Object || !record.TryGetProperty("Status", out JsonElement status) || status.ValueKind != JsonValueKind.Object ||
                        !record.TryGetProperty("AcceptedUpdateIds", out JsonElement ids) || ids.ValueKind != JsonValueKind.Array)
                        throw new JsonException("The operation-status delivery record is incomplete.");
                records = stored.Operations;
                if (records.Any(record => record == null || record.Status == null || record.AcceptedUpdateIds == null ||
                    record.AcceptedUpdateIds.Any(id => id == Guid.Empty) || record.AcceptedUpdateIds.Distinct().Count() != record.AcceptedUpdateIds.Length ||
                    (record.Status.LastUpdateId != Guid.Empty && !record.AcceptedUpdateIds.Contains(record.Status.LastUpdateId))))
                    throw new JsonException("The operation-status file contains invalid delivery records.");
                statuses = records.Select(record => record.Status).ToArray();
            }
            if (statuses.Any(status => !OperationStatusValidation.IsValidStatus(status)) || statuses.GroupBy(status => status.OperationId).Any(group => group.Count() != 1))
                throw new JsonException("The operation-status file contains invalid or duplicate records.");
            foreach (OperationStatus status in statuses)
            {
                _operations[status.OperationId] = status;
                _acceptedUpdates[status.OperationId] = status.LastUpdateId == Guid.Empty ? new HashSet<Guid>() : new HashSet<Guid> { status.LastUpdateId };
            }
            foreach (PersistedOperation record in records) _acceptedUpdates[record.Status.OperationId] = new HashSet<Guid>(record.AcceptedUpdateIds);
            if (PruneCompletedOperationsUnsafe(DateTime.UtcNow)) TryPersist();
        }
        catch (FileNotFoundException) when (!File.Exists(_storagePath + ".bak") && !Directory.Exists(_storagePath + ".bak")) { /* No prior status history exists. */ }
        catch (DirectoryNotFoundException) when (!File.Exists(_storagePath + ".bak") && !Directory.Exists(_storagePath + ".bak")) { /* New installation; the writer creates the directory. */ }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            _writesEnabled = false;
            _logger.Error(ex, "OperationStatusStore/Load: Could not load {0}. Preserving the status files until repair/restart; operations continue with in-memory status tracking.", _storagePath);
        }
    }

    private bool TryPersist()
    {
        _persistenceDirty = true;
        if (!_writesEnabled) return false;
        try
        {
            string json = JsonSerializer.Serialize(new PersistedStatuses { Operations = _operations.Values.OrderBy(status => status.UpdatedUtc)
                .Select(status => new PersistedOperation { Status = status, AcceptedUpdateIds = _acceptedUpdates.TryGetValue(status.OperationId, out HashSet<Guid>? ids) ? ids.ToArray() : Array.Empty<Guid>() }).ToArray() });
            AtomicFileStore.WriteAllText(_storagePath, json, $"{_storagePath}.bak");
            _persistenceDirty = false;
            return true;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            _logger.Error(ex, "OperationStatusStore/TryPersist: Could not persist operation statuses to {0}; keeping them in memory for retry.", _storagePath);
            return false;
        }
    }

    private bool PruneCompletedOperationsUnsafe(DateTime utcNow)
    {
        Guid[] expiredOperationIds = _operations.Values
            .Where(status => status.IsTerminal && status.UpdatedUtc.ToUniversalTime() <= utcNow.ToUniversalTime() - ControlProtocol.CompletedOperationLifetime)
            .Select(status => status.OperationId)
            .ToArray();
        Guid[] excessOperationIds = _operations.Values
            .Where(status => status.IsTerminal && !expiredOperationIds.Contains(status.OperationId))
            .GroupBy(status => status.OwnerUserSid, StringComparer.OrdinalIgnoreCase)
            .SelectMany(group => group.OrderByDescending(status => status.UpdatedUtc).ThenBy(status => status.OperationId)
                .Skip(ControlProtocol.MaximumCompletedOperationsPerUser))
            .Select(status => status.OperationId)
            .ToArray();
        foreach (Guid operationId in expiredOperationIds.Concat(excessOperationIds))
        {
            _operations.Remove(operationId);
            _acceptedUpdates.Remove(operationId);
        }
        return expiredOperationIds.Length + excessOperationIds.Length > 0;
    }

    private sealed class PersistedStatuses
    {
        public int SchemaVersion { get; set; } = 2;
        public PersistedOperation[] Operations { get; set; } = Array.Empty<PersistedOperation>();
    }

    private sealed class PersistedOperation
    {
        public OperationStatus Status { get; set; } = new OperationStatus();
        public Guid[] AcceptedUpdateIds { get; set; } = Array.Empty<Guid>();
    }

    private static OperationStatus Copy(OperationStatus status)
    {
        return new OperationStatus
        {
            OperationId = status.OperationId,
            OperationType = status.OperationType,
            OwnerUserSid = status.OwnerUserSid,
            OwnerSessionId = status.OwnerSessionId,
            Sequence = status.Sequence,
            Phase = status.Phase,
            Message = status.Message,
            StartedUtc = status.StartedUtc,
            UpdatedUtc = status.UpdatedUtc,
            IsTerminal = status.IsTerminal,
            IsSuccessful = status.IsSuccessful,
            ErrorCode = status.ErrorCode,
            LastUpdateId = status.LastUpdateId,
            IsAuthoritative = status.IsAuthoritative,
            IsStale = status.IsStale,
            StaleReason = status.StaleReason
        };
    }
}
