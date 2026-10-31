using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using NLog;

namespace DisplayMagician.ControlService;

public sealed class ControlServiceWorker : BackgroundService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly NamedPipeControlServer _pipeServer;
    private readonly MachineDiagnosticLogLevelStore _loggingSettings;
    private readonly ControlClientPipeServer _clientPipeServer;
    private readonly ControlClientEventPipeServer _clientEventPipeServer;
    private readonly GatewayPairingPipeServer _gatewayPairingPipeServer;
    private readonly StoragePaths _storagePaths;
    private readonly MachineScheduleCoordinator _machineScheduleCoordinator;
    private readonly ClientSyncCoordinator _clientSyncCoordinator;
    private readonly AnonymousMetricsSender _anonymousMetricsSender;
    private readonly ControlClientEventHub _eventHub;
    private readonly OperationStatusStore _operationStatusStore;
    private readonly OperationDecisionStore _operationDecisionStore;
    private readonly GatewayEventJournal _gatewayEventJournal;
    private readonly ControlStateCoordinator _controlStateCoordinator;
    private readonly GatewayFirewallManager _gatewayFirewallManager;

    public ControlServiceWorker(NamedPipeControlServer pipeServer, ControlClientPipeServer clientPipeServer, ControlClientEventPipeServer clientEventPipeServer, GatewayPairingPipeServer gatewayPairingPipeServer, StoragePaths storagePaths, MachineScheduleCoordinator machineScheduleCoordinator, ClientSyncCoordinator clientSyncCoordinator, AnonymousMetricsSender anonymousMetricsSender, ControlClientEventHub eventHub, OperationStatusStore operationStatusStore, OperationDecisionStore operationDecisionStore, GatewayEventJournal gatewayEventJournal, ControlStateCoordinator controlStateCoordinator, GatewayFirewallManager gatewayFirewallManager, MachineDiagnosticLogLevelStore loggingSettings)
    {
        _pipeServer = pipeServer;
        _loggingSettings = loggingSettings;
        _clientPipeServer = clientPipeServer;
        _clientEventPipeServer = clientEventPipeServer;
        _gatewayPairingPipeServer = gatewayPairingPipeServer;
        _storagePaths = storagePaths;
        _machineScheduleCoordinator = machineScheduleCoordinator;
        _clientSyncCoordinator = clientSyncCoordinator;
        _anonymousMetricsSender = anonymousMetricsSender;
        _eventHub = eventHub;
        _operationStatusStore = operationStatusStore;
        _operationDecisionStore = operationDecisionStore;
        _gatewayEventJournal = gatewayEventJournal;
        _controlStateCoordinator = controlStateCoordinator;
        _gatewayFirewallManager = gatewayFirewallManager;
        try { _gatewayEventJournal.Reconcile(_operationStatusStore.GetReplayState(), _operationDecisionStore.GetReplayState(), DateTime.UtcNow); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.Error(ex, "ControlServiceWorker/ControlServiceWorker: Gateway replay reconciliation is unavailable; local operation routing remains available.");
        }
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
        bool firewallReady = false;
        try
        {
            _gatewayFirewallManager.Reconcile();
            firewallReady = true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "ControlServiceWorker/ExecuteAsync: Could not reconcile the installed Gateway firewall rules with saved settings.");
        }
        using CancellationTokenSource lifetime = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        Task[] required =
        {
            _pipeServer.RunAsync(lifetime.Token), _clientPipeServer.RunAsync(lifetime.Token),
            _clientEventPipeServer.RunAsync(lifetime.Token), _gatewayPairingPipeServer.RunAsync(lifetime.Token),
            RunClientSyncAsync(lifetime.Token), RunLoggingAsync(lifetime.Token)
        };
        Task firewallRetry = firewallReady ? Task.CompletedTask : RetryGatewayFirewallAsync(lifetime.Token);
        try { await SuperviseAsync(required, new[] { firewallRetry }, lifetime).ConfigureAwait(false); }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            _logger.Error(ex, "ControlServiceWorker/ExecuteAsync: A required service loop stopped; stopping the service and its remaining listeners.");
            Environment.ExitCode = 1;
            // The loops have been cancelled and observed. StopHost alone reports a clean
            // service stop, which does not trigger Windows' configured failure recovery.
            LogManager.Flush(TimeSpan.FromSeconds(3));
            if (WindowsServiceHelpers.IsWindowsService()) Environment.Exit(1);
            throw;
        }
        _logger.Info("ControlServiceWorker/ExecuteAsync: Control Service pipe listener has stopped.");
    }

    internal static async Task SuperviseAsync(Task[] required, Task[] background, CancellationTokenSource lifetime)
    {
        await Task.WhenAny(required).ConfigureAwait(false);
        bool stopping = lifetime.IsCancellationRequested;
        lifetime.Cancel();
        try { await Task.WhenAll(required.Concat(background)).ConfigureAwait(false); }
        catch (OperationCanceledException) when (stopping) { return; }
        catch (OperationCanceledException ex) { throw new InvalidOperationException("A required service loop stopped unexpectedly.", ex); }
        if (!stopping) throw new InvalidOperationException("A required service loop stopped unexpectedly.");
    }

    private async Task RunLoggingAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string level = _loggingSettings.GetActiveLevel(DateTime.UtcNow);
                Program.ApplyDiagnosticLogLevel(level);
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }
    private async Task RetryGatewayFirewallAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).ConfigureAwait(false);
                _gatewayFirewallManager.Reconcile();
                _logger.Info("ControlServiceWorker/RetryGatewayFirewallAsync: Reconciled the installed Gateway firewall rules after a startup failure.");
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "ControlServiceWorker/RetryGatewayFirewallAsync: Gateway firewall rules are still unavailable; retrying in one minute.");
            }
        }
    }

    private async Task RunClientSyncAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _clientSyncCoordinator.SyncAsync(new ClientSyncRequest(), null, null, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ControlServiceWorker/RunClientSyncAsync: Client sync failed. The Control Service pipe listeners will continue running.");
            }

            try
            {
                await _anonymousMetricsSender.TrySendAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ControlServiceWorker/RunClientSyncAsync: Anonymous metrics delivery failed. The Control Service pipe listeners will continue running.");
            }

            try
            {
                _operationDecisionStore.Expire(DateTime.UtcNow);
                _operationStatusStore.RefreshAuthority(_controlStateCoordinator.GetStatus(DateTime.UtcNow).Agents);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ControlServiceWorker/RunClientSyncAsync: Operation maintenance failed. The Control Service pipe listeners will continue running.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private void PublishOperationStatus(OperationStatus status)
    {
        try
        {
            _gatewayEventJournal.Append(status, DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.Error(ex, "ControlServiceWorker/PublishOperationStatus: Could not retain Gateway event for operation {0}.", status.OperationId);
            ResetReplay(status.OwnerUserSid);
        }
        _eventHub.Publish(status.OwnerUserSid, status.OwnerSessionId, new ControlClientEvent { EventType = ControlClientEventType.OperationStatusUpdated, PublishedUtc = DateTime.UtcNow, OperationStatus = status });
    }

    private void PublishOperationDecision(OperationDecision decision)
    {
        try
        {
            _gatewayEventJournal.Append(decision, DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.Error(ex, "ControlServiceWorker/PublishOperationDecision: Could not retain Gateway event for decision {0}.", decision.PromptId);
            ResetReplay(decision.OwnerUserSid);
        }
        _eventHub.Publish(decision.OwnerUserSid, decision.OwnerSessionId, new ControlClientEvent { EventType = ControlClientEventType.OperationDecisionUpdated, Scope = ControlClientEventScope.User, PublishedUtc = DateTime.UtcNow, OperationDecision = decision });
    }

    private void ResetReplay(string ownerUserSid)
    {
        try { _gatewayEventJournal.MarkContinuityLost(ownerUserSid); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // In-memory reads remain blocked until recovery succeeds; startup also reconciles persisted operation state.
            _logger.Error(ex, "ControlServiceWorker/ResetReplay: Could not persist replay recovery for user {0}.", ownerUserSid);
        }
    }
}
