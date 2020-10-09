using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HeliosPlus
{
    internal static class DeviceNotification
    {
        #region Constants & types

        public const int DbtDeviceArrival = 0x8000; // System detected a new device        
        public const int DbtDeviceRemoveComplete = 0x8004; // Device is gone      
        public const int WmDeviceChange = 0x0219; // Device change event      

        #endregion

        #region Methods

        public static bool IsMonitor(IntPtr lParam)
        {
            return IsDeviceOfClass(lParam, GuidDeviceInterfaceMonitorDevice);
        }

        public static bool IsUsbDevice(IntPtr lParam)
        {
            return IsDeviceOfClass(lParam, GuidDeviceInterfaceUSBDevice);
        }

        /// Registers a window to receive notifications when Monitor devices are plugged or unplugged.
        public static void RegisterMonitorDeviceNotification(IntPtr windowHandle)
        {
            var dbi = CreateBroadcastDeviceInterface(GuidDeviceInterfaceMonitorDevice);
            monitorNotificationHandle = RegisterDeviceNotification(dbi, windowHandle);
        }

        /// Registers a window to receive notifications when USB devices are plugged or unplugged.
        public static void RegisterUsbDeviceNotification(IntPtr windowHandle)
        {
            var dbi = CreateBroadcastDeviceInterface(GuidDeviceInterfaceUSBDevice);
            usbNotificationHandle = RegisterDeviceNotification(dbi, windowHandle);
        }

        /// UnRegisters the window for Monitor device notifications
        public static void UnRegisterMonitorDeviceNotification()
        {
            UnregisterDeviceNotification(monitorNotificationHandle);
        }

        /// UnRegisters the window for USB device notifications
        public static void UnRegisterUsbDeviceNotification()
        {
            UnregisterDeviceNotification(usbNotificationHandle);
        }

        #endregion

        #region Private or protected constants & types

        private const int DbtDeviceTypeDeviceInterface = 5;

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/install/guid-devinterface-usb-device
        private static readonly Guid GuidDeviceInterfaceUSBDevice = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED"); // USB devices

        // https://docs.microsoft.com/en-us/windows-hardware/drivers/install/guid-devinterface-monitor
        private static readonly Guid GuidDeviceInterfaceMonitorDevice = new Guid("E6F07B5F-EE97-4a90-B076-33F57BF4EAA7"); // Monitor devices
        private static IntPtr usbNotificationHandle;
        private static IntPtr monitorNotificationHandle;

        [StructLayout(LayoutKind.Sequential)]
        private struct DevBroadcastDeviceInterface
        {
            internal int Size;
            internal int DeviceType;
            internal int Reserved;
            internal Guid ClassGuid;
            internal short Name;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DevBroadcastHdr
        {
            internal UInt32 Size;
            internal UInt32 DeviceType;
            internal UInt32 Reserved;
        }

        #endregion

        #region Private & protected methods

        private static bool IsDeviceOfClass(IntPtr lParam, Guid classGuid)
        {
            var hdr = Marshal.PtrToStructure<DevBroadcastHdr>(lParam);
            if (hdr.DeviceType != DbtDeviceTypeDeviceInterface)
                return false;

            var devIF = Marshal.PtrToStructure<DevBroadcastDeviceInterface>(lParam);

            return devIF.ClassGuid == classGuid;

        }

        private static DevBroadcastDeviceInterface CreateBroadcastDeviceInterface(Guid classGuid)
        {
            var dbi = new DevBroadcastDeviceInterface
            {
                DeviceType = DbtDeviceTypeDeviceInterface,
                Reserved = 0,
                ClassGuid = classGuid,
                Name = 0
            };

            dbi.Size = Marshal.SizeOf(dbi);

            return dbi;
        }

        private static IntPtr RegisterDeviceNotification(DevBroadcastDeviceInterface dbi, IntPtr windowHandle)
        {
            var buffer = Marshal.AllocHGlobal(dbi.Size);
            IntPtr handle;

            try
            {
                Marshal.StructureToPtr(dbi, buffer, true);

                handle = RegisterDeviceNotification(windowHandle, buffer, 0);
            }
            finally
            {
                // Free buffer
                Marshal.FreeHGlobal(buffer);
            }

            return handle;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);

        #endregion
    }
}
