using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagicianShared.Windows;
using Newtonsoft.Json;

namespace DisplayMagician.Messaging
{
    public sealed class AnonymousMetricsService
    {
        private static readonly Uri HeartbeatUri = new Uri("https://www.displaymagician.com/metrics/v1/heartbeat");
        private readonly HttpClient _httpClient;
        private readonly ProgramSettings _settings;
        private readonly NLog.Logger _logger;
        private readonly bool _testMode;
        private long _lastPersistedRuntimeMinutes;

        public AnonymousMetricsService(HttpClient httpClient, ProgramSettings settings, NLog.Logger logger, bool testMode)
        {
            _httpClient = httpClient;
            _settings = settings;
            _logger = logger;
            _testMode = testMode;
        }

        public async Task TrySendAsync(TimeSpan activeRuntime, bool allowInitialHeartbeat, CancellationToken cancellationToken)
        {
            if (_testMode || !_settings.ShareAnonymousUsageMetrics)
            {
                return;
            }

            long elapsedMinutes = Math.Max(0, (long)activeRuntime.TotalMinutes);
            if (elapsedMinutes > _lastPersistedRuntimeMinutes)
            {
                _settings.TotalAnonymousMetricActiveMinutes += elapsedMinutes - _lastPersistedRuntimeMinutes;
                _lastPersistedRuntimeMinutes = elapsedMinutes;
                _settings.SaveSettings();
            }
            DateTime now = DateTime.UtcNow;
            bool isInitialHeartbeat = string.IsNullOrWhiteSpace(_settings.LastMetricsReportedVersion);
            if (isInitialHeartbeat && !allowInitialHeartbeat)
            {
                return;
            }

            bool versionChanged = !isInitialHeartbeat && !string.Equals(_settings.LastMetricsReportedVersion, Program.AppVersion, StringComparison.OrdinalIgnoreCase);
            if (!versionChanged && _settings.NextMetricsHeartbeatUtc.HasValue && now < _settings.NextMetricsHeartbeatUtc.Value)
            {
                return;
            }

            var payload = new
            {
                schemaVersion = 1,
                installId = _settings.InstallId,
                appVersion = Program.AppVersion,
                updateChannel = _settings.UpgradeToPreReleases ? "prerelease" : "stable",
                launches = _settings.TotalAnonymousMetricLaunches,
                activeMinutes = _settings.TotalAnonymousMetricActiveMinutes,
                graphicsLibrary = GetGraphicsLibrary(),
                connectedScreenCount = Math.Min(16, Math.Max(0, System.Windows.Forms.Screen.AllScreens.Length)),
                windowsBuild = GetWindowsBuild()
            };

            try
            {
                using StringContent content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                using HttpResponseMessage response = await _httpClient.PostAsync(HeartbeatUri, content, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    _settings.LastMetricsReportedVersion = Program.AppVersion;
                    _settings.NextMetricsHeartbeatUtc = now.AddDays(7).AddHours(GetStableJitterHours(_settings.InstallId));
                    _settings.SaveSettings();
                }
                else
                {
                    _logger.Warn($"AnonymousMetricsService/TrySendAsync: Heartbeat was not accepted (statusCode={(int)response.StatusCode}).");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "AnonymousMetricsService/TrySendAsync: Anonymous metrics heartbeat failed.");
            }
        }

        private static string GetGraphicsLibrary()
        {
            try
            {
                foreach (string vendor in WinLibrary.GetAllPCIVideoCardVendors())
                {
                    if (string.Equals(vendor, "10DE", StringComparison.OrdinalIgnoreCase) || string.Equals(vendor, "NVIDIA", StringComparison.OrdinalIgnoreCase)) return "nvidia";
                    if (string.Equals(vendor, "1002", StringComparison.OrdinalIgnoreCase) || string.Equals(vendor, "AMD", StringComparison.OrdinalIgnoreCase)) return "amd";
                    if (string.Equals(vendor, "8086", StringComparison.OrdinalIgnoreCase) || string.Equals(vendor, "INTEL", StringComparison.OrdinalIgnoreCase)) return "intel";
                }
            }
            catch { }
            return "unknown";
        }

        private static string GetWindowsBuild()
        {
            Version version = Environment.OSVersion.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        private static int GetStableJitterHours(string installId)
        {
            unchecked
            {
                int hash = 17;
                foreach (char character in installId ?? string.Empty) hash = (hash * 31) + character;
                return Math.Abs(hash % 7);
            }
        }
    }
}
