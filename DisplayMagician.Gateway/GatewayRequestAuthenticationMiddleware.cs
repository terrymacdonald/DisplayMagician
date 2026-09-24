using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.AspNetCore.Http;

namespace DisplayMagician.Gateway;

/// <summary>Authenticates every protected Gateway request before a route can access ControlService data.</summary>
public sealed class GatewayRequestAuthenticationMiddleware
{
    public const string AuthenticationItemName = "DisplayMagician.Gateway.Authentication";
    private readonly RequestDelegate _next;

    public GatewayRequestAuthenticationMiddleware(RequestDelegate next) => _next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task InvokeAsync(HttpContext context, IGatewayAuthenticationClient controlServiceClient)
    {
        if (IsPublicPath(context.Request.Path))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        context.Request.EnableBuffering();
        string bodyHash;
        using (MemoryStream body = new MemoryStream())
        {
            await context.Request.Body.CopyToAsync(body, context.RequestAborted).ConfigureAwait(false);
            bodyHash = Convert.ToHexString(SHA256.HashData(body.ToArray()));
        }
        context.Request.Body.Position = 0;

        if (!DateTime.TryParse(context.Request.Headers["X-DisplayMagician-Timestamp"], null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime timestampUtc))
        {
            await RejectAsync(context, "A valid signed-request timestamp is required.").ConfigureAwait(false);
            return;
        }

        GatewayAuthenticationResult authentication = await controlServiceClient.AuthenticateAsync(new GatewayAuthenticationRequest
        {
            SignedRequest = new SignedGatewayRequest
            {
                DeviceId = context.Request.Headers["X-DisplayMagician-Device-Id"].ToString(),
                TimestampUtc = timestampUtc,
                Nonce = context.Request.Headers["X-DisplayMagician-Nonce"].ToString(),
                Signature = context.Request.Headers["X-DisplayMagician-Signature"].ToString()
            },
            Method = context.Request.Method,
            Path = context.Request.Path,
            BodySha256 = bodyHash,
            SourceIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty
        }, context.RequestAborted).ConfigureAwait(false);

        if (!authentication.IsAuthenticated)
        {
            await RejectAsync(context, authentication.Message).ConfigureAwait(false);
            return;
        }

        context.Items[AuthenticationItemName] = authentication;
        await _next(context).ConfigureAwait(false);
    }

    public static GatewayAuthenticationResult GetAuthentication(HttpContext context) => context.Items.TryGetValue(AuthenticationItemName, out object? value) && value is GatewayAuthenticationResult authentication ? authentication : throw new InvalidOperationException("The Gateway request was not authenticated.");

    private static bool IsPublicPath(PathString path) => path == "/v1/identity" || path == "/v1/pairing/request" || path == "/v1/pairing/status";

    private static async Task RejectAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { error = string.IsNullOrWhiteSpace(message) ? "Authentication failed." : message }).ConfigureAwait(false);
    }
}
