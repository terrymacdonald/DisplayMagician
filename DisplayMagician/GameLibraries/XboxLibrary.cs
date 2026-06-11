using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;
using DisplayMagician;
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
        private string _xboxAppIdRegex = @"^[0-9A-F]{1,16}$";
        private string _xboxExe;
        private string _xboxPath;
        private bool _isXboxInstalled = false;
        private List<string> _xboxProcessList = new List<string>(){ "XboxAppServices" };
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        // Other constants that are useful
        #endregion

        #region Class Constructors
        static XboxLibrary() { }

        private XboxLibrary()
        {
            try
            {
                // Xbox Game Pass relies on Windows Gaming Services (XboxAppServices.exe).
                // Detect availability by checking for its presence in the Windows System32 folder.
                _xboxExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "XboxAppServices.exe");
                _xboxPath = Path.GetDirectoryName(_xboxExe);
                if (File.Exists(_xboxExe))
                {
                    logger.Info($"XboxLibrary/XboxLibrary: Xbox library is available. Found {_xboxExe}");
                    _isXboxInstalled = true;
                }
                else
                {
                    logger.Info($"XboxLibrary/XboxLibrary: Xbox library is not available (XboxAppServices.exe not found).");
                }
            }
            catch (IOException ex)
            {
                logger.Warn(ex, "XboxLibrary/XboxLibrary: IOException when checking for Xbox installation.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Warn(ex, "XboxLibrary/XboxLibrary: UnauthorizedAccessException when checking for Xbox installation.");
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
                return "Xbox";
            } 
        }

        public override SupportedGameLibraryType GameLibraryType
        {
            get
            {
                return SupportedGameLibraryType.Xbox;
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
                return Program.AppHasPackageIdentity && _isXboxInstalled;
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
                catch (Exception) { 
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
            // Fix: Cast your numeric condition rule validation safely using an explicit string comparison block
            if (string.IsNullOrWhiteSpace(xboxGameId) || xboxGameId == "0")
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
            Match match = Regex.Match(xboxGameNameOrId, _xboxAppIdRegex, RegexOptions.IgnoreCase);
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


            Match match = Regex.Match(xboxGameNameOrId, _xboxAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (XboxGame testXboxGame in _allXboxGames)
                {
                    if (xboxGameNameOrId.Equals(testXboxGame.Id))
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

            Match match = Regex.Match(xboxGameNameOrId, _xboxAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (XboxGame testXboxGame in _allXboxGames)
                {
                    if (xboxGameNameOrId.Equals(testXboxGame.Id))
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
            if (!Program.AppHasPackageIdentity)
            {
                logger.Warn($"XboxLibrary/LoadInstalledGames: Skipping Xbox game enumeration because DisplayMagician is not running with package identity.");
                return false;
            }

            if (!_isXboxInstalled)
            {
                logger.Info($"XboxLibrary/LoadInstalledGames: Xbox library is not installed");
                return false;
            }

            try
            {
                // Fix: Flush the primary catalog tracker cache prior to scanning to completely avoid catalog duplication leaks on re-scans!
                _allXboxGames.Clear();

                var manager = new PackageManager();
                IEnumerable<Package> packages = manager.FindPackagesForUser(WindowsIdentity.GetCurrent().User.Value);

                foreach (var package in packages)
                {
                    // Skip frameworks, resource packs, and non-Store packages (sideloaded, dev, system)
                    if (package.IsFramework || package.IsResourcePackage || package.SignatureKind != PackageSignatureKind.Store)
                    {
                        continue;
                    }

                    string installPath;
                    try
                    {
                        if (package.InstalledLocation == null)
                            continue;
                        installPath = package.InstalledLocation.Path;
                    }
                    catch
                    {
                        // InstalledLocation accessor can throw Win32Exception for some packages
                        continue;
                    }

                    // Xbox Game Pass games always have a MicrosoftGame.Config in their install directory;
                    // regular Store apps do not, so this is the definitive Xbox game filter.
                    string gameConfigPath = Path.Combine(installPath, "MicrosoftGame.Config");
                    if (!File.Exists(gameConfigPath))
                        continue;

                    try
                    {
                        var gameConfig = XDocument.Load(gameConfigPath);
                        var gameElement = gameConfig.Root;
                        if (gameElement == null)
                        {
                            logger.Warn($"XboxLibrary/LoadInstalledGames: MicrosoftGame.Config in {installPath} has no root element, skipping.");
                            continue;
                        }

                        string aumid = null;
                        string entryDisplayName = null;
                        try
                        {
                            IReadOnlyList<AppListEntry> appListEntries = (IReadOnlyList<AppListEntry>)package.GetAppListEntries();
                            if (appListEntries.Count > 0)
                            {
                                aumid = appListEntries[0].AppUserModelId;
                                entryDisplayName = appListEntries[0].DisplayInfo.DisplayName;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Debug(ex, $"XboxLibrary/LoadInstalledGames: Could not get AppListEntry for {package.Id.FamilyName}");
                        }

                        string gameName = gameElement.Element("ShellVisuals")?.Attribute("DefaultDisplayName")?.Value;
                        if (String.IsNullOrWhiteSpace(gameName))
                            gameName = entryDisplayName;
                        if (String.IsNullOrWhiteSpace(gameName))
                            gameName = package.Id.FamilyName;
                        gameName = gameName.NormaliseGameName();

                        string titleId = gameElement.Element("TitleId")?.Value;
                        if (String.IsNullOrWhiteSpace(titleId))
                            titleId = package.Id.FamilyName;

                        string exePath = null;
                        var executableList = gameElement.Element("ExecutableList");
                        if (executableList != null)
                        {
                            var exeElement = executableList.Elements("Executable")
                                .FirstOrDefault(e => e.Attribute("TargetDeviceFamily")?.Value == "PC")
                                ?? executableList.Elements("Executable").FirstOrDefault();
                            string exeName = exeElement?.Attribute("Name")?.Value;
                            if (!String.IsNullOrWhiteSpace(exeName))
                                exePath = Path.Combine(installPath, exeName);
                        }

                        if (String.IsNullOrWhiteSpace(exePath) && String.IsNullOrWhiteSpace(aumid))
                        {
                            logger.Debug($"XboxLibrary/LoadInstalledGames: Skipping '{gameName}' — no executable or AUMID found.");
                            continue;
                        }

                        string iconPath = !String.IsNullOrWhiteSpace(exePath) ? exePath : package.Logo.LocalPath;

                        var xboxGame = new XboxGame(titleId, gameName, exePath ?? "", iconPath, aumid ?? "");
                        _allXboxGames.Add(xboxGame);
                        logger.Debug($"XboxLibrary/LoadInstalledGames: Found Xbox game '{gameName}' (TitleId={titleId}, AUMID={aumid})");
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"XboxLibrary/LoadInstalledGames: Exception processing package {package.Id.FamilyName}");
                    }
                }

                logger.Info($"XboxLibrary/LoadInstalledGames: Found {_allXboxGames.Count} installed Xbox Game Pass games");
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "XboxLibrary/LoadInstalledGames: Exception enumerating installed packages");
                return false;
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
    }

}
