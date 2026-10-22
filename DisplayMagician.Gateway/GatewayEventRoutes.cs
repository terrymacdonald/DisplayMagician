using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NLog;

namespace DisplayMagician.Gateway;

/// <summary>Streams authorized journal records with a cursor that survives reconnects.</summary>
internal static class GatewayEventRoutes
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public static void Map(WebApplication app) => app.MapGet("/v1/events", StreamAsync);

    internal static async Task StreamAsync(HttpContext context, IGatewayEventClient client, GatewayEventStreamLimiter limiter)
    {
        GatewayAuthenticationResult authentication = GatewayRequestAuthenticationMiddleware.GetAuthentication(context);
        if (!HasReadGrant(authentication))
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status403Forbidden, "capability-denied", "Capability denied", "The paired device has no event read grant.").ConfigureAwait(false);
            return;
        }

        string[] suppliedIds = context.Request.Headers["Last-Event-ID"].Select(value => value ?? string.Empty).ToArray();
        if (suppliedIds.Length > 1 || suppliedIds.Length == 1 && (suppliedIds[0].Length is < 1 or > 128 || suppliedIds[0].Contains('\r') || suppliedIds[0].Contains('\n')))
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status400BadRequest, "cursor-invalid", "Invalid event cursor", "Last-Event-ID must be one opaque cursor of at most 128 characters.").ConfigureAwait(false);
            return;
        }

        using IDisposable? lease = limiter.TryEnter(authentication);
        if (lease == null)
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status429TooManyRequests, "rate-limited", "Too many event streams", "Close an existing stream before opening another.", true, 1).ConfigureAwait(false);
            return;
        }

        string? cursor = suppliedIds.Length == 1 ? suppliedIds[0] : null;
        GatewayEventReplay first;
        try
        {
            first = await client.ReadRemoteEventsAsync(authentication, cursor, context.RequestAborted).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or GatewayControlServiceUnavailableException)
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status503ServiceUnavailable, "target-unavailable", "Event history unavailable", "The event stream cannot start now.", true, 1).ConfigureAwait(false);
            return;
        }

        bool sendSnapshot = cursor == null || first.State != GatewayEventReplayState.Available;
        GatewayEventReplay? pendingReplay = sendSnapshot ? null : first;
        object? snapshot = null;
        string? resetReason = first.State == GatewayEventReplayState.CursorExpired ? "cursor-expired" : first.State == GatewayEventReplayState.CursorUnknown ? "cursor-unknown" : null;
        if (sendSnapshot)
        {
            try
            {
                // Capture the journal head first. Any edit during snapshot assembly is replayed after this ID.
                if (cursor != null) first = await client.ReadRemoteEventsAsync(authentication, null, context.RequestAborted).ConfigureAwait(false);
                Dictionary<string, object?> data = new Dictionary<string, object?>
                {
                    ["capturedAt"] = DateTime.UtcNow,
                    ["resetReason"] = resetReason
                };
                if (authentication.GrantedCapabilities.Contains(RemoteClientCapabilities.StatusRead, StringComparer.Ordinal))
                {
                    ControlResponse operations = await client.RemoteResourceAsync(ControlMessageType.ListRemoteOperations, authentication, string.Empty, context.RequestAborted).ConfigureAwait(false);
                    if (!operations.IsSuccessful) throw new InvalidOperationException(operations.Message);
                    OperationStatus[] ordered = operations.OperationStatuses.OrderByDescending(item => item.StartedUtc).ThenBy(item => item.OperationId).ToArray();
                    data["operations"] = ordered.Take(100).Select(GatewayResourceRoutes.ToOperation).ToArray();
                    data["operationsHasMore"] = ordered.Length > 100;
                }
                if (authentication.GrantedCapabilities.Contains(RemoteClientCapabilities.DecisionsRead, StringComparer.Ordinal))
                {
                    ControlResponse decisions = await client.RemoteResourceAsync(ControlMessageType.ListRemoteDecisions, authentication, string.Empty, context.RequestAborted).ConfigureAwait(false);
                    if (!decisions.IsSuccessful) throw new InvalidOperationException(decisions.Message);
                    OperationDecision[] ordered = decisions.OperationDecisions.OrderByDescending(item => item.CreatedUtc).ThenBy(item => item.PromptId).ToArray();
                    data["pendingDecisions"] = ordered.Take(100).Select(GatewayResourceRoutes.ToDecision).ToArray();
                    data["pendingDecisionsHasMore"] = ordered.Length > 100;
                }
                snapshot = data;
                cursor = first.HeadId;
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or GatewayControlServiceUnavailableException)
            {
                await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status503ServiceUnavailable, "target-unavailable", "Event snapshot unavailable", "The event stream cannot start now.", true, 1).ConfigureAwait(false);
                return;
            }
        }

        using CancellationTokenSource streamLifetime = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        streamLifetime.CancelAfter(TimeSpan.FromSeconds(5));
        CancellationToken streamToken = streamLifetime.Token;
        string credential = context.Request.Headers.Authorization.ToString().Substring(7);
        string sourceIp = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        try
        {
            GatewayAuthenticationResult current = await client.AuthenticateAsync(new GatewayAuthenticationRequest { BearerCredential = credential, Method = "GET", Path = "/v1/events", SourceIpAddress = sourceIp }, streamToken).ConfigureAwait(false);
            if (!SameAuthentication(authentication, current))
            {
                await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status401Unauthorized, "credential-invalid", "Credential changed", "Reconnect using the current paired-device credential.").ConfigureAwait(false);
                return;
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException or IOException or InvalidOperationException or GatewayControlServiceUnavailableException)
        {
            if (!context.RequestAborted.IsCancellationRequested) await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status503ServiceUnavailable, "target-unavailable", "Credential check unavailable", "The event stream cannot start now.", true, 1).ConfigureAwait(false);
            return;
        }
        streamLifetime.CancelAfter(TimeSpan.FromSeconds(5));
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "text/event-stream; charset=utf-8";
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers["X-Accel-Buffering"] = "no";
        try
        {
            await context.Response.StartAsync(streamToken).ConfigureAwait(false);
            if (snapshot != null) await WriteEventAsync(context, cursor!, "snapshot", snapshot, streamToken).ConfigureAwait(false);
            DateTime nextCredentialCheck = DateTime.UtcNow.AddSeconds(4);
            DateTime nextHeartbeat = DateTime.UtcNow.AddSeconds(10);
            while (!context.RequestAborted.IsCancellationRequested)
            {
                if (DateTime.UtcNow >= nextCredentialCheck)
                {
                    GatewayAuthenticationResult current = await client.AuthenticateAsync(new GatewayAuthenticationRequest { BearerCredential = credential, Method = "GET", Path = "/v1/events", SourceIpAddress = sourceIp }, streamToken).ConfigureAwait(false);
                    if (!SameAuthentication(authentication, current)) break;
                    streamLifetime.CancelAfter(TimeSpan.FromSeconds(5));
                    nextCredentialCheck = DateTime.UtcNow.AddSeconds(4);
                }

                GatewayEventReplay replay = pendingReplay ?? await client.ReadRemoteEventsAsync(authentication, cursor, streamToken).ConfigureAwait(false);
                pendingReplay = null;
                if (replay.State != GatewayEventReplayState.Available) break;
                foreach (GatewayEventRecord record in replay.Events)
                {
                    if (TryCreateEvent(record, out string name, out object? data)) await WriteEventAsync(context, record.Id, name, data!, streamToken).ConfigureAwait(false);
                }
                cursor = replay.NextCursor;
                if (replay.HasMore) continue;
                if (DateTime.UtcNow >= nextHeartbeat)
                {
                    await context.Response.WriteAsync(": keep-alive\n\n", streamToken).ConfigureAwait(false);
                    await context.Response.Body.FlushAsync(streamToken).ConfigureAwait(false);
                    nextHeartbeat = DateTime.UtcNow.AddSeconds(10);
                }
                await Task.Delay(TimeSpan.FromSeconds(1), streamToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (streamLifetime.IsCancellationRequested)
        {
            // The client disconnected or the host is shutting down.
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or GatewayControlServiceUnavailableException)
        {
            Logger.Warn(ex, "GatewayEventRoutes/StreamAsync: Event stream stopped for device {0}.", authentication.DeviceId);
        }
    }

    internal static bool TryCreateEvent(GatewayEventRecord record, out string name, out object? data)
    {
        (name, data) = record.EventType switch
        {
            ControlClientEventType.OperationStatusUpdated when record.OperationStatus != null => ("operation.updated", (object)new { operation = GatewayResourceRoutes.ToOperation(record.OperationStatus) }),
            ControlClientEventType.OperationDecisionUpdated when record.OperationDecision != null => ("decision.updated", (object)new { decision = GatewayResourceRoutes.ToDecision(record.OperationDecision) }),
            ControlClientEventType.DisplayProfileChanged when record.ResourceChange != null => ("display-profile.changed", record.ResourceChange),
            ControlClientEventType.AudioProfileChanged when record.ResourceChange != null => ("audio-profile.changed", record.ResourceChange),
            ControlClientEventType.ShortcutChanged when record.ResourceChange != null => ("shortcut.changed", record.ResourceChange),
            _ => (string.Empty, null)
        };
        return data != null;
    }

    internal static string FormatEvent(string id, string name, object data) =>
        "id: " + id + "\nevent: " + name + "\ndata: " + JsonSerializer.Serialize(data, JsonOptions) + "\n\n";

    private static bool SameAuthentication(GatewayAuthenticationResult expected, GatewayAuthenticationResult current) =>
        current.IsAuthenticated && current.DeviceId == expected.DeviceId && current.OwnerUserSid == expected.OwnerUserSid && current.PreferredSessionId == expected.PreferredSessionId &&
        current.GrantedCapabilities.OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(expected.GrantedCapabilities.OrderBy(item => item, StringComparer.Ordinal));

    private static async Task WriteEventAsync(HttpContext context, string id, string name, object data, CancellationToken token)
    {
        await context.Response.WriteAsync(FormatEvent(id, name, data), token).ConfigureAwait(false);
        await context.Response.Body.FlushAsync(token).ConfigureAwait(false);
    }

    private static bool HasReadGrant(GatewayAuthenticationResult authentication) => authentication.GrantedCapabilities.Any(capability =>
        capability is RemoteClientCapabilities.StatusRead or RemoteClientCapabilities.DecisionsRead or RemoteClientCapabilities.ProfilesRead or RemoteClientCapabilities.AudioProfilesRead or RemoteClientCapabilities.ShortcutsRead);
}
