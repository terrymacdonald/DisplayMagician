using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Diagnostics;
using DisplayMagician.Processes;
using Newtonsoft.Json;

namespace DisplayMagician.GameLibraries
{
    public class UplayGame : Game
    {
        //private string _gameRegistryKey;
        private string _uplayGameId;
        private string _uplayGameName;
        private string _uplayGameExePath;
        private string _uplayGameDir;
        private string _uplayGameExe;
        private string _uplayGameProcessName;
        private List<Process> _uplayGameProcesses = new List<Process>();
        private string _uplayGameIconPath;
        private static readonly UplayLibrary _uplayGameLibrary = UplayLibrary.GetLibrary();
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public UplayGame(string uplayGameId, string uplayGameName, string uplayGameExePath, string uplayGameIconPath)
        {

            //_gameRegistryKey = $@"{UplayLibrary.registryUplayInstallsKey}\\{uplayGameId}";
            _uplayGameId = uplayGameId;
            _uplayGameName = uplayGameName;
            _uplayGameExePath = uplayGameExePath;
            _uplayGameDir = Path.GetDirectoryName(uplayGameExePath);
            _uplayGameExe = Path.GetFileName(_uplayGameExePath);
            _uplayGameProcessName = Path.GetFileNameWithoutExtension(_uplayGameExePath);
            _uplayGameIconPath = uplayGameIconPath;

        }

        public override string Id
        {
            get => _uplayGameId;
            set => _uplayGameId = value;
        }

        public override string Name
        {
            get => _uplayGameName;
            set => _uplayGameName = value;
        }

        public override SupportedGameLibraryType GameLibraryType
        {
            get => SupportedGameLibraryType.Uplay;
        }

        [JsonIgnore]
        public override GameLibrary GameLibrary
        {
            get => _uplayGameLibrary;
        }

        public override string IconPath
        {
            get => _uplayGameIconPath;
            set => _uplayGameIconPath = value;
        }

        public override string ExePath
        {
            get => _uplayGameExePath;
            set => _uplayGameExePath = value;
        }

        public override string Directory
        {
            get => _uplayGameDir;
            set => _uplayGameDir = value;
        }
        public override string Executable
        {
            get => _uplayGameExe;
            set => _uplayGameExe = value;
        }

        public override string ProcessName
        {
            get => _uplayGameProcessName;
            set => _uplayGameProcessName = value;
        }

        public override List<Process> Processes
        {
            get => _uplayGameProcesses;
            set => _uplayGameProcesses = value;
        }

        public override bool IsRunning
        {
            get
            {
                if (IsProcessTreeMonitorActive)
                    return IsProcessTreeRunning;
                return !ProcessUtils.ProcessExited(_uplayGameProcessName);
                /*int numGameProcesses = 0;
                _uplayGameProcesses = Process.GetProcessesByName(_uplayGameProcessName).ToList();
                foreach (Process gameProcess in _uplayGameProcesses)
                {
                    try
                    {                       
                        if (gameProcess.ProcessName.Equals(_uplayGameProcessName))
                            numGameProcesses++;
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex, $"UplayGame/IsRunning: Accessing Process.ProcessName caused exception. Trying GameUtils.GetMainModuleFilepath instead");
                        // If there is a race condition where MainModule isn't available, then we 
                        // instead try the much slower GetMainModuleFilepath (which does the same thing)
                        string filePath = GameUtils.GetMainModuleFilepath(gameProcess.Id);
                        if (filePath == null)
                        {
                            // if we hit this bit then GameUtils.GetMainModuleFilepath failed,
                            // so we just assume that the process is a game process
                            // as it matched the original process search
                            numGameProcesses++;
                            continue;
                        }
                        else
                        {
                            if (filePath.StartsWith(_uplayGameExePath))
                                numGameProcesses++;
                        }
                            
                    }
                }
                if (numGameProcesses > 0)
                    return true;
                else
                    return false;*/
            }
        }

        // Have to do much more research to figure out how to detect when Uplay is updating a game      
        public override bool IsUpdating
        {
            get
            {
                return false;
            }
        }

        public override bool IsInstalled
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_uplayGameExePath) && File.Exists(_uplayGameExePath);
            }
        }

        public bool CopyTo(UplayGame uplayGame)
        {
            if (uplayGame == null)
                return false;

            // Copy ALL structural data to ensure process state tracking persists cleanly across instances
            uplayGame.IconPath = IconPath;
            uplayGame.Id = Id;
            uplayGame.Name = Name;
            uplayGame.ExePath = ExePath;
            uplayGame.Directory = Directory;
            uplayGame.Executable = Executable;
            uplayGame.ProcessName = ProcessName;
            return true;
        }

        public override string ToString()
        {
            var name = _uplayGameName;

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

        public override bool Start(out List<Process> processesStarted, string gameArguments = "", ProcessPriority priority = ProcessPriority.Normal, int timeout = 20, bool runExeAsAdmin = false)
        {
            processesStarted = new List<Process>();
            BeginProcessTreeMonitoring(timeout);

            // CASE 1: Custom arguments provided -> Bypass protocol and execute game binary directly
            if (!string.IsNullOrWhiteSpace(gameArguments))
            {
                logger.Info($"UplayGame/Start: Arguments detected for {Name}. Launching game executable directly to pass parameters.");

                var directProcesses = ProcessUtils.StartProcess(ExePath, gameArguments, priority, timeout, runExeAsAdmin);
                if (directProcesses != null && directProcesses.Count > 0)
                {
                    processesStarted.AddRange(directProcesses);
                }
                return processesStarted.Count > 0;
            }

            // CASE 2: No arguments -> Request standard Ubisoft Connect URI Protocol Handler
            string address = $@"uplay://launch/{Id}";
            logger.Info($"UplayGame/Start: No arguments. Requesting standard URI Protocol: {address}");

            var launcherProcesses = ProcessUtils.StartProcess(address, null, priority, timeout, runExeAsAdmin);
            if (launcherProcesses != null && launcherProcesses.Count > 0)
            {
                processesStarted.AddRange(launcherProcesses);
            }

            // SAFEGUARD: If Ubisoft Connect was already open in the tray, deep-link triggers exit instantly.
            // Perform an immediate name-based system process query so DisplayMagician captures tracking control.
            if (processesStarted.Count == 0 && !string.IsNullOrWhiteSpace(_uplayGameProcessName))
            {
                var activeGameProcesses = Process.GetProcessesByName(_uplayGameProcessName).ToList();
                if (activeGameProcesses.Count > 0)
                {
                    logger.Debug($"UplayGame/Start: Re-associated untracked background engine process '{_uplayGameProcessName}' for {Name}.");
                    processesStarted.AddRange(activeGameProcesses);
                }
            }

            return true;
        }

        public override bool Stop()
        {
            logger.Info($"UplayGame/Stop: Request received to stop {Name} (Process Name: {_uplayGameProcessName})");
            bool allStopped = true;

            try
            {
                // Step 1: Drain explicitly tracked tracking list tokens via ProcessUtils
                if (Processes != null && Processes.Count > 0)
                {
                    allStopped = ProcessUtils.StopProcess(Processes);
                    Processes.Clear();
                }

                // Step 2: System-wide background name query fallback sweep
                if (!string.IsNullOrWhiteSpace(_uplayGameProcessName))
                {
                    var runningInstances = Process.GetProcessesByName(_uplayGameProcessName).ToList();
                    if (runningInstances.Count > 0)
                    {
                        logger.Debug($"UplayGame/Stop: Clearing {runningInstances.Count} remaining untracked instances of '{_uplayGameProcessName}'.");
                        bool backupStopped = ProcessUtils.StopProcess(runningInstances);
                        allStopped = allStopped && backupStopped;
                    }
                }

                return allStopped;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"UplayGame/Stop: Exception encountered while terminating {Name}");
                return false;
            }
        }

    }
}
