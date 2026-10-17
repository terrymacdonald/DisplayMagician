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
    private readonly Dictionary<string, UserEvents> _users = new Dictionary<string, UserEvents>(StringComparer.OrdinalIgnoreCase);

    public GatewayEventJournal(StoragePaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        _storagePath = Path.Combine(paths.MachinePath, "GatewayEventJournal.json");
        Load();
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
            string headId = user.Events.Count == 0 ? user.AnchorId : user.Events[^1].Id;
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
            return user.Events.Count == 0 ? user.AnchorId : user.Events[^1].Id;
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
        user.Events.Add(record);
        user.RetainedBytes += JsonSerializer.SerializeToUtf8Bytes(record).Length + 1;
    }

    private UserEvents GetOrCreateUserUnsafe(string ownerUserSid)
    {
        if (_users.TryGetValue(ownerUserSid, out UserEvents? user)) return user;
        user = new UserEvents { OwnerUserSid = ownerUserSid, CursorSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) };
        user.AnchorId = CreateCursor(user, 0);
        _users.Add(ownerUserSid, user);
        PersistUnsafe();
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
            events.Add(record);
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
            if (user.Events[index].PublishedUtc.Add(EventLifetime) > utcNow.ToUniversalTime()) continue;
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

    private static string CreateCursor(UserEvents user, long sequence)
    {
        byte[] secret = Convert.FromBase64String(user.CursorSecret);
        byte[] digest = HMACSHA256.HashData(secret, Encoding.UTF8.GetBytes(sequence.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        return $"v1.{sequence}.{Convert.ToHexString(digest.AsSpan(0, 16))}";
    }

    private static bool IsValidCursor(UserEvents user, string cursor, long sequence)
    {
        string expected = CreateCursor(user, sequence);
        return cursor.Length == expected.Length && CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(cursor), Encoding.ASCII.GetBytes(expected));
    }

    private static bool TryParseSequence(string cursor, out long sequence)
    {
        sequence = 0;
        if (string.IsNullOrWhiteSpace(cursor)) return false;
        string[] parts = cursor.Split('.');
        return parts.Length == 3 && parts[0] == "v1" && long.TryParse(parts[1], out sequence) && sequence >= 0 && parts[2].Length == 32 && parts[2].All(Uri.IsHexDigit);
    }

    private static long ParseSequence(string cursor) => TryParseSequence(cursor, out long sequence) ? sequence : 0;

    private void Load()
    {
        try
        {
            if (!File.Exists(_storagePath)) return;
            UserEvents[]? persisted = JsonSerializer.Deserialize<UserEvents[]>(File.ReadAllText(_storagePath));
            foreach (UserEvents user in persisted ?? Array.Empty<UserEvents>())
            {
                if (string.IsNullOrWhiteSpace(user.OwnerUserSid) || !TryParseSequence(user.AnchorId, out long anchorSequence) || anchorSequence != 0 || string.IsNullOrEmpty(user.CursorSecret)) continue;
                if (!IsValidCursor(user, user.AnchorId, 0)) continue;
                user.Events ??= new List<GatewayEventRecord>();
                user.Events = user.Events.Where(item => item.OwnerUserSid.Equals(user.OwnerUserSid, StringComparison.OrdinalIgnoreCase) && TryParseSequence(item.Id, out long sequence) && sequence > 0 && IsValidCursor(user, item.Id, sequence))
                    .OrderBy(item => ParseSequence(item.Id)).ToList();
                foreach (GatewayEventRecord item in user.Events)
                {
                    if (item.OwnerSessionId == 0) item.OwnerSessionId = item.OperationStatus?.OwnerSessionId ?? item.OperationDecision?.OwnerSessionId ?? 0;
                }
                user.RetainedBytes = user.Events.Sum(item => (long)JsonSerializer.SerializeToUtf8Bytes(item).Length + 1);
                user.NextSequence = Math.Max(user.NextSequence, user.Events.Count == 0 ? 1 : ParseSequence(user.Events[^1].Id) + 1);
                PruneUnsafe(user, DateTime.UtcNow);
                _users[user.OwnerUserSid] = user;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or FormatException)
        {
            Logger.Error(ex, "GatewayEventJournal/Load: Could not read replay history at {0}.", _storagePath);
        }
    }

    private void PersistUnsafe()
    {
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
}
