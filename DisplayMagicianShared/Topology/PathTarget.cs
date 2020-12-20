using System;
using System.Linq;
using WindowsDisplayAPI.DisplayConfig;
using DisplayMagician.Shared.NVIDIA;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace DisplayMagician.Shared.Topology
{
    public class PathTarget : IEquatable<PathTarget>
    {
        public PathTarget(PathTargetInfo targetInfo, SurroundTopology surround = null)
        {
            DevicePath = targetInfo.DisplayTarget.DevicePath;
            var index = DevicePath.IndexOf("{", StringComparison.InvariantCultureIgnoreCase);

            if (index > 0)
            {
                DevicePath = DevicePath.Substring(0, index).TrimEnd('#');
            }

            FrequencyInMillihertz = targetInfo.FrequencyInMillihertz;
            Rotation = targetInfo.Rotation.ToRotation();
            Scaling = targetInfo.Scaling.ToScaling();
            ScanLineOrdering = targetInfo.ScanLineOrdering.ToScanLineOrdering();

            try
            {
                DisplayName = targetInfo.DisplayTarget.FriendlyName;
            }
            catch
            {
                DisplayName = null;
            }

            SurroundTopology = surround ?? SurroundTopology.FromPathTargetInfo(targetInfo);
        }


        public PathTarget()
        {
        }

        public string DevicePath { get; set; }

        public string DisplayName { get; set; }

        public ulong FrequencyInMillihertz { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Rotation Rotation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Scaling Scaling { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ScanLineOrdering ScanLineOrdering { get; set; }

        public SurroundTopology SurroundTopology { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return DisplayName ?? $"PathTarget {DevicePath}";
        }


        public PathTargetInfo ToPathTargetInfo()
        {
            var targetDevice =
                PathDisplayTarget.GetDisplayTargets()
                    .FirstOrDefault(
                        target => target.DevicePath.StartsWith(DevicePath,
                            StringComparison.InvariantCultureIgnoreCase));

            if (targetDevice == null)
            {
                return null;
            }

            return new PathTargetInfo(new PathDisplayTarget(targetDevice.Adapter, targetDevice.TargetId),
                FrequencyInMillihertz, ScanLineOrdering.ToDisplayConfigScanLineOrdering(),
                Rotation.ToDisplayConfigRotation(), Scaling.ToDisplayConfigScaling());
        }

        // The public override for the Object.Equals
        public override bool Equals(object obj)
        {
            return this.Equals(obj as PathTarget);
        }

        // Profiles are equal if their contents (except name) are equal
        public bool Equals(PathTarget other)
        {

            // If parameter is null, return false.
            if (Object.ReferenceEquals(other, null))
                return false;

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, other))
                return true;

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != other.GetType())
                return false;

            // Check whether the Profile Viewport properties are equal
            // Two profiles are equal only when they have the same viewport data exactly
            if (FrequencyInMillihertz == other.FrequencyInMillihertz &&
                Rotation.Equals(other.Rotation) &&
                Scaling.Equals(other.Scaling) &&
                ScanLineOrdering.Equals(other.ScanLineOrdering) &&
                DisplayName.Equals(other.DisplayName) &&
                DevicePath.Equals(other.DevicePath))
            {
                // If the above all match, then we need to check the SurroundTopology matches
                if (SurroundTopology == null && other.SurroundTopology == null)
                    return true;
                else if (SurroundTopology != null && other.SurroundTopology == null)
                    return false;
                else if (SurroundTopology == null && other.SurroundTopology != null)
                    return false;
                else if (SurroundTopology.Equals(other.SurroundTopology))
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
            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashFrequencyInMillihertz = FrequencyInMillihertz.GetHashCode();

            // Get hash code for the Position field if it is not null.
            int hashRotation = Rotation.GetHashCode();

            // Get hash code for the Scaling field if it is not null.
            int hashScaling = Scaling.GetHashCode();

            // Get hash code for the ScanLineOrdering field if it is not null.
            int hashScanLineOrdering = ScanLineOrdering.GetHashCode();

            // Get hash code for the hashDisplayName field if it is not null.
            int hashDisplayName = DisplayName == null ? 0 : DisplayName.GetHashCode();

            // Get hash code for the DevicePath field if it is not null.
            int hashDevicePath = DevicePath == null ? 0 : DevicePath.GetHashCode();

            // Get hash code for the SurroundTopology field if it is not null.
            int hashSurroundTopology = SurroundTopology == null ? 0 : SurroundTopology.GetHashCode();

            //Calculate the hash code for the product.
            return hashFrequencyInMillihertz ^ hashRotation ^ hashScaling ^ hashScanLineOrdering ^
                hashDisplayName ^ hashDevicePath ^ hashSurroundTopology;
        }

    }

    // Custom comparer for the ProfileViewportTargetDisplay class
    class ProfileViewportTargetDisplayComparer : IEqualityComparer<PathTarget>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(PathTarget x, PathTarget y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            // Check whether the Profile Viewport properties are equal
            // Two profiles are equal only when they have the same viewport data exactly
            if (x.FrequencyInMillihertz == y.FrequencyInMillihertz &&
                x.Rotation.Equals(y.Rotation) &&
                x.Scaling.Equals(y.Scaling) &&
                x.ScanLineOrdering.Equals(y.ScanLineOrdering) &&
                x.DisplayName.Equals(y.DisplayName) &&
                x.DevicePath.Equals(y.DevicePath))
            {
                // If the above all match, then we need to check the SurroundTopology matches
                if (x.SurroundTopology == null && y.SurroundTopology == null)
                    return true;
                else if (x.SurroundTopology != null && y.SurroundTopology == null)
                    return false;
                else if (x.SurroundTopology == null && y.SurroundTopology != null)
                    return false;
                else if (x.SurroundTopology.Equals(y.SurroundTopology))
                    return true;

                return false;
            }
            else
                return false;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.
        public int GetHashCode(PathTarget profileViewport)
        {
            // Check whether the object is null
            if (Object.ReferenceEquals(profileViewport, null)) return 0;

            // Get hash code for the FrequencyInMillihertz field if it is not null.
            int hashFrequencyInMillihertz = profileViewport.FrequencyInMillihertz.GetHashCode();

            // Get hash code for the Position field if it is not null.
            int hashRotation = profileViewport.Rotation.GetHashCode();

            // Get hash code for the Scaling field if it is not null.
            int hashScaling = profileViewport.Scaling.GetHashCode();

            // Get hash code for the ScanLineOrdering field if it is not null.
            int hashScanLineOrdering = profileViewport.ScanLineOrdering.GetHashCode();

            // Get hash code for the hashDisplayName field if it is not null.
            int hashDisplayName = profileViewport.DisplayName == null ? 0 : profileViewport.DisplayName.GetHashCode();

            // Get hash code for the DevicePath field if it is not null.
            int hashDevicePath = profileViewport.DevicePath == null ? 0 : profileViewport.DevicePath.GetHashCode();

            // Get hash code for the SurroundTopology field if it is not null.
            int hashSurroundTopology = profileViewport.SurroundTopology == null ? 0 : profileViewport.SurroundTopology.GetHashCode();

            //Calculate the hash code for the product.
            return hashFrequencyInMillihertz ^ hashRotation ^ hashScaling ^ hashScanLineOrdering ^
                hashDisplayName ^ hashDevicePath ^ hashSurroundTopology;
        }

    }
}