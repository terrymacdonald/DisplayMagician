using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DisplayMagician.Resources;
using System.Diagnostics;
using DisplayMagician.Processes;
using System.ComponentModel;
using Newtonsoft.Json;
using Windows.ApplicationModel;
using Windows.System;
using Windows.ApplicationModel.Core;
using Windows.System.Diagnostics;
using System.Threading.Tasks;

namespace DisplayMagician.AppLibraries
{
    public class LocalApp : App
    {
        private string _LocalAppId;
        private string _LocalAppName;
        private string _LocalAppExePath;
        private string _LocalAppDir;
        private string _LocalAppExe;
        private bool _LocalExecutableArgumentsRequired = false;
        private string _LocalAppProcessName;
        private List<Process> _LocalAppProcesses = new List<Process>();
        private string _LocalAppIconPath;
        private InstalledAppType _LocalAppType = InstalledAppType.InstalledProgram;
        //private Package _LocalAppPackage;
        private AppListEntry _LocalAppListEntry;
        private AppDiagnosticInfoWatcher _LocalAppUWPWatcher = null;
        private string _LocalAppFamilyName = "";
        private AppResourceGroupExecutionState _LocalAppIsRunning = AppResourceGroupExecutionState.NotRunning;
        //private string _gogURI;
        private static readonly LocalLibrary _LocalAppLibrary = LocalLibrary.GetLibrary();
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static LocalApp()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }

        public LocalApp() { }

        public LocalApp(string LocalAppId, string LocalAppName, string LocalAppExePath, string LocalAppIconPath)
        {

            //_AppRegistryKey = $@"{LocalLibrary.registryGogInstallsKey}\\{LocalAppId}";
            _LocalAppId = LocalAppId;
            _LocalAppName = LocalAppName;
            _LocalAppExePath = LocalAppExePath;
            _LocalAppDir = Path.GetDirectoryName(LocalAppExePath);
            _LocalAppExe = Path.GetFileName(_LocalAppExePath);
            _LocalAppProcessName = Path.GetFileNameWithoutExtension(_LocalAppExePath);
            _LocalAppIconPath = LocalAppIconPath;

        }

        [DefaultValue("")]
        public override string Id
        {
            get => _LocalAppId;
            set => _LocalAppId = value;
        }

        [DefaultValue("")]
        public override string Name
        {
            get => _LocalAppName;
            set => _LocalAppName = value;
        }

        [DefaultValue(SupportedAppLibraryType.Local)]
        public override SupportedAppLibraryType AppLibraryType
        {
            get => SupportedAppLibraryType.Local;
        }

        [DefaultValue("")]
        public override string IconPath
        {
            get => _LocalAppIconPath;
            set => _LocalAppIconPath = value;
        }

        [DefaultValue("")]
        public override string ExePath
        {
            get => _LocalAppExePath;
            set => _LocalAppExePath = value;
        }

        [DefaultValue(false)]
        public override bool ExecutableArgumentsRequired {
            get => _LocalExecutableArgumentsRequired;
            set => _LocalExecutableArgumentsRequired = value;
        }

        [DefaultValue("")]
        public override string Directory
        {
            get => _LocalAppDir;
            set => _LocalAppDir = value;
        }

        [DefaultValue("")]
        public override string ProcessName
        {
            get => _LocalAppProcessName;
            set => _LocalAppProcessName = value;
        }

        [DefaultValue(default(List<Process>))]
        public override List<Process> Processes
        {
            get => _LocalAppProcesses;
            set => _LocalAppProcesses = value;
        }

        [DefaultValue(InstalledAppType.InstalledProgram)]
        public InstalledAppType LocalAppType
        {
            get => _LocalAppType;
            set => _LocalAppType = value;
        }
        
        public string FamilyName
        {
            get => _LocalAppFamilyName;
            set => _LocalAppFamilyName = value;
        }

        [JsonIgnore]
        public AppListEntry AppListEntry
        {
            get => _LocalAppListEntry;
            set => _LocalAppListEntry = value;
        }

        [JsonIgnore]
        public override AppLibrary AppLibrary
        {
            get => _LocalAppLibrary;
        }

        [JsonIgnore]
        public override bool IsRunning
        {
            get
            {
                // Check if it is an installed program app
                if (LocalAppType == InstalledAppType.InstalledProgram)
                {
                    int numAppProcesses = 0;
                    _LocalAppProcesses = Process.GetProcessesByName(_LocalAppProcessName).ToList();
                    foreach (Process AppProcess in _LocalAppProcesses)
                    {
                        try
                        {
                            if (AppProcess.ProcessName.Equals(_LocalAppProcessName))
                                numAppProcesses++;
                        }
                        catch (Exception ex)
                        {
                            logger.Debug(ex, $"LocalApp/IsRunning: Accessing Process.ProcessName caused exception. Trying AppUtils.GetMainModuleFilepath instead");
                            // If there is a race condition where MainModule isn't available, then we 
                            // instead try the much slower GetMainModuleFilepath (which does the same thing)
                            string filePath = AppProcess.MainModule.FileName;
                            if (filePath == null)
                            {
                                // if we hit this bit then AppUtils.GetMainModuleFilepath failed,
                                // so we just assume that the process is a App process
                                // as it matched the process search
                                numAppProcesses++;
                                continue;
                            }
                            else
                            {
                                if (filePath.StartsWith(_LocalAppExe))
                                    numAppProcesses++;
                            }
                        }
                    }

                    if (numAppProcesses > 0)
                        return true;
                    else
                        return false;
                }
                else if (LocalAppType == InstalledAppType.UWP)
                {
                    if (_LocalAppListEntry is AppListEntry)
                    {

                        if (_LocalAppIsRunning == AppResourceGroupExecutionState.Running)
                        {
                            return true;
                        }
                        else 
                        {
                            return false;
                        }

                        /*

                        IReadOnlyList<AppListEntry> applListEntries = (IReadOnlyList<AppListEntry>)_LocalAppPackage.GetAppListEntries();
                        if (applListEntries.Count > 0)
                        {
                            string name = "";
                            string aumi = "";

                            var entry = applListEntries[0];
                            aumi = entry.AppUserModelId;
                            name = entry.DisplayInfo.DisplayName;

                            var things = Windows.System.AppDiagnosticInfo.RequestInfoAsync().GetResults();
                            foreach (var thing in things)
                            {
                                thing.GetResourceGroups();
                            }

                            List<PackageContentGroup> pcgList = _LocalAppPackage.GetContentGroupsAsync().GetResults().ToList();
                            foreach (PackageContentGroup pcg in pcgList)
                            {
                                pcg.
                            }
                        }*/                        
                    }
                    else
                    {
                        logger.Error($"LocalApp/IsRunning: This UWP LocalApp does not have a Package associated with it. There was an error creating the LocalApp, which means we cannot use it now.");
                        return false;
                    }
                }
                else
                {
                    logger.Error($"LocalApp/IsRunning: This LocalApp is not a recognised InstalledPpType.");
                    return false;
                }
                    

            }
                //ProcessUtils.GetChildProcesses();
                
                
        }

        private void UWPWatcherAdded(AppDiagnosticInfoWatcher sender, AppDiagnosticInfoWatcherEventArgs args)
        {
            // This function is run whenever a new UWP app is started
            if (args.AppDiagnosticInfo.AppInfo.AppUserModelId == _LocalAppId)
            {
                _LocalAppIsRunning = AppResourceGroupExecutionState.Running;
            }

        }

        private void UWPWatcherRemoved(AppDiagnosticInfoWatcher sender, AppDiagnosticInfoWatcherEventArgs args)
        {
            // This function is run whenever a new UWP app is stopped or terminated
            if (args.AppDiagnosticInfo.AppInfo.AppUserModelId == _LocalAppId)
            {
                _LocalAppIsRunning = AppResourceGroupExecutionState.NotRunning;
                _LocalAppUWPWatcher.Stop();
            }

        }

        [JsonIgnore]
        public override bool IsUpdating
        {
            get
            {
                return false;
            }
        }

        public override bool CopyTo(App LocalApp)
        {
            if (!(LocalApp is LocalApp))
                return false;

            // Copy all the App data over to the other App
            LocalApp.IconPath = IconPath;
            LocalApp.Id = Id;
            LocalApp.Name = Name;
            LocalApp.ExePath = ExePath;
            LocalApp.Directory = Directory;
            return true;
        }

        public override bool Start(out List<Process> processesStarted, string gameArguments = "", ProcessPriority priority = ProcessPriority.Normal, int timeout = 20, bool runExeAsAdmin = false)
        {
            processesStarted = new List<Process>();
            Process process = null;
            
            if (LocalAppType == InstalledAppType.InstalledProgram)
            {
                processesStarted = ProcessUtils.StartProcess(ExePath, Arguments, priority, timeout, runExeAsAdmin);
                if (processesStarted.Count > 0)
                {
                    logger.Trace($"LocalApp/Start: Started LocalApp installed program {Name} with {processesStarted.Count} processes.");
                }
                else
                {
                    logger.Error($"LocalApp/Start: Unable to start LocalApp installed program {Name} as no processes were created!");
                }
            }
            else if (LocalAppType == InstalledAppType.UWP)
            {
                // Create UWP watcher to watch this app starting
                var _LocalAppUWPWatcher = Windows.System.AppDiagnosticInfo.CreateWatcher();
                _LocalAppUWPWatcher.Added += UWPWatcherAdded;
                _LocalAppUWPWatcher.Removed += UWPWatcherRemoved;
                _LocalAppUWPWatcher.Start();

                _LocalAppListEntry = InstalledProgram.GetUWPAppListEntryByAUMID(_LocalAppId);
                if (_LocalAppListEntry is AppListEntry)
                {
                    bool myLaunchWorked = StartUWPProcess().GetAwaiter().GetResult();

                    if (myLaunchWorked)
                    {
                        // app launched
                        logger.Error($"LocalApp/Start: Started LocalApp application {Name} successfully!");
                        _LocalAppIsRunning = AppResourceGroupExecutionState.Running;
                        return true;
                    }
                    else
                    {
                        // app not launched!
                        logger.Error($"LocalApp/Start: Unable to start LocalApp application {Name} as the launch didn't work!");
                        _LocalAppIsRunning = AppResourceGroupExecutionState.NotRunning;
                        return false;
                    }
                }    
                else
                {
                    // app not launched!
                    logger.Error($"LocalApp/Start: Unable to start LocalApp application {Name} as the AUMI {_LocalAppId} cannot be found!");
                    _LocalAppIsRunning = AppResourceGroupExecutionState.NotRunning;
                    return false;
                }
               
                //processesStarted = StartUWPProcess(ExePath, Arguments, priority, timeout, runExeAsAdmin);
                /*if (processesStarted.Count > 0)
                {
                    logger.Trace($"LocalApp/Start: Started LocalApp UWP {Name} with {processesStarted.Count} processes.");
                }
                else
                {
                    logger.Error($"LocalApp/Start: Unable to start LocalApp UWP {Name} as no processes were created!");
                }*/
                
            }
            else
            {
                logger.Error($"LocalApp/Start: Unable to start LocalApp as the App is of an unknown type!");
            }
                        
            //processesCreated = ProcessUtils.StartProcess(shortcutToUse.ExecutableNameAndPath, shortcutToUse.ExecutableArguments, shortcutToUse.ProcessPriority, shortcutToUse.StartTimeout, shortcutToUse.RunExeAsAdministrator);
            if (process != null)
            {
                processesStarted.Add(process);
                return true;
            }
            else
            {
                return false;
            }
        }        

        public override bool Stop()
        {
            return true;
        }

        public override string ToString()
        {
            var name = _LocalAppName;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = Language.Unknown;
            }

            if (IsRunning)
            {
                return name + " " + Language.Running;
            }

            /*if (IsUpdating)
            {
                return name + " " + Language.Updating;
            }*/

            return name;
        }

        private async Task<bool> StartUWPProcess()
        {
            bool result = await _LocalAppListEntry.LaunchAsync();

            return result;
        }

    }
}