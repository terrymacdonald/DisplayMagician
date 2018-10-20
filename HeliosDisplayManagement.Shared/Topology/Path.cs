using System;
using System.Drawing;
using System.Linq;
using WindowsDisplayAPI.DisplayConfig;
using WindowsDisplayAPI.Native.DisplayConfig;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HeliosDisplayManagement.Shared.Topology
{
    public class Path : IEquatable<Path>
    {
        public Path(PathInfo pathInfo)
        {
            SourceId = pathInfo.DisplaySource.SourceId;
            PixelFormat = pathInfo.PixelFormat;
            Position = pathInfo.Position;
            Resolution = pathInfo.Resolution;
            Targets = pathInfo.TargetsInfo.Select(target => new PathTarget(target)).ToArray();
        }

        public Path()
        {
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public DisplayConfigPixelFormat PixelFormat { get; set; }

        public Point Position { get; set; }

        public Size Resolution { get; set; }

        public uint SourceId { get; set; }

        public PathTarget[] Targets { get; set; }

        /// <inheritdoc />
        public bool Equals(Path other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return (PixelFormat == other.PixelFormat) &&
                   Position.Equals(other.Position) && Resolution.Equals(other.Resolution) &&
                   (Targets.Length == other.Targets.Length) && Targets.All(target => other.Targets.Contains(target));
        }

        public static bool operator ==(Path left, Path right)
        {
            return Equals(left, right) || left?.Equals(right) == true;
        }

        public static bool operator !=(Path left, Path right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Path) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) PixelFormat;
                hashCode = (hashCode*397) ^ Position.GetHashCode();
                hashCode = (hashCode*397) ^ Resolution.GetHashCode();
                hashCode = (hashCode*397) ^ (Targets?.GetHashCode() ?? 0);
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
            var targets = Targets.Select(target => target.ToPathTargetInfo()).Where(info => info != null).ToArray();
            if (targets.Any())
                return new PathInfo(new PathDisplaySource(targets.First().DisplayTarget.Adapter, SourceId), Position,
                    Resolution, PixelFormat, targets);
            return null;
        }
    }
}