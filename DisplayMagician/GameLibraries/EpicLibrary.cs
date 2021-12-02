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
        private string _epicProgramFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Epic Games");
        private bool _isEpicInstalled = false;
        private List<string> _epicProcessList = new List<string>(){ "EpicGamesLauncher" };

        internal string registryEpicOnlineServicesKey = @"SOFTWARE\Epic Games\EOS";
        //internal string registryEpicLauncherKey = @"SOFTWARE\WOW6432Node\Epic";
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        // Other constants that are useful
        #endregion

        #region Class Constructors
        static EpicLibrary() { }

        private EpicLibrary()
        {
            try
            {
                logger.Trace($"EpicLibrary/EpicLibrary: Epic Online Services registry key = HKLM\\{registryEpicOnlineServicesKey}");
                // Find the EpicExe location, and the EpicPath for later
                RegistryKey EpicOnlineServicesKey = Registry.CurrentUser.OpenSubKey(registryEpicOnlineServicesKey, RegistryKeyPermissionCheck.ReadSubTree);
                if (EpicOnlineServicesKey == null)
                {
                    logger.Info($"EpicLibrary/EpicLibrary: Epic library is not installed!");
                    return;
                }
                    
                _epicExe = EpicOnlineServicesKey.GetValue("ModSdkCommand", @"C:/Program Files (x86)/Epic Games/Launcher/Portal/Binaries/Win64/EpicGamesLauncher.exe").ToString();
                _epicPath = Path.GetDirectoryName(_epicExe);
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
                logger.Warn(ex, "EpicLibrary/EpicLibrary: The user does not have the permissions required to read the Epic Online Services registry key.");
            }
            catch(ObjectDisposedException ex)
            {
                logger.Warn(ex, "EpicLibrary/EpicLibrary: The Microsoft.Win32.RegistryKey is closed when trying to access the Epic Online Services registry key (closed keys cannot be accessed).");
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "EpicLibrary/EpicLibrary: The Epic Online Services registry key has been marked for deletion so we cannot access the value dueing the EpicLibrary check.");
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

                try
                {
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
                catch (Exception ex)
                {
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

                var localInstalledGameListFile = Path.Combine(_epicLocalContent, "UnrealEngineLauncher","LauncherInstalled.dat");
                logger.Trace($"EpicLibrary/LoadInstalledGames: Looking for locally installed games file {localInstalledGameListFile}");
                if (!File.Exists(localInstalledGameListFile))
                {
                    logger.Error($"EpicLibrary/LoadInstalledGames: Couldn't find locally installed games file {localInstalledGameListFile}. There seems to be a problem with your Epic installation.");
                    return false;
                }

                logger.Trace($"EpicLibrary/LoadInstalledGames: Locally installed games file {localInstalledGameListFile} exists!");
                LauncherInstalled epicLauncherInstalledDat;
                try
                {
                    epicLauncherInstalledDat = JsonConvert.DeserializeObject<LauncherInstalled>(File.ReadAllText(localInstalledGameListFile));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"EpicLibrary/LoadInstalledGames: Exception trying to convert the {localInstalledGameListFile} to a JSON object to read the installed games. There seems to be a problem with your Epic installation.");
                    return false;
                }

                List<InstalledManifiest> allManifests = new List<InstalledManifiest>();
                var installListPath = Path.Combine(_epicLocalContent, "EpicGamesLauncher", "Data", "Manifests");
                if (!Directory.Exists(installListPath))
                {
                    logger.Error($"EpicLibrary/LoadInstalledGames: Couldn't find the manifests for any locally installed games {installListPath}. There seems to be a problem with your Epic installation.");
                    return false;
                }

                foreach (string localInstalledGameManifestFile in Directory.GetFiles(installListPath, "*.item"))
                {
                    InstalledManifiest epicManifest;
                    try
                    {
                        epicManifest = JsonConvert.DeserializeObject<InstalledManifiest>(File.ReadAllText(localInstalledGameManifestFile));
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"EpicLibrary/LoadInstalledGames: Exception trying to convert the {localInstalledGameListFile} to a JSON object to read the installed games. There seems to be a problem with your Epic installation.");
                        return false;
                    }

                    if (epicManifest != null)
                    // Some weird issue causes manifest to be created empty by Epic client
                    {
                        allManifests.Add(epicManifest);
                    }
                }

                if (allManifests.Count == 0)
                {
                    logger.Warn($"EpicLibrary/LoadInstalledGames: No Epic games installed in the Epic library");
                    return false;
                }

                foreach (LauncherInstalled.InstalledApp installedApp in epicLauncherInstalledDat.InstallationList)
                {
                    if (installedApp.AppName.StartsWith("UE_"))
                    {
                        continue;
                    }

                    InstalledManifiest installedAppManifest = allManifests.FirstOrDefault(a => a.AppName == installedApp.AppName);

                    // DLC
                    if (installedAppManifest.AppName != installedAppManifest.MainGameAppName)
                    {
                        continue;
                    }

                    // UE plugins
                    if (installedAppManifest.AppCategories?.Any(a => a == "plugins" || a == "plugins/engine") == true)
                    {
                        continue;
                    }

                    // Extract the info into a game object                    
                    EpicGame epicGame = new EpicGame();
                    epicGame.Name = installedAppManifest?.DisplayName ?? Path.GetFileName(installedApp.InstallLocation);
                    epicGame.Directory  = installedAppManifest?.InstallLocation ?? installedApp.InstallLocation;
                    epicGame.Executable = installedAppManifest.LaunchExecutable;
                    epicGame.ExePath = Path.Combine(epicGame.Directory, installedAppManifest.LaunchExecutable);
                    epicGame.IconPath = epicGame.ExePath;
                    epicGame.Id = installedAppManifest?.MainGameAppName?? installedApp.AppName;
                    epicGame.ProcessName = Path.GetFileNameWithoutExtension(epicGame.ExePath);

                    // Add the Epic Game to the list of Epic Games
                    _allEpicGames.Add(epicGame);

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

        /*public override Process StartGame(Game game, string gameArguments = "", ProcessPriorityClass processPriority = ProcessPriorityClass.Normal)
        {
            string address = $@"com.epicgames.launcher://apps/{game.Id}?action=launch&silent=true";
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
            string address = $@"com.epicgames.launcher://apps/{game.Id}?action=launch&silent=true";
            if (!String.IsNullOrWhiteSpace(gameArguments))
            {
                address += @"/" + gameArguments;
            }
            //Process gameProcess = Process.Start(address);
            List<Process> gameProcesses = ProcessUtils.StartProcess(address, null, processPriority);
            return gameProcesses;
        }
        #endregion

    }

    public class LauncherInstalled
    {
        public class InstalledApp
        {
            public string InstallLocation;
            public string AppName;
            public long AppID;
            public string AppVersion;
        }

        public List<InstalledApp> InstallationList;
    }

    public class InstalledManifiest
    {
        public int FormatVersion;
        public bool bIsCompleteInstalln;
        public string LaunchCommand;
        public string LaunchExecutable;
        public string ManifestLocation;
        public bool bIsApplication;
        public bool bIsExecutable;
        public bool bIsManaged;
        public bool bNeedsValidation;
        public bool bRequiresAuth;
        public bool bAllowMultipleInstances;
        public bool bCanRunOffline;
        public string AppName;
        public string CatalogNamespace;
        public string CatalogItemId;
        public List<string> AppCategories;
        public string DisplayName;
        public string FullAppName;
        public string InstallationGuid;
        public string InstallLocation;
        public string InstallSessionId;
        public string StagingLocation;
        public string TechnicalType;
        public string VaultThumbnailUrl;
        public string VaultTitleText;
        public string InstallSize;
        public string MainWindowProcessName;
        public List<string> ProcessNames;
        public string MainGameAppName;
        public string MainGameCatalogueItemId;
        public string MandatoryAppFolderName;
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
