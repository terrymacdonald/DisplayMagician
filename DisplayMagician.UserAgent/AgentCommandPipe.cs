using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
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
    private readonly Dictionary<Guid, (ControlMessageType MessageType, string Payload, ControlResponse Response, DateTime CompletedUtc)> _successfulResponses = new Dictionary<Guid, (ControlMessageType, string, ControlResponse, DateTime)>();

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
                ControlResponse response;
                lock (_successfulResponses)
                {
                    RemoveExpiredResponses();
                    if (_successfulResponses.TryGetValue(request.RequestId, out var replay))
                    {
                        response = replay.MessageType == request.MessageType && string.Equals(replay.Payload, request.Payload, StringComparison.Ordinal)
                            ? replay.Response
                            : new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A request ID cannot be reused for a different User Agent command." };
                    }
                    else
                    {
                        response = null!;
                    }
                }
                if (response == null)
                {
                    response = await ExecuteCommandAsync(request, commandHandler, cancellationToken).ConfigureAwait(false);
                    if (response.IsSuccessful)
                    {
                        lock (_successfulResponses)
                        {
                            RemoveExpiredResponses();
                            _successfulResponses[request.RequestId] = (request.MessageType, request.Payload, response, DateTime.UtcNow);
                        }
                    }
                }

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
            catch (EndOfStreamException ex)
            {
                Logger.Debug(ex, "AgentCommandServer/RunAsync: The Control Service closed the User Agent command pipe before completing a request.");
            }
            catch (IOException ex)
            {
                Logger.Debug(ex, "AgentCommandServer/RunAsync: The User Agent command pipe was disconnected during a Control Service request.");
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Warn(ex, "AgentCommandServer/RunAsync: Rejected an unauthorised caller on the User Agent command pipe.");
            }
            catch (TimeoutException ex)
            {
                Logger.Warn(ex, "AgentCommandServer/RunAsync: The Control Service did not send a complete User Agent command before the request timeout.");
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is JsonException)
            {
                Logger.Warn(ex, "AgentCommandServer/RunAsync: The Control Service sent an invalid User Agent command request.");
            }
        }
    }

    internal static async Task<ControlResponse> ExecuteCommandAsync(ControlEnvelope request, Func<ControlEnvelope, CancellationToken, Task<ControlResponse>> commandHandler, CancellationToken cancellationToken)
    {
        try
        {
            if (request.ProtocolVersion != ControlProtocol.CurrentVersion)
            {
                return new ControlResponse
                {
                    IsSuccessful = false,
                    ErrorCode = ControlErrorCode.UnsupportedProtocolVersion,
                    Message = "The Control Service uses an unsupported protocol version."
                };
            }

            if (!ControlProtocol.TryCreateWelcome(request.Hello, "UserAgent", ControlProtocol.UserAgentCapabilities, out ProtocolWelcome? welcome, out ControlErrorCode negotiationError, out string negotiationMessage))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = negotiationError, Message = negotiationMessage };
            }

            ControlResponse response = await commandHandler(request, cancellationToken).ConfigureAwait(false);
            response.ProtocolWelcome = welcome;
            return response;
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
        SecurityIdentifier localSystemSid = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        PipeSecurity security = new PipeSecurity();
        security.AddAccessRule(new PipeAccessRule(userSid, PipeAccessRights.FullControl, AccessControlType.Allow));
        security.AddAccessRule(new PipeAccessRule(localSystemSid, PipeAccessRights.ReadWrite, AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, security, HandleInheritability.None);
    }

    private static bool IsControlService(NamedPipeServerStream pipe)
    {
        bool isLocalSystem = false;
        pipe.RunAsClient(() =>
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            isLocalSystem = identity.User?.IsWellKnown(WellKnownSidType.LocalSystemSid) == true;
        });
        if (!isLocalSystem || !GetNamedPipeClientProcessId(pipe.SafePipeHandle, out uint processId))
        {
            Logger.Warn("AgentCommandServer/IsControlService: Rejected a command-pipe caller because it was not the LocalSystem Control Service.");
            return false;
        }

        try
        {
            using System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(checked((int)processId));
            string expectedPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "ControlService", "DisplayMagician.ControlService.exe"));
            bool isControlService = string.Equals(process.MainModule?.FileName, expectedPath, StringComparison.OrdinalIgnoreCase);
            if (!isControlService)
            {
                Logger.Warn("AgentCommandServer/IsControlService: Rejected command-pipe caller process {0} because it was not the installed Control Service executable.", processId);
            }

            return isControlService;
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

    private void RemoveExpiredResponses()
    {
        DateTime cutoffUtc = DateTime.UtcNow.AddHours(-24);
        foreach (Guid requestId in _successfulResponses.Where(pair => pair.Value.CompletedUtc < cutoffUtc).Select(pair => pair.Key).ToArray())
        {
            _successfulResponses.Remove(requestId);
        }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool GetNamedPipeClientProcessId(Microsoft.Win32.SafeHandles.SafePipeHandle pipe, out uint clientProcessId);
}
