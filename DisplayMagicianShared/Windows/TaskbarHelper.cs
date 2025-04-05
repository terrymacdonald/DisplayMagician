using DisplayMagicianShared;
using DisplayMagicianShared.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using static DisplayMagicianShared.Windows.TaskbarHelper;

namespace DisplayMagicianShared.Windows
{

    public enum TaskbarPosition
    {
        Left,
        Top,
        Right,
        Bottom,
        Unknown
    }

    //[JsonConverter(typeof(CustomRectConverter))]
    [TypeConverter(typeof(RectTypeConverter))]
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        [JsonProperty]
        public int Left;
        [JsonProperty] 
        public int Top;
        [JsonProperty] 
        public int Right;
        [JsonProperty] 
        public int Bottom;

        public int X => Left;
        public int Y => Top;
        public int Width => Right - Left;
        public int Height => Bottom - Top;

        public Rect(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        public override string ToString()
        {
            return $"{Left}#{Top}#{Right}#{Bottom}";
        }

        public static Rect FromXYWH(int x, int y, int width, int height)
        {
            return new Rect(x, y, x + width, y + height);
        }

        public override bool Equals(object obj) => obj is Rect other && this.Equals(other);
        public bool Equals(Rect other)
            => Left == other.Left &&
               Top == other.Top &&
               Right == other.Right &&
               Bottom == other.Bottom;

        public override int GetHashCode()
        {
            return (Left, Top, Right, Bottom).GetHashCode();
        }

        public static bool operator ==(Rect lhs, Rect rhs) => lhs.Equals(rhs);

        public static bool operator !=(Rect lhs, Rect rhs) => !(lhs == rhs);
    }


    public static class TaskbarHelper
    {
        // Constants for SHChangeNotify
        private const uint SHCNE_ASSOCCHANGED = 0x8000000;
        private const uint SHCNF_IDLIST = 0x0;

        // Constants for SetDisplayConfig
        private const uint QDC_ONLY_ACTIVE_PATHS = 0x00000002;
        private const uint SDC_APPLY = 0x00000080;
        private const uint SDC_USE_SUPPLIED_DISPLAY_CONFIG = 0x00000020;
        private const uint SDC_ALLOW_CHANGES = 0x00000004;

        // P/Invoke for SHChangeNotify
        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        // P/Invoke for QueryDisplayConfig
        [DllImport("user32.dll")]
        private static extern int QueryDisplayConfig(
            uint flags,
            ref uint numPathArrayElements,
            [Out] DISPLAYCONFIG_PATH_INFO[] pathInfoArray,
            ref uint numModeInfoArrayElements,
            [Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
            IntPtr currentTopologyId
        );

        // P/Invoke for SetDisplayConfig
        [DllImport("user32.dll")]
        private static extern int SetDisplayConfig(
            uint numPathArrayElements,
            [In] DISPLAYCONFIG_PATH_INFO[] pathArray,
            uint numModeInfoArrayElements,
            [In] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
            uint flags
        );

        // P/Invoke for Restart Manager API
        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
        private static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmRegisterResources(uint dwSessionHandle,
            uint nFiles, string[] rgsFilenames,
            uint nApplications, RM_UNIQUE_PROCESS[] rgApplications,
            uint nServices, string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmShutdown(uint pSessionHandle, uint lActionFlags, IntPtr fnStatus);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmRestart(uint pSessionHandle, uint dwRestartFlags, IntPtr fnStatus);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmEndSession(uint pSessionHandle);

        // Structure for RM_UNIQUE_PROCESS
        [StructLayout(LayoutKind.Sequential)]
        private struct RM_UNIQUE_PROCESS
        {
            public int dwProcessId;
            public FILETIME ProcessStartTime;
        }

        // Structure for FILETIME
        [StructLayout(LayoutKind.Sequential)]
        private struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(Point pt, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);        

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public uint cbSize;
            public Rect rcMonitor;
            public Rect rcWork;
            public uint dwFlags;
        }

        /// <summary>
        /// Forces the taskbar to redraw by notifying the shell, reapplying display configurations,
        /// and restarting Explorer if necessary.
        /// </summary>
        public static void ForceTaskbarRedraw()
        {
            SharedLogger.logger.Trace("TasbkbarHelper/ForceTaskbarRedraw: Waiting 500ms for the main display layout changes to take effect.");
            Thread.Sleep(500);

            if (!AreTaskbarsVisibleOnAllScreens())
            {
                SharedLogger.logger.Trace("TasbkbarHelper/ForceTaskbarRedraw: Windows Taskbar is missing from a display. It is neither shown onscreen nor hidden. We need to try and get that taskbar working again.");
                SharedLogger.logger.Trace("TasbkbarHelper/ForceTaskbarRedraw: Attempting to get Windows to redraw the screen by sending a windows message that the screen layout has changed.");
                NudgeShellToRefresh();
                SharedLogger.logger.Trace("TasbkbarHelper/ForceTaskbarRedraw: Waiting 500ms for the windows message to take effect.");
                Thread.Sleep(100);
                // Check if taskbars are visible on all screens before reapplying the display config again
                if (!AreTaskbarsVisibleOnAllScreens())
                {
                    SharedLogger.logger.Trace("TasbkbarHelper/ForceTaskbarRedraw: The windows taskbar is still missing from a display. Attempting to get Windows to reapply the current display layout to force it to redraw the screens.");
                    ReapplyCurrentDisplayConfig();
                    SharedLogger.logger.Trace("TasbkbarHelper/ForceTaskbarRedraw: Waiting 500ms for the windows message to take effect.");
                    Thread.Sleep(100);
                }
                // Check if taskbars are visible on all screens before restarting Explorer
                if (!AreTaskbarsVisibleOnAllScreens())
                {
                    SharedLogger.logger.Trace("TasbkbarHelper/ForceTaskbarRedraw: The windows taskbar is still missing from a display. Attempting to get Windows to restart windows explorer using windows restart manager so that explorer keeps it's current configuration.");
                    RestartExplorer();
                    SharedLogger.logger.Trace("TasbkbarHelper/ForceTaskbarRedraw: Waiting 500ms for the windows explorer restart to take effect.");
                    Thread.Sleep(100);
                }
                // Check if taskbars are visible on all screens before restarting Explorer
                if (AreTaskbarsVisibleOnAllScreens())
                {
                    SharedLogger.logger.Trace("TasbkbarHelper/ForceTaskbarRedraw: All windows taskbars are available (but may be set to auto-hide).");
                }
                else
                {
                    SharedLogger.logger.Trace("TasbkbarHelper/ForceTaskbarRedraw: At least one windows taskbar is still missing from a display, even though we tried getting it back. THere is nothign further we can do in this instance.");
                }
            }
            else
            {
                SharedLogger.logger.Trace("TasbkbarHelper/ForceTaskbarRedraw: All the Windows Taskbars are available (either shown onscreen or hidden).");
            }
        }

        public static IEnumerable<(Rect MonitorBounds, Rect WorkArea)> GetAllMonitorAreas()
        {
            var monitorAreas = new List<(Rect, Rect)>();

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData) =>
            {
                var mi = new MONITORINFO { cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO)) };
                if (GetMonitorInfo(hMonitor, ref mi))
                {
                    monitorAreas.Add((mi.rcMonitor, mi.rcWork));
                }
                return true;
            }, IntPtr.Zero);

            return monitorAreas;
        }

        public static bool AreTaskbarsVisibleOnAllScreens()
        {
            foreach (var (monitorBounds, workArea) in GetAllMonitorAreas())
            {
                if (monitorBounds.Width != workArea.Width || monitorBounds.Height != workArea.Height)
                {
                    // Taskbar detected on this monitor
                }
                else
                {
                    // No taskbar detected on this monitor
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Notifies the shell to refresh UI elements.
        /// </summary>
        private static void NudgeShellToRefresh()
        {
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Reapplies the current display configuration to ensure settings are applied correctly.
        /// </summary>
        private static void ReapplyCurrentDisplayConfig()
        {

            // Step 1: Retrieve the required buffer sizes for path and mode information
            WIN32STATUS error = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out int pathCount, out int modeCount);
            if (error != WIN32STATUS.ERROR_SUCCESS)
            {
                Console.WriteLine($"GetDisplayConfigBufferSizes failed with error {error}");
                return;
            }

            // Step 2: Allocate arrays based on the retrieved sizes
            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

            // Step 3: Retrieve the current display configurations
            error = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (error != WIN32STATUS.ERROR_SUCCESS)
            {
                Console.WriteLine($"QueryDisplayConfig failed with error {error}");
                return;
            }

            // Step 4: Reapply the current display configurations
            error = CCDImport.SetDisplayConfig((uint)pathCount, paths, (uint)modeCount, modes,
                SDC.SDC_APPLY | SDC.SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC.SDC_ALLOW_CHANGES);
            if (error != WIN32STATUS.ERROR_SUCCESS)
            {
                Console.WriteLine($"SetDisplayConfig failed with error {error}");
            }
        }

        /// <summary>
        /// Gracefully restarts the Explorer process using the Restart Manager API.
        /// </summary>
        private static void RestartExplorer()
        {
            uint sessionHandle;
            string sessionKey = Guid.NewGuid().ToString();

            int result = RmStartSession(out sessionHandle, 0, sessionKey);
            if (result != 0)
            {
                Console.WriteLine("Could not start Restart Manager session.");
                return;
            }

            try
            {
                string[] resources = { @"C:\Windows\explorer.exe" };
                result = RmRegisterResources(sessionHandle, (uint)resources.Length, resources, 0, null, 0, null);
                if (result != 0)
                {
                    Console.WriteLine("Could not register resources.");
                    return;
                }

                result = RmShutdown(sessionHandle, 0, IntPtr.Zero);
                if (result != 0)
                {
                    Console.WriteLine("Could not shut down applications.");
                    return;
                }

                result = RmRestart(sessionHandle, 0, IntPtr.Zero);
                if (result != 0)
                {
                    Console.WriteLine("Could not restart applications.");
                }
            }
            finally
            {
                RmEndSession(sessionHandle);
            }


        }

        /// <summary>
        /// Returns the edge of each monitor's screen where the taskbar is docked.
        /// </summary>
        public static Dictionary<Rect, TaskbarPosition> GetTaskbarPositions()
        {
            var positions = new Dictionary<Rect, TaskbarPosition>();

            foreach (var (monitorBounds, workArea) in GetAllMonitorAreas())
            {
                var position = GetTaskbarPositionPerMonitor(monitorBounds, workArea);
                positions[monitorBounds] = position;
            }

            return positions;
        }

        /// <summary>
        /// Determines the edge where the taskbar is docked on a single monitor.
        /// </summary>
        private static TaskbarPosition GetTaskbarPositionPerMonitor(Rect bounds, Rect work)
        {
            if (bounds.Width > work.Width)
            {
                // Taskbar is on left or right
                if (work.Left > bounds.Left)
                    return TaskbarPosition.Left;
                else if (work.Right < bounds.Right)
                    return TaskbarPosition.Right;
            }
            else if (bounds.Height > work.Height)
            {
                // Taskbar is on top or bottom
                if (work.Top > bounds.Top)
                    return TaskbarPosition.Top;
                else if (work.Bottom < bounds.Bottom)
                    return TaskbarPosition.Bottom;
            }

            return TaskbarPosition.Unknown;
        }
    }



    public class RectTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string rectString)
            {
                var parts = rectString.Split('#');
                if (parts.Length == 4 &&
                    int.TryParse(parts[0], out int left) &&
                    int.TryParse(parts[1], out int top) &&
                    int.TryParse(parts[2], out int right) &&
                    int.TryParse(parts[3], out int bottom))
                {
                    return new Rect(left, top, right, bottom);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Rect rect)
            {
                return $"{rect.Left}#{rect.Top}#{rect.Right}#{rect.Bottom}";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }


    /* #region JsonConverterRect
     public class CustomRectConverter : JsonConverter
     {
         public override bool CanConvert(Type objectType)
         {
             return objectType == typeof(Rect);
         }

         public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
         {
             string rectString = (string)reader.Value;

             if (string.IsNullOrEmpty(rectString) || !rectString.Contains("#"))
             {
                 return default(Rect);
             }

             string[] rectStringArray = rectString.Split('#');

             return new Rect(
                 int.Parse(rectStringArray[0]),
                 int.Parse(rectStringArray[1]),
                 int.Parse(rectStringArray[2]),
                 int.Parse(rectStringArray[3]));
         }

         public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
         {
             Rect rectToStore = (Rect)value;

             string stringToStore = $"{rectToStore.Left}#{rectToStore.Top}#{rectToStore.Right}#{rectToStore.Bottom}";

             writer.WriteValue(stringToStore);
         }
     }

     #endregion*/


}
