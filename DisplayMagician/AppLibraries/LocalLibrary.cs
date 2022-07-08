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
using System.Threading.Tasks;

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
        private string _LocalExe;
        private string _LocalPath;
        private bool _isLocalInstalled = false;
        private List<string> _LocalProcessList = new List<string>(){ "LocalAppServices" };
        private string LocalAppIdRegex = @"/^[0-9A-F]{1,10}$";

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        // Other constants that are useful
        #endregion

        #region Class Constructors
        static LocalLibrary() { }

        private LocalLibrary()
        {
            _isLocalInstalled = true;
        }
        #endregion

        #region Class Properties
        public override List<App> AllInstalledApps
        {
            get
            {
                // Disabled as we now do it manually when DM starts
                // Load the Locally Installed Apps from Windows if needed
                /*if (_allLocalApps.Count == 0)
                    LoadInstalledApps();*/
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
                return "Locally Installed";
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
                // As this library accesses all the installed apps within Windows, it is always installed.
                return true;
            }

        }

        public override bool IsRunning
        {
            get
            {
                // As this 'library' is  Windows, it is always running.
                return true;                
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
            Match match = Regex.Match(LocalAppNameOrId, LocalAppIdRegex, RegexOptions.IgnoreCase);
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


            Match match = Regex.Match(LocalAppNameOrId, LocalAppIdRegex, RegexOptions.IgnoreCase);
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

            Match match = Regex.Match(LocalAppNameOrId, LocalAppIdRegex, RegexOptions.IgnoreCase);
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

                // Get the installed programs from registry and UWP package data
                Task<List<InstalledProgram>> getInstalledProgram = InstalledProgram.GetInstalledProgramsAsync();
                List<InstalledProgram> installedPrograms = getInstalledProgram.Result;
                installedPrograms.AddRange(InstalledProgram.GetUWPApps());

                // Loop through the returned data, and create a list of DisplayMagician Apps
                foreach (InstalledProgram installedProgram in installedPrograms)
                {
                    string localAppFilename = Path.GetFileName(installedProgram.Path);
                    LocalApp localApp = new LocalApp();
                    localApp.Id = localAppFilename;
                    localApp.Name = installedProgram.Name;
                    localApp.Directory = installedProgram.WorkDir;
                    localApp.ExePath = installedProgram.Path;
                    localApp.Arguments = installedProgram.Arguments;                                                            
                    localApp.IconPath = installedProgram.Icon;                    
                    localApp.ProcessName = Path.GetFileNameWithoutExtension(localApp.ExePath);

                    // Add the Locally Installed App to the list of Apps
                    _allLocalApps.Add(localApp);
                }                

               logger.Info($"LocalLibrary/LoadInstalledApps: Found {_allLocalApps.Count} locally installed Apps");

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
                logger.Warn(ex, "LocalLibrary/GetAllInstalledApps: The user does not have the permissions required to read the locally installed InstallDir registry key.");
            }
            catch (ObjectDisposedException ex)
            {
                logger.Warn(ex, "LocalLibrary/GetAllInstalledApps: The Microsoft.Win32.RegistryKey is closed when trying to access the locally installed InstallDir registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "LocalLibrary/GetAllInstalledApps: The locally installed InstallDir registry key has been marked for deletion so we cannot access the value dueing the LocalLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "LocalLibrary/GetAllInstalledApps: The user does not have the necessary registry rights to check whether locally installed apps are installed.");
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
