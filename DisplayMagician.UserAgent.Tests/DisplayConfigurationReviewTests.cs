using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using DisplayMagician.UserAgent.Runtime;
using DisplayMagician.UserAgent.Runtime.Windows;
using ApplyProfileResult = DisplayMagician.UserAgent.Runtime.ApplyProfileResult;
using AgentWinLibrary = DisplayMagician.UserAgent.Runtime.Windows.WinLibrary;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

[Collection("Agent repository state")]
public sealed class DisplayConfigurationReviewTests
{
    [Theory]
    [InlineData("Resolution", true)]
    [InlineData("Position", true)]
    [InlineData("Rotation", true)]
    [InlineData("Topology", true)]
    [InlineData("Cloning", true)]
    [InlineData("RefreshRate", false)]
    [InlineData("Scaling", false)]
    public void ReadbackDistinguishesLayoutFailuresFromOptionalSettings(string change, bool fatal)
    {
        WINDOWS_DISPLAY_CONFIG saved = Configuration();
        WINDOWS_DISPLAY_CONFIG actual = Configuration();
        switch (change)
        {
            case "Resolution": actual.DisplayConfigModes[0].SourceMode.Width = 1280; break;
            case "Position": actual.DisplayConfigModes[0].SourceMode.Position.X = 1920; break;
            case "Rotation": actual.DisplayConfigPaths[0].TargetInfo.Rotation = DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_ROTATE90; break;
            case "Topology": actual.DisplayConfigPaths = Array.Empty<DISPLAYCONFIG_PATH_INFO>(); break;
            case "Cloning": actual.IsCloned = true; break;
            case "RefreshRate": actual.DisplayConfigPaths[0].TargetInfo.RefreshRate.Numerator = 120; break;
            case "Scaling": actual.DisplayConfigPaths[0].TargetInfo.Scaling = DISPLAYCONFIG_SCALING.DISPLAYCONFIG_SCALING_CENTERED; break;
        }
        DisplayConfigurationDiagnostics diagnostics = new();
        AgentWinLibrary.CompareAppliedLayout(saved, actual, diagnostics);
        Assert.Equal(!fatal, diagnostics.Result.IsSuccessful);
        Assert.Contains(diagnostics.Result.Issues, issue => issue.Stage == (change == "Cloning" ? "Topology" : change) &&
            issue.Severity == (fatal ? DisplayConfigurationSeverity.Fatal : DisplayConfigurationSeverity.Warning));
    }

    [Fact]
    public void NewlyReportedFatalCannotBeHiddenByASuccessReturn()
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(diagnostics.Run("Capture", "Display", () =>
        {
            diagnostics.Add("Enumeration", "Intel", "Failed read.", DisplayConfigurationSeverity.Fatal);
            return true;
        }, DisplayConfigurationSeverity.Fatal));
        Assert.Single(diagnostics.Result.Issues);
    }

    [Fact]
    public void EarlierFatalDoesNotPreventAFallbackStageFromReportingSuccess()
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        diagnostics.Add("FirstAttempt", "Windows", "Failed.", DisplayConfigurationSeverity.Fatal);
        Assert.True(diagnostics.Run("Fallback", "Windows", () => true, DisplayConfigurationSeverity.Fatal));
        Assert.False(diagnostics.Result.IsSuccessful);
    }

    [Fact]
    public void ReadbackAcceptsEquivalentRefreshFractionsAndPackedVirtualIndices()
    {
        WINDOWS_DISPLAY_CONFIG saved = Configuration();
        WINDOWS_DISPLAY_CONFIG actual = Configuration();
        saved.DisplayConfigPaths[0].SourceInfo.ModeInfoIdx = uint.MaxValue;
        actual.DisplayConfigPaths[0].SourceInfo.ModeInfoIdx = 0x10001;
        actual.DisplayConfigPaths[0].TargetInfo.RefreshRate.Numerator = 60000;
        actual.DisplayConfigPaths[0].TargetInfo.RefreshRate.Denominator = 1000;
        DisplayConfigurationDiagnostics diagnostics = new();
        AgentWinLibrary.CompareAppliedLayout(saved, actual, diagnostics);
        Assert.Empty(diagnostics.Result.Issues);
    }

    [Fact]
    public void RenumberedTargetProducesVerificationWarningWithoutGuessingGeometry()
    {
        WINDOWS_DISPLAY_CONFIG actual = Configuration();
        actual.DisplayConfigPaths[0].TargetInfo.Id = 9;
        DisplayConfigurationDiagnostics diagnostics = new();
        AgentWinLibrary.CompareAppliedLayout(Configuration(), actual, diagnostics);
        Assert.True(diagnostics.Result.IsSuccessful);
        Assert.Equal("Verification", Assert.Single(diagnostics.Result.Issues).Stage);
    }

    [Theory]
    [InlineData(DisplayConfigurationSeverity.Warning, true)]
    [InlineData(DisplayConfigurationSeverity.Fatal, false)]
    public void ExceptionsRetainNativeCodeAndRespectStageSeverity(DisplayConfigurationSeverity severity, bool successful)
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(diagnostics.Run("HDR", "Windows", () => throw new Win32Exception(5, "Denied"), severity));
        Assert.Equal(successful, diagnostics.Result.IsSuccessful);
        DisplayConfigurationIssue issue = Assert.Single(diagnostics.Result.Issues);
        Assert.Equal("5", issue.NativeErrorCode);
        Assert.NotNull(issue.ExceptionHResult);
    }

    [Fact]
    public void WarningFromAnInnerStageCannotHideAnOuterFatalFailure()
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        diagnostics.Run("Topology", "Windows", () =>
        {
            diagnostics.Add("HDR", "Windows", "Optional setting failed.");
            return false;
        }, DisplayConfigurationSeverity.Fatal);
        Assert.False(diagnostics.Result.IsSuccessful);
        Assert.Contains(diagnostics.Result.Issues, issue => issue.Stage == "Topology");
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void RepeatedApplyUsesTheFinalAttemptAndPreservesConfiguredDelays(bool firstSucceeds, bool finalSucceeds)
    {
        ProfileItem profile = new() { WindowsDisplayConfig = Configuration(), ApplyProfileCount = 2, ApplyProfileDelay = 3 };
        int attempts = 0;
        List<int> waits = new();
        int refreshes = 0;
        ApplyProfileResult outcome = ProfileRepository.ApplyProfile(profile, out DisplayConfigurationResult result, diagnostics =>
        {
            bool success = ++attempts == 1 ? firstSucceeds : finalSucceeds;
            if (!success) diagnostics.Add("Topology", "Windows", "Simulated failure.", DisplayConfigurationSeverity.Fatal);
            else diagnostics.Result.TopologyApplied = true;
            return success;
        }, waits.Add, () => refreshes++);
        Assert.Equal(2, attempts);
        Assert.Equal(new[] { 3000, 200 }, waits);
        Assert.Equal(2, result.Attempt);
        Assert.Equal(finalSucceeds, result.IsSuccessful);
        Assert.Equal(finalSucceeds ? ApplyProfileResult.Successful : ApplyProfileResult.Error, outcome);
        Assert.Equal(finalSucceeds ? 1 : 0, refreshes);
    }

    [Fact]
    public void WarningOnlyApplyStillSucceeds()
    {
        ProfileItem profile = new() { WindowsDisplayConfig = Configuration(), ApplyProfileCount = 1 };
        ApplyProfileResult outcome = ProfileRepository.ApplyProfile(profile, out DisplayConfigurationResult result, diagnostics =>
        {
            diagnostics.Result.TopologyApplied = true;
            diagnostics.Add("HDR", "Windows", "Simulated HDR failure.");
            return true;
        }, _ => { }, () => { });
        Assert.Equal(ApplyProfileResult.Successful, outcome);
        Assert.True(result.HasWarnings);
        Assert.True(result.TopologyApplied);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ShortcutOnlyRequestsDecisionAndDisablesPlacementForFatalApply(bool fatal)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagician-DisplayReview", Guid.NewGuid().ToString("N"));
        try
        {
            ShortcutStore store = new(root);
            Assert.NotNull(store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = 0,
                Json = "{\"Shortcuts\":[{\"UUID\":\"test\",\"Category\":2,\"ProfileUUID\":\"target\"}]}" }).Snapshot);
            ShortcutRunner runner = new(store, new AutomaticGameDetectionRegistry(), new FakeProfiles(fatal), new ShortcutRecoveryStore(root));
            int decisions = 0;
            bool? displayFailed = null;
            List<OperationStatusUpdate> updates = new();
            ShortcutRunResult result = await runner.ApplyShortcutProfilesAsync("test", 0, CancellationToken.None, (update, _) =>
            {
                updates.Add(update);
                if (update.Phase == OperationPhase.StartingPrograms)
                    displayFailed = ((ShortcutRunner.WindowPlacementRun?)typeof(ShortcutRunner).GetField("_activeWindowPlacementRun", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(runner))?.DisplayProfileFailed;
                return Task.CompletedTask;
            }, requestDecisionAsync: (_, _) => { decisions++; return Task.FromResult(new OperationDecision { IsResolved = true, ResolvedChoice = OperationDecisionChoice.Continue }); });
            Assert.Equal(ShortcutRunOutcome.Completed, result.Outcome);
            Assert.Equal(fatal ? 1 : 0, decisions);
            Assert.Equal(fatal, displayFailed);
            Assert.Contains(updates, update => update.DisplayResults.Any(details => !details.IsRestoration && details.Issues.Length > 0));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void DiagnosticsRoundTripAndOutboxCopiesDoNotShareMutableIssues()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagician-DisplayReview", Guid.NewGuid().ToString("N"));
        try
        {
            OperationStatusUpdate update = new() { OperationId = Guid.NewGuid(), DisplayResults = new[] { DisplayConfigurationDiagnostics.Failure("profile", "Topology", "Failure") } };
            OperationStatusOutbox outbox = new(root);
            outbox.Enqueue(update);
            update.DisplayResults[0].Issues[0].Message = "Changed caller";
            OperationStatusUpdate persisted = Assert.Single(new OperationStatusOutbox(root).GetPending());
            Assert.Equal("Failure", persisted.DisplayResults[0].Issues[0].Message);
            persisted.DisplayResults[0].Issues[0].Message = "Changed reader";
            Assert.Equal("Failure", Assert.Single(outbox.GetActive()).DisplayResults[0].Issues[0].Message);
            OperationStatusUpdate old = JsonSerializer.Deserialize<OperationStatusUpdate>("{\"OperationId\":\"" + update.OperationId + "\"}")!;
            Assert.Empty(old.DisplayResults);
            Assert.True(OperationStatusValidation.IsValidUpdate(old));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void MalformedNestedDiagnosticsAreRejected()
    {
        OperationStatusUpdate update = new() { OperationId = Guid.NewGuid(), DisplayResults = new[] { new DisplayConfigurationResult { Issues = new DisplayConfigurationIssue[] { null! } } } };
        Assert.False(OperationStatusValidation.IsValidUpdate(update));
    }

    [Fact]
    public void FailedCapturePreservesCachedProfileButReportsFailure()
    {
        ProfileItem? previous = ProfileRepository.CurrentProfile;
        try
        {
            ProfileItem cached = new();
            ProfileRepository.CurrentProfile = cached;
            bool success = ProfileRepository.TryUpdateActiveProfile(out DisplayConfigurationResult result, (_, diagnostics) =>
            {
                diagnostics.Add("Capture", "Windows", "Simulated native refresh failure.", DisplayConfigurationSeverity.Fatal);
                return false;
            });
            Assert.False(success);
            Assert.True(result.IsCapture);
            Assert.False(result.IsSuccessful);
            Assert.Same(cached, ProfileRepository.CurrentProfile);
        }
        finally { ProfileRepository.CurrentProfile = previous!; }
    }

    [Fact]
    public async Task DiagnosticsRemainAvailableAfterLaterProgressReconnectionAndCompletion()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagician-DisplayReview", Guid.NewGuid().ToString("N"));
        try
        {
            RecordingStatusClient client = new();
            ProfileCommandHandler handler = new(new AgentRegistration { UserSid = "test-user", SessionId = 1 }, client, null, root, new FakeProfiles(false));
            var publish = typeof(ProfileCommandHandler).GetMethod("PublishShortcutStatusAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            Guid id = Guid.NewGuid();
            OperationStatusUpdate first = new() { OperationId = id,
                DisplayResults = new[] { new DisplayConfigurationResult { Issues = new[] { new DisplayConfigurationIssue { Stage = "HDR", Component = "Windows" } } } } };
            await (Task)publish.Invoke(handler, new object[] { first, CancellationToken.None })!;
            await (Task)publish.Invoke(handler, new object[] { first, CancellationToken.None })!;
            Assert.Single(client.Updates);
            await (Task)publish.Invoke(handler, new object[] { new OperationStatusUpdate { OperationId = id, Phase = OperationPhase.WaitingForGameToClose }, CancellationToken.None })!;
            Assert.Single(Assert.Single(new OperationStatusOutbox(root).GetActive()).DisplayResults);
            await (Task)publish.Invoke(handler, new object[] { new OperationStatusUpdate { OperationId = id, Phase = OperationPhase.Completed, IsTerminal = true, IsSuccessful = true }, CancellationToken.None })!;
            Assert.Equal("HDR", Assert.Single(client.Updates.Last().DisplayResults).Issues[0].Stage);
            Assert.Empty(new OperationStatusOutbox(root).GetActive());
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private sealed class RecordingStatusClient : ControlServiceClient
    {
        internal List<OperationStatusUpdate> Updates { get; } = new();
        public override Task<OperationStatus> PublishOperationStatusAsync(AgentRegistration registration, OperationStatusUpdate update, CancellationToken token)
        {
            Updates.Add(update);
            return Task.FromResult(new OperationStatus { OperationId = update.OperationId });
        }
    }

    private sealed class FakeProfiles(bool fatal) : UserProfileOperationService
    {
        internal override UserProfileOperationState CaptureCurrentProfileState(bool display, bool audio, CancellationToken cancellationToken = default)
            => new("original", "") { DisplayProfileSnapshotJson = display ? "{}" : "" };

        internal override Task<ApplyDisplayProfileOperationResult> ApplyDisplayProfileAsync(string id, CancellationToken token, string? snapshot)
        {
            DisplayConfigurationDiagnostics diagnostics = new();
            diagnostics.Result.ProfileId = id;
            diagnostics.Result.TopologyApplied = !fatal;
            if (snapshot == null) diagnostics.Add(fatal ? "Topology" : "HDR", "Windows", "Simulated failure.", fatal ? DisplayConfigurationSeverity.Fatal : DisplayConfigurationSeverity.Warning);
            return Task.FromResult(new ApplyDisplayProfileOperationResult(snapshot != null || !fatal, false) { Details = diagnostics.Result });
        }
    }

    private static WINDOWS_DISPLAY_CONFIG Configuration() => new()
    {
        DisplayConfigPaths = new[] { new DISPLAYCONFIG_PATH_INFO
        {
            Flags = DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_ACTIVE,
            SourceInfo = new DISPLAYCONFIG_PATH_SOURCE_INFO { Id = 1 },
            TargetInfo = new DISPLAYCONFIG_PATH_TARGET_INFO { Id = 2, Rotation = DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_IDENTITY,
                Scaling = DISPLAYCONFIG_SCALING.DISPLAYCONFIG_SCALING_IDENTITY, RefreshRate = new DISPLAYCONFIG_RATIONAL { Numerator = 60, Denominator = 1 } }
        } },
        DisplayConfigModes = new[] { new DISPLAYCONFIG_MODE_INFO { Id = 1, InfoType = DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE,
            SourceMode = new DISPLAYCONFIG_SOURCE_MODE { Width = 1920, Height = 1080 } } }
    };
}
