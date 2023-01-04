using DisplayMagicianShared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Windows.Data.Json;


namespace DisplayMagician
{

    public class ProgramSettings
    {
        #region Class Variables
        // Common items to the class
        private static bool _programSettingsLoaded = false;
        // Other constants that are useful
        public static string programSettingsStorageJsonFileName = Path.Combine(Program.AppDataPath, $"Settings_2.5.json");
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        #region Instance Variables
        private bool _startOnBootUp = true;
        private bool _minimiseOnStart = true;
        private bool _showSplashScreen = true;
        private bool _showMinimiseMessageInActionCenter = true;
        private bool _showStatusMessageInActionCenter = true;
        private bool _upgradeToPrereleases = false;
        private bool _upgradeEnabled = true;
        private bool _installedDesktopContextMenu = true;
        private int _lastMessageIdRead = 0;
        private List<int> _messagesToMonitor = new List<int>();
        private string _logLevel = NLog.LogLevel.Warn.ToString();
        private string _displayMagicianVersion = null;
        private Keys _hotkeyMainWindow = Keys.None;
        private Keys _hotkeyDisplayProfileWindow = Keys.None;
        private Keys _hotkeyShortcutLibraryWindow = Keys.None;
        private ScreenLayout _fovCalcScreenLayout = ScreenLayout.TripleScreen;
        private double _fovCalcScreenSize = 27;
        private ScreenMeasurementUnit _fovCalcScreenSizeUnit = ScreenMeasurementUnit.Inch;
        private ScreenAspectRatio _fovCalcAspectRatio = ScreenAspectRatio.SixteenByNine;
        private double _fovCalcAspectRatioX = 16;
        private double _fovCalcAspectRatioY = 9;
        private double _fovCalcDistanceToScreen = 56;
        private ScreenMeasurementUnit _fovCalcDistanceToScreenUnit = ScreenMeasurementUnit.CM; 
        private double _fovCalcBezelSize = 3;
        private ScreenMeasurementUnit _fovCalcBezelSizeUnit = ScreenMeasurementUnit.MM;
        #endregion

        #region Class Properties
        [DefaultValue(0)]
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

        [DefaultValue(false)]
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

        [DefaultValue(true)]
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

        [DefaultValue(true)]
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

        [DefaultValue(true)]
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

        [DefaultValue(false)]
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

        [DefaultValue(true)]
        public bool UpgradeEnabled
        {
            get
            {
                return _upgradeEnabled;
            }
            set
            {
                _upgradeEnabled = value;
            }
        }

        [DefaultValue(false)]
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

        [DefaultValue(true)]
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

        [DefaultValue(0)]
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

        [DefaultValue(default(List<int>))]
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

        [DefaultValue("Trace")]
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

        [DefaultValue(Keys.None)]
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

        [DefaultValue(Keys.None)]
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

        [DefaultValue(Keys.None)]
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

        [DefaultValue(ScreenLayout.TripleScreen)]
        public ScreenLayout FovCalcScreenLayout
        {
            get
            {
                return _fovCalcScreenLayout;
            }
            set
            {
                _fovCalcScreenLayout = value;
            }
        }

        [DefaultValue(27)]
        public double FovCalcScreenSize
        {
            get
            {
                return _fovCalcScreenSize;
            }
            set
            {
                _fovCalcScreenSize = value;
            }
        }

        [DefaultValue(ScreenMeasurementUnit.Inch)]
        public ScreenMeasurementUnit FovCalcScreenSizeUnit
        {
            get
            {
                return _fovCalcScreenSizeUnit;
            }
            set
            {
                _fovCalcScreenSizeUnit = value;
            }
        }

        [DefaultValue(ScreenAspectRatio.SixteenByNine)]
        public ScreenAspectRatio FovCalcAspectRatio
        {
            get
            {
                return _fovCalcAspectRatio;
            }
            set
            {
                _fovCalcAspectRatio = value;
            }
        }

        [DefaultValue(16)]
        public double FovCalcAspectRatioX
        {
            get
            {
                return _fovCalcAspectRatioX;
            }
            set
            {
                _fovCalcAspectRatioX = value;
            }
        }

        [DefaultValue(9)]
        public double FovCalcAspectRatioY
        {
            get
            {
                return _fovCalcAspectRatioY;
            }
            set
            {
                _fovCalcAspectRatioY = value;
            }
        }

        [DefaultValue(56)]
        public double FovCalcDistanceToScreen
        {
            get
            {
                return _fovCalcDistanceToScreen;
            }
            set
            {
                _fovCalcDistanceToScreen = value;
            }
        }

        [DefaultValue(ScreenMeasurementUnit.CM)]
        public ScreenMeasurementUnit FovCalcDistanceToScreenUnit
        {
            get
            {
                return _fovCalcDistanceToScreenUnit;
            }
            set
            {
                _fovCalcDistanceToScreenUnit = value;
            }
        }

        [DefaultValue(3)]
        public double FovCalcBezelSize
        {
            get
            {
                return _fovCalcBezelSize;
            }
            set
            {
                _fovCalcBezelSize = value;
            }
        }

        [DefaultValue(ScreenMeasurementUnit.MM)]
        public ScreenMeasurementUnit FovCalcBezelSizeUnit
        {
            get
            {
                return _fovCalcBezelSizeUnit;
            }
            set
            {
                _fovCalcBezelSizeUnit = value;
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
                List<string> jsonErrors = new List<string>();
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
                            DefaultValueHandling = DefaultValueHandling.Populate,
                            TypeNameHandling = TypeNameHandling.Auto,
                            ObjectCreationHandling = ObjectCreationHandling.Replace,
                            Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
{
                                jsonErrors.Add($"JSON.net Error: {args.ErrorContext.Error.Source}:{args.ErrorContext.Error.StackTrace} - {args.ErrorContext.Error.Message} | InnerException:{args.ErrorContext.Error.InnerException.Source}:{args.ErrorContext.Error.InnerException.StackTrace} - {args.ErrorContext.Error.InnerException.Message}");
                                args.ErrorContext.Handled = true;
                            },
                        });
                    }
                    catch (JsonReaderException ex)
                    {
                        // If there is a error in the JSON format
                        if (ex.HResult == -2146233088)
                        {
                            SharedLogger.logger.Error(ex, $"ProgramSettings/LoadSettings: JSONReaderException - The Program Settings file {programSettingsStorageJsonFileName} contains a syntax error. Please check the file for correctness with a JSON validator.");
                        }
                        else
                        {
                            SharedLogger.logger.Error(ex, $"ProgramSettings/LoadSettings: JSONReaderException while trying to process the Program Settings file {programSettingsStorageJsonFileName} but JsonConvert threw an exception.");
                        }
                        MessageBox.Show($"The Program Settings file {programSettingsStorageJsonFileName} contains a syntax error. Please check the file for correctness with a JSON validator.", "Error loading the Program Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"ProgramSettings/LoadSettings: Tried to parse the JSON in the Program Settings file {programSettingsStorageJsonFileName} but the JsonConvert threw an exception.");
                        MessageBox.Show($"The Program Settings file {programSettingsStorageJsonFileName} contains a syntax error. Please check the file for correctness with a JSON validator.", "Error loading the Program Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    // If we have any JSON.net errors, then we need to records them in the logs
                    if (jsonErrors.Count > 0)
                    {
                        foreach (string jsonError in jsonErrors)
                        {
                            SharedLogger.logger.Error($"ProgramSettings/LoadSettings: {jsonErrors}");
                        }
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

            List<string> jsonErrors = new List<string>();
            try
            {
                
                var json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    TypeNameHandling = TypeNameHandling.Auto,
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                    {
                        jsonErrors.Add($"JSON.net Error: {args.ErrorContext.Error.Source}:{args.ErrorContext.Error.StackTrace} - {args.ErrorContext.Error.Message} | InnerException:{args.ErrorContext.Error.InnerException.Source}:{args.ErrorContext.Error.InnerException.StackTrace} - {args.ErrorContext.Error.InnerException.Message}");
                        args.ErrorContext.Handled = true;
                    },
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

            // If we have any JSON.net errors, then we need to records them in the logs
            if (jsonErrors.Count > 0)
            {
                foreach (string jsonError in jsonErrors)
                {
                    logger.Error($"ProgramSettings/SaveSettings: {jsonErrors}");
                }
            }

            return false;
        }

        #endregion
    }


}
