using System;
using System.Text.Json;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class OperationRepresentationTests
{
    [Fact]
    public void OperationJsonPreservesLargeSequencesAndDisplayWarnings()
    {
        Guid operationId = Guid.NewGuid();
        GatewayOperationView operation = GatewayResourceRoutes.ToOperation(new OperationStatus
        {
            OperationId = operationId,
            OperationType = DisplayOperationType.StartShortcut,
            Phase = OperationPhase.WaitingForGameToClose,
            Message = "Game running",
            DisplayResults = new[] { new DisplayConfigurationResult { ProfileId = "test-profile", TopologyApplied = true,
                Issues = new[] { new DisplayConfigurationIssue { Stage = "HDR", Component = "Windows", DisplayId = "2", Message = "Could not enable HDR.", NativeErrorCode = "5" } } } },
            Sequence = 4294967296,
            StartedUtc = new DateTime(2026, 10, 22, 0, 0, 0, DateTimeKind.Utc),
            UpdatedUtc = new DateTime(2026, 10, 22, 0, 1, 0, DateTimeKind.Utc)
        });

        using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(operation, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        JsonElement json = document.RootElement;
        Assert.Equal(operationId, json.GetProperty("id").GetGuid());
        Assert.Equal("run-shortcut", json.GetProperty("type").GetString());
        Assert.Equal("running", json.GetProperty("status").GetString());
        Assert.Equal("waiting-for-game-to-close", json.GetProperty("phase").GetString());
        Assert.Equal("Game running", json.GetProperty("message").GetString());
        Assert.Equal(4294967296L, json.GetProperty("sequence").GetInt64());
        Assert.Equal(new DateTime(2026, 10, 22, 0, 0, 0, DateTimeKind.Utc), json.GetProperty("startedAt").GetDateTime());
        Assert.Equal(new DateTime(2026, 10, 22, 0, 1, 0, DateTimeKind.Utc), json.GetProperty("updatedAt").GetDateTime());
        Assert.False(json.GetProperty("isFinished").GetBoolean());
        Assert.False(json.GetProperty("isStale").GetBoolean());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("completedAt").ValueKind);
        Assert.Equal(JsonValueKind.Null, json.GetProperty("staleReason").ValueKind);
        Assert.Equal(JsonValueKind.Null, json.GetProperty("source").ValueKind);
        Assert.True(json.GetProperty("canCancel").GetBoolean());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("error").ValueKind);
        JsonElement links = json.GetProperty("links");
        Assert.Equal($"/v1/operations/{operationId:D}", links.GetProperty("self").GetString());
        Assert.Equal($"/v1/operations/{operationId:D}/decisions", links.GetProperty("decisions").GetString());
        Assert.Equal($"/v1/operations/{operationId:D}/cancellations", links.GetProperty("cancellations").GetString());
        JsonElement details = json.GetProperty("displayResults")[0];
        Assert.True(details.GetProperty("isSuccessful").GetBoolean());
        Assert.True(details.GetProperty("hasWarnings").GetBoolean());
        Assert.Equal("5", details.GetProperty("issues")[0].GetProperty("nativeErrorCode").GetString());
    }
}
