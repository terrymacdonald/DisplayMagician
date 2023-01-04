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
                // Disabled as we now do it manually when DM starts
                // Load the Xbox Games from Xbox Client if needed
                /*if (_allXboxGames.Count == 0)
                    LoadInstalledGames();*/
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


        public override bool AddGame(Game xboxGame)
        {
            if (!(xboxGame is XboxGame))
                return false;
            
            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (ContainsGame(xboxGame))
            {
                logger.Debug($"XboxLibrary/AddXboxGame: Updating Xbox game {xboxGame.Name} in our Xbox library");
                // We update the existing Shortcut with the data over
                XboxGame XboxGameToUpdate = (XboxGame)GetGame(xboxGame.Id.ToString());
                xboxGame.CopyTo(XboxGameToUpdate);
            }
            else
            {
                logger.Debug($"XboxLibrary/AddXboxGame: Adding Xbox game {xboxGame.Name} to our Xbox library");
                // Add the XboxGame to the list of XboxGames
                _allXboxGames.Add(xboxGame);
            }

            //Doublecheck it's been added
            if (ContainsGame(xboxGame))
            {
                return true;
            }
            else
                return false;

        }

        public override bool RemoveGame(Game xboxGame)
        {
            if (!(xboxGame is XboxGame))
                return false;

            logger.Debug($"XboxLibrary/RemoveXboxGame: Removing Xbox game {xboxGame.Name} from our Xbox library");

            // Remove the XboxGame from the list.
            int numRemoved = _allXboxGames.RemoveAll(item => item.Id.Equals(xboxGame.Id));

            if (numRemoved == 1)
            {
                logger.Debug($"XboxLibrary/RemoveXboxGame: Removed Xbox game with name {xboxGame.Name}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"XboxLibrary/RemoveXboxGame: Didn't remove Xbox game with ID {xboxGame.Name} from the Xbox Library");
                return false;
            }                
            else
                throw new XboxLibraryException();
        }

        public override bool RemoveGameById(string xboxGameId)
        {
            if (xboxGameId.Equals(0))
                return false;

            logger.Debug($"XboxLibrary/RemoveXboxGame2: Removing Xbox game with ID {xboxGameId} from the Xbox library");

            // Remove the XboxGame from the list.
            int numRemoved = _allXboxGames.RemoveAll(item => item.Id.Equals(xboxGameId));

            if (numRemoved == 1)
            {
                logger.Debug($"XboxLibrary/RemoveXboxGame2: Removed Xbox game with ID {xboxGameId}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"XboxLibrary/RemoveXboxGame2: Didn't remove Xbox game with ID {xboxGameId} from the Xbox Library");
                return false;
            }
            else
                throw new XboxLibraryException();
        }

        public override bool RemoveGame(string xboxGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(xboxGameNameOrId))
                return false;

            logger.Debug($"XboxLibrary/RemoveXboxGame3: Removing Xbox game with Name or ID {xboxGameNameOrId} from the Xbox library");

            int numRemoved;
            Match match = Regex.Match(xboxGameNameOrId, GogAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                numRemoved = _allXboxGames.RemoveAll(item => xboxGameNameOrId.Equals(item.Id));
            else
                numRemoved = _allXboxGames.RemoveAll(item => xboxGameNameOrId.Equals(item.Name));

            if (numRemoved == 1)
            {
                logger.Debug($"XboxLibrary/RemoveXboxGame3: Removed Xbox game with Name or UUID {xboxGameNameOrId} ");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"XboxLibrary/RemoveXboxGame3: Didn't remove Xbox game with Name or UUID {xboxGameNameOrId} from the Xbox Library");
                return false;
            }
            else
                throw new XboxLibraryException();

        }

        public override bool ContainsGame(Game xboxGame)
        {
            if (!(xboxGame is XboxGame))
                return false;

            foreach (XboxGame testXboxGame in _allXboxGames)
            {
                if (testXboxGame.Id.Equals(xboxGame.Id))
                    return true;
            }

            return false;
        }

        public override bool ContainsGameById(string xboxGameId)
        {
            foreach (XboxGame testXboxGame in _allXboxGames)
            {
                if (xboxGameId == testXboxGame.Id)
                    return true;
            }


            return false;

        }

        public override bool ContainsGame(string xboxGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(xboxGameNameOrId))
                return false;


            Match match = Regex.Match(xboxGameNameOrId, GogAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (XboxGame testXboxGame in _allXboxGames)
                {
                    if (xboxGameNameOrId.Equals(Convert.ToInt32(testXboxGame.Id)))
                        return true;
                }

            }
            else
            {
                foreach (XboxGame testXboxGame in _allXboxGames)
                {
                    if (xboxGameNameOrId.Equals(testXboxGame.Name))
                        return true;
                }

            }

            return false;

        }


        public override Game GetGame(string xboxGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(xboxGameNameOrId))
                return null;

            Match match = Regex.Match(xboxGameNameOrId, GogAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (XboxGame testXboxGame in _allXboxGames)
                {
                    if (xboxGameNameOrId.Equals(Convert.ToInt32(testXboxGame.Id)))
                        return testXboxGame;
                }

            }
            else
            {
                foreach (XboxGame testXboxGame in _allXboxGames)
                {
                    if (xboxGameNameOrId.Equals(testXboxGame.Name))
                        return testXboxGame;
                }

            }

            return null;

        }

        public override Game GetGameById(string xboxGameId)
        {
            foreach (XboxGame testXboxGame in _allXboxGames)
            {
                if (xboxGameId == testXboxGame.Id)
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
                    string xboxGameInfoFilename = Path.Combine(gogSupportInstallerGameDir, $"XboxGame-{gameID}.info");
                    logger.Trace($"XboxLibrary/LoadInstalledGames: Looking for games info file {xboxGameInfoFilename}");
                    if (!File.Exists(xboxGameInfoFilename))
                    {
                        logger.Warn($"XboxLibrary/LoadInstalledGames: Couldn't find games info file {xboxGameInfoFilename}. There seems to be a problem with your GOG installation.");
                        continue;
                    }

                    // Now we get the information from the Gog Info file to parse it
                    XboxGameInfo xboxGameInfo;
                    try
                    {
                        xboxGameInfo = JsonConvert.DeserializeObject<XboxGameInfo>(File.ReadAllText(xboxGameInfoFilename));
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"XboxLibrary/LoadInstalledGames: Exception trying to convert the {xboxGameInfoFilename} to a JSON object to read the installed games. There seems to be a problem with your GOG installation.");
                        continue;
                    }

                    // Now we check this is a 'Root Game' i.e. it is a  base game, not something else
                    if (xboxGameInfo.gameId != xboxGameInfo.rootGameId)
                    {
                        logger.Trace($"XboxLibrary/LoadInstalledGames: Game {xboxGameInfo.name} is not a base game (probably DLC) so we're skipping it.");
                    }

                    // Now we check the Gog game registry key too, to get some more information that we need
                    string registryGogGalaxyGameKey = registryGogGalaxyGamesKey + xboxGameInfo.gameId;
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
                    XboxGame xboxGame = new XboxGame();
                    xboxGame.Id = xboxGameInfo.gameId;
                    xboxGame.Name = xboxGameInfo.name;
                    xboxGame.Directory = gameDirectory;
                    xboxGame.Executable = GogGalaxyGameKey.GetValue("exeFile", "").ToString();
                    xboxGame.ExePath = gameExePath;
                    //XboxGame.IconPath = gameIconPath;
                    xboxGame.IconPath = gameExePath;
                    xboxGame.ProcessName = Path.GetFileNameWithoutExtension(xboxGame.ExePath);

                    // Add the Gog Game to the list of Gog Games
                    _allXboxGames.Add(xboxGame);
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
            List<Process> startedProcesses = new List<Process>();
            if (game.Start(out startedProcesses, gameArguments, processPriority))
            {
                logger.Trace($"XboxLibrary/StartGame: Successfully started Xbox game {game.Name}");
            }
            else
            {
                logger.Trace($"XboxLibrary/StartGame: Failed to start Xbox game {game.Name}");
            }
            return startedProcesses;
        }

        public override bool StopGame(Game game)
        {
            if (game.Stop())
            {
                logger.Trace($"XboxLibrary/StopGame: Successfully stopped Xbox game {game.Name}");
                return true;
            }
            else
            {
                logger.Trace($"XboxLibrary/StopGame: Failed to stop Xbox game {game.Name}");
                return false;
            }            
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
