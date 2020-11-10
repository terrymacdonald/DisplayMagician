using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ValveKeyValue;
//using HeliosPlus.GameLibraries.UplayAppInfoParser;
using Microsoft.Win32;
using System.IO;
using System.Drawing.IconLib;
using System.Security;
using System.Diagnostics;
using EDIDParser;
using System.ComponentModel;

namespace HeliosPlus.GameLibraries
{
    public static class UplayLibrary
    {
        #region Class Variables
        // Common items to the class
        private static List<UplayGame> _allUplayGames = new List<UplayGame>();
        private static string uplayAppIdRegex = @"/^[0-9A-F]{1,10}$";
        private static string _uplayExe;
        private static string _uplayPath;
        private static string _uplayConfigVdfFile;
        internal static string registryUplayLauncherKey = @"SOFTWARE\WOW6432Node\Ubisoft\Launcher";
        internal static string registryUplayInstallsKey = @"SOFTWARE\WOW6432Node\Ubisoft\Launcher\Installs";
        internal static string registryUplayOpenCmdKey = @"SOFTWARE\Classes\uplay\Shell\Open\Command";

        // Other constants that are useful
        #endregion

        private struct UplayAppInfo
        {
            public uint GameID;
            public string GameName;
            public List<string> GameExes;
            public string GameInstallDir;
            public string GameUplayIconPath;
        }

        #region Class Constructors
        static UplayLibrary()
        {
            // Find the UplayExe location, and the UplayPath for later
            RegistryKey uplayInstallKey = Registry.LocalMachine.OpenSubKey(registryUplayLauncherKey, RegistryKeyPermissionCheck.ReadSubTree);
            _uplayPath = uplayInstallKey.GetValue("InstallDir", "C:\\Program Files (x86)\\Ubisoft\\Ubisoft Game Launcher\\").ToString();
            _uplayExe = $"{_uplayPath}upc.exe";
        }
        #endregion

        #region Class Properties
        public static List<UplayGame> AllInstalledGames
        {
            get
            {
                // Load the Uplay Games from Uplay Client if needed
                if (_allUplayGames == null)
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
                if (!string.IsNullOrWhiteSpace(UplayExe) && File.Exists(UplayExe))
                    return true;

                return false;
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
                // We update the existing Shortcut with the data over
                UplayGame uplayGameToUpdate = GetUplayGame(uplayGame.Id.ToString());
                uplayGame.CopyTo(uplayGameToUpdate);
            }
            else
            {
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

            // Remove the uplayGame from the list.
            int numRemoved = _allUplayGames.RemoveAll(item => item.Id.Equals(uplayGame.Id));

            if (numRemoved == 1)
            {
                return true;
            }
            else if (numRemoved == 0)
                return false;
            else
                throw new UplayLibraryException();
        }


        public static bool RemoveUplayGame(string uplayGameNameOrUuid)
        {
            if (String.IsNullOrWhiteSpace(uplayGameNameOrUuid))
                return false;

            int numRemoved;
            Match match = Regex.Match(uplayGameNameOrUuid, uplayAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                numRemoved = _allUplayGames.RemoveAll(item => uplayGameNameOrUuid.Equals(Convert.ToUInt32(item.Id)));
            else
                numRemoved = _allUplayGames.RemoveAll(item => uplayGameNameOrUuid.Equals(item.Name));

            if (numRemoved == 1)
                return true;
            else if (numRemoved == 0)
                return false;
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

        public static bool ContainsUplayGame(string uplayGameNameOrUuid)
        {
            if (String.IsNullOrWhiteSpace(uplayGameNameOrUuid))
                return false;


            Match match = Regex.Match(uplayGameNameOrUuid, uplayAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (UplayGame testUplayGame in _allUplayGames)
                {
                    if (uplayGameNameOrUuid.Equals(Convert.ToUInt32(testUplayGame.Id)))
                        return true;
                }

            }
            else
            {
                foreach (UplayGame testUplayGame in _allUplayGames)
                {
                    if (uplayGameNameOrUuid.Equals(testUplayGame.Name))
                        return true;
                }

            }

            return false;

        }

        public static bool ContainsUplayGame(uint uplayGameId)
        {
            foreach (UplayGame testUplayGame in _allUplayGames)
            {
                if (uplayGameId == testUplayGame.Id)
                    return true;
            }

           
            return false;

        }


        public static UplayGame GetUplayGame(string uplayGameNameOrUuid)
        {
            if (String.IsNullOrWhiteSpace(uplayGameNameOrUuid))
                return null;

            Match match = Regex.Match(uplayGameNameOrUuid, uplayAppIdRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                foreach (UplayGame testUplayGame in _allUplayGames)
                {
                    if (uplayGameNameOrUuid.Equals(Convert.ToUInt32(testUplayGame.Id)))
                        return testUplayGame;
                }

            }
            else
            {
                foreach (UplayGame testUplayGame in _allUplayGames)
                {
                    if (uplayGameNameOrUuid.Equals(testUplayGame.Name))
                        return testUplayGame;
                }

            }

            return null;

        }

        public static UplayGame GetUplayGame(uint uplayGameId)
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

                if (_uplayExe == string.Empty || !File.Exists(_uplayExe))
                {
                    // Uplay isn't installed, so we return an empty list.
                    return false;
                }


                // Look in HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Ubisoft\\Launcher and check the InstallDir key
                // That returns the location of the install dir : E:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\

                //RegistryKey uplayInstallKey = Registry.CurrentUser.OpenSubKey(registryUplayInstallsKey, RegistryKeyPermissionCheck.ReadSubTree);
                //string uplayInstallDir = uplayInstallKey.GetValue("InstallDir", "C:\\Program Files (x86)\\Ubisoft\\Ubisoft Game Launcher\\").ToString();

                // Access {installdir}\\cache\\configuration\\configurations file
                string mypath = _uplayPath + @"cache\\configuration\\configurations";
                string uplayConfigFileString = File.ReadAllText(mypath);
                uplayConfigFileString = uplayConfigFileString.Remove(0, 12);
                // Split the file into records at the SOH unicode character
                List<string> uplayConfigFile = uplayConfigFileString.Split((Char)1).ToList();

                // Go through every record and attempt to parse it
                foreach (string uplayEntry in uplayConfigFile) {
                    // Skip any Uplay entry records that don't start with 'version:' 
                    if (!uplayEntry.StartsWith("version:",StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    //Split the record into entrylines
                    List<string> uplayEntryLines = uplayEntry.Split('\n').ToList();

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
                            string[] split = uplayEntryLines[currentIndex].Trim().Split(':');
                            localizations.Add(split[0], split[1]);
                            currentIndex++;
                        }
                            
                    }

                    // for each game record grab:
                    // name: (lookup the id in lookup table to find the name if needed)


                    // thumb_image: (lookup the id in lookup table to find the thumbnail)
                    // icon_image: (lookup the id in lookup table to find the ICON)
                    // find the exe name looking at root: -> start_game: -> online: -> executables: -> path: -> relative: (get ACU.exe)
                    // Lookup the Game registry key from looking at root: -> start_game: -> online: -> executables: -> working_directory: -> register: (get HKEY_LOCAL_MACHINE\SOFTWARE\Ubisoft\Launcher\Installs\720\InstallDir)
                    // Extract the GameAppID from the number in the working directory (e.g. 720)
                    // Lookup the Game install path by reading the game registry key: D:/Ubisoft Game Launcher/Assassin's Creed Unity/
                    // join the Game install path and the exe name to get the full game exe path: D:/Ubisoft Game Launcher/Assassin's Creed Unity/ACU.exe
                    // Then we have the gameID, the thumbimage, the icon, the name, the exe path
                }


                List<uint> uplayAppIdsInstalled = new List<uint>();
                // Now look for what games app id's are actually installed on this computer
                using (RegistryKey uplayAppsKey = Registry.CurrentUser.OpenSubKey(registryUplayInstallsKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (uplayAppsKey != null)
                    {
                        // Loop through the subKeys as they are the Uplay Game IDs
                        foreach (string uplayGameKeyName in uplayAppsKey.GetSubKeyNames())
                        {
                            uint uplayAppId = 0;
                            if (uint.TryParse(uplayGameKeyName, out uplayAppId))
                            {
                                string uplayGameKeyFullName = $"{ registryUplayInstallsKey}\\{uplayGameKeyName}";
                                using (RegistryKey uplayGameKey = Registry.CurrentUser.OpenSubKey(uplayGameKeyFullName, RegistryKeyPermissionCheck.ReadSubTree))
                                {
                                    // If the Installed Value is set to 1, then the game is installed
                                    // We want to keep track of that for later
                                    if ((int)uplayGameKey.GetValue(@"Installed", 0) == 1)
                                    {
                                        // Add this Uplay App ID to the list we're keeping for later
                                        uplayAppIdsInstalled.Add(uplayAppId);
                                    }

                                }

                            }
                        }
                    }
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
