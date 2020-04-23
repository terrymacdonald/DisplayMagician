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
using HeliosPlus.Steam;
using HeliosPlus.Shared;
using HeliosPlus.Uplay;
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

            var app = new CommandLineApplication();

            //app.Name = "HeliosDM+";
            //app.Name = Assembly.GetEntryAssembly().GetName().Name;
            app.Description = "This application helps configure your NVIDIA Videocard for multiple displays.";
            app.ExtendedHelpText = "This application helps configure your NVIDIA Videocard for multiple displays. It has some nifty features such as the "
                + Environment.NewLine + " ability to temporarily change your screen settings while you are playing a game, and then change them back once finished.";

            app.GetFullNameAndVersion();
            app.MakeSuggestionsInErrorMessage = true;
            app.HelpOption("-?|-h|--help");

            app.VersionOption("-v|--version", () => {
                return string.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version);
            });

            var optionProfile = app.Option("-p|--profile <PROFILE>", "The Profile Name or Profile ID of the profile to you want to use.", CommandOptionType.SingleValue);
            optionProfile.Validators.Add(new ProfileMustExistValidator());

            // This is the SwitchProfile command
            app.Command("SwitchProfile", (command) =>
            {
                //description and help text of the command.
                command.Description = "Use this command to temporarily change profiles, and load your favourite game or application.";
                command.ExtendedHelpText = "Use this command to create a new shortcut to your favourite game.";
                command.HelpOption("-?|-h|--help");

                command.OnExecute(() =>
                {

                    command.ShowHelp();

                    Console.WriteLine("Specify a subcommand");
                    SwitchToProfile(
                        GetProfile(optionProfile.Value())
                    );


                    //command.ShowHelp();
                    return 1;
                });

                command.Command("execute", (subcommand) =>
                {
                    subcommand.Description = "Change to a display profile and run an application or game executable.";
                    var argumentExecutable = subcommand.Argument("executabletorun", "The game exectuable file to run.").IsRequired();
                    argumentExecutable.Validators.Add(new FileArgumentMustExistValidator());
                    var optionWaitFor = subcommand.Option("-w|--waitfor <PROCESSNAME>", "(optional) The application/game to start when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut. Cannot be used with --steam or --uplay options.", CommandOptionType.SingleValue);
                    optionWaitFor.Validators.Add(new FileOptionMustExistValidator());
                    var optionTimeout = subcommand.Option<uint>("-t|--timeout", "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    var optionArguments = subcommand.Option("-a|--arguments", "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    subcommand.OnExecute(() =>
                    {
                        Console.WriteLine($"Starting executable {argumentExecutable.Value}");

                        SwitchToExecutable(
                            GetProfile(optionProfile.Value()),
                            argumentExecutable.Value,
                            optionWaitFor.Value(),
                            Convert.ToUInt32(optionTimeout.Value()),
                            optionArguments.Value()
                            );

                        return 0;
                    });
                });

                command.Command("steam", (subcommand) =>
                {
                    subcommand.Description = "Change to a display profile and run a Steam game.";
                    var argumentSteam = subcommand.Argument("steamgameid", "The Steam Game ID.").IsRequired();
                    argumentSteam.Validators.Add(new SteamArgumentMustExistValidator());
                    var optionTimeout = subcommand.Option<uint>("-t|--timeout", "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    var optionArguments = subcommand.Option("-a|--arguments", "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    subcommand.OnExecute(() =>
                    {
                        Console.WriteLine($"Starting Steam Game {argumentSteam.Value}");

                        SwitchToSteamGame(
                            GetProfile(optionProfile.Value()),
                            argumentSteam.Value,
                            Convert.ToUInt32(optionTimeout.Value()),
                            optionArguments.Value()
                            );

                        return 0;
                    });
                });

                command.Command("uplay", (subcommand) =>
                {
                    subcommand.Description = "Change to a display profile and run a Uplay game.";
                    var argumentUplay = subcommand.Argument("uplaygameid", "The Uplay Game ID to run for when we're temporarily switching profile and running the Uplay application/game.").IsRequired();
                    argumentUplay.Validators.Add(new UplayArgumentMustExistValidator()); 
                    var optionTimeout = subcommand.Option<uint>("-t|--timeout", "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    var optionArguments = subcommand.Option("-a|--arguments", "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    subcommand.OnExecute(() =>
                    {
                        Console.WriteLine($"Starting Uplay Game {argumentUplay.Value}");

                        SwitchToUplayGame(
                            GetProfile(optionProfile.Value()),
                            argumentUplay.Value,
                            Convert.ToUInt32(optionTimeout.Value()),
                            optionArguments.Value()
                            );

                        return 0;
                    });
                });

            });


            // This is the CreateShortcut command
            app.Command("CreateShortcut", (command) =>
            {
                //description and help text of the command.
                command.Description = "Use this command to create a new shortcut to your favourite game.";
                command.ExtendedHelpText = "Use this command to create a new shortcut to your favourite game.";
                command.HelpOption("-?|-h|--help");

                command.OnExecute(() =>
                {
                    Console.WriteLine("Specify a subcommand");
                    command.ShowHelp();
                    return 1;
                });

                command.Command("execute", (subcommand) =>
                {
                    subcommand.Description = "Create a shortcut to run a Game executable.";
                    var argumentExecutable = subcommand.Argument("executabletorun", "The game exectuable file to run.").IsRequired();
                    argumentExecutable.Validators.Add(new FileArgumentMustExistValidator());
                    var optionWaitFor = subcommand.Option("-w|--waitfor <PROCESSNAME>", "(optional) The application/game to start when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut. Cannot be used with --steam or --uplay options.", CommandOptionType.SingleValue);
                    optionWaitFor.Validators.Add(new FileOptionMustExistValidator());
                    var optionTimeout = subcommand.Option<uint>("-t|--timeout", "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    var optionArguments = subcommand.Option("-a|--arguments", "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    subcommand.OnExecute(() =>
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

                command.Command("steam", (subcommand) =>
                {
                    subcommand.Description = "Create a Steam Game shortcut.";
                    var argumentSteam = subcommand.Argument("steamgameid", "The Steam Game ID.").IsRequired();
                    argumentSteam.Validators.Add(new SteamArgumentMustExistValidator());
                    var optionTimeout = subcommand.Option<uint>("-t|--timeout", "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    var optionArguments = subcommand.Option("-a|--arguments", "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    subcommand.OnExecute(() =>
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

                command.Command("uplay", (subcommand) =>
                {
                    subcommand.Description = "Create a Uplay Game shortcut.";
                    var argumentUplay = subcommand.Argument("uplaygameid", "The Uplay Game ID to run for when we're temporarily switching profile and running the Uplay application/game.").IsRequired();
                    argumentUplay.Validators.Add(new UplayArgumentMustExistValidator());
                    var optionTimeout = subcommand.Option<uint>("-t|--timeout", "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    var optionArguments = subcommand.Option("-a|--arguments", "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut.", CommandOptionType.SingleValue);
                    subcommand.OnExecute(() =>
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

            });

            // This is the EditProfile command
            app.Command("EditProfile", (command) =>
            {
                //description and help text of the command.
                command.Description = "Use this command to edit a HeliosDMPlus profile.";
                command.ExtendedHelpText = "Use this command to edit a HeliosDMPlus profile.";
                command.HelpOption("-?|-h|--help");

                command.OnExecute(() =>
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




            var steamGame = new SteamGame(Convert.ToUInt32(steamGameIdToRun));

            if (!SteamGame.SteamInstalled)
            {
                throw new Exception(Language.Steam_is_not_installed);
            }

            if (!File.Exists(SteamGame.SteamAddress))
            {
                throw new Exception(Language.Steam_executable_file_not_found);
            }

            if (!steamGame.IsInstalled)
            {
                throw new Exception(Language.Steam_game_is_not_installed);
            }

            if (!steamGame.IsOwned)
            {
                throw new Exception(Language.Steam_game_is_not_owned);
            }

            if (!GoProfile(profile))
            {
                throw new Exception(Language.Can_not_change_active_profile);
            }

            var address = $"steam://rungameid/{steamGame.AppId}";

            if (!string.IsNullOrWhiteSpace(steamGameArguments))
            {
                address += "/" + steamGameArguments;
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




            var steamGame = new SteamGame(Convert.ToUInt32(uplayGameIdToRun));

            if (!SteamGame.SteamInstalled)
            {
                throw new Exception(Language.Steam_is_not_installed);
            }

            if (!File.Exists(SteamGame.SteamAddress))
            {
                throw new Exception(Language.Steam_executable_file_not_found);
            }

            if (!steamGame.IsInstalled)
            {
                throw new Exception(Language.Steam_game_is_not_installed);
            }

            if (!steamGame.IsOwned)
            {
                throw new Exception(Language.Steam_game_is_not_owned);
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
            }

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