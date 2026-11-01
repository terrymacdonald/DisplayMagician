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
            if (!input.CanSeek) throw new InvalidDataException("The Steam app-info stream must support seeking.");
            var magic = reader.ReadUInt32();
            if (magic != Magic && magic != Magic28 && magic != Magic29)
                throw new InvalidDataException($"Unknown magic header: {magic:X}");

            EUniverse universe = (EUniverse)reader.ReadUInt32();
            List<App> apps = new List<App>();
            var options = new KVSerializerOptions();
            long recordsEnd = input.Length;
            if (magic == Magic29)
            {
                long stringTableOffset = reader.ReadInt64();
                long recordsStart = input.Position;
                if (stringTableOffset < recordsStart + sizeof(uint) || stringTableOffset > input.Length - sizeof(uint))
                    throw new InvalidDataException("The Steam string-table offset is outside the file's record/table boundaries.");
                recordsEnd = stringTableOffset;
                input.Position = stringTableOffset;
                uint stringCount = reader.ReadUInt32();
                // Every string needs at least its null terminator. Check before allocating.
                if (stringCount > int.MaxValue || stringCount > input.Length - input.Position)
                    throw new InvalidDataException("The Steam string-table count exceeds the available data.");
                var stringTable = new StringTable((int)stringCount);
                for (uint i = 0; i < stringCount; i++)
                    stringTable.Add(ReadNullTermUtf8String(input));
                input.Position = recordsStart;
                options.StringTable = stringTable;
            }

            var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);
            while (true)
            {
                if (recordsEnd - input.Position < sizeof(uint))
                    throw new InvalidDataException("The Steam app-info records have no complete end marker.");
                uint appid = reader.ReadUInt32();
                if (appid == 0) break;
                if (recordsEnd - input.Position < sizeof(uint))
                    throw new InvalidDataException("The Steam app-info record size is truncated.");
                uint size = reader.ReadUInt32();
                int headerSize = magic == Magic ? 40 : 60;
                if (size <= headerSize || size > recordsEnd - input.Position || size - headerSize > int.MaxValue)
                    throw new InvalidDataException("The Steam app-info record length exceeds its available data.");
                var app = new App
                {
                    AppID = appid,
                    InfoState = reader.ReadUInt32(),
                    LastUpdated = DateTimeFromUnixTime(reader.ReadUInt32()),
                    Token = reader.ReadUInt64(),
                    Hash = new ReadOnlyCollection<byte>(reader.ReadBytes(20)),
                    ChangeNumber = reader.ReadUInt32()
                };
                if (magic == Magic28 || magic == Magic29)
                    app.BinaryDataHash = new ReadOnlyCollection<byte>(reader.ReadBytes(20));

                int dataSize = checked((int)(size - headerSize));
                byte[] bytes = reader.ReadBytes(dataSize);
                if (bytes.Length != dataSize) throw new EndOfStreamException("The Steam app-info record changed or was truncated while reading.");
                // Isolate the declared record so malformed VDF cannot consume the next one.
                using MemoryStream data = new MemoryStream(bytes, writable: false);
                app.Data = deserializer.Deserialize(data, options);
                if (data.Position != data.Length) throw new InvalidDataException("The Steam app-info record has trailing data.");
                apps.Add(app);
            }
            Universe = universe;
            Apps = apps;
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

                    if (b < 0) throw new EndOfStreamException("The Steam string table contains an unterminated string.");
                    if (b == 0)
                    {
                        break;
                    }

                    if (position >= buffer.Length)
                    {
                        if (buffer.Length > int.MaxValue / 2) throw new InvalidDataException("The Steam string table contains an oversized string.");
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