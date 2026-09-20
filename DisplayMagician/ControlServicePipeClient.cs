using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
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

    public Task<ControlResponse> StartShortcutAsync(string shortcutId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(shortcutId))
        {
            return Task.FromResult(new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A shortcut ID is required." });
        }

        return SendAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.StartShortcut,
            Payload = JsonSerializer.Serialize(new StartShortcutRequest { ShortcutId = shortcutId })
        }, cancellationToken);
    }

    public async Task<ControlResponse> StartShortcutWhenAgentAvailableAsync(string shortcutId, CancellationToken cancellationToken)
    {
        const int maximumAttempts = 40;
        for (int attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            ControlResponse response = await StartShortcutAsync(shortcutId, cancellationToken).ConfigureAwait(false);
            if (!ControlServiceRetryPolicy.ShouldRetryAfterStartingAgent(response) || attempt == maximumAttempts)
            {
                return response;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException("The User Agent registration retry loop completed unexpectedly.");
    }

    public async Task<OperationStatus> GetOperationStatusAsync(Guid operationId, CancellationToken cancellationToken)
    {
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException("An operation ID is required.", nameof(operationId));
        }

        ControlResponse response = await SendAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.GetOperationStatus,
            Payload = JsonSerializer.Serialize(new OperationStatusRequest { OperationId = operationId })
        }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.OperationStatus != null ? response.OperationStatus : throw new InvalidOperationException(response.Message);
    }

    public async Task<OperationStatus[]> ListOperationStatusesAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListOperationStatuses }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful ? response.OperationStatuses : throw new InvalidOperationException(response.Message);
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

    public async Task<GameListResult> ListGamesAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListGames }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.GameList != null ? response.GameList : throw new InvalidOperationException(response.Message);
    }

    public async Task<AppListResult> ListAppsAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListApps }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.AppList != null ? response.AppList : throw new InvalidOperationException(response.Message);
    }

    public async Task<MessageListResult> ListMessagesAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListMessages }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.MessageList != null ? response.MessageList : throw new InvalidOperationException(response.Message);
    }

    public async Task<MessageListResult> SetMessageReadStateAsync(IEnumerable<string> messageIds, bool isRead, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(messageIds);
        ControlResponse response = await SendAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.SetMessageReadState,
            Payload = JsonSerializer.Serialize(new SetMessageReadStateRequest { MessageIds = messageIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToArray(), IsRead = isRead })
        }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.MessageList != null ? response.MessageList : throw new InvalidOperationException(response.Message);
    }

    public async Task<MessageSyncResult> SyncMessagesAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.SyncMessages }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.MessageSync != null ? response.MessageSync : throw new InvalidOperationException(response.Message);
    }

    public Task<ControlResponse> StopAgentIfIdleAsync(CancellationToken cancellationToken)
    {
        return SendAsync(new ControlEnvelope { MessageType = ControlMessageType.StopAgentIfIdle }, cancellationToken);
    }

    public Task<ControlResponse> CreateProfileFromCurrentAsync(string name, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.CreateProfileFromCurrent, Payload = JsonSerializer.Serialize(new CreateProfileRequest { Name = name }) }, cancellationToken);

    public Task<ControlResponse> RenameProfileAsync(string profileId, string name, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.RenameProfile, Payload = JsonSerializer.Serialize(new RenameProfileRequest { ProfileId = profileId, Name = name }) }, cancellationToken);

    public Task<ControlResponse> DeleteProfileAsync(string profileId, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.DeleteProfile, Payload = JsonSerializer.Serialize(new DeleteProfileRequest { ProfileId = profileId }) }, cancellationToken);

    public Task<ControlResponse> UpdateProfileFromCurrentAsync(string profileId, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.UpdateProfileFromCurrent, Payload = JsonSerializer.Serialize(new DeleteProfileRequest { ProfileId = profileId }) }, cancellationToken);

    public async Task<AudioProfileListResult> ListAudioProfilesAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListAudioProfiles }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.AudioProfileList != null ? response.AudioProfileList : throw new InvalidOperationException(response.Message);
    }

    public Task<ControlResponse> ApplyAudioProfileAsync(string profileId, int deviceWaitMilliseconds, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ApplyAudioProfile, Payload = JsonSerializer.Serialize(new ApplyAudioProfileRequest { ProfileId = profileId, DeviceWaitMilliseconds = deviceWaitMilliseconds }) }, cancellationToken);

    public Task<ControlResponse> CreateAudioProfileFromCurrentAsync(string name, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.CreateAudioProfileFromCurrent, Payload = JsonSerializer.Serialize(new CreateProfileRequest { Name = name }) }, cancellationToken);

    public Task<ControlResponse> RenameAudioProfileAsync(string profileId, string name, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.RenameAudioProfile, Payload = JsonSerializer.Serialize(new RenameProfileRequest { ProfileId = profileId, Name = name }) }, cancellationToken);

    public Task<ControlResponse> DeleteAudioProfileAsync(string profileId, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.DeleteAudioProfile, Payload = JsonSerializer.Serialize(new DeleteProfileRequest { ProfileId = profileId }) }, cancellationToken);

    public Task<ControlResponse> UpdateAudioProfileFromCurrentAsync(string profileId, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.UpdateAudioProfileFromCurrent, Payload = JsonSerializer.Serialize(new DeleteProfileRequest { ProfileId = profileId }) }, cancellationToken);

    public async Task<RepositorySnapshot> GetRepositorySnapshotAsync(RepositoryKind repository, CancellationToken cancellationToken)
    {
        const int maximumAttempts = 40;
        for (int attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.GetRepositorySnapshot, Payload = JsonSerializer.Serialize(new RepositorySnapshotRequest { Repository = repository }) }, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessful && response.RepositorySnapshot != null)
            {
                return response.RepositorySnapshot;
            }

            if (!ControlServiceRetryPolicy.ShouldRetryAfterStartingAgent(response) || attempt == maximumAttempts)
            {
                throw new InvalidOperationException(response.Message);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException("The User Agent registration retry loop completed unexpectedly.");
    }

    public async Task<RepositoryCommitResult> CommitRepositorySnapshotAsync(RepositoryCommitRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.CommitRepositorySnapshot, Payload = JsonSerializer.Serialize(request) }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.RepositoryCommit != null ? response.RepositoryCommit : throw new InvalidOperationException(response.Message);
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
