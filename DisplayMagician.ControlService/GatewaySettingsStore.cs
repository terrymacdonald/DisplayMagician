using System;
using System.IO;
using System.Text.Json;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Persists the administrator-approved Gateway bind and QR advertisement settings.</summary>
public sealed class GatewaySettingsStore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly object _syncRoot = new object();
    private readonly string _storagePath;
    private GatewaySettings _settings;

    public GatewaySettingsStore(StoragePaths storagePaths)
    {
        ArgumentNullException.ThrowIfNull(storagePaths);
        _storagePath = Path.Combine(storagePaths.MachinePath, "GatewaySettings.json");
        _settings = Load();
    }

    public GatewaySettings Get()
    {
        lock (_syncRoot)
        {
            return Copy(_settings);
        }
    }

    public GatewaySettings Update(GatewaySettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        GatewaySettings normalized = Normalize(settings);
        lock (_syncRoot)
        {
            AtomicFileStore.WriteAllText(_storagePath, JsonSerializer.Serialize(normalized), $"{_storagePath}.bak");
            _settings = normalized;
            return Copy(_settings);
        }
    }

    private GatewaySettings Load()
    {
        try
        {
            return File.Exists(_storagePath) ? Normalize(JsonSerializer.Deserialize<GatewaySettings>(File.ReadAllText(_storagePath)) ?? new GatewaySettings()) : new GatewaySettings();
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            Logger.Error(ex, "GatewaySettingsStore/Load: Could not load Gateway settings from {0}.", _storagePath);
            return new GatewaySettings();
        }
    }

    private static GatewaySettings Normalize(GatewaySettings settings)
    {
        if (settings.LanPort is < 1 or > 65535 || settings.RemotePort is < 1 or > 65535)
        {
            throw new ArgumentException("Gateway ports must be between 1 and 65535.", nameof(settings));
        }

        return new GatewaySettings { LanBindAddress = string.IsNullOrWhiteSpace(settings.LanBindAddress) ? "*" : settings.LanBindAddress.Trim(), LanAdvertisedHost = settings.LanAdvertisedHost?.Trim() ?? string.Empty, LanPort = settings.LanPort, RemoteHost = settings.RemoteHost?.Trim() ?? string.Empty, RemotePort = settings.RemotePort };
    }

    private static GatewaySettings Copy(GatewaySettings settings) => new GatewaySettings { LanBindAddress = settings.LanBindAddress, LanAdvertisedHost = settings.LanAdvertisedHost, LanPort = settings.LanPort, RemoteHost = settings.RemoteHost, RemotePort = settings.RemotePort };
}
