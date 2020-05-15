using System;
using System.Drawing;
using System.Linq;
using WindowsDisplayAPI.DisplayConfig;
using WindowsDisplayAPI.Native.DisplayConfig;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace HeliosPlus.Shared.Topology
{
    public class ProfileViewport
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

        // The public override for the Object.Equals
        public override bool Equals(object obj)
        {
            return this.Equals(obj as ProfileViewport);
        }

        // Profiles are equal if their contents (except name) are equal
        public bool Equals(ProfileViewport other)
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
            if (PixelFormat == other.PixelFormat &&
                Position.Equals(other.Position) &&
                Resolution.Equals(other.Resolution) &&
                SourceId == other.SourceId)
            {
                // If the above all match, then we need to check the DisplayTargets
                foreach (ProfileViewportTargetDisplay targetDisplay in TargetDisplays)
                {
                    if (!other.TargetDisplays.Contains(targetDisplay))
                        return false;
                }
                return true;
            }
            else
                return false;
        }

        // If Equals() returns true for this object compared to  another
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            // Get hash code for the PixelFormat field if it is not null.
            int hashPixelFormat = PixelFormat.GetHashCode();

            // Get hash code for the Position field if it is not null.
            int hashPosition = Position == null ? 0 : Position.GetHashCode();

            // Get hash code for the Resolution field if it is not null.
            int hashResolution = Resolution == null ? 0 : Resolution.GetHashCode();

            // Get hash code for the SourceId field if it is not null.
            int hashSourceId = SourceId.GetHashCode();

            // Get hash code for the TargetDisplays field if it is not null.
            int hashTargetDisplays = TargetDisplays == null ? 0 : TargetDisplays.GetHashCode();

            //Calculate the hash code for the product.
            return hashPixelFormat ^ hashPosition ^ hashResolution ^ hashSourceId ^ hashTargetDisplays;
        }
    }

    // Custom comparer for the ProfileViewport class
    class ProfileViewportComparer : IEqualityComparer<ProfileViewport>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(ProfileViewport x, ProfileViewport y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            // Check whether the Profile Viewport properties are equal
            // Two profiles are equal only when they have the same viewport data exactly
            if (x.PixelFormat == y.PixelFormat &&
                x.Position.Equals(y.Position) &&
                x.Resolution.Equals(y.Resolution) &&
                x.SourceId == y.SourceId)
            {
                // If the above all match, then we need to check the DisplayTargets
                // If they aren't equal then we need to return false;
                if (!x.TargetDisplays.SequenceEqual(y.TargetDisplays))
                    return false;
                else
                    return true;
                /*                foreach (ProfileViewportTargetDisplay xTargetDisplay in x.TargetDisplays)
                                {
                                    if (!y.TargetDisplays.Contains(xTargetDisplay))
                                        return false;
                                }*/
                //return true;
            }
            else 
                return false;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.
        public int GetHashCode(ProfileViewport profileViewport)
        {
            // Check whether the object is null
            if (Object.ReferenceEquals(profileViewport, null)) return 0;

            // Get hash code for the PixelFormat field if it is not null.
            int hashPixelFormat = profileViewport.PixelFormat.GetHashCode();

            // Get hash code for the Position field if it is not null.
            int hashPosition = profileViewport.Position == null ? 0 : profileViewport.Position.GetHashCode();

            // Get hash code for the Resolution field if it is not null.
            int hashResolution = profileViewport.Resolution == null ? 0 : profileViewport.Resolution.GetHashCode();

            // Get hash code for the SourceId field if it is not null.
            int hashSourceId = profileViewport.SourceId.GetHashCode();

            // Get hash code for the TargetDisplays field if it is not null.
            int hashTargetDisplays = profileViewport.TargetDisplays == null ? 0 : profileViewport.TargetDisplays.GetHashCode();

            //Calculate the hash code for the product.
            return hashPixelFormat ^ hashPosition ^ hashResolution ^ hashSourceId ^ hashTargetDisplays;
        }

    }
}