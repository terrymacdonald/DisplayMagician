using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagician
{
    public class ProgramSettings
    {
        #region Class Variables
        // Common items to the class
        private static bool _programSettingsLoaded = false;
        // Other constants that are useful
        private static string _programSettingsStorageJsonFileName = Path.Combine(Program.AppDataPath, $"Settings_{FileVersion.ToString(2)}.json");
        #endregion

        #region Instance Variables
        private bool _minimiseOnStart = false;
        #endregion

        #region Class Properties
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

        public static Version FileVersion
        {
            get => new Version(1, 0, 0);
        }

        #endregion

        #region Class Methods
        public static ProgramSettings LoadSettings()
        {
            ProgramSettings programSettings = null;

            if (File.Exists(_programSettingsStorageJsonFileName))
            {
                var json = File.ReadAllText(_programSettingsStorageJsonFileName, Encoding.Unicode);

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
                    catch (FileNotFoundException ex)
                    {
                        Console.WriteLine($"ProgramSettings/LoadSettings exception 1: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unable to load Program Settings JSON file {_programSettingsStorageJsonFileName}: " + ex.Message);
                    }
                }
            }

            // If there isn't any settings in the file then create a new ProgramSettings object
            if (programSettings == null)
                programSettings = new ProgramSettings();
            _programSettingsLoaded = true;
            return programSettings ;
        }

        public bool SaveSettings()
        {

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
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"ProgramSettings/SaveSettings exception 1: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to save Program Settings JSON file {_programSettingsStorageJsonFileName}: " + ex.Message);
            }

            return false;
        }

        #endregion
    }


}
