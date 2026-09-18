using System;
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
    }

    private static AgentRegistration CreateAgent()
    {
        return new AgentRegistration { UserSid = "S-1-5-21-100", SessionId = 10, ProcessId = 1000, CommandPipeName = "test-agent-command" };
    }

    private sealed class RecordingAgentCommandClient : IAgentCommandClient
    {
        public bool WasCalled { get; private set; }

        public AgentRegistration? Agent { get; private set; }

        public ControlEnvelope? Request { get; private set; }

        public Task<ControlResponse> SendAsync(AgentRegistration agent, ControlEnvelope request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            Agent = agent;
            Request = request;
            return Task.FromResult(new ControlResponse { IsSuccessful = true, Message = "Test command response." });
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

        public Task<UserAgentLaunchResult> LaunchUserAgentAsync(string userSid, int sessionId, CancellationToken cancellationToken)
        {
            WasCalled = true;
            _coordinator.RegisterAgent(_agent, DateTime.UtcNow);
            return Task.FromResult(new UserAgentLaunchResult { IsSuccessful = true, Message = "Test Agent launched." });
        }
    }
}
