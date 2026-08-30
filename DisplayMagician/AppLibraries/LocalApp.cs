using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Diagnostics;
using DisplayMagician.Processes;
using System.ComponentModel;
using Newtonsoft.Json;
using Windows.ApplicationModel;
using Windows.System;
using Windows.ApplicationModel.Core;
using Windows.System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

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

        public LocalApp() 
        {
            // put in some sensible defaults for the properties
            _LocalAppId = "";
            _LocalAppName = "";
            _LocalAppExePath = "";
            _LocalAppDir = "";
            _LocalAppExe = "";
            _LocalAppProcessName = "";
            _LocalAppIconPath = "";
        }

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

        [DefaultValue(SupportedAppLibraryType.Unknown)]
        public override SupportedAppLibraryType AppLibraryType
        {
            get
            {
                if (_LocalAppType == InstalledAppType.InstalledProgram)
                {
                    return SupportedAppLibraryType.LocalInstalledApp;
                }
                else if (_LocalAppType == InstalledAppType.UWP)
                {
                    return SupportedAppLibraryType.LocalUWPApp;
                }
                return SupportedAppLibraryType.Unknown;
            }
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

        [DefaultValue("")]
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
                            logger.Debug(ex, $"LocalApp/IsRunning: Accessing Process.ProcessName caused exception. Trying AppProcess.MainModule.FileName instead");
                            // If there is a race condition where MainModule isn't available, then we 
                            // instead try the much slower MainModule.FileName (which does the same thing)
                            string filePath = null;
                            try
                            {
                                filePath = AppProcess.MainModule.FileName;
                            }
                            catch (Exception ex2)
                            {
                                logger.Debug(ex2, $"LocalApp/IsRunning: Accessing AppProcess.MainModule.FileName also caused exception. Assuming it is a matching App process.");
                            }
                            if (filePath == null)
                            {
                                // if we hit this bit then MainModule.FileName failed,
                                // so we just assume that the process is a App process
                                // as it matched the process search
                                numAppProcesses++;
                                continue;
                            }
                            else
                            {
                                if (filePath.StartsWith(_LocalAppExePath))
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
                    logger.Error($"LocalApp/IsRunning: This LocalApp is not a recognised InstalledAppType.");
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

        [JsonIgnore]
        public override bool IsInstalled
        {
            get
            {
                // Check if it is an installed program app
                if (LocalAppType == InstalledAppType.InstalledProgram)
                {
                    return !string.IsNullOrWhiteSpace(_LocalAppExePath) && File.Exists(_LocalAppExePath);
                }
                else if (LocalAppType == InstalledAppType.UWP)
                {
                    if (UWPIsInstalled(_LocalAppId).Result)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    logger.Error($"LocalApp/IsRunning: This LocalApp is not a recognised InstalledAppType.");
                    return false;
                }
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
            if (!Program.AppHasPackageIdentity)
            {
                logger.Debug($"LocalApp/UWPIsRunning: DisplayMagician is not running with package identity. UWP running-state checks are disabled for {aumid}.");
                return false;
            }

            // First check whether we actually have permission to use the AppDiagnosticInfo APIs.
            // Without a package identity (i.e. running as an unpackaged Win32 app), this will
            // return Limited or Denied, and calling RequestInfoForAppAsync will throw a
            // ThreadAbortException / "insufficient rights" error. Fall back to the watcher state
            // in that case — the watcher was started in Start() and tracks Added/Removed events.
            var accessStatus = await AppDiagnosticInfo.RequestAccessAsync();
            if (accessStatus != DiagnosticAccessStatus.Allowed)
            {
                logger.Debug($"LocalApp/UWPIsRunning: AppDiagnosticInfo access is {accessStatus} (not Allowed) for {aumid}. Falling back to watcher state.");
                return _LocalAppIsRunning == AppResourceGroupExecutionState.Running;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                IList<AppDiagnosticInfo> infos = await AppDiagnosticInfo.RequestInfoForAppAsync(aumid).AsTask(cts.Token);
                foreach (var thing in infos)
                {
                    var groups = thing.GetResourceGroups();
                    if (groups.Count == 0)
                        return false;

                    // We only check the first resource group as it represents the main part of the UWP app.
                    // NOTE: this may not always be the right group, but there is no reliable way to pick the correct one.
                    AppResourceGroupExecutionState status = groups[0].GetStateReport().ExecutionState;

                    // NotRunning: Windows has terminated the app (~10s after user closes it).
                    // Unknown:    Windows cannot determine the state — treat as not running.
                    // Running / Suspending: the app is alive (Suspending is a transient state, app is still present).
                    // Suspended: the app is frozen in memory. Treated as not running so DisplayMagician
                    //            reverts settings promptly once the app is suspended/closed.
                    //            Change to `true` here if you want to keep waiting while the app is suspended.
                    return status == AppResourceGroupExecutionState.Running
                        || status == AppResourceGroupExecutionState.Suspending;
                }

                // No diagnostic info returned — app is not running.
                return false;
            }
            catch (OperationCanceledException)
            {
                logger.Debug($"LocalApp/UWPIsRunning: Timeout waiting for AppDiagnosticInfo for {aumid}. Falling back to watcher state.");
                return _LocalAppIsRunning == AppResourceGroupExecutionState.Running;
            }
            catch (Exception ex)
            {
                logger.Debug(ex, $"LocalApp/UWPIsRunning: AppDiagnosticInfo.RequestInfoForAppAsync({aumid}) caused an exception. Falling back to watcher state.");
                return _LocalAppIsRunning == AppResourceGroupExecutionState.Running;
            }
        }

        private async Task<bool> UWPIsInstalled(string aumid)
        {
            if (!Program.AppHasPackageIdentity)
            {
                logger.Debug($"LocalApp/UWPIsInstalled: DisplayMagician is not running with package identity. UWP install checks are disabled for {aumid}.");
                return false;
            }

            // Request access to app diagnostics
            var accessStatus = await AppDiagnosticInfo.RequestAccessAsync();
            if (accessStatus != DiagnosticAccessStatus.Allowed)
            {
                logger.Debug($"LocalApp/UWPIsInstalled: Access to app diagnostics denied or limited: {accessStatus}");
                return false;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                // Attempt to retrieve diagnostic info with a timeout
                var infos = await AppDiagnosticInfo.RequestInfoForAppAsync(aumid).AsTask(cts.Token);
                foreach (var info in infos)
                {
                    var installPath = info.AppInfo.Package.InstalledPath;
                    return !string.IsNullOrWhiteSpace(installPath) && File.Exists(installPath);
                }
                return false;
            }
            catch (OperationCanceledException)
            {
                logger.Debug($"\"LocalApp/UWPIsInstalled: Timeout while retrieving diagnostic info for AUMID: {aumid}");
                return false;
            }
            catch (Exception ex)
            {
                logger.Debug(ex, $"\"LocalApp/UWPIsInstalled: Exception occurred while retrieving diagnostic info for AUMID: {aumid}");
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
            
            if (LocalAppType == InstalledAppType.InstalledProgram)
            {
                processesStarted = ProcessUtils.StartProcess(ExePath, Arguments, priority, timeout, runExeAsAdmin);
                if (processesStarted.Count > 0)
                {
                    logger.Trace($"LocalApp/Start: Started LocalApp installed program {Name} with {processesStarted.Count} processes.");
                    return true;
                }

                logger.Error($"LocalApp/Start: Unable to start LocalApp installed program {Name} as no processes were created!");
                return false;
            }
            else if (LocalAppType == InstalledAppType.UWP)
            {
                if (!Program.AppHasPackageIdentity)
                {
                    logger.Warn($"LocalApp/Start: Cannot start UWP app {Name} because DisplayMagician is not running with package identity.");
                    return false;
                }

                var accessStatus = AppDiagnosticInfo.RequestAccessAsync().GetResults();
                if (accessStatus != DiagnosticAccessStatus.Allowed)
                {
                    logger.Debug($"LocalApp/UWPIsRunning: Access to app diagnostics denied by user or limited: {accessStatus}");
                    return false;
                }

                // Create UWP watcher to watch this app starting
                _LocalAppUWPWatcher = Windows.System.AppDiagnosticInfo.CreateWatcher();
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
                name = "Unknown";
            }

            if (IsRunning)
            {
                return name + " " + "[Running]";
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
