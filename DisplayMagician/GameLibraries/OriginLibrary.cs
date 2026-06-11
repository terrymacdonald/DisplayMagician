using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.IO;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Web;
using System.Diagnostics;
using System.Text;
using DisplayMagician.Processes;

namespace DisplayMagician.GameLibraries
{
    public sealed class OriginLibrary : GameLibrary
    {
        #region Class Variables
        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static readonly OriginLibrary _instance = new OriginLibrary();


        // Common items to the class
        private List<Game> _allOriginGames = new List<Game>();
        private string OriginAppIdRegex = @"^[0-9A-F]{1,10}$";
        private string _originExe;
        private string _originPath;
        private string _originLocalContent = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Origin");
        private bool _isOriginInstalled = false;
        private List<string> _originProcessList = new List<string>(){ "origin" };

        //private  string _originConfigVdfFile;
        internal  string registryOriginLauncherKey = @"SOFTWARE\WOW6432Node\Origin";
        //internal  string registryOriginInstallsKey = @"SOFTWARE\WOW6432Node\Ubisoft\Launcher\Installs";
        //internal  string registryOriginOpenCmdKey = @"SOFTWARE\Classes\Origin\Shell\Open\Command";
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        // Other constants that are useful
        #endregion

        #region Class Constructors
        static OriginLibrary() { }

        private OriginLibrary()
        {
            try
            {
                logger.Trace($"OriginLibrary/OriginLibrary: Origin launcher registry key = HKLM\\{registryOriginLauncherKey}");
                // Find the OriginExe location, and the OriginPath for later
                RegistryKey OriginInstallKey = Registry.LocalMachine.OpenSubKey(registryOriginLauncherKey, RegistryKeyPermissionCheck.ReadSubTree);
                if (OriginInstallKey == null)
                {
                    logger.Info($"OriginLibrary/OriginLibrary: Origin library is not installed!");
                    return;
                }

                _originExe = OriginInstallKey.GetValue("ClientPath", @"C:\Program Files (x86)\Origin\Origin.exe").ToString();
                _originPath = Path.GetDirectoryName(_originExe);
                if (File.Exists(_originExe))
                {
                    logger.Info($"OriginLibrary/OriginLibrary: Origin library is installed in {_originPath}. Found {_originExe}");
                    _isOriginInstalled = true;
                }
                else
                {
                    logger.Info($"OriginLibrary/OriginLibrary: Origin library is not installed!");
                }
                   
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "OriginLibrary/OriginLibrary: The user does not have the permissions required to read the Origin ClientPath registry key.");
            }
            catch(ObjectDisposedException ex)
            {
                logger.Warn(ex, "OriginLibrary/OriginLibrary: The Microsoft.Win32.RegistryKey is closed when trying to access the Origin ClientPath registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "OriginLibrary/OriginLibrary: The Origin ClientPath registry key has been marked for deletion so we cannot access the value dueing the OriginLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "OriginLibrary/OriginLibrary: The user does not have the necessary registry rights to check whether Origin is installed.");
            }
        }
        #endregion

        #region Class Properties
        public override List<Game> AllInstalledGames
        {
            get
            {
                // Disabled as we now do it manually when DM starts
                // Load the Origin Games from Origin Client if needed
                /*if (_allOriginGames.Count == 0)
                    LoadInstalledGames();*/
                return _allOriginGames;
            }
        }


        public override int InstalledGameCount
        {
            get
            {
                return _allOriginGames.Count;
            }
        }

        public override string GameLibraryName 
        { 
            get 
            {
                return "Origin";
            } 
        }

        public override SupportedGameLibraryType GameLibraryType
        {
            get
            {
                return SupportedGameLibraryType.Origin;
            }
        }

        public override string GameLibraryExe
        {
            get
            {
                return _originExe;
            }
        }

        public override string GameLibraryPath
        {
            get
            {
                return _originPath;
            }
        }

        public override bool IsGameLibraryInstalled
        {
            get
            {
                return _isOriginInstalled;
            }

        }

        public override bool IsRunning
        {
            get
            {
                List<Process> originLibraryProcesses = new List<Process>();

                try
                {
                    foreach (string originLibraryProcessName in _originProcessList)
                    {
                        // Look for the processes with the ProcessName we sorted out earlier
                        originLibraryProcesses.AddRange(Process.GetProcessesByName(originLibraryProcessName));
                    }

                    // If we have found one or more processes then we should be good to go
                    // so let's break, and get to the next step....
                    if (originLibraryProcesses.Count > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"OriginLibrary/IsRunning: Exception while trying to get the Origin Library processes with names: {string.Join(", ", _originProcessList)}");
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
                return _originProcessList;
            }
        }


        #endregion

        #region Class Methods
        public static OriginLibrary GetLibrary()
        {
            return _instance;
        }


        public override bool AddGame(Game originGame)
        {
            if (!(originGame is OriginGame))
                return false;
            
            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (ContainsGame(originGame))
            {
                logger.Debug($"OriginLibrary/AddOriginGame: Updating Origin game {originGame.Name} in our Origin library");
                // We update the existing Shortcut with the data over
                OriginGame originGameToUpdate = (OriginGame)GetGame(originGame.Id.ToString());
                originGame.CopyTo(originGameToUpdate);
            }
            else
            {
                logger.Debug($"OriginLibrary/AddOriginGame: Adding Origin game {originGame.Name} to our Origin library");
                // Add the OriginGame to the list of OriginGames
                _allOriginGames.Add(originGame);
            }

            //Doublecheck it's been added
            if (ContainsGame(originGame))
            {
                return true;
            }
            else
                return false;

        }

        public override bool RemoveGame(Game originGame)
        {
            if (!(originGame is OriginGame))
                return false;

            logger.Debug($"OriginLibrary/RemoveOriginGame: Removing Origin game {originGame.Name} from our Origin library");

            // Remove the OriginGame from the list.
            int numRemoved = _allOriginGames.RemoveAll(item => item.Id.Equals(originGame.Id));

            if (numRemoved == 1)
            {
                logger.Debug($"OriginLibrary/RemoveOriginGame: Removed Origin game with name {originGame.Name}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"OriginLibrary/RemoveOriginGame: Didn't remove Origin game with ID {originGame.Name} from the Origin Library");
                return false;
            }                
            else
                throw new OriginLibraryException();
        }

        public override bool RemoveGameById(string originGameId)
        {
            // Fix: Formulate the numeric safety trap check accurately against a valid string identity
            if (string.IsNullOrWhiteSpace(originGameId) || originGameId == "0")
                return false;

            logger.Debug($"OriginLibrary/RemoveOriginGame2: Removing Origin game with ID {originGameId} from the Origin library");

            // Remove the OriginGame from the list.
            int numRemoved = _allOriginGames.RemoveAll(item => item.Id.Equals(originGameId));

            if (numRemoved == 1)
            {
                logger.Debug($"OriginLibrary/RemoveOriginGame2: Removed Origin game with ID {originGameId}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"OriginLibrary/RemoveOriginGame2: Didn't remove Origin game with ID {originGameId} from the Origin Library");
                return false;
            }
            else
                throw new OriginLibraryException();
        }

        public override bool RemoveGame(string originGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(originGameNameOrId))
                return false;

            logger.Debug($"OriginLibrary/RemoveOriginGame3: Removing Origin game with Name or ID {originGameNameOrId} from the Origin library");

            int numRemoved;
            Match match = Regex.Match(originGameNameOrId, OriginAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                numRemoved = _allOriginGames.RemoveAll(item => originGameNameOrId.Equals(item.Id));
            else
                numRemoved = _allOriginGames.RemoveAll(item => originGameNameOrId.Equals(item.Name));

            if (numRemoved == 1)
            {
                logger.Debug($"OriginLibrary/RemoveOriginGame3: Removed Origin game with Name or UUID {originGameNameOrId} ");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"OriginLibrary/RemoveOriginGame3: Didn't remove Origin game with Name or UUID {originGameNameOrId} from the Origin Library");
                return false;
            }
            else
                throw new OriginLibraryException();

        }

        public override bool ContainsGame(Game originGame)
        {
            if (!(originGame is OriginGame))
                return false;

            foreach (OriginGame testOriginGame in _allOriginGames)
            {
                if (testOriginGame.Id.Equals(originGame.Id))
                    return true;
            }

            return false;
        }

        public override bool ContainsGameById(string originGameId)
        {
            foreach (OriginGame testOriginGame in _allOriginGames)
            {
                if (originGameId == testOriginGame.Id)
                    return true;
            }


            return false;

        }

        public override bool ContainsGame(string originGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(originGameNameOrId))
                return false;


            Match match = Regex.Match(originGameNameOrId, OriginAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (OriginGame testOriginGame in _allOriginGames)
                {
                    if (originGameNameOrId.Equals(testOriginGame.Id))
                        return true;
                }

            }
            else
            {
                foreach (OriginGame testOriginGame in _allOriginGames)
                {
                    if (originGameNameOrId.Equals(testOriginGame.Name))
                        return true;
                }

            }

            return false;

        }


        public override Game GetGame(string originGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(originGameNameOrId))
                return null;

            Match match = Regex.Match(originGameNameOrId, OriginAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (OriginGame testOriginGame in _allOriginGames)
                {
                    if (originGameNameOrId.Equals(testOriginGame.Id))
                        return testOriginGame;
                }

            }
            else
            {
                foreach (OriginGame testOriginGame in _allOriginGames)
                {
                    if (originGameNameOrId.Equals(testOriginGame.Name))
                        return testOriginGame;
                }

            }

            return null;

        }

        public override Game GetGameById(string originGameId)
        {
            foreach (OriginGame testOriginGame in _allOriginGames)
            {
                if (originGameId == testOriginGame.Id)
                    return testOriginGame;
            }

            return null;

        }

        private Dictionary<string, string> ParseOriginManifest(string path)
        {
            string encodedContents = File.ReadAllText(path);
            Dictionary<string, string> parameters = Regex.Matches(encodedContents, "([^?=&]+)(=([^&]*))?").Cast<Match>().ToDictionary(x => x.Groups[1].Value, x => x.Groups[3].Value);
            return parameters;
        }


        public override bool LoadInstalledGames()
        {
            try
            {
                if (!_isOriginInstalled)
                {
                    logger.Info($"OriginLibrary/LoadInstalledGames: Origin library is not installed");
                    return false;
                }

                var localContentPath = Path.Combine(_originLocalContent, "LocalContent");
                logger.Trace($"OriginLibrary/LoadInstalledGames: Looking for Local Content in {localContentPath}");

                if (!Directory.Exists(localContentPath))
                {
                    logger.Warn($"OriginLibrary/LoadInstalledGames: No Origin games installed in the Origin library");
                    return false;
                }

                logger.Trace($"OriginLibrary/LoadInstalledGames: Local Content Directory {localContentPath} exists!");
                string[] packages = Directory.GetFiles(localContentPath, "*.mfst", SearchOption.AllDirectories);
                
                // Fix: String join array values to prevent metadata logging printout leaks
                logger.Trace($"OriginLibrary/LoadInstalledGames: Found .mfst files in Local Content: {string.Join(", ", packages)}");

                if (packages.Length == 0)
                {
                    logger.Warn($"OriginLibrary/LoadInstalledGames: No Origin games installed in the Origin library");
                    return false;
                }

                // Fix: Clean list collection index beforehand to completely avoid catalog duplication leaks on re-scans
                _allOriginGames.Clear();

                foreach (string package in packages)
                {
                    logger.Trace($"OriginLibrary/LoadInstalledGames: Parsing {package} name to find GameID");
                    try
                    {
                        GameAppInfo originGame = new GameAppInfo();
                        originGame.GameID = Path.GetFileNameWithoutExtension(package);
                        logger.Trace($"OriginLibrary/LoadInstalledGames: Got GameID of {originGame.GameID} from file {package}");
                        
                        if (!originGame.GameID.StartsWith("Origin"))
                        {
                            Match match = Regex.Match(originGame.GameID, @"^(.*?)(\d+)$");
                            if (!match.Success)
                            {
                                logger.Warn($"OriginLibrary/LoadInstalledGames: Failed to match game id from file {package} name so ignoring game");
                                continue;
                            }

                            originGame.GameID = match.Groups[1].Value + ":" + match.Groups[2].Value;
                            logger.Trace($"OriginLibrary/LoadInstalledGames: GameID doesn't start with 'Origin' so using alternative pattern: {originGame.GameID}");
                        }

                        Dictionary<string, string> manifestInfo = ParseOriginManifest(package);

                        if (manifestInfo.ContainsKey("ddinitialdownload") && manifestInfo["ddinitialdownload"] == "1")
                        {
                            logger.Warn($"OriginLibrary/LoadInstalledGames: Origin is still downloading the game with Game ID {originGame.GameID} to install it");
                            continue;
                        }
                        if (manifestInfo.ContainsKey("downloading") && manifestInfo["downloading"] == "1")
                        {
                            logger.Warn($"OriginLibrary/LoadInstalledGames: Origin is downloading game updates for the game with Game ID {originGame.GameID}");
                            continue;
                        }

                        originGame.GameInstallDir = null;
                        if (manifestInfo.ContainsKey("dipinstallpath"))
                        {
                            originGame.GameInstallDir = HttpUtility.UrlDecode(manifestInfo["dipinstallpath"]);
                            if (String.IsNullOrEmpty(originGame.GameInstallDir) || !Directory.Exists(originGame.GameInstallDir))
                            {
                                logger.Warn($"OriginLibrary/LoadInstalledGames: Origin game with ID {originGame.GameID} found but no valid directory found at {originGame.GameInstallDir}");
                                continue;
                            }
                        }
                        else
                        {
                            logger.Warn($"OriginLibrary/LoadInstalledGames: Couldn't figure out where Game ID {originGame.GameID} is installed. Skipping game.");
                            continue;
                        }

                        string gameInstallerData = Path.Combine(originGame.GameInstallDir, @"__Installer", @"installerdata.xml");
                        logger.Trace($"OriginLibrary/LoadInstalledGames: Parsing the Game Installer Data at {gameInstallerData}");

                        if (File.Exists(gameInstallerData))
                        {
                            XDocument xdoc = XDocument.Load(gameInstallerData);
                            float manifestVersion;
                            
                            if (xdoc.XPathSelectElement("/DiPManifest")?.Attribute("version")?.Value != null)
                            {
                                if (!Single.TryParse(xdoc.XPathSelectElement("/DiPManifest").Attribute("version").Value, out manifestVersion))
                                {
                                    logger.Error($"OriginLibrary/LoadInstalledGames: v4 - Couldn't determine installer.xml manifest version for {gameInstallerData}. Skipping file.");
                                    continue;
                                }
                            }
                            else if (xdoc.XPathSelectElement("/game")?.Attribute("manifestVersion")?.Value != null)
                            {
                                if (!Single.TryParse(xdoc.XPathSelectElement("/game").Attribute("manifestVersion").Value, out manifestVersion))
                                {
                                    logger.Error($"OriginLibrary/LoadInstalledGames: v3 - Couldn't determine installer.xml manifest version for {gameInstallerData}. Skipping file.");
                                    continue;
                                }
                            }
                            else
                            {
                                logger.Error($"OriginLibrary/LoadInstalledGames: Unrecognised installer.xml manifest structure for {gameInstallerData}. Skipping file.");
                                continue;
                            }

                            if (manifestVersion >= 4.0)
                            {                                    
                                originGame.GameName = xdoc.XPathSelectElement("/DiPManifest/gameTitles/gameTitle[@locale='en_US']")?.Value;
                                originGame.GameExePath = GetActualFilePath(xdoc.XPathSelectElement("/DiPManifest/runtime/launcher[requires64BitOS/text() = '1']/filePath")?.Value);
                                if (originGame.GameExePath == null)
                                {
                                    originGame.GameExePath = GetActualFilePath(xdoc.XPathSelectElement("/DiPManifest/runtime/launcher[requires64BitOS/text() = '0']/filePath")?.Value);
                                    if (originGame.GameExePath == null)
                                    {
                                        logger.Error($"OriginLibrary/LoadInstalledGames: Couldn't find 64-bit or 32-bit game exe in version 4 manifest for {originGame.GameName}. Skipping file.");
                                        continue;
                                    }
                                }
                            }
                            else if (manifestVersion >= 3.0 && manifestVersion < 4.0)
                            {
                                originGame.GameName = xdoc.XPathSelectElement("/game/metadata/localeInfo[@locale='en_US']/title")?.Value;
                                originGame.GameExePath = GetActualFilePath(xdoc.XPathSelectElement("/game/runtime/launcher[requires64BitOS/text() = '1']/filePath")?.Value);
                                if (originGame.GameExePath == null)
                                {
                                    originGame.GameExePath = GetActualFilePath(xdoc.XPathSelectElement("/game/runtime/launcher[requires64BitOS/text() = '0']/filePath")?.Value);
                                    if (originGame.GameExePath == null)
                                    {
                                        logger.Error($"OriginLibrary/LoadInstalledGames: Couldn't find 64-bit or 32-bit game exe in version 3 manifest for {originGame.GameName}. Skipping file.");
                                        continue;
                                    }
                                }
                            }
                            else if (manifestVersion >= 2.0 && manifestVersion < 3.0)
                            {
                                originGame.GameName = xdoc.XPathSelectElement("/game/metadata/localeInfo[@locale='en_US']/title")?.Value;
                                string mnsftRelFileName = xdoc.XPathSelectElement("/game/installManifest/filePath")?.Value;
                                string mnsftFullFileName = Path.Combine(originGame.GameInstallDir, mnsftRelFileName ?? string.Empty);
                                
                                string mnsftData;
                                try
                                {
                                    mnsftData = File.ReadAllText(mnsftFullFileName, Encoding.Unicode);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex, $"OriginLibrary/LoadInstalledGames: Tried to read mnfst file {mnsftFullFileName} but file system threw exception. Skipping game.");
                                    continue;
                                }                                    
                                
                                string[] parFiles;
                                try
                                {
                                    parFiles = Directory.GetFiles(originGame.GameInstallDir, "*.par", SearchOption.AllDirectories);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex, $"OriginLibrary/LoadInstalledGames: Tried to scan for *.par parameters in game directory {originGame.GameInstallDir}. Skipping game.");
                                    continue;
                                }

                                if (parFiles.Length == 0)
                                {
                                    MatchCollection mc = Regex.Matches(mnsftData, @"""([^/]*).exe""");
                                    if (mc.Count > 0)
                                    {
                                        originGame.GameExePath = mc[0].Groups[1].ToString();
                                    }
                                }
                                else if (parFiles.Length > 0)
                                {
                                    string parFileBaseName = Path.GetFileNameWithoutExtension(parFiles[0]);
                                    MatchCollection mc = Regex.Matches(mnsftData, $@"""{parFiles[0]}.exe""");
                                    if (mc.Count > 0)
                                    {
                                        originGame.GameExePath = mc[0].Groups[1].ToString();
                                    }
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

                            if (!File.Exists(originGame.GameExePath))
                            {
                                logger.Warn($"OriginLibrary/LoadInstalledGames: Origin game with ID {originGame.GameID} found but no executable file exists at target location. Skipping game.");
                                continue;
                            }

                            originGame.GameIconPath = originGame.GameExePath;

                            _allOriginGames.Add(new OriginGame(originGame.GameID, originGame.GameName, originGame.GameExePath, originGame.GameIconPath));
                        }
                        else
                        {
                            logger.Trace($"OriginLibrary/LoadInstalledGames: Couldn't find structural installer metadata record at {gameInstallerData} for {originGame.GameID}. Skipping game.");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"OriginLibrary/LoadInstalledGames: Failed to import installed Origin package profile: {package}. Continuing scan operations.");
                    }
                }

                logger.Info($"OriginLibrary/LoadInstalledGames: Found {_allOriginGames.Count} fully indexed and verified Origin games.");
            }
            // Fix: Standard catch-all handler accurately contextualizes operations and drops the copy-pasted registry text
            catch (Exception ex)
            {
                logger.Error(ex, "OriginLibrary/LoadInstalledGames: Operational core failure occurred while checking local manifest files or decoding XML installer schemas.");
                return false;
            }

            return true;
        }

        /*public override Process StartGame(Game game, string gameArguments = "", ProcessPriorityClass processPriority = ProcessPriorityClass.Normal)
        {
            string address = $"origin2://game/launch?offerIds={game.Id}";
            if (String.IsNullOrWhiteSpace(gameArguments))
            {
                address += "/" + gameArguments;
            }
            Process gameProcess = Process.Start(address);
            gameProcess.PriorityClass = processPriority;
            return gameProcess;
        }*/
        public override List<Process> StartGame(Game game, string gameArguments = "", ProcessPriority processPriority = ProcessPriority.Normal)
        {
            List<Process> startedProcesses = new List<Process>();
            if (game.Start(out startedProcesses, gameArguments, processPriority))
            {
                logger.Trace($"OriginLibrary/StartGame: Successfully started Origin game {game.Name}");
            }
            else
            {
                logger.Trace($"OriginLibrary/StartGame: Failed to start Origin game {game.Name}");
            }
            return startedProcesses;
        }

        public override bool StopGame(Game game)
        {
            if (game.Stop())
            {
                logger.Trace($"OriginLibrary/StopGame: Successfully stopped Origin game {game.Name}");
                return true;
            }
            else
            {
                logger.Trace($"OriginLibrary/StopGame: Failed to stop Origin game {game.Name}");
                return false;
            }
        }


        private string GetActualFilePath(string gameFilePath)
        {            
            string originGameInstallLocation = "";
            // Check whether gameFilePath contains a registry key! Cause if it does we need to lookup the path there instead
            if (gameFilePath.StartsWith("[HKEY_LOCAL_MACHINE"))
            {
                logger.Trace($"OriginLibrary/GetActualFilePath: Game File Path starts with a registery key so needs to be translated");
                // The filePath contains a registry key lookup that we need to execute and replace
                string originGameInstallKeyNameAndValue = "";
                string originGameRestOfFile = "";
                MatchCollection mc = Regex.Matches(gameFilePath, @"\[HKEY_LOCAL_MACHINE\\(.*)\](.*)");
                if (mc.Count > 0)
                {
                    // Split the Reg key bit from the File Path bit

                    originGameInstallKeyNameAndValue = mc[0].Groups[1].ToString();
                    logger.Trace($"OriginLibrary/GetActualFilePath: originGameInstallKeyNameAndValue = {originGameInstallKeyNameAndValue}");
                    originGameRestOfFile = mc[0].Groups[2].ToString();
                    logger.Trace($"OriginLibrary/GetActualFilePath: originGameRestOfFile = {originGameRestOfFile}");
                    if (originGameInstallKeyNameAndValue == null || originGameInstallKeyNameAndValue == "")
                    {
                        // then we have a problem and we need to continue and ignore this game
                        logger.Warn($"OriginLibrary/GetActualFilePath: Origin game path {gameFilePath} has registry key but we can't extract it!");
                        return null;
                    }

                    // Split the reg key from the value name 

                    string originGameInstallKeyName = "";
                    string originGameInstallKeyValue = "";
                    mc = Regex.Matches(originGameInstallKeyNameAndValue, @"(.*)\\([^\\]*)");
                    if (mc.Count > 0)
                    {
                        originGameInstallKeyName = mc[0].Groups[1].ToString();
                        logger.Trace($"OriginLibrary/GetActualFilePath: originGameInstallKeyName = {originGameInstallKeyName }");
                        originGameInstallKeyValue = mc[0].Groups[2].ToString();
                        logger.Trace($"OriginLibrary/GetActualFilePath: originGameInstallKeyValue = {originGameInstallKeyValue }");
                    }

                    // Lookup the reg key to figure out where the game is installed 
                    try
                    {
                        RegistryKey originGameInstallKey = Registry.LocalMachine.OpenSubKey(originGameInstallKeyName, RegistryKeyPermissionCheck.ReadSubTree);
                        if (originGameInstallKey == null)
                        {
                            // then we have a problem as we cannot find the game exe location!
                            logger.Warn($"OriginLibrary/GetActualFilePath: Origin game path {gameFilePath} has a install reg key we cannot find! originGameInstallKey is {gameFilePath} and originGameInstallKeyValue is {originGameInstallKeyValue}.");
                            return null;
                        }
                        originGameInstallLocation = Path.Combine(originGameInstallKey.GetValue(originGameInstallKeyValue)?.ToString() ?? string.Empty, originGameRestOfFile);
                        if (!File.Exists(originGameInstallLocation))
                        {
                            // then we have a problem as we cannot locate the game exe file to start!
                            logger.Warn($"OriginLibrary/GetActualFilePath: Origin game path {gameFilePath} has gameexe we cannot find! originGameInstallLocation is {originGameInstallLocation}.");
                            return null;
                        }
                        return originGameInstallLocation;
                    }
                    catch (SecurityException ex)
                    {
                        logger.Warn(ex, $"OriginLibrary/GetActualFilePath: The user does not have the permissions required to read the Origin Game location registry key {originGameInstallKeyName}, so skipping game");
                        return null;
                    }
                    catch (ObjectDisposedException ex)
                    {
                        logger.Warn(ex, "OriginLibrary/GetActualFilePath: The Microsoft.Win32.RegistryKey is closed when trying to access the Origin ClientPath registry key (closed keys cannot be accessed), so skipping game");
                        return null;
                    }
                    catch (IOException ex)
                    {
                        logger.Warn(ex, "OriginLibrary/GetActualFilePath: The Origin ClientPath registry key has been marked for deletion so we cannot access the value dueing the OriginLibrary check, so skipping game");
                        return null;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        logger.Warn(ex, "OriginLibrary/GetActualFilePath: The user does not have the necessary registry rights to check whether Origin is installed, so skipping game");
                        return null;
                    }
                }
                else
                {
                    logger.Warn($"OriginLibrary/GetActualFilePath: Game File Path {gameFilePath} starts with '[HEKY_LOCAL_MACHINE' but didn't match the regex when it should have");
                    return null;
                }

            }
            else if (gameFilePath.StartsWith("[HKEY_CURRENT_USER"))
            {
                // The filePath contains a registry key lookup that we need to execute and replace
                MatchCollection mc = Regex.Matches(gameFilePath, @"\[HKEY_CURRENT_USER\\(.*)\](.*)");
                if (mc.Count > 0)
                {
                    string originGameInstallKeyNameAndValue = mc[0].Groups[1].ToString();
                    string originGameRestOfFile = mc[0].Groups[2].ToString();
                    if (originGameInstallKeyNameAndValue == null)
                    {
                        // then we have a problem and we need to continue and ignore this game
                        logger.Warn($"OriginLibrary/GetActualFilePath: Origin game path {gameFilePath} has registry but we can't match it! gameFilePath is {gameFilePath}.");
                        return null;
                    }

                    mc = Regex.Matches(originGameInstallKeyNameAndValue, @"(.*)\\([^\\]*)");
                    string originGameInstallKeyName = mc[0].Groups[1].ToString();
                    string originGameInstallKeyValue = mc[0].Groups[2].ToString();

                    try
                    {
                        RegistryKey originGameInstallKey = Registry.LocalMachine.OpenSubKey(originGameInstallKeyName, RegistryKeyPermissionCheck.ReadSubTree);
                        if (originGameInstallKey == null)
                        {
                            // then we have a problem as we cannot find the game exe location!
                            logger.Warn($"OriginLibrary/GetActualFilePath: Origin game path {gameFilePath} has a install reg key we cannot find! originGameInstallKey is {gameFilePath} and originGameInstallKeyValue is {originGameInstallKeyValue}.");
                            return null;
                        }
                        originGameInstallLocation = Path.Combine(originGameInstallKey.GetValue(originGameInstallKeyValue)?.ToString() ?? string.Empty, originGameRestOfFile);
                        if (!File.Exists(originGameInstallLocation))
                        {
                            // then we have a problem as we cannot locate the game exe file to start!
                            logger.Warn($"OriginLibrary/GetActualFilePath: Origin game path {gameFilePath} has gameexe we cannot find! originGameInstallLocation is {originGameInstallLocation}.");
                            return null;
                        }
                        return originGameInstallLocation;

                    }
                    catch (SecurityException ex)
                    {
                        logger.Warn(ex, $"OriginLibrary/GetActualFilePath: The user does not have the permissions required to read the Origin Game location registry key {originGameInstallKeyName}, so skipping game");
                        return null;
                    }
                    catch (ObjectDisposedException ex)
                    {
                        logger.Warn(ex, "OriginLibrary/GetActualFilePath: The Microsoft.Win32.RegistryKey is closed when trying to access the Origin ClientPath registry key (closed keys cannot be accessed), so skipping game");
                        return null;
                    }
                    catch (IOException ex)
                    {
                        logger.Warn(ex, "OriginLibrary/GetActualFilePath: The Origin ClientPath registry key has been marked for deletion so we cannot access the value dueing the OriginLibrary check, so skipping game");
                        return null;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        logger.Warn(ex, "OriginLibrary/GetActualFilePath: The user does not have the necessary registry rights to check whether Origin is installed, so skipping game");
                        return null;
                    }
                }
                else
                {
                    logger.Warn($"OriginLibrary/GetActualFilePath: Game File Path {gameFilePath} starts with '[HKEY_CURRENT_USER' but didn't match the regex when it should have, so skipping game");
                    return null;
                }
            }
            else
            {
                // If we get here, then the gameFilepath is the actual filepath! So we just copy it.
                logger.Trace($"OriginLibrary/GetActualFilePath: Game File Path {gameFilePath} doesn't start with '[HKEY_LOCAL_MACHINE' or '[HKEY_CURRENT_USER' so it must be aplain file path");
                return gameFilePath;
            }
        }

        #endregion

    }

    [global::System.Serializable]
    public class OriginLibraryException : GameLibraryException
    {
        public OriginLibraryException() { }
        public OriginLibraryException(string message) : base(message) { }
        public OriginLibraryException(string message, Exception inner) : base(message, inner) { }
    }

}
