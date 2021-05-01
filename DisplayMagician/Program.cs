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

namespace DisplayMagician {

    public enum ApplyProfileResult
    {
        Successful,
        Cancelled,
        Error
    }

    internal static class Program
    {
        internal static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        public static string AppStartupPath = Application.StartupPath;
        public static string AppIconPath = Path.Combine(Program.AppDataPath, $"Icons");
        public static string AppProfilePath = Path.Combine(Program.AppDataPath, $"Profiles");
        public static string AppShortcutPath = Path.Combine(Program.AppDataPath, $"Shortcuts");
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

            // This is the RunShortcut command
            app.Command(DisplayMagicianStartupAction.RunShortcut.ToString(), (runShortcutCmd) =>
            {
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
                var argumentProfile = runProfileCmd.Argument("\"Profile_UUID\"", "(required) The UUID of the profile to run from those stored in the profile file.").IsRequired();
                argumentProfile.Validators.Add(new ProfileMustExistValidator());

                //description and help text of the command.
                runProfileCmd.Description = "Use this command to change to a display profile of your choosing.";

                runProfileCmd.OnExecute(() =>
                {
                    logger.Debug($"ChangeProfile commandline command was invoked!");

                    try
                    {
                        // Lookup the profile
                        ProfileItem profileToUse = ProfileRepository.AllProfiles.Where(p => p.UUID.Equals(argumentProfile.Value)).First();

                        ApplyProfile(profileToUse);
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
                Console.WriteLine("Starting Normally...");
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

                    // Save a copy of the Steam Icon, and all the game library ones in preparation for future use
                    if (!File.Exists(AppSteamIconFilename))
                    {
                        Icon heliosIcon = (Icon)Properties.Resources.Steam;
                        using (FileStream fs = new FileStream(AppSteamIconFilename, FileMode.Create))
                            heliosIcon.Save(fs);
                    }

                    // Save a copy of the Uplay Icon, and all the game library ones in preparation for future use
                    if (!File.Exists(AppUplayIconFilename))
                    {
                        Icon heliosIcon = (Icon)Properties.Resources.Uplay;
                        using (FileStream fs = new FileStream(AppUplayIconFilename, FileMode.Create))
                            heliosIcon.Save(fs);
                    }

                    // Save a copy of the Epic Icon, and all the game library ones in preparation for future use
                    if (!File.Exists(AppEpicIconFilename))
                    {
                        Icon heliosIcon = (Icon)Properties.Resources.Epic;
                        using (FileStream fs = new FileStream(AppEpicIconFilename, FileMode.Create))
                            heliosIcon.Save(fs);
                    }

                    // Save a copy of the Origin Icon, and all the game library ones in preparation for future use
                    if (!File.Exists(AppOriginIconFilename))
                    {
                        Icon heliosIcon = (Icon)Properties.Resources.Origin;
                        using (FileStream fs = new FileStream(AppOriginIconFilename, FileMode.Create))
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

        // ApplyProfile lives here so that the UI works.
        public static ApplyProfileResult ApplyProfile(ProfileItem profile)
        {
            logger.Debug($"Program/ApplyProfile: Starting");

            profile.RefreshPossbility();          

            // We need to check if the profile is valid
            if (!profile.IsPossible)
            {
                logger.Debug($"Program/ApplyProfile: The supplied profile {profile.Name} isn't currently possible to use, so we can't apply it. This means a display that existed before has been removed, or moved.");
                return ApplyProfileResult.Error;
            }


            // We need to check if the profile is the same one that we're on
            if (profile.UUID == ProfileRepository.GetActiveProfile().UUID)
            {
                logger.Debug($"Program/ApplyProfile: The supplied profile {profile.Name} is currently in use, so we don't need to apply it.");
                return ApplyProfileResult.Successful;
            }

            try
            {
                // Now lets prepare changing the display topology task
                Task applyTopologyTask = new Task(() =>
                {
                    Console.WriteLine("Program/ApplyProfile : Applying Profile Topology " + profile.Name);
                    if (!ProfileRepository.ApplyNVIDIAGridTopology(profile))
                    {
                        // Somehow return that this profile topology didn't apply
                        throw new ApplyTopologyException("Program/ApplyProfile: ApplyNVIDIAGridTopology: Error setting up the NVIDIA Surround Grid Topology");
                    }
                });

                Task applyPathInfoTask = new Task(() => {
                    Console.WriteLine("Program/ApplyProfile  : Applying Profile Path " + profile.Name);
                    if (!ProfileRepository.ApplyWindowsDisplayPathInfo(profile))
                    {
                        // Somehow return that this profile path info didn't apply
                        throw new ApplyPathInfoException("Program/ApplyProfile: ApplyWindowsDisplayPathInfo: Error configuring the Windows Display Devices");
                    }

                });

                // Set up the UI forms to show
                ApplyingProfileForm timeoutForm = new ApplyingProfileForm(null, 3, $"Changing to '{profile.Name}' Profile", "Press ESC to cancel", Color.Orange, true); 
                ApplyingProfileForm topologyForm = new ApplyingProfileForm(applyTopologyTask, 30, $"Changing to '{profile.Name}' Profile", "Applying NVIDIA Grid Topology", Color.Aquamarine);
                ApplyingProfileForm pathInfoForm = new ApplyingProfileForm(applyPathInfoTask, 15, $"Changing to '{profile.Name}' Profile", "Adjusting Windows Display Device positions", Color.LawnGreen);

                if (timeoutForm.ShowDialog() == DialogResult.Cancel)
                {
                    return ApplyProfileResult.Cancelled;
                }

                // We only want to do the topology change if the profile we're on now
                // or the profile we're going to are NVIDIA surround profiles
                int toProfileSurroundTopologyCount =
                    profile.Paths.SelectMany(paths => paths.TargetDisplays)
                        .Select(target => target.SurroundTopology)
                        .Where(topology => topology != null)
                        .Select(topology => topology.ToGridTopology())
                        .Count();
                if (toProfileSurroundTopologyCount > 0)
                    logger.Debug($"Program/ApplyProfile: {profile.Name} profile we want to use is a NVIDIA Surround profile, so we need to change the NVIDIA GRID topology.");
                else
                    logger.Debug($"Program/ApplyProfile: {profile.Name} profile we want to use does not use NVIDIA Surround.");

                int fromProfileSurroundTopologyCount =
                    ProfileRepository.CurrentProfile.Paths.SelectMany(paths => paths.TargetDisplays)
                        .Select(target => target.SurroundTopology)
                        .Where(topology => topology != null)
                        .Select(topology => topology.ToGridTopology())
                        .Count();
                if (fromProfileSurroundTopologyCount > 0)
                    logger.Debug($"Program/ApplyProfile: {ProfileRepository.CurrentProfile} profile currently in use is a NVIDIA Surround profile, so we need to change the NVIDIA GRID topology.");
                else
                    logger.Debug($"Program/ApplyProfile: {ProfileRepository.CurrentProfile} profile currently in use does not use NVIDIA Surround.");

                if (toProfileSurroundTopologyCount > 0 || fromProfileSurroundTopologyCount > 0)
                {
                    logger.Debug($"Program/ApplyProfile: Changing the NVIDIA GRID topology to apply or remove a NVIDIA Surround profile");
                    topologyForm.ShowDialog();

                    try
                    {
                        applyTopologyTask.Wait();
                    }
                    catch (AggregateException ae)
                    {
                        logger.Error(ae, $"Program/ApplyProfile exception during applyTopologyTask");
                        foreach (var e in ae.InnerExceptions)
                        {
                            // Handle the custom exception.
                            if (e is ApplyTopologyException)
                            {
                                Console.WriteLine(e.Message);
                            }
                            // Rethrow any other exception.
                            else
                            {
                                throw;
                            }
                        }
                    }

                    if (applyTopologyTask.IsFaulted)
                        Console.WriteLine("Program/ApplyProfile : Applying Profile Topology stage failed to complete");

                    if (!applyTopologyTask.IsCompleted)
                    {
                        logger.Debug($"Program/ApplyProfile: Failed to complete applying or removing the NVIDIA Surround profile");
                        return ApplyProfileResult.Error;
                    }
                }

                // We always want to do the WindowsDisplayAPI PathInfo part
                logger.Debug($"Program/ApplyProfile: Changing the Windows Display Path Info to change the Windows Display layout");
                pathInfoForm.ShowDialog();
                try
                {
                    applyPathInfoTask.Wait();
                }
                catch (AggregateException ae)
                {
                    logger.Error(ae, $"Program/ApplyProfile exception during applyPathInfoTask");
                    foreach (var e in ae.InnerExceptions)
                    {
                        // Handle the custom exception.
                        if (e is ApplyPathInfoException)
                        {
                            Console.WriteLine(e.Message);
                        }
                        // Rethrow any other exception.
                        else
                        {
                            throw;
                        }
                    }
                }

                if (applyPathInfoTask.IsFaulted)
                    logger.Debug($"Program/ApplyProfile: Applying Profile PathInfo stage failed to complete");

                if (!applyPathInfoTask.IsCompleted)
                    return ApplyProfileResult.Error;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfileRepository/ApplyTopology exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                {
                    logger.Debug($"Program/ApplyProfile: Failed to complete changing the Windows Display layout");
                    return ApplyProfileResult.Error;
                }
            }

            ProfileRepository.UpdateActiveProfile();

            return ApplyProfileResult.Successful;
        }

        public static bool LoadGamesInBackground()
        {

            logger.Debug($"Program/LoadGamesInBackground: Starting");
            // Now lets prepare loading all the Steam games we have installed
            
            Task loadSteamGamesTask = new Task(() =>
            {
                // Check if Steam is installed
                GameLibrary steamLibrary = SteamLibrary.GetLibrary();
                if (steamLibrary.IsGameLibraryInstalled)
                {
                    // Load Steam library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed Steam Games");
                    Console.Write("Loading Installed Steam Games...");
                    if (!steamLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Steam Games!");
                    }
                    Console.WriteLine("Done.");
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed Steam Games (found {steamLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: Steam not installed.");
                    Console.WriteLine("Steam not installed.");
                }
            });

            // Now lets prepare loading all the Uplay games we have installed
            Task loadUplayGamesTask = new Task(() =>
            {
                // Check if Uplay is installed
                GameLibrary uplayLibrary = SteamLibrary.GetLibrary();
                if (uplayLibrary.IsGameLibraryInstalled)
                {
                    // Load Uplay library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed Uplay Games");
                    Console.Write("Loading Installed Uplay Games...");
                    if (!uplayLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Uplay Games!");
                    }
                    Console.WriteLine("Done.");
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed Uplay Games (found {uplayLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: Uplay not installed.");
                    Console.WriteLine("Uplay not installed.");
                }

            });

            // Now lets prepare loading all the Origin games we have installed
            Task loadOriginGamesTask = new Task(() =>
            {
                // Check if Origin is installed
                GameLibrary originLibrary = SteamLibrary.GetLibrary();
                if (originLibrary.IsGameLibraryInstalled)
                {
                    // Load Origin library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed Origin Games");
                    Console.Write("Loading Installed Origin Games...");
                    if (!originLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Origin Games!");
                    }
                    Console.WriteLine("Done.");
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed Origin Games (found {originLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: Origin not installed.");
                    Console.WriteLine("Origin not installed.");
                }

            });

            Action loadSteamGamesAction = new Action(() =>
            {
                // Check if Steam is installed
                GameLibrary steamLibrary = SteamLibrary.GetLibrary();
                if (steamLibrary.IsGameLibraryInstalled)
                {
                    // Load Steam library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed Steam Games");
                    Console.Write("Loading Installed Steam Games...");
                    if (!steamLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Steam Games!");
                    }
                    Console.WriteLine("Done.");
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
                    Console.Write("Loading Installed Uplay Games...");
                    if (!uplayLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Uplay Games!");
                    }
                    Console.WriteLine("Done.");
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
                    Console.Write("Loading Installed Origin Games...");
                    if (!originLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Origin Games!");
                    }
                    Console.WriteLine("Done.");
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed Origin Games (found {originLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: Origin not installed.");
                    Console.WriteLine("Origin not installed.");
                }

            });


            // Store all the actions in a array so we can wait on them later
            List<Action> loadGamesActions = new List<Action>();
            loadGamesActions.Add(loadSteamGamesAction);
            loadGamesActions.Add(loadUplayGamesAction);
            loadGamesActions.Add(loadOriginGamesAction);

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


            // TODO replicate this failed Task handling in Actions
            /*bool failedAction = false;
            foreach (var loadGameAction in loadGamesActions)
            {
                if (loadGameAction.  .Exception != null)
                {
                    failedTask = true;
                    foreach (var ex in loadGameTask.Exception.InnerExceptions)
                        logger.Error(ex, $"Program/LoadGamesInBackground exception during loadGamesTasks");
                }
            }

            if (failedTask)
                return false;*/

            return true;

        }

    }


    public class ApplyTopologyException : Exception
    {
        public ApplyTopologyException()
        { }

        public ApplyTopologyException(string message) : base(message)
        { }

        public ApplyTopologyException(string message, Exception innerException) : base(message, innerException)
        { }
        public ApplyTopologyException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }

    public class ApplyPathInfoException : Exception
    {
        public ApplyPathInfoException()
        { }
        
        public ApplyPathInfoException(string message) : base(message)
        { }
        public ApplyPathInfoException(string message, Exception innerException) : base(message, innerException)
        { }
        public ApplyPathInfoException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
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