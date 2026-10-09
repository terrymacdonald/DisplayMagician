using System;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class PairedClientSupportSnapshotTests
{
    [Fact]
    public void SnapshotExcludesCredentialHashAndKeyMaterial()
    {
        string snapshot = PairedClientSupportSnapshot.Serialize(new[]
        {
            new PairedClient
            {
                DeviceId = "phone",
                OwnerUserSid = "owner",
                CredentialHash = new string('A', 64),
                PublicKeyJwk = "private-key-marker",
                PublicKeyFingerprint = "key-fingerprint-marker"
            }
        });

        Assert.Contains("phone", snapshot);
        Assert.DoesNotContain(new string('A', 64), snapshot);
        Assert.DoesNotContain("CredentialHash", snapshot);
        Assert.DoesNotContain("private-key-marker", snapshot);
        Assert.DoesNotContain("key-fingerprint-marker", snapshot);
    }
}
