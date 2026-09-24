using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using NLog;

namespace DisplayMagician.ControlService;

public sealed class ControlClientPipeServer
{
    private const int MaximumConnectedClients = 16;
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly ProfileOperationRouter _profileOperationRouter;
    private readonly OperationStatusStore _operationStatusStore;
    private readonly ClientSyncCoordinator _clientSyncCoordinator;
    private readonly MachineScheduleCoordinator _machineScheduleCoordinator;
    private readonly ControlStateCoordinator _stateCoordinator;
    private readonly AuditStore _auditStore;
    private readonly RecoveryAdministrationStore _recoveryAdministrationStore;
    private readonly OperationDecisionStore _operationDecisionStore;
    private readonly StoragePaths _storagePaths;
    private readonly ControlRequestReplayStore _requestReplayStore;
    private readonly SemaphoreSlim _connectedClientSlots = new SemaphoreSlim(MaximumConnectedClients, MaximumConnectedClients);
    private readonly SemaphoreSlim _replayableMutationLock = new SemaphoreSlim(1, 1);

    public ControlClientPipeServer(ProfileOperationRouter profileOperationRouter, OperationStatusStore operationStatusStore, ClientSyncCoordinator clientSyncCoordinator, MachineScheduleCoordinator machineScheduleCoordinator, ControlStateCoordinator stateCoordinator, AuditStore auditStore, RecoveryAdministrationStore recoveryAdministrationStore, OperationDecisionStore operationDecisionStore, StoragePaths storagePaths, ControlRequestReplayStore requestReplayStore)
    {
        _profileOperationRouter = profileOperationRouter ?? throw new ArgumentNullException(nameof(profileOperationRouter));
        _operationStatusStore = operationStatusStore ?? throw new ArgumentNullException(nameof(operationStatusStore));
        _clientSyncCoordinator = clientSyncCoordinator ?? throw new ArgumentNullException(nameof(clientSyncCoordinator));
        _machineScheduleCoordinator = machineScheduleCoordinator ?? throw new ArgumentNullException(nameof(machineScheduleCoordinator));
        _stateCoordinator = stateCoordinator ?? throw new ArgumentNullException(nameof(stateCoordinator));
        _auditStore = auditStore ?? throw new ArgumentNullException(nameof(auditStore));
        _recoveryAdministrationStore = recoveryAdministrationStore ?? throw new ArgumentNullException(nameof(recoveryAdministrationStore));
        _operationDecisionStore = operationDecisionStore ?? throw new ArgumentNullException(nameof(operationDecisionStore));
        _storagePaths = storagePaths ?? throw new ArgumentNullException(nameof(storagePaths));
        _requestReplayStore = requestReplayStore ?? throw new ArgumentNullException(nameof(requestReplayStore));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            NamedPipeServerStream? pipe = null;
            try
            {
                pipe = CreatePipe();
                await pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                if (!await _connectedClientSlots.WaitAsync(0, cancellationToken).ConfigureAwait(false))
                {
                    _logger.Warn("ControlClientPipeServer/RunAsync: Rejected a client request because the connected-client limit of {0} was reached.", MaximumConnectedClients);
                    pipe.Dispose();
                    continue;
                }

                _ = HandleClientWithSlotAsync(pipe, cancellationToken);
                pipe = null;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                pipe?.Dispose();
                return;
            }
            catch (Exception ex)
            {
                pipe?.Dispose();
                _logger.Error(ex, "ControlClientPipeServer/RunAsync: Unable to accept a client request.");
            }
        }
    }

    private async Task HandleClientWithSlotAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        try
        {
            await HandleClientAsync(pipe, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "ControlClientPipeServer/HandleClientWithSlotAsync: Client request processing failed unexpectedly.");
        }
        finally
        {
            _connectedClientSlots.Release();
        }
    }

    private static NamedPipeServerStream CreatePipe()
    {
        PipeSecurity security = new PipeSecurity();
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
        return NamedPipeServerStreamAcl.Create(ControlProtocol.ClientPipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, security, HandleInheritability.None);
    }

    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        using (pipe)
        {
            try
            {
                ControlEnvelope? request = await ReadEnvelopeWithTimeoutAsync(pipe, RequestTimeout, cancellationToken).ConfigureAwait(false);
                if (request == null)
                {
                    return;
                }

                using IDisposable requestScope = SupportLogScope.BeginRequest(request.RequestId);
                ControlResponse response;
                if (request.ProtocolVersion != ControlProtocol.CurrentVersion)
                {
                    response = new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.UnsupportedProtocolVersion, Message = "The client uses an unsupported protocol version." };
                }
                else if (!ControlProtocol.TryCreateWelcome(request.Hello, "ControlService", ControlProtocol.ControlServiceCapabilities, out ProtocolWelcome? welcome, out ControlErrorCode negotiationError, out string negotiationMessage))
                {
                    response = new ControlResponse { IsSuccessful = false, ErrorCode = negotiationError, Message = negotiationMessage };
                }
                else
                {
                    PipeClientIdentity identity = GetClientIdentity(pipe);
                    bool isReplayableMutation = IsReplayableMutation(request.MessageType);
                    if (isReplayableMutation)
                    {
                        await _replayableMutationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                    }

                    try
                    {
                        if (isReplayableMutation && _requestReplayStore.TryGet(identity.UserSid, request, DateTime.UtcNow, out ControlResponse replayedResponse))
                        {
                            response = replayedResponse;
                        }
                        else
                        {
                            try
                            {
                                response = request.MessageType switch
                                {
                                    ControlMessageType.ListProfiles => await _profileOperationRouter.ListProfilesAsync(identity.UserSid, identity.SessionId, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.ListGames => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.ListApps => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.ListShortcuts => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.ListMessages or ControlMessageType.SetMessageReadState => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.SyncMessages => await SyncClientAsync(identity, new ClientSyncRequest { IsManual = true }, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.SyncClient => await SyncClientAsync(identity, JsonSerializer.Deserialize<ClientSyncRequest>(request.Payload) ?? new ClientSyncRequest(), cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.GetAnonymousMetricsSettings => GetAnonymousMetricsSettings(),
                                    ControlMessageType.UpdateAnonymousMetricsSettings => UpdateAnonymousMetricsSettings(request),
                                    ControlMessageType.InitializeAnonymousMetrics => InitializeAnonymousMetrics(request),
                                    ControlMessageType.ReportAnonymousMetricsUsage => ReportAnonymousMetricsUsage(request),
                                    ControlMessageType.ApplyProfile => await ApplyProfileAsync(identity, request, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.StartShortcut => await StartShortcutAsync(identity, request, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.CancelOperation => await CancelOperationAsync(identity, request, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.StopAgentIfIdle => await _profileOperationRouter.StopAgentIfIdleAsync(identity.UserSid, identity.SessionId, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.RestartUserAgent => await _profileOperationRouter.RestartUserAgentAsync(identity.UserSid, identity.SessionId, request.RequestId, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.CreateProfileFromCurrent or ControlMessageType.RenameProfile or ControlMessageType.DeleteProfile or ControlMessageType.UpdateProfileFromCurrent or ControlMessageType.UpdateDisplayProfileSettings => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.ListAudioProfiles or ControlMessageType.ApplyAudioProfile or ControlMessageType.CreateAudioProfileFromCurrent or ControlMessageType.RenameAudioProfile or ControlMessageType.DeleteAudioProfile or ControlMessageType.UpdateAudioProfileFromCurrent => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.GetRepositorySnapshot => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.CommitRepositorySnapshot => await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.CreateUserSupportBundle => await CreateUserSupportBundleAsync(identity, request, cancellationToken).ConfigureAwait(false),
                                    ControlMessageType.ResolveOperationDecision => ResolveOperationDecision(identity, request),
                                    ControlMessageType.ListOperationDecisions => ListOperationDecisions(identity),
                                    ControlMessageType.GetOperationStatus => GetOperationStatus(identity, request),
                                    ControlMessageType.ListOperationStatuses => ListOperationStatuses(identity),
                                    ControlMessageType.GetServiceStatus => GetServiceStatus(identity),
                                    ControlMessageType.ForceReleaseDisplayControl => ForceReleaseDisplayControl(identity, request),
                                    ControlMessageType.RecordRecoveryAdministration => RecordRecoveryAdministration(identity, request),
                                    _ => new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The requested client operation is not supported." }
                                };
                            }
                            catch (JsonException ex)
                            {
                                _logger.Warn(ex, "ControlClientPipeServer/HandleClientAsync: The client sent an invalid JSON request payload.");
                                response = new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The client request payload was invalid." };
                            }

                            if (isReplayableMutation && response.IsSuccessful)
                            {
                                _requestReplayStore.Store(identity.UserSid, request, response, DateTime.UtcNow);
                            }
                        }
                    }
                    finally
                    {
                        if (isReplayableMutation)
                        {
                            _replayableMutationLock.Release();
                        }
                    }

                    response.ProtocolWelcome = welcome;
                }

                await ControlEnvelopeSerializer.WriteAsync(pipe, new ControlEnvelope { MessageType = request.MessageType, RequestId = request.RequestId, Payload = JsonSerializer.Serialize(response) }, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException || ex is JsonException || ex is EndOfStreamException || ex is TimeoutException)
            {
                _logger.Debug(ex, "ControlClientPipeServer/HandleClientAsync: Client request ended or was invalid.");
            }
        }
    }

    private static async Task<ControlEnvelope?> ReadEnvelopeWithTimeoutAsync(NamedPipeServerStream pipe, TimeSpan timeout, CancellationToken serviceCancellationToken)
    {
        using CancellationTokenSource timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(serviceCancellationToken);
        timeoutCancellationTokenSource.CancelAfter(timeout);
        try
        {
            return await ControlEnvelopeSerializer.ReadAsync(pipe, timeoutCancellationTokenSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!serviceCancellationToken.IsCancellationRequested && timeoutCancellationTokenSource.IsCancellationRequested)
        {
            throw new TimeoutException($"The client did not send a complete request within {timeout.TotalSeconds:0} seconds.", ex);
        }
    }

    private static bool IsReplayableMutation(ControlMessageType messageType)
    {
        return messageType is ControlMessageType.ApplyProfile or ControlMessageType.StartShortcut or ControlMessageType.CancelOperation or
            ControlMessageType.CreateProfileFromCurrent or ControlMessageType.RenameProfile or ControlMessageType.DeleteProfile or ControlMessageType.UpdateProfileFromCurrent or ControlMessageType.UpdateDisplayProfileSettings or
            ControlMessageType.ApplyAudioProfile or ControlMessageType.CreateAudioProfileFromCurrent or ControlMessageType.RenameAudioProfile or ControlMessageType.DeleteAudioProfile or ControlMessageType.UpdateAudioProfileFromCurrent or
            ControlMessageType.CommitRepositorySnapshot or ControlMessageType.SetMessageReadState or ControlMessageType.ResolveOperationDecision or ControlMessageType.UpdateAnonymousMetricsSettings or
            ControlMessageType.InitializeAnonymousMetrics or ControlMessageType.ForceReleaseDisplayControl or ControlMessageType.RecordRecoveryAdministration or ControlMessageType.RestartUserAgent or
            ControlMessageType.CreateUserSupportBundle;
    }

    private Task<ControlResponse> ApplyProfileAsync(PipeClientIdentity identity, ControlEnvelope request, CancellationToken cancellationToken)
    {
        ApplyProfileRequest? applyRequest = JsonSerializer.Deserialize<ApplyProfileRequest>(request.Payload);
        return applyRequest == null
            ? Task.FromResult(new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The display profile request was invalid." })
            : _profileOperationRouter.ApplyProfileAsync(identity.UserSid, identity.SessionId, applyRequest.ProfileId, applyRequest.OperationId, request.RequestId, cancellationToken);
    }

    private async Task<ControlResponse> StartShortcutAsync(PipeClientIdentity identity, ControlEnvelope request, CancellationToken cancellationToken)
    {
        StartShortcutRequest? startRequest = JsonSerializer.Deserialize<StartShortcutRequest>(request.Payload);
        if (startRequest == null)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The shortcut request was invalid." };
        }

        Guid operationId = startRequest.OperationId == Guid.Empty ? Guid.NewGuid() : startRequest.OperationId;
        using IDisposable operationScope = SupportLogScope.BeginOperation(operationId);
        return await _profileOperationRouter.StartShortcutAsync(identity.UserSid, identity.SessionId, startRequest.ShortcutId, operationId, request.RequestId, cancellationToken).ConfigureAwait(false);
    }

    private Task<ControlResponse> CancelOperationAsync(PipeClientIdentity identity, ControlEnvelope request, CancellationToken cancellationToken)
    {
        CancelOperationRequest? cancelRequest = JsonSerializer.Deserialize<CancelOperationRequest>(request.Payload);
        return cancelRequest == null
            ? Task.FromResult(new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The cancellation request was invalid." })
            : _profileOperationRouter.CancelOperationAsync(identity.UserSid, identity.SessionId, cancelRequest.OperationId, cancellationToken);
    }

    private async Task<ControlResponse> SyncClientAsync(PipeClientIdentity identity, ClientSyncRequest request, CancellationToken cancellationToken)
    {
        ClientSyncResult result = await _clientSyncCoordinator.SyncAsync(request, identity.UserSid, identity.SessionId, cancellationToken).ConfigureAwait(false);
        return new ControlResponse
        {
            IsSuccessful = result.WasDue || !request.IsManual,
            ErrorCode = result.WasDue || !request.IsManual ? ControlErrorCode.None : ControlErrorCode.InvalidRequest,
            Message = result.WasDue ? "Combined client sync completed." : "Combined client sync was not due.",
            ClientSync = result,
            MessageSync = result.MessageSync
        };
    }

    private ControlResponse GetAnonymousMetricsSettings()
    {
        MachineScheduleState state = _machineScheduleCoordinator.GetState();
        return new ControlResponse { IsSuccessful = true, Message = "Anonymous metrics settings returned.", AnonymousMetricsSettings = new AnonymousMetricsSettings { ShareAnonymousUsageMetrics = state.ShareAnonymousUsageMetrics } };
    }

    private ControlResponse UpdateAnonymousMetricsSettings(ControlEnvelope request)
    {
        AnonymousMetricsSettings? settings = JsonSerializer.Deserialize<AnonymousMetricsSettings>(request.Payload);
        if (settings == null)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The anonymous metrics settings were invalid." };
        }

        MachineScheduleState state = _machineScheduleCoordinator.UpdateAnonymousMetricsSettings(settings);
        return new ControlResponse { IsSuccessful = true, Message = "Anonymous metrics settings updated.", AnonymousMetricsSettings = new AnonymousMetricsSettings { ShareAnonymousUsageMetrics = state.ShareAnonymousUsageMetrics } };
    }

    private ControlResponse InitializeAnonymousMetrics(ControlEnvelope request)
    {
        InitializeAnonymousMetricsRequest? initialization = JsonSerializer.Deserialize<InitializeAnonymousMetricsRequest>(request.Payload);
        if (initialization == null)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The anonymous metrics initialization was invalid." };
        }

        MachineScheduleState state = _machineScheduleCoordinator.InitializeAnonymousMetrics(initialization);
        return new ControlResponse { IsSuccessful = true, Message = "Anonymous metrics initialized.", AnonymousMetricsSettings = new AnonymousMetricsSettings { ShareAnonymousUsageMetrics = state.ShareAnonymousUsageMetrics } };
    }

    private ControlResponse ReportAnonymousMetricsUsage(ControlEnvelope request)
    {
        AnonymousMetricsUsageReport? usage = JsonSerializer.Deserialize<AnonymousMetricsUsageReport>(request.Payload);
        if (usage == null || string.IsNullOrWhiteSpace(usage.AppVersion))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The anonymous metrics usage report was invalid." };
        }

        _machineScheduleCoordinator.RecordAnonymousMetricsUsage(usage);
        return new ControlResponse { IsSuccessful = true, Message = "Anonymous metrics usage recorded." };
    }

    private ControlResponse GetOperationStatus(PipeClientIdentity identity, ControlEnvelope request)
    {
        OperationStatusRequest? statusRequest = JsonSerializer.Deserialize<OperationStatusRequest>(request.Payload);
        OperationStatus? status = statusRequest == null || statusRequest.OperationId == Guid.Empty ? null : _operationStatusStore.Get(identity.UserSid, statusRequest.OperationId);
        return status == null
            ? new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.OperationNotFound, Message = "The requested operation was not found." }
            : new ControlResponse { IsSuccessful = true, Message = "Operation status returned.", OperationStatus = status };
    }

    private ControlResponse ListOperationStatuses(PipeClientIdentity identity)
    {
        return new ControlResponse { IsSuccessful = true, Message = "Operation statuses returned.", OperationStatuses = _operationStatusStore.GetAll(identity.UserSid) };
    }

    private ControlResponse GetServiceStatus(PipeClientIdentity identity)
    {
        ControlServiceStatus status = _stateCoordinator.GetStatus(DateTime.UtcNow);
        status.Agents = Array.FindAll(status.Agents, agent => string.Equals(agent.UserSid, identity.UserSid, StringComparison.OrdinalIgnoreCase));
        if (!identity.IsAdministrator && !string.Equals(status.DisplayControlLease?.OwnerUserSid, identity.UserSid, StringComparison.OrdinalIgnoreCase))
        {
            status.DisplayControlLease = null;
        }
        if (identity.IsAdministrator)
        {
            status.LatestRecoveryAdministration = _recoveryAdministrationStore.GetLatest();
            status.RecoveryAdministrations = _recoveryAdministrationStore.GetAll();
        }

        return new ControlResponse { IsSuccessful = true, Message = "Control Service status returned.", ServiceStatus = status };
    }

    private ControlResponse ResolveOperationDecision(PipeClientIdentity identity, ControlEnvelope request)
    {
        ResolveOperationDecisionRequest? resolution = JsonSerializer.Deserialize<ResolveOperationDecisionRequest>(request.Payload);
        if (resolution == null || resolution.PromptId == Guid.Empty || resolution.Choice == OperationDecisionChoice.Unknown)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ValidationFailed, Message = "A valid operation decision is required." };
        }

        OperationDecision? decision = _operationDecisionStore.Resolve(identity.UserSid, identity.SessionId, resolution.PromptId, resolution.Choice, DateTime.UtcNow);
        return decision == null
            ? new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.DecisionUnavailable, Message = "The operation decision is unavailable, expired, or has already been resolved." }
            : new ControlResponse { IsSuccessful = true, Message = "Operation decision recorded.", OperationDecision = decision };
    }

    private ControlResponse ListOperationDecisions(PipeClientIdentity identity)
    {
        return new ControlResponse { IsSuccessful = true, Message = "Pending operation decisions returned.", OperationDecisions = _operationDecisionStore.GetPending(identity.UserSid, identity.SessionId) };
    }

    private async Task<ControlResponse> CreateUserSupportBundleAsync(PipeClientIdentity identity, ControlEnvelope request, CancellationToken cancellationToken)
    {
        CreateUserSupportBundleRequest? supportBundleRequest = JsonSerializer.Deserialize<CreateUserSupportBundleRequest>(request.Payload);
        if (supportBundleRequest == null || string.IsNullOrWhiteSpace(supportBundleRequest.DestinationPath))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A support ZIP destination is required." };
        }

        string stagingRoot = Path.Combine(_storagePaths.GetUserPaths(identity.UserSid).BackupsPath, "SupportStaging", Guid.NewGuid().ToString("N"));
        string machineLogsStagingPath = Path.Combine(stagingRoot, "MachineLogs");
        string machineConfigurationStagingPath = Path.Combine(stagingRoot, "Configuration", "Machine");
        List<string> machineCollectionWarnings = new List<string>();
        try
        {
            Directory.CreateDirectory(machineLogsStagingPath);
            if (Directory.Exists(_storagePaths.MachineLogsPath))
            {
                foreach (string sourcePath in Directory.EnumerateFiles(_storagePaths.MachineLogsPath, "*", SearchOption.AllDirectories))
                {
                    string destinationPath = Path.Combine(machineLogsStagingPath, Path.GetRelativePath(_storagePaths.MachineLogsPath, sourcePath));
                    string? destinationDirectory = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrWhiteSpace(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    CopyStagedFile(sourcePath, destinationPath);
                }
            }

            StageInstallerLog(machineLogsStagingPath, machineCollectionWarnings);

            Directory.CreateDirectory(machineConfigurationStagingPath);
            foreach (string sourcePath in new[]
            {
                Path.Combine(_storagePaths.MachinePath, "DisplayControlLease.json"),
                Path.Combine(_storagePaths.MachinePath, "OperationDecisions.json"),
                Path.Combine(_storagePaths.MachinePath, "OperationStatuses.json"),
                Path.Combine(_storagePaths.MachinePath, "ScheduleState.json"),
                Path.Combine(_storagePaths.MachinePath, "PairedClients.json"),
                Path.Combine(_storagePaths.MachineDiagnosticsPath, "RecoveryAdministration.json")
            })
            {
                if (File.Exists(sourcePath))
                {
                    CopyStagedFile(sourcePath, Path.Combine(machineConfigurationStagingPath, Path.GetFileName(sourcePath)));
                }
            }

            supportBundleRequest.MachineLogsStagingPath = machineLogsStagingPath;
            supportBundleRequest.MachineConfigurationStagingPath = machineConfigurationStagingPath;
            supportBundleRequest.MachineCollectionWarnings = machineCollectionWarnings.ToArray();
            request.Payload = JsonSerializer.Serialize(supportBundleRequest);
            return await _profileOperationRouter.ManageProfileAsync(identity.UserSid, identity.SessionId, request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException)
        {
            _logger.Error(ex, "ControlClientPipeServer/CreateUserSupportBundleAsync: Could not stage Control Service logs for SID {0}.", identity.UserSid);
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "DisplayMagician could not collect Control Service logs for the support ZIP file." };
        }
        finally
        {
            DeleteSupportStagingDirectory(stagingRoot);
        }
    }

    private static void DeleteSupportStagingDirectory(string stagingRoot)
    {
        try
        {
            if (Directory.Exists(stagingRoot))
            {
                Directory.Delete(stagingRoot, true);
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException)
        {
            _logger.Warn(ex, "ControlClientPipeServer/DeleteSupportStagingDirectory: Could not remove support ZIP staging directory {0}. It was left in place for later cleanup.", stagingRoot);
        }
    }

    private static void CopyStagedFile(string sourcePath, string destinationPath)
    {
        using FileStream source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using FileStream destination = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
        source.CopyTo(destination);
    }

    private static void StageInstallerLog(string machineLogsStagingPath, List<string> warnings)
    {
        try
        {
            using RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey(@"Software\DisplayMagician", false);
            string? installerLogPath = registryKey?.GetValue("LastInstallerLogPath") as string;
            if (string.IsNullOrWhiteSpace(installerLogPath))
            {
                return;
            }

            if (!File.Exists(installerLogPath))
            {
                warnings.Add("The last installer transaction log was no longer available.");
                return;
            }

            CopyStagedFile(installerLogPath, Path.Combine(machineLogsStagingPath, "Installer-LastTransaction.log"));
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Security.SecurityException || ex is ArgumentException || ex is NotSupportedException)
        {
            _logger.Warn(ex, "ControlClientPipeServer/StageInstallerLog: The retained installer log could not be staged for the support ZIP.");
            warnings.Add("The last installer transaction log could not be collected.");
        }
    }

    private ControlResponse ForceReleaseDisplayControl(PipeClientIdentity identity, ControlEnvelope request)
    {
        if (!identity.IsAdministrator)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AdministratorRequired, Message = "Force-releasing display control requires an elevated administrator session." };
        }

        DisplayControlLease? releasedLease = _stateCoordinator.ForceReleaseDisplayControl();
        if (releasedLease == null)
        {
            return new ControlResponse { IsSuccessful = true, Message = "No display-control lease was active." };
        }

        bool wasRecoveryRecordStored = _recoveryAdministrationStore.MarkForceReleased(releasedLease, identity.UserSid, identity.SessionId);
        _auditStore.Append("DisplayControlForceReleased", "HighSeverity", $"Released lease held by session {releasedLease.OwnerSessionId}; recovery required: {releasedLease.IsRecoveryRequired}.", identity.UserSid, identity.SessionId);
        return new ControlResponse { IsSuccessful = true, Message = wasRecoveryRecordStored ? "Display control was force-released. Any interrupted operation must be checked before further use." : "Display control was force-released, but its recovery audit record could not be saved. Any interrupted operation must be checked before further use." };
    }

    private ControlResponse RecordRecoveryAdministration(PipeClientIdentity identity, ControlEnvelope request)
    {
        if (!identity.IsAdministrator)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AdministratorRequired, Message = "Recording this recovery action requires an elevated administrator session." };
        }

        RecoveryAdministrationRequest? recoveryRequest = JsonSerializer.Deserialize<RecoveryAdministrationRequest>(request.Payload);
        if (recoveryRequest?.Action != "RestartControlService" || string.IsNullOrWhiteSpace(recoveryRequest.Outcome))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The recovery administration record was invalid." };
        }

        if (!_recoveryAdministrationStore.Record(recoveryRequest.Action, recoveryRequest.Outcome, identity.UserSid, identity.SessionId))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ExecutionFailed, Message = "The recovery administration record could not be stored." };
        }

        _auditStore.Append(recoveryRequest.Action, "HighSeverity", recoveryRequest.Outcome, identity.UserSid, identity.SessionId);
        return new ControlResponse { IsSuccessful = true, Message = "Recovery administration record stored." };
    }

    private static PipeClientIdentity GetClientIdentity(NamedPipeServerStream pipe)
    {
        string? userSid = null;
        bool isAdministrator = false;
        pipe.RunAsClient(() =>
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent(TokenAccessLevels.Query);
            userSid = identity.User?.Value;
            isAdministrator = new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        });
        if (string.IsNullOrWhiteSpace(userSid) || !GetNamedPipeClientProcessId(pipe.SafePipeHandle, out uint processId))
        {
            throw new UnauthorizedAccessException("The Control Service could not verify the pipe client identity.");
        }

        using Process process = Process.GetProcessById(checked((int)processId));
        return new PipeClientIdentity(userSid, process.SessionId, isAdministrator);
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool GetNamedPipeClientProcessId(SafePipeHandle pipe, out uint clientProcessId);

    private sealed record PipeClientIdentity(string UserSid, int SessionId, bool IsAdministrator);
}
