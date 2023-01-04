using DisplayMagician.GameLibraries.SteamAppInfoParser;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using ValveKeyValue;

// Code used from SteamDatabase team at https://github.com/SteamDatabase/SteamAppInfo/tree/master/SteamAppInfoParser

namespace DisplayMagician.GameLibraries.SteamAppInfoParser
{
    class AppInfo
    {
        private const uint Magic28 = 0x07_56_44_28;
        private const uint Magic = 0x07_56_44_27;

        public EUniverse Universe { get; set; }

        public List<App> Apps { get; set; } = new List<App>();

        /// <summary>
        /// Opens and reads the given filename.
        /// </summary>
        /// <param name="filename">The file to open and read.</param>
        public void Read(string filename)
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            Read(fs);
        }

        /// <summary>
        /// Reads the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Stream"/> to read from.</param>
        public void Read(Stream input)
        {
            using var reader = new BinaryReader(input);
            var magic = reader.ReadUInt32();

            if (magic != Magic && magic != Magic28)
            {
                throw new InvalidDataException($"Unknown magic header: {magic:X}");
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

                reader.ReadUInt32(); // size until end of Data

                var app = new App
                {
                    AppID = appid,
                    InfoState = reader.ReadUInt32(),
                    LastUpdated = DateTimeFromUnixTime(reader.ReadUInt32()),
                    Token = reader.ReadUInt64(),
                    Hash = new ReadOnlyCollection<byte>(reader.ReadBytes(20)),
                    ChangeNumber = reader.ReadUInt32(),
                };

                if (magic == Magic28)
                {
                    app.BinaryDataHash = new ReadOnlyCollection<byte>(reader.ReadBytes(20));
                }

                app.Data = deserializer.Deserialize(input);

                Apps.Add(app);
            } while (true);
        }

        public static DateTime DateTimeFromUnixTime(uint unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
        }
    }
}