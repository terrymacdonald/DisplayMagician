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
    private readonly OperationStatusStore _operationStatusStore;
    private readonly ClientSyncCoordinator _clientSyncCoordinator;

    public ControlClientPipeServer(ProfileOperationRouter profileOperationRouter, OperationStatusStore operationStatusStore, ClientSyncCoordinator clientSyncCoordinator)
    {
        _profileOperationRouter = profileOperationRouter ?? throw new ArgumentNullException(nameof(profileOperationRouter));
        _operationStatusStore = operationStatusStore ?? throw new ArgumentNullException(nameof(operationStatusStore));
        _clientSyncCoordinator = clientSyncCoordinator ?? throw new ArgumentNullException(nameof(clientSyncCoordinator));
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
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
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
                    ControlMessageType.ListGames => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.ListApps => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.ListShortcuts => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.ListMessages or ControlMessageType.SetMessageReadState => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.SyncMessages => await SyncClientAsync(identity, new ClientSyncRequest { IsManual = true }, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.SyncClient => await SyncClientAsync(identity, JsonSerializer.Deserialize<ClientSyncRequest>(request.Payload) ?? new ClientSyncRequest(), cancellationToken).ConfigureAwait(false),
                    ControlMessageType.ApplyProfile => await ApplyProfileAsync(identity, request, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.StartShortcut => await StartShortcutAsync(identity, request, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.CancelOperation => await CancelOperationAsync(identity, request, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.StopAgentIfIdle => await _profileOperationRouter.StopAgentIfIdleAsync(identity.UserSid, identity.SessionId, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.CreateProfileFromCurrent or ControlMessageType.RenameProfile or ControlMessageType.DeleteProfile or ControlMessageType.UpdateProfileFromCurrent or ControlMessageType.UpdateDisplayProfileSettings => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.ListAudioProfiles or ControlMessageType.ApplyAudioProfile or ControlMessageType.CreateAudioProfileFromCurrent or ControlMessageType.RenameAudioProfile or ControlMessageType.DeleteAudioProfile or ControlMessageType.UpdateAudioProfileFromCurrent => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.GetRepositorySnapshot => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.CommitRepositorySnapshot => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                    ControlMessageType.GetOperationStatus => GetOperationStatus(identity, request),
                    ControlMessageType.ListOperationStatuses => ListOperationStatuses(identity),
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

    private Task<ControlResponse> StartShortcutAsync(PipeClientIdentity identity, ControlEnvelope request, CancellationToken cancellationToken)
    {
        StartShortcutRequest? startRequest = JsonSerializer.Deserialize<StartShortcutRequest>(request.Payload);
        return startRequest == null
            ? Task.FromResult(new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The shortcut request was invalid." })
            : _profileOperationRouter.StartShortcutAsync(identity.UserSid, identity.SessionId, startRequest.ShortcutId, cancellationToken);
    }

    private Task<ControlResponse> CancelOperationAsync(PipeClientIdentity identity, ControlEnvelope request, CancellationToken cancellationToken)
    {
        CancelOperationRequest? cancelRequest = JsonSerializer.Deserialize<CancelOperationRequest>(request.Payload);
        return cancelRequest == null
            ? Task.FromResult(new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The cancellation request was invalid." })
            : _profileOperationRouter.CancelOperationAsync(identity.UserSid, identity.SessionId, cancelRequest.OperationId, cancellationToken);
    }

    private async Task<ControlResponse> SyncClientAsync(PipeClientIdentity identity, ClientSyncRequest request, CancellationToken cancellationToken)
    {
        ClientSyncResult result = await _clientSyncCoordinator.SyncAsync(request, identity.UserSid, identity.SessionId, cancellationToken).ConfigureAwait(false);
        return new ControlResponse
        {
            IsSuccessful = result.WasDue || !request.IsManual,
            ErrorCode = result.WasDue || !request.IsManual ? ControlErrorCode.None : ControlErrorCode.InvalidRequest,
            Message = result.WasDue ? "Combined client sync completed." : "Combined client sync was not due.",
            ClientSync = result,
            MessageSync = result.MessageSync
        };
    }

    private ControlResponse GetOperationStatus(PipeClientIdentity identity, ControlEnvelope request)
    {
        OperationStatusRequest? statusRequest = JsonSerializer.Deserialize<OperationStatusRequest>(request.Payload);
        OperationStatus? status = statusRequest == null || statusRequest.OperationId == Guid.Empty ? null : _operationStatusStore.Get(identity.UserSid, statusRequest.OperationId);
        return status == null
            ? new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The requested operation was not found." }
            : new ControlResponse { IsSuccessful = true, Message = "Operation status returned.", OperationStatus = status };
    }

    private ControlResponse ListOperationStatuses(PipeClientIdentity identity)
    {
        return new ControlResponse { IsSuccessful = true, Message = "Operation statuses returned.", OperationStatuses = _operationStatusStore.GetAll(identity.UserSid) };
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
