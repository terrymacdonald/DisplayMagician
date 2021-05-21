using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.IconLib;
using System.IO;
using System.Linq;
using System.Text;
using WindowsDisplayAPI.DisplayConfig;
using NvAPIWrapper.Mosaic;
using NvAPIWrapper.Native.Mosaic;
using WindowsDisplayAPI;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NvAPIWrapper.Native.GPU;
using System.Windows.Forms;

namespace DisplayMagicianShared
{


    public static class ProfileRepository
    {
        #region Class Variables
        // Common items to the class
        private static List<ProfileItem> _allProfiles = new List<ProfileItem>();
        public static Dictionary<string, bool> _profileWarningLookup = new Dictionary<string, bool>();
        private static bool _profilesLoaded = false;
        public static Version _version = new Version(1, 0, 0);
        private static ProfileItem _currentProfile;
        private static List<string> _connectedDisplayIdentifiers = new List<string>();
        private static bool notifiedEDIDErrorToUser = false;

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

            // Initialise the the NVIDIA NvAPIWrapper
            try
            {
                SharedLogger.logger.Debug($"ProfileRepository/ProfileRepository: Initialising the NvAPIWrapper.NVIDIA library.");
                NvAPIWrapper.NVIDIA.Initialize();

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
                SharedLogger.logger.Warn(ex, $"ProfileRepository/ProfileRepository: Initialising NVIDIA NvAPIWrapper caused an exception.");
            }
            // Load the Profiles from storage
            LoadProfiles();
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

        
        public static List<string> ConnectedDisplayIdentifiers
        {
            get
            {
                if (_connectedDisplayIdentifiers.Count == 0)
                    // Load the Profiles from storage if they need to be
                    _connectedDisplayIdentifiers = GenerateAllAvailableDisplayIdentifiers();


                return _connectedDisplayIdentifiers;
            }
            set
            {
                _connectedDisplayIdentifiers = value;
            }
        }


        #endregion

        #region Class Methods
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


        public static bool ContainsProfile(ProfileItem Profile)
        {
            if (!(Profile is ProfileItem))
                return false;

            SharedLogger.logger.Debug($"ProfileRepository/ContainsProfile: Checking if our profile repository contains a profile called {Profile.Name}");

            foreach (ProfileItem testProfile in _allProfiles)
            {
                if (testProfile.UUID.Equals(Profile.UUID))
                {
                    SharedLogger.logger.Debug($"ProfileRepository/ContainsProfile: Our profile repository does contain a profile called {Profile.Name}");
                    return true;
                }
                    
            }
            SharedLogger.logger.Debug($"ProfileRepository/ContainsProfile: Our profile repository doesn't contain a profile called {Profile.Name}");
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

        public static bool ContainsCurrentProfile()
        {
            if (!(_currentProfile is ProfileItem))
                return false;

            SharedLogger.logger.Debug($"ProfileRepository/ContainsCurrentProfile: Checking if our profile repository contains the display profile currently in use");

            foreach (ProfileItem testProfile in _allProfiles)
            {
                // TODO - change for Equals
                if (testProfile.Equals(_currentProfile))
                {
                    SharedLogger.logger.Debug($"ProfileRepository/ContainsCurrentProfile: Our profile repository does contain the display profile currently in use");
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


        /*public static void UpdateActiveProfile()
        {

            SharedLogger.logger.Debug($"ProfileRepository/UpdateActiveProfile: Updating the profile currently active (in use now).");

            ProfileItem activeProfile = new ProfileItem
            {
                Name = "Current Display Profile",
                Paths = PathInfo.GetActivePaths().Select(info => new DisplayMagicianShared.Topology.Path(info)).ToArray()
            };

            activeProfile.ProfileIcon = new ProfileIcon(activeProfile);
            activeProfile.ProfileBitmap = activeProfile.ProfileIcon.ToBitmap(256, 256);

            if (_profilesLoaded && _allProfiles.Count > 0)
            {
                foreach (ProfileItem loadedProfile in ProfileRepository.AllProfiles)
                {
                    if (activeProfile.Paths.SequenceEqual(loadedProfile.Paths))
                    {
                        _currentProfile = loadedProfile;
                        SharedLogger.logger.Debug($"ProfileRepository/UpdateActiveProfile: The profile {loadedProfile.Name} is currently active (in use now).");
                        return;
                    }
                }
            }
            SharedLogger.logger.Debug($"ProfileRepository/UpdateActiveProfile: The current profile is a new profile that doesn't already exist in the Profile Repository.");
            _currentProfile = activeProfile;

            //IsPossibleRefresh();

        }*/


        public static void UpdateActiveProfile()
        {

            SharedLogger.logger.Debug($"ProfileRepository/UpdateActiveProfile: Updating the profile currently active (in use now).");

            ProfileItem activeProfile = new ProfileItem
            {
                Name = "Current Display Profile",
                Paths = PathInfo.GetActivePaths().Select(info => new DisplayMagicianShared.Topology.Path(info)).ToArray(),
                //ProfileDisplayIdentifiers = ProfileRepository.GenerateProfileDisplayIdentifiers()
            };

            activeProfile.ProfileIcon = new ProfileIcon(activeProfile);
            activeProfile.ProfileBitmap = activeProfile.ProfileIcon.ToBitmap(256, 256);

            if (_profilesLoaded && _allProfiles.Count > 0)
            {
                foreach (ProfileItem loadedProfile in ProfileRepository.AllProfiles)
                {
                    if (activeProfile.Equals(loadedProfile))
                    {
                        _currentProfile = loadedProfile;
                        SharedLogger.logger.Debug($"ProfileRepository/UpdateActiveProfile: The profile {loadedProfile.Name} is currently active (in use now).");
                        return;
                    }
                }
            }
            SharedLogger.logger.Debug($"ProfileRepository/UpdateActiveProfile: The current profile is a new profile that doesn't already exist in the Profile Repository.");
            _currentProfile = activeProfile;

            //IsPossibleRefresh();

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
            if (!(_currentProfile is ProfileItem))
                return false;

            if (!(profile is ProfileItem))
                return false;

            SharedLogger.logger.Debug($"ProfileRepository/IsActiveProfile: Checking whether the profile {profile.Name} is the currently active profile.");

            //if (profile.Paths.SequenceEqual(_currentProfile.Paths))
            if (profile.Equals(_currentProfile))
            {
                SharedLogger.logger.Debug($"ProfileRepository/IsActiveProfile: The profile {profile.Name} is the currently active profile.");
                return true;
            }

            SharedLogger.logger.Debug($"ProfileRepository/IsActiveProfile: The profile {profile.Name} is not the currently active profile.");
            return false;
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


                    ProfileItem myCurrentProfile = new ProfileItem
                    {
                        Name = "Current Display Profile",
                        Paths = PathInfo.GetActivePaths().Select(info => new DisplayMagicianShared.Topology.Path(info)).ToArray()
                    };

                    _currentProfile = myCurrentProfile;

                    SharedLogger.logger.Debug($"ProfileRepository/LoadProfiles: Finding the current profile in the Profile Repository");

                    // Lookup all the Profile Names in the Saved Profiles
                    foreach (ProfileItem loadedProfile in _allProfiles)
                    {
                        if (ProfileRepository.IsActiveProfile(loadedProfile))
                            _currentProfile = loadedProfile;

                    }

                    // Sort the profiles alphabetically
                    _allProfiles.Sort();

                }
                else
                {
                    SharedLogger.logger.Debug($"ProfileRepository/LoadProfiles: The {_profileStorageJsonFileName} profile JSON file exists but is empty! So we're going to treat it as if it didn't exist.");
                    UpdateActiveProfile();
                }
            } 
            else
            {
                // If we get here, then we don't have any profiles saved!
                // So we gotta start from scratch
                SharedLogger.logger.Debug($"ProfileRepository/LoadProfiles: Couldn't find the {_profileStorageJsonFileName} profile JSON file that contains the Profiles");
                UpdateActiveProfile();
            }
            _profilesLoaded = true;

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
            ConnectedDisplayIdentifiers = GenerateAllAvailableDisplayIdentifiers();

            if (_profilesLoaded && _allProfiles.Count > 0)
            {

                foreach (ProfileItem loadedProfile in AllProfiles)
                    loadedProfile.RefreshPossbility();
            }
        }


        public static List<string> GenerateProfileDisplayIdentifiers()
        {
            SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: Generating the unique Display Identifiers for the currently active profile");

            List<string> displayIdentifiers = new List<string>();

            // If the Video Card is an NVidia, then we should generate specific NVidia displayIdentifiers
            bool isNvidia = false;
            NvAPIWrapper.GPU.PhysicalGPU[] myPhysicalGPUs = null;
            try
            {
                myPhysicalGPUs = NvAPIWrapper.GPU.PhysicalGPU.GetPhysicalGPUs();
                isNvidia = true;
                SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: The video card is a NVIDIA video card.");
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Debug(ex, "ProfileRepository/GenerateProfileDisplayIdentifiers: Attemped to get GetPhysicalCPUs through NvAPIWrapper library but got exception. This means the video card isn't compatible with the NvAPIWrapper library we use. It is unlikely to be an NVIDIA video card.");
            }

            if (isNvidia && myPhysicalGPUs != null && myPhysicalGPUs.Length > 0)
            //if (false)
            {
                SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: We were able to GetPhysicalCPUs through NvAPIWrapper library. There are {myPhysicalGPUs.Length} Physical GPUs detected");

                foreach (NvAPIWrapper.GPU.PhysicalGPU myPhysicalGPU in myPhysicalGPUs)
                {
                    // get a list of all physical outputs attached to the GPUs
                    NvAPIWrapper.GPU.GPUOutput[] myGPUOutputs = myPhysicalGPU.ActiveOutputs;
                    foreach (NvAPIWrapper.GPU.GPUOutput aGPUOutput in myGPUOutputs)
                    {
                        SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: We were able to detect {myGPUOutputs.Length} outputs");
                        // Figure out the displaydevice attached to the output
                        NvAPIWrapper.Display.DisplayDevice aConnectedDisplayDevice = myPhysicalGPU.GetDisplayDeviceByOutput(aGPUOutput);

                        // Create an array of all the important display info we need to record
                        List<string> displayInfo = new List<string>();
                        displayInfo.Add("NVIDIA");
                        try 
                        {
                            displayInfo.Add(myPhysicalGPU.ArchitectInformation.ShortName.ToString());
                        }
                        catch(Exception ex)
                        {
                            SharedLogger.logger.Warn(ex,$"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Architecture ShortName from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(myPhysicalGPU.ArchitectInformation.Revision.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Architecture Revision from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(myPhysicalGPU.Board.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Board details from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(myPhysicalGPU.Foundry.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Foundry from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(myPhysicalGPU.GPUId.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA GPUId from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(myPhysicalGPU.GPUType.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA GPUType from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(aConnectedDisplayDevice.ConnectionType.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Connection from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(aConnectedDisplayDevice.DisplayId.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA DisplayID from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }

                        // Create a display identifier out of it
                        string displayIdentifier = String.Join("|", displayInfo);
                        // Add it to the list of display identifiers so we can return it
                        displayIdentifiers.Add(displayIdentifier);

                        SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                    }

                }
            }
            // else videocard is not NVIdia so we just use the WindowsAPI access method
            // Note: This won't support any special AMD EyeFinity profiles unfortunately.....
            // TODO: Add the detection and generation of the device ids using an AMD library
            //       so that we can match valid AMD Eyefinity profiles with valid AMD standard profiles.
            else
            {
                // Then go through the adapters we have running using the WindowsDisplayAPI
                List<Display> attachedDisplayDevices = Display.GetDisplays().ToList();

                SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: We are using the standard Windows Display API to figure out what display devices are attached and available. There are {attachedDisplayDevices.Count} display devices detected.");

                foreach (Display attachedDisplay in attachedDisplayDevices)
                {
                    DisplayAdapter displayAdapter = null;
                    PathDisplayAdapter pathDisplayAdapter = null;
                    PathDisplaySource pathDisplaySource = null;
                    PathDisplayTarget pathDisplayTarget = null;

                    
                    try
                    {
                        // We keep these lines here to detect if there is an exception so we can report it
                        // nicely to the user.
                        displayAdapter = attachedDisplay.Adapter;
                        pathDisplayAdapter = displayAdapter.ToPathDisplayAdapter();
                        pathDisplaySource = attachedDisplay.ToPathDisplaySource();
                        pathDisplayTarget = attachedDisplay.ToPathDisplayTarget();

                        // This line is just to force an EDID lookup first up so that we can deterine if there is an issue 
                        // with the Monitor, and then tell the user
                        string EDIDManufacturerId = pathDisplayTarget.EDIDManufactureId.ToString();

                        // print some trace messages so we can figure out issues if needed later
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADDN : {attachedDisplay.DeviceName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADDFN : {attachedDisplay.DisplayFullName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADDIN : {attachedDisplay.DisplayName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADDIN : {attachedDisplay.IsAvailable}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADDIGP : {attachedDisplay.IsGDIPrimary}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADDIV : {attachedDisplay.IsValid}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSCD : {attachedDisplay.CurrentSetting.ColorDepth}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSF : {attachedDisplay.CurrentSetting.Frequency}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSIE : {attachedDisplay.CurrentSetting.IsEnable}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSII : {attachedDisplay.CurrentSetting.IsInterlaced}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSO : {attachedDisplay.CurrentSetting.Orientation}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSOSM : {attachedDisplay.CurrentSetting.OutputScalingMode}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSP : {attachedDisplay.CurrentSetting.Position}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSR : {attachedDisplay.CurrentSetting.Resolution}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: DP : {displayAdapter.DevicePath}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: DK : {displayAdapter.DeviceKey}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: DN : {displayAdapter.DeviceName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: DK : {displayAdapter.DeviceKey}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: AI : {pathDisplayAdapter.AdapterId}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: AIDP : {pathDisplayAdapter.DevicePath}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: AIII : {pathDisplayAdapter.IsInvalid}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: DDA : {displayAdapter.DeviceName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDSA : {pathDisplaySource.Adapter}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDSCDS : {pathDisplaySource.CurrentDPIScale}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDSDN : {pathDisplaySource.DisplayName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDSMDS : {pathDisplaySource.MaximumDPIScale}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDSRDS : {pathDisplaySource.RecommendedDPIScale}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDSSI : {pathDisplaySource.SourceId}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTA : {pathDisplayTarget.Adapter}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTCI : {pathDisplayTarget.ConnectorInstance}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTDP : {pathDisplayTarget.DevicePath}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTEMC : {pathDisplayTarget.EDIDManufactureCode}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTEMI : {pathDisplayTarget.EDIDManufactureId}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTEPC : {pathDisplayTarget.EDIDProductCode}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTFN : {pathDisplayTarget.FriendlyName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTIA : {pathDisplayTarget.IsAvailable}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTPR : {pathDisplayTarget.PreferredResolution}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTPSM : {pathDisplayTarget.PreferredSignalMode}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTTI : {pathDisplayTarget.TargetId}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTVRS : {pathDisplayTarget.VirtualResolutionSupport}");
                    }
                    catch (WindowsDisplayAPI.Exceptions.InvalidEDIDInformation ex)
                    {
                        SharedLogger.logger.Error(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception while trying to get information from your monitor {attachedDisplay.DisplayFullName} about it's configuration. DisplayMagician may not be able to use this monitor!");
                        if (!notifiedEDIDErrorToUser)
                        {
                            MessageBox.Show(
                                $"Your monitor {attachedDisplay.DisplayFullName} is not responding when we ask about it's configuration. DisplayMagician may not be able to use this monitor!", @"DisplayMagician cannot talk to your monitor",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            notifiedEDIDErrorToUser = true;
                        }                            
                    }
                    catch (WindowsDisplayAPI.Exceptions.TargetNotAvailableException ex)
                    {
                        SharedLogger.logger.Error(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception while we were trying to access the DisplayTarget to gather information about your display configuration.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception accessing one of the WindowsDisplayAPI items to print it out during a TRACE session");
                    }

                    // Create an array of all the important display info we need to record
                    List<string> displayInfo = new List<string>();
                    displayInfo.Add("WINAPI");
                    try
                    {
                        displayInfo.Add(displayAdapter.DeviceName.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting Windows Display Adapter Device name from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayAdapter.AdapterId.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting Windows Display Adapter ID from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayTarget.ConnectorInstance.ToString());
                    }                    
                    catch (WindowsDisplayAPI.Exceptions.TargetNotAvailableException ex)
                    {
                        SharedLogger.logger.Error(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception2 while we were trying to access the DisplayTarget to gather information about your display configuration.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting Windows Display Target Connector Instance from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayTarget.FriendlyName);
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting Windows Display Target Friendly name from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayTarget.EDIDManufactureCode.ToString());
                    }
                    catch (WindowsDisplayAPI.Exceptions.InvalidEDIDInformation ex)
                    {
                        SharedLogger.logger.Error(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers2: Exception while trying to get information from your monitor {attachedDisplay.DisplayFullName} about it's configuration. DisplayMagician may not be able to use this monitor!");
                    }
                    catch (WindowsDisplayAPI.Exceptions.TargetNotAvailableException ex)
                    {
                        SharedLogger.logger.Error(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers2: Exception while we were trying to access the DisplayTarget to gather information about your display configuration.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers2: Exception getting Windows Display EDID Manufacturer Code from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayTarget.EDIDManufactureId.ToString());
                    }
                    catch (WindowsDisplayAPI.Exceptions.InvalidEDIDInformation ex)
                    {
                        SharedLogger.logger.Error(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers3: Exception while trying to get information from your monitor {attachedDisplay.DisplayFullName} about it's configuration. DisplayMagician may not be able to use this monitor!");
                    }
                    catch (WindowsDisplayAPI.Exceptions.TargetNotAvailableException ex)
                    {
                        SharedLogger.logger.Error(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers3: Exception while we were trying to access the DisplayTarget to gather information about your display configuration.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers3: Exception getting Windows Display EDID Manufacturer ID from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayTarget.EDIDProductCode.ToString());
                    }
                    catch (WindowsDisplayAPI.Exceptions.InvalidEDIDInformation ex)
                    {
                        SharedLogger.logger.Error(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers4: Exception while trying to get information from your monitor {attachedDisplay.DisplayFullName} about it's configuration. DisplayMagician may not be able to use this monitor!");
                    }
                    catch (WindowsDisplayAPI.Exceptions.TargetNotAvailableException ex)
                    {
                        SharedLogger.logger.Error(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers4: Exception while we were trying to access the DisplayTarget to gather information about your display configuration.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers4: Exception getting Windows Display EDID Product Code from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayTarget.TargetId.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting Windows Display Target ID from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }

                    // Create a display identifier out of it
                    string displayIdentifier = String.Join("|", displayInfo);
                    // Add it to the list of display identifiers so we can return it
                    displayIdentifiers.Add(displayIdentifier);
                    SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                    
                }

            }

            // Sort the display identifiers
            displayIdentifiers.Sort();

            return displayIdentifiers;
        }

        public static List<string> GenerateAllAvailableDisplayIdentifiers()
        {
            SharedLogger.logger.Debug($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: Generating all the Display Identifiers currently active now");

            List<string> displayIdentifiers = new List<string>();

            // If the Video Card is an NVidia, then we should generate specific NVidia displayIdentifiers
            bool isNvidia = false;
            NvAPIWrapper.GPU.PhysicalGPU[] myPhysicalGPUs = null;
            try
            {
                myPhysicalGPUs = NvAPIWrapper.GPU.PhysicalGPU.GetPhysicalGPUs();
                isNvidia = true;
                SharedLogger.logger.Debug($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: The video card is a NVIDIA video card.");
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Debug(ex, "ProfileRepository/GenerateAllAvailableDisplayIdentifiers: Attemped to get GetPhysicalCPUs through NvAPIWrapper library but got exception. This means the video card isn't compatible with the NvAPIWrapper library we use. It is unlikely to be an NVIDIA video card.");
            }

            if (isNvidia && myPhysicalGPUs != null && myPhysicalGPUs.Length > 0)
            //if (false)
            {
                SharedLogger.logger.Debug($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: We were able to GetPhysicalCPUs through NvAPIWrapper library. There are {myPhysicalGPUs.Length} Physical GPUs detected");

                foreach (NvAPIWrapper.GPU.PhysicalGPU myPhysicalGPU in myPhysicalGPUs)
                {
                    // get a list of all physical outputs attached to the GPUs
                    NvAPIWrapper.Display.DisplayDevice[] allDisplayDevices = myPhysicalGPU.GetConnectedDisplayDevices(ConnectedIdsFlag.None);

                    SharedLogger.logger.Debug($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: We were able to detect {allDisplayDevices.Length} connected devices");
                    foreach (NvAPIWrapper.Display.DisplayDevice aDisplayDevice in allDisplayDevices)
                    {

                        if (aDisplayDevice.IsAvailable== true)
                        {
                            // Create an array of all the important display info we need to record
                            List<string> displayInfo = new List<string>();
                            displayInfo.Add("NVIDIA");
                            try
                            {
                                displayInfo.Add(myPhysicalGPU.ArchitectInformation.ShortName.ToString());
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Architecture ShortName from video card. Substituting with a # instead");
                                displayInfo.Add("#");
                            }
                            try
                            {
                                displayInfo.Add(myPhysicalGPU.ArchitectInformation.Revision.ToString());
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Architecture Revision from video card. Substituting with a # instead");
                                displayInfo.Add("#");
                            }
                            try
                            {
                                displayInfo.Add(myPhysicalGPU.Board.ToString());
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Board details from video card. Substituting with a # instead");
                                displayInfo.Add("#");
                            }
                            try
                            {
                                displayInfo.Add(myPhysicalGPU.Foundry.ToString());
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Foundry from video card. Substituting with a # instead");
                                displayInfo.Add("#");
                            }
                            try
                            {
                                displayInfo.Add(myPhysicalGPU.GPUId.ToString());
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA GPUId from video card. Substituting with a # instead");
                                displayInfo.Add("#");
                            }
                            try
                            {
                                displayInfo.Add(myPhysicalGPU.GPUType.ToString());
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA GPUType from video card. Substituting with a # instead");
                                displayInfo.Add("#");
                            }
                            try
                            {
                                displayInfo.Add(aDisplayDevice.ConnectionType.ToString());
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA Connection from video card. Substituting with a # instead");
                                displayInfo.Add("#");
                            }
                            try
                            {
                                displayInfo.Add(aDisplayDevice.DisplayId.ToString());
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting NVIDIA DisplayID from video card. Substituting with a # instead");
                                displayInfo.Add("#");
                            }

                            // Create a display identifier out of it
                            string displayIdentifier = String.Join("|", displayInfo);
                            // Add it to the list of display identifiers so we can return it
                            displayIdentifiers.Add(displayIdentifier);
                            SharedLogger.logger.Debug($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                        }
                    }
                }
            }
            // else videocard is not NVIDIA so we just use the WindowsAPI access method
            // Note: This won't support any special AMD EyeFinity profiles unfortunately.....
            // TODO: Add the detection and generation of the device ids using an AMD library
            //       so that we can match valid AMD Eyefinity profiles with valid AMD standard profiles.
            else
            {
                
                // Then go through the adapters we have running using the WindowsDisplayAPI
                List<Display> attachedDisplayDevices = Display.GetDisplays().ToList();
                List<UnAttachedDisplay> unattachedDisplayDevices = UnAttachedDisplay.GetUnAttachedDisplays().ToList();

                SharedLogger.logger.Debug($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: We are using the standard Windows Display API to figure out what display devices are attached and available. There are {attachedDisplayDevices.Count} display devices attached and {unattachedDisplayDevices.Count} devices unattached.");


                foreach (Display attachedDisplay in attachedDisplayDevices)
                {
                    DisplayAdapter displayAdapter = attachedDisplay.Adapter;
                    PathDisplayAdapter pathDisplayAdapter = displayAdapter.ToPathDisplayAdapter();
                    PathDisplaySource pathDisplaySource = attachedDisplay.ToPathDisplaySource();
                    PathDisplayTarget pathDisplayTarget = attachedDisplay.ToPathDisplayTarget();

                    try
                    {
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADDN : {attachedDisplay.DeviceName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADDFN : {attachedDisplay.DisplayFullName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADDIN : {attachedDisplay.DisplayName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADDIN : {attachedDisplay.IsAvailable}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADDIGP : {attachedDisplay.IsGDIPrimary}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADDIV : {attachedDisplay.IsValid}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSCD : {attachedDisplay.CurrentSetting.ColorDepth}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSF : {attachedDisplay.CurrentSetting.Frequency}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSIE : {attachedDisplay.CurrentSetting.IsEnable}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSII : {attachedDisplay.CurrentSetting.IsInterlaced}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSO : {attachedDisplay.CurrentSetting.Orientation}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSOSM : {attachedDisplay.CurrentSetting.OutputScalingMode}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSP : {attachedDisplay.CurrentSetting.Position}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: ADCSR : {attachedDisplay.CurrentSetting.Resolution}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: DP : {displayAdapter.DevicePath}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: DK : {displayAdapter.DeviceKey}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: DN : {displayAdapter.DeviceName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: DK : {displayAdapter.DeviceKey}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: AI : {pathDisplayAdapter.AdapterId}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: AIDP : {pathDisplayAdapter.DevicePath}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: AIII : {pathDisplayAdapter.IsInvalid}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: DDA : {displayAdapter.DeviceName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDSA : {pathDisplaySource.Adapter}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDSCDS : {pathDisplaySource.CurrentDPIScale}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDSDN : {pathDisplaySource.DisplayName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDSMDS : {pathDisplaySource.MaximumDPIScale}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDSRDS : {pathDisplaySource.RecommendedDPIScale}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDSSI : {pathDisplaySource.SourceId}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTA : {pathDisplayTarget.Adapter}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTCI : {pathDisplayTarget.ConnectorInstance}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTDP : {pathDisplayTarget.DevicePath}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTEMC : {pathDisplayTarget.EDIDManufactureCode}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTEMI : {pathDisplayTarget.EDIDManufactureId}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTEPC : {pathDisplayTarget.EDIDProductCode}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTFN : {pathDisplayTarget.FriendlyName}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTIA : {pathDisplayTarget.IsAvailable}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTPR : {pathDisplayTarget.PreferredResolution}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTPSM : {pathDisplayTarget.PreferredSignalMode}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTTI : {pathDisplayTarget.TargetId}");
                        SharedLogger.logger.Trace($"ProfileRepository/GenerateProfileDisplayIdentifiers: PDTVRS : {pathDisplayTarget.VirtualResolutionSupport}");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception accessing one of the WindowsDisplayAPI items to print it out during a TRACE session");
                    }

                    // Create an array of all the important display info we need to record
                    List<string> displayInfo = new List<string>();
                    displayInfo.Add("WINAPI");
                    try
                    {
                        displayInfo.Add(displayAdapter.DeviceName.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting Windows Display Adapter Device name from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayAdapter.AdapterId.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting Windows Display Adapter ID from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayTarget.ConnectorInstance.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting Windows Display Target Connector Instance from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayTarget.FriendlyName);
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting Windows Display Target Friendly name from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayTarget.EDIDManufactureCode.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting Windows Display EDID Manufacturer Code from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayTarget.EDIDManufactureId.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting Windows Display EDID Manufacturer ID from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayTarget.EDIDProductCode.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting Windows Display EDID Product Code from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(pathDisplayTarget.TargetId.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception getting Windows Display Target ID from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }

                    // Create a display identifier out of it
                    string displayIdentifier = String.Join("|", displayInfo);
                    // Add it to the list of display identifiers so we can return it
                    displayIdentifiers.Add(displayIdentifier);
                    SharedLogger.logger.Debug($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: Attached DisplayIdentifier: {displayIdentifier}");
                }

                foreach (UnAttachedDisplay unattachedDisplay in unattachedDisplayDevices)
                {
                    DisplayAdapter displayAdapter = unattachedDisplay.Adapter;
                    PathDisplayAdapter pathDisplayAdapter = displayAdapter.ToPathDisplayAdapter();
                    PathDisplaySource pathDisplaySource = unattachedDisplay.ToPathDisplaySource();
                    PathDisplayTarget pathDisplayTarget = unattachedDisplay.ToPathDisplayTarget();

                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: ADDN : {unattachedDisplay.DeviceName}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: ADDFN : {unattachedDisplay.DisplayFullName}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: ADDIN : {unattachedDisplay.DisplayName}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: ADDIN : {unattachedDisplay.IsAvailable}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: ADDIV : {unattachedDisplay.IsValid}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: DP : {displayAdapter.DevicePath}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: DK : {displayAdapter.DeviceKey}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: DN : {displayAdapter.DeviceName}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: DK : {displayAdapter.DeviceKey}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: AI : {pathDisplayAdapter.AdapterId}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: AIDP : {pathDisplayAdapter.DevicePath}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: AIII : {pathDisplayAdapter.IsInvalid}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDSA : {pathDisplaySource.Adapter}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDSCDS : {pathDisplaySource.CurrentDPIScale}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDSDN : {pathDisplaySource.DisplayName}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDSMDS : {pathDisplaySource.MaximumDPIScale}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDSRDS : {pathDisplaySource.RecommendedDPIScale}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDSSI : {pathDisplaySource.SourceId}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDTA : {pathDisplayTarget.Adapter}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDTCI : {pathDisplayTarget.ConnectorInstance}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDTDP : {pathDisplayTarget.DevicePath}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDTEMC : {pathDisplayTarget.EDIDManufactureCode}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDTEMI : {pathDisplayTarget.EDIDManufactureId}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDTEPC : {pathDisplayTarget.EDIDProductCode}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDTFN : {pathDisplayTarget.FriendlyName}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDTIA : {pathDisplayTarget.IsAvailable}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDTPR : {pathDisplayTarget.PreferredResolution}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDTPSM : {pathDisplayTarget.PreferredSignalMode}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDTTI : {pathDisplayTarget.TargetId}");
                    SharedLogger.logger.Trace($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: PDTVRS : {pathDisplayTarget.VirtualResolutionSupport}");

                    // Create an array of all the important display info we need to record
                    string[] displayInfo = {
                                "WINAPI",
                                displayAdapter.DeviceName.ToString(),
                                pathDisplayAdapter.AdapterId.ToString(),
                                pathDisplayTarget.ConnectorInstance.ToString(),
                                pathDisplayTarget.FriendlyName,
                                pathDisplayTarget.EDIDManufactureCode.ToString(),
                                pathDisplayTarget.EDIDManufactureId.ToString(),
                                pathDisplayTarget.EDIDProductCode.ToString(),
                                pathDisplayTarget.TargetId.ToString(),
                            };

                    // Create a display identifier out of it
                    string displayIdentifier = String.Join("|", displayInfo);
                    // Add it to the list of display identifiers so we can return it
                    displayIdentifiers.Add(displayIdentifier);
                    SharedLogger.logger.Debug($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: Unattached DisplayIdentifier: {displayIdentifier}");
                }

            }

            // Sort the display identifiers
            displayIdentifiers.Sort();

            return displayIdentifiers;
        }

        public static bool ApplyNVIDIAGridTopology(ProfileItem profile)
        {
            SharedLogger.logger.Debug($"ProfileRepository/ApplyNVIDIAGridTopology: Attempting to apply NVIDIA Grid Topology");

            if (!(profile is ProfileItem))
                return false;

            try
            {
                var surroundTopologies =
                    profile.Paths.SelectMany(paths => paths.TargetDisplays)
                        .Select(target => target.SurroundTopology)
                        .Where(topology => topology != null)
                        .Select(topology => topology.ToGridTopology())
                        .ToArray();

                if (surroundTopologies.Length == 0)
                {
                    // The profile we're changing to does not use NVIDIA Surround
                    // So we need to set the Grid Topologies to individual screens
                    // in preparation for the PathInfo step later
                    SharedLogger.logger.Debug($"ProfileRepository/ApplyNVIDIAGridTopology: Changing NVIDIA Grid Topology to individual screens so that we can move them to their final positions in a subsequent step");
                    var currentTopologies = GridTopology.GetGridTopologies();

                    if (currentTopologies.Any(topology => topology.Rows * topology.Columns > 1))
                    {
                        surroundTopologies =
                            GridTopology.GetGridTopologies()
                                .SelectMany(topology => topology.Displays)
                                .Select(displays => new GridTopology(1, 1, new[] { displays }))
                                .ToArray();

                        GridTopology.SetGridTopologies(surroundTopologies, SetDisplayTopologyFlag.MaximizePerformance);
                    }
                } else if (surroundTopologies.Length > 0)
                {
                    SharedLogger.logger.Debug($"ProfileRepository/ApplyNVIDIAGridTopology: Changing NVIDIA Grid Topology to a surround screen so that we can move it to its final positions in a subsequent step");
                    // This profile is an NVIDIA Surround profile 
                    GridTopology.SetGridTopologies(surroundTopologies, SetDisplayTopologyFlag.MaximizePerformance);
                }
                SharedLogger.logger.Debug($"ProfileRepository/ApplyNVIDIAGridTopology: NVIDIA Grid Topology successfully changed");
                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"ProfileRepository/ApplyNVIDIAGridTopology: Exception attempting to apply a change to the NVIDIA Grid Topology.");
                return false;
            }
        }

        public static bool ApplyWindowsDisplayPathInfo(ProfileItem profile)
        {
            SharedLogger.logger.Debug($"ProfileRepository/ApplyWindowsDisplayPathInfo: Moving display screens to where they are supposed to be with this display profile");
            if (!(profile is ProfileItem))
                return false;

            try
            {
                var pathInfos = profile.Paths.Select(paths => paths.ToPathInfo()).Where(info => info != null).ToArray();
                PathInfo.ApplyPathInfos(pathInfos, true, true, true);
                SharedLogger.logger.Debug($"ProfileRepository/ApplyWindowsDisplayPathInfo: Successfully moved display screens to where they are supposed to be");
                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"ProfileRepository/ApplyWindowsDisplayPathInfo: Exception attempting to move the display screens to where they are supposed to be in this display profile.");
                return false;
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

