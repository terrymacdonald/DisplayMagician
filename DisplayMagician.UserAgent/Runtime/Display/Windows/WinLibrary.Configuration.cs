using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DisplayMagician.Contracts;

namespace DisplayMagician.UserAgent.Runtime.Windows;

public partial class WinLibrary
{
    internal static WIN32STATUS ValidateWindowsLayout(WINDOWS_DISPLAY_CONFIG desired, bool refreshAware, out SDC flags,
        Func<uint, DISPLAYCONFIG_PATH_INFO[], uint, DISPLAYCONFIG_MODE_INFO[]?, SDC, WIN32STATUS>? setConfiguration = null)
    {
        setConfiguration ??= CCDImport.SetDisplayConfig;
        flags = SDC.DISPLAYMAGICIAN_VALIDATE;
        if (refreshAware || desired.DisplayConfigPaths.Any(path => path.Flags.HasFlag(DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE)))
            flags |= SDC.SDC_VIRTUAL_MODE_AWARE;
        if (refreshAware) flags |= SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE;
        WIN32STATUS error = setConfiguration((uint)desired.DisplayConfigPaths.Length, desired.DisplayConfigPaths.ToArray(),
            (uint)desired.DisplayConfigModes.Length, desired.DisplayConfigModes.ToArray(), flags);
        SharedLogger.logger.Log(error == WIN32STATUS.ERROR_SUCCESS ? NLog.LogLevel.Trace : NLog.LogLevel.Warn, "WinLibrary/ValidateWindowsLayout: Full-layout validation returned {0}; flags={1}.", error, flags);
        if (flags.HasFlag(SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE) && IsWindowsCompatibilityError(error))
        {
            flags &= ~SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE;
            SharedLogger.logger.Warn("WinLibrary/ValidateWindowsLayout: Retrying validation without refresh-rate awareness, preserving virtual-mode awareness; flags={0}.", flags);
            error = setConfiguration((uint)desired.DisplayConfigPaths.Length, desired.DisplayConfigPaths.ToArray(),
                (uint)desired.DisplayConfigModes.Length, desired.DisplayConfigModes.ToArray(), flags);
            SharedLogger.logger.Log(error == WIN32STATUS.ERROR_SUCCESS ? NLog.LogLevel.Trace : NLog.LogLevel.Warn, "WinLibrary/ValidateWindowsLayout: Reduced-refresh validation returned {0}.", error);
        }
        return error;
    }

    internal static bool ApplyWindowsLayout(WINDOWS_DISPLAY_CONFIG desired, Func<DISPLAYCONFIG_PATH_INFO[]> readLegacyInventory,
        bool refreshAware, int delayInMs, DisplayConfigurationDiagnostics diagnostics,
        Func<uint, DISPLAYCONFIG_PATH_INFO[], uint, DISPLAYCONFIG_MODE_INFO[]?, SDC, WIN32STATUS>? setConfiguration = null,
        Action<int>? wait = null, Action<DISPLAYCONFIG_PATH_INFO[]>? legacyApplied = null)
    {
        setConfiguration ??= CCDImport.SetDisplayConfig;
        wait ??= Thread.Sleep;
        WIN32STATUS error = ValidateWindowsLayout(desired, refreshAware, out SDC validationFlags, setConfiguration);
        SDC setFlags = (validationFlags & ~SDC.SDC_VALIDATE) | SDC.SDC_APPLY;
        SDC topologyFlags = SDC.SDC_APPLY | SDC.SDC_TOPOLOGY_SUPPLIED | SDC.SDC_ALLOW_CHANGES |
            (validationFlags & (SDC.SDC_VIRTUAL_MODE_AWARE | SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE));
        bool legacyTopologyAlreadyTried = false;
        if (error == WIN32STATUS.ERROR_SUCCESS)
        {
            // Retain the two full-layout attempts and their existing settling delays.
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                if (attempt == 2) wait(delayInMs * 2);
                error = setConfiguration((uint)desired.DisplayConfigPaths.Length, desired.DisplayConfigPaths.ToArray(),
                    (uint)desired.DisplayConfigModes.Length, desired.DisplayConfigModes.ToArray(), setFlags);
                SharedLogger.logger.Log(error == WIN32STATUS.ERROR_SUCCESS ? NLog.LogLevel.Trace : NLog.LogLevel.Warn, "WinLibrary/ApplyWindowsLayout: Full-layout attempt {0} returned {1}; flags={2}.", attempt, error, setFlags);
                if (setFlags.HasFlag(SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE) && IsWindowsCompatibilityError(error))
                {
                    setFlags &= ~SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE;
                    topologyFlags &= ~SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE;
                    SharedLogger.logger.Warn("WinLibrary/ApplyWindowsLayout: Retrying full-layout attempt {0} without refresh-rate awareness; flags={1}.", attempt, setFlags);
                    error = setConfiguration((uint)desired.DisplayConfigPaths.Length, desired.DisplayConfigPaths.ToArray(),
                        (uint)desired.DisplayConfigModes.Length, desired.DisplayConfigModes.ToArray(), setFlags);
                }
                if (error == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Info("WinLibrary/ApplyWindowsLayout: Full-layout attempt {0} succeeded; flags={1}.", attempt, setFlags);
                    wait(delayInMs);
                    diagnostics.Result.TopologyApplied = true;
                    return true;
                }
            }

            wait(delayInMs * 3);
            error = ApplyTopologyOnlyConfiguration(desired.DisplayConfigPaths, topologyFlags, setConfiguration);
            SharedLogger.logger.Log(error == WIN32STATUS.ERROR_SUCCESS ? NLog.LogLevel.Trace : NLog.LogLevel.Warn, "WinLibrary/ApplyWindowsLayout: Modern topology-only attempt returned {0}; flags={1}.", error, topologyFlags);
            if (topologyFlags.HasFlag(SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE) && IsWindowsCompatibilityError(error))
            {
                topologyFlags &= ~SDC.SDC_VIRTUAL_REFRESH_RATE_AWARE;
                SharedLogger.logger.Warn("WinLibrary/ApplyWindowsLayout: Retrying topology-only without refresh-rate awareness; flags={0}.", topologyFlags);
                error = ApplyTopologyOnlyConfiguration(desired.DisplayConfigPaths, topologyFlags, setConfiguration);
            }
            legacyTopologyAlreadyTried = !topologyFlags.HasFlag(SDC.SDC_VIRTUAL_MODE_AWARE) &&
                !desired.DisplayConfigPaths.Any(path => path.Flags.HasFlag(DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE));
            if (error == WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Info("WinLibrary/ApplyWindowsLayout: Modern topology-only recovery succeeded; flags={0}.", topologyFlags);
                wait(delayInMs);
                diagnostics.Result.TopologyApplied = true;
                return true;
            }
        }

        if (IsWindowsCompatibilityError(error) && !legacyTopologyAlreadyTried)
        {
            DISPLAYCONFIG_PATH_INFO[] legacyInventory;
            try { legacyInventory = readLegacyInventory(); }
            catch (Exception ex)
            {
                diagnostics.Add("LegacyTopology", "Windows", "Could not read current legacy display routes: " + ex.Message, exception: ex);
                diagnostics.Add("Topology", "Windows", "Windows could not apply the display topology; legacy recovery routes are unavailable.",
                    DisplayConfigurationSeverity.Fatal, nativeErrorCode: error.ToString());
                return false;
            }
            if (TryCreateLegacyTopology(desired.DisplayConfigPaths, legacyInventory, out DISPLAYCONFIG_PATH_INFO[] legacyPaths, out string reason))
            {
                const SDC legacyFlags = SDC.SDC_TOPOLOGY_SUPPLIED | SDC.SDC_ALLOW_CHANGES;
                SharedLogger.logger.Warn("WinLibrary/ApplyWindowsLayout: Trying final legacy topology compatibility route after {0}; paths={1}.", error, legacyPaths.Length);
                error = setConfiguration((uint)legacyPaths.Length, legacyPaths.ToArray(), 0, null, legacyFlags | SDC.SDC_VALIDATE);
                SharedLogger.logger.Log(error == WIN32STATUS.ERROR_SUCCESS ? NLog.LogLevel.Trace : NLog.LogLevel.Warn, "WinLibrary/ApplyWindowsLayout: Legacy topology validation returned {0}.", error);
                if (error == WIN32STATUS.ERROR_SUCCESS)
                {
                    error = setConfiguration((uint)legacyPaths.Length, legacyPaths.ToArray(), 0, null, legacyFlags | SDC.SDC_APPLY);
                    SharedLogger.logger.Log(error == WIN32STATUS.ERROR_SUCCESS ? NLog.LogLevel.Info : NLog.LogLevel.Warn, "WinLibrary/ApplyWindowsLayout: Final legacy topology apply returned {0}.", error);
                    if (error == WIN32STATUS.ERROR_SUCCESS)
                    {
                        legacyApplied?.Invoke(legacyPaths.ToArray());
                        wait(delayInMs);
                        diagnostics.Result.TopologyApplied = true;
                        return true;
                    }
                }
            }
            else
            {
                diagnostics.Add("LegacyTopology", "Windows", "Legacy topology conversion is unavailable: " + reason,
                    nativeErrorCode: error.ToString());
            }
        }
        diagnostics.Add("Topology", "Windows", "Windows could not apply the display topology after the available attempts.",
            DisplayConfigurationSeverity.Fatal, nativeErrorCode: error.ToString());
        return false;
    }

    private static bool IsWindowsCompatibilityError(WIN32STATUS error) =>
        error == WIN32STATUS.ERROR_INVALID_PARAMETER || error == WIN32STATUS.ERROR_NOT_SUPPORTED;

    internal static bool TryCreateLegacyTopology(DISPLAYCONFIG_PATH_INFO[] savedPaths, DISPLAYCONFIG_PATH_INFO[] inventory,
        out DISPLAYCONFIG_PATH_INFO[] legacyPaths, out string reason)
    {
        legacyPaths = Array.Empty<DISPLAYCONFIG_PATH_INFO>();
        reason = string.Empty;
        DISPLAYCONFIG_PATH_INFO[] desired = savedPaths.Where(path => path.Flags.HasFlag(DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_ACTIVE)).ToArray();
        if (desired.Length == 0) { reason = "There are no active paths to recover."; return false; }
        Dictionary<(ulong Adapter, uint Value, int Kind), int> groups = new();
        int[] pathGroups = new int[desired.Length];
        List<List<(ulong Adapter, uint Source)>> choices = new();
        HashSet<(ulong Adapter, uint Target)> targets = new();
        for (int index = 0; index < desired.Length; index++)
        {
            DISPLAYCONFIG_PATH_INFO path = desired[index];
            if (!targets.Add((path.TargetInfo.AdapterId.Value, path.TargetInfo.Id)))
            { reason = "A saved target appears more than once."; return false; }
            (ulong Adapter, uint Value, int Kind) key;
            if (path.Flags.HasFlag(DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE) && path.SourceInfo.sourceModeInfoIdx != ushort.MaxValue)
                key = (path.SourceInfo.AdapterId.Value, path.SourceInfo.sourceModeInfoIdx, 0);
            else if (path.Flags.HasFlag(DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE) && path.SourceInfo.cloneGroupId != ushort.MaxValue)
                key = (0, path.SourceInfo.cloneGroupId, 1);
            else if (path.SourceInfo.Id != uint.MaxValue)
                key = (path.SourceInfo.AdapterId.Value, path.SourceInfo.Id, 2);
            else { reason = "A saved path has no source or clone identity."; return false; }

            // Use Windows' own legacy routes. SUPPORT_VIRTUAL_MODE is reporting-only:
            // do not clear it by hand or submit packed indexes as legacy indexes.
            List<(ulong Adapter, uint Source)> available = inventory.Where(candidate =>
                !candidate.Flags.HasFlag(DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE) &&
                candidate.TargetInfo.TargetAvailable &&
                candidate.TargetInfo.AdapterId.Equals(path.TargetInfo.AdapterId) && candidate.TargetInfo.Id == path.TargetInfo.Id &&
                candidate.SourceInfo.AdapterId.Equals(path.SourceInfo.AdapterId) && candidate.SourceInfo.Id != uint.MaxValue)
                .Select(candidate => (candidate.SourceInfo.AdapterId.Value, candidate.SourceInfo.Id)).Distinct()
                .OrderByDescending(source => source.Id == path.SourceInfo.Id).ToList();
            if (!groups.TryGetValue(key, out int group))
            {
                group = groups.Count;
                groups.Add(key, group);
                choices.Add(available);
            }
            else choices[group] = choices[group].Intersect(available).ToList();
            if (choices[group].Count == 0)
            { reason = "The requested outputs have no common legacy source for their saved clone relationship."; return false; }
            pathGroups[index] = group;
        }

        // Matching avoids a greedy choice accidentally consuming the only route
        // available to another extended group. Different groups must remain distinct.
        Dictionary<(ulong Adapter, uint Source), int> owners = new();
        (ulong Adapter, uint Source)[] assignments = new (ulong, uint)[groups.Count];
        bool Assign(int group, HashSet<(ulong Adapter, uint Source)> visited)
        {
            foreach (var source in choices[group])
            {
                if (!visited.Add(source)) continue;
                if (!owners.TryGetValue(source, out int owner) || Assign(owner, visited))
                {
                    owners[source] = group;
                    assignments[group] = source;
                    return true;
                }
            }
            return false;
        }
        for (int group = 0; group < groups.Count; group++)
            if (!Assign(group, new HashSet<(ulong, uint)>()))
            { reason = "There are not enough distinct legacy sources to preserve the extended groups."; return false; }

        DISPLAYCONFIG_PATH_INFO[] converted = new DISPLAYCONFIG_PATH_INFO[desired.Length];
        for (int index = 0; index < desired.Length; index++)
        {
            DISPLAYCONFIG_PATH_INFO path = desired[index];
            var source = assignments[pathGroups[index]];
            DISPLAYCONFIG_PATH_INFO candidate = inventory.First(candidate =>
                !candidate.Flags.HasFlag(DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE) &&
                candidate.TargetInfo.TargetAvailable && candidate.TargetInfo.AdapterId.Equals(path.TargetInfo.AdapterId) &&
                candidate.TargetInfo.Id == path.TargetInfo.Id && candidate.SourceInfo.AdapterId.Value == source.Adapter && candidate.SourceInfo.Id == source.Source);
            candidate.Flags = (candidate.Flags | DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_ACTIVE) & ~DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_BOOST_REFRESH_RATE;
            candidate.SourceInfo.ModeInfoIdx = CCDImport.DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
            candidate.TargetInfo.ModeInfoIdx = CCDImport.DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
            candidate.TargetInfo.Rotation = path.TargetInfo.Rotation;
            candidate.TargetInfo.Scaling = path.TargetInfo.Scaling;
            candidate.TargetInfo.RefreshRate = path.TargetInfo.RefreshRate;
            candidate.TargetInfo.ScanLineOrdering = path.TargetInfo.ScanLineOrdering;
            converted[index] = candidate;
        }
        legacyPaths = converted;
        return true;
    }
}
