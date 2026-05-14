using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using DisplayMagician.GameLibraries;
using System.ComponentModel;
using Newtonsoft.Json;
using Windows.System.Preview;

namespace DisplayMagician.AppLibraries
{
    public class App
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public App()
        {
            // put in some sensible defaults for a blank App
            Id = "";
            Name = "";
            ExePath = "";
            Arguments = "";
            IconPath = "";
            Directory = "";
            ProcessName = "";
            AppBitmap = new ShortcutBitmap() { };
            AvailableAppBitmaps = new List<ShortcutBitmap>() { };
        }

        #region Properties
        [DefaultValue("")]
        public virtual string Id { get;set; }

        [DefaultValue(SupportedAppLibraryType.Unknown)]
        public virtual SupportedAppLibraryType AppLibraryType { get; }

        [JsonIgnore]
        public virtual AppLibrary AppLibrary { get; }

        [JsonIgnore]
        public virtual bool IsRunning { get; set; }

        [JsonIgnore]
        public virtual bool IsUpdating { get; set; }

        [JsonIgnore]
        public virtual bool IsInstalled { get; set; }

        [DefaultValue("")]
        public virtual string Name { get; set; }

        [DefaultValue("")]
        public virtual string ExePath { get; set; }

        [DefaultValue("")]
        public virtual bool ExecutableArgumentsRequired { get; set; }

        [DefaultValue("")]
        public virtual string Arguments { get; set; }

        [DefaultValue("")]
        public virtual string IconPath { get; set; }

        [DefaultValue("")]
        public virtual string Directory { get; set; }

        [DefaultValue("")]
        public virtual string ProcessName { get; set; }

        [JsonIgnore]
        public virtual List<Process> Processes { get; set; }
       
        public ShortcutBitmap AppBitmap { get; set; }

        public List<ShortcutBitmap> AvailableAppBitmaps { get; set; }

        #endregion


        #region Methods

        public virtual bool CopyTo(App steamApp)
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
