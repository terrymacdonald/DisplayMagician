using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

/// <summary>Compares authoritative User Agent repository snapshots by stable saved-resource ID.</summary>
public static class SavedResourceChangeDetector
{
    public static GatewaySavedResourceChange[] Detect(RepositoryKind kind, string beforeJson, string afterJson)
    {
        Dictionary<string, (string Id, string Json)> before = ReadItems(kind, beforeJson);
        Dictionary<string, (string Id, string Json)> after = ReadItems(kind, afterJson);
        List<GatewaySavedResourceChange> changes = new List<GatewaySavedResourceChange>();
        foreach ((string key, (string id, string json)) in after.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (before.TryGetValue(key, out (string Id, string Json) previous) && previous.Id == id && previous.Json == json) continue;
            changes.Add(Create(kind, id, SavedResourceChangeAction.Upsert));
        }
        foreach ((string key, (string id, string json)) in before.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (after.ContainsKey(key)) continue;
            changes.Add(Create(kind, id, SavedResourceChangeAction.Deleted));
        }
        return changes.ToArray();
    }

    public static void Validate(RepositoryKind kind, string json) => _ = ReadItems(kind, json);

    public static GatewaySavedResourceChange Create(RepositoryKind kind, string id, SavedResourceChangeAction action)
    {
        if (string.IsNullOrWhiteSpace(id) || action is not (SavedResourceChangeAction.Upsert or SavedResourceChangeAction.Deleted)) throw new ArgumentException("The saved resource change is invalid.", nameof(id));
        (string type, string route) = kind switch
        {
            RepositoryKind.DisplayProfiles => ("display-profile", "display-profiles"),
            RepositoryKind.AudioProfiles => ("audio-profile", "audio-profiles"),
            RepositoryKind.Shortcuts => ("shortcut", "shortcuts"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        return new GatewaySavedResourceChange
        {
            ResourceType = type,
            Id = id,
            Action = action == SavedResourceChangeAction.Upsert ? "upsert" : "deleted",
            Href = action == SavedResourceChangeAction.Upsert ? $"/v1/{route}/{Uri.EscapeDataString(id)}" : null
        };
    }

    private static Dictionary<string, (string Id, string Json)> ReadItems(RepositoryKind kind, string json)
    {
        string property = kind switch
        {
            RepositoryKind.DisplayProfiles => "Profiles",
            RepositoryKind.AudioProfiles => "AudioProfiles",
            RepositoryKind.Shortcuts => "Shortcuts",
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        Dictionary<string, (string Id, string Json)> items = new Dictionary<string, (string Id, string Json)>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(json)) return items;
        using JsonDocument document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object || !document.RootElement.TryGetProperty(property, out JsonElement array) || array.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException($"The {kind} snapshot does not contain a {property} array.");
        }
        foreach (JsonElement item in array.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("UUID", out JsonElement idValue) || idValue.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(idValue.GetString()))
            {
                throw new JsonException($"The {kind} snapshot contains an item without an ID.");
            }
            string id = idValue.GetString()!;
            if (!items.TryAdd(id, (id, item.GetRawText()))) throw new JsonException($"The {kind} snapshot contains duplicate IDs.");
        }
        return items;
    }
}
