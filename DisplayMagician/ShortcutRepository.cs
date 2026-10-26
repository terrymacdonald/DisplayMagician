using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private static string _shortcutFileVersion = "8";
        private static string uuidV4Regex = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static IUserAgentRepositoryConnection _userAgentRepositoryConnection;
        private static long _userAgentRepositoryRevision;
        private static string _lastSavedJson = string.Empty;
        public static string LastSaveErrorMessage { get; private set; } = string.Empty;
        public static bool LastSaveWasConflict { get; private set; }
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
            _lastSavedJson = snapshot.Json;
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
                if (!SaveShortcuts()) return false;
                SaveCommittedShortcutIcon(shortcut);
                IsValidRefresh();

                return true;
            }
            else
                return false;

        }

        public static bool UpdateShortcut(ShortcutItem shortcut)
        {
            int index = _allShortcuts.FindIndex(item => item.UUID.Equals(shortcut.UUID, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                LastSaveErrorMessage = "This shortcut was deleted elsewhere. Close the editor and refresh the library.";
                return false;
            }
            // Positions are captured through their own action, never edited in this form.
            // Preserve a newer capture/clear when the user retries an editor conflict.
            shortcut.WindowPlacements = new List<DisplayMagician.ConfigurationDefinitions.ShortcutWindowPlacementDefinition>(_allShortcuts[index].WindowPlacements);
            _allShortcuts[index] = shortcut;
            if (!SaveShortcuts()) return false;
            SaveCommittedShortcutIcon(shortcut);
            IsValidRefresh();
            return true;
        }

        public static bool RemoveShortcut(ShortcutItem shortcut)
        {
            logger.Trace($"ShortcutRepository/RemoveShortcut: Removing shortcut {shortcut.Name} if it exists in our shortcut repository");

            if (!(shortcut is ShortcutItem))
                return false;

            // Remove the shortcut from the list.
            int numRemoved = _allShortcuts.RemoveAll(item => item.UUID.Equals(shortcut.UUID, StringComparison.OrdinalIgnoreCase));

            if (numRemoved == 1)
            {
                if (!SaveShortcuts()) return false;
                try
                {
                    if (!string.IsNullOrWhiteSpace(shortcut.SavedShortcutIconCacheFilename))
                        File.Delete(shortcut.SavedShortcutIconCacheFilename);
                    Program.AppDirectInputManager?.RemoveHotkeysByUUID(shortcut.UUID);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "ShortcutRepository/RemoveShortcut: Shortcut {0} was deleted but its local icon or hotkey could not be cleaned up.", shortcut.UUID);
                }
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
            if (String.IsNullOrWhiteSpace(shortcutNameOrUuid)) return false;
            bool isId = Regex.IsMatch(shortcutNameOrUuid, uuidV4Regex, RegexOptions.IgnoreCase);
            List<ShortcutItem> matches = _allShortcuts.FindAll(item => isId
                ? item.UUID.Equals(shortcutNameOrUuid, StringComparison.OrdinalIgnoreCase)
                : item.Name.Equals(shortcutNameOrUuid, StringComparison.OrdinalIgnoreCase));
            if (matches.Count == 0) return false;
            if (matches.Count != 1) throw new ShortcutRepositoryException();
            return RemoveShortcut(matches[0]);
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

        public static bool RenameShortcutProfile(DisplayProfileView newProfile)
        {
            logger.Debug($"ShortcutRepository/RenameShortcutProfile: Renaming the profile in any shortcuts containing the old profile name");

            if (newProfile == null || string.IsNullOrWhiteSpace(newProfile.Id))
                return false;

            foreach (ShortcutItem testShortcut in ShortcutRepository.AllShortcuts)
            {
                if (testShortcut.ProfileUUID.Equals(newProfile.Id, StringComparison.OrdinalIgnoreCase) && testShortcut.AutoName)
                {
                    logger.Debug($"ShortcutRepository/RenameShortcutProfile: Renaming {testShortcut.Name} shortcut's profile to {newProfile.Name} since the original profile has just been renamed.");
                    testShortcut.ProfileToUse = new DisplayProfileView { Id = newProfile.Id, Name = newProfile.Name };
                    testShortcut.AutoSuggestShortcutName();
                }
            }

            if (!SaveShortcuts()) return false;
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
                    if (!SaveShortcuts()) return false;
                    SaveCommittedShortcutIcon(copiedShortcut);
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

        private static void SaveCommittedShortcutIcon(ShortcutItem shortcut)
        {
            try { shortcut.SaveShortcutIconToCache(); }
            catch (Exception ex)
            {
                logger.Warn(ex, "ShortcutRepository/SaveCommittedShortcutIcon: Shortcut {0} was saved but its local icon could not be cached.", shortcut.UUID);
            }
        }

        public static bool LoadShortcuts()
        {
            if (_userAgentRepositoryConnection == null)
            {
                logger.Error("ShortcutRepository/LoadShortcuts: The desktop shortcut cache is not connected to the User Agent.");
                return false;
            }

            RepositorySnapshot snapshot = _userAgentRepositoryConnection.GetRepositorySnapshot(RepositoryKind.Shortcuts);
            if (snapshot.Repository != RepositoryKind.Shortcuts)
            {
                logger.Error("ShortcutRepository/LoadShortcuts: The User Agent returned the wrong repository snapshot for shortcuts.");
                return false;
            }

            LoadShortcutsFromJson(snapshot.Json);
            _userAgentRepositoryRevision = snapshot.Revision;
            _lastSavedJson = snapshot.Json;
            _shortcutsLoaded = true;
            logger.Debug("ShortcutRepository/LoadShortcuts: Loaded the desktop shortcut cache from the User Agent.");
            return true;
        }

        public static bool SaveShortcuts()
        {
            LastSaveWasConflict = false;
            LastSaveErrorMessage = "The shortcut changes could not be saved. Check the Control Service connection and try again.";
            if (_userAgentRepositoryConnection == null)
            {
                logger.Error("ShortcutRepository/SaveShortcuts: The desktop shortcut cache is not connected to the User Agent.");
                LoadShortcutsFromJson(_lastSavedJson);
                return false;
            }

            List<string> jsonErrors = new List<string>();
            bool saved = false;

            try
            {
                logger.Debug($"ShortcutRepository/SaveShortcuts: Converting the objects to JSON format.");

                JsonSerializerSettings mySerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    TypeNameHandling = TypeNameHandling.Auto,
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

                foreach (ShortcutItem shortcut in _allShortcuts)
                {
                    if (string.IsNullOrWhiteSpace(shortcut.SavedShortcutIconCacheFilename))
                        shortcut.SavedShortcutIconCacheFilename = Path.Combine(Program.AppIconPath, "ShortcutCache", $"{shortcut.UUID}.ico");
                    for (int index = 0; index < shortcut.StartPrograms.Count; index++)
                    {
                        StartProgram program = shortcut.StartPrograms[index];
                        if (!string.IsNullOrWhiteSpace(program.Id)) continue;
                        program.Id = Guid.NewGuid().ToString("D");
                        shortcut.StartPrograms[index] = program;
                    }
                }

                var json = JsonConvert.SerializeObject(shortcutFile, Formatting.Indented, mySerializerSettings);

                if (jsonErrors.Count > 0)
                {
                    foreach (string jsonError in jsonErrors)
                        logger.Error($"ShortcutRepository/SaveShortcuts: {jsonError}");

                    return false;
                }

                if (!string.IsNullOrWhiteSpace(json))
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
                        _lastSavedJson = commitResult.Snapshot.Json;
                        LastSaveWasConflict = true;
                        LastSaveErrorMessage = "The shortcuts changed elsewhere. Your change was not saved. The library has refreshed; please try again.";
                        LoadShortcutsFromJson(commitResult.Snapshot.Json);
                        _shortcutsLoaded = true;
                        logger.Warn("ShortcutRepository/SaveShortcuts: The shortcut cache was stale. Reloaded the User Agent version instead of overwriting it.");
                        return false;
                    }

                    logger.Debug("ShortcutRepository/SaveShortcuts: Committed the shortcut cache through the User Agent.");
                    _lastSavedJson = commitResult.Snapshot.Json;
                    LastSaveErrorMessage = string.Empty;
                    saved = true;
                    return true;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "ShortcutRepository/SaveShortcuts: Unable to commit the shortcut cache through the User Agent.");
            }
            finally
            {
                // Keep menus and other clients of this cache on the last authoritative
                // snapshot. The editor retains its separate draft after a failed save.
                if (!saved) LoadShortcutsFromJson(_lastSavedJson);
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
                if (string.IsNullOrWhiteSpace(shortcut.ProfileUUID) || shortcut.ProfileUUID.Equals(ShortcutItem.SkipDisplayChangeProfile.Id, StringComparison.OrdinalIgnoreCase))
                {
                    shortcut.ProfileToUse = null;
                    continue;
                }

                DisplayProfileView profile = DesktopProfileViewCache.Get(shortcut.ProfileUUID);
                shortcut.ProfileToUse = profile == null ? null : new DisplayProfileView { Id = profile.Id, Name = profile.Name };
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

