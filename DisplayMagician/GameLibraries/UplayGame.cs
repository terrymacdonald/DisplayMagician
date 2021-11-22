using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DisplayMagician.Resources;
using System.Diagnostics;
using DisplayMagician.Processes;

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

        static UplayGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }


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

        public override SupportedGameLibraryType GameLibrary
        {
            get => SupportedGameLibraryType.Uplay;
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
                return ProcessUtils.ProcessExited(_uplayGameProcessName);
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

        public bool CopyTo(UplayGame uplayGame)
        {
            if (!(uplayGame is UplayGame))
                return false;

            // Copy all the game data over to the other game
            uplayGame.IconPath = IconPath;
            uplayGame.Id = Id;
            uplayGame.Name = Name;
            uplayGame.ExePath = ExePath;
            uplayGame.Directory = Directory;
            return true;
        }

        public override string ToString()
        {
            var name = _uplayGameName;

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

    }
}