using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

public interface ISessionLauncherClient
{
    Task<UserAgentLaunchResult> LaunchUserAgentAsync(string userSid, int sessionId, Guid requestId, Guid? operationId, string? diagnosticLogLevel, CancellationToken cancellationToken);
    Task<UserAgentLaunchResult> StopUserAgentAsync(string userSid, int sessionId, int processId, Guid requestId, CancellationToken cancellationToken);
}

public sealed class SessionLauncherClient : ISessionLauncherClient
{
    public async Task<UserAgentLaunchResult> LaunchUserAgentAsync(string userSid, int sessionId, Guid requestId, Guid? operationId, string? diagnosticLogLevel, CancellationToken cancellationToken)
    {
        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.SessionLauncherPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(ControlProtocol.ConnectionTimeout, cancellationToken).ConfigureAwait(false);
        ControlEnvelope request = new ControlEnvelope
        {
            MessageType = ControlMessageType.LaunchUserAgent,
            RequestId = requestId,
            Payload = JsonSerializer.Serialize(new UserAgentLaunchRequest { UserSid = userSid, SessionId = sessionId, OperationId = operationId, DiagnosticLogLevel = diagnosticLogLevel })
        };
        ControlEnvelope response = await SendAndReceiveAsync(pipe, request, cancellationToken).ConfigureAwait(false);
        if (response.MessageType != request.MessageType)
        {
            throw new InvalidDataException("The Session Launcher returned an invalid response.");
        }

        return JsonSerializer.Deserialize<UserAgentLaunchResult>(response.Payload)
            ?? throw new InvalidDataException("The Session Launcher returned an unreadable response.");
    }

    public async Task<UserAgentLaunchResult> StopUserAgentAsync(string userSid, int sessionId, int processId, Guid requestId, CancellationToken cancellationToken)
    {
        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.SessionLauncherPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(ControlProtocol.ConnectionTimeout, cancellationToken).ConfigureAwait(false);
        ControlEnvelope request = new ControlEnvelope { MessageType = ControlMessageType.StopUserAgent, RequestId = requestId, Payload = JsonSerializer.Serialize(new UserAgentStopRequest { UserSid = userSid, SessionId = sessionId, ProcessId = processId }) };
        ControlEnvelope response = await SendAndReceiveAsync(pipe, request, cancellationToken).ConfigureAwait(false);
        if (response.MessageType != request.MessageType)
        {
            throw new InvalidDataException("The Session Launcher returned an invalid response.");
        }

        return JsonSerializer.Deserialize<UserAgentLaunchResult>(response.Payload)
            ?? throw new InvalidDataException("The Session Launcher returned an unreadable response.");
    }

    private static async Task<ControlEnvelope> SendAndReceiveAsync(NamedPipeClientStream pipe, ControlEnvelope request, CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(ControlProtocol.ResponseTimeout);
        try
        {
            await ControlEnvelopeSerializer.WriteAsync(pipe, request, timeoutSource.Token).ConfigureAwait(false);
            ControlEnvelope? response = await ControlEnvelopeSerializer.ReadAsync(pipe, timeoutSource.Token).ConfigureAwait(false);
            if (response == null || response.RequestId != request.RequestId)
            {
                throw new InvalidDataException("The Session Launcher returned an invalid response.");
            }

            return response;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("The Session Launcher did not respond within the permitted time.");
        }
    }
}
