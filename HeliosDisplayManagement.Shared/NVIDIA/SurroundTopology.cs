using System;
using System.Drawing;
using System.Linq;
using WindowsDisplayAPI.DisplayConfig;
using NvAPIWrapper.Mosaic;
using NvAPIWrapper.Native.Interfaces.Mosaic;

namespace HeliosPlus.Shared.NVIDIA
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
                            Resolution.Width > display.Overlap.HorizontalOverlap &&
                            Resolution.Height > display.Overlap.VerticalOverlap)
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
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (AcceleratePrimaryDisplay.Equals(other.AcceleratePrimaryDisplay))
                Console.WriteLine("accprimdisp is true");
            if (ApplyWithBezelCorrectedResolution.Equals(other.ApplyWithBezelCorrectedResolution))
                Console.WriteLine("appwithbezelcorrectres is true");
            if (BaseMosaicPanoramic.Equals(other.BaseMosaicPanoramic))
                Console.WriteLine("basemosiacis true");
            if (ColorDepth.Equals(other.ColorDepth))
                Console.WriteLine("colordepth is true");
            if (Columns.Equals(other.Columns) )
                Console.WriteLine("colums is true");
            if (Displays.Length.Equals(other.Displays.Length))
                Console.WriteLine("disp length is true");
            if (Displays.All(display => other.Displays.Contains(display)))
                Console.WriteLine("disp all is true");
            if (DriverReloadAllowed.Equals(other.DriverReloadAllowed) )
                Console.WriteLine("driver reload is true");
            if (Frequency.Equals(other.Frequency) )
                Console.WriteLine("freq is true");
            if (ImmersiveGaming.Equals(other.ImmersiveGaming) )
                Console.WriteLine("immers is true");
            if (Resolution.Equals(other.Resolution))
                Console.WriteLine("res is true");
            if (Rows.Equals(other.Rows))
                Console.WriteLine("rows is true");


            if (AcceleratePrimaryDisplay.Equals(other.AcceleratePrimaryDisplay) &&
                   ApplyWithBezelCorrectedResolution.Equals(other.ApplyWithBezelCorrectedResolution) &&
                   BaseMosaicPanoramic.Equals(other.BaseMosaicPanoramic) &&
                   ColorDepth.Equals(other.ColorDepth) &&
                   Columns.Equals(other.Columns) &&
                   Displays.Length.Equals(other.Displays.Length) &&
                   Displays.All(display => other.Displays.Contains(display)) &&
                   DriverReloadAllowed.Equals(other.DriverReloadAllowed) &&
                   Frequency.Equals(other.Frequency) &&
                   ImmersiveGaming.Equals(other.ImmersiveGaming) &&
                   Resolution.Equals(other.Resolution) &&
                   Rows.Equals(other.Rows))
            {
                return true;
            }

            return false;
        }

        // ReSharper disable once ExcessiveIndentation
        public static SurroundTopology FromPathTargetInfo(PathTargetInfo pathTargetInfo)
        {
            // We go through the code if only the path belongs to a NVIDIA virtual surround display
            // TODO: Should we try to resolve every target info to be sure?
            if (!string.Equals(
                    pathTargetInfo.DisplayTarget.EDIDManufactureCode,
                    "NVS",
                    StringComparison.InvariantCultureIgnoreCase
                ) &&
                !string.Equals(
                    pathTargetInfo.DisplayTarget.FriendlyName,
                    "NV Surround",
                    StringComparison.InvariantCultureIgnoreCase
                ) &&
                !pathTargetInfo.DisplayTarget.DevicePath.ToLower().Contains("&UID5120".ToLower()))
            {
                return null;
            }

            try
            {
                // Get parent DisplayConfig PathInfo by checking display targets
                var correspondingWindowsPathInfo =
                    PathInfo.GetActivePaths()
                        .FirstOrDefault(
                            info =>
                                info.TargetsInfo.Any(
                                    targetInfo =>
                                        targetInfo.DisplayTarget == pathTargetInfo.DisplayTarget));

                if (correspondingWindowsPathInfo != null)
                {
                    // Get corresponding NvAPI PathInfo
                    // If position is same, then the two paths are equal, after all position is whats important in path sources
                    var correspondingNvidiaPathInfo =
                        NvAPIWrapper.Display.PathInfo.GetDisplaysConfig()
                            .FirstOrDefault(
                                info =>
                                    info.Position.X == correspondingWindowsPathInfo.Position.X &&
                                    info.Position.Y == correspondingWindowsPathInfo.Position.Y &&
                                    info.Resolution.Width == correspondingWindowsPathInfo.Resolution.Width &&
                                    info.Resolution.Height == correspondingWindowsPathInfo.Resolution.Height);

                    if (correspondingNvidiaPathInfo != null)
                    {
                        // Get corresponding NvAPI PathTargetInfo
                        // We now assume that there is only one target for a NvAPI PathInfo, in an other word, for now, it is not possible to have a cloned surround display
                        var correspondingNvidiaTargetInfo = correspondingNvidiaPathInfo.TargetsInfo.FirstOrDefault();

                        if (correspondingNvidiaTargetInfo != null)
                        {
                            // Get corresponding NvAPI Grid Topology
                            // We also assume that the NVS monitor uses a similar display id to one of real physical monitors
                            var correspondingNvidiaTopology =
                                GridTopology.GetGridTopologies()
                                    .FirstOrDefault(
                                        topology => topology.Displays.Any(display =>
                                            display.DisplayDevice == correspondingNvidiaTargetInfo.DisplayDevice));

                            if (correspondingNvidiaTopology != null)
                            {
                                return new SurroundTopology(correspondingNvidiaTopology);
                            }
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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((SurroundTopology) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = AcceleratePrimaryDisplay.GetHashCode();
                hashCode = (hashCode * 397) ^ ApplyWithBezelCorrectedResolution.GetHashCode();
                hashCode = (hashCode * 397) ^ BaseMosaicPanoramic.GetHashCode();
                hashCode = (hashCode * 397) ^ ColorDepth;
                hashCode = (hashCode * 397) ^ Columns;
                hashCode = (hashCode * 397) ^ (Displays?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ DriverReloadAllowed.GetHashCode();
                hashCode = (hashCode * 397) ^ Frequency;
                hashCode = (hashCode * 397) ^ ImmersiveGaming.GetHashCode();
                hashCode = (hashCode * 397) ^ Resolution.GetHashCode();
                hashCode = (hashCode * 397) ^ Rows;

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
            {
                if (displaySetting.Width == Resolution.Width &&
                    displaySetting.Height == Resolution.Height)
                {
                    if (displaySetting.BitsPerPixel == ColorDepth)
                    {
                        if (displaySetting.Frequency == Frequency)
                        {
                            bestDisplaySettings = displaySetting;

                            break;
                        }

                        if (bestDisplaySettings == null || displaySetting.Frequency > bestDisplaySettings.Frequency)
                        {
                            bestDisplaySettings = displaySetting;
                        }
                    }
                    else if (bestDisplaySettings == null ||
                             displaySetting.BitsPerPixel > bestDisplaySettings.BitsPerPixel)
                    {
                        bestDisplaySettings = displaySetting;
                    }
                }
                else if (bestDisplaySettings == null ||
                         displaySetting.Width * displaySetting.Height >
                         bestDisplaySettings.Width * bestDisplaySettings.Height)
                {
                    bestDisplaySettings = displaySetting;
                }
            }

            if (bestDisplaySettings != null)
            {
                gridTopology.SetDisplaySettings(bestDisplaySettings);
            }

            return gridTopology;
        }
    }
}