using System;
using System.Drawing;
using System.Linq;
using WindowsDisplayAPI.DisplayConfig;
using NvAPIWrapper.Mosaic;
using NvAPIWrapper.Native.Interfaces.Mosaic;
using System.Collections.Generic;

namespace DisplayMagicianShared.NVIDIA
{
    public class SurroundTopology
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

  
        // ReSharper disable once ExcessiveIndentation
        public static SurroundTopology FromPathTargetInfo(PathTargetInfo pathTargetInfo)
        {
            // We go through the code if only the path belongs to a NVIDIA virtual surround display
            // and is not null
            if (pathTargetInfo == null)
            {
                SharedLogger.logger.Trace($"SurroundTopology/FromPathTargetInfo: The PathTargetInfo object supplied was null, so we have to return null back.");
                return null;
            }

            string EDIDManufactureCode = "";
            string friendlyName = "";
            bool devicePathContainsUID5120 = false;
            bool isNvidiaEDID = false;
            bool isNvidiaDeviceName = false;
            bool isNvidiaDevicePath = false;

            try
            {
                EDIDManufactureCode = pathTargetInfo.DisplayTarget.EDIDManufactureCode;
                SharedLogger.logger.Trace($"SurroundTopology/FromPathTargetInfo: Grabbed EDIDManufactureCode of {EDIDManufactureCode}.");
                if (string.Equals(EDIDManufactureCode, "NVS", StringComparison.InvariantCultureIgnoreCase))
                {
                    SharedLogger.logger.Trace($"SurroundTopology/FromPathTargetInfo: The EDIDManufactureCode of {EDIDManufactureCode} matches 'NVS', so this is an NVIDIA surround topology.");
                    isNvidiaEDID = true;
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex, $"SurroundTopology/FromPathTargetInfo: Exception trying to access EDIDManufactureCode.");
            }

            try
            {
                friendlyName = pathTargetInfo.DisplayTarget.FriendlyName;
                SharedLogger.logger.Trace($"SurroundTopology/FromPathTargetInfo: Grabbed Display FriendlyName of {friendlyName}.");
                if (string.Equals(friendlyName, "NV Surround", StringComparison.InvariantCultureIgnoreCase))
                {
                    SharedLogger.logger.Trace($"SurroundTopology/FromPathTargetInfo: The Display FriendlyName of {friendlyName} matches 'NV Surround', so this is an NVIDIA surround topology.");
                    isNvidiaDeviceName = true;
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex, $"SurroundTopology/FromPathTargetInfo: Exception trying to access friendlyName.");
            }

            try
            {
                devicePathContainsUID5120 = pathTargetInfo.DisplayTarget.DevicePath.ToLower().Contains("&UID5120".ToLower());
                SharedLogger.logger.Trace($"SurroundTopology/FromPathTargetInfo: Testing if the Display DevicePath contains UID5120 = {devicePathContainsUID5120}.");
                if (devicePathContainsUID5120)
                {
                    SharedLogger.logger.Trace($"SurroundTopology/FromPathTargetInfo: The Device Path contains UID5120, so this is an NVIDIA surround topology.");
                    isNvidiaDevicePath = true;
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex, $"SurroundTopology/FromPathTargetInfo: Exception trying to access friendlyName.");
            }

            // If the checks haven't passed, then we return null
            if (!isNvidiaEDID && !isNvidiaDeviceName && !isNvidiaDevicePath)
            {
                SharedLogger.logger.Warn($"SurroundTopology/FromPathTargetInfo: As far as we can tell, this isn't an NVIDIA Surround window, so we're returning null.");
                return null;
            }

            try
            {
                // Get parent DisplayConfig PathInfo by checking display targets
                SharedLogger.logger.Trace($"SurroundTopology/FromPathTargetInfo: Get parent DisplayConfig PathInfo by checking display targets.");
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
                    SharedLogger.logger.Trace($"SurroundTopology/FromPathTargetInfo: Get corresponding NvAPI PathInfo. If position is same, then the two paths are equal, after all position is whats important in path sources");
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
                        SharedLogger.logger.Trace($"SurroundTopology/FromPathTargetInfo: Get corresponding NvAPI PathTargetInfo. We now assume that there is only one target for a NvAPI PathInfo, in an other word, for now, it is not possible to have a cloned surround display");
                        var correspondingNvidiaTargetInfo = correspondingNvidiaPathInfo.TargetsInfo.FirstOrDefault();

                        if (correspondingNvidiaTargetInfo != null)
                        {
                            // Get corresponding NvAPI Grid Topology
                            // We also assume that the NVS monitor uses a similar display id to one of real physical monitors
                            SharedLogger.logger.Trace($"SurroundTopology/FromPathTargetInfo: Get corresponding NvAPI Grid Topology. We also assume that the NVS monitor uses a similar display id to one of real physical monitors");
                            var correspondingNvidiaTopology =
                                GridTopology.GetGridTopologies()
                                    .FirstOrDefault(
                                        topology => topology.Displays.Any(display =>
                                            display.DisplayDevice == correspondingNvidiaTargetInfo.DisplayDevice));

                            if (correspondingNvidiaTopology != null)
                            {
                                SharedLogger.logger.Trace($"SurroundTopology/FromPathTargetInfo: Return the new NVIDIA Topology SurroundTopology object");
                                return new SurroundTopology(correspondingNvidiaTopology);
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"SurroundTopology/FromPathTargetInfo: The correspondingNvidiaTopology SurroundTopology object wa null, so nothing to return");
                                return null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex, $"SurroundTopology/FromPathTargetInfo: Exception trying to get the Grid Topology from the NVIDIA driver.");
            }
            // if we get here, then we've failed to get the NVIDIA Grid Topology
            SharedLogger.logger.Warn($"SurroundTopology/FromPathTargetInfo: We've tried to get the Grid Topology from the NVIDIA driver but we can't!");
            return null;
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

        // The public override for the Object.Equals
        public override bool Equals(object obj)
        {
            return this.Equals(obj as SurroundTopology);
        }

        // SurroundTopoligies are equal if their contents (except name) are equal
        public bool Equals(SurroundTopology other)
        {

            // If parameter is null, return false.
            if (other is null)
                return false;

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, other))
                return true;

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != other.GetType())
                return false;

            // Check whether the Profile Viewport properties are equal
            // Two profiles are equal only when they have the same viewport data exactly
            if (AcceleratePrimaryDisplay.Equals(other.AcceleratePrimaryDisplay) &&
                ApplyWithBezelCorrectedResolution.Equals(other.ApplyWithBezelCorrectedResolution) &&
                BaseMosaicPanoramic.Equals(other.BaseMosaicPanoramic) &&
                ColorDepth.Equals(other.ColorDepth) &&
                Columns.Equals(other.Columns) &&
                Displays.Length.Equals(other.Displays.Length) &&
                DriverReloadAllowed.Equals(other.DriverReloadAllowed) &&
                Frequency.Equals(other.Frequency) &&
                ImmersiveGaming.Equals(other.ImmersiveGaming) &&
                Resolution.Equals(other.Resolution) &&
                Rows.Equals(other.Rows))
            {
                // If the above all match, then we need to check the Displays matche
                if (Displays == null && other.Displays == null)
                    return true;
                else if (Displays != null && other.Displays == null)
                    return false;
                else if (Displays == null && other.Displays != null)
                    return false;
                else if (Displays.SequenceEqual(other.Displays))
                    return true;

                return false;
            }
            else
                return false;
        }

        // If Equals() returns true for this object compared to  another
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            // Get hash code for the AcceleratePrimaryDisplay field if it is not null.
            int hashAcceleratePrimaryDisplay = AcceleratePrimaryDisplay.GetHashCode();

            // Get hash code for the ApplyWithBezelCorrectedResolution field if it is not null.
            int hashApplyWithBezelCorrectedResolution = ApplyWithBezelCorrectedResolution.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashBaseMosaicPanoramic = BaseMosaicPanoramic.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashColorDepth = ColorDepth.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashColumns = Columns.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashDriverReloadAllowed = DriverReloadAllowed.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashFrequency = Frequency.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashImmersiveGaming = ImmersiveGaming.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashResolution = Resolution.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashRows = Rows.GetHashCode();

            // Get hash code for the Displays field if it is not null.
            int hashDisplays = Displays.GetHashCode();

            //Calculate the hash code for the product.
            return hashAcceleratePrimaryDisplay ^ hashApplyWithBezelCorrectedResolution ^ hashBaseMosaicPanoramic ^
                hashColorDepth ^ hashColumns ^ hashDriverReloadAllowed ^ hashFrequency ^ hashImmersiveGaming ^
                hashResolution ^ hashRows ^ hashDisplays;
        }
    }

    // Custom comparer for the ProfileViewportTargetDisplay class
    class SurroundTopologyComparer : IEqualityComparer<SurroundTopology>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(SurroundTopology x, SurroundTopology y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (x is null || y is null)
                return false;

            // Check whether the Profile Viewport properties are equal
            // Two profiles are equal only when they have the same viewport data exactly

            if (x.AcceleratePrimaryDisplay.Equals(y.AcceleratePrimaryDisplay) &&
                x.ApplyWithBezelCorrectedResolution.Equals(y.ApplyWithBezelCorrectedResolution) &&
                x.BaseMosaicPanoramic.Equals(y.BaseMosaicPanoramic) &&
                x.ColorDepth.Equals(y.ColorDepth) &&
                x.Columns.Equals(y.Columns) &&
                x.Displays.Length.Equals(y.Displays.Length) &&
                x.DriverReloadAllowed.Equals(y.DriverReloadAllowed) &&
                x.Frequency.Equals(y.Frequency) &&
                x.ImmersiveGaming.Equals(y.ImmersiveGaming) &&
                x.Resolution.Equals(y.Resolution) &&
                x.Rows.Equals(y.Rows))
            {
                // If the above all match, then we need to check the Displays matche
                if (x.Displays == null && y.Displays == null)
                    return true;
                else if (x.Displays != null && y.Displays == null)
                    return false;
                else if (x.Displays == null && y.Displays != null)
                    return false;
                else if (x.Displays.SequenceEqual(y.Displays))
                    return true;

                return false;
            }
            else
                return false;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.
        public int GetHashCode(SurroundTopology surroundTopology)
        {
            // Check whether the object is null
            if (surroundTopology is null) return 0;

            // Get hash code for the AcceleratePrimaryDisplay field if it is not null.
            int hashAcceleratePrimaryDisplay = surroundTopology.AcceleratePrimaryDisplay.GetHashCode();

            // Get hash code for the ApplyWithBezelCorrectedResolution field if it is not null.
            int hashApplyWithBezelCorrectedResolution = surroundTopology.ApplyWithBezelCorrectedResolution.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashBaseMosaicPanoramic = surroundTopology.BaseMosaicPanoramic.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashColorDepth = surroundTopology.ColorDepth.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashColumns = surroundTopology.Columns.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashDriverReloadAllowed = surroundTopology.DriverReloadAllowed.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashFrequency = surroundTopology.Frequency.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashImmersiveGaming = surroundTopology.ImmersiveGaming.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashResolution = surroundTopology.Resolution.GetHashCode();

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashRows = surroundTopology.Rows.GetHashCode();

            // Get hash code for the Displays field if it is not null.
            int hashDisplays = surroundTopology.Displays.GetHashCode();

            //Calculate the hash code for the product.
            return hashAcceleratePrimaryDisplay ^ hashApplyWithBezelCorrectedResolution ^ hashBaseMosaicPanoramic ^
                hashColorDepth ^ hashColumns ^ hashDriverReloadAllowed ^ hashFrequency ^ hashImmersiveGaming ^
                hashResolution ^ hashRows ^ hashDisplays;
        }

    }
}