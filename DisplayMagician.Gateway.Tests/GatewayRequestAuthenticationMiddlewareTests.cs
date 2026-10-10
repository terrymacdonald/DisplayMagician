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
    public async Task InvokeAsync_AllowsCandidatePollingOnlyOnItsSingleResourceRoute()
    {
        bool nextCalled = false;
        GatewayRequestAuthenticationMiddleware middleware = new GatewayRequestAuthenticationMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/v1/pairing-requests/00000000-0000-0000-0000-000000000001";
        context.Request.Headers.Authorization = "DisplayMagician-Pairing polling-secret";

        await middleware.InvokeAsync(context, new FakeClient { Result = new GatewayAuthenticationResult() }, new GatewayTrafficLimiter(), RegisteredState());

        Assert.True(nextCalled);

        nextCalled = false;
        context.Request.Path = "/v1/pairing-requests/00000000-0000-0000-0000-000000000001/decision";
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context, new FakeClient { Result = new GatewayAuthenticationResult() }, new GatewayTrafficLimiter(), RegisteredState());
        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_RejectsBearerOnCandidatePollWithPairingChallenge()
    {
        bool nextCalled = false;
        GatewayRequestAuthenticationMiddleware middleware = new GatewayRequestAuthenticationMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/v1/pairing-requests/00000000-0000-0000-0000-000000000001";
        context.Request.Headers.Authorization = "Bearer sample-credential";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, new FakeClient(), new GatewayTrafficLimiter(), RegisteredState());

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal("DisplayMagician-Pairing", context.Response.Headers.WWWAuthenticate.ToString());
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

    [Fact]
    public async Task InvokeAsync_ReportsControlServiceOutageAsRetryableServiceFailure()
    {
        bool nextCalled = false;
        GatewayRequestAuthenticationMiddleware middleware = new GatewayRequestAuthenticationMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Path = "/v1/operations";
        context.Request.Headers.Authorization = "Bearer sample-credential";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, new FakeClient { Error = new GatewayControlServiceUnavailableException() }, new GatewayTrafficLimiter(), RegisteredState());

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        Assert.Equal("1", context.Response.Headers.RetryAfter.ToString());
    }

    private sealed class FakeClient : IGatewayAuthenticationClient
    {
        public GatewayAuthenticationResult Result { get; set; } = new GatewayAuthenticationResult { IsAuthenticated = true, DeviceId = "device", OwnerUserSid = "user" };
        public Exception? Error { get; set; }
        public Task<GatewayAuthenticationResult> AuthenticateAsync(GatewayAuthenticationRequest request, CancellationToken cancellationToken) => Error == null ? Task.FromResult(Result) : Task.FromException<GatewayAuthenticationResult>(Error);
    }

    private static GatewayRegistrationState RegisteredState()
    {
        GatewayRegistrationState state = new GatewayRegistrationState();
        state.SetRegistered(true);
        return state;
    }
}
