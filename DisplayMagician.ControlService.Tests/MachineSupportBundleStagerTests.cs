using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class MachineSupportBundleStagerTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-MachineSupport-{Guid.NewGuid():N}");
    private StoragePaths Paths => new StoragePaths(Path.Combine(_root, "Data"));
    private string StagePath => Path.Combine(Paths.GetUserPaths("S-1-5-18").BackupsPath, "SupportStaging", "test");

    [Fact]
    public void Stage_CollectsComponentLogsAndApprovedSnapshotsWithoutSecretState()
    {
        Paths.EnsureMachineDirectories();
        File.WriteAllText(Path.Combine(Paths.MachineLogsPath, "ControlService.log"), "service log");
        File.WriteAllText(Path.Combine(Paths.MachineLogsPath, "SessionLauncher-old.log"), "launcher log");
        File.WriteAllText(Path.Combine(Paths.MachineDiagnosticsPath, "Audit.jsonl"), "audit");
        File.WriteAllText(Path.Combine(Paths.MachinePath, "OperationStatuses.json"), "status");
        File.WriteAllText(Path.Combine(Paths.MachinePath, "OperationStatuses.json.bak"), "backup");
        File.WriteAllText(Path.Combine(Paths.MachinePath, "Replay.json"), "must exclude");
        File.WriteAllText(Path.Combine(Paths.MachinePath, "PairedClients.json"), JsonSerializer.Serialize(new[]
        {
            new PairedClient { DeviceId = "device", CredentialHash = "SECRET-HASH", PublicKeyJwk = "SECRET-KEY" }
        }));
        List<string> warnings = new List<string>();

        MachineSupportBundleStager.Stage(Paths, StagePath, warnings);

        Assert.Equal("service log", File.ReadAllText(Path.Combine(StagePath, "MachineLogs", "ControlService.log")));
        Assert.Equal("launcher log", File.ReadAllText(Path.Combine(StagePath, "MachineLogs", "SessionLauncher-old.log")));
        Assert.Equal("audit", File.ReadAllText(Path.Combine(StagePath, "MachineLogs", "Diagnostics", "Audit.jsonl")));
        string configurationPath = Path.Combine(StagePath, "Configuration", "Machine");
        Assert.Equal("backup", File.ReadAllText(Path.Combine(configurationPath, "OperationStatuses.json.bak")));
        Assert.False(File.Exists(Path.Combine(configurationPath, "Replay.json")));
        Assert.False(File.Exists(Path.Combine(configurationPath, "PairedClients.json")));
        string redacted = File.ReadAllText(Path.Combine(configurationPath, "PairedClients.redacted.json"));
        Assert.Contains("device", redacted);
        Assert.DoesNotContain("SECRET", redacted);
        Assert.Contains(warnings, warning => warning.Contains("GatewaySettings.json", StringComparison.Ordinal));
    }

    [Fact]
    public void LockedLog_DoesNotPreventCollectingOtherLogs()
    {
        Paths.EnsureMachineDirectories();
        string lockedPath = Path.Combine(Paths.MachineLogsPath, "ControlService.log");
        File.WriteAllText(lockedPath, "locked");
        File.WriteAllText(Path.Combine(Paths.MachineLogsPath, "SessionLauncher.log"), "readable");
        List<string> warnings = new List<string>();
        using (FileStream locked = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            MachineSupportBundleStager.Stage(Paths, StagePath, warnings);
        }
        Assert.Contains(warnings, warning => warning.StartsWith("ControlService.log could not be collected:", StringComparison.Ordinal));
        Assert.False(File.Exists(Path.Combine(StagePath, "MachineLogs", "ControlService.log")));
        Assert.Equal("readable", File.ReadAllText(Path.Combine(StagePath, "MachineLogs", "SessionLauncher.log")));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[null]")]
    [InlineData("{")]
    [InlineData("{}")]
    public void InvalidPairingSnapshot_IsReportedWithoutRawFallbackOrLostLogs(string json)
    {
        Paths.EnsureMachineDirectories();
        File.WriteAllText(Path.Combine(Paths.MachinePath, "PairedClients.json"), json);
        File.WriteAllText(Path.Combine(Paths.MachineLogsPath, "ControlService.log"), "readable");
        List<string> warnings = new List<string>();

        MachineSupportBundleStager.Stage(Paths, StagePath, warnings);

        Assert.Contains(warnings, warning => warning.StartsWith("PairedClients.json could not be collected", StringComparison.Ordinal));
        Assert.False(File.Exists(Path.Combine(StagePath, "Configuration", "Machine", "PairedClients.redacted.json")));
        Assert.True(File.Exists(Path.Combine(StagePath, "MachineLogs", "ControlService.log")));
        Assert.Equal(json, File.ReadAllText(Path.Combine(Paths.MachinePath, "PairedClients.json")));
    }

    [Fact]
    public void MissingMachineSources_AreReported()
    {
        List<string> warnings = new List<string>();
        MachineSupportBundleStager.Stage(Paths, StagePath, warnings);
        Assert.Contains(warnings, warning => warning.StartsWith("Machine logs folder", StringComparison.Ordinal));
        Assert.Contains(warnings, warning => warning.StartsWith("Audit.jsonl", StringComparison.Ordinal));
        Assert.Contains(warnings, warning => warning.StartsWith("OperationStatuses.json", StringComparison.Ordinal));
        Assert.Contains(warnings, warning => warning.StartsWith("PairedClients.json", StringComparison.Ordinal));
        Assert.DoesNotContain(warnings, warning => warning.Contains(".bak", StringComparison.Ordinal));
    }

    [Fact]
    public void InaccessibleOptionalBackup_IsReportedRatherThanMistakenForAbsence()
    {
        Paths.EnsureMachineDirectories();
        string backup = Path.Combine(Paths.MachinePath, "OperationStatuses.json.bak");
        File.WriteAllText(backup, "retained backup");
        List<string> warnings = new List<string>();
        using (FileStream locked = new FileStream(backup, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            MachineSupportBundleStager.Stage(Paths, StagePath, warnings);
        }
        Assert.Contains(warnings, warning => warning.StartsWith("OperationStatuses.json.bak could not be collected:", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LinkedLogSubdirectory_IsSkippedWithoutFollowingExternalFilesOrCycles(bool cycle)
    {
        Paths.EnsureMachineDirectories();
        string external = Directory.CreateDirectory(Path.Combine(_root, "External")).FullName;
        File.WriteAllText(Path.Combine(external, "private.log"), "private");
        string link = Path.Combine(Paths.MachineLogsPath, "Link");
        CreateJunction(link, cycle ? Paths.MachineLogsPath : external);
        try
        {
            List<string> warnings = new List<string>();
            MachineSupportBundleStager.Stage(Paths, StagePath, warnings);
            Assert.Contains(warnings, warning => warning.Contains("filesystem link", StringComparison.Ordinal));
            Assert.False(Directory.Exists(Path.Combine(StagePath, "MachineLogs", "Link")));
        }
        finally { Directory.Delete(link); }
    }

    [Fact]
    public void LinkedStagingAncestor_RejectsWritesAndCleanupPreservesExternalFiles()
    {
        string backups = Paths.GetUserPaths("S-1-5-18").BackupsPath;
        Directory.CreateDirectory(backups);
        string external = Directory.CreateDirectory(Path.Combine(_root, "External")).FullName;
        Directory.CreateDirectory(Path.Combine(external, "test"));
        string sentinel = Path.Combine(external, "test", "private.txt");
        File.WriteAllText(sentinel, "preserve");
        string link = Path.Combine(backups, "SupportStaging");
        CreateJunction(link, external);
        try
        {
            Assert.Throws<IOException>(() => MachineSupportBundleStager.Stage(Paths, StagePath, new List<string>()));
            MethodInfo cleanup = typeof(ControlClientPipeServer).GetMethod("DeleteSupportStagingDirectory", BindingFlags.NonPublic | BindingFlags.Static)!;
            cleanup.Invoke(null, new object[] { StagePath });
            Assert.Equal("preserve", File.ReadAllText(sentinel));
            Assert.False(Directory.Exists(Path.Combine(external, "test", "MachineLogs")));
        }
        finally { Directory.Delete(link); }
    }

    [Fact]
    public void ExistingDestination_IsNotOverwrittenOrTreatedAsMissingSource()
    {
        Directory.CreateDirectory(_root);
        string source = Path.Combine(_root, "source.log");
        string destination = Path.Combine(_root, "destination.log");
        File.WriteAllText(source, "new");
        File.WriteAllText(destination, "preserve");
        List<string> warnings = new List<string>();
        Assert.Throws<IOException>(() => MachineSupportBundleStager.CopyFile(source, destination, warnings));
        Assert.Equal("preserve", File.ReadAllText(destination));
        Assert.Empty(warnings);
    }

    private static void CreateJunction(string link, string target)
    {
        using Process process = Process.Start(new ProcessStartInfo("cmd.exe")
        {
            Arguments = $"/d /c mklink /J \"{link}\" \"{target}\"", UseShellExecute = false,
            CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true
        })!;
        string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        Assert.True(process.WaitForExit(10000), "Junction creation timed out.");
        Assert.True(process.ExitCode == 0, output);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}
