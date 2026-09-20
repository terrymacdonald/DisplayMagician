using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Contracts;
using DisplayMagicianShared;
using DisplayMagician.GameLibraries;
using SharedApplyProfileResult = DisplayMagicianShared.ApplyProfileResult;

namespace DisplayMagician.UserAgent;

public sealed class ProfileCommandHandler
{
    private readonly AgentRegistration _registration;
    private readonly string _userDataPath;
    private readonly ShortcutStore _shortcutStore;
    private readonly AutomaticGameDetectionRegistry _automaticGameDetectionRegistry;
    private readonly UserProfileOperationService _userProfileOperationService;
    private readonly ShortcutRunner _shortcutRunner;
    private bool _stopRequested;

    public bool StopRequested => _stopRequested;
    public AutomaticGameDetectionRegistry AutomaticGameDetectionRegistry => _automaticGameDetectionRegistry;
    public ShortcutRunner ShortcutRunner => _shortcutRunner;

    public ProfileCommandHandler(AgentRegistration registration)
    {
        _registration = registration ?? throw new ArgumentNullException(nameof(registration));
        _userDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician", "Users", _registration.UserSid);
        ProfileRepository.ConfigureStoragePath(_userDataPath);
        AudioProfileRepository.ConfigureStoragePath(_userDataPath);
        _shortcutStore = new ShortcutStore(_userDataPath);
        _automaticGameDetectionRegistry = new AutomaticGameDetectionRegistry();
        _automaticGameDetectionRegistry.ReplaceAutomaticDetections(_shortcutStore.GetShortcutDefinitions());
        _userProfileOperationService = new UserProfileOperationService();
        _shortcutRunner = new ShortcutRunner(_shortcutStore, _automaticGameDetectionRegistry, _userProfileOperationService);
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
                .Select(game => new GameView { Id = game.Id, Name = game.Name, Library = (int)game.GameLibraryType, ExecutablePath = game.ExePath })
                .ToArray();
            return new ControlResponse { IsSuccessful = true, Message = "Games returned.", GameList = new GameListResult { Games = games } };
        }

        if (request.MessageType == ControlMessageType.StartShortcut)
        {
            StartShortcutRequest? startRequest = JsonSerializer.Deserialize<StartShortcutRequest>(request.Payload);
            if (startRequest == null || string.IsNullOrWhiteSpace(startRequest.ShortcutId))
            {
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A shortcut ID is required." };
            }

            _registration.OperationState = AgentOperationState.Running;
            try
            {
                ShortcutRunResult result = await _shortcutRunner.ApplyShortcutProfilesAsync(startRequest.ShortcutId, 0, cancellationToken).ConfigureAwait(false);
                bool wasSuccessful = result.Outcome == ShortcutRunOutcome.Completed;
                return new ControlResponse { IsSuccessful = wasSuccessful, ErrorCode = wasSuccessful ? ControlErrorCode.None : ControlErrorCode.InvalidRequest, Message = result.Outcome.ToString(), OperationStatus = new OperationStatus { OperationId = result.OperationId, OperationType = DisplayOperationType.StartShortcut, Phase = wasSuccessful ? OperationPhase.Completed : OperationPhase.Failed, IsTerminal = true, IsSuccessful = wasSuccessful } };
            }
            finally
            {
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
            return new ControlResponse { IsSuccessful = true, Message = "Audio profiles returned.", AudioProfileList = new AudioProfileListResult { Profiles = profiles, Views = AudioProfileRepository.AllAudioProfiles.Select(profile => new AudioProfileView { Id = profile.UUID, Name = profile.Name, SettingsText = profile.GenerateSettingsText() }).ToArray() } };
        }

        if (request.MessageType == ControlMessageType.ApplyAudioProfile)
        {
            ApplyAudioProfileRequest? audioApplyRequest = JsonSerializer.Deserialize<ApplyAudioProfileRequest>(request.Payload);
            ApplyAudioProfileOperationResult result = audioApplyRequest == null
                ? new ApplyAudioProfileOperationResult(false, Array.Empty<string>())
                : _userProfileOperationService.ApplyAudioProfile(audioApplyRequest.ProfileId, audioApplyRequest.DeviceWaitMilliseconds);
            return new ControlResponse { IsSuccessful = result.IsSuccessful, ErrorCode = result.IsSuccessful ? ControlErrorCode.None : ControlErrorCode.InvalidRequest, Message = result.IsSuccessful ? "Audio profile applied." : $"Audio profile could not be applied. Missing devices: {string.Join(", ", result.MissingDeviceNames)}" };
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
