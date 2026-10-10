using System;
using System.IO;
using DisplayMagician.ControlService;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class GatewayHttpIdempotencyStoreTests
{
    [Fact]
    public void AcceptedMutationReplaysAcrossRestartAndConflictsOnChangedRequest()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            StoragePaths paths = new StoragePaths(root);
            GatewayHttpIdempotencyStore store = new GatewayHttpIdempotencyStore(paths);
            string key = Guid.NewGuid().ToString("D");
            Guid gatewayInstanceId = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;
            Assert.Equal(GatewayHttpIdempotencyState.Accepted, store.Begin("device-hash", key, "request-hash", gatewayInstanceId, now).State);
            Assert.Equal(GatewayHttpIdempotencyState.Pending, store.Begin("device-hash", key, "request-hash", gatewayInstanceId, now).State);
            store.Complete("device-hash", key, "request-hash", 202, "application/json", "/v1/operations/abc", "1", "{\"operationId\":\"abc\"}");

            GatewayHttpIdempotencyStore reloaded = new GatewayHttpIdempotencyStore(paths);
            GatewayHttpIdempotencyResult replay = reloaded.Begin("device-hash", key, "request-hash", gatewayInstanceId, now.AddMinutes(1));
            Assert.Equal(GatewayHttpIdempotencyState.Replay, replay.State);
            Assert.Equal(202, replay.StatusCode);
            Assert.Equal("application/json", replay.ContentType);
            Assert.Equal("/v1/operations/abc", replay.Location);
            Assert.Equal("1", replay.RetryAfter);
            Assert.Equal("{\"operationId\":\"abc\"}", replay.ResponseJson);
            Assert.Equal(GatewayHttpIdempotencyState.Conflict, reloaded.Begin("device-hash", key, "changed", gatewayInstanceId, now.AddMinutes(1)).State);
            Assert.Equal(GatewayHttpIdempotencyState.Accepted, reloaded.Begin("another-device", key, "changed", gatewayInstanceId, now.AddMinutes(1)).State);
            Assert.Equal(GatewayHttpIdempotencyState.Accepted, reloaded.Begin("device-hash", key, "changed", gatewayInstanceId, now.AddHours(25)).State);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void RetainedResponseUsesUtf8ByteLimitAndCanReplayServerSizeError()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            GatewayHttpIdempotencyStore store = new GatewayHttpIdempotencyStore(new StoragePaths(root));
            string key = Guid.NewGuid().ToString("D");
            Guid gatewayInstanceId = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;
            Assert.Equal(GatewayHttpIdempotencyState.Accepted, store.Begin("device", key, "request", gatewayInstanceId, now).State);
            Assert.Throws<InvalidOperationException>(() => store.Complete("device", key, "request", 200, "application/json", "", "", new string('é', 600000)));
            store.Complete("device", key, "request", 500, "application/problem+json", "", "", "{\"errorCode\":\"response-too-large\"}");

            GatewayHttpIdempotencyResult replay = store.Begin("device", key, "request", gatewayInstanceId, now.AddMinutes(1));
            Assert.Equal(GatewayHttpIdempotencyState.Replay, replay.State);
            Assert.Equal(500, replay.StatusCode);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void InterruptedReservationNeverStartsAgainAndKeepsOperationId()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            StoragePaths paths = new StoragePaths(root);
            GatewayHttpIdempotencyStore store = new GatewayHttpIdempotencyStore(paths);
            string key = Guid.NewGuid().ToString("D");
            Guid gatewayInstanceId = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;
            GatewayHttpIdempotencyResult accepted = store.Begin("device", key, "request", gatewayInstanceId, now);
            Assert.Equal(GatewayHttpIdempotencyState.Accepted, accepted.State);
            Assert.NotEqual(Guid.Empty, accepted.OperationId);
            Assert.Equal(GatewayHttpIdempotencyState.Pending, store.Begin("device", key, "request", gatewayInstanceId, now.AddSeconds(1)).State);
            Assert.Equal(GatewayHttpIdempotencyState.OutcomeUnknown, store.Begin("device", key, "request", Guid.NewGuid(), now.AddSeconds(1)).State);
            Assert.Equal(GatewayHttpIdempotencyState.OutcomeUnknown, store.Begin("device", key, "request", gatewayInstanceId, now.AddMinutes(2)).State);

            GatewayHttpIdempotencyStore reloaded = new GatewayHttpIdempotencyStore(paths);
            GatewayHttpIdempotencyResult unknown = reloaded.Begin("device", key, "request", gatewayInstanceId, now.AddMinutes(1));
            Assert.Equal(GatewayHttpIdempotencyState.OutcomeUnknown, unknown.State);
            Assert.Equal(accepted.OperationId, unknown.OperationId);
            Assert.Equal(GatewayHttpIdempotencyState.OutcomeUnknown, reloaded.Begin("device", key, "request", gatewayInstanceId, now.AddDays(30)).State);
            Assert.Equal(GatewayHttpIdempotencyState.Conflict, reloaded.Begin("device", key, "changed", gatewayInstanceId, now.AddDays(30)).State);

            reloaded.Complete("device", key, "request", 202, "application/json", $"/v1/operations/{accepted.OperationId:D}", "", "{}");
            Assert.Equal(GatewayHttpIdempotencyState.Replay, reloaded.Begin("device", key, "request", gatewayInstanceId, now.AddMinutes(1)).State);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
