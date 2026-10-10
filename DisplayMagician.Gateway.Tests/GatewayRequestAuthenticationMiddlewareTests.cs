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
        context.Request.Method = "POST";
        context.Request.Path = "/v1/pairing-requests";
        await middleware.InvokeAsync(context, new FakeClient(), new GatewayTrafficLimiter(), RegisteredState());
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_BlocksPairingUntilCurrentIdentityIsRegistered()
    {
        bool nextCalled = false;
        GatewayRequestAuthenticationMiddleware middleware = new GatewayRequestAuthenticationMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/v1/pairing-requests";
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context, new FakeClient(), new GatewayTrafficLimiter(), new GatewayRegistrationState());
        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_RejectsProtectedRequestWithoutBearerCredential()
    {
        GatewayRequestAuthenticationMiddleware middleware = new GatewayRequestAuthenticationMiddleware(_ => Task.CompletedTask);
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Path = "/v1/operations";
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context, new FakeClient(), new GatewayTrafficLimiter(), RegisteredState());
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
        context.Request.Path = "/v1/operations";
        context.Request.Headers.Authorization = "Bearer sample-credential";
        context.Request.Body = new MemoryStream();

        await middleware.InvokeAsync(context, new FakeClient(), new GatewayTrafficLimiter(), RegisteredState());

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_RejectsAuthenticationFailureBeforeTheRoute()
    {
        bool nextCalled = false;
        GatewayRequestAuthenticationMiddleware middleware = new GatewayRequestAuthenticationMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Path = "/v1/operations";
        context.Request.Headers.Authorization = "Bearer sample-credential";
        context.Request.Body = new MemoryStream();

        await middleware.InvokeAsync(context, new FakeClient { Result = new GatewayAuthenticationResult { Message = "Invalid credential." } }, new GatewayTrafficLimiter(), RegisteredState());

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    private sealed class FakeClient : IGatewayAuthenticationClient
    {
        public GatewayAuthenticationResult Result { get; set; } = new GatewayAuthenticationResult { IsAuthenticated = true, DeviceId = "device", OwnerUserSid = "user" };
        public Task<GatewayAuthenticationResult> AuthenticateAsync(GatewayAuthenticationRequest request, CancellationToken cancellationToken) => Task.FromResult(Result);
    }

    private static GatewayRegistrationState RegisteredState()
    {
        GatewayRegistrationState state = new GatewayRegistrationState();
        state.SetRegistered(true);
        return state;
    }
}
