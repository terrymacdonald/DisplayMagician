using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DisplayMagician.Contracts;

/// <summary>Structural validation shared by status delivery and its persisted snapshots.</summary>
public static class OperationStatusValidation
{
    public static bool IsValidUpdate([NotNullWhen(true)] OperationStatusUpdate? update, bool requireUpdateId = false)
    {
        return update != null && update.OperationId != Guid.Empty && (!requireUpdateId || update.UpdateId != Guid.Empty) &&
            Enum.IsDefined(update.OperationType) && Enum.IsDefined(update.Phase) && Enum.IsDefined(update.ErrorCode) && ValidDisplayResults(update.DisplayResults);
    }

    private static bool ValidDisplayResults(DisplayConfigurationResult[]? results) => results != null && results.All(result =>
        result != null && result.Attempt > 0 && result.Issues != null && result.Issues.All(issue =>
            issue != null && Enum.IsDefined(issue.Severity) && !string.IsNullOrWhiteSpace(issue.Stage) && !string.IsNullOrWhiteSpace(issue.Component)));

    public static bool IsValidStatus(OperationStatus? status)
    {
        return status != null && status.OperationId != Guid.Empty && !string.IsNullOrWhiteSpace(status.OwnerUserSid) &&
            status.OwnerSessionId >= 0 && status.Sequence > 0 && status.Sequence < long.MaxValue &&
            status.StartedUtc != default && status.UpdatedUtc != default &&
            Enum.IsDefined(status.OperationType) && Enum.IsDefined(status.Phase) && Enum.IsDefined(status.ErrorCode) && ValidDisplayResults(status.DisplayResults);
    }
}
