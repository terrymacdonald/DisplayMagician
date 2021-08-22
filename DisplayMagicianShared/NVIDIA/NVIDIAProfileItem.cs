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
using DisplayMagicianShared.Windows;

namespace DisplayMagicianShared.NVIDIA
{
    public class NVIDIAProfileItem : ProfileItem, IComparable
    {
        private static List<NVIDIAProfileItem> _allSavedProfiles = new List<NVIDIAProfileItem>();
        private ProfileIcon _profileIcon;
        private Bitmap _profileBitmap, _profileShortcutBitmap;
        private List<string> _profileDisplayIdentifiers = new List<string>();
        private List<ScreenPosition> _screens;
        private NVIDIA_DISPLAY_CONFIG _nvidiaDisplayConfig = new NVIDIA_DISPLAY_CONFIG();
        private WINDOWS_DISPLAY_CONFIG _windowsDisplayConfig = new WINDOWS_DISPLAY_CONFIG();
        private static readonly string uuidV4Regex = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";

        private string _uuid = "";
        private bool _isPossible = false;
        private Keys _hotkey = Keys.None;

        public NVIDIAProfileItem()
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

        public override string Driver { get; } = "NVIDIA";

        public override string Name { get; set; }

        [JsonRequired]
        public NVIDIA_DISPLAY_CONFIG NVIDIADisplayConfig
        {
            get
            {
                return _nvidiaDisplayConfig;
            }
            set
            {
                _nvidiaDisplayConfig = value;
            }
        }

        [JsonRequired]
        public WINDOWS_DISPLAY_CONFIG WindowsDisplayConfig
        {
            get
            {
                return _windowsDisplayConfig;
            }
            set
            {
                _windowsDisplayConfig = value;
            }
        }


        [JsonIgnore]
        public override ProfileIcon ProfileIcon
        {
            get
            {
                if (_profileIcon != null)
                    return _profileIcon;
                else
                {
                    _profileIcon = new ProfileIcon(this);
                    return _profileIcon;
                }
            }
            set
            {
                _profileIcon = value;
            }

        }

        public override List<string> ProfileDisplayIdentifiers
        {
            get
            {
                if (_profileDisplayIdentifiers.Count == 0)
                {
                    _profileDisplayIdentifiers = NVIDIALibrary.GetLibrary().GetCurrentDisplayIdentifiers();
                }
                return _profileDisplayIdentifiers;
            }
            set
            {
                if (value is List<string>)
                    _profileDisplayIdentifiers = value;
            }
        }

        [JsonRequired]
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


        [JsonConverter(typeof(CustomBitmapConverter))]
        public new Bitmap ProfileBitmap
        {
            get
            {
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

        #endregion

        public override bool IsValid()
        {

            if (Paths != null &&
                ProfileIcon is ProfileIcon &&
                System.IO.File.Exists(SavedProfileIconCacheFilename) &&
                ProfileBitmap is Bitmap &&
                ProfileTightestBitmap is Bitmap &&
                ProfileDisplayIdentifiers.Count > 0)
                return true;
            else
                return false;
        }



        public bool CopyTo(NVIDIAProfileItem profile, bool overwriteId = true)
        {
            if (!(profile is NVIDIAProfileItem))
                return false;

            if (overwriteId == true)
                profile.UUID = UUID;

            // Copy all our profile data over to the other profile
            profile.Name = Name;
            //profile.Paths = Paths;
            profile.ProfileIcon = ProfileIcon;
            profile.SavedProfileIconCacheFilename = SavedProfileIconCacheFilename;
            profile.ProfileBitmap = ProfileBitmap;
            profile.ProfileTightestBitmap = ProfileTightestBitmap;
            profile.ProfileDisplayIdentifiers = ProfileDisplayIdentifiers;
            return true;
        }

        public override bool PreSave()
        {
            // Prepare our profile data for saving
            if (_profileDisplayIdentifiers.Count == 0)
            {
                _profileDisplayIdentifiers = NVIDIALibrary.GetLibrary().GetCurrentDisplayIdentifiers();
            }

            // Return if it is valid and we should continue
            return IsValid();
        }

        public override void RefreshPossbility()
        {
            // Check whether this profile is possible
            if (NVIDIALibrary.GetLibrary().IsPossibleConfig(_nvidiaDisplayConfig))
            {

                SharedLogger.logger.Debug($"ProfileRepository/IsPossibleRefresh: The NVIDIA profile {Name} is possible!");
                _isPossible = true;

            }
            else
            {
                SharedLogger.logger.Debug($"ProfileRepository/IsPossibleRefresh: The NVIDIA profile {Name} is NOT possible!");
                _isPossible = false;
            }

        }

        public override bool CreateProfileFromCurrentDisplaySettings()
        {

            NVIDIALibrary nvidiaLibrary = NVIDIALibrary.GetLibrary();
            if (nvidiaLibrary.IsInstalled)
            {
                // Create the profile data from the current config
                _nvidiaDisplayConfig = nvidiaLibrary.GetActiveConfig();
                _windowsDisplayConfig= WinLibrary.GetLibrary().GetActiveConfig();

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

            if (_displayConfig.Count > 0)
            {
                foreach (var adapter in _displayConfig.AdapterConfigs)
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
            return this.Equals(obj as NVIDIAProfileItem);
        }

        // Profiles are equal if their Viewports are equal
        public bool Equals(NVIDIAProfileItem other)
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

            // If NVIDIA Display Config is different then return false.
            if (!NVIDIADisplayConfig.Equals(other.NVIDIADisplayConfig))
                return false;

            // If Windows Display Config is different then return false.
            if (!WindowsDisplayConfig.Equals(other.WindowsDisplayConfig))
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

            // TODO: Make this work in NVIDIA land
            foreach (Topology.Path profilePath in Paths)
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
            }


            if (foundPathsCount == foundOtherPathsCount)
                return true;
            else
                return false;
        }

        // If Equals() returns true for this object compared to  another
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {

            // Get hash code for the ProfileDisplayIdentifiers field if it is not null.
            int hashIds = ProfileDisplayIdentifiers == null ? 0 : ProfileDisplayIdentifiers.GetHashCode();

            // Get ProfileData too
            int hashProfileData = Paths == null ? 0 : Paths.GetHashCode();

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
    class NVIDIAProfileComparer : IEqualityComparer<NVIDIAProfileItem>
    {
        // Products are equal if their names and product numbers are equal.
        /*public bool Equals(NVIDIAProfileItem x, NVIDIAProfileItem y)
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

        public bool Equals(NVIDIAProfileItem x, NVIDIAProfileItem y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (x is null || y is null)
                return false;

            if (x.Paths.Length != y.Paths.Length)
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


            // Check whether the profiles' properties are equal
            // We need to exclude the name as the name is solely for saving to disk
            // and displaying to the user. 
            // Two profiles are equal only when they have the same viewport data
            int foundPathsCount = 0;
            int foundOtherPathsCount = 0;

            // TODO: Fix this so it finds compares ProfileData
            foreach (Topology.Path profilePath in x.Paths)
            {
                if (y.Paths.Contains(profilePath))
                {
                    foundPathsCount++;
                    continue;
                }

            }
            foreach (Topology.Path otherPath in y.Paths)
            {
                if (x.Paths.Contains(otherPath))
                {
                    foundOtherPathsCount++;
                    continue;
                }
            }


            if (foundPathsCount == foundOtherPathsCount)
                return true;
            else
                return false;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.
        /*public int GetHashCode(NVIDIAProfileItem profile)
        {

            // Check whether the object is null
            if (profile is null) return 0;

            // Get hash code for the Viewports field if it is not null.
            int hashPaths = profile.Paths == null ? 0 : profile.Paths.GetHashCode();

            //Calculate the hash code for the product.
            return hashPaths;

        }*/
        // Modified the GetHashCode to compare the displayidentifier
        public int GetHashCode(NVIDIAProfileItem profile)
        {

            // Check whether the object is null
            if (profile is null) return 0;

            // Get hash code for the ProfileDisplayIdentifiers field if it is not null.
            int hashIds = profile.ProfileDisplayIdentifiers == null ? 0 : profile.ProfileDisplayIdentifiers.GetHashCode();

            // Get hash code for the Paths
            int hashPaths = profile.Paths == null ? 0 : profile.Paths.GetHashCode();

            //Calculate the hash code for the product.
            return (hashIds, hashPaths).GetHashCode();

        }
    }
}