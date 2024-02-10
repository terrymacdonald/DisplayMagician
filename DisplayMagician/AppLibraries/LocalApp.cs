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
        private Package _LocalAppPackage;
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
        public Package AppPackage
        {
            get => _LocalAppPackage;
            set => _LocalAppPackage = value;
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
                        if (UWPIsRunning(_LocalAppId).Result)
                        {
                            return true;
                        }
                        else
                        {
                            // UWP App has exited, so we should remove the UWPWatcher if it hasn't already been done
                            if (_LocalAppUWPWatcher != null)
                            {
                                _LocalAppUWPWatcher.Stop();
                                _LocalAppUWPWatcher = null;
                            }
                            return false;
                        }
                        
                    }
                    else
                    {
                        logger.Error($"LocalApp/IsRunning: This UWP LocalApp does not have a Package associated with it. There was an error created when we started the LocalApp, which means we cannot use the same reference now.");
                        return false;
                    }
                }
                else
                {
                    logger.Error($"LocalApp/IsRunning: This LocalApp is not a recognised InstalledPpType.");
                    return false;
                }
                    

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
            }

        }

        private async Task<bool> UWPIsRunning(string aumid)
        {
            try
            {
                IList<AppDiagnosticInfo> infos = await AppDiagnosticInfo.RequestInfoForAppAsync(aumid);
                foreach (var thing in infos)
                {
                    // We only monitor the first item in the resource group, as it seems to be the main part of the UWP app in most apps
                    // NOTE - this may not always monitor the right part of the app, but I'm not sure how to make this logic better.
                    AppResourceGroupExecutionState status = thing.GetResourceGroups()[0].GetStateReport().ExecutionState;
                    if (status == AppResourceGroupExecutionState.NotRunning || status == AppResourceGroupExecutionState.Suspended || status == AppResourceGroupExecutionState.Suspending || status == AppResourceGroupExecutionState.Unknown)
                    {
                        // IMPORTANT - This status only occurs when Windows terminates the app processes (or if it doesn't know the status of the app).
                        // This happens 10 seconds after the app is closed using the X, or when windows runs out of resources and has to terminate the app due to low resources.
                        // This UWP application lifecycle means that there is a 10 second delay between when the UWP app is closed, and when it is really closed by Windows
                        // (and therefore when DisplayMagician can detect it).
                        return false;
                    }
                    else
                    {
                        // False is returned if the UWP app is Running, Suspended or Suspending.
                        return true;
                    }
                }
                return false;
            }
            catch(Exception ex)
            {
                logger.Debug(ex, $"LocalApp/UWPIsRunning: AppDiagnosticInfo.RequestInfoForAppAsync({aumid}) cause an exception due to insufficent rights.");
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
                
            }
            else
            {
                logger.Error($"LocalApp/Start: Unable to start LocalApp as the App is of an unknown type!");
            }
                        
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