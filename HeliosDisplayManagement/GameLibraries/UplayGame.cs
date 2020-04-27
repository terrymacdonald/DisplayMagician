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

namespace HeliosPlus.GameLibraries
{
    public class UplayGame
    {
        private static string _uplayExe;
        private static string _uplayPath;
        private static string _uplayConfigVdfFile;
        private static string _registryUplayKey = @"SOFTWARE\\Valve\\Uplay";
        private static string _registryAppsKey = $@"{_registryUplayKey}\\Apps";
        private static SupportedGameLibrary _library = SupportedGameLibrary.Uplay;
        //private static string _iconCachePath;
        private string _gameRegistryKey;
        private uint _uplayGameId;
        private string _uplayGameName;
        private string _uplayGamePath;
        private string _uplayGameExe;
        private Icon _uplayGameIcon;
        private static List<UplayGame> _allUplayGames;



        static UplayGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }


        public UplayGame(uint uplayGameId, string uplayGameName, string uplayGamePath, string uplayGameExe, Icon uplayGameIcon)
        {

            _gameRegistryKey = $@"{_registryAppsKey}\\{uplayGameId}";
            _uplayGameId = uplayGameId;
            _uplayGameName = uplayGameName;
            //_iconCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            //    Assembly.GetExecutingAssembly().GetName().Name, @"UplayIconCache");
            _uplayGamePath = uplayGamePath;
            _uplayGameExe = uplayGameExe;
            _uplayGameIcon = uplayGameIcon;

        }

        public uint GameId { get => _uplayGameId; }

        public SupportedGameLibrary GameLibrary { get => SupportedGameLibrary.Uplay; }

        public Icon GameIcon { get => _uplayGameIcon; }


        /*        public static string GameIdCacheFilePath
                {
                    get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        Assembly.GetExecutingAssembly().GetName().Name, @"UplayGamesCache.json");
                }
        */

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

        /*public static List<GameLibraryAppDetails> GetAllGames()
        {
            lock (AllGamesLock)
            {
                if (_allGames == null)
                {
                    _allGames = GetCachedGameIds()?.ToList();
                }
            }

            // Update only once
            if (!_allGamesUpdated)
            {
                if (_allGames?.Count > 0)
                {
                    UpdateGamesFromWeb();
                }
                else
                {
                    UpdateGamesFromWeb()?.Join();
                }
            }

            return _allGames;
        }*/

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
                    _uplayPath = (string)key?.GetValue(@"UplayPath", string.Empty) ?? string.Empty;
                }

                if (_uplayExe == string.Empty)
                {
                    // Uplay isn't installed, so we return an empty list.
                    return uplayGameList;
                }

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
                                using (RegistryKey uplayGameKey = Registry.CurrentUser.OpenSubKey(uplayGameKeyName, RegistryKeyPermissionCheck.ReadSubTree))
                                {
                                    // If the Installed Value is set to 1, then the game is installed
                                    // We want to keep track of that for later
                                    if ((bool)uplayGameKey.GetValue(@"Installed", 0))
                                    {
                                        // Add this Uplay App ID to the list we're keeping for later
                                        uplayAppIdsInstalled.Add(uplayAppId);
                                    }

                                }
                            }
                        }
                    }
                }

                // Now we access the config.vdf that lives in the Uplay Config file, as that lists all 
                // the UplayLibraries. We need to find out where they areso we can interrogate them
                _uplayConfigVdfFile = Path.Combine(_uplayPath, @"config", @"config.vdf");
                string uplayConfigVdfText = File.ReadAllText(_uplayConfigVdfFile);

                List<string> uplayLibrariesPaths = new List<string>();
                // Now we have to parse the config.vdf looking for the location of the UplayLibraries
                // We look for lines similar to this: "BaseInstallFolder_1"		"E:\\UplayLibrary"
                // There may be multiple so we need to check the whole file
                Regex uplayLibrariesRegex = new Regex(@"""BaseInstallFolder_\d+""\s+""([^""])"")", RegexOptions.IgnoreCase);
                // Try to match all lines against the Regex.
                Match uplayLibrariesMatches = uplayLibrariesRegex.Match(uplayConfigVdfText);
                // If at least one of them matched!
                if (uplayLibrariesMatches.Success)
                {
                    // Loop throug the results and add to an array
                    for (int i = 1; i <= uplayLibrariesMatches.Groups.Count; i++)
                    {
                        Console.WriteLine($"Found uplay library: {uplayLibrariesMatches.Groups[i].Value}");
                        uplayLibrariesPaths.Add(uplayLibrariesMatches.Groups[i].Value);
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
                        Regex appidRegex = new Regex(@"""appid""\s+""(\d+)"")", RegexOptions.IgnoreCase);
                        Match appidMatches = appidRegex.Match(uplayLibraryAppManifestText);
                        if (appidMatches.Success)
                        {

                            uint uplayGameId = 0;
                            string uplayGameName = String.Empty;
                            if (uint.TryParse(appidMatches.Groups[1].Value, out uplayGameId))
                            {
                                // Check if this game is one that was installed
                                if (uplayAppIdsInstalled.Contains(uplayGameId))
                                {
                                    // This game is an installed game! so we start to populate it with data!
                                    // Grab the Uplay game name from the app manifeest file 
                                    Regex nameRegex = new Regex(@"""name""\s+""([^""])"")", RegexOptions.IgnoreCase);
                                    Match nameMatches = nameRegex.Match(uplayLibraryAppManifestText);
                                    if (nameMatches.Success)
                                    {
                                        uplayGameName = nameMatches.Groups[1].Value;

                                        // We need to also get the installdir from the app manifeest file 
                                        Regex installDirRegex = new Regex(@"""installdir""\s+""([^""])"")", RegexOptions.IgnoreCase);
                                        Match installDirMatches = installDirRegex.Match(uplayLibraryAppManifestText);
                                        if (installDirMatches.Success)
                                        {
                                            // Construct the full path to the game dir
                                            string uplayGameInstallDir = Path.Combine(uplayLibraryPath, @"uplayapps", @"common", installDirMatches.Groups[1].Value);
                                            // Get the names of the *.config within the gamesdir. There is one per game, and it is the main game exe
                                            // This is the one we want to get the icon from.
                                            string[] uplayGameConfigs = Directory.GetFiles(uplayGameInstallDir, "*.config");
                                            // Pick the first one, and use that (as there should only be one). Derive the exe name from it
                                            //string uplayGameExe = Path.Combine(uplayGameInstallDir, uplayGameConfigs[0].Remove(uplayGameConfigs[0].LastIndexOf(".config")));
                                            string uplayGameExe = Path.Combine(uplayGameInstallDir, Path.GetFileNameWithoutExtension(uplayGameConfigs[0]));
                                            // Now we need to get the Icon
                                            Icon uplayGameIcon = Icon.ExtractAssociatedIcon(uplayGameExe);
                                            uplayGameList.Add(new UplayGame(uplayGameId, uplayGameName, uplayGameInstallDir, uplayGameExe, uplayGameIcon));
                                        }

                                    }
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

        /*      public static string GetAppName(uint appId)
              {
                  return GetAllGames()?.FirstOrDefault(g => g.AppId == appId)?.Name;
              }*/

        /*    private static void CacheGameIds(IEnumerable<GameLibraryAppDetails> gameIds)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(gameIds, Formatting.Indented);

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var dir = Path.GetDirectoryName(GameIdCacheFilePath);

                        if (dir != null)
                        {
                            Directory.CreateDirectory(dir);
                            File.WriteAllText(GameIdCacheFilePath, json, Encoding.Unicode);
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }*/
        /*
                private static GameLibraryAppDetails[] GetCachedGameIds()
                {
                    try
                    {
                        if (File.Exists(GameIdCacheFilePath))
                        {
                            var json = File.ReadAllText(GameIdCacheFilePath, Encoding.Unicode);

                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                return JsonConvert.DeserializeObject<GameLibraryAppDetails[]>(json);
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    return null;
                }*/

        /* private static Thread UpdateGamesFromWeb()
         {
             if (_allGamesUpdated)
             {
                 return null;
             }

             _allGamesUpdated = true;
             var thread = new Thread(() =>
             {
                 try
                 {
                     var newGames = new List<GameLibraryAppDetails>();

                     using (var webClient = new WebClient())
                     {
                         webClient.Headers.Add(@"User-Agent",
                             @"Mozilla/5.0 (Windows NT 10.0; WOW64; rv:52.0) Gecko/20100101 Firefox/52.0");
                         webClient.Headers.Add(@"Accept", @"text/html,application/xhtml+xml,application/xml;");
                         var response = webClient.OpenRead(@"https://uplaydb.info/api/GetAppList/");

                         if (response != null)
                         {
                             using (response)
                             {
                                 using (var reader = new StreamReader(response))
                                 {
                                     var content = reader.ReadToEnd();

                                     if (!string.IsNullOrWhiteSpace(content))
                                     {
                                         dynamic appids = JsonConvert.DeserializeObject(content);

                                         if (appids != null && appids.success == true)
                                         {
                                             foreach (var app in appids.data)
                                             {
                                                 try
                                                 {
                                                     newGames.Add(new GameLibraryAppDetails(SupportedGameLibrary.Uplay, uint.Parse(app.Name), app.Value.Value));
                                                 }
                                                 catch
                                                 {
                                                     // ignored
                                                 }
                                             }
                                         }
                                     }

                                     reader.Close();
                                 }

                                 response.Close();
                             }
                         }
                     }

                     *//*                    if (newGames.Count > 0)
                                         {
                                             lock (AllGamesLock)
                                             {
                                                 _allGames = newGames;
                                                 CacheGameIds(_allGames);
                                             }
                                         }*//*
                 }
                 catch
                 {
                     // ignored
                 }
             });
             thread.Start();

             return thread;
         }*/

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

        /* public Task<string> DownloadIcon()
         {
             return Task.Run(() =>
             {
                 if (!Directory.Exists(IconCachePath))
                 {
                     try
                     {
                         Directory.CreateDirectory(IconCachePath);
                     }
                     catch
                     {
                         return null;
                     }
                 }

                 var localPath = Path.Combine(IconCachePath, GameId + ".ico");

                 if (File.Exists(localPath))
                 {
                     return localPath;
                 }

                 var iconUrl = new HtmlWeb().Load("https://uplaydb.info/app/" + GameId)
                     .DocumentNode.SelectNodes("//a[@href]")
                     .Select(node => node.Attributes["href"].Value)
                     .FirstOrDefault(attribute => attribute.EndsWith(".ico") && attribute.Contains("/" + GameId + "/"));

                 if (!string.IsNullOrWhiteSpace(iconUrl))
                 {
                     try
                     {
                         using (var client = new WebClient())
                         {
                             client.DownloadFile(iconUrl, localPath);
                         }
                     }
                     catch
                     {
                         return null;
                     }
                 }

                 return File.Exists(localPath) ? localPath : null;
             });
         }*/
    }
}