using System;
using System.Diagnostics.CodeAnalysis;

namespace DisplayMagician.Contracts;

/// <summary>Structural validation shared by status delivery and its persisted snapshots.</summary>
public static class OperationStatusValidation
{
    public static bool IsValidUpdate([NotNullWhen(true)] OperationStatusUpdate? update, bool requireUpdateId = false)
    {
        return update != null && update.OperationId != Guid.Empty && (!requireUpdateId || update.UpdateId != Guid.Empty) &&
            Enum.IsDefined(update.OperationType) && Enum.IsDefined(update.Phase) && Enum.IsDefined(update.ErrorCode);
    }

    public static bool IsValidStatus(OperationStatus? status)
    {
        return status != null && status.OperationId != Guid.Empty && !string.IsNullOrWhiteSpace(status.OwnerUserSid) &&
            status.OwnerSessionId >= 0 && status.Sequence > 0 && status.Sequence < long.MaxValue &&
            status.StartedUtc != default && status.UpdatedUtc != default &&
            Enum.IsDefined(status.OperationType) && Enum.IsDefined(status.Phase) && Enum.IsDefined(status.ErrorCode);
    }
}
