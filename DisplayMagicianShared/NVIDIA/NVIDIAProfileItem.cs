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

        public override VIDEO_MODE VideoMode { get; } = VIDEO_MODE.NVIDIA;

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

        #endregion

        public override bool IsValid()
        {

            if (NVIDIALibrary.GetLibrary().IsValidConfig(_nvidiaDisplayConfig) &&
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

        // Actually set this profile active
        public override bool SetActive()
        {
            NVIDIALibrary nvidiaLibrary = NVIDIALibrary.GetLibrary();
            WinLibrary winLibrary = WinLibrary.GetLibrary();
            if (nvidiaLibrary.IsInstalled)
            {
                if (!nvidiaLibrary.IsActiveConfig(_nvidiaDisplayConfig) && !winLibrary.IsActiveConfig(_windowsDisplayConfig))
                {
                    if (nvidiaLibrary.IsPossibleConfig(_nvidiaDisplayConfig))
                    {
                        SharedLogger.logger.Trace($"ProfileRepository/SetActive: The NVIDIA display settings within profile {Name} are possible to use right now, so we'll use attempt to use them.");
                        bool itWorkedforNVIDIA = nvidiaLibrary.SetActiveConfig(_nvidiaDisplayConfig);

                        if (itWorkedforNVIDIA)
                        {
                            SharedLogger.logger.Trace($"ProfileRepository/SetActive: The NVIDIA display settings within profile {Name} were successfully applied.");
                            // Then let's try to also apply the windows changes
                            // Note: we are unable to check if the Windows CCD display config is possible, as it won't match if either the current display config is a Mosaic config,
                            // or if the display config we want to change to is a Mosaic config. So we just have to assume that it will work!
                            bool itWorkedforWindows = winLibrary.SetActiveConfig(_windowsDisplayConfig);
                            if (itWorkedforWindows)
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
                            SharedLogger.logger.Trace($"ProfileRepository/SetActive: The NVIDIA display settings within profile {Name} were NOT applied correctly.");
                        }

                    }
                    else
                    {
                        SharedLogger.logger.Error($"ProfileRepository/SetActive: ERROR - Cannot apply the NVIDIA display config in profile {Name} as it is not currently possible to use it.");
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
            //_screens = GetScreenPositions();

            return true;
        }

        public override List<ScreenPosition> GetScreenPositions()
        {
            // Set up some colours
            Color primaryScreenColor = Color.FromArgb(0, 174, 241); // represents Primary screen blue
            Color spannedScreenColor = Color.FromArgb(118, 185, 0); // represents NVIDIA Green
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
            // Now we need to check for Spanned screens
            if (_nvidiaDisplayConfig.MosaicConfig.IsMosaicEnabled)
            {
                // TODO: Make the NVIDIA displays show the individual screens and overlap!
                


                // Create a dictionary of all the screen sizes we want
                //Dictionary<string,SpannedScreenPosition> MosaicScreens = new Dictionary<string,SpannedScreenPosition>();
                for (int i = 0; i < _nvidiaDisplayConfig.MosaicConfig.MosaicGridCount; i++)
                {
                    ScreenPosition screen = new ScreenPosition();
                    screen.Library = "NVIDIA";
                    screen.Colour = normalScreenColor;
                    if (_nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos[i].DisplayCount > 1)
                    {
                        // Set some basics about the screen                        
                        screen.SpannedScreens = new List<SpannedScreenPosition>();
                        screen.Name = "NVIDIA Surround/Mosaic";
                        screen.IsSpanned = true;
                        screen.SpannedRows = (int)_nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos[i].Rows;
                        screen.SpannedColumns = (int)_nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos[i].Columns;
                        screen.Colour = spannedScreenColor;

                        // This is a combined surround/mosaic screen
                        // We need to build the size of the screen to match it later so we check the MosaicViewports
                        uint minX = 0;
                        uint minY = 0;
                        uint maxX = 0;
                        uint maxY = 0;
                        uint overallX = 0;
                        uint overallY = 0;
                        int overallWidth = 0;
                        int overallHeight = 0;
                        for (int j = 0; j < _nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos[i].DisplayCount; j++)
                        {
                            SpannedScreenPosition spannedScreen = new SpannedScreenPosition();
                            spannedScreen.Name = _nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos[i].Displays[j].DisplayId.ToString();
                            spannedScreen.Colour = spannedScreenColor;

                            // Calculate screen size
                            NV_RECT viewRect = _nvidiaDisplayConfig.MosaicConfig.MosaicViewports[i][j];
                            if (viewRect.Left < minX)
                            {
                                minX = viewRect.Left;
                            }
                            if (viewRect.Top < minY)
                            {
                                minY = viewRect.Top;
                            }
                            if (viewRect.Right > maxX)
                            {
                                maxX = viewRect.Right;
                            }
                            if (viewRect.Bottom > maxY)
                            {
                                maxY = viewRect.Bottom;
                            }
                            uint width = viewRect.Right - viewRect.Left + 1;
                            uint height = viewRect.Bottom - viewRect.Top + 1;
                            spannedScreen.ScreenX = (int)viewRect.Left;
                            spannedScreen.ScreenY = (int)viewRect.Top;
                            spannedScreen.ScreenWidth = (int)width;
                            spannedScreen.ScreenHeight = (int)height;

                            // Figure out the overall figures for the screen
                            if (viewRect.Left < overallX)
                            {
                                overallX = viewRect.Left;
                            }
                            if (viewRect.Top < overallY)
                            {
                                overallY = viewRect.Top;
                            }

                            overallWidth = (int)maxX - (int)minX + 1;
                            overallHeight = (int)maxY - (int)minY + 1;

                            spannedScreen.Row = i + 1;
                            spannedScreen.Column = j + 1;

                            // Add the spanned screen to the screen
                            screen.SpannedScreens.Add(spannedScreen);
                        }

                        //screen.Name = targetId.ToString();
                        //screen.DisplayConnector = displayMode.DisplayConnector;
                        screen.ScreenX = (int)overallX;
                        screen.ScreenY = (int)overallY;
                        screen.ScreenWidth = (int)overallWidth;
                        screen.ScreenHeight = (int)overallHeight;

                        // If we're at the 0,0 coordinate then we're the primary monitor
                        if (screen.ScreenX == 0 && screen.ScreenY == 0)
                        {
                            // Record we're primary screen
                            screen.IsPrimary = true;
                            // Change the colour to be the primary colour, but only if it isn't a surround screen
                            if (screen.Colour != spannedScreenColor)
                            {
                                screen.Colour = primaryScreenColor;
                            }                                
                        }

                    }
                    else if (_nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos[i].DisplayCount == 1)
                    {
                        // This is a single screen
                        // Set some basics about the screen
                        uint displayId = _nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos[i].Displays[0].DisplayId;
                        string windowsDisplayName = _nvidiaDisplayConfig.DisplayNames[displayId];
                        uint sourceIndex = _windowsDisplayConfig.DisplaySources[windowsDisplayName];
                        for (int x = 0; x < _windowsDisplayConfig.DisplayConfigModes.Length; x++)
                        {
                            // Skip this if its not a source info config type
                            if (_windowsDisplayConfig.DisplayConfigModes[x].InfoType != DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE)
                            {
                                continue;
                            }

                            // If the source index matches the index of the source info object we're  looking at, then process it!
                            if (_windowsDisplayConfig.DisplayConfigModes[x].Id == sourceIndex)
                            {
                                screen.Name = displayId.ToString();

                                screen.ScreenX = (int)_windowsDisplayConfig.DisplayConfigModes[x].SourceMode.Position.X;
                                screen.ScreenY = (int)_windowsDisplayConfig.DisplayConfigModes[x].SourceMode.Position.Y;
                                screen.ScreenWidth = (int)_windowsDisplayConfig.DisplayConfigModes[x].SourceMode.Width;
                                screen.ScreenHeight = (int)_windowsDisplayConfig.DisplayConfigModes[x].SourceMode.Height;
                                break;
                            }
                        }
                                                

                        // If we're at the 0,0 coordinate then we're the primary monitor
                        if (screen.ScreenX == 0 && screen.ScreenY == 0)
                        {
                            screen.IsPrimary = true;
                            screen.Colour = primaryScreenColor;
                        }

                    }

                    _screens.Add(screen);
                }
            }
            else
            {
                foreach (var path in _windowsDisplayConfig.DisplayConfigPaths)
                {
                    // For each path we go through and get the relevant info we need.
                    if (_windowsDisplayConfig.DisplayConfigPaths.Length > 0)
                    {
                        // Set some basics about the screen
                        ScreenPosition screen = new ScreenPosition();
                        screen.Library = "NVIDIA";
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
            }

            

            

            /*
            // Go through the screens, and update the Mosaic screens with their info (if there are any)
            if (_nvidiaDisplayConfig.MosaicConfig.IsMosaicEnabled && _nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos.Length)
            {
                // *** Enum values for the mosaic topology type ***
                // NV_MOSAIC_TOPO_1x2_BASIC = 1
                // NV_MOSAIC_TOPO_2x1_BASIC = 2,
                // NV_MOSAIC_TOPO_1x3_BASIC = 3,
                // NV_MOSAIC_TOPO_3x1_BASIC = 4,
                // NV_MOSAIC_TOPO_1x4_BASIC = 5,
                // NV_MOSAIC_TOPO_4x1_BASIC = 6,
                // NV_MOSAIC_TOPO_2x2_BASIC = 7,
                // NV_MOSAIC_TOPO_2x3_BASIC = 8,
                // NV_MOSAIC_TOPO_2x4_BASIC = 9,
                // NV_MOSAIC_TOPO_3x2_BASIC = 10,
                // NV_MOSAIC_TOPO_4x2_BASIC = 11,
                // NV_MOSAIC_TOPO_1x5_BASIC = 12,
                // NV_MOSAIC_TOPO_1x6_BASIC = 13,
                // NV_MOSAIC_TOPO_7x1_BASIC = 14,

                // *** Enum values for the mosaic topology type ***
                // NV_MOSAIC_TOPO_1x2_PASSIVE_STEREO = 23,
                // NV_MOSAIC_TOPO_2x1_PASSIVE_STEREO = 24,
                // NV_MOSAIC_TOPO_1x3_PASSIVE_STEREO = 25,
                // NV_MOSAIC_TOPO_3x1_PASSIVE_STEREO = 26,
                // NV_MOSAIC_TOPO_1x4_PASSIVE_STEREO = 27,
                // NV_MOSAIC_TOPO_4x1_PASSIVE_STEREO = 28,
                // NV_MOSAIC_TOPO_2x2_PASSIVE_STEREO = 29,
                for (int screenIndex = 0; screenIndex < _screens.Count; screenIndex++)
                {
                    // go through each screen, and check if it matches a mosaic screen
                    
                }
            }*/


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
            return (NVIDIADisplayConfig, WindowsDisplayConfig, ProfileDisplayIdentifiers).GetHashCode();

        }

    }

}