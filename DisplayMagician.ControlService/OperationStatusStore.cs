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
    private const int MaximumCompletedOperations = 100;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _storagePath;
    private readonly Dictionary<Guid, OperationStatus> _operations = new Dictionary<Guid, OperationStatus>();

    public OperationStatusStore(StoragePaths storagePaths)
    {
        ArgumentNullException.ThrowIfNull(storagePaths);
        _storagePath = Path.Combine(storagePaths.MachinePath, "OperationStatuses.json");
        Load();
    }

    public event Action<OperationStatus>? StatusUpdated;

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
            RemoveOldCompletedOperations();
            Persist();
        }

        OperationStatus publishedStatus = Copy(status);
        StatusUpdated?.Invoke(publishedStatus);
        return publishedStatus;
    }

    public OperationStatus? Get(string ownerUserSid, Guid operationId)
    {
        lock (_syncRoot)
        {
            return _operations.TryGetValue(operationId, out OperationStatus? status) && string.Equals(status.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase)
                ? Copy(status)
                : null;
        }
    }

    public OperationStatus[] GetAll(string ownerUserSid)
    {
        lock (_syncRoot)
        {
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
            if (changedStatuses.Count > 0) Persist();
        }
        foreach (OperationStatus status in changedStatuses) StatusUpdated?.Invoke(status);
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

    private void RemoveOldCompletedOperations()
    {
        Guid[] obsoleteOperationIds = _operations.Values
            .Where(status => status.IsTerminal)
            .OrderByDescending(status => status.UpdatedUtc)
            .Skip(MaximumCompletedOperations)
            .Select(status => status.OperationId)
            .ToArray();
        foreach (Guid operationId in obsoleteOperationIds)
        {
            _operations.Remove(operationId);
        }
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
