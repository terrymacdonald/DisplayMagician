using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class ProtocolNegotiationTests
{
    [Fact]
    public void TryCreateWelcome_SelectsTheCompatibleVersionAndAdvertisesCapabilities()
    {
        bool wasAccepted = ControlProtocol.TryCreateWelcome(new ProtocolHello
        {
            MinimumProtocolVersion = ControlProtocol.MinimumSupportedVersion,
            MaximumProtocolVersion = ControlProtocol.CurrentVersion,
            RequiredCapabilities = new[] { "operation-status" }
        }, "ControlService", ControlProtocol.ControlServiceCapabilities, out ProtocolWelcome? welcome, out ControlErrorCode errorCode, out string message);

        Assert.True(wasAccepted);
        Assert.Equal(ControlErrorCode.None, errorCode);
        Assert.Empty(message);
        Assert.NotNull(welcome);
        Assert.Equal(ControlProtocol.CurrentVersion, welcome!.SelectedProtocolVersion);
        Assert.Contains("operation-status", welcome.SupportedCapabilities);
    }

    [Fact]
    public void TryCreateWelcome_RejectsAnIncompatibleVersionRange()
    {
        bool wasAccepted = ControlProtocol.TryCreateWelcome(new ProtocolHello { MinimumProtocolVersion = ControlProtocol.CurrentVersion + 1, MaximumProtocolVersion = ControlProtocol.CurrentVersion + 2 }, "ControlService", ControlProtocol.ControlServiceCapabilities, out _, out ControlErrorCode errorCode, out _);

        Assert.False(wasAccepted);
        Assert.Equal(ControlErrorCode.IncompatibleProtocolVersion, errorCode);
    }

    [Fact]
    public void TryCreateWelcome_RejectsRequiredCapabilitiesThatAreUnavailable()
    {
        bool wasAccepted = ControlProtocol.TryCreateWelcome(new ProtocolHello { RequiredCapabilities = new[] { "future-only-capability" } }, "ControlService", ControlProtocol.ControlServiceCapabilities, out _, out ControlErrorCode errorCode, out _);

        Assert.False(wasAccepted);
        Assert.Equal(ControlErrorCode.RequiredCapabilityUnavailable, errorCode);
    }
}
