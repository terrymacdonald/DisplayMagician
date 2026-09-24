using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Owns one-hour pairing sessions and only creates a remote-client association after authorised approval.</summary>
public sealed class DevicePairingCoordinator
{
    public static readonly TimeSpan PairingLifetime = TimeSpan.FromHours(1);
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _storagePath;
    private readonly PairedClientRepository _pairedClients;
    private readonly List<PairingSessionRecord> _sessions;

    public DevicePairingCoordinator(StoragePaths storagePaths, PairedClientRepository pairedClients)
    {
        ArgumentNullException.ThrowIfNull(storagePaths);
        _pairedClients = pairedClients ?? throw new ArgumentNullException(nameof(pairedClients));
        _storagePath = Path.Combine(storagePaths.MachinePath, "DevicePairingSessions.json");
        _sessions = Load();
    }

    public DevicePairingQrCode CreateQrCode(string ownerUserSid, GatewayPairingIdentity gateway, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        ArgumentNullException.ThrowIfNull(gateway);
        if (string.IsNullOrWhiteSpace(gateway.GatewayUri) || string.IsNullOrWhiteSpace(gateway.HostId) || string.IsNullOrWhiteSpace(gateway.HostIdentityPublicKeyJwk) || string.IsNullOrWhiteSpace(gateway.TlsCertificateSha256))
        {
            throw new ArgumentException("A complete Gateway identity is required.", nameof(gateway));
        }

        byte[] secretBytes = RandomNumberGenerator.GetBytes(32);
        string secret = Convert.ToBase64String(secretBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        DateTime expiresUtc = utcNow.ToUniversalTime().Add(PairingLifetime);
        lock (_syncRoot)
        {
            ExpireUnsafe(utcNow);
            PairingSessionRecord session = new PairingSessionRecord
            {
                PairingSessionId = Guid.NewGuid(),
                OwnerUserSid = ownerUserSid,
                Gateway = Copy(gateway),
                SecretHash = GetHash(secret),
                CreatedUtc = utcNow.ToUniversalTime(),
                ExpiresUtc = expiresUtc,
                State = DevicePairingState.AwaitingDevice
            };
            _sessions.Add(session);
            PersistUnsafe();
            return new DevicePairingQrCode { PairingSessionId = session.PairingSessionId, PairingSecret = secret, ExpiresUtc = expiresUtc, Gateway = Copy(gateway) };
        }
    }

    public DevicePairingResult Submit(DevicePairingRequest request, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (_syncRoot)
        {
            ExpireUnsafe(utcNow);
            PairingSessionRecord? session = _sessions.LastOrDefault(candidate => candidate.PairingSessionId == request.PairingSessionId);
            if (session == null || !FixedTimeEquals(session.SecretHash, GetHash(request.PairingSecret)))
            {
                return new DevicePairingResult { State = DevicePairingState.Rejected, Message = "The pairing session is unavailable." };
            }

            if (!IsValidRequest(request))
            {
                return new DevicePairingResult { State = DevicePairingState.Rejected, Message = "The pairing request is invalid." };
            }

            if (session.State == DevicePairingState.AwaitingApproval && string.Equals(session.DeviceId, request.DeviceId, StringComparison.Ordinal) && string.Equals(session.DeviceDisplayName, request.DeviceDisplayName, StringComparison.Ordinal) && string.Equals(session.DevicePublicKeyJwk, request.DevicePublicKeyJwk, StringComparison.Ordinal) && session.RequestedCapabilities.SequenceEqual(request.RequestedCapabilities, StringComparer.Ordinal))
            {
                return new DevicePairingResult { State = DevicePairingState.AwaitingApproval, DeviceId = request.DeviceId, Message = "Awaiting approval." };
            }

            if (session.State != DevicePairingState.AwaitingDevice)
            {
                return new DevicePairingResult { State = DevicePairingState.Rejected, Message = "The pairing session is unavailable." };
            }

            session.DeviceId = request.DeviceId;
            session.DeviceDisplayName = request.DeviceDisplayName;
            session.DevicePublicKeyJwk = request.DevicePublicKeyJwk;
            session.RequestedCapabilities = (request.RequestedCapabilities ?? Array.Empty<string>()).ToArray();
            session.State = DevicePairingState.AwaitingApproval;
            PersistUnsafe();
            return new DevicePairingResult { State = DevicePairingState.AwaitingApproval, DeviceId = request.DeviceId, Message = "Awaiting approval." };
        }
    }

    public DevicePairingSessionView[] GetPendingForUser(string ownerUserSid, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        lock (_syncRoot)
        {
            ExpireUnsafe(utcNow);
            return _sessions.Where(session => string.Equals(session.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase) && session.State == DevicePairingState.AwaitingApproval).Select(ToView).ToArray();
        }
    }

    public DevicePairingResult ApproveFromLocalClient(string ownerUserSid, ApproveDevicePairingRequest request, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        return Approve(ownerUserSid, request, utcNow);
    }

    public DevicePairingResult ApproveFromPairedClient(string ownerUserSid, string approvingDeviceId, ApproveDevicePairingRequest request, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        ArgumentException.ThrowIfNullOrWhiteSpace(approvingDeviceId);
        if (!_pairedClients.HasCapability(ownerUserSid, approvingDeviceId, RemoteClientCapabilities.PairingApprove))
        {
            return new DevicePairingResult { State = DevicePairingState.Rejected, Message = "This device is not authorised to approve pairing." };
        }

        return Approve(ownerUserSid, request, utcNow);
    }

    public DevicePairingResult Reject(string ownerUserSid, Guid pairingSessionId, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        lock (_syncRoot)
        {
            ExpireUnsafe(utcNow);
            PairingSessionRecord? session = _sessions.LastOrDefault(candidate => candidate.PairingSessionId == pairingSessionId && string.Equals(candidate.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase));
            if (session == null || session.State != DevicePairingState.AwaitingApproval)
            {
                return new DevicePairingResult { State = DevicePairingState.Rejected, Message = "The pairing request is unavailable." };
            }

            session.State = DevicePairingState.Rejected;
            PersistUnsafe();
            return new DevicePairingResult { State = DevicePairingState.Rejected, DeviceId = session.DeviceId, Message = "Pairing was rejected." };
        }
    }

    private DevicePairingResult Approve(string ownerUserSid, ApproveDevicePairingRequest request, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (_syncRoot)
        {
            ExpireUnsafe(utcNow);
            PairingSessionRecord? session = _sessions.LastOrDefault(candidate => candidate.PairingSessionId == request.PairingSessionId && string.Equals(candidate.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase));
            if (session == null || session.State != DevicePairingState.AwaitingApproval || _pairedClients.IsActiveDeviceId(session.DeviceId) || !AreGrantedCapabilitiesValid(session, request.GrantedCapabilities))
            {
                return new DevicePairingResult { State = DevicePairingState.Rejected, Message = "The pairing request is unavailable or invalid." };
            }

            DateTime pairedUtc = utcNow.ToUniversalTime();
            _pairedClients.Upsert(new PairedClient
            {
                DeviceId = session.DeviceId,
                OwnerUserSid = session.OwnerUserSid,
                DisplayName = session.DeviceDisplayName,
                PublicKeyJwk = session.DevicePublicKeyJwk,
                PublicKeyFingerprint = GetFingerprint(session.DevicePublicKeyJwk),
                GrantedCapabilities = request.GrantedCapabilities.ToArray(),
                PairedUtc = pairedUtc
            });
            session.State = DevicePairingState.Approved;
            PersistUnsafe();
            return new DevicePairingResult { State = DevicePairingState.Approved, DeviceId = session.DeviceId, Message = "Pairing was approved." };
        }
    }

    private static bool IsValidRequest(DevicePairingRequest request)
    {
        string[] capabilities = request.RequestedCapabilities ?? Array.Empty<string>();
        return IsBounded(request.DeviceId, ControlProtocol.MaximumDeviceIdLength) && IsBounded(request.DeviceDisplayName, ControlProtocol.MaximumDisplayNameLength) && IsP256PublicKeyJwk(request.DevicePublicKeyJwk) && capabilities.Length <= ControlProtocol.MaximumRequiredCapabilities && capabilities.All(IsValidCapability) && capabilities.Distinct(StringComparer.Ordinal).Count() == capabilities.Length;
    }

    private static bool AreGrantedCapabilitiesValid(PairingSessionRecord session, string[]? grantedCapabilities)
    {
        string[] granted = grantedCapabilities ?? Array.Empty<string>();
        return granted.Length <= ControlProtocol.MaximumRequiredCapabilities && granted.All(IsValidCapability) && granted.Distinct(StringComparer.Ordinal).Count() == granted.Length && granted.All(capability => session.RequestedCapabilities.Contains(capability, StringComparer.Ordinal));
    }

    private static bool IsP256PublicKeyJwk(string? publicKeyJwk)
    {
        if (string.IsNullOrWhiteSpace(publicKeyJwk) || publicKeyJwk.Length > 2048)
        {
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(publicKeyJwk);
            JsonElement root = document.RootElement;
            return root.ValueKind == JsonValueKind.Object && !root.TryGetProperty("d", out _) && root.TryGetProperty("kty", out JsonElement keyType) && keyType.GetString() == "EC" && root.TryGetProperty("crv", out JsonElement curve) && curve.GetString() == "P-256" && root.TryGetProperty("x", out JsonElement x) && IsBase64UrlCoordinate(x.GetString()) && root.TryGetProperty("y", out JsonElement y) && IsBase64UrlCoordinate(y.GetString());
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsBase64UrlCoordinate(string? value) => value != null && value.Length == 43 && value.All(character => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_');
    private static bool IsValidCapability(string? capability) => capability != null && RemoteClientCapabilities.All.Contains(capability, StringComparer.Ordinal);
    private static bool IsBounded(string? value, int maximumLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maximumLength;
    private static string GetHash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)));
    private static string GetFingerprint(string publicKeyJwk) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(publicKeyJwk)));
    private static bool FixedTimeEquals(string left, string right) => CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(left), Encoding.UTF8.GetBytes(right));

    private void ExpireUnsafe(DateTime utcNow)
    {
        bool changed = false;
        foreach (PairingSessionRecord session in _sessions.Where(candidate => (candidate.State == DevicePairingState.AwaitingDevice || candidate.State == DevicePairingState.AwaitingApproval) && candidate.ExpiresUtc <= utcNow.ToUniversalTime()))
        {
            session.State = DevicePairingState.Expired;
            changed = true;
        }

        if (changed)
        {
            PersistUnsafe();
        }
    }

    private List<PairingSessionRecord> Load()
    {
        try
        {
            return File.Exists(_storagePath) ? JsonSerializer.Deserialize<PairingSessionRecord[]>(File.ReadAllText(_storagePath))?.ToList() ?? new List<PairingSessionRecord>() : new List<PairingSessionRecord>();
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            Logger.Error(ex, "DevicePairingCoordinator/Load: Could not load device pairing sessions from {0}.", _storagePath);
            return new List<PairingSessionRecord>();
        }
    }

    private void PersistUnsafe()
    {
        try
        {
            AtomicFileStore.WriteAllText(_storagePath, JsonSerializer.Serialize(_sessions.ToArray()), $"{_storagePath}.bak");
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.Error(ex, "DevicePairingCoordinator/PersistUnsafe: Could not persist device pairing sessions to {0}.", _storagePath);
            throw;
        }
    }

    private static DevicePairingSessionView ToView(PairingSessionRecord session)
    {
        return new DevicePairingSessionView { PairingSessionId = session.PairingSessionId, OwnerUserSid = session.OwnerUserSid, State = session.State, CreatedUtc = session.CreatedUtc, ExpiresUtc = session.ExpiresUtc, DeviceId = session.DeviceId, DeviceDisplayName = session.DeviceDisplayName, RequestedCapabilities = (session.RequestedCapabilities ?? Array.Empty<string>()).ToArray() };
    }

    private static GatewayPairingIdentity Copy(GatewayPairingIdentity gateway)
    {
        return new GatewayPairingIdentity { GatewayUri = gateway.GatewayUri, HostId = gateway.HostId, HostIdentityPublicKeyJwk = gateway.HostIdentityPublicKeyJwk, TlsCertificateSha256 = gateway.TlsCertificateSha256 };
    }

    private sealed class PairingSessionRecord
    {
        public Guid PairingSessionId { get; set; }
        public string OwnerUserSid { get; set; } = string.Empty;
        public GatewayPairingIdentity Gateway { get; set; } = new GatewayPairingIdentity();
        public string SecretHash { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public DevicePairingState State { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceDisplayName { get; set; } = string.Empty;
        public string DevicePublicKeyJwk { get; set; } = string.Empty;
        public string[] RequestedCapabilities { get; set; } = Array.Empty<string>();
    }
}
