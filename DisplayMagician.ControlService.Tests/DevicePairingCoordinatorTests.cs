using System;
using System.IO;
using System.Linq;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class DevicePairingCoordinatorTests
{
    [Fact]
    public void LocalApproval_PersistsTheApprovedPublicKeyAssociation()
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
            PairedClient[] clients = new PairedClientRepository(paths).GetActiveForUser("S-1-5-21-100");

            Assert.Equal(DevicePairingState.AwaitingApproval, submitted.State);
            Assert.Equal(DevicePairingState.Approved, approved.State);
            PairedClient client = Assert.Single(clients);
            Assert.Equal("phone-123", client.DeviceId);
            Assert.Equal("Phone", client.DisplayName);
            Assert.Equal(new[] { RemoteClientCapabilities.StatusRead, RemoteClientCapabilities.PairingApprove }, client.GrantedCapabilities);
            Assert.NotEmpty(client.PublicKeyFingerprint);
            Assert.DoesNotContain(qrCode.PairingSecret, File.ReadAllText(Path.Combine(paths.MachinePath, "DevicePairingSessions.json")));
        }
        finally
        {
            DeleteStorageRoot(storageRoot);
        }
    }

    [Fact]
    public void PairingSession_ExpiresAfterOneHourAndCannotBeApproved()
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

            Assert.Equal(DevicePairingState.Rejected, submitted.State);
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

    private static string CreateStorageRoot() => Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
    private static void DeleteStorageRoot(string storageRoot)
    {
        if (Directory.Exists(storageRoot))
        {
            Directory.Delete(storageRoot, true);
        }
    }

    private static GatewayPairingIdentity CreateGateway() => new GatewayPairingIdentity { GatewayUri = "https://displaymagician.local:22846", HostId = "host-123", HostIdentityPublicKeyJwk = ValidP256Jwk, TlsCertificateSha256 = new string('A', 64) };
    private static DevicePairingRequest CreateRequest(DevicePairingQrCode qrCode) => new DevicePairingRequest { PairingSessionId = qrCode.PairingSessionId, PairingSecret = qrCode.PairingSecret, DeviceId = "phone-123", DeviceDisplayName = "Phone", DevicePublicKeyJwk = ValidP256Jwk, RequestedCapabilities = new[] { RemoteClientCapabilities.StatusRead, RemoteClientCapabilities.PairingApprove } };

    private const string ValidP256Jwk = "{\"kty\":\"EC\",\"crv\":\"P-256\",\"x\":\"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\",\"y\":\"BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB\"}";
}
