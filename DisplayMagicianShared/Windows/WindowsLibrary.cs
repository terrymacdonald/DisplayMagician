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
            WIN32STATUS err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out var pathCount, out var modeCount);
            if (err != WIN32STATUS.ERROR_SUCCESS)
                throw new Win32Exception((int)err);

            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err != WIN32STATUS.ERROR_SUCCESS)
                throw new Win32Exception((int)err);

            foreach (var path in paths)
            {
                // get display name
                var info = new DISPLAYCONFIG_GET_TARGET_NAME();
                info.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
                info.Header.Size = Marshal.SizeOf<DISPLAYCONFIG_GET_TARGET_NAME>();
                info.Header.AdapterId = path.TargetInfo.AdapterId;
                info.Header.Id = path.TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref info);
                if (err != WIN32STATUS.ERROR_SUCCESS)
                    throw new Win32Exception((int)err);

                var colorInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();
                colorInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
                colorInfo.Header.Size = Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>();
                colorInfo.Header.AdapterId = path.TargetInfo.AdapterId;
                colorInfo.Header.Id = path.TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref colorInfo);
                if (err != WIN32STATUS.ERROR_SUCCESS)
                    throw new Win32Exception((int)err);

                Console.WriteLine(info.MonitorFriendlyDeviceName);
                Console.WriteLine(" Advanced Color Supported: " + colorInfo.AdvancedColorSupported);
                Console.WriteLine(" Advanced Color Enabled  : " + colorInfo.AdvancedColorEnabled);
                Console.WriteLine();
            }
        }
    }  
}
