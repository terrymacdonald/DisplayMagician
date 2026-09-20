using System;
using System.IO;
using System.Text;
using System.Text.Json;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Appends machine-level security and recovery events to a durable JSON-lines journal.</summary>
public sealed class AuditStore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _auditPath;

    public AuditStore(StoragePaths storagePaths)
    {
        ArgumentNullException.ThrowIfNull(storagePaths);
        _auditPath = Path.Combine(storagePaths.MachineDiagnosticsPath, "Audit.jsonl");
    }

    public void Append(string eventType, string outcome, string details, string? userSid = null, int? sessionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(outcome);
        AuditRecord record = new AuditRecord
        {
            OccurredUtc = DateTime.UtcNow,
            EventType = eventType,
            Outcome = outcome,
            Details = details ?? string.Empty,
            UserSid = userSid,
            SessionId = sessionId
        };

        try
        {
            lock (_syncRoot)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_auditPath)!);
                using FileStream stream = new FileStream(_auditPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough);
                using StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                writer.WriteLine(JsonSerializer.Serialize(record));
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.Error(ex, "AuditStore/Append: Could not append audit event {0} to {1}.", eventType, _auditPath);
        }
    }
}

public sealed class AuditRecord
{
    public DateTime OccurredUtc { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Outcome { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;

    public string? UserSid { get; set; }

    public int? SessionId { get; set; }
}