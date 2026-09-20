using System;
using System.IO;
using System.Text.Json;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Persists administrator decisions that intentionally abandon automatic recovery.</summary>
public sealed class RecoveryAdministrationStore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly string _storagePath;

    public RecoveryAdministrationStore(StoragePaths storagePaths)
    {
        ArgumentNullException.ThrowIfNull(storagePaths);
        _storagePath = Path.Combine(storagePaths.MachineDiagnosticsPath, "RecoveryAdministration.json");
    }

    public RecoveryAdministrationRecord? GetLatest()
    {
        try
        {
            return File.Exists(_storagePath) ? JsonSerializer.Deserialize<RecoveryAdministrationRecord>(File.ReadAllText(_storagePath)) : null;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            Logger.Error(ex, "RecoveryAdministrationStore/GetLatest: Could not read recovery administration record from {0}.", _storagePath);
            return null;
        }
    }

    public void MarkForceReleased(DisplayControlLease releasedLease, string administratorSid, int administratorSessionId)
    {
        ArgumentNullException.ThrowIfNull(releasedLease);
        RecoveryAdministrationRecord record = new RecoveryAdministrationRecord
        {
            OccurredUtc = DateTime.UtcNow,
            Action = "ForceReleaseDisplayControl",
            Outcome = "RecoveryAbandoned",
            AdministratorSid = administratorSid,
            AdministratorSessionId = administratorSessionId,
            ReleasedLease = releasedLease
        };
        AtomicFileStore.WriteAllText(_storagePath, JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }), $"{_storagePath}.bak");
    }
}