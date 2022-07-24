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
using IWshRuntimeLibrary;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;
using Windows.ApplicationModel.Core;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Web;
//using DisplayMagician.GameLibraries.SteamAppInfoParser;

namespace DisplayMagician.AppLibraries
{
    class InstalledProgram
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string Path { get; set; }
        public string Arguments { get; set; }
        public string IconPath { get; set; }
        //public int IconIndex { get; set; }
        public string WorkDir { get; set; }
        public string Name { get; set; }
        public string AppId { get; set; }

        public ShortcutBitmap Logo { get; set; }

        public List<ShortcutBitmap> AllLogos { get; set; }

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
                    IconPath = file.FullName,
                    WorkDir = System.IO.Path.GetDirectoryName(file.FullName),
                    Name = programName,
                    AppId = $"FromProgramData_{programName}",
                    Arguments = ""
                };
            }
            else if (file.Extension?.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) == true)
            {
                var data = GetLnkShortcutData(file.FullName);
                var name = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                if (System.IO.File.Exists(data.Path))
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(data.Path);
                    name = !string.IsNullOrEmpty(versionInfo.ProductName?.Trim()) ? versionInfo.ProductName : System.IO.Path.GetFileNameWithoutExtension(file.FullName);
                }

                var program = new InstalledProgram
                {
                    Path = data.Path,
                    WorkDir = data.WorkDir,
                    Arguments = data.Arguments,
                    Name = name,
                    AppId = $"FromProgramData_{name}"
                };

                if (!String.IsNullOrEmpty(data.IconPath))
                {
                    var reg = Regex.Match(data.IconPath, @"^(.+),(\d+)$");
                    if (reg.Success)
                    {
                        program.IconPath = reg.Groups[1].Value;
                        //program.IconIndex = int.Parse(reg.Groups[2].Value);
                    }
                    else
                    {
                        program.IconPath = data.IconPath;
                    }
                }
                else
                {
                    program.IconPath = data.Path;
                }

                return program;
            }
            else if (file.Extension?.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) == true)
            {
                return new InstalledProgram
                {
                    Path = file.FullName,
                    Name = System.IO.Path.GetFileNameWithoutExtension(file.FullName),
                    WorkDir = System.IO.Path.GetDirectoryName(file.FullName),
                    AppId = $"FromProgramData_{System.IO.Path.GetFileNameWithoutExtension(file.FullName)}",
                    Arguments = "",
                    IconPath = file.FullName
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
            string iconLocation;
            if (!String.IsNullOrWhiteSpace(link.IconLocation))
            {
                if (Regex.IsMatch(link.IconLocation, @"^,\d+$"))
                {
                    // This is an empty shortcut path, so we need to use the target path instead
                    iconLocation = link.TargetPath;
                }
                else if (Regex.IsMatch(link.IconLocation, @",\d+$"))
                {
                    // This is a shortcut path, so we need to remove the icon number from the end
                    Match myMatches = Regex.Match(link.IconLocation, @"^(.*?),\d+$");
                    if (myMatches.Success)
                    {
                        iconLocation = myMatches.Groups[1].Value;
                    }
                    else
                    {
                        iconLocation = link.TargetPath;
                    }
                }
                else
                {
                    iconLocation = link.IconLocation;
                }
            }
            else
            {
                iconLocation = link.TargetPath;
            }


            return new InstalledProgram()
            {
                Path = link.TargetPath,
                IconPath = iconLocation,
                Arguments = link.Arguments,
                WorkDir = link.WorkingDirectory,
                Name = link.FullName,
                AppId = $"FromLink_{link.FullName}"
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
                    // Finish if this task is cancelled
                    if (cancelToken?.IsCancellationRequested == true)
                    {
                        return null;
                    }

                    // Skip this shortcut if it's a directory
                    if (shortcut.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        continue;
                    }

                    // Get the link filename
                    var fileName = shortcut.Name;
                    var Directory = System.IO.Path.GetDirectoryName(shortcut.FullName);

                    // Skip the folders we want to ignore
                    if (folderExceptions.FirstOrDefault(a => shortcut.FullName.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0) != null)
                    {
                        continue;
                    }

                    // Parse the link file to get access to the settings in it.
                    var link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcut.FullName);
                    var target = link.TargetPath;

                    // Skip the paths we don't want to process.
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

                    string iconLocation;
                    if (!String.IsNullOrWhiteSpace(link.IconLocation))
                    {
                        if (Regex.IsMatch(link.IconLocation, @"^,\d+$"))
                        {
                            // This is an empty shortcut path, so we need to use the target path instead
                            iconLocation = link.TargetPath;
                        }
                        else if (Regex.IsMatch(link.IconLocation, @",\d+$"))
                        {
                            // This is a shortcut path, so we need to remove the icon number from the end
                            Match myMatches = Regex.Match(link.IconLocation, @"^(.*?),\d+$");
                            if (myMatches.Success)
                            {
                                iconLocation = myMatches.Groups[1].Value;
                            }
                            else
                            {
                                iconLocation = link.TargetPath;
                            }
                        }
                        else
                        {
                            if (System.IO.File.Exists(link.IconLocation))
                            {
                                iconLocation = link.IconLocation;
                            }                            
                            else
                            {
                                iconLocation = link.TargetPath;
                            }
                        }
                    }
                    else
                    {
                        iconLocation = link.TargetPath;
                    }

                    string workingDir = link.WorkingDirectory;
                    if (link.WorkingDirectory == null || String.IsNullOrWhiteSpace(link.WorkingDirectory) || !System.IO.File.Exists(link.WorkingDirectory))
                    {
                        workingDir = System.IO.Path.GetDirectoryName(target);
                    }

                    List<ShortcutBitmap> allLogos = ImageUtils.GetMeAllBitmapsFromFile(DecodeIndirectFolders(iconLocation));

                    var app = new InstalledProgram()
                    {
                        Path = DecodeIndirectFolders(target),
                        IconPath = DecodeIndirectFolders(iconLocation),
                        Name = System.IO.Path.GetFileNameWithoutExtension(shortcut.Name),
                        WorkDir = DecodeIndirectFolders(workingDir),
                        AppId = $"FromFolder_{System.IO.Path.GetFileNameWithoutExtension(shortcut.Name)}",
                        Logo = ImageUtils.GetMeLargestAvailableBitmap(allLogos),
                        AllLogos = allLogos,
                        Arguments = ""
                    };

                    apps.Add(app);
                }

                return apps;
            });
        }

        public static async Task<List<InstalledProgram>> GetInstalledProgramsAsync(CancellationTokenSource cancelToken = null)
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
            if (System.IO.File.Exists(defPath))
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

                    bool worked = true;
                    try
                    {
                        IReadOnlyList<AppListEntry> applListEntries = (IReadOnlyList<AppListEntry>)package.GetAppListEntries();
                        if (applListEntries.Count == 0)
                        {
                            continue;
                        }
                        string name = "";
                        string aumi = "";

                        var entry = applListEntries[0];
                        aumi = entry.AppUserModelId;                        
                        name = entry.DisplayInfo.DisplayName;
                        ShortcutBitmap bitmap = new ShortcutBitmap();
                        try 
                        {
                            var logoStream = entry.DisplayInfo.GetLogo(new Windows.Foundation.Size(150, 150));
                            var bitmapImage = new BitmapImage();
                            using (var randomAccessStream = logoStream.OpenReadAsync().GetResults())
                            using (var stream = randomAccessStream.AsStream())
                            {
                                bitmapImage.BeginInit();
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.StreamSource = stream;
                                bitmapImage.EndInit();
                            }
                            bitmap.Image = ImageUtils.BitmapImage2Bitmap(bitmapImage);
                            bitmap.Source = package.Logo.LocalPath;
                            bitmap.Size = new Size(bitmap.Image.Width, bitmap.Image.Height);
                            bitmap.Order = 0;
                        }
                        catch (Exception ex2)
                        {
                            bitmap.Image = new Bitmap(package.Logo.LocalPath);
                            bitmap.Source = package.Logo.LocalPath;
                            bitmap.Size = new Size(bitmap.Image.Width, bitmap.Image.Height);   
                            bitmap.Order = 0;
                        }
                        var windowsDirectoryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows);

                        List<ShortcutBitmap> allLogos = new List<ShortcutBitmap>();
                        allLogos.Add(bitmap);

                        var app = new InstalledProgram()
                        {
                            Name = Utils.NormaliseGameName(name),
                            WorkDir = package.InstalledLocation.Path,
                            Path = $"{windowsDirectoryPath}\\explorer.exe",
                            Arguments = $"shell:AppsFolder\\{aumi}",
                            IconPath = package.Logo.LocalPath,
                            Logo = bitmap,
                            AllLogos = allLogos,
                            AppId = package.Id.FamilyName
                        };

                        apps.Add(app);
                    }
                    catch (Exception ex)
                    {
                        worked = false;
                    }

                    if (!worked)
                    {
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
                                name = Utils.GetIndirectResourceString(package.Id.FullName, package.Id.Name, name);
                                if (String.IsNullOrEmpty(name))
                                {
                                    name = manifest.SelectSingleNode(@"/*[local-name() = 'Package']/*[local-name() = 'Identity']").Attributes["Name"].Value;
                                }
                            }

                            var windowsUWPPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows);

                            ShortcutBitmap bitmap = new ShortcutBitmap();
                            bitmap.Image = new Bitmap(package.Logo.LocalPath);
                            bitmap.Source = package.Logo.LocalPath;
                            bitmap.Size = new Size(bitmap.Image.Width, bitmap.Image.Height);
                            bitmap.Order = 0;

                            List<ShortcutBitmap> allLogos = new List<ShortcutBitmap>();
                            allLogos.Add(bitmap);

                            var app = new InstalledProgram()
                            {
                                Name = Utils.NormaliseGameName(name),
                                WorkDir = package.InstalledLocation.Path,
                                Path = $"{windowsUWPPath}\\explorer.exe",
                                Arguments = $"shell:AppsFolder\\{package.Id.FamilyName}!{appId}",
                                IconPath = iconPath,
                                Logo = bitmap,
                                AllLogos = allLogos,
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
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to get list of installed UWP apps.");
            }

            return apps;
        }

        public static string DecodeIndirectFolders(string indirectPath)
        {
            if (Regex.IsMatch(indirectPath, @"\%([^\\])\%"))
            {
                // This is a special folder variable, so we need to decode it
                Match myMatches = Regex.Match(indirectPath, @"\%(.+?)\%");
                if (myMatches.Success)
                {
                    string specialVariable = myMatches.Groups[1].Value;
                    string replacement = "";
                    if (specialVariable.Equals("ProgramFiles",StringComparison.CurrentCultureIgnoreCase))
                    {
                        replacement = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                        indirectPath.Replace("%ProgramFiles%", replacement);
                    }
                    else if (specialVariable.Equals("ProgramFilesX86", StringComparison.CurrentCultureIgnoreCase))
                    {
                        replacement = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                        indirectPath.Replace("%ProgramFilesX86%", replacement);
                    }
                    else if (specialVariable.Equals("ProgramFiles(x86)", StringComparison.CurrentCultureIgnoreCase))
                    {
                        replacement = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                        indirectPath.Replace("%ProgramFiles(x86)%", replacement);
                    }
                    else if (specialVariable.Equals("CommonProgramFiles", StringComparison.CurrentCultureIgnoreCase))
                    {
                        replacement = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
                        indirectPath.Replace("%CommonProgramFiles%", replacement);
                    }
                    else if (specialVariable.Equals("CommonProgramFilesX86", StringComparison.CurrentCultureIgnoreCase))
                    {
                        replacement = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86);
                        indirectPath.Replace("%CommonProgramFilesX86%", replacement);
                    }
                    else if (specialVariable.Equals("CommonProgramFiles(x86)", StringComparison.CurrentCultureIgnoreCase))
                    {
                        replacement = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86);
                        indirectPath.Replace("%CommonProgramFiles(x86)%", replacement);
                    }
                }
            }
            return indirectPath;
        }

    }
}

