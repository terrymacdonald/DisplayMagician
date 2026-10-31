using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Owns atomic updates of all persistent machine preferences, independently of operational state.</summary>
public sealed class ControlServiceSettingsStore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _path;
    private ControlServiceSettingsDocument _state;
    private bool _writesEnabled = true;

    public ControlServiceSettingsStore(StoragePaths paths)
    {
        _path = Path.Combine(paths.MachinePath, "ControlServiceSettings.json");
        try { _state = LoadOrMigrate(paths); }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is ArgumentException)
        {
            _state = new ControlServiceSettingsDocument();
            _writesEnabled = false;
            Logger.Error(ex, "ControlServiceSettingsStore/ControlServiceSettingsStore: Preferences at {0} are unavailable; preserving files and rejecting preference changes until repair/restart.", _path);
        }
    }

    public ControlServiceSettingsDocument Get()
    {
        lock (_syncRoot) return JsonSerializer.Deserialize<ControlServiceSettingsDocument>(JsonSerializer.Serialize(_state))!;
    }

    public ControlServiceSettingsDocument Update(Action<ControlServiceSettingsDocument> update)
    {
        ArgumentNullException.ThrowIfNull(update);
        lock (_syncRoot)
        {
            if (!_writesEnabled) throw new IOException("Service preferences require repair and service restart before changes can be saved.");
            try { ReadPreferences(); }
            catch (Exception ex) when (ex is JsonException || ex is ArgumentException || ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                _writesEnabled = false;
                Logger.Error(ex, "ControlServiceSettingsStore/Update: Preferences at {0} became invalid; retaining cached preferences and preserving files until repair/restart.", _path);
                throw new IOException("Service preferences require repair and service restart before changes can be saved.", ex);
            }
            ControlServiceSettingsDocument candidate = Get();
            update(candidate);
            if (!candidate.IsValid()) throw new ArgumentException("The service preferences are invalid.");
            candidate.Gateway = GatewaySettingsStore.Normalize(candidate.Gateway);
            string json = JsonSerializer.Serialize(candidate);
            if (json != JsonSerializer.Serialize(_state)) AtomicFileStore.WriteAllText(_path, json, _path + ".bak");
            _state = JsonSerializer.Deserialize<ControlServiceSettingsDocument>(json)!;
            return Get();
        }
    }

    private ControlServiceSettingsDocument LoadOrMigrate(StoragePaths paths)
    {
        try
        {
            return ReadPreferences();
        }
        catch (FileNotFoundException) when (!File.Exists(_path + ".bak") && !Directory.Exists(_path + ".bak")) { }
        catch (DirectoryNotFoundException) when (!File.Exists(_path + ".bak") && !Directory.Exists(_path + ".bak")) { }

        ControlServiceSettingsDocument migrated = new ControlServiceSettingsDocument();
        Dictionary<string, byte[]> sources = new Dictionary<string, byte[]>();
        using JsonDocument? gateway = ReadLegacy(Path.Combine(paths.MachinePath, "GatewaySettings.json"), sources);
        if (gateway != null)
            migrated.Gateway = GatewaySettingsStore.Normalize(gateway.RootElement.Deserialize<GatewaySettings>() ?? throw new JsonException("Invalid legacy Gateway settings."));

        using JsonDocument? logging = ReadLegacy(Path.Combine(paths.MachineDiagnosticsPath, "LoggingSettings.json"), sources);
        if (logging != null)
        {
            SharedLoggingSettings old = logging.RootElement.Deserialize<SharedLoggingSettings>() ?? throw new JsonException("Invalid legacy logging settings.");
            if (!logging.RootElement.TryGetProperty("SchemaVersion", out _) || !logging.RootElement.TryGetProperty("SavedLevel", out _)
                || !logging.RootElement.TryGetProperty("HasSavedLevel", out _) || !logging.RootElement.TryGetProperty("Sessions", out _) || !old.IsValid())
                throw new JsonException("Invalid or unsupported legacy logging settings.");
            migrated.Logging.SavedLevel = old.SavedLevel;
            migrated.Logging.HasSavedLevel = old.HasSavedLevel;
        }

        using JsonDocument? schedule = ReadLegacy(Path.Combine(paths.MachinePath, "ScheduleState.json"), sources);
        if (schedule != null && schedule.RootElement.TryGetProperty("ShareAnonymousUsageMetrics", out JsonElement choice))
        {
            if (choice.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw new JsonException("Invalid legacy metrics preference.");
            migrated.ShareAnonymousUsageMetrics = choice.GetBoolean();
            migrated.HasAnonymousMetricsChoice = true;
        }

        // Validate every source before writing the new document. Keep original files and exact backup bytes.
        if (sources.Count > 0)
        {
            string backupRoot = Path.Combine(paths.MachineBackupsPath, "preferences_consolidation_" + Guid.NewGuid().ToString("N"));
            foreach (KeyValuePair<string, byte[]> source in sources)
                AtomicFileStore.WriteBytes(Path.Combine(backupRoot, Path.GetFileName(source.Key)), source.Value);
        }
        AtomicFileStore.WriteAllText(_path, JsonSerializer.Serialize(migrated));
        return migrated;
    }

    private ControlServiceSettingsDocument ReadPreferences()
    {
        ControlServiceSettingsDocument document = JsonSerializer.Deserialize<ControlServiceSettingsDocument>(File.ReadAllText(_path))
            ?? throw new JsonException("The service preferences are missing.");
        if (!document.IsValid()) throw new JsonException("The service preferences are invalid or unsupported.");
        document.Gateway = GatewaySettingsStore.Normalize(document.Gateway);
        return document;
    }

    private static JsonDocument? ReadLegacy(string path, Dictionary<string, byte[]> sources)
    {
        byte[] content;
        try { content = File.ReadAllBytes(path); }
        catch (FileNotFoundException) when (!File.Exists(path + ".bak") && !Directory.Exists(path + ".bak")) { return null; }
        catch (DirectoryNotFoundException) when (!File.Exists(path + ".bak") && !Directory.Exists(path + ".bak")) { return null; }
        // StreamReader supports BOMs without assuming that all legacy sources use one encoding.
        using MemoryStream stream = new MemoryStream(content);
        using StreamReader reader = new StreamReader(stream);
        JsonDocument document = JsonDocument.Parse(reader.ReadToEnd());
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            document.Dispose();
            throw new JsonException("A legacy preferences document is not an object.");
        }
        sources.Add(path, content);
        return document;
    }
}
