using System;
using System.Text.Json;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using DisplayMagician.Contracts;
using DisplayMagician.Gateway;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class GatewayEventRoutesTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("retained", false)]
    [InlineData("unknown", true)]
    public async Task StreamSnapshotAndReplayHaveNoSetupGap(string? cursor, bool snapshotExpected)
    {
        using CancellationTokenSource stop = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        GatewayAuthenticationResult authentication = new GatewayAuthenticationResult { IsAuthenticated = true, OwnerUserSid = "owner", DeviceId = "phone", GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead } };
        FakeEventClient client = new FakeEventClient(authentication, stop);
        DefaultHttpContext context = new DefaultHttpContext();
        context.RequestAborted = stop.Token;
        context.Request.Headers.Authorization = "Bearer test-credential";
        if (cursor != null) context.Request.Headers["Last-Event-ID"] = cursor;
        context.Items[GatewayRequestAuthenticationMiddleware.AuthenticationItemName] = authentication;
        context.Response.Body = new MemoryStream();

        await GatewayEventRoutes.StreamAsync(context, client, new GatewayEventStreamLimiter());

        string frames = Encoding.UTF8.GetString(((MemoryStream)context.Response.Body).ToArray());
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(snapshotExpected, frames.Contains("event: snapshot"));
        Assert.Contains("event: operation.updated", frames);
        Assert.Contains("id: after-setup", frames);
        Assert.Equal(snapshotExpected ? 1 : 0, client.SnapshotReads);
        Assert.DoesNotContain("pendingDecisions", frames);
        Assert.DoesNotContain("OwnerUserSid", frames);
        if (cursor == "unknown") Assert.Contains("cursor-unknown", frames);
    }

    [Fact]
    public async Task DuplicateStreamIsRejectedBeforeReadingHistory()
    {
        using CancellationTokenSource stop = new CancellationTokenSource();
        GatewayAuthenticationResult authentication = new GatewayAuthenticationResult { IsAuthenticated = true, OwnerUserSid = "owner", DeviceId = "phone", GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead } };
        GatewayEventStreamLimiter limiter = new GatewayEventStreamLimiter();
        using IDisposable? existing = limiter.TryEnter(authentication);
        FakeEventClient client = new FakeEventClient(authentication, stop);
        DefaultHttpContext context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Items[GatewayRequestAuthenticationMiddleware.AuthenticationItemName] = authentication;

        await GatewayEventRoutes.StreamAsync(context, client, limiter);

        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.Equal("1", context.Response.Headers.RetryAfter);
        Assert.Equal(0, client.HistoryReads);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RevocationOrStalledHistoryClosesTheStreamWithinFiveSeconds(bool stall)
    {
        GatewayAuthenticationResult authentication = new GatewayAuthenticationResult { IsAuthenticated = true, OwnerUserSid = "owner", DeviceId = "phone", GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead } };
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer test-credential";
        context.Request.Headers["Last-Event-ID"] = "head";
        context.Items[GatewayRequestAuthenticationMiddleware.AuthenticationItemName] = authentication;
        context.Response.Body = new MemoryStream();
        ExpiryClient client = new ExpiryClient(authentication, stall);
        Stopwatch elapsed = Stopwatch.StartNew();

        await GatewayEventRoutes.StreamAsync(context, client, new GatewayEventStreamLimiter()).WaitAsync(TimeSpan.FromSeconds(7));

        Assert.InRange(elapsed.Elapsed.TotalSeconds, 3.5, 6.5);
        Assert.Equal(stall ? 1 : 2, client.AuthenticationCalls);
    }

    private sealed class ExpiryClient : IGatewayEventClient
    {
        private readonly GatewayAuthenticationResult _authentication;
        private readonly bool _stall;
        private int _reads;
        public int AuthenticationCalls { get; private set; }
        public ExpiryClient(GatewayAuthenticationResult authentication, bool stall) { _authentication = authentication; _stall = stall; }
        public Task<GatewayAuthenticationResult> AuthenticateAsync(GatewayAuthenticationRequest request, CancellationToken cancellationToken) => Task.FromResult(++AuthenticationCalls == 1 ? _authentication : new GatewayAuthenticationResult());
        public Task<ControlResponse> RemoteResourceAsync(ControlMessageType messageType, GatewayAuthenticationResult authentication, string payload, CancellationToken cancellationToken) => throw new InvalidOperationException("A retained cursor must not request a snapshot.");
        public async Task<GatewayEventReplay> ReadRemoteEventsAsync(GatewayAuthenticationResult authentication, string? cursor, CancellationToken cancellationToken)
        {
            if (++_reads > 1 && _stall) await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new GatewayEventReplay { HeadId = "head", NextCursor = "head" };
        }
    }

    private sealed class FakeEventClient : IGatewayEventClient
    {
        private readonly GatewayAuthenticationResult _authentication;
        private readonly CancellationTokenSource _stop;
        public int SnapshotReads { get; private set; }
        public int HistoryReads { get; private set; }
        public FakeEventClient(GatewayAuthenticationResult authentication, CancellationTokenSource stop) { _authentication = authentication; _stop = stop; }
        public Task<GatewayAuthenticationResult> AuthenticateAsync(GatewayAuthenticationRequest request, CancellationToken cancellationToken) => Task.FromResult(_authentication);
        public Task<ControlResponse> RemoteResourceAsync(ControlMessageType messageType, GatewayAuthenticationResult authentication, string payload, CancellationToken cancellationToken)
        {
            SnapshotReads++;
            Assert.Equal(ControlMessageType.ListRemoteOperations, messageType);
            return Task.FromResult(new ControlResponse { IsSuccessful = true });
        }
        public Task<GatewayEventReplay> ReadRemoteEventsAsync(GatewayAuthenticationResult authentication, string? cursor, CancellationToken cancellationToken)
        {
            HistoryReads++;
            if (cursor == null) return Task.FromResult(new GatewayEventReplay { HeadId = "head", NextCursor = "head" });
            if (cursor == "unknown") return Task.FromResult(new GatewayEventReplay { State = GatewayEventReplayState.CursorUnknown });
            if (cursor == "after-setup") { _stop.Cancel(); cancellationToken.ThrowIfCancellationRequested(); }
            return Task.FromResult(new GatewayEventReplay { HeadId = "after-setup", NextCursor = "after-setup", HasMore = true,
                Events = new[] { new GatewayEventRecord { Id = "after-setup", EventType = ControlClientEventType.OperationStatusUpdated, OperationStatus = new OperationStatus { OperationId = Guid.NewGuid() } } } });
        }
    }

    [Fact]
    public void StreamLimitsOneDeviceFourPerUserAndReleasesLeases()
    {
        GatewayEventStreamLimiter limiter = new GatewayEventStreamLimiter();
        IDisposable?[] leases = new IDisposable?[4];
        for (int index = 0; index < leases.Length; index++)
        {
            leases[index] = limiter.TryEnter(new GatewayAuthenticationResult { OwnerUserSid = "owner", DeviceId = "phone-" + index });
            Assert.NotNull(leases[index]);
        }

        Assert.Null(limiter.TryEnter(new GatewayAuthenticationResult { OwnerUserSid = "owner", DeviceId = "phone-0" }));
        Assert.Null(limiter.TryEnter(new GatewayAuthenticationResult { OwnerUserSid = "owner", DeviceId = "phone-4" }));
        using IDisposable otherUser = Assert.IsAssignableFrom<IDisposable>(limiter.TryEnter(new GatewayAuthenticationResult { OwnerUserSid = "other", DeviceId = "phone-0" }));
        leases[0]!.Dispose();
        leases[0]!.Dispose();
        using IDisposable replacement = Assert.IsAssignableFrom<IDisposable>(limiter.TryEnter(new GatewayAuthenticationResult { OwnerUserSid = "owner", DeviceId = "phone-4" }));
        for (int index = 1; index < leases.Length; index++) leases[index]!.Dispose();
    }

    [Fact]
    public void SavedResourceFrameUsesTypedNameAndRestShape()
    {
        GatewayEventRecord record = new GatewayEventRecord
        {
            Id = "v1.3.example",
            EventType = ControlClientEventType.ShortcutChanged,
            ResourceChange = new GatewaySavedResourceChange { ResourceType = "shortcut", Id = "one", Action = "deleted" }
        };

        Assert.True(GatewayEventRoutes.TryCreateEvent(record, out string name, out object? data));
        string frame = GatewayEventRoutes.FormatEvent(record.Id, name, data!);

        Assert.StartsWith("id: v1.3.example\nevent: shortcut.changed\ndata: ", frame);
        Assert.EndsWith("\n\n", frame);
        string json = frame.Substring(frame.IndexOf("data: ", StringComparison.Ordinal) + 6).Trim();
        using JsonDocument payload = JsonDocument.Parse(json);
        Assert.Equal("shortcut", payload.RootElement.GetProperty("resourceType").GetString());
        Assert.Equal("deleted", payload.RootElement.GetProperty("action").GetString());
        Assert.Equal(JsonValueKind.Null, payload.RootElement.GetProperty("href").ValueKind);
    }

    [Fact]
    public void OperationFrameUsesRestOperationStatus()
    {
        GatewayEventRecord record = new GatewayEventRecord
        {
            Id = "v1.4.example",
            EventType = ControlClientEventType.OperationStatusUpdated,
            OperationStatus = new OperationStatus { OperationId = Guid.NewGuid(), IsTerminal = true, IsSuccessful = true }
        };

        Assert.True(GatewayEventRoutes.TryCreateEvent(record, out string name, out object? data));
        string frame = GatewayEventRoutes.FormatEvent(record.Id, name, data!);
        using JsonDocument payload = JsonDocument.Parse(frame.Substring(frame.IndexOf("data: ", StringComparison.Ordinal) + 6).Trim());

        Assert.Equal("operation.updated", name);
        Assert.Equal("succeeded", payload.RootElement.GetProperty("operation").GetProperty("status").GetString());
        Assert.True(payload.RootElement.GetProperty("operation").GetProperty("isFinished").GetBoolean());
    }
}
