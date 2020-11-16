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
//using HtmlAgilityPack;
using Microsoft.Win32;
using Newtonsoft.Json;
//using VdfParser;
//using Gameloop.Vdf;
using System.Collections.ObjectModel;
using ValveKeyValue;
using System.Security.Cryptography;
using System.ServiceModel.Configuration;
//using HeliosPlus.GameLibraries.UplayAppInfoParser;
using TsudaKageyu;
using System.Drawing.IconLib;
using System.Drawing.IconLib.Exceptions;
using System.Diagnostics;

namespace HeliosPlus.GameLibraries
{
    public class UplayGame : Game
    {
        /*private static string UplayLibrary.UplayExe;
        private static string UplayLibrary.UplayPath;
        private static string _uplayConfigVdfFile;
        private static string _registryUplayKey = @"SOFTWARE\\Valve\\Uplay";
        private static string _registryAppsKey = $@"{_registryUplayKey}\\Apps";*/
        private string _gameRegistryKey;
        private uint _uplayGameId;
        private string _uplayGameName;
        private string _uplayGameExePath;
        private string _uplayGameDir;
        private string _uplayGameExe;
        private string _uplayGameProcessName;
        private string _uplayGameIconPath;
        private static List<UplayGame> _allInstalledUplayGames = null;

        /*private struct UplayAppInfo
        {
            public uint GameID; 
            public string GameName;
            public List<string> GameExes;
            public string GameInstallDir;
            public string GameUplayIconPath;
        }*/

        static UplayGame()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (send, certificate, chain, sslPolicyErrors) => true;
        }


        public UplayGame(uint uplayGameId, string uplayGameName, string uplayGameExePath, string uplayGameIconPath)
        {

            _gameRegistryKey = $@"{UplayLibrary.registryUplayInstallsKey}\\{uplayGameId}";
            _uplayGameId = uplayGameId;
            _uplayGameName = uplayGameName;
            _uplayGameExePath = uplayGameExePath;
            _uplayGameDir = Path.GetDirectoryName(uplayGameExePath);
            _uplayGameExe = Path.GetFileName(_uplayGameExePath);
            _uplayGameProcessName = Path.GetFileNameWithoutExtension(_uplayGameExePath);
            _uplayGameIconPath = uplayGameIconPath;

        }

        public override uint Id
        {
            get => _uplayGameId;
            set => _uplayGameId = value;
        }

        public override string Name
        {
            get => _uplayGameName;
            set => _uplayGameName = value;
        }

        public override SupportedGameLibrary GameLibrary
        {
            get => SupportedGameLibrary.Uplay;
        }

        public override string IconPath
        {
            get => _uplayGameIconPath;
            set => _uplayGameIconPath = value;
        }

        public override string ExePath
        {
            get => _uplayGameExePath;
            set => _uplayGameExePath = value;
        }

        public override string Directory
        {
            get => _uplayGameExePath;
            set => _uplayGameExePath = value;
        }

        public override bool IsRunning
        {
            get
            {
                /*try
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
                    Console.WriteLine($"UplayGame/IsRunning securityexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                    if (ex.Source != null)
                        Console.WriteLine("SecurityException source: {0} - Message: {1}", ex.Source, ex.Message);
                    throw;
                }
                catch (IOException ex)
                {
                    // Extract some information from this exception, and then
                    // throw it to the parent method.
                    Console.WriteLine($"UplayGame/IsRunning ioexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                    if (ex.Source != null)
                        Console.WriteLine("IOException source: {0} - Message: {1}", ex.Source, ex.Message);
                    throw;
                }*/

                bool isRunning = Process.GetProcessesByName(_uplayGameProcessName)
                    .FirstOrDefault(p => p.MainModule.FileName
                    .StartsWith(ExePath, StringComparison.OrdinalIgnoreCase)) != default(Process);
                return isRunning;
            }
        }

        /*public override bool IsUpdating
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
                    Console.WriteLine($"UplayGame/IsUpdating securityexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                    if (ex.Source != null)
                        Console.WriteLine("SecurityException source: {0} - Message: {1}", ex.Source, ex.Message);
                    throw;
                }
                catch (IOException ex)
                {
                    // Extract some information from this exception, and then
                    // throw it to the parent method.
                    Console.WriteLine($"UplayGame/IsUpdating ioexception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                    if (ex.Source != null)
                        Console.WriteLine("IOException source: {0} - Message: {1}", ex.Source, ex.Message);
                    throw;
                }
            }
        }*/

        public bool CopyTo(UplayGame uplayGame)
        {
            if (!(uplayGame is UplayGame))
                return false;

            // Copy all the game data over to the other game
            uplayGame.IconPath = IconPath;
            uplayGame.Id = Id;
            uplayGame.Name = Name;
            uplayGame.ExePath = ExePath;
            return true;
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

            /*if (IsUpdating)
            {
                return name + " " + Language.Updating;
            }*/

            return name;
        }

    }
}