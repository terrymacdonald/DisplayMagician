using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ValveKeyValue;
using DisplayMagician.GameLibraries.SteamAppInfoParser;
using Microsoft.Win32;
using System.IO;
using System.Security;
using System.Diagnostics;
using DisplayMagician.Processes;
using YamlDotNet.Serialization;
using Windows.Devices.Perception;
using Windows.Devices.Printers;

namespace DisplayMagician.GameLibraries
{
    public sealed class SteamLibrary : GameLibrary
    {

        private struct SteamAppInfo
        {
            public string GameID;
            public string GameName;
            public List<string> GameExes;
            public string GameInstallDir;
            public string GameIconPath;
        }

        #region Class Variables
        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static SteamLibrary _instance = new SteamLibrary();

        // Common items to the class
        private List<Game> _allSteamGames = new List<Game>();
        private string steamAppIdRegex = @"^[0-9A-F]{1,10}$";
        private string _steamExe;
        private string _steamPath;
        private string _steamConfigVdfFile;
        private List<string> _steamProcessList = new List<string>() { "steam"};
        private string _registrySteamKey = @"SOFTWARE\WOW6432Node\Valve\Steam"; // under LocalMachine
        private string _registryAppsKey = $@"SOFTWARE\Valve\Steam\Apps"; // under CurrentUser
        private string _registryUsersKey = $@"SOFTWARE\Valve\Steam\Users"; // under CurrentUser
        private bool _isSteamInstalled = false;
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        // Other constants that are useful
        #endregion

        #region Class Constructors
        static SteamLibrary() { }

        private SteamLibrary()
        {
            try
            {
                logger.Trace($"SteamLibrary/SteamLibrary: Steam launcher registry key = HKLM\\{_registrySteamKey}");
                // Find the SteamExe location, and the SteamPath for later
                using (var steamInstallKey = Registry.LocalMachine.OpenSubKey(_registrySteamKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (steamInstallKey == null)
                    {
                        logger.Info($"SteamLibrary/SteamLibrary: Steam library is not installed!");
                        return;
                    }
                    _steamPath = steamInstallKey.GetValue("InstallPath", "C:\\Program Files (x86)\\Steam").ToString();
                    _steamExe = $"{_steamPath}\\steam.exe";
                }                   
                if (File.Exists(_steamExe))
                {
                    logger.Info($"SteamLibrary/SteamLibrary: Steam library is installed in {_steamPath}. Found {_steamExe}");
                       _isSteamInstalled = true;
                    }
                else
                {
                    logger.Info($"SteamLibrary/SteamLibrary: Steam library is not installed!");
                }
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "SteamLibrary/SteamLibrary: The user does not have the permissions required to read the Steam registry key.");
            }
            catch (ObjectDisposedException ex)
            {
                logger.Warn(ex, "SteamLibrary/SteamLibrary: The Microsoft.Win32.RegistryKey is closed when trying to access the Steam registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "SteamLibrary/SteamLibrary: The Steam registry key has been marked for deletion so we cannot access the value during the SteamLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "SteamLibrary/SteamLibrary: The user does not have the necessary registry rights to check whether Steam is installed.");
            }
        }
        #endregion

        #region Class Properties
        public override List<Game> AllInstalledGames
        {
            get
            {
                // Disabled as we now do it manually when DM starts
                // Load the Steam Games from Steam Client if needed
                /*if (_allSteamGames.Count == 0)
                    LoadInstalledGames();*/
                return _allSteamGames;
            }
        }


        public override int InstalledGameCount
        {
            get
            {
                return _allSteamGames.Count;
            }
        }

        public string SteamRegistryKey
        {
            get
            {
                return _registrySteamKey;
            }
        }

        public string SteamAppsRegistryKey
        {
            get
            {
                return _registryAppsKey;
            }
        }

        public override string GameLibraryName
        {
            get
            {
                return "Steam";
            }
        }

        public override SupportedGameLibraryType GameLibraryType
        {
            get
            {
                return SupportedGameLibraryType.Steam;
            }
        }

        public override string GameLibraryExe
        {
            get
            {
                return _steamExe;
            }
        }

        public override string GameLibraryPath
        {
            get
            {
                return _steamPath;
            }
        }

        public override bool IsGameLibraryInstalled
        {
            get
            {
                return _isSteamInstalled;
            }

        }

        public override bool IsRunning
        {
            get
            {
                List<Process> steamLibraryProcesses = new List<Process>();

                try
                {
                    foreach (string steamLibraryProcessName in _steamProcessList)
                    {
                        // Look for the processes with the ProcessName we sorted out earlier
                        steamLibraryProcesses.AddRange(Process.GetProcessesByName(steamLibraryProcessName));
                    }

                    // If we have found one or more processes then we should be good to go
                    // so let's break, and get to the next step....
                    if (steamLibraryProcesses.Count > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"SteamLibrary/IsRunning: Exception while trying to get the steam library processes matching process names: {string.Join(", ", _steamProcessList)}.");
                    return false;
                }
            }

        }

        public override bool IsUpdating
        {
            get
            {
                // Not implemeted at present
                // so we just return a false
                // TODO Implement Origin specific detection for updating the game client
                return false;
            }

        }

        public override List<string> GameLibraryProcesses
        {
            get
            {
                return _steamProcessList;
            }
        }


        #endregion

        #region Class Methods
        public static SteamLibrary GetLibrary()
        {
            return _instance;
        }

        public override bool AddGame(Game steamGame)
        {
            if (!(steamGame is SteamGame))
                return false;

            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (ContainsGame(steamGame))
            {
                logger.Debug($"SteamLibrary/AddSteamGame: Updating Steam game {steamGame.Name} in our Steam library");
                // We update the existing Shortcut with the data over
                SteamGame steamGameToUpdate = (SteamGame)GetGameById(steamGame.Id.ToString());
                steamGame.CopyTo(steamGameToUpdate);
            }
            else
            {
                logger.Debug($"SteamLibrary/AddSteamGame: Adding Steam game {steamGame.Name} to our Steam library");
                // Add the steamGame to the list of steamGames
                _allSteamGames.Add(steamGame);
            }

            //Doublecheck it's been added
            if (ContainsGame(steamGame))
            {
                return true;
            }
            else
                return false;

        }

        public override bool RemoveGame(Game steamGame)
        {
            if (!(steamGame is SteamGame))
                return false;

            logger.Debug($"SteamLibrary/RemoveSteamGame: Removing Steam game {steamGame.Name} from our Steam library");

            // Remove the steamGame from the list.
            int numRemoved = _allSteamGames.RemoveAll(item => item.Id.Equals(steamGame.Id));

            if (numRemoved == 1)
            {
                logger.Debug($"SteamLibrary/RemoveSteamGame: Removed Steam game with name {steamGame.Name}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"SteamLibrary/RemoveSteamGame: Didn't remove Steam game with ID {steamGame.Name} from the Steam Library");
                return false;
            }
            else
                throw new SteamLibraryException();
        }

        public override bool RemoveGameById(string steamGameId)
        {
            if (steamGameId.Equals("0"))
                return false;

            logger.Debug($"SteamLibrary/RemoveSteamGame2: Removing Steam game with ID {steamGameId} from the Steam library");

            // Remove the steamGame from the list.
            int numRemoved = _allSteamGames.RemoveAll(item => item.Id.Equals(steamGameId));

            if (numRemoved == 1)
            {
                logger.Debug($"SteamLibrary/RemoveSteamGame2: Removed Steam game with ID {steamGameId}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"SteamLibrary/RemoveSteamGame2: Didn't remove Steam game with ID {steamGameId} from the Steam Library");
                return false;
            }
            else
                throw new SteamLibraryException();
        }


        public override bool RemoveGame(string steamGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(steamGameNameOrId))
                return false;

            logger.Debug($"SteamLibrary/RemoveSteamGame3: Removing Steam game with Name or UUID {steamGameNameOrId} from the Steam library");

            int numRemoved;
            Match match = Regex.Match(steamGameNameOrId, steamAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                numRemoved = _allSteamGames.RemoveAll(item => steamGameNameOrId.Equals(item.Id));
            else
                numRemoved = _allSteamGames.RemoveAll(item => steamGameNameOrId.Equals(item.Name));

            if (numRemoved == 1)
            {
                logger.Debug($"SteamLibrary/RemoveSteamGame3: Removed Steam game with Name or UUID {steamGameNameOrId} ");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"SteamLibrary/RemoveSteamGame3: Didn't remove Steam game with Name or UUID {steamGameNameOrId} from the Steam Library");
                return false;
            }
            else
                throw new SteamLibraryException();

        }

        public override bool ContainsGame(Game steamGame)
        {
            if (!(steamGame is SteamGame))
                return false;

            foreach (SteamGame testSteamGame in _allSteamGames)
            {
                if (testSteamGame.Id.Equals(steamGame.Id))
                    return true;
            }

            return false;
        }

        public override bool ContainsGame(string steamGameNameOrUuid)
        {
            if (String.IsNullOrWhiteSpace(steamGameNameOrUuid))
                return false;


            Match match = Regex.Match(steamGameNameOrUuid, steamAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (SteamGame testSteamGame in _allSteamGames)
                {
                    if (steamGameNameOrUuid.Equals(testSteamGame.Id))
                        return true;
                }

            }
            else
            {
                foreach (SteamGame testSteamGame in _allSteamGames)
                {
                    if (steamGameNameOrUuid.Equals(testSteamGame.Name))
                        return true;
                }

            }

            return false;

        }

        public override bool ContainsGameById(string steamGameId)
        {
            foreach (SteamGame testSteamGame in _allSteamGames)
            {
                if (steamGameId == testSteamGame.Id)
                    return true;
            }

           
            return false;

        }


        public override Game GetGame(string steamGameNameOrUuid)
        {
            if (String.IsNullOrWhiteSpace(steamGameNameOrUuid))
                return null;

            Match match = Regex.Match(steamGameNameOrUuid, steamAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (SteamGame testSteamGame in _allSteamGames)
                {
                    if (steamGameNameOrUuid.Equals(testSteamGame.Id))
                        return testSteamGame;
                }

            }
            else
            {
                foreach (SteamGame testSteamGame in _allSteamGames)
                {
                    if (steamGameNameOrUuid.Equals(testSteamGame.Name))
                        return testSteamGame;
                }

            }

            return null;

        }

        public override Game GetGameById(string steamGameId)
        {
            foreach (SteamGame testSteamGame in _allSteamGames)
            {
                if (steamGameId == testSteamGame.Id)
                    return testSteamGame;
            }

            return null;

        }

        public override bool LoadInstalledGames()
        {
            try
            {
                if (!_isSteamInstalled)
                {
                    logger.Info($"SteamLibrary/LoadInstalledGames: Steam library is not installed");
                    return false;
                }

                logger.Trace($"SteamLibrary/LoadInstalledGames: Steam Base Registry Key = HKLM\\{_registrySteamKey}");
                logger.Trace($"SteamLibrary/LoadInstalledGames: Steam Apps Registry Key = HKCU\\{_registryAppsKey}");

                // FIX: Clear out the existing collection to prevent catalog duplication on re-scans!
                _allSteamGames.Clear();

                List<string> steamAppIdsInstalled = new List<string>();
                using (RegistryKey steamAppsKey = Registry.CurrentUser.OpenSubKey(_registryAppsKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (steamAppsKey != null)
                    {
                        foreach (string steamAppId in steamAppsKey.GetSubKeyNames())
                        {
                            logger.Trace($"SteamLibrary/LoadInstalledGames: Found SteamGameKeyName = {steamAppId}");
                            if (!String.IsNullOrWhiteSpace(steamAppId))
                            {
                                string steamGameKeyFullName = $"{_registryAppsKey}\\{steamAppId}";
                                using (RegistryKey steamGameKey = Registry.CurrentUser.OpenSubKey(steamGameKeyFullName, RegistryKeyPermissionCheck.ReadSubTree))
                                {
                                    if (steamGameKey == null)
                                        continue;
                                    if ((int)steamGameKey.GetValue(@"Installed", 0) == 1)
                                    {
                                        logger.Trace($"SteamLibrary/LoadInstalledGames: {steamGameKeyFullName} is an installed Steam App.");
                                        steamAppIdsInstalled.Add(steamAppId);
                                    }
                                }
                            }
                        }

                        if (steamAppIdsInstalled.Count == 0)
                        {
                            logger.Warn($"SteamLibrary/LoadInstalledGames: No Steam games installed in the Steam library");
                            return false;
                        }
                    }
                    else
                    {
                        logger.Warn($"SteamLibrary/LoadInstalledGames: Couldn't access the Steam Registry Key {_registrySteamKey}");
                        return false;
                    }
                }

                Dictionary<string, SteamAppInfo> steamAppInfo = new Dictionary<string, SteamAppInfo>();
                string appInfoVdfFile = Path.Combine(_steamPath, "appcache", "appinfo.vdf");
                var newAppInfo = new AppInfo();
                try
                {
                    newAppInfo.Read(appInfoVdfFile);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"SteamLibrary/LoadInstalledGames: Exception while reading appinfo.vdf");
                    return false;
                }
                logger.Trace($"SteamLibrary/LoadInstalledGames: Found {newAppInfo.Apps.Count} apps in the {appInfoVdfFile} VDF file");

                foreach (var app in newAppInfo.Apps)
                {
                    string detectedAppID = app.AppID.ToString();
                    if (steamAppIdsInstalled.Contains(detectedAppID))
                    {
                        try
                        {
                            SteamAppInfo steamGameAppInfo = new SteamAppInfo
                            {
                                GameID = detectedAppID,
                                GameExes = new List<string>()
                            };
                            string steamAppType = "";

                            foreach (var (dataKey, data) in app.Data)
                            {
                                if (dataKey == "common")
                                {
                                    foreach (var (commonKey, common) in data)
                                    {
                                        if (commonKey == "name")
                                        {
                                            steamGameAppInfo.GameName = (string)common;
                                        }
                                        else if (commonKey == "clienticon")
                                        {
                                            steamGameAppInfo.GameIconPath = Path.Combine(_steamPath, @"steam", @"games", String.Concat((string)common, @".ico"));
                                        }
                                        else if (commonKey == "type")
                                        {
                                            steamAppType = (string)common;
                                        }
                                    }
                                }
                                else if (dataKey == "config")
                                {
                                    foreach (var (configKey, config) in data)
                                    {
                                        if (configKey == "installdir")
                                        {
                                            steamGameAppInfo.GameInstallDir = (string)config;
                                        }
                                        else if (configKey == "launch")
                                        {
                                            foreach (var (launchKey, launch) in config)
                                            {
                                                foreach (var (launchNumKey, launch_num) in launch)
                                                {
                                                    if (launchNumKey == "executable")
                                                    {
                                                        steamGameAppInfo.GameExes.Add((string)launch_num);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (steamAppType.Equals("Game", StringComparison.OrdinalIgnoreCase))
                            {
                                steamAppInfo.Add(detectedAppID, steamGameAppInfo);
                            }
                        }
                        catch (ArgumentException ex)
                        {
                            logger.Warn(ex, $"SteamLibrary/LoadInstalledGames: ArgumentException while processing the {appInfoVdfFile} VDF file");
                        }
                    }
                }

                List<string> steamLibrariesPaths = new List<string> { _steamPath };

                string _steamLibraryFoldersVdfFile = Path.Combine(_steamPath, "steamapps", "libraryfolders.vdf");
                if (File.Exists(_steamLibraryFoldersVdfFile))
                {
                    string steamLibraryFoldersText = File.ReadAllText(_steamLibraryFoldersVdfFile, Encoding.UTF8);
                    Regex steamLibrariesRegex = new Regex(@"\t""path""\t\t""(.*?)""\n", RegexOptions.IgnoreCase);
                    MatchCollection steamLibrariesMatches = steamLibrariesRegex.Matches(steamLibraryFoldersText);
                    foreach (Match steamLibraryMatch in steamLibrariesMatches)
                    {
                        if (steamLibraryMatch.Success)
                        {
                            string steamLibraryPath = Regex.Unescape(steamLibraryMatch.Groups[1].Value);
                            if (!steamLibraryPath.Equals(_steamPath) && Directory.Exists(steamLibraryPath))
                            {
                                if (!steamLibrariesPaths.Contains(steamLibraryPath))
                                {
                                    steamLibrariesPaths.Add(steamLibraryPath);
                                }
                            }
                        }
                    }
                }

                _steamConfigVdfFile = Path.Combine(_steamPath, "config", "config.vdf");
                if (File.Exists(_steamConfigVdfFile))
                {
                    string steamConfigVdfText = File.ReadAllText(_steamConfigVdfFile, Encoding.UTF8);
                    Regex steamLibrariesRegex = new Regex(@"""BaseInstallFolder_\d+""\s+""(.*)""", RegexOptions.IgnoreCase);
                    MatchCollection steamLibrariesMatches = steamLibrariesRegex.Matches(steamConfigVdfText);
                    foreach (Match steamLibraryMatch in steamLibrariesMatches)
                    {
                        if (steamLibraryMatch.Success)
                        {
                            string steamLibraryPath = Regex.Unescape(steamLibraryMatch.Groups[1].Value);
                            if (!steamLibraryPath.Equals(_steamPath) && Directory.Exists(steamLibraryPath))
                            {
                                if (!steamLibrariesPaths.Contains(steamLibraryPath))
                                {
                                    steamLibrariesPaths.Add(steamLibraryPath);
                                }
                            }
                        }
                    }
                }

                foreach (string steamLibraryPath in steamLibrariesPaths)
                {
                    string steamLibraryAppManifestPath = Path.Combine(steamLibraryPath, @"steamapps");
                    try
                    {
                        string[] steamLibraryAppManifestFilenames = Directory.GetFiles(steamLibraryAppManifestPath, "appmanifest_*.acf");
                        foreach (string steamLibraryAppManifestFilename in steamLibraryAppManifestFilenames)
                        {
                            string steamLibraryAppManifestText = File.ReadAllText(steamLibraryAppManifestFilename);
                            Regex appidRegex = new Regex(@"""appid""\s+""(\d+)""", RegexOptions.IgnoreCase);
                            Match appidMatches = appidRegex.Match(steamLibraryAppManifestText);
                            if (appidMatches.Success && !String.IsNullOrWhiteSpace(appidMatches.Groups[1].Value))
                            {
                                string steamGameId = appidMatches.Groups[1].Value;
                                if (steamAppInfo.ContainsKey(steamGameId))
                                {
                                    string steamGameExe = "";
                                    string steamGameName = steamAppInfo[steamGameId].GameName;
                                    string steamGameInstallDir = Path.Combine(steamLibraryPath, @"steamapps", @"common", steamAppInfo[steamGameId].GameInstallDir);

                                    if (steamAppInfo[steamGameId].GameExes.Count > 0)
                                    {
                                        foreach (string gameExe in steamAppInfo[steamGameId].GameExes)
                                        {
                                            steamGameExe = Path.Combine(steamGameInstallDir, gameExe);
                                            if (File.Exists(steamGameExe))
                                                break;
                                        }
                                    }

                                    string steamGameIconPath = "";
                                    if (File.Exists(steamAppInfo[steamGameId].GameIconPath) && steamAppInfo[steamGameId].GameIconPath.EndsWith(".ico"))
                                    {
                                        steamGameIconPath = steamAppInfo[steamGameId].GameIconPath;
                                    }
                                    else if (!String.IsNullOrEmpty(steamGameExe))
                                    {
                                        steamGameIconPath = steamGameExe;
                                    }
                                    else
                                    {
                                        steamGameIconPath = _steamPath;
                                    }

                                    SteamGame gameToAdd = new SteamGame(steamGameId, steamGameName, steamGameExe, steamGameIconPath);
                                    _allSteamGames.Add(gameToAdd);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"SteamLibrary/LoadInstalledGames: Operational hiccup scanning storage directory path: {steamLibraryPath}.");
                    }
                }

                // Non-Steam Game Parsing Section
                using (RegistryKey steamUsersKey = Registry.CurrentUser.OpenSubKey(_registryUsersKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (steamUsersKey != null)
                    {
                        foreach (string steamUserId in steamUsersKey.GetSubKeyNames())
                        {
                            if (!String.IsNullOrWhiteSpace(steamUserId))
                            {
                                string shortcutsVdfFile = Path.Combine(_steamPath, "userdata", steamUserId, "config", "shortcuts.vdf");
                                if (File.Exists(shortcutsVdfFile))
                                {
                                    var fs = new FileStream(shortcutsVdfFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                    try
                                    {
                                        var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);
                                        var kvDoc = deserializer.Deserialize(fs);
                                        foreach (var (kvItemKey, kvItem) in kvDoc.Root)
                                        {
                                            string shortcutGameID = "";
                                            string shortcutGameName = "";
                                            string shortcutGameExe = "";
                                            string shortcutGameIconPath = "";

                                            foreach (var (subItemKey, subItem) in kvItem)
                                            {
                                                switch (subItemKey.ToLower())
                                                {
                                                    case "appid":
                                                        var appid = (UInt64)(int)subItem;
                                                        shortcutGameID = ((appid << 32) | 0x02000000).ToString();
                                                        break;
                                                    case "appname":
                                                        shortcutGameName = $"{(string)subItem} (via Steam)";
                                                        break;
                                                    case "exe":
                                                        string tempString2 = (string)subItem;
                                                        shortcutGameExe = (tempString2.StartsWith("\"") && tempString2.EndsWith("\"")) 
                                                            ? tempString2.Substring(1, (tempString2.Length - 2)) 
                                                            : tempString2;
                                                        
                                                        if (File.Exists(shortcutGameExe))
                                                            shortcutGameIconPath = shortcutGameExe;
                                                        break;
                                                    case "icon":
                                                        if (!String.IsNullOrWhiteSpace((string)subItem) && File.Exists((string)subItem))
                                                            shortcutGameIconPath = (string)subItem;
                                                        break;
                                                }
                                            }
                                            SteamGame gameToAdd = new SteamGame(shortcutGameID, shortcutGameName, shortcutGameExe, shortcutGameIconPath);
                                            _allSteamGames.Add(gameToAdd);
                                        }
                                    }
                                    finally
                                    {
                                        fs.Close();
                                    }
                                }
                            }
                        }
                    }
                }

                logger.Info($"SteamLibrary/LoadInstalledGames: Found {_allSteamGames.Count} total installed Steam titles & shortcuts.");
            }
            // FIX: Catch-all clean exception block replaces the bulky, misleading registry handlers
            catch (Exception ex)
            {
                logger.Error(ex, "SteamLibrary/LoadInstalledGames: Core exception encountered while parsing VDF/ACF configurations or processing active user manifests.");
                return false;
            }

            return true;
        }


        public override List<Process> StartGame(Game game, string gameArguments = "", ProcessPriority processPriority = ProcessPriority.Normal)
        {
            List<Process> startedProcesses = new List<Process>();
            if (game.Start(out startedProcesses, gameArguments, processPriority))
            {
                logger.Trace($"SteamLibrary/StartGame: Successfully started Steam game {game.Name}");
            }
            else
            {
                logger.Trace($"SteamLibrary/StartGame: Failed to start Steam game {game.Name}");
            }
            return startedProcesses;
        }

        public override bool StopGame(Game game)
        {
            if (game.Stop())
            {
                logger.Trace($"SteamLibrary/StopGame: Successfully stopped Steam game {game.Name}");
                return true;
            }
            else
            {
                logger.Trace($"SteamLibrary/StopGame: Failed to stop Steam game {game.Name}");
                return false;
            }
        }
        #endregion

    }

    [global::System.Serializable]
    public class SteamLibraryException : GameLibraryException
    {
        public SteamLibraryException() { }
        public SteamLibraryException(string message) : base(message) { }
        public SteamLibraryException(string message, Exception inner) : base(message, inner) { }
    }

}
