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
        public static void WakeAllMonitors()
        {
            SharedLogger.logger.Info("DDC/CI/WakeAllMonitors: Attempting to wake all monitors.");

            SharedLogger.logger.Trace("DDC/CI/WakeAllMonitors: Creating callback function to wake up a single monitor.");
            MonitorEnumProc callback = new MonitorEnumProc((IntPtr hMonitor, IntPtr hdc, ref Rect rect, IntPtr data) =>
            {
                WakeSingleMonitor(hMonitor);
                return true;
            });

            SharedLogger.logger.Trace("DDC/CI/WakeAllMonitors: Attempting to run the callback across all monitors using Windows Display Drivers.");
            if (!EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero))
            {
                SharedLogger.logger.Warn("DDC/CI/WakeAllMonitors: WARNING - Failed to wake all monitors.");
            }
            else
            {
                SharedLogger.logger.Info("DDC/CI/WakeAllMonitors: Successfully contacted all monitors to wake them up.");
            }
        }

        private static void WakeSingleMonitor(IntPtr hMonitor)
        {
            SharedLogger.logger.Trace("DDC/CI/PokeSingleMonitor: Getting number of physical monitors from supplied monitor reference.");
            if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out uint count) || count == 0)
            {
                SharedLogger.logger.Warn("DDC/CI: Could not get number of physical monitors.");
                return;
            }

            SharedLogger.logger.Trace($"DDC/CI/PokeSingleMonitor: Found {count} physical monitors from supplied monitor reference.");
            var monitors = new PHYSICAL_MONITOR[count];
            if (!GetPhysicalMonitorsFromHMONITOR(hMonitor, count, monitors))
            {
                SharedLogger.logger.Warn("DDC/CI/PokeSingleMonitor: Could not get physical monitor handles.");
                return;            
            }

            SharedLogger.logger.Trace($"DDC/CI/PokeSingleMonitor: Got {count} physical monitor handes to enable control of the physical monitors.");

            foreach (var mon in monitors)
            {
                try
                {
                    // Disabling the power off we do before we ask the monitor to wake up
                    // Some monitors will not wake up again due to bios issues. So only keeping the wake up command.
                    /*// Power off
                    if (!SetVCPFeature(mon.hPhysicalMonitor, VCP_POWER_MODE, POWER_OFF))
                    {
                        int err = Marshal.GetLastWin32Error();
                        SharedLogger.logger.Warn($"DDC/CI: Failed to power off monitor. Error: {err}");
                    }
                    Thread.Sleep(1000);*/

                    SharedLogger.logger.Trace($"DDC/CI: Sending power on/wake up command (VCP_POWER_MODE = POWER_ON) to monitor: {mon.szPhysicalMonitorDescription}");

                    // Power on
                    if (!SetVCPFeature(mon.hPhysicalMonitor, VCP_POWER_MODE, POWER_ON))
                    {
                        int err = Marshal.GetLastWin32Error();
                        SharedLogger.logger.Warn($"DDC/CI: Failed to power on monitor {mon.szPhysicalMonitorDescription}. Error: {err}");
                    }
                    else
                    {
                        SharedLogger.logger.Info($"DDC/CI: Monitor {mon.szPhysicalMonitorDescription} successfully powered on.");
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"DDC/CI: Exception while sending power-on command to monitor {mon.szPhysicalMonitorDescription}: {ex.Message}");
                }
            }

            SharedLogger.logger.Trace($"DDC/CI/PokeSingleMonitor: Freeing physical monitor handle memory.");
            DestroyPhysicalMonitors(count, monitors);
        }
    }
}
