using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DisplayMagicianShared;
using Microsoft.Win32;
using Newtonsoft.Json;

// This file is based on Soroush Falahati's amazing HeliosDisplayManagement software
// available at https://github.com/falahati/HeliosDisplayManagement

// Substantial modifications made by Terry MacDonald 2022 onwards

namespace DisplayMagicianShared.Windows
{
    public class TaskBarLayout
    {

        public enum TaskBarEdge : UInt32
        {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }

        [Flags]
        public enum TaskBarOptions : UInt32
        {
            None = 0,
            AutoHide = 1 << 0,
            KeepOnTop = 1 << 1,
            UseSmallIcons = 1 << 2,
            HideClock = 1 << 3,
            HideVolume = 1 << 4,
            HideNetwork = 1 << 5,
            HidePower = 1 << 6,
            WindowPreview = 1 << 7,
            Unknown1 = 1 << 8,
            Unknown2 = 1 << 9,
            HideActionCenter = 1 << 10,
            Unknown3 = 1 << 11,
            HideLocation = 1 << 12,
            HideLanguageBar = 1 << 13
        }

        private const string MainDisplayAddress =
            "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StuckRects{0:D}";

        private const string MultiDisplayAddress =
            "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\MMStuckRects{0:D}";

        /*private static readonly Dictionary<int, byte[]> Headers = new Dictionary<int, byte[]>
        {
            {2, new byte[] {0x28, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF}},
            {3, new byte[] {0x30, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0xFF, 0xFF}}
        };
*/
        public bool ReadFromRegistry(string regKeyValue, out bool retryNeeded)
        {
            retryNeeded = false;
            bool MMStuckRectVerFound = false;
            // Check if key exists
            int version = 3;
            string address = string.Format(MultiDisplayAddress, version);
            if (Registry.CurrentUser.OpenSubKey(address) != null)
            {
                MMStuckRectVerFound = true;
                SharedLogger.logger.Trace($"TaskBarLayout/ReadFromRegistry: Found MMStuckRect3 registry key! {address}");
            }
            else
            {
                // If it's not version 3, then try version 2
                version = 2;
                address = string.Format(MultiDisplayAddress, version);
                if (Registry.CurrentUser.OpenSubKey(address) != null)
                {
                    MMStuckRectVerFound = true;
                    SharedLogger.logger.Trace($"TaskBarLayout/ReadFromRegistry: Found MMStuckRect2 registry key! {address}");
                }
                else
                {
                    // It's not v2 or v3, so it must be a single display
                    MMStuckRectVerFound = false;
                    SharedLogger.logger.Warn($"TaskBarLayout/ReadFromRegistry: Couldn't find an MMStuckRect2 or MMStuckRect3 registry key! Going to test if it is a single display only.");
                }
            }

            if (MMStuckRectVerFound)
            {
                // Check if value exists
                if (version >= 2 && version <= 3)
                {

                    try
                    {
                        using (var key = Registry.CurrentUser.OpenSubKey(
                                address,
                                RegistryKeyPermissionCheck.ReadSubTree))
                        {
                            if (key.GetValueNames().Contains(regKeyValue))
                            {
                                var binary = key?.GetValue(regKeyValue) as byte[];
                                if (binary?.Length > 0)
                                {
                                    MainScreen = false;
                                    RegKeyValue = regKeyValue;
                                    Binary = binary;
                                    Version = version;

                                    // Extract the values from the binary byte field
                                    PopulateFieldsFromBinary();

                                    SharedLogger.logger.Trace($"TaskBarLayout/ReadFromRegistry: The taskbar for {RegKeyValue} is against the {Edge} edge, is positioned at ({TaskBarLocation.X},{TaskBarLocation.Y}) and is {TaskBarLocation.Width}x{TaskBarLocation.Height} in size.");

                                    // If we get here then we're done and don't need to continue with the rest of the code.
                                    return true;
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"TaskBarLayout/ReadFromRegistry: Unable to get the TaskBarStuckRectangle binary settings from {regKeyValue} screen. Screen details may not be available yet in registry.");
                                    retryNeeded = true;
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"TaskBarLayout/ReadFromRegistry: Unable to find {regKeyValue} key in {address}. Screen details may not be available yet in registry.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Trace(ex, $"TaskBarLayout/ReadFromRegistry: Exception while trying to open RegKey {address}. Unable to get the TaskBarStuckRectangle binary settings. Screen details may not be available yet in registry.");
                    }
                }
                else
                {
                    SharedLogger.logger.Error($"TaskBarLayout/ReadFromRegistry: A MMStuckRect entry was found, but the version of the field is wrong.");
                }
            }
            else
            {
                SharedLogger.logger.Trace($"TaskBarLayout/ReadFromRegistry: A MMStuckRect entry was NOT found. We will try to find the object in the StuckRect registry key instead");
            }

            bool StuckRectVerFound = false;
            // Check if string exists
            version = 3;
            address = string.Format(MainDisplayAddress, version);
            if (Registry.CurrentUser.OpenSubKey(address) != null)
            {
                StuckRectVerFound = true;
                SharedLogger.logger.Trace($"TaskBarLayout/ReadFromRegistry: Found StuckRect3 single display registry key! {address}");
            }
            else
            {
                // If it's not version 3, then try version 2
                version = 2;
                address = string.Format(MainDisplayAddress, version);
                if (Registry.CurrentUser.OpenSubKey(address) != null)
                {
                    StuckRectVerFound = true;
                    SharedLogger.logger.Trace($"TaskBarLayout/ReadFromRegistry: Found StuckRect2 single display registry key! {address}");
                }
                else
                {
                    SharedLogger.logger.Error($"TaskBarLayout/ReadFromRegistry: Couldn't find an single display StuckRect2 or StuckRect3 registry key! So we have to just return after doing nothing as there is nothing we can do.");
                    return false;
                }
            }

            if (StuckRectVerFound)
            {
                // Check if value exists
                if (version >= 2 && version <= 3)
                {
                    try
                    {
                        using (var key = Registry.CurrentUser.OpenSubKey(
                                address,
                                RegistryKeyPermissionCheck.ReadSubTree))
                        {
                            if (key.GetValueNames().Contains(regKeyValue))
                            {
                                var binary = key?.GetValue(regKeyValue) as byte[];
                                if (binary?.Length > 0)
                                {
                                    MainScreen = true;
                                    RegKeyValue = regKeyValue;
                                    Binary = binary;
                                    Version = version;

                                    // Extract the values from the binary byte field
                                    PopulateFieldsFromBinary();

                                    SharedLogger.logger.Trace($"TaskBarLayout/ReadFromRegistry: The taskbar for {RegKeyValue} is against the {Edge} edge, is positioned at ({TaskBarLocation.X},{TaskBarLocation.Y}) and is {TaskBarLocation.Width}x{TaskBarLocation.Height} in size.");
                                    return true;
                                }
                                else
                                {
                                    SharedLogger.logger.Error($"TaskBarLayout/ReadFromRegistry: Unable to get the TaskBarStuckRectangle binary settings from {regKeyValue} screen.");
                                    retryNeeded = true;
                                    return false;
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"TaskBarLayout/ReadFromRegistry: Unable to find {regKeyValue} key in {address}. Screen details may not be available yet in registry.");
                                return false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Trace(ex, $"TaskBarLayout/ReadFromRegistry: Exception2 while trying to open RegKey {address}. Unable to get the TaskBarStuckRectangle binary settings. Screen details may not be available yet in registry.");
                        return false;
                    }

                }
                else
                {
                    SharedLogger.logger.Error($"TaskBarLayout/ReadFromRegistry: A StuckRect entry was found, but the version of the field is wrong.");
                    return false;
                }
            }
            else
            {
                SharedLogger.logger.Error($"TaskBarLayout/ReadFromRegistry: A StuckRect entry was NOT found. This means we're unable to get the taskbar location, an unable to return a sensible TaskBarStuckRectangle object.");
                return false;
            }

        }

        public TaskBarLayout()
        {
        }

        public byte[] Binary { get; set; }

        public string RegKeyValue { get; set; }

        public bool MainScreen { get; set; }

        public UInt32 DPI { get; set; }

        public TaskBarEdge Edge { get; set; }

        public Rectangle TaskBarLocation { get; set; }

        public Rectangle StartMenuLocation { get; set; }

        public Rectangle MonitorLocation { get; set; }

        public Size MinSize { get; set; }

        public TaskBarOptions Options { get; set; }

        public uint Rows { get; set; }

        public int Version { get; set; }

        public override bool Equals(object obj) => obj is TaskBarLayout other && this.Equals(other);
        public bool Equals(TaskBarLayout other)
        {
            return Version == other.Version &&
                RegKeyValue == other.RegKeyValue &&
                MainScreen == other.MainScreen &&
                DPI == other.DPI &&
                Edge == other.Edge &&
                TaskBarLocation == other.TaskBarLocation &&
                MinSize == other.MinSize &&
                Options == other.Options &&
                Rows == other.Rows;
        }

        public override int GetHashCode()
        {
            return (Version, MainScreen, RegKeyValue, DPI, Edge, TaskBarLocation, MinSize, Options, Rows).GetHashCode();
        }
        public static bool operator ==(TaskBarLayout lhs, TaskBarLayout rhs) => lhs.Equals(rhs);

        public static bool operator !=(TaskBarLayout lhs, TaskBarLayout rhs) => !(lhs == rhs);

        static bool Xor(byte[] a, byte[] b)

        {

            int x = a.Length ^ b.Length;

            for (int i = 0; i < a.Length && i < b.Length; ++i)

            {

                x |= a[i] ^ b[i];

            }

            return x == 0;

        }

        private bool PopulateFieldsFromBinary()
        {
            if (Binary == null)
            {
                return false;
            }

            // Now we decipher the binary properties features to populate the stuckrectangle 
            // DPI 
            if (Binary.Length < 44)
            {
                DPI = 0;
            }
            else
            {
                DPI = BitConverter.ToUInt32(Binary, 40);
            }
            // Edge
            if (Binary.Length < 16)
            {
                Edge = TaskBarEdge.Bottom;
            }
            else
            {
                Edge = (TaskBarEdge)BitConverter.ToUInt32(Binary, 12);
            }
            // Location
            if (Binary.Length < 40)
            {
                TaskBarLocation = Rectangle.Empty;
            }
            else
            {
                var left = BitConverter.ToInt32(Binary, 24);
                var top = BitConverter.ToInt32(Binary, 28);
                var right = BitConverter.ToInt32(Binary, 32);
                var bottom = BitConverter.ToInt32(Binary, 36);

                TaskBarLocation = Rectangle.FromLTRB(left, top, right, bottom);
            }
            // MinSize
            if (Binary.Length < 24)
            {
                MinSize = Size.Empty;
            }
            else
            {
                var width = BitConverter.ToInt32(Binary, 16);
                var height = BitConverter.ToInt32(Binary, 20);

                MinSize = new Size(width, height);
            }
            // Options
            if (Binary.Length < 12)
            {
                Options = 0;
            }
            else
            {
                Options = (TaskBarOptions)BitConverter.ToUInt32(Binary, 8);
            }
            // Rows
            if (Binary.Length < 48)
            {
                Rows = 1;
            }
            else
            {
                Rows = BitConverter.ToUInt32(Binary, 44);
            }

            SharedLogger.logger.Trace($"TaskBarLayout/PopulateFieldsFromBinary: Grabbed the following settings for {RegKeyValue} from the registry: DPI = {DPI}, Edge = {Edge}, Location = ({TaskBarLocation.X},{TaskBarLocation.Y}), MinSize = {TaskBarLocation.Width}x{TaskBarLocation.Height}, Options = {Options}, Rows = {Rows}.");

            return true;
        }

        public bool PopulateBinaryFromFields()
        {
            if (Binary == null)
            {
                return false;
            }
            
            // Set the DPI
            if (Binary.Length < 44)
            {
                DPI = 0;
                var bytes = BitConverter.GetBytes(DPI);
                Array.Copy(bytes, 0, Binary, 40, 4);
            }
            else
            {
                var bytes = BitConverter.GetBytes(DPI);
                Array.Copy(bytes, 0, Binary, 40, 4);
            }
            // Edge
            if (Binary.Length < 16)
            {
                Edge = TaskBarEdge.Bottom;
                var bytes = BitConverter.GetBytes((uint)Edge);
                Array.Copy(bytes, 0, Binary, 12, 4);
            }
            else
            {
                var bytes = BitConverter.GetBytes((uint)Edge);
                Array.Copy(bytes, 0, Binary, 12, 4);
            }
            // Location
            if (Binary.Length < 40)
            {
                var bytes = BitConverter.GetBytes(0);
                Array.Copy(bytes, 0, Binary, 24, 4);

                bytes = BitConverter.GetBytes(0);
                Array.Copy(bytes, 0, Binary, 28, 4);

                bytes = BitConverter.GetBytes(0);
                Array.Copy(bytes, 0, Binary, 32, 4);

                bytes = BitConverter.GetBytes(0);
                Array.Copy(bytes, 0, Binary, 36, 4);
            }
            else
            {
                var bytes = BitConverter.GetBytes(TaskBarLocation.Left);
                Array.Copy(bytes, 0, Binary, 24, 4);

                bytes = BitConverter.GetBytes(TaskBarLocation.Top);
                Array.Copy(bytes, 0, Binary, 28, 4);

                bytes = BitConverter.GetBytes(TaskBarLocation.Right);
                Array.Copy(bytes, 0, Binary, 32, 4);

                bytes = BitConverter.GetBytes(TaskBarLocation.Bottom);
                Array.Copy(bytes, 0, Binary, 36, 4);
            }
            // MinSize
            if (Binary.Length < 24)
            {
                var bytes = BitConverter.GetBytes(0);
                Array.Copy(bytes, 0, Binary, 16, 4);

                bytes = BitConverter.GetBytes(0);
                Array.Copy(bytes, 0, Binary, 20, 4);
            }
            else
            {
                var bytes = BitConverter.GetBytes(MinSize.Width);
                Array.Copy(bytes, 0, Binary, 16, 4);

                bytes = BitConverter.GetBytes(MinSize.Height);
                Array.Copy(bytes, 0, Binary, 20, 4);
            }
            // Options
            if (Binary.Length < 12)
            {
                var bytes = BitConverter.GetBytes((uint)0);
                Array.Copy(bytes, 0, Binary, 8, 4);
            }
            else
            {
                var bytes = BitConverter.GetBytes((uint)Options);
                Array.Copy(bytes, 0, Binary, 8, 4);
            }
            // Rows
            if (Binary.Length < 48)
            {
                var bytes = BitConverter.GetBytes(1);
                Array.Copy(bytes, 0, Binary, 44, 4);
            }
            else
            {
                var bytes = BitConverter.GetBytes(Rows);
                Array.Copy(bytes, 0, Binary, 44, 4);
            }

            SharedLogger.logger.Trace($"TaskBarLayout/PopulateBinaryFromFields: Set the following settings for {RegKeyValue} into registry: DPI = {DPI}, Edge = {Edge}, Location = ({TaskBarLocation.X},{TaskBarLocation.Y}), MinSize = {TaskBarLocation.Width}x{TaskBarLocation.Height}, Options = {Options}, Rows = {Rows}.");

            return true;
        }

        public bool WriteToRegistry()
        {
            // Update the binary with the current settings from the object
            //PopulateBinaryFromFields();

            // Write the binary field to registry
            string address;
            if (MainScreen && RegKeyValue.Equals("Settings"))
            {
                address = string.Format(MainDisplayAddress, Version);
                // Set the Main Screen 
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(
                        address,
                        RegistryKeyPermissionCheck.ReadWriteSubTree))
                    {
                        key.SetValue(RegKeyValue, Binary);
                        SharedLogger.logger.Trace($"TaskBarLayout/Apply: Successfully applied TaskBarStuckRectangle registry settings for the {RegKeyValue} Screen in {address}!");
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"TaskBarLayout/GetCurrent: Unable to set the {RegKeyValue} TaskBarStuckRectangle registry settings in {address} due to an exception!");
                }
            }
            else
            {
                address = string.Format(MultiDisplayAddress, Version);
                // Grab the main screen taskbar placement
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(
                        address,
                        RegistryKeyPermissionCheck.ReadWriteSubTree))
                    {
                        key.SetValue(RegKeyValue, Binary);
                        SharedLogger.logger.Trace($"TaskBarLayout/WriteToRegistry: Successfully applied TaskBarStuckRectangle registry settings for the {RegKeyValue} Screen in {address}!");
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"TaskBarLayout/WriteToRegistry: Unable to set the {RegKeyValue} TaskBarStuckRectangle registry settings in {address} due to an exception!");
                }
            }

            return true;
        }

        public static Dictionary<string, TaskBarLayout> GetAllCurrentTaskBarLayouts(Dictionary<string, List<DISPLAY_SOURCE>> displaySources, out bool retryNeeded)
        {
            Dictionary<string, TaskBarLayout> taskBarStuckRectangles = new Dictionary<string, TaskBarLayout>();
            int state;
            bool tbsrReadWorked = false;

            APPBARDATA abd = new APPBARDATA();

            // Sleep delay just for testing so I can get the position of the Start Menu
            // Note for future me, the Start menu window is moved around the desktop to be next to the  start button that is pressed by the user.
            // e.g. if you have two screens, and you click the right most start button, the Start menu window is moved to be the same as the WorkRect of the 
            // monitor that the start button is on.
            //System.Threading.Thread.Sleep(5000);

            // Firstly try to get the position of the main screen and main start menu
            try
            {
                // Figure out which monitor this taskbar is on
                IntPtr mainTaskbarHwnd = Utils.FindWindow("Shell_TrayWnd", "");
                IntPtr mainMonitorHwnd = Utils.MonitorFromWindow(mainTaskbarHwnd, Utils.MONITOR_DEFAULTTOPRIMARY);
                //IntPtr startMenuHwnd = Utils.FindWindow("Windows.UI.Core.CoreWindow", "Start");

                //Utils.GetWindowRect(startMenuHwnd, out RECT lpRect);

                // Figure out the monitor coordinates
                MONITORINFOEX monitorInfo = new MONITORINFOEX();
                monitorInfo.cbSize = (UInt32)Marshal.SizeOf(typeof(MONITORINFOEX));
                //monitorInfo.szDevice = new char[Utils.CCHDEVICENAME];
                SharedLogger.logger.Trace($"TaskBarLayout/GetAllCurrentTaskBarPositions: Getting the monitor coordinates from the main monitor");
                Utils.GetMonitorInfo(mainMonitorHwnd, ref monitorInfo);

                abd.hWnd = mainTaskbarHwnd;
                abd.uEdge = ABEDGE.ABE_BOTTOM;
                abd.lParam = 0x1;
                abd.cbSize = Marshal.SizeOf(typeof(APPBARDATA));

                state = Utils.SHAppBarMessage(Utils.ABM_GETTASKBARPOS, ref abd);

                if (state == 1)
                {
                    int tbWidth = Math.Abs(abd.rc.left - abd.rc.right);
                    int tbHeight = Math.Abs(abd.rc.top - abd.rc.bottom);
                    int monWidth = Math.Abs(monitorInfo.rcMonitor.left - monitorInfo.rcMonitor.right);
                    int monHeight = Math.Abs(monitorInfo.rcMonitor.top - monitorInfo.rcMonitor.bottom);

                    TaskBarLayout tbsr = new TaskBarLayout();
                    // Now we're at the point that we should be able to update the binary that we grabbed earlier when the object was created
                    tbsrReadWorked = tbsr.ReadFromRegistry(GetRegKeyValueFromDevicePath(displaySources[monitorInfo.szDevice][0].DevicePath),out retryNeeded);
                    if (retryNeeded)
                    {
                        SharedLogger.logger.Error($"TaskBarLayout/GetAllCurrentTaskBarPositions: Taskbar read #1 from registry didn't work.");
                        retryNeeded = true;
                        return taskBarStuckRectangles;
                    }
                    tbsr.Edge = (TaskBarEdge)abd.uEdge;
                    tbsr.MonitorLocation = new System.Drawing.Rectangle(monitorInfo.rcMonitor.left, monitorInfo.rcMonitor.top, monWidth, monHeight);
                    switch (tbsr.Edge)
                    {
                        case TaskBarEdge.Left:
                            tbsr.TaskBarLocation = new System.Drawing.Rectangle(monitorInfo.rcMonitor.left, monitorInfo.rcMonitor.top, tbWidth, tbHeight);
                            break;
                        case TaskBarEdge.Top:
                            tbsr.TaskBarLocation = new System.Drawing.Rectangle(monitorInfo.rcMonitor.left, monitorInfo.rcMonitor.top, tbWidth, tbHeight);
                            break;
                        case TaskBarEdge.Right:
                            tbsr.TaskBarLocation = new System.Drawing.Rectangle(monitorInfo.rcWork.right, monitorInfo.rcWork.top, tbWidth, tbHeight);
                            break;
                        case TaskBarEdge.Bottom:
                            tbsr.TaskBarLocation = new System.Drawing.Rectangle(monitorInfo.rcWork.left, monitorInfo.rcWork.bottom, tbWidth, tbHeight);
                            break;
                        default:
                            // Default is bottom taskbar
                            tbsr.TaskBarLocation = new System.Drawing.Rectangle(monitorInfo.rcWork.left, monitorInfo.rcWork.bottom, tbWidth, tbHeight);
                            break;
                    }
                    tbsr.MainScreen = true;

                    // Now as a LAST step we update the Binary field just before we apply it to make sure that the correct binary settings are stored
                    tbsr.PopulateBinaryFromFields();

                    taskBarStuckRectangles.Add(monitorInfo.szDevice, tbsr);

                    // If it's a main screen, also add a duplicate so we track the main StuckRects settings separately too
                    TaskBarLayout tbsrMain = new TaskBarLayout();
                    tbsrReadWorked = tbsrMain.ReadFromRegistry("Settings",out retryNeeded) ;
                    if (!retryNeeded)
                    {
                        SharedLogger.logger.Error($"TaskBarLayout/GetAllCurrentTaskBarPositions: Taskbar read #1 from registry didn't work.");
                        retryNeeded = true;
                        return taskBarStuckRectangles;
                    }
                    tbsrMain.Edge = tbsr.Edge;
                    tbsrMain.MonitorLocation = tbsr.MonitorLocation;
                    tbsrMain.TaskBarLocation = tbsr.TaskBarLocation;
                    tbsrMain.MainScreen = tbsr.MainScreen;

                    SharedLogger.logger.Trace($"TaskBarLayout/GetAllCurrentTaskBarPositions: Main monitor coordinates are {tbsrMain.MonitorLocation.X},{tbsrMain.MonitorLocation.Y} and it is {tbsrMain.MonitorLocation.Width}x{tbsrMain.MonitorLocation.Height}");
                    SharedLogger.logger.Trace($"TaskBarLayout/GetAllCurrentTaskBarPositions: Main taskbar coordinates are {tbsrMain.TaskBarLocation.X},{tbsrMain.TaskBarLocation.Y} and it is {tbsrMain.TaskBarLocation.Width}x{tbsrMain.TaskBarLocation.Height}");
                    SharedLogger.logger.Trace($"TaskBarLayout/GetAllCurrentTaskBarPositions: Main taskbar is {tbsrMain.Edge.ToString("G")}");

                    // Now as a LAST step we update the Binary field just before we apply it to make sure that the correct binary settings are stored
                    tbsrMain.PopulateBinaryFromFields();
                    taskBarStuckRectangles.Add("Settings", tbsrMain);
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"TaskBarLayout/GetAllCurrentTaskBarPositions: Exception while trying to get the main taskbar position");
            }

            // Then go through the secondary windows and get the position of them
            // Tell Windows to refresh the Other Windows Taskbars if needed
            int clonedCount = 0;
            try
            {
                IntPtr lastTaskBarWindowHwnd = (IntPtr)Utils.NULL;
                for (int i = 0; i < 100; i++)
                {
                    // Find the next "Shell_SecondaryTrayWnd" window 
                    IntPtr nextTaskBarWindowHwnd = Utils.FindWindowEx((IntPtr)Utils.NULL, lastTaskBarWindowHwnd, "Shell_SecondaryTrayWnd", null);
                    if (nextTaskBarWindowHwnd == (IntPtr)Utils.NULL)
                    {
                        // No more windows taskbars to notify
                        break;
                    }

                    IntPtr secMonitorHwnd = Utils.MonitorFromWindow(nextTaskBarWindowHwnd, Utils.MONITOR_DEFAULTTONEAREST);

                    // Figure out the monitor coordinates
                    MONITORINFOEX monitorInfo = new MONITORINFOEX();
                    monitorInfo.cbSize = (UInt32)Marshal.SizeOf(typeof(MONITORINFOEX));
                    //monitorInfo.szDevice = new char[Utils.CCHDEVICENAME];
                    Utils.GetMonitorInfo(secMonitorHwnd, ref monitorInfo);

                    // Figure out the position of the taskbar ourselves
                    int monWidth = Math.Abs(monitorInfo.rcMonitor.left - monitorInfo.rcMonitor.right);
                    int monHeight = Math.Abs(monitorInfo.rcMonitor.top - monitorInfo.rcMonitor.bottom);
                    int wrkWidth = Math.Abs(monitorInfo.rcWork.left - monitorInfo.rcWork.right);
                    int wrkHeight = Math.Abs(monitorInfo.rcWork.top - monitorInfo.rcWork.bottom);
                    int tbWidth;
                    int tbHeight;

                    TaskBarLayout tbsr = new TaskBarLayout();
                    // Now we're at the point that we should be able to update the binary that we grabbed earlier when the object was created
                    tbsrReadWorked = tbsr.ReadFromRegistry(GetRegKeyValueFromDevicePath(displaySources[monitorInfo.szDevice][0].DevicePath), out retryNeeded);
                    if (!tbsrReadWorked)
                    {
                        SharedLogger.logger.Error($"TaskBarLayout/GetAllCurrentTaskBarPositions: Taskbar read #3 from registry didn't work.");
                        retryNeeded = true;
                        return taskBarStuckRectangles;
                    }

                    if (monWidth == wrkWidth)
                    {
                        // Taskbar on top or bottom
                        if (monitorInfo.rcMonitor.left == monitorInfo.rcWork.left && monitorInfo.rcMonitor.top == monitorInfo.rcWork.top)
                        {
                            // Taskbar on bottom
                            tbWidth = monWidth;
                            tbHeight = monHeight - wrkHeight;
                            tbsr.TaskBarLocation = new System.Drawing.Rectangle(monitorInfo.rcMonitor.left, monitorInfo.rcWork.bottom, tbWidth, tbHeight);
                            tbsr.Edge = TaskBarEdge.Bottom;
                        }
                        else if (monitorInfo.rcMonitor.right == monitorInfo.rcWork.right && monitorInfo.rcMonitor.bottom == monitorInfo.rcWork.bottom)
                        {
                            // Taskbar on top
                            tbWidth = monWidth;
                            tbHeight = monHeight - wrkHeight;
                            tbsr.TaskBarLocation = new System.Drawing.Rectangle(monitorInfo.rcWork.left, monitorInfo.rcMonitor.top, tbWidth, tbHeight);
                            tbsr.Edge = TaskBarEdge.Top;
                        }
                        else
                        {
                            // Invalid state
                            SharedLogger.logger.Error($"TaskBarLayout/GetAllCurrentTaskBarPositions: Taskbar position was not on a horizontal edge of a monitor!");
                            SharedLogger.logger.Error($"TaskBarLayout/GetAllCurrentTaskBarPositions: Forcing Taskbar position to be at the bottom");
                            tbWidth = monWidth;
                            tbHeight = monHeight - wrkHeight;
                            tbsr.TaskBarLocation = new System.Drawing.Rectangle(monitorInfo.rcMonitor.left, monitorInfo.rcWork.bottom, tbWidth, tbHeight);
                            tbsr.Edge = TaskBarEdge.Bottom;
                        }

                    }
                    else if (monHeight == wrkHeight)
                    {
                        // Taskbar on the sides
                        if (monitorInfo.rcMonitor.right == monitorInfo.rcWork.right && monitorInfo.rcMonitor.bottom == monitorInfo.rcWork.bottom)
                        {
                            // Taskbar on left
                            tbWidth = monWidth - wrkWidth;
                            tbHeight = monHeight;
                            tbsr.TaskBarLocation = new System.Drawing.Rectangle(monitorInfo.rcMonitor.left, monitorInfo.rcMonitor.top, tbWidth, tbHeight);
                            tbsr.Edge = TaskBarEdge.Left;
                        }
                        else if (monitorInfo.rcMonitor.left == monitorInfo.rcWork.left && monitorInfo.rcMonitor.top == monitorInfo.rcWork.top)
                        {
                            // Taskbar on right
                            tbWidth = monWidth - wrkWidth;
                            tbHeight = monHeight;
                            tbsr.TaskBarLocation = new System.Drawing.Rectangle(monitorInfo.rcWork.right, monitorInfo.rcMonitor.top, tbWidth, tbHeight);
                            tbsr.Edge = TaskBarEdge.Right;
                        }
                        else
                        {
                            // Invalid state
                            SharedLogger.logger.Error($"TaskBarLayout/GetAllCurrentTaskBarPositions: Taskbar position was not on a vertical edge of a monitor!");
                            SharedLogger.logger.Error($"TaskBarLayout/GetAllCurrentTaskBarPositions: Forcing Taskbar position to be at the bottom");
                            tbWidth = monWidth;
                            tbHeight = monHeight - wrkHeight;
                            tbsr.TaskBarLocation = new System.Drawing.Rectangle(monitorInfo.rcMonitor.left, monitorInfo.rcWork.bottom, tbWidth, tbHeight);
                            tbsr.Edge = TaskBarEdge.Bottom;
                        }
                    }
                    else
                    {
                        // Invalid state
                        SharedLogger.logger.Error($"TaskBarLayout/GetAllCurrentTaskBarPositions: Taskbar position was not fully along one of the monitor edges!");
                        SharedLogger.logger.Error($"TaskBarLayout/GetAllCurrentTaskBarPositions: Forcing Taskbar position to be at the bottom");
                        tbWidth = monWidth;
                        tbHeight = monHeight - wrkHeight;
                        tbsr.TaskBarLocation = new System.Drawing.Rectangle(monitorInfo.rcMonitor.left, monitorInfo.rcWork.bottom, tbWidth, tbHeight);
                        tbsr.Edge = TaskBarEdge.Bottom;
                    }

                    tbsr.MonitorLocation = new System.Drawing.Rectangle(monitorInfo.rcMonitor.left, monitorInfo.rcMonitor.top, monWidth, monHeight);
                    tbsr.MainScreen = false;

                    SharedLogger.logger.Trace($"TaskBarLayout/GetAllCurrentTaskBarPositions: Secondary monitor coordinates are {tbsr.MonitorLocation.X},{tbsr.MonitorLocation.Y} and it is {tbsr.MonitorLocation.Width}x{tbsr.MonitorLocation.Height}");
                    SharedLogger.logger.Trace($"TaskBarLayout/GetAllCurrentTaskBarPositions: Secondary taskbar coordinates are {tbsr.TaskBarLocation.X},{tbsr.TaskBarLocation.Y} and it is {tbsr.TaskBarLocation.Width}x{tbsr.TaskBarLocation.Height}");
                    SharedLogger.logger.Trace($"TaskBarLayout/GetAllCurrentTaskBarPositions: Secondary taskbar is {tbsr.Edge.ToString("G")}");

                    // Now as a LAST step we update the Binary field to make sure that the correct binary settings are stored
                    // This means the correct location should be returned even if the registry isn't updated as we're patching the registry object before we store it.
                    tbsr.PopulateBinaryFromFields();

                    if (!taskBarStuckRectangles.ContainsKey(monitorInfo.szDevice))
                    {
                        taskBarStuckRectangles.Add(monitorInfo.szDevice, tbsr);
                    }
                    else
                    {
                        SharedLogger.logger.Error($"TaskBarLayout/GetAllCurrentTaskBarPositions: Skipping grabbing Taskbar position from a cloned display {monitorInfo.szDevice}");
                        clonedCount++;
                    }

                    // Prep the next taskbar window so we continue through them
                    lastTaskBarWindowHwnd = nextTaskBarWindowHwnd;
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"TaskBarLayout/GetAllCurrentTaskBarPositions: Exception while trying to get a secondary taskbar position");
            }

            // Check if the display reg keys shown match the display sources
            foreach (var tbrKey in taskBarStuckRectangles.Keys)
            {
                if (tbrKey.Equals("Settings"))
                {
                    continue;
                }
                // If there isn't a match then we have a problem.
                if (!displaySources.ContainsKey(tbrKey))
                {
                    SharedLogger.logger.Error($"TaskBarLayout/GetAllCurrentTaskBarPositions: We have an error because Display Sources array doesn't include the {tbrKey} taskbar data. This means we have a mismatch somewhere.");
                    retryNeeded = true;
                }
            }
          
            retryNeeded = false;
            return taskBarStuckRectangles;
        }

        public bool MoveTaskBar()
        {
            if (RegKeyValue.Equals("Settings") && MainScreen)
            {
                // We only want to set the position for the main screen if it has a "Settings" entry and is a main screen
                // Find the window to move
                IntPtr mainTaskbarHwnd = Utils.FindWindow("Shell_TrayWnd", "");
                //IntPtr startButtonHandle = Utils.FindWindowEx(mainTaskbarHwnd, IntPtr.Zero, "Start", null);
                IntPtr systemTrayNotifyHandle = Utils.FindWindowEx(mainTaskbarHwnd, IntPtr.Zero, "TrayNotifyWnd", null);
                //IntPtr rebarWindowHandle = Utils.FindWindowEx(mainTaskbarHwnd, IntPtr.Zero, "ReBarWindow32", null);
                //IntPtr trayDesktopShowButtonHandle = Utils.FindWindowEx(systemTrayNotifyHandle, IntPtr.Zero, "TrayShowDesktopButtonWClass", null);

                IntPtr result;

                // ===== MOVE THE MAIN TASKBAR WINDOW =====
                // Prepare the taskbar for moving
                Utils.SendMessageTimeout(mainTaskbarHwnd, Utils.WM_ENTERSIZEMOVE, IntPtr.Zero, IntPtr.Zero, SendMessageTimeoutFlag.SMTO_NORMAL, 10, out result);
                // Move the taskbar window
                Utils.MoveWindow(mainTaskbarHwnd, TaskBarLocation.X, TaskBarLocation.Y, TaskBarLocation.Width, TaskBarLocation.Height, false);

                // ===== LOCK THE MAIN TASKBAR WINDOW BACK DOWN =====
                // Tell the taskbar we've stopped moving it now
                Utils.SendMessageTimeout(mainTaskbarHwnd, Utils.WM_EXITSIZEMOVE, IntPtr.Zero, IntPtr.Zero, SendMessageTimeoutFlag.SMTO_NORMAL, 10, out result);
                // Tell the taskbar it needs to update it's theme
                Utils.PostMessage(mainTaskbarHwnd, Utils.WM_THEMECHANGED, IntPtr.Zero, IntPtr.Zero);
                // Tell the taskbar it needs to recalculate it's work area
                Utils.SendMessageTimeout(systemTrayNotifyHandle, Utils.WM_SETTINGCHANGE, (IntPtr)Utils.SPI_SETWORKAREA, IntPtr.Zero, SendMessageTimeoutFlag.SMTO_NORMAL, 10, out result);

                // We also save the taskbar position for the monitor in registry, so that Windows will actually properly update the position 
                // after 5 seconds (and this one will stick between reboots too!).
                WriteToRegistry();

            }
            else if (MainScreen && !RegKeyValue.Equals("Settings"))
            {
                // If it's a main screen, but not "settings", then its the registry key only taskbar setting we need to change
                // This is because hte only screen settings that matter are the StuckRect3 registry key (for the main screen) and
                // all of the secondary windows
                WriteToRegistry();
            }
            else
            {
                // This is a secondary screen, so we need to set it's position
                // Then go through the secondary windows and get the position of them
                // Tell Windows to refresh the Other Windows Taskbars if needed
                //WriteToRegistry();

                IntPtr mainTaskbarHwnd = Utils.FindWindow("Shell_TrayWnd", "");

                IntPtr lastTaskBarWindowHwnd = (IntPtr)Utils.NULL;
                for (int i = 0; i < 100; i++)
                {
                    // Find the next "Shell_SecondaryTrayWnd" window 
                    IntPtr nextTaskBarWindowHwnd = Utils.FindWindowEx((IntPtr)Utils.NULL, lastTaskBarWindowHwnd, "Shell_SecondaryTrayWnd", null);
                    if (nextTaskBarWindowHwnd == (IntPtr)Utils.NULL)
                    {
                        // No more windows taskbars to notify
                        break;
                    }

                    IntPtr secMonitorHwnd = Utils.MonitorFromWindow(nextTaskBarWindowHwnd, Utils.MONITOR_DEFAULTTONEAREST);

                    // Figure out this monitor coordinates
                    MONITORINFOEX monitorInfo = new MONITORINFOEX();
                    monitorInfo.cbSize = (UInt32)Marshal.SizeOf(typeof(MONITORINFOEX));
                    //monitorInfo.szDevice = new char[Utils.CCHDEVICENAME];
                    Utils.GetMonitorInfo(secMonitorHwnd, ref monitorInfo);

                    // Figure out the position of the taskbar ourselves
                    int monWidth = Math.Abs(monitorInfo.rcMonitor.left - monitorInfo.rcMonitor.right);
                    int monHeight = Math.Abs(monitorInfo.rcMonitor.top - monitorInfo.rcMonitor.bottom);
                    Rectangle thisMonitorLocation = new Rectangle(monitorInfo.rcMonitor.left, monitorInfo.rcMonitor.top, monWidth, monHeight);
                    if (MonitorLocation.Equals(thisMonitorLocation))
                    {
                        // This is the right monitor, so we should move the taskbar on it.

                        IntPtr result;

                        // ===== MOVE THE MAIN TASKBAR WINDOW =====
                        // Prepare the taskbar for moving
                        Utils.SendMessageTimeout(nextTaskBarWindowHwnd, Utils.WM_ENTERSIZEMOVE, IntPtr.Zero, IntPtr.Zero, SendMessageTimeoutFlag.SMTO_NORMAL, 10, out result);
                        // Move the taskbar window
                        Utils.MoveWindow(nextTaskBarWindowHwnd, TaskBarLocation.X, TaskBarLocation.Y, TaskBarLocation.Width, TaskBarLocation.Height, true);

                        // ===== LOCK THE MAIN TASKBAR WINDOW BACK DOWN =====
                        // Tell the taskbar we've stopped moving it now
                        //Utils.SendMessageTimeout(nextTaskBarWindowHwnd, Utils.WM_EXITSIZEMOVE, IntPtr.Zero, IntPtr.Zero, SendMessageTimeoutFlag.SMTO_NORMAL, 10, out result);
                        // Tell the taskbar it needs to update it's theme
                        //Utils.PostMessage(nextTaskBarWindowHwnd, Utils.WM_THEMECHANGED, IntPtr.Zero, IntPtr.Zero);
                        // Tell the taskbar it needs to recalculate it's work area
                        //Utils.SendMessageTimeout(nextTaskBarWindowHwnd, Utils.WM_SETTINGCHANGE, (IntPtr)Utils.SPI_SETWORKAREA, IntPtr.Zero, SendMessageTimeoutFlag.SMTO_NORMAL, 10, out result);

                        // We also save the taskbar position for the monitor in registry, so that Windows will actually properly update the position 
                        // after 5 seconds (and this one will stick between reboots too!).
                        WriteToRegistry();


                        // We then want to stop as we've found the correct taskbar to move!
                        break;

                    }

                    // Prep the next taskbar window so we continue through them
                    lastTaskBarWindowHwnd = nextTaskBarWindowHwnd;
                }
            }

            return true;
        }


        public static string GetRegKeyValueFromDevicePath(string devicePath)
        {
            string regKeyValue = "";
            // e.g. "\\\\?\\DISPLAY#NVS10DE#5&2b46c695&0&UID185344#{e6f07b5f-ee97-4a90-b076-33f57bf4eaa7}"
            string pattern = @"DISPLAY\#(.*)\#\{";
            Match match = Regex.Match(devicePath, pattern);
            if (match.Success)
            {
                regKeyValue = match.Groups[1].Value;
                SharedLogger.logger.Trace($"TaskBarLayout/GetRegKeyValueFromDevicePath: Found regKeyValue {regKeyValue } in the devicePath {devicePath }.");
            }
            else
            {
                SharedLogger.logger.Warn($"TaskBarLayout/GetRegKeyValueFromDevicePath: We were unable to figure out the regKeyValue {regKeyValue } in the devicePath {devicePath }..");
            }
            return regKeyValue;
        }

        public static bool ForceTaskBarRedraw(IntPtr mainToolBarHWnd)
        {
            // Tell Windows to refresh the Main Screen Windows Taskbar registry settings by telling Explorer to update.
            Utils.SendMessage(mainToolBarHWnd, Utils.WM_SETTINGCHANGE, (IntPtr)Utils.SPI_SETWORKAREA, (IntPtr)Utils.NULL);
            // Tell Windows to refresh the child Windows in the taskbar
            IntPtr lastChildWindowHwnd = (IntPtr)Utils.NULL;
            for (int i = 0; i < 100; i++)
            {
                // Find the next "Shell_SecondaryTrayWnd" window 
                IntPtr nextChildWindowHwnd = Utils.FindWindowEx((IntPtr)Utils.NULL, mainToolBarHWnd, "", null);
                if (nextChildWindowHwnd == (IntPtr)Utils.NULL)
                {
                    // No more windows taskbars to notify
                    break;
                }
                // Send the "Shell_TrayWnd" window a WM_SETTINGCHANGE with a wParameter of SPI_SETWORKAREA
                Utils.SendMessage(lastChildWindowHwnd, Utils.WM_SETTINGCHANGE, (IntPtr)Utils.SPI_SETWORKAREA, (IntPtr)Utils.NULL);
                lastChildWindowHwnd = nextChildWindowHwnd;
            }

            //IntPtr explorerToolBarHWnd = Utils.FindWindow("Shell_TrayWnd", null);
            //Utils.PostMessage((IntPtr)Utils.HWND_BROADCAST, Utils.SHELLHOOK, 0x13, (int) mainToolBarHWnd);
            //Utils.PostMessage((IntPtr)Utils.HWND_BROADCAST, Utils.WM_SETTINGCHANGE, (int)Utils.SPI_SETWORKAREA, (int)Utils.NULL);
            /*IntPtr result;
            Utils.SendMessageTimeout((IntPtr)Utils.HWND_BROADCAST, Utils.WM_USER_1, (IntPtr)Utils.NULL, (IntPtr)Utils.NULL, Utils.SendMessageTimeoutFlag.SMTO_ABORTIFHUNG, 15, out result);*/
            return true;
        }

    }

    [global::System.Serializable]
    public class TaskBarStuckRectangleException : Exception
    {
        public TaskBarStuckRectangleException() { }
        public TaskBarStuckRectangleException(string message) : base(message) { }
        public TaskBarStuckRectangleException(string message, Exception inner) : base(message, inner) { }
        protected TaskBarStuckRectangleException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}