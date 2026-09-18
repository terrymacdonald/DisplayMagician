using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagician;

internal sealed class ControlServicePipeClient
{
    public async Task<ControlResponse> ApplyProfileAsync(string profileId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A display profile ID is required." };
        }

        return await SendAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.ApplyProfile,
            Payload = JsonSerializer.Serialize(new ApplyProfileRequest { ProfileId = profileId })
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ControlResponse> ApplyProfileWhenAgentAvailableAsync(string profileId, CancellationToken cancellationToken)
    {
        const int maximumAttempts = 40;
        for (int attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            ControlResponse response = await ApplyProfileAsync(profileId, cancellationToken).ConfigureAwait(false);
            if (!ControlServiceRetryPolicy.ShouldRetryAfterStartingAgent(response) || attempt == maximumAttempts)
            {
                return response;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException("The User Agent registration retry loop completed unexpectedly.");
    }

    public async Task<ProfileListResult> ListProfilesAsync(CancellationToken cancellationToken)
    {
        const int maximumAttempts = 40;
        for (int attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListProfiles }, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessful && response.ProfileList != null)
            {
                return response.ProfileList;
            }

            if (!ControlServiceRetryPolicy.ShouldRetryAfterStartingAgent(response) || attempt == maximumAttempts)
            {
                throw new InvalidOperationException(response.Message);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException("The User Agent registration retry loop completed unexpectedly.");
    }

    public Task<ControlResponse> StopAgentIfIdleAsync(CancellationToken cancellationToken)
    {
        return SendAsync(new ControlEnvelope { MessageType = ControlMessageType.StopAgentIfIdle }, cancellationToken);
    }

    private static async Task<ControlResponse> SendAsync(ControlEnvelope request, CancellationToken cancellationToken)
    {
        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.ClientPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
        await ControlEnvelopeSerializer.WriteAsync(pipe, request, cancellationToken).ConfigureAwait(false);
        ControlEnvelope response = await ControlEnvelopeSerializer.ReadAsync(pipe, cancellationToken).ConfigureAwait(false);
        if (response == null || response.RequestId != request.RequestId)
        {
            throw new InvalidDataException("The Control Service returned an invalid response.");
        }

        return JsonSerializer.Deserialize<ControlResponse>(response.Payload)
            ?? throw new InvalidDataException("The Control Service returned an unreadable response.");
    }
}
