using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class UserSupportBundleGeneratorTests
{
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
            File.WriteAllText(Path.Combine(userDataPath, "Logs", "UserAgent.log"), "agent log");
            File.WriteAllText(Path.Combine(userDataPath, "LegacyFiles", "Donation.json"), "legacy configuration");
            File.WriteAllText(Path.Combine(userDataPath, "Migration.json"), "migration");

            string destinationPath = Path.Combine(fixtureRoot, "DisplayMagician-Support.zip");
            UserSupportBundleGenerator generator = new UserSupportBundleGenerator(userDataPath, new AgentRegistration { UserSid = "S-1-5-18", Version = "4.0.0-test" });
            string machineLogsPath = Path.Combine(userDataPath, "Backups", "SupportStaging", "test", "MachineLogs");
            Directory.CreateDirectory(machineLogsPath);
            File.WriteAllText(Path.Combine(machineLogsPath, "ControlService.log"), "service log");
            UserSupportBundleResult result = generator.Create(destinationPath, machineLogsPath);

            Assert.Equal(destinationPath, result.DestinationPath);
            using ZipArchive archive = ZipFile.OpenRead(destinationPath);
            string[] entryNames = archive.Entries
                .Select(entry => entry.FullName.Replace('\\', '/'))
                .ToArray();
            Assert.Contains("Profiles/DisplayProfiles.json", entryNames);
            Assert.Contains("AudioProfiles/AudioProfiles.json", entryNames);
            Assert.Contains("Shortcuts/Shortcuts.json", entryNames);
            Assert.Contains("Settings/Settings.json", entryNames);
            Assert.Contains("Logs/DisplayMagician.log", entryNames);
            Assert.Contains("Logs/UserAgent.log", entryNames);
            Assert.Contains("MachineLogs/ControlService.log", entryNames);
            Assert.Contains("LegacyFiles/Donation.json", entryNames);
            Assert.Contains("Migration.json", entryNames);
            Assert.Contains("support-manifest.json", entryNames);
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
