using System;
using System.IO;
using System.Text.Json;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class MachineScheduleStoreTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-ScheduleRecovery-" + Guid.NewGuid().ToString("N"));

    [Theory]
    [InlineData("broken JSON")]
    [InlineData("null")]
    [InlineData(null)]
    public void InvalidOrMissingPrimaryRecoversBackupWithoutChangingPreferences(string? primary)
    {
        StoragePaths paths = new(_root);
        ControlServiceSettingsStore preferences = new(paths);
        preferences.Update(document => document.ShareAnonymousUsageMetrics = false);
        string path = Path.Combine(paths.MachinePath, "ScheduleState.json");
        string backup = JsonSerializer.Serialize(new MachineScheduleState
        {
            InstallId = "retained-install", TotalAnonymousMetricLaunches = 73,
            ConsecutiveClientSyncFailures = 3, NextClientSyncUtc = DateTime.UtcNow.AddHours(8)
        });
        if (primary != null) File.WriteAllText(path, primary);
        File.WriteAllText(path + ".bak", backup);

        MachineScheduleStore store = new(paths, preferences);

        Assert.Equal("retained-install", store.Get().InstallId);
        Assert.Equal(73, store.Get().TotalAnonymousMetricLaunches);
        Assert.Equal(3, store.Get().ConsecutiveClientSyncFailures);
        Assert.False(store.Get().ShareAnonymousUsageMetrics);
        Assert.Equal(backup, File.ReadAllText(path + ".bak"));
        Assert.Equal("retained-install", JsonSerializer.Deserialize<MachineScheduleState>(File.ReadAllText(path))!.InstallId);
        Assert.False(new MachineScheduleCoordinator(store).IsClientSyncDue(DateTime.UtcNow));
    }

    [Fact]
    public void UnusablePrimaryAndBackupResetAndResumeNormalScheduling()
    {
        StoragePaths paths = new(_root);
        ControlServiceSettingsStore preferences = new(paths);
        string path = Path.Combine(paths.MachinePath, "ScheduleState.json");
        File.WriteAllText(path, "broken primary");
        File.WriteAllText(path + ".bak", "broken backup");
        MachineScheduleCoordinator schedule = new(new MachineScheduleStore(paths, preferences));

        MachineScheduleState reset = schedule.EnsureInitialized();
        Assert.True(Guid.TryParse(reset.InstallId, out _));
        Assert.NotNull(reset.NextClientSyncUtc);
        Assert.Equal(0, reset.TotalAnonymousMetricLaunches);
        schedule.RecordClientSyncSuccess(DateTime.UtcNow);
        Assert.Equal(reset.InstallId, new MachineScheduleStore(paths, preferences).Get().InstallId);
        Assert.NotNull(JsonSerializer.Deserialize<MachineScheduleState>(File.ReadAllText(path))!.LastSuccessfulClientSyncUtc);
    }

    [Fact]
    public void LockedPrimaryUsesBackupAndLaterSavesCanRetry()
    {
        StoragePaths paths = new(_root);
        ControlServiceSettingsStore preferences = new(paths);
        string path = Path.Combine(paths.MachinePath, "ScheduleState.json");
        File.WriteAllText(path, "unreadable while locked");
        File.WriteAllText(path + ".bak", JsonSerializer.Serialize(new MachineScheduleState { InstallId = "retained-install" }));
        MachineScheduleStore store;
        using (FileStream locked = new(path, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            store = new MachineScheduleStore(paths, preferences);
            Assert.Equal("retained-install", store.Get().InstallId);
        }
        new MachineScheduleCoordinator(store).RecordClientSyncSuccess(DateTime.UtcNow);
        Assert.Equal("retained-install", new MachineScheduleStore(paths, preferences).Get().InstallId);
    }

    [Fact]
    public void ValidPrimaryWinsWithoutReadingOrReplacingStaleBackup()
    {
        StoragePaths paths = new(_root);
        ControlServiceSettingsStore preferences = new(paths);
        string path = Path.Combine(paths.MachinePath, "ScheduleState.json");
        string primary = JsonSerializer.Serialize(new MachineScheduleState { InstallId = "current-install" });
        File.WriteAllText(path, primary);
        File.WriteAllText(path + ".bak", "stale backup");
        Assert.Equal("current-install", new MachineScheduleStore(paths, preferences).Get().InstallId);
        Assert.Equal(primary, File.ReadAllText(path));
        Assert.Equal("stale backup", File.ReadAllText(path + ".bak"));
    }

    [Fact]
    public void Update_PersistsMachineScheduleStateWithUtcValues()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            StoragePaths storagePaths = new StoragePaths(storageRoot);
            MachineScheduleStore store = new MachineScheduleStore(storagePaths);
            DateTime localDueTime = new DateTime(2026, 9, 21, 10, 30, 0, DateTimeKind.Local);

            store.Update(state =>
            {
                state.InstallId = "machine-install-id";
                state.NextClientSyncUtc = localDueTime;
                state.ConsecutiveClientSyncFailures = -1;
                state.TotalAnonymousMetricLaunches = 12;
            });

            MachineScheduleState reloaded = new MachineScheduleStore(storagePaths).Get();

            Assert.Equal("machine-install-id", reloaded.InstallId);
            Assert.Equal(localDueTime.ToUniversalTime(), reloaded.NextClientSyncUtc);
            Assert.Equal(0, reloaded.ConsecutiveClientSyncFailures);
            Assert.Equal(12, reloaded.TotalAnonymousMetricLaunches);
        }
        finally
        {
            if (Directory.Exists(storageRoot))
            {
                Directory.Delete(storageRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void Constructor_UsesDefaultsWhenPersistedStateIsInvalid()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            string machinePath = Path.Combine(storageRoot, "Machine");
            Directory.CreateDirectory(machinePath);
            File.WriteAllText(Path.Combine(machinePath, "ScheduleState.json"), "not-json");

            MachineScheduleState state = new MachineScheduleStore(new StoragePaths(storageRoot)).Get();

            Assert.True(state.ShareAnonymousUsageMetrics);
            Assert.Equal(string.Empty, state.InstallId);
            Assert.Null(state.NextClientSyncUtc);
        }
        finally
        {
            if (Directory.Exists(storageRoot))
            {
                Directory.Delete(storageRoot, recursive: true);
            }
        }
    }
    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
