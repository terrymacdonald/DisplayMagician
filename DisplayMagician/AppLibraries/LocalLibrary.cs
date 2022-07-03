using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.IO;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Web;
using System.Diagnostics;
using Newtonsoft.Json;
using DisplayMagician.Processes;

namespace DisplayMagician.AppLibraries
{
    public sealed class LocalLibrary : AppLibrary
    {
        #region Class Variables
        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static readonly LocalLibrary _instance = new LocalLibrary();


        // Common items to the class
        private List<App> _allLocalApps = new List<App>();
        private string GogAppIdRegex = @"/^[0-9A-F]{1,10}$";
        private string _LocalExe;
        private string _LocalPath;
        private string _gogLocalContent = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GOG.com");
        private string _gogProgramFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "GOG Galaxy");
        private bool _isLocalInstalled = false;
        private List<string> _LocalProcessList = new List<string>(){ "LocalAppServices" };

        internal string registryGogGalaxyClientKey = @"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient"; 
        internal string registryGogGalaxyClientPathKey = @"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths";
        internal string registryGogGalaxyAppsKey = @"SOFTWARE\WOW6432Node\GOG.com\Apps\";     
        //internal string registryGogLauncherKey = @"SOFTWARE\WOW6432Node\Gog";
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        // Other constants that are useful
        #endregion

        #region Class Constructors
        static LocalLibrary() { }

        private LocalLibrary()
        {
            try
            {
                logger.Trace($"LocalLibrary/LocalLibrary: Gog Online Services registry key = HKLM\\{registryGogGalaxyClientKey}");
                // Find the GogExe location, and the GogPath for later
                RegistryKey GogGalaxyClientKey = Registry.LocalMachine.OpenSubKey(registryGogGalaxyClientKey, RegistryKeyPermissionCheck.ReadSubTree);
                if (GogGalaxyClientKey == null)
                {
                    logger.Info($"LocalLibrary/LocalLibrary: GOG library is not installed!");
                    return;
                }
                string gogClientExeFilename = GogGalaxyClientKey.GetValue("clientExecutable", @"GalaxyClient.exe").ToString();

                RegistryKey GogGalaxyClientPathKey = Registry.LocalMachine.OpenSubKey(registryGogGalaxyClientPathKey, RegistryKeyPermissionCheck.ReadSubTree);
                string gogClientPath = GogGalaxyClientKey.GetValue("client", @"C:\Program Files (x86)\GOG Galaxy").ToString();
                _LocalPath = Path.GetDirectoryName(gogClientPath);
                _LocalExe = Path.Combine(gogClientPath, gogClientExeFilename);                
                if (File.Exists(_LocalExe))
                {
                    logger.Info($"LocalLibrary/LocalLibrary: GOG library is installed in {_LocalPath}. Found {_LocalExe}");
                    _isLocalInstalled = true;
                }
                else
                {
                    logger.Info($"LocalLibrary/LocalLibrary: GOG library is not installed!");
                }
                   
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "LocalLibrary/LocalLibrary: The user does not have the permissions required to read the Gog Online Services registry key.");
            }
            catch(ObjectDisposedException ex)
            {
                logger.Warn(ex, "LocalLibrary/LocalLibrary: The Microsoft.Win32.RegistryKey is closed when trying to access the Gog Online Services registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "LocalLibrary/LocalLibrary: The Gog Online Services registry key has been marked for deletion so we cannot access the value dueing the LocalLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "LocalLibrary/LocalLibrary: The user does not have the necessary registry rights to check whether Gog is installed.");
            }
        }
        #endregion

        #region Class Properties
        public override List<App> AllInstalledApps
        {
            get
            {
                // Load the Gog Apps from Gog Client if needed
                if (_allLocalApps.Count == 0)
                    LoadInstalledApps();
                return _allLocalApps;
            }
        }


        public override int InstalledAppCount
        {
            get
            {
                return _allLocalApps.Count;
            }
        }

        public override string AppLibraryName 
        { 
            get 
            {
                return "GOG";
            } 
        }

        public override SupportedAppLibraryType AppLibraryType
        {
            get
            {
                return SupportedAppLibraryType.Local;
            }
        }

        public override string AppLibraryExe
        {
            get
            {
                return _LocalExe;
            }
        }

        public override string AppLibraryPath
        {
            get
            {
                return _LocalPath;
            }
        }

        public override bool IsAppLibraryInstalled
        {
            get
            {
                return _isLocalInstalled;
            }

        }

        public override bool IsRunning
        {
            get
            {
                List<Process> LocalLibraryProcesses = new List<Process>();

                try
                {
                    foreach (string LocalLibraryProcessName in _LocalProcessList)
                    {
                        // Look for the processes with the ProcessName we sorted out earlier
                        LocalLibraryProcesses.AddRange(Process.GetProcessesByName(LocalLibraryProcessName));
                    }

                    // If we have found one or more processes then we should be good to go
                    // so let's break, and get to the next step....
                    if (LocalLibraryProcesses.Count > 0)
                        return true;
                    else
                        return false;
                }                
                catch (Exception ex) { 
                    return false; 
                }
            }

        }

        public override bool IsUpdating
        {
            get
            {
                // Not implemeted at present
                // so we just return a false
                // TODO Implement Gog specific detection for updating the App client
                return false;
            }

        }

        public override List<string> AppLibraryProcesses
        {
            get
            {
                return _LocalProcessList;
            }
        }


        #endregion

        #region Class Methods
        public static LocalLibrary GetLibrary()
        {
            return _instance;
        }


        public override bool AddApp(App LocalApp)
        {
            if (!(LocalApp is LocalApp))
                return false;
            
            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (ContainsApp(LocalApp))
            {
                logger.Debug($"LocalLibrary/AddLocalApp: Updating Local App {LocalApp.Name} in our Local library");
                // We update the existing Shortcut with the data over
                LocalApp LocalAppToUpdate = (LocalApp)GetApp(LocalApp.Id.ToString());
                LocalApp.CopyTo(LocalAppToUpdate);
            }
            else
            {
                logger.Debug($"LocalLibrary/AddLocalApp: Adding Local App {LocalApp.Name} to our Local library");
                // Add the LocalApp to the list of LocalApps
                _allLocalApps.Add(LocalApp);
            }

            //Doublecheck it's been added
            if (ContainsApp(LocalApp))
            {
                return true;
            }
            else
                return false;

        }

        public override bool RemoveApp(App LocalApp)
        {
            if (!(LocalApp is LocalApp))
                return false;

            logger.Debug($"LocalLibrary/RemoveLocalApp: Removing Local App {LocalApp.Name} from our Local library");

            // Remove the LocalApp from the list.
            int numRemoved = _allLocalApps.RemoveAll(item => item.Id.Equals(LocalApp.Id));

            if (numRemoved == 1)
            {
                logger.Debug($"LocalLibrary/RemoveLocalApp: Removed Local App with name {LocalApp.Name}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"LocalLibrary/RemoveLocalApp: Didn't remove Local App with ID {LocalApp.Name} from the Local Library");
                return false;
            }                
            else
                throw new LocalLibraryException();
        }

        public override bool RemoveAppById(string LocalAppId)
        {
            if (LocalAppId.Equals(0))
                return false;

            logger.Debug($"LocalLibrary/RemoveLocalApp2: Removing Local App with ID {LocalAppId} from the Local library");

            // Remove the LocalApp from the list.
            int numRemoved = _allLocalApps.RemoveAll(item => item.Id.Equals(LocalAppId));

            if (numRemoved == 1)
            {
                logger.Debug($"LocalLibrary/RemoveLocalApp2: Removed Local App with ID {LocalAppId}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"LocalLibrary/RemoveLocalApp2: Didn't remove Local App with ID {LocalAppId} from the Local Library");
                return false;
            }
            else
                throw new LocalLibraryException();
        }

        public override bool RemoveApp(string LocalAppNameOrId)
        {
            if (String.IsNullOrWhiteSpace(LocalAppNameOrId))
                return false;

            logger.Debug($"LocalLibrary/RemoveLocalApp3: Removing Local App with Name or ID {LocalAppNameOrId} from the Local library");

            int numRemoved;
            Match match = Regex.Match(LocalAppNameOrId, GogAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                numRemoved = _allLocalApps.RemoveAll(item => LocalAppNameOrId.Equals(item.Id));
            else
                numRemoved = _allLocalApps.RemoveAll(item => LocalAppNameOrId.Equals(item.Name));

            if (numRemoved == 1)
            {
                logger.Debug($"LocalLibrary/RemoveLocalApp3: Removed Local App with Name or UUID {LocalAppNameOrId} ");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"LocalLibrary/RemoveLocalApp3: Didn't remove Local App with Name or UUID {LocalAppNameOrId} from the Local Library");
                return false;
            }
            else
                throw new LocalLibraryException();

        }

        public override bool ContainsApp(App LocalApp)
        {
            if (!(LocalApp is LocalApp))
                return false;

            foreach (LocalApp testLocalApp in _allLocalApps)
            {
                if (testLocalApp.Id.Equals(LocalApp.Id))
                    return true;
            }

            return false;
        }

        public override bool ContainsAppById(string LocalAppId)
        {
            foreach (LocalApp testLocalApp in _allLocalApps)
            {
                if (LocalAppId == testLocalApp.Id)
                    return true;
            }


            return false;

        }

        public override bool ContainsApp(string LocalAppNameOrId)
        {
            if (String.IsNullOrWhiteSpace(LocalAppNameOrId))
                return false;


            Match match = Regex.Match(LocalAppNameOrId, GogAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (LocalApp testLocalApp in _allLocalApps)
                {
                    if (LocalAppNameOrId.Equals(Convert.ToInt32(testLocalApp.Id)))
                        return true;
                }

            }
            else
            {
                foreach (LocalApp testLocalApp in _allLocalApps)
                {
                    if (LocalAppNameOrId.Equals(testLocalApp.Name))
                        return true;
                }

            }

            return false;

        }


        public override App GetApp(string LocalAppNameOrId)
        {
            if (String.IsNullOrWhiteSpace(LocalAppNameOrId))
                return null;

            Match match = Regex.Match(LocalAppNameOrId, GogAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (LocalApp testLocalApp in _allLocalApps)
                {
                    if (LocalAppNameOrId.Equals(Convert.ToInt32(testLocalApp.Id)))
                        return testLocalApp;
                }

            }
            else
            {
                foreach (LocalApp testLocalApp in _allLocalApps)
                {
                    if (LocalAppNameOrId.Equals(testLocalApp.Name))
                        return testLocalApp;
                }

            }

            return null;

        }

        public override App GetAppById(string LocalAppId)
        {
            foreach (LocalApp testLocalApp in _allLocalApps)
            {
                if (LocalAppId == testLocalApp.Id)
                    return testLocalApp;
            }

            return null;

        }

        public override bool LoadInstalledApps()
        {
            try
            {

                if (!_isLocalInstalled)
                {
                    // Gog isn't installed, so we return an empty list.
                    logger.Info($"LocalLibrary/LoadInstalledApps: Local library is not installed");
                    return false;
                }

                string gogSupportInstallerDir = Path.Combine(_gogLocalContent, "supportInstaller");

                logger.Trace($"LocalLibrary/LoadInstalledApps: supportInstaller Directory {gogSupportInstallerDir} exists!");
                string[] gogSupportInstallerAppDirs = Directory.GetDirectories(gogSupportInstallerDir, "*", SearchOption.AllDirectories);
                logger.Trace($"LocalLibrary/LoadInstalledApps: Found App directories in supportInstaller Directory {gogSupportInstallerDir}: {gogSupportInstallerAppDirs.ToString()}");

                // If there are no Apps installed then return false
                if (gogSupportInstallerAppDirs.Length == 0)
                {
                    logger.Warn($"LocalLibrary/LoadInstalledApps: No GOG Apps installed in the GOG Galaxy library");
                    return false;
                }
                foreach (string gogSupportInstallerAppDir in gogSupportInstallerAppDirs)
                {
                    logger.Trace($"LocalLibrary/LoadInstalledApps: Parsing {gogSupportInstallerAppDir} name to find AppID");
                    Match match = Regex.Match(gogSupportInstallerAppDir, @"(\d{10})$");
                    if (!match.Success)
                    {
                        logger.Warn($"LocalLibrary/LoadInstalledApps: Failed to match the 10 digit App id from directory name {gogSupportInstallerAppDir} so ignoring App");
                        continue;
                    }

                    string AppID = match.Groups[1].Value;
                    logger.Trace($"LocalLibrary/LoadInstalledApps: Found AppID {AppID} matching pattern in App directory name");
                    string LocalAppInfoFilename = Path.Combine(gogSupportInstallerAppDir, $"LocalApp-{AppID}.info");
                    logger.Trace($"LocalLibrary/LoadInstalledApps: Looking for Apps info file {LocalAppInfoFilename}");
                    if (!File.Exists(LocalAppInfoFilename))
                    {
                        logger.Warn($"LocalLibrary/LoadInstalledApps: Couldn't find Apps info file {LocalAppInfoFilename}. There seems to be a problem with your GOG installation.");
                        continue;
                    }

                    // Now we get the information from the Gog Info file to parse it
                    LocalAppInfo LocalAppInfo;
                    try
                    {
                        LocalAppInfo = JsonConvert.DeserializeObject<LocalAppInfo>(File.ReadAllText(LocalAppInfoFilename));
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"LocalLibrary/LoadInstalledApps: Exception trying to convert the {LocalAppInfoFilename} to a JSON object to read the installed Apps. There seems to be a problem with your GOG installation.");
                        continue;
                    }

                    // Now we check this is a 'Root App' i.e. it is a  base App, not something else
                    if (LocalAppInfo.AppId != LocalAppInfo.rootAppId)
                    {
                        logger.Trace($"LocalLibrary/LoadInstalledApps: App {LocalAppInfo.name} is not a base App (probably DLC) so we're skipping it.");
                    }

                    // Now we check the Gog App registry key too, to get some more information that we need
                    string registryGogGalaxyAppKey = registryGogGalaxyAppsKey + LocalAppInfo.AppId;
                    logger.Trace($"LocalLibrary/LocalLibrary: GOG Galaxy Apps registry key = HKLM\\{registryGogGalaxyAppKey}");
                    RegistryKey GogGalaxyAppKey = Registry.LocalMachine.OpenSubKey(registryGogGalaxyAppKey, RegistryKeyPermissionCheck.ReadSubTree);
                    if (GogGalaxyAppKey == null)
                    {
                        logger.Info($"LocalLibrary/LocalLibrary: Could not find the GOG Galaxy Apps registry key {registryGogGalaxyAppsKey} so can't get all the information about the App we need! There seems to be a problem with your GOG installation.");
                        continue;
                    }

                    string AppDirectory = GogGalaxyAppKey.GetValue("path", "").ToString();
                    string AppExePath = GogGalaxyAppKey.GetValue("exe", "").ToString();
                    if (!File.Exists(AppExePath))
                    {
                        logger.Info($"LocalLibrary/LocalLibrary: Could not find the GOG Galaxy App file {AppExePath} so can't run the App later! There seems to be a problem with your GOG installation.");
                        continue;
                    }
                    /*string AppIconPath = Path.Combine(AppDirectory, $"LocalApp-{AppID}.ico");                    
                    if (!File.Exists(AppIconPath))
                    {
                        AppIconPath = AppExePath;
                    }*/

                    // Extract the info into a App object                    
                    LocalApp LocalApp = new LocalApp();
                    LocalApp.Id = LocalAppInfo.AppId;
                    LocalApp.Name = LocalAppInfo.name;
                    LocalApp.Directory = AppDirectory;
                    LocalApp.Executable = GogGalaxyAppKey.GetValue("exeFile", "").ToString();
                    LocalApp.ExePath = AppExePath;
                    //LocalApp.IconPath = AppIconPath;
                    LocalApp.IconPath = AppExePath;
                    LocalApp.ProcessName = Path.GetFileNameWithoutExtension(LocalApp.ExePath);

                    // Add the Gog App to the list of Gog Apps
                    _allLocalApps.Add(LocalApp);
                }

                logger.Info($"LocalLibrary/LoadInstalledApps: Found {_allLocalApps.Count} installed GOG Apps");

            }

            catch (ArgumentNullException ex)
            {
                logger.Warn(ex, "LocalLibrary/GetAllInstalledApps: An argument supplied to the function is null.");
            }
            catch (NotSupportedException ex)
            {
                logger.Warn(ex, "LocalLibrary/GetAllInstalledApps: The invoked method is not supported or reading, seeking or writing to a stream that isn't supported.");
            }
            catch (PathTooLongException ex)
            {
                logger.Warn(ex, "LocalLibrary/GetAllInstalledApps: The path is longer than the maximum allowed by the operating system.");
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "LocalLibrary/GetAllInstalledApps: The user does not have the permissions required to read the GOG InstallDir registry key.");
            }
            catch (ObjectDisposedException ex)
            {
                logger.Warn(ex, "LocalLibrary/GetAllInstalledApps: The Microsoft.Win32.RegistryKey is closed when trying to access the GOG InstallDir registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "LocalLibrary/GetAllInstalledApps: The GOG InstallDir registry key has been marked for deletion so we cannot access the value dueing the LocalLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "LocalLibrary/GetAllInstalledApps: The user does not have the necessary registry rights to check whether Gog is installed.");
            }

            return true;
        }


        public override List<Process> StartApp(App App, string AppArguments = "", ProcessPriority processPriority = ProcessPriority.Normal)
        {
            string args = $@"/command=runApp /AppId={App.Id} /path=""{App.Directory}""";
            if (String.IsNullOrWhiteSpace(AppArguments))
            {
                args += AppArguments;
            }
            List<Process> AppProcesses = ProcessUtils.StartProcess(_LocalExe, args, processPriority);
            return AppProcesses;
        }

        
        #endregion

    }

    public class LocalPlayTask
    {
        public string category;
        public string compatibilityFlags;
        public bool isPrimary;
        public List<string> languages;
        public string name;
        public string path;
        public string type;
    }
    public class LocalAppInfo
    {
        public string buildId;
        public string clientId;
        public string AppId;
        public string language;
        public List<string> languages;
        public string name;
        public List<LocalPlayTask> playTasks;
        public string rootAppId;
        public int version;
    }

    [global::System.Serializable]
    public class LocalLibraryException : AppLibraryException
    {
        public LocalLibraryException() { }
        public LocalLibraryException(string message) : base(message) { }
        public LocalLibraryException(string message, Exception inner) : base(message, inner) { }
        protected LocalLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
