using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class RoutingReliabilityTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
    private static AgentRegistration Agent(string owner = "owner", int session = 10) => new AgentRegistration { UserSid = owner, SessionId = session, ProcessId = 1000 + session, IsReady = true, CommandPipeName = "unused" };

    [Theory]
    [InlineData("profile")]
    [InlineData("shortcut")]
    [InlineData("restart")]
    public async Task LauncherFailureReturnsAResponseInsteadOfDroppingTheClient(string action)
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        RecordingClient client = new RecordingClient();
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, client, new FailedLauncher());
        ControlResponse result = action switch
        {
            "profile" => await router.ApplyProfileAsync("owner", 10, "display", CancellationToken.None),
            "shortcut" => await router.StartShortcutAsync("owner", 10, "game", CancellationToken.None),
            _ => await router.RestartUserAgentAsync("owner", 10, Guid.NewGuid(), CancellationToken.None)
        };
        Assert.False(result.IsSuccessful);
        Assert.False(client.WasCalled);
        Assert.Null(coordinator.GetDisplayControlLease());
    }

    [Fact]
    public async Task MalformedShortcutReplyRetainsRecoveryAndReportsAnUncertainOutcome()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration owner = Agent();
        coordinator.RegisterAgent(owner, DateTime.UtcNow);
        RecordingClient client = new RecordingClient { Send = _ => throw new InvalidDataException("Invalid reply") };
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, client);
        Guid operation = Guid.NewGuid();
        ControlResponse result = await router.StartShortcutAsync(owner.UserSid, owner.SessionId, "game", operation, Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(ControlErrorCode.OutcomeUnknown, result.ErrorCode);
        Assert.True(coordinator.GetDisplayControlLease()!.IsRecoveryRequired);
        Assert.Equal(operation, coordinator.GetDisplayControlLease()!.ActiveOperationId);
    }

    [Fact]
    public async Task UnconfirmedForcedStopDoesNotUnregisterOrReplaceTheAgent()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration owner = Agent();
        coordinator.RegisterAgent(owner, DateTime.UtcNow);
        FailedLauncher launcher = new FailedLauncher();
        RecoveryAdministrationStore recovery = new RecoveryAdministrationStore(new StoragePaths(_root));
        ProfileOperationRouter router = new ProfileOperationRouter(coordinator, new RecordingClient(), launcher, recovery);
        ControlResponse result = await router.RestartUserAgentAsync(owner.UserSid, owner.SessionId, Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(ControlErrorCode.OutcomeUnknown, result.ErrorCode);
        Assert.Equal(0, launcher.Launches);
        Assert.Equal(1, launcher.Stops);
        Assert.Equal(owner.ProcessId, coordinator.GetAgentRegistration(owner.UserSid, owner.SessionId)!.ProcessId);
        Assert.Equal("Failed", recovery.GetLatest()!.Outcome);
    }

    [Fact]
    public void LateIdleHeartbeatCannotEraseARecoveryRequirement()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration owner = Agent();
        coordinator.RegisterAgent(owner, DateTime.UtcNow);
        coordinator.TryAcquireDisplayControl(owner.UserSid, owner.SessionId, DateTime.UtcNow);
        Guid operation = Guid.NewGuid();
        coordinator.TryBeginDisplayOperation(owner.UserSid, owner.SessionId, operation, DateTime.UtcNow);
        coordinator.RecordOperationStatus(new OperationStatus { OwnerUserSid = owner.UserSid, OwnerSessionId = owner.SessionId,
            OperationId = operation, IsTerminal = true, ErrorCode = ControlErrorCode.RecoveryRequired }, DateTime.UtcNow);
        coordinator.RecordHeartbeat(owner.UserSid, owner.SessionId, AgentOperationState.Idle, false, DateTime.UtcNow);
        Assert.True(coordinator.GetDisplayControlLease()!.IsRecoveryRequired);
        Assert.Equal(operation, coordinator.GetDisplayControlLease()!.ActiveOperationId);
        Assert.Equal(ControlErrorCode.RecoveryRequired, coordinator.TryAcquireDisplayControl(owner.UserSid, owner.SessionId, DateTime.UtcNow).ErrorCode);
    }

    [Fact]
    public void ReconnectingRunningAgentCannotClaimThatRecoveryHasFinished()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration owner = Agent();
        coordinator.RegisterAgent(owner, DateTime.UtcNow);
        coordinator.TryAcquireDisplayControl(owner.UserSid, owner.SessionId, DateTime.UtcNow);
        Guid operation = Guid.NewGuid();
        coordinator.TryBeginDisplayOperation(owner.UserSid, owner.SessionId, operation, DateTime.UtcNow);
        coordinator.CompleteDisplayOperation(owner.UserSid, owner.SessionId, operation, true, DateTime.UtcNow);
        owner.OperationState = AgentOperationState.Running;
        owner.IsRecoveryRequired = false;
        Assert.True(coordinator.UpdateAgentRegistration(owner, DateTime.UtcNow, out _));
        Assert.Equal(operation, coordinator.GetDisplayControlLease()!.ActiveOperationId);
        Assert.True(coordinator.GetDisplayControlLease()!.IsRecoveryRequired);
    }

    [Fact]
    public void ReusedSessionIdDoesNotLetAnotherUsersHeartbeatUpdateTheOldLease()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration owner = Agent();
        DateTime now = DateTime.UtcNow;
        coordinator.RegisterAgent(owner, now);
        coordinator.TryAcquireDisplayControl(owner.UserSid, owner.SessionId, now);
        coordinator.TryBeginDisplayOperation(owner.UserSid, owner.SessionId, Guid.NewGuid(), now);
        coordinator.UnregisterAgent(owner.UserSid, owner.SessionId, owner.ProcessId);
        DateTime previousHeartbeat = coordinator.GetDisplayControlLease()!.LastHeartbeatUtc;
        AgentRegistration other = Agent("other", owner.SessionId);
        coordinator.RegisterAgent(other, now.AddSeconds(1));
        coordinator.RecordHeartbeat(other.UserSid, other.SessionId, AgentOperationState.Idle, false, now.AddSeconds(2));
        Assert.Equal(owner.UserSid, coordinator.GetDisplayControlLease()!.OwnerUserSid);
        Assert.Equal(previousHeartbeat, coordinator.GetDisplayControlLease()!.LastHeartbeatUtc);
        Assert.True(coordinator.GetDisplayControlLease()!.IsRecoveryRequired);
    }

    [Fact]
    public async Task AcceptedDisplayApplyKeepsItsLeaseUntilMatchingCompletion()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration owner = Agent();
        AgentRegistration other = Agent("other", 20);
        coordinator.RegisterAgent(owner, DateTime.UtcNow);
        coordinator.RegisterAgent(other, DateTime.UtcNow);
        Guid operation = Guid.NewGuid();
        RecordingClient client = new RecordingClient { Send = _ => new ControlResponse { IsSuccessful = true,
            OperationStatus = new OperationStatus { OperationId = operation, IsTerminal = false } } };
        ControlResponse result = await new ProfileOperationRouter(coordinator, client).ApplyProfileAsync(owner.UserSid, owner.SessionId, "display", operation, Guid.NewGuid(), CancellationToken.None);
        Assert.True(result.IsSuccessful);
        coordinator.RecordHeartbeat(owner.UserSid, owner.SessionId, AgentOperationState.Idle, false, DateTime.UtcNow);
        Assert.Equal(operation, coordinator.GetDisplayControlLease()!.ActiveOperationId);
        Assert.False(coordinator.TryAcquireDisplayControl(other.UserSid, other.SessionId, DateTime.UtcNow).IsGranted);
        coordinator.RecordOperationStatus(new OperationStatus { OwnerUserSid = owner.UserSid, OwnerSessionId = owner.SessionId, OperationId = operation, IsTerminal = true }, DateTime.UtcNow);
        Assert.True(coordinator.TryAcquireDisplayControl(other.UserSid, other.SessionId, DateTime.UtcNow).IsGranted);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RemoteShortcutUsesTheMachineLeaseAndPreservesCorrelation(bool busy)
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration owner = Agent();
        coordinator.RegisterAgent(owner, DateTime.UtcNow);
        if (busy)
        {
            AgentRegistration other = Agent("other", 20);
            coordinator.RegisterAgent(other, DateTime.UtcNow);
            Assert.True(coordinator.TryAcquireDisplayControl(other.UserSid, other.SessionId, DateTime.UtcNow).IsGranted);
            Assert.True(coordinator.TryBeginDisplayOperation(other.UserSid, other.SessionId, Guid.NewGuid(), DateTime.UtcNow));
        }
        Guid operation = Guid.NewGuid();
        ControlEnvelope request = RemoteRequest(operation);
        RecordingClient client = new RecordingClient { Send = incoming =>
        {
            Assert.Equal(request.RequestId, incoming.RequestId);
            Assert.Equal(operation, coordinator.GetDisplayControlLease()!.ActiveOperationId);
            Assert.Equal(operation, JsonSerializer.Deserialize<StartShortcutRequest>(incoming.Payload)!.OperationId);
            return new ControlResponse { IsSuccessful = true };
        } };
        ControlResponse response = Server(coordinator, client).ExecuteCommand(request, true);
        Assert.Equal(!busy, response.IsSuccessful);
        Assert.Equal(!busy, client.WasCalled);
        if (busy) Assert.Equal(ControlErrorCode.DisplayControlBusy, response.ErrorCode);
    }

    [Theory]
    [InlineData("outer-json", ControlErrorCode.InvalidRequest)]
    [InlineData("inner-json", ControlErrorCode.InvalidRequest)]
    [InlineData("inner-null", ControlErrorCode.InvalidRequest)]
    [InlineData("grants-null", ControlErrorCode.Unauthorized)]
    [InlineData("frame-version", ControlErrorCode.UnsupportedProtocolVersion)]
    [InlineData("caller", ControlErrorCode.Unauthorized)]
    public void InvalidGatewayRequestsReturnStructuredErrorsWithoutDispatch(string invalid, ControlErrorCode expected)
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        coordinator.RegisterAgent(Agent(), DateTime.UtcNow);
        RecordingClient client = new RecordingClient();
        ControlEnvelope request = RemoteRequest(Guid.NewGuid());
        GatewayRemoteCommand command = JsonSerializer.Deserialize<GatewayRemoteCommand>(request.Payload)!;
        switch (invalid)
        {
            case "outer-json": request.Payload = "{"; break;
            case "inner-json": command.Payload = "{"; request.Payload = JsonSerializer.Serialize(command); break;
            case "inner-null": command.Payload = null!; request.Payload = JsonSerializer.Serialize(command); break;
            case "grants-null": command.Authentication.GrantedCapabilities = null!; request.Payload = JsonSerializer.Serialize(command); break;
            case "frame-version": request.ProtocolVersion++; break;
        }
        ControlResponse response = Server(coordinator, client).ExecuteCommand(request, invalid != "caller");
        Assert.False(response.IsSuccessful);
        Assert.Equal(expected, response.ErrorCode);
        Assert.False(client.WasCalled);
    }

    [Fact]
    public void OldOrForeignCompletionCannotReleaseANewerReservation()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration owner = Agent();
        coordinator.RegisterAgent(owner, DateTime.UtcNow);
        coordinator.TryAcquireDisplayControl(owner.UserSid, owner.SessionId, DateTime.UtcNow);
        Guid operation = Guid.NewGuid();
        coordinator.TryBeginDisplayOperation(owner.UserSid, owner.SessionId, operation, DateTime.UtcNow);
        coordinator.RecordOperationStatus(new OperationStatus { OwnerUserSid = owner.UserSid, OwnerSessionId = owner.SessionId, OperationId = Guid.NewGuid(), IsTerminal = true }, DateTime.UtcNow);
        coordinator.RecordOperationStatus(new OperationStatus { OwnerUserSid = "other", OwnerSessionId = owner.SessionId, OperationId = operation, IsTerminal = true }, DateTime.UtcNow);
        Assert.Equal(operation, coordinator.GetDisplayControlLease()!.ActiveOperationId);
    }

    [Fact]
    public void AutomaticGameProgressBindsTheRealOperationIdentity()
    {
        ControlStateCoordinator coordinator = new ControlStateCoordinator();
        AgentRegistration owner = Agent();
        coordinator.RegisterAgent(owner, DateTime.UtcNow);
        coordinator.TryAcquireDisplayControl(owner.UserSid, owner.SessionId, DateTime.UtcNow);
        coordinator.RecordHeartbeat(owner.UserSid, owner.SessionId, AgentOperationState.Running, false, DateTime.UtcNow);
        Guid operation = Guid.NewGuid();
        coordinator.RecordOperationStatus(new OperationStatus { OwnerUserSid = owner.UserSid, OwnerSessionId = owner.SessionId, OperationId = operation }, DateTime.UtcNow);
        Assert.Equal(operation, coordinator.GetDisplayControlLease()!.ActiveOperationId);
        coordinator.RecordHeartbeat(owner.UserSid, owner.SessionId, AgentOperationState.Idle, false, DateTime.UtcNow);
        Assert.Equal(operation, coordinator.GetDisplayControlLease()!.ActiveOperationId);
        coordinator.RecordOperationStatus(new OperationStatus { OwnerUserSid = owner.UserSid, OwnerSessionId = owner.SessionId, OperationId = operation, IsTerminal = true }, DateTime.UtcNow);
        Assert.Null(coordinator.GetDisplayControlLease()!.ActiveOperationId);
    }

    private GatewayPairingPipeServer Server(ControlStateCoordinator coordinator, IAgentCommandClient client)
    {
        StoragePaths paths = new StoragePaths(_root);
        PairedClientRepository clients = new PairedClientRepository(paths);
        return new GatewayPairingPipeServer(new DevicePairingCoordinator(paths, clients), new GatewayIdentityRegistry(), new GatewayRequestAuthenticator(clients),
            new GatewayHttpIdempotencyStore(paths), new OperationStatusStore(paths), new OperationDecisionStore(paths), new ProfileOperationRouter(coordinator, client), coordinator);
    }

    private static ControlEnvelope RemoteRequest(Guid operation) => new ControlEnvelope
    {
        MessageType = ControlMessageType.StartRemoteShortcut,
        Hello = ControlProtocol.CreateHello(ControlClientKind.Gateway, "test-gateway"),
        Payload = JsonSerializer.Serialize(new GatewayRemoteCommand
        {
            Authentication = new GatewayAuthenticationResult { IsAuthenticated = true, OwnerUserSid = "owner", DeviceId = "phone", GrantedCapabilities = new[] { RemoteClientCapabilities.ShortcutsRun } },
            TargetSessionId = 10, Payload = JsonSerializer.Serialize(new StartShortcutRequest { ShortcutId = "game", OperationId = operation })
        })
    };

    private sealed class RecordingClient : IAgentCommandClient
    {
        public bool WasCalled { get; private set; }
        public Func<ControlEnvelope, ControlResponse> Send { get; init; } = _ => new ControlResponse { IsSuccessful = true };
        public Task<ControlResponse> SendAsync(AgentRegistration agent, ControlEnvelope request, CancellationToken cancellationToken) { WasCalled = true; return Task.FromResult(Send(request)); }
    }

    private sealed class FailedLauncher : ISessionLauncherClient
    {
        public int Launches { get; private set; }
        public int Stops { get; private set; }
        public Task<UserAgentLaunchResult> LaunchUserAgentAsync(string userSid, int sessionId, Guid requestId, Guid? operationId, string? diagnosticLogLevel, CancellationToken cancellationToken)
        {
            Launches++;
            return Task.FromException<UserAgentLaunchResult>(new SessionLauncherUnavailableException("Broker unavailable", new InvalidDataException("Bad negotiation")));
        }
        public Task<UserAgentLaunchResult> StopUserAgentAsync(string userSid, int sessionId, int processId, Guid requestId, CancellationToken cancellationToken)
        {
            Stops++;
            return Task.FromException<UserAgentLaunchResult>(new InvalidDataException("Unconfirmed stop"));
        }
    }

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
