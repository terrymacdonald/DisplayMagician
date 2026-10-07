using System;
using System.IO;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class ControlRequestReplayStoreTests
{
    [Fact]
    public void Store_ReplaysASuccessfulMatchingRequestAfterTheServiceRestarts()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            StoragePaths storagePaths = new StoragePaths(storageRoot);
            ControlEnvelope request = new ControlEnvelope { MessageType = ControlMessageType.StartShortcut, Payload = "{\"shortcutId\":\"game\"}" };
            ControlResponse originalResponse = new ControlResponse { IsSuccessful = true, Message = "Shortcut started." };
            DateTime now = DateTime.UtcNow;

            new ControlRequestReplayStore(storagePaths).Store("S-1-5-21-100", request, originalResponse, now);

            Assert.True(new ControlRequestReplayStore(storagePaths).TryGet("S-1-5-21-100", request, now.AddMinutes(1), out ControlResponse replayedResponse));
            Assert.True(replayedResponse.IsSuccessful);
            Assert.Equal("Shortcut started.", replayedResponse.Message);
        }
        finally
        {
            if (Directory.Exists(storageRoot))
            {
                Directory.Delete(storageRoot, true);
            }
        }
    }

    [Fact]
    public void TryGet_RejectsReusingARequestIdForDifferentContent()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            ControlRequestReplayStore store = new ControlRequestReplayStore(new StoragePaths(storageRoot));
            ControlEnvelope request = new ControlEnvelope { MessageType = ControlMessageType.ApplyProfile, Payload = "{\"profileId\":\"one\"}" };
            store.Store("S-1-5-21-100", request, new ControlResponse { IsSuccessful = true }, DateTime.UtcNow);
            request.Payload = "{\"profileId\":\"two\"}";

            Assert.True(store.TryGet("S-1-5-21-100", request, DateTime.UtcNow, out ControlResponse response));
            Assert.False(response.IsSuccessful);
            Assert.Equal(ControlErrorCode.InvalidRequest, response.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(storageRoot))
            {
                Directory.Delete(storageRoot, true);
            }
        }
    }

    [Fact]
    public void TryGet_DoesNotReplayARequestOlderThanTwentyFourHours()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            ControlRequestReplayStore store = new ControlRequestReplayStore(new StoragePaths(storageRoot));
            ControlEnvelope request = new ControlEnvelope { MessageType = ControlMessageType.ApplyProfile, Payload = "{\"profileId\":\"one\"}" };
            DateTime now = DateTime.UtcNow;
            store.Store("S-1-5-21-100", request, new ControlResponse { IsSuccessful = true }, now);

            Assert.False(store.TryGet("S-1-5-21-100", request, now.AddHours(24).AddTicks(1), out _));
        }
        finally
        {
            if (Directory.Exists(storageRoot)) Directory.Delete(storageRoot, true);
        }
    }
}
