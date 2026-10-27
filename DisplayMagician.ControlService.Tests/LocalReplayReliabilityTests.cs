using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class LocalReplayReliabilityTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
    private StoragePaths Paths => new StoragePaths(_root);
    private string Ledger => Path.Combine(Paths.MachinePath, "ControlRequestReplay.json");
    private static ControlEnvelope Mutation() => new ControlEnvelope { MessageType = ControlMessageType.StartShortcut, Payload = "{\"ShortcutId\":\"game\"}" };

    [Theory]
    [InlineData("not-json")]
    [InlineData("null")]
    [InlineData("[null]")]
    [InlineData("[{}]")]
    public async Task CorruptLedgerBlocksMutationsButKeepsReadsAndFiles(string contents)
    {
        Directory.CreateDirectory(Paths.MachinePath);
        File.WriteAllText(Ledger, contents);
        File.WriteAllText(Ledger + ".bak", "[]");
        ControlRequestReplayStore store = new ControlRequestReplayStore(Paths);
        Assert.False(store.IsAvailable);
        int calls = 0;
        Task<ControlResponse> Execute() { calls++; return Task.FromResult(new ControlResponse { IsSuccessful = true }); }
        Assert.False((await store.ExecuteAsync("owner", Mutation(), Execute, CancellationToken.None)).IsSuccessful);
        Assert.False((await store.ExecuteAsync("owner", new ControlEnvelope { MessageType = ControlMessageType.UpdateGatewaySettings }, Execute, CancellationToken.None)).IsSuccessful);
        Assert.True((await store.ExecuteAsync("owner", new ControlEnvelope { MessageType = ControlMessageType.ListProfiles }, Execute, CancellationToken.None)).IsSuccessful);
        Assert.Equal(1, calls);
        Assert.Equal(contents, File.ReadAllText(Ledger));
        Assert.Equal("[]", File.ReadAllText(Ledger + ".bak"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MissingPrimaryCannotDiscardExistingHistory(bool backupOnly)
    {
        Directory.CreateDirectory(Paths.MachinePath);
        if (backupOnly) File.WriteAllText(Ledger + ".bak", "[]");
        else Directory.CreateDirectory(Ledger);
        Assert.False(new ControlRequestReplayStore(Paths).IsAvailable);
    }

    [Theory]
    [InlineData("future-version")]
    [InlineData("null-response")]
    [InlineData("invalid-response")]
    [InlineData("duplicate")]
    public void InvalidNestedRecordsCannotBeTreatedAsEmptyHistory(string invalid)
    {
        Directory.CreateDirectory(Paths.MachinePath);
        JsonObject record = JsonNode.Parse(JsonSerializer.Serialize(LegacyRecord(Guid.NewGuid(), "{}")))!.AsObject();
        if (invalid == "future-version") record["SchemaVersion"] = 3;
        if (invalid == "null-response") record["ResponsePayload"] = null;
        if (invalid == "invalid-response") record["ResponsePayload"] = "{";
        JsonArray records = new JsonArray(record);
        if (invalid == "duplicate")
        {
            JsonObject duplicate = record.DeepClone().AsObject();
            duplicate["UserSid"] = "OWNER";
            records.Add(duplicate);
        }
        string contents = records.ToJsonString();
        File.WriteAllText(Ledger, contents);
        Assert.False(new ControlRequestReplayStore(Paths).IsAvailable);
        Assert.Equal(contents, File.ReadAllText(Ledger));
    }

    [Fact]
    public void ARequestIdentityCannotReplayAnotherUsersResult()
    {
        ControlRequestReplayStore store = new ControlRequestReplayStore(Paths);
        ControlEnvelope request = Mutation();
        store.Store("owner", request, new ControlResponse { IsSuccessful = true, Message = "owner-only result" }, DateTime.UtcNow);
        Assert.False(store.TryGet("another-owner", request, DateTime.UtcNow, out _));
        Assert.True(store.TryBegin("another-owner", request, DateTime.UtcNow, out _));
        Assert.True(store.TryGet("owner", request, DateTime.UtcNow, out ControlResponse original));
        Assert.Equal("owner-only result", original.Message);
    }

    [Fact]
    public async Task ReservationIsDurableBeforeExecutionAndSuccessReplaysAfterRestart()
    {
        ControlRequestReplayStore store = new ControlRequestReplayStore(Paths);
        ControlEnvelope request = Mutation();
        int calls = 0;
        ControlResponse response = await store.ExecuteAsync("owner", request, () =>
        {
            calls++;
            Assert.True(new ControlRequestReplayStore(Paths).TryGet("owner", request, DateTime.UtcNow.AddDays(30), out ControlResponse pending));
            Assert.Equal(ControlErrorCode.OutcomeUnknown, pending.ErrorCode);
            return Task.FromResult(new ControlResponse { IsSuccessful = true, Message = "executed once" });
        }, CancellationToken.None);
        Assert.True(response.IsSuccessful);
        ControlRequestReplayStore restarted = new ControlRequestReplayStore(Paths);
        ControlResponse replay = await restarted.ExecuteAsync("owner", request, () => { calls++; return Task.FromResult(new ControlResponse()); }, CancellationToken.None);
        Assert.True(replay.IsSuccessful);
        Assert.Equal("executed once", replay.Message);
        Assert.Equal(1, calls);
        request.Payload = "different";
        Assert.True(restarted.TryGet("owner", request, DateTime.UtcNow, out ControlResponse conflict));
        Assert.Equal(ControlErrorCode.InvalidRequest, conflict.ErrorCode);
    }

    [Fact]
    public async Task ExplicitFailuresRemainRetryableButInterruptedWorkDoesNot()
    {
        ControlRequestReplayStore store = new ControlRequestReplayStore(Paths);
        ControlEnvelope request = Mutation();
        Assert.False((await store.ExecuteAsync("owner", request, () => Task.FromResult(new ControlResponse { ErrorCode = ControlErrorCode.DisplayControlBusy }), CancellationToken.None)).IsSuccessful);
        Assert.False(new ControlRequestReplayStore(Paths).TryGet("owner", request, DateTime.UtcNow, out _));
        ControlResponse uncertain = await store.ExecuteAsync("owner", request, () => Task.FromException<ControlResponse>(new IOException("Reply lost")), CancellationToken.None);
        Assert.Equal(ControlErrorCode.OutcomeUnknown, uncertain.ErrorCode);
        int calls = 0;
        ControlResponse replay = await new ControlRequestReplayStore(Paths).ExecuteAsync("owner", request,
            () => { calls++; return Task.FromResult(new ControlResponse { IsSuccessful = true }); }, CancellationToken.None);
        Assert.Equal(ControlErrorCode.OutcomeUnknown, replay.ErrorCode);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task ConcurrentDuplicateWaitsAndReplaysTheFirstResult()
    {
        ControlRequestReplayStore store = new ControlRequestReplayStore(Paths);
        ControlEnvelope request = Mutation();
        TaskCompletionSource<bool> started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<bool> release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        int calls = 0;
        async Task<ControlResponse> Execute() { calls++; started.TrySetResult(true); await release.Task; return new ControlResponse { IsSuccessful = true }; }
        Task<ControlResponse> first = store.ExecuteAsync("owner", request, Execute, CancellationToken.None);
        try
        {
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Task<ControlResponse> second = store.ExecuteAsync("owner", request, Execute, CancellationToken.None);
            Assert.False(second.IsCompleted);
            release.TrySetResult(true);
            Assert.True((await first).IsSuccessful);
            Assert.True((await second).IsSuccessful);
            Assert.Equal(1, calls);
        }
        finally { release.TrySetResult(true); await first; }
    }

    [Fact]
    public async Task FailedReservationWriteNeverExecutesTheCommand()
    {
        ControlRequestReplayStore store = new ControlRequestReplayStore(Paths);
        Directory.CreateDirectory(Ledger);
        int calls = 0;
        ControlResponse response = await store.ExecuteAsync("owner", Mutation(), () => { calls++; return Task.FromResult(new ControlResponse { IsSuccessful = true }); }, CancellationToken.None);
        Assert.False(response.IsSuccessful);
        Assert.False(store.IsAvailable);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task FailedCompletionWritePreservesTheDurableReservation()
    {
        ControlRequestReplayStore store = new ControlRequestReplayStore(Paths);
        ControlEnvelope request = Mutation();
        FileStream? locked = null;
        try
        {
            ControlResponse response = await store.ExecuteAsync("owner", request, () =>
            {
                locked = new FileStream(Ledger, FileMode.Open, FileAccess.Read, FileShare.Read);
                return Task.FromResult(new ControlResponse { IsSuccessful = true });
            }, CancellationToken.None);
            Assert.Equal(ControlErrorCode.OutcomeUnknown, response.ErrorCode);
            Assert.False(store.IsAvailable);
        }
        finally { locked?.Dispose(); }
        Assert.True(new ControlRequestReplayStore(Paths).TryGet("owner", request, DateTime.UtcNow, out ControlResponse recovered));
        Assert.Equal(ControlErrorCode.OutcomeUnknown, recovered.ErrorCode);
    }

    [Fact]
    public void LegacyResultsRemainReadableAndAreBackedUpBeforeNewReservations()
    {
        Directory.CreateDirectory(Paths.MachinePath);
        ControlEnvelope old = Mutation();
        string legacy = JsonSerializer.Serialize(new[] { LegacyRecord(old.RequestId, old.Payload) });
        File.WriteAllText(Ledger, legacy);
        ControlRequestReplayStore store = new ControlRequestReplayStore(Paths);
        Assert.True(store.IsAvailable);
        Assert.True(store.TryGet("owner", old, DateTime.UtcNow, out ControlResponse response));
        Assert.True(response.IsSuccessful);
        Assert.True(store.TryBegin("owner", Mutation(), DateTime.UtcNow, out _));
        Assert.Equal(legacy, File.ReadAllText(Ledger + ".bak"));
    }

    [Fact]
    public void CapacityDoesNotEvictUnexpiredOrUncertainRequests()
    {
        Directory.CreateDirectory(Paths.MachinePath);
        ControlEnvelope request = Mutation();
        object[] records = Enumerable.Range(0, 1000).Select(_ => LegacyRecord(Guid.NewGuid(), request.Payload)).ToArray();
        string original = JsonSerializer.Serialize(records);
        File.WriteAllText(Ledger, original);
        ControlRequestReplayStore store = new ControlRequestReplayStore(Paths);
        Assert.True(store.IsAvailable);
        Assert.False(store.TryBegin("owner", request, DateTime.UtcNow, out ControlResponse response));
        Assert.False(response.IsSuccessful);
        Assert.True(store.IsAvailable);
        Assert.Equal(original, File.ReadAllText(Ledger));
    }

    private static object LegacyRecord(Guid requestId, string payload) => new
    {
        UserSid = "owner", RequestId = requestId, MessageType = ControlMessageType.StartShortcut,
        PayloadHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))),
        CompletedUtc = DateTime.UtcNow, ResponsePayload = JsonSerializer.Serialize(new ControlResponse { IsSuccessful = true })
    };

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
