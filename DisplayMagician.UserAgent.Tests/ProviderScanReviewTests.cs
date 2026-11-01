using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DisplayMagician.GameLibraries;
using DisplayMagician.GameLibraries.SteamAppInfoParser;
using Newtonsoft.Json;
using ValveKeyValue;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

[Collection("Agent repository state")]
public sealed class ProviderScanReviewTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-ProviderScan", Guid.NewGuid().ToString("N"));
    private readonly List<Game> _previousGames = GameLibrary.AllInstalledGamesInAllLibraries;
    private readonly bool _previousLoaded = GameLibrary.GamesLoaded;

    [Theory]
    [InlineData(" ", "\n", false)]
    [InlineData("\t", "\r\n", false)]
    [InlineData("  \t  ", " ", false)]
    [InlineData(" ", "\r\n", true)]
    [InlineData("\t", "\n", true)]
    public void SteamLibraryFoldersAcceptWhitespaceCommentsAndOldAndNewEntries(string whitespace, string newline, bool oldFormat)
    {
        string path = Path.Combine(_root, "secondary library", "target");
        string entry = oldFormat ? "\"1\"" + whitespace + Quote(path)
            : "\"1\"" + whitespace + "{" + newline + "\"path\"" + whitespace + Quote(path) + newline + "\"apps\" {} }";
        string source = "\"libraryfolders\"" + newline + "{" + newline + "// library comment\n" + entry + newline + "}";
        string file = Write("libraryfolders.vdf", source);
        Assert.Equal(path, Assert.Single(SteamLibrary.ReadLibraryPaths(file, false)));
    }

    [Fact]
    public void SteamLegacyConfigurationParsesNestedFoldersWithoutRegexUnescapingPaths()
    {
        string path = Path.Combine(_root, "tools", "new library");
        string file = Write("config.vdf", "\"InstallConfigStore\" { \"Software\" { \"Valve\" { \"Steam\" { \"BaseInstallFolder_1\" " + Quote(path) + " } } } }");
        Assert.Equal(path, Assert.Single(SteamLibrary.ReadLibraryPaths(file, true)));
    }

    [Theory]
    [InlineData("\"libraryfolders\" { \"1\" { \"path\"")]
    [InlineData("\"libraryfolders\" { \"1\" { \"apps\" {} } }")]
    [InlineData("\"libraryfolders\" { \"1\" \"\" }")]
    [InlineData("\"unrecognised\" {}")]
    public void InvalidSteamFolderDefinitionsFailTheScanInsteadOfPublishingAnEmptyCatalogue(string content)
    {
        SteamLibrary library = Steam();
        Write("steamapps/libraryfolders.vdf", content);
        Write("steamapps/appmanifest_42.acf", "\"AppState\" { \"appid\" \"42\" }");
        Assert.False(library.LoadInstalledGames());
        Assert.Equal("42", Assert.Single(library.AllInstalledGames).Id);
    }

    [Fact]
    public void SteamFindsAnExternalLibraryAndDoesNotDuplicateCaseVariantsOrLegacyPaths()
    {
        SteamLibrary library = Steam();
        string external = Path.Combine(_root, "external");
        Directory.CreateDirectory(Path.Combine(external, "steamapps"));
        Write("steamapps/libraryfolders.vdf", "\"libraryfolders\" { \"0\" { \"path\" " + Quote(_root) + " } \"1\" { \"path\" " + Quote(external) + " } }");
        Write("config/config.vdf", "\"InstallConfigStore\" { \"BaseInstallFolder_1\" " + Quote(external.ToUpperInvariant()) + " }");
        Write("external/steamapps/appmanifest_42.acf", "\"AppState\" { \"appid\" \"42\" }");
        Assert.True(library.LoadInstalledGames());
        Assert.Equal("42", Assert.Single(library.AllInstalledGames).Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SteamContinuesAfterAMalformedOrLockedManifestButReportsIncomplete(bool locked)
    {
        SteamLibrary library = Steam();
        string bad = Write("steamapps/appmanifest_1.acf", "\"AppState\" { \"appid\"");
        Write("steamapps/appmanifest_42.acf", "\"AppState\" { \"APPID\" \"42\" }");
        using FileStream? held = locked ? new FileStream(bad, FileMode.Open, FileAccess.ReadWrite, FileShare.None) : null;
        Assert.False(library.LoadInstalledGames());
        Assert.Equal("42", Assert.Single(library.AllInstalledGames).Id);
    }

    [Fact]
    public void SteamEmptyReadableLibrarySucceedsWithoutAnAppInfoCache()
    {
        SteamLibrary library = Steam(Array.Empty<string>(), Array.Empty<App>());
        library.AllInstalledGames.Add(new Game { Id = "removed" });
        Assert.True(library.LoadInstalledGames());
        Assert.Empty(library.AllInstalledGames);
    }

    [Fact]
    public void SteamUnreadableInstalledAppMetadataAndCustomShortcutsAreNotSuccessfulEmptyScans()
    {
        SteamLibrary missingData = Steam(new[] { "42" }, new[] { new App { AppID = 42 } });
        Assert.False(missingData.LoadInstalledGames());
        SteamLibrary custom = new(_root, Array.Empty<string>(), Array.Empty<App>(), new[] { "fixture-user" });
        Write("userdata/fixture-user/config/shortcuts.vdf", "invalid binary shortcuts");
        Assert.False(custom.LoadInstalledGames());
    }

    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"InstallationList\":null}")]
    [InlineData("{\"InstallationList\":{}}")]
    public void EpicMalformedOrMissingInstallationListsAreFailuresNotEmptyLibraries(string content)
    {
        EpicLibrary library = Epic();
        Write("UnrealEngineLauncher/LauncherInstalled.dat", content);
        Assert.False(library.LoadInstalledGames());
    }

    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("{}")]
    public void EpicContinuesPastBadManifestsButDoesNotCallThePartialScanSuccessful(string badManifest)
    {
        EpicLibrary library = Epic("good", "bad");
        Write("EpicGamesLauncher/Data/Manifests/bad.item", badManifest);
        Assert.False(library.LoadInstalledGames());
        Assert.Equal("good", Assert.Single(library.AllInstalledGames).Id);
    }

    [Fact]
    public void EpicInstalledEntryWithoutItsManifestAndUnreadableManifestDirectoryAreFailures()
    {
        EpicLibrary library = Epic("good");
        File.Delete(Path.Combine(_root, "EpicGamesLauncher", "Data", "Manifests", "good.item"));
        Assert.False(library.LoadInstalledGames());
        Directory.Delete(Path.Combine(_root, "EpicGamesLauncher", "Data", "Manifests"));
        Assert.False(library.LoadInstalledGames());
    }

    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("{}")]
    public void GogContinuesPastInvalidMetadataAndReportsIncomplete(string badMetadata)
    {
        GogLibrary library = Gog();
        Write("supportInstaller/1000000001/goggame-1000000001.info", badMetadata);
        GogInfo("1000000002");
        Assert.False(library.LoadInstalledGames());
        Assert.Equal("1000000002", Assert.Single(library.AllInstalledGames).Id);
    }

    [Fact]
    public void GogMissingInfoOrUnreadableInstallationDataDoesNotEraseOtherResults()
    {
        GogLibrary library = Gog(id => id == "1000000002" ? throw new IOException("Fixture registry read failure.") : (_root, Path.Combine(_root, "game.exe"), "game.exe"));
        Directory.CreateDirectory(Path.Combine(_root, "supportInstaller", "1000000001"));
        GogInfo("1000000002");
        GogInfo("1000000003");
        Assert.False(library.LoadInstalledGames());
        Assert.Equal("1000000003", Assert.Single(library.AllInstalledGames).Id);
    }

    [Fact]
    public void GogMissingSupportDirectoryIsAFailure()
    {
        GogLibrary library = Gog();
        Directory.Delete(Path.Combine(_root, "supportInstaller"));
        Assert.False(library.LoadInstalledGames());
    }

    [Theory]
    [InlineData("Steam")]
    [InlineData("Epic")]
    [InlineData("GOG")]
    public void RealProviderFailureRetainsThePreviousCatalogueAndASuccessfulEmptyRefreshClearsIt(string provider)
    {
        GameLibrary library;
        string broken;
        if (provider == "Steam")
        {
            library = Steam();
            broken = Write("steamapps/appmanifest_1.acf", "\"AppState\" { \"appid\"");
        }
        else if (provider == "Epic")
        {
            library = Epic();
            broken = Write("EpicGamesLauncher/Data/Manifests/bad.item", "{");
        }
        else
        {
            library = Gog();
            broken = Write("supportInstaller/1000000001/goggame-1000000001.info", "{");
        }
        Game old = new() { Id = "previous-complete-game" };
        library.AllInstalledGames.Add(old);
        GameLibrary.AllInstalledGamesInAllLibraries = new() { old };
        GameLibrary.GamesLoaded = true;
        Assert.False(GameLibrary.LoadGamesInBackground(new[] { library }));
        Assert.Same(old, Assert.Single(GameLibrary.AllInstalledGamesInAllLibraries));
        Assert.Same(old, Assert.Single(library.AllInstalledGames));
        File.Delete(broken);
        if (provider == "GOG") Directory.Delete(Path.GetDirectoryName(broken)!);
        Assert.True(GameLibrary.LoadGamesInBackground(new[] { library }));
        Assert.Empty(library.AllInstalledGames);
        Assert.Empty(GameLibrary.AllInstalledGamesInAllLibraries);
    }

    [Fact]
    public void FirstScanKeepsTheSuccessfulRealProviderAndExcludesTheFailedProvidersPartialResults()
    {
        SteamLibrary steam = Steam();
        Write("steamapps/appmanifest_42.acf", "\"AppState\" { \"appid\" \"42\" }");
        EpicLibrary epic = Epic("good", "bad");
        Write("EpicGamesLauncher/Data/Manifests/bad.item", "{");
        GameLibrary.AllInstalledGamesInAllLibraries = new();
        GameLibrary.GamesLoaded = false;
        Assert.True(GameLibrary.LoadGamesInBackground(new GameLibrary[] { steam, epic }));
        Assert.Equal("42", Assert.Single(GameLibrary.AllInstalledGamesInAllLibraries).Id);
        Assert.Empty(epic.AllInstalledGames);
    }

    private SteamLibrary Steam(IReadOnlyList<string>? ids = null, IReadOnlyList<App>? apps = null)
    {
        Directory.CreateDirectory(Path.Combine(_root, "steamapps"));
        using MemoryStream metadata = new(Encoding.UTF8.GetBytes("\"appinfo\" { \"common\" { \"name\" \"Fixture game\" \"type\" \"Game\" } \"config\" { \"installdir\" \"fixture\" } }"));
        KVObject data = KVSerializer.Create(KVSerializationFormat.KeyValues1Text).Deserialize(metadata);
        return new SteamLibrary(_root, ids ?? new[] { "42" }, apps ?? new[] { new App { AppID = 42, Data = data } }, Array.Empty<string>());
    }

    private EpicLibrary Epic(params string[] ids)
    {
        Directory.CreateDirectory(Path.Combine(_root, "EpicGamesLauncher", "Data", "Manifests"));
        Write("UnrealEngineLauncher/LauncherInstalled.dat", JsonConvert.SerializeObject(new LauncherInstalled
        {
            InstallationList = ids.Select(id => new LauncherInstalled.InstalledApp { AppName = id, InstallLocation = _root }).ToList()
        }));
        foreach (string id in ids)
            Write("EpicGamesLauncher/Data/Manifests/" + id + ".item", JsonConvert.SerializeObject(new InstalledManifiest
            {
                AppName = id, MainGameAppName = id, DisplayName = id, InstallLocation = _root, LaunchExecutable = "game.exe"
            }));
        return new EpicLibrary(_root);
    }

    private GogLibrary Gog(Func<string, (string Directory, string Executable, string FileName)?>? installation = null)
    {
        Directory.CreateDirectory(Path.Combine(_root, "supportInstaller"));
        string executable = Write("game.exe", "fixture executable path; never launched");
        return new GogLibrary(_root, installation ?? (_ => (_root, executable, "game.exe")));
    }

    private void GogInfo(string id) => Write("supportInstaller/" + id + "/goggame-" + id + ".info",
        JsonConvert.SerializeObject(new GogGameInfo { gameId = id, rootGameId = id, name = id }));

    private string Write(string relativePath, string content)
    {
        string path = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(false));
        return path;
    }

    private static string Quote(string path) => "\"" + path.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

    public void Dispose()
    {
        GameLibrary.AllInstalledGamesInAllLibraries = _previousGames;
        GameLibrary.GamesLoaded = _previousLoaded;
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}
