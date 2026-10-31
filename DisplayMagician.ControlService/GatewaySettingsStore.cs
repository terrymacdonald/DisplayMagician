using System;
using System.Net;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

/// <summary>Persists the administrator-approved Gateway bind and QR advertisement settings.</summary>
public sealed class GatewaySettingsStore
{
    private readonly ControlServiceSettingsStore _preferences;

    public GatewaySettingsStore(StoragePaths storagePaths, ControlServiceSettingsStore? preferences = null)
    {
        _preferences = preferences ?? new ControlServiceSettingsStore(storagePaths);
    }

    public GatewaySettings Get() => _preferences.Get().Gateway;

    public GatewaySettings Update(GatewaySettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        GatewaySettings normalized = Normalize(settings);
        return _preferences.Update(document => document.Gateway = normalized).Gateway;
    }

    internal static GatewaySettings Normalize(GatewaySettings settings)
    {
        if (settings.LanPort is < 1 or > 65535 || settings.RemotePort is < 1 or > 65535)
        {
            throw new ArgumentException("Gateway ports must be between 1 and 65535.", nameof(settings));
        }

        string bindAddress = string.IsNullOrWhiteSpace(settings.LanBindAddress) ? "*" : settings.LanBindAddress.Trim();
        if (bindAddress != "*" && !IPAddress.TryParse(bindAddress, out _))
        {
            throw new ArgumentException("Gateway bind address must be an IP address or all interfaces.", nameof(settings));
        }

        string advertisedHost = settings.LanAdvertisedHost?.Trim() ?? string.Empty;
        string remoteHost = settings.RemoteHost?.Trim() ?? string.Empty;
        if ((!string.IsNullOrEmpty(advertisedHost) && !IsDnsOrIpAddress(advertisedHost)) ||
            (!string.IsNullOrEmpty(remoteHost) && !IsDnsOrIpAddress(remoteHost)))
        {
            throw new ArgumentException("Gateway advertised hosts must be DNS names or IP addresses.", nameof(settings));
        }

        return new GatewaySettings { LanBindAddress = bindAddress, LanAdvertisedHost = advertisedHost, LanPort = settings.LanPort, AllowPublicNetworks = settings.AllowPublicNetworks, RemoteHost = remoteHost, RemotePort = settings.RemotePort };
    }

    private static bool IsDnsOrIpAddress(string host) => IPAddress.TryParse(host, out _) || Uri.CheckHostName(host) == UriHostNameType.Dns;

}
