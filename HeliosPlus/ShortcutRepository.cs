using HeliosPlus.GameLibraries;
using HeliosPlus.InterProcess;
using HeliosPlus.Resources;
using HeliosPlus.Shared;
using Newtonsoft.Json;
using NvAPIWrapper.Mosaic;
using NvAPIWrapper.Native.Mosaic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.IconLib;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeliosPlus
{

    public static class ShortcutRepository
    {
        #region Class Variables
        // Common items to the class
        private static List<ShortcutItem> _allShortcuts = new List<ShortcutItem>();
        private static bool _shortcutsLoaded = false;
        // Other constants that are useful
        private static string AppShortcutStoragePath = Path.Combine(Program.AppDataPath, $"Shortcuts");
        private static string _shortcutStorageJsonFileName = Path.Combine(AppShortcutStoragePath, $"Shortcuts_{Version.ToString(2)}.json");
        private static string uuidV4Regex = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";
        #endregion

        #region Class Constructors
        static ShortcutRepository()
        {

            try
            {
                NvAPIWrapper.NVIDIA.Initialize();

                // Create the Profile Storage Path if it doesn't exist so that it's avilable for all the program
                if (!Directory.Exists(AppShortcutStoragePath))
                {
                    Directory.CreateDirectory(AppShortcutStoragePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ShortcutItem/Instansiation exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // ignored
            }


            // Load the Shortcuts from storage
            LoadShortcuts();
        }

        #endregion

        #region Class Properties
        public static List<ShortcutItem> AllShortcuts
        {
            get
            {
                if (!_shortcutsLoaded)
                    // Load the Shortcuts from storage
                    LoadShortcuts();

                return _allShortcuts;
            }
        }


        public static int ShortcutCount
        {
            get
            {
                if (!_shortcutsLoaded)
                    // Load the Shortcuts from storage
                    LoadShortcuts();

                return _allShortcuts.Count;
            }
        }

        public static Version Version
        {
            get => new Version(1, 0, 0);
        }

        #endregion

        #region Class Methods
        public static bool AddShortcut(ShortcutItem shortcut)
        {
            if (!(shortcut is ShortcutItem))
                return false;

            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (ContainsShortcut(shortcut))
            {
                // We update the existing Shortcut with the data over
                ShortcutItem shortcutToUpdate = GetShortcut(shortcut.UUID);
                shortcut.CopyTo(shortcutToUpdate);
            }
            else
            {
                // Add the shortcut to the list of shortcuts
                _allShortcuts.Add(shortcut);
            }

            //Doublecheck it's been added
            if (ContainsShortcut(shortcut))
            {
                // Generate the Shortcut Icon ready to be used
                SaveShortcutIconToCache(shortcut);

                // Save the shortcuts JSON as it's different
                SaveShortcuts();

                return true;
            }
            else
                return false;

        }

        public static bool RemoveShortcut(ShortcutItem shortcut)
        {
            if (!(shortcut is ShortcutItem))
                return false;

            // Remove the Shortcut Icons from the Cache
            List<ShortcutItem> shortcutsToRemove = _allShortcuts.FindAll(item => item.UUID.Equals(shortcut.UUID, StringComparison.InvariantCultureIgnoreCase));
            foreach (ShortcutItem shortcutToRemove in shortcutsToRemove)
            {
                try
                {
                    File.Delete(shortcutToRemove.SavedShortcutIconCacheFilename);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ShortcutRepository/RemoveShortcut exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");

                    // TODO check and report
                }
            }

            // Remove the shortcut from the list.
            int numRemoved = _allShortcuts.RemoveAll(item => item.UUID.Equals(shortcut.UUID, StringComparison.InvariantCultureIgnoreCase));

            if (numRemoved == 1)
            {
                SaveShortcuts();
                return true;
            }
            else if (numRemoved == 0)
                return false;
            else
                throw new ShortcutRepositoryException();
        }


        public static bool RemoveShortcut(string shortcutNameOrUuid)
        {
            if (String.IsNullOrWhiteSpace(shortcutNameOrUuid))
                return false;

            List<ShortcutItem> shortcutsToRemove;
            int numRemoved;

            //string uuidV4Regex = @"/^[0-9A-F]{8}-[0-9A-F]{4}-4[0-9A-F]{3}-[89AB][0-9A-F]{3}-[0-9A-F]{12}$/i";
            Match match = Regex.Match(shortcutNameOrUuid, uuidV4Regex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                shortcutsToRemove = _allShortcuts.FindAll(item => item.UUID.Equals(shortcutNameOrUuid, StringComparison.InvariantCultureIgnoreCase));
                numRemoved = _allShortcuts.RemoveAll(item => item.UUID.Equals(shortcutNameOrUuid, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                shortcutsToRemove = _allShortcuts.FindAll(item => item.Name.Equals(shortcutNameOrUuid, StringComparison.InvariantCultureIgnoreCase));
                numRemoved = _allShortcuts.RemoveAll(item => item.Name.Equals(shortcutNameOrUuid, StringComparison.InvariantCultureIgnoreCase));
            }
            // Remove the Shortcut Icons from the Cache
            foreach (ShortcutItem shortcutToRemove in shortcutsToRemove)
            {
                try
                {
                    File.Delete(shortcutToRemove.SavedShortcutIconCacheFilename);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ShortcutRepository/RemoveShortcut exception 2: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");

                    // TODO check and report
                }
            }

            if (numRemoved == 1)
            {
                SaveShortcuts();
                return true;
            }
            else if (numRemoved == 0)
                return false;
            else
                throw new ShortcutRepositoryException();

        }


        public static bool ContainsShortcut(ShortcutItem shortcut)
        {
            if (!(shortcut is ShortcutItem))
                return false;

            foreach (ShortcutItem testShortcut in _allShortcuts)
            {
                if (testShortcut.UUID.Equals(shortcut.UUID,StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool ContainsShortcut(string shortcutNameOrUuid)
        {
            if (String.IsNullOrWhiteSpace(shortcutNameOrUuid))
                return false;


            //string uuidV4Regex = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";
            Match match = Regex.Match(shortcutNameOrUuid, uuidV4Regex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (ShortcutItem testShortcut in _allShortcuts)
                {
                    if (testShortcut.UUID.Equals(shortcutNameOrUuid, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }

            }
            else
            {
                foreach (ShortcutItem testShortcut in _allShortcuts)
                {
                    if (testShortcut.Name.Equals(shortcutNameOrUuid, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }

            }

            return false;

        }


        public static ShortcutItem GetShortcut(string shortcutNameOrUuid)
        {
            if (String.IsNullOrWhiteSpace(shortcutNameOrUuid))
                return null;

            //string uuidV4Regex = @"/^[0-9A-F]{8}-[0-9A-F]{4}-4[0-9A-F]{3}-[89AB][0-9A-F]{3}-[0-9A-F]{12}$/i";
            Match match = Regex.Match(shortcutNameOrUuid, uuidV4Regex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (ShortcutItem testShortcut in _allShortcuts)
                {
                    if (testShortcut.UUID.Equals(shortcutNameOrUuid, StringComparison.InvariantCultureIgnoreCase))
                        return testShortcut;
                }

            }
            else
            {
                foreach (ShortcutItem testShortcut in _allShortcuts)
                {
                    if (testShortcut.Name.Equals(shortcutNameOrUuid, StringComparison.InvariantCultureIgnoreCase))
                        return testShortcut;
                }

            }

            return null;

        }

        public static bool RenameShortcutProfile(ProfileItem newProfile)
        {
            if (!(newProfile is ProfileItem))
                return false;

            foreach (ShortcutItem testShortcut in ShortcutRepository.AllShortcuts)
            {
                if (testShortcut.ProfileUUID.Equals(newProfile.UUID, StringComparison.InvariantCultureIgnoreCase) && testShortcut.AutoName)
                {
                    testShortcut.ProfileToUse = newProfile;
                    testShortcut.AutoSuggestShortcutName();
                }
            }

            SaveShortcuts();

            return true;
        }


        private static bool LoadShortcuts()
        {

            if (File.Exists(_shortcutStorageJsonFileName))
            {
                var json = File.ReadAllText(_shortcutStorageJsonFileName, Encoding.Unicode);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    List<ShortcutItem> shortcuts = new List<ShortcutItem>();
                    try
                    {
                        _allShortcuts = JsonConvert.DeserializeObject<List<ShortcutItem>>(json, new JsonSerializerSettings
                        {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Include,
                            TypeNameHandling = TypeNameHandling.Auto
                        });
                    }
                    catch (Exception ex)
                    {
                        // ignored
                        Console.WriteLine($"Unable to load Shortcuts from JSON file {_shortcutStorageJsonFileName}: " + ex.Message);
                    }

                    // Lookup all the Profile Names in the Saved Profiles
                    foreach (ShortcutItem updatedShortcut in _allShortcuts)
                    {
                        foreach (ProfileItem profile in ProfileRepository.AllProfiles)
                        {

                            if (profile.Equals(updatedShortcut.ProfileToUse))
                            {
                                // And assign the matching Profile if we find it.
                                updatedShortcut.ProfileToUse = profile;
                                updatedShortcut.IsPossible = true;
                                break;
                            }
                        }
                    }
                }
            }
            _shortcutsLoaded = true;
            return true;
        }

        private static bool SaveShortcuts()
        {

            if (!Directory.Exists(AppShortcutStoragePath))
            {
                try
                {
                    Directory.CreateDirectory(AppShortcutStoragePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ShortcutRepository/SaveShortcuts exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");

                    Console.WriteLine($"Unable to create Shortcut folder {AppShortcutStoragePath}: " + ex.Message);

                }
            }

            try
            {
                var json = JsonConvert.SerializeObject(_allShortcuts, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    TypeNameHandling = TypeNameHandling.Auto

                });


                if (!string.IsNullOrWhiteSpace(json))
                {
                    File.WriteAllText(_shortcutStorageJsonFileName, json, Encoding.Unicode);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ShortcutRepository/SaveShortcuts exception 2: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");

                Console.WriteLine($"Unable to save Shortcut JSON file {_shortcutStorageJsonFileName}: " + ex.Message);
            }

            return false;
        }

        private static void SaveShortcutIconToCache(ShortcutItem shortcut)
        {

            // Only add the rest of the options if the permanence is temporary
            if (shortcut.Permanence == ShortcutPermanence.Temporary)
            {
                // Only add this set of options if the shortcut is to an standalone application
                if (shortcut.Category == ShortcutCategory.Application)
                {
                    // Work out the name of the shortcut we'll save.
                    shortcut.SavedShortcutIconCacheFilename = Path.Combine(AppShortcutStoragePath, String.Concat(@"executable-", shortcut.ProfileToUse.UUID, "-", Path.GetFileNameWithoutExtension(shortcut.ExecutableNameAndPath), @".ico"));

                }
                // Only add the rest of the options if the temporary switch radio button is set
                // and if the game launching radio button is set
                else if (shortcut.Permanence == ShortcutPermanence.Temporary)
                {
                    // TODO need to make this work so at least one game library is installed
                    // i.e. if (!SteamGame.SteamInstalled && !UplayGame.UplayInstalled )
                    if (shortcut.GameLibrary == SupportedGameLibrary.Steam)
                    {
                        // Work out the name of the shortcut we'll save.
                        shortcut.SavedShortcutIconCacheFilename = Path.Combine(AppShortcutStoragePath, String.Concat(@"steam-", shortcut.ProfileToUse.UUID, "-", shortcut.GameAppId.ToString(), @".ico"));

                    }
                    else if (shortcut.GameLibrary == SupportedGameLibrary.Uplay)
                    {
                        // Work out the name of the shortcut we'll save.
                        shortcut.SavedShortcutIconCacheFilename = Path.Combine(AppShortcutStoragePath, String.Concat(@"uplay-", shortcut.ProfileToUse.UUID, "-", shortcut.GameAppId.ToString(), @".ico"));
                    }

                }

            }
            // Only add the rest of the options if the shortcut is permanent 
            else
            {
                // Work out the name of the shortcut we'll save.
                shortcut.SavedShortcutIconCacheFilename = Path.Combine(AppShortcutStoragePath, String.Concat(@"permanent-", shortcut.ProfileToUse.UUID, @".ico"));
            }

            MultiIcon shortcutIcon;
            try
            {
                //shortcutIcon = new ProfileIcon(shortcut.ProfileToUse).ToIconOverlay(shortcut.OriginalIconPath);
                shortcutIcon = shortcut.ToIconOverlay();
                shortcutIcon.Save(shortcut.SavedShortcutIconCacheFilename, MultiIconFormat.ICO);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ShortcutRepository/SaveShortcutIconToCache exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");

                // If we fail to create an icon based on the original executable or game
                // Then we use the standard HeliosPlus profile one.
                shortcutIcon = shortcut.ProfileToUse.ProfileIcon.ToIcon();
                shortcutIcon.Save(shortcut.SavedShortcutIconCacheFilename, MultiIconFormat.ICO);
            }
        }


        // ReSharper disable once CyclomaticComplexity
        public static void RunShortcut(ShortcutItem shortcutToUse)
        {
            // Do some validation to make sure the shortcut is sensible
            // And that we have enough to try and action within the shortcut
            // including checking the Profile in the shortcut is possible
            // (in other words check everything in the shortcut is still valid)
            if (!(shortcutToUse is ShortcutItem))
                return;

            (bool valid, string reason) = shortcutToUse.IsValid();
            if (!valid)
            {
                throw new Exception(string.Format("ShortcutRepository/SaveShortcutIconToCache exception: Unable to run the shortcut '{0}': {1}", shortcutToUse.Name, reason));
            }

            // Remember the profile we are on now
            bool needToChangeProfiles = false;
            ProfileItem rollbackProfile = ProfileRepository.CurrentProfile;
            if (!rollbackProfile.Equals(shortcutToUse.ProfileToUse))
                needToChangeProfiles = true;



            // Tell the IPC Service we are busy right now, and keep the previous status for later
            InstanceStatus rollbackInstanceStatus = IPCService.GetInstance().Status;
            IPCService.GetInstance().Status = InstanceStatus.Busy;

            // Only change profiles if we have to
            if (needToChangeProfiles)
            {
                // Apply the Profile!
                //if (!ProfileRepository.ApplyProfile(shortcutToUse.ProfileToUse))
                if (!Program.ApplyProfile(shortcutToUse.ProfileToUse))
                {
                    throw new Exception(Language.Cannot_change_active_profile);
                }
            }

            // Set the IP Service status back to what it was
            IPCService.GetInstance().Status = rollbackInstanceStatus;

            // Now run the pre-start applications
            List<Process> startProgramsToStop = new List<Process>();
            if (shortcutToUse.StartPrograms is List<StartProgram> && shortcutToUse.StartPrograms.Count > 0)
            {
                foreach (StartProgram processToStart in shortcutToUse.StartPrograms
                    .Where(program => program.Enabled == true)
                    .OrderBy(program => program.Priority))
                {
                    // Start the executable
                    Console.WriteLine($"Starting process {processToStart.Executable}");
                    Process process = null;
                    if (processToStart.ExecutableArgumentsRequired)
                        process = System.Diagnostics.Process.Start(processToStart.Executable, processToStart.Arguments);
                    else
                        process = System.Diagnostics.Process.Start(processToStart.Executable);
                    // Record t
                    startProgramsToStop.Add(process);
                }
            }

            // Now start the main game, and wait if we have to
            if (shortcutToUse.Category.Equals(ShortcutCategory.Application))
            {
                // Start the executable
                Process process = null;
                if (shortcutToUse.ExecutableArgumentsRequired)
                    process = System.Diagnostics.Process.Start(shortcutToUse.ExecutableNameAndPath, shortcutToUse.ExecutableArguments);
                else
                    process = System.Diagnostics.Process.Start(shortcutToUse.ExecutableNameAndPath);

                // Create a list of processes to monitor
                List<Process> processesToMonitor = new List<Process>();

                // Work out if we are monitoring another process other than the main executable
                if (shortcutToUse.ProcessNameToMonitorUsesExecutable)
                {
                    // If we are monitoring the same executable we started, then lets do that
                    processesToMonitor.Add(process);
                }
                else
                {
                    // Now wait a little while for all the processes we want to monitor to start up
                    var ticks = 0;
                    while (ticks < shortcutToUse.ExecutableTimeout * 1000)
                    {
                        // Look for the processes with the ProcessName we want (which in Windows is the filename without the extension)
                        processesToMonitor = System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(shortcutToUse.DifferentExecutableToMonitor)).ToList();

                        //  TODO: Fix this logic error that will only ever wait for the first process....
                        if (processesToMonitor.Count > 0)
                        {
                            break;
                        }

                        Thread.Sleep(300);
                        ticks += 300;
                    }

                    // If none started up before the timeout, then ignore them
                    if (processesToMonitor.Count == 0)
                    {
                        processesToMonitor.Add(process);
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
                    Console.WriteLine($"ShortcutItem/Run exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
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
                        Console.WriteLine($"ShortcutItem/Run exception 2: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
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
            else if (shortcutToUse.Category.Equals(ShortcutCategory.Game))
            {
                // If the game is a Steam Game we check for that
                if (shortcutToUse.GameLibrary.Equals(SupportedGameLibrary.Steam))
                {
                    // We now need to get the SteamGame info
                    SteamGame steamGameToRun = SteamLibrary.GetSteamGame(shortcutToUse.GameAppId);

                    // If the GameAppID matches a Steam game, then lets run it
                    if (steamGameToRun is SteamGame)
                    {
                        // Prepare to start the steam game using the URI interface 
                        // as used by Steam for it's own desktop shortcuts.
                        var address = $"steam://rungameid/{steamGameToRun.GameId}";
                        if (shortcutToUse.GameArgumentsRequired)
                        {
                            address += "/" + shortcutToUse.GameArguments;
                        }

                        // Start the URI Handler to run Steam
                        var steamProcess = System.Diagnostics.Process.Start(address);

                        // Wait for Steam game to update if needed
                        var ticks = 0;
                        while (ticks < shortcutToUse.GameTimeout * 1000)
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

                        /*// Add a status notification icon in the status area
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
                            Console.WriteLine($"Program/SwitchToSteamGame exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                            // ignored
                        }*/

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
                            Console.WriteLine($"{steamGameToRun.GameName} has been exited. Now continuing with cleanup tasks.");
                        }

                        // Remove the status notification icon from the status area
                        // once we've exited the game
                        /*if (notify != null)
                        {
                            notify.Visible = false;
                            notify.Dispose();
                            Application.DoEvents();
                        }*/

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

            // Stop the pre-started startPrograms that we'd started earlier
            if (startProgramsToStop.Count > 0)
            {
                // Stop the programs in the reverse order we started them
                foreach (Process processToStop in startProgramsToStop.Reverse<Process>())
                {
                    Console.WriteLine($"Stopping process {processToStop.StartInfo.FileName}");
                    // Stop the program
                    processToStop.Close();
                }
            }

            // Change back to the original profile only if it is different
            if (needToChangeProfiles)
            {
                //if (!ProfileRepository.ApplyProfile(rollbackProfile))
                if (!Program.ApplyProfile(rollbackProfile))
                {
                    throw new Exception(Language.Cannot_change_active_profile);
                }
            }

        }

        #endregion

    }

    [global::System.Serializable]
    public class ShortcutRepositoryException : Exception
    {
        public ShortcutRepositoryException() { }
        public ShortcutRepositoryException(string message) : base(message) { }
        public ShortcutRepositoryException(string message, Exception inner) : base(message, inner) { }
        protected ShortcutRepositoryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


}
