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

    [Fact]
    public void GetChangedSince_ReturnsOnlyTheOwnersChangedOperationsIncludingCursorTimestamp()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            OperationStatusStore store = new OperationStatusStore(new StoragePaths(storageRoot));
            DateTime cursor = new DateTime(2026, 9, 25, 1, 2, 3, DateTimeKind.Utc);
            store.Publish("S-1-5-21-100", 10, new OperationStatusUpdate { OperationId = Guid.NewGuid(), OperationType = DisplayOperationType.StartShortcut, Phase = OperationPhase.StartingGame, Message = "At cursor." }, cursor);
            store.Publish("S-1-5-21-100", 10, new OperationStatusUpdate { OperationId = Guid.NewGuid(), OperationType = DisplayOperationType.StartShortcut, Phase = OperationPhase.WaitingForGameToClose, Message = "After cursor." }, cursor.AddSeconds(1));
            store.Publish("S-1-5-21-200", 20, new OperationStatusUpdate { OperationId = Guid.NewGuid(), OperationType = DisplayOperationType.ApplyDisplayProfile, Phase = OperationPhase.ApplyingDisplayProfile, Message = "Other user." }, cursor.AddSeconds(1));

            OperationStatus[] statuses = store.GetChangedSince("S-1-5-21-100", cursor);

            Assert.Equal(2, statuses.Length);
            Assert.All(statuses, status => Assert.Equal("S-1-5-21-100", status.OwnerUserSid));
            Assert.Contains(statuses, status => status.UpdatedUtc == cursor);
        }
        finally { if (Directory.Exists(storageRoot)) Directory.Delete(storageRoot, true); }
    }

    [Fact]
    public void Publish_DoesNotAllowALateProgressUpdateToReopenATerminalOperation()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        OperationStatusStore store = new OperationStatusStore(new StoragePaths(storageRoot));
        Guid operationId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        OperationStatus completed = store.Publish("S-1-5-21-100", 10, new OperationStatusUpdate
        {
            OperationId = operationId,
            OperationType = DisplayOperationType.StartShortcut,
            Phase = OperationPhase.Completed,
            Message = "Completed.",
            IsTerminal = true,
            IsSuccessful = true
        }, now);
        OperationStatus lateProgress = store.Publish("S-1-5-21-100", 10, new OperationStatusUpdate
        {
            OperationId = operationId,
            OperationType = DisplayOperationType.StartShortcut,
            Phase = OperationPhase.WaitingForGameToClose,
            Message = "Late progress."
        }, now.AddSeconds(1));

        Assert.Equal(completed.Sequence, lateProgress.Sequence);
        Assert.True(lateProgress.IsTerminal);
        Assert.Equal(OperationPhase.Completed, lateProgress.Phase);
    }

    [Fact]
    public void Publish_SameDeliveryId_DoesNotDuplicateAnOperationUpdate()
    {
        OperationStatusStore store = new OperationStatusStore(new StoragePaths(Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"))));
        Guid operationId = Guid.NewGuid();
        Guid updateId = Guid.NewGuid();
        OperationStatusUpdate update = new OperationStatusUpdate { UpdateId = updateId, OperationId = operationId, OperationType = DisplayOperationType.StartShortcut, Phase = OperationPhase.StartingGame, Message = "Starting." };

        OperationStatus first = store.Publish("S-1-5-21-100", 10, update, DateTime.UtcNow);
        OperationStatus replay = store.Publish("S-1-5-21-100", 10, update, DateTime.UtcNow.AddSeconds(1));

        Assert.Equal(1, first.Sequence);
        Assert.Equal(first.Sequence, replay.Sequence);
        Assert.Equal(updateId, replay.LastUpdateId);
    }

    [Fact]
    public void TerminalHistoryIsLimitedPerUserWithoutRemovingActiveOperations()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            StoragePaths paths = new StoragePaths(root);
            OperationStatusStore store = new OperationStatusStore(paths);
            DateTime now = DateTime.UtcNow;
            Guid oldestFirstUser = Guid.Empty;
            for (int index = 0; index < 101; index++)
            {
                Guid operationId = Guid.NewGuid();
                if (index == 0) oldestFirstUser = operationId;
                store.Publish("first-user", 1, new OperationStatusUpdate { OperationId = operationId, Phase = OperationPhase.Completed, IsTerminal = true, IsSuccessful = true },
                    now.AddMinutes(index - 101));
            }
            Guid secondUserOperation = Guid.NewGuid();
            Guid activeOperation = Guid.NewGuid();
            store.Publish("second-user", 2, new OperationStatusUpdate { OperationId = secondUserOperation, Phase = OperationPhase.Completed, IsTerminal = true, IsSuccessful = true }, now);
            store.Publish("first-user", 1, new OperationStatusUpdate { OperationId = activeOperation, Phase = OperationPhase.Requested }, now);

            OperationStatusStore reloaded = new OperationStatusStore(paths);
            Assert.Null(reloaded.Get("first-user", oldestFirstUser));
            Assert.Equal(101, reloaded.GetAll("first-user").Length);
            Assert.NotNull(reloaded.Get("first-user", activeOperation));
            Assert.Equal(secondUserOperation, Assert.Single(reloaded.GetAll("second-user")).OperationId);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void TerminalHistoryExpiresAfterSevenDays()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            StoragePaths paths = new StoragePaths(root);
            OperationStatusStore store = new OperationStatusStore(paths);
            Guid expiredOperation = Guid.NewGuid();
            Guid retainedOperation = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;
            store.Publish("owner", 1, new OperationStatusUpdate { OperationId = expiredOperation, Phase = OperationPhase.Completed, IsTerminal = true }, now.AddDays(-8));
            store.Publish("owner", 1, new OperationStatusUpdate { OperationId = retainedOperation, Phase = OperationPhase.Completed, IsTerminal = true }, now);

            Assert.Null(new OperationStatusStore(paths).Get("owner", expiredOperation));
            Assert.Equal(retainedOperation, Assert.Single(store.GetAll("owner")).OperationId);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
