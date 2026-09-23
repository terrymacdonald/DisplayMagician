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
                Payload = JsonSerializer.Serialize(new ApplyProfileRequest { ProfileId = profileId, OperationId = Guid.NewGuid() })
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

        public async Task<AudioProfileListResult> ListAudioProfilesAsync(CancellationToken cancellationToken)
        {
            ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListAudioProfiles }, cancellationToken).ConfigureAwait(false);
            return response.IsSuccessful && response.AudioProfileList != null ? response.AudioProfileList : throw new InvalidOperationException(response.Message);
        }

        public Task<ControlResponse> CreateAudioProfileFromCurrentAsync(string name, CancellationToken cancellationToken)
        {
            return SendAsync(new ControlEnvelope
            {
                MessageType = ControlMessageType.CreateAudioProfileFromCurrent,
                Payload = JsonSerializer.Serialize(new CreateProfileRequest { Name = name })
            }, cancellationToken);
        }

        public Task<ControlResponse> ApplyAudioProfileAsync(string profileId, CancellationToken cancellationToken)
        {
            return SendAsync(new ControlEnvelope
            {
                MessageType = ControlMessageType.ApplyAudioProfile,
                Payload = JsonSerializer.Serialize(new ApplyAudioProfileRequest { ProfileId = profileId })
            }, cancellationToken);
        }

        public async Task<ShortcutListResult> ListShortcutsAsync(CancellationToken cancellationToken)
        {
            ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListShortcuts }, cancellationToken).ConfigureAwait(false);
            return response.IsSuccessful && response.ShortcutList != null ? response.ShortcutList : throw new InvalidOperationException(response.Message);
        }

        public Task<ControlResponse> StartShortcutAsync(string shortcutId, CancellationToken cancellationToken)
        {
            return SendAsync(new ControlEnvelope
            {
                MessageType = ControlMessageType.StartShortcut,
                Payload = JsonSerializer.Serialize(new StartShortcutRequest { ShortcutId = shortcutId, OperationId = Guid.NewGuid() })
            }, cancellationToken);
        }

        public async Task<OperationStatus[]> ListOperationStatusesAsync(CancellationToken cancellationToken)
        {
            ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListOperationStatuses }, cancellationToken).ConfigureAwait(false);
            return response.IsSuccessful ? response.OperationStatuses : throw new InvalidOperationException(response.Message);
        }

        public async Task<OperationDecision[]> ListOperationDecisionsAsync(CancellationToken cancellationToken)
        {
            ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListOperationDecisions }, cancellationToken).ConfigureAwait(false);
            return response.IsSuccessful ? response.OperationDecisions : throw new InvalidOperationException(response.Message);
        }

        public async Task<OperationDecision> ResolveOperationDecisionAsync(Guid promptId, OperationDecisionChoice choice, CancellationToken cancellationToken)
        {
            ControlResponse response = await SendAsync(new ControlEnvelope
            {
                MessageType = ControlMessageType.ResolveOperationDecision,
                Payload = JsonSerializer.Serialize(new ResolveOperationDecisionRequest { PromptId = promptId, Choice = choice })
            }, cancellationToken).ConfigureAwait(false);
            return response.IsSuccessful && response.OperationDecision != null ? response.OperationDecision : throw new InvalidOperationException(response.Message);
        }

        private static async Task<ControlResponse> SendAsync(ControlEnvelope request, CancellationToken cancellationToken)
        {
            using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.ClientPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await pipe.ConnectAsync(ControlProtocol.ConnectionTimeout, cancellationToken).ConfigureAwait(false);
            using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(ControlProtocol.ResponseTimeout);
            ControlEnvelope response;
            try
            {
                await ControlEnvelopeSerializer.WriteAsync(pipe, request, timeoutSource.Token).ConfigureAwait(false);
                response = await ControlEnvelopeSerializer.ReadAsync(pipe, timeoutSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("The Control Service did not respond within the permitted time.");
            }

            if (response.RequestId != request.RequestId || response.MessageType != request.MessageType)
            {
                throw new InvalidDataException("The Control Service returned an invalid response.");
            }

            return JsonSerializer.Deserialize<ControlResponse>(response.Payload)
                ?? throw new InvalidDataException("The Control Service returned an unreadable response.");
        }
    }
}
