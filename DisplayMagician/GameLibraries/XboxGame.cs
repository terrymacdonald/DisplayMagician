using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using DisplayMagician.Resources;
using Microsoft.Win32;
using System.Diagnostics;
using DisplayMagician.Processes;
using Newtonsoft.Json;

namespace DisplayMagician.GameLibraries
{
    public class XboxGame : Game
    {
        private string _gameRegistryKey;
        private string _xboxGameId;
        private string _xboxGameName;
        private string _xboxGameExePath;
        private string _xboxGameDir;
        private string _xboxGameExe;
        private string _xboxGameProcessName;
        private List<Process> _xboxGameProcesses = new List<Process>();
        private string _xboxGameIconPath;
        private static readonly SteamLibrary _xboxGameLibrary = SteamLibrary.GetLibrary();
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static XboxGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }

        public XboxGame() { }

        public XboxGame(string xboxGameId, string xboxGameName, string xboxGameExePath, string xboxGameIconPath)
        {

            _gameRegistryKey = $@"{_xboxGameLibrary.SteamAppsRegistryKey}\\{xboxGameId}";
            _xboxGameId = xboxGameId;
            _xboxGameName = xboxGameName;
            _xboxGameExePath = xboxGameExePath;
            _xboxGameDir = Path.GetDirectoryName(xboxGameExePath);
            _xboxGameExe = Path.GetFileName(_xboxGameExePath);
            _xboxGameProcessName = Path.GetFileNameWithoutExtension(_xboxGameExePath);
            _xboxGameIconPath = xboxGameIconPath;

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
            get => SupportedGameLibraryType.Steam; 
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
                    logger.Warn(ex, $"XboxGame/IsUpdating: SecurityException when trying to open {_gameRegistryKey} registry key");
                }
                catch (IOException ex)
                {
                    logger.Warn(ex, $"XboxGame/IsUpdating: IOException when trying to open {_gameRegistryKey} registry key");
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"XboxGame/IsUpdating: Exception when trying to open {_gameRegistryKey} registry key");
                }
                return false;
            }
        }

        public bool CopyInto(XboxGame xboxGame)
        {
            if (!(xboxGame is XboxGame))
                return false;

            // Copy all the game data over to the other game
            xboxGame.IconPath = IconPath;
            xboxGame.Id = Id;
            xboxGame.Name = Name;
            xboxGame.ExePath = ExePath;
            xboxGame.Directory = Directory;
            return true;
        }

        public override string ToString()
        {
            var name = _xboxGameName;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = Language.Unknown;
            }

            if (IsRunning)
            {
                return name + " " + Language.Running;
            }

            if (IsUpdating)
            {
                return name + " " + Language.Updating;
            }

            return name;
        }

        public override bool Start(out List<Process> processesStarted, string gameArguments = "", ProcessPriority priority = ProcessPriority.Normal, int timeout = 20, bool runExeAsAdmin = false)
        {
            string address = $@"uplay://launch/{Id}";
            if (!String.IsNullOrWhiteSpace(gameArguments))
            {
                address += @"/" + gameArguments;
            }
            processesStarted = ProcessUtils.StartProcess(address, null, priority);
            return true;
        }

        public override bool Stop()
        {
            return true;
        }

    }
}