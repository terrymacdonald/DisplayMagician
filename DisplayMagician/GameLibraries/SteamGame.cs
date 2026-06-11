using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
//using DisplayMagician.Resources;
using Microsoft.Win32;
using System.Diagnostics;
using DisplayMagician.Processes;
using Newtonsoft.Json;

namespace DisplayMagician.GameLibraries
{
    public class SteamGame : Game
    {
        private string _gameRegistryKey;
        private string _steamGameId;
        private string _steamGameName;
        private string _steamGameExePath;
        private string _steamGameDir;
        private string _steamGameExe;
        private string _steamGameProcessName;
        private List<Process> _steamGameProcesses = new List<Process>();
        private string _steamGameIconPath;
        private static readonly SteamLibrary _steamGameLibrary = SteamLibrary.GetLibrary();
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public SteamGame(string steamGameId, string steamGameName, string steamGameExePath, string steamGameIconPath)
        {

            _gameRegistryKey = $@"{_steamGameLibrary.SteamAppsRegistryKey}\\{steamGameId}";
            _steamGameId = steamGameId;
            _steamGameName = steamGameName;
            _steamGameExePath = steamGameExePath;
            _steamGameDir = Path.GetDirectoryName(steamGameExePath);
            _steamGameExe = Path.GetFileName(_steamGameExePath);
            _steamGameProcessName = Path.GetFileNameWithoutExtension(_steamGameExePath);
            _steamGameIconPath = steamGameIconPath;

        }

        public override string Id { 
            get => _steamGameId;
            set => _steamGameId = value;
        }

        public override string Name
        {
            get => _steamGameName;
            set => _steamGameName = value;
        }

        public override SupportedGameLibraryType GameLibraryType { 
            get => SupportedGameLibraryType.Steam; 
        }

        [JsonIgnore]
        public override GameLibrary GameLibrary
        {
            get => _steamGameLibrary;
        }

        public override string IconPath { 
            get => _steamGameIconPath; 
            set => _steamGameIconPath = value;
        }

        public override string ExePath
        {
            get => _steamGameExePath;
            set => _steamGameExePath = value;
        }

        public override string Directory
        {
            get => _steamGameDir;
            set => _steamGameDir = value;
        }

        public override string Executable 
        {
            get => _steamGameExe;
            set => _steamGameExe = value;
        }

        public override string ProcessName 
        {
            get => _steamGameProcessName;
            set => _steamGameProcessName = value;
        }

        public override List<Process> Processes
        {
            get => _steamGameProcesses;
            set => _steamGameProcesses = value;
        }

        public override bool IsRunning
        {
            get
            {
                //int numGameProcesses = 0;
                return !ProcessUtils.ProcessExited(_steamGameProcessName);
                /*_steamGameProcesses = Process.GetProcessesByName(_steamGameProcessName).ToList();
                foreach (Process gameProcess in _steamGameProcesses)
                {
                    try
                    {
                        //if (gameProcess.MainModule.FileName.StartsWith(_steamGameExePath))
                        //    numGameProcesses++;
                        if (!gameProcess.HasExited)
                        {
                            numGameProcesses++;
                        }
                            
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex, $"SteamGame/IsRunning: Accessing Process.MainModule caused exception. Trying GameUtils.GetMainModuleFilepath instead");

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
                            if (filePath.StartsWith(_steamGameExePath))
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

        public override bool IsUpdating
        {
            get
            {
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(_gameRegistryKey, RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        if (key != null)
                        {
                            int updateValue;
                            int.TryParse(key.GetValue(@"Updating", 0).ToString(),out updateValue);
                            if (updateValue == 1)
                            {
                                return true;
                            }
                        }                        
                    }
                }
                catch (SecurityException ex)
                {
                    logger.Warn(ex, $"SteamGame/IsUpdating: SecurityException when trying to open {_gameRegistryKey} registry key");
                }
                catch (IOException ex)
                {
                    logger.Warn(ex, $"SteamGame/IsUpdating: IOException when trying to open {_gameRegistryKey} registry key");
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"SteamGame/IsUpdating: Exception when trying to open {_gameRegistryKey} registry key");
                }
                return false;
            }
        }

        public override bool IsInstalled
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_steamGameExePath) && File.Exists(_steamGameExePath);
            }
        }

        public bool CopyTo(SteamGame steamGame)
        {
            if (steamGame == null)
                return false;

            // Copy ALL structural data variables to preserve runtime identity
            steamGame.IconPath = IconPath;
            steamGame.Id = Id;
            steamGame.Name = Name;
            steamGame.ExePath = ExePath;
            steamGame.Directory = Directory;
            steamGame.Executable = Executable;
            steamGame.ProcessName = ProcessName;
            return true;
        }

        public override string ToString()
        {
            var name = _steamGameName;

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

            // CASE 1: Custom arguments provided -> Use steam.exe command-line launch sequence
            if (!string.IsNullOrWhiteSpace(gameArguments))
            {
                string steamExePath = _steamGameLibrary.GameLibraryExe;
                string launchArgs = $"-applaunch {Id} {gameArguments}";
                
                logger.Info($"SteamGame/Start: Arguments detected. Launching via Steam CLI: {steamExePath} {launchArgs}");
                
                var directProcesses = ProcessUtils.StartProcess(steamExePath, launchArgs, priority, timeout, runExeAsAdmin);
                if (directProcesses != null && directProcesses.Count > 0)
                {
                    processesStarted.AddRange(directProcesses);
                }
                return processesStarted.Count > 0;
            }

            // CASE 2: No arguments -> Use standard Windows shell URI Protocol handler
            string address = $@"steam://rungameid/{Id}";
            logger.Info($"SteamGame/Start: No arguments. Requesting standard URI Protocol: {address}");

            var launcherProcesses = ProcessUtils.StartProcess(address, null, priority, timeout, runExeAsAdmin);
            if (launcherProcesses != null && launcherProcesses.Count > 0)
            {
                processesStarted.AddRange(launcherProcesses);
            }

            // SAFEGUARD: Because tracking asynchronous launchers through Steam often drops the initial handle,
            // look up the process instantly by its engine executable name so DisplayMagician captures it.
            if (processesStarted.Count == 0 && !string.IsNullOrWhiteSpace(_steamGameProcessName))
            {
                var activeGameProcesses = Process.GetProcessesByName(_steamGameProcessName).ToList();
                if (activeGameProcesses.Count > 0)
                {
                    logger.Debug($"SteamGame/Start: Captured active background engine process '{_steamGameProcessName}' for tracking.");
                    processesStarted.AddRange(activeGameProcesses);
                }
            }

            return true;
        }

        public override bool Stop()
        {
            logger.Info($"SteamGame/Stop: Request received to stop {Name} (Process Name: {_steamGameProcessName})");
            bool allStopped = true;

            try
            {
                // Step 1: Kill tracked tracking list items via ProcessUtils
                if (Processes != null && Processes.Count > 0)
                {
                    allStopped = ProcessUtils.StopProcess(Processes);
                    Processes.Clear();
                }

                // Step 2: System-wide name query fallback sweep
                if (!string.IsNullOrWhiteSpace(_steamGameProcessName))
                {
                    var runningInstances = Process.GetProcessesByName(_steamGameProcessName).ToList();
                    if (runningInstances.Count > 0)
                    {
                        logger.Debug($"SteamGame/Stop: Clearing {runningInstances.Count} remaining untracked instances of '{_steamGameProcessName}'.");
                        bool backupStopped = ProcessUtils.StopProcess(runningInstances);
                        allStopped = allStopped && backupStopped;
                    }
                }

                return allStopped;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"SteamGame/Stop: Exception encountered while shutting down {Name}");
                return false;
            }
        }

    }
}