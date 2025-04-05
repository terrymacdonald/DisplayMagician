using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagicianShared
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    public struct WINDOWPOS
    {
        public IntPtr hWnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public SET_WINDOW_POSITION_FLAGS flags;

        // Returns the WINDOWPOS structure pointed to by the lParam parameter
        // of a WM_WINDOWPOSCHANGING or WM_WINDOWPOSCHANGED message.
        public static WINDOWPOS FromMessage(IntPtr lParam)
        {
            // Marshal the lParam parameter to an WINDOWPOS structure,
            // and return the new structure
            return (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
        }

        // Replaces the original WINDOWPOS structure pointed to by the lParam
        // parameter of a WM_WINDOWPOSCHANGING or WM_WINDOWPSCHANGING message
        // with this one, so that the native window will be able to see any
        // changes that we have made to its values.
        public void UpdateMessage(IntPtr lParam)
        {
            // Marshal this updated structure back to lParam so the native
            // window can respond to our changes.
            // The old structure that it points to should be deleted, too.
            Marshal.StructureToPtr(this, lParam, true);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    public struct RECT
    {
        public Int32 left;
        public Int32 top;
        public Int32 right;
        public Int32 bottom;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
    public struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public ABEDGE uEdge;
        public RECT rc;
        public int lParam;
    }



    /// <summary>
    /// The MONITORINFOEX structure contains information about a display monitor.
    /// The GetMonitorInfo function stores information into a MONITORINFOEX structure or a MONITORINFO structure.
    /// The MONITORINFOEX structure is a superset of the MONITORINFO structure. The MONITORINFOEX structure adds a string member to contain a name
    /// for the display monitor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    public struct MONITORINFOEX
    {
        /// <summary>
        /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFOEX) (72) before calling the GetMonitorInfo function.
        /// Doing so lets the function determine the type of structure you are passing to it.
        /// </summary>
        public UInt32 cbSize;

        /// <summary>
        /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates.
        /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
        /// </summary>
        public RECT rcMonitor;

        /// <summary>
        /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications,
        /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor.
        /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars.
        /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
        /// </summary>
        public RECT rcWork;

        /// <summary>
        /// The attributes of the display monitor.
        ///
        /// This member can be the following value:
        ///   1 : MONITORINFOF_PRIMARY
        /// </summary>
        public UInt32 dwFlags;

        /// <summary>
        /// A string that specifies the device name of the monitor being used. Most applications have no use for a display monitor name,
        /// and so can save some bytes by using a MONITORINFO structure.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Utils.CCHDEVICENAME)]
        public string szDevice;

        /*public void Init()
        {
            this.cbSize = 40 + 2 * Utils.CCHDEVICENAME;
            this.DeviceName = string.Empty;
        }*/
    }


    /// <summary>
    /// The MONITORINFO structure contains information about a display monitor.
    /// The GetMonitorInfo function stores information into a MONITORINFOEX structure or a MONITORINFO structure.
    /// The MONITORINFOEX structure is a superset of the MONITORINFO structure. The MONITORINFOEX structure adds a string member to contain a name
    /// for the display monitor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    public struct MONITORINFO
    {
        /// <summary>
        /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFOEX) (72) before calling the GetMonitorInfo function.
        /// Doing so lets the function determine the type of structure you are passing to it.
        /// </summary>
        public UInt32 cbSize;

        /// <summary>
        /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates.
        /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
        /// </summary>
        public RECT rcMonitor;

        /// <summary>
        /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications,
        /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor.
        /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars.
        /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
        /// </summary>
        public RECT rcWork;

        /// <summary>
        /// The attributes of the display monitor.
        ///
        /// This member can be the following value:
        ///   1 : MONITORINFOF_PRIMARY
        /// </summary>
        public UInt32 dwFlags;

        /*public void Init()
        {
            this.cbSize = 40 + 2 * Utils.CCHDEVICENAME;
            this.DeviceName = string.Empty;
        }*/
    }


    [Flags]
    public enum SET_WINDOW_POSITION_FLAGS : UInt32
    {
        SWP_ASYNCWINDOWPOS = 0x4000,
        SWP_DEFERERASE = 0x2000,
        SWP_DRAWFRAME = 0x0020,
        SWP_FRAMECHANGED = 0x0020,
        SWP_HIDEWINDOW = 0x0080,
        SWP_NOACTIVATE = 0x0010,
        SWP_NOCOPYBITS = 0x0100,
        SWP_NOMOVE = 0x0002,
        SWP_NOOWNERZORDER = 0x0200,
        SWP_NOREDRAW = 0x0008,
        SWP_NOREPOSITION = 0x0200,
        SWP_NOSENDCHANGING = 0x0400,
        SWP_NOSIZE = 0x0001,
        SWP_NOZORDER = 0x0004,
        SWP_SHOWWINDOW = 0x0040,
    }

    public enum SET_WINDOW_POSITION_ZORDER : Int32
    {
        HWND_TOP = 0,
        HWND_BOTTOM = 1,
        HWND_TOPMOST = -1,
        HWND_NOTOPMOST = -2,
    }


    [Flags]
    public enum SendMessageTimeoutFlag : uint
    {
        SMTO_NORMAL = 0x0,
        SMTO_BLOCK = 0x1,
        SMTO_ABORTIFHUNG = 0x2,
        SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
        SMTO_ERRORONEXIT = 0x20
    }

    public enum ABEDGE : UInt32
    {
        ABE_LEFT = 0x0,
        ABE_TOP = 0x1,
        ABE_RIGHT = 0x2,
        ABE_BOTTOM = 0x3,
    }

    [Flags]
    public enum MOUSEKEYS : UInt32
    {
        MK_LBUTTON = 0x1,
        MK_RBUTTON = 0x2,
        MK_SHIFT = 0x4,
        MK_CONTROL = 0x8,
        MK_MBUTTON = 0x10,
        MK_XBUTTON1 = 0x20,
        MK_XBUTTON2 = 0x40,
    }


    enum SYSCOMMAND : int
    {
        SC_SIZE = 0xF000,
        SC_MOVE = 0xF010,
        SC_MINIMIZE = 0xF020,
        SC_MAXIMIZE = 0xF030,
        SC_NEXTWINDOW = 0xF040,
        SC_PREVWINDOW = 0xF050,
        SC_CLOSE = 0xF060,
        SC_VSCROLL = 0xF070,
        SC_HSCROLL = 0xF080,
        SC_MOUSEMENU = 0xF090,
        SC_KEYMENU = 0xF100,
        SC_ARRANGE = 0xF110,
        SC_RESTORE = 0xF120,
        SC_TASKLIST = 0xF130,
        SC_SCREENSAVE = 0xF140,
        SC_HOTKEY = 0xF150,
        //#if(WINVER >= 0x0400) //Win95
        SC_DEFAULT = 0xF160,
        SC_MONITORPOWER = 0xF170,
        SC_CONTEXTHELP = 0xF180,
        SC_SEPARATOR = 0xF00F,
        //#endif /* WINVER >= 0x0400 */

        //#if(WINVER >= 0x0600) //Vista
        SCF_ISSECURE = 0x00000001,
        //#endif /* WINVER >= 0x0600 */

        /*
          * Obsolete names
          */
        SC_ICON = SC_MINIMIZE,
        SC_ZOOM = SC_MAXIMIZE,
    }


    class Utils
    {


        #region enum HChangeNotifyEventID
        /// <summary>
        /// Describes the event that has occurred.
        /// Typically, only one event is specified at a time.
        /// If more than one event is specified, the values contained
        /// in the <i>dwItem1</i> and <i>dwItem2</i>
        /// parameters must be the same, respectively, for all specified events.
        /// This parameter can be one or more of the following values.
        /// </summary>
        /// <remarks>
        /// <para><b>Windows NT/2000/XP:</b> <i>dwItem2</i> contains the index
        /// in the system image list that has changed.
        /// <i>dwItem1</i> is not used and should be <see langword="null"/>.</para>
        /// <para><b>Windows 95/98:</b> <i>dwItem1</i> contains the index
        /// in the system image list that has changed.
        /// <i>dwItem2</i> is not used and should be <see langword="null"/>.</para>
        /// </remarks>
        [Flags]
        public enum HChangeNotifyEventID
        {
            /// <summary>
            /// All events have occurred.
            /// </summary>
            SHCNE_ALLEVENTS = 0x7FFFFFFF,

            /// <summary>
            /// A file type association has changed. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/>
            /// must be specified in the <i>uFlags</i> parameter.
            /// <i>dwItem1</i> and <i>dwItem2</i> are not used and must be <see langword="null"/>.
            /// </summary>
            SHCNE_ASSOCCHANGED = 0x08000000,

            /// <summary>
            /// The attributes of an item or folder have changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the item or folder that has changed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_ATTRIBUTES = 0x00000800,

            /// <summary>
            /// A nonfolder item has been created.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the item that was created.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_CREATE = 0x00000002,

            /// <summary>
            /// A nonfolder item has been deleted.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the item that was deleted.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DELETE = 0x00000004,

            /// <summary>
            /// A drive has been added.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that was added.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DRIVEADD = 0x00000100,

            /// <summary>
            /// A drive has been added and the Shell should create a new window for the drive.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that was added.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DRIVEADDGUI = 0x00010000,

            /// <summary>
            /// A drive has been removed. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that was removed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DRIVEREMOVED = 0x00000080,

            /// <summary>
            /// Not currently used.
            /// </summary>
            SHCNE_EXTENDED_EVENT = 0x04000000,

            /// <summary>
            /// The amount of free space on a drive has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive on which the free space changed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_FREESPACE = 0x00040000,

            /// <summary>
            /// Storage media has been inserted into a drive.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that contains the new media.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_MEDIAINSERTED = 0x00000020,

            /// <summary>
            /// Storage media has been removed from a drive.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive from which the media was removed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_MEDIAREMOVED = 0x00000040,

            /// <summary>
            /// A folder has been created. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/>
            /// or <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that was created.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_MKDIR = 0x00000008,

            /// <summary>
            /// A folder on the local computer is being shared via the network.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that is being shared.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_NETSHARE = 0x00000200,

            /// <summary>
            /// A folder on the local computer is no longer being shared via the network.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that is no longer being shared.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_NETUNSHARE = 0x00000400,

            /// <summary>
            /// The name of a folder has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the previous pointer to an item identifier list (PIDL) or name of the folder.
            /// <i>dwItem2</i> contains the new PIDL or name of the folder.
            /// </summary>
            SHCNE_RENAMEFOLDER = 0x00020000,

            /// <summary>
            /// The name of a nonfolder item has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the previous PIDL or name of the item.
            /// <i>dwItem2</i> contains the new PIDL or name of the item.
            /// </summary>
            SHCNE_RENAMEITEM = 0x00000001,

            /// <summary>
            /// A folder has been removed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that was removed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_RMDIR = 0x00000010,

            /// <summary>
            /// The computer has disconnected from a server.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the server from which the computer was disconnected.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_SERVERDISCONNECT = 0x00004000,

            /// <summary>
            /// The contents of an existing folder have changed,
            /// but the folder still exists and has not been renamed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that has changed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// If a folder has been created, deleted, or renamed, use SHCNE_MKDIR, SHCNE_RMDIR, or
            /// SHCNE_RENAMEFOLDER, respectively, instead.
            /// </summary>
            SHCNE_UPDATEDIR = 0x00001000,

            /// <summary>
            /// An image in the system image list has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_DWORD"/> must be specified in <i>uFlags</i>.
            /// </summary>
            SHCNE_UPDATEIMAGE = 0x00008000,

        }
        #endregion // enum HChangeNotifyEventID

        #region public enum HChangeNotifyFlags
        /// <summary>
        /// Flags that indicate the meaning of the <i>dwItem1</i> and <i>dwItem2</i> parameters.
        /// The uFlags parameter must be one of the following values.
        /// </summary>
        [Flags]
        public enum HChangeNotifyFlags
        {
            /// <summary>
            /// The <i>dwItem1</i> and <i>dwItem2</i> parameters are DWORD values.
            /// </summary>
            SHCNF_DWORD = 0x0003,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of ITEMIDLIST structures that
            /// represent the item(s) affected by the change.
            /// Each ITEMIDLIST must be relative to the desktop folder.
            /// </summary>
            SHCNF_IDLIST = 0x0000,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of
            /// maximum length MAX_PATH that contain the full path names
            /// of the items affected by the change.
            /// </summary>
            SHCNF_PATHA = 0x0001,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of
            /// maximum length MAX_PATH that contain the full path names
            /// of the items affected by the change.
            /// </summary>
            SHCNF_PATHW = 0x0005,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that
            /// represent the friendly names of the printer(s) affected by the change.
            /// </summary>
            SHCNF_PRINTERA = 0x0002,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that
            /// represent the friendly names of the printer(s) affected by the change.
            /// </summary>
            SHCNF_PRINTERW = 0x0006,
            /// <summary>
            /// The function should not return until the notification
            /// has been delivered to all affected components.
            /// As this flag modifies other data-type flags, it cannot by used by itself.
            /// </summary>
            SHCNF_FLUSH = 0x1000,
            /// <summary>
            /// The function should begin delivering notifications to all affected components
            /// but should return as soon as the notification process has begun.
            /// As this flag modifies other data-type flags, it cannot by used by itself.
            /// </summary>
            SHCNF_FLUSHNOWAIT = 0x2000
        }
        #endregion // enum HChangeNotifyFlags

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, String lpWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr windowHandle, uint message, IntPtr wordParameter, IntPtr longParameter, SendMessageTimeoutFlag flag, uint timeout, out IntPtr resultHandle);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, string lParam);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, Int16 wParam, Int16 lParam);


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern void SHChangeNotify(HChangeNotifyEventID wEventId,
                                   HChangeNotifyFlags uFlags,
                                   IntPtr dwItem1,
                                   IntPtr dwItem2);

        [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, SET_WINDOW_POSITION_ZORDER hWndInsertAfter, int x, int y, int cx, int cy, SET_WINDOW_POSITION_FLAGS uFlags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int ShowWindow(IntPtr hwnd, int command);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);


        /// <summary>
        ///     The MoveWindow function changes the position and dimensions of the specified window. For a top-level window, the
        ///     position and dimensions are relative to the upper-left corner of the screen. For a child window, they are relative
        ///     to the upper-left corner of the parent window's client area.
        ///     <para>
        ///     Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633534%28v=vs.85%29.aspx for more
        ///     information
        ///     </para>
        /// </summary>
        /// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br /> Handle to the window.</param>
        /// <param name="X">C++ ( X [in]. Type: int )<br />Specifies the new position of the left side of the window.</param>
        /// <param name="Y">C++ ( Y [in]. Type: int )<br /> Specifies the new position of the top of the window.</param>
        /// <param name="nWidth">C++ ( nWidth [in]. Type: int )<br />Specifies the new width of the window.</param>
        /// <param name="nHeight">C++ ( nHeight [in]. Type: int )<br />Specifies the new height of the window.</param>
        /// <param name="bRepaint">
        ///     C++ ( bRepaint [in]. Type: bool )<br />Specifies whether the window is to be repainted. If this
        ///     parameter is TRUE, the window receives a message. If the parameter is FALSE, no repainting of any kind occurs. This
        ///     applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the
        ///     parent window uncovered as a result of moving a child window.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero.<br /> If the function fails, the return value is zero.
        ///     <br />To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        public static bool IsWindows11()
        {
            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            var currentBuildStr = (string)reg.GetValue("CurrentBuild");
            var currentBuild = int.Parse(currentBuildStr);

            return currentBuild >= 22000;
        }

        public static int MakeLParam(int p, int p_2)
        {
            return ((p_2 << 16) | (p & 0xFFFF));
        }

        internal delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        public static List<MONITORINFOEX> EnumMonitors()
        {
            List<MONITORINFOEX> monitors = new List<MONITORINFOEX>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
        delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            MONITORINFOEX mi = new MONITORINFOEX();
            mi.cbSize = (uint)Marshal.SizeOf(mi);
            bool success = GetMonitorInfo(hMonitor, ref mi);
            if (success)
            {
                monitors.Add(mi);
            }
            return true;
        }, IntPtr.Zero);
            return monitors;
        }

        private static bool MonitorEnumCallBack(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            MONITORINFOEX mon_info = new MONITORINFOEX();
            mon_info.cbSize = (UInt32)Marshal.SizeOf(typeof(MONITORINFOEX));
            //mon_info.szDevice = new char[Utils.CCHDEVICENAME];
            GetMonitorInfo(hMonitor, ref mon_info);
            ///Monitor info is stored in 'mon_info'
            return true;
        }

        /*public static bool RefreshNotificationTray()
        {
            Utils.SHChangeNotify(Utils.HChangeNotifyEventID.SHCNE_ASSOCCHANGED, Utils.HChangeNotifyFlags.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);

            IntPtr result;
            IntPtr lparam = Marshal.StringToHGlobalAuto("Environment");
            Utils.SendMessage((IntPtr)Utils.HWND_BROADCAST, Utils.WM_SETTINGCHANGE, (IntPtr)0, lparam);
            Utils.SendMessage((IntPtr)Utils.HWND_BROADCAST, Utils.WM_SETTINGCHANGE, (IntPtr)0, (IntPtr)0);
            return true;
        }*/

        public static Point PointFromLParam(IntPtr lParam)
        {
            return new Point((int)(lParam) & 0xFFFF, ((int)(lParam) >> 16) & 0xFFFF);
        }

        public static IntPtr LParamFromPoint(Point point)
        {
            return (IntPtr)((point.Y << 16) | (point.X & 0xFFFF));
        }

        public static IntPtr LParamFromPoint(int x, int y)
        {
            return (IntPtr)((y << 16) | (x & 0xFFFF));
        }


        public const int NULL = 0;
        public const int HWND_BROADCAST = 0xffff;
        public const int WM_ENTERSIZEMOVE = 0x0231;
        public const int WM_EXITSIZEMOVE = 0x0232;
        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public const int WM_WINDOWPOSCHANGED = 0x0047;
        public const int WM_SYSCOMMAND = 0x112;
        public const int WM_NOTIFY = 0xA005;
        public const int WM_SETTINGCHANGE = 0x001a;
        public const int WM_THEMECHANGED = 0x031a;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int SPI_SETWORKAREA = 0x002F;
        public const int SHELLHOOK = 0xC028;
        public const int WM_USER_REFRESHTASKBAR = 0x05CA;
        public const int WM_USER_451 = 0x05C3;
        public const int WM_USER_440 = 0x05B8;
        public const int WM_USER_336 = 0x0550;
        public const int WM_USER_92 = 0x045C;
        public const int WM_USER_7 = 0x0407;
        public const int WM_USER_1 = 0x0401;
        public const int WM_USER_100 = 0x0464;
        public const int WM_USER_13 = 0x040D;
        public const int wParam_SHELLTRAY = 0x00000006;

        public const int ABM_NEW = 0x00000000;
        public const int ABM_REMOVE = 0x00000001;
        public const int ABM_QUERYPOS = 0x00000002;
        public const int ABM_SETPOS = 0x00000003;
        public const int ABM_GETSTATE = 0x00000004;
        public const int ABM_GETTASKBARPOS = 0x00000005;
        public const int ABM_ACTIVATE = 0x00000006; // lParam == TRUE/FALSE means activate/deactivate
        public const int ABM_GETAUTOHIDEBAR = 0x00000007;
        public const int ABM_SETAUTOHIDEBAR = 0x00000008; // this can fail at any time.  MUST check the result
                                                          // lParam = TRUE/FALSE  Set/Unset
                                                          // uEdge = what edge
        public const int ABM_WINDOWPOSCHANGED = 0x0000009;
        public const int ABM_SETSTATE = 0x0000000a;

        // these are put in the wparam of callback messages
        public const int ABN_STATECHANGE = 0x0000000;
        public const int ABN_POSCHANGED = 0x0000001;
        public const int ABN_FULLSCREENAPP = 0x0000002;
        public const int ABN_WINDOWARRANGE = 0x0000003; // lParam == TRUE means hide

        // flags for get state
        public const int ABS_AUTOHIDE = 0x0000001;
        public const int ABS_ALWAYSONTOP = 0x0000002;

        public const int ABE_LEFT = 0;
        public const int ABE_TOP = 1;
        public const int ABE_RIGHT = 2;
        public const int ABE_BOTTOM = 3;

        public const int MONITOR_DEFAULTTONULL = 0;
        public const int MONITOR_DEFAULTTOPRIMARY = 1;
        public const int MONITOR_DEFAULTTONEAREST = 2;

        // size of a device name string
        public const int CCHDEVICENAME = 32;
        public const uint MONITORINFOF_PRIMARY = 1;


        public static bool OldFileVersionsExist(string path, string searchPattern = "", string skipFilename = "")
        {
            try
            {
                if (String.IsNullOrWhiteSpace(path))
                {
                    SharedLogger.logger.Error($"SharedUtils/OldFileVersionsExist: We were passed an empty path, so returning an empty list of matching files.");
                    return false;
                }

                string[] filesThatMatch;

                if (String.IsNullOrWhiteSpace(searchPattern))
                {
                    filesThatMatch = Directory.GetFiles(path);
                }
                else
                {
                    filesThatMatch = Directory.GetFiles(path, searchPattern);
                }

                if (filesThatMatch.Length > 0)
                {
                    SharedLogger.logger.Trace($"SharedUtils/OldFileVersionsExist: Found {filesThatMatch.Length} files matching the '{searchPattern}' pattern in {path}");
                    foreach (var filename in filesThatMatch)
                    {
                        // If this is the file we should skip, then let's skip it
                        if (filename.Equals(skipFilename, StringComparison.InvariantCultureIgnoreCase))
                        {
                            SharedLogger.logger.Trace($"SharedUtils/OldFileVersionsExist: Skipping {filename} as we want to ignore the {skipFilename} file.");
                            continue;
                        }

                        return true;
                    }
                }
                else
                {
                    SharedLogger.logger.Trace($"SharedUtils/OldFileVersionsExist: We tried looking for all files that matched the pattern '{searchPattern}' in path {path} and couldn't find any. skipping processing.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"SharedUtils/OldFileVersionsExist: Exception while trying to OldFileVersionsExist. Unable to find the files.");
                return false;
            }
        }

        public static bool RenameOldFileVersions(string path, string searchPattern, string skipFilename)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(path))
                {
                    SharedLogger.logger.Error($"SharedUtils/RenameOldFileVersions: We were passed an empty path, so returning an empty list of matching files.");
                    return false;
                }

                string[] filesToRename;

                if (String.IsNullOrWhiteSpace(searchPattern))
                {
                    filesToRename = Directory.GetFiles(path);
                }
                else
                {
                    filesToRename = Directory.GetFiles(path, searchPattern);
                }

                if (filesToRename.Length > 0)
                {
                    SharedLogger.logger.Trace($"SharedUtils/RenameOldFileVersions: Found {filesToRename.Length} files matching the '{searchPattern}' pattern in {path}");
                    foreach (var filename in filesToRename)
                    {
                        // If this is the file we should skip, then let's skip it
                        if (filename.Equals(skipFilename, StringComparison.InvariantCultureIgnoreCase))
                        {
                            SharedLogger.logger.Trace($"SharedUtils/RenameOldFileVersions: Skipping renaming {filename} to {filename}.old as we want to keep the {skipFilename} file.");
                            continue;
                        }

                        try
                        {
                            SharedLogger.logger.Trace($"SharedUtils/RenameOldFileVersions: Attempting to rename {filename} to {filename}.old");
                            File.Move(filename, $"{filename}.old");
                        }
                        catch (Exception ex2)
                        {
                            SharedLogger.logger.Error(ex2, $"SharedUtils/RenameOldFileVersions: Exception while trying to rename {filename} to {filename}.old. Skipping this rename.");
                        }
                    }
                }
                else
                {
                    SharedLogger.logger.Trace($"SharedUtils/RenameOldFileVersions: We tried looking for all files that matched the pattern '{searchPattern}' in path {path} and couldn't find any. skipping processing.");
                }

                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"SharedUtils/RenameOldFileVersions: Exception while trying to RenameOldFileVersions. Unable to rename the files.");
                return false;
            }
        }

        public static bool UpgradeOldFileVersions(string path, string searchPattern = "", string newFilename = "")
        {
            try
            {
                if (String.IsNullOrWhiteSpace(path))
                {
                    SharedLogger.logger.Error($"SharedUtils/UpgradeOldFileVersions: We were passed an empty path, so returning an empty list of matching files.");
                    return false;
                }

                string newFullFilename = Path.Combine(path, newFilename);

                // get all the names of the files that match the search pattern
                // get the last one (as it is the latest one in use) and convert it to the new file format
                // rename all the files matching the search to .old files

                string[] filesToUpgrade;

                if (String.IsNullOrWhiteSpace(searchPattern))
                {
                    filesToUpgrade = Directory.GetFiles(path);
                }
                else
                {
                    filesToUpgrade = Directory.GetFiles(path, searchPattern);
                }

                if (filesToUpgrade.Length > 0)
                {
                    SharedLogger.logger.Trace($"SharedUtils/UpgradeOldFileVersions: Found {filesToUpgrade.Length} files matching the '{searchPattern}' pattern in {path} to upgrade");

                    // get the last files in the list
                    var lastFile = filesToUpgrade.Last();
                    if (newFilename != Path.GetFileName(lastFile))
                    {
                        // If the new filename is different from the last file, then upgrade the last file
                        try
                        {
                            SharedLogger.logger.Trace($"SharedUtils/UpgradeOldFileVersions: Attempting to copy {lastFile} to {newFilename} to upgrade it.");
                            File.Copy(lastFile, newFullFilename);
                        }
                        catch (Exception ex1)
                        {
                            SharedLogger.logger.Error(ex1, $"SharedUtils/UpgradeOldFileVersions: Exception while trying to copy {lastFile} to {newFullFilename}. Unable to copy the file.");
                        }
                    }

                    // Now we need to rename all the old files to .old files
                    foreach (var filename in filesToUpgrade)
                    {
                        // If this is the file we should skip, then let's skip it
                        if (filename.Equals(newFilename, StringComparison.InvariantCultureIgnoreCase))
                        {
                            SharedLogger.logger.Trace($"SharedUtils/UpgradeOldFileVersions: Skipping renaming {filename} to {filename}.old as we want to keep the {newFilename} file.");
                            continue;
                        }

                        try
                        {
                            SharedLogger.logger.Trace($"SharedUtils/UpgradeOldFileVersions: Attempting to rename {filename} to {filename}.old");
                            File.Move(filename, $"{filename}.old");
                        }
                        catch (Exception ex2)
                        {
                            SharedLogger.logger.Error(ex2, $"SharedUtils/UpgradeOldFileVersions: Exception while trying to rename {filename} to {filename}.old. Skipping this rename.");
                        }
                    }
                }
                else
                {
                    SharedLogger.logger.Trace($"SharedUtils/UpgradeOldFileVersions: We tried looking for all files that matched the pattern '{searchPattern}' in path {path} and couldn't find any. skipping processing.");
                }

                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"SharedUtils/UpgradeOldFileVersions: Exception while trying to RenameOldFileVersions. Unable to rename the files.");
                return false;
            }
        }

    }

    public static class CollectionComparer
    {
        public static bool EqualButDifferentOrder<T>(IList<T> list1, IList<T> list2)
        {

            if (list1.Count != list2.Count)
            {
                return false;
            }

            // Now we need to go through the list1, checking that all it's items are in list2
            foreach (T item1 in list1)
            {
                bool foundIt = false;
                foreach (T item2 in list2)
                {
                    if (item1.Equals(item2))
                    {
                        foundIt = true;
                        break;
                    }
                }
                if (!foundIt)
                {
                    return false;
                }
            }

            // Now we need to go through the list2, checking that all it's items are in list1
            foreach (T item2 in list2)
            {
                bool foundIt = false;
                foreach (T item1 in list1)
                {
                    if (item1.Equals(item2))
                    {
                        foundIt = true;
                        break;
                    }
                }
                if (!foundIt)
                {
                    return false;
                }
            }

            return true;
        }


        public static bool EqualButDifferentOrder<TKey, TValue>(IDictionary<TKey, TValue> dict1, IDictionary<TKey, TValue> dict2)
        {

            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            // Now we need to go through the dict1, checking that all it's items are in dict2
            foreach (KeyValuePair<TKey, TValue> item1 in dict1)
            {
                bool foundIt = false;
                foreach (KeyValuePair<TKey, TValue> item2 in dict2)
                {
                    if (item1.Key.Equals(item2.Key) && item1.Value.Equals(item2.Value))
                    {
                        foundIt = true;
                        break;
                    }
                }
                if (!foundIt)
                {
                    return false;
                }
            }

            // Now we need to go through the dict2, checking that all it's items are in dict1
            foreach (KeyValuePair<TKey, TValue> item2 in dict2)
            {
                bool foundIt = false;
                foreach (KeyValuePair<TKey, TValue> item1 in dict1)
                {
                    if (item1.Key.Equals(item2.Key) && item1.Value.Equals(item2.Value))
                    {
                        foundIt = true;
                        break;
                    }
                }
                if (!foundIt)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AreEquivalent(object obj1, object obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 == null || obj2 == null) return false;

            var type1 = obj1.GetType();
            var type2 = obj2.GetType();

            if (type1 != type2) return false;

            // Handle primitive types and strings
            if (type1.IsPrimitive || obj1 is string)
            {
                return obj1.Equals(obj2);
            }

            // Handle dictionaries
            if (typeof(IDictionary).IsAssignableFrom(type1))
            {
                var dict1 = (IDictionary)obj1;
                var dict2 = (IDictionary)obj2;

                if (dict1.Count != dict2.Count) return false;

                foreach (var key in dict1.Keys)
                {
                    if (!dict2.Contains(key) || !AreEquivalent(dict1[key], dict2[key]))
                    {
                        return false;
                    }
                }
                return true;
            }

            // Handle enumerables (lists, arrays)
            if (typeof(IEnumerable).IsAssignableFrom(type1))
            {
                var enum1 = ((IEnumerable)obj1).Cast<object>().ToList();
                var enum2 = ((IEnumerable)obj2).Cast<object>().ToList();

                if (enum1.Count != enum2.Count) return false;

                // Use HashSet for unordered comparison
                var set1 = new HashSet<object>(enum1, new ObjectComparer());
                var set2 = new HashSet<object>(enum2, new ObjectComparer());

                return set1.SetEquals(set2);
            }

            // Handle complex objects
            var properties = type1.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var value1 = property.GetValue(obj1);
                var value2 = property.GetValue(obj2);

                if (!AreEquivalent(value1, value2))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class ObjectComparer : IEqualityComparer<object>
    {
        public new bool Equals(object x, object y)
        {
            return CollectionComparer.AreEquivalent(x, y);
        }

        public int GetHashCode(object obj)
        {
            if (obj == null) return 0;
            return obj.GetHashCode();
        }
    }

}