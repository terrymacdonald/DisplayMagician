using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent.Runtime;
using SharedApplyProfileResult = DisplayMagician.UserAgent.Runtime.ApplyProfileResult;

namespace DisplayMagician.UserAgent;

/// <summary>Runs the User Agent's existing display and audio profile operations for commands and shortcuts.</summary>
public class UserProfileOperationService
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    internal static readonly Newtonsoft.Json.JsonSerializerSettings RecoveryJsonSettings = new Newtonsoft.Json.JsonSerializerSettings
    {
        ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace
    };

    public UserProfileOperationState CaptureCurrentProfileState() => CaptureCurrentProfileState(false, false);

    internal virtual UserProfileOperationState CaptureCurrentProfileState(bool captureDisplaySnapshot, bool captureAudioSnapshot)
    {
        // External Windows settings changes do not invalidate the repository caches.
        // Refresh before deciding whether a shortcut can skip applying its profiles.
        ProfileRepository.UpdateActiveProfile();
        AudioProfileRepository.UpdateActiveAudioProfile();
        string displayId = ProfileRepository.CurrentProfile?.UUID ?? string.Empty;
        string audioId = AudioProfileRepository.CurrentAudioProfile?.UUID ?? string.Empty;
        string displayJson = string.Empty;
        string audioJson = string.Empty;
        if (captureDisplaySnapshot)
        {
            ProfileItem display = new ProfileItem();
            if (!display.CreateProfileFromCurrentDisplaySettings())
                throw new InvalidOperationException("The original display configuration could not be captured for recovery.");
            displayJson = SerializeDisplayRecoveryProfile(display);
        }
        if (captureAudioSnapshot)
        {
            AudioProfileItem audio = new AudioProfileItem();
            if (!audio.CreateProfileFromCurrentAudioSettings())
                throw new InvalidOperationException("The original audio configuration could not be captured for recovery.");
            audioJson = Newtonsoft.Json.JsonConvert.SerializeObject(audio);
        }
        return new UserProfileOperationState(displayId, audioId)
        {
            DisplayProfileSnapshotJson = displayJson, AudioProfileSnapshotJson = audioJson
        };
    }

    // Serialize only restorable configuration, avoiding bitmap/icon getters and cache files.
    internal static string SerializeDisplayRecoveryProfile(ProfileItem profile) => Newtonsoft.Json.JsonConvert.SerializeObject(new
    {
        profile.UUID, profile.Name, profile.NVIDIADisplayConfig, profile.AMDDisplayConfig,
        profile.IntelDisplayConfig, profile.WindowsDisplayConfig, profile.WallpaperConfiguration,
        profile.ProfileDisplayIdentifiers, profile.ApplyProfileCount, profile.ApplyProfileDelay,
        profile.ForceExplorerRestart
    });

    public Task<ApplyDisplayProfileOperationResult> ApplyDisplayProfileAsync(string profileId, CancellationToken cancellationToken)
        => ApplyDisplayProfileAsync(profileId, cancellationToken, null);

    internal virtual async Task<ApplyDisplayProfileOperationResult> ApplyDisplayProfileAsync(string profileId, CancellationToken cancellationToken, string? snapshotJson)
    {
        ProfileItem? profile;
        try
        {
            profile = !string.IsNullOrWhiteSpace(snapshotJson)
                ? Newtonsoft.Json.JsonConvert.DeserializeObject<ProfileItem>(snapshotJson, RecoveryJsonSettings)
                : ProfileRepository.AllProfiles.FirstOrDefault(item => string.Equals(item.UUID, profileId, StringComparison.OrdinalIgnoreCase));
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            Logger.Error(ex, "UserProfileOperationService/ApplyDisplayProfileAsync: Could not read the original display recovery snapshot for profile {0}.", profileId);
            return new ApplyDisplayProfileOperationResult(false, false);
        }
        if (profile == null)
        {
            return new ApplyDisplayProfileOperationResult(false, false);
        }

        SharedApplyProfileResult result = await Task.Run(() => ProfileRepository.ApplyProfile(profile), cancellationToken).ConfigureAwait(false);
        return new ApplyDisplayProfileOperationResult(result == SharedApplyProfileResult.Successful, result == SharedApplyProfileResult.Cancelled);
    }

    public ApplyAudioProfileOperationResult ApplyAudioProfile(string profileId, int deviceWaitMilliseconds)
        => ApplyAudioProfile(profileId, deviceWaitMilliseconds, null);

    internal virtual ApplyAudioProfileOperationResult ApplyAudioProfile(string profileId, int deviceWaitMilliseconds, string? snapshotJson)
    {
        AudioProfileItem? profile;
        try
        {
            profile = !string.IsNullOrWhiteSpace(snapshotJson)
                ? Newtonsoft.Json.JsonConvert.DeserializeObject<AudioProfileItem>(snapshotJson, RecoveryJsonSettings)
                : AudioProfileRepository.AllAudioProfiles.FirstOrDefault(item => string.Equals(item.UUID, profileId, StringComparison.OrdinalIgnoreCase));
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            Logger.Error(ex, "UserProfileOperationService/ApplyAudioProfile: Could not read the original audio recovery snapshot for profile {0}.", profileId);
            return new ApplyAudioProfileOperationResult(false, Array.Empty<string>());
        }
        List<string> missingDeviceNames = new List<string>();
        int effectiveDeviceWaitMilliseconds = deviceWaitMilliseconds > 0 ? deviceWaitMilliseconds : ControlProtocol.DefaultAudioDeviceWaitMilliseconds;
        bool applied = profile != null && profile.TrySetActive(effectiveDeviceWaitMilliseconds, 500, out missingDeviceNames);
        if (applied)
        {
            AudioProfileRepository.UpdateActiveAudioProfile();
        }

        return new ApplyAudioProfileOperationResult(applied, missingDeviceNames);
    }
}

public sealed record ApplyDisplayProfileOperationResult(bool IsSuccessful, bool WasCancelled);
public sealed record ApplyAudioProfileOperationResult(bool IsSuccessful, IReadOnlyList<string> MissingDeviceNames);
public sealed record UserProfileOperationState(string DisplayProfileId, string AudioProfileId)
{
    public string DisplayProfileSnapshotJson { get; init; } = string.Empty;
    public string AudioProfileSnapshotJson { get; init; } = string.Empty;
}
