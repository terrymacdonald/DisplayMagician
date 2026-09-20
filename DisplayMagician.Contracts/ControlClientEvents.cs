using System;

namespace DisplayMagician.Contracts;

public enum ControlClientEventType
{
    Unknown = 0,
    ClientSyncCompleted = 1,
    OperationStatusUpdated = 2
}

/// <summary>An event published by Control Service to one authenticated local UI session.</summary>
public sealed class ControlClientEvent
{
    public ControlClientEventType EventType { get; set; }
    public DateTime PublishedUtc { get; set; }
    public ClientSyncResult? ClientSync { get; set; }
    public OperationStatus? OperationStatus { get; set; }
}