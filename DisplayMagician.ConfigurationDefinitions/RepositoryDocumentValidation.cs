using System;
using System.Collections.Generic;
using System.Text.Json;
using DisplayMagician.Contracts;

namespace DisplayMagician.ConfigurationDefinitions;

/// <summary>Validates the portable repository envelope without loading hardware or discarding unknown fields.</summary>
public static class RepositoryDocumentValidation
{
    public static void Validate(string json, RepositoryKind repository)
    {
        string collectionName = repository switch
        {
            RepositoryKind.DisplayProfiles => "Profiles",
            RepositoryKind.AudioProfiles => "AudioProfiles",
            RepositoryKind.Shortcuts => "Shortcuts",
            _ => throw new ArgumentOutOfRangeException(nameof(repository))
        };
        using JsonDocument document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object ||
            !document.RootElement.TryGetProperty(collectionName, out JsonElement items) || items.ValueKind != JsonValueKind.Array)
            throw new JsonException($"The repository must contain a {collectionName} array.");

        HashSet<string> identities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (repository == RepositoryKind.Shortcuts) ValidateShortcutFileVersion(document.RootElement);
        foreach (JsonElement item in items.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("UUID", out JsonElement id) ||
                id.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(id.GetString()) || !identities.Add(id.GetString()!))
                throw new JsonException("Repository entries must have distinct, nonempty UUID strings.");

            if (repository == RepositoryKind.Shortcuts)
            {
                foreach (string programCollectionName in new[] { "StartPrograms", "StopPrograms" })
                {
                    if (!item.TryGetProperty(programCollectionName, out JsonElement programs) || programs.ValueKind == JsonValueKind.Null) continue;
                    if (programs.ValueKind != JsonValueKind.Array) throw new JsonException($"{programCollectionName} must be an array.");
                    string delayName = programCollectionName == "StopPrograms" ? "WaitAfterStopSecs" : "WaitAfterStartSecs";
                    foreach (JsonElement program in programs.EnumerateArray())
                    {
                        if (program.ValueKind != JsonValueKind.Object) throw new JsonException($"{programCollectionName} entries must be objects.");
                        if (programCollectionName == "StopPrograms" && program.TryGetProperty("StopAllInstances", out JsonElement allInstances) &&
                            allInstances.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                            throw new JsonException("StopAllInstances must be a boolean.");
                        if (!program.TryGetProperty(delayName, out JsonElement delay)) continue;
                        if (delay.ValueKind != JsonValueKind.Number || !delay.TryGetInt32(out int seconds) || seconds < 0 || seconds > 999)
                            throw new JsonException($"{delayName} must be an integer between 0 and 999 seconds.");
                    }
                }
            }
        }
    }

    public static void ValidateShortcutFileVersion(JsonElement root)
    {
        // Unversioned/blank files are supported legacy inputs; migration owns their upgrade.
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("ShortcutFileVersion", out JsonElement version) || version.ValueKind == JsonValueKind.Null) return;
        if (version.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(version.GetString())) return;
        if (version.ValueKind != JsonValueKind.String || !int.TryParse(version.GetString(), out int number) || number < 1 || number > ShortcutDefinition.CurrentFileVersion)
            throw new JsonException($"Unsupported shortcut file version. Supported versions are 1 to {ShortcutDefinition.CurrentFileVersion}; the file must be preserved.");
    }
}
