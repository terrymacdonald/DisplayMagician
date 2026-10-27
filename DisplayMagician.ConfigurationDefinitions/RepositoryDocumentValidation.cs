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
        foreach (JsonElement item in items.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("UUID", out JsonElement id) ||
                id.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(id.GetString()) || !identities.Add(id.GetString()!))
                throw new JsonException("Repository entries must have distinct, nonempty UUID strings.");
        }
    }
}
