using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValveKeyValue;

namespace HeliosPlus.GameLibraries.SteamAppInfoParser
{
    public class Package
    {
        public uint SubID { get; set; }

        public ReadOnlyCollection<byte> Hash { get; set; }

        public uint ChangeNumber { get; set; }

        public ulong Token { get; set; }

        public KVObject Data { get; set; }
    }
}
