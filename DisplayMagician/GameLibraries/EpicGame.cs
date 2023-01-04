using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DisplayMagician.Resources;
using System.Diagnostics;
using DisplayMagician.Processes;
using Newtonsoft.Json;

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
        private List<Process> _epicGameProcesses = new List<Process>();
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

        public override SupportedGameLibraryType GameLibraryType
        {
            get => SupportedGameLibraryType.Epic;
        }

        [JsonIgnore]
        public override GameLibrary GameLibrary
        {
            get => _epicGameLibrary;
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

        public override List<Process> Processes
        {
            get => _epicGameProcesses;
            set => _epicGameProcesses = value;
        }

        public override bool IsRunning
        {
            get
            {
                return !ProcessUtils.ProcessExited(_epicGameProcessName);
            }
        }

        // TODO Have to do much more research to figure out how to detect when Epic is updating a game
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

        public override bool Start(out List<Process> processesStarted, string gameArguments = "", ProcessPriority priority = ProcessPriority.Normal, int timeout = 20, bool runExeAsAdmin = false)
        {
            string address = $@"com.epicgames.launcher://apps/{Id}?action=launch&silent=true";
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