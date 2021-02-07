using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DisplayMagicianShared
{
    class Logger
    {

        internal static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        internal static string AppLogPath = Path.Combine(AppDataPath, $"Logs");
        internal static string AppSettingsFile = Path.Combine(AppDataPath, $"Settings_1.0.json");
        public static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static Logger()
        {
            // Prepare NLog for logging
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            //string date = DateTime.Now.ToString("yyyyMMdd.HHmmss");
            string AppLogFilename = Path.Combine(AppLogPath, $"DisplayMagicianShared.log");

            // Create the Logging Dir if it doesn't exist so that it's avilable for all 
            // parts of the program to use
            if (!Directory.Exists(AppLogPath))
            {
                try
                {
                    Directory.CreateDirectory(AppLogPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Logger/Logger: Exception creating logging directory {AppLogPath} - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                }
            }

            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = AppLogFilename
            };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Figure out what logging level we're at in the main app
            // and set that up here
            NLog.LogLevel logLevel = NLog.LogLevel.Info;

            // Look for the LogLevel line and use that
            //  "LogLevel": "Warn"
            if (File.Exists(AppLogFilename))
            {

                using (StreamReader sr = File.OpenText(AppLogFilename))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        string pattern = @"\s?""LogLevel""\s?\:\s?""(Trace|Info|Warn|Error|Debug|Fatal)""";
                        MatchCollection matches = Regex.Matches(s, pattern);

                        switch (matches[1].Value)
                        {
                            case "Trace":
                                logLevel = NLog.LogLevel.Trace;
                                break;
                            case "Info":
                                logLevel = NLog.LogLevel.Info;
                                break;
                            case "Warn":
                                logLevel = NLog.LogLevel.Warn;
                                break;
                            case "Error":
                                logLevel = NLog.LogLevel.Error;
                                break;
                            case "Debug":
                                logLevel = NLog.LogLevel.Debug;
                                break;
                            default:
                                logLevel = NLog.LogLevel.Warn;
                                break;
                        }
                        break;
                    }
                }
                
            }
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(logLevel, NLog.LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }

    }
}
