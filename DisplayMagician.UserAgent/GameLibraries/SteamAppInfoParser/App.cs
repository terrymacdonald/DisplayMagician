using System;
using System.Collections.ObjectModel;
using ValveKeyValue;

namespace DisplayMagician.GameLibraries.SteamAppInfoParser
{
    public class App
    {
        public uint AppID { get; set; }

        public uint InfoState { get; set; }

        public DateTime LastUpdated { get; set; }

        public ulong Token { get; set; }

        public ReadOnlyCollection<byte> Hash { get; set; }
        public ReadOnlyCollection<byte> BinaryDataHash { get; set; }

        public uint ChangeNumber { get; set; }

        public KVObject Data { get; set; }
    }
}