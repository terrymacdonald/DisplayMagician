using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HeliosPlus.Resources;
using HeliosPlus.Shared;
using HtmlAgilityPack;
using Microsoft.Win32;
using Newtonsoft.Json;
//using VdfParser;
//using Gameloop.Vdf;
using System.Collections.ObjectModel;
using ValveKeyValue;
using System.Security.Cryptography;
using System.ServiceModel.Configuration;
using HeliosPlus.GameLibraries.SteamAppInfoParser;
using TsudaKageyu;
using System.Drawing.IconLib;
using System.Drawing.IconLib.Exceptions;

namespace HeliosPlus.GameLibraries
{
    public class UplayGame
    {
        private static string _uplayExe;
        private static string _uplayPath;
        private static string _uplayConfigVdfFile;
        private static string _registryUplayKey = @"SOFTWARE\\Valve\\Uplay";
        private static string _registryAppsKey = $@"{_registryUplayKey}\\Apps";
        private string _gameRegistryKey;
        private uint _uplayGameId;
        private string _uplayGameName;
        private string _uplayGamePath;
        private string _uplayGameExe;
        private string _uplayGameIconPath;
        private static List<UplayGame> _allUplayGames;

        private struct UplayAppInfo
        {
            public uint GameID;
            public string GameName;
            public List<string> GameExes;
            public string GameInstallDir;
            public string GameUplayIconPath;
        }

        static UplayGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }


        public UplayGame(uint uplayGameId, string uplayGameName, string uplayGamePath, string uplayGameExe, string uplayGameIconPath)
        {

            _gameRegistryKey = $@"{_registryAppsKey}\\{uplayGameId}";
            _uplayGameId = uplayGameId;
            _uplayGameName = uplayGameName;
            _uplayGamePath = uplayGamePath;
            _uplayGameExe = uplayGameExe;
            _uplayGameIconPath = uplayGameIconPath;

            // Find the UplayExe location, and the UplayPath for later
            using (var key = Registry.CurrentUser.OpenSubKey(_registryUplayKey, RegistryKeyPermissionCheck.ReadSubTree))
            {
                _uplayExe = (string)key?.GetValue(@"UplayExe", string.Empty) ?? string.Empty;
                _uplayExe = _uplayExe.Replace('/', '\\');
                _uplayPath = (string)key?.GetValue(@"UplayPath", string.Empty) ?? string.Empty;
                _uplayPath = _uplayPath.Replace('/', '\\');
            }

        }

        public uint GameId { get => _uplayGameId; }

        public static SupportedGameLibrary GameLibrary { get => SupportedGameLibrary.Uplay; }

        public string GameIconPath { get => _uplayGameIconPath; }

        public bool IsRunning
        {
            get
            {
                try
                {
                    using (
                        var key = Registry.CurrentUser.OpenSubKey(_gameRegistryKey, RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        if ((int)key?.GetValue(@"Running", 0) == 1)
                        {
                            return true;
                        }
                        return false;
                    }
                }
                catch (SecurityException ex)
                {
                    Console.WriteLine($"SteamGame/IsUpdating securityexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                    if (ex.Source != null)
                        Console.WriteLine("SecurityException source: {0} - Message: {1}", ex.Source, ex.Message);
                    throw;
                }
                catch (IOException ex)
                {
                    // Extract some information from this exception, and then
                    // throw it to the parent method.
                    Console.WriteLine($"SteamGame/IsUpdating ioexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                    if (ex.Source != null)
                        Console.WriteLine("IOException source: {0} - Message: {1}", ex.Source, ex.Message);
                    throw;
                }
            }
        }

        public bool IsUpdating
        {
            get
            {
                try
                {
                    using (
                        var key = Registry.CurrentUser.OpenSubKey(_gameRegistryKey, RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        if ((int)key?.GetValue(@"Updating", 0) == 1)
                        {
                            return true;
                        }
                        return false;
                    }
                }
                catch (SecurityException ex)
                {
                    Console.WriteLine($"SteamGame/IsUpdating securityexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                    if (ex.Source != null)
                        Console.WriteLine("SecurityException source: {0} - Message: {1}", ex.Source, ex.Message);
                    throw;
                }
                catch (IOException ex)
                {
                    // Extract some information from this exception, and then
                    // throw it to the parent method.
                    Console.WriteLine($"SteamGame/IsUpdating ioexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                    if (ex.Source != null)
                        Console.WriteLine("IOException source: {0} - Message: {1}", ex.Source, ex.Message);
                    throw;
                }
            }
        }

        public string GameName { get => _uplayGameName; }

        public static string UplayExe { get => _uplayExe; }

        public string GamePath { get => _uplayGamePath; }

        public static List<UplayGame> AllGames { get => _allUplayGames; }

        public static bool UplayInstalled
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(UplayGame._uplayExe) && File.Exists(UplayGame._uplayExe))
                {
                    return true;
                }
                return false;
            }

        }

        public static List<UplayGame> GetAllInstalledGames()
        {
            List<UplayGame> uplayGameList = new List<UplayGame>();
            _allUplayGames = uplayGameList;

            try
            {

                // Find the UplayExe location, and the UplayPath for later
                using (var key = Registry.CurrentUser.OpenSubKey(_registryUplayKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    _uplayExe = (string)key?.GetValue(@"UplayExe", string.Empty) ?? string.Empty;
                    _uplayExe = _uplayExe.Replace('/', '\\');
                    _uplayPath = (string)key?.GetValue(@"UplayPath", string.Empty) ?? string.Empty;
                    _uplayPath = _uplayPath.Replace('/', '\\');
                }

                if (_uplayExe == string.Empty || !File.Exists(_uplayExe))
                {
                    // Uplay isn't installed, so we return an empty list.
                    return uplayGameList;
                }

                //Icon _uplayIcon = Icon.ExtractAssociatedIcon(_uplayExe);
                //IconExtractor uplayIconExtractor = new IconExtractor(_uplayExe);
                //Icon _uplayIcon = uplayIconExtractor.GetIcon(0);
                MultiIcon _uplayIcon = new MultiIcon();
                _uplayIcon.Load(_uplayExe);

                List<uint> uplayAppIdsInstalled = new List<uint>();
                // Now look for what games app id's are actually installed on this computer
                using (RegistryKey uplayAppsKey = Registry.CurrentUser.OpenSubKey(_registryAppsKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (uplayAppsKey != null)
                    {
                        // Loop through the subKeys as they are the Uplay Game IDs
                        foreach (string uplayGameKeyName in uplayAppsKey.GetSubKeyNames())
                        {
                            uint uplayAppId = 0;
                            if (uint.TryParse(uplayGameKeyName, out uplayAppId))
                            {
                                string uplayGameKeyFullName = $"{_registryAppsKey}\\{uplayGameKeyName}";
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

                // Now we parse the uplay appinfo.vdf to get access to things like:
                // - The game name
                // - THe game installation dir
                // - Sometimes the game icon
                // - Sometimes the game executable name (from which we can get the icon)
                Dictionary<uint, UplayAppInfo> uplayAppInfo = new Dictionary<uint, UplayAppInfo>();

                string appInfoVdfFile = Path.Combine(_uplayPath, "appcache", "appinfo.vdf");
                var newAppInfo = new AppInfo();
                newAppInfo.Read(appInfoVdfFile);

                Console.WriteLine($"{newAppInfo.Apps.Count} apps");

                // Chec through all the apps we've extracted
                foreach (var app in newAppInfo.Apps)
                {
                    // We only care about the appIDs we have listed as actual games
                    // (The AppIds include all other DLC and Uplay specific stuff too)
                    if (uplayAppIdsInstalled.Contains(app.AppID))
                    {

                        try
                        {

                            UplayAppInfo uplayGameAppInfo = new UplayAppInfo();
                            uplayGameAppInfo.GameID = app.AppID;
                            uplayGameAppInfo.GameExes = new List<string>();

                            foreach (KVObject data in app.Data)
                            {
                                //Console.WriteLine($"App: {app.AppID} - Data.Name: {data.Name}");

                                if (data.Name == "common")
                                {
                                    foreach (KVObject common in data.Children)
                                    {

                                        //Console.WriteLine($"App: {app.AppID} - Common {common.Name}: {common.Value}");

                                        if (common.Name == "name")
                                        {
                                            Console.WriteLine($"App: {app.AppID} - Common {common.Name}: {common.Value}");
                                            uplayGameAppInfo.GameName = common.Value.ToString();
                                        }
                                        else if (common.Name == "clienticon")
                                        {
                                            Console.WriteLine($"App: {app.AppID} - Common {common.Name}: {common.Value}");
                                            uplayGameAppInfo.GameUplayIconPath = Path.Combine(_uplayPath, @"uplay", @"games", String.Concat(common.Value, @".ico"));
                                        }
                                        else if (common.Name == "type")
                                        {
                                            Console.WriteLine($"App: {app.AppID} - Common {common.Name}: {common.Value}");
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
                                            Console.WriteLine($"App: {app.AppID} - Config {config.Name}: {config.Value}");
                                            uplayGameAppInfo.GameInstallDir = config.Value.ToString();
                                        }
                                        else if (config.Name == "launch")
                                        {
                                            foreach (KVObject launch in config.Children)
                                            {
                                                foreach (KVObject launch_num in launch.Children)
                                                {
                                                    if (launch_num.Name == "executable")
                                                    {
                                                        Console.WriteLine($"App: {app.AppID} - Config - Launch {launch.Name} - {launch_num.Name}: {launch_num.Value}");
                                                        uplayGameAppInfo.GameExes.Add(launch_num.Value.ToString());
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            uplayAppInfo.Add(app.AppID, uplayGameAppInfo);
                        }
                        catch (ArgumentException ex)
                        {
                            Console.WriteLine($"UplayGame/GetAllInstalledGames argumentexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                            //we just want to ignore it if we try to add it twice....
                        }

                        Console.WriteLine($"App: {app.AppID} - Token: {app.Token}");
                    }
                }



                // Now we access the config.vdf that lives in the Uplay Config file, as that lists all 
                // the UplayLibraries. We need to find out where they areso we can interrogate them
                _uplayConfigVdfFile = Path.Combine(_uplayPath, "config", "config.vdf");
                string uplayConfigVdfText = File.ReadAllText(_uplayConfigVdfFile, Encoding.UTF8);

                List<string> uplayLibrariesPaths = new List<string>();
                // Now we have to parse the config.vdf looking for the location of the UplayLibraries
                // We look for lines similar to this: "BaseInstallFolder_1"		"E:\\UplayLibrary"
                // There may be multiple so we need to check the whole file
                Regex uplayLibrariesRegex = new Regex(@"""BaseInstallFolder_\d+""\s+""(.*)""", RegexOptions.IgnoreCase);
                // Try to match all lines against the Regex.
                Match uplayLibrariesMatches = uplayLibrariesRegex.Match(uplayConfigVdfText);
                // If at least one of them matched!
                if (uplayLibrariesMatches.Success)
                {
                    // Loop throug the results and add to an array
                    for (int i = 1; i < uplayLibrariesMatches.Groups.Count; i++)
                    {
                        string uplayLibraryPath = Regex.Unescape(uplayLibrariesMatches.Groups[i].Value);
                        Console.WriteLine($"Found uplay library: {uplayLibraryPath}");
                        uplayLibrariesPaths.Add(uplayLibraryPath);
                    }
                }

                // Now we go off and find the details for the games in each Uplay Library
                foreach (string uplayLibraryPath in uplayLibrariesPaths)
                {
                    // Work out the path to the appmanifests for this uplayLibrary
                    string uplayLibraryAppManifestPath = Path.Combine(uplayLibraryPath, @"uplayapps");
                    // Get the names of the App Manifests for the games installed in this UplayLibrary
                    string[] uplayLibraryAppManifestFilenames = Directory.GetFiles(uplayLibraryAppManifestPath, "appmanifest_*.acf");
                    // Go through each app and extract it's details
                    foreach (string uplayLibraryAppManifestFilename in uplayLibraryAppManifestFilenames)
                    {
                        // Read in the contents of the file
                        string uplayLibraryAppManifestText = File.ReadAllText(uplayLibraryAppManifestFilename);
                        // Grab the appid from the file
                        Regex appidRegex = new Regex(@"""appid""\s+""(\d+)""", RegexOptions.IgnoreCase);
                        Match appidMatches = appidRegex.Match(uplayLibraryAppManifestText);
                        if (appidMatches.Success)
                        {

                            uint uplayGameId = 0;
                            if (uint.TryParse(appidMatches.Groups[1].Value, out uplayGameId))
                            {
                                // Check if this game is one that was installed
                                if (uplayAppInfo.ContainsKey(uplayGameId))
                                {
                                    // This game is an installed game! so we start to populate it with data!
                                    string uplayGameExe = "";

                                    string uplayGameName = uplayAppInfo[uplayGameId].GameName;

                                    // Construct the full path to the game dir from the appInfo and libraryAppManifest data
                                    string uplayGameInstallDir = Path.Combine(uplayLibraryPath, @"uplayapps", @"common", uplayAppInfo[uplayGameId].GameInstallDir);

                                    // Next, we need to get the Icons we want to use, and make sure it's the latest one.
                                    string uplayGameIconPath = "";
                                    // First of all, we attempt to use the Icon that Uplay has cached, if it's available, as that will be updated to the latest
                                    if (File.Exists(uplayAppInfo[uplayGameId].GameUplayIconPath))
                                    {
                                        uplayGameIconPath = uplayAppInfo[uplayGameId].GameUplayIconPath;
                                    }
                                    // If there isn't an icon for us to use, then we need to extract one from the Game Executables
                                    else if (uplayAppInfo[uplayGameId].GameExes.Count > 0)
                                    {
                                        foreach (string gameExe in uplayAppInfo[uplayGameId].GameExes)
                                        {
                                            uplayGameExe = Path.Combine(uplayGameInstallDir, gameExe);
                                            // If the game executable exists, then we can proceed
                                            if (File.Exists(uplayGameExe))
                                            {
                                                // Now we need to get the Icon from the app if possible if it's not in the games folder
                                                uplayGameIconPath = uplayGameExe;
                                            }
                                        }

                                    }
                                    // The absolute worst case means we don't have an icon to use. SO we use the Uplay one.
                                    else
                                    {
                                        // And we have to make do with a Uplay Icon
                                        uplayGameIconPath = _uplayPath;

                                    }

                                    // And finally we try to populate the 'where', to see what gets run
                                    // And so we can extract the process name
                                    if (uplayAppInfo[uplayGameId].GameExes.Count > 0)
                                    {
                                        foreach (string gameExe in uplayAppInfo[uplayGameId].GameExes)
                                        {
                                            uplayGameExe = Path.Combine(uplayGameInstallDir, gameExe);
                                            // If the game executable exists, then we can proceed
                                            if (File.Exists(uplayGameExe))
                                            {
                                                break;
                                            }
                                        }

                                    }

                                    // And we add the Game to the list of games we have!
                                    uplayGameList.Add(new UplayGame(uplayGameId, uplayGameName, uplayGameInstallDir, uplayGameExe, uplayGameIconPath));

                                }
                            }
                        }
                    }
                }
            }
            catch (SecurityException e)
            {
                if (e.Source != null)
                    Console.WriteLine("SecurityException source: {0} - Message: {1}", e.Source, e.Message);
                throw;
            }
            catch (UnauthorizedAccessException e)
            {
                if (e.Source != null)
                    Console.WriteLine("UnauthorizedAccessException  source: {0} - Message: {1}", e.Source, e.Message);
                throw;
            }
            catch (ObjectDisposedException e)
            {
                if (e.Source != null)
                    Console.WriteLine("ObjectDisposedException  source: {0} - Message: {1}", e.Source, e.Message);
                throw;
            }
            catch (IOException e)
            {
                // Extract some information from this exception, and then
                // throw it to the parent method.
                if (e.Source != null)
                    Console.WriteLine("IOException source: {0} - Message: {1}", e.Source, e.Message);
                throw;
            }

            return uplayGameList;
        }

        public override string ToString()
        {
            var name = _uplayGameName;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = Language.Unknown;
            }

            if (IsRunning)
            {
                return name + " " + Language.Running;
            }

            if (IsUpdating)
            {
                return name + " " + Language.Updating;
            }

            /*if (IsInstalled)
            {
                return name + " " + Language.Installed;
            }*/

            return name + " " + Language.Not_Installed;
        }

    }
}