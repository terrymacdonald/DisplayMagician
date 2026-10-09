using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

/// <summary>Machine-owned 24-hour replay ledger for accepted Gateway HTTP mutations.</summary>
public sealed class GatewayHttpIdempotencyStore
{
    private static readonly TimeSpan Retention = TimeSpan.FromHours(24);
    private readonly object _syncRoot = new object();
    private readonly string _storagePath;
    private readonly List<Record> _records;

    public GatewayHttpIdempotencyStore(StoragePaths storagePaths)
    {
        _storagePath = Path.Combine(storagePaths.MachinePath, "GatewayHttpIdempotency.json");
        _records = File.Exists(_storagePath)
            ? JsonSerializer.Deserialize<List<Record>>(File.ReadAllText(_storagePath)) ?? new List<Record>()
            : new List<Record>();
    }

    public GatewayHttpIdempotencyResult Begin(string scopeHash, string key, string requestHash, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(scopeHash) || string.IsNullOrWhiteSpace(requestHash) || !IsCanonicalUuidV4(key))
        {
            return new GatewayHttpIdempotencyResult { State = GatewayHttpIdempotencyState.Invalid };
        }

        lock (_syncRoot)
        {
            PruneUnsafe(utcNow);
            Record? existing = _records.FirstOrDefault(record => record.ScopeHash == scopeHash && record.Key == key);
            if (existing != null)
            {
                if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
                {
                    return new GatewayHttpIdempotencyResult { State = GatewayHttpIdempotencyState.Conflict };
                }

                return existing.Completed
                    ? new GatewayHttpIdempotencyResult { State = GatewayHttpIdempotencyState.Replay, StatusCode = existing.StatusCode, ContentType = existing.ContentType, Location = existing.Location, RetryAfter = existing.RetryAfter, ResponseJson = existing.ResponseJson }
                    : new GatewayHttpIdempotencyResult { State = GatewayHttpIdempotencyState.Pending };
            }

            _records.Add(new Record { ScopeHash = scopeHash, Key = key, RequestHash = requestHash, AcceptedUtc = utcNow.ToUniversalTime() });
            PersistUnsafe();
            return new GatewayHttpIdempotencyResult { State = GatewayHttpIdempotencyState.Accepted };
        }
    }

    public void Complete(string scopeHash, string key, string requestHash, int statusCode, string contentType, string location, string retryAfter, string responseJson)
    {
        lock (_syncRoot)
        {
            Record? record = _records.FirstOrDefault(candidate => candidate.ScopeHash == scopeHash && candidate.Key == key && candidate.RequestHash == requestHash);
            if (record == null || statusCode is < 200 or > 599 || Encoding.UTF8.GetByteCount(responseJson) > 1024 * 1024)
            {
                throw new InvalidOperationException("The Gateway idempotency result could not be stored.");
            }

            if (record.Completed)
            {
                if (record.StatusCode == statusCode && record.ContentType == contentType && record.Location == location && record.RetryAfter == retryAfter && record.ResponseJson == responseJson) return;
                throw new InvalidOperationException("A completed Gateway idempotency result cannot be replaced.");
            }

            record.StatusCode = statusCode;
            record.ContentType = contentType;
            record.Location = location;
            record.RetryAfter = retryAfter;
            record.ResponseJson = responseJson;
            record.Completed = true;
            PersistUnsafe();
        }
    }

    private void PruneUnsafe(DateTime utcNow)
    {
        if (_records.RemoveAll(record => record.AcceptedUtc.Add(Retention) <= utcNow.ToUniversalTime()) > 0) PersistUnsafe();
    }

    private void PersistUnsafe() => AtomicFileStore.WriteAllText(_storagePath, JsonSerializer.Serialize(_records), $"{_storagePath}.bak");

    private static bool IsCanonicalUuidV4(string key) => Guid.TryParseExact(key, "D", out Guid parsed) &&
        string.Equals(parsed.ToString("D"), key, StringComparison.Ordinal) && parsed.Version == 4;

    public sealed class Record
    {
        public string ScopeHash { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string RequestHash { get; set; } = string.Empty;
        public DateTime AcceptedUtc { get; set; }
        public bool Completed { get; set; }
        public int StatusCode { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string RetryAfter { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;
    }
}

