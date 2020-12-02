using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace DisplayMagician.Shared
{
    public class TaskBarSettings
    {
        private const string AdvancedSettingsAddress =
            "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";

        public Tuple<string, int>[] Options { get; set; }

        public TaskBarStuckRectangle SingleMonitorStuckRectangle { get; set; }

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
                Options = taskBarOptions.ToArray(),
                SingleMonitorStuckRectangle = TaskBarStuckRectangle.GetCurrent()
            };
        }

        public bool Apply()
        {
            if (SingleMonitorStuckRectangle == null ||
                Options.Length == 0)
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