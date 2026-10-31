using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class ServiceBackgroundReviewTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-BackgroundReview-" + Guid.NewGuid().ToString("N"));
    private StoragePaths Paths => new(_root);
    private string JournalPath => Path.Combine(Paths.MachinePath, "GatewayEventJournal.json");

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{")]
    [InlineData("[null]")]
    [InlineData("[{\"OwnerUserSid\":\"owner\",\"CursorSecret\":\"invalid base64\"}]")]
    public void InvalidJournalPreservesEvidenceAndRejectsReplayAndEdits(string json)
    {
        Paths.EnsureMachineDirectories();
        File.WriteAllText(JournalPath, json);
        File.WriteAllText(JournalPath + ".bak", "preserve backup");
        GatewayEventJournal journal = new(Paths);
        Assert.Throws<IOException>(() => journal.GetHead("owner", DateTime.UtcNow));
        Assert.Throws<IOException>(() => journal.ReadAfter("owner", "cursor", DateTime.UtcNow));
        Assert.Throws<IOException>(() => journal.BeginSavedResourceEdit("owner"));
        Assert.Throws<IOException>(() => journal.Append(new OperationStatus { OwnerUserSid = "owner" }, DateTime.UtcNow));
        Assert.Throws<IOException>(() => journal.MarkContinuityLost("owner"));
        Assert.Equal(json, File.ReadAllText(JournalPath));
        Assert.Equal("preserve backup", File.ReadAllText(JournalPath + ".bak"));
        Assert.False(File.Exists(Path.Combine(Paths.MachinePath, "GatewayEventJournal.pending.json")));
        MachineScheduleCoordinator schedule = new(new MachineScheduleStore(Paths));
        Assert.NotEmpty(schedule.EnsureInitialized().InstallId);
        OperationStatusStore statuses = new(Paths);
        Assert.Empty(statuses.GetReplayState());
    }

    [Fact]
    public void MissingJournalWithBackupCannotStartANewGeneration()
    {
        Paths.EnsureMachineDirectories();
        File.WriteAllText(JournalPath + ".bak", "preserve backup");
        Assert.Throws<IOException>(() => new GatewayEventJournal(Paths).GetHead("owner", DateTime.UtcNow));
        Assert.False(File.Exists(JournalPath));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[null]")]
    [InlineData("{")]
    public void InvalidPendingRecoveryPreservesTheValidJournal(string json)
    {
        GatewayEventJournal journal = new(Paths);
        journal.Append(new OperationStatus { OwnerUserSid = "owner" }, DateTime.UtcNow);
        JsonNode retained = JsonNode.Parse(File.ReadAllText(JournalPath))!;
        retained[0]!["Events"]![0]!["PublishedUtc"] = DateTime.UtcNow.AddHours(-25);
        string valid = retained.ToJsonString();
        File.WriteAllText(JournalPath, valid);
        string pending = Path.Combine(Paths.MachinePath, "GatewayEventJournal.pending.json");
        File.WriteAllText(pending, json);
        GatewayEventJournal restarted = new(Paths);
        Assert.Throws<IOException>(() => restarted.GetHead("owner", DateTime.UtcNow));
        Assert.Throws<IOException>(() => restarted.BeginSavedResourceEdit("owner"));
        Assert.Equal(valid, File.ReadAllText(JournalPath));
        Assert.Equal(json, File.ReadAllText(pending));
    }

    [Fact]
    public void JournalValidationIsAllOrNothingAndRepairRequiresRestart()
    {
        GatewayEventJournal journal = new(Paths);
        string cursor = journal.GetHead("owner", DateTime.UtcNow);
        string valid = File.ReadAllText(JournalPath);
        string invalid = valid[..^1] + ",null]";
        File.WriteAllText(JournalPath, invalid);
        GatewayEventJournal restarted = new(Paths);
        Assert.Throws<IOException>(() => restarted.GetHead("owner", DateTime.UtcNow));
        Assert.Equal(invalid, File.ReadAllText(JournalPath));
        File.WriteAllText(JournalPath, valid);
        Assert.Throws<IOException>(() => restarted.GetHead("owner", DateTime.UtcNow));
        Assert.Equal(cursor, new GatewayEventJournal(Paths).GetHead("owner", DateTime.UtcNow));
    }

    [Theory]
    [InlineData("duplicate-owner")]
    [InlineData("foreign-payload")]
    [InlineData("unsupported-event")]
    [InlineData("invalid-secret")]
    [InlineData("sequence")]
    public void InvalidNestedJournalDataIsPreserved(string mutation)
    {
        GatewayEventJournal journal = new(Paths);
        journal.Append(new OperationStatus { OwnerUserSid = "owner", Sequence = 1 }, DateTime.UtcNow);
        JsonArray document = JsonNode.Parse(File.ReadAllText(JournalPath))!.AsArray();
        JsonObject owner = document[0]!.AsObject();
        JsonObject record = owner["Events"]![0]!.AsObject();
        if (mutation == "duplicate-owner") document.Add(owner.DeepClone());
        else if (mutation == "foreign-payload") record["OperationStatus"]!["OwnerUserSid"] = "other";
        else if (mutation == "unsupported-event") record["EventType"] = int.MaxValue;
        else if (mutation == "invalid-secret") owner["CursorSecret"] = "not base64";
        else owner["NextSequence"] = 1;
        string invalid = document.ToJsonString();
        File.WriteAllText(JournalPath, invalid);
        Assert.Throws<IOException>(() => new GatewayEventJournal(Paths).GetHead("owner", DateTime.UtcNow));
        Assert.Equal(invalid, File.ReadAllText(JournalPath));
    }

    [Fact]
    public async Task BrokenJournalBlocksSavedEditsButStillRoutesLocalShortcutRuns()
    {
        Paths.EnsureMachineDirectories();
        File.WriteAllText(JournalPath, "{");
        ControlStateCoordinator state = new();
        const string user = "S-1-5-21-100";
        state.RegisterAgent(new AgentRegistration { UserSid = user, SessionId = 10, ProcessId = 100, IsReady = true, CommandPipeName = "test-only" }, DateTime.UtcNow);
        RecordingAgent agent = new();
        ProfileOperationRouter router = new(state, agent, new SessionLauncherClient(), null, new MachineDiagnosticLogLevelStore(Paths), new GatewayEventJournal(Paths));
        ControlResponse edit = await router.ManageProfileAsync(user, 10, new ControlEnvelope { MessageType = ControlMessageType.DeleteProfile, Payload = "{\"ProfileId\":\"saved\"}" }, CancellationToken.None);
        Assert.False(edit.IsSuccessful);
        Assert.Equal(0, agent.Calls);
        ControlResponse run = await router.StartShortcutAsync(user, 10, "saved-shortcut", CancellationToken.None);
        Assert.True(run.IsSuccessful, run.Message);
        Assert.Equal(1, agent.Calls);
        Assert.Equal(ControlMessageType.StartShortcut, agent.LastMessage);
        Assert.Equal("{", File.ReadAllText(JournalPath));
    }

    [Fact]
    public void AppendAndReplayUseIndependentNestedSnapshots()
    {
        GatewayEventJournal journal = new(Paths);
        DateTime now = DateTime.UtcNow;
        string cursor = journal.GetHead("owner", now);
        OperationStatus status = new() { OwnerUserSid = "owner", OperationId = Guid.NewGuid(), Message = "original", Sequence = 1 };
        OperationDecision decision = new() { OwnerUserSid = "owner", PromptId = Guid.NewGuid(), Title = "original" };
        GatewaySavedResourceChange resource = new() { ResourceType = "shortcut", Id = "original", Action = "deleted" };
        journal.Append(status, now);
        journal.Append(decision, now);
        journal.Append("owner", 1, resource, now);
        status.Message = "mutated input";
        decision.Title = "mutated input";
        resource.Id = "mutated input";
        GatewayEventReplay first = journal.ReadAfter("owner", cursor, now);
        Assert.Equal("original", first.Events[0].OperationStatus!.Message);
        Assert.Equal("original", first.Events[1].OperationDecision!.Title);
        Assert.Equal("original", first.Events[2].ResourceChange!.Id);
        first.Events[0].OperationStatus!.Message = "mutated output";
        first.Events[1].OperationDecision!.Title = "mutated output";
        first.Events[2].ResourceChange!.Id = "mutated output";
        first.Events[0].OwnerUserSid = "other user";
        GatewayEventReplay second = journal.ReadAfter("owner", cursor, now);
        Assert.Equal("owner", second.Events[0].OwnerUserSid);
        Assert.Equal("original", second.Events[0].OperationStatus!.Message);
        Assert.Equal("original", second.Events[1].OperationDecision!.Title);
        Assert.Equal("original", second.Events[2].ResourceChange!.Id);
        Assert.Equal("original", new GatewayEventJournal(Paths).ReadAfter("owner", cursor, now).Events[0].OperationStatus!.Message);
    }

    [Fact]
    public void FailedNewOwnerSaveCannotExposeAnUncommittedCursor()
    {
        GatewayEventJournal journal = new(Paths);
        journal.GetHead("first", DateTime.UtcNow);
        using (FileStream locked = new(JournalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Assert.Throws<IOException>(() => journal.GetHead("second", DateTime.UtcNow));
            Assert.Throws<IOException>(() => journal.GetHead("second", DateTime.UtcNow));
        }
        string cursor = journal.GetHead("second", DateTime.UtcNow);
        Assert.Equal(cursor, new GatewayEventJournal(Paths).GetHead("second", DateTime.UtcNow));
    }

    [Theory]
    [InlineData("completed")]
    [InlineData("faulted")]
    [InlineData("cancelled")]
    public async Task RequiredLoopExitCancelsAndObservesOtherLoops(string outcome)
    {
        using CancellationTokenSource lifetime = new();
        TaskCompletionSource trigger = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task sibling = Task.Delay(Timeout.Infinite, lifetime.Token);
        Task optional = Task.Delay(Timeout.Infinite, lifetime.Token);
        Task supervised = ControlServiceWorker.SuperviseAsync(new[] { trigger.Task, sibling }, new[] { optional }, lifetime);
        if (outcome == "faulted") trigger.SetException(new IOException("listener failed"));
        else if (outcome == "cancelled") trigger.SetCanceled();
        else trigger.SetResult();
        Exception error = await Assert.ThrowsAnyAsync<Exception>(() => supervised.WaitAsync(TimeSpan.FromSeconds(3)));
        if (outcome == "faulted") Assert.IsType<IOException>(error);
        else Assert.IsType<InvalidOperationException>(error);
        Assert.True(supervised.IsCompleted);
        Assert.True(lifetime.IsCancellationRequested);
        Assert.True(sibling.IsCompleted);
        Assert.True(optional.IsCompleted);
    }

    [Fact]
    public async Task NormalShutdownAndCompletedFirewallRetryAreNotListenerFailures()
    {
        using CancellationTokenSource lifetime = new();
        Task listener = Task.Delay(Timeout.Infinite, lifetime.Token);
        Task supervised = ControlServiceWorker.SuperviseAsync(new[] { listener }, new[] { Task.CompletedTask }, lifetime);
        Assert.False(supervised.IsCompleted);
        lifetime.Cancel();
        await supervised.WaitAsync(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HeartbeatHashCannotOverflowAndUsageCountersSaturate()
    {
        MachineScheduleStore store = new(Paths);
        // This legacy install identifier hashes to Int32.MinValue with the existing stable hash.
        string installId = new(new[] { (char)2, (char)28, (char)8, (char)15, (char)21, (char)7, (char)4 });
        store.Update(state => { state.InstallId = installId; state.TotalAnonymousMetricLaunches = long.MaxValue; state.TotalAnonymousMetricActiveMinutes = long.MaxValue - 1; });
        MachineScheduleCoordinator schedule = new(store);
        DateTime now = DateTime.UtcNow;
        MachineScheduleState heartbeat = schedule.RecordMetricsHeartbeatSuccess(now, "4.0");
        Assert.InRange(heartbeat.NextMetricsHeartbeatUtc!.Value, now.AddDays(7), now.AddDays(14));
        MachineScheduleState usage = schedule.RecordAnonymousMetricsUsage(new AnonymousMetricsUsageReport { AppVersion = "4.0", IsLaunch = true, ActiveMinutes = 3 });
        Assert.Equal(long.MaxValue, usage.TotalAnonymousMetricLaunches);
        Assert.Equal(long.MaxValue, usage.TotalAnonymousMetricActiveMinutes);
    }

    [Fact]
    public async Task MetricsCallerCancellationPropagatesWithoutSchedulingSuccess()
    {
        MachineScheduleCoordinator schedule = new(new MachineScheduleStore(Paths));
        schedule.RecordAnonymousMetricsUsage(new AnonymousMetricsUsageReport { AppVersion = "4.0" });
        using CancellationTokenSource cancellation = new();
        using HttpClient http = new(new DelegateHandler((_, _) =>
        {
            cancellation.Cancel();
            return Task.FromCanceled<HttpResponseMessage>(cancellation.Token);
        }));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new AnonymousMetricsSender(http, schedule).TrySendAsync(cancellation.Token));
        Assert.Null(schedule.GetState().NextMetricsHeartbeatUtc);
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("{\"schemaVersion\":1e100}")]
    public async Task InvalidSyncDocumentRecordsBackoffAndReleasesGate(string json)
    {
        MachineScheduleCoordinator schedule = new(new MachineScheduleStore(Paths));
        using HttpClient http = new(new DelegateHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") })));
        ClientSyncCoordinator sync = new(http, schedule, new ControlStateCoordinator(), new AgentCommandClient());
        await sync.SyncAsync(new ClientSyncRequest { IsManual = true }, null, null, CancellationToken.None);
        await sync.SyncAsync(new ClientSyncRequest { IsManual = true }, null, null, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(3));
        Assert.Equal(2, schedule.GetState().ConsecutiveClientSyncFailures);
    }

    [Fact]
    public async Task ChunkedSyncStopsReadingAtTheConfiguredDocumentLimit()
    {
        CountingStream stream = new();
        using HttpClient http = new(new DelegateHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) })));
        MachineScheduleCoordinator schedule = new(new MachineScheduleStore(Paths));
        ClientSyncCoordinator sync = new(http, schedule, new ControlStateCoordinator(), new AgentCommandClient());
        await sync.SyncAsync(new ClientSyncRequest { IsManual = true }, null, null, CancellationToken.None);
        Assert.Equal(1024 * 1024 + 1, stream.BytesRead);
        Assert.Equal(1, schedule.GetState().ConsecutiveClientSyncFailures);
    }

    [Fact]
    public async Task StalledSyncBodyHonoursTheConfiguredHttpTimeout()
    {
        using HttpClient http = new(new DelegateHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(new StalledStream()) })))
        { Timeout = TimeSpan.FromMilliseconds(100) };
        MachineScheduleCoordinator schedule = new(new MachineScheduleStore(Paths));
        ClientSyncCoordinator sync = new(http, schedule, new ControlStateCoordinator(), new AgentCommandClient());
        await sync.SyncAsync(new ClientSyncRequest { IsManual = true }, null, null, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(3));
        Assert.Equal(1, schedule.GetState().ConsecutiveClientSyncFailures);
    }

    [Fact]
    public async Task SyncCallerCancellationDoesNotRecordFailureOrHoldTheGate()
    {
        using CancellationTokenSource cancellation = new();
        int calls = 0;
        using HttpClient http = new(new DelegateHandler((_, _) =>
        {
            if (++calls == 1)
            {
                cancellation.Cancel();
                return Task.FromCanceled<HttpResponseMessage>(cancellation.Token);
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") });
        }));
        MachineScheduleCoordinator schedule = new(new MachineScheduleStore(Paths));
        ClientSyncCoordinator sync = new(http, schedule, new ControlStateCoordinator(), new AgentCommandClient());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sync.SyncAsync(new ClientSyncRequest { IsManual = true }, null, null, cancellation.Token));
        Assert.Equal(0, schedule.GetState().ConsecutiveClientSyncFailures);
        await sync.SyncAsync(new ClientSyncRequest { IsManual = true }, null, null, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(3));
        Assert.Equal(1, schedule.GetState().ConsecutiveClientSyncFailures);
    }

    private sealed class StalledStream : CountingStream
    {
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return 0;
        }
    }

    private sealed class DelegateHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request, cancellationToken);
    }

    private sealed class RecordingAgent : IAgentCommandClient
    {
        public int Calls { get; private set; }
        public ControlMessageType LastMessage { get; private set; }
        public Task<ControlResponse> SendAsync(AgentRegistration agent, ControlEnvelope request, CancellationToken cancellationToken)
        {
            Calls++;
            LastMessage = request.MessageType;
            return Task.FromResult(new ControlResponse { IsSuccessful = true });
        }
    }

    private class CountingStream : Stream
    {
        public int BytesRead { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = Math.Min(count, 2 * 1024 * 1024 - BytesRead);
            Array.Fill(buffer, (byte)'x', offset, read);
            BytesRead += read;
            return read;
        }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
