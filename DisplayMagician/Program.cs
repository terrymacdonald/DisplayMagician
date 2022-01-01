using System;
using McMaster.Extensions.CommandLineUtils;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using DesktopNotifications;
using System.Windows.Forms;
//using DisplayMagician.InterProcess;
using DisplayMagician.Resources;
using DisplayMagicianShared;
using DisplayMagician.UIForms;
using DisplayMagician.GameLibraries;
using System.Text.RegularExpressions;
using System.Drawing;
using Windows.UI.Notifications;
using System.Runtime.Serialization;
using NLog.Config;
using System.Collections.Generic;
using AutoUpdaterDotNET;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.Win32;

namespace DisplayMagician {

    internal static class Program
    {
        internal static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        public static string AppStartupPath = Application.StartupPath;
        public static string AppIconPath = Path.Combine(Program.AppDataPath, $"Icons");
        public static string AppProfilePath = Path.Combine(Program.AppDataPath, $"Profiles");
        public static string AppShortcutPath = Path.Combine(Program.AppDataPath, $"Shortcuts");
        public static string AppWallpaperPath = Path.Combine(Program.AppDataPath, $"Wallpaper");
        public static string AppLogPath = Path.Combine(Program.AppDataPath, $"Logs");
        public static string AppDisplayMagicianIconFilename = Path.Combine(AppIconPath, @"DisplayMagician.ico");
        public static string AppOriginIconFilename = Path.Combine(AppIconPath, @"Origin.ico");
        public static string AppSteamIconFilename = Path.Combine(AppIconPath, @"Steam.ico");
        public static string AppUplayIconFilename = Path.Combine(AppIconPath, @"Uplay.ico");
        public static string AppEpicIconFilename = Path.Combine(AppIconPath, @"Epic.ico");
        public static string AppDownloadsPath = Utils.GetDownloadsPath();
        public static string AppPermStartMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "DisplayMagician","DisplayMagician.lnk");
        public static string AppTempStartMenuPath = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.Programs),"DisplayMagician.lnk");
        public const string AppUserModelId = "LittleBitBig.DisplayMagician";
        public const string AppActivationId = "4F319902-EB8C-43E6-8A51-8EA74E4308F8";        
        public static bool AppToastActivated = false;
        public static bool WaitingForGameToExit = false;
        public static ProgramSettings AppProgramSettings;
        public static MainForm AppMainForm;
        public static LoadingForm AppSplashScreen;
        public static ShortcutLoadingForm AppShortcutLoadingSplashScreen;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static SharedLogger sharedLogger;
        private static bool _gamesLoaded = false;
        private static bool _tempShortcutRegistered = false;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {

            // Create the remote server if we're first instance, or
            // If we're a subsequent instance, pass the command line parameters to the first instance and then 
            bool isFirstInstance = SingleInstance.LaunchOrReturn(otherInstance => { MessageBox.Show("got data: " + otherInstance.Skip(1).FirstOrDefault()); }, args);
            if (isFirstInstance)
            {
                Console.WriteLine($"Program/Main: This is the first DisplayMagician to start, so will be the one to actually perform the actions.");
            }
            else
            {
                Console.WriteLine($"Program/Main: There is already another DisplayMagician running, so we'll use that one to actually perform the actions.");
                Application.Exit();
            }

            // If we get here, then we're the first instance!
            RegisterDisplayMagicianWithWindows();

            // Prepare NLog for internal logging - Comment out when not required
            //NLog.Common.InternalLogger.LogLevel = NLog.LogLevel.Debug;
            //NLog.Common.InternalLogger.LogToConsole = true;
            //NLog.Common.InternalLogger.LogFile = "C:\\Users\\terry\\AppData\\Local\\DisplayMagician\\Logs\\nlog-internal.txt";

            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            //string date = DateTime.Now.ToString("yyyyMMdd.HHmmss");
            string AppLogFilename = Path.Combine(Program.AppLogPath, $"DisplayMagician.log");

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
                    Console.WriteLine($"Program/Main Exception: Cannot create the Application Log Folder {AppLogPath} - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                }
            }

            // NOTE: This had to be moved up from the later state
            // Copy the old Settings file to the new v2 name
            bool upgradedSettingsFile = false;
            string oldSettingsFile = Path.Combine(AppDataPath, "Settings_1.0.json");
            string newSettingsFile = Path.Combine(AppDataPath, "Settings_2.0.json");
            try
            {
                if (File.Exists(oldSettingsFile) && !File.Exists(newSettingsFile))
                {
                    File.Copy(oldSettingsFile, newSettingsFile, true);
                    upgradedSettingsFile = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Program/Main: Exception upgrading v1 settings file {oldSettingsFile} to v2 settings file {ProgramSettings.programSettingsStorageJsonFileName}.");
            }

            // Load the program settings
            AppProgramSettings = ProgramSettings.LoadSettings();
            

            // Rules for mapping loggers to targets          
            /*NLog.LogLevel logLevel = null;
            switch (AppProgramSettings.LogLevel)
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
                    logLevel = NLog.LogLevel.Info;
                    break;
            }*/
            // TODO - remove this temporary action to force Trace level logging
            // I've set this as it was too onerous continuously teaching people how to turn on TRACE logging
            // While there are a large number of big changes taking place with DisplayMagician, this will minimise
            // the backwards and forwards it takes to get the right level of log information for me to troubleshoot.
            NLog.LogLevel logLevel = NLog.LogLevel.Trace;
            AppProgramSettings.LogLevel = "Trace";


            // Create the log file target
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = AppLogFilename,
                DeleteOldFileOnStartup = true
            };

            // Create a logging rule to use the log file target
            var loggingRule = new LoggingRule("LogToFile");
            loggingRule.EnableLoggingForLevels(logLevel, NLog.LogLevel.Fatal);
            loggingRule.Targets.Add(logfile);
            loggingRule.LoggerNamePattern = "*";
            config.LoggingRules.Add(loggingRule);

            // Create the log console target
            var logconsole = new NLog.Targets.ColoredConsoleTarget("logconsole")
            {
                Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}",
            };

            // Create a logging rule to use the log console target
            var loggingRule2 = new LoggingRule("LogToConsole");
            loggingRule2.EnableLoggingForLevels(NLog.LogLevel.Info, NLog.LogLevel.Fatal);
            loggingRule2.Targets.Add(logconsole);
            loggingRule.LoggerNamePattern = "*";
            config.LoggingRules.Add(loggingRule2);

            // Apply config           
            NLog.LogManager.Configuration = config;
            
            // Make DisplayMagicianShared use the same log file by sending it the 
            // details of the existing NLog logger
            sharedLogger = new SharedLogger(logger);

            // Start the Log file
            logger.Info($"Starting {Application.ProductName} v{Application.ProductVersion}");

            // Create the other DM Dir if it doesn't exist so that it's avilable for all 
            // parts of the program to use
            if (!Directory.Exists(AppIconPath))
            {
                try
                {
                    Directory.CreateDirectory(AppIconPath);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/StartUpNormally exception: Cannot create the Application Icon Folder {AppLogPath}");
                }
            }
            if (!Directory.Exists(AppProfilePath))
            {
                try
                {
                    Directory.CreateDirectory(AppProfilePath);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/StartUpNormally exception: Cannot create the Application Profile Folder {AppProfilePath}");
                }
            }
            if (!Directory.Exists(AppShortcutPath))
            {
                try
                {
                    Directory.CreateDirectory(AppShortcutPath);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/StartUpNormally exception: Cannot create the Application Shortcut Folder {AppShortcutPath}");
                }
            }
            if (!Directory.Exists(AppWallpaperPath))
            {
                try
                {
                    Directory.CreateDirectory(AppWallpaperPath);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/StartUpNormally exception: Cannot create the Application Wallpaper Folder {AppWallpaperPath}");
                }
            }

            // Write the Application Name
            Console.WriteLine($"{Application.ProductName} v{Application.ProductVersion}");
            for (int i = 0; i <= Application.ProductName.Length + Application.ProductVersion .Length; i++)
            {
                Console.Write("=");
            }
            Console.WriteLine("=");
            Console.WriteLine($"Copyright © Terry MacDonald 2020-{DateTime.Today.Year}");
            Console.WriteLine(@"Derived from Helios Display Management - Copyright © Soroush Falahati 2017-2020");


            //Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false); 
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;            

            // Check if it's an upgrade from DisplayMagician v1 to v2
            // and if it is then copy the old configs to the new filenames and
            // explain to the user what they need to do.
            // e.g. DisplayProfiles_1.0.json exists, but DisplayProfiles_2.1.json doesn't
            if (File.Exists(Path.Combine(AppProfilePath, "DisplayProfiles_1.0.json")) && !File.Exists(Path.Combine(AppProfilePath, "DisplayProfiles_2.1.json")))
            {
                logger.Info($"Program/Main: This is an upgrade from DisplayMagician v1.0 to DisplayMagician v2.1, so performing some upgrade steps.");
                // Note whether we copied the old Settings file to the new v2 name earlier (before the logging was enabled)
                if (upgradedSettingsFile)
                {
                    logger.Info($"Program/Main: Upgraded v1.0 settings file {oldSettingsFile} to v2.0 settings file {newSettingsFile} earlier in loading process (before logging service was available).");
                }                

                // Copy the old Game Shortcuts file to the new v2 name
                string oldShortcutsFile = Path.Combine(AppShortcutPath, "Shortcuts_1.0.json");
                string newShortcutsFile = Path.Combine(AppShortcutPath, "Shortcuts_2.0.json");
                try
                {                    
                    if (File.Exists(oldShortcutsFile) && !File.Exists(newShortcutsFile))
                    {
                        logger.Info($"Program/Main: Upgrading v1 shortcut file {oldShortcutsFile} to v2 shortcut file {newShortcutsFile }.");
                        File.Copy(oldShortcutsFile, newShortcutsFile);
                    }
                }    
                catch(Exception ex)
                {
                    logger.Error(ex, $"Program/Main: Exception upgrading v1.0 shortcut file {oldShortcutsFile} to v2.0 shortcut file {ShortcutRepository.ShortcutStorageFileName}.");
                }

                // Warn the user about the fact we need a new DisplayProfiles_2.0.json
                StartMessageForm myMessageWindow = new StartMessageForm();
                myMessageWindow.MessageMode = "rtf";
                myMessageWindow.URL = "https://displaymagician.littlebitbig.com/messages/DisplayMagician1to2.rtf";
                myMessageWindow.HeadingText = "DisplayMagician v2.1.0 Upgrade Warning";
                myMessageWindow.ButtonText = "&Close";
                myMessageWindow.ShowDialog();
            }
            // Check if it's an upgrade from DisplayMagician v2.0 to v2.1
            // and if it is then copy the old configs to the new filenames and
            // explain to the user what they need to do.
            // e.g. DisplayProfiles_2.1.json exists, but DisplayProfiles_2.0.json doesn't
            else if (File.Exists(Path.Combine(AppProfilePath, "DisplayProfiles_2.0.json")) && !File.Exists(Path.Combine(AppProfilePath, "DisplayProfiles_2.1.json")))
            {
                logger.Info($"Program/Main: This is an upgrade from DisplayMagician v2.0 to DisplayMagician v2.1, so performing some upgrade steps.");
                // Warn the user about the fact we need a new DisplayProfiles_2.0.json
                StartMessageForm myMessageWindow = new StartMessageForm();
                myMessageWindow.MessageMode = "rtf";
                myMessageWindow.URL = "https://displaymagician.littlebitbig.com/messages/DisplayMagician20to21.rtf";
                myMessageWindow.HeadingText = "DisplayMagician v2.1.0 Upgrade Warning";
                myMessageWindow.ButtonText = "&Close";
                myMessageWindow.ShowDialog();
            }           

            logger.Debug($"Setting up commandline processing configuration");
            var app = new CommandLineApplication
            {
                AllowArgumentSeparator = true,
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
            }; 

            app.Description = "DisplayMagician is an open source tool for automatically configuring your displays and sound for a game or application from a single Windows Shortcut.";
            app.ExtendedHelpText = "DisplayMagician is an open source tool for automatically configuring your displays and sound for a game"
                + Environment.NewLine + "or application from a single Windows Shortcut, and reverting them back when finished.";

            app.GetFullNameAndVersion();
            app.MakeSuggestionsInErrorMessage = true;
            app.HelpOption("-?|-h|--help", inherited:true);

            app.VersionOption("-v|--version", () => {
                DeRegisterDisplayMagicianWithWindows();
                return string.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version);
            });

            CommandOption debug = app.Option("--debug", "Generate a DisplayMagician.log debug-level log file", CommandOptionType.NoValue);
            CommandOption trace = app.Option("--trace", "Generate a DisplayMagician.log trace-level log file", CommandOptionType.NoValue);
            CommandOption forcedVideoLibrary = app.Option("--force-video-library", "Bypass the normal video detection logic to force a particular video library (AMD, NVIDIA, Windows)", CommandOptionType.SingleValue);

            // This is the RunShortcut command
            app.Command(DisplayMagicianStartupAction.RunShortcut.ToString(), (runShortcutCmd) =>
            {
                // Try to load all the games in parallel to this process
                //Task.Run(() => LoadGamesInBackground());
                
                // Set the --trace or --debug options if supplied
                if (trace.HasValue())
                {
                    Console.WriteLine($"Changing logging level to TRACE level as --trace was provided on the commandline.");
                    logger.Info($"Changing logging level to TRACE level as --trace was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }
                else if (debug.HasValue())
                {
                    Console.WriteLine($"Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    logger.Info($"Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Debug, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }

                // Set the --force-video-library option if supplied
                if (forcedVideoLibrary.HasValue())
                {
                    if (forcedVideoLibrary.Value().Equals("NVIDIA"))
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.NVIDIA);
                        Console.WriteLine($"Forcing NVIDIA Video Library as '--force-video-library NVIDIA' was provided on the commandline.");
                        logger.Info($"Forcing NVIDIA Video Library as '--force-video-library NVIDIA' was provided on the commandline.");
                    }
                    else if (forcedVideoLibrary.Value().Equals("AMD"))
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.AMD);
                        Console.WriteLine($"Forcing AMD Video Library as '--force-video-library AMD' was provided on the commandline.");
                        logger.Info($"Forcing AMD Video Library as '--force-video-library AMD' was provided on the commandline.");
                    }
                    else if (forcedVideoLibrary.Value().Equals("Windows"))
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.WINDOWS);
                        Console.WriteLine($"Forcing Windows CCD Video Library as '--force-video-library Windows' was provided on the commandline.");
                        logger.Info($"Forcing Windows CCD Video Library as '--force-video-library Windows' was provided on the commandline.");
                    }
                    else
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.DETECT);
                        logger.Info($"Leaving DisplayMagician to detect the best Video Library to use.");
                    }
                }
                else
                {
                    ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.DETECT);
                    logger.Info($"Leaving DisplayMagician to detect the best Video Library to use.");
                }
                var argumentShortcut = runShortcutCmd.Argument("\"SHORTCUT_UUID\"", "(required) The UUID of the shortcut to run from those stored in the shortcut library.").IsRequired();
                argumentShortcut.Validators.Add(new ShortcutMustExistValidator());

                //description and help text of the command.
                runShortcutCmd.Description = "Use this command to run favourite game or application with a display profile of your choosing.";

                runShortcutCmd.OnExecute(() =>
                {
                    logger.Debug($"RunShortcut commandline command was invoked!");

                    // Load the games in background onexecute
                    GameLibrary.LoadGamesInBackground();

                    RunShortcut(argumentShortcut.Value);
                    DeRegisterDisplayMagicianWithWindows();
                    return 0;
                });
            });

            // This is the ChangeProfile command
            app.Command(DisplayMagicianStartupAction.ChangeProfile.ToString(), (runProfileCmd) =>
            {
                // Set the --trace or --debug options if supplied
                if (trace.HasValue())
                {
                    Console.WriteLine($"Changing logging level to TRACE level as --trace was provided on the commandline.");
                    logger.Info($"Changing logging level to TRACE level as --trace was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }
                else if (debug.HasValue())
                {
                    Console.WriteLine($"Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    logger.Info($"Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Debug, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }

                // Set the --force-video-library option if supplied
                if (forcedVideoLibrary.HasValue())
                {
                    if (forcedVideoLibrary.Value().Equals("NVIDIA"))
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.NVIDIA);
                        Console.WriteLine($"Forcing NVIDIA Video Library as '--force-video-library NVIDIA' was provided on the commandline.");
                        logger.Info($"Forcing NVIDIA Video Library as '--force-video-library NVIDIA' was provided on the commandline.");
                    }
                    else if (forcedVideoLibrary.Value().Equals("AMD"))
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.AMD);
                        Console.WriteLine($"Forcing AMD Video Library as '--force-video-library AMD' was provided on the commandline.");
                        logger.Info($"Forcing AMD Video Library as '--force-video-library AMD' was provided on the commandline.");
                    }
                    else if (forcedVideoLibrary.Value().Equals("Windows"))
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.WINDOWS);
                        Console.WriteLine($"Forcing Windows CCD Video Library as '--force-video-library Windows' was provided on the commandline.");
                        logger.Info($"Forcing Windows CCD Video Library as '--force-video-library Windows' was provided on the commandline.");
                    }
                    else
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.DETECT);
                        logger.Info($"Leaving DisplayMagician to detect the best Video Library to use.");
                    }
                }
                else
                {
                    ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.DETECT);
                    logger.Info($"Leaving DisplayMagician to detect the best Video Library to use.");
                }

                var argumentProfile = runProfileCmd.Argument("\"Profile_UUID\"", "(required) The UUID of the profile to run from those stored in the profile file.").IsRequired();
                argumentProfile.Validators.Add(new ProfileMustExistValidator());

                //description and help text of the command.
                runProfileCmd.Description = "Use this command to change to a display profile of your choosing.";

                runProfileCmd.OnExecute(() =>
                {
                    logger.Debug($"ChangeProfile commandline command was invoked!");

                    try
                    {
                        RunProfile(argumentProfile.Value);
                        DeRegisterDisplayMagicianWithWindows();
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Program/Main exception running ApplyProfile(profileToUse)");
                        DeRegisterDisplayMagicianWithWindows();
                        return 1;
                    }
                });
            });

            // This is the CreateProfile command
            app.Command(DisplayMagicianStartupAction.CreateProfile.ToString(), (createProfileCmd) =>
            {
                // Set the --trace or --debug options if supplied
                if (trace.HasValue())
                {
                    Console.WriteLine($"Changing logging level to TRACE level as --trace was provided on the commandline.");
                    logger.Info($"Changing logging level to TRACE level as --trace was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }
                else if (debug.HasValue())
                {
                    Console.WriteLine($"Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    logger.Info($"Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Debug, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }

                // Set the --force-video-library option if supplied
                if (forcedVideoLibrary.HasValue())
                {
                    if (forcedVideoLibrary.Value().Equals("NVIDIA"))
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.NVIDIA);
                        Console.WriteLine($"Forcing NVIDIA Video Library as '--force-video-library NVIDIA' was provided on the commandline.");
                        logger.Info($"Forcing NVIDIA Video Library as '--force-video-library NVIDIA' was provided on the commandline.");
                    }
                    else if (forcedVideoLibrary.Value().Equals("AMD"))
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.AMD);
                        Console.WriteLine($"Forcing AMD Video Library as '--force-video-library AMD' was provided on the commandline.");
                        logger.Info($"Forcing AMD Video Library as '--force-video-library AMD' was provided on the commandline.");
                    }
                    else if (forcedVideoLibrary.Value().Equals("Windows"))
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.WINDOWS);
                        Console.WriteLine($"Forcing Windows CCD Video Library as '--force-video-library Windows' was provided on the commandline.");
                        logger.Info($"Forcing Windows CCD Video Library as '--force-video-library Windows' was provided on the commandline.");
                    }
                    else
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.DETECT);
                        logger.Info($"Leaving DisplayMagician to detect the best Video Library to use.");
                    }
                }
                else
                {
                    ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.DETECT);
                    logger.Info($"Leaving DisplayMagician to detect the best Video Library to use.");
                }
                //description and help text of the command.
                createProfileCmd.Description = "Use this command to go directly to the create display profile screen.";

                createProfileCmd.OnExecute(() =>
                {
                    logger.Debug($"CreateProfile commandline command was invoked!");
                    Console.WriteLine("Starting up and creating a new Display Profile...");
                    CreateProfile();
                    DeRegisterDisplayMagicianWithWindows();
                    return 0;
                });
            });

            app.OnExecute(() =>
            {
                // Set the --trace or --debug options if supplied
                if (trace.HasValue())
                {
                    Console.WriteLine($"Changing logging level to TRACE level as --trace was provided on the commandline.");
                    logger.Info($"Changing logging level to TRACE level as --trace was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }
                else if (debug.HasValue())
                {
                    Console.WriteLine($"Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    logger.Info($"Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Debug, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }

                // Set the --force-video-library option if supplied
                if (forcedVideoLibrary.HasValue())
                {
                    if (forcedVideoLibrary.Value().Equals("NVIDIA"))
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.NVIDIA);
                        Console.WriteLine($"Forcing NVIDIA Video Library as '--force-video-library NVIDIA' was provided on the commandline.");
                        logger.Info($"Forcing NVIDIA Video Library as '--force-video-library NVIDIA' was provided on the commandline.");
                    }
                    else if (forcedVideoLibrary.Value().Equals("AMD"))
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.AMD);
                        Console.WriteLine($"Forcing AMD Video Library as '--force-video-library AMD' was provided on the commandline.");
                        logger.Info($"Forcing AMD Video Library as '--force-video-library AMD' was provided on the commandline.");
                    }
                    else if (forcedVideoLibrary.Value().Equals("Windows"))
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.WINDOWS);
                        Console.WriteLine($"Forcing Windows CCD Video Library as '--force-video-library Windows' was provided on the commandline.");
                        logger.Info($"Forcing Windows CCD Video Library as '--force-video-library Windows' was provided on the commandline.");
                    }
                    else
                    {
                        ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.DETECT);
                        logger.Info($"Leaving DisplayMagician to detect the best Video Library to use.");
                    }
                }
                else
                {
                    ProfileRepository.InitialiseRepository(FORCED_VIDEO_MODE.DETECT);
                    logger.Info($"Leaving DisplayMagician to detect the best Video Library to use.");
                }

                logger.Debug($"No commandline command was invoked, so starting up normally");
                // Add a workaround to handle the weird way that Windows tell us that DisplayMagician 
                // was started from a Notification Toast when closed (Windows 10)
                // Due to the way that CommandLineUtils library works we need to handle this as
                // 'Remaining Arguments'
                if (app.RemainingArguments != null && app.RemainingArguments.Count > 0)
                {
                    foreach (string myArg in app.RemainingArguments)
                    {
                        if (myArg.Equals("-ToastActivated"))
                        {
                            logger.Debug($"We were started by the user clicking on a Windows Toast");
                            Program.AppToastActivated = true;
                            break;
                        }

                    }
                }
                logger.Info("Starting Normally...");

                // Try to load all the games in parallel to this process
                //Task.Run(() => LoadGamesInBackground());
                logger.Debug($"Try to load all the Games in the background to avoid locking the UI");
                GameLibrary.LoadGamesInBackground();

                StartUpApplication();
                DeRegisterDisplayMagicianWithWindows();
                return 0;
            });

            
            if (AppProgramSettings.ShowSplashScreen)
            {
                //Show Splash Form
                AppSplashScreen = new LoadingForm();
                var splashThread = new Thread(new ThreadStart(
                    () => Application.Run(AppSplashScreen)));
                splashThread.SetApartmentState(ApartmentState.STA);
                splashThread.Start();
            }         

            try
            {
                logger.Debug($"Starting commandline processing");
                // This begins the actual execution of the application
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                logger.Error(ex, $"Program/Main exception parsing the Commands passed to the program");
                Console.WriteLine("Didn't recognise the supplied commandline options: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Program/Main commandParsingException: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // You'll always want to catch this exception, otherwise it will generate a messy and confusing error for the end user.
                // the message will usually be something like:
                // "Unrecognized command or argument '<invalid-command>'"
                logger.Error(ex, $"Program/Main general exception during app.Execute(args)");
                Console.WriteLine($"Program/Main exception: Unable to execute application - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
            }

            logger.Debug($"Beginning to shutdown");

            logger.Debug($"Clearing all previous windows toast notifications as they aren't needed any longer");
            // Remove all the notifications we have set as they don't matter now!
            DesktopNotificationManagerCompat.History.Clear();

            // Shutdown NLog
            logger.Debug($"Stopping logging processes");
            NLog.LogManager.Shutdown();

            // Exit with a 0 Errorlevel to indicate everything worked fine!
            return 0;
        }       

        private static void CreateProfile()
        {
            logger.Debug($"Program/CreateProfile: Starting");

            try
            {
                // Start the IPC Service to 
                /*if (!IPCService.StartService())
                {
                    throw new Exception(Language.Can_not_open_a_named_pipe_for_Inter_process_communication);
                }*/

            
                //IPCService.GetInstance().Status = InstanceStatus.User;

                // Close the splash screen
                if (ProgramSettings.LoadSettings().ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                    AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));

                // Run the program with directly showing CreateProfile form
                Application.Run(new DisplayProfileForm());

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/CreateProfile exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                logger.Error(ex, $"Program/CreateProfile top level exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                MessageBox.Show(
                    ex.Message,
                    Language.Fatal_Error,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

        }

        private static void StartUpApplication()
        {
            logger.Debug($"Program/StartUpApplication: Starting");

            try
            {
                // Start the IPC Service to 
                /*if (!IPCService.StartService())
                {
                    throw new Exception(Language.Can_not_open_a_named_pipe_for_Inter_process_communication);
                }*/

                // Create the Shortcut Icon Cache if it doesn't exist so that it's avilable for all the program
                if (!Directory.Exists(AppIconPath))
                {
                    try
                    {
                        Directory.CreateDirectory(AppIconPath);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Program/StartUpApplication exception while trying to create directory {AppIconPath}");
                    }
                }

                try
                {
                    // Save a copy of the DisplayMagician Icon
                    if (!File.Exists(AppDisplayMagicianIconFilename))
                    {
                        Icon heliosIcon = (Icon)Properties.Resources.DisplayMagician;
                        using (FileStream fs = new FileStream(AppDisplayMagicianIconFilename, FileMode.Create))
                            heliosIcon.Save(fs);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/StartUpApplication exception create Icon files for future use in {AppIconPath}");
                }

                //IPCService.GetInstance().Status = InstanceStatus.User;

                // Check for updates
                CheckForUpdates();

                 // Show any messages we need to show
                ShowMessages();

                // Run the program with normal startup
                AppMainForm = new MainForm();
                AppMainForm.Load += MainForm_LoadCompleted;
                Application.Run(AppMainForm);                

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Program/StartUpApplication top level exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                MessageBox.Show(
                    ex.Message,
                    Language.Fatal_Error,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            
        }

        private static void MainForm_LoadCompleted(object sender, EventArgs e)
        {
            if (ProgramSettings.LoadSettings().ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));
            AppMainForm.TopMost = true;
            AppMainForm.Activate();
            AppMainForm.TopMost = false;
        }

        // ReSharper disable once CyclomaticComplexity
        private static void RunShortcut(string shortcutUUID)
        {
            logger.Debug($"Program/RunShortcut: Starting");

            ShortcutItem shortcutToRun = null;

            // Close the splash screen
            if (ProgramSettings.LoadSettings().ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));

            // Check there is only one version of this application so we won't
            // mess with another monitoring session
            /*if (
                IPCClient.QueryAll()
                    .Any(
                        client =>
                            client.Status == InstanceStatus.Busy ||
                            client.Status == InstanceStatus.OnHold))
            {
                throw new Exception(
                    Language
                        .Another_instance_of_this_program_is_in_working_state_Please_close_other_instances_before_trying_to_switch_profile);
            }*/

            // Match the ShortcutName to the actual shortcut listed in the shortcut library
            // And error if we can't find it.
            if (ShortcutRepository.ContainsShortcut(shortcutUUID))
            {
                // make sure we trim the "" if there are any
                shortcutUUID = shortcutUUID.Trim('"');
                shortcutToRun = ShortcutRepository.GetShortcut(shortcutUUID);
            }
            else
            {
                throw new Exception(Language.Cannot_find_shortcut_in_library);
            }

            if (shortcutToRun is ShortcutItem)
            {
                ShortcutRepository.RunShortcut(shortcutToRun);
            }

            //IPCService.GetInstance().Status = InstanceStatus.Busy;

        }

        public static bool IsValidFilename(string testName)
        {
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(testName)) { return false; };

            return true;
        }

        public static void RunProfile(string profileName)
        {
            logger.Trace($"Program/RunProfile: Starting");

            // Close the splash screen
            if (ProgramSettings.LoadSettings().ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));

            // Lookup the profile
            ProfileItem profileToUse = ProfileRepository.AllProfiles.Where(p => p.UUID.Equals(profileName)).First();
            logger.Trace($"Program/RunProfile: Found profile called {profileName} and now starting to apply the profile");

            ProfileRepository.ApplyProfile(profileToUse);

        }

        
        public static string HotkeyToString(Keys hotkey)
        {
            string parsedHotkey = String.Empty;
            KeysConverter kc = new KeysConverter();

            // Lets parse the hotkey to create the text we need
            parsedHotkey = kc.ConvertToString(hotkey);

            // Control also shows as Ctrl+ControlKey, so we trim the +ControlKeu
            if (parsedHotkey.Contains("+ControlKey"))
                parsedHotkey = parsedHotkey.Replace("+ControlKey", "");

            // Shift also shows as Shift+ShiftKey, so we trim the +ShiftKeu
            if (parsedHotkey.Contains("+ShiftKey"))
                parsedHotkey = parsedHotkey.Replace("+ShiftKey", "");

            // Alt also shows as Alt+Menu, so we trim the +Menu
            if (parsedHotkey.Contains("+Menu"))
                parsedHotkey = parsedHotkey.Replace("+Menu", "");

            return parsedHotkey;
        }

        public static void ShowMessages()
        {
            // Get the message index
            string json;
            List<MessageItem> messageIndex;
            WebClient client = new WebClient();
            string indexUrl = "https://displaymagician.littlebitbig.com/messages/index_2.0.json";
            try
            {
                json = client.DownloadString(indexUrl);
                if (String.IsNullOrWhiteSpace(json))
                {
                    logger.Trace($"Program/ShowMessages: There were no messages in the {indexUrl} message index.");
                    return;
                }
                messageIndex = JsonConvert.DeserializeObject<List<MessageItem>>(json);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Program/ShowMessages: Exception while trying to load the messages index from {indexUrl}.");
                return;
            }

            ProgramSettings programSettings = ProgramSettings.LoadSettings();

            foreach (MessageItem message in messageIndex)
            {
                // Skip if we've already shown it
                if (message.Id <= programSettings.LastMessageIdRead)
                {
                    // Unless it's one coming up that we're monitoring
                    if (!programSettings.MessagesToMonitor.Contains(message.Id))
                    {
                        continue;
                    }
                }

                // Firstly, check that the version is correct
                Version myAppVersion = Assembly.GetEntryAssembly().GetName().Version;
                if (!string.IsNullOrWhiteSpace(message.MinVersion))
                {
                    Version minVersion;
                    if (!(Version.TryParse(message.MinVersion,out minVersion)))
                    {
                        logger.Error($"Program/ShowMessages: Couldn't show message \"{message.HeadingText}\" (#{message.Id}) as we couldn't parse the minversion string.");
                        continue;
                    }

                    if (!(myAppVersion >= minVersion))
                    {
                        logger.Debug($"Program/ShowMessages: Message is for version >= {minVersion} and this is version {myAppVersion} so not showing message.");
                        continue;
                    }
                }
                if (!string.IsNullOrWhiteSpace(message.MaxVersion))
                {
                    Version maxVersion;
                    if (!(Version.TryParse(message.MaxVersion, out maxVersion)))
                    {
                        logger.Error($"Program/ShowMessages: Couldn't show message \"{message.HeadingText}\" (#{message.Id}) as we couldn't parse the maxversion string.");
                        continue;
                    }

                    if (!(myAppVersion <= maxVersion))
                    {
                        logger.Debug($"Program/ShowMessages: Message is for version <= {maxVersion} and this is version {myAppVersion} so not showing message.");
                        // Save it if it's one coming up that we're monitoring and we haven't already saved it
                        continue;
                    }
                }

                // Secondly check if the dates are such that we should show this
                if (!string.IsNullOrWhiteSpace(message.StartDate))
                {
                    // Convert datestring to a datetime
                    DateTime startTime;
                    if (!DateTime.TryParse(message.StartDate,out startTime))
                    {
                        logger.Error($"Program/ShowMessages: Couldn't show message \"{message.HeadingText}\" (#{message.Id}) as we couldn't parse the start date.");
                        continue;
                    }

                    if (!(DateTime.Now >= startTime))
                    {
                        logger.Debug($"Program/ShowMessages: Message start date for \"{message.HeadingText}\" (#{message.Id}) not yet reached so not ready to show message.");
                        if (!programSettings.MessagesToMonitor.Contains(message.Id))
                        {
                            programSettings.MessagesToMonitor.Add(message.Id);
                            programSettings.SaveSettings();
                        }
                        continue;
                    }
                }
                if (!string.IsNullOrWhiteSpace(message.EndDate))
                {
                    // Convert datestring to a datetime
                    DateTime endTime;
                    if (!DateTime.TryParse(message.EndDate, out endTime))
                    {
                        logger.Error($"Program/ShowMessages: Couldn't show message \"{message.HeadingText}\" (#{message.Id}) as we couldn't parse the end date.");
                        continue;
                    }

                    if (!(DateTime.Now <= endTime))
                    {
                        logger.Debug($"Program/ShowMessages: Message end date for \"{message.HeadingText}\" (#{message.Id}) past so not showing message as it's too old.");
                        continue;
                    }
                }

                StartMessageForm myMessageWindow = new StartMessageForm();
                myMessageWindow.MessageMode = message.MessageMode;
                myMessageWindow.URL = message.Url;
                myMessageWindow.HeadingText = message.HeadingText;
                myMessageWindow.ButtonText = message.ButtonText;
                myMessageWindow.ShowDialog();
                // If this the list of messages is still trying to monitor this message, then remove it if we've shown it to the user.
                if (programSettings.MessagesToMonitor.Contains(message.Id))
                {
                    programSettings.MessagesToMonitor.Remove(message.Id);
                    programSettings.SaveSettings();
                }

                // Update the latest message id to keep track of where we're up to
                if (message.Id > programSettings.LastMessageIdRead)
                {
                    programSettings.LastMessageIdRead = message.Id;
                }
                
            }
            
        }

        public static void CheckForUpdates()
        {
            //Run the AutoUpdater to see if there are any updates available.
            //FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
            //AutoUpdater.InstalledVersion = new Version(fvi.FileVersion);
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
            AutoUpdater.RunUpdateAsAdmin = true;
            AutoUpdater.HttpUserAgent = "DisplayMagician AutoUpdater";
            if (Program.AppProgramSettings.UpgradeToPreReleases == false)
            {
                AutoUpdater.Start("http://displaymagician.littlebitbig.com/update/update_2.0.json");
            }
            else
            {
                AutoUpdater.Start("http://displaymagician.littlebitbig.com/update/prerelease_2.0.json");
            }
        }

        private static void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
            logger.Trace($"Program/AutoUpdaterOnParseUpdateInfoEvent: Received the following Update JSON file from {AutoUpdater.AppCastURL}: {args.RemoteData}");
            try
            {
                logger.Trace($"MainForm/AutoUpdaterOnParseUpdateInfoEvent: Trying to create an UpdateInfoEventArgs object from the received Update JSON file.");
                args.UpdateInfo = new UpdateInfoEventArgs
                {
                    CurrentVersion = (string)json["version"],
                    ChangelogURL = (string)json["changelog"],
                    DownloadURL = (string)json["url"],
                    Mandatory = new Mandatory
                    {
                        Value = (bool)json["mandatory"]["value"],
                        UpdateMode = (Mode)(int)json["mandatory"]["mode"],
                        MinimumVersion = (string)json["mandatory"]["minVersion"]
                    },
                    CheckSum = new CheckSum
                    {
                        Value = (string)json["checksum"]["value"],
                        HashingAlgorithm = (string)json["checksum"]["hashingAlgorithm"]
                    }
                };
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Program/AutoUpdaterOnParseUpdateInfoEvent: Exception trying to create an UpdateInfoEventArgs object from the received Update JSON file.");
            }

        }

        private static void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error == null)
            {
                if (args.IsUpdateAvailable)
                {
                    // Shut down the splash screen
                    if (Program.AppProgramSettings.ShowSplashScreen && Program.AppSplashScreen != null && !Program.AppSplashScreen.Disposing && !Program.AppSplashScreen.IsDisposed)
                        Program.AppSplashScreen.Invoke(new Action(() => Program.AppSplashScreen.Close()));

                    logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - There is an upgrade to version {args.CurrentVersion} available from {args.DownloadURL}. We're using version {args.InstalledVersion} at the moment.");
                    DialogResult dialogResult;                    

                    if (args.Mandatory.Value)
                    {
                        logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - New version {args.CurrentVersion} available. Current version is {args.InstalledVersion}. Mandatory upgrade.");
                        dialogResult =
                            MessageBox.Show(
                                $@"There is new version {args.CurrentVersion} available. You are using version {args.InstalledVersion}. This is required update. Press Ok to begin updating the application.", @"Update Available",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                    }
                    else
                    {
                        logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - New version {args.CurrentVersion} available. Current version is {args.InstalledVersion}. Optional upgrade.");
                        dialogResult =
                            MessageBox.Show(
                                $@"There is new version {args.CurrentVersion} available. You are using version {
                                        args.InstalledVersion
                                    }. Do you want to update the application now?", @"Update Available",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);
                    }

                    // Uncomment the following line if you want to show standard update dialog instead.
                    //AutoUpdater.ShowUpdateForm(args);

                    if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                    {
                        try
                        {
                            logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - Downloading {args.InstalledVersion} update.");
                            if (AutoUpdater.DownloadUpdate(args))
                            {
                                logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - Restarting to apply {args.InstalledVersion} update.");
                                Application.Exit();
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex, $"Program/AutoUpdaterOnCheckForUpdateEvent - Exception during update download.");
                            MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                // Shut down the splash screen
                if (Program.AppProgramSettings.ShowSplashScreen && Program.AppSplashScreen != null && !Program.AppSplashScreen.Disposing && !Program.AppSplashScreen.IsDisposed)
                    Program.AppSplashScreen.Invoke(new Action(() => Program.AppSplashScreen.Close()));

                if (args.Error is WebException)
                {
                    logger.Warn(args.Error, $"Program/AutoUpdaterOnCheckForUpdateEvent - WebException - There was a problem reaching the update server.");
                    MessageBox.Show(
                        @"There is a problem reaching update server. Please check your internet connection and try again later.",
                        @"Update Check Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    logger.Warn(args.Error, $"Program/AutoUpdaterOnCheckForUpdateEvent - There was a problem performing the update: {args.Error.Message}");
                    MessageBox.Show(args.Error.Message,
                        args.Error.GetType().ToString(), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private static void RegisterDisplayMagicianWithWindows()
        {
            // This sets the Application User Model ID to "LittleBitBig.DisplayMagician" so that
            // Windows 10 recognises the application, and allows features such as Toasts, 
            // taskbar pinning and similar.
            // Register AUMID, COM server, and activator
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<DesktopNotificationActivator>(AppUserModelId);
            DesktopNotificationManagerCompat.RegisterActivator<DesktopNotificationActivator>();

            // Force toasts to work if we're not 'installed' per se by creating a temp DisplayMagician start menu icon
            // Allows running from a ZIP file rather than forcing the app to be installed. If we don't do this then Toasts just wouldn't work.
            try
            {
                if (!IsInstalledVersion())
                {
                    _tempShortcutRegistered = true;
                    ShortcutManager.RegisterAppForNotifications(
                        AppTempStartMenuPath, Assembly.GetExecutingAssembly().Location, null, AppUserModelId, AppActivationId);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/RegisterDisplayMagicianWithWindows - Exception while trying to register the temporary application shortcut {AppTempStartMenuPath}. Windows Toasts will not work.");
            }
        }


        private static void DeRegisterDisplayMagicianWithWindows()
        {
            // Remove the temporary shortcut if we have added it
            if (_tempShortcutRegistered)
            {
                try
                {
                    File.Delete(AppTempStartMenuPath);
                }
                catch(Exception ex)
                {
                    logger.Warn(ex, $"Program/DeRegisterDisplayMagicianWithWindows - Exception while deleting the temporary application shortcut {AppTempStartMenuPath} ");
                }
                _tempShortcutRegistered = false;
            }
        }

        public static bool IsInstalledVersion()

        {
            string installKey = @"SOFTWARE\DisplayMagician";
            string thisInstallDir = Path.GetDirectoryName(Application.ExecutablePath) + "\\";

            try
            {
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(installKey))
                {
                    if (rk.GetValue("InstallDir") != null && rk.GetValue("InstallDir").ToString() == thisInstallDir)
                    {
                        return true; //exists
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            // …
            return true;
        }

    }


public class LoadingInstalledGamesException : Exception
    {
        public LoadingInstalledGamesException()
        { }
        public LoadingInstalledGamesException(string message) : base(message)
        { }
        public LoadingInstalledGamesException(string message, Exception innerException) : base(message, innerException)
        { }
        public LoadingInstalledGamesException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}