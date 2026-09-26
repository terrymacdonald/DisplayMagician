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

        return await ApplyProfileAsync(profileId, Guid.NewGuid(), Guid.NewGuid(), cancellationToken).ConfigureAwait(false);
    }

    private static Task<ControlResponse> ApplyProfileAsync(string profileId, Guid operationId, Guid requestId, CancellationToken cancellationToken)
    {
        return SendAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.ApplyProfile,
            RequestId = requestId,
            Payload = JsonSerializer.Serialize(new ApplyProfileRequest { ProfileId = profileId, OperationId = operationId })
        }, cancellationToken);
    }

    public Task<ControlResponse> StartShortcutAsync(string shortcutId, CancellationToken cancellationToken)
    {
        return StartShortcutAsync(shortcutId, Guid.NewGuid(), Guid.NewGuid(), cancellationToken);
    }

    private Task<ControlResponse> StartShortcutAsync(string shortcutId, Guid operationId, Guid requestId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(shortcutId))
        {
            return Task.FromResult(new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A shortcut ID is required." });
        }

        return SendAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.StartShortcut,
            RequestId = requestId,
            Payload = JsonSerializer.Serialize(new StartShortcutRequest { ShortcutId = shortcutId, OperationId = operationId })
        }, cancellationToken);
    }

    public Task<ControlResponse> CancelOperationAsync(Guid operationId, CancellationToken cancellationToken)
    {
        if (operationId == Guid.Empty)
        {
            return Task.FromResult(new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "An operation ID is required." });
        }

        return SendAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.CancelOperation,
            Payload = JsonSerializer.Serialize(new CancelOperationRequest { OperationId = operationId })
        }, cancellationToken);
    }

    public async Task<ControlResponse> StartShortcutWhenAgentAvailableAsync(string shortcutId, CancellationToken cancellationToken)
    {
        Guid operationId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();
        const int maximumAttempts = 40;
        for (int attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            ControlResponse response = await StartShortcutAsync(shortcutId, operationId, requestId, cancellationToken).ConfigureAwait(false);
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

    public async Task<OperationDecision> ResolveOperationDecisionAsync(Guid promptId, OperationDecisionChoice choice, CancellationToken cancellationToken)
    {
        if (promptId == Guid.Empty || choice == OperationDecisionChoice.Unknown)
        {
            throw new ArgumentException("A prompt ID and decision choice are required.");
        }

        ControlResponse response = await SendAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.ResolveOperationDecision,
            Payload = JsonSerializer.Serialize(new ResolveOperationDecisionRequest { PromptId = promptId, Choice = choice })
        }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.OperationDecision != null ? response.OperationDecision : throw new InvalidOperationException(response.Message);
    }

    public async Task<OperationDecision[]> ListOperationDecisionsAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListOperationDecisions }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful ? response.OperationDecisions : throw new InvalidOperationException(response.Message);
    }

    public async Task<ControlServiceStatus> GetServiceStatusAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.GetServiceStatus }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.ServiceStatus != null ? response.ServiceStatus : throw new InvalidOperationException(response.Message);
    }

    public async Task<ControlResponse> ApplyProfileWhenAgentAvailableAsync(string profileId, CancellationToken cancellationToken)
    {
        Guid requestId = Guid.NewGuid();
        Guid operationId = Guid.NewGuid();
        const int maximumAttempts = 40;
        for (int attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            ControlResponse response = await ApplyProfileAsync(profileId, operationId, requestId, cancellationToken).ConfigureAwait(false);
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

    public async Task<ShortcutListResult> ListShortcutsAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListShortcuts }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.ShortcutList != null ? response.ShortcutList : throw new InvalidOperationException(response.Message);
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

    public async Task<ClientSyncResult> SyncClientAsync(bool isManual, bool preferPrerelease, CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.SyncClient,
            Payload = JsonSerializer.Serialize(new ClientSyncRequest { IsManual = isManual, PreferPrerelease = preferPrerelease })
        }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.ClientSync != null ? response.ClientSync : throw new InvalidOperationException(response.Message);
    }

    public async Task<AnonymousMetricsSettings> GetAnonymousMetricsSettingsAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.GetAnonymousMetricsSettings }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.AnonymousMetricsSettings != null ? response.AnonymousMetricsSettings : throw new InvalidOperationException(response.Message);
    }

    public async Task<AnonymousMetricsSettings> UpdateAnonymousMetricsSettingsAsync(bool shareAnonymousUsageMetrics, CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.UpdateAnonymousMetricsSettings,
            Payload = JsonSerializer.Serialize(new AnonymousMetricsSettings { ShareAnonymousUsageMetrics = shareAnonymousUsageMetrics })
        }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.AnonymousMetricsSettings != null ? response.AnonymousMetricsSettings : throw new InvalidOperationException(response.Message);
    }

    public Task InitializeAnonymousMetricsAsync(InitializeAnonymousMetricsRequest initialization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(initialization);
        return SendRequiredAsync(new ControlEnvelope { MessageType = ControlMessageType.InitializeAnonymousMetrics, Payload = JsonSerializer.Serialize(initialization) }, cancellationToken);
    }

    public Task ReportAnonymousMetricsUsageAsync(AnonymousMetricsUsageReport usage, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(usage);
        return SendRequiredAsync(new ControlEnvelope { MessageType = ControlMessageType.ReportAnonymousMetricsUsage, Payload = JsonSerializer.Serialize(usage) }, cancellationToken);
    }

    public Task<ControlResponse> StopAgentIfIdleAsync(CancellationToken cancellationToken)
    {
        return SendAsync(new ControlEnvelope { MessageType = ControlMessageType.StopAgentIfIdle }, cancellationToken);
    }

    public Task<ControlResponse> RestartUserAgentAsync(CancellationToken cancellationToken)
    {
        return SendAsync(new ControlEnvelope { MessageType = ControlMessageType.RestartUserAgent }, cancellationToken);
    }

    public Task<ControlResponse> SetTemporaryDiagnosticLogLevelAsync(string level, Guid ownerId, CancellationToken cancellationToken)
    {
        return SendAsync(new ControlEnvelope { MessageType = ControlMessageType.SetTemporaryDiagnosticLogLevel, Payload = JsonSerializer.Serialize(new TemporaryDiagnosticLogLevelRequest { Level = level, OwnerId = ownerId, DurationMinutes = 30 }) }, cancellationToken);
    }

    public Task<ControlResponse> ReleaseTemporaryDiagnosticLogLevelAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        return SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ReleaseTemporaryDiagnosticLogLevel, Payload = JsonSerializer.Serialize(new ReleaseTemporaryDiagnosticLogLevelRequest { OwnerId = ownerId }) }, cancellationToken);
    }

    public async Task<GatewaySettings> GetGatewaySettingsAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.GetGatewaySettings }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.GatewaySettings != null ? response.GatewaySettings : throw new InvalidOperationException(response.Message);
    }

    public Task<ControlResponse> UpdateGatewaySettingsAsync(GatewaySettings settings, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return SendAsync(new ControlEnvelope { MessageType = ControlMessageType.UpdateGatewaySettings, Payload = JsonSerializer.Serialize(settings) }, cancellationToken);
    }

    public async Task<GatewayIdentityView> GetGatewayIdentityAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.GetGatewayIdentity }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.GatewayIdentity != null ? response.GatewayIdentity : throw new InvalidOperationException(response.Message);
    }

    public async Task<DevicePairingQrCode> CreateDevicePairingQrAsync(string gatewayUri, CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.CreateDevicePairingQr, Payload = JsonSerializer.Serialize(new CreateDevicePairingQrRequest { GatewayUri = gatewayUri ?? string.Empty }) }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.DevicePairingQrCode != null ? response.DevicePairingQrCode : throw new InvalidOperationException(response.Message);
    }

    public async Task<DevicePairingSessionView[]> ListDevicePairingRequestsAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListDevicePairingRequests }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful ? response.DevicePairingRequests : throw new InvalidOperationException(response.Message);
    }

    public Task<ControlResponse> ApproveDevicePairingAsync(ApproveDevicePairingRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ApproveDevicePairing, Payload = JsonSerializer.Serialize(request) }, cancellationToken);
    }

    public Task<ControlResponse> RejectDevicePairingAsync(Guid pairingSessionId, CancellationToken cancellationToken)
    {
        return SendAsync(new ControlEnvelope { MessageType = ControlMessageType.RejectDevicePairing, Payload = JsonSerializer.Serialize(new RejectDevicePairingRequest { PairingSessionId = pairingSessionId }) }, cancellationToken);
    }

    public async Task<PairedClientView[]> ListPairedClientsAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListPairedClients }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful ? response.PairedClients : throw new InvalidOperationException(response.Message);
    }

    public Task<ControlResponse> RevokePairedClientAsync(string deviceId, CancellationToken cancellationToken)
    {
        return SendAsync(new ControlEnvelope { MessageType = ControlMessageType.RevokePairedClient, Payload = JsonSerializer.Serialize(new RevokePairedClientRequest { DeviceId = deviceId ?? string.Empty }) }, cancellationToken);
    }

    public Task<ControlResponse> CreateUserSupportBundleAsync(string destinationPath, CancellationToken cancellationToken)
    {
        return SendAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.CreateUserSupportBundle,
            Payload = JsonSerializer.Serialize(new CreateUserSupportBundleRequest { DestinationPath = destinationPath ?? string.Empty })
        }, cancellationToken);
    }

    public Task<ControlResponse> ForceReleaseDisplayControlAsync(CancellationToken cancellationToken)
    {
        return SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ForceReleaseDisplayControl }, cancellationToken);
    }

    public Task<ControlResponse> RecordRecoveryAdministrationAsync(string action, string outcome, CancellationToken cancellationToken)
    {
        return SendAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.RecordRecoveryAdministration,
            Payload = JsonSerializer.Serialize(new RecoveryAdministrationRequest { Action = action ?? string.Empty, Outcome = outcome ?? string.Empty })
        }, cancellationToken);
    }

    private async Task SendRequiredAsync(ControlEnvelope request, CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessful)
        {
            throw new InvalidOperationException(response.Message);
        }
    }

    public Task<ControlResponse> CreateProfileFromCurrentAsync(string name, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.CreateProfileFromCurrent, Payload = JsonSerializer.Serialize(new CreateProfileRequest { Name = name }) }, cancellationToken);

    public Task<ControlResponse> RenameProfileAsync(string profileId, string name, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.RenameProfile, Payload = JsonSerializer.Serialize(new RenameProfileRequest { ProfileId = profileId, Name = name }) }, cancellationToken);

    public Task<ControlResponse> DeleteProfileAsync(string profileId, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.DeleteProfile, Payload = JsonSerializer.Serialize(new DeleteProfileRequest { ProfileId = profileId }) }, cancellationToken);

    public Task<ControlResponse> UpdateProfileFromCurrentAsync(string profileId, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.UpdateProfileFromCurrent, Payload = JsonSerializer.Serialize(new DeleteProfileRequest { ProfileId = profileId }) }, cancellationToken);

    public Task<ControlResponse> UpdateDisplayProfileSettingsAsync(string profileId, DisplayProfileSettings settings, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.UpdateDisplayProfileSettings, Payload = JsonSerializer.Serialize(new UpdateDisplayProfileSettingsRequest { ProfileId = profileId, Settings = settings }) }, cancellationToken);

    public async Task<AudioProfileListResult> ListAudioProfilesAsync(CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ListAudioProfiles }, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.AudioProfileList != null ? response.AudioProfileList : throw new InvalidOperationException(response.Message);
    }

    public Task<ControlResponse> ApplyAudioProfileAsync(string profileId, int deviceWaitMilliseconds, CancellationToken cancellationToken) => SendAsync(new ControlEnvelope { MessageType = ControlMessageType.ApplyAudioProfile, Payload = JsonSerializer.Serialize(new ApplyAudioProfileRequest { ProfileId = profileId, DeviceWaitMilliseconds = deviceWaitMilliseconds, OperationId = Guid.NewGuid() }) }, cancellationToken);

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

    public async Task SubscribeClientEventsAsync(Func<ControlClientEvent, Task> onEvent, Action<ProtocolWelcome> onConnected, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(onEvent);
        ArgumentNullException.ThrowIfNull(onConnected);
        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.ClientEventPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(ControlProtocol.ConnectionTimeout, cancellationToken).ConfigureAwait(false);
        ControlEnvelope request = new ControlEnvelope { MessageType = ControlMessageType.SubscribeClientEvents, Hello = ControlProtocol.CreateHello(ControlClientKind.DesktopApplication, "DisplayMagician.WinForms", "DisplayMagician") };
        ControlEnvelope response = await SendAndReceiveEnvelopeAsync(pipe, request, cancellationToken).ConfigureAwait(false);
        if (response.MessageType != ControlMessageType.SubscribeClientEvents)
        {
            throw new InvalidDataException("The Control Service returned an invalid event subscription response.");
        }

        ControlResponse subscriptionResponse = JsonSerializer.Deserialize<ControlResponse>(response.Payload)
            ?? throw new InvalidDataException("The Control Service returned an unreadable event subscription response.");
        if (!subscriptionResponse.IsSuccessful)
        {
            throw new InvalidOperationException(subscriptionResponse.Message);
        }

        if (subscriptionResponse.ProtocolWelcome == null)
        {
            throw new InvalidDataException("The Control Service did not provide event subscription compatibility metadata.");
        }

        onConnected(subscriptionResponse.ProtocolWelcome);

        foreach (OperationStatus status in subscriptionResponse.OperationStatuses)
        {
            await onEvent(new ControlClientEvent
            {
                EventType = ControlClientEventType.OperationStatusUpdated,
                PublishedUtc = DateTime.UtcNow,
                OperationStatus = status
            }).ConfigureAwait(false);
        }

        foreach (OperationDecision decision in subscriptionResponse.OperationDecisions)
        {
            await onEvent(new ControlClientEvent
            {
                EventType = ControlClientEventType.OperationDecisionUpdated,
                PublishedUtc = decision.CreatedUtc,
                OperationDecision = decision
            }).ConfigureAwait(false);
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            using CancellationTokenSource eventIdleTimeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            eventIdleTimeoutSource.CancelAfter(ControlProtocol.EventIdleTimeout);
            ControlEnvelope clientEventEnvelope;
            try
            {
                clientEventEnvelope = await ControlEnvelopeSerializer.ReadAsync(pipe, eventIdleTimeoutSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && eventIdleTimeoutSource.IsCancellationRequested)
            {
                throw new TimeoutException("The Control Service event subscription became idle and will be reconnected.", ex);
            }
            if (clientEventEnvelope == null || clientEventEnvelope.MessageType != ControlMessageType.ClientEvent)
            {
                throw new InvalidDataException("The Control Service closed the event subscription unexpectedly.");
            }

            ControlClientEvent clientEvent = JsonSerializer.Deserialize<ControlClientEvent>(clientEventEnvelope.Payload)
                ?? throw new InvalidDataException("The Control Service returned an unreadable client event.");
            await onEvent(clientEvent).ConfigureAwait(false);
        }
    }

    private static async Task<ControlResponse> SendAsync(ControlEnvelope request, CancellationToken cancellationToken)
    {
        request.Hello = ControlProtocol.CreateHello(ControlClientKind.DesktopApplication, "DisplayMagician.WinForms", "DisplayMagician");
        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.ClientPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(ControlProtocol.ConnectionTimeout, cancellationToken).ConfigureAwait(false);
        ControlEnvelope response = await SendAndReceiveEnvelopeAsync(pipe, request, cancellationToken).ConfigureAwait(false);

        ControlResponse controlResponse = JsonSerializer.Deserialize<ControlResponse>(response.Payload)
            ?? throw new InvalidDataException("The Control Service returned an unreadable response.");
        if (controlResponse.IsSuccessful && !ControlProtocol.IsCompatibleWelcome(request.Hello, controlResponse.ProtocolWelcome))
        {
            throw new InvalidDataException("The Control Service did not complete a compatible protocol negotiation.");
        }

        return controlResponse;
    }

    private static async Task<ControlEnvelope> SendAndReceiveEnvelopeAsync(NamedPipeClientStream pipe, ControlEnvelope request, CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(ControlProtocol.ResponseTimeout);
        try
        {
            await ControlEnvelopeSerializer.WriteAsync(pipe, request, timeoutSource.Token).ConfigureAwait(false);
            ControlEnvelope response = await ControlEnvelopeSerializer.ReadAsync(pipe, timeoutSource.Token).ConfigureAwait(false)
                ?? throw new InvalidDataException("The Control Service returned an invalid response.");
            if (response == null || response.RequestId != request.RequestId || response.MessageType != request.MessageType)
            {
                throw new InvalidDataException("The Control Service returned an invalid response.");
            }

            return response;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("The Control Service did not respond within the permitted time.");
        }
    }
}
