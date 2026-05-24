using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DisplayMagicianShared.Windows;

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
        /// Full path to the wallpaper image. Initially set to the live OS path
        /// captured by <see cref="Wallpaper.CaptureCurrentWallpaperConfig"/>; updated
        /// to the permanent app-storage path after
        /// <see cref="Wallpaper.SaveWallpaperFiles"/> has been called.
        /// </summary>
        public string WallpaperFilePath { get; set; } = "";

        /// <summary>
        /// Monitor bounding rectangle in virtual-screen coordinates. Used to match
        /// this entry to a ScreenPosition in DisplayView for the wallpaper preview.
        /// </summary>
        public RECT MonitorBounds { get; set; } = default;

        public WallpaperMonitorConfig() { }

        public WallpaperMonitorConfig(string monitorDevicePath, string wallpaperFilePath, RECT monitorBounds)
        {
            MonitorDevicePath = monitorDevicePath;
            WallpaperFilePath = wallpaperFilePath;
            MonitorBounds = monitorBounds;
        }

        public override bool Equals(object obj) => obj is WallpaperMonitorConfig other && Equals(other);
        public bool Equals(WallpaperMonitorConfig other)
        {
            if (other is null) return false;
            return MonitorDevicePath == other.MonitorDevicePath &&
                   WallpaperFilePath == other.WallpaperFilePath &&
                   MonitorBounds.left == other.MonitorBounds.left &&
                   MonitorBounds.top == other.MonitorBounds.top &&
                   MonitorBounds.right == other.MonitorBounds.right &&
                   MonitorBounds.bottom == other.MonitorBounds.bottom;
        }
        public override int GetHashCode() => (MonitorDevicePath, WallpaperFilePath, MonitorBounds.left, MonitorBounds.top).GetHashCode();
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
        public Wallpaper.Mode WallpaperMode { get; set; } = Wallpaper.Mode.Apply;

        /// <summary>Single global fit style applied to all monitors (Picture mode only).</summary>
        public Wallpaper.Style WallpaperStyle { get; set; } = Wallpaper.Style.Fill;

        /// <summary>Desktop background fill colour (COLORREF, 0x00BBGGRR).</summary>
        public uint BackgroundColor { get; set; } = 0;

        /// <summary>Which of the four Windows background modes was active when this profile was captured.</summary>
        public Wallpaper.BackgroundType BackgroundType { get; set; } = Wallpaper.BackgroundType.Picture;

        /// <summary>Source folder path for Slideshow mode.</summary>
        public string SlideshowDirectoryPath { get; set; } = "";

        /// <summary>Slideshow rotation interval in seconds (Slideshow mode only).</summary>
        public uint SlideshowIntervalSeconds { get; set; } = 300;

        /// <summary>Whether the slideshow plays in random order (Slideshow mode only).</summary>
        public bool SlideshowShuffle { get; set; } = false;

        /// <summary>Whether the slideshow continues running on battery power (Slideshow mode only).</summary>
        public bool SlideshowBatteryPower { get; set; } = false;

        /// <summary>One entry per connected monitor at capture time (Picture mode only).</summary>
        public List<WallpaperMonitorConfig> MonitorWallpapers { get; set; } = new List<WallpaperMonitorConfig>();

        public WallpaperConfig() { }

        public override bool Equals(object obj) => obj is WallpaperConfig other && Equals(other);
        public bool Equals(WallpaperConfig other)
        {
            if (other is null) return false;
            if (WallpaperMode != other.WallpaperMode) return false;
            if (WallpaperStyle != other.WallpaperStyle) return false;
            if (BackgroundColor != other.BackgroundColor) return false;
            if (BackgroundType != other.BackgroundType) return false;
            if (SlideshowDirectoryPath != other.SlideshowDirectoryPath) return false;
            if (SlideshowIntervalSeconds != other.SlideshowIntervalSeconds) return false;
            if (SlideshowShuffle != other.SlideshowShuffle) return false;
            if (SlideshowBatteryPower != other.SlideshowBatteryPower) return false;
            if (MonitorWallpapers.Count != other.MonitorWallpapers.Count) return false;
            var otherByPath = new Dictionary<string, WallpaperMonitorConfig>(StringComparer.Ordinal);
            foreach (var m in other.MonitorWallpapers)
                otherByPath[m.MonitorDevicePath] = m;
            foreach (var m in MonitorWallpapers)
            {
                if (!otherByPath.TryGetValue(m.MonitorDevicePath, out var om)) return false;
                if (!m.Equals(om)) return false;
            }
            return true;
        }
        public override int GetHashCode() => (WallpaperMode, WallpaperStyle, BackgroundColor, BackgroundType, SlideshowDirectoryPath, SlideshowShuffle, SlideshowBatteryPower, MonitorWallpapers.Count).GetHashCode();
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
            Apply     = 0,
            DoNothing = 1,
        }

        /// <summary>
        /// The four Windows desktop background types available in
        /// Settings → Personalisation → Background.
        /// </summary>
        public enum BackgroundType : int
        {
            Picture     = 0,
            SolidColour = 1,
            Slideshow   = 2,
            Spotlight   = 3,
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

            [PreserveSig] int SetSlideshow([MarshalAs(UnmanagedType.Interface)] IShellItemArray items);
            [PreserveSig] int GetSlideshow([MarshalAs(UnmanagedType.Interface)] out IShellItemArray items);
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

        // Minimal IShellItem COM interface — vtable order must be exact
        [ComImport]
        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            [PreserveSig] int BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
            [PreserveSig] int GetParent(out IShellItem ppsi);
            [PreserveSig] int GetDisplayName(uint sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            [PreserveSig] int GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
            [PreserveSig] int Compare(IShellItem psi, uint hint, out int piOrder);
        }

        // Minimal IShellItemArray COM interface — vtable order must be exact
        [ComImport]
        [Guid("B63EA76D-1F85-456F-A19C-48159EFA858B")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItemArray
        {
            [PreserveSig] int BindToHandler(IntPtr pbc, ref Guid rbhid, ref Guid riid, out IntPtr ppvOut);
            [PreserveSig] int GetPropertyStore(int flags, ref Guid riid, out IntPtr ppv);
            [PreserveSig] int GetPropertyDescriptionList(IntPtr keyType, ref Guid riid, out IntPtr ppv);
            [PreserveSig] int GetAttributes(uint AttribFlags, uint sfgaoMask, out uint psfgaoAttribs);
            [PreserveSig] int GetCount(out uint pdwNumItems);
            [PreserveSig] int GetItemAt(uint dwIndex, [MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);
            [PreserveSig] int EnumItems(out IntPtr ppenumShellItems);
        }

        // Creates an IShellItem for a filesystem path (used to build slideshow item arrays)
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SHCreateItemFromParsingName(
            string pszPath,
            IntPtr pbc,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out IShellItem ppv);

        // Wraps a single IShellItem in an IShellItemArray (used for SetSlideshow)
        [DllImport("shell32.dll", PreserveSig = false)]
        private static extern void SHCreateShellItemArrayFromShellItem(
            [MarshalAs(UnmanagedType.Interface)] IShellItem psi,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppv);

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
        // Private helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// FNV-1a hash of a monitor's position and size. Deterministic across
        /// process runs (no runtime seed), filename-safe 8-hex-char output.
        /// Using width/height rather than raw right/bottom makes the hash stable
        /// independent of where the monitor sits in the virtual desktop.
        /// </summary>
        private static string MonitorBoundsHash(RECT r)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ (uint)r.left)              * 16777619u;
                h = (h ^ (uint)r.top)               * 16777619u;
                h = (h ^ (uint)(r.right  - r.left)) * 16777619u;
                h = (h ^ (uint)(r.bottom - r.top))  * 16777619u;
                return h.ToString("x8");
            }
        }

        // -----------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------

        /// <summary>
        /// Snapshots the current per-monitor wallpaper configuration. Captures
        /// metadata and live image paths only — no files are copied at this stage.
        /// Call <see cref="SaveWallpaperFiles"/> (e.g. from <c>PreSave</c>) to
        /// copy the images into permanent storage.
        /// Returns a <see cref="WallpaperConfig"/> ready to be stored on a
        /// <see cref="ProfileItem"/>.
        /// </summary>
        public static WallpaperConfig CaptureCurrentWallpaperConfig(WINDOWS_DISPLAY_CONFIG windowsDisplayConfig)
        {
            var config = new WallpaperConfig
            {
                WallpaperMode   = Mode.Apply,
                WallpaperStyle  = Style.Fill,
                BackgroundColor = 0,
                MonitorWallpapers = new List<WallpaperMonitorConfig>()
            };

            IDesktopWallpaper idw = null;
            try
            {
                // Detect which background type Windows has active
                using (var regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers"))
                {
                    int bgType = regKey?.GetValue("BackgroundType") is int v ? v : 0;
                    config.BackgroundType = bgType switch
                    {
                        1 => BackgroundType.SolidColour,
                        2 => BackgroundType.Slideshow,
                        3 => BackgroundType.Spotlight,
                        _ => BackgroundType.Picture,
                    };

                    if (config.BackgroundType == BackgroundType.Slideshow)
                        config.SlideshowDirectoryPath = regKey?.GetValue("SlideshowDirectoryPath") as string ?? "";
                }

                idw = (IDesktopWallpaper)new DesktopWallpaperClass();

                // Capture global style and background colour
                if (idw.GetPosition(out DESKTOP_WALLPAPER_POSITION pos) == 0)
                    config.WallpaperStyle = PositionToStyle(pos);

                if (idw.GetBackgroundColor(out uint bgColor) == 0)
                    config.BackgroundColor = bgColor;

                // For Slideshow, also capture interval and shuffle settings
                if (config.BackgroundType == BackgroundType.Slideshow)
                {
                    if (idw.GetSlideshowOptions(out uint slideshowOpts, out uint slideshowTick) == 0)
                    {
                        config.SlideshowIntervalSeconds = slideshowTick / 1000;
                        config.SlideshowShuffle      = (slideshowOpts & 0x01) != 0;
                        config.SlideshowBatteryPower = (slideshowOpts & 0x02) != 0;
                    }
                }

                // Capture per-monitor wallpaper live paths (Picture mode only)
                if (config.BackgroundType == BackgroundType.Picture)
                {
                    // Build device-path → bounds lookup from WinLibrary data.
                    // IDesktopWallpaper.GetMonitorRECT fails for most monitors in practice;
                    // the GdiDisplaySettings data captured by WinLibrary is more reliable.
                    var devicePathToBounds = new Dictionary<string, RECT>(StringComparer.OrdinalIgnoreCase);
                    foreach (var kvp in windowsDisplayConfig.DisplaySources)
                    {
                        if (!windowsDisplayConfig.GdiDisplaySettings.TryGetValue(kvp.Key, out var gdi))
                            continue;
                        var dm = gdi.DeviceMode;
                        var r = new RECT
                        {
                            left   = dm.Position.X,
                            top    = dm.Position.Y,
                            right  = dm.Position.X + (int)dm.PixelsWidth,
                            bottom = dm.Position.Y + (int)dm.PixelsHeight
                        };
                        foreach (var src in kvp.Value)
                            if (!string.IsNullOrEmpty(src.DevicePath))
                                devicePathToBounds[src.DevicePath] = r;
                    }

                    if (idw.GetMonitorDevicePathCount(out uint monitorCount) == 0)
                    {
                        for (uint i = 0; i < monitorCount; i++)
                        {
                            if (idw.GetMonitorDevicePathAt(i, out string monitorPath) != 0)
                            {
                                SharedLogger.logger.Warn($"Wallpaper/CaptureCurrentWallpaperConfig: Could not get device path for monitor index {i}, skipping.");
                                continue;
                            }

                            if (idw.GetWallpaper(monitorPath, out string wallpaperPath) != 0 ||
                                string.IsNullOrEmpty(wallpaperPath))
                            {
                                SharedLogger.logger.Trace($"Wallpaper/CaptureCurrentWallpaperConfig: Monitor {monitorPath} has no wallpaper set, skipping.");
                                continue;
                            }

                            if (!File.Exists(wallpaperPath))
                            {
                                SharedLogger.logger.Warn($"Wallpaper/CaptureCurrentWallpaperConfig: Wallpaper file '{wallpaperPath}' for monitor {monitorPath} does not exist on disk, skipping.");
                                continue;
                            }

                            if (!devicePathToBounds.TryGetValue(monitorPath, out RECT monitorRect))
                            {
                                SharedLogger.logger.Trace($"Wallpaper/CaptureCurrentWallpaperConfig: No bounds found in display config for monitor {monitorPath}, using default RECT.");
                                monitorRect = default;
                            }

                            // Store the live path; SaveWallpaperFiles will copy it to permanent storage later.
                            config.MonitorWallpapers.Add(new WallpaperMonitorConfig(monitorPath, wallpaperPath, monitorRect));
                            SharedLogger.logger.Trace($"Wallpaper/CaptureCurrentWallpaperConfig: Captured live wallpaper path for monitor {monitorPath} -> {wallpaperPath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"Wallpaper/CaptureCurrentWallpaperConfig: Exception capturing wallpaper configuration: {ex.Message}");
            }
            finally
            {
                if (idw != null) Marshal.ReleaseComObject(idw);
            }

            return config;
        }

        /// <summary>
        /// Copies each monitor's wallpaper image into <paramref name="storePath"/> using
        /// a deterministic filename derived from <paramref name="profileUUID"/> and the
        /// monitor bounds hash, then updates <see cref="WallpaperMonitorConfig.WallpaperFilePath"/>
        /// in-place to the new destination. Safe to call multiple times (idempotent).
        /// </summary>
        public static void SaveWallpaperFiles(WallpaperConfig config, string storePath, string profileUUID)
        {
            if (config == null || config.BackgroundType != BackgroundType.Picture)
                return;

            if (!Directory.Exists(storePath))
                Directory.CreateDirectory(storePath);

            foreach (var mon in config.MonitorWallpapers)
            {
                if (string.IsNullOrEmpty(mon.WallpaperFilePath))
                    continue;

                string ext = Path.GetExtension(mon.WallpaperFilePath);
                if (string.IsNullOrEmpty(ext)) ext = ".png";
                string dest = Path.Combine(storePath, $"wallpaper-{profileUUID}-{MonitorBoundsHash(mon.MonitorBounds)}{ext}");

                // Idempotent: if the source is already the intended destination, skip the copy.
                // This also avoids a Windows CopyFile ERROR_SHARING_VIOLATION when the active
                // wallpaper file is locked (source == dest).
                try
                {
                    if (string.Equals(Path.GetFullPath(mon.WallpaperFilePath), Path.GetFullPath(dest),
                                      StringComparison.OrdinalIgnoreCase))
                    {
                        mon.WallpaperFilePath = dest;
                        SharedLogger.logger.Trace($"Wallpaper/SaveWallpaperFiles: Wallpaper for monitor {mon.MonitorDevicePath} already at destination, skipping copy.");
                        continue;
                    }
                }
                catch { /* invalid path (e.g. \\?\ prefix) — fall through */ }

                if (!File.Exists(mon.WallpaperFilePath))
                {
                    SharedLogger.logger.Warn($"Wallpaper/SaveWallpaperFiles: Source file '{mon.WallpaperFilePath}' does not exist for monitor {mon.MonitorDevicePath}, skipping.");
                    continue;
                }

                try
                {
                    File.Copy(mon.WallpaperFilePath, dest, overwrite: true);
                    mon.WallpaperFilePath = dest;
                    SharedLogger.logger.Trace($"Wallpaper/SaveWallpaperFiles: Saved wallpaper for monitor {mon.MonitorDevicePath} -> {dest}");
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"Wallpaper/SaveWallpaperFiles: Exception copying wallpaper for monitor {mon.MonitorDevicePath}: {ex.Message}");
                }
            }
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

            IDesktopWallpaper idw = null;
            try
            {
                idw = (IDesktopWallpaper)new DesktopWallpaperClass();

                switch (config.BackgroundType)
                {
                    case BackgroundType.SolidColour:
                    {
                        // Clear all wallpaper images and apply background colour only
                        if (idw.GetMonitorDevicePathCount(out uint monCount) == 0)
                        {
                            for (uint i = 0; i < monCount; i++)
                            {
                                if (idw.GetMonitorDevicePathAt(i, out string monPath) == 0)
                                    idw.SetWallpaper(monPath, "");
                            }
                        }
                        idw.SetBackgroundColor(config.BackgroundColor);
                        using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers", true))
                            key?.SetValue("BackgroundType", 1, RegistryValueKind.DWord);
                        SharedLogger.logger.Trace("Wallpaper/Apply: Applied Solid Colour background.");
                        break;
                    }

                    case BackgroundType.Slideshow:
                    {
                        if (string.IsNullOrEmpty(config.SlideshowDirectoryPath) ||
                            !Directory.Exists(config.SlideshowDirectoryPath))
                        {
                            SharedLogger.logger.Warn($"Wallpaper/Apply: Slideshow source folder '{config.SlideshowDirectoryPath}' not found, skipping slideshow restore.");
                            break;
                        }

                        idw.SetPosition(StyleToPosition(config.WallpaperStyle));
                        idw.SetBackgroundColor(config.BackgroundColor);

                        Guid iShellItemGuid      = typeof(IShellItem).GUID;
                        Guid iShellItemArrayGuid = typeof(IShellItemArray).GUID;
                        SHCreateItemFromParsingName(config.SlideshowDirectoryPath, IntPtr.Zero, iShellItemGuid, out IShellItem folderItem);
                        SHCreateShellItemArrayFromShellItem(folderItem, iShellItemArrayGuid, out IShellItemArray itemArray);
                        idw.SetSlideshow(itemArray);

                        uint slideshowOpts = (config.SlideshowShuffle      ? 0x01u : 0u)
                                           | (config.SlideshowBatteryPower ? 0x02u : 0u);
                        uint intervalMs    = config.SlideshowIntervalSeconds * 1000u;
                        if (intervalMs < 10000u) intervalMs = 10000u;   // Windows minimum is 10 seconds
                        idw.SetSlideshowOptions(slideshowOpts, intervalMs);

                        using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers", true))
                        {
                            key?.SetValue("BackgroundType", 2, RegistryValueKind.DWord);
                            key?.SetValue("SlideshowDirectoryPath", config.SlideshowDirectoryPath, RegistryValueKind.String);
                        }
                        SharedLogger.logger.Trace($"Wallpaper/Apply: Applied Slideshow from '{config.SlideshowDirectoryPath}'.");
                        break;
                    }

                    case BackgroundType.Spotlight:
                    {
                        // Spotlight for desktop is only available on Windows 11 (build >= 22000)
                        if (Environment.OSVersion.Version.Build < 22000)
                        {
                            SharedLogger.logger.Info("Wallpaper/Apply: Windows Spotlight for the desktop is only available on Windows 11. Skipping Spotlight restore on this OS.");
                            break;
                        }
                        using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers", true))
                            key?.SetValue("BackgroundType", 3, RegistryValueKind.DWord);
                        SharedLogger.logger.Info("Wallpaper/Apply: Windows Spotlight re-enabled. Note: the specific image shown cannot be restored — Windows will select its own image on its own schedule.");
                        break;
                    }

                    default: // BackgroundType.Picture
                    {
                        if (config.MonitorWallpapers.Count == 0)
                        {
                            SharedLogger.logger.Trace("Wallpaper/Apply: Picture mode with no stored wallpapers, skipping.");
                            break;
                        }

                        // Apply global style and background colour first
                        idw.SetPosition(StyleToPosition(config.WallpaperStyle));
                        idw.SetBackgroundColor(config.BackgroundColor);

                        // Apply per-monitor wallpaper images
                        foreach (WallpaperMonitorConfig mon in config.MonitorWallpapers)
                        {
                            if (string.IsNullOrEmpty(mon.WallpaperFilePath) || !File.Exists(mon.WallpaperFilePath))
                            {
                                SharedLogger.logger.Warn($"Wallpaper/Apply: Wallpaper file '{mon.WallpaperFilePath}' for monitor {mon.MonitorDevicePath} not found, skipping.");
                                continue;
                            }

                            int hr = idw.SetWallpaper(mon.MonitorDevicePath, mon.WallpaperFilePath);
                            if (hr != 0)
                                SharedLogger.logger.Warn($"Wallpaper/Apply: SetWallpaper returned HRESULT 0x{hr:X8} for monitor {mon.MonitorDevicePath}.");
                            else
                                SharedLogger.logger.Trace($"Wallpaper/Apply: Set wallpaper for monitor {mon.MonitorDevicePath} to {mon.WallpaperFilePath}.");
                        }

                        using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers", true))
                            key?.SetValue("BackgroundType", 0, RegistryValueKind.DWord);
                        break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"Wallpaper/Apply: Exception applying wallpaper configuration: {ex.Message}");
                return false;
            }
            finally
            {
                if (idw != null) Marshal.ReleaseComObject(idw);
            }
        }
    }
}
