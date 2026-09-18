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
    public string GameAppId { get; init; } = string.Empty;
    public string GameName { get; init; } = string.Empty;
    public GameLaunchMode GameLaunchMode { get; init; }
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
