using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class GatewayRequestAuthenticatorTests
{
    [Fact]
    public void Authenticate_AcceptsValidSignatureOnce_AndRejectsReplayAndTampering()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            DateTime now = DateTime.UtcNow;
            GatewayRequestAuthenticator authenticator = CreateAuthenticator(root, key);
            GatewayAuthenticationRequest valid = CreateRequest(key, "/v1/status", "ABC", now, "nonce-1");

            GatewayAuthenticationResult accepted = authenticator.Authenticate(valid, now);
            GatewayAuthenticationResult replayed = authenticator.Authenticate(valid, now.AddSeconds(1));
            GatewayAuthenticationRequest tampered = CreateRequest(key, "/v1/status", "ABC", now, "nonce-2");
            tampered.Path = "/v1/shortcuts";
            GatewayAuthenticationResult tamperedResult = authenticator.Authenticate(tampered, now);

            Assert.True(accepted.IsAuthenticated);
            Assert.False(replayed.IsAuthenticated);
            Assert.False(tamperedResult.IsAuthenticated);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Authenticate_RejectsStaleAndRevokedDevices()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            DateTime now = DateTime.UtcNow;
            StoragePaths paths = new StoragePaths(root);
            PairedClientRepository repository = new PairedClientRepository(paths);
            repository.Upsert(CreateClient(key));
            GatewayRequestAuthenticator authenticator = new GatewayRequestAuthenticator(repository);
            Assert.False(authenticator.Authenticate(CreateRequest(key, "/v1/status", "ABC", now.AddMinutes(-6), "stale"), now).IsAuthenticated);
            Assert.True(repository.Revoke("S-1-5-21-100", "device-1", now));
            Assert.False(authenticator.Authenticate(CreateRequest(key, "/v1/status", "ABC", now, "revoked"), now).IsAuthenticated);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Authenticate_RejectsUnknownDeviceInvalidSignatureAndBodyTampering_WithoutRecordingAuthentication()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            DateTime now = DateTime.UtcNow;
            StoragePaths paths = new StoragePaths(root);
            PairedClientRepository repository = new PairedClientRepository(paths);
            repository.Upsert(CreateClient(key));
            GatewayRequestAuthenticator authenticator = new GatewayRequestAuthenticator(repository);

            GatewayAuthenticationRequest unknownDevice = CreateRequest(key, "/v1/status", "ABC", now, "unknown");
            unknownDevice.SignedRequest.DeviceId = "unknown-device";
            GatewayAuthenticationRequest invalidSignature = CreateRequest(key, "/v1/status", "ABC", now, "invalid-signature");
            invalidSignature.SignedRequest.Signature = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            GatewayAuthenticationRequest bodyTampered = CreateRequest(key, "/v1/profiles/apply", "ABC", now, "body-tampered");
            bodyTampered.BodySha256 = "DEF";

            Assert.False(authenticator.Authenticate(unknownDevice, now).IsAuthenticated);
            Assert.False(authenticator.Authenticate(invalidSignature, now).IsAuthenticated);
            Assert.False(authenticator.Authenticate(bodyTampered, now).IsAuthenticated);
            Assert.Null(repository.FindActiveByDeviceId("device-1")!.LastAuthenticatedUtc);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Authenticate_RecordsConnectionMetadataOnlyAfterASuccessfulSignature()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            DateTime now = DateTime.UtcNow;
            StoragePaths paths = new StoragePaths(root);
            PairedClientRepository repository = new PairedClientRepository(paths);
            repository.Upsert(CreateClient(key));
            GatewayAuthenticationResult result = new GatewayRequestAuthenticator(repository).Authenticate(CreateRequest(key, "/v1/status", "ABC", now, "accepted"), now);
            PairedClient client = repository.FindActiveByDeviceId("device-1")!;

            Assert.True(result.IsAuthenticated);
            Assert.Equal("192.168.1.20", client.LastKnownIpAddress);
            Assert.Equal(now, client.LastAuthenticatedUtc);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Authenticate_ReturnsOnlyThePairedDevicesGrantedCapabilitiesAndOwner()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            DateTime now = DateTime.UtcNow;
            StoragePaths paths = new StoragePaths(root);
            PairedClientRepository repository = new PairedClientRepository(paths);
            repository.Upsert(new PairedClient { DeviceId = "device-1", OwnerUserSid = "S-1-5-21-100", DisplayName = "Read only", ClientType = "Test", PublicKeyJwk = CreateJwk(key), GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead }, PairedUtc = now });

            GatewayAuthenticationResult result = new GatewayRequestAuthenticator(repository).Authenticate(CreateRequest(key, "/v1/status", "ABC", now, "read-only"), now);

            Assert.True(result.IsAuthenticated);
            Assert.Equal("S-1-5-21-100", result.OwnerUserSid);
            Assert.Equal(new[] { RemoteClientCapabilities.StatusRead }, result.GrantedCapabilities);
            Assert.DoesNotContain(RemoteClientCapabilities.ShortcutsRun, result.GrantedCapabilities);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private static GatewayRequestAuthenticator CreateAuthenticator(string root, ECDsa key)
    {
        StoragePaths paths = new StoragePaths(root);
        PairedClientRepository repository = new PairedClientRepository(paths);
        repository.Upsert(CreateClient(key));
        return new GatewayRequestAuthenticator(repository);
    }

    private static PairedClient CreateClient(ECDsa key) => new PairedClient { DeviceId = "device-1", OwnerUserSid = "S-1-5-21-100", DisplayName = "Test", ClientType = "Test", PublicKeyJwk = CreateJwk(key), GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead }, PairedUtc = DateTime.UtcNow };

    private static GatewayAuthenticationRequest CreateRequest(ECDsa key, string path, string bodyHash, DateTime timestamp, string nonce)
    {
        string payload = SignedGatewayRequest.CreateCanonicalPayload("GET", path, bodyHash, timestamp, nonce);
        string signature = ToBase64Url(key.SignData(Encoding.UTF8.GetBytes(payload), HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation));
        return new GatewayAuthenticationRequest { SignedRequest = new SignedGatewayRequest { DeviceId = "device-1", TimestampUtc = timestamp, Nonce = nonce, Signature = signature }, Method = "GET", Path = path, BodySha256 = bodyHash, SourceIpAddress = "192.168.1.20" };
    }

    private static string CreateJwk(ECDsa key)
    {
        ECParameters parameters = key.ExportParameters(false);
        return JsonSerializer.Serialize(new { kty = "EC", crv = "P-256", x = ToBase64Url(parameters.Q.X!), y = ToBase64Url(parameters.Q.Y!) });
    }

    private static string ToBase64Url(byte[] value) => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
