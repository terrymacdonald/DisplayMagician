using System;
using System.IO;
using System.Text;
using System.Text.Json;
using DisplayMagician.ControlService;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class UserDataMigrationRunnerTests
{
    [Fact]
    public void Migrate_PreservesAllKnownDataAndRetiresEachSourceFile()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), $"DisplayMagician-V4-Migration-{Guid.NewGuid():N}");
        try
        {
            string legacyRoot = Path.Combine(fixtureRoot, "legacy");
            string storageRoot = Path.Combine(fixtureRoot, "storage");
            Directory.CreateDirectory(Path.Combine(legacyRoot, "Profiles"));
            Directory.CreateDirectory(Path.Combine(legacyRoot, "Shortcuts"));
            Directory.CreateDirectory(Path.Combine(legacyRoot, "Icons"));
            Directory.CreateDirectory(Path.Combine(legacyRoot, "Wallpaper"));
            Directory.CreateDirectory(Path.Combine(legacyRoot, "Messages", "media"));
            Directory.CreateDirectory(Path.Combine(legacyRoot, "Logs"));

            File.WriteAllText(Path.Combine(legacyRoot, "Settings.json"), "{\"SettingsFileVersion\":\"7\",\"Settings\":{}}", Encoding.Unicode);
            File.WriteAllText(Path.Combine(legacyRoot, "Donation.json"), "{\"DonationSettingsFileVersion\":\"1\",\"DonationSettings\":{}}", Encoding.Unicode);
            string legacyWallpaperPath = Path.Combine(legacyRoot, "Wallpaper", "wallpaper.jpg");
            File.WriteAllText(legacyWallpaperPath, "wallpaper");
            string profileJson = $"{{\"Profiles\":[{{\"WallpaperFilePath\":\"{legacyWallpaperPath.Replace("\\", "\\\\")}\"}}]}}";
            File.WriteAllText(Path.Combine(legacyRoot, "Profiles", "DisplayProfiles.json"), profileJson, Encoding.Unicode);
            File.WriteAllText(Path.Combine(legacyRoot, "Profiles", "profile.ico"), "profile-icon");
            File.WriteAllText(Path.Combine(legacyRoot, "Shortcuts", "Shortcuts.json"), "{\"Shortcuts\":[]}", Encoding.Unicode);
            File.WriteAllText(Path.Combine(legacyRoot, "Shortcuts", "shortcut.ico"), "shortcut-icon");
            File.WriteAllText(Path.Combine(legacyRoot, "Icons", "Steam.ico"), "steam-icon");
            File.WriteAllText(Path.Combine(legacyRoot, "Messages", "MessagesIndex.json"), "{\"Messages\":[]}", Encoding.UTF8);
            File.WriteAllText(Path.Combine(legacyRoot, "Messages", "media", "notice.png"), "media");
            File.WriteAllText(Path.Combine(legacyRoot, "Logs", "DisplayMagician.log"), "log");
            File.WriteAllText(Path.Combine(legacyRoot, "future.dat"), "future");

            StoragePaths storagePaths = new StoragePaths(storageRoot);
            UserStoragePaths userPaths = storagePaths.GetUserPaths("S-1-5-18");
            UserDataMigrationRunner runner = new UserDataMigrationRunner(new LegacyFileMigration());

            UserDataMigrationResult result = runner.Migrate(legacyRoot, userPaths);

            Assert.True(result.IsSuccessful, result.Message);
            Assert.True(File.Exists(Path.Combine(userPaths.SettingsPath, "Settings.json")));
            Assert.True(File.Exists(Path.Combine(userPaths.ProfilesPath, "profile.ico")));
            Assert.True(File.Exists(Path.Combine(userPaths.ShortcutsPath, "shortcut.ico")));
            Assert.True(File.Exists(Path.Combine(userPaths.IconsPath, "Steam.ico")));
            Assert.True(File.Exists(Path.Combine(userPaths.WallpaperPath, "wallpaper.jpg")));
            Assert.True(File.Exists(Path.Combine(userPaths.MessagesPath, "media", "notice.png")));
            Assert.True(File.Exists(Path.Combine(userPaths.LogsPath, "DisplayMagician.log")));
            Assert.True(File.Exists(Path.Combine(userPaths.LegacyFilesPath, "future.dat")));
            Assert.True(File.Exists(userPaths.MigrationMarkerPath));
            Assert.True(File.Exists(Path.Combine(legacyRoot, "Settings.json.old")));
            Assert.False(File.Exists(Path.Combine(legacyRoot, "Messages", "media", "notice.png")));

            string migratedProfileJson = File.ReadAllText(Path.Combine(userPaths.ProfilesPath, "DisplayProfiles.json"));
            using JsonDocument migratedProfile = JsonDocument.Parse(migratedProfileJson);
            string? migratedWallpaperPath = migratedProfile.RootElement
                .GetProperty("Profiles")[0]
                .GetProperty("WallpaperFilePath")
                .GetString();
            Assert.Equal(Path.Combine(userPaths.WallpaperPath, "wallpaper.jpg"), migratedWallpaperPath);
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
