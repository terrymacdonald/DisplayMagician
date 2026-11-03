using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent.Runtime;
using DisplayMagician.UserAgent.Runtime.Windows;
using Xunit;
using AgentWinLibrary = DisplayMagician.UserAgent.Runtime.Windows.WinLibrary;

namespace DisplayMagician.UserAgent.Tests;

public sealed class WindowsLayoutFallbackTests
{
    private const int Delay = 10;

    [Fact]
    public void SuccessfulFirstApplyRetainsFlagsAndOnlyUsesTheExistingSuccessDelay()
    {
        WINDOWS_DISPLAY_CONFIG saved = Saved();
        List<SDC> flags = new();
        List<int> waits = new();
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.True(AgentWinLibrary.ApplyWindowsLayout(saved, Inventory, true, Delay, diagnostics, (count, paths, modesCount, modes, value) =>
        {
            flags.Add(value);
            Assert.Equal(2u, count);
            Assert.Equal((uint)saved.DisplayConfigModes.Length, modesCount);
            Assert.Equal(JsonConvert.SerializeObject(saved.DisplayConfigPaths), JsonConvert.SerializeObject(paths));
            Assert.Equal(JsonConvert.SerializeObject(saved.DisplayConfigModes), JsonConvert.SerializeObject(modes));
            Assert.True(value.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE));
            Assert.True(value.HasFlag(SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE));
            return WIN32STATUS.ERROR_SUCCESS;
        }, waits.Add));
        Assert.Equal(2, flags.Count);
        Assert.True(flags[0].HasFlag(SDC.SDC_VALIDATE));
        Assert.True(flags[1].HasFlag(SDC.SDC_APPLY));
        Assert.Equal(new[] { Delay }, waits);
        Assert.True(diagnostics.Result.TopologyApplied);
        Assert.Empty(diagnostics.Result.Issues);
    }

    [Theory]
    [InlineData(WIN32STATUS.ERROR_INVALID_PARAMETER)]
    [InlineData(WIN32STATUS.ERROR_NOT_SUPPORTED)]
    public void ValidationFallbackDropsOnlyRefreshAwarenessAndUsesTheResultForApply(WIN32STATUS rejected)
    {
        List<SDC> flags = new();
        Assert.True(AgentWinLibrary.ApplyWindowsLayout(Saved(), Inventory, true, Delay, new(), (_, _, _, _, value) =>
        {
            flags.Add(value);
            return flags.Count == 1 ? rejected : WIN32STATUS.ERROR_SUCCESS;
        }, _ => { }));
        Assert.Equal(3, flags.Count);
        Assert.All(flags, value => Assert.True(value.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE)));
        Assert.True(flags[0].HasFlag(SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE));
        Assert.False(flags[1].HasFlag(SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE));
        Assert.Equal((flags[1] & ~SDC.SDC_VALIDATE) | SDC.SDC_APPLY, flags[2]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ApplyFallbackKeepsVirtualAwarenessDuringEitherFullAttempt(bool secondAttempt)
    {
        List<int> waits = new();
        int fullCalls = 0;
        Assert.True(AgentWinLibrary.ApplyWindowsLayout(Saved(), Inventory, true, Delay, new(), (_, _, _, _, flags) =>
        {
            Assert.True(flags.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE));
            Assert.False(flags.HasFlag(SDC.SDC_TOPOLOGY_SUPPLIED));
            if (flags.HasFlag(SDC.SDC_VALIDATE)) return WIN32STATUS.ERROR_SUCCESS;
            fullCalls++;
            if (secondAttempt && fullCalls == 1) return WIN32STATUS.ERROR_GEN_FAILURE;
            return flags.HasFlag(SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE) ? WIN32STATUS.ERROR_INVALID_PARAMETER : WIN32STATUS.ERROR_SUCCESS;
        }, waits.Add));
        Assert.Equal(secondAttempt ? 3 : 2, fullCalls);
        Assert.Equal(secondAttempt ? new[] { Delay * 2, Delay } : new[] { Delay }, waits);
    }

    [Fact]
    public void ModernTopologyRetryPreservesClonesAndTheExistingTwoAndThreeTimesDelays()
    {
        List<int> waits = new();
        int full = 0, topology = 0;
        Assert.True(AgentWinLibrary.ApplyWindowsLayout(Saved(), Inventory, true, Delay, new(), (_, paths, count, modes, flags) =>
        {
            Assert.True(flags.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE));
            if (flags.HasFlag(SDC.SDC_VALIDATE)) return WIN32STATUS.ERROR_SUCCESS;
            if (!flags.HasFlag(SDC.SDC_TOPOLOGY_SUPPLIED)) { full++; return WIN32STATUS.ERROR_GEN_FAILURE; }
            topology++;
            Assert.Equal(0u, count);
            Assert.Null(modes);
            Assert.Equal(paths[0].SourceInfo.cloneGroupId, paths[1].SourceInfo.cloneGroupId);
            Assert.All(paths, path => Assert.Equal(uint.MaxValue, path.TargetInfo.ModeInfoIdx));
            return flags.HasFlag(SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE) ? WIN32STATUS.ERROR_NOT_SUPPORTED : WIN32STATUS.ERROR_SUCCESS;
        }, waits.Add));
        Assert.Equal(2, full);
        Assert.Equal(2, topology);
        Assert.Equal(new[] { Delay * 2, Delay * 3, Delay }, waits);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FinalLegacyRouteIsValidatedOnceBeforeOneApplyAndNeverMutatesSavedOrQueriedData(bool validationFails)
    {
        WINDOWS_DISPLAY_CONFIG saved = Saved();
        DISPLAYCONFIG_PATH_INFO[] inventory = Inventory();
        string before = JsonConvert.SerializeObject(saved);
        string inventoryBefore = JsonConvert.SerializeObject(inventory);
        int legacyValidation = 0, legacyApply = 0, modernApply = 0;
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.True(AgentWinLibrary.ApplyWindowsLayout(saved, () => inventory, true, Delay, diagnostics, (_, paths, count, modes, flags) =>
        {
            if (flags.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE))
            {
                if (flags.HasFlag(SDC.SDC_VALIDATE)) return validationFails ? WIN32STATUS.ERROR_INVALID_PARAMETER : WIN32STATUS.ERROR_SUCCESS;
                modernApply++;
                // Deliberately corrupt the supplied copy. Every subsequent attempt must use intact data.
                paths[0].SourceInfo.Id = 999;
                if (modes != null) modes[0].SourceMode.Width = 1;
                return WIN32STATUS.ERROR_INVALID_PARAMETER;
            }
            Assert.True(flags.HasFlag(SDC.SDC_TOPOLOGY_SUPPLIED));
            Assert.False(flags.HasFlag(SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE));
            Assert.False(flags.HasFlag(SDC.SDC_SAVE_TO_DATABASE));
            Assert.Null(modes);
            Assert.Equal(0u, count);
            Assert.Equal(new uint[] { 10, 11 }, paths.Select(path => path.TargetInfo.Id));
            Assert.Equal(paths[0].SourceInfo.Id, paths[1].SourceInfo.Id);
            Assert.NotEqual(999u, paths[0].SourceInfo.Id);
            Assert.All(paths, path =>
            {
                Assert.False(path.Flags.HasFlag(DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE));
                Assert.False(path.Flags.HasFlag(DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_BOOST_REFRESH_RATE));
                Assert.Equal(uint.MaxValue, path.SourceInfo.ModeInfoIdx);
                Assert.Equal(uint.MaxValue, path.TargetInfo.ModeInfoIdx);
            });
            if (flags.HasFlag(SDC.SDC_VALIDATE)) { legacyValidation++; paths[0].SourceInfo.Id = 999; }
            else { Assert.Equal(1, legacyValidation); legacyApply++; }
            return WIN32STATUS.ERROR_SUCCESS;
        }, _ => { }));
        Assert.Equal(validationFails ? 0 : 4, modernApply);
        Assert.Equal(1, legacyValidation);
        Assert.Equal(1, legacyApply);
        Assert.Equal(before, JsonConvert.SerializeObject(saved));
        Assert.Equal(inventoryBefore, JsonConvert.SerializeObject(inventory));
        Assert.True(diagnostics.Result.IsSuccessful);
        Assert.True(diagnostics.Result.TopologyApplied);
    }

    [Theory]
    [InlineData(WIN32STATUS.ERROR_ACCESS_DENIED)]
    [InlineData(WIN32STATUS.ERROR_GEN_FAILURE)]
    [InlineData(WIN32STATUS.ERROR_BAD_CONFIGURATION)]
    public void NonCompatibilityValidationErrorsNeverEnterAnyApplyRoute(WIN32STATUS error)
    {
        int calls = 0;
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(AgentWinLibrary.ApplyWindowsLayout(Saved(), Inventory, true, Delay, diagnostics, (_, _, _, _, flags) =>
        {
            calls++;
            Assert.True(flags.HasFlag(SDC.SDC_VALIDATE));
            return error;
        }, _ => throw new InvalidOperationException("Unexpected delay")));
        Assert.Equal(1, calls);
        Assert.Equal(error.ToString(), Assert.Single(diagnostics.Result.Issues).NativeErrorCode);
    }

    [Fact]
    public void NonCompatibilityApplyFailureKeepsExistingAttemptsWithoutAddingALegacyAttempt()
    {
        int applies = 0;
        List<int> waits = new();
        Assert.False(AgentWinLibrary.ApplyWindowsLayout(Saved(), Inventory, true, Delay, new(), (_, _, _, _, flags) =>
        {
            Assert.True(flags.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE));
            if (flags.HasFlag(SDC.SDC_VALIDATE)) return WIN32STATUS.ERROR_SUCCESS;
            applies++;
            return WIN32STATUS.ERROR_GEN_FAILURE;
        }, waits.Add));
        Assert.Equal(3, applies);
        Assert.Equal(new[] { Delay * 2, Delay * 3 }, waits);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LegacyValidationOrApplyFailureIsTerminalForThatCompatibilityRoute(bool rejectValidation)
    {
        int legacyValidations = 0, legacyApplies = 0;
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(AgentWinLibrary.ApplyWindowsLayout(Saved(), Inventory, true, Delay, diagnostics, (_, _, _, _, flags) =>
        {
            if (flags.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE)) return WIN32STATUS.ERROR_INVALID_PARAMETER;
            if (flags.HasFlag(SDC.SDC_VALIDATE)) { legacyValidations++; return rejectValidation ? WIN32STATUS.ERROR_ACCESS_DENIED : WIN32STATUS.ERROR_SUCCESS; }
            legacyApplies++;
            return WIN32STATUS.ERROR_GEN_FAILURE;
        }, _ => { }));
        Assert.Equal(1, legacyValidations);
        Assert.Equal(rejectValidation ? 0 : 1, legacyApplies);
        Assert.Equal((rejectValidation ? WIN32STATUS.ERROR_ACCESS_DENIED : WIN32STATUS.ERROR_GEN_FAILURE).ToString(), Assert.Single(diagnostics.Result.Issues).NativeErrorCode);
    }

    [Fact]
    public void ValidationHelperIsReadOnlyAndSupportsVirtualDataWithoutDynamicRefresh()
    {
        int calls = 0;
        WIN32STATUS result = AgentWinLibrary.ValidateWindowsLayout(Saved(), false, out SDC finalFlags, (_, _, _, _, flags) =>
        {
            calls++;
            Assert.True(flags.HasFlag(SDC.SDC_VALIDATE));
            Assert.False(flags.HasFlag(SDC.SDC_APPLY));
            Assert.True(flags.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE));
            Assert.False(flags.HasFlag(SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE));
            return WIN32STATUS.ERROR_SUCCESS;
        });
        Assert.Equal(WIN32STATUS.ERROR_SUCCESS, result);
        Assert.Equal(1, calls);
        Assert.False(finalFlags.HasFlag(SDC.SDC_APPLY));
    }

    [Fact]
    public void AlreadyLegacyTopologyIsNotSubmittedAsAnExtraDuplicateAttempt()
    {
        WINDOWS_DISPLAY_CONFIG saved = new() { DisplayConfigPaths = new[] { Route(0, 10) } };
        int calls = 0;
        Assert.False(AgentWinLibrary.ApplyWindowsLayout(saved, Inventory, false, Delay, new(), (_, _, _, _, flags) =>
        {
            calls++;
            Assert.False(flags.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE));
            return flags.HasFlag(SDC.SDC_VALIDATE) ? WIN32STATUS.ERROR_SUCCESS : WIN32STATUS.ERROR_INVALID_PARAMETER;
        }, _ => { }));
        Assert.Equal(4, calls);
    }

    [Fact]
    public void LegacySourceMatchingFindsACompleteAssignmentWithoutMergingExtendedGroups()
    {
        WINDOWS_DISPLAY_CONFIG saved = Saved();
        saved.DisplayConfigPaths[1].SourceInfo.sourceModeInfoIdx = 1;
        DISPLAYCONFIG_PATH_INFO[] inventory = { Route(0, 10), Route(1, 10), Route(0, 11) };
        Assert.True(AgentWinLibrary.TryCreateLegacyTopology(saved.DisplayConfigPaths, inventory, out var converted, out string reason), reason);
        Assert.Equal(1u, converted[0].SourceInfo.Id);
        Assert.Equal(0u, converted[1].SourceInfo.Id);
        Assert.Equal(new uint[] { 10, 11 }, converted.Select(path => path.TargetInfo.Id));
    }

    [Fact]
    public void ConversionDoesNotInventRoutesOrEnableInactiveTargets()
    {
        WINDOWS_DISPLAY_CONFIG saved = Saved();
        DISPLAYCONFIG_PATH_INFO inactive = Route(0, 99);
        inactive.Flags = 0;
        saved.DisplayConfigPaths = saved.DisplayConfigPaths.Append(inactive).ToArray();
        Assert.True(AgentWinLibrary.TryCreateLegacyTopology(saved.DisplayConfigPaths, Inventory(), out var converted, out _));
        Assert.Equal(new uint[] { 10, 11 }, converted.Select(path => path.TargetInfo.Id));
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("virtual")]
    [InlineData("unavailable")]
    [InlineData("adapter")]
    [InlineData("clone")]
    public void UnrepresentableLegacyTopologySkipsTheFinalNativeRoute(string kind)
    {
        WINDOWS_DISPLAY_CONFIG saved = Saved();
        DISPLAYCONFIG_PATH_INFO[] inventory = Inventory();
        if (kind == "missing") inventory = Array.Empty<DISPLAYCONFIG_PATH_INFO>();
        if (kind == "virtual") for (int i = 0; i < inventory.Length; i++) inventory[i].Flags |= DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE;
        if (kind == "unavailable") for (int i = 0; i < inventory.Length; i++) inventory[i].TargetInfo.TargetAvailable = false;
        if (kind == "adapter") for (int i = 0; i < inventory.Length; i++) inventory[i].SourceInfo.AdapterId.LowPart = 2;
        if (kind == "clone") inventory = new[] { Route(0, 10), Route(1, 11) };
        int calls = 0;
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(AgentWinLibrary.ApplyWindowsLayout(saved, () => inventory, true, Delay, diagnostics, (_, _, _, _, flags) =>
        {
            calls++;
            Assert.True(flags.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE));
            return WIN32STATUS.ERROR_INVALID_PARAMETER;
        }, _ => { }));
        Assert.Equal(2, calls);
        Assert.Contains(diagnostics.Result.Issues, issue => issue.Stage == "LegacyTopology");
        Assert.False(diagnostics.Result.IsSuccessful);
    }

    [Theory]
    [InlineData("duplicate")]
    [InlineData("identity")]
    [InlineData("cross-adapter")]
    [InlineData("empty")]
    public void ConversionRejectsMissingOrUnrepresentableCloneIdentity(string kind)
    {
        WINDOWS_DISPLAY_CONFIG saved = Saved();
        DISPLAYCONFIG_PATH_INFO[] inventory = Inventory();
        if (kind == "duplicate") saved.DisplayConfigPaths[1].TargetInfo.Id = 10;
        if (kind == "empty") saved.DisplayConfigPaths = Array.Empty<DISPLAYCONFIG_PATH_INFO>();
        if (kind == "identity")
        {
            saved.DisplayConfigPaths[0].SourceInfo.sourceModeInfoIdx = ushort.MaxValue;
            saved.DisplayConfigPaths[0].SourceInfo.cloneGroupId = ushort.MaxValue;
            saved.DisplayConfigPaths[0].SourceInfo.Id = uint.MaxValue;
        }
        if (kind == "cross-adapter")
        {
            for (int i = 0; i < saved.DisplayConfigPaths.Length; i++)
            { saved.DisplayConfigPaths[i].SourceInfo.sourceModeInfoIdx = ushort.MaxValue; saved.DisplayConfigPaths[i].SourceInfo.cloneGroupId = 7; }
            saved.DisplayConfigPaths[1].SourceInfo.AdapterId.LowPart = 2;
            saved.DisplayConfigPaths[1].TargetInfo.AdapterId.LowPart = 2;
            DISPLAYCONFIG_PATH_INFO second = Route(0, 11);
            second.SourceInfo.AdapterId.LowPart = 2;
            second.TargetInfo.AdapterId.LowPart = 2;
            inventory = new[] { Route(0, 10), second };
        }
        Assert.False(AgentWinLibrary.TryCreateLegacyTopology(saved.DisplayConfigPaths, inventory, out var converted, out string reason));
        Assert.Empty(converted);
        Assert.False(string.IsNullOrWhiteSpace(reason));
    }

    [Fact]
    public void LegacyRoutesAreReadOnlyAfterModernAttemptsFailAndExactlyOnce()
    {
        int modernApplies = 0, reads = 0;
        bool ReadAfterAttempts()
        {
            Assert.Equal(4, modernApplies);
            reads++;
            return true;
        }
        Assert.True(AgentWinLibrary.ApplyWindowsLayout(Saved(), () => { Assert.True(ReadAfterAttempts()); return Inventory(); }, true, Delay, new(),
            (_, _, _, _, flags) =>
            {
                if (flags.HasFlag(SDC.SDC_VALIDATE)) return WIN32STATUS.ERROR_SUCCESS;
                if (flags.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE)) { modernApplies++; return WIN32STATUS.ERROR_INVALID_PARAMETER; }
                return WIN32STATUS.ERROR_SUCCESS;
            }, _ => { }));
        Assert.Equal(1, reads);
    }

    [Fact]
    public void NormalSuccessDoesNotRequestLegacyInventory()
    {
        Assert.True(AgentWinLibrary.ApplyWindowsLayout(Saved(), () => throw new InvalidOperationException("Unexpected legacy scan"),
            true, Delay, new(), (_, _, _, _, _) => WIN32STATUS.ERROR_SUCCESS, _ => { }));
    }

    [Fact]
    public void FailedLegacyInventoryReadIsReportedWithoutAnotherApply()
    {
        int calls = 0;
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(AgentWinLibrary.ApplyWindowsLayout(Saved(), () => throw new InvalidOperationException("Legacy scan failed"),
            true, Delay, diagnostics, (_, _, _, _, flags) =>
            {
                calls++;
                Assert.True(flags.HasFlag(SDC.SDC_VALIDATE));
                return WIN32STATUS.ERROR_INVALID_PARAMETER;
            }, _ => { }));
        Assert.Equal(2, calls);
        Assert.Contains(diagnostics.Result.Issues, issue => issue.Stage == "LegacyTopology" && issue.Message.Contains("Legacy scan failed"));
        Assert.False(diagnostics.Result.IsSuccessful);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LegacyConversionPreservesClonesWhenPackedSourceModesAreAbsent(bool explicitGroup)
    {
        WINDOWS_DISPLAY_CONFIG saved = Saved();
        for (int index = 0; index < saved.DisplayConfigPaths.Length; index++)
        {
            saved.DisplayConfigPaths[index].SourceInfo.sourceModeInfoIdx = ushort.MaxValue;
            saved.DisplayConfigPaths[index].SourceInfo.cloneGroupId = explicitGroup ? (ushort)42 : ushort.MaxValue;
            if (!explicitGroup) saved.DisplayConfigPaths[index].SourceInfo.Id = 0;
        }
        string before = JsonConvert.SerializeObject(saved.DisplayConfigPaths);
        Assert.True(AgentWinLibrary.TryCreateLegacyTopology(saved.DisplayConfigPaths, Inventory(), out var converted, out string reason), reason);
        Assert.Equal(converted[0].SourceInfo.AdapterId.Value, converted[1].SourceInfo.AdapterId.Value);
        Assert.Equal(converted[0].SourceInfo.Id, converted[1].SourceInfo.Id);
        Assert.Equal(before, JsonConvert.SerializeObject(saved.DisplayConfigPaths));
    }

    [Fact]
    public void EqualSourceNumbersOnDifferentAdaptersRemainSeparateExtendedGroups()
    {
        WINDOWS_DISPLAY_CONFIG saved = Saved();
        saved.DisplayConfigPaths[1].SourceInfo.AdapterId.LowPart = 2;
        saved.DisplayConfigPaths[1].TargetInfo.AdapterId.LowPart = 2;
        DISPLAYCONFIG_PATH_INFO second = Route(0, 11);
        second.SourceInfo.AdapterId.LowPart = 2;
        second.TargetInfo.AdapterId.LowPart = 2;
        Assert.True(AgentWinLibrary.TryCreateLegacyTopology(saved.DisplayConfigPaths, new[] { Route(0, 10), second }, out var converted, out string reason), reason);
        Assert.Equal(0u, converted[0].SourceInfo.Id);
        Assert.Equal(0u, converted[1].SourceInfo.Id);
        Assert.NotEqual(converted[0].SourceInfo.AdapterId.Value, converted[1].SourceInfo.AdapterId.Value);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LegacyDpiWritesFollowConvertedTargetsInsteadOfOldSourceIds(bool conflicting)
    {
        Dictionary<string, List<DISPLAY_SOURCE>> sources = new()
        {
            ["first"] = new() { new() { AdapterId = new LUID { LowPart = 1 }, SourceId = 9, TargetId = 10, SourceDPIScalingInfo = new() { Current = 100 } } },
            ["second"] = new() { new() { AdapterId = new LUID { LowPart = 1 }, SourceId = 8, TargetId = 11, SourceDPIScalingInfo = new() { Current = conflicting ? 150u : 100u } } }
        };
        DISPLAYCONFIG_PATH_INFO[] legacy = { Route(4, 10), Route(4, 11) };
        List<uint> called = new();
        DisplayConfigurationDiagnostics diagnostics = new();
        AgentWinLibrary.ApplySourceDpiSettings(sources, source => { called.Add(source.SourceId); return true; }, diagnostics, legacy);
        Assert.Equal(new uint[] { 4 }, called);
        Assert.Equal(9u, sources["first"][0].SourceId);
        Assert.Equal(8u, sources["second"][0].SourceId);
        Assert.True(diagnostics.Result.IsSuccessful);
        Assert.Equal(conflicting ? 1 : 0, diagnostics.Result.Issues.Length);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnknownOrAmbiguousLegacyDpiTargetsWarnWithoutWriting(bool ambiguous)
    {
        Dictionary<string, List<DISPLAY_SOURCE>> sources = new()
        {
            ["saved"] = new() { new() { AdapterId = new LUID { LowPart = 1 }, SourceId = 9, TargetId = 10, SourceDPIScalingInfo = new() { Current = 100 } } }
        };
        DISPLAYCONFIG_PATH_INFO duplicate = Route(5, 10);
        duplicate.TargetInfo.AdapterId.LowPart = 2;
        DISPLAYCONFIG_PATH_INFO[] legacy = ambiguous ? new[] { Route(4, 10), duplicate } : new[] { Route(4, 99) };
        DisplayConfigurationDiagnostics diagnostics = new();
        AgentWinLibrary.ApplySourceDpiSettings(sources, _ => throw new InvalidOperationException("Unexpected DPI write"), diagnostics, legacy);
        Assert.Equal("Scaling", Assert.Single(diagnostics.Result.Issues).Stage);
        Assert.True(diagnostics.Result.IsSuccessful);
    }

    [Fact]
    public void UncapturedLegacyDpiDoesNotOverrideAnotherCloneMembersCapturedPreference()
    {
        Dictionary<string, List<DISPLAY_SOURCE>> sources = new()
        {
            ["uncaptured"] = new() { new() { AdapterId = new LUID { LowPart = 1 }, SourceId = 9, TargetId = 10 } },
            ["captured"] = new() { new() { AdapterId = new LUID { LowPart = 1 }, SourceId = 8, TargetId = 11, SourceDPIScalingInfo = new() { Current = 150 } } }
        };
        List<uint> requested = new();
        DisplayConfigurationDiagnostics diagnostics = new();
        AgentWinLibrary.ApplySourceDpiSettings(sources, source => { Assert.Equal(4u, source.SourceId); requested.Add(source.SourceDPIScalingInfo.Current); return true; },
            diagnostics, new[] { Route(4, 10), Route(4, 11) });
        Assert.Equal(new uint[] { 150 }, requested);
        Assert.Empty(diagnostics.Result.Issues);
    }

    [Fact]
    public void SuccessfulLegacyRouteReportsItsPreparedPathsForFollowingOptionalSettings()
    {
        DISPLAYCONFIG_PATH_INFO[]? applied = null;
        Assert.True(AgentWinLibrary.ApplyWindowsLayout(Saved(), Inventory, true, Delay, new(), (_, _, _, _, flags) =>
            flags.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE) ? WIN32STATUS.ERROR_INVALID_PARAMETER : WIN32STATUS.ERROR_SUCCESS,
            _ => { }, legacyApplied: paths => applied = paths));
        Assert.NotNull(applied);
        Assert.Equal(applied[0].SourceInfo.Id, applied[1].SourceInfo.Id);
        Assert.Equal(new uint[] { 10, 11 }, applied.Select(path => path.TargetInfo.Id));
    }

    private static WINDOWS_DISPLAY_CONFIG Saved()
    {
        DISPLAYCONFIG_PATH_INFO[] paths = { Route(0, 10), Route(1, 11) };
        for (int index = 0; index < paths.Length; index++)
        {
            paths[index].Flags |= DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE | DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_BOOST_REFRESH_RATE;
            paths[index].SourceInfo.sourceModeInfoIdx = 0;
            paths[index].SourceInfo.cloneGroupId = ushort.MaxValue;
            paths[index].TargetInfo.ModeInfoIdx = 0x0001ffff;
        }
        return new WINDOWS_DISPLAY_CONFIG
        {
            DisplayConfigPaths = paths,
            DisplayConfigModes = new[]
            {
                new DISPLAYCONFIG_MODE_INFO { AdapterId = new LUID { LowPart = 1 }, Id = 0, InfoType = DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE,
                    SourceMode = new DISPLAYCONFIG_SOURCE_MODE { Width = 3840, Height = 1080 } },
                new DISPLAYCONFIG_MODE_INFO { AdapterId = new LUID { LowPart = 1 }, Id = 10, InfoType = DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET }
            }
        };
    }

    private static DISPLAYCONFIG_PATH_INFO[] Inventory() => new[] { Route(0, 10), Route(1, 10), Route(0, 11), Route(1, 11) };
    private static DISPLAYCONFIG_PATH_INFO Route(uint source, uint target) => new()
    {
        Flags = DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_ACTIVE,
        SourceInfo = new DISPLAYCONFIG_PATH_SOURCE_INFO { AdapterId = new LUID { LowPart = 1 }, Id = source, ModeInfoIdx = 2 },
        TargetInfo = new DISPLAYCONFIG_PATH_TARGET_INFO { AdapterId = new LUID { LowPart = 1 }, Id = target, ModeInfoIdx = 3, TargetAvailable = true,
            Rotation = DISPLAYCONFIG_ROTATION.DISPLAYCONFIG_ROTATION_IDENTITY, RefreshRate = new DISPLAYCONFIG_RATIONAL { Numerator = 60, Denominator = 1 } }
    };
}
