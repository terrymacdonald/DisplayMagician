using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DisplayMagician.Gateway;

/// <summary>Writes the shared REST error shape without including credentials or query parameters.</summary>
public static class GatewayProblemDetails
{
    public static async Task WriteAsync(HttpContext context, int status, string code, string title, string detail, bool retryable = false, int? retryAfterSeconds = null)
    {
        object payload = CreatePayload(context, code, title, status, detail, retryable, retryAfterSeconds);
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload), context.RequestAborted).ConfigureAwait(false);
    }

    public static IResult CreateResult(HttpContext context, int status, string code, string title, string detail, bool retryable = false, int? retryAfterSeconds = null)
    {
        return Results.Json(CreatePayload(context, code, title, status, detail, retryable, retryAfterSeconds), statusCode: status, contentType: "application/problem+json");
    }

    private static object CreatePayload(HttpContext context, string code, string title, int status, string detail, bool retryable, int? retryAfterSeconds)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (status == StatusCodes.Status401Unauthorized) context.Response.Headers.WWWAuthenticate = "Bearer";
        if (retryAfterSeconds.HasValue)
        {
            context.Response.Headers.RetryAfter = retryAfterSeconds.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        string requestId = context.Response.Headers[GatewayProtocolMiddleware.RequestIdHeaderName].ToString();
        if (string.IsNullOrWhiteSpace(requestId))
        {
            requestId = Guid.NewGuid().ToString("D");
            context.Response.Headers[GatewayProtocolMiddleware.RequestIdHeaderName] = requestId;
        }

        return new
        {
            type = $"https://displaymagician.org/problems/{code}",
            title,
            status,
            detail,
            instance = context.Request.Path.Value ?? string.Empty,
            errorCode = code,
            requestId,
            retryable
        };
    }
}
