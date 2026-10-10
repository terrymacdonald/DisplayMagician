using System;
using System.Text.Json.Serialization;

namespace DisplayMagician.Contracts;

public sealed class DisplayProfileSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsValid { get; set; }
    public int ConnectedDisplayCount { get; set; }
    public string Href { get; set; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ThumbnailPngBase64 { get; set; }
}

public sealed class DisplayProfileDetail
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsValid { get; set; }
    public int ConnectedDisplayCount { get; set; }
    public string Href { get; set; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ThumbnailPngBase64 { get; set; }
    public int PrimaryDisplayWidth { get; set; }
    public int PrimaryDisplayHeight { get; set; }
    public string DiagnosticMessage { get; set; } = string.Empty;
}

public sealed class DisplayProfilePage
{
    public DisplayProfileSummary[] Items { get; set; } = Array.Empty<DisplayProfileSummary>();
    public string? NextCursor { get; set; }
}

public sealed class AudioProfileSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Href { get; set; } = string.Empty;
}

public sealed class AudioProfileDetail
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Href { get; set; } = string.Empty;
    public string SettingsSummary { get; set; } = string.Empty;
    public string[] UnavailableDeviceNames { get; set; } = Array.Empty<string>();
}

public sealed class AudioProfilePage
{
    public AudioProfileSummary[] Items { get; set; } = Array.Empty<AudioProfileSummary>();
    public string? NextCursor { get; set; }
}

public sealed class ShortcutSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IconPngBase64 { get; set; }
}

public sealed class ShortcutDetail
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IconPngBase64 { get; set; }
    public string? DisplayProfileId { get; set; }
}

public sealed class ShortcutPage
{
    public ShortcutSummary[] Items { get; set; } = Array.Empty<ShortcutSummary>();
    public string? NextCursor { get; set; }
}

public sealed class OperationAccepted
{
    public Guid OperationId { get; set; }
    public string Status { get; set; } = "requested";
    public string Href { get; set; } = string.Empty;
}

public sealed class GatewayOperationView
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Phase { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public long Sequence { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsFinished { get; set; }
    public bool IsStale { get; set; }
    public string? StaleReason { get; set; }
    public bool CanCancel { get; set; }
    public GatewayOperationSource? Source { get; set; }
    public GatewayOperationError? Error { get; set; }
    public GatewayOperationLinks Links { get; set; } = new GatewayOperationLinks();
}

public sealed class GatewayOperationSource
{
    public string Type { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
}

public sealed class GatewayOperationError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Retryable { get; set; }
}

public sealed class GatewayOperationLinks
{
    public string Self { get; set; } = string.Empty;
    public string Decisions { get; set; } = string.Empty;
    public string Cancellations { get; set; } = string.Empty;
}

public sealed class GatewayOperationPage
{
    public GatewayOperationView[] Items { get; set; } = Array.Empty<GatewayOperationView>();
    public string? NextCursor { get; set; }
}

public sealed class GatewayDecisionView
{
    public Guid Id { get; set; }
    public Guid OperationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string[] AllowedChoices { get; set; } = Array.Empty<string>();
    public string DefaultChoice { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? ResolvedChoice { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string Href { get; set; } = string.Empty;
    public string OperationHref { get; set; } = string.Empty;
}

public sealed class GatewayDecisionPage
{
    public GatewayDecisionView[] Items { get; set; } = Array.Empty<GatewayDecisionView>();
    public string? NextCursor { get; set; }
}

public sealed class GatewayDecisionAnswerRequest
{
    public string Choice { get; set; } = string.Empty;
}
