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
    private readonly ControlClientEventPipeServer _clientEventPipeServer;
    private readonly StoragePaths _storagePaths;
    private readonly MachineScheduleCoordinator _machineScheduleCoordinator;
    private readonly ClientSyncCoordinator _clientSyncCoordinator;
    private readonly AnonymousMetricsSender _anonymousMetricsSender;
    private readonly ControlClientEventHub _eventHub;
    private readonly OperationStatusStore _operationStatusStore;
    private readonly OperationDecisionStore _operationDecisionStore;
    private readonly ControlStateCoordinator _controlStateCoordinator;

    public ControlServiceWorker(NamedPipeControlServer pipeServer, ControlClientPipeServer clientPipeServer, ControlClientEventPipeServer clientEventPipeServer, StoragePaths storagePaths, MachineScheduleCoordinator machineScheduleCoordinator, ClientSyncCoordinator clientSyncCoordinator, AnonymousMetricsSender anonymousMetricsSender, ControlClientEventHub eventHub, OperationStatusStore operationStatusStore, OperationDecisionStore operationDecisionStore, ControlStateCoordinator controlStateCoordinator)
    {
        _pipeServer = pipeServer;
        _clientPipeServer = clientPipeServer;
        _clientEventPipeServer = clientEventPipeServer;
        _storagePaths = storagePaths;
        _machineScheduleCoordinator = machineScheduleCoordinator;
        _clientSyncCoordinator = clientSyncCoordinator;
        _anonymousMetricsSender = anonymousMetricsSender;
        _eventHub = eventHub;
        _operationStatusStore = operationStatusStore;
        _operationDecisionStore = operationDecisionStore;
        _controlStateCoordinator = controlStateCoordinator;
        _operationStatusStore.StatusUpdated += PublishOperationStatus;
        _operationDecisionStore.DecisionUpdated += PublishOperationDecision;
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
        Task clientEventServer = _clientEventPipeServer.RunAsync(stoppingToken);
        Task clientSync = RunClientSyncAsync(stoppingToken);
        await Task.WhenAll(agentServer, clientServer, clientEventServer, clientSync).ConfigureAwait(false);
        _logger.Info("ControlServiceWorker/ExecuteAsync: Control Service pipe listener has stopped.");
    }

    private async Task RunClientSyncAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _clientSyncCoordinator.SyncAsync(new ClientSyncRequest(), null, null, stoppingToken).ConfigureAwait(false);
            await _anonymousMetricsSender.TrySendAsync(stoppingToken).ConfigureAwait(false);
            _operationDecisionStore.Expire(DateTime.UtcNow);
            _operationStatusStore.RefreshAuthority(_controlStateCoordinator.GetStatus(DateTime.UtcNow).Agents);
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken).ConfigureAwait(false);
        }
    }

    private void PublishOperationStatus(OperationStatus status)
    {
        _eventHub.Publish(status.OwnerUserSid, status.OwnerSessionId, new ControlClientEvent { EventType = ControlClientEventType.OperationStatusUpdated, PublishedUtc = DateTime.UtcNow, OperationStatus = status });
    }

    private void PublishOperationDecision(OperationDecision decision)
    {
        _eventHub.Publish(decision.OwnerUserSid, decision.OwnerSessionId, new ControlClientEvent { EventType = ControlClientEventType.OperationDecisionUpdated, PublishedUtc = DateTime.UtcNow, OperationDecision = decision });
    }
}
