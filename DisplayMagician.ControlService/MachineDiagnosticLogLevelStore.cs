using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

public sealed class MachineDiagnosticLogLevelStore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _path;
    private SharedLoggingSettings _state = new SharedLoggingSettings();
    private bool _writesEnabled = true;

    public MachineDiagnosticLogLevelStore(StoragePaths storagePaths)
    {
        _path = Path.Combine(storagePaths.MachineDiagnosticsPath, "LoggingSettings.json");
        try
        {
            string json = File.ReadAllText(_path);
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty("SchemaVersion", out _)
                || !document.RootElement.TryGetProperty("SavedLevel", out _)
                || !document.RootElement.TryGetProperty("HasSavedLevel", out _)
                || !document.RootElement.TryGetProperty("Sessions", out _)) throw new JsonException("Incomplete logging settings.");
            _state = JsonSerializer.Deserialize<SharedLoggingSettings>(json) ?? throw new JsonException("Missing logging settings.");
            if (!_state.IsValid()) throw new JsonException("Invalid or unsupported logging settings.");
        }
        catch (FileNotFoundException) when (!File.Exists(_path + ".bak") && !Directory.Exists(_path + ".bak")) { }
        catch (DirectoryNotFoundException) when (!File.Exists(_path + ".bak") && !Directory.Exists(_path + ".bak")) { }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            _state = new SharedLoggingSettings();
            _writesEnabled = false;
            Logger.Error(ex, "MachineDiagnosticLogLevelStore/MachineDiagnosticLogLevelStore: Logging settings at {0} are unavailable; preserving files and rejecting changes until repair/restart.", _path);
        }
    }

    public SharedLoggingSettings GetSettings()
    {
        lock (_syncRoot) return JsonSerializer.Deserialize<SharedLoggingSettings>(JsonSerializer.Serialize(_state))!;
    }

    public string GetActiveLevel(DateTime utcNow)
    {
        lock (_syncRoot) return _state.GetEffectiveLevel(utcNow);
    }

    public SharedLoggingSettings SetSavedLevel(SetSharedLogLevelRequest request)
    {
        if (request == null || !SharedLoggingSettings.IsLevel(request.Level)) throw new ArgumentException("The shared logging level is invalid.");
        lock (_syncRoot)
        {
            if (request.InitializeOnly && _state.HasSavedLevel) return GetSettings();
            SharedLoggingSettings candidate = GetSettings();
            candidate.SavedLevel = request.Level;
            candidate.HasSavedLevel = true;
            Persist(candidate);
            return GetSettings();
        }
    }

    public string Set(TemporaryDiagnosticLogLevelRequest request, DateTime utcNow)
        => Set(request, utcNow, "legacy-local", 0);

    public string Set(TemporaryDiagnosticLogLevelRequest request, DateTime utcNow, string userSid, int sessionId)
    {
        if (request == null || request.OwnerId == Guid.Empty || request.Level is not ("Trace" or "Debug") || string.IsNullOrWhiteSpace(userSid) || sessionId < 0)
            throw new ArgumentException("The diagnostic logging session is invalid.");
        lock (_syncRoot)
        {
            DiagnosticLoggingSession? existing = _state.Sessions.FirstOrDefault(session => session.OwnerId == request.OwnerId);
            if (existing != null && (existing.UserSid != userSid || existing.SessionId != sessionId))
                throw new UnauthorizedAccessException("The diagnostic session belongs to another caller.");
            SharedLoggingSettings candidate = GetSettings();
            candidate.Sessions = candidate.Sessions.Where(session => session.ExpiresUtc > utcNow && session.OwnerId != request.OwnerId)
                .Append(new DiagnosticLoggingSession { OwnerId = request.OwnerId, UserSid = userSid, SessionId = sessionId, Level = request.Level, ExpiresUtc = utcNow.AddMinutes(1) }).ToArray();
            Persist(candidate);
            return _state.GetEffectiveLevel(utcNow);
        }
    }

    public string Release(Guid ownerId, DateTime utcNow) => Release(ownerId, utcNow, "legacy-local", 0);

    public string Release(Guid ownerId, DateTime utcNow, string userSid, int sessionId)
    {
        lock (_syncRoot)
        {
            DiagnosticLoggingSession? existing = _state.Sessions.FirstOrDefault(session => session.OwnerId == ownerId);
            if (existing != null && (existing.UserSid != userSid || existing.SessionId != sessionId))
                throw new UnauthorizedAccessException("The diagnostic session belongs to another caller.");
            if (existing != null)
            {
                SharedLoggingSettings candidate = GetSettings();
                candidate.Sessions = candidate.Sessions.Where(session => session.OwnerId != ownerId && session.ExpiresUtc > utcNow).ToArray();
                Persist(candidate);
            }
            return _state.GetEffectiveLevel(utcNow);
        }
    }

    private void Persist(SharedLoggingSettings candidate)
    {
        if (!_writesEnabled) throw new IOException("Logging settings require repair and service restart before changes can be saved.");
        AtomicFileStore.WriteAllText(_path, JsonSerializer.Serialize(candidate), _path + ".bak");
        _state = candidate;
    }
}

// Retained for reading/exporting the previous temporary-only document; it is not rewritten.
public sealed class TemporaryDiagnosticLogLevelState
{
    public string Level { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public DateTime ExpiresUtc { get; set; }
}
