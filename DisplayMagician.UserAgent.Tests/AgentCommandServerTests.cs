using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class AgentCommandServerTests
{
    private static ControlEnvelope CreateRequest(Guid? id = null, string payload = "{}", ControlMessageType type = ControlMessageType.StartShortcut)
        => new ControlEnvelope { RequestId = id ?? Guid.NewGuid(), MessageType = type, Payload = payload, Hello = ControlProtocol.CreateHello(ControlClientKind.ControlService, "DisplayMagician.Test") };

    [Fact]
    public async Task ConcurrentDuplicatesExecuteOnceAndCancellationCanRunIndependently()
    {
        AgentCommandServer server = new AgentCommandServer("test-unused");
        ControlEnvelope request = CreateRequest();
        TaskCompletionSource<bool> started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<bool> release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        int calls = 0;
        async Task<ControlResponse> Handler(ControlEnvelope command, CancellationToken token)
        {
            Interlocked.Increment(ref calls);
            if (command.MessageType == ControlMessageType.StartShortcut)
            {
                started.TrySetResult(true);
                await release.Task.WaitAsync(TimeSpan.FromSeconds(10), token);
            }
            return new ControlResponse { IsSuccessful = true };
        }
        Task<ControlResponse> first = server.ExecuteCachedCommandAsync(request, Handler, CancellationToken.None);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Task<ControlResponse> second = server.ExecuteCachedCommandAsync(CreateRequest(request.RequestId), Handler, CancellationToken.None);
        try
        {
            Assert.False(second.IsCompleted);
            ControlResponse cancel = await server.ExecuteCachedCommandAsync(CreateRequest(type: ControlMessageType.CancelOperation), Handler, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(cancel.IsSuccessful);
            Assert.Equal(2, calls);
            Assert.Equal(ControlErrorCode.InvalidRequest, (await server.ExecuteCachedCommandAsync(CreateRequest(request.RequestId, "changed"), Handler, CancellationToken.None)).ErrorCode);
            Assert.Equal(ControlErrorCode.InvalidRequest, (await server.ExecuteCachedCommandAsync(CreateRequest(request.RequestId, type: ControlMessageType.CancelOperation), Handler, CancellationToken.None)).ErrorCode);
        }
        finally { release.TrySetResult(true); }
        Assert.True((await first).IsSuccessful);
        Assert.True((await second).IsSuccessful);
        Assert.True((await server.ExecuteCachedCommandAsync(request, Handler, CancellationToken.None)).IsSuccessful);
        Assert.Equal(2, calls);
        Assert.Equal(ControlErrorCode.InvalidRequest, (await server.ExecuteCachedCommandAsync(CreateRequest(request.RequestId, "changed"), Handler, CancellationToken.None)).ErrorCode);
    }

    [Fact]
    public async Task CancellingDuplicateWaitDoesNotCancelTheOwner()
    {
        AgentCommandServer server = new AgentCommandServer("test-unused");
        ControlEnvelope request = CreateRequest();
        TaskCompletionSource<ControlResponse> release = new TaskCompletionSource<ControlResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        int calls = 0;
        Task<ControlResponse> Handler(ControlEnvelope command, CancellationToken token) { Interlocked.Increment(ref calls); return release.Task; }
        Task<ControlResponse> first = server.ExecuteCachedCommandAsync(request, Handler, CancellationToken.None);
        using CancellationTokenSource cancellation = new CancellationTokenSource();
        Task<ControlResponse> duplicate = server.ExecuteCachedCommandAsync(CreateRequest(request.RequestId), Handler, cancellation.Token);
        cancellation.Cancel();
        try
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => duplicate);
            Assert.False(first.IsCompleted);
        }
        finally { release.TrySetResult(new ControlResponse { IsSuccessful = true }); }
        Assert.True((await first).IsSuccessful);
        Assert.True((await server.ExecuteCachedCommandAsync(request, Handler, CancellationToken.None)).IsSuccessful);
        Assert.Equal(1, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FailedExecutionDoesNotLeaveAnInFlightEntry(bool throws)
    {
        AgentCommandServer server = new AgentCommandServer("test-unused");
        ControlEnvelope request = CreateRequest();
        TaskCompletionSource<ControlResponse> release = new TaskCompletionSource<ControlResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        int calls = 0;
        Task<ControlResponse> Handler(ControlEnvelope command, CancellationToken token) { Interlocked.Increment(ref calls); return release.Task; }
        Task<ControlResponse> first = server.ExecuteCachedCommandAsync(request, Handler, CancellationToken.None);
        Task<ControlResponse> second = server.ExecuteCachedCommandAsync(CreateRequest(request.RequestId), Handler, CancellationToken.None);
        if (throws)
        {
            release.TrySetException(new IOException("Test failure."));
            await Assert.ThrowsAsync<IOException>(() => first);
            await Assert.ThrowsAsync<IOException>(() => second);
        }
        else
        {
            release.TrySetResult(new ControlResponse { ErrorCode = ControlErrorCode.DisplayControlBusy });
            Assert.Equal(ControlErrorCode.DisplayControlBusy, (await first).ErrorCode);
            Assert.Equal(ControlErrorCode.DisplayControlBusy, (await second).ErrorCode);
        }
        Assert.Equal(1, calls);
        ControlResponse retry = await server.ExecuteCachedCommandAsync(request, (_, _) => Task.FromResult(new ControlResponse { IsSuccessful = true }), CancellationToken.None);
        Assert.True(retry.IsSuccessful);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task CacheReplayStillValidatesEveryIncomingEnvelope(int invalidField)
    {
        AgentCommandServer server = new AgentCommandServer("test-unused");
        ControlEnvelope request = CreateRequest();
        int calls = 0;
        Task<ControlResponse> Handler(ControlEnvelope command, CancellationToken token) { calls++; return Task.FromResult(new ControlResponse { IsSuccessful = true }); }
        Assert.True((await server.ExecuteCachedCommandAsync(request, Handler, CancellationToken.None)).IsSuccessful);
        ControlEnvelope invalid = CreateRequest(request.RequestId);
        if (invalidField == 0) invalid.ProtocolVersion++;
        if (invalidField == 1) invalid.Hello = null!;
        if (invalidField == 2) invalid.Hello!.RequiredCapabilities = new[] { "unsupported" };
        if (invalidField == 3) invalid.Hello!.ClientKind = ControlClientKind.DesktopApplication;
        if (invalidField == 4) invalid.Payload = null!;
        Assert.False((await server.ExecuteCachedCommandAsync(invalid, Handler, CancellationToken.None)).IsSuccessful);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task CancelledOwnerCompletesWaitersAndRemovesItsInFlightEntry()
    {
        AgentCommandServer server = new AgentCommandServer("test-unused");
        ControlEnvelope request = CreateRequest();
        using CancellationTokenSource cancellation = new CancellationTokenSource();
        TaskCompletionSource<ControlResponse> release = new TaskCompletionSource<ControlResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<ControlResponse> first = server.ExecuteCachedCommandAsync(request, (_, token) => release.Task.WaitAsync(token), cancellation.Token);
        Task<ControlResponse> second = server.ExecuteCachedCommandAsync(CreateRequest(request.RequestId), (_, _) => throw new InvalidOperationException("Duplicate must not execute."), CancellationToken.None);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => second);
        Assert.True((await server.ExecuteCachedCommandAsync(request, (_, _) => Task.FromResult(new ControlResponse { IsSuccessful = true }), CancellationToken.None)).IsSuccessful);
    }

    [Fact]
    public async Task EmptyRequestIdentityIsRejectedBeforeExecution()
    {
        bool called = false;
        ControlResponse result = await new AgentCommandServer("test-unused").ExecuteCachedCommandAsync(CreateRequest(Guid.Empty), (_, _) =>
        {
            called = true;
            return Task.FromResult(new ControlResponse { IsSuccessful = true });
        }, CancellationToken.None);
        Assert.False(called);
        Assert.Equal(ControlErrorCode.InvalidRequest, result.ErrorCode);
    }

    [Fact]
    public async Task ExecuteCommandAsync_ReturnsInvalidRequestWhenTheHandlerRejectsMalformedJson()
    {
        ControlResponse response = await AgentCommandServer.ExecuteCommandAsync(
            new ControlEnvelope { MessageType = ControlMessageType.ApplyProfile, Hello = ControlProtocol.CreateHello(ControlClientKind.ControlService, "DisplayMagician.Test") },
            (_, _) => Task.FromException<ControlResponse>(new JsonException("Invalid payload.")),
            CancellationToken.None);

        Assert.False(response.IsSuccessful);
        Assert.Equal(ControlErrorCode.InvalidRequest, response.ErrorCode);
    }
}
