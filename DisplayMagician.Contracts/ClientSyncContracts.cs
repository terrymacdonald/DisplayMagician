using System;
using System.Collections.Generic;

namespace DisplayMagician.Contracts;

/// <summary>Requests the machine-wide combined client-sync document from the Control Service.</summary>
public sealed class ClientSyncRequest
{
    public bool IsManual { get; set; }

    public bool PreferPrerelease { get; set; }
}

/// <summary>Represents one validated update channel from the combined client-sync document.</summary>
public sealed class ClientSyncUpdateView
{
    public string Version { get; set; } = string.Empty;
    public string Changelog { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool Mandatory { get; set; }
    public int MandatoryMode { get; set; }
    public string MandatoryMinimumVersion { get; set; } = string.Empty;
    public string ChecksumValue { get; set; } = string.Empty;
    public string ChecksumAlgorithm { get; set; } = string.Empty;
}

/// <summary>Contains the client-visible result of one combined client-sync operation.</summary>
public sealed class ClientSyncResult
{
    public bool WasDue { get; set; }
    public ClientSyncUpdateView? StableUpdate { get; set; }
    public ClientSyncUpdateView? PrereleaseUpdate { get; set; }
    public MessageSyncResult? MessageSync { get; set; }
}

/// <summary>Authoritative message snapshot extracted from a validated combined client-sync document.</summary>
public sealed class ClientSyncMessageManifest
{
    public int SchemaVersion { get; set; }
    public DateTime? GeneratedUtc { get; set; }
    public List<ClientSyncMessageEntry> Messages { get; set; } = new List<ClientSyncMessageEntry>();
}

public sealed class ClientSyncMessageEntry
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = "published";
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public bool ShowOnStartup { get; set; }
    public DateTime? PublishedUtc { get; set; }
    public DateTime? DeletedUtc { get; set; }
    public string MinVersion { get; set; } = string.Empty;
    public string MaxVersion { get; set; } = string.Empty;
    public DateTime? StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public List<string> Vendors { get; set; } = new List<string>();
    public string Kind { get; set; } = "standard";
    public string ReleaseVersion { get; set; } = string.Empty;
    public string ReleaseChannel { get; set; } = string.Empty;
    public long? GithubReleaseId { get; set; }
    public string UpdateAction { get; set; } = string.Empty;
    public List<ClientSyncMessageMedia> Media { get; set; } = new List<ClientSyncMessageMedia>();
}

public sealed class ClientSyncMessageMedia
{
    public string Url { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}