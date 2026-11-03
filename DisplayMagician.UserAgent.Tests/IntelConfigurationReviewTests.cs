using System;
using System.Collections.Generic;
using System.ComponentModel;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent.Runtime;
using DisplayMagician.UserAgent.Runtime.Intel;
using IGCLWrapper;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

[Collection("Agent repository state")]
public sealed class IntelConfigurationReviewTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectedOrThrowingRequiredWritesAreFatal(bool throws)
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(IntelLibrary.ApplyCombinedDisplay(true, Group(2, 3840), Group(0, 0),
            _ => throws ? throw new Win32Exception(5) : false, () => throw new InvalidOperationException("Read after failed write"),
            _ => { }, 10, diagnostics, "adapter"));
        Assert.False(diagnostics.Result.IsSuccessful);
        Assert.All(diagnostics.Result.Issues, issue => Assert.Equal("adapter", issue.DisplayId));
        if (throws) Assert.Equal("5", Assert.Single(diagnostics.Result.Issues).NativeErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void IncompleteDesiredGroupDoesNotDisableTheWorkingGroup(int children)
    {
        CombinedDisplayArgsDto desired = Group(2, 3840);
        desired.ChildInfos = children == 0 ? null! : new List<CombinedDisplayChildInfoDto>(new CombinedDisplayChildInfoDto[children]);
        if (children == 5) desired.NumOutputs = 5;
        DisplayConfigurationDiagnostics diagnostics = new();
        int writes = 0;
        Assert.False(IntelLibrary.ApplyCombinedDisplay(true, desired, Group(2, 1920), _ => { writes++; return true; },
            () => desired, _ => throw new InvalidOperationException("Unexpected wait"), 10, diagnostics, "adapter"));
        Assert.Equal(0, writes);
        Assert.False(diagnostics.Result.IsSuccessful);
    }

    [Fact]
    public void ReplacementPreservesDisableWaitEnableReadSequence()
    {
        CombinedDisplayArgsDto desired = Group(2, 3840);
        List<string> calls = new();
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.True(IntelLibrary.ApplyCombinedDisplay(true, desired, Group(2, 1920), args =>
        {
            calls.Add(args.OpType == ctl_combined_display_optype_t.CTL_COMBINED_DISPLAY_OPTYPE_DISABLE ? "disable" : "enable");
            return true;
        }, () => { calls.Add("read"); return desired; }, delay => calls.Add("wait " + delay), 10, diagnostics, "adapter"));
        Assert.Equal(new[] { "disable", "wait 10", "enable", "read" }, calls);
        Assert.True(diagnostics.Result.IsSuccessful);
        Assert.NotEqual(ctl_combined_display_optype_t.CTL_COMBINED_DISPLAY_OPTYPE_ENABLE, desired.OpType);
    }

    [Fact]
    public void FailedPreparatoryDisablePreventsCreationAndWaiting()
    {
        int writes = 0;
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(IntelLibrary.ApplyCombinedDisplay(true, Group(2, 3840), Group(2, 1920), args =>
        {
            writes++;
            Assert.Equal(ctl_combined_display_optype_t.CTL_COMBINED_DISPLAY_OPTYPE_DISABLE, args.OpType);
            return false;
        }, () => throw new InvalidOperationException(), _ => throw new InvalidOperationException(), 10, diagnostics, "adapter"));
        Assert.Equal(1, writes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RequiredReadbackCannotSilentlySucceed(int failure)
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(IntelLibrary.ApplyCombinedDisplay(true, Group(2, 3840), Group(0, 0), _ => true,
            () => failure == 0 ? null : failure == 1 ? Group(3, 3840) : throw new Win32Exception(5),
            _ => { }, 10, diagnostics, "adapter"));
        Assert.False(diagnostics.Result.IsSuccessful);
        Assert.Contains(diagnostics.Result.Issues, issue => issue.Stage == "CombinedDisplay" && issue.DisplayId == "adapter");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OrdinaryLayoutHonorsDisableResult(bool accepted)
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.Equal(accepted, IntelLibrary.ApplyCombinedDisplay(false, default, Group(2, 3840), args =>
        {
            Assert.Equal(ctl_combined_display_optype_t.CTL_COMBINED_DISPLAY_OPTYPE_DISABLE, args.OpType);
            return accepted;
        }, () => throw new InvalidOperationException(), _ => { }, 10, diagnostics, "adapter"));
        Assert.Equal(accepted, diagnostics.Result.IsSuccessful);
    }

    [Fact]
    public void ExistingLiveMatchAvoidsUnnecessaryChanges()
    {
        CombinedDisplayArgsDto desired = Group(2, 3840);
        Assert.True(IntelLibrary.ApplyCombinedDisplay(true, desired, desired,
            _ => throw new InvalidOperationException(), () => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException(), 10, new DisplayConfigurationDiagnostics(), "adapter"));
    }

    [Fact]
    public void DimensionDifferenceRemainsWarningBeforeWindowsApplication()
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.True(IntelLibrary.ApplyCombinedDisplay(true, Group(2, 3840), Group(0, 0), _ => true,
            () => Group(2, 1920), _ => { }, 10, diagnostics, "adapter"));
        Assert.True(diagnostics.Result.IsSuccessful);
        Assert.True(diagnostics.Result.HasWarnings);
    }

    [Fact]
    public void RejectedOptionalSettingReportsWarningWithDisplayIdentity()
    {
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(diagnostics.Run("GPUScaling", "Intel", () => false, DisplayConfigurationSeverity.Warning, "display-2"));
        Assert.True(diagnostics.Result.IsSuccessful);
        Assert.Equal("display-2", Assert.Single(diagnostics.Result.Issues).DisplayId);
    }

    [Fact]
    public void TopologyAdvisoryUsesProvidedSnapshotData()
    {
        INTEL_DISPLAY_CONFIG desired = new() { IsInUse = true, CombinedDisplayIsInUse = true };
        CombinedDisplayArgsDto group = Group(2, 3840);
        group.CombinedDisplayOutputWindowsDisplayEncoderId = 99;
        group.ChildInfos[0] = new CombinedDisplayChildInfoDto { DisplayOutputWindowsDisplayEncoderId = 1 };
        group.ChildInfos[1] = new CombinedDisplayChildInfoDto { DisplayOutputWindowsDisplayEncoderId = 2 };
        desired.PhysicalAdapters.Add("adapter", new INTEL_ADAPTER { IsCombinedDisplay = true, CombinedDisplay = group });
        desired.DisplayIdentifiers.Add("IntelIGCL#99");
        Assert.Contains("WINAPI|monitor|path|99", IntelDisplayTopology.GetHiddenIdentifiers(desired, desired,
            new[] { "IntelIGCL#1", "IntelIGCL#2" }, new[] { "WINAPI|monitor|path|99" }));
        Assert.Empty(IntelDisplayTopology.GetHiddenIdentifiers(desired, new INTEL_DISPLAY_CONFIG(),
            new[] { "IntelIGCL#1" }, new[] { "WINAPI|monitor|path|99" }));
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)2)]
    public void MatchingIncompleteDtoDoesNotProveRequiredGroupingExists(byte outputs)
    {
        CombinedDisplayArgsDto incomplete = new() { NumOutputs = outputs };
        incomplete.ChildInfos = null!;
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.False(IntelLibrary.ApplyCombinedDisplay(true, incomplete, incomplete,
            _ => throw new InvalidOperationException(), () => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException(), 10, diagnostics, "adapter"));
        Assert.False(diagnostics.Result.IsSuccessful);
    }

    [Fact]
    public void ZeroStoredCountUsesTheWrappersChildCountConvention()
    {
        CombinedDisplayArgsDto desired = Group(2, 3840);
        desired.NumOutputs = 0;
        DisplayConfigurationDiagnostics diagnostics = new();
        Assert.True(IntelLibrary.ApplyCombinedDisplay(true, desired, Group(0, 0), _ => true,
            () => Group(2, 3840), _ => { }, 10, diagnostics, "adapter"));
    }

    private static CombinedDisplayArgsDto Group(byte count, uint width) => new()
    {
        NumOutputs = count, CombinedDesktopWidth = width, CombinedDesktopHeight = 1080,
        ChildInfos = new List<CombinedDisplayChildInfoDto>(new CombinedDisplayChildInfoDto[count])
    };
}
