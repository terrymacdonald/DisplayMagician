using DisplayMagicianShared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DisplayMagician
{
    public struct DonationSettingsFile
    {
        public string DonationSettingsFileVersion;
        public DateTime LastUpdated;
        public DonationSettings DonationSettings;
    }

    public class DonationSettings
    {
        public const string CurrentDonationSettingsFileVersion = "1";
        public const string DonationSettingsStorageJsonFileName = "Donation.json";
        public static string DonationSettingsStorageJsonFullFileName = Path.Combine(Program.AppDataPath, DonationSettingsStorageJsonFileName);

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private DateTime _lastDonationDate = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private DateTime _lastDonateButtonAnimationDate = DateTime.UtcNow;
        private DateTime _lastDonationFormDate = DateTime.UtcNow;
        private int _numberOfDonations = 0;
        private int _numberOfStartsSinceLastDonationButtonAnimation = 0;
        private int _numberOfStartsSinceLastDonationForm = 0;
        private int _numberOfTimesRun = 0;

        public DateTime LastDonationDate
        {
            get
            {
                return _lastDonationDate;
            }
            set
            {
                _lastDonationDate = value;
            }
        }

        public DateTime LastDonateButtonAnimationDate
        {
            get
            {
                return _lastDonateButtonAnimationDate;
            }
            set
            {
                _lastDonateButtonAnimationDate = value;
            }
        }

        [DefaultValue(0)]
        public int NumberOfStartsSinceLastDonationButtonAnimation
        {
            get
            {
                return _numberOfStartsSinceLastDonationButtonAnimation;
            }
            set
            {
                _numberOfStartsSinceLastDonationButtonAnimation = value;
            }
        }

        public DateTime LastDonationFormDate
        {
            get
            {
                return _lastDonationFormDate;
            }
            set
            {
                _lastDonationFormDate = value;
            }
        }

        [DefaultValue(0)]
        public int NumberOfStartsSinceLastDonationForm
        {
            get
            {
                return _numberOfStartsSinceLastDonationForm;
            }
            set
            {
                _numberOfStartsSinceLastDonationForm = value;
            }
        }

        [DefaultValue(0)]
        public int NumberOfTimesRun
        {
            get
            {
                return _numberOfTimesRun;
            }
            set
            {
                _numberOfTimesRun = value;
            }
        }

        [DefaultValue(0)]
        public int NumberOfDonations
        {
            get
            {
                return _numberOfDonations;
            }
            set
            {
                _numberOfDonations = value;
            }
        }

        public static DonationSettings LoadSettings()
        {
            DonationSettings donationSettings = null;

            if (File.Exists(DonationSettingsStorageJsonFullFileName))
            {
                string json = "";
                List<string> jsonErrors = new List<string>();

                try
                {
                    json = File.ReadAllText(DonationSettingsStorageJsonFullFileName, Encoding.Unicode);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"DonationSettings/LoadSettings: Tried to read the JSON file {DonationSettingsStorageJsonFullFileName} to memory from disk but File.ReadAllText threw an exception.");
                }

                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        JsonSerializerSettings serializerSettings = new JsonSerializerSettings
                        {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Populate,
                            TypeNameHandling = TypeNameHandling.Auto,
                            ObjectCreationHandling = ObjectCreationHandling.Replace,
                            Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                            {
                                jsonErrors.Add($"JSON.net Error: {args.ErrorContext.Error.Source}:{args.ErrorContext.Error.StackTrace} - {args.ErrorContext.Error.Message} | InnerException:{args.ErrorContext.Error.InnerException?.Source}:{args.ErrorContext.Error.InnerException?.StackTrace} - {args.ErrorContext.Error.InnerException?.Message}");
                                args.ErrorContext.Handled = true;
                            },
                        };

                        DonationSettingsFile donationSettingsFile = JsonConvert.DeserializeObject<DonationSettingsFile>(json, serializerSettings);
                        donationSettings = donationSettingsFile.DonationSettings;
                    }
                    catch (JsonReaderException ex)
                    {
                        SharedLogger.logger.Error(ex, $"DonationSettings/LoadSettings: JSONReaderException while trying to process the Donation Settings file {DonationSettingsStorageJsonFullFileName}.");
                        MessageBox.Show($"The Donation Settings file {DonationSettingsStorageJsonFullFileName} contains a syntax error. Please check the file for correctness with a JSON validator.", "Error loading the Donation Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"DonationSettings/LoadSettings: Tried to parse the JSON in the Donation Settings file {DonationSettingsStorageJsonFullFileName} but JsonConvert threw an exception.");
                        MessageBox.Show($"The Donation Settings file {DonationSettingsStorageJsonFullFileName} contains a syntax error. Please check the file for correctness with a JSON validator.", "Error loading the Donation Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    foreach (string jsonError in jsonErrors)
                    {
                        SharedLogger.logger.Error($"DonationSettings/LoadSettings: {jsonError}");
                    }
                }
            }

            if (donationSettings == null)
            {
                donationSettings = new DonationSettings();
                donationSettings.SaveSettings();
            }

            return donationSettings;
        }

        public bool SaveSettings()
        {
            logger.Debug($"DonationSettings/SaveSettings: Attempting to save the donation settings to {DonationSettingsStorageJsonFullFileName}.");

            List<string> jsonErrors = new List<string>();
            try
            {
                JsonSerializerSettings serializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    TypeNameHandling = TypeNameHandling.Auto,
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                    {
                        jsonErrors.Add($"JSON.net Error: {args.ErrorContext.Error.Source}:{args.ErrorContext.Error.StackTrace} - {args.ErrorContext.Error.Message} | InnerException:{args.ErrorContext.Error.InnerException?.Source}:{args.ErrorContext.Error.InnerException?.StackTrace} - {args.ErrorContext.Error.InnerException?.Message}");
                        args.ErrorContext.Handled = true;
                    },
                };

                DonationSettingsFile donationSettingsFile = new DonationSettingsFile
                {
                    DonationSettingsFileVersion = CurrentDonationSettingsFileVersion,
                    LastUpdated = DateTime.UtcNow,
                    DonationSettings = this
                };

                string json = JsonConvert.SerializeObject(donationSettingsFile, Formatting.Indented, serializerSettings);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    File.WriteAllText(DonationSettingsStorageJsonFullFileName, json, Encoding.Unicode);
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"DonationSettings/SaveSettings: Exception attempting to save the donation settings to {DonationSettingsStorageJsonFullFileName}.");
            }

            foreach (string jsonError in jsonErrors)
            {
                logger.Error($"DonationSettings/SaveSettings: {jsonError}");
            }

            return false;
        }
    }
}
