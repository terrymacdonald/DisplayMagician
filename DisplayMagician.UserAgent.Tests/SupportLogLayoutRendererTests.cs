using System;
using System.Reflection;
using System.Text;
using DisplayMagician.Contracts;
using NLog;
using NLog.Layouts;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class SupportLogLayoutRendererTests
{
    [Fact]
    public void Register_EnablesTheCommonLayoutRendererForNLogTargets()
    {
        SupportLogLayout.Register();
        SimpleLayout layout = new SimpleLayout("${displaymagicianlog:component=DesktopApp}");

        string line = layout.Render(new LogEventInfo(LogLevel.Info, "DisplayMagician.Program", "Program/Main: Started."));

        Assert.Contains("component=DesktopApp", line);
        Assert.Contains("source=Program/Main", line);
        Assert.Contains("msg=Started.", line);
    }

    [Fact]
    public void Append_UsesTheCurrentAsyncCorrelationScope()
    {
        SupportLogLayoutRenderer renderer = new SupportLogLayoutRenderer { Component = "UserAgent" };
        StringBuilder output = new StringBuilder();
        using IDisposable operationScope = SupportLogScope.BeginOperation(Guid.Parse("7fe7d61d-06c8-454e-af5b-846ea8da1cf1"));
        using IDisposable requestScope = SupportLogScope.BeginRequest(Guid.Parse("8c2f99d4-4d89-4df6-a178-e9fe073bc3d3"));
        LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, "DisplayMagician.UserAgent.Program", "Program/Main: Started.");
        MethodInfo append = typeof(SupportLogLayoutRenderer).GetMethod("Append", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The support log renderer Append method was not found.");

        append.Invoke(renderer, new object[] { output, logEvent });

        Assert.Contains("operation_id=7fe7d61d-06c8-454e-af5b-846ea8da1cf1", output.ToString());
        Assert.Contains("request_id=8c2f99d4-4d89-4df6-a178-e9fe073bc3d3", output.ToString());
    }

    [Fact]
    public void Append_RendersOneEscapedLogfmtLineForAnExceptionEvent()
    {
        SupportLogLayoutRenderer renderer = new SupportLogLayoutRenderer { Component = "ControlService" };
        Guid operationId = Guid.Parse("7fe7d61d-06c8-454e-af5b-846ea8da1cf1");
        Guid requestId = Guid.Parse("8c2f99d4-4d89-4df6-a178-e9fe073bc3d3");
        LogEventInfo logEvent = new LogEventInfo(LogLevel.Error, "DisplayMagician.ControlService.ControlClientPipeServer", "ControlClientPipeServer/CreateUserSupportBundleAsync: Could not stage \"machine\" logs.")
        {
            Exception = new InvalidOperationException("The log file is locked.\r\nRetry the request.")
        };
        logEvent.Properties["OperationId"] = operationId;
        logEvent.Properties["RequestId"] = requestId;
        StringBuilder output = new StringBuilder();

        MethodInfo append = typeof(SupportLogLayoutRenderer).GetMethod("Append", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The support log renderer Append method was not found.");
        append.Invoke(renderer, new object[] { output, logEvent });

        string line = output.ToString();
        Assert.DoesNotContain('\r', line);
        Assert.DoesNotContain('\n', line);
        Assert.Contains("component=ControlService", line);
        Assert.Contains("level=ERROR", line);
        Assert.Contains("source=ControlClientPipeServer/CreateUserSupportBundleAsync", line);
        Assert.Contains($"operation_id={operationId}", line);
        Assert.Contains($"request_id={requestId}", line);
        Assert.Contains("msg=\"Could not stage \\\"machine\\\" logs.\"", line);
        Assert.Contains("ex_type=System.InvalidOperationException", line);
        Assert.Contains("ex_stack=\"System.InvalidOperationException: The log file is locked.\\r\\nRetry the request.", line);
    }

    [Fact]
    public void Append_RemovesLegacySeverityDecorationFromTheMessage()
    {
        SupportLogLayoutRenderer renderer = new SupportLogLayoutRenderer { Component = "DesktopApp" };
        LogEventInfo logEvent = new LogEventInfo(LogLevel.Error, "DisplayMagician.Program", "Program/Main: ERROR - The profile could not be applied.");
        StringBuilder output = new StringBuilder();
        MethodInfo append = typeof(SupportLogLayoutRenderer).GetMethod("Append", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The support log renderer Append method was not found.");

        append.Invoke(renderer, new object[] { output, logEvent });

        Assert.Contains("level=ERROR", output.ToString());
        Assert.Contains("msg=\"The profile could not be applied.\"", output.ToString());
        Assert.DoesNotContain("ERROR -", output.ToString());
    }

    [Fact]
    public void Append_RedactsSensitiveKeyValueAndBearerTokenContent()
    {
        SupportLogLayoutRenderer renderer = new SupportLogLayoutRenderer { Component = "ControlService" };
        LogEventInfo logEvent = new LogEventInfo(LogLevel.Error, "DisplayMagician.ControlService.Client", "Client/Sync: token=abc123 Authorization: Bearer private-value")
        {
            Exception = new InvalidOperationException("password=super-secret")
        };
        StringBuilder output = new StringBuilder();
        MethodInfo append = typeof(SupportLogLayoutRenderer).GetMethod("Append", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The support log renderer Append method was not found.");

        append.Invoke(renderer, new object[] { output, logEvent });

        Assert.DoesNotContain("abc123", output.ToString());
        Assert.DoesNotContain("private-value", output.ToString());
        Assert.DoesNotContain("super-secret", output.ToString());
        Assert.Contains("token=<redacted>", output.ToString());
        Assert.Contains("Bearer <redacted>", output.ToString());
        Assert.Contains("password=<redacted>", output.ToString());
    }
}
