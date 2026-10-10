using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
        builder.Services.AddSingleton<GatewayTrafficLimiter>();
        builder.Services.AddSingleton<GatewayRegistrationState>();
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
        app.UseMiddleware<GatewayJsonResponseLimitMiddleware>();
        app.UseMiddleware<GatewayRequestAuthenticationMiddleware>();
        app.UseMiddleware<GatewayIdempotencyMiddleware>();
        app.MapGet("/v1/identity", (GatewayIdentity gatewayIdentity) => Results.Ok(new GatewayPublicIdentityView
        {
            HostId = gatewayIdentity.HostId,
            HostIdentityPublicKeyJwk = gatewayIdentity.HostIdentityPublicKeyJwk,
            TlsSpkiSha256 = gatewayIdentity.TlsSpkiSha256
        }));
        app.MapGet("/v1/capabilities", (GatewayIdentity gatewayIdentity) => Results.Ok(new GatewayCapabilitiesView
        {
            GatewayVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0",
            HostId = gatewayIdentity.HostId,
            KnownCapabilities = RemoteClientCapabilities.All.ToArray(),
            SupportedFeatures = new[] { "pairing", "devices", "display-profiles", "audio-profiles", "shortcuts", "operations", "decisions" }
        }));
        app.MapPost("/v1/pairing-requests", async (PairingSubmissionRequest request, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            DevicePairingRequest controlRequest = request.ToControlRequest();
            controlRequest.SourceIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            DevicePairingResult result = await client.SubmitDevicePairingAsync(controlRequest, token).ConfigureAwait(false);
            if (result.ProblemCode == "target-unavailable") return GatewayProblemDetails.CreateResult(context, StatusCodes.Status503ServiceUnavailable, "target-unavailable", "Control Service unavailable", "The pairing request could not be accepted now.", true, 1);
            if (result.State != DevicePairingState.AwaitingApproval)
            {
                return GatewayProblemDetails.CreateResult(context, result.State == DevicePairingState.Expired ? StatusCodes.Status410Gone : StatusCodes.Status422UnprocessableEntity, result.ProblemCode ?? "validation-failed", "Pairing request unavailable", result.Message);
            }

            string href = $"/v1/pairing-requests/{request.PairingRequestId:D}";
            context.Response.Headers.Location = href;
            return Results.Json(new PairingSubmissionAccepted { PairingRequestId = request.PairingRequestId, Href = href, ExpiresAt = result.ExpiresUtc }, statusCode: StatusCodes.Status202Accepted);
        });
        app.MapGet("/v1/pairing-requests/{pairingRequestId:guid}", async (Guid pairingRequestId, HttpContext context, GatewayControlServiceClient client, CancellationToken token) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            string authorization = context.Request.Headers.Authorization.ToString();
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                ControlResponse pairedResponse = await client.RemoteResourceAsync(ControlMessageType.GetRemotePairingRequest, GatewayRequestAuthenticationMiddleware.GetAuthentication(context), pairingRequestId.ToString("D"), token).ConfigureAwait(false);
                return pairedResponse.IsSuccessful && pairedResponse.DevicePairingRequest != null
                    ? Results.Ok(GatewayResourceRoutes.ToPairingItem(pairedResponse.DevicePairingRequest))
                    : GatewayResponseMapper.Map(context, pairedResponse);
            }
            const string prefix = "DisplayMagician-Pairing ";
            if (!authorization.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || authorization.Length <= prefix.Length || authorization.AsSpan(prefix.Length).Contains(' '))
            {
                return GatewayProblemDetails.CreateResult(context, StatusCodes.Status401Unauthorized, "authentication-required", "Pairing secret required", "Send the phone-generated polling secret in the DisplayMagician-Pairing Authorization scheme.");
            }

            DevicePairingResult result = await client.GetDevicePairingStatusAsync(new DevicePairingStatusRequest { PairingSessionId = pairingRequestId, PollingSecret = authorization.Substring(prefix.Length) }, token).ConfigureAwait(false);
            if (result.ProblemCode == "target-unavailable") return GatewayProblemDetails.CreateResult(context, StatusCodes.Status503ServiceUnavailable, "target-unavailable", "Control Service unavailable", "The pairing request could not be checked now.", true, 1);
            if (result.ProblemCode == "rate-limited") return GatewayProblemDetails.CreateResult(context, StatusCodes.Status429TooManyRequests, "rate-limited", "Pairing poll too frequent", result.Message, true, 2);
            if (result.State == DevicePairingState.Expired) return GatewayProblemDetails.CreateResult(context, StatusCodes.Status410Gone, result.ProblemCode ?? "pairing-expired", "Pairing delivery unavailable", result.Message);
            if (string.IsNullOrEmpty(result.DeviceId)) return GatewayProblemDetails.CreateResult(context, StatusCodes.Status404NotFound, "resource-not-found", "Pairing request not found", "The pairing request is unavailable.");
            return Results.Ok(new PairingPollView
            {
                PairingRequestId = pairingRequestId,
                Status = result.State == DevicePairingState.Approved ? "approved" : result.State == DevicePairingState.Rejected ? "rejected" : "awaiting-approval",
                ExpiresAt = result.ExpiresUtc,
                DeviceId = result.DeviceId,
                Credential = result.Credential,
                GrantedCapabilities = result.GrantedCapabilities
            });
        });
        GatewayResourceRoutes.Map(app);
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
