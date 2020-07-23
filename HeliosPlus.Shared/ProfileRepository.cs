using HeliosPlus.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.IconLib;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using WindowsDisplayAPI.DisplayConfig;
using HeliosPlus.Shared.Resources;
using Newtonsoft.Json;
using NvAPIWrapper.Mosaic;
using NvAPIWrapper.Native.Mosaic;
using HeliosPlus.Shared.Topology;
using System.Drawing;
using System.Drawing.Imaging;
using WindowsDisplayAPI;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Resources;
using System.Net.NetworkInformation;
using NvAPIWrapper.Mosaic;
using NvAPIWrapper.Native.Mosaic;

namespace HeliosPlus.Shared
{

    public static class ProfileRepository
    {
        #region Class Variables
        // Common items to the class
        private static List<ProfileItem> _allProfiles = null;
        public static Version Version = new Version(1, 0, 0);
        // Other constants that are useful
        public static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HeliosPlus");
        public static string AppIconPath = Path.Combine(AppDataPath, $"Icons");
        public static string AppHeliosPlusIconFilename = Path.Combine(AppIconPath, @"HeliosPlus.ico");
        private static string _profileStorageJsonPath = Path.Combine(AppDataPath, $"Profiles");
        private static string _profileStorageJsonFileName = Path.Combine(_profileStorageJsonPath, $"DisplayProfiles_{Version.ToString(2)}.json");
        private static uint _lastProfileId;
        private static ProfileItem _currentProfile;
        private static List<Display> _availableDisplays;
        private static List<UnAttachedDisplay> _unavailableDisplays;

        #endregion

        #region Class Constructors
        static ProfileRepository()
        {
            // Load the Profiles from storage
            LoadProfiles();
        }
        #endregion

        #region Class Properties
        public static List<ProfileItem> AllProfiles
        {
            get
            {
                if (_allProfiles == null)
                    // Load the Profiles from storage if they need to be
                    LoadProfiles();
                return _allProfiles;
            }
        }

        public static ProfileItem CurrentProfile
        {
            get 
            {
                UpdateActiveProfile();
                return _currentProfile;
            }
            set
            {
                if (value is ProfileItem)
                {
                    _currentProfile = value;
                    // And if we have the _originalBitmap we can also save the Bitmap overlay, but only if the ProfileToUse is set
                    //if (_originalBitmap is Bitmap)
                    //    _shortcutBitmap = ToBitmapOverlay(_originalBitmap, ProfileToUse.ProfileTightestBitmap, 256, 256);
                }
            }
        }

        public static int ProfileCount
        {
            get
            {
                if (_allProfiles == null)
                    // Load the Profiles from storage if they need to be
                    LoadProfiles();
                return _allProfiles.Count;
            }
        }

        #endregion

        #region Class Methods
        public static bool AddProfile(ProfileItem Profile)
        {
            if (!(Profile is ProfileItem))
                return false;

            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (!ContainsProfile(Profile))
            {
                // Add the Profile to the list of Profiles
                _allProfiles.Add(Profile);
            }


            /*            // Doublecheck if it already exists
                        // Because then we just update the one that already exists
                        if (ContainsProfile(Profile))
                        {
                            // We update the existing Profile with the data over
                            ProfileItem ProfileToUpdate = GetProfile(Profile.UUID);
                            Profile.CopyTo(ProfileToUpdate);
                        }
                        else
                        {
                            // Add the Profile to the list of Profiles
                            _allProfiles.Add(Profile);
                        }
            */
            //Doublecheck it's been added
            if (ContainsProfile(Profile))
            {
                // Generate the Profile Icon ready to be used
                SaveProfileIconToCache(Profile);

                // Save the Profiles JSON as it's different
                SaveProfiles();

                return true;
            }
            else
                return false;

        }

        public static bool RemoveProfile(ProfileItem Profile)
        {
            if (!(Profile is ProfileItem))
                return false;

            // Remove the Profile Icons from the Cache
            List<ProfileItem> ProfilesToRemove = _allProfiles.FindAll(item => item.UUID.Equals(Profile.UUID));
            foreach (ProfileItem ProfileToRemove in ProfilesToRemove)
            {
                try
                {
                    File.Delete(ProfileToRemove.SavedProfileIconCacheFilename);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ProfileRepository/RemoveProfile exception: {ex.Message}: {ex.InnerException}");
                    // TODO check and report
                }
            }

            // Remove the Profile from the list.
            int numRemoved = _allProfiles.RemoveAll(item => item.UUID.Equals(Profile.UUID));

            if (numRemoved == 1)
            {
                SaveProfiles();
                return true;
            }
            else if (numRemoved == 0)
                return false;
            else
                throw new ProfileRepositoryException();
        }


        public static bool RemoveProfile(string ProfileName)
        {
            if (String.IsNullOrWhiteSpace(ProfileName))
                return false;

            // Remove the Profile Icons from the Cache
            List<ProfileItem> ProfilesToRemove = _allProfiles.FindAll(item => item.Name.Equals(ProfileName));
            foreach (ProfileItem ProfileToRemove in ProfilesToRemove)
            {
                try
                {
                    File.Delete(ProfileToRemove.SavedProfileIconCacheFilename);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ProfileRepository/RemoveProfile exception 2: {ex.Message}: {ex.InnerException}");
                    // TODO check and report
                }
            }

            // Remove the Profile from the list.
            int numRemoved = _allProfiles.RemoveAll(item => item.Name.Equals(ProfileName));

            if (numRemoved == 1)
            {
                SaveProfiles();
                return true;
            }
            else if (numRemoved == 0)
                return false;
            else
                throw new ProfileRepositoryException();

        }

        public static bool RemoveProfile(uint ProfileId)
        {
            if (ProfileId == 0)
                return false;

            // Remove the Profile Icons from the Cache
            List<ProfileItem> ProfilesToRemove = _allProfiles.FindAll(item => item.UUID.Equals(ProfileId));
            foreach (ProfileItem ProfileToRemove in ProfilesToRemove)
            {
                try
                {
                    File.Delete(ProfileToRemove.SavedProfileIconCacheFilename);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ProfileRepository/RemoveProfile exception 3: {ex.Message}: {ex.InnerException}");
                    // TODO check and report
                }
            }

            // Remove the Profile from the list.
            int numRemoved = _allProfiles.RemoveAll(item => item.UUID.Equals(ProfileId));

            if (numRemoved == 1)
            {
                SaveProfiles();
                return true;
            }    
            else if (numRemoved == 0)
                return false;
            else
                throw new ProfileRepositoryException();
        }


        public static bool ContainsProfile(ProfileItem Profile)
        {
            if (!(Profile is ProfileItem))
                return false;

            foreach (ProfileItem testProfile in _allProfiles)
            {
                if (testProfile.UUID.Equals(Profile.UUID))
                    return true;
            }

            return false;
        }

        public static bool ContainsProfile(string ProfileNameOrId)
        {
            if (String.IsNullOrWhiteSpace(ProfileNameOrId))
                return false;

            if (ProfileItem.IsValidUUID(ProfileNameOrId))
                foreach (ProfileItem testProfile in _allProfiles)
                {
                    if (testProfile.UUID.Equals(ProfileNameOrId))
                        return true;
                }
            else
                foreach (ProfileItem testProfile in _allProfiles)
                {
                    if (testProfile.Name.Equals(ProfileNameOrId))
                        return true;
                }

            return false;

        }

        public static ProfileItem GetProfile(string ProfileNameOrId)
        {
            if (String.IsNullOrWhiteSpace(ProfileNameOrId))
                return null;

            if (ProfileItem.IsValidUUID(ProfileNameOrId))
                foreach (ProfileItem testProfile in _allProfiles)
                {
                    if (testProfile.UUID.Equals(ProfileNameOrId))
                        return testProfile;
                }
            else
                foreach (ProfileItem testProfile in _allProfiles)
                {
                    if (testProfile.Name.Equals(ProfileNameOrId))
                        return testProfile;
                }

            return null;
        }

        public static bool RenameProfile(ProfileItem profile, string renamedName)
        {
            if (!(profile is ProfileItem))
                return false;

            if (!IsValidFilename(renamedName))
                return false;

            profile.Name = GetValidFilename(renamedName);

            // If it's been added to the list of AllProfiles
            // then we also need to reproduce the Icons
            if (ContainsProfile(profile))
            {


                // rename the old Profile Icon to the new name
                //string newSavedProfileIconCacheFilename = Path.Combine(_profileStorageJsonPath, String.Concat(@"profile-", profile.UUID, @".ico"));
                //File.Move(profile.SavedProfileIconCacheFilename, newSavedProfileIconCacheFilename);

                // Then update the profile too
                //profile.SavedProfileIconCacheFilename = newSavedProfileIconCacheFilename;

                // Save the Profiles JSON as it's different now
                SaveProfiles();

                return true;
            }
            else
                return false;

        }


        public static void UpdateActiveProfile()
        {

            ProfileItem activeProfile = new ProfileItem
            {
                Name = "Current Display Profile",
                Viewports = PathInfo.GetActivePaths().Select(info => new ProfileViewport(info)).ToArray(),
                ProfileIcon = new ProfileIcon(_currentProfile),
                ProfileBitmap = _currentProfile.ProfileIcon.ToBitmap(256, 256)
            };

            foreach (ProfileItem loadedProfile in ProfileRepository.AllProfiles)
            {
                if (activeProfile.Equals(loadedProfile))
                {
                    _currentProfile = loadedProfile;
                    return;
                }
            }

            _currentProfile = activeProfile;

        }


        public static ProfileItem GetActiveProfile()
        {
            UpdateActiveProfile();

            if (!(_currentProfile is ProfileItem))
                return null;
            return _currentProfile;
        }

        public static bool IsActiveProfile(ProfileItem profile)
        {
            UpdateActiveProfile();

            if (!(_currentProfile is ProfileItem))
                return false;

            if (!(profile is ProfileItem))
                return false;

            if (profile.Equals(_currentProfile))
                return true;

            return false;
        }

        private static bool LoadProfiles()
        {

            if (File.Exists(_profileStorageJsonFileName))
            {
                var json = File.ReadAllText(_profileStorageJsonFileName, Encoding.Unicode);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    //List<ProfileItem> profiles = new List<ProfileItem>();
                    try
                    {
                        _allProfiles = JsonConvert.DeserializeObject<List<ProfileItem>>(json, new JsonSerializerSettings
                        {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Include,
                            TypeNameHandling = TypeNameHandling.Auto,
                            ObjectCreationHandling = ObjectCreationHandling.Replace
                        });
                    }
                    catch (Exception ex)
                    {
                        // ignored
                        Console.WriteLine($"ProfileRepository/LoadProfiles exception: {ex.Message}: {ex.InnerException}");
                        Console.WriteLine($"Unable to load Profiles from JSON file {_profileStorageJsonFileName}: " + ex.Message);
                    }

                    ProfileItem myCurrentProfile = new ProfileItem
                    {
                        Name = "Current Display Profile",
                        Viewports = PathInfo.GetActivePaths().Select(info => new ProfileViewport(info)).ToArray()
                    };

                    _currentProfile = myCurrentProfile;

                    // Lookup all the Profile Names in the Saved Profiles
                    foreach (ProfileItem loadedProfile in _allProfiles)
                    {
                        // Save a profile Icon to the profile
/*                        loadedProfile.ProfileIcon = new ProfileIcon(loadedProfile);
                        loadedProfile.ProfileBitmap = loadedProfile.ProfileIcon.ToBitmap(256, 256);
*/
                        if (ProfileRepository.IsActiveProfile(loadedProfile))
                            _currentProfile = loadedProfile;

                    }
                }
            } else
            {
                // If we get here, then we don't have any profiles saved!
                // So we gotta start from scratch
                // Create a new profile based on our current display settings
                ProfileItem myCurrentProfile = new ProfileItem
                {
                    Name = "Current Display Profile",
                    Viewports = PathInfo.GetActivePaths().Select(info => new ProfileViewport(info)).ToArray()
                };

                _currentProfile = myCurrentProfile;

                // Save a profile Icon to the profile
                _currentProfile.ProfileIcon = new ProfileIcon(_currentProfile);
                _currentProfile.ProfileBitmap = _currentProfile.ProfileIcon.ToBitmap(256, 256);
            }
            return true;
        }

        public static bool SaveProfiles()
        {

            if (!Directory.Exists(_profileStorageJsonPath))
            {
                try
                {
                    Directory.CreateDirectory(_profileStorageJsonPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ProfileRepository/SaveProfiles exception: {ex.Message}: {ex.InnerException}");
                    Console.WriteLine($"Unable to create Profile folder {_profileStorageJsonPath}: " + ex.Message);

                }
            }

            try
            {
                var json = JsonConvert.SerializeObject(_allProfiles, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    TypeNameHandling = TypeNameHandling.Auto

                });


                if (!string.IsNullOrWhiteSpace(json))
                {
                    File.WriteAllText(_profileStorageJsonFileName, json, Encoding.Unicode);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfileRepository/SaveProfiles exception 2: {ex.Message}: {ex.InnerException}");
                Console.WriteLine($"Unable to save Profile JSON file {_profileStorageJsonFileName}: " + ex.Message);
            }

            return false;
        }

        private static void SaveProfileIconToCache(ProfileItem profile)
        {

            // Work out the name of the Profile we'll save.
            profile.SavedProfileIconCacheFilename = Path.Combine(_profileStorageJsonPath, String.Concat(@"profile-", profile.UUID, @".ico"));

            MultiIcon ProfileIcon;
            try
            {
                ProfileIcon = profile.ProfileIcon.ToIcon();
                ProfileIcon.Save(profile.SavedProfileIconCacheFilename, MultiIconFormat.ICO);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfileRepository/SaveProfileIconToCache exception: {ex.Message}: {ex.InnerException}");
                // If we fail to create an icon based on the Profile, then we use the standard HeliosPlus profile one.
                // Which is created on program startup.
                File.Copy(AppHeliosPlusIconFilename, profile.SavedProfileIconCacheFilename);

            }
        }

        public static void UpdateCurrentProfile()
        {
            _currentProfile = new ProfileItem
            {
                Name = "Current Display Profile",
                Viewports = PathInfo.GetActivePaths().Select(info => new ProfileViewport(info)).ToArray()
            };
        }

        public static bool IsValidFilename(string testName)
        {
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(testName)) { return false; };

            return true;
        }

        public static string GetValidFilename(string uncheckedFilename)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                uncheckedFilename = uncheckedFilename.Replace(c.ToString(), "");
            }
            return uncheckedFilename;
        }

        #endregion

    }


    [global::System.Serializable]
    public class ProfileRepositoryException : Exception
    {
        public ProfileRepositoryException() { }
        public ProfileRepositoryException(string message) : base(message) { }
        public ProfileRepositoryException(string message, Exception inner) : base(message, inner) { }
        protected ProfileRepositoryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


}
