using System;
using System.Collections.Generic;
using System.Linq;
using DisplayMagician.Contracts;

namespace DisplayMagician;

/// <summary>Desktop event ordering guards; authoritative operation state remains in ControlService.</summary>
internal sealed class OperationEventTracker
{
    private readonly object _syncRoot = new object();
    private readonly Dictionary<Guid, (long Sequence, bool Terminal, DateTime RetainUntil, long Order)> _statuses = new();
    private readonly Dictionary<Guid, DateTime> _resolvedPrompts = new();
    private long _nextOrder;

    internal bool TryAcceptStatus(OperationStatus status, DateTime utcNow)
    {
        if (status == null || status.OperationId == Guid.Empty || status.Sequence <= 0) return false;
        lock (_syncRoot)
        {
            foreach (Guid id in _statuses.Where(entry => entry.Value.Terminal && entry.Value.RetainUntil <= utcNow).Select(entry => entry.Key).ToArray())
                _statuses.Remove(id);
            if (_statuses.TryGetValue(status.OperationId, out var previous) && (previous.Terminal || status.Sequence <= previous.Sequence)) return false;
            _statuses[status.OperationId] = (status.Sequence, status.IsTerminal, utcNow.Add(ControlProtocol.CompletedOperationLifetime), ++_nextOrder);
            foreach (Guid id in _statuses.Where(entry => entry.Value.Terminal).OrderByDescending(entry => entry.Value.Order)
                .Skip(ControlProtocol.MaximumCompletedOperationsPerUser).Select(entry => entry.Key).ToArray())
                _statuses.Remove(id);
            return true;
        }
    }

    internal bool TryAcceptDecision(OperationDecision decision, DateTime utcNow)
    {
        if (decision == null || decision.PromptId == Guid.Empty) return false;
        lock (_syncRoot)
        {
            foreach (Guid id in _resolvedPrompts.Where(entry => entry.Value <= utcNow).Select(entry => entry.Key).ToArray()) _resolvedPrompts.Remove(id);
            if (decision.IsResolved)
            {
                if (decision.ExpiresUtc > utcNow) _resolvedPrompts[decision.PromptId] = decision.ExpiresUtc;
                return true;
            }
            return decision.ExpiresUtc > utcNow && !_resolvedPrompts.ContainsKey(decision.PromptId);
        }
    }

    internal void ResetActiveSequences()
    {
        lock (_syncRoot)
            foreach (Guid id in _statuses.Where(entry => !entry.Value.Terminal).Select(entry => entry.Key).ToArray()) _statuses.Remove(id);
    }
}
