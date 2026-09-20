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
    public class EpicGame : Game
    {
        private string _epicGameId;
        private string _epicGameName;
        private string _epicGameExePath;
        private string _epicGameDir;
        private string _epicGameExe;
        private string _epicGameProcessName;
        private List<Process> _epicGameProcesses = new List<Process>();
        private string _epicGameIconPath;
        //private string _epicURI;
        private static readonly EpicLibrary _epicGameLibrary = EpicLibrary.GetLibrary();
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public EpicGame() { }

        public EpicGame(string epicGameId, string epicGameName, string epicGameExePath, string epicGameIconPath)
        {

            //_gameRegistryKey = $@"{EpicLibrary.registryEpicInstallsKey}\\{EpicGameId}";
            _epicGameId = epicGameId;
            _epicGameName = epicGameName;
            _epicGameExePath = epicGameExePath;
            _epicGameDir = Path.GetDirectoryName(epicGameExePath);
            _epicGameExe = Path.GetFileName(_epicGameExePath);
            _epicGameProcessName = Path.GetFileNameWithoutExtension(_epicGameExePath);
            _epicGameIconPath = epicGameIconPath;

        }

        public override string Id
        {
            get => _epicGameId;
            set => _epicGameId = value;
        }

        public override string Name
        {
            get => _epicGameName;
            set => _epicGameName = value;
        }

        public override SupportedGameLibraryType GameLibraryType
        {
            get => SupportedGameLibraryType.Epic;
        }

        [JsonIgnore]
        public override GameLibrary GameLibrary
        {
            get => _epicGameLibrary;
        }

        public override string IconPath
        {
            get => _epicGameIconPath;
            set => _epicGameIconPath = value;
        }

        public override string ExePath
        {
            get => _epicGameExePath;
            set => _epicGameExePath = value;
        }

        public override string Directory
        {
            get => _epicGameDir;
            set => _epicGameDir = value;
        }

        public override string Executable
        {
            get => _epicGameExe;
            set => _epicGameExe = value;
        }

        public override string ProcessName
        {
            get => _epicGameProcessName;
            set => _epicGameProcessName = value;
        }

        public override List<Process> Processes
        {
            get => _epicGameProcesses;
            set => _epicGameProcesses = value;
        }

        public override bool IsRunning
        {
            get
            {
                if (IsProcessTreeMonitorActive)
                    return IsProcessTreeRunning;
                return !ProcessUtils.ProcessExited(_epicGameProcessName);
            }
        }

        // TODO Have to do much more research to figure out how to detect when Epic is updating a game
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
                return !string.IsNullOrWhiteSpace(_epicGameExePath) && File.Exists(_epicGameExePath);
            }
        }

        public bool CopyTo(EpicGame epicGame)
        {
            if (epicGame == null)
                return false;

            // Copy all the game data over to the other game
            epicGame.IconPath = IconPath;
            epicGame.Id = Id;
            epicGame.Name = Name;
            epicGame.ExePath = ExePath;
            epicGame.Directory = Directory;
            epicGame.ProcessName = ProcessName;
            epicGame.IconPath = IconPath;
            return true;
        }

        public override string ToString()
        {
            var name = _epicGameName;

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

            // CASE 1: Custom arguments are provided -> Bypass protocol to launch executable directly
            if (!string.IsNullOrWhiteSpace(gameArguments))
            {
                logger.Info($"EpicGame/Start: Custom arguments detected for {Name}. Bypassing launcher protocol.");

                // -EpicPortal skips the launcher DRM step so the executable accepts custom CLI arguments
                string directArgs = $"-EpicPortal {gameArguments}";

                // Run direct process using all your DisplayMagician structural rules
                var directProcesses = ProcessUtils.StartProcess(ExePath, directArgs, priority, timeout, runExeAsAdmin);
                if (directProcesses != null && directProcesses.Count > 0)
                {
                    processesStarted.AddRange(directProcesses);
                }
                
                return processesStarted.Count > 0;
            }

            // CASE 2: No custom arguments -> Use standard Launcher URI Protocol
            // (Requires composite structure: SandboxID%3ACatalogID%3AArtifactID)
            string address = $@"com.epicgames.launcher://apps/{Id}?action=launch&silent=true";
            logger.Info($"EpicGame/Start: No custom arguments. Requesting URI Protocol: {address}");

            // Trigger URL Protocol Handler via ProcessUtils
            var launcherProcesses = ProcessUtils.StartProcess(address, null, priority, timeout, runExeAsAdmin);
            if (launcherProcesses != null && launcherProcesses.Count > 0)
            {
                processesStarted.AddRange(launcherProcesses);
            }

            // SAFEGUARD: Because URI protocol calls return instantly (or return null if handled by an
            // already-open Epic Launcher background process), let's perform an immediate snapshot look 
            // to find the game engine process by name so DisplayMagician doesn't lose tracking state.
            if (processesStarted.Count == 0 && !string.IsNullOrWhiteSpace(_epicGameProcessName))
            {
                logger.Trace($"EpicGame/Start: URI handler returned no process token. Searching active system processes for '{_epicGameProcessName}'...");
                var discoveredProcesses = Process.GetProcessesByName(_epicGameProcessName).ToList();
                if (discoveredProcesses.Count > 0)
                {
                    logger.Debug($"EpicGame/Start: Successfully matched active process target for {Name}. Resuming tracking framework.");
                    processesStarted.AddRange(discoveredProcesses);
                }
            }

            return true;
        }

        public override bool Stop()
        {
            logger.Info($"EpicGame/Stop: Request received to stop {Name} (Process Name: {_epicGameProcessName})");
            bool allStopped = true;

            try
            {
                // Stage 1: Stop processes that are explicitly captured in the DisplayMagician tracking list
                if (Processes != null && Processes.Count > 0)
                {
                    logger.Debug($"EpicGame/Stop: Terminating explicitly tracked processes for {Name}.");
                    
                    // This utilizes your existing ProcessUtils method which loops through backward,
                    // tries CloseMainWindow() first, and then falls back to Kill() if it hangs.
                    allStopped = ProcessUtils.StopProcess(Processes);
                    
                    // Clear out the tracking tokens once closed
                    Processes.Clear();
                }

                // Stage 2: Backup check. If the game was launched via URI protocol, the tracking list 
                // might be empty, or a launcher child process might still be running. Look it up by name.
                if (!string.IsNullOrWhiteSpace(_epicGameProcessName))
                {
                    var activeInstances = Process.GetProcessesByName(_epicGameProcessName).ToList();
                    if (activeInstances.Count > 0)
                    {
                        logger.Debug($"EpicGame/Stop: Found {activeInstances.Count} untracked active processes matching '{_epicGameProcessName}'. Stopping them now.");
                        
                        // Pass the newly discovered runtime processes into your existing handler
                        bool backupStopped = ProcessUtils.StopProcess(activeInstances);
                        allStopped = allStopped && backupStopped;
                    }
                }

                if (allStopped)
                {
                    logger.Info($"EpicGame/Stop: Successfully stopped all process instances for {Name}.");
                }
                else
                {
                    logger.Warn($"EpicGame/Stop: One or more process instances for {Name} failed to close cleanly.");
                }

                return allStopped;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"EpicGame/Stop: Unexpected exception encountered while trying to stop {Name}");
                return false;
            }
        }

    }
}
