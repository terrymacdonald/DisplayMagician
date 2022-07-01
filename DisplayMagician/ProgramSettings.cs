using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;


namespace DisplayMagician
{

    public class ProgramSettings
    {
        #region Class Variables
        // Common items to the class
        private static bool _programSettingsLoaded = false;
        // Other constants that are useful
        public static string programSettingsStorageJsonFileName = Path.Combine(Program.AppDataPath, $"Settings_2.4.json");
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        #region Instance Variables
        private bool _startOnBootUp = true;
        private bool _minimiseOnStart = true;
        private bool _showSplashScreen = true;
        private bool _showMinimiseMessageInActionCenter = true;
        private bool _showStatusMessageInActionCenter = true;
        private bool _upgradeToPrereleases = false;
        private bool _installedDesktopContextMenu = true;
        private int _lastMessageIdRead = 0;
        private List<int> _messagesToMonitor = new List<int>();
        private string _logLevel = NLog.LogLevel.Trace.ToString();
        private string _displayMagicianVersion = null;
        private Keys _hotkeyMainWindow = Keys.None;
        private Keys _hotkeyDisplayProfileWindow = Keys.None;
        private Keys _hotkeyShortcutLibraryWindow = Keys.None;
        #endregion

        #region Class Properties
        public string DisplayMagicianVersion
        {
            get 
            {
                if (_displayMagicianVersion == null)
                {
                    return Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                return _displayMagicianVersion;
            }
            set
            {
                _displayMagicianVersion = value;
            }
        }
        public bool StartOnBootUp
        {
            get
            {
                return _startOnBootUp;
            }
            set
            {
                _startOnBootUp = value;
            }
        }

        public bool ShowSplashScreen
        {
            get
            {
                return _showSplashScreen;
            }
            set
            {
                _showSplashScreen = value;
            }
        }

        public bool ShowMinimiseMessageInActionCenter
        {
            get
            {
                return _showMinimiseMessageInActionCenter;
            }
            set
            {
                _showMinimiseMessageInActionCenter = value;
            }
        }

        public bool ShowStatusMessageInActionCenter
        {
            get
            {
                return _showStatusMessageInActionCenter;
            }
            set
            {
                _showStatusMessageInActionCenter = value;               
            }
        }

        public bool UpgradeToPreReleases
        {
            get
            {
                return _upgradeToPrereleases;
            }
            set
            {
                _upgradeToPrereleases = value;
            }
        }

        public bool MinimiseOnStart { 
            get
            {
                return _minimiseOnStart;
            }
            set 
            {
                _minimiseOnStart = value;
            } 
        }

        public bool InstalledDesktopContextMenu
        {
            get
            {
                return _installedDesktopContextMenu;
            }
            set
            {
                _installedDesktopContextMenu = value;
            }
        }

        public int LastMessageIdRead
        {
            get
            {
                return _lastMessageIdRead;
            }
            set
            {
                _lastMessageIdRead = value;
            }
        }

        public List<int> MessagesToMonitor
        {
            get
            {
                return _messagesToMonitor;
            }
            set
            {
                _messagesToMonitor = value;
            }
        }

        public string LogLevel
        {
            get
            {
                return _logLevel;
            }
            set
            {

                switch (value.ToLower())
                {
                    case "trace":
                        _logLevel = NLog.LogLevel.Trace.ToString();
                        break;
                    case "debug":
                        _logLevel = NLog.LogLevel.Debug.ToString();
                        break;
                    case "info":
                        _logLevel = NLog.LogLevel.Info.ToString();
                        break;
                    case "warn":
                        _logLevel = NLog.LogLevel.Warn.ToString();
                        break;
                    case "error":
                        _logLevel = NLog.LogLevel.Error.ToString();
                        break;
                    case "fatal":
                        _logLevel = NLog.LogLevel.Fatal.ToString();
                        break;
                    default:
                        _logLevel = NLog.LogLevel.Info.ToString();
                        break;

                }
            }
        }

        public Keys HotkeyMainWindow
        {
            get
            {
                return _hotkeyMainWindow;
            }
            set
            {
                _hotkeyMainWindow = value;
            }
        }

        public Keys HotkeyDisplayProfileWindow
        {
            get
            {
                return _hotkeyDisplayProfileWindow;
            }
            set
            {
                _hotkeyDisplayProfileWindow = value;
            }
        }

        public Keys HotkeyShortcutLibraryWindow
        {
            get
            {
                return _hotkeyShortcutLibraryWindow;
            }
            set
            {
                _hotkeyShortcutLibraryWindow = value;
            }
        }

        #endregion

        #region Class Methods

        ~ProgramSettings()
        {
            // Save the program settings on program exit
            SaveSettings();
        }

        public static ProgramSettings LoadSettings()
        {
            // NOTE: This function gets called before NLog has setup the logger, meaning
            // that we can't log to the log file yet. This is because we need to load the
            // loglevel settings so we know what level to configure the logger to write!
            // This means we have to only use console.write in this function....
            ProgramSettings programSettings = null;
            _programSettingsLoaded = false;

            if (File.Exists(programSettingsStorageJsonFileName))
            {
                string json = "";
                try {
                    json = File.ReadAllText(programSettingsStorageJsonFileName, Encoding.Unicode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ProgramSettings/LoadSettings: Tried to read the JSON file {programSettingsStorageJsonFileName} to memory from disk but File.ReadAllText threw an exception. {ex}");
                }

                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        programSettings = JsonConvert.DeserializeObject<ProgramSettings>(json, new JsonSerializerSettings
                        {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Include,
                            TypeNameHandling = TypeNameHandling.Auto
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ProgramSettings/LoadSettings: Tried to parse the JSON file {programSettingsStorageJsonFileName} but the JsonConvert threw an exception. {ex}");
                    }

                    if (programSettings.DisplayMagicianVersion == null) {
                        programSettings.DisplayMagicianVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    }
                }
            }
            else
            {
                Console.WriteLine($"ProgramSettings/LoadSettings: No ProgramSettings file found. Creating new one at {programSettingsStorageJsonFileName}");
                programSettings = new ProgramSettings();
                programSettings.SaveSettings();
            }

            // If there isn't any settings in the file then create a new ProgramSettings object
            if (programSettings == null)
                programSettings = new ProgramSettings();
            _programSettingsLoaded = true;

            return programSettings ;
        }

        public bool SaveSettings()
        {

            logger.Debug($"ProgramSettings/SaveSettings: Attempting to save the program settings to the {programSettingsStorageJsonFileName}.");

            // Force the PreviousDisplayMagicianVersion to this version just before we save the settings.
            DisplayMagicianVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            try
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    TypeNameHandling = TypeNameHandling.Auto

                });


                if (!string.IsNullOrWhiteSpace(json))
                {
                    File.WriteAllText(programSettingsStorageJsonFileName, json, Encoding.Unicode);
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"ProgramSettings/SaveSettings: Exception attempting to save the program settings to {programSettingsStorageJsonFileName}.");
            }

            return false;
        }

        #endregion
    }


}
