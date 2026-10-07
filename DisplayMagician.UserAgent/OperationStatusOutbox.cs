using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent.Runtime;
using NLog;

namespace DisplayMagician.UserAgent;

/// <summary>Durable Agent-side delivery queue and live-operation snapshot. The service may restart independently.</summary>
public sealed class OperationStatusOutbox
{
    private const int SchemaVersion = 1;
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _path;
    private readonly List<OperationStatusUpdate> _pending = new List<OperationStatusUpdate>();
    private readonly Dictionary<Guid, OperationStatusUpdate> _active = new Dictionary<Guid, OperationStatusUpdate>();

    public OperationStatusOutbox(string userDataPath)
    {
        _path = Path.Combine(userDataPath ?? throw new ArgumentNullException(nameof(userDataPath)), "Settings", "OperationStatusOutbox.json");
        Load();
    }

    public void Enqueue(OperationStatusUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);
        if (update.OperationId == Guid.Empty) throw new ArgumentException("An operation ID is required.", nameof(update));
        lock (_syncRoot)
        {
            update.UpdateId = update.UpdateId == Guid.Empty ? Guid.NewGuid() : update.UpdateId;
            _pending.Add(Copy(update));
            if (update.IsTerminal) _active.Remove(update.OperationId); else _active[update.OperationId] = Copy(update);
            Persist();
        }
    }

    public OperationStatusUpdate[] GetPending() { lock (_syncRoot) return _pending.Select(Copy).ToArray(); }
    public OperationStatusUpdate[] GetActive() { lock (_syncRoot) return _active.Values.Select(Copy).ToArray(); }

    public void Acknowledge(Guid updateId)
    {
        lock (_syncRoot)
        {
            if (_pending.RemoveAll(update => update.UpdateId == updateId) > 0) Persist();
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_path)) return;
            PersistedOutbox? stored = JsonSerializer.Deserialize<PersistedOutbox>(File.ReadAllText(_path));
            if (stored == null || stored.SchemaVersion != SchemaVersion) return;
            _pending.AddRange((stored.Pending ?? Array.Empty<OperationStatusUpdate>()).Where(IsValid).Select(Copy));
            foreach (OperationStatusUpdate update in (stored.Active ?? Array.Empty<OperationStatusUpdate>()).Where(IsValid)) _active[update.OperationId] = Copy(update);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            Logger.Error(ex, "OperationStatusOutbox/Load: Could not load the persisted operation-status outbox from {0}.", _path);
        }
    }

    private void Persist()
    {
        try
        {
            AtomicFile.WriteAllText(_path, JsonSerializer.Serialize(new PersistedOutbox { SchemaVersion = SchemaVersion, Pending = _pending.ToArray(), Active = _active.Values.ToArray() }), Encoding.UTF8);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.Error(ex, "OperationStatusOutbox/Persist: Could not persist the operation-status outbox to {0}.", _path);
        }
    }

    private static bool IsValid(OperationStatusUpdate update) => update != null && update.OperationId != Guid.Empty && update.UpdateId != Guid.Empty;
    private static OperationStatusUpdate Copy(OperationStatusUpdate source) => new OperationStatusUpdate { UpdateId = source.UpdateId, OperationId = source.OperationId, OperationType = source.OperationType, Phase = source.Phase, Message = source.Message, IsTerminal = source.IsTerminal, IsSuccessful = source.IsSuccessful, ErrorCode = source.ErrorCode };
    private sealed class PersistedOutbox { public int SchemaVersion { get; set; } public OperationStatusUpdate[] Pending { get; set; } = Array.Empty<OperationStatusUpdate>(); public OperationStatusUpdate[] Active { get; set; } = Array.Empty<OperationStatusUpdate>(); }
}
