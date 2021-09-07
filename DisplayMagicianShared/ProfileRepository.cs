using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.IconLib;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DisplayMagicianShared.AMD;
using DisplayMagicianShared.NVIDIA;
using DisplayMagicianShared.Windows;
using System.Runtime.Serialization;

namespace DisplayMagicianShared
{
    // This enum sets the video card mode used within DisplayMagician
    // It effectively controls what video card library is used to store profiles on the computer
    // We look up the PCI vendor ID for the video cards, and then we look for them in the order from most commonly
    // sold video card to the least, followed by the generic 'catch-all' windows mode.
    public enum VIDEO_MODE : Int32
    {
        WINDOWS = 0,
        NVIDIA = 1,
        AMD = 2,
    }

    public enum FORCED_VIDEO_MODE : Int32
    {
        WINDOWS = 0,
        NVIDIA = 1,
        AMD = 2,
        DETECT = 99,
    }

    public enum ApplyProfileResult
    {
        Successful,
        Cancelled,
        Error
    }

    public static class ProfileRepository
    {
        #region Class Variables
        // Common items to the class
        private static List<ProfileItem> _allProfiles = new List<ProfileItem>();
        public static Dictionary<string, bool> _profileWarningLookup = new Dictionary<string, bool>();
        private static bool _profilesLoaded = false;
        public static Version _version = new Version(2, 0, 0);
        private static ProfileItem _currentProfile;
        private static List<string> _connectedDisplayIdentifiers = new List<string>();
        private static bool notifiedEDIDErrorToUser = false;
        private static AMDLibrary amdLibrary;
        private static NVIDIALibrary nvidiaLibrary;
        private static WinLibrary winLibrary;
        // Make the default video mode Windows
        private static VIDEO_MODE _currentVideoMode = VIDEO_MODE.WINDOWS;
        private static FORCED_VIDEO_MODE _forcedVideoMode = FORCED_VIDEO_MODE.DETECT;

        // Other constants that are useful
        public static string AppDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        public static string AppIconPath = System.IO.Path.Combine(AppDataPath, $"Icons");
        public static string AppDisplayMagicianIconFilename = System.IO.Path.Combine(AppIconPath, @"DisplayMagician.ico");
        private static readonly string AppProfileStoragePath = System.IO.Path.Combine(AppDataPath, $"Profiles");
        private static readonly string _profileStorageJsonFileName = System.IO.Path.Combine(AppProfileStoragePath, $"DisplayProfiles_{_version.ToString(2)}.json");
        


        #endregion

        #region Class Constructors
        static ProfileRepository()
        {
            
            try
            {
                // Create the Profile Storage Path if it doesn't exist so that it's avilable for all the program
                if (!Directory.Exists(AppProfileStoragePath))
                {
                    SharedLogger.logger.Debug($"ProfileRepository/ProfileRepository: Creating the Profiles storage folder {AppProfileStoragePath}.");
                    Directory.CreateDirectory(AppProfileStoragePath);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                SharedLogger.logger.Fatal(ex, $"ProfileRepository/ProfileRepository: DisplayMagician doesn't have permissions to create the Profiles storage folder {AppProfileStoragePath}.");
            }
            catch (ArgumentException ex)
            {
                SharedLogger.logger.Fatal(ex, $"ProfileRepository/ProfileRepository: DisplayMagician can't create the Profiles storage folder {AppProfileStoragePath} due to an invalid argument.");
            }
            catch (PathTooLongException ex)
            {
                SharedLogger.logger.Fatal(ex, $"ProfileRepository/ProfileRepository: DisplayMagician can't create the Profiles storage folder {AppProfileStoragePath} as the path is too long.");
            }
            catch (DirectoryNotFoundException ex)
            {
                SharedLogger.logger.Fatal(ex, $"ProfileRepository/ProfileRepository: DisplayMagician can't create the Profiles storage folder {AppProfileStoragePath} as the parent folder isn't there.");
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex, $"ProfileRepository/ProfileRepository: Exception creating the Profiles storage folder.");
            }
        }
        #endregion

        #region Class Properties
        public static List<ProfileItem> AllProfiles
        {
            get
            {
                if (!_profilesLoaded)
                    // Load the Profiles from storage if they need to be
                    LoadProfiles();
                return _allProfiles;
            }
        }

        public static Dictionary<string, bool> ProfileWarningLookup
        {
            get
            {
                if (!_profilesLoaded)
                    // Load the Profiles from storage if they need to be
                    LoadProfiles();

                return _profileWarningLookup;
            }
        }

        public static ProfileItem CurrentProfile
        {
            get
            {
                if (_currentProfile == null)
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
                if (!_profilesLoaded)
                    // Load the Profiles from storage if they need to be
                    LoadProfiles();


                return _allProfiles.Count;
            }
        }

        public static VIDEO_MODE CurrentVideoMode
        {
            get
            {
                return _currentVideoMode;
            }
            set
            {
                _currentVideoMode = value;
            }
        }
        public static FORCED_VIDEO_MODE ForcedVideoMode
        {
            get
            {
                return _forcedVideoMode;
            }
            set
            {
                _forcedVideoMode = value;
                SetVideoCardMode(value);
            }
        }


        public static List<string> ConnectedDisplayIdentifiers
        {
            get
            {
                if (_connectedDisplayIdentifiers.Count == 0)
                    // Load the Profiles from storage if they need to be
                    _connectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers();


                return _connectedDisplayIdentifiers;
            }
            set
            {
                _connectedDisplayIdentifiers = value;
            }
        }


        public static bool ProfilesLoaded {
            get
            {
                return _profilesLoaded;
            }
            set
            {
                _profilesLoaded = value;
            }
        }

        #endregion

        #region Class Methods
        public static bool InitialiseRepository(FORCED_VIDEO_MODE forcedVideoMode = FORCED_VIDEO_MODE.DETECT)
        {
            if (!SetVideoCardMode(forcedVideoMode))
            {
                return false;
            }

            if (!LoadProfiles())
            {
                return false;
            }

            return true;
        }


        public static bool AddProfile(ProfileItem profile)
        {
            if (!(profile is ProfileItem))
                return false;

            SharedLogger.logger.Debug($"ProfileRepository/AddProfile: Adding profile {profile.Name} to our profile repository");

            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (!ContainsProfile(profile))
            {
                // Add the Profile to the list of Profiles
                _allProfiles.Add(profile);

                // Generate the Profile Icon ready to be used
                SaveProfileIconToCache(profile);

                profile.PreSave();

                // Save the Profiles JSON as it's different
                SaveProfiles();
            }

            // Refresh the profiles to see whats valid
            IsPossibleRefresh();

            //Doublecheck it's been added
            if (ContainsProfile(profile))
            {
                return true;
            }
            else
                return false;

        }


        public static bool RemoveProfile(ProfileItem profile)
        {
            if (!(profile is ProfileItem))
                return false;

            SharedLogger.logger.Debug($"ProfileRepository/RemoveProfile: Removing profile {profile.Name} if it exists in our profile repository");

            // Remove the Profile Icons from the Cache
            List<ProfileItem> ProfilesToRemove = _allProfiles.FindAll(item => item.UUID.Equals(profile.UUID));
            foreach (ProfileItem ProfileToRemove in ProfilesToRemove)
            {
                try
                {
                    File.Delete(ProfileToRemove.SavedProfileIconCacheFilename);
                    File.Delete(ProfileToRemove.WallpaperBitmapFilename);
                }
                catch (UnauthorizedAccessException ex)
                {
                    SharedLogger.logger.Error(ex, $"ProfileRepository/RemoveProfile: DisplayMagician doesn't have permissions to delete the cached Profile Icon {ProfileToRemove.SavedProfileIconCacheFilename}.");
                }
                catch (ArgumentException ex)
                {
                    SharedLogger.logger.Error(ex, $"ProfileRepository/RemoveProfile: DisplayMagician can't delete the cached Profile Icon {ProfileToRemove.SavedProfileIconCacheFilename} due to an invalid argument.");
                }
                catch (PathTooLongException ex)
                {
                    SharedLogger.logger.Error(ex, $"ProfileRepository/RemoveProfile: DisplayMagician can't delete the cached Profile Icon {ProfileToRemove.SavedProfileIconCacheFilename} as the path is too long.");
                }
                catch (DirectoryNotFoundException ex)
                {
                    SharedLogger.logger.Error(ex, $"ProfileRepository/RemoveProfile: DisplayMagician can't delete the cached Profile Icon {ProfileToRemove.SavedProfileIconCacheFilename} as the parent folder isn't there.");
                }

            }

            // Remove the Profile from the list.
            int numRemoved = _allProfiles.RemoveAll(item => item.UUID.Equals(profile.UUID));

            if (numRemoved == 1)
            {
                SaveProfiles();
                IsPossibleRefresh();
                return true;
            }
            else if (numRemoved == 0)
                return false;
            else
                throw new ProfileRepositoryException();
        }


        public static bool RemoveProfile(string profileName)
        {

            if (String.IsNullOrWhiteSpace(profileName))
                return false;

            SharedLogger.logger.Debug($"ProfileRepository/RemoveProfile2: Removing profile {profileName} if it exists in our profile repository");

            // Remove the Profile Icons from the Cache
            List<ProfileItem> ProfilesToRemove = _allProfiles.FindAll(item => item.Name.Equals(profileName));
            foreach (ProfileItem ProfileToRemove in ProfilesToRemove)
            {
                try
                {
                    File.Delete(ProfileToRemove.SavedProfileIconCacheFilename);
                    File.Delete(ProfileToRemove.WallpaperBitmapFilename);
                }
                catch (UnauthorizedAccessException ex)
                {
                    SharedLogger.logger.Error(ex, $"ProfileRepository/RemoveProfile2: DisplayMagician doesn't have permissions to delete the cached Profile Icon {ProfileToRemove.SavedProfileIconCacheFilename}.");
                }
                catch (ArgumentException ex)
                {
                    SharedLogger.logger.Error(ex, $"ProfileRepository/RemoveProfile2: DisplayMagician can't delete the cached Profile Icon {ProfileToRemove.SavedProfileIconCacheFilename} due to an invalid argument.");
                }
                catch (PathTooLongException ex)
                {
                    SharedLogger.logger.Error(ex, $"ProfileRepository/RemoveProfile2: DisplayMagician can't delete the cached Profile Icon {ProfileToRemove.SavedProfileIconCacheFilename} as the path is too long.");
                }
                catch (DirectoryNotFoundException ex)
                {
                    SharedLogger.logger.Error(ex, $"ProfileRepository/RemoveProfile2: DisplayMagician can't delete the cached Profile Icon {ProfileToRemove.SavedProfileIconCacheFilename} as the parent folder isn't there.");
                }
            }

            // Remove the Profile from the list.
            int numRemoved = _allProfiles.RemoveAll(item => item.Name.Equals(profileName));

            if (numRemoved == 1)
            {
                SaveProfiles();
                IsPossibleRefresh();
                return true;
            }
            else if (numRemoved == 0)
                return false;
            else
                throw new ProfileRepositoryException();

        }

        public static bool RemoveProfile(uint profileId)
        {
            if (profileId == 0)
                return false;

            SharedLogger.logger.Debug($"ProfileRepository/RemoveProfile3: Removing profile wih profileId {profileId} if it exists in our profile repository");

            // Remove the Profile Icons from the Cache
            List<ProfileItem> ProfilesToRemove = _allProfiles.FindAll(item => item.UUID.Equals(profileId));
            foreach (ProfileItem ProfileToRemove in ProfilesToRemove)
            {
                try
                {
                    File.Delete(ProfileToRemove.SavedProfileIconCacheFilename);
                    File.Delete(ProfileToRemove.WallpaperBitmapFilename);
                }
                catch (UnauthorizedAccessException ex)
                {
                    SharedLogger.logger.Error(ex, $"ProfileRepository/RemoveProfile3: DisplayMagician doesn't have permissions to delete the cached Profile Icon {ProfileToRemove.SavedProfileIconCacheFilename}.");
                }
                catch (ArgumentException ex)
                {
                    SharedLogger.logger.Error(ex, $"ProfileRepository/RemoveProfile3: DisplayMagician can't delete the cached Profile Icon {ProfileToRemove.SavedProfileIconCacheFilename} due to an invalid argument.");
                }
                catch (PathTooLongException ex)
                {
                    SharedLogger.logger.Error(ex, $"ProfileRepository/RemoveProfile3: DisplayMagician can't delete the cached Profile Icon {ProfileToRemove.SavedProfileIconCacheFilename} as the path is too long.");
                }
                catch (DirectoryNotFoundException ex)
                {
                    SharedLogger.logger.Error(ex, $"ProfileRepository/RemoveProfile3: DisplayMagician can't delete the cached Profile Icon {ProfileToRemove.SavedProfileIconCacheFilename} as the parent folder isn't there.");
                }
            }

            // Remove the Profile from the list.
            int numRemoved = _allProfiles.RemoveAll(item => item.UUID.Equals(profileId));

            if (numRemoved == 1)
            {
                SaveProfiles();
                IsPossibleRefresh();
                return true;
            }
            else if (numRemoved == 0)
                return false;
            else
                throw new ProfileRepositoryException();
        }


        public static bool ContainsProfile(ProfileItem profile)
        {
            if (!(profile is ProfileItem))
                return false;

            SharedLogger.logger.Debug($"ProfileRepository/ContainsProfile: Checking if our profile repository contains a profile called {profile.Name}");

            foreach (ProfileItem testProfile in _allProfiles)
            {
                if (testProfile.Equals(profile))
                {
                    SharedLogger.logger.Debug($"ProfileRepository/ContainsProfile: Our profile repository does contain a profile called {profile.Name}");
                    return true;
                } 
            }
            SharedLogger.logger.Debug($"ProfileRepository/ContainsProfile: Our profile repository doesn't contain a profile called {profile.Name}");
            return false;
        }

        public static bool ContainsProfile(string ProfileNameOrId)
        {
            if (String.IsNullOrWhiteSpace(ProfileNameOrId))
                return false;

            SharedLogger.logger.Debug($"ProfileRepository/ContainsProfile2: Checking if our profile repository contains a profile with UUID or Name {ProfileNameOrId}");

            if (ProfileItem.IsValidUUID(ProfileNameOrId))
                foreach (ProfileItem testProfile in _allProfiles)
                {
                    if (testProfile.UUID.Equals(ProfileNameOrId))
                    {
                        SharedLogger.logger.Debug($"ProfileRepository/ContainsProfile2: Our profile repository does contain a profile with UUID {ProfileNameOrId}");
                        return true;
                    }

                }
            else
                foreach (ProfileItem testProfile in _allProfiles)
                {
                    if (testProfile.Name.Equals(ProfileNameOrId))
                    {
                        SharedLogger.logger.Debug($"ProfileRepository/ContainsProfile2: Our profile repository does contain a profile with Name {ProfileNameOrId}");
                        return true;
                    }

                }

            SharedLogger.logger.Debug($"ProfileRepository/ContainsProfile2: Our profile repository doesn't contain a profile with a UUID or Name {ProfileNameOrId}");
            return false;

        }

        public static bool ContainsCurrentProfile(out string savedProfileName)
        {
            savedProfileName = "";

            if (!(_currentProfile is ProfileItem))
            {
                return false;
            }
                

            SharedLogger.logger.Debug($"ProfileRepository/ContainsCurrentProfile: Checking if our profile repository contains the display profile currently in use");

            foreach (ProfileItem testProfile in _allProfiles)
            {
                if (testProfile.Equals(_currentProfile))
                {
                    SharedLogger.logger.Debug($"ProfileRepository/ContainsProfile: Our profile repository does contain a profile called {testProfile.Name}");
                    savedProfileName = testProfile.Name;
                    return true;
                }
            }

            SharedLogger.logger.Debug($"ProfileRepository/ContainsCurrentProfile: Our profile repository doesn't contain the display profile currently in use");
            return false;
        }

        public static ProfileItem GetProfile(string ProfileNameOrId)
        {

            SharedLogger.logger.Debug($"ProfileRepository/GetProfile: Finding and returning {ProfileNameOrId} if it exists in our profile repository");

            if (String.IsNullOrWhiteSpace(ProfileNameOrId))
            {
                SharedLogger.logger.Error($"ProfileRepository/GetProfile: Profile to get was empty or only whitespace");
                return null;
            }


            if (ProfileItem.IsValidUUID(ProfileNameOrId))
                foreach (ProfileItem testProfile in _allProfiles)
                {
                    if (testProfile.UUID.Equals(ProfileNameOrId))
                    {
                        SharedLogger.logger.Debug($"ProfileRepository/GetProfile: Returning profile with UUID {ProfileNameOrId}");
                        return testProfile;
                    }

                }
            else
                foreach (ProfileItem testProfile in _allProfiles)
                {
                    if (testProfile.Name.Equals(ProfileNameOrId))
                    {
                        SharedLogger.logger.Debug($"ProfileRepository/GetProfile: Returning profile with Name {ProfileNameOrId}");
                        return testProfile;
                    }

                }

            SharedLogger.logger.Debug($"ProfileRepository/GetProfile: Didn't match any profiles with UUD or Name {ProfileNameOrId}");
            return null;
        }

        public static bool RenameProfile(ProfileItem profile, string renamedName)
        {
            if (!(profile is ProfileItem))
            {
                SharedLogger.logger.Error($"ProfileRepository/RenameProfile: Profile to rename was empty or only whitespace");
                return false;
            }


            SharedLogger.logger.Debug($"ProfileRepository/RenameProfile: Attempting to rename profile {profile.Name} to {renamedName}");

            if (!IsValidFilename(renamedName))
            {
                SharedLogger.logger.Error($"ProfileRepository/RenameProfile: The name the user wanted to renamed to profile to is not a valid filename");
                return false;
            }

            profile.Name = GetValidFilename(renamedName);

            IsPossibleRefresh();

            // If it's been added to the list of AllProfiles
            // then we also need to reproduce the Icons
            if (ContainsProfile(profile))
            {
                // Save the Profiles JSON as it's different now
                SaveProfiles();
                SharedLogger.logger.Debug($"ProfileRepository/RenameProfile: The profile was successfully renamed from {profile.Name} to {renamedName}");
                return true;
            }
            else
            {
                SharedLogger.logger.Debug($"ProfileRepository/RenameProfile: The profile was not renamed from {profile.Name} to {renamedName}");
                return false;
            }
        }

        public static void UpdateActiveProfile()
        {


            //ProfileItem activeProfile;

            SharedLogger.logger.Debug($"ProfileRepository/UpdateActiveProfile: Updating the profile currently active (in use now).");

            ProfileItem profile;
            SharedLogger.logger.Debug($"ProfileRepository/UpdateActiveProfile: Attempting to access configuration through NVIDIA, then AMD, then Windows CCD interfaces, in that order.");
            if (_currentVideoMode == VIDEO_MODE.NVIDIA)
            {
                SharedLogger.logger.Debug($"ProfileRepository/UpdateActiveProfile: NVIDIA NVAPI Driver is installed, so using that for this display profile.");
                profile = new ProfileItem
                {
                    Name = "Current NVIDIA Display Profile",
                    VideoMode = VIDEO_MODE.NVIDIA
                };                
            }
            else if (_currentVideoMode == VIDEO_MODE.AMD)
            {
                SharedLogger.logger.Debug($"ProfileRepository/UpdateActiveProfile: NVIDIA is not installed but the AMD ADL Driver IS installed, so using that for this display profile.");
                profile = new ProfileItem
                {
                    Name = "Current AMD Display Profile",
                    VideoMode = VIDEO_MODE.AMD
                };
            }
            else
            {
                profile = new ProfileItem
                {
                    Name = "Current Windows Display Profile",
                    VideoMode = VIDEO_MODE.WINDOWS
                };
            }

            profile.CreateProfileFromCurrentDisplaySettings();

            if (_profilesLoaded && _allProfiles.Count > 0)
            {

                foreach (ProfileItem loadedProfile in ProfileRepository.AllProfiles)
                {
                    if (loadedProfile.Equals(profile))
                    {
                        _currentProfile = loadedProfile;
                        SharedLogger.logger.Debug($"ProfileRepository/UpdateActiveProfile: The {loadedProfile.VideoMode.ToString("G")} profile '{loadedProfile.Name}' is currently active (in use now).");
                        return;
                    }
                }
            }
            SharedLogger.logger.Debug($"ProfileRepository/UpdateActiveProfile: The current profile is a new profile that doesn't already exist in the Profile Repository.");
            _currentProfile = profile;
        }

        public static ProfileItem GetActiveProfile()
        {
            if (!(_currentProfile is ProfileItem))
                return null;

            SharedLogger.logger.Debug($"ProfileRepository/GetActiveProfile: Retrieving the currently active profile.");

            return _currentProfile;
        }

        public static bool IsActiveProfile(ProfileItem profile)
        {
            SharedLogger.logger.Trace($"ProfileRepository/IsActiveProfile: Checking whether the profile {profile.Name} is the currently active profile.");
            if (_currentProfile == null)
            {
                SharedLogger.logger.Error($"ProfileRepository/IsActiveProfile: The current profile {profile.Name} is null, so can't test it against anything.");
                return false;
            }

            if (profile == null)
            {
                SharedLogger.logger.Error($"ProfileRepository/IsActiveProfile: The requested profile {profile.Name} is null. Not changing anything, and reporting an error");
                return false;
            }             
            
            if (profile.Equals(_currentProfile))
            {
                SharedLogger.logger.Debug($"ProfileRepository/IsActiveProfile: The profile {profile.Name} is the currently active profile.");
                return true;
            }
            else
            {
                SharedLogger.logger.Debug($"ProfileRepository/IsActiveProfile: The profile {profile.Name} is the not currently active profile.");
                return false;
            }
        }

        
        private static bool LoadProfiles()
        {
            SharedLogger.logger.Debug($"ProfileRepository/LoadProfiles: Loading profiles from {_profileStorageJsonFileName} into the Profile Repository");

            if (File.Exists(_profileStorageJsonFileName))
            {
                string json = "";
                try
                {
                    json = File.ReadAllText(_profileStorageJsonFileName, Encoding.Unicode);
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"ProfileRepository/LoadProfiles: Tried to read the JSON file {_profileStorageJsonFileName} to memory but File.ReadAllTextthrew an exception.");
                }

                if (!string.IsNullOrWhiteSpace(json))
                {
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
                        SharedLogger.logger.Error(ex, $"ProfileRepository/LoadProfiles: Tried to parse the JSON in the {_profileStorageJsonFileName} but the JsonConvert threw an exception.");
                    }

                    SharedLogger.logger.Debug($"ProfileRepository/LoadProfiles: Finding the current profile in the Profile Repository");

                    // Go through all the  profiles and set up the needed structures (such as the Screens list)
                    // and check if the current profile is used
                    /*foreach (ProfileItem loadedProfile in _allProfiles)
                    {
                        SharedLogger.logger.Debug($"ProfileRepository/LoadProfiles: Profile {loadedProfile.Name} is a NVIDIA Profile");
                        loadedProfile.PerformPostLoadingTasks();
                        *//*if (ProfileRepository.IsActiveProfile(loadedProfile))
                            _currentProfile = loadedProfile;*//*
                    }*/

                    // Sort the profiles alphabetically
                    _allProfiles.Sort();

                }
                else
                {
                    SharedLogger.logger.Debug($"ProfileRepository/LoadProfiles: The {_profileStorageJsonFileName} profile JSON file exists but is empty! So we're going to treat it as if it didn't exist.");
                    //UpdateActiveProfile();
                }
            } 
            else
            {
                // If we get here, then we don't have any profiles saved!
                // So we gotta start from scratch
                SharedLogger.logger.Debug($"ProfileRepository/LoadProfiles: Couldn't find the {_profileStorageJsonFileName} profile JSON file that contains the Profiles");
                //UpdateActiveProfile();
            }
            _profilesLoaded = true;

            // Update the current active profile
            UpdateActiveProfile();
            IsPossibleRefresh();

            return true;
        }

        public static bool SaveProfiles()
        {
            SharedLogger.logger.Debug($"ProfileRepository/SaveProfiles: Attempting to save the profiles repository to the {AppProfileStoragePath}.");

            if (!Directory.Exists(AppProfileStoragePath))
            {
                try
                {
                    Directory.CreateDirectory(AppProfileStoragePath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    SharedLogger.logger.Fatal(ex, $"ProfileRepository/SaveProfiles: DisplayMagician doesn't have permissions to create the Profiles storage folder {AppProfileStoragePath}.");
                }
                catch (ArgumentException ex)
                {
                    SharedLogger.logger.Fatal(ex, $"ProfileRepository/SaveProfiles: DisplayMagician can't create the Profiles storage folder {AppProfileStoragePath} due to an invalid argument.");
                }
                catch (PathTooLongException ex)
                {
                    SharedLogger.logger.Fatal(ex, $"ProfileRepository/SaveProfiles: DisplayMagician can't create the Profiles storage folder {AppProfileStoragePath} as the path is too long.");
                }
                catch (DirectoryNotFoundException ex)
                {
                    SharedLogger.logger.Fatal(ex, $"ProfileRepository/SaveProfiles: DisplayMagician can't create the Profiles storage folder {AppProfileStoragePath} as the parent folder isn't there.");
                }
            }
            else
            {
                SharedLogger.logger.Debug($"ProfileRepository/SaveProfiles: Profiles folder {AppProfileStoragePath} exists.");
            }
            try
            {
                SharedLogger.logger.Debug($"ProfileRepository/SaveProfiles: Converting the objects to JSON format.");

                var json = JsonConvert.SerializeObject(_allProfiles, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    TypeNameHandling = TypeNameHandling.Auto

                });


                if (!string.IsNullOrWhiteSpace(json))
                {
                    SharedLogger.logger.Debug($"ProfileRepository/SaveProfiles: Saving the profile repository to the {_profileStorageJsonFileName}.");

                    File.WriteAllText(_profileStorageJsonFileName, json, Encoding.Unicode);
                    return true;
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"ProfileRepository/SaveProfiles: Unable to save the profile repository to the {_profileStorageJsonFileName}.");
            }

            return false;
        }

        private static void SaveProfileIconToCache(ProfileItem profile)
        {
            // Work out the name of the Profile we'll save.
            profile.SavedProfileIconCacheFilename = System.IO.Path.Combine(AppProfileStoragePath, string.Concat(@"profile-", profile.UUID, @".ico"));

            SharedLogger.logger.Debug($"ProfileRepository/SaveProfileIconToCache: Attempting to save the profile icon {profile.SavedProfileIconCacheFilename} to the {AppProfileStoragePath} folder");

            MultiIcon ProfileIcon;
            try
            {
                ProfileIcon = profile.ProfileIcon.ToIcon();
                ProfileIcon.Save(profile.SavedProfileIconCacheFilename, MultiIconFormat.ICO);
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex,$"ProfileRepository/SaveProfileIconToCache: Exception saving the profile icon {profile.SavedProfileIconCacheFilename} to the {AppProfileStoragePath} folder. Using the default DisplayMagician icon instead");
                // If we fail to create an icon based on the Profile, then we use the standard DisplayMagician profile one.
                // Which is created on program startup.
                File.Copy(AppDisplayMagicianIconFilename, profile.SavedProfileIconCacheFilename);
            }
        }

        public static void IsPossibleRefresh()
        {
            // We need to refresh the cached answer
            // Get the list of connected devices
            ConnectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers();

            if (_profilesLoaded && _allProfiles.Count > 0)
            {

                foreach (ProfileItem loadedProfile in AllProfiles)
                    loadedProfile.RefreshPossbility();
            }
        }


        public static List<string> GetAllConnectedDisplayIdentifiers()
        {
            if (_currentVideoMode == VIDEO_MODE.NVIDIA && NVIDIALibrary.GetLibrary().IsInstalled)
            {
                return NVIDIALibrary.GetLibrary().GetAllConnectedDisplayIdentifiers();
            }
            else if (_currentVideoMode == VIDEO_MODE.AMD && AMDLibrary.GetLibrary().IsInstalled)
            {
                return AMDLibrary.GetLibrary().GetAllConnectedDisplayIdentifiers();
            }
            else 
            {
                return WinLibrary.GetLibrary().GetAllConnectedDisplayIdentifiers();
            }
        }

        public static List<string> GetCurrentDisplayIdentifiers()
        {
            if (_currentVideoMode == VIDEO_MODE.NVIDIA && NVIDIALibrary.GetLibrary().IsInstalled)
            {
                return NVIDIALibrary.GetLibrary().GetCurrentDisplayIdentifiers();
            }
            else if (_currentVideoMode == VIDEO_MODE.AMD && AMDLibrary.GetLibrary().IsInstalled)
            {
                return AMDLibrary.GetLibrary().GetCurrentDisplayIdentifiers();
            }
            else
            {
                return WinLibrary.GetLibrary().GetCurrentDisplayIdentifiers();
            }
        }

        public static bool IsValidFilename(string testName)
        {
            SharedLogger.logger.Trace($"ProfileRepository/IsValidFilename: Checking whether {testName} is a valid filename");
            string strTheseAreInvalidFileNameChars = new string(System.IO.Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(testName)) {
                SharedLogger.logger.Trace($"ProfileRepository/IsValidFilename: {testName} is a valid filename");
                return false;
            }
            else
            {
                SharedLogger.logger.Debug($"ProfileRepository/IsValidFilename: {testName} isn't a valid filename as it contains one of these characters [" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
                return true;
            }            
        }

        public static string GetValidFilename(string uncheckedFilename)
        {
            SharedLogger.logger.Trace($"ProfileRepository/GetValidFilename: Modifying filename {uncheckedFilename} to be a valid filename for this filesystem");
            string invalid = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                uncheckedFilename = uncheckedFilename.Replace(c.ToString(), "");
            }
            SharedLogger.logger.Trace($"ProfileRepository/GetValidFilename: Modified filename {uncheckedFilename} so it is a valid filename for this filesystem");
            return uncheckedFilename;
        }

        // ApplyProfile lives here so that the UI works.
        public static ApplyProfileResult ApplyProfile(ProfileItem profile)
        {
            SharedLogger.logger.Trace($"Program/ApplyProfile: Starting");
            // We try to time the profile display swap
            Stopwatch stopWatch = new Stopwatch();
            bool wasDisplayChangeSuccessful = true;

            if (profile == null)
            {
                SharedLogger.logger.Debug($"ProfileRepository/ApplyProfile: The supplied profile is null! Can't be used.");
                return ApplyProfileResult.Error;
            }

            try
            {
                // We start the timer just before we attempt the display change
                stopWatch.Start();

                // We try to swap profiles. The profiles have checking logic in them
                if (!(profile.SetActive()))
                {
                    SharedLogger.logger.Error($"ProfileRepository/ApplyProfile: Error applying the {profile.VideoMode.ToString("G")} Profile!");
                    return ApplyProfileResult.Error;
                }
                else
                {
                    SharedLogger.logger.Trace($"ProfileRepository/ApplyProfile: Successfully applied the  {profile.VideoMode.ToString("G")} Profile!");
                    return ApplyProfileResult.Error;
                }

            }
            catch (Exception ex)
            {
                SharedLogger.logger.Debug($"ProfileRepository/ApplyProfile: Failed to complete changing the Windows Display layout");
                wasDisplayChangeSuccessful = false;
                return ApplyProfileResult.Error;
            }
            finally
            {
                // If the applying path info worked, then we attempt to set the desktop background if needed
                if (profile.WallpaperMode.Equals(Wallpaper.Mode.Apply) && !String.IsNullOrWhiteSpace(profile.WallpaperBitmapFilename))
                {
                    if (Wallpaper.Set(profile.WallpaperBitmapFilename, profile.WallpaperStyle))
                    {
                        SharedLogger.logger.Trace($"Program/ApplyProfile: We attempted to set the desktop wallpaper to {profile.SavedProfileIconCacheFilename} using {profile.WallpaperStyle} style for profile {profile.Name}, and it worked!");
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"Program/ApplyProfile: We attempted to set the desktop wallpaper to {profile.SavedProfileIconCacheFilename} using {profile.WallpaperStyle} style for profile {profile.Name}, and it failed :(");
                    }
                }
                else if (profile.WallpaperMode.Equals(Wallpaper.Mode.Clear))
                {
                    if (Wallpaper.Clear())
                    {
                        SharedLogger.logger.Trace($"Program/ApplyProfile: We attempted to clear the desktop wallpaper and it worked!");
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"Program/ApplyProfile: We attempted to clear the desktop wallpaper and it failed :(");
                    }
                }
                // We stop the stop watch
                stopWatch.Stop();
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;
                string result = "failed";
                if (wasDisplayChangeSuccessful)
                {
                    result = "was successful";
                }
                // Display the TimeSpan time and result.
                SharedLogger.logger.Debug($"ProfileRepository/ApplyProfile: Display change attempt took {ts.Minutes}:{ts.Seconds}.{ts.Milliseconds} and {result}.");
            }


            ProfileRepository.UpdateActiveProfile();

            return ApplyProfileResult.Successful;
        }

        public static bool SetVideoCardMode(FORCED_VIDEO_MODE forcedVideoMode = FORCED_VIDEO_MODE.DETECT)
        {            
            _forcedVideoMode = forcedVideoMode;
            // This sets the order in which the different modes have been chosen.
            // NVIDIA Video cards are the most common, so go first
            if (_forcedVideoMode == FORCED_VIDEO_MODE.NVIDIA)
            {
                // We force the video mode to be NVIDIA
                _currentVideoMode = VIDEO_MODE.NVIDIA;
            }
            else if (forcedVideoMode == FORCED_VIDEO_MODE.AMD)
            {
                // We force the video mode to be AMD
                _currentVideoMode = VIDEO_MODE.AMD;
            }
            else if (forcedVideoMode == FORCED_VIDEO_MODE.WINDOWS)
            {
                // We force the video mode to be WINDOWS
                _currentVideoMode = VIDEO_MODE.WINDOWS;
            }
            else
            {
                // We do normal video library detection based on the video card!

                // Figure out the Video Cards and see what mode we want
                // Get a list of all the PCI Vendor IDs
                List<string> videoCardVendors = WinLibrary.GetLibrary().GetCurrentPCIVideoCardVendors();
                if (NVIDIALibrary.GetLibrary().PCIVendorIDs.All(value => videoCardVendors.Contains(value)))
                {
                    // We detected a NVIDIA video card in the computer
                    _currentVideoMode = VIDEO_MODE.NVIDIA;
                }
                else if (AMDLibrary.GetLibrary().PCIVendorIDs.All(value => videoCardVendors.Contains(value)))
                {
                    // We detected an AMD video card in the computer
                    _currentVideoMode = VIDEO_MODE.AMD;
                }
                else
                {
                    // We fallback to the built-in Windows CCD drivers
                    _currentVideoMode = VIDEO_MODE.WINDOWS;
                }
            }

            if (_currentVideoMode == VIDEO_MODE.NVIDIA)
            {
                // Initialise the the NVIDIA NvAPI Library
                try
                {
                    SharedLogger.logger.Debug($"ProfileRepository/ProfileRepository: Initialising the NVIDIA NVAPI library.");
                    nvidiaLibrary = new NVIDIALibrary();
                    _currentVideoMode = VIDEO_MODE.NVIDIA;
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"ProfileRepository/ProfileRepository: Initialising NVIDIA NVAPI caused an exception.");
                    return false;
                }
            }
            else if (_currentVideoMode == VIDEO_MODE.AMD)
            {
                // Initialise the the AMD ADL Library
                try
                {
                    SharedLogger.logger.Debug($"ProfileRepository/ProfileRepository: Initialising the AMD ADL library.");
                    amdLibrary = new AMDLibrary();
                    _currentVideoMode = VIDEO_MODE.AMD;
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"ProfileRepository/ProfileRepository: Initialising AMD ADL caused an exception.");
                    return false;
                }
            }

            return true;
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


    public class ApplyTopologyException : Exception
    {
        public ApplyTopologyException()
        { }

        public ApplyTopologyException(string message) : base(message)
        { }

        public ApplyTopologyException(string message, Exception innerException) : base(message, innerException)
        { }
        public ApplyTopologyException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}

