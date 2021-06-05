using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DisplayMagician.Resources;
using System.Diagnostics;

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
        private string _epicGameIconPath;
        //private string _epicURI;
        private static readonly EpicLibrary _epicGameLibrary = EpicLibrary.GetLibrary();
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static EpicGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }

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

        public override SupportedGameLibraryType GameLibrary
        {
            get => SupportedGameLibraryType.Epic;
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

        public override bool IsRunning
        {
            get
            {
                int numGameProcesses = 0;
                List<Process> gameProcesses = Process.GetProcessesByName(_epicGameProcessName).ToList();
                foreach (Process gameProcess in gameProcesses)
                {
                    try
                    {                       
                        if (gameProcess.ProcessName.Equals(_epicGameProcessName))
                            numGameProcesses++;
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex, $"EpicGame/IsRunning: Accessing Process.ProcessName caused exception. Trying GameUtils.GetMainModuleFilepath instead");
                        // If there is a race condition where MainModule isn't available, then we 
                        // instead try the much slower GetMainModuleFilepath (which does the same thing)
                        string filePath = GameUtils.GetMainModuleFilepath(gameProcess.Id);
                        if (filePath == null)
                        {
                            // if we hit this bit then GameUtils.GetMainModuleFilepath failed,
                            // so we just assume that the process is a game process
                            // as it matched the epical process search
                            numGameProcesses++;
                            continue;
                        }
                        else
                        {
                            if (filePath.StartsWith(_epicGameExePath))
                                numGameProcesses++;
                        }
                            
                    }
                }
                if (numGameProcesses > 0)
                    return true;
                else
                    return false;
            }
        }

        // Have to do much more research to figure out how to detect when Epic is updating a game
        public override bool IsUpdating
        {
            get
            {
                return false;
            }
        }

        public bool CopyTo(EpicGame EpicGame)
        {
            if (!(EpicGame is EpicGame))
                return false;

            // Copy all the game data over to the other game
            EpicGame.IconPath = IconPath;
            EpicGame.Id = Id;
            EpicGame.Name = Name;
            EpicGame.ExePath = ExePath;
            EpicGame.Directory = Directory;
            return true;
        }

        public override string ToString()
        {
            var name = _epicGameName;

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