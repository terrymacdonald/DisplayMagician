using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.IO;
using System.Security;
using System.Diagnostics;
using ProtoBuf;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Globalization;

namespace DisplayMagician.GameLibraries
{
    public class UplayLibrary : GameLibrary
    {


        #region Class Variables
        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static readonly UplayLibrary _instance = new UplayLibrary();
       
        // Common items to the class
        private List<Game> _allGames = new List<Game>();
        private string uplayAppIdRegex = @"/^[0-9A-F]{1,10}$";
        private bool _isUplayInstalled = false;
        private string _uplayExe;
        private string _uplayPath;
        private List<string> _uplayProcessList = new List<string>() { "UbisoftGameLauncher", "UbisoftGameLauncher64" };
        //private string _uplayConfigVdfFile;
        internal string registryUplayLauncherKey = @"SOFTWARE\WOW6432Node\Ubisoft\Launcher";
        internal string registryUplayInstallsKey = @"SOFTWARE\WOW6432Node\Ubisoft\Launcher\Installs";
        internal string registryUplayOpenCmdKey = @"SOFTWARE\Classes\uplay\Shell\Open\Command";
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        // Other constants that are useful
        #endregion


        #region Class Constructors
        static UplayLibrary() { }

        private UplayLibrary()
        {
            try
            {
                logger.Trace($"UplayLibrary/UplayLibrary: Uplay launcher registry key = HKLM\\{registryUplayLauncherKey}");
                // Find the UplayExe location, and the UplayPath for later
                RegistryKey uplayInstallKey = Registry.LocalMachine.OpenSubKey(registryUplayLauncherKey, RegistryKeyPermissionCheck.ReadSubTree);
                if (uplayInstallKey == null)
                {
                    logger.Info($"UplayLibrary/UplayLibrary: Uplay library is not installed!");
                    return;
                }
                _uplayPath = uplayInstallKey.GetValue("InstallDir", "C:\\Program Files (x86)\\Ubisoft\\Ubisoft Game Launcher\\").ToString();
                _uplayExe = $"{_uplayPath}upc.exe";
                if (File.Exists(_uplayExe))
                {
                    logger.Info($"UplayLibrary/UplayLibrary: Uplay library is installed in {_uplayPath}. Found {_uplayExe}");
                    _isUplayInstalled = true;
                }
                else
                {
                    logger.Info($"UplayLibrary/UplayLibrary: Uplay library is not installed!");
                }
                   
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "UplayLibrary/UplayLibrary: The user does not have the permissions required to read the Uplay InstallDir registry key.");
            }
            catch(ObjectDisposedException ex)
            {
                logger.Warn(ex, "UplayLibrary/UplayLibrary: The Microsoft.Win32.RegistryKey is closed when trying to access the Uplay InstallDir registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "UplayLibrary/UplayLibrary: The Uplay InstallDir registry key has been marked for deletion so we cannot access the value dueing the UplayLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "UplayLibrary/UplayLibrary: The user does not have the necessary registry rights to check whether Uplay is installed.");
            }
        }
        #endregion

        #region Class Properties
        public override List<Game> AllInstalledGames
        {
            get
            {
                // Load the Uplay Games from Uplay Client if needed
                if (_allGames.Count == 0)
                    LoadInstalledGames();
                return _allGames;
            }
        }


        public override int InstalledGameCount
        {
            get
            {
                return _allGames.Count;
            }
        }

        public override string GameLibraryName
        {
            get
            {
                return "Uplay";
            }
        }

        public override SupportedGameLibraryType GameLibraryType
        {
            get
            {
                return SupportedGameLibraryType.Uplay;
            }
        }

        public override string GameLibraryExe
        {
            get
            {
                return _uplayExe;
            }
        }

        public override string GameLibraryPath
        {
            get
            {
                return _uplayPath;
            }
        }

        public override bool IsGameLibraryInstalled
        {
            get
            {
                return _isUplayInstalled;
            }

        }

        public override bool IsRunning
        {
            get
            {
                List<Process> uplayLibraryProcesses = new List<Process>();

                foreach (string uplayLibraryProcessName in _uplayProcessList)
                {
                    // Look for the processes with the ProcessName we sorted out earlier
                    uplayLibraryProcesses.AddRange(Process.GetProcessesByName(uplayLibraryProcessName));
                }

                // If we have found one or more processes then we should be good to go
                // so let's break, and get to the next step....
                if (uplayLibraryProcesses.Count > 0)
                    return true;
                else
                    return false;
            }

        }

        public override bool IsUpdating
        {
            get
            {
                // Not implemeted at present
                // so we just return a false
                // TODO Implement Uplay specific detection for updating the game client
                return false;
            }

        }

        public override List<string> GameLibraryProcesses
        {
            get
            {
                return _uplayProcessList;
            }
        }


        #endregion

        #region Class Methods
        public static UplayLibrary GetLibrary()
        {
            return _instance;
        }

        public override bool AddGame(Game uplayGame)
        {
            if (!(uplayGame is UplayGame))
                return false;
            
            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (ContainsGame(uplayGame))
            {
                logger.Debug($"UplayLibrary/AddGame: Updating Uplay game {uplayGame.Name} in our Uplay library");
                // We update the existing Shortcut with the data over
                UplayGame uplayGameToUpdate = (UplayGame)GetGame(uplayGame.Id.ToString());
                uplayGame.CopyTo(uplayGameToUpdate);
            }
            else
            {
                logger.Debug($"UplayLibrary/AddGame: Adding Uplay game {uplayGame.Name} to our Uplay library");
                // Add the uplayGame to the list of uplayGames
                _allGames.Add(uplayGame);
            }

            //Doublecheck it's been added
            if (ContainsGame(uplayGame))
            {
                return true;
            }
            else
                return false;

        }

        public override bool RemoveGame(Game uplayGame)
        {
            if (!(uplayGame is Game))
                return false;

            logger.Debug($"UplayLibrary/RemoveGame: Removing Uplay game {uplayGame.Name} from our Uplay library");

            // Remove the uplayGame from the list.
            int numRemoved = _allGames.RemoveAll(item => item.Id.Equals(uplayGame.Id));

            if (numRemoved == 1)
            {
                logger.Debug($"UplayLibrary/RemoveGame: Removed Uplay game with name {uplayGame.Name}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"UplayLibrary/RemoveGame: Didn't remove Uplay game with ID {uplayGame.Name} from the Uplay Library");
                return false;
            }                
            else
                throw new UplayLibraryException();
        }

        public override bool RemoveGameById(string uplayGameId)
        {
            if (uplayGameId.Equals(0))
                return false;

            logger.Debug($"UplayLibrary/RemoveGame2: Removing Uplay game with ID {uplayGameId} from the Uplay library");

            // Remove the uplayGame from the list.
            int numRemoved = _allGames.RemoveAll(item => item.Id.Equals(uplayGameId));

            if (numRemoved == 1)
            {
                logger.Debug($"UplayLibrary/RemoveGame2: Removed Uplay game with ID {uplayGameId}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"UplayLibrary/RemoveGame2: Didn't remove Uplay game with ID {uplayGameId} from the Uplay Library");
                return false;
            }
            else
                throw new UplayLibraryException();
        }

        public override bool RemoveGame(string uplayGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(uplayGameNameOrId))
                return false;

            logger.Debug($"UplayLibrary/RemoveGame3: Removing Uplay game with Name or ID {uplayGameNameOrId} from the Uplay library");

            int numRemoved;
            Match match = Regex.Match(uplayGameNameOrId, uplayAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                numRemoved = _allGames.RemoveAll(item => uplayGameNameOrId.Equals(item.Id));
            else
                numRemoved = _allGames.RemoveAll(item => uplayGameNameOrId.Equals(item.Name));

            if (numRemoved == 1)
            {
                logger.Debug($"UplayLibrary/RemoveGame3: Removed Uplay game with Name or UUID {uplayGameNameOrId} ");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"UplayLibrary/RemoveGame3: Didn't remove Uplay game with Name or UUID {uplayGameNameOrId} from the Uplay Library");
                return false;
            }
            else
                throw new UplayLibraryException();

        }

        public override bool ContainsGame(Game uplayGame)
        {
            if (!(uplayGame is UplayGame))
                return false;

            foreach (UplayGame testGame in _allGames)
            {
                if (testGame.Id.Equals(uplayGame.Id))
                    return true;
            }

            return false;
        }

        public override bool ContainsGameById(string uplayGameId)
        {
            foreach (UplayGame testGame in _allGames)
            {
                if (uplayGameId == testGame.Id)
                    return true;
            }


            return false;

        }

        public override bool ContainsGame(string uplayGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(uplayGameNameOrId))
                return false;


            Match match = Regex.Match(uplayGameNameOrId, uplayAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (UplayGame testGame in _allGames)
                {
                    if (uplayGameNameOrId.Equals(Convert.ToInt32(testGame.Id)))
                        return true;
                }

            }
            else
            {
                foreach (UplayGame testGame in _allGames)
                {
                    if (uplayGameNameOrId.Equals(testGame.Name))
                        return true;
                }

            }

            return false;

        }


        public override Game GetGame(string uplayGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(uplayGameNameOrId))
                return null;

            Match match = Regex.Match(uplayGameNameOrId, uplayAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (UplayGame testGame in _allGames)
                {
                    if (uplayGameNameOrId.Equals(Convert.ToInt32(testGame.Id)))
                        return testGame;
                }

            }
            else
            {
                foreach (UplayGame testGame in _allGames)
                {
                    if (uplayGameNameOrId.Equals(testGame.Name))
                        return testGame;
                }

            }

            return null;

        }

        public override Game GetGameById(string uplayGameId)
        {
            foreach (UplayGame testGame in _allGames)
            {
                if (uplayGameId == testGame.Id)
                    return testGame;
            }

            return null;

        }

        public bool GetInstallDirFromRegKey(string regKeyPath, out string filePath)
        {
            filePath = "";

            RegistryKey uplayGameInstallKey;
            if (regKeyPath.StartsWith("HKEY_LOCAL_MACHINE"))
            {
                logger.Trace($"UplayLibrary/GetInstallDirFromRegKey: Accessing HKLM reg key {regKeyPath}");
                string regKeyText = regKeyPath.Replace(@"HKEY_LOCAL_MACHINE\", "");
                uplayGameInstallKey = Registry.LocalMachine.OpenSubKey(regKeyText, RegistryKeyPermissionCheck.ReadSubTree);
            }
            else if (regKeyPath.StartsWith("HKEY_CURRENT_USER"))
            {
                logger.Trace($"UplayLibrary/GetInstallDirFromRegKey: Accessing HKCU reg key {regKeyPath}");
                string regKeyText = regKeyPath.Replace(@"HKEY_CURRENT_USER\", "");
                uplayGameInstallKey = Registry.LocalMachine.OpenSubKey(regKeyText, RegistryKeyPermissionCheck.ReadSubTree);
            }
            else
            {
                logger.Trace($"UplayLibrary/GetInstallDirFromRegKey: Skipping processing as regkey supplied was odd: {regKeyPath}");
                return false;
            }

            // If the key doesn't exist we skip it as the game isn't installed any longer!
            if (uplayGameInstallKey == null)
            {
                logger.Trace($"UplayLibrary/GetInstallDirFromRegKey: Skipping Uplay Game as it isn't installed at the moment (it was uninstalled at some point)");
                return false;
            }

            // From that we lookup the actual game path
            string gameInstallDir = uplayGameInstallKey.GetValue("InstallDir", "").ToString();
            logger.Trace($"UplayLibrary/GetInstallDirFromRegKey: gameInstallDir found  = {gameInstallDir}");
            if (!String.IsNullOrWhiteSpace(gameInstallDir))
            {
                filePath = Path.GetFullPath(gameInstallDir).TrimEnd('\\');
                return true;
            }
            else
            {
                logger.Warn($"UplayLibrary/GetInstallDirFromRegKey: gameInstallDir is null or all whitespace!");
                return false;
            }
        }


        public override bool LoadInstalledGames()
        {
            try
            {

                if (!_isUplayInstalled)
                {
                    // Uplay isn't installed, so we return an empty list.
                    logger.Info($"UplayLibrary/LoadInstalledGames: Uplay library is not installed");
                    return false;
                }

                logger.Trace($"UplayLibrary/LoadInstalledGames: Uplay Game Installs Registry Key = HKLM\\{registryUplayInstallsKey}");

                using (RegistryKey uplayInstallKey = Registry.LocalMachine.OpenSubKey(registryUplayInstallsKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (uplayInstallKey != null)
                    {
                        int uplayGamesInstalledCount = 0;
                        // Loop through the subKeys as they are the Steam Game IDs
                        foreach (string uplayGameKeyName in uplayInstallKey.GetSubKeyNames())
                        {
                            logger.Trace($"UplayLibrary/LoadInstalledGames: Found uplayGameKeyName = {uplayGameKeyName}");
                            if (int.TryParse(uplayGameKeyName, out int uplayGameId))
                            {
                                logger.Trace($"UplayLibrary/LoadInstalledGames: uplayGameKeyName is an int, so trying to see if it is a game");
                                string uplayGameKeyFullName = $"{registryUplayInstallsKey}\\{uplayGameKeyName}";
                                using (RegistryKey uplayGameKey = Registry.LocalMachine.OpenSubKey(uplayGameKeyFullName, RegistryKeyPermissionCheck.ReadSubTree))
                                {
                                    // If the Installed Value is set to 1, then the game is installed
                                    // We want to keep track of that for later
                                    if (!uplayGameKey.GetValue(@"InstallDir", "").ToString().Equals(""))
                                    {
                                        logger.Trace($"UplayLibrary/LoadInstalledGames: {uplayGameKey} contains an 'InstallDir' value so is an installed Uplay Game.");
                                        // Add this Steam App ID to the list we're keeping for later
                                        uplayGamesInstalledCount++;
                                    }
                                    else
                                    {
                                        logger.Trace($"UplayLibrary/LoadInstalledGames: {uplayGameKey} does not contain an 'Installed' value so can't be a Uplay Game.");
                                    }

                                }
                            }
                        }

                        if (uplayGamesInstalledCount == 0)
                        {
                            // There aren't any game ids so return false
                            logger.Warn($"UplayLibrary/LoadInstalledGames: No Uplay games installed in the Uplay library");
                            return false;
                        }
                        else
                        {
                            logger.Info($"UplayLibrary/LoadInstalledGames: Found {uplayGamesInstalledCount} installed games in the Uplay library");
                        }

                    }
                    else
                    {
                        // There isnt any Uplay registry key
                        logger.Warn($"UplayLibrary/LoadInstalledGames: Couldn't access the Uplay Registry Key {registryUplayInstallsKey}");
                        return false;
                    }
                }
                // Look in HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Ubisoft\\Launcher and check the InstallDir key
                // That returns the location of the install dir : E:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\

                // Access {installdir}\\cache\\configuration\\configurations file
                string uplayConfigFilePath = _uplayPath + @"cache\configuration\configurations";
                logger.Trace($"UplayLibrary/LoadInstalledGames: Uplay Config File Path = {uplayConfigFilePath }");

                var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

                using (var file = File.OpenRead(uplayConfigFilePath))
                {
                    try
                    {
                        var gameCollection = ProtoBuf.Serializer.Deserialize<UplayCachedGameCollection>(file).Games;
                        foreach (var item in gameCollection)
                        {
                            if (!String.IsNullOrEmpty(item.GameInfo))
                            {
                                ProductInformation productInfo;
                                try
                                {
                                    productInfo = deserializer.Deserialize<ProductInformation>(item.GameInfo);
                                    var root = productInfo.root;

                                    string gameId = ""; 
                                    string gameName = "";
                                    string gameExePath = "";
                                    string gameIconPath = "";

                                    // Try finding the Game Name using the localisation currently in use as a first step
                                    logger.Trace($"UplayLibrary/LoadInstalledGames: Looking for the Uplay game name.");
                                    string currentLang = CultureInfo.CurrentCulture.Name;                                    
                                    foreach (var lang in productInfo.localizations)
                                    {
                                        // If we find the same language as the user is using, then let's use that!
                                        if (lang.Key.Equals(currentLang))
                                        {
                                            gameName = lang.Value.l1;
                                            logger.Trace($"UplayLibrary/LoadInstalledGames: We found the Uplay game name '{gameName}' in the user's language of {currentLang}.");
                                            break;
                                        }
                                    }
                                    // If the gameName isn't available in the users language, then we go for default
                                    if (String.IsNullOrEmpty(gameName) && productInfo.localizations.ContainsKey("default"))
                                    {
                                        gameName = productInfo.localizations["default"].l1;
                                        if (!String.IsNullOrEmpty(gameName))
                                        {
                                            logger.Trace($"UplayLibrary/LoadInstalledGames: Looking for the Uplay game name with the en language as the local language didn't work. We found game name '{gameName}'. ");
                                        }
                                        else
                                        {
                                            logger.Trace($"UplayLibrary/LoadInstalledGames: Looking for the Uplay game name with the en language as the local language didn't work. We found no en language. ");
                                        }
                                    }
                                    

                                    // Now we'll try to sort out the rest of the game data!
                                    // We first look for the online executable information
                                    if (root.start_game.online.executables.Count > 0)
                                    {
                                        logger.Trace($"UplayLibrary/LoadInstalledGames: Uplay game {gameName} has some online executables to process! ");

                                        // First up we look at the online games, cause they're just better!
                                        foreach (var executable in root.start_game.online.executables)
                                        {
                                            string exePath = "";

                                            // Check if its a full path or a relative path
                                            if (!String.IsNullOrEmpty(executable.path.relative))
                                            {
                                                if (executable.working_directory.register.StartsWith("HKEY_LOCAL_MACHINE"))
                                                {
                                                    // This copes with relative files using a HKEY_LOCAL_MACHINE registry
                                                    
                                                    string regKeyText = executable.working_directory.register;
                                                    regKeyText = regKeyText.Replace(@"\InstallDir", "");
                                                    regKeyText = regKeyText.Replace(@"Ubisoft", @"WOW6432Node\Ubisoft");
                                                    logger.Trace($"UplayLibrary/GetInstallDirFromRegKey: Accessing HKLM reg key {regKeyText}");
                                                    if (this.GetInstallDirFromRegKey(regKeyText, out exePath))
                                                    {
                                                        gameExePath = Path.Combine(exePath, executable.path.relative);                                                        
                                                        logger.Trace($"UplayLibrary/LoadInstalledGames: Relative executable uses local machine registry key: {executable.working_directory.register} ");
                                                    }
                                                    // Get the GameID from the reg key
                                                    string pattern = @"Installs\\(\d+)\\InstallDir";
                                                    MatchCollection mc = Regex.Matches(executable.working_directory.register, pattern);
                                                    if (mc.Count > 0)
                                                    {
                                                        gameId = mc[0].Groups[1].Value;
                                                    }
                                                    logger.Trace($"UplayLibrary/LoadInstalledGames: Got uplay Game ID: {gameId} ");
                                                }
                                                /*else if (executable.working_directory.register.StartsWith("HKEY_CURRENT_USER"))
                                                {
                                                    // This copes with relative files using a HKEY_CURRENT_USER registry

                                                    string regKeyText = executable.working_directory.register;
                                                    regKeyText = regKeyText.Replace(@"\InstallDir", "");
                                                    regKeyText = regKeyText.Replace(@"Ubisoft", @"WOW6432Node\Ubisoft");
                                                    logger.Trace($"UplayLibrary/GetInstallDirFromRegKey: Accessing HKLM reg key {regKeyText}");

                                                    if (this.GetInstallDirFromRegKey(executable.working_directory.register, out exePath))
                                                    {
                                                        gameExePath = Path.Combine(exePath, executable.path.relative);
                                                        logger.Trace($"UplayLibrary/LoadInstalledGames: Relative executable uses current user registry key: {executable.working_directory.register} ");
                                                    }
                                                    // Get the GameID from the reg key
                                                    string pattern = @"Installs\\(\d+)\\InstallDir";
                                                    MatchCollection mc = Regex.Matches(executable.working_directory.register, pattern);
                                                    if (mc.Count > 0)
                                                    {
                                                        gameId = mc[0].Groups[1].Value;
                                                    }
                                                    logger.Trace($"UplayLibrary/LoadInstalledGames: Got uplay Game ID: {gameId} ");
                                                }*/
                                                else if (!String.IsNullOrEmpty(executable.working_directory.append))
                                                {
                                                    // This copes with relative files using an appended path
                                                    gameExePath = Path.Combine(executable.working_directory.append, executable.path.relative);
                                                    gameIconPath = Path.Combine(executable.working_directory.append, executable.icon_image);
                                                    logger.Trace($"UplayLibrary/LoadInstalledGames: Relative executable uses appended file path: {executable.working_directory.append} ");
                                                    gameId = productInfo.uplay_id.ToString();
                                                    logger.Trace($"UplayLibrary/LoadInstalledGames: Got uplay Game ID: {gameId} ");
                                                }
                                                else
                                                {
                                                    // Problem!
                                                    logger.Error($"UplayLibrary/LoadInstalledGames: Found relative GameExePath {executable.path.relative} for Uplay game {gameName} but no registry key or appended file path! Skipping this game.");
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                // This should cope with full pathed files, but we have no examples to test! So log it
                                                logger.Error($"UplayLibrary/LoadInstalledGames: Found non-relative GameExePath {executable.path} for Uplay game {gameName} but we've not seen it before so no idea how to handle it! Skipping this game.");
                                                logger.Error($"UplayLibrary/LoadInstalledGames: executable.path for troubleshooting: {executable.path}");
                                                continue;
                                            }

                                            // We should check the exe file exists, and if it doesn't then we need to do the next exe
                                            if (!File.Exists(gameExePath))
                                            {
                                                logger.Error($"UplayLibrary/LoadInstalledGames: Couldn't find the GameExePath {gameExePath} for Uplay game {gameName} so skipping this exe, and trying the next one.");
                                                continue;
                                            }

                                            // Now try to get the Uplay game icon
                                            if (!String.IsNullOrEmpty(root.icon_image))
                                            {
                                                gameIconPath = Path.Combine(_uplayPath, "data", "games", root.icon_image);

                                                // If the icon file isn't actually there, then use the game exe instead.
                                                if (!File.Exists(gameIconPath))
                                                {
                                                    gameIconPath = gameExePath;
                                                }
                                            }
                                                
                                            logger.Trace($"UplayLibrary/LoadInstalledGames: Found GameExePath {exePath} and Icon Path {gameIconPath} for Uplay game {gameName}.");

                                            // We do a final check to make sure that we do have a GameName, and if not we use the shortcut
                                            if (String.IsNullOrEmpty(gameName) && !String.IsNullOrEmpty(executable.shortcut_name))
                                            {
                                                gameName = executable.shortcut_name;
                                                logger.Trace($"UplayLibrary/LoadInstalledGames: Game Name was still empty, so we're using the shortcut name as a last resort: {executable.shortcut_name} ");
                                            }

                                            // Now we need to save the game name, cause if we're here then we're good enough to save
                                            // Then we have the gameID, the thumbimage, the icon, the name, the exe path
                                            // And we add the Game to the list of games we have!
                                            _allGames.Add(new UplayGame(gameId, gameName, gameExePath, gameIconPath));
                                            logger.Trace($"UplayLibrary/LoadInstalledGames: Adding Uplay Game with game id {productInfo.uplay_id}, name {gameName}, game exe {gameExePath} and icon path {gameIconPath}");
                                            break;
                                        }

                                    }
                                    // This is the offline exes
                                    else if (root.start_game.offline.executables.Count > 0)
                                    {
                                        logger.Trace($"UplayLibrary/LoadInstalledGames: Uplay game {gameName} has some offline executables to process! ");

                                        // we look at the offline games, cause there weren't any online ones
                                        foreach (var executable in root.start_game.offline.executables)
                                        {
                                            string exePath = "";

                                            // Check if its a full path or a relative path
                                            if (!String.IsNullOrEmpty(executable.path.relative))
                                            {
                                                if (executable.working_directory.register.StartsWith("HKEY_LOCAL_MACHINE"))
                                                {
                                                    // This copes with relative files using a HKEY_LOCAL_MACHINE registry

                                                    string regKeyText = executable.working_directory.register;
                                                    regKeyText = regKeyText.Replace(@"\InstallDir", "");
                                                    regKeyText = regKeyText.Replace(@"Ubisoft", @"WOW6432Node\Ubisoft");
                                                    logger.Trace($"UplayLibrary/GetInstallDirFromRegKey: Accessing HKLM reg key {regKeyText}");

                                                    if (this.GetInstallDirFromRegKey(regKeyText, out exePath))
                                                    {
                                                        gameExePath = Path.Combine(exePath, executable.path.relative);
                                                        logger.Trace($"UplayLibrary/LoadInstalledGames: Relative executable uses local machine registry key: {executable.working_directory.register} ");
                                                    }
                                                    // Get the GameID from the reg key
                                                    string pattern = @"Installs\\(\d+)\\InstallDir";
                                                    MatchCollection mc = Regex.Matches(executable.working_directory.register, pattern);
                                                    if (mc.Count > 0)
                                                    {
                                                        gameId = mc[0].Groups[1].Value;
                                                    }
                                                    logger.Trace($"UplayLibrary/LoadInstalledGames: Got uplay Game ID: {gameId} ");

                                                }
                                                /*else if (executable.working_directory.register.StartsWith("HKEY_CURRENT_USER"))
                                                {
                                                    // This copes with relative files using a HKEY_CURRENT_USER registry

                                                    string regKeyText = executable.working_directory.register;
                                                    regKeyText = regKeyText.Replace(@"\InstallDir", "");
                                                    regKeyText = regKeyText.Replace(@"Ubisoft", @"WOW6432Node\Ubisoft");
                                                    logger.Trace($"UplayLibrary/GetInstallDirFromRegKey: Accessing HKLM reg key {regKeyText}");

                                                    if (this.GetInstallDirFromRegKey(executable.working_directory.register, out exePath))
                                                    {
                                                        gameExePath = Path.Combine(exePath, executable.path.relative);
                                                        logger.Trace($"UplayLibrary/LoadInstalledGames: Relative executable uses current user registry key: {executable.working_directory.register} ");
                                                    }
                                                    // Get the GameID from the reg key
                                                    string pattern = @"Installs\\(\d+)\\InstallDir";
                                                    MatchCollection mc = Regex.Matches(executable.working_directory.register, pattern);
                                                    if (mc.Count > 0)
                                                    {
                                                        gameId = mc[0].Groups[1].Value;
                                                    }
                                                    logger.Trace($"UplayLibrary/LoadInstalledGames: Got uplay Game ID: {gameId} ");
                                                }*/
                                                else if (!String.IsNullOrEmpty(executable.working_directory.append))
                                                {
                                                    // This copes with relative files using an appended path
                                                    gameExePath = Path.Combine(executable.working_directory.append, executable.path.relative);
                                                    gameIconPath = Path.Combine(executable.working_directory.append, executable.icon_image);
                                                    logger.Trace($"UplayLibrary/LoadInstalledGames: Relative executable uses appended file path: {executable.working_directory.append} ");
                                                    gameId = productInfo.uplay_id.ToString();
                                                    logger.Trace($"UplayLibrary/LoadInstalledGames: Got uplay Game ID: {gameId} ");
                                                }
                                                else
                                                {
                                                    // Problem!
                                                    logger.Error($"UplayLibrary/LoadInstalledGames: Found relative GameExePath {executable.path.relative} for Uplay game {gameName} but no registry key or appended file path! Skipping this game.");
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                // This should cope with full pathed files, but we have no examples to test! So log it
                                                logger.Error($"UplayLibrary/LoadInstalledGames: Found non-relative GameExePath {executable.path} for Uplay game {gameName} but we've not seen it before so no idea how to handle it! Skipping this game.");
                                                logger.Error($"UplayLibrary/LoadInstalledGames: executable.path for troubleshooting: {executable.path}");
                                                continue;
                                            }

                                            // We should check the exe file exists, and if it doesn't then we need to do the next exe
                                            if (!File.Exists(gameExePath))
                                            {
                                                logger.Error($"UplayLibrary/LoadInstalledGames: Couldn't find the GameExePath {gameExePath} for Uplay game {gameName} so skipping this exe, and trying the next one.");
                                                continue;
                                            }

                                            // Now try to get the Uplay game icon
                                            if (!String.IsNullOrEmpty(root.icon_image))
                                            {
                                                gameIconPath = Path.Combine(_uplayPath, "data", "games", root.icon_image);

                                                // If the icon file isn't actually there, then use the game exe instead.
                                                if (!File.Exists(gameIconPath))
                                                {
                                                    gameIconPath = gameExePath;
                                                }
                                            }

                                            logger.Trace($"UplayLibrary/LoadInstalledGames: Found GameExePath {exePath} and Icon Path {gameIconPath} for Uplay game {gameName}.");

                                            // We do a final check to make sure that we do have a GameName, and if not we use the shortcut
                                            if (String.IsNullOrEmpty(gameName) && !String.IsNullOrEmpty(executable.shortcut_name))
                                            {
                                                gameName = executable.shortcut_name;
                                                logger.Trace($"UplayLibrary/LoadInstalledGames: Game Name was still empty, so we're using the shortcut name as a last resort: {executable.shortcut_name} ");
                                            }

                                            // Now we need to save the game name, cause if we're here then we're good enough to save
                                            // Then we have the gameID, the thumbimage, the icon, the name, the exe path
                                            // And we add the Game to the list of games we have!
                                            _allGames.Add(new UplayGame(gameId, gameName, gameExePath, gameIconPath));
                                            logger.Trace($"UplayLibrary/LoadInstalledGames: Adding Uplay Game with game id {productInfo.uplay_id}, name {gameName}, game exe {gameExePath} and icon path {gameIconPath}");
                                            break;
                                        }

                                    }
                                    else
                                    {
                                        logger.Trace($"UplayLibrary/LoadInstalledGames: Uplay Entry {gameName} doesn't have any executables associated with it! We have to skip adding this game.");
                                        continue;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // If we get an error processing the game YAML, lets try and skip this game and try the next one. It might work!
                                    if (item.GameInfo.StartsWith("root:"))
                                    {
                                        logger.Warn($"UplayLibrary/LoadInstalledGames: Problem deserialising the YAML embedded in the Uplay configuration file {uplayConfigFilePath}. Cannot process this Uplay game! (Uplay ID:{item.UplayId}): {item.GameInfo}");
                                        continue;
                                    }
                                    else
                                    {
                                        logger.Trace($"UplayLibrary/LoadInstalledGames: This Uplay entry (Uplay ID:{item.UplayId}) in the Uplay configuration file {uplayConfigFilePath} is not a YAML config so skipping: {item.GameInfo}");
                                    }
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // We can't do anything if we hit here.
                        logger.Error($"UplayLibrary/LoadInstalledGames: Problem deserialising the protobuf Uplay configuration file {uplayConfigFilePath}. Cannot process any Uplay games!");
                        return false;
                    }
                }                   

                logger.Info($"UplayLibrary/LoadInstalledGames: Found {_allGames.Count} installed Uplay games");

            }

            catch (ArgumentNullException ex)
            {
                logger.Warn(ex, "UplayLibrary/GetAllInstalledGames: An argument supplied to the function is null.");
            }
            catch (NotSupportedException ex)
            {
                logger.Warn(ex, "UplayLibrary/GetAllInstalledGames: The invoked method is not supported or reading, seeking or writing tp a stream that isn't supported.");
            }
            catch (PathTooLongException ex)
            {
                logger.Warn(ex, "UplayLibrary/GetAllInstalledGames: The path is longer than the maximum allowed by the operating system.");
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "UplayLibrary/GetAllInstalledGames: The user does not have the permissions required to read the Uplay InstallDir registry key.");
            }
            catch (ObjectDisposedException ex)
            {
                logger.Warn(ex, "UplayLibrary/GetAllInstalledGames: The Microsoft.Win32.RegistryKey is closed when trying to access the Uplay InstallDir registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "UplayLibrary/GetAllInstalledGames: The Uplay InstallDir registry key has been marked for deletion so we cannot access the value dueing the UplayLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "UplayLibrary/GetAllInstalledGames: The user does not have the necessary registry rights to check whether Uplay is installed.");
            }

            return true;
        }

        /*public override Process StartGame(Game game, string gameArguments = "", ProcessPriorityClass processPriority = ProcessPriorityClass.Normal)
        {
            string address = $@"uplay://launch/{game.Id}";
            if (String.IsNullOrWhiteSpace(gameArguments))
            {
                address += @"/" + gameArguments;
            }
            else
            {
                address += "/0";
            }
            Process gameProcess = Process.Start(address);
            gameProcess.PriorityClass = processPriority;
            return gameProcess;

        }*/

        public override List<Process> StartGame(Game game, string gameArguments = "", ProcessPriority processPriority = ProcessPriority.Normal)
        {
            string address = $@"uplay://launch/{game.Id}";
            if (String.IsNullOrWhiteSpace(gameArguments))
            {
                address += @"/" + gameArguments;
            }
            else
            {
                address += "/0";
            }
            List<Process> gameProcesses = ProcessUtils.StartProcess(address, null, processPriority);
            return gameProcesses;
        }

        #endregion

    }

    [global::System.Serializable]
    public class UplayLibraryException : Exception
    {
        public UplayLibraryException() { }
        public UplayLibraryException(string message) : base(message) { }
        public UplayLibraryException(string message, Exception inner) : base(message, inner) { }
        protected UplayLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
