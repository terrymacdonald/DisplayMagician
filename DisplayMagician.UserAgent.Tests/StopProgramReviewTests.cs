using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

[Collection("Agent repository state")]
public sealed class StopProgramReviewTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-StopReview", Guid.NewGuid().ToString("N"));
    private readonly List<Process> _children = new();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StopActionSelectsNewestOrAllAndRepeatedEntriesSelectTheNextCopy(bool all)
    {
        Process older = Start();
        Process newer = Start();
        Assert.True(newer.StartTime > older.StartTime);
        List<int> selected = new();
        List<ShortcutRunner.StoppedProgram> restarts = new();
        ShortcutStopProgramDefinition definition = new() { ExecutablePath = Executable, StopAllInstances = all, RestartAfterwards = true };
        ProgramActionOutcome first = ShortcutRunner.StopProgramInstances(definition, 0, restarts, process =>
        {
            selected.Add(process.Id);
            process.Kill();
            return process.WaitForExit(5000);
        });
        Assert.Equal(ProgramActionOutcome.Succeeded, first);
        Assert.Equal(all ? new[] { newer.Id, older.Id } : new[] { newer.Id }, selected);
        Assert.Single(restarts);
        Assert.True(newer.HasExited);
        if (!all)
        {
            Assert.False(older.HasExited);
            Assert.Equal(ProgramActionOutcome.Succeeded, ShortcutRunner.StopProgramInstances(definition, 1, restarts, process =>
            {
                Assert.Equal(older.Id, process.Id);
                process.Kill();
                return process.WaitForExit(5000);
            }));
            Assert.Equal(2, restarts.Count);
        }
        Assert.Equal(ProgramActionOutcome.Skipped, ShortcutRunner.StopProgramInstances(definition, 2, restarts, _ => throw new Exception("Nothing should remain.")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PartialStopPreservesOneRestartAndContinuesAfterAnExpectedException(bool throwOnFailure)
    {
        Process older = Start();
        Process newer = Start();
        List<ShortcutRunner.StoppedProgram> restarts = new();
        int attempts = 0;
        ProgramActionOutcome result = ShortcutRunner.StopProgramInstances(new ShortcutStopProgramDefinition
        {
            ExecutablePath = Executable, StopAllInstances = true, RestartAfterwards = true
        }, 7, restarts, process =>
        {
            attempts++;
            if (process.Id == newer.Id)
            {
                if (throwOnFailure) throw new Win32Exception("Simulated inaccessible instance.");
                return false;
            }
            process.Kill();
            return process.WaitForExit(5000);
        });
        Assert.Equal(ProgramActionOutcome.Failed, result);
        Assert.Equal(2, attempts);
        Assert.Equal(7, Assert.Single(restarts).Sequence);
        Assert.False(newer.HasExited);
        Assert.True(older.HasExited);
        int restartCalls = 0;
        ShortcutRunner.RunPostGameProgramCleanup(new(), restarts, restartProgram: _ =>
        {
            restartCalls++;
            return new();
        });
        Assert.Equal(1, restartCalls); // The surviving copy must not suppress the approved restart.
    }

    [Fact]
    public void CompleteStopFailureDoesNotScheduleARestart()
    {
        _ = Start();
        List<ShortcutRunner.StoppedProgram> restarts = new();
        Assert.Equal(ProgramActionOutcome.Failed, ShortcutRunner.StopProgramInstances(new()
        {
            ExecutablePath = Executable, RestartAfterwards = true
        }, 0, restarts, _ => false));
        Assert.Empty(restarts);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CleanupContinuesInReverseOrderAndDisposesHandlesAfterFailure(bool throwOnStop)
    {
        Process child = Start();
        Process cleanupHandle = Process.GetProcessById(child.Id);
        List<ShortcutRunner.StartedProgram> started = new() { new(0, 3, new() { cleanupHandle }, null) };
        List<ShortcutRunner.StoppedProgram> stopped = new()
        {
            new(new() { ExecutablePath = "first.exe" }, 0), new(new() { ExecutablePath = "second.exe" }, 1)
        };
        List<string> order = new();
        ShortcutRunner.RunPostGameProgramCleanup(started, stopped, _ =>
        {
            order.Add("stop-helper");
            if (throwOnStop) throw new Win32Exception("Simulated cleanup failure.");
            return false;
        }, definition =>
        {
            order.Add(definition.ExecutablePath);
            if (definition.ExecutablePath == "second.exe") throw new IOException("Simulated restart failure.");
            return new();
        });
        Assert.Equal(new[] { "stop-helper", "second.exe", "first.exe" }, order);
        Assert.Empty(started);
        Assert.Empty(stopped);
        Assert.Throws<InvalidOperationException>(() => cleanupHandle.Id);
        Assert.False(child.HasExited);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExpectedActionFailureUsesTheExistingDecisionFlow(bool stopAndRestore)
    {
        ShortcutDefinition shortcut = new() { Id = "test", StartPrograms = new[]
        {
            new ShortcutStartProgramDefinition { Priority = 0, ExecutablePath = "first.exe", WaitAfterStartSecs = 999 },
            new ShortcutStartProgramDefinition { Priority = 1, ExecutablePath = "second.exe" }
        } };
        int attempts = 0;
        int decisions = 0;
        bool result = await ShortcutRunner.RunPreGameProgramsAsync(shortcut, new(), new(), new(Guid.NewGuid(), shortcut), (request, _) =>
        {
            decisions++;
            Assert.Equal(OperationDecisionChoice.Continue, request.DefaultChoice);
            return Task.FromResult(new OperationDecision { ResolvedChoice = stopAndRestore ? OperationDecisionChoice.StopAndRestore : OperationDecisionChoice.Continue });
        }, Guid.NewGuid(), CancellationToken.None, null, (_, sequence) =>
        {
            attempts++;
            if (sequence == 0) throw new Win32Exception("Simulated preparation failure.");
            return ProgramActionOutcome.Succeeded;
        });
        Assert.Equal(!stopAndRestore, result);
        Assert.Equal(stopAndRestore ? 1 : 2, attempts);
        Assert.Equal(1, decisions);
    }

    [Fact]
    public void ShellDispatchWithoutAProcessDoesNotReadItsPidOrReportLaunchFailure()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(Executable, "test shell target");
        List<Process> handles = ProcessUtils.StartProcessCore(Executable, "private arguments", ProcessPriority.Normal, 1, false, out bool dispatched, _ => false);
        Assert.True(dispatched);
        Assert.Empty(handles);
        List<Process> failed = ProcessUtils.StartProcessCore(Executable, "", ProcessPriority.Normal, 1, false, out bool failedDispatch, _ => throw new Win32Exception(2));
        Assert.False(failedDispatch);
        Assert.Empty(failed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void VersionNineMigrationPreservesTheChoiceAndBacksUpTheOriginal(bool stopAll)
    {
        Directory.CreateDirectory(Path.Combine(_root, "Shortcuts"));
        string path = Path.Combine(_root, "Shortcuts", "Shortcuts.json");
        JsonObject explicitEntry = new() { ["StopAllInstances"] = stopAll, ["WaitAfterStopSecs"] = 17, ["Custom"] = "keep" };
        string original = new JsonObject { ["ShortcutFileVersion"] = "9", ["Shortcuts"] = new JsonArray(new JsonObject
        {
            ["UUID"] = "test", ["StopPrograms"] = new JsonArray(new JsonObject(), explicitEntry)
        }) }.ToJsonString();
        File.WriteAllText(path, original, Encoding.Unicode);
        ShortcutStore store = new(_root);
        Assert.Equal(new[] { false, stopAll }, Assert.Single(store.GetShortcutDefinitions()).StopPrograms.Select(entry => entry.StopAllInstances));
        using JsonDocument saved = JsonDocument.Parse(store.GetSnapshot().Json);
        Assert.Equal("10", saved.RootElement.GetProperty("ShortcutFileVersion").GetString());
        Assert.Equal("keep", saved.RootElement.GetProperty("Shortcuts")[0].GetProperty("StopPrograms")[1].GetProperty("Custom").GetString());
        Assert.Equal(original, File.ReadAllText(Assert.Single(Directory.GetFiles(Path.GetDirectoryName(path)!, "Shortcuts.json.v9.*.bak")), Encoding.Unicode));
        byte[] migrated = File.ReadAllBytes(path);
        _ = new ShortcutStore(_root);
        Assert.Equal(migrated, File.ReadAllBytes(path));
        store.ReplaceWindowPlacements("test", Array.Empty<ShortcutWindowPlacementDefinition>());
        using JsonDocument updated = JsonDocument.Parse(store.GetSnapshot().Json);
        Assert.Equal("10", updated.RootElement.GetProperty("ShortcutFileVersion").GetString());
        Assert.Equal(new[] { false, stopAll }, Assert.Single(store.GetShortcutDefinitions()).StopPrograms.Select(entry => entry.StopAllInstances));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"true\"")]
    [InlineData("1")]
    public void InvalidStopAllInstancesCannotReplaceAValidRepository(string value)
    {
        ShortcutStore store = new(_root);
        store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts,
            Json = "{\"Shortcuts\":[{\"UUID\":\"test\",\"StopPrograms\":[{\"StopAllInstances\":true}]}]}" });
        RepositorySnapshot snapshot = store.GetSnapshot();
        string original = snapshot.Json;
        string invalid = "{\"Shortcuts\":[{\"UUID\":\"test\",\"StopPrograms\":[{\"StopAllInstances\":" + value + "}]}]}";
        Assert.Throws<JsonException>(() => store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = snapshot.Revision, Json = invalid }));
        Assert.Equal(original, store.GetSnapshot().Json);
        Assert.Throws<JsonException>(() => store.GetShortcutDefinitions(invalid));
    }

    [Fact]
    public void StopAllInstancesSurvivesDesktopCopyPortableConversionAndAgentParsing()
    {
        global::DisplayMagician.ShortcutItem shortcut = new() { StopPrograms = new()
        {
            new() { Executable = "target.exe", StopAllInstances = true }
        } };
        global::DisplayMagician.ShortcutItem copied = shortcut.CreateEditingCopy();
        Assert.True(Assert.Single(copied.StopPrograms).StopAllInstances);
        using System.Drawing.Bitmap originalImage = new(1, 1);
        using System.Drawing.Bitmap shortcutImage = new(1, 1);
        using System.Drawing.Bitmap selectedImage = new(1, 1);
        shortcut.OriginalLargeBitmap = originalImage;
        shortcut.ShortcutBitmap = shortcutImage;
        shortcut.SelectedImage = new() { Image = selectedImage };
        try
        {
            Assert.True(shortcut.CopyTo(copied));
            Assert.True(Assert.Single(copied.StopPrograms).StopAllInstances);
            Assert.True(Assert.Single(copied.CreateConfigurationDefinition().StopPrograms).StopAllInstances);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(new { Shortcuts = new[] { copied } });
            Assert.True(Assert.Single(Assert.Single(new ShortcutStore(_root).GetShortcutDefinitions(json)).StopPrograms).StopAllInstances);
        }
        finally
        {
            copied.OriginalLargeBitmap?.Dispose();
            copied.ShortcutBitmap?.Dispose();
            copied.SelectedImage.Image?.Dispose();
        }
    }

    [Fact]
    public void CheckboxHasApprovedLabelAndEditsTheDraftWithoutMarkingInitialLoadUnsaved() => WinFormsTestRunner.Run(() =>
    {
        using ShortcutForm form = new(new global::DisplayMagician.ControlServicePipeClient(), (_, _, _) => { });
        using StopProgramControl control = new();
        form.Controls.Add(control);
        typeof(ShortcutForm).GetField("_loadedShortcut", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(form, true);
        FieldInfo unsaved = typeof(ShortcutForm).GetField("_isUnsaved", BindingFlags.Instance | BindingFlags.NonPublic)!;
        unsaved.SetValue(form, false);
        control.StopProgram = new() { StopAllInstances = true };
        CheckBox checkbox = (CheckBox)control.Controls["chk_stop_all_instances"]!;
        Assert.Equal("Stop all instances", checkbox.Text);
        Assert.True(checkbox.Checked);
        Assert.Equal(false, unsaved.GetValue(form));
        checkbox.Checked = false;
        Assert.False(control.StopProgram.StopAllInstances);
        Assert.Equal(true, unsaved.GetValue(form));
        control.StopProgram = new() { Disabled = true, StopAllInstances = true };
        Assert.False(checkbox.Enabled);
        Assert.True(checkbox.Checked);
        return Task.CompletedTask;
    });

    private string Executable => Path.Combine(_root, "dm-stop-review.exe");

    private Process Start()
    {
        Directory.CreateDirectory(_root);
        if (!File.Exists(Executable)) File.Copy(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe"), Executable);
        Process process = Process.Start(new ProcessStartInfo(Executable, "-n 60 127.0.0.1") { UseShellExecute = false, CreateNoWindow = true })!;
        _children.Add(process);
        return process;
    }

    public void Dispose()
    {
        foreach (Process process in _children)
        {
            try { if (!process.HasExited) { process.Kill(); process.WaitForExit(5000); } }
            finally { process.Dispose(); }
        }
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}
