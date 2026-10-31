using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public static void ConfigureStoragePath(string applicationDataPath)
        {
            if (string.IsNullOrWhiteSpace(applicationDataPath))
                throw new ArgumentException("An application data path is required.", nameof(applicationDataPath));

            DonationSettingsStorageJsonFullFileName = Path.Combine(Path.GetFullPath(applicationDataPath), DonationSettingsStorageJsonFileName);
        }

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalPreferences { get; set; }

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
            return Program.AppProgramSettings?.Donation ?? new DonationSettings();
        }

        public bool SaveSettings()
        {
            ProgramSettings owner = Program.AppProgramSettings;
            if (owner == null)
            {
                logger.Error("DonationSettings/SaveSettings: The primary program settings have not been loaded.");
                return false;
            }
            lock (owner.PersistenceLock)
            {
                owner.Donation = this;
                return owner.SaveSettings();
            }
        }
    }
}
