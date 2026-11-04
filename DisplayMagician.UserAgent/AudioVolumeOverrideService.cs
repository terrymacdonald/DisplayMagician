using System;
using System.Collections.Generic;
using DisplayMagician.ConfigurationDefinitions;
using NLog;
using WindowsAudioWrapper;
using WindowsAudioWrapper.Models;

namespace DisplayMagician.UserAgent;

public sealed class AudioVolumeOverrideService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public bool TryCapture(ShortcutDefinition shortcut, out AudioVolumeOverrideState state)
    {
        using IDisposable hardwareAccess = Runtime.DisplayHardwareAccess.Enter();
        state = new AudioVolumeOverrideState();
        try
        {
            using WindowsAudioController controller = new WindowsAudioController();
            if (shortcut.OverrideAudioSpeakerVolume)
            {
                AudioEndpointInfo speaker = controller.GetDefaultPlaybackDevice();
                if (string.IsNullOrWhiteSpace(speaker.DeviceId))
                {
                    return false;
                }

                state.Entries.Add(new AudioVolumeOverrideEntry(speaker.DeviceId, speaker.VolumePercent, Math.Clamp(shortcut.OverrideAudioSpeakerVolumeLevel, 0, 100)));
            }

            if (shortcut.OverrideAudioMicrophoneVolume)
            {
                AudioEndpointInfo microphone = controller.GetDefaultRecordingDevice();
                if (string.IsNullOrWhiteSpace(microphone.DeviceId))
                {
                    return false;
                }

                state.Entries.Add(new AudioVolumeOverrideEntry(microphone.DeviceId, microphone.VolumePercent, Math.Clamp(shortcut.OverrideAudioMicrophoneVolumeLevel, 0, 100)));
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "AudioVolumeOverrideService/TryCapture: Could not capture the active audio endpoint volumes.");
            return false;
        }
    }

    public bool Apply(AudioVolumeOverrideState state)
    {
        return SetVolumes(state.Entries, entry => entry.OverrideVolumePercent, "apply");
    }

    public bool Restore(IEnumerable<AudioVolumeOverrideEntry> entries) => Restore(entries, null);

    internal bool Restore(IEnumerable<AudioVolumeOverrideEntry> entries, ApplyAudioProfileOperationResult? audioRestoreResult)
    {
        List<AudioVolumeOverrideEntry> pending = GetVolumeRestoreEntries(entries, audioRestoreResult);
        return pending.Count == 0 || SetVolumes(pending, entry => entry.OriginalVolumePercent, "restore");
    }

    internal static IReadOnlyDictionary<string, decimal> GetProfileVolumeTargets(AudioProfile? profile)
    {
        Dictionary<string, decimal> targets = new(StringComparer.OrdinalIgnoreCase);
        // ApplyProfile.EnsureDefaults enables volume for each nonempty multimedia endpoint ID.
        string? playbackId = profile?.Playback?.MultimediaDevice?.DeviceId;
        string? recordingId = profile?.Recording?.MultimediaDevice?.DeviceId;
        if (!string.IsNullOrWhiteSpace(playbackId)) targets[playbackId] = profile!.Playback.VolumePercent;
        if (!string.IsNullOrWhiteSpace(recordingId)) targets[recordingId] = profile!.Recording.VolumePercent;
        return targets;
    }

    internal static List<AudioVolumeOverrideEntry> GetVolumeRestoreEntries(IEnumerable<AudioVolumeOverrideEntry> entries,
        ApplyAudioProfileOperationResult? audioRestoreResult)
    {
        List<AudioVolumeOverrideEntry> pending = new();
        foreach (AudioVolumeOverrideEntry entry in entries)
        {
            if (audioRestoreResult != null && !string.IsNullOrEmpty(entry.DeviceId) &&
                audioRestoreResult.VolumeTargets.TryGetValue(entry.DeviceId, out decimal originalVolume))
            {
                if (audioRestoreResult.IsSuccessful) continue; // The original profile already restored this endpoint.
                // A partial profile failure may still have changed volume; never overwrite it with the shortcut-profile value.
                pending.Add(entry with { OriginalVolumePercent = originalVolume });
            }
            else pending.Add(entry); // A different endpoint, or a volume-only shortcut.
        }
        return pending;
    }

    private static bool SetVolumes(IEnumerable<AudioVolumeOverrideEntry> entries, Func<AudioVolumeOverrideEntry, decimal> getVolume, string action)
    {
        using IDisposable hardwareAccess = Runtime.DisplayHardwareAccess.Enter();
        try
        {
            using WindowsAudioController controller = new WindowsAudioController();
            return SetVolumes(entries, getVolume, action, controller.SetVolumePercent);
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "AudioVolumeOverrideService/SetVolumes: Could not {0} audio endpoint volumes.", action);
            return false;
        }
    }

    internal static bool SetVolumes(IEnumerable<AudioVolumeOverrideEntry> entries, Func<AudioVolumeOverrideEntry, decimal> getVolume,
        string action, Action<string, decimal> setVolume)
    {
        bool successful = true;
        foreach (AudioVolumeOverrideEntry entry in entries)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entry.DeviceId))
                    throw new ArgumentException("The audio endpoint ID is missing.");
                setVolume(entry.DeviceId, Math.Clamp(getVolume(entry), 0, 100));
            }
            catch (Exception ex)
            {
                successful = false;
                _logger.Warn(ex, "AudioVolumeOverrideService/SetVolumes: Could not {0} volume for endpoint {1}; continuing with remaining endpoints.", action, entry.DeviceId);
            }
        }

        return successful;
    }
}

public sealed class AudioVolumeOverrideState
{
    public List<AudioVolumeOverrideEntry> Entries { get; set; } = new List<AudioVolumeOverrideEntry>();
}

public sealed record AudioVolumeOverrideEntry(string DeviceId, decimal OriginalVolumePercent, decimal OverrideVolumePercent);