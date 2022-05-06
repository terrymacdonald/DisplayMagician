using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace DisplayMagician
{
    class InstalledProgram
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string Path { get; set; }
        public string Arguments { get; set; }
        public string Icon { get; set; }
        public int IconIndex { get; set; }
        public string WorkDir { get; set; }
        public string Name { get; set; }
        public string AppId { get; set; }

        public override string ToString()
        {
            return Name;
        }


        public static async Task<List<InstalledProgram>> GetExecutablesFromFolder(string path, SearchOption searchOption, CancellationTokenSource cancelToken = null)
        {
            return await Task.Run(() =>
            {
                var execs = new List<InstalledProgram>();
                var files = new SafeFileEnumerator(path, "*.*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    if (cancelToken?.IsCancellationRequested == true)
                    {
                        return null;
                    }

                    if (file.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        continue;
                    }

                    if (UninstallProgram.IsFileUninstaller(file.Name))
                    {
                        continue;
                    }

                    if (String.IsNullOrEmpty(file.Extension))
                    {
                        continue;
                    }

                    if (file.Extension.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true ||
                        file.Extension.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) == true ||
                        file.Extension.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        execs.Add(GetProgramData(file.FullName));
                    }
                }

                return execs;
            });
        }

        public static InstalledProgram GetProgramData(string filePath)
        {
            var file = new FileInfo(filePath);
            if (file.Extension?.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true)
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(file.FullName);
                var programName = !string.IsNullOrEmpty(versionInfo.ProductName?.Trim()) ? versionInfo.ProductName : new DirectoryInfo(System.IO.Path.GetDirectoryName(file.FullName)).Name;
                return new InstalledProgram
                {
                    Path = file.FullName,
                    Icon = file.FullName,
                    WorkDir = System.IO.Path.GetDirectoryName(file.FullName),
                    Name = programName
                };
            }
            else if (file.Extension?.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) == true)
            {
                var data = GetLnkShortcutData(file.FullName);
                var name = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                if (File.Exists(data.Path))
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(data.Path);
                    name = !string.IsNullOrEmpty(versionInfo.ProductName?.Trim()) ? versionInfo.ProductName : System.IO.Path.GetFileNameWithoutExtension(file.FullName);
                }

                var program = new InstalledProgram
                {
                    Path = data.Path,
                    WorkDir = data.WorkDir,
                    Arguments = data.Arguments,
                    Name = name
                };

                if (!String.IsNullOrEmpty(data.Icon))
                {
                    var reg = Regex.Match(data.Icon, @"^(.+),(\d+)$");
                    if (reg.Success)
                    {
                        program.Icon = reg.Groups[1].Value;
                        program.IconIndex = int.Parse(reg.Groups[2].Value);
                    }
                    else
                    {
                        program.Icon = data.Icon;
                    }
                }
                else
                {
                    program.Icon = data.Path;
                }

                return program;
            }
            else if (file.Extension?.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) == true)
            {
                return new InstalledProgram
                {
                    Path = file.FullName,
                    Name = System.IO.Path.GetFileNameWithoutExtension(file.FullName),
                    WorkDir = System.IO.Path.GetDirectoryName(file.FullName)
                };
            }

            throw new NotSupportedException("Only exe, bat and lnk files are supported.");
        }

        public static void CreateShortcut(string executablePath, string arguments, string iconPath, string shortcutPath)
        {
            var shell = new IWshRuntimeLibrary.WshShell();
            var link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
            link.TargetPath = executablePath;
            link.WorkingDirectory = System.IO.Path.GetDirectoryName(executablePath);
            link.Arguments = arguments;
            link.IconLocation = string.IsNullOrEmpty(iconPath) ? executablePath + ",0" : iconPath;
            link.Save();
        }

        public static InstalledProgram GetLnkShortcutData(string lnkPath)
        {
            var shell = new IWshRuntimeLibrary.WshShell();
            var link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(lnkPath);
            return new InstalledProgram()
            {
                Path = link.TargetPath,
                Icon = link.IconLocation == ",0" ? link.TargetPath : link.IconLocation,
                Arguments = link.Arguments,
                WorkDir = link.WorkingDirectory,
                Name = link.FullName
            };
        }
        public static async Task<List<InstalledProgram>> GetShortcutProgramsFromFolder(string path, CancellationTokenSource cancelToken = null)
        {
            return await Task.Run(() =>
            {
                var folderExceptions = new string[]
                {
                    @"\Accessibility\",
                    @"\Accessories\",
                    @"\Administrative Tools\",
                    @"\Maintenance\",
                    @"\StartUp\",
                    @"\Windows ",
                    @"\Microsoft ",
                };

                var pathExceptions = new string[]
                {
                    @"\system32\",
                    @"\windows\",
                };

                var shell = new IWshRuntimeLibrary.WshShell();
                var apps = new List<InstalledProgram>();
                var shortucts = new SafeFileEnumerator(path, "*.lnk", SearchOption.AllDirectories);

                foreach (var shortcut in shortucts)
                {
                    if (cancelToken?.IsCancellationRequested == true)
                    {
                        return null;
                    }

                    if (shortcut.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        continue;
                    }

                    var fileName = shortcut.Name;
                    var Directory = System.IO.Path.GetDirectoryName(shortcut.FullName);

                    if (folderExceptions.FirstOrDefault(a => shortcut.FullName.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0) != null)
                    {
                        continue;
                    }

                    var link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcut.FullName);
                    var target = link.TargetPath;

                    if (pathExceptions.FirstOrDefault(a => target.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0) != null)
                    {
                        continue;
                    }

                    // Ignore uninstallers
                    if (UninstallProgram.IsFileUninstaller(System.IO.Path.GetFileName(target)))
                    {
                        continue;
                    }

                    // Ignore duplicates
                    if (apps.FirstOrDefault(a => a.Path == target) != null)
                    {
                        continue;
                    }

                    // Ignore non-application links
                    if (System.IO.Path.GetExtension(target) != ".exe")
                    {
                        continue;
                    }

                    var app = new InstalledProgram()
                    {
                        Path = target,
                        Icon = link.IconLocation,
                        Name = System.IO.Path.GetFileNameWithoutExtension(shortcut.Name),
                        WorkDir = link.WorkingDirectory
                    };

                    apps.Add(app);
                }

                return apps;
            });
        }

        public static async Task<List<InstalledProgram>> GetInstalledPrograms(CancellationTokenSource cancelToken = null)
        {
            var apps = new List<InstalledProgram>();

            // Get apps from All Users
            var allPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");
            var allApps = await GetShortcutProgramsFromFolder(allPath);
            if (cancelToken?.IsCancellationRequested == true)
            {
                return null;
            }
            else
            {
                apps.AddRange(allApps);
            }

            // Get current user apps
            var userPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
            var userApps = await GetShortcutProgramsFromFolder(userPath);
            if (cancelToken?.IsCancellationRequested == true)
            {
                return null;
            }
            else
            {
                apps.AddRange(userApps);
            }

            return apps;
        }

        private static string GetUWPGameIcon(string defPath)
        {
            if (File.Exists(defPath))
            {
                return defPath;
            }

            var folder = System.IO.Path.GetDirectoryName(defPath);
            var fileMask = System.IO.Path.GetFileNameWithoutExtension(defPath) + ".scale*.png";
            var files = Directory.GetFiles(folder, fileMask);

            if (files == null || files.Count() == 0)
            {
                return string.Empty;
            }
            else
            {
                var icons = files.Where(a => Regex.IsMatch(a, @"\.scale-\d+\.png"));
                if (icons.Any())
                {
                    return icons.OrderBy(a => a).Last();
                }

                return string.Empty;
            }
        }

        public static List<InstalledProgram> GetUWPApps()
        {
            var apps = new List<InstalledProgram>();

            try
            {
                var manager = new PackageManager();
                IEnumerable<Package> packages = manager.FindPackagesForUser(WindowsIdentity.GetCurrent().User.Value);
                foreach (var package in packages)
                {
                    if (package.IsFramework || package.IsResourcePackage || package.SignatureKind != PackageSignatureKind.Store)
                    {
                        continue;
                    }

                    try
                    {
                        if (package.InstalledLocation == null)
                        {
                            continue;
                        }
                    }
                    catch
                    {
                        // InstalledLocation accessor may throw Win32 exception for unknown reason
                        continue;
                    }

                    try
                    {
                        string manifestPath;
                        if (package.IsBundle)
                        {
                            manifestPath = @"AppxMetadata\AppxBundleManifest.xml";
                        }
                        else
                        {
                            manifestPath = "AppxManifest.xml";
                        }

                        manifestPath = System.IO.Path.Combine(package.InstalledLocation.Path, manifestPath);
                        var manifest = new XmlDocument();
                        manifest.Load(manifestPath);

                        var apxApp = manifest.SelectSingleNode(@"/*[local-name() = 'Package']/*[local-name() = 'Applications']//*[local-name() = 'Application'][1]");
                        var appId = apxApp.Attributes["Id"].Value;

                        var visuals = apxApp.SelectSingleNode(@"//*[local-name() = 'VisualElements']");
                        var iconPath = visuals.Attributes["Square150x150Logo"]?.Value;
                        if (String.IsNullOrEmpty(iconPath))
                        {
                            iconPath = visuals.Attributes["Square70x70Logo"]?.Value;
                            if (String.IsNullOrEmpty(iconPath))
                            {
                                iconPath = visuals.Attributes["Square44x44Logo"]?.Value;
                                if (String.IsNullOrEmpty(iconPath))
                                {
                                    iconPath = visuals.Attributes["Logo"]?.Value;
                                }
                            }
                        }

                        if (!String.IsNullOrEmpty(iconPath))
                        {
                            iconPath = System.IO.Path.Combine(package.InstalledLocation.Path, iconPath);
                            iconPath = GetUWPGameIcon(iconPath);
                        }

                        var name = manifest.SelectSingleNode(@"/*[local-name() = 'Package']/*[local-name() = 'Properties']/*[local-name() = 'DisplayName']").InnerText;
                        if (name.StartsWith("ms-resource"))
                        {
                            name = MsResources.GetIndirectResourceString(package.Id.FullName, package.Id.Name, name);
                            if (String.IsNullOrEmpty(name))
                            {
                                name = manifest.SelectSingleNode(@"/*[local-name() = 'Package']/*[local-name() = 'Identity']").Attributes["Name"].Value;
                            }
                        }

                        var app = new InstalledProgram()
                        {
                            Name = Utils.NormaliseGameName(name),
                            WorkDir = package.InstalledLocation.Path,
                            Path = "explorer.exe",
                            Arguments = $"shell:AppsFolder\\{package.Id.FamilyName}!{appId}",
                            Icon = iconPath,
                            AppId = package.Id.FamilyName
                        };

                        apps.Add(app);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, $"Failed to parse UWP game info.");
                    }
                }
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to get list of installed UWP apps.");
            }

            return apps;
        }



    }
}
