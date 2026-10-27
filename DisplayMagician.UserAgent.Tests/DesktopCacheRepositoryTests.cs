using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;
using DisplayMagician.UIForms;
using Newtonsoft.Json;
using Xunit;
using DesktopCache = DisplayMagician.DesktopProfileViewCache;
using DesktopShortcut = DisplayMagician.ShortcutItem;
using DesktopRepository = DisplayMagician.ShortcutRepository;

namespace DisplayMagician.UserAgent.Tests;

[Collection("Agent repository state")]
public sealed class DesktopCacheRepositoryTests : IDisposable
{
    private readonly List<(FieldInfo Field, object? Value)> _original = new();
    private readonly string _previousIconPath = global::DisplayMagician.Program.AppIconPath;
    private readonly string _iconRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianDesktopCacheTests", Guid.NewGuid().ToString("N"));
    private readonly NLog.Config.LoggingConfiguration? _originalLogging = NLog.LogManager.Configuration;
    private readonly NLog.Targets.MemoryTarget _messages = new() { Layout = "${message} ${exception:format=tostring}" };

    public DesktopCacheRepositoryTests()
    {
        global::DisplayMagician.Program.AppIconPath = _iconRoot;
        NLog.Config.LoggingConfiguration logging = new();
        logging.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, _messages);
        NLog.LogManager.Configuration = logging;
        foreach (Type type in new[] { typeof(DesktopRepository), typeof(DesktopCache) })
            foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic).Where(field => !field.IsInitOnly && !field.IsLiteral))
                _original.Add((field, field.GetValue(null)));
        typeof(DesktopCache).GetField("_views", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, Array.Empty<DisplayProfileView>());
    }

    [Theory]
    [InlineData("{")]
    [InlineData("{\"Shortcuts\":[null]}")]
    [InlineData("{}")]
    public void UnreadableRefreshPreservesTheUsableSnapshotAndRevision(string json)
    {
        Connection connection = new();
        DesktopRepository.ConnectToUserAgent(connection);
        string id = Assert.Single(DesktopRepository.AllShortcuts).UUID;
        connection.Snapshot = Snapshot(90, json);

        Assert.Throws<InvalidDataException>(() => DesktopRepository.LoadShortcuts());
        Assert.Equal(id, Assert.Single(DesktopRepository.AllShortcuts).UUID);
        connection.Commit = request =>
        {
            Assert.Equal(10, request.ExpectedRevision);
            return new RepositoryCommitResult { Snapshot = Snapshot(11, request.Json) };
        };
        Assert.True(DesktopRepository.SaveShortcuts(), string.Join(Environment.NewLine, _messages.Logs));
    }

    [Fact]
    public void FailedReconnectRetainsThePreviousConnection()
    {
        Connection original = new();
        DesktopRepository.ConnectToUserAgent(original);
        Connection failed = new() { Snapshot = Snapshot(90, "{") };
        Assert.Throws<InvalidDataException>(() => DesktopRepository.ConnectToUserAgent(failed));

        Assert.True(DesktopRepository.SaveShortcuts());
        Assert.Equal(1, original.Commits);
        Assert.Equal(0, failed.Commits);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidCommitReplyDoesNotPoisonTheRollbackSnapshotOrRevision(bool wrongRepository)
    {
        Connection connection = new();
        DesktopRepository.ConnectToUserAgent(connection);
        connection.Commit = _ => new RepositoryCommitResult
        {
            Snapshot = new RepositorySnapshot { Repository = wrongRepository ? RepositoryKind.AudioProfiles : RepositoryKind.Shortcuts, Revision = 90, Json = "{" }
        };
        DesktopShortcut draft = Assert.Single(DesktopRepository.AllShortcuts).CreateEditingCopy();
        draft.Name = "Unsaved";

        Assert.False(DesktopRepository.UpdateShortcut(draft));
        Assert.Equal("Original", Assert.Single(DesktopRepository.AllShortcuts).Name);
        Assert.Equal("Unsaved", draft.Name);
        connection.Commit = request =>
        {
            Assert.Equal(10, request.ExpectedRevision);
            return new RepositoryCommitResult { Snapshot = Snapshot(11, request.Json) };
        };
        Assert.True(DesktopRepository.SaveShortcuts());
    }

    [Fact]
    public void CommittedSnapshotIsAuthoritativeAndDetachedFromTheEditorDraft()
    {
        Connection connection = new();
        DesktopRepository.ConnectToUserAgent(connection);
        DesktopShortcut draft = Assert.Single(DesktopRepository.AllShortcuts).CreateEditingCopy();
        draft.Name = "Submitted";
        connection.Commit = _ => new RepositoryCommitResult { Snapshot = Snapshot(11, Json(draft.UUID, "Authoritative")) };

        Assert.True(DesktopRepository.UpdateShortcut(draft));
        draft.Name = "Later unsaved edit";
        Assert.Equal("Authoritative", Assert.Single(DesktopRepository.AllShortcuts).Name);
    }

    [Fact]
    public void ShortcutListMembershipCannotBeChangedByAReader()
    {
        DesktopRepository.ConnectToUserAgent(new Connection());
        List<DesktopShortcut> view = DesktopRepository.AllShortcuts;
        using IEnumerator<DesktopShortcut> reader = view.GetEnumerator();
        Assert.True(reader.MoveNext());
        DesktopRepository.AllShortcuts.Clear();
        Assert.Equal(1, DesktopRepository.ShortcutCount);
        Assert.False(reader.MoveNext());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EditedShortcutDetectsChangesAndDeletionAfterAnotherRefresh(bool deleted)
    {
        Connection connection = new();
        DesktopRepository.ConnectToUserAgent(connection);
        DesktopShortcut original = Assert.Single(DesktopRepository.AllShortcuts).CreateEditingCopy();
        DesktopShortcut draft = original.CreateEditingCopy();
        draft.Name = "My draft";
        connection.Snapshot = Snapshot(20, deleted ? "{\"Shortcuts\":[]}" : Json(original.UUID, "Other device"));
        Assert.True(DesktopRepository.LoadShortcuts());

        Assert.False(DesktopRepository.UpdateShortcut(draft, original, out bool changed));
        Assert.True(changed);
        Assert.Equal(0, connection.Commits);
        Assert.Equal("My draft", draft.Name);
        if (deleted) Assert.Empty(DesktopRepository.AllShortcuts);
        else Assert.Equal("Other device", Assert.Single(DesktopRepository.AllShortcuts).Name);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnrelatedShortcutConflictRebasesOnceWithoutLosingTheOtherChange(bool sameShortcutChanged)
    {
        Connection connection = new();
        DesktopRepository.ConnectToUserAgent(connection);
        DesktopShortcut original = Assert.Single(DesktopRepository.AllShortcuts).CreateEditingCopy();
        DesktopShortcut draft = original.CreateEditingCopy();
        draft.Name = "My draft";
        DesktopShortcut other = original.CreateEditingCopy();
        other.UUID = Guid.NewGuid().ToString("D");
        other.Name = "Other shortcut";
        DesktopShortcut current = original.CreateEditingCopy();
        if (sameShortcutChanged) current.Name = "Other device";
        string remote = JsonConvert.SerializeObject(new { Shortcuts = new[] { current, other } });
        connection.Commit = request => connection.Commits == 1
            ? new RepositoryCommitResult { WasConflict = true, Snapshot = Snapshot(20, remote) }
            : new RepositoryCommitResult { Snapshot = Snapshot(21, request.Json) };

        Assert.Equal(!sameShortcutChanged, DesktopRepository.UpdateShortcut(draft, original, out bool changed));
        Assert.Equal(sameShortcutChanged, changed);
        Assert.Equal(sameShortcutChanged ? 1 : 2, connection.Commits);
        Assert.Equal(2, DesktopRepository.ShortcutCount);
        Assert.Equal("Other shortcut", DesktopRepository.GetShortcut(other.UUID).Name);
        Assert.Equal(sameShortcutChanged ? "Other device" : "My draft", DesktopRepository.GetShortcut(original.UUID).Name);
    }

    [Fact]
    public void RepeatedUnrelatedConflictsStopAfterOneRetryAndLogExhaustion()
    {
        Connection connection = new();
        DesktopRepository.ConnectToUserAgent(connection);
        DesktopShortcut original = Assert.Single(DesktopRepository.AllShortcuts).CreateEditingCopy();
        DesktopShortcut draft = original.CreateEditingCopy();
        draft.Name = "My draft";
        connection.Commit = request => new RepositoryCommitResult { WasConflict = true, Snapshot = Snapshot(request.ExpectedRevision + 1, connection.Snapshot.Json) };
        Assert.False(DesktopRepository.UpdateShortcut(draft, original, out bool changed));
        Assert.False(changed);
        Assert.Equal(2, connection.Commits);
        Assert.Contains(_messages.Logs, message => message.Contains("Stopped after two conflicting commit attempts"));
        Assert.Equal("Original", Assert.Single(DesktopRepository.AllShortcuts).Name);
        Assert.Equal("My draft", draft.Name);
    }

    [Fact]
    public void NewWindowCaptureDoesNotConflictWithAnEditAndIsPreserved()
    {
        Connection connection = new();
        DesktopRepository.ConnectToUserAgent(connection);
        DesktopShortcut original = Assert.Single(DesktopRepository.AllShortcuts).CreateEditingCopy();
        DesktopShortcut draft = original.CreateEditingCopy();
        draft.Name = "My draft";
        DesktopShortcut captured = original.CreateEditingCopy();
        captured.WindowPlacements.Add(new() { ProgramId = "game", Left = 123 });
        connection.Snapshot = Snapshot(20, JsonConvert.SerializeObject(new { Shortcuts = new[] { captured } }));
        Assert.True(DesktopRepository.LoadShortcuts());
        Assert.True(DesktopRepository.UpdateShortcut(draft, original, out bool changed));
        Assert.False(changed);
        Assert.Equal(123, Assert.Single(DesktopRepository.GetShortcut(original.UUID).WindowPlacements).Left);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConflictedEditorCanCancelOrSaveItsDraftAsANewShortcut(bool saveNew) => WinFormsTestRunner.Run(async () =>
    {
        Connection connection = new();
        DesktopRepository.ConnectToUserAgent(connection);
        DesktopShortcut original = Assert.Single(DesktopRepository.AllShortcuts);
        List<string> errors = new();
        int prompts = 0;
        using ShortcutForm form = new(new ProfileClient(new()), (message, _, _) => errors.Add(message), resolveShortcutConflict: () =>
        {
            prompts++;
            return saveNew ? DialogResult.No : DialogResult.Cancel;
        });
        form.EditingExistingShortcut = true;
        form.Shortcut = original;
        form.Shortcut.Name = "My draft";
        form.Shortcut.StartPrograms.Add(new global::DisplayMagician.StartProgram { Id = "helper", Executable = "helper.exe" });
        form.Shortcut.WindowPlacements.Add(new() { ProgramId = "helper", Left = 123 });
        connection.Snapshot = Snapshot(20, Json(original.UUID, "Other device"));
        DesktopRepository.LoadShortcuts();

        Task<bool> save = (Task<bool>)typeof(ShortcutForm).GetMethod("SaveDraftAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, null)!;
        Assert.Equal(saveNew, await save);
        Assert.Equal(1, prompts);
        Assert.Empty(errors);
        Assert.Equal("Other device", DesktopRepository.GetShortcut(original.UUID).Name);
        Assert.Equal(saveNew ? 2 : 1, DesktopRepository.ShortcutCount);
        Assert.Equal(!saveNew, form.EditingExistingShortcut);
        if (saveNew)
        {
            Assert.NotEqual(original.UUID, form.Shortcut.UUID);
            Assert.Equal("My draft (Copy)", form.Shortcut.Name);
            Assert.Contains(form.Shortcut.UUID, form.Shortcut.SavedShortcutIconCacheFilename);
            DesktopShortcut copy = DesktopRepository.GetShortcut(form.Shortcut.UUID);
            Assert.Equal("helper", Assert.Single(copy.StartPrograms).Id);
            Assert.Equal("helper", Assert.Single(copy.WindowPlacements).ProgramId);
            Assert.Equal(123, Assert.Single(copy.WindowPlacements).Left);
        }
        else Assert.Equal("My draft", form.Shortcut.Name);
    });

    [Fact]
    public void ConflictedEditorReopensTheLatestVersionWithoutSavingOrClosing() => WinFormsTestRunner.Run(async () =>
    {
        Connection connection = new();
        DesktopRepository.ConnectToUserAgent(connection);
        DesktopShortcut original = Assert.Single(DesktopRepository.AllShortcuts);
        using ShortcutForm form = new(new ProfileClient(new()), (_, _, _) => { }, resolveShortcutConflict: () => DialogResult.Yes);
        form.EditingExistingShortcut = true;
        form.Shortcut = original;
        form.Shortcut.Name = "My draft";
        connection.Snapshot = Snapshot(20, Json(original.UUID, "Other device"));
        DesktopRepository.LoadShortcuts();

        Assert.False(await (Task<bool>)typeof(ShortcutForm).GetMethod("SaveDraftAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, null)!);
        Assert.Equal("Other device", form.Shortcut.Name);
        Assert.Equal(original.UUID, form.Shortcut.UUID);
        Assert.Equal(0, connection.Commits);
        Assert.False(form.IsDisposed);
        Assert.Equal(DialogResult.None, form.DialogResult);
        Assert.False((bool)typeof(ShortcutForm).GetField("_isUnsaved", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!);
    });

    [Fact]
    public void FailedReopenKeepsTheDraftAvailable() => WinFormsTestRunner.Run(async () =>
    {
        Connection connection = new();
        DesktopRepository.ConnectToUserAgent(connection);
        DesktopShortcut original = Assert.Single(DesktopRepository.AllShortcuts);
        using ShortcutForm form = new(new ProfileClient(new()), (_, _, _) => { }, resolveShortcutConflict: () => DialogResult.Yes);
        form.EditingExistingShortcut = true;
        form.Shortcut = original;
        form.Shortcut.Name = "My draft";
        connection.Snapshot = Snapshot(20, Json(original.UUID, "Other device"));
        DesktopRepository.LoadShortcuts();
        connection.Read = () => throw new IOException("Unavailable snapshot");

        await Assert.ThrowsAsync<IOException>(async () => await (Task<bool>)typeof(ShortcutForm).GetMethod("SaveDraftAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, null)!);
        Assert.Equal("My draft", form.Shortcut.Name);
        Assert.Equal(original.UUID, form.Shortcut.UUID);
        Assert.Equal(0, connection.Commits);
    });

    [Fact]
    public async Task RefreshAndCommitKeepTheMatchingSnapshotRevisionTogether()
    {
        Connection connection = new();
        DesktopRepository.ConnectToUserAgent(connection);
        using ManualResetEventSlim readEntered = new();
        using ManualResetEventSlim releaseRead = new();
        using ManualResetEventSlim saveStarted = new();
        using ManualResetEventSlim commitEntered = new();
        connection.Read = () =>
        {
            readEntered.Set();
            if (!releaseRead.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            return Snapshot(20, Json(Guid.NewGuid().ToString("D"), "Refreshed"));
        };
        connection.Commit = request =>
        {
            commitEntered.Set();
            Assert.Equal(20, request.ExpectedRevision);
            Assert.Contains("Refreshed", request.Json);
            return new RepositoryCommitResult { Snapshot = Snapshot(21, request.Json) };
        };
        Task<bool> refresh = Task.Run(DesktopRepository.LoadShortcuts);
        Task<bool>? save = null;
        try
        {
            Assert.True(readEntered.Wait(TimeSpan.FromSeconds(10)));
            save = Task.Run(() => { saveStarted.Set(); return DesktopRepository.SaveShortcuts(); });
            Assert.True(saveStarted.Wait(TimeSpan.FromSeconds(10)));
            Assert.False(commitEntered.Wait(TimeSpan.FromMilliseconds(200)));
        }
        finally
        {
            releaseRead.Set();
            await refresh;
            if (save != null) await save;
        }
        Assert.True(await refresh);
        Assert.True(await save!);
        Assert.Equal("Refreshed", Assert.Single(DesktopRepository.AllShortcuts).Name);
    }

    [Fact]
    public async Task ProfileViewsAndNestedSettingsAreIsolatedFromBothResponseAndReaders()
    {
        DisplayProfileView profile = new() { Id = "profile", Name = "Saved", Settings = new() { ApplyProfileCount = 3 } };
        ProfileListResult response = new() { SavedProfiles = new[] { profile }, CurrentLayout = profile };
        Assert.True(await DesktopCache.RefreshAsync(new ProfileClient(response), CancellationToken.None));
        profile.Name = "Response changed";
        profile.Settings.ApplyProfileCount = 90;
        response.SavedProfiles[0] = new DisplayProfileView { Id = "replacement" };

        DisplayProfileView saved = Assert.Single(DesktopCache.SavedProfiles);
        Assert.Equal("Saved", saved.Name);
        Assert.Equal(3, saved.Settings.ApplyProfileCount);
        saved.Name = "Reader changed";
        saved.Settings.ApplyProfileCount = 91;
        DesktopCache.Get("profile").Name = "Lookup changed";
        DesktopCache.CurrentLayout.Settings.ApplyProfileCount = 92;
        Assert.Equal("Saved", Assert.Single(DesktopCache.SavedProfiles).Name);
        Assert.Equal(3, DesktopCache.Get("profile").Settings.ApplyProfileCount);
        Assert.Equal(3, DesktopCache.CurrentLayout.Settings.ApplyProfileCount);
    }

    [Fact]
    public async Task CancelledProfileRefreshCannotPublishASuccessfulLateReply()
    {
        Assert.True(await DesktopCache.RefreshAsync(new ProfileClient(new ProfileListResult { SavedProfiles = new[] { new DisplayProfileView { Id = "saved" } } }), CancellationToken.None));
        using CancellationTokenSource lifetime = new();
        ProfileClient client = new(new ProfileListResult { SavedProfiles = new[] { new DisplayProfileView { Id = "late" } } }, lifetime.Cancel);
        Assert.False(await DesktopCache.RefreshAsync(client, lifetime.Token));
        Assert.Equal("saved", Assert.Single(DesktopCache.SavedProfiles).Id);
    }

    public void Dispose()
    {
        foreach ((FieldInfo field, object? value) in _original) field.SetValue(null, value);
        global::DisplayMagician.Program.AppIconPath = _previousIconPath;
        NLog.LogManager.Configuration = _originalLogging;
        if (Directory.Exists(_iconRoot)) Directory.Delete(_iconRoot, true);
    }

    private static string Json(string id, string name) => JsonConvert.SerializeObject(new
    {
        Shortcuts = new[] { new { UUID = id, Name = name, AutoName = false, Category = ShortcutCategory.NoGame,
            ProfileUUID = DesktopShortcut.SkipDisplayChangeUUID, AudioProfileUUID = DesktopShortcut.SkipAudioProfilesChangeUUID,
            SelectedImage = new global::DisplayMagician.ShortcutBitmap() } }
    });

    private static RepositorySnapshot Snapshot(long revision, string json) => new() { Repository = RepositoryKind.Shortcuts, Revision = revision, Json = json };

    private sealed class Connection : IUserAgentRepositoryConnection
    {
        public RepositorySnapshot Snapshot { get; set; } = DesktopCacheRepositoryTests.Snapshot(10, Json(Guid.NewGuid().ToString("D"), "Original"));
        public Func<RepositorySnapshot>? Read { get; set; }
        public Func<RepositoryCommitRequest, RepositoryCommitResult>? Commit { get; set; }
        public int Commits { get; private set; }
        public RepositorySnapshot GetRepositorySnapshot(RepositoryKind repository) => Read?.Invoke() ?? Snapshot;
        public RepositoryCommitResult CommitRepositorySnapshot(RepositoryCommitRequest request)
        {
            Commits++;
            return Commit?.Invoke(request) ?? new RepositoryCommitResult { Snapshot = DesktopCacheRepositoryTests.Snapshot(request.ExpectedRevision + 1, request.Json) };
        }
    }

    private sealed class ProfileClient : global::DisplayMagician.ControlServicePipeClient
    {
        private readonly ProfileListResult _response;
        private readonly Action? _beforeReply;
        public ProfileClient(ProfileListResult response, Action? beforeReply = null) { _response = response; _beforeReply = beforeReply; }
        protected override Task<ControlResponse> SendAsync(ControlEnvelope request, CancellationToken cancellationToken)
        {
            _beforeReply?.Invoke();
            return Task.FromResult(new ControlResponse { IsSuccessful = true, ProfileList = _response, AudioProfileList = new AudioProfileListResult() });
        }
    }
}
