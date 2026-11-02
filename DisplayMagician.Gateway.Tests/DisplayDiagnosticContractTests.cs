using System;
using System.Text.Json;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class DisplayDiagnosticContractTests
{
    [Fact]
    public void SuccessfulOperationExposesWarningsWithoutAnOperationError()
    {
        OperationStatus status = new()
        {
            OperationId = Guid.NewGuid(), IsTerminal = true, IsSuccessful = true, Phase = OperationPhase.Completed,
            DisplayResults = new[] { new DisplayConfigurationResult { TopologyApplied = true, Issues = new[] {
                new DisplayConfigurationIssue { Stage = "HDR", Component = "Windows", NativeErrorCode = "5" } } } }
        };
        GatewayOperationView view = GatewayResourceRoutes.ToOperation(status);
        Assert.Equal("succeeded", view.Status);
        Assert.Null(view.Error);
        Assert.True(Assert.Single(view.DisplayResults).HasWarnings);
        status.DisplayResults[0].Issues[0].Stage = "Changed";
        using JsonDocument json = JsonDocument.Parse(JsonSerializer.Serialize(view, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        JsonElement issue = json.RootElement.GetProperty("displayResults")[0].GetProperty("issues")[0];
        Assert.Equal("HDR", issue.GetProperty("stage").GetString());
        Assert.Equal(0, issue.GetProperty("severity").GetInt32());
        Assert.Equal("5", issue.GetProperty("nativeErrorCode").GetString());
    }
}
