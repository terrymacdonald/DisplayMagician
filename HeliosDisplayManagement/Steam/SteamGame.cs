using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using HeliosDisplayManagement.Resources;
using HtmlAgilityPack;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace HeliosDisplayManagement.Steam
{
    public class SteamGame
    {
        private static List<SteamAppIdNamePair> _allGames;
        private static bool _allGamesUpdated;
        private static readonly object AllGamesLock = new object();
        private string _name;

        static SteamGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }

        public SteamGame(uint appId)
        {
            AppId = appId;
        }

        public uint AppId { get; }

        public static string GameIdsPath
            =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Assembly.GetExecutingAssembly().GetName().Name, @"SteamGames.xml");

        public static string IconCache
            =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Assembly.GetExecutingAssembly().GetName().Name, @"SteamIconCache");

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

        public string Name => _name ?? (_name = GetAppName(AppId));

        private string RegistryApp => $@"{RegistryApps}\\{AppId}";
        private static string RegistryApps => $@"{RegistrySteam}\\Apps";
        private static string RegistrySteam => @"SOFTWARE\\Valve\\Steam";

        public static string SteamAddress
        {
            get
            {
                using (
                    var key = Registry.CurrentUser.OpenSubKey(RegistrySteam, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    return (string) key?.GetValue(@"SteamExe", string.Empty) ?? string.Empty;
                }
            }
        }

        public static bool SteamInstalled => !string.IsNullOrWhiteSpace(SteamAddress);

        public static List<SteamAppIdNamePair> GetAllGames()
        {
            lock (AllGamesLock)
            {
                if (_allGames == null)
                    _allGames = GetCachedGameIds()?.ToList();
            }
            // Update only once
            if (!_allGamesUpdated)
                if (_allGames?.Count > 0)
                    UpdateGamesFromWeb();
                else
                    UpdateGamesFromWeb()?.Join();
            return _allGames;
        }

        public static SteamGame[] GetAllOwnedGames()
        {
            var list = new List<SteamGame>();
            try
            {
                using (
                    var subKey = Registry.CurrentUser.OpenSubKey(RegistryApps, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (subKey != null)
                        foreach (var keyName in subKey.GetSubKeyNames())
                        {
                            uint gameId;
                            if (uint.TryParse(keyName, out gameId))
                                list.Add(new SteamGame(gameId));
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

        private static void CacheGameIds(IEnumerable<SteamAppIdNamePair> gameIds)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(SteamAppIdNamePair[]));
                var sb = new StringBuilder();
                using (var writer = XmlWriter.Create(sb))
                {
                    serializer.Serialize(writer, gameIds.ToArray());
                }
                var xml = sb.ToString();
                try
                {
                    var doc = XDocument.Parse(xml);
                    xml = doc.ToString();
                }
                catch
                {
                    // ignored
                }
                if (!string.IsNullOrWhiteSpace(xml))
                {
                    var dir = Path.GetDirectoryName(GameIdsPath);
                    if (dir != null)
                    {
                        Directory.CreateDirectory(dir);
                        File.WriteAllText(GameIdsPath, xml, Encoding.Unicode);
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        private static SteamAppIdNamePair[] GetCachedGameIds()
        {
            try
            {
                if (File.Exists(GameIdsPath))
                {
                    var xml = File.ReadAllText(GameIdsPath, Encoding.Unicode);
                    if (!string.IsNullOrWhiteSpace(xml))
                    {
                        var serializer = new XmlSerializer(typeof(SteamAppIdNamePair[]));
                        using (var reader = XmlReader.Create(new StringReader(xml)))
                        {
                            return (SteamAppIdNamePair[]) serializer.Deserialize(reader);
                        }
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
                return null;
            _allGamesUpdated = true;
            var thread = new Thread(() =>
            {
                try
                {
                    var newGames = new List<SteamAppIdNamePair>();
                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add(@"User-Agent",
                            @"Mozilla/5.0 (Windows NT 10.0; WOW64; rv:52.0) Gecko/20100101 Firefox/52.0");
                        webClient.Headers.Add(@"Accept", @"text/html,application/xhtml+xml,application/xml;");
                        var response = webClient.OpenRead(@"https://steamdb.info/api/GetAppList/");
                        if (response != null)
                            using (response)
                            {
                                using (var reader = new StreamReader(response))
                                {
                                    var content = reader.ReadToEnd();
                                    if (!string.IsNullOrWhiteSpace(content))
                                    {
                                        dynamic appids = JsonConvert.DeserializeObject(content);
                                        if ((appids != null) && (appids.success == true))
                                            foreach (var app in appids.data)
                                                try
                                                {
                                                    newGames.Add(new SteamAppIdNamePair(uint.Parse(app.Name),
                                                        app.Value.Value));
                                                }
                                                catch
                                                {
                                                    // ignored
                                                }
                                    }
                                    reader.Close();
                                }
                                response.Close();
                            }
                    }
                    if (newGames.Count > 0)
                        lock (AllGamesLock)
                        {
                            _allGames = newGames;
                            CacheGameIds(_allGames);
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
                name = Language.Unknown;
            if (IsRunning)
                return name + " " + Language.Running;
            if (IsUpdating)
                return name + " " + Language.Updating;
            if (IsInstalled)
                return name + " " + Language.Installed;
            if (IsOwned)
                return name + " " + Language.Not_Installed;
            return name + " " + Language.Not_Owned;
        }

        public Task<string> GetIcon()
        {
            return Task.Run(() =>
            {
                if (!Directory.Exists(IconCache))
                    try
                    {
                        Directory.CreateDirectory(IconCache);
                    }
                    catch
                    {
                        return null;
                    }
                var localPath = Path.Combine(IconCache, AppId + ".ico");
                if (File.Exists(localPath))
                    return localPath;
                var iconUrl = new HtmlWeb().Load("https://steamdb.info/app/" + AppId)
                    .DocumentNode.SelectNodes("//a[@href]")
                    .Select(node => node.Attributes["href"].Value)
                    .FirstOrDefault(attribute => attribute.EndsWith(".ico") && attribute.Contains("/" + AppId + "/"));
                if (!string.IsNullOrWhiteSpace(iconUrl))
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
                return File.Exists(localPath) ? localPath : null;
            });
        }
    }
}