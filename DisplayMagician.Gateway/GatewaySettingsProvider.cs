using System;
using System.IO;
using System.Text.Json;
using DisplayMagician.Contracts;

namespace DisplayMagician.Gateway;

public static class GatewaySettingsProvider
{
    public static GatewaySettings Load()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician", "Machine", "GatewaySettings.json");
        try
        {
            GatewaySettings? settings = File.Exists(path) ? JsonSerializer.Deserialize<GatewaySettings>(File.ReadAllText(path)) : null;
            return settings ?? new GatewaySettings();
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            return new GatewaySettings();
        }
    }
}
