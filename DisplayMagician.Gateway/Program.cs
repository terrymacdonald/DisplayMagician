using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
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
        GatewaySettings settings = GatewaySettingsProvider.Load();
        GatewayIdentityProvider identityProvider = new GatewayIdentityProvider();
        GatewayIdentity identity = identityProvider.GetOrCreate(settings);
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddWindowsService(options => options.ServiceName = "DisplayMagicianGateway");
        builder.Services.AddSingleton(identity);
        builder.Services.AddSingleton(identityProvider);
        builder.Services.AddSingleton(settings);
        builder.Services.AddSingleton<GatewayControlServiceClient>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<GatewayStatusFeed>();
        builder.Services.AddSingleton<GatewayTrafficLimiter>();
        builder.Services.AddSingleton<GatewayRegistrationState>();
        builder.Services.AddHostedService(provider => provider.GetRequiredService<GatewayStatusFeed>());
        builder.Services.AddSingleton<IGatewayAuthenticationClient>(provider => provider.GetRequiredService<GatewayControlServiceClient>());
        builder.Services.AddHostedService<GatewayRegistrationService>();
        builder.Services.AddHostedService<GatewayCertificateRenewalService>();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;
            options.Limits.MaxRequestBodySize = 64 * 1024;
            if (settings.LanBindAddress == "*" || string.IsNullOrWhiteSpace(settings.LanBindAddress))
            {
                options.ListenAnyIP(settings.LanPort, listenOptions => listenOptions.UseHttps(httpsOptions => httpsOptions.ServerCertificateSelector = (_, _) => identity.TlsCertificate));
            }
            else if (IPAddress.TryParse(settings.LanBindAddress, out IPAddress? bindAddress))
            {
                options.Listen(bindAddress, settings.LanPort, listenOptions => listenOptions.UseHttps(httpsOptions => httpsOptions.ServerCertificateSelector = (_, _) => identity.TlsCertificate));
            }
            else
            {
                throw new InvalidOperationException("Gateway LAN bind address must be an IP address or all interfaces.");
            }
        });

        WebApplication app = builder.Build();
        app.UseExceptionHandler(errorApp => errorApp.Run(context =>
        {
            Exception? error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (error is Microsoft.AspNetCore.Http.BadHttpRequestException badRequest)
            {
                int status = badRequest.StatusCode;
                string code = status == StatusCodes.Status413PayloadTooLarge ? "request-too-large" : "validation-failed";
                return GatewayProblemDetails.WriteAsync(context, status, code, "Invalid request", "The request could not be accepted.");
            }

            return GatewayProblemDetails.WriteAsync(context, StatusCodes.Status500InternalServerError, "internal-error", "Internal error", "The Gateway could not complete the request.");
        }));
        app.UseStatusCodePages(statusContext =>
        {
            int status = statusContext.HttpContext.Response.StatusCode;
            string code = status == StatusCodes.Status404NotFound ? "resource-not-found" : "validation-failed";
            string title = status == StatusCodes.Status404NotFound ? "Resource not found" : "Request failed";
            return GatewayProblemDetails.WriteAsync(statusContext.HttpContext, status, code, title, "The requested resource or route is unavailable.");
        });
        app.UseMiddleware<GatewayProtocolMiddleware>();
        app.UseMiddleware<GatewayRequestAuthenticationMiddleware>();
        app.UseMiddleware<GatewayIdempotencyMiddleware>();
        app.MapGet("/v1/identity", (GatewayIdentity gatewayIdentity) => Results.Ok(gatewayIdentity.ToView()));
        app.MapPost("/v1/pairing/request", async (DevicePairingRequest request, HttpContext httpContext, GatewayControlServiceClient controlServiceClient, CancellationToken cancellationToken) =>
        {
            request.SourceIpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            DevicePairingResult result = await controlServiceClient.SubmitDevicePairingAsync(request, cancellationToken).ConfigureAwait(false);
            return result.State == DevicePairingState.AwaitingApproval
                ? Results.Json(result, statusCode: StatusCodes.Status202Accepted)
                : GatewayProblemDetails.CreateResult(httpContext, result.State == DevicePairingState.Expired ? StatusCodes.Status410Gone : StatusCodes.Status400BadRequest, string.IsNullOrWhiteSpace(result.ProblemCode) ? "validation-failed" : result.ProblemCode, "Pairing request unavailable", result.Message);
        });
        app.MapPost("/v1/pairing/status", async (DevicePairingStatusRequest request, HttpContext context, GatewayControlServiceClient controlServiceClient, CancellationToken cancellationToken) =>
        {
            DevicePairingResult result = await controlServiceClient.GetDevicePairingStatusAsync(request, cancellationToken).ConfigureAwait(false);
            if (result.ProblemCode == "rate-limited")
            {
                return GatewayProblemDetails.CreateResult(context, StatusCodes.Status429TooManyRequests, "rate-limited", "Pairing poll too frequent", result.Message, true, 2);
            }
            return result.State == DevicePairingState.Expired
                ? GatewayProblemDetails.CreateResult(context, StatusCodes.Status410Gone, result.ProblemCode ?? "pairing-expired", "Pairing delivery unavailable", result.Message)
                : Results.Json(result);
        });
        app.MapGet("/v1/status", async (HttpContext context, int? targetSessionId, DateTime? changedSinceUtc, GatewayControlServiceClient controlServiceClient, CancellationToken cancellationToken) =>
        {
            GatewayAuthenticationResult authentication = GatewayRequestAuthenticationMiddleware.GetAuthentication(context);
            if (!authentication.GrantedCapabilities.Contains(RemoteClientCapabilities.StatusRead, StringComparer.Ordinal))
            {
                return GatewayProblemDetails.CreateResult(context, StatusCodes.Status403Forbidden, "capability-denied", "Capability denied", "The paired device is not authorised to read status.");
            }

            try
            {
                return Results.Ok(await controlServiceClient.GetRemoteUserStatusAsync(authentication, targetSessionId, changedSinceUtc, cancellationToken).ConfigureAwait(false));
            }
            catch (InvalidOperationException)
            {
                return GatewayProblemDetails.CreateResult(context, StatusCodes.Status503ServiceUnavailable, "target-unavailable", "Target unavailable", "The paired user's status is temporarily unavailable.", true, 1);
            }
        });
        app.MapGet("/v1/status/stream", async (HttpContext context, int? targetSessionId, GatewayStatusFeed statusFeed, CancellationToken cancellationToken) =>
        {
            GatewayAuthenticationResult authentication = GatewayRequestAuthenticationMiddleware.GetAuthentication(context);
            if (!authentication.GrantedCapabilities.Contains(RemoteClientCapabilities.StatusRead, StringComparer.Ordinal))
            {
                await GatewayProblemDetails.WriteAsync(context, StatusCodes.Status403Forbidden, "capability-denied", "Capability denied", "The paired device is not authorised to read status.").ConfigureAwait(false);
                return;
            }

            context.Response.ContentType = "text/event-stream";
            context.Response.Headers.CacheControl = "no-cache";
            ChannelReader<RemoteUserStatus> updates = statusFeed.Subscribe(authentication, targetSessionId, cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                using CancellationTokenSource keepAliveCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                keepAliveCancellation.CancelAfter(TimeSpan.FromSeconds(15));

                try
                {
                    RemoteUserStatus status = await updates.ReadAsync(keepAliveCancellation.Token).ConfigureAwait(false);
                    string json = System.Text.Json.JsonSerializer.Serialize(status);
                    await context.Response.WriteAsync($"event: status\ndata: {json}\n\n", cancellationToken).ConfigureAwait(false);
                    await context.Response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    await context.Response.WriteAsync(": keep-alive\n\n", cancellationToken).ConfigureAwait(false);
                    await context.Response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (ChannelClosedException)
                {
                    break;
                }
            }
        });
        app.MapGet("/v1/display-profiles", async (int? targetSessionId, HttpContext context, GatewayControlServiceClient client, CancellationToken token) => GatewayResponseMapper.Map(context, await client.ListRemoteAsync(ControlMessageType.ListRemoteProfiles, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), targetSessionId, token)));
        app.MapGet("/v1/audio-profiles", async (int? targetSessionId, HttpContext context, GatewayControlServiceClient client, CancellationToken token) => GatewayResponseMapper.Map(context, await client.ListRemoteAsync(ControlMessageType.ListRemoteAudioProfiles, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), targetSessionId, token)));
        app.MapGet("/v1/shortcuts", async (int? targetSessionId, HttpContext context, GatewayControlServiceClient client, CancellationToken token) => GatewayResponseMapper.Map(context, await client.ListRemoteAsync(ControlMessageType.ListRemoteShortcuts, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), targetSessionId, token)));
        app.MapPost("/v1/display-profiles/apply", async (int? targetSessionId, ApplyProfileRequest request, HttpContext context, GatewayControlServiceClient client, CancellationToken token) => GatewayResponseMapper.Map(context, await client.ExecuteRemoteAsync(ControlMessageType.ApplyRemoteProfile, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), targetSessionId, System.Text.Json.JsonSerializer.Serialize(request), token)));
        app.MapPost("/v1/audio-profiles/apply", async (int? targetSessionId, ApplyAudioProfileRequest request, HttpContext context, GatewayControlServiceClient client, CancellationToken token) => GatewayResponseMapper.Map(context, await client.ExecuteRemoteAsync(ControlMessageType.ApplyRemoteAudioProfile, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), targetSessionId, System.Text.Json.JsonSerializer.Serialize(request), token)));
        app.MapPost("/v1/shortcuts/run", async (int? targetSessionId, StartShortcutRequest request, HttpContext context, GatewayControlServiceClient client, CancellationToken token) => GatewayResponseMapper.Map(context, await client.ExecuteRemoteAsync(ControlMessageType.StartRemoteShortcut, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), targetSessionId, System.Text.Json.JsonSerializer.Serialize(request), token)));
        app.MapPost("/v1/decisions/answer", async (int? targetSessionId, ResolveOperationDecisionRequest request, HttpContext context, GatewayControlServiceClient client, CancellationToken token) => GatewayResponseMapper.Map(context, await client.ExecuteRemoteAsync(ControlMessageType.ResolveRemoteOperationDecision, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), targetSessionId, System.Text.Json.JsonSerializer.Serialize(request), token)));
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
