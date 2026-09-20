namespace DisplayMagician;

/// <summary>
/// Compatibility representation used by the existing process-monitoring source
/// while shortcut runtime is moved into the User Agent. Shortcut definitions use
/// ShortcutDefinitionProcessPriority at the IPC/persistence boundary.
/// </summary>
public enum ProcessPriority
{
    High = 2,
    AboveNormal = 1,
    Normal = 0,
    BelowNormal = -1,
    Idle = -24
}
