using System;
using System.Linq;
using WindowsDisplayAPI.DisplayConfig;
using WindowsDisplayAPI.Native.DisplayConfig;
using HeliosDisplayManagement.Shared.NVIDIA;

namespace HeliosDisplayManagement.Shared.Topology
{
    public class PathTarget : IEquatable<PathTarget>
    {
        public PathTarget(PathTargetInfo targetInfo, SurroundTopology surround = null)
        {
            DevicePath = targetInfo.DisplayTarget.DevicePath;
            var index = DevicePath.IndexOf("{", StringComparison.InvariantCultureIgnoreCase);
            if (index > 0)
                DevicePath = DevicePath.Substring(0, index).TrimEnd('#');

            FrequencyInMillihertz = targetInfo.FrequencyInMillihertz;
            Rotation = targetInfo.Rotation.ToRotation();
            Scaling = targetInfo.Scaling;
            ScanLineOrdering = targetInfo.ScanLineOrdering;
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
        public Rotation Rotation { get; set; }
        public DisplayConfigScaling Scaling { get; set; }
        public DisplayConfigScanLineOrdering ScanLineOrdering { get; set; }

        public SurroundTopology SurroundTopology { get; set; }

        /// <inheritdoc />
        public bool Equals(PathTarget other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return (FrequencyInMillihertz == other.FrequencyInMillihertz) &&
                   (Rotation == other.Rotation) && (Scaling == other.Scaling) &&
                   (ScanLineOrdering == other.ScanLineOrdering) && (DevicePath == other.DevicePath) &&
                   (SurroundTopology == other.SurroundTopology);
        }

        public static bool operator ==(PathTarget left, PathTarget right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PathTarget left, PathTarget right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PathTarget) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FrequencyInMillihertz.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Rotation;
                hashCode = (hashCode*397) ^ (int) Scaling;
                hashCode = (hashCode*397) ^ (int) ScanLineOrdering;
                hashCode = (hashCode*397) ^ DevicePath.GetHashCode();
                hashCode = (hashCode*397) ^ SurroundTopology?.GetHashCode() ?? 0;
                return hashCode;
            }
        }

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
                        target => target.DevicePath.StartsWith(DevicePath, StringComparison.InvariantCultureIgnoreCase));
            if (targetDevice == null)
                return null;
            return new PathTargetInfo(new PathDisplayTarget(targetDevice.Adapter, targetDevice.TargetId),
                FrequencyInMillihertz, ScanLineOrdering, Rotation.ToDisplayConfigRotation(), Scaling);
        }
    }
}