using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

public sealed class ClientSyncCoordinator
{
    private const int MaximumDocumentBytes = 1024 * 1024;
    private static readonly Uri SyncUri = new Uri("https://sync.displaymagician.com/sync/client-sync.json");
    private static readonly Uri SyncBaseUri = new Uri("https://sync.displaymagician.com/");
    private static readonly Regex Sha256Pattern = new Regex("^[a-fA-F0-9]{64}$", RegexOptions.Compiled);
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly HttpClient _httpClient;
    private readonly MachineScheduleCoordinator _machineScheduleCoordinator;
    private readonly ControlStateCoordinator _controlStateCoordinator;
    private readonly IAgentCommandClient _agentCommandClient;
    private readonly SemaphoreSlim _syncGate = new SemaphoreSlim(1, 1);
    private ClientSyncMessageManifest? _latestMessageManifest;

    public ClientSyncCoordinator(MachineScheduleCoordinator machineScheduleCoordinator, ControlStateCoordinator controlStateCoordinator, IAgentCommandClient agentCommandClient)
        : this(new HttpClient(), machineScheduleCoordinator, controlStateCoordinator, agentCommandClient)
    {
    }

    public ClientSyncCoordinator(HttpClient httpClient, MachineScheduleCoordinator machineScheduleCoordinator, ControlStateCoordinator controlStateCoordinator, IAgentCommandClient agentCommandClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _machineScheduleCoordinator = machineScheduleCoordinator ?? throw new ArgumentNullException(nameof(machineScheduleCoordinator));
        _controlStateCoordinator = controlStateCoordinator ?? throw new ArgumentNullException(nameof(controlStateCoordinator));
        _agentCommandClient = agentCommandClient ?? throw new ArgumentNullException(nameof(agentCommandClient));
    }

    public async Task<ClientSyncResult> SyncAsync(ClientSyncRequest request, string? requestingUserSid, int? requestingSessionId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        DateTime now = DateTime.UtcNow;
        if (!request.IsManual && !_machineScheduleCoordinator.IsClientSyncDue(now))
        {
            return new ClientSyncResult { WasDue = false };
        }

        await _syncGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            now = DateTime.UtcNow;
            if (!request.IsManual && !_machineScheduleCoordinator.IsClientSyncDue(now))
            {
                return new ClientSyncResult { WasDue = false };
            }

            ClientSyncSnapshot? snapshot = await DownloadAndValidateAsync(cancellationToken).ConfigureAwait(false);
            if (snapshot == null)
            {
                _machineScheduleCoordinator.RecordClientSyncFailure(now);
                return new ClientSyncResult();
            }

            _latestMessageManifest = snapshot.MessageManifest;
            MessageSyncResult? requestingMessageResult = await DistributeManifestAsync(snapshot.MessageManifest, requestingUserSid, requestingSessionId, cancellationToken).ConfigureAwait(false);
            _machineScheduleCoordinator.RecordClientSyncSuccess(now);
            return new ClientSyncResult
            {
                WasDue = true,
                StableUpdate = snapshot.StableUpdate,
                PrereleaseUpdate = snapshot.PrereleaseUpdate,
                MessageSync = requestingMessageResult
            };
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is JsonException || ex is InvalidOperationException || ex is IOException || ex is TimeoutException)
        {
            _machineScheduleCoordinator.RecordClientSyncFailure(now);
            Logger.Warn(ex, "ClientSyncCoordinator/SyncAsync: Combined client sync failed.");
            return new ClientSyncResult();
        }
        finally
        {
            _syncGate.Release();
        }
    }

    public async Task SendLatestManifestAsync(AgentRegistration agent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(agent);
        ClientSyncMessageManifest? manifest = _latestMessageManifest;
        if (manifest == null)
        {
            return;
        }

        await SendManifestAsync(agent, manifest, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ClientSyncSnapshot?> DownloadAndValidateAsync(CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, SyncUri);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode || response.Content.Headers.ContentLength > MaximumDocumentBytes)
        {
            return null;
        }

        byte[] content = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        if (content.Length == 0 || content.Length > MaximumDocumentBytes)
        {
            return null;
        }

        using JsonDocument document = JsonDocument.Parse(content);
        JsonElement root = document.RootElement;
        if (!root.TryGetProperty("schemaVersion", out JsonElement schemaVersion) || schemaVersion.GetInt32() != 1 ||
            !root.TryGetProperty("updates", out JsonElement updates) ||
            !updates.TryGetProperty("stable", out JsonElement stable) || !updates.TryGetProperty("prerelease", out JsonElement prerelease) ||
            !TryCreateUpdate(stable, out ClientSyncUpdateView? stableUpdate) || !TryCreateUpdate(prerelease, out ClientSyncUpdateView? prereleaseUpdate))
        {
            return null;
        }

        ClientSyncMessageManifest manifest = new ClientSyncMessageManifest
        {
            SchemaVersion = 1,
            GeneratedUtc = root.TryGetProperty("publishedUtc", out JsonElement publishedUtc) && publishedUtc.TryGetDateTime(out DateTime published) ? published.ToUniversalTime() : null
        };
        if (root.TryGetProperty("messages", out JsonElement messages) && messages.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement message in messages.EnumerateArray())
            {
                if (TryCreateMessage(message, out ClientSyncMessageEntry? entry))
                {
                    manifest.Messages.Add(entry);
                }
            }
        }

        return new ClientSyncSnapshot(stableUpdate, prereleaseUpdate, manifest);
    }

    private async Task<MessageSyncResult?> DistributeManifestAsync(ClientSyncMessageManifest manifest, string? requestingUserSid, int? requestingSessionId, CancellationToken cancellationToken)
    {
        MessageSyncResult? requestingResult = null;
        foreach (AgentRegistration agent in _controlStateCoordinator.GetAgentRegistrations())
        {
            try
            {
                ControlResponse response = await SendManifestAsync(agent, manifest, cancellationToken).ConfigureAwait(false);
                if (string.Equals(agent.UserSid, requestingUserSid, StringComparison.OrdinalIgnoreCase) && agent.SessionId == requestingSessionId)
                {
                    requestingResult = response.MessageSync;
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is IOException || ex is TimeoutException)
            {
                Logger.Warn(ex, "ClientSyncCoordinator/DistributeManifestAsync: Could not deliver the combined client-sync messages to Agent SID {0}, session {1}.", agent.UserSid, agent.SessionId);
            }
        }

        return requestingResult;
    }

    private Task<ControlResponse> SendManifestAsync(AgentRegistration agent, ClientSyncMessageManifest manifest, CancellationToken cancellationToken)
    {
        return _agentCommandClient.SendAsync(agent, new ControlEnvelope
        {
            MessageType = ControlMessageType.ApplyClientSyncMessages,
            Payload = JsonSerializer.Serialize(manifest)
        }, cancellationToken);
    }

    private static bool TryCreateUpdate(JsonElement source, out ClientSyncUpdateView? update)
    {
        update = null;
        if (!source.TryGetProperty("version", out JsonElement versionValue) || !Version.TryParse(versionValue.GetString(), out Version? version) || version.Build < 0 || version.Revision < 0 ||
            !TryGetHttpsString(source, "url", out string url) || !TryGetHttpsString(source, "changelog", out string changelog) ||
            !source.TryGetProperty("mandatory", out JsonElement mandatory) || !mandatory.TryGetProperty("value", out JsonElement mandatoryValue) || !mandatory.TryGetProperty("mode", out JsonElement mandatoryMode) || mandatoryMode.GetInt32() < 0 || mandatoryMode.GetInt32() > 2 ||
            !source.TryGetProperty("checksum", out JsonElement checksum) || !checksum.TryGetProperty("value", out JsonElement checksumValue) || !checksum.TryGetProperty("hashingAlgorithm", out JsonElement checksumAlgorithm) ||
            !string.Equals(checksumAlgorithm.GetString(), "SHA256", StringComparison.OrdinalIgnoreCase) || !Sha256Pattern.IsMatch(checksumValue.GetString() ?? string.Empty))
        {
            return false;
        }

        update = new ClientSyncUpdateView
        {
            Version = version.ToString(),
            Url = url,
            Changelog = changelog,
            Mandatory = mandatoryValue.GetBoolean(),
            MandatoryMode = mandatoryMode.GetInt32(),
            MandatoryMinimumVersion = mandatory.TryGetProperty("minVersion", out JsonElement minimumVersion) ? minimumVersion.GetString() ?? string.Empty : string.Empty,
            ChecksumValue = checksumValue.GetString() ?? string.Empty,
            ChecksumAlgorithm = checksumAlgorithm.GetString() ?? string.Empty
        };
        return true;
    }

    private static bool TryCreateMessage(JsonElement source, out ClientSyncMessageEntry? entry)
    {
        entry = null;
        if (!source.TryGetProperty("id", out JsonElement id) || !Guid.TryParse(id.GetString(), out Guid _) ||
            !source.TryGetProperty("status", out JsonElement status))
        {
            return false;
        }

        string entryStatus = status.GetString() ?? string.Empty;
        if (string.Equals(entryStatus, "deleted", StringComparison.OrdinalIgnoreCase))
        {
            entry = new ClientSyncMessageEntry { Id = id.GetString() ?? string.Empty, Status = entryStatus };
            return true;
        }

        if (!string.Equals(entryStatus, "published", StringComparison.OrdinalIgnoreCase) || !TryGetStaticArtifactUrl(source, "url", "/sync/messages/", out string url) ||
            !source.TryGetProperty("sha256", out JsonElement sha256) || !Sha256Pattern.IsMatch(sha256.GetString() ?? string.Empty) ||
            !source.TryGetProperty("format", out JsonElement format) || !(string.Equals(format.GetString(), "html", StringComparison.OrdinalIgnoreCase) || string.Equals(format.GetString(), "md", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        entry = new ClientSyncMessageEntry
        {
            Id = id.GetString() ?? string.Empty,
            Status = entryStatus,
            Title = GetString(source, "title"),
            Url = url,
            Format = format.GetString() ?? string.Empty,
            Sha256 = sha256.GetString() ?? string.Empty,
            ShowOnStartup = GetBoolean(source, "showOnStartup"),
            PublishedUtc = GetUtcDate(source, "publishedUtc"),
            DeletedUtc = GetUtcDate(source, "deletedUtc"),
            MinVersion = GetString(source, "minVersion"),
            MaxVersion = GetString(source, "maxVersion"),
            StartUtc = GetUtcDate(source, "startUtc"),
            EndUtc = GetUtcDate(source, "endUtc"),
            Vendors = GetStrings(source, "vendors"),
            Kind = GetString(source, "kind"),
            ReleaseVersion = GetString(source, "releaseVersion"),
            ReleaseChannel = GetString(source, "releaseChannel"),
            GithubReleaseId = source.TryGetProperty("githubReleaseId", out JsonElement releaseId) && releaseId.TryGetInt64(out long idValue) ? idValue : null,
            UpdateAction = GetString(source, "updateAction")
        };
        if (source.TryGetProperty("media", out JsonElement media) && media.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in media.EnumerateArray())
            {
                if (!TryGetStaticArtifactUrl(item, "url", "/sync/media/", out string mediaUrl) || !item.TryGetProperty("sha256", out JsonElement mediaHash) ||
                    !Sha256Pattern.IsMatch(mediaHash.GetString() ?? string.Empty) || !item.TryGetProperty("contentType", out JsonElement contentType) ||
                    !(contentType.GetString() == "image/png" || contentType.GetString() == "image/jpeg" || contentType.GetString() == "image/gif" || contentType.GetString() == "image/webp"))
                {
                    entry = null;
                    return false;
                }

                entry.Media.Add(new ClientSyncMessageMedia { Url = mediaUrl, Sha256 = mediaHash.GetString() ?? string.Empty, ContentType = contentType.GetString() ?? string.Empty });
            }
        }

        return true;
    }

    private static bool TryGetHttpsString(JsonElement source, string propertyName, out string value)
    {
        value = string.Empty;
        return source.TryGetProperty(propertyName, out JsonElement property) && Uri.TryCreate(property.GetString(), UriKind.Absolute, out Uri? uri) &&
            uri.Scheme == Uri.UriSchemeHttps && string.IsNullOrEmpty(uri.UserInfo) && (value = uri.AbsoluteUri) != null;
    }

    private static bool TryGetStaticArtifactUrl(JsonElement source, string propertyName, string expectedPathPrefix, out string value)
    {
        value = string.Empty;
        if (!source.TryGetProperty(propertyName, out JsonElement property))
        {
            return false;
        }

        string candidate = property.GetString() ?? string.Empty;
        if (candidate.Contains("..", StringComparison.Ordinal) || candidate.Contains('@'))
        {
            return false;
        }

        if (candidate.StartsWith("/", StringComparison.Ordinal))
        {
            value = candidate;
            return candidate.StartsWith(expectedPathPrefix, StringComparison.Ordinal);
        }

        return Uri.TryCreate(candidate, UriKind.Absolute, out Uri? uri) && uri.Scheme == Uri.UriSchemeHttps &&
            string.Equals(uri.Host, SyncBaseUri.Host, StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(uri.UserInfo) &&
            uri.AbsolutePath.StartsWith(expectedPathPrefix, StringComparison.Ordinal) && (value = uri.AbsoluteUri) != null;
    }

    private static string GetString(JsonElement source, string propertyName) => source.TryGetProperty(propertyName, out JsonElement property) ? property.GetString() ?? string.Empty : string.Empty;
    private static bool GetBoolean(JsonElement source, string propertyName) => source.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.True;
    private static DateTime? GetUtcDate(JsonElement source, string propertyName) => source.TryGetProperty(propertyName, out JsonElement property) && property.TryGetDateTime(out DateTime value) ? value.ToUniversalTime() : null;
    private static List<string> GetStrings(JsonElement source, string propertyName) => source.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.Array ? property.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString() ?? string.Empty).ToList() : new List<string>();

    private sealed record ClientSyncSnapshot(ClientSyncUpdateView StableUpdate, ClientSyncUpdateView PrereleaseUpdate, ClientSyncMessageManifest MessageManifest);
}