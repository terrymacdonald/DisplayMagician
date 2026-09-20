using System;

namespace DisplayMagician.ControlService;

/// <summary>Applies the machine-wide scheduling and backoff policy for background service work.</summary>
public sealed class MachineScheduleCoordinator
{
    private readonly MachineScheduleStore _store;

    public MachineScheduleCoordinator(MachineScheduleStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public MachineScheduleState EnsureInitialized()
    {
        return _store.Update(state =>
        {
            if (string.IsNullOrWhiteSpace(state.InstallId))
            {
                state.InstallId = Guid.NewGuid().ToString();
            }
        });
    }

    public bool IsClientSyncDue(DateTime utcNow)
    {
        MachineScheduleState state = EnsureInitialized();
        return !state.NextClientSyncUtc.HasValue || utcNow >= state.NextClientSyncUtc.Value;
    }

    public MachineScheduleState RecordClientSyncSuccess(DateTime utcNow)
    {
        return _store.Update(state =>
        {
            EnsureInstallId(state);
            state.LastSuccessfulClientSyncUtc = utcNow;
            state.ConsecutiveClientSyncFailures = 0;
            state.NextClientSyncUtc = utcNow.AddMinutes((24 * 60) + GetStableJitterMinutes(state.InstallId));
        });
    }

    public MachineScheduleState RecordClientSyncFailure(DateTime utcNow)
    {
        return _store.Update(state =>
        {
            EnsureInstallId(state);
            state.ConsecutiveClientSyncFailures = Math.Min(state.ConsecutiveClientSyncFailures + 1, 6);
            state.NextClientSyncUtc = utcNow.AddHours(Math.Min(Math.Pow(2, state.ConsecutiveClientSyncFailures - 1), 24));
        });
    }

    public bool IsMetricsHeartbeatDue(DateTime utcNow, string appVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appVersion);
        MachineScheduleState state = EnsureInitialized();
        return !string.Equals(state.LastMetricsReportedVersion, appVersion, StringComparison.OrdinalIgnoreCase) ||
            !state.NextMetricsHeartbeatUtc.HasValue || utcNow >= state.NextMetricsHeartbeatUtc.Value;
    }

    public MachineScheduleState RecordMetricsHeartbeatSuccess(DateTime utcNow, string appVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appVersion);
        return _store.Update(state =>
        {
            EnsureInstallId(state);
            state.LastMetricsReportedVersion = appVersion;
            state.NextMetricsHeartbeatUtc = GetNextWeeklyHeartbeatUtc(utcNow, state.InstallId);
        });
    }

    private static void EnsureInstallId(MachineScheduleState state)
    {
        if (string.IsNullOrWhiteSpace(state.InstallId))
        {
            state.InstallId = Guid.NewGuid().ToString();
        }
    }

    private static int GetStableJitterMinutes(string installId)
    {
        unchecked
        {
            int hash = 17;
            foreach (char character in installId ?? string.Empty)
            {
                hash = (hash * 31) + character;
            }

            return (int)((uint)hash % (12 * 60 + 1));
        }
    }

    private static DateTime GetNextWeeklyHeartbeatUtc(DateTime utcNow, string installId)
    {
        unchecked
        {
            int hash = 17;
            foreach (char character in installId ?? string.Empty)
            {
                hash = (hash * 31) + character;
            }

            int positiveHash = Math.Abs(hash);
            int daySlot = positiveHash % 7;
            int hourOffset = Math.Abs((positiveHash / 7) % 7);
            DateTime earliestUtc = utcNow.AddDays(7);
            DateTime weekStartUtc = earliestUtc.Date.AddDays(-(int)earliestUtc.DayOfWeek);
            DateTime scheduledUtc = weekStartUtc.AddDays(daySlot).AddHours(hourOffset);
            while (scheduledUtc < earliestUtc)
            {
                scheduledUtc = scheduledUtc.AddDays(7);
            }

            return scheduledUtc;
        }
    }
}