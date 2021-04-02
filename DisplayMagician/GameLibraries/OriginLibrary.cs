using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
//using ValveKeyValue;
using Microsoft.Win32;
using Microsoft.QueryStringDotNET;
using System.IO;
using System.Security;
using System.Diagnostics;
using System.Net;
using System.Linq;

namespace DisplayMagician.GameLibraries
{
    public static class OriginLibrary
    {
        #region Class Variables
        // Common items to the class
        private static List<Game> _allOriginGames = new List<Game>();
        private static string originAppIdRegex = @"/^[0-9A-F]{1,10}$";
        private static string _originExe;
        private static string _originPath;
        private static string _originLocalContent = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"Origin");
        private static bool _isOriginInstalled = false;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        // Other constants that are useful
        #endregion

        private struct OriginAppInfo
        {
            public int GameID;
            public string GameName;
            public List<string> GameExes;
            public string GameInstallDir;
            public string GameOriginIconPath;
        }

        #region Class Constructors
        static OriginLibrary()
        {
            try
            {
                // Find the OriginExe location, and the OriginPath for later
                using (var key = Registry.CurrentUser.OpenSubKey(OriginLibrary.OriginRegistryKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    _originExe = (string)key?.GetValue(@"OriginExe", string.Empty) ?? string.Empty;
                    _originExe = _originExe.Replace('/', '\\');
                    _originPath = (string)key?.GetValue(@"OriginPath", string.Empty) ?? string.Empty;
                    _originPath = _originPath.Replace('/', '\\');
                }
                if (File.Exists(_originExe))
                    _isOriginInstalled = true;
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex,"The user does not have the permissions required to read the Origin registry key.");
            }
            catch (ObjectDisposedException ex)
            {
                logger.Warn(ex, "The Microsoft.Win32.RegistryKey is closed when trying to access theOrigin registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "The Origin registry key has been marked for deletion so we cannot access the value during the OriginLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "The user does not have the necessary registry rights to check whether Origin is installed.");
            }
        }
        #endregion

        #region Class Properties
        public static List<Game> AllInstalledGames
        {
            get
            {
                // Load the Origin Games from Origin Client if needed
                if (_allOriginGames.Count == 0)
                    LoadInstalledGames();
                return _allOriginGames;
            }
        }


        public static int InstalledOriginGameCount
        {
            get
            {
                return _allOriginGames.Count;
            }
        }

       /* public static string OriginRegistryKey
        {
            get
            {
                return _registryOriginKey;
            }
        }

        public static string OriginAppsRegistryKey
        {
            get
            {
                return _registryAppsKey;
            }
        }
*/
        public static string OriginExe
        {
            get
            {
                return _originExe;
            }
        }

        public static string OriginPath
        {
            get
            {
                return _originPath;
            }
        }

        public static bool IsOriginInstalled
        {
            get
            {
                return _isOriginInstalled;
            }

        }


        #endregion

        #region Class Methods
        public static bool AddOriginGame(OriginGame originGame)
        {
            if (!(originGame is OriginGame))
                return false;

            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (ContainsOriginGame(originGame))
            {
                // We update the existing Shortcut with the data over
                OriginGame originGameToUpdate = GetOriginGame(originGame.Id.ToString());
                originGame.CopyInto(originGameToUpdate);
            }
            else
            {
                // Add the originGame to the list of originGames
                _allOriginGames.Add(originGame);
            }

            //Doublecheck it's been added
            if (ContainsOriginGame(originGame))
            {
                return true;
            }
            else
                return false;

        }

        public static bool RemoveOriginGame(OriginGame originGame)
        {
            if (!(originGame is OriginGame))
                return false;

            // Remove the originGame from the list.
            int numRemoved = _allOriginGames.RemoveAll(item => item.Id.Equals(originGame.Id));

            if (numRemoved == 1)
            {
                return true;
            }
            else if (numRemoved == 0)
                return false;
            else
                throw new OriginLibraryException();
        }

        public static bool RemoveOriginGameById(string originGameId)
        {
            if (originGameId.Equals("0"))
                return false;

            // Remove the originGame from the list.
            int numRemoved = _allOriginGames.RemoveAll(item => item.Id.Equals(originGameId));

            if (numRemoved == 1)
            {
                return true;
            }
            else if (numRemoved == 0)
                return false;
            else
                throw new OriginLibraryException();
        }


        public static bool RemoveOriginGame(string originGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(originGameNameOrId))
                return false;

            int numRemoved;
            Match match = Regex.Match(originGameNameOrId, originAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                numRemoved = _allOriginGames.RemoveAll(item => originGameNameOrId.Equals(item.Id));
            else
                numRemoved = _allOriginGames.RemoveAll(item => originGameNameOrId.Equals(item.Name));

            if (numRemoved == 1)
                return true;
            else if (numRemoved == 0)
                return false;
            else
                throw new OriginLibraryException();

        }

        public static bool ContainsOriginGame(OriginGame originGame)
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

        public static bool ContainsOriginGame(string originGameNameOrUuid)
        {
            if (String.IsNullOrWhiteSpace(originGameNameOrUuid))
                return false;


            Match match = Regex.Match(originGameNameOrUuid, originAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (OriginGame testOriginGame in _allOriginGames)
                {
                    if (originGameNameOrUuid.Equals(Convert.ToInt32(testOriginGame.Id)))
                        return true;
                }

            }
            else
            {
                foreach (OriginGame testOriginGame in _allOriginGames)
                {
                    if (originGameNameOrUuid.Equals(testOriginGame.Name))
                        return true;
                }

            }

            return false;

        }

        public static bool ContainsOriginGameById(string originGameId)
        {
            foreach (OriginGame testOriginGame in _allOriginGames)
            {
                if (originGameId == testOriginGame.Id)
                    return true;
            }

           
            return false;

        }


        public static OriginGame GetOriginGame(string originGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(originGameNameOrId))
                return null;

            Match match = Regex.Match(originGameNameOrId, originAppIdRegex, RegexOptions.IgnoreCase);
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

        public static OriginGame GetOriginGameById(string originGameId)
        {
            foreach (OriginGame testOriginGame in _allOriginGames)
            {
                if (originGameId == testOriginGame.Id)
                    return testOriginGame;
            }

            return null;

        }

        private static Dictionary<string, string> ParseOriginManifest(string path)
        {
            string encodedContents = File.ReadAllText(path);
            Dictionary<string, string> parameters = Regex.Matches(encodedContents, "([^?=&]+)(=([^&]*))?").Cast<Match>().ToDictionary(x => x.Groups[1].Value, x => x.Groups[3].Value);
            return parameters;
        }

        public static bool LoadInstalledGames()
        {
            try
            {

                // Find the OriginExe location, and the OriginPath for later
                /*using (var key = Registry.CurrentUser.OpenSubKey(_registryOriginKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    _originExe = (string)key?.GetValue(@"OriginExe", string.Empty) ?? string.Empty;
                    _originExe = _originExe.Replace('/', '\\');
                    _originPath = (string)key?.GetValue(@"OriginPath", string.Empty) ?? string.Empty;
                    _originPath = _originPath.Replace('/', '\\');
                }*/

                if (!_isOriginInstalled)
                {
                    // Origin isn't installed, so we return an empty list.
                    return false;
                }

                //Icon _originIcon = Icon.ExtractAssociatedIcon(_originExe);
                //IconExtractor originIconExtractor = new IconExtractor(_originExe);
                //Icon _originIcon = originIconExtractor.GetIcon(0);
                //MultiIcon _originIcon = new MultiIcon();
                //_originIcon.Load(_originExe);

                var localContentPath = Path.Combine(_originLocalContent, "LocalContent");
                //var games = new Dictionary<string, GameInfo>();

                if (Directory.Exists(localContentPath))
                {
                    string[] packages = Directory.GetFiles(localContentPath, "*.mfst", SearchOption.AllDirectories);
                    foreach (string package in packages)
                    {
                        try
                        {
                            string gameId = Path.GetFileNameWithoutExtension(package);
                            if (!gameId.StartsWith("Origin"))
                            {
                                // If the gameId doesn't start with origin, then we need to find it!
                                // Get game id by fixing file via adding : before integer part of the name
                                // for example OFB-EAST52017 converts to OFB-EAST:52017
                                Match match = Regex.Match(gameId, @"^(.*?)(\d+)$");
                                if (!match.Success)
                                {
                                    logger.Warn("Failed to get game id from file " + package);
                                    continue;
                                }

                                gameId = match.Groups[1].Value + ":" + match.Groups[2].Value;
                            }

                            // Now we get the rest of the game information out of the manifest file
                            Dictionary<string,string> manifestInfo = ParseOriginManifest(package);

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

                            string gamePath = null;
                            if (manifestInfo.ContainsKey("dipinstallpath"))
                            {
                                // This is where Origin has installed this game                                
                                if (Directory.Exists(manifestInfo["downloading"]))
                                {
                                    gamePath = manifestInfo["downloading"];
                                }
                            }

                            // And we add the Game to the list of games we have!
                            _allOriginGames.Add(new OriginGame(gameId, gameName, gameExe, gameIconPath));



                            var newGame = new GameInfo()
                            {
                                Source = "Origin",
                                GameId = gameId,
                                IsInstalled = true,
                                Platform = "PC"
                            };

                            GameLocalDataResponse localData = null;

                            try
                            {
                                localData = GetLocalManifest(gameId);
                            }
                            catch (Exception e) when (!Environment.IsDebugBuild)
                            {
                                logger.Error(e, $"Failed to get Origin manifest for a {gameId}, {package}");
                                continue;
                            }

                            if (localData == null)
                            {
                                continue;
                            }

                            if (localData.offerType != "Base Game" && localData.offerType != "DEMO")
                            {
                                continue;
                            }

                            newGame.Name = StringExtensions.NormalizeGameName(localData.localizableAttributes.displayName);
                            var installDir = GetInstallDirectory(localData);
                            if (installDir.IsNullOrEmpty())
                            {
                                continue;
                            }

                            newGame.InstallDirectory = installDir;
                            newGame.PlayAction = new GameAction
                            {
                                IsHandledByPlugin = true,
                                Type = GameActionType.URL,
                                Path = Origin.GetLaunchString(gameId)
                            };

                            games.Add(newGame.GameId, newGame);
                        }
                        catch (Exception e) when (!Environment.IsDebugBuild)
                        {
                            logger.Error(e, $"Failed to import installed Origin game {package}.");
                        }
                    }











                    List<int> originAppIdsInstalled = new List<int>();
                // Now look for what games app id's are actually installed on this computer
                using (RegistryKey originAppsKey = Registry.CurrentUser.OpenSubKey(_registryAppsKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (originAppsKey != null)
                    {
                        // Loop through the subKeys as they are the Origin Game IDs
                        foreach (string originGameKeyName in originAppsKey.GetSubKeyNames())
                        {
                            if (int.TryParse(originGameKeyName, out int originAppId))
                            {
                                string originGameKeyFullName = $"{_registryAppsKey}\\{originGameKeyName}";
                                using (RegistryKey originGameKey = Registry.CurrentUser.OpenSubKey(originGameKeyFullName, RegistryKeyPermissionCheck.ReadSubTree))
                                {
                                    // If the Installed Value is set to 1, then the game is installed
                                    // We want to keep track of that for later
                                    if ((int)originGameKey.GetValue(@"Installed", 0) == 1)
                                    {
                                        // Add this Origin App ID to the list we're keeping for later
                                        originAppIdsInstalled.Add(originAppId);
                                    }

                                }

                            }
                        }
                    }
                }

                // Now we parse the origin appinfo.vdf to get access to things like:
                // - The game name
                // - THe game installation dir
                // - Sometimes the game icon
                // - Sometimes the game executable name (from which we can get the icon)
                Dictionary<int, OriginAppInfo> originAppInfo = new Dictionary<int, OriginAppInfo>();

                string appInfoVdfFile = Path.Combine(_originPath, "appcache", "appinfo.vdf");
                var newAppInfo = new AppInfo();
                newAppInfo.Read(appInfoVdfFile);

                Debug.WriteLine($"{newAppInfo.Apps.Count} apps");

                // Chec through all the apps we've extracted
                foreach (var app in newAppInfo.Apps)
                {
                    // We only care about the appIDs we have listed as actual games
                    // (The AppIds include all other DLC and Origin specific stuff too)
                    int detectedAppID = Convert.ToInt32(app.AppID);
                    if (originAppIdsInstalled.Contains(detectedAppID))
                    {

                        try
                        {

                            OriginAppInfo originGameAppInfo = new OriginAppInfo
                            {
                                GameID = detectedAppID,
                                GameExes = new List<string>()
                            };

                            foreach (KVObject data in app.Data)
                            {
                                //Debug.WriteLine($"App: {app.AppID} - Data.Name: {data.Name}");

                                if (data.Name == "common")
                                {
                                    foreach (KVObject common in data.Children)
                                    {

                                        //Debug.WriteLine($"App: {app.AppID} - Common {common.Name}: {common.Value}");

                                        if (common.Name == "name")
                                        {
                                            Debug.WriteLine($"App: {app.AppID} - Common {common.Name}: {common.Value}");
                                            originGameAppInfo.GameName = common.Value.ToString();
                                        }
                                        else if (common.Name == "clienticon")
                                        {
                                            Debug.WriteLine($"App: {app.AppID} - Common {common.Name}: {common.Value}");
                                            originGameAppInfo.GameOriginIconPath = Path.Combine(_originPath, @"origin", @"games", String.Concat(common.Value, @".ico"));
                                        }
                                        else if (common.Name == "type")
                                        {
                                            Debug.WriteLine($"App: {app.AppID} - Common {common.Name}: {common.Value}");
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
                                            Debug.WriteLine($"App: {detectedAppID} - Config {config.Name}: {config.Value}");
                                            originGameAppInfo.GameInstallDir = config.Value.ToString();
                                        }
                                        else if (config.Name == "launch")
                                        {
                                            foreach (KVObject launch in config.Children)
                                            {
                                                foreach (KVObject launch_num in launch.Children)
                                                {
                                                    if (launch_num.Name == "executable")
                                                    {
                                                        Debug.WriteLine($"App: {detectedAppID} - Config - Launch {launch.Name} - {launch_num.Name}: {launch_num.Value}");
                                                        originGameAppInfo.GameExes.Add(launch_num.Value.ToString());
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            originAppInfo.Add(detectedAppID, originGameAppInfo);
                        }
                        catch (ArgumentException ex)
                        {
                            Console.WriteLine($"OriginGame/GetAllInstalledGames exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                            //we just want to ignore it if we try to add it twice....
                        }

                        Debug.WriteLine($"App: {detectedAppID} - Token: {app.Token}");
                    }
                }



                // Now we access the config.vdf that lives in the Origin Config file, as that lists all 
                // the OriginLibraries. We need to find out where they areso we can interrogate them
                _originConfigVdfFile = Path.Combine(_originPath, "config", "config.vdf");
                string originConfigVdfText = File.ReadAllText(_originConfigVdfFile, Encoding.UTF8);

                List<string> originLibrariesPaths = new List<string>();
                // Now we have to parse the config.vdf looking for the location of the OriginLibraries
                // We look for lines similar to this: "BaseInstallFolder_1"		"E:\\OriginLibrary"
                // There may be multiple so we need to check the whole file
                Regex originLibrariesRegex = new Regex(@"""BaseInstallFolder_\d+""\s+""(.*)""", RegexOptions.IgnoreCase);
                // Try to match all lines against the Regex.
                MatchCollection originLibrariesMatches = originLibrariesRegex.Matches(originConfigVdfText);
                // If at least one of them matched!
                foreach (Match originLibraryMatch in originLibrariesMatches)
                {
                    if (originLibraryMatch.Success)
                    { 
                        string originLibraryPath = Regex.Unescape(originLibraryMatch.Groups[1].Value);
                        Debug.WriteLine($"Found origin library: {originLibraryPath}");
                        originLibrariesPaths.Add(originLibraryPath);
                    }
                }
                // Now we go off and find the details for the games in each Origin Library
                foreach (string originLibraryPath in originLibrariesPaths)
                {
                    // Work out the path to the appmanifests for this originLibrary
                    string originLibraryAppManifestPath = Path.Combine(originLibraryPath, @"originapps");
                    // Get the names of the App Manifests for the games installed in this OriginLibrary
                    string[] originLibraryAppManifestFilenames = Directory.GetFiles(originLibraryAppManifestPath, "appmanifest_*.acf");
                    // Go through each app and extract it's details
                    foreach (string originLibraryAppManifestFilename in originLibraryAppManifestFilenames)
                    {
                        // Read in the contents of the file
                        string originLibraryAppManifestText = File.ReadAllText(originLibraryAppManifestFilename);
                        // Grab the appid from the file
                        Regex appidRegex = new Regex(@"""appid""\s+""(\d+)""", RegexOptions.IgnoreCase);
                        Match appidMatches = appidRegex.Match(originLibraryAppManifestText);
                        if (appidMatches.Success)
                        {

                            if (int.TryParse(appidMatches.Groups[1].Value, out int originGameId))
                            {
                                // Check if this game is one that was installed
                                if (originAppInfo.ContainsKey(originGameId))
                                {
                                    // This game is an installed game! so we start to populate it with data!
                                    string originGameExe = "";

                                    string originGameName = originAppInfo[originGameId].GameName;

                                    // Construct the full path to the game dir from the appInfo and libraryAppManifest data
                                    string originGameInstallDir = Path.Combine(originLibraryPath, @"originapps", @"common", originAppInfo[originGameId].GameInstallDir);

                                    // And finally we try to populate the 'where', to see what gets run
                                    // And so we can extract the process name
                                    if (originAppInfo[originGameId].GameExes.Count > 0)
                                    {
                                        foreach (string gameExe in originAppInfo[originGameId].GameExes)
                                        {
                                            originGameExe = Path.Combine(originGameInstallDir, gameExe);
                                            // If the game executable exists, then we can proceed
                                            if (File.Exists(originGameExe))
                                            {
                                                break;
                                            }
                                        }

                                    }

                                    // Next, we need to get the Icons we want to use, and make sure it's the latest one.
                                    string originGameIconPath = "";
                                    // First of all, we attempt to use the Icon that Origin has cached, if it's available, as that will be updated to the latest
                                    if (File.Exists(originAppInfo[originGameId].GameOriginIconPath) && originAppInfo[originGameId].GameOriginIconPath.EndsWith(".ico"))
                                    {
                                        originGameIconPath = originAppInfo[originGameId].GameOriginIconPath;
                                    }
                                    // If there isn't an icon for us to use, then we need to extract one from the Game Executables
                                    else if (!String.IsNullOrEmpty(originGameExe))
                                    {
                                        originGameIconPath = originGameExe;
                                    }
                                    // The absolute worst case means we don't have an icon to use. SO we use the Origin one.
                                    else
                                    {
                                        // And we have to make do with a Origin Icon
                                        originGameIconPath = _originPath;
                                    }

                                    // And we add the Game to the list of games we have!
                                    _allOriginGames.Add(new OriginGame(originGameId, originGameName, originGameExe, originGameIconPath));

                                }
                            }
                        }
                    }
                }
            }
            catch (SecurityException ex)
            {
                Console.WriteLine($"OriginGame/GetAllInstalledGames securityexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                if (ex.Source != null)
                    Console.WriteLine("SecurityException source: {0} - Message: {1}", ex.Source, ex.Message);
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"OriginGame/GetAllInstalledGames unauthorizedaccessexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                if (ex.Source != null)
                    Console.WriteLine("UnauthorizedAccessException  source: {0} - Message: {1}", ex.Source, ex.Message);
                throw;
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"OriginGame/GetAllInstalledGames objectdisposedexceptions: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                if (ex.Source != null)
                    Console.WriteLine("ObjectDisposedException  source: {0} - Message: {1}", ex.Source, ex.Message);
                throw;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"OriginGame/GetAllInstalledGames ioexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // Extract some information from this exception, and then
                // throw it to the parent method.
                if (ex.Source != null)
                    Console.WriteLine("IOException source: {0} - Message: {1}", ex.Source, ex.Message);
                throw;
            }

            return true;
        }

        #endregion

    }

    [global::System.Serializable]
    public class OriginLibraryException : Exception
    {
        public OriginLibraryException() { }
        public OriginLibraryException(string message) : base(message) { }
        public OriginLibraryException(string message, Exception inner) : base(message, inner) { }
        protected OriginLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
