using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using DisplayMagician.Contracts;
using McMaster.Extensions.CommandLineUtils;

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
            ERROR_PROFILE_SETTINGS_ALREADY_EXIST = 105, // Errorlevel returned when CreateProfile command is used, and the current display settings already match an existing saved profile
            ERROR_PROFILE_NAME_TAKEN = 106, // Errorlevel returned when CreateProfile command is used, and the supplied name is already used by a different profile (and -force was not supplied)
            ERROR_CREATING_PROFILE = 107, // Errorlevel returned when CreateProfile command is used, and the profile could not be saved for an unexpected reason
        };

        public static string AppVersion = ThisAssembly.AssemblyFileVersion;
        public static bool verboseMode = false;
        public static bool parseableMode = false;
        private static readonly ControlServicePipeClient _controlServicePipeClient = new ControlServicePipeClient();

        static int Main(string[] args)
        {

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
            app.Command("ChangeProfile", (runProfileCmd) =>
            {
                
                var argumentProfile = runProfileCmd.Argument("\"Profile_UUID\"|\"Name\"", "(required) The UUID or the Name of the profile to use.").IsRequired();

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
            app.Command("CurrentProfile", (currentProfileCmd) =>
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
            app.Command("AllProfiles", (allProfilesCmd) =>
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

            // This is the CreateProfile command
            // This will save the current display configuration as a new named profile
            app.Command("CreateProfile", (createProfileCmd) =>
            {
                var argumentName = createProfileCmd.Argument("\"Name\"", "(required) The name to give the new display profile.").IsRequired();

                //description and help text of the command.
                createProfileCmd.Description = "Use this command to save the current display configuration as a new display profile.";
                CommandOption forceCreateProfile = createProfileCmd.Option("-force", "Force replacement of an existing profile that has the same name (only applies to name conflicts; duplicate display settings are always blocked)", CommandOptionType.NoValue);
                CommandOption verboseCreateProfile = createProfileCmd.Option("-v", "Communicate more about what is happening whilst doing it", CommandOptionType.NoValue);
                CommandOption parseableCreateProfile = createProfileCmd.Option("-p", "Make the output easier to parse with regex", CommandOptionType.NoValue);

                createProfileCmd.OnExecute(() =>
                {
                    if (verboseCreateProfile.HasValue()) verboseMode = true;
                    if (parseableCreateProfile.HasValue()) parseableMode = true;

                    if (verboseMode) Console.WriteLine($"Program/Main: CreateProfile commandline command was invoked!");

                    try
                    {
                        ERRORLEVEL errLevel = CreateProfile(argumentName.Value, forceCreateProfile.HasValue());
                        return (int)errLevel;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Program/Main: Exception running CreateProfile: - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                        return (int)ERRORLEVEL.ERROR_EXCEPTION;
                    }
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

            string profileName = "UNKNOWN";
            string profileUUID = "UNKNOWN";
            ERRORLEVEL errLevel = ERRORLEVEL.OK;
            try
            {
                ProfileListResult profileList = _controlServicePipeClient.ListProfilesAsync(CancellationToken.None).GetAwaiter().GetResult();
                DisplayProfileView currentProfile = profileList.Views.FirstOrDefault(profile => profile.IsActive) ?? profileList.CurrentLayout;
                if (currentProfile != null)
                {
                    profileName = currentProfile.Name;
                    profileUUID = currentProfile.Id;
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
            ProfileListResult profileList = _controlServicePipeClient.ListProfilesAsync(CancellationToken.None).GetAwaiter().GetResult();
            DisplayProfileView profileToUse = profileList.Views.FirstOrDefault(profile => string.Equals(profile.Id, profileUUID, StringComparison.OrdinalIgnoreCase))
                ?? profileList.Views.FirstOrDefault(profile => string.Equals(profile.Name, profileUUID, StringComparison.OrdinalIgnoreCase));
            if (profileToUse == null)
            {
                Console.WriteLine($"Program/RunProfile: ERROR - We tried looking for a profile with UUID or Name {profileUUID} and couldn't find it. It probably is an old display profile that has been deleted previously by the user.");
                return ERRORLEVEL.ERROR_CANNOT_FIND_PROFILE;
            }

            if (verboseMode) Console.WriteLine($"Program/RunProfile: Found profile with Name {profileToUse.Name} and now starting to apply the profile");
            ControlResponse response = _controlServicePipeClient.ApplyProfileAsync(profileToUse.Id, CancellationToken.None).GetAwaiter().GetResult();
            if (response.IsSuccessful)
            {
                Console.WriteLine($"Successfully applied the '{profileToUse.Name}' Display Profile.");
                return ERRORLEVEL.OK;
            }

            if (response.ApplyProfile?.WasCancelled == true)
            {
                Console.WriteLine($"Program/RunProfile: ERROR - The user cancelled changing to Profile {profileToUse.Name}.");
                return ERRORLEVEL.CANCELED_BY_USER;
            }

            Console.WriteLine($"Program/RunProfile: ERROR - Error applying the Profile {profileToUse.Name}. {response.Message}");
            return ERRORLEVEL.ERROR_APPLYING_PROFILE;
        }

        public static ERRORLEVEL AllProfiles()
        {
            if (verboseMode) Console.WriteLine($"Program/AllProfiles: Getting all saved profile");

            ERRORLEVEL errLevel = ERRORLEVEL.OK;

            if (!parseableMode) Console.WriteLine($"Saved Display Profiles:");

            try
            {
                ProfileListResult profileList = _controlServicePipeClient.ListProfilesAsync(CancellationToken.None).GetAwaiter().GetResult();
                foreach (DisplayProfileView profile in profileList.Views)
                {
                    if (parseableMode)
                    {
                        Console.WriteLine($"{profile.Name}|{profile.Id}");
                    }
                    else
                    {
                        Console.WriteLine($"- \"{profile.Name}\" (UUID: \"{profile.Id}\")");
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

        public static ERRORLEVEL CreateProfile(string name, bool force)
        {
            if (verboseMode) Console.WriteLine($"Program/CreateProfile: Attempting to create a new display profile named \"{name}\"");

            try
            {
                ProfileListResult profileList = _controlServicePipeClient.ListProfilesAsync(CancellationToken.None).GetAwaiter().GetResult();
                DisplayProfileView nameMatch = profileList.Views.FirstOrDefault(profile => string.Equals(profile.Name, name, StringComparison.OrdinalIgnoreCase));
                if (nameMatch != null)
                {
                    if (!force)
                    {
                        if (parseableMode)
                        {
                            Console.WriteLine($"NAME_TAKEN|{nameMatch.Name}|{nameMatch.Id}");
                        }
                        else
                        {
                            Console.WriteLine($"Program/CreateProfile: ERROR - A profile named \"{name}\" already exists (UUID: \"{nameMatch.Id}\"). Use the -force option to replace it with the current display settings.");
                        }

                        return ERRORLEVEL.ERROR_PROFILE_NAME_TAKEN;
                    }

                    if (verboseMode) Console.WriteLine($"Program/CreateProfile: Removing existing profile \"{nameMatch.Name}\" (UUID: \"{nameMatch.Id}\") to replace it.");
                    ControlResponse deleteResponse = _controlServicePipeClient.DeleteProfileAsync(nameMatch.Id, CancellationToken.None).GetAwaiter().GetResult();
                    if (!deleteResponse.IsSuccessful)
                    {
                        Console.WriteLine($"Program/CreateProfile: ERROR - The existing profile \"{name}\" could not be removed. {deleteResponse.Message}");
                        return ERRORLEVEL.ERROR_CREATING_PROFILE;
                    }
                }

                if (verboseMode) Console.WriteLine($"Program/CreateProfile: Saving new profile \"{name}\" through the Control Service.");
                ControlResponse createResponse = _controlServicePipeClient.CreateProfileFromCurrentAsync(name, CancellationToken.None).GetAwaiter().GetResult();
                if (!createResponse.IsSuccessful)
                {
                    if (createResponse.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) || createResponse.Message.Contains("already saved", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine(parseableMode ? "DUPLICATE_SETTINGS|UNKNOWN|UNKNOWN" : $"Program/CreateProfile: ERROR - The current display settings are already saved as a profile. {createResponse.Message}");
                        return ERRORLEVEL.ERROR_PROFILE_SETTINGS_ALREADY_EXIST;
                    }

                    Console.WriteLine($"Program/CreateProfile: ERROR - The profile \"{name}\" could not be saved. {createResponse.Message}");
                    return ERRORLEVEL.ERROR_CREATING_PROFILE;
                }

                ProfileListResult updatedProfileList = _controlServicePipeClient.ListProfilesAsync(CancellationToken.None).GetAwaiter().GetResult();
                DisplayProfileView createdProfile = updatedProfileList.Views.FirstOrDefault(profile => string.Equals(profile.Name, name, StringComparison.OrdinalIgnoreCase));
                if (createdProfile == null)
                {
                    Console.WriteLine($"Program/CreateProfile: ERROR - The profile \"{name}\" was created but could not be retrieved.");
                    return ERRORLEVEL.ERROR_CREATING_PROFILE;
                }

                if (parseableMode)
                {
                    Console.WriteLine($"{createdProfile.Name}|{createdProfile.Id}");
                }
                else
                {
                    Console.WriteLine($"Display profile \"{createdProfile.Name}\" (UUID: \"{createdProfile.Id}\") created successfully.");
                }

                return ERRORLEVEL.OK;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program/CreateProfile: ERROR - Exception while creating the display profile: - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return ERRORLEVEL.ERROR_EXCEPTION;
            }
        }
    }
}
