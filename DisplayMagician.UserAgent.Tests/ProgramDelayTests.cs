using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Contracts;
using DisplayMagician.Processes;
using DisplayMagician.UIForms;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ProgramDelayTests
{
    [Theory]
    [InlineData("5")]
    [InlineData("6")]
    [InlineData("7")]
    [InlineData("8")]
    public void MigrationDefaultsMissingDelaysAndPreservesExistingData(string version)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagician-ProgramDelay", Guid.NewGuid().ToString("N"));
        string directory = Path.Combine(root, "Shortcuts");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "Shortcuts.json");
        try
        {
            string original = new JsonObject
            {
                ["ShortcutFileVersion"] = version,
                ["CustomSetting"] = "keep",
                ["Shortcuts"] = new JsonArray(new JsonObject
                {
                    ["UUID"] = "delay-test", ["MonitorLaunchedExecutable"] = false,
                    ["StartPrograms"] = new JsonArray(new JsonObject { ["WaitAfterStartSecs"] = 999 }, new JsonObject()),
                    ["StopPrograms"] = new JsonArray(new JsonObject { ["WaitAfterStopSecs"] = 17 }, new JsonObject()),
                    ["AfterPrograms"] = new JsonArray(new JsonObject { ["WaitAfterStartSecs"] = 23 }, new JsonObject())
                })
            }.ToJsonString();
            File.WriteAllText(path, original, Encoding.Unicode);
            ShortcutStore store = new(root);
            using JsonDocument saved = JsonDocument.Parse(store.GetSnapshot().Json);
            Assert.Equal("9", saved.RootElement.GetProperty("ShortcutFileVersion").GetString());
            Assert.Equal("keep", saved.RootElement.GetProperty("CustomSetting").GetString());
            ShortcutDefinition definition = Assert.Single(store.GetShortcutDefinitions());
            Assert.False(definition.MonitorLaunchedExecutable);
            Assert.Equal(new[] { 999, 0 }, definition.StartPrograms.Select(program => program.WaitAfterStartSecs));
            Assert.Equal(new[] { 17, 0 }, definition.StopPrograms.Select(program => program.WaitAfterStopSecs));
            Assert.Equal(new[] { 23, 0 }, definition.AfterPrograms.Select(program => program.WaitAfterStartSecs));
            Assert.Equal(original, File.ReadAllText(Assert.Single(Directory.GetFiles(directory, "Shortcuts.json.v*.bak")), Encoding.Unicode));
            byte[] migrated = File.ReadAllBytes(path);
            _ = new ShortcutStore(root);
            Assert.Equal(migrated, File.ReadAllBytes(path));
            Assert.Single(Directory.GetFiles(directory, "Shortcuts.json.v*.bak"));
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MigrationAcceptsLegacyArraysAndJsonNetCollections(bool wrapped)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagician-ProgramDelay", Guid.NewGuid().ToString("N"));
        string directory = Path.Combine(root, "Shortcuts");
        Directory.CreateDirectory(directory);
        try
        {
            string shortcut = "{\"UUID\":\"legacy\",\"StartPrograms\":{\"$values\":[{}]},\"StopPrograms\":[{}],\"AfterPrograms\":[{}]}";
            string json = wrapped ? "{\"ShortcutFileVersion\":\"5\",\"Shortcuts\":{\"$values\":[" + shortcut + "]}}" : "[" + shortcut + "]";
            File.WriteAllText(Path.Combine(directory, "Shortcuts.json"), json, Encoding.Unicode);
            ShortcutDefinition definition = Assert.Single(new ShortcutStore(root).GetShortcutDefinitions());
            Assert.Equal(0, Assert.Single(definition.StartPrograms).WaitAfterStartSecs);
            Assert.Equal(0, Assert.Single(definition.StopPrograms).WaitAfterStopSecs);
            Assert.Equal(0, Assert.Single(definition.AfterPrograms).WaitAfterStartSecs);
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData("StartPrograms", "WaitAfterStartSecs", "-1", false)]
    [InlineData("StartPrograms", "WaitAfterStartSecs", "1000", false)]
    [InlineData("StopPrograms", "WaitAfterStopSecs", "1.5", false)]
    [InlineData("StopPrograms", "WaitAfterStopSecs", "\"2\"", false)]
    [InlineData("StartPrograms", "WaitAfterStartSecs", "null", false)]
    [InlineData("StopPrograms", "WaitAfterStopSecs", "0", true)]
    [InlineData("StartPrograms", "WaitAfterStartSecs", "999", true)]
    public void CommitsValidateExactDelayRangeWithoutReplacingExistingData(string collection, string field, string value, bool valid)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagician-ProgramDelay", Guid.NewGuid().ToString("N"));
        try
        {
            ShortcutStore store = new(root);
            RepositoryCommitResult initial = store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, Json = "{\"Shortcuts\":[]}" });
            RepositorySnapshot initialSnapshot = Assert.IsType<RepositorySnapshot>(initial.Snapshot);
            RepositoryCommitRequest request = new()
            {
                Repository = RepositoryKind.Shortcuts, ExpectedRevision = initialSnapshot.Revision,
                Json = "{\"Shortcuts\":[{\"UUID\":\"test\",\"" + collection + "\":[{\"" + field + "\":" + value + "}]}]}"
            };
            if (valid) Assert.False(store.Commit(request).WasConflict);
            else
            {
                Assert.Throws<JsonException>(() => store.Commit(request));
                Assert.Equal(initialSnapshot.Json, store.GetSnapshot().Json);
                Assert.Equal(initialSnapshot.Revision, store.GetSnapshot().Revision);
            }
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void CopyAndPortableConversionRetainAllDelays()
    {
        using Bitmap originalImage = new(1, 1);
        using Bitmap shortcutImage = new(1, 1);
        using Bitmap selectedImage = new(1, 1);
        global::DisplayMagician.ShortcutItem shortcut = new()
        {
            OriginalLargeBitmap = originalImage,
            ShortcutBitmap = shortcutImage,
            SelectedImage = new() { Image = selectedImage },
            StartPrograms = new() { new() { WaitAfterStartSecs = 999 } },
            StopPrograms = new() { new() { WaitAfterStopSecs = 42 } },
            AfterPrograms = new() { new() { WaitAfterStartSecs = 17 } }
        };
        global::DisplayMagician.ShortcutItem copy = new();
        try
        {
            Assert.True(shortcut.CopyTo(copy));
            ShortcutDefinition definition = copy.CreateConfigurationDefinition();
            Assert.Equal(999, Assert.Single(definition.StartPrograms).WaitAfterStartSecs);
            Assert.Equal(42, Assert.Single(definition.StopPrograms).WaitAfterStopSecs);
            Assert.Equal(17, Assert.Single(definition.AfterPrograms).WaitAfterStartSecs);
        }
        finally
        {
            copy.OriginalLargeBitmap?.Dispose();
            copy.ShortcutBitmap?.Dispose();
            copy.SelectedImage.Image?.Dispose();
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(999)]
    [InlineData(1000)]
    public void DesignerControlsPreserveInvalidSavedValuesUntilEdited(int seconds) => WinFormsTestRunner.Run(() =>
    {
        using StartProgramControl start = new(new global::DisplayMagician.StartProgram { WaitAfterStartSecs = seconds });
        using StopProgramControl stop = new(new global::DisplayMagician.StopProgram { WaitAfterStopSecs = seconds });
        NumericUpDown startDelay = Assert.IsType<NumericUpDown>(start.Controls["nud_wait_after_start"]);
        NumericUpDown stopDelay = Assert.IsType<NumericUpDown>(stop.Controls["nud_wait_after_stop"]);
        Assert.Equal(999m, startDelay.Maximum);
        Assert.Equal(999m, stopDelay.Maximum);
        Assert.Equal(Math.Clamp(seconds, 0, 999), startDelay.Value);
        Assert.Equal(seconds, start.StartProgram.WaitAfterStartSecs);
        Assert.Equal(seconds, stop.StopProgram.WaitAfterStopSecs);
        ErrorProvider errors = (ErrorProvider)typeof(StartProgramControl).GetField("error_provider", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(start)!;
        Assert.Equal(seconds < 0 || seconds > 999, !string.IsNullOrEmpty(errors.GetError(startDelay)));
        startDelay.Value = 12;
        stopDelay.Value = 13;
        Assert.Equal(12, start.StartProgram.WaitAfterStartSecs);
        Assert.Equal(13, stop.StopProgram.WaitAfterStopSecs);
        Assert.Empty(errors.GetError(startDelay));
        start.StartProgram = new() { Disabled = true, WaitAfterStartSecs = 12 };
        stop.StopProgram = new() { Disabled = true, WaitAfterStopSecs = 13 };
        Assert.False(startDelay.Enabled);
        Assert.False(stopDelay.Enabled);
        return Task.CompletedTask;
    });

    [Fact]
    public void EditingEitherDelayMarksTheOwningShortcutUnsaved() => WinFormsTestRunner.Run(() =>
    {
        using ShortcutForm form = new(new global::DisplayMagician.ControlServicePipeClient(), (_, _, _) => { });
        using StartProgramControl start = new();
        using StopProgramControl stop = new();
        form.Controls.Add(start);
        form.Controls.Add(stop);
        FieldInfo loaded = typeof(ShortcutForm).GetField("_loadedShortcut", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo unsaved = typeof(ShortcutForm).GetField("_isUnsaved", BindingFlags.Instance | BindingFlags.NonPublic)!;
        loaded.SetValue(form, true);
        unsaved.SetValue(form, false);
        ((NumericUpDown)start.Controls["nud_wait_after_start"]!).Value = 1;
        Assert.Equal(true, unsaved.GetValue(form));
        unsaved.SetValue(form, false);
        ((NumericUpDown)stop.Controls["nud_wait_after_stop"]!).Value = 2;
        Assert.Equal(true, unsaved.GetValue(form));
        return Task.CompletedTask;
    });

    [Theory]
    [InlineData(false, -1)]
    [InlineData(false, 1000)]
    [InlineData(true, -1)]
    [InlineData(true, 1000)]
    public async Task InvalidPersistedDelayFailsBeforeAnyProgramRuns(bool stop, int seconds)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagician-ProgramDelay", Guid.NewGuid().ToString("N"));
        string directory = Path.Combine(root, "Shortcuts");
        Directory.CreateDirectory(directory);
        try
        {
            JsonObject shortcut = new()
            {
                ["UUID"] = "invalid", ["Category"] = 2,
                [stop ? "StopPrograms" : "StartPrograms"] = new JsonArray(new JsonObject
                {
                    ["Executable"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "whoami.exe"),
                    [stop ? "WaitAfterStopSecs" : "WaitAfterStartSecs"] = seconds
                })
            };
            File.WriteAllText(Path.Combine(directory, "Shortcuts.json"),
                new JsonObject { ["ShortcutFileVersion"] = "9", ["Shortcuts"] = new JsonArray(shortcut) }.ToJsonString(), Encoding.Unicode);
            ShortcutRunner runner = new(new ShortcutStore(root), new AutomaticGameDetectionRegistry(), new UserProfileOperationService(), new ShortcutRecoveryStore(root));
            List<OperationPhase> phases = new();
            ShortcutRunResult result = await runner.ApplyShortcutProfilesAsync("invalid", 0, CancellationToken.None, (status, _) =>
            {
                phases.Add(status.Phase);
                return Task.CompletedTask;
            });
            Assert.Equal(ShortcutRunOutcome.Failed, result.Outcome);
            Assert.Contains("between 0 and 999 seconds", result.Message);
            Assert.Equal(new[] { OperationPhase.Validating }, phases);
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SuccessfulActionWaitsIncludingTheLastStepAndCancellationCleansUp(bool stopAction, bool cancel)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagician-ProgramDelay", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string executable = Path.Combine(root, "dm-delay-test.exe");
        File.Copy(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe"), executable);
        string mainExecutable = Path.Combine(root, "dm-delay-main.exe");
        File.Copy(executable, mainExecutable);
        using Process? running = stopAction ? Process.Start(new ProcessStartInfo(executable, "-n 30 127.0.0.1") { UseShellExecute = false, CreateNoWindow = true }) : null;
        try
        {
            ShortcutStore store = new(root);
            JsonObject program = new()
            {
                ["Executable"] = executable, ["Arguments"] = "-n 30 127.0.0.1",
                ["CloseOnFinish"] = true, [stopAction ? "WaitAfterStopSecs" : "WaitAfterStartSecs"] = cancel ? 999 : 1
            };
            JsonObject shortcut = new()
            {
                ["UUID"] = "run", ["Category"] = 0,
                ["ExecutableNameAndPath"] = mainExecutable,
                ["ExecutableArguments"] = "-n 3 127.0.0.1",
                [stopAction ? "StopPrograms" : "StartPrograms"] = new JsonArray(program)
            };
            store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, Json = new JsonObject { ["Shortcuts"] = new JsonArray(shortcut) }.ToJsonString() });
            ShortcutRunner runner = new(store, new AutomaticGameDetectionRegistry(), new UserProfileOperationService(), new ShortcutRecoveryStore(root));
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(15));
            Stopwatch? wait = null;
            TimeSpan? waited = null;
            // Task.Delay uses the Windows timer clock; Stopwatch can observe a completion
            // just under the requested duration. Allow one timer tick without hiding a skipped wait.
            TimeSpan minimumObservedWait = TimeSpan.FromMilliseconds(980);
            bool mainStarted = false;
            Guid operationId = Guid.NewGuid();
            ShortcutRunResult result = await runner.ApplyShortcutProfilesAsync("run", 0, cancellation.Token, (status, _) =>
            {
                Assert.Equal(operationId, status.OperationId);
                if (status.Phase == OperationPhase.StartingGame)
                {
                    mainStarted = true;
                    Assert.NotNull(wait);
                    Assert.True(wait.Elapsed >= minimumObservedWait);
                }
                if (status.Phase == OperationPhase.StartingPrograms && status.Message.StartsWith("Waiting ", StringComparison.Ordinal))
                {
                    Assert.Equal(OperationPhase.StartingPrograms, status.Phase);
                    wait = Stopwatch.StartNew();
                    if (cancel) cancellation.Cancel();
                }
                else if (wait != null && waited == null) waited = wait.Elapsed;
                return Task.CompletedTask;
            }, operationId);
            Assert.NotNull(wait);
            Assert.Equal(cancel ? ShortcutRunOutcome.Cancelled : ShortcutRunOutcome.Completed, result.Outcome);
            Assert.Equal(!cancel, mainStarted);
            if (!cancel) Assert.True(waited >= minimumObservedWait, $"Wait ended after {waited}.");
            Assert.False(ProcessTreeMonitor.IsExecutableRunning(executable));
            Assert.False(ProcessTreeMonitor.IsExecutableRunning(mainExecutable));
            Assert.False(runner.IsRecoveryRequired);
        }
        finally
        {
            if (running != null && !running.HasExited) { running.Kill(); running.WaitForExit(5000); }
            Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData("already-running")]
    [InlineData("absent-stop")]
    [InlineData("disabled")]
    [InlineData("failed")]
    public async Task SkippedDisabledAndFailedActionsDoNotWait(string scenario)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagician-ProgramDelay", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string executable = Path.Combine(root, "dm-delay-skip.exe");
        File.Copy(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe"), executable);
        using Process? running = scenario == "already-running" ? Process.Start(new ProcessStartInfo(executable, "-n 30 127.0.0.1") { UseShellExecute = false, CreateNoWindow = true }) : null;
        try
        {
            bool stop = scenario == "absent-stop";
            ShortcutStore store = new(root);
            JsonObject shortcut = new()
            {
                ["UUID"] = "skip", ["Category"] = 2,
                [stop ? "StopPrograms" : "StartPrograms"] = new JsonArray(new JsonObject
                {
                    ["Executable"] = executable, ["Disabled"] = scenario == "disabled",
                    ["DontStartIfAlreadyRunning"] = true,
                    [stop ? "WaitAfterStopSecs" : "WaitAfterStartSecs"] = 999
                }),
                ["AfterPrograms"] = new JsonArray(new JsonObject
                {
                    ["Executable"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "whoami.exe"),
                    ["WaitAfterStartSecs"] = 999
                })
            };
            store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, Json = new JsonObject { ["Shortcuts"] = new JsonArray(shortcut) }.ToJsonString() });
            ShortcutRunner runner = new(store, new AutomaticGameDetectionRegistry(), new UserProfileOperationService(), new ShortcutRecoveryStore(root));
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(10));
            List<string> messages = new();
            int failures = 0;
            ShortcutRunResult result = await runner.ApplyShortcutProfilesAsync("skip", 0, cancellation.Token, (status, _) =>
            {
                messages.Add(status.Message);
                if (scenario == "failed" && status.Phase == OperationPhase.StartingPrograms) File.Delete(executable);
                return Task.CompletedTask;
            }, requestDecisionAsync: (request, _) =>
            {
                failures++;
                return Task.FromResult(new OperationDecision { ResolvedChoice = OperationDecisionChoice.Continue });
            });
            Assert.Equal(ShortcutRunOutcome.Completed, result.Outcome);
            Assert.DoesNotContain(messages, message => message.StartsWith("Waiting ", StringComparison.Ordinal));
            Assert.Equal(scenario == "failed" ? 1 : 0, failures);
            if (running != null) Assert.False(running.HasExited);
        }
        finally
        {
            if (running != null && !running.HasExited) { running.Kill(); running.WaitForExit(5000); }
            Directory.Delete(root, true);
        }
    }
}
