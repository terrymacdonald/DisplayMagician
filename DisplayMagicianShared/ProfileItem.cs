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
using DisplayMagicianShared.AMD;
using DisplayMagicianShared.NVIDIA;
using DisplayMagicianShared.Windows;

namespace DisplayMagicianShared
{

    public struct ScreenPosition
    {        
        public int ScreenX;
        public int ScreenY;
        public int ScreenWidth;
        public int ScreenHeight;
        public string Name;
        public string Library;
        public bool IsPrimary;
        public Color Colour;
        public string DisplayConnector;
        internal bool HDRSupported;
        internal bool HDREnabled;
        public List<string> Features;
        // If the screen is AMD Eyefinity or NVIDIA Surround or similar, it has screens that are part of it
        // These fields indicate this. The spanned screens are added to the SpannedScreens field as required
        public bool IsSpanned;
        public List<SpannedScreenPosition> SpannedScreens;
        public int SpannedColumns;
        public int SpannedRows;
    }

    public struct SpannedScreenPosition
    {
        public int ScreenX;
        public int ScreenY;
        public int ScreenWidth;
        public int ScreenHeight;
        public string Name;
        public Color Colour;
        public List<string> Features;
        public int Column;
        public int Row;
    }

    public class ProfileItem : IComparable<ProfileItem>, IEquatable<ProfileItem>
    {
        private static List<ProfileItem> _allSavedProfiles = new List<ProfileItem>();
        private ProfileIcon _profileIcon;
        private Bitmap _profileBitmap, _profileShortcutBitmap;
        private List<string> _profileDisplayIdentifiers = new List<string>();
        private List<ScreenPosition> _screens = new List<ScreenPosition>();
        private NVIDIA_DISPLAY_CONFIG _nvidiaDisplayConfig = new NVIDIA_DISPLAY_CONFIG();
        private AMD_DISPLAY_CONFIG _amdDisplayConfig = new AMD_DISPLAY_CONFIG();
        private WINDOWS_DISPLAY_CONFIG _windowsDisplayConfig = new WINDOWS_DISPLAY_CONFIG();

        internal static string AppDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        private static string AppWallpaperPath = Path.Combine(AppDataPath, $"Wallpaper");
        private static readonly string uuidV4Regex = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";

        private string _uuid = "";
        private bool _isPossible = false;
        private Keys _hotkey = Keys.None;
        private string _wallpaperBitmapFilename = "";


        #region JsonConverterBitmap
        internal class CustomBitmapConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            //convert from byte to bitmap (deserialize)

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                string image = (string)reader.Value;

                byte[] byteBuffer = Convert.FromBase64String(image);
                MemoryStream memoryStream = new MemoryStream(byteBuffer)
                {
                    Position = 0
                };

                return (Bitmap)Bitmap.FromStream(memoryStream);
            }

            //convert bitmap to byte (serialize)
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Bitmap bitmap = (Bitmap)value;

                ImageConverter converter = new ImageConverter();
                writer.WriteValue((byte[])converter.ConvertTo(bitmap, typeof(byte[])));
            }

            public static System.Drawing.Imaging.ImageFormat GetImageFormat(Bitmap bitmap)
            {
                ImageFormat img = bitmap.RawFormat;

                if (img.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                    return System.Drawing.Imaging.ImageFormat.Jpeg;
                if (img.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
                    return System.Drawing.Imaging.ImageFormat.Bmp;
                if (img.Equals(System.Drawing.Imaging.ImageFormat.Png))
                    return System.Drawing.Imaging.ImageFormat.Png;
                if (img.Equals(System.Drawing.Imaging.ImageFormat.Emf))
                    return System.Drawing.Imaging.ImageFormat.Emf;
                if (img.Equals(System.Drawing.Imaging.ImageFormat.Exif))
                    return System.Drawing.Imaging.ImageFormat.Exif;
                if (img.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                    return System.Drawing.Imaging.ImageFormat.Gif;
                if (img.Equals(System.Drawing.Imaging.ImageFormat.Icon))
                    return System.Drawing.Imaging.ImageFormat.Icon;
                if (img.Equals(System.Drawing.Imaging.ImageFormat.MemoryBmp))
                    return System.Drawing.Imaging.ImageFormat.MemoryBmp;
                if (img.Equals(System.Drawing.Imaging.ImageFormat.Tiff))
                    return System.Drawing.Imaging.ImageFormat.Tiff;
                else
                    return System.Drawing.Imaging.ImageFormat.Wmf;
            }

        }

        #endregion
        public ProfileItem()
        {
        }

        public static Version Version = new Version(2, 1);

        #region Instance Properties

        public string UUID
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_uuid))
                    _uuid = Guid.NewGuid().ToString("D");
                return _uuid;
            }
            set
            {
                Match match = Regex.Match(value, uuidV4Regex, RegexOptions.IgnoreCase);
                if (match.Success)
                    _uuid = value;
            }
        }

        [JsonIgnore]
        public virtual bool IsPossible
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
        public virtual bool IsActive
        {
            get
            {
                
                if (this.Equals(ProfileRepository.CurrentProfile))
                    return true;
                else
                    return false;

            }
        }

        public virtual VIDEO_MODE VideoMode { get; set; } = VIDEO_MODE.WINDOWS;

        public Keys Hotkey {
            get 
            {
                return _hotkey;
            }
            set 
            {
                _hotkey = value;
            }
        }

        public virtual string Name { get; set; }

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


        [JsonIgnore]
        public virtual ProfileIcon ProfileIcon
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

        [JsonIgnore]
        public virtual List<ScreenPosition> Screens
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

        public string SavedProfileIconCacheFilename { get; set; }

        public Wallpaper.Mode WallpaperMode { get; set; }

        public Wallpaper.Style WallpaperStyle { get; set; }

        public string WallpaperBitmapFilename{ 
            get
            {
                return _wallpaperBitmapFilename;
            }
            set
            {
                _wallpaperBitmapFilename = value;
            }
        }

        public virtual List<string> ProfileDisplayIdentifiers
        {
            get
            {
                if (_profileDisplayIdentifiers.Count == 0)
                {
                    _profileDisplayIdentifiers = ProfileRepository.GetCurrentDisplayIdentifiers(); 
                }
                return _profileDisplayIdentifiers;
            }
            set
            {
                if (value is List<string>)
                    _profileDisplayIdentifiers = value;
            }
        }

        [JsonConverter(typeof(CustomBitmapConverter))]
        public virtual Bitmap ProfileBitmap
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

        [JsonConverter(typeof(CustomBitmapConverter))]
        public virtual Bitmap ProfileTightestBitmap
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

        public static bool IsValidName(string testName)
        {
            foreach (ProfileItem loadedProfile in _allSavedProfiles)
            {
                if (loadedProfile.Name == testName)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidUUID(string testId)
        {
            Match match = Regex.Match(testId, uuidV4Regex, RegexOptions.IgnoreCase);
            if (match.Success)
                return true;
            else
                return false;
        }

        public bool IsValid()
        {
            if (VideoMode == VIDEO_MODE.NVIDIA) 
            {
                if (!NVIDIALibrary.GetLibrary().IsValidConfig(_nvidiaDisplayConfig))
                {
                    SharedLogger.logger.Error($"ProfileRepository/IsValid: The profile {Name} has an invalid NVIDIA display config");
                    return false;
                }
            }
            else if (VideoMode == VIDEO_MODE.AMD)
            {
                if (!AMDLibrary.GetLibrary().IsValidConfig(_amdDisplayConfig))
                {
                    SharedLogger.logger.Error($"ProfileRepository/IsValid: The profile {Name} has an invalid AMD display config");
                    return false;
                }                    
            }
            else if (VideoMode == VIDEO_MODE.WINDOWS)
            {
                if (!WinLibrary.GetLibrary().IsValidConfig(_windowsDisplayConfig))
                {
                    SharedLogger.logger.Error($"ProfileRepository/IsValid: The profile {Name} has an invalid Windows CCD display config");
                    return false;
                }
            }
            else
            {
                SharedLogger.logger.Error($"ProfileRepository/IsValid: The profile {Name} has an unknown video mode!");
            }

            // The rest of the 
            if (ProfileIcon is ProfileIcon &&
                System.IO.File.Exists(SavedProfileIconCacheFilename) &&
                ProfileBitmap is Bitmap &&
                ProfileTightestBitmap is Bitmap &&
                ProfileDisplayIdentifiers.Count > 0)
                return true;
            else
                return false;

        }



        public virtual bool CopyTo(ProfileItem profile, bool overwriteId = true)
        {
            if (overwriteId == true)
                profile.UUID = UUID;

            // Copy all our profile data over to the other profile
            profile.Name = Name;
            profile.AMDDisplayConfig = AMDDisplayConfig;
            profile.NVIDIADisplayConfig = NVIDIADisplayConfig; 
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

        public virtual bool PreSave()
        {
            // Prepare our profile data for saving
            if (_profileDisplayIdentifiers.Count == 0)
            {
                _profileDisplayIdentifiers = ProfileRepository.GetCurrentDisplayIdentifiers();
            }

            // Return if it is valid and we should continue
            return IsValid();
        }


        public bool CreateProfileFromCurrentDisplaySettings()
        {
            if (VideoMode == VIDEO_MODE.NVIDIA)
            {
                NVIDIALibrary nvidiaLibrary = NVIDIALibrary.GetLibrary();
                if (nvidiaLibrary.IsInstalled)
                {
                    // Create the profile data from the current config
                    _nvidiaDisplayConfig = nvidiaLibrary.GetActiveConfig();
                    _windowsDisplayConfig = WinLibrary.GetLibrary().GetActiveConfig();
                    _profileDisplayIdentifiers = nvidiaLibrary.GetCurrentDisplayIdentifiers();

                    // Now, since the ActiveProfile has changed, we need to regenerate screen positions
                    _screens = GetScreenPositions();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(VideoMode == VIDEO_MODE.AMD)
            {
                AMDLibrary amdLibrary = AMDLibrary.GetLibrary();
                if (amdLibrary.IsInstalled)
                {
                    // Create the profile data from the current config
                    _amdDisplayConfig = amdLibrary.GetActiveConfig();
                    _windowsDisplayConfig = WinLibrary.GetLibrary().GetActiveConfig();
                    _profileDisplayIdentifiers = amdLibrary.GetCurrentDisplayIdentifiers();

                    // Now, since the ActiveProfile has changed, we need to regenerate screen positions
                    _screens = GetScreenPositions();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (VideoMode == VIDEO_MODE.WINDOWS)
            {
                WinLibrary winLibrary = WinLibrary.GetLibrary();
                if (winLibrary.IsInstalled)
                {
                    // Create the profile data from the current config
                    _windowsDisplayConfig = winLibrary.GetActiveConfig();
                    _profileDisplayIdentifiers = winLibrary.GetCurrentDisplayIdentifiers();

                    // Now, since the ActiveProfile has changed, we need to regenerate screen positions
                    _screens = GetScreenPositions();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                SharedLogger.logger.Error($"ProfileRepository/CreateProfileFromCurrentDisplaySettings: Tried to use an unknown video mode!");
                return false;
            }
        }

        /*public bool PerformPostLoadingTasks()
        {
            // First thing we do is to set up the Screens
            //_screens = GetScreenPositions();

            return true;
        }*/


        // ReSharper disable once FunctionComplexityOverflow
        // ReSharper disable once CyclomaticComplexity
        public bool CreateShortcut(string shortcutFileName)
        {
            string shortcutDescription = string.Empty;
            string shortcutIconFileName;

            var shortcutArgs = new List<string>
            {
                // Add the SwitchProfile command as the first argument to start to switch to another profile
                $"{DisplayMagicianStartupAction.ChangeProfile}",
                $"\"{UUID}\""
            };

            // Prepare text for the shortcut description field
            shortcutDescription = $"Change to the '{Name}' DisplayMagician Display Profile.";

            // Now we are ready to create a shortcut based on the filename the user gave us
            shortcutFileName = System.IO.Path.ChangeExtension(shortcutFileName, @"lnk");

            // And we use the Icon from the shortcutIconCache
            shortcutIconFileName = SavedProfileIconCacheFilename;

            // If the user supplied a file
            if (shortcutFileName != null)
            {
                try
                {
                    // Remove the old file if it exists to replace it
                    if (System.IO.File.Exists(shortcutFileName))
                    {
                        System.IO.File.Delete(shortcutFileName);
                    }

                    // Actually create the shortcut!
                    //var wshShellType = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
                    //dynamic wshShell = Activator.CreateInstance(wshShellType);


                    WshShell shell = new WshShell();
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutFileName);

                    shortcut.TargetPath = Application.ExecutablePath;
                    shortcut.Arguments = string.Join(" ", shortcutArgs);
                    shortcut.Description = shortcutDescription;
                    shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath) ??
                                                string.Empty;

                    shortcut.IconLocation = shortcutIconFileName;
                    shortcut.Save();
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"ShortcutItem/CreateShortcut: Execption while creating desktop shortcut!");

                    // Clean up a failed attempt
                    if (System.IO.File.Exists(shortcutFileName))
                    {
                        System.IO.File.Delete(shortcutFileName);
                    }
                }
            }

            // Return a status on how it went
            // true if it was a success or false if it was not
            return shortcutFileName != null && System.IO.File.Exists(shortcutFileName);
        }

        public virtual void RefreshPossbility()
        {
            // Check whether this profile is possible
            if (ProfileRepository.CurrentVideoMode == VIDEO_MODE.NVIDIA && NVIDIALibrary.GetLibrary().IsInstalled)
            {
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
            else if (ProfileRepository.CurrentVideoMode == VIDEO_MODE.AMD && AMDLibrary.GetLibrary().IsInstalled)
            {
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
            else if (ProfileRepository.CurrentVideoMode == VIDEO_MODE.WINDOWS && WinLibrary.GetLibrary().IsInstalled)
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
        public bool SetActive()
        {
            if (VideoMode == VIDEO_MODE.NVIDIA)
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
            }
            else if (VideoMode == VIDEO_MODE.AMD)
            {
                AMDLibrary amdLibrary = AMDLibrary.GetLibrary();
                WinLibrary winLibrary = WinLibrary.GetLibrary();
                if (amdLibrary.IsInstalled)
                {
                    if (!amdLibrary.IsActiveConfig(_amdDisplayConfig) && !winLibrary.IsActiveConfig(_windowsDisplayConfig))
                    {
                        if (amdLibrary.IsPossibleConfig(_amdDisplayConfig))
                        {
                            SharedLogger.logger.Trace($"ProfileRepository/SetActive: The AMD display settings within profile {Name} are possible to use right now, so we'll use attempt to use them.");
                            bool itWorkedforNVIDIA = amdLibrary.SetActiveConfig(_amdDisplayConfig);

                            if (itWorkedforNVIDIA)
                            {
                                SharedLogger.logger.Trace($"ProfileRepository/SetActive: The AMD display settings within profile {Name} were successfully applied.");
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
                                SharedLogger.logger.Trace($"ProfileRepository/SetActive: The AMD display settings within profile {Name} were NOT applied correctly.");
                            }

                        }
                        else
                        {
                            SharedLogger.logger.Error($"ProfileRepository/SetActive: ERROR - Cannot apply the AMD display config in profile {Name} as it is not currently possible to use it.");
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Info($"ProfileRepository/SetActive: The display settings in profile {Name} are already installed. No need to install them again. Exiting.");
                    }
                }
            }
            else if (VideoMode == VIDEO_MODE.WINDOWS)
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
                
            }
            return false;
        }


        public List<ScreenPosition> GetScreenPositions()
        {
            if (VideoMode == VIDEO_MODE.NVIDIA)
            {
                return GetNVIDIAScreenPositions();
            }
            else if (VideoMode == VIDEO_MODE.AMD)
            {
                return GetAMDScreenPositions();
            }
            else if (VideoMode == VIDEO_MODE.WINDOWS)
            {
                return GetWindowsScreenPositions();
            }
            return new List<ScreenPosition>();
        }


        private List<ScreenPosition> GetNVIDIAScreenPositions()
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
                        // This is a single screen with an adjoining mosaic screen
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

        private List<ScreenPosition> GetAMDScreenPositions()
        {
            // Set up some colours
            Color primaryScreenColor = Color.FromArgb(0, 174, 241); // represents Primary screen blue
            Color spannedScreenColor = Color.FromArgb(221, 0, 49); // represents AMD Red
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
                        // This is a single screen with an adjoining mosaic screen
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

            return _screens;
        }

        private List<ScreenPosition> GetWindowsScreenPositions()
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


        public int CompareTo(ProfileItem other)
        {

            int result = CompareToValues(other);

            // If comparison based solely on values
            // returns zero, indicating that two instances
            // are equal in those fields they have in common,
            // only then we break the tie by comparing
            // data types of the two instances.
            if (result == 0)
                result = CompareTypes(other);

            return result;

        }

        protected virtual int CompareToValues(ProfileItem other)
        {

            if (object.ReferenceEquals(other, null))
                return 1;   // All instances are greater than null

            // Base class simply compares Mark properties
            return Name.CompareTo(other.Name);

        }

        protected int CompareTypes(ProfileItem other)
        {

            // Base type is considered less than derived type
            // when two instances have the same values of
            // base fields.

            // Instances of two distinct derived types are
            // ordered by comparing full names of their
            // types when base fields are equal.
            // This is consistent comparison rule for all
            // instances of the two derived types.

            int result = 0;

            Type thisType = this.GetType();
            Type otherType = other.GetType();

            if (otherType.IsSubclassOf(thisType))
                result = -1;    // other is subclass of this class
            else if (thisType.IsSubclassOf(otherType))
                result = 1;     // this is subclass of other class
            else if (thisType != otherType)
                result = thisType.FullName.CompareTo(otherType.FullName);
            // cut the tie with a test that returns
            // the same value for all objects

            return result;

        }


        // The object specific Equals
        public bool Equals(ProfileItem other)
        {
            // Check references
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            // Check the object fields
            // ProfileDisplayIdentifiers may be the same but in different order within the array, so we need to handle
            // that fact.
            return NVIDIADisplayConfig.Equals(other.NVIDIADisplayConfig) &&
                //AMDDisplayConfig.Equals(other.AMDDisplayConfig) && 
                WindowsDisplayConfig.Equals(other.WindowsDisplayConfig) &&
                ProfileDisplayIdentifiers.SequenceEqual (other.ProfileDisplayIdentifiers);
        }

        // The public override for the Object.Equals
        public override bool Equals(Object obj)
        {
            // Check references
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            // If different types then can't be true
            if (obj.GetType() == this.GetType()) return false;
            if (!(obj is ProfileItem)) return false;
            // Check the object fields as this must the same object as obj, and we need to test in more detail
            return Equals((ProfileItem) obj);
        }

        // If Equals() returns true for this object compared to  another
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            // Calculate the hash code for the product.
            return (NVIDIADisplayConfig, AMDDisplayConfig, WindowsDisplayConfig, ProfileDisplayIdentifiers).GetHashCode();

        }

        public static bool operator ==(ProfileItem lhs, ProfileItem rhs)
        {
            /*if (object.ReferenceEquals(lhs, rhs))
                return true;

            if (!object.ReferenceEquals(lhs, null) &&
                !object.ReferenceEquals(rhs, null) &&
                lhs.Equals(rhs))
                return true;

            return false;*/
            return Equals(lhs, rhs);
        }

        public static bool operator !=(ProfileItem lhs, ProfileItem rhs)
        {
            return !Equals(lhs, rhs);
        }

        // IMPORTANT - This ProfileItem ToString function is required to make the Profile ImageListView work properly! DO NOT DELETE!
        public override string ToString()
        {
            return (Name ?? Language.UN_TITLED_PROFILE);
        }
    }
}


