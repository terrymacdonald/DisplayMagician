using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace DisplayMagician.Gateway;

/// <summary>Reserves each paired-device mutation before route work and replays its retained HTTP result.</summary>
public sealed class GatewayIdempotencyMiddleware
{
    private readonly RequestDelegate _next;

    public GatewayIdempotencyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, GatewayControlServiceClient controlServiceClient)
    {
        if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method) ||
            context.Request.Path == "/v1/pairing/status")
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        string key = context.Request.Headers["Idempotency-Key"].ToString();
        if (string.IsNullOrEmpty(key))
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status400BadRequest, "idempotency-key-required", "Idempotency key required", "Send a new lowercase UUID v4 Idempotency-Key for this mutation.").ConfigureAwait(false);
            return;
        }

        if (!Guid.TryParseExact(key, "D", out Guid parsedKey) || parsedKey.Version != 4 || parsedKey.ToString("D") != key)
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status400BadRequest, "idempotency-key-invalid", "Invalid idempotency key", "Idempotency-Key must be a lowercase UUID v4.").ConfigureAwait(false);
            return;
        }

        context.Request.EnableBuffering();
        using MemoryStream requestBody = new MemoryStream();
        await context.Request.Body.CopyToAsync(requestBody, context.RequestAborted).ConfigureAwait(false);
        context.Request.Body.Position = 0;
        string scopeHash;
        if (context.Request.Path == "/v1/pairing/request")
        {
            DevicePairingRequest? pairingRequest;
            try
            {
                pairingRequest = JsonSerializer.Deserialize<DevicePairingRequest>(requestBody.ToArray(), new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }
            catch (JsonException)
            {
                pairingRequest = null;
            }

            if (pairingRequest == null)
            {
                await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status400BadRequest, "validation-failed", "Invalid pairing request", "A valid pairing request body is required.").ConfigureAwait(false);
                return;
            }

            DevicePairingState pairingState = await controlServiceClient.ValidatePairingSubmissionAsync(pairingRequest, context.RequestAborted).ConfigureAwait(false);
            if (pairingState is DevicePairingState.Expired)
            {
                await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status410Gone, "pairing-expired", "Pairing expired", "The pairing session has expired.").ConfigureAwait(false);
                return;
            }

            if (pairingState == DevicePairingState.Unknown)
            {
                await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status503ServiceUnavailable, "target-unavailable", "Control Service unavailable", "The pairing session could not be checked.", true, 1).ConfigureAwait(false);
                return;
            }

            if (pairingState == DevicePairingState.Rejected)
            {
                await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status400BadRequest, "validation-failed", "Invalid pairing request", "The pairing session or request is invalid.").ConfigureAwait(false);
                return;
            }

            scopeHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("pairing\n" + pairingRequest.PairingSessionId.ToString("D") + "\n" + pairingRequest.PairingSecret)));
        }
        else
        {
            scopeHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("credential\n" + context.Request.Headers.Authorization.ToString().Substring(7))));
        }

        string rawTarget = context.Features.Get<IHttpRequestFeature>()?.RawTarget ?? context.Request.Path + context.Request.QueryString;
        byte[] fingerprint = JsonSerializer.SerializeToUtf8Bytes(new
        {
            method = context.Request.Method,
            rawTarget,
            contentType = context.Request.ContentType ?? string.Empty,
            body = Convert.ToBase64String(requestBody.ToArray())
        });
        GatewayHttpIdempotencyRequest reservation = new GatewayHttpIdempotencyRequest
        {
            ScopeHash = scopeHash,
            Key = key,
            RequestHash = Convert.ToHexString(SHA256.HashData(fingerprint))
        };

        GatewayHttpIdempotencyResult result;
        try
        {
            Stopwatch wait = Stopwatch.StartNew();
            do
            {
                result = await controlServiceClient.BeginIdempotencyAsync(reservation, context.RequestAborted).ConfigureAwait(false);
                if (result.State != GatewayHttpIdempotencyState.Pending) break;
                if (wait.Elapsed >= TimeSpan.FromSeconds(2)) break;
                await Task.Delay(100, context.RequestAborted).ConfigureAwait(false);
            } while (true);
        }
        catch (InvalidOperationException)
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status503ServiceUnavailable, "target-unavailable", "Control Service unavailable", "The mutation could not be reserved safely.", true, 1).ConfigureAwait(false);
            return;
        }

        if (result.State == GatewayHttpIdempotencyState.Conflict)
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status409Conflict, "idempotency-key-conflict", "Idempotency key conflict", "This key was already used for a different request.").ConfigureAwait(false);
            return;
        }

        if (result.State == GatewayHttpIdempotencyState.Pending)
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status409Conflict, "idempotency-pending", "Mutation still pending", "Retry the same request shortly.", true, 1).ConfigureAwait(false);
            return;
        }

        if (result.State == GatewayHttpIdempotencyState.Replay)
        {
            context.Response.StatusCode = result.StatusCode;
            context.Response.Headers.CacheControl = "no-store";
            if (!string.IsNullOrEmpty(result.ContentType)) context.Response.ContentType = result.ContentType;
            if (!string.IsNullOrEmpty(result.ResponseJson)) await context.Response.WriteAsync(result.ResponseJson, context.RequestAborted).ConfigureAwait(false);
            return;
        }

        Stream originalBody = context.Response.Body;
        using MemoryStream responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        try
        {
            await _next(context).ConfigureAwait(false);
            reservation.StatusCode = context.Response.StatusCode;
            reservation.ContentType = context.Response.ContentType ?? string.Empty;
            reservation.ResponseJson = Encoding.UTF8.GetString(responseBody.ToArray());
            if (reservation.ResponseJson.Length > 1024 * 1024)
            {
                throw new InvalidOperationException("The Gateway mutation response exceeded its size limit.");
            }

            await controlServiceClient.CompleteIdempotencyAsync(reservation, CancellationToken.None).ConfigureAwait(false);
            context.Response.Body = originalBody;
            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalBody, context.RequestAborted).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            context.Response.Body = originalBody;
            string requestId = context.Response.Headers[GatewayProtocolMiddleware.RequestIdHeaderName].ToString();
            context.Response.Clear();
            context.Response.Headers[GatewayProtocolMiddleware.RequestIdHeaderName] = requestId;
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status503ServiceUnavailable, "target-unavailable", "Mutation result unavailable", "The mutation result could not be retained safely. Retry with the same key.", true, 1).ConfigureAwait(false);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }
}
