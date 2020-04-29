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

namespace HeliosPlus {
    public enum SupportedGameLibrary
    {
        Unknown,
        Steam,
        Uplay
    }

    internal static class Program
    {

        internal static string ShortcutIconCachePath;

        internal static Profile GetProfile(string profileName)
        {
            // Create an array of display profiles we have
            var profiles = Profile.GetAllProfiles().ToArray();
            // Check if the user supplied a --profile option using the profiles' ID
            var profileIndex = profiles.Length > 0 ? Array.FindIndex(profiles, p => p.Id.Equals(profileName, StringComparison.InvariantCultureIgnoreCase)) : -1;
            // If the profileID wasn't there, maybe they used the profile name?
            if (profileIndex == -1)
            {
                // Try and lookup the profile in the profiles' Name fields
                profileIndex = profiles.Length > 0 ? Array.FindIndex(profiles, p => p.Name.StartsWith(profileName, StringComparison.InvariantCultureIgnoreCase)) : -1;
            }

            return profiles[profileIndex];
        }

        internal static bool GoProfile(Profile profile)
        {
            if (profile.IsActive)
            {
                return true;
            }

            var instanceStatus = IPCService.GetInstance().Status;

            try
            {
                IPCService.GetInstance().Status = InstanceStatus.Busy;
                var failed = false;

                if (new SplashForm(() =>
                    {
                        Task.Factory.StartNew(() =>
                        {
                            if (!profile.Apply())
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

        private static void CreateShortcutToExecutable(Profile profile, string executableToRun, string processToMonitor, uint timeout, string executableArguments)
        {

            IPCService.GetInstance().Status = InstanceStatus.User;
            new ShortcutForm(profile)
            {
                ExecutableNameAndPath = executableToRun,
                ExecutableArguments = executableArguments,
                ProcessNameToMonitor = processToMonitor,
                ExecutableTimeout = timeout
            }.ShowDialog();
        }

        private static void CreateShortcutToSteamGame(Profile profile, string steamGameIdToRun, uint timeout, string executableArguments)
        {

            IPCService.GetInstance().Status = InstanceStatus.User;
            new ShortcutForm(profile)
            {
                GameLibrary = SupportedGameLibrary.Steam,
                GameAppId = Convert.ToUInt32(steamGameIdToRun),
                GameTimeout = timeout,
                GameArguments = executableArguments,
            }.ShowDialog();
        }

        private static void CreateShortcutToUplayGame(Profile profile, string uplayGameIdToRun, uint timeout, string executableArguments)
        {

            IPCService.GetInstance().Status = InstanceStatus.User;
            new ShortcutForm(profile)
            {
                GameLibrary = SupportedGameLibrary.Uplay,
                GameAppId = Convert.ToUInt32(uplayGameIdToRun),
                GameTimeout = timeout,
                GameArguments = executableArguments,
            }.ShowDialog();
        }


        private static void EditProfile(Profile profile)
        {

            // Get the status of the 
            IPCService.GetInstance().Status = InstanceStatus.User;
            var editForm = new EditForm(profile);

            if (editForm.ShowDialog() == DialogResult.OK)
            {
                profile = editForm.Profile;
            }

            var profiles = Profile.GetAllProfiles().ToArray();
            if (!Profile.SetAllProfiles(profiles))
            {
                throw new Exception(Language.Failed_to_save_profile);
            }
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

            // Figure out where the shortcut's will go
            ShortcutIconCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Assembly.GetExecutingAssembly().GetName().Name, @"ShortcutIconCache");
            // Create the Shortcut Icon Cache if it doesn't exist so that it's avilable for all the program
            if (!Directory.Exists(ShortcutIconCachePath))
            {
                try
                {
                    Directory.CreateDirectory(ShortcutIconCachePath);
                }
                catch
                {
                }
            }

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
            app.Command("SwitchProfile", (switchProfileCmd) =>
            {
                //description and help text of the command.
                switchProfileCmd.Description = "Use this command to temporarily change profiles, and load your favourite game or application.";

                switchProfileCmd.OnExecute(() =>
                {
                    switchProfileCmd.ShowHelp();
                    return 1;
                });

                switchProfileCmd.Command("permanent", (switchProfilePermanentSubCmd) =>
                {
                    switchProfilePermanentSubCmd.Description = "Change to a different display profile permanently (until you manually switch back).";
                    var optionProfile = switchProfilePermanentSubCmd.Option("-p|--profile <PROFILENAME>", "(required) The Profile Name or Profile ID of the display profile to you want to use.", CommandOptionType.SingleValue).IsRequired();
                    optionProfile.Validators.Add(new ProfileMustExistValidator());
                    switchProfilePermanentSubCmd.OnExecute(() =>
                    {
                        Console.WriteLine($"Changing to display profile {optionProfile.Value()}.");

                        SwitchToProfile(GetProfile(optionProfile.Value()));
                        return 0;
                    });
                });


                switchProfileCmd.Command("exe", (switchProfileExecuteSubCmd) =>
                {
                    switchProfileExecuteSubCmd.Description = "Temporarily change to a different display profile, run an application or game executable, then change back.";
                    var argumentExecutable = switchProfileExecuteSubCmd.Argument("PATH_TO_EXE", "(required) The game exectuable file to run.").IsRequired();
                    argumentExecutable.Validators.Add(new FileArgumentMustExistValidator());
                    var optionProfile = switchProfileExecuteSubCmd.Option("-p|--profile <PROFILENAME>", "(required) The Profile Name or Profile ID of the display profile to you want to use.", CommandOptionType.SingleValue).IsRequired();
                    optionProfile.Validators.Add(new ProfileMustExistValidator());
                    var optionWaitFor = switchProfileExecuteSubCmd.Option("-w|--waitfor <PROCESSNAME>", "(optional) The application/game to start when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut. Cannot be used with --steam or --uplay options.", CommandOptionType.SingleValue);
                    optionWaitFor.Validators.Add(new FileOptionMustExistValidator());
                    var optionTimeout = switchProfileExecuteSubCmd.Option<uint>("-t|--timeout", "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game.", CommandOptionType.SingleValue);
                    var optionArguments = switchProfileExecuteSubCmd.Option("-a|--arguments", "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game.", CommandOptionType.SingleValue);
                    switchProfileExecuteSubCmd.OnExecute(() =>
                    {
                        Console.WriteLine($"Changing to display profile {optionProfile.Value()}, running executable {argumentExecutable.Value} then reverting back to this display profile when finished.");

                        SwitchToExecutable(
                            GetProfile(optionProfile.Value()),
                            //GetProfile(argProfile.Value),
                            argumentExecutable.Value,
                            optionWaitFor.Value(),
                            Convert.ToUInt32(optionTimeout.Value()),
                            optionArguments.Value()
                            );

                        return 0;
                    });
                });

                switchProfileCmd.Command("steam", (switchProfileSteamSubCmd) =>
                {
                    switchProfileSteamSubCmd.Description = "Change to a display profile and run a Steam game, then swap back.";
                    var argumentSteam = switchProfileSteamSubCmd.Argument("STEAM_GAME_ID", "(required) The Steam Game ID.").IsRequired();
                    argumentSteam.Validators.Add(new SteamArgumentMustExistValidator());
                    var optionProfile = switchProfileSteamSubCmd.Option("-p|--profile <PROFILENAME>", "(required) The Profile Name or Profile ID of the display profile to you want to use.", CommandOptionType.SingleValue).IsRequired();
                    optionProfile.Validators.Add(new ProfileMustExistValidator());
                    var optionTimeout = switchProfileSteamSubCmd.Option<uint>("-t|--timeout", "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game. ", CommandOptionType.SingleValue);
                    var optionArguments = switchProfileSteamSubCmd.Option("-a|--arguments", "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game.", CommandOptionType.SingleValue);
                    switchProfileSteamSubCmd.OnExecute(() =>
                    {
                        Console.WriteLine($"Changing to display profile {optionProfile.Value()}, running Steam Game ID:{argumentSteam.Value} then reverting back to this display profile when finished.");

                        SwitchToSteamGame(
                            GetProfile(optionProfile.Value()),
                            //GetProfile(argProfile.Value),
                            argumentSteam.Value,
                            Convert.ToUInt32(optionTimeout.Value()),
                            optionArguments.Value()
                            );

                        return 0;
                    });
                });

                switchProfileCmd.Command("uplay", (switchProfileUplaySubCmd) =>
                {
                    switchProfileUplaySubCmd.Description = "Change to a display profile and run a Uplay game.";
                    var argumentUplay = switchProfileUplaySubCmd.Argument("UPLAY_GAME_ID", "(required) The Uplay Game ID to run for when we're temporarily switching profile and running the Uplay application/game.").IsRequired();
                    argumentUplay.Validators.Add(new UplayArgumentMustExistValidator());
                    var optionProfile = switchProfileUplaySubCmd.Option("-p|--profile <PROFILENAME>", "(required) The Profile Name or Profile ID of the display profile to you want to use.", CommandOptionType.SingleValue).IsRequired();
                    optionProfile.Validators.Add(new ProfileMustExistValidator());
                    var optionTimeout = switchProfileUplaySubCmd.Option<uint>("-t|--timeout", "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game.", CommandOptionType.SingleValue);
                    var optionArguments = switchProfileUplaySubCmd.Option("-a|--arguments", "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game.", CommandOptionType.SingleValue);
                    switchProfileUplaySubCmd.OnExecute(() =>
                    {
                        Console.WriteLine($"Changing to display profile {optionProfile.Value()}, running Uplay Game ID:{argumentUplay.Value} then reverting back to this display profile when finished.");

                        SwitchToUplayGame(
                            GetProfile(optionProfile.Value()),
                            //GetProfile(argProfile.Value),
                            argumentUplay.Value,
                            Convert.ToUInt32(optionTimeout.Value()),
                            optionArguments.Value()
                            );

                        return 0;
                    });
                });

            });


            /*// This is the CreateShortcut command
            app.Command("CreateShortcut", (createShortcutCmd) =>
            {
                //description and help text of the command.
                createShortcutCmd.Description = "Use this command to create a new shortcut to your favourite game.";
                //createShortcutCmd.ExtendedHelpText = "Use this command to create a new shortcut to your favourite game.";

                var optionProfile = createShortcutCmd.Option("-p|--profile", "The Profile Name or Profile ID of the profile to you want to use.", CommandOptionType.SingleValue).IsRequired();
                optionProfile.Validators.Add(new ProfileMustExistValidator());

                createShortcutCmd.OnExecute(() =>
                {
                    createShortcutCmd.ShowHelp();
                    return 1;
                });

                createShortcutCmd.Command("exe", (createShortcutExecutableSubCmd) =>
                {
                    createShortcutExecutableSubCmd.Description = "Create a shortcut to run a Game executable.";
                    var argumentExecutable = createShortcutExecutableSubCmd.Argument("executabletorun", "The game exectuable file to run.").IsRequired();
                    argumentExecutable.Validators.Add(new FileArgumentMustExistValidator());
                    var optionWaitFor = createShortcutExecutableSubCmd.Option("-w|--waitfor <PROCESSNAME>", "(optional) The application/game to start when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut. Cannot be used with --steam or --uplay options.", CommandOptionType.SingleValue);
                    optionWaitFor.Validators.Add(new FileOptionMustExistValidator());
                    var optionTimeout = createShortcutExecutableSubCmd.Option<uint>("-t|--timeout", "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    var optionArguments = createShortcutExecutableSubCmd.Option("-a|--arguments", "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    createShortcutExecutableSubCmd.OnExecute(() =>
                    {
                        Console.WriteLine($"Creating a Desktop Shortcut to the application or game {argumentExecutable.Value}");

                        CreateShortcutToExecutable(
                            GetProfile(optionProfile.Value()),
                            argumentExecutable.Value,
                            optionWaitFor.Value(),
                            Convert.ToUInt32(optionTimeout.Value()),
                            optionArguments.Value()
                            );

                        return 0;
                    });
                });

                createShortcutCmd.Command("steam", (createShortcutSteamSubCmd) =>
                {
                    createShortcutSteamSubCmd.Description = "Create a Steam Game shortcut.";
                    var argumentSteam = createShortcutSteamSubCmd.Argument("steamgameid", "The Steam Game ID.").IsRequired();
                    argumentSteam.Validators.Add(new SteamArgumentMustExistValidator());
                    var optionTimeout = createShortcutSteamSubCmd.Option<uint>("-t|--timeout", "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    var optionArguments = createShortcutSteamSubCmd.Option("-a|--arguments", "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    createShortcutSteamSubCmd.OnExecute(() =>
                    {
                        Console.WriteLine($"Creating a Desktop Shortcut to the Steam Game {argumentSteam.Value}");

                        CreateShortcutToSteamGame(
                            GetProfile(optionProfile.Value()),
                            argumentSteam.Value,
                            Convert.ToUInt32(optionTimeout.Value()),
                            optionArguments.Value()
                            );

                        return 0;
                    });
                });

                createShortcutCmd.Command("uplay", (createShortcutUplaySubCmd) =>
                {
                    createShortcutUplaySubCmd.Description = "Create a Uplay Game shortcut.";
                    var argumentUplay = createShortcutUplaySubCmd.Argument("uplaygameid", "The Uplay Game ID to run for when we're temporarily switching profile and running the Uplay application/game.").IsRequired();
                    argumentUplay.Validators.Add(new UplayArgumentMustExistValidator());
                    var optionTimeout = createShortcutUplaySubCmd.Option<uint>("-t|--timeout", "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    var optionArguments = createShortcutUplaySubCmd.Option("-a|--arguments", "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    createShortcutUplaySubCmd.OnExecute(() =>
                    {
                        Console.WriteLine($"Creating a Desktop Shortcut to the Uplay Game {argumentUplay.Value}");

                        CreateShortcutToUplayGame(
                            GetProfile(optionProfile.Value()),
                            argumentUplay.Value,
                            Convert.ToUInt32(optionTimeout.Value()),
                            optionArguments.Value()
                            );

                        return 0;
                    });
                });

            });*/

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
                // You'll always want to catch this exception, otherwise it will generate a messy and confusing error for the end user.
                // the message will usually be something like:
                // "Unrecognized command or argument '<invalid-command>'"
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
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


                IPCService.GetInstance().Status = InstanceStatus.User;
                Application.Run(new MainForm());

            }
            catch (Exception e)
            {
                MessageBox.Show(
                    string.Format(Language.Operation_Failed, e.Message),
                    Language.Fatal_Error,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            
        }



        private static void SwitchToExecutable(Profile profile, string executableToRun, string processToMonitor, uint timeout, string executableArguments)
        {
            var rollbackProfile = Profile.GetCurrent(string.Empty);

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
                    Icon = Properties.Resources.Icon,
                    Text = string.Format(
                        Language.Waiting_for_the_0_to_terminate,
                        processes[0].ProcessName),
                    Visible = true
                };
                Application.DoEvents();
            }
            catch
            {
                // ignored
            }

            foreach (var p in processes)
            {
                try
                {
                    p.WaitForExit();
                }
                catch
                {
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

            if (!rollbackProfile.IsActive)
            {
                if (!GoProfile(rollbackProfile))
                {
                    throw new Exception(Language.Can_not_change_active_profile);
                }
            }
        }



        private static void SwitchToSteamGame(Profile profile, string steamGameIdToRun, uint timeout, string steamGameArguments)
        {

            // Convert the steamGameIdToRun string to a uint for Steam Games
            uint steamGameIdUint = 0;
            if (!uint.TryParse(steamGameIdToRun, out steamGameIdUint))
            {
                throw new Exception("ERROR - Couldn't convert the string steamGameIdToRun parameter to steamGameIdUint in SwitchToSteamGame!");
            }

            // Save the profile we're on now
            var rollbackProfile = Profile.GetCurrent(string.Empty);

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
                    Icon = Properties.Resources.Icon,
                    Text = string.Format(
                        Language.Waiting_for_the_0_to_terminate,
                        steamGameToRun.GameName),
                    Visible = true
                };
                Application.DoEvents();
            }
            catch
            {
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

            if (!rollbackProfile.IsActive)
            {
                if (!GoProfile(rollbackProfile))
                {
                    throw new Exception(Language.Can_not_change_active_profile);
                }
            }

        }

        private static void SwitchToUplayGame(Profile profile, string uplayGameIdToRun, uint timeout, string uplayGameArguments)
        {

            var rollbackProfile = Profile.GetCurrent(string.Empty);

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

            if (!rollbackProfile.IsActive)
            {
                if (!GoProfile(rollbackProfile))
                {
                    throw new Exception(Language.Can_not_change_active_profile);
                }
            }*/

        }


        // ReSharper disable once CyclomaticComplexity
        private static void SwitchToProfile(Profile profile)
        {
            var rollbackProfile = Profile.GetCurrent(string.Empty);

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
    }
}