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
        private string steamAppIdRegex = @"/^[0-9A-F]{1,10}$";
        private string _steamExe;
        private string _steamPath;
        private string _steamConfigVdfFile;
        private List<string> _steamProcessList = new List<string>() { "steam"};
        private string _registrySteamKey = @"SOFTWARE\WOW6432Node\Valve\Steam"; // under LocalMachine
        private string _registryAppsKey = $@"SOFTWARE\Valve\Steam\Apps"; // under CurrentUser
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
                // Load the Steam Games from Steam Client if needed
                if (_allSteamGames.Count == 0)
                    LoadInstalledGames();
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
                    logger.Warn(ex, $"SteamLibrary/IsRunning: Exception while trying to get the steam library processes matching process names: {_steamProcessList.ToString()}.");
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
                    if (steamGameNameOrUuid.Equals(Convert.ToInt32(testSteamGame.Id)))
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
                    if (steamGameNameOrUuid.Equals(Convert.ToInt32(testSteamGame.Id)))
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
                    // Steam isn't installed, so we return an empty list.
                    logger.Info($"SteamLibrary/LoadInstalledGames: Steam library is not installed");
                    return false;
                }

                logger.Trace($"SteamLibrary/LoadInstalledGames: Steam Base Registry Key = HKLM\\{_registrySteamKey}");
                logger.Trace($"SteamLibrary/LoadInstalledGames: Steam Apps Registry Key = HKCU\\{_registryAppsKey}");

                List<string> steamAppIdsInstalled = new List<string>();
                // Now look for what games app id's are actually installed on this computer
                using (RegistryKey steamAppsKey = Registry.CurrentUser.OpenSubKey(_registryAppsKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (steamAppsKey != null)
                    {
                        //
                        // Loop through the subKeys as they are the Steam Game IDs
                        foreach (string steamAppId in steamAppsKey.GetSubKeyNames())
                        {
                            logger.Trace($"SteamLibrary/LoadInstalledGames: Found SteamGameKeyName = {steamAppId}");
                            if (!String.IsNullOrWhiteSpace(steamAppId))
                            {
                                logger.Trace($"SteamLibrary/LoadInstalledGames: SteamGameKeyName is an int, so trying to see if it is an installed app");
                                string steamGameKeyFullName = $"{_registryAppsKey}\\{steamAppId}";
                                using (RegistryKey steamGameKey = Registry.CurrentUser.OpenSubKey(steamGameKeyFullName, RegistryKeyPermissionCheck.ReadSubTree))
                                {
                                    // If the Installed Value is set to 1, then the game is installed
                                    // We want to keep track of that for later
                                    if ((int)steamGameKey.GetValue(@"Installed", 0) == 1)
                                    {
                                        logger.Trace($"SteamLibrary/LoadInstalledGames: {steamGameKeyFullName} contains an 'Installed' value so is an installed Steam App.");
                                        // Add this Steam App ID to the list we're keeping for later
                                        steamAppIdsInstalled.Add(steamAppId);
                                    }
                                    else
                                    {
                                        logger.Trace($"SteamLibrary/LoadInstalledGames: {steamGameKeyFullName} does not contain an 'Installed' value so can't be a Steam App.");
                                    }

                            }

                            }
                        }

                        if (steamAppIdsInstalled.Count == 0)
                        {
                            // There aren't any game ids so return false
                            logger.Warn($"SteamLibrary/LoadInstalledGames: No Steam games installed in the Steam library");
                            return false;
                        }
                        
                    }
                    else
                    {
                        // There isnt any steam registry key
                        logger.Warn($"SteamLibrary/LoadInstalledGames: Couldn't access the Steam Registry Key {_registrySteamKey}");
                        return false;
                    }
                }

                // Now we parse the steam appinfo.vdf to get access to things like:
                // - The game name
                // - THe game installation dir
                // - Sometimes the game icon
                // - Sometimes the game executable name (from which we can get the icon)
                Dictionary<string, SteamAppInfo> steamAppInfo = new Dictionary<string, SteamAppInfo>();

                string appInfoVdfFile = Path.Combine(_steamPath, "appcache", "appinfo.vdf");
                var newAppInfo = new AppInfo();
                newAppInfo.Read(appInfoVdfFile);

                logger.Trace($"SteamLibrary/LoadInstalledGames: Found {newAppInfo.Apps.Count} apps in the {appInfoVdfFile} VDF file");

                // Chec through all the apps we've extracted
                foreach (var app in newAppInfo.Apps)
                {
                    // We only care about the appIDs we have listed as actual games
                    // (The AppIds include all other DLC and Steam specific stuff too)
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

                            foreach (KVObject data in app.Data)
                            {
                                logger.Trace($"SteamLibrary/LoadInstalledGames: Found App: {app.AppID} - Data.Name: {data.Name}");

                                if (data.Name == "common")
                                {
                                    foreach (KVObject common in data.Children)
                                    {

                                        if (common.Name == "name")
                                        {
                                            logger.Trace($"SteamLibrary/LoadInstalledGames: name: App: {app.AppID} - Common {common.Name}: {common.Value}");
                                            steamGameAppInfo.GameName = common.Value.ToString();
                                        }
                                        else if (common.Name == "clienticon")
                                        {
                                            logger.Trace($"SteamLibrary/LoadInstalledGames: clienticon: App: {app.AppID} - Common {common.Name}: {common.Value}");
                                            steamGameAppInfo.GameIconPath = Path.Combine(_steamPath, @"steam", @"games", String.Concat(common.Value, @".ico"));
                                        }
                                        else if (common.Name == "type")
                                        {
                                            logger.Trace($"SteamLibrary/LoadInstalledGames: type: App: {app.AppID} - Common {common.Name}: {common.Value}");
                                            steamAppType = common.Value.ToString();
                                        }
                                        else
                                        {
                                            logger.Trace($"SteamLibrary/LoadInstalledGames: Found unrecognised line App: {app.AppID} - Common {common.Name}: {common.Value}");
                                        }
                                    }
                                }
                                else if (data.Name == "config")
                                {
                                    foreach (KVObject config in data.Children)
                                    {
                                        //Console.WriteLine($"App: {app.AppID} - Config {config.Name}: {config.Value}");

                                        if (config.Name == "installdir")
                                        {
                                            logger.Trace($"SteamLibrary/LoadInstalledGames: Found installdir App: {detectedAppID} - Config {config.Name}: {config.Value}");
                                            steamGameAppInfo.GameInstallDir = config.Value.ToString();
                                        }
                                        else if (config.Name == "launch")
                                        {
                                            foreach (KVObject launch in config.Children)
                                            {
                                                foreach (KVObject launch_num in launch.Children)
                                                {
                                                    if (launch_num.Name == "executable")
                                                    {
                                                        logger.Trace($"SteamLibrary/LoadInstalledGames: Found launch executable App: {detectedAppID} - Config - Launch {launch.Name} - {launch_num.Name}: {launch_num.Value}");
                                                        steamGameAppInfo.GameExes.Add(launch_num.Value.ToString());
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            // Only store the app if it's a game!
                            if (steamAppType.Equals("Game",StringComparison.OrdinalIgnoreCase))
                            {
                                steamAppInfo.Add(detectedAppID, steamGameAppInfo);
                                logger.Trace($"SteamLibrary/LoadInstalledGames: Adding Game with ID {detectedAppID} '{steamGameAppInfo.GameName}' to the list of games");
                            }
                                
                        }
                        catch (ArgumentException ex)
                        {
                            logger.Warn(ex, $"SteamLibrary/LoadInstalledGames: ArgumentException while processing the {appInfoVdfFile} VDF file");
                            //we just want to ignore it if we try to add it twice....
                        }

                        logger.Trace($"SteamLibrary/LoadInstalledGames: Found end of loop App: {detectedAppID} - Token: {app.Token}");
                    }
                }

                List<string> steamLibrariesPaths = new List<string>();
                // We add the default library which is based on where Steam was installed
                logger.Info($"SteamLibrary/LoadInstalledGames: Found original steam library {_steamPath}");
                steamLibrariesPaths.Add(_steamPath);

                // Now we access the LibraryFolders.vdf that lives in the Steamapps folder, as that lists all 
                // the SteamLibraries. We need to find out where they areso we can interrogate them
                string _steamLibraryFoldersVdfFile = Path.Combine(_steamPath, "steamapps", "libraryfolders.vdf");
                if (File.Exists(_steamLibraryFoldersVdfFile))
                {
                    logger.Trace($"SteamLibrary/LoadInstalledGames: Processing the {_steamLibraryFoldersVdfFile} VDF file looking for additional Steam Libraries");
                    string steamLibraryFoldersText = File.ReadAllText(_steamLibraryFoldersVdfFile, Encoding.UTF8);

                    logger.Trace($"SteamLibrary/LoadInstalledGames: Processing the {_steamLibraryFoldersVdfFile} VDF file");


                    // Now we have to parse the config.vdf looking for the location of any additional SteamLibraries
                    // We look for lines similar to this: "BaseInstallFolder_1"		"E:\\SteamLibrary"
                    // There may be multiple so we need to check the whole file
                    Regex steamLibrariesRegex = new Regex(@"\t""path""\t\t""(.*?)""\n", RegexOptions.IgnoreCase);
                    // Try to match all lines against the Regex.
                    MatchCollection steamLibrariesMatches = steamLibrariesRegex.Matches(steamLibraryFoldersText);
                    // If at least one of them matched!
                    foreach (Match steamLibraryMatch in steamLibrariesMatches)
                    {
                        if (steamLibraryMatch.Success)
                        {
                            // Check the entry is actually a directory 
                            string steamLibraryPath = Regex.Unescape(steamLibraryMatch.Groups[1].Value);
                            if (!steamLibraryPath.Equals(_steamPath) && Directory.Exists(steamLibraryPath))
                            {
                                if (!steamLibrariesPaths.Contains(steamLibraryPath))
                                {
                                    logger.Info($"SteamLibrary/LoadInstalledGames: Found additional steam library {steamLibraryPath} in the {_steamLibraryFoldersVdfFile} VDF file ");
                                    steamLibrariesPaths.Add(steamLibraryPath);
                                }
                            }
                            else
                            {
                                logger.Trace($"SteamLibrary/LoadInstalledGames: Found what it thought was an additional steam library {steamLibraryPath} in {_steamLibraryFoldersVdfFile}, but it didn't exist on the file system, or was already added");
                            }
                        }
                    }
                }

                _steamConfigVdfFile = Path.Combine(_steamPath, "config", "config.vdf");
                if (File.Exists(_steamConfigVdfFile))
                {
                    logger.Trace($"SteamLibrary/LoadInstalledGames: Processing the {_steamConfigVdfFile} VDF file as well as it was found too");
                    // Now we access the config.vdf that lives in the Steam Config file, as that lists all 
                    // the SteamLibraries. We need to find out where they areso we can interrogate them                    
                    string steamConfigVdfText = File.ReadAllText(_steamConfigVdfFile, Encoding.UTF8);

                    logger.Trace($"SteamLibrary/LoadInstalledGames: Processing the {_steamConfigVdfFile} VDF file");


                    // Now we have to parse the config.vdf looking for the location of any additional SteamLibraries
                    // We look for lines similar to this: "BaseInstallFolder_1"		"E:\\SteamLibrary"
                    // There may be multiple so we need to check the whole file
                    Regex steamLibrariesRegex = new Regex(@"""BaseInstallFolder_\d+""\s+""(.*)""", RegexOptions.IgnoreCase);
                    // Try to match all lines against the Regex.
                    MatchCollection steamLibrariesMatches = steamLibrariesRegex.Matches(steamConfigVdfText);
                    // If at least one of them matched!
                    foreach (Match steamLibraryMatch in steamLibrariesMatches)
                    {
                        if (steamLibraryMatch.Success)
                        {
                            string steamLibraryPath = Regex.Unescape(steamLibraryMatch.Groups[1].Value);
                            if (!steamLibraryPath.Equals(_steamPath) && Directory.Exists(steamLibraryPath))
                            {
                                logger.Info($"SteamLibrary/LoadInstalledGames: Found additional steam library {steamLibraryPath}");
                                // Check if the steam library is already in the list!
                                if (!steamLibrariesPaths.Contains(steamLibraryPath))
                                {
                                    logger.Info($"SteamLibrary/LoadInstalledGames: Aadditional steam library {steamLibraryPath}");
                                    steamLibrariesPaths.Add(steamLibraryPath);
                                }                                
                            }
                            else
                            {
                                logger.Trace($"SteamLibrary/LoadInstalledGames: Found what it thought was an additional steam library {steamLibraryPath} in {_steamConfigVdfFile}, but it didn't exist on the file system, or was already added");
                            }
                        }
                    }
                }
                

                
                // Now we go off and find the details for the games in each Steam Library
                foreach (string steamLibraryPath in steamLibrariesPaths)
                {
                    // Work out the path to the appmanifests for this steamLibrary
                    string steamLibraryAppManifestPath = Path.Combine(steamLibraryPath, @"steamapps");
                    try
                    {
                        // Get the names of the App Manifests for the games installed in this SteamLibrary
                        string[] steamLibraryAppManifestFilenames = Directory.GetFiles(steamLibraryAppManifestPath, "appmanifest_*.acf");
                        // Go through each app and extract it's details
                        foreach (string steamLibraryAppManifestFilename in steamLibraryAppManifestFilenames)
                        {
                            logger.Trace($"SteamLibrary/LoadInstalledGames: Found {steamLibraryAppManifestFilename} app manifest within steam library {steamLibraryPath}");
                            // Read in the contents of the file
                            string steamLibraryAppManifestText = File.ReadAllText(steamLibraryAppManifestFilename);
                            // Grab the appid from the file
                            Regex appidRegex = new Regex(@"""appid""\s+""(\d+)""", RegexOptions.IgnoreCase);
                            Match appidMatches = appidRegex.Match(steamLibraryAppManifestText);
                            if (appidMatches.Success)
                            {
                                if (!String.IsNullOrWhiteSpace(appidMatches.Groups[1].Value))
                                {
                                    string steamGameId = appidMatches.Groups[1].Value;
                                    logger.Trace($"SteamLibrary/LoadInstalledGames: Found Steam Game ID {steamGameId} within {steamLibraryAppManifestFilename} steam app manifest within steam library {steamLibraryPath}");
                                    // Check if this game is one that was installed
                                    if (steamAppInfo.ContainsKey(steamGameId))
                                    {
                                        logger.Trace($"SteamLibrary/LoadInstalledGames: Steam Game ID {steamGameId} is installed within steam library {steamLibraryPath}!");
                                        // This game is an installed game! so we start to populate it with data!
                                        string steamGameExe = "";

                                        string steamGameName = steamAppInfo[steamGameId].GameName;

                                        // Construct the full path to the game dir from the appInfo and libraryAppManifest data
                                        string steamGameInstallDir = Path.Combine(steamLibraryPath, @"steamapps", @"common", steamAppInfo[steamGameId].GameInstallDir);

                                        logger.Trace($"SteamLibrary/LoadInstalledGames: Looking for Steam Game ID {steamGameId} at {steamGameInstallDir }");

                                        // And finally we try to populate the 'where', to see what gets run
                                        // And so we can extract the process name
                                        if (steamAppInfo[steamGameId].GameExes.Count > 0)
                                        {
                                            foreach (string gameExe in steamAppInfo[steamGameId].GameExes)
                                            {
                                                steamGameExe = Path.Combine(steamGameInstallDir, gameExe);
                                                logger.Trace($"SteamLibrary/LoadInstalledGames: Looking for Steam Game Exe {steamGameExe} for Steam Game ID {steamGameId} at {steamGameInstallDir }");
                                                // If the game executable exists, then we can proceed
                                                if (File.Exists(steamGameExe))
                                                {
                                                    logger.Debug($"SteamLibrary/LoadInstalledGames: Found Steam Game Exe {steamGameExe} for Steam Game ID {steamGameId} at {steamGameInstallDir }");
                                                    break;
                                                }
                                            }

                                        }

                                        // Next, we need to get the Icons we want to use, and make sure it's the latest one.
                                        string steamGameIconPath = "";
                                        // First of all, we attempt to use the Icon that Steam has cached, if it's available, as that will be updated to the latest
                                        if (File.Exists(steamAppInfo[steamGameId].GameIconPath) && steamAppInfo[steamGameId].GameIconPath.EndsWith(".ico"))
                                        {
                                            steamGameIconPath = steamAppInfo[steamGameId].GameIconPath;
                                            logger.Debug($"SteamLibrary/LoadInstalledGames: Found Steam Game Icon Path {steamGameIconPath} for Steam Game ID {steamGameId} at {steamGameInstallDir }");

                                        }
                                        // If there isn't an icon for us to use, then we need to extract one from the Game Executables
                                        else if (!String.IsNullOrEmpty(steamGameExe))
                                        {
                                            steamGameIconPath = steamGameExe;
                                            logger.Debug($"SteamLibrary/LoadInstalledGames: Found Steam Game Icon Path {steamGameIconPath} for Steam Game ID {steamGameId} at {steamGameInstallDir }");
                                        }
                                        // The absolute worst case means we don't have an icon to use. SO we use the Steam one.
                                        else
                                        {
                                            // And we have to make do with a Steam Icon
                                            logger.Debug($"SteamLibrary/LoadInstalledGames: Couldn't find Steam Game Icon Path {steamGameIconPath} for Steam Game ID {steamGameId} so using default Steam Icon");
                                            steamGameIconPath = _steamPath;
                                        }

                                        // And we add the Game to the list of games we have!
                                        SteamGame gameToAdd = new SteamGame(steamGameId, steamGameName, steamGameExe, steamGameIconPath);
                                        _allSteamGames.Add(gameToAdd);
                                        logger.Debug($"SteamLibrary/LoadInstalledGames: Adding Steam Game with game id {steamGameId}, name {steamGameName}, game exe {steamGameExe} and icon path {steamGameIconPath}");
                                    }
                                }
                            }
                        }
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        logger.Warn(ex, $"SteamLibrary/LoadInstalledGames: Exception: Steam Library {steamLibraryPath} doesn't contain 'steamapps' subfolder . This is most likely because the Steam Library doesn't have any games installed in it.");
                    }
                    catch (PathTooLongException ex)
                    {
                        logger.Warn(ex, $"SteamLibrary/LoadInstalledGames: Exception: Steam Library {steamLibraryPath} is too long for Windows to access. Please make the path shorter by moving the directory.");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        logger.Warn(ex, $"SteamLibrary/LoadInstalledGames: Exception: We don't have permission to access Steam Library {steamLibraryPath}. Please check you are able to access {steamLibraryPath} in Windows Explorer.");
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"SteamLibrary/LoadInstalledGames: Exception: General exception while trying to scan Steam Library {steamLibraryPath} for games.");
                    }
                }
                logger.Info($"SteamLibrary/LoadInstalledGames: Found {_allSteamGames.Count} installed Steam games");
            }
            catch (ArgumentNullException ex)
            {
                logger.Warn(ex, "SteamLibrary/GetAllInstalledGames: An argument supplied to the function is null.");
            }
            catch (NotSupportedException ex)
            {
                logger.Warn(ex, "SteamLibrary/GetAllInstalledGames: The invoked method is not supported or reading, seeking or writing tp a stream that isn't supported.");
            }
            catch (PathTooLongException ex)
            {
                logger.Warn(ex, "SteamLibrary/GetAllInstalledGames: The path is longer than the maximum allowed by the operating system.");
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "SteamLibrary/GetAllInstalledGames: The user does not have the permissions required to read the Steam InstallDir registry key.");
            }
            catch (ObjectDisposedException ex)
            {
                logger.Warn(ex, "SteamLibrary/GetAllInstalledGames: The Microsoft.Win32.RegistryKey is closed when trying to access the Steam InstallDir registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "SteamLibrary/GetAllInstalledGames: The Steam InstallDir registry key has been marked for deletion so we cannot access the value dueing the SteamLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "SteamLibrary/GetAllInstalledGames: The user does not have the necessary registry rights to check whether Steam is installed.");
            }

            return true;
        }


        public override List<Process> StartGame(Game game, string gameArguments = "", ProcessPriority processPriority = ProcessPriority.Normal)
        {
            string address = $@"steam://rungameid/{game.Id}";
            if (!String.IsNullOrWhiteSpace(gameArguments))
            {
                address += @"//" + gameArguments;
            }
            //Process gameProcess = Process.Start(address);
            List<Process> gameProcesses = ProcessUtils.StartProcess(address,null,processPriority);

            // Wait 1 second then see if we need to find the child processes.

            return gameProcesses;
        }
        #endregion

    }

    [global::System.Serializable]
    public class SteamLibraryException : GameLibraryException
    {
        public SteamLibraryException() { }
        public SteamLibraryException(string message) : base(message) { }
        public SteamLibraryException(string message, Exception inner) : base(message, inner) { }
        protected SteamLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
