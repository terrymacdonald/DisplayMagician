using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class UserSupportBundleGeneratorTests
{
    [Fact]
    public void Create_ReturnsAWarningAndContinuesWhenALogFileIsLocked()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), $"DisplayMagician-SupportBundle-{Guid.NewGuid():N}");
        try
        {
            string userDataPath = Path.Combine(fixtureRoot, "UserData");
            string logsPath = Path.Combine(userDataPath, "Logs");
            Directory.CreateDirectory(logsPath);
            string lockedLogPath = Path.Combine(logsPath, "UserAgent-locked.log");
            File.WriteAllText(lockedLogPath, "active log");
            string destinationPath = Path.Combine(fixtureRoot, "DisplayMagician-Support.zip");
            UserSupportBundleGenerator generator = new UserSupportBundleGenerator(userDataPath, new AgentRegistration { UserSid = "S-1-5-18", Version = "4.0.0-test" }, Path.Combine(fixtureRoot, "NoLegacyLogs"));

            using (FileStream lockedLog = new FileStream(lockedLogPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                UserSupportBundleResult result = generator.Create(destinationPath, Path.Combine(userDataPath, "Backups", "SupportStaging", "Missing"));

                Assert.Contains(result.Warnings, warning => warning.StartsWith($"Logs{Path.DirectorySeparatorChar}UserAgent-locked.log could not be added:", StringComparison.Ordinal));
            }

            using ZipArchive archive = ZipFile.OpenRead(destinationPath);
            Assert.Null(archive.GetEntry("Logs/UserAgent-locked.log"));
        }
        finally
        {
            if (Directory.Exists(fixtureRoot))
            {
                Directory.Delete(fixtureRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void Create_IncludesAllUserLogsAndConfigurationFiles()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), $"DisplayMagician-SupportBundle-{Guid.NewGuid():N}");
        try
        {
            string userDataPath = Path.Combine(fixtureRoot, "UserData");
            Directory.CreateDirectory(Path.Combine(userDataPath, "Profiles"));
            Directory.CreateDirectory(Path.Combine(userDataPath, "AudioProfiles"));
            Directory.CreateDirectory(Path.Combine(userDataPath, "Shortcuts"));
            Directory.CreateDirectory(Path.Combine(userDataPath, "Settings"));
            Directory.CreateDirectory(Path.Combine(userDataPath, "Logs"));
            Directory.CreateDirectory(Path.Combine(userDataPath, "LegacyFiles"));
            File.WriteAllText(Path.Combine(userDataPath, "Profiles", "DisplayProfiles.json"), "profiles");
            File.WriteAllText(Path.Combine(userDataPath, "AudioProfiles", "AudioProfiles.json"), "audio");
            File.WriteAllText(Path.Combine(userDataPath, "Shortcuts", "Shortcuts.json"), "shortcuts");
            File.WriteAllText(Path.Combine(userDataPath, "Settings", "Settings.json"), "settings");
            File.WriteAllText(Path.Combine(userDataPath, "Logs", "DisplayMagician.log"), "desktop log");
            File.WriteAllText(Path.Combine(userDataPath, "Logs", "DesktopConsole-20260922.log"), "console log");
            File.WriteAllText(Path.Combine(userDataPath, "Logs", "UserAgent.log"), "agent log");
            File.WriteAllText(Path.Combine(userDataPath, "LegacyFiles", "Donation.json"), "legacy configuration");
            File.WriteAllText(Path.Combine(userDataPath, "Migration.json"), "migration");
            string legacyLogPath = Path.Combine(fixtureRoot, "LegacyLogs");
            Directory.CreateDirectory(legacyLogPath);
            File.WriteAllText(Path.Combine(legacyLogPath, "DisplayMagician.log"), "legacy desktop log");

            string destinationPath = Path.Combine(fixtureRoot, "DisplayMagician-Support.zip");
            UserSupportBundleGenerator generator = new UserSupportBundleGenerator(userDataPath, new AgentRegistration { UserSid = "S-1-5-18", Version = "4.0.0-test" }, legacyLogPath);
            string machineLogsPath = Path.Combine(userDataPath, "Backups", "SupportStaging", "test", "MachineLogs");
            Directory.CreateDirectory(machineLogsPath);
            File.WriteAllText(Path.Combine(machineLogsPath, "ControlService.log"), "service log");
            File.WriteAllText(Path.Combine(machineLogsPath, "SessionLauncher-20260922.log"), "session launcher log");
            string machineConfigurationPath = Path.Combine(userDataPath, "Backups", "SupportStaging", "test", "Configuration", "Machine");
            Directory.CreateDirectory(machineConfigurationPath);
            File.WriteAllText(Path.Combine(machineConfigurationPath, "ScheduleState.json"), "schedule state");
            UserSupportBundleResult result = generator.Create(destinationPath, machineLogsPath, machineConfigurationPath, new[] { "The last installer transaction log was no longer available." });

            Assert.Equal(destinationPath, result.DestinationPath);
            Assert.Contains("The last installer transaction log was no longer available.", result.Warnings);
            using ZipArchive archive = ZipFile.OpenRead(destinationPath);
            string[] entryNames = archive.Entries
                .Select(entry => entry.FullName.Replace('\\', '/'))
                .ToArray();
            Assert.Contains("Configuration/Profiles/DisplayProfiles.json", entryNames);
            Assert.Contains("Configuration/AudioProfiles/AudioProfiles.json", entryNames);
            Assert.Contains("Configuration/Shortcuts/Shortcuts.json", entryNames);
            Assert.Contains("Configuration/Settings/Settings.json", entryNames);
            Assert.Contains("Logs/DisplayMagician.log", entryNames);
            Assert.Contains("Logs/DesktopConsole-20260922.log", entryNames);
            Assert.Contains("Logs/UserAgent.log", entryNames);
            Assert.Contains("MachineLogs/ControlService.log", entryNames);
            Assert.Contains("MachineLogs/SessionLauncher-20260922.log", entryNames);
            Assert.Contains("Configuration/Machine/ScheduleState.json", entryNames);
            Assert.Contains("Configuration/LegacyFiles/Donation.json", entryNames);
            Assert.Contains("Logs/Legacy/DisplayMagician.log", entryNames);
            Assert.Contains("Configuration/Migration.json", entryNames);
            Assert.Contains("support-manifest.json", entryNames);
            ZipArchiveEntry manifestEntry = archive.GetEntry("support-manifest.json")!;
            using StreamReader manifestReader = new StreamReader(manifestEntry.Open());
            using JsonDocument manifest = JsonDocument.Parse(manifestReader.ReadToEnd());
            Assert.Equal(1, manifest.RootElement.GetProperty("LogContractVersion").GetInt32());
            Assert.Equal("4.0.0-test", manifest.RootElement.GetProperty("ComponentVersions").GetProperty("UserAgent").GetString());
            Assert.Contains(manifest.RootElement.GetProperty("Warnings").EnumerateArray(), warning => warning.GetString() == "The last installer transaction log was no longer available.");
            Assert.Contains(manifest.RootElement.GetProperty("IncludedEntries").EnumerateArray(), entry => entry.GetProperty("Path").GetString() == "MachineLogs/ControlService.log");
            Assert.Contains(manifest.RootElement.GetProperty("IncludedEntries").EnumerateArray(), entry => entry.GetProperty("Path").GetString() == "Logs/DesktopConsole-20260922.log");
            Assert.Contains(manifest.RootElement.GetProperty("IncludedEntries").EnumerateArray(), entry => entry.GetProperty("Path").GetString() == "MachineLogs/SessionLauncher-20260922.log");
            Assert.Contains(manifest.RootElement.GetProperty("IncludedEntries").EnumerateArray(), entry => entry.GetProperty("Path").GetString() == "Configuration/Machine/ScheduleState.json");
            JsonElement consoleLog = manifest.RootElement.GetProperty("IncludedEntries").EnumerateArray().Single(entry => entry.GetProperty("Path").GetString() == "Logs/DesktopConsole-20260922.log");
            Assert.Equal("Log", consoleLog.GetProperty("Category").GetString());
            Assert.Equal("DesktopConsole", consoleLog.GetProperty("Component").GetString());
            Assert.Equal("logfmt-v1", consoleLog.GetProperty("Format").GetString());
            Assert.Equal("Included", consoleLog.GetProperty("CollectionResult").GetString());
            JsonElement machineConfiguration = manifest.RootElement.GetProperty("IncludedEntries").EnumerateArray().Single(entry => entry.GetProperty("Path").GetString() == "Configuration/Machine/ScheduleState.json");
            Assert.Equal("Configuration", machineConfiguration.GetProperty("Category").GetString());
            Assert.Equal("ControlService", machineConfiguration.GetProperty("Component").GetString());
            Assert.Equal("json", machineConfiguration.GetProperty("Format").GetString());
            JsonElement legacyLog = manifest.RootElement.GetProperty("IncludedEntries").EnumerateArray().Single(entry => entry.GetProperty("Path").GetString() == "Logs/Legacy/DisplayMagician.log");
            Assert.Equal("LegacyLog", legacyLog.GetProperty("Category").GetString());
            Assert.Equal("DesktopApp", legacyLog.GetProperty("Component").GetString());
            Assert.Equal("legacy-text", legacyLog.GetProperty("Format").GetString());
        }
        finally
        {
            if (Directory.Exists(fixtureRoot))
            {
                Directory.Delete(fixtureRoot, recursive: true);
            }
        }
    }
}
