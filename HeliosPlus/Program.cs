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


        internal static ProfileItem GetProfile(string profileName)
        {
            // Create an array of display profiles we have
            var profiles = ProfileRepository.AllProfiles.ToArray();
            // Check if the user supplied a --profile option using the profiles' ID
            var profileIndex = profiles.Length > 0 ? Array.FindIndex(profiles, p => p.UUID.Equals(profileName, StringComparison.InvariantCultureIgnoreCase)) : -1;
            // If the profileID wasn't there, maybe they used the profile name?
            if (profileIndex == -1)
            {
                // Try and lookup the profile in the profiles' Name fields
                profileIndex = profiles.Length > 0 ? Array.FindIndex(profiles, p => p.Name.StartsWith(profileName, StringComparison.InvariantCultureIgnoreCase)) : -1;
            }

            return profiles[profileIndex];
        }

        internal static bool GoProfile(ProfileItem profile)
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
            Console.WriteLine(@"Copyright © Terry MacDonald 2020-{DateTime.Today.Year}");
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

            // This is the SwitchProfile command
            app.Command("RunShortcut", (switchProfileCmd) =>
            {
                var argumentShortcut = switchProfileCmd.Argument("\"SHORTCUT_NAME\"", "(required) The name of the shortcut to run from those stored in the shortcut library.").IsRequired();
                argumentShortcut.Validators.Add(new ShortcutMustExistValidator());

                //description and help text of the command.
                switchProfileCmd.Description = "Use this command to temporarily change profiles, and load your favourite game or application.";

                switchProfileCmd.OnExecute(() =>
                {
                    Console.WriteLine($"Editing profile {argumentShortcut.Value}");

                    SwitchToProfile(GetProfile(argumentShortcut.Value));

                    return 0;
                });
            });


            // This is the EditProfile command
            app.Command("EditProfile", (editProfileCmd) =>
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

            });

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

        private static void SwitchToExecutable(ProfileItem profile, string executableToRun, string processToMonitor, uint timeout, string executableArguments)
        {
            var rollbackProfile = ProfileRepository.CurrentProfile;

            if (!profile.IsPossible)
            {
                throw new Exception(Language.Selected_profile_is_not_possible);
            }

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

            if (!GoProfile(profile))
            {
                throw new Exception(Language.Can_not_change_active_profile);
            }

            var process = System.Diagnostics.Process.Start(executableToRun, executableArguments);
            var processes = new System.Diagnostics.Process[0];

            var ticks = 0;

            while (ticks < timeout * 1000)
            {

                processes = System.Diagnostics.Process.GetProcessesByName(processToMonitor);

                if (processes.Length > 0)
                {
                    break;
                }

                Thread.Sleep(300);
                ticks += 300;
            }


            if (processes.Length == 0)
            {
                processes = new[] { process };
            }

            IPCService.GetInstance().HoldProcessId = processes.FirstOrDefault()?.Id ?? 0;
            IPCService.GetInstance().Status = InstanceStatus.OnHold;
            NotifyIcon notify = null;

            try
            {
                notify = new NotifyIcon
                {
                    Icon = Properties.Resources.HeliosPlus,
                    Text = string.Format(
                        Language.Waiting_for_the_0_to_terminate,
                        processes[0].ProcessName),
                    Visible = true
                };
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/SwitchToExecutable exception: {ex.Message}: {ex.InnerException}");
                // ignored
            }

            foreach (var p in processes)
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

            if (notify != null)
            {
                notify.Visible = false;
                notify.Dispose();
                Application.DoEvents();
            }

            IPCService.GetInstance().Status = InstanceStatus.Busy;

            // Change back to the original profile if it is different
            if (!ProfileRepository.IsActiveProfile(rollbackProfile))
            {
                if (!GoProfile(rollbackProfile))
                {
                    throw new Exception(Language.Can_not_change_active_profile);
                }
            }
        }



        private static void SwitchToSteamGame(ProfileItem profile, string steamGameIdToRun, uint timeout, string steamGameArguments)
        {

            // Convert the steamGameIdToRun string to a uint for Steam Games
            uint steamGameIdUint = 0;
            if (!uint.TryParse(steamGameIdToRun, out steamGameIdUint))
            {
                throw new Exception("ERROR - Couldn't convert the string steamGameIdToRun parameter to steamGameIdUint in SwitchToSteamGame!");
            }

            // Save the profile we're on now
            var rollbackProfile = ProfileRepository.CurrentProfile;

            // Check that the profile we've been asked to change to will actually work
            if (!profile.IsPossible)
            {
                throw new Exception(Language.Selected_profile_is_not_possible);
            }

            // 
            if ( IPCClient.QueryAll().Any(
                        client =>
                            client.Status == InstanceStatus.Busy ||
                            client.Status == InstanceStatus.OnHold))
            {
                throw new Exception(
                    Language
                        .Another_instance_of_this_program_is_in_working_state_Please_close_other_instances_before_trying_to_switch_profile);
            }

            // Create the SteamGame objects so we can use them shortly
            // Get the game information relevant to the game we're switching to
            List<SteamGame> allSteamGames = SteamGame.GetAllInstalledGames();

            // Check if Steam is installed and error if it isn't
            if (!SteamGame.SteamInstalled)
            {
                throw new Exception(Language.Steam_is_not_installed);
            }

            // Otherwise try to find the game we've been asked to run
            SteamGame steamGameToRun = null;
            foreach (SteamGame steamGameToCheck in allSteamGames)
            {
                if (steamGameToCheck.GameId == steamGameIdUint)
                {
                    steamGameToRun = steamGameToCheck;
                    break;
                }
                    
            }

            // Attempt to change to a different profile if it's needed
            if (!GoProfile(profile))
            {
                throw new Exception(Language.Can_not_change_active_profile);
            }

            // Prepare to start the steam game using the URI interface 
            // as used by Steam for it's own desktop shortcuts.
            var address = $"steam://rungameid/{steamGameToRun.GameId}";
            if (!string.IsNullOrWhiteSpace(steamGameArguments))
            {
                address += "/" + steamGameArguments;
            }


            var steamProcess = System.Diagnostics.Process.Start(address);
            // Wait for steam game to update and then run
            var ticks = 0;

            while (ticks < timeout * 1000)
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

            IPCService.GetInstance().HoldProcessId = steamProcess?.Id ?? 0;
            IPCService.GetInstance().Status = InstanceStatus.OnHold;
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

            if (notify != null)
            {
                notify.Visible = false;
                notify.Dispose();
                Application.DoEvents();
            }

            IPCService.GetInstance().Status = InstanceStatus.Busy;

            // Change back to the original profile if it is different
            if (!ProfileRepository.IsActiveProfile(rollbackProfile))
            {
                if (!GoProfile(rollbackProfile))
                {
                    throw new Exception(Language.Can_not_change_active_profile);
                }
            }

        }

        private static void SwitchToUplayGame(ProfileItem profile, string uplayGameIdToRun, uint timeout, string uplayGameArguments)
        {

            var rollbackProfile = ProfileRepository.CurrentProfile;

            if (!profile.IsPossible)
            {
                throw new Exception(Language.Selected_profile_is_not_possible);
            }

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




            /*var steamGame = new SteamGame(Convert.ToUInt32(uplayGameIdToRun));

            if (!SteamGame.SteamInstalled)
            {
                throw new Exception(Language.Steam_is_not_installed);
            }

            if (!File.Exists(SteamGame.SteamExe))
            {
                throw new Exception(Language.Steam_executable_file_not_found);
            }

            if (!steamGame.IsInstalled)
            {
                throw new Exception(Language.Steam_game_is_not_installed);
            }

            if (!GoProfile(profile))
            {
                throw new Exception(Language.Can_not_change_active_profile);
            }

            var address = $"uplay://rungameid/{steamGame.AppId}";

            if (!string.IsNullOrWhiteSpace(uplayGameArguments))
            {
                address += "/" + uplayGameArguments;
            }

            var steamProcess = System.Diagnostics.Process.Start(address);
            // Wait for steam game to update and then run
            var ticks = 0;

            while (ticks < timeout * 1000)
            {
                if (steamGame.IsRunning)
                {
                    break;
                }

                Thread.Sleep(300);

                if (!steamGame.IsUpdating)
                {
                    ticks += 300;
                }
            }

            IPCService.GetInstance().HoldProcessId = steamProcess?.Id ?? 0;
            IPCService.GetInstance().Status = InstanceStatus.OnHold;
            NotifyIcon notify = null;

            try
            {
                notify = new NotifyIcon
                {
                    Icon = Properties.Resources.Icon,
                    Text = string.Format(
                        Language.Waiting_for_the_0_to_terminate,
                        steamGame.Name),
                    Visible = true
                };
                Application.DoEvents();
            }
            catch
            {
                // ignored
            }

            // Wait for the game to exit
            if (steamGame.IsRunning)
            {
                while (true)
                {
                    if (!steamGame.IsRunning)
                    {
                        break;
                    }

                    Thread.Sleep(300);
                }
            }

            if (notify != null)
            {
                notify.Visible = false;
                notify.Dispose();
                Application.DoEvents();
            }

            IPCService.GetInstance().Status = InstanceStatus.Busy;

            // Change back to the original profile if it is different
            if (!ProfileRepository.IsActiveProfile(rollbackProfile))
            {
                if (!GoProfile(rollbackProfile))
                {
                    throw new Exception(Language.Can_not_change_active_profile);
                }
            }*/

        }


        // ReSharper disable once CyclomaticComplexity
        private static void SwitchToProfile(ProfileItem profile)
        {
            var rollbackProfile = ProfileRepository.CurrentProfile;

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

            if (!GoProfile(profile))
            {
                throw new Exception(Language.Can_not_change_active_profile);
            }
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