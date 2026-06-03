using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using WindowsWallpaperWrapper;
using WwwRect = WindowsWallpaperWrapper.Interop.RECT;

namespace DisplayMagicianShared
{
    // ---------------------------------------------------------------------------
    // Full wallpaper configuration stored on a display profile.
    // WallpaperSettings holds the wrapper's polymorphic config object; the
    // WallpaperMode flag is DisplayMagician-specific (Apply vs DoNothing).
    // ---------------------------------------------------------------------------
    public class WallpaperConfig : IEquatable<WallpaperConfig>
    {
        /// <summary>What to do with wallpapers when this profile is applied.</summary>
        public Wallpaper.Mode WallpaperMode { get; set; } = Wallpaper.Mode.Apply;

        /// <summary>
        /// The wrapper library's polymorphic wallpaper configuration.
        /// Stored as the concrete subtype using Newtonsoft.Json TypeNameHandling.Auto,
        /// which is already enabled in ProfileRepository.
        /// </summary>
        public WindowsWallpaperConfig WallpaperSettings { get; set; }

        // Convenience accessor — returns the per-monitor list when in Picture mode,
        // or an empty list for all other modes. Allows callers (DisplayView,
        // ProfileRepository cleanup) to iterate monitors without casting.
        [JsonIgnore]
        public List<MonitorWallpaperConfig> MonitorWallpapers =>
            (WallpaperSettings as PictureWallpaperConfig)?.MonitorWallpapers
            ?? new List<MonitorWallpaperConfig>();

        public WallpaperConfig() { }

        public override bool Equals(object obj) => obj is WallpaperConfig other && Equals(other);
        public bool Equals(WallpaperConfig other)
        {
            if (other is null) return false;
            if (WallpaperMode != other.WallpaperMode) return false;
            if (WallpaperSettings is null != other.WallpaperSettings is null) return false;
            if (WallpaperSettings is null) return true;
            if (WallpaperSettings.GetType() != other.WallpaperSettings.GetType()) return false;
            // Deep equality via JSON round-trip – good enough for profile comparison.
            return JsonConvert.SerializeObject(WallpaperSettings) ==
                   JsonConvert.SerializeObject(other.WallpaperSettings);
        }
        public override int GetHashCode() => (WallpaperMode, WallpaperSettings?.GetType()?.Name, MonitorWallpapers.Count).GetHashCode();
        public static bool operator ==(WallpaperConfig lhs, WallpaperConfig rhs)
            => lhs is null ? rhs is null : lhs.Equals(rhs);
        public static bool operator !=(WallpaperConfig lhs, WallpaperConfig rhs)
            => !(lhs == rhs);
    }

    // ---------------------------------------------------------------------------
    // Static wallpaper management: capture current state, apply, persist files.
    // Delegates capture and apply to the WindowsWallpaperWrapper library.
    // ---------------------------------------------------------------------------
    public static class Wallpaper
    {
        // -----------------------------------------------------------------------
        // Enumerations
        // -----------------------------------------------------------------------

        public enum Mode : int
        {
            Apply = 0,
            DoNothing = 1,
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// FNV-1a hash of a monitor's position and size, used to derive deterministic
        /// filenames for stored wallpaper images.
        /// </summary>
        private static string MonitorBoundsHash(WwwRect r)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ (uint)r.left)  * 16777619u;
                h = (h ^ (uint)r.top)   * 16777619u;
                h = (h ^ (uint)(r.right  - r.left)) * 16777619u;
                h = (h ^ (uint)(r.bottom - r.top))  * 16777619u;
                return h.ToString("x8");
            }
        }

        // -----------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------

        /// <summary>
        /// Snapshots the current wallpaper configuration by calling
        /// <see cref="WallpaperWrapper.GetCurrentWallpaperConfig"/>.
        /// Captures metadata and live image paths only — no files are copied at
        /// this stage. Call <see cref="SaveWallpaperFiles"/> (e.g. from
        /// <c>PreSave</c>) to copy images into permanent storage.
        /// Returns a <see cref="WallpaperConfig"/> ready to be stored on a
        /// <see cref="ProfileItem"/>.
        /// </summary>
        public static WallpaperConfig CaptureCurrentWallpaperConfig()
        {
            var config = new WallpaperConfig { WallpaperMode = Mode.Apply };
            try
            {
                config.WallpaperSettings = WallpaperWrapper.GetCurrentWallpaperConfig();
                SharedLogger.logger.Trace($"Wallpaper/CaptureCurrentWallpaperConfig: Captured BackgroundType={config.WallpaperSettings?.BackgroundType}, MonitorWallpapers={config.MonitorWallpapers.Count}.");
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"Wallpaper/CaptureCurrentWallpaperConfig: Exception capturing wallpaper configuration: {ex.Message}");
                config.WallpaperMode = Mode.Apply;
            }
            return config;
        }

        /// <summary>
        /// Copies each monitor's wallpaper image into <paramref name="storePath"/>
        /// using a deterministic filename derived from <paramref name="profileUUID"/>
        /// and the monitor bounds hash, then updates
        /// <see cref="MonitorWallpaperConfig.WallpaperFilePath"/> in-place to the
        /// new destination. Safe to call multiple times (idempotent).
        /// Only operates when <c>WallpaperSettings</c> is a
        /// <see cref="PictureWallpaperConfig"/>.
        /// </summary>
        public static void SaveWallpaperFiles(WallpaperConfig config, string storePath, string profileUUID)
        {
            if (config?.WallpaperSettings is not PictureWallpaperConfig picture)
                return;

            if (!Directory.Exists(storePath))
                Directory.CreateDirectory(storePath);

            foreach (var mon in picture.MonitorWallpapers)
            {
                if (string.IsNullOrEmpty(mon.WallpaperFilePath))
                    continue;

                string ext = Path.GetExtension(mon.WallpaperFilePath);
                if (string.IsNullOrEmpty(ext)) ext = ".png";
                string dest = Path.Combine(storePath, $"wallpaper-{profileUUID}-{MonitorBoundsHash(mon.MonitorBounds)}{ext}");

                try
                {
                    if (string.Equals(Path.GetFullPath(mon.WallpaperFilePath), Path.GetFullPath(dest),
                                      StringComparison.OrdinalIgnoreCase))
                    {
                        mon.WallpaperFilePath = dest;
                        SharedLogger.logger.Trace($"Wallpaper/SaveWallpaperFiles: Wallpaper for monitor {mon.MonitorHardwareId}/{mon.MonitorConnectorId} already at destination, skipping copy.");
                        continue;
                    }
                }
                catch { /* invalid path — fall through */ }

                if (!File.Exists(mon.WallpaperFilePath))
                {
                    SharedLogger.logger.Warn($"Wallpaper/SaveWallpaperFiles: Source file '{mon.WallpaperFilePath}' does not exist for monitor {mon.MonitorHardwareId}/{mon.MonitorConnectorId}, skipping.");
                    continue;
                }

                try
                {
                    File.Copy(mon.WallpaperFilePath, dest, overwrite: true);
                    mon.WallpaperFilePath = dest;
                    SharedLogger.logger.Trace($"Wallpaper/SaveWallpaperFiles: Saved wallpaper for monitor {mon.MonitorHardwareId}/{mon.MonitorConnectorId} -> {dest}");
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"Wallpaper/SaveWallpaperFiles: Exception copying wallpaper for monitor {mon.MonitorHardwareId}/{mon.MonitorConnectorId}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Applies a previously captured <see cref="WallpaperConfig"/> to the
        /// desktop by calling
        /// <see cref="WallpaperWrapper.SetCurrentWallpaperConfig"/>.
        /// </summary>
        public static bool Apply(WallpaperConfig config)
        {
            if (config?.WallpaperSettings == null)
            {
                SharedLogger.logger.Warn("Wallpaper/Apply: Null WallpaperConfig or WallpaperSettings supplied, nothing to apply.");
                return false;
            }

            try
            {
                WallpaperWrapper.SetCurrentWallpaperConfig(config.WallpaperSettings);
                SharedLogger.logger.Trace($"Wallpaper/Apply: Applied wallpaper config BackgroundType={config.WallpaperSettings.BackgroundType}.");
                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"Wallpaper/Apply: Exception applying wallpaper configuration: {ex.Message}");
                return false;
            }
        }
    }
}

