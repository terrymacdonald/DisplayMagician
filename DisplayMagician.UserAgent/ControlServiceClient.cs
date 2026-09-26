using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagician.UserAgent;

public sealed class ControlServiceClient
{
    private readonly SemaphoreSlim _requestLock = new SemaphoreSlim(1, 1);
    private NamedPipeClientStream? _pipe;

    public async Task RunAsync(AgentRegistration registration, TimeSpan heartbeatInterval, bool acquireDisplayControl, bool migrateUserData, TaskCompletionSource<ControlResponse>? migrationCompletion, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registration);

        using NamedPipeClientStream pipe = new NamedPipeClientStream(
            ".",
            ControlProtocol.ServicePipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        await pipe.ConnectAsync(ControlProtocol.ConnectionTimeout, cancellationToken).ConfigureAwait(false);
        _pipe = pipe;

        try
        {
            ControlEnvelope request = new ControlEnvelope
            {
                MessageType = ControlMessageType.AgentRegistration,
                Payload = JsonSerializer.Serialize(registration)
            };
            ControlResponse registrationResponse = await SendPersistentAsync(request, cancellationToken).ConfigureAwait(false);
            if (!registrationResponse.IsSuccessful)
            {
                migrationCompletion?.TrySetResult(registrationResponse);
                throw new InvalidOperationException(registrationResponse.Message);
            }

            if (acquireDisplayControl)
            {
                ControlResponse leaseResponse = await SendPersistentAsync(new ControlEnvelope
                {
                    MessageType = ControlMessageType.AcquireDisplayControl
                }, cancellationToken).ConfigureAwait(false);
                if (!leaseResponse.IsSuccessful)
                {
                    throw new InvalidOperationException(leaseResponse.Message);
                }
            }

            if (migrateUserData)
            {
                ControlResponse migrationResponse = await SendPersistentAsync(new ControlEnvelope
                {
                    MessageType = ControlMessageType.MigrateUserData
                }, cancellationToken).ConfigureAwait(false);
                migrationCompletion?.TrySetResult(migrationResponse);
                if (!migrationResponse.IsSuccessful)
                {
                    throw new InvalidOperationException(migrationResponse.Message);
                }
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(heartbeatInterval, cancellationToken).ConfigureAwait(false);
                ControlEnvelope heartbeat = new ControlEnvelope
                {
                    MessageType = ControlMessageType.AgentHeartbeat,
                    Payload = JsonSerializer.Serialize(new AgentHeartbeat
                    {
                        OperationState = registration.OperationState,
                        IsRecoveryRequired = registration.IsRecoveryRequired
                    })
                };
                ControlResponse heartbeatResponse = await SendPersistentAsync(heartbeat, cancellationToken).ConfigureAwait(false);
                if (!heartbeatResponse.IsSuccessful)
                {
                    throw new InvalidOperationException(heartbeatResponse.Message);
                }
            }
        }
        finally
        {
            if (ReferenceEquals(_pipe, pipe))
            {
                _pipe = null;
            }
        }
    }

    public async Task<ControlResponse> RegisterOnceAsync(AgentRegistration registration, CancellationToken cancellationToken)
    {
        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.ServicePipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(ControlProtocol.ConnectionTimeout, cancellationToken).ConfigureAwait(false);
        return await SendAndReceiveAsync(pipe, new ControlEnvelope
        {
            MessageType = ControlMessageType.AgentRegistration,
            Payload = JsonSerializer.Serialize(registration)
        }, cancellationToken).ConfigureAwait(false);
    }

    public Task<ControlResponse> UpdateRegistrationAsync(AgentRegistration registration, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registration);
        return SendPersistentAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.AgentRegistration,
            Payload = JsonSerializer.Serialize(registration)
        }, cancellationToken);
    }

    public Task<ControlResponse> AcquireDisplayControlAsync(AgentRegistration registration, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registration);
        return SendPersistentAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.AcquireDisplayControl
        }, cancellationToken);
    }

    public Task<ControlResponse> ReportAgentOperationStateAsync(AgentRegistration registration, AgentOperationState operationState, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registration);
        return SendPersistentAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.AgentHeartbeat,
            Payload = JsonSerializer.Serialize(new AgentHeartbeat
            {
                OperationState = operationState,
                IsRecoveryRequired = registration.IsRecoveryRequired
            })
        }, cancellationToken);
    }

    /// <summary>
    /// Sends an operation status update over the User Agent's persistent authenticated
    /// Control Service connection.
    /// </summary>
    public async Task<OperationStatus> PublishOperationStatusAsync(AgentRegistration registration, OperationStatusUpdate update, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentNullException.ThrowIfNull(update);

        ControlResponse statusResponse = await SendPersistentAsync(new ControlEnvelope
        {
            MessageType = update.IsTerminal ? ControlMessageType.OperationCompleted : ControlMessageType.OperationProgress,
            Payload = JsonSerializer.Serialize(update)
        }, cancellationToken).ConfigureAwait(false);
        if (!statusResponse.IsSuccessful || statusResponse.OperationStatus == null)
        {
            throw new InvalidOperationException(statusResponse.Message);
        }

        return statusResponse.OperationStatus;
    }

    public async Task ReconcileOperationStatusesAsync(AgentRegistration registration, OperationStatusUpdate[] activeUpdates, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ControlResponse response = await SendPersistentAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.ReconcileOperationStatuses,
            Payload = JsonSerializer.Serialize(new OperationStatusReconciliationRequest { ActiveUpdates = activeUpdates ?? Array.Empty<OperationStatusUpdate>() })
        }, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessful) throw new InvalidOperationException(response.Message);
    }

    public async Task<OperationDecision> RequestOperationDecisionAsync(AgentRegistration registration, RequestOperationDecisionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentNullException.ThrowIfNull(request);

        ControlResponse response = await SendPersistentAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.RequestOperationDecision,
            Payload = JsonSerializer.Serialize(request)
        }, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessful || response.OperationDecision == null)
        {
            throw new InvalidOperationException(response.Message);
        }

        OperationDecision decision = response.OperationDecision;
        while (!decision.IsResolved)
        {
            TimeSpan untilExpiry = decision.ExpiresUtc - DateTime.UtcNow;
            if (untilExpiry <= TimeSpan.Zero)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await Task.Delay(untilExpiry < TimeSpan.FromSeconds(1) ? untilExpiry : TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
            }

            ControlResponse statusResponse = await SendPersistentAsync(new ControlEnvelope
            {
                MessageType = ControlMessageType.GetOperationDecision,
                Payload = JsonSerializer.Serialize(new OperationDecisionStatusRequest { PromptId = decision.PromptId })
            }, cancellationToken).ConfigureAwait(false);
            if (!statusResponse.IsSuccessful || statusResponse.OperationDecision == null)
            {
                throw new InvalidOperationException(statusResponse.Message);
            }

            decision = statusResponse.OperationDecision;
        }

        return decision;
    }

    private async Task<ControlResponse> SendPersistentAsync(ControlEnvelope request, CancellationToken cancellationToken)
    {
        await _requestLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            NamedPipeClientStream pipe = _pipe
                ?? throw new IOException("The persistent Control Service connection is not available.");
            if (!pipe.IsConnected)
            {
                throw new IOException("The persistent Control Service connection is not connected.");
            }

            return await SendAndReceiveAsync(pipe, request, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _requestLock.Release();
        }
    }

    private static async Task<ControlResponse> SendAndReceiveAsync(NamedPipeClientStream pipe, ControlEnvelope request, CancellationToken cancellationToken)
    {
        request.Hello = ControlProtocol.CreateHello(ControlClientKind.UserAgent, "DisplayMagician.UserAgent", "DisplayMagician User Agent");
        using CancellationTokenSource responseTimeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        responseTimeoutSource.CancelAfter(ControlProtocol.ResponseTimeout);
        try
        {
            await ControlEnvelopeSerializer.WriteAsync(pipe, request, responseTimeoutSource.Token).ConfigureAwait(false);
            ControlEnvelope? response = await ControlEnvelopeSerializer.ReadAsync(pipe, responseTimeoutSource.Token).ConfigureAwait(false);
            if (response == null || response.RequestId != request.RequestId || response.MessageType != request.MessageType)
            {
                throw new InvalidDataException("The Control Service returned an invalid response.");
            }

            ControlResponse controlResponse = JsonSerializer.Deserialize<ControlResponse>(response.Payload)
                ?? throw new InvalidDataException("The Control Service returned an unreadable registration response.");
            if (controlResponse.IsSuccessful && !ControlProtocol.IsCompatibleWelcome(request.Hello, controlResponse.ProtocolWelcome))
            {
                throw new InvalidDataException("The Control Service did not complete a compatible protocol negotiation.");
            }

            return controlResponse;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("The Control Service did not respond within the permitted time.");
        }
    }
}
