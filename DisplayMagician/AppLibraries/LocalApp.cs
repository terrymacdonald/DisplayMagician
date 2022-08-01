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
        public override SupportedAppLibraryType AppLibrary
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


        [JsonIgnore]
        public override bool IsRunning
        {
            get
            {
                // Check if it is a UWP app

                //return !ProcessUtils.ProcessExited(_LocalAppProcessName);
                int numAppProcesses = 0;
                if (Path.GetFileName(_LocalAppExePath).Equals("explorer.exe"))
                {
                    // it's a UWP app, so we detect it differently
                    //Package uGetUWPAppPackageByAUMID()


                    _LocalAppProcesses = Process.GetProcesses().Where(pr => pr.MainModule.FileName.Contains(_LocalAppDir)).ToList();
                    if (_LocalAppProcesses.Count > 0)
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
                    // its NOT a UWP app
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

            }
                //ProcessUtils.GetChildProcesses();
                
                
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

        public override bool Start(ProcessPriority priority, int timeout, bool runExeAsAdmin, out List<Process> processesStarted)
        {
            processesStarted = new List<Process>();
            Process process = null;
            
            if (LocalAppType == InstalledAppType.InstalledProgram)
            {
                processesStarted = ProcessUtils.StartProcess(ExePath, Arguments, priority, timeout, runExeAsAdmin);
            }
            else if (LocalAppType == InstalledAppType.UWP)
            {
                processesStarted = StartUWPProcess(ExePath, Arguments, priority, timeout, runExeAsAdmin);
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

        private static List<Process> StartUWPProcess(string executable, string arguments, ProcessPriority processPriority, int startTimeout = 1, bool runAsAdministrator = false)
        {
            List<Process> returnedProcesses = ProcessUtils.StartProcess(executable, arguments, processPriority, startTimeout, runAsAdministrator);            

            return returnedProcesses;
        }

    }
}