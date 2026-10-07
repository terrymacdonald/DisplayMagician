using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class DiagnosticsAndRecoveryTests
{
    [Fact]
    public void OperationDecisionStore_UsesFirstValidResponseAndDefaultsToContinueOnExpiry()
    {
        string storageRoot = CreateStorageRoot();
        try
        {
            OperationDecisionStore store = new OperationDecisionStore(new StoragePaths(storageRoot));
            DateTime now = DateTime.UtcNow;
            OperationDecision decision = store.Create("S-1-5-21-100", 10, Guid.NewGuid(), "Display profile failed", "Continue with the current display configuration?", new[] { OperationDecisionChoice.Continue, OperationDecisionChoice.StopAndRestore }, OperationDecisionChoice.Continue, now.AddMinutes(1), now);

            Assert.Equal(decision.PromptId, Assert.Single(store.GetPending("S-1-5-21-100", 10)).PromptId);
            Assert.Equal(decision.PromptId, Assert.Single(store.GetPending("S-1-5-21-100", 11)).PromptId);
            Assert.Empty(store.GetPending("S-1-5-21-999", 10));
            Assert.Null(store.Resolve("S-1-5-21-999", 10, decision.PromptId, OperationDecisionChoice.StopAndRestore, now));
            OperationDecision? resolved = store.Resolve("S-1-5-21-100", 11, decision.PromptId, OperationDecisionChoice.StopAndRestore, now);
            Assert.NotNull(resolved);
            Assert.Equal(OperationDecisionChoice.StopAndRestore, resolved!.ResolvedChoice);
            Assert.Empty(store.GetPending("S-1-5-21-100", 10));
            Assert.Null(store.Resolve("S-1-5-21-100", 10, decision.PromptId, OperationDecisionChoice.Continue, now));

            OperationDecision expiringDecision = store.Create("S-1-5-21-100", 10, Guid.NewGuid(), "Audio profile failed", "Continue with current audio?", new[] { OperationDecisionChoice.Continue, OperationDecisionChoice.StopAndRestore }, OperationDecisionChoice.Continue, now.AddSeconds(1), now);
            OperationDecision expired = Assert.Single(store.Expire(now.AddSeconds(2)));
            Assert.Equal(expiringDecision.PromptId, expired.PromptId);
            Assert.Equal(OperationDecisionChoice.Continue, expired.ResolvedChoice);
        }
        finally
        {
            DeleteStorageRoot(storageRoot);
        }
    }

    [Fact]
    public async Task ControlClientEventHub_PublishesSessionScopedEventsOnlyToTheOwningSession()
    {
        ControlClientEventHub hub = new ControlClientEventHub();
        using ControlClientEventSubscription firstSession = hub.Subscribe("S-1-5-21-100", 10);
        using ControlClientEventSubscription secondSession = hub.Subscribe("S-1-5-21-100", 11);
        using ControlClientEventSubscription otherUser = hub.Subscribe("S-1-5-21-999", 10);
        ControlClientEvent clientEvent = new ControlClientEvent { EventType = ControlClientEventType.OperationStatusUpdated, PublishedUtc = DateTime.UtcNow };

        hub.Publish("S-1-5-21-100", 10, clientEvent);

        Assert.Equal(clientEvent.EventType, (await firstSession.Reader.ReadAsync(CancellationToken.None)).EventType);
        Assert.False(secondSession.Reader.TryRead(out _));
        Assert.False(otherUser.Reader.TryRead(out _));
    }

    [Fact]
    public void Append_WritesDurableStructuredAuditRecord()
    {
        string storageRoot = CreateStorageRoot();
        try
        {
            StoragePaths storagePaths = new StoragePaths(storageRoot);
            AuditStore store = new AuditStore(storagePaths);

            store.Append("DisplayControlForceReleased", "HighSeverity", "An emergency release was requested.", "S-1-5-21-100", 10);

            string auditPath = Path.Combine(storagePaths.MachineDiagnosticsPath, "Audit.jsonl");
            Assert.True(File.Exists(auditPath));
            AuditRecord? record = JsonSerializer.Deserialize<AuditRecord>(File.ReadAllText(auditPath));
            Assert.NotNull(record);
            Assert.Equal("DisplayControlForceReleased", record!.EventType);
            Assert.Equal("HighSeverity", record.Outcome);
            Assert.Equal("S-1-5-21-100", record.UserSid);
            Assert.Equal(10, record.SessionId);
        }
        finally
        {
            DeleteStorageRoot(storageRoot);
        }
    }

    [Fact]
    public void ForceReleaseDisplayControl_ClearsPersistedRecoveryLease()
    {
        string storageRoot = CreateStorageRoot();
        try
        {
            StoragePaths storagePaths = new StoragePaths(storageRoot);
            DisplayControlLeaseStore leaseStore = new DisplayControlLeaseStore(storagePaths);
            ControlStateCoordinator coordinator = new ControlStateCoordinator(leaseStore);
            AgentRegistration agent = new AgentRegistration { UserSid = "S-1-5-21-100", SessionId = 10, ProcessId = 1000 };
            DateTime now = DateTime.UtcNow;
            coordinator.RegisterAgent(agent, now);
            Assert.True(coordinator.TryAcquireDisplayControl(agent.UserSid, agent.SessionId, agent.SessionId, now).IsGranted);
            coordinator.RecordHeartbeat(agent.UserSid, agent.SessionId, AgentOperationState.RecoveryRequired, true, now.AddSeconds(1));

            DisplayControlLease? releasedLease = coordinator.ForceReleaseDisplayControl();

            Assert.NotNull(releasedLease);
            Assert.True(releasedLease!.IsRecoveryRequired);
            Assert.Null(coordinator.GetDisplayControlLease());
            Assert.Null(new ControlStateCoordinator(leaseStore).GetDisplayControlLease());
        }
        finally
        {
            DeleteStorageRoot(storageRoot);
        }
    }

    [Fact]
    public void MarkForceReleased_PersistsRecoveryAbandonmentRecord()
    {
        string storageRoot = CreateStorageRoot();
        try
        {
            StoragePaths storagePaths = new StoragePaths(storageRoot);
            RecoveryAdministrationStore store = new RecoveryAdministrationStore(storagePaths);
            DisplayControlLease lease = new DisplayControlLease { OwnerUserSid = "S-1-5-21-100", OwnerSessionId = 10, IsRecoveryRequired = true };

            Assert.True(store.MarkForceReleased(lease, "S-1-5-21-999", 20));

            RecoveryAdministrationRecord? record = new RecoveryAdministrationStore(storagePaths).GetLatest();
            Assert.NotNull(record);
            Assert.Equal("ForceReleaseDisplayControl", record!.Action);
            Assert.Equal("RecoveryAbandoned", record.Outcome);
            Assert.Equal("S-1-5-21-999", record.AdministratorSid);
            Assert.True(record.ReleasedLease!.IsRecoveryRequired);
        }
        finally
        {
            DeleteStorageRoot(storageRoot);
        }
    }

    [Fact]
    public void RecoveryAdministrationStore_RetainsRecentRecoveryHistory()
    {
        string storageRoot = CreateStorageRoot();
        try
        {
            RecoveryAdministrationStore store = new RecoveryAdministrationStore(new StoragePaths(storageRoot));

            Assert.True(store.Record("RestartUserAgent", "Succeeded", "S-1-5-21-100", 10));
            Assert.True(store.Record("RestartControlService", "Succeeded", "S-1-5-21-999", 20));

            RecoveryAdministrationRecord[] records = new RecoveryAdministrationStore(new StoragePaths(storageRoot)).GetAll();
            Assert.Equal(2, records.Length);
            Assert.Equal("RestartUserAgent", records[0].Action);
            Assert.Equal("RestartControlService", records[1].Action);
        }
        finally
        {
            DeleteStorageRoot(storageRoot);
        }
    }

    private static string CreateStorageRoot()
    {
        return Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
    }

    private static void DeleteStorageRoot(string storageRoot)
    {
        if (Directory.Exists(storageRoot))
        {
            Directory.Delete(storageRoot, recursive: true);
        }
    }
}
