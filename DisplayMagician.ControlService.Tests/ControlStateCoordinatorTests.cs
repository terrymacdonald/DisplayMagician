using System;
using System.IO;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class ControlStateCoordinatorTests
{
    [Fact]
    public void TryAcquireDisplayControl_ReleasesIdleLeaseWhenConsoleUserChanges()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        DateTime now = DateTime.UtcNow;
        AgentRegistration firstUser = CreateAgent("S-1-5-21-100", 10, 1000);
        AgentRegistration secondUser = CreateAgent("S-1-5-21-200", 11, 2000);
        coordinator.RegisterAgent(firstUser, now);
        Assert.True(coordinator.TryAcquireDisplayControl(firstUser.UserSid, firstUser.SessionId, firstUser.SessionId, now).IsGranted);

        coordinator.RegisterAgent(secondUser, now.AddSeconds(1));
        LeaseDecision decision = coordinator.TryAcquireDisplayControl(secondUser.UserSid, secondUser.SessionId, secondUser.SessionId, now.AddSeconds(1));

        Assert.True(decision.IsGranted, decision.Message);
        Assert.Equal(secondUser.SessionId, decision.Lease?.OwnerSessionId);
    }

    [Fact]
    public void TryAcquireDisplayControl_ReleasesIdleLeaseWhenOwnerHeartbeatIsStale()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        DateTime now = DateTime.UtcNow;
        AgentRegistration firstUser = CreateAgent("S-1-5-21-100", 10, 1000);
        AgentRegistration secondUser = CreateAgent("S-1-5-21-200", 11, 2000);
        coordinator.RegisterAgent(firstUser, now);
        Assert.True(coordinator.TryAcquireDisplayControl(firstUser.UserSid, firstUser.SessionId, firstUser.SessionId, now).IsGranted);

        DateTime later = now.AddSeconds(46);
        coordinator.RegisterAgent(secondUser, later);
        LeaseDecision decision = coordinator.TryAcquireDisplayControl(secondUser.UserSid, secondUser.SessionId, secondUser.SessionId, later);

        Assert.True(decision.IsGranted, decision.Message);
        Assert.Equal(secondUser.SessionId, decision.Lease?.OwnerSessionId);
    }

    [Fact]
    public void TryAcquireDisplayControl_DoesNotReleaseLeaseThatRequiresRecovery()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        DateTime now = DateTime.UtcNow;
        AgentRegistration firstUser = CreateAgent("S-1-5-21-100", 10, 1000);
        AgentRegistration secondUser = CreateAgent("S-1-5-21-200", 11, 2000);
        coordinator.RegisterAgent(firstUser, now);
        Assert.True(coordinator.TryAcquireDisplayControl(firstUser.UserSid, firstUser.SessionId, firstUser.SessionId, now).IsGranted);
        coordinator.RecordHeartbeat(firstUser.UserSid, firstUser.SessionId, AgentOperationState.RecoveryRequired, true, now.AddSeconds(1));

        coordinator.RegisterAgent(secondUser, now.AddSeconds(2));
        LeaseDecision decision = coordinator.TryAcquireDisplayControl(secondUser.UserSid, secondUser.SessionId, secondUser.SessionId, now.AddSeconds(2));

        Assert.False(decision.IsGranted);
        Assert.Equal(ControlErrorCode.DisplayControlBusy, decision.ErrorCode);
    }

    [Fact]
    public void UnregisterAgent_KeepsLeaseWhenAnOperationWasRunning()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        DateTime now = DateTime.UtcNow;
        AgentRegistration agent = CreateAgent("S-1-5-21-100", 10, 1000);
        coordinator.RegisterAgent(agent, now);
        Assert.True(coordinator.TryAcquireDisplayControl(agent.UserSid, agent.SessionId, agent.SessionId, now).IsGranted);
        coordinator.RecordHeartbeat(agent.UserSid, agent.SessionId, AgentOperationState.Running, false, now.AddSeconds(1));

        coordinator.UnregisterAgent(agent.UserSid, agent.SessionId, agent.ProcessId);

        DisplayControlLease? lease = coordinator.GetDisplayControlLease();
        Assert.NotNull(lease);
        Assert.True(lease!.IsRecoveryRequired);
        Assert.NotNull(lease.ActiveOperationId);
    }

    [Fact]
    public void TryBeginDisplayOperation_MarksTheLeaseActiveUntilTheAgentResponds()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        DateTime now = DateTime.UtcNow;
        AgentRegistration agent = CreateAgent("S-1-5-21-100", 10, 1000);
        coordinator.RegisterAgent(agent, now);
        Assert.True(coordinator.TryAcquireDisplayControl(agent.UserSid, agent.SessionId, agent.SessionId, now).IsGranted);
        Guid operationId = Guid.NewGuid();

        Assert.True(coordinator.TryBeginDisplayOperation(agent.UserSid, agent.SessionId, operationId, now.AddSeconds(1)));
        Assert.Equal(operationId, coordinator.GetDisplayControlLease()!.ActiveOperationId);

        coordinator.CompleteDisplayOperation(agent.UserSid, agent.SessionId, operationId, false, now.AddSeconds(2));

        Assert.Null(coordinator.GetDisplayControlLease()!.ActiveOperationId);
        Assert.False(coordinator.GetDisplayControlLease()!.IsRecoveryRequired);
    }

    [Fact]
    public void CompleteDisplayOperation_PreservesTheLeaseForRecoveryWhenTheAgentCannotConfirmCompletion()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        DateTime now = DateTime.UtcNow;
        AgentRegistration agent = CreateAgent("S-1-5-21-100", 10, 1000);
        coordinator.RegisterAgent(agent, now);
        Assert.True(coordinator.TryAcquireDisplayControl(agent.UserSid, agent.SessionId, agent.SessionId, now).IsGranted);
        Guid operationId = Guid.NewGuid();
        Assert.True(coordinator.TryBeginDisplayOperation(agent.UserSid, agent.SessionId, operationId, now.AddSeconds(1)));

        coordinator.CompleteDisplayOperation(agent.UserSid, agent.SessionId, operationId, true, now.AddSeconds(2));

        DisplayControlLease lease = coordinator.GetDisplayControlLease()!;
        Assert.True(lease.IsRecoveryRequired);
        Assert.Equal(operationId, lease.ActiveOperationId);
    }

    [Fact]
    public void UnregisterAgent_KeepsPersistentRegistrationWhenAnOperationConnectionCloses()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        DateTime now = DateTime.UtcNow;
        AgentRegistration agent = CreateAgent("S-1-5-21-100", 10, 1000);
        coordinator.RegisterAgent(agent, now);
        coordinator.RegisterAgent(agent, now.AddSeconds(1));

        coordinator.UnregisterAgent(agent.UserSid, agent.SessionId, agent.ProcessId);

        ControlServiceStatus status = coordinator.GetStatus(now.AddSeconds(2));
        Assert.Single(status.Agents);
        Assert.Equal(agent.ProcessId, status.Agents[0].ProcessId);
    }

    [Fact]
    public void TryRegisterAgentConnection_RejectsDifferentProcessForSameSession()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        DateTime now = DateTime.UtcNow;
        AgentRegistration firstAgent = CreateAgent("S-1-5-21-100", 10, 1000);
        AgentRegistration secondAgent = CreateAgent("S-1-5-21-100", 10, 2000);

        Assert.True(coordinator.TryRegisterAgentConnection(firstAgent, now, out _));
        Assert.False(coordinator.TryRegisterAgentConnection(secondAgent, now.AddSeconds(1), out string message));
        Assert.Contains("already connected", message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(firstAgent.ProcessId, coordinator.GetAgentRegistration(firstAgent.UserSid, firstAgent.SessionId)!.ProcessId);
    }

    [Fact]
    public void UpdateAgentRegistration_DoesNotAddAnotherConnection()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        DateTime now = DateTime.UtcNow;
        AgentRegistration agent = CreateAgent("S-1-5-21-100", 10, 1000);
        Assert.True(coordinator.TryRegisterAgentConnection(agent, now, out _));

        agent.IsReady = true;
        Assert.True(coordinator.UpdateAgentRegistration(agent, now.AddSeconds(1), out _));
        coordinator.UnregisterAgent(agent.UserSid, agent.SessionId, agent.ProcessId);

        Assert.Null(coordinator.GetAgentRegistration(agent.UserSid, agent.SessionId));
    }

    [Fact]
    public void RecoveryRequiredLease_SurvivesServiceRestartUntilOwnerAgentConfirmsRestoration()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ControlState-{Guid.NewGuid():N}");
        try
        {
            DateTime now = DateTime.UtcNow;
            StoragePaths storagePaths = new StoragePaths(root);
            DisplayControlLeaseStore leaseStore = new DisplayControlLeaseStore(storagePaths);
            ControlStateCoordinator coordinator = new ControlStateCoordinator(leaseStore);
            AgentRegistration owner = CreateAgent("S-1-5-21-100", 10, 1000);
            coordinator.RegisterAgent(owner, now);
            Assert.True(coordinator.TryAcquireDisplayControl(owner.UserSid, owner.SessionId, owner.SessionId, now).IsGranted);
            coordinator.RecordHeartbeat(owner.UserSid, owner.SessionId, AgentOperationState.RecoveryRequired, true, now.AddSeconds(1));

            ControlStateCoordinator restartedCoordinator = new ControlStateCoordinator(leaseStore);
            AgentRegistration secondUser = CreateAgent("S-1-5-21-200", 11, 2000);
            restartedCoordinator.RegisterAgent(secondUser, now.AddSeconds(2));
            LeaseDecision denied = restartedCoordinator.TryAcquireDisplayControl(secondUser.UserSid, secondUser.SessionId, secondUser.SessionId, now.AddSeconds(2));
            Assert.False(denied.IsGranted);
            Assert.Equal(ControlErrorCode.DisplayControlBusy, denied.ErrorCode);

            AgentRegistration recoveredOwner = CreateAgent(owner.UserSid, owner.SessionId, owner.ProcessId);
            restartedCoordinator.RegisterAgent(recoveredOwner, now.AddSeconds(3));
            LeaseDecision granted = restartedCoordinator.TryAcquireDisplayControl(recoveredOwner.UserSid, recoveredOwner.SessionId, recoveredOwner.SessionId, now.AddSeconds(3));
            Assert.True(granted.IsGranted, granted.Message);
            Assert.False(granted.Lease!.IsRecoveryRequired);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    private static AgentRegistration CreateAgent(string userSid, int sessionId, int processId)
    {
        return new AgentRegistration
        {
            UserSid = userSid,
            SessionId = sessionId,
            ProcessId = processId,
            OperationState = AgentOperationState.Idle
        };
    }
}
