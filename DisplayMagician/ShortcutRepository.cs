using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using DisplayMagician.GameLibraries;
using DisplayMagician.InterProcess;
using DisplayMagicianShared;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace DisplayMagician
{

    public static class ShortcutRepository
    {
        #region Class Variables
        // Common items to the class
        private static List<ShortcutItem> _allShortcuts = new List<ShortcutItem>();
        //public static Dictionary<string, bool> _shortcutWarningLookup = new Dictionary<string, bool>();
        //public static Dictionary<string, bool> _shortcutErrorLookup = new Dictionary<string, bool>();
        private static bool _shortcutsLoaded = false;
        // Other constants that are useful
        private static string AppShortcutStoragePath = Path.Combine(Program.AppDataPath, $"Shortcuts");
        private static string _shortcutStorageJsonFileName = Path.Combine(AppShortcutStoragePath, $"Shortcuts_{Version.ToString(2)}.json");
        private static string uuidV4Regex = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";
        private static CoreAudioController _audioController = null;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        #region Class Constructors
        static ShortcutRepository()
        {

            try
            {
                NvAPIWrapper.NVIDIA.Initialize();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ShortcutRepository/ShortcutRepository: Initialising NVIDIA NvAPIWrapper or CoreAudioController caused an exception.");
            }

            try
            {
                _audioController = new CoreAudioController();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ShortcutRepository/ShortcutRepository: Exception while trying to initialise CoreAudioController. Audio Chipset on your computer is not supported. You will be unable to set audio settings.");
            }

            //_audioController.DefaultPlaybackDevice.SetAsDefault();
            //_audioController.DefaultCaptureDevice.SetAsDefault();

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

        /*public static Dictionary<string, bool> ShortcutWarningLookup
        {
            get
            {
                if (!_shortcutsLoaded)
                    // Load the Shortcuts from storage if they need to be
                    LoadShortcuts();

                return _shortcutWarningLookup;
            }
        }

        public static Dictionary<string, bool> ShortcutErrorLookup
        {
            get
            {
                if (!_shortcutsLoaded)
                    // Load the Shortcuts from storage if they need to be
                    LoadShortcuts();

                return _shortcutErrorLookup;
            }
        }*/

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

#pragma warning disable CS3003 // Type is not CLS-compliant
        public static CoreAudioController AudioController
#pragma warning restore CS3003 // Type is not CLS-compliant
        {
            get
            {
                return _audioController;
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
            logger.Trace($"ShortcutRepository/AddShortcut: Adding shortcut {shortcut.Name} to our shortcut repository");

            if (!(shortcut is ShortcutItem))
                return false;

            // Add the shortcut to the list of shortcuts
            _allShortcuts.Add(shortcut);

            //Doublecheck it's been added
            if (ContainsShortcut(shortcut))
            {
                // Save the shortcuts JSON as it's different
                SaveShortcuts();
                IsValidRefresh();
                return true;
            }
            else
                return false;

        }

        public static bool RemoveShortcut(ShortcutItem shortcut)
        {
            logger.Trace($"ShortcutRepository/RemoveShortcut: Removing shortcut {shortcut.Name} if it exists in our shortcut repository");

            if (!(shortcut is ShortcutItem))
                return false;

            // Remove the Shortcut Icons from the Cache
            List<ShortcutItem> shortcutsToRemove = _allShortcuts.FindAll(item => item.UUID.Equals(shortcut.UUID, StringComparison.OrdinalIgnoreCase));
            foreach (ShortcutItem shortcutToRemove in shortcutsToRemove)
            {
                try
                {
                    logger.Info($"ShortcutRepository/RemoveShortcut: Removing shortcut {shortcutToRemove.Name}");
                    File.Delete(shortcutToRemove.SavedShortcutIconCacheFilename);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ShortcutRepository/RemoveShortcut: Exception removing shortcut {shortcutToRemove.Name}");
                }
            }

            // Remove the shortcut from the list.
            int numRemoved = _allShortcuts.RemoveAll(item => item.UUID.Equals(shortcut.UUID, StringComparison.OrdinalIgnoreCase));

            if (numRemoved == 1)
            {
                SaveShortcuts();
                IsValidRefresh();
                logger.Trace($"ShortcutRepository/RemoveShortcut: Our shortcut repository does contain a shortcut we were looking for");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Trace($"ShortcutRepository/RemoveShortcut: Our shortcut repository doesn't contain a shortcut we were looking for");
                return false;
            }
                
            else
                throw new ShortcutRepositoryException();
        }


        public static bool RemoveShortcut(string shortcutNameOrUuid)
        {

            logger.Trace($"ShortcutRepository/RemoveShortcut2: Removing shortcut {shortcutNameOrUuid} if it exists in our shortcut repository");

            if (String.IsNullOrWhiteSpace(shortcutNameOrUuid))
            {
                logger.Error($"ShortcutRepository/RemoveShortcut2: Shortcut to look for was empty or only whitespace");
                return false;
            }
            List<ShortcutItem> shortcutsToRemove;
            int numRemoved;

            Match match = Regex.Match(shortcutNameOrUuid, uuidV4Regex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                shortcutsToRemove = _allShortcuts.FindAll(item => item.UUID.Equals(shortcutNameOrUuid, StringComparison.OrdinalIgnoreCase));
                numRemoved = _allShortcuts.RemoveAll(item => item.UUID.Equals(shortcutNameOrUuid, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                shortcutsToRemove = _allShortcuts.FindAll(item => item.Name.Equals(shortcutNameOrUuid, StringComparison.OrdinalIgnoreCase));
                numRemoved = _allShortcuts.RemoveAll(item => item.Name.Equals(shortcutNameOrUuid, StringComparison.OrdinalIgnoreCase));
            }
            // Remove the Shortcut Icons from the Cache
            foreach (ShortcutItem shortcutToRemove in shortcutsToRemove)
            {
                try
                {
                    logger.Info($"ShortcutRepository/RemoveShortcut2: Removing shortcut {shortcutToRemove.Name}");
                    File.Delete(shortcutToRemove.SavedShortcutIconCacheFilename);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ShortcutRepository/RemoveShortcut2: Exception removing shortcut {shortcutToRemove.Name}");
                }
            }

            if (numRemoved == 1)
            {
                SaveShortcuts();
                IsValidRefresh();
                logger.Trace($"ShortcutRepository/RemoveShortcut2: Our shortcut repository does contain a shortcut with Name or UUID {shortcutNameOrUuid}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Trace($"ShortcutRepository/RemoveShortcut2: Our shortcut repository doesn't contain a shortcut with Name or UUID {shortcutNameOrUuid}");
                return false;
            }
            else
                throw new ShortcutRepositoryException();

        }


        public static bool ContainsShortcut(ShortcutItem shortcut)
        {

            logger.Trace($"ShortcutRepository/ContainsShortcut: Checking whether {shortcut.Name} exists in our shortcut repository");

            if (!(shortcut is ShortcutItem))
                return false;

            foreach (ShortcutItem testShortcut in _allShortcuts)
            {
                if (testShortcut.UUID.Equals(shortcut.UUID, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Trace($"ShortcutRepository/ContainsShortcut: {shortcut.Name} does exist in our shortcut repository");
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsShortcut(string shortcutNameOrUuid)
        {

            logger.Trace($"ShortcutRepository/ContainsShortcut2: Checking whether {shortcutNameOrUuid} exists in our shortcut repository");

            if (String.IsNullOrWhiteSpace(shortcutNameOrUuid))
            {
                logger.Error($"ShortcutRepository/ContainsShortcut2: Shortcut to look for was empty or only whitespace");
                return false;
            }

            Match match = Regex.Match(shortcutNameOrUuid, uuidV4Regex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (ShortcutItem testShortcut in _allShortcuts)
                {
                    if (testShortcut.UUID.Equals(shortcutNameOrUuid, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Trace($"ShortcutRepository/ContainsShortcut2: Shortcut with UUID {shortcutNameOrUuid} does exist in our shortcut repository");
                        return true;
                    }
                }

            }
            else
            {
                foreach (ShortcutItem testShortcut in _allShortcuts)
                {
                    if (testShortcut.Name.Equals(shortcutNameOrUuid, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Trace($"ShortcutRepository/ContainsShortcut2: Shortcut with name {shortcutNameOrUuid} does exist in our shortcut repository");
                        return true;
                    }
                }

            }

            logger.Trace($"ShortcutRepository/ContainsShortcut2: Shortcut with name {shortcutNameOrUuid} doesn't exist in our shortcut repository");
            return false;

        }


        public static ShortcutItem GetShortcut(string shortcutNameOrUuid)
        {
            logger.Trace($"ShortcutRepository/GetShortcut: Finding and returning {shortcutNameOrUuid} if it exists in our shortcut repository");

            if (String.IsNullOrWhiteSpace(shortcutNameOrUuid))
            {
                logger.Error($"ShortcutRepository/GetShortcut: Shortcut to get was empty or only whitespace");
                return null;
            }

            Match match = Regex.Match(shortcutNameOrUuid, uuidV4Regex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (ShortcutItem testShortcut in _allShortcuts)
                {
                    if (testShortcut.UUID.Equals(shortcutNameOrUuid, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Trace($"ShortcutRepository/GetShortcut: Returning shortcut with UUID {shortcutNameOrUuid}");
                        return testShortcut;
                    }
                }

            }
            else
            {
                foreach (ShortcutItem testShortcut in _allShortcuts)
                {
                    if (testShortcut.Name.Equals(shortcutNameOrUuid, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Trace($"ShortcutRepository/GetShortcut: Returning shortcut with Name {shortcutNameOrUuid}");
                        return testShortcut;
                    }
                }

            }

            logger.Trace($"ShortcutRepository/GetShortcut: No shortcut was found to return with UUID or Name {shortcutNameOrUuid}");
            return null;

        }

#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public static bool RenameShortcutProfile(ProfileItem newProfile)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            logger.Debug($"ShortcutRepository/RenameShortcutProfile: Renaming the profile in any shortcuts containing the old profile name");

            if (!(newProfile is ProfileItem))
                return false;

            foreach (ShortcutItem testShortcut in ShortcutRepository.AllShortcuts)
            {
                if (testShortcut.ProfileUUID.Equals(newProfile.UUID, StringComparison.OrdinalIgnoreCase) && testShortcut.AutoName)
                {
                    logger.Debug($"ShortcutRepository/RenameShortcutProfile: Renaming {testShortcut.Name} shortcut's profile to {newProfile.Name} since the original profile has just been renamed.");
                    testShortcut.ProfileToUse = newProfile;
                    testShortcut.AutoSuggestShortcutName();
                }
            }

            SaveShortcuts();
            IsValidRefresh();
            return true;
        }

        private static bool LoadShortcuts()
        {

            logger.Debug($"ShortcutRepository/LoadShortcuts: Loading shortcuts from {_shortcutStorageJsonFileName} into the Shortcut Repository");

            if (File.Exists(_shortcutStorageJsonFileName))
            {
                string json = "";
                try
                {
                    json = File.ReadAllText(_shortcutStorageJsonFileName, Encoding.Unicode);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ShortcutRepository/LoadShortcuts: Tried to read the JSON file {_shortcutStorageJsonFileName} to memory but File.ReadAllTextthrew an exception.");
                }

                if (!string.IsNullOrWhiteSpace(json))
                {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    List<ShortcutItem> shortcuts = new List<ShortcutItem>();
#pragma warning restore IDE0059 // Unnecessary assignment of a value
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
                        logger.Error(ex, $"ShortcutRepository/LoadShortcuts: Tried to parse the JSON in the {_shortcutStorageJsonFileName} but the JsonConvert threw an exception.");
                    }

                    // Lookup all the Profile Names in the Saved Profiles
                    // and link the profiles to the Shortcuts as we only 
                    // store the profile names to allow users to uodate profiles
                    // separately from the shortcuts
                    logger.Debug($"ShortcutRepository/LoadShortcuts: Connecting Shortcut profile names to the real profile objects");
                    foreach (ShortcutItem updatedShortcut in _allShortcuts)
                    {
                        bool foundProfile = false;
                        foreach (ProfileItem profile in ProfileRepository.AllProfiles)
                        {

                            if (profile.UUID.Equals(updatedShortcut.ProfileUUID))
                            {
                                // And assign the matching Profile if we find it.
                                updatedShortcut.ProfileToUse = profile;
                                foundProfile = true;
                                logger.Debug($"ShortcutRepository/LoadShortcuts: Found the profile with UUID {updatedShortcut.ProfileUUID} and linked it to a profile!");
                                break;
                            }
                        }

                        if (!foundProfile)
                        {
                            // We should only get here if there isn't a profile to match to.
                            logger.Debug($"ShortcutRepository/LoadShortcuts: Couldn't find the profile with UUID {updatedShortcut.ProfileUUID} so couldn't link it to a profile! We can't use this shortcut.");
                            updatedShortcut.ProfileToUse = null;
                        }                            
                    }

                    // Sort the shortcuts alphabetically
                    _allShortcuts.Sort();
                }
                else
                {
                    logger.Debug($"ShortcutRepository/LoadShortcuts: The {_shortcutStorageJsonFileName} shortcut JSON file exists but is empty! So we're going to treat it as if it didn't exist.");
                }
            }
            else
            {
                logger.Debug($"ShortcutRepository/LoadShortcuts: Couldn't find the {_shortcutStorageJsonFileName} shortcut JSON file that contains the Shortcuts");
            }
            _shortcutsLoaded = true;

            IsValidRefresh();
            return true;
        }

        public static bool SaveShortcuts()
        {
            logger.Debug($"ShortcutRepository/SaveShortcuts: Attempting to save the shortcut repository to the {_shortcutStorageJsonFileName}.");

            if (!Directory.Exists(AppShortcutStoragePath))
            {
                logger.Debug($"ShortcutRepository/SaveShortcuts: Creating the shortcut folder {AppShortcutStoragePath} as it doesn't currently exist.");
                try
                {
                    Directory.CreateDirectory(AppShortcutStoragePath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.Fatal(ex, $"ShortcutRepository/SaveShortcuts: DisplayMagician doesn't have permissions to create the Shortcuts storage folder {AppShortcutStoragePath}.");
                }
                catch (ArgumentException ex)
                {
                    logger.Fatal(ex, $"ShortcutRepository/SaveShortcuts: DisplayMagician can't create the Shortcuts storage folder {AppShortcutStoragePath} due to an invalid argument.");
                }
                catch (PathTooLongException ex)
                {
                    logger.Fatal(ex, $"ShortcutRepository/SaveShortcuts: DisplayMagician can't create the Shortcuts storage folder {AppShortcutStoragePath} as the path is too long.");
                }
                catch (DirectoryNotFoundException ex)
                {
                    logger.Fatal(ex, $"ShortcutRepository/SaveShortcuts: DisplayMagician can't create the Shortcuts storage folder {AppShortcutStoragePath} as the parent folder isn't there.");
                }
            }
            else
            {
                logger.Debug($"ShortcutRepository/SaveShortcuts: Shortcut folder {AppShortcutStoragePath} exists.");
            }


            try
            {
                logger.Debug($"ShortcutRepository/SaveShortcuts: Converting the objects to JSON format.");

                var json = JsonConvert.SerializeObject(_allShortcuts, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    TypeNameHandling = TypeNameHandling.Auto

                });


                if (!string.IsNullOrWhiteSpace(json))
                {
                    logger.Debug($"ShortcutRepository/SaveShortcuts: Saving the shortcut repository to the {_shortcutStorageJsonFileName}.");

                    File.WriteAllText(_shortcutStorageJsonFileName, json, Encoding.Unicode);
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"ShortcutRepository/SaveShortcuts: Unable to save the shortcut repository to the {_shortcutStorageJsonFileName}.");
            }

            return false;
        }

        public static void IsValidRefresh()
        {
            // We need to refresh the cached answer
            // Get the list of connected devices

            foreach (ShortcutItem loadedShortcut in AllShortcuts)
            {
                loadedShortcut.RefreshValidity();
            }
        }


        // ReSharper disable once CyclomaticComplexity
        public static void RunShortcut(ShortcutItem shortcutToUse, NotifyIcon notifyIcon = null)
        {
            logger.Debug($"ShortcutRepository/RunShortcut: Running the shortcut {shortcutToUse.Name}.");

            // Do some validation to make sure the shortcut is sensible
            // And that we have enough to try and action within the shortcut
            // including checking the Profile in the shortcut is possible
            // (in other words check everything in the shortcut is still valid)
            if (!(shortcutToUse is ShortcutItem))
                return;

            // Check the shortcut is still valid.
            shortcutToUse.RefreshValidity();

            if (shortcutToUse.IsValid == ShortcutValidity.Error || shortcutToUse.IsValid == ShortcutValidity.Warning)
            {
                logger.Error($"ShortcutRepository/RunShortcut: Cannot run the shortcut {shortcutToUse.Name} as it isn't valid");
                string errorReasons = String.Join(", ", (from error in shortcutToUse.Errors select error.Message));
                MessageBox.Show(
                    $"Unable to run the shortcut '{shortcutToUse.Name}': {errorReasons}",
                    @"Cannot run the Shortcut",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

            // Remember the profile we are on now
            bool needToChangeProfiles = false;
            ProfileItem rollbackProfile = ProfileRepository.CurrentProfile;
            if (!rollbackProfile.Equals(shortcutToUse.ProfileToUse))
            {
                logger.Debug($"ShortcutRepository/RunShortcut: We need to change to the {shortcutToUse.ProfileToUse} profile.");
                needToChangeProfiles = true;
            }
            else
            {
                logger.Debug($"ShortcutRepository/RunShortcut: We're already on the {rollbackProfile.Name} profile so no need to change profiles.");
            }
            // Tell the IPC Service we are busy right now, and keep the previous status for later
            //InstanceStatus rollbackInstanceStatus = IPCService.GetInstance().Status;
            //IPCService.GetInstance().Status = InstanceStatus.Busy;

            // Only change profiles if we have to
            if (needToChangeProfiles)
            {
                logger.Info($"ShortcutRepository/RunShortcut: Changing to the {rollbackProfile.Name} profile.");
                // Apply the Profile!
                ApplyProfileResult result = Program.ApplyProfile(shortcutToUse.ProfileToUse);
                if (result == ApplyProfileResult.Error)
                {
                    Console.WriteLine($"ERROR - Cannot apply '{shortcutToUse.ProfileToUse.Name}' Display Profile");
                    logger.Error($"ShortcutRepository/RunShortcut: Cannot apply '{shortcutToUse.ProfileToUse.Name}' Display Profile");
                    return;
                }
                else if (result == ApplyProfileResult.Cancelled)
                {
                    Console.WriteLine($"ERROR - User cancelled applying '{shortcutToUse.ProfileToUse.Name}' Display Profile");
                    logger.Error($"ShortcutRepository/RunShortcut: User cancelled applying '{shortcutToUse.ProfileToUse.Name}' Display Profile");
                    return;
                }
                
            }

            // Get the list of Audio Devices currently connected and active
            bool needToChangeAudioDevice = false;
            CoreAudioDevice rollbackAudioDevice = null;
            double rollbackAudioVolume = 50;
            List<CoreAudioDevice> activeAudioDevices = new List<CoreAudioDevice>();
            bool needToChangeCaptureDevice = false;
            CoreAudioDevice rollbackCaptureDevice = null;
            double rollbackCaptureVolume = 50;
            List<CoreAudioDevice> activeCaptureDevices = new List<CoreAudioDevice>();

            if (_audioController != null)
            {
                try {
                    activeAudioDevices = _audioController.GetPlaybackDevices(DeviceState.Active).ToList();
                    if (activeAudioDevices.Count > 0)
                    {
                        // Change Audio Device (if one specified)
                        if (shortcutToUse.ChangeAudioDevice && !shortcutToUse.AudioDevice.Equals(""))
                        {

                            // record the old audio device
                            rollbackAudioDevice = _audioController.DefaultPlaybackDevice;
                            if (rollbackAudioDevice != null)
                            {
                                rollbackAudioVolume = _audioController.DefaultPlaybackDevice.Volume;
                                if (!rollbackAudioDevice.FullName.Equals(shortcutToUse.AudioDevice))
                                {
                                    logger.Debug($"ShortcutRepository/RunShortcut: We need to change to the {shortcutToUse.AudioDevice} audio device.");
                                    needToChangeAudioDevice = true;
                                }
                                
                            }

                            if (needToChangeAudioDevice)
                            {
                                logger.Info($"ShortcutRepository/RunShortcut: Changing to the {shortcutToUse.AudioDevice} audio device.");

                                foreach (CoreAudioDevice audioDevice in activeAudioDevices)
                                {
                                    if (audioDevice.FullName.Equals(shortcutToUse.AudioDevice))
                                    {
                                        // use the Audio Device
                                        audioDevice.SetAsDefault();
                                    }
                                }
                            }
                            else
                            {
                                logger.Info($"ShortcutRepository/RunShortcut: We're already using the {shortcutToUse.AudioDevice} audio device so no need to change audio devices.");
                            }

                            if (shortcutToUse.SetAudioVolume)
                            {
                                logger.Info($"ShortcutRepository/RunShortcut: Setting {shortcutToUse.AudioDevice} volume level to {shortcutToUse.AudioVolume}%.");
                                Task myTask = new Task(() =>
                                {
                                    _audioController.DefaultPlaybackDevice.SetVolumeAsync(Convert.ToDouble(shortcutToUse.AudioVolume));
                                });
                                myTask.Start();
                                myTask.Wait(2000);
                            }
                            else
                            {
                                logger.Info($"ShortcutRepository/RunShortcut: We don't need to set the {shortcutToUse.AudioDevice} volume level.");
                            }
                        }
                        else
                        {
                            logger.Info($"ShortcutRepository/RunShortcut: Shortcut does not require changing Audio Device.");
                        }
                    }
                    else
                    {
                        logger.Warn($"ShortcutRepository/RunShortcut: No active Audio Devices to use so skipping audio device checks!");
                    }
                }
                catch(Exception ex)
                {
                    logger.Warn(ex, $"ShortcutRepository/RunShortcut: Exception accessing or manipulating Audio Devices!");
                }


                try
                {
                    // Get the list of Audio Devices currently connected
                    activeCaptureDevices = _audioController.GetCaptureDevices(DeviceState.Active).ToList();
                    if (activeCaptureDevices.Count > 0)
                    {

                        // Change capture Device (if one specified)
                        if (shortcutToUse.ChangeCaptureDevice && !shortcutToUse.CaptureDevice.Equals(""))
                        {
                            // record the old microphone device
                            rollbackCaptureDevice = _audioController.DefaultCaptureDevice;
                            if (rollbackCaptureDevice != null)
                            {
                                rollbackCaptureVolume = _audioController.DefaultCaptureDevice.Volume;
                                if (!rollbackCaptureDevice.FullName.Equals(shortcutToUse.CaptureDevice))
                                {
                                    logger.Debug($"ShortcutRepository/RunShortcut: We need to change to the {shortcutToUse.CaptureDevice} capture (microphone) device.");
                                    needToChangeCaptureDevice = true;
                                }
                            }

                            if (needToChangeCaptureDevice) 
                            {
                                logger.Info($"ShortcutRepository/RunShortcut: Changing to the {shortcutToUse.CaptureDevice} capture (microphone) device.");

                                foreach (CoreAudioDevice captureDevice in activeCaptureDevices)
                                {
                                    if (captureDevice.FullName.Equals(shortcutToUse.CaptureDevice))
                                    {
                                        // use the Audio Device
                                        captureDevice.SetAsDefault();
                                    }
                                }
                            }
                            else
                            {
                                logger.Info($"ShortcutRepository/RunShortcut: We're already using the {shortcutToUse.CaptureDevice} capture (microphone) device so no need to change capture devices.");
                            }

                            if (shortcutToUse.SetCaptureVolume)
                            {
                                logger.Info($"ShortcutRepository/RunShortcut: Setting {shortcutToUse.CaptureDevice} capture (microphone) level to {shortcutToUse.CaptureVolume}%.");
                                Task myTask = new Task(() =>
                                {
                                    _audioController.DefaultCaptureDevice.SetVolumeAsync(Convert.ToDouble(shortcutToUse.CaptureVolume));
                                });
                                myTask.Start();
                                myTask.Wait(2000);
                            }
                            else
                            {
                                logger.Info($"ShortcutRepository/RunShortcut: We don't need to set the {shortcutToUse.CaptureDevice} capture (microphone) volume level.");
                            }

                        }
                        else
                        {
                            logger.Info($"ShortcutRepository/RunShortcut: Shortcut does not require changing capture (microphone) device.");
                        }
                    }
                    else
                    {
                        logger.Warn($"ShortcutRepository/RunShortcut: No active Capture Devices to use so skipping capture device checks!");
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutRepository/RunShortcut: Exception accessing or manipulating Capture Devices!");
                }
            }
            else
            {
                logger.Warn($"ShortcutRepository/RunShortcut: CoreAudio Controller is null, so we can't set Audio or Capture Devices!");
            }

            // Set the IP Service status back to what it was
            //IPCService.GetInstance().Status = rollbackInstanceStatus;

            // Now run the pre-start applications
            List<Process> startProgramsToStop = new List<Process>();
            List<StartProgram> startProgramsToStart = shortcutToUse.StartPrograms.Where(program => program.Enabled == true).OrderBy(program => program.Priority).ToList();
            if (startProgramsToStart.Count > 0)
            {
                logger.Info($"ShortcutRepository/RunShortcut: Starting {startProgramsToStart.Count} programs before the main game or executable");
                foreach (StartProgram processToStart in startProgramsToStart)
                {
                    // If required, check whether a process is started already
                    if (processToStart.DontStartIfAlreadyRunning)
                    {
                        logger.Info($"ShortcutRepository/RunShortcut: Checking if process {processToStart.Executable} is already running");
                        Process[] alreadyRunningProcesses = System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processToStart.Executable));
                        if (alreadyRunningProcesses.Length > 0)
                        {
                            logger.Info($"ShortcutRepository/RunShortcut: Process {processToStart.Executable} is already running, so we won't start another one");
                            break;
                        }
                            
                    }

                    // Start the executable
                    logger.Info($"ShortcutRepository/RunShortcut: Starting process {processToStart.Executable}");
                    Process process = null;
                    try
                    {
                        if (processToStart.ExecutableArgumentsRequired)
                            process = System.Diagnostics.Process.Start(processToStart.Executable, processToStart.Arguments);
                        else
                            process = System.Diagnostics.Process.Start(processToStart.Executable);
                        // Record t
                        if (processToStart.CloseOnFinish)
                        {
                            logger.Debug($"ShortcutRepository/RunShortcut: We need to stop {processToStart.Executable} after the main game or executable is closed.");
                            startProgramsToStop.Add(process);
                        }
                        else
                        {
                            logger.Debug($"ShortcutRepository/RunShortcut: No need to stop {processToStart.Executable} after the main game or executable is closed, so we'll just leave it running");
                        }
                    }
                    catch (Win32Exception ex)
                    {
                        logger.Error(ex, $"ShortcutRepository/RunShortcut: Win32Exception starting process {processToStart.Executable}. Windows complained about something while trying to create a new process.");
                    }
                    catch (ObjectDisposedException ex)
                    {
                        logger.Error(ex, $"ShortcutRepository/RunShortcut: Exception starting process {processToStart.Executable}. The object was disposed before we could start the process.");
                    }
                    catch (FileNotFoundException ex)
                    {
                        logger.Error(ex, $"ShortcutRepository/RunShortcut: Win32Exception starting process {processToStart.Executable}. The file wasn't found by DisplayMagician and so we couldn't start it");
                    }
                    catch (InvalidOperationException ex)
                    {
                        logger.Error(ex, $"ShortcutRepository/RunShortcut: Exception starting process {processToStart.Executable}. Method call is invalid for the current state.");
                    }

                }
            }
            else
            {
                logger.Info($"ShortcutRepository/RunShortcut: No programs to start before the main game or executable");
            }

            // Add a status notification icon in the status area
            // but only if we are going to wait for a process to finish
            string oldNotifyText = "";
            bool temporaryNotifyIcon = false;
            ContextMenuStrip oldContextMenuStrip = null;

            // If we're running the shortcut from the ShortcutLibrary
            // then we get given the NotifyIcon through the function
            // parameters i.e. if temporaryIcon is false in that case.
            // This means we need to save the state if the temporaryIcon
            // is false.
            // Conversely, if temporaryIcon is true, then we need 
            // to create a NotifyIncon as MainForm isn't running to create
            // one for us already!
            if (notifyIcon == null)
            {
                logger.Debug($"ShortcutRepository/RunShortcut: We need to create a temporary system tray icon as we're running from a shortcut");
                temporaryNotifyIcon = true;
            }

            if (temporaryNotifyIcon)
            {
                logger.Debug($"ShortcutRepository/RunShortcut: Create a temporary system tray icon (user clicked a desktop shortcut)");

                if (!shortcutToUse.Category.Equals(ShortcutCategory.NoGame))
                {

                    try
                    {
                        notifyIcon = new NotifyIcon
                        {
                            Icon = Properties.Resources.DisplayMagician,
                            Visible = true
                        };
                        Application.DoEvents();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ShortcutRepository/RunShortcut exception: Trying to  {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                        logger.Error(ex, $"ShortcutRepository/RunShortcut exception setting NotifyIcon");
                        // ignored
                    }
                }
            }
            else
            {

                logger.Debug($"ShortcutRepository/RunShortcut: Updating existing system tray icon (we're running a shortcut from within the main application window)");
                // If we reach here then we're running the shortcut
                // from the ShortcutLibrary window, so we need to 
                // remember what the text was so we can return it to
                // normal after we're done!
                oldNotifyText = notifyIcon.Text;
                oldContextMenuStrip = notifyIcon.ContextMenuStrip;
                notifyIcon.ContextMenuStrip = null;
                Application.DoEvents();
            }


            // Now start the main game, and wait if we have to
            if (shortcutToUse.Category.Equals(ShortcutCategory.Application))
            {
                logger.Info($"ShortcutRepository/RunShortcut: Starting the main executable that we wanted to run, and that we're going to monitor and watch");
                // Start the executable

                try
                {
                    Process process = null;
                    if (shortcutToUse.ExecutableArgumentsRequired)
                        process = System.Diagnostics.Process.Start(shortcutToUse.ExecutableNameAndPath, shortcutToUse.ExecutableArguments);
                    else
                        process = System.Diagnostics.Process.Start(shortcutToUse.ExecutableNameAndPath);
                }
                catch (Win32Exception ex)
                {
                    logger.Error(ex, $"ShortcutRepository/RunShortcut: Win32Exception starting main executable process {shortcutToUse.ExecutableNameAndPath}. Windows complained about something while trying to create a new process.");
                }
                catch (ObjectDisposedException ex)
                {
                    logger.Error(ex, $"ShortcutRepository/RunShortcut: Exception starting main executable process {shortcutToUse.ExecutableNameAndPath}. The object was disposed before we could start the process.");
                }
                catch (FileNotFoundException ex)
                {
                    logger.Error(ex, $"ShortcutRepository/RunShortcut: Win32Exception starting main executable process {shortcutToUse.ExecutableNameAndPath}. The file wasn't found by DisplayMagician and so we couldn't start it");
                }
                catch (InvalidOperationException ex)
                {
                    logger.Error(ex, $"ShortcutRepository/RunShortcut: Exception starting main executable process {shortcutToUse.ExecutableNameAndPath}. Method call is invalid for the current state.");
                }

                // Figure out what we want to look for
                string processNameToLookFor;                
                if (shortcutToUse.ProcessNameToMonitorUsesExecutable)
                {
                    // If we are monitoring the same executable we started, then lets do get that name ready
                    processNameToLookFor = System.IO.Path.GetFileNameWithoutExtension(shortcutToUse.ExecutableNameAndPath);
                }
                else
                {
                    // If we are monitoring a different executable, then lets do get that name ready instead
                    processNameToLookFor = System.IO.Path.GetFileNameWithoutExtension(shortcutToUse.DifferentExecutableToMonitor);
                }
                logger.Debug($"ShortcutRepository/RunShortcut: Looking for processes with the name {processNameToLookFor} so that we can monitor them and know when they are closed.");

                // Now look for the thing we're supposed to monitor
                // and wait until it starts up
                List<Process> processesToMonitor = new List<Process>();
                for (int secs = 0; secs <= (shortcutToUse.StartTimeout * 1000); secs += 500)
                {
                    // Look for the processes with the ProcessName we sorted out earlier
                    processesToMonitor = Process.GetProcessesByName(processNameToLookFor).ToList();

                    // If we have found one or more processes then we should be good to go
                    // so let's break
                    if (processesToMonitor.Count > 0)
                    {
                        logger.Debug($"ShortcutRepository/RunShortcut: Found {processesToMonitor.Count} '{processNameToLookFor}' processes to monitor");
                        break;
                    }

                    // Let's wait a little while if we couldn't find
                    // any processes yet
                    Thread.Sleep(500);
                }
                //  make sure we have things to monitor and alert if not
                if (processesToMonitor.Count == 0)
                {
                    logger.Error($"ShortcutRepository/RunShortcut: No '{processNameToLookFor}' processes found before waiting timeout. DisplayMagician was unable to find any processes before the {shortcutToUse.StartTimeout} second timeout");
                }

                // Store the process to monitor for later
                //IPCService.GetInstance().HoldProcessId = processesToMonitor.FirstOrDefault()?.Id ?? 0;
                //IPCService.GetInstance().Status = InstanceStatus.OnHold;

                // Add a status notification icon in the status area
                string notificationText = $"DisplayMagician: Running {shortcutToUse.ExecutableNameAndPath}...";
                if (notificationText.Length >= 64)
                {
                    string thingToRun = shortcutToUse.ExecutableNameAndPath.Substring(0, 35);
                    notifyIcon.Text = $"DisplayMagician: Running {thingToRun}...";
                }
                Application.DoEvents();

                logger.Debug($"ShortcutRepository/RunShortcut: Creating the Windows Toast to notify the user we're going to wait for the executable {shortcutToUse.ExecutableNameAndPath} to close.");
                // Now we want to tell the user we're running an application!
                // Construct the Windows toast content
                ToastContentBuilder tcBuilder = new ToastContentBuilder()
                    .AddToastActivationInfo("notify=runningApplication", ToastActivationType.Foreground)
                    .AddText($"Running {processNameToLookFor}", hintMaxLines: 1)
                    .AddText($"Waiting for all {processNameToLookFor} windows to exit...");
                    //.AddButton("Stop", ToastActivationType.Background, "notify=runningGame&action=stop");
                ToastContent toastContent = tcBuilder.Content;
                // Make sure to use Windows.Data.Xml.Dom
                var doc = new XmlDocument();
                doc.LoadXml(toastContent.GetContent());
                // And create the toast notification
                var toast = new ToastNotification(doc);
                // Remove any other Notifications from us
                DesktopNotifications.DesktopNotificationManagerCompat.History.Clear();
                // And then show this notification
                DesktopNotifications.DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);

                // Wait an extra few seconds to give the application time to settle down
                Thread.Sleep(2000);

                // if we have things to monitor, then we should start to wait for them
                logger.Debug($"ShortcutRepository/RunShortcut: Waiting for application {processNameToLookFor} to exit.");
                if (processesToMonitor.Count > 0)
                {
                    logger.Debug($"ShortcutRepository/RunShortcut: {processesToMonitor.Count} '{processNameToLookFor}' processes are still running");
                    while (true)
                    {
                        processesToMonitor = Process.GetProcessesByName(processNameToLookFor).ToList();

                        // If we have no more processes left then we're done!
                        if (processesToMonitor.Count == 0)
                        {
                            logger.Debug($"ShortcutRepository/RunShortcut: No more '{processNameToLookFor}' processes are still running");
                            break;
                        }

                        // Send a message to windows so that it doesn't think
                        // we're locked and try to kill us
                        System.Threading.Thread.CurrentThread.Join(0);
                        Thread.Sleep(1000);
                    }
                }
                logger.Debug($"ShortcutRepository/RunShortcut: Executable {processNameToLookFor} has exited.");


                logger.Debug($"ShortcutRepository/RunShortcut: Creating a Windows Toast to notify the user that the executable {shortcutToUse.ExecutableNameAndPath} has closed.");
                // Tell the user that the application has closed
                // Construct the toast content
                tcBuilder = new ToastContentBuilder()
                    .AddToastActivationInfo("notify=stopDetected", ToastActivationType.Foreground)
                    .AddText($"{processNameToLookFor} was closed", hintMaxLines: 1)
                    .AddText($"All {processNameToLookFor} processes were shutdown and changes were reverted.");
                toastContent = tcBuilder.Content;
                // Make sure to use Windows.Data.Xml.Dom
                doc = new XmlDocument();
                doc.LoadXml(toastContent.GetContent());
                // And create the toast notification
                toast = new ToastNotification(doc);
                // Remove any other Notifications from us
                DesktopNotifications.DesktopNotificationManagerCompat.History.Clear();
                // And then show it
                DesktopNotifications.DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);


            }
            else if (shortcutToUse.Category.Equals(ShortcutCategory.Game))
            {
                Game gameToRun = null;
                GameLibrary gameLibraryToUse = null;

                // If the game is a Steam Game we check for that
                if (shortcutToUse.GameLibrary.Equals(SupportedGameLibraryType.Steam))
                {
                    // We now need to get the SteamGame info
                    gameLibraryToUse = SteamLibrary.GetLibrary();
                }
                // If the game is a Uplay Uplay Game we check for that
                else if (shortcutToUse.GameLibrary.Equals(SupportedGameLibraryType.Uplay))
                {
                    // We now need to get the Uplay Game  info
                    gameLibraryToUse = UplayLibrary.GetLibrary();
                }
                // If the game is a Uplay Game we check for that
                else if (shortcutToUse.GameLibrary.Equals(SupportedGameLibraryType.Origin))
                {
                    // We now need to get the Uplay Game  info
                    gameLibraryToUse = UplayLibrary.GetLibrary();
                }

                gameToRun = gameLibraryToUse.GetGameById(shortcutToUse.GameAppId);
                logger.Info($"ShortcutRepository/RunShortcut: Starting the {gameToRun.Name} {gameLibraryToUse.GameLibraryName} Game, and then we're going to monitor it to wait for it to close.");

                // If the GameAppID is not null, then we've matched a game! Lets run it.
                if (gameToRun != null)
                {

                    // Now we want to tell the user we're start a game
                    // Construct the Windows toast content
                    ToastContentBuilder tcBuilder = new ToastContentBuilder()
                        .AddToastActivationInfo($"notify=starting{gameLibraryToUse.GameLibraryName}", ToastActivationType.Foreground)
                        .AddText($"Starting {gameLibraryToUse.GameLibraryName}", hintMaxLines: 1)
                        .AddText($"Waiting for {gameLibraryToUse.GameLibraryName} Game Library to start (and update if needed)...");
                    //.AddButton("Stop", ToastActivationType.Background, "notify=runningGame&action=stop");
                    ToastContent toastContent = tcBuilder.Content;
                    // Make sure to use Windows.Data.Xml.Dom
                    var doc = new XmlDocument();
                    doc.LoadXml(toastContent.GetContent());
                    // And create the toast notification
                    var toast = new ToastNotification(doc);
                    // Remove any other Notifications from us
                    DesktopNotifications.DesktopNotificationManagerCompat.History.Clear();
                    // And then show this notification
                    DesktopNotifications.DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);

                    Process gameProcess;
                    if (gameToRun.StartMode.Equals(Game.GameStartMode.URI))
                    {

                        // Prepare to start the game using the URI method
                        string address = "";
                        if (shortcutToUse.GameArgumentsRequired)
                        {
                            address = gameToRun.GetStartURI(shortcutToUse.GameArguments);
                            logger.Debug($"ShortcutRepository/RunShortcut: Shortcut has arguments: {shortcutToUse.GameArguments}");
                        }
                        else
                        {
                            address = gameToRun.GetStartURI("");
                            logger.Debug($"ShortcutRepository/RunShortcut: Shortcut has no arguments");
                        }
                        logger.Debug($"ShortcutRepository/RunShortcut: Game launch URI is {address}");
                        gameProcess = Process.Start(address);
                    }

                    // Delay 500ms
                    Thread.Sleep(500);

                    // Wait for GameLibrary to start
                    for (int secs = 0; secs >= (shortcutToUse.StartTimeout * 1000); secs += 500)
                    {

                        // If we have found one or more processes then we should be good to go
                        // so let's break, and get to the next step....
                        if (gameLibraryToUse.IsRunning)
                        {
                            logger.Debug($"ShortcutRepository/RunShortcut: Found at least one GameLibrary process has started");
                            break;
                        }

                        // Let's wait a little while if we couldn't find
                        // any processes yet
                        Thread.Sleep(500);

                    }

                    // Delay 5secs
                    Thread.Sleep(5000);
                    logger.Debug($"ShortcutRepository/RunShortcut: Pausing to let the game library start the game.");

                    // Store the Process ID for later
                    //IPCService.GetInstance().HoldProcessId = gameLibraryProcesses.FirstOrDefault()?.Id ?? 0;
                    //IPCService.GetInstance().Status = InstanceStatus.OnHold;
                  
                    // At this point, if the user wants to actually monitor a different process, 
                    // then we actually need to monitor that instead
                    if (shortcutToUse.MonitorDifferentGameExe)
                    {
                        // If we are monitoring a different executable rather than the game itself, then lets get that name ready instead
                        string altGameProcessToMonitor = System.IO.Path.GetFileNameWithoutExtension(shortcutToUse.DifferentGameExeToMonitor);

                        // Add a status notification icon in the status area
                        if (gameToRun.Name.Length <= 41)
                            notifyIcon.Text = $"DisplayMagician: Running {gameToRun.Name}...";
                        else
                            notifyIcon.Text = $"DisplayMagician: Running {gameToRun.Name.Substring(0, 41)}...";
                        Application.DoEvents();

                        // Now we want to tell the user we're running a game!
                        // Construct the Windows toast content
                        tcBuilder = new ToastContentBuilder()
                            .AddToastActivationInfo($"notify=running{gameLibraryToUse.GameLibraryName}Game", ToastActivationType.Foreground)
                            .AddText($"Running {shortcutToUse.GameName}", hintMaxLines: 1)
                            .AddText($"Waiting for the {altGameProcessToMonitor} alternative game process to exit...");
                        //.AddButton("Stop", ToastActivationType.Background, "notify=runningGame&action=stop");
                        toastContent = tcBuilder.Content;
                        // Make sure to use Windows.Data.Xml.Dom
                        doc = new XmlDocument();
                        doc.LoadXml(toastContent.GetContent());
                        // And create the toast notification
                        toast = new ToastNotification(doc);
                        // Remove any other Notifications from us
                        DesktopNotifications.DesktopNotificationManagerCompat.History.Clear();
                        // And then show this notification
                        DesktopNotifications.DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);

                        // Now look for the thing we're supposed to monitor
                        // and wait until it starts up
                        List<Process> processesToMonitor = new List<Process>();
                        for (int secs = 0; secs <= (shortcutToUse.StartTimeout * 1000); secs += 500)
                        {
                            // Look for the processes with the ProcessName we sorted out earlier
                            processesToMonitor = Process.GetProcessesByName(altGameProcessToMonitor).ToList();

                            // If we have found one or more processes then we should be good to go
                            // so let's break
                            if (processesToMonitor.Count > 0)
                            {
                                logger.Debug($"ShortcutRepository/RunShortcut: Found {processesToMonitor.Count} '{altGameProcessToMonitor}' processes to monitor");
                                break;
                            }

                            // Let's wait a little while if we couldn't find
                            // any processes yet
                            Thread.Sleep(500);
                        }
                        //  make sure we have things to monitor and alert if not
                        if (processesToMonitor.Count == 0)
                        {
                            logger.Error($"ShortcutRepository/RunShortcut: No Alternative Game Executable '{altGameProcessToMonitor}' processes found before waiting timeout. DisplayMagician was unable to find any processes before the {shortcutToUse.StartTimeout} second timeout");
                        }

                        // if we have things to monitor, then we should start to wait for them
                        logger.Debug($"ShortcutRepository/RunShortcut: Waiting for alternative game proocess {altGameProcessToMonitor} to exit.");
                        if (processesToMonitor.Count > 0)
                        {
                            logger.Debug($"ShortcutRepository/RunShortcut: {processesToMonitor.Count} Alternative Game Executable '{altGameProcessToMonitor}' processes are still running");
                            while (true)
                            {
                                processesToMonitor = Process.GetProcessesByName(altGameProcessToMonitor).ToList();

                                // If we have no more processes left then we're done!
                                if (processesToMonitor.Count == 0)
                                {
                                    logger.Debug($"ShortcutRepository/RunShortcut: No more '{altGameProcessToMonitor}' processes are still running");
                                    break;
                                }

                                // Send a message to windows so that it doesn't think
                                // we're locked and try to kill us
                                System.Threading.Thread.CurrentThread.Join(0);
                                Thread.Sleep(1000);
                            }
                        }
                        logger.Debug($"ShortcutRepository/RunShortcut: Alternative Game Executable {altGameProcessToMonitor} has exited.");

                        // Tell the user that the Alt Game Executable has closed
                        // Construct the toast content
                        tcBuilder = new ToastContentBuilder()
                            .AddToastActivationInfo("notify=stopDetected", ToastActivationType.Foreground)
                            .AddText($"{altGameProcessToMonitor} was closed", hintMaxLines: 1)
                            .AddText($"{altGameProcessToMonitor} alternative game executable was exited.");
                        toastContent = tcBuilder.Content;
                        // Make sure to use Windows.Data.Xml.Dom
                        doc = new XmlDocument();
                        doc.LoadXml(toastContent.GetContent());
                        // And create the toast notification
                        toast = new ToastNotification(doc);
                        // Remove any other Notifications from us
                        DesktopNotifications.DesktopNotificationManagerCompat.History.Clear();
                        // And then show it
                        DesktopNotifications.DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);

                    }
                    else
                    {
                        // we are monitoring the game thats actually running (the most common scenario)

                        // Add a status notification icon in the status area
                        if (gameToRun.Name.Length <= 41)
                            notifyIcon.Text = $"DisplayMagician: Running {gameToRun.Name}...";
                        else
                            notifyIcon.Text = $"DisplayMagician: Running {gameToRun.Name.Substring(0, 41)}...";
                        Application.DoEvents();

                        // Now we want to tell the user we're running a game!
                        // Construct the Windows toast content
                        tcBuilder = new ToastContentBuilder()
                            .AddToastActivationInfo($"notify=running{gameLibraryToUse.GameLibraryName}Game", ToastActivationType.Foreground)
                            .AddText($"Running {shortcutToUse.GameName}", hintMaxLines: 1)
                            .AddText($"Waiting for the {gameLibraryToUse.GameLibraryName} Game {gameToRun.Name} to exit...");
                        //.AddButton("Stop", ToastActivationType.Background, "notify=runningGame&action=stop");
                        toastContent = tcBuilder.Content;
                        // Make sure to use Windows.Data.Xml.Dom
                        doc = new XmlDocument();
                        doc.LoadXml(toastContent.GetContent());
                        // And create the toast notification
                        toast = new ToastNotification(doc);
                        // Remove any other Notifications from us
                        DesktopNotifications.DesktopNotificationManagerCompat.History.Clear();
                        // And then show this notification
                        DesktopNotifications.DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);


                        // Now we know the game library app is running then 
                        // we wait until the game has started running (*allows for updates to occur)
                        for (int secs = 0; secs >= (shortcutToUse.StartTimeout * 1000); secs += 500)
                        {

                            if (gameToRun.IsRunning)
                            {
                                // The game is running! So now we continue processing
                                logger.Debug($"ShortcutRepository/RunShortcut: Found the '{gameToRun.Name}' process has started");
                                break;
                            }

                            // Delay 500ms
                            Thread.Sleep(500);

                        }

                        // Wait 5 more seconds for the game process to spawn	
                        // Thread.Sleep(5000);

                        // This is the main waiting thread!
                        // Wait for the game to exit
                        logger.Debug($"ShortcutRepository/RunShortcut: waiting for {gameLibraryToUse.GameLibraryName} Game {gameToRun.Name} to exit.");
                        while (true)
                        {
                            if (!gameToRun.IsRunning)
                            {
                                logger.Debug($"ShortcutRepository/RunShortcut: {gameLibraryToUse.GameLibraryName} Game {gameToRun.Name} is no longer running (IsRunning is false).");
                                break;
                            }

                            // Send a message to windows so that it doesn't think
                            // we're locked and try to kill us
                            System.Threading.Thread.CurrentThread.Join(0);
                            Thread.Sleep(1000);
                        }
                        Console.WriteLine($"{gameToRun.Name} has exited.");
                        logger.Debug($"ShortcutRepository/RunShortcut: {gameLibraryToUse.GameLibraryName} Game {gameToRun.Name} has exited.");

                        // Tell the user that the Game has closed
                        // Construct the toast content
                        tcBuilder = new ToastContentBuilder()
                            .AddToastActivationInfo("notify=stopDetected", ToastActivationType.Foreground)
                            .AddText($"{shortcutToUse.GameName} was closed", hintMaxLines: 1)
                            .AddText($"{shortcutToUse.GameName} game was exited.");
                        toastContent = tcBuilder.Content;
                        // Make sure to use Windows.Data.Xml.Dom
                        doc = new XmlDocument();
                        doc.LoadXml(toastContent.GetContent());
                        // And create the toast notification
                        toast = new ToastNotification(doc);
                        // Remove any other Notifications from us
                        DesktopNotifications.DesktopNotificationManagerCompat.History.Clear();
                        // And then show it
                        DesktopNotifications.DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);

                    }

                }
                else
                {
                    logger.Error($"ShortcutRepository/RunShortcut: Error starting the {gameToRun.Name} {gameToRun.GameLibrary} Game as the game wasn't found.");
                }

            }


            // Remove the status notification icon from the status area
            // once we've exited the game, but only if its a game or app
            logger.Debug($"ShortcutRepository/RunShortcut: Changing the system tray icon message back to what it was.");
            if (temporaryNotifyIcon)
            {
                
                if (!shortcutToUse.Category.Equals(ShortcutCategory.NoGame))
                {
                    if (notifyIcon != null)
                    {
                        notifyIcon.Visible = false;
                        notifyIcon.Dispose();
                        Application.DoEvents();
                    }
                }
            }
            else
            {
                // If we're running the shortcut from the ShortcutLibrary
                // then we want to reset the NotifyIcon back
                notifyIcon.Text = oldNotifyText;
                notifyIcon.ContextMenuStrip = oldContextMenuStrip;
                Application.DoEvents();
            }

            // Only replace the notification if we're minimised
            if (Program.AppProgramSettings.MinimiseOnStart)
            {
                logger.Debug($"ShortcutRepository/RunShortcut: We're minimised, so we also need to update the Windows notification content");
                // Remind the user that DisplayMagician is running the in background
                // Construct the toast content
                ToastContentBuilder tcBuilder = new ToastContentBuilder()
                    .AddToastActivationInfo("notify=minimiseStart&action=open", ToastActivationType.Foreground)
                    .AddText("DisplayMagician is minimised", hintMaxLines: 1)
                    .AddButton("Open", ToastActivationType.Background, "notify=minimiseStart&action=open");
                ToastContent toastContent = tcBuilder.Content;
                // Make sure to use Windows.Data.Xml.Dom
                var doc = new XmlDocument();
                doc.LoadXml(toastContent.GetContent());

                // And create the toast notification
                var toast = new ToastNotification(doc)
                {
                    SuppressPopup = true
                };

                // And then show it
                DesktopNotifications.DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
            }
            

            // Stop the pre-started startPrograms that we'd started earlier
            if (startProgramsToStop.Count > 0)
            {
                logger.Debug($"ShortcutRepository/RunShortcut: We started {startProgramsToStart.Count} programs before the main executable or game, and now we want to stop {startProgramsToStop.Count } of them");

                // Stop the programs in the reverse order we started them
                foreach (Process processToStop in startProgramsToStop.Reverse<Process>())
                {
                    Console.WriteLine($"Stopping process {processToStop.StartInfo.FileName}");
                    logger.Debug($"ShortcutRepository/RunShortcut: Stopping process {processToStop.StartInfo.FileName}");
                    try
                    {
                        // Stop the program
                        processToStop.CloseMainWindow();
                        processToStop.WaitForExit(5000);
                        if (!processToStop.HasExited)
                        {
                            Console.WriteLine($"- Process {processToStop.StartInfo.FileName} wouldn't stop cleanly. Forcing program close.");
                            logger.Warn($"ShortcutRepository/RunShortcut: Process {processToStop.StartInfo.FileName} wouldn't stop cleanly. Forcing program close.");
                            processToStop.Kill();
                            processToStop.WaitForExit(5000);
                        }
                        processToStop.Close();
                    }
                    catch (Win32Exception ex) {
                        logger.Error(ex, $"ShortcutRepository/RunShortcut: Couldn't access the wait status for a process we're trying to stop.");
                    }
                    catch (InvalidOperationException ex) {
                        logger.Error(ex, $"ShortcutRepository/RunShortcut: Couldn't kill the process as there is no process associated with the Process object.");
                    }
                    catch (SystemException ex)
                    {
                        logger.Error(ex, $"ShortcutRepository/RunShortcut: Couldn't WaitForExit the process as there is no process associated with the Process object (or cannot get the ID from the process handle).");
                    }

                    catch (AggregateException ae) {
                        logger.Error(ae, $"ShortcutRepository/RunShortcut: Got an AggregateException.");
                    }

                }
            }

            // Change Audio Device back (if one specified)
            if (activeAudioDevices.Count > 0)
            {
                if (needToChangeAudioDevice && shortcutToUse.AudioPermanence == ShortcutPermanence.Temporary)
                {
                    logger.Debug($"ShortcutRepository/RunShortcut: Reverting default audio back to {rollbackAudioDevice.Name} audio device");
                    // use the Audio Device
                    rollbackAudioDevice.SetAsDefault();

                    if (shortcutToUse.SetAudioVolume)
                    {
                        logger.Debug($"ShortcutRepository/RunShortcut: Reverting default audio volume back to {shortcutToUse.SetAudioVolume}% volume");
                        Task myTask = new Task(() =>
                        {
                            rollbackAudioDevice.SetVolumeAsync(Convert.ToDouble(rollbackAudioVolume));
                        });
                        myTask.Start();
                        myTask.Wait(2000);
                    }
                }
                else
                {
                    logger.Debug($"ShortcutRepository/RunShortcut: Shortcut did not require changing Audio Device, so no need to change it back.");
                }
            }
            else
            {
                logger.Debug($"ShortcutRepository/RunShortcut: No Audio Devices active, so no need to change them back.");
            }


            // Change Capture Device back (if one specified)
            if (activeCaptureDevices.Count > 0)
            {
                if (needToChangeCaptureDevice && shortcutToUse.CapturePermanence == ShortcutPermanence.Temporary)
                {
                    logger.Debug($"ShortcutRepository/RunShortcut: Reverting default capture (microphone) device back to {rollbackCaptureDevice.Name} capture device");
                    // use the Audio Device
                    rollbackCaptureDevice.SetAsDefault();

                    if (shortcutToUse.SetCaptureVolume)
                    {
                        logger.Debug($"ShortcutRepository/RunShortcut: Reverting default capture (microphone) volume back to {shortcutToUse.SetAudioVolume}% volume");
                        Task myTask = new Task(() =>
                        {
                            rollbackCaptureDevice.SetVolumeAsync(Convert.ToDouble(rollbackCaptureVolume));
                        });
                        myTask.Start();
                        myTask.Wait(2000);
                    }

                }
                else
                {
                    logger.Debug($"ShortcutRepository/RunShortcut: Shortcut did not require changing Capture Device, so no need to change it back.");
                }
            }
            else
            {
                logger.Debug($"ShortcutRepository/RunShortcut: No Capture Devices active, so no need to change them back.");
            }

            // Change back to the original profile only if it is different
            // And if we're temporary
            if (needToChangeProfiles && shortcutToUse.DisplayPermanence == ShortcutPermanence.Temporary)
            {
                logger.Debug($"ShortcutRepository/RunShortcut: Rolling back display profile to {rollbackProfile.Name}");

                ApplyProfileResult result = Program.ApplyProfile(rollbackProfile);
                                
                if (result == ApplyProfileResult.Error)
                {
                    Console.WriteLine($"ERROR - Cannot revert back to '{rollbackProfile.Name}' Display Profile");
                    logger.Error($"ShortcutRepository/RunShortcut: Error rolling back display profile to {rollbackProfile.Name}");
                    return;
                }
                else if (result == ApplyProfileResult.Cancelled)
                {
                    Console.WriteLine($"ERROR - User cancelled revert back to '{rollbackProfile.Name}' Display Profile");
                    logger.Error($"ShortcutRepository/RunShortcut: User cancelled rolling back display profile to {rollbackProfile.Name}");
                    return;
                }

            }
            else
            {
                logger.Debug($"ShortcutRepository/RunShortcut: Shortcut did not require changing Display Profile, so no need to change it back.");
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

