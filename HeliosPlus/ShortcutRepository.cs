using HeliosPlus.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.IconLib;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeliosPlus
{

    class ShortcutRepository
    {
        #region Class Variables
        // Common items to the class
        private static List<Shortcut> _allShortcuts = new List<Shortcut>();
        public static Version Version = new Version(1, 0, 0);
        // Other constants that are useful
        private static string _shortcutStorageJsonPath = Path.Combine(Program.AppDataPath, $"Shortcuts");
        private static string _shortcutStorageJsonFileName = Path.Combine(_shortcutStorageJsonPath, $"Shortcuts_{Version.ToString(2)}.json");
        private static uint _lastShortcutId;
        #endregion

        #region Instance Variables
        // Individual items per class instance
        #endregion


        #region Class Constructors
        public ShortcutRepository()
        {
            // Load the Shortcuts from storage
            if (LoadShortcuts() && ShortcutCount > 0)
            {
                // Work out the starting NextShortcutId value
                long max = _allShortcuts.Max<Shortcut>(item => item.Id);
                _lastShortcutId = Convert.ToUInt32(max);
            } else
                _lastShortcutId = 0;
        }

        public ShortcutRepository(Shortcut shortcut) : this()
        {
            if (shortcut is Shortcut)
                AddShortcut(shortcut);
        }
        #endregion

        #region Class Properties
        public static List<Shortcut> AllShortcuts
        {
            get
            {
                if (_allShortcuts == null)
                    // Load the Shortcuts from storage
                    if (LoadShortcuts() && ShortcutCount > 0)
                    {
                        // Work out the starting NextShortcutId value
                        long max = _allShortcuts.Max<Shortcut>(item => item.Id);
                        _lastShortcutId = Convert.ToUInt32(max);
                    }
                    else
                        _lastShortcutId = 0;

                return _allShortcuts;
            }
        }


        public static int ShortcutCount
        {
            get
            {
                return _allShortcuts.Count;
            }
        }

        #endregion

        #region Class Methods
        public static bool AddShortcut(Shortcut shortcut)
        {
            if (!(shortcut is Shortcut))
                return false;

            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (ContainsShortcut(shortcut))
            {
                // We update the existing Shortcut with the data over
                Shortcut shortcutToUpdate = GetShortcut(shortcut.Id);
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

        public static bool RemoveShortcut(Shortcut shortcut)
        {
            if (!(shortcut is Shortcut))
                return false;

            // Remove the Shortcut Icons from the Cache
            List<Shortcut> shortcutsToRemove = _allShortcuts.FindAll(item => item.Id.Equals(shortcut.Id));
            foreach (Shortcut shortcutToRemove in shortcutsToRemove)
            {
                try
                {
                    File.Delete(shortcutToRemove.SavedShortcutIconCacheFilename);
                }
                catch
                {
                    // TODO check and report
                }
            }

            // Remove the shortcut from the list.
            int numRemoved = _allShortcuts.RemoveAll(item => item.Id.Equals(shortcut.Id));

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


        public static bool RemoveShortcut(string shortcutName)
        {
            if (String.IsNullOrWhiteSpace(shortcutName))
                return false;

            // Remove the Shortcut Icons from the Cache
            List<Shortcut> shortcutsToRemove = _allShortcuts.FindAll(item => item.Name.Equals(shortcutName));
            foreach (Shortcut shortcutToRemove in shortcutsToRemove)
            {
                try
                {
                    File.Delete(shortcutToRemove.SavedShortcutIconCacheFilename);
                }
                catch
                {
                    // TODO check and report
                }
            }

            // Remove the shortcut from the list.
            int numRemoved = _allShortcuts.RemoveAll(item => item.Name.Equals(shortcutName));

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

        public static bool RemoveShortcut(uint shortcutId)
        {
            if (shortcutId == 0)
                return false;

            // Remove the Shortcut Icons from the Cache
            List<Shortcut> shortcutsToRemove = _allShortcuts.FindAll(item => item.Id.Equals(shortcutId));
            foreach (Shortcut shortcutToRemove in shortcutsToRemove)
            {
                try
                {
                    File.Delete(shortcutToRemove.SavedShortcutIconCacheFilename);
                }
                catch
                {
                    // TODO check and report
                }
            }

            // Remove the shortcut from the list.
            int numRemoved = _allShortcuts.RemoveAll(item => item.Id.Equals(shortcutId));

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


        public static bool ContainsShortcut(Shortcut shortcut)
        {
            if (!(shortcut is Shortcut))
                return false;

            foreach (Shortcut testShortcut in _allShortcuts)
            {
                if (testShortcut.Id.Equals(shortcut.Id))
                    return true;
            }

            return false;
        }

        public static bool ContainsShortcut(string shortcutName)
        {
            if (String.IsNullOrWhiteSpace(shortcutName))
                return false;

            foreach (Shortcut testShortcut in _allShortcuts)
            {
                if (testShortcut.Name.Equals(shortcutName))
                    return true;
            }

            return false;

        }

        public static bool ContainsShortcut(uint shortcutId)
        {
            if (shortcutId == 0)
                return true;

            foreach (Shortcut testShortcut in _allShortcuts)
            {
                if (testShortcut.Id.Equals(shortcutId))
                    return true;
            }

            return false;

        }


        public static Shortcut GetShortcut(string shortcutName)
        {
            if (String.IsNullOrWhiteSpace(shortcutName))
                return null;

            foreach (Shortcut testShortcut in _allShortcuts)
            {
                if (testShortcut.Name.Equals(shortcutName))
                    return testShortcut;
            }

            return null;
        }

        public static Shortcut GetShortcut(uint shortcutId)
        {
            if (shortcutId == 0)
                return null;

            foreach (Shortcut testShortcut in _allShortcuts)
            {
                if (testShortcut.Id.Equals(shortcutId))
                    return testShortcut;
            }

            return null;
        }

        public static uint GetNextAvailableShortcutId()
        {
            return ++_lastShortcutId;
        }



        private static bool LoadShortcuts()
        {

            if (File.Exists(_shortcutStorageJsonFileName))
            {
                var json = File.ReadAllText(_shortcutStorageJsonFileName, Encoding.Unicode);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    List<Shortcut> shortcuts = new List<Shortcut>();
                    try
                    {
                        _allShortcuts = JsonConvert.DeserializeObject<List<Shortcut>>(json, new JsonSerializerSettings
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
                    foreach (Shortcut updatedShortcut in _allShortcuts)
                    {
                        foreach (Profile profile in Profile.AllSavedProfiles)
                        {

                            if (profile.Name.Equals(updatedShortcut.ProfileName))
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
            return true;
        }

        private static bool SaveShortcuts()
        {

            if (!Directory.Exists(_shortcutStorageJsonPath))
            {
                try
                {
                    Directory.CreateDirectory(_shortcutStorageJsonPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to create Shortcut folder {_shortcutStorageJsonPath}: " + ex.Message);

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
                Console.WriteLine($"Unable to save Shortcut JSON file {_shortcutStorageJsonFileName}: " + ex.Message);
            }

            return false;
        }

        private static void SaveShortcutIconToCache(Shortcut shortcut)
        {

            // Only add the rest of the options if the permanence is temporary
            if (shortcut.Permanence == ShortcutPermanence.Temporary)
            {
                // Only add this set of options if the shortcut is to an standalone application
                if (shortcut.Category == ShortcutCategory.Application)
                {
                    // Work out the name of the shortcut we'll save.
                    shortcut.SavedShortcutIconCacheFilename = Path.Combine(_shortcutStorageJsonPath, String.Concat(@"executable-", Program.GetValidFilename(shortcut.Name).ToLower(CultureInfo.InvariantCulture), "-", Path.GetFileNameWithoutExtension(shortcut.ExecutableNameAndPath), @".ico"));

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
                        shortcut.SavedShortcutIconCacheFilename = Path.Combine(_shortcutStorageJsonPath, String.Concat(@"steam-", Program.GetValidFilename(shortcut.Name).ToLower(CultureInfo.InvariantCulture), "-", shortcut.GameAppId.ToString(), @".ico"));

                    }
                    else if (shortcut.GameLibrary == SupportedGameLibrary.Uplay)
                    {
                        // Work out the name of the shortcut we'll save.
                        shortcut.SavedShortcutIconCacheFilename = Path.Combine(_shortcutStorageJsonPath, String.Concat(@"uplay-", Program.GetValidFilename(shortcut.Name).ToLower(CultureInfo.InvariantCulture), "-", shortcut.GameAppId.ToString(), @".ico"));
                    }

                }

            }
            // Only add the rest of the options if the shortcut is permanent 
            else
            {
                // Work out the name of the shortcut we'll save.
                shortcut.SavedShortcutIconCacheFilename = Path.Combine(_shortcutStorageJsonPath, String.Concat(@"permanent-", Program.GetValidFilename(shortcut.Name).ToLower(CultureInfo.InvariantCulture), @".ico"));
            }

            MultiIcon shortcutIcon;
            try
            {
                shortcutIcon = new ProfileIcon(shortcut.ProfileToUse).ToIconOverlay(shortcut.OriginalIconPath);
                shortcutIcon.Save(shortcut.SavedShortcutIconCacheFilename, MultiIconFormat.ICO);
            }
            catch (Exception ex)
            {
                // If we fail to create an icon based on the original executable or game
                // Then we use the standard HeliosPlus profile one.
                shortcutIcon = new ProfileIcon(shortcut.ProfileToUse).ToIcon();
                shortcutIcon.Save(shortcut.SavedShortcutIconCacheFilename, MultiIconFormat.ICO);
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
