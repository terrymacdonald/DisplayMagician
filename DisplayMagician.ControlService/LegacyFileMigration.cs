using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DisplayMagician.ControlService;

public sealed class LegacyFileMigration
{
    public LegacyFileMigrationResult MigrateJsonFile(string legacyFilePath, string destinationFilePath, UserStoragePaths userPaths)
    {
        if (!File.Exists(legacyFilePath))
        {
            return new LegacyFileMigrationResult(legacyFilePath, destinationFilePath, false, "The legacy file does not exist.");
        }

        try
        {
            userPaths.EnsureDirectories();
            string backupPath = Path.Combine(userPaths.BackupsPath, $"{Path.GetFileName(legacyFilePath)}.{DateTime.UtcNow:yyyyMMddHHmmss}.pre-v4.bak");
            File.Copy(legacyFilePath, backupPath, overwrite: false);

            string json = File.ReadAllText(legacyFilePath);
            using JsonDocument document = JsonDocument.Parse(json);
            AtomicFileStore.WriteAllText(destinationFilePath, json, Path.Combine(userPaths.BackupsPath, $"{Path.GetFileName(destinationFilePath)}.replace.bak"));

            using JsonDocument destinationDocument = JsonDocument.Parse(File.ReadAllText(destinationFilePath));
            string retiredLegacyPath = GetRetiredLegacyPath(legacyFilePath);
            File.Move(legacyFilePath, retiredLegacyPath);

            return new LegacyFileMigrationResult(legacyFilePath, destinationFilePath, true, "Migrated and retained the legacy file with a .old suffix.", retiredLegacyPath);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is ArgumentException || ex is NotSupportedException)
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
