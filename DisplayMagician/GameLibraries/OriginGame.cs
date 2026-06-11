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
    public class OriginGame : Game
    {
        private string _originGameId;
        private string _originGameName;
        private string _originGameExePath;
        private string _originGameDir;
        private string _originGameExe;
        private string _originGameProcessName;
        private List<Process> _originGameProcesses = new List<Process>();
        private string _originGameIconPath;
        //private string _originURI;
        private static readonly OriginLibrary _originGameLibrary = OriginLibrary.GetLibrary();
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public OriginGame(string originGameId, string originGameName, string originGameExePath, string originGameIconPath)
        {

            //_gameRegistryKey = $@"{OriginLibrary.registryOriginInstallsKey}\\{OriginGameId}";
            _originGameId = originGameId;
            _originGameName = originGameName;
            _originGameExePath = originGameExePath;
            _originGameDir = Path.GetDirectoryName(originGameExePath);
            _originGameExe = Path.GetFileName(_originGameExePath);
            _originGameProcessName = Path.GetFileNameWithoutExtension(_originGameExePath);
            _originGameIconPath = originGameIconPath;

        }

        public override string Id
        {
            get => _originGameId;
            set => _originGameId = value;
        }

        public override string Name
        {
            get => _originGameName;
            set => _originGameName = value;
        }

        public override SupportedGameLibraryType GameLibraryType
        {
            get => SupportedGameLibraryType.Origin;
        }

        [JsonIgnore]
        public override GameLibrary GameLibrary
        {
            get => _originGameLibrary;
        }

        public override string IconPath
        {
            get => _originGameIconPath;
            set => _originGameIconPath = value;
        }

        public override string ExePath
        {
            get => _originGameExePath;
            set => _originGameExePath = value;
        }

        public override string Directory
        {
            get => _originGameDir;
            set => _originGameDir = value;
        }

        public override string Executable
        {
            get => _originGameExe;
            set => _originGameExe = value;
        }

        public override string ProcessName
        {
            get => _originGameProcessName;
            set => _originGameProcessName = value;
        }

        public override List<Process> Processes
        {
            get => _originGameProcesses;
            set => _originGameProcesses = value;
        }

        public override bool IsRunning
        {
            get
            {
                return !ProcessUtils.ProcessExited(_originGameProcessName);
                /*int numGameProcesses = 0;
                _originGameProcesses = Process.GetProcessesByName(_originGameProcessName).ToList();
                foreach (Process gameProcess in _originGameProcesses)
                {
                    try
                    {                       
                        if (gameProcess.ProcessName.Equals(_originGameProcessName))
                            numGameProcesses++;
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex, $"OriginGame/IsRunning: Accessing Process.ProcessName caused exception. Trying GameUtils.GetMainModuleFilepath instead");
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
                            if (filePath.StartsWith(_originGameExePath))
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

        // Have to do much more research to figure out how to detect when Origin is updating a game
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
                return !string.IsNullOrWhiteSpace(_originGameExePath) && File.Exists(_originGameExePath);
            }
        }

        public bool CopyTo(OriginGame originGame)
        {
            if (originGame == null)
                return false;

            // Copy ALL structural parameters to keep tracking contexts synchronized across clones
            originGame.IconPath = IconPath;
            originGame.Id = Id;
            originGame.Name = Name;
            originGame.ExePath = ExePath;
            originGame.Directory = Directory;
            originGame.Executable = Executable;
            originGame.ProcessName = ProcessName;
            return true;
        }

        public override string ToString()
        {
            var name = _originGameName;

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

            // Construct the base launch URL
            string address = $"origin2://game/launch?offerIds={Id}";
            
            // Fix: Pass parameters properly via the standard URI query wrapper format
            if (!String.IsNullOrWhiteSpace(gameArguments))
            {
                address += "&cmdParams=" + Uri.EscapeDataString(gameArguments);
                logger.Info($"OriginGame/Start: Custom arguments detected for {Name}. Appending via URI parameter query payload.");
            }
            
            logger.Info($"OriginGame/Start: Launching via Launcher URI Protocol: {address}");

            // Fire the launcher command 
            var launcherProcesses = ProcessUtils.StartProcess(address, null, priority, timeout, runExeAsAdmin);
            if (launcherProcesses != null && launcherProcesses.Count > 0)
            {
                processesStarted.AddRange(launcherProcesses);
            }

            // SAFEGUARD: If the launcher instance was already active, the process call exits instantly.
            // Query the active OS pool by name right away to let DisplayMagician capture tracking authority.
            if (processesStarted.Count == 0 && !string.IsNullOrWhiteSpace(_originGameProcessName))
            {
                var activeGameProcesses = Process.GetProcessesByName(_originGameProcessName).ToList();
                if (activeGameProcesses.Count > 0)
                {
                    logger.Debug($"OriginGame/Start: Successfully matched active process target for {Name} ('{_originGameProcessName}'). Resuming tracking state.");
                    processesStarted.AddRange(activeGameProcesses);
                }
            }

            return true;
        }

        public override bool Stop()
        {
            logger.Info($"OriginGame/Stop: Request received to stop {Name} (Process Name: {_originGameProcessName})");
            bool allStopped = true;

            try
            {
                // Step 1: Drain explicitly tracked execution tokens via ProcessUtils
                if (Processes != null && Processes.Count > 0)
                {
                    allStopped = ProcessUtils.StopProcess(Processes);
                    Processes.Clear();
                }

                // Step 2: System-wide background fallback sweep by string key identification
                if (!string.IsNullOrWhiteSpace(_originGameProcessName))
                {
                    var runningInstances = Process.GetProcessesByName(_originGameProcessName).ToList();
                    if (runningInstances.Count > 0)
                    {
                        logger.Debug($"OriginGame/Stop: Clearing {runningInstances.Count} remaining untracked background engine threads matching '{_originGameProcessName}'.");
                        bool backupStopped = ProcessUtils.StopProcess(runningInstances);
                        allStopped = allStopped && backupStopped;
                    }
                }

                return allStopped;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"OriginGame/Stop: Exception encountered while shutting down {Name}");
                return false;
            }
        }
    }
}