using System;
using System.IO;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class GatewayEventJournalTests
{
    [Fact]
    public void InterruptedSavedEditRetiresItsCursorAfterRestart()
    {
        StoragePaths paths = new StoragePaths(Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N")));
        GatewayEventJournal journal = new GatewayEventJournal(paths);
        string cursor = journal.GetHead("owner", DateTime.UtcNow);
        journal.BeginSavedResourceEdit("owner"); // Simulate process termination before publishing or disposing.

        GatewayEventJournal restarted = new GatewayEventJournal(paths);

        Assert.Equal(GatewayEventReplayState.CursorUnknown, restarted.ReadAfter("owner", cursor, DateTime.UtcNow).State);
        Assert.NotEqual(cursor, restarted.GetHead("owner", DateTime.UtcNow));
    }

    [Fact]
    public void CompletedSavedEditPreservesReplayAcrossRestart()
    {
        StoragePaths paths = new StoragePaths(Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N")));
        GatewayEventJournal journal = new GatewayEventJournal(paths);
        string cursor = journal.GetHead("owner", DateTime.UtcNow);
        using (GatewayEventJournal.SavedResourceEdit edit = journal.BeginSavedResourceEdit("owner"))
        {
            journal.Append("owner", 10, new GatewaySavedResourceChange { ResourceType = "shortcut", Id = "saved", Action = "deleted" }, DateTime.UtcNow);
            edit.Complete();
        }

        GatewayEventReplay replay = new GatewayEventJournal(paths).ReadAfter("owner", cursor, DateTime.UtcNow);

        Assert.Equal(GatewayEventReplayState.Available, replay.State);
        Assert.Equal("saved", Assert.Single(replay.Events).ResourceChange?.Id);
    }

    [Fact]
    public void FailedPublicationRetiresOnlyItsOwnersCursor()
    {
        StoragePaths paths = new StoragePaths(Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N")));
        GatewayEventJournal journal = new GatewayEventJournal(paths);
        string ownerCursor = journal.GetHead("owner", DateTime.UtcNow);
        string otherCursor = journal.GetHead("other", DateTime.UtcNow);
        using (journal.BeginSavedResourceEdit("owner")) { }

        Assert.Equal(GatewayEventReplayState.CursorUnknown, journal.ReadAfter("owner", ownerCursor, DateTime.UtcNow).State);
        Assert.Equal(GatewayEventReplayState.Available, journal.ReadAfter("other", otherCursor, DateTime.UtcNow).State);
    }

    [Fact]
    public void JournalWriteFailureAfterAnEditCannotLeaveAResumableGap()
    {
        StoragePaths paths = new StoragePaths(Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N")));
        GatewayEventJournal journal = new GatewayEventJournal(paths);
        string cursor = journal.GetHead("owner", DateTime.UtcNow);
        string blockedBackup = Path.Combine(paths.MachinePath, "GatewayEventJournal.json.bak");
        using (journal.BeginSavedResourceEdit("owner"))
        {
            Directory.CreateDirectory(blockedBackup);
            Exception? failure = Record.Exception(() => journal.Append("owner", 10, new GatewaySavedResourceChange { ResourceType = "shortcut", Id = "saved", Action = "deleted" }, DateTime.UtcNow));
            Assert.True(failure is IOException or UnauthorizedAccessException);
            Directory.Delete(blockedBackup);
        }

        Assert.Equal(GatewayEventReplayState.CursorUnknown, journal.ReadAfter("owner", cursor, DateTime.UtcNow).State);
        Assert.Equal(GatewayEventReplayState.CursorUnknown, new GatewayEventJournal(paths).ReadAfter("owner", cursor, DateTime.UtcNow).State);
    }

    [Fact]
    public void ReconcileDetectsStateCommittedBeforeTheEventAndPreservesSubsequentRestarts()
    {
        StoragePaths paths = new StoragePaths(Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N")));
        GatewayEventJournal journal = new GatewayEventJournal(paths);
        string cursor = journal.GetHead("owner", DateTime.UtcNow);
        OperationStatus status = new OperationStatus { OwnerUserSid = "owner", OperationId = Guid.NewGuid(), Sequence = 2, UpdatedUtc = DateTime.UtcNow };
        journal.Reconcile(new[] { status }, Array.Empty<OperationDecision>(), DateTime.UtcNow);
        Assert.Equal(GatewayEventReplayState.CursorUnknown, journal.ReadAfter("owner", cursor, DateTime.UtcNow).State);
        string recovered = journal.GetHead("owner", DateTime.UtcNow);

        GatewayEventJournal restarted = new GatewayEventJournal(paths);
        restarted.Reconcile(new[] { status }, Array.Empty<OperationDecision>(), DateTime.UtcNow);

        Assert.Equal(GatewayEventReplayState.Available, restarted.ReadAfter("owner", recovered, DateTime.UtcNow).State);
    }

    [Fact]
    public void SnapshotBoundaryRemainsUsableWhenAllReplayRecordsHaveExpired()
    {
        StoragePaths paths = new StoragePaths(Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N")));
        GatewayEventJournal journal = new GatewayEventJournal(paths);
        DateTime now = DateTime.UtcNow;
        journal.Append(new OperationStatus { OwnerUserSid = "owner", OperationId = Guid.NewGuid() }, now.AddDays(-2));
        string cursor = journal.GetHead("owner", now);

        Assert.Equal(GatewayEventReplayState.Available, journal.ReadAfter("owner", cursor, now).State);
        journal.Append(new OperationStatus { OwnerUserSid = "owner", OperationId = Guid.NewGuid() }, now);
        Assert.Single(journal.ReadAfter("owner", cursor, now).Events);
        Assert.Equal(GatewayEventReplayState.Available, new GatewayEventJournal(paths).ReadAfter("owner", cursor, now).State);
    }

    [Fact]
    public void ReplayPrunesAtThePublishedItemAndByteLimits()
    {
        StoragePaths paths = new StoragePaths(Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N")));
        GatewayEventJournal journal = new GatewayEventJournal(paths);
        DateTime now = DateTime.UtcNow;
        string original = journal.GetHead("owner", now);
        GatewaySavedResourceChange[] changes = new GatewaySavedResourceChange[10001];
        for (int index = 0; index < changes.Length; index++) changes[index] = new GatewaySavedResourceChange { ResourceType = "shortcut", Id = index.ToString(), Action = "deleted" };
        journal.AppendBatch("owner", 10, changes, now);
        Assert.Equal(GatewayEventReplayState.CursorExpired, journal.ReadAfter("owner", original, now).State);

        string beforeOversize = journal.GetHead("owner", now);
        journal.Append("owner", 10, new GatewaySavedResourceChange { ResourceType = "shortcut", Id = "large", Action = "upsert", Href = new string('a', 11 * 1024 * 1024) }, now);

        Assert.Equal(GatewayEventReplayState.CursorExpired, journal.ReadAfter("owner", beforeOversize, now).State);
        Assert.Empty(journal.ReadAfter("owner", journal.GetHead("owner", now), now).Events);
    }

    [Fact]
    public void ReplaySurvivesRestartAndIsScopedToTheOwner()
    {
        StoragePaths paths = new StoragePaths(Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N")));
        GatewayEventJournal journal = new GatewayEventJournal(paths);
        DateTime now = DateTime.UtcNow;
        string firstUserHead = journal.GetHead("S-1-5-21-100", now);
        string secondUserHead = journal.GetHead("S-1-5-21-200", now);
        Guid operationId = Guid.NewGuid();
        journal.Append(new OperationStatus { OwnerUserSid = "S-1-5-21-100", OperationId = operationId }, now);
        journal.Append(new OperationDecision { OwnerUserSid = "S-1-5-21-100", PromptId = Guid.NewGuid(), OperationId = operationId }, now.AddSeconds(1));

        GatewayEventJournal reloaded = new GatewayEventJournal(paths);
        GatewayEventReplay firstUser = reloaded.ReadAfter("S-1-5-21-100", firstUserHead, now.AddSeconds(2));
        GatewayEventReplay secondUser = reloaded.ReadAfter("S-1-5-21-200", secondUserHead, now.AddSeconds(2));

        Assert.Equal(GatewayEventReplayState.Available, firstUser.State);
        Assert.Equal(2, firstUser.Events.Length);
        Assert.Equal(ControlClientEventType.OperationStatusUpdated, firstUser.Events[0].EventType);
        Assert.Equal(ControlClientEventType.OperationDecisionUpdated, firstUser.Events[1].EventType);
        Assert.Equal(operationId, firstUser.Events[0].OperationStatus?.OperationId);
        Assert.Empty(secondUser.Events);
        Assert.Equal(GatewayEventReplayState.CursorUnknown, reloaded.ReadAfter("S-1-5-21-200", firstUser.Events[0].Id, now.AddSeconds(2)).State);
        Assert.Empty(reloaded.ReadAfter("S-1-5-21-100", firstUser.Events[1].Id, now.AddSeconds(2)).Events);
    }

    [Fact]
    public void OldEventsExpireButAnUnknownCursorStaysDistinct()
    {
        StoragePaths paths = new StoragePaths(Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N")));
        GatewayEventJournal journal = new GatewayEventJournal(paths);
        DateTime now = DateTime.UtcNow;
        journal.Append(new OperationStatus { OwnerUserSid = "S-1-5-21-100", OperationId = Guid.NewGuid() }, now);
        string firstId = journal.GetHead("S-1-5-21-100", now);
        journal.Append(new OperationStatus { OwnerUserSid = "S-1-5-21-100", OperationId = Guid.NewGuid() }, now.AddHours(23));

        GatewayEventReplay expired = journal.ReadAfter("S-1-5-21-100", firstId, now.AddHours(25));

        Assert.Equal(GatewayEventReplayState.CursorExpired, expired.State);
        Assert.Empty(expired.Events);
        Assert.Equal(GatewayEventReplayState.CursorUnknown, journal.ReadAfter("S-1-5-21-200", firstId, now.AddHours(25)).State);
        Assert.Equal(GatewayEventReplayState.CursorUnknown, journal.ReadAfter("S-1-5-21-100", "v1.999." + Guid.NewGuid().ToString("N"), now.AddHours(25)).State);
    }

    [Fact]
    public void ReplayPagesAdvanceWithoutSkippingEvents()
    {
        StoragePaths paths = new StoragePaths(Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N")));
        GatewayEventJournal journal = new GatewayEventJournal(paths);
        DateTime now = DateTime.UtcNow;
        string anchor = journal.GetHead("S-1-5-21-100", now);
        for (int index = 0; index < 101; index++)
        {
            journal.Append(new OperationStatus { OwnerUserSid = "S-1-5-21-100", OperationId = Guid.NewGuid() }, now.AddSeconds(index));
        }

        GatewayEventReplay first = journal.ReadAfter("S-1-5-21-100", anchor, now.AddSeconds(102));
        GatewayEventReplay second = journal.ReadAfter("S-1-5-21-100", first.NextCursor, now.AddSeconds(102));

        Assert.Equal(100, first.Events.Length);
        Assert.True(first.HasMore);
        Assert.Single(second.Events);
        Assert.False(second.HasMore);
        Assert.Equal(first.HeadId, second.Events[0].Id);
    }

    [Fact]
    public void SavedResourceBatchKeepsActionsAndLinksAcrossRestart()
    {
        StoragePaths paths = new StoragePaths(Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N")));
        GatewayEventJournal journal = new GatewayEventJournal(paths);
        DateTime now = DateTime.UtcNow;
        string cursor = journal.GetHead("owner", now);
        journal.AppendBatch("owner", 10, new[]
        {
            new GatewaySavedResourceChange { ResourceType = "display-profile", Id = "new", Action = "upsert", Href = "/v1/display-profiles/new" },
            new GatewaySavedResourceChange { ResourceType = "display-profile", Id = "old", Action = "deleted" }
        }, now);

        GatewayEventRecord[] events = new GatewayEventJournal(paths).ReadAfter("owner", cursor, now).Events;

        Assert.Equal(2, events.Length);
        Assert.Equal("upsert", events[0].ResourceChange?.Action);
        Assert.Equal("/v1/display-profiles/new", events[0].ResourceChange?.Href);
        Assert.Equal("deleted", events[1].ResourceChange?.Action);
        Assert.Null(events[1].ResourceChange?.Href);
    }
}
