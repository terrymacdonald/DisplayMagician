using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.UserAgent.Runtime;
using SharedApplyProfileResult = DisplayMagician.UserAgent.Runtime.ApplyProfileResult;

namespace DisplayMagician.UserAgent;

/// <summary>Runs the User Agent's existing display and audio profile operations for commands and shortcuts.</summary>
public sealed class UserProfileOperationService
{
    public UserProfileOperationState CaptureCurrentProfileState()
    {
        return new UserProfileOperationState(ProfileRepository.CurrentProfile?.UUID ?? string.Empty, AudioProfileRepository.CurrentAudioProfile?.UUID ?? string.Empty);
    }

    public async Task<ApplyDisplayProfileOperationResult> ApplyDisplayProfileAsync(string profileId, CancellationToken cancellationToken)
    {
        ProfileItem? profile = ProfileRepository.AllProfiles.FirstOrDefault(item => string.Equals(item.UUID, profileId, StringComparison.OrdinalIgnoreCase));
        if (profile == null)
        {
            return new ApplyDisplayProfileOperationResult(false, false);
        }

        SharedApplyProfileResult result = await Task.Run(() => ProfileRepository.ApplyProfile(profile), cancellationToken).ConfigureAwait(false);
        return new ApplyDisplayProfileOperationResult(result == SharedApplyProfileResult.Successful, result == SharedApplyProfileResult.Cancelled);
    }

    public ApplyAudioProfileOperationResult ApplyAudioProfile(string profileId, int deviceWaitMilliseconds)
    {
        AudioProfileItem? profile = AudioProfileRepository.AllAudioProfiles.FirstOrDefault(item => string.Equals(item.UUID, profileId, StringComparison.OrdinalIgnoreCase));
        List<string> missingDeviceNames = new List<string>();
        bool applied = profile != null && profile.TrySetActive(Math.Max(0, deviceWaitMilliseconds), 500, out missingDeviceNames);
        if (applied)
        {
            AudioProfileRepository.UpdateActiveAudioProfile();
        }

        return new ApplyAudioProfileOperationResult(applied, missingDeviceNames);
    }
}

public sealed record ApplyDisplayProfileOperationResult(bool IsSuccessful, bool WasCancelled);
public sealed record ApplyAudioProfileOperationResult(bool IsSuccessful, IReadOnlyList<string> MissingDeviceNames);
public sealed record UserProfileOperationState(string DisplayProfileId, string AudioProfileId);
