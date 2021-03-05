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

namespace DisplayMagicianShared
{

    public static class ProfileRepository
    {
        #region Class Variables
        // Common items to the class
        private static List<ProfileItem> _allProfiles = new List<ProfileItem>();
        public static Dictionary<string, bool> _profileValidityLookup = new Dictionary<string, bool>();
        private static bool _profilesLoaded = false;
        public static Version _version = new Version(1, 0, 0);
        private static ProfileItem _currentProfile;

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

                SharedLogger.logger.Debug($"ProfileRepository/ProfileRepository: Creating the Profiles storage folder {AppProfileStoragePath}.");
                // Create the Profile Storage Path if it doesn't exist so that it's avilable for all the program
                if (!Directory.Exists(AppProfileStoragePath))
                {
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

        public static Dictionary<string, bool> ProfileValidityLookup
        {
            get
            {
                if (!_profilesLoaded)
                    // Load the Profiles from storage if they need to be
                    LoadProfiles();

                return _profileValidityLookup;
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
                if (testProfile.Paths.SequenceEqual(_currentProfile.Paths))
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


        public static void UpdateActiveProfile()
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

            if (profile.Paths.SequenceEqual(_currentProfile.Paths))
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
            List<string> connectedDisplayIdentifiers = GenerateAllAvailableDisplayIdentifiers();

            if (_profilesLoaded && _allProfiles.Count > 0)
            {

                _profileValidityLookup.Clear();

                foreach (ProfileItem loadedProfile in AllProfiles)
                {

                    // Check each display in this profile and make sure it's currently available
                    int validDisplayCount = 0;
                    foreach (string profileDisplayIdentifier in loadedProfile.ProfileDisplayIdentifiers)
                    {
                        // If this profile has a display that isn't currently available then we need to say it's a no!
                        if (connectedDisplayIdentifiers.Contains(profileDisplayIdentifier))
                            validDisplayCount++;
                    }
                    if (validDisplayCount == loadedProfile.ProfileDisplayIdentifiers.Count)
                    {
                        SharedLogger.logger.Debug($"ProfileRepository/IsPossibleRefresh: The profile {loadedProfile.Name} is possible!"); 
                        loadedProfile.IsPossible = true;
                        _profileValidityLookup[loadedProfile.Name] = true;
                    }
                        
                    else
                    {
                        SharedLogger.logger.Debug($"ProfileRepository/IsPossibleRefresh: The profile {loadedProfile.Name} is NOT possible!");
                        loadedProfile.IsPossible = false;
                        _profileValidityLookup[loadedProfile.Name] = false;
                    }
                    
                }
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
                        string[] displayInfo = {
                            "NVIDIA",
                            myPhysicalGPU.CorrespondingLogicalGPU.ToString(),
                            myPhysicalGPU.ToString(),
                            myPhysicalGPU.ArchitectInformation.ShortName.ToString(),
                            myPhysicalGPU.ArchitectInformation.Revision.ToString(),
                            myPhysicalGPU.Board.ToString(),
                            myPhysicalGPU.Foundry.ToString(),
                            myPhysicalGPU.GPUId.ToString(),
                            myPhysicalGPU.GPUType.ToString(),
                            //aGPUOutput.OutputId.ToString(),
                            aConnectedDisplayDevice.ConnectionType.ToString(),
                            aConnectedDisplayDevice.DisplayId.ToString()
                        };

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
                    DisplayAdapter displayAdapter = attachedDisplay.Adapter;
                    PathDisplayAdapter pathDisplayAdapter = displayAdapter.ToPathDisplayAdapter();
                    PathDisplaySource pathDisplaySource = attachedDisplay.ToPathDisplaySource();
                    PathDisplayTarget pathDisplayTarget = attachedDisplay.ToPathDisplayTarget();

                    Debug.WriteLine($"ADDN : {attachedDisplay.DeviceName}");
                    Debug.WriteLine($"ADDFN : {attachedDisplay.DisplayFullName}");
                    Debug.WriteLine($"ADDIN : {attachedDisplay.DisplayName}");
                    Debug.WriteLine($"ADDIN : {attachedDisplay.IsAvailable}");
                    Debug.WriteLine($"ADDIGP : {attachedDisplay.IsGDIPrimary}");
                    Debug.WriteLine($"ADDIV : {attachedDisplay.IsValid}");
                    Debug.WriteLine($"ADCSCD : {attachedDisplay.CurrentSetting.ColorDepth}");
                    Debug.WriteLine($"ADCSF : {attachedDisplay.CurrentSetting.Frequency}");
                    Debug.WriteLine($"ADCSIE : {attachedDisplay.CurrentSetting.IsEnable}");
                    Debug.WriteLine($"ADCSII : {attachedDisplay.CurrentSetting.IsInterlaced}");
                    Debug.WriteLine($"ADCSO : {attachedDisplay.CurrentSetting.Orientation}");
                    Debug.WriteLine($"ADCSOSM : {attachedDisplay.CurrentSetting.OutputScalingMode}");
                    Debug.WriteLine($"ADCSP : {attachedDisplay.CurrentSetting.Position}");
                    Debug.WriteLine($"ADCSR : {attachedDisplay.CurrentSetting.Resolution}");
                    Debug.WriteLine($"DP : {displayAdapter.DevicePath}");
                    Debug.WriteLine($"DK : {displayAdapter.DeviceKey}");
                    Debug.WriteLine($"DN : {displayAdapter.DeviceName}");
                    Debug.WriteLine($"DK : {displayAdapter.DeviceKey}");
                    Debug.WriteLine($"AI : {pathDisplayAdapter.AdapterId}");
                    Debug.WriteLine($"AIDP : {pathDisplayAdapter.DevicePath}");
                    Debug.WriteLine($"AIII : {pathDisplayAdapter.IsInvalid}");
                    Debug.WriteLine($"DDA : {displayAdapter.DeviceName}");
                    Debug.WriteLine($"PDSA : {pathDisplaySource.Adapter}");
                    Debug.WriteLine($"PDSCDS : {pathDisplaySource.CurrentDPIScale}");
                    Debug.WriteLine($"PDSDN : {pathDisplaySource.DisplayName}");
                    Debug.WriteLine($"PDSMDS : {pathDisplaySource.MaximumDPIScale}");
                    Debug.WriteLine($"PDSRDS : {pathDisplaySource.RecommendedDPIScale}");
                    Debug.WriteLine($"PDSSI : {pathDisplaySource.SourceId}");
                    Debug.WriteLine($"PDTA : {pathDisplayTarget.Adapter}");
                    Debug.WriteLine($"PDTCI : {pathDisplayTarget.ConnectorInstance}");
                    Debug.WriteLine($"PDTDP : {pathDisplayTarget.DevicePath}");
                    Debug.WriteLine($"PDTEMC : {pathDisplayTarget.EDIDManufactureCode}");
                    Debug.WriteLine($"PDTEMI : {pathDisplayTarget.EDIDManufactureId}");
                    Debug.WriteLine($"PDTEPC : {pathDisplayTarget.EDIDProductCode}");
                    Debug.WriteLine($"PDTFN : {pathDisplayTarget.FriendlyName}");
                    Debug.WriteLine($"PDTIA : {pathDisplayTarget.IsAvailable}");
                    Debug.WriteLine($"PDTPR : {pathDisplayTarget.PreferredResolution}");
                    Debug.WriteLine($"PDTPSM : {pathDisplayTarget.PreferredSignalMode}");
                    Debug.WriteLine($"PDTTI : {pathDisplayTarget.TargetId}");
                    Debug.WriteLine($"PDTVRS : {pathDisplayTarget.VirtualResolutionSupport}");

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
                    SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                }

            }

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
                            string[] displayInfo = {
                                "NVIDIA",
                                myPhysicalGPU.CorrespondingLogicalGPU.ToString(),
                                myPhysicalGPU.ToString(),
                                myPhysicalGPU.ArchitectInformation.ShortName.ToString(),
                                myPhysicalGPU.ArchitectInformation.Revision.ToString(),
                                myPhysicalGPU.Board.ToString(),
                                myPhysicalGPU.Foundry.ToString(),
                                myPhysicalGPU.GPUId.ToString(),
                                myPhysicalGPU.GPUType.ToString(),
                                //aDisplayDevice.Output.OutputId.ToString(),
                                aDisplayDevice.ConnectionType.ToString(),
                                aDisplayDevice.DisplayId.ToString(),
                            };

                            // Create a display identifier out of it
                            string displayIdentifier = String.Join("|", displayInfo);
                            // Add it to the list of display identifiers so we can return it
                            displayIdentifiers.Add(displayIdentifier);
                            SharedLogger.logger.Debug($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                        }
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
                List<UnAttachedDisplay> unattachedDisplayDevices = UnAttachedDisplay.GetUnAttachedDisplays().ToList();

                SharedLogger.logger.Debug($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: We are using the standard Windows Display API to figure out what display devices are attached and available. There are {attachedDisplayDevices.Count} display devices attached and {unattachedDisplayDevices.Count} devices unattached.");


                foreach (Display attachedDisplay in attachedDisplayDevices)
                {
                    DisplayAdapter displayAdapter = attachedDisplay.Adapter;
                    PathDisplayAdapter pathDisplayAdapter = displayAdapter.ToPathDisplayAdapter();
                    PathDisplaySource pathDisplaySource = attachedDisplay.ToPathDisplaySource();
                    PathDisplayTarget pathDisplayTarget = attachedDisplay.ToPathDisplayTarget();

                    Debug.WriteLine($"ADDN : {attachedDisplay.DeviceName}");
                    Debug.WriteLine($"ADDFN : {attachedDisplay.DisplayFullName}");
                    Debug.WriteLine($"ADDIN : {attachedDisplay.DisplayName}");
                    Debug.WriteLine($"ADDIN : {attachedDisplay.IsAvailable}");
                    Debug.WriteLine($"ADDIGP : {attachedDisplay.IsGDIPrimary}");
                    Debug.WriteLine($"ADDIV : {attachedDisplay.IsValid}");
                    Debug.WriteLine($"ADCSCD : {attachedDisplay.CurrentSetting.ColorDepth}");
                    Debug.WriteLine($"ADCSF : {attachedDisplay.CurrentSetting.Frequency}");
                    Debug.WriteLine($"ADCSIE : {attachedDisplay.CurrentSetting.IsEnable}");
                    Debug.WriteLine($"ADCSII : {attachedDisplay.CurrentSetting.IsInterlaced}");
                    Debug.WriteLine($"ADCSO : {attachedDisplay.CurrentSetting.Orientation}");
                    Debug.WriteLine($"ADCSOSM : {attachedDisplay.CurrentSetting.OutputScalingMode}");
                    Debug.WriteLine($"ADCSP : {attachedDisplay.CurrentSetting.Position}");
                    Debug.WriteLine($"ADCSR : {attachedDisplay.CurrentSetting.Resolution}");
                    Debug.WriteLine($"DP : {displayAdapter.DevicePath}");
                    Debug.WriteLine($"DK : {displayAdapter.DeviceKey}");
                    Debug.WriteLine($"DN : {displayAdapter.DeviceName}");
                    Debug.WriteLine($"DK : {displayAdapter.DeviceKey}");
                    Debug.WriteLine($"AI : {pathDisplayAdapter.AdapterId}");
                    Debug.WriteLine($"AIDP : {pathDisplayAdapter.DevicePath}");
                    Debug.WriteLine($"AIII : {pathDisplayAdapter.IsInvalid}");
                    Debug.WriteLine($"DDA : {displayAdapter.DeviceName}");
                    Debug.WriteLine($"PDSA : {pathDisplaySource.Adapter}");
                    Debug.WriteLine($"PDSCDS : {pathDisplaySource.CurrentDPIScale}");
                    Debug.WriteLine($"PDSDN : {pathDisplaySource.DisplayName}");
                    Debug.WriteLine($"PDSMDS : {pathDisplaySource.MaximumDPIScale}");
                    Debug.WriteLine($"PDSRDS : {pathDisplaySource.RecommendedDPIScale}");
                    Debug.WriteLine($"PDSSI : {pathDisplaySource.SourceId}");
                    Debug.WriteLine($"PDTA : {pathDisplayTarget.Adapter}");
                    Debug.WriteLine($"PDTCI : {pathDisplayTarget.ConnectorInstance}");
                    Debug.WriteLine($"PDTDP : {pathDisplayTarget.DevicePath}");
                    Debug.WriteLine($"PDTEMC : {pathDisplayTarget.EDIDManufactureCode}");
                    Debug.WriteLine($"PDTEMI : {pathDisplayTarget.EDIDManufactureId}");
                    Debug.WriteLine($"PDTEPC : {pathDisplayTarget.EDIDProductCode}");
                    Debug.WriteLine($"PDTFN : {pathDisplayTarget.FriendlyName}");
                    Debug.WriteLine($"PDTIA : {pathDisplayTarget.IsAvailable}");
                    Debug.WriteLine($"PDTPR : {pathDisplayTarget.PreferredResolution}");
                    Debug.WriteLine($"PDTPSM : {pathDisplayTarget.PreferredSignalMode}");
                    Debug.WriteLine($"PDTTI : {pathDisplayTarget.TargetId}");
                    Debug.WriteLine($"PDTVRS : {pathDisplayTarget.VirtualResolutionSupport}");

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
                    SharedLogger.logger.Debug($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: Attached DisplayIdentifier: {displayIdentifier}");
                }

                foreach (UnAttachedDisplay unattachedDisplay in unattachedDisplayDevices)
                {
                    DisplayAdapter displayAdapter = unattachedDisplay.Adapter;
                    PathDisplayAdapter pathDisplayAdapter = displayAdapter.ToPathDisplayAdapter();
                    PathDisplaySource pathDisplaySource = unattachedDisplay.ToPathDisplaySource();
                    PathDisplayTarget pathDisplayTarget = unattachedDisplay.ToPathDisplayTarget();

                    Debug.WriteLine($"ADDN : {unattachedDisplay.DeviceName}");
                    Debug.WriteLine($"ADDFN : {unattachedDisplay.DisplayFullName}");
                    Debug.WriteLine($"ADDIN : {unattachedDisplay.DisplayName}");
                    Debug.WriteLine($"ADDIN : {unattachedDisplay.IsAvailable}");
                    Debug.WriteLine($"ADDIV : {unattachedDisplay.IsValid}");
                    Debug.WriteLine($"DP : {displayAdapter.DevicePath}");
                    Debug.WriteLine($"DK : {displayAdapter.DeviceKey}");
                    Debug.WriteLine($"DN : {displayAdapter.DeviceName}");
                    Debug.WriteLine($"DK : {displayAdapter.DeviceKey}");
                    Debug.WriteLine($"AI : {pathDisplayAdapter.AdapterId}");
                    Debug.WriteLine($"AIDP : {pathDisplayAdapter.DevicePath}");
                    Debug.WriteLine($"AIII : {pathDisplayAdapter.IsInvalid}");
                    Debug.WriteLine($"PDSA : {pathDisplaySource.Adapter}");
                    Debug.WriteLine($"PDSCDS : {pathDisplaySource.CurrentDPIScale}");
                    Debug.WriteLine($"PDSDN : {pathDisplaySource.DisplayName}");
                    Debug.WriteLine($"PDSMDS : {pathDisplaySource.MaximumDPIScale}");
                    Debug.WriteLine($"PDSRDS : {pathDisplaySource.RecommendedDPIScale}");
                    Debug.WriteLine($"PDSSI : {pathDisplaySource.SourceId}");
                    Debug.WriteLine($"PDTA : {pathDisplayTarget.Adapter}");
                    Debug.WriteLine($"PDTCI : {pathDisplayTarget.ConnectorInstance}");
                    Debug.WriteLine($"PDTDP : {pathDisplayTarget.DevicePath}");
                    Debug.WriteLine($"PDTEMC : {pathDisplayTarget.EDIDManufactureCode}");
                    Debug.WriteLine($"PDTEMI : {pathDisplayTarget.EDIDManufactureId}");
                    Debug.WriteLine($"PDTEPC : {pathDisplayTarget.EDIDProductCode}");
                    Debug.WriteLine($"PDTFN : {pathDisplayTarget.FriendlyName}");
                    Debug.WriteLine($"PDTIA : {pathDisplayTarget.IsAvailable}");
                    Debug.WriteLine($"PDTPR : {pathDisplayTarget.PreferredResolution}");
                    Debug.WriteLine($"PDTPSM : {pathDisplayTarget.PreferredSignalMode}");
                    Debug.WriteLine($"PDTTI : {pathDisplayTarget.TargetId}");
                    Debug.WriteLine($"PDTVRS : {pathDisplayTarget.VirtualResolutionSupport}");

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

