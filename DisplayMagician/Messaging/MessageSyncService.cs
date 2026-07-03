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
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"MessageSyncService/EnsureStorage: Failed to initialise message store at {_messagesFolderPath}.");
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

            MessageManifestDocument manifest = await DownloadManifestAsync(cancellationToken).ConfigureAwait(false);
            if (manifest == null)
            {
                return new MessageSyncResult { Success = false, UnreadCount = store.Messages.Count(m => !m.IsRead) };
            }

            if (manifest.SchemaVersion != CurrentSchemaVersion)
            {
                _logger.Warn($"MessageSyncService/SyncMessagesAsync: Unsupported message schema version {manifest.SchemaVersion}.");
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

                if (store.Messages.Any(m => m.Id.Equals(entry.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (!IsEligibleForClient(entry, appVersion, currentVendorIds))
                {
                    continue;
                }

                string markdownContent = await DownloadMarkdownAsync(manifestUri, entry.MarkdownUrl, cancellationToken).ConfigureAwait(false);
                if (markdownContent == null)
                {
                    continue;
                }

                string markdownFileName = BuildSafeMarkdownFileName(entry.Id);
                string markdownFullPath = Path.Combine(_messagesFolderPath, markdownFileName);
                try
                {
                    await File.WriteAllTextAsync(markdownFullPath, markdownContent, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, $"MessageSyncService/SyncMessagesAsync: Failed to write markdown file {markdownFullPath}.");
                    continue;
                }

                store.Messages.Add(new LocalMessage
                {
                    Id = entry.Id,
                    Title = string.IsNullOrWhiteSpace(entry.Title) ? "DisplayMagician Message" : entry.Title,
                    MarkdownFileName = markdownFileName,
                    SourceMarkdownUrl = entry.MarkdownUrl,
                    PublishedUtc = entry.PublishedUtc,
                    ReceivedUtc = DateTime.UtcNow,
                    IsRead = false,
                    Vendors = entry.Vendors ?? new List<string>()
                });
                newMessages++;
            }

            PruneToMaxMessages(store);
            store.LastSuccessfulCheckUtc = DateTime.UtcNow;
            SaveStore(store);

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
                string json = await _httpClient.GetStringAsync(_manifestUrl, cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.Warn($"MessageSyncService/DownloadManifestAsync: Manifest at {_manifestUrl} was empty.");
                    return null;
                }

                return JsonConvert.DeserializeObject<MessageManifestDocument>(json);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"MessageSyncService/DownloadManifestAsync: Could not fetch manifest {_manifestUrl}.");
                return null;
            }
        }

        private async Task<string> DownloadMarkdownAsync(Uri manifestUri, string markdownUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(markdownUrl))
            {
                return null;
            }

            try
            {
                Uri resolved = Uri.TryCreate(markdownUrl, UriKind.Absolute, out Uri absolute)
                    ? absolute
                    : new Uri(manifestUri, markdownUrl);

                return await _httpClient.GetStringAsync(resolved, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"MessageSyncService/DownloadMarkdownAsync: Failed to fetch markdown {markdownUrl}.");
                return null;
            }
        }

        private bool TryValidateEntry(MessageManifestEntry entry, HashSet<string> seenIds)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Id))
            {
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

        private static string BuildSafeMarkdownFileName(string id)
        {
            StringBuilder sb = new StringBuilder(id.Length + 3);
            foreach (char c in id)
            {
                sb.Append(char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_');
            }

            sb.Append(".md");
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
            catch
            {
                // If vendor detection fails, keep set empty and rely on default matching behavior.
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
                _logger.Warn(ex, $"MessageSyncService/TryDeleteMarkdownFile: Failed to delete {fullPath}.");
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
                    _logger.Warn(ex, $"MessageSyncService/LoadStore: Failed to load message store {_storePath}. Recreating.");
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
                    _logger.Warn(ex, $"MessageSyncService/SaveStore: Failed to persist message store {_storePath}.");
                }
            }
        }
    }
}
