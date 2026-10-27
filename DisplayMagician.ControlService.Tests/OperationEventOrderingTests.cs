using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class OperationEventOrderingTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagicianOperationEventTests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task ConcurrentCommitsNotifyInOrderWithoutHoldingTheStateLock()
    {
        OperationStatusStore store = new(new StoragePaths(_root));
        using ManualResetEventSlim firstEntered = new();
        using ManualResetEventSlim releaseFirst = new();
        List<long> observed = new();
        store.StatusUpdated += status =>
        {
            if (status.Sequence == 1)
            {
                firstEntered.Set();
                if (!releaseFirst.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            }
            observed.Add(status.Sequence);
        };
        Guid id = Guid.NewGuid();
        Task<OperationStatus> first = Task.Run(() => store.Publish("owner", 1, Update(id), DateTime.UtcNow));
        try
        {
            Assert.True(firstEntered.Wait(TimeSpan.FromSeconds(10)));
            Task<OperationStatus> second = Task.Run(() => store.Publish("owner", 1, Update(id), DateTime.UtcNow));
            Assert.Equal(2, (await second.WaitAsync(TimeSpan.FromSeconds(5))).Sequence);
            Assert.Equal(2, store.Get("owner", id)!.Sequence);
        }
        finally { releaseFirst.Set(); await first; }
        Assert.Equal(new long[] { 1, 2 }, observed);
    }

    [Fact]
    public void SubscriberMutationsAndExceptionsCannotChangeRepliesOrBlockOtherSubscribers()
    {
        OperationStatusStore store = new(new StoragePaths(_root));
        List<string> messages = new();
        store.StatusUpdated += status => { status.Message = "Changed"; throw new InvalidOperationException("Subscriber failed"); };
        store.StatusUpdated += status => messages.Add(status.Message);
        Guid id = Guid.NewGuid();
        OperationStatus result = store.Publish("owner", 1, Update(id), DateTime.UtcNow);
        Assert.Equal("Original", result.Message);
        Assert.Equal("Original", store.Get("owner", id)!.Message);
        Assert.Equal(new[] { "Original" }, messages);
        store.Publish("owner", 1, Update(id), DateTime.UtcNow);
        Assert.Equal(2, messages.Count);
    }

    [Fact]
    public async Task EventSubscriptionsOwnIndependentDeepSnapshots()
    {
        ControlClientEventHub hub = new();
        using ControlClientEventSubscription first = hub.Subscribe("owner", 1);
        using ControlClientEventSubscription second = hub.Subscribe("owner", 2);
        ControlClientEvent source = new() { Scope = ControlClientEventScope.User, EventType = ControlClientEventType.OperationDecisionUpdated,
            OperationDecision = new OperationDecision { Title = "Original", AllowedChoices = new[] { OperationDecisionChoice.Continue } } };
        hub.Publish("owner", 1, source);
        source.OperationDecision.Title = "Sender changed";
        ControlClientEvent firstEvent = await first.Reader.ReadAsync();
        firstEvent.OperationDecision!.AllowedChoices[0] = OperationDecisionChoice.StopAndRestore;
        ControlClientEvent secondEvent = await second.Reader.ReadAsync();
        Assert.Equal("Original", firstEvent.OperationDecision.Title);
        Assert.Equal("Original", secondEvent.OperationDecision!.Title);
        Assert.Equal(OperationDecisionChoice.Continue, Assert.Single(secondEvent.OperationDecision.AllowedChoices));
    }

    private static OperationStatusUpdate Update(Guid id) => new() { OperationId = id, OperationType = DisplayOperationType.StartShortcut, Phase = OperationPhase.StartingGame, Message = "Original" };
    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
