using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using DisplayMagician.UserAgent.Runtime;
using DisplayMagician.UserAgent.Runtime.AMD;
using DisplayMagician.UserAgent.Runtime.Windows;
using Xunit;
using ApplyProfileResult = DisplayMagician.UserAgent.Runtime.ApplyProfileResult;

namespace DisplayMagician.UserAgent.Tests;

[Collection("Agent repository state")]
public sealed class DisplayHardwareAccessTests : IDisposable
{
    private readonly List<(FieldInfo Field, object? Value)> _previous = new();
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-DisplayAccess", Guid.NewGuid().ToString("N"));
    public DisplayHardwareAccessTests()
    {
        foreach (Type type in new[] { typeof(ProfileRepository), typeof(AudioProfileRepository) })
            foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(field => !field.IsInitOnly && !field.IsLiteral))
                _previous.Add((field, field.GetValue(null)));
        ProfileRepository.ConfigureStoragePath(_root);
        // These cases exercise the authoritative Agent repository, not a client cache connection
        // left by another test using the legacy repository adapter. Restore it in Dispose.
        typeof(ProfileRepository).GetField("_userAgentRepositoryConnection", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, null);
        ProfileRepository.ProfilesLoaded = true;
        AudioProfileRepository.AudioProfilesLoaded = true;
    }

    [Fact]
    public async Task CaptureWaitsUntilApplyAndItsVerificationFinish()
    {
        using ManualResetEventSlim applying = new();
        using ManualResetEventSlim finish = new();
        using ManualResetEventSlim captureRequested = new();
        int sequence = 0;
        Task operation = Task.Run(() => ProfileRepository.ApplyProfile(Profile(), out _, _ =>
        {
            Assert.Equal(0, Interlocked.CompareExchange(ref sequence, 1, 0));
            applying.Set();
            Assert.True(finish.Wait(TimeSpan.FromSeconds(5)));
            return true;
        }, _ => { }, () => Assert.Equal(1, Interlocked.CompareExchange(ref sequence, 2, 1))));
        Assert.True(applying.Wait(TimeSpan.FromSeconds(5)));
        Task capture = Task.Run(() =>
        {
            captureRequested.Set();
            Assert.True(ProfileRepository.TryUpdateActiveProfile(out _, (_, _) =>
            {
                Assert.Equal(2, Interlocked.CompareExchange(ref sequence, 3, 2));
                return true;
            }));
        });
        try
        {
            Assert.True(captureRequested.Wait(TimeSpan.FromSeconds(5)));
            Assert.Equal(1, Volatile.Read(ref sequence));
        }
        finally { finish.Set(); }
        await Task.WhenAll(operation, capture).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(3, sequence);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EachApplyAttemptUsesFreshStateEvenWithoutAChangeEvent(bool readSucceeds)
    {
        List<string> calls = new();
        ProfileItem profile = Profile();
        profile.ApplyProfileCount = 2;
        ApplyProfileResult result = ProfileRepository.ApplyProfile(profile, out DisplayConfigurationResult details,
            _ => { calls.Add("apply"); return true; }, _ => { }, () => { },
            _ => { calls.Add("read"); return readSucceeds; });
        Assert.Equal(readSucceeds ? new[] { "read", "apply", "read", "apply" } : new[] { "read", "read" }, calls);
        Assert.Equal(readSucceeds ? ApplyProfileResult.Successful : ApplyProfileResult.Error, result);
        Assert.Equal(readSucceeds, details.IsSuccessful);
    }

    [Fact]
    public void VendorSettingChangeDoesNotDependOnWindowsNotifications()
    {
        ProfileItem saved = new() { Name = "Saved" };
        AMD_DISPLAY_CONFIG desired = new() { IsInUse = true };
        desired.Displays.Add(1, new AMD_DISPLAY_WITH_SETTINGS { IsSupportedFreeSync = true, IsEnabledFreeSync = true });
        saved.AMDDisplayConfig = desired;
        ProfileRepository.AllProfiles.Add(saved);
        ProfileRepository.CurrentProfile = saved;
        Assert.True(ProfileRepository.TryUpdateActiveProfile(out _, (captured, _) =>
        {
            AMD_DISPLAY_CONFIG actual = new() { IsInUse = true };
            actual.Displays.Add(1, new AMD_DISPLAY_WITH_SETTINGS { IsSupportedFreeSync = true, IsEnabledFreeSync = false });
            captured.AMDDisplayConfig = actual;
            return true;
        }));
        Assert.NotEqual(saved.UUID, ProfileRepository.CurrentProfile!.UUID);
        Assert.False(ProfileRepository.CurrentProfile.AMDDisplayConfig.Displays[1].IsEnabledFreeSync);
        Assert.True(saved.AMDDisplayConfig.Displays[1].IsEnabledFreeSync);
    }

    [Fact]
    public async Task OrdinaryListsReturnPreviousCompleteSnapshotWhileRefreshIsRunning()
    {
        using ManualResetEventSlim started = new();
        using ManualResetEventSlim finish = new();
        int refreshes = 0;
        ProfileCommandHandler handler = Handler(() =>
        {
            Interlocked.Increment(ref refreshes);
            return ProfileRepository.TryUpdateActiveProfile(out _, (captured, _) =>
            {
                captured.Name = "New complete state";
                started.Set();
                Assert.True(finish.Wait(TimeSpan.FromSeconds(5)));
                return true;
            });
        });
        ProfileRepository.CurrentProfile = new ProfileItem { Name = "Previous complete state" };
        Task<ControlResponse> refresh = handler.HandleAsync(Request(true), CancellationToken.None);
        try
        {
            Assert.True(started.Wait(TimeSpan.FromSeconds(5)));
            ControlResponse cached = await handler.HandleAsync(Request(false), CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(3));
            Assert.True(cached.IsSuccessful);
            Assert.Equal("Previous complete state", cached.ProfileList!.CurrentLayout!.Name);
            Assert.Equal(1, Volatile.Read(ref refreshes));
        }
        finally { finish.Set(); }
        Assert.True((await refresh).IsSuccessful);
        Assert.Equal("New complete state", ProfileRepository.CurrentProfile!.Name);
    }

    [Fact]
    public async Task FailedExplicitRefreshKeepsTheSnapshotAndReportsFailure()
    {
        ProfileCommandHandler handler = Handler(() => false);
        ProfileItem previous = new() { Name = "Previous" };
        ProfileRepository.CurrentProfile = previous;
        ControlResponse response = await handler.HandleAsync(Request(true), CancellationToken.None);
        Assert.False(response.IsSuccessful);
        Assert.Equal(ControlErrorCode.ExecutionFailed, response.ErrorCode);
        Assert.Same(previous, ProfileRepository.CurrentProfile);
    }

    [Fact]
    public async Task CancellationCanBeRequestedWhileHardwareAccessIsHeld()
    {
        ProfileCommandHandler handler = Handler(() => true);
        using CancellationTokenSource operation = new();
        Guid operationId = Guid.NewGuid();
        Assert.True((bool)typeof(ProfileCommandHandler).GetMethod("TryBeginOperation", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(handler, new object[] { operationId, operation, DisplayOperationType.StartShortcut })!);
        using ManualResetEventSlim entered = new();
        using ManualResetEventSlim finish = new();
        Task hardware = Task.Run(() =>
        {
            using IDisposable scope = DisplayHardwareAccess.Enter();
            entered.Set();
            Assert.True(finish.Wait(TimeSpan.FromSeconds(5)));
        });
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
            ControlResponse response = await handler.HandleAsync(new ControlEnvelope
            {
                MessageType = ControlMessageType.CancelOperation,
                Payload = JsonSerializer.Serialize(new CancelOperationRequest { OperationId = operationId })
            }, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(3));
            Assert.True(response.IsSuccessful);
            Assert.True(operation.IsCancellationRequested);
        }
        finally { finish.Set(); }
        await hardware;
    }

    [Fact]
    public void SnapshotAccessAndConnectivityDoNotPerformLazyHardwareReads()
    {
        Assert.Null(ProfileRepository.CurrentProfile);
        ProfileRepository.ConnectedDisplayIdentifiers = new List<string>();
        Assert.Empty(ProfileRepository.ConnectedDisplayIdentifiers);
        Assert.Null(ProfileRepository.GetActiveProfile());
    }

    [Fact]
    public void NestedAccessRemainsHeldUntilTheOuterPhaseEnds()
    {
        using IDisposable outer = DisplayHardwareAccess.Enter();
        using (DisplayHardwareAccess.Enter()) Assert.True(Monitor.IsEntered(DisplayHardwareAccess.SyncRoot));
        Assert.True(Monitor.IsEntered(DisplayHardwareAccess.SyncRoot));
    }

    [Fact]
    public async Task CancellationWhileWaitingDoesNotReleaseAnotherOperationsAccess()
    {
        using ManualResetEventSlim ownerReady = new();
        using ManualResetEventSlim release = new();
        Task owner = Task.Run(() =>
        {
            using IDisposable scope = DisplayHardwareAccess.Enter();
            ownerReady.Set();
            Assert.True(release.Wait(TimeSpan.FromSeconds(5)));
            Assert.True(Monitor.IsEntered(DisplayHardwareAccess.SyncRoot));
        });
        Assert.True(ownerReady.Wait(TimeSpan.FromSeconds(5)));
        try
        {
            using CancellationTokenSource cancellation = new(TimeSpan.FromMilliseconds(100));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Task.Run(() =>
            {
                using IDisposable scope = DisplayHardwareAccess.Enter(cancellation.Token);
            }));
        }
        finally { release.Set(); }
        await owner;
        using IDisposable available = DisplayHardwareAccess.Enter();
    }

    [Fact]
    public void FailureReleasesAccessForTheNextOperation()
    {
        Assert.Throws<IOException>((Action)(() =>
        {
            using IDisposable scope = DisplayHardwareAccess.Enter();
            throw new IOException("Failed hardware operation");
        }));
        Assert.False(Monitor.IsEntered(DisplayHardwareAccess.SyncRoot));
        using IDisposable next = DisplayHardwareAccess.Enter();
    }

    [Fact]
    public void PublishedSnapshotDoesNotShareMutableDefinitionData()
    {
        ProfileItem definition = Profile();
        ProfileRepository.PublishCapturedProfile(definition);
        definition.Name = "Changed definition";
        definition.WindowsDisplayConfig.DisplayConfigModes[0].SourceMode.Width = 1;
        Assert.NotSame(definition, ProfileRepository.CurrentProfile);
        Assert.NotEqual(definition.Name, ProfileRepository.CurrentProfile!.Name);
        Assert.Equal((uint)1920, ProfileRepository.CurrentProfile.WindowsDisplayConfig.DisplayConfigModes[0].SourceMode.Width);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SavingCurrentLayoutKeepsListsReadableAndPreservesConcurrentEdits(bool editDuringCapture)
    {
        ProfileItem definition = Profile();
        ProfileRepository.AllProfiles.Add(definition);
        using ManualResetEventSlim started = new();
        using ManualResetEventSlim finish = new();
        Task<bool> saving = Task.Run(() => ProfileRepository.CaptureLayoutToProfile(definition, captured =>
        {
            captured.Name = "Captured candidate";
            started.Set();
            Assert.True(finish.Wait(TimeSpan.FromSeconds(5)));
            return true;
        }));
        try
        {
            Assert.True(started.Wait(TimeSpan.FromSeconds(5)));
            ProfileItem[] listed = await Task.Run(ProfileRepository.GetProfilesForClientSnapshot).WaitAsync(TimeSpan.FromSeconds(3));
            Assert.Equal("Display", Assert.Single(listed).Name);
            if (editDuringCapture) Assert.True(ProfileRepository.RenameProfile(definition, "Other device edit"));
        }
        finally { finish.Set(); }
        Assert.Equal(!editDuringCapture, await saving);
        Assert.Equal(editDuringCapture ? "Other device edit" : "Captured candidate", Assert.Single(ProfileRepository.GetProfilesForClientSnapshot()).Name);
        Assert.Equal(editDuringCapture ? "Other device edit" : "Display", definition.Name);
    }

    [Fact]
    public void FreshAdapterMatchingDoesNotMutateSavedOrCapturedData()
    {
        ProfileItem saved = Profile();
        WINDOWS_DISPLAY_CONFIG savedConfig = saved.WindowsDisplayConfig;
        savedConfig.DisplayAdapters.Add(7, "adapter-name");
        savedConfig.DisplayConfigPaths[0].SourceInfo.AdapterId = new LUID { LowPart = 7 };
        savedConfig.DisplayConfigPaths[0].TargetInfo.AdapterId = new LUID { LowPart = 7 };
        savedConfig.DisplayConfigModes[0].AdapterId = new LUID { LowPart = 7 };
        ProfileRepository.AllProfiles.Add(saved);
        Assert.True(ProfileRepository.TryUpdateActiveProfile(out _, (capture, _) =>
        {
            WINDOWS_DISPLAY_CONFIG actual = Profile().WindowsDisplayConfig;
            actual.DisplayAdapters.Add(9, "adapter-name");
            actual.DisplayConfigPaths[0].SourceInfo.AdapterId = new LUID { LowPart = 9 };
            actual.DisplayConfigPaths[0].TargetInfo.AdapterId = new LUID { LowPart = 9 };
            actual.DisplayConfigModes[0].AdapterId = new LUID { LowPart = 9 };
            capture.WindowsDisplayConfig = actual;
            return true;
        }));
        Assert.Equal(saved.UUID, ProfileRepository.CurrentProfile!.UUID);
        Assert.Equal((ulong)7, saved.WindowsDisplayConfig.DisplayConfigPaths[0].SourceInfo.AdapterId.Value);
        Assert.Equal((ulong)9, ProfileRepository.CurrentProfile.WindowsDisplayConfig.DisplayConfigPaths[0].SourceInfo.AdapterId.Value);
        Assert.NotSame(saved, ProfileRepository.CurrentProfile);
    }

    [Fact]
    public void FatalCaptureDiagnosticCannotPublishAReportedSuccessfulRead()
    {
        ProfileItem previous = Profile();
        ProfileRepository.CurrentProfile = previous;
        Assert.False(ProfileRepository.TryUpdateActiveProfile(out _, (_, diagnostics) =>
        {
            diagnostics.Add("Capture", "Windows", "Incomplete snapshot", DisplayConfigurationSeverity.Fatal);
            return true;
        }));
        Assert.Same(previous, ProfileRepository.CurrentProfile);
    }

    [Fact]
    public async Task ColdDefinitionLoadDoesNotWaitForHardwareOrPatchStoredAdapterIds()
    {
        ProfileItem saved = Profile();
        saved.WindowsDisplayConfig.DisplayAdapters.Add(7, "stored-adapter");
        saved.WindowsDisplayConfig.DisplayConfigPaths[0].SourceInfo.AdapterId = new LUID { LowPart = 7 };
        File.WriteAllText(ProfileRepository.ProfileStorageFileName,
            Newtonsoft.Json.JsonConvert.SerializeObject(new { ProfileFileVersion = "4", Profiles = new[] { Newtonsoft.Json.Linq.JObject.Parse(UserProfileOperationService.SerializeDisplayRecoveryProfile(saved)) } }),
            System.Text.Encoding.Unicode);
        ProfileRepository.ProfilesLoaded = false;
        using ManualResetEventSlim entered = new();
        using ManualResetEventSlim finish = new();
        Task hardware = Task.Run(() =>
        {
            using IDisposable scope = DisplayHardwareAccess.Enter();
            entered.Set();
            Assert.True(finish.Wait(TimeSpan.FromSeconds(5)));
        });
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
            ProfileItem[] definitions = await Task.Run(ProfileRepository.GetProfilesForClientSnapshot).WaitAsync(TimeSpan.FromSeconds(3));
            Assert.Equal((ulong)7, Assert.Single(definitions).WindowsDisplayConfig.DisplayConfigPaths[0].SourceInfo.AdapterId.Value);
        }
        finally { finish.Set(); }
        await hardware;
    }

    [Fact]
    public async Task SaturatedCaptureQueueLeavesCapacityForControlRequests()
    {
        int captures = 0;
        ProfileCommandHandler handler = Handler(() => { Interlocked.Increment(ref captures); return true; });
        using ManualResetEventSlim entered = new();
        using ManualResetEventSlim finish = new();
        Task hardware = Task.Run(() =>
        {
            using IDisposable scope = DisplayHardwareAccess.Enter();
            entered.Set();
            Assert.True(finish.Wait(TimeSpan.FromSeconds(5)));
        });
        Task<ControlResponse>[] queued = Array.Empty<Task<ControlResponse>>();
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
            queued = Enumerable.Range(0, AgentCommandServer.MaximumConnectedCommandClients - 1)
                .Select(_ => handler.HandleAsync(Request(true), CancellationToken.None)).ToArray();
            ControlResponse busy = await handler.HandleAsync(Request(true), CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(3));
            Assert.Equal(ControlErrorCode.DisplayControlBusy, busy.ErrorCode);
            Assert.Equal(0, Volatile.Read(ref captures));
            ControlResponse cancellation = await handler.HandleAsync(new ControlEnvelope
            {
                MessageType = ControlMessageType.CancelOperation,
                Payload = JsonSerializer.Serialize(new CancelOperationRequest { OperationId = Guid.NewGuid() })
            }, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(3));
            Assert.Equal(ControlErrorCode.InvalidRequest, cancellation.ErrorCode);
        }
        finally { finish.Set(); }
        await hardware;
        Assert.All(await Task.WhenAll(queued), result => Assert.True(result.IsSuccessful));
        Assert.Equal(AgentCommandServer.MaximumConnectedCommandClients - 1, captures);
    }

    [Fact]
    public void UnsupportedGammaDefaultDoesNotFailDisplayCaptureConversion()
    {
        AMD_GAMMA_INFO unsupported = new(default(ADLXWrapper.GammaDto));
        Assert.False(unsupported.IsSupported);
        Assert.Empty(unsupported.ReGammaRamp.Gamma);
        Assert.Empty(unsupported.DeGammaRamp.Gamma);
        Assert.Empty(unsupported.ToGammaDto().ReGammaRamp.Values);
    }

    [Fact]
    public void ConnectivityAndLayoutArePublishedTogetherAndFailureKeepsBoth()
    {
        ProfileRepository.ConnectedDisplayIdentifiers = new List<string> { "old-display" };
        ProfileRepository.CurrentProfile = new ProfileItem { Name = "Old layout" };
        DisplayDetectionSnapshot old = ProfileRepository.GetDisplaySnapshot();
        ProfileItem target = new() { ProfileDisplayIdentifiers = new List<string> { "old-display" } };
        Assert.Empty(target.GetUndetectedDisplayIdentifiers());
        Assert.False(ProfileRepository.TryUpdateActiveProfile(out _, (_, _) =>
        {
            Assert.True(ProfileRepository.TryRefreshDisplayDetectionState(new DisplayConfigurationDiagnostics(),
                () => (new List<string> { "new-display" }, false)));
            Assert.Same(old, ProfileRepository.GetDisplaySnapshot());
            Assert.Empty(target.GetUndetectedDisplayIdentifiers());
            return false;
        }));
        Assert.Same(old, ProfileRepository.GetDisplaySnapshot());
        Assert.True(ProfileRepository.TryUpdateActiveProfile(out _, (capture, _) => { capture.Name = "New layout"; return true; }));
        DisplayDetectionSnapshot updated = ProfileRepository.GetDisplaySnapshot();
        Assert.Equal("New layout", updated.Profile!.Name);
        Assert.Equal(new[] { "new-display" }, updated.ConnectedIdentifiers);
        Assert.Equal(new[] { "old-display" }, target.GetUndetectedDisplayIdentifiers());
        Assert.Equal(new[] { "old-display" }, old.ConnectedIdentifiers);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConnectivityEnumerationFailureDoesNotPublishAnEmptyTopology(bool throws)
    {
        ProfileRepository.ConnectedDisplayIdentifiers = new List<string> { "connected" };
        ProfileRepository.CurrentProfile = new ProfileItem();
        DisplayDetectionSnapshot before = ProfileRepository.GetDisplaySnapshot();
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(ProfileRepository.TryRefreshDisplayDetectionState(diagnostics,
            () => throws ? throw new IOException("Enumeration failed") : (new List<string>(), true)));
        Assert.False(diagnostics.Result.IsSuccessful);
        Assert.Same(before, ProfileRepository.GetDisplaySnapshot());
        Assert.Equal(new[] { "connected" }, ProfileRepository.ConnectedDisplayIdentifiers);
    }

    [Fact]
    public void ProfileDefaultsAndPreviewUseStoredDataOnly()
    {
        ProfileItem profile = new();
        Assert.False(profile.NVIDIADisplayConfig.IsInUse);
        Assert.False(profile.AMDDisplayConfig.IsInUse);
        Assert.False(profile.IntelDisplayConfig.IsInUse);
        Assert.NotNull(profile.NVIDIADisplayConfig.PhysicalAdapters);
        Assert.NotNull(profile.WindowsDisplayConfig.DisplayConfigPaths);
        Assert.Empty(profile.GetScreenPositions());
    }

    [Fact]
    public async Task EmptyProfileSerializationDoesNotWaitForHardwareAccess()
    {
        using ManualResetEventSlim entered = new();
        using ManualResetEventSlim release = new();
        Task hardware = Task.Run(() =>
        {
            using IDisposable access = DisplayHardwareAccess.Enter();
            entered.Set();
            Assert.True(release.Wait(TimeSpan.FromSeconds(10)));
        });
        Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
        Task<string> serialized = Task.Run(() => UserProfileOperationService.SerializeDisplayRecoveryProfile(new ProfileItem()));
        try
        {
            string json = await serialized.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.Empty(Newtonsoft.Json.Linq.JObject.Parse(json)["ProfileDisplayIdentifiers"]!);
        }
        finally
        {
            release.Set();
            await hardware;
            await serialized;
        }
    }

    [Fact]
    public void MigratedSnapshotRevisionCanBeUsedForTheNextClientCommit()
    {
        string path = ProfileRepository.ProfileStorageFileName;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{\"ProfileFileVersion\":\"4\",\"Profiles\":[],\"FutureSetting\":\"keep\"}", System.Text.Encoding.Unicode);
        RepositorySnapshot snapshot = ProfileRepository.GetSnapshot();
        Assert.Equal("5", Newtonsoft.Json.Linq.JObject.Parse(snapshot.Json)["ProfileFileVersion"]);
        RepositoryCommitResult commit = ProfileRepository.CommitSnapshot(new RepositoryCommitRequest
        {
            Repository = RepositoryKind.DisplayProfiles, ExpectedRevision = snapshot.Revision, Json = snapshot.Json
        });
        Assert.False(commit.WasConflict);
        Assert.Equal("keep", Newtonsoft.Json.Linq.JObject.Parse(commit.Snapshot!.Json)["FutureSetting"]);
        Assert.Single(Directory.GetFiles(Path.GetDirectoryName(path)!, "*.identity-migration.*.bak"));
    }

    [Fact]
    public void RuntimeSaveCannotReplaceANewerUnsupportedProfileFile()
    {
        string path = ProfileRepository.ProfileStorageFileName;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        const string original = "{\"ProfileFileVersion\":\"6\",\"Profiles\":[]}";
        File.WriteAllText(path, original, System.Text.Encoding.Unicode);
        Assert.Throws<Newtonsoft.Json.JsonSerializationException>(() => ProfileRepository.GetSnapshot());
        Assert.False(ProfileRepository.SaveProfiles());
        Assert.Equal(original, File.ReadAllText(path, System.Text.Encoding.Unicode));
        Assert.Single(Directory.GetFiles(Path.GetDirectoryName(path)!));
    }

    private ProfileCommandHandler Handler(Func<bool> refresh)
    {
        ProfileCommandHandler handler = new(new AgentRegistration { UserSid = "test", SessionId = 1 }, new ControlServiceClient(), null,
            _root, new UserProfileOperationService(), refreshDisplaySnapshot: refresh);
        ProfileRepository.ProfilesLoaded = true;
        return handler;
    }
    private static ControlEnvelope Request(bool refresh) => new()
    {
        MessageType = ControlMessageType.ListProfiles,
        Payload = JsonSerializer.Serialize(new ProfileListRequest { RefreshCurrentLayout = refresh })
    };
    private static ProfileItem Profile() => new()
    {
        Name = "Display", ApplyProfileCount = 1,
        WindowsDisplayConfig = new WINDOWS_DISPLAY_CONFIG
        {
            DisplayConfigPaths = new[] { new DISPLAYCONFIG_PATH_INFO { Flags = DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_ACTIVE,
                SourceInfo = new DISPLAYCONFIG_PATH_SOURCE_INFO { Id = 1 }, TargetInfo = new DISPLAYCONFIG_PATH_TARGET_INFO { Id = 2 } } },
            DisplayConfigModes = new[] { new DISPLAYCONFIG_MODE_INFO { Id = 1, InfoType = DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE,
                SourceMode = new DISPLAYCONFIG_SOURCE_MODE { Width = 1920, Height = 1080 } } }
        }
    };
    public void Dispose()
    {
        foreach (var previous in _previous) previous.Field.SetValue(null, previous.Value);
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}
