using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DisplayMagician.ControlService;

public sealed class UserDataMigrationRunner
{
    private readonly LegacyFileMigration _legacyFileMigration;

    public UserDataMigrationRunner(LegacyFileMigration legacyFileMigration)
    {
        _legacyFileMigration = legacyFileMigration ?? throw new ArgumentNullException(nameof(legacyFileMigration));
    }

    public UserDataMigrationResult Migrate(string legacyAppDataPath, UserStoragePaths userPaths)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(legacyAppDataPath);
        ArgumentNullException.ThrowIfNull(userPaths);

        if (File.Exists(userPaths.MigrationMarkerPath))
        {
            return new UserDataMigrationResult(true, true, "This user's legacy data has already been migrated.", Array.Empty<LegacyFileMigrationResult>());
        }

        List<LegacyFileMigrationResult> results = new List<LegacyFileMigrationResult>();
        foreach (MigrationFile file in GetMigrationFiles(userPaths))
        {
            string sourcePath = Path.Combine(legacyAppDataPath, file.LegacyRelativePath);
            results.Add(_legacyFileMigration.MigrateJsonFile(sourcePath, file.DestinationPath, userPaths));
        }

        bool hasFailure = results.Exists(result => !result.IsSuccessful && File.Exists(result.LegacyFilePath));
        if (hasFailure)
        {
            return new UserDataMigrationResult(false, false, "One or more legacy files could not be migrated. Their source files were left in place.", results);
        }

        MigrationMarker marker = new MigrationMarker
        {
            SchemaVersion = 1,
            MigratedUtc = DateTime.UtcNow,
            LegacyAppDataPath = legacyAppDataPath,
            Files = results.ConvertAll(result => new MigrationMarkerFile
            {
                LegacyFilePath = result.LegacyFilePath,
                DestinationFilePath = result.DestinationFilePath,
                RetiredLegacyPath = result.RetiredLegacyPath,
                IsMigrated = result.IsSuccessful
            })
        };
        AtomicFileStore.WriteAllText(userPaths.MigrationMarkerPath, JsonSerializer.Serialize(marker, new JsonSerializerOptions { WriteIndented = true }));

        return new UserDataMigrationResult(true, false, "Legacy user data migration completed.", results);
    }

    private static MigrationFile[] GetMigrationFiles(UserStoragePaths userPaths)
    {
        return new[]
        {
            new MigrationFile("Settings.json", Path.Combine(userPaths.SettingsPath, "Settings.json")),
            new MigrationFile("DonationSettings.json", Path.Combine(userPaths.SettingsPath, "DonationSettings.json")),
            new MigrationFile(Path.Combine("Profiles", "DisplayProfiles.json"), Path.Combine(userPaths.ProfilesPath, "DisplayProfiles.json")),
            new MigrationFile(Path.Combine("AudioProfiles", "AudioProfiles.json"), Path.Combine(userPaths.AudioProfilesPath, "AudioProfiles.json")),
            new MigrationFile(Path.Combine("Shortcuts", "Shortcuts.json"), Path.Combine(userPaths.ShortcutsPath, "Shortcuts.json")),
            new MigrationFile(Path.Combine("Messages", "MessagesIndex.json"), Path.Combine(userPaths.MessagesPath, "MessagesIndex.json"))
        };
    }

    private sealed class MigrationFile
    {
        public MigrationFile(string legacyRelativePath, string destinationPath)
        {
            LegacyRelativePath = legacyRelativePath;
            DestinationPath = destinationPath;
        }

        public string LegacyRelativePath { get; }

        public string DestinationPath { get; }
    }
}

public sealed class UserDataMigrationResult
{
    public UserDataMigrationResult(bool isSuccessful, bool wasAlreadyMigrated, string message, IReadOnlyList<LegacyFileMigrationResult> files)
    {
        IsSuccessful = isSuccessful;
        WasAlreadyMigrated = wasAlreadyMigrated;
        Message = message;
        Files = files;
    }

    public bool IsSuccessful { get; }

    public bool WasAlreadyMigrated { get; }

    public string Message { get; }

    public IReadOnlyList<LegacyFileMigrationResult> Files { get; }
}

public sealed class MigrationMarker
{
    public int SchemaVersion { get; set; }

    public DateTime MigratedUtc { get; set; }

    public string LegacyAppDataPath { get; set; } = string.Empty;

    public List<MigrationMarkerFile> Files { get; set; } = new List<MigrationMarkerFile>();
}

public sealed class MigrationMarkerFile
{
    public string LegacyFilePath { get; set; } = string.Empty;

    public string DestinationFilePath { get; set; } = string.Empty;

    public string? RetiredLegacyPath { get; set; }

    public bool IsMigrated { get; set; }
}
