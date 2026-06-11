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
    public class XboxGame : Game
    {
        private string _xboxGameId;
        private string _xboxGameName;
        private string _xboxGameExePath;
        private string _xboxGameDir;
        private string _xboxGameExe;
        private string _xboxGameProcessName;
        private List<Process> _xboxGameProcesses = new List<Process>();
        private string _xboxGameIconPath;
        private string _xboxGameAUMID;
        private static readonly XboxLibrary _xboxGameLibrary = XboxLibrary.GetLibrary();
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public XboxGame() { }

        public XboxGame(string xboxGameId, string xboxGameName, string xboxGameExePath, string xboxGameIconPath, string xboxGameAUMID = "")
        {

            _xboxGameId = xboxGameId;
            _xboxGameName = xboxGameName;
            _xboxGameExePath = xboxGameExePath;
            _xboxGameDir = !String.IsNullOrWhiteSpace(xboxGameExePath) ? Path.GetDirectoryName(xboxGameExePath) : "";
            _xboxGameExe = !String.IsNullOrWhiteSpace(xboxGameExePath) ? Path.GetFileName(xboxGameExePath) : "";
            _xboxGameProcessName = !String.IsNullOrWhiteSpace(xboxGameExePath) ? Path.GetFileNameWithoutExtension(xboxGameExePath) : "";
            _xboxGameIconPath = xboxGameIconPath;
            _xboxGameAUMID = xboxGameAUMID;

        }

        public override string Id { 
            get => _xboxGameId;
            set => _xboxGameId = value;
        }

        public override string Name
        {
            get => _xboxGameName;
            set => _xboxGameName = value;
        }

        public override SupportedGameLibraryType GameLibraryType { 
            get => SupportedGameLibraryType.Xbox; 
        }

        [JsonIgnore]
        public override GameLibrary GameLibrary
        {
            get => _xboxGameLibrary;
        }

        public override string IconPath { 
            get => _xboxGameIconPath; 
            set => _xboxGameIconPath = value;
        }

        public string AUMID
        {
            get => _xboxGameAUMID;
            set => _xboxGameAUMID = value;
        }

        public override string ExePath
        {
            get => _xboxGameExePath;
            set => _xboxGameExePath = value;
        }

        public override string Directory
        {
            get => _xboxGameDir;
            set => _xboxGameDir = value;
        }

        public override string Executable 
        {
            get => _xboxGameExe;
            set => _xboxGameExe = value;
        }

        public override string ProcessName 
        {
            get => _xboxGameProcessName;
            set => _xboxGameProcessName = value;
        }

        public override List<Process> Processes
        {
            get => _xboxGameProcesses;
            set => _xboxGameProcesses = value;
        }

        public override bool IsRunning
        {
            get
            {
                //int numGameProcesses = 0;
                return !ProcessUtils.ProcessExited(_xboxGameProcessName);
                /*_xboxGameProcesses = Process.GetProcessesByName(_xboxGameProcessName).ToList();
                foreach (Process gameProcess in _xboxGameProcesses)
                {
                    try
                    {
                        //if (gameProcess.MainModule.FileName.StartsWith(_xboxGameExePath))
                        //    numGameProcesses++;
                        if (!gameProcess.HasExited)
                        {
                            numGameProcesses++;
                        }
                            
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex, $"XboxGame/IsRunning: Accessing Process.MainModule caused exception. Trying GameUtils.GetMainModuleFilepath instead");

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
                            if (filePath.StartsWith(_xboxGameExePath))
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

        // TODO: Implement Xbox Game Pass update detection
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
                if (!String.IsNullOrWhiteSpace(_xboxGameExePath))
                    return File.Exists(_xboxGameExePath);
                // For UWP-only Xbox games with no exe path, trust the AUMID presence
                return !String.IsNullOrWhiteSpace(_xboxGameAUMID);
            }
        }

        public bool CopyTo(XboxGame xboxGame)
        {
            if (xboxGame == null)
                return false;

            // Copy ALL identity data components to preserve tracking validity across references
            xboxGame.IconPath = IconPath;
            xboxGame.Id = Id;
            xboxGame.Name = Name;
            xboxGame.ExePath = ExePath;
            xboxGame.Directory = Directory;
            xboxGame.Executable = Executable;
            xboxGame.ProcessName = ProcessName;
            xboxGame.AUMID = AUMID;
            return true;
        }

        public override string ToString()
        {
            var name = _xboxGameName;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Unknown";
            }

            if (IsRunning)
            {
                return name + " " + "[Running]";
            }

            if (IsUpdating)
            {
                return name + " " + "[Updating]";
            }

            return name;
        }

        public override bool Start(out List<Process> processesStarted, string gameArguments = "", ProcessPriority priority = ProcessPriority.Normal, int timeout = 20, bool runExeAsAdmin = false)
        {
            processesStarted = new List<Process>();

            // CASE 1: Launch modern sandboxed MSIX/UWP package via AUMID
            if (!String.IsNullOrWhiteSpace(_xboxGameAUMID))
            {
                if (!string.IsNullOrWhiteSpace(gameArguments))
                {
                    logger.Warn($"XboxGame/Start: Arguments '{gameArguments}' were defined for {Name}. Sandboxed UWP/AppX applications launched via shell AUMID parameters do not support custom runtime command forwarding. Arguments will be bypassed.");
                }

                string explorerExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
                string launchArgs = $"shell:AppsFolder\\{_xboxGameAUMID}";
                
                logger.Info($"XboxGame/Start: Launching sandboxed application container via Shell AUMID wrapper: {launchArgs}");
                
                // This fires the call to the shell environment
                ProcessUtils.StartProcess(explorerExe, launchArgs, priority, timeout, runExeAsAdmin);
            }
            // CASE 2: Fall back to unsealed Win32 runtime executable pathway if no AUMID is resolved
            else
            {
                logger.Info($"XboxGame/Start: No AUMID discovered. Attemping direct fallback execution sequence for target path: {ExePath}");
                var directProcesses = ProcessUtils.StartProcess(ExePath, gameArguments, priority, timeout, runExeAsAdmin);
                if (directProcesses != null && directProcesses.Count > 0)
                {
                    processesStarted.AddRange(directProcesses);
                }
            }

            // MANDATORY SAFEGUARD: Shell execution returns instantly without generating a direct process handle token.
            // Query the active OS process tree by name immediately so DisplayMagician captures tracking control.
            if (processesStarted.Count == 0 && !string.IsNullOrWhiteSpace(_xboxGameProcessName))
            {
                // Allow a brief moment for the sandboxed package thread to deploy into the active window tree
                System.Threading.Thread.Sleep(500);
                
                var activeGameProcesses = Process.GetProcessesByName(_xboxGameProcessName).ToList();
                if (activeGameProcesses.Count > 0)
                {
                    logger.Debug($"XboxGame/Start: Successfully bound active background application process hook context for '{_xboxGameProcessName}'.");
                    processesStarted.AddRange(activeGameProcesses);
                }
            }

            return true;
        }

        public override bool Stop()
        {
            logger.Info($"XboxGame/Stop: Request received to stop {Name} (Process Name: {_xboxGameProcessName})");
            bool allStopped = true;

            try
            {
                // Step 1: Drain explicitly tracked tracking list items via ProcessUtils
                if (Processes != null && Processes.Count > 0)
                {
                    allStopped = ProcessUtils.StopProcess(Processes);
                    Processes.Clear();
                }

                // Step 2: System-wide background name query sweep fallback (Critical for UWP app hooks)
                if (!string.IsNullOrWhiteSpace(_xboxGameProcessName))
                {
                    var runningInstances = Process.GetProcessesByName(_xboxGameProcessName).ToList();
                    if (runningInstances.Count > 0)
                    {
                        logger.Debug($"XboxGame/Stop: Clearing {runningInstances.Count} remaining untracked sandboxed instances of '{_xboxGameProcessName}'.");
                        bool backupStopped = ProcessUtils.StopProcess(runningInstances);
                        allStopped = allStopped && backupStopped;
                    }
                }

                return allStopped;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"XboxGame/Stop: Exception encountered while terminating sandboxed package execution for {Name}");
                return false;
            }
        }

    }
}