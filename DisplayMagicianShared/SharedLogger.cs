using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DisplayMagicianShared
{
    public class SharedLogger
    {

        internal static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        internal static string AppLogPath = Path.Combine(AppDataPath, $"Logs");
        internal static string AppSettingsFile = Path.Combine(AppDataPath, $"Settings_1.0.json");
        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Provides a way of passing the NLog Logger instance to the DisplayMagician.Shared library so we log to a single log file.
        /// </summary>
        /// <param name="parentLogger"></param>
        public SharedLogger(NLog.Logger parentLogger)
        {
            SharedLogger.logger = parentLogger;
        }
    }
}
