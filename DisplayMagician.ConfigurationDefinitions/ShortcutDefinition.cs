using System;
using System.Collections.Generic;

namespace DisplayMagician.ConfigurationDefinitions;

/// <summary>
/// The portable, persisted information required to identify and execute a
/// shortcut. Runtime state, UI images, and hardware objects do not belong here.
/// </summary>
public sealed class ShortcutDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ShortcutDefinitionCategory Category { get; init; }

    public bool AutoName { get; init; } = true;
    public string ProfileId { get; init; } = string.Empty;
    public string AudioProfileId { get; init; } = string.Empty;
    public ShortcutDefinitionPermanence DisplayPermanence { get; init; } = ShortcutDefinitionPermanence.Temporary;
    public ShortcutDefinitionPermanence AudioPermanence { get; init; } = ShortcutDefinitionPermanence.Temporary;
    public ShortcutDefinitionProcessPriority ProcessPriority { get; init; } = ShortcutDefinitionProcessPriority.Normal;

    public string ExecutablePath { get; init; } = string.Empty;
    public string ExecutableArguments { get; init; } = string.Empty;
    public bool ExecutableArgumentsRequired { get; init; }
    public bool RunExecutableAsAdministrator { get; init; }
    public bool MonitorExecutablePath { get; init; } = true;
    public string DifferentExecutablePathToMonitor { get; init; } = string.Empty;

    public string ApplicationId { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = string.Empty;
    public int ApplicationLibrary { get; init; } = -1;

    public string GameAppId { get; init; } = string.Empty;
    public string GameName { get; init; } = string.Empty;
    public int GameLibrary { get; init; }
    public GameLaunchMode GameLaunchMode { get; init; }
    public int StartTimeoutSeconds { get; init; } = 60;
    public string GameArguments { get; init; } = string.Empty;
    public bool GameArgumentsRequired { get; init; }
    public string DifferentGameExecutablePathToMonitor { get; init; } = string.Empty;
    public bool MonitorDifferentGameExecutable { get; init; }

    public bool OverrideAudioSpeakerVolume { get; init; }
    public int OverrideAudioSpeakerVolumeLevel { get; init; } = 50;
    public bool OverrideAudioMicrophoneVolume { get; init; }
    public int OverrideAudioMicrophoneVolumeLevel { get; init; } = 50;

    public IReadOnlyList<ShortcutStartProgramDefinition> StartPrograms { get; init; } = Array.Empty<ShortcutStartProgramDefinition>();
    public IReadOnlyList<ShortcutAfterProgramDefinition> AfterPrograms { get; init; } = Array.Empty<ShortcutAfterProgramDefinition>();
    public IReadOnlyList<ShortcutStopProgramDefinition> StopPrograms { get; init; } = Array.Empty<ShortcutStopProgramDefinition>();
}

public sealed class ShortcutStartProgramDefinition
{
    public int Priority { get; init; }
    public bool Disabled { get; init; }
    public ShortcutDefinitionProcessPriority ProcessPriority { get; init; } = ShortcutDefinitionProcessPriority.Normal;
    public string ExecutablePath { get; init; } = string.Empty;
    public string ApplicationId { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = string.Empty;
    public string Arguments { get; init; } = string.Empty;
    public bool ArgumentsRequired { get; init; }
    public bool CloseOnFinish { get; init; }
    public bool DoNotStartIfAlreadyRunning { get; init; }
    public bool RunAsAdministrator { get; init; }
}

public sealed class ShortcutAfterProgramDefinition
{
    public int Priority { get; init; }
    public bool Disabled { get; init; }
    public ShortcutDefinitionProcessPriority ProcessPriority { get; init; } = ShortcutDefinitionProcessPriority.Normal;
    public string ExecutablePath { get; init; } = string.Empty;
    public string Arguments { get; init; } = string.Empty;
    public bool ArgumentsRequired { get; init; }
    public bool DoNotStartIfAlreadyRunning { get; init; }
    public bool RunAsAdministrator { get; init; }
}

public sealed class ShortcutStopProgramDefinition
{
    public int Priority { get; init; }
    public bool Disabled { get; init; }
    public string ExecutablePath { get; init; } = string.Empty;
    public bool RestartAfterwards { get; init; }
    public ShortcutDefinitionProcessPriority RestartProcessPriority { get; init; } = ShortcutDefinitionProcessPriority.Normal;
    public bool RunAsAdministrator { get; init; }
}

/// <summary>Matches the persisted values of the existing ShortcutCategory.</summary>
public enum ShortcutDefinitionCategory
{
    Unknown = -1,
    Executable = 0,
    Game = 1,
    NoGame = 2,
    Application = 3
}

public enum GameLaunchMode
{
    StartGame = 0,
    DetectGameRunning = 1
}

/// <summary>Matches the persisted values of the existing ShortcutPermanence.</summary>
public enum ShortcutDefinitionPermanence
{
    Permanent = 0,
    Temporary = 1
}

/// <summary>Matches the persisted values of the existing ProcessPriority.</summary>
public enum ShortcutDefinitionProcessPriority
{
    High = 2,
    AboveNormal = 1,
    Normal = 0,
    BelowNormal = -1,
    Idle = -24
}
