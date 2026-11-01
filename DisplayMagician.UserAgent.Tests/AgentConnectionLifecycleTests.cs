using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class AgentConnectionLifecycleTests
{
    [Fact]
    public async Task RegistrationCompletesBeforeConcurrentRequestsUseTheConnection()
    {
        string name = "DisplayMagician.Test." + Guid.NewGuid().ToString("N");
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        using NamedPipeServerStream server = new(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        ControlServiceClient client = new(name);
        AgentRegistration registration = new();
        Task run = client.RunAsync(registration, TimeSpan.FromHours(1), false, false, null, timeout.Token);
        await server.WaitForConnectionAsync(timeout.Token);
        ControlEnvelope first = (await ControlEnvelopeSerializer.ReadAsync(server, timeout.Token))!;
        Assert.Equal(ControlMessageType.AgentRegistration, first.MessageType);
        Task<ControlResponse> concurrent = client.ReportAgentOperationStateAsync(registration, AgentOperationState.Idle, timeout.Token);
        Assert.False(concurrent.IsCompleted);
        await ReplyAsync(server, first, timeout.Token);
        ControlEnvelope second = (await ControlEnvelopeSerializer.ReadAsync(server, timeout.Token))!;
        Assert.Equal(ControlMessageType.AgentHeartbeat, second.MessageType);
        await ReplyAsync(server, second, timeout.Token);
        Assert.True((await concurrent).IsSuccessful);
        timeout.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => run);
    }

    [Fact]
    public async Task CancellingAnExchangeDiscardsTheConnectionBeforeAnotherRequest()
    {
        string name = "DisplayMagician.Test." + Guid.NewGuid().ToString("N");
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        using NamedPipeServerStream server = new(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        ControlServiceClient client = new(name);
        AgentRegistration registration = new();
        Task run = client.RunAsync(registration, TimeSpan.FromHours(1), false, false, null, timeout.Token);
        await server.WaitForConnectionAsync(timeout.Token);
        ControlEnvelope first = (await ControlEnvelopeSerializer.ReadAsync(server, timeout.Token))!;
        await ReplyAsync(server, first, timeout.Token);
        using CancellationTokenSource requestCancellation = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
        Task<ControlResponse> pending = client.ReportAgentOperationStateAsync(registration, AgentOperationState.Running, requestCancellation.Token);
        Assert.NotNull(await ControlEnvelopeSerializer.ReadAsync(server, timeout.Token));
        requestCancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        await Assert.ThrowsAsync<IOException>(() => client.ReportAgentOperationStateAsync(registration, AgentOperationState.Idle, timeout.Token));
        timeout.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => run);
    }

    [Theory]
    [InlineData("version")]
    [InlineData("endpoint")]
    [InlineData("json")]
    [InlineData("request")]
    public async Task InvalidResponsesAreRejected(string invalidField)
    {
        string name = "DisplayMagician.Test." + Guid.NewGuid().ToString("N");
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        using NamedPipeServerStream server = new(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        using NamedPipeClientStream client = new(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
        Task connect = client.ConnectAsync(timeout.Token);
        await server.WaitForConnectionAsync(timeout.Token);
        await connect;
        ControlEnvelope request = new() { MessageType = ControlMessageType.AgentHeartbeat };
        Task<ControlResponse> response = ControlServiceClient.SendAndReceiveAsync(client, request, timeout.Token);
        ControlEnvelope received = (await ControlEnvelopeSerializer.ReadAsync(server, timeout.Token))!;
        Assert.True(ControlProtocol.TryCreateWelcome(received.Hello, invalidField == "endpoint" ? "UserAgent" : "ControlService",
            ControlProtocol.ControlServiceCapabilities, out ProtocolWelcome? welcome, out _, out _));
        await ControlEnvelopeSerializer.WriteAsync(server, new ControlEnvelope
        {
            ProtocolVersion = invalidField == "version" ? ControlProtocol.CurrentVersion + 1 : ControlProtocol.CurrentVersion,
            RequestId = invalidField == "request" ? Guid.NewGuid() : received.RequestId,
            MessageType = received.MessageType,
            Payload = invalidField == "json" ? "{" : JsonSerializer.Serialize(new ControlResponse { IsSuccessful = true, ProtocolWelcome = welcome })
        }, timeout.Token);
        await Assert.ThrowsAsync<InvalidDataException>(() => response);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ListenerExitDoesNotLeaveMigrationWaitingForever(bool faulted)
    {
        TaskCompletionSource<ControlResponse> migration = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task listener = faulted ? Task.FromException(new IOException("Listener failed.")) : Task.CompletedTask;
        await Assert.ThrowsAsync<IOException>(() => Program.AwaitMigrationAsync(migration.Task, listener));
        Assert.False(migration.Task.IsCompleted);
    }

    [Fact]
    public async Task ListenerCreationFailureReachesStartupInsteadOfRetryingForever()
    {
        AgentCommandServer server = new("unused", () => throw new IOException("Cannot create listener."));
        Task run = server.RunAsync((_, _) => Task.FromResult(new ControlResponse()), () => false, CancellationToken.None);
        await Assert.ThrowsAsync<IOException>(() => run);
        TaskCompletionSource<ControlResponse> migration = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await Assert.ThrowsAsync<IOException>(() => Program.AwaitMigrationAsync(migration.Task, run));
    }

    private static Task ReplyAsync(Stream pipe, ControlEnvelope request, CancellationToken cancellationToken)
    {
        Assert.True(ControlProtocol.TryCreateWelcome(request.Hello, "ControlService", ControlProtocol.ControlServiceCapabilities,
            out ProtocolWelcome? welcome, out _, out _));
        return ControlEnvelopeSerializer.WriteAsync(pipe, new ControlEnvelope
        {
            MessageType = request.MessageType, RequestId = request.RequestId,
            Payload = JsonSerializer.Serialize(new ControlResponse { IsSuccessful = true, ProtocolWelcome = welcome })
        }, cancellationToken);
    }

    [Fact]
    public async Task CommandListenerWaitsForAcceptedHandlerCleanupDuringShutdown()
    {
        string name = "DisplayMagician.Test." + Guid.NewGuid().ToString("N");
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        using CancellationTokenSource shutdown = new();
        TaskCompletionSource<bool> entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<bool> cancelled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<bool> finish = new(TaskCreationOptions.RunContinuationsAsynchronously);
        AgentCommandServer server = new(name, () => new NamedPipeServerStream(name, PipeDirection.InOut, 4, PipeTransmissionMode.Byte, PipeOptions.Asynchronous));
        Task run = server.RunAsync(async (_, token) =>
        {
            entered.TrySetResult(true);
            try { await Task.Delay(Timeout.Infinite, token); }
            catch (OperationCanceledException) { cancelled.TrySetResult(true); }
            await finish.Task.WaitAsync(timeout.Token);
            return new ControlResponse { IsSuccessful = true };
        }, () => false, shutdown.Token);
        using NamedPipeClientStream client = new(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
        try
        {
            await client.ConnectAsync(timeout.Token);
            await ControlEnvelopeSerializer.WriteAsync(client, new ControlEnvelope
            {
                MessageType = ControlMessageType.ListProfiles,
                Hello = ControlProtocol.CreateHello(ControlClientKind.ControlService, "DisplayMagician.Test")
            }, timeout.Token);
            await entered.Task.WaitAsync(timeout.Token);
            shutdown.Cancel();
            await cancelled.Task.WaitAsync(timeout.Token);
            Assert.False(run.IsCompleted);
        }
        finally
        {
            finish.TrySetResult(true);
            shutdown.Cancel();
            await run.WaitAsync(timeout.Token);
        }
    }

    [Fact]
    public async Task AcceptedStopStillStopsListenerWhenCallerDisconnectsBeforeReply()
    {
        string name = "DisplayMagician.Test." + Guid.NewGuid().ToString("N");
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        TaskCompletionSource<bool> entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<bool> finish = new(TaskCreationOptions.RunContinuationsAsynchronously);
        bool stopped = false;
        AgentCommandServer server = new(name, () => new NamedPipeServerStream(name, PipeDirection.InOut, 4, PipeTransmissionMode.Byte, PipeOptions.Asynchronous));
        Task run = server.RunAsync(async (_, token) =>
        {
            entered.TrySetResult(true);
            await finish.Task.WaitAsync(token);
            stopped = true;
            return new ControlResponse { IsSuccessful = true };
        }, () => stopped, timeout.Token);
        using NamedPipeClientStream client = new(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
        try
        {
            await client.ConnectAsync(timeout.Token);
            await ControlEnvelopeSerializer.WriteAsync(client, new ControlEnvelope
            {
                MessageType = ControlMessageType.StopAgentIfIdle,
                Hello = ControlProtocol.CreateHello(ControlClientKind.ControlService, "DisplayMagician.Test")
            }, timeout.Token);
            await entered.Task.WaitAsync(timeout.Token);
            client.Dispose();
            finish.TrySetResult(true);
            await run.WaitAsync(timeout.Token);
            Assert.True(stopped);
        }
        finally
        {
            finish.TrySetResult(true);
            timeout.Cancel();
            await run;
        }
    }
}
