using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using WindowsDisplayAPI.DisplayConfig;
using HeliosDisplayManagement.Shared.Resources;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Mosaic;
using NvAPIWrapper.Native.Mosaic;
using Path = HeliosDisplayManagement.Shared.Topology.Path;

namespace HeliosDisplayManagement.Shared
{
    public class Profile : IEquatable<Profile>
    {
        private static Profile _currentProfile;

        public static Version Version = new Version(1, 0);

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

        public bool IsActive
        {
            get
            {
                if (_currentProfile == null)
                    _currentProfile = GetCurrent(string.Empty);
                return _currentProfile.Equals(this);
            }
        }

        public bool IsPossible
        {
            get
            {
                var surroundTopologies =
                    Paths.SelectMany(path => path.Targets)
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
                                topology => topology.Displays.All(display => displayDevices.Contains(display.DisplayId))))
                            return false;

                        // And to see if one path have two surround targets
                        if (Paths.Any(path => path.Targets.Count(target => target.SurroundTopology != null) > 1))
                            return false;

                        return true;
                    }
                    catch
                    {
                        // ignore
                    }
                    return false;
                }
                return true;
                //return PathInfo.ValidatePathInfos(Paths.Select(path => path.ToPathInfo()));
            }
        }

        public string Name { get; set; }

        public Path[] Paths { get; set; } = new Path[0];

        public static string ProfilesPath
            =>
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Assembly.GetExecutingAssembly().GetName().Name, $"DisplayProfiles_{Version.ToString(2)}.xml");

        /// <inheritdoc />
        public bool Equals(Profile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Paths.All(path => other.Paths.Contains(path));
        }

        public static IEnumerable<Profile> GetAllProfiles()
        {
            try
            {
                if (File.Exists(ProfilesPath))
                {
                    var xml = File.ReadAllText(ProfilesPath, Encoding.Unicode);
                    if (!string.IsNullOrWhiteSpace(xml))
                    {
                        var serializer = XmlSerializer.FromTypes(new[] {typeof(Profile[])})[0];
                        using (var reader = XmlReader.Create(new StringReader(xml)))
                        {
                            return (Profile[]) serializer.Deserialize(reader);
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
            return new Profile[0];
        }

        public static Profile GetCurrent(string name = null)
        {
            _currentProfile = new Profile
            {
                Name = name,
                Paths = PathInfo.GetActivePaths().Select(info => new Path(info)).ToArray()
            };
            return _currentProfile;
        }

        public static bool operator ==(Profile left, Profile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Profile left, Profile right)
        {
            return !Equals(left, right);
        }

        public static void RefreshActiveStatus()
        {
            _currentProfile = null;
        }

        public static bool SetAllProfiles(IEnumerable<Profile> array)
        {
            try
            {
                var serializer = XmlSerializer.FromTypes(new[] {typeof(Profile[])})[0];
                var sb = new StringBuilder();
                using (var writer = XmlWriter.Create(sb))
                {
                    serializer.Serialize(writer, array.ToArray());
                }
                var xml = sb.ToString();
                try
                {
                    var doc = XDocument.Parse(xml);
                    xml = doc.ToString();
                }
                catch
                {
                    // ignored
                }
                if (!string.IsNullOrWhiteSpace(xml))
                {
                    var dir = System.IO.Path.GetDirectoryName(ProfilesPath);
                    if (dir != null)
                    {
                        Directory.CreateDirectory(dir);
                        File.WriteAllText(ProfilesPath, xml, Encoding.Unicode);
                        return true;
                    }
                }
            }
            catch
            {
                // ignored
            }
            return false;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Profile) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Paths?.GetHashCode() ?? 0)*397;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return (Name ?? Language.UN_TITLED_PROFILE) + (IsActive ? " " + Language._Active_ : "");
        }

        public bool Apply()
        {
            try
            {
                Thread.Sleep(2000);
                try
                {
                    var surroundTopologies =
                        Paths.SelectMany(path => path.Targets)
                            .Select(target => target.SurroundTopology)
                            .Where(topology => topology != null)
                            .Select(topology => topology.ToGridTopology())
                            .ToArray();
                    if (surroundTopologies.Length == 0)
                    {
                        var currentTopologies = GridTopology.GetGridTopologies();
                        if (currentTopologies.Any(topology => topology.Rows*topology.Columns > 1))
                            surroundTopologies =
                                GridTopology.GetGridTopologies()
                                    .SelectMany(topology => topology.Displays)
                                    .Select(displays => new GridTopology(1, 1, new[] {displays}))
                                    .ToArray();
                    }
                    if (surroundTopologies.Length > 0)
                        GridTopology.SetGridTopologies(surroundTopologies, SetDisplayTopologyFlag.MaximizePerformance);
                }
                catch
                {
                    // ignored
                }
                Thread.Sleep(19000);
                PathInfo.ApplyPathInfos(Paths.Select(path => path.ToPathInfo()), true, true);
                Thread.Sleep(9000);
                RefreshActiveStatus();
                return true;
            }
            catch
            {
                RefreshActiveStatus();
                return false;
            }
        }

        public Profile Clone()
        {
            var serializer = XmlSerializer.FromTypes(new[] {typeof(Profile)})[0];
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                serializer.Serialize(writer, this);
            }
            using (var reader = XmlReader.Create(new StringReader(sb.ToString())))
            {
                return (Profile) serializer.Deserialize(reader);
            }
        }
    }
}