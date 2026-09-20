using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DisplayMagician.Messaging
{
    public sealed class MessageStoreDocument
    {
        [JsonProperty("schemaVersion")]
        public int SchemaVersion { get; set; } = 1;

        [JsonProperty("lastSuccessfulCheckUtc")]
        public DateTime? LastSuccessfulCheckUtc { get; set; }

        [JsonProperty("lastAttemptCheckUtc")]
        public DateTime? LastAttemptCheckUtc { get; set; }

        [JsonProperty("messages")]
        public List<LocalMessage> Messages { get; set; } = new List<LocalMessage>();
    }

    public sealed class LocalMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("markdownFileName")]
        public string MarkdownFileName { get; set; } = string.Empty;

        [JsonProperty("sourceMarkdownUrl")]
        public string SourceMarkdownUrl { get; set; } = string.Empty;

        [JsonProperty("publishedUtc")]
        public DateTime? PublishedUtc { get; set; }

        [JsonProperty("receivedUtc")]
        public DateTime ReceivedUtc { get; set; } = DateTime.UtcNow;

        [JsonProperty("isRead")]
        public bool IsRead { get; set; }

        [JsonProperty("vendors")]
        public List<string> Vendors { get; set; } = new List<string>();

        [JsonProperty("format")]
        public string Format { get; set; } = string.Empty;

        [JsonProperty("sha256")]
        public string Sha256 { get; set; } = string.Empty;

        [JsonProperty("showOnStartup")]
        public bool ShowOnStartup { get; set; }

        [JsonProperty("downloadAttempts")]
        public int DownloadAttempts { get; set; }

        [JsonProperty("isFaulty")]
        public bool IsFaulty { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; } = "standard";

        [JsonProperty("releaseVersion")]
        public string ReleaseVersion { get; set; }

        [JsonProperty("releaseChannel")]
        public string ReleaseChannel { get; set; }

        [JsonProperty("githubReleaseId")]
        public long? GithubReleaseId { get; set; }

        [JsonProperty("updateAction")]
        public string UpdateAction { get; set; }
    }

    public sealed class MessageSyncResult
    {
        public bool Success { get; set; }
        public int NewMessagesCount { get; set; }
        public int UnreadCount { get; set; }
    }
}
