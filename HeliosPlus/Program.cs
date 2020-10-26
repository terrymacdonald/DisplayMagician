using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HeliosPlus.InterProcess;
using HeliosPlus.Resources;
using HeliosPlus.GameLibraries;
using HeliosPlus.Shared;
using HeliosPlus.UIForms;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Diagnostics.Contracts;

namespace HeliosPlus {
 
    public enum SupportedGameLibrary
    {
        Unknown,
        Steam,
        Uplay
    }

    internal static class Program
    {

        internal static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HeliosPlus");
        public static string AppIconPath = Path.Combine(Program.AppDataPath, $"Icons");
        public static string AppProfilePath = Path.Combine(Program.AppDataPath, $"Profiles");
        public static string AppShortcutPath = Path.Combine(Program.AppDataPath, $"Shortcuts");
        public static string AppHeliosPlusIconFilename = Path.Combine(AppIconPath, @"HeliosPlus.ico");
        public static string AppOriginIconFilename = Path.Combine(AppIconPath, @"Origin.ico");
        public static string AppSteamIconFilename = Path.Combine(AppIconPath, @"Steam.ico");
        public static string AppUplayIconFilename = Path.Combine(AppIconPath, @"Uplay.ico");
        public static string AppEpicIconFilename = Path.Combine(AppIconPath, @"Epic.ico");
        public static ProgramSettings AppProgramSettings;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {

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
            
            // Load the program settings
            AppProgramSettings = ProgramSettings.LoadSettings();

            var app = new CommandLineApplication();

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
            app.Command(HeliosStartupAction.RunShortcut.ToString(), (runShortcutCmd) =>
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

            app.OnExecute(() =>
            {

                Console.WriteLine("Starting Normally...");
                StartUpNormally();
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
                Console.WriteLine("Didn't recognise the supplied commandline options: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Program/Main commandParsingException: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // You'll always want to catch this exception, otherwise it will generate a messy and confusing error for the end user.
                // the message will usually be something like:
                // "Unrecognized command or argument '<invalid-command>'"
                Console.WriteLine($"Program/Main exception: Unable to execute application - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
            }

            // Exit with a 0 Errorlevel to indicate everything worked fine!
            return 0;
        }

        private static void StartUpNormally()
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
                        // TODO
                    }
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
                        // TODO
                    }
                }

                try
                {
                    // Save a copy of the HeliosPlus Icon, and all the game library ones in preparation for future use
                    if (!File.Exists(AppHeliosPlusIconFilename))
                    {
                        Icon heliosIcon = (Icon)Properties.Resources.HeliosPlus;
                        using (FileStream fs = new FileStream(AppHeliosPlusIconFilename, FileMode.Create))
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
                }
                IPCService.GetInstance().Status = InstanceStatus.User;
                Application.Run(new UIForms.MainForm());

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/StartUpNormally exception 3: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
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
                if (!HeliosPlus.GameLibraries.SteamLibrary.LoadInstalledGames())
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
                /* if (!HeliosPlus.GameLibraries.UplayLibrary.LoadInstalledGames())
                {
                    // Somehow return that this profile topology didn't apply
                    throw new LoadingInstalledGamesException("Program/LoadGamesInBackground: Cannot load installed Uplay Games!");
                }
                */

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
        public ApplyTopologyException(String message) : base(message)
        { }
    }

    public class ApplyPathInfoException : Exception
    {
        public ApplyPathInfoException(String message) : base(message)
        { }
    }

    public class LoadingInstalledGamesException : Exception
    {
        public LoadingInstalledGamesException(String message) : base(message)
        { }
    }
}