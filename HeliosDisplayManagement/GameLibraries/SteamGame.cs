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
    public class SteamGame
    {
        private static string _steamExe;
        private static string _steamPath;
        private static string _steamConfigVdfFile;
        private static string _registrySteamKey = @"SOFTWARE\\Valve\\Steam";
        private static string _registryAppsKey = $@"{_registrySteamKey}\\Apps";
        private string _gameRegistryKey;
        private uint _steamGameId;
        private string _steamGameName;
        private string _steamGamePath;
        private string _steamGameExe;
        private string _steamGameIconPath;
        private static List<SteamGame> _allSteamGames;

        private struct SteamAppInfo
        {
            public uint GameID; 
            public string GameName;
            public List<string> GameExes;
            public string GameInstallDir;
            public string GameSteamIconPath;
        }

        static SteamGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }


        public SteamGame(uint steamGameId, string steamGameName, string steamGamePath, string steamGameExe, string steamGameIconPath)
        {

            _gameRegistryKey = $@"{_registryAppsKey}\\{steamGameId}";
            _steamGameId = steamGameId;
            _steamGameName = steamGameName;
            _steamGamePath = steamGamePath;
            _steamGameExe = steamGameExe;
            _steamGameIconPath = steamGameIconPath;

            // Find the SteamExe location, and the SteamPath for later
            using (var key = Registry.CurrentUser.OpenSubKey(_registrySteamKey, RegistryKeyPermissionCheck.ReadSubTree))
            {
                _steamExe = (string)key?.GetValue(@"SteamExe", string.Empty) ?? string.Empty;
                _steamExe = _steamExe.Replace('/', '\\');
                _steamPath = (string)key?.GetValue(@"SteamPath", string.Empty) ?? string.Empty;
                _steamPath = _steamPath.Replace('/', '\\');
            }

        }

        public uint GameId { get => _steamGameId; }

        public static SupportedGameLibrary GameLibrary { get => SupportedGameLibrary.Steam; }

        public string GameIconPath { get => _steamGameIconPath; }
                  
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
                catch (SecurityException e)
                {
                    if (e.Source != null)
                        Console.WriteLine("SecurityException source: {0} - Message: {1}", e.Source, e.Message);
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
                catch (SecurityException e)
                {
                    if (e.Source != null)
                        Console.WriteLine("SecurityException source: {0} - Message: {1}", e.Source, e.Message);
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
            }
        }

        public string GameName { get => _steamGameName; }

        public static string SteamExe { get => _steamExe; }

        public string GamePath { get => _steamGamePath; }

        public static List<SteamGame> AllGames { get => _allSteamGames; }

        public static bool SteamInstalled
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SteamGame._steamExe) && File.Exists(SteamGame._steamExe))
                {
                    return true;
                }
                return false;
            }

        }

        public static List<SteamGame> GetAllInstalledGames()
        {
            List<SteamGame> steamGameList = new List<SteamGame>();
            _allSteamGames = steamGameList;

            try
            {

                // Find the SteamExe location, and the SteamPath for later
                using (var key = Registry.CurrentUser.OpenSubKey(_registrySteamKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    _steamExe = (string)key?.GetValue(@"SteamExe", string.Empty) ?? string.Empty;
                    _steamExe = _steamExe.Replace('/','\\');
                    _steamPath = (string)key?.GetValue(@"SteamPath", string.Empty) ?? string.Empty;
                    _steamPath = _steamPath.Replace('/', '\\');
                }

                if (_steamExe == string.Empty || !File.Exists(_steamExe))
                {
                    // Steam isn't installed, so we return an empty list.
                    return steamGameList;
                }

                //Icon _steamIcon = Icon.ExtractAssociatedIcon(_steamExe);
                //IconExtractor steamIconExtractor = new IconExtractor(_steamExe);
                //Icon _steamIcon = steamIconExtractor.GetIcon(0);
                MultiIcon _steamIcon = new MultiIcon();
                _steamIcon.Load(_steamExe);

                List<uint> steamAppIdsInstalled = new List<uint>();
                // Now look for what games app id's are actually installed on this computer
                using (RegistryKey steamAppsKey = Registry.CurrentUser.OpenSubKey(_registryAppsKey, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (steamAppsKey != null)
                    {
                        // Loop through the subKeys as they are the Steam Game IDs
                        foreach (string steamGameKeyName in steamAppsKey.GetSubKeyNames())
                        {
                            uint steamAppId = 0;
                            if (uint.TryParse(steamGameKeyName, out steamAppId))
                            {
                                string steamGameKeyFullName = $"{_registryAppsKey}\\{steamGameKeyName}";
                                using (RegistryKey steamGameKey = Registry.CurrentUser.OpenSubKey(steamGameKeyFullName, RegistryKeyPermissionCheck.ReadSubTree))
                                {
                                    // If the Installed Value is set to 1, then the game is installed
                                    // We want to keep track of that for later
                                    if ((int)steamGameKey.GetValue(@"Installed", 0) == 1)
                                    {
                                        // Add this Steam App ID to the list we're keeping for later
                                        steamAppIdsInstalled.Add(steamAppId);
                                    }

                                }

                            }
                        }
                    }
                }

                // Now we parse the steam appinfo.vdf to get access to things like:
                // - The game name
                // - THe game installation dir
                // - Sometimes the game icon
                // - Sometimes the game executable name (from which we can get the icon)
                Dictionary<uint, SteamAppInfo> steamAppInfo = new Dictionary<uint, SteamAppInfo>();

                string appInfoVdfFile = Path.Combine(_steamPath, "appcache", "appinfo.vdf");
                var newAppInfo = new AppInfo();
                newAppInfo.Read(appInfoVdfFile);

                Console.WriteLine($"{newAppInfo.Apps.Count} apps");

                // Chec through all the apps we've extracted
                foreach (var app in newAppInfo.Apps)
                {
                    // We only care about the appIDs we have listed as actual games
                    // (The AppIds include all other DLC and Steam specific stuff too)
                    if ( steamAppIdsInstalled.Contains(app.AppID))
                    {

                        try
                        {

                            SteamAppInfo steamGameAppInfo = new SteamAppInfo();
                            steamGameAppInfo.GameID = app.AppID;
                            steamGameAppInfo.GameExes = new List<string>();

                            foreach (KVObject data in app.Data)
                            {
                                //Console.WriteLine($"App: {app.AppID} - Data.Name: {data.Name}");

                                if (data.Name == "common")
                                {
                                    foreach (KVObject common in data.Children) {

                                        //Console.WriteLine($"App: {app.AppID} - Common {common.Name}: {common.Value}");

                                        if (common.Name == "name")
                                        {
                                            Console.WriteLine($"App: {app.AppID} - Common {common.Name}: {common.Value}");
                                            steamGameAppInfo.GameName = common.Value.ToString();
                                        }
                                        else if (common.Name == "clienticon")
                                        {
                                            Console.WriteLine($"App: {app.AppID} - Common {common.Name}: {common.Value}");
                                            steamGameAppInfo.GameSteamIconPath = Path.Combine(_steamPath, @"steam", @"games", String.Concat(common.Value, @".ico"));
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
                                            steamGameAppInfo.GameInstallDir = config.Value.ToString();
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
                                                        steamGameAppInfo.GameExes.Add(launch_num.Value.ToString());
                                                    }
                                                    
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            steamAppInfo.Add(app.AppID, steamGameAppInfo);
                        }
                        catch (ArgumentException e)
                        {
                            //we just want to ignore it if we try to add it twice....
                        }

                        Console.WriteLine($"App: {app.AppID} - Token: {app.Token}");
                    }
                }


               
                // Now we access the config.vdf that lives in the Steam Config file, as that lists all 
                // the SteamLibraries. We need to find out where they areso we can interrogate them
                _steamConfigVdfFile = Path.Combine(_steamPath, "config", "config.vdf");
                string steamConfigVdfText = File.ReadAllText(_steamConfigVdfFile, Encoding.UTF8);

                List<string> steamLibrariesPaths = new List<string>();
                // Now we have to parse the config.vdf looking for the location of the SteamLibraries
                // We look for lines similar to this: "BaseInstallFolder_1"		"E:\\SteamLibrary"
                // There may be multiple so we need to check the whole file
                Regex steamLibrariesRegex = new Regex(@"""BaseInstallFolder_\d+""\s+""(.*)""", RegexOptions.IgnoreCase);
                // Try to match all lines against the Regex.
                Match steamLibrariesMatches = steamLibrariesRegex.Match(steamConfigVdfText);
                // If at least one of them matched!
                if (steamLibrariesMatches.Success)
                {
                    // Loop throug the results and add to an array
                    for (int i = 1; i < steamLibrariesMatches.Groups.Count; i++)
                    {
                        string steamLibraryPath = Regex.Unescape(steamLibrariesMatches.Groups[i].Value);
                        Console.WriteLine($"Found steam library: {steamLibraryPath}");
                        steamLibrariesPaths.Add(steamLibraryPath);
                    }
                }

                // Now we go off and find the details for the games in each Steam Library
                foreach (string steamLibraryPath in steamLibrariesPaths) 
                {
                    // Work out the path to the appmanifests for this steamLibrary
                    string steamLibraryAppManifestPath = Path.Combine(steamLibraryPath, @"steamapps");
                    // Get the names of the App Manifests for the games installed in this SteamLibrary
                    string[] steamLibraryAppManifestFilenames = Directory.GetFiles(steamLibraryAppManifestPath, "appmanifest_*.acf");
                    // Go through each app and extract it's details
                    foreach (string steamLibraryAppManifestFilename in steamLibraryAppManifestFilenames)
                    {
                        // Read in the contents of the file
                        string steamLibraryAppManifestText = File.ReadAllText(steamLibraryAppManifestFilename);
                        // Grab the appid from the file
                        Regex appidRegex = new Regex(@"""appid""\s+""(\d+)""", RegexOptions.IgnoreCase);
                        Match appidMatches = appidRegex.Match(steamLibraryAppManifestText);
                        if (appidMatches.Success)
                        {

                            uint steamGameId = 0;
                            if (uint.TryParse(appidMatches.Groups[1].Value, out steamGameId))
                            {
                                // Check if this game is one that was installed
                                if (steamAppInfo.ContainsKey(steamGameId)) {
                                    // This game is an installed game! so we start to populate it with data!
                                    string steamGameExe = "";

                                    string steamGameName = steamAppInfo[steamGameId].GameName;

                                    // Construct the full path to the game dir from the appInfo and libraryAppManifest data
                                    string steamGameInstallDir = Path.Combine(steamLibraryPath, @"steamapps", @"common", steamAppInfo[steamGameId].GameInstallDir);

                                    // Next, we need to get the Icons we want to use, and make sure it's the latest one.
                                    string steamGameIconPath = "";
                                    // First of all, we attempt to use the Icon that Steam has cached, if it's available, as that will be updated to the latest
                                    if (File.Exists(steamAppInfo[steamGameId].GameSteamIconPath))
                                    {
                                        steamGameIconPath = steamAppInfo[steamGameId].GameSteamIconPath;
                                    }
                                    // If there isn't an icon for us to use, then we need to extract one from the Game Executables
                                    else if (steamAppInfo[steamGameId].GameExes.Count > 0)
                                    {
                                        foreach (string gameExe in steamAppInfo[steamGameId].GameExes)
                                        {
                                            steamGameExe = Path.Combine(steamGameInstallDir, gameExe);
                                            // If the game executable exists, then we can proceed
                                            if (File.Exists(steamGameExe))
                                            {
                                                // Now we need to get the Icon from the app if possible if it's not in the games folder
                                                steamGameIconPath = steamGameExe;
                                            }
                                        }

                                    } 
                                    // The absolute worst case means we don't have an icon to use. SO we use the Steam one.
                                    else
                                    {
                                        // And we have to make do with a Steam Icon
                                        steamGameIconPath = _steamPath;

                                    }

                                    // And finally we try to populate the 'where', to see what gets run
                                    // And so we can extract the process name
                                    if (steamAppInfo[steamGameId].GameExes.Count > 0)
                                    {
                                        foreach (string gameExe in steamAppInfo[steamGameId].GameExes)
                                        {
                                            steamGameExe = Path.Combine(steamGameInstallDir, gameExe);
                                            // If the game executable exists, then we can proceed
                                            if (File.Exists(steamGameExe))
                                            {
                                                break;
                                            }
                                        }

                                    }

                                    // And we add the Game to the list of games we have!
                                    steamGameList.Add(new SteamGame(steamGameId, steamGameName, steamGameInstallDir, steamGameExe, steamGameIconPath));

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

            return steamGameList;
        }

        public override string ToString()
        {
            var name = _steamGameName;

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

            return name;
        }

    }
}