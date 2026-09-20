using System;
using System.Collections.Generic;
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
                ExecutableArguments = GetString(shortcut, "ExecutableArguments"),
                ExecutableArgumentsRequired = GetBoolean(shortcut, "ExecutableArgumentsRequired"),
                RunExecutableAsAdministrator = GetBoolean(shortcut, "RunExeAsAdministrator"),
                MonitorExecutablePath = GetBoolean(shortcut, "ProcessNameToMonitorUsesExecutable", true),
                DifferentExecutablePathToMonitor = GetString(shortcut, "DifferentExecutableToMonitor"),
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
                AfterPrograms = GetAfterPrograms(shortcut),
                StopPrograms = GetStopPrograms(shortcut)
            });
        }

        return definitions;
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

    private static int GetInt32(JsonElement element, string propertyName, int defaultValue = 0)
    {
        return element.TryGetProperty(propertyName, out JsonElement property) && property.TryGetInt32(out int value)
            ? value
            : defaultValue;
    }

    private static bool GetBoolean(JsonElement element, string propertyName, bool defaultValue = false)
    {
        return element.TryGetProperty(propertyName, out JsonElement property) && (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False)
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
