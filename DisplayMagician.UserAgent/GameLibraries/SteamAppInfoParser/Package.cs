using System.Collections.ObjectModel;
using ValveKeyValue;

namespace DisplayMagician.GameLibraries.SteamAppInfoParser
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