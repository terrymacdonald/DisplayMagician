using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
//using System.CommandLine.DragonFruit;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HeliosDisplayManagement.InterProcess;
using HeliosDisplayManagement.Resources;
using HeliosDisplayManagement.Shared;
using HeliosDisplayManagement.Steam;
using HeliosDisplayManagement.UIForms;

namespace HeliosDisplayManagement
{
    internal static class Program
    {

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

        private static void CreateShortcut(IReadOnlyList<Profile> profiles, int profileIndex)
        {
            if (profileIndex < 0)
            {
                throw new Exception(Language.Selected_profile_is_invalid_or_not_found);
            }

            IPCService.GetInstance().Status = InstanceStatus.User;
            new ShortcutForm(profiles[profileIndex])
            {
                FileName = CommandLineOptions.ExecuteFilename,
                SteamAppId = CommandLineOptions.ExecuteSteamApp,
                Arguments = CommandLineOptions.ExecuteArguments,
                ProcessName = CommandLineOptions.ExecuteProcessName,
                Timeout = CommandLineOptions.ExecuteProcessTimeout
            }.ShowDialog();
        }

        private static void EditProfile(IList<Profile> profiles, int profileIndex)
        {
            if (profileIndex < 0)
            {
                throw new Exception(Language.Selected_profile_is_invalid_or_not_found);
            }

            IPCService.GetInstance().Status = InstanceStatus.User;
            var editForm = new EditForm(profiles[profileIndex]);

            if (editForm.ShowDialog() == DialogResult.OK)
            {
                profiles[profileIndex] = editForm.Profile;
            }

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
            var cmd = new RootCommand();
            cmd.Description = "This is an application and things.";
            cmd.Add(new Option<HeliosStartupAction>(
                aliases: new string[] {"--action", "-a"}, 
                getDefaultValue :() => HeliosStartupAction.None,
                description: "(required) The startup action to perform: None (Do nothing), SwitchProfile (Change to another profile and optionally run an application/game), CreateShortcut (Create a Desktop Shortcut), EditProfile (Edit a profile)"
                )
            );
            cmd.Add(new Option<string>(
                aliases: new string[] { "--profile-id", "-p" },
                description: "(required) UUID string that selects the profile to use."
                )
            );
            cmd.Add(new Option<string>(
                aliases: new string[] { "--arguments" },
                description: "(optional) Extra arguments to pass to the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut."
                )
            ); 
            cmd.Add(new Option<string>(
                aliases: new string[] { "--execute", "-e" },
                description: "(optional) The application/game to start when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut."
                )
            );
            cmd.Add(new Option<string>(
                aliases: new string[] { "--waitfor", "-w" },
                description: "(optional) The process name to wait for when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut."
                )
            );
            cmd.Add(new Option<uint>(
                aliases: new string[] { "--timeout", "-t" },
                description: "(optional) The time in seconds we should delay starting the application/game when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut."
                )
            );
            cmd.Add(new Option<uint>(
                aliases: new string[] { "--steam", "-s" },
                description: "(optional)The Steam AppID to run for when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut."
                )
            );
            /*cmd.Add(new Option<uint>(
                aliases: new string[] { "--uplay" },
                description: "(optional)The Uplay ID to run when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut."
                )
            );
            cmd.Add(new Option<uint>(
                aliases: new string[] { "--epic" },
                description: "(optional)The Epic ID to run when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut."
                )
            );
            cmd.Add(new Option<uint>(
                aliases: new string[] { "--origin" },
                description: "(optional)The Origin ID to run when we're temporarily switching profile and running the application/game. Also can be used when creating a shortcut."
                )
            );*/

            cmd.Handler = CommandHandler.Create<HeliosStartupAction, string, string, string, string, uint, uint>((action, profileId, arguments, execute, waitfor, timeout, steam) =>
            {

                // Validate combinations of Command line options
                // If a profileId is supplied then we need an action other than None
                if (action == HeliosStartupAction.None && !String.IsNullOrEmpty(profileId))
                {
                    Console.WriteLine("Error - If you supply a Profile ID or Name then you must also provide an Action.");
                    return 1;
                }

                //var help = new System.CommandLine.Help.HelpBuilder(invocationContext.Console);
                //Console.WriteLine(help);


                // Save these in CommandLineOptions for easy access from other parts of the application
                CommandLineOptions.Action = action;
                CommandLineOptions.ProfileId = profileId;
                CommandLineOptions.ExecuteArguments = arguments;
                CommandLineOptions.ExecuteFilename = execute;
                CommandLineOptions.ExecuteProcessName = waitfor;
                CommandLineOptions.ExecuteProcessTimeout = timeout;
                CommandLineOptions.ExecuteSteamApp = steam;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                try
                {
                    if (!IPCService.StartService())
                    {
                        throw new Exception(Language.Can_not_open_a_named_pipe_for_Inter_process_communication);
                    }

                    // Create an array of profiles
                    var profiles = Profile.GetAllProfiles().ToArray();
                    // Show the user the profiles if they want to look
                    foreach (Profile profile in profiles)
                    {
                        Console.WriteLine($"Found Profile: {profile.Name} (ID:{profile.Id})");
                    }
                    // Try and lookup the profileId in the profileIndex
                    var profileIndex = profiles.Length > 0 ? Array.FindIndex(profiles, p => p.Id.Equals(CommandLineOptions.ProfileId, StringComparison.InvariantCultureIgnoreCase)) : -1;
                    // If the profileID wasn't there, maybe they used the profile name?
                    if (profileIndex == -1)
                    {
                        profileIndex = profiles.Length > 0 ? Array.FindIndex(profiles, p => p.Name.Equals(CommandLineOptions.ProfileId, StringComparison.InvariantCultureIgnoreCase)) : -1;
                    }
                    Console.WriteLine($"Using Profile: {profiles[profileIndex].Name} (ID:{profiles[profileIndex].Id})");


                    switch (CommandLineOptions.Action)
                    {
                        case HeliosStartupAction.SwitchProfile:
                            SwitchProfile(profiles, profileIndex);

                            break;
                        case HeliosStartupAction.EditProfile:
                            EditProfile(profiles, profileIndex);

                            break;
                        case HeliosStartupAction.CreateShortcut:
                            CreateShortcut(profiles, profileIndex);

                            break;
                        default:
                            IPCService.GetInstance().Status = InstanceStatus.User;
                            Application.Run(new MainForm());

                            break;
                    }


                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        string.Format(Language.Operation_Failed, e.Message),
                        Language.Fatal_Error,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                return 0;
            });

            var result = cmd.InvokeAsync(args).Result;

            return result;


        }


        // ReSharper disable once CyclomaticComplexity
        private static void SwitchProfile(IReadOnlyList<Profile> profiles, int profileIndex)
        {
            var rollbackProfile = Profile.GetCurrent(string.Empty);

            if (profileIndex < 0)
            {
                throw new Exception(Language.Selected_profile_is_invalid_or_not_found);
            }

            if (!profiles[profileIndex].IsPossible)
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

            if (!string.IsNullOrWhiteSpace(CommandLineOptions.ExecuteFilename))
            {
                if (!File.Exists(CommandLineOptions.ExecuteFilename))
                {
                    throw new Exception(Language.Executable_file_not_found);
                }

                if (!GoProfile(profiles[profileIndex]))
                {
                    throw new Exception(Language.Can_not_change_active_profile);
                }

                var process = System.Diagnostics.Process.Start(CommandLineOptions.ExecuteFilename,
                    CommandLineOptions.ExecuteArguments);
                var processes = new System.Diagnostics.Process[0];

                if (!string.IsNullOrWhiteSpace(CommandLineOptions.ExecuteProcessName))
                {
                    var ticks = 0;

                    while (ticks < CommandLineOptions.ExecuteProcessTimeout * 1000)
                    {
                        processes = System.Diagnostics.Process.GetProcessesByName(CommandLineOptions.ExecuteProcessName);

                        if (processes.Length > 0)
                        {
                            break;
                        }

                        Thread.Sleep(300);
                        ticks += 300;
                    }
                }

                if (processes.Length == 0)
                {
                    processes = new[] {process};
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
            else if (CommandLineOptions.ExecuteSteamApp > 0)
            {
                var steamGame = new SteamGame(CommandLineOptions.ExecuteSteamApp);

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

                if (!GoProfile(profiles[profileIndex]))
                {
                    throw new Exception(Language.Can_not_change_active_profile);
                }

                var address = $"steam://rungameid/{steamGame.AppId}";

                if (!string.IsNullOrWhiteSpace(CommandLineOptions.ExecuteArguments))
                {
                    address += "/" + CommandLineOptions.ExecuteArguments;
                }

                var steamProcess = System.Diagnostics.Process.Start(address);
                // Wait for steam game to update and then run
                var ticks = 0;

                while (ticks < CommandLineOptions.ExecuteProcessTimeout * 1000)
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
            else
            {
                if (!GoProfile(profiles[profileIndex]))
                {
                    throw new Exception(Language.Can_not_change_active_profile);
                }
            }
        }
    }
}