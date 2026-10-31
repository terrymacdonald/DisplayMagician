using System;
using System.IO;
using System.Text.Json;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class GatewaySettingsProviderTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-GatewayPrefs-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void ReadsConsolidatedPreferencesInsteadOfStaleLegacyFile()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "GatewaySettings.json"), "{\"LanPort\":24000}");
        ControlServiceSettingsDocument preferences = new() { Gateway = new GatewaySettings { LanPort = 24567, AllowPublicNetworks = true } };
        File.WriteAllText(Path.Combine(_root, "ControlServiceSettings.json"), JsonSerializer.Serialize(preferences));
        GatewaySettings settings = GatewaySettingsProvider.Load(_root);
        Assert.Equal(24567, settings.LanPort);
        Assert.True(settings.AllowPublicNetworks);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("{")]
    [InlineData("{}")]
    [InlineData("{\"SchemaVersion\":2,\"Gateway\":{},\"Logging\":{\"SavedLevel\":\"Warn\",\"HasSavedLevel\":true},\"ShareAnonymousUsageMetrics\":false,\"HasAnonymousMetricsChoice\":true}")]
    [InlineData("{\"SchemaVersion\":1,\"Gateway\":{\"LanPort\":0},\"Logging\":{\"SavedLevel\":\"Warn\",\"HasSavedLevel\":true},\"ShareAnonymousUsageMetrics\":false,\"HasAnonymousMetricsChoice\":true}")]
    public void InvalidOrMissingPrimaryUsesDefaultsAndNeverReimportsLegacy(string? json)
    {
        Directory.CreateDirectory(_root);
        string path = Path.Combine(_root, "ControlServiceSettings.json");
        File.WriteAllText(Path.Combine(_root, "GatewaySettings.json"), "{\"LanPort\":24000,\"AllowPublicNetworks\":true}");
        File.WriteAllText(path + ".bak", "preserve backup");
        if (json != null) File.WriteAllText(path, json);
        GatewaySettings settings = GatewaySettingsProvider.Load(_root);
        Assert.NotEqual(24000, settings.LanPort);
        Assert.False(settings.AllowPublicNetworks);
        Assert.Equal("preserve backup", File.ReadAllText(path + ".bak"));
        if (json != null) Assert.Equal(json, File.ReadAllText(path));
        else Assert.False(File.Exists(path));
    }

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
