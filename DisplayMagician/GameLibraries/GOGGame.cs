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
    public class GogGame : Game
    {
        private string _gogGameId;
        private string _gogGameName;
        private string _gogGameExePath;
        private string _gogGameDir;
        private string _gogGameExe;
        private string _gogGameProcessName;
        private List<Process> _gogGameProcesses = new List<Process>();
        private string _gogGameIconPath;
        //private string _gogURI;
        private static readonly GogLibrary _gogGameLibrary = GogLibrary.GetLibrary();
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public GogGame() { }

        public GogGame(string gogGameId, string gogGameName, string gogGameExePath, string gogGameIconPath)
        {

            //_gameRegistryKey = $@"{GogLibrary.registryGogInstallsKey}\\{GogGameId}";
            _gogGameId = gogGameId;
            _gogGameName = gogGameName;
            _gogGameExePath = gogGameExePath;
            _gogGameDir = Path.GetDirectoryName(gogGameExePath);
            _gogGameExe = Path.GetFileName(_gogGameExePath);
            _gogGameProcessName = Path.GetFileNameWithoutExtension(_gogGameExePath);
            _gogGameIconPath = gogGameIconPath;

        }

        public override string Id
        {
            get => _gogGameId;
            set => _gogGameId = value;
        }

        public override string Name
        {
            get => _gogGameName;
            set => _gogGameName = value;
        }

        public override SupportedGameLibraryType GameLibraryType
        {
            get => SupportedGameLibraryType.GOG;
        }

        [JsonIgnore]
        public override GameLibrary GameLibrary
        {
            get => _gogGameLibrary;
        }

        public override string IconPath
        {
            get => _gogGameIconPath;
            set => _gogGameIconPath = value;
        }

        public override string ExePath
        {
            get => _gogGameExePath;
            set => _gogGameExePath = value;
        }

        public override string Directory
        {
            get => _gogGameDir;
            set => _gogGameDir = value;
        }

        public override string Executable
        {
            get => _gogGameExe;
            set => _gogGameExe = value;
        }

        public override string ProcessName
        {
            get => _gogGameProcessName;
            set => _gogGameProcessName = value;
        }

        public override List<Process> Processes
        {
            get => _gogGameProcesses;
            set => _gogGameProcesses = value;
        }

        public override bool IsRunning
        {
            get
            {
                return !ProcessUtils.ProcessExited(_gogGameProcessName);
                /*int numGameProcesses = 0;
                _gogGameProcesses = Process.GetProcessesByName(_gogGameProcessName).ToList();
                foreach (Process gameProcess in _gogGameProcesses)
                {
                    try
                    {                       
                        if (gameProcess.ProcessName.Equals(_gogGameProcessName))
                            numGameProcesses++;
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex, $"GogGame/IsRunning: Accessing Process.ProcessName caused exception. Trying GameUtils.GetMainModuleFilepath instead");
                        // If there is a race condition where MainModule isn't available, then we 
                        // instead try the much slower GetMainModuleFilepath (which does the same thing)
                        string filePath = GameUtils.GetMainModuleFilepath(gameProcess.Id);
                        if (filePath == null)
                        {
                            // if we hit this bit then GameUtils.GetMainModuleFilepath failed,
                            // so we just assume that the process is a game process
                            // as it matched the gogal process search
                            numGameProcesses++;
                            continue;
                        }
                        else
                        {
                            if (filePath.StartsWith(_gogGameExePath))
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

        // Have to do much more research to figure out how to detect when Gog is updating a game
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
                return !string.IsNullOrWhiteSpace(_gogGameExePath) && File.Exists(_gogGameExePath);
            }
        }

        public bool CopyTo(GogGame gogGame)
        {
            if (gogGame == null)
                return false;

            // Copy ALL identity data components to preserve tracking validity across references
            gogGame.IconPath = IconPath;
            gogGame.Id = Id;
            gogGame.Name = Name;
            gogGame.ExePath = ExePath;
            gogGame.Directory = Directory;
            gogGame.Executable = Executable;
            gogGame.ProcessName = ProcessName;
            return true;
        }

        public override string ToString()
        {
            var name = _gogGameName;

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

            // CASE 1: Custom arguments provided -> Bypass launcher and run DRM-free executable directly
            if (!string.IsNullOrWhiteSpace(gameArguments))
            {
                logger.Info($"GogGame/Start: Arguments detected for {Name}. Bypassing GOG Galaxy to execute DRM-free binary directly.");

                var directProcesses = ProcessUtils.StartProcess(ExePath, gameArguments, priority, timeout, runExeAsAdmin);
                if (directProcesses != null && directProcesses.Count > 0)
                {
                    processesStarted.AddRange(directProcesses);
                }
                return processesStarted.Count > 0;
            }

            // CASE 2: No arguments -> Request GOG Galaxy Client launcher execution
            string launcherExe = GogLibrary.GetLibrary().GameLibraryExe;
            string args = $@"/command=runGame /gameId={Id} /path=""{Directory}""";
            
            logger.Info($"GogGame/Start: No arguments. Launching {Name} via GOG Galaxy Client: {launcherExe} {args}");

            var launcherProcesses = ProcessUtils.StartProcess(launcherExe, args, priority, timeout, runExeAsAdmin);
            if (launcherProcesses != null && launcherProcesses.Count > 0)
            {
                processesStarted.AddRange(launcherProcesses);
            }

            // SAFEGUARD: If the Galaxy Client was already open in the tray, StartProcess may return instantly.
            // Query the active OS process tree by name immediately so DisplayMagician captures tracking control.
            if (processesStarted.Count == 0 && !string.IsNullOrWhiteSpace(_gogGameProcessName))
            {
                var activeGameProcesses = Process.GetProcessesByName(_gogGameProcessName).ToList();
                if (activeGameProcesses.Count > 0)
                {
                    logger.Debug($"GogGame/Start: Re-associated untracked background engine process '{_gogGameProcessName}' for {Name}.");
                    processesStarted.AddRange(activeGameProcesses);
                }
            }

            return true;
        }

        public override bool Stop()
        {
            logger.Info($"GogGame/Stop: Request received to stop {Name} (Process Name: {_gogGameProcessName})");
            bool allStopped = true;

            try
            {
                // Step 1: Kill tracked tracking list tokens via ProcessUtils
                if (Processes != null && Processes.Count > 0)
                {
                    allStopped = ProcessUtils.StopProcess(Processes);
                    Processes.Clear();
                }

                // Step 2: System-wide background name query sweep fallback
                if (!string.IsNullOrWhiteSpace(_gogGameProcessName))
                {
                    var runningInstances = Process.GetProcessesByName(_gogGameProcessName).ToList();
                    if (runningInstances.Count > 0)
                    {
                        logger.Debug($"GogGame/Stop: Found {runningInstances.Count} untracked active instances of '{_gogGameProcessName}'. Closing them now.");
                        bool backupStopped = ProcessUtils.StopProcess(runningInstances);
                        allStopped = allStopped && backupStopped;
                    }
                }

                return allStopped;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GogGame/Stop: Exception encountered while terminating {Name}");
                return false;
            }
        }

    }
}