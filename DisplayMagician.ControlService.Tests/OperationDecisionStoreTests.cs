using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class OperationDecisionStoreTests
{
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
