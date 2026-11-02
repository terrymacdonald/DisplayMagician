using System;
using System.IO;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class DisplayDiagnosticStatusTests
{
    [Fact]
    public void DiagnosticsSurviveProgressCompletionAndReloadWithIndependentCopies()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagician-DiagnosticStatus", Guid.NewGuid().ToString("N"));
        try
        {
            StoragePaths paths = new(root);
            OperationStatusStore store = new(paths);
            Guid id = Guid.NewGuid();
            OperationStatusUpdate update = new()
            {
                OperationId = id, Phase = OperationPhase.ApplyingDisplayProfile,
                DisplayResults = new[] { new DisplayConfigurationResult { ProfileId = "profile", TopologyApplied = true,
                    Issues = new[] { new DisplayConfigurationIssue { Stage = "HDR", Component = "Windows", Message = "Not applied", NativeErrorCode = "5" } } } }
            };
            store.Publish("user", 1, update, DateTime.UtcNow);
            update.DisplayResults[0].Issues[0].Message = "Caller mutation";
            OperationStatus completed = store.Publish("user", 1, new OperationStatusUpdate { OperationId = id,
                Phase = OperationPhase.Completed, IsTerminal = true, IsSuccessful = true }, DateTime.UtcNow);
            Assert.Equal("Not applied", Assert.Single(Assert.Single(completed.DisplayResults).Issues).Message);
            completed.DisplayResults[0].Issues[0].Message = "Reader mutation";
            OperationStatus loaded = new OperationStatusStore(paths).Get("user", id)!;
            Assert.True(loaded.IsSuccessful);
            Assert.Equal("5", loaded.DisplayResults[0].Issues[0].NativeErrorCode);
            Assert.Equal("Not applied", loaded.DisplayResults[0].Issues[0].Message);
            Assert.Null(store.Get("another-user", id));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
