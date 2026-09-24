using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Persists the machine-owned public-key associations for approved remote clients.</summary>
public sealed class PairedClientRepository
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _storagePath;
    private readonly List<PairedClient> _clients;

    public PairedClientRepository(StoragePaths storagePaths)
    {
        ArgumentNullException.ThrowIfNull(storagePaths);
        _storagePath = Path.Combine(storagePaths.MachinePath, "PairedClients.json");
        _clients = Load();
    }

    public PairedClient[] GetActiveForUser(string ownerUserSid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        lock (_syncRoot)
        {
            return _clients.Where(client => client.RevokedUtc == null && string.Equals(client.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase)).Select(Copy).ToArray();
        }
    }

    public bool HasCapability(string ownerUserSid, string deviceId, string capability)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(capability);
        lock (_syncRoot)
        {
            PairedClient? client = _clients.LastOrDefault(candidate => candidate.RevokedUtc == null && string.Equals(candidate.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase) && string.Equals(candidate.DeviceId, deviceId, StringComparison.Ordinal));
            return client != null && client.GrantedCapabilities.Contains(capability, StringComparer.Ordinal);
        }
    }

    public bool IsActiveDeviceId(string deviceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        lock (_syncRoot)
        {
            return _clients.Any(client => client.RevokedUtc == null && string.Equals(client.DeviceId, deviceId, StringComparison.Ordinal));
        }
    }

    public void Upsert(PairedClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        lock (_syncRoot)
        {
            _clients.RemoveAll(candidate => string.Equals(candidate.DeviceId, client.DeviceId, StringComparison.Ordinal));
            _clients.Add(Copy(client));
            PersistUnsafe();
        }
    }

    public bool Revoke(string ownerUserSid, string deviceId, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        lock (_syncRoot)
        {
            PairedClient? client = _clients.LastOrDefault(candidate => candidate.RevokedUtc == null && string.Equals(candidate.OwnerUserSid, ownerUserSid, StringComparison.OrdinalIgnoreCase) && string.Equals(candidate.DeviceId, deviceId, StringComparison.Ordinal));
            if (client == null)
            {
                return false;
            }

            client.RevokedUtc = utcNow.ToUniversalTime();
            PersistUnsafe();
            return true;
        }
    }

    private List<PairedClient> Load()
    {
        try
        {
            if (!File.Exists(_storagePath))
            {
                return new List<PairedClient>();
            }

            return JsonSerializer.Deserialize<PairedClient[]>(File.ReadAllText(_storagePath))?.Select(Copy).ToList() ?? new List<PairedClient>();
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            Logger.Error(ex, "PairedClientRepository/Load: Could not load paired clients from {0}.", _storagePath);
            return new List<PairedClient>();
        }
    }

    private void PersistUnsafe()
    {
        try
        {
            AtomicFileStore.WriteAllText(_storagePath, JsonSerializer.Serialize(_clients.ToArray()), $"{_storagePath}.bak");
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.Error(ex, "PairedClientRepository/PersistUnsafe: Could not persist paired clients to {0}.", _storagePath);
            throw;
        }
    }

    private static PairedClient Copy(PairedClient client)
    {
        return new PairedClient
        {
            DeviceId = client.DeviceId,
            OwnerUserSid = client.OwnerUserSid,
            DisplayName = client.DisplayName,
            PublicKeyJwk = client.PublicKeyJwk,
            PublicKeyFingerprint = client.PublicKeyFingerprint,
            GrantedCapabilities = (client.GrantedCapabilities ?? Array.Empty<string>()).ToArray(),
            PairedUtc = client.PairedUtc,
            LastAuthenticatedUtc = client.LastAuthenticatedUtc,
            RevokedUtc = client.RevokedUtc
        };
    }
}
