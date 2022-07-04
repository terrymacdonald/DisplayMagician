using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DisplayMagician;

namespace DisplayMagician.GameLibraries
{
    public enum SupportedGameLibraryType
    {
        Unknown = 0,
        Steam = 1,
        Uplay = 2,
        Origin = 3,
        Epic = 4,
        GOG = 5
    }

    public class GameLibrary
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public struct GameAppInfo
        {
            public string GameID;
            public string GameName;
            public string GameExePath;
            public string GameInstallDir;
            public string GameIconPath;
        }

        #region Class Properties

        public static List<Game> AllInstalledGamesInAllLibraries { get; set; }
        public static bool GamesLoaded { get; set; } = false;

        public static bool GamesImagesLoaded { get; set; } = false;

        public virtual List<Game> AllInstalledGames { get; set; }

        public virtual int InstalledGameCount { get; set; }

        public virtual string GameLibraryName { get; set; }

        public virtual SupportedGameLibraryType GameLibraryType { get; set; }

        public virtual string GameLibraryExe { get; set; }

        public virtual string GameLibraryPath { get; set; }

        public virtual bool IsGameLibraryInstalled { get; set; }

        public virtual bool IsRunning { get; set; }

        public virtual bool IsUpdating { get; set; }

        public virtual List<string> GameLibraryProcesses { get; set; }
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


        public virtual Game GetGame(string gameNameOrId)
        {
            return null;
        }

        public virtual Game GetGameById(string gameId)
        {
            return null;
        }

        public virtual bool LoadInstalledGames()
        {
            return false;
        }

        public virtual List<Process> StartGame(Game game, string gameArguments = "", ProcessPriority processPriority = ProcessPriority.Normal)
        {
            return null;
        }

        public static bool LoadGamesInBackground()
        {

            logger.Trace($"Program/LoadGamesInBackground: Attempting to load games from detected game libraries.");


            // Clear the game libraries in case this is a refresh
            SteamLibrary steamLibrary = SteamLibrary.GetLibrary();
            steamLibrary.AllInstalledGames.Clear();
            UplayLibrary uplayLibrary = UplayLibrary.GetLibrary();
            uplayLibrary.AllInstalledGames.Clear();
            OriginLibrary originLibrary = OriginLibrary.GetLibrary();
            originLibrary.AllInstalledGames.Clear();
            EpicLibrary epicLibrary = EpicLibrary.GetLibrary();
            epicLibrary.AllInstalledGames.Clear();
            GogLibrary gogLibrary = GogLibrary.GetLibrary();
            gogLibrary.AllInstalledGames.Clear();

            // Now lets prepare loading all the Steam games we have installed
            Action loadSteamGamesAction = new Action(() =>
            {
                // Check if Steam is installed
                if (steamLibrary.IsGameLibraryInstalled)
                {
                    // Load Steam library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed Steam Games");
                    if (!steamLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Steam Games!");
                    }
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed Steam Games (found {steamLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: Steam not installed.");
                    Console.WriteLine("Steam not installed.");
                }
            });

            // Now lets prepare loading all the Uplay games we have installed
            Action loadUplayGamesAction = new Action(() =>
            {
                // Check if Uplay is installed
                if (uplayLibrary.IsGameLibraryInstalled)
                {
                    // Load Uplay library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed Uplay Games");
                    if (!uplayLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Uplay Games!");
                    }
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed Uplay Games (found {uplayLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: Uplay not installed.");
                    Console.WriteLine("Uplay not installed.");
                }

            });

            // Now lets prepare loading all the Origin games we have installed
            Action loadOriginGamesAction = new Action(() =>
            {
                // Check if Origin is installed
                if (originLibrary.IsGameLibraryInstalled)
                {
                    // Load Origin library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed Origin Games");
                    if (!originLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Origin Games!");
                    }
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed Origin Games (found {originLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: Origin not installed.");
                    Console.WriteLine("Origin not installed.");
                }

            });

            // Now lets prepare loading all the Epic games we have installed
            Action loadEpicGamesAction = new Action(() =>
            {
                // Check if Epic is installed
                if (epicLibrary.IsGameLibraryInstalled)
                {
                    // Load Origin library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed Epic Games");
                    if (!epicLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed Epic Games!");
                    }
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed Epic Games (found {epicLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: Epic not installed.");
                    Console.WriteLine("Epic not installed.");
                }

            });

            // Now lets prepare loading all the GOG games we have installed
            Action loadGogGamesAction = new Action(() =>
            {
                // Check if GOG is installed
                if (gogLibrary.IsGameLibraryInstalled)
                {
                    // Load Origin library games
                    logger.Info($"Program/LoadGamesInBackground: Loading Installed GOG Games");
                    if (!gogLibrary.LoadInstalledGames())
                    {
                        logger.Info($"Program/LoadGamesInBackground: Cannot load installed GOG Games!");
                    }
                    logger.Info($"Program/LoadGamesInBackground: Loaded all Installed GOG Games (found {gogLibrary.InstalledGameCount})");
                }
                else
                {
                    logger.Info($"Program/LoadGamesInBackground: GOG not installed.");
                    Console.WriteLine("GOG not installed.");
                }

            });

            // Store all the actions in a array so we can wait on them later
            List<Action> loadGamesActions = new List<Action>();
            loadGamesActions.Add(loadSteamGamesAction);
            loadGamesActions.Add(loadUplayGamesAction);
            loadGamesActions.Add(loadOriginGamesAction);
            loadGamesActions.Add(loadEpicGamesAction);
            loadGamesActions.Add(loadGogGamesAction);

            try
            {
                logger.Debug($"Program/LoadGamesInBackground: Running game loading actions.");
                // Go through and start all the actions, making sure we only have one threat per action to avoid thread issues
                int threads = loadGamesActions.Count;
                ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = threads };
                Parallel.Invoke(options, loadGamesActions.ToArray());
                // Once we get here , we know that all the parallel actions have returned
                logger.Debug($"Program/LoadGamesInBackground: All game loading tasks finished");
            }
            catch (AggregateException ae)
            {
                logger.Error(ae, $"Program/LoadGamesInBackground: One or more exception during execution of loadGamesActions");
                foreach (Exception ex in ae.InnerExceptions)
                {
                    logger.Error(ex, $"Program/LoadGamesInBackground: LoadGamesActions exception:");
                }
            }
            
            // Produce a single array of Games we can reference later
            GameLibrary.AllInstalledGamesInAllLibraries = new List<Game>();
            GameLibrary.AllInstalledGamesInAllLibraries.AddRange(steamLibrary.AllInstalledGames);
            GameLibrary.AllInstalledGamesInAllLibraries.AddRange(uplayLibrary.AllInstalledGames);
            GameLibrary.AllInstalledGamesInAllLibraries.AddRange(originLibrary.AllInstalledGames);
            GameLibrary.AllInstalledGamesInAllLibraries.AddRange(epicLibrary.AllInstalledGames);
            GameLibrary.AllInstalledGamesInAllLibraries.AddRange(gogLibrary.AllInstalledGames);

            // Stop creating Game Bitmaps from the Games so the rest of the program is faster later
            //RefreshGameBitmaps();

            GamesLoaded = true;

            return true;
        }

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
                    if (game.GameLibrary.Equals(SupportedGameLibraryType.Steam))
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.Steam, "Steam Icon", game.ExePath, bmList.Count);
                    }
                    else if (game.GameLibrary.Equals(SupportedGameLibraryType.Uplay))
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.Uplay, "Uplay Icon", game.ExePath, bmList.Count);
                    }
                    else if (game.GameLibrary.Equals(SupportedGameLibraryType.Origin))
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.Origin, "Origin Icon", game.ExePath, bmList.Count);
                    }
                    else if (game.GameLibrary.Equals(SupportedGameLibraryType.Epic))
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.Epic, "Epic Icon", game.ExePath, bmList.Count);
                    }
                    else if (game.GameLibrary.Equals(SupportedGameLibraryType.GOG))
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.GOG, "GOG Icon", game.ExePath, bmList.Count);
                    }
                    else
                    {
                        bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.DisplayMagician.ToBitmap(), "DisplayMagician Icon", game.ExePath, bmList.Count);
                    }
                    // Add the shortcutbitmap to the list
                    bmList.Add(bm);

                }

                game.AvailableGameBitmaps = bmList;
                game.GameBitmap = ImageUtils.GetMeLargestAvailableBitmap(bmList);
            }
            GamesImagesLoaded = true;
        }



        #endregion

    }

    [global::System.Serializable]
    public class GameLibraryException : Exception
    {
        public GameLibraryException() { }
        public GameLibraryException(string message) : base(message) { }
        public GameLibraryException(string message, Exception inner) : base(message, inner) { }
        protected GameLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}