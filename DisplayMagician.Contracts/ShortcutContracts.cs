namespace DisplayMagician.Contracts;

/// <summary>Stable library identifiers persisted by shortcut definitions and returned to clients.</summary>
public enum SupportedGameLibraryType
{
    Unknown = 0,
    Steam = 1,
    Uplay = 2,
    Origin = 3,
    Epic = 4,
    GOG = 5,
    Xbox = 6
}

public enum GameLaunchMode
{
    StartGame = 0,
    DetectGameRunning = 1
}

public enum ShortcutPermanence
{
    Permanent = 0,
    Temporary = 1
}

public enum ShortcutCategory
{
    Executable = 0,
    Game = 1,
    NoGame = 2,
    Application = 3
}

public enum ProcessPriority
{
    High = 2,
    AboveNormal = 1,
    Normal = 0,
    BelowNormal = -1,
    Idle = -24
}