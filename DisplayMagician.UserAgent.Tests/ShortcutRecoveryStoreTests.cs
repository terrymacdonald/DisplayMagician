using System;
using System.IO;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ShortcutRecoveryStoreTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void RecoveryWithMissingOriginalSnapshotOrUnsupportedVersionIsPreserved(int version)
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutRecovery-{Guid.NewGuid():N}");
        try
        {
            ShortcutRecoveryStore store = new ShortcutRecoveryStore(root);
            store.Save(new ShortcutRecoveryRecord { SchemaVersion = version, RequiresDisplayRestore = true, DisplayProfileId = "changed-profile" });

            Assert.Null(store.GetPending());
            Assert.True(store.HasPendingRecovery());
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void VersionTwoRecoveryPreservesOriginalSnapshotsIndependentlyOfSavedProfiles()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutRecovery-{Guid.NewGuid():N}");
        try
        {
            ShortcutRecoveryStore store = new ShortcutRecoveryStore(root);
            ShortcutRecoveryRecord record = new ShortcutRecoveryRecord
            {
                SchemaVersion = 2, OperationId = Guid.NewGuid(),
                DisplayProfileId = "deleted-profile", RequiresDisplayRestore = true,
                DisplayProfileSnapshotJson = "{\"Name\":\"Unsaved original display\"}",
                AudioProfileSnapshotJson = "{\"Name\":\"Unsaved original audio\"}"
            };
            store.Save(record);
            ShortcutRecoveryRecord loaded = new ShortcutRecoveryStore(root).GetPending()!;
            Assert.Equal(2, loaded.SchemaVersion);
            Assert.Equal(record.DisplayProfileSnapshotJson, loaded.DisplayProfileSnapshotJson);
            Assert.Equal(record.AudioProfileSnapshotJson, loaded.AudioProfileSnapshotJson);

            File.WriteAllText(Path.Combine(root, "Settings", "ShortcutRecovery.json"), "{\"DisplayProfileId\":\"legacy-profile\",\"RequiresDisplayRestore\":true}");
            ShortcutRecoveryRecord legacy = store.GetPending()!;
            Assert.Equal(1, legacy.SchemaVersion);
            Assert.Equal("legacy-profile", legacy.DisplayProfileId);
            Assert.Empty(legacy.DisplayProfileSnapshotJson);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void SaveAndClear_PersistsAndRemovesThePendingRecoveryRecord()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutRecovery-{Guid.NewGuid():N}");
        try
        {
            ShortcutRecoveryStore store = new ShortcutRecoveryStore(root);
            ShortcutRecoveryRecord record = new ShortcutRecoveryRecord
            {
                OperationId = Guid.NewGuid(),
                ShortcutId = "shortcut-id",
                DisplayProfileId = "display-profile",
                AudioProfileId = "audio-profile",
                RequiresDisplayRestore = true,
                RequiresAudioRestore = true,
                CreatedUtc = DateTime.UtcNow
            };

            store.Save(record);
            ShortcutRecoveryRecord? pending = store.GetPending();

            Assert.NotNull(pending);
            Assert.Equal(record.OperationId, pending!.OperationId);
            Assert.Equal("shortcut-id", pending.ShortcutId);
            Assert.True(pending.RequiresDisplayRestore);
            Assert.True(pending.RequiresAudioRestore);

            store.Clear();

            Assert.Null(store.GetPending());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void HasPendingRecovery_ReturnsTrueWhenTheRecoveryRecordCannotBeRead()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutRecovery-{Guid.NewGuid():N}");
        try
        {
            string settingsPath = Path.Combine(root, "Settings");
            Directory.CreateDirectory(settingsPath);
            File.WriteAllText(Path.Combine(settingsPath, "ShortcutRecovery.json"), "not valid json");
            ShortcutRecoveryStore store = new ShortcutRecoveryStore(root);

            Assert.True(store.HasPendingRecovery());
            Assert.Null(store.GetPending());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}
