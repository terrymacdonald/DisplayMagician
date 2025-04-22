using DisplayMagician.Processes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;

namespace DisplayMagician.GameLibraries
{
    public class Game
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Game()
        {
            // put in some sensible defaults for a blank App
            Id = "";
            Name = "";
            ExePath = "";
            IconPath = "";
            Directory = "";
            Executable = "";
            ProcessName = "";
            GameBitmap = new ShortcutBitmap() { };
            AvailableGameBitmaps = new List<ShortcutBitmap>() { };
        }

        #region Properties
        public virtual string Id { get; set; }

        public virtual SupportedGameLibraryType GameLibraryType { get; }

        [JsonIgnore]
        public virtual GameLibrary GameLibrary { get; }

        [JsonIgnore]
        public virtual bool IsRunning { get; set; }

        [JsonIgnore]
        public virtual bool IsUpdating { get; set; }

        [JsonIgnore]
        public virtual bool IsInstalled { get; set; }

        public virtual string Name { get; set; }

        public virtual string ExePath { get; set; }

        public virtual string IconPath { get; set; }

        public virtual string Directory { get; set; }

        public virtual string Executable { get; set; }

        public virtual string ProcessName { get; set; }

        [JsonIgnore]
        public virtual List<Process> Processes { get; set; }

        public ShortcutBitmap GameBitmap { get; set; }

        public List<ShortcutBitmap> AvailableGameBitmaps { get; set; }

        #endregion


        #region Methods

        public virtual bool CopyTo(Game steamGame)
        {
            return true;
        }

        public virtual bool Start(out List<Process> processesStarted, string gameArguments = "", ProcessPriority priority = ProcessPriority.Normal, int timeout = 20, bool runExeAsAdmin = false)
        {
            processesStarted = new List<Process>();
            return true;
        }

        public virtual bool Stop()
        {
            return true;
        }

        #endregion
    }
}
