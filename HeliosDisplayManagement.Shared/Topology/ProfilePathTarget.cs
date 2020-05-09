using System;
using System.Linq;
using WindowsDisplayAPI.DisplayConfig;
using HeliosPlus.Shared.NVIDIA;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HeliosPlus.Shared.Topology
{
    public class ProfilePathTarget : IEquatable<ProfilePathTarget>
    {
        public ProfilePathTarget(PathTargetInfo targetInfo, SurroundTopology surround = null)
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


        public ProfilePathTarget()
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
        public bool Equals(ProfilePathTarget other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return FrequencyInMillihertz == other.FrequencyInMillihertz &&
                   Rotation == other.Rotation &&
                   Scaling == other.Scaling &&
                   ScanLineOrdering == other.ScanLineOrdering &&
                   DevicePath == other.DevicePath &&
                   SurroundTopology == other.SurroundTopology;
        }

        public static bool operator ==(ProfilePathTarget left, ProfilePathTarget right)
        {
            return Equals(left, right) || left?.Equals(right) == true;
        }

        public static bool operator !=(ProfilePathTarget left, ProfilePathTarget right)
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

            return Equals((ProfilePathTarget) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FrequencyInMillihertz.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Rotation;
                hashCode = (hashCode * 397) ^ (int) Scaling;
                hashCode = (hashCode * 397) ^ (int) ScanLineOrdering;
                hashCode = (hashCode * 397) ^ DevicePath.GetHashCode();
                hashCode = (hashCode * 397) ^ SurroundTopology?.GetHashCode() ?? 0;

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
    }
}