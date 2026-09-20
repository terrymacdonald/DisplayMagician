using System;
using System.IO;
using System.Text.Json;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Persists machine-wide client-sync and anonymous-metrics schedule state.</summary>
public sealed class MachineScheduleStore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _storagePath;
    private MachineScheduleState _state;

    public MachineScheduleStore(StoragePaths storagePaths)
    {
        ArgumentNullException.ThrowIfNull(storagePaths);
        _storagePath = Path.Combine(storagePaths.MachinePath, "ScheduleState.json");
        _state = Load();
    }

    public MachineScheduleState Get()
    {
        lock (_syncRoot)
        {
            return Copy(_state);
        }
    }

    public MachineScheduleState Update(Action<MachineScheduleState> update)
    {
        ArgumentNullException.ThrowIfNull(update);
        lock (_syncRoot)
        {
            MachineScheduleState updated = Copy(_state);
            update(updated);
            Normalize(updated);
            Persist(updated);
            _state = updated;
            return Copy(updated);
        }
    }

    private MachineScheduleState Load()
    {
        try
        {
            if (!File.Exists(_storagePath))
            {
                return new MachineScheduleState();
            }

            MachineScheduleState? loaded = JsonSerializer.Deserialize<MachineScheduleState>(File.ReadAllText(_storagePath));
            if (loaded == null)
            {
                return new MachineScheduleState();
            }

            Normalize(loaded);
            return loaded;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            Logger.Error(ex, "MachineScheduleStore/Load: Could not load schedule state from {0}.", _storagePath);
            return new MachineScheduleState();
        }
    }

    private void Persist(MachineScheduleState state)
    {
        try
        {
            AtomicFileStore.WriteAllText(_storagePath, JsonSerializer.Serialize(state), $"{_storagePath}.bak");
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.Error(ex, "MachineScheduleStore/Persist: Could not persist schedule state to {0}.", _storagePath);
            throw;
        }
    }

    private static void Normalize(MachineScheduleState state)
    {
        state.NextClientSyncUtc = state.NextClientSyncUtc?.ToUniversalTime();
        state.LastSuccessfulClientSyncUtc = state.LastSuccessfulClientSyncUtc?.ToUniversalTime();
        state.NextMetricsHeartbeatUtc = state.NextMetricsHeartbeatUtc?.ToUniversalTime();
        state.ConsecutiveClientSyncFailures = Math.Max(0, state.ConsecutiveClientSyncFailures);
        state.TotalAnonymousMetricLaunches = Math.Max(0, state.TotalAnonymousMetricLaunches);
        state.TotalAnonymousMetricActiveMinutes = Math.Max(0, state.TotalAnonymousMetricActiveMinutes);
    }

    private static MachineScheduleState Copy(MachineScheduleState state)
    {
        return new MachineScheduleState
        {
            InstallId = state.InstallId,
            NextClientSyncUtc = state.NextClientSyncUtc,
            LastSuccessfulClientSyncUtc = state.LastSuccessfulClientSyncUtc,
            ConsecutiveClientSyncFailures = state.ConsecutiveClientSyncFailures,
            NextMetricsHeartbeatUtc = state.NextMetricsHeartbeatUtc,
            LastMetricsReportedVersion = state.LastMetricsReportedVersion,
            ShareAnonymousUsageMetrics = state.ShareAnonymousUsageMetrics,
            TotalAnonymousMetricLaunches = state.TotalAnonymousMetricLaunches,
            TotalAnonymousMetricActiveMinutes = state.TotalAnonymousMetricActiveMinutes
        };
    }
}

public sealed class MachineScheduleState
{
    public string InstallId { get; set; } = string.Empty;
    public DateTime? NextClientSyncUtc { get; set; }
    public DateTime? LastSuccessfulClientSyncUtc { get; set; }
    public int ConsecutiveClientSyncFailures { get; set; }
    public DateTime? NextMetricsHeartbeatUtc { get; set; }
    public string LastMetricsReportedVersion { get; set; } = string.Empty;
    public bool ShareAnonymousUsageMetrics { get; set; } = true;
    public long TotalAnonymousMetricLaunches { get; set; }
    public long TotalAnonymousMetricActiveMinutes { get; set; }
}