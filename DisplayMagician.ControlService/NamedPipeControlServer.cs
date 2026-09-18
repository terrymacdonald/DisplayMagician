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

public sealed class NamedPipeControlServer
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly ControlStateCoordinator _coordinator;

    public NamedPipeControlServer(ControlStateCoordinator coordinator)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
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
            }
            catch (Exception ex)
            {
                pipe?.Dispose();
                _logger.Error(ex, "NamedPipeControlServer/RunAsync: Unable to accept a Control Service pipe client.");
            }
        }
    }

    private static NamedPipeServerStream CreatePipe()
    {
        SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        PipeSecurity pipeSecurity = new PipeSecurity();
        pipeSecurity.AddAccessRule(new PipeAccessRule(everyone, PipeAccessRights.ReadWrite, AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(
            ControlProtocol.ServicePipeName,
            PipeDirection.InOut,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous,
            0,
            0,
            pipeSecurity,
            HandleInheritability.None);
    }

    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        using (pipe)
        {
            try
            {
                ControlEnvelope? envelope = await ControlEnvelopeSerializer.ReadAsync(pipe, cancellationToken).ConfigureAwait(false);
                if (envelope == null)
                {
                    return;
                }

                PipeClientIdentity identity = GetClientIdentity(pipe);

                if (envelope.ProtocolVersion != ControlProtocol.CurrentVersion)
                {
                    await SendResultAsync(pipe, envelope.RequestId, false, ControlErrorCode.UnsupportedProtocolVersion, "The client uses an unsupported protocol version.", cancellationToken).ConfigureAwait(false);
                    return;
                }

                if (envelope.MessageType != ControlMessageType.AgentRegistration)
                {
                    await SendResultAsync(pipe, envelope.RequestId, false, ControlErrorCode.InvalidRequest, "The first pipe message must be an Agent registration.", cancellationToken).ConfigureAwait(false);
                    return;
                }

                AgentRegistration? registration = JsonSerializer.Deserialize<AgentRegistration>(envelope.Payload);
                if (registration == null || !string.Equals(registration.UserSid, identity.UserSid, StringComparison.OrdinalIgnoreCase) || registration.SessionId != identity.SessionId || registration.ProcessId != identity.ProcessId)
                {
                    _logger.Warn("NamedPipeControlServer/HandleClientAsync: Rejected Agent registration because its claimed identity did not match the Windows pipe client. ProcessId={0}", identity.ProcessId);
                    await SendResultAsync(pipe, envelope.RequestId, false, ControlErrorCode.CallerIdentityMismatch, "The claimed Agent identity does not match the Windows pipe client.", cancellationToken).ConfigureAwait(false);
                    return;
                }

                _coordinator.RegisterAgent(registration, DateTime.UtcNow);
                _logger.Info("NamedPipeControlServer/HandleClientAsync: Registered User Agent for SID {0}, session {1}, process {2}.", identity.UserSid, identity.SessionId, identity.ProcessId);
                await SendResultAsync(pipe, envelope.RequestId, true, ControlErrorCode.None, "Agent registration accepted.", cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Service shutdown is expected and does not require an error log.
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "NamedPipeControlServer/HandleClientAsync: Pipe client processing failed.");
            }
        }
    }

    private static PipeClientIdentity GetClientIdentity(NamedPipeServerStream pipe)
    {
        string? userSid = null;
        pipe.RunAsClient(() =>
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent(TokenAccessLevels.Query);
            userSid = identity.User?.Value;
        });

        if (string.IsNullOrWhiteSpace(userSid) || !GetNamedPipeClientProcessId(pipe.SafePipeHandle, out uint processId))
        {
            throw new UnauthorizedAccessException("The Control Service could not verify the named-pipe client identity.");
        }

        using Process process = Process.GetProcessById(checked((int)processId));
        return new PipeClientIdentity(userSid, process.SessionId, checked((int)processId));
    }

    private static Task SendResultAsync(NamedPipeServerStream pipe, Guid requestId, bool isSuccessful, ControlErrorCode errorCode, string message, CancellationToken cancellationToken)
    {
        ControlEnvelope response = new ControlEnvelope
        {
            MessageType = ControlMessageType.GetServiceStatus,
            RequestId = requestId,
            Payload = JsonSerializer.Serialize(new ControlResponse
            {
                IsSuccessful = isSuccessful,
                ErrorCode = errorCode,
                Message = message
            })
        };

        return ControlEnvelopeSerializer.WriteAsync(pipe, response, cancellationToken);
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool GetNamedPipeClientProcessId(SafePipeHandle pipe, out uint clientProcessId);

    private sealed class PipeClientIdentity
    {
        public PipeClientIdentity(string userSid, int sessionId, int processId)
        {
            UserSid = userSid;
            SessionId = sessionId;
            ProcessId = processId;
        }

        public string UserSid { get; }

        public int SessionId { get; }

        public int ProcessId { get; }
    }

}
