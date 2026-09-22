using System;
using System.IO;
using System.Text.Json;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class DiagnosticsAndRecoveryTests
{
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

            store.MarkForceReleased(lease, "S-1-5-21-999", 20);

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
