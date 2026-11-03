using System;
using System.Collections.Generic;
using System.Linq;
using IGCLWrapper;

namespace DisplayMagician.UserAgent.Runtime.Intel;

internal static class IntelDisplayTopology
{
    internal static List<string> GetHiddenIdentifiers(INTEL_DISPLAY_CONFIG displayConfig, INTEL_DISPLAY_CONFIG activeDisplayConfig, IEnumerable<string> connectedIdentifiers, IEnumerable<string> candidateIdentifiers)
    {
        List<string> topologyHiddenIdentifiers = new List<string>();

        if (!displayConfig.IsInUse || candidateIdentifiers == null)
            return topologyHiddenIdentifiers;

        if (!displayConfig.CombinedDisplayIsInUse && !activeDisplayConfig.CombinedDisplayIsInUse)
            return topologyHiddenIdentifiers;

        HashSet<uint> requiredPhysicalEncoderIds = new HashSet<uint>();
        foreach (string displayIdentifier in displayConfig.DisplayIdentifiers ?? new List<string>())
        {
            if (TryGetWindowsDisplayEncoderId(displayIdentifier, out uint displayEncoderId))
                requiredPhysicalEncoderIds.Add(displayEncoderId);
        }

        HashSet<uint> topologyHiddenEncoderIds = new HashSet<uint>();
        HashSet<uint> connectedEncoderIds = new HashSet<uint>(connectedIdentifiers
            .Select(identifier => TryGetWindowsDisplayEncoderId(identifier, out uint displayEncoderId) ? displayEncoderId : 0)
            .Where(displayEncoderId => displayEncoderId != 0));

        if (!AddCombinedDisplayEncoderIds(displayConfig, requiredPhysicalEncoderIds, topologyHiddenEncoderIds, true))
            return topologyHiddenIdentifiers;

        if (!AddCombinedDisplayEncoderIds(activeDisplayConfig, requiredPhysicalEncoderIds, topologyHiddenEncoderIds, false, connectedEncoderIds))
            return topologyHiddenIdentifiers;

        if (requiredPhysicalEncoderIds.Count == 0)
        {
            SharedLogger.logger.Warn("IntelDisplayTopology/GetHiddenIdentifiers: No physical Intel display outputs were available to validate the combined display topology.");
            return topologyHiddenIdentifiers;
        }

        if (!requiredPhysicalEncoderIds.All(connectedEncoderIds.Contains))
        {
            SharedLogger.logger.Trace("IntelDisplayTopology/GetHiddenIdentifiers: A required physical Intel display output is unavailable, so retaining the missing-display warning.");
            return topologyHiddenIdentifiers;
        }

        foreach (string candidateIdentifier in candidateIdentifiers.Where(identifier => !String.IsNullOrWhiteSpace(identifier)))
        {
            if (!TryGetWindowsDisplayEncoderId(candidateIdentifier, out uint displayEncoderId))
                continue;

            bool isWindowsLogicalMonitor = candidateIdentifier.StartsWith("WINAPI|", StringComparison.OrdinalIgnoreCase);
            bool isTopologyHiddenIntelOutput = candidateIdentifier.StartsWith("IntelIGCL#", StringComparison.OrdinalIgnoreCase) && topologyHiddenEncoderIds.Contains(displayEncoderId);

            if ((isWindowsLogicalMonitor && topologyHiddenEncoderIds.Contains(displayEncoderId)) || isTopologyHiddenIntelOutput)
                topologyHiddenIdentifiers.Add(candidateIdentifier);
        }

        if (topologyHiddenIdentifiers.Count > 0)
            SharedLogger.logger.Trace($"IntelDisplayTopology/GetHiddenIdentifiers: Ignoring {topologyHiddenIdentifiers.Count} expected logical-monitor identifier(s) hidden by the Intel Combined Display topology.");

        return topologyHiddenIdentifiers;
    }

    private static bool AddCombinedDisplayEncoderIds(INTEL_DISPLAY_CONFIG displayConfig, HashSet<uint> requiredPhysicalEncoderIds, HashSet<uint> topologyHiddenEncoderIds, bool updateRequiredPhysicalEncoderIds, HashSet<uint>? activeCombinedChildEncoderIds = null)
    {
        if (!displayConfig.CombinedDisplayIsInUse)
            return true;

        List<INTEL_ADAPTER> combinedDisplayAdapters = displayConfig.PhysicalAdapters?.Values
            .Where(adapter => adapter.IsCombinedDisplay)
            .ToList() ?? new List<INTEL_ADAPTER>();

        if (combinedDisplayAdapters.Count == 0)
        {
            SharedLogger.logger.Warn("IntelDisplayTopology/AddCombinedDisplayEncoderIds: The Intel combined display configuration did not contain a combined display adapter.");
            return false;
        }

        foreach (INTEL_ADAPTER adapter in combinedDisplayAdapters)
        {
            if (adapter.CombinedDisplay.ChildInfos == null || adapter.CombinedDisplay.ChildInfos.Count == 0)
            {
                SharedLogger.logger.Warn($"IntelDisplayTopology/AddCombinedDisplayEncoderIds: The Intel combined display on adapter {adapter.Name} did not contain component display outputs.");
                return false;
            }

            uint combinedDisplayEncoderId = adapter.CombinedDisplay.CombinedDisplayOutputWindowsDisplayEncoderId;
            topologyHiddenEncoderIds.Add(combinedDisplayEncoderId);
            if (updateRequiredPhysicalEncoderIds)
                requiredPhysicalEncoderIds.Remove(combinedDisplayEncoderId);

            foreach (CombinedDisplayChildInfoDto childInfo in adapter.CombinedDisplay.ChildInfos)
            {
                uint childDisplayEncoderId = childInfo.DisplayOutputWindowsDisplayEncoderId;
                topologyHiddenEncoderIds.Add(childDisplayEncoderId);
                activeCombinedChildEncoderIds?.Add(childDisplayEncoderId);
                if (updateRequiredPhysicalEncoderIds)
                    requiredPhysicalEncoderIds.Add(childDisplayEncoderId);
            }
        }

        return true;
    }

    private static bool TryGetWindowsDisplayEncoderId(string displayIdentifier, out uint displayEncoderId)
    {
        displayEncoderId = 0;
        if (String.IsNullOrWhiteSpace(displayIdentifier))
            return false;

        if (displayIdentifier.StartsWith("WINAPI|", StringComparison.OrdinalIgnoreCase))
        {
            string[] windowsIdentifierParts = displayIdentifier.Split('|');
            return windowsIdentifierParts.Length > 3 && UInt32.TryParse(windowsIdentifierParts[3], out displayEncoderId);
        }

        if (displayIdentifier.StartsWith("IntelIGCL#", StringComparison.OrdinalIgnoreCase))
        {
            int finalSeparatorIndex = displayIdentifier.LastIndexOf('#');
            return finalSeparatorIndex >= 0 && UInt32.TryParse(displayIdentifier.Substring(finalSeparatorIndex + 1), out displayEncoderId);
        }

        return false;
    }

}
