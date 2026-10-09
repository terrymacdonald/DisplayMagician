using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class GatewayBearerAuthenticationTests
{
    [Fact]
    public void CredentialAuthenticatesOnlyItsOwnerUntilRevoked()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            StoragePaths paths = new StoragePaths(root);
            PairedClientRepository clients = new PairedClientRepository(paths);
            string credential = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            clients.Upsert(new PairedClient
            {
                DeviceId = "phone-1",
                OwnerUserSid = "S-1-5-21-100",
                CredentialHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential))),
                GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead }
            });
            GatewayRequestAuthenticator authenticator = new GatewayRequestAuthenticator(clients);
            DateTime now = DateTime.UtcNow;

            GatewayAuthenticationResult valid = authenticator.Authenticate(new GatewayAuthenticationRequest { BearerCredential = credential }, now);
            GatewayAuthenticationResult invalid = authenticator.Authenticate(new GatewayAuthenticationRequest { BearerCredential = new string('Z', 43) }, now);
            Assert.True(valid.IsAuthenticated);
            Assert.Equal("S-1-5-21-100", valid.OwnerUserSid);
            Assert.Equal("phone-1", valid.DeviceId);
            Assert.Equal(new[] { RemoteClientCapabilities.StatusRead }, valid.GrantedCapabilities);
            Assert.False(invalid.IsAuthenticated);
            Assert.True(clients.Revoke("S-1-5-21-100", "phone-1", now));
            Assert.False(authenticator.Authenticate(new GatewayAuthenticationRequest { BearerCredential = credential }, now).IsAuthenticated);
            Assert.DoesNotContain(credential, File.ReadAllText(Path.Combine(paths.MachinePath, "PairedClients.json")));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
