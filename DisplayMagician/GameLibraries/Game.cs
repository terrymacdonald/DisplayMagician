namespace DisplayMagician.GameLibraries
{
    public class Game
    {

        public enum GameStartMode
        {
            URI
        }



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

        #endregion


        #region Methods
        public virtual string GetStartURI(string gameArguments = "")
        {
            return "";
        }

        public virtual bool CopyTo(SteamGame steamGame)
        {
            return true;
        }
        #endregion
    }
}
