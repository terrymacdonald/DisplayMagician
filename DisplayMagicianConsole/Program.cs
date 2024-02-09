using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagicianShared;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;
using static System.Net.Mime.MediaTypeNames;

namespace DisplayMagicianConsole
{
    internal class Program
    {

        public enum ERRORLEVEL : int
        {
            OK = 0, // Errorlevel returned when everything has worked as it should
            CANCELED_BY_USER = 1,  // Errorlevel returned when an action was cancelled by a user           
            PROFILE_UNKNOWN = 50, // Errorlevel used in CurrentProfile to return the fact the current display profile is not a saved profile, and so is unknown.
            ERROR_EXCEPTION = 100,  // Errorlevel returned when an excption of some kind has occurred.
            ERROR_CANNOT_FIND_SHORTCUT = 101,  // Errorlevel returned when RunShortcut command is used, and it cannot find the shortcut to run
            ERROR_CANNOT_FIND_PROFILE = 102,  // Errorlevel returned when RunProfile command is used, and it cannot find the profile to apply
            ERROR_APPLYING_PROFILE = 103,  // Errorlevel returned when RunProfile command is used, and it cannot apply the profile for some reason
            ERROR_UNKNOWN_COMMAND = 104, // Errorlevel returned when DisplayMagician is given an unregonised command
        };

        public static CancellationTokenSource AppCancellationTokenSource = new CancellationTokenSource();
        //Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        public static SemaphoreSlim AppBackgroundTaskSemaphoreSlim = new SemaphoreSlim(1, 1);

        static int Main(string[] args)
        {

            var app = new CommandLineApplication
            {
                AllowArgumentSeparator = true,
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
            };

            app.Description = "DisplayMagician is an open source tool for automatically configuring your displays and sound for a game or application from a single Windows Shortcut. DisplayMagicianConsole is the command line component of DisplayMagician.";
            app.ExtendedHelpText = "DisplayMagicianConsole allows you to view your current Display Profile or change your Display Profile from a command line.";

            app.GetFullNameAndVersion();
            app.MakeSuggestionsInErrorMessage = true;
            app.HelpOption("-?|-h|--help", inherited: true);

            app.VersionOption("-v|--version", () => {
                return string.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version);
            });

            CommandOption debug = app.Option("--debug", "Generate a DisplayMagician.log debug-level log file", CommandOptionType.NoValue);
            CommandOption trace = app.Option("--trace", "Generate a DisplayMagician.log trace-level log file", CommandOptionType.NoValue);
            //CommandOption forcedVideoLibrary = app.Option("--force-video-library", "Bypass the normal video detection logic to force a particular video library (AMD, NVIDIA, Windows)", CommandOptionType.SingleValue);

            // This is the ChangeProfile command
            app.Command(DisplayMagicianStartupAction.ChangeProfile.ToString(), (runProfileCmd) =>
            {
                //Console.WriteLine($"Program/Main: Processing the {runProfileCmd} command...");

                var argumentProfile = runProfileCmd.Argument("\"Profile_UUID\"", "(required) The UUID of the profile to run from those stored in the profile file.").IsRequired();
                argumentProfile.Validators.Add(new ProfileMustExistValidator());

                //description and help text of the command.
                runProfileCmd.Description = "Use this command to change to a display profile of your choosing.";

                runProfileCmd.OnExecute(() =>
                {
                    Console.WriteLine($"Program/Main: ChangeProfile commandline command was invoked!");

                    try
                    {
                        ERRORLEVEL errLevel = RunProfile(argumentProfile.Value);
                        return (int)errLevel;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Program/Main exception running ApplyProfile(profileToUse)");
                        return (int)ERRORLEVEL.ERROR_EXCEPTION;
                    }
                });
            });

            // This is the CurrentProfile command
            // This will output the current display profile if one matches, or 'Unknown'
            app.Command(DisplayMagicianStartupAction.CurrentProfile.ToString(), (currentProfileCmd) =>
            {
                //Console.WriteLine($"Program/Main: Processing the {currentProfileCmd} command...");

                ProfileRepository.InitialiseRepository();
                Console.WriteLine($"Program/Main: Leaving DisplayMagician to detect the best Video Library to use.");

                //description and help text of the command.
                currentProfileCmd.Description = "Use this command to output the name of the display profile currently in use. It will return 'UNKNOWN' if the display profile doesn't match any saved display profiles";

                currentProfileCmd.OnExecute(() =>
                {
                    Console.WriteLine($"Program/Main: CurrentProfile commandline command was invoked!");
                    ERRORLEVEL errLevel = CurrentProfile();
                    return (int)errLevel;
                });
            });

            app.OnExecute(() =>
            {
                Console.WriteLine($"Program/Main: Starting the app normally as there was no command supplied...");

                // Update the Active Profile before we load the Main Form
                ProfileRepository.InitialiseRepository();

                Console.WriteLine($"Program/Main: Showing the CurrentProfile command by default!");
                ERRORLEVEL errLevel = CurrentProfile();
                return (int)errLevel;
            });

            // Starting the actual commandline parsing app
            try
            {
                Console.WriteLine($"Invoking commandline processing");
                // This begins the actual execution of the application
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine($"Didn't recognise the supplied commandline options: - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return (int)ERRORLEVEL.ERROR_UNKNOWN_COMMAND;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Program/Main commandParsingException: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // You'll always want to catch this exception, otherwise it will generate a messy and confusing error for the end user.
                // the message will usually be something like:
                // "Unrecognized command or argument '<invalid-command>'"
                Console.WriteLine($"Program/Main exception: Unable to execute application - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return (int)ERRORLEVEL.ERROR_EXCEPTION;
            }

            return (int)ERRORLEVEL.OK;
        }

        public static ERRORLEVEL CurrentProfile()
        {
            Console.WriteLine($"Program/CurrentProfile: Finding the current profile in use");

            // Lookup the profile
            ProfileItem currentProfile;
            string profileName = "UNKNOWN";
            ERRORLEVEL errLevel = ERRORLEVEL.OK;
            try
            {
                ProfileRepository.UpdateActiveProfile();
                currentProfile = ProfileRepository.GetActiveProfile();
                if (currentProfile is ProfileItem)
                {
                    profileName = currentProfile.Name;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/CurrentProfile: Exception while trying to get the name of the DisplayMagician profile currently in use.");
                errLevel = ERRORLEVEL.ERROR_EXCEPTION;
            }

            Console.WriteLine($"Display Profile in use: {profileName}");

            return errLevel;
        }


        public static ERRORLEVEL RunProfile(string profileName)
        {
            //Console.WriteLine($"Program/RunProfile: Running profile {profileName}");
            ERRORLEVEL errLevel = ERRORLEVEL.OK;

            if (ProfileRepository.AllProfiles.Where(p => p.UUID.Equals(profileName)).Any())
            {
                Console.WriteLine($"Program/RunProfile: Found profile called {profileName} and now starting to apply the profile");

                // Get the profile
                ProfileItem profileToUse = ProfileRepository.AllProfiles.Where(p => p.UUID.Equals(profileName)).First();

                ApplyProfileResult result = Program.ApplyProfileTask(profileToUse);
                if (result == ApplyProfileResult.Cancelled)
                    errLevel = ERRORLEVEL.CANCELED_BY_USER;
                else if (result == ApplyProfileResult.Error)
                    errLevel = ERRORLEVEL.ERROR_APPLYING_PROFILE;
            }
            else
            {
                Console.WriteLine($"Program/RunProfile: We tried looking for a profile called {profileName} and couldn't find it. It probably is an old display profile that has been deleted previously by the user.");
                errLevel = ERRORLEVEL.ERROR_CANNOT_FIND_PROFILE;
            }

            return errLevel;
        }

        public static ApplyProfileResult ApplyProfileTask(ProfileItem profile)
        {
            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            if (Program.AppBackgroundTaskSemaphoreSlim.CurrentCount == 0)
            {
                Console.WriteLine($"Program/ApplyProfileTask: Cannot apply the display profile {profile.Name} as another task is running!");
                return ApplyProfileResult.Error;
            }
            //await Program.AppBackgroundTaskSemaphoreSlim.WaitAsync(0);
            bool gotGreenLightToProceed = Program.AppBackgroundTaskSemaphoreSlim.Wait(0);
            if (gotGreenLightToProceed)
            {
                Console.WriteLine($"Program/ApplyProfileTask: Got exclusive control of the ApplyProfileTask");
            }
            else
            {
                Console.WriteLine($"Program/ApplyProfileTask: Failed to get control of the ApplyProfileTask, so unable to continue. Returning an Error.");
                return ApplyProfileResult.Error;
            }
            ApplyProfileResult result = ApplyProfileResult.Error;
            if (Program.AppCancellationTokenSource != null)
            {
                Program.AppCancellationTokenSource.Dispose();
            }
            Program.AppCancellationTokenSource = new CancellationTokenSource();
            try
            {
                Task<ApplyProfileResult> taskToRun = Task.Run(() => ProfileRepository.ApplyProfile(profile));
                taskToRun.Wait(120);
                result = taskToRun.Result;
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"Program/ApplyProfileTask: User cancelled the ApplyProfile {profile.Name}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/ApplyProfileTask: Exception while trying to apply Profile {profile.Name}.");
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                if (gotGreenLightToProceed)
                {
                    Program.AppBackgroundTaskSemaphoreSlim.Release();
                }
            }

            //taskToRun.RunSynchronously();
            //result = taskToRun.GetAwaiter().GetResult();                
            if (result == ApplyProfileResult.Successful)
            {
                /*MainForm myMainForm = Program.AppMainForm;
                if (myMainForm.InvokeRequired)
                {
                    myMainForm.BeginInvoke((MethodInvoker)delegate {
                        myMainForm.UpdateNotifyIconText($"DisplayMagician ({profile.Name})");
                    });
                }
                else
                {
                    myMainForm.UpdateNotifyIconText($"DisplayMagician ({profile.Name})");
                }*/

                Console.WriteLine($"Program/ApplyProfileTask: Successfully applied Profile {profile.Name}.");
            }
            else if (result == ApplyProfileResult.Cancelled)
            {
                Console.WriteLine($"Program/ApplyProfileTask: The user cancelled changing to Profile {profile.Name}.");
            }
            else
            {
                Console.WriteLine($"Program/ApplyProfileTask: Error applying the Profile {profile.Name}. Unable to change the display layout.");
            }

            // Replace the code above with this code when it is time for the UI rewrite, as it is non-blocking
            //result = await Task.Run(() => ProfileRepository.ApplyProfile(profile));
            return result;
        }
    }
}
