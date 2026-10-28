using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace DisplayMagician.UserAgent;

internal sealed class UserLogLevelSettings
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly string _settingsPath;
    private readonly Action<string> _applyLevel;
    private readonly string? _startupOverride;
    private string? _lastSavedLevel;

    internal UserLogLevelSettings(string settingsPath, Action<string> applyLevel, string? startupOverride = null)
    {
        _settingsPath = settingsPath;
        _applyLevel = applyLevel;
        _startupOverride = string.Equals(startupOverride, "Trace", StringComparison.OrdinalIgnoreCase) ? "Trace"
            : string.Equals(startupOverride, "Debug", StringComparison.OrdinalIgnoreCase) ? "Debug" : null;
    }

    internal void Refresh()
    {
        try
        {
            // Read the existing saved setting without loading WinForms types or processing $type metadata.
            // BOM detection supports the Settings writer's UTF-16 files and migrated UTF-8 files.
            using FileStream input = new FileStream(_settingsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using StreamReader reader = new StreamReader(input);
            JObject document = JObject.Parse(reader.ReadToEnd());
            JObject settings = document["Settings"] as JObject ?? throw new JsonException("The settings object is missing.");
            JToken? token = settings["LogLevel"];
            if (token != null && token.Type != JTokenType.String) throw new JsonException("The saved log level must be a string.");
            string value = token?.Value<string>() ?? "Warn";
            LogLevel level = LogLevel.FromString(value);
            if (level.Ordinal < LogLevel.Trace.Ordinal || level.Ordinal > LogLevel.Fatal.Ordinal)
                throw new ArgumentException("The saved log level is unsupported.");
            if (string.Equals(_lastSavedLevel, level.Name, StringComparison.Ordinal)) return;

            // Match WinForms: an explicit diagnostic startup level takes precedence initially;
            // a later saved log-level change applies to the running process.
            _applyLevel(_lastSavedLevel == null ? _startupOverride ?? level.Name : level.Name);
            _lastSavedLevel = level.Name;
        }
        catch (FileNotFoundException)
        {
            // The settings file may not exist before migration/first desktop settings save.
        }
        catch (DirectoryNotFoundException)
        {
            // The user settings directory may not exist before migration completes.
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is ArgumentException)
        {
            Logger.Warn(ex, "UserLogLevelSettings/Refresh: Could not apply the saved user logging level from {0}; retaining the current level.", _settingsPath);
        }
    }
}
