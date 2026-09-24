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
    public async Task RunAsync(AgentRegistration registration, TimeSpan heartbeatInterval, bool acquireDisplayControl, bool migrateUserData, TaskCompletionSource<ControlResponse>? migrationCompletion, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registration);
        try
        {
            using NamedPipeClientStream pipe = new NamedPipeClientStream(
                ".",
                ControlProtocol.ServicePipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            await pipe.ConnectAsync(ControlProtocol.ConnectionTimeout, cancellationToken).ConfigureAwait(false);

            ControlEnvelope request = new ControlEnvelope
            {
                MessageType = ControlMessageType.AgentRegistration,
                Payload = JsonSerializer.Serialize(registration)
            };
            ControlResponse registrationResponse = await SendAndReceiveAsync(pipe, request, cancellationToken).ConfigureAwait(false);
            if (!registrationResponse.IsSuccessful)
            {
                throw new InvalidOperationException(registrationResponse.Message);
            }

            if (acquireDisplayControl)
            {
                ControlResponse leaseResponse = await SendAndReceiveAsync(pipe, new ControlEnvelope
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
                ControlResponse migrationResponse = await SendAndReceiveAsync(pipe, new ControlEnvelope
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
                ControlResponse heartbeatResponse = await SendAndReceiveAsync(pipe, heartbeat, cancellationToken).ConfigureAwait(false);
                if (!heartbeatResponse.IsSuccessful)
                {
                    throw new InvalidOperationException(heartbeatResponse.Message);
                }
            }
        }
        catch
        {
            throw;
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

    public async Task<ControlResponse> AcquireDisplayControlAsync(AgentRegistration registration, CancellationToken cancellationToken)
    {
        using NamedPipeClientStream pipe = await ConnectRegisteredAgentPipeAsync(registration, cancellationToken).ConfigureAwait(false);
        return await SendAndReceiveAsync(pipe, new ControlEnvelope
        {
            MessageType = ControlMessageType.AcquireDisplayControl
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ControlResponse> ReportAgentOperationStateAsync(AgentRegistration registration, AgentOperationState operationState, CancellationToken cancellationToken)
    {
        using NamedPipeClientStream pipe = await ConnectRegisteredAgentPipeAsync(registration, cancellationToken).ConfigureAwait(false);
        return await SendAndReceiveAsync(pipe, new ControlEnvelope
        {
            MessageType = ControlMessageType.AgentHeartbeat,
            Payload = JsonSerializer.Serialize(new AgentHeartbeat
            {
                OperationState = operationState,
                IsRecoveryRequired = registration.IsRecoveryRequired
            })
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a progress update on a short-lived authenticated Agent connection. The
    /// long-lived heartbeat connection remains independent, so a runner can report
    /// progress while the Agent is otherwise idle or busy.
    /// </summary>
    public async Task<OperationStatus> PublishOperationStatusAsync(AgentRegistration registration, OperationStatusUpdate update, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentNullException.ThrowIfNull(update);

        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.ServicePipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(ControlProtocol.ConnectionTimeout, cancellationToken).ConfigureAwait(false);
        ControlResponse registrationResponse = await SendAndReceiveAsync(pipe, new ControlEnvelope
        {
            MessageType = ControlMessageType.AgentRegistration,
            Payload = JsonSerializer.Serialize(registration)
        }, cancellationToken).ConfigureAwait(false);
        if (!registrationResponse.IsSuccessful)
        {
            throw new InvalidOperationException(registrationResponse.Message);
        }

        ControlResponse statusResponse = await SendAndReceiveAsync(pipe, new ControlEnvelope
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
        using NamedPipeClientStream pipe = await ConnectRegisteredAgentPipeAsync(registration, cancellationToken).ConfigureAwait(false);
        ControlResponse response = await SendAndReceiveAsync(pipe, new ControlEnvelope
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

        using NamedPipeClientStream pipe = await ConnectRegisteredAgentPipeAsync(registration, cancellationToken).ConfigureAwait(false);
        ControlResponse response = await SendAndReceiveAsync(pipe, new ControlEnvelope
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

            using NamedPipeClientStream statusPipe = await ConnectRegisteredAgentPipeAsync(registration, cancellationToken).ConfigureAwait(false);
            ControlResponse statusResponse = await SendAndReceiveAsync(statusPipe, new ControlEnvelope
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

    private static async Task<NamedPipeClientStream> ConnectRegisteredAgentPipeAsync(AgentRegistration registration, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registration);
        NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.ServicePipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        try
        {
            await pipe.ConnectAsync(ControlProtocol.ConnectionTimeout, cancellationToken).ConfigureAwait(false);
            ControlResponse registrationResponse = await SendAndReceiveAsync(pipe, new ControlEnvelope
            {
                MessageType = ControlMessageType.AgentRegistration,
                Payload = JsonSerializer.Serialize(registration)
            }, cancellationToken).ConfigureAwait(false);
            if (!registrationResponse.IsSuccessful)
            {
                throw new InvalidOperationException(registrationResponse.Message);
            }

            return pipe;
        }
        catch
        {
            pipe.Dispose();
            throw;
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
