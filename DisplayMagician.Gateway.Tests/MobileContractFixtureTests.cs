using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using DisplayMagician.Contracts;
using DisplayMagician.Gateway;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class MobileContractFixtureTests
{
    [Fact]
    public void NativeClientFixtureMatchesTheActualGatewayOperationRepresentation()
    {
        JsonNode fixture = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "mobile_contract.json")))!;
        GatewayOperationView operation = GatewayResourceRoutes.ToOperation(new OperationStatus
        {
            OperationId = Guid.Parse("4eca79f6-8fcf-418c-b55d-61dfa14ba5bd"),
            OperationType = DisplayOperationType.StartShortcut,
            Phase = OperationPhase.WaitingForGameToClose,
            Message = "Game running",
            Sequence = 4294967296,
            StartedUtc = new DateTime(2026, 10, 22, 0, 0, 0, DateTimeKind.Utc),
            UpdatedUtc = new DateTime(2026, 10, 22, 0, 1, 0, DateTimeKind.Utc)
        });

        JsonNode actual = JsonSerializer.SerializeToNode(operation, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        Assert.True(JsonNode.DeepEquals(fixture["operation"], actual));
    }
}
