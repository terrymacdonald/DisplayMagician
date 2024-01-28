using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
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
using System.Runtime.InteropServices;
using System.ComponentModel;
using NLog.Targets;

namespace DisplayMagicianShared
{

    public enum ScreenRotation
    {
        ROTATE_0,
        ROTATE_90,
        ROTATE_180,
        ROTATE_270,
    }

    public struct ScreenPosition
    {        
        public int ScreenX;
        public int ScreenY;
        public int ScreenWidth;
        public int ScreenHeight;
        public string Name;
        public string AdapterName;
        public string Library;
        public bool IsPrimary;
        public bool IsClone;
        public int ClonedCopies;
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
        public TaskBarLayout.TaskBarEdge TaskBarEdge;
        public ScreenRotation Rotation;

        public override bool Equals(object obj) => obj is ScreenPosition other && this.Equals(other);
        public bool Equals(ScreenPosition other)
        => // AdapterId.Equals(other.AdapterId) && // Removed the AdapterId from the Equals, as it changes after reboot.
           //Id == other.Id && // Removed the ID too, as that changes if the user has a Clone!
           ScreenX.Equals(other.ScreenX) &&
           ScreenY.Equals(other.ScreenY) &&
           ScreenWidth.Equals(other.ScreenWidth) &&
           ScreenHeight.Equals(other.ScreenHeight);
        public override int GetHashCode()
        {
            return (ScreenX, ScreenY, ScreenWidth, ScreenHeight).GetHashCode();
        }

        public static bool operator ==(ScreenPosition lhs, ScreenPosition rhs) => lhs.Equals(rhs);

        public static bool operator !=(ScreenPosition lhs, ScreenPosition rhs) => !(lhs == rhs);
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
        private NVIDIA_DISPLAY_CONFIG _nvidiaDisplayConfig;
        private AMD_DISPLAY_CONFIG _amdDisplayConfig;
        private WINDOWS_DISPLAY_CONFIG _windowsDisplayConfig;

        internal static string AppDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        private static string AppWallpaperPath = Path.Combine(AppDataPath, $"Wallpaper");
        private static readonly string uuidV4Regex = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";

        private string _uuid = "";
        private bool _isPossible = false;
        private bool _isNVIDIAPossible = false;
        private bool _isAMDPossible = false;
        private bool _isWindowsPossible = false;
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
            // Create a default profile Name to avoid null exceptions
            Name = "Current Display Profile";

            // Create default filenames to avoid null exceptions
            SavedProfileIconCacheFilename = "";
            WallpaperBitmapFilename = "";


            // Fill out a new NVIDIA and AMD object when a profile is being created
            // so that it will save correctly. Json.NET will save null references by default
            // unless we fill them up first, and that in turn causes NullReference errors when
            // loading the DisplayProfiles_2.0.json into DisplayMagician next time.
            // We cannot make the structs themselves create the default entry, so instead, we 
            // make each library create the default.
            _nvidiaDisplayConfig = NVIDIALibrary.GetLibrary().CreateDefaultConfig();
            _amdDisplayConfig = AMDLibrary.GetLibrary().CreateDefaultConfig();
            _windowsDisplayConfig = WinLibrary.GetLibrary().CreateDefaultConfig();
        }

        public static Version Version = new Version(2, 1);

        #region Instance Properties

        [DefaultValue("")]

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
            /*set
            {
                _isPossible = value;
            }*/
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

        /*[DefaultValue(VIDEO_MODE.WINDOWS)]

        public virtual VIDEO_MODE VideoMode { get; set; } = VIDEO_MODE.WINDOWS;*/

        [DefaultValue(Keys.None)]
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

        [DefaultValue("")]

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
                if (_screens.Count == 0 && ProfileRepository.ProfilesLoaded)
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

        [DefaultValue("")]
        public string SavedProfileIconCacheFilename { get; set; }

        [DefaultValue(Wallpaper.Mode.DoNothing)]
        public Wallpaper.Mode WallpaperMode { get; set; }

        [DefaultValue(Wallpaper.Style.Fill)]
        public Wallpaper.Style WallpaperStyle { get; set; }

        [DefaultValue("")]
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

        [DefaultValue(default(List<string>))]
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

        [DefaultValue(default(Bitmap))]
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

        [DefaultValue(default(Bitmap))]
        [JsonConverter(typeof(CustomBitmapConverter))]
        public virtual Bitmap ProfileTightestBitmap
        {
            get
            {
                if (ProfileRepository.ProfilesLoaded)
                {
                    if (_profileShortcutBitmap != null)
                        return _profileShortcutBitmap;
                    else
                    {
                        //_profileShortcutBitmap = this.ProfileIcon.ToTightestBitmap();
                        _profileShortcutBitmap = this.ProfileIcon.ToTightestBitmap();
                        return _profileShortcutBitmap;
                    }
                }
                else
                {
                    return null;
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
            NVIDIALibrary nvidiaLibrary = NVIDIALibrary.GetLibrary();
            AMDLibrary amdLibrary = AMDLibrary.GetLibrary();
            WinLibrary winLibrary = WinLibrary.GetLibrary();

            if (nvidiaLibrary.IsInstalled) 
            {
                /*// Check if this is a
                if (VideoMode == VIDEO_MODE.NVIDIA && nvidiaLibrary.IsInstalled && _nvidiaDisplayConfig.PhysicalAdapters.Count > 0 && _nvidiaDisplayConfig.DisplayIdentifiers.Count == 0)
                {
                    if (amdLibrary.IsInstalled && _amdDisplayConfig.AdapterConfigs.Count > 0 && _amdDisplayConfig.DisplayIdentifiers.Count > 0)
                    {
                        SharedLogger.logger.Warn($"ProfileItem/CreateProfileFromCurrentDisplaySettings: This PC is likely an AMD CPU with an integrated GPU. Changing the VideoMode to AMD so DisplayMagician still works.");
                        VideoMode = VIDEO_MODE.AMD;
                    }
                    else if (_windowsDisplayConfig.DisplayIdentifiers.Count > 0)
                    {
                        SharedLogger.logger.Warn($"ProfileItem/CreateProfileFromCurrentDisplaySettings: This PC is likely a CPU with an integrated GPU. Changing the VideoMode to Windows so DisplayMagician still works.");
                        VideoMode = VIDEO_MODE.WINDOWS;

                    }

                }*/
                if (!nvidiaLibrary.IsValidConfig(_nvidiaDisplayConfig))
                {
                    SharedLogger.logger.Error($"ProfileItem/IsValid: The profile {Name} has an invalid NVIDIA display config");
                    return false;
                }
            }
            
            if (amdLibrary.IsInstalled)
            {
                if (!amdLibrary.IsValidConfig(_amdDisplayConfig))
                {
                    SharedLogger.logger.Error($"ProfileItem/IsValid: The profile {Name} has an invalid AMD display config");
                    return false;
                }                    
            }
                        
            if (!winLibrary.IsValidConfig(_windowsDisplayConfig))
            {
                SharedLogger.logger.Error($"ProfileItem/IsValid: The profile {Name} has an invalid Windows CCD display config");
                return false;
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
            // Disabling as this should never happen now
            /*if (_profileDisplayIdentifiers.Count == 0)
            {
                _profileDisplayIdentifiers = ProfileRepository.GetCurrentDisplayIdentifiers();
            }*/

            // Return if it is valid and we should continue
            return IsValid();
        }


        public bool CreateProfileFromCurrentDisplaySettings(bool fastScan = true)
        {
            // Calling the 3 different libraries automatically gets the different configs from each of the 3 video libraries.
            // If the video library isn't in use then it also fills in the defaults so that the JSON file can save properly
            // (C# Structs populate with default values which mean that arrays start with null)

            try
            {
                //await Program.AppBackgroundTaskSemaphoreSlim.WaitAsync(0);
                NVIDIALibrary nvidiaLibrary = NVIDIALibrary.GetLibrary();
                AMDLibrary amdLibrary = AMDLibrary.GetLibrary();
                WinLibrary winLibrary = WinLibrary.GetLibrary();

                // For a library update to the latest version so that we pick up any new changes since the last update
                /*if (VideoMode == VIDEO_MODE.NVIDIA && nvidiaLibrary.IsInstalled)
                {
                    nvidiaLibrary.UpdateActiveConfig();
                    winLibrary.UpdateActiveConfig(fastScan);
                }
                else if (VideoMode == VIDEO_MODE.AMD && amdLibrary.IsInstalled)
                {
                    amdLibrary.UpdateActiveConfig();
                    winLibrary.UpdateActiveConfig(fastScan);
                }
                else
                {
                    winLibrary.UpdateActiveConfig(fastScan);
                }      */
                if (nvidiaLibrary.IsInstalled)
                {
                    nvidiaLibrary.UpdateActiveConfig();
                }
                if (amdLibrary.IsInstalled)
                {
                    amdLibrary.UpdateActiveConfig();
                }

                // Always update Windows display settings
                winLibrary.UpdateActiveConfig(fastScan);
                


                // Grab the profile data from the current stored config (that we just updated)
                _nvidiaDisplayConfig = nvidiaLibrary.ActiveDisplayConfig;
                _amdDisplayConfig = amdLibrary.ActiveDisplayConfig;                
                _windowsDisplayConfig = winLibrary.ActiveDisplayConfig;
                _profileDisplayIdentifiers = ProfileRepository.GetCurrentDisplayIdentifiers();
                

                /*if (nvidiaLibrary.IsInstalled)
                {
                    // Set the default Video Mode to NVIDIA
                    SharedLogger.logger.Debug($"ProfileItem/CreateProfileFromCurrentDisplaySettings: This PC is likely a CPU with an NVIDIA GPU. Changing the VideoMode to NVIDIA to reflect this.");
                    VideoMode = VIDEO_MODE.NVIDIA;

                    // Check if there are situations we want to change to another video mode
                    if (_nvidiaDisplayConfig.PhysicalAdapters.Count > 0 && _nvidiaDisplayConfig.DisplayIdentifiers.Count == 0)
                    {
                        SharedLogger.logger.Info($"ProfileItem/CreateProfileFromCurrentDisplaySettings: The NVIDIA config has a physical adapter but no display identifiers in NVIDIA mode, yet we should have at least one screen on the screen. The PC may be a laptop, with another GPU within the CPU, in which case the screen is being driven by that card instead. Ignore this message in that case.");
                        // Try to handle if the PC is a laptop with an AMD, by changing the mode for this display from NVIDIA to something else that suits more
                        // Try AMD first in case we have an AMD chipset in the laptop
                        if (amdLibrary.IsInstalled && _amdDisplayConfig.AdapterConfigs.Count > 0 && _amdDisplayConfig.DisplayIdentifiers.Count > 0)
                        {
                            SharedLogger.logger.Info($"ProfileItem/CreateProfileFromCurrentDisplaySettings: This PC is likely an AMD CPU with an integrated GPU. Changing the VideoMode to AMD so DisplayMagician still works.");
                            VideoMode = VIDEO_MODE.AMD;
                        }
                        else if (_windowsDisplayConfig.DisplayIdentifiers.Count > 0)
                        {
                            SharedLogger.logger.Info($"ProfileItem/CreateProfileFromCurrentDisplaySettings: This PC is likely a CPU with an integrated GPU. Changing the VideoMode to Windows so DisplayMagician still works.");
                            VideoMode = VIDEO_MODE.WINDOWS;

                        }

                    }
                }
                
                if (amdLibrary.IsInstalled)
                {
                    // Set the default Video Mode to AMD
                    SharedLogger.logger.Debug($"ProfileItem/CreateProfileFromCurrentDisplaySettings: This PC is likely a CPU with an AMD GPU. Changing the VideoMode to AMD to reflect this.");
                    VideoMode = VIDEO_MODE.AMD;

                    if (amdLibrary.IsInstalled && _amdDisplayConfig.DisplayIdentifiers.Count == 0)
                    {
                        SharedLogger.logger.Info($"ProfileItem/CreateProfileFromCurrentDisplaySettings: This PC is likely an AMD CPU with an integrated GPU but no detected screens. Changing the VideoMode to Windows so DisplayMagician still works.");
                        VideoMode = VIDEO_MODE.WINDOWS;
                    }
                }
                else // Else it's windows
                {
                    // Set the default Video Mode to Windows
                    SharedLogger.logger.Info($"ProfileItem/CreateProfileFromCurrentDisplaySettings: This PC is likely a CPU with GPU that Windows can detect. Changing the VideoMode to AMD to reflect this.");
                    VideoMode = VIDEO_MODE.WINDOWS;

                    if (_windowsDisplayConfig.DisplayIdentifiers.Count == 0)
                    {
                        SharedLogger.logger.Warn($"ProfileItem/CreateProfileFromCurrentDisplaySettings: The Windows config has no display identifiers in Windows mode, yet we should have at least one screen. The PC may be running headless, in which case ignore this message.");
                    }
                }*/

                // Now, since the ActiveProfile has changed, we need to regenerate screen positions
                _screens = GetScreenPositions();

                // We also need to update the ProfileIcon so that all the icons and image lists are updated
                _profileIcon = new ProfileIcon(this);
                // And then update the bitmaps
                _profileBitmap = this.ProfileIcon.ToBitmap(256, 256);
                _profileShortcutBitmap = this.ProfileIcon.ToTightestBitmap();

                return true;
                
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"ProfileItem/CreateProfileFromCurrentDisplaySettings: Exception getting the config settings!");
                return false;
            }
        }


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
                    IWshShortcut shortcut = shell.CreateShortcut(shortcutFileName) as IWshShortcut;

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
                    SharedLogger.logger.Warn(ex, $"ProfileItem/CreateShortcut: Execption while creating desktop shortcut!");

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
            // Set isPossible to true unless we find it can't be done.
            _isPossible = true;

            // Now go through each item and check if this is in there
            foreach (string identifier in _profileDisplayIdentifiers)
            {

                if (!ProfileRepository.ConnectedDisplayIdentifiers.Contains(identifier))
                {
                    _isPossible =  false;
                    break;
                }
            }

        }

        // Actually set this profile active
        public bool SetActive()
        {
            NVIDIALibrary nvidiaLibrary = NVIDIALibrary.GetLibrary();
            AMDLibrary amdLibrary = AMDLibrary.GetLibrary();
            WinLibrary winLibrary = WinLibrary.GetLibrary();

            bool nvidiaMainConfigTried = false;
            bool nvidiaMainConfigApplied = false;
            bool amdMainConfigTried = false; 
            bool amdMainConfigApplied = false;
            bool winMainConfigApplied = false;
            bool nvidiaOverrideConfigApplied = false;
            bool amdOverrideConfigApplied = false;


            if (nvidiaLibrary.IsInstalled)
            {
                // Check if the NVIDIA settings are already in use, if so skip applying them
                if (nvidiaLibrary.IsActiveConfig(_nvidiaDisplayConfig))
                {
                    SharedLogger.logger.Info($"ProfileItem/SetActive: The NVIDIA display settings in profile {Name} are already installed. No need to install them again. Skipping.");
                }
                else
                {
                    if (nvidiaLibrary.IsPossibleConfig(_nvidiaDisplayConfig))
                    {

                        SharedLogger.logger.Trace($"ProfileItem/SetActive: The NVIDIA display settings within profile {Name} are possible to use right now, so we'll use attempt to use them.");
                        nvidiaMainConfigTried = true;
                        nvidiaMainConfigApplied = nvidiaLibrary.SetActiveConfig(_nvidiaDisplayConfig);

                        SharedLogger.logger.Trace($"ProfileItem/SetActive: Waiting 0.5 seconds to let the NVIDIA display change take place before continuing.");
                        System.Threading.Thread.Sleep(500);

                        // Lets update the screens so Windows knows whats happening
                        // NVIDIA makes such large changes to the available screens in windows, we need to do this.
                        winLibrary.UpdateActiveConfig();
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"ProfileItem/SetActive: Cannot apply the NVIDIA display config in profile {Name} as it is not currently possible to use it. Exiting");
                        return false;
                    }
                }
            }

            if (amdLibrary.IsInstalled)
            {
                // Check if the AMD settings are already in use, if so skip applying them
                if (amdLibrary.IsActiveConfig(_amdDisplayConfig))
                {
                    SharedLogger.logger.Info($"ProfileItem/SetActive: The AMD display settings in profile {Name} are already installed. No need to install them again. Skipping.");
                }
                else
                {
                    if (amdLibrary.IsPossibleConfig(_amdDisplayConfig))
                    {

                        SharedLogger.logger.Trace($"ProfileItem/SetActive: The AMD display settings within profile {Name} are possible to use right now, so we'll use attempt to use them.");
                        amdMainConfigTried = true;
                        amdMainConfigApplied = amdLibrary.SetActiveConfig(_amdDisplayConfig);

                        SharedLogger.logger.Trace($"ProfileItem/SetActive: Waiting 0.5 seconds to let the AMD display change take place before continuing.");
                        System.Threading.Thread.Sleep(500);

                        // Lets update the screens so Windows knows whats happening
                        // AMD makes such large changes to the available screens in windows, we need to do this.
                        winLibrary.UpdateActiveConfig();
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"ProfileItem/SetActive: Cannot apply the AMD display config in profile {Name} as it is not currently possible to use it.");
                        return false;
                    }
                }
            }

            // Then let's try to also apply the windows changes
            // Note: we are unable to check if the Windows CCD display config is possible, as it won't match if either the current display config is a Mosaic config,
            // or if the display config we want to change to is a Mosaic config. So we just have to assume that it will work
            winMainConfigApplied = winLibrary.SetActiveConfig(_windowsDisplayConfig);
            if (winMainConfigApplied)
            {
                SharedLogger.logger.Trace($"ProfileItem/SetActive: The Windows CCD display settings within the profile {Name} were successfully applied.");
            }
            else
            {
                SharedLogger.logger.Warn($"ProfileItem/SetActive: The Windows CCD display settings within the profile {Name} were NOT applied correctly.");
            }

            // Now apply the NVIDIA config override if the NVIDIA config was tried and successfully changed.
            if (nvidiaMainConfigTried && nvidiaMainConfigApplied && winMainConfigApplied)
            {
                nvidiaOverrideConfigApplied = nvidiaLibrary.SetActiveConfigOverride(_nvidiaDisplayConfig);

                if (nvidiaOverrideConfigApplied)
                {
                    SharedLogger.logger.Trace($"ProfileItem/SetActive: The NVIDIA display settings that override windows within the profile {Name} were successfully applied.");
                }
                else
                {
                    SharedLogger.logger.Warn($"ProfileItem/SetActive: The NVIDIA display settings that override windows within the profile {Name} were NOT applied correctly.");
                }
            }
            else
            {
                SharedLogger.logger.Trace($"ProfileItem/SetActive: Skipping applying the NVIDIA override config within profile {Name} as either NVIDIA or Windows CCD had an earlier error.");
            }


            // Now apply the AMD config override if the AMD config was tried and successfully changed.
            if (amdMainConfigTried && amdMainConfigApplied && winMainConfigApplied)
            {
                amdOverrideConfigApplied = amdLibrary.SetActiveConfigOverride(_amdDisplayConfig);

                if (amdOverrideConfigApplied)
                {
                    SharedLogger.logger.Trace($"ProfileItem/SetActive: The AMD display settings that override windows within the profile {Name} were successfully applied.");
                }
                else
                {
                    SharedLogger.logger.Warn($"ProfileItem/SetActive: The AMD display settings that override windows within the profile {Name} were NOT applied correctly.");
                }
            }
            else
            {
                SharedLogger.logger.Trace($"ProfileItem/SetActive: Skipping applying the AMD override config within profile {Name} as either NVIDIA or Windows CCD had an earlier error.");
            }

            // Lets update the screen config again for the final time.
            nvidiaLibrary.UpdateActiveConfig();
            amdLibrary.UpdateActiveConfig();
            winLibrary.UpdateActiveConfig();

            return true;
        }

        public List<ScreenPosition> GetScreenPositions()
        {
            /*if (VideoMode == VIDEO_MODE.NVIDIA)
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

            return new List<ScreenPosition>();*/

            List<ScreenPosition> allScreens = new List<ScreenPosition>() { };

            List<ScreenPosition> nvidiaScreens = new List<ScreenPosition>() { };

            if (NVIDIALibrary.GetLibrary().IsInstalled)
            {
                nvidiaScreens.AddRange(GetNVIDIAScreenPositions());

                // Ignore any windows screens that already exist from AMD and NVIDIA
                // IMPORTANT: This logic depends on allScreens only containing NVIDIA and AMD screens, and also that AMD and NVIDIA don't each add the same screen
                // If you change any code above this, then you need to make suyre this is still true!
                foreach (var screen in nvidiaScreens)
                {
                    if (!allScreens.Contains(screen))
                    {
                        allScreens.Add(screen);
                    }
                }
            }

            List<ScreenPosition> amdScreens = new List<ScreenPosition>() { };

            if (AMDLibrary.GetLibrary().IsInstalled)
            {
                amdScreens.AddRange(GetAMDScreenPositions());

                // Ignore any windows screens that already exist from AMD and NVIDIA
                // IMPORTANT: This logic depends on allScreens only containing NVIDIA and AMD screens, and also that AMD and NVIDIA don't each add the same screen
                // If you change any code above this, then you need to make suyre this is still true!
                foreach (var screen in amdScreens)
                {
                    if (!allScreens.Contains(screen))
                    {
                        allScreens.Add(screen);
                    }
                }
                
            }

            List<ScreenPosition> winScreens = new List<ScreenPosition>() { };
            winScreens.AddRange(GetWindowsScreenPositions());            

            // Ignore any windows screens that already exist from AMD and NVIDIA
            // IMPORTANT: This logic depends on allScreens only containing NVIDIA and AMD screens, and also that AMD and NVIDIA don't each add the same screen
            // If you change any code above this, then you need to make suyre this is still true!
            foreach (var screen in winScreens)
            {
                if (!allScreens.Contains(screen))
                {
                    allScreens.Add(screen);
                }
            }

            GetTaskbarLocationsForNonWindowsScreens(ref allScreens);

            return allScreens;
        }        

        private List<ScreenPosition> GetNVIDIAScreenPositions()
        {
            // Set up some colours
            Color primaryScreenColor = Color.FromArgb(0, 174, 241); // represents Primary screen blue
            Color spannedScreenColor = Color.FromArgb(118, 185, 0); // represents NVIDIA Green
            Color normalScreenColor = Color.FromArgb(155, 155, 155); // represents normal screen colour (gray)

            // Now we create the screens structure from the AMD profile information
            _screens = new List<ScreenPosition>() { };

            int pathCount = _windowsDisplayConfig.DisplayConfigPaths.Length;
            // First of all we need to figure out how many display paths we have.
            if (pathCount < 1)
            {
                // Return an empty screen if we have no Display Config Paths to use!
                return _screens;
            }

            // Now we need to check for Spanned screens (Surround)
            if (_nvidiaDisplayConfig.MosaicConfig.IsMosaicEnabled && _nvidiaDisplayConfig.MosaicConfig.MosaicGridCount > 0)
            {
                for (int i = 0; i < _nvidiaDisplayConfig.MosaicConfig.MosaicGridCount; i++)
                {

                    ScreenPosition screen = new ScreenPosition();
                    screen.Library = "NVIDIA";
                    if (_nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos[i].DisplayCount > 1)
                    {
                        // It's a spanned screen!
                        // Set some basics about the screen                        
                        screen.SpannedScreens = new List<SpannedScreenPosition>() { };
                        screen.Name = "NVIDIA Surround/Mosaic";
                        screen.IsSpanned = true;
                        screen.SpannedRows = (int)_nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos[i].Rows;
                        screen.SpannedColumns = (int)_nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos[i].Columns;
                        screen.Colour = spannedScreenColor;
                        screen.Rotation = ScreenRotation.ROTATE_0;

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

                        // Need to look for the Windows layout details now we know the size of this display
                        // Set some basics about the screen
                        try
                        {
                            UInt32 displayId = _nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos[i].Displays[0].DisplayId;
                            List<NV_DISPLAYCONFIG_PATH_INFO_V2> displaySources = _nvidiaDisplayConfig.DisplayConfigs;
                            bool breakOuterLoop = false;
                            foreach (var displaySource in displaySources)
                            {
                                foreach (NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2 targetInfo in displaySource.TargetInfo)
                                {
                                    if (targetInfo.DisplayId == displayId)
                                    {
                                        screen.Name = displayId.ToString();
                                        screen.ScreenX = displaySource.SourceModeInfo.Position.X;
                                        screen.ScreenY = displaySource.SourceModeInfo.Position.Y;
                                        screen.ScreenWidth = (int)displaySource.SourceModeInfo.Resolution.Width;
                                        screen.ScreenHeight = (int)displaySource.SourceModeInfo.Resolution.Height;
                                        breakOuterLoop = true;
                                        break;
                                    }
                                }

                                if (breakOuterLoop)
                                {
                                    break;
                                }
                            }
                        }
                        catch (KeyNotFoundException ex)
                        {
                            // Thrown if the Windows display doesn't match the NVIDIA display.
                            // Typically happens during configuration of a new Mosaic mode.
                            // If we hit this issue, then we just want to skip over it, as we can update it later when the user pushes the button.
                            // This only happens due to the auto detection stuff functionality we have built in to try and update as quickly as we can.
                            // So its something that we can safely ignore if we hit this exception as it is part of the expect behaviour
                            SharedLogger.logger.Trace(ex, $"ProfileItem/GetNVIDIAScreenPositions: The windows screen doesn't match the NVIDIA screen. This can happen during a transition to Mosaic.");
                            continue;
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Trace(ex, $"ProfileItem/GetNVIDIAScreenPositions: Exception ocurred whilst looking for the Windows layout details now we know the size of this display.");
                            // Some other exception has occurred and we need to report it.
                            //screen.Name = targetId.ToString();
                            //screen.DisplayConnector = displayMode.DisplayConnector;
                            screen.ScreenX = (int)overallX;
                            screen.ScreenY = (int)overallY;
                            screen.ScreenWidth = (int)overallWidth;
                            screen.ScreenHeight = (int)overallHeight;
                            screen.Rotation = ScreenRotation.ROTATE_0;
                        }

                    }
                    else
                    {
                        // It's a standalone screen
                        screen.SpannedScreens = new List<SpannedScreenPosition>() { };
                        screen.Name = _nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos[i].Displays[0].DisplayId.ToString();
                        screen.IsSpanned = false;
                        screen.SpannedRows = 1;
                        screen.SpannedColumns = 1;
                        screen.Colour = normalScreenColor;

                        try
                        {
                            UInt32 displayId = _nvidiaDisplayConfig.MosaicConfig.MosaicGridTopos[i].Displays[0].DisplayId;
                            List<NV_DISPLAYCONFIG_PATH_INFO_V2> displaySources = _nvidiaDisplayConfig.DisplayConfigs;
                            bool breakOuterLoop = false;
                            foreach (var displaySource in displaySources)
                            {
                                foreach (NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2 targetInfo in displaySource.TargetInfo)
                                {
                                    if (targetInfo.DisplayId == displayId)
                                    {
                                        screen.Name = displayId.ToString();
                                        screen.ScreenX = displaySource.SourceModeInfo.Position.X;
                                        screen.ScreenY = displaySource.SourceModeInfo.Position.Y;
                                        //screen.ScreenWidth = (int)displaySource.SourceModeInfo.Resolution.Width;
                                        //screen.ScreenHeight = (int)displaySource.SourceModeInfo.Resolution.Height;
                                        if (targetInfo.Details.Rotation == NV_ROTATE.ROTATE_0)
                                        {
                                            screen.ScreenWidth = (int)displaySource.SourceModeInfo.Resolution.Width;
                                            screen.ScreenHeight = (int)displaySource.SourceModeInfo.Resolution.Height;
                                            screen.Rotation = ScreenRotation.ROTATE_0;
                                        }
                                        else if (targetInfo.Details.Rotation == NV_ROTATE.ROTATE_90)
                                        {
                                            screen.ScreenWidth = (int)displaySource.SourceModeInfo.Resolution.Height;
                                            screen.ScreenHeight = (int)displaySource.SourceModeInfo.Resolution.Width;
                                            screen.Rotation = ScreenRotation.ROTATE_90;
                                        }
                                        else if (targetInfo.Details.Rotation == NV_ROTATE.ROTATE_180)
                                        {
                                            screen.ScreenWidth = (int)displaySource.SourceModeInfo.Resolution.Width;
                                            screen.ScreenHeight = (int)displaySource.SourceModeInfo.Resolution.Height;
                                            screen.Rotation = ScreenRotation.ROTATE_180;
                                        }
                                        else if (targetInfo.Details.Rotation == NV_ROTATE.ROTATE_270)
                                        {
                                            screen.ScreenWidth = (int)displaySource.SourceModeInfo.Resolution.Height;
                                            screen.ScreenHeight = (int)displaySource.SourceModeInfo.Resolution.Width;
                                            screen.Rotation = ScreenRotation.ROTATE_270;
                                        }
                                        else
                                        {
                                            screen.ScreenWidth = (int)displaySource.SourceModeInfo.Resolution.Width;
                                            screen.ScreenHeight = (int)displaySource.SourceModeInfo.Resolution.Height;
                                            screen.Rotation = ScreenRotation.ROTATE_0;
                                        }
                                        breakOuterLoop = true;
                                        break;
                                    }
                                }

                                if (breakOuterLoop)
                                {
                                    break;
                                }
                            }
                        }
                        catch (KeyNotFoundException ex)
                        {
                            // Thrown if the Windows display doesn't match the NVIDIA display.
                            // Typically happens during configuration of a new Mosaic mode.
                            // If we hit this issue, then we just want to skip over it, as we can update it later when the user pushes the button.
                            // This only happens due to the auto detection stuff functionality we have built in to try and update as quickly as we can.
                            // So its something that we can safely ignore if we hit this exception as it is part of the expect behaviour
                            SharedLogger.logger.Trace(ex, $"ProfileItem/GetNVIDIAScreenPositions: Exception thrown as the Windows display doesn't match the NVIDIA display. This is expected behaviour and can be safely ignored.");
                            continue;
                        }
                        catch (Exception ex)
                        {
                            // Some other exception has occurred and we need to report it.
                            SharedLogger.logger.Error(ex, $"ProfileItem/GetNVIDIAScreenPositions: Unable to get the non-mosaic screen size for a secondary screen to a surround screen.");
                        }

                    }

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

                    // Force the taskbar edge to the bottom as it is an NVIDIA surround screen
                    screen.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;

                    SharedLogger.logger.Trace($"ProfileItem/GetNVIDIAScreenPositions: Added a new NVIDIA Spanned Screen {screen.Name} ({screen.ScreenWidth}x{screen.ScreenHeight}) at position {screen.ScreenX},{screen.ScreenY}.");

                    _screens.Add(screen);
                
                }
            }
            else
            {
                // If mosaic isn't enabled then we use the NVIDIA DisplayConfig structure to find the details
                try
                {
                    SharedLogger.logger.Trace($"ProfileItem/GetNVIDIAScreenPositions: Mosaic isn't enabled so using the DisplayConfig based screen details.");
                    List<NV_DISPLAYCONFIG_PATH_INFO_V2> displaySources = _nvidiaDisplayConfig.DisplayConfigs;
                    foreach (var displaySource in displaySources)
                    {
                        int targetInfoIndex = 0;
                        SharedLogger.logger.Trace($"ProfileItem/GetNVIDIAScreenPositions: Processing screen source index #{targetInfoIndex}.");

                        foreach (NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2 targetInfo in displaySource.TargetInfo)
                        {
                            SharedLogger.logger.Trace($"ProfileItem/GetNVIDIAScreenPositions: Processing target screen ID:{targetInfo.DisplayId}.");

                            ScreenPosition screen = new ScreenPosition();
                            screen.Library = "NVIDIA";

                            // Find out if we're a cloned screen
                            if (_nvidiaDisplayConfig.IsCloned && displaySource.TargetInfoCount > 1)
                            {
                                if (targetInfoIndex == 0)
                                {
                                    // Show that this window has clones, and show how many there are.
                                    SharedLogger.logger.Trace($"ProfileItem/GetNVIDIAScreenPositions: The screen ID:{targetInfo.DisplayId} is the source of a cloned group.");
                                    screen.IsClone = true;
                                    screen.ClonedCopies = (int)displaySource.TargetInfoCount;
                                }
                                else
                                {
                                    // Skip getting layout details from the clones themselves, as we have no idea where they are!
                                    SharedLogger.logger.Trace($"ProfileItem/GetNVIDIAScreenPositions: The screen ID:{targetInfo.DisplayId} is part of a cloned group (but we don'tt need to show it so skipping).");
                                    continue;
                                }

                            }
                            else 
                            {
                                SharedLogger.logger.Trace($"ProfileItem/GetNVIDIAScreenPositions: The screen ID:{targetInfo.DisplayId} is NOT part of a cloned group.");
                            }

                            // It's a normal screen
                            screen.SpannedScreens = new List<SpannedScreenPosition>() { };
                            screen.Name = targetInfo.DisplayId.ToString();
                            screen.IsSpanned = false;
                            screen.SpannedRows = 1;
                            screen.SpannedColumns = 1;
                            screen.Colour = normalScreenColor;
                            screen.ScreenX = displaySource.SourceModeInfo.Position.X;
                            screen.ScreenY = displaySource.SourceModeInfo.Position.Y;
                            //screen.ScreenWidth = (int)displaySource.SourceModeInfo.Resolution.Width;
                            //screen.ScreenHeight = (int)displaySource.SourceModeInfo.Resolution.Height;
                            if (screen.ScreenWidth == 0)
                            {
                                SharedLogger.logger.Error($"ProfileItem/GetNVIDIAScreenPositions: The screen width is 0 and it shouldn't be! Skipping this display id #{targetInfo.DisplayId.ToString()}.");
                            }
                            if (screen.ScreenHeight == 0)
                            {
                                SharedLogger.logger.Error($"ProfileItem/GetNVIDIAScreenPositions: The screen height is 0 and it shouldn't be! Skipping this display id #{targetInfo.DisplayId.ToString()}.");
                            }

                            if (targetInfo.Details.Rotation == NV_ROTATE.ROTATE_0)
                            {
                                screen.ScreenWidth = (int)displaySource.SourceModeInfo.Resolution.Width;
                                screen.ScreenHeight = (int)displaySource.SourceModeInfo.Resolution.Height; 
                                screen.Rotation = ScreenRotation.ROTATE_0;
                            }
                            else if (targetInfo.Details.Rotation == NV_ROTATE.ROTATE_90 )
                            {
                                screen.ScreenWidth = (int)displaySource.SourceModeInfo.Resolution.Height;
                                screen.ScreenHeight = (int)displaySource.SourceModeInfo.Resolution.Width;
                                screen.Rotation = ScreenRotation.ROTATE_90;
                            }
                            else if (targetInfo.Details.Rotation == NV_ROTATE.ROTATE_180)
                            {
                                screen.ScreenWidth = (int)displaySource.SourceModeInfo.Resolution.Width;
                                screen.ScreenHeight = (int)displaySource.SourceModeInfo.Resolution.Height; 
                                screen.Rotation = ScreenRotation.ROTATE_180;
                            }
                            else if (targetInfo.Details.Rotation == NV_ROTATE.ROTATE_270)
                            {
                                screen.ScreenWidth = (int)displaySource.SourceModeInfo.Resolution.Height;
                                screen.ScreenHeight = (int)displaySource.SourceModeInfo.Resolution.Width;
                                screen.Rotation = ScreenRotation.ROTATE_270;
                            }
                            else
                            {
                                screen.ScreenWidth = (int)displaySource.SourceModeInfo.Resolution.Width;
                                screen.ScreenHeight = (int)displaySource.SourceModeInfo.Resolution.Height;
                                screen.Rotation = ScreenRotation.ROTATE_0;
                            }

                            // If we're at the 0,0 coordinate then we're the primary monitor
                            if (screen.ScreenX == 0 && screen.ScreenY == 0)
                            {
                                SharedLogger.logger.Trace($"ProfileItem/GetNVIDIAScreenPositions: NVIDIA Screen {screen.Name} is the primary monitor.");
                                // Record we're primary screen
                                screen.IsPrimary = true;
                                // Change the colour to be the primary colour, but only if it isn't a surround screen
                                if (screen.Colour != spannedScreenColor)
                                {
                                    screen.Colour = primaryScreenColor;
                                }
                            }
                            
                            try
                            {
                                if (_nvidiaDisplayConfig.DisplayNames.ContainsKey(targetInfo.DisplayId.ToString()))
                                {
                                    string windowsDisplayName = _nvidiaDisplayConfig.DisplayNames[targetInfo.DisplayId.ToString()];
                                    UInt32 windowsUID = _windowsDisplayConfig.DisplaySources[windowsDisplayName].First().TargetId;
                                    // IMPORTANT: This lookup WILL DEFINITELY CAUSE AN EXCEPTION right after windows changes back from 
                                    // NVIDIA Surround to a non-surround profile. This is expected, as it is caused bythe way Windows is SOOOO slow to update
                                    // the taskbar locations in memory (it takes up to 15 seconds!). NOthing I can do, except put this protection in place :( .

                                    if (_windowsDisplayConfig.TaskBarLayout.Count > 0)
                                    {
                                        foreach (var taskBar in _windowsDisplayConfig.TaskBarLayout)
                                        {
                                            var taskBarValue = taskBar.Value;
                                            if (taskBarValue is TaskBarLayout && taskBarValue.RegKeyValue != null && taskBarValue.RegKeyValue.Contains($"UID{windowsUID}"))
                                            {
                                                screen.TaskBarEdge = taskBarValue.Edge;
                                                break;
                                            }
                                        }
                                    }
                                    SharedLogger.logger.Trace($"ProfileItem/GetNVIDIAScreenPositions: Position of the taskbar on display {targetInfo.DisplayId} is on the {screen.TaskBarEdge } of the screen.");
                                }
                                else
                                {
                                    SharedLogger.logger.Warn($"ProfileItem/GetNVIDIAScreenPositions: Couldn't get the position of the taskbar on display {targetInfo.DisplayId} so assuming its at the bottom.");
                                    screen.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;
                                }
                            }
                            catch (Exception ex)
                            {
                                // Guess that it is at the bottom (90% correct)
                                SharedLogger.logger.Warn(ex, $"ProfileItem/GetNVIDIAScreenPositions: Exception trying to get the position of the taskbar on display {targetInfo.DisplayId}");
                                screen.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;
                            }

                            SharedLogger.logger.Trace($"ProfileItem/GetNVIDIAScreenPositions: (2) Added a non-surround NVIDIA Screen {screen.Name} ({screen.ScreenWidth}x{screen.ScreenHeight}) at position {screen.ScreenX},{screen.ScreenY}.");

                            _screens.Add(screen);
                            targetInfoIndex++;
                        }

                    }
                }
                catch (Exception ex)
                {
                    // Some other exception has occurred and we need to report it.
                    SharedLogger.logger.Error(ex, $"ProfileItem/GetNVIDIAScreenPositions: Exception while trying to get the screen details. (#2) Mosaic isn't enabled, but unable to get the screen details. ");
                }

            }

            /*// Now we also need to try and find if there are any other displays connected that don't use an NVIDIA card
            try
            {
                // Get the list of Windows screens
                List<ScreenPosition> windowsScreens = FindAllWindowsScreens();

                // Now look through the list of Windows Screens to see if there are any we haven't already go from NVIDIA.
                // If there are new ones, then add them to the list we're returning
                foreach (ScreenPosition windowsScreen in windowsScreens)
                {
                    // Check if we already have this screen information via NVIDIA driver
                    if (_screens.Any(scr => scr.ScreenX == windowsScreen.ScreenX && scr.ScreenY == windowsScreen.ScreenY && scr.ScreenWidth == windowsScreen.ScreenWidth && scr.ScreenHeight == windowsScreen.ScreenHeight)){
                        // If the WindowsScreen is already recorded via NVIDIA, then we just ignore it and skip it.
                        continue;
                    }

                    // If we get here then it's a screen that we don't have via NVIDIA, so we need to add it
                    _screens.Add(windowsScreen);
                    SharedLogger.logger.Trace($"ProfileItem/GetNVIDIAScreenPositions: (3) Added a Windows Screen {windowsScreen.Name} ({windowsScreen.ScreenWidth}x{windowsScreen.ScreenHeight}) at position {windowsScreen.ScreenX},{windowsScreen.ScreenY}.");

                }
            }
            catch (Exception ex)
            {
                // Some other exception has occurred and we need to report it.
                SharedLogger.logger.Error(ex, $"ProfileItem/GetNVIDIAScreenPositions: Exception while trying to find any additional windows screens that the NVIDIA driver didn't tell us about.");
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
            _screens = new List<ScreenPosition>() { };

            int pathCount = _windowsDisplayConfig.DisplayConfigPaths.Length;
            // First of all we need to figure out how many display paths we have.
            if (pathCount < 1)
            {
                // Return an empty screen if we have no Display Config Paths to use!
                return _screens;
            }

            // Go through the AMD Eyefinity screens
            if (_amdDisplayConfig.SlsConfig.IsSlsEnabled)
            {
                for (int i = 0; i < _amdDisplayConfig.DisplayMaps.Count; i++)
                {
                    ScreenPosition screen = new ScreenPosition();
                    screen.Library = "AMD";
                    screen.Colour = normalScreenColor;
                    // This is multiple screens
                    screen.SpannedScreens = new List<SpannedScreenPosition>();
                    screen.Name = "AMD Eyefinity";
                    //screen.IsSpanned = true;
                    screen.SpannedRows = _amdDisplayConfig.SlsConfig.SLSMapConfigs[i].SLSMap.Grid.SLSGridRow;
                    screen.SpannedColumns = _amdDisplayConfig.SlsConfig.SLSMapConfigs[i].SLSMap.Grid.SLSGridColumn;
                    screen.Colour = spannedScreenColor;                                           

                    //screen.Name = targetId.ToString();
                    //screen.DisplayConnector = displayMode.DisplayConnector;
                    screen.ScreenX = _amdDisplayConfig.DisplayMaps[i].DisplayMode.XPos;
                    screen.ScreenY = _amdDisplayConfig.DisplayMaps[i].DisplayMode.YPos;
                    screen.Rotation = ScreenRotation.ROTATE_0;
                    
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

                    // Set the taskbar location for this screen at the bottom
                    screen.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;

                    SharedLogger.logger.Trace($"ProfileItem/GetAMDScreenPositions: Added a new AMD Spanned Screen {screen.Name} ({screen.ScreenWidth}x{screen.ScreenHeight}) at position {screen.ScreenX},{screen.ScreenY}.");

                    _screens.Add(screen);
                }
            }

            // Next, go through the screens as Windows knows them, and then enhance the info with Eyefinity data if it applies
            foreach (var path in _windowsDisplayConfig.DisplayConfigPaths)
            {
                // For each path we go through and get the relevant info we need.
                if (_windowsDisplayConfig.DisplayConfigPaths.Length > 0)
                {
                    UInt64 adapterId = path.SourceInfo.AdapterId.Value;
                    UInt32 sourceId = path.SourceInfo.Id;
                    UInt32 targetId = path.TargetInfo.Id;

                    // Set some basics about the screen
                    ScreenPosition screen = new ScreenPosition();
                    screen.Library = "AMD";
                    //screen.AdapterName = adapterId.ToString();
                    screen.IsSpanned = false;
                    screen.Colour = normalScreenColor; // this is the default unless overridden by the primary screen
                    screen.IsClone = false;
                    screen.ClonedCopies = 0;
                    try
                    {
                        // Set the default taskbar position as the bottom of the screen                        
                        screen.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;
                        // If we have a valid taskbar location stored then use that instead
                        if (_windowsDisplayConfig.TaskBarLayout.Count > 0)
                        {
                            foreach (var taskBar in _windowsDisplayConfig.TaskBarLayout)
                            {
                                var taskBarValue = taskBar.Value;
                                if (taskBarValue is TaskBarLayout && taskBarValue.RegKeyValue != null && taskBarValue.RegKeyValue.Contains($"UID{targetId}"))
                                {
                                    screen.TaskBarEdge = taskBarValue.Edge;
                                    break;
                                }
                            }                            
                        }
                        SharedLogger.logger.Trace($"ProfileItem/GetNVIDIAScreenPositions: Position of the taskbar on display {targetId} is on the {screen.TaskBarEdge} of the screen.");
                    }
                    catch (Exception ex)
                    {
                        // Guess that it is at the bottom (90% correct)
                        SharedLogger.logger.Warn(ex, $"ProfileItem/GetNVIDIAScreenPositions: Exception trying to get the position of the taskbar on display {targetId}");
                        screen.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;
                    }

                    // Find out if this source is cloned
                    foreach (var displaySource in _windowsDisplayConfig.DisplaySources)
                    {
                        // All of the items in the Value array are the same source, so we can just check the first one in the array!
                        if (displaySource.Value[0].SourceId == sourceId)
                        {
                            // If there is more than one item in the array, then it's a cloned source!
                            if (displaySource.Value.Count > 1)
                            {
                                // We have a cloned display
                                screen.IsClone = true;
                                screen.ClonedCopies = displaySource.Value.Count;
                            }
                            break;
                        }
                    }

                    // Go through the screens as Windows knows them, and then enhance the info with Mosaic data if it applies
                    foreach (DISPLAYCONFIG_MODE_INFO displayMode in _windowsDisplayConfig.DisplayConfigModes)
                    {
                        // Find the matching Display Config Source Mode
                        if (displayMode.InfoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE && displayMode.Id == sourceId && displayMode.AdapterId.Value == adapterId)
                        {
                            screen.Name = targetId.ToString();
                            //screen.DisplayConnector = displayMode.DisplayConnector;
                            screen.ScreenX = displayMode.SourceMode.Position.X;
                            screen.ScreenY = displayMode.SourceMode.Position.Y;
                            //screen.ScreenWidth = (int)displayMode.SourceMode.Width;
                            //screen.ScreenHeight = (int)displayMode.SourceMode.Height;

                            if (path.TargetInfo.Rotation == DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_IDENTITY)
                            {
                                screen.ScreenWidth = (int)displayMode.SourceMode.Width;
                                screen.ScreenHeight = (int)displayMode.SourceMode.Height;
                                screen.Rotation = ScreenRotation.ROTATE_0;
                            }
                            else if (path.TargetInfo.Rotation == DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_ROTATE90)
                            {
                                // Portrait screen so need to change width and height
                                screen.ScreenWidth = (int)displayMode.SourceMode.Height;
                                screen.ScreenHeight = (int)displayMode.SourceMode.Width;
                                screen.Rotation = ScreenRotation.ROTATE_90;
                            }
                            else if (path.TargetInfo.Rotation == DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_ROTATE180)
                            {
                                screen.ScreenWidth = (int)displayMode.SourceMode.Width;
                                screen.ScreenHeight = (int)displayMode.SourceMode.Height;
                                screen.Rotation = ScreenRotation.ROTATE_180;
                            }
                            else if (path.TargetInfo.Rotation == DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_ROTATE270)
                            {
                                screen.ScreenWidth = (int)displayMode.SourceMode.Width;
                                screen.ScreenHeight = (int)displayMode.SourceMode.Height;
                                screen.Rotation = ScreenRotation.ROTATE_270;
                            }
                            else
                            {
                                screen.ScreenWidth = (int)displayMode.SourceMode.Width;
                                screen.ScreenHeight = (int)displayMode.SourceMode.Height;
                                screen.Rotation = ScreenRotation.ROTATE_0;
                            }

                            // If we're at the 0,0 coordinate then we're the primary monitor
                            if (screen.ScreenX == 0 && screen.ScreenY == 0)
                            {
                                screen.IsPrimary = true;
                                screen.Colour = primaryScreenColor;
                            }
                            break;
                        }
                    }

                    // Decide if this screen is one we've had earlier, and if so, skip it
                    if (_screens.Any(s => s.ScreenX == screen.ScreenX && s.ScreenY == screen.ScreenY && s.ScreenWidth == screen.ScreenWidth && s.ScreenHeight == screen.ScreenHeight))
                    {
                        SharedLogger.logger.Trace($"ProfileItem/GetAMDScreenPositions: We've already got the {screen.Name} ({screen.ScreenWidth}x{screen.ScreenHeight}) screen from the AMD driver, so skipping it from the Windows driver.");
                        continue;
                    }

                    if (_windowsDisplayConfig.DisplayHDRStates.Count > 0)
                    {
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

                    }

                    SharedLogger.logger.Trace($"ProfileItem/GetAMDScreenPositions: Added a new Screen {screen.Name} ({screen.ScreenWidth}x{screen.ScreenHeight}) at position {screen.ScreenX},{screen.ScreenY}.");

                    _screens.Add(screen);
                }
            }

            /*// Now we also need to try and find if there are any other displays connected that don't use an AMD card
            try
            {
                // Get the list of Windows screens
                List<ScreenPosition> windowsScreens = FindAllWindowsScreens();

                // Now look through the list of Windows Screens to see if there are any we haven't already go from AMD.
                // If there are new ones, then add them to the list we're returning
                foreach (ScreenPosition windowsScreen in windowsScreens)
                {
                    // Check if we already have this screen information via AMD driver
                    if (_screens.Any(scr => scr.ScreenX == windowsScreen.ScreenX && scr.ScreenY == windowsScreen.ScreenY && scr.ScreenWidth == windowsScreen.ScreenWidth && scr.ScreenHeight == windowsScreen.ScreenHeight)){
                        // If the WindowsScreen is already recorded via AMD, then we just ignore it and skip it.
                        continue;
                    }

                    // If we get here then it's a screen that we don't have via AMD, so we need to add it
                    _screens.Add(windowsScreen);
                    SharedLogger.logger.Trace($"ProfileItem/GetAMDScreenPositions: (3) Added a Windows Screen {windowsScreen.Name} ({windowsScreen.ScreenWidth}x{windowsScreen.ScreenHeight}) at position {windowsScreen.ScreenX},{windowsScreen.ScreenY}.");

                }
            }
            catch (Exception ex)
            {
                // Some other exception has occurred and we need to report it.
                SharedLogger.logger.Error(ex, $"ProfileItem/GetAMDScreenPositions: Exception while trying to find any additional windows screens that the AMD driver didn't tell us about.");
            }*/

            return _screens;
        }

        private List<ScreenPosition> GetWindowsScreenPositions()
        {
            _screens = FindAllWindowsScreens();
            return _screens;
        }

        private List<ScreenPosition> FindAllWindowsScreens()
        {
            // Set up some colours
            Color primaryScreenColor = Color.FromArgb(0, 174, 241); // represents Primary screen blue
            Color normalScreenColor = Color.FromArgb(155, 155, 155); // represents normal screen colour (gray)

            // Now we create the screens structure from the AMD profile information
            List<ScreenPosition> windowsScreens = new List<ScreenPosition>() { };

            int pathCount = _windowsDisplayConfig.DisplayConfigPaths.Length;
            // First of all we need to figure out how many display paths we have.
            if (pathCount < 1)
            {
                // Return an empty screen if we have no Display Config Paths to use!
                return windowsScreens;
            }

            foreach (var path in _windowsDisplayConfig.DisplayConfigPaths)
            {
                
                UInt64 adapterId = path.SourceInfo.AdapterId.Value;
                UInt32 sourceId = path.SourceInfo.Id;
                UInt32 targetId = path.TargetInfo.Id;

                // Set some basics about the screen
                ScreenPosition screen = new ScreenPosition();
                screen.Library = "WINDOWS";
                //screen.AdapterName = adapterId.ToString();
                screen.IsSpanned = false;
                screen.Colour = normalScreenColor; // this is the default unless overridden by the primary screen
                screen.IsClone = false;
                screen.ClonedCopies = 0;
                try
                {
                    // Set the default taskbar position as the bottom of the screen                        
                    screen.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;
                    // If we have a valid taskbar location stored then use that instead
                    if (_windowsDisplayConfig.TaskBarLayout.Count > 0)
                    {
                        foreach (var taskBar in _windowsDisplayConfig.TaskBarLayout)
                        {
                            var taskBarValue = taskBar.Value;
                            if (taskBarValue is TaskBarLayout && taskBarValue.RegKeyValue != null && taskBarValue.RegKeyValue.Contains($"UID{targetId}"))
                            {
                                screen.TaskBarEdge = taskBarValue.Edge;
                                break;
                            }
                        }
                    }
                    SharedLogger.logger.Trace($"ProfileItem/GetWindowsScreenPositions: Position of the taskbar on display {targetId} is on the {screen.TaskBarEdge} of the screen.");
                }
                catch (Exception ex)
                {
                    // Guess that it is at the bottom (90% correct)
                    SharedLogger.logger.Warn(ex, $"ProfileItem/GetWindowsScreenPositions: Exception trying to get the position of the taskbar on display {targetId}");
                    screen.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;
                }

                // Find out if this source is cloned
                foreach (var displaySource in _windowsDisplayConfig.DisplaySources)
                {
                    // All of the items in the Value array are the same source, so we can just check the first one in the array!
                    if (displaySource.Value[0].SourceId == sourceId)
                    {
                        // If there is more than one item in the array, then it's a cloned source!
                        if (displaySource.Value.Count > 1)
                        {
                            // We have a cloned display
                            screen.IsClone = true;
                            screen.ClonedCopies = displaySource.Value.Count;
                        }
                        break;
                    }
                }

                // Go through the screens as Windows knows them, and then enhance the info with Mosaic data if it applies
                foreach (DISPLAYCONFIG_MODE_INFO displayMode in _windowsDisplayConfig.DisplayConfigModes)
                {
                    // Find the matching Display Config Source Mode
                    if (displayMode.InfoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE && displayMode.Id == sourceId && displayMode.AdapterId.Value == adapterId)
                    {
                        screen.Name = targetId.ToString();
                        //screen.DisplayConnector = displayMode.DisplayConnector;
                        screen.ScreenX = displayMode.SourceMode.Position.X;
                        screen.ScreenY = displayMode.SourceMode.Position.Y;
                        if (path.TargetInfo.Rotation == DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_IDENTITY)
                        {
                            screen.ScreenWidth = (int)displayMode.SourceMode.Width;
                            screen.ScreenHeight = (int)displayMode.SourceMode.Height;
                            screen.Rotation = ScreenRotation.ROTATE_0;
                        }
                        else if (path.TargetInfo.Rotation == DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_ROTATE90)
                        {
                            // Portrait screen so need to change width and height
                            screen.ScreenWidth = (int)displayMode.SourceMode.Height;
                            screen.ScreenHeight = (int)displayMode.SourceMode.Width;
                            screen.Rotation = ScreenRotation.ROTATE_90;
                        }
                        else if (path.TargetInfo.Rotation == DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_ROTATE180)
                        {
                            screen.ScreenWidth = (int)displayMode.SourceMode.Width;
                            screen.ScreenHeight = (int)displayMode.SourceMode.Height;
                            screen.Rotation = ScreenRotation.ROTATE_180;
                        }
                        else if (path.TargetInfo.Rotation == DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_ROTATE270)
                        {
                            screen.ScreenWidth = (int)displayMode.SourceMode.Width;
                            screen.ScreenHeight = (int)displayMode.SourceMode.Height;
                            screen.Rotation = ScreenRotation.ROTATE_270;
                        }
                        else
                        {
                            screen.ScreenWidth = (int)displayMode.SourceMode.Width;
                            screen.ScreenHeight = (int)displayMode.SourceMode.Height;
                            screen.Rotation = ScreenRotation.ROTATE_0;
                        }
                    }
                    else
                    {
                        // Skip DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_DESKTOP_IMAGE and DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET objects
                        continue;
                    }

                    // If we're at the 0,0 coordinate then we're the primary monitor
                    if (screen.ScreenX == 0 && screen.ScreenY == 0)
                    {
                        screen.IsPrimary = true;
                        screen.Colour = primaryScreenColor;
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

                // Now we try to set the taskbar positions
                if (screen.IsPrimary)
                {
                    // If the screen is the primary screen, then we check if we need to use the StuckRect 'Settings' reg keys
                    // rather than the MMStuckRect reg keys
                    try
                    {
                        if (_windowsDisplayConfig.TaskBarLayout.Count(tbr => tbr.Value.RegKeyValue != null && tbr.Value.RegKeyValue.Contains("Settings")) > 0)
                        {
                            // Set the default taskbar position as the bottom of the screen                        
                            screen.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;
                            // If we have a valid taskbar location stored then use that instead
                            if (_windowsDisplayConfig.TaskBarLayout.Count > 0)
                            {
                                foreach (var taskBar in _windowsDisplayConfig.TaskBarLayout)
                                {
                                    var taskBarValue = taskBar.Value;
                                    if (taskBarValue is TaskBarLayout && taskBarValue.RegKeyValue != null && taskBarValue.RegKeyValue.Contains($"Settings"))
                                    {
                                        screen.TaskBarEdge = taskBarValue.Edge;
                                        break;
                                    }
                                }
                            }

                            SharedLogger.logger.Trace($"ProfileItem/GetWindowsScreenPositions: Position of the taskbar on the primary display {targetId} is on the {screen.TaskBarEdge} of the screen.");
                        }
                        else
                        {
                            SharedLogger.logger.Warn($"ProfileItem/GetWindowsScreenPositions: Problem trying to get the position of the taskbar on primary display {targetId}. Assuming it's on the bottom edge.");
                            screen.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Guess that it is at the bottom (90% correct)
                        SharedLogger.logger.Warn(ex, $"ProfileItem/GetWindowsScreenPositions: Exception trying to get the position of the taskbar on primary display {targetId}");
                        screen.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;
                    }

                }
                else
                {
                    try
                    {
                        int numMatches = _windowsDisplayConfig.TaskBarLayout.Count(tbr => tbr.Value.RegKeyValue != null && tbr.Value.RegKeyValue.Contains($"UID{targetId}"));
                        if (numMatches > 1)
                        {
                            var matchingTbls = (from tbl in _windowsDisplayConfig.TaskBarLayout where tbl.Value.RegKeyValue.Contains($"UID{targetId}") select tbl.Value).ToList();
                            bool foundIt = false;
                            foreach (var matchingTbl in matchingTbls)
                            {
                                // find display source that matches.
                                foreach (var displaySource in _windowsDisplayConfig.DisplaySources)
                                {
                                    foreach (var displayDevice in displaySource.Value)
                                    {
                                        // We want to find the displaydevice that has the same adapter id
                                        if (displayDevice.AdapterId.Value == adapterId && displayDevice.DevicePath.Contains(matchingTbl.RegKeyValue))
                                        {
                                            // This is the actual display we want!
                                            foundIt = true;
                                            screen.TaskBarEdge = matchingTbl.Edge;
                                            SharedLogger.logger.Trace($"ProfileItem/GetWindowsScreenPositions: Position of the taskbar on display {targetId} is on the {screen.TaskBarEdge } of the screen.");
                                            break;
                                        }
                                    }         
                                    // If we've found it already then stop looking
                                    if (foundIt)
                                    {
                                        break;
                                    }
                                }
                            }                              
                            if (!foundIt)
                            {
                                screen.TaskBarEdge = _windowsDisplayConfig.TaskBarLayout.First(tbr => tbr.Value.RegKeyValue.Contains($"UID{targetId}")).Value.Edge;
                                SharedLogger.logger.Trace($"ProfileItem/GetWindowsScreenPositions: Couldn't find the taskbar location for display {targetId} when it had multiple matching UIDs. Assuming the screen edge is at the bottom of the screen.");
                            }
                        }
                        else if (numMatches == 1)
                        {
                            screen.TaskBarEdge = _windowsDisplayConfig.TaskBarLayout.First(tbr => tbr.Value.RegKeyValue.Contains($"UID{targetId}")).Value.Edge;
                            SharedLogger.logger.Trace($"ProfileItem/GetWindowsScreenPositions: Position of the taskbar on display {targetId} is on the {screen.TaskBarEdge } of the screen.");
                        }
                        else
                        {
                            SharedLogger.logger.Warn($"ProfileItem/GetWindowsScreenPositions: Problem trying to get the position of the taskbar on display {targetId} as UID doesn't exist. Assuming it's on the bottom edge.");
                            screen.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Guess that it is at the bottom (90% correct)
                        SharedLogger.logger.Warn(ex, $"ProfileItem/GetWindowsScreenPositions: Exception trying to get the position of the taskbar on display {targetId}");
                        screen.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;
                    }
                }

                SharedLogger.logger.Trace($"ProfileItem/GetWindowsScreenPositions: Added a new Screen {screen.Name} ({screen.ScreenWidth}x{screen.ScreenHeight}) at position {screen.ScreenX},{screen.ScreenY}.");

                windowsScreens.Add(screen);

            }

            return windowsScreens;
        }

        private bool GetTaskbarLocationsForNonWindowsScreens(ref List<ScreenPosition> screensToLocate)
        {
            // We first get all of the taskbar locations in a list, so we know what we're looking for
            // We're going to use the taskbar rectangle to figure out which screen its on, so we can do this with any screen position
            Dictionary<Rectangle, TaskBarLayout.TaskBarEdge> taskbarPositions = new Dictionary<Rectangle, TaskBarLayout.TaskBarEdge>() { };

            // If the screen is the primary screen, then we check if we need to use the StuckRect 'Settings' reg keys
            // rather than the MMStuckRect reg keys
            try
            {
                if (_windowsDisplayConfig.TaskBarLayout.Count > 0)
                {
                    foreach (var taskBar in _windowsDisplayConfig.TaskBarLayout)
                    {
                        var taskBarValue = taskBar.Value;
                        // Check that this screen hasn't already been added
                        if (!taskbarPositions.ContainsKey(taskBarValue.MonitorLocation))
                        {
                            taskbarPositions.Add(taskBarValue.MonitorLocation, taskBarValue.Edge);
                            SharedLogger.logger.Trace($"ProfileItem/GetTaskbarLocationsForNonWindowsScreens: Tracking position of the taskbar on the primary display at ({taskBarValue.MonitorLocation.X},{taskBarValue.MonitorLocation.Y}) with an edge location of {taskBarValue.Edge}.");
                        }
                        else 
                        {
                            SharedLogger.logger.Trace($"ProfileItem/GetTaskbarLocationsForNonWindowsScreens: Skipping recording the taskbar poition on the primary display at ({taskBarValue.MonitorLocation.X},{taskBarValue.MonitorLocation.Y}) with an edge location of {taskBarValue.Edge} as it was already recorded.");
                        }
                    }
                 }
            }
            catch (Exception ex)
            {
                // Guess that it is at the bottom (90% correct)
                SharedLogger.logger.Warn(ex, $"ProfileItem/GetTaskbarLocationsForNonWindowsScreens: Exception trying to get the position of the taskbar on primary display for storing it for later");
            }


            for (int i=0; i < screensToLocate.Count;i++)
            {
                var screenToLocate = screensToLocate[i];

                // Set a default
                screenToLocate.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;

                // Create a temp rectangle for the window
                Rectangle displayWindow = new Rectangle(screenToLocate.ScreenX, screenToLocate.ScreenX, screenToLocate.ScreenWidth, screenToLocate.ScreenHeight);

                // find which taskbar is in this window
                foreach (Rectangle taskBarRect in taskbarPositions.Keys)
                {
                    if (displayWindow.Contains(taskBarRect))
                    {
                        if (taskBarRect.Bottom == displayWindow.Bottom)
                        {
                            screenToLocate.TaskBarEdge = TaskBarLayout.TaskBarEdge.Bottom;
                        }
                        else if(taskBarRect.Left == displayWindow.Left)
                        {
                            screenToLocate.TaskBarEdge = TaskBarLayout.TaskBarEdge.Left;
                        }
                        else if (taskBarRect.Top == displayWindow.Top)
                        {
                            screenToLocate.TaskBarEdge = TaskBarLayout.TaskBarEdge.Top;
                        }
                        else if (taskBarRect.Right == displayWindow.Right)
                        {
                            screenToLocate.TaskBarEdge = TaskBarLayout.TaskBarEdge.Right;
                        }
                        break;
                    }
                }
            }
            return true;
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
                AMDDisplayConfig.Equals(other.AMDDisplayConfig) && 
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

        public string CreateCommand()
        {
            return $"{Application.ExecutablePath} {DisplayMagicianStartupAction.ChangeProfile} \"{UUID}\"";
        }

    }
}


