using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class ControlServicePreferencesTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ServicePrefs-{Guid.NewGuid():N}");
    private StoragePaths Paths => new StoragePaths(_root);
    private string SettingsPath => Path.Combine(Paths.MachinePath, "ControlServiceSettings.json");

    [Fact]
    public void MigrationImportsAllPreferencesAndPreservesLegacyFilesAndBackups()
    {
        Paths.EnsureMachineDirectories();
        string gateway = Path.Combine(Paths.MachinePath, "GatewaySettings.json");
        string logging = Path.Combine(Paths.MachineDiagnosticsPath, "LoggingSettings.json");
        string schedule = Path.Combine(Paths.MachinePath, "ScheduleState.json");
        File.WriteAllText(gateway, JsonSerializer.Serialize(new GatewaySettings { LanPort = 24000, AllowPublicNetworks = true }));
        File.WriteAllText(logging, JsonSerializer.Serialize(new SharedLoggingSettings { SavedLevel = "Error", HasSavedLevel = true }));
        File.WriteAllText(schedule, "{\"ShareAnonymousUsageMetrics\":false,\"InstallId\":\"preserve-id\",\"TotalAnonymousMetricLaunches\":73}");
        byte[][] originals = new[] { gateway, logging, schedule }.Select(File.ReadAllBytes).ToArray();

        ControlServiceSettingsStore store = new ControlServiceSettingsStore(Paths);

        Assert.Equal(24000, store.Get().Gateway.LanPort);
        Assert.True(store.Get().Gateway.AllowPublicNetworks);
        Assert.Equal("Error", store.Get().Logging.SavedLevel);
        Assert.True(store.Get().Logging.HasSavedLevel);
        Assert.False(store.Get().ShareAnonymousUsageMetrics);
        Assert.True(store.Get().HasAnonymousMetricsChoice);
        string[] sourcePaths = { gateway, logging, schedule };
        for (int index = 0; index < sourcePaths.Length; index++)
        {
            Assert.Equal(originals[index], File.ReadAllBytes(sourcePaths[index]));
            string backup = Assert.Single(Directory.GetFiles(Paths.MachineBackupsPath, Path.GetFileName(sourcePaths[index]), SearchOption.AllDirectories));
            Assert.Equal(originals[index], File.ReadAllBytes(backup));
        }
        Assert.DoesNotContain("Sessions", File.ReadAllText(SettingsPath));
        Assert.DoesNotContain("TotalAnonymousMetricLaunches", File.ReadAllText(SettingsPath));
    }

    [Fact]
    public async Task ConcurrentSectionChangesPreserveEachOtherAndUnknownSections()
    {
        ControlServiceSettingsStore store = new ControlServiceSettingsStore(Paths);
        using JsonDocument future = JsonDocument.Parse("{\"Keep\":123}");
        store.Update(settings => settings.AdditionalSettings = new() { ["FuturePreferences"] = future.RootElement.Clone() });
        await Task.WhenAll(
            Task.Run(() => store.Update(settings => settings.Gateway.LanPort = 24567)),
            Task.Run(() => store.Update(settings => { settings.Logging.SavedLevel = "Trace"; settings.Logging.HasSavedLevel = true; })),
            Task.Run(() => store.Update(settings => settings.ShareAnonymousUsageMetrics = false)));
        ControlServiceSettingsDocument saved = new ControlServiceSettingsStore(Paths).Get();
        Assert.Equal(24567, saved.Gateway.LanPort);
        Assert.Equal("Trace", saved.Logging.SavedLevel);
        Assert.False(saved.ShareAnonymousUsageMetrics);
        Assert.Equal(123, saved.AdditionalSettings!["FuturePreferences"].GetProperty("Keep").GetInt32());
    }

    [Fact]
    public void FailedSectionSavePreservesEntirePreviousSnapshot()
    {
        ControlServiceSettingsStore store = new ControlServiceSettingsStore(Paths);
        string previous = File.ReadAllText(SettingsPath);
        using (FileStream locked = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            Assert.ThrowsAny<IOException>(() => store.Update(settings => settings.Logging.SavedLevel = "Debug"));
        Assert.Equal("Warn", store.Get().Logging.SavedLevel);
        Assert.Equal(previous, File.ReadAllText(SettingsPath));
    }

    [Fact]
    public void UnchangedUpdateDoesNotRewritePreferences()
    {
        ControlServiceSettingsStore store = new ControlServiceSettingsStore(Paths);
        using FileStream locked = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        Assert.Equal("Warn", store.Update(_ => { }).Logging.SavedLevel);
    }

    [Fact]
    public void ReadersCannotChangeAuthoritativePreferences()
    {
        ControlServiceSettingsStore store = new ControlServiceSettingsStore(Paths);
        ControlServiceSettingsDocument snapshot = store.Get();
        snapshot.Logging.SavedLevel = "Trace";
        snapshot.Gateway.AllowPublicNetworks = true;
        Assert.Equal("Warn", store.Get().Logging.SavedLevel);
        Assert.False(store.Get().Gateway.AllowPublicNetworks);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{")]
    [InlineData("{\"SchemaVersion\":2,\"Gateway\":{},\"Logging\":{\"SavedLevel\":\"Warn\",\"HasSavedLevel\":true},\"ShareAnonymousUsageMetrics\":false,\"HasAnonymousMetricsChoice\":true}")]
    [InlineData("{\"SchemaVersion\":1,\"Gateway\":{\"LanPort\":0},\"Logging\":{\"SavedLevel\":\"Warn\",\"HasSavedLevel\":true},\"ShareAnonymousUsageMetrics\":false,\"HasAnonymousMetricsChoice\":true}")]
    [InlineData("{\"SchemaVersion\":1,\"Gateway\":{},\"Logging\":{},\"ShareAnonymousUsageMetrics\":false,\"HasAnonymousMetricsChoice\":true}")]
    public void InvalidPreferencesPreserveEvidenceAndAllowOperationalUpdates(string json)
    {
        Paths.EnsureMachineDirectories();
        File.WriteAllText(SettingsPath, json);
        File.WriteAllText(SettingsPath + ".bak", "preserve backup");
        ControlServiceSettingsStore preferences = new(Paths);
        Assert.Equal("Warn", preferences.Get().Logging.SavedLevel);
        Assert.Throws<IOException>(() => preferences.Update(document => document.Logging.SavedLevel = "Info"));
        GatewaySettingsStore gateway = new(Paths, preferences);
        Assert.Throws<IOException>(() => gateway.Update(new GatewaySettings { LanPort = 23000 }));
        MachineScheduleCoordinator schedule = new(new MachineScheduleStore(Paths, preferences));
        Assert.NotEmpty(schedule.EnsureInitialized().InstallId);
        schedule.RecordAnonymousMetricsUsage(new AnonymousMetricsUsageReport { AppVersion = "4.0", IsLaunch = true });
        Assert.Equal(1, schedule.GetState().TotalAnonymousMetricLaunches);
        Assert.Throws<IOException>(() => schedule.UpdateAnonymousMetricsSettings(new AnonymousMetricsSettings { ShareAnonymousUsageMetrics = false }));
        Assert.Equal(json, File.ReadAllText(SettingsPath));
        Assert.Equal("preserve backup", File.ReadAllText(SettingsPath + ".bak"));
    }

    [Fact]
    public void MissingPrimaryWithBackupDoesNotReimportStalePreferences()
    {
        Paths.EnsureMachineDirectories();
        File.WriteAllText(SettingsPath + ".bak", "preserve backup");
        File.WriteAllText(Path.Combine(Paths.MachinePath, "GatewaySettings.json"), "{\"LanPort\":24000}");
        ControlServiceSettingsStore store = new(Paths);
        Assert.NotEqual(24000, store.Get().Gateway.LanPort);
        Assert.Throws<IOException>(() => store.Update(_ => { }));
        Assert.False(File.Exists(SettingsPath));
    }

    [Fact]
    public void InvalidMigrationSourceIsPreservedAndRepairRequiresRestart()
    {
        Paths.EnsureMachineDirectories();
        string legacy = Path.Combine(Paths.MachinePath, "GatewaySettings.json");
        File.WriteAllText(legacy, "{");
        ControlServiceSettingsStore store = new(Paths);
        Assert.False(File.Exists(SettingsPath));
        Assert.Equal("{", File.ReadAllText(legacy));
        File.WriteAllText(legacy, "{\"LanPort\":24000}");
        Assert.Throws<IOException>(() => store.Update(_ => { }));
        Assert.Equal(24000, new ControlServiceSettingsStore(Paths).Get().Gateway.LanPort);
    }

    [Fact]
    public void FailedMigrationBackupDoesNotPublishNewPreferencesOrChangeSources()
    {
        Directory.CreateDirectory(Paths.MachinePath);
        string legacy = Path.Combine(Paths.MachinePath, "GatewaySettings.json");
        File.WriteAllText(legacy, "{\"LanPort\":24000}");
        File.WriteAllText(Paths.MachineBackupsPath, "blocked backup directory");
        ControlServiceSettingsStore store = new(Paths);
        Assert.False(File.Exists(SettingsPath));
        Assert.Equal("{\"LanPort\":24000}", File.ReadAllText(legacy));
        Assert.Throws<IOException>(() => store.Update(_ => { }));
    }

    [Fact]
    public async Task SharedAdaptersPreserveConcurrentSectionChangesAndIgnoreStaleLegacyOnRestart()
    {
        ControlServiceSettingsStore preferences = new(Paths);
        GatewaySettingsStore gateway = new(Paths, preferences);
        MachineDiagnosticLogLevelStore logging = new(Paths, preferences);
        MachineScheduleStore schedule = new(Paths, preferences);
        await Task.WhenAll(
            Task.Run(() => gateway.Update(new GatewaySettings { LanPort = 24678 })),
            Task.Run(() => logging.SetSavedLevel(new SetSharedLogLevelRequest { Level = "Error" })),
            Task.Run(() => schedule.SetAnonymousMetricsPreference(false)));
        File.WriteAllText(Path.Combine(Paths.MachinePath, "GatewaySettings.json"), "{\"LanPort\":23000}");
        ControlServiceSettingsDocument saved = new ControlServiceSettingsStore(Paths).Get();
        Assert.Equal(24678, saved.Gateway.LanPort);
        Assert.Equal("Error", saved.Logging.SavedLevel);
        Assert.False(saved.ShareAnonymousUsageMetrics);
    }

    [Fact]
    public void FrequentOperationalWritesDoNotRewritePreferencesAndDueChecksDoNotRewriteSchedules()
    {
        ControlServiceSettingsStore preferences = new(Paths);
        MachineDiagnosticLogLevelStore logging = new(Paths, preferences);
        MachineScheduleCoordinator schedule = new(new MachineScheduleStore(Paths, preferences));
        DateTime now = DateTime.UtcNow;
        schedule.EnsureInitialized();
        using (FileStream locked = new(SettingsPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Guid owner = Guid.NewGuid();
            logging.Set(new TemporaryDiagnosticLogLevelRequest { OwnerId = owner, Level = "Trace" }, now);
            logging.Set(new TemporaryDiagnosticLogLevelRequest { OwnerId = owner, Level = "Trace" }, now.AddSeconds(15));
            logging.Release(owner, now.AddSeconds(20));
            schedule.RecordClientSyncSuccess(now);
        }
        string schedulePath = Path.Combine(Paths.MachinePath, "ScheduleState.json");
        using (FileStream locked = new(schedulePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Assert.False(schedule.IsClientSyncDue(now));
            schedule.IsMetricsHeartbeatDue(now, "4.0");
            schedule.UpdateAnonymousMetricsSettings(new AnonymousMetricsSettings { ShareAnonymousUsageMetrics = false });
        }
        Assert.DoesNotContain("ShareAnonymousUsageMetrics", File.ReadAllText(schedulePath));
        Assert.DoesNotContain("SavedLevel", File.ReadAllText(Path.Combine(Paths.MachineDiagnosticsPath, "DiagnosticLoggingSessions.json")));
    }

    [Fact]
    public void MetricsMigrationKeepsRuntimeCountersAndCannotOverwriteNewerPreferenceOnRetry()
    {
        Paths.EnsureMachineDirectories();
        string path = Path.Combine(Paths.MachinePath, "ScheduleState.json");
        File.WriteAllText(path, "{\"ShareAnonymousUsageMetrics\":false,\"InstallId\":\"preserve-id\",\"TotalAnonymousMetricLaunches\":73}");
        ControlServiceSettingsStore preferences = new(Paths);
        MachineScheduleCoordinator schedule = new(new MachineScheduleStore(Paths, preferences));
        Assert.False(schedule.GetState().ShareAnonymousUsageMetrics);
        Assert.Equal(73, schedule.GetState().TotalAnonymousMetricLaunches);
        Assert.Equal("preserve-id", schedule.GetState().InstallId);
        schedule.UpdateAnonymousMetricsSettings(new AnonymousMetricsSettings { ShareAnonymousUsageMetrics = true });
        schedule.InitializeAnonymousMetrics(new InitializeAnonymousMetricsRequest { ShareAnonymousUsageMetrics = false, Launches = 73 });
        Assert.True(schedule.GetState().ShareAnonymousUsageMetrics);
    }

    [Fact]
    public void LegacyLoggingSessionsMigrateSeparatelyAndAreNotReimportedAfterRelease()
    {
        Paths.EnsureMachineDirectories();
        DateTime now = DateTime.UtcNow;
        Guid owner = Guid.NewGuid();
        string legacyPath = Path.Combine(Paths.MachineDiagnosticsPath, "LoggingSettings.json");
        string legacy = JsonSerializer.Serialize(new SharedLoggingSettings
        {
            SavedLevel = "Error", HasSavedLevel = true,
            Sessions = new[] { new DiagnosticLoggingSession { OwnerId = owner, UserSid = "user", SessionId = 1, Level = "Trace", ExpiresUtc = now.AddMinutes(1) } }
        });
        File.WriteAllText(legacyPath, legacy);
        MachineDiagnosticLogLevelStore logging = new(Paths);
        Assert.Equal("Trace", logging.GetActiveLevel(now));
        Assert.Equal("Error", logging.Release(owner, now, "user", 1));
        Assert.Equal("Error", new MachineDiagnosticLogLevelStore(Paths).GetActiveLevel(now));
        Assert.Equal(legacy, File.ReadAllText(legacyPath));
    }

    [Fact]
    public void CorruptSessionsDoNotBlockPreferenceChangesAndCannotBeOverwritten()
    {
        ControlServiceSettingsStore preferences = new(Paths);
        string path = Path.Combine(Paths.MachineDiagnosticsPath, "DiagnosticLoggingSessions.json");
        Directory.CreateDirectory(Paths.MachineDiagnosticsPath);
        File.WriteAllText(path, "{");
        MachineDiagnosticLogLevelStore logging = new(Paths, preferences);
        logging.SetSavedLevel(new SetSharedLogLevelRequest { Level = "Error" });
        Assert.Equal("Error", logging.GetActiveLevel(DateTime.UtcNow));
        Assert.Throws<IOException>(() => logging.Set(new TemporaryDiagnosticLogLevelRequest { OwnerId = Guid.NewGuid(), Level = "Debug" }, DateTime.UtcNow));
        Assert.Equal("{", File.ReadAllText(path));
    }

    [Fact]
    public void CorruptionAfterStartupRetainsCachedPreferencesAndRequiresRestartAfterRepair()
    {
        ControlServiceSettingsStore preferences = new(Paths);
        preferences.Update(document => document.Logging.SavedLevel = "Info");
        string valid = File.ReadAllText(SettingsPath);
        string backup = File.ReadAllText(SettingsPath + ".bak");
        File.WriteAllText(SettingsPath, "{");
        Assert.Throws<IOException>(() => preferences.Update(document => document.Logging.SavedLevel = "Debug"));
        Assert.Equal("Info", preferences.Get().Logging.SavedLevel);
        Assert.Equal("{", File.ReadAllText(SettingsPath));
        Assert.Equal(backup, File.ReadAllText(SettingsPath + ".bak"));
        File.WriteAllText(SettingsPath, valid);
        Assert.Throws<IOException>(() => preferences.Update(_ => { }));
        Assert.Equal("Info", new ControlServiceSettingsStore(Paths).Get().Logging.SavedLevel);
    }

    [Fact]
    public void BrokerReadsIndependentSectionsAndCachedLeasesExpireDuringCorruption()
    {
        ControlServiceSettingsStore preferences = new(Paths);
        MachineDiagnosticLogLevelStore logging = new(Paths, preferences);
        DateTime now = DateTime.UtcNow;
        logging.SetSavedLevel(new SetSharedLogLevelRequest { Level = "Error" });
        logging.Set(new TemporaryDiagnosticLogLevelRequest { OwnerId = Guid.NewGuid(), Level = "Trace" }, now);
        SharedLoggingSettings snapshot = new();
        DisplayMagician.SessionLauncher.SessionLauncherWorker.RefreshLoggingSettings(Paths.MachinePath, snapshot);
        Assert.Equal("Trace", snapshot.GetEffectiveLevel(now));
        File.WriteAllText(SettingsPath, "{");
        File.WriteAllText(Path.Combine(Paths.MachineDiagnosticsPath, "DiagnosticLoggingSessions.json"), "{");
        DisplayMagician.SessionLauncher.SessionLauncherWorker.RefreshLoggingSettings(Paths.MachinePath, snapshot);
        Assert.Equal("Error", snapshot.GetEffectiveLevel(now.AddMinutes(1)));
        SharedLoggingSettings restarted = new();
        DisplayMagician.SessionLauncher.SessionLauncherWorker.RefreshLoggingSettings(Paths.MachinePath, restarted);
        Assert.Equal("Warn", restarted.GetEffectiveLevel(now));
    }

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
