using System;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace DisplayMagician.Contracts;

public sealed class SharedLoggingSettings
{
    public int SchemaVersion { get; set; } = 1;
    public string SavedLevel { get; set; } = "Warn";
    public bool HasSavedLevel { get; set; }
    public DiagnosticLoggingSession[] Sessions { get; set; } = Array.Empty<DiagnosticLoggingSession>();

    public bool IsValid() => SchemaVersion == 1 && IsLevel(SavedLevel) && Sessions != null
        && Sessions.All(session => session != null && session.OwnerId != Guid.Empty && !string.IsNullOrWhiteSpace(session.UserSid)
            && session.SessionId >= 0 && session.ExpiresUtc != default && (session.Level == "Trace" || session.Level == "Debug"))
        && Sessions.Select(session => session.OwnerId).Distinct().Count() == Sessions.Length;

    public string GetEffectiveLevel(DateTime utcNow)
    {
        if (!IsValid()) throw new ArgumentException("The shared logging settings are invalid.");
        return Sessions.Where(session => session.ExpiresUtc > utcNow).Select(session => LogLevel.FromString(session.Level))
            .Append(LogLevel.FromString(SavedLevel)).OrderBy(level => level.Ordinal).First().Name;
    }

    public static bool IsLevel(string? level) => level is "Trace" or "Debug" or "Info" or "Warn" or "Error" or "Fatal";

    public static void ApplyToTarget(LogFactory factory, string targetName, string level)
    {
        if (!IsLevel(level)) throw new ArgumentException("The logging level is invalid.");
        LoggingConfiguration? configuration = factory.Configuration;
        Target? target = configuration?.FindTargetByName(targetName);
        if (configuration == null || target == null) return;
        bool changed = false;
        LogLevel minimum = LogLevel.FromString(level);
        foreach (LoggingRule rule in configuration.LoggingRules)
        {
            if (!rule.Targets.Contains(target)) continue;
            if (rule.Levels.SequenceEqual(LogLevel.AllLoggingLevels.Where(candidate => candidate.Ordinal >= minimum.Ordinal))) continue;
            rule.SetLoggingLevels(minimum, LogLevel.Fatal);
            changed = true;
        }
        if (changed) factory.ReconfigExistingLoggers();
    }
}

public sealed class DiagnosticLoggingSession
{
    public Guid OwnerId { get; set; }
    public string UserSid { get; set; } = string.Empty;
    public int SessionId { get; set; }
    public string Level { get; set; } = "Debug";
    public DateTime ExpiresUtc { get; set; }
}

public sealed class SetSharedLogLevelRequest
{
    public string Level { get; set; } = "Warn";
    public bool InitializeOnly { get; set; }
}
