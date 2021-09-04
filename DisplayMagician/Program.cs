using System;
using McMaster.Extensions.CommandLineUtils;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.InterProcess;
using DisplayMagician.Resources;
using DisplayMagicianShared;
using DisplayMagician.UIForms;
using DisplayMagician.GameLibraries;
using System.Text.RegularExpressions;
using System.Drawing;
using DesktopNotifications;
using System.Runtime.Serialization;
using NLog.Config;
using System.Collections.Generic;
using System.Collections;
using DisplayMagicianShared.AMD;
using DisplayMagicianShared.NVIDIA;
using DisplayMagicianShared.Windows;

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
        public static bool AppToastActivated = false;
        public static bool WaitingForGameToExit = false;
        public static ProgramSettings AppProgramSettings;
        public static MainForm AppMainForm;

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static SharedLogger sharedLogger;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {

            // This sets the Application User Model ID to "LittleBitBig.DisplayMagician" so that
            // Windows 10 recognises the application, and allows features such as Toasts, 
            // taskbar pinning and similar.
            // Register AUMID, COM server, and activator
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<DesktopNotificationActivator>(ShellUtils.AUMID);
            DesktopNotificationManagerCompat.RegisterActivator<DesktopNotificationActivator>();

            // Prepare NLog for logging

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
                    Console.WriteLine($"Program/StartUpNormally exception: Cannot create the Application Log Folder {AppLogPath} - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                }
            }
            
            // Load the program settings
            AppProgramSettings = ProgramSettings.LoadSettings();

            // Rules for mapping loggers to targets          
            NLog.LogLevel logLevel = null;
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
            }
           
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


            logger.Debug($"Setting up commandline processing configuration");
            var app = new CommandLineApplication
            {
                AllowArgumentSeparator = true,
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
            }; 

            app.Description = "This application helps configure your NVIDIA Videocard for multiple displays.";
            app.ExtendedHelpText = "This application helps configure your NVIDIA Videocard for multiple displays. It has some nifty features such as the "
                + Environment.NewLine + " ability to temporarily change your screen settings while you are playing a game, and then change them back once finished.";

            app.GetFullNameAndVersion();
            app.MakeSuggestionsInErrorMessage = true;
            app.HelpOption("-?|-h|--help", inherited:true);

            app.VersionOption("-v|--version", () => {
                return string.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version);
            });

            CommandOption debug = app.Option("--debug", "Generate a DisplayMagician.log debug-level log file", CommandOptionType.NoValue);
            CommandOption trace = app.Option("--trace", "Generate a DisplayMagician.log trace-level log file", CommandOptionType.NoValue);
            CommandOption forcedVideoLibrary = app.Option("--force-video-library", "Bypass the normal video detection logic to force a particular video library (AMD, NVIDIA, Windows)", CommandOptionType.SingleValue);

            // This is the RunShortcut command
            app.Command(DisplayMagicianStartupAction.RunShortcut.ToString(), (runShortcutCmd) =>
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
                var argumentShortcut = runShortcutCmd.Argument("\"SHORTCUT_UUID\"", "(required) The UUID of the shortcut to run from those stored in the shortcut library.").IsRequired();
                argumentShortcut.Validators.Add(new ShortcutMustExistValidator());

                //description and help text of the command.
                runShortcutCmd.Description = "Use this command to run favourite game or application with a display profile of your choosing.";

                runShortcutCmd.OnExecute(() =>
                {
                    logger.Debug($"RunShortcut commandline command was invoked!");

                    // 
                    RunShortcut(argumentShortcut.Value);
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
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Program/Main exception running ApplyProfile(profileToUse)");
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
                    ProfileRepository.ForcedVideoMode = FORCED_VIDEO_MODE.DETECT;
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
                StartUpApplication();
                return 0;
            });

            logger.Debug($"Try to load all the Games in the background to avoid locking the UI");

            // Try to load all the games in parallel to this process
            Task.Run(() => LoadGamesInBackground());

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
                if (!IPCService.StartService())
                {
                    throw new Exception(Language.Can_not_open_a_named_pipe_for_Inter_process_communication);
                }

            
                IPCService.GetInstance().Status = InstanceStatus.User;

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
                if (!IPCService.StartService())
                {
                    throw new Exception(Language.Can_not_open_a_named_pipe_for_Inter_process_communication);
                }

                // Create the Shortcut Icon Cache if it doesn't exist so that it's avilable for all the program
                if (!Directory.Exists(AppIconPath))
                {
                    try
                    {
                        Directory.CreateDirectory(AppIconPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Program/StartUpNormally exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                        logger.Error(ex, $"Program/StartUpNormally exception while trying to create directory {AppIconPath}");
                    }
                }

                try
                {
                    // Save a copy of the DisplayMagician Icon, and all the game library ones in preparation for future use
                    if (!File.Exists(AppDisplayMagicianIconFilename))
                    {
                        Icon heliosIcon = (Icon)Properties.Resources.DisplayMagician;
                        using (FileStream fs = new FileStream(AppDisplayMagicianIconFilename, FileMode.Create))
                            heliosIcon.Save(fs);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Program/StartUpNormally exception 2: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                    logger.Error(ex, $"Program/StartUpNormally exception create Icon files for future use in {AppIconPath}");
                }

                IPCService.GetInstance().Status = InstanceStatus.User;

                // Run the program with normal startup
                AppMainForm = new MainForm();
                Application.Run(AppMainForm);                

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/StartUpNormally exception 3: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                logger.Error(ex, $"Program/StartUpNormally top level exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                MessageBox.Show(
                    ex.Message,
                    Language.Fatal_Error,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            
        }


        // ReSharper disable once CyclomaticComplexity
        private static void RunShortcut(string shortcutUUID)
        {
            logger.Debug($"Program/RunShortcut: Starting");

            ShortcutItem shortcutToRun = null;

            // Check there is only one version of this application so we won't
            // mess with another monitoring session
            if (
                IPCClient.QueryAll()
                    .Any(
                        client =>
                            client.Status == InstanceStatus.Busy ||
                            client.Status == InstanceStatus.OnHold))
            {
                throw new Exception(
                    Language
                        .Another_instance_of_this_program_is_in_working_state_Please_close_other_instances_before_trying_to_switch_profile);
            }

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

            IPCService.GetInstance().Status = InstanceStatus.Busy;

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

            // Lookup the profile
            ProfileItem profileToUse = ProfileRepository.AllProfiles.Where(p => p.UUID.Equals(profileName)).First();
            logger.Trace($"Program/RunProfile: Found profile called {profileName} and now starting to apply the profile");

            ProfileRepository.ApplyProfile(profileToUse);

        }

        public static bool LoadGamesInBackground()
        {

            logger.Debug($"Program/LoadGamesInBackground: Starting");
            // Now lets prepare loading all the Steam games we have installed
            Action loadSteamGamesAction = new Action(() =>
            {
                // Check if Steam is installed
                GameLibrary steamLibrary = SteamLibrary.GetLibrary();
                if (steamLibrary.IsGameLibraryInstalled)
                {
                    // Load Steam library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed Steam Games");
                    if (!steamLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Steam Games!");
                    }
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed Steam Games (found {steamLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: Steam not installed.");
                    Console.WriteLine("Steam not installed.");
                }
            });

            // Now lets prepare loading all the Uplay games we have installed
            Action loadUplayGamesAction = new Action(() =>
            {
                // Check if Uplay is installed
                GameLibrary uplayLibrary = UplayLibrary.GetLibrary();
                if (uplayLibrary.IsGameLibraryInstalled)
                {
                    // Load Uplay library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed Uplay Games");
                    if (!uplayLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Uplay Games!");
                    }
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed Uplay Games (found {uplayLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: Uplay not installed.");
                    Console.WriteLine("Uplay not installed.");
                }

            });

            // Now lets prepare loading all the Origin games we have installed
            Action loadOriginGamesAction = new Action(() =>
            {
                // Check if Origin is installed
                GameLibrary originLibrary = OriginLibrary.GetLibrary();
                if (originLibrary.IsGameLibraryInstalled)
                {
                    // Load Origin library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed Origin Games");
                    if (!originLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Origin Games!");
                    }
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed Origin Games (found {originLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: Origin not installed.");
                    Console.WriteLine("Origin not installed.");
                }

            });

            // Now lets prepare loading all the Epic games we have installed
            Action loadEpicGamesAction = new Action(() =>
            {
                // Check if Epic is installed
                GameLibrary epicLibrary = OriginLibrary.GetLibrary();
                if (epicLibrary.IsGameLibraryInstalled)
                {
                    // Load Origin library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed Epic Games");
                    if (!epicLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Epic Games!");
                    }
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed Epic Games (found {epicLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: Epic not installed.");
                    Console.WriteLine("Epic not installed.");
                }

            });

            // Now lets prepare loading all the GOG games we have installed
            Action loadGogGamesAction = new Action(() =>
            {
                // Check if GOG is installed
                GameLibrary gogLibrary = GogLibrary.GetLibrary();
                if (gogLibrary.IsGameLibraryInstalled)
                {
                    // Load Origin library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed GOG Games");
                    if (!gogLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed GOG Games!");
                    }
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed GOG Games (found {gogLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: GOG not installed.");
                    Console.WriteLine("GOG not installed.");
                }

            });

            // Store all the actions in a array so we can wait on them later
            List<Action> loadGamesActions = new List<Action>();
            loadGamesActions.Add(loadSteamGamesAction);
            loadGamesActions.Add(loadUplayGamesAction);
            loadGamesActions.Add(loadOriginGamesAction);
            loadGamesActions.Add(loadEpicGamesAction);
            loadGamesActions.Add(loadGogGamesAction);

            try
            {
                logger.Debug($"Program/LoadGamesInBackground: Running game loading actions.");
                // Go through and start all the actions, making sure we only have one threat per action to avoid thread issues
                int threads = loadGamesActions.Count;
                ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = threads };
                Parallel.Invoke(options, loadGamesActions.ToArray());
                logger.Debug($"Program/LoadGamesInBackground: All game loading tasks finished");
            }
            catch (AggregateException ae)
            {
                logger.Error(ae, $"Program/LoadGamesInBackground exception during loadGamesActions");
            }

            // Produce a single array of Games we can reference later
            GameLibrary.AllInstalledGamesInAllLibraries = SteamLibrary.GetLibrary().AllInstalledGames;
            GameLibrary.AllInstalledGamesInAllLibraries.AddRange(UplayLibrary.GetLibrary().AllInstalledGames);
            GameLibrary.AllInstalledGamesInAllLibraries.AddRange(OriginLibrary.GetLibrary().AllInstalledGames);
            GameLibrary.AllInstalledGamesInAllLibraries.AddRange(EpicLibrary.GetLibrary().AllInstalledGames);
            GameLibrary.AllInstalledGamesInAllLibraries.AddRange(GogLibrary.GetLibrary().AllInstalledGames);

            // Create Game Bitmaps from the Games so the rest of the program is faster later
            // Get the bitmap out of the IconPath 
            // IconPath can be an ICO, or an EXE
            foreach (var game in GameLibrary.AllInstalledGamesInAllLibraries)
            {
                Bitmap bm = null;
                try
                {
                    /*ArrayList filesToSearchForIcon = new ArrayList();
                    filesToSearchForIcon.Add(game.ExePath);
                    if (game.IconPath != game.ExePath)
                        filesToSearchForIcon.Add(game.IconPath);

                    bm = ImageUtils.GetMeABitmapFromFile(filesToSearchForIcon);*/

                    // We only want the icon location that the GameLibrary told us to use
                    // Note: This may be an icon file, or an exe file.
                    // This function tries to get a 256x256 Vista sized bitmap from the file
                    logger.Trace($"Program/LoadGamesInBackground: Attempting to get game bitmaps from {game.Name}.");
                    bm = ImageUtils.GetMeABitmapFromFile(game.IconPath);
                    if (bm != null && bm.GetType() == typeof(Bitmap))
                    {
                        logger.Trace($"Program/LoadGamesInBackground: Got game bitmaps from {game.Name}.");
                    }
                    else
                    {
                        logger.Trace($"Program/LoadGamesInBackground: Couldn't get game bitmaps from {game.Name} for some reason.");
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/LoadGamesInBackground: Exception building game bitmaps for {game.Name} during load");                    
                }

                if (bm == null)
                {
                    if (game.GameLibrary.Equals(SupportedGameLibraryType.Steam))
                        bm = Properties.Resources.Steam;
                    else if (game.GameLibrary.Equals(SupportedGameLibraryType.Uplay))
                        bm = Properties.Resources.Uplay;
                    else if (game.GameLibrary.Equals(SupportedGameLibraryType.Origin))
                        bm = Properties.Resources.Origin;
                    else if (game.GameLibrary.Equals(SupportedGameLibraryType.Epic))
                        bm = Properties.Resources.Epic;
                    else if (game.GameLibrary.Equals(SupportedGameLibraryType.GOG))
                        bm = Properties.Resources.GOG;
                    else
                        bm = Properties.Resources.DisplayMagician.ToBitmap();
                }

                game.GameBitmap = bm;
            }

            return true;

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