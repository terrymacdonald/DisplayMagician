using System;
using System.IO;
using System.Linq;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class DevicePairingCoordinatorTests
{
    [Fact]
    public void LocalApproval_DeliversCredentialOnceAndPersistsOnlyItsHash()
    {
        string storageRoot = CreateStorageRoot();
        try
        {
            StoragePaths paths = new StoragePaths(storageRoot);
            PairedClientRepository repository = new PairedClientRepository(paths);
            DevicePairingCoordinator coordinator = new DevicePairingCoordinator(paths, repository);
            DateTime now = DateTime.UtcNow;
            DevicePairingQrCode qrCode = coordinator.CreateQrCode("S-1-5-21-100", CreateGateway(), now);

            DevicePairingResult submitted = coordinator.Submit(CreateRequest(qrCode), now.AddMinutes(1));
            DevicePairingResult approved = coordinator.ApproveFromLocalClient("S-1-5-21-100", new ApproveDevicePairingRequest { PairingSessionId = qrCode.PairingSessionId, GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead, RemoteClientCapabilities.PairingApprove } }, now.AddMinutes(2));
            Assert.Empty(repository.GetActiveForUser("S-1-5-21-100"));
            DevicePairingResult delivered = coordinator.GetStatus(new DevicePairingStatusRequest { PairingSessionId = qrCode.PairingSessionId, PollingSecret = PollingSecret, DeviceId = "phone-123" }, now.AddMinutes(3));
            DevicePairingResult consumed = coordinator.GetStatus(new DevicePairingStatusRequest { PairingSessionId = qrCode.PairingSessionId, PollingSecret = PollingSecret, DeviceId = "phone-123" }, now.AddMinutes(4));
            DevicePairingResult consumedAfterRestart = new DevicePairingCoordinator(paths, new PairedClientRepository(paths)).GetStatus(new DevicePairingStatusRequest { PairingSessionId = qrCode.PairingSessionId, PollingSecret = PollingSecret, DeviceId = "phone-123" }, now.AddMinutes(4));
            PairedClient[] clients = new PairedClientRepository(paths).GetActiveForUser("S-1-5-21-100");

            Assert.Equal(DevicePairingState.AwaitingApproval, submitted.State);
            Assert.Equal(DevicePairingState.Approved, approved.State);
            Assert.Equal(DevicePairingState.Approved, delivered.State);
            Assert.Equal(43, Assert.IsType<string>(delivered.Credential).Length);
            Assert.Equal(DevicePairingState.Expired, consumed.State);
            Assert.Equal(DevicePairingState.Expired, consumedAfterRestart.State);
            PairedClient client = Assert.Single(clients);
            Assert.Equal("phone-123", client.DeviceId);
            Assert.Equal("Phone", client.DisplayName);
            Assert.Equal(new[] { RemoteClientCapabilities.StatusRead, RemoteClientCapabilities.PairingApprove }, client.GrantedCapabilities);
            Assert.NotEmpty(client.PublicKeyFingerprint);
            Assert.Equal(64, client.CredentialHash.Length);
            Assert.DoesNotContain(delivered.Credential, File.ReadAllText(Path.Combine(paths.MachinePath, "PairedClients.json")));
            Assert.DoesNotContain(PollingSecret, File.ReadAllText(Path.Combine(paths.MachinePath, "DevicePairingSessions.json")));
            Assert.DoesNotContain(qrCode.PairingSecret, File.ReadAllText(Path.Combine(paths.MachinePath, "DevicePairingSessions.json")));
        }
        finally
        {
            DeleteStorageRoot(storageRoot);
        }
    }

    [Fact]
    public void PairingSession_ExpiresAfterTenMinutesAndCannotBeApproved()
    {
        string storageRoot = CreateStorageRoot();
        try
        {
            StoragePaths paths = new StoragePaths(storageRoot);
            DevicePairingCoordinator coordinator = new DevicePairingCoordinator(paths, new PairedClientRepository(paths));
            DateTime now = DateTime.UtcNow;
            DevicePairingQrCode qrCode = coordinator.CreateQrCode("S-1-5-21-100", CreateGateway(), now);

            DevicePairingResult submitted = coordinator.Submit(CreateRequest(qrCode), now.Add(DevicePairingCoordinator.PairingLifetime).AddSeconds(1));
            DevicePairingResult approval = coordinator.ApproveFromLocalClient("S-1-5-21-100", new ApproveDevicePairingRequest { PairingSessionId = qrCode.PairingSessionId }, now.Add(DevicePairingCoordinator.PairingLifetime).AddSeconds(2));

            Assert.Equal(DevicePairingState.Expired, submitted.State);
            Assert.Equal(DevicePairingState.Rejected, approval.State);
        }
        finally
        {
            DeleteStorageRoot(storageRoot);
        }
    }

    [Fact]
    public void PairingSubmission_CanBeRetriedAfterALostResponse()
    {
        string storageRoot = CreateStorageRoot();
        try
        {
            StoragePaths paths = new StoragePaths(storageRoot);
            DevicePairingCoordinator coordinator = new DevicePairingCoordinator(paths, new PairedClientRepository(paths));
            DateTime now = DateTime.UtcNow;
            DevicePairingQrCode qrCode = coordinator.CreateQrCode("S-1-5-21-100", CreateGateway(), now);
            DevicePairingRequest request = CreateRequest(qrCode);

            DevicePairingResult firstResult = coordinator.Submit(request, now.AddMinutes(1));
            DevicePairingResult retryResult = coordinator.Submit(request, now.AddMinutes(2));

            Assert.Equal(DevicePairingState.AwaitingApproval, firstResult.State);
            Assert.Equal(DevicePairingState.AwaitingApproval, retryResult.State);
            Assert.Equal("phone-123", retryResult.DeviceId);
        }
        finally
        {
            DeleteStorageRoot(storageRoot);
        }
    }

    [Fact]
    public void BearerPairingDoesNotRequireADeviceKeyPairAndGrantsAllRequestedCapabilities()
    {
        string root = CreateStorageRoot();
        try
        {
            StoragePaths paths = new StoragePaths(root);
            PairedClientRepository clients = new PairedClientRepository(paths);
            DevicePairingCoordinator coordinator = new DevicePairingCoordinator(paths, clients);
            DateTime now = DateTime.UtcNow;
            DevicePairingQrCode qr = coordinator.CreateQrCode("owner", CreateGateway(), now);
            DevicePairingRequest request = CreateRequest(qr);
            request.DevicePublicKeyJwk = string.Empty;

            Assert.Equal(DevicePairingState.AwaitingApproval, coordinator.Submit(request, now.AddMinutes(1)).State);
            Assert.Equal(DevicePairingState.Rejected, coordinator.ApproveFromLocalClient("owner", new ApproveDevicePairingRequest { PairingSessionId = qr.PairingSessionId, GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead } }, now.AddMinutes(2)).State);
            Assert.Equal(DevicePairingState.Approved, coordinator.ApproveFromLocalClient("owner", new ApproveDevicePairingRequest { PairingSessionId = qr.PairingSessionId, GrantedCapabilities = request.RequestedCapabilities }, now.AddMinutes(2)).State);
            DevicePairingResult delivered = coordinator.GetStatus(new DevicePairingStatusRequest { PairingSessionId = qr.PairingSessionId, PollingSecret = PollingSecret, DeviceId = request.DeviceId }, now.AddMinutes(3));
            Assert.Equal(request.RequestedCapabilities, delivered.GrantedCapabilities);
            Assert.True(new GatewayRequestAuthenticator(clients).Authenticate(new GatewayAuthenticationRequest { BearerCredential = Assert.IsType<string>(delivered.Credential) }, now.AddMinutes(3)).IsAuthenticated);
        }
        finally
        {
            DeleteStorageRoot(root);
        }
    }

    [Fact]
    public void RePairingSameUserReplacesOldCredentialButCannotClaimAnotherUsersDeviceId()
    {
        string root = CreateStorageRoot();
        try
        {
            StoragePaths paths = new StoragePaths(root);
            PairedClientRepository clients = new PairedClientRepository(paths);
            DevicePairingCoordinator coordinator = new DevicePairingCoordinator(paths, clients);
            DateTime now = DateTime.UtcNow;
            DevicePairingQrCode firstQr = coordinator.CreateQrCode("owner", CreateGateway(), now);
            coordinator.Submit(CreateRequest(firstQr), now.AddMinutes(1));
            coordinator.ApproveFromLocalClient("owner", new ApproveDevicePairingRequest { PairingSessionId = firstQr.PairingSessionId, GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead, RemoteClientCapabilities.PairingApprove } }, now.AddMinutes(2));
            string firstCredential = Assert.IsType<string>(coordinator.GetStatus(new DevicePairingStatusRequest { PairingSessionId = firstQr.PairingSessionId, PollingSecret = PollingSecret, DeviceId = "phone-123" }, now.AddMinutes(3)).Credential);

            DevicePairingQrCode secondQr = coordinator.CreateQrCode("owner", CreateGateway(), now.AddMinutes(3));
            DevicePairingRequest secondRequest = CreateRequest(secondQr);
            secondRequest.RequestedCapabilities = new[] { RemoteClientCapabilities.PairingApprove };
            coordinator.Submit(secondRequest, now.AddMinutes(4));
            Assert.Equal(DevicePairingState.Approved, coordinator.ApproveFromLocalClient("owner", new ApproveDevicePairingRequest { PairingSessionId = secondQr.PairingSessionId, GrantedCapabilities = new[] { RemoteClientCapabilities.PairingApprove } }, now.AddMinutes(5)).State);
            Assert.Null(clients.FindActiveByCredential(firstCredential));
            string secondCredential = Assert.IsType<string>(coordinator.GetStatus(new DevicePairingStatusRequest { PairingSessionId = secondQr.PairingSessionId, PollingSecret = PollingSecret, DeviceId = "phone-123" }, now.AddMinutes(6)).Credential);
            Assert.NotNull(clients.FindActiveByCredential(secondCredential));

            DevicePairingQrCode anotherUsersQr = coordinator.CreateQrCode("other-owner", CreateGateway(), now.AddMinutes(6));
            coordinator.Submit(CreateRequest(anotherUsersQr), now.AddMinutes(7));
            Assert.Equal(DevicePairingState.Rejected, coordinator.ApproveFromLocalClient("other-owner", new ApproveDevicePairingRequest { PairingSessionId = anotherUsersQr.PairingSessionId, GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead } }, now.AddMinutes(8)).State);
            Assert.Equal("owner", clients.FindActiveByCredential(secondCredential)!.OwnerUserSid);
        }
        finally
        {
            DeleteStorageRoot(root);
        }
    }

    [Fact]
    public void PairedApproval_RequiresThePairingApprovalCapability()
    {
        string storageRoot = CreateStorageRoot();
        try
        {
            StoragePaths paths = new StoragePaths(storageRoot);
            PairedClientRepository repository = new PairedClientRepository(paths);
            repository.Upsert(new PairedClient { DeviceId = "approved-device", OwnerUserSid = "S-1-5-21-100", DisplayName = "Existing Phone", PublicKeyJwk = ValidP256Jwk, PublicKeyFingerprint = "fingerprint", GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead }, PairedUtc = DateTime.UtcNow });
            DevicePairingCoordinator coordinator = new DevicePairingCoordinator(paths, repository);
            DateTime now = DateTime.UtcNow;
            DevicePairingQrCode qrCode = coordinator.CreateQrCode("S-1-5-21-100", CreateGateway(), now);
            coordinator.Submit(CreateRequest(qrCode), now.AddMinutes(1));

            DevicePairingResult result = coordinator.ApproveFromPairedClient("S-1-5-21-100", "approved-device", new ApproveDevicePairingRequest { PairingSessionId = qrCode.PairingSessionId, GrantedCapabilities = new[] { RemoteClientCapabilities.StatusRead } }, now.AddMinutes(2));

            Assert.Equal(DevicePairingState.Rejected, result.State);
            Assert.DoesNotContain(repository.GetActiveForUser("S-1-5-21-100"), client => client.DeviceId == "phone-123");

            repository.Upsert(new PairedClient { DeviceId = "approved-device", OwnerUserSid = "S-1-5-21-100", DisplayName = "Existing Phone", PublicKeyJwk = ValidP256Jwk, PublicKeyFingerprint = "fingerprint", GrantedCapabilities = new[] { RemoteClientCapabilities.PairingApprove }, PairedUtc = now });
            DevicePairingSessionView pending = Assert.Single(coordinator.GetPendingForUser("S-1-5-21-100", now.AddMinutes(2)));
            DevicePairingResult approved = coordinator.ApproveFromPairedClient("S-1-5-21-100", "approved-device", new ApproveDevicePairingRequest { PairingSessionId = qrCode.PairingSessionId, GrantedCapabilities = pending.RequestedCapabilities }, now.AddMinutes(3));
            Assert.Equal(DevicePairingState.Approved, approved.State);
            DevicePairingResult delivered = coordinator.GetStatus(new DevicePairingStatusRequest { PairingSessionId = qrCode.PairingSessionId, PollingSecret = PollingSecret }, now.AddMinutes(4));
            Assert.Equal(pending.RequestedCapabilities, delivered.GrantedCapabilities);
        }
        finally
        {
            DeleteStorageRoot(storageRoot);
        }
    }

    [Fact]
    public void PairingRequest_RejectsPrivateKeyMaterialAndUnrequestedCapabilities()
    {
        string storageRoot = CreateStorageRoot();
        try
        {
            StoragePaths paths = new StoragePaths(storageRoot);
            DevicePairingCoordinator coordinator = new DevicePairingCoordinator(paths, new PairedClientRepository(paths));
            DateTime now = DateTime.UtcNow;
            DevicePairingQrCode privateKeyQrCode = coordinator.CreateQrCode("S-1-5-21-100", CreateGateway(), now);
            DevicePairingRequest privateKeyRequest = CreateRequest(privateKeyQrCode);
            privateKeyRequest.DevicePublicKeyJwk = "{\"kty\":\"EC\",\"crv\":\"P-256\",\"x\":\"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\",\"y\":\"BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB\",\"d\":\"secret\"}";

            DevicePairingResult privateKeyResult = coordinator.Submit(privateKeyRequest, now.AddMinutes(1));
            DevicePairingQrCode capabilityQrCode = coordinator.CreateQrCode("S-1-5-21-100", CreateGateway(), now);
            coordinator.Submit(CreateRequest(capabilityQrCode), now.AddMinutes(1));
            DevicePairingResult capabilityResult = coordinator.ApproveFromLocalClient("S-1-5-21-100", new ApproveDevicePairingRequest { PairingSessionId = capabilityQrCode.PairingSessionId, GrantedCapabilities = new[] { RemoteClientCapabilities.ShortcutsRun } }, now.AddMinutes(2));

            Assert.Equal(DevicePairingState.Rejected, privateKeyResult.State);
            Assert.Equal(DevicePairingState.Rejected, capabilityResult.State);
        }
        finally
        {
            DeleteStorageRoot(storageRoot);
        }
    }

    [Fact]
    public void PairingStatus_RequiresTheCorrectOneTimeSecretAndDevice()
    {
        string storageRoot = CreateStorageRoot();
        try
        {
            StoragePaths paths = new StoragePaths(storageRoot);
            DevicePairingCoordinator coordinator = new DevicePairingCoordinator(paths, new PairedClientRepository(paths));
            DateTime now = DateTime.UtcNow;
            DevicePairingQrCode qrCode = coordinator.CreateQrCode("S-1-5-21-100", CreateGateway(), now);
            coordinator.Submit(CreateRequest(qrCode), now.AddMinutes(1));

            DevicePairingResult valid = coordinator.GetStatus(new DevicePairingStatusRequest { PairingSessionId = qrCode.PairingSessionId, PollingSecret = PollingSecret, DeviceId = "phone-123" }, now.AddMinutes(2));
            DevicePairingResult invalid = coordinator.GetStatus(new DevicePairingStatusRequest { PairingSessionId = qrCode.PairingSessionId, PollingSecret = "wrong", DeviceId = "phone-123" }, now.AddMinutes(2));

            Assert.Equal(DevicePairingState.AwaitingApproval, valid.State);
            Assert.Equal(DevicePairingState.Rejected, invalid.State);
        }
        finally { DeleteStorageRoot(storageRoot); }
    }

    [Fact]
    public void PairingStatus_EnforcesTwoSecondPollingInterval()
    {
        string root = CreateStorageRoot();
        try
        {
            StoragePaths paths = new StoragePaths(root);
            DevicePairingCoordinator coordinator = new DevicePairingCoordinator(paths, new PairedClientRepository(paths));
            DateTime now = DateTime.UtcNow;
            DevicePairingQrCode qr = coordinator.CreateQrCode("owner", CreateGateway(), now);
            coordinator.Submit(CreateRequest(qr), now.AddSeconds(1));
            DevicePairingStatusRequest request = new DevicePairingStatusRequest { PairingSessionId = qr.PairingSessionId, PollingSecret = PollingSecret, DeviceId = "phone-123" };

            Assert.Equal(DevicePairingState.AwaitingApproval, coordinator.GetStatus(request, now.AddSeconds(2)).State);
            Assert.Equal("rate-limited", coordinator.GetStatus(request, now.AddSeconds(3)).ProblemCode);
            Assert.Equal(DevicePairingState.AwaitingApproval, coordinator.GetStatus(request, now.AddSeconds(4)).State);
        }
        finally
        {
            DeleteStorageRoot(root);
        }
    }

    private static string CreateStorageRoot() => Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
    private static void DeleteStorageRoot(string storageRoot)
    {
        if (Directory.Exists(storageRoot))
        {
            Directory.Delete(storageRoot, true);
        }
    }

    private static GatewayPairingIdentity CreateGateway() => new GatewayPairingIdentity { GatewayUri = "https://displaymagician.local:22846", HostId = "host-123", HostIdentityPublicKeyJwk = ValidP256Jwk, TlsSpkiSha256 = Convert.ToBase64String(new byte[32]) };
    private static DevicePairingRequest CreateRequest(DevicePairingQrCode qrCode) => new DevicePairingRequest { PairingSessionId = qrCode.PairingSessionId, PairingSecret = qrCode.PairingSecret, PollingSecret = PollingSecret, DeviceId = "phone-123", DeviceDisplayName = "Phone", ClientType = "Android phone", SourceIpAddress = "192.168.1.20", DevicePublicKeyJwk = ValidP256Jwk, RequestedCapabilities = new[] { RemoteClientCapabilities.StatusRead, RemoteClientCapabilities.PairingApprove } };

    private static readonly string PollingSecret = Convert.ToBase64String(Enumerable.Repeat((byte)7, 32).ToArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private const string ValidP256Jwk = "{\"kty\":\"EC\",\"crv\":\"P-256\",\"x\":\"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\",\"y\":\"BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB\"}";
}
