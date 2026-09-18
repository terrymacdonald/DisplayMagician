using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace DisplayMagician.ControlService;

public sealed class LegacyFileMigration
{
    public LegacyFileMigrationResult MigrateFile(string legacyFilePath, string destinationFilePath, UserStoragePaths userPaths, Func<string, string>? jsonTransform = null)
    {
        if (!File.Exists(legacyFilePath))
        {
            return File.Exists(destinationFilePath)
                ? new LegacyFileMigrationResult(legacyFilePath, destinationFilePath, ValidateDestination(destinationFilePath, jsonTransform != null), "The destination was already migrated during an earlier attempt.")
                : new LegacyFileMigrationResult(legacyFilePath, destinationFilePath, false, "The legacy file does not exist.");
        }

        try
        {
            userPaths.EnsureDirectories();
            string backupPath = GetUniquePath(Path.Combine(userPaths.BackupsPath, $"{Path.GetFileName(legacyFilePath)}.{DateTime.UtcNow:yyyyMMddHHmmss}.pre-v4.bak"));
            File.Copy(legacyFilePath, backupPath, overwrite: false);

            if (jsonTransform != null)
            {
                string json = jsonTransform(File.ReadAllText(legacyFilePath));
                using JsonDocument document = JsonDocument.Parse(json);
                AtomicFileStore.WriteAllText(destinationFilePath, json, GetUniquePath(Path.Combine(userPaths.BackupsPath, $"{Path.GetFileName(destinationFilePath)}.replace.bak")));
            }
            else
            {
                byte[] content = File.ReadAllBytes(legacyFilePath);
                AtomicFileStore.WriteBytes(destinationFilePath, content, GetUniquePath(Path.Combine(userPaths.BackupsPath, $"{Path.GetFileName(destinationFilePath)}.replace.bak")));
            }

            bool destinationIsValid = ValidateDestination(destinationFilePath, jsonTransform != null);
            bool contentWasPreserved = jsonTransform != null || FilesHaveSameHash(legacyFilePath, destinationFilePath);
            if (!destinationIsValid || !contentWasPreserved)
            {
                return new LegacyFileMigrationResult(legacyFilePath, destinationFilePath, false, "The migrated destination file failed validation or did not match its legacy source.");
            }

            string retiredLegacyPath = GetRetiredLegacyPath(legacyFilePath);
            File.Move(legacyFilePath, retiredLegacyPath);

            return new LegacyFileMigrationResult(legacyFilePath, destinationFilePath, true, "Migrated and retained the legacy file with a .old suffix.", retiredLegacyPath);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Text.Json.JsonException || ex is ArgumentException || ex is NotSupportedException)
        {
            return new LegacyFileMigrationResult(legacyFilePath, destinationFilePath, false, ex.Message);
        }
    }

    private static string GetRetiredLegacyPath(string legacyFilePath)
    {
        string candidate = $"{legacyFilePath}.old";
        if (!File.Exists(candidate))
        {
            return candidate;
        }

        return $"{legacyFilePath}.{DateTime.UtcNow:yyyyMMddHHmmss}.old";
    }

    private static bool ValidateDestination(string destinationFilePath, bool requiresDisplayProfileValidation)
    {
        if (!File.Exists(destinationFilePath) || new FileInfo(destinationFilePath).Length == 0)
        {
            return false;
        }

        if (destinationFilePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            string json = File.ReadAllText(destinationFilePath);
            using JsonDocument document = JsonDocument.Parse(json);
            if (requiresDisplayProfileValidation && string.Equals(Path.GetFileName(destinationFilePath), "DisplayProfiles.json", StringComparison.OrdinalIgnoreCase))
            {
                ValidateDisplayProfiles(json);
            }

            return true;
        }

        return true;
    }

    private static void ValidateDisplayProfiles(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object ||
            !document.RootElement.TryGetProperty("Profiles", out JsonElement profiles) ||
            profiles.ValueKind != JsonValueKind.Array)
        {
            throw new System.Text.Json.JsonException("DisplayProfiles.json must contain a Profiles array.");
        }
    }

    private static bool FilesHaveSameHash(string firstPath, string secondPath)
    {
        using FileStream first = File.OpenRead(firstPath);
        using FileStream second = File.OpenRead(secondPath);
        byte[] firstHash = SHA256.HashData(first);
        byte[] secondHash = SHA256.HashData(second);
        return CryptographicOperations.FixedTimeEquals(firstHash, secondHash);
    }

    private static string GetUniquePath(string proposedPath)
    {
        if (!File.Exists(proposedPath))
        {
            return proposedPath;
        }

        return $"{proposedPath}.{Guid.NewGuid():N}";
    }
}

public sealed class LegacyFileMigrationResult
{
    public LegacyFileMigrationResult(string legacyFilePath, string destinationFilePath, bool isSuccessful, string message, string? retiredLegacyPath = null)
    {
        LegacyFilePath = legacyFilePath;
        DestinationFilePath = destinationFilePath;
        IsSuccessful = isSuccessful;
        Message = message;
        RetiredLegacyPath = retiredLegacyPath;
    }

    public string LegacyFilePath { get; }

    public string DestinationFilePath { get; }

    public bool IsSuccessful { get; }

    public string Message { get; }

    public string? RetiredLegacyPath { get; }
}
