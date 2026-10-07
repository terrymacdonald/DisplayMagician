using System;
using System.Threading;
using DisplayMagician.Contracts;

namespace DisplayMagician;

/// <summary>
/// Adapts the asynchronous Control Service pipe protocol for the desktop shortcut repository.
/// </summary>
internal sealed class UserAgentRepositoryConnection : IUserAgentRepositoryConnection
{
    private readonly ControlServicePipeClient _controlServiceClient;

    public UserAgentRepositoryConnection(ControlServicePipeClient controlServiceClient)
    {
        _controlServiceClient = controlServiceClient ?? throw new ArgumentNullException(nameof(controlServiceClient));
    }

    public RepositorySnapshot GetRepositorySnapshot(RepositoryKind repository)
    {
        return _controlServiceClient.GetRepositorySnapshotAsync(repository, CancellationToken.None).GetAwaiter().GetResult();
    }

    public RepositoryCommitResult CommitRepositorySnapshot(RepositoryCommitRequest request)
    {
        return _controlServiceClient.CommitRepositorySnapshotAsync(request, CancellationToken.None).GetAwaiter().GetResult();
    }
}
