using System;
using System.Collections.Generic;
using System.Linq;

namespace DisplayMagician.UserAgent;

/// <summary>Tracks game running transitions so already-running games do not trigger automatic shortcuts.</summary>
public sealed class AutomaticGameDetectionStateTracker
{
    private readonly Dictionary<string, bool> _previouslyRunning = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

    public bool HasNewlyStarted(string shortcutId, bool isRunning)
    {
        if (string.IsNullOrWhiteSpace(shortcutId))
        {
            return false;
        }

        if (!_previouslyRunning.TryGetValue(shortcutId, out bool wasRunning))
        {
            _previouslyRunning.Add(shortcutId, isRunning);
            return false;
        }

        _previouslyRunning[shortcutId] = isRunning;
        return isRunning && !wasRunning;
    }

    public void Remove(string shortcutId)
    {
        if (!string.IsNullOrWhiteSpace(shortcutId))
        {
            _previouslyRunning.Remove(shortcutId);
        }
    }

    public IReadOnlyList<string> GetTrackedShortcutIds()
    {
        return _previouslyRunning.Keys.ToArray();
    }
}