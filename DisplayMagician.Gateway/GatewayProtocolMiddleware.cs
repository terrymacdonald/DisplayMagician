using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace DisplayMagician.Gateway;

/// <summary>Negotiates the shared protocol on every HTTP request before authentication or route work.</summary>
public sealed class GatewayProtocolMiddleware
{
    public const string HelloHeaderName = "DisplayMagician-Protocol-Hello";
    public const string WelcomeHeaderName = "DisplayMagician-Protocol-Welcome";
    public const string RequestIdHeaderName = "DisplayMagician-Request-Id";

    private const int MaximumEncodedHelloLength = 16 * 1024;
    private static readonly string[] SupportedCapabilities =
    {
        ControlCapabilities.ProtocolNegotiation,
        ControlCapabilities.OperationStatus,
        ControlCapabilities.OperationDecisions,
        ControlCapabilities.Profiles,
        ControlCapabilities.AudioProfiles,
        ControlCapabilities.Shortcuts,
        ControlCapabilities.ClientEvents
    };

    private readonly RequestDelegate _next;

    public GatewayProtocolMiddleware(RequestDelegate next) => _next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task InvokeAsync(HttpContext context)
    {
        string requestId = Guid.NewGuid().ToString("D");
        context.Response.Headers[RequestIdHeaderName] = requestId;

        if (!context.Request.Headers.TryGetValue(HelloHeaderName, out StringValues headerValues) || headerValues.Count != 1)
        {
            await RejectAsync(context, requestId, StatusCodes.Status400BadRequest, "protocol-hello-invalid", "Invalid protocol hello", "A protocol hello header is required.").ConfigureAwait(false);
            return;
        }

        string encodedHello = headerValues[0] ?? string.Empty;
        if (encodedHello.Length == 0 || encodedHello.Length > MaximumEncodedHelloLength || encodedHello.Length % 4 == 1 || encodedHello.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-' && character != '_'))
        {
            await RejectAsync(context, requestId, StatusCodes.Status400BadRequest, "protocol-hello-invalid", "Invalid protocol hello", "The protocol hello header is malformed.").ConfigureAwait(false);
            return;
        }

        ProtocolHello? hello;
        try
        {
            string padded = encodedHello.Replace('-', '+').Replace('_', '/') + new string('=', (4 - encodedHello.Length % 4) % 4);
            byte[] json = Convert.FromBase64String(padded);
            if (!string.Equals(Convert.ToBase64String(json).TrimEnd('=').Replace('+', '-').Replace('/', '_'), encodedHello, StringComparison.Ordinal))
            {
                throw new FormatException("The protocol hello was not canonically encoded.");
            }

            hello = JsonSerializer.Deserialize<ProtocolHello>(json);
        }
        catch (Exception exception) when (exception is FormatException or JsonException)
        {
            await RejectAsync(context, requestId, StatusCodes.Status400BadRequest, "protocol-hello-invalid", "Invalid protocol hello", "The protocol hello header is malformed.").ConfigureAwait(false);
            return;
        }

        if (!ControlProtocol.TryCreateWelcome(hello, "Gateway", SupportedCapabilities, out ProtocolWelcome? welcome, out ControlErrorCode errorCode, out string message))
        {
            int status = errorCode == ControlErrorCode.IncompatibleProtocolVersion ? StatusCodes.Status409Conflict
                : errorCode == ControlErrorCode.RequiredCapabilityUnavailable ? StatusCodes.Status412PreconditionFailed
                : StatusCodes.Status400BadRequest;
            string code = status == StatusCodes.Status409Conflict ? "protocol-version-incompatible"
                : status == StatusCodes.Status412PreconditionFailed ? "protocol-capability-unavailable"
                : "protocol-hello-invalid";
            await RejectAsync(context, requestId, status, code, "Protocol negotiation failed", message).ConfigureAwait(false);
            return;
        }

        string encodedWelcome = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(welcome)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        context.Response.Headers[WelcomeHeaderName] = encodedWelcome;
        context.Response.OnStarting(() =>
        {
            if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
            {
                context.Response.Headers.Remove(WelcomeHeaderName);
            }

            return Task.CompletedTask;
        });

        await _next(context).ConfigureAwait(false);
        if (context.Response.StatusCode >= StatusCodes.Status400BadRequest && !context.Response.HasStarted)
        {
            context.Response.Headers.Remove(WelcomeHeaderName);
        }
    }

    private static async Task RejectAsync(HttpContext context, string requestId, int status, string code, string title, string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        string body = JsonSerializer.Serialize(new
        {
            type = $"https://displaymagician.org/problems/{code}",
            title,
            status,
            detail,
            instance = context.Request.Path.Value ?? string.Empty,
            errorCode = code,
            requestId,
            retryable = false
        });
        await context.Response.WriteAsync(body, context.RequestAborted).ConfigureAwait(false);
    }
}
