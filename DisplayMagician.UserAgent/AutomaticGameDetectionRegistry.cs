using System;
using System.Collections.Generic;
using DisplayMagician.ConfigurationDefinitions;

namespace DisplayMagician.UserAgent;

/// <summary>
/// Maintains the set of automatic game detections owned by the User Agent.
/// Process watching is added by ShortcutRunner; this class protects its lifecycle
/// and prevents two shortcuts reacting to the same game monitor target.
/// </summary>
public sealed class AutomaticGameDetectionRegistry
{
    private readonly object _syncRoot = new object();
    private readonly Dictionary<string, ShortcutDefinition> _registeredShortcuts = new Dictionary<string, ShortcutDefinition>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ShortcutDefinition> _suspendedShortcuts = new Dictionary<string, ShortcutDefinition>(StringComparer.OrdinalIgnoreCase);

    public AutomaticGameDetectionRegistrationResult RegisterAutomaticDetection(ShortcutDefinition shortcut)
    {
        ArgumentNullException.ThrowIfNull(shortcut);
        lock (_syncRoot)
        {
            if (!IsEligibleForAutomaticDetection(shortcut))
            {
                return AutomaticGameDetectionRegistrationResult.NotEligible;
            }

            foreach (ShortcutDefinition registeredShortcut in _registeredShortcuts.Values)
            {
                if (string.Equals(registeredShortcut.Id, shortcut.Id, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.Equals(GetGameMonitorTarget(registeredShortcut), GetGameMonitorTarget(shortcut), StringComparison.OrdinalIgnoreCase))
                {
                    return AutomaticGameDetectionRegistrationResult.ConflictingGameMonitorTarget;
                }
            }

            if (_registeredShortcuts.ContainsKey(shortcut.Id))
            {
                _registeredShortcuts[shortcut.Id] = shortcut;
                return AutomaticGameDetectionRegistrationResult.Updated;
            }

            _registeredShortcuts.Add(shortcut.Id, shortcut);
            return AutomaticGameDetectionRegistrationResult.Registered;
        }
    }

    public void ReplaceAutomaticDetections(IEnumerable<ShortcutDefinition> shortcuts)
    {
        ArgumentNullException.ThrowIfNull(shortcuts);
        lock (_syncRoot)
        {
            _registeredShortcuts.Clear();
            _suspendedShortcuts.Clear();
            foreach (ShortcutDefinition shortcut in shortcuts)
            {
                RegisterAutomaticDetection(shortcut);
            }
        }
    }

    public bool SuspendAutomaticDetectionForManualRun(string shortcutId)
    {
        lock (_syncRoot)
        {
            if (string.IsNullOrWhiteSpace(shortcutId) || !_registeredShortcuts.Remove(shortcutId, out ShortcutDefinition? shortcut))
            {
                return false;
            }

            _suspendedShortcuts[shortcutId] = shortcut;
            return true;
        }
    }

    public AutomaticGameDetectionRegistrationResult RestoreAutomaticDetectionAfterManualRun(string shortcutId)
    {
        lock (_syncRoot)
        {
            if (string.IsNullOrWhiteSpace(shortcutId) || !_suspendedShortcuts.TryGetValue(shortcutId, out ShortcutDefinition? shortcut))
            {
                return AutomaticGameDetectionRegistrationResult.NotRegistered;
            }

            AutomaticGameDetectionRegistrationResult result = RegisterAutomaticDetection(shortcut);
            if (result == AutomaticGameDetectionRegistrationResult.Registered || result == AutomaticGameDetectionRegistrationResult.Updated)
            {
                _suspendedShortcuts.Remove(shortcutId);
            }

            return result;
        }
    }

    public bool RemoveAutomaticDetection(string shortcutId)
    {
        lock (_syncRoot)
        {
            if (string.IsNullOrWhiteSpace(shortcutId))
            {
                return false;
            }

            bool removedRegisteredShortcut = _registeredShortcuts.Remove(shortcutId);
            bool removedSuspendedShortcut = _suspendedShortcuts.Remove(shortcutId);
            return removedRegisteredShortcut || removedSuspendedShortcut;
        }
    }

    public bool IsAutomaticDetectionRegistered(string shortcutId)
    {
        lock (_syncRoot)
        {
            return !string.IsNullOrWhiteSpace(shortcutId) && _registeredShortcuts.ContainsKey(shortcutId);
        }
    }

    public IReadOnlyList<ShortcutDefinition> GetRegisteredShortcuts()
    {
        lock (_syncRoot)
        {
            return new List<ShortcutDefinition>(_registeredShortcuts.Values);
        }
    }

    private static bool IsEligibleForAutomaticDetection(ShortcutDefinition shortcut)
    {
        return shortcut.GameLaunchMode == GameLaunchMode.DetectGameRunning &&
            shortcut.Category == ShortcutDefinitionCategory.Game &&
            !string.IsNullOrWhiteSpace(shortcut.Id) &&
            !string.IsNullOrWhiteSpace(GetGameMonitorTarget(shortcut));
    }

    private static string GetGameMonitorTarget(ShortcutDefinition shortcut)
    {
        if (shortcut.MonitorDifferentGameExecutable && !string.IsNullOrWhiteSpace(shortcut.DifferentGameExecutablePathToMonitor))
        {
            return $"executable:{shortcut.DifferentGameExecutablePathToMonitor}";
        }

        return $"game:{shortcut.GameLibrary}:{shortcut.GameAppId}";
    }
}

public enum AutomaticGameDetectionRegistrationResult
{
    NotRegistered = 0,
    Registered = 1,
    Updated = 2,
    NotEligible = 3,
    ConflictingGameMonitorTarget = 4
}
