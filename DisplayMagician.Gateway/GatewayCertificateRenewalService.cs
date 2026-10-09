using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.Extensions.Hosting;
using NLog;

namespace DisplayMagician.Gateway;

/// <summary>Renews the Gateway certificate while Kestrel continues to serve the stable TLS key.</summary>
public sealed class GatewayCertificateRenewalService : BackgroundService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly GatewayIdentityProvider _provider;
    private readonly GatewayIdentity _identity;
    private readonly GatewaySettings _settings;

    public GatewayCertificateRenewalService(GatewayIdentityProvider provider, GatewayIdentity identity, GatewaySettings settings)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _identity = identity ?? throw new ArgumentNullException(nameof(identity));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromHours(1));
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            if (_identity.TlsCertificate.NotAfter.ToUniversalTime() > DateTime.UtcNow.AddDays(30))
            {
                continue;
            }

            try
            {
                GatewayIdentity refreshed = _provider.GetOrCreate(_settings);
                X509Certificate2 current = _identity.TlsCertificate;
                if (!string.Equals(current.Thumbprint, refreshed.TlsCertificate.Thumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    _identity.ReplaceCertificate(refreshed.TlsCertificate);
                    Logger.Info("GatewayCertificateRenewalService/ExecuteAsync: Renewed the Gateway TLS certificate while retaining the configured key.");
                }
                else
                {
                    refreshed.TlsCertificate.Dispose();
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.Security.Cryptography.CryptographicException or System.IO.IOException or UnauthorizedAccessException)
            {
                Logger.Error(ex, "GatewayCertificateRenewalService/ExecuteAsync: Could not renew the Gateway TLS certificate.");
            }
        }
    }
}
