using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.Gateway;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class GatewayRequestAuthenticationMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_BypassesPublicPairingRoute()
    {
        bool nextCalled = false;
        GatewayRequestAuthenticationMiddleware middleware = new GatewayRequestAuthenticationMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Path = "/v1/pairing/request";
        await middleware.InvokeAsync(context, new FakeClient());
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_RejectsProtectedRequestWithoutTimestamp()
    {
        GatewayRequestAuthenticationMiddleware middleware = new GatewayRequestAuthenticationMiddleware(_ => Task.CompletedTask);
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Path = "/v1/status";
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context, new FakeClient());
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_AllowsAuthenticatedProtectedRequestAndStoresTheContext()
    {
        bool nextCalled = false;
        GatewayRequestAuthenticationMiddleware middleware = new GatewayRequestAuthenticationMiddleware(context =>
        {
            nextCalled = GatewayRequestAuthenticationMiddleware.GetAuthentication(context).IsAuthenticated;
            return Task.CompletedTask;
        });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Path = "/v1/status";
        context.Request.Headers["X-DisplayMagician-Timestamp"] = DateTime.UtcNow.ToString("O");
        context.Request.Body = new MemoryStream();

        await middleware.InvokeAsync(context, new FakeClient());

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_RejectsAuthenticationFailureBeforeTheRoute()
    {
        bool nextCalled = false;
        GatewayRequestAuthenticationMiddleware middleware = new GatewayRequestAuthenticationMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Path = "/v1/status";
        context.Request.Headers["X-DisplayMagician-Timestamp"] = DateTime.UtcNow.ToString("O");
        context.Request.Body = new MemoryStream();

        await middleware.InvokeAsync(context, new FakeClient { Result = new GatewayAuthenticationResult { Message = "Invalid signature." } });

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    private sealed class FakeClient : IGatewayAuthenticationClient
    {
        public GatewayAuthenticationResult Result { get; set; } = new GatewayAuthenticationResult { IsAuthenticated = true, DeviceId = "device", OwnerUserSid = "user" };
        public Task<GatewayAuthenticationResult> AuthenticateAsync(GatewayAuthenticationRequest request, CancellationToken cancellationToken) => Task.FromResult(Result);
    }
}
