using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagician.GameLibraries
{
    public class Game
    {

        #region Properties
        public virtual int Id { get; set; }

        public virtual SupportedGameLibrary GameLibrary { get; }

        public virtual bool IsRunning { get; set; }

        public virtual bool IsUpdating { get; set; }

        public virtual string Name { get; set; }

        public virtual string ExePath { get; set; }

        public virtual string IconPath { get; set; }

        public virtual string Directory { get; set; }

        public virtual string Executable { get; set; }

        public virtual string ProcessName { get; set; }

        #endregion


        #region Methods
        public virtual bool CopyTo(SteamGame steamGame)
        {
            return true;
        }
        #endregion
    }
}
