using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class OperationDecisionStoreTests
{
    [Theory]
    [InlineData(60)]
    [InlineData(61)]
    public async Task LateAnswerAppliesTheDefaultEvenBeforeTheExpirySweep(int seconds)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            StoragePaths paths = new StoragePaths(root);
            OperationDecisionStore store = new OperationDecisionStore(paths);
            DateTime now = DateTime.UtcNow;
            OperationDecision prompt = store.Create("owner", 1, Guid.NewGuid(), "Continue?", "Test",
                new[] { OperationDecisionChoice.Continue, OperationDecisionChoice.StopAndRestore }, OperationDecisionChoice.Continue, now.AddMinutes(1), now);
            Task<OperationDecision> waiting = store.WaitForResolutionAsync(prompt.PromptId, CancellationToken.None);

            Assert.Null(store.Resolve("owner", 2, prompt.PromptId, OperationDecisionChoice.StopAndRestore, now.AddSeconds(seconds)));
            Assert.Equal(OperationDecisionChoice.Continue, (await waiting.WaitAsync(TimeSpan.FromSeconds(5))).ResolvedChoice);
            Assert.Equal(OperationDecisionChoice.Continue, new OperationDecisionStore(paths).Get("owner", prompt.PromptId)!.ResolvedChoice);
            Assert.Empty(store.GetPending("owner", 1));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Resolve_RejectsAnotherUsersDecisionEvenWhenThePromptIdIsKnown()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            OperationDecisionStore store = new OperationDecisionStore(new StoragePaths(storageRoot));
            OperationDecision decision = store.Create("S-1-5-21-100", 10, Guid.NewGuid(), "Continue?", "A display change failed.", new[] { OperationDecisionChoice.Continue }, OperationDecisionChoice.Continue, DateTime.UtcNow.AddMinutes(1), DateTime.UtcNow);

            OperationDecision? resolved = store.Resolve("S-1-5-21-200", 20, decision.PromptId, OperationDecisionChoice.Continue, DateTime.UtcNow);

            Assert.Null(resolved);
            Assert.False(store.Get("S-1-5-21-100", decision.PromptId)!.IsResolved);
        }
        finally { if (Directory.Exists(storageRoot)) Directory.Delete(storageRoot, true); }
    }

    [Fact]
    public async Task WaitForResolutionAsync_RestoresAnUnresolvedDecisionAfterServiceRestart()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            StoragePaths storagePaths = new StoragePaths(storageRoot);
            DateTime now = DateTime.UtcNow;
            OperationDecision original = new OperationDecisionStore(storagePaths).Create(
                "S-1-5-21-100", 10, Guid.NewGuid(), "Continue?", "A display change failed.",
                new[] { OperationDecisionChoice.Continue, OperationDecisionChoice.StopAndRestore }, OperationDecisionChoice.Continue,
                now.AddMinutes(1), now);
            OperationDecisionStore restartedStore = new OperationDecisionStore(storagePaths);
            Task<OperationDecision> waiting = restartedStore.WaitForResolutionAsync(original.PromptId, CancellationToken.None);

            OperationDecision? resolved = restartedStore.Resolve("S-1-5-21-100", 11, original.PromptId, OperationDecisionChoice.Continue, now.AddSeconds(1));

            Assert.NotNull(resolved);
            Assert.Equal(OperationDecisionChoice.Continue, (await waiting).ResolvedChoice);
        }
        finally
        {
            if (Directory.Exists(storageRoot)) Directory.Delete(storageRoot, true);
        }
    }
}
