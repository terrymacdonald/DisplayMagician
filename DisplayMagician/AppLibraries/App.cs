using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;

namespace DisplayMagician.AppLibraries
{
    public class App
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Properties
        public virtual string Id { get; set; }

        public virtual SupportedAppLibraryType AppLibrary { get; }

        public virtual bool IsRunning { get; set; }

        public virtual bool IsUpdating { get; set; }

        public virtual string Name { get; set; }

        public virtual string ExePath { get; set; }

        public virtual string IconPath { get; set; }

        public virtual string Directory { get; set; }

        public virtual string Executable { get; set; }

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

       
        #endregion
    }
}
