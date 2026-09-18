using System;
using System.IO;
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
        try
        {
            _storagePaths.EnsureMachineDirectories();
            _logger.Info("ControlServiceWorker/ExecuteAsync: Machine storage is ready at {0}.", _storagePaths.MachinePath);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
        {
            // The installer provisions ProgramData ACLs. Keep the local coordinator available for development diagnostics if they are absent.
            _logger.Error(ex, "ControlServiceWorker/ExecuteAsync: Machine storage at {0} is unavailable. Persistent operations, including migration, will fail until installer permissions are repaired.", _storagePaths.MachinePath);
        }
        await _pipeServer.RunAsync(stoppingToken).ConfigureAwait(false);
        _logger.Info("ControlServiceWorker/ExecuteAsync: Control Service pipe listener has stopped.");
    }
}
