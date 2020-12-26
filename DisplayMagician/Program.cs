using System;
using McMaster.Extensions.CommandLineUtils;
using System.Diagnostics;
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
using System.Text.RegularExpressions;
using System.Drawing;
using DesktopNotifications;
using System.Runtime.Serialization;

namespace DisplayMagician {
 
    public enum SupportedGameLibrary
    {
        Unknown,
        Steam,
        Uplay
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
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            string date = DateTime.Now.ToString("yyyyMMdd.HHmmss");
            string AppLogFilename = Path.Combine(Program.AppLogPath, $"DisplayMagician.log");

            // Create the Shortcut Icon Cache if it doesn't exist so that it's avilable for all the program
            if (!Directory.Exists(AppLogPath))
            {
                try
                {
                    Directory.CreateDirectory(AppLogPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Program/StartUpNormally exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                }
            }

            var logfile = new NLog.Targets.FileTarget("logfile") { 
                FileName = AppLogFilename 
            };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

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
                    logLevel = NLog.LogLevel.Warn;
                    break;
            }
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(logLevel, NLog.LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;


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
            

            var app = new CommandLineApplication
            {
                AllowArgumentSeparator = true,
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
            }; 

            //app.Name = "HeliosDM+";
            //app.Name = Assembly.GetEntryAssembly().GetName().Name;
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
                    Console.WriteLine("Starting up and creating a new Display Profile...");
                    StartUpApplication(DisplayMagicianStartupAction.CreateProfile);
                    return 0;
                });
            });

            app.OnExecute(() =>
            {

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
                            Program.AppToastActivated = true;
                            break;
                        }

                    }
                }
                Console.WriteLine("Starting Normally...");
                StartUpApplication();
                return 0;
            });

            // Try to load all the games in parallel to this process
            Task.Run(() => LoadGamesInBackground());

            try
            {
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

            // Remove all the notifications we have set as they don't matter now!
            DesktopNotificationManagerCompat.History.Clear();

            // Shutdown NLog
            NLog.LogManager.Shutdown();

            // Exit with a 0 Errorlevel to indicate everything worked fine!
            return 0;
        }

        private static void StartUpApplication(DisplayMagicianStartupAction startupAction = DisplayMagicianStartupAction.StartUpNormally)
        {
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

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
                if (startupAction == DisplayMagicianStartupAction.StartUpNormally)
                {
                    AppMainForm = new MainForm();
                    Application.Run(AppMainForm);
                }
                else if (startupAction == DisplayMagicianStartupAction.CreateProfile) 
                    Application.Run(new DisplayProfileForm());

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/StartUpNormally exception 3: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                logger.Error(ex, $"Program/StartUpNormally top level exception");
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
        public static bool ApplyProfile(ProfileItem profile)
        {

            // We need to check if the profile is valid
            if (!profile.IsPossible)
                return false;

            // We need to check if the profile is the same one that we're on
            if (profile.IsActive)
                return false;

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
                    return false;
                }

                // We only want to do the topology change if the profile we're on now
                // or the profile we're going to are NVIDIA surround profiles
                int toProfileSurroundTopologyCount =
                    profile.Paths.SelectMany(paths => paths.TargetDisplays)
                        .Select(target => target.SurroundTopology)
                        .Where(topology => topology != null)
                        .Select(topology => topology.ToGridTopology())
                        .Count();
                int fromProfileSurroundTopologyCount =
                    ProfileRepository.CurrentProfile.Paths.SelectMany(paths => paths.TargetDisplays)
                        .Select(target => target.SurroundTopology)
                        .Where(topology => topology != null)
                        .Select(topology => topology.ToGridTopology())
                        .Count();

                if (toProfileSurroundTopologyCount > 0 || fromProfileSurroundTopologyCount > 0)
                {
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
                        return false;
                }

                // We always want to do the WindowsDisplayAPI PathInfo part
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
                    Console.WriteLine("Program/ApplyProfile : Applying Profile PathInfo stage failed to complete");

                if (!applyPathInfoTask.IsCompleted)
                    return false;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfileRepository/ApplyTopology exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return false;
            }

            return true;
        }

        public static bool LoadGamesInBackground()
        {

            Debug.WriteLine("Program/LoadGamesInBackground : Starting");
            // Now lets prepare loading all the Steam games we have installed
            Task loadSteamGamesTask = new Task(() =>
            {
                // Load Steam library games
                Console.WriteLine("Program/LoadGamesInBackground : Loading Installed Steam Games ");
                if (!DisplayMagician.GameLibraries.SteamLibrary.LoadInstalledGames())
                {
                    // Somehow return that this profile topology didn't apply
                    throw new LoadingInstalledGamesException("Program/LoadGamesInBackground: Cannot load installed Steam Games!");
                }                
            });

            // Now lets prepare loading all the Uplay games we have installed
            Task loadUplayGamesTask = new Task(() =>
            {
                // Load Uplay library games
                Console.WriteLine("Program/LoadGamesInBackground : Loading Installed Uplay Games ");
                if (!DisplayMagician.GameLibraries.UplayLibrary.LoadInstalledGames())
                {
                    // Somehow return that this profile topology didn't apply
                    throw new LoadingInstalledGamesException("Program/LoadGamesInBackground: Cannot load installed Uplay Games!");
                }
            });

            // Store all the tasks in an array so we can wait on them later
            Task[] loadGamesTasks = new Task[2];
            loadGamesTasks[0] = loadSteamGamesTask;
            loadGamesTasks[1] = loadUplayGamesTask;

            Console.WriteLine("Program/LoadGamesInBackground : Running tasks");
            // Go through and start all the tasks
            foreach (Task loadGameTask in loadGamesTasks)
                loadGameTask.Start();

            try
            {
                Console.WriteLine("Program/LoadGamesInBackground : Waiting for tasks to finish");
                Task.WaitAll(loadGamesTasks);
                Console.WriteLine("Program/LoadGamesInBackground : All tasks completed!");
            }
            catch (AggregateException ae)
            {
                logger.Error(ae, $"Program/LoadGamesInBackground exception during loadGamesTasks");
                Console.WriteLine("Program/LoadGamesInBackground : Task exception!");
                foreach (var e in ae.InnerExceptions)
                {
                    // Handle the custom exception.
                    if (e is LoadingInstalledGamesException)
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

            bool failedTask = false;
            foreach (var loadGameTask in loadGamesTasks)
            {
                Console.WriteLine($"Program/LoadGamesInBackground: LoadGameTask #{loadGameTask.Id}: {loadGameTask.Status}");
                if (loadGameTask.Exception != null)
                {
                    failedTask = true;
                    foreach (var ex in loadGameTask.Exception.InnerExceptions)
                        Console.WriteLine("      {0}: {1}", ex.GetType().Name,
                                          ex.Message);
                }
            }

            if (failedTask)
                return false;

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