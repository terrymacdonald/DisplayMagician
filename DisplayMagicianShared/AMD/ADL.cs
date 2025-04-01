using System;
using System.Runtime.InteropServices;
using FARPROC = System.IntPtr;
using HMODULE = System.IntPtr;

namespace DisplayMagicianShared.AMD
{
    public delegate IntPtr ADL_Main_Memory_Alloc_Delegate(int size);

    public enum ADL_STATUS : int
    {
        // Result Codes
        /// <summary> ADL function completed successfully. </summary>                
        ADL_OK = 0,
        /// <summary> Generic Error.Most likely one or more of the Escape calls to the driver failed!</summary>
        ADL_ERR = -1,
        /// <summary> Call can't be made due to disabled adapter. </summary>
        ADL_ERR_DISABLED_ADAPTER = -10,
        /// <summary> Invalid ADL index passed. </summary>
        ADL_ERR_INVALID_ADL_IDX = -5,
        /// <summary> Invalid Callback. </summary>
        ADL_ERR_INVALID_CALLBACK = -11,
        /// <summary> Invalid controller index passed.</summary>
        ADL_ERR_INVALID_CONTROLLER_IDX = -6,
        /// <summary> Invalid display index passed.</summary>
        ADL_ERR_INVALID_DISPLAY_IDX = -7,
        /// <summary> One of the parameter passed is invalid.</summary>
        ADL_ERR_INVALID_PARAM = -3,
        /// <summary> One of the parameter size is invalid.</summary>
        ADL_ERR_INVALID_PARAM_SIZE = -4,
        /// <summary> There's no Linux XDisplay in Linux Console environment.</summary>
        ADL_ERR_NO_XDISPLAY = -21,
        /// <summary> ADL not initialized.</summary>
        ADL_ERR_NOT_INIT = -2,
        /// <summary> Function not supported by the driver. </summary>
        ADL_ERR_NOT_SUPPORTED = -8,
        /// <summary> Null Pointer error.</summary>
        ADL_ERR_NULL_POINTER = -9,
        /// <summary> Display Resource conflict.</summary>
        ADL_ERR_RESOURCE_CONFLICT = -12,
        /// <summary> Err Set incomplete</summary>
        ADL_ERR_SET_INCOMPLETE = -20,
        /// <summary> All OK but need mode change. </summary>
        ADL_OK_MODE_CHANGE = 2,
        /// <summary> All OK, but need restart.</summary>
        ADL_OK_RESTART = 3,
        /// <summary> All OK, but need to wait</summary>
        ADL_OK_WAIT = 4,
        /// <summary> All OK, but with warning.</summary>
        ADL_OK_WARNING = 1,
    }

    public enum ADL_CONNECTION_TYPE : int
    {
        VGA = 0,
        DVI = 1,
        DVI_SL = 2,
        HDMI = 4,
        DisplayPort = 4,
        ActiveDongleDPToDVI_SL = 5,
        ActiveDongleDPToDVI_DL = 6,
        ActiveDongleDPToHDMI = 7,
        ActiveDongleDPToVGA = 8,
        PassiveDongleDPToHDMI = 9,
        PassiveDongleDPToDVI = 10,
        MST = 11,
        ActiveDongle = 12,
        Virtual = 13
    }

    public enum ADL_DISPLAY_CONNECTION_TYPE : int
    {
        Unknown = 0,
        VGA = 1,
        DVI_D = 2,
        DVI_I = 3,
        HDMI = 4,
        ATICV_NTSC_Dongle = 4,
        ATICV_JPN_Dongle = 5,
        ATICV_NONI2C_NTSC_Dongle = 6,
        ATICV_NONI2C_JPN_Dongle = 7,
        Proprietary = 8,
        HDMITypeA = 10,
        HDMITypeB = 11,
        SVideo = 12,
        Composite = 13,
        RCA_3Component = 14,
        DisplayPort = 15,
        EDP = 16,
        WirelessDisplay = 17,
        USBTypeC = 18
    }

    [Flags]
    public enum ADL_DISPLAY_MODE_FLAG : int
    {
        ColourFormat565 = 1,
        ColourFormat8888 = 2,
        Degrees0 = 4,
        Degrees90 = 8,
        Degrees180 = 10,
        Degrees270 = 20,
        ExactRefreshRate = 80,
        RoundedRefreshRate = 40
    }
    public enum ADL_DISPLAY_MODE_INTERLACING : int
    {
        Progressive = 0,
        Interlaced = 2
    }

    public enum ADL_COLORDEPTH : int
    {
        ColorDepth_Unknown = 0,
        ColorDepth_666 = 1,
        ColorDepth_888 = 2,
        ColorDepth_101010 = 3,
        ColorDepth_121212 = 4,
        ColorDepth_141414 = 5,
        ColorDepth_161616 = 6,
    }


    /// <summary> ADLAdapterInfo Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_MODE : IEquatable<ADL_MODE>
    {
        /// <summary> Adapter index. </summary>
        public int AdapterIndex;
        /// <summary> Display IDs. </summary>
        public ADL_DISPLAY_ID DisplayID;
        /// <summary> Screen position X coordinate. </summary>
        public int XPos;
        /// <summary> Screen position Y coordinate. </summary>
        public int YPos;
        /// <summary> Screen resolution Width.  </summary>
        public int XRes;
        /// <summary> Screen resolution Height. </summary>
        public int YRes;
        /// <summary> Screen Color Depth. E.g., 16, 32.  </summary>
        public int ColourDepth;
        /// <summary> Screen refresh rate. </summary>
        public float RefreshRate;
        /// <summary> Screen orientation. E.g., 0, 90, 180, 270. </summary>
        public int Orientation;
        /// <summary> Vista mode flag indicating Progressive or Interlaced mode.  </summary>
        public int ModeFlag;
        /// <summary> The bit mask identifying the number of bits this Mode is currently using. </summary>
        public int ModeMask;
        /// <summary> The bit mask identifying the display status. </summary>
        public int ModeValue;

        // Mode Mask settings
        public bool ColourFormat565Supported => (ModeMask & 0x1) == 0x1;
        public bool ColourFormat8888Supported => (ModeMask & 0x2) == 0x2;
        public bool Orientation000Supported => (ModeMask & 0x4) == 0x4;
        public bool Orientation090Supported => (ModeMask & 0x8) == 0x8;
        public bool Orientation180Supported => (ModeMask & 0x10) == 0x10;
        public bool Orientation270Supported => (ModeMask & 0x20) == 0x20;
        public bool RefreshRateRoundedSupported => (ModeMask & 0x40) == 0x40;
        public bool RefreshRateOnlySupported => (ModeMask & 0x80) == 0x80;

        // Mode Value settings
        public bool ColourFormat565Set => (ModeValue & 0x1) == 0x1;
        public bool ColourFormat8888Set => (ModeValue & 0x2) == 0x2;
        public bool Orientation000Set => (ModeValue & 0x4) == 0x4;
        public bool Orientation090Set => (ModeValue & 0x8) == 0x8;
        public bool Orientation180Set => (ModeValue & 0x10) == 0x10;
        public bool Orientation270Set => (ModeValue & 0x20) == 0x20;
        public bool RefreshRateRoundedSet => (ModeValue & 0x40) == 0x40;
        public bool RefreshRateOnlySet => (ModeValue & 0x80) == 0x80;

        // Mode Flag settings
        public bool ProgressiveSet => ModeValue == 0x0;
        public bool InterlacedSet => ModeValue == 0x2;

        public override bool Equals(object obj) => obj is ADL_MODE other && this.Equals(other);
        public bool Equals(ADL_MODE other)
            => AdapterIndex == other.AdapterIndex &&
                DisplayID.Equals(other.DisplayID) &&
                XPos == other.XPos &&
                YPos == other.YPos &&
                XRes == other.XRes &&
                YRes == other.YRes &&
                ColourDepth == other.ColourDepth &&
                RefreshRate == other.RefreshRate &&
                Orientation == other.Orientation &&
                ModeFlag == other.ModeFlag &&
                ModeMask == other.ModeMask &&
                ModeValue == other.ModeValue;

        public override int GetHashCode()
        {
            return (AdapterIndex, DisplayID, XPos, YPos, XRes, YRes, ColourDepth, RefreshRate, Orientation, ModeFlag, ModeMask, ModeValue).GetHashCode();
        }

        public static bool operator ==(ADL_MODE lhs, ADL_MODE rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_MODE lhs, ADL_MODE rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLDisplayTarget </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_DISPLAY_TARGET : IEquatable<ADL_DISPLAY_TARGET>
    {
        /// <summary> Display IDs. </summary>
        public ADL_DISPLAY_ID DisplayID;
        /// <summary> The display map index identify this manner and the desktop surface. </summary>
        public int DisplayMapIndex;
        /// <summary> The bit mask identifies the number of bits DisplayTarget is currently using. </summary>
        public int DisplayTargetMask;
        /// <summary> The bit mask identifies the display status. </summary>
        public int DisplayTargetValue;

        // DisplayTarget Mask settings
        public bool DisplayTargetPreferredSupported => (DisplayTargetMask & 0x1) == 0x1;

        // DisplayTarget Value settings
        public bool DisplayTargetPreferredSet => (DisplayTargetValue & 0x1) == 0x1;

        public override bool Equals(object obj) => obj is ADL_DISPLAY_TARGET other && this.Equals(other);
        public bool Equals(ADL_DISPLAY_TARGET other)
            => DisplayID.Equals(other.DisplayID) &&
                DisplayMapIndex == other.DisplayMapIndex &&
                DisplayTargetMask == other.DisplayTargetMask &&
                DisplayTargetValue == other.DisplayTargetValue;

        public override int GetHashCode()
        {
            return (DisplayID, DisplayMapIndex, DisplayTargetMask, DisplayTargetValue).GetHashCode();
        }

        public static bool operator ==(ADL_DISPLAY_TARGET lhs, ADL_DISPLAY_TARGET rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_DISPLAY_TARGET lhs, ADL_DISPLAY_TARGET rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLAdapterDisplayCap </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_ADAPTER_DISPLAY_CAP : IEquatable<ADL_ADAPTER_DISPLAY_CAP>
    {
        /// <summary> The Persistent logical Adapter Index </summary>
        public int AdapterIndex;
        /// <summary> The bit mask identifies the number of bits AdapterDisplayCap is currently using. Sum all the bits defined in ADL_ADAPTER_DISPLAYCAP_XXX </summary>
        public int AdapterDisplayCapMask;
        /// <summary> The bit mask identifies the status. Refer to ADL_ADAPTER_DISPLAYCAP_XXX </summary>
        public int AdapterDisplayCapValue;

        // AdapterDisplayCap Mask settings
        public bool NotActiveSupported => (AdapterDisplayCapMask & 0x1) == 0x1;
        public bool SingleSupported => (AdapterDisplayCapMask & 0x1) == 0x2;
        public bool CloneSupported => (AdapterDisplayCapMask & 0x1) == 0x4;
        public bool NStretch1GPUSupported => (AdapterDisplayCapMask & 0x1) == 0x8;
        public bool NStretchNGPUSupported => (AdapterDisplayCapMask & 0x1) == 0x10;
        public bool TwoVStretchSupported => (AdapterDisplayCapMask & 0x1) == 0x20;
        public bool TwoHStretchSupported => (AdapterDisplayCapMask & 0x1) == 0x40;
        public bool ExtendedSupported => (AdapterDisplayCapMask & 0x1) == 0x80;
        public bool PreferDisplaySupported => (AdapterDisplayCapMask & 0x1) == 0x100;
        public bool BezelSupported => (AdapterDisplayCapMask & 0x1) == 0x200;


        // AdapterDisplayCap Value settings
        public bool NotActiveSet => (AdapterDisplayCapValue & 0x1) == 0x1;
        public bool SingleSet => (AdapterDisplayCapValue & 0x1) == 0x2;
        public bool CloneSet => (AdapterDisplayCapValue & 0x1) == 0x4;
        public bool NStretch1GPUSet => (AdapterDisplayCapValue & 0x1) == 0x8;
        public bool NStretchNGPUSet => (AdapterDisplayCapValue & 0x1) == 0x10;
        public bool TwoVStretchSet => (AdapterDisplayCapValue & 0x1) == 0x20;
        public bool TwoHStretchSet => (AdapterDisplayCapValue & 0x1) == 0x40;
        public bool ExtendedSet => (AdapterDisplayCapValue & 0x1) == 0x80;
        public bool PreferDisplaySet => (AdapterDisplayCapValue & 0x1) == 0x100;
        public bool BezelSet => (AdapterDisplayCapValue & 0x1) == 0x200;

        /*        #define ADL_ADAPTER_DISPLAYCAP_MANNER_SUPPORTED_NOTACTIVE        0x00000001
                #define ADL_ADAPTER_DISPLAYCAP_MANNER_SUPPORTED_SINGLE            0x00000002
                #define ADL_ADAPTER_DISPLAYCAP_MANNER_SUPPORTED_CLONE            0x00000004
                #define ADL_ADAPTER_DISPLAYCAP_MANNER_SUPPORTED_NSTRETCH1GPU    0x00000008
                #define ADL_ADAPTER_DISPLAYCAP_MANNER_SUPPORTED_NSTRETCHNGPU    0x00000010

                        /// Legacy support for XP
                #define ADL_ADAPTER_DISPLAYCAP_MANNER_SUPPORTED_2VSTRETCH        0x00000020
                #define ADL_ADAPTER_DISPLAYCAP_MANNER_SUPPORTED_2HSTRETCH        0x00000040
                #define ADL_ADAPTER_DISPLAYCAP_MANNER_SUPPORTED_EXTENDED        0x00000080

                #define ADL_ADAPTER_DISPLAYCAP_PREFERDISPLAY_SUPPORTED            0x00000100
                #define ADL_ADAPTER_DISPLAYCAP_BEZEL_SUPPORTED                    0x00000200*/

        public override bool Equals(object obj) => obj is ADL_ADAPTER_DISPLAY_CAP other && this.Equals(other);
        public bool Equals(ADL_ADAPTER_DISPLAY_CAP other)
            => AdapterIndex == other.AdapterIndex &&
                AdapterDisplayCapMask == other.AdapterDisplayCapMask &&
                AdapterDisplayCapValue == other.AdapterDisplayCapValue;

        public override int GetHashCode()
        {
            return (AdapterIndex, AdapterDisplayCapMask, AdapterDisplayCapValue).GetHashCode();
        }
        public static bool operator ==(ADL_ADAPTER_DISPLAY_CAP lhs, ADL_ADAPTER_DISPLAY_CAP rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_ADAPTER_DISPLAY_CAP lhs, ADL_ADAPTER_DISPLAY_CAP rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLAdapterInfo Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_ADAPTER_INFO : IEquatable<ADL_ADAPTER_INFO>
    {
        /// <summary>The size of the structure</summary>
        public int Size;
        /// <summary> Adapter Index</summary>
        public int AdapterIndex;
        /// <summary> Adapter UDID</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_PATH)]
        public string UDID;
        /// <summary> Adapter Bus Number</summary>
        public int BusNumber;
        /// <summary> Adapter Driver Number</summary>
        public int DriverNumber;
        /// <summary> Adapter Function Number</summary>
        public int FunctionNumber;
        /// <summary> Adapter Vendor ID</summary>
        public int VendorID;
        /// <summary> Adapter Adapter name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_PATH)]
        public string AdapterName;
        /// <summary> Adapter Display name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_PATH)]
        public string DisplayName;
        /// <summary> Adapter Present status</summary>
        public int Present;
        /// <summary> Adapter Exist status</summary>
        public int Exist;
        /// <summary> Adapter Driver Path</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_PATH)]
        public string DriverPath;
        /// <summary> Adapter Driver Ext Path</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_PATH)]
        public string DriverPathExt;
        /// <summary> Adapter PNP String</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_PATH)]
        public string PNPString;
        /// <summary> OS Display Index</summary>
        public int OSDisplayIndex;

        public override bool Equals(object obj) => obj is ADL_ADAPTER_INFO other && this.Equals(other);
        public bool Equals(ADL_ADAPTER_INFO other)
            => Size == other.Size &&
                AdapterIndex == other.AdapterIndex &&
                UDID.Equals(other.UDID) &&
                BusNumber == other.BusNumber &&
                DriverNumber == other.DriverNumber &&
                FunctionNumber == other.FunctionNumber &&
                VendorID == other.VendorID &&
                AdapterName.Equals(other.AdapterName) &&
                DisplayName.Equals(other.DisplayName) &&
                Present == other.Present &&
                Exist == other.Exist &&
                DriverPath.Equals(other.DriverPath) &&
                DriverPathExt.Equals(other.DriverPathExt) &&
                PNPString.Equals(other.PNPString) &&
                OSDisplayIndex == other.OSDisplayIndex;

        public override int GetHashCode()
        {
            return (Size, AdapterIndex, UDID, BusNumber, DriverNumber, FunctionNumber, VendorID, AdapterName, DisplayName, Present, Exist, DriverPath, DriverPathExt, PNPString, OSDisplayIndex).GetHashCode();
        }

        public static bool operator ==(ADL_ADAPTER_INFO lhs, ADL_ADAPTER_INFO rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_ADAPTER_INFO lhs, ADL_ADAPTER_INFO rhs) => !(lhs == rhs);
        //public override string ToString() => $"{type.ToString("G")}";
    }


    /// <summary> ADLAdapterInfoX2 Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_ADAPTER_INFOX2 : IEquatable<ADL_ADAPTER_INFOX2>
    {
        /// <summary>The size of the structure</summary>
        public int Size;
        /// <summary> Adapter Index</summary>
        public int AdapterIndex;
        /// <summary> Adapter UDID</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_PATH)]
        public string UDID;
        /// <summary> Adapter Bus Number</summary>
        public int BusNumber;
        /// <summary> Adapter Device Number</summary>
        public int DeviceNumber;
        /// <summary> Adapter Function Number</summary>
        public int FunctionNumber;
        /// <summary> Adapter Vendor ID</summary>
        public int VendorID;
        /// <summary> Adapter Adapter name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_DISPLAY_NAME)]
        public string AdapterName;
        /// <summary> Adapter Display name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_DISPLAY_NAME)]
        public string DisplayName;
        /// <summary> Adapter Present status</summary>
        public int Present;
        /// <summary> Adapter Exist status</summary>
        public int Exist;
        /// <summary> Adapter Driver Path</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_PATH)]
        public string DriverPath;
        /// <summary> Adapter Driver Ext Path</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_PATH)]
        public string DriverPathExt;
        /// <summary> Adapter PNP String</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_PATH)]
        public string PNPString;
        /// <summary> OS Display Index</summary>
        public int OSDisplayIndex;
        /// <summary> Display Info Mask</summary>
        public int InfoMask;
        /// <summary> Display Info Value</summary>
        public int InfoValue;

        // Info Mask settings
        public bool DisplayConnectedSupported => (InfoMask & 0x1) == 0x1;
        public bool DisplayMappedSupported => (InfoMask & 0x2) == 0x2;
        public bool NonLocalSupported => (InfoMask & 0x4) == 0x4;
        public bool ForcibleSupported => (InfoMask & 0x8) == 0x8;
        public bool GenLockSupported => (InfoMask & 0x10) == 0x10;
        public bool MultiVPUSupported => (InfoMask & 0x20) == 0x20;
        public bool LDADisplaySupported => (InfoMask & 0x40) == 0x40;
        public bool ModeTimingOverrideSupported => (InfoMask & 0x80) == 0x80;
        public bool MannerSingleSupported => (InfoMask & 0x100) == 0x100;
        public bool MannerCloneSupported => (InfoMask & 0x200) == 0x200;
        public bool Manner2VStretchSupported => (InfoMask & 0x400) == 0x400;
        public bool Manner2HStretchSupported => (InfoMask & 0x800) == 0x800;
        public bool MannerExtendedSupported => (InfoMask & 0x1000) == 0x1000;
        public bool MannerNStretch1GPUSupported => (InfoMask & 0x10000) == 0x10000;
        public bool MannerNStretchNGPUSupported => (InfoMask & 0x20000) == 0x20000;
        public bool MannerReserved2Supported => (InfoMask & 0x40000) == 0x40000;
        public bool MannerReserved3Supported => (InfoMask & 0x80000) == 0x80000;
        public bool ShowTypeProjectorSupported => (InfoMask & 0x100000) == 0x100000;

        // Info Value settings
        public bool DisplayConnectedSet => (InfoValue & 0x1) == 0x1;
        public bool DisplayMappedSet => (InfoValue & 0x2) == 0x2;
        public bool NonLocalSet => (InfoValue & 0x4) == 0x4;
        public bool ForcibleSet => (InfoValue & 0x8) == 0x8;
        public bool GenLockSet => (InfoValue & 0x10) == 0x10;
        public bool MultiVPUSet => (InfoValue & 0x20) == 0x20;
        public bool LDADisplaySet => (InfoValue & 0x40) == 0x40;
        public bool ModeTimingOverrideSet => (InfoValue & 0x80) == 0x80;
        public bool MannerSingleSet => (InfoValue & 0x100) == 0x100;
        public bool MannerCloneSet => (InfoValue & 0x200) == 0x200;
        public bool Manner2VStretchSet => (InfoValue & 0x400) == 0x400;
        public bool Manner2HStretchSet => (InfoValue & 0x800) == 0x800;
        public bool MannerExtendedSet => (InfoValue & 0x1000) == 0x1000;
        public bool MannerNStretch1GPUSet => (InfoValue & 0x10000) == 0x10000;
        public bool MannerNStretchNGPUSet => (InfoValue & 0x20000) == 0x20000;
        public bool MannerReserved2Set => (InfoValue & 0x40000) == 0x40000;
        public bool MannerReserved3Set => (InfoValue & 0x80000) == 0x80000;
        public bool ShowTypeProjectorSet => (InfoValue & 0x100000) == 0x100000;

        public override bool Equals(object obj) => obj is ADL_ADAPTER_INFOX2 other && this.Equals(other);
        public bool Equals(ADL_ADAPTER_INFOX2 other)
            => Size == other.Size &&
                AdapterIndex == other.AdapterIndex &&
                UDID.Equals(other.UDID) &&
                BusNumber == other.BusNumber &&
                DeviceNumber == other.DeviceNumber &&
                FunctionNumber == other.FunctionNumber &&
                VendorID == other.VendorID &&
                AdapterName.Equals(other.AdapterName) &&
                DisplayName.Equals(other.DisplayName) &&
                Present == other.Present &&
                Exist == other.Exist &&
                DriverPath.Equals(other.DriverPath) &&
                DriverPathExt.Equals(other.DriverPathExt) &&
                PNPString.Equals(other.PNPString) &&
                OSDisplayIndex == other.OSDisplayIndex &&
                InfoMask == other.InfoMask &&
                InfoValue == other.InfoValue;

        public override int GetHashCode()
        {
            return (Size, AdapterIndex, UDID, BusNumber, DeviceNumber, FunctionNumber, VendorID, AdapterName, DisplayName, Present, Exist, DriverPath, DriverPathExt, PNPString, OSDisplayIndex).GetHashCode();
        }

        public static bool operator ==(ADL_ADAPTER_INFOX2 lhs, ADL_ADAPTER_INFOX2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_ADAPTER_INFOX2 lhs, ADL_ADAPTER_INFOX2 rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLDisplayEDIDData Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_DISPLAY_EDID_DATA : IEquatable<ADL_DISPLAY_EDID_DATA>
    {
        /// <summary> Size</summary>
        public int Size;
        /// <summary> Flag</summary>
        public int Flag;
        /// <summary> EDIDSize </summary>
        public int EDIDSize;
        /// <summary> Block Index </summary>
        public int BlockIndex;
        /// <summary> EDIDData [256] </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_EDIDDATA_SIZE)]
        public string EDIDData;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] Reserved;

        public override bool Equals(object obj) => obj is ADL_DISPLAY_EDID_DATA other && this.Equals(other);
        public bool Equals(ADL_DISPLAY_EDID_DATA other)
            => Size == other.Size &&
                Flag == other.Flag &&
                EDIDSize == other.EDIDSize &&
                BlockIndex == other.BlockIndex &&
                EDIDData.Equals(other.EDIDData);

        public override int GetHashCode()
        {
            return (Size, Flag, EDIDSize, BlockIndex, EDIDData).GetHashCode();
        }

        public static bool operator ==(ADL_DISPLAY_EDID_DATA lhs, ADL_DISPLAY_EDID_DATA rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_DISPLAY_EDID_DATA lhs, ADL_DISPLAY_EDID_DATA rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";

    }


    /// <summary> ADLDDCInfo2 Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_DDC_INFO2 : IEquatable<ADL_DDC_INFO2>
    {
        /// <summary> Size of the structure. </summary>
        public int Size;
        /// <summary> Whether this display device support DDC</summary>
        public int SupportsDDC;
        /// <summary> Returns the manufacturer ID of the display device. Should be zeroed if this information is not available.</summary>
        public int ManufacturerID;
        /// <summary> Returns the product ID of the display device. Should be zeroed if this informatiadlon is not available.</summary>
        public int ProductID;
        /// <summary> Returns the name of the display device. Should be zeroed if this information is not available.</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_DISPLAY_NAME)]
        public string DisplayName;
        /// <summary> Returns the maximum Horizontal supported resolution. Should be zeroed if this information is not available.</summary>
        public int MaxHResolution;
        /// <summary> Returns the maximum Vertical supported resolution. Should be zeroed if this information is not available. </summary>
        public int MaxVResolution;
        /// <summary> Returns the maximum supported refresh rate. Should be zeroed if this information is not available. </summary>
        public int MaxRefresh;
        /// <summary> Returns the display device preferred timing mode's horizontal resolution.</summary>
        public int PTMCx;
        /// <summary> Returns the display device preferred timing mode's vertical resolution. </summary>
        public int PTMCy;
        /// <summary> Returns the display device preferred timing mode's refresh rate.</summary>
        public int PTMRefreshRate;
        /// <summary> Return EDID flags.</summary>
        public int DDCInfoFlag;
        /// <summary> Returns 1 if the display supported packed pixel, 0 otherwise. </summary>
        public int PackedPixelSupported;
        /// <summary> Returns the Pixel formats the display supports DDCInfo Pixel Formats.</summary>
        public int PanelPixelFormat;
        /// <summary> Return EDID serial ID.</summary>
        public int SerialID;
        /// <summary> Return minimum monitor luminance data.</summary>
        public int MinLuminanceData;
        /// <summary> Return average monitor luminance data. </summary>
        public int AvgLuminanceData;
        /// <summary> Return maximum monitor luminance data.</summary>
        public int MaxLuminanceData;
        /// <summary> Bit vector of supported transfer functions ADLSourceContentAttributes transfer functions (gamma). </summary>
        public int SupportedTransferFunction;
        /// <summary> Bit vector of supported color spaces ADLSourceContentAttributes color spaces.</summary>
        public int SupportedColorSpace;
        /// <summary> Display Red Chromaticity X coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityRedX;
        /// <summary> Display Red Chromaticity Y coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityRedY;
        /// <summary> Display Green Chromaticity X coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityGreenX;
        /// <summary> Display Green  Chromaticity Y coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityGreenY;
        /// <summary> Display Blue Chromaticity X coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityBlueX;
        /// <summary> Display Blue Chromaticity Y coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityBlueY;
        /// <summary> Display White Chromaticity X coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityWhiteX;
        /// <summary> Display White Chromaticity Y coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityWhiteY;
        /// <summary> Display diffuse screen reflectance 0-1 (100%) in units of 0.01.</summary>
        public int DiffuseScreenReflectance;
        /// <summary> Display specular screen reflectance 0-1 (100%) in units of 0.01.</summary>
        public int SpecularScreenReflectance;
        /// <summary> Bit vector of supported color spaces ADLDDCInfo2 HDR support options.</summary>
        public int SupportedHDR;
        /// <summary> Bit vector for freesync flags.</summary>
        public int FreesyncFlags;
        /// <summary> Return minimum monitor luminance without dimming data.</summary>
        public int MinLuminanceNoDimmingData;
        /// <summary> Returns the maximum backlight maximum luminance. Should be zeroed if this information is not available.</summary>
        public int MaxBacklightMaxLuminanceData;
        /// <summAry> Returns the minimum backlight maximum luminance. Should be zeroed if this information is not available.</summary>
        public int MinBacklightMaxLuminanceData;
        /// <summary> Returns the maximum backlight minimum luminance. Should be zeroed if this information is not available.</summary>
        public int MaxBacklightMinLuminanceData;
        /// <summary> Returns the minimum backlight maximum luminance. Should be zeroed if this information is not available.</summary>
        public int MinBacklightMinLuminanceData;
        /// <summary> Reserved </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] Reserved;

        // DDC Info Flag settings
        public bool IsProjectorDevice => (DDCInfoFlag & 0x1) == 0x1;
        public bool IsEDIDExtension => (DDCInfoFlag & 0x2) == 0x2;
        public bool IsDigitalDevice => (DDCInfoFlag & 0x4) == 0x4;
        public bool IsHDMIAudioDevice => (DDCInfoFlag & 0x8) == 0x8;
        public bool SupportsAI => (DDCInfoFlag & 0x10) == 0x10;
        public bool SupportsxvYCC601 => (DDCInfoFlag & 0x10) == 0x20;
        public bool SupportsxvYCC709 => (DDCInfoFlag & 0x10) == 0x40;

        /*#define ADL_DISPLAYDDCINFOEX_FLAG_PROJECTORDEVICE       (1 << 0)
        #define ADL_DISPLAYDDCINFOEX_FLAG_EDIDEXTENSION         (1 << 1)
        #define ADL_DISPLAYDDCINFOEX_FLAG_DIGITALDEVICE         (1 << 2)
        #define ADL_DISPLAYDDCINFOEX_FLAG_HDMIAUDIODEVICE       (1 << 3)
        #define ADL_DISPLAYDDCINFOEX_FLAG_SUPPORTS_AI           (1 << 4)
        #define ADL_DISPLAYDDCINFOEX_FLAG_SUPPORT_xvYCC601      (1 << 5)
        #define ADL_DISPLAYDDCINFOEX_FLAG_SUPPORT_xvYCC709      (1 << 6)*/

        // Panel Pixel Format settings
        public bool PixelFormatUnknown => (PanelPixelFormat & 0x1) == 0x1;
        public bool PixelFormatRGB => (PanelPixelFormat & 0x2) == 0x2;
        public bool PixelFormatYCRCB444 => (PanelPixelFormat & 0x4) == 0x4;
        public bool PixelFormatYCRCB422 => (PanelPixelFormat & 0x8) == 0x8;
        public bool PixelFormatLimitedRange => (PanelPixelFormat & 0x10) == 0x10;
        public bool PixelFormatYCRCB420 => (PanelPixelFormat & 0x10) == 0x20;

        // ADL_DISPLAY_ADJUSTMENT_PIXELFORMAT adjustment values
        // (bit-vector)
        /*#define ADL_DISPLAY_PIXELFORMAT_UNKNOWN             0
        #define ADL_DISPLAY_PIXELFORMAT_RGB                       (1 << 0)
        #define ADL_DISPLAY_PIXELFORMAT_YCRCB444                  (1 << 1)    //Limited range
        #define ADL_DISPLAY_PIXELFORMAT_YCRCB422                 (1 << 2)    //Limited range
        #define ADL_DISPLAY_PIXELFORMAT_RGB_LIMITED_RANGE      (1 << 3)
        #define ADL_DISPLAY_PIXELFORMAT_RGB_FULL_RANGE    ADL_DISPLAY_PIXELFORMAT_RGB  //Full range
        #define ADL_DISPLAY_PIXELFORMAT_YCRCB420              (1 << 4)*/

        // Supported Transfer Function settings
        //public bool DisplayConnectedSupported => (SupportedTransferFunction & 0x1) == 0x1;

        // Supported Color Space settings
        //public bool DisplayConnectedSupported => (SupportedColorSpace & 0x1) == 0x1;

        // Supported HDR settings
        public bool CEA861_3Supported => (SupportedHDR & 0x1) == 0x1;
        public bool DolbyVisionSupported => (SupportedHDR & 0x2) == 0x2;
        public bool FreeSyncHDRSupported => (SupportedHDR & 0x4) == 0x4;

        // Freesync Flags settings
        public bool FreeSyncHDRBacklightSupported => (SupportedHDR & 0x1) == 0x1;
        public bool FreeSyncHDRLocalDimmingSupported => (SupportedHDR & 0x2) == 0x2;

        public override bool Equals(object obj) => obj is ADL_DDC_INFO2 other && this.Equals(other);
        public bool Equals(ADL_DDC_INFO2 other)
            => Size == other.Size &&
            SupportsDDC == other.SupportsDDC &&
            ManufacturerID == other.ManufacturerID &&
            ProductID == other.ProductID &&
            DisplayName.Equals(other.DisplayName) &&
            MaxHResolution == other.MaxHResolution &&
            MaxVResolution == other.MaxVResolution &&
            MaxRefresh == other.MaxRefresh &&
            PTMCx == other.PTMCx &&
            PTMCy == other.PTMCy &&
            PTMRefreshRate == other.PTMRefreshRate &&
            DDCInfoFlag == other.DDCInfoFlag &&
            PackedPixelSupported == other.PackedPixelSupported &&
            PanelPixelFormat == other.PanelPixelFormat &&
            SerialID == other.SerialID &&
            MinLuminanceData == other.MinLuminanceData &&
            AvgLuminanceData == other.AvgLuminanceData &&
            MaxLuminanceData == other.MaxLuminanceData &&
            SupportedTransferFunction == other.SupportedTransferFunction &&
            SupportedColorSpace == other.SupportedColorSpace &&
            NativeDisplayChromaticityRedX == other.NativeDisplayChromaticityRedX &&
            NativeDisplayChromaticityRedY == other.NativeDisplayChromaticityRedY &&
            NativeDisplayChromaticityGreenX == other.NativeDisplayChromaticityGreenX &&
            NativeDisplayChromaticityGreenY == other.NativeDisplayChromaticityGreenY &&
            NativeDisplayChromaticityBlueX == other.NativeDisplayChromaticityBlueX &&
            NativeDisplayChromaticityBlueY == other.NativeDisplayChromaticityBlueY &&
            NativeDisplayChromaticityWhiteX == other.NativeDisplayChromaticityWhiteX &&
            NativeDisplayChromaticityWhiteY == other.NativeDisplayChromaticityWhiteY &&
            DiffuseScreenReflectance == other.DiffuseScreenReflectance &&
            SpecularScreenReflectance == other.SpecularScreenReflectance &&
            SupportedHDR == other.SupportedHDR &&
            FreesyncFlags == other.FreesyncFlags &&
            MinLuminanceNoDimmingData == other.MinLuminanceNoDimmingData &&
            MaxBacklightMaxLuminanceData == other.MaxBacklightMaxLuminanceData &&
            MinBacklightMaxLuminanceData == other.MinBacklightMaxLuminanceData &&
            MaxBacklightMinLuminanceData == other.MaxBacklightMinLuminanceData &&
            MinBacklightMinLuminanceData == other.MinBacklightMinLuminanceData;

        public override int GetHashCode()
        {
            return (Size, SupportsDDC, ManufacturerID, ProductID, DisplayName, MaxHResolution, MaxVResolution, MaxRefresh, PTMCx, PTMCy, PTMRefreshRate, DDCInfoFlag,
                PackedPixelSupported, PanelPixelFormat, SerialID, MinLuminanceData, AvgLuminanceData, MaxLuminanceData, SupportedTransferFunction, SupportedColorSpace,
                NativeDisplayChromaticityRedX, NativeDisplayChromaticityRedY, NativeDisplayChromaticityGreenX, NativeDisplayChromaticityGreenY,
                NativeDisplayChromaticityBlueX, NativeDisplayChromaticityBlueY, NativeDisplayChromaticityWhiteX, NativeDisplayChromaticityWhiteY,
                DiffuseScreenReflectance, SpecularScreenReflectance, SupportedHDR, FreesyncFlags, MinLuminanceNoDimmingData, MaxBacklightMaxLuminanceData,
                MinBacklightMaxLuminanceData, MaxBacklightMinLuminanceData, MinBacklightMinLuminanceData).GetHashCode();
        }

        public static bool operator ==(ADL_DDC_INFO2 lhs, ADL_DDC_INFO2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_DDC_INFO2 lhs, ADL_DDC_INFO2 rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLDisplayID Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_DISPLAY_ID : IEquatable<ADL_DISPLAY_ID>
    {
        /// <summary> Display Logical Index </summary>
        public int DisplayLogicalIndex;
        /// <summary> Display Physical Index </summary>
        public int DisplayPhysicalIndex;
        /// <summary> Adapter Logical Index </summary>
        public int DisplayLogicalAdapterIndex;
        /// <summary> Adapter Physical Index </summary>
        public int DisplayPhysicalAdapterIndex;

        public override bool Equals(object obj) => obj is ADL_DISPLAY_ID other && this.Equals(other);
        public bool Equals(ADL_DISPLAY_ID other)
            => DisplayLogicalIndex == other.DisplayLogicalIndex &&
                DisplayPhysicalIndex == other.DisplayPhysicalIndex &&
                DisplayLogicalAdapterIndex == other.DisplayLogicalAdapterIndex &&
                DisplayPhysicalAdapterIndex == other.DisplayPhysicalAdapterIndex;

        public override int GetHashCode()
        {
            return (DisplayLogicalIndex, DisplayPhysicalIndex, DisplayLogicalAdapterIndex, DisplayPhysicalAdapterIndex).GetHashCode();
        }

        public static bool operator ==(ADL_DISPLAY_ID lhs, ADL_DISPLAY_ID rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_DISPLAY_ID lhs, ADL_DISPLAY_ID rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLDisplayInfo Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_DISPLAY_INFO : IEquatable<ADL_DISPLAY_INFO>
    {
        /// <summary> Display Index </summary>
        public ADL_DISPLAY_ID DisplayID;
        /// <summary> Display Controller Index </summary>
        public int DisplayControllerIndex;
        /// <summary> Display Name </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_PATH)]
        public string DisplayName;
        /// <summary> Display Manufacturer Name </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADLImport.ADL_MAX_PATH)]
        public string DisplayManufacturerName;
        /// <summary> Display Type : The Display type. CRT, TV,CV,DFP are some of display types,</summary>
        public int DisplayType;
        /// <summary> Display output type </summary>
        public ADL_CONNECTION_TYPE DisplayOutputType;
        /// <summary> Connector type</summary        
        public ADL_DISPLAY_CONNECTION_TYPE DisplayConnector;
        ///<summary> Indicating the display info bits' mask.<summary>
        public int DisplayInfoMask;
        ///<summary> Indicating the display info value.<summary>
        public int DisplayInfoValue;

        // Display Type - no idea what the settings are

        // Display Output Type settings       
        public bool DisplayConnectorIsUnknown => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.Unknown;
        public bool DisplayConnectorIsVGA => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.VGA;
        public bool DisplayConnectorIsDVI_D => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.DVI_D;
        public bool DisplayConnectorIsDVI_I => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.DVI_I;
        public bool DisplayConnectorIsATICVDongleNTSC => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.ATICV_NTSC_Dongle;
        public bool DisplayConnectorIsATICVDongleJPN => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.ATICV_JPN_Dongle;
        public bool DisplayConnectorIsATICVDongleNonI2CJPN => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.ATICV_NONI2C_JPN_Dongle;
        public bool DisplayConnectorIsATICVDongleNonI2CNTSC => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.ATICV_NONI2C_NTSC_Dongle;
        public bool DisplayConnectorIsProprietary => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.Proprietary;
        public bool DisplayConnectorIsHDMITypeA => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.HDMITypeA;
        public bool DisplayConnectorIsHDMITypeB => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.HDMITypeB;
        public bool DisplayConnectorIsSVideo => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.SVideo;
        public bool DisplayConnectorIsComposite => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.Composite;
        public bool DisplayConnectorIsRCA3Component => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.RCA_3Component;
        public bool DisplayConnectorIsDisplayPort => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.DisplayPort;
        public bool DisplayConnectorIsEDP => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.EDP;
        public bool DisplayConnectorIsWirelessDisplay => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.WirelessDisplay;
        public bool DisplayConnectorIsUSBTypeC => DisplayConnector == ADL_DISPLAY_CONNECTION_TYPE.USBTypeC;


        /*#define ADL_DISPLAY_CONTYPE_UNKNOWN                 0
        #define ADL_DISPLAY_CONTYPE_VGA                     1
        #define ADL_DISPLAY_CONTYPE_DVI_D                   2
        #define ADL_DISPLAY_CONTYPE_DVI_I                   3
        #define ADL_DISPLAY_CONTYPE_ATICVDONGLE_NTSC        4
        #define ADL_DISPLAY_CONTYPE_ATICVDONGLE_JPN         5
        #define ADL_DISPLAY_CONTYPE_ATICVDONGLE_NONI2C_JPN  6
        #define ADL_DISPLAY_CONTYPE_ATICVDONGLE_NONI2C_NTSC 7
        #define ADL_DISPLAY_CONTYPE_PROPRIETARY                8
        #define ADL_DISPLAY_CONTYPE_HDMI_TYPE_A             10
        #define ADL_DISPLAY_CONTYPE_HDMI_TYPE_B             11
        #define ADL_DISPLAY_CONTYPE_SVIDEO                   12
        #define ADL_DISPLAY_CONTYPE_COMPOSITE               13
        #define ADL_DISPLAY_CONTYPE_RCA_3COMPONENT          14
        #define ADL_DISPLAY_CONTYPE_DISPLAYPORT             15
        #define ADL_DISPLAY_CONTYPE_EDP                     16
        #define ADL_DISPLAY_CONTYPE_WIRELESSDISPLAY         17
        #define ADL_DISPLAY_CONTYPE_USB_TYPE_C              18*/

        // Display Connector

        public bool DisplayOutputTypeIsVGA => DisplayOutputType == ADL_CONNECTION_TYPE.VGA;
        public bool DisplayOutputTypeIsDVI => DisplayOutputType == ADL_CONNECTION_TYPE.DVI;
        public bool DisplayOutputTypeIsDVI_SL => DisplayOutputType == ADL_CONNECTION_TYPE.DVI_SL;
        public bool DisplayOutputTypeIsHDMI => DisplayOutputType == ADL_CONNECTION_TYPE.HDMI;
        public bool DisplayOutputTypeIsDisplayPort => DisplayOutputType == ADL_CONNECTION_TYPE.DisplayPort;
        public bool DisplayOutputTypeIsActiveDongleDP_DVI_SL => DisplayOutputType == ADL_CONNECTION_TYPE.ActiveDongleDPToDVI_SL;
        public bool DisplayOutputTypeIsActiveDongleDP_DVI_DL => DisplayOutputType == ADL_CONNECTION_TYPE.ActiveDongleDPToDVI_DL;
        public bool DisplayOutputTypeIsActiveDongleDP_HDMI => DisplayOutputType == ADL_CONNECTION_TYPE.ActiveDongleDPToHDMI;
        public bool DisplayOutputTypeIsActiveDongleDP_VGA => DisplayOutputType == ADL_CONNECTION_TYPE.ActiveDongleDPToVGA;
        public bool DisplayOutputTypeIsPassiveDongleDP_HDMI => DisplayOutputType == ADL_CONNECTION_TYPE.PassiveDongleDPToHDMI;
        public bool DisplayOutputTypeIsPassiveDongleDP_DVI => DisplayOutputType == ADL_CONNECTION_TYPE.PassiveDongleDPToDVI;
        public bool DisplayOutputTypeIsMST => DisplayOutputType == ADL_CONNECTION_TYPE.MST;
        public bool DisplayOutputTypeIsActiveDongle => DisplayOutputType == ADL_CONNECTION_TYPE.ActiveDongle;
        public bool DisplayOutputTypeIsVirtual => DisplayOutputType == ADL_CONNECTION_TYPE.Virtual;

        /*#define ADL_CONNECTION_TYPE_VGA 0
        #define ADL_CONNECTION_TYPE_DVI 1
        #define ADL_CONNECTION_TYPE_DVI_SL 2
        #define ADL_CONNECTION_TYPE_HDMI 3
        #define ADL_CONNECTION_TYPE_DISPLAY_PORT 4
        #define ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_DVI_SL 5
        #define ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_DVI_DL 6
        #define ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_HDMI 7
        #define ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_VGA 8
        #define ADL_CONNECTION_TYPE_PASSIVE_DONGLE_DP_HDMI 9
        #define ADL_CONNECTION_TYPE_PASSIVE_DONGLE_DP_DVI 10
        #define ADL_CONNECTION_TYPE_MST 11
        #define ADL_CONNECTION_TYPE_ACTIVE_DONGLE          12
        #define ADL_CONNECTION_TYPE_VIRTUAL    13*/


        // Display Info Mask settings
        public bool DisplayConnectedSupported => (DisplayInfoMask & 0x1) == 0x1;
        public bool DisplayMappedSupported => (DisplayInfoMask & 0x2) == 0x2;
        public bool NonLocalSupported => (DisplayInfoMask & 0x4) == 0x4;
        public bool ForcibleSupported => (DisplayInfoMask & 0x8) == 0x8;
        public bool GenLockSupported => (DisplayInfoMask & 0x10) == 0x10;
        public bool MultiVPUSupported => (DisplayInfoMask & 0x20) == 0x20;
        public bool LDADisplaySupported => (DisplayInfoMask & 0x40) == 0x40;
        public bool ModeTimingOverrideSupported => (DisplayInfoMask & 0x80) == 0x80;
        public bool MannerSingleSupported => (DisplayInfoMask & 0x100) == 0x100;
        public bool MannerCloneSupported => (DisplayInfoMask & 0x200) == 0x200;
        public bool Manner2VStretchSupported => (DisplayInfoMask & 0x400) == 0x400;
        public bool Manner2HStretchSupported => (DisplayInfoMask & 0x800) == 0x800;
        public bool MannerExtendedSupported => (DisplayInfoMask & 0x1000) == 0x1000;
        public bool MannerNStretch1GPUSupported => (DisplayInfoMask & 0x10000) == 0x10000;
        public bool MannerNStretchNGPUSupported => (DisplayInfoMask & 0x20000) == 0x20000;
        public bool MannerReserved2Supported => (DisplayInfoMask & 0x40000) == 0x40000;
        public bool MannerReserved3Supported => (DisplayInfoMask & 0x80000) == 0x80000;
        public bool ShowTypeProjectorSupported => (DisplayInfoMask & 0x100000) == 0x100000;

        // Display Info Value settings
        public bool DisplayConnectedSet => (DisplayInfoValue & 0x1) == 0x1;
        public bool DisplayMappedSet => (DisplayInfoValue & 0x2) == 0x2;
        public bool NonLocalSet => (DisplayInfoValue & 0x4) == 0x4;
        public bool ForcibleSet => (DisplayInfoValue & 0x8) == 0x8;
        public bool GenLockSet => (DisplayInfoValue & 0x10) == 0x10;
        public bool MultiVPUSet => (DisplayInfoValue & 0x20) == 0x20;
        public bool LDADisplaySet => (DisplayInfoValue & 0x40) == 0x40;
        public bool ModeTimingOverrideSet => (DisplayInfoValue & 0x80) == 0x80;
        public bool MannerSingleSet => (DisplayInfoValue & 0x100) == 0x100;
        public bool MannerCloneSet => (DisplayInfoValue & 0x200) == 0x200;
        public bool Manner2VStretchSet => (DisplayInfoValue & 0x400) == 0x400;
        public bool Manner2HStretchSet => (DisplayInfoValue & 0x800) == 0x800;
        public bool MannerExtendedSet => (DisplayInfoValue & 0x1000) == 0x1000;
        public bool MannerNStretch1GPUSet => (DisplayInfoValue & 0x10000) == 0x10000;
        public bool MannerNStretchNGPUSet => (DisplayInfoValue & 0x20000) == 0x20000;
        public bool MannerReserved2Set => (DisplayInfoValue & 0x40000) == 0x40000;
        public bool MannerReserved3Set => (DisplayInfoValue & 0x80000) == 0x80000;
        public bool ShowTypeProjectorSet => (DisplayInfoValue & 0x100000) == 0x100000;

        public override bool Equals(object obj) => obj is ADL_DISPLAY_INFO other && this.Equals(other);
        public bool Equals(ADL_DISPLAY_INFO other)
            => DisplayID.Equals(other.DisplayID) &&
                DisplayControllerIndex == other.DisplayControllerIndex &&
                DisplayName.Equals(other.DisplayName) &&
                DisplayID.Equals(other.DisplayID) &&
                DisplayType == other.DisplayType &&
                DisplayOutputType == other.DisplayOutputType &&
                DisplayConnector == other.DisplayConnector &&
                DisplayInfoMask == other.DisplayInfoMask &&
                DisplayInfoValue == other.DisplayInfoValue;

        public override int GetHashCode()
        {
            return (DisplayID, DisplayControllerIndex, DisplayName, DisplayID, DisplayType, DisplayOutputType, DisplayConnector, DisplayInfoMask, DisplayInfoValue).GetHashCode();
        }

        public static bool operator ==(ADL_DISPLAY_INFO lhs, ADL_DISPLAY_INFO rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_DISPLAY_INFO lhs, ADL_DISPLAY_INFO rhs) => !(lhs == rhs);
        //public override string ToString() => $"{type.ToString("G")}";
    }


    /// <summary> ADLDisplayConfig Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_DISPLAY_CONFIG : IEquatable<ADL_DISPLAY_CONFIG>
    {
        /// <summary> Size of this data structure </summary>
        public long Size;
        /// <summary> HDTV Connector Type </summary>
        public long ConnectorType;
        /// <summary> HDTV Capabilities themselves </summary>
        public long DeviceData;
        /// <summary> Overridden HDTV capabilities</summary>
        public long OverriddedDeviceData;
        /// <summary> Reserved for future use</summary>
        public long Reserved;

        // Connector Type
        public bool ConnectorTypeIsUnknown => ConnectorType == 0;
        public bool ConnectorTypeIsCVNonI2CJP => ConnectorType == 1;
        public bool ConnectorTypeIsCVJPN => ConnectorType == 2;
        public bool ConnectorTypeIsCVNA => ConnectorType == 3;
        public bool ConnectorTypeIsCVNonI2CNA => ConnectorType == 4;
        public bool ConnectorTypeIsVGA => ConnectorType == 5;
        public bool ConnectorTypeIsDVI_D => ConnectorType == 6;
        public bool ConnectorTypeIsDVI_I => ConnectorType == 7;
        public bool ConnectorTypeIsHDMITypeA => ConnectorType == 8;
        public bool ConnectorTypeIsHDMITypeB => ConnectorType == 9;
        public bool ConnectorTypeIsDisplayPort => ConnectorType == 10;

        public override bool Equals(object obj) => obj is ADL_DISPLAY_CONFIG other && this.Equals(other);
        public bool Equals(ADL_DISPLAY_CONFIG other)
            => Size == other.Size &&
                ConnectorType == other.ConnectorType &&
                DeviceData == other.DeviceData &&
                OverriddedDeviceData == other.OverriddedDeviceData;

        public override int GetHashCode()
        {
            return (Size, ConnectorType, DeviceData, OverriddedDeviceData).GetHashCode();
        }

        public static bool operator ==(ADL_DISPLAY_CONFIG lhs, ADL_DISPLAY_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_DISPLAY_CONFIG lhs, ADL_DISPLAY_CONFIG rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLDisplayMap Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_DISPLAY_MAP : IEquatable<ADL_DISPLAY_MAP>
    {
        /// <summary> The current display map index. It is the OS desktop index. For example, if the OS index 1 is showing clone mode, the display map will be 1. </summary>
        public int DisplayMapIndex;
        /// <summary> The Display Mode for the current map.</summary>
        public ADL_MODE DisplayMode;
        /// <summary> The number of display targets belongs to this map </summary>
        public int NumDisplayTarget;
        /// <summary> The first target array index in the Target array </summary>
        public int FirstDisplayTargetArrayIndex;
        /// <summary> The bit mask identifies the number of bits DisplayMap is currently using. It is the sum of all the bit definitions defined in ADL_DISPLAY_DISPLAYMAP_MANNER_xxx.</summary>
        public int DisplayMapMask;
        /// <summary> The bit mask identifies the display status. The detailed definition is in ADL_DISPLAY_DISPLAYMAP_MANNER_xxx.</summary>
        public int DisplayMapValue;

        // Display Map Mask settings
        public bool DisplayMapReservedSupported => (DisplayMapMask & 0x1) == 0x1;
        public bool DisplayMapNotActiveSupported => (DisplayMapMask & 0x2) == 0x2;
        public bool DisplayMapSingleSupported => (DisplayMapMask & 0x4) == 0x4;
        public bool DisplayMapCloneSupported => (DisplayMapMask & 0x8) == 0x8;
        public bool DisplayMapReserved1Supported => (DisplayMapMask & 0x10) == 0x10;
        public bool DisplayMapHStretchSupported => (DisplayMapMask & 0x20) == 0x20;
        public bool DisplayMapVStretchSupported => (DisplayMapMask & 0x40) == 0x40;
        public bool DisplayMapVLDSupported => (DisplayMapMask & 0x80) == 0x80;

        // Display Map Value settings
        public bool DisplayMapReservedSet => (DisplayMapValue & 0x1) == 0x1;
        public bool DisplayMapNotActiveSet => (DisplayMapValue & 0x2) == 0x2;
        public bool DisplayMapSingleSet => (DisplayMapValue & 0x4) == 0x4;
        public bool DisplayMapCloneSet => (DisplayMapValue & 0x8) == 0x8;
        public bool DisplayMapReserved1Set => (DisplayMapValue & 0x10) == 0x10;
        public bool DisplayMapHStretchSet => (DisplayMapValue & 0x20) == 0x20;
        public bool DisplayMapVStretchSet => (DisplayMapValue & 0x40) == 0x40;
        public bool DisplayMapVLDSet => (DisplayMapValue & 0x80) == 0x80;

        public override bool Equals(object obj) => obj is ADL_DISPLAY_MAP other && this.Equals(other);
        public bool Equals(ADL_DISPLAY_MAP other)
            => DisplayMapIndex == other.DisplayMapIndex &&
                DisplayMode.Equals(other.DisplayMode) &&
                NumDisplayTarget == other.NumDisplayTarget &&
                FirstDisplayTargetArrayIndex == other.FirstDisplayTargetArrayIndex &&
                DisplayMapMask == other.DisplayMapMask &&
                DisplayMapValue == other.DisplayMapValue;

        public override int GetHashCode()
        {
            return (DisplayMapIndex, DisplayMode, NumDisplayTarget, FirstDisplayTargetArrayIndex, DisplayMapMask, DisplayMapValue).GetHashCode();
        }

        public static bool operator ==(ADL_DISPLAY_MAP lhs, ADL_DISPLAY_MAP rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_DISPLAY_MAP lhs, ADL_DISPLAY_MAP rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLAdapterCaps Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_ADAPTER_CAPSX2 : IEquatable<ADL_ADAPTER_CAPSX2>
    {
        /// <summary> AdapterID for this adapter </summary>
        public int AdapterID;
        /// <summary> Number of controllers for this adapter. </summary>
        public int NumControllers;
        /// <summary> Number of displays for this adapter.</summary>
        public int NumDisplays;
        /// <summary> Number of overlays for this adapter.</summary>
        public int NumOverlays;
        /// <summary> Number of GLSyncConnectors. </summary>
        public int NumOfGLSyncConnectors;
        /// <summary> The bit mask identifies the adapter caps. </summary>
        public int CapsMask;
        /// <summary> The bit identifies the adapter caps define_adapter_caps. </summary>
        public int CapsValue;
        /// <summary> Number of Connectors for this adapter. </summary>
        public int NumConnectors;

        // Caps Mask settings
        public bool CapNotActiveSupported => (CapsMask & 0x1) == 0x1;
        public bool CapSingleSupported => (CapsMask & 0x2) == 0x2;
        public bool CapCloneSupported => (CapsMask & 0x4) == 0x4;
        public bool CapNStretch1GPUSupported => (CapsMask & 0x8) == 0x8;
        public bool CapNStretchNGPUSupported => (CapsMask & 0x10) == 0x10;
        public bool Cap2VStretchSupported => (CapsMask & 0x20) == 0x20;
        public bool Cap2HStretchSupported => (CapsMask & 0x40) == 0x40;
        public bool CapExtendedSupported => (CapsMask & 0x80) == 0x80;
        public bool CapPreferredDisplaySupported => (CapsMask & 0x100) == 0x100;
        public bool CapBezelSupported => (CapsMask & 0x200) == 0x200;

        // Caps Value settings
        public bool CapNotActiveSet => (CapsValue & 0x1) == 0x1;
        public bool CapSingleSet => (CapsValue & 0x2) == 0x2;
        public bool CapCloneSet => (CapsValue & 0x4) == 0x4;
        public bool CapNStretch1GPUSet => (CapsValue & 0x8) == 0x8;
        public bool CapNStretchNGPUSet => (CapsValue & 0x10) == 0x10;
        public bool Cap2VStretchSet => (CapsValue & 0x20) == 0x20;
        public bool Cap2HStretchSet => (CapsValue & 0x40) == 0x40;
        public bool CapExtendedSet => (CapsValue & 0x80) == 0x80;
        public bool CapPreferredDisplaySet => (CapsValue & 0x100) == 0x100;
        public bool CapBezelSet => (CapsValue & 0x200) == 0x200;

        public override bool Equals(object obj) => obj is ADL_ADAPTER_CAPSX2 other && this.Equals(other);
        public bool Equals(ADL_ADAPTER_CAPSX2 other)
            => AdapterID == other.AdapterID &&
                NumControllers == other.NumControllers &&
                NumDisplays == other.NumDisplays &&
                NumOverlays == other.NumOverlays &&
                NumOfGLSyncConnectors == other.NumOfGLSyncConnectors &&
                CapsMask == other.CapsMask &&
                CapsValue == other.CapsValue &&
                NumConnectors == other.NumConnectors;

        public override int GetHashCode()
        {
            return (AdapterID, NumControllers, NumDisplays, NumOverlays, NumOfGLSyncConnectors, CapsMask, CapsValue, NumConnectors).GetHashCode();
        }

        public static bool operator ==(ADL_ADAPTER_CAPSX2 lhs, ADL_ADAPTER_CAPSX2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_ADAPTER_CAPSX2 lhs, ADL_ADAPTER_CAPSX2 rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLPossibleMap Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_POSSIBLE_MAP : IEquatable<ADL_POSSIBLE_MAP>
    {
        /// <summary> Index</summary>
        public int Index;
        /// <summary> Adapter Index. </summary>
        public int AdapterIndex;
        /// <summary> Display Map Number</summary>
        public int NumDisplayMap;
        /// <summary> The DisplayMaps being tested</summary>
        public ADL_DISPLAY_MAP DisplayMaps;
        /// <summary> Number of Display Targets</summary>
        public int NumDisplayTarget;
        /// <summary> The DisplayTargets being tested </summary>
        public ADL_DISPLAY_TARGET DisplayTargets;

        public override bool Equals(object obj) => obj is ADL_POSSIBLE_MAP other && this.Equals(other);
        public bool Equals(ADL_POSSIBLE_MAP other)
            => Index == other.Index &&
                AdapterIndex == other.AdapterIndex &&
                NumDisplayMap == other.NumDisplayMap &&
                DisplayMaps.Equals(other.DisplayMaps) &&
                NumDisplayTarget == other.NumDisplayTarget &&
                DisplayTargets.Equals(other.DisplayTargets);

        public override int GetHashCode()
        {
            return (Index, AdapterIndex, NumDisplayMap, DisplayMaps, NumDisplayTarget, DisplayTargets).GetHashCode();
        }

        public static bool operator ==(ADL_POSSIBLE_MAP lhs, ADL_POSSIBLE_MAP rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_POSSIBLE_MAP lhs, ADL_POSSIBLE_MAP rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLPossibleMapping Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_POSSIBLE_MAPPING : IEquatable<ADL_POSSIBLE_MAPPING>
    {
        /// <summary>Display Index</summary>
        public int DisplayIndex;
        /// <summary> Display Controller Index</summary>
        public int DisplayControllerIndex;
        /// <summary> The display manner options supported</summary>
        public int DisplayMannerSupported;

        // Display Manner Supported settings
        public bool MapReservedSupported => (DisplayMannerSupported & 0x1) == 0x1;
        public bool MapNotActiveSupported => (DisplayMannerSupported & 0x2) == 0x2;
        public bool MapSingleSupported => (DisplayMannerSupported & 0x4) == 0x4;
        public bool MapCloneSupported => (DisplayMannerSupported & 0x8) == 0x8;
        public bool MapReserved1Supported => (DisplayMannerSupported & 0x10) == 0x10;
        public bool MapHStretchSupported => (DisplayMannerSupported & 0x20) == 0x20;
        public bool MapVStretchSupported => (DisplayMannerSupported & 0x40) == 0x40;
        public bool MapVLDSupported => (DisplayMannerSupported & 0x80) == 0x80;

        // ADL_DISPLAY_DISPLAYMAP_MANNER_ Definitions
        // for ADLDisplayMap.iDisplayMapMask and ADLDisplayMap.iDisplayMapValue
        // (bit-vector)
        /*#define ADL_DISPLAY_DISPLAYMAP_MANNER_RESERVED            0x00000001
        #define ADL_DISPLAY_DISPLAYMAP_MANNER_NOTACTIVE            0x00000002
        #define ADL_DISPLAY_DISPLAYMAP_MANNER_SINGLE            0x00000004
        #define ADL_DISPLAY_DISPLAYMAP_MANNER_CLONE                0x00000008
        #define ADL_DISPLAY_DISPLAYMAP_MANNER_RESERVED1            0x00000010  // Removed NSTRETCH
        #define ADL_DISPLAY_DISPLAYMAP_MANNER_HSTRETCH            0x00000020
        #define ADL_DISPLAY_DISPLAYMAP_MANNER_VSTRETCH            0x00000040
        #define ADL_DISPLAY_DISPLAYMAP_MANNER_VLD                0x00000080*/

        public override bool Equals(object obj) => obj is ADL_POSSIBLE_MAPPING other && this.Equals(other);
        public bool Equals(ADL_POSSIBLE_MAPPING other)
            => DisplayIndex == other.DisplayIndex &&
                DisplayControllerIndex == other.DisplayControllerIndex &&
                DisplayMannerSupported == other.DisplayMannerSupported;

        public override int GetHashCode()
        {
            return (DisplayIndex, DisplayControllerIndex, DisplayMannerSupported).GetHashCode();
        }

        public static bool operator ==(ADL_POSSIBLE_MAPPING lhs, ADL_POSSIBLE_MAPPING rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_POSSIBLE_MAPPING lhs, ADL_POSSIBLE_MAPPING rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLPossibleMapResult Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_POSSIBLE_MAP_RESULT : IEquatable<ADL_POSSIBLE_MAP_RESULT>
    {
        /// <summary>Index</summary>
        public int Index;
        // The bit mask and value identifies the number of bits PossibleMapResult is currently using. It will be the sum all the bit definitions defined in ADL_DISPLAY_POSSIBLEMAPRESULT_VALID.
        /// <summary> Display Controller Index</summary>
        public int PossibleMapResultMask;
        /// <summary> The display manner options supported</summary>
        public int PossibleMapResultValue;

        // Possible Map Result Mask settings
        public bool MapResultValidSupported => (PossibleMapResultMask & 0x1) == 0x1;
        public bool MapNotActiveSupported => (PossibleMapResultMask & 0x2) == 0x2;
        public bool MapSingleSupported => (PossibleMapResultMask & 0x4) == 0x4;

        // Possible Map Result Mask settings
        public bool MapResultValidSet => (PossibleMapResultValue & 0x1) == 0x1;
        public bool MapNotActiveSet => (PossibleMapResultValue & 0x2) == 0x2;
        public bool MapSingleSet => (PossibleMapResultValue & 0x4) == 0x4;


        // ADL_DISPLAY_POSSIBLEMAPRESULT_VALID Definitions
        // for ADLPossibleMapResult.iPossibleMapResultMask and ADLPossibleMapResult.iPossibleMapResultValue
        // (bit-vector)
        /*#define ADL_DISPLAY_POSSIBLEMAPRESULT_VALID                0x00000001
        #define ADL_DISPLAY_POSSIBLEMAPRESULT_BEZELSUPPORTED    0x00000002
        #define ADL_DISPLAY_POSSIBLEMAPRESULT_OVERLAPSUPPORTED    0x00000004*/

        public override bool Equals(object obj) => obj is ADL_POSSIBLE_MAP_RESULT other && this.Equals(other);
        public bool Equals(ADL_POSSIBLE_MAP_RESULT other)
            => Index == other.Index &&
                PossibleMapResultMask == other.PossibleMapResultMask &&
                PossibleMapResultValue == other.PossibleMapResultValue;

        public override int GetHashCode()
        {
            return (Index, PossibleMapResultMask, PossibleMapResultValue).GetHashCode();
        }

        public static bool operator ==(ADL_POSSIBLE_MAP_RESULT lhs, ADL_POSSIBLE_MAP_RESULT rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_POSSIBLE_MAP_RESULT lhs, ADL_POSSIBLE_MAP_RESULT rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLSLSGrid Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_SLS_GRID : IEquatable<ADL_SLS_GRID>
    {
        /// <summary> The Adapter index </summary>
        public int AdapterIndex;
        /// <summary> The grid index </summary>
        public int SLSGridIndex;
        /// <summary>The grid row. </summary>
        public int SLSGridRow;
        /// <summary> The grid column </summary>
        public int SLSGridColumn;
        /// <summary> The grid bit mask identifies the number of bits DisplayMap is currently using. </summary>
        public int SLSGridMask;
        /// <summary> The grid bit value identifies the display status. </summary>
        public int SLSGridValue;

        // SLS Grid Mask settings
        public bool SLSGridRelativeToLandscapeSupported => (SLSGridMask & 0x1) == 0x1;
        public bool SLSGridRelativeToCurrentAngleSupported => (SLSGridMask & 0x2) == 0x2;
        public bool SLSGridPortraitModeSupported => (SLSGridMask & 0x4) == 0x4;
        public bool SLSGridKeepTargetRotationSupported => (SLSGridMask & 0x8) == 0x8;
        public bool SLSGridSameModeSLSSupported => (SLSGridMask & 0x10) == 0x10;
        public bool SLSGridMixModeSLSSupported => (SLSGridMask & 0x20) == 0x20;
        public bool SLSGridDisplayRotationSupported => (SLSGridMask & 0x40) == 0x40;
        public bool SLSGridDesktopRotationSupported => (SLSGridMask & 0x80) == 0x80;

        // SLS Grid Value settings
        public bool SLSGridRelativeToLandscapeSet => (SLSGridValue & 0x1) == 0x1;
        public bool SLSGridRelativeToCurrentAngleSet => (SLSGridValue & 0x2) == 0x2;
        public bool SLSGridPortraitModeSet => (SLSGridValue & 0x4) == 0x4;
        public bool SLSGridKeepTargetRotationSet => (SLSGridValue & 0x8) == 0x8;
        public bool SLSGridSameModeSLSSet => (SLSGridValue & 0x10) == 0x10;
        public bool SLSGridMixModeSLSSet => (SLSGridValue & 0x20) == 0x20;
        public bool SLSGridDisplayRotationSet => (SLSGridValue & 0x40) == 0x40;
        public bool SLSGridDesktopRotationSet => (SLSGridValue & 0x80) == 0x80;

        /*#define ADL_DISPLAY_SLSGRID_CAP_OPTION_RELATIVETO_LANDSCAPE     0x00000001
        #define ADL_DISPLAY_SLSGRID_CAP_OPTION_RELATIVETO_CURRENTANGLE     0x00000002
        #define ADL_DISPLAY_SLSGRID_PORTAIT_MODE                         0x00000004
        #define ADL_DISPLAY_SLSGRID_KEEPTARGETROTATION                  0x00000080

        #define ADL_DISPLAY_SLSGRID_SAMEMODESLS_SUPPORT        0x00000010
        #define ADL_DISPLAY_SLSGRID_MIXMODESLS_SUPPORT        0x00000020
        #define ADL_DISPLAY_SLSGRID_DISPLAYROTATION_SUPPORT    0x00000040
        #define ADL_DISPLAY_SLSGRID_DESKTOPROTATION_SUPPORT    0x00000080*/

        public override bool Equals(object obj) => obj is ADL_SLS_GRID other && this.Equals(other);
        public bool Equals(ADL_SLS_GRID other)
            => AdapterIndex == other.AdapterIndex &&
                SLSGridIndex == other.SLSGridIndex &&
                SLSGridRow == other.SLSGridRow &&
                SLSGridColumn == other.SLSGridColumn &&
                SLSGridMask == other.SLSGridMask &&
                SLSGridValue == other.SLSGridValue;

        public override int GetHashCode()
        {
            return (AdapterIndex, SLSGridIndex, SLSGridRow, SLSGridColumn, SLSGridMask, SLSGridValue).GetHashCode();
        }
        public static bool operator ==(ADL_SLS_GRID lhs, ADL_SLS_GRID rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_SLS_GRID lhs, ADL_SLS_GRID rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLSLSMap Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_SLS_MAP : IEquatable<ADL_SLS_MAP>
    {
        /// <summary> The Adapter Index </summary>
        public int AdapterIndex;
        /// <summary> The current display map index. It is the OS Desktop index. </summary>
        public int SLSMapIndex;
        /// <summary> The current grid </summary>
        public ADL_SLS_GRID Grid;
        /// <summary> OS Surface Index </summary>
        public int SurfaceMapIndex;
        /// <summary> Screen orientation. E.g., 0, 90, 180, 270. </summary>
        public int Orientation;
        /// <summary> The number of display targets belongs to this map. </summary>
        public int NumSLSTarget;
        /// <summary> The first target array index in the Target array. </summary>
        public int FirstSLSTargetArrayIndex;
        /// <summary> The number of native modes belongs to this map. </summary>
        public int NumNativeMode;
        /// <summary>The first native mode array index in the native mode array. </summary>
        public int FirstNativeModeArrayIndex;
        /// <summary> The number of bezel modes belongs to this map. </summary>
        public int NumBezelMode;
        /// <summary> The first bezel mode array index in the native mode array </summary>
        public int FirstBezelModeArrayIndex;
        /// <summary> The number of bezel offsets belongs to this map. </summary>
        public int NumBezelOffset;
        /// <summary> The first bezel offset array index in the native mode array </summary>
        public int FirstBezelOffsetArrayIndex;
        /// <summary> Bitmask identifies display map status </summary>
        public int SLSMapMask;
        /// <summary> Bitmask identifies display map status </summary>
        public int SLSMapValue;

        // SLS Orientation settings
        public bool Orientation000 => (Orientation & 0x1) == 0x1;
        public bool Orientation090 => (Orientation & 0x2) == 0x2;
        public bool Orientation180 => (Orientation & 0x4) == 0x4;
        public bool Orientation270 => (Orientation & 0x8) == 0x8;

        /*#define ADL_DISPLAY_SLSGRID_ORIENTATION_000        0x00000001
        #define ADL_DISPLAY_SLSGRID_ORIENTATION_090        0x00000002
        #define ADL_DISPLAY_SLSGRID_ORIENTATION_180        0x00000004
        #define ADL_DISPLAY_SLSGRID_ORIENTATION_270        0x00000008*/

        // SLS Map Mask settings
        public bool SLSMapDisplayArrangedSupported => (SLSMapMask & 0x2) == 0x2;
        public bool SLSMapCurrentInUseSupported => (SLSMapMask & 0x4) == 0x4;
        public bool SLSMapBezelModeSupported => (SLSMapMask & 0x10) == 0x10;
        public bool SLSMapLayoutModeFitSupported => (SLSMapMask & 0x100) == 0x100;
        public bool SLSMapLayoutModeFillSupported => (SLSMapMask & 0x200) == 0x200;
        public bool SLSMapLayoutModeExpandSupported => (SLSMapMask & 0x400) == 0x400;
        public bool SLSMapIsSLSSupported => (SLSMapMask & 0x1000) == 0x1000;
        public bool SLSMapIsSLSBuilderSupported => (SLSMapMask & 0x2000) == 0x2000;
        public bool SLSMapIsCloneVTSupported => (SLSMapMask & 0x4000) == 0x4000;

        // SLS Map Value settings
        public bool SLSMapDisplayArrangedSet => (SLSMapValue & 0x2) == 0x2;
        public bool SLSMapCurrentInUseSet => (SLSMapValue & 0x4) == 0x4;
        public bool SLSMapBezelModeSet => (SLSMapValue & 0x10) == 0x10;
        public bool SLSMapLayoutModeFitSet => (SLSMapValue & 0x100) == 0x100;
        public bool SLSMapLayoutModeFillSet => (SLSMapValue & 0x200) == 0x200;
        public bool SLSMapLayoutModeExpandSet => (SLSMapValue & 0x400) == 0x400;
        public bool SLSMapIsSLSSet => (SLSMapValue & 0x1000) == 0x1000;
        public bool SLSMapIsSLSBuilderSet => (SLSMapValue & 0x2000) == 0x2000;
        public bool SLSMapIsCloneVTSet => (SLSMapValue & 0x4000) == 0x4000;


        /*#define ADL_DISPLAY_SLSMAP_DISPLAYARRANGED        0x0002
        #define ADL_DISPLAY_SLSMAP_CURRENTCONFIG        0x0004
        #define ADL_DISPLAY_SLSMAP_BEZELMODE            0x0010
        #define ADL_DISPLAY_SLSMAP_SLSLAYOUTMODE_FIT        0x0100
        #define ADL_DISPLAY_SLSMAP_SLSLAYOUTMODE_FILL       0x0200
        #define ADL_DISPLAY_SLSMAP_SLSLAYOUTMODE_EXPAND     0x0400
        #define ADL_DISPLAY_SLSMAP_IS_SLS        0x1000
        #define ADL_DISPLAY_SLSMAP_IS_SLSBUILDER 0x2000
        #define ADL_DISPLAY_SLSMAP_IS_CLONEVT     0x4000
        */

        public override bool Equals(object obj) => obj is ADL_SLS_MAP other && this.Equals(other);
        public bool Equals(ADL_SLS_MAP other)
            => AdapterIndex == other.AdapterIndex &&
                SLSMapIndex == other.SLSMapIndex &&
                Grid.Equals(other.Grid) &&
                SurfaceMapIndex == other.SurfaceMapIndex &&
                Orientation == other.Orientation &&
                NumSLSTarget == other.NumSLSTarget &&
                FirstSLSTargetArrayIndex == other.FirstSLSTargetArrayIndex &&
                NumNativeMode == other.NumNativeMode &&
                FirstNativeModeArrayIndex == other.FirstNativeModeArrayIndex &&
                NumBezelMode == other.NumBezelMode &&
                FirstBezelModeArrayIndex == other.FirstBezelModeArrayIndex &&
                NumBezelOffset == other.NumBezelOffset &&
                FirstBezelOffsetArrayIndex == other.FirstBezelOffsetArrayIndex &&
                SLSMapMask == other.SLSMapMask &&
                SLSMapValue == other.SLSMapValue;

        public override int GetHashCode()
        {
            return (AdapterIndex, SLSMapIndex, Grid, SurfaceMapIndex, Orientation, NumSLSTarget, FirstSLSTargetArrayIndex, NumNativeMode, FirstNativeModeArrayIndex, NumBezelMode, FirstBezelModeArrayIndex,
                NumBezelOffset, FirstBezelOffsetArrayIndex, SLSMapMask, SLSMapValue).GetHashCode();
        }

        public static bool operator ==(ADL_SLS_MAP lhs, ADL_SLS_MAP rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_SLS_MAP lhs, ADL_SLS_MAP rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLSLSTarget Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_SLS_TARGET : IEquatable<ADL_SLS_TARGET>
    {
        /// <summary> The Adapter Index </summary>
        public int AdapterIndex;
        /// <summary> The SLS map index. </summary>
        public int SLSMapIndex;
        /// <summary> The target ID.  </summary>
        public ADL_DISPLAY_TARGET DisplayTarget;
        /// <summary> Target postion X in SLS grid </summary>
        public int SLSGridPositionX;
        /// <summary> Target postion Y in SLS grid </summary>
        public int SLSGridPositionY;
        /// <summary> The view size width, height and rotation angle per SLS Target.  </summary>
        public ADL_MODE ViewSize;
        /// <summary> The bit mask identifies the bits in iSLSTargetValue are currently used. Should be set to 1 unless you are using SLS Builder. </summary>
        public int SLSTargetMask;
        /// <summary> The bit mask identifies status info. Should be set to 1 unless you are using SLS Builder.</summary>
        public int SLSTargetValue;


        // SLSTargetMask settings
        public bool SLSTargetNotSLSBuilderSupported => (SLSTargetMask & 0x1) == 0x1;

        // SLSTargetValue settings
        public bool SLSTargetNotSLSBuilderSet => (SLSTargetValue & 0x1) == 0x1;

        public override bool Equals(object obj) => obj is ADL_SLS_TARGET other && this.Equals(other);
        public bool Equals(ADL_SLS_TARGET other)
            => AdapterIndex == other.AdapterIndex &&
                SLSMapIndex == other.SLSMapIndex &&
                DisplayTarget.Equals(other.DisplayTarget) &&
                SLSGridPositionX == other.SLSGridPositionX &&
                SLSGridPositionY == other.SLSGridPositionY &&
                ViewSize.Equals(other.ViewSize) &&
                SLSTargetMask == other.SLSTargetMask &&
                SLSTargetValue == other.SLSTargetValue;

        public override int GetHashCode()
        {
            return (AdapterIndex, SLSMapIndex, DisplayTarget, SLSGridPositionX, SLSGridPositionY, ViewSize, SLSTargetMask, SLSTargetValue).GetHashCode();
        }

        public static bool operator ==(ADL_SLS_TARGET lhs, ADL_SLS_TARGET rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_SLS_TARGET lhs, ADL_SLS_TARGET rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLSLSMode Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_SLS_MODE : IEquatable<ADL_SLS_MODE>
    {
        /// <summary> The Adapter Index </summary>
        public int AdapterIndex;
        /// <summary> The current display map index. It is the OS Desktop index. </summary>
        public int SLSMapIndex;
        /// <summary> The mode index. </summary>
        public int SLSModeIndex;
        /// <summary> The target ID.  </summary>
        public ADL_MODE DisplayMode;
        /// <summary> The bit mask identifies the number of bits Mode is currently using. </summary>
        public int SLSNativeModeMask;
        /// <summary> The bit mask identifies the display status. </summary>
        public int SLSNativeModeValue;

        public override bool Equals(object obj) => obj is ADL_SLS_MODE other && this.Equals(other);
        public bool Equals(ADL_SLS_MODE other)
            => AdapterIndex == other.AdapterIndex &&
                SLSMapIndex == other.SLSMapIndex &&
                SLSModeIndex == other.SLSModeIndex &&
                DisplayMode.Equals(other.DisplayMode) &&
                SLSNativeModeMask == other.SLSNativeModeMask &&
                SLSNativeModeValue == other.SLSNativeModeValue;

        public override int GetHashCode()
        {
            return (AdapterIndex, SLSMapIndex, SLSModeIndex, DisplayMode, SLSNativeModeMask, SLSNativeModeValue).GetHashCode();
        }

        public static bool operator ==(ADL_SLS_MODE lhs, ADL_SLS_MODE rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_SLS_MODE lhs, ADL_SLS_MODE rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLBezelTransientMode Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_BEZEL_TRANSIENT_MODE : IEquatable<ADL_BEZEL_TRANSIENT_MODE>
    {
        /// <summary> The Adapter Index </summary>
        public int AdapterIndex;
        /// <summary> SLS Map Index. </summary>
        public int SLSMapIndex;
        /// <summary> SLS Mode Index. </summary>
        public int SLSModeIndex;
        /// <summary> The target ID.  </summary>
        public ADL_MODE DisplayMode;
        /// <summary> The number of bezel offsets belongs to this map.  </summary>
        public int NumBezelOffset;
        /// <summary> The first bezel offset array index in the native mode array. </summary>
        public int FirstBezelOffsetArrayIndex;
        /// <summary> The bit mask identifies the bits this structure is currently using. </summary>
        public int SLSBezelTransientModeMask;
        /// <summary> The bit mask identifies the display status.  </summary>
        public int SLSBezelTransientModeValue;

        public override bool Equals(object obj) => obj is ADL_BEZEL_TRANSIENT_MODE other && this.Equals(other);
        public bool Equals(ADL_BEZEL_TRANSIENT_MODE other)
            => AdapterIndex == other.AdapterIndex &&
                SLSMapIndex == other.SLSMapIndex &&
                SLSModeIndex == other.SLSModeIndex &&
                DisplayMode.Equals(other.DisplayMode) &&
                NumBezelOffset == other.NumBezelOffset &&
                FirstBezelOffsetArrayIndex == other.FirstBezelOffsetArrayIndex &&
                SLSBezelTransientModeMask == other.SLSBezelTransientModeMask &&
                SLSBezelTransientModeValue == other.SLSBezelTransientModeValue;

        public override int GetHashCode()
        {
            return (AdapterIndex, SLSMapIndex, SLSModeIndex, DisplayMode, NumBezelOffset, FirstBezelOffsetArrayIndex, SLSBezelTransientModeMask, SLSBezelTransientModeValue).GetHashCode();
        }

        public static bool operator ==(ADL_BEZEL_TRANSIENT_MODE lhs, ADL_BEZEL_TRANSIENT_MODE rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_BEZEL_TRANSIENT_MODE lhs, ADL_BEZEL_TRANSIENT_MODE rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }

    /// <summary> ADLPossibleSLSMap Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_POSSIBLE_SLS_MAP : IEquatable<ADL_POSSIBLE_SLS_MAP>
    {
        /// <summary> SLS Map Index. </summary>
        public int SLSMapIndex;
        /// <summary> Num SLS Map. </summary>
        public int NumSLSMap;
        /// <summary> The SLS Map List.  </summary>
        public ADL_SLS_MAP[] SLSMaps;  // Not quite sure this is right
        /// <summary> The number of SLS Targets</summary>
        public int NumSLSTarget;
        /// <summary> The SLS Target List. </summary>
        public ADL_SLS_TARGET[] SLSTargets; // Not quite sure this is right

        public override bool Equals(object obj) => obj is ADL_POSSIBLE_SLS_MAP other && this.Equals(other);
        public bool Equals(ADL_POSSIBLE_SLS_MAP other)
            => SLSMapIndex == other.SLSMapIndex &&
                NumSLSMap == other.NumSLSMap &&
                SLSMaps.Equals(other.SLSMaps) &&
                NumSLSTarget == other.NumSLSTarget &&
                SLSTargets.Equals(other.SLSTargets);

        public override int GetHashCode()
        {
            return (SLSMapIndex, NumSLSMap, SLSMaps, NumSLSTarget, SLSTargets).GetHashCode();
        }

        public static bool operator ==(ADL_POSSIBLE_SLS_MAP lhs, ADL_POSSIBLE_SLS_MAP rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_POSSIBLE_SLS_MAP lhs, ADL_POSSIBLE_SLS_MAP rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }


    /// <summary> ADLSLSOffset Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADL_SLS_OFFSET : IEquatable<ADL_SLS_OFFSET>
    {
        /// <summary> The Adapter Index </summary>
        public int AdapterIndex;
        /// <summary> The current display map index. It is the OS Desktop index. </summary>
        public int SLSMapIndex;
        /// <summary> The target ID.  </summary>
        public ADL_DISPLAY_ID DisplayID;
        /// <summary> SLS Bezel Mode Index. </summary>
        public int BezelModeIndex;
        /// <summary>SLS Bezel Offset X. </summary>
        public int BezelOffsetX;
        /// <summary>SLS Bezel Offset Y. </summary>
        public int BezelOffsetY;
        /// <summary> SLS Display Width. </summary>
        public int DisplayWidth;
        /// <summary> SLS Display Height. </summary>
        public int DisplayHeight;
        /// <summary> The bit mask identifies the number of bits Offset is currently using. </summary>
        public int BezelOffsetMask;
        /// <summary> The bit mask identifies the display status. </summary>
        public int BezelOffsetValue;

        // BezelOffsetMask settings
        public bool SLSBezelOffsetStepByStepSupported => (BezelOffsetMask & 0x4) == 0x4;
        public bool SLSBezelOffsetCommitSupported => (BezelOffsetMask & 0x8) == 0x8;

        // BezelOffsetValue settings
        public bool SLSBezelOffsetStepByStepSet => (BezelOffsetValue & 0x4) == 0x4;
        public bool SLSBezelOffsetCommitSet => (BezelOffsetValue & 0x8) == 0x8;

        /*#define ADL_DISPLAY_BEZELOFFSET_STEPBYSTEPSET            0x00000004
        #define ADL_DISPLAY_BEZELOFFSET_COMMIT                    0x00000008*/

        public override bool Equals(object obj) => obj is ADL_SLS_OFFSET other && this.Equals(other);
        public bool Equals(ADL_SLS_OFFSET other)
            => AdapterIndex == other.AdapterIndex &&
                SLSMapIndex == other.SLSMapIndex &&
                DisplayID.Equals(other.DisplayID) &&
                BezelModeIndex == other.BezelModeIndex &&
                BezelOffsetX == other.BezelOffsetX &&
                BezelOffsetY == other.BezelOffsetY &&
                DisplayWidth == other.DisplayWidth &&
                DisplayHeight == other.DisplayHeight &&
                BezelOffsetMask == other.BezelOffsetMask &&
                BezelOffsetValue == other.BezelOffsetValue;

        public override int GetHashCode()
        {
            return (AdapterIndex, SLSMapIndex, DisplayID, BezelModeIndex, BezelOffsetX, BezelOffsetY, DisplayWidth, DisplayHeight, BezelOffsetMask, BezelOffsetValue).GetHashCode();
        }

        public static bool operator ==(ADL_SLS_OFFSET lhs, ADL_SLS_OFFSET rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADL_SLS_OFFSET lhs, ADL_SLS_OFFSET rhs) => !(lhs == rhs);

        //public override string ToString() => $"{type.ToString("G")}";
    }


    class ADLImport
    {

        /// <summary> Define false </summary>
        public const int ADL_FALSE = 0;
        /// <summary> Define true </summary>
        public const int ADL_TRUE = 1;

        /// <summary> Selects all adapters instead of aparticular single adapter</summary>
        public const int ADL_ADAPTER_INDEX_ALL = -1;
        ///    Defines APIs with iOption none
        public const int ADL_MAIN_API_OPTION_NONE = 0;
        /// <summary> Define the maximum char</summary>
        public const int ADL_MAX_CHAR = 4096;
        /// <summary> Define the maximum path</summary>
        public const int ADL_MAX_PATH = 256;
        /// <summary> Define the maximum adapters</summary>
        public const int ADL_MAX_ADAPTERS = 250;
        /// <summary> Define the maximum displays</summary>
        public const int ADL_MAX_DISPLAYS = 150;
        /// <summary> Define the maximum device name length</summary>
        public const int ADL_MAX_DEVICENAME = 32;
        /// <summary> Define the maximum EDID Data length</summary>
        public const int ADL_MAX_EDIDDATA_SIZE = 256; // number of UCHAR
        /// <summary> Define the maximum display names</summary>
        public const int ADL_MAX_DISPLAY_NAME = 256;




        /// <summary> Define the driver ok</summary>
        public const int ADL_DRIVER_OK = 0;
        /// <summary> Maximum number of GL-Sync ports on the GL-Sync module </summary>
        public const int ADL_MAX_GLSYNC_PORTS = 8;
        /// <summary> Maximum number of GL-Sync ports on the GL-Sync module </summary>
        public const int ADL_MAX_GLSYNC_PORT_LEDS = 8;
        /// <summary> Maximum number of ADLModes for the adapter </summary>
        public const int ADL_MAX_NUM_DISPLAYMODES = 1024;
        /// <summary> Indicates the active dongle, all types </summary>
        public const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE = 12;
        /// <summary> Indicates the Active dongle DP->DVI(double link) connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_DVI_DL = 6;
        /// <summary> Indicates the Active dongle DP->DVI(single link) connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_DVI_SL = 5;
        /// <summary> Indicates the Active dongle DP->HDMI connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_HDMI = 7;
        /// <summary> Indicates the Active dongle DP->VGA connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_VGA = 8;
        /// <summary> Indicates the DISPLAY PORT connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_DISPLAY_PORT = 4;
        /// <summary> Indicates the DVI_I connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_DVI = 1;
        /// <summary> Indicates the DVI_SL connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_DVI_SL = 2;
        /// <summary> Indicates the HDMI connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_HDMI = 3;
        /// <summary> Indicates the MST type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_MST = 11;
        /// <summary> Indicates the Active dongle DP->VGA connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_PASSIVE_DONGLE_DP_DVI = 10;
        /// <summary> Indicates the Passive dongle DP->HDMI connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_PASSIVE_DONGLE_DP_HDMI = 9;
        /// <summary> Indicates the VGA connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_VGA = 0;
        /// <summary> Indicates the Virtual Connection Type.</summary>
        public const int ADL_CONNECTION_TYPE_VIRTUAL = 13;
        /// <summary> Indicates Active Dongle-JP Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_ATICVDONGLE_JP = 5;
        /// <summary> Indicates Active Dongle-NA Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_ATICVDONGLE_NA = 4;
        /// <summary> Indicates Active Dongle-NONI2C Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_ATICVDONGLE_NONI2C = 6;
        /// <summary> Indicates Active Dongle-NONI2C-D Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_ATICVDONGLE_NONI2C_D = 7;
        /// <summary> Indicates Display port Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_DISPLAYPORT = 10;
        /// <summary> Indicates DVI-D Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_DVI_D = 2;
        /// <summary> Indicates DVI-I Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_DVI_I = 3;
        /// <summary> Indicates EDP Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_EDP = 11;
        /// <summary> Indicates HDMI-Type A Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_HDMI_TYPE_A = 8;
        /// <summary> Indicates HDMI-Type B Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_HDMI_TYPE_B = 9;
        /// <summary> Indicates MiniDP Connector type. </summary>
        public const int ADL_CONNECTOR_TYPE_MINI_DISPLAYPORT = 12;
        /// <summary> Indicates Unknown Connector type. </summary>
        public const int ADL_CONNECTOR_TYPE_UNKNOWN = 0;
        /// <summary> Indicates USB type C Connector type. </summary>
        public const int ADL_CONNECTOR_TYPE_USB_TYPE_C = 14;
        /// <summary> Indicates VGA Connector type.  </summary>
        public const int ADL_CONNECTOR_TYPE_VGA = 1;
        /// <summary> Indicates Virtual Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_VIRTUAL = 13;

        // ADL Display Connector Types
        /// <summary> Indicates Unknown Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_UNKNOWN = 0;
        /// <summary> Indicates VGA Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_VGA = 1;
        /// <summary> Indicates DVI-D Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_DVI_D = 2;
        /// <summary> Indicates DVI-I Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_DVI_I = 3;
        /// <summary> Indicates ATICV NTSC Dongle Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_ATICVDONGLE_NTSC = 4;
        /// <summary> Indicates ATICV Japanese Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_ATICVDONGLE_JPN = 5;
        /// <summary> Indicates ATICV non-I2C Japanese Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_ATICVDONGLE_NONI2C_JPN = 6;
        /// <summary> Indicates ATICV non-I2C NTSC Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_ATICVDONGLE_NONI2C_NTSC = 7;
        /// <summary> Indicates Proprietary Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_PROPRIETARY = 8;
        /// <summary> Indicates HDMI Type A Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_HDMI_TYPE_A = 10;
        /// <summary> Indicates HDMI Type B Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_HDMI_TYPE_B = 11;
        /// <summary> Indicates S-Video Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_SVIDEO = 12;
        /// <summary> Indicates Composite Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_COMPOSITE = 13;
        /// <summary> Indicates RCA 3-component Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_RCA_3COMPONENT = 14;
        /// <summary> Indicates DisplayPort Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_DISPLAYPORT = 15;
        /// <summary> Indicates EDP Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_EDP = 16;
        /// <summary> Indicates Wireless Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_WIRELESSDISPLAY = 17;
        /// <summary> Indicates USB Type-C Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_USB_TYPE_C = 18;

        // Display Info Constants
        /// <summary> Indicates the display is connected .</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_DISPLAYCONNECTED = 0x00000001;
        /// <summary> Indicates the display is mapped within OS </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_DISPLAYMAPPED = 0x00000002;
        /// <summary> Indicates the display can be forced </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_FORCIBLESUPPORTED = 0x00000008;
        /// <summary> Indicates the display supports genlock </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_GENLOCKSUPPORTED = 0x00000010;
        /// <summary> Indicates the display is an LDA display.</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_LDA_DISPLAY = 0x00000040;
        /// <summary> Indicates the display supports 2x Horizontal stretch </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_2HSTRETCH = 0x00000800;
        /// <summary> Indicates the display supports 2x Vertical stretch </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_2VSTRETCH = 0x00000400;
        /// <summary> Indicates the display supports cloned desktops </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_CLONE = 0x00000200;
        /// <summary> Indicates the display supports extended desktops </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_EXTENDED = 0x00001000;
        /// <summary> Indicates the display supports N Stretched on 1 GPU</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_NSTRETCH1GPU = 0x00010000;
        /// <summary> Indicates the display supports N Stretched on N GPUs</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_NSTRETCHNGPU = 0x00020000;
        /// <summary> Reserved display info flag #2</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_RESERVED2 = 0x00040000;
        /// <summary> Reserved display info flag #3</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_RESERVED3 = 0x00080000;
        /// <summary> Indicates the display supports single desktop </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_SINGLE = 0x00000100;
        /// <summary> Indicates the display supports overriding the mode timing </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MODETIMING_OVERRIDESSUPPORTED = 0x00000080;
        /// <summary> Indicates the display supports multi-vpu</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MULTIVPU_SUPPORTED = 0x00000020;
        /// <summary> Indicates the display is non-local to this machine </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_NONLOCAL = 0x00000004;
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_SHOWTYPE_PROJECTOR = 0x00100000;

        // Display Mode Constants
        /// <summary> Indicates the display is in interlaced mode</summary>
        public const int ADL_DISPLAY_MODE_INTERLACED_FLAG = 2;
        /// <summary> Indicates the display is in progressive mode </summary>
        public const int ADL_DISPLAY_MODE_PROGRESSIVE_FLAG = 0;
        /// <summary> Indicates the display colour format is 565</summary>
        public const int ADL_DISPLAY_MODE_COLOURFORMAT_565 = 0x00000001;
        /// <summary> Indicates the display colour format is 8888 </summary>
        public const int ADL_DISPLAY_MODE_COLOURFORMAT_8888 = 0x00000002; // 
        /// <summary> Indicates the display orientation is normal position</summary>
        public const int ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_000 = 0x00000004;
        /// <summary> Indicates the display is in the 90 degree position</summary>
        public const int ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_090 = 0x00000008;
        /// <summary> Indicates the display in the 180 degree position</summary>
        public const int ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_180 = 0x00000010;
        /// <summary> Indicates the display is in the 270 degree position</summary>
        public const int ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_270 = 0x00000020;
        /// <summary> Indicates the display refresh rate is exact </summary>
        public const int ADL_DISPLAY_MODE_REFRESHRATE_ONLY = 0x00000080;
        /// <summary> Indicates the display refresh rate is rounded</summary>
        public const int ADL_DISPLAY_MODE_REFRESHRATE_ROUNDED = 0x00000040;

        // DDCInfoX2 DDCInfo Flag values
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_PROJECTORDEVICE = (1 << 0);
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_EDIDEXTENSION = (1 << 1);
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_DIGITALDEVICE = (1 << 2);
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_HDMIAUDIODEVICE = (1 << 3);
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_SUPPORTS_AI = (1 << 4);
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_SUPPORT_xvYCC601 = (1 << 5);
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_SUPPORT_xvYCC709 = (1 << 6);


        // HDR Constants
        /// <summary> HDR10/CEA861.3 HDR supported</summary>
        public const int ADL_HDR_CEA861_3 = 0x0001;
        /// <summary> DolbyVision HDR supported</summary>
        public const int ADL_HDR_DOLBYVISION = 0x0002;
        /// <summary> FreeSync HDR supported.</summary>
        public const int ADL_HDR_FREESYNC_HDR = 0x0004;

        // DisplayMap constants

        // ADL_DISPLAY_DISPLAYMAP_MANNER_ Definitions
        // for ADLDisplayMap.iDisplayMapMask and ADLDisplayMap.iDisplayMapValue
        // (bit-vector)
        /// <summary> Indicates the display map manner is reserved</summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_RESERVED = 0x00000001;
        /// <summary> Indicates the display map manner is not active </summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_NOTACTIVE = 0x00000002;
        /// <summary> Indicates the display map manner is single screens </summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_SINGLE = 0x00000004;
        /// <summary> Indicates the display map manner is clone of another display </summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_CLONE = 0x00000008;
        /// <summary> Indicates the display map manner is reserved</summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_RESERVED1 = 0x00000010;  // Removed NSTRETCH
        /// <summary> Indicates the display map manner is horizontal stretch </summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_HSTRETCH = 0x00000020;
        /// <summary> Indicates the display map manner is vertical stretch </summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_VSTRETCH = 0x00000040;
        /// <summary> Indicates the display map manner is VLD </summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_VLD = 0x00000080;


        // ADL_DISPLAY_DISPLAYMAP_OPTION_ Definitions
        // for iOption in function ADL_Display_DisplayMapConfig_Get
        // (bit-vector)
        /// <summary> Indicates the display map option is GPU Info</summary>
        public const int ADL_DISPLAY_DISPLAYMAP_OPTION_GPUINFO = 0x00000001;

        // ADL_DISPLAY_DISPLAYTARGET_ Definitions
        // for ADLDisplayTarget.iDisplayTargetMask and ADLDisplayTarget.iDisplayTargetValue
        // (bit-vector)
        /// <summary> Indicates the display target is preferred </summary>
        public const int ADL_DISPLAY_DISPLAYTARGET_PREFERRED = 0x00000001;

        // ADL_DISPLAY_POSSIBLEMAPRESULT_VALID Definitions
        // for ADLPossibleMapResult.iPossibleMapResultMask and ADLPossibleMapResult.iPossibleMapResultValue
        // (bit-vector)
        /// <summary> Indicates the display map result is valid</summary>
        public const int ADL_DISPLAY_POSSIBLEMAPRESULT_VALID = 0x00000001;
        /// <summary> Indicates the display map result supports bezels</summary>
        public const int ADL_DISPLAY_POSSIBLEMAPRESULT_BEZELSUPPORTED = 0x00000002;
        /// <summary> Indicates the display map result supports overlap</summary>
        public const int ADL_DISPLAY_POSSIBLEMAPRESULT_OVERLAPSUPPORTED = 0x00000004;


        //#define ADL_DISPLAY_SLSMAPINDEXLIST_OPTION_ACTIVE        0x00000001
        public const int ADL_DISPLAY_SLSMAPINDEXLIST_OPTION_ACTIVE = 0x00000001;


        // ADL_DISPLAY_SLSGRID_CAP_OPTION_RELATIVETO_LANDSCAPE Definitions
        // for ADL_SLS_MAP.iOption
        // (bit-vector)
        // Bit vector, specifies the layout type of SLS grid data and portrait flag.
        // There are two types of SLS layouts: relative to landscape (ref \ADL_DISPLAY_SLSGRID_CAP_OPTION_RELATIVETO_LANDSCAPE) and relative to current angle(ref \ADL_DISPLAY_SLSGRID_CAP_OPTION_RELATIVETO_CURRENTANGLE).
        // If the current desktop associated with the input adapter is rotated to 90 or 270 degree, set bit ref \ADL_DISPLAY_SLSGRID_PORTAIT_MODE to 1
        public const int ADL_DISPLAY_SLSGRID_CAP_OPTION_RELATIVETO_LANDSCAPE = 0x00000001;
        public const int ADL_DISPLAY_SLSGRID_CAP_OPTION_RELATIVETO_CURRENTANGLE = 0x00000002;
        public const int ADL_DISPLAY_SLSGRID_PORTAIT_MODE = 0x00000004;
        public const int ADL_DISPLAY_SLSGRID_KEEPTARGETROTATION = 0x00000080;


        public const int ADL_DISPLAY_SLSMAPCONFIG_GET_OPTION_RELATIVETO_LANDSCAPE = 0x00000001;
        public const int ADL_DISPLAY_SLSMAPCONFIG_GET_OPTION_RELATIVETO_CURRENTANGLE = 0x00000002;

        public const int ADL_DISPLAY_SLSMAPCONFIG_CREATE_OPTION_RELATIVETO_LANDSCAPE = 0x00000001;
        public const int ADL_DISPLAY_SLSMAPCONFIG_CREATE_OPTION_RELATIVETO_CURRENTANGLE = 0x00000002;

        public const int ADL_DISPLAY_SLSMAPCONFIG_REARRANGE_OPTION_RELATIVETO_LANDSCAPE = 0x00000001;
        public const int ADL_DISPLAY_SLSMAPCONFIG_REARRANGE_OPTION_RELATIVETO_CURRENTANGLE = 0x00000002;


        public const int ADL_DISPLAY_SLSMAP_SLSLAYOUTMODE_FIT = 0x0100;
        public const int ADL_DISPLAY_SLSMAP_SLSLAYOUTMODE_FILL = 0x0200;
        public const int ADL_DISPLAY_SLSMAP_SLSLAYOUTMODE_EXPAND = 0x0400;

        public const int ADL_DISPLAY_SLSMAP_IS_SLS = 0x1000;
        public const int ADL_DISPLAY_SLSMAP_IS_SLSBUILDER = 0x2000;
        public const int ADL_DISPLAY_SLSMAP_IS_CLONEVT = 0x4000;


        #region Internal Constant
        /// <summary> Atiadlxx_FileName </summary>
        public const string ATI_ADL_DLL = "atiadlxx.dll";
        /// <summary> Kernel32_FileName </summary>
        public const string Kernel32_FileName = "kernel32.dll";
        #endregion Internal Constant

        #region DLLImport
        [DllImport(Kernel32_FileName)]
        public static extern HMODULE GetModuleHandle(string moduleName);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Main_Control_Create(ADL_Main_Memory_Alloc_Delegate callback, int enumConnectedAdapters, out IntPtr contextHandle);

        //typedef int (* ADL2_MAIN_CONTROLX2_CREATE) (ADL_MAIN_MALLOC_CALLBACK, int iEnumConnectedAdapter_, ADL_CONTEXT_HANDLE* context_, ADLThreadingModel);

        //typedef int (* ADL2_MAIN_CONTROL_DESTROY) (ADL_CONTEXT_HANDLE);
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Main_Control_Destroy(IntPtr contextHandle);

        // This is used to set the display settings permanently
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Flush_Driver_Data(IntPtr ADLContextHandle, int adapterIndex);


        // adapter functions
        //typedef int (* ADL2_DISPLAY_POSSIBLEMODE_GET) (ADL_CONTEXT_HANDLE, int, int*, ADLMode**);
        // This function retrieves the OS possible modes list for a specified input adapter.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_PossibleMode_Get(IntPtr ADLContextHandle, int adapterIndex, out int numModes, out IntPtr modes);

        //typedef int (* ADL2_ADAPTER_PRIMARY_SET) (ADL_CONTEXT_HANDLE, int);
        // This function sets the adapter index for a specified primary display adapter.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Adapter_Primary_Set(IntPtr ADLContextHandle, int primaryAdapterIndex);

        //typedef int (* ADL2_ADAPTER_PRIMARY_GET) (ADL_CONTEXT_HANDLE, int*);
        // This function retrieves the adapter index for the primary display adapter.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Adapter_Primary_Get(IntPtr ADLContextHandle, out int primaryAdapterIndex);

        //typedef int (* ADL2_ADAPTER_ACTIVE_SET) (ADL_CONTEXT_HANDLE, int, int, int*);
        // This function enables or disables extended desktop mode for a specified display.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Adapter_Active_Set(IntPtr ADLContextHandle, int primaryAdapterIndex, int desiredStatus, out int newlyActivated);


        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Adapter_NumberOfAdapters_Get(IntPtr contextHandle, out int numAdapters);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Adapter_Active_Get(IntPtr ADLContextHandle, int adapterIndex, out int status);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_AdapterX2_Caps(IntPtr ADLContextHandle, int adapterIndex, out ADL_ADAPTER_CAPSX2 adapterCapabilities);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Adapter_AdapterInfo_Get(IntPtr ADLContextHandle, int inputSize, out IntPtr AdapterInfoArray);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Adapter_AdapterInfoX2_Get(IntPtr ADLContextHandle, out IntPtr AdapterInfoArray);

        //typedef int (* ADL2_ADAPTER_ADAPTERINFOX3_GET) (ADL_CONTEXT_HANDLE context, int iAdapterIndex, int* numAdapters, AdapterInfo** lppAdapterInfo);
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Adapter_AdapterInfoX3_Get(IntPtr ADLContextHandle, int adapterIndex, out int numAdapters, out IntPtr AdapterInfoArray);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Adapter_AdapterInfoX4_Get(IntPtr ADLContextHandle, int adapterIndex, out int numAdapters, out IntPtr AdapterInfoX2Array);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_DDCInfo2_Get(IntPtr contextHandle, int adapterIndex, int displayIndex, out ADL_DDC_INFO2 DDCInfo);

        //typedef int (* ADL2_DISPLAY_DISPLAYINFO_GET) (ADL_CONTEXT_HANDLE context, int iAdapterIndex, int* lpNumDisplays, ADLDisplayInfo** lppInfo, int iForceDetect);
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_DisplayInfo_Get(IntPtr ADLContextHandle, int adapterIndex, out int numDisplays, out IntPtr displayInfoArray, int forceDetect);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_DeviceConfig_Get(IntPtr ADLContextHandle, int adapterIndex, int displayIndex, out ADL_DISPLAY_CONFIG displayConfig);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_HDRState_Get(IntPtr ADLContextHandle, int adapterIndex, ADL_DISPLAY_ID displayID, out int support, out int enable);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_HDRState_Set(IntPtr ADLContextHandle, int adapterIndex, ADL_DISPLAY_ID displayID, int enable);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_ColorDepth_Get(IntPtr ADLContextHandle, int adapterIndex, ADL_DISPLAY_ID displayID, out ADL_COLORDEPTH colourDepth);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_DisplayMapConfig_PossibleAddAndRemove(IntPtr ADLContextHandle, int adapterIndex, int numDisplayMap, in ADL_DISPLAY_MAP displayMap, int numDisplayTarget, in ADL_DISPLAY_TARGET displayTarget, out int numPossibleAddTarget, out IntPtr possibleAddTarget, out int numPossibleRemoveTarget, out IntPtr possibleRemoveTarget);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Adapter_Desktop_Caps(IntPtr ADLContextHandle, int adapterIndex, out int DesktopCapsValue, out int DesktopCapsMask);

        // Function to retrieve active desktop supported SLS grid size.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Adapter_Desktop_SupportedSLSGridTypes_Get(IntPtr ADLContextHandle, int adapterIndex, int numDisplayTargetToUse, ref ADL_DISPLAY_TARGET displayTargetToUse, out int numSLSGrid, out ADL_DISPLAY_TARGET SLSGrid, out int option);

        // Function to get the current supported SLS grid patterns (MxN) for a GPU.
        // This function gets a list of supported SLS grids for a specified input adapter based on display devices currently connected to the GPU.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_SLSGrid_Caps(IntPtr ADLContextHandle, int adapterIndex, out int NumSLSGrid, out IntPtr SLSGrid, int option);

        // Function to get the active SLS map index list for a given GPU.
        // This function retrieves a list of active SLS map indexes for a specified input GPU.            
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_SLSMapIndexList_Get(IntPtr ADLContextHandle, int adapterIndex, out int numSLSMapIndexList, out IntPtr SLSMapIndexList, int option);

        // Definitions of the used function pointers. Add more if you use other ADL APIs
        // SLS functions
        //typedef int (* ADL2_DISPLAY_SLSMAPCONFIG_VALID) (ADL_CONTEXT_HANDLE context, int iAdapterIndex, ADLSLSMap slsMap, int iNumDisplayTarget, ADLSLSTarget* lpSLSTarget, int* lpSupportedSLSLayoutImageMode, int* lpReasonForNotSupportSLS, int iOption);
        // Function to Set SLS configuration for each display index the controller and the adapter is being mapped to.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_SLSMapConfig_Valid(IntPtr ADLContextHandle, int adapterIndex, ADL_SLS_MAP SLSMap, int numDisplayTarget, ADL_SLS_TARGET[] displayTarget, out int supportedSLSLayoutImageMode, out int reasonForNotSupportingSLS, int option);

        //typedef int (* ADL2_DISPLAY_SLSMAPINDEX_GET) (ADL_CONTEXT_HANDLE, int, int, ADLDisplayTarget*, int*);
        // Function to get a SLS map index based on a group of displays that are connected in the given adapter. The driver only searches the active SLS grid database.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_SLSMapIndex_Get(IntPtr ADLContextHandle, int adapterIndex, int numDisplayTarget, ADL_DISPLAY_TARGET[] displayTarget, out int SLSMapIndex);

        //typedef int (* ADL2_DISPLAY_SLSMAPCONFIG_GET) (ADL_CONTEXT_HANDLE, int, int, ADLSLSMap*, int*, ADLSLSTarget**, int*, ADLSLSMode**, int*, ADLBezelTransientMode**, int*, ADLBezelTransientMode**, int*, ADLSLSOffset**, int);
        // This function retrieves an SLS configuration, which includes the, SLS map, SLS targets, SLS standard modes, bezel modes or a transient mode, and offsets.           
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_SLSMapConfig_Get(IntPtr ADLContextHandle, int adapterIndex, int SLSMapIndex, out ADL_SLS_MAP slsMap, out int NumSLSTarget, out IntPtr SLSTargetArrayBuffer, out int lpNumNativeMode, out IntPtr NativeModeBuffer, out int NumBezelMode, out IntPtr BezelModeBuffer, out int NumTransientMode, out IntPtr TransientModeBuffer, out int NumSLSOffset, out IntPtr SLSOffsetBuffer, int iOption);

        // typedef int ADL2_Display_SLSMapConfigX2_Get(ADL_CONTEXT_HANDLE context, int iAdapterIndex, int iSLSMapIndex, ADLSLSMap* lpSLSMap, int* lpNumSLSTarget, ADLSLSTarget** lppSLSTarget, int* lpNumNativeMode, ADLSLSMode** lppNativeMode, int* lpNumNativeModeOffsets, ADLSLSOffset** lppNativeModeOffsets, int* lpNumBezelMode, ADLBezelTransientMode** lppBezelMode, int* lpNumTransientMode, ADLBezelTransientMode** lppTransientMode, int* lpNumSLSOffset, ADLSLSOffset** lppSLSOffset, int iOption)
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_SLSMapConfigX2_Get(IntPtr ADLContextHandle, int adapterIndex, int SLSMapIndex, ref ADL_SLS_MAP slsMap, out int NumSLSTarget, out IntPtr SLSTargetBuffer, out int lpNumNativeMode, out IntPtr NativeModeBuffer, out int NumNativeModeOffsets, out IntPtr NativeModeOffsetsBuffer, out int NumBezelMode, out IntPtr BezelModeBuffer, out int NumTransientMode, out IntPtr TransientModeBuffer, out int NumSLSOffset, out IntPtr SLSOffsetBuffer, int option);

        //typedef int (* ADL2_DISPLAY_SLSMAPCONFIG_DELETE) (ADL_CONTEXT_HANDLE context, int iAdapterIndex, int iSLSMapIndex);
        // This function deletes an SLS map from the driver database based on the input SLS map index.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Display_SLSMapConfig_Delete(IntPtr ADLContextHandle, int adapterIndex, int SLSMapIndex);


        //typedef int (* ADL2_DISPLAY_SLSMAPCONFIG_CREATE) (ADL_CONTEXT_HANDLE context, int iAdapterIndex, ADLSLSMap SLSMap, int iNumTarget, ADLSLSTarget* lpSLSTarget, int iBezelModePercent, int* lpSLSMapIndex, int iOption);
        // This function creates an SLS configuration with a given grid, targets, and bezel mode percent. It will output an SLS map index if the SLS map is successfully created.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_SLSMapConfig_Create(IntPtr ADLContextHandle, int adapterIndex, ADL_SLS_MAP SLSMap, int numDisplayTarget, ADL_SLS_TARGET[] displayTarget, int bezelModePercent, out int SLSMapIndex, int option);

        //typedef int (* ADL2_DISPLAY_SLSMAPCONFIG_REARRANGE) (ADL_CONTEXT_HANDLE, int, int, int, ADLSLSTarget*, ADLSLSMap, int);
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_SLSMapConfig_Rearrange(IntPtr ADLContextHandle, int adapterIndex, int SLSMapIndex, int numDisplayTarget, ADL_SLS_TARGET[] displayTargets, ADL_SLS_MAP SLSMap, int option);

        //typedef int (* ADL2_DISPLAY_SLSMAPCONFIG_SETSTATE) (ADL_CONTEXT_HANDLE, int, int, int);
        // This is used to set the SLS Grid we want from the SLSMap by selecting the one we want and supplying that as an index.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_SLSMapConfig_SetState(IntPtr ADLContextHandle, int adapterIndex, int SLSMapIndex, int State);


        //typedef int 	ADL2_Display_SLSRecords_Get (ADL_CONTEXT_HANDLE context, int iAdapterIndex, ADLDisplayID displayID, int *lpNumOfRecords, int **lppGridIDList)
        // This is used to set the SLS Grid we want from the SLSMap by selecting the one we want and supplying that as an index.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_SLSRecords_Get(IntPtr ADLContextHandle, int adapterIndex, ADL_DISPLAY_ID displayID, out int numOfRecords, out IntPtr gridIDList);

        //typedef int (* ADL2_DISPLAY_SLSMAPINDEXLIST_GET) (ADL_CONTEXT_HANDLE, int, int*, int**, int);
        // This function retrieves a list of active SLS map indexes for a specified input GPU.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_SLSMapIndexList_Get(IntPtr ADLContextHandle, int AdapterIndex, out int numSLSMapIndexList, IntPtr SLSMapIndexList, int option);

        //typedef int (* ADL2_DISPLAY_MODES_GET) (ADL_CONTEXT_HANDLE, int, int, int*, ADLMode**);
        // This function retrieves the current display mode information.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_Modes_Get(IntPtr ADLContextHandle, int adapterIndex, int displayIndex, out int numModes, out IntPtr modes);


        //typedef int (* ADL2_DISPLAY_MODES_SET) (ADL_CONTEXT_HANDLE, int, int, int, ADLMode*);
        // This function sets the current display mode information.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_Modes_Set(IntPtr ADLContextHandle, int adapterIndex, int displayIndex, int numModes, ref ADL_MODE modes);

        //typedef int (* ADL2_DISPLAY_BEZELOFFSET_SET) (ADL_CONTEXT_HANDLE, int, int, int, LPADLSLSOffset, ADLSLSMap, int);
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_BezelOffset_Set(IntPtr ADLContextHandle, int adapterIndex, int SLSMapIndex, int numBezelOffset, ref ADL_SLS_OFFSET displayTargetToUse, ADL_SLS_MAP SLSMap, int option);

        //display map functions
        //typedef int (* ADL2_DISPLAY_DISPLAYMAPCONFIG_GET) (ADL_CONTEXT_HANDLE context, int iAdapterIndex, int* lpNumDisplayMap, ADLDisplayMap** lppDisplayMap, int* lpNumDisplayTarget, ADLDisplayTarget** lppDisplayTarget, int iOptions);
        // This function retrieves the current display map configurations, including the controllers and adapters mapped to each display.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_DisplayMapConfig_Get(IntPtr ADLContextHandle, int adapterIndex, out int numDisplayMap, out IntPtr displayMap, out int numDisplayTarget, out IntPtr displayTarget, int options);

        //typedef int (* ADL2_DISPLAY_DISPLAYMAPCONFIG_SET) (ADL_CONTEXT_HANDLE, int, int, ADLDisplayMap*, int, ADLDisplayTarget*);
        // This function sets the current display configurations for each display, including the controller and adapter mapped to each display.
        // Possible display configurations are single, clone, extended desktop, and stretch mode.
        // If clone mode is desired and the current display configuration is extended desktop mode, the function disables extended desktop mode in order to enable clone mode.
        // If extended display mode is desired and the current display configuration is single mode, this function enables extended desktop.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_DisplayMapConfig_Set(IntPtr ADLContextHandle, int adapterIndex, int numDisplayMap, ADL_DISPLAY_MAP[] displayMap, int numDisplayTarget, ADL_DISPLAY_TARGET[] displayTarget);

        // This function validate the list of the display configurations for a specified input adapter. This function is applicable to all OS platforms.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_DisplayMapConfig_Validate(IntPtr ADLContextHandle, int adapterIndex, int numPossibleMap, ref ADL_POSSIBLE_MAP possibleMaps, out int numPossibleMapResult, out IntPtr possibleMapResult);

        // Function to indicate whether displays are physically connected to an adapter.
        // This function indicates whether displays are physically connected to a specified adapter.        
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL2_Display_ConnectedDisplays_Get(IntPtr context, int adapterIndex, out int connections);


        // ======================================


        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Main_Control_Create(ADL_Main_Memory_Alloc_Delegate callback, int enumConnectedAdapters);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Main_Control_Destroy();


        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Main_Control_IsFunctionValid(HMODULE module, string procName);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern FARPROC ADL_Main_Control_GetProcAddress(HMODULE module, string procName);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Adapter_NumberOfAdapters_Get(ref int numAdapters);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Adapter_AdapterInfo_Get(out IntPtr info, int inputSize);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Adapter_Active_Get(int adapterIndex, ref int status);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Adapter_ID_Get(int adapterIndex, ref int adapterId);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_AdapterX2_Caps(int adapterIndex, out ADL_ADAPTER_CAPSX2 adapterCapabilities);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Display_DisplayInfo_Get(int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Display_DeviceConfig_Get(int adapterIndex, int displayIndex, out ADL_DISPLAY_CONFIG displayConfig);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Display_EdidData_Get(int adapterIndex, int displayIndex, ref ADL_DISPLAY_EDID_DATA EDIDData);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Display_DisplayMapConfig_Get(int adapterIndex, out int numDisplayMap, out IntPtr displayMap, out int numDisplayTarget, out IntPtr displayTarget, int options);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Display_DisplayMapConfig_PossibleAddAndRemove(int adapterIndex, int numDisplayMap, ADL_DISPLAY_MAP displayMap, int numDisplayTarget, ADL_DISPLAY_TARGET displayTarget, out int numPossibleAddTarget, out IntPtr possibleAddTarget, out int numPossibleRemoveTarget, out IntPtr possibleRemoveTarget);

        //typedef int (* ADL2_DISPLAY_SLSMAPCONFIG_GET) (ADL_CONTEXT_HANDLE, int, int, ADLSLSMap*, int*, ADLSLSTarget**, int*, ADLSLSMode**, int*, ADLBezelTransientMode**, int*, ADLBezelTransientMode**, int*, ADLSLSOffset**, int);
        // This function retrieves an SLS configuration, which includes the, SLS map, SLS targets, SLS standard modes, bezel modes or a transient mode, and offsets.           
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Display_SLSMapConfig_Get(int adapterIndex, int SLSMapIndex, out ADL_SLS_MAP SLSMap, out int NumSLSTarget, out IntPtr SLSTargetArray, out int lpNumNativeMode, out IntPtr NativeMode, out int NumBezelMode, out IntPtr BezelMode, out int NumTransientMode, out IntPtr TransientMode, out int NumSLSOffset, out IntPtr SLSOffset, int iOption);

        // typedef int ADL2_Display_SLSMapConfigX2_Get(ADL_CONTEXT_HANDLE context, int iAdapterIndex, int iSLSMapIndex, ADLSLSMap* lpSLSMap, int* lpNumSLSTarget, ADLSLSTarget** lppSLSTarget, int* lpNumNativeMode, ADLSLSMode** lppNativeMode, int* lpNumNativeModeOffsets, ADLSLSOffset** lppNativeModeOffsets, int* lpNumBezelMode, ADLBezelTransientMode** lppBezelMode, int* lpNumTransientMode, ADLBezelTransientMode** lppTransientMode, int* lpNumSLSOffset, ADLSLSOffset** lppSLSOffset, int iOption)
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Display_SLSMapConfigX2_Get(int adapterIndex, int SLSMapIndex, out ADL_SLS_MAP SLSMap, out int NumSLSTarget, out IntPtr SLSTargetArray, out int lpNumNativeMode, out IntPtr NativeMode, out int NumNativeModeOffsets, out IntPtr NativeModeOffsets, out int NumBezelMode, out IntPtr BezelMode, out int NumTransientMode, out IntPtr TransientMode, out int NumSLSOffset, out IntPtr SLSOffset, int option);

        // This is used to set the SLS Grid we want from the SLSMap by selecting the one we want and supplying that as an index.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Display_SLSMapConfig_SetState(int AdapterIndex, int SLSMapIndex, int State);

        // Function to get the current supported SLS grid patterns (MxN) for a GPU.
        // This function gets a list of supported SLS grids for a specified input adapter based on display devices currently connected to the GPU.
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Display_SLSGrid_Caps(int adapterIndex, ref int NumSLSGrid, out IntPtr SLSGrid, int option);

        // Function to get the active SLS map index list for a given GPU.
        // This function retrieves a list of active SLS map indexes for a specified input GPU.            
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Display_SLSMapIndexList_Get(int adapterIndex, out int numSLSMapIndexList, out IntPtr SLSMapIndexList, int options);

        // Function to get the active SLS map index list for a given GPU.
        // This function retrieves a list of active SLS map indexes for a specified input GPU.            
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ADL_STATUS ADL_Display_SLSMapIndex_Get(int adapterIndex, int ADLNumDisplayTarget, ref ADL_DISPLAY_TARGET displayTarget, ref int SLSMapIndex);

        #endregion DLLImport

        public static ADL_Main_Memory_Alloc_Delegate ADL_Main_Memory_Alloc = ADL_Main_Memory_Alloc_Function;
        /// <summary> Build in memory allocation function</summary>
        /// <param name="size">input size</param>
        /// <returns>return the memory buffer</returns>
        public static IntPtr ADL_Main_Memory_Alloc_Function(int size)
        {
            //Console.WriteLine($"\nCallback called with param: {size}");
            IntPtr result = Marshal.AllocCoTaskMem(size);
            return result;
        }

    }
}