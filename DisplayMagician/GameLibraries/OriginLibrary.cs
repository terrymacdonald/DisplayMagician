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
        private string OriginAppIdRegex = @"/^[0-9A-F]{1,10}$";
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
        private OriginLibrary()
        {
            try
            {
                logger.Trace($"OriginLibrary/OriginLibrary: Origin launcher registry key = HKLM\\{registryOriginLauncherKey}");
                // Find the OriginExe location, and the OriginPath for later
                RegistryKey OriginInstallKey = Registry.LocalMachine.OpenSubKey(registryOriginLauncherKey, RegistryKeyPermissionCheck.ReadSubTree);
                if (OriginInstallKey == null)
                    return;
                _originExe = OriginInstallKey.GetValue("ClientPath", @"C:\Program Files (x86)\Origin\Origin.exe").ToString();
                _originPath = _originExe;
                _originPath = _originPath.Replace(@"\Origin.exe", "");
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
                // Load the Origin Games from Origin Client if needed
                if (_allOriginGames.Count == 0)
                    LoadInstalledGames();
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


        public  bool AddGame(OriginGame originGame)
        {
            if (!(originGame is OriginGame))
                return false;
            
            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (ContainsGame(originGame))
            {
                logger.Debug($"OriginLibrary/AddOriginGame: Updating Origin game {originGame.Name} in our Origin library");
                // We update the existing Shortcut with the data over
                OriginGame originGameToUpdate = GetGame(originGame.Id.ToString());
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

        public bool RemoveGame(OriginGame originGame)
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
            if (originGameId.Equals(0))
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

        public bool ContainsGame(OriginGame originGame)
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
                    if (originGameNameOrId.Equals(Convert.ToInt32(testOriginGame.Id)))
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


        public new OriginGame GetGame(string originGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(originGameNameOrId))
                return null;

            Match match = Regex.Match(originGameNameOrId, OriginAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (OriginGame testOriginGame in _allOriginGames)
                {
                    if (originGameNameOrId.Equals(Convert.ToInt32(testOriginGame.Id)))
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

        public new OriginGame GetGameById(string originGameId)
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
                    // Origin isn't installed, so we return an empty list.
                    logger.Info($"OriginLibrary/LoadInstalledGames: Origin library is not installed");
                    return false;
                }

                var localContentPath = Path.Combine(_originLocalContent, "LocalContent");
                //var games = new Dictionary<string, GameInfo>();

                if (Directory.Exists(localContentPath))
                {
                    string[] packages = Directory.GetFiles(localContentPath, "*.mfst", SearchOption.AllDirectories);
                    foreach (string package in packages)
                    {
                        try
                        {
                            GameAppInfo originGame = new GameAppInfo();
                            originGame.GameID = Path.GetFileNameWithoutExtension(package);
                            if (!originGame.GameID.StartsWith("Origin"))
                            {
                                // If the gameId doesn't start with origin, then we need to find it!
                                // Get game id by fixing file via adding : before integer part of the name
                                // for example OFB-EAST52017 converts to OFB-EAST:52017
                                Match match = Regex.Match(originGame.GameID, @"^(.*?)(\d+)$");
                                if (!match.Success)
                                {
                                    logger.Warn("Failed to get game id from file " + package);
                                    continue;
                                }

                                originGame.GameID = match.Groups[1].Value + ":" + match.Groups[2].Value;
                            }

                            // Now we get the rest of the game information out of the manifest file
                            Dictionary<string, string> manifestInfo = ParseOriginManifest(package);

                            if (manifestInfo.ContainsKey("ddinitialdownload") && manifestInfo["ddinitialdownload"] == "1")
                            {
                                // Origin is downloading and installing the game so we skip it
                                continue;
                            }
                            if (manifestInfo.ContainsKey("downloading") && manifestInfo["downloading"] == "1")
                            {
                                // Origin is downloading some new content so we can't play it at the moment
                                // but we can still configure it
                                continue;
                            }

                            originGame.GameInstallDir = null;
                            if (manifestInfo.ContainsKey("dipinstallpath"))
                            {
                                // This is where Origin has installed this game
                                originGame.GameInstallDir = HttpUtility.UrlDecode(manifestInfo["dipinstallpath"]);
                                if (!Directory.Exists(originGame.GameInstallDir))
                                {
                                    logger.Debug($"OriginLibrary/LoadInstalledGames: Origin game with ID {originGame.GameID} found but no valid directory found at {originGame.GameInstallDir}");
                                    continue;
                                }
                            }


                            // Now we want to look in the dinstallpath location for the game info
                            // for the __Installer\installerdata.xml

                            string gameInstallerData = Path.Combine(originGame.GameInstallDir, @"__Installer", @"installerdata.xml");

                            if (File.Exists(gameInstallerData))
                            {
                                // Now we parse the XML
                                XDocument xdoc = XDocument.Load(gameInstallerData);
                                originGame.GameName = xdoc.XPathSelectElement("/DiPManifest/gameTitles/gameTitle[@locale='en_US']").Value;
                                string gameFilePath = xdoc.XPathSelectElement("/DiPManifest/runtime/launcher/filePath").Value;
                                // Check whether gameFilePath contains a registry key! Cause if it does we need to lookup the path there instead
                                if (gameFilePath.StartsWith("[HKEY_LOCAL_MACHINE"))
                                {
                                    // The filePath contains a registry key lookup that we need to execute and replace
                                    MatchCollection mc = Regex.Matches(gameFilePath, @"\[HKEY_LOCAL_MACHINE\\(.*)\](.*)");
                                    string originGameInstallKeyNameAndValue = mc[0].Groups[1].ToString();
                                    string originGameRestOfFile = mc[0].Groups[2].ToString();
                                    if (originGameInstallKeyNameAndValue == null)
                                    {
                                        // then we have a problem and we need to continue and ignore this game
                                        logger.Warn($"OriginLibrary/LoadInstalledGames: Origin game with ID {originGame.GameID} has registry but we can't match it! gameFilePath is {gameFilePath}.");
                                        continue;
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
                                            logger.Warn($"OriginLibrary/LoadInstalledGames: Origin game with ID {originGame.GameID} has a install reg key we cannot find! originGameInstallKey is {gameFilePath} and originGameInstallKeyValue is {originGameInstallKeyValue}.");
                                            continue;
                                        }
                                        string originGameInstallLocation = originGameInstallKey.GetValue(originGameInstallKeyValue).ToString();
                                        originGameInstallLocation = Path.Combine(originGameInstallLocation, originGameRestOfFile);
                                        if (!File.Exists(originGameInstallLocation))
                                        {
                                            // then we have a problem as we cannot locate the game exe file to start!
                                            logger.Warn($"OriginLibrary/LoadInstalledGames: Origin game with ID {originGame.GameID} has gameexe we cannot find! originGameInstallLocation is {originGameInstallLocation}.");
                                            continue;
                                        }
                                        originGame.GameExePath = originGameInstallLocation;

                                    }
                                    catch (SecurityException ex)
                                    {
                                        logger.Warn(ex, "OriginLibrary/LoadInstalledGames: The user does not have the permissions required to read the Origin Game location registry key {}.");
                                    }
                                    catch (ObjectDisposedException ex)
                                    {
                                        logger.Warn(ex, "OriginLibrary/LoadInstalledGames: The Microsoft.Win32.RegistryKey is closed when trying to access the Origin ClientPath registry key (closed keys cannot be accessed).");
                                    }
                                    catch (IOException ex)
                                    {
                                        logger.Warn(ex, "OriginLibrary/LoadInstalledGames: The Origin ClientPath registry key has been marked for deletion so we cannot access the value dueing the OriginLibrary check.");
                                    }
                                    catch (UnauthorizedAccessException ex)
                                    {
                                        logger.Warn(ex, "OriginLibrary/LoadInstalledGames: The user does not have the necessary registry rights to check whether Origin is installed.");
                                    }
                                }
                                else if (gameFilePath.StartsWith("[HKEY_CURRENT_USER"))
                                {
                                    // The filePath contains a registry key lookup that we need to execute and replace
                                    MatchCollection mc = Regex.Matches(gameFilePath, @"\[HKEY_CURRENT_USER\\(.*)\](.*)");
                                    string originGameInstallKeyNameAndValue = mc[0].Groups[1].ToString();
                                    string originGameRestOfFile = mc[0].Groups[2].ToString();
                                    if (originGameInstallKeyNameAndValue == null)
                                    {
                                        // then we have a problem and we need to continue and ignore this game
                                        logger.Warn($"OriginLibrary/LoadInstalledGames: Origin game with ID {originGame.GameID} has registry but we can't match it! gameFilePath is {gameFilePath}.");
                                        continue;
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
                                            logger.Warn($"OriginLibrary/LoadInstalledGames: Origin game with ID {originGame.GameID} has a install reg key we cannot find! originGameInstallKey is {gameFilePath} and originGameInstallKeyValue is {originGameInstallKeyValue}.");
                                            continue;
                                        }
                                        string originGameInstallLocation = originGameInstallKey.GetValue(originGameInstallKeyValue).ToString();
                                        originGameInstallLocation = Path.Combine(originGameInstallLocation, originGameRestOfFile);
                                        if (!File.Exists(originGameInstallLocation))
                                        {
                                            // then we have a problem as we cannot locate the game exe file to start!
                                            logger.Warn($"OriginLibrary/LoadInstalledGames: Origin game with ID {originGame.GameID} has gameexe we cannot find! originGameInstallLocation is {originGameInstallLocation}.");
                                            continue;
                                        }
                                        originGame.GameExePath = originGameInstallLocation;

                                    }
                                    catch (SecurityException ex)
                                    {
                                        logger.Warn(ex, "OriginLibrary/LoadInstalledGames: The user does not have the permissions required to read the Origin Game location registry key {}.");
                                    }
                                    catch (ObjectDisposedException ex)
                                    {
                                        logger.Warn(ex, "OriginLibrary/LoadInstalledGames: The Microsoft.Win32.RegistryKey is closed when trying to access the Origin ClientPath registry key (closed keys cannot be accessed).");
                                    }
                                    catch (IOException ex)
                                    {
                                        logger.Warn(ex, "OriginLibrary/LoadInstalledGames: The Origin ClientPath registry key has been marked for deletion so we cannot access the value dueing the OriginLibrary check.");
                                    }
                                    catch (UnauthorizedAccessException ex)
                                    {
                                        logger.Warn(ex, "OriginLibrary/LoadInstalledGames: The user does not have the necessary registry rights to check whether Origin is installed.");
                                    }
                                }


                                if (!File.Exists(originGame.GameExePath))
                                {
                                    logger.Debug($"OriginLibrary/LoadInstalledGames: Origin game with ID {originGame.GameID} found but no game exe found at {originGame.GameExePath}");
                                    continue;
                                }

                                // TODO check for icon! For now we will just use the exe one
                                originGame.GameIconPath = originGame.GameExePath;

                                // If we reach here we add the Game to the list of games we have!
                                _allOriginGames.Add(new OriginGame(originGame.GameID, originGame.GameName, originGame.GameExePath, originGame.GameIconPath));

                            }
                            else
                            {
                                // If we can't find the __Installer\installerdata.xml file then we ignore this game
                                continue;
                            }
                            
                            
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, $"OriginLibrary/LoadInstalledGames: Failed to import installed Origin game {package}.");
                        }
                    }
                }
                else
                {
                    logger.Warn($"OriginLibrary/LoadInstalledGames: No Origin games installed in the Origin library");
                    return false;
                }

                // up to here

                

                /*    if (gotGameRegistryKey)
                    {
                        // Now we need to lookup the game install path in registry using the game reg we got above
                        // We assume its 64-bit OS too (not 32bit)
                        using (RegistryKey OriginGameInstallKey = Registry.LocalMachine.OpenSubKey(gameRegistryKey, RegistryKeyPermissionCheck.ReadSubTree))
                        {
                            // If the key doesn't exist we skip it as the game isn't installed any longer!
                            if (OriginGameInstallKey == null)
                            {
                                logger.Trace($"OriginLibrary/LoadInstalledGames: Skipping Origin Game {OriginGameAppInfo.GameName} as it isn't installed at the moment (it was uninstalled at some point)");
                                continue;
                            }

                            // If we get here, then we have a real game.
                            foreach (string regKeyName in OriginGameInstallKey.GetValueNames())
                            {
                                logger.Trace($"OriginLibrary/LoadInstalledGames: OriginGameInstallKey[{regKeyName}] = {OriginGameInstallKey.GetValue(regKeyName)}");
                            }

                            // From that we lookup the actual game path
                            string gameInstallDir = OriginGameInstallKey.GetValue("InstallDir", "").ToString();
                            logger.Trace($"OriginLibrary/LoadInstalledGames: gameInstallDir found  = {gameInstallDir}");
                            if (!String.IsNullOrWhiteSpace(gameInstallDir))
                            {
                                OriginGameAppInfo.GameInstallDir = Path.GetFullPath(gameInstallDir).TrimEnd('\\');
                                logger.Trace($"OriginLibrary/LoadInstalledGames: OriginGameAppInfo.GameInstallDir = {OriginGameAppInfo.GameInstallDir }");
                                OriginGameAppInfo.GameExe = Path.Combine(OriginGameAppInfo.GameInstallDir, gameFileName);
                                logger.Trace($"OriginLibrary/LoadInstalledGames: OriginGameAppInfo.GameExe = {OriginGameAppInfo.GameExePath }");
                                OriginGameAppInfo.GameID = gameId;
                                logger.Trace($"OriginLibrary/LoadInstalledGames: OriginGameAppInfo.GameID = {OriginGameAppInfo.GameID }");
                            }
                            else
                            {
                                logger.Warn($"OriginLibrary/LoadInstalledGames: gameInstallDir is null or all whitespace!");
                            }

                            // Then we have the gameID, the thumbimage, the icon, the name, the exe path
                            // And we add the Game to the list of games we have!
                            _allOriginGames.Add(new OriginGame(OriginGameAppInfo.GameID, OriginGameAppInfo.GameName, OriginGameAppInfo.GameExePath, OriginGameAppInfo.GameOriginIconPath));
                            logger.Debug($"OriginLibrary/LoadInstalledGames: Adding Origin Game with game id {OriginGameAppInfo.GameID}, name {OriginGameAppInfo.GameName}, game exe {OriginGameAppInfo.GameExePath} and icon path {OriginGameAppInfo.GameOriginIconPath}");
                        }
                    }
                    
                }*/

                logger.Info($"OriginLibrary/LoadInstalledGames: Found {_allOriginGames.Count} installed Origin games");

            }

            catch (ArgumentNullException ex)
            {
                logger.Warn(ex, "OriginLibrary/GetAllInstalledGames: An argument supplied to the function is null.");
            }
            catch (NotSupportedException ex)
            {
                logger.Warn(ex, "OriginLibrary/GetAllInstalledGames: The invoked method is not supported or reading, seeking or writing tp a stream that isn't supported.");
            }
            catch (PathTooLongException ex)
            {
                logger.Warn(ex, "OriginLibrary/GetAllInstalledGames: The path is longer than the maximum allowed by the operating system.");
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "OriginLibrary/GetAllInstalledGames: The user does not have the permissions required to read the Origin InstallDir registry key.");
            }
            catch (ObjectDisposedException ex)
            {
                logger.Warn(ex, "OriginLibrary/GetAllInstalledGames: The Microsoft.Win32.RegistryKey is closed when trying to access the Origin InstallDir registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "OriginLibrary/GetAllInstalledGames: The Origin InstallDir registry key has been marked for deletion so we cannot access the value dueing the OriginLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "OriginLibrary/GetAllInstalledGames: The user does not have the necessary registry rights to check whether Origin is installed.");
            }

            return true;
        }

        #endregion

    }

    [global::System.Serializable]
    public class OriginLibraryException : GameLibraryException
    {
        public OriginLibraryException() { }
        public OriginLibraryException(string message) : base(message) { }
        public OriginLibraryException(string message, Exception inner) : base(message, inner) { }
        protected OriginLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
