using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.Extensions.Hosting;
using NLog;

namespace DisplayMagician.ControlService;

public sealed class ControlServiceWorker : BackgroundService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly NamedPipeControlServer _pipeServer;
    private readonly ControlClientPipeServer _clientPipeServer;
    private readonly StoragePaths _storagePaths;
    private readonly MachineScheduleCoordinator _machineScheduleCoordinator;
    private readonly ClientSyncCoordinator _clientSyncCoordinator;

    public ControlServiceWorker(NamedPipeControlServer pipeServer, ControlClientPipeServer clientPipeServer, StoragePaths storagePaths, MachineScheduleCoordinator machineScheduleCoordinator, ClientSyncCoordinator clientSyncCoordinator)
    {
        _pipeServer = pipeServer;
        _clientPipeServer = clientPipeServer;
        _storagePaths = storagePaths;
        _machineScheduleCoordinator = machineScheduleCoordinator;
        _clientSyncCoordinator = clientSyncCoordinator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Info("ControlServiceWorker/ExecuteAsync: Control Service pipe listener is starting.");
        try
        {
            _storagePaths.EnsureMachineDirectories();
            _machineScheduleCoordinator.EnsureInitialized();
            _logger.Info("ControlServiceWorker/ExecuteAsync: Machine storage is ready at {0}.", _storagePaths.MachinePath);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
        {
            // The installer provisions ProgramData ACLs. Keep the local coordinator available for development diagnostics if they are absent.
            _logger.Error(ex, "ControlServiceWorker/ExecuteAsync: Machine storage at {0} is unavailable. Persistent operations, including migration, will fail until installer permissions are repaired.", _storagePaths.MachinePath);
        }
        Task agentServer = _pipeServer.RunAsync(stoppingToken);
        Task clientServer = _clientPipeServer.RunAsync(stoppingToken);
        Task clientSync = RunClientSyncAsync(stoppingToken);
        await Task.WhenAll(agentServer, clientServer, clientSync).ConfigureAwait(false);
        _logger.Info("ControlServiceWorker/ExecuteAsync: Control Service pipe listener has stopped.");
    }

    private async Task RunClientSyncAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _clientSyncCoordinator.SyncAsync(new ClientSyncRequest(), null, null, stoppingToken).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).ConfigureAwait(false);
        }
    }
}
