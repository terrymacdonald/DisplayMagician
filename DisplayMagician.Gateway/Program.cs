using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
        GatewaySettings settings = GatewaySettingsProvider.Load();
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddWindowsService(options => options.ServiceName = "DisplayMagicianGateway");
        builder.Services.AddSingleton(identity);
        builder.Services.AddSingleton(settings);
        builder.Services.AddSingleton<GatewayControlServiceClient>();
        builder.Services.AddHostedService<GatewayRegistrationService>();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;
            options.Limits.MaxRequestBodySize = ControlProtocol.MaximumMessageLength;
            if (settings.LanBindAddress == "*" || string.IsNullOrWhiteSpace(settings.LanBindAddress))
            {
                options.ListenAnyIP(settings.LanPort, listenOptions => listenOptions.UseHttps(identity.TlsCertificate));
            }
            else if (IPAddress.TryParse(settings.LanBindAddress, out IPAddress? bindAddress))
            {
                options.Listen(bindAddress, settings.LanPort, listenOptions => listenOptions.UseHttps(identity.TlsCertificate));
            }
            else
            {
                throw new InvalidOperationException("Gateway LAN bind address must be an IP address or all interfaces.");
            }
        });

        WebApplication app = builder.Build();
        app.MapGet("/v1/identity", (GatewayIdentity gatewayIdentity) => Results.Ok(gatewayIdentity.ToView()));
        app.MapPost("/v1/pairing/request", async (DevicePairingRequest request, HttpContext httpContext, GatewayControlServiceClient controlServiceClient, CancellationToken cancellationToken) =>
        {
            request.SourceIpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            DevicePairingResult result = await controlServiceClient.SubmitDevicePairingAsync(request, cancellationToken).ConfigureAwait(false);
            return Results.Json(result, statusCode: result.State == DevicePairingState.AwaitingApproval ? StatusCodes.Status202Accepted : StatusCodes.Status400BadRequest);
        });
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
