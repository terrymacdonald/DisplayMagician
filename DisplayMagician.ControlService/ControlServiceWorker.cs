using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NLog;

namespace DisplayMagician.ControlService;

public sealed class ControlServiceWorker : BackgroundService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly NamedPipeControlServer _pipeServer;
    private readonly StoragePaths _storagePaths;

    public ControlServiceWorker(NamedPipeControlServer pipeServer, StoragePaths storagePaths)
    {
        _pipeServer = pipeServer;
        _storagePaths = storagePaths;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Info("ControlServiceWorker/ExecuteAsync: Control Service pipe listener is starting.");
        _storagePaths.EnsureMachineDirectories();
        _logger.Info("ControlServiceWorker/ExecuteAsync: Machine storage is ready at {0}.", _storagePaths.MachinePath);
        await _pipeServer.RunAsync(stoppingToken).ConfigureAwait(false);
        _logger.Info("ControlServiceWorker/ExecuteAsync: Control Service pipe listener has stopped.");
    }
}
