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

        static SteamGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }


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

        public override SupportedGameLibraryType GameLibrary { 
            get => SupportedGameLibraryType.Steam; 
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

        public bool CopyInto(SteamGame steamGame)
        {
            if (!(steamGame is SteamGame))
                return false;

            // Copy all the game data over to the other game
            steamGame.IconPath = IconPath;
            steamGame.Id = Id;
            steamGame.Name = Name;
            steamGame.ExePath = ExePath;
            steamGame.Directory = Directory;
            return true;
        }

        public override string ToString()
        {
            var name = _steamGameName;

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

    }
}