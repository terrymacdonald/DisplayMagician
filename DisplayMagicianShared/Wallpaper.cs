using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace DisplayMagicianShared
{
    // ---------------------------------------------------------------------------
    // Per-monitor wallpaper entry: links a monitor device path to the locally
    // stored copy of its wallpaper image.
    // ---------------------------------------------------------------------------
    public class WallpaperMonitorConfig : IEquatable<WallpaperMonitorConfig>
    {
        /// <summary>Windows monitor device path (e.g. \\.\DISPLAY1\...).</summary>
        public string MonitorDevicePath { get; set; } = "";

        /// <summary>
        /// Full path to the wallpaper image copy stored under the app wallpaper
        /// folder, named with a GUID to avoid clashes.
        /// </summary>
        public string StoredFilename { get; set; } = "";

        /// <summary>
        /// Monitor bounding rectangle in virtual-screen coordinates. Used to match
        /// this entry to a ScreenPosition in DisplayView for the wallpaper preview.
        /// </summary>
        public RECT MonitorBounds { get; set; } = default;

        public WallpaperMonitorConfig() { }

        public WallpaperMonitorConfig(string monitorDevicePath, string storedFilename, RECT monitorBounds)
        {
            MonitorDevicePath = monitorDevicePath;
            StoredFilename = storedFilename;
            MonitorBounds = monitorBounds;
        }

        public override bool Equals(object obj) => obj is WallpaperMonitorConfig other && Equals(other);
        public bool Equals(WallpaperMonitorConfig other)
        {
            if (other is null) return false;
            return MonitorDevicePath == other.MonitorDevicePath &&
                   StoredFilename == other.StoredFilename &&
                   MonitorBounds.left == other.MonitorBounds.left &&
                   MonitorBounds.top == other.MonitorBounds.top &&
                   MonitorBounds.right == other.MonitorBounds.right &&
                   MonitorBounds.bottom == other.MonitorBounds.bottom;
        }
        public override int GetHashCode() => (MonitorDevicePath, StoredFilename, MonitorBounds.left, MonitorBounds.top).GetHashCode();
        public static bool operator ==(WallpaperMonitorConfig lhs, WallpaperMonitorConfig rhs)
            => lhs is null ? rhs is null : lhs.Equals(rhs);
        public static bool operator !=(WallpaperMonitorConfig lhs, WallpaperMonitorConfig rhs)
            => !(lhs == rhs);
    }

    // ---------------------------------------------------------------------------
    // Full wallpaper configuration for a display profile.
    // Stored directly on ProfileItem, independent of WinLibrary.
    // ---------------------------------------------------------------------------
    public class WallpaperConfig : IEquatable<WallpaperConfig>
    {
        /// <summary>What to do with wallpapers when this profile is applied.</summary>
        public Wallpaper.Mode WallpaperMode { get; set; } = Wallpaper.Mode.DoNothing;

        /// <summary>Single global fit style applied to all monitors.</summary>
        public Wallpaper.Style WallpaperStyle { get; set; } = Wallpaper.Style.Fill;

        /// <summary>Desktop background fill colour (COLORREF, 0x00BBGGRR).</summary>
        public uint BackgroundColor { get; set; } = 0;

        /// <summary>One entry per connected monitor at capture time.</summary>
        public List<WallpaperMonitorConfig> MonitorWallpapers { get; set; } = new List<WallpaperMonitorConfig>();

        public WallpaperConfig() { }

        public override bool Equals(object obj) => obj is WallpaperConfig other && Equals(other);
        public bool Equals(WallpaperConfig other)
        {
            if (other is null) return false;
            if (WallpaperMode != other.WallpaperMode) return false;
            if (WallpaperStyle != other.WallpaperStyle) return false;
            if (BackgroundColor != other.BackgroundColor) return false;
            if (MonitorWallpapers.Count != other.MonitorWallpapers.Count) return false;
            for (int i = 0; i < MonitorWallpapers.Count; i++)
                if (!MonitorWallpapers[i].Equals(other.MonitorWallpapers[i])) return false;
            return true;
        }
        public override int GetHashCode() => (WallpaperMode, WallpaperStyle, BackgroundColor, MonitorWallpapers.Count).GetHashCode();
        public static bool operator ==(WallpaperConfig lhs, WallpaperConfig rhs)
            => lhs is null ? rhs is null : lhs.Equals(rhs);
        public static bool operator !=(WallpaperConfig lhs, WallpaperConfig rhs)
            => !(lhs == rhs);
    }

    // ---------------------------------------------------------------------------
    // Static wallpaper management: capture current state, apply, clear.
    // Uses the IDesktopWallpaper COM API for per-monitor support.
    // ---------------------------------------------------------------------------
    public static class Wallpaper
    {
        // -----------------------------------------------------------------------
        // Enumerations
        // -----------------------------------------------------------------------

        public enum Style : int
        {
            Fill    = 0,
            Fit     = 1,
            Stretch = 2,
            Tile    = 3,
            Center  = 4,
            Span    = 5
        }

        public enum Mode : int
        {
            DoNothing = 0,
            Clear     = 1,
            Apply     = 2
        }

        // -----------------------------------------------------------------------
        // IDesktopWallpaper COM interface
        // CLSID: {C2CF3110-460E-4fc1-B9D0-8A1C0C9CC4BD}
        // IID:   {B92B56A9-8B55-4E14-9A89-0199BBB6F93B}
        // Vtable order must be exact; unused slots use IntPtr to skip safely.
        // -----------------------------------------------------------------------

        [ComImport]
        [Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDesktopWallpaper
        {
            [PreserveSig] int SetWallpaper(
                [MarshalAs(UnmanagedType.LPWStr)] string monitorID,
                [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);

            [PreserveSig] int GetWallpaper(
                [MarshalAs(UnmanagedType.LPWStr)] string monitorID,
                [MarshalAs(UnmanagedType.LPWStr)] out string wallpaper);

            [PreserveSig] int GetMonitorDevicePathAt(
                uint monitorIndex,
                [MarshalAs(UnmanagedType.LPWStr)] out string monitorID);

            [PreserveSig] int GetMonitorDevicePathCount(out uint count);

            [PreserveSig] int GetMonitorRECT(
                [MarshalAs(UnmanagedType.LPWStr)] string monitorID,
                out RECT displayRect);

            [PreserveSig] int SetBackgroundColor(uint color);
            [PreserveSig] int GetBackgroundColor(out uint color);

            [PreserveSig] int SetPosition(DESKTOP_WALLPAPER_POSITION position);
            [PreserveSig] int GetPosition(out DESKTOP_WALLPAPER_POSITION position);

            // Slideshow / status methods not needed — placeholder slots
            [PreserveSig] int SetSlideshow(IntPtr items);
            [PreserveSig] int GetSlideshow(out IntPtr items);
            [PreserveSig] int SetSlideshowOptions(uint options, uint slideshowTick);
            [PreserveSig] int GetSlideshowOptions(out uint options, out uint slideshowTick);
            [PreserveSig] int AdvanceSlideshow([MarshalAs(UnmanagedType.LPWStr)] string monitorID, int direction);
            [PreserveSig] int GetStatus(out uint state);
            [PreserveSig] int Enable([MarshalAs(UnmanagedType.Bool)] bool enable);
        }

        [ComImport]
        [Guid("C2CF3110-460E-4fc1-B9D0-8A1C0C9CC4BD")]
        [ClassInterface(ClassInterfaceType.None)]
        private class DesktopWallpaperClass { }

        // Maps Wallpaper.Style to the DESKTOP_WALLPAPER_POSITION COM enum value
        private enum DESKTOP_WALLPAPER_POSITION : int
        {
            DWPOS_CENTER  = 0,
            DWPOS_TILE    = 1,
            DWPOS_STRETCH = 2,
            DWPOS_FIT     = 3,
            DWPOS_FILL    = 4,
            DWPOS_SPAN    = 5,
        }

        private static DESKTOP_WALLPAPER_POSITION StyleToPosition(Style style)
        {
            return style switch
            {
                Style.Center  => DESKTOP_WALLPAPER_POSITION.DWPOS_CENTER,
                Style.Tile    => DESKTOP_WALLPAPER_POSITION.DWPOS_TILE,
                Style.Stretch => DESKTOP_WALLPAPER_POSITION.DWPOS_STRETCH,
                Style.Fit     => DESKTOP_WALLPAPER_POSITION.DWPOS_FIT,
                Style.Fill    => DESKTOP_WALLPAPER_POSITION.DWPOS_FILL,
                Style.Span    => DESKTOP_WALLPAPER_POSITION.DWPOS_SPAN,
                _             => DESKTOP_WALLPAPER_POSITION.DWPOS_FILL,
            };
        }

        private static Style PositionToStyle(DESKTOP_WALLPAPER_POSITION pos)
        {
            return pos switch
            {
                DESKTOP_WALLPAPER_POSITION.DWPOS_CENTER  => Style.Center,
                DESKTOP_WALLPAPER_POSITION.DWPOS_TILE    => Style.Tile,
                DESKTOP_WALLPAPER_POSITION.DWPOS_STRETCH => Style.Stretch,
                DESKTOP_WALLPAPER_POSITION.DWPOS_FIT     => Style.Fit,
                DESKTOP_WALLPAPER_POSITION.DWPOS_FILL    => Style.Fill,
                DESKTOP_WALLPAPER_POSITION.DWPOS_SPAN    => Style.Span,
                _                                        => Style.Fill,
            };
        }

        // -----------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------

        /// <summary>
        /// Snapshots the current per-monitor wallpaper configuration. For each
        /// connected monitor the current wallpaper image is copied into
        /// <paramref name="wallpaperStorePath"/> under a unique GUID-based
        /// filename so there are no clashes between profiles.
        /// Returns a <see cref="WallpaperConfig"/> ready to be stored on a
        /// <see cref="ProfileItem"/>.
        /// </summary>
        public static WallpaperConfig GetCurrentWallpaperConfig(string wallpaperStorePath)
        {
            var config = new WallpaperConfig
            {
                WallpaperMode  = Mode.Apply,
                WallpaperStyle = Style.Fill,
                BackgroundColor = 0,
                MonitorWallpapers = new List<WallpaperMonitorConfig>()
            };

            try
            {
                // Ensure the storage folder exists
                if (!Directory.Exists(wallpaperStorePath))
                    Directory.CreateDirectory(wallpaperStorePath);

                IDesktopWallpaper idw = (IDesktopWallpaper)new DesktopWallpaperClass();

                // Capture global style and background colour
                if (idw.GetPosition(out DESKTOP_WALLPAPER_POSITION pos) == 0)
                    config.WallpaperStyle = PositionToStyle(pos);

                if (idw.GetBackgroundColor(out uint bgColor) == 0)
                    config.BackgroundColor = bgColor;

                // Capture per-monitor wallpaper images
                if (idw.GetMonitorDevicePathCount(out uint monitorCount) == 0)
                {
                    for (uint i = 0; i < monitorCount; i++)
                    {
                        if (idw.GetMonitorDevicePathAt(i, out string monitorPath) != 0)
                        {
                            SharedLogger.logger.Warn($"Wallpaper/GetCurrentWallpaperConfig: Could not get device path for monitor index {i}, skipping.");
                            continue;
                        }

                        if (idw.GetWallpaper(monitorPath, out string wallpaperPath) != 0 ||
                            string.IsNullOrEmpty(wallpaperPath))
                        {
                            SharedLogger.logger.Trace($"Wallpaper/GetCurrentWallpaperConfig: Monitor {monitorPath} has no wallpaper set, skipping.");
                            continue;
                        }

                        if (!File.Exists(wallpaperPath))
                        {
                            SharedLogger.logger.Warn($"Wallpaper/GetCurrentWallpaperConfig: Wallpaper file '{wallpaperPath}' for monitor {monitorPath} does not exist on disk, skipping.");
                            continue;
                        }

                        // Build a unique destination filename preserving the original extension
                        string ext = Path.GetExtension(wallpaperPath);
                        if (string.IsNullOrEmpty(ext)) ext = ".png";
                        string destFilename = Path.Combine(wallpaperStorePath, $"wallpaper-{Guid.NewGuid()}{ext}");

                        // Capture the monitor bounding rect for DisplayView preview matching
                        RECT monitorRect = default;
                        idw.GetMonitorRECT(monitorPath, out monitorRect);

                        File.Copy(wallpaperPath, destFilename, overwrite: true);

                        config.MonitorWallpapers.Add(new WallpaperMonitorConfig(monitorPath, destFilename, monitorRect));
                        SharedLogger.logger.Trace($"Wallpaper/GetCurrentWallpaperConfig: Captured wallpaper for monitor {monitorPath} -> {destFilename}");
                    }
                }

                Marshal.ReleaseComObject(idw);
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"Wallpaper/GetCurrentWallpaperConfig: Exception capturing wallpaper configuration: {ex.Message}");
            }

            return config;
        }

        /// <summary>
        /// Applies a previously captured <see cref="WallpaperConfig"/> to the
        /// desktop. Sets the global fit style and background colour, then
        /// assigns each monitor's stored wallpaper image.
        /// </summary>
        public static bool Apply(WallpaperConfig config)
        {
            if (config == null)
            {
                SharedLogger.logger.Warn("Wallpaper/Apply: Null WallpaperConfig supplied, nothing to apply.");
                return false;
            }

            try
            {
                IDesktopWallpaper idw = (IDesktopWallpaper)new DesktopWallpaperClass();

                // Apply global style and background colour first
                idw.SetPosition(StyleToPosition(config.WallpaperStyle));
                idw.SetBackgroundColor(config.BackgroundColor);

                // Apply per-monitor wallpaper images
                foreach (WallpaperMonitorConfig mon in config.MonitorWallpapers)
                {
                    if (string.IsNullOrEmpty(mon.StoredFilename) || !File.Exists(mon.StoredFilename))
                    {
                        SharedLogger.logger.Warn($"Wallpaper/Apply: Stored wallpaper file '{mon.StoredFilename}' for monitor {mon.MonitorDevicePath} not found, skipping.");
                        continue;
                    }

                    int hr = idw.SetWallpaper(mon.MonitorDevicePath, mon.StoredFilename);
                    if (hr != 0)
                        SharedLogger.logger.Warn($"Wallpaper/Apply: SetWallpaper returned HRESULT 0x{hr:X8} for monitor {mon.MonitorDevicePath}.");
                    else
                        SharedLogger.logger.Trace($"Wallpaper/Apply: Set wallpaper for monitor {mon.MonitorDevicePath} to {mon.StoredFilename}.");
                }

                Marshal.ReleaseComObject(idw);
                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"Wallpaper/Apply: Exception applying wallpaper configuration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clears the desktop wallpaper on all monitors.
        /// </summary>
        public static bool Clear()
        {
            try
            {
                IDesktopWallpaper idw = (IDesktopWallpaper)new DesktopWallpaperClass();

                if (idw.GetMonitorDevicePathCount(out uint monitorCount) == 0)
                {
                    for (uint i = 0; i < monitorCount; i++)
                    {
                        if (idw.GetMonitorDevicePathAt(i, out string monitorPath) == 0)
                        {
                            idw.SetWallpaper(monitorPath, "");
                        }
                    }
                }

                Marshal.ReleaseComObject(idw);
                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"Wallpaper/Clear: Exception clearing wallpapers: {ex.Message}");
                return false;
            }
        }
    }
}
