using System;
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
