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
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        ArgumentNullException.ThrowIfNull(update);
        if (update.OperationId == Guid.Empty)
        {
            throw new ArgumentException("An operation ID is required.", nameof(update));
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

                if (update.UpdateId != Guid.Empty && existing.LastUpdateId == update.UpdateId)
                {
                    return Copy(existing);
                }

                if (existing.IsTerminal)
                {
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
            PruneCompletedOperationsUnsafe(utcNow);
            Persist();
            _notifications.Enqueue(Copy(status));
        }

        PublishNotifications();
        return Copy(status);
    }

    public OperationStatus? Get(string ownerUserSid, Guid operationId)
    {
        lock (_syncRoot)
        {
            if (PruneCompletedOperationsUnsafe(DateTime.UtcNow)) Persist();
            return _operations.TryGetValue(operationId, out OperationStatus? status) && string.Equals(status.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase)
                ? Copy(status)
                : null;
        }
    }

    public OperationStatus[] GetAll(string ownerUserSid)
    {
        lock (_syncRoot)
        {
            if (PruneCompletedOperationsUnsafe(DateTime.UtcNow)) Persist();
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
            if (PruneCompletedOperationsUnsafe(DateTime.UtcNow)) Persist();
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
            if (changedStatuses.Count > 0)
            {
                Persist();
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
            if (!File.Exists(_storagePath))
            {
                return;
            }

            OperationStatus[]? statuses = JsonSerializer.Deserialize<OperationStatus[]>(File.ReadAllText(_storagePath));
            if (statuses == null)
            {
                return;
            }

            foreach (OperationStatus status in statuses.Where(status => status.OperationId != Guid.Empty && !string.IsNullOrWhiteSpace(status.OwnerUserSid)))
            {
                _operations[status.OperationId] = status;
            }
            if (PruneCompletedOperationsUnsafe(DateTime.UtcNow)) Persist();
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            _logger.Error(ex, "OperationStatusStore/Load: Could not load persisted operation statuses from {0}.", _storagePath);
        }
    }

    private void Persist()
    {
        try
        {
            string json = JsonSerializer.Serialize(_operations.Values.OrderBy(status => status.UpdatedUtc).ToArray());
            AtomicFileStore.WriteAllText(_storagePath, json, $"{_storagePath}.bak");
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            _logger.Error(ex, "OperationStatusStore/Persist: Could not persist operation statuses to {0}.", _storagePath);
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
        }
        return expiredOperationIds.Length + excessOperationIds.Length > 0;
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
