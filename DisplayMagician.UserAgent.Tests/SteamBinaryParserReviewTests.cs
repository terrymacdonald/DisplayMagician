using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DisplayMagician.GameLibraries.SteamAppInfoParser;
using ValveKeyValue;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class SteamBinaryParserReviewTests
{
    [Theory]
    [InlineData(0x27)]
    [InlineData(0x28)]
    [InlineData(0x29)]
    public void SupportedAppFormatsReplacePreviousResultsOnlyAfterSuccessfulReads(int format)
    {
        AppInfo parser = new();
        byte[] valid = AppFile(format);
        parser.Read(new MemoryStream(valid));
        Assert.Equal(42u, Assert.Single(parser.Apps).AppID);
        Assert.NotNull(parser.Apps[0].Data);
        Assert.Equal("Fixture", (string)parser.Apps[0].Data!["common"]["name"]);
        parser.Read(new MemoryStream(valid));
        Assert.Single(parser.Apps);
        List<App> saved = parser.Apps;
        EUniverse universe = parser.Universe;
        byte[] truncated = valid[..(format == 0x29 ? checked((int)BitConverter.ToInt64(valid, 8)) - 2 : valid.Length - 2)];
        Assert.Throws<InvalidDataException>(() => parser.Read(new MemoryStream(truncated)));
        Assert.Same(saved, parser.Apps);
        Assert.Equal(universe, parser.Universe);
    }

    [Theory]
    [InlineData(-1L)]
    [InlineData(0L)]
    [InlineData(16L)]
    [InlineData(long.MaxValue)]
    public void InvalidStringTableOffsetsAreRejectedBeforeSeekingOrAllocating(long offset)
    {
        byte[] bytes = AppFile(0x29);
        BitConverter.GetBytes(offset).CopyTo(bytes, 8);
        Assert.Throws<InvalidDataException>(() => new AppInfo().Read(new MemoryStream(bytes)));
    }

    [Theory]
    [InlineData(uint.MaxValue)]
    [InlineData(1000000u)]
    public void ImpossibleStringCountsFailBeforeCreatingTheTable(uint count)
    {
        byte[] bytes = AppFile(0x29);
        int table = checked((int)BitConverter.ToInt64(bytes, 8));
        BitConverter.GetBytes(count).CopyTo(bytes, table);
        Assert.Throws<InvalidDataException>(() => new AppInfo().Read(new MemoryStream(bytes)));
    }

    [Fact]
    public void StringTableEntriesRequireANullTerminator()
    {
        byte[] valid = AppFile(0x29);
        Assert.Throws<EndOfStreamException>(() => new AppInfo().Read(new MemoryStream(valid[..^1])));
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(39u)]
    [InlineData(40u)]
    [InlineData(uint.MaxValue)]
    public void InvalidAppRecordSizesAreRejected(uint size)
    {
        byte[] bytes = AppFile(0x27);
        BitConverter.GetBytes(size).CopyTo(bytes, 12);
        Assert.Throws<InvalidDataException>(() => new AppInfo().Read(new MemoryStream(bytes)));
    }

    [Fact]
    public void AppRecordsCannotOverlapTheStringTable()
    {
        byte[] bytes = AppFile(0x29);
        uint overlap = checked((uint)(BitConverter.ToInt64(bytes, 8) - 24 + 1));
        BitConverter.GetBytes(overlap).CopyTo(bytes, 20);
        Assert.Throws<InvalidDataException>(() => new AppInfo().Read(new MemoryStream(bytes)));
    }

    [Fact]
    public void MalformedVdfCannotReadPastTheDeclaredRecordBoundary()
    {
        byte[] bytes = AppFile(0x27);
        BitConverter.GetBytes(41u).CopyTo(bytes, 12);
        PositionOnDisposeStream input = new(bytes);
        Exception? failure = Record.Exception(() => new AppInfo().Read(input));
        KeyValueException error = Assert.IsType<KeyValueException>(failure);
        Assert.IsType<EndOfStreamException>(error.InnerException);
        Assert.Equal(57, input.DisposedPosition);
        Assert.False(input.CanRead);
    }

    [Fact]
    public void ACompleteFirstRecordIsNotPublishedWhenTheNextRecordIsTruncated()
    {
        byte[] valid = AppFile(0x27);
        using MemoryStream file = new();
        file.Write(valid.AsSpan(0, valid.Length - 4));
        file.Write(BitConverter.GetBytes(84u));
        file.Write(BitConverter.GetBytes(100u));
        AppInfo parser = new();
        parser.Read(new MemoryStream(valid));
        List<App> saved = parser.Apps;
        Assert.Throws<InvalidDataException>(() => parser.Read(new MemoryStream(file.ToArray())));
        Assert.Same(saved, parser.Apps);
        Assert.Equal(42u, Assert.Single(parser.Apps).AppID);
    }

    [Theory]
    [InlineData(0x27)]
    [InlineData(0x28)]
    public void SupportedPackageFormatsReplaceRatherThanAppendAndPreserveResultsOnFailure(int format)
    {
        byte[] valid = PackageFile(format);
        PackageInfo parser = new();
        parser.Read(new MemoryStream(valid));
        parser.Read(new MemoryStream(valid));
        Assert.Equal(42u, Assert.Single(parser.Packages).SubID);
        Assert.Equal("Fixture", (string)parser.Packages[0].Data!["common"]["name"]);
        List<Package> saved = parser.Packages;
        EUniverse universe = parser.Universe;
        Assert.ThrowsAny<IOException>(() => parser.Read(new MemoryStream(valid[..^2])));
        Assert.Same(saved, parser.Packages);
        Assert.Equal(universe, parser.Universe);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnknownFormatsReleaseTheFileAndKeepExistingParserState(bool package)
    {
        string path = Path.Combine(Path.GetTempPath(), "DisplayMagician-BinaryParser-" + Guid.NewGuid().ToString("N") + ".vdf");
        try
        {
            File.WriteAllBytes(path, new byte[8]);
            if (package) Assert.Throws<InvalidDataException>(() => new PackageInfo().Read(path));
            else Assert.Throws<InvalidDataException>(() => new AppInfo().Read(path));
            using FileStream exclusive = new(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.Equal(8, exclusive.Length);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(7)]
    public void TruncatedHeadersAreRejectedAndInputIsClosed(int length)
    {
        PositionOnDisposeStream app = new(AppFile(0x27)[..length]);
        PositionOnDisposeStream package = new(PackageFile(0x28)[..length]);
        Assert.ThrowsAny<IOException>(() => new AppInfo().Read(app));
        Assert.ThrowsAny<IOException>(() => new PackageInfo().Read(package));
        Assert.False(app.CanRead);
        Assert.False(package.CanRead);
    }

    private static byte[] AppFile(int format)
    {
        StringTable? table = format == 0x29 ? new StringTable(new[] { "appinfo", "common", "name" }) : null;
        byte[] vdf = Vdf("appinfo", table);
        using MemoryStream file = new();
        using BinaryWriter writer = new(file, Encoding.UTF8, leaveOpen: true);
        writer.Write(0x07564400u | (uint)format);
        writer.Write(1u);
        if (format == 0x29) writer.Write(0L);
        writer.Write(42u);
        writer.Write(checked((uint)((format == 0x27 ? 40 : 60) + vdf.Length)));
        writer.Write(1u);
        writer.Write(1u);
        writer.Write(7UL);
        writer.Write(new byte[20]);
        writer.Write(5u);
        if (format != 0x27) writer.Write(new byte[20]);
        writer.Write(vdf);
        writer.Write(0u);
        if (table != null)
        {
            long offset = file.Position;
            string[] strings = table.ToArray();
            writer.Write(checked((uint)strings.Length));
            foreach (string value in strings)
            {
                writer.Write(Encoding.UTF8.GetBytes(value));
                writer.Write((byte)0);
            }
            file.Position = 8;
            writer.Write(offset);
        }
        return file.ToArray();
    }

    private static byte[] PackageFile(int format)
    {
        using MemoryStream file = new();
        using BinaryWriter writer = new(file, Encoding.UTF8, leaveOpen: true);
        writer.Write(0x06565500u | (uint)format);
        writer.Write(1u);
        writer.Write(42u);
        writer.Write(new byte[20]);
        writer.Write(5u);
        if (format != 0x27) writer.Write(7UL);
        writer.Write(Vdf("packageinfo", null));
        writer.Write(uint.MaxValue);
        return file.ToArray();
    }

    private static byte[] Vdf(string root, StringTable? table)
    {
        using MemoryStream text = new(Encoding.UTF8.GetBytes("\"" + root + "\" { \"common\" { \"name\" \"Fixture\" } }"));
        var document = KVSerializer.Create(KVSerializationFormat.KeyValues1Text).Deserialize(text);
        using MemoryStream binary = new();
        KVSerializer.Create(KVSerializationFormat.KeyValues1Binary).Serialize(binary, document, new KVSerializerOptions { StringTable = table });
        return binary.ToArray();
    }

    private sealed class PositionOnDisposeStream(byte[] bytes) : MemoryStream(bytes)
    {
        public long DisposedPosition { get; private set; }
        protected override void Dispose(bool disposing)
        {
            if (CanRead) DisposedPosition = Position;
            base.Dispose(disposing);
        }
    }
}
