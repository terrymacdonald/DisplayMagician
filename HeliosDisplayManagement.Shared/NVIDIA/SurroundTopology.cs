using System;
using System.Drawing;
using System.Linq;
using WindowsDisplayAPI.DisplayConfig;
using NvAPIWrapper.Mosaic;
using NvAPIWrapper.Native.Interfaces.Mosaic;

namespace HeliosDisplayManagement.Shared.NVIDIA
{
    public class SurroundTopology : IEquatable<SurroundTopology>
    {
        public SurroundTopology(GridTopology topology)
        {
            Rows = topology.Rows;
            Columns = topology.Columns;
            Resolution = new Size(topology.Resolution.Width, topology.Resolution.Height);
            ColorDepth = topology.Resolution.ColorDepth;
            Frequency = topology.Frequency;
            Displays =
                topology.Displays.Where(
                        display =>
                            (Resolution.Width > display.Overlap.HorizontalOverlap) &&
                            (Resolution.Height > display.Overlap.VerticalOverlap))
                    .Select(display => new SurroundTopologyDisplay(display))
                    .ToArray();
            ApplyWithBezelCorrectedResolution = topology.ApplyWithBezelCorrectedResolution;
            ImmersiveGaming = topology.ImmersiveGaming;
            BaseMosaicPanoramic = topology.BaseMosaicPanoramic;
            DriverReloadAllowed = topology.DriverReloadAllowed;
            AcceleratePrimaryDisplay = topology.AcceleratePrimaryDisplay;
        }

        public SurroundTopology()
        {
        }

        public bool AcceleratePrimaryDisplay { get; set; }
        public bool ApplyWithBezelCorrectedResolution { get; set; }
        public bool BaseMosaicPanoramic { get; set; }
        public int ColorDepth { get; set; }
        public int Columns { get; set; }
        public SurroundTopologyDisplay[] Displays { get; set; }
        public bool DriverReloadAllowed { get; set; }
        public int Frequency { get; set; }
        public bool ImmersiveGaming { get; set; }
        public Size Resolution { get; set; }
        public int Rows { get; set; }

        /// <inheritdoc />
        public bool Equals(SurroundTopology other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return (AcceleratePrimaryDisplay == other.AcceleratePrimaryDisplay) &&
                   (ApplyWithBezelCorrectedResolution == other.ApplyWithBezelCorrectedResolution) &&
                   (BaseMosaicPanoramic == other.BaseMosaicPanoramic) && (ColorDepth == other.ColorDepth) &&
                   (Columns == other.Columns) && (Displays.Length == other.Displays.Length) &&
                   Displays.All(display => other.Displays.Contains(display)) &&
                   (DriverReloadAllowed == other.DriverReloadAllowed) && (Frequency == other.Frequency) &&
                   (ImmersiveGaming == other.ImmersiveGaming) && Resolution.Equals(other.Resolution) &&
                   (Rows == other.Rows);
        }

        public static SurroundTopology FromPathTargetInfo(PathTargetInfo pathTargetInfo)
        {
            // We go through the code if only the path belongs to a NVIDIA virtual surround display
            if (pathTargetInfo.DisplayTarget.EDIDManufactureCode != "NVS")
                return null;
            try
            {
                var correspondingWindowsPathInfo =
                    PathInfo.GetAllPaths()
                        .FirstOrDefault(
                            info =>
                                info.TargetsInfo.Any(
                                    targetInfo => targetInfo.DisplayTarget == pathTargetInfo.DisplayTarget));
                if (correspondingWindowsPathInfo != null)
                {
                    // If position is same, then the two paths are equal, after all position is whats important in path sources
                    var correspondingNvidiaPathInfo =
                        NvAPIWrapper.Display.PathInfo.GetDisplaysConfig()
                            .FirstOrDefault(
                                info =>
                                    (info.Position.X == correspondingWindowsPathInfo.Position.X) &&
                                    (info.Position.Y == correspondingWindowsPathInfo.Position.Y) &&
                                    (info.Resolution.Width == correspondingWindowsPathInfo.Resolution.Width) &&
                                    (info.Resolution.Height == correspondingWindowsPathInfo.Resolution.Height));
                    if (correspondingNvidiaPathInfo != null)
                    {
                        // We now assume that there is only one target for a NVS path
                        var correspondingNvidiaTargetInfo = correspondingNvidiaPathInfo.TargetsInfo.FirstOrDefault();
                        if (correspondingNvidiaTargetInfo != null)
                        {
                            var correspondingNvidiaTopology =
                                GridTopology.GetGridTopologies()
                                    .FirstOrDefault(
                                        topology =>
                                            topology.Displays.Select(display => display.DisplayDevice)
                                                .Contains(correspondingNvidiaTargetInfo.DisplayDevice));
                            if (correspondingNvidiaTopology != null)
                                return new SurroundTopology(correspondingNvidiaTopology);
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
            return null;
        }
        
        public static bool operator ==(SurroundTopology left, SurroundTopology right)
        {
            return Equals(left, right) || left?.Equals(right) == true;
        }
        
        public static bool operator !=(SurroundTopology left, SurroundTopology right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SurroundTopology) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = AcceleratePrimaryDisplay.GetHashCode();
                hashCode = (hashCode*397) ^ ApplyWithBezelCorrectedResolution.GetHashCode();
                hashCode = (hashCode*397) ^ BaseMosaicPanoramic.GetHashCode();
                hashCode = (hashCode*397) ^ ColorDepth;
                hashCode = (hashCode*397) ^ Columns;
                hashCode = (hashCode*397) ^ (Displays?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ DriverReloadAllowed.GetHashCode();
                hashCode = (hashCode*397) ^ Frequency;
                hashCode = (hashCode*397) ^ ImmersiveGaming.GetHashCode();
                hashCode = (hashCode*397) ^ Resolution.GetHashCode();
                hashCode = (hashCode*397) ^ Rows;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"SurroundTopology[{Rows}, {Columns}] ({Resolution.Width}, {Resolution.Height}) @ {Frequency}";
        }

        // ReSharper disable once ExcessiveIndentation
        public GridTopology ToGridTopology()
        {
            var gridTopology = new GridTopology(Rows, Columns,
                Displays.Select(display => display.ToGridTopologyDisplay()).ToArray())
            {
                ApplyWithBezelCorrectedResolution = ApplyWithBezelCorrectedResolution,
                ImmersiveGaming = ImmersiveGaming,
                BaseMosaicPanoramic = BaseMosaicPanoramic,
                DriverReloadAllowed = DriverReloadAllowed,
                AcceleratePrimaryDisplay = AcceleratePrimaryDisplay
            };
            IDisplaySettings bestDisplaySettings = null;
            foreach (var displaySetting in gridTopology.GetPossibleDisplaySettings())
                if ((displaySetting.Width == Resolution.Width) &&
                    (displaySetting.Height == Resolution.Height))
                {
                    if (displaySetting.BitsPerPixel == ColorDepth)
                    {
                        if (displaySetting.Frequency == Frequency)
                        {
                            bestDisplaySettings = displaySetting;
                            break;
                        }
                        if ((bestDisplaySettings == null) || (displaySetting.Frequency > bestDisplaySettings.Frequency))
                            bestDisplaySettings = displaySetting;
                    }
                    else if ((bestDisplaySettings == null) ||
                             (displaySetting.BitsPerPixel > bestDisplaySettings.BitsPerPixel))
                    {
                        bestDisplaySettings = displaySetting;
                    }
                }
                else if ((bestDisplaySettings == null) ||
                         (displaySetting.Width*displaySetting.Height >
                          bestDisplaySettings.Width*bestDisplaySettings.Height))
                {
                    bestDisplaySettings = displaySetting;
                }
            if (bestDisplaySettings != null)
                gridTopology.SetDisplaySettings(bestDisplaySettings);
            return gridTopology;
        }
    }
}