using System;
using System.Linq;

namespace DisplayMagician.Contracts;

public enum DisplayConfigurationSeverity
{
    Warning,
    Fatal
}

/// <summary>A failed display stage. Native codes are populated only when supplied by the API.</summary>
public sealed class DisplayConfigurationIssue
{
    public DisplayConfigurationSeverity Severity { get; set; }
    public string Stage { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string DisplayId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? NativeErrorCode { get; set; }
    public int? ExceptionHResult { get; set; }

    public DisplayConfigurationIssue Copy() => (DisplayConfigurationIssue)MemberwiseClone();
}

/// <summary>Additive diagnostics for a capture, apply or restoration, independent of operation progress.</summary>
public sealed class DisplayConfigurationResult
{
    public string ProfileId { get; set; } = string.Empty;
    public bool IsCapture { get; set; }
    public bool IsRestoration { get; set; }
    public int Attempt { get; set; } = 1;
    public bool TopologyApplied { get; set; }
    public DisplayConfigurationIssue[] Issues { get; set; } = Array.Empty<DisplayConfigurationIssue>();

    public bool IsSuccessful => !Issues.Any(issue => issue.Severity == DisplayConfigurationSeverity.Fatal);
    public bool HasWarnings => Issues.Any(issue => issue.Severity == DisplayConfigurationSeverity.Warning);

    public DisplayConfigurationResult Copy() => new DisplayConfigurationResult
    {
        ProfileId = ProfileId, IsCapture = IsCapture, IsRestoration = IsRestoration,
        Attempt = Attempt, TopologyApplied = TopologyApplied,
        Issues = Issues.Select(issue => issue.Copy()).ToArray()
    };
}
