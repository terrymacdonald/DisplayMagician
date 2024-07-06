using DisplayMagician.GameLibraries.SteamAppInfoParser;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using ValveKeyValue;

// Code used from SteamDatabase team at https://github.com/SteamDatabase/SteamAppInfo/tree/master/SteamAppInfoParser

namespace DisplayMagician.GameLibraries.SteamAppInfoParser
{

    class AppInfo
    {
        private const uint Magic29 = 0x07_56_44_29;
        private const uint Magic28 = 0x07_56_44_28;
        private const uint Magic = 0x07_56_44_27;

        public EUniverse Universe { get; set; }

        public List<App> Apps { get; set; } = [];

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

            if (magic != Magic && magic != Magic28 && magic != Magic29)
            {
                throw new InvalidDataException($"Unknown magic header: {magic:X}");
            }

            Universe = (EUniverse)reader.ReadUInt32();

            var options = new KVSerializerOptions();

            if (magic == Magic29)
            {
                var stringTableOffset = reader.ReadInt64();
                var offset = reader.BaseStream.Position;
                reader.BaseStream.Position = stringTableOffset;
                var stringCount = reader.ReadUInt32();
                var stringTable = new StringTable((int)stringCount);

                for (var i = 0; i < stringCount; i++)
                {
                    stringTable.Add(ReadNullTermUtf8String(reader.BaseStream));
                }

                reader.BaseStream.Position = offset;

                options.StringTable = stringTable;
            }

            var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);

            do
            {
                var appid = reader.ReadUInt32();

                if (appid == 0)
                {
                    break;
                }

                var size = reader.ReadUInt32(); // size until end of Data
                var end = reader.BaseStream.Position + size;

                var app = new App
                {
                    AppID = appid,
                    InfoState = reader.ReadUInt32(),
                    LastUpdated = DateTimeFromUnixTime(reader.ReadUInt32()),
                    Token = reader.ReadUInt64(),
                    Hash = new ReadOnlyCollection<byte>(reader.ReadBytes(20)),
                    ChangeNumber = reader.ReadUInt32(),
                };

                if (magic == Magic28 || magic == Magic29)
                {
                    app.BinaryDataHash = new ReadOnlyCollection<byte>(reader.ReadBytes(20));
                }

                app.Data = deserializer.Deserialize(input, options);

                if (reader.BaseStream.Position != end)
                {
                    throw new InvalidDataException();
                }

                Apps.Add(app);
            } while (true);
        }

        private static DateTime DateTimeFromUnixTime(uint unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
        }
    }*/

    class AppInfo
    {
        private const uint Magic29 = 0x07_56_44_29;
        private const uint Magic28 = 0x07_56_44_28;
        private const uint Magic = 0x07_56_44_27;

        public EUniverse Universe { get; set; }

        public List<App> Apps { get; set; } = [];

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

            if (magic != Magic && magic != Magic28 && magic != Magic29)
            {
                throw new InvalidDataException($"Unknown magic header: {magic:X}");
            }

            Universe = (EUniverse)reader.ReadUInt32();

            var options = new KVSerializerOptions();

            if (magic == Magic29)
            {
                var stringTableOffset = reader.ReadInt64();
                var offset = reader.BaseStream.Position;
                reader.BaseStream.Position = stringTableOffset;
                var stringCount = reader.ReadUInt32();
                var stringPool = new string[stringCount];

                for (var i = 0; i < stringCount; i++)
                {
                    stringPool[i] = ReadNullTermUtf8String(reader.BaseStream);
                }

                reader.BaseStream.Position = offset;

                options.StringPool = stringPool;
            }

            var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);

            do
            {
                var appid = reader.ReadUInt32();

                if (appid == 0)
                {
                    break;
                }

                var size = reader.ReadUInt32(); // size until end of Data
                var end = reader.BaseStream.Position + size;

                var app = new App
                {
                    AppID = appid,
                    InfoState = reader.ReadUInt32(),
                    LastUpdated = DateTimeFromUnixTime(reader.ReadUInt32()),
                    Token = reader.ReadUInt64(),
                    Hash = new ReadOnlyCollection<byte>(reader.ReadBytes(20)),
                    ChangeNumber = reader.ReadUInt32(),
                };

                if (magic == Magic28 || magic == Magic29)
                {
                    app.BinaryDataHash = new ReadOnlyCollection<byte>(reader.ReadBytes(20));
                }

                app.Data = deserializer.Deserialize(input, options);

                if (reader.BaseStream.Position != end)
                {
                    throw new InvalidDataException();
                }

                Apps.Add(app);
            } while (true);
        }

        private static DateTime DateTimeFromUnixTime(uint unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
        }

        private static string ReadNullTermUtf8String(Stream stream)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(32);

            try
            {
                var position = 0;

                do
                {
                    var b = stream.ReadByte();

                    if (b <= 0) // null byte or stream ended
                    {
                        break;
                    }

                    if (position >= buffer.Length)
                    {
                        var newBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length * 2);
                        Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
                        ArrayPool<byte>.Shared.Return(buffer);
                        buffer = newBuffer;
                    }

                    buffer[position++] = (byte)b;
                }
                while (true);

                return Encoding.UTF8.GetString(buffer[..position]);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}