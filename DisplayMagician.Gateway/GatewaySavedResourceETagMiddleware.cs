using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DisplayMagician.Gateway;

/// <summary>Conditional revalidation for saved profile and shortcut representations.</summary>
public sealed class GatewaySavedResourceETagMiddleware
{
    private readonly RequestDelegate _next;

    public GatewaySavedResourceETagMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method) ||
            !context.Request.Path.StartsWithSegments("/v1/display-profiles") &&
            !context.Request.Path.StartsWithSegments("/v1/audio-profiles") &&
            !context.Request.Path.StartsWithSegments("/v1/shortcuts"))
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
            if (context.Response.StatusCode == StatusCodes.Status200OK &&
                context.Response.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true)
            {
                string etag = "\"" + Convert.ToHexString(SHA256.HashData(bufferedBody.ToArray())) + "\"";
                context.Response.Headers.ETag = etag;
                context.Response.Headers.CacheControl = "private, no-cache";
                string[] candidates = context.Request.Headers.IfNoneMatch.ToString().Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (Array.Exists(candidates, candidate => candidate == "*" || string.Equals(
                    candidate.StartsWith("W/", StringComparison.Ordinal) ? candidate.Substring(2) : candidate, etag, StringComparison.Ordinal)))
                {
                    context.Response.StatusCode = StatusCodes.Status304NotModified;
                    context.Response.ContentType = null;
                    context.Response.ContentLength = null;
                    return;
                }
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
