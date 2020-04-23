using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HeliosPlus.Resources;
using HtmlAgilityPack;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace HeliosPlus.Uplay
{
    public class UplayGame
    {
        private static List<UplayAppIdNamePair> _allGames;
        private static bool _allGamesUpdated;
        private static readonly object AllGamesLock = new object();
        private string _name;

        static UplayGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }

        public UplayGame(uint appId)
        {
            AppId = appId;
        }

        public uint AppId { get; }

        public static string GameIdsPath
        {
            get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Assembly.GetExecutingAssembly().GetName().Name, @"SteamGames.json");
        }

        public static string IconCache
        {
            get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Assembly.GetExecutingAssembly().GetName().Name, @"SteamIconCache");
        }

        public bool IsInstalled
        {
            get
            {
                try
                {
                    using (
                        var key = Registry.CurrentUser.OpenSubKey(RegistryApp, RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        return (int) (key?.GetValue(@"Installed", 0) ?? 0) > 0;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsOwned
        {
            get
            {
                try
                {
                    using (
                        var key = Registry.CurrentUser.OpenSubKey(RegistryApp, RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        return key != null;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsRunning
        {
            get
            {
                try
                {
                    using (
                        var key = Registry.CurrentUser.OpenSubKey(RegistryApp, RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        return (int) (key?.GetValue(@"Running", 0) ?? 0) > 0;
                    }
                }
                catch
                {
                    return false;
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
                        var key = Registry.CurrentUser.OpenSubKey(RegistryApp, RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        return (int) (key?.GetValue(@"Updating", 0) ?? 0) > 0;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public string Name
        {
            get => _name ?? (_name = GetAppName(AppId));
        }

        private string RegistryApp
        {
            get => $@"{RegistryApps}\\{AppId}";
        }

        private static string RegistryApps
        {
            get => $@"{RegistryUplay}\\Apps";
        }

        private static string RegistryUplay
        {
            get => @"SOFTWARE\\Valve\\Steam";
        }

        public static string UplayAddress
        {
            get
            {
                using (
                    var key = Registry.CurrentUser.OpenSubKey(RegistryUplay, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    return (string) key?.GetValue(@"SteamExe", string.Empty) ?? string.Empty;
                }
            }
        }

        public static bool UplayInstalled
        {
            get => !string.IsNullOrWhiteSpace(UplayAddress);
        }

        public static List<UplayAppIdNamePair> GetAllGames()
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
        }

        public static UplayGame[] GetAllOwnedGames()
        {
            var list = new List<UplayGame>();

            try
            {
                using (
                    var subKey = Registry.CurrentUser.OpenSubKey(RegistryApps, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (subKey != null)
                    {
                        foreach (var keyName in subKey.GetSubKeyNames())
                        {
                            uint gameId;

                            if (uint.TryParse(keyName, out gameId))
                            {
                                list.Add(new UplayGame(gameId));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return list.ToArray();
        }

        public static string GetAppName(uint appId)
        {
            return GetAllGames()?.FirstOrDefault(g => g.AppId == appId)?.Name;
        }

        private static void CacheGameIds(IEnumerable<UplayAppIdNamePair> gameIds)
        {
            try
            {
                var json = JsonConvert.SerializeObject(gameIds, Formatting.Indented);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    var dir = Path.GetDirectoryName(GameIdsPath);

                    if (dir != null)
                    {
                        Directory.CreateDirectory(dir);
                        File.WriteAllText(GameIdsPath, json, Encoding.Unicode);
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        private static UplayAppIdNamePair[] GetCachedGameIds()
        {
            try
            {
                if (File.Exists(GameIdsPath))
                {
                    var json = File.ReadAllText(GameIdsPath, Encoding.Unicode);

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        return JsonConvert.DeserializeObject<UplayAppIdNamePair[]>(json);
                    }
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

        private static Thread UpdateGamesFromWeb()
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
                    var newGames = new List<UplayAppIdNamePair>();

                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add(@"User-Agent",
                            @"Mozilla/5.0 (Windows NT 10.0; WOW64; rv:52.0) Gecko/20100101 Firefox/52.0");
                        webClient.Headers.Add(@"Accept", @"text/html,application/xhtml+xml,application/xml;");
                        var response = webClient.OpenRead(@"https://steamdb.info/api/GetAppList/");

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
                                                    newGames.Add(new UplayAppIdNamePair(uint.Parse(app.Name),
                                                        app.Value.Value));
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

                    if (newGames.Count > 0)
                    {
                        lock (AllGamesLock)
                        {
                            _allGames = newGames;
                            CacheGameIds(_allGames);
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            });
            thread.Start();

            return thread;
        }

        public override string ToString()
        {
            var name = Name;

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

            if (IsInstalled)
            {
                return name + " " + Language.Installed;
            }

            if (IsOwned)
            {
                return name + " " + Language.Not_Installed;
            }

            return name + " " + Language.Not_Owned;
        }

        public Task<string> GetIcon()
        {
            return Task.Run(() =>
            {
                if (!Directory.Exists(IconCache))
                {
                    try
                    {
                        Directory.CreateDirectory(IconCache);
                    }
                    catch
                    {
                        return null;
                    }
                }

                var localPath = Path.Combine(IconCache, AppId + ".ico");

                if (File.Exists(localPath))
                {
                    return localPath;
                }

                var iconUrl = new HtmlWeb().Load("https://steamdb.info/app/" + AppId)
                    .DocumentNode.SelectNodes("//a[@href]")
                    .Select(node => node.Attributes["href"].Value)
                    .FirstOrDefault(attribute => attribute.EndsWith(".ico") && attribute.Contains("/" + AppId + "/"));

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
        }
    }
}