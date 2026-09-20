using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagicianConsole
{
    internal sealed class ControlServicePipeClient
    {
        public async Task<ProfileListResult> ListProfilesAsync(CancellationToken cancellationToken)
        {
            ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListProfiles }, cancellationToken).ConfigureAwait(false);
            return response.IsSuccessful && response.ProfileList != null
                ? response.ProfileList
                : throw new InvalidOperationException(response.Message);
        }

        public Task<ControlResponse> ApplyProfileAsync(string profileId, CancellationToken cancellationToken)
        {
            return SendAsync(new ControlEnvelope
            {
                MessageType = ControlMessageType.ApplyProfile,
                Payload = JsonSerializer.Serialize(new ApplyProfileRequest { ProfileId = profileId })
            }, cancellationToken);
        }

        public Task<ControlResponse> CreateProfileFromCurrentAsync(string name, CancellationToken cancellationToken)
        {
            return SendAsync(new ControlEnvelope
            {
                MessageType = ControlMessageType.CreateProfileFromCurrent,
                Payload = JsonSerializer.Serialize(new CreateProfileRequest { Name = name })
            }, cancellationToken);
        }

        public Task<ControlResponse> DeleteProfileAsync(string profileId, CancellationToken cancellationToken)
        {
            return SendAsync(new ControlEnvelope
            {
                MessageType = ControlMessageType.DeleteProfile,
                Payload = JsonSerializer.Serialize(new DeleteProfileRequest { ProfileId = profileId })
            }, cancellationToken);
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
}