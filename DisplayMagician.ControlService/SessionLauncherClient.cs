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
    Task<UserAgentLaunchResult> LaunchUserAgentAsync(string userSid, int sessionId, CancellationToken cancellationToken);
}

public sealed class SessionLauncherClient : ISessionLauncherClient
{
    public async Task<UserAgentLaunchResult> LaunchUserAgentAsync(string userSid, int sessionId, CancellationToken cancellationToken)
    {
        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.SessionLauncherPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
        ControlEnvelope request = new ControlEnvelope
        {
            MessageType = ControlMessageType.LaunchUserAgent,
            Payload = JsonSerializer.Serialize(new UserAgentLaunchRequest { UserSid = userSid, SessionId = sessionId })
        };
        await ControlEnvelopeSerializer.WriteAsync(pipe, request, cancellationToken).ConfigureAwait(false);
        ControlEnvelope? response = await ControlEnvelopeSerializer.ReadAsync(pipe, cancellationToken).ConfigureAwait(false);
        if (response == null || response.RequestId != request.RequestId)
        {
            throw new InvalidDataException("The Session Launcher returned an invalid response.");
        }

        return JsonSerializer.Deserialize<UserAgentLaunchResult>(response.Payload)
            ?? throw new InvalidDataException("The Session Launcher returned an unreadable response.");
    }
}
