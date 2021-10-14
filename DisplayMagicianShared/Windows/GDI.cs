using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagicianShared.Windows
{
    // 90% of this file is cribbed from WindowsDisplayAPI by Soroush Falahati
    // The other 10% is from MikedouglasDev's ChangeScreenResolution
    // https://github.com/mikedouglasdev/changescreenresolution/blob/master/ChangeScreenResolutionSolution/ChangeScreenResolution/SafeNativeMethods.cs
    // and GemingLeader here: https://www.c-sharpcorner.com/uploadfile/GemingLeader/changing-display-settings-programmatically/


    public enum CHANGE_DISPLAY_RESULTS
    {
        /// <summary>
        ///     Completed successfully
        /// </summary>
        Successful = 0,

        /// <summary>
        ///     Changes needs restart
        /// </summary>
        Restart = 1,

        /// <summary>
        ///     Failed to change and save setings
        /// </summary>
        Failed = -1,

        /// <summary>
        ///     Invalid data provided
        /// </summary>
        BadMode = -2,

        /// <summary>
        ///     Changes not updated
        /// </summary>
        NotUpdated = -3,

        /// <summary>
        ///     Invalid flags provided
        /// </summary>
        BadFlags = -4,

        /// <summary>
        ///     Bad parameters provided
        /// </summary>
        BadParam = -5,

        /// <summary>
        ///     Bad Dual View mode used with mode
        /// </summary>
        BadDualView = -6
    }

    [Flags]
    public enum CHANGE_DISPLAY_SETTINGS_FLAGS : UInt32
    {
        CDS_NONE = 0,
        CDS_UPDATEREGISTRY = 0x00000001,
        CDS_TEST = 0x00000002,
        CDS_FULLSCREEN = 0x00000004,
        CDS_GLOBAL = 0x00000008,
        CDS_SET_PRIMARY = 0x00000010,
        CDS_VIDEOPARAMETERS = 0x00000020,
        CDS_ENABLE_UNSAFE_MODES = 0x00000100,
        CDS_DISABLE_UNSAFE_MODES = 0x00000200,
        CDS_RESET = 0x40000000,
        CDS_RESET_EX = 0x20000000,
        CDS_NORESET = 0x10000000
    }

    public enum DEVICE_CAPABILITY : Int32
    {
        DriverVersion = 0,
        Technology = 2,
        HorizontalSizeInMM = 4,
        VerticalSizeInMM = 6,
        HorizontalResolution = 8,
        VerticalResolution = 10,
        BitsPerPixel = 12,
        Planes = 14,
        NumberOfBrushes = 16,
        NumberOfPens = 18,
        NumberOfMarkers = 20,
        NumberOfFonts = 22,
        NumberOfColors = 24,
        DeviceDescriptorSize = 26,
        CurveCapabilities = 28,
        LineCapabilities = 30,
        PolygonalCapabilities = 32,
        TextCapabilities = 34,
        ClipCapabilities = 36,
        RasterCapabilities = 38,
        HorizontalAspect = 40,
        VerticalAspect = 42,
        HypotenuseAspect = 44,
        //ShadeBlendingCapabilities = 45,
        HorizontalLogicalPixels = 88,
        VerticalLogicalPixels = 90,
        PaletteSize = 104,
        ReservedPaletteSize = 106,
        ColorResolution = 108,

        // Printer Only
        PhysicalWidth = 110,
        PhysicalHeight = 111,
        PhysicalHorizontalMargin = 112,
        PhysicalVerticalMargin = 113,
        HorizontalScalingFactor = 114,
        VerticalScalingFactor = 115,

        // Display Only
        VerticalRefreshRateInHz = 116,
        DesktopVerticalResolution = 117,
        DesktopHorizontalResolution = 118,
        PreferredBLTAlignment = 119,
        ShadeBlendingCapabilities = 120,
        ColorManagementCapabilities = 121,
    }

    [Flags]
    public enum DEVICE_MODE_FIELDS : UInt32
    {
        None = 0,
        Position = 0x20,
        DisplayOrientation = 0x80,
        Color = 0x800,
        Duplex = 0x1000,
        YResolution = 0x2000,
        TtOption = 0x4000,
        Collate = 0x8000,
        FormName = 0x10000,
        LogPixels = 0x20000,
        BitsPerPixel = 0x40000,
        PelsWidth = 0x80000,
        PelsHeight = 0x100000,
        DisplayFlags = 0x200000,
        DisplayFrequency = 0x400000,
        DisplayFixedOutput = 0x20000000,
        AllDisplay = Position |
                     DisplayOrientation |
                     YResolution |
                     BitsPerPixel |
                     PelsWidth |
                     PelsHeight |
                     DisplayFlags |
                     DisplayFrequency |
                     DisplayFixedOutput,
    }

    [Flags]
    public enum DISPLAY_DEVICE_STATE_FLAGS : UInt32
    {
        /// <summary>
        ///     The device is part of the desktop.
        /// </summary>
        AttachedToDesktop = 0x1,
        MultiDriver = 0x2,

        /// <summary>
        ///     The device is part of the desktop.
        /// </summary>
        PrimaryDevice = 0x4,

        /// <summary>
        ///     Represents a pseudo device used to mirror application drawing for remoting or other purposes.
        /// </summary>
        MirroringDriver = 0x8,

        /// <summary>
        ///     The device is VGA compatible.
        /// </summary>
        VGACompatible = 0x10,

        /// <summary>
        ///     The device is removable; it cannot be the primary display.
        /// </summary>
        Removable = 0x20,

        /// <summary>
        ///     The device has more display modes than its output devices support.
        /// </summary>
        ModesPruned = 0x8000000,
        Remote = 0x4000000,
        Disconnect = 0x2000000
    }

    public enum DISPLAY_FIXED_OUTPUT : UInt32
    {
        /// <summary>
        ///     Default behavior
        /// </summary>
        Default = 0,

        /// <summary>
        ///     Stretches the output to fit to the display
        /// </summary>
        Stretch = 1,

        /// <summary>
        ///     Centers the output in the middle of the display
        /// </summary>
        Center = 2
    }

    [Flags]
    public enum DISPLAY_FLAGS : UInt32
    {
        None = 0,
        Grayscale = 1,
        Interlaced = 2
    }

    public enum DISPLAY_ORIENTATION : UInt32
    {
        /// <summary>
        ///     No rotation
        /// </summary>
        Rotate0Degree = 0,

        /// <summary>
        ///     90 degree rotation
        /// </summary>
        Rotate90Degree = 1,

        /// <summary>
        ///     180 degree rotation
        /// </summary>
        Rotate180Degree = 2,

        /// <summary>
        ///     270 degree rotation
        /// </summary>
        Rotate270Degree = 3
    }

    public enum DISPLAY_SETTINGS_MODE : Int32
    {
        CurrentSettings = -1, // Retrieves current display mode
        RegistrySettings = -2 // Retrieves current display mode stored within the registry.
    }

    public enum DISPLAY_TECHNOLOGY : Int32
    {
        Plotter = 0,
        RasterDisplay = 1,
        RasterPrinter = 2,
        RasterCamera = 3,
        CharacterStream = 4,
        MetaFile = 5,
        DisplayFile = 6,
    }

    public enum MONITOR_FROM_FLAG : UInt32
    {
        DefaultToNull = 0,
        DefaultToPrimary = 1,
        DefaultToNearest = 2,
    }

    [Flags]
    public enum MONITOR_INFO_FLAGS : UInt32
    {
        None = 0,
        Primary = 1
    }



    // https://msdn.microsoft.com/en-us/library/windows/desktop/dd183565(v=vs.85).aspx
    // https://www.c-sharpcorner.com/uploadfile/GemingLeader/changing-display-settings-programmatically/
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
    public struct DEVICE_MODE : IEquatable<DEVICE_MODE>
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        [FieldOffset(0)]
        public string DeviceName;

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(32)]
        public UInt16 SpecificationVersion;

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(34)]
        public UInt16 DriverVersion;

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(36)]
        public UInt16 Size;

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(38)]
        public UInt16 DriverExtra;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(40)]
        public DEVICE_MODE_FIELDS Fields;

        [MarshalAs(UnmanagedType.Struct)]
        [FieldOffset(44)]
        public POINTL Position;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(52)]
        public DISPLAY_ORIENTATION DisplayOrientation;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(56)]
        public DISPLAY_FIXED_OUTPUT DisplayFixedOutput;

        [MarshalAs(UnmanagedType.I2)]
        [FieldOffset(60)]
        public Int16 Color;

        [MarshalAs(UnmanagedType.I2)]
        [FieldOffset(62)]
        public Int16 Duplex;

        [MarshalAs(UnmanagedType.I2)]
        [FieldOffset(64)]
        public Int16 YResolution;

        [MarshalAs(UnmanagedType.I2)]
        [FieldOffset(66)]
        public Int16 TrueTypeOption;

        [MarshalAs(UnmanagedType.I2)]
        [FieldOffset(68)]
        public Int16 Collate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        [FieldOffset(72)]
        public string FormName;

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(102)]
        public UInt16 LogicalInchPixels;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(104)]
        public UInt32 BitsPerPixel;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(108)]
        public UInt32 PixelsWidth;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(112)]
        public UInt32 PixelsHeight;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(116)]
        public DISPLAY_FLAGS DisplayFlags;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(120)]
        public UInt32 DisplayFrequency;

        public override bool Equals(object obj) => obj is DEVICE_MODE other && this.Equals(other);

        public bool Equals(DEVICE_MODE other)
            => DeviceName.Equals(other.DeviceName) &&
                SpecificationVersion == other.SpecificationVersion &&
                DriverVersion.Equals(other.DriverVersion) &&
                Size.Equals(other.Size) &&
                DriverExtra.Equals(other.DriverExtra) &&
                Fields.Equals(other.Fields) &&
                Position.Equals(other.Position) &&
                DisplayOrientation.Equals(other.DisplayOrientation) &&
                DisplayFixedOutput.Equals(other.DisplayFixedOutput) &&
                Color.Equals(other.Color) &&
                Duplex.Equals(other.Duplex) &&
                YResolution.Equals(other.YResolution) &&
                TrueTypeOption.Equals(other.TrueTypeOption) &&
                Collate.Equals(other.Collate) &&
                FormName.Equals(other.FormName) &&
                LogicalInchPixels.Equals(other.LogicalInchPixels) &&
                BitsPerPixel.Equals(other.BitsPerPixel) &&
                PixelsWidth.Equals(other.PixelsWidth) &&
                PixelsHeight.Equals(other.PixelsHeight) &&
                DisplayFlags.Equals(other.DisplayFlags) &&
                DisplayFrequency == other.DisplayFrequency;

        public override int GetHashCode()
        {
            return (DeviceName, SpecificationVersion, DriverVersion, Size, DriverExtra, Fields, Position, DisplayOrientation, DisplayFixedOutput, Color, Duplex,
                YResolution, TrueTypeOption, Collate, FormName, LogicalInchPixels, BitsPerPixel, PixelsWidth, PixelsHeight, DisplayFlags, DisplayFrequency).GetHashCode();
        }

        public static bool operator ==(DEVICE_MODE lhs, DEVICE_MODE rhs) => lhs.Equals(rhs);

        public static bool operator !=(DEVICE_MODE lhs, DEVICE_MODE rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAY_DEVICE : IEquatable<DISPLAY_DEVICE>
    {
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 Size;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;

        [MarshalAs(UnmanagedType.U4)]
        public DISPLAY_DEVICE_STATE_FLAGS StateFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;

        public override bool Equals(object obj) => obj is DISPLAY_DEVICE other && this.Equals(other);

        public bool Equals(DISPLAY_DEVICE other)
            => Size == other.Size &&
                DeviceName == other.DeviceName &&
                DeviceString == other.DeviceString &&
                StateFlags == other.StateFlags &&
                DeviceId == other.DeviceId &&
                DeviceKey == other.DeviceKey;

        public override int GetHashCode()
        {
            return (Size, DeviceName, DeviceString, StateFlags, DeviceId, DeviceKey).GetHashCode();
        }

        public static bool operator ==(DISPLAY_DEVICE lhs, DISPLAY_DEVICE rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAY_DEVICE lhs, DISPLAY_DEVICE rhs) => !(lhs == rhs);

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GAMMA_RAMP : IEquatable<GAMMA_RAMP>
    {
        public const int DataPoints = 256;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = DataPoints)]
        public UInt16[] Red;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = DataPoints)]
        public UInt16[] Green;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = DataPoints)]
        public UInt16[] Blue;

        public override bool Equals(object obj) => obj is GAMMA_RAMP other && this.Equals(other);

        public bool Equals(GAMMA_RAMP other)
            => Red.SequenceEqual(other.Red) &&
                Green.SequenceEqual(other.Green) &&
                Blue.SequenceEqual(other.Blue);

        public override int GetHashCode()
        {
            return (Red, Green, Blue).GetHashCode();
        }

        public static bool operator ==(GAMMA_RAMP lhs, GAMMA_RAMP rhs) => lhs.Equals(rhs);

        public static bool operator !=(GAMMA_RAMP lhs, GAMMA_RAMP rhs) => !(lhs == rhs);

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITOR_INFO : IEquatable<MONITOR_INFO>
    {
        internal UInt32 Size;
        public RECTL Bounds;
        public RECTL WorkingArea;
        public MONITOR_INFO_FLAGS Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DisplayName;

        public override bool Equals(object obj) => obj is MONITOR_INFO other && this.Equals(other);

        public bool Equals(MONITOR_INFO other)
            => Size == other.Size &&
                Bounds.Equals(other.Bounds) &&
                WorkingArea.Equals(other.WorkingArea) &&
                Flags == other.Flags &&
                DisplayName == other.DisplayName;

        public override int GetHashCode()
        {
            return (Size, Bounds, WorkingArea, Flags, DisplayName).GetHashCode();
        }

        public static bool operator ==(MONITOR_INFO lhs, MONITOR_INFO rhs) => lhs.Equals(rhs);

        public static bool operator !=(MONITOR_INFO lhs, MONITOR_INFO rhs) => !(lhs == rhs);
    }

    public struct GDI_DISPLAY_SETTING : IEquatable<GDI_DISPLAY_SETTING>
    {
        public bool IsEnabled;
        public bool IsPrimary;
        public DISPLAY_DEVICE Device;
        public DEVICE_MODE DeviceMode;

        public override bool Equals(object obj) => obj is GDI_DISPLAY_SETTING other && this.Equals(other);

        public bool Equals(GDI_DISPLAY_SETTING other)
            => IsEnabled == other.IsEnabled &&
                IsPrimary == other.IsPrimary &&
                Device.Equals(other.Device) &&
                DeviceMode.Equals(other.DeviceMode);

        public override int GetHashCode()
        {
            return (IsEnabled, IsPrimary, Device, DeviceMode).GetHashCode();
        }

        public static bool operator ==(GDI_DISPLAY_SETTING lhs, GDI_DISPLAY_SETTING rhs) => lhs.Equals(rhs);

        public static bool operator !=(GDI_DISPLAY_SETTING lhs, GDI_DISPLAY_SETTING rhs) => !(lhs == rhs);
    }


    internal class DCHandle : SafeHandle
    {
        private readonly bool _created;

        private DCHandle(IntPtr handle, bool created) : base(handle, true)
        {
            _created = created;
        }

        public override bool IsInvalid
        {
            get => handle == IntPtr.Zero;
        }

        public static DCHandle CreateFromDevice(string screenName, string devicePath)
        {
            return new DCHandle(
                GDIImport.CreateDC(screenName, devicePath, null, IntPtr.Zero),
                true
            );
        }

        public static DCHandle CreateFromScreen(string screenName)
        {
            return CreateFromDevice(screenName, screenName);
        }

        public static DCHandle CreateFromWindow(IntPtr windowHandle)
        {
            return new DCHandle(
                GDIImport.GetDC(windowHandle),
                true
            );
        }

        public static DCHandle CreateGlobal()
        {
            return new DCHandle(
                GDIImport.CreateDC("DISPLAY", null, null, IntPtr.Zero),
                true
            );
        }

        protected override bool ReleaseHandle()
        {
            return _created
                ? GDIImport.DeleteDC(this)
                : GDIImport.ReleaseDC(IntPtr.Zero, this);
        }
    }

    class GDIImport
    {
        [DllImport("user32", CharSet = CharSet.Ansi)]
        public static extern CHANGE_DISPLAY_RESULTS ChangeDisplaySettingsEx(
            string deviceName,
            ref DEVICE_MODE devMode,
            IntPtr handler,
            CHANGE_DISPLAY_SETTINGS_FLAGS flags,
            IntPtr param
        );

        [DllImport("user32", CharSet = CharSet.Ansi)]
        public static extern CHANGE_DISPLAY_RESULTS ChangeDisplaySettingsEx(
            string deviceName,
            IntPtr devModePointer,
            IntPtr handler,
            CHANGE_DISPLAY_SETTINGS_FLAGS flags,
            IntPtr param
        );

        [DllImport("user32", CharSet = CharSet.Ansi)]
        public static extern bool EnumDisplaySettings(
            string deviceName,
            DISPLAY_SETTINGS_MODE mode,
            ref DEVICE_MODE devMode
        );

        [DllImport("gdi32", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateDC(string driver, string device, string port, IntPtr deviceMode);

        [DllImport("gdi32")]
        internal static extern bool DeleteDC(DCHandle dcHandle);


        [DllImport("user32", CharSet = CharSet.Unicode)]
        internal static extern bool EnumDisplayDevices(
            string deviceName,
            UInt32 deviceNumber,
            ref DISPLAY_DEVICE displayDevice,
            UInt32 flags
        );

        [DllImport("user32")]
        internal static extern bool EnumDisplayMonitors(
            [In] IntPtr dcHandle,
            [In] IntPtr clip,
            MonitorEnumProcedure callback,
            IntPtr callbackObject
        );

        [DllImport("user32")]
        internal static extern IntPtr GetDC(IntPtr windowHandle);

        [DllImport("gdi32")]
        internal static extern int GetDeviceCaps(DCHandle dcHandle, DEVICE_CAPABILITY index);

        [DllImport("gdi32")]
        internal static extern bool GetDeviceGammaRamp(DCHandle dcHandle, ref GAMMA_RAMP ramp);

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(
            IntPtr monitorHandle,
            ref MONITOR_INFO monitorInfo
        );

        [DllImport("user32")]
        internal static extern IntPtr MonitorFromPoint(
            [In] POINTL point,
            MONITOR_FROM_FLAG flag
        );

        [DllImport("user32")]
        internal static extern IntPtr MonitorFromRect(
            [In] RECTL rectangle,
            MONITOR_FROM_FLAG flag
        );

        [DllImport("user32")]
        internal static extern IntPtr MonitorFromWindow(
            [In] IntPtr windowHandle,
            MONITOR_FROM_FLAG flag
        );

        [DllImport("user32")]
        internal static extern bool ReleaseDC([In] IntPtr windowHandle, [In] DCHandle dcHandle);

        [DllImport("gdi32")]
        internal static extern bool SetDeviceGammaRamp(DCHandle dcHandle, ref GAMMA_RAMP ramp);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int MonitorEnumProcedure(
            IntPtr monitorHandle,
            IntPtr dcHandle,
            ref RECTL rect,
            IntPtr callbackObject
        );
    }
}