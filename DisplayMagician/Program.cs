using System;
using McMaster.Extensions.CommandLineUtils;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Forms;
using DisplayMagician.Contracts;
using DisplayMagician.UIForms;
using System.Text.RegularExpressions;
using System.Drawing;
using NLog.Config;
using System.Collections.Generic;
using System.Collections.Concurrent;
using AutoUpdaterDotNET;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.ComponentModel;
using System.Text;
using System.Globalization;
using System.Web;
using Vortice.DirectInput;
using System.Diagnostics;

using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.Security.Authorization.AppCapabilityAccess;


namespace DisplayMagician {

    public static class Program
    {
        internal static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        public static string AppStartupPath = Application.StartupPath;
        public static string AppIconPath = Path.Combine(Program.AppDataPath, $"Icons");
        public static string AppWallpaperPath = Path.Combine(Program.AppDataPath, $"Wallpaper");
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
        // Keep the desktop taskbar identity in sync with the MSI Start menu shortcut.
        public const string AppTaskbarUserModelId = "LittleBitBig.DisplayMagician.Desktop";
        public const string AppActivationId = "4F319902-EB8C-43E6-8A51-8EA74E4308F8";        
        public static bool AppToastActivated = false;
        public static bool AppNotInstalled = false;
        //public static bool AppInstalled = false;
        //public static bool AppNewInstall = false;
        //public static bool AppVersionUpgrade = false;
        public static bool AppHasPackageIdentity = false;
        //public static string AppLastVersionRun = "0.0";
        private static readonly object _activeOperationCancellationLock = new object();
        private static CancellationTokenSource _activeOperationCancellationSource;
        //Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        public static SemaphoreSlim AppBackgroundTaskSemaphoreSlim = new SemaphoreSlim(1, 1);

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
        private static bool _packageIdentityWarningNeeded = false;
        private static bool _autoUpdaterEventsRegistered = false;
        private static bool _lastUpdateCheckWasAutomatic = true;
        private static string _requestedMessageUpdateVersion;
        private static string _requestedMessageUpdateChannel;
        private static bool _startupBackgroundTasksQueued = false;
        private static bool _isElevatedRecoveryAction;
        private static SynchronizationContext _mainSynchronizationContext;
        private static readonly Stopwatch _interactiveRuntimeStopwatch = Stopwatch.StartNew();
        private static readonly CancellationTokenSource _clientEventListenerCancellationSource = new CancellationTokenSource();
        private static readonly ConcurrentDictionary<Guid, long> _lastOperationStatusSequences = new ConcurrentDictionary<Guid, long>();
        private static readonly ConcurrentDictionary<Guid, byte> _displayedOperationDecisionPrompts = new ConcurrentDictionary<Guid, byte>();
        private static readonly ConcurrentDictionary<Guid, OperationDecisionForm> _operationDecisionForms = new ConcurrentDictionary<Guid, OperationDecisionForm>();
        internal const string TestUpdateFeedCommandLineOption = "--test-update-feed";
        private const string PackageIdentityRestartCommandLineOption = "--package-identity-restart";
        internal const string ForceReleaseDisplayControlCommandLineOption = "--force-release-display-control";
        internal const string RestartControlServiceCommandLineOption = "--restart-control-service";
        internal const string ServerSettingsCommandLineOption = "--server-settings";
        internal const string RestartGatewayCommandLineOption = "--restart-gateway";
        private const string ControlServiceName = "DisplayMagicianControlService";

        private static volatile bool _useTestUpdateFeed;
        private static Process _userAgentProcess;

        public static bool CancelActiveOperation()
        {
            lock (_activeOperationCancellationLock)
            {
                if (_activeOperationCancellationSource == null)
                    return false;

                _activeOperationCancellationSource.Cancel();
                return true;
            }
        }

        private static void ConfigureDesktopSettingsAndLogPath(string userDataPath)
        {
            string rootPath = Path.GetFullPath(userDataPath);
            ProgramSettings.ConfigureStoragePath(Path.Combine(rootPath, "Settings"));
            DonationSettings.ConfigureStoragePath(Path.Combine(rootPath, "Settings"));
            AppLogPath = Path.Combine(rootPath, "Logs");
        }

        private static void ConfigureLogPath(string legacyLogPath)
        {
            string preferredLogPath = AppLogPath;
            try
            {
                Directory.CreateDirectory(preferredLogPath);
                string probePath = Path.Combine(preferredLogPath, $".write-probe-{Guid.NewGuid():N}.tmp");
                using (FileStream probe = new FileStream(probePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose))
                {
                    probe.WriteByte(0);
                }

                if (!string.Equals(preferredLogPath, legacyLogPath, StringComparison.OrdinalIgnoreCase) && Directory.Exists(legacyLogPath))
                {
                    foreach (string legacyLogFile in Directory.EnumerateFiles(legacyLogPath, "*.log", SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            string destinationPath = Path.Combine(preferredLogPath, Path.GetFileName(legacyLogFile));
                            if (File.Exists(destinationPath))
                            {
                                destinationPath = Path.Combine(preferredLogPath, $"{Path.GetFileNameWithoutExtension(legacyLogFile)}-{Guid.NewGuid():N}{Path.GetExtension(legacyLogFile)}");
                            }

                            File.Move(legacyLogFile, destinationPath);
                        }
                        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                        {
                            Console.WriteLine($"Program/ConfigureLogPath: Could not move legacy log {legacyLogFile} to {preferredLogPath}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException)
            {
                AppLogPath = legacyLogPath;
                try
                {
                    Directory.CreateDirectory(AppLogPath);
                }
                catch (Exception fallbackException) when (fallbackException is IOException || fallbackException is UnauthorizedAccessException || fallbackException is NotSupportedException)
                {
                    Console.WriteLine($"Program/ConfigureLogPath: Cannot create a log directory at {preferredLogPath} or fallback path {AppLogPath}: {fallbackException.Message}");
                    return;
                }

                Console.WriteLine($"Program/ConfigureLogPath: Using legacy log path {AppLogPath} because {preferredLogPath} is not writable: {ex.Message}");
            }
        }

        private static CancellationTokenSource BeginActiveOperationCancellation()
        {
            lock (_activeOperationCancellationLock)
            {
                if (_activeOperationCancellationSource != null)
                    throw new InvalidOperationException("A cancellable operation is already active.");

                _activeOperationCancellationSource = new CancellationTokenSource();
                return _activeOperationCancellationSource;
            }
        }

        private static void CompleteActiveOperationCancellation(CancellationTokenSource cancellationSource)
        {
            lock (_activeOperationCancellationLock)
            {
                if (!ReferenceEquals(_activeOperationCancellationSource, cancellationSource))
                    return;

                _activeOperationCancellationSource = null;
                cancellationSource.Dispose();
            }
        }

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
            ERROR_RECOVERY_HISTORY_NOT_RECORDED = 106, // The recovery action completed, but its administration history could not be recorded.
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
            Application.ApplicationExit += (sender, eventArgs) =>
            {
                if (_isElevatedRecoveryAction)
                    return;

                _clientEventListenerCancellationSource.Cancel();
                StopUserAgentIfIdle();
            };

            string legacyLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician", "Logs");
            if (V4UserDataPathResolver.TryGetMigratedUserDataPath(out string migratedUserDataPath))
            {
                ConfigureDesktopSettingsAndLogPath(migratedUserDataPath);
            }

            ConfigureLogPath(legacyLogPath);


            
            // Prepare NLog for internal logging - Comment out when not required
            //NLog.Common.InternalLogger.LogLevel = NLog.LogLevel.Debug;
            //NLog.Common.InternalLogger.LogToConsole = true;
            //NLog.Common.InternalLogger.LogFile = "C:\\Users\\terry\\AppData\\Local\\DisplayMagician\\Logs\\nlog-internal.txt";

            SupportLogLayout.Register();
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
                Layout = "${displaymagicianlog:component=DesktopApp}"
            };

            // Create a logging rule to use the log file target
            var loggingRule = new LoggingRule("LogToFile");
            loggingRule.EnableLoggingForLevels(logLevel, NLog.LogLevel.Fatal);
            loggingRule.Targets.Add(logfile);
            loggingRule.LoggerNamePattern = "*";
            config.LoggingRules.Add(loggingRule);

            // Apply config           
            NLog.LogManager.Configuration = config;

            // Keep the legacy desktop helpers on the same log file by sending them the
            // details of the existing NLog logger
            sharedLogger = new SharedLogger(logger);

            // Start the Log file
            logger.Info($"Program/Main: Starting {Application.ProductName} v{Application.ProductVersion}");

            if (args.Any(argument => string.Equals(argument, ForceReleaseDisplayControlCommandLineOption, StringComparison.OrdinalIgnoreCase)))
            {
                _isElevatedRecoveryAction = true;
                return ForceReleaseDisplayControlFromElevatedProcess();
            }

            if (args.Any(argument => string.Equals(argument, RestartControlServiceCommandLineOption, StringComparison.OrdinalIgnoreCase)))
            {
                _isElevatedRecoveryAction = true;
                return RestartControlServiceFromElevatedProcess();
            }

            if (args.Any(argument => string.Equals(argument, ServerSettingsCommandLineOption, StringComparison.OrdinalIgnoreCase)))
            {
                _isElevatedRecoveryAction = true;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                using ServerSettingsForm serverSettingsForm = new ServerSettingsForm();
                serverSettingsForm.ShowDialog();
                return (int)ERRORLEVEL.OK;
            }

            if (args.Any(argument => string.Equals(argument, RestartGatewayCommandLineOption, StringComparison.OrdinalIgnoreCase)))
            {
                _isElevatedRecoveryAction = true;
                return RunServiceControlCommand("stop", out _, "DisplayMagicianGateway") == 0 && RunServiceControlCommand("start", out _, "DisplayMagicianGateway") == 0 ? (int)ERRORLEVEL.OK : (int)ERRORLEVEL.ERROR_EXCEPTION;
            }

            // Check for the --test-update-feed to check for the test update feed instead of the normal update feed. This is useful for testing the update feed without having to change the code.
            if (args.Any(argument => string.Equals(argument, TestUpdateFeedCommandLineOption, StringComparison.OrdinalIgnoreCase)))
            {
                EnableTestUpdateFeed("the startup command line", checkForUpdatesNow: false);
            }


            // PACKAGE IDENTITY INITIALIZATION AND CHECKS
            if (EnsurePackageIdentity(args))
                return (int)ERRORLEVEL.OK;

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

            logger.Trace($"Program/Main: Setting visual styles and rendering mode");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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

            bool settingsChanged = AppProgramSettings.EnsureInstallIdentity(false);
            if (settingsChanged)
            {
                AppProgramSettings.SaveSettings();
            }
            ReportAnonymousMetricsUsage(isLaunch: true, activeMinutes: 0);

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
            if (!ConnectDesktopStateToUserAgent())
            {
                MessageBox.Show("DisplayMagician could not connect to the User Agent that manages your profiles. Please restart DisplayMagician and try again.", "DisplayMagician User Agent", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (int)ERRORLEVEL.ERROR_EXCEPTION;
            }

            if (AppProgramSettings.InstallDesktopContextMenu)
            {
                logger.Trace($"Program/Main: Installing the context menu on startup");
                ContextMenu.InstallContextMenu();
            }

            // Next we create the MainForm object but keep it hidden for now
            logger.Trace($"Program/Main: Creating the MainForm object");
            AppMainForm = new MainForm();

            ShowMigrationSummary(migrationResult.Notices);

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

                

                // Keep the splash screen in the foreground until the normal main window is visible,
                // then explicitly transfer focus to it. A minimized startup has no main window to show.
                if (!AppProgramSettings.MinimiseOnStart)
                    AppMainForm.Shown += MainForm_ShownAndOpenApp;

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
                string[] commandLineArguments = args
                    .Where(argument => !string.Equals(argument, TestUpdateFeedCommandLineOption, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                errorLevelToReturnToOS = app.Execute(commandLineArguments);
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

            ReportAnonymousMetricsUsage(isLaunch: false, activeMinutes: Math.Max(0, (long)_interactiveRuntimeStopwatch.Elapsed.TotalMinutes));

            logger.Trace($"Program/Main: Disposing the DirectInput manager.");
            AppDirectInputManager?.Dispose();
            AppDirectInputManager = null;

            // Shutdown NLog
            logger.Trace($"Program/Main: Stopping logging processes");
            NLog.LogManager.Shutdown();

            logger.Trace($"Program/Main: Disposing the CancellationToken");
            // Dispose of the CancellationTokenSource
            Program.CancelActiveOperation();

            // Exit with a 0 Errorlevel to indicate everything worked fine!
            logger.Trace($"Program/Main: Returning the following errorlevel to the OS: {errorLevelToReturnToOS} ({((ERRORLEVEL)errorLevelToReturnToOS).ToString()})");
            return errorLevelToReturnToOS;
        }       

        private static int ForceReleaseDisplayControlFromElevatedProcess()
        {
            try
            {
                logger.Info("Program/ForceReleaseDisplayControlFromElevatedProcess: Processing the elevated emergency display-control release request.");
                ControlResponse response = new ControlServicePipeClient()
                    .ForceReleaseDisplayControlAsync(CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                if (response.IsSuccessful)
                {
                    logger.Info("Program/ForceReleaseDisplayControlFromElevatedProcess: {0}", response.Message);
                    return (int)ERRORLEVEL.OK;
                }

                logger.Error("Program/ForceReleaseDisplayControlFromElevatedProcess: The Control Service rejected the emergency release. ErrorCode={0}; Message={1}", response.ErrorCode, response.Message);
                return (int)ERRORLEVEL.ERROR_EXCEPTION;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Program/ForceReleaseDisplayControlFromElevatedProcess: The emergency display-control release request failed.");
                return (int)ERRORLEVEL.ERROR_EXCEPTION;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        private static int RestartControlServiceFromElevatedProcess()
        {
            try
            {
                try
                {
                    ControlServiceStatus status = new ControlServicePipeClient().GetServiceStatusAsync(CancellationToken.None).GetAwaiter().GetResult();
                    if (status.DisplayControlLease?.ActiveOperationId != null || status.DisplayControlLease?.IsRecoveryRequired == true)
                    {
                        logger.Error("Program/RestartControlServiceFromElevatedProcess: Control Service restart was refused because display control is active or recovery is required.");
                        return (int)ERRORLEVEL.ERROR_EXCEPTION;
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is TimeoutException || ex is InvalidOperationException)
                {
                    logger.Warn(ex, "Program/RestartControlServiceFromElevatedProcess: Could not obtain Control Service status before restarting it; continuing with the administrator-requested restart.");
                }

                int stopExitCode = RunServiceControlCommand("stop", out string stopOutput);
                if (stopExitCode != 0 && stopExitCode != 1062)
                {
                    logger.Error("Program/RestartControlServiceFromElevatedProcess: Control Service stop command failed. ExitCode={0}; Output={1}", stopExitCode, stopOutput);
                    return (int)ERRORLEVEL.ERROR_EXCEPTION;
                }

                if (!WaitForControlServiceState("STOPPED", TimeSpan.FromSeconds(30)))
                {
                    logger.Error("Program/RestartControlServiceFromElevatedProcess: Control Service did not stop within the expected time.");
                    return (int)ERRORLEVEL.ERROR_EXCEPTION;
                }

                int startExitCode = RunServiceControlCommand("start", out string startOutput);
                if (startExitCode != 0)
                {
                    logger.Error("Program/RestartControlServiceFromElevatedProcess: Control Service start command failed. ExitCode={0}; Output={1}", startExitCode, startOutput);
                    return (int)ERRORLEVEL.ERROR_EXCEPTION;
                }

                if (!WaitForControlServiceState("RUNNING", TimeSpan.FromSeconds(30)))
                {
                    logger.Error("Program/RestartControlServiceFromElevatedProcess: Control Service did not start within the expected time.");
                    return (int)ERRORLEVEL.ERROR_EXCEPTION;
                }

                try
                {
                    ControlResponse recoveryRecordResponse = new ControlServicePipeClient().RecordRecoveryAdministrationAsync("RestartControlService", "Succeeded", CancellationToken.None).GetAwaiter().GetResult();
                    if (!recoveryRecordResponse.IsSuccessful)
                    {
                        logger.Error("Program/RestartControlServiceFromElevatedProcess: Control Service restarted but rejected its recovery history record. ErrorCode={0}; Message={1}", recoveryRecordResponse.ErrorCode, recoveryRecordResponse.Message);
                        return (int)ERRORLEVEL.ERROR_RECOVERY_HISTORY_NOT_RECORDED;
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is InvalidOperationException)
                {
                    logger.Warn(ex, "Program/RestartControlServiceFromElevatedProcess: Control Service restarted but its recovery history could not be recorded.");
                }

                logger.Info("Program/RestartControlServiceFromElevatedProcess: Control Service restarted successfully.");
                return (int)ERRORLEVEL.OK;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Program/RestartControlServiceFromElevatedProcess: Control Service restart failed.");
                return (int)ERRORLEVEL.ERROR_EXCEPTION;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        private static int RunServiceControlCommand(string action, out string output, string serviceName = null)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "sc.exe"), $"{action} \"{serviceName ?? ControlServiceName}\"")
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("Windows could not start the Service Control command.");
            output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            if (!process.WaitForExit(10000))
            {
                throw new TimeoutException("The Service Control command did not finish in time.");
            }

            return process.ExitCode;
        }

        private static bool WaitForControlServiceState(string expectedState, TimeSpan timeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < timeout)
            {
                int expectedStateCode = string.Equals(expectedState, "STOPPED", StringComparison.Ordinal) ? 1 : 4;
                int queryExitCode = RunServiceControlCommand("query", out string queryOutput);
                if (queryExitCode == 0 && queryOutput.IndexOf($"STATE              : {expectedStateCode}", StringComparison.Ordinal) >= 0)
                {
                    return true;
                }

                Thread.Sleep(500);
            }

            return false;
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

                // A normal startup closes the splash screen from MainForm_ShownAndOpenApp,
                // once the main window can receive focus. A minimized startup has no shown form.
                if (AppProgramSettings.MinimiseOnStart && AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
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

        private static void MainForm_ShownAndOpenApp(object sender, EventArgs e)
        {
            logger.Trace("Program/MainForm_ShownAndOpenApp: Closing the splash screen and activating the main window.");

            if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));

            if (AppMainForm == null || AppMainForm.IsDisposed || !AppMainForm.Visible)
                return;

            bool wasTopMost = AppMainForm.TopMost;
            AppMainForm.TopMost = true;
            AppMainForm.BringToFront();
            AppMainForm.Activate();
            AppMainForm.TopMost = wasTopMost;
        }

        // ReSharper disable once CyclomaticComplexity
        public static ERRORLEVEL RunShortcut(string shortcutUUID)
        {
            logger.Debug($"Program/RunShortcut: Running shortcut {shortcutUUID}");

            // Close the splash screen
            if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));

            if (string.IsNullOrWhiteSpace(shortcutUUID))
            {
                logger.Error("Program/RunShortcut: A shortcut ID is required.");
                return ERRORLEVEL.ERROR_CANNOT_FIND_SHORTCUT;
            }

            shortcutUUID = shortcutUUID.Trim('"');
            return RunShortcutThroughUserAgent(shortcutUUID);
        }

        private static ERRORLEVEL RunShortcutThroughUserAgent(string shortcutUUID)
        {
            try
            {
                ControlServicePipeClient controlServiceClient = new ControlServicePipeClient();
                DisplayMagician.Contracts.ControlResponse response = controlServiceClient.StartShortcutWhenAgentAvailableAsync(shortcutUUID, CancellationToken.None).GetAwaiter().GetResult();
                if (response.IsSuccessful)
                {
                    return ERRORLEVEL.OK;
                }

                logger.Error("Program/RunShortcutThroughUserAgent: The User Agent rejected shortcut {0}. ErrorCode={1}; Message={2}", shortcutUUID, response.ErrorCode, response.Message);
                return response.OperationStatus?.Phase == DisplayMagician.Contracts.OperationPhase.Cancelled
                    ? ERRORLEVEL.CANCELED_BY_USER
                    : ERRORLEVEL.ERROR_EXCEPTION;
            }
            catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is InvalidOperationException)
            {
                logger.Error(ex, "Program/RunShortcutThroughUserAgent: The Control Service path is unavailable for shortcut {0}.", shortcutUUID);
                return ERRORLEVEL.ERROR_EXCEPTION;
            }
        }

        public static ERRORLEVEL RunProfile(string profileId)
        {
            logger.Trace($"Program/RunProfile: Running profile {profileId}");

            // Close the splash screen
            if (AppProgramSettings.ShowSplashScreen && AppSplashScreen != null && !AppSplashScreen.Disposing && !AppSplashScreen.IsDisposed)
                AppSplashScreen.Invoke(new Action(() => AppSplashScreen.Close()));

            if (string.IsNullOrWhiteSpace(profileId))
            {
                logger.Error("Program/RunProfile: A display profile ID is required.");
                return ERRORLEVEL.ERROR_CANNOT_FIND_PROFILE;
            }

            profileId = profileId.Trim('"');
            try
            {
                if (!EnsureUserAgentStarted())
                {
                    return ERRORLEVEL.ERROR_APPLYING_PROFILE;
                }

                ControlServicePipeClient controlServiceClient = new ControlServicePipeClient();
                DisplayMagician.Contracts.ControlResponse response = controlServiceClient.ApplyProfileWhenAgentAvailableAsync(profileId, CancellationToken.None).GetAwaiter().GetResult();
                if (response.IsSuccessful)
                {
                    return ERRORLEVEL.OK;
                }

                logger.Error("Program/RunProfile: The Control Service did not apply profile {0}. ErrorCode={1}; Message={2}", profileId, response.ErrorCode, response.Message);
                return response.ApplyProfile?.WasCancelled == true
                    ? ERRORLEVEL.CANCELED_BY_USER
                    : response.ErrorCode is DisplayMagician.Contracts.ControlErrorCode.InvalidRequest or DisplayMagician.Contracts.ControlErrorCode.ProfileNotFound
                        ? ERRORLEVEL.ERROR_CANNOT_FIND_PROFILE
                        : ERRORLEVEL.ERROR_APPLYING_PROFILE;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Program/RunProfile: Could not invoke the User Agent for profile {0}.", profileId);
                return ERRORLEVEL.ERROR_APPLYING_PROFILE;
            }
        }


        public static bool IsValidFilename(string testName)
        {
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(testName)) { return false; };

            return true;
        }

        internal static bool EnsureUserAgentStarted()
        {
            try
            {
                int currentSessionId = Process.GetCurrentProcess().SessionId;

                if (_userAgentProcess != null &&
                    !_userAgentProcess.HasExited &&
                    _userAgentProcess.SessionId == currentSessionId)
                {
                    return true;
                }

                Process[] existingAgents =
                    Process.GetProcessesByName("DisplayMagician.UserAgent");

                foreach (Process process in existingAgents)
                {
                    try
                    {
                        if (!process.HasExited &&
                            process.SessionId == currentSessionId)
                        {
                            _userAgentProcess = process;

                            logger.Info(
                                "Program/EnsureUserAgentStarted: Reusing existing User Agent process {0} in session {1}.",
                                process.Id,
                                currentSessionId);

                            return true;
                        }
                    }
                    catch
                    {
                        process.Dispose();
                    }
                }

                string userAgentPath = Path.Combine(
                    AppStartupPath,
                    "UserAgent",
                    "DisplayMagician.UserAgent.exe");

                if (!File.Exists(userAgentPath))
                {
                    logger.Error(
                        "Program/EnsureUserAgentStarted: User Agent executable was not found at {0}.",
                        userAgentPath);

                    return false;
                }

                _userAgentProcess = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = userAgentPath,
                        WorkingDirectory = Path.GetDirectoryName(userAgentPath)!,
                        UseShellExecute = false
                    });

                return _userAgentProcess != null;
            }
            catch (Exception ex)
            {
                logger.Error(
                    ex,
                    "Program/EnsureUserAgentStarted: Failed to start the User Agent.");

                return false;
            }
        }

        private static bool ConnectDesktopStateToUserAgent()
        {
            try
            {
                if (!EnsureUserAgentStarted())
                {
                    return false;
                }

                Exception lastException = null;

                for (int attempt = 1; attempt <= 20; attempt++)
                {
                    try
                    {
                        UserAgentRepositoryConnection userAgentRepositoryConnection =
                            new UserAgentRepositoryConnection(
                                new ControlServicePipeClient());

                        ShortcutRepository.ConnectToUserAgent(
                            userAgentRepositoryConnection);

                        DesktopProfileViewCache.Refresh();

                        logger.Info(
                            "Program/ConnectDesktopStateToUserAgent: Loaded display profile views and the shortcut cache from the User Agent.");

                        return true;
                    }
                    catch (Exception ex) when (
                        ex is IOException ||
                        ex is TimeoutException ||
                        ex is InvalidOperationException)
                    {
                        lastException = ex;

                        logger.Debug(
                            ex,
                            "Program/ConnectDesktopStateToUserAgent: User Agent is not ready yet. Attempt {0}/20.",
                            attempt);

                        Thread.Sleep(500);
                    }
                }

                if (lastException != null)
                {
                    logger.Error(
                        lastException,
                        "Program/ConnectDesktopStateToUserAgent: User Agent did not become ready within the startup timeout.");
                }

                return false;
            }
            catch (Exception ex)
            {
                logger.Error(
                    ex,
                    "Program/ConnectDesktopStateToUserAgent: Could not load desktop state from the User Agent.");

                return false;
            }
        }

        internal static void StopUserAgentIfIdle()
        {
            if (_userAgentProcess == null || _userAgentProcess.HasExited)
            {
                return;
            }

            try
            {
                ControlServicePipeClient controlServiceClient = new ControlServicePipeClient();
                DisplayMagician.Contracts.ControlResponse response = controlServiceClient.StopAgentIfIdleAsync(CancellationToken.None).GetAwaiter().GetResult();
                if (!response.IsSuccessful)
                {
                    logger.Info("Program/StopUserAgentIfIdle: The User Agent remains running. ErrorCode={0}; Message={1}", response.ErrorCode, response.Message);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Program/StopUserAgentIfIdle: Unable to ask the User Agent to stop before WinForms exits.");
            }
        }

        private static bool EnsurePackageIdentity(string[] startupArguments)
        {
            if (ExecutionMode.TryGetPackageFullName(out string packageFullName, out int errorCode))
            {
                AppHasPackageIdentity = true;
                logger.Info($"Program/EnsurePackageIdentity: DisplayMagician is running with package identity {packageFullName}.");
                return false;
            }

            AppHasPackageIdentity = false;
            logger.Warn($"Program/EnsurePackageIdentity: DisplayMagician is not running with package identity. GetCurrentPackageFullName returned {errorCode}.");

            if (!File.Exists(AppIdentityPkgPath))
            {
                logger.Warn($"Program/EnsurePackageIdentity: Cannot register package identity because {AppIdentityPkgPath} does not exist.");
                _packageIdentityWarningNeeded = true;
                return false;
            }

            bool registrationSucceeded = RegisterPackageWithExternalLocationAsync(AppStartupPath, AppIdentityPkgPath).GetAwaiter().GetResult();
            if (registrationSucceeded)
            {
                if (!startupArguments.Any(argument => String.Equals(argument, PackageIdentityRestartCommandLineOption, StringComparison.OrdinalIgnoreCase)))
                {
                    string restartArguments = String.Join(" ", startupArguments
                        .Select(argument => $"\"{argument.Replace("\"", "\\\"")}\"")
                        .Append(PackageIdentityRestartCommandLineOption));
                    try
                    {
                        logger.Info("Program/EnsurePackageIdentity: Package identity registration completed. Restarting DisplayMagician so the new process receives its package identity.");
                        Process.Start(new ProcessStartInfo(Application.ExecutablePath, restartArguments) { UseShellExecute = true });
                        return true;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, "Program/EnsurePackageIdentity: Package identity registration completed, but DisplayMagician could not restart itself.");
                    }
                }

                logger.Warn("Program/EnsurePackageIdentity: Package identity registration completed, but the restarted process still has no package identity.");
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

            return false;
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
                try
                {
                    await ListenForControlServiceEventsAsync(_clientEventListenerCancellationSource.Token);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "Program/QueueStartupBackgroundTasks: Control Service event subscription failed. DisplayMagician will continue running.");
                }
            });
        }

        private static async Task ListenForControlServiceEventsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await new ControlServicePipeClient().SubscribeClientEventsAsync(HandleControlServiceEventAsync, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is InvalidOperationException)
                {
                    logger.Warn(ex, "Program/ListenForControlServiceEventsAsync: Control Service event listener disconnected. Retrying shortly.");
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }
        }

        private static Task HandleControlServiceEventAsync(ControlClientEvent clientEvent)
        {
            if (clientEvent.EventType == ControlClientEventType.ClientSyncCompleted && clientEvent.ClientSync != null)
            {
                _mainSynchronizationContext?.Post(_ => HandleClientSyncEvent(clientEvent.ClientSync), null);
            }
            else if (clientEvent.EventType == ControlClientEventType.OperationStatusUpdated && clientEvent.OperationStatus != null)
            {
                if (!clientEvent.OperationStatus.IsTerminal && clientEvent.PublishedUtc != default && DateTime.UtcNow - clientEvent.PublishedUtc > ControlProtocol.EventIdleTimeout)
                {
                    logger.Warn("Program/HandleControlServiceEventAsync: Ignored delayed operation status for {0}.", clientEvent.OperationStatus.OperationId);
                    return Task.CompletedTask;
                }
                _mainSynchronizationContext?.Post(_ => HandleOperationStatusEvent(clientEvent.OperationStatus), null);
            }
            else if (clientEvent.EventType == ControlClientEventType.OperationDecisionUpdated && clientEvent.OperationDecision != null)
            {
                _mainSynchronizationContext?.Post(_ => HandleOperationDecisionEvent(clientEvent.OperationDecision), null);
            }

            return Task.CompletedTask;
        }

        private static void HandleOperationStatusEvent(OperationStatus status)
        {
            if (_lastOperationStatusSequences.TryGetValue(status.OperationId, out long lastSequence) && status.Sequence <= lastSequence)
            {
                return;
            }

            _lastOperationStatusSequences[status.OperationId] = status.Sequence;
            if (status.IsTerminal)
            {
                _lastOperationStatusSequences.TryRemove(status.OperationId, out _);
            }

            ShowOperationStatusToast(status);
        }

        private static void HandleOperationDecisionEvent(OperationDecision decision)
        {
            if (decision.IsResolved)
            {
                if (_operationDecisionForms.TryRemove(decision.PromptId, out OperationDecisionForm activeForm) && !activeForm.IsDisposed)
                {
                    activeForm.CloseBecauseAnotherClientResponded();
                }

                _displayedOperationDecisionPrompts.TryRemove(decision.PromptId, out _);
                return;
            }

            if (!_displayedOperationDecisionPrompts.TryAdd(decision.PromptId, 0))
            {
                return;
            }

            using OperationDecisionForm form = new OperationDecisionForm(decision);
            _operationDecisionForms[decision.PromptId] = form;
            try
            {
                form.ShowDialog(AppMainForm);
                if (!form.WasResolvedByAnotherClient)
                {
                    _ = ResolveOperationDecisionAsync(decision.PromptId, form.SelectedChoice);
                }
            }
            finally
            {
                _operationDecisionForms.TryRemove(decision.PromptId, out _);
            }
        }

        private static async Task ResolveOperationDecisionAsync(Guid promptId, OperationDecisionChoice choice)
        {
            try
            {
                await new ControlServicePipeClient().ResolveOperationDecisionAsync(promptId, choice, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is InvalidOperationException)
            {
                logger.Warn(ex, "Program/ResolveOperationDecisionAsync: Could not resolve operation decision {0}; the Control Service will use its Continue default.", promptId);
            }
            finally
            {
                _displayedOperationDecisionPrompts.TryRemove(promptId, out _);
            }
        }

        private static void HandleClientSyncEvent(DisplayMagician.Contracts.ClientSyncResult syncResult)
        {
            if (syncResult.MessageSync?.NewMessagesCount > 0 && AppProgramSettings?.ShowMessageToasts != false)
            {
                ShowNewMessagesToast(syncResult.MessageSync.NewMessagesCount);
            }

            RefreshMessageIndicators();
            ClientSyncUpdateView selectedUpdate = AppProgramSettings?.UpgradeToPreReleases == true ? syncResult.PrereleaseUpdate : syncResult.StableUpdate;
            if (selectedUpdate != null)
            {
                ShowClientSyncUpdate(selectedUpdate, automatic: true);
            }
        }

        private static async Task RunClientSyncAndNotifyUserAsync(bool manual)
        {
            DisplayMagician.Contracts.ClientSyncResult syncResult = await new ControlServicePipeClient().SyncClientAsync(manual, AppProgramSettings?.UpgradeToPreReleases == true, CancellationToken.None).ConfigureAwait(false);
            if (!syncResult.WasDue)
            {
                return;
            }

            if (syncResult.MessageSync?.NewMessagesCount > 0 && AppProgramSettings?.ShowMessageToasts != false)
            {
                ShowNewMessagesToast(syncResult.MessageSync.NewMessagesCount);
            }
            RefreshMessageIndicators();
            ClientSyncUpdateView selectedUpdate = AppProgramSettings?.UpgradeToPreReleases == true ? syncResult.PrereleaseUpdate : syncResult.StableUpdate;
            if (selectedUpdate != null)
            {
                ShowClientSyncUpdate(selectedUpdate, manual);
            }
        }

        public static bool GetShareAnonymousUsageMetrics()
        {
            try
            {
                return new ControlServicePipeClient().GetAnonymousMetricsSettingsAsync(CancellationToken.None).GetAwaiter().GetResult().ShareAnonymousUsageMetrics;
            }
            catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is InvalidOperationException)
            {
                logger.Warn(ex, "Program/GetShareAnonymousUsageMetrics: The Control Service anonymous metrics settings are unavailable.");
                return false;
            }
        }

        public static bool UpdateShareAnonymousUsageMetrics(bool shareAnonymousUsageMetrics)
        {
            try
            {
                new ControlServicePipeClient().UpdateAnonymousMetricsSettingsAsync(shareAnonymousUsageMetrics, CancellationToken.None).GetAwaiter().GetResult();
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is InvalidOperationException)
            {
                logger.Warn(ex, "Program/UpdateShareAnonymousUsageMetrics: The Control Service anonymous metrics settings could not be updated.");
                return false;
            }
        }

        public static async Task CheckForNewMessagesAsync(Form owner)
        {
            DisplayMagician.Contracts.ClientSyncResult result;
            try
            {
                result = await new ControlServicePipeClient().SyncClientAsync(true, AppProgramSettings?.UpgradeToPreReleases == true, CancellationToken.None);
            }
            catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is InvalidOperationException)
            {
                MessageBox.Show(owner, "DisplayMagician could not check for new messages. Please try again later.", "Check for new messages", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            RefreshMessageIndicators();
            int newMessagesCount = result.MessageSync?.NewMessagesCount ?? 0;
            string completionMessage = newMessagesCount == 1
                ? "DisplayMagician found 1 new message."
                : $"DisplayMagician found {newMessagesCount} new messages.";
            MessageBox.Show(owner, completionMessage, "Check for new messages", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void ReportAnonymousMetricsUsage(bool isLaunch, long activeMinutes)
        {
            try
            {
                new ControlServicePipeClient().ReportAnonymousMetricsUsageAsync(new AnonymousMetricsUsageReport
                {
                    AppVersion = AppVersion,
                    UpdateChannel = AppProgramSettings.UpgradeToPreReleases ? "prerelease" : "stable",
                    IsLaunch = isLaunch,
                    ActiveMinutes = activeMinutes
                }, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is InvalidOperationException)
            {
                logger.Warn(ex, "Program/ReportAnonymousMetricsUsage: The Control Service anonymous metrics store is unavailable.");
            }
        }

        private static MessageListResult GetMessageListFromUserAgent()
        {
            try
            {
                return new ControlServicePipeClient().ListMessagesAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is InvalidOperationException)
            {
                logger.Warn(ex, "Program/GetMessageListFromUserAgent: The User Agent message store is unavailable.");
                return new MessageListResult();
            }
        }

        public static int GetUnreadMessageCount()
        {
            return GetMessageListFromUserAgent().UnreadCount;
        }

        private static void SetMessageReadState(IEnumerable<string> ids, bool isRead)
        {
            try
            {
                new ControlServicePipeClient().SetMessageReadStateAsync(ids, isRead, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is InvalidOperationException)
            {
                logger.Warn(ex, "Program/SetMessageReadState: The User Agent message store is unavailable.");
            }
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

        private static void ShowOperationStatusToast(OperationStatus status)
        {
            try
            {
                string operationName = status.OperationType == DisplayOperationType.StartShortcut ? "Game shortcut" : "Display operation";
                string outcome = status.Phase switch
                {
                    OperationPhase.Completed => "completed",
                    OperationPhase.Cancelled => "cancelled",
                    OperationPhase.Failed => "failed",
                    _ => "update"
                };
                string headerText = status.IsTerminal ? $"{operationName} {outcome}" : $"{operationName}: {status.Phase}";
                string message = string.IsNullOrWhiteSpace(status.Message) ? "DisplayMagician is processing your request." : status.Message;
                if (status.IsStale)
                {
                    message = $"{message} Status is no longer confirmed. {status.StaleReason}";
                }

                ToastContentBuilder toast = new ToastContentBuilder()
                    .AddText(headerText, hintMaxLines: 1)
                    .AddText(message)
                    .SetToastDuration(ToastDuration.Short);
                if (status.IsTerminal)
                {
                    toast.AddAudio(new Uri("ms-winsoundevent:Notification.Default"), false, true);
                }

                toast.Show();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Program/ShowOperationStatusToast: Could not show operation status toast for {0} sequence {1}.", status.OperationId, status.Sequence);
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
                if (AppDirectInputManager == null)
                {
                    logger.Trace($"Program/StartDirectInputManager: Creating DirectInput Device Manager.");
                    AppDirectInputManager = new DirectInputManager();
                    logger.Trace($"Program/StartDirectInputManager: Initialising DirectInput Device Manager with the MainForm window handle.");
                    AppDirectInputManager.Initialize(AppMainForm.Handle);
                }

                AppDirectInputManager.Stop();
                AppDirectInputManager.ClearRegisteredHotkeys();
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

        public static void RefreshDirectInputHotkeys()
        {
            StartDirectInputManager();
        }

        private static void ShowMigrationSummary(IReadOnlyList<ConfigMigrationRunner.MigrationNotice> notices)
        {
            if (notices == null || notices.Count == 0)
                return;

            try
            {
                StringBuilder summary = new StringBuilder();
                foreach (ConfigMigrationRunner.MigrationNotice notice in notices)
                {
                    if (!string.IsNullOrWhiteSpace(notice.Title))
                    {
                        summary.AppendLine($"**{notice.Title}**");
                    }

                    summary.AppendLine(notice.Message);
                    summary.AppendLine();
                }

                using (MigrationSummaryForm migrationSummaryForm = new MigrationSummaryForm())
                {
                    migrationSummaryForm.SummaryText = summary.ToString().Trim();
                    migrationSummaryForm.ShowDialog(AppMainForm);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Program/ShowMigrationSummary: Unable to show the completed migration summary.");
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

        internal static void EnableTestUpdateFeed(string source, bool checkForUpdatesNow)
        {
            bool wasAlreadyEnabled = _useTestUpdateFeed;
            _useTestUpdateFeed = true;

            logger.Warn($"Program/EnableTestUpdateFeed: TEST UPDATE MODE {(wasAlreadyEnabled ? "is already" : "has been")} enabled from {source}. This session will use the static test client sync document.");

            if (checkForUpdatesNow)
            {
                logger.Warn($"Program/EnableTestUpdateFeed: Starting an immediate test-feed update check requested from {source}.");
                CheckForUpdates(automatic: false);
            }
        }

        public static void CheckForUpdates(bool automatic = true, string requestedMessageUpdateVersion = null, string requestedMessageUpdateChannel = null)
        {
            _lastUpdateCheckWasAutomatic = automatic;
            _requestedMessageUpdateVersion = requestedMessageUpdateVersion;
            _requestedMessageUpdateChannel = requestedMessageUpdateChannel;
            logger.Info($"Program/CheckForUpdates: Starting {(automatic ? "automatic" : "manual")} combined client sync.");
            Task.Run(() => RunClientSyncAndNotifyUserAsync(manual: !automatic));
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

        private static void ShowClientSyncUpdate(ClientSyncUpdateView update, bool automatic)
        {
            if (!AppProgramSettings.UpgradeEnabled || !Version.TryParse(update.Version, out Version availableVersion) || !Version.TryParse(AppVersion, out Version installedVersion))
            {
                return;
            }

            _lastUpdateCheckWasAutomatic = automatic;
            RegisterAutoUpdaterEvents();
            AutoUpdater.RunUpdateAsAdmin = true;
            AutoUpdater.HttpUserAgent = "DisplayMagician AutoUpdater";
            AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Days;
            AutoUpdater.RemindLaterAt = 7;
            AutoUpdater.InstalledVersion = installedVersion;
            AppUpgradeExtraDetails = new UpgradeExtraDetails();

            AutoUpdaterOnCheckForUpdateEvent(new UpdateInfoEventArgs
            {
                CurrentVersion = update.Version,
                InstalledVersion = installedVersion,
                DownloadURL = update.Url,
                ChangelogURL = update.Changelog,
                IsUpdateAvailable = availableVersion > installedVersion,
                Mandatory = new Mandatory
                {
                    Value = update.Mandatory,
                    UpdateMode = (Mode)update.MandatoryMode,
                    MinimumVersion = update.MandatoryMinimumVersion
                },
                CheckSum = new CheckSum
                {
                    Value = update.ChecksumValue,
                    HashingAlgorithm = update.ChecksumAlgorithm
                }
            });
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
                if (args.IsUpdateAvailable)
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
                    MessageView releaseAnnouncement = GetMessageListFromUserAgent().Messages.FirstOrDefault(message =>
                        string.Equals(message.Kind, "releaseAnnouncement", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(message.ReleaseVersion, args.CurrentVersion, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(message.ReleaseChannel, updateChannel, StringComparison.OrdinalIgnoreCase));

                    if (releaseAnnouncement != null)
                    {
                        SetMessageReadState(new[] { releaseAnnouncement.Id }, true);
                        RefreshMessageIndicators();

                        if (!string.IsNullOrWhiteSpace(releaseAnnouncement.Content))
                        {
                            upgradeForm.ReleaseNotesHtml = releaseAnnouncement.Content;
                            upgradeForm.ReleaseNotesFormat = releaseAnnouncement.Format;
                        }
                        else
                        {
                            logger.Warn($"Program/AutoUpdaterOnCheckForUpdateEvent: Release announcement content is missing for version {args.CurrentVersion} (messageId={releaseAnnouncement.Id}). Showing the upgrade-form fallback text instead.");
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
                                    Program.CancelActiveOperation();
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
                        AppTempStartMenuPath, Application.ExecutablePath, null, AppUserModelId, AppActivationId);
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
