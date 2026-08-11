using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagicianShared.Windows;
using Newtonsoft.Json;

namespace DisplayMagician.Messaging
{
    public sealed class MessageSyncService
    {
        private const int CurrentSchemaVersion = 1;
        private const int MaxStoredMessages = 50;
        private static readonly TimeSpan DailyInterval = TimeSpan.FromHours(24);
        private static readonly HashSet<string> SupportedFormats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "html",
            "md",
            "markdown"
        };

        private readonly HttpClient _httpClient;
        private readonly NLog.Logger _logger;
        private readonly string _manifestUrl;
        private readonly string _messagesFolderPath;
        private readonly string _storePath;
        private readonly object _storeLock = new object();

        public MessageSyncService(HttpClient httpClient, NLog.Logger logger, string manifestUrl, string messagesFolderPath)
        {
            _httpClient = httpClient;
            _logger = logger;
            _manifestUrl = manifestUrl;
            _messagesFolderPath = messagesFolderPath;
            _storePath = Path.Combine(_messagesFolderPath, "MessagesIndex.json");
        }

        public void EnsureStorage()
        {
            try
            {
                Directory.CreateDirectory(_messagesFolderPath);
                if (!File.Exists(_storePath))
                {
                    SaveStore(new MessageStoreDocument());
                    _logger.Info($"MessageSyncService/EnsureStorage: Created new message index store at {_storePath}.");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"MessageSyncService/EnsureStorage: Failed to initialise message store (messagesFolderPath={_messagesFolderPath}, storePath={_storePath}).");
            }
        }

        public bool IsDailyCheckDue()
        {
            MessageStoreDocument store = LoadStore();
            if (!store.LastSuccessfulCheckUtc.HasValue)
            {
                return true;
            }

            return (DateTime.UtcNow - store.LastSuccessfulCheckUtc.Value) >= DailyInterval;
        }

        public int GetUnreadCount()
        {
            MessageStoreDocument store = LoadStore();
            return store.Messages.Count(m => !m.IsRead);
        }

        public List<LocalMessage> GetMessages()
        {
            MessageStoreDocument store = LoadStore();
            return store.Messages
                .OrderByDescending(m => m.ReceivedUtc)
                .ThenByDescending(m => m.PublishedUtc ?? DateTime.MinValue)
                .ToList();
        }

        public bool SetReadState(IEnumerable<string> ids, bool isRead)
        {
            if (ids == null)
            {
                return false;
            }

            HashSet<string> idSet = new HashSet<string>(ids.Where(i => !string.IsNullOrWhiteSpace(i)), StringComparer.OrdinalIgnoreCase);
            if (idSet.Count == 0)
            {
                return false;
            }

            MessageStoreDocument store = LoadStore();
            bool changed = false;
            foreach (LocalMessage message in store.Messages)
            {
                if (!idSet.Contains(message.Id))
                {
                    continue;
                }

                if (message.IsRead != isRead)
                {
                    message.IsRead = isRead;
                    changed = true;
                }
            }

            if (changed)
            {
                SaveStore(store);
            }

            return changed;
        }

        public async Task<MessageSyncResult> SyncMessagesAsync(string appVersion, CancellationToken cancellationToken)
        {
            MessageStoreDocument store = LoadStore();
            store.LastAttemptCheckUtc = DateTime.UtcNow;
            SaveStore(store);

            _logger.Trace($"MessageSyncService/SyncMessagesAsync: Starting sync (manifestUrl={_manifestUrl}, appVersion={appVersion}, existingMessages={store.Messages.Count}).");

            MessageManifestDocument manifest = await DownloadManifestAsync(cancellationToken).ConfigureAwait(false);
            if (manifest == null)
            {
                _logger.Warn($"MessageSyncService/SyncMessagesAsync: Manifest download/parsing returned null (manifestUrl={_manifestUrl}).");
                return new MessageSyncResult { Success = false, UnreadCount = store.Messages.Count(m => !m.IsRead) };
            }

            if (manifest.SchemaVersion > CurrentSchemaVersion)
            {
                _logger.Warn($"MessageSyncService/SyncMessagesAsync: Unsupported message schema version {manifest.SchemaVersion} (supported up to schema version {CurrentSchemaVersion}, manifestUrl={_manifestUrl}).");
                return new MessageSyncResult { Success = false, UnreadCount = store.Messages.Count(m => !m.IsRead) };
            }

            HashSet<string> seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> currentVendorIds = GetCurrentVendorIds();
            Uri manifestUri = new Uri(_manifestUrl, UriKind.Absolute);
            int newMessages = 0;

            foreach (MessageManifestEntry entry in manifest.Messages ?? new List<MessageManifestEntry>())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!TryValidateEntry(entry, seenIds))
                {
                    continue;
                }

                // Explicit testing of message format for future compatibility
                string format = entry.Format;
                if (string.IsNullOrWhiteSpace(format))
                {
                    string urlToCheck = !string.IsNullOrWhiteSpace(entry.Url) ? entry.Url : entry.MarkdownUrl;
                    if (!string.IsNullOrWhiteSpace(urlToCheck))
                    {
                        if (urlToCheck.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || urlToCheck.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
                        {
                            format = "html";
                        }
                        else
                        {
                            format = "md";
                        }
                    }
                    else
                    {
                        format = "md";
                    }
                }
                format = format.ToLowerInvariant();
                if (format == "markdown")
                {
                    format = "md";
                }

                if (!SupportedFormats.Contains(format))
                {
                    _logger.Warn($"MessageSyncService/SyncMessagesAsync: Skipping message id={entry.Id} due to unsupported format '{format}'. Supported formats are (html, md, markdown).");
                    continue;
                }

                LocalMessage existing = store.Messages.FirstOrDefault(m => m.Id.Equals(entry.Id, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    if (existing.IsFaulty)
                    {
                        _logger.Trace($"MessageSyncService/SyncMessagesAsync: Skipping faulty message id={entry.Id}.");
                        continue;
                    }

                    // Check if file exists. If it successfully exists, we can skip downloading it.
                    string checkPath = Path.Combine(_messagesFolderPath, existing.MarkdownFileName);
                    if (File.Exists(checkPath))
                    {
                        _logger.Trace($"MessageSyncService/SyncMessagesAsync: Skipping existing valid message id={entry.Id}.");
                        continue;
                    }

                    if (existing.DownloadAttempts >= 3)
                    {
                        existing.IsFaulty = true;
                        _logger.Warn($"MessageSyncService/SyncMessagesAsync: Missing message file id={entry.Id} but exhausted download attempts. Marked as faulty.");
                        continue;
                    }
                }

                if (!IsEligibleForClient(entry, appVersion, currentVendorIds))
                {
                    continue;
                }

                string targetUrl = !string.IsNullOrWhiteSpace(entry.Url) ? entry.Url : entry.MarkdownUrl;
                string content = await DownloadMarkdownAsync(manifestUri, targetUrl, cancellationToken).ConfigureAwait(false);

                // Compute and verify hash if provided in manifest
                bool isHashValid = true;
                if (content != null && !string.IsNullOrWhiteSpace(entry.Hash))
                {
                    try
                    {
                        using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
                        {
                            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                            byte[] hashBytes = sha256.ComputeHash(contentBytes);
                            StringBuilder sb = new StringBuilder(hashBytes.Length * 2);
                            foreach (byte b in hashBytes)
                            {
                                sb.Append(b.ToString("x2"));
                            }
                            string computedHash = sb.ToString();

                            if (!computedHash.Equals(entry.Hash.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                isHashValid = false;
                                _logger.Warn($"MessageSyncService/SyncMessagesAsync: Hash mismatch for message id={entry.Id}. Expected: '{entry.Hash}', Computed: '{computedHash}'.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, $"MessageSyncService/SyncMessagesAsync: Exception while calculating SHA-256 for message id={entry.Id}.");
                        isHashValid = false;
                    }
                }

                if (content == null || !isHashValid)
                {
                    // Track retry attempts
                    if (existing != null)
                    {
                        existing.DownloadAttempts++;
                        if (existing.DownloadAttempts >= 3)
                        {
                            existing.IsFaulty = true;
                            _logger.Error($"MessageSyncService/SyncMessagesAsync: Message id={entry.Id} failed verification or download after multiple attempts. Marked as faulty.");
                        }
                    }
                    else
                    {
                        // Add to database so we track its failed attempts
                        store.Messages.Add(new LocalMessage
                        {
                            Id = entry.Id,
                            Title = string.IsNullOrWhiteSpace(entry.Title) ? "DisplayMagician Message" : entry.Title,
                            MarkdownFileName = BuildSafeFileName(entry.Id, format),
                            SourceMarkdownUrl = targetUrl,
                            PublishedUtc = entry.PublishedUtc,
                            ReceivedUtc = DateTime.UtcNow,
                            IsRead = false,
                            Vendors = entry.Vendors ?? new List<string>(),
                            Format = format,
                            Hash = entry.Hash,
                            ShowOnStartup = entry.ShowOnStartup,
                            DownloadAttempts = 1,
                            IsFaulty = false
                        });
                    }
                    continue;
                }

                string safeFileName = BuildSafeFileName(entry.Id, format);
                string fullPath = Path.Combine(_messagesFolderPath, safeFileName);
                try
                {
                    await File.WriteAllTextAsync(fullPath, content, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, $"MessageSyncService/SyncMessagesAsync: Failed to write message file (messageId={entry.Id}, markdownPath={fullPath}, sourceUrl={targetUrl}).");
                    if (existing != null)
                    {
                        existing.DownloadAttempts++;
                        if (existing.DownloadAttempts >= 3)
                        {
                            existing.IsFaulty = true;
                        }
                    }
                    continue;
                }

                if (existing != null)
                {
                    existing.MarkdownFileName = safeFileName;
                    existing.SourceMarkdownUrl = targetUrl;
                    existing.Format = format;
                    existing.Hash = entry.Hash;
                    existing.ShowOnStartup = entry.ShowOnStartup;
                    existing.DownloadAttempts = 0;
                    existing.IsFaulty = false;
                }
                else
                {
                    store.Messages.Add(new LocalMessage
                    {
                        Id = entry.Id,
                        Title = string.IsNullOrWhiteSpace(entry.Title) ? "DisplayMagician Message" : entry.Title,
                        MarkdownFileName = safeFileName,
                        SourceMarkdownUrl = targetUrl,
                        PublishedUtc = entry.PublishedUtc,
                        ReceivedUtc = DateTime.UtcNow,
                        IsRead = false,
                        Vendors = entry.Vendors ?? new List<string>(),
                        Format = format,
                        Hash = entry.Hash,
                        ShowOnStartup = entry.ShowOnStartup,
                        DownloadAttempts = 0,
                        IsFaulty = false
                    });
                    newMessages++;
                }
            }

            PruneToMaxMessages(store);
            store.LastSuccessfulCheckUtc = DateTime.UtcNow;
            SaveStore(store);

            _logger.Info($"MessageSyncService/SyncMessagesAsync: Sync finished (newMessages={newMessages}, totalMessages={store.Messages.Count}, unread={store.Messages.Count(m => !m.IsRead)}).");

            return new MessageSyncResult
            {
                Success = true,
                NewMessagesCount = newMessages,
                UnreadCount = store.Messages.Count(m => !m.IsRead)
            };
        }

        private async Task<MessageManifestDocument> DownloadManifestAsync(CancellationToken cancellationToken)
        {
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(_manifestUrl, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.Warn($"MessageSyncService/DownloadManifestAsync: Manifest request failed (url={_manifestUrl}, statusCode={(int)response.StatusCode}, reason={response.ReasonPhrase}).");
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.Warn($"MessageSyncService/DownloadManifestAsync: Manifest at {_manifestUrl} was empty.");
                    return null;
                }

                return JsonConvert.DeserializeObject<MessageManifestDocument>(json);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"MessageSyncService/DownloadManifestAsync: Could not fetch or parse manifest (url={_manifestUrl}).");
                return null;
            }
        }

        private async Task<string> DownloadMarkdownAsync(Uri manifestUri, string markdownUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(markdownUrl))
            {
                _logger.Warn($"MessageSyncService/DownloadMarkdownAsync: Markdown URL was empty in manifest entry.");
                return null;
            }

            try
            {
                Uri resolved = Uri.TryCreate(markdownUrl, UriKind.Absolute, out Uri absolute)
                    ? absolute
                    : new Uri(manifestUri, markdownUrl);

                using HttpResponseMessage response = await _httpClient.GetAsync(resolved, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.Warn($"MessageSyncService/DownloadMarkdownAsync: Markdown request failed (manifestUrl={manifestUri}, sourceMarkdownUrl={markdownUrl}, resolvedUrl={resolved}, statusCode={(int)response.StatusCode}, reason={response.ReasonPhrase}).");
                    return null;
                }

                string markdown = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(markdown))
                {
                    _logger.Warn($"MessageSyncService/DownloadMarkdownAsync: Markdown content was empty (manifestUrl={manifestUri}, sourceMarkdownUrl={markdownUrl}, resolvedUrl={resolved}).");
                    return null;
                }

                return markdown;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"MessageSyncService/DownloadMarkdownAsync: Failed to fetch markdown (manifestUrl={manifestUri}, sourceMarkdownUrl={markdownUrl}).");
                return null;
            }
        }

        private bool TryValidateEntry(MessageManifestEntry entry, HashSet<string> seenIds)
        {
            if (entry == null)
            {
                _logger.Warn($"MessageSyncService/TryValidateEntry: Skipping null manifest message entry.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(entry.Id))
            {
                _logger.Warn($"MessageSyncService/TryValidateEntry: Skipping manifest entry with missing id (title={entry.Title ?? string.Empty}, markdownUrl={entry.MarkdownUrl ?? string.Empty}).");
                return false;
            }

            if (!Guid.TryParse(entry.Id, out _))
            {
                _logger.Warn($"MessageSyncService/TryValidateEntry: Skipping non-UUID message id '{entry.Id}'.");
                return false;
            }

            if (!seenIds.Add(entry.Id))
            {
                _logger.Warn($"MessageSyncService/TryValidateEntry: Duplicate message id '{entry.Id}' in manifest. Keeping first instance.");
                return false;
            }

            return true;
        }

        private bool IsEligibleForClient(MessageManifestEntry entry, string appVersion, HashSet<string> currentVendorIds)
        {
            if (!MatchesVersionRange(entry.MinVersion, entry.MaxVersion, appVersion))
            {
                return false;
            }

            if (entry.StartUtc.HasValue && DateTime.UtcNow < entry.StartUtc.Value)
            {
                return false;
            }

            if (entry.EndUtc.HasValue && DateTime.UtcNow > entry.EndUtc.Value)
            {
                return false;
            }

            if (entry.Vendors == null || entry.Vendors.Count == 0)
            {
                return true;
            }

            HashSet<string> requiredVendors = new HashSet<string>(entry.Vendors.Select(NormalizeVendorToken), StringComparer.OrdinalIgnoreCase);
            requiredVendors.RemoveWhere(v => string.IsNullOrWhiteSpace(v));
            if (requiredVendors.Count == 0)
            {
                return true;
            }

            return currentVendorIds.Overlaps(requiredVendors);
        }

        private static bool MatchesVersionRange(string minVersionText, string maxVersionText, string currentVersionText)
        {
            Version current = ParseVersion(currentVersionText);
            if (current == null)
            {
                return true;
            }

            Version min = ParseVersion(minVersionText);
            if (min != null && current < min)
            {
                return false;
            }

            Version max = ParseVersion(maxVersionText);
            if (max != null && current > max)
            {
                return false;
            }

            return true;
        }

        private static Version ParseVersion(string versionText)
        {
            if (string.IsNullOrWhiteSpace(versionText))
            {
                return null;
            }

            string normalized = versionText.Trim();
            int separatorIndex = normalized.IndexOfAny(new[] { '-', '+' });
            if (separatorIndex > 0)
            {
                normalized = normalized.Substring(0, separatorIndex);
            }

            return Version.TryParse(normalized, out Version version)
                ? version
                : null;
        }

        private static string NormalizeVendorToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return string.Empty;
            }

            string normalized = token.Trim().ToUpperInvariant();
            return normalized switch
            {
                "INTEL" => "8086",
                "NVIDIA" => "10DE",
                "AMD" => "1002",
                _ => normalized
            };
        }

        private static string BuildSafeFileName(string id, string format)
        {
            StringBuilder sb = new StringBuilder(id.Length + 5);
            foreach (char c in id)
            {
                sb.Append(char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_');
            }

            if (format.Equals("html", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append(".html");
            }
            else
            {
                sb.Append(".md");
            }
            return sb.ToString();
        }

        private static HashSet<string> GetCurrentVendorIds()
        {
            HashSet<string> vendorIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (string vendor in WinLibrary.GetAllPCIVideoCardVendors())
                {
                    vendorIds.Add(NormalizeVendorToken(vendor));
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "MessageSyncService/GetCurrentVendorIds: Failed to detect GPU vendor IDs. Vendor-filtered messages may be skipped.");
            }

            return vendorIds;
        }

        private void PruneToMaxMessages(MessageStoreDocument store)
        {
            if (store.Messages.Count <= MaxStoredMessages)
            {
                return;
            }

            List<LocalMessage> ordered = store.Messages
                .OrderBy(m => m.ReceivedUtc)
                .ThenBy(m => m.PublishedUtc ?? DateTime.MaxValue)
                .ToList();

            while (ordered.Count > MaxStoredMessages)
            {
                LocalMessage toRemove = ordered[0];
                ordered.RemoveAt(0);
                TryDeleteMarkdownFile(toRemove.MarkdownFileName);
            }

            int removedCount = store.Messages.Count - ordered.Count;
            if (removedCount > 0)
            {
                _logger.Info($"MessageSyncService/PruneToMaxMessages: Pruned {removedCount} old message(s) to enforce max={MaxStoredMessages}.");
            }

            store.Messages = ordered
                .OrderByDescending(m => m.ReceivedUtc)
                .ToList();
        }

        private void TryDeleteMarkdownFile(string markdownFileName)
        {
            if (string.IsNullOrWhiteSpace(markdownFileName))
            {
                return;
            }

            string fullPath = Path.Combine(_messagesFolderPath, markdownFileName);
            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"MessageSyncService/TryDeleteMarkdownFile: Failed to delete markdown file (fullPath={fullPath}).");
            }
        }

        private MessageStoreDocument LoadStore()
        {
            lock (_storeLock)
            {
                try
                {
                    Directory.CreateDirectory(_messagesFolderPath);
                    if (!File.Exists(_storePath))
                    {
                        return new MessageStoreDocument();
                    }

                    string json = File.ReadAllText(_storePath);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new MessageStoreDocument();
                    }

                    MessageStoreDocument store = JsonConvert.DeserializeObject<MessageStoreDocument>(json);
                    return store ?? new MessageStoreDocument();
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, $"MessageSyncService/LoadStore: Failed to load message store (storePath={_storePath}). Recreating in-memory store for this run.");
                    return new MessageStoreDocument();
                }
            }
        }

        private void SaveStore(MessageStoreDocument store)
        {
            lock (_storeLock)
            {
                try
                {
                    Directory.CreateDirectory(_messagesFolderPath);
                    string json = JsonConvert.SerializeObject(store, Formatting.Indented);
                    File.WriteAllText(_storePath, json);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, $"MessageSyncService/SaveStore: Failed to persist message store (storePath={_storePath}, messageCount={store?.Messages?.Count ?? 0}).");
                }
            }
        }
    }
}
