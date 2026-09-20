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

    public bool Restore(IEnumerable<AudioVolumeOverrideEntry> entries)
    {
        return SetVolumes(entries, entry => entry.OriginalVolumePercent, "restore");
    }

    private static bool SetVolumes(IEnumerable<AudioVolumeOverrideEntry> entries, Func<AudioVolumeOverrideEntry, decimal> getVolume, string action)
    {
        try
        {
            using WindowsAudioController controller = new WindowsAudioController();
            foreach (AudioVolumeOverrideEntry entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.DeviceId))
                {
                    return false;
                }

                controller.SetVolumePercent(entry.DeviceId, Math.Clamp(getVolume(entry), 0, 100));
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "AudioVolumeOverrideService/SetVolumes: Could not {0} audio endpoint volumes.", action);
            return false;
        }
    }
}

public sealed class AudioVolumeOverrideState
{
    public List<AudioVolumeOverrideEntry> Entries { get; set; } = new List<AudioVolumeOverrideEntry>();
}

public sealed record AudioVolumeOverrideEntry(string DeviceId, decimal OriginalVolumePercent, decimal OverrideVolumePercent);