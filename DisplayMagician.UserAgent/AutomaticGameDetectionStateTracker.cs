using System;
using System.Collections.Generic;
using System.Linq;
using DisplayMagician.Processes;

namespace DisplayMagician.UserAgent;

/// <summary>Tracks newly seen process identities without retriggering on old running copies.</summary>
public sealed class AutomaticGameDetectionStateTracker
{
    private readonly Dictionary<string, HashSet<ProcessIdentity>> _seen = new Dictionary<string, HashSet<ProcessIdentity>>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ProcessIdentity> _pending = new Dictionary<string, ProcessIdentity>(StringComparer.OrdinalIgnoreCase);

    public ProcessIdentity? Observe(string shortcutId, IReadOnlyList<ProcessIdentity> runningProcesses)
    {
        if (string.IsNullOrWhiteSpace(shortcutId)) return null;
        HashSet<ProcessIdentity> running = runningProcesses.ToHashSet();
        if (!_seen.TryGetValue(shortcutId, out HashSet<ProcessIdentity>? seen))
        {
            _seen[shortcutId] = running;
            return null;
        }

        ProcessIdentity[] newlyStarted = running.Where(identity => !seen.Contains(identity)).ToArray();
        seen.IntersectWith(running);
        seen.UnionWith(running);
        if (newlyStarted.Length > 0)
            _pending[shortcutId] = newlyStarted.OrderByDescending(identity => identity.StartTimeUtc).First();

        if (_pending.TryGetValue(shortcutId, out ProcessIdentity pending) && running.Contains(pending))
            return pending;
        _pending.Remove(shortcutId);
        return null;
    }

    public void MarkHandled(string shortcutId, ProcessIdentity identity)
    {
        if (_pending.TryGetValue(shortcutId, out ProcessIdentity pending) && pending == identity)
            _pending.Remove(shortcutId);
    }

    public void Remove(string shortcutId)
    {
        if (string.IsNullOrWhiteSpace(shortcutId)) return;
        _seen.Remove(shortcutId);
        _pending.Remove(shortcutId);
    }

    public IReadOnlyList<string> GetTrackedShortcutIds()
    {
        return _seen.Keys.ToArray();
    }
}
