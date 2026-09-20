using System;

namespace DisplayMagician.Contracts;

public sealed class AnonymousMetricsSettings
{
    public bool ShareAnonymousUsageMetrics { get; set; } = true;
}

/// <summary>Imports legacy per-user metrics state once into the machine-owned service store.</summary>
public sealed class InitializeAnonymousMetricsRequest
{
    public string InstallId { get; set; } = string.Empty;
    public bool ShareAnonymousUsageMetrics { get; set; } = true;
    public long Launches { get; set; }
    public long ActiveMinutes { get; set; }
    public DateTime? NextHeartbeatUtc { get; set; }
    public string LastReportedVersion { get; set; } = string.Empty;
}

public sealed class AnonymousMetricsUsageReport
{
    public string AppVersion { get; set; } = string.Empty;
    public string UpdateChannel { get; set; } = "stable";
    public bool IsLaunch { get; set; }
    public long ActiveMinutes { get; set; }
}