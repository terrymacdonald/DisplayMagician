using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DisplayMagician.Gateway;

/// <summary>Keeps non-streaming JSON replies within the first-release response bound.</summary>
public sealed class GatewayJsonResponseLimitMiddleware
{
    public const int MaximumJsonResponseBytes = 1024 * 1024;
    private readonly RequestDelegate _next;

    public GatewayJsonResponseLimitMiddleware(RequestDelegate next) => _next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/v1/status/stream")
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        Stream originalBody = context.Response.Body;
        using MemoryStream bufferedBody = new MemoryStream();
        context.Response.Body = bufferedBody;
        try
        {
            await _next(context).ConfigureAwait(false);
            context.Response.Body = originalBody;

            if (context.Response.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true &&
                bufferedBody.Length > MaximumJsonResponseBytes)
            {
                string requestId = context.Response.Headers[GatewayProtocolMiddleware.RequestIdHeaderName].ToString();
                context.Response.Clear();
                context.Response.Headers[GatewayProtocolMiddleware.RequestIdHeaderName] = requestId;
                await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status500InternalServerError,
                    "response-too-large", "Response too large", "The result exceeds the Gateway's 1 MiB JSON response limit.").ConfigureAwait(false);
                return;
            }

            bufferedBody.Position = 0;
            await bufferedBody.CopyToAsync(originalBody, context.RequestAborted).ConfigureAwait(false);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }
}
