using System;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class GatewayIdentityRegistryTests
{
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
