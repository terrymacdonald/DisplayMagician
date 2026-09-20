using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ShortcutRunnerTests
{
    [Fact]
    public async Task PrepareRunAsync_ReturnsTheAgentOwnedShortcutDefinition()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutRunner-{Guid.NewGuid():N}");
        ShortcutStore store = new ShortcutStore(root);
        RepositorySnapshot snapshot = store.GetSnapshot();
        store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = snapshot.Revision, Json = "{\"Shortcuts\":[{\"UUID\":\"runner-shortcut\",\"Name\":\"Runner shortcut\",\"Category\":0}]}" });
        ShortcutRunner runner = new ShortcutRunner(store, new AutomaticGameDetectionRegistry(), new UserProfileOperationService(), new ShortcutRecoveryStore(root));

        ShortcutRunResult result = await runner.PrepareRunAsync("runner-shortcut", CancellationToken.None);

        Assert.Equal(ShortcutRunOutcome.Prepared, result.Outcome);
        Assert.NotEqual(Guid.Empty, result.OperationId);
        Assert.Equal("Runner shortcut", result.Shortcut!.Name);
    }

    [Fact]
    public async Task ApplyShortcutProfilesAsync_ReturnsCompletedAfterANoGameShortcutFinishes()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutRunner-{Guid.NewGuid():N}");
        ShortcutStore store = new ShortcutStore(root);
        RepositorySnapshot snapshot = store.GetSnapshot();
        store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = snapshot.Revision, Json = "{\"Shortcuts\":[{\"UUID\":\"runner-shortcut\",\"Name\":\"Runner shortcut\",\"Category\":2}]}" });
        ShortcutRunner runner = new ShortcutRunner(store, new AutomaticGameDetectionRegistry(), new UserProfileOperationService(), new ShortcutRecoveryStore(root));

        ShortcutRunResult result = await runner.ApplyShortcutProfilesAsync("runner-shortcut", 0, CancellationToken.None);

        Assert.Equal(ShortcutRunOutcome.Completed, result.Outcome);
        Assert.NotEqual(Guid.Empty, result.OperationId);
    }

    [Fact]
    public async Task ApplyShortcutProfilesAsync_ReturnsCancelledWhenRequestedBeforeExecution()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutRunner-{Guid.NewGuid():N}");
        ShortcutStore store = new ShortcutStore(root);
        RepositorySnapshot snapshot = store.GetSnapshot();
        store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = snapshot.Revision, Json = "{\"Shortcuts\":[{\"UUID\":\"runner-shortcut\",\"Name\":\"Runner shortcut\",\"Category\":2}]}" });
        ShortcutRunner runner = new ShortcutRunner(store, new AutomaticGameDetectionRegistry(), new UserProfileOperationService(), new ShortcutRecoveryStore(root));
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        ShortcutRunResult result = await runner.ApplyShortcutProfilesAsync("runner-shortcut", 0, cancellationTokenSource.Token);

        Assert.Equal(ShortcutRunOutcome.Cancelled, result.Outcome);
        Assert.NotEqual(Guid.Empty, result.OperationId);
    }

    [Fact]
    public async Task ApplyShortcutProfilesAsync_FailsWhenARequiredStartProgramCannotRun()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutRunner-{Guid.NewGuid():N}");
        ShortcutStore store = new ShortcutStore(root);
        RepositorySnapshot snapshot = store.GetSnapshot();
        store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = snapshot.Revision, Json = "{\"Shortcuts\":[{\"UUID\":\"runner-shortcut\",\"Name\":\"Runner shortcut\",\"Category\":2,\"StartPrograms\":[{\"Executable\":\"C:\\\\missing-start-program.exe\"}]}]}" });
        ShortcutRunner runner = new ShortcutRunner(store, new AutomaticGameDetectionRegistry(), new UserProfileOperationService(), new ShortcutRecoveryStore(root));

        ShortcutRunResult result = await runner.ApplyShortcutProfilesAsync("runner-shortcut", 0, CancellationToken.None);

        Assert.Equal(ShortcutRunOutcome.Failed, result.Outcome);
    }

    [Fact]
    public async Task ApplyShortcutProfilesAsync_FailsBeforeSideEffectsWhenRequiredExecutableArgumentsAreMissing()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutRunner-{Guid.NewGuid():N}");
        ShortcutStore store = new ShortcutStore(root);
        RepositorySnapshot snapshot = store.GetSnapshot();
        string executablePath = Environment.ProcessPath!;
        store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = snapshot.Revision, Json = $"{{\"Shortcuts\":[{{\"UUID\":\"runner-shortcut\",\"Name\":\"Runner shortcut\",\"Category\":0,\"Executable\":\"{executablePath.Replace("\\", "\\\\")}\",\"ExecutableArgumentsRequired\":true}}]}}" });
        ShortcutRunner runner = new ShortcutRunner(store, new AutomaticGameDetectionRegistry(), new UserProfileOperationService(), new ShortcutRecoveryStore(root));

        ShortcutRunResult result = await runner.ApplyShortcutProfilesAsync("runner-shortcut", 0, CancellationToken.None);

        Assert.Equal(ShortcutRunOutcome.Failed, result.Outcome);
    }

    [Fact]
    public async Task ApplyShortcutProfilesAsync_RestoresAutomaticDetectionAfterAManualRunFails()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutRunner-{Guid.NewGuid():N}");
        ShortcutStore store = new ShortcutStore(root);
        RepositorySnapshot snapshot = store.GetSnapshot();
        store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = snapshot.Revision, Json = "{\"Shortcuts\":[{\"UUID\":\"automatic-shortcut\",\"Name\":\"Automatic shortcut\",\"Category\":1,\"GameLaunchMode\":1,\"GameLibrary\":1,\"GameAppId\":\"missing-game\",\"ProfileUUID\":\"missing-profile\"}]}" });
        AutomaticGameDetectionRegistry registry = new AutomaticGameDetectionRegistry();
        registry.ReplaceAutomaticDetections(store.GetShortcutDefinitions());
        ShortcutRunner runner = new ShortcutRunner(store, registry, new UserProfileOperationService(), new ShortcutRecoveryStore(root));

        ShortcutRunResult result = await runner.ApplyShortcutProfilesAsync("automatic-shortcut", 0, CancellationToken.None);

        Assert.Equal(ShortcutRunOutcome.Failed, result.Outcome);
        Assert.True(registry.IsAutomaticDetectionRegistered("automatic-shortcut"));
    }

    [Fact]
    public async Task RestorePendingRecoveryAsync_FailsClosedWhenTheRecoveryRecordIsUnreadable()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutRunner-{Guid.NewGuid():N}");
        try
        {
            string settingsPath = Path.Combine(root, "Settings");
            Directory.CreateDirectory(settingsPath);
            File.WriteAllText(Path.Combine(settingsPath, "ShortcutRecovery.json"), "not valid json");
            ShortcutRunner runner = new ShortcutRunner(new ShortcutStore(root), new AutomaticGameDetectionRegistry(), new UserProfileOperationService(), new ShortcutRecoveryStore(root));

            bool restored = await runner.RestorePendingRecoveryAsync(0, CancellationToken.None);

            Assert.False(restored);
            Assert.True(runner.IsRecoveryRequired);
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
    public async Task ApplyShortcutProfilesAsync_RejectsUwpApplicationShortcutWithoutAnAppUserModelId()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutRunner-{Guid.NewGuid():N}");
        ShortcutStore store = new ShortcutStore(root);
        RepositorySnapshot snapshot = store.GetSnapshot();
        store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = snapshot.Revision, Json = "{\"Shortcuts\":[{\"UUID\":\"application-shortcut\",\"Category\":3,\"ApplicationLibrary\":2}]}" });
        ShortcutRunner runner = new ShortcutRunner(store, new AutomaticGameDetectionRegistry(), new UserProfileOperationService(), new ShortcutRecoveryStore(root));

        ShortcutRunResult result = await runner.ApplyShortcutProfilesAsync("application-shortcut", 0, CancellationToken.None);

        Assert.Equal(ShortcutRunOutcome.Failed, result.Outcome);
    }
}
