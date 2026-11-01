using System;
using System.IO;
using System.Text;
using System.Text.Json;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Durably stores the machine-wide display-control lease, including unresolved recovery state.</summary>
public sealed class DisplayControlLeaseStore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly string _leasePath;

    public DisplayControlLeaseStore(StoragePaths storagePaths)
    {
        ArgumentNullException.ThrowIfNull(storagePaths);
        _leasePath = Path.Combine(storagePaths.MachinePath, "DisplayControlLease.json");
    }

    public DisplayControlLease? Load()
    {
        try
        {
            DisplayControlLease lease = JsonSerializer.Deserialize<DisplayControlLease>(File.ReadAllText(_leasePath, Encoding.UTF8))
                ?? throw new JsonException("The lease document is null.");
            if (string.IsNullOrWhiteSpace(lease.OwnerUserSid) || lease.OwnerSessionId < 0 || lease.ActiveOperationId == Guid.Empty)
                throw new JsonException("The lease identity is invalid.");
            return lease;
        }
        catch (FileNotFoundException) { return null; } // No lease is normal on first start or after release.
        catch (DirectoryNotFoundException) { return null; }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            Logger.Warn(ex, "DisplayControlLeaseStore/Load: Could not read lease at {0}; starting without a lease. Reapply the desired profile if required.", _leasePath);
            return null;
        }
    }

    public void Save(DisplayControlLease lease)
    {
        ArgumentNullException.ThrowIfNull(lease);
        AtomicFileStore.WriteAllText(_leasePath, JsonSerializer.Serialize(lease), _leasePath + ".bak");
    }

    public void Clear()
    {
        if (File.Exists(_leasePath))
        {
            File.Delete(_leasePath);
        }
    }

}
