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

        #region Properties
        public virtual string Id { get; set; }

        public virtual SupportedGameLibraryType GameLibraryType { get; }

        [JsonIgnore]
        public virtual GameLibrary GameLibrary { get; }

        public virtual bool IsRunning { get; set; }

        public virtual bool IsUpdating { get; set; }

        public virtual string Name { get; set; }

        public virtual string ExePath { get; set; }

        public virtual string IconPath { get; set; }

        public virtual string Directory { get; set; }

        public virtual string Executable { get; set; }

        public virtual string ProcessName { get; set; }

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
