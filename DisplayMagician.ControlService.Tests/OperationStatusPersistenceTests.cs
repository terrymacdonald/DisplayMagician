using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class OperationStatusPersistenceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagicianStatusPersistenceTests", Guid.NewGuid().ToString("N"));
    private StoragePaths Paths => new(_root);
    private string Pathname => Path.Combine(Paths.MachinePath, "OperationStatuses.json");

    public static IEnumerable<object[]> InvalidFiles()
    {
        yield return new object[] { "null" };
        yield return new object[] { "[null]" };
        yield return new object[] { "{" };
        foreach ((string property, object value) in new (string, object)[]
        {
            ("OperationId", Guid.Empty), ("OwnerUserSid", ""), ("OwnerSessionId", -1), ("Sequence", 0),
            ("StartedUtc", default(DateTime)), ("UpdatedUtc", default(DateTime)), ("Phase", 999), ("OperationType", 999), ("ErrorCode", 999)
        })
        {
            JsonObject record = JsonSerializer.SerializeToNode(Status())!.AsObject();
            record[property] = JsonSerializer.SerializeToNode(value);
            yield return new object[] { new JsonArray(record).ToJsonString() };
        }
        OperationStatus duplicate = Status();
        yield return new object[] { JsonSerializer.Serialize(new[] { duplicate, duplicate }) };
        yield return new object[] { "{\"SchemaVersion\":3,\"Operations\":[]}" };
        yield return new object[] { "{\"SchemaVersion\":\"2\",\"Operations\":[]}" };
        yield return new object[] { "{\"SchemaVersion\":2,\"Operations\":[null]}" };
        yield return new object[] { "{\"SchemaVersion\":2,\"Operations\":[{}]}" };
    }

    [Theory]
    [MemberData(nameof(InvalidFiles))]
    public void InvalidHistoryIsPreservedWhileNewUpdatesRemainAvailableInMemory(string json)
    {
        Directory.CreateDirectory(Paths.MachinePath);
        File.WriteAllText(Pathname, json);
        File.WriteAllText(Pathname + ".bak", "Retain backup");
        OperationStatusStore store = new(Paths);
        Assert.Empty(store.GetAll("owner"));
        OperationStatusUpdate update = Update(Guid.NewGuid());
        OperationStatus result = store.Publish("owner", 1, update, DateTime.UtcNow, out bool persisted);
        Assert.False(persisted);
        Assert.Equal(update.OperationId, result.OperationId);
        Assert.NotNull(store.Get("owner", update.OperationId));
        Assert.Equal(json, File.ReadAllText(Pathname));
        Assert.Equal("Retain backup", File.ReadAllText(Pathname + ".bak"));
        File.WriteAllText(Pathname, "[]");
        store.Publish("owner", 1, Update(Guid.NewGuid()), DateTime.UtcNow, out bool persistedWithoutRestart);
        Assert.False(persistedWithoutRestart);
        Assert.Equal("[]", File.ReadAllText(Pathname));
    }

    [Fact]
    public void MissingPrimaryWithBackupCannotBeMistakenForANewStore()
    {
        Directory.CreateDirectory(Paths.MachinePath);
        File.WriteAllText(Pathname + ".bak", "Prior history");
        OperationStatusStore store = new(Paths);
        store.Publish("owner", 1, Update(Guid.NewGuid()), DateTime.UtcNow, out bool persisted);
        Assert.False(persisted);
        Assert.False(File.Exists(Pathname));
        Assert.Equal("Prior history", File.ReadAllText(Pathname + ".bak"));
    }

    [Fact]
    public void DirectoryAtTheHistoryPathDoesNotCrashOrEnableRewrites()
    {
        Directory.CreateDirectory(Pathname);
        OperationStatusStore store = new(Paths);
        store.Publish("owner", 1, Update(Guid.NewGuid()), DateTime.UtcNow, out bool persisted);
        Assert.False(persisted);
        Assert.True(Directory.Exists(Pathname));
    }

    [Fact]
    public void LegacyArrayMigratesWithABackupAndReloads()
    {
        Directory.CreateDirectory(Paths.MachinePath);
        OperationStatus original = Status();
        string legacy = JsonSerializer.Serialize(new[] { original });
        File.WriteAllText(Pathname, legacy);
        OperationStatusStore store = new(Paths);
        Assert.NotNull(store.Get("owner", original.OperationId));
        store.Publish("owner", 1, Update(Guid.NewGuid()), DateTime.UtcNow, out bool persisted);
        Assert.True(persisted);
        Assert.Equal(legacy, File.ReadAllText(Pathname + ".bak"));
        using JsonDocument stored = JsonDocument.Parse(File.ReadAllText(Pathname));
        Assert.Equal(2, stored.RootElement.GetProperty("SchemaVersion").GetInt32());
        Assert.Equal(2, new OperationStatusStore(Paths).GetAll("owner").Length);
    }

    [Fact]
    public void FailedWriteIsNotAcknowledgedAndRetryKeepsTheSameSequence()
    {
        OperationStatusStore store = new(Paths);
        Guid id = Guid.NewGuid();
        store.Publish("owner", 1, Update(id), DateTime.UtcNow);
        string before = File.ReadAllText(Pathname);
        OperationStatusUpdate next = Update(id);
        next.Phase = OperationPhase.WaitingForGameToClose;
        using (FileStream locked = new(Pathname, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            OperationStatus failed = store.Publish("owner", 1, next, DateTime.UtcNow, out bool persisted);
            Assert.False(persisted);
            Assert.Equal(2, failed.Sequence);
            Assert.Equal(before, File.ReadAllText(Pathname));
        }
        OperationStatus retry = store.Publish("owner", 1, next, DateTime.UtcNow, out bool saved);
        Assert.True(saved);
        Assert.Equal(2, retry.Sequence);
        Assert.Equal(next.UpdateId, new OperationStatusStore(Paths).Get("owner", id)!.LastUpdateId);
        Assert.Empty(Directory.GetFiles(Paths.MachinePath, "*.tmp"));
    }

    [Fact]
    public void OlderDeliveryRetriesCannotRegressTheLatestStateAfterServiceRestart()
    {
        OperationStatusStore store = new(Paths);
        Guid id = Guid.NewGuid();
        OperationStatusUpdate first = Update(id);
        OperationStatusUpdate second = Update(id);
        second.Phase = OperationPhase.WaitingForGameToClose;
        store.Publish("owner", 1, first, DateTime.UtcNow);
        store.Publish("owner", 1, second, DateTime.UtcNow);
        OperationStatusStore restarted = new(Paths);
        OperationStatus replay = restarted.Publish("owner", 1, first, DateTime.UtcNow, out bool persisted);
        Assert.True(persisted);
        Assert.Equal(2, replay.Sequence);
        Assert.Equal(OperationPhase.WaitingForGameToClose, replay.Phase);
    }

    [Fact]
    public async Task UnsavedTerminalResponseStillCompletesMachineCoordination()
    {
        Directory.CreateDirectory(Paths.MachinePath);
        File.WriteAllText(Pathname, "Unreadable history");
        ControlStateCoordinator coordinator = new();
        AgentRegistration agent = new() { UserSid = "owner", SessionId = 1, ProcessId = 100, IsReady = true };
        coordinator.RegisterAgent(agent, DateTime.UtcNow);
        Assert.True(coordinator.TryAcquireDisplayControl("owner", 1, DateTime.UtcNow).IsGranted);
        Guid id = Guid.NewGuid();
        Assert.True(coordinator.TryBeginDisplayOperation("owner", 1, id, DateTime.UtcNow));
        OperationStatusUpdate terminal = Update(id);
        terminal.IsTerminal = true;
        terminal.Phase = OperationPhase.Completed;
        ControlResponse response = await SendAgentRequestAsync(coordinator, new OperationStatusStore(Paths), new ControlEnvelope
            { MessageType = ControlMessageType.OperationCompleted, Payload = JsonSerializer.Serialize(terminal) });
        Assert.False(response.IsSuccessful);
        Assert.Equal(ControlErrorCode.ExecutionFailed, response.ErrorCode);
        Assert.True(response.OperationStatus!.IsTerminal);
        Assert.Null(coordinator.GetDisplayControlLease()!.ActiveOperationId);
        Assert.Equal("Unreadable history", File.ReadAllText(Pathname));
    }

    [Fact]
    public async Task ReconciliationOwnershipFailureReturnsAStructuredResponse()
    {
        OperationStatusStore store = new(Paths);
        OperationStatusUpdate update = Update(Guid.NewGuid());
        store.Publish("another-owner", 2, update, DateTime.UtcNow);
        ControlResponse response = await SendAgentRequestAsync(new ControlStateCoordinator(), store, new ControlEnvelope
        {
            MessageType = ControlMessageType.ReconcileOperationStatuses,
            Payload = JsonSerializer.Serialize(new OperationStatusReconciliationRequest { ActiveUpdates = new[] { update } })
        });
        Assert.False(response.IsSuccessful);
        Assert.Equal(ControlErrorCode.CallerIdentityMismatch, response.ErrorCode);
        Assert.Equal(1, store.Get("another-owner", update.OperationId)!.Sequence);
    }

    private async Task<ControlResponse> SendAgentRequestAsync(ControlStateCoordinator coordinator, OperationStatusStore store, ControlEnvelope request)
    {
        using HttpClient http = new();
        ClientSyncCoordinator sync = new(http, new MachineScheduleCoordinator(new MachineScheduleStore(Paths)), coordinator, new AgentCommandClient());
        NamedPipeControlServer server = new(coordinator, Paths, new UserDataMigrationRunner(new LegacyFileMigration()), store, sync, new OperationDecisionStore(Paths));
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        string name = "dm-status-response-" + Guid.NewGuid().ToString("N");
        using NamedPipeServerStream serverPipe = new(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        using NamedPipeClientStream clientPipe = new(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
        Task connection = serverPipe.WaitForConnectionAsync(timeout.Token);
        await clientPipe.ConnectAsync(timeout.Token);
        await connection;
        Type identityType = typeof(NamedPipeControlServer).GetNestedType("PipeClientIdentity", BindingFlags.NonPublic)!;
        object identity = Activator.CreateInstance(identityType, "owner", 1, 100)!;
        Task handling = (Task)typeof(NamedPipeControlServer).GetMethod("HandleAgentMessageAsync", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(server, new object[] { serverPipe, identity, request, timeout.Token })!;
        ControlEnvelope responseEnvelope = (await ControlEnvelopeSerializer.ReadAsync(clientPipe, timeout.Token))!;
        await handling;
        return JsonSerializer.Deserialize<ControlResponse>(responseEnvelope.Payload)!;
    }

    private static OperationStatus Status() => new() { OperationId = Guid.NewGuid(), OwnerUserSid = "owner", OwnerSessionId = 1, Sequence = 1,
        StartedUtc = DateTime.UtcNow, UpdatedUtc = DateTime.UtcNow, LastUpdateId = Guid.NewGuid(), OperationType = DisplayOperationType.StartShortcut, Phase = OperationPhase.StartingGame };
    private static OperationStatusUpdate Update(Guid id) => new() { UpdateId = Guid.NewGuid(), OperationId = id, OperationType = DisplayOperationType.StartShortcut, Phase = OperationPhase.StartingGame };
    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
