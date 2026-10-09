using System.IO;
using System.Text;
using System.Threading.Tasks;
using DisplayMagician.Gateway;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class GatewayJsonResponseLimitMiddlewareTests
{
    [Fact]
    public async Task JsonAtLimitIsReturnedWithoutChanges()
    {
        string json = "\"" + new string('a', GatewayJsonResponseLimitMiddleware.MaximumJsonResponseBytes - 2) + "\"";
        GatewayJsonResponseLimitMiddleware middleware = new GatewayJsonResponseLimitMiddleware(async context =>
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(GatewayJsonResponseLimitMiddleware.MaximumJsonResponseBytes, context.Response.Body.Length);
    }

    [Fact]
    public async Task OversizedJsonReturnsSmallProblemWithSameRequestId()
    {
        GatewayJsonResponseLimitMiddleware middleware = new GatewayJsonResponseLimitMiddleware(context =>
            Results.Json(new { data = new string('a', GatewayJsonResponseLimitMiddleware.MaximumJsonResponseBytes) }).ExecuteAsync(context));
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Path = "/v1/display-profiles";
        context.Response.Headers[GatewayProtocolMiddleware.RequestIdHeaderName] = "original-request-id";
        context.Response.Body = new MemoryStream();
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("original-request-id", context.Response.Headers[GatewayProtocolMiddleware.RequestIdHeaderName]);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.True(context.Response.Body.Length < GatewayJsonResponseLimitMiddleware.MaximumJsonResponseBytes);
        context.Response.Body.Position = 0;
        using StreamReader reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        Assert.Contains("response-too-large", await reader.ReadToEndAsync());
    }
}
