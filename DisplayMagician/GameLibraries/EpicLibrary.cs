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

namespace DisplayMagician.GameLibraries
{
    public sealed class EpicLibrary : GameLibrary
    {
        #region Class Variables
        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static readonly EpicLibrary _instance = new EpicLibrary();


        // Common items to the class
        private List<Game> _allEpicGames = new List<Game>();
        private string EpicAppIdRegex = @"/^[0-9A-F]{1,10}$";
        private string _epicExe;
        private string _epicPath;
        private string _epicLocalContent = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Epic");
        private bool _isEpicInstalled = false;
        private List<string> _epicProcessList = new List<string>(){ "epic" };

        //private  string _epicConfigVdfFile;
        internal  string registryEpicLauncherKey = @"SOFTWARE\WOW6432Node\Epic";
        //internal  string registryEpicInstallsKey = @"SOFTWARE\WOW6432Node\Ubisoft\Launcher\Installs";
        //internal  string registryEpicOpenCmdKey = @"SOFTWARE\Classes\Epic\Shell\Open\Command";
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        // Other constants that are useful
        #endregion

        #region Class Constructors
        static EpicLibrary() { }

        private EpicLibrary()
        {
            try
            {
                logger.Trace($"EpicLibrary/EpicLibrary: Epic launcher registry key = HKLM\\{registryEpicLauncherKey}");
                // Find the EpicExe location, and the EpicPath for later
                RegistryKey EpicInstallKey = Registry.LocalMachine.OpenSubKey(registryEpicLauncherKey, RegistryKeyPermissionCheck.ReadSubTree);
                if (EpicInstallKey == null)
                    return;
                _epicExe = EpicInstallKey.GetValue("ClientPath", @"C:\Program Files (x86)\Epic\Epic.exe").ToString();
                _epicPath = _epicExe;
                _epicPath = _epicPath.Replace(@"\Epic.exe", "");
                if (File.Exists(_epicExe))
                {
                    logger.Info($"EpicLibrary/EpicLibrary: Epic library is installed in {_epicPath}. Found {_epicExe}");
                    _isEpicInstalled = true;
                }
                else
                {
                    logger.Info($"EpicLibrary/EpicLibrary: Epic library is not installed!");
                }
                   
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "EpicLibrary/EpicLibrary: The user does not have the permissions required to read the Epic ClientPath registry key.");
            }
            catch(ObjectDisposedException ex)
            {
                logger.Warn(ex, "EpicLibrary/EpicLibrary: The Microsoft.Win32.RegistryKey is closed when trying to access the Epic ClientPath registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "EpicLibrary/EpicLibrary: The Epic ClientPath registry key has been marked for deletion so we cannot access the value dueing the EpicLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "EpicLibrary/EpicLibrary: The user does not have the necessary registry rights to check whether Epic is installed.");
            }
        }
        #endregion

        #region Class Properties
        public override List<Game> AllInstalledGames
        {
            get
            {
                // Load the Epic Games from Epic Client if needed
                if (_allEpicGames.Count == 0)
                    LoadInstalledGames();
                return _allEpicGames;
            }
        }


        public override int InstalledGameCount
        {
            get
            {
                return _allEpicGames.Count;
            }
        }

        public override string GameLibraryName 
        { 
            get 
            {
                return "Epic";
            } 
        }

        public override SupportedGameLibraryType GameLibraryType
        {
            get
            {
                return SupportedGameLibraryType.Epic;
            }
        }

        public override string GameLibraryExe
        {
            get
            {
                return _epicExe;
            }
        }

        public override string GameLibraryPath
        {
            get
            {
                return _epicPath;
            }
        }

        public override bool IsGameLibraryInstalled
        {
            get
            {
                return _isEpicInstalled;
            }

        }

        public override bool IsRunning
        {
            get
            {
                List<Process> epicLibraryProcesses = new List<Process>();

                foreach (string epicLibraryProcessName in _epicProcessList)
                {
                    // Look for the processes with the ProcessName we sorted out earlier
                    epicLibraryProcesses.AddRange(Process.GetProcessesByName(epicLibraryProcessName));
                }

                // If we have found one or more processes then we should be good to go
                // so let's break, and get to the next step....
                if (epicLibraryProcesses.Count > 0)
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
                // TODO Implement Epic specific detection for updating the game client
                return false;
            }

        }

        public override List<string> GameLibraryProcesses
        {
            get
            {
                return _epicProcessList;
            }
        }


        #endregion

        #region Class Methods
        public static EpicLibrary GetLibrary()
        {
            return _instance;
        }


        public override bool AddGame(Game epicGame)
        {
            if (!(epicGame is EpicGame))
                return false;
            
            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (ContainsGame(epicGame))
            {
                logger.Debug($"EpicLibrary/AddEpicGame: Updating Epic game {epicGame.Name} in our Epic library");
                // We update the existing Shortcut with the data over
                EpicGame epicGameToUpdate = (EpicGame)GetGame(epicGame.Id.ToString());
                epicGame.CopyTo(epicGameToUpdate);
            }
            else
            {
                logger.Debug($"EpicLibrary/AddEpicGame: Adding Epic game {epicGame.Name} to our Epic library");
                // Add the EpicGame to the list of EpicGames
                _allEpicGames.Add(epicGame);
            }

            //Doublecheck it's been added
            if (ContainsGame(epicGame))
            {
                return true;
            }
            else
                return false;

        }

        public override bool RemoveGame(Game epicGame)
        {
            if (!(epicGame is EpicGame))
                return false;

            logger.Debug($"EpicLibrary/RemoveEpicGame: Removing Epic game {epicGame.Name} from our Epic library");

            // Remove the EpicGame from the list.
            int numRemoved = _allEpicGames.RemoveAll(item => item.Id.Equals(epicGame.Id));

            if (numRemoved == 1)
            {
                logger.Debug($"EpicLibrary/RemoveEpicGame: Removed Epic game with name {epicGame.Name}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"EpicLibrary/RemoveEpicGame: Didn't remove Epic game with ID {epicGame.Name} from the Epic Library");
                return false;
            }                
            else
                throw new EpicLibraryException();
        }

        public override bool RemoveGameById(string epicGameId)
        {
            if (epicGameId.Equals(0))
                return false;

            logger.Debug($"EpicLibrary/RemoveEpicGame2: Removing Epic game with ID {epicGameId} from the Epic library");

            // Remove the EpicGame from the list.
            int numRemoved = _allEpicGames.RemoveAll(item => item.Id.Equals(epicGameId));

            if (numRemoved == 1)
            {
                logger.Debug($"EpicLibrary/RemoveEpicGame2: Removed Epic game with ID {epicGameId}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"EpicLibrary/RemoveEpicGame2: Didn't remove Epic game with ID {epicGameId} from the Epic Library");
                return false;
            }
            else
                throw new EpicLibraryException();
        }

        public override bool RemoveGame(string epicGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(epicGameNameOrId))
                return false;

            logger.Debug($"EpicLibrary/RemoveEpicGame3: Removing Epic game with Name or ID {epicGameNameOrId} from the Epic library");

            int numRemoved;
            Match match = Regex.Match(epicGameNameOrId, EpicAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                numRemoved = _allEpicGames.RemoveAll(item => epicGameNameOrId.Equals(item.Id));
            else
                numRemoved = _allEpicGames.RemoveAll(item => epicGameNameOrId.Equals(item.Name));

            if (numRemoved == 1)
            {
                logger.Debug($"EpicLibrary/RemoveEpicGame3: Removed Epic game with Name or UUID {epicGameNameOrId} ");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"EpicLibrary/RemoveEpicGame3: Didn't remove Epic game with Name or UUID {epicGameNameOrId} from the Epic Library");
                return false;
            }
            else
                throw new EpicLibraryException();

        }

        public override bool ContainsGame(Game epicGame)
        {
            if (!(epicGame is EpicGame))
                return false;

            foreach (EpicGame testEpicGame in _allEpicGames)
            {
                if (testEpicGame.Id.Equals(epicGame.Id))
                    return true;
            }

            return false;
        }

        public override bool ContainsGameById(string epicGameId)
        {
            foreach (EpicGame testEpicGame in _allEpicGames)
            {
                if (epicGameId == testEpicGame.Id)
                    return true;
            }


            return false;

        }

        public override bool ContainsGame(string epicGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(epicGameNameOrId))
                return false;


            Match match = Regex.Match(epicGameNameOrId, EpicAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (EpicGame testEpicGame in _allEpicGames)
                {
                    if (epicGameNameOrId.Equals(Convert.ToInt32(testEpicGame.Id)))
                        return true;
                }

            }
            else
            {
                foreach (EpicGame testEpicGame in _allEpicGames)
                {
                    if (epicGameNameOrId.Equals(testEpicGame.Name))
                        return true;
                }

            }

            return false;

        }


        public override Game GetGame(string epicGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(epicGameNameOrId))
                return null;

            Match match = Regex.Match(epicGameNameOrId, EpicAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (EpicGame testEpicGame in _allEpicGames)
                {
                    if (epicGameNameOrId.Equals(Convert.ToInt32(testEpicGame.Id)))
                        return testEpicGame;
                }

            }
            else
            {
                foreach (EpicGame testEpicGame in _allEpicGames)
                {
                    if (epicGameNameOrId.Equals(testEpicGame.Name))
                        return testEpicGame;
                }

            }

            return null;

        }

        public override Game GetGameById(string epicGameId)
        {
            foreach (EpicGame testEpicGame in _allEpicGames)
            {
                if (epicGameId == testEpicGame.Id)
                    return testEpicGame;
            }

            return null;

        }

        private Dictionary<string, string> ParseEpicManifest(string path)
        {
            string encodedContents = File.ReadAllText(path);
            Dictionary<string, string> parameters = Regex.Matches(encodedContents, "([^?=&]+)(=([^&]*))?").Cast<Match>().ToDictionary(x => x.Groups[1].Value, x => x.Groups[3].Value);
            return parameters;
        }


        public override bool LoadInstalledGames()
        {
            try
            {

                if (!_isEpicInstalled)
                {
                    // Epic isn't installed, so we return an empty list.
                    logger.Info($"EpicLibrary/LoadInstalledGames: Epic library is not installed");
                    return false;
                }

                var localContentPath = Path.Combine(_epicLocalContent, "LocalContent");
                logger.Trace($"EpicLibrary/LoadInstalledGames: Looking for Local Content in {localContentPath}");

                if (Directory.Exists(localContentPath))
                {
                    logger.Trace($"EpicLibrary/LoadInstalledGames: Local Content Directory {localContentPath} exists!");
                    string[] packages = Directory.GetFiles(localContentPath, "*.mfst", SearchOption.AllDirectories);
                    logger.Trace($"EpicLibrary/LoadInstalledGames: Found .mfst files in Local Content Directory {localContentPath}: {packages.ToString()}");
                    foreach (string package in packages)
                    {
                        logger.Trace($"EpicLibrary/LoadInstalledGames: Parsing {package} name to find GameID");
                        try
                        {
                            GameAppInfo epicGame = new GameAppInfo();
                            epicGame.GameID = Path.GetFileNameWithoutExtension(package);
                            logger.Trace($"EpicLibrary/LoadInstalledGames: Got GameID of {epicGame.GameID } from file {package}");
                            if (!epicGame.GameID.StartsWith("Epic"))
                            {
                                // If the gameId doesn't start with epic, then we need to find it!
                                // Get game id by fixing file via adding : before integer part of the name
                                // for example OFB-EAST52017 converts to OFB-EAST:52017
                                Match match = Regex.Match(epicGame.GameID, @"^(.*?)(\d+)$");
                                if (!match.Success)
                                {
                                    logger.Warn($"EpicLibrary/LoadInstalledGames: Failed to match game id from file {package} name so ignoring game");
                                    continue;
                                }

                                epicGame.GameID = match.Groups[1].Value + ":" + match.Groups[2].Value;
                                logger.Trace($"EpicLibrary/LoadInstalledGames: GameID doesn't start with 'Epic' so using different pattern to find {epicGame.GameID} GameID");
                            }

                            // Now we get the rest of the game information out of the manifest file
                            Dictionary<string, string> manifestInfo = ParseEpicManifest(package);

                            logger.Trace($"EpicLibrary/LoadInstalledGames: Looking whether Epic is still downloading the game to install it");
                            if (manifestInfo.ContainsKey("ddinitialdownload") && manifestInfo["ddinitialdownload"] == "1")
                            {
                                // Epic is downloading and installing the game so we skip it
                                logger.Warn($"EpicLibrary/LoadInstalledGames: Epic is still downloading the game with Game ID {epicGame.GameID} to install it");
                                continue;
                            }
                            logger.Trace($"EpicLibrary/LoadInstalledGames: Looking whether Epic is downloading game updates");
                            if (manifestInfo.ContainsKey("downloading") && manifestInfo["downloading"] == "1")
                            {
                                // Epic is downloading some new content so we can't play it at the moment
                                logger.Warn($"EpicLibrary/LoadInstalledGames: Epic is downloading game updates for the game with Game ID {epicGame.GameID}");
                                continue;
                            }

                            epicGame.GameInstallDir = null;
                            logger.Trace($"EpicLibrary/LoadInstalledGames: Looking where the game with Game ID {epicGame.GameID} is installed");
                            if (manifestInfo.ContainsKey("dipinstallpath"))
                            {
                                // This is where Epic has installed this game
                                epicGame.GameInstallDir = HttpUtility.UrlDecode(manifestInfo["dipinstallpath"]);
                                if (String.IsNullOrEmpty(epicGame.GameInstallDir) || !Directory.Exists(epicGame.GameInstallDir))
                                {
                                    logger.Warn($"EpicLibrary/LoadInstalledGames: Epic game with ID {epicGame.GameID} found but no valid directory found at {epicGame.GameInstallDir}");
                                    continue;
                                }
                            }
                            else
                            {
                                logger.Warn($"EpicLibrary/LoadInstalledGames: Couldn't figure out where Game ID {epicGame.GameID} is installed. Skipping game.");
                            }

                            string gameInstallerData = Path.Combine(epicGame.GameInstallDir, @"__Installer", @"installerdata.xml");
                            logger.Trace($"EpicLibrary/LoadInstalledGames: Parsing the Game Installer Data at {gameInstallerData}");

                            if (File.Exists(gameInstallerData))
                            {
                                logger.Trace($"EpicLibrary/LoadInstalledGames: Game Installer Data file was found at {gameInstallerData}");
                                logger.Trace($"EpicLibrary/LoadInstalledGames: Attempting to parse XML Game Installer Data file at {gameInstallerData}");
                                // Now we parse the XML
                                XDocument xdoc = XDocument.Load(gameInstallerData);
                                epicGame.GameName = xdoc.XPathSelectElement("/DiPManifest/gameTitles/gameTitle[@locale='en_US']").Value;
                                logger.Trace($"EpicLibrary/LoadInstalledGames: Game Name {epicGame.GameName} found in Game Installer Data file {gameInstallerData}");
                                string gameFilePath = xdoc.XPathSelectElement("/DiPManifest/runtime/launcher/filePath").Value;
                                logger.Trace($"EpicLibrary/LoadInstalledGames: Game File Path is {gameFilePath } found in Game Installer Data file {gameInstallerData}");

                                string epicGameInstallLocation = "";
                                // Check whether gameFilePath contains a registry key! Cause if it does we need to lookup the path there instead
                                if (gameFilePath.StartsWith("[HKEY_LOCAL_MACHINE"))
                                {
                                    logger.Trace($"EpicLibrary/LoadInstalledGames: Game File Path starts with a registery key so needs to be translated");
                                    // The filePath contains a registry key lookup that we need to execute and replace
                                    string epicGameInstallKeyNameAndValue = "";
                                    string epicGameRestOfFile = "";
                                    MatchCollection mc = Regex.Matches(gameFilePath, @"\[HKEY_LOCAL_MACHINE\\(.*)\](.*)");
                                    if (mc.Count > 0)
                                    {
                                        // Split the Reg key bit from the File Path bit

                                        epicGameInstallKeyNameAndValue = mc[0].Groups[1].ToString();
                                        logger.Trace($"EpicLibrary/LoadInstalledGames: epicGameInstallKeyNameAndValue = {epicGameInstallKeyNameAndValue}");
                                        epicGameRestOfFile = mc[0].Groups[2].ToString();
                                        logger.Trace($"EpicLibrary/LoadInstalledGames: epicGameRestOfFile = {epicGameRestOfFile}");
                                        if (epicGameInstallKeyNameAndValue == null || epicGameInstallKeyNameAndValue == "")
                                        {
                                            // then we have a problem and we need to continue and ignore this game
                                            logger.Warn($"EpicLibrary/LoadInstalledGames: Epic game with ID {epicGame.GameID} has registry key but we can't extract it! gameFilePath is {gameFilePath}.");
                                            continue;
                                        }

                                        // Split the reg key from the value name 

                                        string epicGameInstallKeyName = "";
                                        string epicGameInstallKeyValue = "";
                                        mc = Regex.Matches(epicGameInstallKeyNameAndValue, @"(.*)\\([^\\]*)");
                                        if (mc.Count > 0)
                                        {
                                            epicGameInstallKeyName = mc[0].Groups[1].ToString();
                                            logger.Trace($"EpicLibrary/LoadInstalledGames: epicGameInstallKeyName = {epicGameInstallKeyName }");
                                            epicGameInstallKeyValue = mc[0].Groups[2].ToString();
                                            logger.Trace($"EpicLibrary/LoadInstalledGames: epicGameInstallKeyValue = {epicGameInstallKeyValue }");
                                        }

                                        // Lookup the reg key to figure out where the game is installed 
                                        try
                                        {
                                            RegistryKey epicGameInstallKey = Registry.LocalMachine.OpenSubKey(epicGameInstallKeyName, RegistryKeyPermissionCheck.ReadSubTree);
                                            if (epicGameInstallKey == null)
                                            {
                                                // then we have a problem as we cannot find the game exe location!
                                                logger.Warn($"EpicLibrary/LoadInstalledGames: Epic game with ID {epicGame.GameID} has a install reg key we cannot find! epicGameInstallKey is {gameFilePath} and epicGameInstallKeyValue is {epicGameInstallKeyValue}.");
                                                continue;
                                            }
                                            epicGameInstallLocation = Path.Combine(epicGameInstallKey.GetValue(epicGameInstallKeyValue).ToString(), epicGameRestOfFile);
                                            if (!File.Exists(epicGameInstallLocation))
                                            {
                                                // then we have a problem as we cannot locate the game exe file to start!
                                                logger.Warn($"EpicLibrary/LoadInstalledGames: Epic game with ID {epicGame.GameID} has gameexe we cannot find! epicGameInstallLocation is {epicGameInstallLocation}.");
                                                continue;
                                            }
                                            epicGame.GameExePath = epicGameInstallLocation;
                                        }
                                        catch (SecurityException ex)
                                        {
                                            logger.Warn(ex, $"EpicLibrary/LoadInstalledGames: The user does not have the permissions required to read the Epic Game location registry key {epicGameInstallKeyName}, so skipping game");
                                            continue;
                                        }
                                        catch (ObjectDisposedException ex)
                                        {
                                            logger.Warn(ex, "EpicLibrary/LoadInstalledGames: The Microsoft.Win32.RegistryKey is closed when trying to access the Epic ClientPath registry key (closed keys cannot be accessed), so skipping game");
                                            continue;
                                        }
                                        catch (IOException ex)
                                        {
                                            logger.Warn(ex, "EpicLibrary/LoadInstalledGames: The Epic ClientPath registry key has been marked for deletion so we cannot access the value dueing the EpicLibrary check, so skipping game");
                                            continue;
                                        }
                                        catch (UnauthorizedAccessException ex)
                                        {
                                            logger.Warn(ex, "EpicLibrary/LoadInstalledGames: The user does not have the necessary registry rights to check whether Epic is installed, so skipping game");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        logger.Warn($"EpicLibrary/LoadInstalledGames: Game File Path {gameFilePath} starts with '[HEKY_LOCAL_MACHINE' but didn't match the regex when it should have");
                                        continue;
                                    }
                                    
                                }
                                else if (gameFilePath.StartsWith("[HKEY_CURRENT_USER"))
                                {
                                    // The filePath contains a registry key lookup that we need to execute and replace
                                    MatchCollection mc = Regex.Matches(gameFilePath, @"\[HKEY_CURRENT_USER\\(.*)\](.*)");
                                    if (mc.Count > 0)
                                    {
                                        string epicGameInstallKeyNameAndValue = mc[0].Groups[1].ToString();
                                        string epicGameRestOfFile = mc[0].Groups[2].ToString();
                                        if (epicGameInstallKeyNameAndValue == null)
                                        {
                                            // then we have a problem and we need to continue and ignore this game
                                            logger.Warn($"EpicLibrary/LoadInstalledGames: Epic game with ID {epicGame.GameID} has registry but we can't match it! gameFilePath is {gameFilePath}.");
                                            continue;
                                        }

                                        mc = Regex.Matches(epicGameInstallKeyNameAndValue, @"(.*)\\([^\\]*)");
                                        string epicGameInstallKeyName = mc[0].Groups[1].ToString();
                                        string epicGameInstallKeyValue = mc[0].Groups[2].ToString();

                                        try
                                        {
                                            RegistryKey epicGameInstallKey = Registry.LocalMachine.OpenSubKey(epicGameInstallKeyName, RegistryKeyPermissionCheck.ReadSubTree);
                                            if (epicGameInstallKey == null)
                                            {
                                                // then we have a problem as we cannot find the game exe location!
                                                logger.Warn($"EpicLibrary/LoadInstalledGames: Epic game with ID {epicGame.GameID} has a install reg key we cannot find! epicGameInstallKey is {gameFilePath} and epicGameInstallKeyValue is {epicGameInstallKeyValue}.");
                                                continue;
                                            }
                                            epicGameInstallLocation = Path.Combine(epicGameInstallKey.GetValue(epicGameInstallKeyValue).ToString(), epicGameRestOfFile);
                                            if (!File.Exists(epicGameInstallLocation))
                                            {
                                                // then we have a problem as we cannot locate the game exe file to start!
                                                logger.Warn($"EpicLibrary/LoadInstalledGames: Epic game with ID {epicGame.GameID} has gameexe we cannot find! epicGameInstallLocation is {epicGameInstallLocation}.");
                                                continue;
                                            }
                                            epicGame.GameExePath = epicGameInstallLocation;

                                        }
                                        catch (SecurityException ex)
                                        {
                                            logger.Warn(ex, $"EpicLibrary/LoadInstalledGames: The user does not have the permissions required to read the Epic Game location registry key {epicGameInstallKeyName}, so skipping game");
                                            continue;
                                        }
                                        catch (ObjectDisposedException ex)
                                        {
                                            logger.Warn(ex, "EpicLibrary/LoadInstalledGames: The Microsoft.Win32.RegistryKey is closed when trying to access the Epic ClientPath registry key (closed keys cannot be accessed), so skipping game");
                                            continue;
                                        }
                                        catch (IOException ex)
                                        {
                                            logger.Warn(ex, "EpicLibrary/LoadInstalledGames: The Epic ClientPath registry key has been marked for deletion so we cannot access the value dueing the EpicLibrary check, so skipping game");
                                            continue;
                                        }
                                        catch (UnauthorizedAccessException ex)
                                        {
                                            logger.Warn(ex, "EpicLibrary/LoadInstalledGames: The user does not have the necessary registry rights to check whether Epic is installed, so skipping game");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        logger.Warn($"EpicLibrary/LoadInstalledGames: Game File Path {gameFilePath} starts with '[HKEY_CURRENT_USER' but didn't match the regex when it should have, so skipping game");
                                        continue;
                                    }
                                }
                                else
                                {
                                    // If we get here, then the gameFilepath is the actual filepath! So we just copy it.
                                    logger.Trace($"EpicLibrary/LoadInstalledGames: Game File Path {gameFilePath} doesn't start with '[HKEY_LOCAL_MACHINE' or '[HKEY_CURRENT_USER' so it must be aplain file path");
                                    epicGame.GameExePath = gameFilePath;
                                }


                                if (!File.Exists(epicGame.GameExePath))
                                {
                                    logger.Warn($"EpicLibrary/LoadInstalledGames: Epic game with ID {epicGame.GameID} found but no game exe found at epicGame.GameExePath {epicGame.GameExePath} so skipping game");
                                    continue;
                                }

                                // TODO check for icon! For now we will just use the exe one                               
                                epicGame.GameIconPath = epicGame.GameExePath;
                                logger.Trace($"EpicLibrary/LoadInstalledGames: Epic gameIconPath = {epicGame.GameIconPath} (currently just taking it from the file exe!");

                                // If we reach here we add the Game to the list of games we have!
                                _allEpicGames.Add(new EpicGame(epicGame.GameID, epicGame.GameName, epicGame.GameExePath, epicGame.GameIconPath));
                            }
                            else
                            {
                                // If we can't find the __Installer\installerdata.xml file then we ignore this game
                                logger.Trace($"EpicLibrary/LoadInstalledGames: Couldn't find Game Installer Data file at {gameInstallerData} for game with GameID {epicGame.GameID} so skipping this game");
                                continue;
                            }
                            
                            
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, $"EpicLibrary/LoadInstalledGames: Failed to import installed Epic game {package}.");
                        }
                    }
                }
                else
                {
                    logger.Warn($"EpicLibrary/LoadInstalledGames: No Epic games installed in the Epic library");
                    return false;
                }

                logger.Info($"EpicLibrary/LoadInstalledGames: Found {_allEpicGames.Count} installed Epic games");

            }

            catch (ArgumentNullException ex)
            {
                logger.Warn(ex, "EpicLibrary/GetAllInstalledGames: An argument supplied to the function is null.");
            }
            catch (NotSupportedException ex)
            {
                logger.Warn(ex, "EpicLibrary/GetAllInstalledGames: The invoked method is not supported or reading, seeking or writing tp a stream that isn't supported.");
            }
            catch (PathTooLongException ex)
            {
                logger.Warn(ex, "EpicLibrary/GetAllInstalledGames: The path is longer than the maximum allowed by the operating system.");
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "EpicLibrary/GetAllInstalledGames: The user does not have the permissions required to read the Epic InstallDir registry key.");
            }
            catch (ObjectDisposedException ex)
            {
                logger.Warn(ex, "EpicLibrary/GetAllInstalledGames: The Microsoft.Win32.RegistryKey is closed when trying to access the Epic InstallDir registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "EpicLibrary/GetAllInstalledGames: The Epic InstallDir registry key has been marked for deletion so we cannot access the value dueing the EpicLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "EpicLibrary/GetAllInstalledGames: The user does not have the necessary registry rights to check whether Epic is installed.");
            }

            return true;
        }

        #endregion

    }

    [global::System.Serializable]
    public class EpicLibraryException : GameLibraryException
    {
        public EpicLibraryException() { }
        public EpicLibraryException(string message) : base(message) { }
        public EpicLibraryException(string message, Exception inner) : base(message, inner) { }
        protected EpicLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
