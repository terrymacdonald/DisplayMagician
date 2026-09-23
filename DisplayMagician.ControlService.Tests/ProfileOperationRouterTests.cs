using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class ProfileOperationRouterTests
{
    [Fact]
    public async Task ListProfilesAsync_ReturnsAgentUnavailableWhenNoAgentIsRegistered()
    {
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
        ProfileOperationRouter router = new ProfileOperationRouter(new ControlStateCoordinator(), commandClient);

        ControlResponse response = await router.ListProfilesAsync("S-1-5-21-100", 10, CancellationToken.None);

        Assert.False(response.IsSuccessful);
        Assert.Equal(ControlErrorCode.AgentUnavailable, response.ErrorCode);
        Assert.False(commandClient.WasCalled);
    }

    [Fact]
    public async Task ListProfilesAsync_ForwardsToRegisteredAgentCommandPipe()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration agent = CreateAgent();
        coordinator.RegisterAgent(agent, DateTime.UtcNow);
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, commandClient);

        ControlResponse response = await router.ListProfilesAsync(agent.UserSid, agent.SessionId, CancellationToken.None);

        Assert.True(response.IsSuccessful);
        Assert.True(commandClient.WasCalled);
        Assert.Equal(ControlMessageType.ListProfiles, commandClient.Request!.MessageType);
        Assert.Equal(agent.CommandPipeName, commandClient.Agent!.CommandPipeName);
    }

    [Fact]
    public async Task ApplyProfileAsync_RejectsMissingProfileIdWithoutCallingAgent()
    {
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
        ProfileOperationRouter router = new ProfileOperationRouter(new ControlStateCoordinator(), commandClient);

        ControlResponse response = await router.ApplyProfileAsync("S-1-5-21-100", 10, " ", CancellationToken.None);

        Assert.False(response.IsSuccessful);
        Assert.Equal(ControlErrorCode.InvalidRequest, response.ErrorCode);
        Assert.False(commandClient.WasCalled);
    }

    [Fact]
    public async Task ApplyProfileAsync_MarksTheDisplayLeaseActiveBeforeForwardingTheCommand()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration agent = CreateAgent();
        coordinator.RegisterAgent(agent, DateTime.UtcNow);
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient
        {
            OnSend = (_, request) =>
            {
                Assert.Equal(ControlMessageType.ApplyProfile, request.MessageType);
                Assert.NotNull(coordinator.GetDisplayControlLease()!.ActiveOperationId);
            }
        };
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, commandClient, new RegisteringSessionLauncherClient(coordinator, agent), () => agent.SessionId);

        ControlResponse response = await router.ApplyProfileAsync(agent.UserSid, agent.SessionId, "display-profile", CancellationToken.None);

        Assert.True(response.IsSuccessful, response.Message);
        Assert.True(commandClient.WasCalled);
        Assert.Null(coordinator.GetDisplayControlLease()!.ActiveOperationId);
    }

    [Fact]
    public async Task ApplyProfileAsync_StartsAndWaitsForAReadyAgentBeforeAcquiringTheDisplayLease()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration agent = CreateAgent();
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
        RegisteringSessionLauncherClient sessionLauncherClient = new RegisteringSessionLauncherClient(coordinator, agent);
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, commandClient, sessionLauncherClient, () => agent.SessionId);

        ControlResponse response = await router.ApplyProfileAsync(agent.UserSid, agent.SessionId, "display-profile", CancellationToken.None);

        Assert.True(response.IsSuccessful, response.Message);
        Assert.True(sessionLauncherClient.WasCalled);
        Assert.True(commandClient.WasCalled);
    }

    [Fact]
    public async Task ApplyProfileAsync_PreservesTheClientRequestIdAcrossTheAgentLaunchAndCommand()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration agent = CreateAgent();
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
        RegisteringSessionLauncherClient sessionLauncherClient = new RegisteringSessionLauncherClient(coordinator, agent);
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, commandClient, sessionLauncherClient, () => agent.SessionId);
        Guid requestId = Guid.NewGuid();

        ControlResponse response = await router.ApplyProfileAsync(agent.UserSid, agent.SessionId, "display-profile", requestId, CancellationToken.None);

        Assert.True(response.IsSuccessful, response.Message);
        Assert.Equal(requestId, sessionLauncherClient.RequestId);
        Assert.Equal(requestId, commandClient.Request!.RequestId);
    }

    [Fact]
    public async Task StartShortcutAsync_ForwardsShortcutIdToRegisteredAgent()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration agent = CreateAgent();
        coordinator.RegisterAgent(agent, DateTime.UtcNow);
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, commandClient, new RegisteringSessionLauncherClient(coordinator, agent), () => agent.SessionId);
        Guid operationId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        ControlResponse response = await router.StartShortcutAsync(agent.UserSid, agent.SessionId, "shortcut-123", operationId, requestId, CancellationToken.None);

        Assert.True(response.IsSuccessful);
        Assert.True(commandClient.WasCalled);
        Assert.Equal(ControlMessageType.StartShortcut, commandClient.Request!.MessageType);
        StartShortcutRequest? request = System.Text.Json.JsonSerializer.Deserialize<StartShortcutRequest>(commandClient.Request.Payload);
        Assert.NotNull(request);
        Assert.Equal("shortcut-123", request.ShortcutId);
        Assert.Equal(operationId, request.OperationId);
        Assert.Equal(requestId, commandClient.Request.RequestId);
        Assert.Equal(operationId, coordinator.GetDisplayControlLease()!.ActiveOperationId);
    }

    [Fact]
    public async Task StartShortcutAsync_RejectsMissingShortcutIdWithoutCallingAgent()
    {
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
        ProfileOperationRouter router = new ProfileOperationRouter(new ControlStateCoordinator(), commandClient);

        ControlResponse response = await router.StartShortcutAsync("S-1-5-21-100", 10, " ", CancellationToken.None);

        Assert.False(response.IsSuccessful);
        Assert.Equal(ControlErrorCode.InvalidRequest, response.ErrorCode);
        Assert.False(commandClient.WasCalled);
    }

    [Fact]
    public async Task StartShortcutAsync_PreservesCorrelationWhenSessionLauncherStartsTheAgent()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration agent = CreateAgent();
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
        RegisteringSessionLauncherClient sessionLauncherClient = new RegisteringSessionLauncherClient(coordinator, agent);
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, commandClient, sessionLauncherClient, () => agent.SessionId);
        Guid operationId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        ControlResponse response = await router.StartShortcutAsync(agent.UserSid, agent.SessionId, "shortcut-123", operationId, requestId, CancellationToken.None);

        Assert.True(sessionLauncherClient.WasCalled);
        Assert.True(response.IsSuccessful, response.Message);
        Assert.Equal(requestId, sessionLauncherClient.RequestId);
        Assert.Equal(operationId, sessionLauncherClient.OperationId);
        Assert.Equal(requestId, commandClient.Request!.RequestId);
        StartShortcutRequest? request = System.Text.Json.JsonSerializer.Deserialize<StartShortcutRequest>(commandClient.Request.Payload);
        Assert.NotNull(request);
        Assert.Equal(operationId, request.OperationId);
    }

    [Fact]
    public async Task CancelOperationAsync_ForwardsOperationIdToTheRegisteredAgent()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration agent = CreateAgent();
        coordinator.RegisterAgent(agent, DateTime.UtcNow);
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, commandClient);
        Guid operationId = Guid.NewGuid();

        ControlResponse response = await router.CancelOperationAsync(agent.UserSid, agent.SessionId, operationId, CancellationToken.None);

        Assert.True(response.IsSuccessful);
        Assert.True(commandClient.WasCalled);
        Assert.Equal(ControlMessageType.CancelOperation, commandClient.Request!.MessageType);
        CancelOperationRequest? request = System.Text.Json.JsonSerializer.Deserialize<CancelOperationRequest>(commandClient.Request.Payload);
        Assert.NotNull(request);
        Assert.Equal(operationId, request.OperationId);
    }

    [Fact]
    public async Task CancelOperationAsync_RejectsAnEmptyOperationIdWithoutCallingAgent()
    {
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
        ProfileOperationRouter router = new ProfileOperationRouter(new ControlStateCoordinator(), commandClient);

        ControlResponse response = await router.CancelOperationAsync("S-1-5-21-100", 10, Guid.Empty, CancellationToken.None);

        Assert.False(response.IsSuccessful);
        Assert.Equal(ControlErrorCode.InvalidRequest, response.ErrorCode);
        Assert.False(commandClient.WasCalled);
    }

    [Fact]
    public async Task ManageProfileAsync_ForwardsProfileMutationToRegisteredAgent()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration agent = CreateAgent();
        coordinator.RegisterAgent(agent, DateTime.UtcNow);
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, commandClient);

        ControlResponse response = await router.ManageProfileAsync(agent.UserSid, agent.SessionId, new ControlEnvelope { MessageType = ControlMessageType.DeleteProfile, Payload = "{}" }, CancellationToken.None);

        Assert.True(response.IsSuccessful);
        Assert.True(commandClient.WasCalled);
        Assert.Equal(ControlMessageType.DeleteProfile, commandClient.Request!.MessageType);
    }

    [Fact]
    public async Task ListProfilesAsync_StartsMissingAgentAndForwardsAfterItRegisters()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration agent = CreateAgent();
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
        RegisteringSessionLauncherClient sessionLauncherClient = new RegisteringSessionLauncherClient(coordinator, agent);
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, commandClient, sessionLauncherClient);

        ControlResponse response = await router.ListProfilesAsync(agent.UserSid, agent.SessionId, CancellationToken.None);

        Assert.True(response.IsSuccessful);
        Assert.True(sessionLauncherClient.WasCalled);
        Assert.True(commandClient.WasCalled);
        Assert.Equal(commandClient.Request!.RequestId, sessionLauncherClient.RequestId);
    }

    [Fact]
    public async Task RestartUserAgentAsync_StartsAnAgentWhenNoneIsConnected()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration agent = CreateAgent();
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
        RegisteringSessionLauncherClient sessionLauncherClient = new RegisteringSessionLauncherClient(coordinator, agent);
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        RecoveryAdministrationStore recoveryAdministrationStore = new RecoveryAdministrationStore(new StoragePaths(storageRoot));
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, commandClient, sessionLauncherClient, recoveryAdministrationStore);

        try
        {
            ControlResponse response = await router.RestartUserAgentAsync(agent.UserSid, agent.SessionId, Guid.NewGuid(), CancellationToken.None);

            Assert.True(response.IsSuccessful, response.Message);
            Assert.True(sessionLauncherClient.WasCalled);
            Assert.False(commandClient.WasCalled);
            Assert.NotNull(coordinator.GetAgentRegistration(agent.UserSid, agent.SessionId));
            Assert.Equal("RestartUserAgent", recoveryAdministrationStore.GetLatest()!.Action);
        }
        finally
        {
            if (Directory.Exists(storageRoot)) Directory.Delete(storageRoot, recursive: true);
        }
    }

    [Fact]
    public async Task RestartUserAgentAsync_RecordsFailureWhenAgentRefusesToStop()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            ControlStateCoordinator coordinator = new ControlStateCoordinator();
            AgentRegistration agent = CreateAgent();
            coordinator.RegisterAgent(agent, DateTime.UtcNow);
            RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient { Response = new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.DisplayControlBusy, Message = "Agent has active work." } };
            RecoveryAdministrationStore recoveryAdministrationStore = new RecoveryAdministrationStore(new StoragePaths(storageRoot));
            ProfileOperationRouter router = new ProfileOperationRouter(coordinator, commandClient, new RegisteringSessionLauncherClient(coordinator, agent), recoveryAdministrationStore);

            ControlResponse response = await router.RestartUserAgentAsync(agent.UserSid, agent.SessionId, Guid.NewGuid(), CancellationToken.None);

            Assert.False(response.IsSuccessful);
            RecoveryAdministrationRecord record = Assert.IsType<RecoveryAdministrationRecord>(recoveryAdministrationStore.GetLatest());
            Assert.Equal("RestartUserAgent", record.Action);
            Assert.Equal("Failed", record.Outcome);
        }
        finally
        {
            if (Directory.Exists(storageRoot)) Directory.Delete(storageRoot, recursive: true);
        }
    }

    [Fact]
    public async Task RestartUserAgentAsync_StopsAnIdleConnectedAgentBeforeStartingItsReplacement()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration oldAgent = CreateAgent();
        AgentRegistration replacementAgent = CreateAgent();
        replacementAgent.ProcessId = 2000;
        coordinator.RegisterAgent(oldAgent, DateTime.UtcNow);
        RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient
        {
            OnSend = (agent, request) =>
            {
                if (request.MessageType == ControlMessageType.StopAgentIfIdle)
                {
                    coordinator.UnregisterAgent(agent.UserSid, agent.SessionId, agent.ProcessId);
                }
            }
        };
        RegisteringSessionLauncherClient sessionLauncherClient = new RegisteringSessionLauncherClient(coordinator, replacementAgent);
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, commandClient, sessionLauncherClient);

        ControlResponse response = await router.RestartUserAgentAsync(oldAgent.UserSid, oldAgent.SessionId, Guid.NewGuid(), CancellationToken.None);

        Assert.True(response.IsSuccessful, response.Message);
        Assert.True(commandClient.WasCalled);
        Assert.Equal(ControlMessageType.StopAgentIfIdle, commandClient.Request!.MessageType);
        Assert.True(sessionLauncherClient.WasCalled);
        Assert.Equal(replacementAgent.ProcessId, coordinator.GetAgentRegistration(oldAgent.UserSid, oldAgent.SessionId)!.ProcessId);
    }

    private static AgentRegistration CreateAgent()
    {
        return new AgentRegistration { UserSid = "S-1-5-21-100", SessionId = 10, ProcessId = 1000, IsReady = true, CommandPipeName = "test-agent-command" };
    }

    private sealed class RecordingAgentCommandClient : IAgentCommandClient
    {
        public bool WasCalled { get; private set; }

        public AgentRegistration? Agent { get; private set; }

        public ControlEnvelope? Request { get; private set; }

        public Action<AgentRegistration, ControlEnvelope>? OnSend { get; init; }

        public ControlResponse Response { get; init; } = new ControlResponse { IsSuccessful = true, Message = "Test command response." };

        public Task<ControlResponse> SendAsync(AgentRegistration agent, ControlEnvelope request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            Agent = agent;
            Request = request;
            OnSend?.Invoke(agent, request);
            return Task.FromResult(Response);
        }
    }

    private sealed class RegisteringSessionLauncherClient : ISessionLauncherClient
    {
        private readonly ControlStateCoordinator _coordinator;
        private readonly AgentRegistration _agent;

        public RegisteringSessionLauncherClient(ControlStateCoordinator coordinator, AgentRegistration agent)
        {
            _coordinator = coordinator;
            _agent = agent;
        }

        public bool WasCalled { get; private set; }

        public Guid RequestId { get; private set; }

        public Guid? OperationId { get; private set; }

        public Task<UserAgentLaunchResult> LaunchUserAgentAsync(string userSid, int sessionId, Guid requestId, Guid? operationId, CancellationToken cancellationToken)
        {
            WasCalled = true;
            RequestId = requestId;
            OperationId = operationId;
            _coordinator.RegisterAgent(_agent, DateTime.UtcNow);
            return Task.FromResult(new UserAgentLaunchResult { IsSuccessful = true, Message = "Test Agent launched." });
        }

        public Task<UserAgentLaunchResult> StopUserAgentAsync(string userSid, int sessionId, int processId, Guid requestId, CancellationToken cancellationToken)
        {
            return Task.FromResult(new UserAgentLaunchResult { IsSuccessful = true, Message = "Test Agent stopped." });
        }
    }
}
