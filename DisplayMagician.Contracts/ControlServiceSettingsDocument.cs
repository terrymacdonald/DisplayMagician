using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DisplayMagician.Contracts;

/// <summary>Shared, non-secret machine preferences. Operational records have independent stores.</summary>
public sealed class ControlServiceSettingsDocument
{
    [JsonRequired]
    public int SchemaVersion { get; set; } = 1;
    [JsonRequired]
    public GatewaySettings Gateway { get; set; } = new GatewaySettings();
    [JsonRequired]
    public SharedLoggingPreferences Logging { get; set; } = new SharedLoggingPreferences();
    [JsonRequired]
    public bool ShareAnonymousUsageMetrics { get; set; } = true;
    public bool HasAnonymousMetricsChoice { get; set; }
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalSettings { get; set; }

    public bool IsValid() => SchemaVersion == 1 && Gateway != null && Logging != null
        && SharedLoggingSettings.IsLevel(Logging.SavedLevel);
}

public sealed class SharedLoggingPreferences
{
    public string SavedLevel { get; set; } = "Warn";
    public bool HasSavedLevel { get; set; }
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalSettings { get; set; }
}

/// <summary>Frequently renewed diagnostic leases; deliberately contains no saved preferences.</summary>
public sealed class DiagnosticLoggingState
{
    [JsonRequired]
    public int SchemaVersion { get; set; } = 1;
    [JsonRequired]
    public DiagnosticLoggingSession[] Sessions { get; set; } = System.Array.Empty<DiagnosticLoggingSession>();

    public bool IsValid() => SchemaVersion == 1 && new SharedLoggingSettings { Sessions = Sessions }.IsValid();
}
