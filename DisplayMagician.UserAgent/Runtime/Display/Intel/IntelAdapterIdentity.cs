using System;
using System.Collections.Generic;
using System.Linq;
using IGCLWrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisplayMagician.UserAgent.Runtime.Intel;

/// <summary>Persistent PCI location, rather than enumeration order or a temporary driver handle.</summary>
internal static class IntelAdapterIdentity
{
    internal static string LegacyKey(DeviceAdapterPropertiesDto properties) =>
        $"{properties.PciVendorId:X4}_{properties.PciDeviceId:X4}_{properties.PciSubsysVendorId:X4}_{properties.PciSubsysId:X4}_{properties.RevId:X2}";

    internal static string Key(DeviceAdapterPropertiesDto properties) =>
        $"{LegacyKey(properties)}-PCI_{properties.AdapterBdf.Bus:X2}_{properties.AdapterBdf.Device:X2}_{properties.AdapterBdf.Function:X2}";

    internal static string DisplayKey(DeviceAdapterPropertiesDto properties, uint encoderId) =>
        $"{Key(properties)}-PORT_{encoderId}";

    internal const string RecaptureMessage = "This Intel display profile has incomplete or ambiguous saved adapter identity. Recapture the display profile before applying it.";

    internal static bool RequiresRecapture(INTEL_DISPLAY_CONFIG config) => config.IsInUse &&
        (config.IdentityRequiresRecapture || (config.PhysicalAdapters != null && config.PhysicalAdapters.Keys.Any(key => !key.Contains("-PCI_", StringComparison.Ordinal))) ||
         (config.Displays != null && config.Displays.Keys.Any(key => !key.Contains("-PCI_", StringComparison.Ordinal))));

    internal static bool ValidateForApply(INTEL_DISPLAY_CONFIG config, DisplayConfigurationDiagnostics diagnostics)
    {
        if (!RequiresRecapture(config)) return true;
        diagnostics.Add("AdapterIdentity", "Intel",
            RecaptureMessage,
            DisplayMagician.Contracts.DisplayConfigurationSeverity.Fatal);
        return false;
    }

    // Work on JSON so unknown settings and the distinction between missing BDF and PCI 00:00.0 survive.
    internal static void MigrateProfile(JObject profile)
    {
        if (profile["IntelDisplayConfig"] is not JObject config || config["PhysicalAdapters"] is not JObject adapters) return;
        bool previouslyRequiredRecapture = (bool?)config["IdentityRequiresRecapture"] == true;
        Dictionary<string, string> remap = new(StringComparer.Ordinal);
        foreach (JProperty entry in adapters.Properties().Where(entry => !entry.Name.StartsWith("$", StringComparison.Ordinal)).ToList())
        {
            if (entry.Value is not JObject adapter) continue;
            if (previouslyRequiredRecapture) continue;
            if (entry.Name.Contains("-PCI_", StringComparison.Ordinal))
            {
                // The saved key already contains its captured location. Do not replace
                // it from optional/defaulted adapter-property fields on later reads.
                remap.Add(entry.Name, entry.Name);
                adapter["AdapterID"] = entry.Name;
                continue;
            }
            if (adapter["AdapterProperties"] is not JObject properties ||
                properties["AdapterBdf"] is not JObject bdf || bdf["Bus"] == null || bdf["Device"] == null || bdf["Function"] == null)
            {
                config["IdentityRequiresRecapture"] = true;
                continue;
            }
            DeviceAdapterPropertiesDto storedProperties = properties.ToObject<DeviceAdapterPropertiesDto>();
            string key = Key(storedProperties);
            if (entry.Name != key && adapters.Property(key) != null)
                throw new JsonSerializationException($"Intel adapter identity migration would merge distinct saved entries: {key}.");
            remap.Add(entry.Name, key);
            adapter["AdapterID"] = key;
            if (entry.Name != key)
            {
                JToken value = entry.Value;
                entry.Remove();
                adapters.Add(key, value);
            }
        }
        if (config["Displays"] is not JObject displays)
        {
            MigrateIdentifiers(profile, config, adapters);
            return;
        }
        foreach (JProperty entry in displays.Properties().Where(entry => !entry.Name.StartsWith("$", StringComparison.Ordinal)).ToList())
        {
            if (entry.Value is not JObject display || !remap.TryGetValue((string?)display["DeviceID"] ?? "", out string? adapterKey)) continue;
            int port = entry.Name.LastIndexOf("-PORT_", StringComparison.Ordinal);
            if (port < 0) continue;
            string key = adapterKey + entry.Name.Substring(port);
            if (entry.Name != key && displays.Property(key) != null)
                throw new JsonSerializationException($"Intel display identity migration would merge distinct saved entries: {key}.");
            display["DeviceID"] = adapterKey;
            display["DisplayDeviceID"] = key;
            if (entry.Name != key)
            {
                JToken value = entry.Value;
                entry.Remove();
                displays.Add(key, value);
            }
        }
        MigrateIdentifiers(profile, config, adapters);
    }

    internal static string ScopeIdentifier(string identifier, string adapterId)
    {
        if (TryGetOutput(identifier, out _, out _)) return identifier;
        int separator = identifier.LastIndexOf('#');
        return separator < 0 ? identifier : identifier.Insert(separator, "#ADAPTER=" + adapterId);
    }

    internal static bool TryGetOutput(string identifier, out string adapterId, out uint encoderId)
    {
        adapterId = string.Empty;
        encoderId = 0;
        if (!identifier.StartsWith("IntelIGCL#", StringComparison.OrdinalIgnoreCase)) return false;
        int marker = identifier.LastIndexOf("#ADAPTER=", StringComparison.Ordinal);
        int separator = identifier.LastIndexOf('#');
        if (marker < 0 || separator <= marker + 9 || !uint.TryParse(identifier.Substring(separator + 1), out encoderId)) return false;
        adapterId = identifier.Substring(marker + 9, separator - marker - 9);
        return adapterId.Length > 0;
    }

    private static void MigrateIdentifiers(JObject profile, JObject config, JObject adapters)
    {
        foreach (JToken? collection in new[] { config["DisplayIdentifiers"], profile["ProfileDisplayIdentifiers"] })
        {
            JArray? identifiers = collection as JArray ?? (collection as JObject)?["$values"] as JArray;
            if (identifiers == null) continue;
            for (int index = 0; index < identifiers.Count; index++)
            {
                if (identifiers[index].Type != JTokenType.String) continue;
                string identifier = (string)identifiers[index]!;
                if (!identifier.StartsWith("IntelIGCL#", StringComparison.OrdinalIgnoreCase) || TryGetOutput(identifier, out _, out _)) continue;
                string[] parts = identifier.Split('#');
                List<string> matches = adapters.Properties().Where(entry => entry.Value is JObject && !entry.Name.StartsWith("$", StringComparison.Ordinal))
                    .Where(entry => parts.Length == 11 &&
                        string.Equals((string?)entry.Value["Name"], parts[1], StringComparison.Ordinal) &&
                        (string?)entry.Value["AdapterProperties"]?["PciDeviceId"] == parts[2] &&
                        (string?)entry.Value["AdapterProperties"]?["PciSubsysId"] == parts[3] &&
                        entry.Name.Contains("-PCI_", StringComparison.Ordinal)).Select(entry => entry.Name).ToList();
                if (matches.Count == 1) identifiers[index] = ScopeIdentifier(identifier, matches[0]);
                else config["IdentityRequiresRecapture"] = true; // The old identifier cannot distinguish these adapters.
            }
        }
    }
}
