using DisplayMagicianShared.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

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
        public override int GetHashCode() => (WallpaperMode, WallpaperStyle, BackgroundColor, BackgroundType, SlideshowDirectoryPath, SlideshowShuffle, MonitorWallpapers.Count).GetHashCode();
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
            Fill = 0,
            Fit = 1,
            Stretch = 2,
            Tile = 3,
            Center = 4,
            Span = 5
        }

        public enum Mode : int
        {
            Apply = 0,
            DoNothing = 1,
        }

        /// <summary>
        /// The four Windows desktop background types available in
        /// Settings → Personalisation → Background.
        /// </summary>
        public enum BackgroundType : int
        {
            Picture = 0,
            SolidColour = 1,
            Slideshow = 2,
            Spotlight = 3,
        }

        // Native methods to force Windows Explorer to refresh its settings layout
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessageTimeout(
            IntPtr hWnd, uint Msg, IntPtr wParam, string lParam,
            uint fuFlags, uint uTimeout, out IntPtr lpdwResult
        );

        private const uint WM_SETTINGCHANGE = 0x001A;
        private const uint SMTO_ABORTIFHUNG = 0x0002;
        private static readonly IntPtr HWND_BROADCAST = (IntPtr)0xffff;
        private const string ControlPanelDesktopKeyPath = @"Control Panel\Desktop";
        private const string ExplorerWallpapersKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers";
        private const string DesktopSpotlightKeyPath = @"Software\Microsoft\Windows\CurrentVersion\DesktopSpotlight";
        private const string DesktopSpotlightSettingsKeyPath = @"Software\Microsoft\Windows\CurrentVersion\DesktopSpotlight\Settings";
        private const string CloudContentPolicyKeyPath = @"Software\Policies\Microsoft\Windows\CloudContent";
        private const string BackgroundAccessApplicationsKeyPath = @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications";
        private const string DesktopSpotlightCreativesKeyPath = @"Software\Microsoft\Windows\CurrentVersion\DesktopSpotlight\Creatives";
        private const string DesktopSpotlightExtensionContract = "WindowsUdk.UI.Shell.DesktopSpotlight.DesktopSpotlightExtension";

        // WinRT Desktop Spotlight private interface IDs discovered from DesktopSpotlight.dll symbols.
        private static readonly Guid IID_IDesktopSpotlightExtensionStatics = new Guid("151a5f2b-906b-5417-bc0b-2d84fb193a15");

        private static readonly object DesktopSpotlightProviderLock = new object();
        private static DesktopSpotlightProviderCandidate _desktopSpotlightProvider;

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
            [PreserveSig]
            int SetWallpaper(
                [MarshalAs(UnmanagedType.LPWStr)] string monitorID,
                [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);

            [PreserveSig]
            int GetWallpaper(
                [MarshalAs(UnmanagedType.LPWStr)] string monitorID,
                [MarshalAs(UnmanagedType.LPWStr)] out string wallpaper);

            [PreserveSig]
            int GetMonitorDevicePathAt(
                uint monitorIndex,
                [MarshalAs(UnmanagedType.LPWStr)] out string monitorID);

            [PreserveSig] int GetMonitorDevicePathCount(out uint count);

            [PreserveSig]
            int GetMonitorRECT(
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

        [DllImport("combase.dll")]
        private static extern int RoInitialize(RO_INIT_TYPE initType);

        [DllImport("combase.dll")]
        private static extern void RoUninitialize();

        [DllImport("combase.dll", CharSet = CharSet.Unicode)]
        private static extern int WindowsCreateString(
            string sourceString,
            int length,
            out IntPtr hstring);

        [DllImport("combase.dll")]
        private static extern int WindowsDeleteString(IntPtr hstring);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryExW(
            string lpLibFileName,
            IntPtr hFile,
            LoadLibraryFlags dwFlags);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int DllGetActivationFactoryDelegate(IntPtr activatableClassId, out IntPtr factory);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int WinRtGetDefaultDelegate(IntPtr thisPtr, out IntPtr extension);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int WinRtGetUInt32Delegate(IntPtr thisPtr, out uint value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int WinRtSetUInt32Delegate(IntPtr thisPtr, uint value);

        private enum RO_INIT_TYPE
        {
            RO_INIT_SINGLETHREADED = 0,
            RO_INIT_MULTITHREADED = 1
        }

        [Flags]
        private enum LoadLibraryFlags : uint
        {
            None = 0,
            LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100,
            LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000
        }

        // Maps Wallpaper.Style to the DESKTOP_WALLPAPER_POSITION COM enum value
        private enum DESKTOP_WALLPAPER_POSITION : int
        {
            DWPOS_CENTER = 0,
            DWPOS_TILE = 1,
            DWPOS_STRETCH = 2,
            DWPOS_FIT = 3,
            DWPOS_FILL = 4,
            DWPOS_SPAN = 5,
        }

        private static DESKTOP_WALLPAPER_POSITION StyleToPosition(Style style)
        {
            return style switch
            {
                Style.Center => DESKTOP_WALLPAPER_POSITION.DWPOS_CENTER,
                Style.Tile => DESKTOP_WALLPAPER_POSITION.DWPOS_TILE,
                Style.Stretch => DESKTOP_WALLPAPER_POSITION.DWPOS_STRETCH,
                Style.Fit => DESKTOP_WALLPAPER_POSITION.DWPOS_FIT,
                Style.Fill => DESKTOP_WALLPAPER_POSITION.DWPOS_FILL,
                Style.Span => DESKTOP_WALLPAPER_POSITION.DWPOS_SPAN,
                _ => DESKTOP_WALLPAPER_POSITION.DWPOS_FILL,
            };
        }

        private static Style PositionToStyle(DESKTOP_WALLPAPER_POSITION pos)
        {
            return pos switch
            {
                DESKTOP_WALLPAPER_POSITION.DWPOS_CENTER => Style.Center,
                DESKTOP_WALLPAPER_POSITION.DWPOS_TILE => Style.Tile,
                DESKTOP_WALLPAPER_POSITION.DWPOS_STRETCH => Style.Stretch,
                DESKTOP_WALLPAPER_POSITION.DWPOS_FIT => Style.Fit,
                DESKTOP_WALLPAPER_POSITION.DWPOS_FILL => Style.Fill,
                DESKTOP_WALLPAPER_POSITION.DWPOS_SPAN => Style.Span,
                _ => Style.Fill,
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
                h = (h ^ (uint)r.left) * 16777619u;
                h = (h ^ (uint)r.top) * 16777619u;
                h = (h ^ (uint)(r.right - r.left)) * 16777619u;
                h = (h ^ (uint)(r.bottom - r.top)) * 16777619u;
                return h.ToString("x8");
            }
        }

        private static void SetExplorerBackgroundType(BackgroundType backgroundType)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(ExplorerWallpapersKeyPath))
            {
                if (key == null)
                {
                    SharedLogger.logger.Warn($"Wallpaper/SetExplorerBackgroundType: Could not open or create HKCU\\{ExplorerWallpapersKeyPath}.");
                    return;
                }

                key.SetValue("BackgroundType", (int)backgroundType, RegistryValueKind.DWord);
            }
        }

        private static void SetDesktopSpotlightEnabled(bool enabled)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(DesktopSpotlightSettingsKeyPath))
            {
                if (key == null)
                {
                    SharedLogger.logger.Warn($"Wallpaper/SetDesktopSpotlightEnabled: Could not open or create HKCU\\{DesktopSpotlightSettingsKeyPath}.");
                    return;
                }

                key.SetValue("SpotlightDisabledReason", 100, RegistryValueKind.DWord);
                key.SetValue("EnabledState", enabled ? 1 : 0, RegistryValueKind.DWord);
            }

            SetDesktopLastUpdated();
        }

        private static void SetDesktopLastUpdated()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(ControlPanelDesktopKeyPath))
            {
                if (key == null)
                {
                    SharedLogger.logger.Warn($"Wallpaper/SetDesktopLastUpdated: Could not open or create HKCU\\{ControlPanelDesktopKeyPath}.");
                    return;
                }

                key.SetValue("LastUpdated", unchecked((int)0xFFFFFFFFu), RegistryValueKind.DWord); //0xFFFFFFFFu
            }
        }

        private static void SetDesktopSpotlightRefreshTime()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(DesktopSpotlightKeyPath))
            {
                if (key == null)
                {
                    SharedLogger.logger.Warn($"Wallpaper/SetDesktopSpotlightRefreshTime: Could not open or create HKCU\\{DesktopSpotlightKeyPath}.");
                    return;
                }

                string refreshTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                key.SetValue("WallpaperRefresh", refreshTime, RegistryValueKind.String);
            }
        }

        private static bool IsRegistryDwordEnabled(RegistryKey rootKey, string subKeyPath, string valueName)
        {
            try
            {
                using (var key = rootKey.OpenSubKey(subKeyPath))
                    return key?.GetValue(valueName) is int value && value != 0;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Trace($"Wallpaper/IsRegistryDwordEnabled: Could not read {rootKey.Name}\\{subKeyPath}\\{valueName}: {ex.Message}");
                return false;
            }
        }

        private static void LogDesktopSpotlightBlockers()
        {
            if (IsRegistryDwordEnabled(Registry.CurrentUser, CloudContentPolicyKeyPath, "DisableWindowsSpotlightFeatures") ||
                IsRegistryDwordEnabled(Registry.LocalMachine, CloudContentPolicyKeyPath, "DisableWindowsSpotlightFeatures"))
            {
                SharedLogger.logger.Warn("Wallpaper/LogDesktopSpotlightBlockers: Windows Spotlight is disabled by the DisableWindowsSpotlightFeatures policy.");
            }

            if (IsRegistryDwordEnabled(Registry.CurrentUser, CloudContentPolicyKeyPath, "DisableSpotlightCollectionOnDesktop") ||
                IsRegistryDwordEnabled(Registry.LocalMachine, CloudContentPolicyKeyPath, "DisableSpotlightCollectionOnDesktop"))
            {
                SharedLogger.logger.Warn("Wallpaper/LogDesktopSpotlightBlockers: Desktop Spotlight collection is disabled by policy.");
            }

            if (IsRegistryDwordEnabled(Registry.CurrentUser, BackgroundAccessApplicationsKeyPath, "GlobalUserDisabled"))
            {
                SharedLogger.logger.Warn("Wallpaper/LogDesktopSpotlightBlockers: Background apps are globally disabled for this user; Windows may reject Desktop Spotlight.");
            }
        }

        private static void BroadcastWallpaperSettingsChanged()
        {
            SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "Desktop", SMTO_ABORTIFHUNG, 2000, out _);
            SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "Control Panel\\Desktop", SMTO_ABORTIFHUNG, 2000, out _);
            SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "Wallpapers", SMTO_ABORTIFHUNG, 2000, out _);
            SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, ExplorerWallpapersKeyPath, SMTO_ABORTIFHUNG, 2000, out _);
            SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, null, SMTO_ABORTIFHUNG, 2000, out _);
        }

        /// <summary>
        /// Discovers and validates the AppX/SystemApp provider that implements Desktop Spotlight.
        /// This intentionally avoids hard-coding MicrosoftWindows.Client.CBS_cw5n1h2txyewy so that
        /// future package/version changes can still be handled.
        /// </summary>
        private static bool EnsureDesktopSpotlightProviderInitialised()
        {
            lock (DesktopSpotlightProviderLock)
            {
                if (_desktopSpotlightProvider != null)
                    return true;

                DesktopSpotlightProviderCandidate provider = SelectDesktopSpotlightProviderCandidate();
                if (provider == null)
                    return false;

                _desktopSpotlightProvider = provider;
                SharedLogger.logger.Info($"Wallpaper/EnsureDesktopSpotlightProviderInitialised: Selected provider '{provider.RuntimeClassName}' from '{provider.DllFullPath}'.");
                return true;
            }
        }

        private static DesktopSpotlightProviderCandidate SelectDesktopSpotlightProviderCandidate()
        {
            List<DesktopSpotlightProviderCandidate> candidates = DiscoverDesktopSpotlightProviderCandidates();

            if (candidates.Count == 0)
            {
                SharedLogger.logger.Warn($"Wallpaper/SelectDesktopSpotlightProviderCandidate: No AppX manifest was found declaring '{DesktopSpotlightExtensionContract}'.");
                return null;
            }

            SharedLogger.logger.Trace($"Wallpaper/SelectDesktopSpotlightProviderCandidate: Found {candidates.Count} manifest candidate(s).");

            List<DesktopSpotlightProviderCandidate> validCandidates = new List<DesktopSpotlightProviderCandidate>();

            foreach (DesktopSpotlightProviderCandidate candidate in candidates)
            {
                if (ValidateDesktopSpotlightProviderCandidate(candidate, out string validationMessage))
                {
                    candidate.ValidationMessage = validationMessage;
                    validCandidates.Add(candidate);

                    SharedLogger.logger.Trace(
                        $"Wallpaper/SelectDesktopSpotlightProviderCandidate: Valid candidate: package='{candidate.PackageName}', version='{candidate.PackageVersion}', dll='{candidate.DllFullPath}', {validationMessage}");
                }
                else
                {
                    SharedLogger.logger.Trace(
                        $"Wallpaper/SelectDesktopSpotlightProviderCandidate: Rejected candidate: package='{candidate.PackageName}', version='{candidate.PackageVersion}', dll='{candidate.DllFullPath}', reason='{validationMessage}'");
                }
            }

            if (validCandidates.Count == 0)
            {
                SharedLogger.logger.Warn("Wallpaper/SelectDesktopSpotlightProviderCandidate: No candidate passed validation.");
                return null;
            }

            // Select the newest working candidate, with a preference for Windows SystemApps over
            // WindowsApps packages. Validation is the key gate; version/modified time are tiebreakers.
            DesktopSpotlightProviderCandidate selected = validCandidates
                .OrderByDescending(c => c.IsSystemApp ? 1 : 0)
                .ThenByDescending(c => c.PackageVersion ?? new Version(0, 0, 0, 0))
                .ThenByDescending(c => GetSafeLastWriteTimeUtc(c.ManifestPath))
                .First();

            SharedLogger.logger.Info(
                $"Wallpaper/SelectDesktopSpotlightProviderCandidate: Selected package='{selected.PackageName}', version='{selected.PackageVersion}', dll='{selected.DllFullPath}'.");

            return selected;
        }

        private static List<DesktopSpotlightProviderCandidate> DiscoverDesktopSpotlightProviderCandidates()
        {
            List<DesktopSpotlightProviderCandidate> candidates = new List<DesktopSpotlightProviderCandidate>();

            foreach (string manifestPath in EnumeratePotentialAppxManifests())
            {
                try
                {
                    DesktopSpotlightProviderCandidate candidate = TryReadDesktopSpotlightProviderFromManifest(manifestPath);
                    if (candidate != null)
                        candidates.Add(candidate);
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Trace($"Wallpaper/DiscoverDesktopSpotlightProviderCandidates: Skipping manifest '{manifestPath}': {ex.Message}");
                }
            }

            return candidates;
        }

        private static IEnumerable<string> EnumeratePotentialAppxManifests()
        {
            string windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string systemAppsDir = Path.Combine(windowsDir, "SystemApps");

            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string windowsAppsDir = Path.Combine(programFiles, "WindowsApps");

            foreach (string path in EnumerateManifestsUnder(systemAppsDir))
                yield return path;

            foreach (string path in EnumerateManifestsUnder(windowsAppsDir))
                yield return path;
        }

        private static IEnumerable<string> EnumerateManifestsUnder(string root)
        {
            if (!Directory.Exists(root))
                yield break;

            IEnumerable<string> directories;

            try
            {
                directories = Directory.EnumerateDirectories(root);
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Trace($"Wallpaper/EnumerateManifestsUnder: Could not enumerate '{root}': {ex.Message}");
                yield break;
            }

            foreach (string directory in directories)
            {
                string manifest1 = Path.Combine(directory, "AppxManifest.xml");
                string manifest2 = Path.Combine(directory, "appxmanifest.xml");

                if (File.Exists(manifest1))
                    yield return manifest1;

                if (File.Exists(manifest2) && !string.Equals(manifest1, manifest2, StringComparison.OrdinalIgnoreCase))
                    yield return manifest2;
            }
        }

        private static DesktopSpotlightProviderCandidate TryReadDesktopSpotlightProviderFromManifest(string manifestPath)
        {
            string manifestText = File.ReadAllText(manifestPath);

            if (!manifestText.Contains(DesktopSpotlightExtensionContract, StringComparison.OrdinalIgnoreCase))
                return null;

            XDocument doc = XDocument.Parse(manifestText, LoadOptions.PreserveWhitespace);

            string packageDirectory = Path.GetDirectoryName(manifestPath);
            if (string.IsNullOrEmpty(packageDirectory))
                return null;

            XElement identity = doc
                .Descendants()
                .FirstOrDefault(e => string.Equals(e.Name.LocalName, "Identity", StringComparison.OrdinalIgnoreCase));

            string packageName = identity?.Attribute("Name")?.Value ?? Path.GetFileName(packageDirectory);
            string publisher = identity?.Attribute("Publisher")?.Value ?? "";
            Version packageVersion = TryParseVersion(identity?.Attribute("Version")?.Value);

            List<string> runtimeClassesFromContract = doc
                .Descendants()
                .Where(e => string.Equals(e.Name.LocalName, DesktopSpotlightExtensionContract, StringComparison.OrdinalIgnoreCase))
                .Select(e => (e.Value ?? string.Empty).Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            List<InProcessServerInfo> inProcessServers = doc
                .Descendants()
                .Where(e => string.Equals(e.Name.LocalName, "InProcessServer", StringComparison.OrdinalIgnoreCase))
                .Select(e => ReadInProcessServer(e, packageDirectory))
                .Where(s => s != null)
                .ToList();

            foreach (string runtimeClass in runtimeClassesFromContract)
            {
                InProcessServerInfo matchingServer = inProcessServers
                    .FirstOrDefault(s => s.ActivatableClasses.Contains(runtimeClass, StringComparer.OrdinalIgnoreCase));

                if (matchingServer != null)
                {
                    return new DesktopSpotlightProviderCandidate
                    {
                        PackageDirectory = packageDirectory,
                        ManifestPath = manifestPath,
                        PackageName = packageName,
                        Publisher = publisher,
                        PackageVersion = packageVersion,
                        RuntimeClassName = runtimeClass,
                        DllRelativePath = matchingServer.RelativePath,
                        DllFullPath = matchingServer.FullPath,
                        IsSystemApp = packageDirectory.StartsWith(
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SystemApps"),
                            StringComparison.OrdinalIgnoreCase)
                    };
                }
            }

            InProcessServerInfo fallbackServer = inProcessServers
                .FirstOrDefault(s =>
                    s.ActivatableClasses.Any(c => c.Contains("DesktopSpotlightProvider", StringComparison.OrdinalIgnoreCase)) ||
                    s.RelativePath.Contains("DesktopSpotlight", StringComparison.OrdinalIgnoreCase));

            if (fallbackServer == null)
                return null;

            string fallbackRuntimeClass =
                runtimeClassesFromContract.FirstOrDefault()
                ?? fallbackServer.ActivatableClasses.FirstOrDefault(c => c.Contains("DesktopSpotlightProvider", StringComparison.OrdinalIgnoreCase))
                ?? fallbackServer.ActivatableClasses.FirstOrDefault()
                ?? "DesktopSpotlight.DesktopSpotlightProvider";

            return new DesktopSpotlightProviderCandidate
            {
                PackageDirectory = packageDirectory,
                ManifestPath = manifestPath,
                PackageName = packageName,
                Publisher = publisher,
                PackageVersion = packageVersion,
                RuntimeClassName = fallbackRuntimeClass,
                DllRelativePath = fallbackServer.RelativePath,
                DllFullPath = fallbackServer.FullPath,
                IsSystemApp = packageDirectory.StartsWith(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SystemApps"),
                    StringComparison.OrdinalIgnoreCase)
            };
        }

        private static InProcessServerInfo ReadInProcessServer(XElement inProcessServer, string packageDirectory)
        {
            string pathValue = inProcessServer
                .Elements()
                .FirstOrDefault(e => string.Equals(e.Name.LocalName, "Path", StringComparison.OrdinalIgnoreCase))
                ?.Value
                ?.Trim();

            if (string.IsNullOrWhiteSpace(pathValue))
                return null;

            List<string> activatableClasses = inProcessServer
                .Descendants()
                .Where(e => string.Equals(e.Name.LocalName, "ActivatableClass", StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Attribute("ActivatableClassId")?.Value?.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            string fullPath = Path.IsPathRooted(pathValue)
                ? pathValue
                : Path.Combine(packageDirectory, pathValue);

            return new InProcessServerInfo
            {
                RelativePath = pathValue,
                FullPath = fullPath,
                ActivatableClasses = activatableClasses
            };
        }

        private static bool ValidateDesktopSpotlightProviderCandidate(DesktopSpotlightProviderCandidate candidate, out string validationMessage)
        {
            validationMessage = "";

            if (candidate == null)
            {
                validationMessage = "Candidate was null.";
                return false;
            }

            if (!File.Exists(candidate.DllFullPath))
            {
                validationMessage = "DesktopSpotlight DLL does not exist.";
                return false;
            }

            IntPtr factory = IntPtr.Zero;
            IntPtr statics = IntPtr.Zero;
            IntPtr extension = IntPtr.Zero;
            bool roInitialised = false;

            try
            {
                int hr = RoInitialize(RO_INIT_TYPE.RO_INIT_MULTITHREADED);
                roInitialised = hr >= 0;
                if (hr < 0)
                {
                    validationMessage = $"RoInitialize failed with HRESULT 0x{hr:X8}.";
                    return false;
                }

                factory = GetActivationFactoryViaDll(candidate.DllFullPath, candidate.RuntimeClassName);

                hr = QueryInterface(factory, IID_IDesktopSpotlightExtensionStatics, out statics);
                if (hr < 0 || statics == IntPtr.Zero)
                {
                    validationMessage = $"QI IDesktopSpotlightExtensionStatics failed with HRESULT 0x{hr:X8}.";
                    return false;
                }

                hr = GetDefaultFromStatics(statics, out extension);
                if (hr < 0 || extension == IntPtr.Zero)
                {
                    validationMessage = $"IDesktopSpotlightExtensionStatics.GetDefault failed with HRESULT 0x{hr:X8}.";
                    return false;
                }

                hr = GetWallpaperCount(extension, out uint wallpaperCount);
                if (hr < 0 || wallpaperCount == 0)
                {
                    validationMessage = $"get_WallpaperCount failed or returned zero. HRESULT 0x{hr:X8}, count {wallpaperCount}.";
                    return false;
                }

                hr = GetActiveWallpaperIndex(extension, out uint activeIndex);
                if (hr < 0)
                {
                    validationMessage = $"get_ActiveWallpaperIndex failed with HRESULT 0x{hr:X8}.";
                    return false;
                }

                int? registryImageIndex = ReadRegistryDword(DesktopSpotlightCreativesKeyPath, "ImageIndex");
                uint selectedIndex = SelectDesktopSpotlightWallpaperIndex(wallpaperCount, activeIndex, registryImageIndex);

                candidate.WallpaperCount = wallpaperCount;
                candidate.ActiveWallpaperIndex = activeIndex;
                candidate.RegistryImageIndex = registryImageIndex;
                candidate.SelectedWallpaperIndex = selectedIndex;

                validationMessage = $"wallpaperCount={wallpaperCount}, activeIndex={activeIndex}, registryImageIndex={(registryImageIndex.HasValue ? registryImageIndex.Value.ToString() : "<missing>")}, selectedIndex={selectedIndex}";
                return true;
            }
            catch (Exception ex)
            {
                validationMessage = ex.Message;
                return false;
            }
            finally
            {
                if (extension != IntPtr.Zero) Marshal.Release(extension);
                if (statics != IntPtr.Zero) Marshal.Release(statics);
                if (factory != IntPtr.Zero) Marshal.Release(factory);

                if (roInitialised)
                    RoUninitialize();
            }
        }

        private static bool ApplyDesktopSpotlight()
        {
            if (!EnsureDesktopSpotlightProviderInitialised())
            {
                SharedLogger.logger.Warn("Wallpaper/ApplyDesktopSpotlight: Could not initialise a usable Desktop Spotlight provider.");
                return false;
            }

            IntPtr factory = IntPtr.Zero;
            IntPtr statics = IntPtr.Zero;
            IntPtr extension = IntPtr.Zero;
            bool roInitialised = false;

            try
            {
                int hr = RoInitialize(RO_INIT_TYPE.RO_INIT_MULTITHREADED);
                roInitialised = hr >= 0;
                if (hr < 0)
                {
                    SharedLogger.logger.Warn($"Wallpaper/ApplyDesktopSpotlight: RoInitialize failed with HRESULT 0x{hr:X8}.");
                    return false;
                }

                factory = GetActivationFactoryViaDll(_desktopSpotlightProvider.DllFullPath, _desktopSpotlightProvider.RuntimeClassName);

                hr = QueryInterface(factory, IID_IDesktopSpotlightExtensionStatics, out statics);
                if (hr < 0 || statics == IntPtr.Zero)
                {
                    SharedLogger.logger.Warn($"Wallpaper/ApplyDesktopSpotlight: QI IDesktopSpotlightExtensionStatics failed with HRESULT 0x{hr:X8}.");
                    return false;
                }

                hr = GetDefaultFromStatics(statics, out extension);
                if (hr < 0 || extension == IntPtr.Zero)
                {
                    SharedLogger.logger.Warn($"Wallpaper/ApplyDesktopSpotlight: GetDefault failed with HRESULT 0x{hr:X8}.");
                    return false;
                }

                hr = GetWallpaperCount(extension, out uint wallpaperCount);
                if (hr < 0 || wallpaperCount == 0)
                {
                    SharedLogger.logger.Warn($"Wallpaper/ApplyDesktopSpotlight: get_WallpaperCount failed or returned zero. HRESULT 0x{hr:X8}, count {wallpaperCount}.");
                    return false;
                }

                hr = GetActiveWallpaperIndex(extension, out uint activeIndex);
                if (hr < 0)
                {
                    SharedLogger.logger.Warn($"Wallpaper/ApplyDesktopSpotlight: get_ActiveWallpaperIndex failed with HRESULT 0x{hr:X8}.");
                    return false;
                }

                int? registryImageIndex = ReadRegistryDword(DesktopSpotlightCreativesKeyPath, "ImageIndex");
                uint selectedIndex = SelectDesktopSpotlightWallpaperIndex(wallpaperCount, activeIndex, registryImageIndex);

                SharedLogger.logger.Info(
                    $"Wallpaper/ApplyDesktopSpotlight: Using provider '{_desktopSpotlightProvider.PackageName}' v{_desktopSpotlightProvider.PackageVersion}; wallpaperCount={wallpaperCount}, activeIndex={activeIndex}, registryImageIndex={(registryImageIndex.HasValue ? registryImageIndex.Value.ToString() : "<missing>")}, selectedIndex={selectedIndex}.");

                // Order matters. Windows Settings accepts the Spotlight mode when the supporting
                // DesktopSpotlight settings are in place before BackgroundType is switched to 3.
                WriteRegistryDword(DesktopSpotlightSettingsKeyPath, "EnabledState", 1);
                WriteRegistryDword(DesktopSpotlightSettingsKeyPath, "SpotlightDisabledReason", 100);
                WriteRegistryDword(ExplorerWallpapersKeyPath, "BackgroundType", (int)BackgroundType.Spotlight);

                hr = SetActiveWallpaperIndex(extension, selectedIndex);
                if (hr < 0)
                {
                    SharedLogger.logger.Warn($"Wallpaper/ApplyDesktopSpotlight: SetActiveWallpaperIndex({selectedIndex}) failed with HRESULT 0x{hr:X8}.");
                    return false;
                }

                int? finalBackgroundType = ReadRegistryDword(ExplorerWallpapersKeyPath, "BackgroundType");
                int? finalEnabledState = ReadRegistryDword(DesktopSpotlightSettingsKeyPath, "EnabledState");

                SharedLogger.logger.Info(
                    $"Wallpaper/ApplyDesktopSpotlight: Applied Desktop Spotlight. Final BackgroundType={finalBackgroundType?.ToString() ?? "<missing>"}, EnabledState={finalEnabledState?.ToString() ?? "<missing>"}.");

                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"Wallpaper/ApplyDesktopSpotlight: Exception applying Desktop Spotlight: {ex.Message}");
                return false;
            }
            finally
            {
                if (extension != IntPtr.Zero) Marshal.Release(extension);
                if (statics != IntPtr.Zero) Marshal.Release(statics);
                if (factory != IntPtr.Zero) Marshal.Release(factory);

                if (roInitialised)
                    RoUninitialize();
            }
        }

        private static uint SelectDesktopSpotlightWallpaperIndex(uint wallpaperCount, uint activeIndex, int? registryImageIndex)
        {
            if (wallpaperCount == 0)
                return 0;

            if (activeIndex < wallpaperCount)
                return activeIndex;

            if (registryImageIndex.HasValue &&
                registryImageIndex.Value >= 0 &&
                registryImageIndex.Value < wallpaperCount)
            {
                return (uint)registryImageIndex.Value;
            }

            return 0;
        }

        private static int? ReadRegistryDword(string subKeyPath, string valueName)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(subKeyPath))
                {
                    if (key?.GetValue(valueName) is int value)
                        return value;
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Trace($"Wallpaper/ReadRegistryDword: Could not read HKCU\\{subKeyPath}\\{valueName}: {ex.Message}");
            }

            return null;
        }

        private static void WriteRegistryDword(string subKeyPath, string valueName, int value)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(subKeyPath))
            {
                if (key == null)
                    throw new InvalidOperationException($"Could not open or create HKCU\\{subKeyPath}.");

                key.SetValue(valueName, value, RegistryValueKind.DWord);
                key.Flush();
            }
        }

        private static IntPtr GetActivationFactoryViaDll(string dllFullPath, string runtimeClassName)
        {
            IntPtr module = LoadLibraryExW(
                dllFullPath,
                IntPtr.Zero,
                LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR |
                LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);

            if (module == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"LoadLibraryExW failed for '{dllFullPath}'. Win32 error {error}: {new Win32Exception(error).Message}");
            }

            IntPtr proc = GetProcAddress(module, "DllGetActivationFactory");
            if (proc == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"GetProcAddress(DllGetActivationFactory) failed for '{dllFullPath}'. Win32 error {error}: {new Win32Exception(error).Message}");
            }

            IntPtr classHstring = CreateHString(runtimeClassName);

            try
            {
                DllGetActivationFactoryDelegate dllGetActivationFactory =
                    (DllGetActivationFactoryDelegate)Marshal.GetDelegateForFunctionPointer(proc, typeof(DllGetActivationFactoryDelegate));

                int hr = dllGetActivationFactory(classHstring, out IntPtr factory);
                if (hr < 0 || factory == IntPtr.Zero)
                    throw new COMException($"DllGetActivationFactory failed for '{runtimeClassName}'.", hr);

                return factory;
            }
            finally
            {
                WindowsDeleteString(classHstring);
            }
        }

        private static int QueryInterface(IntPtr unknown, Guid iid, out IntPtr iface)
        {
            return Marshal.QueryInterface(unknown, ref iid, out iface);
        }

        private static int GetDefaultFromStatics(IntPtr statics, out IntPtr extension)
        {
            IntPtr vtbl = Marshal.ReadIntPtr(statics);
            IntPtr getDefaultPtr = Marshal.ReadIntPtr(vtbl, 0x30);

            WinRtGetDefaultDelegate getDefault =
                (WinRtGetDefaultDelegate)Marshal.GetDelegateForFunctionPointer(getDefaultPtr, typeof(WinRtGetDefaultDelegate));

            return getDefault(statics, out extension);
        }

        private static int GetWallpaperCount(IntPtr extension, out uint count)
        {
            IntPtr vtbl = Marshal.ReadIntPtr(extension);
            IntPtr fn = Marshal.ReadIntPtr(vtbl, 0x30);

            WinRtGetUInt32Delegate getWallpaperCount =
                (WinRtGetUInt32Delegate)Marshal.GetDelegateForFunctionPointer(fn, typeof(WinRtGetUInt32Delegate));

            return getWallpaperCount(extension, out count);
        }

        private static int GetActiveWallpaperIndex(IntPtr extension, out uint index)
        {
            IntPtr vtbl = Marshal.ReadIntPtr(extension);
            IntPtr fn = Marshal.ReadIntPtr(vtbl, 0x38);

            WinRtGetUInt32Delegate getActiveWallpaperIndex =
                (WinRtGetUInt32Delegate)Marshal.GetDelegateForFunctionPointer(fn, typeof(WinRtGetUInt32Delegate));

            return getActiveWallpaperIndex(extension, out index);
        }

        private static int SetActiveWallpaperIndex(IntPtr extension, uint index)
        {
            IntPtr vtbl = Marshal.ReadIntPtr(extension);
            IntPtr fn = Marshal.ReadIntPtr(vtbl, 0x40);

            WinRtSetUInt32Delegate setActiveWallpaperIndex =
                (WinRtSetUInt32Delegate)Marshal.GetDelegateForFunctionPointer(fn, typeof(WinRtSetUInt32Delegate));

            return setActiveWallpaperIndex(extension, index);
        }

        private static IntPtr CreateHString(string value)
        {
            int hr = WindowsCreateString(value, value.Length, out IntPtr hstring);
            if (hr < 0)
                throw new COMException("WindowsCreateString failed.", hr);

            return hstring;
        }

        private static Version TryParseVersion(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return Version.TryParse(value, out Version version)
                ? version
                : null;
        }

        private static DateTime GetSafeLastWriteTimeUtc(string path)
        {
            try
            {
                return File.GetLastWriteTimeUtc(path);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private sealed class DesktopSpotlightProviderCandidate
        {
            public string PackageDirectory { get; set; } = "";
            public string ManifestPath { get; set; } = "";
            public string PackageName { get; set; } = "";
            public string Publisher { get; set; } = "";
            public Version PackageVersion { get; set; }
            public string RuntimeClassName { get; set; } = "";
            public string DllRelativePath { get; set; } = "";
            public string DllFullPath { get; set; } = "";
            public bool IsSystemApp { get; set; }
            public uint WallpaperCount { get; set; }
            public uint ActiveWallpaperIndex { get; set; }
            public int? RegistryImageIndex { get; set; }
            public uint SelectedWallpaperIndex { get; set; }
            public string ValidationMessage { get; set; } = "";
        }

        private sealed class InProcessServerInfo
        {
            public string RelativePath { get; set; } = "";
            public string FullPath { get; set; } = "";
            public List<string> ActivatableClasses { get; set; } = new List<string>();
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
                // Always capture the wallpaper state as something that should be applied.
                // The UI can still allow the user to opt out later by changing this to DoNothing.
                WallpaperMode = Mode.Apply,
                WallpaperStyle = Style.Fill,
                BackgroundColor = 0,
                BackgroundType = BackgroundType.Picture,
                MonitorWallpapers = new List<WallpaperMonitorConfig>()
            };

            IDesktopWallpaper idw = null;
            try
            {
                idw = (IDesktopWallpaper)new DesktopWallpaperClass();

                // Capture global style and background colour first. These are useful for
                // Picture, Slideshow, and Solid Colour captures.
                if (idw.GetPosition(out DESKTOP_WALLPAPER_POSITION pos) == 0)
                    config.WallpaperStyle = PositionToStyle(pos);

                if (idw.GetBackgroundColor(out uint bgColor) == 0)
                    config.BackgroundColor = bgColor;

                // Detect the active background mode from the most reliable source available.
                // IDesktopWallpaper.GetStatus() is more reliable for Slideshow than the
                // Explorer\Wallpapers\BackgroundType registry value, which can lag or be stale.
                int registryBackgroundType = 0;
                using (var regKey = Registry.CurrentUser.OpenSubKey(ExplorerWallpapersKeyPath))
                {
                    registryBackgroundType = regKey?.GetValue("BackgroundType") is int v ? v : 0;
                }

                bool slideshowCurrentlyActive = idw.GetStatus(out uint slideshowState) == 0
                                                && (slideshowState & 0x02u) != 0;

                if (slideshowCurrentlyActive)
                {
                    config.BackgroundType = BackgroundType.Slideshow;
                }
                else
                {
                    config.BackgroundType = registryBackgroundType switch
                    {
                        1 => BackgroundType.SolidColour,
                        2 => BackgroundType.Slideshow,
                        3 => BackgroundType.Spotlight,
                        _ => BackgroundType.Picture,
                    };
                }

                SharedLogger.logger.Trace($"Wallpaper/CaptureCurrentWallpaperConfig: Detected BackgroundType={config.BackgroundType}, RegistryBackgroundType={registryBackgroundType}, SlideshowStatus=0x{slideshowState:X8}.");

                // For Slideshow, capture interval, shuffle, and source folder via COM.
                // Empty MonitorWallpapers is expected and valid for Slideshow.
                if (config.BackgroundType == BackgroundType.Slideshow)
                {
                    if (idw.GetSlideshowOptions(out uint slideshowOpts, out uint slideshowTick) == 0)
                    {
                        config.SlideshowIntervalSeconds = slideshowTick / 1000;
                        config.SlideshowShuffle = (slideshowOpts & 0x01) != 0;
                    }

                    // GetSlideshow returns the IShellItemArray passed to SetSlideshow.
                    // The first item is either the folder itself or one of its images;
                    // walking up to the parent directory gives a reliable folder path.
                    if (idw.GetSlideshow(out IShellItemArray slideshowArray) == 0 && slideshowArray != null)
                    {
                        try
                        {
                            if (slideshowArray.GetItemAt(0, out IShellItem firstItem) == 0 && firstItem != null)
                            {
                                try
                                {
                                    if (firstItem.GetDisplayName(0x80058000u /* SIGDN_FILESYSPATH */, out string itemPath) == 0
                                        && !string.IsNullOrEmpty(itemPath))
                                    {
                                        config.SlideshowDirectoryPath = Directory.Exists(itemPath)
                                            ? itemPath
                                            : Path.GetDirectoryName(itemPath) ?? itemPath;
                                    }
                                }
                                finally { Marshal.ReleaseComObject(firstItem); }
                            }
                        }
                        finally { Marshal.ReleaseComObject(slideshowArray); }
                    }

                    SharedLogger.logger.Trace($"Wallpaper/CaptureCurrentWallpaperConfig: Captured Slideshow folder='{config.SlideshowDirectoryPath}', interval={config.SlideshowIntervalSeconds}s, shuffle={config.SlideshowShuffle}.");
                }

                // Build device-path -> bounds using DisplayConfigPaths + DisplayConfigModes.
                // This is only stored for Picture captures, but we build it here so the
                // Picture probing code stays self-contained.
                var devicePathToBounds = new Dictionary<string, RECT>(StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in windowsDisplayConfig.DisplaySources)
                {
                    foreach (var src in kvp.Value)
                    {
                        if (string.IsNullOrEmpty(src.DevicePath))
                            continue;

                        var rotation = DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_IDENTITY;
                        foreach (var path in windowsDisplayConfig.DisplayConfigPaths)
                        {
                            if (path.SourceInfo.Id == src.SourceId &&
                                path.SourceInfo.AdapterId.Value == src.AdapterId.Value)
                            {
                                rotation = path.TargetInfo.Rotation;
                                break;
                            }
                        }

                        foreach (var mode in windowsDisplayConfig.DisplayConfigModes)
                        {
                            if (mode.InfoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE &&
                                mode.Id == src.SourceId &&
                                mode.AdapterId.Value == src.AdapterId.Value)
                            {
                                int x = mode.SourceMode.Position.X;
                                int y = mode.SourceMode.Position.Y;
                                int w, h;
                                if (rotation == DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_ROTATE90 ||
                                    rotation == DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_ROTATE270)
                                {
                                    w = (int)mode.SourceMode.Height;
                                    h = (int)mode.SourceMode.Width;
                                }
                                else
                                {
                                    w = (int)mode.SourceMode.Width;
                                    h = (int)mode.SourceMode.Height;
                                }
                                devicePathToBounds[src.DevicePath] = new RECT
                                {
                                    left = x,
                                    top = y,
                                    right = x + w,
                                    bottom = y + h
                                };
                                break;
                            }
                        }
                    }
                }

                // For Picture mode, capture per-monitor wallpaper live paths.
                // If Windows reports Picture but every monitor has an empty wallpaper path,
                // treat it as Solid Colour rather than saving a broken Picture profile.
                if (config.BackgroundType == BackgroundType.Picture)
                {
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
                                SharedLogger.logger.Trace($"Wallpaper/CaptureCurrentWallpaperConfig: Monitor {monitorPath} has no wallpaper set.");
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

                            config.MonitorWallpapers.Add(new WallpaperMonitorConfig(monitorPath, wallpaperPath, monitorRect));
                            SharedLogger.logger.Trace($"Wallpaper/CaptureCurrentWallpaperConfig: Captured live wallpaper path for monitor {monitorPath} -> {wallpaperPath}");
                        }
                    }

                    if (config.MonitorWallpapers.Count == 0)
                    {
                        config.BackgroundType = BackgroundType.SolidColour;
                        SharedLogger.logger.Trace("Wallpaper/CaptureCurrentWallpaperConfig: Registry indicated Picture, but no monitor wallpaper paths were available; capturing as SolidColour.");
                    }
                }

                // These modes intentionally do not populate MonitorWallpapers.
                // That is not a failure and must not imply DoNothing.
                if (config.BackgroundType == BackgroundType.SolidColour ||
                    config.BackgroundType == BackgroundType.Slideshow ||
                    config.BackgroundType == BackgroundType.Spotlight)
                {
                    config.MonitorWallpapers.Clear();
                }

                // Always capture as Apply. The user can opt out later in the UI.
                config.WallpaperMode = Mode.Apply;

                SharedLogger.logger.Trace($"Wallpaper/CaptureCurrentWallpaperConfig: Final capture BackgroundType={config.BackgroundType}, WallpaperMode={config.WallpaperMode}, MonitorWallpapers={config.MonitorWallpapers.Count}.");
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"Wallpaper/CaptureCurrentWallpaperConfig: Exception capturing wallpaper configuration: {ex.Message}");
                config.WallpaperMode = Mode.Apply;
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

                // DSS_SLIDESHOW (0x02): a slideshow is currently configured via IDesktopWallpaper.
                // Using GetStatus() is more reliable than reading the registry BackgroundType value,
                // which can lag behind the actual COM state.
                // Spotlight does NOT set DSS_SLIDESHOW (it is managed by ContentDeliveryManager),
                // so this check is inherently Spotlight-safe without a separate registry guard.
                bool slideshowCurrentlyActive = idw.GetStatus(out uint slideshowState) == 0
                                                && (slideshowState & 0x02u) != 0;


                switch (config.BackgroundType)
                {
                    case BackgroundType.SolidColour:
                        {
                            if (slideshowCurrentlyActive) idw.SetSlideshow(null); // stop running slideshow
                            SetDesktopSpotlightEnabled(false);

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
                            SetExplorerBackgroundType(BackgroundType.SolidColour);
                            BroadcastWallpaperSettingsChanged();
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

                            SetDesktopSpotlightEnabled(false);
                            idw.SetPosition(StyleToPosition(config.WallpaperStyle));
                            idw.SetBackgroundColor(config.BackgroundColor);

                            Guid iShellItemGuid = typeof(IShellItem).GUID;
                            Guid iShellItemArrayGuid = typeof(IShellItemArray).GUID;
                            SHCreateItemFromParsingName(config.SlideshowDirectoryPath, IntPtr.Zero, iShellItemGuid, out IShellItem folderItem);
                            SHCreateShellItemArrayFromShellItem(folderItem, iShellItemArrayGuid, out IShellItemArray itemArray);
                            try
                            {
                                idw.SetSlideshow(itemArray);
                            }
                            finally
                            {
                                if (itemArray != null) Marshal.ReleaseComObject(itemArray);
                                if (folderItem != null) Marshal.ReleaseComObject(folderItem);
                            }

                            uint slideshowOpts = config.SlideshowShuffle ? 0x01u : 0u;
                            uint intervalMs = config.SlideshowIntervalSeconds * 1000u;
                            if (intervalMs < 10000u) intervalMs = 10000u;   // Windows minimum is 10 seconds
                            idw.SetSlideshowOptions(slideshowOpts, intervalMs);

                            SetExplorerBackgroundType(BackgroundType.Slideshow);
                            BroadcastWallpaperSettingsChanged();
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

                            LogDesktopSpotlightBlockers();

                            if (!ApplyDesktopSpotlight())
                            {
                                SharedLogger.logger.Warn("Wallpaper/Apply: Failed to apply Windows Spotlight using the DesktopSpotlight provider.");
                                break;
                            }

                            SharedLogger.logger.Info("Wallpaper/Apply: Windows Spotlight re-enabled using the DesktopSpotlight provider.");
                            break;
                        }

                    default: // BackgroundType.Picture
                        {
                            if (config.MonitorWallpapers.Count == 0)
                            {
                                SharedLogger.logger.Trace("Wallpaper/Apply: Picture mode with no stored wallpapers, skipping.");
                                break;
                            }

                            SetDesktopSpotlightEnabled(false);
                            SetExplorerBackgroundType(BackgroundType.Picture);

                            if (slideshowCurrentlyActive) idw.SetSlideshow(null); // stop running slideshow
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

                            BroadcastWallpaperSettingsChanged();
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
