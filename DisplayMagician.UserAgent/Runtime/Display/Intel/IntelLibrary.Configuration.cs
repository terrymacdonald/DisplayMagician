using System;
using DisplayMagician.Contracts;
using IGCLWrapper;

namespace DisplayMagician.UserAgent.Runtime.Intel;

public partial class IntelLibrary
{
    internal static bool ApplyCombinedDisplay(bool required, CombinedDisplayArgsDto desired, CombinedDisplayArgsDto? current,
        Func<CombinedDisplayArgsDto, bool> write, Func<CombinedDisplayArgsDto?> read, Action<int> wait, int delay,
        DisplayConfigurationDiagnostics diagnostics, string adapterId)
    {
        if (!required)
        {
            if (!current.HasValue || current.Value.NumOutputs <= 1) return true;
            CombinedDisplayArgsDto disabled = current.Value;
            disabled.OpType = ctl_combined_display_optype_t.CTL_COMBINED_DISPLAY_OPTYPE_DISABLE;
            bool accepted = diagnostics.Run("CombinedDisplay", "Intel", () => write(disabled), DisplayConfigurationSeverity.Fatal, adapterId);
            if (accepted) SharedLogger.logger.Trace("IntelLibrary/ApplyCombinedDisplay: Driver accepted Combined Display removal for adapter {0}.", adapterId);
            return accepted;
        }
        int outputs = desired.NumOutputs == 0 ? desired.ChildInfos?.Count ?? 0 : desired.NumOutputs;
        // Validate before disabling an existing working group. These are the same
        // output-count/child-list requirements enforced by the shipped wrapper.
        if (outputs < 2 || outputs > 4 || desired.ChildInfos == null || desired.ChildInfos.Count < outputs)
        {
            diagnostics.Add("CombinedDisplay", "Intel", "The saved Combined Display has incomplete component outputs.", DisplayConfigurationSeverity.Fatal, adapterId);
            return false;
        }
        if (current.HasValue && current.Value.NumOutputs > 1 && current.Value.Equals(desired)) return true;
        if (current.HasValue && current.Value.NumOutputs > 1)
        {
            CombinedDisplayArgsDto disabled = current.Value;
            disabled.OpType = ctl_combined_display_optype_t.CTL_COMBINED_DISPLAY_OPTYPE_DISABLE;
            if (!diagnostics.Run("CombinedDisplay", "Intel", () => write(disabled), DisplayConfigurationSeverity.Fatal, adapterId)) return false;
            SharedLogger.logger.Trace("IntelLibrary/ApplyCombinedDisplay: Driver accepted preparatory group removal for adapter {0}; waiting {1} ms.", adapterId, delay);
            wait(delay);
        }
        desired.OpType = ctl_combined_display_optype_t.CTL_COMBINED_DISPLAY_OPTYPE_ENABLE;
        if (!diagnostics.Run("CombinedDisplay", "Intel", () => write(desired), DisplayConfigurationSeverity.Fatal, adapterId)) return false;
        SharedLogger.logger.Trace("IntelLibrary/ApplyCombinedDisplay: Driver accepted Combined Display creation for adapter {0}; checking output count.", adapterId);
        try
        {
            CombinedDisplayArgsDto? actual = read();
            if (!actual.HasValue || actual.Value.NumOutputs != outputs)
            {
                diagnostics.Add("CombinedDisplay", "Intel", "The required Combined Display outputs were not applied.", DisplayConfigurationSeverity.Fatal, adapterId);
                return false;
            }
            if (actual.Value.CombinedDesktopWidth != desired.CombinedDesktopWidth || actual.Value.CombinedDesktopHeight != desired.CombinedDesktopHeight)
                diagnostics.Add("CombinedDisplay", "Intel", "Combined Display is enabled but its dimensions differ before Windows layout application.", displayId: adapterId);
            return true;
        }
        catch (Exception ex)
        {
            diagnostics.Add("CombinedDisplay", "Intel", "Could not verify the required Combined Display: " + ex.Message, DisplayConfigurationSeverity.Fatal, adapterId, ex);
            return false;
        }
    }
}
