using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.Gateway;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class GatewayProtocolMiddlewareTests
{
    [Fact]
    public async Task MissingHelloRejectsPublicRequestWithCorrelatedProblem()
    {
        bool nextCalled = false;
        GatewayProtocolMiddleware middleware = new GatewayProtocolMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        DefaultHttpContext context = CreateContext();

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        using JsonDocument problem = JsonDocument.Parse(ReadResponse(context));
        Assert.Equal("protocol-hello-invalid", problem.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(context.Response.Headers[GatewayProtocolMiddleware.RequestIdHeaderName].ToString(), problem.RootElement.GetProperty("requestId").GetString());
    }

    [Theory]
    [InlineData(2, "protocol-version-incompatible", StatusCodes.Status409Conflict)]
    [InlineData(1, "protocol-capability-unavailable", StatusCodes.Status412PreconditionFailed)]
    public async Task IncompatibleHelloStopsBeforeRouteWork(int minimumVersion, string expectedCode, int expectedStatus)
    {
        bool nextCalled = false;
        GatewayProtocolMiddleware middleware = new GatewayProtocolMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        DefaultHttpContext context = CreateContext();
        ProtocolHello hello = ControlProtocol.CreateHello(ControlClientKind.RemoteApplication, "phone");
        hello.MinimumProtocolVersion = minimumVersion;
        hello.MaximumProtocolVersion = minimumVersion;
        if (minimumVersion == 1) hello.RequiredCapabilities = new[] { "unavailable-feature" };
        context.Request.Headers[GatewayProtocolMiddleware.HelloHeaderName] = Encode(hello);

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(expectedStatus, context.Response.StatusCode);
        using JsonDocument problem = JsonDocument.Parse(ReadResponse(context));
        Assert.Equal(expectedCode, problem.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task ValidHelloAddsWelcomeBeforePublicRouteResponse()
    {
        GatewayProtocolMiddleware middleware = new GatewayProtocolMiddleware(async context =>
        {
            await context.Response.StartAsync();
            await context.Response.WriteAsync("ok");
        });
        DefaultHttpContext context = CreateContext();
        ProtocolHello hello = ControlProtocol.CreateHello(ControlClientKind.RemoteApplication, "phone");
        hello.RequiredCapabilities = new[] { ControlCapabilities.ProtocolNegotiation };
        context.Request.Headers[GatewayProtocolMiddleware.HelloHeaderName] = Encode(hello);

        await middleware.InvokeAsync(context);

        Assert.Equal("ok", ReadResponse(context));
        string encodedWelcome = context.Response.Headers[GatewayProtocolMiddleware.WelcomeHeaderName].ToString();
        Assert.NotEmpty(encodedWelcome);
        using JsonDocument welcome = JsonDocument.Parse(Decode(encodedWelcome));
        Assert.Equal(ControlProtocol.CurrentVersion, welcome.RootElement.GetProperty("SelectedProtocolVersion").GetInt32());
        Assert.Equal("Gateway", welcome.RootElement.GetProperty("EndpointKind").GetString());
    }

    [Fact]
    public async Task DownstreamErrorDoesNotReturnWelcome()
    {
        GatewayProtocolMiddleware middleware = new GatewayProtocolMiddleware(context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        });
        DefaultHttpContext context = CreateContext();
        context.Request.Headers[GatewayProtocolMiddleware.HelloHeaderName] = Encode(ControlProtocol.CreateHello(ControlClientKind.RemoteApplication, "phone"));

        await middleware.InvokeAsync(context);

        Assert.False(context.Response.Headers.ContainsKey(GatewayProtocolMiddleware.WelcomeHeaderName));
        Assert.True(context.Response.Headers.ContainsKey(GatewayProtocolMiddleware.RequestIdHeaderName));
    }

    private static DefaultHttpContext CreateContext()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Path = "/v1/identity";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static string Encode(ProtocolHello hello) => Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(hello)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Decode(string value) => Convert.FromBase64String(value.Replace('-', '+').Replace('_', '/') + new string('=', (4 - value.Length % 4) % 4));

    private static string ReadResponse(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        using StreamReader reader = new StreamReader(context.Response.Body, leaveOpen: true);
        return reader.ReadToEnd();
    }
}
