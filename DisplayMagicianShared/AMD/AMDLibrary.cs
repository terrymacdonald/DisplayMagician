using ADLXWrapper;
using DisplayMagicianShared;
using DisplayMagicianShared.NVIDIA;
using DisplayMagicianShared.Windows;
using EDIDParser;
using IGCLWrapper;
using Microsoft.VisualBasic;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Devices.PointOfService;
using Windows.Graphics;
using WinRT;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DisplayMagicianShared.AMD
{

    [StructLayout(LayoutKind.Sequential)]
    public struct AMD_SLSMAP_CONFIG : IEquatable<AMD_SLSMAP_CONFIG>
    {
        public ADL_SLS_MAP SLSMap;
        public List<ADL_SLS_TARGET> SLSTargets;
        public List<ADL_SLS_MODE> NativeModes;
        public List<ADL_SLS_OFFSET> NativeModeOffsets;
        public List<ADL_BEZEL_TRANSIENT_MODE> BezelModes;
        public List<ADL_BEZEL_TRANSIENT_MODE> TransientModes;
        public List<ADL_SLS_OFFSET> SLSOffsets;
        public int BezelModePercent;

        public AMD_SLSMAP_CONFIG()
        {
            SLSMap = new ADL_SLS_MAP();
            SLSTargets = new List<ADL_SLS_TARGET>() { };
            NativeModes = new List<ADL_SLS_MODE>() { };
            NativeModeOffsets = new List<ADL_SLS_OFFSET>() { };
            BezelModes = new List<ADL_BEZEL_TRANSIENT_MODE>() { };
            TransientModes = new List<ADL_BEZEL_TRANSIENT_MODE>() { };
            SLSOffsets = new List<ADL_SLS_OFFSET>() { };
            BezelModePercent= 0;
        }

        public override bool Equals(object obj) => obj is AMD_SLS_CONFIG other && this.Equals(other);

        public bool Equals(AMD_SLSMAP_CONFIG other)
        => SLSMap == other.SLSMap &&
           SLSTargets.SequenceEqual(other.SLSTargets) &&
           NativeModes.SequenceEqual(other.NativeModes) &&
           NativeModeOffsets.SequenceEqual(other.NativeModeOffsets) &&
           BezelModes.SequenceEqual(other.BezelModes) &&
           TransientModes.SequenceEqual(other.TransientModes) &&
           SLSOffsets.SequenceEqual(other.SLSOffsets) &&
           BezelModePercent == other.BezelModePercent;

        public override int GetHashCode()
        {
            return (SLSMap, SLSTargets, NativeModes, NativeModeOffsets, BezelModes, TransientModes, SLSOffsets, BezelModePercent).GetHashCode();
        }
        public static bool operator ==(AMD_SLSMAP_CONFIG lhs, AMD_SLSMAP_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(AMD_SLSMAP_CONFIG lhs, AMD_SLSMAP_CONFIG rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AMD_SLS_CONFIG : IEquatable<AMD_SLS_CONFIG>
    {
        public bool IsSlsEnabled;
        public List<AMD_SLSMAP_CONFIG> SLSMapConfigs;
        public List<ADL_MODE> SLSEnabledDisplayTargets;

        public AMD_SLS_CONFIG()
        {
            IsSlsEnabled = false;
            SLSMapConfigs = new List<AMD_SLSMAP_CONFIG>();
            SLSEnabledDisplayTargets = new List<ADL_MODE>();
        }

        public override bool Equals(object obj) => obj is AMD_SLS_CONFIG other && this.Equals(other);

        public bool Equals(AMD_SLS_CONFIG other)
        => IsSlsEnabled == other.IsSlsEnabled &&
           SLSMapConfigs.SequenceEqual(other.SLSMapConfigs) &&
           SLSEnabledDisplayTargets.SequenceEqual(other.SLSEnabledDisplayTargets);

        public override int GetHashCode()
        {
            return (IsSlsEnabled, SLSMapConfigs, SLSEnabledDisplayTargets).GetHashCode();
        }
        public static bool operator ==(AMD_SLS_CONFIG lhs, AMD_SLS_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(AMD_SLS_CONFIG lhs, AMD_SLS_CONFIG rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AMD_DESKTOP : IEquatable<AMD_DESKTOP>
    {
        public long NumberOfDisplays;
        public List<AMD_DISPLAY> Displays;
        public ADLX_ORIENTATION Orientation;
        public int SizeWidth;
        public int SizeHeight;
        public int TopLeftX;
        public int TopLeftY;
        public ADLX_DESKTOP_TYPE Type;

        public AMD_DESKTOP()
        {
            Displays = new List<AMD_DISPLAY>();
            Type = ADLX_DESKTOP_TYPE.DESKTOP_SINGLE;
        }

        public override bool Equals(object obj) => obj is AMD_DESKTOP other && this.Equals(other);
        public bool Equals(AMD_DESKTOP other)
        {
            if (NumberOfDisplays != other.NumberOfDisplays)
            {
                SharedLogger.logger.Trace($"AMD_DESKTOP/Equals: The NumberOfDisplays values don't equal each other");
                return false;
            }
            if (!Displays.SequenceEqual(other.Displays))
            {
                SharedLogger.logger.Trace($"AMD_DESKTOP/Equals: The SequenceEqual values don't equal each other");
                return false;
            }
            if (Orientation != other.Orientation)
            {
                SharedLogger.logger.Trace($"AMD_DESKTOP/Equals: The Orientation values don't equal each other");
                return false;
            }
            if (SizeWidth != other.SizeWidth)
            {
                SharedLogger.logger.Trace($"AMD_DESKTOP/Equals: The SizeWidth values don't equal each other");
                return false;
            }
            if (SizeHeight != other.SizeHeight)
            {
                SharedLogger.logger.Trace($"AMD_DESKTOP/Equals: The SizeHeight values don't equal each other");
                return false;
            }
            if (TopLeftX != other.TopLeftX)
            {
                SharedLogger.logger.Trace($"AMD_DESKTOP/Equals: The TopLeftX values don't equal each other");
                return false;
            }
            if (TopLeftY != other.TopLeftY)
            {
                SharedLogger.logger.Trace($"AMD_DESKTOP/Equals: The TopLeftY values don't equal each other");
                return false;
            }
            if (Type != other.Type)
            {
                SharedLogger.logger.Trace($"AMD_DESKTOP/Equals: The Type values don't equal each other");
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (NumberOfDisplays, Displays, Orientation, SizeWidth, SizeHeight, TopLeftX, TopLeftY, Type).GetHashCode();
        }
        public static bool operator ==(AMD_DESKTOP lhs, AMD_DESKTOP rhs) => lhs.Equals(rhs);

        public static bool operator !=(AMD_DESKTOP lhs, AMD_DESKTOP rhs) => !(lhs == rhs);
    }

    /* [StructLayout(LayoutKind.Sequential)]
     public struct EYEFINITY_GRID_NODE : IEquatable<EYEFINITY_GRID_NODE>
     {
         public long Row;
         public long Column;
         public ADLX_ORIENTATION DisplayOrientation;
         public int DisplayWidth;
         public int DisplayHeight;
         public int DisplayTopLeftX;
         public int DisplayTopLeftY;
         public long DisplayUniqueId;

         public EYEFINITY_GRID_NODE()
         {
             DisplayOrientation = ADLX_ORIENTATION.ORIENTATION_LANDSCAPE;
         }

         public override bool Equals(object obj) => obj is EYEFINITY_GRID_NODE other && this.Equals(other);
         public bool Equals(EYEFINITY_GRID_NODE other)
         {
             if (Row != other.Row)
             {
                 SharedLogger.logger.Trace($"EYEFINITY_GRID_NODE/Equals: The Row values don't equal each other");
                 return false;
             }
             if (Column != other.Column)
             {
                 SharedLogger.logger.Trace($"EYEFINITY_GRID_NODE/Equals: The Column values don't equal each other");
                 return false;
             }
             if (DisplayOrientation != other.DisplayOrientation)
             {
                 SharedLogger.logger.Trace($"EYEFINITY_GRID_NODE/Equals: The DisplayOrientation values don't equal each other");
                 return false;
             }
             if (DisplayWidth != other.DisplayWidth)
             {
                 SharedLogger.logger.Trace($"EYEFINITY_GRID_NODE/Equals: The DisplayWidth values don't equal each other");
                 return false;
             }
             if (DisplayHeight != other.DisplayHeight)
             {
                 SharedLogger.logger.Trace($"EYEFINITY_GRID_NODE/Equals: The DisplayHeight values don't equal each other");
                 return false;
             }
             if(DisplayTopLeftX != other.DisplayTopLeftX)
             {
                 SharedLogger.logger.Trace($"EYEFINITY_GRID_NODE/Equals: The DisplayTopLeftX values don't equal each other");
                 return false;
             }
             if (DisplayTopLeftY != other.DisplayTopLeftY)
             {
                 SharedLogger.logger.Trace($"EYEFINITY_GRID_NODE/Equals: The DisplayTopLeftY values don't equal each other");
                 return false;
             }
             if (DisplayUniqueId != other.DisplayUniqueId)
             {
                 SharedLogger.logger.Trace($"EYEFINITY_GRID_NODE/Equals: The DisplayUniqueId values don't equal each other");
                 return false;
             }
             return true;
         }

         public override int GetHashCode()
         {
             return (Row, Column, DisplayOrientation, DisplayWidth, DisplayHeight, DisplayTopLeftX, DisplayTopLeftY, DisplayUniqueId).GetHashCode();
         }
         public static bool operator ==(EYEFINITY_GRID_NODE lhs, EYEFINITY_GRID_NODE rhs) => lhs.Equals(rhs);

         public static bool operator !=(EYEFINITY_GRID_NODE lhs, EYEFINITY_GRID_NODE rhs) => !(lhs == rhs);
     }*/

    [StructLayout(LayoutKind.Sequential)]
    public struct AMD_EYEFINITY_DESKTOP : IEquatable<AMD_EYEFINITY_DESKTOP>
    {
        public long Rows;
        public long Columns;
        public ADLX_ORIENTATION Orientation;
        public int SizeWidth;
        public int SizeHeight;
        public int TopLeftX;
        public int TopLeftY;
        //public EYEFINITY_GRID_NODE[][] Grid;

        public AMD_EYEFINITY_DESKTOP()
        {
            Orientation = ADLX_ORIENTATION.ORIENTATION_LANDSCAPE;
            //Grid = Array.Empty<EYEFINITY_GRID_NODE[]>();
        }

        public override bool Equals(object obj) => obj is AMD_EYEFINITY_DESKTOP other && this.Equals(other);
        public bool Equals(AMD_EYEFINITY_DESKTOP other)
        {
            if (Rows != other.Rows)
            {
                SharedLogger.logger.Trace($"AMD_EYEFINITY_DESKTOP/Rows: The Rows values don't equal each other");
                return false;
            }
            if (Columns != other.Columns)
            {
                SharedLogger.logger.Trace($"AMD_EYEFINITY_DESKTOP/Equals: The Columns values don't equal each other");
                return false;
            }
            if (Orientation != other.Orientation)
            {
                SharedLogger.logger.Trace($"AMD_EYEFINITY_DESKTOP/Equals: The Orientation values don't equal each other");
                return false;
            }
            if (SizeWidth != other.SizeWidth)
            {
                SharedLogger.logger.Trace($"AMD_EYEFINITY_DESKTOP/Equals: The SizeWidth values don't equal each other");
                return false;
            }
            if (SizeHeight != other.SizeHeight)
            {
                SharedLogger.logger.Trace($"AMD_EYEFINITY_DESKTOP/Equals: The SizeHeight values don't equal each other");
                return false;
            }
            if (TopLeftX != other.TopLeftX)
            {
                SharedLogger.logger.Trace($"AMD_EYEFINITY_DESKTOP/Equals: The TopLeftX values don't equal each other");
                return false;
            }
            if (TopLeftY != other.TopLeftY)
            {
                SharedLogger.logger.Trace($"AMD_EYEFINITY_DESKTOP/Equals: The TopLeftY values don't equal each other");
                return false;
            }
            /*if (Grid.SequenceEqual(other.Grid))
            {
                SharedLogger.logger.Trace($"AMD_EYEFINITY_DESKTOP/Equals: The Grid values don't equal each other");
                return false;
            }*/
            return true;
        }

        public override int GetHashCode()
        {
            return (Rows, Columns, Orientation, SizeWidth, SizeHeight, TopLeftX, TopLeftY).GetHashCode();
        }
        public static bool operator ==(AMD_EYEFINITY_DESKTOP lhs, AMD_EYEFINITY_DESKTOP rhs) => lhs.Equals(rhs);

        public static bool operator !=(AMD_EYEFINITY_DESKTOP lhs, AMD_EYEFINITY_DESKTOP rhs) => !(lhs == rhs);
    }

    public struct AMD_3DLUT_INFO : IEquatable<AMD_3DLUT_INFO>
    {
        public bool IsSupportedSCE { get; set; }
        public bool IsSupportedSCEVividGaming { get; set; }
        public bool IsSupportedSCEDynamicContrast { get; set; }
        public bool IsSupportedUser3DLUT { get; set; }

        public bool IsCurrentSCEDisabled { get; set; }
        public bool IsCurrentSCEVividGaming { get; set; }
        public bool HasDynamicContrast { get; set; }

        // If using Dynamic Contrast mode:
        public int CurrentDynamicContrastValue { get; set; }
        public IntRange DynamicContrastRange { get; set; }  // min, max, step

        public AMD_3DLUT_INFO()
        {
            IsSupportedSCE = false;
            IsSupportedSCEVividGaming = false;
            IsSupportedSCEDynamicContrast = false;
            IsSupportedUser3DLUT = false;
            IsCurrentSCEDisabled = false;
            IsCurrentSCEVividGaming = false;
            HasDynamicContrast = false;            
            CurrentDynamicContrastValue = 0;
            DynamicContrastRange = new IntRange();
        }

        public AMD_3DLUT_INFO(bool isSupportedSCE, bool isSupportedSCEVividGaming, bool isSupportedSCEDynamicContrast,
                             bool isSupportedUser3DLUT, bool isCurrentSCEDisabled, bool isCurrentSCEVividGaming,
                             bool hasDynamicContrast, int currentDynamicContrastValue, IntRange dynamicContrastRange)
        {
            IsSupportedSCE = isSupportedSCE;
            IsSupportedSCEVividGaming = isSupportedSCEVividGaming;
            IsSupportedSCEDynamicContrast = isSupportedSCEDynamicContrast;
            IsSupportedUser3DLUT = isSupportedUser3DLUT;
            IsCurrentSCEDisabled = isCurrentSCEDisabled;
            IsCurrentSCEVividGaming = isCurrentSCEVividGaming;
            HasDynamicContrast = hasDynamicContrast;
            CurrentDynamicContrastValue = currentDynamicContrastValue;
            DynamicContrastRange = dynamicContrastRange;
        }

        public AMD_3DLUT_INFO(ThreeDLUTDto threeDLUTDto)
        {
            IsSupportedSCE = threeDLUTDto.IsSceSupported;
            IsSupportedSCEVividGaming = threeDLUTDto.IsSceVividGamingSupported;
            IsSupportedSCEDynamicContrast = threeDLUTDto.IsSceDynamicContrastSupported;
            IsSupportedUser3DLUT = threeDLUTDto.IsUser3DLutSupported;
            IsCurrentSCEDisabled = threeDLUTDto.IsCurrentSceDisabled;
            IsCurrentSCEVividGaming = threeDLUTDto.IsCurrentSceVividGaming;
            HasDynamicContrast = threeDLUTDto.HasDynamicContrast;
            CurrentDynamicContrastValue = threeDLUTDto.CurrentDynamicContrast;
            DynamicContrastRange = new IntRange
            {
                Min = threeDLUTDto.DynamicContrastRange.MinValue,
                Max = threeDLUTDto.DynamicContrastRange.MaxValue,
                Step = threeDLUTDto.DynamicContrastRange.Step
            };
        }

        public ThreeDLUTDto ToThreeDLUTDto()
        {
            return new ThreeDLUTDto(
                isSceSupported: IsSupportedSCE,
                isSceVividGamingSupported: IsSupportedSCEVividGaming,
                isSceDynamicContrastSupported: IsSupportedSCEDynamicContrast,
                isUser3DLutSupported: IsSupportedUser3DLUT,
                isCurrentSceDisabled: IsCurrentSCEDisabled,
                isCurrentSceVividGaming: IsCurrentSCEVividGaming,
                hasDynamicContrast: HasDynamicContrast,
                currentDynamicContrast: CurrentDynamicContrastValue,
                dynamicContrastRange: new IntRangeDto(DynamicContrastRange.Min, DynamicContrastRange.Max, DynamicContrastRange.Step)
            );
        }

        public override bool Equals(object obj) => obj is AMD_3DLUT_INFO other && this.Equals(other);
        public bool Equals(AMD_3DLUT_INFO other)
        {
            if (IsSupportedSCE != other.IsSupportedSCE)
            {
                SharedLogger.logger.Trace($"AMD_3DLUT_INFO/Equals: The IsSupportedSCE values don't equal each other");
                return false;
            }
            if (IsSupportedSCEVividGaming != other.IsSupportedSCEVividGaming)
            {
                SharedLogger.logger.Trace($"AMD_3DLUT_INFO/Equals: The IsSupportedSCEVividGaming values don't equal each other");
                return false;
            }
            if (IsSupportedUser3DLUT != other.IsSupportedUser3DLUT)
            {
                SharedLogger.logger.Trace($"AMD_3DLUT_INFO/Equals: The IsSupportedUser3DLUT values don't equal each other");
                return false;
            }
            if (IsCurrentSCEDisabled != other.IsCurrentSCEDisabled)
            {
                SharedLogger.logger.Trace($"AMD_3DLUT_INFO/Equals: The IsCurrentSCEDisabled values don't equal each other");
                return false;
            }
            if (IsCurrentSCEVividGaming != other.IsCurrentSCEVividGaming)
            {
                SharedLogger.logger.Trace($"AMD_3DLUT_INFO/Equals: The IsCurrentSCEVividGaming values don't equal each other");
                return false;
            }
            if (CurrentDynamicContrastValue != other.CurrentDynamicContrastValue)
            {
                SharedLogger.logger.Trace($"AMD_3DLUT_INFO/Equals: The CurrentDynamicContrastValue values don't equal each other");
                return false;
            }
            if (!DynamicContrastRange.Equals(other.DynamicContrastRange))
            {
                SharedLogger.logger.Trace($"AMD_3DLUT_INFO/Equals: The DynamicContrastRange values don't equal each other");
                return false;
            }
            return true;
        }
        public override int GetHashCode() => (IsSupportedSCE, IsSupportedSCEVividGaming, IsSupportedSCEDynamicContrast, IsSupportedUser3DLUT, IsCurrentSCEDisabled, IsCurrentSCEVividGaming, CurrentDynamicContrastValue, DynamicContrastRange).GetHashCode();
    }

    /// Range helper
    public struct IntRange: IEquatable<IntRange>
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int Step { get; set; }
        public override bool Equals(object obj) => obj is IntRange other && this.Equals(other);
        public bool Equals(IntRange other)
        {
            if (Min != other.Min)
            {
                SharedLogger.logger.Trace($"IntRange/Equals: The Min values don't equal each other");
                return false;
            }
            if (Max != other.Max)
            {
                SharedLogger.logger.Trace($"IntRange/Equals: The Max values don't equal each other");
                return false;
            }
            if (Step != other.Step)
            {
                SharedLogger.logger.Trace($"IntRange/Equals: The Step values don't equal each other");
                return false;
            }
            return true;
        }
        public override int GetHashCode() => (Min, Max, Step).GetHashCode();
    }

    public enum LutMode
    {
        None = 0,
        SDR = 1,
        HDR = 2,
        All = 3
    }


    public struct AMD_CONNECTIVITY_EXPERIENCE_INFO : IEquatable<AMD_CONNECTIVITY_EXPERIENCE_INFO>
    {
        public bool IsHdmiQualityDetectionSupported { get; init; }
        public bool IsHdmiQualityDetectionEnabled { get; init; }
        public bool IsDpLinkRateSupported { get; init; }
        public ADLX_DP_LINK_RATE DpLinkRate { get; init; }
        public bool IsRelativePreEmphasisSupported { get; init; }
        public int RelativePreEmphasis { get; init; }
        public bool IsRelativeVoltageSwingSupported { get; init; }
        public int RelativeVoltageSwing { get; init; }

        public AMD_CONNECTIVITY_EXPERIENCE_INFO()
        {
            IsHdmiQualityDetectionSupported = false;
            IsHdmiQualityDetectionEnabled = false;
            IsDpLinkRateSupported = false;
            DpLinkRate = ADLX_DP_LINK_RATE.DP_LINK_RATE_UNKNOWN;
            IsRelativePreEmphasisSupported = false;
            RelativePreEmphasis = 0;
            IsRelativeVoltageSwingSupported = false;
            RelativeVoltageSwing = 0;
        }

        public AMD_CONNECTIVITY_EXPERIENCE_INFO(bool isHdmiQualityDetectionSupported, bool isHdmiQualityDetectionEnabled,
                                               bool isDpLinkRateSupported, ADLX_DP_LINK_RATE dpLinkRate,
                                               bool isRelativePreEmphasisSupported, int relativePreEmphasis,
                                               bool isRelativeVoltageSwingSupported, int relativeVoltageSwing)
        {
            IsHdmiQualityDetectionSupported = isHdmiQualityDetectionSupported;
            IsHdmiQualityDetectionEnabled = isHdmiQualityDetectionEnabled;
            IsDpLinkRateSupported = isDpLinkRateSupported;
            DpLinkRate = dpLinkRate;
            IsRelativePreEmphasisSupported = isRelativePreEmphasisSupported;
            RelativePreEmphasis = relativePreEmphasis;
            IsRelativeVoltageSwingSupported = isRelativeVoltageSwingSupported;
            RelativeVoltageSwing = relativeVoltageSwing;
        }

        public AMD_CONNECTIVITY_EXPERIENCE_INFO(ConnectivityExperienceDto ConnExp)
        {
            IsHdmiQualityDetectionSupported = ConnExp.IsHdmiQualityDetectionSupported;
            IsHdmiQualityDetectionEnabled = ConnExp.IsHdmiQualityDetectionEnabled;
            IsDpLinkRateSupported = ConnExp.IsDpLinkRateSupported;
            DpLinkRate = ConnExp.DpLinkRate;
            IsRelativePreEmphasisSupported = ConnExp.IsRelativePreEmphasisSupported;
            RelativePreEmphasis = ConnExp.RelativePreEmphasis;
            IsRelativeVoltageSwingSupported = ConnExp.IsRelativeVoltageSwingSupported;
            RelativeVoltageSwing = ConnExp.RelativeVoltageSwing;
        }

        public ConnectivityExperienceDto ToConnectivityExperienceDto()
        {
            return new ConnectivityExperienceDto(
                isHdmiQualityDetectionSupported: IsHdmiQualityDetectionSupported,
                isHdmiQualityDetectionEnabled: IsHdmiQualityDetectionEnabled,
                isDpLinkRateSupported: IsDpLinkRateSupported,
                dpLinkRate: DpLinkRate,
                isRelativePreEmphasisSupported: IsRelativePreEmphasisSupported,
                relativePreEmphasis: RelativePreEmphasis,
                isRelativeVoltageSwingSupported: IsRelativeVoltageSwingSupported,
                relativeVoltageSwing: RelativeVoltageSwing
            );
        }   

        public override bool Equals(object obj) => obj is AMD_CONNECTIVITY_EXPERIENCE_INFO other && this.Equals(other);
        public bool Equals(AMD_CONNECTIVITY_EXPERIENCE_INFO other)
        {
            if (IsHdmiQualityDetectionSupported != other.IsHdmiQualityDetectionSupported)
            {
                SharedLogger.logger.Trace($"AMD_CONNECTIVITY_EXPERIENCE_INFO/Equals: The IsHdmiQualityDetectionSupported values don't equal each other");
                return false;
            }
            if (IsHdmiQualityDetectionEnabled != other.IsHdmiQualityDetectionEnabled)
            {
                SharedLogger.logger.Trace($"AMD_CONNECTIVITY_EXPERIENCE_INFO/Equals: The IsHdmiQualityDetectionEnabled values don't equal each other");
                return false;
            }
            if (IsDpLinkRateSupported != other.IsDpLinkRateSupported)
            {
                SharedLogger.logger.Trace($"AMD_CONNECTIVITY_EXPERIENCE_INFO/Equals: The IsDpLinkRateSupported values don't equal each other");
                return false;
            }
            if (DpLinkRate != other.DpLinkRate)
            {
                SharedLogger.logger.Trace($"AMD_CONNECTIVITY_EXPERIENCE_INFO/Equals: The DpLinkRate values don't equal each other");
                return false;
            }
            if (IsRelativePreEmphasisSupported != other.IsRelativePreEmphasisSupported)
            {
                SharedLogger.logger.Trace($"AMD_CONNECTIVITY_EXPERIENCE_INFO/Equals: The IsRelativePreEmphasisSupported values don't equal each other");
                return false;
            }
            if (RelativePreEmphasis != other.RelativePreEmphasis)
            {
                SharedLogger.logger.Trace($"AMD_CONNECTIVITY_EXPERIENCE_INFO/Equals: The RelativePreEmphasis values don't equal each other");
                return false;
            }
            if (IsRelativeVoltageSwingSupported != other.IsRelativeVoltageSwingSupported)
            {
                SharedLogger.logger.Trace($"AMD_CONNECTIVITY_EXPERIENCE_INFO/Equals: The IsRelativeVoltageSwingSupported values don't equal each other");
                return false;
            }
            if (RelativeVoltageSwing != other.RelativeVoltageSwing)
            {
                SharedLogger.logger.Trace($"AMD_CONNECTIVITY_EXPERIENCE_INFO/Equals: The RelativeVoltageSwing values don't equal each other");
                return false;
            }
            return true;
        }
        public override int GetHashCode() => (IsHdmiQualityDetectionSupported, IsHdmiQualityDetectionEnabled, IsDpLinkRateSupported, DpLinkRate, 
        IsRelativePreEmphasisSupported, RelativePreEmphasis, IsRelativeVoltageSwingSupported, RelativeVoltageSwing).GetHashCode();
    }

    public struct AMD_GAMUT_COLOR_SPACE : IEquatable<AMD_GAMUT_COLOR_SPACE>
    {
        public int RedX { get; init; }
        public int RedY { get; init; }
        public int GreenX { get; init; }
        public int GreenY { get; init; }
        public int BlueX { get; init; }
        public int BlueY { get; init; }

        public AMD_GAMUT_COLOR_SPACE()
        {
            RedX = 0;
            RedY = 0;
            GreenX = 0;
            GreenY = 0;
            BlueX = 0;
            BlueY = 0;
        }

        public AMD_GAMUT_COLOR_SPACE(int redX, int redY, int greenX, int greenY, int blueX, int blueY)
        {
            RedX = redX;
            RedY = redY;
            GreenX = greenX;
            GreenY = greenY;
            BlueX = blueX;
            BlueY = blueY;
        }

        public AMD_GAMUT_COLOR_SPACE(GamutColorSpaceDto gamutColorSpace)
        {
            RedX = gamutColorSpace.Red.X;
            RedY = gamutColorSpace.Red.Y;
            GreenX = gamutColorSpace.Green.X;
            GreenY = gamutColorSpace.Green.Y;
            BlueX = gamutColorSpace.Blue.X;
            BlueY = gamutColorSpace.Blue.Y;
        }

        public GamutColorSpaceDto ToGamutColorSpaceDto()
        {
            return new GamutColorSpaceDto(new PointDto(RedX, RedY), new PointDto(GreenX, GreenY), new PointDto(BlueX, BlueY));
        }

        public override bool Equals(object obj) => obj is AMD_GAMUT_COLOR_SPACE other && this.Equals(other);
        public bool Equals(AMD_GAMUT_COLOR_SPACE other)
        {
            if (RedX != other.RedX)
            {
                SharedLogger.logger.Trace($"AMD_GAMUT_COLOR_SPACE/Equals: The RedX values don't equal each other");
                return false;
            }
            if (RedY != other.RedY)
            {
                SharedLogger.logger.Trace($"AMD_GAMUT_COLOR_SPACE/Equals: The RedY values don't equal each other");
                return false;
            }
            if (GreenX != other.GreenX)
            {
                SharedLogger.logger.Trace($"AMD_GAMUT_COLOR_SPACE/Equals: The GreenX values don't equal each other");
                return false;
            }
            if (GreenY != other.GreenY)
            {
                SharedLogger.logger.Trace($"AMD_GAMUT_COLOR_SPACE/Equals: The GreenY values don't equal each other");
                return false;
            }
            if (BlueX != other.BlueX)
            {
                SharedLogger.logger.Trace($"AMD_GAMUT_COLOR_SPACE/Equals: The BlueX values don't equal each other");
                return false;
            }
            if (BlueY != other.BlueY)
            {
                SharedLogger.logger.Trace($"AMD_GAMUT_COLOR_SPACE/Equals: The BlueY values don't equal each other");
                return false;
            }
            return true;
        }
        public override int GetHashCode() => (RedX, RedY, GreenX, GreenY, BlueX, BlueY).GetHashCode();
    }

    public struct AMD_GAMUT_INFO : IEquatable<AMD_GAMUT_INFO>
    {
        public bool IsWhitePointSupported { get; init; }
        public bool IsGamutSupported { get; init; }
        public bool IsCurrent5000K { get; init; }
        public bool IsCurrent6500K { get; init; }
        public bool IsCurrent7500K { get; init; }
        public bool IsCurrent9300K { get; init; }
        public bool IsCurrentCustomWhitePoint { get; init; }
        public bool IsCurrent709 { get; init; }
        public bool IsCurrent601 { get; init; }
        public bool IsCurrentAdobe { get; init; }
        public bool IsCurrentCieRgb { get; init; }
        public bool IsCurrent2020 { get; init; }
        public bool IsCurrentCustomColorSpace { get; init; }
        public AMD_GAMUT_COLOR_SPACE CurrentGamutSpace { get; init; }
        public int WhitePointX { get; init; }
        public int WhitePointY { get; init; }
        public bool HasWhitePoint { get; init; }

        public AMD_GAMUT_INFO()
        {
            IsWhitePointSupported = false;
            IsGamutSupported = false;
            IsCurrent5000K = false;
            IsCurrent6500K = false;
            IsCurrent7500K = false;
            IsCurrent9300K = false;
            IsCurrentCustomWhitePoint = false;
            IsCurrent709 = false;
            IsCurrent601 = false;
            IsCurrentAdobe = false;
            IsCurrentCieRgb = false;
            IsCurrent2020 = false;
            IsCurrentCustomColorSpace = false;
            CurrentGamutSpace = new AMD_GAMUT_COLOR_SPACE();
            WhitePointX = 0;
            WhitePointY = 0;
            HasWhitePoint = false;
        }

        public AMD_GAMUT_INFO(bool isWhitePointSupported, bool isGamutSupported,
                             bool isCurrent5000K, bool isCurrent6500K, bool isCurrent7500K, bool isCurrent9300K,
                             bool isCurrentCustomWhitePoint, bool isCurrent709, bool isCurrent601,
                             bool isCurrentAdobe, bool isCurrentCieRgb, bool isCurrent2020,
                             bool isCurrentCustomColorSpace, GamutColorSpaceDto currentGamutSpace,
                             PointDto whitePoint,  bool hasWhitePoint)
        {
            IsWhitePointSupported = isWhitePointSupported;
            IsGamutSupported = isGamutSupported;
            IsCurrent5000K = isCurrent5000K;
            IsCurrent6500K = isCurrent6500K;
            IsCurrent7500K = isCurrent7500K;
            IsCurrent9300K = isCurrent9300K;
            IsCurrentCustomWhitePoint = isCurrentCustomWhitePoint;
            IsCurrent709 = isCurrent709;
            IsCurrent601 = isCurrent601;
            IsCurrentAdobe = isCurrentAdobe;
            IsCurrentCieRgb = isCurrentCieRgb;
            IsCurrent2020 = isCurrent2020;
            IsCurrentCustomColorSpace = isCurrentCustomColorSpace;
            CurrentGamutSpace = new AMD_GAMUT_COLOR_SPACE(currentGamutSpace);
            WhitePointX = whitePoint.X;
            WhitePointY = whitePoint.Y;
            HasWhitePoint = hasWhitePoint;
        }

        public AMD_GAMUT_INFO(GamutDto gamutDto)
        {
            IsWhitePointSupported = gamutDto.IsWhitePointSupported;
            IsGamutSupported = gamutDto.IsGamutSupported;
            IsCurrent5000K = gamutDto.IsCurrent5000K;
            IsCurrent6500K = gamutDto.IsCurrent6500K;
            IsCurrent7500K = gamutDto.IsCurrent7500K;
            IsCurrent9300K = gamutDto.IsCurrent9300K;
            IsCurrentCustomWhitePoint = gamutDto.IsCurrentCustomWhitePoint;
            IsCurrent709 = gamutDto.IsCurrent709;
            IsCurrent601 = gamutDto.IsCurrent601;
            IsCurrentAdobe = gamutDto.IsCurrentAdobe;
            IsCurrentCieRgb = gamutDto.IsCurrentCieRgb;
            IsCurrent2020 = gamutDto.IsCurrent2020;
            IsCurrentCustomColorSpace = gamutDto.IsCurrentCustomColorSpace;
            CurrentGamutSpace = new AMD_GAMUT_COLOR_SPACE(gamutDto.CurrentGamutSpace);
            WhitePointX = gamutDto.WhitePoint.X;
            WhitePointY = gamutDto.WhitePoint.Y;
            HasWhitePoint = gamutDto.HasWhitePoint;
        }

        public GamutDto ToGamutDto()
        {
            return new GamutDto(
                isWhitePointSupported: IsWhitePointSupported,
                isGamutSupported: IsGamutSupported,
                isCurrent5000K: IsCurrent5000K,
                isCurrent6500K: IsCurrent6500K,
                isCurrent7500K: IsCurrent7500K,
                isCurrent9300K: IsCurrent9300K,
                isCurrentCustomWhitePoint: IsCurrentCustomWhitePoint,
                isCurrent709: IsCurrent709,
                isCurrent601: IsCurrent601,
                isCurrentAdobe: IsCurrentAdobe,
                isCurrentCieRgb: IsCurrentCieRgb,
                isCurrent2020: IsCurrent2020,
                isCurrentCustomColorSpace: IsCurrentCustomColorSpace,
                currentGamutSpace: CurrentGamutSpace.ToGamutColorSpaceDto(),
                whitePoint: new PointDto(WhitePointX, WhitePointY),
                hasWhitePoint: HasWhitePoint
            );
        }

        public override bool Equals(object obj) => obj is AMD_GAMUT_INFO other && this.Equals(other);
        public bool Equals(AMD_GAMUT_INFO other)
        {
            if (IsWhitePointSupported != other.IsWhitePointSupported)
            {
                SharedLogger.logger.Trace($"AMD_GAMUT_INFO/Equals: The IsWhitePointSupported values don't equal each other");
                return false;
            }
            if (IsGamutSupported != other.IsGamutSupported)
            {
                SharedLogger.logger.Trace($"AMD_GAMUT_INFO/Equals: The IsGamutSupported values don't equal each other");
                return false;
            }
            if (IsCurrent5000K != other.IsCurrent5000K)
            {
                SharedLogger.logger.Trace($"AMD_GAMUT_INFO/Equals: The IsCurrent5000K values don't equal each other");
                return false;
            }
            if (IsCurrent6500K != other.IsCurrent6500K)
            {
                SharedLogger.logger.Trace($"AMD_GAMUT_INFO/Equals: The IsCurrent6500K values don't equal each other");
                return false;
            }
            if (IsCurrent7500K != other.IsCurrent7500K)
            {
                SharedLogger.logger.Trace($"AMD_GAMUT_INFO/Equals: The IsCurrent7500K values don't equal each other");
                return false;
            }
            if (IsCurrent9300K != other.IsCurrent9300K)
            {
                SharedLogger.logger.Trace($"AMD_GAMUT_INFO/Equals: The IsCurrent9300K values don't equal each other");
                return false;
            }
            return true;
        }

        public override int GetHashCode() => (IsWhitePointSupported, IsGamutSupported, IsCurrent5000K, IsCurrent6500K, IsCurrent7500K, IsCurrent9300K).GetHashCode();
    }

    public struct AMD_REGAMMA_COEFFICIENT : IEquatable<AMD_REGAMMA_COEFFICIENT>
    {
        public int coefficientA0;
        public int coefficientA1;
        public int coefficientA2;
        public int coefficientA3;
        public int gamma;

        public AMD_REGAMMA_COEFFICIENT()
        {
            coefficientA0 = 0;
            coefficientA1 = 0;
            coefficientA2 = 0;
            coefficientA3 = 0;
            gamma = 0;
        }

        public AMD_REGAMMA_COEFFICIENT(int a0, int a1, int a2, int a3, int gammaValue)
        {
            coefficientA0 = a0;
            coefficientA1 = a1;
            coefficientA2 = a2;
            coefficientA3 = a3;
            gamma = gammaValue;
        }

        public AMD_REGAMMA_COEFFICIENT(RegammaCoeffDto regammaCoeffDto)
        {
            coefficientA0 = regammaCoeffDto.CoefficientA0;
            coefficientA1 = regammaCoeffDto.CoefficientA1;
            coefficientA2 = regammaCoeffDto.CoefficientA2;
            coefficientA3 = regammaCoeffDto.CoefficientA3;
            gamma = regammaCoeffDto.Gamma;
        }

        public RegammaCoeffDto ToRegammaCoeffDto()
        {
            return new RegammaCoeffDto(coefficientA0, coefficientA1, coefficientA2, coefficientA3, gamma);
        }

        public override bool Equals(object obj) => obj is AMD_REGAMMA_COEFFICIENT other && this.Equals(other);
        public bool Equals(AMD_REGAMMA_COEFFICIENT other)
        {
            if (coefficientA0 != other.coefficientA0)
            {
                SharedLogger.logger.Trace($"AMD_REGAMMA_COEFFICIENT/Equals: The coefficientA0 values don't equal each other");
                return false;
            }   
            if (coefficientA1 != other.coefficientA1)
            {
                SharedLogger.logger.Trace($"AMD_REGAMMA_COEFFICIENT/Equals: The coefficientA1 values don't equal each other");
                return false;
            }
            if (coefficientA2 != other.coefficientA2)
            {
                SharedLogger.logger.Trace($"AMD_REGAMMA_COEFFICIENT/Equals: The coefficientA2 values don't equal each other");
                return false;
            }
            if (coefficientA3 != other.coefficientA3)
            {
                SharedLogger.logger.Trace($"AMD_REGAMMA_COEFFICIENT/Equals: The coefficientA3 values don't equal each other");
                return false;
            }
            if (gamma != other.gamma)
            {
                SharedLogger.logger.Trace($"AMD_REGAMMA_COEFFICIENT/Equals: The gamma values don't equal each other");
                return false;
            }
            return true;
        }

        public override int GetHashCode() => (coefficientA0, coefficientA1, coefficientA2, coefficientA3, gamma).GetHashCode();
    }

    public struct AMD_GAMMA_RAMP : IEquatable<AMD_GAMMA_RAMP>
    {
        public List<ushort> Gamma { get; init; }        
        public AMD_GAMMA_RAMP()
        {
            Gamma = new List<ushort>();
        }

        public AMD_GAMMA_RAMP(List<ushort> gammaValues)
        {
            Gamma = gammaValues;
        }

        public AMD_GAMMA_RAMP(GammaRampDto gammaRampDto)
        {
            Gamma = new List<ushort>(gammaRampDto.Values);
        }

        public GammaRampDto ToGammaRampDto()
        {
            return new GammaRampDto(Gamma);
        }

        public override bool Equals(object obj) => obj is AMD_GAMMA_RAMP other && this.Equals(other);
        public bool Equals(AMD_GAMMA_RAMP other)
        {
            if (Gamma.Count != other.Gamma.Count)
            {
                SharedLogger.logger.Trace($"AMD_GAMMA_RAMP/Equals: The gamma Count values don't equal each other");
                return false;
            }
            for (int i = 0; i < Gamma.Count; i++)
            {
                if (Gamma[i] != other.Gamma[i])
                {
                    SharedLogger.logger.Trace($"AMD_GAMMA_RAMP/Equals: The gamma values at index {i} don't equal each other");
                    return false;
                }
            }
            return true;
        }
        public override int GetHashCode() => (Gamma).GetHashCode();
    }

    public struct AMD_GAMMA_INFO : IEquatable<AMD_GAMMA_INFO>
    {
        public bool IsSupported { get; init; }
        public bool IsCurrentReGammaSRGB { get; init; }
        public bool IsCurrentReGammaBT709 { get; init; }
        public bool IsCurrentReGammaPQ { get; init; }
        public bool IsCurrentReGammaPQ2084 { get; init; }
        public bool IsCurrentReGamma36 { get; init; }
        public bool HasRegammaCoefficient { get; init; }
        public AMD_REGAMMA_COEFFICIENT RegammaCoefficient { get; init; }
        public bool HasReGammaRamp { get; init; }
        public AMD_GAMMA_RAMP ReGammaRamp { get; init; }
        public bool HasDeGammaRamp { get; init; }
        public AMD_GAMMA_RAMP DeGammaRamp { get; init; }

        public AMD_GAMMA_INFO()
        {
            IsSupported = false;
            IsCurrentReGammaSRGB = false;
            IsCurrentReGammaBT709 = false;
            IsCurrentReGammaPQ = false;
            IsCurrentReGammaPQ2084 = false;
            IsCurrentReGamma36 = false;
            HasRegammaCoefficient = false;
            RegammaCoefficient = new AMD_REGAMMA_COEFFICIENT();
            HasReGammaRamp = false;
            ReGammaRamp = new AMD_GAMMA_RAMP();
            HasDeGammaRamp = false;
            DeGammaRamp = new AMD_GAMMA_RAMP();
        }

        public AMD_GAMMA_INFO(bool isSupported, bool isCurrentReGammaSRGB, bool isCurrentReGammaBT709,
                             bool isCurrentReGammaPQ, bool isCurrentReGammaPQ2084, bool isCurrentReGamma36,
                             bool hasRegammaCoefficient, RegammaCoeffDto regammaCoefficient,
                             bool hasReGammaRamp, GammaRampDto reGammaRamp,
                             bool hasDeGammaRamp, GammaRampDto deGammaRamp)
        {
            IsSupported = isSupported;
            IsCurrentReGammaSRGB = isCurrentReGammaSRGB;
            IsCurrentReGammaBT709 = isCurrentReGammaBT709;
            IsCurrentReGammaPQ = isCurrentReGammaPQ;
            IsCurrentReGammaPQ2084 = isCurrentReGammaPQ2084;
            IsCurrentReGamma36 = isCurrentReGamma36;
            HasRegammaCoefficient = hasRegammaCoefficient;
            RegammaCoefficient = new AMD_REGAMMA_COEFFICIENT(regammaCoefficient);
            HasReGammaRamp = hasReGammaRamp;
            ReGammaRamp = new AMD_GAMMA_RAMP(reGammaRamp);
            HasDeGammaRamp = hasDeGammaRamp;
            DeGammaRamp = new AMD_GAMMA_RAMP(deGammaRamp);
        }

        public AMD_GAMMA_INFO(GammaDto gammaDto)
        {
            IsSupported = gammaDto.IsSupported;
            IsCurrentReGammaSRGB = gammaDto.IsCurrentReGammaSRGB;
            IsCurrentReGammaBT709 = gammaDto.IsCurrentReGammaBT709;
            IsCurrentReGammaPQ = gammaDto.IsCurrentReGammaPQ;
            IsCurrentReGammaPQ2084 = gammaDto.IsCurrentReGammaPQ2084;
            IsCurrentReGamma36 = gammaDto.IsCurrentReGamma36;
            HasRegammaCoefficient = gammaDto.HasRegammaCoefficient;
            RegammaCoefficient = new AMD_REGAMMA_COEFFICIENT(gammaDto.RegammaCoefficient);
            HasReGammaRamp = gammaDto.HasReGammaRamp;
            ReGammaRamp = new AMD_GAMMA_RAMP(gammaDto.ReGammaRamp);
            HasDeGammaRamp = gammaDto.HasDeGammaRamp;
            DeGammaRamp = new AMD_GAMMA_RAMP(gammaDto.DeGammaRamp);
        }

        public GammaDto ToGammaDto()
        {
            return new GammaDto(
                isSupported: IsSupported,
                isCurrentReGammaSRGB: IsCurrentReGammaSRGB,
                isCurrentReGammaBT709: IsCurrentReGammaBT709,
                isCurrentReGammaPQ: IsCurrentReGammaPQ,
                isCurrentReGammaPQ2084: IsCurrentReGammaPQ2084,
                isCurrentReGamma36: IsCurrentReGamma36,
                hasRegammaCoefficient: HasRegammaCoefficient,
                regammaCoefficient: RegammaCoefficient.ToRegammaCoeffDto(),
                hasReGammaRamp: HasReGammaRamp,
                reGammaRamp: ReGammaRamp.ToGammaRampDto(),
                hasDeGammaRamp: HasDeGammaRamp,
                deGammaRamp: DeGammaRamp.ToGammaRampDto()
            );
        }

        public override bool Equals(object obj) => obj is AMD_GAMMA_INFO other && this.Equals(other);
        public bool Equals(AMD_GAMMA_INFO other)
        {
            if (IsSupported != other.IsSupported)
            {
                SharedLogger.logger.Trace($"AMD_GAMMA_INFO/Equals: The IsSupported values don't equal each other");
                return false;
            }
            if (IsCurrentReGammaSRGB != other.IsCurrentReGammaSRGB)
            {
                SharedLogger.logger.Trace($"AMD_GAMMA_INFO/Equals: The IsCurrentReGammaSRGB values don't equal each other");
                return false;
            }
            if (IsCurrentReGammaBT709 != other.IsCurrentReGammaBT709)
            {
                SharedLogger.logger.Trace($"AMD_GAMMA_INFO/Equals: The IsCurrentReGammaBT709 values don't equal each other");
                return false;
            }
            if (IsCurrentReGammaPQ != other.IsCurrentReGammaPQ)
            {
                SharedLogger.logger.Trace($"AMD_GAMMA_INFO/Equals: The IsCurrentReGammaPQ values don't equal each other");
                return false;
            }
            if (IsCurrentReGammaPQ2084 != other.IsCurrentReGammaPQ2084)
            {
                SharedLogger.logger.Trace($"AMD_GAMMA_INFO/Equals: The IsCurrentReGammaPQ2084 values don't equal each other");
                return false;
            }
            if (IsCurrentReGamma36 != other.IsCurrentReGamma36)
            {
                SharedLogger.logger.Trace($"AMD_GAMMA_INFO/Equals: The IsCurrentReGamma36 values don't equal each other");
                return false;
            }
            if (HasRegammaCoefficient != other.HasRegammaCoefficient)
            {
                SharedLogger.logger.Trace($"AMD_GAMMA_INFO/Equals: The HasRegammaCoefficient values don't equal each other");
                return false;
            }
            if (!RegammaCoefficient.Equals(other.RegammaCoefficient))
            {
                SharedLogger.logger.Trace($"AMD_GAMMA_INFO/Equals: The RegammaCoefficient values don't equal each other");
                return false;
            }
            if (HasReGammaRamp != other.HasReGammaRamp)
            {
                SharedLogger.logger.Trace($"AMD_GAMMA_INFO/Equals: The HasReGammaRamp values don't equal each other");
                return false;
            }
            if (!ReGammaRamp.Equals(other.ReGammaRamp))
            {
                SharedLogger.logger.Trace($"AMD_GAMMA_INFO/Equals: The ReGammaRamp values don't equal each other");
                return false;
            }
            if (HasDeGammaRamp != other.HasDeGammaRamp)
            {
                SharedLogger.logger.Trace($"AMD_GAMMA_INFO/Equals: The HasDeGammaRamp values don't equal each other");
                return false;
            }
            if (!DeGammaRamp.Equals(other.DeGammaRamp))
            {
                SharedLogger.logger.Trace($"AMD_GAMMA_INFO/Equals: The DeGammaRamp values don't equal each other");
                return false;
            }
            return true;
        }
        public override int GetHashCode() => (IsSupported, IsCurrentReGammaSRGB, IsCurrentReGammaBT709, IsCurrentReGammaPQ, IsCurrentReGammaPQ2084, 
            IsCurrentReGamma36, HasRegammaCoefficient, RegammaCoefficient, HasReGammaRamp, ReGammaRamp, HasDeGammaRamp, DeGammaRamp).GetHashCode();
    }

    public struct AMD_CUSTOM_COLOR_INFO : IEquatable<AMD_CUSTOM_COLOR_INFO>
    {
        public bool IsSupported { get; init; }
        public bool IsHueSupported { get; init; }
        public int Hue { get; init; }
        public bool IsSaturationSupported { get; init; }
        public int Saturation { get; init; }
        public bool IsBrightnessSupported { get; init; }
        public int Brightness { get; init; }
        public bool IsContrastSupported { get; init; }
        public int Contrast { get; init; }
        public bool IsTemperatureSupported { get; init; }
        public int Temperature { get; init; }

        public AMD_CUSTOM_COLOR_INFO()
        {
            IsSupported = false;
            IsHueSupported = false;
            Hue = 0;
            IsSaturationSupported = false;
            Saturation = 0;
            IsBrightnessSupported = false;
            Brightness = 0;
            IsContrastSupported = false;
            Contrast = 0;
            IsTemperatureSupported = false;
            Temperature = 0;
        }
        public AMD_CUSTOM_COLOR_INFO(bool isSupported, bool isHueSupported, int hue,
                                    bool isSaturationSupported, int saturation,
                                    bool isBrightnessSupported, int brightness,
                                    bool isContrastSupported, int contrast,
                                    bool isTemperatureSupported, int temperature)
        {
            IsSupported = isSupported;
            IsHueSupported = isHueSupported;
            Hue = hue;
            IsSaturationSupported = isSaturationSupported;
            Saturation = saturation;
            IsBrightnessSupported = isBrightnessSupported;
            Brightness = brightness;
            IsContrastSupported = isContrastSupported;
            Contrast = contrast;
            IsTemperatureSupported = isTemperatureSupported;
            Temperature = temperature;
        }
        public AMD_CUSTOM_COLOR_INFO(CustomColorDto customColorDto)
        {
            IsSupported = customColorDto.IsSupported;
            IsHueSupported = customColorDto.IsHueSupported;
            Hue = customColorDto.Hue;
            IsSaturationSupported = customColorDto.IsSaturationSupported;
            Saturation = customColorDto.Saturation;
            IsBrightnessSupported = customColorDto.IsBrightnessSupported;
            Brightness = customColorDto.Brightness;
            IsContrastSupported = customColorDto.IsContrastSupported;
            Contrast = customColorDto.Contrast;
            IsTemperatureSupported = customColorDto.IsTemperatureSupported;
            Temperature = customColorDto.Temperature;
        }
        public CustomColorDto ToCustomColorDto()
        {
            return new CustomColorDto(
                isSupported: IsSupported,
                isHueSupported: IsHueSupported,
                hue: Hue,
                isSaturationSupported: IsSaturationSupported,
                saturation: Saturation,
                isBrightnessSupported: IsBrightnessSupported,
                brightness: Brightness,
                isContrastSupported: IsContrastSupported,
                contrast: Contrast,
                isTemperatureSupported: IsTemperatureSupported,
                temperature: Temperature
            );
        }   
        public override bool Equals(object obj) => obj is AMD_CUSTOM_COLOR_INFO other && this.Equals(other);
        public bool Equals(AMD_CUSTOM_COLOR_INFO other)
        {
            if (IsSupported != other.IsSupported)
            {
                SharedLogger.logger.Trace($"AMD_CUSTOM_COLOR_INFO/Equals: The IsSupported values don't equal each other");
                return false;
            }
            if (IsHueSupported != other.IsHueSupported)
            {
                SharedLogger.logger.Trace($"AMD_CUSTOM_COLOR_INFO/Equals: The IsHueSupported values don't equal each other");
                return false;
            }
            if (Hue != other.Hue)
            {
                SharedLogger.logger.Trace($"AMD_CUSTOM_COLOR_INFO/Equals: The Hue values don't equal each other");
                return false;
            }
            if (IsSaturationSupported != other.IsSaturationSupported)
            {
                SharedLogger.logger.Trace($"AMD_CUSTOM_COLOR_INFO/Equals: The IsSaturationSupported values don't equal each other");
                return false;
            }
            if (Saturation != other.Saturation)
            {
                SharedLogger.logger.Trace($"AMD_CUSTOM_COLOR_INFO/Equals: The Saturation values don't equal each other");
                return false;
            }
            if (IsBrightnessSupported != other.IsBrightnessSupported)
            {
                SharedLogger.logger.Trace($"AMD_CUSTOM_COLOR_INFO/Equals: The IsBrightnessSupported values don't equal each other");
                return false;
            }
            if (Brightness != other.Brightness)
            {
                SharedLogger.logger.Trace($"AMD_CUSTOM_COLOR_INFO/Equals: The Brightness values don't equal each other");
                return false;
            }
            if (IsContrastSupported != other.IsContrastSupported)
            {
                SharedLogger.logger.Trace($"AMD_CUSTOM_COLOR_INFO/Equals: The IsContrastSupported values don't equal each other");
                return false;
            }
            if (Contrast != other.Contrast)
            {
                SharedLogger.logger.Trace($"AMD_CUSTOM_COLOR_INFO/Equals: The Contrast values don't equal each other");
                return false;
            }
            if (IsTemperatureSupported != other.IsTemperatureSupported)
            {
                SharedLogger.logger.Trace($"AMD_CUSTOM_COLOR_INFO/Equals: The IsTemperatureSupported values don't equal each other");
                return false;
            }
            if (Temperature != other.Temperature)
            {
                SharedLogger.logger.Trace($"AMD_CUSTOM_COLOR_INFO/Equals: The Temperature values don't equal each other");
                return false;
            }
            return true;
        }
        public override int GetHashCode() => (IsSupported, IsHueSupported, Hue, IsSaturationSupported, Saturation,
            IsBrightnessSupported, Brightness, IsContrastSupported, Contrast, IsTemperatureSupported, Temperature).GetHashCode();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AMD_DISPLAY_WITH_SETTINGS : IEquatable<AMD_DISPLAY_WITH_SETTINGS>
    {
        public ADLX_DISPLAY_CONNECTOR_TYPE ConnectorType;
        public ADLX_DISPLAY_TYPE Type;
        public string EDID;
        public uint ManufacturerID;
        public string Name;
        public int NativeResolutionWidth;
        public int NativeResolutionHeight;
        public uint PixelClock;
        public double RefreshRate;
        public ADLX_DISPLAY_SCAN_TYPE ScanType;
        public ulong UniqueID;
        public int GpuUniqueID;
        public bool IsSupportedColorDepth;
        public ADLX_COLOR_DEPTH ColorDepth;
        public AMD_CUSTOM_COLOR_INFO CustomColorInfo;
        public bool IsSupportedFreeSync;
        public bool IsEnabledFreeSync;        
        public bool IsSupportedFreeSyncColorAccuracy;
        public bool IsEnabledFreeSyncColorAccuracy;        
        public bool IsSupportedDynamicRefreshRateControl;
        public bool IsEnabledDynamicRefreshRateControl;
        public bool IsSupportedDisplayBlanking;
        public bool IsEnabledDisplayBlanking;
        public AMD_GAMUT_INFO GamutInfo;
        public AMD_GAMMA_INFO GammaInfo;
        public bool IsSupportedGPUScaling;
        public bool IsEnabledGPUScaling;
        public bool IsSupportedIntegerScaling;
        public bool IsEnabledIntegerScaling;
        public bool IsSupportedPixelFormat;
        public ADLX_PIXEL_FORMAT CurrentPixelFormat = ADLX_PIXEL_FORMAT.FORMAT_UNKNOWN;
        public bool IsSupportedScalingMode;
        public ADLX_SCALE_MODE CurrentScalingMode = ADLX_SCALE_MODE.PRESERVE_ASPECT_RATIO;
        public bool IsSupportedVSR = false;
        public bool IsEnabledVSR = false;
        public bool IsSupportedHDCP = false;
        public bool IsEnabledHDCP = false;
        public bool IsSupportedVariBright = false;
        public bool IsEnabledVariBright = false;
        public VariBrightMode VariBrightMode = VariBrightMode.Unknown;
        public AMD_CONNECTIVITY_EXPERIENCE_INFO ConnectivityExperience;
        public AMD_3DLUT_INFO ThreeDLUTSettings { get; set; } = new AMD_3DLUT_INFO();

        public AMD_DISPLAY_WITH_SETTINGS()
        {
            EDID = "";
            Name = "";
            ScanType = ADLX_DISPLAY_SCAN_TYPE.PROGRESSIVE;
            ConnectorType = ADLX_DISPLAY_CONNECTOR_TYPE.DISPLAY_CONTYPE_UNKNOWN;
            Type = ADLX_DISPLAY_TYPE.DISPLAY_TYPE_UNKOWN;
            ColorDepth = ADLX_COLOR_DEPTH.BPC_UNKNOWN;
            CurrentPixelFormat = ADLX_PIXEL_FORMAT.FORMAT_UNKNOWN;
            CurrentScalingMode = ADLX_SCALE_MODE.PRESERVE_ASPECT_RATIO;
            ThreeDLUTSettings = new AMD_3DLUT_INFO();
            GamutInfo = new AMD_GAMUT_INFO();
            VariBrightMode = VariBrightMode.Unknown;
        }

        public override bool Equals(object obj) => obj is AMD_DISPLAY_WITH_SETTINGS other && this.Equals(other);
        public bool Equals(AMD_DISPLAY_WITH_SETTINGS other)
        {
            if (ConnectorType != other.ConnectorType)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The ConnectorType values don't equal each other");
                return false;
            }
            if (Type != other.Type)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The DisplayType values don't equal each other");
                return false;
            }
            if (EDID != other.EDID)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The EDID values don't equal each other");
                return false;
            }
            if (ManufacturerID != other.ManufacturerID)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The ManufacturerID values don't equal each other");
                return false;
            }
            if (Name != other.Name)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The Name values don't equal each other");
                return false;
            }
            if (NativeResolutionWidth != other.NativeResolutionWidth)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The NativeResolutionWidth values don't equal each other");
                return false;
            }
            if (NativeResolutionHeight != other.NativeResolutionHeight)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The NativeResolutionHeight values don't equal each other");
                return false;
            }
            if (PixelClock != other.PixelClock)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The PixelClock values don't equal each other");
                return false;
            }
            if (RefreshRate != other.RefreshRate)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The RefreshRate values don't equal each other");
                return false;
            }
            if (ScanType != other.ScanType)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The ScanType values don't equal each other");
                return false;
            }
            if (UniqueID != other.UniqueID)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The UniqueID values don't equal each other");
                return false;
            }
            if (IsSupportedColorDepth != other.IsSupportedColorDepth)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedColorDepth values don't equal each other");
                return false;
            }
            if (ColorDepth != other.ColorDepth)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The ColorDepth values don't equal each other");
                return false;
            }
            if (!CustomColorInfo.Equals(other.CustomColorInfo))
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The CustomColorInfo values don't equal each other");
                return false;
            }
            if (IsSupportedFreeSync != other.IsSupportedFreeSync)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedFreeSync values don't equal each other");
                return false;
            }
            if (IsEnabledFreeSync != other.IsEnabledFreeSync)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The IsEnabledFreeSync values don't equal each other");
                return false;
            }
            if (IsSupportedGPUScaling != other.IsSupportedGPUScaling)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedGPUScaling values don't equal each other");
                return false;
            }
            if (IsEnabledGPUScaling != other.IsEnabledGPUScaling)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The IsEnabledGPUScaling values don't equal each other");
                return false;
            }
            if (IsSupportedIntegerScaling != other.IsSupportedIntegerScaling)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedIntegerScaling values don't equal each other");
                return false;
            }
            if (IsEnabledIntegerScaling != other.IsEnabledIntegerScaling)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The IsEnabledIntegerScaling values don't equal each other");
                return false;
            }
            if (IsSupportedPixelFormat != other.IsSupportedPixelFormat)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedPixelFormat values don't equal each other");
                return false;
            }
            if (CurrentPixelFormat != other.CurrentPixelFormat)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The CurrentPixelFormat values don't equal each other");
                return false;
            }
            if (IsSupportedScalingMode != other.IsSupportedScalingMode)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedScalingMode values don't equal each other");
                return false;
            }
            if (CurrentScalingMode != other.CurrentScalingMode)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The CurrentScalingMode values don't equal each other");
                return false;
            }
            if (IsSupportedVSR != other.IsSupportedVSR)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedVSR values don't equal each other");
                return false;
            }
            if (IsEnabledVSR != other.IsEnabledVSR)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The IsEnabledVSR values don't equal each other");
                return false;
            }             
            if (!ConnectivityExperience.Equals(other.ConnectivityExperience))
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The ConnectivityExperience values don't equal each other");
                return false;
            }
            if (!ThreeDLUTSettings.Equals(other.ThreeDLUTSettings))
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The ThreeDLUTSettings values don't equal each other");
                return false;
            }
            if (!GamutInfo.Equals(other.GamutInfo))
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The GamutInfo values don't equal each other");
                return false;
            }
            if (!GammaInfo.Equals(other.GammaInfo))
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_WITH_SETTINGS/Equals: The GammaInfo values don't equal each other");
                return false;
            }
            return true;
        }

        // Replace the GetHashCode method in AMD_DISPLAY_WITH_SETTINGS with the following:
        public override int GetHashCode()
        {
            return (ConnectorType, Type, EDID, ManufacturerID, Name, NativeResolutionWidth, NativeResolutionHeight, PixelClock, RefreshRate, ScanType, UniqueID,
                IsSupportedColorDepth, ColorDepth, CustomColorInfo, IsSupportedFreeSync, IsEnabledFreeSync, IsSupportedGPUScaling, IsEnabledGPUScaling,
                IsSupportedIntegerScaling, IsEnabledIntegerScaling, IsSupportedPixelFormat, CurrentPixelFormat, IsSupportedScalingMode, CurrentScalingMode,
                IsSupportedVSR, IsEnabledVSR, ConnectivityExperience, ThreeDLUTSettings, GamutInfo, GammaInfo
            ).GetHashCode();
        }
        public static bool operator ==(AMD_DISPLAY_WITH_SETTINGS lhs, AMD_DISPLAY_WITH_SETTINGS rhs) => lhs.Equals(rhs);

        public static bool operator !=(AMD_DISPLAY_WITH_SETTINGS lhs, AMD_DISPLAY_WITH_SETTINGS rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AMD_DISPLAY : IEquatable<AMD_DISPLAY>
    {
       public ADLX_DISPLAY_CONNECTOR_TYPE ConnectorType;
        public ADLX_DISPLAY_TYPE Type;
        public string EDID;
        public uint ManufacturerID;
        public string Name;
        public int NativeResolutionWidth;
        public int NativeResolutionHeight;
        public uint PixelClock;
        public double RefreshRate;
        public ADLX_DISPLAY_SCAN_TYPE ScanType;
        public ulong UniqueID;
        public int GpuUniqueID;

        public AMD_DISPLAY()
        {
            EDID = "";
            Name = "";
            ScanType = ADLX_DISPLAY_SCAN_TYPE.PROGRESSIVE;
            ConnectorType = ADLX_DISPLAY_CONNECTOR_TYPE.DISPLAY_CONTYPE_UNKNOWN;
            Type = ADLX_DISPLAY_TYPE.DISPLAY_TYPE_UNKOWN;
        }

        public override bool Equals(object obj) => obj is AMD_DISPLAY other && this.Equals(other);
        public bool Equals(AMD_DISPLAY other)
        {
            if (ConnectorType != other.ConnectorType)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY/Equals: The ConnectorType values don't equal each other");
                return false;
            }
            if (Type != other.Type)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY/Equals: The DisplayType values don't equal each other");
                return false;
            }
            if (EDID != other.EDID)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY/Equals: The EDID values don't equal each other");
                return false;
            }
            if (ManufacturerID != other.ManufacturerID)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY/Equals: The ManufacturerID values don't equal each other");
                return false;
            }
            if (Name != other.Name)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY/Equals: The Name values don't equal each other");
                return false;
            }
            if (NativeResolutionWidth  != other.NativeResolutionWidth)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY/Equals: The NativeResolutionWidth values don't equal each other");
                return false;
            }
            if (NativeResolutionHeight != other.NativeResolutionHeight)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY/Equals: The NativeResolutionHeight values don't equal each other");
                return false;
            }
            if (PixelClock != other.PixelClock)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY/Equals: The PixelClock values don't equal each other");
                return false;
            }
            if (RefreshRate != other.RefreshRate)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY/Equals: The RefreshRate values don't equal each other");
                return false;
            }
            if (ScanType != other.ScanType)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY/Equals: The ScanType values don't equal each other");
                return false;
            }
            if (UniqueID != other.UniqueID)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY/Equals: The UniqueID values don't equal each other");
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (ConnectorType, Type, EDID, ManufacturerID, Name, NativeResolutionWidth, NativeResolutionHeight, PixelClock, RefreshRate, ScanType, UniqueID).GetHashCode();
        }
        public static bool operator ==(AMD_DISPLAY lhs, AMD_DISPLAY rhs) => lhs.Equals(rhs);

        public static bool operator !=(AMD_DISPLAY lhs, AMD_DISPLAY rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AMD_GPU_WITH_SETTINGS : IEquatable<AMD_GPU_WITH_SETTINGS>
    {
        public All3DSettingsDto? ThreeDSettings;
        public VideoUpscaleDto? VideoUpscale;
        public VideoSuperResolutionDto? VideoSuperResolution;

        public AMD_GPU_WITH_SETTINGS()
        {
            ThreeDSettings = null;
            VideoUpscale = null;
            VideoSuperResolution = null;
        }

        public override bool Equals(object obj) => obj is AMD_GPU_WITH_SETTINGS other && this.Equals(other);

        public bool Equals(AMD_GPU_WITH_SETTINGS other)
        {
            if (!Nullable.Equals(ThreeDSettings, other.ThreeDSettings))
            {
                SharedLogger.logger.Trace($"AMD_GPU_WITH_SETTINGS/Equals: The ThreeDSettings values don't equal each other");
                return false;
            }
            if (!Nullable.Equals(VideoUpscale, other.VideoUpscale))
            {
                SharedLogger.logger.Trace($"AMD_GPU_WITH_SETTINGS/Equals: The VideoUpscale values don't equal each other");
                return false;
            }
            if (!Nullable.Equals(VideoSuperResolution, other.VideoSuperResolution))
            {
                SharedLogger.logger.Trace($"AMD_GPU_WITH_SETTINGS/Equals: The VideoSuperResolution values don't equal each other");
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (ThreeDSettings, VideoUpscale, VideoSuperResolution).GetHashCode();
        }

        public static bool operator ==(AMD_GPU_WITH_SETTINGS lhs, AMD_GPU_WITH_SETTINGS rhs) => lhs.Equals(rhs);

        public static bool operator !=(AMD_GPU_WITH_SETTINGS lhs, AMD_GPU_WITH_SETTINGS rhs) => !(lhs == rhs);
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct AMD_DISPLAY_CONFIG : IEquatable<AMD_DISPLAY_CONFIG>
    {
        public bool IsInUse;
        public bool IsCloned;
        public bool IsEyefinity;
        public List<AMD_DESKTOP> Desktops;
        public AMD_EYEFINITY_DESKTOP EyefinityDesktop;
        public Dictionary<ulong,AMD_DISPLAY_WITH_SETTINGS> Displays;
        public AMD_SLS_CONFIG Adl2SlsConfig;
        public List<string> DisplayIdentifiers;
        public Dictionary<string, AMD_GPU_WITH_SETTINGS> GPUs;

        public AMD_DISPLAY_CONFIG()
        {
            IsInUse = false;
            IsCloned = false;
            IsEyefinity = false;
            Desktops = new List<AMD_DESKTOP>();
            EyefinityDesktop = new AMD_EYEFINITY_DESKTOP();
            Displays = new Dictionary<ulong,AMD_DISPLAY_WITH_SETTINGS>();
            Adl2SlsConfig = new AMD_SLS_CONFIG();
            DisplayIdentifiers = new List<string>();
            GPUs = new Dictionary<string, AMD_GPU_WITH_SETTINGS>();
        }

        public override bool Equals(object obj) => obj is AMD_DISPLAY_CONFIG other && this.Equals(other);
        public bool Equals(AMD_DISPLAY_CONFIG other)
        {
            if (IsInUse != other.IsInUse)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_CONFIG/Equals: The IsInUse values don't equal each other");
                return false;
            }
            if (IsCloned != other.IsCloned)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_CONFIG/Equals: The IsCloned values don't equal each other");
                return false;
            }
            if (!Desktops.SequenceEqual(other.Desktops))
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_CONFIG/Equals: The Desktops values don't equal each other");
                return false;
            }
            if (IsEyefinity != other.IsEyefinity)
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_CONFIG/Equals: The IsEyefinity values don't equal each other");
                return false;
            }
            if (!EyefinityDesktop.Equals(other.EyefinityDesktop))
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_CONFIG/Equals: The EyefinityDesktop values don't equal each other");
                return false;
            }
            if (!Displays.SequenceEqual(other.Displays))
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_CONFIG/Equals: The Displays values don't equal each other");
                return false;
            }
            if (!Adl2SlsConfig.Equals(other.Adl2SlsConfig))
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_CONFIG/Equals: The Adl2SlsConfig values don't equal each other");
                return false;
            }
            if (!DisplayIdentifiers.SequenceEqual(other.DisplayIdentifiers))
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_CONFIG/Equals: The DisplayIdentifiers values don't equal each other");
                return false;
            }
            if (!GPUs.SequenceEqual(other.GPUs))
            {
                SharedLogger.logger.Trace($"AMD_DISPLAY_CONFIG/Equals: The GPUs values don't equal each other");
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (IsInUse, IsCloned, Desktops, IsEyefinity, EyefinityDesktop, Displays, Adl2SlsConfig, DisplayIdentifiers, GPUs).GetHashCode();
        }

        public static bool operator ==(AMD_DISPLAY_CONFIG lhs, AMD_DISPLAY_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(AMD_DISPLAY_CONFIG lhs, AMD_DISPLAY_CONFIG rhs) => !(lhs == rhs);
    }

    class AMDLibrary : IDisposable
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static AMDLibrary _instance = new AMDLibrary();

        private bool _initialised = false;
        private bool _initialisedADL2 = false;

        // To detect redundant calls
        private bool _disposed = false;

        // Instantiate a SafeHandle instance.
        private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);
        private IntPtr _adlContextHandle = IntPtr.Zero;
        private ADLXApiHelper _adlxHelper;
        //private ADLXHelper _adlxHelper;
        private ADLXSystemServicesHelper _adlxSystem;
        //private int _adlxHighestSupportedSystemVersion = 0; // Only the base SystemServices is supported in all versions of ADLX
        private AMD_DISPLAY_CONFIG? _activeDisplayConfig;
        public List<string> _allConnectedDisplayIdentifiers;
        //public IntPtr hADLXBindingModule = IntPtr.Zero;
        public IntPtr hADLXModule = IntPtr.Zero;
        public const string AMD_ADLX_BINDING_DLL = "ADLXWrapper.dll";
        public const string AMD_ADLX_DLL = "amdadlx64.dll";

        static AMDLibrary() { }
        public AMDLibrary()
        {
            _activeDisplayConfig = CreateDefaultConfig();
            try
            {
                _initialised = false;
                // Check if there is AMD hardware installed
                SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: Looking for AMD PCI hardware...");
                if (WinLibrary.IsPCIVideoCardVendorInstalled(PCIVendorIDs))
                {
                    SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: AMD hardware detected");
                }
                else
                {
                    SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: No AMD hardware detected");
                    return;
                }

                try
                {
                    // Attempt to load the AMD ADLX 64-bit DLL
                    SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: Attempting to load the AMD ADLX DLL {AMD_ADLX_DLL} to confirm it's available for use by our ADLX Csharp Binding DLL");
                    hADLXModule = LoadLibrary(AMD_ADLX_DLL);
                    if (hADLXModule != IntPtr.Zero)
                    {
                        // Successfully loaded the ADLX DLL, which means it's installed!
                        SharedLogger.logger.Trace("AMDLibrary/AMDLibrary: We successfully loaded the AMD ADLX DLL which means the AMD Adrenalin driver software is installed.");
                    }
                    else
                    {
                        // LoadLibrary failed, DLL is not available
                        _initialised = false;
                        SharedLogger.logger.Error("AMDLibrary/AMDLibrary: Failed to load the AMD ADLX DLL. You need to download and install the AMD Adrenalin software from the AMD support website in order to fully support AMD hardware.");
                        return;
                    }
                    
                }
                catch (Exception ex)
                {
                    _initialised = false;
                    SharedLogger.logger.Error(ex, "AMDLibrary/AMDLibrary: Exception while trying to load the AMD ADLX DLL or our AMD ADLX CSharp Binding DLL. You may need to install the AMD driver.");
                }

                SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: Attempting to load the AMD ADL2 DLL {ADLImport.ATI_ADL_DLL} to use for setting AMD Eyefinity (it seems more reliable)");
                try
                {
                    _initialisedADL2 = false;
                    Marshal.PrelinkAll(typeof(ADLImport));
                    SharedLogger.logger.Trace("AMDLibrary/AMDLibrary: Successfully loaded the AMD ADL2 DLL.");
                    try
                    {
                        ADL_STATUS ADLRet;
                        ADLRet = ADLImport.ADL2_Main_Control_Create(ADLImport.ADL_Main_Memory_Alloc, ADLImport.ADL_TRUE, out _adlContextHandle);
                        if (ADLRet == ADL_STATUS.ADL_OK)
                        {
                            _initialisedADL2 = true;
                            SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: AMD ADL2 library was initialised successfully");
                        }
                        else
                        {
                            _initialisedADL2 = false;
                            SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: Error intialising AMD ADL2 library. ADL2_Main_Control_Create() returned error code {ADLRet}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _initialisedADL2 = false;
                        SharedLogger.logger.Trace(ex, $"AMDLibrary/AMDLibrary: Exception intialising AMD ADL2 library. ADL2_Main_Control_Create() caused an exception.");
                    }
                }
                catch (Exception ex)
                {
                    _initialisedADL2 = false;
                    SharedLogger.logger.Error(ex, "AMDLibrary/AMDLibrary: Exception whie trying to load the AMD ADL DLL. You may need to install the AMD driver.");
                }

                // We set the environment variable as a workaround so that ADL2_Display_SLSMapConfigX2_Get works :(
                // This is a weird thing that AMD even set in their own code! WTF! Who programmed that as a feature?
                Environment.SetEnvironmentVariable("ADL_4KWORKAROUND_CANCEL", "TRUE");


                // Initialize ADLX with ADLXHelper
                //_adlxHelper = new ADLXHelper();
                SharedLogger.logger.Trace("AMDLibrary/AMDLibrary: Intialising AMD ADLX Helper interface");
                try
                {
                    _adlxHelper = ADLXApiHelper.Initialize();
                    try
                    {
                        // Get system services
                        SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: Successfully intialised AMD ADLX Helper.");
                        SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: Attemping to access AMD ADLX System Services.");
                        _adlxSystem = _adlxHelper.GetSystemServices();
                        _initialised = true;
                        //_adlxHighestSupportedSystemVersion = 0;
                        SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: Successfully got AMD ADLX System Services.");
                        SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: AMD ADLX library was initialised successfully");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Trace(ex, $"AMDLibrary/AMDLibrary: Exception getting the ADLX System Services");
                        SharedLogger.logger.Trace(ex, $"AMDLibrary/AMDLibrary: Disposing the ADLXHelper to avoid memory leaks");
                        _adlxHelper.Dispose();
                        SharedLogger.logger.Trace(ex, $"AMDLibrary/AMDLibrary: Setting ADLXHelper to null");
                        _adlxHelper = null;
                        _initialised = false;
                        return;
                    }

                    SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: Automatically getting the AMD Display Configuration");
                    _activeDisplayConfig = GetActiveConfig();

                    // If we failed to get the display config, then we can't continue to use the library, so we dispose of it to avoid memory leaks and exit
                    if (_activeDisplayConfig == null)
                    {
                        _activeDisplayConfig = CreateDefaultConfig();
                        SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: The active AMD Display Configuration is null. Disposing the ADLXHelper to avoid memory leaks");
                        _adlxHelper.Dispose();
                        SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: Setting ADLXHelper to null");
                        _adlxHelper = null;
                        _initialised = false;
                        return;
                    }

                    // If we got a display config, but there are no displays, then we continue
                    if (_activeDisplayConfig.Value.Displays.Count == 0)
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: No displays connected to the AMD GPU, so returning an empty display configuration");
                        return;
                    }

                    SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: Automatically getting the AMD Connected Display Identifiers");
                    _allConnectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers(out bool failure);

                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Trace(ex, $"AMDLibrary/AMDLibrary: Exception intialising AMD ADLX Helper.");
                    _initialised = false;
                    return;
                }
            }
            catch (TypeInitializationException ex)
            {
                SharedLogger.logger.Info(ex, $"AMDLibrary/AMDLibrary: TypeInitializationException trying to load the AMD ADLX DLL {AMD_ADLX_BINDING_DLL}. This generally means you don't have the AMD ADLX driver installed.");
                _initialised = false;
                return;
            }
            catch (DllNotFoundException ex)
            {
                // If we get here then the AMD ADL DLL wasn't found. We can't continue to use it, so we log the error and exit
                SharedLogger.logger.Info(ex, $"AMDLibrary/AMDLibrary: DllNotFoundException trying to load the AMD ADLX DLL {AMD_ADLX_BINDING_DLL}. This generally means you don't have the AMD ADLX driver installed.");
                _initialised = false; 
                return;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Info(ex, $"AMDLibrary/AMDLibrary: A general exception trying to load the AMD ADLX DLL {AMD_ADLX_BINDING_DLL}.");
                _initialised = false; 
                return;
            }
        }

        ~AMDLibrary()
        {
            SharedLogger.logger.Trace("AMDLibrary/~AMDLibrary: Destroying AMDX Library");
            Dispose(false);
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose() => Dispose(true);

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed state (managed objects).
                _safeHandle?.Dispose();
            }

            // Dispose unmanaged resources
            if (_adlxHelper != null)
            {

                SharedLogger.logger.Trace("AMDLibrary/Dispose: Destroying AMD ADLX library interface");
                // If the ADLX library was initialised, then we need to free it up.
                if (_initialised)
                {
                    try
                    {
                        // Terminate ADLX
                        _adlxHelper.Dispose();
                        _adlxHelper = null;
                        SharedLogger.logger.Trace($"AMDLibrary/Dispose: AMD ADLX library was destroyed successfully");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Trace(ex, $"AMDLibrary/Dispose: Exception destroying AMD ADL2 library. _adlxHelper.Terminate() caused an exception.");
                    }

                }
            }

            // Dispose unmanaged resources
            if (_adlContextHandle != IntPtr.Zero)
            {

                SharedLogger.logger.Trace("AMDLibrary/Dispose: Destroying AMD ADL2 library interface");
                // If the ADL2 library was initialised, then we need to free it up.
                if (_initialisedADL2)
                {
                    try
                    {
                        ADLImport.ADL2_Main_Control_Destroy(_adlContextHandle);
                        _adlContextHandle = IntPtr.Zero;
                        SharedLogger.logger.Trace($"AMDLibrary/Dispose: AMD ADL2 library was destroyed successfully");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Trace(ex, $"AMDLibrary/Dispose: Exception destroying AMD ADL2 library. ADL2_Main_Control_Destroy() caused an exception.");
                    }

                }
            }

            /*if (hADLXBindingModule != IntPtr.Zero)
            {
                SharedLogger.logger.Trace("AMDLibrary/Dispose: Freeing the AMD ADLX Binding DLL");
                FreeLibrary(hADLXBindingModule);
                hADLXBindingModule = IntPtr.Zero;
            }*/

            if (hADLXModule != IntPtr.Zero)
            {
                SharedLogger.logger.Trace("AMDLibrary/Dispose: Freeing the AMD ADLX DLL");
                FreeLibrary(hADLXModule);
                hADLXModule = IntPtr.Zero;
            }

            _disposed = true;
        }

        public static void KeepVideoCardOn()
        {
            LoadLibrary("AMDExportsDLL.dll");
        }

        public bool IsInstalled
        {
            get
            {
                return _initialised;
            }
        }

        public List<string> PCIVendorIDs
        {
            get
            {
                // A list of all the matching PCI Vendor IDs are per https://www.pcilookup.com/?ven=amd&dev=&action=submit
                return new List<string>() { "1002" };
            }
        }

        public AMD_DISPLAY_CONFIG ActiveDisplayConfig
        {
            get
            {
                if(_activeDisplayConfig == null)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/ActiveDisplayConfig: ActiveDisplayConfig is null, so creating a new one");
                    _activeDisplayConfig = CreateDefaultConfig();
                }
                return _activeDisplayConfig.Value;
            }
            set
            {
                _activeDisplayConfig = value;
            }
        }

        public List<string> CurrentDisplayIdentifiers
        {
            get
            {
                return _activeDisplayConfig.Value.DisplayIdentifiers;
            }
        }

        public static AMDLibrary GetLibrary()
        {
            if (_instance == null)
            {
                _instance = new AMDLibrary();
            }

            return _instance;
        }

        public AMD_DISPLAY_CONFIG CreateDefaultConfig()
        {
            AMD_DISPLAY_CONFIG myDefaultConfig = new AMD_DISPLAY_CONFIG();

            // Fill in the minimal amount we need to avoid null references
            // so that we won't break json.net when we save a default config

            // THIS IS ALL TAKEN CARE OF IN THE STRUCT CONSTRUCTORS NOW \o/ yay!
            myDefaultConfig.IsInUse = false;
            //myDefaultConfig.Desktops = new List<AMD_DESKTOP>();
            //myDefaultConfig.EyefinityDesktop = new AMD_EYEFINITY_DESKTOP();
            //myDefaultConfig.Displays = new Dictionary<long, AMD_DISPLAY_WITH_SETTINGS>();   
            //myDefaultConfig.Adl2SlsConfig = new AMD_SLS_CONFIG();

            return myDefaultConfig;
        }

        public bool UpdateActiveConfig()
        {
            SharedLogger.logger.Trace($"AMDLibrary/UpdateActiveConfig: Updating the currently active config");
            try
            {
                _activeDisplayConfig = GetActiveConfig();
                _allConnectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers(out bool failure);
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Trace(ex, $"AMDLibrary/UpdateActiveConfig: Exception updating the currently active config");
                return false;
            }

            return true;
        }

        public AMD_DISPLAY_CONFIG GetActiveConfig()
        {
            SharedLogger.logger.Trace($"AMDLibrary/GetActiveConfig: Getting the currently active config");
            bool allDisplays = true;
            return GetAMDDisplayConfig(allDisplays);
        }

        private AMD_DISPLAY_CONFIG GetAMDDisplayConfig(bool allDisplays = false)
        {
            // Creat empty config struct so we know there are no nulls in there to break the json serializer
            AMD_DISPLAY_CONFIG myDisplayConfig = CreateDefaultConfig();

            if (_initialised)
            {
                // Get the desktop services
                // This is how we get and iterate through the various desktops. 
                // - A single desktop is associated with one display.
                // - A duplicate desktop is associated with two or more displays.
                // - An AMD Eyefinity desktop is associated with two or more displays.
                List<AMD_DESKTOP> desktopsToStore = new List<AMD_DESKTOP>();
                List<AMD_DISPLAY_WITH_SETTINGS> displaysToStore = new List<AMD_DISPLAY_WITH_SETTINGS>();
                AMD_EYEFINITY_DESKTOP eyefinityDesktopToStore = new AMD_EYEFINITY_DESKTOP();

                SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Attempting to get the ADLX desktop services");
                List<ADLXDesktop> desktopsList = _adlxSystem.EnumerateDesktops().ToList();
                if (desktopsList.Count == 0)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: No desktops found in ADLX system services");
                    return CreateDefaultConfig();
                }
                else
                {
                    SharedLogger.logger.Trace($"AMDLibrary/GetAMDDesktopConfig: Successfully got the desktop list");
                    AMD_DESKTOP newDesktop = new AMD_DESKTOP();
                    // Iterate through the desktop list
                    foreach (var desktop in desktopsList)
                    {
                        var desktopDisplayList = desktop.EnumerateDisplaysForDesktop();
                        newDesktop.Displays = new List<AMD_DISPLAY>();

                        newDesktop.NumberOfDisplays = desktopDisplayList.Count;
                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDesktopConfig: The number of displays that are part of this desktop is {newDesktop.NumberOfDisplays}");

                        if (newDesktop.NumberOfDisplays > 0)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/GetAMDDesktopConfig: The number of displays that are part of this desktop is > 0, so getting list of displays");
                            // Get the list of displays that are part of this desktop
                            foreach (var display in desktopDisplayList)
                            {
                                AMD_DISPLAY newDisplay = new AMD_DISPLAY();

                                // Get the display connection type
                                newDisplay.ConnectorType = display.ConnectorType;

                                // Get the display type
                                //newDisplay.DisplayType = display.DisplayType ;

                                // Get the EDID
                                newDisplay.EDID = display.Edid;

                                // Get the manufacturer ID
                                newDisplay.ManufacturerID = display.ManufacturerId;

                                // Get the display name
                                newDisplay.Name = display.Name;

                                // Get the native resolution
                                newDisplay.NativeResolutionWidth = display.NativeResolutionWidth;
                                newDisplay.NativeResolutionHeight = display.NativeResolutionHeight;

                                // Get the PixelClock
                                newDisplay.PixelClock = display.PixelClock;

                                // Get the refresh rate
                                newDisplay.RefreshRate = display.RefreshRate;

                                // Get the scan type
                                newDisplay.ScanType = display.ScanType;

                                // Get the Unique ID
                                newDisplay.UniqueID = display.UniqueId;

                                // Add the new display to the list of displays for this desktop
                                newDesktop.Displays.Add(newDisplay);                               
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/GetAMDDesktopConfig: The number of displays that are part of this desktop is 0, so not getting list of displays. Skipping.");
                        }

                        // Store the oreientation
                        newDesktop.Orientation = desktop.Orientation;

                        // Store the size and top left location of the desktop
                        newDesktop.SizeWidth = desktop.Width;
                        newDesktop.SizeHeight = desktop.Height;
                        newDesktop.TopLeftX = desktop.TopLeftX;
                        newDesktop.TopLeftY = desktop.TopLeftY;

                        // Store the desktop type
                        newDesktop.Type = desktop.Type;

                        // TODO: Process Eyefinity and Cloned desktops
/*
                        // The the desktop is an eyefinity desktop then set the eyefinity enabled flag
                        // and also process the EyefinityDesktop layout
                        if (newDesktop.Type == ADLX_DESKTOP_TYPE.DESKTOP_EYEFINITY)
                        {
                            isEyefinityEnabled = true;
                            SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Eyefinity desktop detected");
                            // Get the eyefinity desktop

                            // 1. Allocate a void** via SWIG
                            SWIGTYPE_p_p_void ppVoid = ADLX.new_voidP_Ptr();

                            SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Getting a pointer to the Eyefinity desktop object");
                            // 2. Call QueryInterface with the IID for IADLXEyefinityDesktop to get the interface
                            status = desktop.QueryInterface(
                                IADLXEyefinityDesktop.IID(),
                                ppVoid
                            );

                            if (status != ADLX_RESULT.ADLX_OK)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/GetAMDDesktopConfig: Error getting the ADLX display list. systemServices.GetDisplays() returned error code {status}");
                                return CreateDefaultConfig(); ;
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Converting pointer to the Eyefinity desktop object to an IntPtr");

                                // Extract the raw IntPtr from the void** for the IADLXEyefinityDesktop
                                IntPtr rawPtr = ADLX.voidP_Ptr_value(ppVoid);

                                // Wrap it in the managed proxy
                                //    (Constructor args may vary based on SWIG config)
                                IADLXEyefinityDesktop eyefinityDesktop = new IADLXEyefinityDesktop(rawPtr, true);

                                // Use the EyefinityDesktop object to get the Eyefinity layout
                                SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Getting the rows and columns of the diaplay grid for the Eyefinity desktop");
                                SWIGTYPE_p_unsigned_int pRow = ADLX.new_adlx_uintP();
                                ADLX.adlx_uintP_assign(pRow, 0);
                                SWIGTYPE_p_unsigned_int pCol = ADLX.new_adlx_uintP();
                                ADLX.adlx_uintP_assign(pCol, 0);
                                eyefinityDesktop.GridSize(pRow, pCol);
                                myDisplayConfig.EyefinityDesktop.Rows = ADLX.adlx_uintP_value(pRow);
                                myDisplayConfig.EyefinityDesktop.Columns = ADLX.adlx_uintP_value(pCol);

                                *//*for (uint row=1; row<gridRows; row++)
                                {
                                    for (uint col = 1; col < gridCols; col++)
                                    {
                                        // Get the eyefinity desktop orientation
                                        SWIGTYPE_p_ADLX_ORIENTATION pEyefinityDisplayOrientation = ADLX.new_orientationP();
                                        eyefinityDesktop.DisplayOrientation(row, col, pEyefinityDisplayOrientation);
                                        ADLX_ORIENTATION eyefinityOrientation = ADLX.orientationP_value(pEyefinityDisplayOrientation);

                                        // Get the display size
                                        SWIGTYPE_p_int pEyefinityDisplayWidth= ADLX.new_intP();
                                        SWIGTYPE_p_int pEyefinityDisplayHeight = ADLX.new_intP();
                                        eyefinityDesktop.DisplaySize(row,col, pEyefinityDisplayWidth, pEyefinityDisplayHeight);
                                        int eyefinityDisplayWidth = ADLX.intP_value(pEyefinityDisplayWidth);
                                        int eyefinityDisplayHeight = ADLX.intP_value(pEyefinityDisplayHeight);

                                        // Get the display location
                                        ADLX_Point pLocation = ADLX.new_pointP();
                                        eyefinityDesktop.DisplayTopLeft(row, col, pLocation);
                                        ADLX_Point location = ADLX.pointP_value(pLocation);

                                    }
                                }*//*

                                // Copy over the desktop level sizes so that we can match things easier in the future
                                myDisplayConfig.EyefinityDesktop.Orientation = newDesktop.Orientation;
                                myDisplayConfig.EyefinityDesktop.TopLeftX = newDesktop.TopLeftX;
                                myDisplayConfig.EyefinityDesktop.TopLeftY = newDesktop.TopLeftY;
                                myDisplayConfig.EyefinityDesktop.SizeWidth = newDesktop.SizeWidth;
                                myDisplayConfig.EyefinityDesktop.SizeHeight = newDesktop.SizeHeight;

                                // 7. Release when done
                                eyefinityDesktop.Release();
                                ADLX.delete_voidP_Ptr(ppVoid);
                            }
                        }
                        else if (newDesktop.Type == ADLX_DESKTOP_TYPE.DESKTOP_DUPLCATE)
                        {
                            isCloned = true;
                            SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Cloned desktop detected");
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Single desktop detected");
                        }
*/
                        // Release desktop interface
                        desktop.Dispose();

                        // Save the Desktop to the main list
                        myDisplayConfig.Desktops.Add(newDesktop);
                    }
                }

                //-----------------------------------------------------------------------

                // Get the display services
                // This lets us interact with the various displays to get the settings from them
                
                SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Attempting to get the ADLX display services");
                List<ADLXDisplay> displaysList = _adlxSystem.EnumerateDisplays().ToList();
                if (displaysList.Count == 0)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: No displays found in ADLX system services");
                    return CreateDefaultConfig();
                }
                else
                {
                    SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Attempting to get the ADLX display list");
                    foreach (var display in displaysList)
                    {
                        // Create a new AMD_DISPLAY_WITH_SETTINGS to store things in
                        AMD_DISPLAY_WITH_SETTINGS newDisplay = new AMD_DISPLAY_WITH_SETTINGS();

                        newDisplay.ConnectorType = display.ConnectorType;
                        bool isDisplayPort        = newDisplay.ConnectorType == ADLX_DISPLAY_CONNECTOR_TYPE.DISPLAY_CONTYPE_DISPLAYPORT ||
                                                    newDisplay.ConnectorType == ADLX_DISPLAY_CONNECTOR_TYPE.DISPLAY_CONTYPE_EDP ||
                                                    newDisplay.ConnectorType == ADLX_DISPLAY_CONNECTOR_TYPE.DISPLAY_CONTYPE_USB_TYPE_C;
                        bool isHdmi               = newDisplay.ConnectorType == ADLX_DISPLAY_CONNECTOR_TYPE.DISPLAY_CONTYPE_HDMI_TYPE_A ||
                                                    newDisplay.ConnectorType == ADLX_DISPLAY_CONNECTOR_TYPE.DISPLAY_CONTYPE_HDMI_TYPE_B;
                        bool isDigitalWithProtocol = isDisplayPort || isHdmi;
                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Display {display.UniqueId} connector type is {newDisplay.ConnectorType}. isDisplayPort={isDisplayPort}, isHdmi={isHdmi}, isDigitalWithProtocol={isDigitalWithProtocol}.");
                        newDisplay.Type = display.Type;
                        newDisplay.EDID = display.Edid;
                        newDisplay.ManufacturerID = display.ManufacturerId;
                        newDisplay.Name = display.Name;
                        newDisplay.UniqueID = display.UniqueId;
                        newDisplay.NativeResolutionWidth = display.NativeResolutionWidth;
                        newDisplay.NativeResolutionHeight = display.NativeResolutionHeight;
                        newDisplay.PixelClock = display.PixelClock;
                        newDisplay.RefreshRate = display.RefreshRate;
                        newDisplay.ScanType = display.ScanType;
                        newDisplay.GpuUniqueID = display.GpuUniqueId;

                        // Ok now start getting the various settings for this display

                        //------------------------------------
                        // CONVERT THE ADLX OBJECTS TO OUR OBJECTS FOR STORAGE

                         // Get the current custom color object for this display
                        newDisplay.CustomColorInfo = new AMD_CUSTOM_COLOR_INFO(display.GetCustomColor());
                        
                        // Get the gamma information for this display
                        newDisplay.GammaInfo = new AMD_GAMMA_INFO(display.GetGamma());

                        // Get the gammut information for this display
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                newDisplay.GamutInfo = new AMD_GAMUT_INFO(display.GetGamut());
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"AMDLibrary/GetAMDDisplayConfig: Exception getting Gamut info for display {display.UniqueId}.");
                            }
                        }

                        // Now get the connectivity experience settings if we can
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                newDisplay.ConnectivityExperience = new AMD_CONNECTIVITY_EXPERIENCE_INFO(display.GetConnectivityExperience());
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"AMDLibrary/GetAMDDisplayConfig: Exception getting Connectivity Experience for display {display.UniqueId}.");
                            }
                        }

                        // Get the 3DLUT settings for this display
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                newDisplay.ThreeDLUTSettings = new AMD_3DLUT_INFO(display.GetThreeDLut());
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"AMDLibrary/GetAMDDisplayConfig: Exception getting 3DLUT settings for display {display.UniqueId}.");
                            }
                        }

                        // TODO: ADLXWrapper Helper Facade does not support custom resolutions properly yet
                        // so we need to skip that for now. Need to fix that in ADLXWrapper later.

                        //------------------------------------
                        // Get the current color depth for this display
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                (newDisplay.IsSupportedColorDepth, newDisplay.ColorDepth) = display.GetColorDepthState();
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"AMDLibrary/GetAMDDisplayConfig: Exception getting Color Depth for display {display.UniqueId}.");
                            }
                        }

                        // GET THE FreeSync Settings IF WE CAN
                        if (isDisplayPort)
                        {
                            try
                            {
                                (newDisplay.IsSupportedFreeSync, newDisplay.IsEnabledFreeSync) = display.GetFreeSyncState();
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"AMDLibrary/GetAMDDisplayConfig: Exception getting FreeSync state for display {display.UniqueId}.");
                            }
                        }

                        // Get the free sync accuracy range if we can
                        if (isDisplayPort)
                        {
                            try
                            {
                                (newDisplay.IsSupportedFreeSyncColorAccuracy,newDisplay.IsEnabledFreeSyncColorAccuracy) = display.GetFreeSyncColorAccuracyState();
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"AMDLibrary/GetAMDDisplayConfig: Exception getting FreeSync Color Accuracy state for display {display.UniqueId}.");
                            }
                        }

                        // Get the dynamic refresh rate control if we can
                        if (isDisplayPort)
                        {
                            try
                            {
                                (newDisplay.IsSupportedDynamicRefreshRateControl, newDisplay.IsEnabledDynamicRefreshRateControl) = display.GetDynamicRefreshRateControlState();
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"AMDLibrary/GetAMDDisplayConfig: Exception getting Dynamic Refresh Rate Control state for display {display.UniqueId}.");
                            }
                        }

                        // Now get teh display blanking settings if we can
                        (newDisplay.IsSupportedDisplayBlanking,newDisplay.IsEnabledDisplayBlanking) = display.GetDisplayBlankingState();

                        // Now get the Display GPU Scaling settings if we can
                        (newDisplay.IsSupportedGPUScaling,newDisplay.IsEnabledGPUScaling) = display.GetGpuScalingState();
                        
                        // Get the Integer Display Scaling settings if we can                            
                        (newDisplay.IsSupportedIntegerScaling, newDisplay.IsEnabledIntegerScaling) = display.GetIntegerScalingState();

                        // Get the Display Pixel Format settings if we can
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                (newDisplay.IsSupportedPixelFormat, newDisplay.CurrentPixelFormat) = display.GetPixelFormatState();
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"AMDLibrary/GetAMDDisplayConfig: Exception getting Pixel Format for display {display.UniqueId}.");
                            }
                        }                        
                        
                        // Get the Display Scaling Mode if we can
                        (newDisplay.IsSupportedScalingMode, newDisplay.CurrentScalingMode) = display.GetScalingMode();
 
                        // Get the VSR settings if we can
                        (newDisplay.IsSupportedVSR, newDisplay.IsEnabledVSR) = display.GetVirtualSuperResolutionState();

                        // Get the HDCP settings if we can
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                (newDisplay.IsSupportedHDCP, newDisplay.IsEnabledHDCP) = display.GetHdcpState();
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"AMDLibrary/GetAMDDisplayConfig: Exception getting HDCP state for display {display.UniqueId}.");
                            }
                        }

                        // Get VariBright settings
                        (newDisplay.IsSupportedVariBright, newDisplay.IsEnabledVariBright, newDisplay.VariBrightMode) = display.GetVariBrightState();                                            


                        //------------------------------------
                        // Save the Display to the main dictionary of displays with the uniqueid as the key
                        myDisplayConfig.Displays.Add(newDisplay.UniqueID, newDisplay);
                    }

                }

                // Now we have everything we need, so we can build the display config!
                myDisplayConfig.IsInUse = true;

                // Get the display identifiers                
                myDisplayConfig.DisplayIdentifiers = GetCurrentDisplayIdentifiers(out bool failure);

                // Now try to get the AMD Eyefinity layout using ADL2 (the older standard) as it is more configurable
                if (_initialisedADL2)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Attempting to get the AMD Eyefinity layout using the ADL2 API");

                    // Get the Adapter info for ALL adapter and put it in the AdapterBuffer
                    SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Running ADL2_Adapter_AdapterInfoX4_Get to get the information about all AMD Adapters.");
                    int numAdaptersInfo = 0;
                    IntPtr adapterInfoBuffer = IntPtr.Zero;
                    ADL_STATUS ADLRet = ADLImport.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, -1, out numAdaptersInfo, out adapterInfoBuffer);
                    if (ADLRet == ADL_STATUS.ADL_OK)
                    {

                        ADL_ADAPTER_INFOX2[] adapterArray = new ADL_ADAPTER_INFOX2[numAdaptersInfo];
                        if (numAdaptersInfo > 0)
                        {
                            IntPtr currentAdaptersInfoBuffer = adapterInfoBuffer;
                            for (int i = 0; i < numAdaptersInfo; i++)
                            {
                                // build a structure in the array slot
                                adapterArray[i] = new ADL_ADAPTER_INFOX2();
                                // fill the array slot structure with the data from the buffer
                                adapterArray[i] = (ADL_ADAPTER_INFOX2)Marshal.PtrToStructure(currentAdaptersInfoBuffer, typeof(ADL_ADAPTER_INFOX2));
                                // destroy the bit of memory we no longer need
                                //Marshal.DestroyStructure(currentAdaptersInfoBuffer, typeof(ADL_ADAPTER_INFOX2));
                                // advance the buffer forwards to the next object
                                currentAdaptersInfoBuffer = (IntPtr)((long)currentAdaptersInfoBuffer + Marshal.SizeOf(adapterArray[i]));
                            }
                            // Free the memory used by the buffer                        
                            Marshal.FreeCoTaskMem(adapterInfoBuffer);

                            SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Adapter_AdapterInfoX4_Get returned information about all AMD Adapters.");
                            // Now go through each adapter and get the information we need from it
                            for (int adapterIndex = 0; adapterIndex < numAdaptersInfo; adapterIndex++)
                            {
                                // Skip this adapter if it isn't active
                                ADL_ADAPTER_INFOX2 oneAdapter = adapterArray[adapterIndex]; // There is always just one as we asked for a specific one!
                                if (oneAdapter.Exist != ADLImport.ADL_TRUE)
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} doesn't exist at present so skipping detection for this adapter.");
                                    continue;
                                }

                                // Only skip non-present displays if we want all displays information
                                if (oneAdapter.Present != ADLImport.ADL_TRUE)
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't enabled at present so skipping detection for this adapter.");
                                    continue;
                                }

                                // Check if the adapter is active
                                // Skip this adapter if it isn't active
                                int adapterActiveStatus = ADLImport.ADL_FALSE;
                                ADLRet = ADLImport.ADL2_Adapter_Active_Get(_adlContextHandle, adapterIndex, out adapterActiveStatus);
                                if (ADLRet == ADL_STATUS.ADL_OK)
                                {
                                    if (adapterActiveStatus == ADLImport.ADL_TRUE)
                                    {
                                        SharedLogger.logger.Trace($"AMDLibrary/GetSomeDisplayIdentifiers: ADL2_Adapter_Active_Get returned ADL_TRUE - AMD Adapter #{adapterIndex} is active! We can continue.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"AMDLibrary/GetSomeDisplayIdentifiers: ADL2_Adapter_Active_Get returned ADL_FALSE - AMD Adapter #{adapterIndex} is NOT active, so skipping.");
                                        continue;
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Warn($"AMDLibrary/GetSomeDisplayIdentifiers: WARNING - ADL2_Adapter_Active_Get returned ADL_STATUS {ADLRet} when trying to see if AMD Adapter #{adapterIndex} is active. Trying to skip this adapter so something at least works.");
                                    continue;
                                }

                                // Go grab the DisplayMaps and DisplayTargets as that is useful infor for creating screens
                                int numDisplayTargets = 0;
                                int numDisplayMaps = 0;
                                IntPtr displayTargetBuffer = IntPtr.Zero;
                                IntPtr displayMapBuffer = IntPtr.Zero;
                                ADLRet = ADLImport.ADL2_Display_DisplayMapConfig_Get(_adlContextHandle, adapterIndex, out numDisplayMaps, out displayMapBuffer, out numDisplayTargets, out displayTargetBuffer, ADLImport.ADL_DISPLAY_DISPLAYMAP_OPTION_GPUINFO);
                                if (ADLRet == ADL_STATUS.ADL_OK)
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_DisplayMapConfig_Get returned information about all displaytargets connected to AMD adapter {adapterIndex}.");
                                }
                                else
                                {
                                    SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - ADL2_Display_DisplayMapConfig_Get returned ADL_STATUS {ADLRet} when trying to get the display target info from AMD adapter {adapterIndex} in the computer.");
                                    throw new AMDLibraryException($"ADL2_Display_DisplayMapConfig_Get returned ADL_STATUS {ADLRet} when trying to get the display target info from AMD adapter {adapterIndex} in the computer");
                                }

                                ADL_DISPLAY_TARGET[] displayTargetArray = { };
                                if (numDisplayTargets > 0)
                                {
                                    // At this point we know there is at least one screen connected to an adapter
                                    myDisplayConfig.IsInUse = true;

                                    IntPtr currentDisplayTargetBuffer = displayTargetBuffer;
                                    //displayTargetArray = new ADL_DISPLAY_TARGET[numDisplayTargets];
                                    displayTargetArray = new ADL_DISPLAY_TARGET[numDisplayTargets];
                                    for (int i = 0; i < numDisplayTargets; i++)
                                    {
                                        // build a structure in the array slot
                                        displayTargetArray[i] = new ADL_DISPLAY_TARGET();
                                        //displayTargetArray[i] = new ADL_DISPLAY_TARGET();
                                        // fill the array slot structure with the data from the buffer
                                        displayTargetArray[i] = (ADL_DISPLAY_TARGET)Marshal.PtrToStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                                        //displayTargetArray[i] = (ADL_DISPLAY_TARGET)Marshal.PtrToStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                                        // destroy the bit of memory we no longer need
                                        Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                                        // advance the buffer forwards to the next object
                                        currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(displayTargetArray[i]));
                                        //currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(displayTargetArray[i]));

                                    }
                                    // Free the memory used by the buffer                        
                                    Marshal.FreeCoTaskMem(displayTargetBuffer);
                                    // Save the item                            
                                    //savedAdapterConfig.DisplayTargets = new ADL_DISPLAY_TARGET[numDisplayTargets];
                                    //myDisplayConfig.DisplayTargets = displayTargetArray.ToList<ADL_DISPLAY_TARGET>();
                                }
                                else
                                {
                                    // Free the memory used by the buffer                        
                                    Marshal.FreeCoTaskMem(displayTargetBuffer);
                                    // Return the default config as there are no display targets to get info from
                                    return myDisplayConfig;
                                }


                                // If there are more than 1 display targets then eyefinity is possible
                                if (numDisplayTargets > 1)
                                {
                                    // Check if SLS is enabled for this adapter!
                                    int matchingSLSMapIndex = -1;
                                    ADLRet = ADLImport.ADL2_Display_SLSMapIndex_Get(_adlContextHandle, oneAdapter.AdapterIndex, numDisplayTargets, displayTargetArray, out matchingSLSMapIndex);
                                    if (ADLRet == ADL_STATUS.ADL_OK && matchingSLSMapIndex != -1)
                                    {
                                        // We have a matching SLS index!
                                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} has one or more SLS Maps that could be used with this display configuration! Eyefinity (SLS) could be enabled.");

                                        AMD_SLSMAP_CONFIG mySLSMapConfig = new AMD_SLSMAP_CONFIG();

                                        // We want to get the SLSMapConfig for this matching SLS Map to see if it is actually in use
                                        int numSLSTargets = 0;
                                        IntPtr slsTargetBuffer = IntPtr.Zero;
                                        int numNativeMode = 0;
                                        IntPtr nativeModeBuffer = IntPtr.Zero;
                                        int numNativeModeOffsets = 0;
                                        IntPtr nativeModeOffsetsBuffer = IntPtr.Zero;
                                        int numBezelMode = 0;
                                        IntPtr bezelModeBuffer = IntPtr.Zero;
                                        int numTransientMode = 0;
                                        IntPtr transientModeBuffer = IntPtr.Zero;
                                        int numSLSOffset = 0;
                                        IntPtr slsOffsetBuffer = IntPtr.Zero;
                                        ADL_SLS_MAP slsMap = new ADL_SLS_MAP();
                                        ADLRet = ADLImport.ADL2_Display_SLSMapConfigX2_Get(
                                                                                        _adlContextHandle,
                                                                                            oneAdapter.AdapterIndex,
                                                                                            matchingSLSMapIndex,
                                                                                            ref slsMap,
                                                                                            out numSLSTargets,
                                                                                            out slsTargetBuffer,
                                                                                            out numNativeMode,
                                                                                            out nativeModeBuffer,
                                                                                            out numNativeModeOffsets,
                                                                                            out nativeModeOffsetsBuffer,
                                                                                            out numBezelMode,
                                                                                            out bezelModeBuffer,
                                                                                            out numTransientMode,
                                                                                            out transientModeBuffer,
                                                                                            out numSLSOffset,
                                                                                            out slsOffsetBuffer,
                                                                                            ADLImport.ADL_DISPLAY_SLSGRID_CAP_OPTION_RELATIVETO_CURRENTANGLE);
                                        if (ADLRet == ADL_STATUS.ADL_OK)
                                        {
                                            SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_SLSMapConfigX2_Get returned information about the SLS Info connected to AMD adapter {adapterIndex}.");
                                        }
                                        else
                                        {
                                            SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - ADL2_Display_SLSMapConfigX2_Get returned ADL_STATUS {ADLRet} when trying to get the SLS Info from AMD adapter {adapterIndex} in the computer.");
                                            continue;
                                        }

                                        // First check that the number of grid entries is equal to the number
                                        // of display targets associated with this adapter & SLS surface.
                                        if (numDisplayTargets != (slsMap.Grid.SLSGridColumn * slsMap.Grid.SLSGridRow))
                                        {
                                            //Number of display targets returned is not equal to the SLS grid size, so SLS can't be enabled fo this display
                                            //myDisplayConfig.SlsConfig.IsSlsEnabled = false; // This is already set to false at the start!
                                            break;
                                        }

                                        // Add the slsMap to the config we want to store
                                        mySLSMapConfig.SLSMap = slsMap;

                                        // Process the slsTargetBuffer
                                        ADL_SLS_TARGET[] slsTargetArray = new ADL_SLS_TARGET[numSLSTargets];
                                        if (numSLSTargets > 0)
                                        {
                                            IntPtr currentSLSTargetBuffer = slsTargetBuffer;
                                            for (int i = 0; i < numSLSTargets; i++)
                                            {
                                                // build a structure in the array slot
                                                slsTargetArray[i] = new ADL_SLS_TARGET();
                                                // fill the array slot structure with the data from the buffer
                                                slsTargetArray[i] = (ADL_SLS_TARGET)Marshal.PtrToStructure(currentSLSTargetBuffer, typeof(ADL_SLS_TARGET));
                                                // destroy the bit of memory we no longer need
                                                //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                                // advance the buffer forwards to the next object
                                                currentSLSTargetBuffer = (IntPtr)((long)currentSLSTargetBuffer + Marshal.SizeOf(slsTargetArray[i]));
                                            }
                                            // Free the memory used by the buffer                        
                                            Marshal.FreeCoTaskMem(slsTargetBuffer);

                                            // Add the slsTarget to the config we want to store
                                            mySLSMapConfig.SLSTargets = slsTargetArray.ToList();

                                        }
                                        else
                                        {
                                            // Add the slsTarget to the config we want to store
                                            mySLSMapConfig.SLSTargets = new List<ADL_SLS_TARGET>();
                                        }

                                        // Process the nativeModeBuffer
                                        ADL_SLS_MODE[] nativeModeArray = new ADL_SLS_MODE[numNativeMode];
                                        if (numNativeMode > 0)
                                        {
                                            IntPtr currentNativeModeBuffer = nativeModeBuffer;
                                            for (int i = 0; i < numNativeMode; i++)
                                            {
                                                // build a structure in the array slot
                                                nativeModeArray[i] = new ADL_SLS_MODE();
                                                // fill the array slot structure with the data from the buffer
                                                nativeModeArray[i] = (ADL_SLS_MODE)Marshal.PtrToStructure(currentNativeModeBuffer, typeof(ADL_SLS_MODE));
                                                // destroy the bit of memory we no longer need
                                                //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                                // advance the buffer forwards to the next object
                                                currentNativeModeBuffer = (IntPtr)((long)currentNativeModeBuffer + Marshal.SizeOf(nativeModeArray[i]));
                                            }
                                            // Free the memory used by the buffer                        
                                            Marshal.FreeCoTaskMem(nativeModeBuffer);

                                            // Add the nativeMode to the config we want to store
                                            mySLSMapConfig.NativeModes = nativeModeArray.ToList();

                                        }
                                        else
                                        {
                                            // Add the slsTarget to the config we want to store
                                            mySLSMapConfig.NativeModes = new List<ADL_SLS_MODE>();
                                        }

                                        // Process the nativeModeOffsetsBuffer
                                        ADL_SLS_OFFSET[] nativeModeOffsetArray = new ADL_SLS_OFFSET[numNativeModeOffsets];
                                        if (numNativeModeOffsets > 0)
                                        {
                                            IntPtr currentNativeModeOffsetsBuffer = nativeModeOffsetsBuffer;
                                            for (int i = 0; i < numNativeModeOffsets; i++)
                                            {
                                                // build a structure in the array slot
                                                nativeModeOffsetArray[i] = new ADL_SLS_OFFSET();
                                                // fill the array slot structure with the data from the buffer
                                                nativeModeOffsetArray[i] = (ADL_SLS_OFFSET)Marshal.PtrToStructure(currentNativeModeOffsetsBuffer, typeof(ADL_SLS_OFFSET));
                                                // destroy the bit of memory we no longer need
                                                //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                                // advance the buffer forwards to the next object
                                                currentNativeModeOffsetsBuffer = (IntPtr)((long)currentNativeModeOffsetsBuffer + Marshal.SizeOf(nativeModeOffsetArray[i]));
                                            }
                                            // Free the memory used by the buffer                        
                                            Marshal.FreeCoTaskMem(nativeModeOffsetsBuffer);

                                            // Add the nativeModeOffsets to the config we want to store
                                            mySLSMapConfig.NativeModeOffsets = nativeModeOffsetArray.ToList();

                                        }
                                        else
                                        {
                                            // Add the empty list to the config we want to store
                                            mySLSMapConfig.NativeModeOffsets = new List<ADL_SLS_OFFSET>();
                                        }

                                        // Process the bezelModeBuffer
                                        ADL_BEZEL_TRANSIENT_MODE[] bezelModeArray = new ADL_BEZEL_TRANSIENT_MODE[numBezelMode];
                                        if (numBezelMode > 0)
                                        {
                                            IntPtr currentBezelModeBuffer = bezelModeBuffer;
                                            for (int i = 0; i < numBezelMode; i++)
                                            {
                                                // build a structure in the array slot
                                                bezelModeArray[i] = new ADL_BEZEL_TRANSIENT_MODE();
                                                // fill the array slot structure with the data from the buffer
                                                bezelModeArray[i] = (ADL_BEZEL_TRANSIENT_MODE)Marshal.PtrToStructure(currentBezelModeBuffer, typeof(ADL_BEZEL_TRANSIENT_MODE));
                                                // destroy the bit of memory we no longer need
                                                //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                                // advance the buffer forwards to the next object
                                                currentBezelModeBuffer = (IntPtr)((long)currentBezelModeBuffer + Marshal.SizeOf(bezelModeArray[i]));
                                            }
                                            // Free the memory used by the buffer                        
                                            Marshal.FreeCoTaskMem(bezelModeBuffer);

                                            // Add the bezelModes to the config we want to store
                                            mySLSMapConfig.BezelModes = bezelModeArray.ToList();

                                        }
                                        else
                                        {
                                            // Add the slsTarget to the config we want to store
                                            mySLSMapConfig.BezelModes = new List<ADL_BEZEL_TRANSIENT_MODE>();
                                        }

                                        // Process the transientModeBuffer
                                        ADL_BEZEL_TRANSIENT_MODE[] transientModeArray = new ADL_BEZEL_TRANSIENT_MODE[numTransientMode];
                                        if (numTransientMode > 0)
                                        {
                                            IntPtr currentTransientModeBuffer = transientModeBuffer;
                                            for (int i = 0; i < numTransientMode; i++)
                                            {
                                                // build a structure in the array slot
                                                transientModeArray[i] = new ADL_BEZEL_TRANSIENT_MODE();
                                                // fill the array slot structure with the data from the buffer
                                                transientModeArray[i] = (ADL_BEZEL_TRANSIENT_MODE)Marshal.PtrToStructure(currentTransientModeBuffer, typeof(ADL_BEZEL_TRANSIENT_MODE));
                                                // destroy the bit of memory we no longer need
                                                //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                                // advance the buffer forwards to the next object
                                                currentTransientModeBuffer = (IntPtr)((long)currentTransientModeBuffer + Marshal.SizeOf(transientModeArray[i]));
                                            }
                                            // Free the memory used by the buffer                        
                                            Marshal.FreeCoTaskMem(transientModeBuffer);

                                            // Add the transientModes to the config we want to store
                                            mySLSMapConfig.TransientModes = transientModeArray.ToList();
                                        }
                                        else
                                        {
                                            // Add the slsTarget to the config we want to store
                                            mySLSMapConfig.TransientModes = new List<ADL_BEZEL_TRANSIENT_MODE>();
                                        }

                                        // Process the slsOffsetBuffer
                                        ADL_SLS_OFFSET[] slsOffsetArray = new ADL_SLS_OFFSET[numSLSOffset];
                                        if (numSLSOffset > 0)
                                        {
                                            IntPtr currentSLSOffsetBuffer = slsOffsetBuffer;
                                            for (int i = 0; i < numSLSOffset; i++)
                                            {
                                                // build a structure in the array slot
                                                slsOffsetArray[i] = new ADL_SLS_OFFSET();
                                                // fill the array slot structure with the data from the buffer
                                                slsOffsetArray[i] = (ADL_SLS_OFFSET)Marshal.PtrToStructure(currentSLSOffsetBuffer, typeof(ADL_SLS_OFFSET));
                                                // destroy the bit of memory we no longer need
                                                //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                                // advance the buffer forwards to the next object
                                                currentSLSOffsetBuffer = (IntPtr)((long)currentSLSOffsetBuffer + Marshal.SizeOf(slsOffsetArray[i]));
                                            }
                                            // Free the memory used by the buffer                        
                                            Marshal.FreeCoTaskMem(slsOffsetBuffer);

                                            // Add the slsOffsets to the config we want to store
                                            mySLSMapConfig.SLSOffsets = slsOffsetArray.ToList();

                                        }
                                        else
                                        {
                                            // Add the slsTarget to the config we want to store
                                            mySLSMapConfig.SLSOffsets = new List<ADL_SLS_OFFSET>();
                                        }

                                        // Now we try to calculate whether SLS is enabled
                                        // NFI why they don't just add a ADL2_Display_SLSMapConfig_GetState function to make this easy for ppl :(
                                        // NVIDIA make it easy, why can't you AMD?

                                        // Logic cribbed from https://github.com/elitak/amd-adl-sdk/blob/master/Sample/Eyefinity/ati_eyefinity.c
                                        // Go through each display Target
                                        foreach (var displayTarget in displayTargetArray)
                                        {
                                            // Get the current Display Modes for this adapter/display combination
                                            int numDisplayModes;
                                            IntPtr displayModeBuffer;
                                            ADLRet = ADLImport.ADL2_Display_Modes_Get(
                                                                                        _adlContextHandle,
                                                                                            oneAdapter.AdapterIndex,
                                                                                            displayTarget.DisplayID.DisplayLogicalIndex,
                                                                                            out numDisplayModes,
                                                                                            out displayModeBuffer);
                                            if (ADLRet == ADL_STATUS.ADL_OK)
                                            {
                                                SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_Modes_Get returned information about the display modes used by display #{displayTarget.DisplayID.DisplayLogicalAdapterIndex} connected to AMD adapter {adapterIndex}.");
                                            }
                                            else
                                            {
                                                SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - ADL2_Display_Modes_Get returned ADL_STATUS {ADLRet} when trying to get the display modes from AMD adapter {adapterIndex} in the computer.");
                                                continue;
                                            }

                                            ADL_MODE[] displayModeArray = new ADL_MODE[numDisplayModes];
                                            if (numDisplayModes > 0)
                                            {
                                                IntPtr currentDisplayModeBuffer = displayModeBuffer;
                                                for (int i = 0; i < numDisplayModes; i++)
                                                {
                                                    // build a structure in the array slot
                                                    displayModeArray[i] = new ADL_MODE();
                                                    // fill the array slot structure with the data from the buffer
                                                    displayModeArray[i] = (ADL_MODE)Marshal.PtrToStructure(currentDisplayModeBuffer, typeof(ADL_MODE));
                                                    // destroy the bit of memory we no longer need
                                                    //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                                    // advance the buffer forwards to the next object
                                                    currentDisplayModeBuffer = (IntPtr)((long)currentDisplayModeBuffer + Marshal.SizeOf(displayModeArray[i]));
                                                }
                                                // Free the memory used by the buffer                        
                                                Marshal.FreeCoTaskMem(displayModeBuffer);

                                                // Add the slsOffsets to the config we want to store
                                                //mySLSMapConfig.SLSOffsets = displayModeArray.ToList();

                                            }

                                            // If Eyefinity is enabled for this adapter, then the display mode of an
                                            // attached display target will match one of the SLS display modes reported by
                                            // ADL_Display_SLSMapConfig_Get(). The match will either be with "native" SLS 
                                            // modes (which are not bezel-compensated), or with "bezel" SLS modes which are.
                                            // 
                                            // So, simply compare current display mode against all the ones listed for the
                                            // SLS native or bezel-compensated modes: if there is a match, then the mode
                                            // currently used by this adapter is an Eyefinity/SLS mode, and Eyefinity is enabled.
                                            // First check the native SLS mode list
                                            // Process the slsOffsetBuffer
                                            bool isSlsEnabled = false;
                                            foreach (var displayMode in displayModeArray)
                                            {
                                                foreach (var nativeMode in nativeModeArray)
                                                {
                                                    if (nativeMode.DisplayMode.XRes == displayMode.XRes && nativeMode.DisplayMode.YRes == displayMode.YRes)
                                                    {
                                                        isSlsEnabled = true;
                                                        break;
                                                    }

                                                }

                                                // If no match was found, check the bezel-compensated SLS mode list
                                                if (!isSlsEnabled)
                                                {
                                                    foreach (var bezelMode in bezelModeArray)
                                                    {
                                                        if (bezelMode.DisplayMode.XRes == displayMode.XRes && bezelMode.DisplayMode.YRes == displayMode.YRes)
                                                        {
                                                            isSlsEnabled = true;
                                                            break;
                                                        }
                                                    }
                                                }

                                                // Now we check which slot we need to put this display into
                                                if (isSlsEnabled)
                                                {
                                                    // SLS is enabled for this display
                                                    if (!myDisplayConfig.Adl2SlsConfig.SLSEnabledDisplayTargets.Contains(displayMode))
                                                    {
                                                        myDisplayConfig.Adl2SlsConfig.SLSEnabledDisplayTargets.Add(displayMode);
                                                    }
                                                    // we also update the main IsSLSEnabled so that it is indicated at the top level too

                                                    myDisplayConfig.Adl2SlsConfig.IsSlsEnabled = true;
                                                    SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} has a matching SLS grid set! Eyefinity (SLS) is enabled. Setting IsSlsEnabled to true");

                                                }
                                            }

                                        }

                                        // Only Add the mySLSMapConfig to the displayConfig if SLS is enabled
                                        if (myDisplayConfig.Adl2SlsConfig.IsSlsEnabled)
                                        {
                                            myDisplayConfig.Adl2SlsConfig.SLSMapConfigs.Add(mySLSMapConfig);
                                        }

                                    }
                                    else
                                    {
                                        // If we get here then there there was no active SLSGrid, meaning Eyefinity is disabled!
                                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} has no active SLS grids set! Eyefinity (SLS) hasn't even been setup yet. Keeping the default IsSlsEnabled value of false.");
                                    }
                                }
                                else
                                {
                                    // If we get here then there are less than two displays connected. Eyefinity cannot be enabled in this case!
                                    SharedLogger.logger.Info($"AMDLibrary/GetAMDDisplayConfig: There are less than two displays connected to this adapter so Eyefinity cannot be enabled.");
                                }

                            }

                        }
                        else
                        {
                            // Free the memory used by the buffer                        
                            Marshal.FreeCoTaskMem(adapterInfoBuffer);
                            // Return the default config as there are no adapters to get info from
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - ADL2_Adapter_AdapterInfoX4_Get returned ADL_STATUS {ADLRet} when trying to get the adapter info about all AMD Adapters. Trying to skip this adapter so something at least works.");
                    }



                }
                else
                {
                    SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - Tried to get the AMD Eyefinity layout using the older ADL2 AP but the AMD ADL2 library isn't initialised!");
                    myDisplayConfig.Adl2SlsConfig = new AMD_SLS_CONFIG();
                }

            }
            else
            {
                SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - Tried to run GetAMDDisplayConfig but the AMD ADL library isn't initialised!");
                return CreateDefaultConfig();
            }

            // Read per-GPU 3D settings (keyed by stable hardware identity, not session UniqueId)
            try
            {
                var threeDServices = _adlxSystem.Get3DSettingsServices();
                var gpusFor3D = _adlxSystem.EnumerateADLXGPUs();
                foreach (var gpu in gpusFor3D)
                {
                    string gpuKey = $"{gpu.DeviceId}_{gpu.SubSystemId}_{gpu.SubSystemVendorId}_{gpu.RevisionId}";
                    if (!myDisplayConfig.GPUs.TryGetValue(gpuKey, out var gpuSettings))
                        gpuSettings = new AMD_GPU_WITH_SETTINGS();
                    if (threeDServices.TryGetAll3DSettings(gpu.UniqueId, out var settings3D))
                    {
                        gpuSettings.ThreeDSettings = settings3D;
                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Read 3D settings for GPU {gpuKey}.");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Could not read 3D settings for GPU {gpuKey}, skipping.");
                    }
                    myDisplayConfig.GPUs[gpuKey] = gpuSettings;
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex, "AMDLibrary/GetAMDDisplayConfig: Failed to read per-GPU 3D settings via ADLX.");
            }

            // Read per-GPU multimedia settings
            try
            {
                var mmServices = _adlxSystem.GetMultimediaServices();
                var gpusForMm = _adlxSystem.EnumerateADLXGPUs();
                foreach (var gpu in gpusForMm)
                {
                    string gpuKey = $"{gpu.DeviceId}_{gpu.SubSystemId}_{gpu.SubSystemVendorId}_{gpu.RevisionId}";
                    if (!myDisplayConfig.GPUs.TryGetValue(gpuKey, out var gpuSettings))
                        gpuSettings = new AMD_GPU_WITH_SETTINGS();
                    if (mmServices.TryGetVideoUpscale(gpu.UniqueId, out var videoUpscale))
                    {
                        gpuSettings.VideoUpscale = videoUpscale;
                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Read VideoUpscale for GPU {gpuKey}.");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Could not read VideoUpscale for GPU {gpuKey}, skipping.");
                    }
                    if (mmServices.TryGetVideoSuperResolution(gpu.UniqueId, out var videoSuperResolution))
                    {
                        gpuSettings.VideoSuperResolution = videoSuperResolution;
                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Read VideoSuperResolution for GPU {gpuKey}.");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Could not read VideoSuperResolution for GPU {gpuKey}, skipping.");
                    }
                    myDisplayConfig.GPUs[gpuKey] = gpuSettings;
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex, "AMDLibrary/GetAMDDisplayConfig: Failed to read per-GPU multimedia settings via ADLX.");
            }

            // Return the configuration
            return myDisplayConfig;
        }


        public string PrintActiveConfig()
        {
            if (!_initialised)
            {
                SharedLogger.logger.Error($"AMDLibrary/PrintActiveConfig: ERROR - Tried to run PrintActiveConfig but the AMD ADLX library isn't initialised!");
                throw new AMDLibraryException($"Tried to run PrintActiveConfig but the AMD ADLX library isn't initialised!");
            }

            AMD_DISPLAY_CONFIG displayConfig = ActiveDisplayConfig;
            var sb = new StringBuilder();

            sb.AppendLine("****** AMD VIDEO CARDS *******");
            sb.AppendLine($"IsInUse: {displayConfig.IsInUse}");
            sb.AppendLine($"IsCloned: {displayConfig.IsCloned}");
            sb.AppendLine($"IsEyefinity: {displayConfig.IsEyefinity}");
            sb.AppendLine();

            // GPUs
            try
            {
                var gpus = _adlxSystem.EnumerateADLXGPUs();
                foreach (var gpu in gpus)
                {
                    var id = gpu.Identity;
                    sb.AppendLine($"Adapter UniqueId: {id.UniqueId}");
                    sb.AppendLine($"Name: {id.Name}");
                    sb.AppendLine($"VendorId: {id.VendorId}");
                    sb.AppendLine($"ProductName: {id.ProductName}");
                    sb.AppendLine($"GPU Type: {id.GPUType}");
                    sb.AppendLine($"Asic Family: {id.AsicFamilyType}");
                    sb.AppendLine($"VRAM: {id.TotalVRAM} ({id.VRAMType})");
                    sb.AppendLine($"PCI Bus: {id.PciBusType} LaneWidth: {id.PciBusLaneWidth}");
                    sb.AppendLine($"External: {id.IsExternal} ");
                    sb.AppendLine($"HasDesktops: {id.HasDesktops}");
                    sb.AppendLine($"Driver: {id.DriverVersion} ");
                    sb.AppendLine($"AMDSoftware: {id.AMDSoftwareVersion} ");
                    sb.AppendLine($"WindowsDriver: {id.AMDWindowsDriverVersion}");
                    sb.AppendLine($"DeviceId: {id.DeviceId} SubSystem: {id.SubSystemId} SubVendor: {id.SubSystemVendorId} Revision: {id.RevisionId}");
                    sb.AppendLine($"PNP: {id.PNPString}");

                    // Per-GPU settings (3D and multimedia) from stored config
                    string gpuKey = $"{id.DeviceId}_{id.SubSystemId}_{id.SubSystemVendorId}_{id.RevisionId}";
                    if (displayConfig.GPUs.TryGetValue(gpuKey, out var gpuSettings))
                    {
                        if (gpuSettings.ThreeDSettings.HasValue)
                        {
                            var s3d = gpuSettings.ThreeDSettings.Value;
                            sb.AppendLine($"3D AntiLag: Supported={s3d.AntiLag?.IsSupported} Enabled={s3d.AntiLag?.IsEnabled} Level={s3d.AntiLag?.Level}");
                            sb.AppendLine($"3D Chill: Supported={s3d.Chill?.IsSupported} Enabled={s3d.Chill?.IsEnabled} MinFPS={s3d.Chill?.MinFPS} MaxFPS={s3d.Chill?.MaxFPS}");
                            sb.AppendLine($"3D Boost: Supported={s3d.Boost?.IsSupported} Enabled={s3d.Boost?.IsEnabled} MinRes={s3d.Boost?.MinResolution}");
                            sb.AppendLine($"3D RadeonImageSharpening: Supported={s3d.ImageSharpening?.IsSupported} Enabled={s3d.ImageSharpening?.IsEnabled} Sharpness={s3d.ImageSharpening?.Sharpness}");
                            sb.AppendLine($"3D EnhancedSync: Supported={s3d.EnhancedSync?.IsSupported} Enabled={s3d.EnhancedSync?.IsEnabled}");
                            sb.AppendLine($"3D WaitForVerticalRefresh: Supported={s3d.WaitForVerticalRefresh?.IsSupported} Mode={s3d.WaitForVerticalRefresh?.Mode}");
                            sb.AppendLine($"3D FrameRateTargetControl: Supported={s3d.FrameRateTargetControl?.IsSupported} Enabled={s3d.FrameRateTargetControl?.IsEnabled} FPS={s3d.FrameRateTargetControl?.Fps}");
                            sb.AppendLine($"3D AntiAliasing: Supported={s3d.AntiAliasing?.IsSupported} Mode={s3d.AntiAliasing?.Mode}");
                            sb.AppendLine($"3D MorphologicalAntiAliasing: Supported={s3d.MorphologicalAntiAliasing?.IsSupported} Enabled={s3d.MorphologicalAntiAliasing?.IsEnabled}");
                            sb.AppendLine($"3D AnisotropicFiltering: Supported={s3d.AnisotropicFiltering?.IsSupported} Level={s3d.AnisotropicFiltering?.Level}");
                            sb.AppendLine($"3D Tessellation: Supported={s3d.Tessellation?.IsSupported} Mode={s3d.Tessellation?.Mode} Level={s3d.Tessellation?.Level}");
                            sb.AppendLine($"3D FluidMotionFrames: Supported={s3d.FluidMotionFrames?.IsSupported} Enabled={s3d.FluidMotionFrames?.IsEnabled}");
                            sb.AppendLine($"3D RadeonSuperResolution: Supported={s3d.RadeonSuperResolution?.IsSupported} Enabled={s3d.RadeonSuperResolution?.IsEnabled}");
                            sb.AppendLine($"3D ImageSharpenDesktop: Supported={s3d.ImageSharpenDesktop?.IsSupported} Enabled={s3d.ImageSharpenDesktop?.IsEnabled}");
                        }
                        else
                        {
                            sb.AppendLine("3D Settings: not available");
                        }
                        if (gpuSettings.VideoUpscale.HasValue)
                        {
                            var vu = gpuSettings.VideoUpscale.Value;
                            sb.AppendLine($"MM VideoUpscale: Supported={vu.IsSupported} Enabled={vu.IsEnabled} Sharpness={vu.Sharpness} SharpnessRange=({vu.SharpnessRange.MinValue},{vu.SharpnessRange.MaxValue},{vu.SharpnessRange.Step})");
                        }
                        else
                        {
                            sb.AppendLine("MM VideoUpscale: not available");
                        }
                        if (gpuSettings.VideoSuperResolution.HasValue)
                        {
                            var vsr = gpuSettings.VideoSuperResolution.Value;
                            sb.AppendLine($"MM VideoSuperResolution: Supported={vsr.IsSupported} Enabled={vsr.IsEnabled}");
                        }
                        else
                        {
                            sb.AppendLine("MM VideoSuperResolution: not available");
                        }
                    }
                    else
                    {
                        sb.AppendLine("GPU settings: not stored for this GPU");
                    }
                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex, "AMDLibrary/PrintActiveConfig: Failed to enumerate GPUs via ADLX");
                sb.AppendLine();
                sb.AppendLine();
                return sb.ToString();
            }

            // Displays from the active display config
            sb.AppendLine("AMD DISPLAYS");
            foreach (var kvp in displayConfig.Displays)
            {
                var display = kvp.Value;
                sb.AppendLine($"Display UniqueId: {display.UniqueID}");
                sb.AppendLine($"Name: {display.Name}");
                sb.AppendLine($"ManufacturerID: {display.ManufacturerID}");
                sb.AppendLine($"EDID: {display.EDID}");
                sb.AppendLine($"GPU UniqueId: {display.GpuUniqueID}");
                sb.AppendLine($"Type: {display.Type} ");
                sb.AppendLine($"Connector: {display.ConnectorType}");
                sb.AppendLine($"Native: {display.NativeResolutionWidth}x{display.NativeResolutionHeight} @ {display.RefreshRate}Hz PixelClock: {display.PixelClock}");
                sb.AppendLine($"ScanType: {display.ScanType}");
                sb.AppendLine($"ColorDepth: {(display.IsSupportedColorDepth ? display.ColorDepth.ToString() : "Unsupported")}");

                // Custom color
                var cc = display.CustomColorInfo;
                sb.AppendLine($"CustomColor Supported:{cc.IsSupported} ");
                sb.AppendLine($"Hue({cc.IsHueSupported}:{cc.Hue}) ");
                sb.AppendLine($"Sat({cc.IsSaturationSupported}:{cc.Saturation}) ");
                sb.AppendLine($"Bright({cc.IsBrightnessSupported}:{cc.Brightness}) ");
                sb.AppendLine($"Contrast({cc.IsContrastSupported}:{cc.Contrast}) ");
                sb.AppendLine($"Temp({cc.IsTemperatureSupported}:{cc.Temperature})");

                // Gamma/Gamut
                var gamma = display.GammaInfo;
                sb.AppendLine($"Gamma Supported:{gamma.IsSupported} ");
                sb.AppendLine($"Modes: SRGB={gamma.IsCurrentReGammaSRGB} ");
                sb.AppendLine($"BT709={gamma.IsCurrentReGammaBT709} ");
                sb.AppendLine($"PQ={gamma.IsCurrentReGammaPQ} ");
                sb.AppendLine($"PQ2084={gamma.IsCurrentReGammaPQ2084} ");
                sb.AppendLine($"ReGamma36={gamma.IsCurrentReGamma36} ");                
                sb.AppendLine($"RegammaCoeff={gamma.HasRegammaCoefficient} ");
                sb.AppendLine($"ReRamp={gamma.HasReGammaRamp} ");
                sb.AppendLine($"DeRamp={gamma.HasDeGammaRamp}");
                var gamut = display.GamutInfo;
                sb.AppendLine($"Gamut Supported:{gamut.IsGamutSupported} ");
                sb.AppendLine($"WhitePointSupported:{gamut.IsWhitePointSupported} ");
                sb.AppendLine($"Current: 5000K={gamut.IsCurrent5000K} 6500K={gamut.IsCurrent6500K} 7500K={gamut.IsCurrent7500K} 9300K={gamut.IsCurrent9300K} CustomWP={gamut.IsCurrentCustomWhitePoint} 709={gamut.IsCurrent709} 601={gamut.IsCurrent601} Adobe={gamut.IsCurrentAdobe} CIERgb={gamut.IsCurrentCieRgb} 2020={gamut.IsCurrent2020} CustomSpace={gamut.IsCurrentCustomColorSpace} WP=({gamut.WhitePointX},{gamut.WhitePointY})");

                // Connectivity
                var conn = display.ConnectivityExperience;
                sb.AppendLine($"Connectivity: HDMIQuality(supported={conn.IsHdmiQualityDetectionSupported},enabled={conn.IsHdmiQualityDetectionEnabled}) ");
                sb.AppendLine($"DPLink(supported={conn.IsDpLinkRateSupported},rate={conn.DpLinkRate}) ");
                sb.AppendLine($"PreEmphasis(supported={conn.IsRelativePreEmphasisSupported},value={conn.RelativePreEmphasis}) ");
                sb.AppendLine($"VoltageSwing(supported={conn.IsRelativeVoltageSwingSupported},value={conn.RelativeVoltageSwing})");

                // 3DLUT
                var lut = display.ThreeDLUTSettings;
                sb.AppendLine($"3DLUT SupportedSCE:{lut.IsSupportedSCE} ");
                sb.AppendLine($"Vivid:{lut.IsSupportedSCEVividGaming} ");                
                sb.AppendLine($"DynContrast:{lut.IsSupportedSCEDynamicContrast} ");
                sb.AppendLine($"User3DLUT:{lut.IsSupportedUser3DLUT} ");
                sb.AppendLine($"CurrentDisabled:{lut.IsCurrentSCEDisabled} ");
                sb.AppendLine($"CurrentVivid:{lut.IsCurrentSCEVividGaming} ");
                sb.AppendLine($"DynContrastEnabled:{lut.HasDynamicContrast} DynContrastValue:{lut.CurrentDynamicContrastValue} Range({lut.DynamicContrastRange.Min},{lut.DynamicContrastRange.Max},{lut.DynamicContrastRange.Step})");

                // Feature toggles
                sb.AppendLine($"FreeSync: Supported={display.IsSupportedFreeSync} Enabled={display.IsEnabledFreeSync}");
                sb.AppendLine($"FreeSyncColorAccuracy: Supported={display.IsSupportedFreeSyncColorAccuracy} Enabled={display.IsEnabledFreeSyncColorAccuracy}");
                sb.AppendLine($"DynamicRefreshRateControl: Supported={display.IsSupportedDynamicRefreshRateControl} Enabled={display.IsEnabledDynamicRefreshRateControl}");
                sb.AppendLine($"DisplayBlanking: Supported={display.IsSupportedDisplayBlanking} Enabled={display.IsEnabledDisplayBlanking}");
                sb.AppendLine($"GPUScaling: Supported={display.IsSupportedGPUScaling} Enabled={display.IsEnabledGPUScaling}");
                sb.AppendLine($"IntegerScaling: Supported={display.IsSupportedIntegerScaling} Enabled={display.IsEnabledIntegerScaling}");
                sb.AppendLine($"PixelFormat: Supported={display.IsSupportedPixelFormat} Current={display.CurrentPixelFormat}");
                sb.AppendLine($"ScalingMode: Supported={display.IsSupportedScalingMode} Current={display.CurrentScalingMode}");
                sb.AppendLine($"VSR: Supported={display.IsSupportedVSR} Enabled={display.IsEnabledVSR}");
                sb.AppendLine($"HDCP: Supported={display.IsSupportedHDCP} Enabled={display.IsEnabledHDCP}");
                sb.AppendLine($"VariBright: Supported={display.IsSupportedVariBright} Enabled={display.IsEnabledVariBright} Mode={display.VariBrightMode}");

                sb.AppendLine();
            }

            // Eyefinity/SLS summary from stored config (for ADL2 configuration)
            sb.AppendLine("AMD EYEFINITY via ADL2 (SLS)");
            if (displayConfig.Adl2SlsConfig.IsSlsEnabled)
            {
                sb.AppendLine("AMD Eyefinity via ADL2 is Enabled");
                if (displayConfig.Adl2SlsConfig.SLSMapConfigs.Count > 0)
                {
                    int idx = 0;
                    foreach (var slsMap in displayConfig.Adl2SlsConfig.SLSMapConfigs)
                    {
                        sb.AppendLine($"SLS #{idx}: Grid {slsMap.SLSMap.Grid.SLSGridColumn}x{slsMap.SLSMap.Grid.SLSGridRow}, Targets={slsMap.SLSMap.NumSLSTarget}, BezelPercent={slsMap.BezelModePercent}");
                        idx++;
                    }
                }
            }
            else
            {
                sb.AppendLine("AMD Eyefinity via ADL2 (SLS) is Disabled");
            }

            sb.AppendLine();

            // Eyefinity Desktop (ADLX)
            sb.AppendLine("AMD EYEFINITY DESKTOP (ADLX)");
            if (displayConfig.IsEyefinity)
            {
                var ef = displayConfig.EyefinityDesktop;
                sb.AppendLine($"  Rows: {ef.Rows} Columns: {ef.Columns}");
                sb.AppendLine($"  Orientation: {ef.Orientation}");
                sb.AppendLine($"  Size: {ef.SizeWidth}x{ef.SizeHeight}");
                sb.AppendLine($"  TopLeft: ({ef.TopLeftX},{ef.TopLeftY})");
            }
            else
            {
                sb.AppendLine("  Not in use");
            }
            sb.AppendLine();

            // Desktops
            sb.AppendLine("AMD DESKTOPS");
            if (displayConfig.Desktops != null && displayConfig.Desktops.Count > 0)
            {
                int deskIdx = 0;
                foreach (var desktop in displayConfig.Desktops)
                {
                    sb.AppendLine($"  Desktop #{deskIdx}: Type={desktop.Type} Orientation={desktop.Orientation} Size={desktop.SizeWidth}x{desktop.SizeHeight} TopLeft=({desktop.TopLeftX},{desktop.TopLeftY}) Displays={desktop.NumberOfDisplays}");
                    deskIdx++;
                }
            }
            else
            {
                sb.AppendLine("  No desktops stored.");
            }
            sb.AppendLine();

            // Display Identifiers
            if (displayConfig.DisplayIdentifiers != null && displayConfig.DisplayIdentifiers.Count > 0)
            {
                sb.AppendLine("AMD DISPLAY IDENTIFIERS");
                foreach (var id in displayConfig.DisplayIdentifiers)
                {
                    sb.AppendLine($"  {id}");
                }
                sb.AppendLine();
            }

            sb.AppendLine();

            return sb.ToString();
        }

        public bool SetActiveConfig(AMD_DISPLAY_CONFIG displayConfig, bool useADLEyefinity, int delayInMs)
        {

            if (_initialised)
            {
                // This is how we control the various desktops. 
                // - A single desktop is associated with one display.
                // - A duplicate desktop is associated with two or more displays.
                // - An AMD Eyefinity desktop is associated with two or more displays.
                //return true;

                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: Checking if Eyefinity is required for the new display layout");

                // If the display config needs an Eyefinity Desktop then lets create one.
                if (displayConfig.IsEyefinity)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: New display layout requires an Eyefinity desktop");

                    // Check if we are using the new ADLX or older ADL API to create the Eyefinity Desktop
                    if (useADLEyefinity)
                    {                          
                        // If set then we are using the older ADL API to create the Eyefinity Desktop
                        SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: Using the older ADL API to create the Eyefinity Desktop.");
                        
                        // Set the initial state of the ADL_STATUS
                        ADL_STATUS ADLRet = 0;
                        foreach (AMD_SLSMAP_CONFIG slsMapConfig in displayConfig.Adl2SlsConfig.SLSMapConfigs)
                        {
                            // Attempt to turn on this SLS Map Config if it exists in the AMD Radeon driver config database
                            ADLRet = ADLImport.ADL2_Display_SLSMapConfig_SetState(_adlContextHandle, slsMapConfig.SLSMap.AdapterIndex, slsMapConfig.SLSMap.SLSMapIndex, ADLImport.ADL_TRUE);
                            if (ADLRet == ADL_STATUS.ADL_OK)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: ADL2_Display_SLSMapConfig_SetState successfully set the SLSMAP with index {slsMapConfig.SLSMap.SLSMapIndex} to TRUE for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                            }
                            else
                            {
                                SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ERROR - ADL2_Display_SLSMapConfig_SetState returned ADL_STATUS {ADLRet} when trying to set the SLSMAP with index {slsMapConfig.SLSMap.SLSMapIndex} to TRUE for adapter {slsMapConfig.SLSMap.AdapterIndex}.");

                                // If we get an error with just tturning it on, then we need to actually try to created a new Eyefinity map and then enable it
                                // If we reach this stage, then the user has discarded the AMD Eyefinity mode in AMD due to a bad UI design, and we need to work around that slight issue.
                                // (BTW that's FAR to easy to do in the AMD Radeon GUI)
                                // NOTE: There is a slight issue with way of doing things. Although we create a much more robust way of working, we also will never ever actually use the Eyefinity config as saved.
                                //       Instead, we will always drop through to creating an Eyefinity config each time, the only saving grace being that the AMD Driver is smart enough to notice this and it will reuse the same SLSMapIndex number.
                                //       This at least means that we won't keep filling the AMD Driver up with additional EYefinity configs! It will instaed only add one more additional AMD Config if it works this way.

                                int supportedSLSLayoutImageMode;
                                int reasonForNotSupportSLS;
                                ADLRet = ADLImport.ADL2_Display_SLSMapConfig_Valid(_adlContextHandle, slsMapConfig.SLSMap.AdapterIndex, slsMapConfig.SLSMap, slsMapConfig.SLSTargets.Count, slsMapConfig.SLSTargets.ToArray(), out supportedSLSLayoutImageMode, out reasonForNotSupportSLS, ADLImport.ADL_DISPLAY_SLSMAPCONFIG_CREATE_OPTION_RELATIVETO_CURRENTANGLE);
                                if (ADLRet == ADL_STATUS.ADL_OK)
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: ADL2_Display_SLSMapConfig_Valid successfully validated a new SLSMAP config for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                                }
                                else
                                {
                                    SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ERROR - ADL2_Display_SLSMapConfig_Valid returned ADL_STATUS {ADLRet} when trying to create a new SLSMAP for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                                    return false;
                                }

                                // Create and apply the new SLSMap
                                int newSlsMapIndex;
                                ADLRet = ADLImport.ADL2_Display_SLSMapConfig_Create(_adlContextHandle, slsMapConfig.SLSMap.AdapterIndex, slsMapConfig.SLSMap, slsMapConfig.SLSTargets.Count, slsMapConfig.SLSTargets.ToArray(), slsMapConfig.BezelModePercent, out newSlsMapIndex, ADLImport.ADL_DISPLAY_SLSMAPCONFIG_CREATE_OPTION_RELATIVETO_CURRENTANGLE);
                                if (ADLRet == ADL_STATUS.ADL_OK)
                                {
                                    if (newSlsMapIndex != -1)
                                    {
                                        SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: ADL2_Display_SLSMapConfig_Create successfully created the new SLSMAP we just created with index {newSlsMapIndex} to TRUE for adapter {slsMapConfig.SLSMap.AdapterIndex}.");

                                        // At this point we have created a new AMD Eyefinity Config
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ERROR - ADL2_Display_SLSMapConfig_Create returned ADL_STATUS {ADLRet} but the returned SLSMapIndex was -1, which indicates that the new SLSMAP failed to create for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ERROR - ADL2_Display_SLSMapConfig_Create returned ADL_STATUS {ADLRet} when trying to create a new SLSMAP for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                                    return false;
                                }

                                // Make the changes permanent
                                ADLRet = ADLImport.ADL2_Flush_Driver_Data(_adlContextHandle, slsMapConfig.SLSMap.AdapterIndex);
                                if (ADLRet == ADL_STATUS.ADL_OK)
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: ADL2_Flush_Driver_Data successfully saved the adapter settings as permanent for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                                }
                                else
                                {
                                    SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ADL2_Flush_Driver_Data failed to save the adapter settings as permanent for adapter {slsMapConfig.SLSMap.AdapterIndex}. ");
                                    return false;
                                }
                            }

                        }
                    }
                    else
                    {
                        // Otherwise we are using the newer ADLX API to create the Eyefinity Desktop
                        SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: Using the newer ADLX API to create the Eyefinity Desktop.");
                        if (displayConfig.EyefinityDesktop.Equals(ActiveDisplayConfig.EyefinityDesktop))
                        {
                            // If the Eyefinity Desktop is already set then we don't need to do anything
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: Eyefinity layout is exactly the same as the one we want, so skipping setting up the Eyefinity Desktop");
                        }
                        else
                        {
                            // Otherwise we need to use the new ADLX API to create the Eyefinity Desktop
                            // Setup the EyefinityDesktop using the settings the driver stores internally
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: Attempting to create the ADLX EyefinityDesktop");

                            try
                            {
                                // Get the Desktop Services                            
                                var desktopService = _adlxSystem.GetDesktopServices();
                                // Create the Eyefinity Desktop
                                desktopService.CreateEyefinityDesktop();
                                // Check if it matches what we want
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: Successfully created the ADLX Eyefinity Desktop");
                                if (displayConfig.EyefinityDesktop.Equals(ActiveDisplayConfig.EyefinityDesktop))
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: This new Eyefinity layout matches the desired configuraton");
                                }
                                else
                                {
                                    SharedLogger.logger.Warn($"AMDLibrary/SetActiveConfig: This new Eyefinity layout is different from the one we originally saved with this desktop profile. If you have changed your Eyefinity Layout then you need to update this desktop profile!.");
                                }
                                
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, "AMDLibrary/SetActiveConfig: Exception occurred while trying to create the Eyefinity Desktop using ADLX");
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: New display layout does NOT requires a Eyefinity desktop");

                    if (ActiveDisplayConfig.IsEyefinity)
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: Eyefinity layout is currently in use but is NOT required, so we need to destroy the Eyefinity Desktop");

                        // Check if we are using the new ADLX or older ADL API to destroy the Eyefinity Desktop
                        if (useADLEyefinity)
                        {
                            // If set then we are using the older ADL API to destroy the Eyefinity Desktop
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: Using the older ADL API to destroy the Eyefinity Desktop.");

                            // We need to disable the current Eyefinity (SLS) profile to turn it off
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: SLS is enabled in the current display configuration, so we need to turn it off");
                            // Set the initial state of the ADL_STATUS
                            ADL_STATUS ADLRet = 0;

                            foreach (AMD_SLSMAP_CONFIG slsMapConfig in ActiveDisplayConfig.Adl2SlsConfig.SLSMapConfigs)
                            {
                                // Turn off this SLS Map Config
                                ADLRet = ADLImport.ADL2_Display_SLSMapConfig_SetState(_adlContextHandle, slsMapConfig.SLSMap.AdapterIndex, slsMapConfig.SLSMap.SLSMapIndex, ADLImport.ADL_FALSE);
                                if (ADLRet == ADL_STATUS.ADL_OK)
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: ADL2_Display_SLSMapConfig_SetState successfully disabled the SLSMAP with index {slsMapConfig.SLSMap.SLSMapIndex} for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                                }
                                else
                                {
                                    SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ERROR - ADL2_Display_SLSMapConfig_SetState returned ADL_STATUS {ADLRet} when trying to set the SLSMAP with index {slsMapConfig.SLSMap.SLSMapIndex} to FALSE for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                                    return false;
                                }

                                // Make the changes permanent
                                ADLRet = ADLImport.ADL2_Flush_Driver_Data(_adlContextHandle, slsMapConfig.SLSMap.AdapterIndex);
                                if (ADLRet == ADL_STATUS.ADL_OK)
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: ADL2_Flush_Driver_Data successfully saved the adapter settings as permanent for adapter {slsMapConfig.SLSMap.AdapterIndex} (after disable).");
                                }
                                else
                                {
                                    SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ADL2_Flush_Driver_Data failed to save the adapter settings as permanent for adapter {slsMapConfig.SLSMap.AdapterIndex} (after disable).");
                                    return false;
                                }

                            }
                        }
                        else
                        {
                            // Otherwise we are using the new ADLX API to destroy the Eyefinity Desktop
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: Using the newer ADLX API to destroy the Eyefinity Desktop.");

                            try
                            {
                                // Get the Desktop Services                            
                                var desktopService = _adlxSystem.GetDesktopServices();
                                // Destroy All Eyefinity Desktops
                                desktopService.DestroyAllEyefinityDesktops();
                                // Check if it matches what we want
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: Successfully destroyed the ADLX Eyefinity Desktop");
                                if (desktopService.EnumerateDesktops().Any(d => d.Type == ADLX_DESKTOP_TYPE.DESKTOP_EYEFINITY))
                                {
                                    SharedLogger.logger.Warn($"AMDLibrary/SetActiveConfig: There are still Eyefinity displays configured even after destroying all Eyefinity desktops! Something is wrong.");
                                    return false;                                    
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: There are no Eyefinity displays currently configured, just as we wanted.");
                                }                                
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, "AMDLibrary/SetActiveConfig: Exception occurred while trying to destroy the Eyefinity Desktops using ADLX");
                                return false;
                            }

                        }

                    }
                    else
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: Eyefinity layout is not currently in use and is NOT required, so leaving things as they are.");
                    }
                }                    
            

            }
            else
            {
                SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ERROR - Tried to run SetActiveConfig but the AMD ADLX library isn't initialised!");
                throw new AMDLibraryException($"Tried to run SetActiveConfig but the AMD ADLX library isn't initialised!");
            }

            return true;
        }


        public bool SetActiveConfigOverride(AMD_DISPLAY_CONFIG displayConfig, int delayInMs)
        {
            if (_initialised)
            {                
                // Get the current list of all displays available on the system
                var displaysList = _adlxSystem.EnumerateDisplays().ToList();
                if (displaysList.Count == 0)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: No displays found in ADLX system services");
                    return false;
                }

                foreach (var display in displaysList)
                {
                    // Only apply settings we have stored
                    if (!displayConfig.Displays.TryGetValue(display.UniqueId, out var stored))
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: No stored settings for display {display.UniqueId}, skipping.");
                        continue;
                    }

                    try
                    {
                        // Color depth
                        var currentColorDepth = display.GetColorDepthState();
                        if (currentColorDepth.supported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: ColorDepth is supported for display {display.UniqueId}.");
                            if (currentColorDepth.current != stored.ColorDepth)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: ColorDepth is currently {currentColorDepth.current} for display {display.UniqueId} and needs to be {stored.ColorDepth}.");
                                display.SetColorDepth(stored.ColorDepth);
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: ColorDepth already {currentColorDepth.current} for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: ColorDepth NOT supported for display {display.UniqueId}.");
                        }

                        // Custom color (includes support flags inside AMD_CUSTOM_COLOR_INFO)
                        var currentCustomColor = new AMD_CUSTOM_COLOR_INFO(display.GetCustomColor());
                        if (!currentCustomColor.Equals(stored.CustomColorInfo))
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: CustomColor differs for display {display.UniqueId}, applying stored values.");
                            display.ApplyCustomColor(stored.CustomColorInfo.ToCustomColorDto());
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: CustomColor already matches for display {display.UniqueId}, skipping.");
                        }

                        // Gamma best effort reapply
                        var currentGamma = new AMD_GAMMA_INFO(display.GetGamma());
                        if (currentGamma.IsSupported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Gamma supported for display {display.UniqueId}.");
                            if (!currentGamma.Equals(stored.GammaInfo))
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Gamma differs for display {display.UniqueId}, reapplying.");
                                display.ReapplyGamma();
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Gamma already matches for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Gamma NOT supported for display {display.UniqueId}.");
                        }

                        // Gamut best effort reapply
                        var currentGamut = new AMD_GAMUT_INFO(display.GetGamut());
                        if (currentGamut.IsGamutSupported || currentGamut.IsWhitePointSupported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Gamut supported for display {display.UniqueId}.");
                            if (!currentGamut.Equals(stored.GamutInfo))
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Gamut differs for display {display.UniqueId}, reapplying.");
                                display.ReapplyGamut();
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Gamut already matches for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Gamut NOT supported for display {display.UniqueId}.");
                        }

                        // Connectivity experience
                        var currentConn = new AMD_CONNECTIVITY_EXPERIENCE_INFO(display.GetConnectivityExperience());
                        if (!currentConn.Equals(stored.ConnectivityExperience))
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Connectivity experience differs for display {display.UniqueId}, applying.");
                            display.ApplyConnectivityExperience(stored.ConnectivityExperience.ToConnectivityExperienceDto());
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Connectivity experience already matches for display {display.UniqueId}, skipping.");
                        }

                        // Apply 3DLUT
                        var current3dLut = new AMD_3DLUT_INFO(display.GetThreeDLut());
                        if (!current3dLut.Equals(stored.ThreeDLUTSettings))
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: 3DLUT differs for display {display.UniqueId}, reapplying.");
                            display.ReapplyThreeDLut();
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: 3DLUT already matches for display {display.UniqueId}, skipping.");
                        }

                        // FreeSync
                        var currentFreeSync = display.GetFreeSyncState();
                        if (currentFreeSync.supported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: FreeSync supported for display {display.UniqueId}.");
                            if (currentFreeSync.enabled != stored.IsEnabledFreeSync)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: FreeSync is {currentFreeSync.enabled} for display {display.UniqueId}, setting to {stored.IsEnabledFreeSync}.");
                                display.SetFreeSync(stored.IsEnabledFreeSync);
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: FreeSync already {currentFreeSync.enabled} for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: FreeSync NOT supported for display {display.UniqueId}.");
                        }

                        // FreeSync Color Accuracy
                        var currentFsColorAcc = display.GetFreeSyncColorAccuracyState();
                        if (currentFsColorAcc.supported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: FreeSync Color Accuracy supported for display {display.UniqueId}.");
                            if (currentFsColorAcc.enabled != stored.IsEnabledFreeSyncColorAccuracy)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: FreeSync Color Accuracy is {currentFsColorAcc.enabled} for display {display.UniqueId}, setting to {stored.IsEnabledFreeSyncColorAccuracy}.");
                                display.SetFreeSyncColorAccuracy(stored.IsEnabledFreeSyncColorAccuracy);
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: FreeSync Color Accuracy already {currentFsColorAcc.enabled} for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: FreeSync Color Accuracy NOT supported for display {display.UniqueId}.");
                        }

                        // Dynamic Refresh Rate Control
                        var currentDrrc = display.GetDynamicRefreshRateControlState();
                        if (currentDrrc.supported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: DRRC supported for display {display.UniqueId}.");
                            if (currentDrrc.enabled != stored.IsEnabledDynamicRefreshRateControl)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: DRRC is {currentDrrc.enabled} for display {display.UniqueId}, setting to {stored.IsEnabledDynamicRefreshRateControl}.");
                                display.SetDynamicRefreshRateControl(stored.IsEnabledDynamicRefreshRateControl);
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: DRRC already {currentDrrc.enabled} for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: DRRC NOT supported for display {display.UniqueId}.");
                        }

                        // Display blanking
                        var currentBlanking = display.GetDisplayBlankingState();
                        if (currentBlanking.supported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Display blanking supported for display {display.UniqueId}.");
                            if (currentBlanking.blanked != stored.IsEnabledDisplayBlanking)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Blanking is {currentBlanking.blanked} for display {display.UniqueId}, setting to {stored.IsEnabledDisplayBlanking}.");
                                display.SetDisplayBlanked(stored.IsEnabledDisplayBlanking);
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Blanking already {currentBlanking.blanked} for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Display blanking NOT supported for display {display.UniqueId}.");
                        }

                        // GPU scaling
                        var currentGpuScaling = display.GetGpuScalingState();
                        if (currentGpuScaling.supported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: GPU scaling supported for display {display.UniqueId}.");
                            if (currentGpuScaling.enabled != stored.IsEnabledGPUScaling)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: GPU scaling is {currentGpuScaling.enabled} for display {display.UniqueId}, setting to {stored.IsEnabledGPUScaling}.");
                                display.SetGpuScaling(stored.IsEnabledGPUScaling);
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: GPU scaling already {currentGpuScaling.enabled} for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: GPU scaling NOT supported for display {display.UniqueId}.");
                        }

                        // Integer scaling
                        var currentIntScaling = display.GetIntegerScalingState();
                        if (currentIntScaling.supported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Integer scaling supported for display {display.UniqueId}.");
                            if (currentIntScaling.enabled != stored.IsEnabledIntegerScaling)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Integer scaling is {currentIntScaling.enabled} for display {display.UniqueId}, setting to {stored.IsEnabledIntegerScaling}.");
                                display.SetIntegerScaling(stored.IsEnabledIntegerScaling);
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Integer scaling already {currentIntScaling.enabled} for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Integer scaling NOT supported for display {display.UniqueId}.");
                        }

                        // Pixel format
                        var currentPixelFormat = display.GetPixelFormatState();
                        if (currentPixelFormat.supported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Pixel format supported for display {display.UniqueId}.");
                            if (currentPixelFormat.current != stored.CurrentPixelFormat)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Pixel format is {currentPixelFormat.current} for display {display.UniqueId}, setting to {stored.CurrentPixelFormat}.");
                                display.SetPixelFormat(stored.CurrentPixelFormat);
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Pixel format already {currentPixelFormat.current} for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Pixel format NOT supported for display {display.UniqueId}.");
                        }

                        // Scaling mode
                        var currentScalingMode = display.GetScalingMode();
                        if (currentScalingMode.supported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Scaling mode supported for display {display.UniqueId}.");
                            if (currentScalingMode.mode != stored.CurrentScalingMode)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Scaling mode is {currentScalingMode.mode} for display {display.UniqueId}, setting to {stored.CurrentScalingMode}.");
                                display.SetScalingMode(stored.CurrentScalingMode);
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Scaling mode already {currentScalingMode.mode} for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Scaling mode NOT supported for display {display.UniqueId}.");
                        }

                        // Virtual Super Resolution
                        var currentVsr = display.GetVirtualSuperResolutionState();
                        if (currentVsr.supported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: VSR supported for display {display.UniqueId}.");
                            if (currentVsr.enabled != stored.IsEnabledVSR)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: VSR is {currentVsr.enabled} for display {display.UniqueId}, setting to {stored.IsEnabledVSR}.");
                                display.SetVirtualSuperResolution(stored.IsEnabledVSR);
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: VSR already {currentVsr.enabled} for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: VSR NOT supported for display {display.UniqueId}.");
                        }

                        // HDCP
                        var currentHdcp = display.GetHdcpState();
                        if (currentHdcp.supported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: HDCP supported for display {display.UniqueId}.");
                            if (currentHdcp.enabled != stored.IsEnabledHDCP)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: HDCP is {currentHdcp.enabled} for display {display.UniqueId}, setting to {stored.IsEnabledHDCP}.");
                                display.SetHdcp(stored.IsEnabledHDCP);
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: HDCP already {currentHdcp.enabled} for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: HDCP NOT supported for display {display.UniqueId}.");
                        }

                        // VariBright
                        var currentVariBright = display.GetVariBrightState();
                        if (currentVariBright.supported)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: VariBright supported for display {display.UniqueId}.");
                            if (currentVariBright.enabled != stored.IsEnabledVariBright || currentVariBright.mode != stored.VariBrightMode)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: VariBright is Enabled={currentVariBright.enabled} Mode={currentVariBright.mode} for display {display.UniqueId}, setting to Enabled={stored.IsEnabledVariBright} Mode={stored.VariBrightMode}.");
                                display.SetVariBright(stored.IsEnabledVariBright, stored.VariBrightMode);
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: VariBright already matches for display {display.UniqueId}, skipping.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: VariBright NOT supported for display {display.UniqueId}.");
                        }

                        // Custom resolutions (not applied here; API wiring still pending)

                        // Optional delay between displays if requested
                        if (delayInMs > 0)
                        {
                            Thread.Sleep(delayInMs);
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"AMDLibrary/SetActiveConfigOverride: Error applying settings for display {display.UniqueId}");
                        return false;
                    }
                }

                // Apply per-GPU 3D settings (keyed by stable hardware identity)
                try
                {
                    var threeDServices = _adlxSystem.Get3DSettingsServices();
                    var gpusFor3D = _adlxSystem.EnumerateADLXGPUs();
                    foreach (var gpu in gpusFor3D)
                    {
                        string gpuKey = $"{gpu.DeviceId}_{gpu.SubSystemId}_{gpu.SubSystemVendorId}_{gpu.RevisionId}";
                        if (!displayConfig.GPUs.TryGetValue(gpuKey, out var storedGpu))
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: No stored GPU settings for GPU {gpuKey}, skipping.");
                            continue;
                        }
                        if (storedGpu.ThreeDSettings.HasValue)
                        {
                            if (threeDServices.TryApplyAll3DSettings(gpu.UniqueId, storedGpu.ThreeDSettings.Value))
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Applied 3D settings for GPU {gpuKey}.");
                            }
                            else
                            {
                                SharedLogger.logger.Warn($"AMDLibrary/SetActiveConfigOverride: Failed to apply 3D settings for GPU {gpuKey}.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: No stored 3D settings for GPU {gpuKey}, skipping.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, "AMDLibrary/SetActiveConfigOverride: Failed to apply per-GPU 3D settings via ADLX.");
                }

                // Apply per-GPU multimedia settings
                try
                {
                    var mmServices = _adlxSystem.GetMultimediaServices();
                    var gpusForMm = _adlxSystem.EnumerateADLXGPUs();
                    foreach (var gpu in gpusForMm)
                    {
                        string gpuKey = $"{gpu.DeviceId}_{gpu.SubSystemId}_{gpu.SubSystemVendorId}_{gpu.RevisionId}";
                        if (!displayConfig.GPUs.TryGetValue(gpuKey, out var storedGpu))
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: No stored GPU settings for GPU {gpuKey}, skipping multimedia.");
                            continue;
                        }
                        if (storedGpu.VideoUpscale.HasValue && storedGpu.VideoUpscale.Value.IsSupported)
                        {
                            if (mmServices.TrySetVideoUpscaleEnabled(gpu.UniqueId, storedGpu.VideoUpscale.Value.IsEnabled))
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Set VideoUpscale enabled={storedGpu.VideoUpscale.Value.IsEnabled} for GPU {gpuKey}.");
                            }
                            else
                            {
                                SharedLogger.logger.Warn($"AMDLibrary/SetActiveConfigOverride: Failed to set VideoUpscale enabled for GPU {gpuKey}.");
                            }
                            if (storedGpu.VideoUpscale.Value.IsEnabled)
                            {
                                if (mmServices.TrySetVideoUpscaleSharpness(gpu.UniqueId, storedGpu.VideoUpscale.Value.Sharpness))
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Set VideoUpscale sharpness={storedGpu.VideoUpscale.Value.Sharpness} for GPU {gpuKey}.");
                                }
                                else
                                {
                                    SharedLogger.logger.Warn($"AMDLibrary/SetActiveConfigOverride: Failed to set VideoUpscale sharpness for GPU {gpuKey}.");
                                }
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: VideoUpscale not supported or not stored for GPU {gpuKey}, skipping.");
                        }
                        if (storedGpu.VideoSuperResolution.HasValue && storedGpu.VideoSuperResolution.Value.IsSupported)
                        {
                            if (mmServices.TrySetVideoSuperResolutionEnabled(gpu.UniqueId, storedGpu.VideoSuperResolution.Value.IsEnabled))
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: Set VideoSuperResolution enabled={storedGpu.VideoSuperResolution.Value.IsEnabled} for GPU {gpuKey}.");
                            }
                            else
                            {
                                SharedLogger.logger.Warn($"AMDLibrary/SetActiveConfigOverride: Failed to set VideoSuperResolution enabled for GPU {gpuKey}.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: VideoSuperResolution not supported or not stored for GPU {gpuKey}, skipping.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, "AMDLibrary/SetActiveConfigOverride: Failed to apply per-GPU multimedia settings via ADLX.");
                }
            }
            else
            {
                SharedLogger.logger.Error($"AMDLibrary/SetActiveConfigOverride: ERROR - Tried to run SetActiveConfigOverride but the AMD ADLX library isn't initialised!");
                throw new AMDLibraryException($"Tried to run SetActiveConfigOverride but the AMD ADLX library isn't initialised!");
            }

            return true;
        }



        public bool IsActiveConfig(AMD_DISPLAY_CONFIG displayConfig)
        {

            // Check whether the display config is in use now
            SharedLogger.logger.Trace($"AMDLibrary/IsActiveConfig: Checking whether the display configuration is already being used.");
            if (displayConfig.Equals(_activeDisplayConfig))
            {
                SharedLogger.logger.Trace($"AMDLibrary/IsActiveConfig: The display configuration is already being used (supplied displayConfig Equals currentWindowsDisplayConfig)");
                return true;
            }
            else
            {
                SharedLogger.logger.Trace($"AMDLibrary/IsActiveConfig: The display configuration is NOT currently in use (supplied displayConfig Equals currentWindowsDisplayConfig)");
                return false;
            }

        }

        public bool IsValidConfig(AMD_DISPLAY_CONFIG displayConfig)
        {
            // We want to check the AMD Eyefinity (SLS) config is valid
            SharedLogger.logger.Trace($"AMDLibrary/IsValidConfig: Testing whether the display configuration is valid");
            // 

            if (!_initialised || !displayConfig.IsInUse)
            {
                SharedLogger.logger.Trace($"AMDLibrary/IsValidConfig: The AMD display configuration is not in use, so it has no bearing in terms of whether it can be applied now. Returning true.");
                return true;
            }

            if (displayConfig.IsInUse && displayConfig.IsEyefinity)
            {
                // At the moment we just assume the config is true so we try to use it
                return true;
            }
            else
            {
                // Its not a Mosaic topology, so we just let it pass, as it's windows settings that matter.
                return true;
            }
        }

        public bool IsPossibleConfig(AMD_DISPLAY_CONFIG displayConfig)
        {
            // We want to check the AMD profile can be used now
            SharedLogger.logger.Trace($"AMDLibrary/IsPossibleConfig: Testing whether the AMD display configuration is possible to be used now");

            if (!_initialised || !displayConfig.IsInUse)
            {
                SharedLogger.logger.Trace($"AMDLibrary/IsPossibleConfig: The AMD display configuration is not in use, so it has no bearing in terms of whether it can be applied now. Returning true.");
                return true;
            }

            // If both display identifiers are 0 then no displays were connected via AMD and we should just return true.
            if (displayConfig.DisplayIdentifiers.Count == 0 && _allConnectedDisplayIdentifiers.Count == 0)
            {
                return true;
            }
            // but if only allconnected count is 0 then we have a problem
            else if (_allConnectedDisplayIdentifiers.Count == 0)
            {
                return false;
            }

            // Otherwise we need to actuially check through things
            // Check that we have all the displayConfig DisplayIdentifiers we need available now            
            if (displayConfig.DisplayIdentifiers.All(value => _allConnectedDisplayIdentifiers.Contains(value)))
            {
                SharedLogger.logger.Trace($"AMDLibrary/IsPossibleConfig: Success! The AMD display configuration is possible to be used now");
                return true;
            }
            else
            {
                SharedLogger.logger.Trace($"AMDLibrary/IsPossibleConfig: Uh oh! The AMDdisplay configuration is possible cannot be used now");
                return false;
            }
        }

        public List<string> GetCurrentDisplayIdentifiers(out bool failure)
        {
            SharedLogger.logger.Trace($"AMDLibrary/GetCurrentDisplayIdentifiers: Getting the current display identifiers for the displays in use now");

            List<string> displayIdentifiers = new List<string>();
            failure = false;

            if (_initialised)
            {
                // Get the desktop services
                // This is how we get and iterate through the various desktops. 
                // - A single desktop is associated with one display.
                // - A duplicate desktop is associated with two or more displays.
                // - An AMD Eyefinity desktop is associated with two or more displays.
                
                SharedLogger.logger.Trace($"AMDLibrary/GetCurrentDisplayIdentifiers: Attempting to get the ADLX desktop list");
                try
                {
                    // Get display list of the displays urrently in use
                    var displaysList =_adlxSystem.EnumerateDisplaysInUse();  
                    foreach (var display in displaysList)
                    {

                        // Get the GPU related to this display
                        var displaypGPU = display.GetGPU();

                        // Create an array of all the important display info we need to record
                        List<string> displayInfo = new List<string>();
                        displayInfo.Add("AMDADLX");
                        try
                        {
                            displayInfo.Add(displaypGPU.Name);
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetCurrentDisplayIdentifiers: Exception getting GPU Name from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(displaypGPU.UniqueId.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetCurrentDisplayIdentifiers: Exception getting GPU Unique ID from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(displaypGPU.IsExternal.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetCurrentDisplayIdentifiers: Exception getting GPU Is External from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(display.ConnectorType.ToString("G"));
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetCurrentDisplayIdentifiers: Exception getting ADLX Connection Type for the display from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(display.Name);
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetCurrentDisplayIdentifiers: Exception getting ADLX Name for the display from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(display.Type.ToString("G"));
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetCurrentDisplayIdentifiers: Exception getting ADLX Display Type for the display from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(display.ManufacturerId.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetCurrentDisplayIdentifiers: Exception getting ADLX Manufacturer for the display from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(display.UniqueId.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetCurrentDisplayIdentifiers: Exception getting ADLX Display Unique ID for the display from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        // Create a display identifier out of it
                        string displayIdentifier = System.String.Join("|", displayInfo);
                        // Add it to the list of display identifiers so we can return it
                        // but only add it if it doesn't already exist. Otherwise we get duplicates :/
                        if (!displayIdentifiers.Contains(displayIdentifier))
                        {
                            displayIdentifiers.Add(displayIdentifier);
                            SharedLogger.logger.Debug($"AMDLibrary/GetCurrentDisplayIdentifiers: DisplayIdentifier detected: {displayIdentifier}");
                        }                        

                    }
                }
                catch(Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"AMDLibrary/GetCurrentDisplayIdentifiers: Exception trying to get the ADLX desktop services");
                    failure = true;
                    return new List<string>();
                }

            }
            else
            {
                SharedLogger.logger.Error($"AMDLibrary/GetSomeDisplayIdentifiers: ERROR - Tried to get Displays but the AMD ADLX library isn't initialised!");
                throw new AMDLibraryException($"Tried to get Displays but the AMD ADLX library isn't initialised!");
            }

            // Sort the display identifiers
            displayIdentifiers.Sort();

            return displayIdentifiers;
        }

        public List<string> GetAllConnectedDisplayIdentifiers(out bool failure)
        {
            SharedLogger.logger.Trace($"AMDLibrary/GetAllConnectedDisplayIdentifiers: Getting all the display identifiers that can possibly be used");

            List<string> displayIdentifiers = new List<string>();
            failure = false;

            if (_initialised)
            {
                try
                {
                    var displaysList =_adlxSystem.EnumerateDisplays();  
                    foreach (var display in displaysList)
                    {

                        // Get the GPU related to this display
                        var displaypGPU = display.GetGPU();
                        // Create an array of all the important display info we need to record
                        List<string> displayInfo = new List<string>();
                        displayInfo.Add("AMDADLX");
                        try
                        {
                            displayInfo.Add(displaypGPU.Name);
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetAllConnectedDisplayIdentifiers: Exception getting GPU Name from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(displaypGPU.UniqueId.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetAllConnectedDisplayIdentifiers: Exception getting GPU Unique ID from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(displaypGPU.IsExternal.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetAllConnectedDisplayIdentifiers: Exception getting GPU Is External from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(display.ConnectorType.ToString("G"));
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetAllConnectedDisplayIdentifiers: Exception getting ADLX Connection Type for the display from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(display.Name);
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetAllConnectedDisplayIdentifiers: Exception getting ADLX Name for the display from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(display.Type.ToString("G"));
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetAllConnectedDisplayIdentifiers: Exception getting ADLX Display Type for the display from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(display.ManufacturerId.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetAllConnectedDisplayIdentifiers: Exception getting ADLX Manufacturer for the display from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(display.UniqueId.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetAllConnectedDisplayIdentifiers: Exception getting ADLX Display Unique ID for the display from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        // Create a display identifier out of it
                        string displayIdentifier = System.String.Join("|", displayInfo);
                        // Add it to the list of display identifiers so we can return it
                        // but only add it if it doesn't already exist. Otherwise we get duplicates :/
                        if (!displayIdentifiers.Contains(displayIdentifier))
                        {
                            displayIdentifiers.Add(displayIdentifier);
                            SharedLogger.logger.Debug($"AMDLibrary/GetAllConnectedDisplayIdentifiers: DisplayIdentifier detected: {displayIdentifier}");
                        }                        

                    }
                }
                catch(Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"AMDLibrary/GetAllConnectedDisplayIdentifiers: Exception trying to get the ADLX desktop services");
                    failure = true;
                    return new List<string>();
                }

            }
            else
            {
                SharedLogger.logger.Error($"AMDLibrary/GetSomeDisplayIdentifiers: ERROR - Tried to get Displays but the AMD ADLX library isn't initialised!");
                throw new AMDLibraryException($"Tried to get Displays but the AMD ADLX library isn't initialised!");
            }

            // Sort the display identifiers
            displayIdentifiers.Sort();

            return displayIdentifiers;
        }


    }

    [global::System.Serializable]
    public class AMDLibraryException : Exception
    {
        public AMDLibraryException() { }
        public AMDLibraryException(string message) : base(message) { }
        public AMDLibraryException(string message, Exception inner) : base(message, inner) { }
    }
}
