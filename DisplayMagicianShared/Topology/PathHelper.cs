using WindowsDisplayAPI.Native.DisplayConfig;

namespace DisplayMagician.Shared.Topology
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


        public static DisplayConfigScaling ToDisplayConfigScaling(this Scaling scaling)
        {
            switch (scaling)
            {
                case Scaling.Identity:

                    return DisplayConfigScaling.Identity;
                case Scaling.Centered:

                    return DisplayConfigScaling.Centered;
                case Scaling.Stretched:

                    return DisplayConfigScaling.Stretched;
                case Scaling.AspectRatioCenteredMax:

                    return DisplayConfigScaling.AspectRatioCenteredMax;
                case Scaling.Custom:

                    return DisplayConfigScaling.Custom;
                case Scaling.Preferred:

                    return DisplayConfigScaling.Preferred;
                default:

                    return DisplayConfigScaling.NotSpecified;
            }
        }

        public static DisplayConfigScanLineOrdering ToDisplayConfigScanLineOrdering(
            this ScanLineOrdering scanLineOrdering)
        {
            switch (scanLineOrdering)
            {
                case ScanLineOrdering.Progressive:

                    return DisplayConfigScanLineOrdering.Progressive;
                case ScanLineOrdering.InterlacedWithUpperFieldFirst:

                    return DisplayConfigScanLineOrdering.InterlacedWithUpperFieldFirst;
                case ScanLineOrdering.InterlacedWithLowerFieldFirst:

                    return DisplayConfigScanLineOrdering.InterlacedWithLowerFieldFirst;
                default:

                    return DisplayConfigScanLineOrdering.NotSpecified;
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

        public static Scaling ToScaling(this DisplayConfigScaling scaling)
        {
            switch (scaling)
            {
                case DisplayConfigScaling.Identity:

                    return Scaling.Identity;
                case DisplayConfigScaling.Centered:

                    return Scaling.Centered;
                case DisplayConfigScaling.Stretched:

                    return Scaling.Stretched;
                case DisplayConfigScaling.AspectRatioCenteredMax:

                    return Scaling.AspectRatioCenteredMax;
                case DisplayConfigScaling.Custom:

                    return Scaling.Custom;
                case DisplayConfigScaling.Preferred:

                    return Scaling.Preferred;
                default:

                    return Scaling.NotSpecified;
            }
        }

        public static ScanLineOrdering ToScanLineOrdering(this DisplayConfigScanLineOrdering scanLineOrdering)
        {
            switch (scanLineOrdering)
            {
                case DisplayConfigScanLineOrdering.Progressive:

                    return ScanLineOrdering.Progressive;
                case DisplayConfigScanLineOrdering.InterlacedWithUpperFieldFirst:

                    return ScanLineOrdering.InterlacedWithUpperFieldFirst;
                case DisplayConfigScanLineOrdering.InterlacedWithLowerFieldFirst:

                    return ScanLineOrdering.InterlacedWithLowerFieldFirst;
                default:

                    return ScanLineOrdering.NotSpecified;
            }
        }
    }
}