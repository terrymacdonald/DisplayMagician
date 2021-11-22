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
    public class GogGame : Game
    {
        private string _gogGameId;
        private string _gogGameName;
        private string _gogGameExePath;
        private string _gogGameDir;
        private string _gogGameExe;
        private string _gogGameProcessName;
        private List<Process> _gogGameProcesses = new List<Process>();
        private string _gogGameIconPath;
        //private string _gogURI;
        private static readonly GogLibrary _gogGameLibrary = GogLibrary.GetLibrary();
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static GogGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }

        public GogGame() { }

        public GogGame(string gogGameId, string gogGameName, string gogGameExePath, string gogGameIconPath)
        {

            //_gameRegistryKey = $@"{GogLibrary.registryGogInstallsKey}\\{GogGameId}";
            _gogGameId = gogGameId;
            _gogGameName = gogGameName;
            _gogGameExePath = gogGameExePath;
            _gogGameDir = Path.GetDirectoryName(gogGameExePath);
            _gogGameExe = Path.GetFileName(_gogGameExePath);
            _gogGameProcessName = Path.GetFileNameWithoutExtension(_gogGameExePath);
            _gogGameIconPath = gogGameIconPath;

        }

        public override string Id
        {
            get => _gogGameId;
            set => _gogGameId = value;
        }

        public override string Name
        {
            get => _gogGameName;
            set => _gogGameName = value;
        }

        public override SupportedGameLibraryType GameLibrary
        {
            get => SupportedGameLibraryType.GOG;
        }

        public override string IconPath
        {
            get => _gogGameIconPath;
            set => _gogGameIconPath = value;
        }

        public override string ExePath
        {
            get => _gogGameExePath;
            set => _gogGameExePath = value;
        }

        public override string Directory
        {
            get => _gogGameDir;
            set => _gogGameDir = value;
        }

        public override string Executable
        {
            get => _gogGameExe;
            set => _gogGameExe = value;
        }

        public override string ProcessName
        {
            get => _gogGameProcessName;
            set => _gogGameProcessName = value;
        }

        public override List<Process> Processes
        {
            get => _gogGameProcesses;
            set => _gogGameProcesses = value;
        }

        public override bool IsRunning
        {
            get
            {
                return ProcessUtils.ProcessExited(_gogGameProcessName);
                /*int numGameProcesses = 0;
                _gogGameProcesses = Process.GetProcessesByName(_gogGameProcessName).ToList();
                foreach (Process gameProcess in _gogGameProcesses)
                {
                    try
                    {                       
                        if (gameProcess.ProcessName.Equals(_gogGameProcessName))
                            numGameProcesses++;
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex, $"GogGame/IsRunning: Accessing Process.ProcessName caused exception. Trying GameUtils.GetMainModuleFilepath instead");
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
                            if (filePath.StartsWith(_gogGameExePath))
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

        public bool CopyTo(GogGame GogGame)
        {
            if (!(GogGame is GogGame))
                return false;

            // Copy all the game data over to the other game
            GogGame.IconPath = IconPath;
            GogGame.Id = Id;
            GogGame.Name = Name;
            GogGame.ExePath = ExePath;
            GogGame.Directory = Directory;
            return true;
        }

        public override string ToString()
        {
            var name = _gogGameName;

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