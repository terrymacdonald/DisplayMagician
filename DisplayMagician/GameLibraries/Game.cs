using System;
using System.Drawing;

namespace DisplayMagician.GameLibraries
{
    public class Game
    {

        public enum GameStartMode
        {
            URI
        }

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Properties
        public virtual string Id { get; set; }

        public virtual SupportedGameLibraryType GameLibrary { get; }

        public virtual bool IsRunning { get; set; }

        public virtual bool IsUpdating { get; set; }

        public virtual string Name { get; set; }

        public virtual string ExePath { get; set; }

        public virtual string IconPath { get; set; }

        public virtual string Directory { get; set; }

        public virtual string Executable { get; set; }

        public virtual string ProcessName { get; set; }

        public virtual GameStartMode StartMode { get; set; }

        public Bitmap GameBitmap { get; set; }

        #endregion


        #region Methods
        public virtual string GetStartURI(string gameArguments = "")
        {
            return "";
        }

        public virtual bool CopyTo(Game steamGame)
        {
            return true;
        }
        
        #endregion
    }
}
