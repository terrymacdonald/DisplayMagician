using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using WindowsDisplayAPI.DisplayConfig;
using HeliosPlus.Shared.Resources;
using Newtonsoft.Json;
using NvAPIWrapper.Mosaic;
using NvAPIWrapper.Native.Mosaic;
using HeliosPlus.Shared.Topology;
using System.Drawing;
using System.Drawing.Imaging;
using WindowsDisplayAPI;
using System.Text.RegularExpressions;

namespace HeliosPlus.Shared
{
    public class ProfileItem
    {
        private static List<ProfileItem> _allSavedProfiles = new List<ProfileItem>();
        private ProfileIcon _profileIcon;
        private Bitmap _profileBitmap, _profileShortcutBitmap;
        private static List<Display> _availableDisplays;
        private static List<UnAttachedDisplay> _unavailableDisplays;

        internal static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HeliosPlus");

        private string _uuid;
        private Version _version;
        private bool _isActive = false;
        private bool _isPossible = false;


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
                MemoryStream memoryStream = new MemoryStream(byteBuffer);
                memoryStream.Position = 0;

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
        static ProfileItem()
        {
            try
            {
                NvAPIWrapper.NVIDIA.Initialize();
            }
            catch
            {
                // ignored
            }

        }

        public static Version Version = new Version(2, 1);

        #region Instance Properties

        public string UUID
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_uuid))
                    _uuid = Guid.NewGuid().ToString("B"); 
                return _uuid;
            }
            set
            {
                string uuidV4Regex = @"/^[0-9A-F]{8}-[0-9A-F]{4}-4[0-9A-F]{3}-[89AB][0-9A-F]{3}-[0-9A-F]{12}$/i";
                Match match = Regex.Match(value, uuidV4Regex, RegexOptions.IgnoreCase);
                if (match.Success)
                    _uuid = value;
            }
        }

        [JsonIgnore]
        public bool IsPossible
        {
            get
            {
                List<string> unavailableDeviceNames = new List<string>();
                // Then go through the displays in the profile and check they're live
                foreach (ProfileViewport profileViewport in Viewports)
                {
                    foreach (ProfileViewportTargetDisplay profileViewportTargetDisplay in profileViewport.TargetDisplays)
                    {
                        PathTargetInfo viewportTargetInfo = profileViewportTargetDisplay.ToPathTargetInfo();
                        // If viewportTargetInfo is null, then this viewportTargetDisplay isn't currently available
                        // so this makes the profile invalid
                        if (viewportTargetInfo == null)
                            unavailableDeviceNames.Add(profileViewportTargetDisplay.DisplayName);
                    }
                }

                if (unavailableDeviceNames.Count > 0)
                {
                    // Darn - there was at least one unavilable displaytarget
                    return false;
                }
                else
                {
                    // There were no unavailable DisplayTargets!
                    return true;
                }

            }
        }

        public string Name { get; set; }

        public ProfileViewport[] Viewports { get; set; } = new ProfileViewport[0];

        public ProfileIcon ProfileIcon
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

        public string SavedProfileIconCacheFilename { get; set; }

        [JsonConverter(typeof(CustomBitmapConverter))]
        public Bitmap ProfileBitmap
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
        public Bitmap ProfileTightestBitmap
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

        public static bool IsValidId(string testId)
        {
            string uuidV4Regex = @"/^[0-9A-F]{8}-[0-9A-F]{4}-4[0-9A-F]{3}-[89AB][0-9A-F]{3}-[0-9A-F]{12}$/i";
            Match match = Regex.Match(testId, uuidV4Regex, RegexOptions.IgnoreCase);
            if (match.Success)
                return true;
            else
                return false;
        }


        public bool CopyTo(ProfileItem profile, bool overwriteId = false)
        {
            if (!(profile is ProfileItem))
                return false;

            if (overwriteId)
                profile.UUID = UUID;

            // Copy all our profile data over to the other profile
            profile.Name = Name;
            profile.UUID = UUID;
            profile.Name = Name;
            profile.Viewports = Viewports;
            profile.ProfileIcon = ProfileIcon;
            profile.SavedProfileIconCacheFilename = SavedProfileIconCacheFilename;
            profile.ProfileBitmap = ProfileBitmap;
            profile.ProfileTightestBitmap = ProfileTightestBitmap;
            return true;
        }


        // The public override for the Object.Equals
        public override bool Equals(object obj)
        {
            return this.Equals(obj as ProfileItem);
        }

        // Profiles are equal if their contents (except name) are equal
        public bool Equals(ProfileItem other)
        {

            // If parameter is null, return false.
            if (Object.ReferenceEquals(other, null))
                return false;

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, other))
                return true;

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != other.GetType())
                return false;
            
            // Check whether the profiles' properties are equal
            // We need to exclude the name as the name is solely for saving to disk
            // and displaying to the user. 
            // Two profiles are equal only when they have the same viewport data
            if (Viewports.SequenceEqual(other.Viewports))
                return true;
            else
                return false;
        }

        // If Equals() returns true for this object compared to  another
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {

            // Get hash code for the Viewports field if it is not null.
            int hashViewports = Viewports == null ? 0 : Viewports.GetHashCode();

            //Calculate the hash code for the product.
            return hashViewports;

        }


        public override string ToString()
        {
            return (Name ?? Language.UN_TITLED_PROFILE);
        }

        private static string GetValidFilename(string uncheckedFilename)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                uncheckedFilename = uncheckedFilename.Replace(c.ToString(), "");
            }
            return uncheckedFilename;
        }

        public void ApplyTopos()
        {
            Debug.Print("_applyTopos()");
            try
            {
                var surroundTopologies =
                    Viewports.SelectMany(viewport => viewport.TargetDisplays)
                        .Select(target => target.SurroundTopology)
                        .Where(topology => topology != null)
                        .Select(topology => topology.ToGridTopology())
                        .ToArray();

                if (surroundTopologies.Length == 0)
                {
                    var currentTopologies = GridTopology.GetGridTopologies();

                    if (currentTopologies.Any(topology => topology.Rows * topology.Columns > 1))
                    {
                        surroundTopologies =
                            GridTopology.GetGridTopologies()
                                .SelectMany(topology => topology.Displays)
                                .Select(displays => new GridTopology(1, 1, new[] { displays }))
                                .ToArray();
                    }
                }

                if (surroundTopologies.Length > 0)
                {
                    GridTopology.SetGridTopologies(surroundTopologies, SetDisplayTopologyFlag.MaximizePerformance);
                }
            }
            catch
            {
                // ignored
            }
        }

        public void ApplyPathInfos()
        {
            Debug.Print("_applyPathInfos()");
            if (!IsPossible)
            {
                throw new InvalidOperationException(
                    $"Problem applying the '{Name}' Display Profile! The display configuration changed since this profile is created. Please re-create this profile.");
            }

            var pathInfos = Viewports.Select(viewport => viewport.ToPathInfo()).Where(info => info != null).ToArray();
            PathInfo.ApplyPathInfos(pathInfos, true, true, true);
        }

        public IDictionary<string, Action> applyProfileActions()
        {
            var dict = new Dictionary<string, Action>()
            {
                { "Applying_Topos", ApplyTopos },
                { "Applying_Paths", ApplyPathInfos }
            };
            return dict;
        }

        public IDictionary<string, string> applyProfileMsgs()
        {
            var dict = new Dictionary<string, string>()
            {
                { "Applying_Topos", Language.Applying_First_Message },
                { "Applying_Paths", Language.Applying_Second_Message }
            };
            return dict;
        }

        public List<string> applyProfileSequence()
        {
            var list = new List<string>() { "Applying_Topos", "Applying_Paths" };
            return list;
        }

    }

    // Custom comparer for the Profile class
    // Allows us to use 'Contains'
    class ProfileComparer : IEqualityComparer<ProfileItem>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(ProfileItem x, ProfileItem y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            // Check whether the profiles' properties are equal
            // We need to exclude the name as the name is solely for saving to disk
            // and displaying to the user. 
            // Two profiles are equal only when they have the same viewport data
            if (x.Viewports.Equals(y.Viewports))
                return true;
            else
                return false;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.
        public int GetHashCode(ProfileItem profile)
        {

            // Check whether the object is null
            if (Object.ReferenceEquals(profile, null)) return 0;

            // Get hash code for the Viewports field if it is not null.
            int hashViewports = profile.Viewports == null ? 0 : profile.Viewports.GetHashCode();

            //Calculate the hash code for the product.
            return hashViewports;

        }

    }
}