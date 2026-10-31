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

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
