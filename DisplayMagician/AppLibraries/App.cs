using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace DisplayMagician.AppLibraries
{
    public class App
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private bool isUWP = false;


        #region Properties
        public virtual string Id { get; set; }

        public virtual SupportedAppLibraryType AppLibrary { get; }

        public virtual bool IsRunning { get; set; }

        public virtual bool IsUpdating { get; set; }

        public virtual string Name { get; set; }

        public virtual string ExePath { get; set; }

        public virtual bool ExecutableArgumentsRequired { get; set; }

        public virtual string Arguments { get; set; }

        public virtual string IconPath { get; set; }

        public virtual string Directory { get; set; }

        public virtual string ProcessName { get; set; }

        public virtual List<Process> Processes { get; set; }

        public ShortcutBitmap AppBitmap { get; set; }

        public List<ShortcutBitmap> AvailableAppBitmaps { get; set; }

        #endregion


        #region Methods

        public virtual bool CopyTo(App steamApp)
        {
            return true;
        }

        public virtual bool Start(ProcessPriority priority, int timeout, bool runExeAsAdmin, out List<Process> processesStarted)
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
