using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        return GetAll().LastOrDefault();
    }

    public RecoveryAdministrationRecord[] GetAll()
    {
        try
        {
            if (!File.Exists(_storagePath))
            {
                return Array.Empty<RecoveryAdministrationRecord>();
            }

            string json = File.ReadAllText(_storagePath);
            RecoveryAdministrationHistory? history = JsonSerializer.Deserialize<RecoveryAdministrationHistory>(json);
            if (history?.Records?.Length > 0)
            {
                return history.Records;
            }

            RecoveryAdministrationRecord? legacyRecord = JsonSerializer.Deserialize<RecoveryAdministrationRecord>(json);
            return legacyRecord == null || string.IsNullOrWhiteSpace(legacyRecord.Action) ? Array.Empty<RecoveryAdministrationRecord>() : [legacyRecord];
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            Logger.Error(ex, "RecoveryAdministrationStore/GetLatest: Could not read recovery administration record from {0}.", _storagePath);
            return Array.Empty<RecoveryAdministrationRecord>();
        }
    }

    public void MarkForceReleased(DisplayControlLease releasedLease, string administratorSid, int administratorSessionId)
    {
        ArgumentNullException.ThrowIfNull(releasedLease);
        Record("ForceReleaseDisplayControl", "RecoveryAbandoned", administratorSid, administratorSessionId, releasedLease);
    }

    public void Record(string action, string outcome, string actorSid, int actorSessionId, DisplayControlLease? releasedLease = null)
    {
        List<RecoveryAdministrationRecord> records = GetAll().ToList();
        records.Add(new RecoveryAdministrationRecord
        {
            OccurredUtc = DateTime.UtcNow,
            Action = action,
            Outcome = outcome,
            AdministratorSid = actorSid,
            AdministratorSessionId = actorSessionId,
            ReleasedLease = releasedLease
        });
        AtomicFileStore.WriteAllText(_storagePath, JsonSerializer.Serialize(new RecoveryAdministrationHistory { Records = records.TakeLast(50).ToArray() }, new JsonSerializerOptions { WriteIndented = true }), $"{_storagePath}.bak");
    }
}
