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
            Directory.CreateDirectory(Path.Combine(legacyRoot, "AudioProfiles"));
            Directory.CreateDirectory(Path.Combine(legacyRoot, "Shortcuts"));
            Directory.CreateDirectory(Path.Combine(legacyRoot, "Icons"));
            Directory.CreateDirectory(Path.Combine(legacyRoot, "Wallpaper"));
            Directory.CreateDirectory(Path.Combine(legacyRoot, "Messages", "media"));
            Directory.CreateDirectory(Path.Combine(legacyRoot, "Logs"));

            File.WriteAllText(Path.Combine(legacyRoot, "Settings.json"), "{\"SettingsFileVersion\":\"7\",\"Settings\":{}}", Encoding.Unicode);
            File.WriteAllText(Path.Combine(legacyRoot, "Donation.json"), "{\"DonationSettingsFileVersion\":\"1\",\"DonationSettings\":{}}", Encoding.Unicode);
            string legacyWallpaperPath = Path.Combine(legacyRoot, "Wallpaper", "wallpaper.jpg");
            File.WriteAllText(legacyWallpaperPath, "wallpaper");
            string profileJson = $"{{\"Profiles\":[],\"LegacyWallpaperPath\":\"{legacyWallpaperPath.Replace("\\", "\\\\")}\"}}";
            File.WriteAllText(Path.Combine(legacyRoot, "Profiles", "DisplayProfiles.json"), profileJson, Encoding.Unicode);
            File.WriteAllText(Path.Combine(legacyRoot, "Profiles", "profile.ico"), "profile-icon");
            File.WriteAllText(Path.Combine(legacyRoot, "AudioProfiles", "AudioProfiles.json"), "{\"AudioProfiles\":[]}", Encoding.Unicode);
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
            Assert.True(File.Exists(Path.Combine(userPaths.AudioProfilesPath, "AudioProfiles.json")));
            Assert.True(File.Exists(Path.Combine(userPaths.ShortcutsPath, "shortcut.ico")));
            Assert.True(File.Exists(Path.Combine(userPaths.IconsPath, "Steam.ico")));
            Assert.True(File.Exists(Path.Combine(userPaths.WallpaperPath, "wallpaper.jpg")));
            Assert.True(File.Exists(Path.Combine(userPaths.MessagesPath, "media", "notice.png")));
            Assert.True(File.Exists(Path.Combine(userPaths.LogsPath, "DisplayMagician.log")));
            Assert.True(File.Exists(Path.Combine(userPaths.LegacyFilesPath, "future.dat")));
            Assert.True(File.Exists(userPaths.MigrationMarkerPath));
            Assert.True(File.Exists(Path.Combine(legacyRoot, "Settings.json.old")));
            Assert.True(File.Exists(Path.Combine(legacyRoot, "AudioProfiles", "AudioProfiles.json.old")));
            Assert.False(File.Exists(Path.Combine(legacyRoot, "Messages", "media", "notice.png")));
            Assert.Equal(
                File.ReadAllBytes(Path.Combine(legacyRoot, "Messages", "media", "notice.png.old")),
                File.ReadAllBytes(Path.Combine(userPaths.MessagesPath, "media", "notice.png")));

            string migratedProfileJson = File.ReadAllText(Path.Combine(userPaths.ProfilesPath, "DisplayProfiles.json"));
            using JsonDocument migratedProfile = JsonDocument.Parse(migratedProfileJson);
            string? migratedWallpaperPath = migratedProfile.RootElement.GetProperty("LegacyWallpaperPath").GetString();
            Assert.Equal(Path.Combine(userPaths.WallpaperPath, "wallpaper.jpg"), migratedWallpaperPath);
            byte[] migratedProfileBytes = File.ReadAllBytes(Path.Combine(userPaths.ProfilesPath, "DisplayProfiles.json"));
            Assert.Equal(0xFF, migratedProfileBytes[0]);
            Assert.Equal(0xFE, migratedProfileBytes[1]);
            Assert.Contains("Profiles", File.ReadAllText(Path.Combine(userPaths.ProfilesPath, "DisplayProfiles.json"), Encoding.Unicode));

            MigrationMarker? marker = JsonSerializer.Deserialize<MigrationMarker>(File.ReadAllText(userPaths.MigrationMarkerPath));
            Assert.NotNull(marker);
            Assert.All(marker!.Files, file => Assert.True(file.IsMigrated));
            Assert.Contains(marker.Files, file => file.DestinationFilePath == Path.Combine(userPaths.AudioProfilesPath, "AudioProfiles.json") && file.RetiredLegacyPath == Path.Combine(legacyRoot, "AudioProfiles", "AudioProfiles.json.old"));
            Assert.Contains(marker.Files, file => file.DestinationFilePath == Path.Combine(userPaths.ShortcutsPath, "Shortcuts.json") && file.RetiredLegacyPath == Path.Combine(legacyRoot, "Shortcuts", "Shortcuts.json.old"));
            Assert.Contains(marker.Files, file => file.DestinationFilePath == Path.Combine(userPaths.MessagesPath, "MessagesIndex.json") && file.RetiredLegacyPath == Path.Combine(legacyRoot, "Messages", "MessagesIndex.json.old"));
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
    public void MigrateFile_DoesNotRetireInvalidDisplayProfileSource()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), $"DisplayMagician-V4-InvalidProfile-{Guid.NewGuid():N}");
        try
        {
            string legacyProfilePath = Path.Combine(fixtureRoot, "legacy", "DisplayProfiles.json");
            Directory.CreateDirectory(Path.GetDirectoryName(legacyProfilePath)!);
            File.WriteAllText(legacyProfilePath, "{\"Profiles\":null}", Encoding.Unicode);

            StoragePaths storagePaths = new StoragePaths(Path.Combine(fixtureRoot, "storage"));
            UserStoragePaths userPaths = storagePaths.GetUserPaths("S-1-5-18");
            string destinationProfilePath = Path.Combine(userPaths.ProfilesPath, "DisplayProfiles.json");

            LegacyFileMigrationResult result = new LegacyFileMigration().MigrateFile(legacyProfilePath, destinationProfilePath, userPaths, json => json);

            Assert.False(result.IsSuccessful);
            Assert.True(File.Exists(legacyProfilePath));
            Assert.False(File.Exists($"{legacyProfilePath}.old"));
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
    public void Migrate_WhenMigrationMarkerExists_DoesNotImportLegacyDataAgain()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), $"DisplayMagician-V4-MigrationMarker-{Guid.NewGuid():N}");
        try
        {
            string legacyRoot = Path.Combine(fixtureRoot, "legacy");
            Directory.CreateDirectory(Path.Combine(legacyRoot, "Shortcuts"));
            string legacyShortcutPath = Path.Combine(legacyRoot, "Shortcuts", "Shortcuts.json");
            File.WriteAllText(legacyShortcutPath, "{\"Shortcuts\":[]}", Encoding.Unicode);

            StoragePaths storagePaths = new StoragePaths(Path.Combine(fixtureRoot, "storage"));
            UserStoragePaths userPaths = storagePaths.GetUserPaths("S-1-5-18");
            UserDataMigrationRunner runner = new UserDataMigrationRunner(new LegacyFileMigration());

            UserDataMigrationResult firstMigration = runner.Migrate(legacyRoot, userPaths);
            UserDataMigrationResult secondMigration = runner.Migrate(legacyRoot, userPaths);

            Assert.True(firstMigration.IsSuccessful, firstMigration.Message);
            Assert.False(firstMigration.WasAlreadyMigrated);
            Assert.True(secondMigration.IsSuccessful, secondMigration.Message);
            Assert.True(secondMigration.WasAlreadyMigrated);
            Assert.Empty(secondMigration.Files);
            Assert.False(File.Exists(legacyShortcutPath));
            Assert.True(File.Exists($"{legacyShortcutPath}.old"));
            Assert.True(File.Exists(Path.Combine(userPaths.ShortcutsPath, "Shortcuts.json")));
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
