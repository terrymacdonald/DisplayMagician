using System;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.AspNetCore.Http;

namespace DisplayMagician.Gateway;

/// <summary>Authenticates every protected Gateway request before a route can access ControlService data.</summary>
public sealed class GatewayRequestAuthenticationMiddleware
{
    public const string AuthenticationItemName = "DisplayMagician.Gateway.Authentication";
    private readonly RequestDelegate _next;
    private readonly SemaphoreSlim _requestSlots = new SemaphoreSlim(32, 32);

    public GatewayRequestAuthenticationMiddleware(RequestDelegate next) => _next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task InvokeAsync(HttpContext context, IGatewayAuthenticationClient controlServiceClient, GatewayTrafficLimiter trafficLimiter, GatewayRegistrationState registrationState)
    {
        if (!await _requestSlots.WaitAsync(0, context.RequestAborted).ConfigureAwait(false))
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status429TooManyRequests, "rate-limited", "Too many requests", "The Gateway is busy; try again shortly.", true, 1).ConfigureAwait(false);
            return;
        }

        try
        {
        if (!registrationState.IsRegistered && context.Request.Path != "/v1/identity" && context.Request.Path != "/v1/capabilities")
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status503ServiceUnavailable, "target-unavailable", "Gateway not ready", "The Gateway identity is still registering with Control Service.", true, 1).ConfigureAwait(false);
            return;
        }

        if (IsPublicPath(context.Request))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        string sourceIp = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        DateTime now = DateTime.UtcNow;
        if (trafficLimiter.IsSourceBlocked(sourceIp, now))
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status429TooManyRequests, "rate-limited", "Too many requests", "Too many failed authentication attempts.", true, 60).ConfigureAwait(false);
            return;
        }

        string authorization = context.Request.Headers.Authorization.ToString();
        if (IsCandidatePollPath(context.Request))
        {
            trafficLimiter.RecordFailedAuthentication(sourceIp, now);
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status401Unauthorized, "authentication-required", "Pairing secret required", "Send the phone-generated polling secret in the DisplayMagician-Pairing Authorization scheme.", authenticationScheme: "DisplayMagician-Pairing").ConfigureAwait(false);
            return;
        }

        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) || authorization.Length <= 7 || authorization.AsSpan(7).Contains(' '))
        {
            trafficLimiter.RecordFailedAuthentication(sourceIp, now);
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status401Unauthorized, "authentication-required", "Authentication required", "A paired-device bearer credential is required.").ConfigureAwait(false);
            return;
        }

        GatewayAuthenticationResult authentication;
        try
        {
            authentication = await controlServiceClient.AuthenticateAsync(new GatewayAuthenticationRequest
            {
                BearerCredential = authorization.Substring(7),
                Method = context.Request.Method,
                Path = context.Request.Path,
                SourceIpAddress = sourceIp
            }, context.RequestAborted).ConfigureAwait(false);
        }
        catch (GatewayControlServiceUnavailableException)
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status503ServiceUnavailable, "target-unavailable", "Control Service unavailable", "The paired-device credential could not be verified now.", true, 1).ConfigureAwait(false);
            return;
        }

        if (!authentication.IsAuthenticated)
        {
            trafficLimiter.RecordFailedAuthentication(sourceIp, now);
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status401Unauthorized, "credential-invalid", "Credential invalid", "The paired-device credential is invalid.").ConfigureAwait(false);
            return;
        }

        if (!trafficLimiter.TryConsumeDeviceRequest(authentication.DeviceId, !HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method), now))
        {
            await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status429TooManyRequests, "rate-limited", "Too many requests", "The paired device has reached its request limit.", true, 60).ConfigureAwait(false);
            return;
        }

        context.Items[AuthenticationItemName] = authentication;
        context.Response.Headers.CacheControl = "no-store";
        await _next(context).ConfigureAwait(false);
        }
        finally
        {
            _requestSlots.Release();
        }
    }

    public static GatewayAuthenticationResult GetAuthentication(HttpContext context) => context.Items.TryGetValue(AuthenticationItemName, out object? value) && value is GatewayAuthenticationResult authentication ? authentication : throw new InvalidOperationException("The Gateway request was not authenticated.");

    private static bool IsPublicPath(HttpRequest request)
    {
        if (request.Path == "/v1/identity" || request.Path == "/v1/capabilities") return true;
        if (request.Path == "/v1/pairing-requests") return HttpMethods.IsPost(request.Method);
        return IsCandidatePollPath(request) && request.Headers.Authorization.ToString().StartsWith("DisplayMagician-Pairing ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCandidatePollPath(HttpRequest request) => HttpMethods.IsGet(request.Method) && request.Path.StartsWithSegments("/v1/pairing-requests", out PathString remaining) && remaining.HasValue && !remaining.Value!.Trim('/').Contains('/');
}
