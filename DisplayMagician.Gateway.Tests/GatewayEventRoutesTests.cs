using System;
using System.Text.Json;
using DisplayMagician.Contracts;
using DisplayMagician.Gateway;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class GatewayEventRoutesTests
{
    [Fact]
    public void StreamLimitsOneDeviceFourPerUserAndReleasesLeases()
    {
        GatewayEventStreamLimiter limiter = new GatewayEventStreamLimiter();
        IDisposable?[] leases = new IDisposable?[4];
        for (int index = 0; index < leases.Length; index++)
        {
            leases[index] = limiter.TryEnter(new GatewayAuthenticationResult { OwnerUserSid = "owner", DeviceId = "phone-" + index });
            Assert.NotNull(leases[index]);
        }

        Assert.Null(limiter.TryEnter(new GatewayAuthenticationResult { OwnerUserSid = "owner", DeviceId = "phone-0" }));
        Assert.Null(limiter.TryEnter(new GatewayAuthenticationResult { OwnerUserSid = "owner", DeviceId = "phone-4" }));
        using IDisposable otherUser = Assert.IsAssignableFrom<IDisposable>(limiter.TryEnter(new GatewayAuthenticationResult { OwnerUserSid = "other", DeviceId = "phone-0" }));
        leases[0]!.Dispose();
        leases[0]!.Dispose();
        using IDisposable replacement = Assert.IsAssignableFrom<IDisposable>(limiter.TryEnter(new GatewayAuthenticationResult { OwnerUserSid = "owner", DeviceId = "phone-4" }));
        for (int index = 1; index < leases.Length; index++) leases[index]!.Dispose();
    }

    [Fact]
    public void SavedResourceFrameUsesTypedNameAndRestShape()
    {
        GatewayEventRecord record = new GatewayEventRecord
        {
            Id = "v1.3.example",
            EventType = ControlClientEventType.ShortcutChanged,
            ResourceChange = new GatewaySavedResourceChange { ResourceType = "shortcut", Id = "one", Action = "deleted" }
        };

        Assert.True(GatewayEventRoutes.TryCreateEvent(record, out string name, out object? data));
        string frame = GatewayEventRoutes.FormatEvent(record.Id, name, data!);

        Assert.StartsWith("id: v1.3.example\nevent: shortcut.changed\ndata: ", frame);
        Assert.EndsWith("\n\n", frame);
        string json = frame.Substring(frame.IndexOf("data: ", StringComparison.Ordinal) + 6).Trim();
        using JsonDocument payload = JsonDocument.Parse(json);
        Assert.Equal("shortcut", payload.RootElement.GetProperty("resourceType").GetString());
        Assert.Equal("deleted", payload.RootElement.GetProperty("action").GetString());
        Assert.Equal(JsonValueKind.Null, payload.RootElement.GetProperty("href").ValueKind);
    }

    [Fact]
    public void OperationFrameUsesRestOperationStatus()
    {
        GatewayEventRecord record = new GatewayEventRecord
        {
            Id = "v1.4.example",
            EventType = ControlClientEventType.OperationStatusUpdated,
            OperationStatus = new OperationStatus { OperationId = Guid.NewGuid(), IsTerminal = true, IsSuccessful = true }
        };

        Assert.True(GatewayEventRoutes.TryCreateEvent(record, out string name, out object? data));
        string frame = GatewayEventRoutes.FormatEvent(record.Id, name, data!);
        using JsonDocument payload = JsonDocument.Parse(frame.Substring(frame.IndexOf("data: ", StringComparison.Ordinal) + 6).Trim());

        Assert.Equal("operation.updated", name);
        Assert.Equal("succeeded", payload.RootElement.GetProperty("operation").GetProperty("status").GetString());
        Assert.True(payload.RootElement.GetProperty("operation").GetProperty("isFinished").GetBoolean());
    }
}
