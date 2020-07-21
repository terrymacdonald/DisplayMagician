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

namespace HeliosPlus {
    public enum SupportedProgramMode
    {
        RunShortcut,
        EditProfile,
        StartUpNormally
    }

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
        public static string AppHeliosPlusIconFilename = Path.Combine(AppIconPath, @"HeliosPlus.ico");
        public static string AppOriginIconFilename = Path.Combine(AppIconPath, @"Origin.ico");
        public static string AppSteamIconFilename = Path.Combine(AppIconPath, @"Steam.ico");
        public static string AppUplayIconFilename = Path.Combine(AppIconPath, @"Steam.ico");
        public static string AppEpicIconFilename = Path.Combine(AppIconPath, @"Epic.ico");
        //internal static string ShortcutIconCachePath;


        
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
            Console.WriteLine(@"Based on Helios Display Management - Copyright © Soroush Falahati 2017-2020");

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
            app.Command(SupportedProgramMode.RunShortcut.ToString(), (switchProfileCmd) =>
            {
                var argumentShortcut = switchProfileCmd.Argument("\"SHORTCUT_UUID\"", "(required) The UUID of the shortcut to run from those stored in the shortcut library.").IsRequired();
                argumentShortcut.Validators.Add(new ShortcutMustExistValidator());

                //description and help text of the command.
                switchProfileCmd.Description = "Use this command to run favourite game or application with a display profile of your choosing.";

                switchProfileCmd.OnExecute(() =>
                {
                    // 
                    RunShortcut(argumentShortcut.Value);
                    return 0;
                });
            });


            /*// This is the EditProfile command
            app.Command(SupportedProgramMode.EditProfile.ToString(), (editProfileCmd) =>
            {
                //description and help text of the command.
                editProfileCmd.Description = "Use this command to edit a HeliosDMPlus profile.";

                var optionProfile = editProfileCmd.Option("-p|--profile", "The Profile Name or Profile ID of the profile to you want to use.", CommandOptionType.SingleValue).IsRequired();
                optionProfile.Validators.Add(new ProfileMustExistValidator());
                
                editProfileCmd.OnExecute(() =>
                {
                    Console.WriteLine($"Editing profile {optionProfile.Value()}");

                    EditProfile(
                        GetProfile(optionProfile.Value())
                    );

                    return 0;
                });

            });*/

            app.OnExecute(() =>
            {

                Console.WriteLine("Starting Normally...");
                StartUpNormally();
                return 0;
            });


            try
            {
                // This begins the actual execution of the application
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine($"Program/Main commandParsingException: {ex.Message}: {ex.InnerException}");
                // You'll always want to catch this exception, otherwise it will generate a messy and confusing error for the end user.
                // the message will usually be something like:
                // "Unrecognized command or argument '<invalid-command>'"
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/Main exception: {ex.Message}: {ex.InnerException}");
                Console.WriteLine("Unable to execute application: {0}", ex.Message);
            }
            return 0;
            //return app.Execute(args);
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
                        Console.WriteLine($"Program/StartUpNormally exception: {ex.Message}: {ex.InnerException}");
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
                    Console.WriteLine($"Program/StartUpNormally exception 2: {ex.Message}: {ex.InnerException}");
                    // TODO
                }

                IPCService.GetInstance().Status = InstanceStatus.User;
                Application.Run(new UIForms.MainForm());

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/StartUpNormally exception 3: {ex.Message}: {ex.InnerException}");
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
            ProfileItem rollbackProfile = ProfileRepository.CurrentProfile;
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

            // Do some validation to make sure the shortcut is sensible
            // And that we have enough to try and action within the shortcut
            // (in other words check everything in the shortcut is still valid)
            (bool valid, string reason) = shortcutToRun.IsValid();
            if (!valid)
            {
                throw new Exception(string.Format("Unable to run the shortcut '{0}': {1}",shortcutToRun.Name,reason));
            }

            // Try to change to the wanted profile
            if (!SwitchProfile(shortcutToRun.ProfileToUse))
            {
                throw new Exception(Language.Cannot_change_active_profile);
            }

            // Now run the pre-start applications
            // TODO: Add the prestart applications

            // Now start the main game, and wait if we have to
            if (shortcutToRun.Category.Equals(ShortcutCategory.Application))
            {
                // Start the executable
                Process process = null;
                if (shortcutToRun.ExecutableArgumentsRequired)
                    process = System.Diagnostics.Process.Start(shortcutToRun.ExecutableNameAndPath, shortcutToRun.ExecutableArguments);
                else
                    process = System.Diagnostics.Process.Start(shortcutToRun.ExecutableNameAndPath);

                // Create a list of processes to monitor
                Process[] processesToMonitor = Array.Empty<Process>();

                // Work out if we are monitoring another process other than the main executable
                if (shortcutToRun.ProcessNameToMonitorUsesExecutable)
                {
                    // If we are monitoring the same executable we started, then lets do that
                    processesToMonitor = new[] { process };
                }
                else
                {
                    // Now wait a little while for all the processes we want to monitor to start up
                    var ticks = 0;
                    while (ticks < shortcutToRun.ExecutableTimeout * 1000)
                    {
                        // Look for the processes with the ProcessName we want (which in Windows is the filename without the extension)
                        processesToMonitor = System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(shortcutToRun.DifferentExecutableToMonitor));

                        //  TODO: Fix this logic error that will only ever wait for the first process....
                        if (processesToMonitor.Length > 0)
                        {
                            break;
                        }

                        Thread.Sleep(300);
                        ticks += 300;
                    }

                    // If none started up before the timeout, then ignore the 
                    if (processesToMonitor.Length == 0)
                    {
                        processesToMonitor = new[] { process };
                    }
                } 

                // Store the process to monitor for later
                IPCService.GetInstance().HoldProcessId = processesToMonitor.FirstOrDefault()?.Id ?? 0;
                IPCService.GetInstance().Status = InstanceStatus.OnHold;

                // Add a status notification icon in the status area
                NotifyIcon notify = null;
                try
                {
                    notify = new NotifyIcon
                    {
                        Icon = Properties.Resources.HeliosPlus,
                        Text = string.Format(
                            Language.Waiting_for_the_0_to_terminate,
                            processesToMonitor[0].ProcessName),
                        Visible = true
                    };
                    Application.DoEvents();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Program/SwitchToExecutable exception: {ex.Message}: {ex.InnerException}");
                    // ignored
                }

                // Wait for the monitored process to exit
                foreach (var p in processesToMonitor)
                {
                    try
                    {
                        p.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Program/SwitchToExecutable exception 2: {ex.Message}: {ex.InnerException}");
                        // ignored
                    }
                }

                // Remove the status notification icon from the status area
                // once we've existed the game
                if (notify != null)
                {
                    notify.Visible = false;
                    notify.Dispose();
                    Application.DoEvents();
                }

            }
            else if (shortcutToRun.Category.Equals(ShortcutCategory.Game))
            {
                // If the game is a Steam Game we check for that
                if (shortcutToRun.GameLibrary.Equals(SupportedGameLibrary.Steam))
                {
                    // We now need to get the SteamGame info
                    SteamGame steamGameToRun = SteamLibrary.GetSteamGame(shortcutToRun.GameAppId);

                    // If the GameAppID matches a Steam game, then lets run it
                    if (steamGameToRun is SteamGame)
                    {
                        // Prepare to start the steam game using the URI interface 
                        // as used by Steam for it's own desktop shortcuts.
                        var address = $"steam://rungameid/{steamGameToRun.GameId}";
                        if (shortcutToRun.GameArgumentsRequired)
                        {
                            address += "/" + shortcutToRun.GameArguments;
                        }

                        // Start the URI Handler to run Steam
                        var steamProcess = System.Diagnostics.Process.Start(address);

                        // Wait for Steam game to update if needed
                        var ticks = 0;
                        while (ticks < shortcutToRun.GameTimeout * 1000)
                        {
                            if (steamGameToRun.IsRunning)
                            {
                                break;
                            }

                            Thread.Sleep(300);

                            if (!steamGameToRun.IsUpdating)
                            {
                                ticks += 300;
                            }
                        }

                        // Store the Steam Process ID for later
                        IPCService.GetInstance().HoldProcessId = steamProcess?.Id ?? 0;
                        IPCService.GetInstance().Status = InstanceStatus.OnHold;

                        // Add a status notification icon in the status area
                        NotifyIcon notify = null;
                        try
                        {
                            notify = new NotifyIcon
                            {
                                Icon = Properties.Resources.HeliosPlus,
                                Text = string.Format(
                                    Language.Waiting_for_the_0_to_terminate,
                                    steamGameToRun.GameName),
                                Visible = true
                            };
                            Application.DoEvents();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Program/SwitchToSteamGame exception: {ex.Message}: {ex.InnerException}");
                            // ignored
                        }

                        // Wait for the game to exit
                        if (steamGameToRun.IsRunning)
                        {
                            while (true)
                            {
                                if (!steamGameToRun.IsRunning)
                                {
                                    break;
                                }

                                Thread.Sleep(300);
                            }
                        }

                        // Remove the status notification icon from the status area
                        // once we've existed the game
                        if (notify != null)
                        {
                            notify.Visible = false;
                            notify.Dispose();
                            Application.DoEvents();
                        }

                    }

                }
                // If the game is a Uplay Game we check for that
                /*else if (GameLibrary.Equals(SupportedGameLibrary.Uplay))
                {
                    // We need to look up details about the game
                    if (!UplayGame.IsInstalled(GameAppId))
                    {
                        return (false, string.Format("The Uplay Game with AppID '{0}' is not installed on this computer.", GameAppId));
                    }

                }*/


            }


            IPCService.GetInstance().Status = InstanceStatus.Busy;

            // Change back to the original profile if it is different
            if (!ProfileRepository.IsActiveProfile(rollbackProfile))
            {
                if (!SwitchProfile(rollbackProfile))
                {
                    throw new Exception(Language.Cannot_change_active_profile);
                }
            }

        }

        internal static bool SwitchProfile(ProfileItem profile)
        {
            // If we're already on the wanted profile then no need to change!
            if (ProfileRepository.IsActiveProfile(profile))
                return true;

            var instanceStatus = IPCService.GetInstance().Status;

            try
            {
                IPCService.GetInstance().Status = InstanceStatus.Busy;
                var failed = false;

                if (new ApplyingChangesForm(() =>
                {
                    Task.Factory.StartNew(() =>
                    {
                        if (!(ProfileRepository.ApplyProfile(profile)))
                        {
                            failed = true;
                        }
                    }, TaskCreationOptions.LongRunning);
                }, 3, 30).ShowDialog() !=
                    DialogResult.Cancel)
                {
                    if (failed)
                    {
                        throw new Exception(Language.Profile_is_invalid_or_not_possible_to_apply);
                    }

                    return true;
                }

                return false;
            }
            finally
            {
                IPCService.GetInstance().Status = instanceStatus;
            }
        }

        private static void EditProfile(ProfileItem profile)
        {
            // Get the status of the thing
            IPCService.GetInstance().Status = InstanceStatus.User;
            // Load all the profiles from JSON
            //ProfileRepository.AllProfiles
            // Start up the DisplayProfileForm directly
            new DisplayProfileForm(profile).ShowDialog();
            // Then we close down as we're only here to edit one profile
            Application.Exit();
        }

        public static bool IsValidFilename(string testName)
        {
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(testName)) { return false; };

            return true;
        }

        public static string GetValidFilename(string uncheckedFilename)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                uncheckedFilename = uncheckedFilename.Replace(c.ToString(), "");
            }
            return uncheckedFilename;
        }
    }
}