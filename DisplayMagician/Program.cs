using System;
using McMaster.Extensions.CommandLineUtils;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Forms;
using DisplayMagician.Resources;
using DisplayMagicianShared;
using DisplayMagician.UIForms;
using DisplayMagician.GameLibraries;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using NLog.Config;
using System.Collections.Generic;
using AutoUpdaterDotNET;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.Win32;
using DisplayMagician.Processes;
using NETWORKLIST;
using DisplayMagician.AppLibraries;
using System.ComponentModel;
using System.Text;
using System.Globalization;
using System.Net.Http;
using NLog.Targets;
using System.Security.Policy;
using System.Web;

namespace DisplayMagician {

    public static class Program
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
        public static string AppVersion = ThisAssembly.AssemblyFileVersion;

        public static string AppPermStartMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "DisplayMagician","DisplayMagician.lnk");
        public static string AppTempStartMenuPath = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.Programs),"DisplayMagician.lnk");
        public const string AppUserModelId = "LittleBitBig.DisplayMagician";
        public const string AppActivationId = "4F319902-EB8C-43E6-8A51-8EA74E4308F8";        
        public static bool AppToastActivated = false;
        public static bool AppInstalled = false;
        public static bool AppNewInstall = false;
        public static bool AppVersionUpgrade = false;
        public static string AppLastVersionRun = "0.0";
        public static CancellationTokenSource AppCancellationTokenSource = new CancellationTokenSource();
        //Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        public static SemaphoreSlim AppBackgroundTaskSemaphoreSlim = new SemaphoreSlim(1, 1);

        public static bool WaitingForGameToExit = false;
        public static ProgramSettings AppProgramSettings;
        public static MainForm AppMainForm;
        public static LoadingForm AppSplashScreen;
        public static ShortcutLoadingForm AppShortcutLoadingSplashScreen;
        public static UpgradeExtraDetails? AppUpgradeExtraDetails = null;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static SharedLogger sharedLogger;
        private static bool _gamesLoaded = false;
        private static bool _tempShortcutRegistered = false;
        private static bool _bypassSingleInstanceMode = false;
        public static System.Timers.Timer AppUpdateRemindLaterTimer = null;

        private static readonly System.Net.Http.HttpClient AppHttpClient = new System.Net.Http.HttpClient();

        public enum ERRORLEVEL: int
        {
            OK = 0, // Errorlevel returned when everything has worked as it should
            CANCELED_BY_USER = 1,  // Errorlevel returned when an action was cancelled by a user           
            PROFILE_UNKNOWN = 50, // Errorlevel used in CurrentProfile to return the fact the current display profile is not a saved profile, and so is unknown.
            ERROR_EXCEPTION = 100,  // Errorlevel returned when an excption of some kind has occurred.
            ERROR_CANNOT_FIND_SHORTCUT = 101,  // Errorlevel returned when RunShortcut command is used, and it cannot find the shortcut to run
            ERROR_CANNOT_FIND_PROFILE = 102,  // Errorlevel returned when RunProfile command is used, and it cannot find the profile to apply
            ERROR_APPLYING_PROFILE = 103,  // Errorlevel returned when RunProfile command is used, and it cannot apply the profile for some reason
            ERROR_UNKNOWN_COMMAND = 104, // Errorlevel returned when DisplayMagician is given an unregonised command
        };


        public struct UpgradeExtraDetails
        {
            public bool ManualUpgrade;
            public bool UpdatesDisplayProfiles;
            public bool UpdatesGameShortcuts;
            public bool UpdatesSettings;
        }

        private static List<string> _commandsThatBypassSingleInstanceMode = new List<string>
        {
            // "CurrentProfile",
        };

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {          

            // If the command supplied on the commmand line is a command that bypasses singleinstance mode,
            // then skip the single instance mode tests. This is important for commands used in powershell
            if (args.Length > 0 && _commandsThatBypassSingleInstanceMode.Contains(args[0]))
            {
                _bypassSingleInstanceMode = true;
            }


            // If we're not bypassing single instance mode, then we need to check if we're the single instance, and if we're the second instance then
            // we need to pass the command to the single instance and shutdown.
            if (!_bypassSingleInstanceMode)
            {
                // Create the remote server if we're first instance, or
                // If we're a subsequent instance, pass the command line parameters to the first instance and then 
                bool isFirstInstance = SingleInstance.LaunchOrReturn(args);
                if (isFirstInstance)
                {
                    Console.WriteLine($"Program/Main: This is the first DisplayMagician to start, so will be the one to actually perform the actions.");
                }
                else
                {

                    // if we're the second instance of DisplayMagician, then lets close down as the first instance will continue with what we wanted to do.
                    Console.WriteLine($"Program/Main: There is already another DisplayMagician running, so we'll use that one to actually perform the actions. Closing this instance of Displaymagician.");
                    if (System.Windows.Forms.Application.MessageLoop)
                    {
                        // WinForms have loaded
                        Application.Exit();
                    }
                    else
                    {
                        // Console app
                        Environment.Exit(1);
                    }

                }
            }
            

            // If we get here, then we're the first instance!
            // Explicitly register DisplayMagician with Windows so that it can be found by other programs
            RegisterDisplayMagicianWithWindows();

            // Set up some defaults for the shared HttpClient
            AppHttpClient.Timeout = TimeSpan.FromSeconds(30);

            // Prepare NLog for internal logging - Comment out when not required
            //NLog.Common.InternalLogger.LogLevel = NLog.LogLevel.Debug;
            //NLog.Common.InternalLogger.LogToConsole = true;
            //NLog.Common.InternalLogger.LogFile = "C:\\Users\\terry\\AppData\\Local\\DisplayMagician\\Logs\\nlog-internal.txt";

            var config = new NLog.Config.LoggingConfiguration();
            
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

            // Process the version tracking logic
            AppInstalled = false; 
            AppNewInstall = false;
            AppLastVersionRun = "0.0";
            AppVersionUpgrade = false;

            try
            {
                // Figure out if this is First Run of this version since installed
                RegistryKey DMKey = Registry.CurrentUser.OpenSubKey("Software\\DisplayMagician");
                if (DMKey != null)
                {
                    AppInstalled = true;
                    string newInstallKey = DMKey.GetValue("NewInstall").ToString() ?? "0";
                    if (newInstallKey.Equals("1"))
                    {
                        AppNewInstall = true;
                        logger.Info($"Program/Main: This is the first time this version has run since it was installed! We may have upgrade tasks to do!");
                    }
                    else
                    {
                        logger.Trace($"Program/Main: This is NOT the first time this version has run since it was installed. We've run this version before since it was installed.");
                    }

                }
                else
                {
                    logger.Trace($"Program/Main: DisplayMagician hasn't been installed on this host and is running from somewhere else (e.g. via visual studio).");
                }
            }
            catch (NullReferenceException ex)
            {
                logger.Trace(ex, $"Program/Main: Exception whilst trying to find the NewInstall registry key. It means DisplayMagician hasn't been installed on this host and is running from somewhere else (e.g. via visual studio). Problem accessing registry!");
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/Main: Exception whilst trying to see if this version has run since it was installed. Problem accessing registry!");
            }

            try
            {
                // Figure out if this is version is the same as the last version
                // get the last version from the registry (or this version as fallback)
                string lastVersionString = (string)Registry.CurrentUser.GetValue("Software\\DisplayMagician\\LastVersion", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                Version lastVersion = new Version(lastVersionString);


                if (lastVersion < Assembly.GetExecutingAssembly().GetName().Version)
                {
                    AppVersionUpgrade = true;
                    logger.Info($"Program/Main: This is an upgrade from version {lastVersion} to version {Assembly.GetExecutingAssembly().GetName().Version}!");
                }
                else
                {
                    logger.Trace($"Program/Main: This is NOT an upgrade from version {lastVersion} to version {Assembly.GetExecutingAssembly().GetName().Version}.");
                }

            }
            catch (NullReferenceException ex)
            {
                logger.Trace(ex, $"Program/Main: Exception whilst trying to see what the last version of DM was. It means DisplayMagician hasn't been run before anywhere");
                AppVersionUpgrade = true;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/Main: Exception whilst trying to see if this version has run previously at all. It most likely hasnt been installed and is running from somewhere (e.g. via visual studio). Problem accessing registry!");
                AppVersionUpgrade = false;
            }


            try
            {
                // Try to store this version as the last version run (replacing the previous last run version with this one)
                Registry.CurrentUser.SetValue("Software\\DisplayMagician\\LastVersion", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/Main: Exception whilst trying to set the last version registry key to {Application.ProductVersion}.");
            }
            
            // Load the program settings
            AppProgramSettings = ProgramSettings.LoadSettings();
            // Update the program settings for number times run
            AppProgramSettings.NumberOfStartsSinceLastDonationForm++;
            AppProgramSettings.NumberOfStartsSinceLastDonationButtonAnimation++;
            AppProgramSettings.NumberOfTimesRun++;
            // If app settings is new, then set the initial settings we need
            if (AppNewInstall)
            {
                Guid guid = new Guid();
                if (AppProgramSettings.InstallId == "")
                {
                    AppProgramSettings.InstallId = guid.ToString();
                }
                AppProgramSettings.InstallDate = DateTime.UtcNow;
                // AppProgramSettings.LastDonationDate = new DateTime(1980,1,1);
                // AppProgramSettings.LastDonateButtonAnimationDate = new DateTime(1980, 1, 1);

                // Store the updated settings
                AppProgramSettings.SaveSettings();
            }


            // Rules for mapping loggers to targets          
            /* NLog.LogLevel logLevel = null;
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


            // Targets where to log to: File and Console
            string appLogFilename = Path.Combine(Program.AppLogPath, $"DisplayMagician-{DateTime.UtcNow.ToString("yyyy-MM-dd-HHmm",CultureInfo.InvariantCulture)}.log");

            // Create the log file target
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = appLogFilename,
                MaxArchiveFiles = 4,
                ArchiveAboveSize = 41943040, // 40MB max file size
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}|${onexception:EXCEPTION OCCURRED \\:${exception::format=toString,Properties,Data}"
            };

            // Create a logging rule to use the log file target
            var loggingRule = new LoggingRule("LogToFile");
            loggingRule.EnableLoggingForLevels(logLevel, NLog.LogLevel.Fatal);
            loggingRule.Targets.Add(logfile);
            loggingRule.LoggerNamePattern = "*";
            config.LoggingRules.Add(loggingRule);

            // Apply config           
            NLog.LogManager.Configuration = config;
            
            // Make DisplayMagicianShared use the same log file by sending it the 
            // details of the existing NLog logger
            sharedLogger = new SharedLogger(logger);

            // Start the Log file
            logger.Info($"Program/Main: Starting {Application.ProductName} v{Application.ProductVersion}");

            // Create the other DM Dir if it doesn't exist so that it's avilable for all 
            // parts of the program to use
            if (!Directory.Exists(AppIconPath))
            {
                try
                {
                    Directory.CreateDirectory(AppIconPath);
                    logger.Trace($"Program/Main: Created the Application Icon Folder {AppIconPath}");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/Main: exception: Cannot create the Application Icon Folder {AppIconPath}");
                }
            }
            else
            {
                logger.Trace($"Program/Main: Application Icon Folder {AppIconPath} already exists so skipping creating it");
            }
            if (!Directory.Exists(AppProfilePath))
            {
                try
                {
                    Directory.CreateDirectory(AppProfilePath);
                    logger.Trace($"Program/Main: Created the Application Profile Folder {AppProfilePath}");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/Main: exception: Cannot create the Application Profile Folder {AppProfilePath}");
                }
            }
            else
            {
                logger.Trace($"Program/Main: Application Profile Folder {AppProfilePath} already exists so skipping creating it");
            }
            if (!Directory.Exists(AppShortcutPath))
            {
                try
                {
                    Directory.CreateDirectory(AppShortcutPath);
                    logger.Trace($"Program/Main: Created the Application Shortcut Folder {AppShortcutPath}");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/Main: exception: Cannot create the Application Shortcut Folder {AppShortcutPath}");
                }
            }
            else
            {
                logger.Trace($"Program/Main: Application Shortcut Folder {AppShortcutPath} already exists so skipping creating it");
            }
            if (!Directory.Exists(AppWallpaperPath))
            {
                try
                {
                    Directory.CreateDirectory(AppWallpaperPath);
                    logger.Trace($"Program/Main: Created the Application Wallpaper Folder {AppWallpaperPath}");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/Main: exception: Cannot create the Application Wallpaper Folder {AppWallpaperPath}");
                }
            }
            else
            {
                logger.Trace($"Program/Main: Application Wallpaper Folder {AppWallpaperPath} already exists so skipping creating it");
            }

            // Write the Application Name
            Console.WriteLine($"{Application.ProductName} v{Application.ProductVersion}");
            for (int i = 0; i <= Application.ProductName.Length + Application.ProductVersion.Length; i++)
            {
                Console.Write("=");
            }
            Console.WriteLine("=");
            Console.WriteLine($"Copyright © Terry MacDonald 2020-{DateTime.Today.Year}");
            Console.WriteLine(@"Derived from Helios Display Management - Copyright © Soroush Falahati 2017-2020");
            logger.Trace($"Program/Main: Setting visual styles and rendering mode");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false); 
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (AppVersionUpgrade)
            {
                // Do all the upgrade things
                logger.Info($"Program/Main: This is an upgrade from an earlier DisplayMagician Display Profile format to the current DisplayMagician Display Profile format, so it requires the user manual recreate the display profiles.");

                /* // Warn the user about the fact we need them to recreate their Display Profiles again!
                StartMessageForm myMessageWindow = new StartMessageForm();
                myMessageWindow.MessageMode = "rtf";
                myMessageWindow.URL = "https://displaymagician.littlebitbig.com/messages/DisplayMagicianRecreateProfiles.rtf";
                myMessageWindow.HeadingText = "You need to recreate your Display Profiles";
                myMessageWindow.ButtonText = "&Close";
                myMessageWindow.ShowDialog();
                */

                if (AppInstalled)
                { 
                    try
                    {
                        // Set the registry key to turn off the first run setting.
                        RegistryKey DMKey = Registry.CurrentUser.OpenSubKey("Software\\DisplayMagician");
                        DMKey.SetValue("FirstRun", "0");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        logger.Error(ex, $"Program/Main: UnauthorizedAccessException: Unable to set the FirstRun registry key to 0 as no permission to write to registry key.");
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Program/Main: Exception whilst trying to set the FirstRun registry key to 0.");
                    }
                }

            }


            // Next we try to setup the Registry Keys for the DesktopBackground Context Menu
            // This is redone each time we start so that the context menu is always updated and correct.
            if (AppProgramSettings.InstallDesktopContextMenu)
            {
                ContextMenu.InstallContextMenu();
            }

           
            logger.Trace($"Program/Main: Setting up commandline processing configuration");
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
            //CommandOption forcedVideoLibrary = app.Option("--force-video-library", "Bypass the normal video detection logic to force a particular video library (AMD, NVIDIA, Windows)", CommandOptionType.SingleValue);

            logger.Trace($"Program/Main: Preparing the RunShortcut command...");

            // This is the RunShortcut command
            app.Command(DisplayMagicianStartupAction.RunShortcut.ToString(), (runShortcutCmd) =>
            {
                logger.Trace($"Program/Main: Processing the {runShortcutCmd} command...");

                // Try to load all the games in parallel to this process
                //Task.Run(() => LoadGamesInBackground());

                // Set the --trace or --debug options if supplied
                if (trace.HasValue())
                {
                    Console.WriteLine($"Changing logging level to TRACE level as --trace was provided on the commandline.");
                    logger.Info($"Program/Main: Changing logging level to TRACE level as --trace was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }
                else if (debug.HasValue())
                {
                    Console.WriteLine($"Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    logger.Info($"Program/Main: Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Debug, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }

                var argumentShortcut = runShortcutCmd.Argument("\"SHORTCUT_UUID\"", "(required) The UUID of the shortcut to run from those stored in the shortcut library.").IsRequired();
                argumentShortcut.Validators.Add(new ShortcutMustExistValidator());

                //description and help text of the command.
                runShortcutCmd.Description = "Use this command to run favourite game or application with a display profile of your choosing.";

                runShortcutCmd.OnExecute(() =>
                {
                    logger.Debug($"Program/Main: RunShortcut commandline command was invoked!");

                    // Set up the AppMainForm variable that we need to use later
                    AppMainForm = new MainForm();
                    AppMainForm.Load += MainForm_LoadCompleted;

                    // Load the games in background on execute
                    GameLibrary.LoadGamesInBackground();
                    // Load the apps in background on execute
                    //TODO: Add this back in (Note - this was removed as it was causing a crash on startup)
                    //      Need to investigate why this particular part was crashing everything. 
                    AppLibrary.LoadAppsInBackground();

                    // Close the splash screen
                    if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                        AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));

                    ERRORLEVEL errLevel = RunShortcut(argumentShortcut.Value);
                    DeRegisterDisplayMagicianWithWindows();
                    return (int)errLevel;
                });
            });

            logger.Trace($"Program/Main: Preparing the ChangeProfile command...");

            // This is the ChangeProfile command
            app.Command(DisplayMagicianStartupAction.ChangeProfile.ToString(), (runProfileCmd) =>
            {
                logger.Trace($"Program/Main: Processing the {runProfileCmd} command...");

                // Set the --trace or --debug options if supplied
                if (trace.HasValue())
                {
                    Console.WriteLine($"Changing logging level to TRACE level as --trace was provided on the commandline.");
                    logger.Info($"Program/Main: Changing logging level to TRACE level as --trace was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }
                else if (debug.HasValue())
                {
                    Console.WriteLine($"Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    logger.Info($"Program/Main: Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Debug, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }


                var argumentProfile = runProfileCmd.Argument("\"Profile_UUID\"", "(required) The UUID of the profile to run from those stored in the profile file.").IsRequired();
                argumentProfile.Validators.Add(new ProfileMustExistValidator());

                //description and help text of the command.
                runProfileCmd.Description = "Use this command to change to a display profile of your choosing.";

                runProfileCmd.OnExecute(() =>
                {
                    logger.Debug($"Program/Main: ChangeProfile commandline command was invoked!");

                    // Set up the AppMainForm variable that we need to use later
                    AppMainForm = new MainForm();
                    AppMainForm.Load += MainForm_LoadCompleted;

                    // Close the splash screen
                    if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                        AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));

                    try
                    {
                        ERRORLEVEL errLevel = RunProfile(argumentProfile.Value);
                        DeRegisterDisplayMagicianWithWindows();
                        return (int)errLevel;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Program/Main exception running ApplyProfile(profileToUse)");
                        DeRegisterDisplayMagicianWithWindows();
                        return (int)ERRORLEVEL.ERROR_EXCEPTION;
                    }
                });
            });

            logger.Trace($"Program/Main: Preparing the CreateProfile command...");

            // This is the CreateProfile command
            app.Command(DisplayMagicianStartupAction.CreateProfile.ToString(), (createProfileCmd) =>
            {
                logger.Trace($"Program/Main: Processing the {createProfileCmd} command...");

                // Set the --trace or --debug options if supplied
                if (trace.HasValue())
                {
                    Console.WriteLine($"Changing logging level to TRACE level as --trace was provided on the commandline.");
                    logger.Info($"Program/Main: Changing logging level to TRACE level as --trace was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }
                else if (debug.HasValue())
                {
                    Console.WriteLine($"Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    logger.Info($"Program/Main: Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Debug, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }

                //description and help text of the command.
                createProfileCmd.Description = "Use this command to go directly to the create display profile screen.";

                createProfileCmd.OnExecute(() =>
                {
                    logger.Debug($"Program/Main: CreateProfile commandline command was invoked!");
                    Console.WriteLine("Starting up and creating a new Display Profile...");
                    ERRORLEVEL errLevel = CreateProfile();
                    DeRegisterDisplayMagicianWithWindows();
                    return (int)errLevel;
                });
            });
           

            logger.Trace($"Program/Main: Preparing the default command...");

            app.OnExecute(() =>
            {
                logger.Trace($"Program/Main: Starting the app normally as there was no command supplied...");

                // Set the --trace or --debug options if supplied
                if (trace.HasValue())
                {
                    Console.WriteLine($"Changing logging level to TRACE level as --trace was provided on the commandline.");
                    logger.Info($"Program/Main: Changing logging level to TRACE level as --trace was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }
                else if (debug.HasValue())
                {
                    Console.WriteLine($"Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    logger.Info($"Program/Main: Changing logging level to DEBUG level as --debug was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Debug, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }


                logger.Debug($"Program/Main: No commandline command was invoked, so starting up normally");
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
                            logger.Debug($"Program/Main: We were started by the user clicking on a Windows Toast");
                            Program.AppToastActivated = true;
                            break;
                        }

                    }
                }
                logger.Info("Program/Main: Starting Normally...");
               
                // Try to load all the games in parallel to this process
                //Task.Run(() => LoadGamesInBackground());
                logger.Debug($"Program/Main: Try to load all the Games in the background to avoid locking the UI");
                // Load the games in background on execute
                GameLibrary.LoadGamesInBackground();
                // Load the apps in background on execute
                AppLibrary.LoadAppsInBackground();

               /* // Update the Active Profile before we load the Main Form
                ProfileRepository.UpdateActiveProfile();*/

                // Set up the AppMainForm variable that we need to use later
                AppMainForm = new MainForm();
                AppMainForm.Load += MainForm_LoadCompletedAndOpenApp;

                ERRORLEVEL errLevel = StartUpApplication();
                DeRegisterDisplayMagicianWithWindows();
                return (int)errLevel;
            });

            logger.Trace($"Program/Main: Showing the splashscreen if requested");

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
                logger.Debug($"Invoking commandline processing");
                // This begins the actual execution of the application
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                logger.Error(ex, $"Program/Main exception parsing the Commands passed to the program: ");
                Console.WriteLine($"Didn't recognise the supplied commandline options: - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return (int)ERRORLEVEL.ERROR_UNKNOWN_COMMAND;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Program/Main commandParsingException: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // You'll always want to catch this exception, otherwise it will generate a messy and confusing error for the end user.
                // the message will usually be something like:
                // "Unrecognized command or argument '<invalid-command>'"
                logger.Error(ex, $"Program/Main general exception during app.Execute(args): ");
                Console.WriteLine($"Program/Main exception: Unable to execute application - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
            }

            logger.Debug($"Beginning to shutdown as the app command has finished executing.");

            // Close the splash screen if it's still open (happens with some errors)
            if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
            {
                logger.Trace($"Closing the SplashScreen as it may still be open");
                AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));
            }
                

            logger.Trace($"Program/Main: Clearing all previous windows toast notifications as they aren't needed any longer");
            // Remove all the notifications we have set as they don't matter now!
            ToastNotificationManagerCompat.History.Clear();

            // Shutdown NLog
            logger.Trace($"Program/Main: Stopping logging processes");
            NLog.LogManager.Shutdown();

            // Dispose of the CancellationTokenSource
            Program.AppCancellationTokenSource.Dispose();

            // Exit with a 0 Errorlevel to indicate everything worked fine!
            return (int)ERRORLEVEL.OK;
        }       

        public static ERRORLEVEL CreateProfile()
        {
            logger.Debug($"Program/CreateProfile: Starting");

            ERRORLEVEL errLevel = ERRORLEVEL.OK;
            try
            {
                // Close the splash screen
                if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
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
                errLevel = ERRORLEVEL.ERROR_EXCEPTION;
            }

            return errLevel;
        }

        private static ERRORLEVEL StartUpApplication()
        {
            logger.Debug($"Program/StartUpApplication: Starting");

            ERRORLEVEL errLevel = ERRORLEVEL.OK;

            try
            {

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

                // Check for updates
                CheckForUpdates();

                 // Show any messages we need to show
                ShowMessages();

                // Run the program with normal startup
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

                errLevel = ERRORLEVEL.ERROR_EXCEPTION;
            }

            return errLevel;
        }

        private static void MainForm_LoadCompleted(object sender, EventArgs e)
        {
            if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));
        }

        private static void MainForm_LoadCompletedAndOpenApp(object sender, EventArgs e)
        {
            if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));
            AppMainForm.TopMost = true;
            AppMainForm.Activate();
            AppMainForm.TopMost = false;
        }

        // ReSharper disable once CyclomaticComplexity
        public static ERRORLEVEL RunShortcut(string shortcutUUID)
        {
            logger.Debug($"Program/RunShortcut: Running shortcut {shortcutUUID}");

            ERRORLEVEL errLevel = ERRORLEVEL.OK;
            ShortcutItem shortcutToRun = null;

            // Close the splash screen
            if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));

            // Match the ShortcutName to the actual shortcut listed in the shortcut library
            // And error if we can't find it.
            if (ShortcutRepository.ContainsShortcut(shortcutUUID))
            {
                // make sure we trim the "" if there are any
                shortcutUUID = shortcutUUID.Trim('"');
                shortcutToRun = ShortcutRepository.GetShortcut(shortcutUUID);
                if (shortcutToRun is ShortcutItem)
                {
                    shortcutToRun.RefreshValidity();
                    //ShortcutRepository.RunShortcut(shortcutToRun);
                    Program.RunShortcutTask(shortcutToRun);
                }
            }
            else
            {
                logger.Error($"Program/RunShortcut: Cannot find the shortcut with UUID {shortcutUUID}");
                errLevel = ERRORLEVEL.ERROR_CANNOT_FIND_SHORTCUT;
            }

            return errLevel;

        }

        public static ERRORLEVEL RunProfile(string profileName)
        {
            logger.Trace($"Program/RunProfile: Running profile {profileName}");
            ERRORLEVEL errLevel = ERRORLEVEL.OK;

            // Close the splash screen
            if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));


            if (ProfileRepository.AllProfiles.Where(p => p.UUID.Equals(profileName)).Any())
            {
                logger.Trace($"Program/RunProfile: Found profile called {profileName} and now starting to apply the profile");

                // Get the profile
                ProfileItem profileToUse = ProfileRepository.AllProfiles.Where(p => p.UUID.Equals(profileName)).First();
                
                ApplyProfileResult result = Program.ApplyProfileTask(profileToUse);
                if (result == ApplyProfileResult.Cancelled)
                    errLevel = ERRORLEVEL.CANCELED_BY_USER;
                else if (result == ApplyProfileResult.Error)
                    errLevel = ERRORLEVEL.ERROR_APPLYING_PROFILE;
            }
            else 
            { 
                logger.Error($"Program/RunProfile: We tried looking for a profile called {profileName} and couldn't find it. It probably is an old display profile that has been deleted previously by the user.");
                errLevel = ERRORLEVEL.ERROR_CANNOT_FIND_PROFILE;
            }

            return errLevel;
        }


        public static bool IsValidFilename(string testName)
        {
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(testName)) { return false; };

            return true;
        }

        
        

        //public async static Task<RunShortcutResult> RunShortcutTask(ShortcutItem shortcutToUse, NotifyIcon notifyIcon = null)
        public static RunShortcutResult RunShortcutTask(ShortcutItem shortcutToUse)
        {
            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            if (Program.AppBackgroundTaskSemaphoreSlim.CurrentCount == 0)
            {
                logger.Error($"Program/RunShortcutTask: Cannot run the shortcut {shortcutToUse.Name} as another task is running!");
                return RunShortcutResult.Error;
            }
            //await Program.AppBackgroundTaskSemaphoreSlim.WaitAsync(0);
            bool gotGreenLightToProceed = Program.AppBackgroundTaskSemaphoreSlim.Wait(0);
            if (gotGreenLightToProceed)
            {
                logger.Trace($"Program/RunShortcutTask: Got exclusive control of the RunShortcutTask");
            }
            else
            {
                logger.Error($"Program/RunShortcutTask: Failed to get control of the RunShortcutTask, so unable to continue. Returning an Error.");
                return RunShortcutResult.Error;
            }

            // This line creates a new cancellationtokensource, just in case the user used the last one up cancelling something.
            // Each cancellationtoken can only be consumed once, and then needs to be replaced.
            if (Program.AppCancellationTokenSource != null)
            {
                Program.AppCancellationTokenSource.Dispose();
            }
            Program.AppCancellationTokenSource = new CancellationTokenSource();
            RunShortcutResult result = RunShortcutResult.Error;
            try
            {
                CancellationToken cancelToken = AppCancellationTokenSource.Token;
                // Start the RunShortcut Task in a new thread
                Task<RunShortcutResult> output = Task.Factory.StartNew<RunShortcutResult>(() => ShortcutRepository.RunShortcut(shortcutToUse, ref cancelToken), cancelToken);
                // And then wait here until the task completes
                while (true)
                {
                    Application.DoEvents();
                    Thread.Sleep(2000);
                    if (output.IsCompleted || cancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
                //output.Wait(cancelToken);                
            }
            catch (OperationCanceledException ex)
            {
                logger.Trace(ex, $"Program/RunShortcutTask: User cancelled the running the shortcut {shortcutToUse.Name}.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Program/RunShortcutTask: Exception while trying to run the shortcut {shortcutToUse.Name}.");
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                if (gotGreenLightToProceed)
                {
                    Program.AppBackgroundTaskSemaphoreSlim.Release();
                }
            }
            return result;
        }

        //public async static Task<ApplyProfileResult> ApplyProfileTask(ProfileItem profile)
        public static ApplyProfileResult ApplyProfileTask(ProfileItem profile)
        {
            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            if (Program.AppBackgroundTaskSemaphoreSlim.CurrentCount == 0)
            {
                logger.Error($"Program/ApplyProfileTask: Cannot apply the display profile {profile.Name} as another task is running!");
                return ApplyProfileResult.Error;
            }
            //await Program.AppBackgroundTaskSemaphoreSlim.WaitAsync(0);
            bool gotGreenLightToProceed = Program.AppBackgroundTaskSemaphoreSlim.Wait(0);
            if (gotGreenLightToProceed)
            {
                logger.Trace($"Program/ApplyProfileTask: Got exclusive control of the ApplyProfileTask");
            }
            else
            {
                logger.Error($"Program/ApplyProfileTask: Failed to get control of the ApplyProfileTask, so unable to continue. Returning an Error.");
                return ApplyProfileResult.Error;
            }
            ApplyProfileResult result = ApplyProfileResult.Error;            
            if (Program.AppCancellationTokenSource != null)
            {
                Program.AppCancellationTokenSource.Dispose();
            }                
            Program.AppCancellationTokenSource = new CancellationTokenSource();
            try
            {
                Task<ApplyProfileResult> taskToRun = Task.Run(() => ProfileRepository.ApplyProfile(profile));
                taskToRun.Wait(120);
                result = taskToRun.Result;
            }   
            catch (OperationCanceledException ex)
            {
                logger.Trace(ex, $"Program/ApplyProfileTask: User cancelled the ApplyProfile {profile.Name}.");
            }
            catch( Exception ex)
            {
                logger.Error(ex, $"Program/ApplyProfileTask: Exception while trying to apply Profile {profile.Name}.");
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                if (gotGreenLightToProceed)
                {
                    Program.AppBackgroundTaskSemaphoreSlim.Release();
                }                        
            }

            //taskToRun.RunSynchronously();
            //result = taskToRun.GetAwaiter().GetResult();                
            if (result == ApplyProfileResult.Successful)
            {
                MainForm myMainForm = Program.AppMainForm;
                if (myMainForm.InvokeRequired)
                {
                    myMainForm.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate {
                        myMainForm.UpdateNotifyIconText($"DisplayMagician ({profile.Name})");
                    });
                }
                else
                {
                    myMainForm.UpdateNotifyIconText($"DisplayMagician ({profile.Name})");
                }

                logger.Trace($"Program/ApplyProfileTask: Successfully applied Profile {profile.Name}.");
            }
            else if (result == ApplyProfileResult.Cancelled)
            {
                logger.Warn($"Program/ApplyProfileTask: The user cancelled changing to Profile {profile.Name}.");
            }
            else
            {
                logger.Warn($"Program/ApplyProfileTask: Error applying the Profile {profile.Name}. Unable to change the display layout.");
            }

            // Replace the code above with this code when it is time for the UI rewrite, as it is non-blocking
            //result = await Task.Run(() => ProfileRepository.ApplyProfile(profile));
            return result;
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
            string indexUrl = "https://displaymagician.littlebitbig.com/messages/index_2.0.json";
            try
            {
                json = AppHttpClient.GetStringAsync(indexUrl).Result;
                //json = client.DownloadString(indexUrl);
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
           
            foreach (MessageItem message in messageIndex)
            {
                // Skip if we've already shown it
                if (message.Id <= AppProgramSettings.LastMessageIdRead)
                {
                    // Unless it's one coming up that we're monitoring
                    if (!AppProgramSettings.MessagesToMonitor.Contains(message.Id))
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

                    if (!(DateTime.UtcNow >= startTime))
                    {
                        logger.Debug($"Program/ShowMessages: Message start date for \"{message.HeadingText}\" (#{message.Id}) not yet reached so not ready to show message.");
                        if (!AppProgramSettings.MessagesToMonitor.Contains(message.Id))
                        {
                            AppProgramSettings.MessagesToMonitor.Add(message.Id);
                            AppProgramSettings.SaveSettings();
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

                    if (!(DateTime.UtcNow <= endTime))
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
                if (AppProgramSettings.MessagesToMonitor.Contains(message.Id))
                {
                    AppProgramSettings.MessagesToMonitor.Remove(message.Id);
                    AppProgramSettings.SaveSettings();
                }

                // Update the latest message id to keep track of where we're up to
                if (message.Id > AppProgramSettings.LastMessageIdRead)
                {
                    AppProgramSettings.LastMessageIdRead = message.Id;
                }
                
            }
            
        }

        public static void CheckForUpdates()
        {
            // Firstly check if the user wants to upgrade at all
            // If not, just return
            if (!Program.AppProgramSettings.UpgradeEnabled)
            {
                logger.Warn($"Program/CheckForUpdates: User has set the Program Settings to ignore any DisplayMagician updates. Skipping the auto update.");
                return;
            }

            // Second of all, check to see if there is any way to get to the internet on this computer.
            // If not, then why bother!
            try
            {              

                INetworkListManager networkListManager = new NetworkListManager();
                //dynamic networkListManager = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("{DCB00C01-570F-4A9B-8D69-199FDBA5723B}")));
                bool isConnected = networkListManager.IsConnectedToInternet;
                if (!isConnected)
                {
                    logger.Warn($"Program/CheckForUpdates: No internet detected. Skipping the auto update.");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/CheckForUpdates: Exception while trying to get all the network interfaces to make sure we have internet connectivity. Attempting to auto update anyway.");
            }


            //Run the AutoUpdater to see if there are any updates available.
            //FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
            //AutoUpdater.InstalledVersion = new Version(fvi.FileVersion);
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
            AutoUpdater.RunUpdateAsAdmin = true;
            AutoUpdater.HttpUserAgent = "DisplayMagician AutoUpdater";
            AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Days;
            AutoUpdater.RemindLaterAt = 7;
            AutoUpdater.InstalledVersion = new Version(AppVersion);
            if (Program.AppProgramSettings.UpgradeToPreReleases == false)
            {
                string connectionUrl = "http://displaymagician.littlebitbig.com/update/update_2.5.json";
                connectionUrl += ($"?version={HttpUtility.UrlEncode(Program.AppProgramSettings.DisplayMagicianVersion)}");
                connectionUrl += ($"&id={HttpUtility.UrlEncode(Program.AppProgramSettings.InstallId)}");
                AutoUpdater.Start(connectionUrl);
            }
            else
            {
                string connectionUrl = "http://displaymagician.littlebitbig.com/update/prerelease_2.5.json";
                connectionUrl += ($"?version={HttpUtility.UrlEncode(Program.AppProgramSettings.DisplayMagicianVersion)}");
                connectionUrl += ($"&id={HttpUtility.UrlEncode(Program.AppProgramSettings.InstallId)}");
                AutoUpdater.Start(connectionUrl);

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
                    CurrentVersion = (string)json["autoupdate"]["version"],
                    ChangelogURL = (string)json["autoupdate"]["changelog"],
                    DownloadURL = (string)json["autoupdate"]["url"],
                    Mandatory = new Mandatory
                    {
                        Value = (bool)json["autoupdate"]["mandatory"]["value"],
                        UpdateMode = (Mode)(int)json["autoupdate"]["mandatory"]["mode"],
                        MinimumVersion = (string)json["autoupdate"]["mandatory"]["minVersion"]
                    },
                    CheckSum = new CheckSum
                    {
                        Value = (string)json["autoupdate"]["checksum"]["value"],
                        HashingAlgorithm = (string)json["autoupdate"]["checksum"]["hashingAlgorithm"]
                    }
                };
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Program/AutoUpdaterOnParseUpdateInfoEvent: Exception trying to create an UpdateInfoEventArgs object from the received Update JSON file.");
            }

            // Also record the DisplayMagician Update settings.
            try 
            {
                logger.Trace($"MainForm/AutoUpdaterOnParseUpdateInfoEvent: Trying to create an UpgradeExtraDetails object from the received Update JSON file.");
                AppUpgradeExtraDetails = new UpgradeExtraDetails
                {
                    ManualUpgrade = (bool)json["extraDetails"]["manualUpgrade"],
                    UpdatesDisplayProfiles = (bool)json["extraDetails"]["updatesDisplayProfiles"],
                    UpdatesGameShortcuts = (bool)json["extraDetails"]["updatesGameShortcuts"],
                    UpdatesSettings = (bool)json["extraDetails"]["updatesSettings"],
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
                    UpgradeForm upgradeForm = new UpgradeForm();

                    StringBuilder message= new StringBuilder();
                    message.Append(@"{\rtf1\ansi \qc \line \line \line ");
                    

                    if (args.Mandatory.Value)
                    {
                        logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - New version {args.CurrentVersion} available. Current version is {args.InstalledVersion}. Mandatory upgrade.");
                        message.Append($@"There is a new version {args.CurrentVersion} available. You are using version {args.InstalledVersion}. This is a mandatory update. \line \line ");
                    }
                    else
                    {
                        logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - New version {args.CurrentVersion} available. Current version is {args.InstalledVersion}. Optional upgrade.");

                        message.Append($@"There is a new version {args.CurrentVersion} available. You are currently using version {args.InstalledVersion}. \line \line ");                        
                    }

                    if (Program.AppUpgradeExtraDetails.HasValue)
                    {
                        message.Append(@"\b ");
                        if (AppUpgradeExtraDetails.Value.ManualUpgrade)
                        {
                            // Manual upgrade required. This list tells the user what steps that is.
                            if (AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will require you to manually recreate your Display Profiles, recreate your Game Shortcuts and check your DisplayMagician settings. ");
                            }
                            else if (AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && !AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will require you to manually recreate your Display Profiles and recreate your Game Shortcuts. ");
                            }
                            else if (AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && !AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will require you to manually recreate your Display Profiles and check your DisplayMagician settings. ");
                            }
                            else if (AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && !AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && !AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will require you to manually recreate your Display Profiles. ");
                            }
                            else if (!AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will require you to recreate your Game Shortcuts and check your DisplayMagician settings. ");
                            }
                            else if (!AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && !AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will require you to manually recreate your Game Shortcuts. ");
                            }
                            else if (!AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && !AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will require you to manually check your DisplayMagician settings. ");
                            }
                            else if (!AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && !AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && !AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will require you to perform some manual upgrade tasks yourself. ");
                            }
                        }
                        else
                        {
                            // Automatic upgrade required. This list tells the user what steps that is.
                            if (AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will automatically update your Display Profiles, your Game Shortcuts and DisplayMagician settings as part of the upgrade process. ");
                            }
                            else if (AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && !AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will automatically update your Display Profiles and your Game Shortcuts as part of the upgrade process. ");
                            }
                            else if (AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && !AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will automatically update your Display Profiles and DisplayMagician settings as part of the upgrade process. ");
                            }
                            else if (AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && !AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && !AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will automatically update your Display Profiles as part of the upgrade process. ");
                            }
                            else if (!AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will automatically update your Game Shortcuts and DisplayMagician settings as part of the upgrade process. ");
                            }
                            else if (!AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && !AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will automatically update your Game Shortcuts as part of the upgrade process. ");
                            }
                            else if (!AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && !AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will automatically update your DisplayMagician settings as part of the upgrade process. ");
                            }
                            else if (!AppUpgradeExtraDetails.Value.UpdatesDisplayProfiles && !AppUpgradeExtraDetails.Value.UpdatesGameShortcuts && !AppUpgradeExtraDetails.Value.UpdatesSettings)
                            {
                                message.Append($@"The upgrade will automatically update your DisplayMagician configuration as part of the upgrade process. ");
                            }
                        }
                    }
                    message.Append(@"\line \line ");
                    message.Append(@"\b0 ");

                    message.Append($@"Press 'Upgrade now' to update, 'Remind me later' to remind you again in a week's time, or 'Skip' to continue without upgrading.");
                    message.Append(@"}");

                    upgradeForm.Message = message.ToString();

                    dialogResult = upgradeForm.ShowDialog();

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
                    else if (dialogResult.Equals(DialogResult.Cancel) && upgradeForm.Remind)
                    {
                        // The user wants us to remind them in 7 days
                        // We need to set up a timer to do so (code adapted from AutoUpdater.net internal code)
                        AutoUpdater.PersistenceProvider.SetSkippedVersion(null);

                        DateTime remindLaterDateTime = DateTime.UtcNow;
                        switch (AutoUpdater.RemindLaterTimeSpan)
                        {
                            case RemindLaterFormat.Days:
                                remindLaterDateTime = DateTime.UtcNow + TimeSpan.FromDays(AutoUpdater.RemindLaterAt);
                                break;
                            case RemindLaterFormat.Hours:
                                remindLaterDateTime = DateTime.UtcNow + TimeSpan.FromHours(AutoUpdater.RemindLaterAt);
                                break;
                            case RemindLaterFormat.Minutes:
                                remindLaterDateTime = DateTime.UtcNow + TimeSpan.FromMinutes(AutoUpdater.RemindLaterAt);
                                break;
                        }

                        AutoUpdater.PersistenceProvider.SetRemindLater(remindLaterDateTime);
                        
                        TimeSpan timeSpan = remindLaterDateTime - DateTime.UtcNow;

                        var context = SynchronizationContext.Current;

                        AppUpdateRemindLaterTimer = new System.Timers.Timer
                        {
                            Interval = Math.Max(1, timeSpan.TotalMilliseconds),
                            AutoReset = false
                        };

                        AppUpdateRemindLaterTimer.Elapsed += delegate
                        {
                            AppUpdateRemindLaterTimer = null;
                            if (context != null)
                            {
                                try
                                {
                                    context.Send(_ => CheckForUpdates(), null);
                                }
                                catch (InvalidAsynchronousStateException)
                                {
                                    CheckForUpdates();
                                }
                            }
                            else
                            {
                                CheckForUpdates();
                            }
                        };

                        AppUpdateRemindLaterTimer.Start();
                        
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
                    MessageBox.Show($"Program/AutoUpdaterOnCheckForUpdateEvent - There was a problem performing the update: {args.Error.Message}",
                        args.Error.GetType().ToString(), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private static void RegisterDisplayMagicianWithWindows()
        {
            // Listen to notification activation
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                // Obtain any user input (text boxes, menu selections) from the notification
                //ValueSet userInput = toastArgs.UserInput;

                // Need to dispatch to UI thread if performing UI operations
                /*Application.Current.Dispatcher.Invoke(delegate
                {
                    // TODO: Show the corresponding content
                    MessageBox.Show("Toast activated. Args: " + toastArgs.Argument);
                });*/

                // This code is running on the main UI thread!
                // Parse the query string (using NuGet package QueryString.NET)
                if (args.Contains("action"))
                {
                    // See what action is being requested 
                    switch (args["action"])
                    {
                        // Open the Main window
                        case "open":

                            // Open the Main DisplayMagician Window, if the app has started and the mainform is loaded
                            if (Program.AppMainForm != null)
                            {
                                Program.AppMainForm.Invoke((System.Windows.Forms.MethodInvoker)delegate
                                {
                                    Program.AppMainForm.openApplicationWindow();
                                });
                                
                            }                                
                            break;

                        // Exit the application
                        case "exit":

                            // Exit the application (overriding the close restriction)                            
                            if (Program.AppMainForm != null)
                            {
                                Program.AppMainForm.Invoke((System.Windows.Forms.MethodInvoker)delegate
                                {
                                    Program.AppMainForm.exitApplication();
                                });

                            }
                            break;

                        // Stop waiting so that the monitoring stops, and the UI becomes free
                        case "stopWaiting":
                            
                            if (Program.AppMainForm != null)
                            {
                                Program.AppMainForm.Invoke((System.Windows.Forms.MethodInvoker)delegate
                                {
                                    Program.AppCancellationTokenSource.Cancel();
                                });

                            }
                            break;

                        default:
                            break;
                    }
                }

            };

            try
            {
                if (!IsInstalledVersion())
                {
                    // Force toasts to work if we're not 'installed' per se by creating a temp DisplayMagician start menu icon
                    // Allows running from a ZIP file rather than forcing the app to be installed. If we don't do this then Toasts just wouldn't work.
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
                    if (rk == null) 
                    {
                        return false;
                    }
                    if (rk.GetValue("InstallDir") != null && rk.GetValue("InstallDir").ToString() == thisInstallDir)
                    {
                        return true; //exists
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/IsInstalledVersion: DisplayMagician InstallDir isn't in registry! This DisplayMagician isn't installed.");
                return false;
            }
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
        public LoadingInstalledGamesException(string message, string gameName) : base(message)
        { }
    }
}