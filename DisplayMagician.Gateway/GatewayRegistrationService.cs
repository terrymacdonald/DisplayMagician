using System;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.Extensions.Hosting;
using NLog;

namespace DisplayMagician.Gateway;

/// <summary>Registers the running Gateway identity with Control Service and retries if services start out of order.</summary>
public sealed class GatewayRegistrationService : BackgroundService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly GatewayIdentity _identity;
    private readonly GatewaySettings _settings;
    private readonly GatewayControlServiceClient _controlServiceClient;

    public GatewayRegistrationService(GatewayIdentity identity, GatewaySettings settings, GatewayControlServiceClient controlServiceClient)
    {
        _identity = identity ?? throw new ArgumentNullException(nameof(identity));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _controlServiceClient = controlServiceClient ?? throw new ArgumentNullException(nameof(controlServiceClient));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        GatewayPairingIdentity registration = new GatewayPairingIdentity
        {
            GatewayUri = $"https://localhost:{_settings.LanPort}",
            HostId = _identity.HostId,
            HostIdentityPublicKeyJwk = _identity.HostIdentityPublicKeyJwk,
            TlsCertificateSha256 = _identity.TlsCertificateSha256
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            ControlResponse response = await _controlServiceClient.RegisterAsync(registration, stoppingToken).ConfigureAwait(false);
            if (response.IsSuccessful)
            {
                Logger.Debug("GatewayRegistrationService/ExecuteAsync: Registered the Gateway identity with Control Service.");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken).ConfigureAwait(false);
                continue;
            }

            Logger.Warn("GatewayRegistrationService/ExecuteAsync: Could not register the Gateway identity with Control Service: {0}", response.Message);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).ConfigureAwait(false);
        }
    }
}
