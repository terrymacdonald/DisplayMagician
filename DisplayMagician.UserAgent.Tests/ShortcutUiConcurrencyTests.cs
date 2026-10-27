using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;
using DisplayMagician.UIForms;
using Xunit;
using DesktopCache = DisplayMagician.DesktopProfileViewCache;

namespace DisplayMagician.UserAgent.Tests;

[Collection("Agent repository state")]
public sealed class ShortcutUiConcurrencyTests : IDisposable
{
    private readonly List<(FieldInfo Field, object? Value)> _original = new();

    public ShortcutUiConcurrencyTests()
    {
        foreach (Type type in new[] { typeof(global::DisplayMagician.ShortcutRepository), typeof(DesktopCache) })
            foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic).Where(field => !field.IsInitOnly && !field.IsLiteral))
                _original.Add((field, field.GetValue(null)));
        typeof(global::DisplayMagician.ShortcutRepository).GetField("_shortcutsLoaded", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, true);
        typeof(global::DisplayMagician.ShortcutRepository).GetField("_allShortcuts", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, new List<global::DisplayMagician.ShortcutItem>());
        typeof(DesktopCache).GetField("_views", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, Array.Empty<DisplayProfileView>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TrayCoalescesRefreshesAndDoesNotRestoreACompletedOperation(bool oldFails) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        int profileReads = 0;
        using MainForm form = new(client, _ => { profileReads++; return Task.FromResult(true); });
        Guid completed = Guid.NewGuid();
        Task initial = Call(form, "RefreshNotifyIconMenusAsync");
        form.RefreshNotifyIconMenus(completed);
        form.RefreshNotifyIconMenus();
        Assert.Single(client.Pending);
        if (oldFails) client.Pending[0].Completion.SetException(new IOException("Old lookup failed"));
        else client.Pending[0].Completion.SetResult(Session(completed));
        await Until(() => client.Pending.Count == 2);
        client.Pending[1].Completion.SetResult(Session(completed));
        await initial;
        Assert.Null(Field<WindowPlacementSessionView?>(form, "_cachedWindowPlacementSession"));
        Assert.Null(Field<ToolStripMenuItem?>(form, "_saveWindowPositionsMenuItem"));
        Assert.Equal(1, profileReads);
    });

    [Fact]
    public void TrayAppliesTheFollowUpSessionAndClosesWithoutLateUiUpdates() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        using MainForm form = new(client, _ => Task.FromResult(true));
        Task initial = Call(form, "RefreshNotifyIconMenusAsync");
        form.RefreshNotifyIconMenus();
        client.Pending[0].Completion.SetResult(Session(Guid.NewGuid()));
        await Until(() => client.Pending.Count == 2);
        Guid latest = Guid.NewGuid();
        client.Pending[1].Completion.SetResult(Session(latest));
        await initial;
        Assert.Equal(latest, Field<WindowPlacementSessionView>(form, "_cachedWindowPlacementSession").OperationId);
        Assert.NotNull(Field<ToolStripMenuItem?>(form, "_saveWindowPositionsMenuItem"));
        Task pending = Call(form, "RefreshNotifyIconMenusAsync");
        form.Dispose();
        Assert.True(client.Pending[2].Token.IsCancellationRequested);
        client.Pending[2].Completion.SetResult(Session(Guid.NewGuid()));
        await pending;
        Assert.Equal(latest, Field<WindowPlacementSessionView>(form, "_cachedWindowPlacementSession").OperationId);
    });

    [Fact]
    public void DesktopProfileCacheRejectsAnOlderConcurrentRead() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        Task<bool> old = DesktopCache.RefreshAsync(client, CancellationToken.None);
        Task<bool> current = DesktopCache.RefreshAsync(client, CancellationToken.None);
        client.Pending[1].Completion.SetResult(Profiles("new"));
        Assert.True(await current);
        client.Pending[0].Completion.SetResult(Profiles("old"));
        Assert.False(await old);
        Assert.Equal("new", Assert.Single(DesktopCache.SavedProfiles).Id);
    });

    [Theory]
    [InlineData("start", false)]
    [InlineData("start", true)]
    [InlineData("status", false)]
    [InlineData("status", true)]
    [InlineData("windows", false)]
    [InlineData("windows", true)]
    public void ClosingTheLibraryStopsPollingWithoutCancellingTheShortcut(string stage, bool fail) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        List<string> messages = new();
        using ShortcutLibraryForm form = new(client, (message, _, _) => messages.Add(message));
        Task run = Call(form, "RunShortcutAsync", new global::DisplayMagician.ShortcutItem { Name = "Original" });
        Guid id = Guid.NewGuid();
        int index = 0;
        if (stage != "start")
        {
            client.Pending[0].Completion.SetResult(Status(id));
            await Until(() => client.Pending.Count == 2);
            index = 1;
        }
        if (stage == "windows")
        {
            client.Pending[1].Completion.SetResult(Status(id));
            await Until(() => client.Pending.Count == 3);
            index = 2;
        }
        form.Dispose();
        Assert.True(client.Pending[index].Token.IsCancellationRequested);
        if (fail) client.Pending[index].Completion.SetException(new IOException("Disconnected"));
        else client.Pending[index].Completion.SetResult(stage == "windows" ? Session(id) : Status(id));
        await run;
        Assert.Empty(messages);
        Assert.DoesNotContain(client.Pending, request => request.Envelope.MessageType == ControlMessageType.CancelOperation);
    });

    [Fact]
    public void PlacementLookupFailureDoesNotStopShortcutMonitoringOrDereferenceADeletedSelection() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        List<string> messages = new();
        using ShortcutLibraryForm form = new(client, (message, _, _) => messages.Add(message));
        global::DisplayMagician.ShortcutItem shortcut = new() { Name = "Original" };
        Task run = Call(form, "RunShortcutAsync", shortcut);
        Task duplicate = Call(form, "RunShortcutAsync", shortcut);
        await duplicate;
        Assert.Single(client.Pending);
        Guid id = Guid.NewGuid();
        client.Pending[0].Completion.SetResult(Status(id));
        await Until(() => client.Pending.Count == 2);
        client.Pending[1].Completion.SetResult(Status(id));
        await Until(() => client.Pending.Count == 3);
        Set(form, "_selectedShortcut", null);
        client.Pending[2].Completion.SetException(new IOException("Window lookup failed"));
        await Until(() => client.Pending.Count == 4);
        Assert.Equal(ControlMessageType.GetOperationStatus, client.Pending[3].Envelope.MessageType);
        client.Pending[3].Completion.SetResult(Status(id, true));
        await run;
        Assert.Empty(messages);
        Assert.Equal(Guid.Empty, Field<Guid>(form, "_activeShortcutOperationId"));
        Assert.False(Field<Button>(form, "btn_cancel").Enabled);
        Assert.Equal(4, client.Pending.Count);
    });

    [Fact]
    public void CancellationKeepsItsOriginalIdentityAndRejectsDuplicateClicks() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        using ShortcutLibraryForm form = new(client, (_, _, _) => { });
        Guid original = Guid.NewGuid();
        Set(form, "_activeShortcutOperationId", original);
        Click(form, "btn_cancel_Click");
        Click(form, "btn_cancel_Click");
        Assert.Single(client.Pending);
        using JsonDocument payload = JsonDocument.Parse(client.Pending[0].Envelope.Payload);
        Assert.Equal(original, payload.RootElement.GetProperty("OperationId").GetGuid());
        Set(form, "_activeShortcutOperationId", Guid.NewGuid());
        client.Pending[0].Completion.SetResult(new ControlResponse { IsSuccessful = true });
        await Until(() => !Field<bool>(form, "_cancelInProgress"));
        Assert.False(Field<Button>(form, "btn_cancel").Enabled);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClosingDuringSnapshotSaveDoesNotRefreshDisposedControls(bool fail) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        TaskCompletionSource<bool> save = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int saves = 0;
        List<string> messages = new();
        using ShortcutLibraryForm form = new(client, (message, _, _) => messages.Add(message), (_, _) => { saves++; return save.Task; });
        Set(form, "_activeShortcutOperationId", Guid.NewGuid());
        Click(form, "btn_save_window_positions_Click");
        Click(form, "btn_save_window_positions_Click");
        Assert.Equal(1, saves);
        form.Dispose();
        if (fail) save.SetException(new IOException("Save failed")); else save.SetResult(true);
        await Until(() => !Field<bool>(form, "_placementActionInProgress"));
        Assert.Empty(client.Pending);
        Assert.Empty(messages);
    });

    [Fact]
    public void GameRefreshPreservesTheUnsavedDraftAndRejectsOverlappingRefreshes() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        using ShortcutForm form = new(client, (_, _, _) => { });
        Set(form, "_configurationLoaded", true);
        Set(form, "_loadedShortcut", true);
        Field<TextBox>(form, "txt_shortcut_save_name").Text = "Unsaved name";
        Field<TextBox>(form, "txt_args_game").Text = "Unsaved arguments";
        Set(form, "_isUnsaved", true);
        Click(form, "btn_refresh_games_list_Click");
        Click(form, "btn_refresh_games_list_Click");
        Assert.Single(client.Pending);
        client.Pending[0].Completion.SetResult(new ControlResponse { IsSuccessful = true, GameList = new GameListResult() });
        await Until(() => !Field<bool>(form, "_serviceActionInProgress"));
        Assert.Equal("Unsaved name", Field<TextBox>(form, "txt_shortcut_save_name").Text);
        Assert.Equal("Unsaved arguments", Field<TextBox>(form, "txt_args_game").Text);
        Assert.True(Field<bool>(form, "_isUnsaved"));
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OlderPreviewCannotReplaceANewerPreviewForTheSameProfile(bool oldFails) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        using ShortcutForm form = new(client, (_, _, _) => { });
        DisplayProfileView profile = new() { Id = "same", Name = "Same" };
        Set(form, "_profileToUse", profile);
        Task old = Call(form, "LoadDetailedProfilePreviewAsync", profile);
        Task current = Call(form, "LoadDetailedProfilePreviewAsync", profile);
        string freshPng = Png(Color.Red);
        client.Pending[1].Completion.SetResult(Profiles("same", freshPng));
        await current;
        Image? image = Field<PictureBox>(form, "dv_profile").Image;
        Assert.NotNull(image);
        if (oldFails) client.Pending[0].Completion.SetException(new IOException("Old failure"));
        else client.Pending[0].Completion.SetResult(Profiles("same", Png(Color.Blue)));
        await old;
        Assert.Equal(freshPng, profile.DetailedLayoutPngBase64);
        Assert.Same(image, Field<PictureBox>(form, "dv_profile").Image);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EditorInitialLoadFailureNeverEnablesSavingOrContinuesLoadingAfterClosure(bool close) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        List<string> messages = new();
        using ShortcutForm form = new(client, (message, _, _) => messages.Add(message));
        Click(form, "ShortcutForm_Load");
        if (close) form.Dispose();
        client.Pending[0].Completion.SetException(new IOException("Profiles unavailable"));
        await Until(() => close || messages.Count == 1);
        await Task.Yield();
        Assert.Single(client.Pending);
        Assert.False(Field<bool>(form, "_configurationLoaded"));
        Assert.Equal(close ? 0 : 1, messages.Count);
        if (!close) Assert.False(Field<Button>(form, "btn_save").Enabled);
    });

    [Theory]
    [InlineData("failed")]
    [InlineData("saved")]
    [InlineData("discarded")]
    public void SaveOnCloseKeepsAnUnsavedDraftWhenSavingFails(string outcome) => WinFormsTestRunner.Run(() =>
    {
        FakeClient client = new();
        int saves = 0;
        ShortcutForm? form = null;
        using (form = new ShortcutForm(client, (_, _, _) => { }, (_, _) => outcome == "discarded" ? DialogResult.No : DialogResult.Yes,
            () => { saves++; if (outcome == "saved") Set(form!, "_isUnsaved", false); }))
        {
            Set(form, "_loadedShortcut", true);
            Set(form, "_isUnsaved", true);
            FormClosingEventArgs args = new(CloseReason.UserClosing, false);
            form.GetType().GetMethod("OnFormClosing", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, new object[] { args });
            Assert.Equal(outcome == "failed", args.Cancel);
            Assert.Equal(outcome == "discarded" ? 0 : 1, saves);
            Assert.Equal(outcome != "failed", Field<CancellationToken>(form, "_lifetimeToken").IsCancellationRequested);
        }
        return Task.CompletedTask;
    });

    private static ControlResponse Status(Guid id, bool terminal = false) => new() { IsSuccessful = true, OperationStatus = new OperationStatus { OperationId = id, IsTerminal = terminal } };

    [Fact]
    public void TrayRequestsFromBackgroundThreadsDoNotCreateHandlesOrTouchControlsOffThread() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        using MainForm form = new(client, _ => Task.FromResult(true));
        await Task.Run(() => form.RefreshNotifyIconMenus());
        Assert.Empty(client.Pending);
        Assert.False(form.IsHandleCreated);
        int uiThread = Environment.CurrentManagedThreadId;
        _ = form.Handle;
        Assert.Single(client.Pending);
        client.Pending[0].Completion.SetResult(new ControlResponse { IsSuccessful = true });
        await Until(() => !Field<bool>(form, "_notifyMenuRefreshInProgress"));
        await Task.Run(() => form.RefreshNotifyIconMenus());
        await Until(() => client.Pending.Count == 2);
        Assert.Equal(uiThread, client.LastThreadId);
        client.Pending[1].Completion.SetResult(Session(Guid.NewGuid()));
        await Until(() => !Field<bool>(form, "_notifyMenuRefreshInProgress"));
    });

    [Fact]
    public void DeletedAudioProfileLeavesAnExplicitChoiceAndCannotBeSilentlyReplaced() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        using ShortcutForm form = new(client, (_, _, _) => { });
        ConfigureAudioDraft(form);
        Task initial = Call(form, "RefreshAudioProfilesAsync", "chosen");
        client.Pending[0].Completion.SetResult(Audio("chosen"));
        await initial;
        Set(form, "_loadedShortcut", true);
        Set(form, "_isUnsaved", false);
        Task refreshed = Call(form, "RefreshAudioProfilesAsync", (object)null!);
        client.Pending[1].Completion.SetResult(Audio("another"));
        await refreshed;
        Assert.Null(Field<AudioProfileView?>(form, "_audioProfileToUse"));
        Assert.Equal(-1, Field<ListBox>(form, "lb_audio_profiles").SelectedIndex);
        Assert.Single(Field<ListBox>(form, "lb_audio_profiles").Items.Cast<object>());
        Assert.False(Field<CheckBox>(form, "cb_dont_change_audio").Checked);
        Assert.False(Allowed(form));
        Field<CheckBox>(form, "cb_dont_change_audio").Checked = true;
        Assert.True(Allowed(form));
    });

    [Fact]
    public void RefreshingAnExistingAudioChoiceKeepsTheDraftCleanAndUsesTheNewView() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        using ShortcutForm form = new(client, (_, _, _) => { });
        ConfigureAudioDraft(form);
        Task initial = Call(form, "RefreshAudioProfilesAsync", "chosen");
        client.Pending[0].Completion.SetResult(Audio("chosen"));
        await initial;
        AudioProfileView old = Field<AudioProfileView>(form, "_audioProfileToUse");
        Set(form, "_loadedShortcut", true);
        Set(form, "_isUnsaved", false);
        Task current = Call(form, "RefreshAudioProfilesAsync", (object)null!);
        client.Pending[1].Completion.SetResult(Audio("chosen"));
        await current;
        Assert.NotSame(old, Field<AudioProfileView>(form, "_audioProfileToUse"));
        Assert.Equal("chosen", Field<AudioProfileView>(form, "_audioProfileToUse").Id);
        Assert.False(Field<bool>(form, "_isUnsaved"));
    });

    [Fact]
    public void SaveRevalidatesAudioBeforeCommittingTheDraft() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        List<string> messages = new();
        using ShortcutForm form = new(client, (message, _, _) => messages.Add(message));
        ConfigureAudioDraft(form);
        Set(form, "_audioProfileToUse", new AudioProfileView { Id = "deleted" });
        Set(form, "_isUnsaved", true);
        Click(form, "btn_save_Click");
        Assert.Equal(ControlMessageType.ListAudioProfiles, Assert.Single(client.Pending).Envelope.MessageType);
        client.Pending[0].Completion.SetResult(Audio("remaining"));
        await Until(() => !Field<bool>(form, "_serviceActionInProgress"));
        Assert.Single(messages);
        Assert.Contains("no longer available", messages[0]);
        Assert.True(Field<bool>(form, "_isUnsaved"));
        Assert.DoesNotContain(client.Pending, request => request.Envelope.MessageType == ControlMessageType.CommitRepositorySnapshot);
        Assert.Null(Field<AudioProfileView?>(form, "_audioProfileToUse"));
    });

    [Fact]
    public void AsynchronousSaveOnCloseKeepsTheWindowAndDraftWhenTheServiceFails() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        List<string> messages = new();
        using ShortcutForm form = new(client, (message, _, _) => messages.Add(message), (_, _) => DialogResult.Yes);
        ConfigureAudioDraft(form);
        Set(form, "_audioProfileToUse", new AudioProfileView { Id = "chosen" });
        Set(form, "_loadedShortcut", true);
        Set(form, "_isUnsaved", true);
        FormClosingEventArgs args = new(CloseReason.UserClosing, false);
        form.GetType().GetMethod("OnFormClosing", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, new object[] { args });
        Assert.True(args.Cancel);
        Assert.False(Field<CancellationToken>(form, "_lifetimeToken").IsCancellationRequested);
        client.Pending[0].Completion.SetException(new IOException("Audio data unavailable"));
        await Until(() => !Field<bool>(form, "_serviceActionInProgress"));
        Assert.True(Field<bool>(form, "_isUnsaved"));
        Assert.False(form.IsDisposed);
        Assert.Single(messages);
    });

    [Fact]
    public void OldAudioRefreshCannotRestoreADeletedSelection() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        using ShortcutForm form = new(client, (_, _, _) => { });
        Task old = Call(form, "RefreshAudioProfilesAsync", "old");
        Task current = Call(form, "RefreshAudioProfilesAsync", "new");
        client.Pending[1].Completion.SetResult(Audio("new"));
        await current;
        client.Pending[0].Completion.SetResult(Audio("old"));
        await old;
        Assert.Equal("new", Field<AudioProfileView>(form, "_audioProfileToUse").Id);
    });

    private static void ConfigureAudioDraft(ShortcutForm form)
    {
        Set(form, "_configurationLoaded", true);
        Set(form, "_autoName", false);
        Field<RadioButton>(form, "rb_no_game").Checked = true;
        Set(form, "_profileToUse", new DisplayProfileView { Id = global::DisplayMagician.ShortcutItem.SkipDisplayChangeUUID, Name = "Skip" });
        Field<TextBox>(form, "txt_shortcut_save_name").Text = "Draft";
        Field<CheckBox>(form, "cb_dont_change_audio").Checked = false;
    }
    private static bool Allowed(ShortcutForm form) => (bool)form.GetType().GetMethod("AllowedToSave", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, new object[] { false })!;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClearSnapshotIsSerializedAndIgnoresLateFailuresAfterClosure(bool close) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        List<string> messages = new();
        using ShortcutLibraryForm form = new(client, (message, _, _) => messages.Add(message), confirmClear: (_, _) => true);
        global::DisplayMagician.ShortcutItem shortcut = new() { Name = "Snapshot" };
        shortcut.WindowPlacements.Add(new DisplayMagician.ConfigurationDefinitions.ShortcutWindowPlacementDefinition { ProgramId = "main" });
        Set(form, "_selectedShortcut", shortcut);
        Click(form, "tsmi_clear_window_positions_Click");
        Click(form, "tsmi_clear_window_positions_Click");
        Assert.Single(client.Pending);
        if (close) form.Dispose();
        client.Pending[0].Completion.SetResult(new ControlResponse { IsSuccessful = false, Message = "Could not clear" });
        await Until(() => !Field<bool>(form, "_placementActionInProgress"));
        Assert.Equal(close ? 0 : 1, messages.Count);
    });

    [Fact]
    public void SavingPositionsDoesNotReselectTheShortcutChosenBeforeTheAwait() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        TaskCompletionSource<bool> save = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ShortcutLibraryForm form = new(client, (_, _, _) => { }, (_, _) => save.Task);
        global::DisplayMagician.ShortcutItem original = new() { Name = "Original" };
        global::DisplayMagician.ShortcutItem current = new() { Name = "Current" };
        global::DisplayMagician.ShortcutRepository.AllShortcuts.Add(original);
        global::DisplayMagician.ShortcutRepository.AllShortcuts.Add(current);
        Set(form, "_selectedShortcut", original);
        Set(form, "_activeShortcutOperationId", Guid.NewGuid());
        Click(form, "btn_save_window_positions_Click");
        Set(form, "_selectedShortcut", current);
        save.SetResult(true);
        await Until(() => !Field<bool>(form, "_placementActionInProgress"));
        Assert.Equal(current.UUID, Field<global::DisplayMagician.ShortcutItem>(form, "_selectedShortcut").UUID);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SplashCloseRequestsWorkBeforeAndAfterHandleCreation(bool createHandle) => WinFormsTestRunner.Run(async () =>
    {
        using ShortcutLoadingForm splash = new();
        if (createHandle) _ = splash.Handle;
        await Task.Run(() => splash.RequestClose());
        if (!createHandle)
        {
            Assert.False(splash.IsHandleCreated);
            typeof(ShortcutLoadingForm).GetMethod("OnShown", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(splash, new object[] { EventArgs.Empty });
        }
        await Until(() => splash.IsDisposed);
        splash.RequestClose();
    });

    [Fact]
    public void ShortcutDeletedBeforeEditorStartupCannotEnableSaving() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        List<string> messages = new();
        using ShortcutForm form = new(client, (message, _, _) => messages.Add(message));
        Click(form, "ShortcutForm_Load");
        client.Pending[0].Completion.SetResult(Profiles("display"));
        await Until(() => client.Pending.Count == 2);
        client.Pending[1].Completion.SetResult(new ControlResponse { IsSuccessful = true, AudioProfileList = new AudioProfileListResult { CanAccessAudioSettings = true } });
        await Until(() => client.Pending.Count == 3);
        client.Pending[2].Completion.SetResult(new ControlResponse { IsSuccessful = true, RepositorySnapshot = new RepositorySnapshot { Repository = RepositoryKind.Shortcuts, Json = "{\"Shortcuts\":[]}" } });
        await Until(() => client.Pending.Count == 4);
        client.Pending[3].Completion.SetResult(new ControlResponse { IsSuccessful = true, GameList = new GameListResult() });
        await Until(() => form.IsDisposed);
        Assert.False(Field<bool>(form, "_configurationLoaded"));
        Assert.Contains("no longer available", Assert.Single(messages));
    });
    private static ControlResponse Audio(string id) => new() { IsSuccessful = true, AudioProfileList = new AudioProfileListResult { CanAccessAudioSettings = true, SavedProfiles = new[] { new AudioProfileView { Id = id, Name = id } } } };
    private static ControlResponse Session(Guid id) => new() { IsSuccessful = true, WindowPlacementSession = new WindowPlacementSessionView { OperationId = id, CanSave = true } };
    private static ControlResponse Profiles(string id, string? png = null) => new() { IsSuccessful = true, ProfileList = new ProfileListResult { SavedProfiles = new[] { new DisplayProfileView { Id = id, Name = id, DetailedLayoutPngBase64 = png } } } };
    private static string Png(Color color)
    {
        using Bitmap bitmap = new(2, 2);
        bitmap.SetPixel(0, 0, color);
        using MemoryStream stream = new(); bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        return Convert.ToBase64String(stream.ToArray());
    }
    private static T Field<T>(Form form, string name) => (T)form.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
    private static void Set(Form form, string name, object? value) => form.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(form, value);
    private static Task Call(Form form, string method, params object[] args) => (Task)form.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, args)!;
    private static void Click(Form form, string method) => form.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, new object[] { form, EventArgs.Empty });
    private static async Task Until(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition()) { Assert.True(DateTime.UtcNow < deadline, "The shortcut UI action did not complete."); await Task.Delay(10); }
    }
    public void Dispose() { foreach (var entry in _original) entry.Field.SetValue(null, entry.Value); }
    private sealed class FakeClient : global::DisplayMagician.ControlServicePipeClient
    {
        public int LastThreadId { get; private set; }
        public List<(ControlEnvelope Envelope, CancellationToken Token, TaskCompletionSource<ControlResponse> Completion)> Pending { get; } = new();
        protected override Task<ControlResponse> SendAsync(ControlEnvelope request, CancellationToken cancellationToken)
        {
            LastThreadId = Environment.CurrentManagedThreadId;
            TaskCompletionSource<ControlResponse> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Pending.Add((request, cancellationToken, completion)); return completion.Task;
        }
    }
}
