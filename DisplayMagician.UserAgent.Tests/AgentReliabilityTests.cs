using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using DisplayMagician.UserAgent.Messaging;
using Xunit;
using AgentAudioRepository = DisplayMagician.UserAgent.Runtime.AudioProfileRepository;
using AgentAudioProfile = DisplayMagician.UserAgent.Runtime.AudioProfileItem;
using AgentProfileRepository = DisplayMagician.UserAgent.Runtime.ProfileRepository;
using AgentProfile = DisplayMagician.UserAgent.Runtime.ProfileItem;
using AgentMessageSync = DisplayMagician.Messaging.MessageSyncService;

namespace DisplayMagician.UserAgent.Tests;

[CollectionDefinition("Agent repository state", DisableParallelization = true)]
public sealed class AgentRepositoryStateCollection { }

[Collection("Agent repository state")]
public sealed class AgentReliabilityTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-AgentReliability", Guid.NewGuid().ToString("N"));
    private readonly List<(FieldInfo Field, object? Value)> _originalState = new List<(FieldInfo, object?)>();

    [Fact]
    public void FailedLaunchDoesNotPutArgumentsInSupportLogs()
    {
        NLog.Config.LoggingConfiguration? original = NLog.LogManager.Configuration;
        NLog.Targets.MemoryTarget messages = new NLog.Targets.MemoryTarget { Layout = "${message} ${exception:format=message}" };
        NLog.Config.LoggingConfiguration configuration = new NLog.Config.LoggingConfiguration();
        configuration.AddRuleForAllLevels(messages);
        string marker = "private-argument-" + Guid.NewGuid().ToString("N");
        try
        {
            NLog.LogManager.Configuration = configuration;
            Assert.Empty(global::DisplayMagician.Processes.ProcessUtils.StartProcess(Path.Combine(_root, "missing.exe"), marker, global::DisplayMagician.Contracts.ProcessPriority.Normal));
            Assert.NotEmpty(messages.Logs);
            Assert.DoesNotContain(messages.Logs, message => message.Contains(marker, StringComparison.Ordinal));
        }
        finally { NLog.LogManager.Configuration = original; }
    }

    [Fact]
    public async Task MessageWriteFailureIsReportedAndKeepsThePreviousReadState()
    {
        using FakeProfileOperations profiles = new FakeProfileOperations();
        ProfileCommandHandler handler = new ProfileCommandHandler(new AgentRegistration { UserSid = "test", SessionId = 1 }, new FakeServiceClient(), null, _root, profiles);
        string path = Path.Combine(_root, "Messages", "MessagesIndex.json");
        string id = Guid.NewGuid().ToString("D");
        string original = Newtonsoft.Json.JsonConvert.SerializeObject(new DisplayMagician.Messaging.MessageStoreDocument
        { Messages = new List<DisplayMagician.Messaging.LocalMessage> { new DisplayMagician.Messaging.LocalMessage { Id = id, IsRead = false } } });
        File.WriteAllText(path, original);
        using FileStream locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        ControlResponse response = await handler.HandleAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.SetMessageReadState,
            Payload = JsonSerializer.Serialize(new SetMessageReadStateRequest { MessageIds = new[] { id }, IsRead = true })
        }, CancellationToken.None);
        Assert.False(response.IsSuccessful);
        Assert.Equal(ControlErrorCode.ExecutionFailed, response.ErrorCode);
        Assert.Equal(original, File.ReadAllText(path));
    }

    [Theory]
    [InlineData(ControlMessageType.CreateProfileFromCurrent, null)]
    [InlineData(ControlMessageType.CreateProfileFromCurrent, " ")]
    [InlineData(ControlMessageType.CreateAudioProfileFromCurrent, null)]
    [InlineData(ControlMessageType.CreateAudioProfileFromCurrent, " ")]
    [InlineData(ControlMessageType.RenameProfile, null)]
    [InlineData(ControlMessageType.RenameProfile, " ")]
    [InlineData(ControlMessageType.RenameAudioProfile, null)]
    [InlineData(ControlMessageType.RenameAudioProfile, " ")]
    public async Task InvalidProfileNamesAreRejectedBeforeCaptureOrRename(ControlMessageType type, string? name)
    {
        using FakeProfileOperations profiles = new FakeProfileOperations();
        ProfileCommandHandler handler = new ProfileCommandHandler(new AgentRegistration { UserSid = "test", SessionId = 1 }, new FakeServiceClient(), null, _root, profiles);
        AgentProfile display = new AgentProfile { Name = "Original display" };
        AgentAudioProfile audio = new AgentAudioProfile { Name = "Original audio" };
        AgentProfileRepository.AllProfiles.Add(display);
        AgentAudioRepository.AllAudioProfiles.Add(audio);
        string id = type == ControlMessageType.RenameAudioProfile ? audio.UUID : display.UUID;
        ControlResponse response = await handler.HandleAsync(new ControlEnvelope
        {
            MessageType = type,
            Payload = JsonSerializer.Serialize(new { ProfileId = id, Name = name })
        }, CancellationToken.None);
        Assert.False(response.IsSuccessful);
        Assert.Equal(ControlErrorCode.ValidationFailed, response.ErrorCode);
        Assert.Equal("Original display", display.Name);
        Assert.Equal("Original audio", audio.Name);
    }

    public AgentReliabilityTests()
    {
        foreach (Type type in new[] { typeof(AgentProfileRepository), typeof(AgentAudioRepository), typeof(AgentProfile), typeof(AgentAudioProfile) })
            foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(field => !field.IsInitOnly && !field.IsLiteral))
                _originalState.Add((field, field.GetValue(null)));
        AgentProfileRepository.ConfigureStoragePath(_root);
        AgentAudioRepository.ConfigureStoragePath(_root);
        typeof(AgentProfileRepository).GetField("_userAgentRepositoryConnection", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, null);
        typeof(AgentAudioRepository).GetField("_userAgentRepositoryConnection", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, null);
        AgentProfileRepository.ProfilesLoaded = true;
        AgentAudioRepository.AudioProfilesLoaded = true;
    }

    [Theory]
    [InlineData(ControlMessageType.SetMessageReadState, "null")]
    [InlineData(ControlMessageType.SetMessageReadState, "{\"MessageIds\":null}")]
    [InlineData(ControlMessageType.SetMessageReadState, "{\"MessageIds\":[]}")]
    [InlineData(ControlMessageType.SetMessageReadState, "{\"MessageIds\":[null]}")]
    [InlineData(ControlMessageType.SetMessageReadState, "{\"MessageIds\":[\" \" ]}")]
    [InlineData(ControlMessageType.ApplyClientSyncMessages, "{\"SchemaVersion\":1,\"Messages\":null}")]
    [InlineData(ControlMessageType.ApplyClientSyncMessages, "{\"SchemaVersion\":1,\"Messages\":[null]}")]
    [InlineData(ControlMessageType.ApplyClientSyncMessages, "{\"SchemaVersion\":1,\"Messages\":[{\"Media\":null}]}")]
    [InlineData(ControlMessageType.ApplyClientSyncMessages, "{\"SchemaVersion\":1,\"Messages\":[{\"Media\":[null]}]}")]
    [InlineData(ControlMessageType.GetRepositorySnapshot, "{\"Repository\":999}")]
    [InlineData(ControlMessageType.GetRepositorySnapshot, "{\"Repository\":-1}")]
    [InlineData(ControlMessageType.CommitRepositorySnapshot, "{\"Repository\":999,\"Json\":\"{}\"}")]
    [InlineData(ControlMessageType.CommitRepositorySnapshot, "{\"Repository\":1,\"Json\":null}")]
    [InlineData(ControlMessageType.CommitRepositorySnapshot, "{\"Repository\":1,\"Json\":\" \"}")]
    public async Task InvalidNestedRequestsAreRejectedWithoutChangingStoredData(ControlMessageType type, string payload)
    {
        using FakeProfileOperations profiles = new FakeProfileOperations();
        ProfileCommandHandler handler = new ProfileCommandHandler(new AgentRegistration { UserSid = "test", SessionId = 1 }, new FakeServiceClient(), null, _root, profiles);
        string audioPath = Path.Combine(_root, "AudioProfiles", "AudioProfiles.json");
        Directory.CreateDirectory(Path.GetDirectoryName(audioPath)!);
        File.WriteAllText(audioPath, "existing-audio-data");
        string indexPath = Path.Combine(_root, "Messages", "MessagesIndex.json");
        string originalIndex = File.Exists(indexPath) ? File.ReadAllText(indexPath) : "";
        ControlResponse result = await handler.HandleAsync(new ControlEnvelope { MessageType = type, Payload = payload }, CancellationToken.None);
        Assert.False(result.IsSuccessful);
        Assert.Equal(ControlErrorCode.InvalidRequest, result.ErrorCode);
        Assert.Equal("existing-audio-data", File.ReadAllText(audioPath));
        Assert.Equal(originalIndex, File.Exists(indexPath) ? File.ReadAllText(indexPath) : "");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AgentRestartClosesInterruptedOperationsAndPreservesLiveWork(bool recoveryRequired)
    {
        Guid interruptedId = Guid.NewGuid();
        OperationStatusOutbox original = new OperationStatusOutbox(_root);
        original.Enqueue(new OperationStatusUpdate { OperationId = interruptedId, OperationType = DisplayOperationType.StartShortcut, Phase = OperationPhase.WaitingForGameToClose });
        OperationStatusOutbox restarted = new OperationStatusOutbox(_root);
        Guid liveId = Guid.NewGuid();
        restarted.Enqueue(new OperationStatusUpdate { OperationId = liveId, OperationType = DisplayOperationType.ApplyAudioProfile, Phase = OperationPhase.Requested });

        restarted.CompleteInterruptedOperations(recoveryRequired);
        restarted.CompleteInterruptedOperations(recoveryRequired);

        Assert.Equal(liveId, Assert.Single(restarted.GetActive()).OperationId);
        OperationStatusUpdate terminal = Assert.Single(restarted.GetPending(), update => update.OperationId == interruptedId && update.IsTerminal);
        Assert.Equal(OperationPhase.Failed, terminal.Phase);
        Assert.Equal(recoveryRequired ? ControlErrorCode.RecoveryRequired : ControlErrorCode.ExecutionFailed, terminal.ErrorCode);
        Assert.Contains(new OperationStatusOutbox(_root).GetPending(), update => update.UpdateId == terminal.UpdateId);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task ShortcutCompletionIncludesBothRestorationResults(bool displayRestores, bool audioRestores)
    {
        ShortcutStore store = new ShortcutStore(_root);
        store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = 0,
            Json = "{\"Shortcuts\":[{\"UUID\":\"test\",\"Category\":2,\"ProfileUUID\":\"target-display\",\"AudioProfileUUID\":\"target-audio\"}]}" });
        using FakeProfileOperations profiles = new FakeProfileOperations { DisplayRestores = displayRestores, AudioRestores = audioRestores };
        ShortcutRecoveryStore recovery = new ShortcutRecoveryStore(_root);
        ShortcutRunner runner = new ShortcutRunner(store, new AutomaticGameDetectionRegistry(), profiles, recovery);

        ShortcutRunResult result = await runner.ApplyShortcutProfilesAsync("test", 0, CancellationToken.None);

        Assert.Equal(2, profiles.RestoreCalls);
        Assert.Equal(displayRestores && audioRestores ? ShortcutRunOutcome.Completed : ShortcutRunOutcome.Failed, result.Outcome);
        Assert.Equal(!(displayRestores && audioRestores), result.RequiresRecovery);
        Assert.Equal(result.RequiresRecovery, recovery.HasPendingRecovery());
        if (result.RequiresRecovery)
        {
            string original = File.ReadAllText(Path.Combine(_root, "Settings", "ShortcutRecovery.json"));
            await runner.ApplyShortcutProfilesAsync("test", 0, CancellationToken.None);
            Assert.Equal(original, File.ReadAllText(Path.Combine(_root, "Settings", "ShortcutRecovery.json")));
            Assert.Equal(2, profiles.RestoreCalls);
        }
    }

    [Fact]
    public void FailedProfileWritesRollBackCreationRenameDeletionAndSettings()
    {
        AgentProfile display = new AgentProfile { Name = "Original display" };
        AgentAudioProfile audio = new AgentAudioProfile { Name = "Original audio" };
        AgentProfileRepository.AllProfiles.Add(display);
        AgentAudioRepository.AllAudioProfiles.Add(audio);
        // A directory in place of the JSON file forces a real filesystem write failure.
        Directory.CreateDirectory(AgentProfileRepository.ProfileStorageFileName);
        Directory.CreateDirectory(AgentAudioRepository.AudioProfileStorageFileName);

        Assert.False(AgentProfileRepository.RenameProfile(display, "Changed display"));
        Assert.False(AgentAudioRepository.RenameAudioProfile(audio, "Changed audio"));
        Assert.Equal("Original display", display.Name);
        Assert.Equal("Original audio", audio.Name);
        Assert.False(AgentProfileRepository.RemoveProfile(display));
        Assert.False(AgentAudioRepository.RemoveAudioProfile(audio));
        Assert.Same(display, Assert.Single(AgentProfileRepository.AllProfiles));
        Assert.Same(audio, Assert.Single(AgentAudioRepository.AllAudioProfiles));
        Assert.False(AgentProfileRepository.AddProfile(new AgentProfile { Name = "New display", ProfileDisplayIdentifiers = new List<string> { "different-layout" } }));
        Assert.False(AgentAudioRepository.AddAudioProfile(new AgentAudioProfile { Name = "New audio", ApplyProfileDelay = 1 }));
        Assert.Same(display, Assert.Single(AgentProfileRepository.AllProfiles));
        Assert.Same(audio, Assert.Single(AgentAudioRepository.AllAudioProfiles));
        int originalCount = display.ApplyProfileCount;
        Assert.False(AgentProfileRepository.UpdateProfileSettings(display, new DisplayProfileSettings { ApplyProfileCount = originalCount + 1, ForceExplorerRestart = !display.ForceExplorerRestart }));
        Assert.Equal(originalCount, display.ApplyProfileCount);
    }

    [Fact]
    public void ShortcutStateCaptureRefreshesPreviouslyCachedProfileIdentities()
    {
        AgentProfile cachedDisplay = new AgentProfile { Name = "Stale display cache" };
        AgentAudioProfile cachedAudio = new AgentAudioProfile { Name = "Stale audio cache" };
        AgentProfileRepository.CurrentProfile = cachedDisplay;
        AgentAudioRepository.CurrentAudioProfile = cachedAudio;

        UserProfileOperationState state = new UserProfileOperationService().CaptureCurrentProfileState();

        // Neither cache entry belongs to the saved collections. A fresh hardware read
        // cannot legitimately retain either cached identity, including on a headless host.
        Assert.NotEqual(cachedDisplay.UUID, state.DisplayProfileId);
        Assert.NotEqual(cachedAudio.UUID, state.AudioProfileId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ActiveAudioOperationBlocksStopAndOverlappingWorkUntilCompletion(bool failAudio)
    {
        AgentRegistration registration = new AgentRegistration { UserSid = "test-user", SessionId = 1, IsReady = true };
        using FakeProfileOperations profiles = new FakeProfileOperations { BlockAudio = true, FailAudio = failAudio };
        FakeServiceClient client = new FakeServiceClient();
        ProfileCommandHandler handler = new ProfileCommandHandler(registration, client, new UnlockedSession(), _root, profiles);
        AgentAudioProfile audio = new AgentAudioProfile { Name = "Audio target" };
        AgentAudioRepository.AudioProfilesLoaded = true;
        AgentAudioRepository.AllAudioProfiles.Add(audio);
        AgentProfile display = new AgentProfile { Name = "Display target" };
        AgentProfileRepository.ProfilesLoaded = true;
        AgentProfileRepository.AllProfiles.Add(display);
        ControlEnvelope apply = new ControlEnvelope { MessageType = ControlMessageType.ApplyAudioProfile,
            Payload = JsonSerializer.Serialize(new ApplyAudioProfileRequest { ProfileId = audio.UUID }) };
        try
        {
            Assert.True((await handler.HandleAsync(apply, CancellationToken.None)).IsSuccessful);
            await profiles.AudioStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal(AgentOperationState.Running, registration.OperationState);
            Assert.False((await handler.HandleAsync(new ControlEnvelope { MessageType = ControlMessageType.StopAgentIfIdle }, CancellationToken.None)).IsSuccessful);
            Assert.False(handler.StopRequested);
            Assert.Equal(ControlErrorCode.DisplayControlBusy, (await handler.HandleAsync(apply, CancellationToken.None)).ErrorCode);
            Assert.Equal(ControlErrorCode.DisplayControlBusy, (await handler.HandleAsync(new ControlEnvelope
            {
                MessageType = ControlMessageType.ApplyProfile,
                Payload = JsonSerializer.Serialize(new ApplyProfileRequest { ProfileId = display.UUID, OperationId = Guid.NewGuid() })
            }, CancellationToken.None)).ErrorCode);
            Assert.Equal(ControlErrorCode.DisplayControlBusy, (await handler.HandleAsync(new ControlEnvelope
            {
                MessageType = ControlMessageType.StartShortcut,
                Payload = JsonSerializer.Serialize(new StartShortcutRequest { ShortcutId = "test", OperationId = Guid.NewGuid() })
            }, CancellationToken.None)).ErrorCode);
        }
        finally
        {
            profiles.ReleaseAudio.Set();
            await client.IdleReported.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        Assert.Equal(AgentOperationState.Idle, registration.OperationState);
        OperationStatusUpdate terminal = Assert.Single(new OperationStatusOutbox(_root).GetPending(), update => update.OperationType == DisplayOperationType.ApplyAudioProfile && update.IsTerminal);
        Assert.Equal(!failAudio, terminal.IsSuccessful);
        Assert.True((await handler.HandleAsync(new ControlEnvelope { MessageType = ControlMessageType.StopAgentIfIdle }, CancellationToken.None)).IsSuccessful);
        Assert.Equal(ControlErrorCode.DisplayControlBusy, (await handler.HandleAsync(apply, CancellationToken.None)).ErrorCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MessageSyncPreservesReadStateChangedDuringDownload(bool newReadState)
    {
        string directory = Path.Combine(_root, "Messages");
        Directory.CreateDirectory(directory);
        string id = Guid.NewGuid().ToString("D");
        File.WriteAllText(Path.Combine(directory, "message.md"), "old content");
        File.WriteAllText(Path.Combine(directory, "MessagesIndex.json"), Newtonsoft.Json.JsonConvert.SerializeObject(new DisplayMagician.Messaging.MessageStoreDocument
        {
            Messages = new List<DisplayMagician.Messaging.LocalMessage> { new DisplayMagician.Messaging.LocalMessage
                { Id = id, MarkdownFileName = "message.md", Sha256 = "old", IsRead = !newReadState } }
        }));
        using BlockingDownload download = new BlockingDownload();
        using HttpClient http = new HttpClient(download);
        AgentMessageSync sync = new AgentMessageSync(http, NLog.LogManager.GetCurrentClassLogger(), "https://example.invalid/manifest.json", directory);
        UserMessageStore messages = new UserMessageStore(_root, sync);
        Task<DisplayMagician.Messaging.MessageSyncResult> pending = sync.SyncMessagesAsync("4.0", CancellationToken.None,
            new DisplayMagician.Messaging.MessageManifestDocument { Messages = new List<DisplayMagician.Messaging.MessageManifestEntry>
                { new DisplayMagician.Messaging.MessageManifestEntry { Id = id, Title = "Updated", Url = "/message.md", Format = "md" } } });
        try
        {
            await download.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.True(messages.SetReadState(new[] { id }, newReadState));
        }
        finally { download.Response.TrySetResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("new content") }); }
        Assert.True((await pending).Success);
        MessageView message = Assert.Single(messages.GetMessages().Messages);
        Assert.Equal(newReadState, message.IsRead);
        Assert.Equal("new content", message.Content);
    }

    public static IEnumerable<object[]> InvalidRepositoryDocuments()
    {
        foreach (RepositoryKind kind in new[] { RepositoryKind.DisplayProfiles, RepositoryKind.AudioProfiles, RepositoryKind.Shortcuts })
        {
            string field = kind == RepositoryKind.DisplayProfiles ? "Profiles" : kind == RepositoryKind.AudioProfiles ? "AudioProfiles" : "Shortcuts";
            foreach (string json in new[] { "null", "[]", "42", "{}", $"{{\"{field}\":null}}", $"{{\"{field}\":{{}}}}", $"{{\"{field}\":[null]}}", $"{{\"{field}\":[{{}}]}}", $"{{\"{field}\":[{{\"UUID\":\" \"}}]}}", $"{{\"{field}\":[{{\"UUID\":\"same\"}},{{\"UUID\":\"SAME\"}}]}}" })
                yield return new object[] { kind, json };
        }
    }

    [Theory]
    [MemberData(nameof(InvalidRepositoryDocuments))]
    public void InvalidRepositoryDocumentsPreserveTheLastSnapshot(RepositoryKind kind, string json)
    {
        ShortcutStore shortcuts = new ShortcutStore(_root);
        string field = kind == RepositoryKind.DisplayProfiles ? "Profiles" : kind == RepositoryKind.AudioProfiles ? "AudioProfiles" : "Shortcuts";
        RepositoryCommitResult original = CommitRepository(shortcuts, new RepositoryCommitRequest { Repository = kind, Json = $"{{\"{field}\":[]}}" });
        Assert.Throws<JsonException>(() => CommitRepository(shortcuts, new RepositoryCommitRequest { Repository = kind, ExpectedRevision = original.Snapshot!.Revision, Json = json }));
        RepositorySnapshot current = kind == RepositoryKind.DisplayProfiles ? AgentProfileRepository.GetSnapshot() : kind == RepositoryKind.AudioProfiles ? AgentAudioRepository.GetSnapshot() : shortcuts.GetSnapshot();
        Assert.Equal(original.Snapshot!.Json, current.Json);
        Assert.Equal(original.Snapshot.Revision, current.Revision);
        Assert.Empty(Directory.GetFiles(_root, "*.tmp", SearchOption.AllDirectories));
    }

    [Theory]
    [InlineData(RepositoryKind.DisplayProfiles)]
    [InlineData(RepositoryKind.AudioProfiles)]
    [InlineData(RepositoryKind.Shortcuts)]
    public async Task ConcurrentRepositoryCommitsHaveOneWinnerAndReturnThatSnapshotToConflicts(RepositoryKind kind)
    {
        ShortcutStore shortcuts = new ShortcutStore(_root);
        string field = kind == RepositoryKind.DisplayProfiles ? "Profiles" : kind == RepositoryKind.AudioProfiles ? "AudioProfiles" : "Shortcuts";
        RepositoryCommitResult original = CommitRepository(shortcuts, new RepositoryCommitRequest { Repository = kind, Json = $"{{\"{field}\":[],\"Unrelated\":\"preserve\"}}" });
        TaskCompletionSource<bool> start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<RepositoryCommitResult>[] requests = Enumerable.Range(0, 12).Select(index => Task.Run(async () =>
        {
            await start.Task;
            return CommitRepository(shortcuts, new RepositoryCommitRequest { Repository = kind, ExpectedRevision = original.Snapshot!.Revision, Json = $"{{\"{field}\":[],\"Unrelated\":\"preserve\",\"Writer\":{index}}}" });
        })).ToArray();
        start.TrySetResult(true);
        RepositoryCommitResult[] results = await Task.WhenAll(requests).WaitAsync(TimeSpan.FromSeconds(15));
        RepositoryCommitResult winner = Assert.Single(results, result => !result.WasConflict);
        Assert.All(results, result => Assert.Equal(winner.Snapshot!.Revision, result.Snapshot!.Revision));
        Assert.All(results, result => Assert.Equal(winner.Snapshot!.Json, result.Snapshot!.Json));
        Assert.Contains("preserve", winner.Snapshot!.Json);
        string backup = Assert.Single(Directory.GetFiles(_root, "*.json.bak", SearchOption.AllDirectories));
        Assert.Equal(original.Snapshot!.Json, File.ReadAllText(backup, System.Text.Encoding.Unicode));
        Assert.Empty(Directory.GetFiles(_root, "*.tmp", SearchOption.AllDirectories));
    }

    [Theory]
    [InlineData(RepositoryKind.DisplayProfiles)]
    [InlineData(RepositoryKind.AudioProfiles)]
    [InlineData(RepositoryKind.Shortcuts)]
    public void FailedRepositoryReplacementPreservesDataAndRemovesTemporaryFiles(RepositoryKind kind)
    {
        ShortcutStore shortcuts = new ShortcutStore(_root);
        string field = kind == RepositoryKind.DisplayProfiles ? "Profiles" : kind == RepositoryKind.AudioProfiles ? "AudioProfiles" : "Shortcuts";
        RepositoryCommitResult original = CommitRepository(shortcuts, new RepositoryCommitRequest { Repository = kind, Json = $"{{\"{field}\":[]}}" });
        string file = Assert.Single(Directory.GetFiles(_root, kind == RepositoryKind.DisplayProfiles ? "DisplayProfiles.json" : kind == RepositoryKind.AudioProfiles ? "AudioProfiles.json" : "Shortcuts.json", SearchOption.AllDirectories));
        using (FileStream held = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Assert.ThrowsAny<IOException>(() => CommitRepository(shortcuts, new RepositoryCommitRequest { Repository = kind, ExpectedRevision = original.Snapshot!.Revision, Json = $"{{\"{field}\":[],\"Change\":true}}" }));
            Assert.Equal(original.Snapshot!.Json, File.ReadAllText(file, System.Text.Encoding.Unicode));
            Assert.Empty(Directory.GetFiles(_root, "*.tmp", SearchOption.AllDirectories));
        }
        Assert.False(CommitRepository(shortcuts, new RepositoryCommitRequest { Repository = kind, ExpectedRevision = original.Snapshot!.Revision, Json = $"{{\"{field}\":[],\"Change\":true}}" }).WasConflict);
    }

    private static RepositoryCommitResult CommitRepository(ShortcutStore shortcuts, RepositoryCommitRequest request)
        => request.Repository == RepositoryKind.DisplayProfiles ? AgentProfileRepository.CommitSnapshot(request) : request.Repository == RepositoryKind.AudioProfiles ? AgentAudioRepository.CommitSnapshot(request) : shortcuts.Commit(request);

    [Theory]
    [InlineData(RepositoryKind.DisplayProfiles)]
    [InlineData(RepositoryKind.AudioProfiles)]
    public void RuntimeSaveImmediatelyAfterCommitKeepsTheCommittedDefinitions(RepositoryKind kind)
    {
        ShortcutStore shortcuts = new ShortcutStore(_root);
        string field = kind == RepositoryKind.DisplayProfiles ? "Profiles" : "AudioProfiles";
        string id = Guid.NewGuid().ToString("D");
        string json = kind == RepositoryKind.DisplayProfiles
            ? "{\"ProfileFileVersion\":\"4\",\"Profiles\":[" + UserProfileOperationService.SerializeDisplayRecoveryProfile(new AgentProfile { UUID = id, Name = "Committed profile" }) + "]}"
            : $"{{\"AudioProfileFileVersion\":\"1\",\"AudioProfiles\":[{{\"UUID\":\"{id}\",\"Name\":\"Committed profile\",\"WindowsAudioConfig\":{{}}}}]}}";
        CommitRepository(shortcuts, new RepositoryCommitRequest { Repository = kind, Json = json });
        Assert.True(kind == RepositoryKind.DisplayProfiles ? AgentProfileRepository.SaveProfiles() : AgentAudioRepository.SaveAudioProfiles());
        RepositorySnapshot snapshot = kind == RepositoryKind.DisplayProfiles ? AgentProfileRepository.GetSnapshot() : AgentAudioRepository.GetSnapshot();
        using JsonDocument saved = JsonDocument.Parse(snapshot.Json);
        JsonElement profile = Assert.Single(saved.RootElement.GetProperty(field).EnumerateArray());
        Assert.Equal(id, profile.GetProperty("UUID").GetString());
        Assert.Equal("Committed profile", profile.GetProperty("Name").GetString());
    }

    [Theory]
    [InlineData(RepositoryKind.DisplayProfiles)]
    [InlineData(RepositoryKind.AudioProfiles)]
    public async Task ProfileCommitRejectsInvalidKnownPropertyTypesBeforeReplacingData(RepositoryKind kind)
    {
        using FakeProfileOperations profiles = new FakeProfileOperations();
        ProfileCommandHandler handler = new ProfileCommandHandler(new AgentRegistration { UserSid = "test", SessionId = 1 }, new FakeServiceClient(), null, _root, profiles);
        ShortcutStore shortcuts = new ShortcutStore(_root);
        string field = kind == RepositoryKind.DisplayProfiles ? "Profiles" : "AudioProfiles";
        RepositoryCommitResult original = CommitRepository(shortcuts, new RepositoryCommitRequest { Repository = kind, Json = $"{{\"{field}\":[]}}" });
        ControlResponse result = await handler.HandleAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.CommitRepositorySnapshot,
            Payload = JsonSerializer.Serialize(new RepositoryCommitRequest { Repository = kind, ExpectedRevision = original.Snapshot!.Revision, Json = $"{{\"{field}\":[{{\"UUID\":\"target\",\"Name\":[]}}]}}" })
        }, CancellationToken.None);
        Assert.False(result.IsSuccessful);
        Assert.Equal(ControlErrorCode.InvalidRequest, result.ErrorCode);
        Assert.Equal(original.Snapshot!.Revision, (kind == RepositoryKind.DisplayProfiles ? AgentProfileRepository.GetSnapshot() : AgentAudioRepository.GetSnapshot()).Revision);
    }

    public void Dispose()
    {
        foreach (var entry in _originalState) entry.Field.SetValue(null, entry.Value);
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    private sealed class FakeProfileOperations : UserProfileOperationService, IDisposable
    {
        public bool DisplayRestores { get; init; } = true;
        public bool AudioRestores { get; init; } = true;
        public bool BlockAudio { get; init; }
        public bool FailAudio { get; init; }
        public int RestoreCalls { get; private set; }
        public TaskCompletionSource<bool> AudioStarted { get; } = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        public ManualResetEventSlim ReleaseAudio { get; } = new ManualResetEventSlim();
        internal override UserProfileOperationState CaptureCurrentProfileState(bool captureDisplaySnapshot, bool captureAudioSnapshot)
            => new UserProfileOperationState("original-display", "original-audio")
            { DisplayProfileSnapshotJson = captureDisplaySnapshot ? "{}" : "", AudioProfileSnapshotJson = captureAudioSnapshot ? "{}" : "" };
        internal override Task<ApplyDisplayProfileOperationResult> ApplyDisplayProfileAsync(string profileId, CancellationToken cancellationToken, string? snapshotJson)
        {
            if (snapshotJson != null) RestoreCalls++;
            return Task.FromResult(new ApplyDisplayProfileOperationResult(snapshotJson == null || DisplayRestores, false));
        }
        internal override ApplyAudioProfileOperationResult ApplyAudioProfile(string profileId, int deviceWaitMilliseconds, string? snapshotJson)
        {
            if (snapshotJson != null) RestoreCalls++;
            if (BlockAudio) { AudioStarted.TrySetResult(true); if (!ReleaseAudio.Wait(TimeSpan.FromSeconds(15))) throw new TimeoutException("Test did not release audio operation."); }
            if (FailAudio) throw new InvalidOperationException("Simulated audio execution failure.");
            return new ApplyAudioProfileOperationResult(snapshotJson == null || AudioRestores, Array.Empty<string>());
        }
        public void Dispose() => ReleaseAudio.Dispose();
    }

    private sealed class FakeServiceClient : ControlServiceClient
    {
        public TaskCompletionSource<bool> IdleReported { get; } = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        public override Task<ControlResponse> ReportAgentOperationStateAsync(AgentRegistration registration, AgentOperationState operationState, CancellationToken cancellationToken)
        {
            if (operationState == AgentOperationState.Idle) IdleReported.TrySetResult(true);
            return Task.FromResult(new ControlResponse { IsSuccessful = true });
        }
    }

    private sealed class UnlockedSession : IInteractiveSessionStateProvider
    {
        public InteractiveSessionState GetState(int sessionId) => InteractiveSessionState.Unlocked;
    }

    private sealed class BlockingDownload : HttpMessageHandler
    {
        public TaskCompletionSource<bool> Started { get; } = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<HttpResponseMessage> Response { get; } = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        { Started.TrySetResult(true); return Response.Task.WaitAsync(cancellationToken); }
    }
}
