using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DisplayMagicianShared.Windows;
using EDIDParser;
using Newtonsoft.Json;
using Windows.Graphics;

namespace DisplayMagicianShared.NVIDIA
{
    /*    /// <summary>
        ///     This structure defines a group of topologies that work together to create one overall layout.  All of the supported
        ///     topologies are represented with this structure.
        ///     For example, a 'Passive Stereo' topology would be represented with this structure, and would have separate topology
        ///     details for the left and right eyes. The count would be 2. A 'Basic' topology is also represented by this
        ///     structure, with a count of 1.
        ///     The structure is primarily used internally, but is exposed to applications in a read-only fashion because there are
        ///     some details in it that might be useful (like the number of rows/cols, or connected display information).  A user
        ///     can get the filled-in structure by calling NvAPI_Mosaic_GetTopoGroup().
        ///     You can then look at the detailed values within the structure.  There are no entry points which take this structure
        ///     as input (effectively making it read-only).
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        [StructureVersion(1)]
        public struct TopologyGroup : IInitializable, IEquatable<TopologyGroup>
        {
            /// <summary>
            ///     Maximum number of topologies per each group
            /// </summary>
            public const int MaxTopologyPerGroup = 2;

            internal StructureVersion _Version;
            internal readonly TopologyBrief _Brief;
            internal readonly uint _TopologiesCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxTopologyPerGroup)]
            internal readonly TopologyDetails[]
                _TopologyDetails;

            /// <summary>
            ///     The brief details of this topology
            /// </summary>
            public TopologyBrief Brief
            {
                get => _Brief;
            }

            /// <summary>
            ///     Information about the topologies within this group
            /// </summary>
            public TopologyDetails[] TopologyDetails
            {
                get => _TopologyDetails.Take((int)_TopologiesCount).ToArray();
            }

            /// <inheritdoc />
            public bool Equals(TopologyGroup other)
            {
                return _Brief.Equals(other._Brief) &&
                       _TopologiesCount == other._TopologiesCount &&
                       _TopologyDetails.SequenceEqual(other._TopologyDetails);
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is TopologyGroup group && Equals(group);
            }

            /// <summary>
            ///     Checks for equality between two objects of same type
            /// </summary>
            /// <param name="left">The first object</param>
            /// <param name="right">The second object</param>
            /// <returns>true, if both objects are equal, otherwise false</returns>
            public static bool operator ==(TopologyGroup left, TopologyGroup right)
            {
                return left.Equals(right);
            }

            /// <summary>
            ///     Checks for inequality between two objects of same type
            /// </summary>
            /// <param name="left">The first object</param>
            /// <param name="right">The second object</param>
            /// <returns>true, if both objects are not equal, otherwise false</returns>
            public static bool operator !=(TopologyGroup left, TopologyGroup right)
            {
                return !left.Equals(right);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = _Brief.GetHashCode();
                    hashCode = (hashCode * 397) ^ (int)_TopologiesCount;
                    hashCode = (hashCode * 397) ^ (_TopologyDetails?.GetHashCode() ?? 0);

                    return hashCode;
                }
            }
        }*/

    /// <summary>
    ///     Contains possible values for color data color space
    /// </summary>
    public enum ColorDataColorimetry : uint
    {
        /// <summary>
        ///     RGB color space
        /// </summary>
        RGB = 0,

        /// <summary>
        ///     YCC601 color space
        /// </summary>
        YCC601,

        /// <summary>
        ///     YCC709 color space
        /// </summary>
        YCC709,

        /// <summary>
        ///     XVYCC601 color space
        /// </summary>
        XVYCC601,

        /// <summary>
        ///     XVYCC709 color space
        /// </summary>
        XVYCC709,

        /// <summary>
        ///     SYCC601 color space
        /// </summary>
        SYCC601,

        /// <summary>
        ///     ADOBEYCC601 color space
        /// </summary>
        ADOBEYCC601,

        /// <summary>
        ///     ADOBERGB color space
        /// </summary>
        ADOBERGB,

        /// <summary>
        ///     BT2020RGB color space
        /// </summary>
        BT2020RGB,

        /// <summary>
        ///     BT2020YCC color space
        /// </summary>
        BT2020YCC,

        /// <summary>
        ///     BT2020cYCC color space
        /// </summary>
        // ReSharper disable once InconsistentNaming
        BT2020cYCC,

        /// <summary>
        ///     Default color space
        /// </summary>
        Default = 0xFE,

        /// <summary>
        ///     Automatically select color space
        /// </summary>
        Auto = 0xFF
    }

    /// <summary>
    ///     Contains possible values for the color data command
    /// </summary>
    public enum ColorDataCommand : uint
    {
        /// <summary>
        ///     Get the current color data
        /// </summary>
        Get = 1,

        /// <summary>
        ///     Set the current color data
        /// </summary>
        Set,

        /// <summary>
        ///     Check if the passed color data is supported
        /// </summary>
        IsSupportedColor,

        /// <summary>
        ///     Get the default color data
        /// </summary>
        GetDefault
    }

    /// <summary>
    ///     Contains possible values for the color data depth
    /// </summary>
    public enum ColorDataDepth : uint
    {
        /// <summary>
        ///     Default color depth meaning that the current setting should be kept
        /// </summary>
        Default = 0,

        /// <summary>
        ///     6bit per color depth
        /// </summary>
        BPC6 = 1,

        /// <summary>
        ///     8bit per color depth
        /// </summary>
        BPC8 = 2,

        /// <summary>
        ///     10bit per color depth
        /// </summary>
        BPC10 = 3,

        /// <summary>
        ///     12bit per color depth
        /// </summary>
        BPC12 = 4,

        /// <summary>
        ///     16bit per color depth
        /// </summary>
        BPC16 = 5
    }

    /// <summary>
    ///     Contains possible values for the color data desktop color depth
    /// </summary>
    public enum ColorDataDesktopDepth : uint
    {
        /// <summary>
        ///     Default color depth meaning that the current setting should be kept
        /// </summary>
        Default = 0x0,

        /// <summary>
        ///     8bit per integer color component
        /// </summary>
        BPC8 = 0x1,

        /// <summary>
        ///     10bit integer per color component
        /// </summary>
        BPC10 = 0x2,

        /// <summary>
        ///     16bit float per color component
        /// </summary>
        BPC16Float = 0x3,

        /// <summary>
        ///     16bit float per color component wide color gamut
        /// </summary>
        BPC16FloatWcg = 0x4,

        /// <summary>
        ///     16bit float per color component HDR
        /// </summary>
        BPC16FloatHDR = 0x5
    }

    /// <summary>
    ///     Contains possible values for color data dynamic range
    /// </summary>
    public enum ColorDataDynamicRange : uint
    {
        /// <summary>
        ///     VESA standard progress signal
        /// </summary>
        VESA = 0,

        /// <summary>
        ///     CEA interlaced signal
        /// </summary>
        CEA,

        /// <summary>
        ///     Automatically select the best value
        /// </summary>
        Auto
    }

    /// <summary>
    ///     Contains possible color data color format values
    /// </summary>
    public enum ColorDataFormat : uint
    {
        /// <summary>
        ///     RGB color format
        /// </summary>
        RGB = 0,

        /// <summary>
        ///     YUV422 color format
        /// </summary>
        YUV422,

        /// <summary>
        ///     YUV444 color format
        /// </summary>
        YUV444,

        /// <summary>
        ///     YUV420 color format
        /// </summary>
        YUV420,

        /// <summary>
        ///     Default color format
        /// </summary>
        Default = 0xFE,

        /// <summary>
        ///     Automatically select the best color format
        /// </summary>
        Auto = 0xFF
    }

    /// <summary>
    ///     Contains possible values for the HDR color data command
    /// </summary>
    public enum ColorDataHDRCommand : uint
    {
        /// <summary>
        ///     Get the current HDR color data
        /// </summary>
        Get = 0,

        /// <summary>
        ///     Set the current HDR color data
        /// </summary>
        Set = 1
    }

    /// <summary>
    ///     Contains possible color data HDR modes
    /// </summary>
    public enum ColorDataHDRMode : uint
    {
        /// <summary>
        ///     Turn off HDR.
        /// </summary>
        Off = 0,

        /// <summary>
        ///     Source: CCCS [a.k.a FP16 scRGB, linear, sRGB primaries, [-65504,0, 65504] range, RGB(1,1,1) = 80nits]
        ///     Output: UHDA HDR [a.k.a HDR10, RGB/YCC 10/12bpc ST2084(PQ) EOTF RGB(1,1,1) = 10000 nits, Rec2020 color primaries,
        ///     ST2086 static HDR metadata].
        ///     This is the only supported production HDR mode.
        /// </summary>
        UHDA = 2,

        /// <summary>
        ///     Source: CCCS (a.k.a FP16 scRGB)
        ///     Output: EDR (Extended Dynamic Range) - HDR content is tone-mapped and gamut mapped to output on regular SDR display
        ///     set to max luminance ( ~300 nits ).
        /// </summary>
        [Obsolete("Do not use! Internal test mode only, to be removed.", false)]
        EDR = 3,

        /// <summary>
        ///     Source: any
        ///     Output: SDR (Standard Dynamic Range), we continuously send SDR EOTF InfoFrame signaling, HDMI compliance testing.
        /// </summary>
        [Obsolete("Do not use! Internal test mode only, to be removed.", false)]
        SDR = 4,

        /// <summary>
        ///     Source: HDR10 RGB 10bpc
        ///     Output: HDR10 RGB 10 colorDepth - signal UHDA HDR mode (PQ + Rec2020) to the sink but send source pixel values
        ///     unmodified (no PQ or Rec2020 conversions) - assumes source is already in HDR10 format.
        /// </summary>
        [Obsolete("Experimental mode only, not for production!", false)]
        UHDAPassthrough = 5,

        /// <summary>
        ///     Source: CCCS (a.k.a FP16 scRGB)
        ///     Output: notebook HDR
        /// </summary>
        [Obsolete("Do not use! Internal test mode only, to be removed.", false)]
        UHDANB = 6,

        /// <summary>
        ///     Source: RGB8 Dolby Vision encoded (12 colorDepth YCbCr422 packed into RGB8)
        ///     Output: Dolby Vision encoded : Application is to encoded frames in DV format and embed DV dynamic metadata as
        ///     described in Dolby Vision specification.
        /// </summary>
        [Obsolete("Experimental mode only, not for production!", false)]
        DolbyVision = 7
    }

    /// <summary>
    ///     Possible values for the color data selection policy
    /// </summary>
    public enum ColorDataSelectionPolicy : uint
    {
        /// <summary>
        ///     Application or the Nvidia Control Panel user configuration are used to decide the best color format
        /// </summary>
        User = 0,

        /// <summary>
        ///     Driver or the Operating System decides the best color format
        /// </summary>
        BestQuality = 1,

        /// <summary>
        ///     Default value, <see cref="BestQuality" />
        /// </summary>
        Default = BestQuality,

        /// <summary>
        ///     Unknown policy
        /// </summary>
        Unknown = 0xFF
    }

    /// <summary>
    ///     Possible color formats
    /// </summary>
    public enum ColorFormat
    {
        /// <summary>
        ///     Unknown, driver will choose one automatically.
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     8bpp mode
        /// </summary>
        P8 = 41,

        /// <summary>
        ///     16bpp mode
        /// </summary>
        R5G6B5 = 23,

        /// <summary>
        ///     32bpp mode
        /// </summary>
        A8R8G8B8 = 21,

        /// <summary>
        ///     64bpp (floating point)
        /// </summary>
        A16B16G16R16F = 113
    }

    /// <summary>
    ///     Flags for applying settings, used by NvAPI_DISP_SetDisplayConfig()
    /// </summary>
    [Flags]
    public enum DisplayConfigFlags
    {
        /// <summary>
        ///     None
        /// </summary>
        None = 0,

        /// <summary>
        ///     Do not apply
        /// </summary>
        ValidateOnly = 0x00000001,

        /// <summary>
        ///     Save to the persistence storage
        /// </summary>
        SaveToPersistence = 0x00000002,

        /// <summary>
        ///     Driver reload is permitted if necessary
        /// </summary>
        DriverReloadAllowed = 0x00000004,

        /// <summary>
        ///     Refresh OS mode list.
        /// </summary>
        ForceModeEnumeration = 0x00000008,

        /// <summary>
        ///     Tell OS to avoid optimizing CommitVidPn call during a modeset
        /// </summary>
        ForceCommitVideoPresentNetwork = 0x00000010
    }

    /// <summary>
    /// Possible display port color depths
    /// </summary>
    public enum DisplayPortColorDepth : uint
    {
        /// <summary>
        /// Default color depth
        /// </summary>
        Default = 0,
        /// <summary>
        /// 6 bit per color color depth
        /// </summary>
        BPC6,
        /// <summary>
        /// 8 bit per color color depth
        /// </summary>
        BPC8,
        /// <summary>
        /// 10 bit per color color depth
        /// </summary>
        BPC10,
        /// <summary>
        /// 12 bit per color color depth
        /// </summary>
        BPC12,

        /// <summary>
        /// 16 bit per color color depth
        /// </summary>
        BPC16,
    }

    /// <summary>
    ///     Possible display port color formats
    /// </summary>
    public enum DisplayPortColorFormat : uint
    {
        /// <summary>
        ///     RGB color format
        /// </summary>
        RGB = 0,

        /// <summary>
        ///     YCbCr422 color format
        /// </summary>
        YCbCr422 = 1,

        /// <summary>
        ///     YCbCr444 color format
        /// </summary>
        YCbCr444 = 2
    }

    /// <summary>
    ///     Contains possible audio channel allocations (speaker placements)
    /// </summary>
    public enum InfoFrameAudioChannelAllocation : uint
    {
        /// <summary>
        ///     [0] Empty [1] Empty [2] Empty [3] Empty [4] Empty [5] Empty [6] Front Right [7] Front Left
        /// </summary>
        FrFl = 0,

        /// <summary>
        ///     [0] Empty [1] Empty [2] Empty [3] Empty [4] Empty [5] Low Frequency Effects [6] Front Right [7] Front Left
        /// </summary>
        LfeFrFl,

        /// <summary>
        ///     [0] Empty [1] Empty [2] Empty [3] Empty [4] Front Center [5] Empty [6] Front Right [7] Front Left
        /// </summary>
        FcFrFl,

        /// <summary>
        ///     [0] Empty [1] Empty [2] Empty [3] Empty [4] Front Center [5] Low Frequency Effects [6] Front Right [7] Front Left
        /// </summary>
        FcLfeFrFl,

        /// <summary>
        ///     [0] Empty [1] Empty [2] Empty [3] Rear Center [4] Empty [5] Empty [6] Front Right [7] Front Left
        /// </summary>
        RcFrFl,

        /// <summary>
        ///     [0] Empty [1] Empty [2] Empty [3] Rear Center [4] Empty [5] Low Frequency Effects [6] Front Right [7] Front Left
        /// </summary>
        RcLfeFrFl,

        /// <summary>
        ///     [0] Empty [1] Empty [2] Empty [3] Rear Center [4] Front Center [5] Empty [6] Front Right [7] Front Left
        /// </summary>
        RcFcFrFl,

        /// <summary>
        ///     [0] Empty [1] Empty [2] Empty [3] Rear Center [4] Front Center [5] Low Frequency Effects [6] Front Right [7] Front
        ///     Left
        /// </summary>
        RcFcLfeFrFl,

        /// <summary>
        ///     [0] Empty [1] Empty [2] Rear Right [3] Rear Left [4] Empty [5] Empty [6] Front Right [7] Front Left
        /// </summary>
        RrRlFrFl,

        /// <summary>
        ///     [0] Empty [1] Empty [2] Rear Right [3] Rear Left [4] Empty [5] Low Frequency Effects [6] Front Right [7] Front Left
        /// </summary>
        RrRlLfeFrFl,

        /// <summary>
        ///     [0] Empty [1] Empty [2] Rear Right [3] Rear Left [4] Front Center [5] Empty [6] Front Right [7] Front Left
        /// </summary>
        RrRlFcFrFl,

        /// <summary>
        ///     [0] Empty [1] Empty [2] Rear Right [3] Rear Left [4] Front Center [5] Low Frequency Effects [6] Front Right [7]
        ///     Front Left
        /// </summary>
        RrRlFcLfeFrFl,

        /// <summary>
        ///     [0] Empty [1] Rear Center [2] Rear Right [3] Rear Left [4] Empty [5] Empty [6] Front Right [7] Front Left
        /// </summary>
        RcRrRlFrFl,

        /// <summary>
        ///     [0] Empty [1] Rear Center [2] Rear Right [3] Rear Left [4] Empty [5] Low Frequency Effects [6] Front Right [7]
        ///     Front Left
        /// </summary>
        RcRrRlLfeFrFl,

        /// <summary>
        ///     [0] Empty [1] Rear Center [2] Rear Right [3] Rear Left [4] Front Center [5] Empty [6] Front Right [7] Front Left
        /// </summary>
        RcRrRlFcFrFl,

        /// <summary>
        ///     [0] Empty [1] Rear Center [2] Rear Right [3] Rear Left [4] Front Center [5] Low Frequency Effects [6] Front Right
        ///     [7] Front Left
        /// </summary>
        RcRrRlFcLfeFrFl,

        /// <summary>
        ///     [0] Rear Right Of Center [1] Rear Left Of Center [2] Rear Right [3] Rear Left [4] Empty [5] Empty [6] Front Right
        ///     [7] Front Left
        /// </summary>
        RrcRlcRrRlFrFl,

        /// <summary>
        ///     [0] Rear Right Of Center [1] Rear Left Of Center [2] Rear Right [3] Rear Left [4] Empty [5] Low Frequency Effects
        ///     [6] Front Right [7] Front Left
        /// </summary>
        RrcRlcRrRlLfeFrFl,

        /// <summary>
        ///     [0] Rear Right Of Center [1] Rear Left Of Center [2] Rear Right [3] Rear Left [4] Front Center [5] Empty [6] Front
        ///     Right [7] Front Left
        /// </summary>
        RrcRlcRrRlFcFrFl,

        /// <summary>
        ///     [0] Rear Right Of Center [1] Rear Left Of Center [2] Rear Right [3] Rear Left [4] Front Center [5] Low Frequency
        ///     Effects [6] Front Right [7] Front Left
        /// </summary>
        RrcRlcRrRlFcLfeFrFl,

        /// <summary>
        ///     [0] Front Right Of Center [1] Front Left Of Center [2] Empty [3] Empty [4] Empty [5] Empty [6] Front Right [7]
        ///     Front Left
        /// </summary>
        FrcFlcFrFl,

        /// <summary>
        ///     [0] Front Right Of Center [1] Front Left Of Center [2] Empty [3] Empty [4] Empty [5] Low Frequency Effects [6]
        ///     Front Right [7] Front Left
        /// </summary>
        FrcFlcLfeFrFl,

        /// <summary>
        ///     [0] Front Right Of Center [1] Front Left Of Center [2] Empty [3] Empty [4] Front Center [5] Empty [6] Front Right
        ///     [7] Front Left
        /// </summary>
        FrcFlcFcFrFl,

        /// <summary>
        ///     [0] Front Right Of Center [1] Front Left Of Center [2] Empty [3] Empty [4] Front Center [5] Low Frequency Effects
        ///     [6] Front Right [7] Front Left
        /// </summary>
        FrcFlcFcLfeFrFl,

        /// <summary>
        ///     [0] Front Right Of Center [1] Front Left Of Center [2] Empty [3] Rear Center [4] Empty [5] Empty [6] Front Right
        ///     [7] Front Left
        /// </summary>
        FrcFlcRcFrFl,

        /// <summary>
        ///     [0] Front Right Of Center [1] Front Left Of Center [2] Empty [3] Rear Center [4] Empty [5] Low Frequency Effects
        ///     [6] Front Right [7] Front Left
        /// </summary>
        FrcFlcRcLfeFrFl,

        /// <summary>
        ///     [0] Front Right Of Center [1] Front Left Of Center [2] Empty [3] Rear Center [4] Front Center [5] Empty [6] Front
        ///     Right [7] Front Left
        /// </summary>
        FrcFlcRcFcFrFl,

        /// <summary>
        ///     [0] Front Right Of Center [1] Front Left Of Center [2] Empty [3] Rear Center [4] Front Center [5] Low Frequency
        ///     Effects [6] Front Right [7] Front Left
        /// </summary>
        FrcFlcRcFcLfeFrFl,

        /// <summary>
        ///     [0] Front Right Of Center [1] Front Left Of Center [2] Rear Right [3] Rear Left [4] Empty [5] Empty [6] Front Right
        ///     [7] Front Left
        /// </summary>
        FrcFlcRrRlFrFl,

        /// <summary>
        ///     [0] Front Right Of Center [1] Front Left Of Center [2] Rear Right [3] Rear Left [4] Empty [5] Low Frequency Effects
        ///     [6] Front Right [7] Front Left
        /// </summary>
        FrcFlcRrRlLfeFrFl,

        /// <summary>
        ///     [0] Front Right Of Center [1] Front Left Of Center [2] Rear Right [3] Rear Left [4] Front Center [5] Empty [6]
        ///     Front Right [7] Front Left
        /// </summary>
        FrcFlcRrRlFcFrFl,

        /// <summary>
        ///     [0] Front Right Of Center [1] Front Left Of Center [2] Rear Right [3] Rear Left [4] Front Center [5] Low Frequency
        ///     Effects [6] Front Right [7] Front Left
        /// </summary>
        FrcFlcRrRlFcLfeFrFl,

        /// <summary>
        ///     [0] Empty [1] Front Center High [2] Rear Right [3] Rear Left [4] Front Center [5] Empty [6] Front Right [7] Front
        ///     Left
        /// </summary>
        FchRrRlFcFrFl,

        /// <summary>
        ///     [0] Empty [1] Front Center High [2] Rear Right [3] Rear Left [4] Front Center [5] Low Frequency Effects [6] Front
        ///     Right [7] Front Left
        /// </summary>
        FchRrRlFcLfeFrFl,

        /// <summary>
        ///     [0] TopCenter [1] Empty [2] Rear Right [3] Rear Left [4] Front Center [5] Empty [6] Front Right [7] Front Left
        /// </summary>
        TcRrRlFcFrFl,

        /// <summary>
        ///     [0] TopCenter [1] Empty [2] Rear Right [3] Rear Left [4] Front Center [5] Low Frequency Effects [6] Front Right [7]
        ///     Front Left
        /// </summary>
        TcRrRlFcLfeFrFl,

        /// <summary>
        ///     [0] Front Right High [1] Front Left High [2] Rear Right [3] Rear Left [4] Empty [5] Empty [6] Front Right [7] Front
        ///     Left
        /// </summary>
        FrhFlhRrRlFrFl,

        /// <summary>
        ///     [0] Front Right High [1] Front Left High [2] Rear Right [3] Rear Left [4] Empty [5] Low Frequency Effects [6] Front
        ///     Right [7] Front Left
        /// </summary>
        FrhFlhRrRlLfeFrFl,

        /// <summary>
        ///     [0] Front Right Wide [1] Front Left Wide [2] Rear Right [3] Rear Left [4] Empty [5] Empty [6] Front Right [7] Front
        ///     Left
        /// </summary>
        FrwFlwRrRlFrFl,

        /// <summary>
        ///     [0] Front Right Wide [1] Front Left Wide [2] Rear Right [3] Rear Left [4] Empty [5] Low Frequency Effects [6] Front
        ///     Right [7] Front Left
        /// </summary>
        FrwFlwRrRlLfeFrFl,

        /// <summary>
        ///     [0] TopCenter [1] Rear Center [2] Rear Right [3] Rear Left [4] Front Center [5] Empty [6] Front Right [7] Front
        ///     Left
        /// </summary>
        TcRcRrRlFcFrFl,

        /// <summary>
        ///     [0] TopCenter [1] Rear Center [2] Rear Right [3] Rear Left [4] Front Center [5] Low Frequency Effects [6] Front
        ///     Right [7] Front Left
        /// </summary>
        TcRcRrRlFcLfeFrFl,

        /// <summary>
        ///     [0] Front Center High [1] Rear Center [2] Rear Right [3] Rear Left [4] Front Center [5] Empty [6] Front Right [7]
        ///     Front Left
        /// </summary>
        FchRcRrRlFcFrFl,

        /// <summary>
        ///     [0] Front Center High [1] Rear Center [2] Rear Right [3] Rear Left [4] Front Center [5] Low Frequency Effects [6]
        ///     Front Right [7] Front Left
        /// </summary>
        FchRcRrRlFcLfeFrFl,

        /// <summary>
        ///     [0] TopCenter [1] Front Center High [2] Rear Right [3] Rear Left [4] Front Center [5] Empty [6] Front Right [7]
        ///     Front Left
        /// </summary>
        TcFcRrRlFcFrFl,

        /// <summary>
        ///     [0] TopCenter [1] Front Center High [2] Rear Right [3] Rear Left [4] Front Center [5] Low Frequency Effects [6]
        ///     Front Right [7] Front Left
        /// </summary>
        TcFchRrRlFcLfeFrFl,

        /// <summary>
        ///     [0] Front Right High [1] Front Left High [2] Rear Right [3] Rear Left [4] Front Center [5] Empty [6] Front Right
        ///     [7] Front Left
        /// </summary>
        FrhFlhRrRlFcFrFl,

        /// <summary>
        ///     [0] Front Right High [1] Front Left High [2] Rear Right [3] Rear Left [4] Front Center [5] Low Frequency Effects
        ///     [6] Front Right [7] Front Left
        /// </summary>
        FrhFlhRrRlFcLfeFrFl,

        /// <summary>
        ///     [0] Front Right Wide [1] Front Left Wide [2] Rear Right [3] Rear Left [4] Front Center [5] Empty [6] Front Right
        ///     [7] Front Left
        /// </summary>
        FrwFlwRrRlFcFeFl,

        /// <summary>
        ///     [0] Front Right Wide [1] Front Left Wide [2] Rear Right [3] Rear Left [4] Front Center [5] Low Frequency Effects
        ///     [6] Front Right [7] Front Left
        /// </summary>
        FrwFlwRrRlFcLfeFrFl,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 511
    }

    /// <summary>
    ///     Contains possible audio channels
    /// </summary>
    public enum InfoFrameAudioChannelCount : uint
    {
        /// <summary>
        ///     Data is available in the header of source data
        /// </summary>
        InHeader = 0,

        /// <summary>
        ///     Two channels
        /// </summary>
        Two,

        /// <summary>
        ///     Three channels
        /// </summary>
        Three,

        /// <summary>
        ///     Four channels
        /// </summary>
        Four,

        /// <summary>
        ///     Five channels
        /// </summary>
        Five,

        /// <summary>
        ///     Six channels
        /// </summary>
        Six,

        /// <summary>
        ///     Seven channels
        /// </summary>
        Seven,

        /// <summary>
        ///     Eight channels
        /// </summary>
        Eight,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 15
    }

    /// <summary>
    ///     Contains possible audio codecs
    /// </summary>
    public enum InfoFrameAudioCodec : uint
    {
        /// <summary>
        ///     Data is available in the header of source data
        /// </summary>
        InHeader = 0,

        /// <summary>
        ///     Pulse-code modulation
        /// </summary>
        PCM,

        /// <summary>
        ///     Dolby AC-3
        /// </summary>
        AC3,

        /// <summary>
        ///     MPEG1
        /// </summary>
        MPEG1,

        /// <summary>
        ///     MP3 (MPEG-2 Audio Layer III)
        /// </summary>
        MP3,

        /// <summary>
        ///     MPEG2
        /// </summary>
        MPEG2,

        /// <summary>
        ///     Advanced Audio Coding
        /// </summary>
        AACLC,

        /// <summary>
        ///     DTS
        /// </summary>
        DTS,

        /// <summary>
        ///     Adaptive Transform Acoustic Coding
        /// </summary>
        ATRAC,

        /// <summary>
        ///     Direct Stream Digital
        /// </summary>
        DSD,

        /// <summary>
        ///     Dolby Digital Plus
        /// </summary>
        EAC3,

        /// <summary>
        ///     DTS High Definition
        /// </summary>
        DTSHD,

        /// <summary>
        ///     Meridian Lossless Packing
        /// </summary>
        MLP,

        /// <summary>
        ///     DST
        /// </summary>
        DST,

        /// <summary>
        ///     Windows Media Audio Pro
        /// </summary>
        WMAPRO,

        /// <summary>
        ///     Extended audio codec value should be used to get information regarding audio codec
        /// </summary>
        UseExtendedCodecType,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 31
    }

    /// <summary>
    ///     Contains possible extended audio codecs
    /// </summary>
    public enum InfoFrameAudioExtendedCodec : uint
    {
        /// <summary>
        ///     Use the primary audio codec type, data not available
        /// </summary>
        UseCodecType = 0,

        /// <summary>
        ///     High-Efficiency Advanced Audio Coding
        /// </summary>
        HEAAC,

        /// <summary>
        ///     High-Efficiency Advanced Audio Coding 2
        /// </summary>
        HEAACVersion2,

        /// <summary>
        ///     MPEG Surround
        /// </summary>
        MPEGSurround,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 63
    }

    /// <summary>
    ///     Contains possible audio low frequency effects channel playback level
    /// </summary>
    public enum InfoFrameAudioLFEPlaybackLevel : uint
    {
        /// <summary>
        ///     Data not available
        /// </summary>
        NoData = 0,

        /// <summary>
        ///     No change to the source audio
        /// </summary>
        Plus0Decibel,

        /// <summary>
        ///     Adds 10 decibel
        /// </summary>
        Plus10Decibel,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 7
    }

    /// <summary>
    ///     Contains possible audio channel level shift values
    /// </summary>
    public enum InfoFrameAudioLevelShift : uint
    {
        /// <summary>
        ///     No change to the source audio
        /// </summary>
        Shift0Decibel = 0,

        /// <summary>
        ///     Shifts 1 decibel
        /// </summary>
        Shift1Decibel,

        /// <summary>
        ///     Shifts 2 decibel
        /// </summary>
        Shift2Decibel,

        /// <summary>
        ///     Shifts 3 decibel
        /// </summary>
        Shift3Decibel,

        /// <summary>
        ///     Shifts 4 decibel
        /// </summary>
        Shift4Decibel,

        /// <summary>
        ///     Shifts 5 decibel
        /// </summary>
        Shift5Decibel,

        /// <summary>
        ///     Shifts 6 decibel
        /// </summary>
        Shift6Decibel,

        /// <summary>
        ///     Shifts 7 decibel
        /// </summary>
        Shift7Decibel,

        /// <summary>
        ///     Shifts 8 decibel
        /// </summary>
        Shift8Decibel,

        /// <summary>
        ///     Shifts 9 decibel
        /// </summary>
        Shift9Decibel,

        /// <summary>
        ///     Shifts 10 decibel
        /// </summary>
        Shift10Decibel,

        /// <summary>
        ///     Shifts 11 decibel
        /// </summary>
        Shift11Decibel,

        /// <summary>
        ///     Shifts 12 decibel
        /// </summary>
        Shift12Decibel,

        /// <summary>
        ///     Shifts 13 decibel
        /// </summary>
        Shift13Decibel,

        /// <summary>
        ///     Shifts 14 decibel
        /// </summary>
        Shift14Decibel,

        /// <summary>
        ///     Shifts 15 decibel
        /// </summary>
        Shift15Decibel,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 31
    }

    /// <summary>
    ///     Contains possible audio sample rates (sampling frequency)
    /// </summary>
    public enum InfoFrameAudioSampleRate : uint
    {
        /// <summary>
        ///     Data is available in the header of source data
        /// </summary>
        InHeader = 0,

        /// <summary>
        ///     31kHz sampling frequency
        /// </summary>
        F32000Hz,

        /// <summary>
        ///     44.1kHz sampling frequency
        /// </summary>
        F44100Hz,

        /// <summary>
        ///     48kHz sampling frequency
        /// </summary>
        F48000Hz,

        /// <summary>
        ///     88.2kHz sampling frequency
        /// </summary>
        F88200Hz,

        /// <summary>
        ///     96kHz sampling frequency
        /// </summary>
        F96000Hz,

        /// <summary>
        ///     176.4kHz sampling frequency
        /// </summary>
        F176400Hz,

        /// <summary>
        ///     192kHz sampling frequency
        /// </summary>
        F192000Hz,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 15
    }

    /// <summary>
    ///     Contains possible audio sample size (bit depth)
    /// </summary>
    public enum InfoFrameAudioSampleSize : uint
    {
        /// <summary>
        ///     Data is available in the header of source data
        /// </summary>
        InHeader = 0,

        /// <summary>
        ///     16bit audio sample size
        /// </summary>
        B16,

        /// <summary>
        ///     20bit audio sample size
        /// </summary>
        B20,

        /// <summary>
        ///     24bit audio sample size
        /// </summary>
        B24,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 7
    }

    /// <summary>
    ///     Contains possible values for info-frame properties that accept or return a boolean value
    /// </summary>
    public enum InfoFrameBoolean : uint
    {
        /// <summary>
        ///     False
        /// </summary>
        False = 0,

        /// <summary>
        ///     True
        /// </summary>
        True,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 3
    }

    /// <summary>
    ///     Possible commands for info-frame operations
    /// </summary>
    public enum InfoFrameCommand : uint
    {
        /// <summary>
        ///     Returns the fields in the info-frame with values set by the manufacturer (NVIDIA or OEM)
        /// </summary>
        GetDefault = 0,

        /// <summary>
        ///     Sets the fields in the info-frame to auto, and info-frame to the default info-frame for use in a set.
        /// </summary>
        Reset,

        /// <summary>
        ///     Get the current info-frame state.
        /// </summary>
        Get,

        /// <summary>
        ///     Set the current info-frame state (flushed to the monitor), the values are one time and do not persist.
        /// </summary>
        Set,

        /// <summary>
        ///     Get the override info-frame state, non-override fields will be set to value = AUTO, overridden fields will have the
        ///     current override values.
        /// </summary>
        GetOverride,

        /// <summary>
        ///     Set the override info-frame state, non-override fields will be set to value = AUTO, other values indicate override;
        ///     persist across mode-set and reboot.
        /// </summary>
        SetOverride,

        /// <summary>
        ///     Get properties associated with info-frame (each of the info-frame type will have properties).
        /// </summary>
        GetProperty,

        /// <summary>
        ///     Set properties associated with info-frame.
        /// </summary>
        SetProperty
    }

    /// <summary>
    ///     Contains possible info-frame data type
    /// </summary>
    public enum InfoFrameDataType : uint
    {
        /// <summary>
        ///     Auxiliary Video data
        /// </summary>
        AuxiliaryVideoInformation = 2,

        /// <summary>
        ///     Audio data
        /// </summary>
        AudioInformation = 4,
    }

    /// <summary>
    ///     Contains possible info-frame property modes
    /// </summary>
    public enum InfoFramePropertyMode : uint
    {
        /// <summary>
        ///     Driver determines whether to send info-frames.
        /// </summary>
        Auto = 0,

        /// <summary>
        ///     Driver always sends info-frame.
        /// </summary>
        Enable,

        /// <summary>
        ///     Driver never sends info-frame.
        /// </summary>
        Disable,

        /// <summary>
        ///     Driver only sends info-frame when client requests it via info-frame escape call.
        /// </summary>
        AllowOverride
    }

    /// <summary>
    ///     Contains possible values for AVI aspect ratio portions
    /// </summary>
    public enum InfoFrameVideoAspectRatioActivePortion : uint
    {
        /// <summary>
        ///     Disabled or not available
        /// </summary>
        Disabled = 0,

        /// <summary>
        ///     Letter box 16x9
        /// </summary>
        LetterboxGreaterThan16X9 = 4,

        /// <summary>
        ///     Equal to the source frame size
        /// </summary>
        EqualCodedFrame = 8,

        /// <summary>
        ///     Centered 4x3 ratio
        /// </summary>
        Center4X3 = 9,

        /// <summary>
        ///     Centered 16x9 ratio
        /// </summary>
        Center16X9 = 10,

        /// <summary>
        ///     Centered 14x9 ratio
        /// </summary>
        Center14X9 = 11,

        /// <summary>
        ///     Bordered 4x3 on 14x9
        /// </summary>
        Bordered4X3On14X9 = 13,

        /// <summary>
        ///     Bordered 16x9 on 14x9
        /// </summary>
        Bordered16X9On14X9 = 14,

        /// <summary>
        ///     Bordered 16x9 on 4x3
        /// </summary>
        Bordered16X9On4X3 = 15,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 31
    }

    /// <summary>
    ///     Gets the possible values for AVI source aspect ratio
    /// </summary>
    public enum InfoFrameVideoAspectRatioCodedFrame : uint
    {
        /// <summary>
        ///     No data available
        /// </summary>
        NoData = 0,

        /// <summary>
        ///     The 4x3 aspect ratio
        /// </summary>
        Aspect4X3,

        /// <summary>
        ///     The 16x9 aspect ratio
        /// </summary>
        Aspect16X9,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 7
    }

    /// <summary>
    ///     Contains possible AVI bar data that are available and should be used
    /// </summary>
    public enum InfoFrameVideoBarData : uint
    {
        /// <summary>
        ///     No bar data present
        /// </summary>
        NotPresent = 0,

        /// <summary>
        ///     Vertical bar
        /// </summary>
        Vertical,

        /// <summary>
        ///     Horizontal bar
        /// </summary>
        Horizontal,

        /// <summary>
        ///     Both sides have bars
        /// </summary>
        Both,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 7
    }

    /// <summary>
    ///     Contains possible AVI color formats
    /// </summary>
    public enum InfoFrameVideoColorFormat : uint
    {
        /// <summary>
        ///     The RGB color format
        /// </summary>
        RGB = 0,

        /// <summary>
        ///     The YCbCr422 color format
        /// </summary>
        YCbCr422,

        /// <summary>
        ///     The YCbCr444 color format
        /// </summary>
        YCbCr444,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 7
    }

    /// <summary>
    ///     Contains possible values for the AVI color space
    /// </summary>
    public enum InfoFrameVideoColorimetry : uint
    {
        /// <summary>
        ///     No data available
        /// </summary>
        NoData = 0,

        /// <summary>
        ///     The SMPTE170M color space
        /// </summary>
        SMPTE170M,

        /// <summary>
        ///     The ITURBT709 color space
        /// </summary>
        ITURBT709,

        /// <summary>
        ///     Extended colorimetry value should be used to get information regarding AVI color space
        /// </summary>
        UseExtendedColorimetry,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 7
    }

    /// <summary>
    ///     Contains possible AVI content type
    /// </summary>
    public enum InfoFrameVideoContentType : uint
    {
        /// <summary>
        ///     Graphics content
        /// </summary>
        Graphics = 0,

        /// <summary>
        ///     Photo content
        /// </summary>
        Photo,

        /// <summary>
        ///     Cinematic content
        /// </summary>
        Cinema,

        /// <summary>
        ///     Gaming content
        /// </summary>
        Game,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 7
    }

    /// <summary>
    ///     Contains possible values for the AVI extended color space
    /// </summary>
    public enum InfoFrameVideoExtendedColorimetry : uint
    {
        /// <summary>
        ///     The xvYCC601 color space
        /// </summary>
        // ReSharper disable once InconsistentNaming
        xvYCC601 = 0,

        /// <summary>
        ///     The xvYCC709 color space
        /// </summary>
        // ReSharper disable once InconsistentNaming
        xvYCC709,

        /// <summary>
        ///     The sYCC601 color space
        /// </summary>
        // ReSharper disable once InconsistentNaming
        sYCC601,

        /// <summary>
        ///     The AdobeYCC601 color space
        /// </summary>
        AdobeYCC601,

        /// <summary>
        ///     The AdobeRGB color space
        /// </summary>
        AdobeRGB,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 15
    }

    /// <summary>
    ///     Contains possible AVI video content modes
    /// </summary>
    public enum InfoFrameVideoITC : uint
    {
        /// <summary>
        ///     Normal video content (Consumer Electronics)
        /// </summary>
        VideoContent = 0,

        /// <summary>
        ///     Information Technology content
        /// </summary>
        ITContent,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 3
    }

    /// <summary>
    ///     Contains possible values for the AVI non uniform picture scaling
    /// </summary>
    public enum InfoFrameVideoNonUniformPictureScaling : uint
    {
        /// <summary>
        ///     No data available
        /// </summary>
        NoData = 0,

        /// <summary>
        ///     Horizontal scaling
        /// </summary>
        Horizontal,

        /// <summary>
        ///     Vertical scaling
        /// </summary>
        Vertical,

        /// <summary>
        ///     Scaling in both directions
        /// </summary>
        Both,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 7
    }

    /// <summary>
    ///     Contains possible AVI pixel repetition values
    /// </summary>
    public enum InfoFrameVideoPixelRepetition : uint
    {
        /// <summary>
        ///     No pixel repetition
        /// </summary>
        None = 0,

        /// <summary>
        ///     Two pixel repetition
        /// </summary>
        X2,

        /// <summary>
        ///     Three pixel repetition
        /// </summary>
        X3,

        /// <summary>
        ///     Four pixel repetition
        /// </summary>
        X4,

        /// <summary>
        ///     Five pixel repetition
        /// </summary>
        X5,

        /// <summary>
        ///     Six pixel repetition
        /// </summary>
        X6,

        /// <summary>
        ///     Seven pixel repetition
        /// </summary>
        X7,

        /// <summary>
        ///     Eight pixel repetition
        /// </summary>
        X8,

        /// <summary>
        ///     Nine pixel repetition
        /// </summary>
        X9,

        /// <summary>
        ///     Ten pixel repetition
        /// </summary>
        X10,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 31
    }

    /// <summary>
    ///     Contains possible values for the AVI RGB quantization
    /// </summary>
    public enum InfoFrameVideoRGBQuantization : uint
    {
        /// <summary>
        ///     Default setting
        /// </summary>
        Default = 0,

        /// <summary>
        ///     Limited RGB range [16-235] (86%)
        /// </summary>
        LimitedRange,

        /// <summary>
        ///     Full RGB range [0-255] (100%)
        /// </summary>
        FullRange,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 7
    }

    /// <summary>
    ///     Contains possible values for AVI scan information
    /// </summary>
    public enum InfoFrameVideoScanInfo : uint
    {
        /// <summary>
        ///     No data available
        /// </summary>
        NoData = 0,

        /// <summary>
        ///     Overscan
        /// </summary>
        OverScan,

        /// <summary>
        ///     Underscan
        /// </summary>
        UnderScan,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 7
    }

    /// <summary>
    ///     Contains possible AVI YCC quantization
    /// </summary>
    public enum InfoFrameVideoYCCQuantization : uint
    {
        /// <summary>
        ///     Limited YCC range
        /// </summary>
        LimitedRange = 0,

        /// <summary>
        ///     Full YCC range
        /// </summary>
        FullRange,

        /// <summary>
        ///     Auto (Unspecified)
        /// </summary>
        Auto = 7
    }

    /// <summary>
    ///     Possible values for the monitor capabilities connector type
    /// </summary>
    public enum MonitorCapabilitiesConnectorType : uint
    {
        /// <summary>
        ///     Unknown or invalid connector
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     VGA connector
        /// </summary>
        VGA,

        /// <summary>
        ///     Composite connector (TV)
        /// </summary>
        TV,

        /// <summary>
        ///     DVI connector
        /// </summary>
        DVI,

        /// <summary>
        ///     HDMI connector
        /// </summary>
        HDMI,

        /// <summary>
        ///     Display Port connector
        /// </summary>
        DisplayPort
    }

    /// <summary>
    ///     Contains possible values for the monitor capabilities type
    /// </summary>
    public enum MonitorCapabilitiesType : uint
    {
        /// <summary>
        ///     The Vendor Specific Data Block
        /// </summary>
        VSDB = 0x1000,

        /// <summary>
        ///     The Video Capability Data Block
        /// </summary>
        VCDB = 0x1001
    }

    /// <summary>
    ///     Possible rotate modes
    /// </summary>
    public enum Rotate : uint
    {
        /// <summary>
        ///     No rotation
        /// </summary>
        Degree0 = 0,

        /// <summary>
        ///     90 degree rotation
        /// </summary>
        Degree90 = 1,

        /// <summary>
        ///     180 degree rotation
        /// </summary>
        Degree180 = 2,

        /// <summary>
        ///     270 degree rotation
        /// </summary>
        Degree270 = 3,

        /// <summary>
        ///     This value is ignored
        /// </summary>
        Ignored = 4
    }

    /// <summary>
    ///     Possible scaling modes
    /// </summary>
    public enum Scaling
    {
        /// <summary>
        ///     No change
        /// </summary>
        Default = 0,

        /// <summary>
        ///     Balanced  - Full Screen
        /// </summary>
        ToClosest = 1,

        /// <summary>
        ///     Force GPU - Full Screen
        /// </summary>
        ToNative = 2,

        /// <summary>
        ///     Force GPU - Centered\No Scaling
        /// </summary>
        GPUScanOutToNative = 3,

        /// <summary>
        ///     Force GPU - Aspect Ratio
        /// </summary>
        ToAspectScanOutToNative = 5,

        /// <summary>
        ///     Balanced  - Aspect Ratio
        /// </summary>
        ToAspectScanOutToClosest = 6,

        /// <summary>
        ///     Balanced  - Centered\No Scaling
        /// </summary>
        GPUScanOutToClosest = 7,

        /// <summary>
        ///     Customized scaling - For future use
        /// </summary>
        Customized = 255
    }

    /// <summary>
    ///     Holds a list of possible scan out composition configurable parameters
    /// </summary>
    public enum ScanOutCompositionParameter : uint
    {
        /// <summary>
        ///     Warping re-sampling method parameter
        /// </summary>
        WarpingReSamplingMethod = 0
    }

    /// <summary>
    ///     Holds a list of possible scan out composition parameter values
    /// </summary>
    public enum ScanOutCompositionParameterValue : uint
    {
        /// <summary>
        ///     Default parameter value
        /// </summary>
        Default = 0,

        /// <summary>
        ///     BiLinear value for the warping re-sampling method parameter
        /// </summary>
        WarpingReSamplingMethodBiLinear = 0x100,

        /// <summary>
        ///     Bicubic Triangular value for the warping re-sampling method parameter
        /// </summary>
        WarpingReSamplingMethodBicubicTriangular = 0x101,

        /// <summary>
        ///     Bicubic Bell Shaped value for the warping re-sampling method parameter
        /// </summary>
        WarpingReSamplingMethodBicubicBellShaped = 0x102,

        /// <summary>
        ///     Bicubic B-Spline value for the warping re-sampling method parameter
        /// </summary>
        WarpingReSamplingMethodBicubicBSpline = 0x103,

        /// <summary>
        ///     Bicubic Adaptive Triangular value for the warping re-sampling method parameter
        /// </summary>
        WarpingReSamplingMethodBicubicAdaptiveTriangular = 0x104,

        /// <summary>
        ///     Bicubic Adaptive Bell Shaped value for the warping re-sampling method parameter
        /// </summary>
        WarpingReSamplingMethodBicubicAdaptiveBellShaped = 0x105,

        /// <summary>
        ///     Bicubic Adaptive B-Spline value for the warping re-sampling method parameter
        /// </summary>
        WarpingReSamplingMethodBicubicAdaptiveBSpline = 0x106
    }

    /// <summary>
    ///     Display spanning for Windows XP
    /// </summary>
    public enum SpanningOrientation
    {
        /// <summary>
        ///     No spanning
        /// </summary>
        None = 0,

        /// <summary>
        ///     Horizontal spanning
        /// </summary>
        Horizontal = 1,

        /// <summary>
        ///     Vertical spanning
        /// </summary>
        Vertical = 2
    }

    /// <summary>
    ///     Contains possible values for the type of the Static Metadata Descriptor block structure
    /// </summary>
    public enum StaticMetadataDescriptorId : uint
    {
        /// <summary>
        ///     Type 1 Static Metadata Descriptor block structure
        /// </summary>
        StaticMetadataType1 = 0
    }

    /// <summary>
    ///     Possible TV formats
    /// </summary>
    public enum TVFormat : uint
    {
        /// <summary>
        ///     Display is not a TV
        /// </summary>
        None = 0,

        /// <summary>
        ///     Standard definition NTSC M signal
        /// </summary>
        // ReSharper disable once InconsistentNaming
        SD_NTSCM = 0x00000001,

        /// <summary>
        ///     Standard definition NTSC J signal
        /// </summary>
        // ReSharper disable once InconsistentNaming
        SD_NTSCJ = 0x00000002,

        /// <summary>
        ///     Standard definition PAL M signal
        /// </summary>
        // ReSharper disable once InconsistentNaming
        SD_PALM = 0x00000004,

        /// <summary>
        ///     Standard definition PAL DFGH signal
        /// </summary>
        // ReSharper disable once InconsistentNaming
        SD_PALBDGH = 0x00000008,

        /// <summary>
        ///     Standard definition PAL N signal
        /// </summary>
        // ReSharper disable once InconsistentNaming
        SD_PAL_N = 0x00000010,

        /// <summary>
        ///     Standard definition PAL NC signal
        /// </summary>
        // ReSharper disable once InconsistentNaming
        SD_PAL_NC = 0x00000020,

        /// <summary>
        ///     Extended definition with height of 576 pixels interlaced
        /// </summary>
        // ReSharper disable once InconsistentNaming
        SD576i = 0x00000100,

        /// <summary>
        ///     Extended definition with height of 480 pixels interlaced
        /// </summary>
        // ReSharper disable once InconsistentNaming
        SD480i = 0x00000200,

        /// <summary>
        ///     Extended definition with height of 480 pixels progressive
        /// </summary>
        // ReSharper disable once InconsistentNaming
        ED480p = 0x00000400,

        /// <summary>
        ///     Extended definition with height of 576 pixels progressive
        /// </summary>
        // ReSharper disable once InconsistentNaming
        ED576p = 0x00000800,

        /// <summary>
        ///     High definition with height of 720 pixels progressive
        /// </summary>
        // ReSharper disable once InconsistentNaming
        HD720p = 0x00001000,

        /// <summary>
        ///     High definition with height of 1080 pixels interlaced
        /// </summary>
        // ReSharper disable once InconsistentNaming
        HD1080i = 0x00002000,

        /// <summary>
        ///     High definition with height of 1080 pixels progressive
        /// </summary>
        // ReSharper disable once InconsistentNaming
        HD1080p = 0x00004000,

        /// <summary>
        ///     High definition 50 frames per second with height of 720 pixels progressive
        /// </summary>
        // ReSharper disable once InconsistentNaming
        HD720p50 = 0x00008000,

        /// <summary>
        ///     High definition 24 frames per second with height of 1080 pixels progressive
        /// </summary>
        // ReSharper disable once InconsistentNaming
        HD1080p24 = 0x00010000,

        /// <summary>
        ///     High definition 50 frames per second with height of 1080 pixels interlaced
        /// </summary>
        // ReSharper disable once InconsistentNaming
        HD1080i50 = 0x00020000,

        /// <summary>
        ///     High definition 50 frames per second with height of 1080 pixels progressive
        /// </summary>
        // ReSharper disable once InconsistentNaming
        HD1080p50 = 0x00040000,

        /// <summary>
        ///     Ultra high definition 30 frames per second
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp30 = 0x00080000,

        /// <summary>
        ///     Ultra high definition 30 frames per second with width of 3840 pixels
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp30_3840 = UHD4Kp30,

        /// <summary>
        ///     Ultra high definition 25 frames per second
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp25 = 0x00100000,

        /// <summary>
        ///     Ultra high definition 25 frames per second with width of 3840 pixels
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp25_3840 = UHD4Kp25,

        /// <summary>
        ///     Ultra high definition 24 frames per second
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp24 = 0x00200000,

        /// <summary>
        ///     Ultra high definition 24 frames per second with width of 3840 pixels
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp24_3840 = UHD4Kp24,

        /// <summary>
        ///     Ultra high definition 24 frames per second with SMPTE signal
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp24_SMPTE = 0x00400000,

        /// <summary>
        ///     Ultra high definition 50 frames per second with width of 3840 pixels
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp50_3840 = 0x00800000,

        /// <summary>
        ///     Ultra high definition 60 frames per second with width of 3840 pixels
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp60_3840 = 0x00900000,

        /// <summary>
        ///     Ultra high definition 30 frames per second with width of 4096 pixels
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp30_4096 = 0x00A00000,

        /// <summary>
        ///     Ultra high definition 25 frames per second with width of 4096 pixels
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp25_4096 = 0x00B00000,

        /// <summary>
        ///     Ultra high definition 24 frames per second with width of 4096 pixels
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp24_4096 = 0x00C00000,

        /// <summary>
        ///     Ultra high definition 50 frames per second with width of 4096 pixels
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp50_4096 = 0x00D00000,

        /// <summary>
        ///     Ultra high definition 60 frames per second with width of 4096 pixels
        /// </summary>
        // ReSharper disable once InconsistentNaming
        UHD4Kp60_4096 = 0x00E00000,

        /// <summary>
        ///     Any other standard definition TV format
        /// </summary>
        SDOther = 0x01000000,

        /// <summary>
        ///     Any other extended definition TV format
        /// </summary>
        EDOther = 0x02000000,

        /// <summary>
        ///     Any other high definition TV format
        /// </summary>
        HDOther = 0x04000000,

        /// <summary>
        ///     Any other TV format
        /// </summary>
        Any = 0x80000000
    }

    /// <summary>
    ///     Display view modes
    /// </summary>
    public enum TargetViewMode
    {
        /// <summary>
        ///     Standard view mode
        /// </summary>
        Standard = 0,

        /// <summary>
        ///     Cloned view mode
        /// </summary>
        Clone = 1,

        /// <summary>
        ///     Horizontal span view mode
        /// </summary>
        HorizontalSpan = 2,

        /// <summary>
        ///     Vertical span view mode
        /// </summary>
        VerticalSpan = 3,

        /// <summary>
        ///     Dual view mode
        /// </summary>
        DualView = 4,

        /// <summary>
        ///     Multi view mode
        /// </summary>
        MultiView = 5
    }

    /// <summary>
    ///     Horizontal synchronized polarity modes
    /// </summary>
    public enum TimingHorizontalSyncPolarity : byte
    {
        /// <summary>
        ///     Positive horizontal synchronized polarity
        /// </summary>
        Positive = 0,

        /// <summary>
        ///     Negative horizontal synchronized polarity
        /// </summary>
        Negative = 1,

        /// <summary>
        ///     Default horizontal synchronized polarity
        /// </summary>
        Default = Negative
    }

    /// <summary>
    ///     Timing override modes
    /// </summary>
    public enum TimingOverride
    {
        /// <summary>
        ///     Current timing
        /// </summary>
        Current = 0,

        /// <summary>
        ///     Auto timing
        /// </summary>
        Auto,

        /// <summary>
        ///     EDID timing
        /// </summary>
        EDID,

        /// <summary>
        ///     VESA DMT timing
        /// </summary>
        DMT,

        /// <summary>
        ///     VESA DMT timing with reduced blanking
        /// </summary>
        DMTReducedBlanking,

        /// <summary>
        ///     VESA CVT timing
        /// </summary>
        CVT,

        /// <summary>
        ///     VESA CVT timing with reduced blanking
        /// </summary>
        CVTReducedBlanking,

        /// <summary>
        ///     VESA GTF
        /// </summary>
        GTF,

        /// <summary>
        ///     EIA 861x PreDefined timing
        /// </summary>
        EIA861,

        /// <summary>
        ///     AnalogTV PreDefined timing
        /// </summary>
        AnalogTV,

        /// <summary>
        ///     NVIDIA Custom timing
        /// </summary>
        Custom,

        /// <summary>
        ///     NVIDIA PreDefined timing
        /// </summary>
        Predefined,

        /// <summary>
        ///     NVIDIA PreDefined timing
        /// </summary>
        PSF = Predefined,

        /// <summary>
        ///     ASPR timing
        /// </summary>
        ASPR,

        /// <summary>
        ///     Override for SDI timing
        /// </summary>
        SDI,

        /// <summary>
        ///     Not used
        /// </summary>
        Max
    }

    /// <summary>
    ///     Timing scan modes
    /// </summary>
    public enum TimingScanMode : ushort
    {
        /// <summary>
        ///     Progressive scan mode
        /// </summary>
        Progressive = 0,

        /// <summary>
        ///     Interlaced scan mode
        /// </summary>
        Interlaced = 1,

        /// <summary>
        ///     Interlaced scan mode with extra vertical blank
        /// </summary>
        InterlacedWithExtraVerticalBlank = 1,

        /// <summary>
        ///     Interlaced scan mode without extra vertical blank
        /// </summary>
        InterlacedWithNoExtraVerticalBlank = 2
    }

    /// <summary>
    ///     Vertical synchronized polarity modes
    /// </summary>
    public enum TimingVerticalSyncPolarity : byte
    {
        /// <summary>
        ///     Positive vertical synchronized polarity
        /// </summary>
        Positive = 0,

        /// <summary>
        ///     Negative vertical synchronized polarity
        /// </summary>
        Negative = 1,

        /// <summary>
        ///     Default vertical synchronized polarity
        /// </summary>
        Default = Positive
    }

    /// <summary>
    ///     Holds a list of possible warping vertex formats
    /// </summary>
    public enum WarpingVerticeFormat : uint
    {
        /// <summary>
        ///     XYUVRQ Triangle Strip vertex format
        /// </summary>
        TriangleStripXYUVRQ = 0,

        /// <summary>
        ///     XYUVRQ Triangles format
        /// </summary>
        TrianglesXYUVRQ = 1
    }





    /// <summary>
    ///     Holds coordinates of a color in the color space
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ColorDataColorCoordinate : IEquatable<ColorDataColorCoordinate>
    {
        private ushort _X;
        private ushort _Y;

        /// <summary>
        ///     Gets the color space's X coordinate
        /// </summary>
        public float X
        {
            get => (float)_X / 0xC350;
        }

        /// <summary>
        ///     Gets the color space's Y coordinate
        /// </summary>
        public float Y
        {
            get => (float)_Y / 0xC350;
        }

        /// <summary>
        ///     Creates an instance of <see cref="ColorDataColorCoordinate" />.
        /// </summary>
        /// <param name="x">The color space's X coordinate.</param>
        /// <param name="y">The color space's Y coordinate.</param>
        public ColorDataColorCoordinate(float x, float y)
        {
            _X = (ushort)(Math.Min(Math.Max(x, 0), 1) * 0xC350);
            _Y = (ushort)(Math.Min(Math.Max(y, 0), 1) * 0xC350);
        }

        /// <summary>
        ///     Creates an instance of <see cref="ColorDataColorCoordinate" />.
        /// </summary>
        /// <param name="coordinate">The color space's coordinates.</param>
        public ColorDataColorCoordinate(PointF coordinate) : this(coordinate.X, coordinate.Y)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({X:F3}, {Y:F3})";
        }

        /// <inheritdoc />
        public bool Equals(ColorDataColorCoordinate other)
        {
            return _X == other._X && _Y == other._Y;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ColorDataColorCoordinate other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (_X.GetHashCode() * 397) ^ _Y.GetHashCode();
            }
        }

        /// <summary>
        ///     Checks two instance of <see cref="ColorDataColorCoordinate" /> for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>true if both instances are equal, otherwise false.</returns>
        public static bool operator ==(ColorDataColorCoordinate left, ColorDataColorCoordinate right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks two instance of <see cref="ColorDataColorCoordinate" /> for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>true if both instances are not equal, otherwise false.</returns>
        public static bool operator !=(ColorDataColorCoordinate left, ColorDataColorCoordinate right)
        {
            return !left.Equals(right);
        }
    }

    /// <inheritdoc cref="IColorData" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct ColorDataV1 : IInitializable, IColorData
    {
        internal StructureVersion _Version;
        internal ushort _Size;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly byte _Command;
        private readonly ColorDataBag _Data;

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private struct ColorDataBag
        {
            public byte ColorFormat;
            public byte Colorimetry;

            public ColorDataBag(ColorDataFormat colorFormat, ColorDataColorimetry colorimetry)
            {
                ColorFormat = (byte)colorFormat;
                Colorimetry = (byte)colorimetry;
            }
        }

        /// <summary>
        ///     Creates an instance of <see cref="ColorDataV1" /> to retrieve color data information
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public ColorDataV1(ColorDataCommand command)
        {
            this = typeof(ColorDataV1).Instantiate<ColorDataV1>();
            _Size = (ushort)_Version.StructureSize;

            if (command != ColorDataCommand.Get && command != ColorDataCommand.GetDefault)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
        }

        /// <summary>
        ///     Creates an instance of <see cref="ColorDataV1" /> to modify the color data
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        /// <param name="colorFormat">The color data color format.</param>
        /// <param name="colorimetry">The color data color space.</param>
        public ColorDataV1(
            ColorDataCommand command,
            ColorDataFormat colorFormat,
            ColorDataColorimetry colorimetry
        )
        {
            this = typeof(ColorDataV1).Instantiate<ColorDataV1>();
            _Size = (ushort)_Version.StructureSize;

            if (command != ColorDataCommand.Set && command != ColorDataCommand.IsSupportedColor)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
            _Data = new ColorDataBag(colorFormat, colorimetry);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public ColorDataFormat ColorFormat
        {
            get => (ColorDataFormat)_Data.ColorFormat;
        }

        /// <inheritdoc />
        public ColorDataColorimetry Colorimetry
        {
            get => (ColorDataColorimetry)_Data.Colorimetry;
        }

        /// <inheritdoc />
        public ColorDataDynamicRange? DynamicRange
        {
            get => null;
        }

        /// <inheritdoc />
        public ColorDataDepth? ColorDepth
        {
            get => null;
        }

        /// <inheritdoc />
        public ColorDataSelectionPolicy? SelectionPolicy
        {
            get => null;
        }

        /// <inheritdoc />
        public ColorDataDesktopDepth? DesktopColorDepth
        {
            get => null;
        }
    }

    /// <inheritdoc cref="IColorData" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct ColorDataV2 : IInitializable, IColorData
    {
        internal StructureVersion _Version;
        internal ushort _Size;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private byte _Command;
        private ColorDataBag _Data;

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private struct ColorDataBag
        {
            public byte ColorFormat;
            public byte Colorimetry;
            public byte ColorDynamicRange;

            public ColorDataBag(
                ColorDataFormat colorFormat,
                ColorDataColorimetry colorimetry,
                ColorDataDynamicRange colorDynamicRange
            )
            {
                ColorFormat = (byte)colorFormat;
                Colorimetry = (byte)colorimetry;
                ColorDynamicRange = (byte)colorDynamicRange;
            }
        }

        /// <summary>
        ///     Creates an instance of <see cref="ColorDataV2" /> to retrieve color data information
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public ColorDataV2(ColorDataCommand command)
        {
            this = typeof(ColorDataV2).Instantiate<ColorDataV2>();
            _Size = (ushort)_Version.StructureSize;

            if (command != ColorDataCommand.Get && command != ColorDataCommand.GetDefault)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
        }

        /// <summary>
        ///     Creates an instance of <see cref="ColorDataV2" /> to modify the color data
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        /// <param name="colorFormat">The color data color format.</param>
        /// <param name="colorimetry">The color data color space.</param>
        /// <param name="colorDynamicRange">The color data dynamic range.</param>
        public ColorDataV2(
            ColorDataCommand command,
            ColorDataFormat colorFormat,
            ColorDataColorimetry colorimetry,
            ColorDataDynamicRange colorDynamicRange
        )
        {
            this = typeof(ColorDataV2).Instantiate<ColorDataV2>();
            _Size = (ushort)_Version.StructureSize;

            if (command != ColorDataCommand.Set && command != ColorDataCommand.IsSupportedColor)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
            _Data = new ColorDataBag(colorFormat, colorimetry, colorDynamicRange);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public ColorDataFormat ColorFormat
        {
            get => (ColorDataFormat)_Data.ColorFormat;
        }

        /// <inheritdoc />
        public ColorDataColorimetry Colorimetry
        {
            get => (ColorDataColorimetry)_Data.Colorimetry;
        }

        /// <inheritdoc />
        public ColorDataDynamicRange? DynamicRange
        {
            get => (ColorDataDynamicRange)_Data.ColorDynamicRange;
        }

        /// <inheritdoc />
        public ColorDataDepth? ColorDepth
        {
            get => null;
        }

        /// <inheritdoc />
        public ColorDataSelectionPolicy? SelectionPolicy
        {
            get => null;
        }

        /// <inheritdoc />
        public ColorDataDesktopDepth? DesktopColorDepth
        {
            get => null;
        }
    }

    /// <inheritdoc cref="IColorData" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(3)]
    public struct ColorDataV3 : IInitializable, IColorData
    {
        internal StructureVersion _Version;
        internal ushort _Size;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private byte _Command;
        private ColorDataBag _Data;

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private struct ColorDataBag
        {
            public byte ColorFormat;
            public byte Colorimetry;
            public byte ColorDynamicRange;
            public ColorDataDepth ColorDepth;

            public ColorDataBag(
                ColorDataFormat colorFormat,
                ColorDataColorimetry colorimetry,
                ColorDataDynamicRange colorDynamicRange,
                ColorDataDepth colorDepth
            )
            {
                ColorFormat = (byte)colorFormat;
                Colorimetry = (byte)colorimetry;
                ColorDynamicRange = (byte)colorDynamicRange;
                ColorDepth = colorDepth;
            }
        }

        /// <summary>
        ///     Creates an instance of <see cref="ColorDataV3" /> to retrieve color data information
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public ColorDataV3(ColorDataCommand command)
        {
            this = typeof(ColorDataV3).Instantiate<ColorDataV3>();
            _Size = (ushort)_Version.StructureSize;

            if (command != ColorDataCommand.Get && command != ColorDataCommand.GetDefault)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
        }

        /// <summary>
        ///     Creates an instance of <see cref="ColorDataV3" /> to modify the color data
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        /// <param name="colorFormat">The color data color format.</param>
        /// <param name="colorimetry">The color data color space.</param>
        /// <param name="colorDynamicRange">The color data dynamic range.</param>
        /// <param name="colorDepth">The color data color depth.</param>
        public ColorDataV3(
            ColorDataCommand command,
            ColorDataFormat colorFormat,
            ColorDataColorimetry colorimetry,
            ColorDataDynamicRange colorDynamicRange,
            ColorDataDepth colorDepth
        )
        {
            this = typeof(ColorDataV3).Instantiate<ColorDataV3>();
            _Size = (ushort)_Version.StructureSize;

            if (command != ColorDataCommand.Set && command != ColorDataCommand.IsSupportedColor)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
            _Data = new ColorDataBag(colorFormat, colorimetry, colorDynamicRange, colorDepth);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public ColorDataFormat ColorFormat
        {
            get => (ColorDataFormat)_Data.ColorFormat;
        }

        /// <inheritdoc />
        public ColorDataColorimetry Colorimetry
        {
            get => (ColorDataColorimetry)_Data.Colorimetry;
        }

        /// <inheritdoc />
        public ColorDataDynamicRange? DynamicRange
        {
            get => (ColorDataDynamicRange)_Data.ColorDynamicRange;
        }

        /// <inheritdoc />
        public ColorDataDepth? ColorDepth
        {
            get
            {
                switch ((int)_Data.ColorDepth)
                {
                    case 6:
                        return ColorDataDepth.BPC6;
                    case 8:
                        return ColorDataDepth.BPC8;
                    case 10:
                        return ColorDataDepth.BPC10;
                    case 12:
                        return ColorDataDepth.BPC12;
                    case 16:
                        return ColorDataDepth.BPC16;
                    default:
                        return _Data.ColorDepth;
                }
            }
        }

        /// <inheritdoc />
        public ColorDataSelectionPolicy? SelectionPolicy
        {
            get => null;
        }

        /// <inheritdoc />
        public ColorDataDesktopDepth? DesktopColorDepth
        {
            get => null;
        }
    }

    /// <inheritdoc cref="IColorData" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(4)]
    public struct ColorDataV4 : IInitializable, IColorData
    {
        internal StructureVersion _Version;
        internal ushort _Size;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private byte _Command;
        private ColorDataBag _Data;

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private struct ColorDataBag
        {
            public byte ColorFormat;
            public byte Colorimetry;
            public byte ColorDynamicRange;
            public ColorDataDepth ColorDepth;
            public ColorDataSelectionPolicy ColorSelectionPolicy;

            public ColorDataBag(
                ColorDataFormat colorFormat,
                ColorDataColorimetry colorimetry,
                ColorDataDynamicRange colorDynamicRange,
                ColorDataDepth colorDepth,
                ColorDataSelectionPolicy colorSelectionPolicy
            )
            {
                ColorFormat = (byte)colorFormat;
                Colorimetry = (byte)colorimetry;
                ColorDynamicRange = (byte)colorDynamicRange;
                ColorDepth = colorDepth;
                ColorSelectionPolicy = colorSelectionPolicy;
            }
        }

        /// <summary>
        ///     Creates an instance of <see cref="ColorDataV4" /> to retrieve color data information
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public ColorDataV4(ColorDataCommand command)
        {
            this = typeof(ColorDataV4).Instantiate<ColorDataV4>();
            _Size = (ushort)_Version.StructureSize;

            if (command != ColorDataCommand.Get && command != ColorDataCommand.GetDefault)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
        }

        /// <summary>
        ///     Creates an instance of <see cref="ColorDataV4" /> to modify the color data
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        /// <param name="colorFormat">The color data color format.</param>
        /// <param name="colorimetry">The color data color space.</param>
        /// <param name="colorDynamicRange">The color data dynamic range.</param>
        /// <param name="colorDepth">The color data color depth.</param>
        /// <param name="colorSelectionPolicy">The color data selection policy.</param>
        public ColorDataV4(
            ColorDataCommand command,
            ColorDataFormat colorFormat,
            ColorDataColorimetry colorimetry,
            ColorDataDynamicRange colorDynamicRange,
            ColorDataDepth colorDepth,
            ColorDataSelectionPolicy colorSelectionPolicy
        )
        {
            this = typeof(ColorDataV4).Instantiate<ColorDataV4>();
            _Size = (ushort)_Version.StructureSize;

            if (command != ColorDataCommand.Set && command != ColorDataCommand.IsSupportedColor)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
            _Data = new ColorDataBag(colorFormat, colorimetry, colorDynamicRange, colorDepth, colorSelectionPolicy);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public ColorDataFormat ColorFormat
        {
            get => (ColorDataFormat)_Data.ColorFormat;
        }

        /// <inheritdoc />
        public ColorDataColorimetry Colorimetry
        {
            get => (ColorDataColorimetry)_Data.Colorimetry;
        }

        /// <inheritdoc />
        public ColorDataDynamicRange? DynamicRange
        {
            get => (ColorDataDynamicRange)_Data.ColorDynamicRange;
        }

        /// <inheritdoc />
        public ColorDataDepth? ColorDepth
        {
            get
            {
                switch ((int)_Data.ColorDepth)
                {
                    case 6:
                        return ColorDataDepth.BPC6;
                    case 8:
                        return ColorDataDepth.BPC8;
                    case 10:
                        return ColorDataDepth.BPC10;
                    case 12:
                        return ColorDataDepth.BPC12;
                    case 16:
                        return ColorDataDepth.BPC16;
                    default:
                        return _Data.ColorDepth;
                }
            }
        }

        /// <inheritdoc />
        public ColorDataSelectionPolicy? SelectionPolicy
        {
            get => _Data.ColorSelectionPolicy;
        }

        /// <inheritdoc />
        public ColorDataDesktopDepth? DesktopColorDepth
        {
            get => null;
        }
    }

    /// <inheritdoc cref="IColorData" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(5)]
    public struct ColorDataV5 : IInitializable, IColorData
    {
        internal StructureVersion _Version;
        internal ushort _Size;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private byte _Command;
        private ColorDataBag _Data;

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private struct ColorDataBag
        {
            public byte ColorFormat;
            public byte Colorimetry;
            public byte ColorDynamicRange;
            public ColorDataDepth ColorDepth;
            public ColorDataSelectionPolicy ColorSelectionPolicy;
            public ColorDataDesktopDepth DesktopColorDepth;

            public ColorDataBag(
                ColorDataFormat colorFormat,
                ColorDataColorimetry colorimetry,
                ColorDataDynamicRange colorDynamicRange,
                ColorDataDepth colorDepth,
                ColorDataSelectionPolicy colorSelectionPolicy,
                ColorDataDesktopDepth desktopColorDepth
            )
            {
                ColorFormat = (byte)colorFormat;
                Colorimetry = (byte)colorimetry;
                ColorDynamicRange = (byte)colorDynamicRange;
                ColorDepth = colorDepth;
                ColorSelectionPolicy = colorSelectionPolicy;
                DesktopColorDepth = desktopColorDepth;
            }
        }

        /// <summary>
        ///     Creates an instance of <see cref="ColorDataV5" /> to retrieve color data information
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public ColorDataV5(ColorDataCommand command)
        {
            this = typeof(ColorDataV5).Instantiate<ColorDataV5>();
            _Size = (ushort)_Version.StructureSize;

            if (command != ColorDataCommand.Get && command != ColorDataCommand.GetDefault)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
        }

        /// <summary>
        ///     Creates an instance of <see cref="ColorDataV4" /> to modify the color data
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        /// <param name="colorFormat">The color data color format.</param>
        /// <param name="colorimetry">The color data color space.</param>
        /// <param name="dynamicRange">The color data dynamic range.</param>
        /// <param name="colorDepth">The color data color depth.</param>
        /// <param name="colorSelectionPolicy">The color data selection policy.</param>
        /// <param name="desktopColorDepth">The color data desktop color depth.</param>
        public ColorDataV5(
            ColorDataCommand command,
            ColorDataFormat colorFormat,
            ColorDataColorimetry colorimetry,
            ColorDataDynamicRange dynamicRange,
            ColorDataDepth colorDepth,
            ColorDataSelectionPolicy colorSelectionPolicy,
            ColorDataDesktopDepth desktopColorDepth
        )
        {
            this = typeof(ColorDataV5).Instantiate<ColorDataV5>();
            _Size = (ushort)_Version.StructureSize;

            if (command != ColorDataCommand.Set && command != ColorDataCommand.IsSupportedColor)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
            _Data = new ColorDataBag(
                colorFormat,
                colorimetry,
                dynamicRange,
                colorDepth,
                colorSelectionPolicy,
                desktopColorDepth
            );
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public ColorDataFormat ColorFormat
        {
            get => (ColorDataFormat)_Data.ColorFormat;
        }

        /// <inheritdoc />
        public ColorDataColorimetry Colorimetry
        {
            get => (ColorDataColorimetry)_Data.Colorimetry;
        }

        /// <inheritdoc />
        public ColorDataDynamicRange? DynamicRange
        {
            get => (ColorDataDynamicRange)_Data.ColorDynamicRange;
        }

        /// <inheritdoc />
        public ColorDataDepth? ColorDepth
        {
            get
            {
                switch ((int)_Data.ColorDepth)
                {
                    case 6:
                        return ColorDataDepth.BPC6;
                    case 8:
                        return ColorDataDepth.BPC8;
                    case 10:
                        return ColorDataDepth.BPC10;
                    case 12:
                        return ColorDataDepth.BPC12;
                    case 16:
                        return ColorDataDepth.BPC16;
                    default:
                        return _Data.ColorDepth;
                }
            }
        }

        /// <inheritdoc />
        public ColorDataSelectionPolicy? SelectionPolicy
        {
            get => _Data.ColorSelectionPolicy;
        }

        /// <inheritdoc />
        public ColorDataDesktopDepth? DesktopColorDepth
        {
            get => _Data.DesktopColorDepth;
        }
    }

    /// <summary>
    ///     Hold information about a custom display resolution
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct CustomDisplay : IInitializable
    {
        internal StructureVersion _Version;
        internal uint _Width;
        internal uint _Height;
        internal uint _Depth;
        internal ColorFormat _ColorFormat;
        internal ViewPortF _SourcePartition;
        internal float _XRatio;
        internal float _YRatio;
        internal Timing _Timing;
        internal uint _Flags;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the source surface (source mode) width.
        /// </summary>
        public uint Width
        {
            get => _Width;
        }

        /// <summary>
        ///     Gets the source surface (source mode) height.
        /// </summary>
        public uint Height
        {
            get => _Height;
        }

        /// <summary>
        ///     Gets the source surface color depth. "0" means all 8/16/32bpp.
        /// </summary>
        public uint Depth
        {
            get => _Depth;
        }

        /// <summary>
        ///     Gets the color format (optional)
        /// </summary>
        public ColorFormat ColorFormat
        {
            get => _ColorFormat;
        }

        /// <summary>
        ///     Gets the source partition viewport. All values are between [0, 1]. For multi-mon support, should be set to
        ///     (0,0,1.0,1.0) for now.
        /// </summary>
        public ViewPortF SourcePartition
        {
            get => _SourcePartition;
        }

        /// <summary>
        ///     Gets the horizontal scaling ratio.
        /// </summary>
        public float XRatio
        {
            get => _XRatio;
        }

        /// <summary>
        ///     Gets the vertical scaling ratio.
        /// </summary>
        public float YRatio
        {
            get => _YRatio;
        }

        /// <summary>
        ///     Gets the timing used to program TMDS/DAC/LVDS/HDMI/TVEncoder, etc.
        /// </summary>
        public Timing Timing
        {
            get => _Timing;
        }

        /// <summary>
        ///     Gets a boolean value indicating that a hardware mode-set without OS update should be performed.
        /// </summary>
        public bool HardwareModeSetOnly
        {
            get => _Flags.GetBit(0); 
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <summary>
        ///     Creates an instance of <see cref="CustomDisplay" />
        /// </summary>
        /// <param name="width">The source surface (source mode) width.</param>
        /// <param name="height">The source surface (source mode) height.</param>
        /// <param name="depth">The source surface color depth. "0" means all 8/16/32bpp.</param>
        /// <param name="colorFormat">The color format (optional)</param>
        /// <param name="xRatio">The horizontal scaling ratio.</param>
        /// <param name="yRatio">The vertical scaling ratio.</param>
        /// <param name="timing">The timing used to program TMDS/DAC/LVDS/HDMI/TVEncoder, etc.</param>
        /// <param name="hwModeSetOnly">A boolean value indicating that a hardware mode-set without OS update should be performed.</param>
        public CustomDisplay(
            uint width,
            uint height,
            uint depth,
            ColorFormat colorFormat,
            float xRatio,
            float yRatio,
            Timing timing,
            bool hwModeSetOnly
        )
        {
            this = typeof(CustomDisplay).Instantiate<CustomDisplay>();

            _Width = width;
            _Height = height;
            _Depth = depth;
            _ColorFormat = colorFormat;
            _SourcePartition = new ViewPortF(0, 0, 1, 1);
            _XRatio = xRatio;
            _YRatio = yRatio;
            _Timing = timing;
            _Flags = _Flags.SetBit(0, hwModeSetOnly);
        }
    }

    /// <inheritdoc />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct DisplayColorData : IDisplayColorData
    {
        private ColorDataColorCoordinate _FirstColorCoordinate;
        private ColorDataColorCoordinate _SecondColorCoordinate;
        private ColorDataColorCoordinate _ThirdColorCoordinate;
        private ColorDataColorCoordinate _WhiteColorCoordinate;
        private ushort _MaximumDesiredContentLuminance;
        private ushort _MinimumDesiredContentLuminance;
        private ushort _MaximumDesiredFrameAverageLightLevel;

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataColorCoordinate FirstColorCoordinate
        {
            get => _FirstColorCoordinate;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataColorCoordinate SecondColorCoordinate
        {
            get => _SecondColorCoordinate;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataColorCoordinate ThirdColorCoordinate
        {
            get => _ThirdColorCoordinate;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataColorCoordinate WhiteColorCoordinate
        {
            get => _WhiteColorCoordinate;
        }

        /// <summary>
        ///     Gets the maximum desired content luminance [1.0-65535] in cd/m^2
        /// </summary>
        public float MaximumDesiredContentLuminance
        {
            get => _MaximumDesiredContentLuminance;
        }

        /// <summary>
        ///     Gets the maximum desired content frame average light level (a.k.a MaxFALL) [1.0-65535] in cd/m^2
        /// </summary>
        public float MaximumDesiredContentFrameAverageLightLevel
        {
            get => _MaximumDesiredFrameAverageLightLevel;
        }

        /// <summary>
        ///     Gets the maximum desired content luminance [1.0-6.5535] in cd/m^2
        /// </summary>
        public float MinimumDesiredContentLuminance
        {
            get => _MinimumDesiredContentLuminance / 10000f;
        }
    }

    /// <inheritdoc />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct DVStaticMetadata : IEquatable<DVStaticMetadata>, ICloneable
    {
        // TODO: FIX THIS! This actually hasn't been set up properly, but is a simple copy of DisplayColorData, waiting for us to modify it to actually become DVStaticMetadata
        // We need to write an Interface for this.

        /*struct
        {
            NvU32 VSVDB_version               : 3;                //!< Version of Vendor Data block,Version 0: 25 bytes  Version 1: 14 bytes
            NvU32 dm_version                  : 8;                //!< Upper Nibble represents major version of Display Management(DM) while lower represents minor version of DM
            NvU32 supports_2160p60hz          : 1;                //!< If set sink is capable of 4kx2k @ 60hz
            NvU32 supports_YUV422_12bit       : 1;                //!< If set, sink is capable of YUV422-12 bit
            NvU32 supports_global_dimming     : 1;                //!< Indicates if sink supports global dimming
            NvU32 colorimetry                 : 1;                //!< If set indicates sink supports DCI P3 colorimetry, REc709 otherwise
            NvU32 supports_backlight_control  : 2;                //!< This is set when sink is using lowlatency interface and can control its backlight.
            NvU32 backlt_min_luma             : 2;                //!< It is the level for Backlt min luminance value.
            NvU32 interface_supported_by_sink : 2;                //!< Indicates the interface (standard or low latency) supported by the sink.
            NvU32 supports_10b_12b_444        : 2;                //!< It is set when interface supported is low latency, it tells whether it supports 10 bit or 12 bit RGB 4:4:4 or YCbCr 4:4:4 or both.
            NvU32 reserved                    : 9;                //!< Should be set to zero
                                                                  //!< All values below are encoded use DolbyVisionHDMITransmissionSpecification document to decode
            NvU16 target_min_luminance;                           //!< Represents min luminance level of Sink
            NvU16 target_max_luminance;                           //!< Represents max luminance level of sink
            NvU16 cc_red_x;                                       //!< Red primary chromaticity coordinate x
            NvU16 cc_red_y;                                       //!< Red primary chromaticity coordinate y
            NvU16 cc_green_x;                                     //!< Green primary chromaticity coordinate x
            NvU16 cc_green_y;                                     //!< Green primary chromaticity coordinate Y
            NvU16 cc_blue_x;                                      //!< Blue primary chromaticity coordinate x
            NvU16 cc_blue_y;                                      //!< Blue primary chromaticity coordinate y
            NvU16 cc_white_x;                                     //!< White primary chromaticity coordinate x
            NvU16 cc_white_y;                                     //!< White primary chromaticity coordinate y
        }
        dv_static_metadata;*/



        private UInt32 _Flags;
        private UInt16 _TargetMinLuminance;
        private UInt16 _TargetMaxLuminance;
        private UInt16 _CCRedX;
        private UInt16 _CCRedY;
        private UInt16 _CCGreenX;
        private UInt16 _CCGreenY;
        private UInt16 _CCBlueX;
        private UInt16 _CCBlueY;
        private UInt16 _CCWhiteX;
        private UInt16 _CCWhiteY;


        /// <summary>
        ///     Gets a boolean value indicating that a hardware mode-set without OS update should be performed.
        /// </summary>
        public byte VsvdbVersion
        {
            get => (byte)_Flags.GetBits(0,3);
        }

        /// <summary>
        ///     Gets a boolean value indicating that a hardware mode-set without OS update should be performed.
        /// </summary>
        public byte DmVersion
        {
            get => (byte)_Flags.GetBits(3,8);
        }

        /// <summary>
        ///     Gets a boolean value indicating that sink is capable of 4kx2k @ 60hz.
        /// </summary>
        public bool Supports2160p60hz
        {
            get => _Flags.GetBit(12);
            set => _Flags = _Flags.SetBit(12, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating that sink is capable of YUV422-12 bit.
        /// </summary>
        public bool SupportsYUV422_12bit
        {
            get => _Flags.GetBit(13); 
            set => _Flags = _Flags.SetBit(13, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating that sink supports global dimming.
        /// </summary>
        public bool SupportsGlobalDimming
        {
            get => _Flags.GetBit(14); 
            set => _Flags = _Flags.SetBit(14, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating that sink supports DCI P3 colorimetry, REC709 otherwise.
        /// </summary>
        public bool SupportsColorimetry
        {
            get => _Flags.GetBit(15); 
            set => _Flags = _Flags.SetBit(15, value);
        }

        /// <summary>
        ///     Gets a  byte value indicating that the sink is using lowlatency interface and can control its backlight.
        /// </summary>
        public byte SupportsBacklightControl
        {
            get => (byte)_Flags.GetBits(16,2); 
        }

        /// <summary>
        ///     Gets a byte value indicating that the level for Backlt min luminance value (if backlighting set).
        /// </summary>
        public byte BacklitMinimumLumination
        {
            get => (byte)_Flags.GetBits(18, 2);
        }

        /// <summary>
        ///     Gets a byte value indicating the interface (standard or low latency) supported by the sink.
        /// </summary>
        public byte InterfaceSupportedBySink
        {
            get => (byte)_Flags.GetBits(20, 2);
        }

        /// <summary>
        ///     Gets a byte value indicating the interface (standard or low latency) supported by the sink.
        /// </summary>
        public byte Supports10b12b444
        {
            get => (byte)_Flags.GetBits(22, 2);
        }


        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public UInt16 TargetMinLuminance
        {
            get => _TargetMinLuminance;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public UInt16 TargetMaxLuminance
        {
            get => _TargetMaxLuminance;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public UInt16 CCRedX
        {
            get => _CCRedX;
        }
        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public UInt16 CCRedY
        {
            get => _CCRedY;
        }
        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public UInt16 CCGreenX
        {
            get => _CCGreenX;
        }
        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public UInt16 CCGreenY
        {
            get => _CCGreenY;
        }
        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public UInt16 CCBlueX
        {
            get => _CCBlueX;
        }
        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public UInt16 CCBlueY
        {
            get => _CCBlueY;
        }
        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public UInt16 CCWhiteX
        {
            get => _CCWhiteX;
        }
        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public UInt16 CCWhiteY
        {
            get => _CCWhiteY;
        }

        public override bool Equals(object obj) => obj is DVStaticMetadata other && this.Equals(other);
        public bool Equals(DVStaticMetadata other)
        => _Flags == other._Flags &&
           _TargetMinLuminance == other._TargetMinLuminance &&
           _TargetMaxLuminance == other._TargetMaxLuminance &&
           _CCRedX == other._CCRedX &&
           _CCRedY == other._CCRedY &&
           _CCGreenX == other._CCGreenX &&
            _CCGreenY == other._CCGreenY &&
            _CCBlueX == other._CCBlueX &&
            _CCBlueY == other._CCBlueY &&
            _CCWhiteX == other._CCWhiteX &&
            _CCWhiteY == other._CCWhiteY;

        public override Int32 GetHashCode()
        {
            return (_Flags, _TargetMinLuminance, _TargetMaxLuminance, _CCRedX, _CCRedY, _CCGreenX, _CCGreenY, _CCBlueX, _CCBlueY, _CCWhiteX, _CCWhiteY).GetHashCode();
        }
        public static bool operator ==(DVStaticMetadata lhs, DVStaticMetadata rhs) => lhs.Equals(rhs);

        public static bool operator !=(DVStaticMetadata lhs, DVStaticMetadata rhs) => !(lhs == rhs);
        public object Clone()
        {
            DVStaticMetadata other = (DVStaticMetadata)MemberwiseClone();
            return other;
        }
    }

    /// <inheritdoc />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct HDR10PlusVSVDB : IDisplayColorData
    {
        // TODO: FIX THIS! This actually hasn't been set up properly, but is a simple copy of DisplayColorData, waiting for us to modify it to actually become HDR10PlusVSVDB
        // We need to write an Interface for this.

        /*struct
        {
            NvU32 VSVDB_version               : 3;                //!< Version of Vendor Data block,Version 0: 25 bytes  Version 1: 14 bytes
            NvU32 dm_version                  : 8;                //!< Upper Nibble represents major version of Display Management(DM) while lower represents minor version of DM
            NvU32 supports_2160p60hz          : 1;                //!< If set sink is capable of 4kx2k @ 60hz
            NvU32 supports_YUV422_12bit       : 1;                //!< If set, sink is capable of YUV422-12 bit
            NvU32 supports_global_dimming     : 1;                //!< Indicates if sink supports global dimming
            NvU32 colorimetry                 : 1;                //!< If set indicates sink supports DCI P3 colorimetry, REc709 otherwise
            NvU32 supports_backlight_control  : 2;                //!< This is set when sink is using lowlatency interface and can control its backlight.
            NvU32 backlt_min_luma             : 2;                //!< It is the level for Backlt min luminance value.
            NvU32 interface_supported_by_sink : 2;                //!< Indicates the interface (standard or low latency) supported by the sink.
            NvU32 supports_10b_12b_444        : 2;                //!< It is set when interface supported is low latency, it tells whether it supports 10 bit or 12 bit RGB 4:4:4 or YCbCr 4:4:4 or both.
            NvU32 reserved                    : 9;                //!< Should be set to zero
                                                                  //!< All values below are encoded use DolbyVisionHDMITransmissionSpecification document to decode
            NvU16 target_min_luminance;                           //!< Represents min luminance level of Sink
            NvU16 target_max_luminance;                           //!< Represents max luminance level of sink
            NvU16 cc_red_x;                                       //!< Red primary chromaticity coordinate x
            NvU16 cc_red_y;                                       //!< Red primary chromaticity coordinate y
            NvU16 cc_green_x;                                     //!< Green primary chromaticity coordinate x
            NvU16 cc_green_y;                                     //!< Green primary chromaticity coordinate Y
            NvU16 cc_blue_x;                                      //!< Blue primary chromaticity coordinate x
            NvU16 cc_blue_y;                                      //!< Blue primary chromaticity coordinate y
            NvU16 cc_white_x;                                     //!< White primary chromaticity coordinate x
            NvU16 cc_white_y;                                     //!< White primary chromaticity coordinate y
        }
        dv_static_metadata;
        
         
         struct
        {
            NvU16 application_version               : 2;          //!< Application version of HDR10+ Vendor Specific Video Data Block
            NvU16 full_frame_peak_luminance_index   : 2;          //!< Full frame peak luminance index
            NvU16 peak_luminance_index              : 4;          //!< Peak luminance index
            NvU16 reserved                          : 8;
        }hdr10plus_vsvdb;
        */

        private ColorDataColorCoordinate _FirstColorCoordinate;
        private ColorDataColorCoordinate _SecondColorCoordinate;
        private ColorDataColorCoordinate _ThirdColorCoordinate;
        private ColorDataColorCoordinate _WhiteColorCoordinate;
        private ushort _MaximumDesiredContentLuminance;
        private ushort _MinimumDesiredContentLuminance;
        private ushort _MaximumDesiredFrameAverageLightLevel;

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataColorCoordinate FirstColorCoordinate
        {
            get => _FirstColorCoordinate;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataColorCoordinate SecondColorCoordinate
        {
            get => _SecondColorCoordinate;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataColorCoordinate ThirdColorCoordinate
        {
            get => _ThirdColorCoordinate;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataColorCoordinate WhiteColorCoordinate
        {
            get => _WhiteColorCoordinate;
        }

        /// <summary>
        ///     Gets the maximum desired content luminance [1.0-65535] in cd/m^2
        /// </summary>
        public float MaximumDesiredContentLuminance
        {
            get => _MaximumDesiredContentLuminance;
        }

        /// <summary>
        ///     Gets the maximum desired content frame average light level (a.k.a MaxFALL) [1.0-65535] in cd/m^2
        /// </summary>
        public float MaximumDesiredContentFrameAverageLightLevel
        {
            get => _MaximumDesiredFrameAverageLightLevel;
        }

        /// <summary>
        ///     Gets the maximum desired content luminance [1.0-6.5535] in cd/m^2
        /// </summary>
        public float MinimumDesiredContentLuminance
        {
            get => _MinimumDesiredContentLuminance / 10000f;
        }
    }



    /// <summary>
    ///     DisplayHandle is a one-to-one map to the GDI handle of an attached display in the Windows Display Properties
    ///     Settings page.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayHandle : IHandle, IEquatable<DisplayHandle>
    {
        internal readonly IntPtr _MemoryAddress;

        /// <inheritdoc />
        public IntPtr MemoryAddress
        {
            get => _MemoryAddress;
        }

        /// <inheritdoc />
        public bool IsNull
        {
            get => _MemoryAddress == IntPtr.Zero;
        }

        /// <inheritdoc />
        public bool Equals(DisplayHandle other)
        {
            return _MemoryAddress.Equals(other._MemoryAddress);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is DisplayHandle handle && Equals(handle);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _MemoryAddress.GetHashCode();
        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(DisplayHandle left, DisplayHandle right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(DisplayHandle left, DisplayHandle right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"DisplayHandle #{MemoryAddress.ToInt64()}";
        }

        /// <summary>
        ///     Gets default DisplayHandle with a null pointer
        /// </summary>
        public static DisplayHandle DefaultHandle
        {
            get => default(DisplayHandle);
        }
    }

    /// <inheritdoc cref="IHDMISupportInfo" />
    [StructLayout(LayoutKind.Explicit, Pack = 8)]
    [StructureVersion(1)]
    public struct HDMISupportInfoV1 : IInitializable, IHDMISupportInfo
    {
        [FieldOffset(0)] internal StructureVersion _Version;
        [FieldOffset(4)] private uint _Flags;
        [FieldOffset(8)] private uint _EDID861ExtensionRevision;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public bool IsGPUCapableOfHDMIOutput
        {
            get => _Flags.GetBit(0); 
            set => _Flags = _Flags.SetBit(0, value);

        }

        /// <inheritdoc />
        public bool? IsMonitorCapableOfsYCC601
        {
            get => null;
        }

        /// <inheritdoc />
        public bool IsMonitorCapableOfUnderscan
        {
            get => _Flags.GetBit(1); 
            set => _Flags = _Flags.SetBit(1, value);
        }

        /// <inheritdoc />
        public bool? IsMonitorCapableOfAdobeYCC601
        {
            get => null;
        }

        /// <inheritdoc />
        public bool IsMonitorCapableOfBasicAudio
        {
            get => _Flags.GetBit(2); 
            set => _Flags = _Flags.SetBit(2, value);
        }

        /// <inheritdoc />
        public bool IsMonitorCapableOfYCbCr444
        {
            get => _Flags.GetBit(3); 
            set => _Flags = _Flags.SetBit(3, value);
        }

        /// <inheritdoc />
        public bool IsMonitorCapableOfYCbCr422
        {
            get => _Flags.GetBit(4); 
            set => _Flags = _Flags.SetBit(4, value);
        }

        /// <inheritdoc />
        // ReSharper disable once IdentifierTypo
        public bool IsMonitorCapableOfxvYCC601
        {
            get => _Flags.GetBit(5); 
            set => _Flags = _Flags.SetBit(5, value);
        }

        /// <inheritdoc />
        // ReSharper disable once IdentifierTypo
        public bool IsMonitorCapableOfxvYCC709
        {
            get => _Flags.GetBit(6); 
            set => _Flags = _Flags.SetBit(6, value);
        }

        /// <inheritdoc />
        public bool IsHDMIMonitor
        {
            get => _Flags.GetBit(7); 
            set => _Flags = _Flags.SetBit(7, value);
        }

        /// <inheritdoc />
        public bool? IsMonitorCapableOfAdobeRGB
        {
            get => null;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public uint EDID861ExtensionRevision
        {
            get => _EDID861ExtensionRevision;
        }
    }

    /// <inheritdoc cref="IHDMISupportInfo" />
    [StructLayout(LayoutKind.Explicit, Pack = 8)]
    [StructureVersion(2)]
    public struct HDMISupportInfoV2 : IInitializable, IHDMISupportInfo
    {
        [FieldOffset(0)] internal StructureVersion _Version;
        [FieldOffset(4)] private uint _Flags;
        [FieldOffset(8)] private uint _EDID861ExtensionRevision;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public bool IsGPUCapableOfHDMIOutput
        {
            get => _Flags.GetBit(0);
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <inheritdoc />
        public bool IsMonitorCapableOfUnderscan
        {
            get => _Flags.GetBit(1); 
            set => _Flags = _Flags.SetBit(1, value);
        }

        /// <inheritdoc />
        public bool IsMonitorCapableOfBasicAudio
        {
            get => _Flags.GetBit(2); 
            set => _Flags = _Flags.SetBit(2, value);
        }

        /// <inheritdoc />
        public bool IsMonitorCapableOfYCbCr444
        {
            get => _Flags.GetBit(3); 
            set => _Flags = _Flags.SetBit(3, value);
        }

        /// <inheritdoc />
        public bool IsMonitorCapableOfYCbCr422
        {
            get => _Flags.GetBit(4); 
            set => _Flags = _Flags.SetBit(4, value);
        }

        /// <inheritdoc />
        // ReSharper disable once IdentifierTypo
        public bool IsMonitorCapableOfxvYCC601
        {
            get => _Flags.GetBit(5); 
            set => _Flags = _Flags.SetBit(5, value);
        }

        /// <inheritdoc />
        // ReSharper disable once IdentifierTypo
        public bool IsMonitorCapableOfxvYCC709
        {
            get => _Flags.GetBit(6); 
            set => _Flags = _Flags.SetBit(6, value);
        }

        /// <inheritdoc />
        public bool IsHDMIMonitor
        {
            get => _Flags.GetBit(7); 
            set => _Flags = _Flags.SetBit(7, value);
        }

        /// <inheritdoc />
        public bool? IsMonitorCapableOfsYCC601
        {
            get => _Flags.GetBit(8); 
            set => _Flags = _Flags.SetBit(8, value.Value);
        }

        /// <inheritdoc />
        public bool? IsMonitorCapableOfAdobeYCC601
        {
            get => _Flags.GetBit(9);
            set => _Flags = _Flags.SetBit(9, value.Value);
        }

        /// <inheritdoc />
        public bool? IsMonitorCapableOfAdobeRGB
        {
            get => _Flags.GetBit(10);
            set => _Flags = _Flags.SetBit(10, value.Value);
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public uint EDID861ExtensionRevision
        {
            get => _EDID861ExtensionRevision;
        }
    }

    /// <summary>
    ///     Contains information regarding HDR capabilities of a display
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct HDRCapabilitiesV1 : IInitializable, IHDRCapabilities
    {
        internal StructureVersion _Version;
        private uint _RawReserved;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private StaticMetadataDescriptorId _StaticMetadataDescriptorId;
        private DisplayColorData _DisplayData;

        internal HDRCapabilitiesV1(bool expandDriverDefaultHDRParameters)
        {
            this = typeof(HDRCapabilitiesV1).Instantiate<HDRCapabilitiesV1>();
            _RawReserved = 0u.SetBit(3, expandDriverDefaultHDRParameters);
            _StaticMetadataDescriptorId = StaticMetadataDescriptorId.StaticMetadataType1;
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the display color space configurations
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public DisplayColorData DisplayData
        {
            get => _DisplayData;
        }

        /// <summary>
        ///     Gets a boolean value indicating if the HDMI2.0a UHDA HDR with ST2084 EOTF (CEA861.3) is supported.
        /// </summary>
        public bool IsST2084EOTFSupported
        {
            get => _RawReserved.GetBit(0);
            set => _RawReserved = _RawReserved.SetBit(0, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the HDMI2.0a traditional HDR gamma (CEA861.3) is supported.
        /// </summary>
        public bool IsTraditionalHDRGammaSupported
        {
            get => _RawReserved.GetBit(1); 
            set => _RawReserved = _RawReserved.SetBit(1, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the Extended Dynamic Range on SDR displays is supported.
        /// </summary>
        public bool IsEDRSupported
        {
            get => _RawReserved.GetBit(2); 
            set => _RawReserved = _RawReserved.SetBit(2, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the default EDID HDR parameters is expanded;
        ///     otherwise false if this instance contains actual HDR parameters.
        /// </summary>
        public bool IsDriverDefaultHDRParametersExpanded
        {
            get => _RawReserved.GetBit(3); 
            set => _RawReserved = _RawReserved.SetBit(3, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the HDMI2.0a traditional SDR gamma is supported.
        /// </summary>
        public bool IsTraditionalSDRGammaSupported
        {
            get => _RawReserved.GetBit(4); 
            set => _RawReserved = _RawReserved.SetBit(4, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if Dolby Vision is supported.
        /// </summary>
        public bool IsDolbyVisionSupported {
            get => false;
        }
    }

    /// <summary>
    ///     Contains information regarding HDR capabilities of a display
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct HDRCapabilitiesV2 : IInitializable, IHDRCapabilities
    {
        internal StructureVersion _Version;
        private uint _RawReserved;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private StaticMetadataDescriptorId _StaticMetadataDescriptorId;
        private DisplayColorData _DisplayData;
        private DVStaticMetadata _DVStaticMetadata;

        internal HDRCapabilitiesV2(bool expandDriverDefaultHDRParameters)
        {
            this = typeof(HDRCapabilitiesV2).Instantiate<HDRCapabilitiesV2>();
            _RawReserved = 0u.SetBit(3, expandDriverDefaultHDRParameters);
            _StaticMetadataDescriptorId = StaticMetadataDescriptorId.StaticMetadataType1;
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the display color space configurations
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public DisplayColorData DisplayData
        {
            get => _DisplayData;
            set => _DisplayData = value;
        }

        /// <summary>
        ///     Gets a boolean value indicating if the HDMI2.0a UHDA HDR with ST2084 EOTF (CEA861.3) is supported.
        /// </summary>
        public bool IsST2084EOTFSupported
        {
            get => _RawReserved.GetBit(0); 
            set => _RawReserved = _RawReserved.SetBit(0, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the HDMI2.0a traditional HDR gamma (CEA861.3) is supported.
        /// </summary>
        public bool IsTraditionalHDRGammaSupported
        {
            get => _RawReserved.GetBit(1); 
            set => _RawReserved = _RawReserved.SetBit(1, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the Extended Dynamic Range on SDR displays is supported.
        /// </summary>
        public bool IsEDRSupported
        {
            get => _RawReserved.GetBit(2); 
            set => _RawReserved = _RawReserved.SetBit(2, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the default EDID HDR parameters is expanded;
        ///     otherwise false if this instance contains actual HDR parameters.
        /// </summary>
        public bool IsDriverDefaultHDRParametersExpanded
        {
            get => _RawReserved.GetBit(3); 
            set => _RawReserved = _RawReserved.SetBit(3, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the HDMI2.0a traditional SDR gamma is supported.
        /// </summary>
        public bool IsTraditionalSDRGammaSupported
        {
            get => _RawReserved.GetBit(4); 
            set => _RawReserved = _RawReserved.SetBit(4, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if Dolby Vision is supported.
        /// </summary>
        public bool IsDolbyVisionSupported
        {
            get => _RawReserved.GetBit(5); 
            set => _RawReserved = _RawReserved.SetBit(5, value);
        }
    }

    /// <summary>
    ///     Contains information regarding HDR capabilities of a display
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(3)]
    public struct HDRCapabilitiesV3 : IInitializable, IHDRCapabilities
    {
        internal StructureVersion _Version;
        private uint _RawReserved;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private StaticMetadataDescriptorId _StaticMetadataDescriptorId;
        private DisplayColorData _DisplayData;
        private DVStaticMetadata _DVStaticMetadata;
        private HDR10PlusVSVDB _HDR10PlusVSVDB;

        internal HDRCapabilitiesV3(bool expandDriverDefaultHDRParameters)
        {
            this = typeof(HDRCapabilitiesV3).Instantiate<HDRCapabilitiesV3>();
            _RawReserved = 0u.SetBit(3, expandDriverDefaultHDRParameters);
            _StaticMetadataDescriptorId = StaticMetadataDescriptorId.StaticMetadataType1;
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the display color space configurations
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public DisplayColorData DisplayData
        {
            get => _DisplayData;
            set => _DisplayData = value;
        }

        /// <summary>
        ///     Gets a boolean value indicating if the HDMI2.0a UHDA HDR with ST2084 EOTF (CEA861.3) is supported.
        /// </summary>
        public bool IsST2084EOTFSupported
        {
            get => _RawReserved.GetBit(0);
            set => _RawReserved = _RawReserved.SetBit(0, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the HDMI2.0a traditional HDR gamma (CEA861.3) is supported.
        /// </summary>
        public bool IsTraditionalHDRGammaSupported
        {
            get => _RawReserved.GetBit(1); 
            set => _RawReserved = _RawReserved.SetBit(1, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the Extended Dynamic Range on SDR displays is supported.
        /// </summary>
        public bool IsEDRSupported
        {
            get => _RawReserved.GetBit(2); 
            set => _RawReserved = _RawReserved.SetBit(2, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the default EDID HDR parameters is expanded;
        ///     otherwise false if this instance contains actual HDR parameters.
        /// </summary>
        public bool IsDriverDefaultHDRParametersExpanded
        {
            get => _RawReserved.GetBit(3); 
            set => _RawReserved = _RawReserved.SetBit(0, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the HDMI2.0a traditional SDR gamma is supported.
        /// </summary>
        public bool IsTraditionalSDRGammaSupported
        {
            get => _RawReserved.GetBit(4); 
            set => _RawReserved = _RawReserved.SetBit(4, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if Dolby Vision is supported.
        /// </summary>
        public bool IsDolbyVisionSupported
        {
            get => _RawReserved.GetBit(5); 
            set => _RawReserved = _RawReserved.SetBit(5, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if HDR10+ (Sink Side Tonemapping) is supported.
        /// </summary>
        public bool isHdr10PlusSupported
        {
            get => _RawReserved.GetBit(6); 
            set => _RawReserved = _RawReserved.SetBit(6, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if HDR10+ Gaming, a.k.a HDR10+ Source Side Tonemapping (SSTM), is supported.
        /// </summary>
        public bool isHdr10PlusGamingSupported
        {
            get => _RawReserved.GetBit(7); 
            set => _RawReserved = _RawReserved.SetBit(7, value);
        }
    }


    /// <inheritdoc cref="IHDRColorData" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct HDRColorDataV1 : IInitializable, IHDRColorData
    {
        internal StructureVersion _Version;
        private ColorDataHDRCommand _Command;
        private ColorDataHDRMode _HDRMode;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private StaticMetadataDescriptorId _StaticMetadataDescriptorId;
        private MasteringDisplayColorData _MasteringDisplayData;

        /// <summary>
        ///     Creates an instance of <see cref="HDRColorDataV1" />.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        /// <param name="hdrMode">The hdr mode.</param>
        /// <param name="masteringDisplayData">The display color space configurations.</param>
        public HDRColorDataV1(
            ColorDataHDRCommand command,
            ColorDataHDRMode hdrMode,
            MasteringDisplayColorData masteringDisplayData = default
        )
        {
            this = typeof(HDRColorDataV1).Instantiate<HDRColorDataV1>();

            if (command != ColorDataHDRCommand.Set)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = command;
            _HDRMode = hdrMode;
            _MasteringDisplayData = masteringDisplayData;
            _StaticMetadataDescriptorId = StaticMetadataDescriptorId.StaticMetadataType1;
        }


        /// <summary>
        ///     Creates an instance of <see cref="HDRColorDataV1" />.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public HDRColorDataV1(ColorDataHDRCommand command)
        {
            this = typeof(HDRColorDataV1).Instantiate<HDRColorDataV1>();

            if (command != ColorDataHDRCommand.Get)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = command;
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public ColorDataDepth? ColorDepth
        {
            get => null;
        }

        /// <inheritdoc />
        public ColorDataFormat? ColorFormat
        {
            get => null;
        }

        /// <summary>
        ///     Gets the color data command
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataHDRCommand Command
        {
            get => _Command;
            set => _Command = value;
        }

        /// <inheritdoc />
        public ColorDataDynamicRange? DynamicRange
        {
            get => null;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataHDRMode HDRMode
        {
            get => _HDRMode;
            set => _HDRMode = value;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public MasteringDisplayColorData MasteringDisplayData
        {
            get => _MasteringDisplayData;
            set => _MasteringDisplayData = value;
        }
    }

    /// <inheritdoc cref="IHDRColorData" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct HDRColorDataV2 : IInitializable, IHDRColorData
    {
        internal StructureVersion _Version;
        private ColorDataHDRCommand _Command;
        private ColorDataHDRMode _HDRMode;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private StaticMetadataDescriptorId _StaticMetadataDescriptorId;
        private MasteringDisplayColorData _MasteringDisplayData;
        private ColorDataFormat _ColorFormat;
        private ColorDataDynamicRange _DynamicRange;
        private ColorDataDepth _ColorDepth;

        /// <summary>
        ///     Creates an instance of <see cref="HDRColorDataV2" />.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        /// <param name="hdrMode">The hdr mode.</param>
        /// <param name="masteringDisplayData">The display color space configurations.</param>
        /// <param name="colorFormat">The color data color format.</param>
        /// <param name="dynamicRange">The color data dynamic range.</param>
        /// <param name="colorDepth">The color data color depth.</param>
        public HDRColorDataV2(
            ColorDataHDRCommand command,
            ColorDataHDRMode hdrMode,
            MasteringDisplayColorData masteringDisplayData = default,
            ColorDataFormat colorFormat = ColorDataFormat.Default,
            ColorDataDynamicRange dynamicRange = ColorDataDynamicRange.Auto,
            ColorDataDepth colorDepth = ColorDataDepth.Default
        )
        {
            this = typeof(HDRColorDataV2).Instantiate<HDRColorDataV2>();

            if (command != ColorDataHDRCommand.Set)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = command;
            _HDRMode = hdrMode;
            _MasteringDisplayData = masteringDisplayData;
            _ColorFormat = colorFormat;
            _DynamicRange = dynamicRange;
            _ColorDepth = colorDepth;
            _StaticMetadataDescriptorId = StaticMetadataDescriptorId.StaticMetadataType1;
        }

        /// <summary>
        ///     Creates an instance of <see cref="HDRColorDataV2" />.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public HDRColorDataV2(ColorDataHDRCommand command)
        {
            this = typeof(HDRColorDataV2).Instantiate<HDRColorDataV2>();

            if (command != ColorDataHDRCommand.Get)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = command;
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the color data command
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataHDRCommand Command
        {
            get => _Command;
            set => _Command = value;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataHDRMode HDRMode
        {
            get => _HDRMode;
            set => _HDRMode = value;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public MasteringDisplayColorData MasteringDisplayData
        {
            get => _MasteringDisplayData;
            set => _MasteringDisplayData = value;
        }

        /// <inheritdoc />
        public ColorDataFormat? ColorFormat
        {
            get => _ColorFormat;
            set => _ColorFormat = value.Value;
        }

        /// <inheritdoc />
        public ColorDataDynamicRange? DynamicRange
        {
            get => _DynamicRange;
            set => _DynamicRange = value.Value;
        }

        /// <inheritdoc />
        public ColorDataDepth? ColorDepth
        {
            get
            {
                switch ((uint)_ColorDepth)
                {
                    case 6:
                        return ColorDataDepth.BPC6;
                    case 8:
                        return ColorDataDepth.BPC8;
                    case 10:
                        return ColorDataDepth.BPC10;
                    case 12:
                        return ColorDataDepth.BPC12;
                    case 16:
                        return ColorDataDepth.BPC16;
                    default:
                        return _ColorDepth;
                }
            }
            set => _ColorDepth = value.Value;
        }
    }

    /// <summary>
    ///     Contains info-frame audio information
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 8)]
    public struct InfoFrameAudio
    {
        [FieldOffset(0)] private readonly uint _WordAt0;

        [FieldOffset(4)] private readonly uint _WordAt4;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        [FieldOffset(8)] private readonly uint _WordAt8;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        [FieldOffset(12)] private readonly byte _ByteAt12;

        /// <summary>
        ///     Creates an instance of <see cref="InfoFrameAudio" />.
        /// </summary>
        /// <param name="codec">The audio coding type (codec)</param>
        /// <param name="codecExtension">The audio codec from codec extension</param>
        /// <param name="sampleSize">The audio sample size (depth)</param>
        /// <param name="sampleRate">The audio sample rate (sampling frequency)</param>
        /// <param name="channelCount">The number of audio channels</param>
        /// <param name="channelAllocation">The audio channel allocation (speaker placements)</param>
        /// <param name="isDownMixProhibited">A value indicating if down-mix is prohibited</param>
        /// <param name="lfePlaybackLevel">The Low Frequency Effects playback level value</param>
        /// <param name="levelShift">The audio level shift value</param>
        public InfoFrameAudio(
            InfoFrameAudioCodec codec,
            InfoFrameAudioExtendedCodec codecExtension,
            InfoFrameAudioSampleSize sampleSize,
            InfoFrameAudioSampleRate sampleRate,
            InfoFrameAudioChannelCount channelCount,
            InfoFrameAudioChannelAllocation channelAllocation,
            InfoFrameBoolean isDownMixProhibited,
            InfoFrameAudioLFEPlaybackLevel lfePlaybackLevel,
            InfoFrameAudioLevelShift levelShift
        )
        {
            _WordAt0 = 0u
                .SetBits(0, 5, (uint)codec)
                .SetBits(5, 6, (uint)codecExtension)
                .SetBits(11, 3, (uint)sampleSize)
                .SetBits(14, 4, (uint)sampleRate)
                .SetBits(18, 4, (uint)channelCount)
                .SetBits(22, 9, (uint)channelAllocation);
            _WordAt4 = 0u
                .SetBits(0, 2, (uint)isDownMixProhibited)
                .SetBits(2, 3, (uint)lfePlaybackLevel)
                .SetBits(5, 5, (uint)levelShift);
            _WordAt8 = 0;
            _ByteAt12 = 0;
        }

        /// <summary>
        ///     Gets the audio coding type (codec)
        /// </summary>
        public InfoFrameAudioCodec Codec
        {
            get => (InfoFrameAudioCodec)_WordAt0.GetBits(0, 5);
        }

        /// <summary>
        ///     Gets the audio codec from codec extension; only valid when
        ///     <see cref="Codec" /> == <see cref="InfoFrameAudioCodec.UseExtendedCodecType" />
        /// </summary>
        public InfoFrameAudioExtendedCodec? ExtendedCodec
        {
            get
            {
                if (Codec != InfoFrameAudioCodec.UseExtendedCodecType)
                {
                    return null;
                }

                return (InfoFrameAudioExtendedCodec)_WordAt0.GetBits(5, 6);
            }
        }

        /// <summary>
        ///     Gets the audio sample size (depth)
        /// </summary>
        public InfoFrameAudioSampleSize SampleSize
        {
            get => (InfoFrameAudioSampleSize)_WordAt0.GetBits(11, 3);
        }

        /// <summary>
        ///     Gets the audio sample rate (sampling frequency)
        /// </summary>
        public InfoFrameAudioSampleRate SampleRate
        {
            get => (InfoFrameAudioSampleRate)_WordAt0.GetBits(14, 4);
        }

        /// <summary>
        ///     Gets the number of audio channels
        /// </summary>
        public InfoFrameAudioChannelCount ChannelCount
        {
            get => (InfoFrameAudioChannelCount)_WordAt0.GetBits(18, 4);
        }

        /// <summary>
        ///     Gets the audio channel allocation (speaker placements)
        /// </summary>
        public InfoFrameAudioChannelAllocation ChannelAllocation
        {
            get => (InfoFrameAudioChannelAllocation)_WordAt0.GetBits(22, 9);
        }

        /// <summary>
        ///     Gets a value indicating if down-mix is prohibited
        /// </summary>
        public InfoFrameBoolean IsDownMixProhibited
        {
            get => (InfoFrameBoolean)_WordAt4.GetBits(0, 2);
        }

        /// <summary>
        ///     Gets the Low Frequency Effects playback level value
        /// </summary>
        public InfoFrameAudioLFEPlaybackLevel LFEPlaybackLevel
        {
            get => (InfoFrameAudioLFEPlaybackLevel)_WordAt4.GetBits(2, 3);
        }

        /// <summary>
        ///     Gets the audio level shift value
        /// </summary>
        public InfoFrameAudioLevelShift LevelShift
        {
            get => (InfoFrameAudioLevelShift)_WordAt4.GetBits(5, 5);
        }
    }

    /// <summary>
    ///     Contains info-frame requested information or information to be overriden
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 8)]
    [StructureVersion(1)]
    public struct InfoFrameData : IInitializable
    {
        [FieldOffset(0)] internal StructureVersion _Version;

        [FieldOffset(4)]
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private ushort _Size;

        [FieldOffset(6)]
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private byte _Command;

        [FieldOffset(7)] private byte _Type;

        [FieldOffset(8)] private InfoFrameProperty _Property;
        [FieldOffset(8)] private InfoFrameAudio _Audio;
        [FieldOffset(8)] private InfoFrameVideo _Video;

        /// <summary>
        ///     Creates a new instance of <see cref="InfoFrameData" />.
        /// </summary>
        /// <param name="command">
        ///     The operation to be done. Can be used for information retrieval or to reset configurations to
        ///     default.
        /// </param>
        /// <param name="dataType">The type of information.</param>
        public InfoFrameData(InfoFrameCommand command, InfoFrameDataType dataType)
        {
            this = typeof(InfoFrameData).Instantiate<InfoFrameData>();
            _Size = (ushort)_Version.StructureSize;

            if (command != InfoFrameCommand.Get &&
                command != InfoFrameCommand.GetDefault &&
                command != InfoFrameCommand.GetOverride &&
                command != InfoFrameCommand.GetProperty &&
                command != InfoFrameCommand.Reset)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
            _Type = (byte)dataType;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="InfoFrameData" />.
        /// </summary>
        /// <param name="command">The operation to be done. Can only be used to change property information.</param>
        /// <param name="dataType">The type of information.</param>
        /// <param name="propertyInformation">The new property information to be set.</param>
        public InfoFrameData(
            InfoFrameCommand command,
            InfoFrameDataType dataType,
            InfoFrameProperty propertyInformation)
        {
            this = typeof(InfoFrameData).Instantiate<InfoFrameData>();
            _Size = (ushort)_Version.StructureSize;

            if (command != InfoFrameCommand.SetProperty)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
            _Type = (byte)dataType;
            _Property = propertyInformation;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="InfoFrameData" />.
        /// </summary>
        /// <param name="command">The operation to be done. Can only be used to change current or default audio information.</param>
        /// <param name="audioInformation">The new audio information to be set.</param>
        public InfoFrameData(InfoFrameCommand command, InfoFrameAudio audioInformation)
        {
            this = typeof(InfoFrameData).Instantiate<InfoFrameData>();
            _Size = (ushort)_Version.StructureSize;

            if (command != InfoFrameCommand.Set &&
                command != InfoFrameCommand.SetOverride)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
            _Type = (byte)InfoFrameDataType.AudioInformation;
            _Audio = audioInformation;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="InfoFrameData" />.
        /// </summary>
        /// <param name="command">The operation to be done. Can only be used to change current or default video information.</param>
        /// <param name="videoInformation">The new video information to be set.</param>
        public InfoFrameData(InfoFrameCommand command, InfoFrameVideo videoInformation)
        {
            this = typeof(InfoFrameData).Instantiate<InfoFrameData>();
            _Size = (ushort)_Version.StructureSize;

            if (command != InfoFrameCommand.Set &&
                command != InfoFrameCommand.SetOverride)
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _Command = (byte)command;
            _Type = (byte)InfoFrameDataType.AuxiliaryVideoInformation;
            _Video = videoInformation;
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the type of data contained in this instance
        /// </summary>
        public InfoFrameDataType Type
        {
            get => (InfoFrameDataType)_Type;
            set => _Type = (byte)value;
        }

        /// <summary>
        ///     Gets the operation type
        /// </summary>
        public InfoFrameCommand Command
        {
            get => (InfoFrameCommand)_Command;
            set => _Command = (byte)value;
        }

        /// <summary>
        ///     Gets the info-frame audio information if available; otherwise null
        /// </summary>
        public InfoFrameAudio? AudioInformation
        {
            get
            {
                if (Command == InfoFrameCommand.GetProperty || Command == InfoFrameCommand.SetProperty)
                {
                    return null;
                }

                if (Type == InfoFrameDataType.AudioInformation)
                {
                    return _Audio;
                }

                return null;
            }
        }

        /// <summary>
        ///     Gets the info-frame auxiliary video information (AVI) if available; otherwise null
        /// </summary>
        public InfoFrameVideo? AuxiliaryVideoInformation
        {
            get
            {
                if (Command == InfoFrameCommand.GetProperty || Command == InfoFrameCommand.SetProperty)
                {
                    return null;
                }

                if (Type == InfoFrameDataType.AuxiliaryVideoInformation)
                {
                    return _Video;
                }

                return null;
            }
        }

        /// <summary>
        ///     Gets the info-frame property information if available; otherwise null
        /// </summary>
        public InfoFrameProperty? PropertyInformation
        {
            get
            {
                if (Command != InfoFrameCommand.GetProperty && Command != InfoFrameCommand.SetProperty)
                {
                    return null;
                }

                return _Property;
            }
        }
    }

    /// <summary>
    ///     Contains info-frame property information
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 8)]
    public struct InfoFrameProperty
    {
        [FieldOffset(0)] private uint _Word;

        /// <summary>
        ///     Creates an instance of <see cref="InfoFrameProperty" />.
        /// </summary>
        /// <param name="mode">The info-frame operation mode</param>
        /// <param name="isBlackListed">A value indicating if this display (monitor) is blacklisted</param>
        public InfoFrameProperty(InfoFramePropertyMode mode, InfoFrameBoolean isBlackListed)
        {
            _Word = 0u
                .SetBits(0, 4, (uint)mode)
                .SetBits(4, 2, (uint)isBlackListed);
        }

        /// <summary>
        ///     Gets the info-frame operation mode
        /// </summary>
        public InfoFramePropertyMode Mode
        {
            get => (InfoFramePropertyMode)_Word.GetBits(0, 4);
        }

        /// <summary>
        ///     Gets a value indicating if this display (monitor) is blacklisted
        /// </summary>
        public InfoFrameBoolean IsBlackListed
        {
            get => (InfoFrameBoolean)_Word.GetBits(4, 2);
        }

        /// <summary>
        ///     Gets the info-frame version
        /// </summary>
        public byte Version
        {
            get => (byte)_Word.GetBits(16, 8);
        }

        /// <summary>
        ///     Gets the info-frame length
        /// </summary>
        public byte Length
        {
            get => (byte)_Word.GetBits(24, 8);
        }
    }

    /// <summary>
    ///     Contains info-frame video information
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 8)]
    public struct InfoFrameVideo
    {
        [FieldOffset(0)] private readonly uint _WordAt0;
        [FieldOffset(4)] private readonly uint _WordAt4;
        [FieldOffset(8)] private readonly uint _WordAt8;
        [FieldOffset(12)] private readonly uint _WordAt12;
        [FieldOffset(16)] private readonly uint _WordAt16;
        [FieldOffset(20)] private readonly uint _WordAt20;

        /// <summary>
        ///     Creates an instance of <see cref="InfoFrameVideo" />.
        /// </summary>
        /// <param name="videoIdentificationCode">The video identification code (VIC)</param>
        /// <param name="pixelRepetition">The video pixel repetition</param>
        /// <param name="colorFormat">The video color format</param>
        /// <param name="colorimetry">The video color space</param>
        /// <param name="extendedColorimetry">The extended video color space</param>
        /// <param name="rgbQuantization">The RGB quantization configuration</param>
        /// <param name="yccQuantization">The YCC quantization configuration</param>
        /// <param name="contentMode">The video content mode</param>
        /// <param name="contentType">The video content type</param>
        /// <param name="scanInfo">The video scan information</param>
        /// <param name="isActiveFormatInfoPresent">A value indicating if the active format information is present</param>
        /// <param name="activeFormatAspectRatio">The active format aspect ratio</param>
        /// <param name="pictureAspectRatio">The picture aspect ratio</param>
        /// <param name="nonUniformPictureScaling">The non uniform picture scaling direction</param>
        /// <param name="barInfo">The video bar information</param>
        /// <param name="topBar">The top bar value if not auto and present; otherwise null</param>
        /// <param name="bottomBar">The bottom bar value if not auto and present; otherwise null</param>
        /// <param name="leftBar">The left bar value if not auto and present; otherwise null</param>
        /// <param name="rightBar">The right bar value if not auto and present; otherwise null</param>
        public InfoFrameVideo(
            byte videoIdentificationCode,
            InfoFrameVideoPixelRepetition pixelRepetition,
            InfoFrameVideoColorFormat colorFormat,
            InfoFrameVideoColorimetry colorimetry,
            InfoFrameVideoExtendedColorimetry extendedColorimetry,
            InfoFrameVideoRGBQuantization rgbQuantization,
            InfoFrameVideoYCCQuantization yccQuantization,
            InfoFrameVideoITC contentMode,
            InfoFrameVideoContentType contentType,
            InfoFrameVideoScanInfo scanInfo,
            InfoFrameBoolean isActiveFormatInfoPresent,
            InfoFrameVideoAspectRatioActivePortion activeFormatAspectRatio,
            InfoFrameVideoAspectRatioCodedFrame pictureAspectRatio,
            InfoFrameVideoNonUniformPictureScaling nonUniformPictureScaling,
            InfoFrameVideoBarData barInfo,
            uint? topBar,
            uint? bottomBar,
            uint? leftBar,
            uint? rightBar
        )
        {
            _WordAt0 = 0u
                .SetBits(0, 8, videoIdentificationCode)
                .SetBits(8, 5, (uint)pixelRepetition)
                .SetBits(13, 3, (uint)colorFormat)
                .SetBits(16, 3, (uint)colorimetry)
                .SetBits(19, 4, (uint)extendedColorimetry)
                .SetBits(23, 3, (uint)rgbQuantization)
                .SetBits(26, 3, (uint)yccQuantization)
                .SetBits(29, 2, (uint)contentMode);

            _WordAt4 = 0u
                .SetBits(0, 3, (uint)contentType)
                .SetBits(3, 3, (uint)scanInfo)
                .SetBits(6, 2, (uint)isActiveFormatInfoPresent)
                .SetBits(8, 5, (uint)activeFormatAspectRatio)
                .SetBits(13, 3, (uint)pictureAspectRatio)
                .SetBits(16, 3, (uint)nonUniformPictureScaling)
                .SetBits(19, 3, (uint)barInfo);

            _WordAt8 = topBar == null ? 0x1FFFF : 0u.SetBits(0, 17, topBar.Value);
            _WordAt12 = bottomBar == null ? 0x1FFFF : 0u.SetBits(0, 17, bottomBar.Value);
            _WordAt16 = leftBar == null ? 0x1FFFF : 0u.SetBits(0, 17, leftBar.Value);
            _WordAt20 = rightBar == null ? 0x1FFFF : 0u.SetBits(0, 17, rightBar.Value);
        }

        /// <summary>
        ///     Gets the video identification code (VIC)
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public byte? VideoIdentificationCode
        {
            get
            {
                var value = (byte)_WordAt0.GetBits(0, 8);

                if (value == 0xFF)
                {
                    return null;
                }

                return value;
            }
        }

        /// <summary>
        ///     Gets the video pixel repetition
        /// </summary>
        public InfoFrameVideoPixelRepetition PixelRepetition
        {
            get => (InfoFrameVideoPixelRepetition)_WordAt0.GetBits(8, 5);
        }

        /// <summary>
        ///     Gets the video color format
        /// </summary>
        public InfoFrameVideoColorFormat ColorFormat
        {
            get => (InfoFrameVideoColorFormat)_WordAt0.GetBits(13, 3);
        }

        /// <summary>
        ///     Gets the video color space
        /// </summary>
        public InfoFrameVideoColorimetry Colorimetry
        {
            get => (InfoFrameVideoColorimetry)_WordAt0.GetBits(16, 3);
        }

        /// <summary>
        ///     Gets the extended video color space; only valid when <see cref="Colorimetry" /> ==
        ///     <see cref="InfoFrameVideoColorimetry.UseExtendedColorimetry" />
        /// </summary>
        public InfoFrameVideoExtendedColorimetry? ExtendedColorimetry
        {
            get
            {
                if (Colorimetry != InfoFrameVideoColorimetry.UseExtendedColorimetry)
                {
                    return null;
                }

                return (InfoFrameVideoExtendedColorimetry)_WordAt0.GetBits(19, 4);
            }
        }

        /// <summary>
        ///     Gets the RGB quantization configuration
        /// </summary>
        public InfoFrameVideoRGBQuantization RGBQuantization
        {
            get => (InfoFrameVideoRGBQuantization)_WordAt0.GetBits(23, 3);
        }

        /// <summary>
        ///     Gets the YCC quantization configuration
        /// </summary>
        public InfoFrameVideoYCCQuantization YCCQuantization
        {
            get => (InfoFrameVideoYCCQuantization)_WordAt0.GetBits(26, 3);
        }

        /// <summary>
        ///     Gets the video content mode
        /// </summary>
        public InfoFrameVideoITC ContentMode
        {
            get => (InfoFrameVideoITC)_WordAt0.GetBits(29, 2);
        }

        /// <summary>
        ///     Gets the video content type
        /// </summary>
        public InfoFrameVideoContentType ContentType
        {
            get => (InfoFrameVideoContentType)_WordAt4.GetBits(0, 3);
        }

        /// <summary>
        ///     Gets the video scan information
        /// </summary>
        public InfoFrameVideoScanInfo ScanInfo
        {
            get => (InfoFrameVideoScanInfo)_WordAt4.GetBits(3, 3);
        }

        /// <summary>
        ///     Gets a value indicating if the active format information is present
        /// </summary>
        public InfoFrameBoolean IsActiveFormatInfoPresent
        {
            get => (InfoFrameBoolean)_WordAt4.GetBits(6, 2);
        }

        /// <summary>
        ///     Gets the active format aspect ratio
        /// </summary>
        public InfoFrameVideoAspectRatioActivePortion ActiveFormatAspectRatio
        {
            get => (InfoFrameVideoAspectRatioActivePortion)_WordAt4.GetBits(8, 5);
        }

        /// <summary>
        ///     Gets the picture aspect ratio
        /// </summary>
        public InfoFrameVideoAspectRatioCodedFrame PictureAspectRatio
        {
            get => (InfoFrameVideoAspectRatioCodedFrame)_WordAt4.GetBits(13, 3);
        }

        /// <summary>
        ///     Gets the non uniform picture scaling direction
        /// </summary>
        public InfoFrameVideoNonUniformPictureScaling NonUniformPictureScaling
        {
            get => (InfoFrameVideoNonUniformPictureScaling)_WordAt4.GetBits(16, 3);
        }

        /// <summary>
        ///     Gets the video bar information
        /// </summary>
        public InfoFrameVideoBarData BarInfo
        {
            get => (InfoFrameVideoBarData)_WordAt4.GetBits(19, 3);
        }

        /// <summary>
        ///     Gets the top bar value if not auto and present; otherwise null
        /// </summary>
        public uint? TopBar
        {
            get
            {
                if (BarInfo == InfoFrameVideoBarData.NotPresent || BarInfo == InfoFrameVideoBarData.Horizontal)
                {
                    return null;
                }

                var val = _WordAt8.GetBits(0, 17);

                if (val == 0x1FFFF)
                {
                    return null;
                }

                return (uint)val;
            }
        }

        /// <summary>
        ///     Gets the bottom bar value if not auto and present; otherwise null
        /// </summary>
        public uint? BottomBar
        {
            get
            {
                if (BarInfo == InfoFrameVideoBarData.NotPresent || BarInfo == InfoFrameVideoBarData.Horizontal)
                {
                    return null;
                }

                var val = _WordAt12.GetBits(0, 17);

                if (val == 0x1FFFF)
                {
                    return null;
                }

                return (uint)val;
            }
        }

        /// <summary>
        ///     Gets the left bar value if not auto and present; otherwise null
        /// </summary>
        public uint? LeftBar
        {
            get
            {
                if (BarInfo == InfoFrameVideoBarData.NotPresent || BarInfo == InfoFrameVideoBarData.Vertical)
                {
                    return null;
                }

                var val = _WordAt16.GetBits(0, 17);

                if (val == 0x1FFFF)
                {
                    return null;
                }

                return (uint)val;
            }
        }

        /// <summary>
        ///     Gets the right bar value if not auto and present; otherwise null
        /// </summary>
        public uint? RightBar
        {
            get
            {
                if (BarInfo == InfoFrameVideoBarData.NotPresent || BarInfo == InfoFrameVideoBarData.Vertical)
                {
                    return null;
                }

                var val = _WordAt20.GetBits(0, 17);

                if (val == 0x1FFFF)
                {
                    return null;
                }

                return (uint)val;
            }
        }
    }

    /// <summary>
    ///     Locally unique identifier is a 64-bit value guaranteed to be unique only on the system on which it was generated.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct LUID : IEquatable<LUID>
    {
        /// <summary>
        ///     32Bit unsigned integer, low
        /// </summary>
        public uint LowPart;

        /// <summary>
        ///     32Bit signed integer, high
        /// </summary>
        public int HighPart;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{{{LowPart:X}-{HighPart:X}}}";
        }

        /// <inheritdoc />
        public bool Equals(LUID other)
        {
            return LowPart == other.LowPart && HighPart == other.HighPart;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is LUID luid && Equals(luid);
        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(LUID left, LUID right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(LUID left, LUID right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)LowPart * 397) ^ HighPart;
            }
        }

        public object Clone()
        {
            LUID other = (LUID)MemberwiseClone();
            return other;
        }
    }

    /// <inheritdoc />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct MasteringDisplayColorData : IDisplayColorData
    {
        private ColorDataColorCoordinate _FirstColorCoordinate;
        private ColorDataColorCoordinate _SecondColorCoordinate;
        private ColorDataColorCoordinate _ThirdColorCoordinate;
        private ColorDataColorCoordinate _WhiteColorCoordinate;
        private ushort _MaximumMasteringLuminance;
        private ushort _MinimumMasteringLuminance;
        private ushort _MaximumContentLightLevel;
        private ushort _MaximumFrameAverageLightLevel;

        /// <summary>
        ///     Creates an instance of <see cref="MasteringDisplayColorData" />.
        /// </summary>
        /// <param name="firstColorCoordinate">The first primary color coordinate.</param>
        /// <param name="secondColorCoordinate">The second primary color coordinate.</param>
        /// <param name="thirdColorCoordinate">The third primary color coordinate.</param>
        /// <param name="whiteColorCoordinate">The white color coordinate.</param>
        /// <param name="maximumMasteringLuminance">The maximum mastering display luminance [1.0-65535] in cd/m^2</param>
        /// <param name="minimumMasteringLuminance">The maximum mastering display luminance [1.0-6.5535] in cd/m^2</param>
        /// <param name="maximumContentLightLevel">
        ///     The maximum mastering display content light level (a.k.a MaxCLL) [1.0-65535] in
        ///     cd/m^2
        /// </param>
        /// <param name="maximumFrameAverageLightLevel">
        ///     The maximum mastering display frame average light level (a.k.a MaxFALL)
        ///     [1.0-65535] in cd/m^2
        /// </param>
        public MasteringDisplayColorData(
            ColorDataColorCoordinate firstColorCoordinate,
            ColorDataColorCoordinate secondColorCoordinate,
            ColorDataColorCoordinate thirdColorCoordinate,
            ColorDataColorCoordinate whiteColorCoordinate,
            float maximumMasteringLuminance,
            float minimumMasteringLuminance,
            float maximumContentLightLevel,
            float maximumFrameAverageLightLevel
        )
        {
            _FirstColorCoordinate = firstColorCoordinate;
            _SecondColorCoordinate = secondColorCoordinate;
            _ThirdColorCoordinate = thirdColorCoordinate;
            _WhiteColorCoordinate = whiteColorCoordinate;
            _MaximumMasteringLuminance = (ushort)Math.Max(Math.Min(maximumMasteringLuminance, uint.MaxValue), 1);
            _MinimumMasteringLuminance =
                (ushort)Math.Max(Math.Min(minimumMasteringLuminance * 10000, uint.MaxValue), 1);
            _MaximumContentLightLevel = (ushort)Math.Max(Math.Min(maximumContentLightLevel, uint.MaxValue), 1);
            _MaximumFrameAverageLightLevel =
                (ushort)Math.Max(Math.Min(maximumFrameAverageLightLevel, uint.MaxValue), 1);
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataColorCoordinate FirstColorCoordinate
        {
            get => _FirstColorCoordinate;
            set => _FirstColorCoordinate = value;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataColorCoordinate SecondColorCoordinate
        {
            get => _SecondColorCoordinate;
            set => _SecondColorCoordinate = value;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataColorCoordinate ThirdColorCoordinate
        {
            get => _ThirdColorCoordinate;
            set => _ThirdColorCoordinate = value;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ColorDataColorCoordinate WhiteColorCoordinate
        {
            get => _WhiteColorCoordinate;
            set => _WhiteColorCoordinate = value;
        }

        /// <summary>
        ///     Gets the maximum mastering display luminance [1.0-65535] in cd/m^2
        /// </summary>
        public float MaximumMasteringLuminance
        {
            get => _MaximumMasteringLuminance;
            set => _MaximumMasteringLuminance = (ushort)value;
        }

        /// <summary>
        ///     Gets the maximum mastering display frame average light level (a.k.a MaxFALL) [1.0-65535] in cd/m^2
        /// </summary>
        public float MaximumFrameAverageLightLevel
        {
            get => _MaximumFrameAverageLightLevel;
            set => _MaximumFrameAverageLightLevel = (ushort)value;
        }

        /// <summary>
        ///     Gets the maximum mastering display content light level (a.k.a MaxCLL) [1.0-65535] in cd/m^2
        /// </summary>
        public float MaximumContentLightLevel
        {
            get => _MaximumContentLightLevel;
            set => _MaximumContentLightLevel = (ushort)value;
        }

        /// <summary>
        ///     Gets the maximum mastering display luminance [1.0-6.5535] in cd/m^2
        /// </summary>
        public float MinimumMasteringLuminance
        {
            get => _MinimumMasteringLuminance / 10000f;
            set => _MaximumMasteringLuminance = (ushort)(value * 10000f);
        }
    }

    /// <summary>
    ///     Contains the monitor capabilities read from the Vendor Specific Data Block or the Video Capability Data Block
    /// </summary>
    [StructureVersion(1)]
    [StructLayout(LayoutKind.Explicit, Pack = 8)]
    public struct MonitorCapabilities : IInitializable
    {
        [FieldOffset(0)] internal StructureVersion _Version;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        [FieldOffset(4)] private ushort _Size;
        [FieldOffset(8)] private MonitorCapabilitiesType _Type;
        [FieldOffset(12)] private MonitorCapabilitiesConnectorType _ConnectorType;

        [FieldOffset(16)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        private readonly byte[] _Data;

        /// <summary>
        ///     Creates a new instance of <see cref="MonitorCapabilities" />.
        /// </summary>
        /// <param name="type">The type of information to be retrieved.</param>
        public MonitorCapabilities(MonitorCapabilitiesType type)
        {
            this = typeof(MonitorCapabilities).Instantiate<MonitorCapabilities>();
            _Size = (ushort)_Version.StructureSize;
            _Type = type;
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public ushort Size
        {
            get => _Size;
            set => _Size = value;
        }

        /// <summary>
        ///     Gets a boolean value indicating if this instance contains valid information
        /// </summary>
        public bool IsValid
        {
            get => _Data[0].GetBit(0);
            set => _Data[0] = _Data[0].SetBit(0,value);
        }

        /// <summary>
        ///     Gets the monitor capability type
        /// </summary>
        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        public MonitorCapabilitiesType Type
        {
            get => _Type;
        }

        /// <summary>
        ///     Gets the monitor connector type
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public MonitorCapabilitiesConnectorType ConnectorType
        {
            get => _ConnectorType;
        }

        /// <summary>
        ///     Gets the monitor VCDB capabilities information
        /// </summary>
        public MonitorVCDBCapabilities? VCDBCapabilities
        {
            get
            {
                if (IsValid && _Type == MonitorCapabilitiesType.VCDB)
                {
                    return new MonitorVCDBCapabilities(_Data.Skip(1).ToArray());
                }

                return null;
            }
        }

        /// <summary>
        ///     Gets the monitor VSDB capabilities information
        /// </summary>
        public MonitorVSDBCapabilities? VSDBCapabilities
        {
            get
            {
                if (IsValid && _Type == MonitorCapabilitiesType.VSDB)
                {
                    return new MonitorVSDBCapabilities(_Data.Skip(1).ToArray());
                }

                return null;
            }
        }
    }

    /// <summary>
    /// Contains information about a monitor color data
    /// </summary>
    [StructureVersion(1)]
    [StructLayout(LayoutKind.Explicit, Pack = 8, Size = 12)]
    public struct MonitorColorData : IInitializable
    {
        [FieldOffset(0)]
        internal StructureVersion _Version;
        [FieldOffset(4)]
        private DisplayPortColorFormat _ColorFormat;
        [FieldOffset(8)]
        private DisplayPortColorDepth _ColorDepth;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///Gets the monitor display port color format
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public DisplayPortColorFormat ColorFormat
        {
            get => _ColorFormat;
            set => _ColorFormat = value;
        }

        /// <summary>
        /// Gets the monitor display port color depth
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public DisplayPortColorDepth ColorDepth
        {
            get
            {
                switch ((uint)_ColorDepth)
                {
                    case 6:
                        return DisplayPortColorDepth.BPC6;
                    case 8:
                        return DisplayPortColorDepth.BPC8;
                    case 10:
                        return DisplayPortColorDepth.BPC10;
                    case 12:
                        return DisplayPortColorDepth.BPC12;
                    case 16:
                        return DisplayPortColorDepth.BPC16;
                    default:
                        return _ColorDepth;
                }
            }
            set => _ColorDepth = value;
        }
    }

    /// <summary>
    ///     Contains monitor VCDB capabilities
    /// </summary>
    public struct MonitorVCDBCapabilities
    {
        private byte[] _data;

        internal MonitorVCDBCapabilities(byte[] data)
        {
            if (data.Length != 49)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            _data = data;
        }

        /// <summary>
        ///     Gets a boolean value indicating RGB range quantization
        /// </summary>
        public bool QuantizationRangeRGB
        {
            get => _data[0].GetBit(1);
            set => _data[0] = _data [0].SetBit(1,value);
        }

        /// <summary>
        ///     Gets a boolean value indicating Ycc range quantization
        /// </summary>
        public bool QuantizationRangeYcc
        {
            get => _data[0].GetBit(0);
            set => _data[0] = _data[0].SetBit(0, value);
        }

        public byte ScanInfoConsumerElectronicsVideoFormats
        {
            get => (byte)_data[0].GetBits(6, 2);
            set => _data[0] = _data[0].SetBits(6,2, (ulong)value);
        }

        public byte ScanInfoInformationTechnologyVideoFormats
        {
            get => (byte)_data[0].GetBits(4, 2);
            set => _data[0] = _data[0].SetBits(4, 2, (ulong)value);
        }

        public byte ScanInfoPreferredVideoFormat
        {
            get => (byte)_data[0].GetBits(2, 2);
            set => _data[0] = _data[0].SetBits(2,2, (ulong)value);
        }
    }

    /// <summary>
    ///     Contains monitor VSDB capabilities
    /// </summary>
    public struct MonitorVSDBCapabilities
    {
        private byte[] _data;

        internal MonitorVSDBCapabilities(byte[] data)
        {
            if (data.Length != 49)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            _data = data;
        }

        /// <summary>
        ///     Gets the audio latency if available or null
        /// </summary>
        public byte? AudioLatency
        {
            get
            {
                if (!_data[4].GetBit(7))
                {
                    return null;
                }

                return _data[6];
            }
        }

        public byte[] HDMI3D
        {
            get
            {
                if (!_data[9].GetBit(7))
                {
                    return new byte[0];
                }

                return _data.Skip(18).Take(31).Take((int)_data[10].GetBits(0, 5)).ToArray();
            }
        }

        public byte[] HDMIVideoImageCompositors
        {
            get
            {
                if (!_data[4].GetBit(5))
                {
                    return new byte[0];
                }

                return _data.Skip(11).Take(7).Take((int)_data[10].GetBits(5, 3)).ToArray();
            }
        }

        /// <summary>
        ///     Gets the interlaced audio latency if available or null
        /// </summary>
        public byte? InterlacedAudioLatency
        {
            get
            {
                if (!_data[4].GetBit(6))
                {
                    return null;
                }

                return _data[8];
            }
        }

        /// <summary>
        ///     Gets the interlaced video latency if available or null
        /// </summary>
        public byte? InterlacedVideoLatency
        {
            get
            {
                if (!_data[4].GetBit(6))
                {
                    return null;
                }

                return _data[7];
            }
        }

        public bool IsAISupported
        {
            get => _data[2].GetBit(7);
            set => _data[2] = _data[2].SetBit(7, value);
        }

        /// <summary>
        ///     Returns a boolean value indicating if the cinematic content is supported by the monitor or the connection
        /// </summary>
        public bool IsCinemaContentSupported
        {
            get => _data[4].GetBit(2); 
            set => _data[4] = _data[4].SetBit(2, value);
        }

        /// <summary>
        ///     Returns a boolean value indicating if the 30bit deep color is supported by the monitor or the connection
        /// </summary>
        public bool IsDeepColor30BitsSupported
        {
            get => _data[2].GetBit(4); 
            set => _data[2] = _data[2].SetBit(4, value);
        }

        /// <summary>
        ///     Returns a boolean value indicating if the 36bit deep color is supported by the monitor or the connection
        /// </summary>
        public bool IsDeepColor36BitsSupported
        {
            get => _data[2].GetBit(5); 
            set => _data[2] = _data[2].SetBit(5, value);
        }

        /// <summary>
        ///     Returns a boolean value indicating if the 48bit deep color is supported by the monitor or the connection
        /// </summary>
        public bool IsDeepColor48BitsSupported
        {
            get => _data[2].GetBit(6); 
            set => _data[2] = _data[2].SetBit(6, value);
        }


        /// <summary>
        ///     Returns a boolean value indicating if the YCbCr444 deep color is supported by the monitor or the connection
        /// </summary>
        public bool IsDeepColorYCbCr444Supported
        {
            get => _data[2].GetBit(3); 
            set => _data[2] = _data[2].SetBit(3, value);
        }

        /// <summary>
        ///     Returns a boolean value indicating if the dual DVI operation is supported by the monitor or the connection
        /// </summary>
        public bool IsDualDVIOperationSupported
        {
            get => _data[2].GetBit(0); 
            set => _data[2] = _data[2].SetBit(0, value);
        }

        /// <summary>
        ///     Returns a boolean value indicating if the gaming content is supported by the monitor or the connection
        /// </summary>
        public bool IsGameContentSupported
        {
            get => _data[4].GetBit(3); 
            set => _data[4] = _data[4].SetBit(3, value);
        }

        /// <summary>
        ///     Returns a boolean value indicating if the graphics text content is supported by the monitor or the connection
        /// </summary>
        public bool IsGraphicsTextContentSupported
        {
            get => _data[4].GetBit(0); 
            set => _data[4] = _data[4].SetBit(0, value);
        }

        /// <summary>
        ///     Returns a boolean value indicating if the photo content is supported by monitor or the connection
        /// </summary>
        public bool IsPhotoContentSupported
        {
            get => _data[4].GetBit(1); 
            set => _data[4] = _data[4].SetBit(1, value);
        }

        /// <summary>
        ///     Gets the connection max TMDS clock supported by the monitor or the connection
        /// </summary>
        public byte MaxTMDSClock
        {
            get => _data[3];
            set => _data[3] = value;
        }

        /// <summary>
        ///     Gets the monitor physical address on port
        /// </summary>
        public MonitorPhysicalAddress PhysicalAddress
        {
            get => new MonitorPhysicalAddress(
                (byte)_data[0].GetBits(4, 4),
                (byte)_data[0].GetBits(0, 4),
                (byte)_data[1].GetBits(4, 4),
                (byte)_data[1].GetBits(0, 4)
            );
        }

        /// <summary>
        ///     Gets the video latency if available or null
        /// </summary>
        public byte? VideoLatency
        {
            get
            {
                if (!_data[4].GetBit(7))
                {
                    return null;
                }

                return _data[5];
            }
        }

        /// <summary>
        ///     Represents a monitor physical address
        /// </summary>
        public class MonitorPhysicalAddress
        {
            internal MonitorPhysicalAddress(byte a, byte b, byte c, byte d)
            {
                A = a;
                B = b;
                C = c;
                D = d;
            }

            /// <summary>
            ///     Gets the first part of a monitor physical address
            /// </summary>
            public byte A { get; set; }

            /// <summary>
            ///     Gets the second part of a monitor physical address
            /// </summary>
            public byte B { get; set; }


            /// <summary>
            ///     Gets the third part of a monitor physical address
            /// </summary>
            public byte C { get; set; }

            /// <summary>
            ///     Gets the forth part of a monitor physical address
            /// </summary>
            public byte D { get; set; }

            /// <inheritdoc />
            public override string ToString()
            {
                return $"{A:D}.{B:D}.{C:D}.{D:D}";
            }
        }
    }

    /// <summary>
    ///     Holds advanced information about a PathTargetInfo
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PathAdvancedTargetInfo : IInitializable, IEquatable<PathAdvancedTargetInfo>, ICloneable
    {
        internal StructureVersion _Version;
        internal Rotate _Rotation;
        internal Scaling _Scaling;
        internal uint _RefreshRateInMillihertz;
        internal uint _RawReserved;
        internal ConnectorType _ConnectorType;
        internal TVFormat _TVFormat;
        internal TimingOverride _TimingOverride;
        internal Timing _Timing;

        /// <summary>
        ///     Creates a new PathAdvancedTargetInfo for monitors
        /// </summary>
        /// <param name="rotation">Screen rotation</param>
        /// <param name="scale">Screen scaling</param>
        /// <param name="refreshRateInMillihertz">Screen refresh rate</param>
        /// <param name="timingOverride">Timing override</param>
        /// <param name="isInterlaced">Indicates if the mode is interlaced</param>
        /// <param name="isClonePrimary">Indicates if the display is the primary display of a clone topology</param>
        /// <param name="isClonePanAndScanTarget">Indicates if the target Pan and Scan is enabled</param>
        /// <param name="disableVirtualModeSupport"></param>
        /// <param name="isPreferredUnscaledTarget"></param>
        /// <exception cref="NVIDIANotSupportedException"></exception>
        public PathAdvancedTargetInfo(
            Rotate rotation,
            Scaling scale,
            uint refreshRateInMillihertz = 0,
            TimingOverride timingOverride = TimingOverride.Current,
            bool isInterlaced = false,
            bool isClonePrimary = false,
            bool isClonePanAndScanTarget = false,
            bool disableVirtualModeSupport = false,
            bool isPreferredUnscaledTarget = false)
        {
            if (timingOverride == TimingOverride.Custom)
            {
                throw new NVIDIANotSupportedException("Custom timing is not supported yet.");
            }

            this = typeof(PathAdvancedTargetInfo).Instantiate<PathAdvancedTargetInfo>();
            _Rotation = rotation;
            _Scaling = scale;
            _RefreshRateInMillihertz = refreshRateInMillihertz;
            _TimingOverride = timingOverride;
            IsInterlaced = isInterlaced;
            IsClonePrimary = isClonePrimary;
            IsClonePanAndScanTarget = isClonePanAndScanTarget;
            DisableVirtualModeSupport = disableVirtualModeSupport;
            IsPreferredUnscaledTarget = isPreferredUnscaledTarget;
        }

        /// <summary>
        ///     Creates a new PathAdvancedTargetInfo for TVs
        /// </summary>
        /// <param name="rotation">Screen rotation</param>
        /// <param name="scale">Screen scaling</param>
        /// <param name="tvFormat">The TV format to apply</param>
        /// <param name="connectorType">Specify connector type. For TV only</param>
        /// <param name="refreshRateInMillihertz">Screen refresh rate</param>
        /// <param name="timingOverride">Timing override</param>
        /// <param name="isInterlaced">Indicates if the mode is interlaced</param>
        /// <param name="isClonePrimary">Indicates if the display is the primary display of a clone topology</param>
        /// <param name="isClonePanAndScanTarget">Indicates if the target Pan and Scan is enabled</param>
        /// <param name="disableVirtualModeSupport"></param>
        /// <param name="isPreferredUnscaledTarget"></param>
        /// <exception cref="NVIDIANotSupportedException"></exception>
        public PathAdvancedTargetInfo(
            Rotate rotation,
            Scaling scale,
            TVFormat tvFormat,
            ConnectorType connectorType,
            uint refreshRateInMillihertz = 0,
            TimingOverride timingOverride = TimingOverride.Current,
            bool isInterlaced = false,
            bool isClonePrimary = false,
            bool isClonePanAndScanTarget = false,
            bool disableVirtualModeSupport = false,
            bool isPreferredUnscaledTarget = false)
            : this(
                rotation, scale, refreshRateInMillihertz, timingOverride, isInterlaced, isClonePrimary,
                isClonePanAndScanTarget,
                disableVirtualModeSupport, isPreferredUnscaledTarget)
        {
            if (tvFormat == TVFormat.None)
            {
                throw new NVIDIANotSupportedException(
                    "This overload is for TV displays, use the other overload(s) if the display is not a TV.");
            }

            this = typeof(PathAdvancedTargetInfo).Instantiate<PathAdvancedTargetInfo>();
            _TVFormat = tvFormat;
            _ConnectorType = connectorType;
        }

        /// <inheritdoc />
        public bool Equals(PathAdvancedTargetInfo other)
        {
            return _Rotation == other._Rotation &&
                   _Scaling == other._Scaling &&
                   _RefreshRateInMillihertz == other._RefreshRateInMillihertz &&
                   (TVFormat == TVFormat.None || _ConnectorType == other._ConnectorType) &&
                   _TVFormat == other._TVFormat &&
                   _TimingOverride == other._TimingOverride &&
                   _Timing.Equals(other._Timing) &&
                   _RawReserved == other._RawReserved;
        }

        public override bool Equals(object obj) => obj is PathAdvancedTargetInfo other && this.Equals(other);

        public override int GetHashCode()
        {
            return (_Rotation, _Scaling, _RefreshRateInMillihertz, _ConnectorType, _TVFormat, _TimingOverride, _Timing, _RawReserved).GetHashCode();
        }
        public static bool operator ==(PathAdvancedTargetInfo lhs, PathAdvancedTargetInfo rhs) => lhs.Equals(rhs);

        public static bool operator !=(PathAdvancedTargetInfo lhs, PathAdvancedTargetInfo rhs) => !(lhs == rhs);

        public object Clone()
        {
            PathAdvancedTargetInfo other = (PathAdvancedTargetInfo)MemberwiseClone();
            return other;
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Rotation setting
        /// </summary>
        public Rotate Rotation
        {
            get => _Rotation;
            set => _Rotation = value;
        }

        /// <summary>
        ///     Scaling setting
        /// </summary>
        public Scaling Scaling
        {
            get => _Scaling;
            set => _Scaling = value;
        }

        /// <summary>
        ///     Non-interlaced Refresh Rate of the mode, multiplied by 1000, 0 = ignored
        ///     This is the value which driver reports to the OS.
        /// </summary>
        public uint RefreshRateInMillihertz
        {
            get => _RefreshRateInMillihertz;
            set => _RefreshRateInMillihertz = value;
        }

        /// <summary>
        ///     Is the raw flags
        /// </summary>
        public uint RawReserved
        {
            get => _RawReserved;
            set => _RawReserved = value;
        }

        /// <summary>
        ///     Specify connector type. For TV only, ignored if TVFormat == TVFormat.None.
        /// </summary>
        public ConnectorType ConnectorType
        {
            get => _ConnectorType;
            set => _ConnectorType = value;
        }

        /// <summary>
        ///     To choose the last TV format set this value to TVFormat.None
        ///     In case of NvAPI_DISP_GetDisplayConfig(), this field will indicate the currently applied TV format;
        ///     if no TV format is applied, this field will have TVFormat.None value.
        ///     In case of NvAPI_DISP_SetDisplayConfig(), this field should only be set in case of TVs;
        ///     for other displays this field will be ignored and resolution &amp; refresh rate specified in input will be used to
        ///     apply the TV format.
        /// </summary>
        public TVFormat TVFormat
        {
            get => _TVFormat;
            set => _TVFormat = value;
        }

        /// <summary>
        ///     Ignored if TimingOverride == TimingOverride.Current
        /// </summary>
        public TimingOverride TimingOverride
        {
            get => _TimingOverride;
            set => _TimingOverride = value;
        }

        /// <summary>
        ///     Scan out timing, valid only if TimingOverride == TimingOverride.Custom
        ///     The value Timing.PixelClockIn10KHertz is obtained from the EDID. The driver may tweak this value for HDTV, stereo,
        ///     etc., before reporting it to the OS.
        /// </summary>
        public Timing Timing
        {
            get => _Timing;
            set => _Timing = value;
        }

        /// <summary>
        ///     Interlaced mode flag, ignored if refreshRate == 0
        /// </summary>
        public bool IsInterlaced
        {
            get => _RawReserved.GetBit(0);
            set => _RawReserved = _RawReserved.SetBit(0, value);
        }

        /// <summary>
        ///     Declares primary display in clone configuration. This is *NOT* GDI Primary.
        ///     Only one target can be primary per source. If no primary is specified, the first target will automatically be
        ///     primary.
        /// </summary>
        public bool IsClonePrimary
        {
            get => _RawReserved.GetBit(1);
            set => _RawReserved = _RawReserved.SetBit(1, value);
        }

        /// <summary>
        ///     Whether on this target Pan and Scan is enabled or has to be enabled. Valid only when the target is part of clone
        ///     topology.
        /// </summary>
        public bool IsClonePanAndScanTarget
        {
            get => _RawReserved.GetBit(2);
            set => _RawReserved = _RawReserved.SetBit(2, value);
        }

        /// <summary>
        ///     Indicates if virtual mode support is disabled
        /// </summary>
        public bool DisableVirtualModeSupport
        {
            get => _RawReserved.GetBit(3);
            set => _RawReserved = _RawReserved.SetBit(3, value);
        }

        /// <summary>
        ///     Indicates if the target is in preferred unscaled mode
        /// </summary>
        public bool IsPreferredUnscaledTarget
        {
            get => _RawReserved.GetBit(4);
            set => _RawReserved = _RawReserved.SetBit(4, value);
        }
    }

    /// <summary>
    ///     Holds information about a path
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    // ReSharper disable once RedundantExtendsListEntry
    public struct PathInfoV1 : IPathInfo, IInitializable, IAllocatable, IEquatable<PathInfoV1>, ICloneable
    {
        internal StructureVersion _Version;
        internal uint _ReservedSourceId;
        internal uint _TargetInfoCount;
        internal ValueTypeArray<PathTargetInfoV1> _TargetsInfo;
        internal ValueTypeReference<SourceModeInfo> _SourceModeInfo;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint SourceId
        {
            get => _ReservedSourceId;
            set => _ReservedSourceId = value;
        }

        /// <inheritdoc />
        public List<IPathTargetInfo> TargetsInfo
        {
            get => _TargetsInfo.ToList((int)_TargetInfoCount).Cast<IPathTargetInfo>().ToList();
            //get => _TargetsInfo.ToList((int) _TargetInfoCount).Cast<IPathTargetInfo>().ToList();
            set => _TargetsInfo = ValueTypeArray<PathTargetInfoV1>.FromArray(value.Cast<PathTargetInfoV1>().ToArray());
        }

        /// <inheritdoc />
        public SourceModeInfo SourceModeInfo
        {
            get => _SourceModeInfo.ToValueType() ?? default(SourceModeInfo);
            set => _SourceModeInfo = ValueTypeReference<SourceModeInfo>.FromValueType(value, value.GetType());
        }

        /// <summary>
        ///     Creates a new PathInfoV1
        /// </summary>
        /// <param name="targetsInformation">Information about path targets</param>
        /// <param name="sourceModeInformation">Source mode information</param>
        /// <param name="sourceId">Source Id, can be zero</param>
        public PathInfoV1(
            PathTargetInfoV1[] targetsInformation,
            SourceModeInfo sourceModeInformation,
            uint sourceId = 0)
        {
            this = typeof(PathInfoV1).Instantiate<PathInfoV1>();
            _TargetInfoCount = (uint)targetsInformation.Length;
            _TargetsInfo = ValueTypeArray<PathTargetInfoV1>.FromArray(targetsInformation);
            _SourceModeInfo = ValueTypeReference<SourceModeInfo>.FromValueType(sourceModeInformation);
            _ReservedSourceId = sourceId;
        }

        /// <inheritdoc />
        public bool Equals(PathInfoV1 other)
        {
            return _TargetInfoCount == other._TargetInfoCount &&
                   _TargetsInfo.Equals(other._TargetsInfo) &&
                   _SourceModeInfo.Equals(other._SourceModeInfo);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is PathInfoV1 && Equals((PathInfoV1)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_TargetInfoCount;
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                hashCode = (hashCode * 397) ^ _TargetsInfo.GetHashCode();
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                hashCode = (hashCode * 397) ^ _SourceModeInfo.GetHashCode();

                return hashCode;
            }
        }

        public object Clone()
        {
            PathInfoV1 other = (PathInfoV1)MemberwiseClone();
            return other;
        }

        /// <summary>
        ///     Creates a new PathInfoV1
        /// </summary>
        /// <param name="targetsInformation">Information about path targets</param>
        /// <param name="sourceId">Source Id, can be zero</param>
        public PathInfoV1(PathTargetInfoV1[] targetsInformation, uint sourceId = 0)
        {
            this = typeof(PathInfoV1).Instantiate<PathInfoV1>();
            _TargetInfoCount = (uint)targetsInformation.Length;
            _TargetsInfo = ValueTypeArray<PathTargetInfoV1>.FromArray(targetsInformation);
            _SourceModeInfo = ValueTypeReference<SourceModeInfo>.Null;
            _ReservedSourceId = sourceId;
        }

        /// <summary>
        ///     Creates a new PathInfoV1
        /// </summary>
        /// <param name="sourceId">Source Id, can be zero</param>
        public PathInfoV1(uint sourceId)
        {
            this = typeof(PathInfoV1).Instantiate<PathInfoV1>();
            _TargetInfoCount = 0;
            _TargetsInfo = ValueTypeArray<PathTargetInfoV1>.Null;
            _SourceModeInfo = ValueTypeReference<SourceModeInfo>.Null;
            _ReservedSourceId = sourceId;
        }

        /// <summary>
        ///     Creates a new PathInfoV1
        /// </summary>
        /// <param name="sourceModeInfo">Source mode information</param>
        /// <param name="sourceId">Source Id, can be zero</param>
        public PathInfoV1(SourceModeInfo sourceModeInfo, uint sourceId)
        {
            this = typeof(PathInfoV1).Instantiate<PathInfoV1>();
            _TargetInfoCount = 0;
            _TargetsInfo = ValueTypeArray<PathTargetInfoV1>.Null;
            _SourceModeInfo = ValueTypeReference<SourceModeInfo>.FromValueType(sourceModeInfo);
            _ReservedSourceId = sourceId;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            TargetsInfo.DisposeAll();
            _TargetsInfo.Dispose();
            _SourceModeInfo.Dispose();
        }

        void IAllocatable.Allocate()
        {
            if (_TargetInfoCount > 0 && _TargetsInfo.IsNull)
            {
                var targetInfo = typeof(PathTargetInfoV1).Instantiate<PathTargetInfoV1>();
                var targetInfoList = targetInfo.Repeat((int)_TargetInfoCount).AllocateAll();
                _TargetsInfo = ValueTypeArray<PathTargetInfoV1>.FromArray(targetInfoList.ToArray());
            }

            if (_SourceModeInfo.IsNull)
            {
                var sourceModeInfo = typeof(SourceModeInfo).Instantiate<SourceModeInfo>();
                _SourceModeInfo = ValueTypeReference<SourceModeInfo>.FromValueType(sourceModeInfo);
            }
        }
    }

    /// <summary>
    ///     Holds information about a path
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    // ReSharper disable once RedundantExtendsListEntry
    public struct PathInfoV2 : IInitializable, IAllocatable, IEquatable<PathInfoV2>
    {
        internal StructureVersion _Version;
        internal uint _SourceId;
        internal uint _TargetInfoCount;
        internal ValueTypeArray<PathTargetInfoV2> _TargetsInfo;
        internal ValueTypeReference<SourceModeInfo> _SourceModeInfo;
        internal uint _RawReserved;
        internal ValueTypeReference<LUID> _OSAdapterLUID;
        //internal IntPtr _OSAdapterLUID;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint SourceId
        {
            get => _SourceId;
            set => _SourceId = value;
        }

        /// <inheritdoc />
        public uint TargetInfoCount
        {
            get => _TargetInfoCount;
            set => _TargetInfoCount = value;
        }

        /*/// <inheritdoc />
        public bool Equals(PathInfoV2 other)
        {
            return _TargetInfoCount == other._TargetInfoCount &&
                   _TargetsInfo.Equals(other._TargetsInfo) &&
                   _SourceModeInfo.Equals(other._SourceModeInfo) &&
                   _RawReserved == other._RawReserved;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is PathInfoV2 v2 && Equals(v2);
        }*/

        public override bool Equals(object obj) => obj is PathInfoV2 other && this.Equals(other);
        public bool Equals(PathInfoV2 other)
        => _Version == other._Version &&
            _SourceId == other._SourceId &&
            _TargetInfoCount == other._TargetInfoCount &&
            TargetsInfo.SequenceEqual(other.TargetsInfo) &&
            SourceModeInfo.Equals(other.SourceModeInfo) &&
            _RawReserved == other._RawReserved &&
            OSAdapterLUID.ToString().Equals(other.OSAdapterLUID.ToString());

        public override int GetHashCode()
        {
            return (_Version, _SourceId , _TargetInfoCount, TargetsInfo, SourceModeInfo, _RawReserved, OSAdapterLUID).GetHashCode();
        }
        public static bool operator ==(PathInfoV2 lhs, PathInfoV2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(PathInfoV2 lhs, PathInfoV2 rhs) => !(lhs == rhs);

        public object Clone()
        {
            PathInfoV2 other = (PathInfoV2)MemberwiseClone();
            return other;
        }

        /// <inheritdoc />
        public List<PathTargetInfoV2> TargetsInfo
        {
            //get => _TargetsInfo.ToArray((int)_TargetInfoCount)?.Cast<IPathTargetInfo>().ToArray() ?? new IPathTargetInfo[0];
            //get => _TargetsInfo.ToArray((int)_TargetInfoCount).Cast<IPathTargetInfo>().ToList();
            get => _TargetsInfo.ToArray((int)_TargetInfoCount).ToList();
            //set => _TargetsInfo = ValueTypeArray<PathTargetInfoV2>.FromList(value.Cast<PathTargetInfoV2>().ToList());
            //set => _TargetsInfo = ValueTypeArray<PathTargetInfoV2>.FromArray(value.Cast<PathTargetInfoV2>().ToArray());
            set
            {
                if (value == null || value.Count == 0)
                {
                    _TargetsInfo = ValueTypeArray<PathTargetInfoV2>.Null;
                    return;
                }
                PathTargetInfoV2[] mylist = value.ToArray().Cast<PathTargetInfoV2>().ToArray();
                //PathTargetInfoV2[] mylist = value.Cast<PathTargetInfoV2>().ToArray();
                _TargetsInfo = ValueTypeArray<PathTargetInfoV2>.FromArray(mylist);
            }
        }

        /// <inheritdoc />
        public SourceModeInfo SourceModeInfo
        {
            get => _SourceModeInfo.ToValueType() ?? default(SourceModeInfo);
            set => _SourceModeInfo = ValueTypeReference<SourceModeInfo>.FromValueType(value, value.GetType());
        }

        /// <summary>
        ///     True for non-NVIDIA adapter.
        /// </summary>
        public bool IsNonNVIDIAAdapter
        {
            get => _RawReserved.GetBit(0);
            set => _RawReserved = _RawReserved.SetBit(0, value);
        }

        /// <summary>
        ///     Used by Non-NVIDIA adapter for OS Adapter of LUID
        /// </summary>
        public LUID OSAdapterLUID
        {
            get 
            {
                if (_OSAdapterLUID.ToValueType() == null)
                {
                    return default(LUID);
                }
                return _OSAdapterLUID.ToValueType().Value;
            }
            set
            {
                if (value != null)
                {
                    _OSAdapterLUID = ValueTypeReference<LUID>.FromValueType(value, value.GetType());
                }
            }
                
        }

        /// <summary>
        ///     Creates a new PathInfoV2
        /// </summary>
        /// <param name="targetInformations">Information about path targets</param>
        /// <param name="sourceModeInfo">Source mode information</param>
        /// <param name="sourceId">Source Id, can be zero</param>
        public PathInfoV2(PathTargetInfoV2[] targetInformations, SourceModeInfo sourceModeInfo, uint sourceId = 0)
        {
            this = typeof(PathInfoV2).Instantiate<PathInfoV2>();
            _TargetInfoCount = (uint)targetInformations.Length;
            _TargetsInfo = ValueTypeArray<PathTargetInfoV2>.FromArray(targetInformations);
            _SourceModeInfo = ValueTypeReference<SourceModeInfo>.FromValueType(sourceModeInfo);
            _SourceId = sourceId;
        }

        /// <summary>
        ///     Creates a new PathInfoV2
        /// </summary>
        /// <param name="targetInformations">Information about path targets</param>
        /// <param name="sourceId">Source Id, can be zero</param>
        public PathInfoV2(PathTargetInfoV2[] targetInformations, uint sourceId = 0)
        {
            this = typeof(PathInfoV2).Instantiate<PathInfoV2>();
            _TargetInfoCount = (uint)targetInformations.Length;
            _TargetsInfo = ValueTypeArray<PathTargetInfoV2>.FromArray(targetInformations);
            _SourceModeInfo = ValueTypeReference<SourceModeInfo>.Null;
            _SourceId = sourceId;
        }


        /// <summary>
        ///     Creates a new PathInfoV2
        /// </summary>
        /// <param name="sourceId">Source Id, can be zero</param>
        public PathInfoV2(uint sourceId)
        {
            this = typeof(PathInfoV2).Instantiate<PathInfoV2>();
            _TargetInfoCount = 0;
            _TargetsInfo = ValueTypeArray<PathTargetInfoV2>.Null;
            _SourceModeInfo = ValueTypeReference<SourceModeInfo>.Null;
            _SourceId = sourceId;
        }

        /// <summary>
        ///     Creates a new PathInfoV2
        /// </summary>
        /// <param name="sourceModeInfo">Source mode information</param>
        /// <param name="sourceId">Source Id, can be zero</param>
        public PathInfoV2(SourceModeInfo sourceModeInfo, uint sourceId)
        {
            this = typeof(PathInfoV2).Instantiate<PathInfoV2>();
            _TargetInfoCount = 0;
            _TargetsInfo = ValueTypeArray<PathTargetInfoV2>.Null;
            _SourceModeInfo = ValueTypeReference<SourceModeInfo>.FromValueType(sourceModeInfo);
            _SourceId = sourceId;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            TargetsInfo.DisposeAll();
            _TargetsInfo.Dispose();
            _SourceModeInfo.Dispose();
        }

        void IAllocatable.Allocate()
        {
            if (_TargetInfoCount > 0 && _TargetsInfo.IsNull)
            {
                var targetInfo = typeof(PathTargetInfoV2).Instantiate<PathTargetInfoV2>();
                var targetInfoList = targetInfo.Repeat((int)_TargetInfoCount).AllocateAll();
                _TargetsInfo = ValueTypeArray<PathTargetInfoV2>.FromArray(targetInfoList.ToArray());
            }

            if (_SourceModeInfo.IsNull)
            {
                var sourceModeInfo = typeof(SourceModeInfo).Instantiate<SourceModeInfo>();
                _SourceModeInfo = ValueTypeReference<SourceModeInfo>.FromValueType(sourceModeInfo);
            }
        }
    }

    /// <summary>
    ///     Holds information about a path's target
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct PathTargetInfoV1 : IPathTargetInfo,
        IInitializable,
        IDisposable,
        IAllocatable,
        IEquatable<PathTargetInfoV1>,
        IEquatable<PathTargetInfoV2>
    {
        internal uint _DisplayId;
        internal ValueTypeReference<PathAdvancedTargetInfo> _Details;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"PathTargetInfoV2: Display #{_DisplayId}";
        }

        /// <inheritdoc />
        public uint DisplayId
        {
            get => _DisplayId;
            set => _DisplayId = value;
        }

        /// <inheritdoc />
        public bool Equals(PathTargetInfoV1 other)
        {
            return _DisplayId == other._DisplayId && _Details.Equals(other._Details);
        }

        /// <inheritdoc />
        public bool Equals(PathTargetInfoV2 other)
        {
            return _DisplayId == other._DisplayId && _Details.Equals(other._Details);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is PathTargetInfoV1 && Equals((PathTargetInfoV1)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return ((int)_DisplayId * 397) ^ _Details.GetHashCode();
            }
        }

        public object Clone()
        {
            PathTargetInfoV1 other = (PathTargetInfoV1)MemberwiseClone();
            return other;
        }

        /// <inheritdoc />
        public PathAdvancedTargetInfo? Details
        {
            get => _Details.ToValueType() ?? default(PathAdvancedTargetInfo);
            set => _Details = ValueTypeReference<PathAdvancedTargetInfo>.FromValueType(value, value.GetType());
        }

        /// <summary>
        ///     Creates a new PathTargetInfoV1
        /// </summary>
        /// <param name="displayId">Display Id</param>
        public PathTargetInfoV1(uint displayId) : this()
        {
            _DisplayId = displayId;
        }

        /// <summary>
        ///     Creates a new PathTargetInfoV1
        /// </summary>
        /// <param name="displayId">Display Id</param>
        /// <param name="details">Extra information</param>
        public PathTargetInfoV1(uint displayId, PathAdvancedTargetInfo details) : this(displayId)
        {
            _Details = ValueTypeReference<PathAdvancedTargetInfo>.FromValueType(details);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _Details.Dispose();
        }

        void IAllocatable.Allocate()
        {
            if (_Details.IsNull)
            {
                var detail = typeof(PathAdvancedTargetInfo).Instantiate<PathAdvancedTargetInfo>();
                _Details = ValueTypeReference<PathAdvancedTargetInfo>.FromValueType(detail);
            }
        }
    }

    /// <summary>
    ///     Holds information about a path's target
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct PathTargetInfoV2 : IPathTargetInfo,
        IInitializable,
        IDisposable,
        IAllocatable,
        IEquatable<PathTargetInfoV2>,
        ICloneable
    {
        internal uint _DisplayId;
        internal ValueTypeReference<PathAdvancedTargetInfo> _Details;
        internal uint _WindowsCCDTargetId;

        /// <inheritdoc />
        public uint DisplayId
        {
            get => _DisplayId;
            set => _DisplayId = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"PathTargetInfoV2: Display #{_DisplayId}";
        }

        /// <inheritdoc />
        public PathAdvancedTargetInfo? Details
        {
            get => _Details.ToValueType();
            set => _Details = ValueTypeReference<PathAdvancedTargetInfo>.FromValueType(value, value.GetType());
        }

        /// <summary>
        ///     Windows CCD target ID. Must be present only for non-NVIDIA adapter, for NVIDIA adapter this parameter is ignored.
        /// </summary>
        public uint WindowsCCDTargetId
        {
            get => _WindowsCCDTargetId;
            set => _WindowsCCDTargetId = value;
        }

        /// <summary>
        ///     Creates a new PathTargetInfoV1
        /// </summary>
        /// <param name="displayId">Display Id</param>
        public PathTargetInfoV2(uint displayId) : this()
        {
            _DisplayId = displayId;
        }

        public override bool Equals(object obj) => obj is PathTargetInfoV2 other && this.Equals(other);
        public bool Equals(PathTargetInfoV2 other)
        => _DisplayId == other._DisplayId &&
            Details == other.Details &&
            _WindowsCCDTargetId == other._WindowsCCDTargetId;

        public override int GetHashCode()
        {
            return (_DisplayId, Details, _WindowsCCDTargetId).GetHashCode();
        }
        public static bool operator ==(PathTargetInfoV2 lhs, PathTargetInfoV2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(PathTargetInfoV2 lhs, PathTargetInfoV2 rhs) => !(lhs == rhs);

        public object Clone()
        {
            PathTargetInfoV2 other = (PathTargetInfoV2)MemberwiseClone();
            return other;
        }

        /// <summary>
        ///     Creates a new PathTargetInfoV1
        /// </summary>
        /// <param name="displayId">Display Id</param>
        /// <param name="windowsCCDTargetId">Windows CCD target Id</param>
        public PathTargetInfoV2(uint displayId, uint windowsCCDTargetId) : this(displayId)
        {
            _WindowsCCDTargetId = windowsCCDTargetId;
        }

        /// <summary>
        ///     Creates a new PathTargetInfoV1
        /// </summary>
        /// <param name="displayId">Display Id</param>
        /// <param name="details">Extra information</param>
        public PathTargetInfoV2(uint displayId, PathAdvancedTargetInfo details) : this(displayId)
        {
            _Details = ValueTypeReference<PathAdvancedTargetInfo>.FromValueType(details);
        }

        /// <summary>
        ///     Creates a new PathTargetInfoV1
        /// </summary>
        /// <param name="displayId">Display Id</param>
        /// <param name="windowsCCDTargetId">Windows CCD target Id</param>
        /// <param name="details">Extra information</param>
        public PathTargetInfoV2(uint displayId, uint windowsCCDTargetId, PathAdvancedTargetInfo details)
            : this(displayId, windowsCCDTargetId)
        {
            _Details = ValueTypeReference<PathAdvancedTargetInfo>.FromValueType(details);
        }


        /// <inheritdoc />
        public void Dispose()
        {
            _Details.Dispose();
        }

        void IAllocatable.Allocate()
        {
            if (_Details.IsNull)
            {
                var detail = typeof(PathAdvancedTargetInfo).Instantiate<PathAdvancedTargetInfo>();
                _Details = ValueTypeReference<PathAdvancedTargetInfo>.FromValueType(detail);
            }
        }
    }

    /// <summary>
    ///     Holds a [X,Y] pair as a position on a 2D plane
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct Position : IEquatable<Position>, ICloneable
    {
        internal int _X;
        internal int _Y;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{X}, {Y}]";
        }

        /// <inheritdoc />
        public bool Equals(Position other)
        {
            return _X == other._X && _Y == other._Y;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Position position && Equals(position);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (_X * 397) ^ _Y;
            }
        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(Position left, Position right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(Position left, Position right)
        {
            return !left.Equals(right);
        }

        public object Clone()
        {
            Position other = (Position)MemberwiseClone();
            return other;
        }

        /// <summary>
        ///     Creates a new Position
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        public Position(int x, int y)
        {
            _X = x;
            _Y = y;
        }

        /// <summary>
        ///     X value
        /// </summary>
        public int X
        {
            get => _X;
        }

        /// <summary>
        ///     Y value
        /// </summary>
        public int Y
        {
            get => _Y;
        }
    }

    /// <inheritdoc cref="IDisplayDVCInfo" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateDisplayDVCInfo : IInitializable, IDisplayDVCInfo
    {
        internal StructureVersion _Version;
        internal int _CurrentLevel;
        internal int _MinimumLevel;
        internal int _MaximumLevel;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public int CurrentLevel
        {
            get => _CurrentLevel;
        }

        /// <inheritdoc />
        public int MinimumLevel
        {
            get => _MinimumLevel;
        }

        /// <inheritdoc />
        int IDisplayDVCInfo.DefaultLevel
        {
            get => 0;
        }

        /// <inheritdoc />
        public int MaximumLevel
        {
            get => _MaximumLevel;
        }
    }

    /// <inheritdoc cref="IDisplayDVCInfo" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateDisplayDVCInfoEx : IInitializable, IDisplayDVCInfo
    {
        internal StructureVersion _Version;
        internal int _CurrentLevel;
        internal int _MinimumLevel;
        internal int _MaximumLevel;
        internal int _DefaultLevel;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }
        
        /// <inheritdoc />
        public int CurrentLevel
        {
            get => _CurrentLevel;
        }

        /// <inheritdoc />
        public int MinimumLevel
        {
            get => _MinimumLevel;
        }

        /// <inheritdoc />
        public int MaximumLevel
        {
            get => _MaximumLevel;
        }

        /// <inheritdoc />
        public int DefaultLevel
        {
            get => _DefaultLevel;
        }

        internal PrivateDisplayDVCInfoEx(int currentLevel)
        {
            this = typeof(PrivateDisplayDVCInfoEx).Instantiate<PrivateDisplayDVCInfoEx>();
            _CurrentLevel = currentLevel;
        }
    }

    /// <summary>
    ///     Holds the current and the default HUE information
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateDisplayHUEInfo : IInitializable
    {
        internal StructureVersion _Version;
        internal int _CurrentAngle;
        internal int _DefaultAngle;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }
        
        /// <summary>
         ///     Gets or sets the current HUE offset angle [0-359]
         /// </summary>
        public int CurrentAngle
        {
            get => _CurrentAngle;
        }

        /// <summary>
        ///     Gets or sets the default HUE offset angle [0-359]
        /// </summary>
        public int DefaultAngle
        {
            get => _DefaultAngle;
        }
    }

    /// <summary>
    ///     Holds a [Width, Height] pair as the resolution of a display device, as well as a color format
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct Resolution : IEquatable<Resolution>, ICloneable
    {
        internal uint _Width;
        internal uint _Height;
        internal uint _ColorDepth;

        /// <summary>
        ///     Creates a new Resolution
        /// </summary>
        /// <param name="width">Display resolution width</param>
        /// <param name="height">Display resolution height</param>
        /// <param name="colorDepth">Display color depth</param>
        public Resolution(int width, int height, int colorDepth)
        {
            _Width = (uint)width;
            _Height = (uint)height;
            _ColorDepth = (uint)colorDepth;
        }

        /// <inheritdoc />
        public bool Equals(Resolution other)
        {
            return _Width == other._Width && _Height == other._Height && _ColorDepth == other._ColorDepth;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Resolution resolution && Equals(resolution);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_Width;
                hashCode = (hashCode * 397) ^ (int)_Height;
                hashCode = (hashCode * 397) ^ (int)_ColorDepth;

                return hashCode;
            }


        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(Resolution left, Resolution right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(Resolution left, Resolution right)
        {
            return !left.Equals(right);
        }

        public object Clone()
        {
            Resolution other = (Resolution)MemberwiseClone();
            return other;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Width}, {Height}) @ {ColorDepth}bpp";
        }

        /// <summary>
        ///     Display resolution width
        /// </summary>
        public int Width
        {
            get => (int)_Width;
        }

        /// <summary>
        ///     Display resolution height
        /// </summary>
        public int Height
        {
            get => (int)_Height;
        }

        /// <summary>
        ///     Display color depth
        /// </summary>
        public int ColorDepth
        {
            get => (int)_ColorDepth;
        }
    }

    /// <summary>
    ///     Contains information regarding the scan-out configurations
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct ScanOutInformationV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal Rectangle _SourceDesktopRectangle;
        internal Rectangle _SourceViewPortRectangle;
        internal Rectangle _TargetViewPortRectangle;
        internal uint _TargetDisplayWidth;
        internal uint _TargetDisplayHeight;
        internal uint _CloneImportance;
        internal Rotate _SourceToTargetRotation;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the operating system display device rectangle in desktop coordinates displayId is scanning out from.
        /// </summary>
        public Rectangle SourceDesktopRectangle
        {
            get => _SourceDesktopRectangle;
        }

        /// <summary>
        ///     Gets the area inside the SourceDesktopRectangle which is scanned out to the display.
        /// </summary>
        public Rectangle SourceViewPortRectangle
        {
            get => _SourceViewPortRectangle;
        }

        /// <summary>
        ///     Gets the area inside the rectangle described by targetDisplayWidth/Height SourceViewPortRectangle is scanned out
        ///     to.
        /// </summary>
        public Rectangle TargetViewPortRectangle
        {
            get => _TargetViewPortRectangle;
        }

        /// <summary>
        ///     Gets the horizontal size of the active resolution scanned out to the display.
        /// </summary>
        public uint TargetDisplayWidth
        {
            get => _TargetDisplayWidth;
        }

        /// <summary>
        ///     Gets the vertical size of the active resolution scanned out to the display.
        /// </summary>
        public uint TargetDisplayHeight
        {
            get => _TargetDisplayHeight;
        }

        /// <summary>
        ///     Gets the clone importance assigned to the target if the target is a cloned view of the SourceDesktopRectangle
        ///     (0:primary,1 secondary,...).
        /// </summary>
        public uint CloneImportance
        {
            get => _CloneImportance;
        }

        /// <summary>
        ///     Gets the rotation performed between the SourceViewPortRectangle and the TargetViewPortRectangle.
        /// </summary>
        public Rotate SourceToTargetRotation
        {
            get => _SourceToTargetRotation;
        }
    }

    /// <summary>
    ///     Contains information regarding the scan-out intensity state
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct ScanOutIntensityStateV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal uint _IsEnabled;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets a boolean value indicating if the scan out intensity is enabled or not
        /// </summary>
        public bool IsEnabled
        {
            get => _IsEnabled > 0;
        }
    }

    /// <inheritdoc cref="IScanOutIntensity" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct ScanOutIntensityV1 : IDisposable, IInitializable, IScanOutIntensity
    {
        internal StructureVersion _Version;
        internal uint _Width;
        internal uint _Height;
        internal IntPtr _BlendingTexture;

        /// <summary>
        ///     Creates a new instance of <see cref="ScanOutIntensityV1" />.
        /// </summary>
        /// <param name="width">The width of the input texture.</param>
        /// <param name="height">The height of the input texture</param>
        /// <param name="blendingTexture">The array of floating values building an intensity RGB texture.</param>
        public ScanOutIntensityV1(uint width, uint height, float[] blendingTexture)
        {
            if (blendingTexture?.Length != width * height * 3)
            {
                throw new ArgumentOutOfRangeException(nameof(blendingTexture));
            }

            this = typeof(ScanOutIntensityV1).Instantiate<ScanOutIntensityV1>();
            _Width = width;
            _Height = height;
            _BlendingTexture = Marshal.AllocHGlobal((int)(width * height * 3 * sizeof(float)));

            Marshal.Copy(blendingTexture, 0, _BlendingTexture, blendingTexture.Length);
        }
        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint Width
        {
            get => _Width;
        }

        /// <inheritdoc />
        public uint Height
        {
            get => _Height;
        }

        /// <inheritdoc />
        public float[] BlendingTexture
        {
            get
            {
                var floats = new float[_Width * _Height * 3];
                Marshal.Copy(_BlendingTexture, floats, 0, floats.Length);

                return floats;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Marshal.FreeHGlobal(_BlendingTexture);
        }
    }

    /// <inheritdoc cref="IScanOutIntensity" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct ScanOutIntensityV2 : IDisposable, IInitializable, IScanOutIntensity
    {
        internal StructureVersion _Version;
        internal uint _Width;
        internal uint _Height;
        internal IntPtr _BlendingTexture;
        internal IntPtr _OffsetTexture;
        internal uint _OffsetTextureChannels;

        /// <summary>
        ///     Creates a new instance of <see cref="ScanOutIntensityV2" />.
        /// </summary>
        /// <param name="width">The width of the input texture.</param>
        /// <param name="height">The height of the input texture</param>
        /// <param name="blendingTexture">The array of floating values building an intensity RGB texture</param>
        /// <param name="offsetTextureChannels">The number of channels per pixel in the offset texture</param>
        /// <param name="offsetTexture">The array of floating values building an offset texture</param>
        // ReSharper disable once TooManyDependencies
        public ScanOutIntensityV2(
            uint width,
            uint height,
            float[] blendingTexture,
            uint offsetTextureChannels,
            float[] offsetTexture)
        {
            if (blendingTexture?.Length != width * height * 3)
            {
                throw new ArgumentOutOfRangeException(nameof(blendingTexture));
            }

            if (offsetTexture?.Length != width * height * offsetTextureChannels)
            {
                throw new ArgumentOutOfRangeException(nameof(offsetTexture));
            }

            this = typeof(ScanOutIntensityV2).Instantiate<ScanOutIntensityV2>();
            _Width = width;
            _Height = height;
            _BlendingTexture = Marshal.AllocHGlobal((int)(width * height * 3 * sizeof(float)));
            Marshal.Copy(blendingTexture, 0, _BlendingTexture, blendingTexture.Length);

            _OffsetTextureChannels = offsetTextureChannels;
            _OffsetTexture = Marshal.AllocHGlobal((int)(width * height * offsetTextureChannels * sizeof(float)));
            Marshal.Copy(offsetTexture, 0, _OffsetTexture, offsetTexture.Length);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }
        
        /// <inheritdoc />
        public uint Width
        {
            get => _Width;
        }

        /// <inheritdoc />
        public uint Height
        {
            get => _Height;
        }

        /// <summary>
        ///     Gets the number of channels per pixel in the offset texture
        /// </summary>
        public uint OffsetTextureChannels
        {
            get => _OffsetTextureChannels;
        }

        /// <inheritdoc />
        public float[] BlendingTexture
        {
            get
            {
                var floats = new float[_Width * _Height * 3];
                Marshal.Copy(_BlendingTexture, floats, 0, floats.Length);

                return floats;
            }
        }

        /// <summary>
        ///     Gets the array of floating values building an offset texture
        /// </summary>
        public float[] OffsetTexture
        {
            get
            {
                var floats = new float[_Width * _Height * _OffsetTextureChannels];
                Marshal.Copy(_OffsetTexture, floats, 0, floats.Length);

                return floats;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Marshal.FreeHGlobal(_BlendingTexture);
            Marshal.FreeHGlobal(_OffsetTexture);
        }
    }

    /// <summary>
    ///     Contains information regarding the scan-out warping state
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct ScanOutWarpingStateV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal uint _IsEnabled;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }
        
        /// <summary>
         ///     Gets a boolean value indicating if the scan out warping is enabled or not
         /// </summary>
        public bool IsEnabled
        {
            get => _IsEnabled > 0;
        }
    }

    /// <summary>
    ///     Contains information regarding the scan-out warping data
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct ScanOutWarpingV1 : IDisposable, IInitializable
    {
        internal StructureVersion _Version;
        internal IntPtr _Vertices;
        internal WarpingVerticeFormat _VertexFormat;
        internal uint _NumberOfVertices;
        internal IntPtr _TextureRectangle;

        /// <summary>
        ///     Creates a new instance of <see cref="ScanOutWarpingV1" />.
        /// </summary>
        /// <param name="vertexFormat">The format of the input vertices.</param>
        /// <param name="vertices">The array of floating values containing the warping vertices.</param>
        /// <param name="textureRectangle">The rectangle in desktop coordinates describing the source area for the warping.</param>
        public ScanOutWarpingV1(WarpingVerticeFormat vertexFormat, float[] vertices, Rectangle textureRectangle)
        {
            if (vertices.Length % 6 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(vertices));
            }

            this = typeof(ScanOutWarpingV1).Instantiate<ScanOutWarpingV1>();
            _VertexFormat = vertexFormat;
            _NumberOfVertices = (uint)(vertices.Length / 6);
            _Vertices = Marshal.AllocHGlobal(vertices.Length * sizeof(float));
            Marshal.Copy(vertices, 0, _Vertices, vertices.Length);

            _TextureRectangle = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Rectangle)));
            Marshal.StructureToPtr(textureRectangle, _TextureRectangle, true);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the format of the input vertices
        /// </summary>
        public WarpingVerticeFormat VertexFormat
        {
            get => _VertexFormat;
        }

        /// <summary>
        ///     Gets the rectangle in desktop coordinates describing the source area for the warping
        /// </summary>
        public Rectangle TextureRectangle
        {
            get => (Rectangle)Marshal.PtrToStructure(_TextureRectangle, typeof(Rectangle));
        }

        /// <summary>
        ///     Gets the array of floating values containing the warping vertices
        /// </summary>
        public float[] Vertices
        {
            get
            {
                var floats = new float[_NumberOfVertices * 6];
                Marshal.Copy(_Vertices, floats, 0, floats.Length);

                return floats;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Marshal.FreeHGlobal(_Vertices);
            Marshal.FreeHGlobal(_TextureRectangle);
        }
    }

    /// <summary>
    ///     Holds information about a source mode
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct SourceModeInfo : IEquatable<SourceModeInfo>, ICloneable
    {
        internal Resolution _Resolution;
        internal ColorFormat _ColorFormat;
        internal Position _Position;
        internal SpanningOrientation _SpanningOrientation;
        internal uint _RawReserved;

        /// <summary>
        ///     Creates a new SourceModeInfo
        /// </summary>
        /// <param name="resolution">Source resolution</param>
        /// <param name="colorFormat">Must be Format.Unknown</param>
        /// <param name="position">Source position</param>
        /// <param name="spanningOrientation">Spanning orientation for XP</param>
        /// <param name="isGDIPrimary">true if this source represents the GDI primary display, otherwise false</param>
        /// <param name="isSLIFocus">true if this source represents the SLI focus display, otherwise false</param>
        public SourceModeInfo(
            Resolution resolution,
            ColorFormat colorFormat,
            Position position = default(Position),
            SpanningOrientation spanningOrientation = SpanningOrientation.None,
            bool isGDIPrimary = false,
            bool isSLIFocus = false) : this()
        {
            _Resolution = resolution;
            _ColorFormat = colorFormat;
            _Position = position;
            _SpanningOrientation = spanningOrientation;
            IsGDIPrimary = isGDIPrimary;
            IsSLIFocus = isSLIFocus;
        }

        /// <inheritdoc />
        public bool Equals(SourceModeInfo other)
        {
            return _Resolution.Equals(other._Resolution) &&
                   _ColorFormat == other._ColorFormat &&
                   _Position.Equals(other._Position) &&
                   _SpanningOrientation == other._SpanningOrientation &&
                   _RawReserved == other._RawReserved;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj is SourceModeInfo info && Equals(info);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _Resolution.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)_ColorFormat;
                hashCode = (hashCode * 397) ^ _Position.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)_SpanningOrientation;
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                hashCode = (hashCode * 397) ^ (int)_RawReserved;

                return hashCode;
            }
        }

        public object Clone()
        {
            SourceModeInfo other = (SourceModeInfo)MemberwiseClone();
            return other;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resolution} @ {Position} - {ColorFormat}";
        }

        /// <summary>
        ///     Holds the source resolution
        /// </summary>
        public Resolution Resolution
        {
            get => _Resolution;
            set => _Resolution = value;
        }

        /// <summary>
        ///     Ignored at present, must be Format.Unknown
        /// </summary>
        public ColorFormat ColorFormat
        {
            get => _ColorFormat;
            set => _ColorFormat = value;
        }

        /// <summary>
        ///     Is all positions are 0 or invalid, displays will be automatically positioned from left to right with GDI Primary at
        ///     0,0, and all other displays in the order of the path array.
        /// </summary>
        public Position Position
        {
            get => _Position;
            set => _Position = value;
        }

        /// <summary>
        ///     Spanning is only supported on XP
        /// </summary>
        public SpanningOrientation SpanningOrientation
        {
            get => _SpanningOrientation; 
            set => _SpanningOrientation = value;
        }

        /// <summary>
        ///     Indicates if the path is for the primary GDI display
        /// </summary>
        public bool IsGDIPrimary
        {
            get => _RawReserved.GetBit(0);
            set => _RawReserved = _RawReserved.SetBit(0, value);
        }

        /// <summary>
        ///     Indicates if the path is for the SLI focus display
        /// </summary>
        public bool IsSLIFocus
        {
            get => _RawReserved.GetBit(1);
            set => _RawReserved = _RawReserved.SetBit(1, value);
        }
    }

    /// <summary>
    ///     Holds VESA scan out timing parameters
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct Timing : IEquatable<Timing>, ICloneable
    {
        internal ushort _HorizontalVisible;
        internal ushort _HorizontalBorder;
        internal ushort _HorizontalFrontPorch;
        internal ushort _HorizontalSyncWidth;
        internal ushort _HorizontalTotal;
        internal TimingHorizontalSyncPolarity _HorizontalSyncPolarity;
        internal ushort _VerticalVisible;
        internal ushort _VerticalBorder;
        internal ushort _VerticalFrontPorch;
        internal ushort _VerticalSyncWidth;
        internal ushort _VerticalTotal;
        internal TimingVerticalSyncPolarity _VerticalSyncPolarity;
        internal TimingScanMode _ScanMode;
        internal uint _PixelClockIn10KHertz;
        internal TimingExtra _Extra;

        /// <summary>
        ///     Creates an instance of <see cref="Timing" /> structure.
        /// </summary>
        /// <param name="horizontalVisible">The horizontal visible pixels</param>
        /// <param name="verticalVisible">The vertical visible pixels</param>
        /// <param name="horizontalBorder">The horizontal border pixels</param>
        /// <param name="verticalBorder">The vertical border pixels</param>
        /// <param name="horizontalFrontPorch">The horizontal front porch pixels</param>
        /// <param name="verticalFrontPorch">The vertical front porch pixels</param>
        /// <param name="horizontalSyncWidth">The horizontal sync width pixels</param>
        /// <param name="verticalSyncWidth">The vertical sync width pixels</param>
        /// <param name="horizontalTotal">The horizontal total pixels</param>
        /// <param name="verticalTotal">The vertical total pixels</param>
        /// <param name="horizontalPolarity">The horizontal sync polarity</param>
        /// <param name="verticalPolarity">The vertical sync polarity</param>
        /// <param name="scanMode">The scan mode</param>
        /// <param name="extra">The extra timing information</param>
        public Timing(
            ushort horizontalVisible,
            ushort verticalVisible,
            ushort horizontalBorder,
            ushort verticalBorder,
            ushort horizontalFrontPorch,
            ushort verticalFrontPorch,
            ushort horizontalSyncWidth,
            ushort verticalSyncWidth,
            ushort horizontalTotal,
            ushort verticalTotal,
            TimingHorizontalSyncPolarity horizontalPolarity,
            TimingVerticalSyncPolarity verticalPolarity,
            TimingScanMode scanMode,
            TimingExtra extra
        )
        {
            this = typeof(Timing).Instantiate<Timing>();

            _HorizontalVisible = horizontalVisible;
            _HorizontalBorder = horizontalBorder;
            _HorizontalFrontPorch = horizontalFrontPorch;
            _HorizontalSyncWidth = horizontalSyncWidth;
            _HorizontalTotal = horizontalTotal;
            _HorizontalSyncPolarity = horizontalPolarity;

            _VerticalVisible = verticalVisible;
            _VerticalBorder = verticalBorder;
            _VerticalFrontPorch = verticalFrontPorch;
            _VerticalSyncWidth = verticalSyncWidth;
            _VerticalTotal = verticalTotal;
            _VerticalSyncPolarity = verticalPolarity;

            _ScanMode = scanMode;
            _PixelClockIn10KHertz =
                (uint)(horizontalTotal * verticalTotal * (extra.FrequencyInMillihertz / 1000d) / 10000);

            _Extra = extra;
        }

        /// <summary>
        ///     Creates an instance of <see cref="Timing" /> structure.
        /// </summary>
        /// <param name="horizontalVisible">The horizontal visible pixels</param>
        /// <param name="verticalVisible">The vertical visible pixels</param>
        /// <param name="horizontalBorder">The horizontal border pixels</param>
        /// <param name="verticalBorder">The vertical border pixels</param>
        /// <param name="horizontalFrontPorch">The horizontal front porch pixels</param>
        /// <param name="verticalFrontPorch">The vertical front porch pixels</param>
        /// <param name="horizontalSyncWidth">The horizontal sync width pixels</param>
        /// <param name="verticalSyncWidth">The vertical sync width pixels</param>
        /// <param name="horizontalTotal">The horizontal total pixels</param>
        /// <param name="verticalTotal">The vertical total pixels</param>
        /// <param name="horizontalPolarity">The horizontal sync polarity</param>
        /// <param name="verticalPolarity">The vertical sync polarity</param>
        /// <param name="scanMode">The scan mode</param>
        /// <param name="refreshRateFrequencyInHz">The frequency in hertz</param>
        /// <param name="horizontalPixelRepetition">The number of identical horizontal pixels that are repeated; 1 = no repetition</param>
        public Timing(
            ushort horizontalVisible,
            ushort verticalVisible,
            ushort horizontalBorder,
            ushort verticalBorder,
            ushort horizontalFrontPorch,
            ushort verticalFrontPorch,
            ushort horizontalSyncWidth,
            ushort verticalSyncWidth,
            ushort horizontalTotal,
            ushort verticalTotal,
            TimingHorizontalSyncPolarity horizontalPolarity,
            TimingVerticalSyncPolarity verticalPolarity,
            TimingScanMode scanMode,
            double refreshRateFrequencyInHz,
            ushort horizontalPixelRepetition = 1
        ) : this(
            horizontalVisible, verticalVisible, horizontalBorder,
            verticalBorder, horizontalFrontPorch, verticalFrontPorch,
            horizontalSyncWidth, verticalSyncWidth, horizontalTotal,
            verticalTotal, horizontalPolarity, verticalPolarity, scanMode,
            new TimingExtra(
                refreshRateFrequencyInHz,
                $"CUST:{horizontalVisible}x{verticalVisible}x{refreshRateFrequencyInHz:F3}Hz",
                0,
                0,
                horizontalPixelRepetition
            )
        )
        {
        }

        /// <inheritdoc />
        public bool Equals(Timing other)
        {
            return _HorizontalVisible == other._HorizontalVisible &&
                   _HorizontalBorder == other._HorizontalBorder &&
                   _HorizontalFrontPorch == other._HorizontalFrontPorch &&
                   _HorizontalSyncWidth == other._HorizontalSyncWidth &&
                   _HorizontalTotal == other._HorizontalTotal &&
                   _HorizontalSyncPolarity == other._HorizontalSyncPolarity &&
                   _VerticalVisible == other._VerticalVisible &&
                   _VerticalBorder == other._VerticalBorder &&
                   _VerticalFrontPorch == other._VerticalFrontPorch &&
                   _VerticalSyncWidth == other._VerticalSyncWidth &&
                   _VerticalTotal == other._VerticalTotal &&
                   _VerticalSyncPolarity == other._VerticalSyncPolarity &&
                   _ScanMode == other._ScanMode &&
                   _PixelClockIn10KHertz == other._PixelClockIn10KHertz &&
                   _Extra.Equals(other._Extra);
        }

        public override bool Equals(object obj) => obj is Timing other && this.Equals(other);

        public override int GetHashCode()
        {
            return (_HorizontalVisible, _HorizontalBorder, _HorizontalFrontPorch, _HorizontalSyncWidth, _HorizontalTotal, _HorizontalSyncPolarity,
                _VerticalVisible, _VerticalBorder, _VerticalFrontPorch, _VerticalSyncWidth, _VerticalTotal, _VerticalSyncPolarity, _ScanMode,
                _PixelClockIn10KHertz, _Extra).GetHashCode();
        }
        public static bool operator ==(Timing lhs, Timing rhs) => lhs.Equals(rhs);

        public static bool operator !=(Timing lhs, Timing rhs) => !(lhs == rhs);

        public object Clone()
        {
            Timing other = (Timing)MemberwiseClone();
            return other;
        }

        /// <summary>
        ///     Get the horizontal visible pixels
        /// </summary>
        public int HorizontalVisible
        {
            get => _HorizontalVisible;
            set => _HorizontalVisible = (ushort)value;
        }

        /// <summary>
        ///     Get the horizontal border pixels
        /// </summary>
        public int HorizontalBorder
        {
            get => _HorizontalBorder;
            set => _HorizontalBorder = (ushort)value;
        }

        /// <summary>
        ///     Get the horizontal front porch pixels
        /// </summary>
        public int HorizontalFrontPorch
        {
            get => _HorizontalFrontPorch;
            set => _HorizontalFrontPorch = (ushort)value;
        }

        /// <summary>
        ///     Get the horizontal sync width pixels
        /// </summary>
        public int HorizontalSyncWidth
        {
            get => _HorizontalSyncWidth;
            set => _HorizontalSyncWidth = (ushort)value;
        }

        /// <summary>
        ///     Get the horizontal total pixels
        /// </summary>
        public int HorizontalTotal
        {
            get => _HorizontalTotal;
            set => _HorizontalTotal = (ushort)value;
        }

        /// <summary>
        ///     Get the horizontal sync polarity
        /// </summary>
        public TimingHorizontalSyncPolarity HorizontalSyncPolarity
        {
            get => _HorizontalSyncPolarity;
            set => _HorizontalSyncPolarity = value;
        }

        /// <summary>
        ///     Get the vertical visible pixels
        /// </summary>
        public int VerticalVisible
        {
            get => _VerticalVisible;
            set => _VerticalVisible = (ushort)value;
        }

        /// <summary>
        ///     Get the vertical border pixels
        /// </summary>
        public int VerticalBorder
        {
            get => _VerticalBorder;
            set => _VerticalBorder = (ushort)value;
        }

        /// <summary>
        ///     Get the vertical front porch pixels
        /// </summary>
        public int VerticalFrontPorch
        {
            get => _VerticalFrontPorch;
            set => _VerticalFrontPorch = (ushort)value;
        }

        /// <summary>
        ///     Get the vertical sync width pixels
        /// </summary>
        public int VerticalSyncWidth
        {
            get => _VerticalSyncWidth;
            set => _VerticalSyncWidth = (ushort)value;
        }

        /// <summary>
        ///     Get the vertical total pixels
        /// </summary>
        public int VerticalTotal
        {
            get => _VerticalTotal;
            set => _VerticalTotal = (ushort)value;
        }

        /// <summary>
        ///     Get the vertical sync polarity
        /// </summary>
        public TimingVerticalSyncPolarity VerticalSyncPolarity
        {
            get => _VerticalSyncPolarity;
            set => _VerticalSyncPolarity = value;
        }

        /// <summary>
        ///     Get the scan mode
        /// </summary>
        public TimingScanMode ScanMode
        {
            get => _ScanMode;
            set => _ScanMode = value;
        }

        /// <summary>
        ///     Get the pixel clock in 10 kHz
        /// </summary>
        public int PixelClockIn10KHertz
        {
            get => (int)_PixelClockIn10KHertz;
            set => _PixelClockIn10KHertz = (ushort)value;
        }

        /// <summary>
        ///     Get the other timing related extras
        /// </summary>
        public TimingExtra Extra
        {
            get => _Extra;
            set => _Extra = value;
        }

        /// <summary>
        ///     Gets the horizontal active pixels
        /// </summary>
        public int HorizontalActive
        {
            get => HorizontalVisible + HorizontalBorder;
        }

        /// <summary>
        ///     Gets the vertical active pixels
        /// </summary>
        public int VerticalActive
        {
            get => VerticalVisible + VerticalBorder;
        }

        /// <summary>
        ///     Gets the horizontal back porch pixels
        /// </summary>
        public int HorizontalBackPorch
        {
            get => HorizontalBlanking - (HorizontalFrontPorch + HorizontalSyncWidth);
        }

        /// <summary>
        ///     Gets the horizontal blanking pixels
        /// </summary>
        public int HorizontalBlanking
        {
            get => HorizontalTotal - (HorizontalActive + HorizontalBorder);
        }

        /// <summary>
        ///     Gets vertical back porch pixels
        /// </summary>
        public int VerticalBackPorch
        {
            get => VerticalBlanking - (VerticalFrontPorch + VerticalSyncWidth);
        }

        /// <summary>
        ///     Gets the vertical blanking pixels
        /// </summary>
        public int VerticalBlanking
        {
            get => VerticalTotal - (VerticalActive + VerticalBorder);
        }
    }

    /// <summary>
    ///     Holds NVIDIA-specific timing extras
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct TimingExtra : IInitializable, IEquatable<TimingExtra>, ICloneable
    {
        internal uint _HardwareFlags;
        internal ushort _RefreshRate;
        internal uint _FrequencyInMillihertz;
        internal ushort _VerticalAspect;
        internal ushort _HorizontalAspect;
        internal ushort _HorizontalPixelRepetition;
        internal uint _Standard;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        internal string _Name;

        /// <summary>
        ///     Creates a new instance of <see cref="TimingExtra" /> structure.
        /// </summary>
        /// <param name="frequencyInHertz">The timing frequency in hertz</param>
        /// <param name="name">The timing source name</param>
        /// <param name="horizontalAspect">The display horizontal aspect</param>
        /// <param name="verticalAspect">The display vertical aspect</param>
        /// <param name="horizontalPixelRepetition">The number of identical horizontal pixels that are repeated; 1 = no repetition</param>
        /// <param name="hardwareFlags">The NVIDIA hardware-based enhancement, such as double-scan.</param>
        public TimingExtra(
            double frequencyInHertz,
            string name,
            ushort horizontalAspect = 0,
            ushort verticalAspect = 0,
            ushort horizontalPixelRepetition = 1,
            uint hardwareFlags = 0
        ) : this(
            (uint)(frequencyInHertz * 1000d),
            (ushort)frequencyInHertz,
            name,
            horizontalAspect,
            verticalAspect,
            horizontalPixelRepetition,
            hardwareFlags
        )
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TimingExtra" /> structure.
        /// </summary>
        /// <param name="frequencyInMillihertz">The timing frequency in millihertz</param>
        /// <param name="refreshRate">The refresh rate</param>
        /// <param name="name">The timing source name</param>
        /// <param name="horizontalAspect">The display horizontal aspect</param>
        /// <param name="verticalAspect">The display vertical aspect</param>
        /// <param name="horizontalPixelRepetition">The number of identical horizontal pixels that are repeated; 1 = no repetition</param>
        /// <param name="hardwareFlags">The NVIDIA hardware-based enhancement, such as double-scan.</param>
        public TimingExtra(
            uint frequencyInMillihertz,
            ushort refreshRate,
            string name,
            ushort horizontalAspect = 0,
            ushort verticalAspect = 0,
            ushort horizontalPixelRepetition = 1,
            uint hardwareFlags = 0
        )
        {
            this = typeof(TimingExtra).Instantiate<TimingExtra>();
            _FrequencyInMillihertz = frequencyInMillihertz;
            _RefreshRate = refreshRate;
            _HorizontalAspect = horizontalAspect;
            _VerticalAspect = verticalAspect;
            _HorizontalPixelRepetition = horizontalPixelRepetition;
            _HardwareFlags = hardwareFlags;
            _Name = name?.Length > 40 ? name.Substring(0, 40) : name ?? "";
        }

        /// <inheritdoc />
        public bool Equals(TimingExtra other)
        {
            return _HardwareFlags == other._HardwareFlags &&
                   _RefreshRate == other._RefreshRate &&
                   _FrequencyInMillihertz == other._FrequencyInMillihertz &&
                   _VerticalAspect == other._VerticalAspect &&
                   _HorizontalAspect == other._HorizontalAspect &&
                   _HorizontalPixelRepetition == other._HorizontalPixelRepetition &&
                   _Standard == other._Standard;
        }

        public override bool Equals(object obj) => obj is TimingExtra other && this.Equals(other);

        public override int GetHashCode()
        {
            return (_HardwareFlags, _RefreshRate, _FrequencyInMillihertz, _VerticalAspect, _HorizontalAspect, _HorizontalPixelRepetition, _Standard).GetHashCode();
        }
        public static bool operator ==(TimingExtra lhs, TimingExtra rhs) => lhs.Equals(rhs);

        public static bool operator !=(TimingExtra lhs, TimingExtra rhs) => !(lhs == rhs);

        /// <summary>
        ///     Gets the NVIDIA hardware-based enhancement, such as double-scan.
        /// </summary>
        public uint HardwareFlags
        {
            get => _HardwareFlags;
            set => _HardwareFlags = value;
        }

        /// <summary>
        ///     Gets the logical refresh rate to present
        /// </summary>
        public int RefreshRate
        {
            get => _RefreshRate;
            set => _RefreshRate = (ushort)value;
        }

        /// <summary>
        ///     Gets the physical vertical refresh rate in 0.001Hz
        /// </summary>
        public int FrequencyInMillihertz
        {
            get => (int)_FrequencyInMillihertz;
            set => _FrequencyInMillihertz = (uint)value;
        }

        /// <summary>
        ///     Gets the display vertical aspect
        /// </summary>
        public int VerticalAspect
        {
            get => _VerticalAspect;
            set => _VerticalAspect = (ushort)value;
        }

        /// <summary>
        ///     Gets the display horizontal aspect
        /// </summary>
        public int HorizontalAspect
        {
            get => _HorizontalAspect;
            set => _HorizontalAspect = (ushort)value;
        }

        /// <summary>
        ///     Gets the bit-wise pixel repetition factor: 0x1:no pixel repetition; 0x2:each pixel repeats twice horizontally,..
        /// </summary>
        public int PixelRepetition
        {
            get => _HorizontalPixelRepetition;
            set => _HorizontalPixelRepetition = (ushort)value;
        }

        /// <summary>
        ///     Gets the timing standard
        /// </summary>
        public uint Standard
        {
            get => _Standard;
            set => _Standard = value;
        }

        /// <summary>
        ///     Gets the timing name
        /// </summary>
        public string Name
        {
            get => _Name;
            set => _Name = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        public object Clone()
        {
            TimingExtra other = (TimingExtra)MemberwiseClone();
            return other;
        }
    }

    /// <summary>
    ///     Contains the information required for calculating timing for a particular display
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct TimingInput : IInitializable, ICloneable, IEquatable<TimingInput>
    {
        [StructLayout(LayoutKind.Explicit, Pack = 8, Size = 12)]
        internal struct TimingFlag
        {
            [FieldOffset(0)] internal ushort _Flags;
            [FieldOffset(4)] internal byte _TVFormatCEAIdPSFormatId;
            [FieldOffset(8)] internal byte _Scaling;

            public bool IsInterlaced
            {
                get => _Flags.GetBit(0);
                set => _Flags = _Flags.SetBit(0, value);
            }

            public TVFormat TVFormat
            {
                get => (TVFormat)_TVFormatCEAIdPSFormatId;
                set => _TVFormatCEAIdPSFormatId = (byte)value;
            }

            public byte CEAId
            {
                get => _TVFormatCEAIdPSFormatId;
                set => _TVFormatCEAIdPSFormatId = value;
            }

            public byte PredefinedPSFormatId
            {
                get => _TVFormatCEAIdPSFormatId;
                set => _TVFormatCEAIdPSFormatId = value;
            }

            public byte Scaling
            {
                get => _Scaling;
                set => _Scaling = value;
            }

            public TimingFlag(bool isInterlaced, byte scaling) : this()
            {
                IsInterlaced = isInterlaced;
                Scaling = scaling;
            }

            public TimingFlag(bool isInterlaced, byte scaling, TVFormat tvFormat) : this(isInterlaced, scaling)
            {
                TVFormat = tvFormat;
            }

            public TimingFlag(bool isInterlaced, byte scaling, byte ceaIdOrPredefinedPSFormatId) : this(isInterlaced,
                scaling)
            {
                _TVFormatCEAIdPSFormatId = ceaIdOrPredefinedPSFormatId;
            }
        }

        internal StructureVersion _Version;
        internal uint _Width;
        internal uint _Height;
        internal float _RefreshRate;
        internal TimingFlag _Flags;
        internal TimingOverride _TimingType;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }


        /// <summary>
        ///     Gets the visible horizontal size
        /// </summary>
        public uint Width
        {
            get => _Width; 
            set => _Width = value;
        }

        /// <summary>
        ///     Gets the visible vertical size
        /// </summary>
        public uint Height
        {
            get => _Height;
            set => _Height = value;
        }

        /// <summary>
        ///     Gets the timing refresh rate
        /// </summary>
        public float RefreshRate
        {
            get => _RefreshRate;
            set => _RefreshRate = value;
        }

        /// <summary>
        ///     Gets a boolean value indicating if the requested timing is an interlaced timing
        /// </summary>
        public bool IsInterlaced
        {
            get => _Flags.IsInterlaced;
            set => _Flags.IsInterlaced = value;
        }

        /// <summary>
        ///     Gets the preferred scaling
        /// </summary>
        public byte Scaling
        {
            get => _Flags.Scaling;
            set => _Flags.Scaling = value;
        }

        /// <summary>
        ///     Gets timing type (formula) to use for calculating the timing
        /// </summary>
        public TimingOverride TimingType
        {
            get => _TimingType;
            set => _TimingType = value;
        }

        /// <summary>
        ///     Creates an instance of the TimingInput
        /// </summary>
        /// <param name="width">The preferred visible horizontal size</param>
        /// <param name="height">The preferred visible vertical size</param>
        /// <param name="refreshRate">The preferred timing refresh rate</param>
        /// <param name="timingType">The preferred formula to be used for timing calculation</param>
        /// <param name="isInterlaced">A boolean value indicating if the preferred timing is interlaced</param>
        /// <param name="scaling">The preferred scaling factor</param>
        public TimingInput(
            uint width,
            uint height,
            float refreshRate,
            TimingOverride timingType,
            bool isInterlaced = false,
            byte scaling = 0
        )
        {
            this = typeof(TimingInput).Instantiate<TimingInput>();
            _Width = width;
            _Height = height;
            _RefreshRate = refreshRate;
            _TimingType = timingType;
            _Flags = new TimingFlag(isInterlaced, scaling);
        }

        /// <summary>
        ///     Creates an instance of the TimingInput
        /// </summary>
        /// <param name="tvFormat">The preferred analog TV format</param>
        /// <param name="isInterlaced">A boolean value indicating if the preferred timing is interlaced</param>
        /// <param name="scaling">The preferred scaling factor</param>
        public TimingInput(TVFormat tvFormat, bool isInterlaced = false, byte scaling = 0)
            : this(0, 0, 0, TimingOverride.AnalogTV, isInterlaced, scaling)
        {
            _Flags = new TimingFlag(isInterlaced, scaling, tvFormat);
        }

        /// <summary>
        ///     Creates an instance of the TimingInput
        /// </summary>
        /// <param name="ceaIdOrPredefinedPSFormatId">
        ///     The CEA id or the predefined PsF format id depending on the value of other
        ///     arguments
        /// </param>
        /// <param name="timingType">
        ///     The preferred formula to be used for timing calculation, valid values for this overload are
        ///     <see cref="TimingOverride.EIA861" /> and <see cref="TimingOverride.Predefined" />.
        /// </param>
        /// <param name="isInterlaced">A boolean value indicating if the preferred timing is interlaced</param>
        /// <param name="scaling">The preferred scaling factor</param>
        public TimingInput(
            byte ceaIdOrPredefinedPSFormatId,
            TimingOverride timingType,
            bool isInterlaced = false,
            byte scaling = 0
        )
            : this(0, 0, 0, timingType, isInterlaced, scaling)
        {
            if (timingType != TimingOverride.EIA861 && timingType != TimingOverride.Predefined)
            {
                throw new ArgumentException("Invalid timing type passed.", nameof(timingType));
            }

            _Flags = new TimingFlag(isInterlaced, scaling, ceaIdOrPredefinedPSFormatId);
        }

        /// <summary>
        ///     Creates an instance of the TimingInput
        /// </summary>
        /// <param name="timingType">
        ///     The preferred formula to be used for timing calculation.
        /// </param>
        public TimingInput(TimingOverride timingType)
            : this(0, 0, 0, timingType)
        {
        }

        /// <summary>
        ///     Gets the analog TV actual HD/SDTV format
        /// </summary>
        public TVFormat? TVFormat
        {
            get
            {
                if (Width == 0 && Height == 0 && Math.Abs(RefreshRate) < 0.01 && TimingType == TimingOverride.AnalogTV)
                {
                    return _Flags.TVFormat;
                }

                return null;
            }
            set => _Flags.TVFormat = (TVFormat)value;
        }

        /// <summary>
        ///     Gets the EIA/CEA 861B/D predefined short timing descriptor id
        /// </summary>
        public byte? CEAId
        {
            get
            {
                if (Width == 0 && Height == 0 && Math.Abs(RefreshRate) < 0.01 && TimingType == TimingOverride.EIA861)
                {
                    return _Flags.CEAId;
                }

                return null;
            }
            set => _Flags.CEAId = (byte)value;
        }

        /// <summary>
        ///     Gets the Nvidia predefined PsF format id
        /// </summary>
        public byte? PredefinedPSFormatId
        {
            get
            {
                if (TimingType == TimingOverride.Predefined)
                {
                    return _Flags.PredefinedPSFormatId;
                }

                return null;
            }
            set => _Flags.PredefinedPSFormatId = (byte)value;
        }

        public override bool Equals(object obj) => obj is TimingInput other && this.Equals(other);

        public bool Equals(TimingInput other)
        => Version == other.Version &&
            Width == other.Width &&
            Height == other.Height &&
            RefreshRate == other.RefreshRate &&
            TimingType == other.TimingType &&
            Scaling == other.Scaling &&
            IsInterlaced == other.IsInterlaced;

        public override Int32 GetHashCode()
        {
            return (Version, Width, Height, RefreshRate, TimingType, _Flags).GetHashCode();
        }
        public static bool operator ==(TimingInput lhs, TimingInput rhs) => lhs.Equals(rhs);

        public static bool operator !=(TimingInput lhs, TimingInput rhs) => !(lhs == rhs);

        public object Clone()
        {
            TimingInput other = (TimingInput)MemberwiseClone();
            return other;
        }
    }

    /// <summary>
    ///     UnAttachedDisplayHandle is a one-to-one map to the GDI handle of an unattached display.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct UnAttachedDisplayHandle : IHandle, IEquatable<UnAttachedDisplayHandle>, ICloneable
    {
        internal readonly IntPtr _MemoryAddress;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _MemoryAddress.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"UnAttachedDisplayHandle #{MemoryAddress.ToInt64()}";
        }

        public override bool Equals(object obj) => obj is UnAttachedDisplayHandle other && this.Equals(other);

        public bool Equals(UnAttachedDisplayHandle other)
        => MemoryAddress == other.MemoryAddress;

        public static bool operator ==(UnAttachedDisplayHandle lhs, UnAttachedDisplayHandle rhs) => lhs.Equals(rhs);

        public static bool operator !=(UnAttachedDisplayHandle lhs, UnAttachedDisplayHandle rhs) => !(lhs == rhs);

        public object Clone()
        {
            UnAttachedDisplayHandle other = (UnAttachedDisplayHandle)MemberwiseClone();
            return other;
        }

        /// <inheritdoc />
        public IntPtr MemoryAddress
        {
            get => _MemoryAddress;
        }

        /// <inheritdoc />
        public bool IsNull
        {
            get => _MemoryAddress == IntPtr.Zero;
        }
    }

    /// <summary>
    ///     Hold information about the screen view port rectangle
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ViewPortF : ICloneable, IEquatable<ViewPortF>
    {
        internal float _X;
        internal float _Y;
        internal float _Width;
        internal float _Height;

        /// <summary>
        ///     Gets the x-coordinate of the viewport top-left point
        /// </summary>
        public float X
        {
            get => _X;
        }

        /// <summary>
        ///     Gets the y-coordinate of the viewport top-left point
        /// </summary>
        public float Y
        {
            get => _Y;
        }

        /// <summary>
        ///     Gets the width of the viewport.
        /// </summary>
        public float Width
        {
            get => _Width;
        }

        /// <summary>
        ///     Gets the height of the viewport.
        /// </summary>
        public float Height
        {
            get => _Height;
        }

        /// <summary>
        ///     Creates an instance of ViewPortF
        /// </summary>
        /// <param name="x">The x-coordinate of the viewport top-left point</param>
        /// <param name="y">The y-coordinate of the viewport top-left point</param>
        /// <param name="width">The width of the viewport.</param>
        /// <param name="height">The height of the viewport.</param>
        public ViewPortF(float x, float y, float width, float height)
        {
            _X = x;
            _Y = y;
            _Width = width;
            _Height = height;
        }

        /// <summary>
        ///     Creates an instance of <see cref="ViewPortF" />
        /// </summary>
        /// <param name="rect">The rectangle to take view port information from.</param>
        public ViewPortF(RectangleF rect) : this(rect.X, rect.Y, rect.Width, rect.Height)
        {
        }

        /// <summary>
        ///     Return an instance of <see cref="RectangleF" /> representing this view port.
        /// </summary>
        /// <returns></returns>
        public RectangleF ToRectangle()
        {
            return new RectangleF(X, Y, Width, Height);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Width:F1}, {Height:F1}) @ ({X:F1}, {Y:F1})";
        }

        public override bool Equals(object obj) => obj is ViewPortF other && this.Equals(other);

        public bool Equals(ViewPortF other)
        => X == other.X &&
            Y == other.Y &&
            Width == other.Width &&
            Height == other.Height ;

        public override Int32 GetHashCode()
        {
            return (X, Y, Width, Height).GetHashCode();
        }
        public static bool operator ==(ViewPortF lhs, ViewPortF rhs) => lhs.Equals(rhs);

        public static bool operator !=(ViewPortF lhs, ViewPortF rhs) => !(lhs == rhs);

        public object Clone()
        {
            ViewPortF other = (ViewPortF)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct GetAdaptiveSyncData : IInitializable, IEquatable<GetAdaptiveSyncData>, ICloneable
    {
        public StructureVersion _version; // Must be V1
        public UInt32 MaxFrameInterval;             //!< maximum frame interval in micro seconds as set previously using NvAPI_DISP_SetAdaptiveSyncData function. If default values from EDID are used, this parameter returns 0.
        public UInt32 Flags;
        public UInt32 LastFlipRefreshCount;             //!< Number of times the last flip was shown on the screen
        public UInt64 LastFlipTimeStamp;             //!< Timestamp for the lastest flip on the screen
        public UInt32 ReservedEx1;
        public UInt32 ReservedEx2;
        public UInt32 ReservedEx3;
        public UInt32 ReservedEx4;       

        public bool DisableAdaptiveSync => (Flags & 0x1) == 0x1; //!< Indicates if adaptive sync is disabled on the display.
        public bool DisableFrameSplitting => (Flags & 0x1) == 0x1; //!< Indicates if frame splitting is disabled on the display.

        public override bool Equals(object obj) => obj is GetAdaptiveSyncData other && this.Equals(other);

        public bool Equals(GetAdaptiveSyncData other)
        => MaxFrameInterval == other.MaxFrameInterval &&
            Flags == other.Flags;

        public override Int32 GetHashCode()
        {
            return (MaxFrameInterval, Flags).GetHashCode();
        }
        public static bool operator ==(GetAdaptiveSyncData lhs, GetAdaptiveSyncData rhs) => lhs.Equals(rhs);

        public static bool operator !=(GetAdaptiveSyncData lhs, GetAdaptiveSyncData rhs) => !(lhs == rhs);
        public object Clone()
        {
            GetAdaptiveSyncData other = (GetAdaptiveSyncData)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct SetAdaptiveSyncData : IInitializable, IEquatable<SetAdaptiveSyncData>, ICloneable
    {
        public StructureVersion _version; // Must be V1
        public UInt32 MaxFrameInterval;             //!< maximum frame interval in micro seconds as set previously using NvAPI_DISP_SetAdaptiveSyncData function. If default values from EDID are used, this parameter returns 0.
        public UInt32 Flags;
        public UInt32 ReservedEx1;             //!< Number of times the last flip was shown on the screen
        public UInt64 ReservedEx2;             //!< Timestamp for the lastest flip on the screen
        public UInt32 ReservedEx3;
        public UInt32 ReservedEx4;
        public UInt32 ReservedEx5;
        public UInt32 ReservedEx6;
        public UInt32 ReservedEx7;

        public bool DisableAdaptiveSync => (Flags & 0x1) == 0x1; //!< Indicates if adaptive sync is disabled on the display.
        public bool DisableFrameSplitting => (Flags & 0x1) == 0x1; //!< Indicates if frame splitting is disabled on the display.

        public override bool Equals(object obj) => obj is SetAdaptiveSyncData other && this.Equals(other);

        public bool Equals(SetAdaptiveSyncData other)
        => MaxFrameInterval == other.MaxFrameInterval &&
            Flags == other.Flags;

        public override Int32 GetHashCode()
        {
            return (MaxFrameInterval, Flags).GetHashCode();
        }
        public static bool operator ==(SetAdaptiveSyncData lhs, SetAdaptiveSyncData rhs) => lhs.Equals(rhs);

        public static bool operator !=(SetAdaptiveSyncData lhs, SetAdaptiveSyncData rhs) => !(lhs == rhs);
        public object Clone()
        {
            SetAdaptiveSyncData other = (SetAdaptiveSyncData)MemberwiseClone();
            return other;
        }
    }


}
