using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
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
using HeliosDisplayManagement.Uplay;
using HeliosDisplayManagement.UIForms;
using System.Net.NetworkInformation;

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

        private static void CreateShortcut(IReadOnlyList<Profile> profiles, int profileIndex, CommandLineOptions options)
        {
            if (profileIndex < 0)
            {
                throw new Exception(Language.Selected_profile_is_invalid_or_not_found);
            }

            IPCService.GetInstance().Status = InstanceStatus.User;
            new ShortcutForm(profiles[profileIndex])
            {
                FileName = options.ExecuteFilename,
                SteamAppId = options.ExecuteSteamApp,
                Arguments = options.ExecuteArguments,
                ProcessName = options.ExecuteProcessName,
                Timeout = options.ExecuteProcessTimeout
            }.ShowDialog();
        }

        private static void EditProfile(IList<Profile> profiles, int profileIndex, CommandLineOptions options)
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

            /*return Parser.Default.ParseArguments<CommandLineOptions>(args).MapResult(
                options => RunOptions(options),
                _ => 1);*/
            //var parser = new CommandLine.Parser(with => with.HelpWriter = null);
            //var parserResult = parser.ParseArguments<CommandLineOptions, object>(args)
            var result = Parser.Default.ParseArguments<CommandLineOptions>(args)
                .MapResult(
                    options => RunOptions(options),
                    _ => 1);
            return result;
        }

        static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = "Myapp 2.0.0-beta"; //change header
                h.Copyright = "Copyright (c) 2019 Global.com"; //change copyright text
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
        }

        static int RunOptions(CommandLineOptions options)
        {
            // Validate combinations of Command line options
            // If a profileId is supplied then we need an action other than None
            if (options.Action == HeliosStartupAction.None && !String.IsNullOrEmpty(options.Profile))
            {
                Console.WriteLine("Error - If you supply a Profile ID or Name then you must also provide an Action.");
                return 1;
            }

            Console.WriteLine(CommandLine.Parser.Default.FormatCommandLine(options));

            if (options.Action != HeliosStartupAction.None && String.IsNullOrEmpty(options.Profile))
            {
                Console.WriteLine("Error - If you want to perform an Action then you must also provide a Profile ID or Name.");
                return 1;
            }

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
                foreach (Profile aprofile in profiles)
                {
                    Console.WriteLine($"Found Profile: {aprofile.Name} (ID:{aprofile.Id})");
                }
                // Try and lookup the profile in the profiles' ID fields
                var profileIndex = profiles.Length > 0 ? Array.FindIndex(profiles, p => p.Id.Equals(options.Profile, StringComparison.InvariantCultureIgnoreCase)) : -1;
                // If the profileID wasn't there, maybe they used the profile name?
                if (profileIndex == -1)
                {
                    // Try and lookup the profile in the profiles' Name fields
                    profileIndex = profiles.Length > 0 ? Array.FindIndex(profiles, p => p.Name.Equals(options.Profile, StringComparison.InvariantCultureIgnoreCase)) : -1;
                }
                // If the profileID still isn't there, then raise the alarm
                if (profileIndex == -1)
                {
                    Console.WriteLine($"Error - Couldn't find Profile Name or ID supplied via command line: \"{options.Profile}\". Please check the Profile Name or ID you supplied is correct.");
                    return 1;
                }
                Console.WriteLine($"Using Profile: {profiles[profileIndex].Name} (ID:{profiles[profileIndex].Id})");


                switch (options.Action)
                {
                    case HeliosStartupAction.SwitchProfile:
                        SwitchProfile(profiles, profileIndex, options);

                        break;
                    case HeliosStartupAction.EditProfile:
                        EditProfile(profiles, profileIndex, options);

                        break;
                    case HeliosStartupAction.CreateShortcut:
                        CreateShortcut(profiles, profileIndex, options);

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
        }
        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
        }

        // ReSharper disable once CyclomaticComplexity
        private static void SwitchProfile(IReadOnlyList<Profile> profiles, int profileIndex, CommandLineOptions options)
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

            if (!string.IsNullOrWhiteSpace(options.ExecuteFilename))
            {
                if (!File.Exists(options.ExecuteFilename))
                {
                    throw new Exception(Language.Executable_file_not_found);
                }

                if (!GoProfile(profiles[profileIndex]))
                {
                    throw new Exception(Language.Can_not_change_active_profile);
                }

                var process = System.Diagnostics.Process.Start(options.ExecuteFilename,
                    options.ExecuteArguments);
                var processes = new System.Diagnostics.Process[0];

                if (!string.IsNullOrWhiteSpace(options.ExecuteProcessName))
                {
                    var ticks = 0;

                    while (ticks < options.ExecuteProcessTimeout * 1000)
                    {
                        processes = System.Diagnostics.Process.GetProcessesByName(options.ExecuteProcessName);

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
            else if (options.ExecuteSteamApp > 0)
            {
                var steamGame = new SteamGame(options.ExecuteSteamApp);

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

                if (!string.IsNullOrWhiteSpace(options.ExecuteArguments))
                {
                    address += "/" + options.ExecuteArguments;
                }

                var steamProcess = System.Diagnostics.Process.Start(address);
                // Wait for steam game to update and then run
                var ticks = 0;

                while (ticks < options.ExecuteProcessTimeout * 1000)
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