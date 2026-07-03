using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DisplayMagician.Messaging
{
    public sealed class MessageManifestDocument
    {
        [JsonProperty("schemaVersion")]
        public int SchemaVersion { get; set; }

        [JsonProperty("messages")]
        public List<MessageManifestEntry> Messages { get; set; } = new List<MessageManifestEntry>();
    }

    public sealed class MessageManifestEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("markdownUrl")]
        public string MarkdownUrl { get; set; } = string.Empty;

        [JsonProperty("publishedUtc")]
        public DateTime? PublishedUtc { get; set; }

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
