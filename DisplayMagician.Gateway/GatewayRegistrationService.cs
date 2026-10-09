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
    private readonly GatewayRegistrationState _registrationState;

    public GatewayRegistrationService(GatewayIdentity identity, GatewaySettings settings, GatewayControlServiceClient controlServiceClient, GatewayRegistrationState registrationState)
    {
        _identity = identity ?? throw new ArgumentNullException(nameof(identity));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _controlServiceClient = controlServiceClient ?? throw new ArgumentNullException(nameof(controlServiceClient));
        _registrationState = registrationState ?? throw new ArgumentNullException(nameof(registrationState));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            GatewayPairingIdentity registration = new GatewayPairingIdentity
            {
                GatewayUri = new UriBuilder(Uri.UriSchemeHttps, string.IsNullOrWhiteSpace(_settings.LanAdvertisedHost) ? "localhost" : _settings.LanAdvertisedHost, _settings.LanPort).Uri.GetLeftPart(UriPartial.Authority),
                AdditionalGatewayUris = string.IsNullOrWhiteSpace(_settings.RemoteHost)
                    ? Array.Empty<string>()
                    : new[] { new UriBuilder(Uri.UriSchemeHttps, _settings.RemoteHost, _settings.RemotePort).Uri.GetLeftPart(UriPartial.Authority) },
                HostId = _identity.HostId,
                HostIdentityPublicKeyJwk = _identity.HostIdentityPublicKeyJwk,
                TlsSpkiSha256 = _identity.TlsSpkiSha256
            };
            ControlResponse response = await _controlServiceClient.RegisterAsync(registration, stoppingToken).ConfigureAwait(false);
            if (response.IsSuccessful)
            {
                _registrationState.SetRegistered(true);
                Logger.Debug("GatewayRegistrationService/ExecuteAsync: Registered the Gateway identity with Control Service.");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken).ConfigureAwait(false);
                continue;
            }

            _registrationState.SetRegistered(false);
            Logger.Warn("GatewayRegistrationService/ExecuteAsync: Could not register the Gateway identity with Control Service: {0}", response.Message);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).ConfigureAwait(false);
        }
    }
}
