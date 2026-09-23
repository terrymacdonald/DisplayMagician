using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Durably owns operation prompts so any authorized client can observe and resolve one exactly once.</summary>
public sealed class OperationDecisionStore
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _storagePath;
    private readonly Dictionary<Guid, OperationDecision> _decisions = new Dictionary<Guid, OperationDecision>();
    private readonly Dictionary<Guid, TaskCompletionSource<OperationDecision>> _pendingDecisions = new Dictionary<Guid, TaskCompletionSource<OperationDecision>>();

    public OperationDecisionStore(StoragePaths storagePaths)
    {
        ArgumentNullException.ThrowIfNull(storagePaths);
        _storagePath = Path.Combine(storagePaths.MachinePath, "OperationDecisions.json");
        Load();
    }

    public event Action<OperationDecision>? DecisionUpdated;

    public OperationDecision Create(string ownerUserSid, int ownerSessionId, Guid operationId, string title, string message, OperationDecisionChoice[] allowedChoices, OperationDecisionChoice defaultChoice, DateTime expiresUtc, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        if (operationId == Guid.Empty || allowedChoices == null || allowedChoices.Length == 0 || !allowedChoices.Contains(defaultChoice) || expiresUtc <= utcNow)
        {
            throw new ArgumentException("The operation decision is invalid.");
        }

        OperationDecision decision = new OperationDecision
        {
            PromptId = Guid.NewGuid(),
            OperationId = operationId,
            OwnerUserSid = ownerUserSid,
            OwnerSessionId = ownerSessionId,
            Title = title ?? string.Empty,
            Message = message ?? string.Empty,
            AllowedChoices = allowedChoices.Distinct().ToArray(),
            DefaultChoice = defaultChoice,
            CreatedUtc = utcNow,
            ExpiresUtc = expiresUtc
        };
        lock (_syncRoot)
        {
            _decisions.Add(decision.PromptId, decision);
            _pendingDecisions.Add(decision.PromptId, new TaskCompletionSource<OperationDecision>(TaskCreationOptions.RunContinuationsAsynchronously));
            Persist();
        }

        Publish(decision);
        return Copy(decision);
    }

    public Task<OperationDecision> WaitForResolutionAsync(Guid promptId, CancellationToken cancellationToken)
    {
        Task<OperationDecision> completion;
        lock (_syncRoot)
        {
            if (!_decisions.TryGetValue(promptId, out OperationDecision? decision))
            {
                throw new ArgumentException("The operation decision was not found.", nameof(promptId));
            }

            if (decision.IsResolved)
            {
                return Task.FromResult(Copy(decision));
            }

            completion = _pendingDecisions[promptId].Task;
        }

        return completion.WaitAsync(cancellationToken);
    }

    public OperationDecision? Resolve(string ownerUserSid, int ownerSessionId, Guid promptId, OperationDecisionChoice choice, DateTime utcNow)
    {
        OperationDecision? resolved = null;
        lock (_syncRoot)
        {
            if (!_decisions.TryGetValue(promptId, out OperationDecision? decision) || !string.Equals(decision.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase) || decision.IsResolved || !decision.AllowedChoices.Contains(choice))
            {
                return null;
            }

            ResolveUnsafe(decision, choice, utcNow);
            Persist();
            resolved = Copy(decision);
        }

        Publish(resolved);
        CompleteWaiter(resolved);
        return resolved;
    }

    public OperationDecision[] Expire(DateTime utcNow)
    {
        List<OperationDecision> expired = new List<OperationDecision>();
        lock (_syncRoot)
        {
            foreach (OperationDecision decision in _decisions.Values.Where(decision => !decision.IsResolved && decision.ExpiresUtc <= utcNow))
            {
                ResolveUnsafe(decision, decision.DefaultChoice, utcNow);
                expired.Add(Copy(decision));
            }

            if (expired.Count > 0)
            {
                Persist();
            }
        }

        foreach (OperationDecision decision in expired)
        {
            Publish(decision);
            CompleteWaiter(decision);
        }

        return expired.ToArray();
    }

    public OperationDecision[] GetPending(string ownerUserSid, int ownerSessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        lock (_syncRoot)
        {
            return _decisions.Values
                .Where(decision => !decision.IsResolved && string.Equals(decision.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase))
                .OrderBy(decision => decision.CreatedUtc)
                .Select(Copy)
                .ToArray();
        }
    }

    private void ResolveUnsafe(OperationDecision decision, OperationDecisionChoice choice, DateTime utcNow)
    {
        decision.IsResolved = true;
        decision.ResolvedChoice = choice;
        decision.ResolvedUtc = utcNow;
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_storagePath))
            {
                return;
            }

            OperationDecision[]? decisions = JsonSerializer.Deserialize<OperationDecision[]>(File.ReadAllText(_storagePath));
            foreach (OperationDecision decision in decisions ?? Array.Empty<OperationDecision>())
            {
                if (decision.PromptId != Guid.Empty && decision.OperationId != Guid.Empty && !string.IsNullOrWhiteSpace(decision.OwnerUserSid))
                {
                    _decisions[decision.PromptId] = decision;
                    if (!decision.IsResolved)
                    {
                        _pendingDecisions[decision.PromptId] = new TaskCompletionSource<OperationDecision>(TaskCreationOptions.RunContinuationsAsynchronously);
                    }
                }
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            _logger.Error(ex, "OperationDecisionStore/Load: Could not load persisted operation decisions from {0}.", _storagePath);
        }
    }

    private void Persist()
    {
        try
        {
            string json = JsonSerializer.Serialize(_decisions.Values.OrderBy(decision => decision.CreatedUtc).ToArray());
            AtomicFileStore.WriteAllText(_storagePath, json, $"{_storagePath}.bak");
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            _logger.Error(ex, "OperationDecisionStore/Persist: Could not persist operation decisions to {0}.", _storagePath);
        }
    }

    private void Publish(OperationDecision decision)
    {
        DecisionUpdated?.Invoke(Copy(decision));
    }

    private void CompleteWaiter(OperationDecision decision)
    {
        TaskCompletionSource<OperationDecision>? waiter;
        lock (_syncRoot)
        {
            _pendingDecisions.Remove(decision.PromptId, out waiter);
        }

        waiter?.TrySetResult(Copy(decision));
    }

    private static OperationDecision Copy(OperationDecision decision)
    {
        return new OperationDecision
        {
            PromptId = decision.PromptId,
            OperationId = decision.OperationId,
            OwnerUserSid = decision.OwnerUserSid,
            OwnerSessionId = decision.OwnerSessionId,
            Title = decision.Title,
            Message = decision.Message,
            AllowedChoices = decision.AllowedChoices.ToArray(),
            DefaultChoice = decision.DefaultChoice,
            CreatedUtc = decision.CreatedUtc,
            ExpiresUtc = decision.ExpiresUtc,
            IsResolved = decision.IsResolved,
            ResolvedChoice = decision.ResolvedChoice,
            ResolvedUtc = decision.ResolvedUtc
        };
    }
}
