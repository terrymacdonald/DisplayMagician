using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace DisplayMagician
{
    class UninstallProgram
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string DisplayIcon { get; set; }
        public string DisplayName { get; set; }
        public string DisplayVersion { get; set; }
        public string InstallLocation { get; set; }
        public string Publisher { get; set; }
        public string UninstallString { get; set; }
        public string URLInfoAbout { get; set; }
        public string RegistryKeyName { get; set; }
        public string Path { get; set; }

        public override string ToString()
        {
            return DisplayName ?? RegistryKeyName;
        }

        private static readonly string[] uninstallerMasks = new string[]
        {
            "uninst",
            "setup",
            @"unins\d+"
        };

        
        public static bool IsFileUninstaller(string path)
        {
            return uninstallerMasks.Any(a => Regex.IsMatch(path, a, RegexOptions.IgnoreCase));
        }
    

        private static List<UninstallProgram> GetUninstallProgsFromView(RegistryView view)
        {
            var rootString = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            void SearchRoot(RegistryHive hive, List<UninstallProgram> programs)
            {
                using (var root = RegistryKey.OpenBaseKey(hive, view))
                {
                    var keyList = root.OpenSubKey(rootString);
                    if (keyList == null)
                    {
                        return;
                    }

                    foreach (var key in keyList.GetSubKeyNames())
                    {
                        try
                        {
                            using (var prog = root.OpenSubKey(rootString + key))
                            {
                                if (prog == null)
                                {
                                    continue;
                                }

                                var program = new UninstallProgram()
                                {
                                    DisplayIcon = prog.GetValue("DisplayIcon")?.ToString(),
                                    DisplayVersion = prog.GetValue("DisplayVersion")?.ToString(),
                                    DisplayName = prog.GetValue("DisplayName")?.ToString(),
                                    InstallLocation = prog.GetValue("InstallLocation")?.ToString(),
                                    Publisher = prog.GetValue("Publisher")?.ToString(),
                                    UninstallString = prog.GetValue("UninstallString")?.ToString(),
                                    URLInfoAbout = prog.GetValue("URLInfoAbout")?.ToString(),
                                    Path = prog.GetValue("Path")?.ToString(),
                                    RegistryKeyName = key
                                };

                                programs.Add(program);
                            }
                        }
                        catch (System.Security.SecurityException ex)
                        {
                            logger.Warn(ex, $"UninstallProgram/GetUninstallProgsFromView: Failed to read registry key {rootString + key}");
                        }
                    }
                }
            }

            var progs = new List<UninstallProgram>();
            SearchRoot(RegistryHive.LocalMachine, progs);
            SearchRoot(RegistryHive.CurrentUser, progs);
            return progs;
        }

        public static List<UninstallProgram> GetUnistallProgramsList()
        {
            var progs = new List<UninstallProgram>();

            if (Environment.Is64BitOperatingSystem)
            {
                progs.AddRange(GetUninstallProgsFromView(RegistryView.Registry64));
            }

            progs.AddRange(GetUninstallProgsFromView(RegistryView.Registry32));
            return progs;
        }

    }
}
