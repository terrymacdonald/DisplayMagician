using System;
using System.IO;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class OperationStatusStoreTests
{
    [Fact]
    public void Publish_IncrementsSequenceAndNotifiesSubscribers()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        OperationStatusStore store = new OperationStatusStore(new StoragePaths(storageRoot));
        OperationStatus? notifiedStatus = null;
        store.StatusUpdated += status => notifiedStatus = status;
        Guid operationId = Guid.NewGuid();
        DateTime startedUtc = new DateTime(2026, 9, 20, 1, 2, 3, DateTimeKind.Utc);

        OperationStatus firstStatus = store.Publish("S-1-5-21-100", 10, new OperationStatusUpdate
        {
            OperationId = operationId,
            OperationType = DisplayOperationType.StartShortcut,
            Phase = OperationPhase.StartingGame,
            Message = "Starting game."
        }, startedUtc);
        OperationStatus completedStatus = store.Publish("S-1-5-21-100", 10, new OperationStatusUpdate
        {
            OperationId = operationId,
            OperationType = DisplayOperationType.StartShortcut,
            Phase = OperationPhase.Completed,
            Message = "Shortcut completed.",
            IsTerminal = true,
            IsSuccessful = true
        }, startedUtc.AddMinutes(1));

        Assert.Equal(1, firstStatus.Sequence);
        Assert.Equal(2, completedStatus.Sequence);
        Assert.True(completedStatus.IsTerminal);
        Assert.True(completedStatus.IsSuccessful);
        Assert.NotNull(notifiedStatus);
        Assert.Equal(2, notifiedStatus!.Sequence);
        Assert.Equal("Shortcut completed.", notifiedStatus.Message);
    }

    [Fact]
    public void GetAndGetAll_ReturnOnlyTheRequestingUsersOperationsAndReloadPersistedHistory()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        StoragePaths storagePaths = new StoragePaths(storageRoot);
        OperationStatusStore store = new OperationStatusStore(storagePaths);
        Guid firstOperationId = Guid.NewGuid();
        Guid secondOperationId = Guid.NewGuid();

        store.Publish("S-1-5-21-100", 10, new OperationStatusUpdate { OperationId = firstOperationId, OperationType = DisplayOperationType.StartShortcut, Phase = OperationPhase.WaitingForGameToClose, Message = "Waiting." }, DateTime.UtcNow);
        store.Publish("S-1-5-21-200", 20, new OperationStatusUpdate { OperationId = secondOperationId, OperationType = DisplayOperationType.ApplyDisplayProfile, Phase = OperationPhase.ApplyingDisplayProfile, Message = "Applying." }, DateTime.UtcNow);

        OperationStatusStore reloadedStore = new OperationStatusStore(storagePaths);

        Assert.NotNull(reloadedStore.Get("S-1-5-21-100", firstOperationId));
        Assert.Null(reloadedStore.Get("S-1-5-21-200", firstOperationId));
        Assert.Single(reloadedStore.GetAll("S-1-5-21-100"));
        Assert.Single(reloadedStore.GetAll("S-1-5-21-200"));
    }

    [Fact]
    public void GetActive_ReturnsOnlyTheRequestingUsersUnfinishedOperations()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        OperationStatusStore store = new OperationStatusStore(new StoragePaths(storageRoot));
        DateTime now = DateTime.UtcNow;
        Guid activeOperationId = Guid.NewGuid();
        Guid completedOperationId = Guid.NewGuid();

        store.Publish("S-1-5-21-100", 10, new OperationStatusUpdate { OperationId = activeOperationId, OperationType = DisplayOperationType.StartShortcut, Phase = OperationPhase.StartingGame, Message = "Starting game." }, now);
        store.Publish("S-1-5-21-100", 10, new OperationStatusUpdate { OperationId = completedOperationId, OperationType = DisplayOperationType.StartShortcut, Phase = OperationPhase.Completed, Message = "Shortcut completed.", IsTerminal = true, IsSuccessful = true }, now);
        store.Publish("S-1-5-21-200", 20, new OperationStatusUpdate { OperationId = Guid.NewGuid(), OperationType = DisplayOperationType.ApplyDisplayProfile, Phase = OperationPhase.ApplyingDisplayProfile, Message = "Applying." }, now);

        OperationStatus activeStatus = Assert.Single(store.GetActive("S-1-5-21-100"));

        Assert.Equal(activeOperationId, activeStatus.OperationId);
        Assert.Single(store.GetActive("S-1-5-21-200"));
    }
}
