using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Contracts;
using DisplayMagician.AppLibraries;
using DisplayMagician.Messaging;
using DisplayMagician.UserAgent.Messaging;
using DisplayMagician.UserAgent.Runtime;
using DisplayMagician.UserAgent.Runtime.Windows;
using DisplayMagician.GameLibraries;
using SharedApplyProfileResult = DisplayMagician.UserAgent.Runtime.ApplyProfileResult;
using NLog;

namespace DisplayMagician.UserAgent;

public sealed class ProfileCommandHandler
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    private readonly AgentRegistration _registration;
    private readonly string _userDataPath;
    private readonly ShortcutStore _shortcutStore;
    private readonly AutomaticGameDetectionRegistry _automaticGameDetectionRegistry;
    private readonly UserProfileOperationService _userProfileOperationService;
    private readonly ShortcutRecoveryStore _shortcutRecoveryStore;
    private readonly ShortcutRunner _shortcutRunner;
    private readonly MessageSyncService _messageSyncService;
    private readonly UserMessageStore _userMessageStore;
    private readonly UserSupportBundleGenerator _userSupportBundleGenerator;
    private readonly IInteractiveSessionStateProvider _interactiveSessionStateProvider;
    private readonly ControlServiceClient _controlServiceClient;
    private readonly OperationStatusOutbox _operationStatusOutbox;
    private readonly object _shortcutOperationsLock = new object();
    private readonly Dictionary<Guid, CancellationTokenSource> _shortcutOperations = new Dictionary<Guid, CancellationTokenSource>();
    private bool _stopRequested;

    public bool StopRequested => _stopRequested;
    public AutomaticGameDetectionRegistry AutomaticGameDetectionRegistry => _automaticGameDetectionRegistry;
    public ShortcutRunner ShortcutRunner => _shortcutRunner;

    public ProfileCommandHandler(AgentRegistration registration, ControlServiceClient controlServiceClient, IInteractiveSessionStateProvider? interactiveSessionStateProvider = null)
    {
        _registration = registration ?? throw new ArgumentNullException(nameof(registration));
        _controlServiceClient = controlServiceClient ?? throw new ArgumentNullException(nameof(controlServiceClient));
        _interactiveSessionStateProvider = interactiveSessionStateProvider ?? new WtsInteractiveSessionStateProvider();
        _userDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician", "Users", _registration.UserSid);
        _operationStatusOutbox = new OperationStatusOutbox(_userDataPath);
        ProfileRepository.ConfigureStoragePath(_userDataPath);
        AudioProfileRepository.ConfigureStoragePath(_userDataPath);
        _shortcutStore = new ShortcutStore(_userDataPath);
        _automaticGameDetectionRegistry = new AutomaticGameDetectionRegistry();
        _automaticGameDetectionRegistry.ReplaceAutomaticDetections(_shortcutStore.GetShortcutDefinitions());
        _userProfileOperationService = new UserProfileOperationService();
        _shortcutRecoveryStore = new ShortcutRecoveryStore(_userDataPath);
        _shortcutRunner = new ShortcutRunner(_shortcutStore, _automaticGameDetectionRegistry, _userProfileOperationService, _shortcutRecoveryStore);
        _userMessageStore = new UserMessageStore(_userDataPath);
        _userSupportBundleGenerator = new UserSupportBundleGenerator(_userDataPath, _registration);
        _messageSyncService = new MessageSyncService(_httpClient, _logger, "https://sync.displaymagician.com/sync/client-sync.json", Path.Combine(_userDataPath, "Messages"));
        _messageSyncService.EnsureStorage();
        _registration.IsRecoveryRequired = _shortcutRunner.IsRecoveryRequired;
    }

    public async Task<bool> RestorePendingShortcutRecoveryAsync(CancellationToken cancellationToken)
    {
        bool restored = await _shortcutRunner.RestorePendingRecoveryAsync(0, cancellationToken).ConfigureAwait(false);
        _registration.IsRecoveryRequired = !restored;
        return restored;
    }

    public async Task<bool> RunDetectedShortcutAsync(string shortcutId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(shortcutId))
        {
            return false;
        }

        Guid operationId = Guid.NewGuid();
        CancellationTokenSource operationCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        lock (_shortcutOperationsLock)
        {
            if (_shortcutOperations.Count > 0 || _registration.OperationState != AgentOperationState.Idle || _registration.IsRecoveryRequired)
            {
                operationCancellationSource.Dispose();
                return false;
            }

            _shortcutOperations.Add(operationId, operationCancellationSource);
            _registration.OperationState = AgentOperationState.Running;
        }

        await PublishShortcutStatusAsync(new OperationStatusUpdate
        {
            OperationId = operationId,
            OperationType = DisplayOperationType.StartShortcut,
            Phase = OperationPhase.Requested,
            Message = "Automatically detected shortcut operation requested."
        }, CancellationToken.None).ConfigureAwait(false);

        try
        {
            ControlResponse leaseResponse = await _controlServiceClient.AcquireDisplayControlAsync(_registration, cancellationToken).ConfigureAwait(false);
            if (!leaseResponse.IsSuccessful)
            {
                await PublishShortcutStatusAsync(CreateFailedShortcutStatus(operationId, leaseResponse.Message, leaseResponse.ErrorCode), CancellationToken.None).ConfigureAwait(false);
                CompleteShortcutOperation(operationId, operationCancellationSource);
                await ReportIdleAfterShortcutAsync().ConfigureAwait(false);
                return false;
            }

            ControlResponse stateResponse = await _controlServiceClient.ReportAgentOperationStateAsync(_registration, AgentOperationState.Running, cancellationToken).ConfigureAwait(false);
            if (!stateResponse.IsSuccessful)
            {
                await PublishShortcutStatusAsync(CreateFailedShortcutStatus(operationId, stateResponse.Message, stateResponse.ErrorCode), CancellationToken.None).ConfigureAwait(false);
                CompleteShortcutOperation(operationId, operationCancellationSource);
                await ReportIdleAfterShortcutAsync().ConfigureAwait(false);
                return false;
            }

            await RunShortcutOperationAsync(operationId, shortcutId, operationCancellationSource, isAutomaticallyDetected: true).ConfigureAwait(false);
            await ReportIdleAfterShortcutAsync().ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            CompleteShortcutOperation(operationId, operationCancellationSource);
            await ReportIdleAfterShortcutAsync().ConfigureAwait(false);
            return false;
        }
        catch (Exception ex) when (ex is IOException || ex is InvalidOperationException || ex is TimeoutException)
        {
            _logger.Warn(ex, "ProfileCommandHandler/RunDetectedShortcutAsync: Could not start automatically detected shortcut {0}.", shortcutId);
            await PublishShortcutStatusAsync(CreateFailedShortcutStatus(operationId, "The automatic shortcut could not start because the Control Service is unavailable.", ControlErrorCode.AgentUnavailable), CancellationToken.None).ConfigureAwait(false);
            CompleteShortcutOperation(operationId, operationCancellationSource);
            return false;
        }
    }

    public async Task<ControlResponse> HandleAsync(ControlEnvelope request, CancellationToken cancellationToken)
    {
        if (request.MessageType == ControlMessageType.CreateUserSupportBundle)
        {
            CreateUserSupportBundleRequest? supportBundleRequest = JsonSerializer.Deserialize<CreateUserSupportBundleRequest>(request.Payload);
            if (supportBundleRequest == null || string.IsNullOrWhiteSpace(supportBundleRequest.DestinationPath))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A destination path is required for the support ZIP file." };
            }

            try
            {
                UserSupportBundleResult result = _userSupportBundleGenerator.Create(supportBundleRequest.DestinationPath, supportBundleRequest.MachineLogsStagingPath, supportBundleRequest.MachineConfigurationStagingPath, supportBundleRequest.MachineCollectionWarnings);
                return new ControlResponse { IsSuccessful = true, Message = "Support ZIP file created.", UserSupportBundle = result };
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException)
            {
                _logger.Error(ex, "ProfileCommandHandler/HandleAsync: Could not create a support ZIP file at {0}.", supportBundleRequest.DestinationPath);
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "DisplayMagician could not create the support ZIP file at the selected location." };
            }
        }

        if (request.MessageType == ControlMessageType.StopAgentIfIdle)
        {
            if (_registration.OperationState != AgentOperationState.Idle || _registration.IsRecoveryRequired)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.DisplayControlBusy, Message = "The User Agent has active work and cannot stop." };
            }

            _stopRequested = true;
            return new ControlResponse { IsSuccessful = true, Message = "The idle User Agent is stopping." };
        }

        if (request.MessageType == ControlMessageType.ListProfiles)
        {
            ProfileRepository.RefreshDisplayDetectionState();
            ProfileRepository.UpdateActiveProfile();
            return new ControlResponse
            {
                IsSuccessful = true,
                Message = "Profiles returned.",
                ProfileList = new ProfileListResult
                {
                    SavedProfiles = ProfileRepository.AllProfiles.Select(profile => CreateDisplayProfileView(profile)).ToArray(),
                    CurrentLayout = ProfileRepository.CurrentProfile == null ? null : CreateDisplayProfileView(ProfileRepository.CurrentProfile, false)
                }
            };
        }

        if (request.MessageType == ControlMessageType.ListGames)
        {
            if (!GameLibrary.GamesLoaded)
            {
                await Task.Run(GameLibrary.LoadGamesInBackground, cancellationToken).ConfigureAwait(false);
            }

            GameView[] games = GameLibrary.AllInstalledGamesInAllLibraries
                .Select(game => new GameView
                {
                    Id = game.Id,
                    Name = game.Name,
                    Library = (int)game.GameLibraryType,
                    ExecutablePath = game.ExePath,
                    IconPath = game.IconPath,
                    Directory = game.Directory
                })
                .ToArray();
            return new ControlResponse { IsSuccessful = true, Message = "Games returned.", GameList = new GameListResult { Games = games } };
        }

        if (request.MessageType == ControlMessageType.ListApps)
        {
            if (!AppLibrary.AppsLoaded)
            {
                await Task.Run(AppLibrary.LoadAppsInBackground, cancellationToken).ConfigureAwait(false);
            }

            AppView[] apps = AppLibrary.AllInstalledAppsInAllLibraries
                .Select(app => new AppView
                {
                    Id = app.Id,
                    Name = app.Name,
                    Library = (int)app.AppLibraryType,
                    ExecutablePath = app.ExePath,
                    ExecutableArgumentsRequired = app.ExecutableArgumentsRequired,
                    Arguments = app.Arguments,
                    IconPath = app.IconPath,
                    Directory = app.Directory
                })
                .ToArray();
            return new ControlResponse { IsSuccessful = true, Message = "Applications returned.", AppList = new AppListResult { Apps = apps } };
        }

        if (request.MessageType == ControlMessageType.ListShortcuts)
        {
            ShortcutView[] shortcuts = _shortcutStore.GetShortcutDefinitions()
                .Select(shortcut => new ShortcutView
                {
                    Id = shortcut.Id,
                    Name = shortcut.Name,
                    Category = (ShortcutCategory)shortcut.Category,
                    ProfileId = shortcut.ProfileId,
                    IconPngBase64 = GetShortcutIconPngBase64(shortcut)
                })
                .ToArray();
            return new ControlResponse { IsSuccessful = true, Message = "Shortcuts returned.", ShortcutList = new ShortcutListResult { Shortcuts = shortcuts } };
        }

        if (request.MessageType == ControlMessageType.ListMessages)
        {
            return new ControlResponse { IsSuccessful = true, Message = "Messages returned.", MessageList = _userMessageStore.GetMessages() };
        }

        if (request.MessageType == ControlMessageType.SetMessageReadState)
        {
            SetMessageReadStateRequest? setReadStateRequest = JsonSerializer.Deserialize<SetMessageReadStateRequest>(request.Payload);
            if (setReadStateRequest == null || setReadStateRequest.MessageIds.Length == 0)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "At least one message ID is required." };
            }

            _userMessageStore.SetReadState(setReadStateRequest.MessageIds, setReadStateRequest.IsRead);
            return new ControlResponse { IsSuccessful = true, Message = "Message read state updated.", MessageList = _userMessageStore.GetMessages() };
        }

        if (request.MessageType == ControlMessageType.SyncMessages)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "Message synchronization is managed by the Control Service." };
        }

        if (request.MessageType == ControlMessageType.ApplyClientSyncMessages)
        {
            ClientSyncMessageManifest? manifest = JsonSerializer.Deserialize<ClientSyncMessageManifest>(request.Payload);
            if (manifest == null || manifest.SchemaVersion != 1)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The Control Service message snapshot was invalid." };
            }

            DisplayMagician.Messaging.MessageManifestDocument messageDocument = new DisplayMagician.Messaging.MessageManifestDocument
            {
                SchemaVersion = manifest.SchemaVersion,
                GeneratedUtc = manifest.GeneratedUtc,
                Messages = manifest.Messages.Select(message => new DisplayMagician.Messaging.MessageManifestEntry
                {
                    Id = message.Id,
                    Status = message.Status,
                    Title = message.Title,
                    Url = message.Url,
                    Format = message.Format,
                    Sha256 = message.Sha256,
                    ShowOnStartup = message.ShowOnStartup,
                    PublishedUtc = message.PublishedUtc,
                    DeletedUtc = message.DeletedUtc,
                    MinVersion = message.MinVersion,
                    MaxVersion = message.MaxVersion,
                    StartUtc = message.StartUtc,
                    EndUtc = message.EndUtc,
                    Vendors = message.Vendors,
                    Kind = message.Kind,
                    ReleaseVersion = message.ReleaseVersion,
                    ReleaseChannel = message.ReleaseChannel,
                    GithubReleaseId = message.GithubReleaseId,
                    UpdateAction = message.UpdateAction,
                    Media = message.Media.Select(media => new DisplayMagician.Messaging.MessageManifestMedia { Url = media.Url, Sha256 = media.Sha256, ContentType = media.ContentType }).ToList()
                }).ToList()
            };
            DisplayMagician.Messaging.MessageSyncResult syncResult = await _messageSyncService.SyncMessagesAsync(
                typeof(ProfileCommandHandler).Assembly.GetName().Version?.ToString() ?? "0.0.0.0",
                cancellationToken,
                messageDocument,
                new Uri("https://sync.displaymagician.com/"),
                authoritativeSnapshot: true).ConfigureAwait(false);
            return new ControlResponse
            {
                IsSuccessful = syncResult.Success,
                ErrorCode = syncResult.Success ? ControlErrorCode.None : ControlErrorCode.InvalidRequest,
                Message = syncResult.Success ? "Messages synchronized." : "Message synchronization failed.",
                MessageSync = new DisplayMagician.Contracts.MessageSyncResult
                {
                    IsSuccessful = syncResult.Success,
                    NewMessagesCount = syncResult.NewMessagesCount,
                    UnreadCount = syncResult.UnreadCount
                },
                MessageList = _userMessageStore.GetMessages()
            };
        }

        if (request.MessageType == ControlMessageType.StartShortcut)
        {
            StartShortcutRequest? startRequest = JsonSerializer.Deserialize<StartShortcutRequest>(request.Payload);
            if (startRequest == null || string.IsNullOrWhiteSpace(startRequest.ShortcutId))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A shortcut ID is required." };
            }

            InteractiveSessionState sessionState = _interactiveSessionStateProvider.GetState(_registration.SessionId);
            if (!InteractiveSessionPolicy.CanStartShortcut(sessionState))
            {
                return new ControlResponse
                {
                    IsSuccessful = false,
                    ErrorCode = ControlErrorCode.SessionLocked,
                    Message = sessionState == InteractiveSessionState.Locked
                        ? "The User Agent cannot start a shortcut while the interactive session is locked."
                        : "The User Agent could not verify that the interactive session is unlocked."
                };
            }

            Guid operationId = startRequest.OperationId == Guid.Empty ? Guid.NewGuid() : startRequest.OperationId;
            using IDisposable operationScope = SupportLogScope.BeginOperation(operationId);
            CancellationTokenSource operationCancellationSource = new CancellationTokenSource();
            lock (_shortcutOperationsLock)
            {
                if (_shortcutOperations.Count > 0 || _registration.OperationState != AgentOperationState.Idle)
                {
                    operationCancellationSource.Dispose();
                    return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.DisplayControlBusy, Message = "The User Agent already has a shortcut operation in progress." };
                }

                _shortcutOperations.Add(operationId, operationCancellationSource);
                _registration.OperationState = AgentOperationState.Running;
            }

            await PublishShortcutStatusAsync(new OperationStatusUpdate
            {
                OperationId = operationId,
                OperationType = DisplayOperationType.StartShortcut,
                Phase = OperationPhase.Requested,
                Message = "Shortcut operation requested."
            }, CancellationToken.None).ConfigureAwait(false);

            try
            {
                ControlResponse stateResponse = await _controlServiceClient.ReportAgentOperationStateAsync(_registration, AgentOperationState.Running, cancellationToken).ConfigureAwait(false);
                if (!stateResponse.IsSuccessful)
                {
                    await PublishShortcutStatusAsync(CreateFailedShortcutStatus(operationId, stateResponse.Message, stateResponse.ErrorCode), CancellationToken.None).ConfigureAwait(false);
                    CompleteShortcutOperation(operationId, operationCancellationSource);
                    return new ControlResponse { IsSuccessful = false, ErrorCode = stateResponse.ErrorCode, Message = stateResponse.Message };
                }
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidOperationException || ex is TimeoutException)
            {
                _logger.Warn(ex, "ProfileCommandHandler/HandleAsync: Could not report the manual shortcut operation {0} as running.", operationId);
                await PublishShortcutStatusAsync(CreateFailedShortcutStatus(operationId, "The shortcut could not start because the Control Service is unavailable.", ControlErrorCode.AgentUnavailable), CancellationToken.None).ConfigureAwait(false);
                CompleteShortcutOperation(operationId, operationCancellationSource);
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The Control Service is unavailable." };
            }

            _ = RunShortcutOperationAsync(operationId, startRequest.ShortcutId, operationCancellationSource, isAutomaticallyDetected: false);
            return new ControlResponse
            {
                IsSuccessful = true,
                Message = "Shortcut operation started.",
                OperationStatus = new OperationStatus { OperationId = operationId, OperationType = DisplayOperationType.StartShortcut, Phase = OperationPhase.Requested }
            };
        }

        if (request.MessageType == ControlMessageType.CancelOperation)
        {
            CancelOperationRequest? cancelRequest = JsonSerializer.Deserialize<CancelOperationRequest>(request.Payload);
            if (cancelRequest == null || cancelRequest.OperationId == Guid.Empty)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "An operation ID is required." };
            }

            using IDisposable operationScope = SupportLogScope.BeginOperation(cancelRequest.OperationId);
            lock (_shortcutOperationsLock)
            {
                if (!_shortcutOperations.TryGetValue(cancelRequest.OperationId, out CancellationTokenSource? operationCancellationSource))
                {
                    return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The requested operation is not active in this User Agent." };
                }

                operationCancellationSource.Cancel();
            }

            return new ControlResponse { IsSuccessful = true, Message = "Shortcut cancellation requested.", OperationStatus = new OperationStatus { OperationId = cancelRequest.OperationId, OperationType = DisplayOperationType.StartShortcut, Phase = OperationPhase.Requested } };
        }

        if (request.MessageType == ControlMessageType.GetRepositorySnapshot)
        {
            RepositorySnapshotRequest? snapshotRequest = JsonSerializer.Deserialize<RepositorySnapshotRequest>(request.Payload);
            if (snapshotRequest == null || snapshotRequest.Repository == RepositoryKind.Unknown)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The requested repository snapshot is not available from the User Agent yet." };
            }

            if (snapshotRequest.Repository == RepositoryKind.Shortcuts)
            {
                return new ControlResponse { IsSuccessful = true, Message = "Repository snapshot returned.", RepositorySnapshot = _shortcutStore.GetSnapshot() };
            }

            string fileName = snapshotRequest.Repository == RepositoryKind.DisplayProfiles ? "DisplayProfiles.json" : "AudioProfiles.json";
            string directoryName = snapshotRequest.Repository == RepositoryKind.DisplayProfiles ? "Profiles" : "AudioProfiles";
            string path = Path.Combine(_userDataPath, directoryName, fileName);
            byte[] content = File.Exists(path) ? File.ReadAllBytes(path) : Array.Empty<byte>();
            long revision = content.Length == 0 ? 0 : BitConverter.ToInt64(SHA256.HashData(content), 0);
            return new ControlResponse { IsSuccessful = true, Message = "Repository snapshot returned.", RepositorySnapshot = new RepositorySnapshot { Repository = snapshotRequest.Repository, Revision = revision, Json = content.Length == 0 ? string.Empty : System.Text.Encoding.Unicode.GetString(content).TrimStart('\uFEFF') } };
        }

        if (request.MessageType == ControlMessageType.CommitRepositorySnapshot)
        {
            RepositoryCommitRequest? commitRequest = JsonSerializer.Deserialize<RepositoryCommitRequest>(request.Payload);
            if (commitRequest == null || commitRequest.Repository == RepositoryKind.Unknown)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The requested repository commit is not available from the User Agent yet." };
            }

            if (commitRequest.Repository == RepositoryKind.Shortcuts)
            {
                try
                {
                    List<ShortcutDefinition> shortcutDefinitions = _shortcutStore.GetShortcutDefinitions(commitRequest.Json);
                    if (!AutomaticGameDetectionRegistry.TryValidateAutomaticDetections(shortcutDefinitions, out string validationError))
                    {
                        return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = validationError };
                    }

                    RepositoryCommitResult shortcutCommit = _shortcutStore.Commit(commitRequest);
                    if (!shortcutCommit.WasConflict)
                    {
                        _automaticGameDetectionRegistry.ReplaceAutomaticDetections(_shortcutStore.GetShortcutDefinitions());
                    }
                    return new ControlResponse { IsSuccessful = true, Message = shortcutCommit.WasConflict ? "Repository commit conflicted with a newer Agent revision." : "Repository committed.", RepositoryCommit = shortcutCommit };
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is ArgumentException)
                {
                    return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The shortcut repository commit could not be validated or persisted." };
                }
            }

            string directoryName = commitRequest.Repository == RepositoryKind.DisplayProfiles ? "Profiles" : "AudioProfiles";
            string fileName = commitRequest.Repository == RepositoryKind.DisplayProfiles ? "DisplayProfiles.json" : "AudioProfiles.json";
            string path = Path.Combine(_userDataPath, directoryName, fileName);
            byte[] existing = File.Exists(path) ? File.ReadAllBytes(path) : Array.Empty<byte>();
            long revision = existing.Length == 0 ? 0 : BitConverter.ToInt64(SHA256.HashData(existing), 0);
            if (revision != commitRequest.ExpectedRevision)
            {
                return new ControlResponse { IsSuccessful = true, Message = "Repository commit conflicted with a newer Agent revision.", RepositoryCommit = new RepositoryCommitResult { WasConflict = true, Snapshot = CreateSnapshot(commitRequest.Repository, path) } };
            }

            try
            {
                using JsonDocument _ = JsonDocument.Parse(commitRequest.Json);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                string temporaryPath = Path.Combine(Path.GetDirectoryName(path)!, $".{fileName}.{Guid.NewGuid():N}.tmp");
                File.WriteAllText(temporaryPath, commitRequest.Json, System.Text.Encoding.Unicode);
                if (File.Exists(path)) File.Replace(temporaryPath, path, null); else File.Move(temporaryPath, path);
                if (commitRequest.Repository == RepositoryKind.DisplayProfiles) ProfileRepository.ConfigureStoragePath(_userDataPath); else AudioProfileRepository.ConfigureStoragePath(_userDataPath);
                return new ControlResponse { IsSuccessful = true, Message = "Repository committed.", RepositoryCommit = new RepositoryCommitResult { Snapshot = CreateSnapshot(commitRequest.Repository, path) } };
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is ArgumentException)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The repository commit could not be validated or persisted." };
            }
        }

        if (request.MessageType == ControlMessageType.ListAudioProfiles)
        {
            AudioProfileItem[] savedProfiles = AudioProfileRepository.AllAudioProfiles.ToArray();
            AudioProfileRepository.UpdateActiveAudioProfile();
            bool canAccessAudioSettings = AudioProfileRepository.CanAccessAudioSettings;
            AudioProfileItem? currentProfile = AudioProfileRepository.CurrentAudioProfile;
            return new ControlResponse
            {
                IsSuccessful = true,
                Message = "Audio profiles returned.",
                AudioProfileList = new AudioProfileListResult
                {
                    CanAccessAudioSettings = canAccessAudioSettings,
                    SavedProfiles = savedProfiles.Select(profile => CreateAudioProfileView(profile, canAccessAudioSettings)).ToArray(),
                    CurrentLayout = currentProfile == null ? null : CreateAudioProfileView(currentProfile, canAccessAudioSettings, false)
                }
            };
        }

        if (request.MessageType == ControlMessageType.ApplyAudioProfile)
        {
            ApplyAudioProfileRequest? audioApplyRequest = JsonSerializer.Deserialize<ApplyAudioProfileRequest>(request.Payload);
            if (audioApplyRequest == null || string.IsNullOrWhiteSpace(audioApplyRequest.ProfileId))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ValidationFailed, Message = "An audio profile ID is required." };
            }

            if (!AudioProfileRepository.AllAudioProfiles.Any(profile => string.Equals(profile.UUID, audioApplyRequest.ProfileId, StringComparison.OrdinalIgnoreCase)))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AudioProfileNotFound, Message = "The requested audio profile does not exist." };
            }

            int deviceWaitMilliseconds = audioApplyRequest.DeviceWaitMilliseconds > 0 ? audioApplyRequest.DeviceWaitMilliseconds : ControlProtocol.DefaultAudioDeviceWaitMilliseconds;
            ApplyAudioProfileOperationResult result = _userProfileOperationService.ApplyAudioProfile(audioApplyRequest.ProfileId, deviceWaitMilliseconds);
            return new ControlResponse { IsSuccessful = result.IsSuccessful, ErrorCode = result.IsSuccessful ? ControlErrorCode.None : ControlErrorCode.ExecutionFailed, Message = result.IsSuccessful ? "Audio profile applied." : $"Audio profile could not be applied. Missing devices: {string.Join(", ", result.MissingDeviceNames)}" };
        }

        if (request.MessageType == ControlMessageType.CreateAudioProfileFromCurrent)
        {
            CreateProfileRequest? createRequest = JsonSerializer.Deserialize<CreateProfileRequest>(request.Payload);
            if (createRequest == null || !AudioProfileRepository.CanAccessAudioSettings || !AudioProfileRepository.IsValidFilename(createRequest.Name) || AudioProfileRepository.AllAudioProfiles.Any(profile => string.Equals(profile.Name, createRequest.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ValidationFailed, Message = "The audio profile name is invalid, already exists, or audio settings cannot be read." };
            }

            AudioProfileItem profile = new AudioProfileItem { Name = createRequest.Name };
            if (!profile.CreateProfileFromCurrentAudioSettings() || !AudioProfileRepository.AddAudioProfile(profile))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ExecutionFailed, Message = "The current audio settings could not be saved." };
            }

            return new ControlResponse { IsSuccessful = true, Message = "Audio profile created." };
        }

        if (request.MessageType == ControlMessageType.RenameAudioProfile)
        {
            RenameProfileRequest? renameRequest = JsonSerializer.Deserialize<RenameProfileRequest>(request.Payload);
            AudioProfileItem? profile = renameRequest == null ? null : AudioProfileRepository.AllAudioProfiles.FirstOrDefault(item => string.Equals(item.UUID, renameRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
            if (profile == null)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AudioProfileNotFound, Message = "The requested audio profile does not exist." };
            }

            return AudioProfileRepository.RenameAudioProfile(profile, renameRequest!.Name)
                ? new ControlResponse { IsSuccessful = true, Message = "Audio profile renamed." }
                : new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ValidationFailed, Message = "The audio profile could not be renamed." };
        }

        if (request.MessageType == ControlMessageType.DeleteAudioProfile)
        {
            DeleteProfileRequest? deleteRequest = JsonSerializer.Deserialize<DeleteProfileRequest>(request.Payload);
            AudioProfileItem? profile = deleteRequest == null ? null : AudioProfileRepository.AllAudioProfiles.FirstOrDefault(item => string.Equals(item.UUID, deleteRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
            if (profile == null)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AudioProfileNotFound, Message = "The requested audio profile does not exist." };
            }

            return AudioProfileRepository.RemoveAudioProfile(profile)
                ? new ControlResponse { IsSuccessful = true, Message = "Audio profile deleted." }
                : new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ExecutionFailed, Message = "The audio profile could not be deleted." };
        }

        if (request.MessageType == ControlMessageType.UpdateAudioProfileFromCurrent)
        {
            DeleteProfileRequest? updateRequest = JsonSerializer.Deserialize<DeleteProfileRequest>(request.Payload);
            AudioProfileItem? profile = updateRequest == null ? null : AudioProfileRepository.AllAudioProfiles.FirstOrDefault(item => string.Equals(item.UUID, updateRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
            if (profile == null)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AudioProfileNotFound, Message = "The requested audio profile does not exist." };
            }

            if (!AudioProfileRepository.CanAccessAudioSettings || !profile.CreateProfileFromCurrentAudioSettings() || !AudioProfileRepository.SaveAudioProfiles())
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ExecutionFailed, Message = "The current audio settings could not update the audio profile." };
            }

            return new ControlResponse { IsSuccessful = true, Message = "Audio profile updated." };
        }

        if (request.MessageType == ControlMessageType.CreateProfileFromCurrent)
        {
            CreateProfileRequest? createRequest = JsonSerializer.Deserialize<CreateProfileRequest>(request.Payload);
            if (createRequest == null || !ProfileRepository.IsValidFilename(createRequest.Name) || ProfileRepository.AllProfiles.Any(profile => string.Equals(profile.Name, createRequest.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ValidationFailed, Message = "The display profile name is invalid or already in use." };
            }

            ProfileRepository.RefreshDisplayDetectionState();
            ProfileRepository.UpdateActiveProfile();
            ProfileItem? profile = ProfileRepository.CurrentProfile;
            if (profile == null)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ExecutionFailed, Message = "The current display configuration could not be captured." };
            }

            profile.Name = createRequest.Name;
            return ProfileRepository.AddProfile(profile)
                ? new ControlResponse { IsSuccessful = true, Message = "Display profile created." }
                : new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ExecutionFailed, Message = "The display profile could not be saved." };
        }

        if (request.MessageType == ControlMessageType.RenameProfile)
        {
            RenameProfileRequest? renameRequest = JsonSerializer.Deserialize<RenameProfileRequest>(request.Payload);
            ProfileItem? profile = renameRequest == null ? null : ProfileRepository.AllProfiles.FirstOrDefault(item => string.Equals(item.UUID, renameRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
            if (profile == null)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ProfileNotFound, Message = "The requested display profile does not exist." };
            }

            if (!ProfileRepository.RenameProfile(profile, renameRequest!.Name))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ValidationFailed, Message = "The display profile could not be renamed." };
            }

            return new ControlResponse { IsSuccessful = true, Message = "Display profile renamed." };
        }

        if (request.MessageType == ControlMessageType.DeleteProfile)
        {
            DeleteProfileRequest? deleteRequest = JsonSerializer.Deserialize<DeleteProfileRequest>(request.Payload);
            ProfileItem? profile = deleteRequest == null ? null : ProfileRepository.AllProfiles.FirstOrDefault(item => string.Equals(item.UUID, deleteRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
            if (profile == null)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ProfileNotFound, Message = "The requested display profile does not exist." };
            }

            return ProfileRepository.RemoveProfile(profile)
                ? new ControlResponse { IsSuccessful = true, Message = "Display profile deleted." }
                : new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ExecutionFailed, Message = "The display profile could not be deleted." };
        }

        if (request.MessageType == ControlMessageType.UpdateProfileFromCurrent)
        {
            DeleteProfileRequest? updateRequest = JsonSerializer.Deserialize<DeleteProfileRequest>(request.Payload);
            ProfileItem? profile = updateRequest == null ? null : ProfileRepository.AllProfiles.FirstOrDefault(item => string.Equals(item.UUID, updateRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
            if (profile == null)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ProfileNotFound, Message = "The requested display profile does not exist." };
            }

            ProfileRepository.CopyCurrentLayoutToProfile(profile);
            return ProfileRepository.SaveProfiles()
                ? new ControlResponse { IsSuccessful = true, Message = "Display profile updated." }
                : new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ExecutionFailed, Message = "The display profile could not be updated." };
        }

        if (request.MessageType == ControlMessageType.UpdateDisplayProfileSettings)
        {
            UpdateDisplayProfileSettingsRequest? settingsRequest = JsonSerializer.Deserialize<UpdateDisplayProfileSettingsRequest>(request.Payload);
            ProfileItem? profile = settingsRequest == null ? null : ProfileRepository.AllProfiles.FirstOrDefault(item => string.Equals(item.UUID, settingsRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
            if (settingsRequest?.Settings == null)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ValidationFailed, Message = "The display profile settings request was invalid." };
            }

            if (profile == null)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ProfileNotFound, Message = "The requested display profile does not exist." };
            }

            profile.WallpaperConfiguration.WallpaperMode = settingsRequest.Settings.ApplyWallpaper ? Wallpaper.Mode.Apply : Wallpaper.Mode.DoNothing;
            profile.ApplyProfileCount = settingsRequest.Settings.ApplyProfileCount;
            profile.ApplyProfileDelay = settingsRequest.Settings.ApplyProfileDelay;
            profile.ForceExplorerRestart = settingsRequest.Settings.ForceExplorerRestart;
            return ProfileRepository.SaveProfiles()
                ? new ControlResponse { IsSuccessful = true, Message = "Display profile settings updated." }
                : new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ExecutionFailed, Message = "The display profile settings could not be saved." };
        }

        if (request.MessageType != ControlMessageType.ApplyProfile)
        {
            return new ControlResponse
            {
                IsSuccessful = false,
                ErrorCode = ControlErrorCode.InvalidRequest,
                Message = $"The User Agent does not support the {request.MessageType} command."
            };
        }

        ApplyProfileRequest? applyRequest = JsonSerializer.Deserialize<ApplyProfileRequest>(request.Payload);
        if (applyRequest == null || string.IsNullOrWhiteSpace(applyRequest.ProfileId) || applyRequest.OperationId == Guid.Empty)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ValidationFailed, Message = "A display profile ID and operation ID are required." };
        }

        if (!ProfileRepository.AllProfiles.Any(profile => string.Equals(profile.UUID, applyRequest.ProfileId, StringComparison.OrdinalIgnoreCase)))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ProfileNotFound, Message = "The requested display profile does not exist." };
        }

        Guid profileOperationId = applyRequest.OperationId;
        _registration.OperationState = AgentOperationState.Running;
        await PublishShortcutStatusAsync(new OperationStatusUpdate
        {
            OperationId = profileOperationId,
            OperationType = DisplayOperationType.ApplyDisplayProfile,
            Phase = OperationPhase.Requested,
            Message = "Display profile operation requested."
        }, CancellationToken.None).ConfigureAwait(false);

        _ = RunDisplayProfileOperationAsync(profileOperationId, applyRequest.ProfileId, cancellationToken);
        return new ControlResponse
        {
            IsSuccessful = true,
            Message = "Display profile operation accepted.",
            OperationStatus = new OperationStatus
            {
                OperationId = profileOperationId,
                OperationType = DisplayOperationType.ApplyDisplayProfile,
                Phase = OperationPhase.Requested
            }
        };
    }

    private async Task RunDisplayProfileOperationAsync(Guid profileOperationId, string profileId, CancellationToken cancellationToken)
    {
        using IDisposable operationScope = SupportLogScope.BeginOperation(profileOperationId);
        try
        {
            await PublishShortcutStatusAsync(new OperationStatusUpdate
            {
                OperationId = profileOperationId,
                OperationType = DisplayOperationType.ApplyDisplayProfile,
                Phase = OperationPhase.ApplyingDisplayProfile,
                Message = "Applying display profile."
            }, CancellationToken.None).ConfigureAwait(false);
            ApplyDisplayProfileOperationResult result = await _userProfileOperationService.ApplyDisplayProfileAsync(profileId, cancellationToken).ConfigureAwait(false);
            await PublishShortcutStatusAsync(new OperationStatusUpdate
            {
                OperationId = profileOperationId,
                OperationType = DisplayOperationType.ApplyDisplayProfile,
                Phase = result.IsSuccessful ? OperationPhase.Completed : OperationPhase.Failed,
                Message = result.IsSuccessful ? "Display profile applied." : "Display profile could not be applied.",
                IsTerminal = true,
                IsSuccessful = result.IsSuccessful,
                ErrorCode = result.IsSuccessful ? ControlErrorCode.None : ControlErrorCode.ExecutionFailed
            }, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException || ex is InvalidOperationException || ex is TimeoutException)
        {
            await PublishShortcutStatusAsync(new OperationStatusUpdate
            {
                OperationId = profileOperationId,
                OperationType = DisplayOperationType.ApplyDisplayProfile,
                Phase = OperationPhase.Failed,
                Message = "Display profile operation failed unexpectedly.",
                IsTerminal = true,
                ErrorCode = ControlErrorCode.ExecutionFailed
            }, CancellationToken.None).ConfigureAwait(false);
            _logger.Error(ex, "ProfileCommandHandler/HandleAsync: Display profile operation {0} failed unexpectedly.", profileOperationId);
        }
        finally
        {
            _registration.OperationState = AgentOperationState.Idle;
            await ReportIdleAfterShortcutAsync().ConfigureAwait(false);
        }
    }

    private async Task RunShortcutOperationAsync(Guid operationId, string shortcutId, CancellationTokenSource operationCancellationSource, bool isAutomaticallyDetected)
    {
        using IDisposable operationScope = SupportLogScope.BeginOperation(operationId);
        try
        {
            ShortcutRunResult result = isAutomaticallyDetected
                ? await _shortcutRunner.ApplyDetectedGameShortcutAsync(shortcutId, 0, operationCancellationSource.Token, PublishShortcutStatusAsync, operationId, RequestShortcutDecisionAsync).ConfigureAwait(false)
                : await _shortcutRunner.ApplyShortcutProfilesAsync(shortcutId, 0, operationCancellationSource.Token, PublishShortcutStatusAsync, operationId, RequestShortcutDecisionAsync).ConfigureAwait(false);
            bool wasSuccessful = result.Outcome == ShortcutRunOutcome.Completed;
            OperationPhase terminalPhase = result.Outcome == ShortcutRunOutcome.Cancelled ? OperationPhase.Cancelled : wasSuccessful ? OperationPhase.Completed : OperationPhase.Failed;
            ControlErrorCode errorCode = wasSuccessful || terminalPhase == OperationPhase.Cancelled
                ? ControlErrorCode.None
                : result.Outcome == ShortcutRunOutcome.ShortcutNotFound
                    ? ControlErrorCode.ShortcutNotFound
                    : ControlErrorCode.ExecutionFailed;
            await PublishShortcutStatusAsync(new OperationStatusUpdate
            {
                OperationId = operationId,
                OperationType = DisplayOperationType.StartShortcut,
                Phase = terminalPhase,
                Message = result.Outcome.ToString(),
                IsTerminal = true,
                IsSuccessful = wasSuccessful,
                ErrorCode = errorCode
            }, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "ProfileCommandHandler/RunShortcutOperationAsync: Shortcut operation {0} failed unexpectedly.", operationId);
            await PublishShortcutStatusAsync(new OperationStatusUpdate
            {
                OperationId = operationId,
                OperationType = DisplayOperationType.StartShortcut,
                Phase = OperationPhase.Failed,
                Message = "Shortcut operation failed unexpectedly.",
                IsTerminal = true,
                ErrorCode = ControlErrorCode.ExecutionFailed
            }, CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            CompleteShortcutOperation(operationId, operationCancellationSource);
            await ReportIdleAfterShortcutAsync().ConfigureAwait(false);
        }
    }

    private static OperationStatusUpdate CreateFailedShortcutStatus(Guid operationId, string message, ControlErrorCode errorCode)
    {
        return new OperationStatusUpdate
        {
            OperationId = operationId,
            OperationType = DisplayOperationType.StartShortcut,
            Phase = OperationPhase.Failed,
            Message = message,
            IsTerminal = true,
            ErrorCode = errorCode
        };
    }

    private void CompleteShortcutOperation(Guid operationId, CancellationTokenSource operationCancellationSource)
    {
        lock (_shortcutOperationsLock)
        {
            _shortcutOperations.Remove(operationId);
            _registration.IsRecoveryRequired = _shortcutRunner.IsRecoveryRequired;
            _registration.OperationState = AgentOperationState.Idle;
        }

        operationCancellationSource.Dispose();
    }

    private async Task ReportIdleAfterShortcutAsync()
    {
        try
        {
            ControlResponse response = await _controlServiceClient.ReportAgentOperationStateAsync(_registration, AgentOperationState.Idle, CancellationToken.None).ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                _logger.Warn("ProfileCommandHandler/ReportIdleAfterShortcutAsync: Could not report the User Agent as idle after a shortcut: {0}", response.Message);
            }
        }
        catch (Exception ex) when (ex is IOException || ex is InvalidOperationException || ex is TimeoutException)
        {
            _logger.Warn(ex, "ProfileCommandHandler/ReportIdleAfterShortcutAsync: Could not report the User Agent as idle after a shortcut.");
        }
    }

    private Task<OperationDecision> RequestShortcutDecisionAsync(RequestOperationDecisionRequest request, CancellationToken cancellationToken)
    {
        return _controlServiceClient.RequestOperationDecisionAsync(_registration, request, cancellationToken);
    }

    private async Task PublishShortcutStatusAsync(OperationStatusUpdate update, CancellationToken cancellationToken)
    {
        _operationStatusOutbox.Enqueue(update);
        await FlushPendingOperationStatusUpdatesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Retries every status accepted locally but not yet acknowledged by ControlService.</summary>
    public async Task FlushPendingOperationStatusUpdatesAsync(CancellationToken cancellationToken)
    {
        foreach (OperationStatusUpdate update in _operationStatusOutbox.GetPending())
        {
            try
            {
                await _controlServiceClient.PublishOperationStatusAsync(_registration, update, cancellationToken).ConfigureAwait(false);
                _operationStatusOutbox.Acknowledge(update.UpdateId);
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidOperationException || ex is TimeoutException || ex is OperationCanceledException)
            {
                _logger.Warn(ex, "ProfileCommandHandler/FlushPendingOperationStatusUpdatesAsync: Could not publish shortcut operation {0} phase {1}; it remains queued for retry.", update.OperationId, update.Phase);
                return;
            }
        }
    }

    /// <summary>Restores the current Agent view of active operations after ControlService has restarted.</summary>
    public Task ReconcileOperationStatusesAsync(CancellationToken cancellationToken)
    {
        return _controlServiceClient.ReconcileOperationStatusesAsync(_registration, _operationStatusOutbox.GetActive(), cancellationToken);
    }

    private static string? GetThumbnailPngBase64(ProfileItem profile)
    {
        if (profile.ProfileBitmap == null)
        {
            return null;
        }

        using MemoryStream stream = new MemoryStream();
        profile.ProfileBitmap.Save(stream, ImageFormat.Png);
        return Convert.ToBase64String(stream.ToArray());
    }

    private static DisplayProfileView CreateDisplayProfileView(ProfileItem profile, bool isSaved = true)
    {
        bool isValid = profile.HasUsableSavedConfiguration(out string diagnosticMessage);
        string[] undetectedDisplays = isValid ? profile.GetUndetectedDisplayDescriptions().ToArray() : Array.Empty<string>();
        GDI_DISPLAY_SETTING? primaryDisplay = profile.WindowsDisplayConfig.GdiDisplaySettings
            .Select(display => display.Value)
            .FirstOrDefault(display => display.IsPrimary);
        if (undetectedDisplays.Length > 0)
        {
            diagnosticMessage = string.Join(Environment.NewLine, undetectedDisplays);
        }

        return new DisplayProfileView
        {
            Id = profile.UUID,
            Name = profile.Name,
            ThumbnailPngBase64 = GetThumbnailPngBase64(profile),
            ConnectedDisplayCount = profile.WindowsDisplayConfig.DisplayIdentifiers.Count,
            PrimaryDisplayWidth = primaryDisplay == null ? 0 : (int)primaryDisplay.Value.DeviceMode.PixelsWidth,
            PrimaryDisplayHeight = primaryDisplay == null ? 0 : (int)primaryDisplay.Value.DeviceMode.PixelsHeight,
            IsSaved = isSaved,
            IsActive = isSaved && ProfileRepository.IsActiveProfile(profile),
            IsValid = isValid,
            DiagnosticMessage = diagnosticMessage ?? string.Empty,
            Settings = new DisplayProfileSettings
            {
                ApplyWallpaper = profile.WallpaperConfiguration.WallpaperMode == Wallpaper.Mode.Apply,
                BackgroundDescription = profile.WallpaperConfiguration.WallpaperSettings?.BackgroundType.ToString() ?? "Saved Pictures (unique per display)",
                ApplyProfileCount = profile.ApplyProfileCount,
                ApplyProfileDelay = profile.ApplyProfileDelay,
                ForceExplorerRestart = profile.ForceExplorerRestart
            }
        };
    }

    private static AudioProfileView CreateAudioProfileView(AudioProfileItem profile, bool canAccessAudioSettings, bool isSaved = true)
    {
        return new AudioProfileView
        {
            Id = profile.UUID,
            Name = profile.Name,
            IsSaved = isSaved,
            IsActive = isSaved && AudioProfileRepository.IsActiveAudioProfile(profile),
            SettingsText = profile.GenerateSettingsText(),
            UnavailableDeviceNames = canAccessAudioSettings ? profile.GetUnavailableAudioDeviceNames().ToArray() : Array.Empty<string>()
        };
    }

    private static string? GetShortcutIconPngBase64(ShortcutDefinition shortcut)
    {
        using Bitmap? icon = LoadShortcutIcon(shortcut);
        if (icon == null)
        {
            return null;
        }

        using Bitmap? profileThumbnail = LoadProfileThumbnail(shortcut.ProfileId);
        using Bitmap composite = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(composite);
        graphics.Clear(Color.Transparent);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(icon, GetCenteredBounds(icon.Size, composite.Size));

        if (profileThumbnail != null)
        {
            Size overlaySize = FitWithin(profileThumbnail.Size, new Size(70, 70));
            graphics.DrawImage(profileThumbnail, composite.Width - overlaySize.Width, composite.Height - overlaySize.Height - 5, overlaySize.Width, overlaySize.Height);
        }

        using MemoryStream stream = new MemoryStream();
        composite.Save(stream, ImageFormat.Png);
        return Convert.ToBase64String(stream.ToArray());
    }

    private static Bitmap? LoadShortcutIcon(ShortcutDefinition shortcut)
    {
        string iconPath = shortcut.OriginalIconPath;
        if (string.IsNullOrWhiteSpace(iconPath) || !File.Exists(iconPath))
        {
            iconPath = shortcut.ExecutablePath;
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(iconPath) && File.Exists(iconPath))
            {
                using Icon? icon = Icon.ExtractAssociatedIcon(iconPath);
                if (icon != null)
                {
                    return icon.ToBitmap();
                }

                using Image image = Image.FromFile(iconPath);
                return new Bitmap(image);
            }
        }
        catch (Exception ex) when (ex is ArgumentException || ex is ExternalException || ex is IOException)
        {
            _logger.Debug(ex, "ProfileCommandHandler/LoadShortcutIcon: Could not load shortcut icon from {0}.", iconPath);
        }

        return null;
    }

    private static Bitmap? LoadProfileThumbnail(string profileId)
    {
        ProfileItem? profile = ProfileRepository.AllProfiles.FirstOrDefault(item => string.Equals(item.UUID, profileId, StringComparison.OrdinalIgnoreCase));
        return profile?.ProfileBitmap == null ? null : new Bitmap(profile.ProfileBitmap);
    }

    private static Rectangle GetCenteredBounds(Size imageSize, Size targetSize)
    {
        Size fittedSize = FitWithin(imageSize, targetSize);
        return new Rectangle((targetSize.Width - fittedSize.Width) / 2, (targetSize.Height - fittedSize.Height) / 2, fittedSize.Width, fittedSize.Height);
    }

    private static Size FitWithin(Size imageSize, Size bounds)
    {
        if (imageSize.Width <= 0 || imageSize.Height <= 0)
        {
            return bounds;
        }

        double scale = Math.Min((double)bounds.Width / imageSize.Width, (double)bounds.Height / imageSize.Height);
        return new Size(Math.Max(1, (int)Math.Round(imageSize.Width * scale)), Math.Max(1, (int)Math.Round(imageSize.Height * scale)));
    }

    private static RepositorySnapshot CreateSnapshot(RepositoryKind repository, string path)
    {
        byte[] content = File.Exists(path) ? File.ReadAllBytes(path) : Array.Empty<byte>();
        return new RepositorySnapshot
        {
            Repository = repository,
            Revision = content.Length == 0 ? 0 : BitConverter.ToInt64(SHA256.HashData(content), 0),
            Json = content.Length == 0 ? string.Empty : System.Text.Encoding.Unicode.GetString(content).TrimStart('\uFEFF')
        };
    }
}
