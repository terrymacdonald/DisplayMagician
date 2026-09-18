using System;
using System.IO;
using System.Text.Json;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ShortcutStoreTests
{
    [Fact]
    public void Commit_WritesSnapshotAndRejectsStaleRevision()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutStore-{Guid.NewGuid():N}");
        try
        {
            ShortcutStore store = new ShortcutStore(root);
            RepositorySnapshot initial = store.GetSnapshot();

            RepositoryCommitResult committed = store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = initial.Revision, Json = "{\"Shortcuts\":[]}" });

            Assert.False(committed.WasConflict);
            Assert.NotNull(committed.Snapshot);
            Assert.NotEqual(initial.Revision, committed.Snapshot!.Revision);
            using JsonDocument document = JsonDocument.Parse(committed.Snapshot.Json);
            Assert.True(document.RootElement.TryGetProperty("Shortcuts", out _));

            RepositoryCommitResult conflict = store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = initial.Revision, Json = "{\"Shortcuts\":[{}]}" });
            Assert.True(conflict.WasConflict);
            Assert.Equal(committed.Snapshot.Revision, conflict.Snapshot!.Revision);
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
    public void Commit_PreservesAutomaticallyDetectedGameLaunchMode()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutStore-{Guid.NewGuid():N}");
        try
        {
            ShortcutStore store = new ShortcutStore(root);
            RepositorySnapshot initial = store.GetSnapshot();
            string shortcutJson = "{\"ShortcutFileVersion\":\"6\",\"Shortcuts\":[{\"UUID\":\"shortcut-id\",\"GameLaunchMode\":1}]}";

            RepositoryCommitResult committed = store.Commit(new RepositoryCommitRequest
            {
                Repository = RepositoryKind.Shortcuts,
                ExpectedRevision = initial.Revision,
                Json = shortcutJson
            });

            Assert.False(committed.WasConflict);
            using JsonDocument document = JsonDocument.Parse(committed.Snapshot!.Json);
            Assert.Equal(1, document.RootElement.GetProperty("Shortcuts")[0].GetProperty("GameLaunchMode").GetInt32());
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
    public void TryGetShortcutDefinition_ReadsAutomaticallyDetectedGameShortcut()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutStore-{Guid.NewGuid():N}");
        try
        {
            ShortcutStore store = new ShortcutStore(root);
            RepositorySnapshot initial = store.GetSnapshot();
            store.Commit(new RepositoryCommitRequest
            {
                Repository = RepositoryKind.Shortcuts,
                ExpectedRevision = initial.Revision,
                Json = "{\"ShortcutFileVersion\":\"6\",\"Shortcuts\":[{\"UUID\":\"shortcut-id\",\"Name\":\"Test Game\",\"Category\":1,\"GameAppId\":\"42\",\"GameName\":\"Test Game\",\"GameLaunchMode\":1}]}"
            });

            bool wasFound = store.TryGetShortcutDefinition("shortcut-id", out ShortcutDefinition? shortcut);

            Assert.True(wasFound);
            Assert.NotNull(shortcut);
            Assert.Equal(ShortcutDefinitionCategory.Game, shortcut!.Category);
            Assert.Equal(GameLaunchMode.DetectGameRunning, shortcut.GameLaunchMode);
            Assert.Equal("42", shortcut.GameAppId);
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
