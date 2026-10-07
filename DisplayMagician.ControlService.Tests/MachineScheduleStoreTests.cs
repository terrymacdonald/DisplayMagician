using System;
using System.IO;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class MachineScheduleStoreTests
{
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
}