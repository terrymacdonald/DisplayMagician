using System;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagician.Engine;

public interface IDisplayOperationExecutor
{
    Task<DisplayOperationResult> ExecuteAsync(DisplayOperationRequest request, CancellationToken cancellationToken);
}

public sealed class DisplayOperationRequest
{
    public Guid OperationId { get; set; } = Guid.NewGuid();

    public DisplayOperationType OperationType { get; set; }

    public string UserSid { get; set; } = string.Empty;

    public int SessionId { get; set; }

    public string TargetId { get; set; } = string.Empty;
}

public sealed class DisplayOperationResult
{
    public bool IsSuccessful { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool RequiresRecovery { get; set; }
}
