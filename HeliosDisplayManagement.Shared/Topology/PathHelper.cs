using WindowsDisplayAPI.Native.DisplayConfig;

namespace HeliosDisplayManagement.Shared.Topology
{
    internal static class PathHelper
    {
        public static DisplayConfigRotation ToDisplayConfigRotation(this Rotation rotation)
        {
            switch (rotation)
            {
                case Rotation.Identity:
                    return DisplayConfigRotation.Identity;
                case Rotation.Rotate90:
                    return DisplayConfigRotation.Rotate90;
                case Rotation.Rotate180:
                    return DisplayConfigRotation.Rotate180;
                case Rotation.Rotate270:
                    return DisplayConfigRotation.Rotate270;
                default:
                    return DisplayConfigRotation.NotSpecified;
            }
        }

        public static Rotation ToRotation(this DisplayConfigRotation rotation)
        {
            switch (rotation)
            {
                case DisplayConfigRotation.Identity:
                    return Rotation.Identity;
                case DisplayConfigRotation.Rotate90:
                    return Rotation.Rotate90;
                case DisplayConfigRotation.Rotate180:
                    return Rotation.Rotate180;
                case DisplayConfigRotation.Rotate270:
                    return Rotation.Rotate270;
                default:
                    return Rotation.Unknown;
            }
        }
    }
}