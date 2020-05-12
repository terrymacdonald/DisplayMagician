using System;
using System.Drawing;
using System.Linq;
using WindowsDisplayAPI.DisplayConfig;
using WindowsDisplayAPI.Native.DisplayConfig;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HeliosPlus.Shared.Topology
{
    public class ProfileViewport : IEquatable<ProfileViewport>
    {
        public ProfileViewport(PathInfo pathInfo)
        {
            SourceId = pathInfo.DisplaySource.SourceId;
            PixelFormat = pathInfo.PixelFormat;
            Position = pathInfo.Position;
            Resolution = pathInfo.Resolution;
            TargetDisplays = pathInfo.TargetsInfo.Select(targetDisplay => new ProfileViewportTargetDisplay(targetDisplay)).ToArray();
        }

        public ProfileViewport()
        {
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public DisplayConfigPixelFormat PixelFormat { get; set; }

        public Point Position { get; set; }

        public Size Resolution { get; set; }

        public uint SourceId { get; set; }

        public ProfileViewportTargetDisplay[] TargetDisplays { get; set; }

        /// <inheritdoc />
        public bool Equals(ProfileViewport other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (PixelFormat == other.PixelFormat &&
                Position.Equals(other.Position) &&
                Resolution.Equals(other.Resolution) &&
                TargetDisplays.Length == other.TargetDisplays.Length) 
            {
                // TODO fix this so it checks for exact match

                //TargetDisplays.All(target => other.TargetDisplays.Contains(target));
                int thisToOtherTargetDisplayCount = 0;
                int otherToThisTargetDisplayCount = 0;

                foreach (ProfileViewportTargetDisplay myProfileViewportTargetDisplay in TargetDisplays)
                {
                    foreach (ProfileViewportTargetDisplay otherProfileViewportTargetDisplay in other.TargetDisplays)
                    {
                        if (myProfileViewportTargetDisplay.Equals(otherProfileViewportTargetDisplay))
                        {
                            thisToOtherTargetDisplayCount++;
                        }
                    }

                }

                foreach (ProfileViewportTargetDisplay otherProfileViewportTargetDisplay in other.TargetDisplays)
                {
                    foreach (ProfileViewportTargetDisplay myProfileViewportTargetDisplay in TargetDisplays)
                    {
                        if (otherProfileViewportTargetDisplay.Equals(myProfileViewportTargetDisplay))
                        {
                            otherToThisTargetDisplayCount++;
                        }
                    }

                }

                if (thisToOtherTargetDisplayCount == otherToThisTargetDisplayCount)
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
                return false;
            }
             
        }

        public static bool operator ==(ProfileViewport left, ProfileViewport right)
        {
            return Equals(left, right) || left?.Equals(right) == true;
        }

        public static bool operator !=(ProfileViewport left, ProfileViewport right)
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

            return Equals((ProfileViewport) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) PixelFormat;
                hashCode = (hashCode * 397) ^ Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Resolution.GetHashCode();
                hashCode = (hashCode * 397) ^ (TargetDisplays?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"\\\\.\\DISPLAY{SourceId}";
        }

        public PathInfo ToPathInfo()
        {
            var targetDisplays = TargetDisplays.Select(target => target.ToPathTargetInfo()).Where(info => info != null).ToArray();

            if (targetDisplays.Any())
            {
                return new PathInfo(new PathDisplaySource(targetDisplays.First().DisplayTarget.Adapter, SourceId), Position,
                    Resolution, PixelFormat, targetDisplays);
            }

            return null;
        }
    }
}