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

            // Get stored integer Taskbar options from the User Registry
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    AdvancedSettingsAddress,
                    RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(valueName) && valueName.ToLower().Contains("taskbar"))
                                {
                                    var value = key.GetValue(valueName, null,
                                        RegistryValueOptions.DoNotExpandEnvironmentNames);

                                    if (value != null && value is int intValue)
                                    {
                                        taskBarOptions.Add(new Tuple<string, int>(valueName, intValue));
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
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
}
