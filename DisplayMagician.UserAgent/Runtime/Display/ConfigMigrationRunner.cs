using System;
using System.IO;
using System.Text;
using DisplayMagician.UserAgent.Runtime.Intel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisplayMagician.UserAgent.Runtime;

/// <summary>UserAgent owns migrations of authoritative display data; WinForms never opens these files.</summary>
internal static class ConfigMigrationRunner
{
    internal static readonly string DisplayProfileVersion = DisplayMagician.ConfigurationDefinitions.RepositoryDocumentValidation.CurrentDisplayProfileFileVersion.ToString(System.Globalization.CultureInfo.InvariantCulture);

    internal static string MigrateDisplayProfilesJson(string json)
    {
        JToken parsed = JToken.Parse(json);
        JObject root = parsed is JArray legacy ? new JObject { ["Profiles"] = legacy } :
            parsed as JObject ?? throw new JsonSerializationException("Display profiles must be a JSON object or legacy array.");
        JToken? token = root["ProfileFileVersion"];
        string? version = token?.Type == JTokenType.String ? (string?)token : null;
        if (token != null && token.Type != JTokenType.Null &&
            (token.Type != JTokenType.String || (!string.IsNullOrWhiteSpace(version) &&
            (!int.TryParse(version, out int number) || number < 1 || number > DisplayMagician.ConfigurationDefinitions.RepositoryDocumentValidation.CurrentDisplayProfileFileVersion))))
            throw new JsonSerializationException("Unsupported display-profile file version; the original file must be preserved.");
        JArray profiles = root["Profiles"] as JArray ?? (root["Profiles"] as JObject)?["$values"] as JArray
            ?? throw new JsonSerializationException("Display profiles must contain a Profiles array.");
        foreach (JToken item in profiles)
            IntelAdapterIdentity.MigrateProfile(item as JObject ?? throw new JsonSerializationException("Invalid display-profile entry."));
        root["ProfileFileVersion"] = DisplayProfileVersion;
        // Preserve JSON.NET collection metadata when present.
        if (parsed is JObject original && JToken.DeepEquals(original, JToken.Parse(json))) return json;
        return root.ToString(Formatting.Indented);
    }

    internal static void MigrateDisplayProfilesFile(string path)
    {
        if (!File.Exists(path)) return;
        string original = File.ReadAllText(path, Encoding.Unicode);
        if (string.IsNullOrWhiteSpace(original)) return;
        string migrated = MigrateDisplayProfilesJson(original);
        if (string.Equals(original, migrated, StringComparison.Ordinal)) return;
        string backup = $"{path}.identity-migration.{DateTime.UtcNow:yyyyMMddHHmmss}.{Guid.NewGuid():N}.bak";
        File.Copy(path, backup, overwrite: false);
        AtomicFile.WriteAllText(path, migrated, Encoding.Unicode);
        SharedLogger.logger.Info("ConfigMigrationRunner/MigrateDisplayProfilesFile: Migrated display-profile identities in {0}; backup {1}.", path, backup);
    }

    internal static string MigrateDisplayRecoveryJson(string json)
    {
        JObject profile = JObject.Parse(json);
        IntelAdapterIdentity.MigrateProfile(profile);
        return profile.ToString(Formatting.None);
    }
}
