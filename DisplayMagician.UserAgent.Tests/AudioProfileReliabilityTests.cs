using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.UserAgent;
using DisplayMagician.UserAgent.Runtime;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using WindowsAudioWrapper;
using WindowsAudioWrapper.Models;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

[Collection("Agent repository state")]
public sealed class AudioProfileReliabilityTests
{
    [Fact]
    public void IdenticalSettingsRoundtripComparesEqualAndHasMatchingHash()
    {
        AudioProfileItem original = CreateProfile();
        AudioProfileItem copy = JsonConvert.DeserializeObject<AudioProfileItem>(JsonConvert.SerializeObject(original))!;
        copy.Name = "Another label";
        copy.UUID = Guid.NewGuid().ToString();
        Assert.Equal(original, copy);
        Assert.Equal(original.GetHashCode(), copy.GetHashCode());
        copy.WindowsAudioConfig!.Playback.VolumePercent++;
        Assert.NotEqual(original, copy);
    }

    [Fact]
    public void EndpointEnumerationOrderAndTransientFlagsDoNotChangeEquality()
    {
        AudioProfileItem original = CreateProfile();
        original.WindowsAudioConfig!.EndpointVisibilities = new()
        {
            new() { DeviceId = "speaker", Flow = AudioFlow.Render },
            new() { DeviceId = "microphone", Flow = AudioFlow.Capture }
        };
        AudioProfileItem copy = JsonConvert.DeserializeObject<AudioProfileItem>(JsonConvert.SerializeObject(original))!;
        copy.WindowsAudioConfig!.EndpointVisibilities.Reverse();
        copy.WindowsAudioConfig.Playback.IsVolumeEnabled = !original.WindowsAudioConfig.Playback.IsVolumeEnabled;
        Assert.Equal(original, copy);
        Assert.Equal(original.GetHashCode(), copy.GetHashCode());
        copy.WindowsAudioConfig.EndpointVisibilities[0].IsDisabled = true;
        Assert.NotEqual(original, copy);
    }

    [Theory]
    [InlineData("role")]
    [InlineData("mute")]
    [InlineData("format")]
    [InlineData("delay")]
    public void DifferentSavedSettingsCompareUnequal(string setting)
    {
        AudioProfileItem original = CreateProfile();
        AudioProfileItem copy = JsonConvert.DeserializeObject<AudioProfileItem>(JsonConvert.SerializeObject(original))!;
        switch (setting)
        {
            case "role": copy.WindowsAudioConfig!.Playback.CommunicationsDevice.DeviceId = "other-speaker"; break;
            case "mute": copy.WindowsAudioConfig!.Recording.IsMuted = true; break;
            case "format": copy.WindowsAudioConfig!.Playback.StreamFormat.SampleRate = 96000; break;
            case "delay": copy.ApplyProfileDelay++; break;
        }
        Assert.NotEqual(original, copy);
    }

    [Fact]
    public void EmptyConfigurationsCompareSafely()
    {
        AudioProfileItem first = new();
        AudioProfileItem second = new();
        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.NotEqual(first, CreateProfile());
    }

    [Fact]
    public void PreCancelledApplyDoesNotCreateController()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => CreateProfile().TrySetActive(1000, 0, out _, cancellation.Token,
            () => throw new InvalidOperationException("No controller should be created.")));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CancellationDuringDeviceWaitPreventsApplyAndDisposesController(bool endpointAppeared)
    {
        using CancellationTokenSource cancellation = new();
        IWindowsAudioController controller = CreateController((method, _) =>
        {
            if (method == "WaitForAudioDeviceToAppear") { cancellation.Cancel(); return endpointAppeared; }
            if (method == "ApplyProfile") throw new InvalidOperationException("Cancelled work must not apply audio.");
            return null;
        }, out ControllerProxy proxy);
        Assert.Throws<OperationCanceledException>(() => CreateProfile().TrySetActive(1000, 0, out _, cancellation.Token, () => controller));
        Assert.Equal(0, proxy.ApplyCalls);
        Assert.Equal(1, proxy.DisposeCalls);
    }

    [Fact]
    public void CancellationAfterNativeApplyInterruptsSettleDelayAndDisposesController()
    {
        using CancellationTokenSource cancellation = new();
        IWindowsAudioController controller = CreateController((method, _) =>
        {
            if (method == "WaitForAudioDeviceToAppear") return true;
            if (method == "ApplyProfile") { cancellation.Cancel(); return new AudioProfileApplyResult(); }
            return null;
        }, out ControllerProxy proxy);
        Assert.Throws<OperationCanceledException>(() => CreateProfile().TrySetActive(1000, 60000, out _, cancellation.Token, () => controller));
        Assert.Equal(1, proxy.ApplyCalls);
        Assert.Equal(1, proxy.DisposeCalls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ApplyPreservesWrapperResultAndLogsSpecificMessages(bool successful)
    {
        LoggingConfiguration? previous = LogManager.Configuration;
        MemoryTarget target = new() { Layout = "${level}|${message}" };
        LoggingConfiguration configuration = new();
        configuration.AddRuleForAllLevels(target);
        LogManager.Configuration = configuration;
        try
        {
            IWindowsAudioController controller = CreateController((method, _) => method switch
            {
                "WaitForAudioDeviceToAppear" => true,
                "ApplyProfile" => new AudioProfileApplyResult
                {
                    Successful = successful,
                    Messages = new()
                    {
                        AudioOperationMessage.Warning(AudioMessageCode.UnexpectedError, "format unsupported\r\ncontinuing"),
                        new() { Severity = successful ? AudioMessageSeverity.Info : AudioMessageSeverity.Error,
                            Code = AudioMessageCode.UnexpectedError, DeviceId = "speaker", Message = "specific native detail" }
                    }
                },
                _ => null
            }, out ControllerProxy proxy);
            AudioProfileItem profile = CreateProfile();
            Assert.Equal(successful, profile.TrySetActive(1000, 0, out List<string> missing, CancellationToken.None, () => controller));
            Assert.Empty(missing);
            Assert.Contains(target.Logs, message => message.Contains("Warn|") && message.Contains("format unsupported  continuing") && message.Contains(profile.UUID));
            Assert.Contains(target.Logs, message => message.Contains(successful ? "Info|" : "Error|") && message.Contains("specific native detail") && message.Contains("speaker"));
            Assert.All(target.Logs, message => { Assert.DoesNotContain("\r", message); Assert.DoesNotContain("\n", message); });
            Assert.Equal(1, proxy.DisposeCalls);
        }
        finally { LogManager.Configuration = previous; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingEndpointDoesNotPreventOtherVolumeChanges(bool restore)
    {
        List<(string DeviceId, decimal Volume)> applied = new();
        void SetVolume(string deviceId, decimal volume)
        {
            if (deviceId == "missing") throw new IOException("Endpoint disconnected.");
            applied.Add((deviceId, volume));
        }
        AudioVolumeOverrideState state = new() { Entries = new()
        {
            new("missing", 20, 40), new("microphone", 35, 70), new("second-speaker", 45, 80)
        } };
        Assert.False(AudioVolumeOverrideService.SetVolumes(state.Entries,
            entry => restore ? entry.OriginalVolumePercent : entry.OverrideVolumePercent, restore ? "restore" : "apply", SetVolume));
        Assert.Equal(new[] { ("microphone", restore ? 35m : 70m), ("second-speaker", restore ? 45m : 80m) }, applied);
    }

    [Fact]
    public void InvalidVolumeEntryDoesNotPreventRemainingRestorationAndValuesStayClamped()
    {
        List<decimal> volumes = new();
        Assert.False(AudioVolumeOverrideService.SetVolumes(new[]
        {
            new AudioVolumeOverrideEntry("", 50, 0), new("speaker", -1, 0), new("microphone", 101, 0)
        }, entry => entry.OriginalVolumePercent, "restore", (_, volume) => volumes.Add(volume)));
        Assert.Equal(new[] { 0m, 100m }, volumes);
    }

    [Fact]
    public void SuccessfulProfileRestorationSkipsCoveredVolumesWithoutCreatingNativeController()
    {
        ApplyAudioProfileOperationResult restored = new(true, Array.Empty<string>())
        {
            VolumeTargets = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["speaker"] = 50 }
        };
        AudioVolumeOverrideEntry[] entries = { new("SPEAKER", 20, 80) };
        Assert.Empty(AudioVolumeOverrideService.GetVolumeRestoreEntries(entries, restored));
        Assert.True(new AudioVolumeOverrideService().Restore(entries, restored));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RecoveryUsesOriginalProfileVolumeAndPreservesUnrelatedEndpointVolumes(bool profileRestored)
    {
        ApplyAudioProfileOperationResult result = new(profileRestored, Array.Empty<string>())
        {
            VolumeTargets = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["speaker"] = 50 }
        };
        AudioVolumeOverrideEntry[] entries = { new("speaker", 20, 80), new("other-microphone", 35, 70) };
        List<AudioVolumeOverrideEntry> pending = AudioVolumeOverrideService.GetVolumeRestoreEntries(entries, result);
        Dictionary<string, decimal> finalVolumes = new() { ["speaker"] = 50, ["other-microphone"] = 70 };
        Assert.True(AudioVolumeOverrideService.SetVolumes(pending, entry => entry.OriginalVolumePercent, "restore",
            (id, volume) => finalVolumes[id] = volume));
        Assert.Equal(50m, finalVolumes["speaker"]);
        Assert.Equal(35m, finalVolumes["other-microphone"]);
        Assert.Equal(profileRestored ? 1 : 2, pending.Count);
        Assert.Equal(20m, entries[0].OriginalVolumePercent); // Recovery input remains unchanged.
    }

    [Fact]
    public void VolumeOnlyAndLegacyRecoveryRetainCapturedEndpointVolumes()
    {
        AudioVolumeOverrideEntry[] entries = { new("speaker", 50, 80), new("microphone", 35, 70) };
        Assert.Equal(entries, AudioVolumeOverrideService.GetVolumeRestoreEntries(entries, null));
        Assert.Equal(entries, AudioVolumeOverrideService.GetVolumeRestoreEntries(entries, new(true, Array.Empty<string>())));
    }

    [Fact]
    public void ProfileVolumeTargetsUseOnlyMultimediaVolumesAndIgnoreTransientFlags()
    {
        AudioProfileItem original = CreateProfile();
        original.WindowsAudioConfig!.Playback.ConsoleDevice.DeviceId = "console-only";
        original.WindowsAudioConfig.Playback.CommunicationsDevice.DeviceId = "communications-only";
        IReadOnlyDictionary<string, decimal> targets = AudioVolumeOverrideService.GetProfileVolumeTargets(original.WindowsAudioConfig);
        Assert.Equal(2, targets.Count);
        Assert.Equal(55m, targets["SPEAKER"]);
        Assert.Equal(40m, targets["microphone"]);
        Assert.Empty(AudioVolumeOverrideService.GetProfileVolumeTargets(null));
    }

    [Fact]
    public async Task AudioApplyWaitsForDisplayAccessAndCancelsWithoutCreatingController()
    {
        using CancellationTokenSource cancellation = new();
        using ManualResetEventSlim entered = new();
        using ManualResetEventSlim release = new();
        Task display = Task.Run(() =>
        {
            using IDisposable access = DisplayHardwareAccess.Enter();
            entered.Set();
            Assert.True(release.Wait(TimeSpan.FromSeconds(10)));
        });
        int controllers = 0;
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
            Task apply = Task.Run(() => CreateProfile().TrySetActive(1000, 0, out _, cancellation.Token, () =>
            {
                Interlocked.Increment(ref controllers);
                throw new InvalidOperationException("A queued/cancelled apply must not create a controller.");
            }));
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => apply.WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Equal(0, controllers);
        }
        finally { release.Set(); await display; }
    }

    [Fact]
    public void AudioNativeCallsShareTheDisplayGateAndReleaseItAfterFailure()
    {
        IWindowsAudioController controller = CreateController((method, _) =>
        {
            Assert.True(Monitor.IsEntered(DisplayHardwareAccess.SyncRoot));
            if (method == "WaitForAudioDeviceToAppear") return true;
            if (method == "ApplyProfile") throw new IOException("Synthetic native failure.");
            return null;
        }, out ControllerProxy proxy);
        Assert.False(CreateProfile().TrySetActive(1000, 0, out _, CancellationToken.None, () => controller));
        Assert.Equal(1, proxy.DisposeCalls);
        Assert.False(Monitor.IsEntered(DisplayHardwareAccess.SyncRoot));
    }

    private static AudioProfileItem CreateProfile() => new()
    {
        Name = "Synthetic audio", WindowsAudioConfig = new AudioProfile
        {
            Playback = new() { MultimediaDevice = new() { DeviceId = "speaker" }, VolumePercent = 55 },
            Recording = new() { MultimediaDevice = new() { DeviceId = "microphone" }, VolumePercent = 40 }
        }
    };

    private static IWindowsAudioController CreateController(Func<string, object?[]?, object?> invoke, out ControllerProxy proxy)
    {
        IWindowsAudioController controller = DispatchProxy.Create<IWindowsAudioController, ControllerProxy>();
        proxy = (ControllerProxy)controller;
        proxy.Callback = invoke;
        return controller;
    }

    public class ControllerProxy : DispatchProxy
    {
        public Func<string, object?[]?, object?> Callback { get; set; } = null!;
        public int ApplyCalls { get; private set; }
        public int DisposeCalls { get; private set; }
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            string method = targetMethod!.Name;
            if (method == "Dispose") DisposeCalls++;
            if (method == "ApplyProfile") ApplyCalls++;
            return Callback(method, args);
        }
    }
}
