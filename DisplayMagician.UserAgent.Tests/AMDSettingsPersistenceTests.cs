using DisplayMagician.Contracts;
using DisplayMagician.UserAgent.Runtime;
using DisplayMagician.UserAgent.Runtime.AMD;
using Newtonsoft.Json;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class AMDSettingsPersistenceTests
{
    [Fact]
    public void RecreatedEyefinityHandles_PreserveEqualityAndSavedHandles()
    {
        AMD_SLSMAP_CONFIG first = CreateMap(1);
        AMD_SLSMAP_CONFIG second = CreateMap(2);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.Equal(1, first.SLSMap.SLSMapIndex);
        Assert.Equal(1, first.SLSMap.Grid.SLSGridIndex);
        Assert.Equal(1, first.SLSTargets[0].DisplayTarget.DisplayMapIndex);
        Assert.Equal(2, second.SLSMap.SLSMapIndex);
        Assert.Equal(2, second.NativeModes[0].SLSMapIndex);
        Assert.Equal(2, second.NativeModeOffsets[0].SLSMapIndex);
        Assert.Equal(2, second.BezelModes[0].SLSMapIndex);
        Assert.Equal(2, second.TransientModes[0].SLSMapIndex);
        Assert.Equal(2, second.SLSOffsets[0].SLSMapIndex);
    }

    [Theory]
    [InlineData("grid")]
    [InlineData("target")]
    [InlineData("identity")]
    [InlineData("orientation")]
    [InlineData("resolution")]
    [InlineData("refresh")]
    [InlineData("bezel")]
    [InlineData("adapter")]
    public void RealEyefinityChanges_CompareUnequal(string change)
    {
        AMD_SLSMAP_CONFIG first = CreateMap(1);
        AMD_SLSMAP_CONFIG second = CreateMap(2);
        switch (change)
        {
            case "grid":
                second.SLSMap.Grid.SLSGridColumn++;
                break;
            case "target":
            case "identity":
                ADL_SLS_TARGET target = second.SLSTargets[0];
                if (change == "target") target.SLSGridPositionX++;
                else target.DisplayTarget.DisplayID.DisplayPhysicalIndex++;
                second.SLSTargets[0] = target;
                break;
            case "orientation":
                second.SLSMap.Orientation++;
                break;
            case "resolution":
            case "refresh":
                ADL_SLS_MODE mode = second.NativeModes[0];
                if (change == "resolution") mode.DisplayMode.XRes++;
                else mode.DisplayMode.RefreshRate++;
                second.NativeModes[0] = mode;
                break;
            case "bezel":
                ADL_SLS_OFFSET offset = second.SLSOffsets[0];
                offset.BezelOffsetX++;
                second.SLSOffsets[0] = offset;
                break;
            case "adapter":
                second.SLSMap.AdapterIndex++;
                break;
        }
        Assert.NotEqual(first, second);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void InactiveContrastReadability_DoesNotChangePresetEquality(bool disabled, bool vivid)
    {
        AMD_3DLUT_INFO saved = CreateLut(disabled, vivid);
        AMD_3DLUT_INFO current = saved;
        current.HasDynamicContrast = false;
        current.CurrentDynamicContrastValue = 0;
        current.DynamicContrastRange = default;

        Assert.Equal(saved, current);
        Assert.Equal(saved.GetHashCode(), current.GetHashCode());
    }

    [Fact]
    public void ActiveContrastValuesAndPresetChanges_AreNotIgnored()
    {
        AMD_3DLUT_INFO saved = CreateLut(false, false);
        AMD_3DLUT_INFO current = saved;
        current.CurrentDynamicContrastValue++;
        Assert.NotEqual(saved, current);
        current = saved;
        current.HasDynamicContrast = false;
        Assert.NotEqual(saved, current);
        current = saved;
        current.IsCurrentSCEDisabled = true;
        Assert.NotEqual(saved, current);
    }

    [Fact]
    public void SavedLutDto_PreservesPresetAndContrastValue()
    {
        AMD_3DLUT_INFO saved = CreateLut(false, false);
        Assert.Equal(saved, new AMD_3DLUT_INFO(saved.ToThreeDLUTDto()));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InactiveMapDoesNotInheritAnotherAdaptersActiveState(bool activeFirst)
    {
        AMD_DISPLAY_CONFIG config = new();
        AMD_SLSMAP_CONFIG active = CreateMap(1);
        AMD_SLSMAP_CONFIG inactive = CreateMap(2);
        ADL_MODE mode = new() { XRes = 5120, YRes = 1440 };
        if (activeFirst) AMDLibrary.RecordActiveSlsMap(ref config, active, new[] { mode });
        AMDLibrary.RecordActiveSlsMap(ref config, inactive, System.Array.Empty<ADL_MODE>());
        if (!activeFirst) AMDLibrary.RecordActiveSlsMap(ref config, active, new[] { mode });
        AMD_DISPLAY_CONFIG restored = JsonConvert.DeserializeObject<AMD_DISPLAY_CONFIG>(JsonConvert.SerializeObject(config));
        Assert.True(restored.IsEyefinity);
        Assert.True(restored.Adl2SlsConfig.IsSlsEnabled);
        Assert.Equal(1, Assert.Single(restored.Adl2SlsConfig.SLSMapConfigs).SLSMap.SLSMapIndex);
        Assert.Equal(mode, Assert.Single(restored.Adl2SlsConfig.SLSEnabledDisplayTargets));
    }

    [Fact]
    public void MultipleActiveMapsKeepAggregateStateAndDistinctTargets()
    {
        AMD_DISPLAY_CONFIG config = new();
        ADL_MODE first = new() { XRes = 5120, YRes = 1440 };
        ADL_MODE second = new() { XRes = 7680, YRes = 1440 };
        AMDLibrary.RecordActiveSlsMap(ref config, CreateMap(1), new[] { first });
        AMDLibrary.RecordActiveSlsMap(ref config, CreateMap(2), new[] { first, second });
        Assert.True(config.IsEyefinity);
        Assert.True(config.Adl2SlsConfig.IsSlsEnabled);
        Assert.Equal(2, config.Adl2SlsConfig.SLSMapConfigs.Count);
        Assert.Equal(new[] { first, second }, config.Adl2SlsConfig.SLSEnabledDisplayTargets);
        Assert.True(AMDLibrary.HasRequiredSlsMaps(config.Adl2SlsConfig, new DisplayConfigurationDiagnostics()));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MissingRequiredLegacyMapsIsFatalEvenWhenAggregateFlagIsSet(bool missing)
    {
        AMD_SLS_CONFIG config = new() { IsSlsEnabled = true };
        if (missing) config.SLSMapConfigs = null!;
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(AMDLibrary.HasRequiredSlsMaps(config, diagnostics));
        Assert.Equal(DisplayConfigurationSeverity.Fatal, Assert.Single(diagnostics.Result.Issues).Severity);
    }

    [Fact]
    public void InactiveMapLeavesEmptyAggregateDisabled()
    {
        AMD_DISPLAY_CONFIG config = new();
        AMDLibrary.RecordActiveSlsMap(ref config, CreateMap(1), System.Array.Empty<ADL_MODE>());
        Assert.False(config.IsEyefinity);
        Assert.False(config.Adl2SlsConfig.IsSlsEnabled);
        Assert.Empty(config.Adl2SlsConfig.SLSMapConfigs);
        Assert.Empty(config.Adl2SlsConfig.SLSEnabledDisplayTargets);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void SavedGammaPresetsSurviveJsonAndWrapperConversion(int preset)
    {
        AMD_GAMMA_INFO saved = new()
        {
            IsSupported = true,
            IsCurrentReGammaSRGB = preset == 0,
            IsCurrentReGammaBT709 = preset == 1,
            IsCurrentReGammaPQ = preset == 2,
            IsCurrentReGammaPQ2084 = preset == 3,
            IsCurrentReGamma36 = preset == 4
        };
        AMD_GAMMA_INFO restored = JsonConvert.DeserializeObject<AMD_GAMMA_INFO>(JsonConvert.SerializeObject(saved));
        Assert.Equal(saved, new AMD_GAMMA_INFO(restored.ToGammaDto()));
    }

    [Fact]
    public void SavedGammaRampSurvivesJsonAndWrapperConversion()
    {
        AMD_GAMMA_INFO saved = new()
        {
            IsSupported = true, HasReGammaRamp = true,
            ReGammaRamp = new AMD_GAMMA_RAMP(new System.Collections.Generic.List<ushort> { 0, 1000, 32768, 65535 })
        };
        AMD_GAMMA_INFO restored = JsonConvert.DeserializeObject<AMD_GAMMA_INFO>(JsonConvert.SerializeObject(saved));
        Assert.Equal(saved, new AMD_GAMMA_INFO(restored.ToGammaDto()));
    }

    [Fact]
    public void SavedGamutSurvivesJsonAndWrapperConversion()
    {
        ADLXWrapper.GamutDto captured = new()
        {
            IsWhitePointSupported = true, IsGamutSupported = true,
            IsCurrent6500K = true, IsCurrentAdobe = true,
            HasWhitePoint = true, WhitePoint = new ADLXWrapper.PointDto(3127, 3290)
        };
        AMD_GAMUT_INFO saved = new(captured);
        AMD_GAMUT_INFO restored = JsonConvert.DeserializeObject<AMD_GAMUT_INFO>(JsonConvert.SerializeObject(saved));
        Assert.Equal(saved, new AMD_GAMUT_INFO(restored.ToGamutDto()));
    }

    private static AMD_3DLUT_INFO CreateLut(bool disabled, bool vivid) =>
        new AMD_3DLUT_INFO(true, true, true, false, disabled, vivid, true, 5,
            new IntRange { Min = 0, Max = 10, Step = 1 });

    private static AMD_SLSMAP_CONFIG CreateMap(int index)
    {
        AMD_SLSMAP_CONFIG map = new AMD_SLSMAP_CONFIG();
        map.SLSMap.SLSMapIndex = index;
        map.SLSMap.Grid.SLSGridIndex = index;
        map.SLSMap.Grid.SLSGridRow = 1;
        map.SLSMap.Grid.SLSGridColumn = 2;
        map.SLSTargets.Add(new ADL_SLS_TARGET
        {
            SLSMapIndex = index,
            DisplayTarget = new ADL_DISPLAY_TARGET { DisplayMapIndex = index }
        });
        map.NativeModes.Add(new ADL_SLS_MODE
        {
            SLSMapIndex = index,
            DisplayMode = new ADL_MODE { XRes = 5120, YRes = 1440, RefreshRate = 60 }
        });
        map.NativeModeOffsets.Add(new ADL_SLS_OFFSET { SLSMapIndex = index });
        map.BezelModes.Add(new ADL_BEZEL_TRANSIENT_MODE { SLSMapIndex = index });
        map.TransientModes.Add(new ADL_BEZEL_TRANSIENT_MODE { SLSMapIndex = index });
        map.SLSOffsets.Add(new ADL_SLS_OFFSET { SLSMapIndex = index });
        return JsonConvert.DeserializeObject<AMD_SLSMAP_CONFIG>(JsonConvert.SerializeObject(map));
    }
}
