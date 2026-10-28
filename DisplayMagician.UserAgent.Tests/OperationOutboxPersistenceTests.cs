using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent.Runtime;
using Xunit;
using Service = DisplayMagician.ControlService;

namespace DisplayMagician.UserAgent.Tests;

[Collection("Agent repository state")]
public sealed class OperationOutboxPersistenceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagicianOutboxPersistenceTests", Guid.NewGuid().ToString("N"));
    private readonly List<(FieldInfo Field, object? Value)> _original = new();
    private string Pathname => Path.Combine(_root, "Settings", "OperationStatusOutbox.json");

    public OperationOutboxPersistenceTests()
    {
        foreach (Type type in new[] { typeof(ProfileRepository), typeof(AudioProfileRepository) })
            foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Where(field => !field.IsInitOnly && !field.IsLiteral))
                _original.Add((field, field.GetValue(null)));
    }

    public static IEnumerable<object[]> InvalidFiles()
    {
        foreach (string json in new[] { "null", "{", "[]", "{}", "{\"SchemaVersion\":2,\"Pending\":[],\"Active\":[]}",
            "{\"SchemaVersion\":1,\"Pending\":null,\"Active\":[]}", "{\"SchemaVersion\":1,\"Pending\":[],\"Active\":null}",
            "{\"SchemaVersion\":1,\"Pending\":[null],\"Active\":[]}" }) yield return new object[] { json };
        OperationStatusUpdate invalid = Update();
        invalid.UpdateId = Guid.Empty;
        yield return new object[] { Document(new[] { invalid }, Array.Empty<OperationStatusUpdate>()) };
        invalid = Update(); invalid.Phase = (OperationPhase)999;
        yield return new object[] { Document(new[] { invalid }, Array.Empty<OperationStatusUpdate>()) };
        OperationStatusUpdate duplicate = Update();
        yield return new object[] { Document(new[] { duplicate, duplicate }, Array.Empty<OperationStatusUpdate>()) };
        yield return new object[] { Document(Array.Empty<OperationStatusUpdate>(), new[] { duplicate, duplicate }) };
        invalid = Update(); invalid.IsTerminal = true;
        yield return new object[] { Document(Array.Empty<OperationStatusUpdate>(), new[] { invalid }) };
        OperationStatusUpdate conflicting = Copy(duplicate); conflicting.Message = "Different payload";
        yield return new object[] { Document(new[] { duplicate }, new[] { conflicting }) };
    }

    [Theory]
    [MemberData(nameof(InvalidFiles))]
    public void InvalidOutboxesArePreservedAndNewWorkUsesAMemoryQueue(string json)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Pathname)!);
        File.WriteAllText(Pathname, json);
        File.WriteAllText(Pathname + ".bak", "Keep backup");
        OperationStatusOutbox outbox = new(_root);
        Assert.Empty(outbox.GetPending());
        Assert.Empty(outbox.GetActive());
        OperationStatusUpdate update = Update();
        outbox.Enqueue(update);
        Assert.Single(outbox.GetPending());
        Assert.True(outbox.TryAcknowledge(update.UpdateId));
        Assert.Empty(outbox.GetPending());
        Assert.Single(outbox.GetActive());
        Assert.Equal(json, File.ReadAllText(Pathname));
        Assert.Equal("Keep backup", File.ReadAllText(Pathname + ".bak"));
    }

    [Fact]
    public void MissingPrimaryWithBackupDoesNotCreateANewFile()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Pathname)!);
        File.WriteAllText(Pathname + ".bak", "Prior queue");
        OperationStatusOutbox outbox = new(_root);
        outbox.Enqueue(Update());
        Assert.False(File.Exists(Pathname));
        Assert.Equal("Prior queue", File.ReadAllText(Pathname + ".bak"));
    }

    [Fact]
    public void DirectoryAtTheOutboxPathDoesNotCrashOrEnableRewrites()
    {
        Directory.CreateDirectory(Pathname);
        OperationStatusOutbox outbox = new(_root);
        outbox.Enqueue(Update());
        Assert.Single(outbox.GetPending());
        Assert.True(Directory.Exists(Pathname));
    }

    [Fact]
    public void AcknowledgementWriteFailureRetainsTheQueueUntilRetrySucceeds()
    {
        OperationStatusOutbox outbox = new(_root);
        OperationStatusUpdate first = Update();
        OperationStatusUpdate second = Update(first.OperationId);
        outbox.Enqueue(first);
        using (FileStream locked = new(Pathname, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            outbox.Enqueue(second);
            Assert.False(outbox.TryAcknowledge(first.UpdateId));
            Assert.Equal(new[] { first.UpdateId, second.UpdateId }, outbox.GetPending().Select(update => update.UpdateId));
        }
        Assert.True(outbox.TryAcknowledge(first.UpdateId));
        OperationStatusOutbox reloaded = new(_root);
        Assert.Equal(second.UpdateId, Assert.Single(reloaded.GetPending()).UpdateId);
        Assert.Equal(second.UpdateId, Assert.Single(reloaded.GetActive()).UpdateId);
        Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(Pathname)!, "*.tmp"));
    }

    [Fact]
    public void DuplicateEnqueueIsIdempotentButChangedPayloadIsRejected()
    {
        OperationStatusOutbox outbox = new(_root);
        OperationStatusUpdate update = Update();
        outbox.Enqueue(update);
        outbox.Enqueue(update);
        Assert.Single(new OperationStatusOutbox(_root).GetPending());
        OperationStatusUpdate changed = Copy(update); changed.Message = "Changed";
        Assert.Throws<ArgumentException>(() => outbox.Enqueue(changed));
        Assert.Equal(update.Message, Assert.Single(outbox.GetPending()).Message);
    }

    [Fact]
    public async Task ServiceWriteFailureDoesNotPreventLaterTerminalDeliveryOrLoseRetries()
    {
        Service.StoragePaths paths = new(Path.Combine(_root, "service"));
        Directory.CreateDirectory(paths.MachinePath);
        string statusFile = Path.Combine(paths.MachinePath, "OperationStatuses.json");
        File.WriteAllText(statusFile, "[]");
        StoreClient client = new(new Service.OperationStatusStore(paths));
        ProfileCommandHandler handler = Handler(client);
        OperationStatusUpdate first = Update();
        OperationStatusUpdate terminal = Update(first.OperationId); terminal.IsTerminal = true; terminal.Phase = OperationPhase.Completed;
        using (FileStream locked = new(statusFile, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            await Publish(handler, first);
            await Publish(handler, terminal);
            Assert.True(client.Store.Get("owner", first.OperationId)!.IsTerminal);
            Assert.Equal(2, Outbox(handler).GetPending().Length);
            await handler.ReconcileOperationStatusesAsync(CancellationToken.None);
            Assert.Equal(0, client.Reconciliations);
        }
        await handler.ReconcileOperationStatusesAsync(CancellationToken.None);
        Assert.Empty(Outbox(handler).GetPending());
        Assert.Equal(1, client.Reconciliations);
        Assert.Equal(2, new Service.OperationStatusStore(paths).Get("owner", first.OperationId)!.Sequence);
    }

    [Fact]
    public async Task LocalAckFailureStillDeliversTerminalAndOldRetriesStayHarmlessAfterServiceRestart()
    {
        Service.StoragePaths paths = new(Path.Combine(_root, "service"));
        StoreClient client = new(new Service.OperationStatusStore(paths));
        ProfileCommandHandler handler = Handler(client);
        OperationStatusUpdate first = Update();
        OperationStatusUpdate terminal = Update(first.OperationId); terminal.IsTerminal = true; terminal.Phase = OperationPhase.Completed;
        Outbox(handler).Enqueue(first);
        Outbox(handler).Enqueue(terminal);
        using (FileStream locked = new(Pathname, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            await handler.FlushPendingOperationStatusUpdatesAsync(CancellationToken.None);
            Assert.True(client.Store.Get("owner", first.OperationId)!.IsTerminal);
            Assert.Equal(2, Outbox(handler).GetPending().Length);
        }
        client.Store = new Service.OperationStatusStore(paths);
        await handler.FlushPendingOperationStatusUpdatesAsync(CancellationToken.None);
        Assert.Empty(Outbox(handler).GetPending());
        Assert.Equal(2, client.Store.Get("owner", first.OperationId)!.Sequence);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClientRecognisesMemoryOnlyResponsesWithoutAcknowledgingThem(bool matchingOperation)
    {
        OperationStatusUpdate update = Update();
        ReplyClient client = new() { Response = new ControlResponse { ErrorCode = ControlErrorCode.ExecutionFailed,
            OperationStatus = new OperationStatus { OperationId = matchingOperation ? update.OperationId : Guid.NewGuid() } } };
        if (matchingOperation)
            await Assert.ThrowsAsync<OperationStatusPersistenceException>(() => client.PublishOperationStatusAsync(new(), update, CancellationToken.None));
        else
            await Assert.ThrowsAsync<InvalidOperationException>(() => client.PublishOperationStatusAsync(new(), update, CancellationToken.None));
    }

    private ProfileCommandHandler Handler(ControlServiceClient client) => new(new AgentRegistration { UserSid = "owner", SessionId = 1 }, client, null, _root, new UserProfileOperationService());
    private static OperationStatusOutbox Outbox(ProfileCommandHandler handler) => (OperationStatusOutbox)typeof(ProfileCommandHandler).GetField("_operationStatusOutbox", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(handler)!;
    private static Task Publish(ProfileCommandHandler handler, OperationStatusUpdate update) => (Task)typeof(ProfileCommandHandler).GetMethod("PublishShortcutStatusAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(handler, new object[] { update, CancellationToken.None })!;
    private static OperationStatusUpdate Update(Guid? id = null) => new() { OperationId = id ?? Guid.NewGuid(), UpdateId = Guid.NewGuid(), OperationType = DisplayOperationType.StartShortcut, Phase = OperationPhase.StartingGame };
    private static OperationStatusUpdate Copy(OperationStatusUpdate update) => JsonSerializer.Deserialize<OperationStatusUpdate>(JsonSerializer.Serialize(update))!;
    private static string Document(OperationStatusUpdate[] pending, OperationStatusUpdate[] active) => JsonSerializer.Serialize(new { SchemaVersion = 1, Pending = pending, Active = active });
    public void Dispose()
    {
        foreach ((FieldInfo field, object? value) in _original) field.SetValue(null, value);
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    private sealed class StoreClient : ControlServiceClient
    {
        public Service.OperationStatusStore Store { get; set; }
        public int Reconciliations { get; private set; }
        public StoreClient(Service.OperationStatusStore store) => Store = store;
        public override Task<OperationStatus> PublishOperationStatusAsync(AgentRegistration registration, OperationStatusUpdate update, CancellationToken cancellationToken)
        {
            OperationStatus status = Store.Publish("owner", 1, update, DateTime.UtcNow, out bool persisted);
            return persisted ? Task.FromResult(status) : Task.FromException<OperationStatus>(new OperationStatusPersistenceException("Memory only"));
        }
        public override Task ReconcileOperationStatusesAsync(AgentRegistration registration, OperationStatusUpdate[] activeUpdates, CancellationToken cancellationToken)
        { Reconciliations++; return Task.CompletedTask; }
    }

    private sealed class ReplyClient : ControlServiceClient
    {
        public ControlResponse Response { get; set; } = new();
        protected override Task<ControlResponse> SendPersistentAsync(ControlEnvelope request, CancellationToken cancellationToken) => Task.FromResult(Response);
    }
}
