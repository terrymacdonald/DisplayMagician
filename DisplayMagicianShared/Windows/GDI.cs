using System;
using System.Linq;
using System.Runtime.InteropServices;

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

    [StructLayout(LayoutKind.Sequential)]
    public struct APP_BAR_DATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uCallbackMessage;
        public ABE_EDGE uEdge;
        public RECTL rc;
        public ABS_SETTING lParam;
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
            => //DeviceName.Equals(other.DeviceName) &&  // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
               //SpecificationVersion == other.SpecificationVersion &&
               //DriverVersion.Equals(other.DriverVersion) && // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
               //Size.Equals(other.Size) && // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
               //DriverExtra.Equals(other.DriverExtra) && // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
               //Fields.Equals(other.Fields) && // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
               //Position.Equals(other.Position) && // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
                DisplayOrientation.Equals(other.DisplayOrientation) &&
                //DisplayFixedOutput.Equals(other.DisplayFixedOutput) &&
                //Color.Equals(other.Color) && // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
                //Duplex.Equals(other.Duplex) && // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
                //YResolution.Equals(other.YResolution) && // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
                //TrueTypeOption.Equals(other.TrueTypeOption) && // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
                //Collate.Equals(other.Collate) && // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
                //FormName.Equals(other.FormName) && // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
                //LogicalInchPixels.Equals(other.LogicalInchPixels) && // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
                BitsPerPixel.Equals(other.BitsPerPixel) &&
                //PixelsWidth.Equals(other.PixelsWidth) && 
                //PixelsHeight.Equals(other.PixelsHeight) && 
                DisplayFlags.Equals(other.DisplayFlags) &&
                DisplayFrequency == other.DisplayFrequency;

        public override int GetHashCode()
        {
            // Removed specifically for DisplayMagician matching. Remove if you need true equality matching
            //return (DeviceName, SpecificationVersion, DriverVersion, Size, DriverExtra, Fields, Position, DisplayOrientation, DisplayFixedOutput, Color, Duplex,
            //    YResolution, TrueTypeOption, Collate, FormName, LogicalInchPixels, BitsPerPixel, PixelsWidth, PixelsHeight, DisplayFlags, DisplayFrequency).GetHashCode();
            return (DisplayOrientation, BitsPerPixel, DisplayFlags, DisplayFrequency).GetHashCode();
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
                // DeviceName == other.DeviceName && // Had to remove this as the device name often changes after a reboot!
                DeviceString == other.DeviceString &&
                //StateFlags == other.StateFlags &&
                DeviceId == other.DeviceId;
        //DeviceKey == other.DeviceKey;

        public override int GetHashCode()
        {
            //return (Size, DeviceName, DeviceString, StateFlags, DeviceId, DeviceKey).GetHashCode();
            return (Size, DeviceString, DeviceId).GetHashCode();
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
                //IsPrimary == other.IsPrimary &&
                Device.Equals(other.Device) &&
                DeviceMode.Equals(other.DeviceMode);

        public override int GetHashCode()
        {
            //return (IsEnabled, IsPrimary, Device, DeviceMode).GetHashCode();
            return (IsEnabled, Device, DeviceMode).GetHashCode();
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

    public enum ABM_MESSAGE : UInt32
    {
        ABM_NEW = 0x00000000, // Registers a new appbar and specifies the message identifier that the system should use to send notification messages to the appbar.
        ABM_REMOVE = 0x00000001, // Unregisters an appbar, removing the bar from the system's internal list.
        ABM_QUERYPOS = 0x00000002, // Requests a size and screen position for an appbar.
        ABM_SETPOS = 0x00000003, // Sets the size and screen position of an appbar.
        ABM_GETSTATE = 0x00000004, // Retrieves the autohide and always-on-top states of the Windows taskbar.
        ABM_GETTASKBARPOS = 0x00000005, // Retrieves the bounding rectangle of the Windows taskbar. Note that this applies only to the system taskbar. Other objects, particularly toolbars supplied with third-party software, also can be present. As a result, some of the screen area not covered by the Windows taskbar might not be visible to the user. To retrieve the area of the screen not covered by both the taskbar and other app bars—the working area available to your application—, use the GetMonitorInfo function.
        ABM_ACTIVATE = 0x00000006, // Notifies the system to activate or deactivate an appbar. The lParam member of the APPBARDATA pointed to by pData is set to TRUE to activate or FALSE to deactivate.
        ABM_GETAUTOHIDEBAR = 0x00000007, // Retrieves the handle to the autohide appbar associated with a particular edge of the screen.
        ABM_SETAUTOHIDEBAR = 0x00000008, // Registers or unregisters an autohide appbar for an edge of the screen.
        ABM_WINDOWPOSCHANGED = 0x00000009, // Notifies the system when an appbar's position has changed.
        ABM_SETSTATE = 0x0000000A, // Windows XP and later: Sets the state of the appbar's autohide and always-on-top attributes.        
        ABM_GETAUTOHIDEBAREX = 0x0000000B, // Windows XP and later: Retrieves the handle to the autohide appbar associated with a particular edge of a particular monitor.
        ABM_SETAUTOHIDEBAREX = 0x0000000C, // Windows XP and later: Registers or unregisters an autohide appbar for an edge of a particular monitor.
    }

    public enum ABE_EDGE : UInt32
    {
        ABE_LEFT = 0,
        ABE_TOP = 1,
        ABE_RIGHT = 2,
        ABE_BOTTOM = 3,
    }

    [Flags]
    public enum ABS_SETTING : UInt32
    {
        ABS_AUTOHIDE = 0x1,
        ABS_ALWAYSONTOP = 0x2,
    }


    class GDIImport
    {
        private const int ABS_NO_AUTOHIDE = 0x00;
        private const int ABS_AUTOHIDE = 0x01;

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

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);

        // This code was part of development to add recording taskbar location and state so that we could apply it later
        // Windows 11 doesn't support moving 
        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHAppBarMessage(ABM_MESSAGE dwMessage, ref APP_BAR_DATA abd);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int MonitorEnumProcedure(
            IntPtr monitorHandle,
            IntPtr dcHandle,
            ref RECTL rect,
            IntPtr callbackObject
        );


        public static APP_BAR_DATA GetTaskbarPosition()
        {
            APP_BAR_DATA abd = new APP_BAR_DATA();
            abd.cbSize = Marshal.SizeOf(abd);

            // Query the system for an approved size and position.
            SHAppBarMessage(ABM_MESSAGE.ABM_GETTASKBARPOS, ref abd);

            return abd;
        }

        public static bool GetTaskbarAutoHide(APP_BAR_DATA abd)
        {
            // Query the system for an approved size and position.
            ABS_SETTING state = (ABS_SETTING)SHAppBarMessage(ABM_MESSAGE.ABM_GETSTATE, ref abd);

            return state.HasFlag(ABS_SETTING.ABS_AUTOHIDE);
        }


        //public static void MoveTaskbar(APP_BAR_DATA abd, ABE_EDGE edge, Size idealSize)
        public static void SetTaskbarPosition(APP_BAR_DATA abd, ABE_EDGE edge)
        {
            abd.uEdge = edge;
            SHAppBarMessage(ABM_MESSAGE.ABM_SETPOS, ref abd);

            /*// Get current size
            int idealSize = 100;

            if (edge == ABE_EDGE.ABE_LEFT || edge == ABE_EDGE.ABE_RIGHT)
            {
                abd.rc.Top = 0;
                abd.rc.Bottom = SystemInformation.PrimaryMonitorSize.Height;
                if (edge == ABE_EDGE.ABE_LEFT)
                {
                    abd.rc.Right = idealSize;
                }
                else
                {
                    abd.rc.Right = SystemInformation.PrimaryMonitorSize.Width;
                    abd.rc.Left = abd.rc.Right - idealSize;
                }

            }
            else
            {
                abd.rc.Left = 0;
                abd.rc.Right = SystemInformation.PrimaryMonitorSize.Width;
                if (edge == ABE_EDGE.ABE_TOP)
                {
                    abd.rc.Bottom = idealSize;
                }
                else
                {
                    abd.rc.Bottom = SystemInformation.PrimaryMonitorSize.Height;
                    abd.rc.Top = abd.rc.Bottom - idealSize;
                }
            }

            ABS_SETTING state = (ABS_SETTING)SHAppBarMessage(ABM_MESSAGE.ABM_GETSTATE, ref abd);*/
        }

        // THE FOLLOWING CODE WAS AN ATTEMPT TO SET THE TASKBAR POSITION USING CODE
        // TURNS OUT WE CAN'T ACTUALLY SET THE POSITION PROGRAMMATICALLY iIN WIN10 or WIN11
        // Next we want to remember where the windows toolbar is for each screen
        // Query the system for an approved size and position.
        // APP_BAR_DATA taskbarPosition = GDIImport.GetTaskbarPosition();
        // bool taskbarAutoHide = GDIImport.GetTaskbarAutoHide(taskbarPosition);

        // try to move the taskbar
        // GDIImport.SetTaskbarPosition(taskbarPosition, ABE_EDGE.ABE_TOP);
        // GDIImport.SetTaskbarAutoHide(taskbarPosition, true);

        public static void SetTaskbarAutoHide(APP_BAR_DATA abd, bool hide)
        {
            if (hide)
            {
                // Set the autohide flag
                abd.lParam |= ABS_SETTING.ABS_AUTOHIDE;
            }
            else
            {
                // Clear the autohide flag
                abd.lParam &= ~ABS_SETTING.ABS_AUTOHIDE;
            }
            SHAppBarMessage(ABM_MESSAGE.ABM_GETSTATE, ref abd);
        }

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public const uint WM_SYSCOMMAND = 0x0112;
        public const uint SC_MONITORPOWER = 0xF170;

        public static void ResetGraphicsStack()
        {
            // Simulate Win+Ctrl+Shift+B by disabling monitor, then re-enabling
            PostMessage(
                new IntPtr(0xFFFF), // HWND_BROADCAST
                WM_SYSCOMMAND,
                new IntPtr(SC_MONITORPOWER),
                new IntPtr(-1) // Monitor on
            );
        }
    }
}