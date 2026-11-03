using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent.Runtime;
using DisplayMagician.UserAgent.Runtime.NVIDIA;
using NVAPIWrapper;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

[Collection("Agent repository state")]
public sealed class NvidiaConfigurationReviewTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectedOrThrowingSurroundCreationIsFatal(bool throws)
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        bool result = NVIDIALibrary.ApplyMosaicConfiguration(Mosaic(true), Mosaic(false),
            _ => throws ? throw new Win32Exception(5) : false, () => true, () => true, _ => { }, 10, diagnostics);
        Assert.False(result);
        Assert.False(diagnostics.Result.IsSuccessful);
        Assert.Contains(diagnostics.Result.Issues, issue => issue.Severity == DisplayConfigurationSeverity.Fatal);
        if (throws) Assert.Contains(diagnostics.Result.Issues, issue => issue.NativeErrorCode == "5");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreparationFailureDoesNotPreventSuccessfulCreation(bool throws)
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        int creations = 0;
        List<int> waits = new();
        Assert.True(NVIDIALibrary.ApplyMosaicConfiguration(Mosaic(true), Mosaic(false),
            _ => { creations++; return true; }, () => throws ? throw new IOException("Preparation failed") : false,
            () => throw new InvalidOperationException("Required disable should not run"), waits.Add, 10, diagnostics));
        Assert.Equal(1, creations);
        Assert.Equal(new[] { 30 }, waits);
        Assert.True(diagnostics.Result.IsSuccessful);
    }

    [Fact]
    public void LiveMatchSkipsUnnecessaryTopologyWritesAndPreparation()
    {
        NVIDIA_MOSAIC_CONFIG desired = Mosaic(true);
        Assert.True(NVIDIALibrary.ApplyMosaicConfiguration(desired, desired,
            _ => throw new InvalidOperationException("Unnecessary creation"), () => throw new InvalidOperationException("Unnecessary preparation"),
            () => throw new InvalidOperationException("Unnecessary removal"), _ => { }, 0, new DisplayConfigurationDiagnostics()));
    }

    [Fact]
    public void MissingLiveGridsCannotJustifySkippingRequestedCreation()
    {
        NVIDIA_MOSAIC_CONFIG desired = Mosaic(true);
        NVIDIA_MOSAIC_CONFIG cached = desired;
        cached.MosaicGridTopologies = new NVAPIMosaicGridTopologiesDto();
        int creations = 0;
        Assert.True(NVIDIALibrary.ApplyMosaicConfiguration(desired, cached,
            _ => { creations++; return true; }, () => true, () => true, _ => { }, 0, new DisplayConfigurationDiagnostics()));
        Assert.Equal(1, creations);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LiveAggregateFlagControlsRequiredRemoval(bool enabled)
    {
        int removals = 0;
        Assert.True(NVIDIALibrary.ApplyMosaicConfiguration(Mosaic(false), Mosaic(enabled),
            _ => true, () => true, () => { removals++; return true; }, _ => { }, 0, new DisplayConfigurationDiagnostics()));
        Assert.Equal(enabled ? 1 : 0, removals);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectedOrThrowingRequiredRemovalIsFatal(bool throws)
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(NVIDIALibrary.ApplyMosaicConfiguration(Mosaic(false), Mosaic(true),
            _ => true, () => true, () => throws ? throw new Win32Exception(5) : false, _ => { }, 0, diagnostics));
        Assert.False(diagnostics.Result.IsSuccessful);
        if (throws) Assert.Contains(diagnostics.Result.Issues, issue => issue.NativeErrorCode == "5");
    }

    [Theory]
    [InlineData(true, false, false, false, true, 0)]
    [InlineData(false, false, false, false, true, 0)]
    [InlineData(true, true, false, true, false, 1)]
    [InlineData(false, null, false, null, false, 1)]
    [InlineData(null, null, true, null, true, 1)]
    [InlineData(true, true, true, false, true, 1)]
    [InlineData(true, true, true, true, false, 1)]
    [InlineData(null, false, false, false, true, 0)]
    [InlineData(true, null, false, null, true, 0)]
    public void DisableKeepsBothTechniquesAndDoesNotTreatUnavailableReadbackAsSuccess(bool? primary, bool? initialState,
        bool legacyResult, bool? finalState, bool expectedResult, int expectedLegacyCalls)
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        int reads = 0;
        int legacyCalls = 0;
        List<int> waits = new();
        bool result = NVIDIALibrary.DisableMosaicWithFallback(() => primary, () => reads++ == 0 ? initialState : finalState,
            () => { legacyCalls++; return legacyResult; }, waits.Add, 10, diagnostics);
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedLegacyCalls, legacyCalls);
        Assert.Equal(expectedResult, diagnostics.Result.IsSuccessful);
        Assert.All(waits, wait => Assert.Equal(30, wait));
        Assert.Equal((primary.HasValue ? 1 : 0) + expectedLegacyCalls, waits.Count);
    }

    [Fact]
    public void ThrowingPrimaryDisableStillUsesLegacyFallback()
    {
        int reads = 0;
        int legacyCalls = 0;
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.True(NVIDIALibrary.DisableMosaicWithFallback(() => throw new IOException("Primary failed"),
            () => reads++ == 0, () => { legacyCalls++; return true; }, _ => { }, 0, diagnostics));
        Assert.Equal(1, legacyCalls);
        Assert.True(diagnostics.Result.IsSuccessful);
    }

    [Fact]
    public void ThrowingLegacyDisableRetainsNativeErrorAndStops()
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(NVIDIALibrary.DisableMosaicWithFallback(() => false, () => true,
            () => throw new Win32Exception(5), _ => { }, 0, diagnostics));
        Assert.Contains(diagnostics.Result.Issues, issue => issue.NativeErrorCode == "5" && issue.Severity == DisplayConfigurationSeverity.Fatal);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedDrsCaptureIsNotAValidEmptySnapshot(bool throws)
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(NVIDIALibrary.TryCaptureDrsSnapshot(new NVAPIDrsProfileDto { NumOfSettings = 2 },
            () => throws ? throw new IOException("Enumeration failed") : Array.Empty<NVAPIDrsSettingDto>(), diagnostics, out NVIDIA_DRS_CONFIG captured));
        Assert.False(captured.IsBaseProfile);
        Assert.True(diagnostics.Result.HasWarnings);
        Assert.True(diagnostics.Result.IsSuccessful);
        NVIDIALibrary.ApplyDrsSnapshot(captured, new[] { Setting(1, 7) },
            _ => throw new InvalidOperationException("Write after capture failure"),
            _ => throw new InvalidOperationException("Reset after capture failure"),
            () => throw new InvalidOperationException("Save after capture failure"), diagnostics);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void CompletedDrsCapturePreservesLegitimateEmptyOrNonemptyData(int count)
    {
        NVAPIDrsSettingDto[] settings = count == 0 ? Array.Empty<NVAPIDrsSettingDto>() : new[] { Setting(1, 8) };
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.True(NVIDIALibrary.TryCaptureDrsSnapshot(new NVAPIDrsProfileDto { NumOfSettings = (uint)count },
            () => settings, diagnostics, out NVIDIA_DRS_CONFIG captured));
        Assert.True(captured.IsBaseProfile);
        Assert.Equal(settings, captured.DriverSettings);
        Assert.False(diagnostics.Result.HasWarnings);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClearlyIncompleteSavedDrsNeverWritesResetsOrSaves(bool missingCollection)
    {
        NVIDIA_DRS_CONFIG stored = Snapshot();
        stored.ProfileInfo = new NVAPIDrsProfileDto { NumOfSettings = 2 };
        if (missingCollection) stored.DriverSettings = null!;
        DisplayConfigurationDiagnostics diagnostics = new();
        NVIDIALibrary.ApplyDrsSnapshot(stored, new[] { Setting(1, 7) },
            _ => throw new InvalidOperationException("Unexpected write"), _ => throw new InvalidOperationException("Unexpected reset"),
            () => throw new InvalidOperationException("Unexpected save"), diagnostics);
        Assert.True(diagnostics.Result.HasWarnings);
        Assert.True(diagnostics.Result.IsSuccessful);
    }

    [Fact]
    public void CompleteSnapshotStillRestoresAbsentSettingsToDefaultAndSavesOnce()
    {
        List<uint> resets = new();
        int saves = 0;
        NVIDIALibrary.ApplyDrsSnapshot(Snapshot(Setting(1, 7)), new[] { Setting(1, 7), Setting(2, 9) },
            _ => throw new InvalidOperationException("Matching value should not be set"), id => { resets.Add(id); return true; },
            () => { saves++; return true; }, new DisplayConfigurationDiagnostics());
        Assert.Equal(new uint[] { 2 }, resets);
        Assert.Equal(1, saves);
    }

    [Fact]
    public void LegitimateEmptySnapshotRetainsTheExistingResetSemantics()
    {
        int resets = 0;
        int saves = 0;
        NVIDIALibrary.ApplyDrsSnapshot(Snapshot(), new[] { Setting(1, 1) }, _ => true,
            _ => { resets++; return true; }, () => { saves++; return true; }, new DisplayConfigurationDiagnostics());
        Assert.Equal(1, resets);
        Assert.Equal(1, saves);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectedOrThrowingDrsWritesDoNotClaimChangesOrSave(bool throws)
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        NVIDIALibrary.ApplyDrsSnapshot(Snapshot(Setting(1, 1)), new[] { Setting(1, 0) },
            _ => throws ? throw new IOException("Set failed") : false, _ => true,
            () => throw new InvalidOperationException("Nothing was changed"), diagnostics);
        Assert.True(diagnostics.Result.HasWarnings);
        Assert.True(diagnostics.Result.IsSuccessful);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectedOrThrowingDrsSavesAreWarnings(bool throws)
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        int saves = 0;
        NVIDIALibrary.ApplyDrsSnapshot(Snapshot(Setting(1, 1)), new[] { Setting(1, 0) }, _ => true, _ => true,
            () => { saves++; return throws ? throw new IOException("Save failed") : false; }, diagnostics);
        Assert.Equal(1, saves);
        Assert.Contains(diagnostics.Result.Issues, issue => issue.Stage == "DriverSettings");
        Assert.True(diagnostics.Result.IsSuccessful);
    }

    [Fact]
    public void PartialDrsFailureStillAppliesRemainingSettingsAndSavesSuccessfulChanges()
    {
        List<uint> writes = new();
        List<uint> resets = new();
        int saves = 0;
        DisplayConfigurationDiagnostics diagnostics = new();
        NVIDIALibrary.ApplyDrsSnapshot(Snapshot(Setting(1, 1), Setting(2, 2)), new[] { Setting(1, 0), Setting(2, 0), Setting(3, 3) },
            setting => { writes.Add(setting.SettingId); return setting.SettingId != 1; },
            id => { resets.Add(id); return false; }, () => { saves++; return true; }, diagnostics);
        Assert.Equal(new uint[] { 1, 2 }, writes);
        Assert.Equal(new uint[] { 3 }, resets);
        Assert.Equal(1, saves);
        Assert.Equal(2, diagnostics.Result.Issues.Length);
    }

    [Fact]
    public void FreshEnumValuesAreUsedRatherThanAssumingAStoredValueAlreadyMatches()
    {
        int writes = 0;
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.True(NVIDIALibrary.TryCaptureDrsSnapshot(new NVAPIDrsProfileDto { NumOfSettings = 1 },
            () => new[] { Setting(1, 0) }, diagnostics, out NVIDIA_DRS_CONFIG live));
        NVIDIALibrary.ApplyDrsSnapshot(Snapshot(Setting(1, 1)), live.DriverSettings,
            _ => { writes++; return true; }, _ => true, () => true, diagnostics);
        Assert.Equal(1, writes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedOverrideRefreshSkipsStaleComparisonsAndDowngradesCaptureErrors(bool throws)
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        bool success = NVIDIALibrary.TryRefreshOverrideState(capture =>
        {
            if (throws) throw new IOException("Refresh failed");
            capture.Add("Capture", "NVIDIA", "Cannot read current settings", DisplayConfigurationSeverity.Fatal, nativeErrorCode: "5");
            return false;
        }, diagnostics);
        Assert.False(success);
        Assert.True(diagnostics.Result.HasWarnings);
        Assert.True(diagnostics.Result.IsSuccessful);
        Assert.All(diagnostics.Result.Issues, issue => Assert.Equal(DisplayConfigurationSeverity.Warning, issue.Severity));
        if (!throws) Assert.Contains(diagnostics.Result.Issues, issue => issue.NativeErrorCode == "5");
    }

    [Fact]
    public void SuccessfulRefreshRetainsOptionalCaptureWarningsAndAllowsOverrides()
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.True(NVIDIALibrary.TryRefreshOverrideState(capture =>
        {
            capture.Add("DriverSettings", "NVIDIA", "Optional DRS read unavailable");
            return true;
        }, diagnostics));
        Assert.Single(diagnostics.Result.Issues);
        Assert.True(diagnostics.Result.IsSuccessful);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void StringAndBinaryValuesOnlyWriteWhenDifferent(bool binary, bool equal)
    {
        NVAPIDrsSettingDto desired = Setting(1, 0);
        NVAPIDrsSettingDto current = Setting(1, 0);
        desired.SettingType = current.SettingType = binary ? _NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE : _NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE;
        desired.CurrentStringValue = "saved";
        current.CurrentStringValue = equal ? "saved" : "different";
        desired.CurrentBinaryValue = new byte[] { 1, 2 };
        current.CurrentBinaryValue = new byte[] { 1, equal ? (byte)2 : (byte)3 };
        int writes = 0;
        int saves = 0;
        NVIDIALibrary.ApplyDrsSnapshot(Snapshot(desired), new[] { current },
            _ => { writes++; return true; }, _ => throw new InvalidOperationException("Unexpected reset"),
            () => { saves++; return true; }, new DisplayConfigurationDiagnostics());
        Assert.Equal(equal ? 0 : 1, writes);
        Assert.Equal(writes, saves);
    }

    [Fact]
    public void ThrowingDefaultRestoreDoesNotPreventRemainingResetsOrSaving()
    {
        List<uint> resets = new();
        int saves = 0;
        DisplayConfigurationDiagnostics diagnostics = new();
        NVIDIALibrary.ApplyDrsSnapshot(Snapshot(), new[] { Setting(1, 1), Setting(2, 2) },
            _ => throw new InvalidOperationException("Unexpected write"), id =>
            {
                resets.Add(id);
                if (id == 1) throw new Win32Exception(5);
                return true;
            }, () => { saves++; return true; }, diagnostics);
        Assert.Equal(new uint[] { 1, 2 }, resets);
        Assert.Equal(1, saves);
        Assert.Equal("5", Assert.Single(diagnostics.Result.Issues).NativeErrorCode);
        Assert.True(diagnostics.Result.IsSuccessful);
    }

    private static NVAPIDrsSettingDto Setting(uint id, uint value) => new()
    {
        SettingId = id, SettingName = "Setting " + id, SettingType = _NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE, CurrentDwordValue = value
    };

    private static NVIDIA_DRS_CONFIG Snapshot(params NVAPIDrsSettingDto[] settings) => new()
    {
        IsBaseProfile = true, ProfileInfo = new NVAPIDrsProfileDto { NumOfSettings = (uint)settings.Length }, DriverSettings = settings.ToList()
    };

    private static NVIDIA_MOSAIC_CONFIG Mosaic(bool enabled) => new()
    {
        IsMosaicEnabled = enabled,
        MosaicGridTopologies = new NVAPIMosaicGridTopologiesDto(new[] { new NVAPIMosaicGridTopoDto(1, 1,
            false, false, false, false, false, false, Array.Empty<NVAPIMosaicGridTopoDisplayDto>(), new NVAPIMosaicDisplaySettingDto()) })
    };
}
