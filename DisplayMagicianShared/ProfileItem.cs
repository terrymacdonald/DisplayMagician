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
//using WK.Libraries.HotkeyListenerNS;

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
        // These fields indicate this. THe spanned screens are added to the SpannedScreens field
        public bool IsSpanned;
        public string SpannedName;
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
        public bool IsPrimary;
        public Color Colour;
        public List<string> Features;
        public int Column;
        public int Row;
    }

    public class ProfileItem : IComparable
    {
        private static List<ProfileItem> _allSavedProfiles = new List<ProfileItem>();
        private ProfileIcon _profileIcon;
        private Bitmap _profileBitmap, _profileShortcutBitmap;
        private List<string> _profileDisplayIdentifiers = new List<string>();
        private List<ScreenPosition> _screens = new List<ScreenPosition>();

        internal static string AppDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician");
        private static readonly string uuidV4Regex = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";

        private string _uuid = "";
        private bool _isPossible = false;
        private Keys _hotkey = Keys.None;
       

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

        public virtual VIDEO_MODE VideoMode { get; } = VIDEO_MODE.WINDOWS;

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
        //[JsonIgnore]
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

        public virtual bool IsValid()
        {
            return false;
        }

        

        public virtual bool CopyTo(ProfileItem profile, bool overwriteId = true)
        {
            if (!(profile is ProfileItem))
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


        public virtual bool CreateProfileFromCurrentDisplaySettings()
        {
            return false;                
        }

        public virtual bool PerformPostLoadingTasks()
        {
            return false;
        }


        // The public override for the Object.Equals
        public override bool Equals(object obj)
        {
            return this.Equals(obj as ProfileItem);
        }

        // Profiles are equal if their Viewports are equal
        public virtual bool Equals(ProfileItem other)
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

            //if (Paths.Length != other.Paths.Length)
            //    return false;

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

            // Get Paths too
            //int hashPaths = Paths == null ? 0 : Paths.GetHashCode();
            int hashPaths = 0;

            // Calculate the hash code for the product.
            return (hashIds, hashPaths).GetHashCode();

        }
        

        public override string ToString()
        {
            return (Name ?? Language.UN_TITLED_PROFILE);
        }

        public int CompareTo(object obj)
        {
            if (!(obj is ProfileItem)) throw new ArgumentException("Object to CompareTo is not a Shortcut"); ;

            ProfileItem otherProfile = (ProfileItem)obj;
            return this.Name.CompareTo(otherProfile.Name);
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

        public virtual List<ScreenPosition> GetScreenPositions()
        {
            return new List<ScreenPosition>();
        }
    }

    // Custom Equality comparer for the Profile class
    // Allows us to use 'Contains'
    class ProfileComparer : IEqualityComparer<ProfileItem>
    {
        // Products are equal if their names and product numbers are equal.
        /*public bool Equals(ProfileItem x, ProfileItem y)
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

        public bool Equals(ProfileItem x, ProfileItem y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (x is null || y is null)
                return false;

            //if (x.Paths.Length != y.Paths.Length)
            //    return false;

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
            /*foreach (Topology.Path profilePath in x.Paths)
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
            }*/


            if (foundPathsCount == foundOtherPathsCount)
                return true;
            else
                return false;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.
        /*public int GetHashCode(ProfileItem profile)
        {

            // Check whether the object is null
            if (profile is null) return 0;

            // Get hash code for the Viewports field if it is not null.
            int hashPaths = profile.Paths == null ? 0 : profile.Paths.GetHashCode();

            //Calculate the hash code for the product.
            return hashPaths;

        }*/
        // Modified the GetHashCode to compare the displayidentifier
        public int GetHashCode(ProfileItem profile)
        {

            // Check whether the object is null
            if (profile is null) return 0;

            // Get hash code for the ProfileDisplayIdentifiers field if it is not null.
            int hashIds = profile.ProfileDisplayIdentifiers == null ? 0 : profile.ProfileDisplayIdentifiers.GetHashCode();

            // Get hash code for the Paths
            //int hashPaths = profile.Paths == null ? 0 : profile.Paths.GetHashCode();
            int hashPaths = 0;

            //Calculate the hash code for the product.
            return (hashIds,hashPaths).GetHashCode();

        }
    }
}