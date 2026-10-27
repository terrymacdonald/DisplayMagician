using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DisplayMagician;
#if !USER_AGENT
using DisplayMagician.AppLibraries;
#endif

namespace DisplayMagician.GameLibraries
{
    public class GameLibrary
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public struct GameAppInfo
        {
            public string GameID = string.Empty;
            public string GameName = string.Empty;
            public string GameExePath = string.Empty;
            public string GameInstallDir = string.Empty;
            public string GameIconPath = string.Empty;

            public GameAppInfo()
            {
            }
        }

        #region Class Properties

        public static List<Game> AllInstalledGamesInAllLibraries { get; set; } = new List<Game>();
        public static bool GamesLoaded { get; set; } = false;

#if !USER_AGENT
        public static bool GamesImagesLoaded { get; set; } = false;
#endif

        public virtual List<Game> AllInstalledGames { get; set; } = new List<Game>();

        public virtual int InstalledGameCount { get; set; }

        public virtual string GameLibraryName { get; set; } = string.Empty;

        public virtual SupportedGameLibraryType GameLibraryType { get; set; }

        public virtual string GameLibraryExe { get; set; } = string.Empty;

        public virtual string GameLibraryPath { get; set; } = string.Empty;

        public virtual bool IsGameLibraryInstalled { get; set; }

        public virtual bool IsRunning { get; set; }

        public virtual bool IsUpdating { get; set; }

        public virtual List<string> GameLibraryProcesses { get; set; } = new List<string>();
        #endregion

        #region Class Methods
        public virtual bool AddGame(Game game)
        {
            return false;
        }

        public virtual bool RemoveGame(Game game)
        {
            return false;
        }

        public virtual bool RemoveGameById(string gameId)
        {
            return false;
        }

        public virtual bool RemoveGame(string gameNameOrId)
        {
            return false;
        }

        public virtual bool ContainsGame(Game game)
        {
            return false;
        }

        public virtual bool ContainsGameById(string gameId)
        {
            return false;
        }

        public virtual bool ContainsGame(string gameNameOrId)
        {
            return false;
        }


        public virtual Game? GetGame(string gameNameOrId)
        {
            return null;
        }

        public virtual Game? GetGameById(string gameId)
        {
            return null;
        }

        public virtual bool LoadInstalledGames()
        {
            return false;
        }

        public virtual List<Process>? StartGame(Game game, string gameArguments = "", ProcessPriority processPriority = ProcessPriority.Normal)
        {
            return null;
        }

        public virtual bool StopGame(Game game)
        {
            return true;
        }

        private static readonly object _catalogSyncRoot = new object();

        public static bool LoadGamesInBackground()
        {
            try
            {
                return LoadGamesInBackground(new GameLibrary[]
                {
                    SteamLibrary.GetLibrary(), UplayLibrary.GetLibrary(), OriginLibrary.GetLibrary(),
                    EpicLibrary.GetLibrary(), GogLibrary.GetLibrary()
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "GameLibrary/LoadGamesInBackground: Could not initialize game libraries for scanning.");
                return false;
            }
        }

        internal static bool LoadGamesInBackground(IReadOnlyList<GameLibrary> libraries)
        {
            lock (_catalogSyncRoot)
            {
                Dictionary<GameLibrary, Game[]> previous = new Dictionary<GameLibrary, Game[]>();
                try
                {
                    foreach (GameLibrary library in libraries)
                        previous.Add(library, library.AllInstalledGames.ToArray());
                    ConcurrentBag<GameLibrary> failures = new ConcurrentBag<GameLibrary>();
                    Parallel.ForEach(libraries, library =>
                    {
                        try
                        {
                            library.AllInstalledGames.Clear();
                            if (!library.IsGameLibraryInstalled) return;
                            logger.Info("GameLibrary/LoadGamesInBackground: Loading installed games from {0}.", library.GameLibraryType);
                            if (!library.LoadInstalledGames())
                            {
                                failures.Add(library);
                                logger.Warn("GameLibrary/LoadGamesInBackground: Could not load installed games from {0}.", library.GameLibraryType);
                            }
                        }
                        catch (Exception ex)
                        {
                            failures.Add(library);
                            logger.Error(ex, "GameLibrary/LoadGamesInBackground: Game scan failed for {0}.", library.GameLibraryType);
                        }
                    });
                    if (failures.IsEmpty || !GamesLoaded)
                    {
                        // Preserve first-start availability using successful providers only.
                        // Once a catalogue exists, failed refreshes retain it in full.
                        if (!failures.IsEmpty)
                        {
                            foreach (GameLibrary failed in failures) failed.AllInstalledGames.Clear();
                            logger.Warn("GameLibrary/LoadGamesInBackground: Initial scan was incomplete; using successful libraries. Failed library count: {0}.", failures.Count);
                        }
                        // Publish only after every provider has finished.
                        List<Game> games = libraries.SelectMany(library => library.AllInstalledGames).ToList();
                        AllInstalledGamesInAllLibraries = games;
#if !USER_AGENT
                        Program.AppGameList = games;
#endif
                        GamesLoaded = true;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "GameLibrary/LoadGamesInBackground: Game scan failed; retaining the previous catalogue.");
                }

                // Provider scans may have cleared or replaced their lists before failing.
                foreach (KeyValuePair<GameLibrary, Game[]> entry in previous)
                {
                    entry.Key.AllInstalledGames.Clear();
                    entry.Key.AllInstalledGames.AddRange(entry.Value);
                }
                return false;
            }
        }

#if !USER_AGENT
        public static void RefreshGameBitmaps()
        {
            // Create Game Bitmaps from the Games so the rest of the program is faster later
            // Get the bitmap out of the IconPath
            // IconPath can be an ICO, or an EXE
            foreach (var game in GameLibrary.AllInstalledGamesInAllLibraries)
            {
                List<ShortcutBitmap> bmList = new List<ShortcutBitmap>();
                try
                {
                    /*ArrayList filesToSearchForIcon = new ArrayList();
                    filesToSearchForIcon.Add(game.ExePath);
                    if (game.IconPath != game.ExePath)
                        filesToSearchForIcon.Add(game.IconPath);

                    bm = ImageUtils.GetMeABitmapFromFile(filesToSearchForIcon);*/

                    // We only want the icon location that the GameLibrary told us to use
                    // Note: This may be an icon file, or an exe file.
                    // This function tries to get a 256x256 Vista sized bitmap from the file
                    logger.Trace($"Program/LoadGamesInBackground: Attempting to get game bitmaps from {game.Name}.");
                    bmList.AddRange(ImageUtils.GetMeAllBitmapsFromFile(game.IconPath));
                    if (game.ExePath != game.IconPath)
                    {
                        bmList.AddRange(ImageUtils.GetMeAllBitmapsFromFile(game.ExePath));
                    }
                    logger.Trace($"Program/LoadGamesInBackground: Got game bitmaps from {game.Name}.");

                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Program/LoadGamesInBackground: Exception building game bitmaps for {game.Name} during load");
                }

                if (bmList.Count == 0)
                {
                    ShortcutBitmap bm = new ShortcutBitmap();
                    if (game.GameLibraryType.Equals(SupportedGameLibraryType.Steam))
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.Steam, "Steam Icon", game.ExePath, bmList.Count);
                    }
                    else if (game.GameLibraryType.Equals(SupportedGameLibraryType.Uplay))
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.Uplay, "Uplay Icon", game.ExePath, bmList.Count);
                    }
                    else if (game.GameLibraryType.Equals(SupportedGameLibraryType.Origin))
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.Origin, "Origin Icon", game.ExePath, bmList.Count);
                    }
                    else if (game.GameLibraryType.Equals(SupportedGameLibraryType.Epic))
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.Epic, "Epic Icon", game.ExePath, bmList.Count);
                    }
                    else if (game.GameLibraryType.Equals(SupportedGameLibraryType.GOG))
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.GOG, "GOG Icon", game.ExePath, bmList.Count);
                    }
                    else
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.displaymagician.ToBitmap(), "DisplayMagician Icon", game.ExePath, bmList.Count);
                    }
                    // Add the shortcutbitmap to the list
                    bmList.Add(bm);

                }

                game.AvailableGameBitmaps = bmList;
                game.GameBitmap = ImageUtils.GetMeLargestAvailableBitmap(bmList);
            }
            GamesImagesLoaded = true;
        }
#endif

        public static Game? GetAnyGameById(string gameId)
            => GetAnyGameById(gameId, null);

        public static Game? GetAnyGameById(string gameId, SupportedGameLibraryType? libraryType)
        {
            Game? gameToUse = null;
            foreach (Game game in GameLibrary.AllInstalledGamesInAllLibraries)
            {
                if (string.Equals(game.Id, gameId, StringComparison.Ordinal) &&
                    (!libraryType.HasValue || game.GameLibraryType == libraryType.Value))
                {
                    gameToUse = game;
                    break;
                }
            }

            if (gameToUse is Game)
            {
                logger.Info($"GameLibrary/GetGameById: Found {gameToUse.GameLibraryType} App {gameToUse.Name} from ID {gameId}");
                return gameToUse;
            }
            else
            {
                logger.Info($"GameLibrary/GetGameById: Didn't find App with ID {gameId}");
                return gameToUse;
            }
        }


        #endregion

    }

    [global::System.Serializable]
    public class GameLibraryException : Exception
    {
        public GameLibraryException() { }
        public GameLibraryException(string message) : base(message) { }
        public GameLibraryException(string message, Exception inner) : base(message, inner) { }
    }

}
