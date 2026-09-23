using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using NLog;

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
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);
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

                ControlEnvelope? request = await ReadEnvelopeWithTimeoutAsync(pipe, cancellationToken).ConfigureAwait(false);
                if (request == null)
                {
                    continue;
                }

                using IDisposable requestScope = SupportLogScope.BeginRequest(request.RequestId);
                ControlResponse response = await ExecuteCommandAsync(request, commandHandler, cancellationToken).ConfigureAwait(false);

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
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException || ex is JsonException || ex is EndOfStreamException || ex is TimeoutException)
            {
                Logger.Debug(ex, "AgentCommandServer/RunAsync: Control Service command connection ended or was invalid.");
            }
        }
    }

    internal static async Task<ControlResponse> ExecuteCommandAsync(ControlEnvelope request, Func<ControlEnvelope, CancellationToken, Task<ControlResponse>> commandHandler, CancellationToken cancellationToken)
    {
        try
        {
            return request.ProtocolVersion == ControlProtocol.CurrentVersion
                ? await commandHandler(request, cancellationToken).ConfigureAwait(false)
                : new ControlResponse
                {
                    IsSuccessful = false,
                    ErrorCode = ControlErrorCode.UnsupportedProtocolVersion,
                    Message = "The Control Service uses an unsupported protocol version."
                };
        }
        catch (JsonException ex)
        {
            Logger.Warn(ex, "AgentCommandServer/ExecuteCommandAsync: The Control Service sent an invalid JSON request payload.");
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The Control Service request payload was invalid." };
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
        if (!isLocalService || !GetNamedPipeClientProcessId(pipe.SafePipeHandle, out uint processId))
        {
            return false;
        }

        try
        {
            using System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(checked((int)processId));
            string expectedPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "ControlService", "DisplayMagician.ControlService.exe"));
            return string.Equals(process.MainModule?.FileName, expectedPath, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception || ex is UnauthorizedAccessException)
        {
            Logger.Warn(ex, "AgentCommandServer/IsControlService: Could not verify the Control Service executable for pipe process {0}.", processId);
            return false;
        }
    }

    private static async Task<ControlEnvelope?> ReadEnvelopeWithTimeoutAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(RequestTimeout);
        try
        {
            return await ControlEnvelopeSerializer.ReadAsync(pipe, timeoutSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("The Control Service did not send a complete Agent command in time.", ex);
        }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool GetNamedPipeClientProcessId(Microsoft.Win32.SafeHandles.SafePipeHandle pipe, out uint clientProcessId);
}
