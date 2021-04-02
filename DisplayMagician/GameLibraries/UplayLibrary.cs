using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.IO;
using System.Security;

namespace DisplayMagician.GameLibraries
{
    public static class UplayLibrary
    {
        #region Class Variables
        // Common items to the class
        private static List<Game> _allUplayGames = new List<Game>();
        private static string uplayAppIdRegex = @"/^[0-9A-F]{1,10}$";
        private static bool _isUplayInstalled = false;
        private static string _uplayExe;
        private static string _uplayPath;
        //private static string _uplayConfigVdfFile;
        internal static string registryUplayLauncherKey = @"SOFTWARE\WOW6432Node\Ubisoft\Launcher";
        internal static string registryUplayInstallsKey = @"SOFTWARE\WOW6432Node\Ubisoft\Launcher\Installs";
        internal static string registryUplayOpenCmdKey = @"SOFTWARE\Classes\uplay\Shell\Open\Command";
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        // Other constants that are useful
        #endregion

        private struct UplayAppInfo
        {
            public string GameID;
            public string GameName;
            public string GameExe;
            public string GameInstallDir;
            public string GameUplayIconPath;
        }
         
        #region Class Constructors
        static UplayLibrary()
        {
            try
            {
                // Find the UplayExe location, and the UplayPath for later
                RegistryKey uplayInstallKey = Registry.LocalMachine.OpenSubKey(registryUplayLauncherKey, RegistryKeyPermissionCheck.ReadSubTree);
                if (uplayInstallKey == null)
                    return;
                _uplayPath = uplayInstallKey.GetValue("InstallDir", "C:\\Program Files (x86)\\Ubisoft\\Ubisoft Game Launcher\\").ToString();
                _uplayExe = $"{_uplayPath}upc.exe";
                if (File.Exists(_uplayExe))
                   _isUplayInstalled = true;
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
        public static List<Game> AllInstalledGames
        {
            get
            {
                // Load the Uplay Games from Uplay Client if needed
                if (_allUplayGames.Count == 0)
                    LoadInstalledGames();
                return _allUplayGames;
            }
        }


        public static int InstalledUplayGameCount
        {
            get
            {
                return _allUplayGames.Count;
            }
        }

        public static string UplayExe
        {
            get
            {
                return _uplayExe;
            }
        }

        public static string UplayPath
        {
            get
            {
                return _uplayPath;
            }
        }

        public static bool IsUplayInstalled
        {
            get
            {
                return _isUplayInstalled;
            }

        }


        #endregion

        #region Class Methods
        public static bool AddUplayGame(UplayGame uplayGame)
        {
            if (!(uplayGame is UplayGame))
                return false;
            
            // Doublecheck if it already exists
            // Because then we just update the one that already exists
            if (ContainsUplayGame(uplayGame))
            {
                logger.Debug($"UplayLibrary/AddUplayGame: Updating Uplay game {uplayGame.Name} in our Uplay library");
                // We update the existing Shortcut with the data over
                UplayGame uplayGameToUpdate = GetUplayGame(uplayGame.Id.ToString());
                uplayGame.CopyTo(uplayGameToUpdate);
            }
            else
            {
                logger.Debug($"UplayLibrary/AddUplayGame: Adding Uplay game {uplayGame.Name} to our Uplay library");
                // Add the uplayGame to the list of uplayGames
                _allUplayGames.Add(uplayGame);
            }

            //Doublecheck it's been added
            if (ContainsUplayGame(uplayGame))
            {
                return true;
            }
            else
                return false;

        }

        public static bool RemoveUplayGame(UplayGame uplayGame)
        {
            if (!(uplayGame is UplayGame))
                return false;

            logger.Debug($"UplayLibrary/RemoveUplayGame: Removing Uplay game {uplayGame.Name} from our Uplay library");

            // Remove the uplayGame from the list.
            int numRemoved = _allUplayGames.RemoveAll(item => item.Id.Equals(uplayGame.Id));

            if (numRemoved == 1)
            {
                logger.Debug($"UplayLibrary/RemoveUplayGame: Removed Uplay game with name {uplayGame.Name}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"UplayLibrary/RemoveUplayGame: Didn't remove Uplay game with ID {uplayGame.Name} from the Uplay Library");
                return false;
            }
                
            else
                throw new UplayLibraryException();
        }

        public static bool RemoveUplayGameById(string uplayGameId)
        {
            if (uplayGameId.Equals(0))
                return false;

            logger.Debug($"UplayLibrary/RemoveUplayGame2: Removing Uplay game with ID {uplayGameId} from the Uplay library");

            // Remove the uplayGame from the list.
            int numRemoved = _allUplayGames.RemoveAll(item => item.Id.Equals(uplayGameId));

            if (numRemoved == 1)
            {
                logger.Debug($"UplayLibrary/RemoveUplayGame2: Removed Uplay game with ID {uplayGameId}");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"UplayLibrary/RemoveUplayGame2: Didn't remove Uplay game with ID {uplayGameId} from the Uplay Library");
                return false;
            }
            else
                throw new UplayLibraryException();
        }

        public static bool RemoveUplayGame(string uplayGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(uplayGameNameOrId))
                return false;

            logger.Debug($"UplayLibrary/RemoveUplayGame3: Removing Uplay game with Name or ID {uplayGameNameOrId} from the Uplay library");

            int numRemoved;
            Match match = Regex.Match(uplayGameNameOrId, uplayAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                numRemoved = _allUplayGames.RemoveAll(item => uplayGameNameOrId.Equals(item.Id));
            else
                numRemoved = _allUplayGames.RemoveAll(item => uplayGameNameOrId.Equals(item.Name));

            if (numRemoved == 1)
            {
                logger.Debug($"UplayLibrary/RemoveUplayGame3: Removed Uplay game with Name or UUID {uplayGameNameOrId} ");
                return true;
            }
            else if (numRemoved == 0)
            {
                logger.Debug($"UplayLibrary/RemoveUplayGame3: Didn't remove Uplay game with Name or UUID {uplayGameNameOrId} from the Uplay Library");
                return false;
            }
            else
                throw new UplayLibraryException();

        }

        public static bool ContainsUplayGame(UplayGame uplayGame)
        {
            if (!(uplayGame is UplayGame))
                return false;

            foreach (UplayGame testUplayGame in _allUplayGames)
            {
                if (testUplayGame.Id.Equals(uplayGame.Id))
                    return true;
            }

            return false;
        }

        public static bool ContainsUplayGameById(string uplayGameId)
        {
            foreach (UplayGame testUplayGame in _allUplayGames)
            {
                if (uplayGameId == testUplayGame.Id)
                    return true;
            }


            return false;

        }

        public static bool ContainsUplayGame(string uplayGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(uplayGameNameOrId))
                return false;


            Match match = Regex.Match(uplayGameNameOrId, uplayAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (UplayGame testUplayGame in _allUplayGames)
                {
                    if (uplayGameNameOrId.Equals(Convert.ToInt32(testUplayGame.Id)))
                        return true;
                }

            }
            else
            {
                foreach (UplayGame testUplayGame in _allUplayGames)
                {
                    if (uplayGameNameOrId.Equals(testUplayGame.Name))
                        return true;
                }

            }

            return false;

        }


        public static UplayGame GetUplayGame(string uplayGameNameOrId)
        {
            if (String.IsNullOrWhiteSpace(uplayGameNameOrId))
                return null;

            Match match = Regex.Match(uplayGameNameOrId, uplayAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (UplayGame testUplayGame in _allUplayGames)
                {
                    if (uplayGameNameOrId.Equals(Convert.ToInt32(testUplayGame.Id)))
                        return testUplayGame;
                }

            }
            else
            {
                foreach (UplayGame testUplayGame in _allUplayGames)
                {
                    if (uplayGameNameOrId.Equals(testUplayGame.Name))
                        return testUplayGame;
                }

            }

            return null;

        }

        public static UplayGame GetUplayGameById(string uplayGameId)
        {
            foreach (UplayGame testUplayGame in _allUplayGames)
            {
                if (uplayGameId == testUplayGame.Id)
                    return testUplayGame;
            }

            return null;

        }

        public static bool LoadInstalledGames()
        {
            try
            {

                if (!_isUplayInstalled)
                {
                    // Uplay isn't installed, so we return an empty list.
                    return false;
                }


                // Look in HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Ubisoft\\Launcher and check the InstallDir key
                // That returns the location of the install dir : E:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\

                //RegistryKey uplayInstallKey = Registry.CurrentUser.OpenSubKey(registryUplayInstallsKey, RegistryKeyPermissionCheck.ReadSubTree);
                //string uplayInstallDir = uplayInstallKey.GetValue("InstallDir", "C:\\Program Files (x86)\\Ubisoft\\Ubisoft Game Launcher\\").ToString();

                // Access {installdir}\\cache\\configuration\\configurations file
                string uplayConfigFilePath = _uplayPath + @"cache\configuration\configurations";
                string uplayConfigFileString = File.ReadAllText(uplayConfigFilePath);
                uplayConfigFileString = uplayConfigFileString.Remove(0, 12);
                // Split the file into records at the SOH unicode character
                List<string> uplayConfigFile = uplayConfigFileString.Split((Char)1).ToList();

                // Go through every record and attempt to parse it
                foreach (string uplayEntry in uplayConfigFile) {
                    // Skip any Uplay entry records that don't start with 'version:' 
                    if (!uplayEntry.StartsWith("version:",StringComparison.OrdinalIgnoreCase))
                        continue;

                    //Split the record into entrylines
                    string[] delimeters = { "\r\n" };
                    List<string> uplayEntryLines = uplayEntry.Split(delimeters, System.StringSplitOptions.RemoveEmptyEntries).ToList();

                    // Skip any records NOT containing an entryline with '  start_game:' (note 2 leading spaces)
                    // All games contain a start_game entry
                    if (!uplayEntryLines.Exists(a => a.StartsWith("  start_game:")))
                        continue;

                    // Skip any records containing an entryline with '  third_party_platform:' (note 2 leading spaces)
                    // We only want the native uplay games....
                    if (uplayEntryLines.Exists(a => a.StartsWith("  third_party_platform:")))
                        continue;

                    // if we get here then we have a real game to parse!
                    // Yay us :). 

                    // First we want to know the index of the start_game entry to use later
                    //int startGameIndex = uplayEntryLines.FindIndex(a => a.StartsWith("  start_game:"));
                    MatchCollection mc;

                    // First we check if there are any localization CONSTANTS that we will need to map later.
                    Dictionary<string, string> localizations = new Dictionary<string, string>();
                    int localizationsIndex = uplayEntryLines.FindIndex(a => a == "localizations:");
                    // If there are localizations, then we need to store them for later
                    if (localizationsIndex != -1)
                    {
                        // grab the localizations: ->  default: entries to use as a lookup table for the info we need
                        int defaultIndex = localizationsIndex + 1;
                        int currentIndex = defaultIndex + 1;
                        
                        // Grab all EntryLines with 4 leading spaces (these are all the localizations)
                        while (uplayEntryLines[currentIndex].StartsWith("    ")){
                            string[] split = uplayEntryLines[currentIndex].Split(':');
                            localizations.Add(split[0].Trim(), split[1].Trim());
                            currentIndex++;
                        }
                            
                    }

                    // for each game record grab:
                    UplayAppInfo uplayGameAppInfo = new UplayAppInfo();

                    bool gotGameIconPath = false;
                    bool gotGameName = false;
                    string gameFileName = "";
                    bool gotGameFileName = false;
                    string gameId = "";
                    bool gotGameId = false;
                    for (int i = 0; i <= 50; i++)
                    {
                        // Stop this loop once we have both filname and gameid
                        if (gotGameFileName && gotGameId && gotGameIconPath && gotGameName)
                            break;
                        // This line contains the Game Name
                        if (uplayEntryLines[i].StartsWith("  name:", StringComparison.OrdinalIgnoreCase) && !gotGameName)
                        {
                            mc = Regex.Matches(uplayEntryLines[i], @"  name\: (.*)");
                            uplayGameAppInfo.GameName = mc[0].Groups[1].ToString();
                            // if the name contains a localization reference, then dereference it
                            if (localizations.ContainsKey(uplayGameAppInfo.GameName))
                            {
                                uplayGameAppInfo.GameName = localizations[uplayGameAppInfo.GameName];
                            }
                            gotGameName = true;
                        }
                        else if (uplayEntryLines[i].StartsWith("  icon_image:", StringComparison.OrdinalIgnoreCase) && !gotGameIconPath)
                        {
                            mc = Regex.Matches(uplayEntryLines[i], @"icon_image: (.*)");
                            string iconImageFileName = mc[0].Groups[1].ToString();
                            // if the icon_image contains a localization reference, then dereference it
                            if (localizations.ContainsKey(iconImageFileName))
                            {
                                iconImageFileName = localizations[iconImageFileName];
                            }
                            //61fdd16f06ae08158d0a6d476f1c6bd5.ico
                            string uplayGameIconPath = _uplayPath + @"data\games\" + iconImageFileName;
                            if (File.Exists(uplayGameIconPath) && uplayGameIconPath.EndsWith(".ico"))
                            {
                                uplayGameAppInfo.GameUplayIconPath = uplayGameIconPath;
                            }
                            gotGameIconPath = true;
                        }
                        // This line contains the filename
                        else if (uplayEntryLines[i].StartsWith("          relative:") && !gotGameFileName)
                        {
                            mc = Regex.Matches(uplayEntryLines[i], @"relative: (.*)");
                            gameFileName = mc[0].Groups[1].ToString();
                            gotGameFileName = true;
                        }
                        // This line contains the registryKey 
                        else if (uplayEntryLines[i].StartsWith("          register: HKEY_LOCAL_MACHINE") && !gotGameId)
                        {
                            // Lookup the GameId within the registry key
                            mc = Regex.Matches(uplayEntryLines[i], @"Installs\\(\d+)\\InstallDir");
                            gameId = mc[0].Groups[1].ToString();
                            gotGameId = true;
                        }
                    }

                    // Now we need to lookup the game install path in registry using the gameId
                    string registryUplayGameInstallsKey = registryUplayInstallsKey + "\\" + gameId;
                    RegistryKey uplayGameInstallKey = Registry.LocalMachine.OpenSubKey(registryUplayGameInstallsKey, RegistryKeyPermissionCheck.ReadSubTree);

                    // From that  we lookup the actual game path
                    uplayGameAppInfo.GameInstallDir = Path.GetFullPath(uplayGameInstallKey.GetValue("InstallDir", "").ToString()).TrimEnd('\\');
                    uplayGameAppInfo.GameExe = Path.Combine(uplayGameAppInfo.GameInstallDir,gameFileName);
                    uplayGameAppInfo.GameID = gameId;

                    // Then we have the gameID, the thumbimage, the icon, the name, the exe path
                    // And we add the Game to the list of games we have!
                    _allUplayGames.Add(new UplayGame(uplayGameAppInfo.GameID, uplayGameAppInfo.GameName, uplayGameAppInfo.GameExe, uplayGameAppInfo.GameUplayIconPath));
                }

            }
            catch (SecurityException ex)
            {
                Console.WriteLine($"UplayGame/GetAllInstalledGames securityexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                if (ex.Source != null)
                    Console.WriteLine("SecurityException source: {0} - Message: {1}", ex.Source, ex.Message);
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"UplayGame/GetAllInstalledGames unauthorizedaccessexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                if (ex.Source != null)
                    Console.WriteLine("UnauthorizedAccessException  source: {0} - Message: {1}", ex.Source, ex.Message);
                throw;
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"UplayGame/GetAllInstalledGames objectdisposedexceptions: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                if (ex.Source != null)
                    Console.WriteLine("ObjectDisposedException  source: {0} - Message: {1}", ex.Source, ex.Message);
                throw;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"UplayGame/GetAllInstalledGames ioexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
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
    public class UplayLibraryException : Exception
    {
        public UplayLibraryException() { }
        public UplayLibraryException(string message) : base(message) { }
        public UplayLibraryException(string message, Exception inner) : base(message, inner) { }
        protected UplayLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
