using AutoUpdaterDotNET;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace DisplayMagician
{
    /// <summary>Keeps low-frequency updater choices in the primary desktop settings file.</summary>
    internal sealed class SettingsUpdatePersistenceProvider : IPersistenceProvider
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ProgramSettings _owner;
        private readonly Func<(string SkippedVersion, DateTime? ReminderUtc)> _readLegacy;

        internal SettingsUpdatePersistenceProvider(ProgramSettings owner, Func<(string, DateTime?)> readLegacy = null)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _readLegacy = readLegacy ?? ReadLegacyPreferences;
        }

        public Version GetSkippedVersion()
        {
            EnsureImported();
            return Version.TryParse(_owner.SkippedUpdateVersion, out Version version) ? version : null;
        }

        public DateTime? GetRemindLater()
        {
            EnsureImported();
            return _owner.UpdateReminderUtc?.ToLocalTime();
        }

        public void SetSkippedVersion(Version version)
        {
            lock (_owner.PersistenceLock)
            {
                EnsureImported();
                if (!_owner.UpdaterPreferencesImported) throw new IOException("Legacy updater preferences must be imported before saving changes.");
                Save(version?.ToString(), _owner.UpdateReminderUtc);
            }
        }

        public void SetRemindLater(DateTime? remindLaterAt)
        {
            lock (_owner.PersistenceLock)
            {
                EnsureImported();
                if (!_owner.UpdaterPreferencesImported) throw new IOException("Legacy updater preferences must be imported before saving changes.");
                Save(_owner.SkippedUpdateVersion, remindLaterAt?.ToUniversalTime());
            }
        }

        internal void SetReminderAndClearSkippedVersion(DateTime remindLaterAt)
        {
            lock (_owner.PersistenceLock)
            {
                EnsureImported();
                if (!_owner.UpdaterPreferencesImported) throw new IOException("Legacy updater preferences must be imported before saving changes.");
                Save(null, remindLaterAt.ToUniversalTime());
            }
        }

        private void EnsureImported()
        {
            lock (_owner.PersistenceLock)
            {
                if (_owner.UpdaterPreferencesImported) return;
                try
                {
                    (string skipped, DateTime? reminder) = _readLegacy();
                    Save(skipped, reminder);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Security.SecurityException || ex is FormatException)
                {
                    Logger.Warn(ex, "SettingsUpdatePersistenceProvider/EnsureImported: Could not import updater preferences; existing settings and registry values are preserved for retry.");
                }
            }
        }

        private void Save(string skipped, DateTime? reminder)
        {
            lock (_owner.PersistenceLock)
            {
                string oldSkipped = _owner.SkippedUpdateVersion;
                DateTime? oldReminder = _owner.UpdateReminderUtc;
                bool oldImported = _owner.UpdaterPreferencesImported;
                _owner.SkippedUpdateVersion = skipped;
                _owner.UpdateReminderUtc = reminder;
                _owner.UpdaterPreferencesImported = true;
                if (_owner.SaveSettings()) return;
                _owner.SkippedUpdateVersion = oldSkipped;
                _owner.UpdateReminderUtc = oldReminder;
                _owner.UpdaterPreferencesImported = oldImported;
                throw new IOException("The updater preferences could not be saved to the primary settings file.");
            }
        }

        private static (string, DateTime?) ReadLegacyPreferences()
        {
            // Match AutoUpdater.NET 1.9.2's default location, without writing or deleting the old values.
            // https://github.com/ravibpatel/AutoUpdater.NET/blob/v1.9.2/AutoUpdater.NET/AutoUpdater.cs
            Assembly assembly = typeof(Program).Assembly;
            string company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
            string title = string.IsNullOrEmpty(AutoUpdater.AppTitle)
                ? assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? assembly.GetName().Name : AutoUpdater.AppTitle;
            string location = string.IsNullOrEmpty(company) ? $@"Software\{title}\AutoUpdater" : $@"Software\{company}\{title}\AutoUpdater";
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(location, false);
            string skipped = key?.GetValue("SkippedVersion")?.ToString();
            if (!string.IsNullOrWhiteSpace(skipped) && !Version.TryParse(skipped, out _)) throw new FormatException("The legacy skipped version is invalid.");
            string reminder = key?.GetValue("RemindLaterAt")?.ToString();
            if (string.IsNullOrWhiteSpace(reminder)) return (skipped, null);
            DateTime local = DateTime.Parse(reminder, CultureInfo.GetCultureInfo("en-US"));
            return (skipped, DateTime.SpecifyKind(local, DateTimeKind.Local).ToUniversalTime());
        }
    }
}
