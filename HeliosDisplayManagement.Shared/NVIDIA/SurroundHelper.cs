using NvAPIWrapper.Native.Display;

namespace HeliosDisplayManagement.Shared.NVIDIA
{
    internal static class SurroundHelper
    {
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