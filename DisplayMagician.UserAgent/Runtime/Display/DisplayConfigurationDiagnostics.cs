using System;
using System.ComponentModel;
using System.Linq;
using DisplayMagician.Contracts;

namespace DisplayMagician.UserAgent.Runtime;

/// <summary>Per-attempt diagnostics passed explicitly through the existing native execution sequence.</summary>
internal sealed class DisplayConfigurationDiagnostics
{
    internal DisplayConfigurationResult Result { get; } = new DisplayConfigurationResult();

    internal static DisplayConfigurationResult Failure(string profileId, string stage, string message, Exception? exception = null)
    {
        DisplayConfigurationDiagnostics diagnostics = new DisplayConfigurationDiagnostics();
        diagnostics.Result.ProfileId = profileId;
        diagnostics.Add(stage, "Display", message, DisplayConfigurationSeverity.Fatal, exception: exception);
        return diagnostics.Result;
    }

    internal void Add(string stage, string component, string message,
        DisplayConfigurationSeverity severity = DisplayConfigurationSeverity.Warning,
        string displayId = "", Exception? exception = null, string? nativeErrorCode = null)
    {
        DisplayConfigurationIssue issue = new DisplayConfigurationIssue
        {
            Stage = stage, Component = component, Severity = severity, DisplayId = displayId,
            Message = message, ExceptionHResult = exception?.HResult,
            NativeErrorCode = nativeErrorCode ?? (exception is Win32Exception windowsError ? windowsError.NativeErrorCode.ToString() :
                exception is IGCLWrapper.IGCLException intelError ? intelError.Result.ToString() :
                exception is ADLXWrapper.ADLXException amdError ? amdError.Result.ToString() :
                exception is NVAPIWrapper.NVAPIException nvidiaError ? nvidiaError.Status.ToString() : null)
        };
        Result.Issues = Result.Issues.Append(issue).ToArray();
        SharedLogger.logger.Log(severity == DisplayConfigurationSeverity.Fatal ? NLog.LogLevel.Error : NLog.LogLevel.Warn,
            exception, "DisplayConfigurationDiagnostics/Add: Profile={0}; Attempt={1}; Stage={2}; Component={3}; Severity={4}; Display={5}; NativeError={6}; {7}",
            Result.ProfileId, Result.Attempt, stage, component, severity, displayId, issue.NativeErrorCode, message);
    }

    internal bool Run(string stage, string component, Func<bool> action, DisplayConfigurationSeverity severity, string displayId = "")
    {
        int previousCount = Result.Issues.Length;
        try
        {
            if (action())
                return !Result.Issues.Skip(previousCount).Any(issue => issue.Severity == DisplayConfigurationSeverity.Fatal);
            // A native stage may already have recorded a more specific failure and code.
            if (!Result.Issues.Skip(previousCount).Any(issue => issue.Severity == severity))
                Add(stage, component, "The requested display settings could not be applied.", severity, displayId: displayId);
        }
        catch (Exception ex)
        {
            Add(stage, component, ex.Message, severity, displayId: displayId, exception: ex);
        }
        return false;
    }
}
