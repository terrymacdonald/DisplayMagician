using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DisplayMagicianShared.Resources;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;

namespace DisplayMagicianShared.Windows
{    

    public class WinProfileItem : ProfileItem, IEquatable<WinProfileItem>, IComparable
    {
        private static List<WinProfileItem> _allSavedProfiles = new List<WinProfileItem>();
        private ProfileIcon _profileIcon;
        private Bitmap _profileBitmap, _profileShortcutBitmap;
        private List<string> _profileDisplayIdentifiers = new List<string>();
        private List<ScreenPosition> _screens;
        private WINDOWS_DISPLAY_CONFIG _windowsDisplayConfig = new WINDOWS_DISPLAY_CONFIG();
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

        public override VIDEO_MODE VideoMode { get; } = VIDEO_MODE.WINDOWS;

        public override string Name { get; set; }

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

       
        #endregion

        public override bool IsValid()
        {

            if (WinLibrary.GetLibrary().IsValidConfig(_windowsDisplayConfig) &&
                ProfileIcon is ProfileIcon &&
                System.IO.File.Exists(SavedProfileIconCacheFilename) &&
                ProfileBitmap is Bitmap &&
                ProfileTightestBitmap is Bitmap &&
                ProfileDisplayIdentifiers.Count > 0)
                return true;
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
            profile.WindowsDisplayConfig = WindowsDisplayConfig;
            profile.ProfileIcon = ProfileIcon;
            profile.SavedProfileIconCacheFilename = SavedProfileIconCacheFilename;
            profile.ProfileBitmap = ProfileBitmap;
            profile.ProfileTightestBitmap = ProfileTightestBitmap;
            profile.ProfileDisplayIdentifiers = ProfileDisplayIdentifiers;
            profile.WallpaperMode = WallpaperMode;
            profile.WallpaperBitmapFilename = WallpaperBitmapFilename;
            profile.WallpaperStyle = WallpaperStyle;
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
            // Check whether this profile is possible
            if (ProfileRepository.CurrentVideoMode == VIDEO_MODE.WINDOWS && WinLibrary.GetLibrary().IsInstalled)
            {
                if (WinLibrary.GetLibrary().IsPossibleConfig(_windowsDisplayConfig))
                {

                    SharedLogger.logger.Debug($"ProfileRepository/IsPossibleRefresh: The Windows CCD profile {Name} is possible!");
                    _isPossible = true;

                }
                else
                {
                    SharedLogger.logger.Debug($"ProfileRepository/IsPossibleRefresh: The Windows CCD profile {Name} is NOT possible!");
                    _isPossible = false;
                }
            }
            else
            {
                _isPossible = false;
            }
        }


        // Actually set this profile active
        public override bool SetActive()
        {
            WinLibrary winLibrary = WinLibrary.GetLibrary();
            if (winLibrary.IsInstalled)
            {
                if (!winLibrary.IsActiveConfig(_windowsDisplayConfig))
                {
                    if (winLibrary.SetActiveConfig(_windowsDisplayConfig))
                    {
                        SharedLogger.logger.Trace($"ProfileRepository/SetActive: The Windows CCD display settings within profile {Name} were successfully applied.");
                        return true;
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"ProfileRepository/SetActive: The Windows CCD display settings within profile {Name} were NOT applied correctly.");
                    }                    
                }
                else
                {
                    SharedLogger.logger.Info($"ProfileRepository/SetActive: The display settings in profile {Name} are already installed. No need to install them again. Exiting.");
                }
            }
            return false;
        }

        public override bool CreateProfileFromCurrentDisplaySettings()
        {

            WinLibrary winLibrary = WinLibrary.GetLibrary();
            if (winLibrary.IsInstalled)
            {
                // Create the profile data from the current config
                _windowsDisplayConfig = winLibrary.GetActiveConfig();

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
            // Set up some colours
            Color primaryScreenColor = Color.FromArgb(0, 174, 241); // represents Primary screen blue
            Color normalScreenColor = Color.FromArgb(155, 155, 155); // represents normal screen colour (gray)

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
                    screen.Library = "WINDOWS";
                    screen.IsSpanned = false;
                    screen.Colour = normalScreenColor; // this is the default unless overridden by the primary screen

                    UInt32 sourceId = path.SourceInfo.Id;
                    UInt32 targetId = path.TargetInfo.Id;


                    // Go through the screens as Windows knows them, and then enhance the info with Mosaic data if it applies
                    foreach (DISPLAYCONFIG_MODE_INFO displayMode in _windowsDisplayConfig.DisplayConfigModes)
                    {
                        // Find the matching Display Config Source Mode
                        if (displayMode.InfoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE && displayMode.Id == sourceId)
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
                                screen.Colour = primaryScreenColor;
                            }
                            break;
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
                            break;
                        }
                    }

                    _screens.Add(screen);
                }
            }
            
            return _screens;
        }

        /*public int CompareTo(object obj)
        {
            if (!(obj is WinProfileItem)) throw new ArgumentException("Object to CompareTo is not a WinProfileItem"); ;

            WinProfileItem otherProfile = (WinProfileItem)obj;
            return this.Name.CompareTo(otherProfile.Name);
        }*/


        // The public override for the Object.Equals
        public override bool Equals(object obj) => this.Equals(obj as WinProfileItem);

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

            // If Windows Display Config is different then return false.
            if (!WindowsDisplayConfig.Equals(other.WindowsDisplayConfig))
                return false;

            // If Display Identifiers are different then return false.
            if (!ProfileDisplayIdentifiers.SequenceEqual(other.ProfileDisplayIdentifiers))
                return false;

            // Otherwise if all the tests work, then we're good!
            return true;
        }

        // If Equals() returns true for this object compared to  another
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            // Calculate the hash code for the product.
            return (WindowsDisplayConfig, ProfileDisplayIdentifiers).GetHashCode();

        }

        public static bool operator ==(WinProfileItem lhs, WinProfileItem rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(WinProfileItem lhs, WinProfileItem rhs) => !(lhs == rhs);
    }
    
}