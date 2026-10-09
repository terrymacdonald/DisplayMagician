using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class GatewayIdentityRegistryTests
{
    [Fact]
    public void HostIdentityResetRevokesPairedCredentialAcrossRestart()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            StoragePaths paths = new StoragePaths(root);
            PairedClientRepository clients = new PairedClientRepository(paths);
            DevicePairingCoordinator pairing = new DevicePairingCoordinator(paths, clients);
            GatewayIdentityRegistry registry = new GatewayIdentityRegistry(paths, clients, pairing);
            registry.Set(new GatewayPairingIdentity { HostId = "old-host" });
            string credential = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            clients.Upsert(new PairedClient { DeviceId = "phone", OwnerUserSid = "owner", CredentialHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential))) });

            new GatewayIdentityRegistry(paths, clients, pairing).Set(new GatewayPairingIdentity { HostId = "new-host" });

            Assert.Null(new PairedClientRepository(paths).FindActiveByCredential(credential));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void LocallyApprovedTlsPinChangeRevokesDevicesAndClearsApproval()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            StoragePaths paths = new StoragePaths(root);
            PairedClientRepository clients = new PairedClientRepository(paths);
            DevicePairingCoordinator pairing = new DevicePairingCoordinator(paths, clients);
            GatewayIdentityRegistry registry = new GatewayIdentityRegistry(paths, clients, pairing);
            registry.Set(new GatewayPairingIdentity { HostId = "host", TlsSpkiSha256 = "old-pin" });
            Assert.True(registry.ApproveTlsKeyReplacement(DateTime.UtcNow));
            string credential = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            clients.Upsert(new PairedClient { DeviceId = "phone", OwnerUserSid = "owner", CredentialHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential))) });

            registry.Set(new GatewayPairingIdentity { HostId = "host", TlsSpkiSha256 = "new-pin" });

            Assert.Null(clients.FindActiveByCredential(credential));
            Assert.False(File.Exists(Path.Combine(paths.MachinePath, "GatewayTlsResetApproval.json")));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void PairingEndpointMustMatchRegisteredOrigin()
    {
        GatewayIdentityRegistry registry = new GatewayIdentityRegistry();
        registry.Set(new GatewayPairingIdentity
        {
            GatewayUri = "https://displaymagician.local:22846",
            AdditionalGatewayUris = new[] { "https://dm.example.test:443" }
        });

        Assert.True(registry.AllowsGatewayUri(new Uri("https://displaymagician.local:22846")));
        Assert.True(registry.AllowsGatewayUri(new Uri("https://dm.example.test")));
        Assert.False(registry.AllowsGatewayUri(new Uri("https://other.example.test:22846")));
        Assert.False(registry.AllowsGatewayUri(new Uri("https://displaymagician.local:22847")));
        Assert.False(registry.AllowsGatewayUri(new Uri("http://displaymagician.local:22846")));
        Assert.False(registry.AllowsGatewayUri(new Uri("https://displaymagician.local:22846/path")));
    }
}
