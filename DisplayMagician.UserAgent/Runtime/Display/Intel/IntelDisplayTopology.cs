using System;
using System.Collections.Generic;
using System.Linq;
using IGCLWrapper;

namespace DisplayMagician.UserAgent.Runtime.Intel;

internal static class IntelDisplayTopology
{
    internal static List<string> GetHiddenIdentifiers(INTEL_DISPLAY_CONFIG desired, INTEL_DISPLAY_CONFIG active,
        IEnumerable<string> connectedIdentifiers, IEnumerable<string> candidateIdentifiers)
    {
        List<string> hidden = new();
        if (!desired.IsInUse || (!desired.CombinedDisplayIsInUse && !active.CombinedDisplayIsInUse)) return hidden;
        HashSet<(string Adapter, uint Encoder)> required = new();
        HashSet<(string Adapter, uint Encoder)> topology = new();
        HashSet<(string Adapter, uint Encoder)> connected = new();
        foreach (string identifier in desired.DisplayIdentifiers ?? new List<string>())
            if (IntelAdapterIdentity.TryGetOutput(identifier, out string adapter, out uint encoder)) required.Add((adapter, encoder));
        foreach (string identifier in connectedIdentifiers)
            if (IntelAdapterIdentity.TryGetOutput(identifier, out string adapter, out uint encoder)) connected.Add((adapter, encoder));

        if (!AddCombinedOutputs(desired, required, topology, null) || !AddCombinedOutputs(active, null, topology, connected) ||
            required.Count == 0 || !required.IsSubsetOf(connected)) return hidden;

        string[] adapterIds = (desired.PhysicalAdapters?.Keys ?? Enumerable.Empty<string>())
            .Concat(active.PhysicalAdapters?.Keys ?? Enumerable.Empty<string>()).Distinct(StringComparer.Ordinal).ToArray();
        foreach (string identifier in candidateIdentifiers.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            if (IntelAdapterIdentity.TryGetOutput(identifier, out string adapter, out uint encoder))
            {
                if (topology.Contains((adapter, encoder))) hidden.Add(identifier);
                continue;
            }
            if (!identifier.StartsWith("WINAPI|", StringComparison.OrdinalIgnoreCase)) continue;
            string[] parts = identifier.Split('|');
            if (parts.Length < 4 || !uint.TryParse(parts[3], out encoder)) continue;
            // Windows target IDs are adapter-scoped too. The interface path identifies
            // a hardware model; suppress only when that model names one physical card.
            // Identical cards with ambiguous Windows paths retain their advisory.
            string[] matches = adapterIds.Where(id => MatchesWindowsAdapter(id, parts[1])).ToArray();
            if (matches.Length == 1 && topology.Contains((matches[0], encoder))) hidden.Add(identifier);
        }
        if (hidden.Count > 0)
            SharedLogger.logger.Trace("IntelDisplayTopology/GetHiddenIdentifiers: Ignoring {0} logical-monitor identifier(s) hidden by the matching Intel Combined Display topology.", hidden.Count);
        return hidden;
    }

    private static bool AddCombinedOutputs(INTEL_DISPLAY_CONFIG config, HashSet<(string Adapter, uint Encoder)>? required,
        HashSet<(string Adapter, uint Encoder)> topology, HashSet<(string Adapter, uint Encoder)>? connected)
    {
        if (!config.CombinedDisplayIsInUse) return true;
        var adapters = config.PhysicalAdapters?.Where(pair => pair.Value.IsCombinedDisplay).ToArray();
        if (adapters == null || adapters.Length == 0) return false;
        foreach (var pair in adapters)
        {
            CombinedDisplayArgsDto group = pair.Value.CombinedDisplay;
            if (group.ChildInfos == null || group.ChildInfos.Count == 0) return false;
            var combined = (pair.Key, group.CombinedDisplayOutputWindowsDisplayEncoderId);
            topology.Add(combined);
            required?.Remove(combined);
            foreach (CombinedDisplayChildInfoDto child in group.ChildInfos)
            {
                var output = (pair.Key, child.DisplayOutputWindowsDisplayEncoderId);
                topology.Add(output);
                required?.Add(output);
                connected?.Add(output);
            }
        }
        return true;
    }

    private static bool MatchesWindowsAdapter(string adapterId, string devicePath)
    {
        string[] parts = adapterId.Split(new[] { "-PCI_" }, StringSplitOptions.None)[0].Split('_');
        if (parts.Length != 5) return false;
        return devicePath.IndexOf("VEN_" + parts[0] + "&DEV_" + parts[1], StringComparison.OrdinalIgnoreCase) >= 0 &&
            devicePath.IndexOf("SUBSYS_" + parts[3] + parts[2], StringComparison.OrdinalIgnoreCase) >= 0 &&
            devicePath.IndexOf("REV_" + parts[4], StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
