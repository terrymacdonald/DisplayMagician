using System.IO;
using System.Threading.Tasks;
using DisplayMagician.Gateway;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class GatewaySavedResourceETagMiddlewareTests
{
    [Fact]
    public async Task SavedResourceReturnsStrongETagAndEmptyConditionalResponse()
    {
        int executed = 0;
        GatewaySavedResourceETagMiddleware middleware = new GatewaySavedResourceETagMiddleware(async context =>
        {
            executed++;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"items\":[1,2]}");
        });
        DefaultHttpContext first = new DefaultHttpContext();
        first.Request.Method = "GET";
        first.Request.Path = "/v1/display-profiles";
        first.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(first);

        string etag = first.Response.Headers.ETag.ToString();
        Assert.StartsWith("\"", etag);
        Assert.EndsWith("\"", etag);
        Assert.Equal("private, no-cache", first.Response.Headers.CacheControl.ToString());
        Assert.Equal(StatusCodes.Status200OK, first.Response.StatusCode);

        DefaultHttpContext second = new DefaultHttpContext();
        second.Request.Method = "GET";
        second.Request.Path = "/v1/display-profiles";
        second.Request.Headers.IfNoneMatch = etag;
        second.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(second);

        Assert.Equal(2, executed);
        Assert.Equal(StatusCodes.Status304NotModified, second.Response.StatusCode);
        Assert.Equal(etag, second.Response.Headers.ETag.ToString());
        Assert.Equal(0, second.Response.Body.Length);
    }

    [Fact]
    public async Task OperationStatusDoesNotReceiveSavedResourceETag()
    {
        GatewaySavedResourceETagMiddleware middleware = new GatewaySavedResourceETagMiddleware(async context =>
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{}");
        });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/v1/operations";
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context);

        Assert.False(context.Response.Headers.ContainsKey("ETag"));
    }
}
