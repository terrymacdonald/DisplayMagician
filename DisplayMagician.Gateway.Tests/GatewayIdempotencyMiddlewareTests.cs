using System;
using System.IO;
using System.Threading.Tasks;
using DisplayMagician.Gateway;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class GatewayIdempotencyMiddlewareTests
{
    [Fact]
    public async Task MutationRequiresCanonicalUuidV4BeforeRouteWork()
    {
        bool executed = false;
        GatewayIdempotencyMiddleware middleware = new GatewayIdempotencyMiddleware(_ => { executed = true; return Task.CompletedTask; });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/v1/display-profiles/example/applications";
        context.Response.Body = new MemoryStream();
        GatewayControlServiceClient client = new GatewayControlServiceClient(new HttpContextAccessor());

        await middleware.InvokeAsync(context, client);
        Assert.False(executed);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        context.Response.Body = new MemoryStream();
        context.Request.Headers["Idempotency-Key"] = Guid.NewGuid().ToString("D").ToUpperInvariant();
        await middleware.InvokeAsync(context, client);
        Assert.False(executed);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }
}
