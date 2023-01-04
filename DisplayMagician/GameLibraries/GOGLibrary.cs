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
    public sealed class GogLibrary : GameLibrary
    {
        #region Class Variables
        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static readonly GogLibrary _instance = new GogLibrary();


        // Common items to the class
        private List<Game> _allGogGames = new List<Game>();
        private string GogAppIdRegex = @"/^[0-9A-F]{1,10}$";
        private string _gogExe;
        private string _gogPath;
        private string _gogLocalContent = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GOG.com");
        private string _gogProgramFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "GOG Galaxy");
        private bool _isGogInstalled = false;
        private List<string> _gogProcessList = new List<string>(){ "GalaxyClient" };

        internal string registryGogGalaxyClientKey = @"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient"; 
        internal string registryGogGalaxyClientPathKey = @"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths";
        internal string registryGogGalaxyGamesKey = @"SOFTWARE\WOW6432Node\GOG.com\Games\";     
        //internal string registryGogLauncherKey = @"SOFTWARE\WOW6432Node\Gog";
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        // Other constants that are useful
        #endregion

        #region Class Constructors
        static GogLibrary() { }

        private GogLibrary()
        {
            try
            {
                logger.Trace($"GogLibrary/GogLibrary: Gog Online Services registry key = HKLM\\{registryGogGalaxyClientKey}");
                // Find the GogExe location, and the GogPath for later
                RegistryKey GogGalaxyClientKey = Registry.LocalMachine.OpenSubKey(registryGogGalaxyClientKey, RegistryKeyPermissionCheck.ReadSubTree);
                if (GogGalaxyClientKey == null)
                {
                    logger.Info($"GogLibrary/GogLibrary: GOG library is not installed!");
                    return;
                }
                string gogClientExeFilename = GogGalaxyClientKey.GetValue("clientExecutable", @"GalaxyClient.exe").ToString();

                RegistryKey GogGalaxyClientPathKey = Registry.LocalMachine.OpenSubKey(registryGogGalaxyClientPathKey, RegistryKeyPermissionCheck.ReadSubTree);
                string gogClientPath = GogGalaxyClientKey.GetValue("client", @"C:\Program Files (x86)\GOG Galaxy").ToString();
                _gogPath = Path.GetDirectoryName(gogClientPath);
                _gogExe = Path.Combine(gogClientPath, gogClientExeFilename);                
                if (File.Exists(_gogExe))
                {
                    logger.Info($"GogLibrary/GogLibrary: GOG library is installed in {_gogPath}. Found {_gogExe}");
                    _isGogInstalled = true;
                }
                else
                {
                    logger.Info($"GogLibrary/GogLibrary: GOG library is not installed!");
                }
                   
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "GogLibrary/GogLibrary: The user does not have the permissions required to read the Gog Online Services registry key.");
            }
            catch(ObjectDisposedException ex)
            {
                logger.Warn(ex, "GogLibrary/GogLibrary: The Microsoft.Win32.RegistryKey is closed when trying to access the Gog Online Services registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "GogLibrary/GogLibrary: The Gog Online Services registry key has been marked for deletion so we cannot access the value dueing the GogLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "GogLibrary/GogLibrary: The user does not have the necessary registry rights to check whether Gog is installed.");
            }
        }
        #endregion

        #region Class Properties
        public override List<Game> AllInstalledGames
        {
            get
            {
                // Disabled as we now do it manually when DM starts
                // Load the Gog Games from Gog Client if needed
                /*if (_allGogGames.Count == 0)
                    LoadInstalledGames();*/
                return _allGogGames;
            }
        }


        public override int InstalledGameCount
        {
            get
            {
                return _allGogGames.Count;
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
                return _gogExe;
            }
        }

        public override string GameLibraryPath
        {
            get
            {
                return _gogPath;
            }
        }

        public override bool IsGameLibraryInstalled
        {
            get
            {
                return _isGogInstalled;
            }

        }

        public override bool IsRunning
        {
            get
            {
                List<Process> gogLibraryProcesses = new List<Process>();

                try
                {
                    foreach (string gogLibraryProcessName in _gogProcessList)
                    {
                        // Look for the processes with the ProcessName we sorted out earlier
                        gogLibraryProcesses.AddRange(Process.GetProcessesByName(gogLibraryProcessName));
                    }

                    // If we have found one or more processes then we should be good to go
                    // so let's break, and get to the next step....
                    if (gogLibraryProcesses.Count > 0)
                        return true;
                    else
                        return false;
                }                
                catch (Exception ex) {
                    logger.Warn(ex, $"GogLibrary/IsRunning: Exception while trying to get the GOG Library processes with names: {_gogProcessList.ToString()}");
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
                return _gogProcessList;
            }
        }


        #endregion

        #region Class Methods
        public static GogLibrary GetLibrary()
        {
            return _instance;
        }


        public override bool AddGame(Game gogGame)
        {
            if (!(gogGame is GogGame))
                return false;
            
            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (ContainsGame(gogGame))
            {
                logger.Debug($"GogLibrary/AddGogGame: Updating Gog game {gogGame.Name} in our Gog library");
                // We update the existing Shortcut with the data over
                GogGame gogGameToUpdate = (GogGame)GetGame(gogGame.Id.ToString());
                gogGame.CopyTo(gogGameToUpdate);
            }
            else
            {
                logger.Debug($"GogLibrary/AddGogGame: Adding Gog game {gogGame.Name} to our Gog library");
                // Add the GogGame to the list of GogGames
                _allGogGames.Add(gogGame);
            }

            //Doublecheck it's been added
            if (ContainsGame(gogGame))
            {
                return true;
            }
            else
                return false;

        }

        public override bool RemoveGame(Game gogGame)
        {
            if (!(gogGame is GogGame))
                return false;

            logger.Debug($"GogLibrary/RemoveGogGame: Removing Gog game {gogGame.Name} from our Gog library");

            // Remove the GogGame from the list.
            int numRemoved = _allGogGames.RemoveAll(item => item.Id.Equals(gogGame.Id));

            if (numRemoved == 1)
            {
                logger.Debug($"GogLibrary/RemoveGogGame: Removed Gog game with name {gogGame.Name}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"GogLibrary/RemoveGogGame: Didn't remove Gog game with ID {gogGame.Name} from the Gog Library");
                return false;
            }                
            else
                throw new GogLibraryException();
        }

        public override bool RemoveGameById(string gogGameId)
        {
            if (gogGameId.Equals(0))
                return false;

            logger.Debug($"GogLibrary/RemoveGogGame2: Removing Gog game with ID {gogGameId} from the Gog library");

            // Remove the GogGame from the list.
            int numRemoved = _allGogGames.RemoveAll(item => item.Id.Equals(gogGameId));

            if (numRemoved == 1)
            {
                logger.Debug($"GogLibrary/RemoveGogGame2: Removed Gog game with ID {gogGameId}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"GogLibrary/RemoveGogGame2: Didn't remove Gog game with ID {gogGameId} from the Gog Library");
                return false;
            }
            else
                throw new GogLibraryException();
        }

        public override bool RemoveGame(string gogGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(gogGameNameOrId))
                return false;

            logger.Debug($"GogLibrary/RemoveGogGame3: Removing Gog game with Name or ID {gogGameNameOrId} from the Gog library");

            int numRemoved;
            Match match = Regex.Match(gogGameNameOrId, GogAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                numRemoved = _allGogGames.RemoveAll(item => gogGameNameOrId.Equals(item.Id));
            else
                numRemoved = _allGogGames.RemoveAll(item => gogGameNameOrId.Equals(item.Name));

            if (numRemoved == 1)
            {
                logger.Debug($"GogLibrary/RemoveGogGame3: Removed Gog game with Name or UUID {gogGameNameOrId} ");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"GogLibrary/RemoveGogGame3: Didn't remove Gog game with Name or UUID {gogGameNameOrId} from the Gog Library");
                return false;
            }
            else
                throw new GogLibraryException();

        }

        public override bool ContainsGame(Game gogGame)
        {
            if (!(gogGame is GogGame))
                return false;

            foreach (GogGame testGogGame in _allGogGames)
            {
                if (testGogGame.Id.Equals(gogGame.Id))
                    return true;
            }

            return false;
        }

        public override bool ContainsGameById(string gogGameId)
        {
            foreach (GogGame testGogGame in _allGogGames)
            {
                if (gogGameId == testGogGame.Id)
                    return true;
            }


            return false;

        }

        public override bool ContainsGame(string gogGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(gogGameNameOrId))
                return false;


            Match match = Regex.Match(gogGameNameOrId, GogAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (GogGame testGogGame in _allGogGames)
                {
                    if (gogGameNameOrId.Equals(Convert.ToInt32(testGogGame.Id)))
                        return true;
                }

            }
            else
            {
                foreach (GogGame testGogGame in _allGogGames)
                {
                    if (gogGameNameOrId.Equals(testGogGame.Name))
                        return true;
                }

            }

            return false;

        }


        public override Game GetGame(string gogGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(gogGameNameOrId))
                return null;

            Match match = Regex.Match(gogGameNameOrId, GogAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (GogGame testGogGame in _allGogGames)
                {
                    if (gogGameNameOrId.Equals(Convert.ToInt32(testGogGame.Id)))
                        return testGogGame;
                }

            }
            else
            {
                foreach (GogGame testGogGame in _allGogGames)
                {
                    if (gogGameNameOrId.Equals(testGogGame.Name))
                        return testGogGame;
                }

            }

            return null;

        }

        public override Game GetGameById(string gogGameId)
        {
            foreach (GogGame testGogGame in _allGogGames)
            {
                if (gogGameId == testGogGame.Id)
                    return testGogGame;
            }

            return null;

        }

        public override bool LoadInstalledGames()
        {
            try
            {

                if (!_isGogInstalled)
                {
                    // Gog isn't installed, so we return an empty list.
                    logger.Info($"GogLibrary/LoadInstalledGames: Gog library is not installed");
                    return false;
                }

                string gogSupportInstallerDir = Path.Combine(_gogLocalContent, "supportInstaller");

                logger.Trace($"GogLibrary/LoadInstalledGames: supportInstaller Directory {gogSupportInstallerDir} exists!");
                string[] gogSupportInstallerGameDirs = Directory.GetDirectories(gogSupportInstallerDir, "*", SearchOption.AllDirectories);
                logger.Trace($"GogLibrary/LoadInstalledGames: Found game directories in supportInstaller Directory {gogSupportInstallerDir}: {gogSupportInstallerGameDirs.ToString()}");

                // If there are no games installed then return false
                if (gogSupportInstallerGameDirs.Length == 0)
                {
                    logger.Warn($"GogLibrary/LoadInstalledGames: No GOG games installed in the GOG Galaxy library");
                    return false;
                }
                foreach (string gogSupportInstallerGameDir in gogSupportInstallerGameDirs)
                {
                    logger.Trace($"GogLibrary/LoadInstalledGames: Parsing {gogSupportInstallerGameDir} name to find GameID");
                    Match match = Regex.Match(gogSupportInstallerGameDir, @"(\d{10})$");
                    if (!match.Success)
                    {
                        logger.Warn($"GogLibrary/LoadInstalledGames: Failed to match the 10 digit game id from directory name {gogSupportInstallerGameDir} so ignoring game");
                        continue;
                    }

                    string gameID = match.Groups[1].Value;
                    logger.Trace($"GogLibrary/LoadInstalledGames: Found GameID {gameID} matching pattern in game directory name");
                    string gogGameInfoFilename = Path.Combine(gogSupportInstallerGameDir, $"goggame-{gameID}.info");
                    logger.Trace($"GogLibrary/LoadInstalledGames: Looking for games info file {gogGameInfoFilename}");
                    if (!File.Exists(gogGameInfoFilename))
                    {
                        logger.Warn($"GogLibrary/LoadInstalledGames: Couldn't find games info file {gogGameInfoFilename}. There seems to be a problem with your GOG installation.");
                        continue;
                    }

                    // Now we get the information from the Gog Info file to parse it
                    GogGameInfo gogGameInfo;
                    try
                    {
                        gogGameInfo = JsonConvert.DeserializeObject<GogGameInfo>(File.ReadAllText(gogGameInfoFilename));
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"GogLibrary/LoadInstalledGames: Exception trying to convert the {gogGameInfoFilename} to a JSON object to read the installed games. There seems to be a problem with your GOG installation.");
                        continue;
                    }

                    // Now we check this is a 'Root Game' i.e. it is a  base game, not something else
                    if (gogGameInfo.gameId != gogGameInfo.rootGameId)
                    {
                        logger.Trace($"GogLibrary/LoadInstalledGames: Game {gogGameInfo.name} is not a base game (probably DLC) so we're skipping it.");
                    }

                    // Now we check the Gog game registry key too, to get some more information that we need
                    string registryGogGalaxyGameKey = registryGogGalaxyGamesKey + gogGameInfo.gameId;
                    logger.Trace($"GogLibrary/GogLibrary: GOG Galaxy Games registry key = HKLM\\{registryGogGalaxyGameKey}");
                    RegistryKey GogGalaxyGameKey = Registry.LocalMachine.OpenSubKey(registryGogGalaxyGameKey, RegistryKeyPermissionCheck.ReadSubTree);
                    if (GogGalaxyGameKey == null)
                    {
                        logger.Info($"GogLibrary/GogLibrary: Could not find the GOG Galaxy Games registry key {registryGogGalaxyGamesKey} so can't get all the information about the game we need! There seems to be a problem with your GOG installation.");
                        continue;
                    }

                    string gameDirectory = GogGalaxyGameKey.GetValue("path", "").ToString();
                    string gameExePath = GogGalaxyGameKey.GetValue("exe", "").ToString();
                    if (!File.Exists(gameExePath))
                    {
                        logger.Info($"GogLibrary/GogLibrary: Could not find the GOG Galaxy Game file {gameExePath} so can't run the game later! There seems to be a problem with your GOG installation.");
                        continue;
                    }
                    /*string gameIconPath = Path.Combine(gameDirectory, $"goggame-{gameID}.ico");                    
                    if (!File.Exists(gameIconPath))
                    {
                        gameIconPath = gameExePath;
                    }*/

                    // Extract the info into a game object                    
                    GogGame gogGame = new GogGame();
                    gogGame.Id = gogGameInfo.gameId;
                    gogGame.Name = gogGameInfo.name;
                    gogGame.Directory = gameDirectory;
                    gogGame.Executable = GogGalaxyGameKey.GetValue("exeFile", "").ToString();
                    gogGame.ExePath = gameExePath;
                    //gogGame.IconPath = gameIconPath;
                    gogGame.IconPath = gameExePath;
                    gogGame.ProcessName = Path.GetFileNameWithoutExtension(gogGame.ExePath);

                    // Add the Gog Game to the list of Gog Games
                    _allGogGames.Add(gogGame);
                }

                logger.Info($"GogLibrary/LoadInstalledGames: Found {_allGogGames.Count} installed GOG games");

            }

            catch (ArgumentNullException ex)
            {
                logger.Warn(ex, "GogLibrary/GetAllInstalledGames: An argument supplied to the function is null.");
            }
            catch (NotSupportedException ex)
            {
                logger.Warn(ex, "GogLibrary/GetAllInstalledGames: The invoked method is not supported or reading, seeking or writing to a stream that isn't supported.");
            }
            catch (PathTooLongException ex)
            {
                logger.Warn(ex, "GogLibrary/GetAllInstalledGames: The path is longer than the maximum allowed by the operating system.");
            }
            catch (SecurityException ex)
            {
                logger.Warn(ex, "GogLibrary/GetAllInstalledGames: The user does not have the permissions required to read the GOG InstallDir registry key.");
            }
            catch (ObjectDisposedException ex)
            {
                logger.Warn(ex, "GogLibrary/GetAllInstalledGames: The Microsoft.Win32.RegistryKey is closed when trying to access the GOG InstallDir registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "GogLibrary/GetAllInstalledGames: The GOG InstallDir registry key has been marked for deletion so we cannot access the value dueing the GogLibrary check.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "GogLibrary/GetAllInstalledGames: The user does not have the necessary registry rights to check whether Gog is installed.");
            }

            return true;
        }


        public override List<Process> StartGame(Game game, string gameArguments = "", ProcessPriority processPriority = ProcessPriority.Normal)
        {
            List<Process> startedProcesses = new List<Process>();
            if (game.Start(out startedProcesses, gameArguments, processPriority))
            {
                logger.Trace($"GogLibrary/StartGame: Successfully started GoG game {game.Name}");
            }
            else
            {
                logger.Trace($"GogLibrary/StartGame: Failed to start GoG game {game.Name}");
            }
            return startedProcesses;
        }

        public override bool StopGame(Game game)
        {
            if (game.Stop())
            {
                logger.Trace($"GogLibrary/StopGame: Successfully stopped GOG game {game.Name}");
                return true;
            }
            else
            {
                logger.Trace($"GogLibrary/StopGame: Failed to stop GOG game {game.Name}");
                return false;
            }
        }
        #endregion

    }

    public class GogPlayTask
    {
        public string category;
        public string compatibilityFlags;
        public bool isPrimary;
        public List<string> languages;
        public string name;
        public string path;
        public string type;
    }
    public class GogGameInfo
    {
        public string buildId;
        public string clientId;
        public string gameId;
        public string language;
        public List<string> languages;
        public string name;
        public List<GogPlayTask> playTasks;
        public string rootGameId;
        public int version;
    }

    [global::System.Serializable]
    public class GogLibraryException : GameLibraryException
    {
        public GogLibraryException() { }
        public GogLibraryException(string message) : base(message) { }
        public GogLibraryException(string message, Exception inner) : base(message, inner) { }
        protected GogLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
