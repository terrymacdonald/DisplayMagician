using System;
using System.IO;
using System.Text.Json;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.Gateway;

public static class GatewaySettingsProvider
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public static GatewaySettings Load() => Load(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician", "Machine"));

    internal static GatewaySettings Load(string machinePath)
    {
        string path = Path.Combine(machinePath, "ControlServiceSettings.json");
        try
        {
            ControlServiceSettingsDocument? settings = JsonSerializer.Deserialize<ControlServiceSettingsDocument>(File.ReadAllText(path));
            if (settings == null || !settings.IsValid()) throw new JsonException("Invalid or unsupported service preferences.");
            return settings.Gateway;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            Logger.Warn(ex, "GatewaySettingsProvider/Load: Service preferences at {0} are unavailable; using default Gateway settings without rewriting the file.", path);
            return new GatewaySettings();
        }
    }
}
