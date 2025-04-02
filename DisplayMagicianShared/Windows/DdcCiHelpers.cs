using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DisplayMagicianShared.Windows
{
    public static class DdcCiHelper
    {
        private const byte VCP_POWER_MODE = 0xD6;
        private const uint POWER_ON = 0x01;
        private const uint POWER_OFF = 0x04;

        [DllImport("Dxva2.dll", SetLastError = true)]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint count);

        [DllImport("Dxva2.dll", SetLastError = true)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint count, [Out] PHYSICAL_MONITOR[] monitors);

        [DllImport("Dxva2.dll", SetLastError = true)]
        private static extern bool SetVCPFeature(IntPtr hMonitor, byte bVCPCode, uint newValue);

        [DllImport("Dxva2.dll", SetLastError = true)]
        private static extern bool DestroyPhysicalMonitors(uint count, PHYSICAL_MONITOR[] monitors);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int left, top, right, bottom;
        }

        /// <summary>
        /// Attempts to power-cycle all physical monitors using DDC/CI (off then on).
        /// </summary>
        public static void PokeAllMonitors()
        {
            SharedLogger.logger.Info("DDC/CI: Attempting to poke all monitors.");

            MonitorEnumProc callback = new MonitorEnumProc((IntPtr hMonitor, IntPtr hdc, ref Rect rect, IntPtr data) =>
            {
                PokeSingleMonitor(hMonitor);
                return true;
            });

            if (!EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero))
            {
                SharedLogger.logger.Warn("DDC/CI: EnumDisplayMonitors failed.");
            }

        }

        private static void PokeSingleMonitor(IntPtr hMonitor)
        {
            if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out uint count) || count == 0)
            {
                SharedLogger.logger.Warn("DDC/CI: Could not get number of physical monitors.");
                return;
            }

            var monitors = new PHYSICAL_MONITOR[count];
            if (!GetPhysicalMonitorsFromHMONITOR(hMonitor, count, monitors))
            {
                SharedLogger.logger.Warn("DDC/CI: Could not get physical monitor handles.");
                return;
            }

            foreach (var mon in monitors)
            {
                try
                {
                    SharedLogger.logger.Info($"DDC/CI: Sending power cycle to monitor: {mon.szPhysicalMonitorDescription}");

                    // Power off
                    if (!SetVCPFeature(mon.hPhysicalMonitor, VCP_POWER_MODE, POWER_OFF))
                    {
                        int err = Marshal.GetLastWin32Error();
                        SharedLogger.logger.Warn($"DDC/CI: Failed to power off monitor. Error: {err}");
                    }

                    Thread.Sleep(500);

                    // Power on
                    if (!SetVCPFeature(mon.hPhysicalMonitor, VCP_POWER_MODE, POWER_ON))
                    {
                        int err = Marshal.GetLastWin32Error();
                        SharedLogger.logger.Warn($"DDC/CI: Failed to power on monitor. Error: {err}");
                    }
                    else
                    {
                        SharedLogger.logger.Info($"DDC/CI: Monitor successfully power-cycled.");
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error($"DDC/CI: Exception while power cycling monitor: {ex.Message}");
                }
            }

            DestroyPhysicalMonitors(count, monitors);
        }
    }
}
