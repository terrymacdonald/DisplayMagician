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

namespace HeliosPlus.Shared
{
    public class Profile
    {
        private static Profile _currentProfile;
        private static List<Profile> _allSavedProfiles = new List<Profile>();
        private ProfileIcon _profileIcon;
        private Bitmap _profileBitmap;
        private static List<Display> _availableDisplays;
        private static List<UnAttachedDisplay> _unavailableDisplays;

        internal static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HeliosPlus");


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
        static Profile()
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

        public string Id { get; set; } = Guid.NewGuid().ToString("B");

        [JsonIgnore]
        public bool IsActive
        {
            get
            {
                return _currentProfile.Equals(this);
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

        public static string SavedProfilesFilePath
        {
            get => System.IO.Path.Combine(AppDataPath, $"Profiles\\DisplayProfiles_{Version.ToString(2)}.json");
        }

        public static string SavedProfilesPath
        {
            get => System.IO.Path.Combine(AppDataPath, $"Profiles");
        }

        public static List<Profile> AllSavedProfiles 
        {
            get
            {
                if (_allSavedProfiles.Count == 0)
                {
                    Profile.LoadAllProfiles();
                }
                return _allSavedProfiles;
            }
        }

        public static Profile CurrentProfile
        {
            get => _currentProfile;
        }


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

        public string SavedProfileCacheFilename { get; set; }


        [JsonConverter(typeof(CustomBitmapConverter))]
        public Bitmap ProfileBitmap
        {
            get
            {
                if (_profileBitmap != null)
                    return _profileBitmap;
                else
                {
                    _profileBitmap = this.ProfileIcon.ToBitmap(128, 128);
                    return _profileBitmap;
                }
            }
            set
            {
                _profileBitmap = value;
            }

        }

        public static List<Profile> LoadAllProfiles()
        {
            
            if (File.Exists(SavedProfilesFilePath))
            {
                var json = File.ReadAllText(SavedProfilesFilePath, Encoding.Unicode);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    List<Profile> profiles = new List<Profile>();
                    try
                    {
                        //var profiles = JsonConvert.DeserializeObject<Profile[]>(json, new JsonSerializerSettings
                        profiles = JsonConvert.DeserializeObject<List<Profile>>(json, new JsonSerializerSettings
                        {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Include,
                            TypeNameHandling = TypeNameHandling.Auto
                        });
                    }
                    catch (Exception ex)
                    {
                        // ignored
                        Console.WriteLine("Unable to deserialize profile: " + ex.Message);
                    }


                    //Convert array to list
                    //List<Profile> profilesList = profiles.ToList<Profile>();

                    // Find which entry is being used now, and save that info in a class variable
                    Profile myCurrentProfile = new Profile
                    {
                        Name = "Current Display Profile",
                        Viewports = PathInfo.GetActivePaths().Select(info => new ProfileViewport(info)).ToArray()
                    };

                    _currentProfile = myCurrentProfile;

                    foreach (Profile loadedProfile in profiles)
                    {
                        // Save a profile Icon to the profile
                        loadedProfile.ProfileIcon = new ProfileIcon(loadedProfile);
                        loadedProfile.ProfileBitmap = loadedProfile.ProfileIcon.ToBitmap(128,128);

                        if (loadedProfile.IsActive) {
                            _currentProfile = loadedProfile;
                        }

                    }

                    _allSavedProfiles = profiles;

                    return _allSavedProfiles;
                }
            }

            // If we get here, then we don't have any profiles saved!
            // So we gotta start from scratch
            // Create a new profile based on our current display settings
            _currentProfile = new Profile
            {
                Name = "Current Display Profile",
                Viewports = PathInfo.GetActivePaths().Select(info => new ProfileViewport(info)).ToArray()
            };

            // Save a profile Icon to the profile
            _currentProfile.ProfileIcon = new ProfileIcon(_currentProfile);
            _currentProfile.ProfileBitmap = _currentProfile.ProfileIcon.ToBitmap(128, 128);

            // Create a new empty list of all our display profiles as we don't have any saved!
            _allSavedProfiles = new List<Profile>();

            return _allSavedProfiles;
        }

        public static bool IsValidName(string testName)
        {
            foreach (Profile loadedProfile in _allSavedProfiles)
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
            foreach (Profile loadedProfile in _allSavedProfiles)
            {
                if (loadedProfile.Id == testId)
                {
                    return false;
                }
            }

            return true;
        }


        public static void UpdateCurrentProfile()
        {
            _currentProfile = new Profile
            {
                Name = "Current Display Profile",
                Viewports = PathInfo.GetActivePaths().Select(info => new ProfileViewport(info)).ToArray()
            };
        }

        public static bool SaveAllProfiles()
        {
            if (SaveAllProfiles(_allSavedProfiles))
                return true;
            return false;
        }

        public static bool SaveAllProfiles(List<Profile> profilesToSave)
        {

            if (!Directory.Exists(SavedProfilesPath))
            {
                try
                {
                    Directory.CreateDirectory(SavedProfilesPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to create Profile folder " + SavedProfilesPath + ": " + ex.Message);
                }
            }


            // Now we loop over the profiles and save their images for later
            foreach (Profile profileToSave in profilesToSave)
            {
                profileToSave.SaveProfileImageToCache();
            }

            try
            {
                var json = JsonConvert.SerializeObject(profilesToSave, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    TypeNameHandling = TypeNameHandling.Auto

                });


                if (!string.IsNullOrWhiteSpace(json))
                {
                    var dir = System.IO.Path.GetDirectoryName(SavedProfilesPath);

                    if (dir != null)
                    {
                        Directory.CreateDirectory(dir);
                        File.WriteAllText(SavedProfilesFilePath, json, Encoding.Unicode);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to serialize profile: " + ex.Message);
            }

            // Overwrite the list of saved profiles as the new lot we received.
            _allSavedProfiles = profilesToSave;

            return false;
        }


        // The public override for the Object.Equals
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Profile);
        }

        // Profiles are equal if their contents (except name) are equal
        public bool Equals(Profile other)
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

        public bool SaveProfileImageToCache()
        {
            this.SavedProfileCacheFilename = Path.Combine(SavedProfilesPath, GetValidFilename(String.Concat(this.Name + @".png")));
            try
            {
                this.ProfileBitmap.Save(this.SavedProfileCacheFilename, ImageFormat.Png);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to create profile image in cache using " + this.SavedProfileCacheFilename + ": " + ex.Message);
                return false;
            }
        }

        private void _applyTopos()
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

        private void _applyPathInfos()
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
                { "Applying_Topos", _applyTopos },
                { "Applying_Paths", _applyPathInfos }
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

        public bool Apply()
        {
            try
            {

                Debug.Print("Begin profile change");
                Thread.Sleep(2000);
                _applyTopos();

                Debug.Print("Finished setting topologies");
                Debug.Print("Sleep");
                Thread.Sleep(18000);
                Debug.Print("Awake");

                _applyPathInfos();

                Debug.Print("Applying pathInfos");
                Debug.Print("Sleep");
                Thread.Sleep(10000);
                Debug.Print("Awake");

                UpdateCurrentProfile();

                return true;
            }
            catch (Exception ex)
            {
                UpdateCurrentProfile();
                Console.WriteLine($"Profile: Problem applying the '{Name}' Display Profile: {ex.Message}");
                MessageBox.Show($"Problem applying the '{Name}' Display Profile! \n(ex.Message)", $"Problem applying '{Name}' Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

    }

    // Custom comparer for the Profile class
    // Allows us to use 'Contains'
    class ProfileComparer : IEqualityComparer<Profile>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(Profile x, Profile y)
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
        public int GetHashCode(Profile profile)
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