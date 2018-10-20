using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WindowsDisplayAPI;
using WindowsDisplayAPI.DisplayConfig;
using HeliosDisplayManagement.Shared;
using HeliosDisplayManagement.Shared.Topology;

namespace HeliosDisplayManagement
{
    internal class DisplayRepresentation
    {
        public DisplayRepresentation(Display display)
        {
            Name = display.DeviceName;
            Path = display.DevicePath;
            var index = Path.IndexOf("{", StringComparison.InvariantCultureIgnoreCase);

            if (index > 0)
            {
                Path = Path.Substring(0, index).TrimEnd('#');
            }

            IsAvailable = display.IsAvailable;

            if (IsAvailable)
            {
                PossibleSettings = GetDisplay()?.GetPossibleSettings()?.ToArray() ?? new DisplayPossibleSetting[0];
            }
        }

        public DisplayRepresentation(PathTarget display)
        {
            Name = display.DisplayName;
            Path = display.DevicePath;
            IsAvailable = GetDisplay()?.IsAvailable ?? false;

            if (IsAvailable)
            {
                PossibleSettings = GetDisplay()?.GetPossibleSettings()?.ToArray() ?? new DisplayPossibleSetting[0];
            }
        }

        public bool IsAvailable { get; }
        public string Name { get; }
        public string Path { get; }

        public DisplayPossibleSetting[] PossibleSettings { get; }

        public static IEnumerable<DisplayRepresentation> GetDisplays(Profile profile = null)
        {
            //var displays =
            //    Display.GetDisplays()
            //        .Select(display => new DisplayRepresentation(display))
            //        .OrderByDescending(representation => representation.IsAvailable)
            //        .GroupBy(representation => representation.Path)
            //        .Select(grouping => grouping.First()).ToList();
            var displays = new List<DisplayRepresentation>();

            if (profile != null)
            {
                foreach (var target in profile.Paths.SelectMany(path => path.Targets))
                {
                    if (displays.All(display => display.Path != target.DevicePath))
                    {
                        displays.Add(new DisplayRepresentation(target));
                    }
                }
            }

            return displays;
        }

        public Display GetDisplay()
        {
            return Display.GetDisplays().FirstOrDefault(display => display.DevicePath.StartsWith(Path));
        }

        public Path GetPathSource(Profile profile)
        {
            return profile.Paths.FirstOrDefault(path => path.Targets.Any(target => target.DevicePath == Path));
        }

        public PathTarget GetPathTarget(Profile profile)
        {
            return profile.Paths.SelectMany(path => path.Targets).FirstOrDefault(target => target.DevicePath == Path);
        }

        public PathDisplayTarget GetTargetInfo()
        {
            return
                PathDisplayTarget.GetDisplayTargets()
                    .Where(target => target.DevicePath.StartsWith(Path))
                    .OrderByDescending(target => target.IsAvailable)
                    .FirstOrDefault();
        }

        public Bitmap ToBitmap(Size size, Profile profile = null)
        {
            var targetInfo = GetTargetInfo();
            var resolution = Size.Empty;

            if (targetInfo != null && targetInfo.IsAvailable)
            {
                resolution = targetInfo.PreferredResolution;
            }
            else if (profile != null)
            {
                var targetPath = GetPathSource(profile);

                if (targetPath != null)
                {
                    resolution = targetPath.Resolution;
                }
            }

            var p = new Profile {Paths = new Path[1]};
            p.Paths[0] = new Path
            {
                Resolution = resolution,
                Position = new Point(),
                Targets = new PathTarget[1]
            };
            p.Paths[0].Targets[0] = new PathTarget {DevicePath = Path};

            if (profile != null)
            {
                var targetPath = GetPathTarget(profile);

                if (targetPath != null)
                {
                    p.Paths[0].Targets[0].SurroundTopology = targetPath.SurroundTopology;
                }
            }

            return new ProfileIcon(p).ToBitmap(size.Width, size.Height);
        }
    }
}