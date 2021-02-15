using System.Linq;
using System.Management;

namespace DisplayMagician.GameLibraries
{
    class GameUtils
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static string GetMainModuleFilepath(int processId)
        {
            logger.Debug($"GameUtils/GetMainModuleFilepath: Using an alternative thread safe way to get the main module file path from the process with ProcessId = {processId}");

            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            {
                using (var results = searcher.Get())
                {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null)
                    {
                        logger.Debug($"GameUtils/GetMainModuleFilepath: Process eexecutable path is {(string)mo["ExecutablePath"]}");
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        }
    }
}
