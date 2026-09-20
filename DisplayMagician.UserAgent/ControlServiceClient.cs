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
    public async Task<int> ApplyDisplayProfileAsync(AgentRegistration registration, string profileId, string displayMagicianExecutablePath, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayMagicianExecutablePath);

        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.ServicePipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(cancellationToken).ConfigureAwait(false);
        ControlResponse registrationResponse = await SendAndReceiveAsync(pipe, new ControlEnvelope
        {
            MessageType = ControlMessageType.AgentRegistration,
            Payload = JsonSerializer.Serialize(registration)
        }, cancellationToken).ConfigureAwait(false);
        if (!registrationResponse.IsSuccessful)
        {
            throw new InvalidOperationException(registrationResponse.Message);
        }

        ControlResponse leaseResponse = await SendAndReceiveAsync(pipe, new ControlEnvelope
        {
            MessageType = ControlMessageType.AcquireDisplayControl
        }, cancellationToken).ConfigureAwait(false);
        if (!leaseResponse.IsSuccessful)
        {
            throw new InvalidOperationException(leaseResponse.Message);
        }

        using System.Diagnostics.Process process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = displayMagicianExecutablePath,
                UseShellExecute = false,
                Arguments = $"ChangeProfile \"{profileId.Replace("\"", string.Empty, StringComparison.Ordinal)}\" --agent-hosted-operation"
            }
        };
        if (!process.Start())
        {
            throw new InvalidOperationException("DisplayMagician could not start the profile-change operation.");
        }

        while (!process.HasExited)
        {
            await Task.WhenAny(process.WaitForExitAsync(cancellationToken), Task.Delay(TimeSpan.FromSeconds(15), cancellationToken)).ConfigureAwait(false);
            if (!process.HasExited)
            {
                ControlResponse heartbeatResponse = await SendAndReceiveAsync(pipe, new ControlEnvelope
                {
                    MessageType = ControlMessageType.AgentHeartbeat,
                    Payload = JsonSerializer.Serialize(new AgentHeartbeat
                    {
                        OperationState = AgentOperationState.Running,
                        IsRecoveryRequired = false
                    })
                }, cancellationToken).ConfigureAwait(false);
                if (!heartbeatResponse.IsSuccessful)
                {
                    throw new InvalidOperationException(heartbeatResponse.Message);
                }
            }
        }

        ControlResponse completionHeartbeatResponse = await SendAndReceiveAsync(pipe, new ControlEnvelope
        {
            MessageType = ControlMessageType.AgentHeartbeat,
            Payload = JsonSerializer.Serialize(new AgentHeartbeat
            {
                OperationState = AgentOperationState.Idle,
                IsRecoveryRequired = false
            })
        }, cancellationToken).ConfigureAwait(false);
        if (!completionHeartbeatResponse.IsSuccessful)
        {
            throw new InvalidOperationException(completionHeartbeatResponse.Message);
        }

        return process.ExitCode;
    }

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

            await pipe.ConnectAsync(cancellationToken).ConfigureAwait(false);

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
        catch (Exception ex)
        {
            migrationCompletion?.TrySetException(ex);
            throw;
        }
    }

    public async Task<ControlResponse> RegisterOnceAsync(AgentRegistration registration, CancellationToken cancellationToken)
    {
        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.ServicePipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(cancellationToken).ConfigureAwait(false);
        return await SendAndReceiveAsync(pipe, new ControlEnvelope
        {
            MessageType = ControlMessageType.AgentRegistration,
            Payload = JsonSerializer.Serialize(registration)
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
        await pipe.ConnectAsync(cancellationToken).ConfigureAwait(false);
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

    private static async Task<ControlResponse> SendAndReceiveAsync(NamedPipeClientStream pipe, ControlEnvelope request, CancellationToken cancellationToken)
    {
        await ControlEnvelopeSerializer.WriteAsync(pipe, request, cancellationToken).ConfigureAwait(false);
        ControlEnvelope? response = await ControlEnvelopeSerializer.ReadAsync(pipe, cancellationToken).ConfigureAwait(false);
        if (response == null || response.RequestId != request.RequestId)
        {
            throw new InvalidDataException("The Control Service returned an invalid response.");
        }

        return JsonSerializer.Deserialize<ControlResponse>(response.Payload)
            ?? throw new InvalidDataException("The Control Service returned an unreadable registration response.");
    }
}
