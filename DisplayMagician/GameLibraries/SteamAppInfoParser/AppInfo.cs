using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValveKeyValue;

namespace DisplayMagician.GameLibraries.SteamAppInfoParser
{
    class AppInfo
    {
        private const uint Magic = 0x07_56_44_27;

        public EUniverse Universe { get; set; }

        public List<App> Apps { get; set; } = new List<App>();

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

            if (magic != Magic)
            {
                throw new InvalidDataException($"Unknown magic header: {magic}");
            }

            Universe = (EUniverse)reader.ReadUInt32();

            var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);

            do
            {
                var appid = reader.ReadUInt32();

                if (appid == 0)
                {
                    break;
                }

                var app = new App
                {
                    AppID = appid,
                    Size = reader.ReadUInt32(),
                    InfoState = reader.ReadUInt32(),
                    LastUpdated = DateTimeFromUnixTime(reader.ReadUInt32()),
                    Token = reader.ReadUInt64(),
                    Hash = new ReadOnlyCollection<byte>(reader.ReadBytes(20)),
                    ChangeNumber = reader.ReadUInt32(),
                    Data = deserializer.Deserialize(input),
                };

                Apps.Add(app);
            } while (true);
        }

        public static DateTime DateTimeFromUnixTime(uint unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
        }
    }
}
