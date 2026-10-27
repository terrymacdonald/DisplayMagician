using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician;

internal static class DesktopProfileViewCache
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static readonly object _syncRoot = new object();
    private static DisplayProfileView[] _views = Array.Empty<DisplayProfileView>();
    private static DisplayProfileView _currentLayout;
    private static long _refreshVersion;

    public static IReadOnlyList<DisplayProfileView> SavedProfiles
    {
        get
        {
            lock (_syncRoot)
            {
                return _views;
            }
        }
    }

    public static DisplayProfileView CurrentLayout
    {
        get
        {
            lock (_syncRoot)
            {
                return _currentLayout;
            }
        }
    }

    public static bool Refresh() => RefreshAsync(new ControlServicePipeClient(), CancellationToken.None).GetAwaiter().GetResult();

    internal static async Task<bool> RefreshAsync(ControlServicePipeClient client, CancellationToken cancellationToken)
    {
        long refreshVersion = Interlocked.Increment(ref _refreshVersion);
        try
        {
            ProfileListResult profiles = await client.ListProfilesAsync(cancellationToken).ConfigureAwait(false);
            lock (_syncRoot)
            {
                if (refreshVersion != Interlocked.Read(ref _refreshVersion)) return false;
                _views = profiles.SavedProfiles ?? Array.Empty<DisplayProfileView>();
                _currentLayout = profiles.CurrentLayout;
            }

            return true;
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested) return false;
            _logger.Warn(ex, "DesktopProfileViewCache/Refresh: Could not refresh display profile views from the User Agent.");
            return false;
        }
    }

    public static DisplayProfileView Get(string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return null;
        }

        lock (_syncRoot)
        {
            return _views.FirstOrDefault(profile => string.Equals(profile.Id, profileId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static string GetName(string profileId)
    {
        return Get(profileId)?.Name ?? "Unknown";
    }
}
