using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
        public static string AppVersion = ThisAssembly.AssemblyFileVersion;
        public static bool verboseMode = false;
        public static bool parseableMode = false;

        static int Main(string[] args)
        {

            // Initialise the Profiles Repository
            ProfileRepository.InitialiseRepository();

            // Set up the command line processing
            var app = new CommandLineApplication
            {
                AllowArgumentSeparator = true,
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
            };

            app.Description = "DisplayMagician is an open source tool for automatically configuring your displays and sound for a game or application" + Environment.NewLine + "from a single Windows Shortcut. DisplayMagicianConsole is the command line component of DisplayMagician.";
            app.ExtendedHelpText = "DisplayMagicianConsole allows you to view your current Display Profile or change your Display Profile from a command line.";

            app.GetFullNameAndVersion();
            app.MakeSuggestionsInErrorMessage = true;
            app.HelpOption("-?|-h|--help", inherited: true);

            app.VersionOption("-V|--version", () => {
                return string.Format("{0} v{1}", Assembly.GetExecutingAssembly().GetName().Name, AppVersion);
            });

            CommandOption verbose= app.Option("-v", "Communicate more about what is happening whilst doing it", CommandOptionType.NoValue);
            CommandOption parseable = app.Option("-p", "Make the output easier to parse with regex", CommandOptionType.NoValue);

            // This is the ChangeProfile command
            app.Command(DisplayMagicianStartupAction.ChangeProfile.ToString(), (runProfileCmd) =>
            {
                
                var argumentProfile = runProfileCmd.Argument("\"Profile_UUID\"|\"Name\"", $"(required) The UUID or the Name of the profile to use from those stored in the profile JSON file {ProfileRepository.ProfileStorageFileName}.").IsRequired();
                argumentProfile.Validators.Add(new ProfileMustExistValidator());

                //description and help text of the command.
                runProfileCmd.Description = "Use this command to change to a display profile of your choosing.";
                CommandOption verboseProfile = runProfileCmd.Option("-v", "Communicate more about what is happening whilst doing it", CommandOptionType.NoValue);
                CommandOption parseableProfile = runProfileCmd.Option("-p", "Make the output easier to parse with regex", CommandOptionType.NoValue);

                runProfileCmd.OnExecute(() =>
                {
                    if (verboseProfile.HasValue()) verboseMode = true;
                    if (parseableProfile.HasValue()) parseableMode = true;

                    if (verboseMode) Console.WriteLine($"Program/Main: ChangeProfile commandline command was invoked!");

                    try
                    {
                        ERRORLEVEL errLevel = RunProfile(argumentProfile.Value);
                        return (int)errLevel;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Program/Main: Exception running ApplyProfile {argumentProfile.Value}: - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                        return (int)ERRORLEVEL.ERROR_EXCEPTION;
                    }
                });
            });

            // This is the CurrentProfile command
            // This will output the current display profile if one matches, or 'Unknown'
            app.Command(DisplayMagicianStartupAction.CurrentProfile.ToString(), (currentProfileCmd) =>
            {                
                //description and help text of the command.
                currentProfileCmd.Description = "Use this command to output the name of the display profile currently in use. It will return 'UNKNOWN' if the display profile doesn't match any saved display profiles";
                CommandOption verboseProfile = currentProfileCmd.Option("-v", "Communicate more about what is happening whilst doing it", CommandOptionType.NoValue);
                CommandOption parseableProfile =currentProfileCmd.Option("-p", "Make the output easier to parse with regex", CommandOptionType.NoValue);

                currentProfileCmd.OnExecute(() =>
                {
                    if (verboseProfile.HasValue()) verboseMode = true;
                    if (parseableProfile.HasValue()) parseableMode = true;

                    if (verboseMode) Console.WriteLine($"Program/Main: CurrentProfile commandline command was invoked!");
                    ERRORLEVEL errLevel = CurrentProfile();
                    return (int)errLevel;
                });
            });

            // This is the AllProfiles command
            // This will output the list of all saved display profiles that DisplayMagician knows about
            app.Command(DisplayMagicianStartupAction.AllProfiles.ToString(), (allProfilesCmd) =>
            {
                
                //description and help text of the command.
                allProfilesCmd.Description = "Use this command to output the details for all the display profiles saved in DisplayMagician. It will return 'NONE' if there are no display profiles saved";
                CommandOption verboseProfile = allProfilesCmd.Option("-v", "Communicate more about what is happening whilst doing it", CommandOptionType.NoValue);
                CommandOption parseableProfile = allProfilesCmd.Option("-p", "Make the output easier to parse with regex", CommandOptionType.NoValue);

                allProfilesCmd.OnExecute(() =>
                {
                    if (verboseProfile.HasValue()) verboseMode = true;
                    if (parseableProfile.HasValue()) parseableMode = true;
                    if (verboseMode) Console.WriteLine($"Program/Main: AllProfiles commandline command was invoked!");
                    ERRORLEVEL errLevel = AllProfiles();
                    return (int)errLevel;
                });
            });

            // This is the default action without a command supplied
            app.OnExecute(() =>
            {
                if (verbose.HasValue()) verboseMode = true;
                if (parseable.HasValue()) parseableMode = true;

                if (verboseMode) Console.WriteLine($"Program/Main: Starting the app normally as there was no command supplied...");
                
                if (verboseMode) Console.WriteLine($"Program/Main: Showing the CurrentProfile command by default!");
                ERRORLEVEL errLevel = CurrentProfile();
                return (int)errLevel;
            });

            // Starting the actual commandline parsing app
            try
            {
                // This begins the actual execution of the application
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine($"Program/Main exception: ERROR - Didn't recognise the supplied commandline options: - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return (int)ERRORLEVEL.ERROR_UNKNOWN_COMMAND;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Program/Main commandParsingException: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // You'll always want to catch this exception, otherwise it will generate a messy and confusing error for the end user.
                // the message will usually be something like:
                // "Unrecognized command or argument '<invalid-command>'"
                Console.WriteLine($"Program/Main exception: ERROR - Unable to execute application: - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return (int)ERRORLEVEL.ERROR_EXCEPTION;
            }

            return (int)ERRORLEVEL.OK;
        }

        public static ERRORLEVEL CurrentProfile()
        {
            if (verboseMode) Console.WriteLine($"Program/CurrentProfile: Finding the current profile in use");

            // Lookup the profile
            ProfileItem currentProfile;
            string profileName = "UNKNOWN";
            string profileUUID = "UNKNOWN";
            ERRORLEVEL errLevel = ERRORLEVEL.OK;
            try
            {
                ProfileRepository.UpdateActiveProfile();
                currentProfile = ProfileRepository.GetActiveProfile();
                if (currentProfile is ProfileItem)
                {
                    profileName = currentProfile.Name;
                    profileUUID = currentProfile.UUID;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/CurrentProfile: ERROR - Exception while trying to get the name and UUID of the DisplayMagician profile currently in use: - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                errLevel = ERRORLEVEL.ERROR_EXCEPTION;
            }

            if (!parseableMode) Console.WriteLine($"Display Profile currently in use:");

            if (parseableMode)
            {
                Console.WriteLine($"{profileName}|{profileUUID}");
            }            
            else
            {
                Console.WriteLine($"- \"{profileName}\" (UUID: \"{profileUUID}\")");
            }

            return errLevel;
        }


        public static ERRORLEVEL RunProfile(string profileUUID)
        {
            //Console.WriteLine($"Program/RunProfile: Running profile {profileName}");
            ERRORLEVEL errLevel = ERRORLEVEL.OK;

            Regex validateUUIDRegex = new Regex("^[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12}$");

            if (validateUUIDRegex.IsMatch(profileUUID))
            {
                if (ProfileRepository.AllProfiles.Where(p => p.UUID.Equals(profileUUID)).Any())
                {
                    if (verboseMode) Console.WriteLine($"Program/RunProfile: Found profile with UUID {profileUUID} and now starting to apply the profile");

                    // Get the profile
                    ProfileItem profileToUse = ProfileRepository.AllProfiles.Where(p => p.UUID.Equals(profileUUID)).First();

                    ApplyProfileResult result = Program.ApplyProfileTask(profileToUse);
                    if (result == ApplyProfileResult.Cancelled)
                        errLevel = ERRORLEVEL.CANCELED_BY_USER;
                    else if (result == ApplyProfileResult.Error)
                        errLevel = ERRORLEVEL.ERROR_APPLYING_PROFILE;
                }
                else
                {
                    Console.WriteLine($"Program/RunProfile: ERROR - We tried looking for a profile with UUID {profileUUID} and couldn't find it. It probably is an old display profile that has been deleted previously by the user.");
                    errLevel = ERRORLEVEL.ERROR_CANNOT_FIND_PROFILE;
                }
            }
            else
            {
                if (ProfileRepository.AllProfiles.Where(p => p.Name.Equals(profileUUID)).Any())
                {
                    if (verboseMode) Console.WriteLine($"Program/RunProfile: Found profile with Name {profileUUID} and now starting to apply the profile");

                    // Get the profile
                    ProfileItem profileToUse = ProfileRepository.AllProfiles.Where(p => p.Name.Equals(profileUUID)).First();

                    ApplyProfileResult result = Program.ApplyProfileTask(profileToUse);
                    if (result == ApplyProfileResult.Cancelled)
                        errLevel = ERRORLEVEL.CANCELED_BY_USER;
                    else if (result == ApplyProfileResult.Error)
                        errLevel = ERRORLEVEL.ERROR_APPLYING_PROFILE;
                }
                else
                {
                    Console.WriteLine($"Program/RunProfile: ERROR - We tried looking for a profile with a Name {profileUUID} and couldn't find it. It probably is an old display profile that has been deleted previously by the user.");
                    errLevel = ERRORLEVEL.ERROR_CANNOT_FIND_PROFILE;
                }
            }      

            return errLevel;
        }

        public static ERRORLEVEL AllProfiles()
        {
            if (verboseMode) Console.WriteLine($"Program/AllProfiles: Getting all saved profile");

            ERRORLEVEL errLevel = ERRORLEVEL.OK;

            if (!parseableMode) Console.WriteLine($"Saved Display Profiles:");

            try
            {
                foreach (ProfileItem profile in ProfileRepository.AllProfiles)
                {
                    if (parseableMode)
                    {
                        Console.WriteLine($"{profile.Name}|{profile.UUID}");
                    }
                    else
                    {
                        Console.WriteLine($"- \"{profile.Name}\" (UUID: \"{profile.UUID}\")");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/CurrentProfile: ERROR - Exception while trying to get the list of all saved DisplayMagician profiles: - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                errLevel = ERRORLEVEL.ERROR_EXCEPTION;
            }            

            return errLevel;
        }

        public static ApplyProfileResult ApplyProfileTask(ProfileItem profile)
        {
            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            if (Program.AppBackgroundTaskSemaphoreSlim.CurrentCount == 0)
            {
                Console.WriteLine($"Program/ApplyProfileTask: ERROR - Cannot apply the display profile {profile.Name} as another task is running!");
                return ApplyProfileResult.Error;
            }
            //await Program.AppBackgroundTaskSemaphoreSlim.WaitAsync(0);
            bool gotGreenLightToProceed = Program.AppBackgroundTaskSemaphoreSlim.Wait(0);
            if (gotGreenLightToProceed)
            {
                if (verboseMode) Console.WriteLine($"Program/ApplyProfileTask: Got exclusive control of the ApplyProfileTask");
            }
            else
            {
                Console.WriteLine($"Program/ApplyProfileTask: ERROR - Failed to get control of the ApplyProfileTask, so unable to continue. Returning an Error.");
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
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Program/ApplyProfileTask: User cancelled applying the profile {profile.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/ApplyProfileTask: ERROR - Exception while trying to apply Profile {profile.Name}: - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
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

                Console.WriteLine($"Successfully applied the '{profile.Name}' Display Profile.");
            }
            else if (result == ApplyProfileResult.Cancelled)
            {
                Console.WriteLine($"Program/ApplyProfileTask: ERROR - The user cancelled changing to Profile {profile.Name}.");
            }
            else
            {
                Console.WriteLine($"Program/ApplyProfileTask: ERROR - Error applying the Profile {profile.Name}. Unable to change the display layout.");
            }

            // Replace the code above with this code when it is time for the UI rewrite, as it is non-blocking
            //result = await Task.Run(() => ProfileRepository.ApplyProfile(profile));
            return result;
        }
    }
}
