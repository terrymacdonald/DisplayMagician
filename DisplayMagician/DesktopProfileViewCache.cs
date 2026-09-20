using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician;

internal static class DesktopProfileViewCache
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static readonly object _syncRoot = new object();
    private static DisplayProfileView[] _views = Array.Empty<DisplayProfileView>();
    private static DisplayProfileView _currentLayout;

    public static IReadOnlyList<DisplayProfileView> Views
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

    public static bool Refresh()
    {
        try
        {
            ProfileListResult profiles = new ControlServicePipeClient().ListProfilesAsync(CancellationToken.None).GetAwaiter().GetResult();
            lock (_syncRoot)
            {
                _views = profiles.Views ?? Array.Empty<DisplayProfileView>();
                _currentLayout = profiles.CurrentLayout;
            }

            return true;
        }
        catch (Exception ex)
        {
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