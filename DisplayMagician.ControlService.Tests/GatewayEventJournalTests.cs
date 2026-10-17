using System;
using System.IO;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class GatewayEventJournalTests
{
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
}
