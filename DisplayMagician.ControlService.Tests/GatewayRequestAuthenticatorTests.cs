using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class GatewayRequestAuthenticatorTests
{
    [Fact]
    public void LongLivedCredentialAuthenticatesRepeatedlyAndCannotBeReplayedAfterRevocation()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            string credential = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            PairedClientRepository repository = new PairedClientRepository(new StoragePaths(root));
            repository.Upsert(new PairedClient
            {
                DeviceId = "device-1",
                OwnerUserSid = "S-1-5-21-100",
                CredentialHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential))),
                GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead }
            });
            GatewayRequestAuthenticator authenticator = new GatewayRequestAuthenticator(repository);
            DateTime now = DateTime.UtcNow;
            GatewayAuthenticationRequest request = new GatewayAuthenticationRequest { BearerCredential = credential, SourceIpAddress = "192.168.1.20" };

            Assert.True(authenticator.Authenticate(request, now).IsAuthenticated);
            Assert.True(authenticator.Authenticate(request, now.AddMonths(6)).IsAuthenticated);
            Assert.False(authenticator.Authenticate(new GatewayAuthenticationRequest { BearerCredential = new string('Z', 43) }, now).IsAuthenticated);
            PairedClient client = repository.FindActiveByDeviceId("device-1")!;
            Assert.Equal("192.168.1.20", client.LastKnownIpAddress);
            Assert.Equal(now.AddMonths(6), client.LastAuthenticatedUtc);
            Assert.True(repository.Revoke("S-1-5-21-100", "device-1", now.AddMonths(6)));
            Assert.False(authenticator.Authenticate(request, now.AddMonths(6).AddSeconds(1)).IsAuthenticated);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
