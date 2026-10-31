using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Durable per-user replay history for the Gateway's future SSE adapter.</summary>
public sealed class GatewayEventJournal
{
    private const int MaximumEventsPerUser = 10000;
    private const int MaximumBytesPerUser = 10 * 1024 * 1024;
    private const int MaximumReplayEvents = 100;
    private const int MaximumReplayBytes = 1024 * 1024;
    private static readonly TimeSpan EventLifetime = TimeSpan.FromHours(24);
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _storagePath;
    private readonly string _pendingPath;
    private bool _available = true;
    private readonly Dictionary<Guid, string> _pendingEdits = new Dictionary<Guid, string>();
    private readonly HashSet<string> _uncertainUsers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, UserEvents> _users = new Dictionary<string, UserEvents>(StringComparer.OrdinalIgnoreCase);

    public GatewayEventJournal(StoragePaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        _storagePath = Path.Combine(paths.MachinePath, "GatewayEventJournal.json");
        _pendingPath = Path.Combine(paths.MachinePath, "GatewayEventJournal.pending.json");
        try
        {
            // A crash or failed event write can leave an authoritative edit without its event.
            // Retire its replay generation before accepting any old cursor.
            string[] owners = JsonSerializer.Deserialize<string[]>(File.ReadAllText(_pendingPath)) ?? throw new JsonException("Missing pending edit owners.");
            if (owners.Any(string.IsNullOrWhiteSpace)) throw new JsonException("Invalid pending edit owner.");
            foreach (string owner in owners) _uncertainUsers.Add(owner);
        }
        catch (FileNotFoundException) { } // No pending edit is the normal completed state.
        catch (DirectoryNotFoundException) { } // A new installation has no pending edit directory yet.
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            _available = false;
            Logger.Error(ex, "GatewayEventJournal/GatewayEventJournal: Pending edit recovery at {0} is invalid; preserving history and rejecting replay/edits until repair/restart.", _pendingPath);
        }
        // Validate recovery metadata before Load can prune and rewrite otherwise valid history.
        if (_available) Load();
    }

    public SavedResourceEdit BeginSavedResourceEdit(string ownerUserSid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        lock (_syncRoot)
        {
            EnsureAvailable();
            RecoverUnsafe(ownerUserSid);
            Guid id = Guid.NewGuid();
            _pendingEdits.Add(id, ownerUserSid);
            try { PersistPendingUnsafe(); }
            catch
            {
                _pendingEdits.Remove(id);
                throw;
            }
            return new SavedResourceEdit(this, id);
        }
    }

    public void MarkContinuityLost(string ownerUserSid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        lock (_syncRoot)
        {
            EnsureAvailable();
            _uncertainUsers.Add(ownerUserSid);
            PersistPendingUnsafe();
            RecoverUnsafe(ownerUserSid);
        }
    }

    public void Reconcile(OperationStatus[] operations, OperationDecision[] decisions, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(decisions);
        lock (_syncRoot)
        {
            // State files are authoritative. Detect a service crash between their commit and the journal callback.
            string[] owners = operations.Select(item => item.OwnerUserSid).Concat(decisions.Select(item => item.OwnerUserSid)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            try
            {
                foreach (string owner in owners)
                {
                    UserEvents user = GetOrCreateUserUnsafe(owner);
                    bool missingOperation = operations.Where(item => item.OwnerUserSid.Equals(owner, StringComparison.OrdinalIgnoreCase) && (!item.IsTerminal || item.UpdatedUtc > utcNow.Subtract(EventLifetime)))
                        .Any(item => user.Events.LastOrDefault(record => record.OperationStatus?.OperationId == item.OperationId)?.OperationStatus?.Sequence != item.Sequence);
                    bool missingDecision = decisions.Where(item => item.OwnerUserSid.Equals(owner, StringComparison.OrdinalIgnoreCase) && (!item.IsResolved || (item.ResolvedUtc ?? item.CreatedUtc) > utcNow.Subtract(EventLifetime)))
                        .Any(item =>
                        {
                            OperationDecision? recorded = user.Events.LastOrDefault(record => record.OperationDecision?.PromptId == item.PromptId)?.OperationDecision;
                            return recorded == null || recorded.IsResolved != item.IsResolved || recorded.ResolvedUtc != item.ResolvedUtc;
                        });
                    if (!missingOperation && !missingDecision) continue;
                    _uncertainUsers.Add(owner);
                    PersistPendingUnsafe();
                    RecoverUnsafe(owner);
                    UserEvents recovered = GetOrCreateUserUnsafe(owner);
                    foreach (OperationStatus item in operations.Where(item => item.OwnerUserSid.Equals(owner, StringComparison.OrdinalIgnoreCase)))
                        AddRecordUnsafe(recovered, owner, item.OwnerSessionId, ControlClientEventType.OperationStatusUpdated, item, null, null, utcNow);
                    foreach (OperationDecision item in decisions.Where(item => item.OwnerUserSid.Equals(owner, StringComparison.OrdinalIgnoreCase) && (!item.IsResolved || (item.ResolvedUtc ?? item.CreatedUtc) > utcNow.Subtract(EventLifetime))))
                        AddRecordUnsafe(recovered, owner, item.OwnerSessionId, ControlClientEventType.OperationDecisionUpdated, null, item, null, utcNow);
                    PruneUnsafe(recovered, utcNow);
                    PersistUnsafe();
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _uncertainUsers.UnionWith(owners);
                throw;
            }
        }
    }

    private void FinishSavedResourceEdit(Guid id, bool completed)
    {
        lock (_syncRoot)
        {
            if (!_pendingEdits.Remove(id, out string? owner)) return;
            if (!completed) _uncertainUsers.Add(owner);
            try
            {
                RecoverUnsafe(owner);
                PersistPendingUnsafe();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // The durable pending marker remains and forces a reset after restart.
                _uncertainUsers.Add(owner);
                Logger.Error(ex, "GatewayEventJournal/FinishSavedResourceEdit: Could not complete replay recovery for user {0}.", owner);
            }
        }
    }

    private void RecoverUnsafe(string owner)
    {
        if (!_uncertainUsers.Contains(owner)) return;
        _users.Remove(owner);
        _uncertainUsers.Remove(owner);
        try
        {
            GetOrCreateUserUnsafe(owner);
            PersistPendingUnsafe();
        }
        catch
        {
            _uncertainUsers.Add(owner);
            throw;
        }
    }

    private void PersistPendingUnsafe()
    {
        EnsureAvailable();
        string[] owners = _pendingEdits.Values.Concat(_uncertainUsers).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (owners.Length == 0)
        {
            if (File.Exists(_pendingPath)) File.Delete(_pendingPath);
        }
        else AtomicFileStore.WriteAllText(_pendingPath, JsonSerializer.Serialize(owners));
    }

    public void Append(OperationStatus status, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(status);
        Append(status.OwnerUserSid, status.OwnerSessionId, ControlClientEventType.OperationStatusUpdated, status, null, null, utcNow);
    }

    public void Append(OperationDecision decision, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(decision);
        Append(decision.OwnerUserSid, decision.OwnerSessionId, ControlClientEventType.OperationDecisionUpdated, null, decision, null, utcNow);
    }

    public void Append(string ownerUserSid, int ownerSessionId, GatewaySavedResourceChange change, DateTime utcNow)
    {
        AppendBatch(ownerUserSid, ownerSessionId, new[] { change }, utcNow);
    }

    public void AppendBatch(string ownerUserSid, int ownerSessionId, GatewaySavedResourceChange[] changes, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        ArgumentNullException.ThrowIfNull(changes);
        if (changes.Length == 0) return;
        ControlClientEventType[] eventTypes = changes.Select(change =>
        {
            if (change == null || string.IsNullOrWhiteSpace(change.Id) || change.Action is not ("upsert" or "deleted")) throw new ArgumentException("The saved resource change is invalid.", nameof(changes));
            return change.ResourceType switch
            {
                "display-profile" => ControlClientEventType.DisplayProfileChanged,
                "audio-profile" => ControlClientEventType.AudioProfileChanged,
                "shortcut" => ControlClientEventType.ShortcutChanged,
                _ => throw new ArgumentException("The saved resource type is invalid.", nameof(changes))
            };
        }).ToArray();
        lock (_syncRoot)
        {
            UserEvents user = GetOrCreateUserUnsafe(ownerUserSid);
            for (int index = 0; index < changes.Length; index++)
            {
                AddRecordUnsafe(user, ownerUserSid, ownerSessionId, eventTypes[index], null, null, changes[index], utcNow);
            }
            PruneUnsafe(user, utcNow);
            PersistUnsafe();
        }
    }

    public GatewayEventReplay ReadAfter(string ownerUserSid, string cursor, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        lock (_syncRoot)
        {
            UserEvents user = GetOrCreateUserUnsafe(ownerUserSid);
            if (PruneUnsafe(user, utcNow)) PersistUnsafe();
            string headId = HeadUnsafe(user);
            if (cursor.Split('.').LastOrDefault()?.StartsWith("b", StringComparison.Ordinal) == true && TryParseSequence(cursor, out long boundary) && IsValidCursor(user, cursor, boundary))
            {
                if (boundary >= user.NextSequence) return new GatewayEventReplay { State = GatewayEventReplayState.CursorUnknown, HeadId = headId };
                if (boundary < user.FirstSequence - 1) return new GatewayEventReplay { State = GatewayEventReplayState.CursorExpired, HeadId = headId };
                int first = user.Events.FindIndex(item => ParseSequence(item.Id) > boundary);
                return SliceUnsafe(user, first < 0 ? user.Events.Count : first, cursor, headId);
            }
            if (string.Equals(cursor, user.AnchorId, StringComparison.Ordinal))
            {
                return user.FirstSequence > 1
                    ? new GatewayEventReplay { State = GatewayEventReplayState.CursorExpired, HeadId = headId }
                    : SliceUnsafe(user, 0, cursor, headId);
            }

            int index = user.Events.FindIndex(item => string.Equals(item.Id, cursor, StringComparison.Ordinal));
            if (index >= 0)
            {
                return SliceUnsafe(user, index + 1, cursor, headId);
            }

            if (TryParseSequence(cursor, out long sequence) && sequence > 0 && sequence < user.NextSequence && IsValidCursor(user, cursor, sequence))
            {
                return new GatewayEventReplay { State = GatewayEventReplayState.CursorExpired, HeadId = headId };
            }

            return new GatewayEventReplay { State = GatewayEventReplayState.CursorUnknown, HeadId = headId };
        }
    }

    public string GetHead(string ownerUserSid, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        lock (_syncRoot)
        {
            UserEvents user = GetOrCreateUserUnsafe(ownerUserSid);
            if (PruneUnsafe(user, utcNow)) PersistUnsafe();
            return HeadUnsafe(user);
        }
    }

    private void Append(string ownerUserSid, int ownerSessionId, ControlClientEventType eventType, OperationStatus? status, OperationDecision? decision, GatewaySavedResourceChange? resourceChange, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserSid);
        lock (_syncRoot)
        {
            UserEvents user = GetOrCreateUserUnsafe(ownerUserSid);
            AddRecordUnsafe(user, ownerUserSid, ownerSessionId, eventType, status, decision, resourceChange, utcNow);
            PruneUnsafe(user, utcNow);
            PersistUnsafe();
        }
    }

    private static void AddRecordUnsafe(UserEvents user, string ownerUserSid, int ownerSessionId, ControlClientEventType eventType, OperationStatus? status, OperationDecision? decision, GatewaySavedResourceChange? resourceChange, DateTime utcNow)
    {
        long sequence = user.NextSequence++;
        GatewayEventRecord record = new GatewayEventRecord
        {
            Id = CreateCursor(user, sequence),
            OwnerUserSid = ownerUserSid,
            OwnerSessionId = ownerSessionId,
            PublishedUtc = utcNow.ToUniversalTime(),
            EventType = eventType,
            OperationStatus = status,
            OperationDecision = decision,
            ResourceChange = resourceChange
        };
        byte[] snapshot = JsonSerializer.SerializeToUtf8Bytes(record);
        user.Events.Add(JsonSerializer.Deserialize<GatewayEventRecord>(snapshot)!);
        user.RetainedBytes += snapshot.Length + 1;
    }

    private UserEvents GetOrCreateUserUnsafe(string ownerUserSid)
    {
        EnsureAvailable();
        if (_uncertainUsers.Contains(ownerUserSid)) RecoverUnsafe(ownerUserSid);
        if (_users.TryGetValue(ownerUserSid, out UserEvents? user)) return user;
        user = new UserEvents { OwnerUserSid = ownerUserSid, CursorSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) };
        user.AnchorId = CreateCursor(user, 0);
        _users.Add(ownerUserSid, user);
        try { PersistUnsafe(); }
        catch
        {
            _users.Remove(ownerUserSid);
            throw;
        }
        return user;
    }

    private static GatewayEventReplay SliceUnsafe(UserEvents user, int startIndex, string cursor, string headId)
    {
        List<GatewayEventRecord> events = new List<GatewayEventRecord>();
        int bytes = 0;
        for (int index = startIndex; index < user.Events.Count && events.Count < MaximumReplayEvents; index++)
        {
            GatewayEventRecord record = user.Events[index];
            int recordBytes = JsonSerializer.SerializeToUtf8Bytes(record).Length + 1;
            if (events.Count > 0 && bytes + recordBytes > MaximumReplayBytes) break;
            events.Add(JsonSerializer.Deserialize<GatewayEventRecord>(JsonSerializer.Serialize(record))!);
            bytes += recordBytes;
        }

        return new GatewayEventReplay
        {
            State = GatewayEventReplayState.Available,
            HeadId = headId,
            NextCursor = events.Count == 0 ? cursor : events[^1].Id,
            HasMore = startIndex + events.Count < user.Events.Count,
            Events = events.ToArray()
        };
    }

    private static bool PruneUnsafe(UserEvents user, DateTime utcNow)
    {
        int oldCount = user.Events.Count;
        for (int index = user.Events.Count - 1; index >= 0; index--)
        {
            if (user.Events[index].PublishedUtc > utcNow.ToUniversalTime().Subtract(EventLifetime)) continue;
            user.RetainedBytes -= JsonSerializer.SerializeToUtf8Bytes(user.Events[index]).Length + 1;
            user.Events.RemoveAt(index);
        }
        while (user.Events.Count > MaximumEventsPerUser || user.RetainedBytes + 2 > MaximumBytesPerUser)
        {
            user.RetainedBytes -= JsonSerializer.SerializeToUtf8Bytes(user.Events[0]).Length + 1;
            user.Events.RemoveAt(0);
        }
        user.FirstSequence = user.Events.Count == 0 ? user.NextSequence : ParseSequence(user.Events[0].Id);
        return oldCount != user.Events.Count;
    }

    private static string CreateCursor(UserEvents user, long sequence, bool boundary = false)
    {
        byte[] secret = Convert.FromBase64String(user.CursorSecret);
        byte[] digest = HMACSHA256.HashData(secret, Encoding.UTF8.GetBytes((boundary ? "boundary:" : string.Empty) + sequence.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        return $"v1.{sequence}.{(boundary ? "b" : string.Empty)}{Convert.ToHexString(digest.AsSpan(0, 16))}";
    }

    private static string HeadUnsafe(UserEvents user)
    {
        if (user.Events.Count > 0) return user.Events[^1].Id;
        if (user.NextSequence == 1) return user.AnchorId;
        return CreateCursor(user, user.NextSequence - 1, boundary: true);
    }

    private static bool IsValidCursor(UserEvents user, string cursor, long sequence)
    {
        string expected = CreateCursor(user, sequence, boundary: cursor.Split('.').LastOrDefault()?.StartsWith("b", StringComparison.Ordinal) == true);
        return cursor.Length == expected.Length && CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(cursor), Encoding.ASCII.GetBytes(expected));
    }

    private static bool TryParseSequence(string cursor, out long sequence)
    {
        sequence = 0;
        if (string.IsNullOrWhiteSpace(cursor)) return false;
        string[] parts = cursor.Split('.');
        return parts.Length == 3 && parts[0] == "v1" && long.TryParse(parts[1], out sequence) && sequence >= 0 &&
            (parts[2].Length == 32 && parts[2].All(Uri.IsHexDigit) || parts[2].Length == 33 && parts[2][0] == 'b' && parts[2].Substring(1).All(Uri.IsHexDigit));
    }

    private static long ParseSequence(string cursor) => TryParseSequence(cursor, out long sequence) ? sequence : 0;

    private void Load()
    {
        try
        {
            UserEvents[] persisted = JsonSerializer.Deserialize<UserEvents[]>(File.ReadAllText(_storagePath))
                ?? throw new JsonException("Missing replay history.");
            HashSet<string> owners = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (UserEvents user in persisted)
            {
                if (user == null || string.IsNullOrWhiteSpace(user.OwnerUserSid) || !owners.Add(user.OwnerUserSid)
                    || !TryParseSequence(user.AnchorId, out long anchorSequence) || anchorSequence != 0 || string.IsNullOrEmpty(user.CursorSecret)
                    || Convert.FromBase64String(user.CursorSecret).Length != 32 || !IsValidCursor(user, user.AnchorId, 0)
                    || user.Events == null || user.NextSequence < 1 || user.FirstSequence < 1)
                    throw new JsonException("Invalid replay owner or cursor metadata.");
                long previousSequence = 0;
                foreach (GatewayEventRecord item in user.Events)
                {
                    if (item == null || !string.Equals(item.OwnerUserSid, user.OwnerUserSid, StringComparison.OrdinalIgnoreCase)
                        || !TryParseSequence(item.Id, out long sequence) || sequence <= previousSequence || sequence >= user.NextSequence
                        || !IsValidCursor(user, item.Id, sequence)) throw new JsonException("Invalid replay event or sequence.");
                    bool validPayload = item.EventType switch
                    {
                        ControlClientEventType.OperationStatusUpdated => item.OperationStatus != null && item.OperationDecision == null && item.ResourceChange == null
                            && string.Equals(item.OperationStatus.OwnerUserSid, user.OwnerUserSid, StringComparison.OrdinalIgnoreCase),
                        ControlClientEventType.OperationDecisionUpdated => item.OperationDecision != null && item.OperationStatus == null && item.ResourceChange == null
                            && string.Equals(item.OperationDecision.OwnerUserSid, user.OwnerUserSid, StringComparison.OrdinalIgnoreCase),
                        ControlClientEventType.DisplayProfileChanged or ControlClientEventType.AudioProfileChanged or ControlClientEventType.ShortcutChanged =>
                            item.OperationStatus == null && item.OperationDecision == null && item.ResourceChange != null
                            && !string.IsNullOrWhiteSpace(item.ResourceChange.Id) && item.ResourceChange.Action is "upsert" or "deleted"
                            && item.ResourceChange.ResourceType == (item.EventType == ControlClientEventType.DisplayProfileChanged ? "display-profile"
                                : item.EventType == ControlClientEventType.AudioProfileChanged ? "audio-profile" : "shortcut"),
                        _ => false
                    };
                    if (!validPayload || item.PublishedUtc == default) throw new JsonException("Invalid or unsupported replay event payload.");
                    previousSequence = sequence;
                    if (item.OwnerSessionId == 0) item.OwnerSessionId = item.OperationStatus?.OwnerSessionId ?? item.OperationDecision?.OwnerSessionId ?? 0;
                }
                if (user.FirstSequence != (user.Events.Count == 0 ? user.NextSequence : ParseSequence(user.Events[0].Id)))
                    throw new JsonException("Invalid replay boundary.");
            }
            // Validate the complete file before publishing or pruning any owner's history.
            bool pruned = false;
            foreach (UserEvents user in persisted)
            {
                user.RetainedBytes = user.Events.Sum(item => (long)JsonSerializer.SerializeToUtf8Bytes(item).Length + 1);
                pruned |= PruneUnsafe(user, DateTime.UtcNow);
                _users.Add(user.OwnerUserSid, user);
            }
            if (pruned) PersistUnsafe();
        }
        catch (FileNotFoundException) when (!File.Exists(_storagePath + ".bak") && !Directory.Exists(_storagePath + ".bak")) { } // New journal.
        catch (DirectoryNotFoundException) when (!File.Exists(_storagePath + ".bak") && !Directory.Exists(_storagePath + ".bak")) { } // New installation.
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or FormatException)
        {
            _available = false;
            Logger.Error(ex, "GatewayEventJournal/Load: Replay history at {0} is unavailable; preserving files and rejecting replay/edits until repair/restart.", _storagePath);
        }
    }

    private void EnsureAvailable()
    {
        if (!_available) throw new IOException("Gateway replay history requires repair and service restart before replay or saved-resource edits are available.");
    }

    private void PersistUnsafe()
    {
        EnsureAvailable();
        try
        {
            AtomicFileStore.WriteAllText(_storagePath, JsonSerializer.Serialize(_users.Values.ToArray()), _storagePath + ".bak");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Logger.Error(ex, "GatewayEventJournal/PersistUnsafe: Could not persist replay history at {0}.", _storagePath);
            throw;
        }
    }

    public sealed class UserEvents
    {
        public string OwnerUserSid { get; set; } = string.Empty;
        public string AnchorId { get; set; } = string.Empty;
        public string CursorSecret { get; set; } = string.Empty;
        public long FirstSequence { get; set; } = 1;
        public long NextSequence { get; set; } = 1;
        public List<GatewayEventRecord> Events { get; set; } = new List<GatewayEventRecord>();
        [JsonIgnore]
        public long RetainedBytes { get; set; }
    }

    public sealed class SavedResourceEdit : IDisposable
    {
        private GatewayEventJournal? _owner;
        private readonly Guid _id;
        private bool _completed;
        internal SavedResourceEdit(GatewayEventJournal owner, Guid id) { _owner = owner; _id = id; }
        public void Complete() => _completed = true;
        public void Dispose() => System.Threading.Interlocked.Exchange(ref _owner, null)?.FinishSavedResourceEdit(_id, _completed);
    }
}
