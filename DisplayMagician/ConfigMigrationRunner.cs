using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DisplayMagician
{
    internal static class ConfigMigrationRunner
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly List<IConfigMigrationRule> MigrationRules = new List<IConfigMigrationRule>
        {
            new SettingsV4ToV5DonationSplitMigration()
        };

        public static bool RunMigrations()
        {
            string settingsFileName = ProgramSettings.ProgramSettingsStorageJsonFullFileName;

            if (!File.Exists(settingsFileName))
            {
                logger.Trace($"ConfigMigrationRunner/RunMigrations: No {settingsFileName} file exists, so no settings migration is needed.");
                return true;
            }

            try
            {
                JObject settingsFile = ReadJsonObject(settingsFileName);
                MigrationContext context = new MigrationContext(settingsFileName, DonationSettings.DonationSettingsStorageJsonFullFileName, settingsFile);

                bool ranRule;
                do
                {
                    ranRule = false;
                    foreach (IConfigMigrationRule rule in MigrationRules)
                    {
                        if (rule.Applies(context))
                        {
                            logger.Info($"ConfigMigrationRunner/RunMigrations: Applying config migration rule {rule.Name}.");
                            if (!rule.Apply(context))
                            {
                                logger.Error($"ConfigMigrationRunner/RunMigrations: Config migration rule {rule.Name} failed.");
                                return false;
                            }

                            ranRule = true;
                            break;
                        }
                    }
                }
                while (ranRule);

                string currentVersion = context.GetSettingsFileVersion();
                if (!string.Equals(currentVersion, ProgramSettings.CurrentProgramSettingsFileVersion, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Error($"ConfigMigrationRunner/RunMigrations: Unsupported Settings.json file version '{currentVersion}'. Expected '{ProgramSettings.CurrentProgramSettingsFileVersion}'.");
                    return false;
                }

                if (context.GetSettingsObject() == null)
                {
                    logger.Error($"ConfigMigrationRunner/RunMigrations: Settings.json file version '{currentVersion}' did not contain a Settings object.");
                    return false;
                }

                if (context.SettingsFileChanged)
                {
                    WriteJsonObject(context.SettingsFileName, context.SettingsFile);
                }

                return true;
            }
            catch (JsonReaderException ex)
            {
                logger.Error(ex, $"ConfigMigrationRunner/RunMigrations: Settings migration failed because {settingsFileName} contains invalid JSON.");
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"ConfigMigrationRunner/RunMigrations: Settings migration failed unexpectedly while processing {settingsFileName}.");
                return false;
            }
        }

        private static JObject ReadJsonObject(string fileName)
        {
            string json = File.ReadAllText(fileName, Encoding.Unicode);
            return JObject.Parse(json);
        }

        private static void WriteJsonObject(string fileName, JObject jsonObject)
        {
            WriteTextSafely(fileName, jsonObject.ToString(Formatting.Indented));
        }

        private static void WriteTextSafely(string fileName, string contents)
        {
            string directory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempFileName = $"{fileName}.tmp";
            File.WriteAllText(tempFileName, contents, Encoding.Unicode);
            File.Copy(tempFileName, fileName, true);
            File.Delete(tempFileName);
        }

        private static string CreateBackup(string fileName, string migrationName)
        {
            string backupFileName = $"{fileName}.{migrationName}.bak";
            if (File.Exists(backupFileName))
            {
                backupFileName = $"{fileName}.{migrationName}.{DateTime.UtcNow:yyyyMMddHHmmss}.bak";
            }

            File.Copy(fileName, backupFileName);
            return backupFileName;
        }

        private interface IConfigMigrationRule
        {
            string Name { get; }
            bool Applies(MigrationContext context);
            bool Apply(MigrationContext context);
        }

        private sealed class MigrationContext
        {
            public MigrationContext(string settingsFileName, string donationSettingsFileName, JObject settingsFile)
            {
                SettingsFileName = settingsFileName;
                DonationSettingsFileName = donationSettingsFileName;
                SettingsFile = settingsFile;
            }

            public string SettingsFileName { get; }
            public string DonationSettingsFileName { get; }
            public JObject SettingsFile { get; }
            public bool SettingsFileChanged { get; private set; }

            public string GetSettingsFileVersion()
            {
                return SettingsFile["SettingsFileVersion"]?.ToString() ?? "";
            }

            public JObject GetSettingsObject()
            {
                return SettingsFile["Settings"] as JObject;
            }

            public void MarkSettingsFileChanged()
            {
                SettingsFileChanged = true;
            }
        }

        private sealed class SettingsV4ToV5DonationSplitMigration : IConfigMigrationRule
        {
            private const string MigrationName = "v4-to-v5";
            private static readonly string[] DonationFieldNames =
            {
                "LastDonationDate",
                "LastDonateButtonAnimationDate",
                "LastDonationFormDate",
                "NumberOfDonations",
                "NumberOfStartsSinceLastDonationButtonAnimation",
                "NumberOfStartsSinceLastDonationForm",
                "NumberOfTimesRun"
            };

            public string Name => "Settings v4 to v5 donation split";

            public bool Applies(MigrationContext context)
            {
                return string.Equals(context.GetSettingsFileVersion(), "4", StringComparison.OrdinalIgnoreCase);
            }

            public bool Apply(MigrationContext context)
            {
                JObject programSettings = context.GetSettingsObject();
                if (programSettings == null)
                {
                    logger.Error($"ConfigMigrationRunner/{nameof(SettingsV4ToV5DonationSplitMigration)}: Settings.json version 4 did not contain a Settings object.");
                    return false;
                }

                try
                {
                    string backupFileName = CreateBackup(context.SettingsFileName, MigrationName);
                    logger.Info($"ConfigMigrationRunner/{nameof(SettingsV4ToV5DonationSplitMigration)}: Created Settings.json backup at {backupFileName}.");

                    JObject donationSettingsFile = LoadDonationSettingsFile(context.DonationSettingsFileName);
                    JObject donationSettings = donationSettingsFile["DonationSettings"] as JObject ?? new JObject();

                    foreach (string fieldName in DonationFieldNames)
                    {
                        JProperty existingDonationProperty = donationSettings.Property(fieldName);
                        JToken oldSettingsValue = programSettings[fieldName];
                        if (existingDonationProperty == null && oldSettingsValue != null)
                        {
                            donationSettings[fieldName] = oldSettingsValue.DeepClone();
                        }
                    }

                    AddMissingDonationDefaults(donationSettings);

                    foreach (string fieldName in DonationFieldNames)
                    {
                        programSettings.Property(fieldName)?.Remove();
                    }

                    donationSettingsFile["DonationSettingsFileVersion"] = DonationSettings.CurrentDonationSettingsFileVersion;
                    donationSettingsFile["LastUpdated"] = DateTime.UtcNow;
                    donationSettingsFile["DonationSettings"] = donationSettings;

                    WriteJsonObject(context.DonationSettingsFileName, donationSettingsFile);

                    context.SettingsFile["SettingsFileVersion"] = ProgramSettings.CurrentProgramSettingsFileVersion;
                    context.SettingsFile["LastUpdated"] = DateTime.UtcNow;
                    context.MarkSettingsFileChanged();

                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ConfigMigrationRunner/{nameof(SettingsV4ToV5DonationSplitMigration)}: Failed to split donation data from Settings.json.");
                    return false;
                }
            }

            private static JObject LoadDonationSettingsFile(string donationSettingsFileName)
            {
                if (!File.Exists(donationSettingsFileName))
                {
                    return new JObject();
                }

                return ReadJsonObject(donationSettingsFileName);
            }

            private static void AddMissingDonationDefaults(JObject donationSettings)
            {
                JObject defaultDonationSettings = JObject.FromObject(new DonationSettings(), JsonSerializer.Create(DefaultSerializerSettings()));
                foreach (JProperty defaultProperty in defaultDonationSettings.Properties())
                {
                    if (donationSettings.Property(defaultProperty.Name) == null)
                    {
                        donationSettings[defaultProperty.Name] = defaultProperty.Value.DeepClone();
                    }
                }
            }

            private static JsonSerializerSettings DefaultSerializerSettings()
            {
                return new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    TypeNameHandling = TypeNameHandling.Auto,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                };
            }
        }
    }
}
