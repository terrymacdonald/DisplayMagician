using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagicianShared;
using SharedApplyProfileResult = DisplayMagicianShared.ApplyProfileResult;

namespace DisplayMagician.UserAgent;

public sealed class ProfileCommandHandler
{
    private readonly AgentRegistration _registration;
    private bool _stopRequested;

    public bool StopRequested => _stopRequested;

    public ProfileCommandHandler(AgentRegistration registration)
    {
        _registration = registration ?? throw new ArgumentNullException(nameof(registration));
        string userDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician", "Users", _registration.UserSid);
        ProfileRepository.ConfigureStoragePath(userDataPath);
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
                ProfileList = new ProfileListResult { Profiles = profiles }
            };
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

        ProfileItem? profileToApply = ProfileRepository.AllProfiles.FirstOrDefault(profile => string.Equals(profile.UUID, applyRequest.ProfileId, StringComparison.OrdinalIgnoreCase));
        if (profileToApply == null)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The requested display profile does not exist." };
        }

        _registration.OperationState = AgentOperationState.Running;
        try
        {
            SharedApplyProfileResult result = await Task.Run(() => ProfileRepository.ApplyProfile(profileToApply), cancellationToken).ConfigureAwait(false);
            return new ControlResponse
            {
                IsSuccessful = result == SharedApplyProfileResult.Successful,
                ErrorCode = result == SharedApplyProfileResult.Successful ? ControlErrorCode.None : ControlErrorCode.InvalidRequest,
                Message = result == SharedApplyProfileResult.Successful ? "Display profile applied." : "Display profile could not be applied.",
                ApplyProfile = new DisplayMagician.Contracts.ApplyProfileResult { WasCancelled = result == SharedApplyProfileResult.Cancelled }
            };
        }
        finally
        {
            _registration.OperationState = AgentOperationState.Idle;
        }
    }
}
