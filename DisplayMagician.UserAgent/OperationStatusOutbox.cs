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
    private readonly HashSet<Guid> _interrupted = new HashSet<Guid>();
    private readonly Dictionary<Guid, OperationStatusUpdate> _active = new Dictionary<Guid, OperationStatusUpdate>();
    private bool _writesEnabled = true;
    private bool _persistenceDirty;

    public OperationStatusOutbox(string userDataPath)
    {
        _path = Path.Combine(userDataPath ?? throw new ArgumentNullException(nameof(userDataPath)), "Settings", "OperationStatusOutbox.json");
        Load();
        _interrupted.UnionWith(_active.Keys);
    }

    public void Enqueue(OperationStatusUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);
        if (!OperationStatusValidation.IsValidUpdate(update)) throw new ArgumentException("The operation status update is invalid.", nameof(update));
        lock (_syncRoot)
        {
            update.UpdateId = update.UpdateId == Guid.Empty ? Guid.NewGuid() : update.UpdateId;
            OperationStatusUpdate? known = _pending.Concat(_active.Values).FirstOrDefault(item => item.UpdateId == update.UpdateId);
            if (known != null)
            {
                if (JsonSerializer.Serialize(known) != JsonSerializer.Serialize(update))
                    throw new ArgumentException("An update ID cannot be reused with different status data.", nameof(update));
                if (_persistenceDirty) TryPersist();
                return;
            }
            _pending.Add(Copy(update));
            if (update.IsTerminal) _active.Remove(update.OperationId); else _active[update.OperationId] = Copy(update);
            TryPersist();
        }
    }

    public OperationStatusUpdate[] GetPending() { lock (_syncRoot) return _pending.Select(Copy).ToArray(); }
    public OperationStatusUpdate[] GetActive() { lock (_syncRoot) return _active.Values.Select(Copy).ToArray(); }

    // Only operations loaded by this new Agent process are interrupted. A service
    // reconnect uses the same outbox instance and must preserve live operations.
    public void CompleteInterruptedOperations(bool recoveryRequired)
    {
        lock (_syncRoot)
        {
            foreach (Guid operationId in _interrupted)
            {
                if (!_active.TryGetValue(operationId, out OperationStatusUpdate? previous)) continue;
                Enqueue(new OperationStatusUpdate
                {
                    OperationId = operationId, OperationType = previous.OperationType,
                    Phase = OperationPhase.Failed, IsTerminal = true, IsSuccessful = false,
                    ErrorCode = recoveryRequired ? ControlErrorCode.RecoveryRequired : ControlErrorCode.ExecutionFailed,
                    Message = recoveryRequired
                        ? "The User Agent restarted during this operation. Original settings still require recovery."
                        : "The operation was interrupted by a User Agent restart. Startup recovery has completed."
                });
            }
            _interrupted.Clear();
        }
    }

    public void Acknowledge(Guid updateId)
        => TryAcknowledge(updateId);

    internal bool TryAcknowledge(Guid updateId)
    {
        lock (_syncRoot)
        {
            OperationStatusUpdate[] previous = _pending.ToArray();
            if (_pending.RemoveAll(update => update.UpdateId == updateId) == 0)
                return !_writesEnabled || !_persistenceDirty || TryPersist();
            // A corrupt file was never adopted: its new queue exists only in memory.
            // Service durability still permits acknowledging these new deliveries.
            if (!_writesEnabled || TryPersist()) return true;
            _pending.Clear();
            _pending.AddRange(previous);
            return false;
        }
    }

    private void Load()
    {
        try
        {
            string json = File.ReadAllText(_path);
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("SchemaVersion", out JsonElement version) || version.ValueKind != JsonValueKind.Number || !version.TryGetInt32(out int schema) || schema != SchemaVersion ||
                !document.RootElement.TryGetProperty("Pending", out JsonElement pending) || pending.ValueKind != JsonValueKind.Array ||
                !document.RootElement.TryGetProperty("Active", out JsonElement active) || active.ValueKind != JsonValueKind.Array)
                throw new JsonException("The outbox is incomplete or uses an unsupported schema.");
            PersistedOutbox stored = JsonSerializer.Deserialize<PersistedOutbox>(json) ?? throw new JsonException("The outbox is missing.");
            if (stored.Pending.Any(update => !OperationStatusValidation.IsValidUpdate(update, true)) ||
                stored.Active.Any(update => !OperationStatusValidation.IsValidUpdate(update, true) || update.IsTerminal) ||
                stored.Pending.GroupBy(update => update.UpdateId).Any(group => group.Count() != 1) ||
                stored.Active.GroupBy(update => update.OperationId).Any(group => group.Count() != 1) ||
                stored.Active.GroupBy(update => update.UpdateId).Any(group => group.Count() != 1))
                throw new JsonException("The outbox contains invalid or duplicate records.");
            foreach (OperationStatusUpdate update in stored.Active)
            {
                OperationStatusUpdate? matching = stored.Pending.FirstOrDefault(item => item.UpdateId == update.UpdateId);
                if (matching != null && JsonSerializer.Serialize(matching) != JsonSerializer.Serialize(update))
                    throw new JsonException("The outbox contains conflicting update identities.");
            }
            _pending.AddRange(stored.Pending.Select(Copy));
            foreach (OperationStatusUpdate update in stored.Active) _active.Add(update.OperationId, Copy(update));
        }
        catch (FileNotFoundException) when (!File.Exists(_path + ".bak") && !Directory.Exists(_path + ".bak")) { /* No prior delivery queue exists. */ }
        catch (DirectoryNotFoundException) when (!File.Exists(_path + ".bak") && !Directory.Exists(_path + ".bak")) { /* New user storage; the writer creates the directory. */ }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            _writesEnabled = false;
            Logger.Error(ex, "OperationStatusOutbox/Load: Could not load {0}. Preserving the status files until repair/restart; operations continue with in-memory status tracking.", _path);
        }
    }

    private bool TryPersist()
    {
        _persistenceDirty = true;
        if (!_writesEnabled) return false;
        try
        {
            AtomicFile.WriteAllText(_path, JsonSerializer.Serialize(new PersistedOutbox { SchemaVersion = SchemaVersion, Pending = _pending.ToArray(), Active = _active.Values.ToArray() }), Encoding.UTF8);
            _persistenceDirty = false;
            return true;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.Error(ex, "OperationStatusOutbox/TryPersist: Could not persist the operation-status outbox to {0}; keeping it in memory for retry.", _path);
            return false;
        }
    }

    private static OperationStatusUpdate Copy(OperationStatusUpdate source) => new OperationStatusUpdate { UpdateId = source.UpdateId, OperationId = source.OperationId, OperationType = source.OperationType, Phase = source.Phase, Message = source.Message, IsTerminal = source.IsTerminal, IsSuccessful = source.IsSuccessful, ErrorCode = source.ErrorCode };
    private sealed class PersistedOutbox { public int SchemaVersion { get; set; } public OperationStatusUpdate[] Pending { get; set; } = Array.Empty<OperationStatusUpdate>(); public OperationStatusUpdate[] Active { get; set; } = Array.Empty<OperationStatusUpdate>(); }
}
