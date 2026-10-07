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
        return TryGetAll(out RecoveryAdministrationRecord[] records) ? records : Array.Empty<RecoveryAdministrationRecord>();
    }

    public bool MarkForceReleased(DisplayControlLease releasedLease, string administratorSid, int administratorSessionId)
    {
        ArgumentNullException.ThrowIfNull(releasedLease);
        return Record("ForceReleaseDisplayControl", "RecoveryAbandoned", administratorSid, administratorSessionId, releasedLease);
    }

    public bool Record(string action, string outcome, string actorSid, int actorSessionId, DisplayControlLease? releasedLease = null)
    {
        if (!TryGetAll(out RecoveryAdministrationRecord[] existingRecords))
        {
            return false;
        }

        try
        {
            List<RecoveryAdministrationRecord> records = existingRecords.ToList();
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
            return true;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.Error(ex, "RecoveryAdministrationStore/Record: Could not write recovery administration record to {0}.", _storagePath);
            return false;
        }
    }

    private bool TryGetAll(out RecoveryAdministrationRecord[] records)
    {
        records = Array.Empty<RecoveryAdministrationRecord>();
        try
        {
            if (!File.Exists(_storagePath))
            {
                return true;
            }

            string json = File.ReadAllText(_storagePath);
            RecoveryAdministrationHistory? history = JsonSerializer.Deserialize<RecoveryAdministrationHistory>(json);
            if (history?.Records?.Length > 0)
            {
                records = history.Records;
                return true;
            }

            RecoveryAdministrationRecord? legacyRecord = JsonSerializer.Deserialize<RecoveryAdministrationRecord>(json);
            records = legacyRecord == null || string.IsNullOrWhiteSpace(legacyRecord.Action) ? Array.Empty<RecoveryAdministrationRecord>() : [legacyRecord];
            return true;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            Logger.Error(ex, "RecoveryAdministrationStore/TryGetAll: Could not read recovery administration records from {0}.", _storagePath);
            return false;
        }
    }
}
