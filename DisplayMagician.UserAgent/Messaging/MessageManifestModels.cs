using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DisplayMagician.Messaging
{
    public sealed class ClientSyncDocument
    {
        [JsonProperty("schemaVersion")]
        public int SchemaVersion { get; set; }

        [JsonProperty("publishedUtc")]
        public DateTime? PublishedUtc { get; set; }

        [JsonProperty("updates")]
        public ClientSyncUpdates Updates { get; set; } = new ClientSyncUpdates();

        [JsonProperty("messages")]
        public List<MessageManifestEntry> Messages { get; set; } = new List<MessageManifestEntry>();
    }

    public sealed class ClientSyncUpdates
    {
        [JsonProperty("stable")]
        public ClientSyncUpdate Stable { get; set; } = new ClientSyncUpdate();

        [JsonProperty("prerelease")]
        public ClientSyncUpdate Prerelease { get; set; } = new ClientSyncUpdate();
    }

    public sealed class ClientSyncUpdate
    {
        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty("changelog")]
        public string Changelog { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("mandatory")]
        public ClientSyncMandatory Mandatory { get; set; } = new ClientSyncMandatory();

        [JsonProperty("checksum")]
        public ClientSyncChecksum Checksum { get; set; } = new ClientSyncChecksum();
    }

    public sealed class ClientSyncMandatory
    {
        [JsonProperty("value")]
        public bool Value { get; set; }

        [JsonProperty("mode")]
        public int Mode { get; set; }

        [JsonProperty("minVersion")]
        public string? MinVersion { get; set; }
    }

    public sealed class ClientSyncChecksum
    {
        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;

        [JsonProperty("hashingAlgorithm")]
        public string HashingAlgorithm { get; set; } = string.Empty;
    }

    public sealed class MessageManifestDocument
    {
        [JsonProperty("schemaVersion")]
        public int SchemaVersion { get; set; }

        [JsonProperty("generatedUtc")]
        public DateTime? GeneratedUtc { get; set; }

        [JsonProperty("messages")]
        public List<MessageManifestEntry> Messages { get; set; } = new List<MessageManifestEntry>();
    }

    public sealed class MessageManifestEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = "published";

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("format")]
        public string Format { get; set; } = string.Empty;

        [JsonProperty("sha256")]
        public string Sha256 { get; set; } = string.Empty;

        [JsonProperty("showOnStartup")]
        public bool ShowOnStartup { get; set; }

        [JsonProperty("publishedUtc")]
        public DateTime? PublishedUtc { get; set; }

        [JsonProperty("deletedUtc")]
        public DateTime? DeletedUtc { get; set; }

        [JsonProperty("minVersion")]
        public string? MinVersion { get; set; }

        [JsonProperty("maxVersion")]
        public string? MaxVersion { get; set; }

        [JsonProperty("startUtc")]
        public DateTime? StartUtc { get; set; }

        [JsonProperty("endUtc")]
        public DateTime? EndUtc { get; set; }

        [JsonProperty("vendors")]
        public List<string> Vendors { get; set; } = new List<string>();

        [JsonProperty("kind")]
        public string Kind { get; set; } = "standard";

        [JsonProperty("releaseVersion")]
        public string? ReleaseVersion { get; set; }

        [JsonProperty("releaseChannel")]
        public string? ReleaseChannel { get; set; }

        [JsonProperty("githubReleaseId")]
        public long? GithubReleaseId { get; set; }

        [JsonProperty("updateAction")]
        public string? UpdateAction { get; set; }

        [JsonProperty("media")]
        public List<MessageManifestMedia> Media { get; set; } = new List<MessageManifestMedia>();
    }

    public sealed class MessageManifestMedia
    {
        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("sha256")]
        public string Sha256 { get; set; } = string.Empty;

        [JsonProperty("contentType")]
        public string ContentType { get; set; } = string.Empty;
    }
}
