using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using ValveKeyValue;

namespace DisplayMagician.GameLibraries.SteamAppInfoParser
{
    class PackageInfo
    {
        private const uint Magic = 0x06_56_55_28;
        private const uint Magic27 = 0x06_56_55_27;

        public EUniverse Universe { get; set; }

        public List<Package> Packages { get; set; } = new List<Package>();

        /// <summary>
        /// Opens and reads the given filename.
        /// </summary>
        /// <param name="filename">The file to open and read.</param>
        public void Read(string filename)
        {
            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            Read(fs);
        }

        /// <summary>
        /// Reads the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Stream"/> to read from.</param>
        public void Read(Stream input)
        {
            var reader = new BinaryReader(input);
            var magic = reader.ReadUInt32();

            if (magic != Magic && magic != Magic27)
            {
                throw new InvalidDataException($"Unknown magic header: {magic}");
            }

            Universe = (EUniverse)reader.ReadUInt32();

            var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);

            do
            {
                var subid = reader.ReadUInt32();

                if (subid == 0xFFFFFFFF)
                {
                    break;
                }

                var package = new Package
                {
                    SubID = subid,
                    Hash = new ReadOnlyCollection<byte>(reader.ReadBytes(20)),
                    ChangeNumber = reader.ReadUInt32(),
                };

                if (magic != Magic27)
                {
                    package.Token = reader.ReadUInt64();
                }

                package.Data = deserializer.Deserialize(input);

                Packages.Add(package);
            } while (true);
        }
    }
}
