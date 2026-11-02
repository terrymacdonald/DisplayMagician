using System;
using System.Collections.Generic;
using System.Linq;
using DisplayMagician.Contracts;
using NVAPIWrapper;

namespace DisplayMagician.UserAgent.Runtime.NVIDIA;

public partial class NVIDIALibrary
{
    internal static bool ApplyMosaicConfiguration(NVIDIA_MOSAIC_CONFIG desired, NVIDIA_MOSAIC_CONFIG cached,
        Func<NVIDIA_MOSAIC_CONFIG?> readCurrent, Func<NVAPIMosaicGridTopologiesDto, bool> setGrids,
        Func<bool> prepare, Func<bool> disable, Action<int> wait, int delayInMs, DisplayConfigurationDiagnostics diagnostics)
    {
        NVIDIA_MOSAIC_CONFIG? current = null;
        try { current = readCurrent(); }
        catch (Exception ex) { diagnostics.Add("SurroundReadback", "NVIDIA", ex.Message, exception: ex); }
        if (!current.HasValue)
            diagnostics.Add("SurroundReadback", "NVIDIA", "Current Surround state is unavailable; cached state cannot confirm that the requested layout is already applied.");

        if (desired.IsMosaicEnabled)
        {
            if (current.HasValue && current.Value.MosaicGridTopologies.Grids?.Length > 0 && desired.Equals(current.Value)) return true;
            // Preserve the established 1x1 wake-up step. Its failure must not prevent
            // the actual creation attempt, which can still succeed on affected drivers.
            if (!(current?.IsMosaicEnabled ?? cached.IsMosaicEnabled))
            {
                try { prepare(); }
                catch (Exception ex) { diagnostics.Add("SurroundPreparation", "NVIDIA", "The preparatory 1x1 step failed; attempting the requested layout: " + ex.Message, exception: ex); }
            }
            return diagnostics.Run("Surround", "NVIDIA", () =>
            {
                bool applied = setGrids(desired.MosaicGridTopologies);
                wait(delayInMs * 3);
                return applied;
            }, DisplayConfigurationSeverity.Fatal);
        }

        // An unavailable comparison read must not turn unsupported Mosaic hardware
        // into a new failure gate. Retain positive cached evidence that removal is needed;
        // otherwise Windows applies/verifies the requested ordinary layout, with a warning.
        return !(current?.IsMosaicEnabled ?? cached.IsMosaicEnabled) ||
            diagnostics.Run("Surround", "NVIDIA", disable, DisplayConfigurationSeverity.Fatal);
    }

    internal static bool DisableMosaicWithFallback(Func<bool?> setSingleGrids, Func<bool?> readEnabled,
        Func<bool> disableLegacy, Action<int> wait, int delayInMs, DisplayConfigurationDiagnostics diagnostics)
    {
        bool primaryApplied = false;
        try
        {
            bool? primary = setSingleGrids();
            primaryApplied = primary == true;
            if (primary.HasValue) wait(delayInMs * 3);
        }
        catch (Exception ex)
        {
            // This attempt is deliberately recoverable through the legacy disable API.
            SharedLogger.logger.Warn(ex, "NVIDIALibrary/DisableMosaicWithFallback: The 1x1 grid attempt failed; checking state before the legacy fallback.");
        }

        bool? enabled = null;
        try { enabled = readEnabled(); }
        catch (Exception ex) { diagnostics.Add("SurroundReadback", "NVIDIA", ex.Message, exception: ex); }
        if (enabled == false) return true;
        if (primaryApplied && !enabled.HasValue)
        {
            diagnostics.Add("SurroundReadback", "NVIDIA", "The driver accepted the 1x1 grids, but Surround state could not be read back.");
            return true;
        }

        // Missing readback is not proof that Surround was disabled when no setter succeeded.
        bool legacyApplied = diagnostics.Run("Surround", "NVIDIA", () =>
        {
            bool applied = disableLegacy();
            wait(delayInMs * 3);
            return applied;
        }, DisplayConfigurationSeverity.Fatal);
        if (!legacyApplied) return false;
        try
        {
            enabled = readEnabled();
            if (enabled == true)
            {
                diagnostics.Add("Surround", "NVIDIA", "Surround is still enabled after the driver accepted the disable request.", DisplayConfigurationSeverity.Fatal);
                return false;
            }
            if (!enabled.HasValue)
                diagnostics.Add("SurroundReadback", "NVIDIA", "The driver accepted the legacy disable request, but its resulting state is unavailable.");
        }
        catch (Exception ex) { diagnostics.Add("SurroundReadback", "NVIDIA", ex.Message, exception: ex); }
        return true;
    }

    internal static bool TryCaptureDrsSnapshot(NVAPIDrsProfileDto profile, Func<NVAPIDrsSettingDto[]> enumerate,
        DisplayConfigurationDiagnostics diagnostics, out NVIDIA_DRS_CONFIG snapshot)
    {
        snapshot = new NVIDIA_DRS_CONFIG();
        try
        {
            NVAPIDrsSettingDto[] settings = enumerate();
            if (settings == null || (profile.NumOfSettings > 0 && settings.Length == 0))
            {
                diagnostics.Add("DriverSettings", "NVIDIA", "Driver settings could not be completely read; omitting this DRS snapshot.");
                return false;
            }
            snapshot.IsBaseProfile = true;
            snapshot.ProfileInfo = profile;
            snapshot.DriverSettings = settings.ToList();
            return true;
        }
        catch (Exception ex)
        {
            diagnostics.Add("DriverSettings", "NVIDIA", "Driver settings capture failed; omitting this DRS snapshot: " + ex.Message, exception: ex);
            return false;
        }
    }

    internal static void ApplyDrsSnapshot(NVIDIA_DRS_CONFIG stored, IReadOnlyList<NVAPIDrsSettingDto> current,
        Func<NVAPIDrsSettingDto, bool> setSetting, Func<uint, bool> restoreDefault, Func<bool> save,
        DisplayConfigurationDiagnostics diagnostics)
    {
        if (!stored.IsBaseProfile || stored.DriverSettings == null || (stored.ProfileInfo.NumOfSettings > 0 && stored.DriverSettings.Count == 0))
        {
            diagnostics.Add("DriverSettings", "NVIDIA", "The stored DRS snapshot is missing or incomplete; skipping its writes and default resets.");
            return;
        }
        bool needToSave = false;
        foreach (NVAPIDrsSettingDto desired in stored.DriverSettings)
        {
            foreach (NVAPIDrsSettingDto actual in current)
            {
                if (desired.SettingId != actual.SettingId) continue;
                bool matches = desired.SettingType switch
                {
                    _NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE => desired.CurrentDwordValue == actual.CurrentDwordValue,
                    _NVDRS_SETTING_TYPE.NVDRS_STRING_TYPE or _NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE => string.Equals(desired.CurrentStringValue, actual.CurrentStringValue, StringComparison.Ordinal),
                    _NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE => (desired.CurrentBinaryValue == null && actual.CurrentBinaryValue == null) ||
                        (desired.CurrentBinaryValue != null && actual.CurrentBinaryValue != null && desired.CurrentBinaryValue.SequenceEqual(actual.CurrentBinaryValue)),
                    _ => desired.Equals(actual)
                };
                if (!matches)
                {
                    try
                    {
                        if (setSetting(desired))
                        {
                            needToSave = true;
                            SharedLogger.logger.Trace($"NVIDIALibrary/ApplyDrsSnapshot: Applied setting {desired.SettingId} ({desired.SettingName}) in the DRS session; pending save.");
                        }
                        else diagnostics.Add("DriverSettings", "NVIDIA", $"The driver did not apply setting {desired.SettingId} ({desired.SettingName}).");
                    }
                    catch (Exception ex) { diagnostics.Add("DriverSettings", "NVIDIA", $"Could not apply setting {desired.SettingId}: {ex.Message}", exception: ex); }
                }
                break;
            }
        }
        // Preserve snapshot semantics: settings introduced since capture return to defaults.
        foreach (NVAPIDrsSettingDto actual in current)
        {
            if (stored.DriverSettings.Exists(setting => setting.SettingId == actual.SettingId)) continue;
            try
            {
                if (restoreDefault(actual.SettingId))
                {
                    needToSave = true;
                    SharedLogger.logger.Trace($"NVIDIALibrary/ApplyDrsSnapshot: Restored setting {actual.SettingId} ({actual.SettingName}) to default in the DRS session; pending save.");
                }
                else diagnostics.Add("DriverSettings", "NVIDIA", $"The driver did not restore setting {actual.SettingId} ({actual.SettingName}) to its default.");
            }
            catch (Exception ex) { diagnostics.Add("DriverSettings", "NVIDIA", $"Could not restore setting {actual.SettingId}: {ex.Message}", exception: ex); }
        }
        if (!needToSave) return;
        try
        {
            if (!save()) diagnostics.Add("DriverSettings", "NVIDIA", "The driver did not save the changed DRS settings.");
            else SharedLogger.logger.Trace("NVIDIALibrary/ApplyDrsSnapshot: Saved the changed DRS settings.");
        }
        catch (Exception ex) { diagnostics.Add("DriverSettings", "NVIDIA", "Could not save DRS settings: " + ex.Message, exception: ex); }
    }

    internal static bool TryRefreshOverrideState(Func<DisplayConfigurationDiagnostics, bool> refresh, DisplayConfigurationDiagnostics diagnostics)
    {
        DisplayConfigurationDiagnostics capture = new DisplayConfigurationDiagnostics();
        bool refreshed;
        try { refreshed = refresh(capture); }
        catch (Exception ex)
        {
            capture.Add("VendorOverride", "NVIDIA", ex.Message, exception: ex);
            refreshed = false;
        }
        foreach (DisplayConfigurationIssue issue in capture.Result.Issues)
        {
            DisplayConfigurationIssue warning = issue.Copy();
            warning.Severity = DisplayConfigurationSeverity.Warning;
            diagnostics.Result.Issues = diagnostics.Result.Issues.Append(warning).ToArray();
        }
        if (!refreshed)
            diagnostics.Add("VendorOverride", "NVIDIA", "Could not refresh current NVIDIA settings; skipping optional overrides rather than using stale comparisons.");
        return refreshed;
    }
}
