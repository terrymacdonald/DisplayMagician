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
