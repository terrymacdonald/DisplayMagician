using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.SessionLauncher;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class SessionLauncherProtocolTests
{
    [Theory]
    [InlineData(ControlMessageType.LaunchUserAgent, true)]
    [InlineData(ControlMessageType.StopUserAgent, true)]
    [InlineData(ControlMessageType.LaunchUserAgent, false)]
    [InlineData(ControlMessageType.StopUserAgent, false)]
    public async Task ClientAndBrokerNegotiateBeforeDispatchingTheSelectedCommand(ControlMessageType type, bool succeeds)
    {
        ControlEnvelope request = CreateRequest(type);
        int launches = 0;
        int stops = 0;
        UserAgentLaunchResult Result() => new UserAgentLaunchResult { IsSuccessful = succeeds, Message = succeeds ? "completed" : "execution failed" };
        UserAgentLaunchResult result = await ExchangeAsync(request, received =>
        {
            Assert.Equal(request.RequestId, received.RequestId);
            Assert.Equal(ControlClientKind.ControlService, received.Hello.ClientKind);
            Assert.Contains(ControlCapabilities.SessionLauncher, received.Hello.RequiredCapabilities);
            UserAgentLaunchResult response = SessionLauncherPipeServer.ExecuteCommand(received, true, launch =>
            {
                launches++;
                Assert.Equal("owner", launch.UserSid);
                Assert.Equal(7, launch.SessionId);
                Assert.Equal(Guid.Parse("7fe7d61d-06c8-454e-af5b-846ea8da1cf1"), launch.OperationId);
                return Result();
            }, stop =>
            {
                stops++;
                Assert.Equal("owner", stop.UserSid);
                Assert.Equal(7, stop.SessionId);
                Assert.Equal(77, stop.ProcessId);
                return Result();
            });
            return Reply(received, response);
        });

        Assert.Equal(succeeds, result.IsSuccessful);
        Assert.Equal(succeeds ? ControlErrorCode.None : ControlErrorCode.ExecutionFailed, result.ErrorCode);
        Assert.Equal(type == ControlMessageType.LaunchUserAgent ? 1 : 0, launches);
        Assert.Equal(type == ControlMessageType.StopUserAgent ? 1 : 0, stops);
        Assert.True(ControlProtocol.IsCompatibleWelcome(request.Hello, result.ProtocolWelcome));
        Assert.Equal("SessionLauncher", result.ProtocolWelcome!.EndpointKind);
    }

    [Theory]
    [InlineData("caller", ControlErrorCode.Unauthorized)]
    [InlineData("missing-hello", ControlErrorCode.InvalidRequest)]
    [InlineData("outer-version", ControlErrorCode.UnsupportedProtocolVersion)]
    [InlineData("range", ControlErrorCode.IncompatibleProtocolVersion)]
    [InlineData("required", ControlErrorCode.RequiredCapabilityUnavailable)]
    [InlineData("client-kind", ControlErrorCode.InvalidRequest)]
    [InlineData("method", ControlErrorCode.InvalidRequest)]
    [InlineData("payload", ControlErrorCode.InvalidRequest)]
    [InlineData("stop-payload", ControlErrorCode.InvalidRequest)]
    [InlineData("empty-payload", ControlErrorCode.InvalidRequest)]
    public void InvalidRequestsCannotInvokeLaunchOrStop(string invalid, ControlErrorCode expected)
    {
        ControlEnvelope request = CreateRequest(ControlMessageType.LaunchUserAgent);
        switch (invalid)
        {
            case "missing-hello": request.Hello = null!; break;
            case "outer-version": request.ProtocolVersion++; break;
            case "range": request.Hello.MinimumProtocolVersion = request.Hello.MaximumProtocolVersion = ControlProtocol.CurrentVersion + 1; break;
            case "required": request.Hello.RequiredCapabilities = new[] { "unavailable-capability" }; break;
            case "client-kind": request.Hello.ClientKind = ControlClientKind.DesktopApplication; break;
            case "method": request.MessageType = ControlMessageType.ApplyProfile; break;
            case "payload": request.Payload = "{"; break;
            case "stop-payload": request.MessageType = ControlMessageType.StopUserAgent; request.Payload = "{"; break;
            case "empty-payload": request.Payload = null!; break;
        }
        int calls = 0;
        UserAgentLaunchResult result = SessionLauncherPipeServer.ExecuteCommand(request, invalid != "caller",
            _ => { calls++; return new UserAgentLaunchResult { IsSuccessful = true }; },
            _ => { calls++; return new UserAgentLaunchResult { IsSuccessful = true }; });
        Assert.False(result.IsSuccessful);
        Assert.Equal(expected, result.ErrorCode);
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("version")]
    [InlineData("capability")]
    [InlineData("endpoint")]
    [InlineData("request-id")]
    [InlineData("message-type")]
    [InlineData("envelope-version")]
    [InlineData("json")]
    [InlineData("json-null")]
    public async Task ClientRejectsUnnegotiatedOrMismatchedSuccessReplies(string invalid)
    {
        ControlEnvelope request = CreateRequest(ControlMessageType.LaunchUserAgent);
        await Assert.ThrowsAsync<InvalidDataException>(() => ExchangeAsync(request, received =>
        {
            Assert.True(ControlProtocol.TryCreateWelcome(received.Hello, "SessionLauncher", ControlProtocol.SessionLauncherCapabilities,
                out ProtocolWelcome? welcome, out _, out _));
            UserAgentLaunchResult result = new UserAgentLaunchResult { IsSuccessful = true, ProtocolWelcome = welcome };
            switch (invalid)
            {
                case "missing": result.ProtocolWelcome = null; break;
                case "version": welcome!.SelectedProtocolVersion = ControlProtocol.CurrentVersion + 1; break;
                case "capability": welcome!.SupportedCapabilities = new[] { ControlCapabilities.ProtocolNegotiation }; break;
                case "endpoint": welcome!.EndpointKind = "UserAgent"; break;
            }
            ControlEnvelope response = Reply(received, result);
            switch (invalid)
            {
                case "request-id": response.RequestId = Guid.NewGuid(); break;
                case "message-type": response.MessageType = ControlMessageType.GetServiceStatus; break;
                case "envelope-version": response.ProtocolVersion++; break;
                case "json": response.Payload = "{"; break;
                case "json-null": response.Payload = "null"; break;
            }
            return response;
        }));
    }

    [Fact]
    public async Task ClientRejectsADisconnectBeforeAnyReply()
    {
        using MemoryStream disconnected = new MemoryStream();
        await Assert.ThrowsAsync<InvalidDataException>(() => SessionLauncherClient.SendAndReceiveAsync(disconnected,
            CreateRequest(ControlMessageType.StopUserAgent), CancellationToken.None));
    }

    private static ControlEnvelope CreateRequest(ControlMessageType type)
    {
        ProtocolHello hello = ControlProtocol.CreateHello(ControlClientKind.ControlService, "DisplayMagician.ControlService");
        hello.RequiredCapabilities = ControlProtocol.SessionLauncherCapabilities.ToArray();
        return new ControlEnvelope
        {
            MessageType = type, Hello = hello,
            Payload = type == ControlMessageType.LaunchUserAgent
                ? JsonSerializer.Serialize(new UserAgentLaunchRequest { UserSid = "owner", SessionId = 7, OperationId = Guid.Parse("7fe7d61d-06c8-454e-af5b-846ea8da1cf1") })
                : JsonSerializer.Serialize(new UserAgentStopRequest { UserSid = "owner", SessionId = 7, ProcessId = 77 })
        };
    }

    private static ControlEnvelope Reply(ControlEnvelope request, UserAgentLaunchResult result)
        => new ControlEnvelope { MessageType = request.MessageType, RequestId = request.RequestId, Payload = JsonSerializer.Serialize(result) };

    private static async Task<UserAgentLaunchResult> ExchangeAsync(ControlEnvelope request, Func<ControlEnvelope, ControlEnvelope> handler)
    {
        string name = "DisplayMagician-SessionLauncher-Test-" + Guid.NewGuid().ToString("N");
        using CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using NamedPipeServerStream server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
        using NamedPipeClientStream client = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
        Task reply = Task.Run(async () =>
        {
            await server.WaitForConnectionAsync(timeout.Token);
            ControlEnvelope received = (await ControlEnvelopeSerializer.ReadAsync(server, timeout.Token))!;
            await ControlEnvelopeSerializer.WriteAsync(server, handler(received), timeout.Token);
        });
        try
        {
            await client.ConnectAsync(timeout.Token);
            return await SessionLauncherClient.SendAndReceiveAsync(client, request, timeout.Token);
        }
        finally { await reply; }
    }
}
