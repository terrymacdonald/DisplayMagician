using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagicianShared.Windows
{

    public enum WIN32STATUS : uint
    {
        ERROR_SUCCESS = 0,
        ERROR_ACCESS_DENIED = 5,
        ERROR_NOT_SUPPORTED = 50,
        ERROR_GEN_FAILURE = 31,
        ERROR_INVALID_PARAMETER = 87,
        ERROR_INSUFFICIENT_BUFFER = 122,
        ERROR_BAD_CONFIGURATION = 1610,
    }

    public enum DISPLAYCONFIG_DEVICE_INFO_TYPE : uint
    {
        Zero = 0,
        DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1,
        DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2,
        DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE = 3,
        DISPLAYCONFIG_DEVICE_INFO_GET_ADAPTER_NAME = 4,
        DISPLAYCONFIG_DEVICE_INFO_SET_TARGET_PERSISTENCE = 5,
        DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_BASE_TYPE = 6,
        DISPLAYCONFIG_DEVICE_INFO_GET_SUPPORT_VIRTUAL_RESOLUTION = 7,
        DISPLAYCONFIG_DEVICE_INFO_SET_SUPPORT_VIRTUAL_RESOLUTION = 8,
        DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO = 9,
        DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE = 10,
        DISPLAYCONFIG_DEVICE_INFO_GET_SDR_WHITE_LEVEL = 11,
    }

    [Flags]
    public enum DISPLAYCONFIG_COLOR_ENCODING : uint
    {
        DISPLAYCONFIG_COLOR_ENCODING_RGB = 0,
        DISPLAYCONFIG_COLOR_ENCODING_YCBCR444 = 1,
        DISPLAYCONFIG_COLOR_ENCODING_YCBCR422 = 2,
        DISPLAYCONFIG_COLOR_ENCODING_YCBCR420 = 3,
        DISPLAYCONFIG_COLOR_ENCODING_INTENSITY = 4,
    }

    [Flags]
    public enum DISPLAYCONFIG_SCALING : uint
    {
        Zero = 0,
        DISPLAYCONFIG_SCALING_IDENTITY = 1,
        DISPLAYCONFIG_SCALING_CENTERED = 2,
        DISPLAYCONFIG_SCALING_STRETCHED = 3,
        DISPLAYCONFIG_SCALING_ASPECTRATIOCENTEREDMAX = 4,
        DISPLAYCONFIG_SCALING_CUSTOM = 5,
        DISPLAYCONFIG_SCALING_PREFERRED = 128,
        DISPLAYCONFIG_SCALING_FORCEUINT32 = 0xFFFFFFFF,
    }

    [Flags]
    public enum DISPLAYCONFIG_ROTATION : uint
    {
        Zero = 0,
        DISPLAYCONFIG_ROTATION_IDENTITY = 1,
        DISPLAYCONFIG_ROTATION_ROTATE90 = 2,
        DISPLAYCONFIG_ROTATION_ROTATE180 = 3,
        DISPLAYCONFIG_ROTATION_ROTATE270 = 4,
        DISPLAYCONFIG_ROTATION_FORCEUINT32 = 0xFFFFFFFF,
    }

    [Flags]
    public enum DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY : uint
    {
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_OTHER = 4294967295, // - 1
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HD15 = 0,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SVIDEO = 1,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPOSITE_VIDEO = 2,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPONENT_VIDEO = 3,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DVI = 4,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HDMI = 5,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_LVDS = 6,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_D_JPN = 8,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDI = 9,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EXTERNAL = 10,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED = 11,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EXTERNAL = 12,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EMBEDDED = 13,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDTVDONGLE = 14,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_MIRACAST = 15,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INDIRECT_WIRED = 16,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INDIRECT_VIRTUAL = 17,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_public = 0x80000000,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_FORCEUINT32 = 0xFFFFFFFF,
    }

    [Flags]
    public enum DISPLAYCONFIG_TOPOLOGY_ID : uint
    {
        Zero = 0x0,
        DISPLAYCONFIG_TOPOLOGY_public = 0x00000001,
        DISPLAYCONFIG_TOPOLOGY_CLONE = 0x00000002,
        DISPLAYCONFIG_TOPOLOGY_EXTEND = 0x00000004,
        DISPLAYCONFIG_TOPOLOGY_EXTERNAL = 0x00000008,
        DISPLAYCONFIG_TOPOLOGY_FORCEUINT32 = 0xFFFFFFFF,
    }

    [Flags]
    public enum DISPLAYCONFIG_PATH : uint
    {
        Zero = 0x0,
        DISPLAYCONFIG_PATH_ACTIVE = 0x00000001,
        DISPLAYCONFIG_PATH_PREFERRED_UNSCALED = 0x00000004,
        DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE = 0x00000008,
    }

    [Flags]
    public enum DISPLAYCONFIG_SOURCE_FLAGS : uint
    {
        Zero = 0x0,
        DISPLAYCONFIG_SOURCE_IN_USE = 0x00000001,
    }

    [Flags]
    public enum DISPLAYCONFIG_TARGET_FLAGS : uint
    {
        Zero = 0x0,
        DISPLAYCONFIG_TARGET_IN_USE = 0x00000001,
        DISPLAYCONFIG_TARGET_FORCIBLE = 0x00000002,
        DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_BOOT = 0x00000004,
        DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_PATH = 0x00000008,
        DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_SYSTEM = 0x00000010,
        DISPLAYCONFIG_TARGET_IS_HMD = 0x00000020,
    }

    [Flags]
    public enum QDC : uint
    {
        Zero = 0x0,
        QDC_ALL_PATHS = 0x00000001, // Get all paths
        QDC_ONLY_ACTIVE_PATHS = 0x00000002, // Get only the active paths currently in use
        QDC_DATABASE_CURRENT = 0x00000004, // Get the currently active paths as stored in the display database
        QDC_VIRTUAL_MODE_AWARE = 0x00000010, // Get the virtual mode aware paths
        QDC_INCLUDE_HMD = 0x00000020,
    }

    [Flags]
    public enum SDC : uint
    {
        Zero = 0x0,
        SDC_TOPOLOGY_public = 0x00000001,
        SDC_TOPOLOGY_CLONE = 0x00000002,
        SDC_TOPOLOGY_EXTEND = 0x00000004,
        SDC_TOPOLOGY_EXTERNAL = 0x00000008,
        SDC_TOPOLOGY_SUPPLIED = 0x00000010,
        SDC_USE_DATABASE_CURRENT = (SDC_TOPOLOGY_public | SDC_TOPOLOGY_CLONE | SDC_TOPOLOGY_EXTEND | SDC_TOPOLOGY_EXTERNAL),
        SDC_USE_SUPPLIED_DISPLAY_CONFIG = 0x00000020,
        SDC_VALIDATE = 0x00000040,
        SDC_APPLY = 0x00000080,
        SDC_NO_OPTIMIZATION = 0x00000100,
        SDC_SAVE_TO_DATABASE = 0x00000200,
        SDC_ALLOW_CHANGES = 0x00000400,
        SDC_PATH_PERSIST_IF_REQUIRED = 0x00000800,
        SDC_FORCE_MODE_ENUMERATION = 0x00001000,
        SDC_ALLOW_PATH_ORDER_CHANGES = 0x00002000,
        SDC_VIRTUAL_MODE_AWARE = 0x00008000,

        // Special common combinations (only set in this library)
        TEST_IF_VALID_DISPLAYCONFIG = (SDC_VALIDATE | SDC_USE_SUPPLIED_DISPLAY_CONFIG),
        TEST_IF_VALID_DISPLAYCONFIG_WITH_TWEAKS = (SDC_VALIDATE | SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_ALLOW_CHANGES),
        SET_DISPLAYCONFIG_AND_SAVE = (SDC_APPLY | SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_SAVE_TO_DATABASE),
        SET_DISPLAYCONFIG_WITH_TWEAKS_AND_SAVE = (SDC_APPLY | SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_ALLOW_CHANGES | SDC_SAVE_TO_DATABASE),
        DISPLAYMAGICIAN_SET = (SDC_APPLY | SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_ALLOW_CHANGES | SDC_SAVE_TO_DATABASE),
        DISPLAYMAGICIAN_VALIDATE = (SDC_VALIDATE | SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_ALLOW_CHANGES | SDC_SAVE_TO_DATABASE),
        //DISPLAYMAGICIAN_SET = (SDC_APPLY | SDC_TOPOLOGY_SUPPLIED | SDC_ALLOW_CHANGES | SDC_ALLOW_PATH_ORDER_CHANGES ),
        //DISPLAYMAGICIAN_VALIDATE = (SDC_VALIDATE | SDC_TOPOLOGY_SUPPLIED | SDC_ALLOW_CHANGES | SDC_ALLOW_PATH_ORDER_CHANGES ),

        SET_DISPLAYCONFIG_BUT_NOT_SAVE = (SDC_APPLY | SDC_USE_SUPPLIED_DISPLAY_CONFIG),
        TEST_IF_CLONE_VALID = (SDC_VALIDATE | SDC_TOPOLOGY_CLONE),
        SET_CLONE_TOPOLOGY = (SDC_APPLY | SDC_TOPOLOGY_CLONE),
        SET_CLONE_TOPOLOGY_WITH_PATH_PERSISTENCE = (SDC_APPLY | SDC_TOPOLOGY_CLONE | SDC_PATH_PERSIST_IF_REQUIRED),
        RESET_DISPLAYCONFIG_TO_LAST_SAVED = (SDC_APPLY | SDC_USE_DATABASE_CURRENT),
        SET_DISPLAYCONFIG_USING_PATHS_ONLY_AND_SAVE = (SDC_APPLY | SDC_TOPOLOGY_SUPPLIED | SDC_ALLOW_PATH_ORDER_CHANGES),
    }

    [Flags]
    public enum DISPLAYCONFIG_SCANLINE_ORDERING : uint
    {
        DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED = 0,
        DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE = 1,
        DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED = 2,
        DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_UPPERFIELDFIRST = DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED,
        DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST = 3,
        DISPLAYCONFIG_SCANLINE_ORDERING_FORCEUINT32 = 0xFFFFFFFF,
    }

    [Flags]
    public enum DISPLAYCONFIG_PIXELFORMAT : uint
    {
        Zero = 0x0,
        DISPLAYCONFIG_PIXELFORMAT_8BPP = 1,
        DISPLAYCONFIG_PIXELFORMAT_16BPP = 2,
        DISPLAYCONFIG_PIXELFORMAT_24BPP = 3,
        DISPLAYCONFIG_PIXELFORMAT_32BPP = 4,
        DISPLAYCONFIG_PIXELFORMAT_NONGDI = 5,
        DISPLAYCONFIG_PIXELFORMAT_FORCEUINT32 = 0xFFFFFFFF,
    }

    [Flags]
    public enum DISPLAYCONFIG_MODE_INFO_TYPE : uint
    {
        Zero = 0x0,
        DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE = 1,
        DISPLAYCONFIG_MODE_INFO_TYPE_TARGET = 2,
        DISPLAYCONFIG_MODE_INFO_TYPE_DESKTOP_IMAGE = 3,
        DISPLAYCONFIG_MODE_INFO_TYPE_FORCEUINT32 = 0xFFFFFFFF,
    }

    [Flags]
    public enum D3D_VIDEO_SIGNAL_STANDARD : uint
    {
        Uninitialized = 0,
        VesaDmt = 1,
        VesaGtf = 2,
        VesaCvt = 3,
        Ibm = 4,
        Apple = 5,
        NtscM = 6,
        NtscJ = 7,
        Ntsc443 = 8,
        PalB = 9,
        PalB1 = 10,
        PalG = 11,
        PalH = 12,
        PalI = 13,
        PalD = 14,
        PalN = 15,
        PalNc = 16,
        SecamB = 17,
        SecNVIDIA = 18,
        SecamG = 19,
        SecamH = 20,
        SecamK = 21,
        SecamK1 = 22,
        SecamL = 23,
        SecamL1 = 24,
        Eia861 = 25,
        Eia861A = 26,
        Eia861B = 27,
        PalK = 28,
        PalK1 = 29,
        PalL = 30,
        PalM = 31,
        Other = 255
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_DEVICE_INFO_HEADER : IEquatable<DISPLAYCONFIG_DEVICE_INFO_HEADER>
    {
        public DISPLAYCONFIG_DEVICE_INFO_TYPE Type;
        public uint Size;
        public LUID AdapterId;
        public uint Id;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_DEVICE_INFO_HEADER other && this.Equals(other);

        public bool Equals(DISPLAYCONFIG_DEVICE_INFO_HEADER other)
            => Type == other.Type &&
                Size == other.Size &&
                // AdapterId.Equals(other.AdapterId) && // Removed the AdapterId from the Equals, as it changes after reboot.
                Id == other.Id;

        public override int GetHashCode()
        {
            return (Type, Size, AdapterId, Id).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_DEVICE_INFO_HEADER lhs, DISPLAYCONFIG_DEVICE_INFO_HEADER rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_DEVICE_INFO_HEADER lhs, DISPLAYCONFIG_DEVICE_INFO_HEADER rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO : IEquatable<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
        //[MarshalAs(UnmanagedType.U4)]
        public uint Value;
        public DISPLAYCONFIG_COLOR_ENCODING ColorEncoding;
        //[MarshalAs(UnmanagedType.U4)]
        public uint BitsPerColorChannel;

        public bool AdvancedColorSupported => (Value & 0x1) == 0x1;
        public bool AdvancedColorEnabled => (Value & 0x2) == 0x2;
        public bool WideColorEnforced => (Value & 0x4) == 0x4;
        public bool AdvancedColorForceDisabled => (Value & 0x8) == 0x8;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO other && this.Equals(other);

        public bool Equals(DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO other)
            => Header.Equals(other.Header) &&
                Value == other.Value &&
                ColorEncoding.Equals(other.ColorEncoding) &&
                BitsPerColorChannel == other.BitsPerColorChannel;

        public override int GetHashCode()
        {
            return (Header, Value, ColorEncoding, BitsPerColorChannel).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO lhs, DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO lhs, DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTL : IEquatable<POINTL>
    {
        public int X;
        public int Y;

        public override bool Equals(object obj) => obj is POINTL other && this.Equals(other);
        public bool Equals(POINTL other)
            => X == other.X &&
               Y == other.Y;

        public override int GetHashCode()
        {
            return (X, Y).GetHashCode();
        }

        public static bool operator ==(POINTL lhs, POINTL rhs) => lhs.Equals(rhs);

        public static bool operator !=(POINTL lhs, POINTL rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID : IEquatable<LUID>
    {
        public uint LowPart;
        public uint HighPart;

        public ulong Value => ((ulong)HighPart << 32) | LowPart;

        public override bool Equals(object obj) => obj is LUID other && this.Equals(other);
        public bool Equals(LUID other)
            => LowPart == other.LowPart &&
                HighPart == other.HighPart;

        public override int GetHashCode()
        {
            return (LowPart, HighPart).GetHashCode();
        }

        public static bool operator ==(LUID lhs, LUID rhs) => lhs.Equals(rhs);

        public static bool operator !=(LUID lhs, LUID rhs) => !(lhs == rhs);

        public override string ToString() => Value.ToString();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_SOURCE_MODE : IEquatable<DISPLAYCONFIG_SOURCE_MODE>
    {
        public uint Width;
        public uint Height;
        public DISPLAYCONFIG_PIXELFORMAT PixelFormat;
        public POINTL Position;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_SOURCE_MODE other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_SOURCE_MODE other)
            => Width == other.Width &&
                Height == other.Height &&
                PixelFormat.Equals(other.PixelFormat) &&
                Position.Equals(other.Position);

        public override int GetHashCode()
        {
            return (Width, Height, PixelFormat, Position).GetHashCode();
        }
        public static bool operator ==(DISPLAYCONFIG_SOURCE_MODE lhs, DISPLAYCONFIG_SOURCE_MODE rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_SOURCE_MODE lhs, DISPLAYCONFIG_SOURCE_MODE rhs) => !(lhs == rhs);

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_RATIONAL : IEquatable<DISPLAYCONFIG_RATIONAL>
    {
        public uint Numerator;
        public uint Denominator;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_RATIONAL other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_RATIONAL other)
            => Numerator == other.Numerator &&
                Denominator == other.Denominator;

        public override int GetHashCode()
        {
            return (Numerator, Denominator).GetHashCode();
        }
        public static bool operator ==(DISPLAYCONFIG_RATIONAL lhs, DISPLAYCONFIG_RATIONAL rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_RATIONAL lhs, DISPLAYCONFIG_RATIONAL rhs) => !(lhs == rhs);

        public override string ToString() => Numerator + " / " + Denominator;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_2DREGION : IEquatable<DISPLAYCONFIG_2DREGION>
    {
        public uint Cx;
        public uint Cy;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_2DREGION other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_2DREGION other)
            => Cx == other.Cx &&
               Cy == other.Cy;

        public override int GetHashCode()
        {
            return (Cx, Cy).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_2DREGION lhs, DISPLAYCONFIG_2DREGION rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_2DREGION lhs, DISPLAYCONFIG_2DREGION rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_DESKTOP_IMAGE_INFO : IEquatable<DISPLAYCONFIG_DESKTOP_IMAGE_INFO>
    {
        public POINTL PathSourceSize;
        public RECTL DesktopImageRegion;
        public RECTL DesktopImageClip;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_DESKTOP_IMAGE_INFO other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_DESKTOP_IMAGE_INFO other)
            => PathSourceSize.Equals(other.PathSourceSize) &&
               DesktopImageRegion.Equals(other.DesktopImageRegion) &&
               DesktopImageClip.Equals(other.DesktopImageClip);

        public override int GetHashCode()
        {
            return (PathSourceSize, DesktopImageRegion, DesktopImageClip).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_DESKTOP_IMAGE_INFO lhs, DISPLAYCONFIG_DESKTOP_IMAGE_INFO rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_DESKTOP_IMAGE_INFO lhs, DISPLAYCONFIG_DESKTOP_IMAGE_INFO rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO : IEquatable<DISPLAYCONFIG_VIDEO_SIGNAL_INFO>
    {
        public ulong PixelRate;
        public DISPLAYCONFIG_RATIONAL HSyncFreq;
        public DISPLAYCONFIG_RATIONAL VSyncFreq;
        public DISPLAYCONFIG_2DREGION ActiveSize;
        public DISPLAYCONFIG_2DREGION TotalSize;
        public D3D_VIDEO_SIGNAL_STANDARD VideoStandard;
        public DISPLAYCONFIG_SCANLINE_ORDERING ScanLineOrdering;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_VIDEO_SIGNAL_INFO other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_VIDEO_SIGNAL_INFO other)
            => PixelRate == other.PixelRate &&
                HSyncFreq.Equals(other.HSyncFreq) &&
                VSyncFreq.Equals(other.VSyncFreq) &&
                ActiveSize.Equals(other.ActiveSize) &&
                TotalSize.Equals(other.TotalSize) &&
                VideoStandard == other.VideoStandard &&
                ScanLineOrdering.Equals(other.ScanLineOrdering);

        public override int GetHashCode()
        {
            return (PixelRate, HSyncFreq, VSyncFreq, ActiveSize, TotalSize, VideoStandard, ScanLineOrdering).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_VIDEO_SIGNAL_INFO lhs, DISPLAYCONFIG_VIDEO_SIGNAL_INFO rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_VIDEO_SIGNAL_INFO lhs, DISPLAYCONFIG_VIDEO_SIGNAL_INFO rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_TARGET_MODE : IEquatable<DISPLAYCONFIG_TARGET_MODE>
    {
        public DISPLAYCONFIG_VIDEO_SIGNAL_INFO TargetVideoSignalInfo;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_TARGET_MODE other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_TARGET_MODE other)
            => TargetVideoSignalInfo.Equals(other.TargetVideoSignalInfo);

        public override int GetHashCode()
        {
            return (TargetVideoSignalInfo).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_TARGET_MODE lhs, DISPLAYCONFIG_TARGET_MODE rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_TARGET_MODE lhs, DISPLAYCONFIG_TARGET_MODE rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DISPLAYCONFIG_PATH_SOURCE_INFO : IEquatable<DISPLAYCONFIG_PATH_SOURCE_INFO>
    {
        [FieldOffset(0)]
        public LUID AdapterId;
        [FieldOffset(8)]
        public uint Id;
        [FieldOffset(12)]
        public uint ModeInfoIdx;
        [FieldOffset(12)]
        public ushort cloneGroupId;
        [FieldOffset(14)]
        public ushort sourceModeInfoIdx;
        [FieldOffset(16)]
        public DISPLAYCONFIG_SOURCE_FLAGS StatusFlags;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_PATH_SOURCE_INFO other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_PATH_SOURCE_INFO other)
            => // AdapterId.Equals(other.AdapterId) && // Removed the AdapterId from the Equals, as it changes after a reboot.
               //Id == other.Id &&  // Removed the ID from the list as the Display ID it maps to will change after a switch from surround to non-surround profile
                ModeInfoIdx == other.ModeInfoIdx &&
                StatusFlags.Equals(other.StatusFlags);

        public override int GetHashCode()
        {
            //return (AdapterId, Id, ModeInfoIdx, StatusFlags).GetHashCode();
            return (ModeInfoIdx, Id, StatusFlags).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_PATH_SOURCE_INFO lhs, DISPLAYCONFIG_PATH_SOURCE_INFO rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_PATH_SOURCE_INFO lhs, DISPLAYCONFIG_PATH_SOURCE_INFO rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_PATH_TARGET_INFO : IEquatable<DISPLAYCONFIG_PATH_TARGET_INFO>
    {
        public LUID AdapterId;
        public uint Id;
        public uint ModeInfoIdx;
        public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY OutputTechnology;
        public DISPLAYCONFIG_ROTATION Rotation;
        public DISPLAYCONFIG_SCALING Scaling;
        public DISPLAYCONFIG_RATIONAL RefreshRate;
        public DISPLAYCONFIG_SCANLINE_ORDERING ScanLineOrdering;
        public bool TargetAvailable;
        public uint StatusFlags;

        public bool TargetInUse => (StatusFlags & 0x1) == 0x1;
        public bool TargetForcible => (StatusFlags & 0x2) == 0x2;
        public bool ForcedAvailabilityBoot => (StatusFlags & 0x4) == 0x4;
        public bool ForcedAvailabilityPath => (StatusFlags & 0x8) == 0x8;
        public bool ForcedAvailabilitySystem => (StatusFlags & 0x10) == 0x10;
        public bool IsHMD => (StatusFlags & 0x20) == 0x20;


        /* DISPLAYCONFIG_TARGET_IN_USE = 0x00000001,
        DISPLAYCONFIG_TARGET_FORCIBLE = 0x00000002,
        DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_BOOT = 0x00000004,
        DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_PATH = 0x00000008,
        DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_SYSTEM = 0x00000010,
        DISPLAYCONFIG_TARGET_IS_HMD = 0x00000020,*/
        public override bool Equals(object obj) => obj is DISPLAYCONFIG_PATH_TARGET_INFO other && this.Equals(other);

        public bool Equals(DISPLAYCONFIG_PATH_TARGET_INFO other)
            => // AdapterId.Equals(other.AdapterId) && // Removed the AdapterId from the Equals, as it changes after reboot.
                Id == other.Id &&
                ModeInfoIdx == other.ModeInfoIdx &&
                OutputTechnology.Equals(other.OutputTechnology) &&
                Rotation.Equals(other.Rotation) &&
                Scaling.Equals(other.Scaling) &&
                RefreshRate.Equals(other.RefreshRate) &&
                ScanLineOrdering.Equals(other.ScanLineOrdering) &&
                TargetAvailable == other.TargetAvailable &&
                StatusFlags.Equals(StatusFlags);

        public override int GetHashCode()
        {
            return (AdapterId, Id, ModeInfoIdx, OutputTechnology, Rotation, Scaling, RefreshRate, ScanLineOrdering, TargetAvailable, StatusFlags).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_PATH_TARGET_INFO lhs, DISPLAYCONFIG_PATH_TARGET_INFO rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_PATH_TARGET_INFO lhs, DISPLAYCONFIG_PATH_TARGET_INFO rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_PATH_INFO : IEquatable<DISPLAYCONFIG_PATH_INFO>
    {
        public DISPLAYCONFIG_PATH_SOURCE_INFO SourceInfo;
        public DISPLAYCONFIG_PATH_TARGET_INFO TargetInfo;
        public uint Flags;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_PATH_INFO other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_PATH_INFO other)
            => SourceInfo.Equals(other.SourceInfo) &&
               TargetInfo.Equals(other.TargetInfo) &&
               Flags.Equals(other.Flags);

        public override int GetHashCode()
        {
            return (SourceInfo, TargetInfo, Flags).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_PATH_INFO lhs, DISPLAYCONFIG_PATH_INFO rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_PATH_INFO lhs, DISPLAYCONFIG_PATH_INFO rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DISPLAYCONFIG_MODE_INFO : IEquatable<DISPLAYCONFIG_MODE_INFO>
    {
        [FieldOffset((0))]
        public DISPLAYCONFIG_MODE_INFO_TYPE InfoType;

        [FieldOffset(4)]
        public uint Id;

        [FieldOffset(8)]
        public LUID AdapterId;

        // These 3 fields are all a C union in wingdi.dll
        [FieldOffset(16)]
        public DISPLAYCONFIG_TARGET_MODE TargetMode;

        [FieldOffset(16)]
        public DISPLAYCONFIG_SOURCE_MODE SourceMode;

        [FieldOffset(16)]
        public DISPLAYCONFIG_DESKTOP_IMAGE_INFO DesktopImageInfo;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_MODE_INFO other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_MODE_INFO other)
        {
            if (InfoType != other.InfoType)
                return false;

            // This happens when it is a target mode info block
            if (InfoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET &&
                Id == other.Id &&
                TargetMode.Equals(other.TargetMode))
                return true;

            // This happens when it is a source mode info block
            if (InfoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE &&
                //Id == other.Id && // Disabling this check as as the Display ID it maps to will change after a switch from surround to non-surround profile, ruining the equality match
                // Only seems to be a problem with the DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE options weirdly enough!
                SourceMode.Equals(other.SourceMode))
                return true;

            // This happens when it is a desktop image mode info block
            if (InfoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_DESKTOP_IMAGE &&
                Id == other.Id &&
                DesktopImageInfo.Equals(other.DesktopImageInfo))
                return true;

            // This happens when it is a clone - there is an extra entry with all zeros in it!
            if (InfoType == DISPLAYCONFIG_MODE_INFO_TYPE.Zero &&
                Id == other.Id &&
                DesktopImageInfo.Equals(other.DesktopImageInfo) &&
                TargetMode.Equals(other.TargetMode) &&
                SourceMode.Equals(other.SourceMode))
                return true;

            return false;
        }


        public override int GetHashCode()
        {
            if (InfoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
                return (InfoType, Id, TargetMode).GetHashCode();

            if (InfoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE)
                return (InfoType, Id, SourceMode).GetHashCode();

            if (InfoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_DESKTOP_IMAGE)
                return (InfoType, Id, DesktopImageInfo).GetHashCode();

            // otherwise we return everything
            return (InfoType, Id, TargetMode, SourceMode, DesktopImageInfo).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_MODE_INFO lhs, DISPLAYCONFIG_MODE_INFO rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_MODE_INFO lhs, DISPLAYCONFIG_MODE_INFO rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAYCONFIG_SOURCE_DEVICE_NAME : IEquatable<DISPLAYCONFIG_SOURCE_DEVICE_NAME>
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string ViewGdiDeviceName;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_SOURCE_DEVICE_NAME other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_SOURCE_DEVICE_NAME other)
            => Header.Equals(other.Header) &&
               ViewGdiDeviceName == other.ViewGdiDeviceName;

        public override int GetHashCode()
        {
            return (Header, ViewGdiDeviceName).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_SOURCE_DEVICE_NAME lhs, DISPLAYCONFIG_SOURCE_DEVICE_NAME rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_SOURCE_DEVICE_NAME lhs, DISPLAYCONFIG_SOURCE_DEVICE_NAME rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS : IEquatable<DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS>
    {
        public uint Value;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS other)
            => Value == other.Value;

        public bool FriendlyNameFromEdid => (Value & 0x1) == 0x1; // Might be this broken?
        public bool FriendlyNameForced => (Value & 0x2) == 0x2;
        public bool EdidIdsValid => (Value & 0x4) == 0x4;

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS lhs, DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS lhs, DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAYCONFIG_TARGET_DEVICE_NAME : IEquatable<DISPLAYCONFIG_TARGET_DEVICE_NAME>
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
        public DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS Flags;
        public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY OutputTechnology;
        public ushort EdidManufactureId;
        public ushort EdidProductCodeId;
        public uint ConnectorInstance;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string MonitorFriendlyDeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string MonitorDevicePath;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_TARGET_DEVICE_NAME other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_TARGET_DEVICE_NAME other)
            => Header.Equals(other.Header) &&
               Flags.Equals(other.Flags) &&
               OutputTechnology.Equals(other.OutputTechnology) &&
               EdidManufactureId == other.EdidManufactureId &&
               EdidProductCodeId == other.EdidProductCodeId &&
               ConnectorInstance == other.ConnectorInstance &&
               MonitorFriendlyDeviceName == other.MonitorFriendlyDeviceName &&
               MonitorDevicePath == other.MonitorDevicePath;

        public override int GetHashCode()
        {
            return (Header, Flags, OutputTechnology, EdidManufactureId, EdidProductCodeId, ConnectorInstance, MonitorFriendlyDeviceName, MonitorDevicePath).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_TARGET_DEVICE_NAME lhs, DISPLAYCONFIG_TARGET_DEVICE_NAME rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_TARGET_DEVICE_NAME lhs, DISPLAYCONFIG_TARGET_DEVICE_NAME rhs) => !(lhs == rhs);
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_TARGET_PREFERRED_MODE : IEquatable<DISPLAYCONFIG_TARGET_PREFERRED_MODE>
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
        public uint Width;
        public uint Height;
        public DISPLAYCONFIG_TARGET_MODE TargetMode;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_TARGET_PREFERRED_MODE other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_TARGET_PREFERRED_MODE other)
            => Header.Equals(other.Header) &&
               Width == other.Width &&
               Height == other.Height &&
               TargetMode.Equals(other.TargetMode);

        public override int GetHashCode()
        {
            return (Header, Width, Height, TargetMode).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_TARGET_PREFERRED_MODE lhs, DISPLAYCONFIG_TARGET_PREFERRED_MODE rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_TARGET_PREFERRED_MODE lhs, DISPLAYCONFIG_TARGET_PREFERRED_MODE rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAYCONFIG_ADAPTER_NAME : IEquatable<DISPLAYCONFIG_ADAPTER_NAME>
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string AdapterDevicePath;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_ADAPTER_NAME other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_ADAPTER_NAME other)
            => Header.Equals(other.Header) &&
               AdapterDevicePath == other.AdapterDevicePath;

        public override int GetHashCode()
        {
            return (Header, AdapterDevicePath).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_ADAPTER_NAME lhs, DISPLAYCONFIG_ADAPTER_NAME rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_ADAPTER_NAME lhs, DISPLAYCONFIG_ADAPTER_NAME rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAYCONFIG_SUPPORT_VIRTUAL_RESOLUTION : IEquatable<DISPLAYCONFIG_SUPPORT_VIRTUAL_RESOLUTION>
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
        public uint Value;

        public bool IsMonitorVirtualResolutionDisabled
        {
            get => (Value & 0x1) == 0x1;
        }

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_SUPPORT_VIRTUAL_RESOLUTION other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_SUPPORT_VIRTUAL_RESOLUTION other)
            => Header.Equals(other.Header) &&
               Value == other.Value;

        public override int GetHashCode()
        {
            return (Header, Value).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_SUPPORT_VIRTUAL_RESOLUTION lhs, DISPLAYCONFIG_SUPPORT_VIRTUAL_RESOLUTION rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_SUPPORT_VIRTUAL_RESOLUTION lhs, DISPLAYCONFIG_SUPPORT_VIRTUAL_RESOLUTION rhs) => !(lhs == rhs);

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_SET_TARGET_PERSISTENCE : IEquatable<DISPLAYCONFIG_SET_TARGET_PERSISTENCE>
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
        public uint Value;

        public bool IsBootPersistenceOn
        {
            get => (Value & 0x1) == 0x1;
        }

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_SET_TARGET_PERSISTENCE other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_SET_TARGET_PERSISTENCE other)
            => Header.Equals(other.Header) &&
               Value == other.Value;

        public override int GetHashCode()
        {
            return (Header, Value).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_SET_TARGET_PERSISTENCE lhs, DISPLAYCONFIG_SET_TARGET_PERSISTENCE rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_SET_TARGET_PERSISTENCE lhs, DISPLAYCONFIG_SET_TARGET_PERSISTENCE rhs) => !(lhs == rhs);

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_TARGET_BASE_TYPE : IEquatable<DISPLAYCONFIG_TARGET_BASE_TYPE>
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
        //[MarshalAs(UnmanagedType.U4)]
        public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY BaseOutputTechnology;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_TARGET_BASE_TYPE other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_TARGET_BASE_TYPE other)
            => Header.Equals(other.Header) &&
               BaseOutputTechnology == other.BaseOutputTechnology;

        public override int GetHashCode()
        {
            return (Header, BaseOutputTechnology).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_TARGET_BASE_TYPE lhs, DISPLAYCONFIG_TARGET_BASE_TYPE rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_TARGET_BASE_TYPE lhs, DISPLAYCONFIG_TARGET_BASE_TYPE rhs) => !(lhs == rhs);
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE : IEquatable<DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE>
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
        public uint Value;

        public bool EnableAdvancedColor
        {
            get => (Value & 0x1) == 0x1;
            set
            {
                if (value)
                {
                    Value = 0x1;
                }
                else
                {
                    Value = 0x0;
                }
            }
        }

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE other)
            => Header.Equals(other.Header) &&
               Value == other.Value;

        public override int GetHashCode()
        {
            return (Header, Value).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE lhs, DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE lhs, DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_SDR_WHITE_LEVEL : IEquatable<DISPLAYCONFIG_SDR_WHITE_LEVEL>
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
        // SDRWhiteLevel represents a multiplier for standard SDR white
        // peak value i.e. 80 nits represented as fixed point.
        // To get value in nits use the following conversion
        // SDRWhiteLevel in nits = (SDRWhiteLevel / 1000 ) * 80
        // NOTE! Weirdly this is supposed to be a ulong, but there is an error in Win10 64-bit
        // where it actually returns a uint! So had to engineer in a bug :(
        public uint SDRWhiteLevel;

        public override bool Equals(object obj) => obj is DISPLAYCONFIG_SDR_WHITE_LEVEL other && this.Equals(other);
        public bool Equals(DISPLAYCONFIG_SDR_WHITE_LEVEL other)
            => Header.Equals(other.Header) &&
               SDRWhiteLevel == other.SDRWhiteLevel;


        public override int GetHashCode()
        {
            return (Header, SDRWhiteLevel).GetHashCode();
        }

        public static bool operator ==(DISPLAYCONFIG_SDR_WHITE_LEVEL lhs, DISPLAYCONFIG_SDR_WHITE_LEVEL rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAYCONFIG_SDR_WHITE_LEVEL lhs, DISPLAYCONFIG_SDR_WHITE_LEVEL rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECTL : IEquatable<RECTL>
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public override bool Equals(object obj) => obj is RECTL other && this.Equals(other);
        public bool Equals(RECTL other)
            => Left == other.Left &&
               Top == other.Top &&
               Right == other.Right &&
               Bottom == other.Bottom;

        public override int GetHashCode()
        {
            return (Left, Top, Right, Bottom).GetHashCode();
        }

        public static bool operator ==(RECTL lhs, RECTL rhs) => lhs.Equals(rhs);

        public static bool operator !=(RECTL lhs, RECTL rhs) => !(lhs == rhs);
    }


    class CCDImport
    {
        // Set some useful constants
        public const SDC SDC_CCD_TEST_IF_VALID = (SDC.SDC_VALIDATE | SDC.SDC_USE_SUPPLIED_DISPLAY_CONFIG);


        // GetDisplayConfigBufferSizes
        [DllImport("user32")]
        public static extern WIN32STATUS GetDisplayConfigBufferSizes(QDC flags, out int numPathArrayElements, out int numModeInfoArrayElements);

        // QueryDisplayConfig
        [DllImport("user32")]
        public static extern WIN32STATUS QueryDisplayConfig(QDC flags, ref int numPathArrayElements, [In, Out] DISPLAYCONFIG_PATH_INFO[] pathArray, ref int numModeInfoArrayElements, [In, Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray, out DISPLAYCONFIG_TOPOLOGY_ID currentTopologyId);

        [DllImport("user32")]
        public static extern WIN32STATUS QueryDisplayConfig(QDC flags, ref int numPathArrayElements, [In, Out] DISPLAYCONFIG_PATH_INFO[] pathArray, ref int numModeInfoArrayElements, [In, Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray, IntPtr currentTopologyId);

        // DisplayConfigGetDeviceInfo
        [DllImport("user32")]
        public static extern WIN32STATUS DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_SOURCE_DEVICE_NAME requestPacket);

        [DllImport("user32")]
        public static extern WIN32STATUS DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_TARGET_DEVICE_NAME requestPacket);

        [DllImport("user32")]
        public static extern WIN32STATUS DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_TARGET_PREFERRED_MODE requestPacket);

        [DllImport("user32")]
        public static extern WIN32STATUS DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_ADAPTER_NAME requestPacket);

        [DllImport("user32")]
        public static extern WIN32STATUS DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_SET_TARGET_PERSISTENCE requestPacket);

        [DllImport("user32")]
        public static extern WIN32STATUS DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_TARGET_BASE_TYPE requestPacket);

        [DllImport("user32")]
        public static extern WIN32STATUS DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_SUPPORT_VIRTUAL_RESOLUTION requestPacket);

        /*[DllImport("user32")]
        public static extern WIN32STATUS DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_SET_SUPPORT_VIRTUAL_RESOLUTION requestPacket);
*/
        [DllImport("user32")]
        public static extern WIN32STATUS DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO requestPacket);

        [DllImport("user32")]
        public static extern WIN32STATUS DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE requestPacket);

        [DllImport("user32")]
        public static extern WIN32STATUS DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_SDR_WHITE_LEVEL requestPacket);

        // DisplayConfigSetDeviceInfo
        [DllImport("user32")]
        public static extern WIN32STATUS DisplayConfigSetDeviceInfo(ref DISPLAYCONFIG_SET_TARGET_PERSISTENCE requestPacket);

        [DllImport("user32")]
        public static extern WIN32STATUS DisplayConfigSetDeviceInfo(ref DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE requestPacket);


        // Have disabled the DisplayConfigSetDeviceInfo options except for SET_TARGET_PERSISTENCE, as per the note
        // from https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-displayconfigsetdeviceinfo
        // "DisplayConfigSetDeviceInfo can currently only be used to start and stop boot persisted force projection on an analog target."
        /*[DllImport("user32")]
        public static extern int DisplayConfigSetDeviceInfo( ref DISPLAYCONFIG_SUPPORT_VIRTUAL_RESOLUTION targetSupportVirtualResolution );*/

        // SetDisplayConfig
        [DllImport("user32")]
        public static extern WIN32STATUS SetDisplayConfig([In] uint numPathArrayElements, [In] DISPLAYCONFIG_PATH_INFO[] pathArray, [In] uint numModeInfoArrayElements, [In] DISPLAYCONFIG_MODE_INFO[] modeInfoArray, [In] SDC flags);

    }
}