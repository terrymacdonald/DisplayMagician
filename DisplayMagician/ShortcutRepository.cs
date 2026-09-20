using DisplayMagician.GameLibraries;
using DisplayMagician.Processes;
using DisplayMagician.UIForms;
using DisplayMagicianShared;
using DisplayMagicianShared.Windows;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using IUserAgentRepositoryConnection = DisplayMagician.Contracts.IUserAgentRepositoryConnection;
using RepositoryCommitRequest = DisplayMagician.Contracts.RepositoryCommitRequest;
using RepositoryCommitResult = DisplayMagician.Contracts.RepositoryCommitResult;
using RepositoryKind = DisplayMagician.Contracts.RepositoryKind;
using RepositorySnapshot = DisplayMagician.Contracts.RepositorySnapshot;

namespace DisplayMagician
{
    public struct ShortcutFile
    {
        public string ShortcutFileVersion;
        public DateTime LastUpdated;
        public List<ShortcutItem> Shortcuts;

        public override bool Equals(object obj) => obj is ShortcutFile other && this.Equals(other);
        public bool Equals(ShortcutFile other)
        => ShortcutFileVersion.Equals(other.ShortcutFileVersion) &&
           LastUpdated.Equals(other.LastUpdated) &&
           Shortcuts.SequenceEqual(other.Shortcuts);
        public override int GetHashCode()
        {
            return (ShortcutFileVersion, LastUpdated, Shortcuts).GetHashCode();
        }

        public static bool operator ==(ShortcutFile lhs, ShortcutFile rhs) => lhs.Equals(rhs);

        public static bool operator !=(ShortcutFile lhs, ShortcutFile rhs) => !(lhs == rhs);
    }

    public static class ShortcutRepository
    {
        #region Class Variables
        // Common items to the class
        private static List<ShortcutItem> _allShortcuts = new List<ShortcutItem>();
        private static bool _shortcutsLoaded = false;
        //private static bool _cancelWait = false;
        // Other constants that are useful
        private static string AppShortcutStoragePath = Path.Combine(Program.AppDataPath, $"Shortcuts");
        private static string _shortcutFileVersion = "6";
        private static string _shortcutStorageJsonFileName = "Shortcuts.json";
        private static string _shortcutStorageJsonFullFileName = Path.Combine(AppShortcutStoragePath, _shortcutStorageJsonFileName);
        private static string uuidV4Regex = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static IUserAgentRepositoryConnection _userAgentRepositoryConnection;
        private static long _userAgentRepositoryRevision;
        #endregion

        #region Class Constructors
        static ShortcutRepository()
        {
            // Loading is deferred until either AllShortcuts is read or the
            // repository is connected to the User Agent.
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

        public static string ShortcutStorageFileName
        {
            get => _shortcutStorageJsonFullFileName;
        }

        public static string ShortcutStorageFileVersion
        {
            get => _shortcutFileVersion;
        }

        /*public static bool CancelWait {
            get => _cancelWait;
            set => _cancelWait = value;
        }*/

        #endregion

        #region Class Methods
        public static void ConfigureStoragePath(string applicationDataPath)
        {
            if (string.IsNullOrWhiteSpace(applicationDataPath))
                throw new ArgumentException("An application data path is required.", nameof(applicationDataPath));

            AppShortcutStoragePath = Path.Combine(Path.GetFullPath(applicationDataPath), "Shortcuts");
            _shortcutStorageJsonFullFileName = Path.Combine(AppShortcutStoragePath, _shortcutStorageJsonFileName);
            _allShortcuts = new List<ShortcutItem>();
            _shortcutsLoaded = false;
            Directory.CreateDirectory(AppShortcutStoragePath);
        }

        /// <summary>
        /// Loads this repository's local ShortcutItem cache from the User Agent.
        /// Subsequent saves are committed back through the same connection.
        /// </summary>
        public static void ConnectToUserAgent(IUserAgentRepositoryConnection userAgentRepositoryConnection)
        {
            _userAgentRepositoryConnection = userAgentRepositoryConnection ?? throw new ArgumentNullException(nameof(userAgentRepositoryConnection));
            RepositorySnapshot snapshot = _userAgentRepositoryConnection.GetRepositorySnapshot(RepositoryKind.Shortcuts);
            if (snapshot.Repository != RepositoryKind.Shortcuts)
                throw new InvalidOperationException("The User Agent returned the wrong repository snapshot for shortcuts.");

            LoadShortcutsFromJson(snapshot.Json);
            _userAgentRepositoryRevision = snapshot.Revision;
            _shortcutsLoaded = true;
            logger.Debug("ShortcutRepository/ConnectToUserAgent: Loaded the shortcut cache from the User Agent.");
        }

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

                // Remove any hotkeys related to this shortcut if they exist
                Program.AppDirectInputManager.RemoveHotkeysByUUID(shortcutToRemove.UUID.ToString());

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

            //logger.Trace($"ShortcutRepository/ContainsShortcut: Checking whether {shortcut.Name} exists in our shortcut repository");

            if (!(shortcut is ShortcutItem))
                return false;

            foreach (ShortcutItem testShortcut in _allShortcuts)
            {
                if (testShortcut.UUID.Equals(shortcut.UUID, StringComparison.OrdinalIgnoreCase))
                {
                    //logger.Trace($"ShortcutRepository/ContainsShortcut: {shortcut.Name} does exist in our shortcut repository");
                    return true;
                }
            }
            logger.Trace($"ShortcutRepository/ContainsShortcut: Shortcut with name {shortcut.Name} doesn't exist in our shortcut repository");
            return false;
        }

        public static bool ContainsShortcut(string shortcutNameOrUuid)
        {

            //logger.Trace($"ShortcutRepository/ContainsShortcut2: Checking whether {shortcutNameOrUuid} exists in our shortcut repository");

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
                        //logger.Trace($"ShortcutRepository/ContainsShortcut2: Shortcut with UUID {shortcutNameOrUuid} does exist in our shortcut repository");
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
                        //logger.Trace($"ShortcutRepository/ContainsShortcut2: Shortcut with name {shortcutNameOrUuid} does exist in our shortcut repository");
                        return true;
                    }
                }

            }

            logger.Trace($"ShortcutRepository/ContainsShortcut2: Shortcut with name {shortcutNameOrUuid} doesn't exist in our shortcut repository");
            return false;

        }


        public static ShortcutItem GetShortcut(string shortcutNameOrUuid)
        {
            //logger.Trace($"ShortcutRepository/GetShortcut: Finding and returning {shortcutNameOrUuid} if it exists in our shortcut repository");

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
                        //logger.Trace($"ShortcutRepository/GetShortcut: Returning shortcut with UUID {shortcutNameOrUuid}");
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
                        //logger.Trace($"ShortcutRepository/GetShortcut: Returning shortcut with Name {shortcutNameOrUuid}");
                        return testShortcut;
                    }
                }

            }

            logger.Warn($"ShortcutRepository/GetShortcut: No shortcut was found to return with UUID or Name {shortcutNameOrUuid}");
            return null;

        }

        public static string GetShortcutName(string shortcutNameOrUuid)
        {
            logger.Trace($"ShortcutRepository/GetShortcutName: Finding and returning {shortcutNameOrUuid} if it exists in our shortcut repository");

            if (String.IsNullOrWhiteSpace(shortcutNameOrUuid))
            {
                logger.Error($"ShortcutRepository/GetShortcutName: Shortcut to get was empty or only whitespace");
                return string.Empty;
            }

            Match match = Regex.Match(shortcutNameOrUuid, uuidV4Regex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (ShortcutItem testShortcut in _allShortcuts)
                {
                    if (testShortcut.UUID.Equals(shortcutNameOrUuid, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Trace($"ShortcutRepository/GetShortcutName: Returning shortcut '{testShortcut.Name}' with UUID {shortcutNameOrUuid}");
                        return testShortcut.Name;
                    }
                }

            }
            else
            {
                foreach (ShortcutItem testShortcut in _allShortcuts)
                {
                    if (testShortcut.Name.Equals(shortcutNameOrUuid, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Trace($"ShortcutRepository/GetShortcutName: Returning shortcut '{testShortcut.Name}' with Name {shortcutNameOrUuid}");
                        return testShortcut.Name;
                    }
                }

            }

            logger.Warn($"ShortcutRepository/GetShortcutName: No shortcut was found to return with UUID or Name {shortcutNameOrUuid}");
            return string.Empty;

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
                    testShortcut.ProfileToUse = new DisplayProfileView { Id = newProfile.UUID, Name = newProfile.Name };
                    testShortcut.AutoSuggestShortcutName();
                }
            }

            SaveShortcuts();
            IsValidRefresh();
            return true;
        }

        public static bool CopyShortcut(ShortcutItem shortcut, out ShortcutItem copiedShortcut)
        {

            logger.Trace($"ShortcutRepository/CopyShortcut: Checking whether {shortcut.Name} exists in our shortcut repository");

            copiedShortcut = new ShortcutItem();

            if (!(shortcut is ShortcutItem))
                return false;


            if (shortcut.CopyTo(copiedShortcut,false))
            {
                // Copy worked!
                // Add the shortcut to the list of shortcuts
                _allShortcuts.Add(copiedShortcut);

                //Doublecheck it's been added
                if (ContainsShortcut(copiedShortcut))
                {
                    // Select the copied shortcut

                    // Save the shortcuts JSON as it's different
                    SaveShortcuts();
                    IsValidRefresh();
                    return true;
                }
                else
                    return false;
            }
            else
            {
                // Copy failed
                return false;
            }
        }

        public static bool LoadShortcuts()
        {

            logger.Debug($"ShortcutRepository/LoadShortcuts: Loading shortcuts from {_shortcutStorageJsonFullFileName} into the Shortcut Repository");

            _shortcutsLoaded = false;

            // Figure out if we need to upgrade the shortcuts file
            if (Utils.OldFileVersionsExist(AppShortcutStoragePath,"Shortcuts_*.json"))
            {
                logger.Debug($"ShortcutRepository/LoadShortcuts: Upgrading the older shortcuts file to the latest version.");
                if (!Utils.UpgradeOldFileVersions(AppShortcutStoragePath, "Shortcuts_*.json", _shortcutStorageJsonFileName))
                {
                    logger.Error($"ShortcutRepository/LoadShortcuts: Error upgrading the older shortcuts file to the latest version.");
                }
                else
                {
                    logger.Trace($"ShortcutRepository/LoadShortcuts: Upgraded the older shortcuts file to the latest version.");
                }
            }
            else
            {
                logger.Debug($"ShortcutRepository/LoadShortcuts: No need to upgrade the older shortcuts file to the latest version.");
            }


            if (File.Exists(_shortcutStorageJsonFullFileName))
            {
                string json = "";
                try
                {
                    json = File.ReadAllText(_shortcutStorageJsonFullFileName, Encoding.Unicode);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ShortcutRepository/LoadShortcuts: Tried to read the JSON file {_shortcutStorageJsonFullFileName} to memory but File.ReadAllTextthrew an exception.");
                }

                if (!string.IsNullOrWhiteSpace(json))
                {
                    // Firstly perform any modifications we need to do to update the JSON structure
                    // to handle old versions of the file that need updating. Done with a simple regex replace
                    try
                    {

                        // If the shortcuts file doesn't have "ProcessPriority" in it, then we need to add it
                        if (!Regex.Match(json, @"""ProcessPriority""").Success)
                        {
                            // Add the ProcessPriority line as null so its in there at least and won't stop the json load
                            json = Regex.Replace(json, "    \"DifferentExecutableToMonitor\"", "    \"ProcessPriority\": null,\n    \"DifferentExecutableToMonitor\"");
                        }

                    }
                    catch(Exception ex)
                    {
                        // problem updating JSON
                        logger.Error(ex, $"ShortcutRepository/LoadShortcuts: Tried to update the JSON in the {_shortcutStorageJsonFullFileName} but the Regex Replace threw an exception.");
                    }

#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    List<ShortcutItem> shortcuts = new List<ShortcutItem>();
#pragma warning restore IDE0059 // Unnecessary assignment of a value

                    List<string> jsonErrors = new List<string>();
                    try
                    {


                        JsonSerializerSettings mySerializerSettings = new JsonSerializerSettings
                        {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Populate,
                            TypeNameHandling = TypeNameHandling.Auto,
                            SerializationBinder = DisplayMagicianSerializationBinder.Instance,
                            ObjectCreationHandling = ObjectCreationHandling.Replace,
                            Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                            {
                                jsonErrors.Add($"JSON.net Error: {args.ErrorContext.Error.Source}:{args.ErrorContext.Error.StackTrace} - {args.ErrorContext.Error.Message} | InnerException:{args.ErrorContext.Error.InnerException?.Source}:{args.ErrorContext.Error.InnerException?.StackTrace} - {args.ErrorContext.Error.InnerException?.Message}");
                                args.ErrorContext.Handled = true;
                            },
                        };

                        ShortcutFile shortcutFile = JsonConvert.DeserializeObject<ShortcutFile>(json, mySerializerSettings);

                        if (shortcutFile.Shortcuts == null)
                        {
                            throw new Exception("ShortcutRepository/LoadShortcuts: The Shortcuts file was an older file format, so we need to upgrade it.");
                        }

                        _allShortcuts = shortcutFile.Shortcuts;

                    }
                    catch (JsonReaderException ex)
                    {
                        // If there is a error in the JSON format
                        if (ex.HResult == -2146233088)
                        {
                            SharedLogger.logger.Error(ex, $"ShortcutRepository/LoadShortcuts: JSONReaderException - The Shortcuts file {_shortcutStorageJsonFullFileName} contains a syntax error. Please check the file for correctness with a JSON validator.");
                        }
                        else
                        {
                            SharedLogger.logger.Error(ex, $"ShortcutRepository/LoadShortcuts: JSONReaderException while trying to process the Shortcuts json data file {_shortcutStorageJsonFullFileName} but JsonConvert threw an exception.");
                        }
                        MessageBox.Show($"The Game Shortcuts file {_shortcutStorageJsonFullFileName} contains a syntax error. Please check the file for correctness with a JSON validator.", "Error loading the Game Shortcuts", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception)
                    {
                        // If we get here then we may need to import the shortcuts from the old format without the Shortcut Version
                        try
                        {


                            JsonSerializerSettings mySerializerSettings = new JsonSerializerSettings
                            {
                                MissingMemberHandling = MissingMemberHandling.Ignore,
                                NullValueHandling = NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.Populate,
                                TypeNameHandling = TypeNameHandling.Auto,
                                SerializationBinder = DisplayMagicianSerializationBinder.Instance,
                                ObjectCreationHandling = ObjectCreationHandling.Replace,
                                Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                                {
                                    jsonErrors.Add($"JSON.net Error: {args.ErrorContext.Error.Source}:{args.ErrorContext.Error.StackTrace} - {args.ErrorContext.Error.Message} | InnerException:{args.ErrorContext.Error.InnerException?.Source}:{args.ErrorContext.Error.InnerException?.StackTrace} - {args.ErrorContext.Error.InnerException?.Message}");
                                    args.ErrorContext.Handled = true;
                                },
                            };

                            _allShortcuts = JsonConvert.DeserializeObject<List<ShortcutItem>>(json, mySerializerSettings);

                            // Save the Shortcuts JSON as it's different now, and we want to save the upgrade!
                            SaveShortcuts();


                        }
                        catch (JsonReaderException nex)
                        {
                            // If there is a error in the JSON format
                            if (nex.HResult == -2146233088)
                            {
                                SharedLogger.logger.Error(nex, $"ShortcutRepository/LoadShortcuts: JSONReaderException - The Shortcuts file {_shortcutStorageJsonFullFileName} contains a syntax error. Please check the file for correctness with a JSON validator.");
                            }
                            else
                            {
                                SharedLogger.logger.Error(nex, $"ShortcutRepository/LoadShortcuts: JSONReaderException while trying to process the Shortcuts json data file {_shortcutStorageJsonFullFileName} but JsonConvert threw an exception.");
                            }
                            MessageBox.Show($"The Game Shortcuts file {_shortcutStorageJsonFullFileName} contains a syntax error. Please check the file for correctness with a JSON validator.", "Error loading the Game Shortcuts", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (Exception nex)
                        {
                            // If we get here then we may need to import the shortcuts from the old format without the Shortcut Version


                            logger.Error(nex, $"ShortcutRepository/LoadShortcuts: Tried to parse the JSON in the {_shortcutStorageJsonFullFileName} but the JsonConvert threw an exception. There is an error in the Shortcut JSON file!");
                            MessageBox.Show($"The Game Shortcuts file {_shortcutStorageJsonFullFileName} contains a syntax error. Please check the file for correctness with a JSON validator.", "Error loading the Game Shortcuts", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            throw new Exception("ShortcutRepository/LoadShortcuts: Tried to parse the JSON in the {_shortcutStorageJsonFileName} but the JsonConvert threw an exception. There is an error in the Shortcut JSON file!");
                        }
                    }

                    // If we have any JSON.net errors, then we need to records them in the logs
                    if (jsonErrors.Count > 0)
                    {
                        foreach (string jsonError in jsonErrors)
                            {
                                logger.Error($"ShortcutRepository/LoadShortcuts: JSON.Net Error found while loading {_shortcutStorageJsonFullFileName}: {jsonError}");
                            }
                    }

                    logger.Trace($"ShortcutRepository/LoadShortcuts: Loaded {_allShortcuts.Count} shortcuts from {_shortcutStorageJsonFullFileName} Shortcut JSON file");


                    // Lookup all the Profile Names in the Saved Profiles
                    // and link the profiles to the Shortcuts as we only
                    // store the profile names to allow users to uodate profiles
                    // separately from the shortcuts
                    logger.Debug($"ShortcutRepository/LoadShortcuts: Connecting Shortcut profile names to the real profile objects");
                    foreach (ShortcutItem updatedShortcut in _allShortcuts)
                    {
                        if (String.IsNullOrWhiteSpace(updatedShortcut.ProfileUUID))
                        {
                            logger.Error($"ShortcutRepository/LoadShortcuts: Shortcut '{updatedShortcut.Name}' profile UUID is null or whitespace! Skipping this processing this entry, and setting ProfileToUse to null.");
                            updatedShortcut.ProfileToUse = null;
                            continue;
                        }

                        if (updatedShortcut.ProfileUUID.Equals(ProfileItem.SkipDisplayChangeUUID, StringComparison.OrdinalIgnoreCase))
                        {
                            logger.Debug($"ShortcutRepository/LoadShortcuts: Shortcut '{updatedShortcut.Name}' uses 'No Display Change' profile. Setting ProfileToUse to null as we don't want to link it to a profile.");
                            updatedShortcut.ProfileToUse = null;
                            continue;
                        }

                        bool foundProfile = false;
                        foreach (ProfileItem profile in ProfileRepository.AllProfiles)
                        {
                            try
                            {
                                if (!String.IsNullOrWhiteSpace(profile.UUID) && profile.UUID.Equals(updatedShortcut.ProfileUUID))
                                {
                                    // And assign the matching Profile if we find it.
                                    updatedShortcut.ProfileToUse = new DisplayProfileView { Id = profile.UUID, Name = profile.Name };
                                    foundProfile = true;
                                    logger.Debug($"ShortcutRepository/LoadShortcuts: Found the profile with UUID {updatedShortcut.ProfileUUID} and linked it to a profile!");
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, $"ShortcutRepository/LoadShortcuts: Error looking for Profile UUID {updatedShortcut.ProfileUUID} in the list of profiles in the Profile Repository.");
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
                    logger.Trace($"ShortcutRepository/LoadShortcuts: Sorting the Shortcuts alphabetically.");
                    _allShortcuts.Sort();
                }
                else
                {
                    logger.Debug($"ShortcutRepository/LoadShortcuts: The {_shortcutStorageJsonFullFileName} shortcut JSON file exists but is empty! So we're going to treat it as if it didn't exist.");
                }
            }
            else
            {
                logger.Debug($"ShortcutRepository/LoadShortcuts: Couldn't find the {_shortcutStorageJsonFullFileName} shortcut JSON file that contains the Shortcuts. Didn't load any shortcuts at all.");
            }
            logger.Trace($"ShortcutRepository/LoadShortcuts: Checking validity of the loaded shortcuts to make sure they're ok to use now");
            try
            {
                _shortcutsLoaded = true;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"ShortcutRepository/LoadShortcuts: Exception while checking the validity of the loaded shortcuts to make sure they're ok to use");
                return false;
            }
        }

        public static bool SaveShortcuts()
        {
            logger.Debug($"ShortcutRepository/SaveShortcuts: Attempting to save the shortcut repository to the {_shortcutStorageJsonFullFileName}.");

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


            List<string> jsonErrors = new List<string>();

            try
            {
                logger.Debug($"ShortcutRepository/SaveShortcuts: Converting the objects to JSON format.");

                JsonSerializerSettings mySerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    TypeNameHandling = TypeNameHandling.Auto,
                    SerializationBinder = DisplayMagicianSerializationBinder.Instance,
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                    {
                        jsonErrors.Add($"JSON.net Error: {args.ErrorContext.Error.Source}:{args.ErrorContext.Error.StackTrace} - {args.ErrorContext.Error.Message} | InnerException:{args.ErrorContext.Error.InnerException?.Source}:{args.ErrorContext.Error.InnerException?.StackTrace} - {args.ErrorContext.Error.InnerException?.Message}");
                        args.ErrorContext.Handled = true;
                    },
                };

                ShortcutFile shortcutFile = new ShortcutFile
                {
                    ShortcutFileVersion = _shortcutFileVersion,
                    LastUpdated = DateTime.Now,
                    Shortcuts = _allShortcuts
                };

                var json = JsonConvert.SerializeObject(shortcutFile, Formatting.Indented, mySerializerSettings);


                if (!string.IsNullOrWhiteSpace(json) && _userAgentRepositoryConnection != null)
                {
                    RepositoryCommitResult commitResult = _userAgentRepositoryConnection.CommitRepositorySnapshot(new RepositoryCommitRequest
                    {
                        Repository = RepositoryKind.Shortcuts,
                        ExpectedRevision = _userAgentRepositoryRevision,
                        Json = json
                    });

                    if (commitResult.Snapshot == null)
                    {
                        logger.Error("ShortcutRepository/SaveShortcuts: The User Agent did not return a shortcut snapshot after the commit.");
                        return false;
                    }

                    _userAgentRepositoryRevision = commitResult.Snapshot.Revision;
                    if (commitResult.WasConflict)
                    {
                        LoadShortcutsFromJson(commitResult.Snapshot.Json);
                        _shortcutsLoaded = true;
                        logger.Warn("ShortcutRepository/SaveShortcuts: The shortcut cache was stale. Reloaded the User Agent version instead of overwriting it.");
                        return false;
                    }

                    logger.Debug("ShortcutRepository/SaveShortcuts: Committed the shortcut cache through the User Agent.");
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(json))
                {
                    logger.Debug($"ShortcutRepository/SaveShortcuts: Saving the shortcut repository to the {_shortcutStorageJsonFullFileName}.");

                    AtomicFile.WriteAllText(_shortcutStorageJsonFullFileName, json, Encoding.Unicode);
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"ShortcutRepository/SaveShortcuts: Unable to save the shortcut repository to the {_shortcutStorageJsonFullFileName}.");
            }

            // If we have any JSON.net errors, then we need to record them in the logs
            if (jsonErrors.Count > 0)
            {
                foreach (string jsonError in jsonErrors)
                {
                    logger.Error($"ShortcutRepository/SaveShortcuts: {jsonError}");
                }
            }

            return false;
        }

        private static void LoadShortcutsFromJson(string json)
        {
            _allShortcuts = new List<ShortcutItem>();

            if (string.IsNullOrWhiteSpace(json))
                return;

            try
            {
                JsonSerializerSettings serializerSettings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    TypeNameHandling = TypeNameHandling.Auto,
                    SerializationBinder = DisplayMagicianSerializationBinder.Instance,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };

                ShortcutFile shortcutFile = JsonConvert.DeserializeObject<ShortcutFile>(json, serializerSettings);
                if (shortcutFile.Shortcuts == null)
                    throw new InvalidDataException("The User Agent returned shortcuts in an unsupported format.");

                _allShortcuts = shortcutFile.Shortcuts;
                LinkShortcutsToProfiles();
                _allShortcuts.Sort();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "ShortcutRepository/LoadShortcutsFromJson: The User Agent returned unreadable shortcut data.");
                throw new InvalidDataException("The User Agent returned unreadable shortcut data.", ex);
            }
        }

        private static void LinkShortcutsToProfiles()
        {
            foreach (ShortcutItem shortcut in _allShortcuts)
            {
                if (string.IsNullOrWhiteSpace(shortcut.ProfileUUID) || shortcut.ProfileUUID.Equals(ProfileItem.SkipDisplayChangeUUID, StringComparison.OrdinalIgnoreCase))
                {
                    shortcut.ProfileToUse = null;
                    continue;
                }

                ProfileItem profile = ProfileRepository.AllProfiles.FirstOrDefault(profile =>
                    !string.IsNullOrWhiteSpace(profile.UUID) &&
                    profile.UUID.Equals(shortcut.ProfileUUID, StringComparison.OrdinalIgnoreCase));
                shortcut.ProfileToUse = profile == null ? null : new DisplayProfileView { Id = profile.UUID, Name = profile.Name };
            }
        }

        public static void IsValidRefresh()
        {
            // We need to refresh the cached answer
            // Get the list of connected devices
            logger.Trace($"ShortcutRepository/IsValidRefresh: IsValidRefresh starting.");
            foreach (ShortcutItem loadedShortcut in AllShortcuts)
            {
                logger.Trace($"ShortcutRepository/IsValidRefresh: Running RefreshValidity on Shortcut {loadedShortcut.Name}");
                loadedShortcut.RefreshValidity();
            }
            logger.Trace($"ShortcutRepository/IsValidRefresh: IsValidRefresh completed.");
        }

        private static ProcessPriorityClass TranslatePriorityClassToClass(ProcessPriority processPriority)
        {
            ProcessPriorityClass wantedPriorityClass = ProcessPriorityClass.Normal;
            switch (processPriority.ToString("G"))
            {
                case "High":
                    wantedPriorityClass = ProcessPriorityClass.High;
                    break;
                case "AboveNormal":
                    wantedPriorityClass = ProcessPriorityClass.AboveNormal;
                    break;
                case "Normal":
                    wantedPriorityClass = ProcessPriorityClass.Normal;
                    break;
                case "BelowNormal":
                    wantedPriorityClass = ProcessPriorityClass.BelowNormal;
                    break;
                case "Idle":
                    wantedPriorityClass = ProcessPriorityClass.Idle;
                    break;
                default:
                    wantedPriorityClass = ProcessPriorityClass.Normal;
                    break;
            }
            return wantedPriorityClass;

        }


        private static void ShowStatusToast(ToastContentBuilder builder)
        {
            var toastContent = builder.Content;
            var doc = new Windows.Data.Xml.Dom.XmlDocument();
            doc.LoadXml(toastContent.GetContent());
            var toast = new ToastNotification(doc);
            toast.SuppressPopup = false;
            ToastNotificationManagerCompat.History.Clear();
            ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }

        private static void SetTrayText(MainForm form, string text)
        {
            if (form.InvokeRequired)
                form.BeginInvoke((MethodInvoker)delegate { form.UpdateNotifyIconText(text); });
            else
                form.UpdateNotifyIconText(text);
        }

        #endregion

    }

    [global::System.Serializable]
    public class ShortcutRepositoryException : Exception
    {
        public ShortcutRepositoryException() { }
        public ShortcutRepositoryException(string message) : base(message) { }
        public ShortcutRepositoryException(string message, Exception inner) : base(message, inner) { }
    }

}

