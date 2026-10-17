using System;

namespace DisplayMagician.Contracts;

/// <summary>A retained, user-scoped event for Gateway replay. REST formats its payload separately.</summary>
public sealed class GatewayEventRecord
{
    public string Id { get; set; } = string.Empty;
    public string OwnerUserSid { get; set; } = string.Empty;
    public DateTime PublishedUtc { get; set; }
    public ControlClientEventType EventType { get; set; }
    public OperationStatus? OperationStatus { get; set; }
    public OperationDecision? OperationDecision { get; set; }
}

public enum GatewayEventReplayState
{
    Available = 0,
    CursorUnknown = 1,
    CursorExpired = 2
}

public sealed class GatewayEventReplay
{
    public GatewayEventReplayState State { get; set; }
    public string HeadId { get; set; } = string.Empty;
    public string NextCursor { get; set; } = string.Empty;
    public bool HasMore { get; set; }
    public GatewayEventRecord[] Events { get; set; } = Array.Empty<GatewayEventRecord>();
}

public sealed class GatewayEventReadRequest
{
    public GatewayAuthenticationResult Authentication { get; set; } = new GatewayAuthenticationResult();
    public string? Cursor { get; set; }
}
