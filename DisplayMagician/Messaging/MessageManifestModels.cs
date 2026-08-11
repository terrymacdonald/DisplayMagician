using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DisplayMagician.Messaging
{
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
        public string MinVersion { get; set; }

        [JsonProperty("maxVersion")]
        public string MaxVersion { get; set; }

        [JsonProperty("startUtc")]
        public DateTime? StartUtc { get; set; }

        [JsonProperty("endUtc")]
        public DateTime? EndUtc { get; set; }

        [JsonProperty("vendors")]
        public List<string> Vendors { get; set; } = new List<string>();
    }
}
