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
using Newtonsoft.Json;
using DisplayMagician.Processes;

namespace DisplayMagician.GameLibraries
{
    public sealed class XboxLibrary : GameLibrary
    {
        #region Class Variables
        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static readonly XboxLibrary _instance = new XboxLibrary();


        // Common items to the class
        private List<Game> _allXboxGames = new List<Game>();
        private string GogAppIdRegex = @"/^[0-9A-F]{1,10}$";
        private string _xboxExe;
        private string _xboxPath;
        private string _gogLocalContent = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GOG.com");
        private string _gogProgramFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "GOG Galaxy");
        private bool _isXboxInstalled = false;
        private List<string> _xboxProcessList = new List<string>(){ "XboxAppServices" };

        internal string registryGogGalaxyClientKey = @"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient"; 
        internal string registryGogGalaxyClientPathKey = @"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths";
        internal string registryGogGalaxyGamesKey = @"SOFTWARE\WOW6432Node\GOG.com\Games\";     
        //internal string registryGogLauncherKey = @"SOFTWARE\WOW6432Node\Gog";
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        // Other constants that are useful
        #endregion

        #region Class Constructors
        static XboxLibrary() { }

        private XboxLibrary()
        {
            try
            {
                logger.Trace($"XboxLibrary/XboxLibrary: Gog Online Services registry key = HKLM\\{registryGogGalaxyClientKey}");
                // Find the GogExe location, and the GogPath for later
                RegistryKey GogGalaxyClientKey = Registry.LocalMachine.OpenSubKey(registryGogGalaxyClientKey, RegistryKeyPermissionCheck.ReadSubTree);
                if (GogGalaxyClientKey == null)
                {
                    logger.Info($"XboxLibrary/XboxLibrary: GOG library is not installed!");
                    return;
                }
                string gogClientExeFilename = GogGalaxyClientKey.GetValue("clientExecutable", @"GalaxyClient.exe").ToString();

                RegistryKey GogGalaxyClientPathKey = Registry.LocalMachine.OpenSubKey(registryGogGalaxyClientPathKey, RegistryKeyPermissionCheck.ReadSubTree);
                string gogClientPath = GogGalaxyClientKey.GetValue("client", @"C:\Program Files (x86)\GOG Galaxy").ToString();
                _xboxPath = Path.GetDirectoryName(gogClientPath);
                _xboxExe = Path.Combine(gogClientPath, gogClientExeFilename);                
                if (File.Exists(_xboxExe))
                {
                    logger.Info($"XboxLibrary/XboxLibrary: GOG library is installed in {_xboxPath}. Found {_xboxExe}");
                    _isXboxInstalled = true;
                }
                else
                {
                    logger.Info($"XboxLibrary/XboxLibrary: GOG library is not installed!");
                }
                   
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "XboxLibrary/XboxLibrary: The user does not have the permissions required to read the Gog Online Services registry key.");
            }
            catch(ObjectDisposedException ex)
            {
                logger.Warn(ex, "XboxLibrary/XboxLibrary: The Microsoft.Win32.RegistryKey is closed when trying to access the Gog Online Services registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "XboxLibrary/XboxLibrary: The Gog Online Services registry key has been marked for deletion so we cannot access the value dueing the XboxLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "XboxLibrary/XboxLibrary: The user does not have the necessary registry rights to check whether Gog is installed.");
            }
        }
        #endregion

        #region Class Properties
        public override List<Game> AllInstalledGames
        {
            get
            {
                // Load the Gog Games from Gog Client if needed
                if (_allXboxGames.Count == 0)
                    LoadInstalledGames();
                return _allXboxGames;
            }
        }


        public override int InstalledGameCount
        {
            get
            {
                return _allXboxGames.Count;
            }
        }

        public override string GameLibraryName 
        { 
            get 
            {
                return "GOG";
            } 
        }

        public override SupportedGameLibraryType GameLibraryType
        {
            get
            {
                return SupportedGameLibraryType.GOG;
            }
        }

        public override string GameLibraryExe
        {
            get
            {
                return _xboxExe;
            }
        }

        public override string GameLibraryPath
        {
            get
            {
                return _xboxPath;
            }
        }

        public override bool IsGameLibraryInstalled
        {
            get
            {
                return _isXboxInstalled;
            }

        }

        public override bool IsRunning
        {
            get
            {
                List<Process> XboxLibraryProcesses = new List<Process>();

                try
                {
                    foreach (string XboxLibraryProcessName in _xboxProcessList)
                    {
                        // Look for the processes with the ProcessName we sorted out earlier
                        XboxLibraryProcesses.AddRange(Process.GetProcessesByName(XboxLibraryProcessName));
                    }

                    // If we have found one or more processes then we should be good to go
                    // so let's break, and get to the next step....
                    if (XboxLibraryProcesses.Count > 0)
                        return true;
                    else
                        return false;
                }                
                catch (Exception ex) { 
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
                // TODO Implement Gog specific detection for updating the game client
                return false;
            }

        }

        public override List<string> GameLibraryProcesses
        {
            get
            {
                return _xboxProcessList;
            }
        }


        #endregion

        #region Class Methods
        public static XboxLibrary GetLibrary()
        {
            return _instance;
        }


        public override bool AddGame(Game XboxGame)
        {
            if (!(XboxGame is XboxGame))
                return false;
            
            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (ContainsGame(XboxGame))
            {
                logger.Debug($"XboxLibrary/AddXboxGame: Updating Xbox game {XboxGame.Name} in our Xbox library");
                // We update the existing Shortcut with the data over
                XboxGame XboxGameToUpdate = (XboxGame)GetGame(XboxGame.Id.ToString());
                XboxGame.CopyTo(XboxGameToUpdate);
            }
            else
            {
                logger.Debug($"XboxLibrary/AddXboxGame: Adding Xbox game {XboxGame.Name} to our Xbox library");
                // Add the XboxGame to the list of XboxGames
                _allXboxGames.Add(XboxGame);
            }

            //Doublecheck it's been added
            if (ContainsGame(XboxGame))
            {
                return true;
            }
            else
                return false;

        }

        public override bool RemoveGame(Game XboxGame)
        {
            if (!(XboxGame is XboxGame))
                return false;

            logger.Debug($"XboxLibrary/RemoveXboxGame: Removing Xbox game {XboxGame.Name} from our Xbox library");

            // Remove the XboxGame from the list.
            int numRemoved = _allXboxGames.RemoveAll(item => item.Id.Equals(XboxGame.Id));

            if (numRemoved == 1)
            {
                logger.Debug($"XboxLibrary/RemoveXboxGame: Removed Xbox game with name {XboxGame.Name}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"XboxLibrary/RemoveXboxGame: Didn't remove Xbox game with ID {XboxGame.Name} from the Xbox Library");
                return false;
            }                
            else
                throw new XboxLibraryException();
        }

        public override bool RemoveGameById(string XboxGameId)
        {
            if (XboxGameId.Equals(0))
                return false;

            logger.Debug($"XboxLibrary/RemoveXboxGame2: Removing Xbox game with ID {XboxGameId} from the Xbox library");

            // Remove the XboxGame from the list.
            int numRemoved = _allXboxGames.RemoveAll(item => item.Id.Equals(XboxGameId));

            if (numRemoved == 1)
            {
                logger.Debug($"XboxLibrary/RemoveXboxGame2: Removed Xbox game with ID {XboxGameId}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"XboxLibrary/RemoveXboxGame2: Didn't remove Xbox game with ID {XboxGameId} from the Xbox Library");
                return false;
            }
            else
                throw new XboxLibraryException();
        }

        public override bool RemoveGame(string XboxGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(XboxGameNameOrId))
                return false;

            logger.Debug($"XboxLibrary/RemoveXboxGame3: Removing Xbox game with Name or ID {XboxGameNameOrId} from the Xbox library");

            int numRemoved;
            Match match = Regex.Match(XboxGameNameOrId, GogAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                numRemoved = _allXboxGames.RemoveAll(item => XboxGameNameOrId.Equals(item.Id));
            else
                numRemoved = _allXboxGames.RemoveAll(item => XboxGameNameOrId.Equals(item.Name));

            if (numRemoved == 1)
            {
                logger.Debug($"XboxLibrary/RemoveXboxGame3: Removed Xbox game with Name or UUID {XboxGameNameOrId} ");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"XboxLibrary/RemoveXboxGame3: Didn't remove Xbox game with Name or UUID {XboxGameNameOrId} from the Xbox Library");
                return false;
            }
            else
                throw new XboxLibraryException();

        }

        public override bool ContainsGame(Game XboxGame)
        {
            if (!(XboxGame is XboxGame))
                return false;

            foreach (XboxGame testXboxGame in _allXboxGames)
            {
                if (testXboxGame.Id.Equals(XboxGame.Id))
                    return true;
            }

            return false;
        }

        public override bool ContainsGameById(string XboxGameId)
        {
            foreach (XboxGame testXboxGame in _allXboxGames)
            {
                if (XboxGameId == testXboxGame.Id)
                    return true;
            }


            return false;

        }

        public override bool ContainsGame(string XboxGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(XboxGameNameOrId))
                return false;


            Match match = Regex.Match(XboxGameNameOrId, GogAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (XboxGame testXboxGame in _allXboxGames)
                {
                    if (XboxGameNameOrId.Equals(Convert.ToInt32(testXboxGame.Id)))
                        return true;
                }

            }
            else
            {
                foreach (XboxGame testXboxGame in _allXboxGames)
                {
                    if (XboxGameNameOrId.Equals(testXboxGame.Name))
                        return true;
                }

            }

            return false;

        }


        public override Game GetGame(string XboxGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(XboxGameNameOrId))
                return null;

            Match match = Regex.Match(XboxGameNameOrId, GogAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (XboxGame testXboxGame in _allXboxGames)
                {
                    if (XboxGameNameOrId.Equals(Convert.ToInt32(testXboxGame.Id)))
                        return testXboxGame;
                }

            }
            else
            {
                foreach (XboxGame testXboxGame in _allXboxGames)
                {
                    if (XboxGameNameOrId.Equals(testXboxGame.Name))
                        return testXboxGame;
                }

            }

            return null;

        }

        public override Game GetGameById(string XboxGameId)
        {
            foreach (XboxGame testXboxGame in _allXboxGames)
            {
                if (XboxGameId == testXboxGame.Id)
                    return testXboxGame;
            }

            return null;

        }

        public override bool LoadInstalledGames()
        {
            try
            {

                if (!_isXboxInstalled)
                {
                    // Gog isn't installed, so we return an empty list.
                    logger.Info($"XboxLibrary/LoadInstalledGames: Xbox library is not installed");
                    return false;
                }

                string gogSupportInstallerDir = Path.Combine(_gogLocalContent, "supportInstaller");

                logger.Trace($"XboxLibrary/LoadInstalledGames: supportInstaller Directory {gogSupportInstallerDir} exists!");
                string[] gogSupportInstallerGameDirs = Directory.GetDirectories(gogSupportInstallerDir, "*", SearchOption.AllDirectories);
                logger.Trace($"XboxLibrary/LoadInstalledGames: Found game directories in supportInstaller Directory {gogSupportInstallerDir}: {gogSupportInstallerGameDirs.ToString()}");

                // If there are no games installed then return false
                if (gogSupportInstallerGameDirs.Length == 0)
                {
                    logger.Warn($"XboxLibrary/LoadInstalledGames: No GOG games installed in the GOG Galaxy library");
                    return false;
                }
                foreach (string gogSupportInstallerGameDir in gogSupportInstallerGameDirs)
                {
                    logger.Trace($"XboxLibrary/LoadInstalledGames: Parsing {gogSupportInstallerGameDir} name to find GameID");
                    Match match = Regex.Match(gogSupportInstallerGameDir, @"(\d{10})$");
                    if (!match.Success)
                    {
                        logger.Warn($"XboxLibrary/LoadInstalledGames: Failed to match the 10 digit game id from directory name {gogSupportInstallerGameDir} so ignoring game");
                        continue;
                    }

                    string gameID = match.Groups[1].Value;
                    logger.Trace($"XboxLibrary/LoadInstalledGames: Found GameID {gameID} matching pattern in game directory name");
                    string XboxGameInfoFilename = Path.Combine(gogSupportInstallerGameDir, $"XboxGame-{gameID}.info");
                    logger.Trace($"XboxLibrary/LoadInstalledGames: Looking for games info file {XboxGameInfoFilename}");
                    if (!File.Exists(XboxGameInfoFilename))
                    {
                        logger.Warn($"XboxLibrary/LoadInstalledGames: Couldn't find games info file {XboxGameInfoFilename}. There seems to be a problem with your GOG installation.");
                        continue;
                    }

                    // Now we get the information from the Gog Info file to parse it
                    XboxGameInfo XboxGameInfo;
                    try
                    {
                        XboxGameInfo = JsonConvert.DeserializeObject<XboxGameInfo>(File.ReadAllText(XboxGameInfoFilename));
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"XboxLibrary/LoadInstalledGames: Exception trying to convert the {XboxGameInfoFilename} to a JSON object to read the installed games. There seems to be a problem with your GOG installation.");
                        continue;
                    }

                    // Now we check this is a 'Root Game' i.e. it is a  base game, not something else
                    if (XboxGameInfo.gameId != XboxGameInfo.rootGameId)
                    {
                        logger.Trace($"XboxLibrary/LoadInstalledGames: Game {XboxGameInfo.name} is not a base game (probably DLC) so we're skipping it.");
                    }

                    // Now we check the Gog game registry key too, to get some more information that we need
                    string registryGogGalaxyGameKey = registryGogGalaxyGamesKey + XboxGameInfo.gameId;
                    logger.Trace($"XboxLibrary/XboxLibrary: GOG Galaxy Games registry key = HKLM\\{registryGogGalaxyGameKey}");
                    RegistryKey GogGalaxyGameKey = Registry.LocalMachine.OpenSubKey(registryGogGalaxyGameKey, RegistryKeyPermissionCheck.ReadSubTree);
                    if (GogGalaxyGameKey == null)
                    {
                        logger.Info($"XboxLibrary/XboxLibrary: Could not find the GOG Galaxy Games registry key {registryGogGalaxyGamesKey} so can't get all the information about the game we need! There seems to be a problem with your GOG installation.");
                        continue;
                    }

                    string gameDirectory = GogGalaxyGameKey.GetValue("path", "").ToString();
                    string gameExePath = GogGalaxyGameKey.GetValue("exe", "").ToString();
                    if (!File.Exists(gameExePath))
                    {
                        logger.Info($"XboxLibrary/XboxLibrary: Could not find the GOG Galaxy Game file {gameExePath} so can't run the game later! There seems to be a problem with your GOG installation.");
                        continue;
                    }
                    /*string gameIconPath = Path.Combine(gameDirectory, $"XboxGame-{gameID}.ico");                    
                    if (!File.Exists(gameIconPath))
                    {
                        gameIconPath = gameExePath;
                    }*/

                    // Extract the info into a game object                    
                    XboxGame XboxGame = new XboxGame();
                    XboxGame.Id = XboxGameInfo.gameId;
                    XboxGame.Name = XboxGameInfo.name;
                    XboxGame.Directory = gameDirectory;
                    XboxGame.Executable = GogGalaxyGameKey.GetValue("exeFile", "").ToString();
                    XboxGame.ExePath = gameExePath;
                    //XboxGame.IconPath = gameIconPath;
                    XboxGame.IconPath = gameExePath;
                    XboxGame.ProcessName = Path.GetFileNameWithoutExtension(XboxGame.ExePath);

                    // Add the Gog Game to the list of Gog Games
                    _allXboxGames.Add(XboxGame);
                }

                logger.Info($"XboxLibrary/LoadInstalledGames: Found {_allXboxGames.Count} installed GOG games");

            }

            catch (ArgumentNullException ex)
            {
                logger.Warn(ex, "XboxLibrary/GetAllInstalledGames: An argument supplied to the function is null.");
            }
            catch (NotSupportedException ex)
            {
                logger.Warn(ex, "XboxLibrary/GetAllInstalledGames: The invoked method is not supported or reading, seeking or writing to a stream that isn't supported.");
            }
            catch (PathTooLongException ex)
            {
                logger.Warn(ex, "XboxLibrary/GetAllInstalledGames: The path is longer than the maximum allowed by the operating system.");
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "XboxLibrary/GetAllInstalledGames: The user does not have the permissions required to read the GOG InstallDir registry key.");
            }
            catch (ObjectDisposedException ex)
            {
                logger.Warn(ex, "XboxLibrary/GetAllInstalledGames: The Microsoft.Win32.RegistryKey is closed when trying to access the GOG InstallDir registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "XboxLibrary/GetAllInstalledGames: The GOG InstallDir registry key has been marked for deletion so we cannot access the value dueing the XboxLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "XboxLibrary/GetAllInstalledGames: The user does not have the necessary registry rights to check whether Gog is installed.");
            }

            return true;
        }


        public override List<Process> StartGame(Game game, string gameArguments = "", ProcessPriority processPriority = ProcessPriority.Normal)
        {
            string args = $@"/command=runGame /gameId={game.Id} /path=""{game.Directory}""";
            if (String.IsNullOrWhiteSpace(gameArguments))
            {
                args += gameArguments;
            }
            List<Process> gameProcesses = ProcessUtils.StartProcess(_xboxExe, args, processPriority);
            return gameProcesses;
        }

        
        #endregion

    }

    public class XboxPlayTask
    {
        public string category;
        public string compatibilityFlags;
        public bool isPrimary;
        public List<string> languages;
        public string name;
        public string path;
        public string type;
    }
    public class XboxGameInfo
    {
        public string buildId;
        public string clientId;
        public string gameId;
        public string language;
        public List<string> languages;
        public string name;
        public List<XboxPlayTask> playTasks;
        public string rootGameId;
        public int version;
    }

    [global::System.Serializable]
    public class XboxLibraryException : GameLibraryException
    {
        public XboxLibraryException() { }
        public XboxLibraryException(string message) : base(message) { }
        public XboxLibraryException(string message, Exception inner) : base(message, inner) { }
        protected XboxLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
