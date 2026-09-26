using System;
using System.IO;
using System.Text.Json;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

public sealed class MachineDiagnosticLogLevelStore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _path;
    private TemporaryDiagnosticLogLevelState _state;

    public MachineDiagnosticLogLevelStore(StoragePaths storagePaths)
    {
        _path = Path.Combine(storagePaths.MachineDiagnosticsPath, "TemporaryDiagnosticLogLevel.json");
        _state = Load();
    }

    public string GetActiveLevel(DateTime utcNow)
    {
        lock (_syncRoot)
        {
            if (_state.ExpiresUtc <= utcNow || !TryGetLevel(_state.Level, out _))
            {
                if (!string.IsNullOrEmpty(_state.Level))
                {
                    _state = new TemporaryDiagnosticLogLevelState();
                    Persist();
                }

                return LogLevel.Info.Name;
            }

            return _state.Level;
        }
    }

    public string Set(TemporaryDiagnosticLogLevelRequest request, DateTime utcNow)
    {
        if (request.OwnerId == Guid.Empty || request.DurationMinutes is < 1 or > 30 || !TryGetLevel(request.Level, out LogLevel? level) || level == null || level.Ordinal < LogLevel.Debug.Ordinal)
        {
            throw new ArgumentException("The temporary diagnostic log level request is invalid.");
        }

        lock (_syncRoot)
        {
            _state = new TemporaryDiagnosticLogLevelState { Level = level.Name, OwnerId = request.OwnerId, ExpiresUtc = utcNow.AddMinutes(request.DurationMinutes) };
            Persist();
            return _state.Level;
        }
    }

    public string Release(Guid ownerId, DateTime utcNow)
    {
        lock (_syncRoot)
        {
            if (ownerId != Guid.Empty && _state.OwnerId == ownerId)
            {
                _state = new TemporaryDiagnosticLogLevelState();
                Persist();
            }

            return GetActiveLevel(utcNow);
        }
    }

    private TemporaryDiagnosticLogLevelState Load()
    {
        try { return File.Exists(_path) ? JsonSerializer.Deserialize<TemporaryDiagnosticLogLevelState>(File.ReadAllText(_path)) ?? new TemporaryDiagnosticLogLevelState() : new TemporaryDiagnosticLogLevelState(); }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) { Logger.Warn(ex, "MachineDiagnosticLogLevelStore/Load: Could not load temporary diagnostic log level state from {0}.", _path); return new TemporaryDiagnosticLogLevelState(); }
    }

    private void Persist()
    {
        try { AtomicFileStore.WriteAllText(_path, JsonSerializer.Serialize(_state), $"{_path}.bak"); }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { Logger.Error(ex, "MachineDiagnosticLogLevelStore/Persist: Could not persist temporary diagnostic log level state to {0}.", _path); throw; }
    }

    private static bool TryGetLevel(string? value, out LogLevel? level)
    {
        try { level = string.IsNullOrWhiteSpace(value) ? null : LogLevel.FromString(value); return level != null; }
        catch (ArgumentException) { level = null; return false; }
    }
}

public sealed class TemporaryDiagnosticLogLevelState
{
    public string Level { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public DateTime ExpiresUtc { get; set; }
}
