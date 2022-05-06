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
    public class XboxGame : Game
    {
        private string _XboxGameId;
        private string _XboxGameName;
        private string _XboxGameExePath;
        private string _XboxGameDir;
        private string _XboxGameExe;
        private string _XboxGameProcessName;
        private List<Process> _XboxGameProcesses = new List<Process>();
        private string _XboxGameIconPath;
        //private string _gogURI;
        private static readonly XboxLibrary _XboxGameLibrary = XboxLibrary.GetLibrary();
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static XboxGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }

        public XboxGame() { }

        public XboxGame(string XboxGameId, string XboxGameName, string XboxGameExePath, string XboxGameIconPath)
        {

            //_gameRegistryKey = $@"{XboxLibrary.registryGogInstallsKey}\\{XboxGameId}";
            _XboxGameId = XboxGameId;
            _XboxGameName = XboxGameName;
            _XboxGameExePath = XboxGameExePath;
            _XboxGameDir = Path.GetDirectoryName(XboxGameExePath);
            _XboxGameExe = Path.GetFileName(_XboxGameExePath);
            _XboxGameProcessName = Path.GetFileNameWithoutExtension(_XboxGameExePath);
            _XboxGameIconPath = XboxGameIconPath;

        }

        public override string Id
        {
            get => _XboxGameId;
            set => _XboxGameId = value;
        }

        public override string Name
        {
            get => _XboxGameName;
            set => _XboxGameName = value;
        }

        public override SupportedGameLibraryType GameLibrary
        {
            get => SupportedGameLibraryType.GOG;
        }

        public override string IconPath
        {
            get => _XboxGameIconPath;
            set => _XboxGameIconPath = value;
        }

        public override string ExePath
        {
            get => _XboxGameExePath;
            set => _XboxGameExePath = value;
        }

        public override string Directory
        {
            get => _XboxGameDir;
            set => _XboxGameDir = value;
        }

        public override string Executable
        {
            get => _XboxGameExe;
            set => _XboxGameExe = value;
        }

        public override string ProcessName
        {
            get => _XboxGameProcessName;
            set => _XboxGameProcessName = value;
        }

        public override List<Process> Processes
        {
            get => _XboxGameProcesses;
            set => _XboxGameProcesses = value;
        }

        public override bool IsRunning
        {
            get
            {
                return !ProcessUtils.ProcessExited(_XboxGameProcessName);
                /*int numGameProcesses = 0;
                _XboxGameProcesses = Process.GetProcessesByName(_XboxGameProcessName).ToList();
                foreach (Process gameProcess in _XboxGameProcesses)
                {
                    try
                    {                       
                        if (gameProcess.ProcessName.Equals(_XboxGameProcessName))
                            numGameProcesses++;
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex, $"XboxGame/IsRunning: Accessing Process.ProcessName caused exception. Trying GameUtils.GetMainModuleFilepath instead");
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
                            if (filePath.StartsWith(_XboxGameExePath))
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

        public bool CopyTo(XboxGame XboxGame)
        {
            if (!(XboxGame is XboxGame))
                return false;

            // Copy all the game data over to the other game
            XboxGame.IconPath = IconPath;
            XboxGame.Id = Id;
            XboxGame.Name = Name;
            XboxGame.ExePath = ExePath;
            XboxGame.Directory = Directory;
            return true;
        }

        public override string ToString()
        {
            var name = _XboxGameName;

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