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

namespace DisplayMagician.ControlService;

public sealed class ControlClientPipeServer
{
    private readonly ProfileOperationRouter _profileOperationRouter;

    public ControlClientPipeServer(ProfileOperationRouter profileOperationRouter)
    {
        _profileOperationRouter = profileOperationRouter ?? throw new ArgumentNullException(nameof(profileOperationRouter));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using NamedPipeServerStream pipe = CreatePipe();
                await pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                await HandleClientAsync(pipe, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private static NamedPipeServerStream CreatePipe()
    {
        PipeSecurity security = new PipeSecurity();
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
        return NamedPipeServerStreamAcl.Create(ControlProtocol.ClientPipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, security, HandleInheritability.None);
    }

    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        using (pipe)
        {
            ControlEnvelope? request = await ControlEnvelopeSerializer.ReadAsync(pipe, cancellationToken).ConfigureAwait(false);
            if (request == null)
            {
                return;
            }

            ControlResponse response;
            if (request.ProtocolVersion != ControlProtocol.CurrentVersion)
            {
                response = new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.UnsupportedProtocolVersion, Message = "The client uses an unsupported protocol version." };
            }
            else
            {
                PipeClientIdentity identity = GetClientIdentity(pipe);
                response = request.MessageType switch
                {
                    ControlMessageType.ListProfiles => await _profileOperationRouter.ListProfilesAsync(identity.UserSid, identity.SessionId, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.ApplyProfile => await ApplyProfileAsync(identity, request, cancellationToken).ConfigureAwait(false),
                    _ => new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The requested client operation is not supported." }
                };
            }

            await ControlEnvelopeSerializer.WriteAsync(pipe, new ControlEnvelope { MessageType = request.MessageType, RequestId = request.RequestId, Payload = JsonSerializer.Serialize(response) }, cancellationToken).ConfigureAwait(false);
        }
    }

    private Task<ControlResponse> ApplyProfileAsync(PipeClientIdentity identity, ControlEnvelope request, CancellationToken cancellationToken)
    {
        ApplyProfileRequest? applyRequest = JsonSerializer.Deserialize<ApplyProfileRequest>(request.Payload);
        return applyRequest == null
            ? Task.FromResult(new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The display profile request was invalid." })
            : _profileOperationRouter.ApplyProfileAsync(identity.UserSid, identity.SessionId, applyRequest.ProfileId, cancellationToken);
    }

    private static PipeClientIdentity GetClientIdentity(NamedPipeServerStream pipe)
    {
        string? userSid = null;
        pipe.RunAsClient(() => { using WindowsIdentity identity = WindowsIdentity.GetCurrent(TokenAccessLevels.Query); userSid = identity.User?.Value; });
        if (string.IsNullOrWhiteSpace(userSid) || !GetNamedPipeClientProcessId(pipe.SafePipeHandle, out uint processId))
        {
            throw new UnauthorizedAccessException("The Control Service could not verify the pipe client identity.");
        }

        using Process process = Process.GetProcessById(checked((int)processId));
        return new PipeClientIdentity(userSid, process.SessionId);
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool GetNamedPipeClientProcessId(SafePipeHandle pipe, out uint clientProcessId);

    private sealed record PipeClientIdentity(string UserSid, int SessionId);
}
