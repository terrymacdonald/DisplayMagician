using System;
using McMaster.Extensions.CommandLineUtils;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Forms;
using DisplayMagicianShared;
using DisplayMagician.UIForms;
using DisplayMagician.GameLibraries;
using System.Text.RegularExpressions;
using System.Drawing;
using NLog.Config;
using System.Collections.Generic;
using AutoUpdaterDotNET;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using DisplayMagician.AppLibraries;
using System.ComponentModel;
using System.Text;
using System.Globalization;
using System.Web;
using Vortice.DirectInput;
using System.Diagnostics;
using DisplayMagician.Messaging;

using Windows.ApplicationModel;
using Windows.Management.Deployment;


namespace DisplayMagician {

    public static class Program
    {
        internal static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        public static string AppStartupPath = Application.StartupPath;
        public static string AppIconPath = Path.Combine(Program.AppDataPath, $"Icons");
        public static string AppProfilePath = Path.Combine(Program.AppDataPath, $"Profiles");
        public static string AppShortcutPath = Path.Combine(Program.AppDataPath, $"Shortcuts");
        public static string AppWallpaperPath = Path.Combine(Program.AppDataPath, $"Wallpaper");
        public static string AppMessagesPath = Path.Combine(Program.AppDataPath, $"Messages");
        public static string AppLogPath = Path.Combine(Program.AppDataPath, $"Logs");
        public static string AppDisplayMagicianIconFilename = Path.Combine(AppIconPath, @"DisplayMagician.ico");
        public static string AppOriginIconFilename = Path.Combine(AppIconPath, @"Origin.ico");
        public static string AppSteamIconFilename = Path.Combine(AppIconPath, @"Steam.ico");
        public static string AppUplayIconFilename = Path.Combine(AppIconPath, @"Uplay.ico");
        public static string AppEpicIconFilename = Path.Combine(AppIconPath, @"Epic.ico");
        public static string AppDownloadsPath = Utils.GetDownloadsPath();
        public static string AppVersion = ThisAssembly.AssemblyFileVersion;
        public static DirectInputManager AppDirectInputManager;

        public static string AppIdentityPkgPath = Path.Combine(Application.StartupPath, "DisplayMagicianIdentityPkg.msix");
        public static string AppPermStartMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "DisplayMagician","DisplayMagician.lnk");
        public static string AppTempStartMenuPath = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.Programs),"DisplayMagician.lnk");
        public const string AppUserModelId = "LittleBitBig.DisplayMagician";
        public const string AppActivationId = "4F319902-EB8C-43E6-8A51-8EA74E4308F8";        
        public static bool AppToastActivated = false;
        public static bool AppNotInstalled = false;
        //public static bool AppInstalled = false;
        //public static bool AppNewInstall = false;
        //public static bool AppVersionUpgrade = false;
        public static bool AppHasPackageIdentity = false;
        //public static string AppLastVersionRun = "0.0";
        public static CancellationTokenSource AppCancellationTokenSource = new CancellationTokenSource();
        //Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        public static SemaphoreSlim AppBackgroundTaskSemaphoreSlim = new SemaphoreSlim(1, 1);

        public static List<Game> AppGameList = new List<Game>();
        public static List<App> AppAppList = new List<App>();
        public static bool WaitingForGameToExit = false;
        public static ProgramSettings AppProgramSettings;
        public static DonationSettings AppDonationSettings;
        public static MainForm AppMainForm;
        public static LoadingForm AppSplashScreen;
        public static ShortcutLoadingForm AppShortcutLoadingSplashScreen;
        public static UpgradeExtraDetails? AppUpgradeExtraDetails = null;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static SharedLogger sharedLogger;        
        
        private static bool _tempShortcutRegistered = false;
        //private static bool _bypassSingleInstanceMode = false;
        public static System.Timers.Timer AppUpdateRemindLaterTimer = null;
        private static NLog.LogLevel _userWantedLogLevel = NLog.LogLevel.Info; // Default log level is Info, but can be changed later based on user settings
        private static bool _userOverrodeLogLevel = false; // Used to track if the user has overridden the log level via command line options
        private static readonly System.Net.Http.HttpClient AppHttpClient = new System.Net.Http.HttpClient();
        private static bool _packageIdentityWarningNeeded = false;
        private static bool _autoUpdaterEventsRegistered = false;
        private static bool _lastUpdateCheckWasAutomatic = true;
        private static string _requestedMessageUpdateVersion;
        private static string _requestedMessageUpdateChannel;
        private static bool _startupBackgroundTasksQueued = false;
        private static SynchronizationContext _mainSynchronizationContext;
        private static readonly SemaphoreSlim _messageSyncSemaphore = new SemaphoreSlim(1, 1);
        private static MessageSyncService _messageSyncService;
        private static System.Timers.Timer _messageSyncTimer;
        private static System.Timers.Timer _startupMessagePollTimer;
        private static readonly TimeSpan _messageSyncPollInterval = TimeSpan.FromHours(1);
        internal const string MessageManifestUrl = "http://www.displaymagician.com:8787/messages/manifest.json";

        private const string UpdateUrl = "http://www.displaymagician.com:8787/update/update.json";

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
            ERROR_PROFILE_CHANGE_OCCURRING = 105, // Errorlevel returned when DisplayMagician is already making a display profile change and is unable to comeplete what the user requested at this time. Try again soon. 
        };

        public struct UpgradeExtraDetails
        {
            //public bool PreleaseUpgrade;
            public bool ManualUpgrade;
            public bool UpdatesDisplayProfiles;
            public bool UpdatesGameShortcuts;
            public bool UpdatesSettings;
        }



        //private static List<string> _commandsThatBypassSingleInstanceMode = new List<string>
        //{
        //    // "CurrentProfile",
        //};

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {
            // BOOTSTRAP AND INITIALIZATION LOGIC

            // Create the Logging Dir if it doesn't exist so that it's avilable for all
            // parts of the program to use
            if (!Directory.Exists(AppDataPath))
            {
                try
                {
                    Directory.CreateDirectory(AppDataPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Program/Main Exception: Cannot create the Application Data  Folder {AppDataPath} - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                }
            }

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


            
            // Prepare NLog for internal logging - Comment out when not required
            //NLog.Common.InternalLogger.LogLevel = NLog.LogLevel.Debug;
            //NLog.Common.InternalLogger.LogToConsole = true;
            //NLog.Common.InternalLogger.LogFile = "C:\\Users\\terry\\AppData\\Local\\DisplayMagician\\Logs\\nlog-internal.txt";

            var config = new NLog.Config.LoggingConfiguration();

            // To enable us to start logging early, set the logLevel to Info, and then later on we can change it if the user wants it different
            NLog.LogLevel logLevel = NLog.LogLevel.Info;
            _userWantedLogLevel = NLog.LogLevel.Info;
            if (args.Contains("--debug"))
            {
                // Set things to debug mode as the user provided this on the command line
                logLevel = NLog.LogLevel.Debug;
                _userWantedLogLevel = NLog.LogLevel.Trace;
                _userOverrodeLogLevel = true; // User has overridden the log level to debug, so we will use this for the rest of the program
            }
            else if (args.Contains("--trace"))
            {
                // Set things to trace mode as the user provided this on the command line
                logLevel = NLog.LogLevel.Trace;
                _userWantedLogLevel = NLog.LogLevel.Trace;
                _userOverrodeLogLevel = true; // User has overridden the log level to trace, so we will use this for the rest of the program
            }

            // Targets where to log to: File and Console
            string appLogFilename = Path.Combine(Program.AppLogPath, $"DisplayMagician-{DateTime.Now.ToString("yyyy-MM-dd-HHmm", CultureInfo.InvariantCulture)}.log");

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


            // PACKAGE IDENTITY INITIALIZATION AND CHECKS
            EnsurePackageIdentity();

            // SINGLE INSTANCE MODE CHECKS
            // If the command supplied on the commmand line is a command that bypasses singleinstance mode,
            // then skip the single instance mode tests. This is important for commands used in powershell
            //logger.Trace($"Program/Main: Checking if the user has provided a command that bypasses single instance mode.");
            //if (args.Length > 0 && _commandsThatBypassSingleInstanceMode.Contains(args[0]))
            //{
            //    logger.Trace($"Program/Main: The user has provided a command that bypasses single instance mode. We have enabled bypass single instance mode.");
            //    _bypassSingleInstanceMode = true;
            //}

            // If we're not bypassing single instance mode, then we need to check if we're the single instance, and if we're the second instance then
            // we need to pass the command to the single instance and shutdown.
            //if (!_bypassSingleInstanceMode)
            //{
            //logger.Trace($"Program/Main: We're not bypassing single instance mode so we need to check if we're the only instance, otherwise we have to shutdown and send that first instance our command.");


            // Check if we're the single instance, and if we're the second instance then we need to pass the command to the single instance and shutdown.
            // Create the remote server if we're first instance, or If we're a subsequent instance, pass the command line parameters to the first instance and then 
            logger.Trace($"Program/Main: Running the SingleInstance.LaunchOrReturn function to act as either the first or subsequent instances.");
            bool isFirstInstance = SingleInstance.LaunchOrReturn(args);
            if (isFirstInstance)
            {
                logger.Trace($"Program/Main: We are the first DisplayMagician to start, so will be the one to actually perform the actions if we ever get sent any.");
            }
            else
            {
                // if we're the second instance of DisplayMagician, then                   
                // lets close down as the first instance will continue with what we wanted to do.
                logger.Trace($"Program/Main: There is already another DisplayMagician running, so we'll use that one to actually perform the actions. Closing this instance of DisplayMagician.");
                if (Application.MessageLoop)
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
            //}


            // If we get here, then we're the first instance!
            // Explicitly register DisplayMagician with Windows so that it can be found by other programs
            logger.Trace($"Program/Main: Registering DisplayMagician with Windows.");
            RegisterDisplayMagicianWithWindows();

            logger.Trace($"Program/Main: Setting high DPI mode, visual styles and rendering mode");
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Set up some defaults for the shared HttpClient
            AppHttpClient.Timeout = TimeSpan.FromSeconds(30);

            // Check if DisplayMagician is not installed (and is portable) by looking for the installer registry key for this executable path.
            // We need to know this so that we can handle certain things differently for installed vs portable users, such as where we store the settings file, and whether we show the "you need to install DisplayMagician" message when certain errors occur that we can detect are due to the fact the user is running in portable mode without realising it.
            AppNotInstalled = DMIsNotInstalled();          

            // Just log some info to the log file so that the users can understand it.
            if (File.Exists(ProgramSettings.ProgramSettingsStorageJsonFullFileName))
            {
                logger.Info($"Program/Main: Existing settings file found. This programme has been run before.");
            }
            else
            {
                logger.Info($"Program/Main: No settings file found. Treating this as a new installation.");
            }

            // AppVersionUpgrade is determined in UpdateStartupModeFromSettings once the settings file has been loaded,
            // by comparing the version stored in settings against the currently running version.
            

            // MIGRATE ANY CONFIG CHANGES IF NEEDED, THEN LOAD THE SETTINGS
            // Upgrade the configuration files if needed
            logger.Trace($"Program/Main: Running migration logic.");
            ConfigMigrationRunner.MigrationResult migrationResult = ConfigMigrationRunner.RunMigrationsDetailed();
            if (!migrationResult.Success)
            {
                logger.Error($"Program/Main: ERROR - DisplayMagician could not load or migrate the configuration file {ProgramSettings.ProgramSettingsStorageJsonFullFileName}: {migrationResult.Message}");
                if (!RecoverProgramSettingsFile(migrationResult.Message))
                {
                    return (int)ERRORLEVEL.CANCELED_BY_USER;
                }
            }

            // Load the settings from the settings file properly now that we've done the version upgrade if needed. 
            logger.Trace($"Program/Main: Loading Program Settings.");
            AppProgramSettings = ProgramSettings.LoadSettings();
            if (AppProgramSettings == null)
            {
                logger.Error($"Program/Main: ERROR - DisplayMagician could not load the configuration file {ProgramSettings.ProgramSettingsStorageJsonFullFileName}.");
                if (!RecoverProgramSettingsFile("DisplayMagician could not load the settings file."))
                {
                    return (int)ERRORLEVEL.CANCELED_BY_USER;
                }

                //AppNewInstall = true;
                AppProgramSettings = ProgramSettings.LoadSettings();
                if (AppProgramSettings == null)
                {
                    logger.Error($"Program/Main: ERROR - DisplayMagician could not load a brand new configuration file after the original one was faulty and couldn't be loaded.");
                    return (int)ERRORLEVEL.ERROR_EXCEPTION;
                }
            }

            //UpdateStartupModeFromSettings(legacyLastVersionString);

            //logger.Trace($"Program/Main: Ensuring Install Identity by setting install id and install date");
            //if (AppProgramSettings.EnsureInstallIdentity(AppNewInstall))
            //{
            //    logger.Trace($"Program/Main: Saving Program Settings to write new install identity.");
            //    AppProgramSettings.SaveSettings();
            //}

            // Load the Donation Settings and update the number of times run and number of starts since last donation form and button animation, and save the settings back to the file
            logger.Trace($"Program/Main: Loading Donation Settings.");
            AppDonationSettings = DonationSettings.LoadSettings();
            logger.Trace($"Program/Main: Updating Donation Settings counters.");
            AppDonationSettings.NumberOfStartsSinceLastDonationForm++;
            AppDonationSettings.NumberOfStartsSinceLastDonationButtonAnimation++;
            AppDonationSettings.NumberOfTimesRun++;
            logger.Trace($"Program/Main: Saving Donation Settings.");
            AppDonationSettings.SaveSettings();

            // Remove old unneeded user registry keys
            CleanupLegacyUserRegistryValues();

            // Set up the start on bookup if the user wants it, and remove it if they don't want it, but
            try
            {
                if (AppProgramSettings.StartOnBootUp)
                {
                    if (!StartupManager.IsStartupEnabled())
                    {
                        logger.Info($"Program/ReconcilePerUserRegistryState: Startup registry value is missing or stale. Recreating it from settings.");
                        StartupManager.EnableStartup();
                    }
                }
                else
                {
                    StartupManager.DisableStartup();
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/ReconcilePerUserRegistryState: Could not reconcile per-user startup registry state.");
            }

            // Remove the context menu if the user wanted it removed earlier
            try
            {
                if (!AppProgramSettings.InstallDesktopContextMenu)
                {
                    ContextMenu.UninstallContextMenu();
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/ReconcilePerUserRegistryState: Could not remove stale per-user DisplayMagician desktop context menu registry state.");
            }


            // UPDATE LOGGING LEVEL BASED ON USER SETTINGS
            // Now we are at the point that the user settings are loaded, we can set the logging level based on the stored user settings
            // but only if the user hasn't already overridden the log level via command line options

            if (!_userOverrodeLogLevel)
            {
                // If the user has set a log level in the settings, then use that, otherwise use the default of Info
                if (AppProgramSettings.LogLevel != null && AppProgramSettings.LogLevel != "")
                {
                    // Set the log level to the user wanted log level
                    _userWantedLogLevel = NLog.LogLevel.FromString(AppProgramSettings.LogLevel);
                    logger.Info($"Program/Main: User has set the log level to {_userWantedLogLevel} in the settings file.");
                    // Also  update the logging level in logger
                    logger.Trace($"Program/Main: Setting the log level to {_userWantedLogLevel} as it was loaded from the settings file.");
                    config.FindRuleByName("LogToFile").SetLoggingLevels(_userWantedLogLevel, NLog.LogLevel.Fatal);
                    // apply the new logging configuration
                    logger.Trace($"Program/Main: Reconfiguring the updated logging configuration.");
                    NLog.LogManager.ReconfigExistingLoggers();

                }
            }
            else
            {
                logger.Trace($"Program/Main: User has set the log level to {_userWantedLogLevel} via command line options so no need to use the log level from program settings.");
            }


            // STARTUP UI AND OTHER INITIALIZATION
            logger.Trace($"Program/Main: Checking if we should show the loading splashscreen...");
            if (AppProgramSettings.ShowSplashScreen)
            {
                logger.Trace($"Program/Main: Showing the splashscreen as the user wants it shown");
                //Show Splash Form
                AppSplashScreen = new LoadingForm();
                var splashThread = new Thread(new ThreadStart(
                    () => Application.Run(AppSplashScreen)));
                splashThread.SetApartmentState(ApartmentState.STA);
                splashThread.Start();
            }
            else
            {
                logger.Trace($"Program/Main: Not showing the splashscreen as the user wants it hidden");
            }


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

            //if (AppVersionUpgrade)
            //{
            //    // Do all the upgrade things
            //    logger.Info($"Program/Main: This is an upgrade from an earlier DisplayMagician Display Profile format to the current DisplayMagician Display Profile format, so it requires the user manual recreate the display profiles.");

            //    /* // Warn the user about the fact we need them to recreate their Display Profiles again!
            //    StartMessageForm myMessageWindow = new StartMessageForm();
            //    myMessageWindow.MessageMode = "rtf";
            //    myMessageWindow.URL = "https://displaymagician.littlebitbig.com/messages/DisplayMagicianRecreateProfiles.rtf";
            //    myMessageWindow.HeadingText = "You need to recreate your Display Profiles";
            //    myMessageWindow.ButtonText = "&Close";
            //    myMessageWindow.ShowDialog();
            //    */

            //}

            // Next we try to setup the Registry Keys for the DesktopBackground Context Menu
            // This is redone each time we start so that the context menu is always updated and correct.
            if (AppProgramSettings.InstallDesktopContextMenu)
            {
                logger.Trace($"Program/Main: Installing the context menu on startup");
                ContextMenu.InstallContextMenu();
            }

            // Next we create the MainForm object but keep it hidden for now
            logger.Trace($"Program/Main: Creating the MainForm object");
            AppMainForm = new MainForm();

            StartDirectInputManager();
            SingleInstance.MarkReadyForCommands();

            // PARSE THE COMMAND LINE AND EXECUTE THE RELEVANT ACTIONS
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
                return string.Format("Version {0}", Program.AppVersion);
            });

            CommandOption appDebug = app.Option("--debug", "Generate a DisplayMagician.log debug-level log file", CommandOptionType.NoValue);
            CommandOption appTrace = app.Option("--trace", "Generate a DisplayMagician.log trace-level log file", CommandOptionType.NoValue);

            logger.Trace($"Program/Main: Preparing the RunShortcut command...");

            // This is the RunShortcut command
            app.Command(DisplayMagicianStartupAction.RunShortcut.ToString(), (runShortcutCmd) =>
            {
                logger.Trace($"Program/Main: Setting up the {DisplayMagicianStartupAction.RunShortcut.ToString()} command...");

                var argumentShortcut = runShortcutCmd.Argument("\"SHORTCUT_UUID\"", "(required) The UUID of the shortcut to run from those stored in the shortcut library.").IsRequired();
                argumentShortcut.Validators.Add(new ShortcutMustExistValidator());

                //description and help text of the command.
                runShortcutCmd.Description = "Use this command to run favourite game or application with a display profile of your choosing.";

                CommandOption debug = runShortcutCmd.Option("--debug", "Generate a DisplayMagician.log debug-level log file", CommandOptionType.NoValue);
                CommandOption trace = runShortcutCmd.Option("--trace", "Generate a DisplayMagician.log trace-level log file", CommandOptionType.NoValue);

                runShortcutCmd.OnExecute(() =>
                {
                    logger.Debug($"Program/Main: RunShortcut commandline command was invoked!");

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
                                       

                    logger.Trace($"Program/Main: Closing the splashscreen if it is open.");
                    // Close the splash screen
                    if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                        AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));

                    try
                    {
                        logger.Trace($"Program/Main: Starting the RunShortcut process with Shortcut UUID {argumentShortcut.Value.ToString()}.");
                        ERRORLEVEL errLevel = RunShortcut(argumentShortcut.Value);
                        logger.Trace($"Program/Main: Deregistering DisplayMagician with Windows.");
                        DeRegisterDisplayMagicianWithWindows();
                        logger.Trace($"Program/Main: Returning errorlevel {errLevel} to the calling function.");
                        return (int)errLevel;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Program/Main exception attempting to run RunShortcut(shortcutToUse)");
                        DeRegisterDisplayMagicianWithWindows();
                        logger.Trace($"Program/Main: Returning errorlevel ERROR_EXCEPTION to the calling function.");
                        return (int)ERRORLEVEL.ERROR_EXCEPTION;
                    }
                });
            });

            logger.Trace($"Program/Main: Preparing the ChangeProfile command...");

            // This is the ChangeProfile command
            app.Command(DisplayMagicianStartupAction.ChangeProfile.ToString(), (runProfileCmd) =>
            {
                logger.Trace($"Program/Main: Setting up the {DisplayMagicianStartupAction.ChangeProfile.ToString()} command...");

                var argumentProfile = runProfileCmd.Argument("\"Profile_UUID\"", "(required) The UUID of the profile to run from those stored in the profile file.").IsRequired();
                argumentProfile.Validators.Add(new ProfileMustExistValidator());

                //description and help text of the command.
                runProfileCmd.Description = "Use this command to change to a display profile of your choosing.";

                CommandOption debug = runProfileCmd.Option("--debug", "Generate a DisplayMagician.log debug-level log file", CommandOptionType.NoValue);
                CommandOption trace = runProfileCmd.Option("--trace", "Generate a DisplayMagician.log trace-level log file", CommandOptionType.NoValue);

                runProfileCmd.OnExecute(() =>
                {
                    logger.Debug($"Program/Main: ChangeProfile commandline command was invoked!");

                    // Set the --trace or --debug options if supplied
                    if (trace.HasValue())
                    {
                        logger.Info($"Program/Main: Changing logging level to TRACE level as --trace was provided on the commandline.");
                        loggingRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
                        NLog.LogManager.ReconfigExistingLoggers();
                    }
                    else if (debug.HasValue())
                    {
                        logger.Info($"Program/Main: Changing logging level to DEBUG level as --debug was provided on the commandline.");
                        loggingRule.SetLoggingLevels(NLog.LogLevel.Debug, NLog.LogLevel.Fatal);
                        NLog.LogManager.ReconfigExistingLoggers();
                    }

                   /* logger.Trace($"Program/Main: Loading the MainForm");
                    // Set up the AppMainForm variable that we need to use later
                    AppMainForm = new MainForm();
                    AppMainForm.Load += MainForm_LoadCompleted;*/

                    logger.Trace($"Program/Main: Closing the Splashscreen if it is open.");

                    // Close the splash screen
                    if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                        AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));

                    try
                    {
                        logger.Trace($"Program/Main: Starting the RunProfile process with Profile UUID {argumentProfile.Value.ToString()}.");
                        ERRORLEVEL errLevel = RunProfile(argumentProfile.Value);
                        logger.Trace($"Program/Main: Deregistering DisplayMagician with Windows.");
                        DeRegisterDisplayMagicianWithWindows();
                        logger.Trace($"Program/Main: Returning errorlevel {errLevel} to the calling function.");
                        return (int)errLevel;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Program/Main exception running RunProfile(profileToUse):");
                        logger.Trace($"Program/Main: Deregistering DisplayMagician with Windows.");
                        DeRegisterDisplayMagicianWithWindows();
                        logger.Trace($"Program/Main: Returning errorlevel ERROR_EXCEPTION to the calling function.");
                        return (int)ERRORLEVEL.ERROR_EXCEPTION;
                    }
                });
            });

            logger.Trace($"Program/Main: Preparing the CreateProfile command...");

            // This is the CreateProfile command
            app.Command(DisplayMagicianStartupAction.CreateProfile.ToString(), (createProfileCmd) =>
            {
                logger.Trace($"Program/Main: Setting up the {DisplayMagicianStartupAction.CreateProfile.ToString()} command...");               

                //description and help text of the command.
                createProfileCmd.Description = "Use this command to go directly to the create display profile screen.";


                CommandOption debug = createProfileCmd.Option("--debug", "Generate a DisplayMagician.log debug-level log file", CommandOptionType.NoValue);
                CommandOption trace = createProfileCmd.Option("--trace", "Generate a DisplayMagician.log trace-level log file", CommandOptionType.NoValue);

                createProfileCmd.OnExecute(() =>
                {
                    logger.Debug($"Program/Main: CreateProfile commandline command was invoked!");

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

                    try
                    {
                        logger.Trace($"Program/Main: Starting the CreateProfile process to create a new display profile.");
                        ERRORLEVEL errLevel = CreateProfile();
                        logger.Trace($"Program/Main: Deregistering DisplayMagician with Windows.");
                        DeRegisterDisplayMagicianWithWindows();
                        logger.Trace($"Program/Main: Returning errorlevel {errLevel} to the calling function.");
                        return (int)errLevel;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Program/Main exception running CreateProfile:");
                        logger.Trace($"Program/Main: Deregistering DisplayMagician with Windows.");
                        DeRegisterDisplayMagicianWithWindows();
                        logger.Trace($"Program/Main: Returning errorlevel ERROR_EXCEPTION to the calling function.");
                        return (int)ERRORLEVEL.ERROR_EXCEPTION;
                    }
                });
            });
           

            logger.Trace($"Program/Main: Preparing the default command...");

            app.OnExecute(() =>
            {
                logger.Trace($"Program/Main: Starting the app normally as there was no command supplied...");

                // Set the --trace or --debug options if supplied
                if (appTrace.HasValue())
                {
                    Console.WriteLine($"Changing logging level to TRACE level as --trace was provided on the commandline.");
                    logger.Info($"Program/Main: Changing logging level to TRACE level as --trace was provided on the commandline.");
                    loggingRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
                    NLog.LogManager.ReconfigExistingLoggers();
                }
                else if (appDebug.HasValue())
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
                logger.Trace($"Program/Main: Looking for any other commandline arguments provided.");
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
                        else
                        {
                            logger.Warn($"Program/Main: WARNING - Found other Remaining Argument that is not supported: {myArg}");
                        }

                    }
                }
                logger.Info("Program/Main: Starting Normally...");

                

                /* // Update the Active Profile before we load the Main Form
                 ProfileRepository.UpdateActiveProfile();*/

                //AppMainForm.Load += MainForm_LoadCompletedAndOpenApp;

                try
                {
                    logger.Trace($"Program/Main: Starting the application normally as no commands were provided.");
                    ERRORLEVEL errLevel = StartUpApplication();
                    logger.Trace($"Program/Main: Deregistering DisplayMagician with Windows.");
                    DeRegisterDisplayMagicianWithWindows();
                    logger.Trace($"Program/Main: Returning errorlevel {errLevel} to the calling function.");
                    return (int)errLevel;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/Main exception running StartUpApplication():");
                    logger.Trace($"Program/Main: Deregistering DisplayMagician with Windows.");
                    DeRegisterDisplayMagicianWithWindows();
                    logger.Trace($"Program/Main: Returning errorlevel ERROR_EXCEPTION to the calling function.");
                    return (int)ERRORLEVEL.ERROR_EXCEPTION;
                }

            });            

            // default level of errorlevel to return to the OS is OK (unless overridden)
            int errorLevelToReturnToOS = (int)ERRORLEVEL.OK;

            try
            {
                // Close the splash screen if it's still open (happens with some errors)
                if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                {
                    logger.Trace($"Closing the SplashScreen as it may still be open");
                    AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));
                }

                logger.Debug($"Executing the app.execute commandline processing to start parsing the command line options");
                // This begins the actual execution of the application
                errorLevelToReturnToOS = app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                logger.Error(ex, $"Program/Main exception parsing the Commands passed to the program: ");
                return (int)ERRORLEVEL.ERROR_UNKNOWN_COMMAND;
            }
            catch (Exception ex)
            {
                // You'll always want to catch this exception, otherwise it will generate a messy and confusing error for the end user.
                // the message will usually be something like:
                // "Unrecognized command or argument '<invalid-command>'"
                logger.Error(ex, $"Program/Main general exception during app.Execute(args): ");
            }

            logger.Debug($"SHUTDOWN HAS BEGUN! The app command has finished executing and we're starting to get ready for shutdown.");

            // Close the splash screen if it's still open (happens with some errors)
            if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
            {
                logger.Trace($"Closing the SplashScreen as it may still be open");
                AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));
            }

            logger.Trace($"Program/Main: Clearing all previous windows toast notifications as they aren't needed any longer");
            // Remove all the notifications we have set as they don't matter now!
            ToastNotificationManagerCompat.History.Clear();

            logger.Trace($"Program/Main: Stopping message sync timer.");
            _messageSyncTimer?.Stop();
            _messageSyncTimer?.Dispose();
            _messageSyncTimer = null;

            logger.Trace($"Program/Main: Disposing the DirectInput manager.");
            AppDirectInputManager?.Dispose();
            AppDirectInputManager = null;

            // Shutdown NLog
            logger.Trace($"Program/Main: Stopping logging processes");
            NLog.LogManager.Shutdown();

            logger.Trace($"Program/Main: Disposing the CancellationToken");
            // Dispose of the CancellationTokenSource
            Program.AppCancellationTokenSource.Dispose();

            // Exit with a 0 Errorlevel to indicate everything worked fine!
            logger.Trace($"Program/Main: Returning the following errorlevel to the OS: {errorLevelToReturnToOS} ({((ERRORLEVEL)errorLevelToReturnToOS).ToString()})");
            return errorLevelToReturnToOS;
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

                // Enable the MainForm to be shown
                AppMainForm.AllowVisible = true;

                // Run the program with directly showing CreateProfile form
                Application.Run(new DisplayProfileForm());

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/CreateProfile exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                logger.Error(ex, $"Program/CreateProfile top level exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                MessageBox.Show(
                    ex.Message,
                    "Fatal Error",
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
                        Icon heliosIcon = (Icon)Properties.Resources.displaymagician;
                        using (FileStream fs = new FileStream(AppDisplayMagicianIconFilename, FileMode.Create))
                            heliosIcon.Save(fs);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/StartUpApplication exception create Icon files for future use in {AppIconPath}");
                }

                Application.Idle += QueueStartupBackgroundTasks;

                // Close the splash screen
                if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                    AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));

                // Run the program with normal startup
                Application.Run(AppMainForm);                

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Program/StartUpApplication top level exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                MessageBox.Show(
                    ex.Message,
                    "Fatal Error",
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

            if (ProfileRepository.UserChangingProfiles)
            {
                logger.Error($"Program/RunShortcut: The User is currently changing to another Display Profile. We can't run a Game Shortcut until that has finished happening. Please wait.");
                MessageBox.Show("The User is currently changing to another Display Profile. We can't run a Game Shortcut until that has finished happening. Please wait.", "User changing profiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ERRORLEVEL.ERROR_PROFILE_CHANGE_OCCURRING;
            }


            // Match the ShortcutName to the actual shortcut listed in the shortcut library
            // And error if we can't find it.
            if (ShortcutRepository.ContainsShortcut(shortcutUUID))
            {
                // make sure we trim the "" if there are any
                shortcutUUID = shortcutUUID.Trim('"');
                shortcutToRun = ShortcutRepository.GetShortcut(shortcutUUID);
                if (shortcutToRun is ShortcutItem)
                {
                    // We need to update the active profile if we've been run from a shortcut.
                    ProfileRepository.UpdateActiveProfile();
                    // Now refresh the shortcut validity
                    shortcutToRun.RefreshValidity();
                    //ShortcutRepository.RunShortcut(shortcutToRun);
                    RunShortcutResult shortcutResult = Program.RunShortcutTask(shortcutToRun);
                    if (shortcutResult == RunShortcutResult.Cancelled)
                        errLevel = ERRORLEVEL.CANCELED_BY_USER;
                    else if (shortcutResult == RunShortcutResult.Error)
                        errLevel = ERRORLEVEL.ERROR_EXCEPTION;
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

            if (ProfileRepository.UserChangingProfiles)
            {
                logger.Error($"Program/RunProfile: The User is currently changing to another Display Profiles. We can't change to another Display Profile right now. Please wait.");
                MessageBox.Show("The User is currently changing to another Display Profiles. We can't change to another Display Profile right now. Please wait.", "User changing profiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ERRORLEVEL.ERROR_PROFILE_CHANGE_OCCURRING;
            }

            if (ProfileRepository.AllProfiles.Where(p => p.UUID.Equals(profileName)).Any())
            {
                logger.Trace($"Program/RunProfile: Found profile called {profileName} and now starting to apply the profile");

                // Get the profile
                ProfileItem profileToUse = ProfileRepository.AllProfiles.Where(p => p.UUID.Equals(profileName)).First();

                // We need to update the active profile if we've been run from a profile shortcut.
                ProfileRepository.UpdateActiveProfile();

                // Only apply the profile if it is not already active
                if (ProfileRepository.IsActiveProfile(profileToUse))
                {
                    logger.Trace($"Program/RunProfile: Profile {profileToUse.Name} is already the active profile. Notifying user.");
                    new ToastContentBuilder()
                        .AddText("Display Profile Already Active", hintMaxLines: 1)
                        .AddText($"\"{profileToUse.Name}\" is already the current display profile.")
                        .AddAudio(new Uri("ms-winsoundevent:Notification.Default"), false, true)
                        .SetToastDuration(ToastDuration.Short)
                        .Show();
                }
                else
                {
                    // Apply the profile change
                    ApplyProfileResult result = Program.ApplyProfileTask(profileToUse);
                    if (result == ApplyProfileResult.Successful)
                    {
                        logger.Trace($"Program/RunProfile: Profile {profileToUse.Name} was successfully applied.");
                        new ToastContentBuilder()
                            .AddText("Display Profile Applied", hintMaxLines: 1)
                            .AddText($"\"{profileToUse.Name}\" has been applied successfully.")
                            .AddAudio(new Uri("ms-winsoundevent:Notification.Default"), false, true)
                            .SetToastDuration(ToastDuration.Short)
                            .Show();
                    }
                    else if (result == ApplyProfileResult.Cancelled)
                        errLevel = ERRORLEVEL.CANCELED_BY_USER;
                    else if (result == ApplyProfileResult.Error)
                        errLevel = ERRORLEVEL.ERROR_APPLYING_PROFILE;
                }
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
            //await Program.AppBackgroundTaskSemaphoreSlim.WaitAsync(0);
            bool gotGreenLightToProceed = Program.AppBackgroundTaskSemaphoreSlim.Wait(0);
            if (gotGreenLightToProceed)
            {
                logger.Trace($"Program/RunShortcutTask: Got exclusive control of the RunShortcutTask");
            }
            else
            {
                logger.Error($"Program/RunShortcutTask: Cannot run the shortcut {shortcutToUse.Name} as another task is running!");
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
                Task<RunShortcutResult> output = Task.Factory.StartNew<RunShortcutResult>(() => ShortcutRepository.RunShortcut(shortcutToUse, cancelToken), cancelToken);
                // Wait for the task to complete (RunShortcut runs on a background thread)
                output.Wait(cancelToken);
                result = output.Result;
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
            //await Program.AppBackgroundTaskSemaphoreSlim.WaitAsync(0);
            bool gotGreenLightToProceed = Program.AppBackgroundTaskSemaphoreSlim.Wait(0);
            if (gotGreenLightToProceed)
            {
                logger.Trace($"Program/ApplyProfileTask: Got exclusive control of the ApplyProfileTask");
            }
            else
            {
                logger.Error($"Program/ApplyProfileTask: Cannot apply the display profile {profile.Name} as another task is running!");
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
                bool completed = taskToRun.Wait(TimeSpan.FromSeconds(120));
                if (completed)
                    result = taskToRun.Result;
                else
                    logger.Warn($"Program/ApplyProfileTask: Profile apply task timed out after 120 seconds.");
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

        private static void EnsurePackageIdentity()
        {
            if (ExecutionMode.TryGetPackageFullName(out string packageFullName, out int errorCode))
            {
                AppHasPackageIdentity = true;
                logger.Info($"Program/EnsurePackageIdentity: DisplayMagician is running with package identity {packageFullName}.");
                return;
            }

            AppHasPackageIdentity = false;
            logger.Warn($"Program/EnsurePackageIdentity: DisplayMagician is not running with package identity. GetCurrentPackageFullName returned {errorCode}.");

            if (!File.Exists(AppIdentityPkgPath))
            {
                logger.Warn($"Program/EnsurePackageIdentity: Cannot register package identity because {AppIdentityPkgPath} does not exist.");
                _packageIdentityWarningNeeded = true;
                return;
            }

            bool registrationSucceeded = RegisterPackageWithExternalLocationAsync(AppStartupPath, AppIdentityPkgPath).GetAwaiter().GetResult();
            if (registrationSucceeded)
            {
                logger.Info($"Program/EnsurePackageIdentity: Package identity registration completed. Re-checking current process identity.");
            }
            else
            {
                logger.Warn($"Program/EnsurePackageIdentity: Package identity registration did not complete successfully.");
            }

            if (ExecutionMode.TryGetPackageFullName(out packageFullName, out errorCode))
            {
                AppHasPackageIdentity = true;
                logger.Info($"Program/EnsurePackageIdentity: DisplayMagician is now running with package identity {packageFullName}.");
            }
            else
            {
                AppHasPackageIdentity = false;
                _packageIdentityWarningNeeded = true;
                logger.Warn($"Program/EnsurePackageIdentity: DisplayMagician still does not have package identity after registration attempt. GetCurrentPackageFullName returned {errorCode}. UWP and Xbox app monitoring will be disabled for this run.");
            }
        }

        private static void QueueStartupBackgroundTasks(object sender, EventArgs e)
        {
            if (_startupBackgroundTasksQueued)
                return;

            _startupBackgroundTasksQueued = true;
            Application.Idle -= QueueStartupBackgroundTasks;
            _mainSynchronizationContext = SynchronizationContext.Current;

            if (_packageIdentityWarningNeeded)
            {
                ShowPackageIdentityWarningToast();
            }

            Task.Run(async () =>
            {
                // Start the background message poller
                try
                {
                    await RunMessageSyncAndNotifyUserAsync(force: true);
                    EnsureMessageSyncTimer();
                    EnsureStartupMessagePollTimer();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"Program/QueueStartupBackgroundTasks: Automatic message sync failed (force=true, manifestUrl={MessageManifestUrl}, appVersion={AppVersion}, messagesPath={AppMessagesPath}). DisplayMagician will continue running.");
                }

                // Start the background update poller
                try
                {
                    CheckForUpdates(true);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"Program/QueueStartupBackgroundTasks: Automatic update check failed. DisplayMagician will continue running.");
                }
            });
        }

        private static void EnsureMessageSyncTimer()
        {
            if (_messageSyncTimer != null)
            {
                return;
            }

            _messageSyncTimer = new System.Timers.Timer
            {
                Interval = _messageSyncPollInterval.TotalMilliseconds,
                AutoReset = true,
                Enabled = true,
            };

            _messageSyncTimer.Elapsed += async (_, __) =>
            {
                try
                {
                    await RunMessageSyncAndNotifyUserAsync(force: false);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"Program/EnsureMessageSyncTimer: Periodic message sync failed (force=false, intervalHours={_messageSyncPollInterval.TotalHours}, manifestUrl={MessageManifestUrl}, appVersion={AppVersion}).");
                }
            };

            _messageSyncTimer.Start();
        }

        private static void EnsureStartupMessagePollTimer()
        {
            if (_startupMessagePollTimer != null)
            {
                return;
            }

            _startupMessagePollTimer = new System.Timers.Timer
            {
                Interval = TimeSpan.FromMinutes(1).TotalMilliseconds,
                AutoReset = true,
                Enabled = true,
            };

            _startupMessagePollTimer.Elapsed += (_, __) =>
            {
                try
                {
                    List<LocalMessage> storedMessages = GetStoredMessages();
                    if (storedMessages == null || !storedMessages.Any(m => !m.IsRead && m.ShowOnStartup && !m.IsFaulty && string.Equals(m.Kind, "standard", StringComparison.OrdinalIgnoreCase)))
                    {
                        return;
                    }

                    bool gotLock = AppBackgroundTaskSemaphoreSlim.Wait(0);
                    if (!gotLock)
                    {
                        return;
                    }

                    try
                    {
                        if (AppMainForm != null && AppMainForm.IsHandleCreated)
                        {
                            AppMainForm.Invoke((System.Windows.Forms.MethodInvoker)delegate
                            {
                                List<LocalMessage> messagesToShow = GetStoredMessages()
                                    .Where(m => !m.IsRead && m.ShowOnStartup && !m.IsFaulty && string.Equals(m.Kind, "standard", StringComparison.OrdinalIgnoreCase))
                                    .OrderBy(m => m.ReceivedUtc)
                                    .ToList();

                                foreach (LocalMessage message in messagesToShow)
                                {
                                    SetMessageReadState(new[] { message.Id }, true);

                                    string fullPath = Path.Combine(AppMessagesPath, message.MarkdownFileName ?? string.Empty);
                                    if (!File.Exists(fullPath))
                                    {
                                        continue;
                                    }

                                    StartMessageForm myMessageWindow = new StartMessageForm();
                                    myMessageWindow.MessageMode = message.Format;
                                    myMessageWindow.Filename = fullPath;
                                    myMessageWindow.HeadingText = message.Title;
                                    myMessageWindow.ButtonText = "&Close";
                                    myMessageWindow.ShowDialog(AppMainForm);
                                }

                                RefreshMessageIndicators();
                            });
                        }
                    }
                    finally
                    {
                        AppBackgroundTaskSemaphoreSlim.Release();
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "Program/StartupMessagePollTimer: Error checking or showing startup messages.");
                }
            };

            _startupMessagePollTimer.Start();
        }

        private static async Task RunMessageSyncAndNotifyUserAsync(bool force)
        {
            if (_messageSyncService == null)
            {
                _messageSyncService = new MessageSyncService(AppHttpClient, logger, MessageManifestUrl, AppMessagesPath);
                _messageSyncService.EnsureStorage();
            }

            if (!force && !_messageSyncService.IsDailyCheckDue())
            {
                logger.Trace($"Program/RunMessageSyncAndNotifyUserAsync: Skipping sync because daily check is not due yet (force={force}).");
                return;
            }

            if (!await _messageSyncSemaphore.WaitAsync(0).ConfigureAwait(false))
            {
                logger.Warn($"Program/RunMessageSyncAndNotifyUserAsync: Skipping sync because another message sync is already in progress (force={force}).");
                return;
            }

            try
            {
                MessageSyncResult syncResult = await _messageSyncService.SyncMessagesAsync(AppVersion, CancellationToken.None).ConfigureAwait(false);
                if (!syncResult.Success)
                {
                    logger.Warn($"Program/RunMessageSyncAndNotifyUserAsync: Sync completed with failure (force={force}, appVersion={AppVersion}, unreadCount={syncResult.UnreadCount}).");
                    return;
                }

                logger.Info($"Program/RunMessageSyncAndNotifyUserAsync: Sync completed successfully (force={force}, newMessages={syncResult.NewMessagesCount}, unreadCount={syncResult.UnreadCount}).");

                if (syncResult.NewMessagesCount > 0 && AppProgramSettings?.ShowMessageToasts != false)
                {
                    ShowNewMessagesToast(syncResult.NewMessagesCount);
                }
                else if (syncResult.NewMessagesCount > 0)
                {
                    logger.Info($"Program/RunMessageSyncAndNotifyUserAsync: New messages were synced but message toasts are disabled in settings.");
                }

                RefreshMessageIndicators();
            }
            finally
            {
                _messageSyncSemaphore.Release();
            }
        }

        private static MessageSyncService EnsureMessageSyncService()
        {
            if (_messageSyncService == null)
            {
                _messageSyncService = new MessageSyncService(AppHttpClient, logger, MessageManifestUrl, AppMessagesPath);
                _messageSyncService.EnsureStorage();
            }

            return _messageSyncService;
        }

        public static List<LocalMessage> GetStoredMessages()
        {
            return EnsureMessageSyncService().GetMessages();
        }

        public static int GetUnreadMessageCount()
        {
            return EnsureMessageSyncService().GetUnreadCount();
        }

        public static void SetMessageReadState(IEnumerable<string> ids, bool isRead)
        {
            EnsureMessageSyncService().SetReadState(ids, isRead);
        }

        public static void RefreshMessageIndicators()
        {
            try
            {
                if (AppMainForm == null)
                {
                    return;
                }

                int unread = GetUnreadMessageCount();
                AppMainForm.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    AppMainForm.SetUnreadMessageCount(unread);
                });
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/RefreshMessageIndicators: Failed to refresh unread indicator (mainFormNull={AppMainForm == null}).");
            }
        }

        private static void ShowNewMessagesToast(int newMessagesCount)
        {
            try
            {
                string headerText = newMessagesCount == 1
                    ? "You have 1 new message"
                    : $"You have {newMessagesCount} new messages";

                new ToastContentBuilder()
                    .AddText(headerText, hintMaxLines: 1)
                    .AddText("Open DisplayMagician Messages to read them now, or read later.")
                    .AddButton(new ToastButton()
                        .SetContent("Read Now")
                        .AddArgument("action", "readMessagesNow")
                        .SetBackgroundActivation())
                    .AddButton(new ToastButton()
                        .SetContent("Read Later")
                        .AddArgument("action", "readMessagesLater")
                        .SetBackgroundActivation())
                    .AddAudio(new Uri("ms-winsoundevent:Notification.Default"), false, true)
                    .SetToastDuration(ToastDuration.Short)
                    .Show();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/ShowNewMessagesToast: Could not show messages toast.");
            }
        }

        private static void HandleReadMessagesNowAction()
        {
            if (Program.AppMainForm == null)
            {
                logger.Warn($"Program/HandleReadMessagesNowAction: Received readMessagesNow action but AppMainForm is null, so Messages window cannot be opened.");
                return;
            }

            try
            {
                Program.AppMainForm.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    Program.AppMainForm.openApplicationWindow();
                    Program.AppMainForm.openMessagesWindow(selectNewestUnread: true);
                });
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/HandleReadMessagesNowAction: Failed to open Messages window from toast action.");
            }
        }

        private static void ShowPackageIdentityWarningToast()
        {
            try
            {
                new ToastContentBuilder()
                    .AddText("DisplayMagician UWP/Xbox monitoring disabled", hintMaxLines: 1)
                    .AddText("Windows failed to give DisplayMagician permission to monitor UWP and Xbox apps. Please restart DisplayMagician if that functionality is needed.")
                    .AddAudio(new Uri("ms-winsoundevent:Notification.Default"), false, true)
                    .SetToastDuration(ToastDuration.Short)
                    .Show();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/ShowPackageIdentityWarningToast: Could not show package identity warning toast.");
            }
        }

        private static void StartDirectInputManager()
        {
            try
            {
                logger.Trace($"Program/StartDirectInputManager: Creating DirectInput Device Manager.");
                AppDirectInputManager = new DirectInputManager();
                logger.Trace($"Program/StartDirectInputManager: Initialising DirectInput Device Manager with the MainForm window handle.");
                AppDirectInputManager.Initialize(AppMainForm.Handle);
                logger.Trace($"Program/StartDirectInputManager: Registering stored keys and buttons with the DirectInput Device Manager.");
                AppDirectInputManager.RegisterStoredHotkeys(AppProgramSettings);
                AppDirectInputManager.Start(pollIntervalMs: 50);
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/StartDirectInputManager: DirectInput hotkeys could not be started. DisplayMagician will continue without keyboard/joystick hotkeys.");
                AppDirectInputManager?.Dispose();
                AppDirectInputManager = null;
            }
        }

        private static bool RecoverProgramSettingsFile(string reason)
        {
            string settingsFileName = ProgramSettings.ProgramSettingsStorageJsonFullFileName;
            string message = $"DisplayMagician could not load your settings file:\n\n{settingsFileName}\n\n{reason}\n\nSelect Yes to move the problem file aside and create a new blank settings file. Select No to exit DisplayMagician without changing the file.";
            DialogResult recoveryChoice = MessageBox.Show(message, "DisplayMagician settings problem", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (recoveryChoice != DialogResult.Yes)
            {
                logger.Warn($"Program/RecoverProgramSettingsFile: User chose to exit rather than create a blank settings file.");
                return false;
            }

            try
            {
                if (File.Exists(settingsFileName))
                {
                    string backupFileName = CreateRecoveryBackupFileName(settingsFileName);
                    File.Move(settingsFileName, backupFileName);
                    logger.Info($"Program/RecoverProgramSettingsFile: Moved invalid settings file to {backupFileName}.");
                }

                ProgramSettings blankSettings = new ProgramSettings();
                blankSettings.EnsureInstallIdentity(true);
                blankSettings.SaveSettings();
                logger.Info($"Program/RecoverProgramSettingsFile: Created a new blank settings file at {settingsFileName}.");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Program/RecoverProgramSettingsFile: Failed to create a blank settings file.");
                MessageBox.Show($"DisplayMagician could not create a new blank settings file. Please check the log file for details.", "DisplayMagician settings problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static string CreateRecoveryBackupFileName(string settingsFileName)
        {
            string backupFileName = $"{settingsFileName}.invalid-{DateTime.UtcNow:yyyyMMddHHmmss}.bak";
            int suffix = 1;
            while (File.Exists(backupFileName))
            {
                backupFileName = $"{settingsFileName}.invalid-{DateTime.UtcNow:yyyyMMddHHmmss}-{suffix}.bak";
                suffix++;
            }

            return backupFileName;
        }

        //private static void UpdateStartupModeFromSettings()
        //{
        //    Version currentVersion = ParseVersionOrDefault(Program.AppVersion, new Version("0.0.0.0"));
        //    Version lastVersion = currentVersion;

        //    if (!AppNewInstall)
        //    {
        //        string lastVersionString = AppProgramSettings.HasStoredDisplayMagicianVersion
        //            ? AppProgramSettings.DisplayMagicianVersion
        //            : "2.7.2.0";

        //        lastVersion = ParseVersionOrDefault(lastVersionString, currentVersion);
        //    }

        //    AppLastVersionRun = lastVersion.ToString();
        //    AppVersionUpgrade = !AppNewInstall && lastVersion < currentVersion;

        //    if (AppNewInstall)
        //    {
        //        logger.Info($"Program/UpdateStartupModeFromSettings: DisplayMagician is starting with a new settings file.");
        //    }
        //    else if (AppVersionUpgrade)
        //    {
        //        logger.Info($"Program/UpdateStartupModeFromSettings: DisplayMagician is upgrading from version {lastVersion} to version {currentVersion}.");
        //    }
        //    else
        //    {
        //        logger.Trace($"Program/UpdateStartupModeFromSettings: DisplayMagician is running as a standard startup. Last version was {lastVersion}; current version is {currentVersion}.");
        //    }

        //    if (!AppInstalled)
        //    {
        //        logger.Info($"Program/UpdateStartupModeFromSettings: DisplayMagician is running from a folder that does not match the installer registry state. This is valid for portable, dev, or copied-folder runs.");
        //    }
        //}

        private static Version ParseVersionOrDefault(string versionText, Version fallback)
        {
            if (!string.IsNullOrWhiteSpace(versionText) && Version.TryParse(versionText, out Version parsedVersion))
            {
                return parsedVersion;
            }

            return fallback;
        }

        private static void CleanupLegacyUserRegistryValues()
        {
            // We don't want any of these, so lets clean things up if they are there. They aren't used anymore but they might be left over from old versions of the program.

            try
            {
                using (RegistryKey dmKey = Registry.CurrentUser.OpenSubKey(@"Software\DisplayMagician", writable: true))
                {
                    if (dmKey == null)
                        return;

                    if (dmKey.GetValue("LastVersion") != null)
                    {
                        dmKey.DeleteValue("LastVersion", false);
                        logger.Info($"Program/CleanupLegacyUserRegistryValues: Removed legacy HKCU Software\\DisplayMagician LastVersion value.");
                    }

                    if (dmKey.GetValue("FirstRun") != null)
                    {
                        dmKey.DeleteValue("FirstRun", false);
                        logger.Info($"Program/CleanupLegacyUserRegistryValues: Removed legacy HKCU Software\\DisplayMagician FirstRun value.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/CleanupLegacyUserRegistryValues: Could not remove legacy HKCU DisplayMagician registry values.");
            }
        }

        private static async Task<bool> RegisterPackageWithExternalLocationAsync(string externalLocation, string packagePath)
        {
            bool registration = false;
            try
            {
                if (!Directory.Exists(externalLocation))
                {
                    logger.Warn($"Program/RegisterPackageWithExternalLocationAsync: External package location {externalLocation} does not exist.");
                    return false;
                }

                if (!File.Exists(packagePath))
                {
                    logger.Warn($"Program/RegisterPackageWithExternalLocationAsync: Package file {packagePath} does not exist.");
                    return false;
                }

                var externalUri = new Uri(externalLocation);
                var packageUri = new Uri(packagePath);

                logger.Info($"Program/RegisterPackageWithExternalLocationAsync: Registering package {packageUri} with external location {externalUri}.");

                var packageManager = new PackageManager();

                //Declare use of an external location
                var options = new AddPackageOptions();
                options.ExternalLocationUri = externalUri;

                var deploymentOperation = packageManager.AddPackageByUriAsync(packageUri, options);

                await deploymentOperation;

                if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Error)
                {
                    Windows.Management.Deployment.DeploymentResult deploymentResult = deploymentOperation.GetResults();
                    logger.Warn($"Program/RegisterPackageWithExternalLocationAsync: Package registration failed. ErrorCode={deploymentOperation.ErrorCode}; ExtendedErrorCode={deploymentResult.ExtendedErrorCode}; ErrorText={deploymentResult.ErrorText}");

                }
                else if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Canceled)
                {
                    logger.Warn($"Program/RegisterPackageWithExternalLocationAsync: Package registration was cancelled.");
                }
                else if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Completed)
                {
                    registration = true;
                    logger.Info($"Program/RegisterPackageWithExternalLocationAsync: Package registration succeeded.");
                }
                else
                {
                    logger.Warn($"Program/RegisterPackageWithExternalLocationAsync: Package registration ended with unknown status {deploymentOperation.Status}.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/RegisterPackageWithExternalLocationAsync: Package registration failed.");

                return registration;
            }

            return registration;
        }

        public static void CheckForUpdates(bool automatic = true, string requestedMessageUpdateVersion = null, string requestedMessageUpdateChannel = null)
        {
            _lastUpdateCheckWasAutomatic = automatic;
            _requestedMessageUpdateVersion = requestedMessageUpdateVersion;
            _requestedMessageUpdateChannel = requestedMessageUpdateChannel;
            string updateChannel = Program.AppProgramSettings.UpgradeToPreReleases ? "prerelease" : "stable";
            logger.Info($"Program/CheckForUpdates: Starting {(automatic ? "automatic" : "manual")} update check. Installed version is {AppVersion}; selected update channel is {updateChannel}.");

            // Firstly check if the user wants to upgrade at all
            // If not, just return
            if (!Program.AppProgramSettings.UpgradeEnabled)
            {
                _requestedMessageUpdateVersion = null;
                _requestedMessageUpdateChannel = null;
                logger.Warn($"Program/CheckForUpdates: User has set the Program Settings to ignore any DisplayMagician updates. Skipping the auto update.");
                return;
            }

            // Second of all, check to see if there is any way to get to the internet on this computer.
            // If not, then why bother!
            try
            {              

                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    _requestedMessageUpdateVersion = null;
                    _requestedMessageUpdateChannel = null;
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
            RegisterAutoUpdaterEvents();
            AutoUpdater.RunUpdateAsAdmin = true;
            AutoUpdater.HttpUserAgent = "DisplayMagician AutoUpdater";
            AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Days;
            AutoUpdater.RemindLaterAt = 7;
            AutoUpdater.InstalledVersion = new Version(AppVersion);

            string connectionUrl = Program.UpdateUrl;
            connectionUrl += ($"?version={HttpUtility.UrlEncode(Program.AppVersion)}");
            connectionUrl += ($"&install_id={HttpUtility.UrlEncode(Program.AppProgramSettings.InstallId)}");
            connectionUrl += ($"&id={HttpUtility.UrlEncode(Program.AppProgramSettings.InstallId)}");
            logger.Info($"Program/CheckForUpdates: Checking the {updateChannel} channel for an update to installed version {AutoUpdater.InstalledVersion}.");
            AutoUpdater.Start(connectionUrl);
        }

        private static void RegisterAutoUpdaterEvents()
        {
            if (_autoUpdaterEventsRegistered)
                return;

            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
            _autoUpdaterEventsRegistered = true;
        }

        private static void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
            logger.Trace($"Program/AutoUpdaterOnParseUpdateInfoEvent: Received the following Update JSON file from {AutoUpdater.AppCastURL}: {args.RemoteData}");
            try
            {
                bool usePrerelease = !string.IsNullOrWhiteSpace(_requestedMessageUpdateChannel)
                    ? string.Equals(_requestedMessageUpdateChannel, "prerelease", StringComparison.OrdinalIgnoreCase)
                    : Program.AppProgramSettings.UpgradeToPreReleases;
                if (usePrerelease)
                {
                    logger.Info($"Program/AutoUpdaterOnParseUpdateInfoEvent: Update feed contains stable version {json["stable"]["version"]} and prerelease version {json["prerelease"]["version"]}. Pre-release upgrades are enabled, so the prerelease version will be evaluated.");
                    logger.Trace($"MainForm/AutoUpdaterOnParseUpdateInfoEvent: Trying to create an UpdateInfoEventArgs object from the Prerelease info in the received Update JSON file.");
                    args.UpdateInfo = new UpdateInfoEventArgs
                    {
                        CurrentVersion = (string)json["prerelease"]["version"],
                        ChangelogURL = (string)json["prerelease"]["changelog"],
                        DownloadURL = (string)json["prerelease"]["url"],
                        Mandatory = new Mandatory
                        {
                            Value = (bool)json["prerelease"]["mandatory"]["value"],
                            UpdateMode = (Mode)(int)json["prerelease"]["mandatory"]["mode"],
                            MinimumVersion = (string)json["prerelease"]["mandatory"]["minVersion"]
                        },
                        CheckSum = new CheckSum
                        {
                            Value = (string)json["prerelease"]["checksum"]["value"],
                            HashingAlgorithm = (string)json["prerelease"]["checksum"]["hashingAlgorithm"]
                        }
                    };
                    logger.Trace($"MainForm/AutoUpdaterOnParseUpdateInfoEvent: Trying to create an UpgradeExtraDetails object from the Prerelease extraDetails in the received Update JSON file.");
                    AppUpgradeExtraDetails = new UpgradeExtraDetails
                    {
                        //PreleaseUpgrade = true,
                        ManualUpgrade = (bool)json["prerelease"]["manualUpgrade"],
                        UpdatesDisplayProfiles = (bool)json["prerelease"]["updatesDisplayProfiles"],
                        UpdatesGameShortcuts = (bool)json["prerelease"]["updatesGameShortcuts"],
                        UpdatesSettings = (bool)json["prerelease"]["updatesSettings"],
                    };

                }
                else
                {
                    logger.Info($"Program/AutoUpdaterOnParseUpdateInfoEvent: Update feed contains stable version {json["stable"]["version"]} and prerelease version {json["prerelease"]["version"]}. Pre-release upgrades are disabled, so the prerelease version will be skipped and the stable version evaluated.");
                    logger.Trace($"MainForm/AutoUpdaterOnParseUpdateInfoEvent: Trying to create an UpdateInfoEventArgs object from the Stable info in the received Update JSON file.");
                    args.UpdateInfo = new UpdateInfoEventArgs
                    {
                        CurrentVersion = (string)json["stable"]["version"],
                        ChangelogURL = (string)json["stable"]["changelog"],
                        DownloadURL = (string)json["stable"]["url"],
                        Mandatory = new Mandatory
                        {
                            Value = (bool)json["stable"]["mandatory"]["value"],
                            UpdateMode = (Mode)(int)json["stable"]["mandatory"]["mode"],
                            MinimumVersion = (string)json["stable"]["mandatory"]["minVersion"]
                        },
                        CheckSum = new CheckSum
                        {
                            Value = (string)json["stable"]["checksum"]["value"],
                            HashingAlgorithm = (string)json["stable"]["checksum"]["hashingAlgorithm"]
                        }
                    };
                    logger.Trace($"MainForm/AutoUpdaterOnParseUpdateInfoEvent: Trying to create an UpgradeExtraDetails object from the Stable extraDetails in the received Update JSON file.");
                    AppUpgradeExtraDetails = new UpgradeExtraDetails
                    {
                        //PreleaseUpgrade = false,
                        ManualUpgrade = (bool)json["stable"]["manualUpgrade"],
                        UpdatesDisplayProfiles = (bool)json["stable"]["updatesDisplayProfiles"],
                        UpdatesGameShortcuts = (bool)json["stable"]["updatesGameShortcuts"],
                        UpdatesSettings = (bool)json["stable"]["updatesSettings"],
                    };
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Program/AutoUpdaterOnParseUpdateInfoEvent: Exception trying to create an UpdateInfoEventArgs object from the received Update JSON file.");
            }
        }

        private static void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            // AutoUpdater.Net raises this event on a ThreadPool thread. The update dialog and
            // its MainForm owner must be created on the WinForms UI thread.
            if (AppMainForm != null && AppMainForm.IsHandleCreated && AppMainForm.InvokeRequired)
            {
                AppMainForm.BeginInvoke((System.Windows.Forms.MethodInvoker)(() => AutoUpdaterOnCheckForUpdateEvent(args)));
                return;
            }

            string requestedMessageUpdateVersion = _requestedMessageUpdateVersion;
            string requestedMessageUpdateChannel = _requestedMessageUpdateChannel;
            _requestedMessageUpdateVersion = null;
            _requestedMessageUpdateChannel = null;

            if (args.Error == null)
            {
                // TODO: FIX THIS BEFORE RELEASE AS THIS IS A TESTING HACK TO FORCE AN UPDATE TO BE AVAILABLE FOR TESTING PURPOSES. REMOVE THIS BEFORE RELEASE.
                //if (args.IsUpdateAvailable)
                if (true)
                {
                    // Shut down the splash screen
                    if (Program.AppProgramSettings.ShowSplashScreen && Program.AppSplashScreen != null && !Program.AppSplashScreen.Disposing && !Program.AppSplashScreen.IsDisposed)
                        Program.AppSplashScreen.Invoke(new Action(() => Program.AppSplashScreen.Close()));

                    logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - There is an upgrade to version {args.CurrentVersion} available from {args.DownloadURL}. We're using version {args.InstalledVersion} at the moment.");

                    string selectedUpdateChannel = !string.IsNullOrWhiteSpace(requestedMessageUpdateChannel)
                        ? requestedMessageUpdateChannel
                        : AppProgramSettings.UpgradeToPreReleases ? "prerelease" : "stable";
                    bool shouldInstallRequestedMessageUpdate = !string.IsNullOrWhiteSpace(requestedMessageUpdateVersion)
                        && string.Equals(requestedMessageUpdateChannel, selectedUpdateChannel, StringComparison.OrdinalIgnoreCase)
                        && Version.TryParse(requestedMessageUpdateVersion, out Version requestedVersion)
                        && Version.TryParse(args.CurrentVersion, out Version availableVersion)
                        && availableVersion >= requestedVersion;
                    if (shouldInstallRequestedMessageUpdate)
                    {
                        try
                        {
                            logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - User requested installation from message version {requestedMessageUpdateVersion}; downloading available version {args.CurrentVersion}.");
                            if (AutoUpdater.DownloadUpdate(args))
                            {
                                logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - Download completed. Restarting to apply update version {args.CurrentVersion}.");
                                Application.Exit();
                            }
                            else
                            {
                                logger.Warn($"Program/AutoUpdaterOnCheckForUpdateEvent - Update download for requested message version {requestedMessageUpdateVersion} did not complete.");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex, $"Program/AutoUpdaterOnCheckForUpdateEvent - Exception downloading requested message update version {requestedMessageUpdateVersion}.");
                            MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(requestedMessageUpdateVersion))
                    {
                        logger.Warn($"Program/AutoUpdaterOnCheckForUpdateEvent - The update available from the selected channel does not match requested message version {requestedMessageUpdateVersion}; not downloading it.");
                        MessageBox.Show("The update referenced by this message is no longer available from the selected update channel.", "Update unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DialogResult dialogResult;
                    UpgradeForm upgradeForm = new UpgradeForm();
                    upgradeForm.ChangelogURL = args.ChangelogURL;
                    upgradeForm.ReleaseHeading = $"DisplayMagician update {args.CurrentVersion} is available";

                    string updateChannel = AppProgramSettings.UpgradeToPreReleases ? "prerelease" : "stable";
                    LocalMessage releaseAnnouncement = GetStoredMessages().FirstOrDefault(m =>
                        string.Equals(m.Kind, "releaseAnnouncement", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(m.ReleaseVersion, args.CurrentVersion, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(m.ReleaseChannel, updateChannel, StringComparison.OrdinalIgnoreCase));

                    if (releaseAnnouncement != null)
                    {
                        SetMessageReadState(new[] { releaseAnnouncement.Id }, true);
                        RefreshMessageIndicators();

                        string releaseNotesPath = Path.Combine(AppMessagesPath, releaseAnnouncement.MarkdownFileName ?? string.Empty);
                        try
                        {
                            if (File.Exists(releaseNotesPath))
                            {
                                upgradeForm.ReleaseNotesHtml = File.ReadAllText(releaseNotesPath);
                                upgradeForm.ReleaseNotesFormat = releaseAnnouncement.Format;
                            }
                            else
                            {
                                logger.Warn($"Program/AutoUpdaterOnCheckForUpdateEvent: Release announcement content is missing for version {args.CurrentVersion} (messageId={releaseAnnouncement.Id}, fullPath={releaseNotesPath}). Showing the upgrade-form fallback text instead.");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex, $"Program/AutoUpdaterOnCheckForUpdateEvent: Failed to load release announcement content for version {args.CurrentVersion} (messageId={releaseAnnouncement.Id}). Showing the upgrade-form fallback text instead.");
                        }
                    }
                    else
                    {
                        logger.Warn($"Program/AutoUpdaterOnCheckForUpdateEvent: No synchronized release announcement matched the available {updateChannel} update version {args.CurrentVersion}. Showing the upgrade-form fallback text instead.");
                    }

                    dialogResult = upgradeForm.ShowDialog(AppMainForm);

                    if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                    {
                        try
                        {
                            logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - User accepted update from version {args.InstalledVersion} to {args.CurrentVersion}; downloading the update.");
                            if (AutoUpdater.DownloadUpdate(args))
                            {
                                logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - Download completed. Restarting to apply update from version {args.InstalledVersion} to {args.CurrentVersion}.");
                                Application.Exit();
                            }
                            else
                            {
                                logger.Warn($"Program/AutoUpdaterOnCheckForUpdateEvent - Update download for version {args.CurrentVersion} did not complete, so DisplayMagician will remain on version {args.InstalledVersion}.");
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
                        logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - User deferred update from version {args.InstalledVersion} to {args.CurrentVersion}; DisplayMagician remains on version {args.InstalledVersion} until the next reminder.");
                    }
                    else
                    {
                        logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent - User skipped update from version {args.InstalledVersion} to {args.CurrentVersion}; DisplayMagician remains on version {args.InstalledVersion}.");
                    }
                }
                else
                {
                    string updateChannel = AppProgramSettings.UpgradeToPreReleases ? "prerelease" : "stable";
                    logger.Info($"Program/AutoUpdaterOnCheckForUpdateEvent: Update check completed. No {updateChannel} update is required; installed version {args.InstalledVersion} is current relative to available version {args.CurrentVersion}.");
                    if (!string.IsNullOrWhiteSpace(requestedMessageUpdateVersion))
                    {
                        MessageBox.Show("This update is no longer available from the selected update channel.", "Update unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    if (_lastUpdateCheckWasAutomatic)
                    {
                        return;
                    }

                    MessageBox.Show(
                        @"There is a problem reaching update server. Please check your internet connection and try again later.",
                        @"Update Check Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    logger.Warn(args.Error, $"Program/AutoUpdaterOnCheckForUpdateEvent - There was a problem performing the update: {args.Error.Message}");
                    if (_lastUpdateCheckWasAutomatic)
                    {
                        return;
                    }

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

                        case "readMessagesNow":
                            HandleReadMessagesNowAction();
                            break;

                        case "readMessagesLater":
                            break;

                        default:
                            break;
                    }
                }

            };

            try
            {
                if (Program.AppNotInstalled)
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

        public static bool DMIsNotInstalled()

        {
            string installKey = @"SOFTWARE\DisplayMagician";
            string thisInstallDir = Path.GetDirectoryName(Application.ExecutablePath) + "\\";

            try
            {
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(installKey))
                {
                    if (rk == null) 
                    {
                        return true;
                    }
                    if (rk.GetValue("InstallDir") != null && rk.GetValue("InstallDir").ToString() == thisInstallDir)
                    {
                        return false; //exists
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Program/IsInstalledVersion: DisplayMagician InstallDir isn't in registry! This DisplayMagician isn't installed.");
                return true;
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
