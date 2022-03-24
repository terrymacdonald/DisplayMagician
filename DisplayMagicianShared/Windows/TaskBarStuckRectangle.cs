using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

// This file is taken from Soroush Falahati's amazing HeliosDisplayManagement software
// available at https://github.com/falahati/HeliosDisplayManagement

// Modifications made by Terry MacDonald

namespace DisplayMagicianShared.Windows
{
    public class TaskBarSettings
    {
        private const string AdvancedSettingsAddress =
            "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";

        private static List<string> WantedAdvancedSettingValues = new List<string>
        {
            // Win10/11 registry keys (not all will be populated, only those that the user modified from default at least once)
            "MMTaskbarEnabled", //  Multiple Taskbars:  0 for show taskbar on main screen only, 1 for show taskbar on all screens            
            "MMTaskbarMode", // Show taskbar buttons on: 0 = all taskbars, 1 = main taskbar and where windows is open, 2 = taskbar where window is open
            "MMTaskbarGlomLevel", // Buttons on other taskbars: 0 = always combine, combine when the taskbar is full, 2 = never combine
            "NoTaskGrouping", // Disable all Task Grouping (overrides "TaskbarGroupSize"): 0 = enable task grouping, 1 = disable task grouping
            "SearchboxTaskbarMode", // Show Search Button in Taskbar: 0 = remove search button, 1 = show search button
            "ShowTaskViewButton", // Show Taskview Button in Taskbar: 0 = remove taskview button, 1 = show taskview button
            "TaskbarAl", // Start Button Alignment: 0 for left, 1 for center, 
            "TaskbarDa", // Show Widgets button in Taskbar: 0 = remove widgets button, 1 = Show widgets button
            "TaskbarGlomLevel", // Buttons on main screen: 0 = always combine, combine when the taskbar is full, 2 = never combine
            "TaskbarGroupSize", // TaskBar left/right grouping by age: 0 = oldest first (default), 1 = roup by size largest first, 2 = group all with 2 or more, 3 = group all with 3 or more (see NoTaskGrouping to prevent Grouping )
            "TaskbarMn", // Show Chat Button in Taskbar: 0 = remove chat button, 1 = show chat button
            "TaskbarSi", // Taskbar Size: 0 = small, 1 = medium, 2 = Large
            "TaskbarSizeMove", // Lock the Taskbar (prevent resizing): 0 = taskbar size is locked, 1 = taskbar size is unlocked
            "TaskbarSmallIcons", // Small Taskbar Icons: 0 = normal sized icons, 1 = small icons
            "TaskbarSd", // Show Desktop Button in Taskbar: 0 for hid the show desktop button, 1 for show the Show desktop button
        };


        public Tuple<string, int>[] Options { get; set; }

        public override bool Equals(object obj) => obj is TaskBarSettings other && this.Equals(other);
        public bool Equals(TaskBarSettings other)
        => Options.All(a => other.Options.Any(x => x.Item1 == a.Item1 && x.Item2 == a.Item2));

        public override int GetHashCode()
        {
            return (Options).GetHashCode();
        }
        public static bool operator ==(TaskBarSettings lhs, TaskBarSettings rhs) => lhs.Equals(rhs);

        public static bool operator !=(TaskBarSettings lhs, TaskBarSettings rhs) => !(lhs == rhs);

        public static TaskBarSettings GetCurrent()
        {
            var taskBarOptions = new List<Tuple<string, int>>();

            // Get modified and stored Taskbar options from the User Registry
            // Note: Only the taskbar options changed from default at least once in the past will be listed in Registry
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    AdvancedSettingsAddress,
                    RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (key != null)
                    {
                        foreach (var valueName in WantedAdvancedSettingValues)
                        {
                            try
                            {

                                var value = key.GetValue(valueName, null,
                                    RegistryValueOptions.DoNotExpandEnvironmentNames);

                                if (value != null && value is int intValue)
                                {
                                    taskBarOptions.Add(new Tuple<string, int>(valueName, intValue));
                                }
                            }
                            catch (Exception)
                            {
                                // ignored, as this will happen
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (taskBarOptions.Count == 0)
            {
                return null;
            }

            return new TaskBarSettings
            {
                Options = taskBarOptions.ToArray()
            };
        }

        public bool Apply()
        {
            if (Options.Length == 0)
            {
                throw new InvalidOperationException();
            }

            using (var optionsKey = Registry.CurrentUser.OpenSubKey(
                AdvancedSettingsAddress,
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (optionsKey == null)
                {
                    return false;
                }

                // Write
                foreach (var option in Options)
                {
                    try
                    {
                        optionsKey.SetValue(option.Item1, option.Item2);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            return true;
        }
    }

    [global::System.Serializable]
    public class TaskBarStuckRectangleException : Exception
    {
        public TaskBarStuckRectangleException() { }
        public TaskBarStuckRectangleException(string message) : base(message) { }
        public TaskBarStuckRectangleException(string message, Exception inner) : base(message, inner) { }
        protected TaskBarStuckRectangleException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}