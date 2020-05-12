using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WindowsDisplayAPI.DisplayConfig;
using HeliosPlus.Shared.Resources;
using Newtonsoft.Json;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Mosaic;
using NvAPIWrapper.Native.Mosaic;
using HeliosPlus.Shared.Topology;
using System.Drawing;
using System.Drawing.Imaging;
using WindowsDisplayAPI;
using System.Text.RegularExpressions;

namespace HeliosPlus.Shared
{
    public class Profile : IEquatable<Profile>
    {
        private static Profile _currentProfile;
        private static List<Profile> _allSavedProfiles = new List<Profile>();
        private ProfileIcon _profileIcon;
        private Bitmap _profileBitmap;
        private static List<Display> _availableDisplays;

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

                // Firstly check which displays are attached to this computer
                _availableDisplays = Display.GetDisplays().ToList();
                //  DevicePath	"\\\\?\\DISPLAY#DELD065#5&38860457&0&UID4609#{e6f07b5f-ee97-4a90-b076-33f57bf4eaa7}"	
                List<string> availableDevicePath = new List<string>();
                foreach (Display aDisplay in _availableDisplays)
                {
                    // Grab the shorter device path from the display Api
                    Regex devicePathRegex = new Regex(@"^(.+?)#\{", RegexOptions.IgnoreCase);
                    Match devicePathMatches = devicePathRegex.Match(aDisplay.DevicePath);
                    if (devicePathMatches.Success)
                    {
                        availableDevicePath.Add(devicePathMatches.Groups[1].Value.ToString());
                    }
                }

                if (availableDevicePath.Count > 0)
                {

                    int profileDisplaysCount = 0;
                    int profileDisplaysFoundCount = 0;
                    // Then go through the displays in the profile and check they're live
                    foreach (ProfileViewport profileViewport in Viewports)
                    {
                        foreach (ProfileViewportTargetDisplay profileViewportTargetDisplay in profileViewport.TargetDisplays)
                        {
                            profileDisplaysCount++;
                            if (profileViewportTargetDisplay.DevicePath.Equals(availableDevicePath));
                                profileDisplaysFoundCount++;
                        }
                    }

                    if (profileDisplaysCount == profileDisplaysFoundCount)
                    {
                        // If we found all our profile displays in the available displays!
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }


                // This should include the ones not currently enabled.
                var displayAdapters = DisplayAdapter.GetDisplayAdapters().ToArray();
                var displays = Display.GetDisplays().ToArray();


                var physicalGPUs = PhysicalGPU.GetPhysicalGPUs();
                var displayDevices = physicalGPUs[0].GetDisplayDevices().ToArray();
                return true;

                /*var surroundTopologies =
                    Viewports.SelectMany(path => path.TargetDisplays)
                        .Select(target => target.SurroundTopology)
                        .Where(topology => topology != null).ToArray();

                if (surroundTopologies.Length > 0)
                {
                    try
                    {
                        // Not working quite well yet
                        //var status =
                        //    GridTopology.ValidateGridTopologies(
                        //        SurroundTopologies.Select(topology => topology.ToGridTopology()).ToArray(),
                        //        SetDisplayTopologyFlag.MaximizePerformance);
                        //return status.All(topologyStatus => topologyStatus.Errors == DisplayCapacityProblem.NoProblem);

                        // Least we can do is to check for the availability of all display devices
                        var displayDevices =
                            PhysicalGPU.GetPhysicalGPUs()
                                .SelectMany(gpu => gpu.GetDisplayDevices())
                                .Select(device => device.DisplayId);

                        if (!
                            surroundTopologies.All(
                                topology =>
                                    topology.Displays.All(display => displayDevices.Contains(display.DisplayId))))
                        {
                            return false;
                        }

                        // And to see if one path have two surround targets
                        if (Viewports.Any(path => path.TargetDisplays.Count(target => target.SurroundTopology != null) > 1))
                        {
                            return false;
                        }

                        return true;
                    }
                    catch
                    {
                        // ignore
                    }

                    return false;
                }

                return true;*/
                //return PathInfo.ValidatePathInfos(Paths.Select(path => path.ToPathInfo()));
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
            get => _allSavedProfiles;
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

        /// <inheritdoc />
        public bool Equals(Profile other)
        {
            //if the other profile points to null, it's not equal
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            //if the other profile is the same object, it's equal
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            //return Paths.All(path => other.Paths.Contains(path));
            if (Viewports.Length != other.Viewports.Length)
            {
                return false;
            }

            int thisToOtherViewportCount = 0;
            int otherToThisViewportCount = 0;

            foreach (ProfileViewport myProfileViewport in Viewports)
            {
                foreach (ProfileViewport otherProfileViewport in other.Viewports)
                {
                    if (myProfileViewport.Equals(otherProfileViewport))
                    {
                        thisToOtherViewportCount++;
                    }
                }
                
            }

            foreach (ProfileViewport otherProfileViewport in other.Viewports)
            {
                foreach (ProfileViewport myProfileViewport in Viewports)
                {
                    if (myProfileViewport.Equals(otherProfileViewport))
                    {
                        otherToThisViewportCount++;
                    }
                }

            }

            if (thisToOtherViewportCount == otherToThisViewportCount)
            {
                return true;
            }
            else
            {
                return false;
            }

            return false;
        }

        public static IEnumerable<Profile> LoadAllProfiles()
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

                        if (loadedProfile.Equals(myCurrentProfile)) {
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

        

        public static bool operator ==(Profile left, Profile right)
        {
            return Equals(left, right) || left?.Equals(right) == true;
        }

        public static bool operator !=(Profile left, Profile right)
        {
            return !(left == right);
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


        public static void RefreshActiveStatus()
        {
            //_currentProfile = null;
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
                    var dir = System.IO.Path.GetDirectoryName(SavedProfilesFilePath);

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

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Profile) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Viewports?.GetHashCode() ?? 0) * 397;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return (Name ?? Language.UN_TITLED_PROFILE) + (IsActive ? " " + Language._Active_ : "");
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

        public bool Apply()
        {
            try
            {
                Thread.Sleep(2000);

                try
                {
                    var surroundTopologies =
                        Viewports.SelectMany(path => path.TargetDisplays)
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
                                    .Select(displays => new GridTopology(1, 1, new[] {displays}))
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

                Thread.Sleep(18000);
                var pathInfos = Viewports.Select(path => path.ToPathInfo()).Where(info => info != null).ToArray();

                if (!pathInfos.Any())
                {
                    throw new InvalidOperationException(
                        @"Display configuration changed since this profile is created. Please re-create this profile.");
                }

                PathInfo.ApplyPathInfos(pathInfos, true, true, true);
                Thread.Sleep(10000);
                RefreshActiveStatus();

                return true;
            }
            catch (Exception ex)
            {
                RefreshActiveStatus();
                MessageBox.Show(ex.Message, @"Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return false;
            }
        }

        public Profile Clone()
        {
            try
            {
                var serialized = JsonConvert.SerializeObject(this);

                var cloned = JsonConvert.DeserializeObject<Profile>(serialized);
                cloned.Id = Guid.NewGuid().ToString("B");

                return cloned;
            }
            catch
            {
                return null;
            }
        }
    }
}