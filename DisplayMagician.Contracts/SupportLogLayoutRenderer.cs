using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace DisplayMagician.Contracts;

/// <summary>Renders a single, grep-friendly support-log event with safely escaped C# exception detail.</summary>
public static class SupportLogLayout
{
    private static readonly object RegistrationLock = new object();
    private static bool _isRegistered;

    public static void Register()
    {
        lock (RegistrationLock)
        {
            if (_isRegistered)
            {
                return;
            }

            MethodInfo? registerDefinition = ConfigurationItemFactory.Default.LayoutRendererFactory.GetType().GetMethod("RegisterDefinition", new[] { typeof(string), typeof(Type) });
            if (registerDefinition == null)
            {
                throw new InvalidOperationException("The installed NLog version does not support custom layout renderer registration.");
            }

            registerDefinition.Invoke(ConfigurationItemFactory.Default.LayoutRendererFactory, new object[] { "displaymagicianlog", typeof(SupportLogLayoutRenderer) });
            _isRegistered = true;
        }
    }
}

[LayoutRenderer("displaymagicianlog")]
public sealed class SupportLogLayoutRenderer : LayoutRenderer
{
    private static readonly string[] LegacySeverityPrefixes = { "ERROR - ", "WARNING - ", "WARN - ", "INFO - ", "DEBUG - ", "TRACE - ", "FATAL - " };
    private static readonly Regex SensitiveKeyValuePattern = new Regex("\\b(password|token|secret|api[_-]?key)\\s*[:=]\\s*(\"[^\"]*\"|\\S+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex BearerTokenPattern = new Regex(@"\bBearer\s+\S+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public string Component { get; set; } = "Unknown";

    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        string message = logEvent.FormattedMessage ?? string.Empty;
        string source = logEvent.LoggerName ?? "Unknown";
        int sourceSeparator = message.IndexOf(':');
        if (sourceSeparator > 0)
        {
            string candidate = message.Substring(0, sourceSeparator);
            if (candidate.IndexOf('/') > 0 && candidate.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) < 0)
            {
                source = candidate;
                message = message.Substring(sourceSeparator + 1).TrimStart();
            }
        }

        message = RedactSensitiveValues(RemoveLegacySeverityPrefix(message));

        AppendField(builder, "ts", logEvent.TimeStamp.ToUniversalTime().ToString("O"));
        AppendField(builder, "component", Component);
        AppendField(builder, "level", logEvent.Level.Name.ToUpperInvariant());
        AppendField(builder, "source", source);
        AppendField(builder, "operation_id", GetProperty(logEvent, "OperationId"));
        AppendField(builder, "request_id", GetProperty(logEvent, "RequestId"));
        AppendField(builder, "msg", message);

        if (logEvent.Exception != null)
        {
            AppendField(builder, "ex_type", logEvent.Exception.GetType().FullName ?? logEvent.Exception.GetType().Name);
            AppendField(builder, "ex_message", RedactSensitiveValues(logEvent.Exception.Message));
            AppendField(builder, "ex_stack", RedactSensitiveValues(logEvent.Exception.ToString()));
        }
    }

    private static string GetProperty(LogEventInfo logEvent, string propertyName)
    {
        if (logEvent.Properties.TryGetValue(propertyName, out object? eventValue) && eventValue != null)
        {
            return Convert.ToString(eventValue) ?? "-";
        }

        return ScopeContext.TryGetProperty(propertyName, out object? scopeValue) && scopeValue != null
            ? Convert.ToString(scopeValue) ?? "-"
            : "-";
    }

    private static void AppendField(StringBuilder builder, string name, string value)
    {
        if (builder.Length > 0)
        {
            builder.Append(' ');
        }

        builder.Append(name).Append('=');
        if (IsBareValue(value))
        {
            builder.Append(value);
            return;
        }

        builder.Append('"');
        foreach (char character in value)
        {
            switch (character)
            {
                case '\\': builder.Append("\\\\"); break;
                case '"': builder.Append("\\\""); break;
                case '\r': builder.Append("\\r"); break;
                case '\n': builder.Append("\\n"); break;
                case '\t': builder.Append("\\t"); break;
                default: builder.Append(character); break;
            }
        }

        builder.Append('"');
    }

    private static string RemoveLegacySeverityPrefix(string message)
    {
        foreach (string prefix in LegacySeverityPrefixes)
        {
            if (message.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return message.Substring(prefix.Length);
            }
        }

        return message;
    }

    private static string RedactSensitiveValues(string value)
    {
        string redactedValue = BearerTokenPattern.Replace(value, "Bearer <redacted>");
        return SensitiveKeyValuePattern.Replace(redactedValue, match => $"{match.Groups[1].Value}=<redacted>");
    }

    private static bool IsBareValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        foreach (char character in value)
        {
            if (!(char.IsLetterOrDigit(character) || character == '.' || character == '_' || character == '-' || character == '/' || character == ':'))
            {
                return false;
            }
        }

        return true;
    }
}
