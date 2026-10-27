using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Contracts;

namespace DisplayMagician.UserAgent;

/// <summary>Agent-owned persistence boundary for shortcut definitions. Runtime execution moves to ShortcutRunner separately.</summary>
public sealed class ShortcutStore
{
    private readonly string _shortcutFilePath;
    private readonly object _writeLock = new object();
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public ShortcutStore(string userDataPath)
    {
        _shortcutFilePath = Path.Combine(userDataPath, "Shortcuts", "Shortcuts.json");
        MigrateLegacyApplicationObjects();
        MigrateWindowPlacementSchema();
    }

    public RepositorySnapshot GetSnapshot()
    {
        lock (_writeLock)
            return Runtime.AtomicFile.GetSnapshot(_shortcutFilePath, RepositoryKind.Shortcuts);
    }

    public bool TryGetShortcutDefinition(string shortcutId, out ShortcutDefinition? shortcutDefinition)
    {
        shortcutDefinition = null;
        if (string.IsNullOrWhiteSpace(shortcutId))
        {
            return false;
        }

        shortcutDefinition = GetShortcutDefinitions().Find(definition => string.Equals(definition.Id, shortcutId, StringComparison.OrdinalIgnoreCase));
        return shortcutDefinition != null;
    }

    public List<ShortcutDefinition> GetShortcutDefinitions()
    {
        RepositorySnapshot snapshot = GetSnapshot();
        return GetShortcutDefinitions(snapshot.Json);
    }

    public List<ShortcutDefinition> GetShortcutDefinitions(string json)
    {
        List<ShortcutDefinition> definitions = new List<ShortcutDefinition>();
        if (string.IsNullOrWhiteSpace(json))
        {
            return definitions;
        }

        using JsonDocument document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("Shortcuts", out JsonElement shortcuts) || shortcuts.ValueKind != JsonValueKind.Array)
        {
            return definitions;
        }

        foreach (JsonElement shortcut in shortcuts.EnumerateArray())
        {
            string id = GetString(shortcut, "UUID");
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            int category = GetInt32(shortcut, "Category");
            int launchMode = GetInt32(shortcut, "GameLaunchMode");
            definitions.Add(new ShortcutDefinition
            {
                Id = id,
                Name = GetString(shortcut, "Name"),
                Category = Enum.IsDefined(typeof(ShortcutDefinitionCategory), category) ? (ShortcutDefinitionCategory)category : ShortcutDefinitionCategory.Unknown,
                AutoName = GetBoolean(shortcut, "AutoName", true),
                ProfileId = GetString(shortcut, "ProfileUUID"),
                AudioProfileId = GetString(shortcut, "AudioProfileUUID"),
                DisplayPermanence = GetEnum(shortcut, "DisplayPermanence", ShortcutDefinitionPermanence.Temporary),
                AudioPermanence = GetEnum(shortcut, "AudioPermanence", ShortcutDefinitionPermanence.Temporary),
                ProcessPriority = GetEnum(shortcut, "ProcessPriority", ShortcutDefinitionProcessPriority.Normal),
                ExecutablePath = GetString(shortcut, "ExecutableNameAndPath"),
                OriginalIconPath = GetString(shortcut, "OriginalIconPath"),
                ExecutableArguments = GetString(shortcut, "ExecutableArguments"),
                ExecutableArgumentsRequired = GetBoolean(shortcut, "ExecutableArgumentsRequired"),
                RunExecutableAsAdministrator = GetBoolean(shortcut, "RunExeAsAdministrator"),
                MonitorLaunchedExecutable = GetBoolean(shortcut, "MonitorLaunchedExecutable", true),
                DifferentExecutablePathToMonitor = GetString(shortcut, "DifferentExecutableToMonitor"),
                ApplicationId = GetString(shortcut, "ApplicationId"),
                ApplicationName = GetString(shortcut, "ApplicationName"),
                ApplicationLibrary = GetInt32(shortcut, "ApplicationLibrary", -1),
                GameAppId = GetString(shortcut, "GameAppId"),
                GameName = GetString(shortcut, "GameName"),
                GameLibrary = GetInt32(shortcut, "GameLibrary"),
                GameLaunchMode = Enum.IsDefined(typeof(GameLaunchMode), launchMode) ? (GameLaunchMode)launchMode : GameLaunchMode.StartGame,
                StartTimeoutSeconds = GetInt32(shortcut, "StartTimeout", 60),
                GameArguments = GetString(shortcut, "GameArguments"),
                GameArgumentsRequired = GetBoolean(shortcut, "GameArgumentsRequired"),
                DifferentGameExecutablePathToMonitor = GetString(shortcut, "DifferentGameExeToMonitor"),
                MonitorDifferentGameExecutable = GetBoolean(shortcut, "MonitorDifferentGameExe"),
                OverrideAudioSpeakerVolume = GetBoolean(shortcut, "OverrideAudioSpeakerVolume"),
                OverrideAudioSpeakerVolumeLevel = GetInt32(shortcut, "OverrideAudioSpeakerVolumeLevel", 50),
                OverrideAudioMicrophoneVolume = GetBoolean(shortcut, "OverrideAudioMicrophoneVolume"),
                OverrideAudioMicrophoneVolumeLevel = GetInt32(shortcut, "OverrideAudioMicrophoneVolumeLevel", 50),
                StartPrograms = GetStartPrograms(shortcut),
                WindowPlacements = GetWindowPlacements(shortcut),
                WindowPlacementTimeoutSeconds = GetInt32(shortcut, "WindowPlacementTimeoutSeconds", 20),
                AfterPrograms = GetAfterPrograms(shortcut),
                StopPrograms = GetStopPrograms(shortcut)
            });
        }

        return definitions;
    }

    public Dictionary<string, string> GetSavedShortcutBitmaps(string json)
    {
        Dictionary<string, string> bitmaps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(json))
            return bitmaps;

        using JsonDocument document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("Shortcuts", out JsonElement shortcuts) || shortcuts.ValueKind != JsonValueKind.Array)
            return bitmaps;

        foreach (JsonElement shortcut in shortcuts.EnumerateArray())
        {
            string id = GetString(shortcut, "UUID");
            if (!string.IsNullOrWhiteSpace(id) && shortcut.TryGetProperty("ShortcutBitmap", out JsonElement bitmap) &&
                bitmap.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(bitmap.GetString()))
                bitmaps[id] = bitmap.GetString()!;
        }

        return bitmaps;
    }

    public RepositoryCommitResult Commit(RepositoryCommitRequest request)
    {
        if (request.Repository != RepositoryKind.Shortcuts)
            throw new ArgumentException("The shortcut store only accepts shortcut repositories.", nameof(request));
        lock (_writeLock)
        {
            return Runtime.AtomicFile.CommitSnapshot(_shortcutFilePath, request);
        }
    }

    public int ReplaceWindowPlacements(string shortcutId, IReadOnlyList<ShortcutWindowPlacementDefinition> placements)
    {
        lock (_writeLock)
        {
            RepositorySnapshot snapshot = GetSnapshot();
            JsonObject? root = JsonNode.Parse(snapshot.Json) as JsonObject;
            if (root?["Shortcuts"] is not JsonArray shortcuts)
            {
                return -1;
            }

            JsonObject? shortcut = shortcuts.OfType<JsonObject>().FirstOrDefault(item =>
                string.Equals(item["UUID"]?.GetValue<string>(), shortcutId, StringComparison.OrdinalIgnoreCase));
            if (shortcut == null)
            {
                return -1;
            }

            int previousCount = (shortcut["WindowPlacements"] as JsonArray)?.Count ?? 0;
            shortcut["WindowPlacements"] = JsonSerializer.SerializeToNode(placements);
            shortcut["WindowPlacementTimeoutSeconds"] ??= 20;
            root["ShortcutFileVersion"] = "8";
            RepositoryCommitResult result = Commit(new RepositoryCommitRequest
            {
                Repository = RepositoryKind.Shortcuts,
                ExpectedRevision = snapshot.Revision,
                Json = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true })
            });
            if (result.WasConflict) throw new InvalidOperationException("The shortcut snapshot changed while updating window positions.");
            return previousCount;
        }
    }

    private void MigrateWindowPlacementSchema()
    {
        if (!File.Exists(_shortcutFilePath)) return;
        try
        {
            JsonObject? root = JsonNode.Parse(GetSnapshot().Json) as JsonObject;
            if (root?["Shortcuts"] is not JsonArray shortcuts) return;
            string? storedVersion = root["ShortcutFileVersion"]?.GetValue<string>();
            bool knownVersion = int.TryParse(storedVersion, out int versionNumber);
            if (knownVersion && versionNumber > 8) return;
            bool hasLegacyMonitorName = shortcuts.OfType<JsonObject>().Any(shortcut =>
                shortcut.ContainsKey("ProcessNameToMonitorUsesExecutable") || shortcut.ContainsKey("MonitorExecutablePath"));
            if (knownVersion && versionNumber == 8 && !hasLegacyMonitorName) return;
            foreach (JsonObject shortcut in shortcuts.OfType<JsonObject>())
            {
                shortcut["MonitorLaunchedExecutable"] ??= shortcut["ProcessNameToMonitorUsesExecutable"]?.DeepClone()
                    ?? shortcut["MonitorExecutablePath"]?.DeepClone() ?? JsonValue.Create(true);
                shortcut.Remove("ProcessNameToMonitorUsesExecutable");
                shortcut.Remove("MonitorExecutablePath");
                shortcut["WindowPlacements"] ??= new JsonArray();
                shortcut["WindowPlacementTimeoutSeconds"] ??= 20;
                if (shortcut["StartPrograms"] is JsonArray programs)
                {
                    foreach (JsonObject program in programs.OfType<JsonObject>())
                        program["Id"] ??= Guid.NewGuid().ToString("D");
                }
            }
            string previousVersion = int.TryParse(storedVersion, out versionNumber) && versionNumber >= 0
                ? versionNumber.ToString() : "unknown";
            root["ShortcutFileVersion"] = "8";
            string backupPath = $"{_shortcutFilePath}.v{previousVersion}.{DateTime.UtcNow:yyyyMMddHHmmss}.{Guid.NewGuid():N}.bak";
            File.Copy(_shortcutFilePath, backupPath, overwrite: false);
            Runtime.AtomicFile.WriteAllText(_shortcutFilePath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), Encoding.Unicode);
            Logger.Info("ShortcutStore/MigrateWindowPlacementSchema: Upgraded {0} with backup {1}.", _shortcutFilePath, backupPath);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is InvalidOperationException)
        {
            Logger.Error(ex, "ShortcutStore/MigrateWindowPlacementSchema: Could not migrate {0}.", _shortcutFilePath);
            // Loading the old false-valued flag as a missing new flag would silently
            // select the wrong executable. Do not expose an unmigrated snapshot.
            throw new InvalidDataException("The shortcut configuration could not be migrated safely. Existing settings were preserved.", ex);
        }
    }

    private void MigrateLegacyApplicationObjects()
    {
        if (!File.Exists(_shortcutFilePath))
        {
            return;
        }

        try
        {
            string json = Encoding.Unicode.GetString(File.ReadAllBytes(_shortcutFilePath)).TrimStart('\uFEFF');
            JsonNode? root = JsonNode.Parse(json);
            if (root is not JsonObject rootObject || rootObject["Shortcuts"] is not JsonArray shortcuts)
            {
                return;
            }

            if (rootObject["ShortcutFileVersion"] is JsonValue versionValue && versionValue.TryGetValue<string>(out string? version) &&
                int.TryParse(version, out int versionNumber) && versionNumber > 8)
                return;

            bool changed = false;
            foreach (JsonNode? shortcut in shortcuts)
            {
                if (shortcut is JsonObject shortcutObject && shortcutObject.Remove("Application"))
                {
                    changed = true;
                }
            }

            if (!changed)
            {
                return;
            }

            string backupPath = $"{_shortcutFilePath}.legacy-application.{DateTime.UtcNow:yyyyMMddHHmmss}.{Guid.NewGuid():N}.bak";
            File.Copy(_shortcutFilePath, backupPath, overwrite: false);
            Runtime.AtomicFile.WriteAllText(_shortcutFilePath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), Encoding.Unicode);
            Logger.Info($"ShortcutStore/MigrateLegacyApplicationObjects: Removed legacy application objects and backed up shortcuts to {backupPath}.");
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            Logger.Error(ex, $"ShortcutStore/MigrateLegacyApplicationObjects: Could not normalize legacy application objects in {_shortcutFilePath}.");
        }
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static int GetInt32(JsonElement element, string propertyName, int defaultValue = 0)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out int value)
            ? value
            : defaultValue;
    }

    private static bool GetBoolean(JsonElement element, string propertyName, bool defaultValue = false)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out JsonElement property) && (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False)
            ? property.GetBoolean()
            : defaultValue;
    }

    private static TEnum GetEnum<TEnum>(JsonElement element, string propertyName, TEnum defaultValue) where TEnum : struct, Enum
    {
        int value = GetInt32(element, propertyName, Convert.ToInt32(defaultValue));
        return Enum.IsDefined(typeof(TEnum), value) ? (TEnum)Enum.ToObject(typeof(TEnum), value) : defaultValue;
    }

    private static IReadOnlyList<ShortcutStartProgramDefinition> GetStartPrograms(JsonElement shortcut)
    {
        List<ShortcutStartProgramDefinition> programs = new List<ShortcutStartProgramDefinition>();
        if (!shortcut.TryGetProperty("StartPrograms", out JsonElement sourcePrograms) || sourcePrograms.ValueKind != JsonValueKind.Array)
        {
            return programs;
        }

        foreach (JsonElement program in sourcePrograms.EnumerateArray())
        {
            programs.Add(new ShortcutStartProgramDefinition
            {
                Id = GetString(program, "Id"),
                Priority = GetInt32(program, "Priority"),
                Disabled = GetBoolean(program, "Disabled"),
                ProcessPriority = GetEnum(program, "ProcessPriority", ShortcutDefinitionProcessPriority.Normal),
                ExecutablePath = GetString(program, "Executable"),
                ApplicationId = GetString(program, "ApplicationId"),
                ApplicationName = GetString(program, "ApplicationName"),
                Arguments = GetString(program, "Arguments"),
                ArgumentsRequired = GetBoolean(program, "ExecutableArgumentsRequired"),
                CloseOnFinish = GetBoolean(program, "CloseOnFinish"),
                DoNotStartIfAlreadyRunning = GetBoolean(program, "DontStartIfAlreadyRunning"),
                RunAsAdministrator = GetBoolean(program, "RunAsAdministrator")
            });
        }

        return programs;
    }

    private static IReadOnlyList<ShortcutWindowPlacementDefinition> GetWindowPlacements(JsonElement shortcut)
    {
        if (!shortcut.TryGetProperty("WindowPlacements", out JsonElement source) || source.ValueKind != JsonValueKind.Array)
            return Array.Empty<ShortcutWindowPlacementDefinition>();
        try
        {
            return JsonSerializer.Deserialize<ShortcutWindowPlacementDefinition[]>(source.GetRawText()) ?? Array.Empty<ShortcutWindowPlacementDefinition>();
        }
        catch (JsonException ex)
        {
            Logger.Warn(ex, "ShortcutStore/GetWindowPlacements: Could not parse placements for shortcut {0}.", GetString(shortcut, "UUID"));
            return Array.Empty<ShortcutWindowPlacementDefinition>();
        }
    }

    private static IReadOnlyList<ShortcutAfterProgramDefinition> GetAfterPrograms(JsonElement shortcut)
    {
        List<ShortcutAfterProgramDefinition> programs = new List<ShortcutAfterProgramDefinition>();
        if (!shortcut.TryGetProperty("AfterPrograms", out JsonElement sourcePrograms) || sourcePrograms.ValueKind != JsonValueKind.Array)
        {
            return programs;
        }

        foreach (JsonElement program in sourcePrograms.EnumerateArray())
        {
            programs.Add(new ShortcutAfterProgramDefinition
            {
                Priority = GetInt32(program, "Priority"),
                Disabled = GetBoolean(program, "Disabled"),
                ProcessPriority = GetEnum(program, "ProcessPriority", ShortcutDefinitionProcessPriority.Normal),
                ExecutablePath = GetString(program, "Executable"),
                Arguments = GetString(program, "Arguments"),
                ArgumentsRequired = GetBoolean(program, "ExecutableArgumentsRequired"),
                DoNotStartIfAlreadyRunning = GetBoolean(program, "DontStartIfAlreadyRunning"),
                RunAsAdministrator = GetBoolean(program, "RunAsAdministrator")
            });
        }

        return programs;
    }

    private static IReadOnlyList<ShortcutStopProgramDefinition> GetStopPrograms(JsonElement shortcut)
    {
        List<ShortcutStopProgramDefinition> programs = new List<ShortcutStopProgramDefinition>();
        if (!shortcut.TryGetProperty("StopPrograms", out JsonElement sourcePrograms) || sourcePrograms.ValueKind != JsonValueKind.Array)
        {
            return programs;
        }

        foreach (JsonElement program in sourcePrograms.EnumerateArray())
        {
            programs.Add(new ShortcutStopProgramDefinition
            {
                Priority = GetInt32(program, "Priority"),
                Disabled = GetBoolean(program, "Disabled"),
                ExecutablePath = GetString(program, "Executable"),
                RestartAfterwards = GetBoolean(program, "RestartAfterwards"),
                RestartProcessPriority = GetEnum(program, "RestartProcessPriority", ShortcutDefinitionProcessPriority.Normal),
                RunAsAdministrator = GetBoolean(program, "RunAsAdministrator")
            });
        }

        return programs;
    }
}
