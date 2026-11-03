using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using DisplayMagician.UserAgent.Runtime;
using DisplayMagician.UserAgent.Runtime.Windows;
using AgentWinLibrary = DisplayMagician.UserAgent.Runtime.Windows.WinLibrary;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class WindowsDisplayInteropReviewTests
{
    [Fact]
    public void TrimmedCapturePreservesNativeDataIndexesAndValidZeroEntriesThroughJson()
    {
        DISPLAYCONFIG_PATH_INFO[] paths = new DISPLAYCONFIG_PATH_INFO[5];
        DISPLAYCONFIG_MODE_INFO[] modes = new DISPLAYCONFIG_MODE_INFO[6];
        paths[0] = Path(1, 10);
        paths[0].SourceInfo.ModeInfoIdx = 1;
        paths[0].TargetInfo.ModeInfoIdx = 2;
        paths[1] = VirtualPath(1, 11, 1);
        paths[1].TargetInfo.ModeInfoIdx = 0x0002ffff;
        // Index zero is deliberately a valid zero-valued entry: it must stay in place.
        modes[1] = new DISPLAYCONFIG_MODE_INFO { InfoType = DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE,
            Id = 1, SourceMode = new DISPLAYCONFIG_SOURCE_MODE { Width = 1920, Height = 1080, Position = new POINTL { X = -1920 } } };
        modes[2] = new DISPLAYCONFIG_MODE_INFO { InfoType = DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET,
            Id = 10, TargetMode = new DISPLAYCONFIG_TARGET_MODE { TargetVideoSignalInfo = new DISPLAYCONFIG_VIDEO_SIGNAL_INFO { PixelRate = 148500000 } } };
        byte[][] expectedPaths = paths.Take(2).Select(NativeBytes).ToArray();
        byte[][] expectedModes = modes.Take(3).Select(NativeBytes).ToArray();

        AgentWinLibrary.TrimDisplayConfigBuffers(ref paths, 2, ref modes, 3);
        WINDOWS_DISPLAY_CONFIG saved = new() { DisplayConfigPaths = paths, DisplayConfigModes = modes };
        WINDOWS_DISPLAY_CONFIG loaded = JsonConvert.DeserializeObject<WINDOWS_DISPLAY_CONFIG>(JsonConvert.SerializeObject(saved));

        Assert.Equal(2, loaded.DisplayConfigPaths.Length);
        Assert.Equal(3, loaded.DisplayConfigModes.Length);
        for (int i = 0; i < loaded.DisplayConfigPaths.Length; i++)
            Assert.Equal(expectedPaths[i], NativeBytes(loaded.DisplayConfigPaths[i]));
        for (int i = 0; i < loaded.DisplayConfigModes.Length; i++)
            Assert.Equal(expectedModes[i], NativeBytes(loaded.DisplayConfigModes[i]));
        Assert.Equal(1u, loaded.DisplayConfigPaths[0].SourceInfo.ModeInfoIdx);
        Assert.Equal(0x0002ffffu, loaded.DisplayConfigPaths[1].TargetInfo.ModeInfoIdx);
        Assert.Equal(DISPLAYCONFIG_MODE_INFO_TYPE.Zero, loaded.DisplayConfigModes[0].InfoType);
    }

    [Fact]
    public void FullReturnedCountsLeaveBothBuffersIntact()
    {
        DISPLAYCONFIG_PATH_INFO[] paths = { Path(1, 10) };
        DISPLAYCONFIG_MODE_INFO[] modes = new DISPLAYCONFIG_MODE_INFO[2];
        DISPLAYCONFIG_PATH_INFO[] originalPaths = paths;
        DISPLAYCONFIG_MODE_INFO[] originalModes = modes;
        AgentWinLibrary.TrimDisplayConfigBuffers(ref paths, 1, ref modes, 2);
        Assert.Same(originalPaths, paths);
        Assert.Same(originalModes, modes);
    }

    [Fact]
    public void ZeroReturnedCountsProduceEmptyArrays()
    {
        DISPLAYCONFIG_PATH_INFO[] paths = new DISPLAYCONFIG_PATH_INFO[3];
        DISPLAYCONFIG_MODE_INFO[] modes = new DISPLAYCONFIG_MODE_INFO[5];
        AgentWinLibrary.TrimDisplayConfigBuffers(ref paths, 0, ref modes, 0);
        Assert.Empty(paths);
        Assert.Empty(modes);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(3, 1)]
    [InlineData(1, -1)]
    [InlineData(1, 4)]
    public void InvalidReturnedCountsFailBeforeChangingEitherBuffer(int pathCount, int modeCount)
    {
        DISPLAYCONFIG_PATH_INFO[] paths = new DISPLAYCONFIG_PATH_INFO[2];
        DISPLAYCONFIG_MODE_INFO[] modes = new DISPLAYCONFIG_MODE_INFO[3];
        DISPLAYCONFIG_PATH_INFO[] originalPaths = paths;
        DISPLAYCONFIG_MODE_INFO[] originalModes = modes;
        Assert.Throws<DisplayMagician.UserAgent.Runtime.Windows.WinLibraryException>(() =>
            AgentWinLibrary.TrimDisplayConfigBuffers(ref paths, pathCount, ref modes, modeCount));
        Assert.Same(originalPaths, paths);
        Assert.Same(originalModes, modes);
    }

    [Fact]
    public void LegacyTopologyFallbackOmitsModesAndPreservesPathsAndCloneSourceIds()
    {
        DISPLAYCONFIG_PATH_INFO[] original = { Path(1, 10), Path(1, 11), Path(2, 12) };
        WIN32STATUS result = AgentWinLibrary.ApplyTopologyOnlyConfiguration(original,
            SDC.SDC_APPLY | SDC.SDC_TOPOLOGY_SUPPLIED | SDC.SDC_ALLOW_CHANGES,
            (count, paths, modeCount, modes, flags) =>
            {
                Assert.Equal(3u, count);
                Assert.Equal(0u, modeCount);
                Assert.Null(modes);
                Assert.Equal(new uint[] { 10, 11, 12 }, paths.Select(path => path.TargetInfo.Id));
                Assert.Equal(new uint[] { 1, 1, 2 }, paths.Select(path => path.SourceInfo.Id));
                Assert.All(paths, path =>
                {
                    Assert.Equal(uint.MaxValue, path.SourceInfo.ModeInfoIdx);
                    Assert.Equal(uint.MaxValue, path.TargetInfo.ModeInfoIdx);
                });
                Assert.True(flags.HasFlag(SDC.SDC_TOPOLOGY_SUPPLIED));
                paths[0].SourceInfo.Id = 99;
                return WIN32STATUS.ERROR_BAD_CONFIGURATION;
            });
        Assert.Equal(WIN32STATUS.ERROR_BAD_CONFIGURATION, result);
        Assert.Equal(1u, original[0].SourceInfo.Id);
        Assert.All(original, path =>
        {
            Assert.Equal(3u, path.SourceInfo.ModeInfoIdx);
            Assert.Equal(7u, path.TargetInfo.ModeInfoIdx);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void VirtualTopologyPreservesCloneMembershipAfterRefreshAwarenessFallback(bool refreshAware)
    {
        DISPLAYCONFIG_PATH_INFO[] original = { VirtualPath(1, 10, 9), VirtualPath(2, 11, 9), VirtualPath(3, 12, 10), VirtualPath(4, 13, 9) };
        original[3].SourceInfo.AdapterId.LowPart = 2;
        uint savedIndex = original[0].SourceInfo.ModeInfoIdx;
        SDC requested = SDC.SDC_APPLY | SDC.SDC_TOPOLOGY_SUPPLIED;
        if (refreshAware) requested |= SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE;
        AgentWinLibrary.ApplyTopologyOnlyConfiguration(original, requested, (_, paths, modeCount, modes, flags) =>
        {
            Assert.Null(modes);
            Assert.Equal(0u, modeCount);
            Assert.True(flags.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE));
            Assert.Equal(refreshAware, flags.HasFlag(SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE));
            Assert.Equal(paths[0].SourceInfo.cloneGroupId, paths[1].SourceInfo.cloneGroupId);
            Assert.NotEqual(paths[0].SourceInfo.cloneGroupId, paths[2].SourceInfo.cloneGroupId);
            Assert.NotEqual(paths[0].SourceInfo.cloneGroupId, paths[3].SourceInfo.cloneGroupId);
            Assert.All(paths, path =>
            {
                Assert.Equal(ushort.MaxValue, path.SourceInfo.sourceModeInfoIdx);
                Assert.NotEqual(ushort.MaxValue, path.SourceInfo.cloneGroupId);
                Assert.Equal(uint.MaxValue, path.TargetInfo.ModeInfoIdx);
            });
            Assert.Equal(new uint[] { 10, 11, 12, 13 }, paths.Select(path => path.TargetInfo.Id));
            return WIN32STATUS.ERROR_SUCCESS;
        });
        Assert.Equal(savedIndex, original[0].SourceInfo.ModeInfoIdx);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void VirtualTopologyUsesExistingGroupsOrSourceIdentityWhenSourceModesAreAbsent(bool hasGroup, bool crossAdapter)
    {
        DISPLAYCONFIG_PATH_INFO[] original = { VirtualPath(1, 10, ushort.MaxValue), VirtualPath(hasGroup ? 2u : 1u, 11, ushort.MaxValue), VirtualPath(3, 12, ushort.MaxValue) };
        original[0].SourceInfo.cloneGroupId = hasGroup ? (ushort)50 : ushort.MaxValue;
        original[1].SourceInfo.cloneGroupId = hasGroup ? (ushort)50 : ushort.MaxValue;
        original[2].SourceInfo.cloneGroupId = hasGroup ? (ushort)51 : ushort.MaxValue;
        if (crossAdapter) original[1].SourceInfo.AdapterId.LowPart = 2;
        AgentWinLibrary.ApplyTopologyOnlyConfiguration(original, SDC.SDC_APPLY | SDC.SDC_TOPOLOGY_SUPPLIED, (_, paths, _, _, _) =>
        {
            Assert.Equal(paths[0].SourceInfo.cloneGroupId, paths[1].SourceInfo.cloneGroupId);
            Assert.NotEqual(paths[0].SourceInfo.cloneGroupId, paths[2].SourceInfo.cloneGroupId);
            return WIN32STATUS.ERROR_SUCCESS;
        });
    }

    [Fact]
    public void MissingVirtualSourceAndGroupIdentityDoesNotInventACloneRelationship()
    {
        DISPLAYCONFIG_PATH_INFO[] original = { VirtualPath(uint.MaxValue, 10, ushort.MaxValue), VirtualPath(uint.MaxValue, 11, ushort.MaxValue) };
        AgentWinLibrary.ApplyTopologyOnlyConfiguration(original, SDC.SDC_APPLY | SDC.SDC_TOPOLOGY_SUPPLIED, (_, paths, _, _, _) =>
        {
            Assert.NotEqual(paths[0].SourceInfo.cloneGroupId, paths[1].SourceInfo.cloneGroupId);
            return WIN32STATUS.ERROR_SUCCESS;
        });
    }

    [Fact]
    public void NativeStructureSizesAndPackedSourceFieldsRemainCompatible()
    {
        Assert.Equal(20, Marshal.SizeOf<DISPLAYCONFIG_PATH_SOURCE_INFO>());
        Assert.Equal(48, Marshal.SizeOf<DISPLAYCONFIG_PATH_TARGET_INFO>());
        Assert.Equal(72, Marshal.SizeOf<DISPLAYCONFIG_PATH_INFO>());
        Assert.Equal(64, Marshal.SizeOf<DISPLAYCONFIG_MODE_INFO>());
        DISPLAYCONFIG_PATH_SOURCE_INFO source = new() { ModeInfoIdx = 0x12340056 };
        Assert.Equal((ushort)0x1234, source.sourceModeInfoIdx);
        Assert.Equal((ushort)0x0056, source.cloneGroupId);
        source.sourceModeInfoIdx = ushort.MaxValue;
        Assert.Equal(0xffff0056u, source.ModeInfoIdx);
    }

    [Fact]
    public void DpiScopeRestoresExactPreviousContextAndDisposeIsIdempotent()
    {
        List<IntPtr> calls = new();
        IntPtr previous = new IntPtr(0x123456789);
        ThreadDpiAwarenessScope scope = new(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2, context =>
        {
            calls.Add(context);
            return previous;
        });
        scope.Dispose();
        scope.Dispose();
        Assert.Equal(new[] { new IntPtr(-4), previous }, calls);
    }

    [Fact]
    public void NestedDpiScopesRestoreTheCallerContextEvenWhenCaptureThrows()
    {
        IntPtr current = new IntPtr(-5);
        IntPtr SetContext(IntPtr value) { IntPtr previous = current; current = value; return previous; }
        Assert.Throws<InvalidOperationException>(new Action(() =>
        {
            using ThreadDpiAwarenessScope outer = new(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2, SetContext);
            using (new ThreadDpiAwarenessScope(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_SYSTEM_AWARE, SetContext))
                Assert.Equal(new IntPtr(-2), current);
            Assert.Equal(new IntPtr(-4), current);
            throw new InvalidOperationException("Simulated capture failure.");
        }));
        Assert.Equal(new IntPtr(-5), current);
    }

    [Fact]
    public void FailedDpiScopeEntryDoesNotTryToRestoreAnUnknownContext()
    {
        int calls = 0;
        using (new ThreadDpiAwarenessScope(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2, _ => { calls++; return IntPtr.Zero; })) { }
        Assert.Equal(1, calls);
    }

    [Fact]
    public void FailedDpiScopeRestorationDoesNotMaskTheOriginalFailure()
    {
        int calls = 0;
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(new Action(() =>
        {
            using ThreadDpiAwarenessScope scope = new(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2,
                _ => ++calls == 1 ? new IntPtr(-5) : IntPtr.Zero);
            throw new InvalidOperationException("Original failure");
        }));
        Assert.Equal("Original failure", exception.Message);
        Assert.Equal(2, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HdrComparisonCountsEquivalentEntriesRatherThanReusingAMatch(bool reverse)
    {
        ADVANCED_HDR_INFO_PER_PATH[] first = { Hdr(1, true), Hdr(2, true), Hdr(3, false) };
        ADVANCED_HDR_INFO_PER_PATH[] second = { Hdr(1, true), Hdr(2, false), Hdr(3, false) };
        Assert.False(reverse ? CollectionComparer.EqualButDifferentOrder(second, first) : CollectionComparer.EqualButDifferentOrder(first, second));
    }

    [Fact]
    public void HdrComparisonPreservesReorderingDuplicatesAndDisplayIdTolerance()
    {
        ADVANCED_HDR_INFO_PER_PATH[] first = { Hdr(1, true), Hdr(2, true), Hdr(3, false) };
        ADVANCED_HDR_INFO_PER_PATH[] second = { Hdr(9, false), Hdr(8, true), Hdr(7, true) };
        Assert.True(CollectionComparer.EqualButDifferentOrder(first, second));
        Assert.False(CollectionComparer.EqualButDifferentOrder(first, second.Take(2).ToArray()));
        Assert.True(CollectionComparer.EqualButDifferentOrder(Array.Empty<ADVANCED_HDR_INFO_PER_PATH>(), Array.Empty<ADVANCED_HDR_INFO_PER_PATH>()));
    }

    [Theory]
    [InlineData(2u, 1u)]
    [InlineData(2u, 3u)]
    [InlineData(7u, 8u)]
    public void AdapterRemappingKeepsEveryStructureConsistentAcrossRepeatedPasses(uint firstId, uint secondId)
    {
        WINDOWS_DISPLAY_CONFIG saved = new();
        saved.DisplayAdapters.Add(1, "card-A");
        saved.DisplayAdapters.Add(2, "card-B");
        DISPLAYCONFIG_PATH_INFO first = Path(0, 10);
        DISPLAYCONFIG_PATH_INFO second = Path(0, 20);
        second.SourceInfo.AdapterId = new LUID { LowPart = 2 };
        second.TargetInfo.AdapterId = new LUID { LowPart = 2 };
        saved.DisplayConfigPaths = new[] { first, second };
        saved.DisplayConfigModes = new[]
        {
            new DISPLAYCONFIG_MODE_INFO { AdapterId = new LUID { LowPart = 1 } },
            new DISPLAYCONFIG_MODE_INFO { AdapterId = new LUID { LowPart = 2 } }
        };
        for (uint adapterId = 1; adapterId <= 2; adapterId++)
        {
            ADVANCED_HDR_INFO_PER_PATH hdr = new() { AdapterId = new LUID { LowPart = adapterId } };
            hdr.AdvancedColorInfo.Header.AdapterId = hdr.AdapterId;
            hdr.SDRWhiteLevel.Header.AdapterId = hdr.AdapterId;
            saved.DisplayHDRStates.Add(hdr);
        }
        saved.DisplaySources.Add("source-A", new List<DISPLAY_SOURCE> { new() { AdapterId = new LUID { LowPart = 1 } } });
        saved.DisplaySources.Add("source-B", new List<DISPLAY_SOURCE> { new() { AdapterId = new LUID { LowPart = 2 } } });
        Dictionary<ulong, string> current = new() { [firstId] = "card-A", [secondId] = "card-B" };
        for (int pass = 0; pass < 3; pass++)
        {
            AgentWinLibrary.PatchWindowsDisplayConfig(ref saved, AgentWinLibrary.GetAdapterIdMap(saved, current), current);
            Assert.Equal("card-A", saved.DisplayAdapters[firstId]);
            Assert.Equal("card-B", saved.DisplayAdapters[secondId]);
            Assert.Equal(2, saved.DisplayAdapters.Count);
            for (int index = 0; index < 2; index++)
            {
                ulong expected = index == 0 ? firstId : secondId;
                Assert.Equal(expected, saved.DisplayConfigPaths[index].SourceInfo.AdapterId.Value);
                Assert.Equal(expected, saved.DisplayConfigPaths[index].TargetInfo.AdapterId.Value);
                Assert.Equal(expected, saved.DisplayConfigModes[index].AdapterId.Value);
                Assert.Equal(expected, saved.DisplayHDRStates[index].AdapterId.Value);
                Assert.Equal(expected, saved.DisplayHDRStates[index].AdvancedColorInfo.Header.AdapterId.Value);
                Assert.Equal(expected, saved.DisplayHDRStates[index].SDRWhiteLevel.Header.AdapterId.Value);
                Assert.Equal(expected, saved.DisplaySources[index == 0 ? "source-A" : "source-B"][0].AdapterId.Value);
            }
        }
    }

    [Fact]
    public void ConflictingPartialAdapterMapDoesNotDiscardSavedDictionaryEntries()
    {
        WINDOWS_DISPLAY_CONFIG config = new();
        config.DisplayAdapters.Add(1, "card-A");
        config.DisplayAdapters.Add(2, "unavailable-card");
        AgentWinLibrary.PatchWindowsDisplayConfig(ref config, new Dictionary<ulong, ulong> { [1] = 2 },
            new Dictionary<ulong, string> { [2] = "card-A" });
        Assert.Equal("card-A", config.DisplayAdapters[1]);
        Assert.Equal("unavailable-card", config.DisplayAdapters[2]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingDpiSourcesWarnAndStillApplyTheFollowingSource(bool nullList)
    {
        Dictionary<string, List<DISPLAY_SOURCE>> sources = new()
        {
            ["missing"] = nullList ? null! : new List<DISPLAY_SOURCE>(),
            ["working"] = new List<DISPLAY_SOURCE> { new() { SourceId = 7 } }
        };
        DisplayConfigurationDiagnostics diagnostics = new();
        List<uint> called = new();
        AgentWinLibrary.ApplySourceDpiSettings(sources, source => { called.Add(source.SourceId); return true; }, diagnostics);
        Assert.Equal(new uint[] { 7 }, called);
        Assert.True(diagnostics.Result.IsSuccessful);
        Assert.Equal("missing", Assert.Single(diagnostics.Result.Issues).DisplayId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectedOrThrowingDpiWriteDoesNotPreventTheNextSource(bool throws)
    {
        Dictionary<string, List<DISPLAY_SOURCE>> sources = new()
        {
            ["rejected"] = new List<DISPLAY_SOURCE> { new() { SourceId = 1 } },
            ["working"] = new List<DISPLAY_SOURCE> { new() { SourceId = 2 } }
        };
        DisplayConfigurationDiagnostics diagnostics = new();
        List<uint> called = new();
        AgentWinLibrary.ApplySourceDpiSettings(sources, source =>
        {
            called.Add(source.SourceId);
            return source.SourceId == 2 || (throws ? throw new InvalidOperationException("DPI write failed") : false);
        }, diagnostics);
        Assert.Equal(new uint[] { 1, 2 }, called);
        Assert.True(diagnostics.Result.IsSuccessful);
        Assert.Equal("rejected", Assert.Single(diagnostics.Result.Issues).DisplayId);
    }

    private static DISPLAYCONFIG_PATH_INFO Path(uint sourceId, uint targetId) => new()
    {
        Flags = DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_ACTIVE,
        SourceInfo = new DISPLAYCONFIG_PATH_SOURCE_INFO { AdapterId = new LUID { LowPart = 1 }, Id = sourceId, ModeInfoIdx = 3 },
        TargetInfo = new DISPLAYCONFIG_PATH_TARGET_INFO { AdapterId = new LUID { LowPart = 1 }, Id = targetId, ModeInfoIdx = 7 }
    };

    private static byte[] NativeBytes<T>(T value) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            // Initialise padding so comparisons measure the native fields consistently.
            byte[] bytes = new byte[size];
            Marshal.Copy(bytes, 0, buffer, size);
            Marshal.StructureToPtr(value, buffer, false);
            Marshal.Copy(buffer, bytes, 0, size);
            return bytes;
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    private static DISPLAYCONFIG_PATH_INFO VirtualPath(uint sourceId, uint targetId, ushort sourceMode)
    {
        DISPLAYCONFIG_PATH_INFO path = Path(sourceId, targetId);
        path.Flags |= DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE;
        path.SourceInfo.sourceModeInfoIdx = sourceMode;
        path.SourceInfo.cloneGroupId = ushort.MaxValue;
        return path;
    }

    private static ADVANCED_HDR_INFO_PER_PATH Hdr(uint id, bool enabled) => new()
    {
        Id = id,
        // Optional SDR metadata is deliberately absent, producing equivalent entries
        // while retaining the existing equality workaround for changing display IDs.
        AdvancedColorInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO { Value = enabled ? 3u : 1u, BitsPerColorChannel = 10 }
    };
}
