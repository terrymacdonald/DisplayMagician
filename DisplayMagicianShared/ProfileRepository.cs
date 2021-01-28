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
                NvAPIWrapper.NVIDIA.Initialize();

                // Create the Profile Storage Path if it doesn't exist so that it's avilable for all the program
                if (!Directory.Exists(AppProfileStoragePath))
                {
                    Directory.CreateDirectory(AppProfileStoragePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ShortcutItem/Instansiation exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // ignored
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

            //Doublecheck it's been added
            if (ContainsProfile(profile))
            {
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
                    Console.WriteLine($"ProfileRepository/RemoveProfile exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
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
                    Console.WriteLine($"ProfileRepository/RemoveProfile exception 2: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
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
                    Console.WriteLine($"ProfileRepository/RemoveProfile exception 3: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
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

        public static bool ContainsCurrentProfile()
        {
            if (!(_currentProfile is ProfileItem))
                return false;

            foreach (ProfileItem testProfile in _allProfiles)
            {
                if (testProfile.Paths.SequenceEqual(_currentProfile.Paths))
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
                        return;
                    }
                }
            }
            _currentProfile = activeProfile;

        }


        public static ProfileItem GetActiveProfile()
        {
            //UpdateActiveProfile();

            if (!(_currentProfile is ProfileItem))
                return null;
            return _currentProfile;
        }

        public static bool IsActiveProfile(ProfileItem profile)
        {
            //UpdateActiveProfile();

            if (!(_currentProfile is ProfileItem))
                return false;

            if (!(profile is ProfileItem))
                return false;

            if (profile.Paths.SequenceEqual(_currentProfile.Paths))
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
                        Console.WriteLine($"ProfileRepository/LoadProfiles exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                        Console.WriteLine($"Unable to load Profiles from JSON file {_profileStorageJsonFileName}: " + ex.Message);
                    }

                    ProfileItem myCurrentProfile = new ProfileItem
                    {
                        Name = "Current Display Profile",
                        Paths = PathInfo.GetActivePaths().Select(info => new DisplayMagicianShared.Topology.Path(info)).ToArray()
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

                    // Sort the profiles alphabetically
                    _allProfiles.Sort();

                }
            } else
            {
                // If we get here, then we don't have any profiles saved!
                // So we gotta start from scratch
                // Create a new profile based on our current display settings
                ProfileItem myCurrentProfile = new ProfileItem
                {
                    Name = "Current Display Profile",
                    Paths = PathInfo.GetActivePaths().Select(info => new DisplayMagicianShared.Topology.Path(info)).ToArray()
                };

                _currentProfile = myCurrentProfile;

                // Save a profile Icon to the profile
                _currentProfile.ProfileIcon = new ProfileIcon(_currentProfile);
                _currentProfile.ProfileBitmap = _currentProfile.ProfileIcon.ToBitmap(256, 256);
            }
            _profilesLoaded = true;
            return true;
        }

        public static bool SaveProfiles()
        {

            if (!Directory.Exists(AppProfileStoragePath))
            {
                try
                {
                    Directory.CreateDirectory(AppProfileStoragePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ProfileRepository/SaveProfiles exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                    Console.WriteLine($"Unable to create Profile folder {AppProfileStoragePath}: " + ex.Message);

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
                Console.WriteLine($"ProfileRepository/SaveProfiles exception 2: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                Console.WriteLine($"Unable to save Profile JSON file {_profileStorageJsonFileName}: " + ex.Message);
            }

            return false;
        }

        private static void SaveProfileIconToCache(ProfileItem profile)
        {

            // Work out the name of the Profile we'll save.
            profile.SavedProfileIconCacheFilename = System.IO.Path.Combine(AppProfileStoragePath, string.Concat(@"profile-", profile.UUID, @".ico"));

            MultiIcon ProfileIcon;
            try
            {
                ProfileIcon = profile.ProfileIcon.ToIcon();
                ProfileIcon.Save(profile.SavedProfileIconCacheFilename, MultiIconFormat.ICO);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfileRepository/SaveProfileIconToCache exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // If we fail to create an icon based on the Profile, then we use the standard DisplayMagician profile one.
                // Which is created on program startup.
                File.Copy(AppDisplayMagicianIconFilename, profile.SavedProfileIconCacheFilename);

            }
        }



        public static List<string> GenerateProfileDisplayIdentifiers()
        {
            List<string> displayIdentifiers = new List<string>();

            // If the Video Card is an NVidia, then we should generate specific NVidia displayIdentifiers
            bool isNvidia = false;
            NvAPIWrapper.GPU.PhysicalGPU[] myPhysicalGPUs = null;
            try
            {
                myPhysicalGPUs = NvAPIWrapper.GPU.PhysicalGPU.GetPhysicalGPUs();
                isNvidia = true;
            }
            catch (Exception ex)
            { }

            if (isNvidia && myPhysicalGPUs != null && myPhysicalGPUs.Length > 0)
            {

                foreach (NvAPIWrapper.GPU.PhysicalGPU myPhysicalGPU in myPhysicalGPUs)
                {
                    // get a list of all physical outputs attached to the GPUs
                    NvAPIWrapper.GPU.GPUOutput[] myGPUOutputs = myPhysicalGPU.ActiveOutputs;
                    foreach (NvAPIWrapper.GPU.GPUOutput aGPUOutput in myGPUOutputs)
                    {
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

                }

            }

            return displayIdentifiers;
        }

        public static List<string> GenerateAllAvailableDisplayIdentifiers()
        {
            List<string> displayIdentifiers = new List<string>();

            // If the Video Card is an NVidia, then we should generate specific NVidia displayIdentifiers
            bool isNvidia = false;
            NvAPIWrapper.GPU.PhysicalGPU[] myPhysicalGPUs = null;
            try
            {
                myPhysicalGPUs = NvAPIWrapper.GPU.PhysicalGPU.GetPhysicalGPUs();
                isNvidia = true;
            }
            catch (Exception ex)
            { }

            if (isNvidia && myPhysicalGPUs != null && myPhysicalGPUs.Length > 0)
            {

                foreach (NvAPIWrapper.GPU.PhysicalGPU myPhysicalGPU in myPhysicalGPUs)
                {
                    // get a list of all physical outputs attached to the GPUs
                    NvAPIWrapper.Display.DisplayDevice[] allDisplayDevices = myPhysicalGPU.GetConnectedDisplayDevices(ConnectedIdsFlag.None);
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

                }

            }

            return displayIdentifiers;
        }

        public static bool ApplyNVIDIAGridTopology(ProfileItem profile)
        {
            Debug.Print("ProfileRepository.ApplyTopology()");

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
                    // This profile is an NVIDIA Surround profile 
                    GridTopology.SetGridTopologies(surroundTopologies, SetDisplayTopologyFlag.MaximizePerformance);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfileRepository/ApplyTopology exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return false;
            }
        }

        public static bool ApplyWindowsDisplayPathInfo(ProfileItem profile)
        {
            Debug.Print("ProfileRepository.ApplyPathInfo()");
            if (!(profile is ProfileItem))
                return false;

            try
            {
                var pathInfos = profile.Paths.Select(paths => paths.ToPathInfo()).Where(info => info != null).ToArray();
                PathInfo.ApplyPathInfos(pathInfos, true, true, true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfileRepository/ApplyPathInfo exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return false;
            }

        }

        public static bool IsValidFilename(string testName)
        {
            string strTheseAreInvalidFileNameChars = new string(System.IO.Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(testName)) { return false; };

            return true;
        }

        public static string GetValidFilename(string uncheckedFilename)
        {
            string invalid = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
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

