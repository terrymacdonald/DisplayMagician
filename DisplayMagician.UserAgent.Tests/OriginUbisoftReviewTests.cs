using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DisplayMagician.GameLibraries;
using Xunit;
using YamlDotNet.Serialization;

namespace DisplayMagician.UserAgent.Tests;

[Collection("Agent repository state")]
public sealed class OriginUbisoftReviewTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-OriginUbisoft", Guid.NewGuid().ToString("N"));
    private readonly List<Game> _previousGames = GameLibrary.AllInstalledGamesInAllLibraries;
    private readonly bool _previousLoaded = GameLibrary.GamesLoaded;

    [Theory]
    [InlineData("en-US", "3.0")]
    [InlineData("de-DE", "3.0")]
    [InlineData("fr-FR", "4.0")]
    public void OriginParsesVersionsIndependentlyOfCultureAndResolvesRelativeExecutables(string culture, string version)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            string executable = OriginGame("OriginGood", version, "bin/game.exe");
            OriginLibrary library = new(_root);
            Assert.True(library.LoadInstalledGames());
            Assert.Equal(executable, Assert.Single(library.AllInstalledGames).ExePath);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OriginVersionTwoIncludesTheExeExtensionAndMatchesParBasenamesLiterally(bool parameterFile)
    {
        string relative = parameterFile ? "bin/Game[1]+.exe" : "bin/game.exe";
        string executable = OriginGame("OriginLegacy", "2.0", relative);
        string installation = Path.Combine(_root, "OriginLegacy");
        Write("OriginLegacy/unrelated.exe", "not the selected executable");
        Write("OriginLegacy/files.mnfst", (parameterFile ? "\"unrelated.exe\" " : "") + "\"" + relative.Replace('/', '\\') + "\"", Encoding.Unicode);
        if (parameterFile) Write("OriginLegacy/Game[1]+.par", "fixture parameter file");
        OriginLibrary library = new(_root);
        Assert.True(library.LoadInstalledGames());
        Assert.Equal(executable, Assert.Single(library.AllInstalledGames).ExePath);
        Assert.StartsWith(installation, executable);
    }

    [Theory]
    [InlineData("manifest")]
    [InlineData("xml")]
    [InlineData("version")]
    [InlineData("missing_installer")]
    [InlineData("locked")]
    public void OriginContinuesOtherPackagesButReportsMetadataFailuresAsIncomplete(string failure)
    {
        OriginGame("OriginBad", "3.0", "game.exe");
        OriginGame("OriginGood", "4.0", "game.exe");
        string xml = Path.Combine(_root, "OriginBad", "__Installer", "installerdata.xml");
        if (failure == "manifest") Write("LocalContent/OriginBad.mfst", "invalid manifest without an install path");
        if (failure == "xml") File.WriteAllText(xml, "<invalid");
        if (failure == "version") File.WriteAllText(xml, "<game manifestVersion=\"not-a-version\" />");
        if (failure == "missing_installer") File.Delete(xml);
        using FileStream? locked = failure == "locked" ? new FileStream(xml, FileMode.Open, FileAccess.Read, FileShare.None) : null;
        OriginLibrary library = new(_root);
        Assert.False(library.LoadInstalledGames());
        Assert.Contains(library.AllInstalledGames, game => game.Id == "OriginGood");
    }

    [Fact]
    public void OriginDownloadRecordsRemainFilteredWithoutMakingTheScanFail()
    {
        Write("LocalContent/OriginDownloading.mfst", "downloading=1");
        OriginLibrary library = new(_root);
        Assert.True(library.LoadInstalledGames());
        Assert.Empty(library.AllInstalledGames);
    }

    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("1.0")]
    [InlineData("4.0")]
    public void InvalidOrMismatchedOriginManifestVersionsAreIncomplete(string version)
    {
        OriginGame("OriginBad", "3.0", "game.exe");
        Write("OriginBad/__Installer/installerdata.xml", "<game manifestVersion=\"" + version + "\" />");
        Assert.False(new OriginLibrary(_root).LoadInstalledGames());
    }

    [Fact]
    public void OriginMissingContentIsAFailureAndReadableEmptyContentSucceeds()
    {
        OriginLibrary library = new(_root);
        Assert.False(library.LoadInstalledGames());
        Directory.CreateDirectory(Path.Combine(_root, "LocalContent"));
        library.AllInstalledGames.Add(new Game { Id = "removed" });
        Assert.True(library.LoadInstalledGames());
        Assert.Empty(library.AllInstalledGames);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UbisoftCanUseAppendOrRegistryBackedInstallations(bool registry)
    {
        UplayLibrary library = Ubisoft(new() { [72] = _root });
        SaveCache(Game(72, Product(72, registry)));
        Assert.True(library.LoadInstalledGames());
        Assert.Equal("72", Assert.Single(library.AllInstalledGames).Id);
    }

    [Theory]
    [InlineData("root: [broken")]
    [InlineData("null")]
    [InlineData("")]
    public void UbisoftInstalledConfigurationFailuresAreReportedEvenWithoutARootPrefix(string bad)
    {
        UplayLibrary library = Ubisoft(new() { [72] = _root, [73] = _root });
        SaveCache(Game(73, bad), Game(72, Product(72, false)));
        Assert.False(library.LoadInstalledGames());
        Assert.Equal("72", Assert.Single(library.AllInstalledGames).Id);
    }

    [Fact]
    public void UbisoftUninstalledMalformedAndNonLaunchableCacheEntriesKeepTheirExistingFiltering()
    {
        UplayLibrary library = Ubisoft(new() { [72] = _root });
        SaveCache(Game(999, "root: [uninstalled malformed metadata"), Game(72, "root:\n  is_ulc: true"), Game(72, Product(72, false)));
        Assert.True(library.LoadInstalledGames());
        Assert.Equal("72", Assert.Single(library.AllInstalledGames).Id);
    }

    [Fact]
    public void UbisoftUsesInstallIdToIdentifyAnInstalledBrokenCacheEntry()
    {
        UplayLibrary library = Ubisoft(new() { [123] = _root });
        SaveCache(new UplayCachedGame { UplayId = 72, InstallId = 123, GameInfo = "root: [broken" });
        Assert.False(library.LoadInstalledGames());
    }

    [Fact]
    public void UbisoftMissingOrTruncatedCacheFailsAndAnEmptyInstallationListSucceeds()
    {
        UplayLibrary library = Ubisoft(new() { [72] = _root });
        Assert.False(library.LoadInstalledGames());
        SaveCache();
        Assert.False(library.LoadInstalledGames());
        Write("cache/configuration/configurations", "\u000a\u007f");
        Assert.False(library.LoadInstalledGames());
        UplayLibrary empty = Ubisoft(new());
        empty.AllInstalledGames.Add(new Game { Id = "removed" });
        Assert.True(empty.LoadInstalledGames());
        Assert.Empty(empty.AllInstalledGames);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedRefreshRetainsThePreviousCatalogueAndLaterEmptyRefreshRemovesIt(bool ubisoft)
    {
        GameLibrary library;
        Dictionary<uint, string> installed = new() { [72] = _root };
        if (ubisoft)
        {
            library = Ubisoft(installed);
            SaveCache(Game(72, "root: [broken"));
        }
        else
        {
            library = new OriginLibrary(_root);
            Write("LocalContent/OriginBad.mfst", "no install path");
        }
        Game previous = new() { Id = "previous" };
        library.AllInstalledGames.Add(previous);
        GameLibrary.GamesLoaded = true;
        GameLibrary.AllInstalledGamesInAllLibraries = new() { previous };
        Assert.False(GameLibrary.LoadGamesInBackground(new[] { library }));
        Assert.Same(previous, Assert.Single(GameLibrary.AllInstalledGamesInAllLibraries));
        Assert.Same(previous, Assert.Single(library.AllInstalledGames));
        if (ubisoft) installed.Clear();
        else File.Delete(Path.Combine(_root, "LocalContent", "OriginBad.mfst"));
        Assert.True(GameLibrary.LoadGamesInBackground(new[] { library }));
        Assert.Empty(GameLibrary.AllInstalledGamesInAllLibraries);
    }

    private string OriginGame(string id, string version, string relative)
    {
        string directory = Path.Combine(_root, id);
        string executable = Write(id + "/" + relative, "fixture executable; never launched");
        Write("LocalContent/" + id + ".mfst", "dipinstallpath=" + Uri.EscapeDataString(directory));
        XElement runtime = new("runtime", new XElement("launcher", new XElement("requires64BitOS", "1"), new XElement("filePath", relative.Replace('/', '\\'))));
        XElement root = version.StartsWith("4", StringComparison.Ordinal)
            ? new XElement("DiPManifest", new XAttribute("version", version), new XElement("gameTitles", new XElement("gameTitle", new XAttribute("locale", "en_US"), id)), runtime)
            : new XElement("game", new XAttribute("manifestVersion", version), new XElement("metadata", new XElement("localeInfo", new XAttribute("locale", "en_US"), new XElement("title", id))), runtime,
                new XElement("installManifest", new XElement("filePath", "files.mnfst")));
        Write(id + "/__Installer/installerdata.xml", root.ToString());
        return executable;
    }

    private UplayLibrary Ubisoft(Dictionary<uint, string> installed)
    {
        Write("game.exe", "fixture executable; never launched");
        return new UplayLibrary(_root, installed);
    }

    private string Product(uint id, bool registry)
    {
        ProductInformation info = new()
        {
            uplay_id = id, root = new() { start_game = new() { online = new() { executables = new() { new()
            {
                path = new() { relative = "game.exe" }, working_directory = registry
                    ? new() { register = @"HKEY_LOCAL_MACHINE\SOFTWARE\Ubisoft\Launcher\Installs\" + id + @"\InstallDir" }
                    : new() { append = _root }, shortcut_name = "Fixture"
            } } } } }, localizations = new() { ["default"] = new() { l1 = "Fixture" } }
        };
        return new SerializerBuilder().Build().Serialize(info);
    }

    private static UplayCachedGame Game(uint id, string yaml) => new() { UplayId = id, InstallId = id, GameInfo = yaml };

    private void SaveCache(params UplayCachedGame[] games)
    {
        string path = Path.Combine(_root, "cache", "configuration", "configurations");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using FileStream file = File.Create(path);
        ProtoBuf.Serializer.Serialize(file, new UplayCachedGameCollection { Games = games.ToList() });
    }

    private string Write(string relative, string content, Encoding? encoding = null)
    {
        string path = Path.Combine(_root, relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, encoding ?? new UTF8Encoding(false));
        return path;
    }

    public void Dispose()
    {
        GameLibrary.AllInstalledGamesInAllLibraries = _previousGames;
        GameLibrary.GamesLoaded = _previousLoaded;
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}
