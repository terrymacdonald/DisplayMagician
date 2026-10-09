using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.Gateway;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class GatewayResponseMapperTests
{
    [Fact]
    public async Task CapabilityFailureIsAForbiddenProblemWithoutInternalDetails()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Path = "/v1/shortcuts";
        context.Response.Body = new MemoryStream();
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();
        await GatewayResponseMapper.Map(context, new ControlResponse { ErrorCode = ControlErrorCode.Unauthorized, Message = "secret internal detail" }).ExecuteAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using JsonDocument problem = JsonDocument.Parse(context.Response.Body);
        Assert.Equal("capability-denied", problem.RootElement.GetProperty("errorCode").GetString());
        Assert.DoesNotContain("secret internal detail", problem.RootElement.GetRawText());
    }
}
