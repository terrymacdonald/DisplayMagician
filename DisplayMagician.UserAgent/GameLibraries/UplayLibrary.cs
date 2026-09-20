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
using DisplayMagician.Processes;

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
        private string uplayAppIdRegex = @"^[0-9A-F]{1,10}$";
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
                // Disabled as we now do it manually when DM starts
                // Load the Uplay Games from Uplay Client if needed
                /*if (_allGames.Count == 0)
                    LoadInstalledGames();*/
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

                try
                {
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
                catch (Exception ex)
                {
                    logger.Warn(ex, $"UplayLibrary/IsRunning: Exception while trying to get the Uplay Library processes with names: {string.Join(", ", _uplayProcessList)}");
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
            // Fix: Validate the string identity accurately against a valid text check
            if (string.IsNullOrWhiteSpace(uplayGameId) || uplayGameId == "0")
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
                    if (uplayGameNameOrId.Equals(testGame.Id))
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
                    if (uplayGameNameOrId.Equals(testGame.Id))
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
                uplayGameInstallKey = Registry.CurrentUser.OpenSubKey(regKeyText, RegistryKeyPermissionCheck.ReadSubTree);
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
                    logger.Info($"UplayLibrary/LoadInstalledGames: Uplay library is not installed");
                    return false;
                }

                logger.Trace($"UplayLibrary/LoadInstalledGames: Uplay Game Installs Registry Key = HKLM\\{registryUplayInstallsKey}");

                using (RegistryKey uplayInstallKey = Registry.LocalMachine.OpenSubKey(registryUplayInstallsKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (uplayInstallKey != null)
                    {
                        int uplayGamesInstalledCount = 0;
                        foreach (string uplayGameKeyName in uplayInstallKey.GetSubKeyNames())
                        {
                            if (int.TryParse(uplayGameKeyName, out int uplayGameId))
                            {
                                string uplayGameKeyFullName = $"{registryUplayInstallsKey}\\{uplayGameKeyName}";
                                using (RegistryKey uplayGameKey = Registry.LocalMachine.OpenSubKey(uplayGameKeyFullName, RegistryKeyPermissionCheck.ReadSubTree))
                                {
                                    if (uplayGameKey == null)
                                        continue;

                                    if (!uplayGameKey.GetValue(@"InstallDir", "").ToString().Equals(""))
                                    {
                                        uplayGamesInstalledCount++;
                                    }
                                }
                            }
                        }

                        if (uplayGamesInstalledCount == 0)
                        {
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
                        logger.Warn($"UplayLibrary/LoadInstalledGames: Couldn't access the Uplay Registry Key {registryUplayInstallsKey}");
                        return false;
                    }
                }

                string uplayConfigFilePath = Path.Combine(_uplayPath, @"cache\configuration\configurations");
                logger.Trace($"UplayLibrary/LoadInstalledGames: Uplay Config File Path = {uplayConfigFilePath}");

                if (!File.Exists(uplayConfigFilePath))
                {
                    logger.Error($"UplayLibrary/LoadInstalledGames: Configuration file not found at {uplayConfigFilePath}. Cannot index games.");
                    return false;
                }

                var deserializer = new DeserializerBuilder()
                    .IgnoreUnmatchedProperties()
                    .Build();

                // Fix: Clean list index beforehand to completely avoid catalog duplication leaks on re-scans!
                _allGames.Clear();

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

                                    string currentLang = CultureInfo.CurrentCulture.Name;                                    
                                    foreach (var lang in productInfo.localizations)
                                    {
                                        if (lang.Key.Equals(currentLang))
                                        {
                                            gameName = lang.Value.l1;
                                            break;
                                        }
                                    }
                                    if (String.IsNullOrEmpty(gameName) && productInfo.localizations.ContainsKey("default"))
                                    {
                                        gameName = productInfo.localizations["default"].l1;
                                    }

                                    if (root?.start_game != null)
                                    {
                                        if (root.start_game.online?.executables != null && root.start_game.online.executables.Count > 0)
                                        {
                                            foreach (var executable in root.start_game.online.executables)
                                            {
                                                string exePath = "";
                                                if (!String.IsNullOrEmpty(executable.path?.relative))
                                                {
                                                    if (executable.working_directory?.register != null && executable.working_directory.register.StartsWith("HKEY_LOCAL_MACHINE"))
                                                    {
                                                        string regKeyText = executable.working_directory.register.Replace(@"\InstallDir", "").Replace(@"Ubisoft", @"WOW6432Node\Ubisoft");
                                                        if (this.GetInstallDirFromRegKey(regKeyText, out exePath))
                                                        {
                                                            gameExePath = Path.Combine(exePath, executable.path.relative);
                                                        }
                                                        string pattern = @"Installs\\(\d+)\\InstallDir";
                                                        MatchCollection mc = Regex.Matches(executable.working_directory.register, pattern);
                                                        if (mc.Count > 0)
                                                        {
                                                            gameId = mc[0].Groups[1].Value;
                                                        }
                                                    }
                                                    else if (!String.IsNullOrEmpty(executable.working_directory?.append))
                                                    {
                                                        gameExePath = Path.Combine(executable.working_directory.append, executable.path.relative);
                                                        gameIconPath = Path.Combine(executable.working_directory.append, executable.icon_image ?? string.Empty);
                                                        gameId = productInfo.uplay_id.ToString();
                                                    }
                                                    else
                                                    {
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    continue;
                                                }

                                                if (!File.Exists(gameExePath))
                                                    continue;

                                                if (!String.IsNullOrEmpty(root.icon_image))
                                                {
                                                    gameIconPath = Path.Combine(_uplayPath, "data", "games", root.icon_image);
                                                    if (!File.Exists(gameIconPath))
                                                    {
                                                        gameIconPath = gameExePath;
                                                    }
                                                }

                                                if (String.IsNullOrEmpty(gameName) && !String.IsNullOrEmpty(executable.shortcut_name))
                                                {
                                                    gameName = executable.shortcut_name;
                                                }

                                                _allGames.Add(new UplayGame(gameId, gameName, gameExePath, gameIconPath));
                                                break;
                                            }
                                        }
                                        else if (root.start_game.offline?.executables != null && root.start_game.offline.executables.Count > 0)
                                        {
                                            foreach (var executable in root.start_game.offline.executables)
                                            {
                                                string exePath = "";
                                                if (!String.IsNullOrEmpty(executable.path?.relative))
                                                {
                                                    if (executable.working_directory?.register != null && executable.working_directory.register.StartsWith("HKEY_LOCAL_MACHINE"))
                                                    {
                                                        string regKeyText = executable.working_directory.register.Replace(@"\InstallDir", "").Replace(@"Ubisoft", @"WOW6432Node\Ubisoft");
                                                        if (this.GetInstallDirFromRegKey(regKeyText, out exePath))
                                                        {
                                                            gameExePath = Path.Combine(exePath, executable.path.relative);
                                                        }
                                                        string pattern = @"Installs\\(\d+)\\InstallDir";
                                                        MatchCollection mc = Regex.Matches(executable.working_directory.register, pattern);
                                                        if (mc.Count > 0)
                                                        {
                                                            gameId = mc[0].Groups[1].Value;
                                                        }
                                                    }
                                                    else if (!String.IsNullOrEmpty(executable.working_directory?.append))
                                                    {
                                                        gameExePath = Path.Combine(executable.working_directory.append, executable.path.relative);
                                                        gameIconPath = Path.Combine(executable.working_directory.append, executable.icon_image ?? string.Empty);
                                                        gameId = productInfo.uplay_id.ToString();
                                                    }
                                                    else
                                                    {
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    continue;
                                                }

                                                if (!File.Exists(gameExePath))
                                                    continue;

                                                if (!String.IsNullOrEmpty(root.icon_image))
                                                {
                                                    gameIconPath = Path.Combine(_uplayPath, "data", "games", root.icon_image);
                                                    if (!File.Exists(gameIconPath))
                                                    {
                                                        gameIconPath = gameExePath;
                                                    }
                                                }

                                                if (String.IsNullOrEmpty(gameName) && !String.IsNullOrEmpty(executable.shortcut_name))
                                                {
                                                    gameName = executable.shortcut_name;
                                                }

                                                _allGames.Add(new UplayGame(gameId, gameName, gameExePath, gameIconPath));
                                                break;
                                            }
                                        }
                                    }                                    
                                }
                                catch (Exception ex)
                                {
                                    if (item.GameInfo.StartsWith("root:"))
                                    {
                                        logger.Warn(ex, $"UplayLibrary/LoadInstalledGames: Error processing parsed game YAML layout data mapping for ID: {item.UplayId}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"UplayLibrary/LoadInstalledGames: Core structural error parsing protobuf map layout within configuration directory: {uplayConfigFilePath}");
                        return false;
                    }
                }                   

                logger.Info($"UplayLibrary/LoadInstalledGames: Found {_allGames.Count} total installed Ubisoft Connect games.");
            }
            // Fix: Catch-all operational exception block provides accurate file context and strips away bloated registry text
            catch (Exception ex)
            {
                logger.Error(ex, "UplayLibrary/LoadInstalledGames: Failure mapping file system directories or evaluating binary database streams.");
                return false;
            }

            return true;
        }

        public override List<Process> StartGame(Game game, string gameArguments = "", ProcessPriority processPriority = ProcessPriority.Normal)
        {
            List<Process> startedProcesses = new List<Process>();
            if (game.Start(out startedProcesses, gameArguments, processPriority))
            {
                logger.Trace($"UplayLibrary/StartGame: Successfully started Uplay game {game.Name}");
            }
            else
            {
                logger.Trace($"UplayLibrary/StartGame: Failed to start Uplay game {game.Name}");
            }
            return startedProcesses;
        }

        public override bool StopGame(Game game)
        {
            if (game.Stop())
            {
                logger.Trace($"UplayLibrary/StopGame: Successfully stopped Uplay game {game.Name}");
                return true;
            }
            else
            {
                logger.Trace($"UplayLibrary/StopGame: Failed to stop Uplay game {game.Name}");
                return false;
            }
        }

        #endregion

    }

    [global::System.Serializable]
    public class UplayLibraryException : Exception
    {
        public UplayLibraryException() { }
        public UplayLibraryException(string message) : base(message) { }
        public UplayLibraryException(string message, Exception inner) : base(message, inner) { }
    }

}
