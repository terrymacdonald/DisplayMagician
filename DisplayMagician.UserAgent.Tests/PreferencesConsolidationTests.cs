using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DisplayMagician;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

[Collection("Agent repository state")]
public sealed class PreferencesConsolidationTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-DesktopPrefs-{Guid.NewGuid():N}");
    private readonly string _oldSettings = ProgramSettings.ProgramSettingsStorageJsonFullFileName;
    private readonly string _oldSettingsAlias = ProgramSettings._programSettingsStorageJsonFullFileName;
    private readonly string _oldDonation = DonationSettings.DonationSettingsStorageJsonFullFileName;
    private readonly ProgramSettings _oldOwner = DisplayMagician.Program.AppProgramSettings;
    private string SettingsPath => Path.Combine(_root, "Settings.json");
    private string DonationPath => Path.Combine(_root, "Donation.json");

    public PreferencesConsolidationTests()
    {
        Directory.CreateDirectory(_root);
        ProgramSettings.ConfigureStoragePath(_root);
        DonationSettings.ConfigureStoragePath(_root);
    }

    [Fact]
    public void VersionSevenMigrationPreservesPreferencesAndDonationWhileRetiringSeparateWrites()
    {
        File.WriteAllText(SettingsPath, "{\"SettingsFileVersion\":\"7\",\"Settings\":{\"MinimiseOnStart\":true,\"LogLevel\":\"Error\"}}", Encoding.Unicode);
        string legacy = "{\"DonationSettingsFileVersion\":\"1\",\"DonationSettings\":{\"NumberOfTimesRun\":42,\"NumberOfDonations\":3}}";
        File.WriteAllText(DonationPath, legacy, Encoding.Unicode);

        Assert.True(ConfigMigrationRunner.RunMigrationsDetailed().Success);
        JObject migrated = JObject.Parse(File.ReadAllText(SettingsPath));
        Assert.Equal("8", migrated.Value<string>("SettingsFileVersion"));
        Assert.True(migrated["Settings"]!.Value<bool>("MinimiseOnStart"));
        Assert.Equal(42, migrated["Settings"]!["Donation"]!.Value<int>("NumberOfTimesRun"));
        Assert.True(File.Exists(SettingsPath + ".v7-to-v8.bak"));
        Assert.True(File.Exists(DonationPath + ".preferences-consolidation.bak"));
        DisplayMagician.Program.AppProgramSettings = ProgramSettings.LoadSettings();
        DonationSettings donation = DonationSettings.LoadSettings();
        donation.NumberOfTimesRun++;
        Assert.True(donation.SaveSettings());
        Assert.Equal(43, JObject.Parse(File.ReadAllText(SettingsPath))["Settings"]!["Donation"]!.Value<int>("NumberOfTimesRun"));
        Assert.Equal(legacy, File.ReadAllText(DonationPath));
        Assert.True(ConfigMigrationRunner.RunMigrationsDetailed().Success);
        Assert.Equal(43, ProgramSettings.LoadSettings().Donation.NumberOfTimesRun);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("{\"DonationSettingsFileVersion\":\"2\",\"DonationSettings\":{}}")]
    [InlineData("{\"DonationSettingsFileVersion\":\"1\",\"DonationSettings\":null}")]
    public void InvalidLegacyDonationDoesNotOverwritePrimarySettings(string legacy)
    {
        File.WriteAllText(SettingsPath, "{\"SettingsFileVersion\":\"7\",\"Settings\":{\"LogLevel\":\"Trace\"}}", Encoding.Unicode);
        byte[] original = File.ReadAllBytes(SettingsPath);
        File.WriteAllText(DonationPath, legacy, Encoding.Unicode);
        Assert.False(ConfigMigrationRunner.RunMigrationsDetailed().Success);
        Assert.Equal(original, File.ReadAllBytes(SettingsPath));
        Assert.Equal(legacy, File.ReadAllText(DonationPath));
    }

    [Fact]
    public void DonationCanBeImportedWhenPrimarySettingsHaveNotBeenCreated()
    {
        File.WriteAllText(DonationPath, "{\"DonationSettingsFileVersion\":\"1\",\"DonationSettings\":{\"NumberOfTimesRun\":19}}", Encoding.Unicode);
        Assert.True(ConfigMigrationRunner.RunMigrationsDetailed().Success);
        Assert.Equal(19, ProgramSettings.LoadSettings().Donation.NumberOfTimesRun);
    }

    [Fact]
    public void UpdaterImportsOnceAndUsesOnlyPrimarySettingsForLaterWrites()
    {
        ProgramSettings owner = new ProgramSettings { MinimiseOnStart = true };
        DateTime reminder = DateTime.UtcNow.AddDays(3);
        int imports = 0;
        SettingsUpdatePersistenceProvider provider = new SettingsUpdatePersistenceProvider(owner, () => { imports++; return ("4.1.0", reminder); });
        Assert.Equal(new Version("4.1.0"), provider.GetSkippedVersion());
        Assert.Equal(reminder, provider.GetRemindLater()!.Value.ToUniversalTime());
        provider.SetSkippedVersion(new Version("4.2.0"));
        Assert.Equal(1, imports);
        ProgramSettings saved = ProgramSettings.LoadSettings();
        Assert.True(saved.MinimiseOnStart);
        Assert.True(saved.UpdaterPreferencesImported);
        Assert.Equal("4.2.0", saved.SkippedUpdateVersion);
        SettingsUpdatePersistenceProvider restarted = new SettingsUpdatePersistenceProvider(saved, () => throw new InvalidOperationException("Must not reimport."));
        Assert.Equal(new Version("4.2.0"), restarted.GetSkippedVersion());
    }

    [Fact]
    public async Task ConcurrentUpdaterChoicesPreserveEachOtherAndOtherPreferences()
    {
        ProgramSettings owner = new ProgramSettings { MinimiseOnStart = true, UpdaterPreferencesImported = true };
        owner.Donation.NumberOfTimesRun = 7;
        SettingsUpdatePersistenceProvider provider = new SettingsUpdatePersistenceProvider(owner);
        DateTime reminder = DateTime.UtcNow.AddDays(2);
        await Task.WhenAll(Task.Run(() => provider.SetSkippedVersion(new Version("4.3.0"))), Task.Run(() => provider.SetRemindLater(reminder)));
        ProgramSettings saved = ProgramSettings.LoadSettings();
        Assert.Equal("4.3.0", saved.SkippedUpdateVersion);
        Assert.Equal(reminder, saved.UpdateReminderUtc);
        Assert.True(saved.MinimiseOnStart);
        Assert.Equal(7, saved.Donation.NumberOfTimesRun);
    }

    [Fact]
    public void FailedUpdaterSaveRetainsCurrentValuesAndPrimaryFile()
    {
        ProgramSettings owner = new ProgramSettings { UpdaterPreferencesImported = true, SkippedUpdateVersion = "4.1.0" };
        Assert.True(owner.SaveSettings());
        byte[] previous = File.ReadAllBytes(SettingsPath);
        SettingsUpdatePersistenceProvider provider = new SettingsUpdatePersistenceProvider(owner);
        using (FileStream locked = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            Assert.Throws<IOException>(() => provider.SetSkippedVersion(new Version("4.2.0")));
        Assert.Equal("4.1.0", owner.SkippedUpdateVersion);
        Assert.Equal(previous, File.ReadAllBytes(SettingsPath));
    }

    [Fact]
    public void ReminderAndSkippedVersionAreCommittedTogether()
    {
        DateTime originalReminder = DateTime.UtcNow.AddDays(1);
        ProgramSettings owner = new ProgramSettings { UpdaterPreferencesImported = true, SkippedUpdateVersion = "4.1.0", UpdateReminderUtc = originalReminder };
        Assert.True(owner.SaveSettings());
        SettingsUpdatePersistenceProvider provider = new SettingsUpdatePersistenceProvider(owner);
        DateTime nextReminder = DateTime.UtcNow.AddDays(7);
        using (FileStream locked = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            Assert.Throws<IOException>(() => provider.SetReminderAndClearSkippedVersion(nextReminder));
        Assert.Equal("4.1.0", owner.SkippedUpdateVersion);
        Assert.Equal(originalReminder, owner.UpdateReminderUtc);
        provider.SetReminderAndClearSkippedVersion(nextReminder);
        ProgramSettings saved = ProgramSettings.LoadSettings();
        Assert.Null(saved.SkippedUpdateVersion);
        Assert.Equal(nextReminder, saved.UpdateReminderUtc);
    }

    [Fact]
    public void UnknownPreferencesSurviveMigrationLoadAndSave()
    {
        File.WriteAllText(SettingsPath, "{\"SettingsFileVersion\":\"7\",\"Settings\":{\"FutureChoice\":{\"Keep\":true}}}", Encoding.Unicode);
        File.WriteAllText(DonationPath, "{\"DonationSettingsFileVersion\":\"1\",\"DonationSettings\":{\"FutureDonationChoice\":17}}", Encoding.Unicode);
        Assert.True(ConfigMigrationRunner.RunMigrationsDetailed().Success);
        ProgramSettings owner = ProgramSettings.LoadSettings();
        Assert.True(owner.SaveSettings());
        JObject saved = JObject.Parse(File.ReadAllText(SettingsPath));
        Assert.True(saved["Settings"]!["FutureChoice"]!.Value<bool>("Keep"));
        Assert.Equal(17, saved["Settings"]!["Donation"]!.Value<int>("FutureDonationChoice"));
    }

    [Fact]
    public void FailedLegacyUpdaterReadCannotDiscardExistingChoices()
    {
        ProgramSettings owner = new ProgramSettings { SkippedUpdateVersion = "4.1.0" };
        Assert.True(owner.SaveSettings());
        byte[] original = File.ReadAllBytes(SettingsPath);
        SettingsUpdatePersistenceProvider provider = new SettingsUpdatePersistenceProvider(owner, () => throw new IOException("Legacy settings unavailable."));
        Assert.Equal(new Version("4.1.0"), provider.GetSkippedVersion());
        Assert.False(owner.UpdaterPreferencesImported);
        Assert.Throws<IOException>(() => provider.SetSkippedVersion(new Version("4.2.0")));
        Assert.Equal(original, File.ReadAllBytes(SettingsPath));
    }

    public void Dispose()
    {
        ProgramSettings.ProgramSettingsStorageJsonFullFileName = _oldSettings;
        ProgramSettings._programSettingsStorageJsonFullFileName = _oldSettingsAlias;
        DonationSettings.DonationSettingsStorageJsonFullFileName = _oldDonation;
        DisplayMagician.Program.AppProgramSettings = _oldOwner;
        Directory.Delete(_root, true);
    }
}
