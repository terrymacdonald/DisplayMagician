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
using DisplayMagician.GameLibraries;
using DisplayMagician.UIForms;
using DisplayMagician.UserAgent.Runtime;
using Manina.Windows.Forms;
using Vortice.DirectInput;
using Xunit;
using Desktop = global::DisplayMagician;

namespace DisplayMagician.UserAgent.Tests;

[Collection("Agent repository state")]
public sealed class CatalogueReviewTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagicianCatalogueTests", Guid.NewGuid().ToString("N"));
    private readonly List<Game> _previousGames = GameLibrary.AllInstalledGamesInAllLibraries;
    private readonly bool _previousLoaded = GameLibrary.GamesLoaded;
    private readonly Desktop.ProgramSettings _previousSettings = Desktop.Program.AppProgramSettings;
    private readonly List<(FieldInfo Field, object? Value)> _state = new();

    public CatalogueReviewTests()
    {
        Directory.CreateDirectory(_root);
        foreach (Type type in new[] { typeof(ProfileRepository), typeof(AudioProfileRepository), typeof(Desktop.ProgramSettings) })
            foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(field => !field.IsInitOnly && !field.IsLiteral))
                _state.Add((field, field.GetValue(null)));
        Desktop.ProgramSettings.ConfigureStoragePath(_root);
        Desktop.Program.AppProgramSettings = new Desktop.ProgramSettings();
        GameLibrary.AllInstalledGamesInAllLibraries = new List<Game>();
        GameLibrary.GamesLoaded = true;
    }

    [Fact]
    public void GameLookupUsesTheLibraryAndNeverFallsBackToAnotherLibrary()
    {
        FakeGame steam = new("shared", SupportedGameLibraryType.Steam);
        FakeGame epic = new("shared", SupportedGameLibraryType.Epic);
        GameLibrary.AllInstalledGamesInAllLibraries = new() { steam, epic };
        Assert.Same(epic, GameLibrary.GetAnyGameById("shared", SupportedGameLibraryType.Epic));
        Assert.Same(steam, GameLibrary.GetAnyGameById("shared", SupportedGameLibraryType.Steam));
        Assert.Null(GameLibrary.GetAnyGameById("shared", SupportedGameLibraryType.GOG));
    }

    [Fact]
    public async Task ShortcutRunnerResolvesTheSavedLibraryBeforeValidatingItsMonitorTarget()
    {
        FakeGame otherLibrary = new("shared", SupportedGameLibraryType.Steam);
        FakeGame expected = new("shared", SupportedGameLibraryType.Epic) { Owner = new FakeLibrary(SupportedGameLibraryType.Epic) };
        GameLibrary.AllInstalledGamesInAllLibraries = new() { otherLibrary, expected };
        ShortcutStore store = new(_root);
        store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = store.GetSnapshot().Revision,
            Json = "{\"Shortcuts\":[{\"UUID\":\"catalogue-runner\",\"Name\":\"Test\",\"Category\":1,\"GameLibrary\":4,\"GameAppId\":\"shared\"}]}" });
        ShortcutRunner runner = new(store, new AutomaticGameDetectionRegistry(), new UserProfileOperationService(), new ShortcutRecoveryStore(_root));
        ShortcutRunResult result = await runner.ApplyShortcutProfilesAsync("catalogue-runner", 0, CancellationToken.None);
        Assert.Equal(ShortcutRunOutcome.Failed, result.Outcome);
        Assert.Contains("Monitor different executable", result.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedScanRetainsTheCombinedAndProviderCatalogues(bool throws)
    {
        FakeGame saved = new("saved", SupportedGameLibraryType.Steam);
        FakeLibrary library = new(SupportedGameLibraryType.Steam);
        library.AllInstalledGames.Add(saved);
        GameLibrary.AllInstalledGamesInAllLibraries = new() { saved };
        library.Scan = () =>
        {
            library.AllInstalledGames.Add(new FakeGame("partial", SupportedGameLibraryType.Steam));
            if (throws) throw new IOException("Scan failed");
            return false;
        };
        Assert.False(GameLibrary.LoadGamesInBackground(new[] { library }));
        Assert.Same(saved, Assert.Single(GameLibrary.AllInstalledGamesInAllLibraries));
        Assert.Same(saved, Assert.Single(library.AllInstalledGames));
        Assert.True(GameLibrary.GamesLoaded);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FirstScanKeepsSuccessfulLibrariesAndExcludesFailedPartialResults(bool throws)
    {
        GameLibrary.GamesLoaded = false;
        FakeGame good = new("good", SupportedGameLibraryType.Steam);
        FakeLibrary successful = new(SupportedGameLibraryType.Steam);
        successful.Scan = () => { successful.AllInstalledGames.Add(good); return true; };
        FakeLibrary failed = new(SupportedGameLibraryType.Epic);
        failed.Scan = () =>
        {
            failed.AllInstalledGames.Add(new FakeGame("partial", SupportedGameLibraryType.Epic));
            if (throws) throw new IOException("Initial scan failed");
            return false;
        };
        Assert.True(GameLibrary.LoadGamesInBackground(new[] { successful, failed }));
        Assert.True(GameLibrary.GamesLoaded);
        Assert.Same(good, Assert.Single(GameLibrary.AllInstalledGamesInAllLibraries));
        Assert.Empty(failed.AllInstalledGames);
    }

    [Fact]
    public async Task ReadersKeepTheOldCompleteCatalogueUntilTheScanFinishes()
    {
        FakeGame saved = new("saved", SupportedGameLibraryType.Steam);
        FakeGame fresh = new("fresh", SupportedGameLibraryType.Steam);
        GameLibrary.AllInstalledGamesInAllLibraries = new() { saved };
        using ManualResetEventSlim scanning = new();
        using ManualResetEventSlim release = new();
        FakeLibrary library = new(SupportedGameLibraryType.Steam);
        library.Scan = () =>
        {
            library.AllInstalledGames.Add(fresh);
            scanning.Set();
            if (!release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            return true;
        };
        Task<bool> scan = Task.Run(() => GameLibrary.LoadGamesInBackground(new[] { library }));
        try
        {
            Assert.True(scanning.Wait(TimeSpan.FromSeconds(10)));
            Assert.Same(saved, Assert.Single(GameLibrary.AllInstalledGamesInAllLibraries));
        }
        finally { release.Set(); await scan; }
        Assert.True(await scan);
        Assert.Same(fresh, Assert.Single(GameLibrary.AllInstalledGamesInAllLibraries));
    }

    [Fact]
    public async Task ConcurrentScansDoNotClearEachOthersProviderLists()
    {
        using ManualResetEventSlim firstEntered = new();
        using ManualResetEventSlim releaseFirst = new();
        using ManualResetEventSlim secondStarted = new();
        using ManualResetEventSlim secondEntered = new();
        int scans = 0;
        FakeLibrary library = new(SupportedGameLibraryType.Steam);
        library.Scan = () =>
        {
            int number = Interlocked.Increment(ref scans);
            if (number == 1)
            {
                firstEntered.Set();
                if (!releaseFirst.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            }
            else secondEntered.Set();
            library.AllInstalledGames.Add(new FakeGame(number.ToString(), SupportedGameLibraryType.Steam));
            return true;
        };
        Task<bool> first = Task.Run(() => GameLibrary.LoadGamesInBackground(new[] { library }));
        Task<bool>? second = null;
        try
        {
            Assert.True(firstEntered.Wait(TimeSpan.FromSeconds(10)));
            second = Task.Run(() => { secondStarted.Set(); return GameLibrary.LoadGamesInBackground(new[] { library }); });
            Assert.True(secondStarted.Wait(TimeSpan.FromSeconds(10)));
            Assert.False(secondEntered.Wait(TimeSpan.FromMilliseconds(200)));
        }
        finally { releaseFirst.Set(); await first; if (second != null) await second; }
        Assert.True(await first);
        Assert.True(await second!);
        Assert.Equal("2", Assert.Single(GameLibrary.AllInstalledGamesInAllLibraries).Id);
        Assert.Equal("2", Assert.Single(library.AllInstalledGames).Id);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task GameListReadsOnlyRescanWhenRequestedOrUnloaded(bool refresh, bool loaded)
    {
        GameLibrary.GamesLoaded = loaded;
        int scans = 0;
        ProfileCommandHandler handler = Handler(() => { scans++; return true; });
        ControlResponse result = await handler.HandleAsync(new ControlEnvelope
        { MessageType = ControlMessageType.ListGames, Payload = JsonSerializer.Serialize(new GameListRequest { Refresh = refresh }) }, CancellationToken.None);
        Assert.True(result.IsSuccessful);
        Assert.Equal(refresh || !loaded ? 1 : 0, scans);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    public async Task LegacyGameListRequestsRemainCachedReads(string payload)
    {
        int scans = 0;
        ControlResponse result = await Handler(() => { scans++; return true; }).HandleAsync(new ControlEnvelope
        { MessageType = ControlMessageType.ListGames, Payload = payload }, CancellationToken.None);
        Assert.True(result.IsSuccessful);
        Assert.Equal(0, scans);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{")]
    [InlineData("{\"Refresh\":\"yes\"}")]
    public async Task MalformedGameListRequestsDoNotStartAScan(string payload)
    {
        int scans = 0;
        ControlResponse result = await Handler(() => { scans++; return true; }).HandleAsync(new ControlEnvelope
        { MessageType = ControlMessageType.ListGames, Payload = payload }, CancellationToken.None);
        Assert.False(result.IsSuccessful);
        Assert.Equal(ControlErrorCode.InvalidRequest, result.ErrorCode);
        Assert.Equal(0, scans);
    }

    [Fact]
    public async Task FailedRefreshIsReportedAsAFailure()
    {
        ControlResponse result = await Handler(() => false).HandleAsync(new ControlEnvelope
        { MessageType = ControlMessageType.ListGames, Payload = "{\"Refresh\":true}" }, CancellationToken.None);
        Assert.False(result.IsSuccessful);
        Assert.Equal(ControlErrorCode.ExecutionFailed, result.ErrorCode);
        Assert.Contains("previous catalogue", result.Message);
    }

    [Fact]
    public void GameClickUsesTheClickedIdentityRatherThanTheTitle() => WinFormsTestRunner.Run(() =>
    {
        GameView first = new() { Id = "shared", Name = "Same title", Library = 1 };
        GameView clicked = new() { Id = "shared", Name = "Same title", Library = 4 };
        using ShortcutForm form = new(new FakeClient());
        form.Shortcut = new Desktop.ShortcutItem();
        Set(form, "_availableGames", new List<GameView> { first, clicked });
        Call(form, "RefreshGamesList");
        ImageListView list = Field<ImageListView>(form, "ilv_games");
        ClickItem(form, "ilv_games_ItemClick", list.Items[1]);
        Assert.Same(clicked, Field<GameView>(form, "_selectedGame"));
        Assert.Equal(SupportedGameLibraryType.Epic, form.GameLibrary);
        return Task.CompletedTask;
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SavedGameSelectionUsesItsLibraryAndIdEvenWhenItsTitleChanged(bool installed) => WinFormsTestRunner.Run(() =>
    {
        GameView other = new() { Id = "shared", Name = "Old title", Library = 1 };
        GameView expected = new() { Id = "shared", Name = "New title", Library = 4 };
        int warnings = 0;
        using ShortcutForm form = new(new FakeClient(), (_, _, _) => warnings++);
        form.EditingExistingShortcut = true;
        form.Shortcut = new Desktop.ShortcutItem
        {
            Name = "Shortcut", AutoName = false, Category = ShortcutCategory.Game,
            GameAppId = "shared", GameName = "Old title", GameLibrary = SupportedGameLibraryType.Epic,
            ProfileUUID = Desktop.ShortcutItem.SkipDisplayChangeUUID, AudioProfileUUID = Desktop.ShortcutItem.SkipAudioProfilesChangeUUID
        };
        Set(form, "_availableGames", installed ? new List<GameView> { other, expected } : new List<GameView> { other });
        Call(form, "LoadShortcut");
        Assert.Equal(installed ? 0 : 1, warnings);
        if (installed) Assert.Same(expected, Field<GameView>(form, "_selectedGame"));
        else Assert.Null(Field<GameView?>(form, "_selectedGame"));
        return Task.CompletedTask;
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GameValidationRequiresTheExactLibraryAndUsableMonitoringPath(bool correctLibrary) => WinFormsTestRunner.Run(() =>
    {
        string exe = Path.Combine(_root, "game.exe");
        File.WriteAllText(exe, "Test path only; never launched.");
        using ShortcutForm form = new(new FakeClient());
        form.Shortcut = new Desktop.ShortcutItem();
        Set(form, "_configurationLoaded", true);
        Set(form, "_profileToUse", Desktop.ShortcutItem.SkipDisplayChangeProfile);
        Field<TextBox>(form, "txt_shortcut_save_name").Text = "Test shortcut";
        Field<RadioButton>(form, "rb_launcher").Checked = true;
        form.GameLibrary = SupportedGameLibraryType.Epic;
        Set(form, "_gameId", "shared");
        Field<TextBox>(form, "txt_game_name").Text = "Old title";
        Set(form, "_availableGames", new List<GameView> { new() { Id = "shared", Name = "Renamed title", Library = correctLibrary ? 4 : 1, ExecutablePath = exe } });
        Call(form, "RefreshGamesList");
        Assert.Equal(correctLibrary, (bool)Call(form, "AllowedToSave", false)!);
        if (correctLibrary)
        {
            Field<List<GameView>>(form, "_availableGames")[0].ExecutablePath = "";
            Assert.False((bool)Call(form, "AllowedToSave", false)!);
            Field<CheckBox>(form, "cb_wait_alternative_game").Checked = true;
            Field<TextBox>(form, "txt_alternative_game").Text = exe;
            Assert.True((bool)Call(form, "AllowedToSave", false)!);
        }
        return Task.CompletedTask;
    });

    [Fact]
    public void RefreshButtonRequestsARescanWithoutReplacingDraftFields() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new();
        using ShortcutForm form = new(client, (_, _, _) => { });
        Set(form, "_configurationLoaded", true);
        Field<TextBox>(form, "txt_shortcut_save_name").Text = "Unsaved name";
        Call(form, "btn_refresh_games_list_Click", form, EventArgs.Empty);
        await Until(() => !Field<bool>(form, "_serviceActionInProgress"));
        Assert.True(JsonSerializer.Deserialize<GameListRequest>(Assert.Single(client.Requests).Payload)!.Refresh);
        Assert.Equal("Unsaved name", Field<TextBox>(form, "txt_shortcut_save_name").Text);
    });

    [Theory]
    [InlineData(null)]
    [InlineData(2)]
    public void AppPreselectionRequiresAnUnambiguousIdentityEvenWithoutAnExe(int? library) => WinFormsTestRunner.Run(() =>
    {
        FakeClient client = new();
        client.Apps = new[] { new AppView { Id = "shared", Name = "Same", Library = 1 }, new AppView { Id = "shared", Name = "Same", Library = 2 } };
        using ChooseExecutableForm form = new(client);
        form.PreviousAppId = "shared";
        form.PreviousAppLibrary = library;
        Call(form, "ChooseExecutableForm_Load", form, EventArgs.Empty);
        if (library.HasValue) Assert.Equal(library.Value, Field<AppView>(form, "_selectedApp").Library);
        else Assert.Null(Field<AppView?>(form, "_selectedApp"));
        return Task.CompletedTask;
    });

    [Fact]
    public void AppClickUsesTheClickedRowWhenTitlesAndIdsCollide() => WinFormsTestRunner.Run(() =>
    {
        FakeClient client = new();
        client.Apps = new[] { new AppView { Id = "shared", Name = "Same", Library = 1 }, new AppView { Id = "shared", Name = "Same", Library = 2 } };
        using ChooseExecutableForm form = new(client);
        Call(form, "ChooseExecutableForm_Load", form, EventArgs.Empty);
        ImageListView list = Field<ImageListView>(form, "ilv_installed_apps");
        list.Items[1].Selected = true;
        ClickItem(form, "ilv_installed_apps_ItemClick", list.Items[1]);
        Call(form, "btn_select_app_Click", form, EventArgs.Empty);
        Assert.Equal(2, form.AppToUse.Library);
        Assert.Equal(DialogResult.OK, form.DialogResult);
        return Task.CompletedTask;
    });

    [Fact]
    public void CancellingTheFileChooserKeepsTheAppPickerOpen() => WinFormsTestRunner.Run(() =>
    {
        using ChooseExecutableForm form = new(new FakeClient(), chooseExecutable: () => "");
        Call(form, "btn_select_exe_Click", form, EventArgs.Empty);
        Assert.Equal(DialogResult.None, form.DialogResult);
        Assert.False(form.IsDisposed);
        Assert.Null(form.ExeToUse);
        return Task.CompletedTask;
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClosedAppPickerIgnoresLateSuccessAndFailure(bool fail) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new() { Pending = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        int errors = 0;
        using ChooseExecutableForm form = new(client, _ => errors++);
        Task load = (Task)Call(form, "LoadAppsAsync")!;
        form.Dispose();
        Assert.True(client.LastToken.IsCancellationRequested);
        if (fail) client.Pending.SetException(new IOException("Late failure"));
        else client.Pending.SetResult(new ControlResponse { IsSuccessful = true, AppList = new() { Apps = new[] { new AppView { Id = "late" } } } });
        await load;
        Assert.Equal(0, errors);
        Assert.Empty(Field<List<AppView>>(form, "_installedApps"));
    });

    [Fact]
    public void ShortcutDeletionRemovesAllMatchingHotkeysAndPersistsUnrelatedOnes()
    {
        using Desktop.DirectInputManager manager = new(null!);
        string id = Guid.NewGuid().ToString("D");
        List<Desktop.HotkeyKeyboard> keyboard = Desktop.Program.AppProgramSettings.KeyboardHotkeys;
        List<Desktop.HotkeyJoystick> joystick = Desktop.Program.AppProgramSettings.JoystickHotkeys;
        foreach (Key key in new[] { Key.A, Key.B })
        {
            keyboard.Add(new() { UUID = id.ToUpperInvariant(), KeyCodes = new() { key } });
            manager.RegisterKeyCombination(new() { key }, () => { });
        }
        for (int index = 0; index < 2; index++)
        {
            List<Desktop.JoystickButton> buttons = new() { new() { DeviceButtonIndex = index } };
            joystick.Add(new() { UUID = id, Buttons = buttons });
            manager.RegisterJoystickButtons(buttons, () => { });
        }
        keyboard.Add(new() { UUID = "other", KeyCodes = new() { Key.C } });
        joystick.Add(new() { UUID = "other", Buttons = new() { new() { DeviceButtonIndex = 3 } } });
        Assert.True(manager.RemoveHotkeysByUUID(id));
        Assert.Equal("other", Assert.Single(keyboard).UUID);
        Assert.Equal("other", Assert.Single(joystick).UUID);
        Assert.False(manager.HasRegisteredHotkeys);
        using JsonDocument saved = JsonDocument.Parse(File.ReadAllText(Desktop.ProgramSettings.ProgramSettingsStorageJsonFullFileName, System.Text.Encoding.Unicode));
        Assert.Equal("other", Assert.Single(saved.RootElement.GetProperty("Settings").GetProperty("KeyboardHotkeys").EnumerateArray()).GetProperty("UUID").GetString());
        Assert.Equal("other", Assert.Single(saved.RootElement.GetProperty("Settings").GetProperty("JoystickHotkeys").EnumerateArray()).GetProperty("UUID").GetString());
    }

    [Fact]
    public void RegisteredBindingsDoNotShareMutableCallerLists()
    {
        using Desktop.DirectInputManager manager = new(null!);
        List<Key> keys = new() { Key.A };
        List<Desktop.JoystickButton> buttons = new() { new() { DeviceButtonIndex = 1 } };
        manager.RegisterKeyCombination(keys, () => { });
        manager.RegisterJoystickButtons(buttons, () => { });
        keys.Clear();
        buttons.Clear();
        Assert.True(manager.RemoveKeyCombination(new() { Key.A }));
        Assert.True(manager.RemoveJoystickButtons(new() { new() { DeviceButtonIndex = 1 } }));
        Assert.False(manager.HasRegisteredHotkeys);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RemovingByNameRemovesEveryMatchingRecordWithoutBreakingEnumeration(bool joystick)
    {
        using Desktop.DirectInputManager manager = new(null!);
        string name;
        if (joystick)
        {
            Desktop.HotkeyJoystick hotkey = new() { UUID = "first", Buttons = new() { new() { DeviceName = "Pad", DeviceButtonIndex = 2 } } };
            Desktop.Program.AppProgramSettings.JoystickHotkeys.Add(hotkey);
            Desktop.Program.AppProgramSettings.JoystickHotkeys.Add(hotkey);
            name = manager.GetNameOfJoystickHotkey(hotkey);
        }
        else
        {
            Desktop.HotkeyKeyboard hotkey = new() { UUID = "first", KeyCodes = new() { Key.A } };
            Desktop.Program.AppProgramSettings.KeyboardHotkeys.Add(hotkey);
            Desktop.Program.AppProgramSettings.KeyboardHotkeys.Add(hotkey);
            name = manager.GetNameOfKeyboardHotkey(hotkey);
        }
        Assert.True(manager.RemoveHotkeysByName(name));
        Assert.Empty(Desktop.Program.AppProgramSettings.KeyboardHotkeys);
        Assert.Empty(Desktop.Program.AppProgramSettings.JoystickHotkeys);
    }

    [Fact]
    public void HotkeyCleanupDoesNotReportSuccessWhenSettingsCannotBeSaved()
    {
        using Desktop.DirectInputManager manager = new(null!);
        Desktop.Program.AppProgramSettings.KeyboardHotkeys.Add(new() { UUID = "target", KeyCodes = new() { Key.A } });
        string path = Desktop.ProgramSettings.ProgramSettingsStorageJsonFullFileName;
        File.WriteAllText(path, "Existing settings");
        using FileStream locked = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        Assert.False(manager.RemoveHotkeysByUUID("target"));
        Assert.Equal("Existing settings", File.ReadAllText(path));
    }

    private ProfileCommandHandler Handler(Func<bool> scan) => new(new AgentRegistration { UserSid = "test", SessionId = 1 }, new ControlServiceClient(), null, _root, new UserProfileOperationService(), scan);
    private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(owner)!;
    private static void Set(object owner, string name, object? value) => owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(owner, value);
    private static object? Call(object owner, string name, params object[] args) => owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(owner, args);
    private static void ClickItem(object owner, string name, ImageListViewItem row)
    {
        Type eventType = owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetParameters()[1].ParameterType;
        ConstructorInfo constructor = eventType.GetConstructors().First(info => info.GetParameters().Any(parameter => parameter.ParameterType == typeof(ImageListViewItem)));
        object?[] values = constructor.GetParameters().Select(parameter => parameter.ParameterType == typeof(ImageListViewItem) ? row : parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null).ToArray();
        Call(owner, name, owner, constructor.Invoke(values));
    }
    private static async Task Until(Func<bool> condition)
    {
        for (int attempt = 0; attempt < 200 && !condition(); attempt++) await Task.Delay(10);
        Assert.True(condition());
    }

    public void Dispose()
    {
        GameLibrary.AllInstalledGamesInAllLibraries = _previousGames;
        GameLibrary.GamesLoaded = _previousLoaded;
        Desktop.Program.AppProgramSettings = _previousSettings;
        foreach ((FieldInfo field, object? value) in _state) field.SetValue(null, value);
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    private sealed class FakeGame : Game
    {
        public GameLibrary? Owner { get; set; }
        public override GameLibrary? GameLibrary => Owner;
        public override SupportedGameLibraryType GameLibraryType { get; }
        public FakeGame(string id, SupportedGameLibraryType library) { Id = id; Name = id; GameLibraryType = library; }
    }
    private sealed class FakeLibrary : GameLibrary
    {
        public Func<bool> Scan { get; set; } = () => true;
        public FakeLibrary(SupportedGameLibraryType library) { GameLibraryType = library; IsGameLibraryInstalled = true; }
        public override bool LoadInstalledGames() => Scan();
    }
    private sealed class FakeClient : Desktop.ControlServicePipeClient
    {
        public List<ControlEnvelope> Requests { get; } = new();
        public AppView[] Apps { get; set; } = Array.Empty<AppView>();
        public TaskCompletionSource<ControlResponse>? Pending { get; set; }
        public CancellationToken LastToken { get; private set; }
        protected override Task<ControlResponse> SendAsync(ControlEnvelope request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            LastToken = cancellationToken;
            return Pending?.Task ?? Task.FromResult(new ControlResponse { IsSuccessful = true, AppList = new() { Apps = Apps }, GameList = new() });
        }
    }
}
