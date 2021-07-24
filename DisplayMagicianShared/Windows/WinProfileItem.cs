using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DisplayMagicianShared.Resources;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using IWshRuntimeLibrary;
//using ATI.ADL;
//using WK.Libraries.HotkeyListenerNS;

namespace DisplayMagicianShared.Windows
{

    /*// Struct to be used as the AMD Profile
    [JsonObject(MemberSerialization.Fields)]
    public struct AMDProfile
    {
        public List<AMDAdapter> Adapters;
    }

    // Struct to store the Display
    [JsonObject(MemberSerialization.Fields)]
    public struct AMDAdapter
    {
        public int AdapterIndex;
        public string AdapterName;
        public string DisplayName;
        [JsonProperty]
        public ADLAdapterInfoX2 AdapterInfoX2;
        public List<AMDDisplay> Displays;
    }

    // Struct to store the Display
    [JsonObject(MemberSerialization.Fields)]
    public struct AMDDisplay
    {
        public string DisplayName;
        public string DisplayConnector;
        public string UDID;
        [JsonRequired]
        public List<ADLMode> DisplayModes;
        public bool HDRSupported;
        public bool HDREnabled;
        public bool IsEyefinity;

    }*/

    public class WinProfileItem : ProfileItem, IComparable
    {
        private static List<WinProfileItem> _allSavedProfiles = new List<WinProfileItem>();
        private ProfileIcon _profileIcon;
        private Bitmap _profileBitmap, _profileShortcutBitmap;
        private List<string> _profileDisplayIdentifiers = new List<string>();
        private List<ScreenPosition> _screens;
        private WINDOWS_DISPLAY_CONFIG _displayConfig = new WINDOWS_DISPLAY_CONFIG();
        private static readonly string uuidV4Regex = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";

        private string _uuid = "";
        private bool _isPossible = false;
        private Keys _hotkey = Keys.None;

                
        public WinProfileItem()
        {
        }

        public new static Version Version = new Version(2, 1);

        #region Instance Properties

        [JsonIgnore]
        public override bool IsPossible
        {
            get
            {
                // Return the cached answer
                return _isPossible;
            }
            set
            {
                _isPossible = value;
            }
        }

        [JsonIgnore]
        public override bool IsActive
        {
            get
            {

                if (this.Equals(ProfileRepository.CurrentProfile))
                    return true;
                else
                    return false;

            }
        }

        public override string Driver { get; } = "AMD";

        public override string Name { get; set; }

        //public Topology.Path[] Paths { get; set; } = new Topology.Path[0];

        [JsonRequired]
        public WINDOWS_DISPLAY_CONFIG DisplayConfig
        {
            get
            {
                return _displayConfig;
            }
            set
            {
                _displayConfig = value;
            }
        }
            

        public override List<string> ProfileDisplayIdentifiers
        {
            get
            {
                if (_profileDisplayIdentifiers.Count == 0)
                {
                    _profileDisplayIdentifiers = WinLibrary.GetLibrary().GetCurrentDisplayIdentifiers();
                }
                return _profileDisplayIdentifiers;
            }
            set
            {
                if (value is List<string>)
                    _profileDisplayIdentifiers = value;
            }
        }

        [JsonIgnore]
        public override List<ScreenPosition> Screens
        {
            get
            {
                if (_screens.Count == 0)
                {
                    _screens = GetScreenPositions();
                }
                return _screens;
            }
            set
            {
                _screens = value;
            }
        }



        //[JsonConverter(typeof(CustomBitmapConverter))]
        [JsonIgnore]
        public override Bitmap ProfileBitmap
        {
            get
            {
                /*if (!ProfileRepository.ProfilesLoaded)
                    return null;*/

                if (_profileBitmap != null)
                    return _profileBitmap;
                else
                {
                    _profileBitmap = this.ProfileIcon.ToBitmap(256, 256);
                    return _profileBitmap;
                }
            }
            set
            {
                _profileBitmap = value;
            }

        }


        //[JsonConverter(typeof(CustomBitmapConverter))]
        [JsonIgnore]
        public override Bitmap ProfileTightestBitmap
        {
            get
            {
                if (_profileShortcutBitmap != null)
                    return _profileShortcutBitmap;
                else
                {
                    _profileShortcutBitmap = this.ProfileIcon.ToTightestBitmap();
                    return _profileShortcutBitmap;
                }
            }
            set
            {
                _profileShortcutBitmap = value;
            }

        }        

        #endregion

        public override bool IsValid()
        {

            if (ProfileIcon is ProfileIcon &&
                System.IO.File.Exists(SavedProfileIconCacheFilename) &&
                ProfileBitmap is Bitmap &&
                ProfileTightestBitmap is Bitmap &&
                ProfileDisplayIdentifiers.Count > 0)
            {
                if (DisplayConfig.displayConfigModes.Length > 0 && DisplayConfig.displayConfigPaths.Length > 0)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }



        public bool CopyTo(WinProfileItem profile, bool overwriteId = true)
        {
            if (!(profile is WinProfileItem))
                return false;

            if (overwriteId == true)
                profile.UUID = UUID;

            // Copy all our profile data over to the other profile
            profile.Name = Name;
            profile.DisplayConfig = DisplayConfig;
            profile.ProfileIcon = ProfileIcon;
            profile.SavedProfileIconCacheFilename = SavedProfileIconCacheFilename;
            profile.ProfileBitmap = ProfileBitmap;
            profile.ProfileTightestBitmap = ProfileTightestBitmap;
            profile.ProfileDisplayIdentifiers = ProfileDisplayIdentifiers;
            //profile.Screens = Screens;
            return true;
        }

        public override bool PreSave()
        {
            // Prepare our profile data for saving
            if (_profileDisplayIdentifiers.Count == 0)
            {
                _profileDisplayIdentifiers = WinLibrary.GetLibrary().GetCurrentDisplayIdentifiers();
            }

            // Return if it is valid and we should continue
            return IsValid();
        }

        public override void RefreshPossbility()
        {
            // Check each display in this profile and make sure it's currently available
            int validDisplayCount = 0;

            //validDisplayCount = (from connectedDisplay in ProfileRepository.ConnectedDisplayIdentifiers select connectedDisplay == profileDisplayIdentifier).Count();

            foreach (string profileDisplayIdentifier in ProfileDisplayIdentifiers)
            {
                // If this profile has a display that isn't currently available then we need to say it's a no!
                if (ProfileRepository.ConnectedDisplayIdentifiers.Any(s => profileDisplayIdentifier.Equals(s)))
                {
                    SharedLogger.logger.Trace($"ProfileItem/RefreshPossbility: We found the display in the profile {Name} with profileDisplayIdentifier {profileDisplayIdentifier} is connected now.");
                    validDisplayCount++;
                }
                else
                {
                    SharedLogger.logger.Warn($"ProfileItem/RefreshPossbility: We found the display in the profile {Name} with profileDisplayIdentifier {profileDisplayIdentifier} is NOT currently connected, so this profile cannot be used.");
                }

            }
            if (validDisplayCount == ProfileDisplayIdentifiers.Count)
            {

                SharedLogger.logger.Debug($"ProfileRepository/IsPossibleRefresh: The profile {Name} is possible!");
                _isPossible = true;

            }
            else
            {
                SharedLogger.logger.Debug($"ProfileRepository/IsPossibleRefresh: The profile {Name} is NOT possible!");
                _isPossible = false;
            }

        }

        public override bool CreateProfileFromCurrentDisplaySettings()
        {

            WinLibrary winLibrary = WinLibrary.GetLibrary();
            if (winLibrary.IsInstalled)
            {
                // Create the profile data from the current config
                _displayConfig = winLibrary.GetActiveConfig();

                // Now, since the ActiveProfile has changed, we need to regenerate screen positions
                _screens = GetScreenPositions();

                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool PerformPostLoadingTasks()
        {
            // First thing we do is to set up the Screens
            _screens = GetScreenPositions();

            return true;
        }

        public override List<ScreenPosition> GetScreenPositions()
        {

            // Now we create the screens structure from the AMD profile information
            _screens = new List<ScreenPosition>();

            if ( _displayConfig.displayConfigModes.Length > 0 && _displayConfig.displayConfigPaths.Length > 0)
            {
                foreach ( var adapter in _displayConfig.AdapterConfigs)
                {
                    foreach (var display in adapter.Displays)
                    {
                        foreach (var mode in display.DisplayModes)
                        {
                            ScreenPosition screen = new ScreenPosition();
                            screen.Library = "AMD";
                            screen.Name = display.DisplayName;
                            screen.DisplayConnector = display.DisplayConnector; 
                            screen.ScreenX = mode.XPos;
                            screen.ScreenY = mode.YPos;
                            screen.ScreenWidth = mode.XRes;
                            screen.ScreenHeight = mode.YRes;

                            // If we're at the 0,0 coordinate then we're the primary monitor
                            if (screen.ScreenX == 0 && screen.ScreenY == 0)
                            {
                                screen.IsPrimary = true;
                            }

                            // HDR information
                            if (display.HDRSupported)
                            {
                                screen.HDRSupported = true;
                                if (display.HDREnabled)
                                {
                                    screen.HDREnabled = true;
                                }
                                else
                                {
                                    screen.HDREnabled = false;
                                }
                                
                            }
                            else
                            {
                                screen.HDRSupported = false;
                                screen.HDREnabled = false;
                            }
                            
                            // Spanned screen options
                            if (display.IsEyefinity)
                            {
                                screen.IsSpanned = true;
                                screen.Colour = Color.FromArgb(200, 237, 28, 36); // represents AMD Red
                                screen.SpannedName = "AMD Eyefinity";
                            }
                            else
                            {                                
                                screen.IsSpanned = false;
                                screen.Colour = Color.FromArgb(255, 195, 195, 195); // represents normal screen colour
                            }


                            // Figure out features

                            //ATI.ADL.ADL.ConvertDisplayModeFlags(mode.ModeValue);

                            //screen.Features = mode.ModeValue;

                            _screens.Add(screen);
                        }
                    }
                }
            }
            

            return _screens;
        }


        // The public override for the Object.Equals
        public override bool Equals(object obj)
        {
            return this.Equals(obj as WinProfileItem);
        }

        // Profiles are equal if their Viewports are equal
        public bool Equals(WinProfileItem other)
        {

            // If parameter is null, return false.
            if (other is null)
                return false;

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, other))
                return true;

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != other.GetType())
                return false;

            // If the DisplayConfig's equal each other
            if (DisplayConfig.Equals(other.DisplayConfig))
                return false;

            // Check if the profile identifiers are not the same, then return false
            int foundDICount = 0;
            foreach (string profileDI in ProfileDisplayIdentifiers)
            {

                if (other.ProfileDisplayIdentifiers.Contains(profileDI))
                {
                    foundDICount++;
                    continue;
                }

            }

            if (foundDICount != other.ProfileDisplayIdentifiers.Count)
                return false;

            foundDICount = 0;
            foreach (string profileDI in other.ProfileDisplayIdentifiers)
            {

                if (ProfileDisplayIdentifiers.Contains(profileDI))
                {
                    foundDICount++;
                    continue;
                }

            }

            if (foundDICount != ProfileDisplayIdentifiers.Count)
                return false;

            // Check whether the profiles' properties are equal
            // We need to exclude the name as the name is solely for saving to disk
            // and displaying to the user. 
            // Two profiles are equal only when they have the same viewport data
            // The data may be in different orders each run, so we need to compare them one by one

            int foundPathsCount = 0;
            int foundOtherPathsCount = 0;

            // TODO: Make this work in AMD land
            /*foreach (Topology.Path profilePath in Paths)
            {
                if (other.Paths.Contains(profilePath))
                {
                    foundPathsCount++;
                    continue;
                }
                
            }
            foreach (Topology.Path otherPath in other.Paths)
            {
                if (Paths.Contains(otherPath))
                {
                    foundOtherPathsCount++;
                    continue;
                }
            }*/


            if (foundPathsCount == foundOtherPathsCount)
                return true;
            else
                return false;
        }

        // If Equals() returns true for this object compared to  another
        // then GetHashCode() must return the same value for these objects.
        /*public override int GetHashCode()
        {

            // Get hash code for the Viewports field if it is not null.
            int hashPaths = Paths == null ? 0 : Paths.GetHashCode();

            //Calculate the hash code for the product.
            return hashPaths;

        }*/
        public override int GetHashCode()
        {

            // Get hash code for the ProfileDisplayIdentifiers field if it is not null.
            int hashIds = ProfileDisplayIdentifiers == null ? 0 : ProfileDisplayIdentifiers.GetHashCode();

            // Get ProfileData too
            int hashProfileData = DisplayConfig.GetHashCode();

            // Calculate the hash code for the product.
            return (hashIds, hashProfileData).GetHashCode();

        }


        public override string ToString()
        {
            return (Name ?? Language.UN_TITLED_PROFILE);
        }

    }

    // Custom Equality comparer for the Profile class
    // Allows us to use 'Contains'
    class AMDProfileComparer : IEqualityComparer<WinProfileItem>
    {
        // Products are equal if their names and product numbers are equal.
        /*public bool Equals(AMDProfileItem x, AMDProfileItem y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (x is null || y is null)
                return false;

            // Check whether the profiles' properties are equal
            // We need to exclude the name as the name is solely for saving to disk
            // and displaying to the user. 
            // Two profiles are equal only when they have the same viewport data
            if (x.Paths.SequenceEqual(y.Paths))
                return true;
            else
                return false;
        }*/

        public bool Equals(WinProfileItem x, WinProfileItem y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (x is null || y is null)
                return false;
            
            // Check if the profile identifiers are not the same, then return false
            int foundDICount = 0;
            foreach (string profileDI in x.ProfileDisplayIdentifiers)
            {
                if (y.ProfileDisplayIdentifiers.Contains(profileDI))
                {
                    foundDICount++;
                    continue;
                }

            }
            if (foundDICount != x.ProfileDisplayIdentifiers.Count)
                return false;

            foundDICount = 0;
            foreach (string profileDI in y.ProfileDisplayIdentifiers)
            {
                if (x.ProfileDisplayIdentifiers.Contains(profileDI))
                {
                    foundDICount++;
                    continue;
                }

            }
            if (foundDICount != y.ProfileDisplayIdentifiers.Count)
                return false;


            // Now we need to check the Display Configs themselves
            if (x.DisplayConfig.Equals(y.DisplayConfig))
                return false;

            return true;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.
        /*public int GetHashCode(AMDProfileItem profile)
        {

            // Check whether the object is null
            if (profile is null) return 0;

            // Get hash code for the Viewports field if it is not null.
            int hashPaths = profile.Paths == null ? 0 : profile.Paths.GetHashCode();

            //Calculate the hash code for the product.
            return hashPaths;

        }*/
        // Modified the GetHashCode to compare the displayidentifier
        public int GetHashCode(WinProfileItem profile)
        {

            // Check whether the object is null
            if (profile is null) return 0;

            // Get hash code for the ProfileDisplayIdentifiers field if it is not null.
            int hashIds = profile.ProfileDisplayIdentifiers == null ? 0 : profile.ProfileDisplayIdentifiers.GetHashCode();

            // Get hash code for the Paths
            int hashProfileData = profile.DisplayConfig.GetHashCode();

            //Calculate the hash code for the product.
            return (hashIds, hashProfileData).GetHashCode();

        }
    }
}