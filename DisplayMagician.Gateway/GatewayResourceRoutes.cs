using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DisplayMagician.Gateway;

/// <summary>Approved paired-device resources backed by Control Service authority.</summary>
internal static class GatewayResourceRoutes
{
    public static void Map(WebApplication app)
    {
        MapPairingAndDevices(app);
        MapSavedResources(app);
        MapOperationsAndDecisions(app);
    }

    private static void MapPairingAndDevices(WebApplication app)
    {
        app.MapGet("/v1/pairing-requests", async (HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.RemoteResourceAsync(ControlMessageType.ListRemotePairingRequests, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), string.Empty, token).ConfigureAwait(false);
            return response.IsSuccessful
                ? Results.Ok(new PairingRequestPage { Items = response.DevicePairingRequests.Select(ToPairingItem).ToArray() })
                : GatewayResponseMapper.Map(context, response);
        });

        app.MapPut("/v1/pairing-requests/{pairingRequestId:guid}/decision", async (Guid pairingRequestId, PairingDecisionRequest body, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            RemotePairingDecisionCommand decision = new RemotePairingDecisionCommand { PairingRequestId = pairingRequestId, Decision = body.Decision };
            ControlResponse response = await client.RemoteResourceAsync(ControlMessageType.DecideRemotePairingRequest, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), JsonSerializer.Serialize(decision), token).ConfigureAwait(false);
            return response.IsSuccessful && response.DevicePairingRequest != null
                ? Results.Ok(ToPairingItem(response.DevicePairingRequest))
                : GatewayResponseMapper.Map(context, response);
        });

        app.MapGet("/v1/devices/current", async (HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.RemoteResourceAsync(ControlMessageType.GetRemoteCurrentDevice, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), string.Empty, token).ConfigureAwait(false);
            if (!response.IsSuccessful || response.PairedClients.Length == 0) return GatewayResponseMapper.Map(context, response);
            PairedClientView device = response.PairedClients[0];
            return Results.Ok(new CurrentDeviceView { Id = device.DeviceId, DisplayName = device.DisplayName, ClientType = device.ClientType, GrantedCapabilities = device.GrantedCapabilities });
        });

        app.MapGet("/v1/devices", async (HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.RemoteResourceAsync(ControlMessageType.ListRemoteDevices, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), string.Empty, token).ConfigureAwait(false);
            return response.IsSuccessful
                ? Results.Ok(new DevicePage { Items = response.PairedClients.Select(device => new DeviceItem { Id = device.DeviceId, DisplayName = device.DisplayName, ClientType = device.ClientType, GrantedCapabilities = device.GrantedCapabilities, PairedAt = device.PairedUtc, LastSeenAt = device.LastAuthenticatedUtc }).ToArray() })
                : GatewayResponseMapper.Map(context, response);
        });

        app.MapDelete("/v1/devices/{deviceId}", async (string deviceId, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.RemoteResourceAsync(ControlMessageType.RevokeRemoteDevice, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), deviceId, token).ConfigureAwait(false);
            return response.IsSuccessful ? Results.NoContent() : GatewayResponseMapper.Map(context, response);
        });
    }

    private static void MapSavedResources(WebApplication app)
    {
        app.MapGet("/v1/display-profiles", async (bool? includeArtwork, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.ListRemoteAsync(ControlMessageType.ListRemoteProfiles, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), null, token).ConfigureAwait(false);
            return response.IsSuccessful && response.ProfileList != null
                ? Results.Ok(new DisplayProfilePage { Items = response.ProfileList.SavedProfiles.Select(profile => ToDisplaySummary(profile, includeArtwork == true)).ToArray() })
                : GatewayResponseMapper.Map(context, response);
        });

        app.MapGet("/v1/display-profiles/{profileId}", async (string profileId, bool? includeArtwork, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.ListRemoteAsync(ControlMessageType.ListRemoteProfiles, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), null, token).ConfigureAwait(false);
            if (!response.IsSuccessful || response.ProfileList == null) return GatewayResponseMapper.Map(context, response);
            DisplayProfileView? profile = response.ProfileList.SavedProfiles.FirstOrDefault(item => string.Equals(item.Id, profileId, StringComparison.OrdinalIgnoreCase));
            return profile == null
                ? GatewayProblemDetails.CreateResult(context, StatusCodes.Status404NotFound, "resource-not-found", "Display profile not found", "The saved display profile is unavailable.")
                : Results.Ok(new DisplayProfileDetail { Id = profile.Id, Name = profile.Name, IsActive = profile.IsActive, IsValid = profile.IsValid, ConnectedDisplayCount = profile.ConnectedDisplayCount, Href = ResourceHref("display-profiles", profile.Id), ThumbnailPngBase64 = Artwork(profile.ThumbnailPngBase64, includeArtwork == true), PrimaryDisplayWidth = profile.PrimaryDisplayWidth, PrimaryDisplayHeight = profile.PrimaryDisplayHeight, DiagnosticMessage = profile.DiagnosticMessage });
        });

        app.MapPost("/v1/display-profiles/{profileId}/applications", async (string profileId, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            Guid operationId = GatewayIdempotencyMiddleware.GetOperationId(context);
            ControlResponse response = await client.ExecuteRemoteAsync(ControlMessageType.ApplyRemoteProfile, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), null, JsonSerializer.Serialize(new ApplyProfileRequest { ProfileId = profileId, OperationId = operationId }), token).ConfigureAwait(false);
            return AcceptedOperation(context, response, operationId);
        });

        app.MapGet("/v1/audio-profiles", async (HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.ListRemoteAsync(ControlMessageType.ListRemoteAudioProfiles, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), null, token).ConfigureAwait(false);
            return response.IsSuccessful && response.AudioProfileList != null
                ? Results.Ok(new AudioProfilePage { Items = response.AudioProfileList.SavedProfiles.Select(profile => new AudioProfileSummary { Id = profile.Id, Name = profile.Name, IsActive = profile.IsActive, Href = ResourceHref("audio-profiles", profile.Id) }).ToArray() })
                : GatewayResponseMapper.Map(context, response);
        });

        app.MapGet("/v1/audio-profiles/{profileId}", async (string profileId, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.ListRemoteAsync(ControlMessageType.ListRemoteAudioProfiles, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), null, token).ConfigureAwait(false);
            if (!response.IsSuccessful || response.AudioProfileList == null) return GatewayResponseMapper.Map(context, response);
            AudioProfileView? profile = response.AudioProfileList.SavedProfiles.FirstOrDefault(item => string.Equals(item.Id, profileId, StringComparison.OrdinalIgnoreCase));
            return profile == null
                ? GatewayProblemDetails.CreateResult(context, StatusCodes.Status404NotFound, "resource-not-found", "Audio profile not found", "The saved audio profile is unavailable.")
                : Results.Ok(new AudioProfileDetail { Id = profile.Id, Name = profile.Name, IsActive = profile.IsActive, Href = ResourceHref("audio-profiles", profile.Id), SettingsSummary = profile.SettingsText, UnavailableDeviceNames = profile.UnavailableDeviceNames });
        });

        app.MapPost("/v1/audio-profiles/{profileId}/applications", async (string profileId, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            Guid operationId = GatewayIdempotencyMiddleware.GetOperationId(context);
            ControlResponse response = await client.ExecuteRemoteAsync(ControlMessageType.ApplyRemoteAudioProfile, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), null, JsonSerializer.Serialize(new ApplyAudioProfileRequest { ProfileId = profileId, OperationId = operationId }), token).ConfigureAwait(false);
            return AcceptedOperation(context, response, operationId);
        });

        app.MapGet("/v1/shortcuts", async (bool? includeArtwork, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.ListRemoteAsync(ControlMessageType.ListRemoteShortcuts, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), null, token).ConfigureAwait(false);
            return response.IsSuccessful && response.ShortcutList != null
                ? Results.Ok(new ShortcutPage { Items = response.ShortcutList.Shortcuts.Select(shortcut => ToShortcutSummary(shortcut, includeArtwork == true)).ToArray() })
                : GatewayResponseMapper.Map(context, response);
        });

        app.MapGet("/v1/shortcuts/{shortcutId}", async (string shortcutId, bool? includeArtwork, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.ListRemoteAsync(ControlMessageType.ListRemoteShortcuts, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), null, token).ConfigureAwait(false);
            if (!response.IsSuccessful || response.ShortcutList == null) return GatewayResponseMapper.Map(context, response);
            ShortcutView? shortcut = response.ShortcutList.Shortcuts.FirstOrDefault(item => string.Equals(item.Id, shortcutId, StringComparison.OrdinalIgnoreCase));
            return shortcut == null
                ? GatewayProblemDetails.CreateResult(context, StatusCodes.Status404NotFound, "resource-not-found", "Shortcut not found", "The saved shortcut is unavailable.")
                : Results.Ok(new ShortcutDetail { Id = shortcut.Id, Name = shortcut.Name, Category = ShortcutCategoryName(shortcut.Category), Href = ResourceHref("shortcuts", shortcut.Id), IconPngBase64 = Artwork(shortcut.IconPngBase64, includeArtwork == true), DisplayProfileId = string.IsNullOrEmpty(shortcut.ProfileId) ? null : shortcut.ProfileId });
        });

        app.MapPost("/v1/shortcuts/{shortcutId}/runs", async (string shortcutId, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            Guid operationId = GatewayIdempotencyMiddleware.GetOperationId(context);
            ControlResponse response = await client.ExecuteRemoteAsync(ControlMessageType.StartRemoteShortcut, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), null, JsonSerializer.Serialize(new StartShortcutRequest { ShortcutId = shortcutId, OperationId = operationId }), token).ConfigureAwait(false);
            return AcceptedOperation(context, response, operationId);
        });
    }

    private static void MapOperationsAndDecisions(WebApplication app)
    {
        app.MapGet("/v1/operations", async (HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.RemoteResourceAsync(ControlMessageType.ListRemoteOperations, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), string.Empty, token).ConfigureAwait(false);
            return response.IsSuccessful
                ? Results.Ok(new GatewayOperationPage { Items = response.OperationStatuses.Select(ToOperation).ToArray() })
                : GatewayResponseMapper.Map(context, response);
        });

        app.MapGet("/v1/operations/{operationId:guid}", async (Guid operationId, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.RemoteResourceAsync(ControlMessageType.GetRemoteOperation, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), operationId.ToString("D"), token).ConfigureAwait(false);
            return response.IsSuccessful && response.OperationStatus != null ? Results.Ok(ToOperation(response.OperationStatus)) : GatewayResponseMapper.Map(context, response);
        });

        app.MapPost("/v1/operations/{operationId:guid}/cancellations", async (Guid operationId, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.RemoteResourceAsync(ControlMessageType.CancelRemoteOperation, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), operationId.ToString("D"), token).ConfigureAwait(false);
            if (!response.IsSuccessful) return GatewayResponseMapper.Map(context, response);
            context.Response.Headers.Location = $"/v1/operations/{operationId:D}";
            return Results.StatusCode(StatusCodes.Status202Accepted);
        });

        app.MapGet("/v1/decisions", async (string? status, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            if (status != "pending") return GatewayProblemDetails.CreateResult(context, StatusCodes.Status400BadRequest, "validation-failed", "Invalid decision filter", "Use status=pending to read the pending decision inbox.");
            ControlResponse response = await client.RemoteResourceAsync(ControlMessageType.ListRemoteDecisions, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), string.Empty, token).ConfigureAwait(false);
            return response.IsSuccessful ? Results.Ok(new GatewayDecisionPage { Items = response.OperationDecisions.Select(ToDecision).ToArray() }) : GatewayResponseMapper.Map(context, response);
        });

        app.MapGet("/v1/operations/{operationId:guid}/decisions", async (Guid operationId, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            ControlResponse response = await client.RemoteResourceAsync(ControlMessageType.ListRemoteDecisions, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), operationId.ToString("D"), token).ConfigureAwait(false);
            return response.IsSuccessful ? Results.Ok(new GatewayDecisionPage { Items = response.OperationDecisions.Select(ToDecision).ToArray() }) : GatewayResponseMapper.Map(context, response);
        });

        app.MapGet("/v1/operations/{operationId:guid}/decisions/{decisionId:guid}", async (Guid operationId, Guid decisionId, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            RemoteOperationDecisionRequest requested = new RemoteOperationDecisionRequest { OperationId = operationId, DecisionId = decisionId };
            ControlResponse response = await client.RemoteResourceAsync(ControlMessageType.GetRemoteDecision, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), JsonSerializer.Serialize(requested), token).ConfigureAwait(false);
            return response.IsSuccessful && response.OperationDecision != null ? Results.Ok(ToDecision(response.OperationDecision)) : GatewayResponseMapper.Map(context, response);
        });

        app.MapPut("/v1/operations/{operationId:guid}/decisions/{decisionId:guid}/response", async (Guid operationId, Guid decisionId, GatewayDecisionAnswerRequest body, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            OperationDecisionChoice choice = body.Choice switch { "continue" => OperationDecisionChoice.Continue, "stop-and-restore" => OperationDecisionChoice.StopAndRestore, _ => OperationDecisionChoice.Unknown };
            if (choice == OperationDecisionChoice.Unknown) return GatewayProblemDetails.CreateResult(context, StatusCodes.Status422UnprocessableEntity, "validation-failed", "Invalid decision choice", "Choose one of the decision's allowed choices.");
            RemoteOperationDecisionRequest answer = new RemoteOperationDecisionRequest { OperationId = operationId, DecisionId = decisionId, Choice = choice };
            ControlResponse response = await client.RemoteResourceAsync(ControlMessageType.AnswerRemoteDecision, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), JsonSerializer.Serialize(answer), token).ConfigureAwait(false);
            return response.IsSuccessful && response.OperationDecision != null ? Results.Ok(ToDecision(response.OperationDecision)) : GatewayResponseMapper.Map(context, response);
        });
    }

    internal static PairingRequestItem ToPairingItem(DevicePairingSessionView session) => new PairingRequestItem
    {
        PairingRequestId = session.PairingSessionId,
        Status = session.State switch { DevicePairingState.Approved => "approved", DevicePairingState.Rejected => "rejected", DevicePairingState.Expired => "expired", _ => "awaiting-approval" },
        DeviceId = session.DeviceId,
        DisplayName = session.DeviceDisplayName,
        ClientType = session.ClientType,
        RequestedCapabilities = session.RequestedCapabilities,
        GrantedCapabilities = session.GrantedCapabilities,
        CreatedAt = session.CreatedUtc,
        ExpiresAt = session.ExpiresUtc
    };

    private static DisplayProfileSummary ToDisplaySummary(DisplayProfileView profile, bool includeArtwork) => new DisplayProfileSummary { Id = profile.Id, Name = profile.Name, IsActive = profile.IsActive, IsValid = profile.IsValid, ConnectedDisplayCount = profile.ConnectedDisplayCount, Href = ResourceHref("display-profiles", profile.Id), ThumbnailPngBase64 = Artwork(profile.ThumbnailPngBase64, includeArtwork) };
    private static ShortcutSummary ToShortcutSummary(ShortcutView shortcut, bool includeArtwork) => new ShortcutSummary { Id = shortcut.Id, Name = shortcut.Name, Category = ShortcutCategoryName(shortcut.Category), Href = ResourceHref("shortcuts", shortcut.Id), IconPngBase64 = Artwork(shortcut.IconPngBase64, includeArtwork) };
    private static string ShortcutCategoryName(ShortcutCategory category) => category switch { ShortcutCategory.Game => "game", ShortcutCategory.NoGame => "no-game", ShortcutCategory.Application => "application", _ => "executable" };
    private static string ResourceHref(string collection, string id) => $"/v1/{collection}/{Uri.EscapeDataString(id)}";
    private static string? Artwork(string? base64, bool include)
    {
        if (!include || string.IsNullOrEmpty(base64) || base64.Length % 4 != 0) return null;
        int padding = base64.EndsWith("==", StringComparison.Ordinal) ? 2 : base64.EndsWith("=", StringComparison.Ordinal) ? 1 : 0;
        return (long)base64.Length / 4 * 3 - padding <= 262144 ? base64 : null;
    }
    private static IResult AcceptedOperation(HttpContext context, ControlResponse response, Guid operationId)
    {
        if (!response.IsSuccessful) return GatewayResponseMapper.Map(context, response);
        string href = $"/v1/operations/{operationId:D}";
        context.Response.Headers.Location = href;
        return Results.Json(new OperationAccepted { OperationId = operationId, Href = href }, statusCode: StatusCodes.Status202Accepted);
    }

    private static GatewayOperationView ToOperation(OperationStatus operation)
    {
        string status = operation.IsTerminal ? operation.IsSuccessful ? "succeeded" : operation.Phase == OperationPhase.Cancelled ? "cancelled" : "failed" : operation.Phase == OperationPhase.AwaitingUserDecision ? "waiting-for-decision" : operation.Phase == OperationPhase.Requested ? "requested" : "running";
        string href = $"/v1/operations/{operation.OperationId:D}";
        return new GatewayOperationView
        {
            Id = operation.OperationId,
            Type = operation.OperationType switch { DisplayOperationType.ApplyDisplayProfile => "apply-display-profile", DisplayOperationType.ApplyAudioProfile => "apply-audio-profile", DisplayOperationType.StartShortcut => "run-shortcut", _ => "restore-temporary-state" },
            Status = status,
            Phase = operation.Phase switch { OperationPhase.AwaitingUserDecision => "waiting-for-decision", OperationPhase.WaitingForGameToStart => "waiting-for-game-to-start", OperationPhase.WaitingForGameToClose => "waiting-for-game-to-close", _ => string.Concat(operation.Phase.ToString().SelectMany((character, index) => char.IsUpper(character) && index > 0 ? new[] { '-', char.ToLowerInvariant(character) } : new[] { char.ToLowerInvariant(character) })) },
            Message = operation.Message,
            Sequence = operation.Sequence,
            StartedAt = operation.StartedUtc,
            UpdatedAt = operation.UpdatedUtc,
            CompletedAt = operation.IsTerminal ? operation.UpdatedUtc : null,
            IsFinished = operation.IsTerminal,
            IsStale = operation.IsStale,
            StaleReason = operation.IsStale ? operation.StaleReason : null,
            CanCancel = !operation.IsTerminal && !operation.IsStale && operation.IsAuthoritative && operation.OperationType == DisplayOperationType.StartShortcut,
            Error = status == "failed" ? new GatewayOperationError { Code = operation.ErrorCode == ControlErrorCode.AgentUnavailable ? "target-unavailable" : "execution-failed", Message = operation.Message, Retryable = false } : null,
            Links = new GatewayOperationLinks { Self = href, Decisions = href + "/decisions", Cancellations = href + "/cancellations" }
        };
    }

    private static GatewayDecisionView ToDecision(OperationDecision decision)
    {
        string operationHref = $"/v1/operations/{decision.OperationId:D}";
        return new GatewayDecisionView
        {
            Id = decision.PromptId,
            OperationId = decision.OperationId,
            Status = decision.IsResolved ? "resolved" : "pending",
            Title = decision.Title,
            Message = decision.Message,
            AllowedChoices = decision.AllowedChoices.Select(ChoiceName).ToArray(),
            DefaultChoice = ChoiceName(decision.DefaultChoice),
            CreatedAt = decision.CreatedUtc,
            ExpiresAt = decision.ExpiresUtc,
            ResolvedChoice = decision.IsResolved ? ChoiceName(decision.ResolvedChoice) : null,
            ResolvedAt = decision.ResolvedUtc,
            Href = operationHref + $"/decisions/{decision.PromptId:D}",
            OperationHref = operationHref
        };
    }

    private static string ChoiceName(OperationDecisionChoice choice) => choice == OperationDecisionChoice.StopAndRestore ? "stop-and-restore" : "continue";
}
