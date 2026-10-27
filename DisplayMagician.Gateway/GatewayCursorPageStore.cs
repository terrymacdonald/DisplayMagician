using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DisplayMagician.Gateway;

/// <summary>Short-lived, device-scoped snapshots for stable REST collection paging.</summary>
public sealed class GatewayCursorPageStore
{
    private static readonly TimeSpan SnapshotLifetime = TimeSpan.FromMinutes(10);
    private const int MaximumSnapshots = 128;
    private const int MaximumSnapshotsPerDevice = 8;
    private const int MaximumSnapshotBytes = 64 * 1024 * 1024;
    private const int MaximumPageBytes = GatewayJsonResponseLimitMiddleware.MaximumJsonResponseBytes - 4096;
    private static readonly JsonSerializerOptions WebJson = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    private readonly object _syncRoot = new object();
    private readonly Dictionary<Guid, Snapshot> _snapshots = new Dictionary<Guid, Snapshot>();

    public GatewayCursorPageResult<T> Create<T>(string deviceScope, string route, string filters, T[] items, int limit, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (limit is < 1 or > 100) return new GatewayCursorPageResult<T>(GatewayCursorPageState.Invalid);
        byte[] serializedItems = JsonSerializer.SerializeToUtf8Bytes(items, WebJson);
        int snapshotBytes = serializedItems.Length;
        if (snapshotBytes > MaximumSnapshotBytes) return new GatewayCursorPageResult<T>(GatewayCursorPageState.TooLarge);
        // A copied array still shares mutable DTOs and their nested collections.
        // Capture the representation so later caller changes cannot alter a page.
        T[] capturedItems = JsonSerializer.Deserialize<T[]>(serializedItems, WebJson)!;

        lock (_syncRoot)
        {
            PruneUnsafe(utcNow);
            Guid snapshotId = Guid.NewGuid();
            Snapshot snapshot = new Snapshot(snapshotId, deviceScope, route, filters, capturedItems, typeof(T), utcNow.ToUniversalTime().Add(SnapshotLifetime), snapshotBytes);
            GatewayCursorPageResult<T> first = Slice<T>(snapshot, 0, limit);
            if (first.State != GatewayCursorPageState.Success || first.NextCursor == null) return first;

            _snapshots.Add(snapshotId, snapshot);
            while (_snapshots.Values.Count(candidate => candidate.DeviceScope == deviceScope) > MaximumSnapshotsPerDevice)
            {
                Snapshot oldestForDevice = _snapshots.Values.Where(candidate => candidate.DeviceScope == deviceScope && candidate.Id != snapshotId).OrderBy(candidate => candidate.CreatedUtc).First();
                _snapshots.Remove(oldestForDevice.Id);
            }
            while (_snapshots.Count > MaximumSnapshots || _snapshots.Values.Sum(candidate => (long)candidate.SizeBytes) > MaximumSnapshotBytes)
            {
                Snapshot oldest = _snapshots.Values.Where(candidate => candidate.Id != snapshotId).OrderBy(candidate => candidate.CreatedUtc).First();
                _snapshots.Remove(oldest.Id);
            }
            return first;
        }
    }

    public GatewayCursorPageResult<T> Read<T>(string deviceScope, string route, string filters, string cursor, int limit, DateTime utcNow)
    {
        if (limit is < 1 or > 100 || string.IsNullOrWhiteSpace(cursor)) return new GatewayCursorPageResult<T>(GatewayCursorPageState.Invalid);
        string[] parts = cursor.Split('.');
        if (parts.Length != 4 || parts[0] != "v1" || !Guid.TryParseExact(parts[1], "N", out Guid snapshotId) ||
            !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out int offset) || offset < 1 ||
            parts[3].Length != 32 || !parts[3].All(Uri.IsHexDigit))
        {
            return new GatewayCursorPageResult<T>(GatewayCursorPageState.Invalid);
        }

        lock (_syncRoot)
        {
            PruneUnsafe(utcNow);
            if (!_snapshots.TryGetValue(snapshotId, out Snapshot? snapshot)) return new GatewayCursorPageResult<T>(GatewayCursorPageState.Expired);
            if (snapshot.DeviceScope != deviceScope || snapshot.Route != route || snapshot.Filters != filters || snapshot.ItemType != typeof(T) ||
                offset >= snapshot.Items.Length || !CryptographicOperations.FixedTimeEquals(
                    Convert.FromHexString(parts[3]), Signature(snapshot, offset)))
            {
                return new GatewayCursorPageResult<T>(GatewayCursorPageState.Invalid);
            }
            return Slice<T>(snapshot, offset, limit);
        }
    }

    private static GatewayCursorPageResult<T> Slice<T>(Snapshot snapshot, int offset, int limit)
    {
        T[] items = (T[])snapshot.Items;
        List<T> page = new List<T>();
        int pageBytes = 128;
        while (offset + page.Count < items.Length && page.Count < limit)
        {
            T item = items[offset + page.Count];
            int itemBytes = JsonSerializer.SerializeToUtf8Bytes(item, WebJson).Length + 2;
            if (pageBytes + itemBytes > MaximumPageBytes)
            {
                if (page.Count == 0) return new GatewayCursorPageResult<T>(GatewayCursorPageState.TooLarge);
                break;
            }
            page.Add(item);
            pageBytes += itemBytes;
        }

        int nextOffset = offset + page.Count;
        string? nextCursor = nextOffset < items.Length ? $"v1.{snapshot.Id:N}.{nextOffset.ToString(CultureInfo.InvariantCulture)}.{Convert.ToHexString(Signature(snapshot, nextOffset))}" : null;
        T[] pageItems = JsonSerializer.Deserialize<T[]>(JsonSerializer.SerializeToUtf8Bytes(page, WebJson), WebJson)!;
        return new GatewayCursorPageResult<T>(GatewayCursorPageState.Success, pageItems, nextCursor);
    }

    private static byte[] Signature(Snapshot snapshot, int offset)
    {
        byte[] digest = HMACSHA256.HashData(snapshot.Secret, Encoding.UTF8.GetBytes($"{snapshot.Id:N}:{offset.ToString(CultureInfo.InvariantCulture)}"));
        return digest.AsSpan(0, 16).ToArray();
    }

    private void PruneUnsafe(DateTime utcNow)
    {
        foreach (Guid id in _snapshots.Values.Where(snapshot => snapshot.ExpiresUtc <= utcNow.ToUniversalTime()).Select(snapshot => snapshot.Id).ToArray())
        {
            _snapshots.Remove(id);
        }
    }

    private sealed class Snapshot
    {
        public Snapshot(Guid id, string deviceScope, string route, string filters, Array items, Type itemType, DateTime expiresUtc, int sizeBytes)
        {
            Id = id;
            DeviceScope = deviceScope;
            Route = route;
            Filters = filters;
            Items = items;
            ItemType = itemType;
            ExpiresUtc = expiresUtc;
            CreatedUtc = expiresUtc.Subtract(SnapshotLifetime);
            SizeBytes = sizeBytes;
        }

        public Guid Id { get; }
        public string DeviceScope { get; }
        public string Route { get; }
        public string Filters { get; }
        public Array Items { get; }
        public Type ItemType { get; }
        public DateTime CreatedUtc { get; }
        public DateTime ExpiresUtc { get; }
        public int SizeBytes { get; }
        public byte[] Secret { get; } = RandomNumberGenerator.GetBytes(32);
    }
}

public enum GatewayCursorPageState { Success, Invalid, Expired, TooLarge }

public sealed class GatewayCursorPageResult<T>
{
    public GatewayCursorPageResult(GatewayCursorPageState state, T[]? items = null, string? nextCursor = null)
    {
        State = state;
        Items = items ?? Array.Empty<T>();
        NextCursor = nextCursor;
    }

    public GatewayCursorPageState State { get; }
    public T[] Items { get; }
    public string? NextCursor { get; }
}
