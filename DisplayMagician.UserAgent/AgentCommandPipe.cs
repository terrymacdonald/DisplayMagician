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
    private const int MaximumConnectedCommandClients = 4;
    private readonly string _pipeName;
    private readonly SemaphoreSlim _connectedClientSlots = new SemaphoreSlim(MaximumConnectedCommandClients, MaximumConnectedCommandClients);
    private readonly Dictionary<Guid, (ControlMessageType MessageType, string Payload, ControlResponse Response, DateTime CompletedUtc)> _successfulResponses = new Dictionary<Guid, (ControlMessageType, string, ControlResponse, DateTime)>();
    private readonly Dictionary<Guid, (ControlMessageType MessageType, string Payload, TaskCompletionSource<ControlResponse> Completion)> _pendingResponses = new Dictionary<Guid, (ControlMessageType, string, TaskCompletionSource<ControlResponse>)>();

    public AgentCommandServer(string pipeName)
    {
        _pipeName = string.IsNullOrWhiteSpace(pipeName) ? throw new ArgumentException("An Agent command pipe name is required.", nameof(pipeName)) : pipeName;
    }

    public async Task RunAsync(Func<ControlEnvelope, CancellationToken, Task<ControlResponse>> commandHandler, Func<bool> shouldStop, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(commandHandler);
        ArgumentNullException.ThrowIfNull(shouldStop);
        TaskCompletionSource<bool> stopRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        while (!cancellationToken.IsCancellationRequested)
        {
            NamedPipeServerStream? pipe = null;
            try
            {
                pipe = CreatePipe();
                Task connectionTask = pipe.WaitForConnectionAsync(cancellationToken);
                if (await Task.WhenAny(connectionTask, stopRequested.Task).ConfigureAwait(false) == stopRequested.Task)
                {
                    pipe.Dispose();
                    return;
                }

                await connectionTask.ConfigureAwait(false);
                if (!await _connectedClientSlots.WaitAsync(0, cancellationToken).ConfigureAwait(false))
                {
                    Logger.Warn("AgentCommandServer/RunAsync: Rejected a Control Service command because the connected-command limit of {0} was reached.", MaximumConnectedCommandClients);
                    pipe.Dispose();
                    continue;
                }

                _ = HandleClientWithSlotAsync(pipe, commandHandler, shouldStop, stopRequested, cancellationToken);
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
                Logger.Error(ex, "AgentCommandServer/RunAsync: Unable to accept a Control Service command.");
            }
        }
    }

    private async Task HandleClientWithSlotAsync(NamedPipeServerStream pipe, Func<ControlEnvelope, CancellationToken, Task<ControlResponse>> commandHandler, Func<bool> shouldStop, TaskCompletionSource<bool> stopRequested, CancellationToken cancellationToken)
    {
        using (pipe)
        {
            try
            {
                ControlEnvelope? request = await ReadEnvelopeWithTimeoutAsync(pipe, cancellationToken).ConfigureAwait(false);
                if (request == null)
                {
                    return;
                }

                using IDisposable requestScope = SupportLogScope.BeginRequest(request.RequestId);
                ControlResponse response = await ExecuteCachedCommandAsync(request, commandHandler, cancellationToken).ConfigureAwait(false);

                await ControlEnvelopeSerializer.WriteAsync(pipe, new ControlEnvelope
                {
                    MessageType = request.MessageType,
                    RequestId = request.RequestId,
                    Payload = JsonSerializer.Serialize(response)
                }, cancellationToken).ConfigureAwait(false);

                if (shouldStop())
                {
                    stopRequested.TrySetResult(true);
                }
            }
            catch (EndOfStreamException ex)
            {
                Logger.Debug(ex, "AgentCommandServer/HandleClientWithSlotAsync: The Control Service closed the User Agent command pipe before completing a request.");
            }
            catch (IOException ex)
            {
                Logger.Debug(ex, "AgentCommandServer/HandleClientWithSlotAsync: The User Agent command pipe was disconnected during a Control Service request.");
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Warn(ex, "AgentCommandServer/HandleClientWithSlotAsync: Rejected an unauthorised caller on the User Agent command pipe.");
            }
            catch (TimeoutException ex)
            {
                Logger.Warn(ex, "AgentCommandServer/HandleClientWithSlotAsync: The Control Service did not send a complete User Agent command before the request timeout.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "AgentCommandServer/HandleClientWithSlotAsync: The User Agent could not process a Control Service command.");
            }
            finally
            {
                _connectedClientSlots.Release();
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

            if (request.Hello!.ClientKind != ControlClientKind.ControlService || request.Payload == null || request.RequestId == Guid.Empty)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A Control Service request with a payload and request ID is required." };
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

    internal Task<ControlResponse> ExecuteCachedCommandAsync(ControlEnvelope request, Func<ControlEnvelope, CancellationToken, Task<ControlResponse>> commandHandler, CancellationToken cancellationToken)
    {
        // Negotiate every arrival, including replays, before consulting the execution cache.
        return ExecuteCommandAsync(request, async (command, token) =>
        {
            TaskCompletionSource<ControlResponse> completion;
            bool ownsExecution;
            lock (_successfulResponses)
            {
                RemoveExpiredResponses();
                if (_successfulResponses.TryGetValue(command.RequestId, out var replay))
                {
                    return replay.MessageType == command.MessageType && string.Equals(replay.Payload, command.Payload, StringComparison.Ordinal)
                        ? replay.Response
                        : new ControlResponse { ErrorCode = ControlErrorCode.InvalidRequest, Message = "A request ID cannot be reused for a different User Agent command." };
                }
                if (_pendingResponses.TryGetValue(command.RequestId, out var pending))
                {
                    if (pending.MessageType != command.MessageType || !string.Equals(pending.Payload, command.Payload, StringComparison.Ordinal))
                        return new ControlResponse { ErrorCode = ControlErrorCode.InvalidRequest, Message = "A request ID cannot be reused for a different User Agent command." };
                    completion = pending.Completion;
                    ownsExecution = false;
                }
                else
                {
                    completion = new TaskCompletionSource<ControlResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
                    _pendingResponses.Add(command.RequestId, (command.MessageType, command.Payload, completion));
                    ownsExecution = true;
                }
            }

            if (!ownsExecution)
                return await completion.Task.WaitAsync(token).ConfigureAwait(false);

            try
            {
                // Keep existing failure/retry semantics, and share validation failures with current waiters too.
                ControlResponse response = await ExecuteCommandAsync(command, commandHandler, token).ConfigureAwait(false);
                lock (_successfulResponses)
                {
                    if (response.IsSuccessful)
                        _successfulResponses[command.RequestId] = (command.MessageType, command.Payload, response, DateTime.UtcNow);
                    _pendingResponses.Remove(command.RequestId);
                    completion.TrySetResult(response);
                }
                return response;
            }
            catch (Exception ex)
            {
                lock (_successfulResponses)
                {
                    _pendingResponses.Remove(command.RequestId);
                    if (ex is OperationCanceledException)
                        completion.TrySetCanceled(token);
                    else
                        completion.TrySetException(ex);
                }
                // Observe faults even when no duplicate arrived to await the shared task.
                _ = completion.Task.Exception;
                throw;
            }
        }, cancellationToken);
    }

    private NamedPipeServerStream CreatePipe()
    {
        SecurityIdentifier localSystemSid = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        PipeSecurity security = new PipeSecurity();
        // Control Service is the only supported command caller. The interactive user must use
        // Control Service rather than being able to connect directly to the User Agent.
        security.AddAccessRule(new PipeAccessRule(localSystemSid, PipeAccessRights.ReadWrite, AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(_pipeName, PipeDirection.InOut, MaximumConnectedCommandClients, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, security, HandleInheritability.None);
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

}
