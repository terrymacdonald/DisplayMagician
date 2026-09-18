using System;
using System.IO;
using System.Text.Json;
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
}
