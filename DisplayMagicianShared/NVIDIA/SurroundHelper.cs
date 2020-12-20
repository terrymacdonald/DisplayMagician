using NvAPIWrapper.Native.Display;
using NvAPIWrapper.Native.Mosaic;

namespace DisplayMagician.Shared.NVIDIA
{
    internal static class SurroundHelper
    {
        public static PixelShift ToPixelShift(this PixelShiftType pixelShift)
        {
            switch (pixelShift)
            {
                case PixelShiftType.TopLeft2X2Pixels:

                    return PixelShift.TopLeft2X2Pixels;

                case PixelShiftType.BottomRight2X2Pixels:

                    return PixelShift.BottomRight2X2Pixels;

                default:

                    return PixelShift.NoPixelShift;
            }
        }

        public static PixelShiftType ToPixelShiftType(this PixelShift pixelShift)
        {
            switch (pixelShift)
            {
                case PixelShift.TopLeft2X2Pixels:

                    return PixelShiftType.TopLeft2X2Pixels;

                case PixelShift.BottomRight2X2Pixels:

                    return PixelShiftType.BottomRight2X2Pixels;

                default:

                    return PixelShiftType.NoPixelShift;
            }
        }

        public static Rotate ToRotate(this Rotation rotation)
        {
            switch (rotation)
            {
                case Rotation.Identity:

                    return Rotate.Degree0;

                case Rotation.Rotate90:

                    return Rotate.Degree90;

                case Rotation.Rotate180:

                    return Rotate.Degree180;

                case Rotation.Rotate270:

                    return Rotate.Degree270;

                default:

                    return Rotate.Ignored;
            }
        }

        public static Rotation ToRotation(this Rotate rotation)
        {
            switch (rotation)
            {
                case Rotate.Degree0:

                    return Rotation.Identity;

                case Rotate.Degree90:

                    return Rotation.Rotate90;

                case Rotate.Degree180:

                    return Rotation.Rotate180;

                case Rotate.Degree270:

                    return Rotation.Rotate270;

                default:

                    return Rotation.Unknown;
            }
        }
    }
}