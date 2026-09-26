using System;

namespace DisplayMagician.Contracts;

public enum ControlClientEventType
{
    Unknown = 0,
    ClientSyncCompleted = 1,
    OperationStatusUpdated = 2,
    OperationDecisionUpdated = 3,
    SubscriptionHeartbeat = 4
}

/// <summary>Defines whether a local event is delivered to one Windows session or every subscribed session for its user.</summary>
public enum ControlClientEventScope
{
    Session = 0,
    User = 1
}

/// <summary>An event published by Control Service to one authenticated local UI session or, when explicitly scoped, every subscribed session for its user.</summary>
public sealed class ControlClientEvent
{
    public ControlClientEventType EventType { get; set; }
    public ControlClientEventScope Scope { get; set; } = ControlClientEventScope.Session;
    public DateTime PublishedUtc { get; set; }
    public ClientSyncResult? ClientSync { get; set; }
    public OperationStatus? OperationStatus { get; set; }
    public OperationDecision? OperationDecision { get; set; }
}
