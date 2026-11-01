using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class AgentLeaseLifecycleTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-AgentLease-" + Guid.NewGuid().ToString("N"));
    private StoragePaths Paths => new(_root);
    private string LeasePath => Path.Combine(Paths.MachinePath, "DisplayControlLease.json");
    private static AgentRegistration Agent(int processId = 100, int session = 1) => new() { UserSid = "owner", SessionId = session, ProcessId = processId, IsReady = true, CommandPipeName = "test-only" };

    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"OwnerUserSid\":\"owner\",\"OwnerSessionId\":-1}")]
    public void BadLeaseStartsEmptyAndNormalSavesRemainAvailable(string json)
    {
        Paths.EnsureMachineDirectories();
        File.WriteAllText(LeasePath, json);
        File.WriteAllText(LeasePath + ".bak", JsonSerializer.Serialize(new DisplayControlLease { OwnerUserSid = "old-owner", OwnerSessionId = 2, IsRecoveryRequired = true }));
        DisplayControlLeaseStore store = new(Paths);
        Assert.Null(store.Load());
        ControlStateCoordinator coordinator = new(store);
        coordinator.RegisterAgent(Agent(), DateTime.UtcNow);
        Assert.True(coordinator.TryAcquireDisplayControl("owner", 1, DateTime.UtcNow).IsGranted);
        Assert.Equal("owner", store.Load()!.OwnerUserSid);
    }

    [Fact]
    public void MissingPrimaryDoesNotResurrectAnOldBackupAfterRelease()
    {
        Paths.EnsureMachineDirectories();
        File.WriteAllText(LeasePath + ".bak", JsonSerializer.Serialize(new DisplayControlLease { OwnerUserSid = "old-owner", OwnerSessionId = 2, IsRecoveryRequired = true }));
        Assert.Null(new DisplayControlLeaseStore(Paths).Load());
    }

    [Fact]
    public void RestartedActiveLeaseWaitsForReadyOwnerToConfirmRestoration()
    {
        DisplayControlLeaseStore store = new(Paths);
        ControlStateCoordinator original = new(store);
        original.RegisterAgent(Agent(), DateTime.UtcNow);
        original.TryAcquireDisplayControl("owner", 1, DateTime.UtcNow);
        original.TryBeginDisplayOperation("owner", 1, Guid.NewGuid(), DateTime.UtcNow);
        Assert.False(store.Load()!.IsRecoveryRequired);

        ControlStateCoordinator restarted = new(store);
        AgentRegistration replacement = Agent(200);
        replacement.IsReady = false;
        restarted.RegisterAgent(replacement, DateTime.UtcNow);
        Assert.True(restarted.GetDisplayControlLease()!.IsRecoveryRequired);
        Assert.Equal(ControlErrorCode.RecoveryRequired, restarted.TryAcquireDisplayControl("owner", 1, DateTime.UtcNow).ErrorCode);
        replacement.IsReady = true;
        Assert.True(restarted.UpdateAgentRegistration(replacement, DateTime.UtcNow, out _));
        Assert.True(restarted.TryAcquireDisplayControl("owner", 1, DateTime.UtcNow).IsGranted);
        Assert.Null(store.Load()!.ActiveOperationId);
    }

    [Theory]
    [InlineData("acquire")]
    [InlineData("begin")]
    [InlineData("release")]
    public void FailedProspectiveLeaseChangesRetainPriorStateAndCanRetry(string action)
    {
        ControlStateCoordinator coordinator = new(new DisplayControlLeaseStore(Paths));
        coordinator.RegisterAgent(Agent(), DateTime.UtcNow);
        coordinator.RegisterAgent(Agent(200, 2), DateTime.UtcNow);
        coordinator.TryAcquireDisplayControl("owner", 1, DateTime.UtcNow);
        string prior = JsonSerializer.Serialize(coordinator.GetDisplayControlLease());
        Guid operation = Guid.NewGuid();
        Action change = action switch
        {
            "acquire" => () => Assert.True(coordinator.TryAcquireDisplayControl("owner", 2, DateTime.UtcNow).IsGranted),
            "begin" => () => Assert.True(coordinator.TryBeginDisplayOperation("owner", 1, operation, DateTime.UtcNow)),
            _ => () => Assert.NotNull(coordinator.ForceReleaseDisplayControl())
        };
        using (FileStream locked = new(LeasePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            Assert.ThrowsAny<IOException>(change);
        Assert.Equal(prior, JsonSerializer.Serialize(coordinator.GetDisplayControlLease()));
        change();
        if (action == "release") Assert.Null(coordinator.GetDisplayControlLease());
        else if (action == "begin") Assert.Equal(operation, coordinator.GetDisplayControlLease()!.ActiveOperationId);
        else Assert.Equal(2, coordinator.GetDisplayControlLease()!.OwnerSessionId);
    }

    [Fact]
    public void DisconnectRecoveryRemainsObservedWhenItsSaveFails()
    {
        ControlStateCoordinator coordinator = new(new DisplayControlLeaseStore(Paths));
        coordinator.RegisterAgent(Agent(), DateTime.UtcNow);
        coordinator.TryAcquireDisplayControl("owner", 1, DateTime.UtcNow);
        coordinator.TryBeginDisplayOperation("owner", 1, Guid.NewGuid(), DateTime.UtcNow);
        using (FileStream locked = new(LeasePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            Assert.ThrowsAny<IOException>(() => coordinator.UnregisterAgent("owner", 1, 100));
        Assert.Null(coordinator.GetAgentRegistration("owner", 1));
        Assert.True(coordinator.GetDisplayControlLease()!.IsRecoveryRequired);
        coordinator.RegisterAgent(Agent(200), DateTime.UtcNow);
        Assert.True(coordinator.TryAcquireDisplayControl("owner", 1, DateTime.UtcNow).IsGranted);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedReadyRegistrationDoesNotInventAnotherConnection(bool reconnect)
    {
        ControlStateCoordinator coordinator = new(new DisplayControlLeaseStore(Paths));
        AgentRegistration agent = Agent();
        coordinator.RegisterAgent(agent, DateTime.UtcNow);
        coordinator.TryAcquireDisplayControl("owner", 1, DateTime.UtcNow);
        Guid operation = Guid.NewGuid();
        coordinator.TryBeginDisplayOperation("owner", 1, operation, DateTime.UtcNow);
        coordinator.CompleteDisplayOperation("owner", 1, operation, true, DateTime.UtcNow);
        if (!reconnect) coordinator.UnregisterAgent("owner", 1, 100);
        using (FileStream locked = new(LeasePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            Assert.ThrowsAny<IOException>(() => coordinator.RegisterAgent(agent, DateTime.UtcNow));
        if (!reconnect) Assert.Null(coordinator.GetAgentRegistration("owner", 1));
        else
        {
            coordinator.UnregisterAgent("owner", 1, 100);
            Assert.Null(coordinator.GetAgentRegistration("owner", 1));
        }
    }

    [Fact]
    public void ConfirmedStoppedAgentReleasesAllConnectionsAndLateOldMessagesCannotOwnReplacement()
    {
        ControlStateCoordinator coordinator = new();
        coordinator.RegisterAgent(Agent(), DateTime.UtcNow);
        coordinator.RegisterAgent(Agent(), DateTime.UtcNow);
        coordinator.UnregisterStoppedAgent("owner", 1, 100);
        coordinator.RegisterAgent(Agent(200), DateTime.UtcNow);
        coordinator.TryAcquireDisplayControl("owner", 1, DateTime.UtcNow);
        coordinator.UnregisterAgent("owner", 1, 100);
        Assert.False(coordinator.RecordHeartbeat("owner", 1, 100, AgentOperationState.Running, true, DateTime.UtcNow));
        coordinator.RecordOperationStatus(new OperationStatus { OwnerUserSid = "owner", OwnerSessionId = 1, OperationId = Guid.NewGuid() }, DateTime.UtcNow, 100);
        Assert.Equal(200, coordinator.GetAgentRegistration("owner", 1)!.ProcessId);
        Assert.Null(coordinator.GetDisplayControlLease()!.ActiveOperationId);
        Assert.False(coordinator.GetDisplayControlLease()!.IsRecoveryRequired);
        Assert.True(coordinator.RecordHeartbeat("owner", 1, 200, AgentOperationState.Idle, false, DateTime.UtcNow));
    }

    [Fact]
    public async Task NewProcessGetsTheBoundedReadinessWaitBeforeItsFirstRegistration()
    {
        ControlStateCoordinator coordinator = new();
        DelayedLauncher launcher = new();
        RecordingClient client = new();
        ProfileOperationRouter router = new(coordinator, client, launcher);
        Task<ControlResponse> request = router.ListProfilesAsync("owner", 1, CancellationToken.None);
        await launcher.Called.Task.WaitAsync(TimeSpan.FromSeconds(3));
        await Task.Delay(600);
        Assert.False(request.IsCompleted);
        coordinator.RegisterAgent(Agent(), DateTime.UtcNow);
        Assert.True((await request.WaitAsync(TimeSpan.FromSeconds(3))).IsSuccessful);
        Assert.Equal(1, client.Calls);
    }

    private sealed class RecordingClient : IAgentCommandClient
    {
        public int Calls { get; private set; }
        public Task<ControlResponse> SendAsync(AgentRegistration agent, ControlEnvelope request, CancellationToken cancellationToken)
        { Calls++; return Task.FromResult(new ControlResponse { IsSuccessful = true }); }
    }

    private sealed class DelayedLauncher : ISessionLauncherClient
    {
        public TaskCompletionSource Called { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task<UserAgentLaunchResult> LaunchUserAgentAsync(string userSid, int sessionId, Guid requestId, Guid? operationId, string? diagnosticLogLevel, CancellationToken cancellationToken)
        { Called.TrySetResult(); return Task.FromResult(new UserAgentLaunchResult { IsSuccessful = true }); }
        public Task<UserAgentLaunchResult> StopUserAgentAsync(string userSid, int sessionId, int processId, Guid requestId, CancellationToken cancellationToken) => throw new InvalidOperationException("No process should be stopped in this test.");
    }

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
