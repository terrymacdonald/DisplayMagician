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
        /*private static string SteamLibrary.SteamExe;
        private static string SteamLibrary.SteamPath;
        private static string _steamConfigVdfFile;
        private static string _registrySteamKey = @"SOFTWARE\\Valve\\Steam";
        private static string _registryAppsKey = $@"{_registrySteamKey}\\Apps";*/
        private string _gameRegistryKey;
        private uint _steamGameId;
        private string _steamGameName;
        private string _steamGamePath;
        private string _steamGameExe;
        private string _steamGameIconPath;
        private static List<SteamGame> _allInstalledSteamGames = null;

        /*private struct SteamAppInfo
        {
            public uint GameID; 
            public string GameName;
            public List<string> GameExes;
            public string GameInstallDir;
            public string GameSteamIconPath;
        }*/

        static SteamGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }


        public SteamGame(uint steamGameId, string steamGameName, string steamGamePath, string steamGameExe, string steamGameIconPath)
        {

            _gameRegistryKey = $@"{SteamLibrary.SteamAppsRegistryKey}\\{steamGameId}";
            _steamGameId = steamGameId;
            _steamGameName = steamGameName;
            _steamGamePath = steamGamePath;
            _steamGameExe = steamGameExe;
            _steamGameIconPath = steamGameIconPath;

        }

        public uint GameId { 
            get => _steamGameId;
            set => _steamGameId = value;
        }

        public SupportedGameLibrary GameLibrary { 
            get => SupportedGameLibrary.Steam; 
        }

        public string GameIconPath { 
            get => _steamGameIconPath; 
            set => _steamGameIconPath = value;
        }
                  
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
                    Console.WriteLine($"SteamGame/IsRunning securityexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                    if (ex.Source != null)
                        Console.WriteLine("SecurityException source: {0} - Message: {1}", ex.Source, ex.Message);
                    throw;
                }
                catch (IOException ex)
                {
                    // Extract some information from this exception, and then
                    // throw it to the parent method.
                    Console.WriteLine($"SteamGame/IsRunning ioexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
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

        public string GameName { 
            get => _steamGameName; 
            set => _steamGameName = value;
        }

        public string GamePath { 
            get => _steamGamePath;
            set => _steamGamePath = value;
        }

        public bool CopyTo(SteamGame steamGame)
        {
            if (!(steamGame is SteamGame))
                return false;

            // Copy all the game data over to the other game
            steamGame.GameIconPath = GameIconPath;
            steamGame.GameId = GameId;
            steamGame.GameName = GameName;
            steamGame.GamePath = GamePath;
            return true;
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