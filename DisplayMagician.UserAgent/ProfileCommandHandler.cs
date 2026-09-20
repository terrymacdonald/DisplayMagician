using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Contracts;
using DisplayMagician.AppLibraries;
using DisplayMagician.Messaging;
using DisplayMagician.UserAgent.Messaging;
using DisplayMagicianShared;
using DisplayMagician.GameLibraries;
using SharedApplyProfileResult = DisplayMagicianShared.ApplyProfileResult;
using NLog;

namespace DisplayMagician.UserAgent;

public sealed class ProfileCommandHandler
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly AgentRegistration _registration;
    private readonly string _userDataPath;
    private readonly ShortcutStore _shortcutStore;
    private readonly AutomaticGameDetectionRegistry _automaticGameDetectionRegistry;
    private readonly UserProfileOperationService _userProfileOperationService;
    private readonly ShortcutRecoveryStore _shortcutRecoveryStore;
    private readonly ShortcutRunner _shortcutRunner;
    private readonly MessageSyncService _messageSyncService;
    private readonly UserMessageStore _userMessageStore;
    private readonly IInteractiveSessionStateProvider _interactiveSessionStateProvider;
    private readonly ControlServiceClient _controlServiceClient;
    private bool _stopRequested;

    public bool StopRequested => _stopRequested;
    public AutomaticGameDetectionRegistry AutomaticGameDetectionRegistry => _automaticGameDetectionRegistry;
    public ShortcutRunner ShortcutRunner => _shortcutRunner;

    public ProfileCommandHandler(AgentRegistration registration, IInteractiveSessionStateProvider? interactiveSessionStateProvider = null)
    {
        _registration = registration ?? throw new ArgumentNullException(nameof(registration));
        _interactiveSessionStateProvider = interactiveSessionStateProvider ?? new WtsInteractiveSessionStateProvider();
        _controlServiceClient = new ControlServiceClient();
        _userDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician", "Users", _registration.UserSid);
        ProfileRepository.ConfigureStoragePath(_userDataPath);
        AudioProfileRepository.ConfigureStoragePath(_userDataPath);
        _shortcutStore = new ShortcutStore(_userDataPath);
        _automaticGameDetectionRegistry = new AutomaticGameDetectionRegistry();
        _automaticGameDetectionRegistry.ReplaceAutomaticDetections(_shortcutStore.GetShortcutDefinitions());
        _userProfileOperationService = new UserProfileOperationService();
        _shortcutRecoveryStore = new ShortcutRecoveryStore(_userDataPath);
        _shortcutRunner = new ShortcutRunner(_shortcutStore, _automaticGameDetectionRegistry, _userProfileOperationService, _shortcutRecoveryStore);
        _userMessageStore = new UserMessageStore(_userDataPath);
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

    public async Task<ControlResponse> HandleAsync(ControlEnvelope request, CancellationToken cancellationToken)
    {
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
            ProfileSummary[] profiles = ProfileRepository.AllProfiles
                .Select(profile => new ProfileSummary { Id = profile.UUID, Name = profile.Name })
                .ToArray();
            return new ControlResponse
            {
                IsSuccessful = true,
                Message = "Profiles returned.",
                ProfileList = new ProfileListResult
                {
                    Profiles = profiles,
                    Views = ProfileRepository.AllProfiles.Select(profile => new DisplayProfileView { Id = profile.UUID, Name = profile.Name, ThumbnailPngBase64 = GetThumbnailPngBase64(profile) }).ToArray()
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
            DisplayMagician.Messaging.MessageSyncResult syncResult = await _messageSyncService.SyncMessagesAsync(
                typeof(ProfileCommandHandler).Assembly.GetName().Version?.ToString() ?? "0.0.0.0",
                cancellationToken).ConfigureAwait(false);
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

            _registration.OperationState = AgentOperationState.Running;
            try
            {
                ShortcutRunResult result = await _shortcutRunner.ApplyShortcutProfilesAsync(startRequest.ShortcutId, 0, cancellationToken, PublishShortcutStatusAsync).ConfigureAwait(false);
                bool wasSuccessful = result.Outcome == ShortcutRunOutcome.Completed;
                OperationPhase terminalPhase = result.Outcome == ShortcutRunOutcome.Cancelled ? OperationPhase.Cancelled : wasSuccessful ? OperationPhase.Completed : OperationPhase.Failed;
                ControlErrorCode errorCode = wasSuccessful ? ControlErrorCode.None : ControlErrorCode.InvalidRequest;
                OperationStatusUpdate terminalStatus = new OperationStatusUpdate
                {
                    OperationId = result.OperationId,
                    OperationType = DisplayOperationType.StartShortcut,
                    Phase = terminalPhase,
                    Message = result.Outcome.ToString(),
                    IsTerminal = true,
                    IsSuccessful = wasSuccessful,
                    ErrorCode = errorCode
                };
                await PublishShortcutStatusAsync(terminalStatus, CancellationToken.None).ConfigureAwait(false);
                return new ControlResponse { IsSuccessful = wasSuccessful, ErrorCode = errorCode, Message = result.Outcome.ToString(), OperationStatus = new OperationStatus { OperationId = result.OperationId, OperationType = DisplayOperationType.StartShortcut, Phase = terminalPhase, IsTerminal = true, IsSuccessful = wasSuccessful, ErrorCode = errorCode } };
            }
            finally
            {
                _registration.IsRecoveryRequired = _shortcutRunner.IsRecoveryRequired;
                _registration.OperationState = AgentOperationState.Idle;
            }
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
            ProfileSummary[] profiles = AudioProfileRepository.AllAudioProfiles.Select(profile => new ProfileSummary { Id = profile.UUID, Name = profile.Name }).ToArray();
            bool canAccessAudioSettings = AudioProfileRepository.CanAccessAudioSettings;
            return new ControlResponse
            {
                IsSuccessful = true,
                Message = "Audio profiles returned.",
                AudioProfileList = new AudioProfileListResult
                {
                    Profiles = profiles,
                    CanAccessAudioSettings = canAccessAudioSettings,
                    Views = AudioProfileRepository.AllAudioProfiles.Select(profile => new AudioProfileView
                    {
                        Id = profile.UUID,
                        Name = profile.Name,
                        SettingsText = profile.GenerateSettingsText(),
                        UnavailableDeviceNames = canAccessAudioSettings ? profile.GetUnavailableAudioDeviceNames().ToArray() : Array.Empty<string>()
                    }).ToArray()
                }
            };
        }

        if (request.MessageType == ControlMessageType.ApplyAudioProfile)
        {
            ApplyAudioProfileRequest? audioApplyRequest = JsonSerializer.Deserialize<ApplyAudioProfileRequest>(request.Payload);
            ApplyAudioProfileOperationResult result = audioApplyRequest == null
                ? new ApplyAudioProfileOperationResult(false, Array.Empty<string>())
                : _userProfileOperationService.ApplyAudioProfile(audioApplyRequest.ProfileId, audioApplyRequest.DeviceWaitMilliseconds);
            return new ControlResponse { IsSuccessful = result.IsSuccessful, ErrorCode = result.IsSuccessful ? ControlErrorCode.None : ControlErrorCode.InvalidRequest, Message = result.IsSuccessful ? "Audio profile applied." : $"Audio profile could not be applied. Missing devices: {string.Join(", ", result.MissingDeviceNames)}" };
        }

        if (request.MessageType == ControlMessageType.CreateAudioProfileFromCurrent)
        {
            CreateProfileRequest? createRequest = JsonSerializer.Deserialize<CreateProfileRequest>(request.Payload);
            if (createRequest == null || !AudioProfileRepository.CanAccessAudioSettings || !AudioProfileRepository.IsValidFilename(createRequest.Name) || AudioProfileRepository.AllAudioProfiles.Any(profile => string.Equals(profile.Name, createRequest.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The audio profile name is invalid, already exists, or audio settings cannot be read." };
            }

            AudioProfileItem profile = new AudioProfileItem { Name = createRequest.Name };
            if (!profile.CreateProfileFromCurrentAudioSettings() || !AudioProfileRepository.AddAudioProfile(profile))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The current audio settings could not be saved." };
            }

            return new ControlResponse { IsSuccessful = true, Message = "Audio profile created." };
        }

        if (request.MessageType == ControlMessageType.RenameAudioProfile)
        {
            RenameProfileRequest? renameRequest = JsonSerializer.Deserialize<RenameProfileRequest>(request.Payload);
            AudioProfileItem? profile = renameRequest == null ? null : AudioProfileRepository.AllAudioProfiles.FirstOrDefault(item => string.Equals(item.UUID, renameRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
            return profile != null && AudioProfileRepository.RenameAudioProfile(profile, renameRequest!.Name)
                ? new ControlResponse { IsSuccessful = true, Message = "Audio profile renamed." }
                : new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The audio profile could not be renamed." };
        }

        if (request.MessageType == ControlMessageType.DeleteAudioProfile)
        {
            DeleteProfileRequest? deleteRequest = JsonSerializer.Deserialize<DeleteProfileRequest>(request.Payload);
            AudioProfileItem? profile = deleteRequest == null ? null : AudioProfileRepository.AllAudioProfiles.FirstOrDefault(item => string.Equals(item.UUID, deleteRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
            return profile != null && AudioProfileRepository.RemoveAudioProfile(profile)
                ? new ControlResponse { IsSuccessful = true, Message = "Audio profile deleted." }
                : new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The audio profile could not be deleted." };
        }

        if (request.MessageType == ControlMessageType.UpdateAudioProfileFromCurrent)
        {
            DeleteProfileRequest? updateRequest = JsonSerializer.Deserialize<DeleteProfileRequest>(request.Payload);
            AudioProfileItem? profile = updateRequest == null ? null : AudioProfileRepository.AllAudioProfiles.FirstOrDefault(item => string.Equals(item.UUID, updateRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
            if (profile == null || !AudioProfileRepository.CanAccessAudioSettings || !profile.CreateProfileFromCurrentAudioSettings() || !AudioProfileRepository.SaveAudioProfiles())
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The current audio settings could not update the audio profile." };
            }

            return new ControlResponse { IsSuccessful = true, Message = "Audio profile updated." };
        }

        if (request.MessageType == ControlMessageType.CreateProfileFromCurrent)
        {
            CreateProfileRequest? createRequest = JsonSerializer.Deserialize<CreateProfileRequest>(request.Payload);
            if (createRequest == null || !ProfileRepository.IsValidFilename(createRequest.Name) || ProfileRepository.AllProfiles.Any(profile => string.Equals(profile.Name, createRequest.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The display profile name is invalid or already in use." };
            }

            ProfileRepository.RefreshDisplayDetectionState();
            ProfileRepository.UpdateActiveProfile();
            ProfileItem? profile = ProfileRepository.CurrentProfile;
            if (profile == null)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The current display configuration could not be captured." };
            }

            profile.Name = createRequest.Name;
            return ProfileRepository.AddProfile(profile)
                ? new ControlResponse { IsSuccessful = true, Message = "Display profile created." }
                : new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The display profile could not be saved." };
        }

        if (request.MessageType == ControlMessageType.RenameProfile)
        {
            RenameProfileRequest? renameRequest = JsonSerializer.Deserialize<RenameProfileRequest>(request.Payload);
            ProfileItem? profile = renameRequest == null ? null : ProfileRepository.AllProfiles.FirstOrDefault(item => string.Equals(item.UUID, renameRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
            if (profile == null || !ProfileRepository.RenameProfile(profile, renameRequest!.Name))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The display profile could not be renamed." };
            }

            return new ControlResponse { IsSuccessful = true, Message = "Display profile renamed." };
        }

        if (request.MessageType == ControlMessageType.DeleteProfile)
        {
            DeleteProfileRequest? deleteRequest = JsonSerializer.Deserialize<DeleteProfileRequest>(request.Payload);
            ProfileItem? profile = deleteRequest == null ? null : ProfileRepository.AllProfiles.FirstOrDefault(item => string.Equals(item.UUID, deleteRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
            return profile != null && ProfileRepository.RemoveProfile(profile)
                ? new ControlResponse { IsSuccessful = true, Message = "Display profile deleted." }
                : new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The display profile could not be deleted." };
        }

        if (request.MessageType == ControlMessageType.UpdateProfileFromCurrent)
        {
            DeleteProfileRequest? updateRequest = JsonSerializer.Deserialize<DeleteProfileRequest>(request.Payload);
            ProfileItem? profile = updateRequest == null ? null : ProfileRepository.AllProfiles.FirstOrDefault(item => string.Equals(item.UUID, updateRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
            if (profile == null)
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The display profile does not exist." };
            }

            ProfileRepository.CopyCurrentLayoutToProfile(profile);
            return ProfileRepository.SaveProfiles()
                ? new ControlResponse { IsSuccessful = true, Message = "Display profile updated." }
                : new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The display profile could not be updated." };
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
        if (applyRequest == null || string.IsNullOrWhiteSpace(applyRequest.ProfileId))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A display profile ID is required." };
        }

        _registration.OperationState = AgentOperationState.Running;
        try
        {
            ApplyDisplayProfileOperationResult result = await _userProfileOperationService.ApplyDisplayProfileAsync(applyRequest.ProfileId, cancellationToken).ConfigureAwait(false);
            return new ControlResponse
            {
                IsSuccessful = result.IsSuccessful,
                ErrorCode = result.IsSuccessful ? ControlErrorCode.None : ControlErrorCode.InvalidRequest,
                Message = result.IsSuccessful ? "Display profile applied." : "Display profile could not be applied.",
                ApplyProfile = new DisplayMagician.Contracts.ApplyProfileResult { WasCancelled = result.WasCancelled }
            };
        }
        finally
        {
            _registration.OperationState = AgentOperationState.Idle;
        }
    }

    private async Task PublishShortcutStatusAsync(OperationStatusUpdate update, CancellationToken cancellationToken)
    {
        try
        {
            await _controlServiceClient.PublishOperationStatusAsync(_registration, update, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException || ex is InvalidOperationException || ex is TimeoutException || ex is OperationCanceledException)
        {
            _logger.Warn(ex, "ProfileCommandHandler/PublishShortcutStatusAsync: Could not publish shortcut operation {0} phase {1}.", update.OperationId, update.Phase);
        }
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
