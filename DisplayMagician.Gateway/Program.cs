using System;
using System.IO;
using System.Net;
using DisplayMagician.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace DisplayMagician.Gateway;

internal static class Program
{
    private static void Main(string[] args)
    {
        ConfigureLogging();
        GatewayIdentityProvider identityProvider = new GatewayIdentityProvider();
        GatewayIdentity identity = identityProvider.GetOrCreate();
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddWindowsService(options => options.ServiceName = "DisplayMagicianGateway");
        builder.Services.AddSingleton(identity);
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;
            options.Limits.MaxRequestBodySize = ControlProtocol.MaximumMessageLength;
            options.ListenAnyIP(ControlProtocol.DefaultGatewayPort, listenOptions => listenOptions.UseHttps(identity.TlsCertificate));
        });

        WebApplication app = builder.Build();
        app.MapGet("/v1/identity", (GatewayIdentity gatewayIdentity) => Results.Ok(gatewayIdentity.ToView()));
        app.Run();
    }

    private static void ConfigureLogging()
    {
        try
        {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician", "Machine", "Logs", "Gateway", "Gateway-${shortdate}.log");
            SupportLogLayout.Register();
            LoggingConfiguration configuration = new LoggingConfiguration();
            FileTarget logFile = new FileTarget("gateway-log")
            {
                FileName = logPath,
                ArchiveAboveSize = 41943040,
                MaxArchiveFiles = 7,
                Layout = "${displaymagicianlog:component=Gateway}"
            };
            LoggingRule loggingRule = new LoggingRule("GatewayFileLog");
            loggingRule.EnableLoggingForLevels(LogLevel.Info, LogLevel.Fatal);
            loggingRule.Targets.Add(logFile);
            loggingRule.LoggerNamePattern = "*";
            configuration.LoggingRules.Add(loggingRule);
            LogManager.Configuration = configuration;
        }
        catch
        {
            // The Windows Service Control Manager must receive startup failures even if the log directory ACL is unavailable.
        }
    }
}
