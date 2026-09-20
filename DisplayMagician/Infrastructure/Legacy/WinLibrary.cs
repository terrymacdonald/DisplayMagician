using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DisplayMagician;

public static class WinLibrary
{
    public static List<string> GetAllPCIVideoCardVendors()
    {
        List<string> videoCardVendorIds = new List<string>();

        try
        {
            using ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT PNPDeviceID FROM Win32_VideoController");
            foreach (ManagementObject controller in searcher.Get())
            {
                string deviceId = controller["PNPDeviceID"] as string;
                Match match = Regex.Match(deviceId ?? string.Empty, @"(?:PCI|USB)\\(?:VEN|VID)_([\\d\\w]{4})&", RegexOptions.IgnoreCase);
                if (match.Success && !videoCardVendorIds.Any(vendor => string.Equals(vendor, match.Groups[1].Value, StringComparison.OrdinalIgnoreCase)))
                {
                    videoCardVendorIds.Add(match.Groups[1].Value);
                }
            }
        }
        catch (Exception ex)
        {
            SharedLogger.logger.Warn(ex, "WinLibrary/GetAllPCIVideoCardVendors: Could not enumerate video adapters.");
        }

        return videoCardVendorIds;
    }

    public static bool RestartExplorer()
    {
        try
        {
            string explorerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
            int result = RmStartSession(out uint sessionHandle, 0, Guid.NewGuid().ToString());
            if (result != 0)
            {
                throw new Win32Exception(result);
            }

            try
            {
                result = RmRegisterResources(sessionHandle, 1, new[] { explorerPath }, 0, IntPtr.Zero, 0, null);
                if (result != 0)
                {
                    throw new Win32Exception(result);
                }

                result = RmShutdown(sessionHandle, 0, IntPtr.Zero);
                if (result != 0)
                {
                    throw new Win32Exception(result);
                }

                result = RmRestart(sessionHandle, 0, IntPtr.Zero);
                if (result != 0)
                {
                    throw new Win32Exception(result);
                }

                return true;
            }
            finally
            {
                RmEndSession(sessionHandle);
            }
        }
        catch (Exception ex)
        {
            SharedLogger.logger.Error(ex, "WinLibrary/RestartExplorer: Could not restart Windows Explorer.");
            return false;
        }
    }

    [DllImport("rstrtmgr", CharSet = CharSet.Auto)]
    private static extern int RmStartSession(out uint sessionHandle, int sessionFlags, string sessionKey);

    [DllImport("rstrtmgr", CharSet = CharSet.Auto)]
    private static extern int RmRegisterResources(uint sessionHandle, uint fileCount, string[] fileNames, uint applicationCount, IntPtr applications, uint serviceCount, string[] serviceNames);

    [DllImport("rstrtmgr")]
    private static extern int RmShutdown(uint sessionHandle, int actionFlags, IntPtr statusCallback);

    [DllImport("rstrtmgr")]
    private static extern int RmRestart(uint sessionHandle, int restartFlags, IntPtr statusCallback);

    [DllImport("rstrtmgr")]
    private static extern int RmEndSession(uint sessionHandle);
}