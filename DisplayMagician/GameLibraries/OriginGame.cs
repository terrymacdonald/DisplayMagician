using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DisplayMagician.Resources;
using System.Diagnostics;

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
        private string _originGameIconPath;
        //private string _originURI;
        private static readonly OriginLibrary _originGameLibrary = OriginLibrary.GetLibrary();
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static OriginGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }


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

        public override SupportedGameLibraryType GameLibrary
        {
            get => SupportedGameLibraryType.Origin;
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

        public override bool IsRunning
        {
            get
            {
                int numGameProcesses = 0;
                List<Process> gameProcesses = Process.GetProcessesByName(_originGameProcessName).ToList();
                foreach (Process gameProcess in gameProcesses)
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
                    return false;
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

        public override GameStartMode StartMode
        {
            get => GameStartMode.URI;
        }

        public override string GetStartURI(string gameArguments = "")
        {
            string address = $"origin2://game/launch?offerIds={_originGameId}";
            if (String.IsNullOrWhiteSpace(gameArguments))
            {
                address += "/" + gameArguments;
            }
            return address;
        }

        public bool CopyTo(OriginGame OriginGame)
        {
            if (!(OriginGame is OriginGame))
                return false;

            // Copy all the game data over to the other game
            OriginGame.IconPath = IconPath;
            OriginGame.Id = Id;
            OriginGame.Name = Name;
            OriginGame.ExePath = ExePath;
            OriginGame.Directory = Directory;
            return true;
        }

        public override string ToString()
        {
            var name = _originGameName;

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