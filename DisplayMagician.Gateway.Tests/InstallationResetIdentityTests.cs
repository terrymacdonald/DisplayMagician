using System;
using System.IO;
using System.Text.Json;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class InstallationResetIdentityTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-ResetIdentity-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void IdentityResetRetriesOnFailureAndConsumesSuccessfulRequestOnce()
    {
        Directory.CreateDirectory(_root);
        string path = Path.Combine(_root, "GatewayIdentityReset.json");
        File.WriteAllText(path, "{\"InstallationReset\":true}");
        Assert.Throws<IOException>(() => GatewayIdentityProvider.ApplyInstallationReset(path, () => throw new IOException("key unavailable")));
        Assert.True(File.Exists(path));
        int resets = 0;
        GatewayIdentityProvider.ApplyInstallationReset(path, () => resets++);
        GatewayIdentityProvider.ApplyInstallationReset(path, () => resets++);
        Assert.Equal(1, resets);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void InvalidIdentityResetDoesNotDeleteKeysOrConsumeEvidence()
    {
        Directory.CreateDirectory(_root);
        string path = Path.Combine(_root, "GatewayIdentityReset.json");
        File.WriteAllText(path, "{\"InstallationReset\":false}");
        int resets = 0;
        Assert.Throws<JsonException>(() => GatewayIdentityProvider.ApplyInstallationReset(path, () => resets++));
        Assert.Equal(0, resets);
        Assert.True(File.Exists(path));
    }

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
