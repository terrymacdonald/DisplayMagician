using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagician.UserAgent;

public static class AgentCommandPipe
{
    public static string CreateName(int processId)
    {
        if (processId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(processId));
        }

        return $"{ControlProtocol.AgentCommandPipePrefix}{processId}";
    }
}

public sealed class AgentCommandServer
{
    private readonly string _pipeName;

    public AgentCommandServer(string pipeName)
    {
        _pipeName = string.IsNullOrWhiteSpace(pipeName) ? throw new ArgumentException("An Agent command pipe name is required.", nameof(pipeName)) : pipeName;
    }

    public async Task RunAsync(Func<ControlEnvelope, CancellationToken, Task<ControlResponse>> commandHandler, Func<bool> shouldStop, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(commandHandler);
        ArgumentNullException.ThrowIfNull(shouldStop);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using NamedPipeServerStream pipe = CreatePipe();
                await pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                if (!IsControlService(pipe))
                {
                    continue;
                }

                ControlEnvelope? request = await ControlEnvelopeSerializer.ReadAsync(pipe, cancellationToken).ConfigureAwait(false);
                if (request == null)
                {
                    continue;
                }

                ControlResponse response = request.ProtocolVersion == ControlProtocol.CurrentVersion
                    ? await commandHandler(request, cancellationToken).ConfigureAwait(false)
                    : new ControlResponse
                    {
                        IsSuccessful = false,
                        ErrorCode = ControlErrorCode.UnsupportedProtocolVersion,
                        Message = "The Control Service uses an unsupported protocol version."
                    };

                await ControlEnvelopeSerializer.WriteAsync(pipe, new ControlEnvelope
                {
                    MessageType = request.MessageType,
                    RequestId = request.RequestId,
                    Payload = JsonSerializer.Serialize(response)
                }, cancellationToken).ConfigureAwait(false);

                if (shouldStop())
                {
                    return;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private NamedPipeServerStream CreatePipe()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        SecurityIdentifier userSid = identity.User ?? throw new InvalidOperationException("The current Windows user has no SID.");
        SecurityIdentifier localServiceSid = new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null);
        PipeSecurity security = new PipeSecurity();
        security.AddAccessRule(new PipeAccessRule(userSid, PipeAccessRights.FullControl, AccessControlType.Allow));
        security.AddAccessRule(new PipeAccessRule(localServiceSid, PipeAccessRights.ReadWrite, AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, security, HandleInheritability.None);
    }

    private static bool IsControlService(NamedPipeServerStream pipe)
    {
        bool isLocalService = false;
        pipe.RunAsClient(() =>
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            isLocalService = identity.User?.IsWellKnown(WellKnownSidType.LocalServiceSid) == true;
        });
        return isLocalService;
    }
}
