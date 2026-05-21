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
            if (!(xboxGame is XboxGame))
                return false;

            // Copy all the game data over to the other game
            xboxGame.IconPath = IconPath;
            xboxGame.Id = Id;
            xboxGame.Name = Name;
            xboxGame.ExePath = ExePath;
            xboxGame.Directory = Directory;
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
            if (!String.IsNullOrWhiteSpace(_xboxGameAUMID))
            {
                // Xbox Game Pass games are sandboxed packages; launch via AUMID through explorer.exe
                string explorerExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
                processesStarted = ProcessUtils.StartProcess(explorerExe, $"shell:AppsFolder\\{_xboxGameAUMID}", priority);
            }
            else
            {
                // Fall back to direct exe launch if no AUMID is available
                processesStarted = ProcessUtils.StartProcess(_xboxGameExePath, gameArguments, priority);
            }
            return true;
        }

        public override bool Stop()
        {
            return true;
        }

    }
}