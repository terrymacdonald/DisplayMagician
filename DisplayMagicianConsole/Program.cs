using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using DisplayMagician.Contracts;
using McMaster.Extensions.CommandLineUtils;
using NLog;
using NLog.Config;
using NLog.Targets;

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
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly ControlServicePipeClient _controlServicePipeClient = new ControlServicePipeClient();

        static int Main(string[] args)
        {
            ConfigureLogging();
            logger.Info("Program/Main: Desktop Console started with {0} argument(s).", args.Length);

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

            app.Command("Status", operationStatusCmd =>
            {
                operationStatusCmd.Description = "List the current and recently completed DisplayMagician operations for this Windows session.";
                CommandOption parseableStatus = operationStatusCmd.Option("-p", "Make the output easier to parse with regex", CommandOptionType.NoValue);
                operationStatusCmd.OnExecute(() =>
                {
                    if (parseableStatus.HasValue()) parseableMode = true;
                    return (int)ListOperationStatuses();
                });
            });

            app.Command("ListDecisions", operationDecisionsCmd =>
            {
                operationDecisionsCmd.Description = "List pending operation decisions that may be answered by this Windows session.";
                CommandOption parseableDecisions = operationDecisionsCmd.Option("-p", "Make the output easier to parse with regex", CommandOptionType.NoValue);
                operationDecisionsCmd.OnExecute(() =>
                {
                    if (parseableDecisions.HasValue()) parseableMode = true;
                    return (int)ListOperationDecisions();
                });
            });

            app.Command("AnswerDecision", resolveDecisionCmd =>
            {
                resolveDecisionCmd.Description = "Answer a pending operation decision. Use Continue or StopAndRestore.";
                CommandArgument promptId = resolveDecisionCmd.Argument("Prompt_ID", "The pending decision prompt ID.").IsRequired();
                CommandArgument choice = resolveDecisionCmd.Argument("Choice", "Continue or StopAndRestore.").IsRequired();
                resolveDecisionCmd.OnExecute(() => (int)ResolveOperationDecision(promptId.Value, choice.Value));
            });

            app.Command("CreateAudioProfile", createAudioProfileCmd =>
            {
                createAudioProfileCmd.Description = "Save the current audio setup as a new audio profile.";
                CommandArgument name = createAudioProfileCmd.Argument("Name", "The name for the new audio profile.").IsRequired();
                createAudioProfileCmd.OnExecute(() => (int)CreateAudioProfile(name.Value));
            });

            app.Command("AllAudioProfiles", allAudioProfilesCmd =>
            {
                allAudioProfilesCmd.Description = "List all saved audio profiles.";
                CommandOption parseableAudioProfiles = allAudioProfilesCmd.Option("-p", "Make the output easier to parse with regex", CommandOptionType.NoValue);
                allAudioProfilesCmd.OnExecute(() =>
                {
                    if (parseableAudioProfiles.HasValue()) parseableMode = true;
                    return (int)ListAudioProfiles();
                });
            });

            app.Command("CurrentAudioProfile", currentAudioProfileCmd =>
            {
                currentAudioProfileCmd.Description = "Show the saved audio profile matching the current audio setup, or UNKNOWN.";
                CommandOption parseableCurrentAudio = currentAudioProfileCmd.Option("-p", "Make the output easier to parse with regex", CommandOptionType.NoValue);
                currentAudioProfileCmd.OnExecute(() =>
                {
                    if (parseableCurrentAudio.HasValue()) parseableMode = true;
                    return (int)CurrentAudioProfile();
                });
            });

            app.Command("ChangeAudioProfile", changeAudioProfileCmd =>
            {
                changeAudioProfileCmd.Description = "Apply a saved audio profile by UUID or name.";
                CommandArgument profile = changeAudioProfileCmd.Argument("Profile_UUID|Name", "The UUID or name of the audio profile.").IsRequired();
                changeAudioProfileCmd.OnExecute(() => (int)ChangeAudioProfile(profile.Value));
            });

            app.Command("RunShortcut", runShortcutCmd =>
            {
                runShortcutCmd.Description = "Run a saved shortcut by UUID or name.";
                CommandArgument shortcut = runShortcutCmd.Argument("Shortcut_UUID|Name", "The UUID or name of the shortcut.").IsRequired();
                runShortcutCmd.OnExecute(() => (int)RunShortcut(shortcut.Value));
            });

            app.Command("AllShortcuts", allShortcutsCmd =>
            {
                allShortcutsCmd.Description = "List all saved game, application, and executable shortcuts.";
                CommandOption parseableShortcuts = allShortcutsCmd.Option("-p", "Make the output easier to parse with regex", CommandOptionType.NoValue);
                allShortcutsCmd.OnExecute(() =>
                {
                    if (parseableShortcuts.HasValue()) parseableMode = true;
                    return (int)ListShortcuts();
                });
            });

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
                        logger.Error(ex, "Program/Main: Exception running ApplyProfile {0}.", argumentProfile.Value);
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
                        logger.Error(ex, "Program/Main: Exception running CreateProfile.");
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
                logger.Warn(ex, "Program/Main: The supplied command-line options were not recognized.");
                Console.WriteLine($"Program/Main exception: ERROR - Didn't recognise the supplied commandline options: - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return (int)ERRORLEVEL.ERROR_UNKNOWN_COMMAND;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Program/Main: Unable to execute the Desktop Console command.");
                //Console.WriteLine($"Program/Main commandParsingException: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // You'll always want to catch this exception, otherwise it will generate a messy and confusing error for the end user.
                // the message will usually be something like:
                // "Unrecognized command or argument '<invalid-command>'"
                Console.WriteLine($"Program/Main exception: ERROR - Unable to execute application: - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return (int)ERRORLEVEL.ERROR_EXCEPTION;
            }

            return (int)ERRORLEVEL.OK;
        }

        private static void ConfigureLogging()
        {
            string userSid = WindowsIdentity.GetCurrent().User?.Value ?? "UnknownUser";
            string legacyLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician", "Logs");
            string preferredLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician", "Users", userSid, "Logs");
            string logPath = TryPrepareLogPath(preferredLogPath, legacyLogPath) ? preferredLogPath : legacyLogPath;
            try
            {
                Directory.CreateDirectory(logPath);
                SupportLogLayout.Register();
                LoggingConfiguration configuration = new LoggingConfiguration();
                FileTarget logFile = new FileTarget("desktop-console-log")
                {
                    FileName = Path.Combine(logPath, "DesktopConsole-${shortdate}.log"),
                    ArchiveAboveSize = 41943040,
                    MaxArchiveFiles = 4,
                    Layout = "${displaymagicianlog:component=DesktopConsole}"
                };
                configuration.AddRule(LogLevel.Info, LogLevel.Fatal, logFile);
                LogManager.Configuration = configuration;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException)
            {
                Console.Error.WriteLine($"DisplayMagicianConsole could not configure diagnostic logging: {ex.Message}");
            }
        }

        private static bool TryPrepareLogPath(string preferredLogPath, string legacyLogPath)
        {
            try
            {
                Directory.CreateDirectory(preferredLogPath);
                string probePath = Path.Combine(preferredLogPath, $".write-probe-{Guid.NewGuid():N}.tmp");
                using (FileStream probe = new FileStream(probePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose))
                {
                    probe.WriteByte(0);
                }

                if (Directory.Exists(legacyLogPath))
                {
                    foreach (string legacyLogFile in Directory.EnumerateFiles(legacyLogPath, "DesktopConsole-*.log", SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            string destinationPath = Path.Combine(preferredLogPath, Path.GetFileName(legacyLogFile));
                            if (File.Exists(destinationPath))
                            {
                                destinationPath = Path.Combine(preferredLogPath, $"{Path.GetFileNameWithoutExtension(legacyLogFile)}-{Guid.NewGuid():N}{Path.GetExtension(legacyLogFile)}");
                            }

                            File.Move(legacyLogFile, destinationPath);
                        }
                        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                        {
                            Console.Error.WriteLine($"DisplayMagicianConsole could not migrate legacy log {legacyLogFile}: {ex.Message}");
                        }
                    }
                }

                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException)
            {
                Console.Error.WriteLine($"DisplayMagicianConsole is using its legacy log path because ProgramData is unavailable: {ex.Message}");
                return false;
            }
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
                logger.Error(ex, "Program/CurrentProfile: Could not retrieve the current display profile.");
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

        public static ERRORLEVEL CreateAudioProfile(string name)
        {
            try
            {
                ControlResponse response = _controlServicePipeClient.CreateAudioProfileFromCurrentAsync(name, CancellationToken.None).GetAwaiter().GetResult();
                Console.WriteLine(response.IsSuccessful ? $"Audio profile '{name}' created." : $"Could not create audio profile '{name}'. {response.Message}");
                return response.IsSuccessful ? ERRORLEVEL.OK : ERRORLEVEL.ERROR_CREATING_PROFILE;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Program/CreateAudioProfile: Could not create audio profile {0}.", name);
                Console.Error.WriteLine($"DisplayMagicianConsole could not create the audio profile: {ex.Message}");
                return ERRORLEVEL.ERROR_EXCEPTION;
            }
        }

        public static ERRORLEVEL ListAudioProfiles()
        {
            try
            {
                foreach (AudioProfileView profile in _controlServicePipeClient.ListAudioProfilesAsync(CancellationToken.None).GetAwaiter().GetResult().Views)
                {
                    Console.WriteLine(parseableMode ? $"{profile.Name}|{profile.Id}" : $"- \"{profile.Name}\" (UUID: \"{profile.Id}\")");
                }

                return ERRORLEVEL.OK;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Program/ListAudioProfiles: Could not retrieve audio profiles.");
                Console.Error.WriteLine($"DisplayMagicianConsole could not retrieve audio profiles: {ex.Message}");
                return ERRORLEVEL.ERROR_EXCEPTION;
            }
        }

        public static ERRORLEVEL CurrentAudioProfile()
        {
            try
            {
                AudioProfileListResult profiles = _controlServicePipeClient.ListAudioProfilesAsync(CancellationToken.None).GetAwaiter().GetResult();
                AudioProfileView currentProfile = profiles.Views.FirstOrDefault(profile => profile.IsActive) ?? profiles.CurrentLayout;
                string name = currentProfile?.IsSaved == true ? currentProfile.Name : "UNKNOWN";
                string id = currentProfile?.IsSaved == true ? currentProfile.Id : "UNKNOWN";
                Console.WriteLine(parseableMode ? $"{name}|{id}" : $"Current audio profile: \"{name}\" (UUID: \"{id}\")");
                return ERRORLEVEL.OK;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Program/CurrentAudioProfile: Could not retrieve current audio profile.");
                Console.Error.WriteLine($"DisplayMagicianConsole could not retrieve the current audio profile: {ex.Message}");
                return ERRORLEVEL.ERROR_EXCEPTION;
            }
        }

        public static ERRORLEVEL ChangeAudioProfile(string profileIdOrName)
        {
            try
            {
                AudioProfileView profile = _controlServicePipeClient.ListAudioProfilesAsync(CancellationToken.None).GetAwaiter().GetResult().Views.FirstOrDefault(item => string.Equals(item.Id, profileIdOrName, StringComparison.OrdinalIgnoreCase) || string.Equals(item.Name, profileIdOrName, StringComparison.OrdinalIgnoreCase));
                if (profile == null)
                {
                    Console.Error.WriteLine($"No audio profile named or identified by '{profileIdOrName}' was found.");
                    return ERRORLEVEL.ERROR_CANNOT_FIND_PROFILE;
                }

                ControlResponse response = _controlServicePipeClient.ApplyAudioProfileAsync(profile.Id, CancellationToken.None).GetAwaiter().GetResult();
                Console.WriteLine(response.IsSuccessful ? $"Audio profile '{profile.Name}' applied." : $"Could not apply audio profile '{profile.Name}'. {response.Message}");
                return response.IsSuccessful ? ERRORLEVEL.OK : ERRORLEVEL.ERROR_APPLYING_PROFILE;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Program/ChangeAudioProfile: Could not apply audio profile {0}.", profileIdOrName);
                Console.Error.WriteLine($"DisplayMagicianConsole could not apply the audio profile: {ex.Message}");
                return ERRORLEVEL.ERROR_EXCEPTION;
            }
        }

        public static ERRORLEVEL RunShortcut(string shortcutIdOrName)
        {
            try
            {
                ShortcutView shortcut = _controlServicePipeClient.ListShortcutsAsync(CancellationToken.None).GetAwaiter().GetResult().Shortcuts.FirstOrDefault(item => string.Equals(item.Id, shortcutIdOrName, StringComparison.OrdinalIgnoreCase) || string.Equals(item.Name, shortcutIdOrName, StringComparison.OrdinalIgnoreCase));
                if (shortcut == null)
                {
                    Console.Error.WriteLine($"No shortcut named or identified by '{shortcutIdOrName}' was found.");
                    return ERRORLEVEL.ERROR_CANNOT_FIND_SHORTCUT;
                }

                ControlResponse response = _controlServicePipeClient.StartShortcutAsync(shortcut.Id, CancellationToken.None).GetAwaiter().GetResult();
                Console.WriteLine(response.IsSuccessful ? $"Shortcut '{shortcut.Name}' started." : $"Could not start shortcut '{shortcut.Name}'. {response.Message}");
                return response.IsSuccessful ? ERRORLEVEL.OK : ERRORLEVEL.ERROR_EXCEPTION;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Program/RunShortcut: Could not start shortcut {0}.", shortcutIdOrName);
                Console.Error.WriteLine($"DisplayMagicianConsole could not start the shortcut: {ex.Message}");
                return ERRORLEVEL.ERROR_EXCEPTION;
            }
        }

        public static ERRORLEVEL ListShortcuts()
        {
            try
            {
                foreach (ShortcutView shortcut in _controlServicePipeClient.ListShortcutsAsync(CancellationToken.None).GetAwaiter().GetResult().Shortcuts)
                {
                    Console.WriteLine(parseableMode ? $"{shortcut.Name}|{shortcut.Id}|{shortcut.Category}" : $"- \"{shortcut.Name}\" (UUID: \"{shortcut.Id}\", category: {shortcut.Category})");
                }

                return ERRORLEVEL.OK;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Program/ListShortcuts: Could not retrieve shortcuts.");
                Console.Error.WriteLine($"DisplayMagicianConsole could not retrieve shortcuts: {ex.Message}");
                return ERRORLEVEL.ERROR_EXCEPTION;
            }
        }

        public static ERRORLEVEL ListOperationStatuses()
        {
            try
            {
                foreach (OperationStatus status in _controlServicePipeClient.ListOperationStatusesAsync(CancellationToken.None).GetAwaiter().GetResult())
                {
                    string updatedUtc = status.UpdatedUtc.ToUniversalTime().ToString("O");
                    Console.WriteLine(parseableMode
                        ? $"{status.OperationId}|{status.Sequence}|{status.Phase}|{status.IsTerminal}|{status.IsSuccessful}|{updatedUtc}|{status.Message}"
                        : $"{status.OperationId} #{status.Sequence}: {status.Phase} - {status.Message} ({updatedUtc})");
                }

                return ERRORLEVEL.OK;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Program/ListOperationStatuses: Could not retrieve operation statuses.");
                Console.Error.WriteLine($"DisplayMagicianConsole could not retrieve operation statuses: {ex.Message}");
                return ERRORLEVEL.ERROR_EXCEPTION;
            }
        }

        public static ERRORLEVEL ListOperationDecisions()
        {
            try
            {
                foreach (OperationDecision decision in _controlServicePipeClient.ListOperationDecisionsAsync(CancellationToken.None).GetAwaiter().GetResult())
                {
                    string expiresUtc = decision.ExpiresUtc.ToUniversalTime().ToString("O");
                    Console.WriteLine(parseableMode
                        ? $"{decision.PromptId}|{decision.OperationId}|{decision.DefaultChoice}|{expiresUtc}|{decision.Title}|{decision.Message}"
                        : $"{decision.PromptId}: {decision.Title}{Environment.NewLine}{decision.Message}{Environment.NewLine}Default: {decision.DefaultChoice}; expires {expiresUtc}");
                }

                return ERRORLEVEL.OK;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Program/ListOperationDecisions: Could not retrieve pending operation decisions.");
                Console.Error.WriteLine($"DisplayMagicianConsole could not retrieve pending operation decisions: {ex.Message}");
                return ERRORLEVEL.ERROR_EXCEPTION;
            }
        }

        public static ERRORLEVEL ResolveOperationDecision(string promptIdText, string choiceText)
        {
            if (!Guid.TryParse(promptIdText, out Guid promptId) || !Enum.TryParse(choiceText, true, out OperationDecisionChoice choice) || choice == OperationDecisionChoice.Unknown)
            {
                Console.Error.WriteLine("Specify a valid prompt ID and either Continue or StopAndRestore.");
                return ERRORLEVEL.ERROR_EXCEPTION;
            }

            try
            {
                OperationDecision decision = _controlServicePipeClient.ResolveOperationDecisionAsync(promptId, choice, CancellationToken.None).GetAwaiter().GetResult();
                Console.WriteLine($"Operation decision {decision.PromptId} resolved as {decision.ResolvedChoice}.");
                return ERRORLEVEL.OK;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Program/ResolveOperationDecision: Could not resolve operation decision {0}.", promptIdText);
                Console.Error.WriteLine($"DisplayMagicianConsole could not resolve the operation decision: {ex.Message}");
                return ERRORLEVEL.ERROR_EXCEPTION;
            }
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
                logger.Error(ex, "Program/AllProfiles: Could not retrieve saved display profiles.");
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
                logger.Error(ex, "Program/CreateProfile: Could not create a display profile.");
                Console.WriteLine($"Program/CreateProfile: ERROR - Exception while creating the display profile: - {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return ERRORLEVEL.ERROR_EXCEPTION;
            }
        }
    }
}
