using System;
using System.IO;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ShortcutRecoveryStoreTests
{
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