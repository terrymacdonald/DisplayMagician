using System;
using System.Collections.Generic;

namespace DisplayMagician.Gateway;

/// <summary>Applies the first-release per-device and failed-authentication limits.</summary>
public sealed class GatewayTrafficLimiter
{
    private readonly object _syncRoot = new object();
    private readonly Dictionary<string, Window> _requests = new Dictionary<string, Window>(StringComparer.Ordinal);
    private readonly Dictionary<string, Window> _mutations = new Dictionary<string, Window>(StringComparer.Ordinal);
    private readonly Dictionary<string, Window> _failedAuthentications = new Dictionary<string, Window>(StringComparer.Ordinal);
    private DateTime _nextCleanupUtc;

    public bool IsSourceBlocked(string sourceIp, DateTime utcNow) => !CanConsume(_failedAuthentications, sourceIp, 20, utcNow, false);

    public void RecordFailedAuthentication(string sourceIp, DateTime utcNow) => CanConsume(_failedAuthentications, sourceIp, 20, utcNow, true);

    public bool TryConsumeDeviceRequest(string deviceId, bool isMutation, DateTime utcNow)
    {
        lock (_syncRoot)
        {
            PruneExpiredWindowsUnsafe(utcNow);
            if (!CanConsumeUnsafe(_requests, deviceId, 120, utcNow, false) ||
                isMutation && !CanConsumeUnsafe(_mutations, deviceId, 10, utcNow, false))
            {
                return false;
            }

            CanConsumeUnsafe(_requests, deviceId, 120, utcNow, true);
            if (isMutation) CanConsumeUnsafe(_mutations, deviceId, 10, utcNow, true);
            return true;
        }
    }

    private bool CanConsume(Dictionary<string, Window> windows, string key, int limit, DateTime utcNow, bool consume)
    {
        lock (_syncRoot)
        {
            PruneExpiredWindowsUnsafe(utcNow);
            return CanConsumeUnsafe(windows, key, limit, utcNow, consume);
        }
    }

    private void PruneExpiredWindowsUnsafe(DateTime utcNow)
    {
        if (utcNow < _nextCleanupUtc) return;

        foreach (Dictionary<string, Window> windows in new[] { _requests, _mutations, _failedAuthentications })
        {
            foreach (string key in new List<string>(windows.Keys))
            {
                if (utcNow >= windows[key].StartsUtc.AddMinutes(1)) windows.Remove(key);
            }
        }

        _nextCleanupUtc = utcNow.AddMinutes(1);
    }

    private static bool CanConsumeUnsafe(Dictionary<string, Window> windows, string key, int limit, DateTime utcNow, bool consume)
    {
        key ??= string.Empty;
        if (!windows.TryGetValue(key, out Window? window) || utcNow >= window.StartsUtc.AddMinutes(1))
        {
            if (consume) windows[key] = new Window { StartsUtc = utcNow, Count = 1 };
            return true;
        }

        if (window.Count >= limit) return false;
        if (consume) window.Count++;
        return true;
    }

    private sealed class Window
    {
        public DateTime StartsUtc { get; set; }
        public int Count { get; set; }
    }
}
