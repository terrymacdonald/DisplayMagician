using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DisplayMagician.ControlService;

public sealed class UserDataMigrationRunner
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly LegacyFileMigration _legacyFileMigration;
    private readonly object _migrationLock = new object();

    public UserDataMigrationRunner(LegacyFileMigration legacyFileMigration)
    {
        _legacyFileMigration = legacyFileMigration ?? throw new ArgumentNullException(nameof(legacyFileMigration));
    }

    public UserDataMigrationResult Migrate(string legacyAppDataPath, UserStoragePaths userPaths)
    {
        lock (_migrationLock)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(legacyAppDataPath);
            ArgumentNullException.ThrowIfNull(userPaths);

            if (File.Exists(userPaths.MigrationMarkerPath) || Directory.Exists(userPaths.MigrationMarkerPath))
            {
                try
                {
                    using JsonDocument markerDocument = JsonDocument.Parse(File.ReadAllText(userPaths.MigrationMarkerPath));
                    if (markerDocument.RootElement.ValueKind != JsonValueKind.Object ||
                        !markerDocument.RootElement.TryGetProperty("Files", out JsonElement markerFiles) || markerFiles.ValueKind != JsonValueKind.Array)
                        throw new JsonException("The migration marker must contain its completed file list.");
                    MigrationMarker? existing = markerDocument.RootElement.Deserialize<MigrationMarker>();
                    if (existing == null || existing.SchemaVersion != 1 || existing.MigratedUtc == default ||
                        string.IsNullOrWhiteSpace(existing.LegacyAppDataPath) || existing.Files == null ||
                        existing.Files.Any(file => file == null || !file.IsMigrated || string.IsNullOrWhiteSpace(file.LegacyFilePath) || string.IsNullOrWhiteSpace(file.DestinationFilePath)))
                        throw new JsonException("The migration marker is incomplete or uses an unsupported schema.");
                    return new UserDataMigrationResult(true, true, "This user's legacy data has already been migrated.", Array.Empty<LegacyFileMigrationResult>());
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
                {
                    Logger.Warn(ex, "UserDataMigrationRunner/Migrate: Could not validate migration marker {0}; import was stopped.", userPaths.MigrationMarkerPath);
                    return new UserDataMigrationResult(false, false, $"The migration marker at {userPaths.MigrationMarkerPath} could not be validated. Repair it before retrying migration. Existing files were preserved.", Array.Empty<LegacyFileMigrationResult>());
                }
            }

            List<LegacyFileMigrationResult> results = new List<LegacyFileMigrationResult>();
            foreach (MigrationFile file in GetMigrationFiles(legacyAppDataPath, userPaths))
            {
                results.Add(_legacyFileMigration.MigrateFile(file.LegacyFilePath, file.DestinationFilePath, userPaths, file.JsonTransform));
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
    }

    private static List<MigrationFile> GetMigrationFiles(string legacyAppDataPath, UserStoragePaths userPaths)
    {
        List<MigrationFile> files = new List<MigrationFile>();
        AddRootConfigurationFiles(files, legacyAppDataPath, userPaths);
        AddDirectoryFiles(files, Path.Combine(legacyAppDataPath, "Profiles"), userPaths.ProfilesPath, legacyAppDataPath, userPaths, transformDisplayProfileJson: true);
        AddDirectoryFiles(files, Path.Combine(legacyAppDataPath, "AudioProfiles"), userPaths.AudioProfilesPath, legacyAppDataPath, userPaths);
        AddDirectoryFiles(files, Path.Combine(legacyAppDataPath, "Shortcuts"), userPaths.ShortcutsPath, legacyAppDataPath, userPaths);
        AddDirectoryFiles(files, Path.Combine(legacyAppDataPath, "Icons"), userPaths.IconsPath, legacyAppDataPath, userPaths);
        AddDirectoryFiles(files, Path.Combine(legacyAppDataPath, "Wallpaper"), userPaths.WallpaperPath, legacyAppDataPath, userPaths);
        AddDirectoryFiles(files, Path.Combine(legacyAppDataPath, "Messages"), userPaths.MessagesPath, legacyAppDataPath, userPaths);
        //AddDirectoryFiles(files, Path.Combine(legacyAppDataPath, "Logs"), userPaths.LogsPath, legacyAppDataPath, userPaths);
        return files;
    }

    private static void AddRootConfigurationFiles(List<MigrationFile> files, string legacyAppDataPath, UserStoragePaths userPaths)
    {
        if (!Directory.Exists(legacyAppDataPath))
        {
            return;
        }

        foreach (string filePath in Directory.GetFiles(legacyAppDataPath, "*", SearchOption.TopDirectoryOnly))
        {
            string fileName = Path.GetFileName(filePath);

            // Ignore any .old backed up files!
            if (fileName.EndsWith(".old", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (fileName.StartsWith("Settings", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("Donation", StringComparison.OrdinalIgnoreCase))
            {
                files.Add(new MigrationFile(filePath, Path.Combine(userPaths.SettingsPath, fileName)));
            }
            else
            {
                files.Add(new MigrationFile(filePath, Path.Combine(userPaths.LegacyFilesPath, fileName)));
            }
        }
    }

    private static void AddDirectoryFiles(List<MigrationFile> files, string legacyDirectoryPath, string destinationDirectoryPath, string legacyAppDataPath, UserStoragePaths userPaths, bool transformDisplayProfileJson = false)
    {
        if (!Directory.Exists(legacyDirectoryPath))
        {
            return;
        }

        foreach (string legacyFilePath in Directory.GetFiles(legacyDirectoryPath, "*", SearchOption.AllDirectories))
        {
            // Retired sources from a partial migration must remain backups, not new imports.
            if (legacyFilePath.EndsWith(".old", StringComparison.OrdinalIgnoreCase))
                continue;
            string relativePath = Path.GetRelativePath(legacyDirectoryPath, legacyFilePath);
            Func<string, string>? jsonTransform = transformDisplayProfileJson && string.Equals(relativePath, "DisplayProfiles.json", StringComparison.OrdinalIgnoreCase)
                ? json => RewriteWallpaperPaths(json, Path.Combine(legacyAppDataPath, "Wallpaper"), userPaths.WallpaperPath)
                : null;
            files.Add(new MigrationFile(legacyFilePath, Path.Combine(destinationDirectoryPath, relativePath), jsonTransform));
        }
    }

    private static string RewriteWallpaperPaths(string json, string legacyWallpaperPath, string destinationWallpaperPath)
    {
        JsonNode? root = JsonNode.Parse(json);
        if (root == null)
        {
            throw new JsonException("The display profile file did not contain JSON content.");
        }

        RewriteWallpaperPaths(root, legacyWallpaperPath, destinationWallpaperPath);
        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private static JsonNode? RewriteWallpaperPaths(JsonNode? node, string legacyWallpaperPath, string destinationWallpaperPath)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (KeyValuePair<string, JsonNode?> property in jsonObject.ToList())
            {
                JsonNode? rewritten = RewriteWallpaperPaths(property.Value, legacyWallpaperPath, destinationWallpaperPath);
                if (!ReferenceEquals(property.Value, rewritten))
                {
                    jsonObject[property.Key] = rewritten;
                }
            }

            return jsonObject;
        }

        if (node is JsonArray jsonArray)
        {
            for (int index = 0; index < jsonArray.Count; index++)
            {
                JsonNode? original = jsonArray[index];
                JsonNode? rewritten = RewriteWallpaperPaths(original, legacyWallpaperPath, destinationWallpaperPath);
                if (!ReferenceEquals(original, rewritten))
                {
                    jsonArray[index] = rewritten;
                }
            }

            return jsonArray;
        }

        if (node is JsonValue jsonValue && jsonValue.TryGetValue<string>(out string? value) && !string.IsNullOrEmpty(value))
        {
            string rewrittenValue = value.Replace(legacyWallpaperPath, destinationWallpaperPath, StringComparison.OrdinalIgnoreCase);
            return string.Equals(value, rewrittenValue, StringComparison.Ordinal) ? node : JsonValue.Create(rewrittenValue);
        }

        return node;
    }

    private sealed class MigrationFile
    {
        public MigrationFile(string legacyFilePath, string destinationFilePath, Func<string, string>? jsonTransform = null)
        {
            LegacyFilePath = legacyFilePath;
            DestinationFilePath = destinationFilePath;
            JsonTransform = jsonTransform;
        }

        public string LegacyFilePath { get; }

        public string DestinationFilePath { get; }

        public Func<string, string>? JsonTransform { get; }
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
