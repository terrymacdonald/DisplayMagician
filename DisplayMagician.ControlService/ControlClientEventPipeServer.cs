using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.Win32.SafeHandles;
using NLog;

namespace DisplayMagician.ControlService;

public sealed class ControlClientEventPipeServer
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ControlClientEventHub _eventHub;

    public ControlClientEventPipeServer(ControlClientEventHub eventHub)
    {
        _eventHub = eventHub ?? throw new ArgumentNullException(nameof(eventHub));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            NamedPipeServerStream? pipe = null;
            try
            {
                pipe = CreatePipe();
                await pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                _ = HandleClientAsync(pipe, cancellationToken);
                pipe = null;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                pipe?.Dispose();
                return;
            }
            catch (Exception ex)
            {
                pipe?.Dispose();
                Logger.Error(ex, "ControlClientEventPipeServer/RunAsync: Unable to accept an event subscriber.");
            }
        }
    }

    private static NamedPipeServerStream CreatePipe()
    {
        PipeSecurity security = new PipeSecurity();
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
        return NamedPipeServerStreamAcl.Create(ControlProtocol.ClientEventPipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, security, HandleInheritability.None);
    }

    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        using (pipe)
        {
            try
            {
                ControlEnvelope? request = await ControlEnvelopeSerializer.ReadAsync(pipe, cancellationToken).ConfigureAwait(false);
                if (request == null || request.ProtocolVersion != ControlProtocol.CurrentVersion || request.MessageType != ControlMessageType.SubscribeClientEvents)
                {
                    return;
                }

                PipeClientIdentity identity = GetClientIdentity(pipe);
                await SendResponseAsync(pipe, request.RequestId, new ControlResponse { IsSuccessful = true, Message = "Client event subscription accepted." }, cancellationToken).ConfigureAwait(false);
                using ControlClientEventSubscription subscription = _eventHub.Subscribe(identity.UserSid, identity.SessionId);
                await foreach (ControlClientEvent clientEvent in subscription.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
                {
                    await ControlEnvelopeSerializer.WriteAsync(pipe, new ControlEnvelope
                    {
                        MessageType = ControlMessageType.ClientEvent,
                        Payload = JsonSerializer.Serialize(clientEvent)
                    }, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException || ex is JsonException || ex is EndOfStreamException)
            {
                Logger.Debug(ex, "ControlClientEventPipeServer/HandleClientAsync: Client event subscription ended or sent an invalid request.");
            }
        }
    }

    private static PipeClientIdentity GetClientIdentity(NamedPipeServerStream pipe)
    {
        string? userSid = null;
        pipe.RunAsClient(() => { using WindowsIdentity identity = WindowsIdentity.GetCurrent(TokenAccessLevels.Query); userSid = identity.User?.Value; });
        if (string.IsNullOrWhiteSpace(userSid) || !GetNamedPipeClientProcessId(pipe.SafePipeHandle, out uint processId))
        {
            throw new UnauthorizedAccessException("The Control Service could not verify the event subscriber identity.");
        }

        using Process process = Process.GetProcessById(checked((int)processId));
        return new PipeClientIdentity(userSid, process.SessionId);
    }

    private static Task SendResponseAsync(NamedPipeServerStream pipe, Guid requestId, ControlResponse response, CancellationToken cancellationToken)
    {
        return ControlEnvelopeSerializer.WriteAsync(pipe, new ControlEnvelope { MessageType = ControlMessageType.SubscribeClientEvents, RequestId = requestId, Payload = JsonSerializer.Serialize(response) }, cancellationToken);
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool GetNamedPipeClientProcessId(SafePipeHandle pipe, out uint clientProcessId);

    private sealed record PipeClientIdentity(string UserSid, int SessionId);
}
