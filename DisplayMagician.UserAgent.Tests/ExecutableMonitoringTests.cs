using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Contracts;
using DisplayMagician.Processes;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ExecutableMonitoringTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MonitorLaunchedExecutableUsesTheCanonicalJsonNameAndSelectsTheIntendedExecutable(bool launched)
    {
        ShortcutDefinition definition = new ShortcutDefinition
        {
            MonitorLaunchedExecutable = launched, ExecutablePath = "launch.exe", DifferentExecutablePathToMonitor = "target.exe"
        };
        string systemJson = System.Text.Json.JsonSerializer.Serialize(definition);
        string newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(definition);
        foreach (string json in new[] { systemJson, newtonsoftJson })
        {
            Assert.Contains("\"MonitorLaunchedExecutable\":", json);
            Assert.DoesNotContain("\"MonitorExecutablePath\":", json);
            Assert.DoesNotContain("\"ProcessNameToMonitorUsesExecutable\":", json);
            Assert.Equal(launched, System.Text.Json.JsonSerializer.Deserialize<ShortcutDefinition>(json)!.MonitorLaunchedExecutable);
            Assert.Equal(launched, Newtonsoft.Json.JsonConvert.DeserializeObject<ShortcutDefinition>(json)!.MonitorLaunchedExecutable);
        }
        Assert.Equal(launched ? "launch.exe" : "target.exe", ShortcutRunner.GetExecutableMonitorPath(definition));
        ShortcutItem savedShortcut = new ShortcutItem { MonitorLaunchedExecutable = launched };
        string savedJson = Newtonsoft.Json.JsonConvert.SerializeObject(savedShortcut);
        Assert.Contains("\"MonitorLaunchedExecutable\":", savedJson);
        Assert.DoesNotContain("\"ProcessNameToMonitorUsesExecutable\":", savedJson);
        Assert.DoesNotContain("\"MonitorExecutablePath\":", savedJson);
        Assert.Equal(launched, Newtonsoft.Json.JsonConvert.DeserializeObject<ShortcutItem>(savedJson)!.MonitorLaunchedExecutable);
        Assert.Equal(launched, savedShortcut.CreateConfigurationDefinition().MonitorLaunchedExecutable);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ShortcutWaitsForItsSelectedNewProcessAndIgnoresTheOldCopy(bool monitorLaunchedExecutable)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianMonitoringTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string launcher = Path.Combine(root, "dm-launcher.exe");
        string target = Path.Combine(root, "dm-target.exe");
        string cmd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
        File.Copy(cmd, launcher);
        File.Copy(cmd, target);
        string selected = monitorLaunchedExecutable ? launcher : target;
        using CancellationTokenSource cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(25));
        using Process old = Start(selected);
        Task<ShortcutRunResult>? run = null;
        Process? brokerTarget = null;
        try
        {
            ShortcutStore store = new ShortcutStore(root);
            store.Commit(new RepositoryCommitRequest
            {
                Repository = RepositoryKind.Shortcuts, ExpectedRevision = 0,
                Json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Shortcuts = new[] { new
                    {
                        UUID = "monitor-test", Category = 0, ExecutableNameAndPath = launcher,
                        ExecutableArguments = monitorLaunchedExecutable ? "/d /c ping -n 30 127.0.0.1 >nul" : "/d /c exit",
                        MonitorLaunchedExecutable = monitorLaunchedExecutable,
                        DifferentExecutableToMonitor = monitorLaunchedExecutable ? Path.Combine(root, "stale-missing-target.exe") : target,
                        StartTimeout = 10
                    }}
                })
            });
            ShortcutRunner runner = new ShortcutRunner(store, new AutomaticGameDetectionRegistry(), new UserProfileOperationService(), new ShortcutRecoveryStore(root));
            TaskCompletionSource waitingForStart = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource waitingForClose = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            run = runner.ApplyShortcutProfilesAsync("monitor-test", 0, cancellation.Token, (status, _) =>
            {
                if (status.Phase == OperationPhase.WaitingForGameToStart) waitingForStart.TrySetResult();
                if (status.Phase == OperationPhase.WaitingForGameToClose) waitingForClose.TrySetResult();
                return Task.CompletedTask;
            });
            if (!monitorLaunchedExecutable)
            {
                await waitingForStart.Task.WaitAsync(TimeSpan.FromSeconds(15));
                Assert.False(run.IsCompleted);
                // This process is started by the test host, not by the exited launcher:
                // it exercises a broker/service handover outside the launcher's tree.
                brokerTarget = Start(target);
            }
            await waitingForClose.Task.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.False(run.IsCompleted);
            ProcessIdentity selectedIdentity = Assert.Single(ProcessTreeMonitor.FindRunningProcesses(selected).Identities, identity => identity.ProcessId != old.Id);
            using (Process selectedProcess = Process.GetProcessById(selectedIdentity.ProcessId))
            {
                selectedProcess.Kill(entireProcessTree: true);
                selectedProcess.WaitForExit(5000);
            }
            ShortcutRunResult result = await run.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal(ShortcutRunOutcome.Completed, result.Outcome);
            Assert.False(old.HasExited);
        }
        finally
        {
            cancellation.Cancel();
            try { if (run != null) await run.WaitAsync(TimeSpan.FromSeconds(10)); }
            finally
            {
                brokerTarget?.Dispose();
                foreach (string executable in new[] { launcher, target })
                {
                    foreach (ProcessIdentity identity in ProcessTreeMonitor.FindRunningProcesses(executable).Identities)
                    {
                        try
                        {
                            using Process process = Process.GetProcessById(identity.ProcessId);
                            if (!process.HasExited && process.StartTime.ToUniversalTime() == identity.StartTimeUtc)
                            {
                                process.Kill(entireProcessTree: true);
                                process.WaitForExit(5000);
                            }
                        }
                        catch (ArgumentException) { /* The disposable test process exited. */ }
                    }
                }
                Directory.Delete(root, true);
            }
        }
    }

    private static Process Start(string executable) => Process.Start(new ProcessStartInfo(executable, "/d /c ping -n 30 127.0.0.1 >nul")
        { UseShellExecute = false, CreateNoWindow = true })!;
}
