using System;
using System.IO;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class MachineScheduleCoordinatorTests
{
    [Fact]
    public void RecordClientSyncFailure_UsesCappedExponentialBackoff()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            MachineScheduleCoordinator coordinator = new MachineScheduleCoordinator(new MachineScheduleStore(new StoragePaths(storageRoot)));
            DateTime now = new DateTime(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);

            MachineScheduleState firstFailure = coordinator.RecordClientSyncFailure(now);
            MachineScheduleState sixthFailure = firstFailure;
            for (int failure = 2; failure <= 6; failure++)
            {
                sixthFailure = coordinator.RecordClientSyncFailure(now);
            }

            Assert.Equal(now.AddHours(1), firstFailure.NextClientSyncUtc);
            Assert.Equal(6, sixthFailure.ConsecutiveClientSyncFailures);
            Assert.Equal(now.AddHours(24), sixthFailure.NextClientSyncUtc);
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
    public void RecordClientSyncSuccess_ClearsFailuresAndSchedulesNextSync()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            MachineScheduleCoordinator coordinator = new MachineScheduleCoordinator(new MachineScheduleStore(new StoragePaths(storageRoot)));
            DateTime now = new DateTime(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);
            coordinator.RecordClientSyncFailure(now);

            MachineScheduleState success = coordinator.RecordClientSyncSuccess(now);

            Assert.Equal(0, success.ConsecutiveClientSyncFailures);
            Assert.Equal(now, success.LastSuccessfulClientSyncUtc);
            Assert.InRange(success.NextClientSyncUtc!.Value, now.AddHours(24), now.AddHours(36));
            Assert.False(string.IsNullOrWhiteSpace(success.InstallId));
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