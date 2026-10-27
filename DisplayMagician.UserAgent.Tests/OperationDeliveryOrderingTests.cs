using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;
using DisplayMagician.UIForms;
using DisplayMagician.UserAgent.Runtime;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

[Collection("Agent repository state")]
public sealed class OperationDeliveryOrderingTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagicianOperationDeliveryTests", Guid.NewGuid().ToString("N"));
    private readonly List<(FieldInfo Field, object? Value)> _original = new();

    public OperationDeliveryOrderingTests()
    {
        foreach (Type type in new[] { typeof(ProfileRepository), typeof(AudioProfileRepository) })
            foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(field => !field.IsInitOnly && !field.IsLiteral))
                _original.Add((field, field.GetValue(null)));
    }

    [Fact]
    public async Task OverlappingFlushAndReconciliationDeliverEachPhaseInOrder()
    {
        FakeClient client = new() { BlockFirst = true };
        ProfileCommandHandler handler = Handler(client);
        Guid id = Guid.NewGuid();
        Outbox(handler).Enqueue(Update(id, OperationPhase.StartingGame));
        Outbox(handler).Enqueue(Update(id, OperationPhase.WaitingForGameToClose));
        Task first = handler.FlushPendingOperationStatusUpdatesAsync(CancellationToken.None);
        Task second = handler.FlushPendingOperationStatusUpdatesAsync(CancellationToken.None);
        Task reconcile = handler.ReconcileOperationStatusesAsync(CancellationToken.None);
        Assert.Single(client.Sent);
        Assert.Empty(client.Reconciled);
        client.ReleaseFirst.SetResult(true);
        await Task.WhenAll(first, second, reconcile);
        Assert.Equal(new[] { OperationPhase.StartingGame, OperationPhase.WaitingForGameToClose }, client.Sent.Select(update => update.Phase));
        Assert.Equal(OperationPhase.WaitingForGameToClose, Assert.Single(Assert.Single(client.Reconciled)).Phase);
        Assert.Empty(Outbox(handler).GetPending());
    }

    [Fact]
    public async Task NewUpdatesWaitForDeliveryBeforeEnteringTheActiveSnapshot()
    {
        FakeClient client = new() { BlockFirst = true };
        ProfileCommandHandler handler = Handler(client);
        Guid id = Guid.NewGuid();
        Task first = Publish(handler, Update(id, OperationPhase.StartingGame));
        Task next = Publish(handler, Update(id, OperationPhase.WaitingForGameToClose));
        Assert.Equal(OperationPhase.StartingGame, Assert.Single(Outbox(handler).GetActive()).Phase);
        Assert.Single(client.Sent);
        client.ReleaseFirst.SetResult(true);
        await Task.WhenAll(first, next);
        Assert.Equal(new[] { OperationPhase.StartingGame, OperationPhase.WaitingForGameToClose }, client.Sent.Select(update => update.Phase));
    }

    [Fact]
    public async Task FailedDeliveryDoesNotReconcileNewerStateAheadOfThePendingQueue()
    {
        FakeClient client = new() { FailNext = true };
        ProfileCommandHandler handler = Handler(client);
        Guid id = Guid.NewGuid();
        Outbox(handler).Enqueue(Update(id, OperationPhase.StartingGame));
        Outbox(handler).Enqueue(Update(id, OperationPhase.WaitingForGameToClose));
        await handler.ReconcileOperationStatusesAsync(CancellationToken.None);
        Assert.Empty(client.Reconciled);
        Assert.Equal(2, Outbox(handler).GetPending().Length);
        await handler.ReconcileOperationStatusesAsync(CancellationToken.None);
        Assert.Equal(new[] { OperationPhase.StartingGame, OperationPhase.StartingGame, OperationPhase.WaitingForGameToClose }, client.Sent.Select(update => update.Phase));
        Assert.Equal(client.Sent[0].UpdateId, client.Sent[1].UpdateId);
        Assert.Equal(OperationPhase.WaitingForGameToClose, Assert.Single(Assert.Single(client.Reconciled)).Phase);
    }

    [Fact]
    public async Task CancellingAQueuedFlushDoesNotReleaseAnotherCallersGate()
    {
        FakeClient client = new() { BlockFirst = true };
        ProfileCommandHandler handler = Handler(client);
        Outbox(handler).Enqueue(Update(Guid.NewGuid(), OperationPhase.StartingGame));
        Task first = handler.FlushPendingOperationStatusUpdatesAsync(CancellationToken.None);
        using CancellationTokenSource cancel = new();
        Task queued = handler.FlushPendingOperationStatusUpdatesAsync(cancel.Token);
        cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => queued);
        Assert.Single(client.Sent);
        client.ReleaseFirst.SetResult(true);
        await first;
        await handler.FlushPendingOperationStatusUpdatesAsync(CancellationToken.None);
        Assert.Single(client.Sent);
    }

    [Fact]
    public async Task StaleCancellationCannotCancelTheNextOperation()
    {
        ProfileCommandHandler handler = Handler(new FakeClient());
        using CancellationTokenSource previous = new();
        Guid previousId = Guid.NewGuid();
        Assert.True((bool)Call(handler, "TryBeginOperation", previousId, previous, DisplayOperationType.StartShortcut)!);
        Call(handler, "CompleteOperation", previousId, previous);
        using CancellationTokenSource current = new();
        Guid currentId = Guid.NewGuid();
        Assert.True((bool)Call(handler, "TryBeginOperation", currentId, current, DisplayOperationType.StartShortcut)!);
        ControlResponse stale = await handler.HandleAsync(new ControlEnvelope { MessageType = ControlMessageType.CancelOperation,
            Payload = System.Text.Json.JsonSerializer.Serialize(new CancelOperationRequest { OperationId = previousId }) }, CancellationToken.None);
        Assert.False(stale.IsSuccessful);
        Assert.False(current.IsCancellationRequested);
        ControlResponse matching = await handler.HandleAsync(new ControlEnvelope { MessageType = ControlMessageType.CancelOperation,
            Payload = System.Text.Json.JsonSerializer.Serialize(new CancelOperationRequest { OperationId = currentId }) }, CancellationToken.None);
        Assert.True(matching.IsSuccessful);
        Assert.True(current.IsCancellationRequested);
        Call(handler, "CompleteOperation", currentId, current);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(9)]
    public void CompletedDesktopOperationsRejectDelayedDuplicateAndNewerEvents(long sequence)
    {
        global::DisplayMagician.OperationEventTracker tracker = new();
        DateTime now = DateTime.UtcNow;
        Guid id = Guid.NewGuid();
        Assert.True(tracker.TryAcceptStatus(new OperationStatus { OperationId = id, Sequence = 3, IsTerminal = true }, now));
        Assert.False(tracker.TryAcceptStatus(new OperationStatus { OperationId = id, Sequence = sequence, IsTerminal = sequence == 3 }, now));
        Assert.True(tracker.TryAcceptStatus(new OperationStatus { OperationId = Guid.NewGuid(), Sequence = 1 }, now));
    }

    [Fact]
    public void ServiceRestartResetsActiveSequencesButPreservesCompletionGuards()
    {
        global::DisplayMagician.OperationEventTracker tracker = new();
        DateTime now = DateTime.UtcNow;
        Guid active = Guid.NewGuid();
        Guid completed = Guid.NewGuid();
        Assert.True(tracker.TryAcceptStatus(new OperationStatus { OperationId = active, Sequence = 90 }, now));
        Assert.False(tracker.TryAcceptStatus(new OperationStatus { OperationId = active, Sequence = 1 }, now));
        Assert.True(tracker.TryAcceptStatus(new OperationStatus { OperationId = completed, Sequence = 90, IsTerminal = true }, now));
        tracker.ResetActiveSequences();
        Assert.True(tracker.TryAcceptStatus(new OperationStatus { OperationId = active, Sequence = 1 }, now));
        Assert.False(tracker.TryAcceptStatus(new OperationStatus { OperationId = completed, Sequence = 1 }, now));
    }

    [Fact]
    public void CompletionGuardsUseTheSharedHistoryLimitAndLifetime()
    {
        global::DisplayMagician.OperationEventTracker tracker = new();
        DateTime now = DateTime.UtcNow;
        Guid[] ids = Enumerable.Range(0, ControlProtocol.MaximumCompletedOperationsPerUser + 1).Select(_ => Guid.NewGuid()).ToArray();
        foreach (Guid id in ids) Assert.True(tracker.TryAcceptStatus(new OperationStatus { OperationId = id, Sequence = 1, IsTerminal = true }, now));
        Assert.False(tracker.TryAcceptStatus(new OperationStatus { OperationId = ids[^1], Sequence = 2 }, now));
        Assert.True(tracker.TryAcceptStatus(new OperationStatus { OperationId = ids[0], Sequence = 2 }, now));
        Assert.True(tracker.TryAcceptStatus(new OperationStatus { OperationId = ids[^1], Sequence = 2 }, now.Add(ControlProtocol.CompletedOperationLifetime)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResolvedOrExpiredDecisionsCannotBeReopenedByAnOldSnapshot(bool expired)
    {
        global::DisplayMagician.OperationEventTracker tracker = new();
        DateTime now = DateTime.UtcNow;
        Guid id = Guid.NewGuid();
        DateTime expires = expired ? now : now.AddMinutes(1);
        Assert.True(tracker.TryAcceptDecision(new OperationDecision { PromptId = id, IsResolved = true, ExpiresUtc = expires }, now));
        Assert.False(tracker.TryAcceptDecision(new OperationDecision { PromptId = id, ExpiresUtc = expires }, now));
        Assert.True(tracker.TryAcceptDecision(new OperationDecision { PromptId = Guid.NewGuid(), ExpiresUtc = now.AddMinutes(1) }, now));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReconnectReconcilesOnlyDialogsThatPredateThePendingRead(bool stillPending) => WinFormsTestRunner.Run(async () =>
    {
        DecisionClient client = new();
        global::DisplayMagician.OperationEventTracker tracker = new();
        OperationDecision original = Decision();
        using OperationDecisionForm existing = new(original);
        Dictionary<Guid, OperationDecisionForm> forms = new() { [original.PromptId] = existing };
        Task refresh = global::DisplayMagician.Program.RefreshOperationDecisionFormsAsync(client, forms, tracker);
        OperationDecision later = Decision();
        using OperationDecisionForm addedDuringRead = new(later);
        forms.Add(later.PromptId, addedDuringRead);
        client.Reply.SetResult(new ControlResponse { IsSuccessful = true, OperationDecisions = stillPending ? new[] { original } : Array.Empty<OperationDecision>() });
        await refresh;
        Assert.Equal(!stillPending, existing.WasResolvedByAnotherClient);
        Assert.Equal(stillPending, forms.ContainsKey(original.PromptId));
        Assert.False(addedDuringRead.IsDisposed);
        Assert.True(forms.ContainsKey(later.PromptId));
        if (!stillPending) Assert.False(tracker.TryAcceptDecision(original, DateTime.UtcNow));
    });

    [Fact]
    public void FailedPendingReadLeavesTheDialogUsable() => WinFormsTestRunner.Run(async () =>
    {
        DecisionClient client = new();
        OperationDecision decision = Decision();
        using OperationDecisionForm form = new(decision);
        Dictionary<Guid, OperationDecisionForm> forms = new() { [decision.PromptId] = form };
        Task refresh = global::DisplayMagician.Program.RefreshOperationDecisionFormsAsync(client, forms, new());
        client.Reply.SetException(new IOException("Service unavailable"));
        await refresh;
        Assert.False(form.IsDisposed);
        Assert.False(form.WasResolvedByAnotherClient);
        Assert.Single(forms);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeadlineCheckClosesOnlyExpiredPromptsWithoutSubmittingAChoice(bool expired) => WinFormsTestRunner.Run(() =>
    {
        OperationDecision decision = Decision();
        decision.ExpiresUtc = expired ? DateTime.UtcNow.AddMinutes(-1) : DateTime.UtcNow.AddMinutes(1);
        using OperationDecisionForm form = new(decision);
        Call(form, "deadline_timer_Tick", form, EventArgs.Empty);
        Assert.Equal(expired, form.HasExpired);
        Assert.Equal(OperationDecisionChoice.Continue, form.SelectedChoice);
        Assert.False(form.WasResolvedByAnotherClient);
        if (!expired) Assert.False(form.IsDisposed);
        return Task.CompletedTask;
    });

    private static OperationDecision Decision() => new() { PromptId = Guid.NewGuid(), ExpiresUtc = DateTime.UtcNow.AddMinutes(1), AllowedChoices = new[] { OperationDecisionChoice.Continue, OperationDecisionChoice.StopAndRestore } };

    private ProfileCommandHandler Handler(FakeClient client) => new(new AgentRegistration { UserSid = "test", SessionId = 1 }, client, null, _root, new UserProfileOperationService());
    private static OperationStatusOutbox Outbox(ProfileCommandHandler handler) => (OperationStatusOutbox)typeof(ProfileCommandHandler).GetField("_operationStatusOutbox", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(handler)!;
    private static OperationStatusUpdate Update(Guid id, OperationPhase phase) => new() { OperationId = id, OperationType = DisplayOperationType.StartShortcut, Phase = phase };
    private static object? Call(object owner, string name, params object[] args) => owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(owner, args);
    private static Task Publish(ProfileCommandHandler handler, OperationStatusUpdate update) => (Task)Call(handler, "PublishShortcutStatusAsync", update, CancellationToken.None)!;
    public void Dispose()
    {
        foreach ((FieldInfo field, object? value) in _original) field.SetValue(null, value);
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    private sealed class FakeClient : ControlServiceClient
    {
        public bool BlockFirst { get; set; }
        public bool FailNext { get; set; }
        public TaskCompletionSource<bool> ReleaseFirst { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public List<OperationStatusUpdate> Sent { get; } = new();
        public List<OperationStatusUpdate[]> Reconciled { get; } = new();
        public override async Task<OperationStatus> PublishOperationStatusAsync(AgentRegistration registration, OperationStatusUpdate update, CancellationToken cancellationToken)
        {
            Sent.Add(update);
            if (BlockFirst && Sent.Count == 1) await ReleaseFirst.Task.WaitAsync(cancellationToken);
            if (FailNext) { FailNext = false; throw new IOException("Unavailable service"); }
            return new OperationStatus { OperationId = update.OperationId, Sequence = Sent.Count };
        }
        public override Task ReconcileOperationStatusesAsync(AgentRegistration registration, OperationStatusUpdate[] activeUpdates, CancellationToken cancellationToken)
        { Reconciled.Add(activeUpdates); return Task.CompletedTask; }
    }

    private sealed class DecisionClient : global::DisplayMagician.ControlServicePipeClient
    {
        public TaskCompletionSource<ControlResponse> Reply { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override Task<ControlResponse> SendAsync(ControlEnvelope request, CancellationToken cancellationToken) => Reply.Task;
    }
}
