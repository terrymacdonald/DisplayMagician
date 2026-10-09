using System;
using System.IO;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class GatewaySettingsStoreTests
{
    [Fact]
    public void PublicNetworksDefaultOffAndSurviveReloadWhenApproved()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianGatewaySettingsTests", Guid.NewGuid().ToString("N"));
        try
        {
            StoragePaths paths = new StoragePaths(root);
            Directory.CreateDirectory(paths.MachinePath);
            File.WriteAllText(Path.Combine(paths.MachinePath, "GatewaySettings.json"), "{\"LanPort\":22846}");
            GatewaySettingsStore store = new GatewaySettingsStore(paths);
            Assert.False(store.Get().AllowPublicNetworks);

            GatewaySettings approved = store.Get();
            approved.AllowPublicNetworks = true;
            store.Update(approved);

            Assert.True(new GatewaySettingsStore(paths).Get().AllowPublicNetworks);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
