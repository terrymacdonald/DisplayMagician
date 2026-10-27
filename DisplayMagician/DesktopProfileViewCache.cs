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
                return _views.Select(CopyView).ToArray();
            }
        }
    }

    public static DisplayProfileView CurrentLayout
    {
        get
        {
            lock (_syncRoot)
            {
                return CopyView(_currentLayout);
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
                if (cancellationToken.IsCancellationRequested || refreshVersion != Interlocked.Read(ref _refreshVersion)) return false;
                DisplayProfileView[] views = (profiles.SavedProfiles ?? Array.Empty<DisplayProfileView>()).Select(CopyView).ToArray();
                DisplayProfileView currentLayout = CopyView(profiles.CurrentLayout);
                _views = views;
                _currentLayout = currentLayout;
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
            return CopyView(_views.FirstOrDefault(profile => string.Equals(profile.Id, profileId, StringComparison.OrdinalIgnoreCase)));
        }
    }

    public static string GetName(string profileId)
    {
        return Get(profileId)?.Name ?? "Unknown";
    }

    // Transport DTOs are mutable. Neither the response owner nor a cache reader
    // should be able to change another reader's profile/settings snapshot.
    private static DisplayProfileView CopyView(DisplayProfileView profile)
    {
        if (profile == null) return null;
        return new DisplayProfileView
        {
            Id = profile.Id,
            Name = profile.Name,
            ThumbnailPngBase64 = profile.ThumbnailPngBase64,
            DetailedLayoutPngBase64 = profile.DetailedLayoutPngBase64,
            DesktopIconIcoBase64 = profile.DesktopIconIcoBase64,
            ConnectedDisplayCount = profile.ConnectedDisplayCount,
            PrimaryDisplayWidth = profile.PrimaryDisplayWidth,
            PrimaryDisplayHeight = profile.PrimaryDisplayHeight,
            IsSaved = profile.IsSaved,
            IsActive = profile.IsActive,
            IsValid = profile.IsValid,
            DiagnosticMessage = profile.DiagnosticMessage,
            Settings = profile.Settings == null ? new DisplayProfileSettings() : new DisplayProfileSettings
            {
                ApplyWallpaper = profile.Settings.ApplyWallpaper,
                BackgroundDescription = profile.Settings.BackgroundDescription,
                ForceExplorerRestart = profile.Settings.ForceExplorerRestart,
                ApplyProfileCount = profile.Settings.ApplyProfileCount,
                ApplyProfileDelay = profile.Settings.ApplyProfileDelay
            }
        };
    }
}
