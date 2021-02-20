using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using NLog;

namespace DisplayMagician
{

    public class ProgramSettings
    {
        #region Class Variables
        // Common items to the class
        private static bool _programSettingsLoaded = false;
        // Other constants that are useful
        private static string _programSettingsStorageJsonFileName = Path.Combine(Program.AppDataPath, $"Settings_{FileVersion.ToString(2)}.json");
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        #region Instance Variables
        private bool _startOnBootUp = false;
        private bool _minimiseOnStart = false;
        private string _logLevel = NLog.LogLevel.Info.ToString();
        #endregion

        #region Class Properties
        public bool StartOnBootUp
        {
            get
            {
                return _startOnBootUp;
            }
            set
            {
                _startOnBootUp = value;

                // Because a value has changed, we need to save the setting 
                // to remember it for later.
                if (_programSettingsLoaded)
                    SaveSettings();
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

                // Because a value has changed, we need to save the setting 
                // to remember it for later.
                if (_programSettingsLoaded)
                    SaveSettings();
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

                // Because a value has changed, we need to save the setting 
                // to remember it for later.
                if (_programSettingsLoaded)
                    SaveSettings();
            }
        }

        public static Version FileVersion
        {
            get => new Version(1, 0, 0);
        }

        #endregion

        #region Class Methods
        public static ProgramSettings LoadSettings()
        {
            ProgramSettings programSettings = null;

            logger.Debug($"ProgramSettings/LoadSettings: Attempting to load ProgramSettings");

            if (File.Exists(_programSettingsStorageJsonFileName))
            {
                string json = "";
                try {
                    logger.Debug($"ProgramSettings/LoadSettings: Found existing ProgramSettings file at {_programSettingsStorageJsonFileName} so loading text from it");
                    json = File.ReadAllText(_programSettingsStorageJsonFileName, Encoding.Unicode);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ProgramSettings/LoadSettings: Tried to read the JSON file {_programSettingsStorageJsonFileName} to memory but File.ReadAllTextthrew an exception.");
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
                        logger.Error(ex,$"ProgramSettings/LoadSettings: Tried to parse the JSON in the {_programSettingsStorageJsonFileName} but the JsonConvert threw an exception.");
                    }
                }
            }
            else
            {
                logger.Info($"ProgramSettings/LoadSettings: No ProgramSettings file found. Creating new one at {_programSettingsStorageJsonFileName}");
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

            logger.Debug($"ProgramSettings/SaveSettings: Attempting to save the program settings to the {_programSettingsStorageJsonFileName}.");

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
                    File.WriteAllText(_programSettingsStorageJsonFileName, json, Encoding.Unicode);
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"ProgramSettings/SaveSettings: Exception attempting to save the program settings to {_programSettingsStorageJsonFileName}.");
            }

            return false;
        }

        #endregion
    }


}
