using System;
using System.Linq;
using WindowsDisplayAPI.DisplayConfig;
using HeliosPlus.Shared.NVIDIA;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HeliosPlus.Shared.Topology
{
    public class ProfileViewportTargetDisplay : IEquatable<ProfileViewportTargetDisplay>
    {
        public ProfileViewportTargetDisplay(PathTargetInfo targetInfo, SurroundTopology surround = null)
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


        public ProfileViewportTargetDisplay()
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
        public bool Equals(ProfileViewportTargetDisplay other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (FrequencyInMillihertz.Equals(other.FrequencyInMillihertz) &&
                Rotation.Equals(other.Rotation) &&
                Scaling.Equals(other.Scaling) &&
                ScanLineOrdering.Equals(other.ScanLineOrdering) &&
                DevicePath.Equals(other.DevicePath))
            {
                if (SurroundTopology == null)
                {
                    if (other.SurroundTopology == null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (other.SurroundTopology == null)
                    {
                        return false;
                    }
                    else
                    {
                        return SurroundTopology.Equals(other.SurroundTopology);
                    }                   
                }
            }

            return false;
        }

        public static bool operator ==(ProfileViewportTargetDisplay left, ProfileViewportTargetDisplay right)
        {
            return Equals(left, right) || left?.Equals(right) == true;
        }

        public static bool operator !=(ProfileViewportTargetDisplay left, ProfileViewportTargetDisplay right)
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

            return Equals((ProfileViewportTargetDisplay) obj);
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