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

    [Fact]
    public async Task MissingResourcePayloadDoesNotExposeTheControlResponse()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Path = "/v1/operations/00000000-0000-0000-0000-000000000001";
        context.Response.Body = new MemoryStream();
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await GatewayResponseMapper.Map(context, new ControlResponse { IsSuccessful = true, Message = "internal session detail" }).ExecuteAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using JsonDocument problem = JsonDocument.Parse(context.Response.Body);
        Assert.Equal("internal-error", problem.RootElement.GetProperty("errorCode").GetString());
        Assert.DoesNotContain("internal session detail", problem.RootElement.GetRawText());
    }

    [Fact]
    public async Task PairingChallengeUsesItsOwnAuthorizationScheme()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Path = "/v1/pairing-requests/00000000-0000-0000-0000-000000000001";
        context.Response.Body = new MemoryStream();
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await GatewayProblemDetails.CreateResult(context, StatusCodes.Status401Unauthorized, "authentication-required", "Pairing secret required", "Send the polling secret.", authenticationScheme: "DisplayMagician-Pairing").ExecuteAsync(context);

        Assert.Equal("DisplayMagician-Pairing", context.Response.Headers.WWWAuthenticate.ToString());
    }

    [Theory]
    [InlineData(ControlErrorCode.SessionLocked, StatusCodes.Status503ServiceUnavailable, "target-unavailable")]
    [InlineData(ControlErrorCode.RecoveryRequired, StatusCodes.Status409Conflict, "recovery-conflict")]
    [InlineData(ControlErrorCode.ResourceNotFound, StatusCodes.Status404NotFound, "resource-not-found")]
    [InlineData(ControlErrorCode.PairingRequired, StatusCodes.Status401Unauthorized, "authentication-required")]
    [InlineData(ControlErrorCode.ValidationFailed, StatusCodes.Status422UnprocessableEntity, "validation-failed")]
    [InlineData(ControlErrorCode.CallerIdentityMismatch, StatusCodes.Status500InternalServerError, "internal-error")]
    public async Task SharedFailuresUseTheDocumentedHttpMapping(ControlErrorCode errorCode, int expectedStatus, string expectedCode)
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Path = "/v1/operations";
        context.Response.Body = new MemoryStream();
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        await GatewayResponseMapper.Map(context, new ControlResponse { ErrorCode = errorCode }).ExecuteAsync(context);

        Assert.Equal(expectedStatus, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using JsonDocument problem = JsonDocument.Parse(context.Response.Body);
        Assert.Equal(expectedCode, problem.RootElement.GetProperty("errorCode").GetString());
        if (expectedStatus == StatusCodes.Status503ServiceUnavailable)
        {
            Assert.True(problem.RootElement.GetProperty("retryable").GetBoolean());
            Assert.Equal("1", context.Response.Headers.RetryAfter.ToString());
        }
    }
}
