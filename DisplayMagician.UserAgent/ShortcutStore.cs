using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Contracts;

namespace DisplayMagician.UserAgent;

/// <summary>Agent-owned persistence boundary for shortcut definitions. Runtime execution moves to ShortcutRunner separately.</summary>
public sealed class ShortcutStore
{
    private readonly string _shortcutFilePath;

    public ShortcutStore(string userDataPath)
    {
        _shortcutFilePath = Path.Combine(userDataPath, "Shortcuts", "Shortcuts.json");
    }

    public RepositorySnapshot GetSnapshot()
    {
        byte[] content = File.Exists(_shortcutFilePath) ? File.ReadAllBytes(_shortcutFilePath) : Array.Empty<byte>();
        return new RepositorySnapshot { Repository = RepositoryKind.Shortcuts, Revision = content.Length == 0 ? 0 : BitConverter.ToInt64(SHA256.HashData(content), 0), Json = content.Length == 0 ? string.Empty : Encoding.Unicode.GetString(content).TrimStart('\uFEFF') };
    }

    public bool TryGetShortcutDefinition(string shortcutId, out ShortcutDefinition? shortcutDefinition)
    {
        shortcutDefinition = null;
        if (string.IsNullOrWhiteSpace(shortcutId))
        {
            return false;
        }

        RepositorySnapshot snapshot = GetSnapshot();
        if (string.IsNullOrWhiteSpace(snapshot.Json))
        {
            return false;
        }

        using JsonDocument document = JsonDocument.Parse(snapshot.Json);
        if (!document.RootElement.TryGetProperty("Shortcuts", out JsonElement shortcuts) || shortcuts.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (JsonElement shortcut in shortcuts.EnumerateArray())
        {
            string id = GetString(shortcut, "UUID");
            if (!string.Equals(id, shortcutId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int category = GetInt32(shortcut, "Category");
            int launchMode = GetInt32(shortcut, "GameLaunchMode");
            shortcutDefinition = new ShortcutDefinition
            {
                Id = id,
                Name = GetString(shortcut, "Name"),
                Category = Enum.IsDefined(typeof(ShortcutDefinitionCategory), category) ? (ShortcutDefinitionCategory)category : ShortcutDefinitionCategory.Unknown,
                GameAppId = GetString(shortcut, "GameAppId"),
                GameName = GetString(shortcut, "GameName"),
                GameLaunchMode = Enum.IsDefined(typeof(GameLaunchMode), launchMode) ? (GameLaunchMode)launchMode : GameLaunchMode.StartGame
            };
            return true;
        }

        return false;
    }

    public RepositoryCommitResult Commit(RepositoryCommitRequest request)
    {
        RepositorySnapshot snapshot = GetSnapshot();
        if (request.ExpectedRevision != snapshot.Revision)
        {
            return new RepositoryCommitResult { WasConflict = true, Snapshot = snapshot };
        }

        using JsonDocument _ = JsonDocument.Parse(request.Json);
        string directory = Path.GetDirectoryName(_shortcutFilePath)!;
        Directory.CreateDirectory(directory);
        string temporaryPath = Path.Combine(directory, $".Shortcuts.{Guid.NewGuid():N}.tmp");
        File.WriteAllText(temporaryPath, request.Json, Encoding.Unicode);
        if (File.Exists(_shortcutFilePath)) File.Replace(temporaryPath, _shortcutFilePath, null); else File.Move(temporaryPath, _shortcutFilePath);
        return new RepositoryCommitResult { Snapshot = GetSnapshot() };
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static int GetInt32(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out JsonElement property) && property.TryGetInt32(out int value)
            ? value
            : 0;
    }
}
