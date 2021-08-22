using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DisplayMagicianShared.Resources;
using Newtonsoft.Json;
using System.Drawing;
using DisplayMagicianShared.Windows;
using System.Drawing.Imaging;

namespace DisplayMagicianShared.AMD
{

    public class AMDProfileItem : ProfileItem, IComparable
    {
        private static List<AMDProfileItem> _allSavedProfiles = new List<AMDProfileItem>();
        private ProfileIcon _profileIcon;
        private Bitmap _profileBitmap, _profileShortcutBitmap;
        private List<string> _profileDisplayIdentifiers = new List<string>();
        private List<ScreenPosition> _screens;
        private AMD_DISPLAY_CONFIG _amdDisplayConfig = new AMD_DISPLAY_CONFIG();
        private WINDOWS_DISPLAY_CONFIG _windowsDisplayConfig = new WINDOWS_DISPLAY_CONFIG();
        private static readonly string uuidV4Regex = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";

        private string _uuid = "";
        private bool _isPossible = false;
        private Keys _hotkey = Keys.None;

        public AMDProfileItem()
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

        public override VIDEO_MODE VideoMode { get; } = VIDEO_MODE.AMD;

        public override string Name { get; set; }

        [JsonRequired]
        public AMD_DISPLAY_CONFIG AMDDisplayConfig
        {
            get
            {
                return _amdDisplayConfig;
            }
            set
            {
                _amdDisplayConfig = value;
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


        public override List<string> ProfileDisplayIdentifiers
        {
            get
            {
                if (_profileDisplayIdentifiers.Count == 0)
                {
                    _profileDisplayIdentifiers = AMDLibrary.GetLibrary().GetCurrentDisplayIdentifiers();
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

        /*public override Bitmap ProfileTightestBitmap
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

        }        */

        #endregion

        public override bool IsValid()
        {

            if (AMDLibrary.GetLibrary().IsValidConfig(_amdDisplayConfig) && 
                ProfileIcon is ProfileIcon &&
                System.IO.File.Exists(SavedProfileIconCacheFilename) &&
                ProfileBitmap is Bitmap &&
                ProfileTightestBitmap is Bitmap &&
                ProfileDisplayIdentifiers.Count > 0)
            {
                if (AMDDisplayConfig.AdapterConfigs.Count > 0)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }



        public bool CopyTo(AMDProfileItem profile, bool overwriteId = true)
        {
            if (!(profile is AMDProfileItem))
                return false;

            if (overwriteId == true)
                profile.UUID = UUID;

            // Copy all our profile data over to the other profile
            profile.Name = Name;
            profile.AMDDisplayConfig = AMDDisplayConfig;
            profile.WindowsDisplayConfig = WindowsDisplayConfig;
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
                _profileDisplayIdentifiers = AMDLibrary.GetLibrary().GetCurrentDisplayIdentifiers();
            }

            // Return if it is valid and we should continue
            return IsValid();
        }

        public override void RefreshPossbility()
        {
            // Check whether this profile is possible
            if (AMDLibrary.GetLibrary().IsPossibleConfig(_amdDisplayConfig))
            {

                SharedLogger.logger.Debug($"ProfileRepository/IsPossibleRefresh: The AMD profile {Name} is possible!");
                _isPossible = true;

            }
            else
            {
                SharedLogger.logger.Debug($"ProfileRepository/IsPossibleRefresh: The AMD profile {Name} is NOT possible!");
                _isPossible = false;
            }

        }

        public override bool CreateProfileFromCurrentDisplaySettings()
        {

            AMDLibrary amdLibrary = AMDLibrary.GetLibrary();
            if (amdLibrary.IsInstalled)
            {
                // Create the profile data from the current config
                _amdDisplayConfig = amdLibrary.GetActiveConfig();
                _windowsDisplayConfig = WinLibrary.GetLibrary().GetActiveConfig();

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

            int pathCount = _windowsDisplayConfig.DisplayConfigPaths.Length;
            // First of all we need to figure out how many display paths we have.
            if (pathCount < 1)
            {
                // Return an empty screen if we have no Display Config Paths to use!
                return _screens;
            }

            foreach (var path in _windowsDisplayConfig.DisplayConfigPaths)
            {
                // For each path we go through and get the relevant info we need.
                if (_windowsDisplayConfig.DisplayConfigPaths.Length > 0)
                {
                    // Set some basics about the screen
                    ScreenPosition screen = new ScreenPosition();
                    screen.Library = "NVIDIA";

                    UInt32 targetId = path.TargetInfo.Id;

                    foreach (DISPLAYCONFIG_MODE_INFO displayMode in _windowsDisplayConfig.DisplayConfigModes)
                    {
                        // Find the matching Display Config Source Mode
                        if (displayMode.InfoType != DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE && displayMode.Id == targetId)
                        {
                            screen.Name = targetId.ToString();
                            //screen.DisplayConnector = displayMode.DisplayConnector;
                            screen.ScreenX = displayMode.SourceMode.Position.X;
                            screen.ScreenY = displayMode.SourceMode.Position.Y;
                            screen.ScreenWidth = (int)displayMode.SourceMode.Width;
                            screen.ScreenHeight = (int)displayMode.SourceMode.Height;

                            // If we're at the 0,0 coordinate then we're the primary monitor
                            if (screen.ScreenX == 0 && screen.ScreenY == 0)
                            {
                                screen.IsPrimary = true;
                            }
                        }
                    }

                    foreach (ADVANCED_HDR_INFO_PER_PATH hdrInfo in _windowsDisplayConfig.DisplayHDRStates)
                    {
                        // Find the matching HDR information
                        if (hdrInfo.Id == targetId)
                        {
                            // HDR information
                            if (hdrInfo.AdvancedColorInfo.AdvancedColorSupported)
                            {
                                screen.HDRSupported = true;
                                if (hdrInfo.AdvancedColorInfo.AdvancedColorEnabled)
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

                        }
                    }


                    // Now we need to check for Spanned screens
                    /*if (_amdDisplayConfig)
                    {
                        screen.IsSpanned = true;
                        screen.Colour = Color.FromArgb(221,0,49); // represents AMD Red
                        screen.SpannedName = "AMD Eyefinity";
                    }
                    else
                    {
                        screen.IsSpanned = false;
                        screen.Colour = Color.FromArgb(195, 195, 195); // represents normal screen colour
                    }*/

                    screen.IsSpanned = false;
                    screen.Colour = Color.FromArgb(195, 195, 195); // represents normal screen colour

                    _screens.Add(screen);
                }
            }


            return _screens;
        }


        // The public override for the Object.Equals
        public override bool Equals(object obj)
        {
            return this.Equals(obj as AMDProfileItem);
        }

        // Profiles are equal if their Viewports are equal
        public bool Equals(AMDProfileItem other)
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

            // If AMD Display Config is different then return false.
            if (!AMDDisplayConfig.Equals(other.AMDDisplayConfig))
                return false;

            // If Windows Display Config is different then return false.
            if (!WindowsDisplayConfig.Equals(other.WindowsDisplayConfig))
                return false;

            // If Display Identifiers are different then return false.
            if (!ProfileDisplayIdentifiers.SequenceEqual(other.ProfileDisplayIdentifiers))
                return false;

            // Otherwise if all the tests work, then we're good!
            return true;
        }

        public override int GetHashCode()
        {
            // Calculate the hash code for the product.
            return (AMDDisplayConfig, WindowsDisplayConfig, ProfileDisplayIdentifiers).GetHashCode();

        }

    }
    
}