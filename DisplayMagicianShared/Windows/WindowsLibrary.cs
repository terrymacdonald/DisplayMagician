using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace DisplayMagicianShared.Windows
{
    class WindowsCCDLibrary
    {
        static void Main()
        {
            var err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out var pathCount, out var modeCount);
            if (err != 0)
                throw new Win32Exception(err);

            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err != 0)
                throw new Win32Exception(err);

            foreach (var path in paths)
            {
                // get display name
                var info = new DISPLAYCONFIG_GET_TARGET_NAME();
                info.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
                info.header.size = Marshal.SizeOf<DISPLAYCONFIG_GET_TARGET_NAME>();
                info.header.adapterId = path.targetInfo.adapterId;
                info.header.id = path.targetInfo.id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref info);
                if (err != 0)
                    throw new Win32Exception(err);

                var colorInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();
                colorInfo.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
                colorInfo.header.size = Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>();
                colorInfo.header.adapterId = path.targetInfo.adapterId;
                colorInfo.header.id = path.targetInfo.id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref colorInfo);
                if (err != 0)
                    throw new Win32Exception(err);

                Console.WriteLine(info.monitorFriendlyDeviceName);
                Console.WriteLine(" Advanced Color Supported: " + colorInfo.advancedColorSupported);
                Console.WriteLine(" Advanced Color Enabled  : " + colorInfo.advancedColorEnabled);
                Console.WriteLine();
            }
        }
    }  
}
