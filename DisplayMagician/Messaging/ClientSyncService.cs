using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DisplayMagician.Messaging
{
    public sealed class ClientSyncResult
    {
        public bool Success { get; set; }
        public bool WasDue { get; set; }
        public MessageSyncResult MessageResult { get; set; }
        public ClientSyncUpdate SelectedUpdate { get; set; }
        public string ErrorMessage { get; set; }
    }

    public sealed class ClientSyncService
    {
        private const int SupportedSchemaVersion = 1;
        private const int MaximumDocumentBytes = 1024 * 1024;
        private static readonly Uri NormalSyncUri = new Uri("https://sync.displaymagician.com/sync/client-sync.json");
        private static readonly Uri TestSyncUri = new Uri("https://sync.displaymagician.com/sync/test-client-sync.json");
        private static readonly Regex Sha256Pattern = new Regex("^[a-fA-F0-9]{64}$", RegexOptions.Compiled);
        private readonly HttpClient _httpClient;
        private readonly NLog.Logger _logger;
        private readonly MessageSyncService _messageSyncService;
        private readonly ProgramSettings _settings;
        private readonly bool _testMode;
        private readonly SemaphoreSlim _syncGate = new SemaphoreSlim(1, 1);
        private Task<ClientSyncResult> _activeSyncTask;

        public ClientSyncService(HttpClient httpClient, NLog.Logger logger, MessageSyncService messageSyncService, ProgramSettings settings, bool testMode)
        {
            _httpClient = httpClient;
            _logger = logger;
            _messageSyncService = messageSyncService;
            _settings = settings;
            _testMode = testMode;
        }

        public Task<ClientSyncResult> RunAsync(bool manual, string appVersion, CancellationToken cancellationToken)
        {
            lock (_syncGate)
            {
                if (_activeSyncTask != null && !_activeSyncTask.IsCompleted)
                {
                    return _activeSyncTask;
                }

                _activeSyncTask = RunCoreAsync(manual, appVersion, cancellationToken);
                return _activeSyncTask;
            }
        }

        private async Task<ClientSyncResult> RunCoreAsync(bool manual, string appVersion, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            if (!manual && _settings.NextClientSyncUtc.HasValue && now < _settings.NextClientSyncUtc.Value)
            {
                return new ClientSyncResult { Success = true, WasDue = false };
            }

            await _syncGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                Uri syncUri = _testMode ? TestSyncUri : NormalSyncUri;
                ClientSyncDocument document = await DownloadDocumentAsync(syncUri, cancellationToken).ConfigureAwait(false);
                if (!TryValidateDocument(document, out string validationError))
                {
                    return HandleFailure(validationError);
                }

                MessageManifestDocument messageDocument = new MessageManifestDocument
                {
                    SchemaVersion = document.SchemaVersion,
                    GeneratedUtc = document.PublishedUtc,
                    Messages = document.Messages
                };
                MessageSyncResult messageResult = await _messageSyncService.SyncMessagesAsync(appVersion, cancellationToken, messageDocument, new Uri("https://sync.displaymagician.com/")).ConfigureAwait(false);
                if (!messageResult.Success)
                {
                    return HandleFailure("Message content could not be processed.");
                }

                ClientSyncUpdate selectedUpdate = _settings.UpgradeToPreReleases ? document.Updates.Prerelease : document.Updates.Stable;
                ScheduleSuccess(now);
                if (!_settings.SaveSettings())
                {
                    return HandleFailure("Client sync state could not be persisted.");
                }

                return new ClientSyncResult { Success = true, WasDue = true, MessageResult = messageResult, SelectedUpdate = selectedUpdate };
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "ClientSyncService/RunCoreAsync: Combined static client sync failed.");
                return HandleFailure(ex.Message);
            }
            finally
            {
                _syncGate.Release();
            }
        }

        private async Task<ClientSyncDocument> DownloadDocumentAsync(Uri syncUri, CancellationToken cancellationToken)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, syncUri);
            using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode || response.Content.Headers.ContentLength > MaximumDocumentBytes)
            {
                return null;
            }

            byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            if (bytes.Length == 0 || bytes.Length > MaximumDocumentBytes)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<ClientSyncDocument>(System.Text.Encoding.UTF8.GetString(bytes));
        }

        private bool TryValidateDocument(ClientSyncDocument document, out string error)
        {
            error = null;
            if (document == null || document.SchemaVersion != SupportedSchemaVersion || document.Updates?.Stable == null || document.Updates.Prerelease == null)
            {
                error = "The client sync document has an unsupported schema.";
                return false;
            }

            if (!IsValidUpdate(document.Updates.Stable) || !IsValidUpdate(document.Updates.Prerelease))
            {
                error = "The client sync document contains an invalid update.";
                return false;
            }

            return true;
        }

        private static bool IsValidUpdate(ClientSyncUpdate update)
        {
            return update != null
                && Version.TryParse(update.Version, out Version version) && version.Build >= 0 && version.Revision >= 0
                && IsHttpsUrl(update.Url) && IsHttpsUrl(update.Changelog)
                && update.Mandatory != null && update.Mandatory.Mode >= 0 && update.Mandatory.Mode <= 2
                && update.Checksum != null && string.Equals(update.Checksum.HashingAlgorithm, "SHA256", StringComparison.OrdinalIgnoreCase)
                && Sha256Pattern.IsMatch(update.Checksum.Value ?? string.Empty);
        }

        private static bool IsHttpsUrl(string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out Uri uri) && uri.Scheme == Uri.UriSchemeHttps && string.IsNullOrEmpty(uri.UserInfo);
        }

        private ClientSyncResult HandleFailure(string errorMessage)
        {
            int failures = Math.Min(_settings.ConsecutiveClientSyncFailures + 1, 6);
            _settings.ConsecutiveClientSyncFailures = failures;
            _settings.NextClientSyncUtc = DateTime.UtcNow.AddHours(Math.Min(Math.Pow(2, failures - 1), 24));
            _settings.SaveSettings();
            _logger.Warn($"ClientSyncService/HandleFailure: Combined static client sync failed. {errorMessage}");
            return new ClientSyncResult { Success = false, WasDue = true, ErrorMessage = errorMessage };
        }

        private void ScheduleSuccess(DateTime now)
        {
            _settings.LastSuccessfulClientSyncUtc = now;
            _settings.ConsecutiveClientSyncFailures = 0;
            _settings.NextClientSyncUtc = now.AddHours(24 + GetStableJitterHours(_settings.InstallId, 12));
        }

        private static int GetStableJitterHours(string installId, int upperInclusive)
        {
            unchecked
            {
                int hash = 17;
                foreach (char character in installId ?? string.Empty)
                {
                    hash = (hash * 31) + character;
                }
                return Math.Abs(hash % (upperInclusive + 1));
            }
        }
    }
}
