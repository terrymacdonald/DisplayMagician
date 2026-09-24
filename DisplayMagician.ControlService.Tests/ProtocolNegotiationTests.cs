using System.Linq;
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
            ClientKind = ControlClientKind.DesktopApplication,
            ClientId = "DisplayMagician.Test",
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
        bool wasAccepted = ControlProtocol.TryCreateWelcome(new ProtocolHello { ClientKind = ControlClientKind.DesktopApplication, ClientId = "DisplayMagician.Test", MinimumProtocolVersion = ControlProtocol.CurrentVersion + 1, MaximumProtocolVersion = ControlProtocol.CurrentVersion + 2 }, "ControlService", ControlProtocol.ControlServiceCapabilities, out _, out ControlErrorCode errorCode, out _);

        Assert.False(wasAccepted);
        Assert.Equal(ControlErrorCode.IncompatibleProtocolVersion, errorCode);
    }

    [Fact]
    public void TryCreateWelcome_RejectsRequiredCapabilitiesThatAreUnavailable()
    {
        bool wasAccepted = ControlProtocol.TryCreateWelcome(new ProtocolHello { ClientKind = ControlClientKind.DesktopApplication, ClientId = "DisplayMagician.Test", RequiredCapabilities = new[] { "future-only-capability" } }, "ControlService", ControlProtocol.ControlServiceCapabilities, out _, out ControlErrorCode errorCode, out _);

        Assert.False(wasAccepted);
        Assert.Equal(ControlErrorCode.RequiredCapabilityUnavailable, errorCode);
    }

    [Theory]
    [InlineData("Operation-Status")]
    [InlineData("operation status")]
    [InlineData("")]
    public void TryCreateWelcome_RejectsMalformedCapabilityIds(string capability)
    {
        bool wasAccepted = ControlProtocol.TryCreateWelcome(new ProtocolHello { ClientKind = ControlClientKind.DesktopApplication, ClientId = "DisplayMagician.Test", RequiredCapabilities = new[] { capability } }, "ControlService", ControlProtocol.ControlServiceCapabilities, out _, out ControlErrorCode errorCode, out _);

        Assert.False(wasAccepted);
        Assert.Equal(ControlErrorCode.InvalidRequest, errorCode);
    }

    [Fact]
    public void TryCreateWelcome_RejectsDuplicateOrExcessiveCapabilities()
    {
        bool duplicateAccepted = ControlProtocol.TryCreateWelcome(new ProtocolHello { ClientKind = ControlClientKind.DesktopApplication, ClientId = "DisplayMagician.Test", RequiredCapabilities = new[] { "operation-status", "operation-status" } }, "ControlService", ControlProtocol.ControlServiceCapabilities, out _, out ControlErrorCode duplicateError, out _);
        bool excessiveAccepted = ControlProtocol.TryCreateWelcome(new ProtocolHello { ClientKind = ControlClientKind.DesktopApplication, ClientId = "DisplayMagician.Test", RequiredCapabilities = Enumerable.Repeat("operation-status", ControlProtocol.MaximumRequiredCapabilities + 1).Select((value, index) => $"capability-{index}").ToArray() }, "ControlService", ControlProtocol.ControlServiceCapabilities, out _, out ControlErrorCode excessiveError, out _);

        Assert.False(duplicateAccepted);
        Assert.Equal(ControlErrorCode.InvalidRequest, duplicateError);
        Assert.False(excessiveAccepted);
        Assert.Equal(ControlErrorCode.InvalidRequest, excessiveError);
    }

    [Fact]
    public void TryCreateWelcome_ReturnsDefensiveCapabilityCopies()
    {
        string[] capabilities = new[] { "operation-status" };
        Assert.True(ControlProtocol.TryCreateWelcome(ControlProtocol.CreateHello(ControlClientKind.DesktopApplication, "DisplayMagician.Test"), "ControlService", capabilities, out ProtocolWelcome? welcome, out _, out _));

        welcome!.SupportedCapabilities[0] = "changed";

        Assert.Equal("operation-status", capabilities[0]);
    }

    [Fact]
    public void IsCompatibleWelcome_RejectsAMissingOrOutOfRangeWelcome()
    {
        ProtocolHello hello = ControlProtocol.CreateHello(ControlClientKind.DesktopApplication, "DisplayMagician.Test");

        Assert.False(ControlProtocol.IsCompatibleWelcome(hello, null));
        Assert.False(ControlProtocol.IsCompatibleWelcome(hello, new ProtocolWelcome { SelectedProtocolVersion = ControlProtocol.CurrentVersion + 1 }));
    }

    [Fact]
    public void TryCreateWelcome_NegotiatesOptionalCapabilitiesWithoutRejectingTheClient()
    {
        ProtocolHello hello = ControlProtocol.CreateHello(ControlClientKind.DesktopApplication, "DisplayMagician.Test");
        hello.OptionalCapabilities = new[] { ControlCapabilities.OperationStatus, "future-only-capability" };

        bool wasAccepted = ControlProtocol.TryCreateWelcome(hello, "ControlService", ControlProtocol.ControlServiceCapabilities, out ProtocolWelcome? welcome, out ControlErrorCode errorCode, out _);

        Assert.True(wasAccepted);
        Assert.Equal(ControlErrorCode.None, errorCode);
        Assert.Equal(new[] { ControlCapabilities.OperationStatus }, welcome!.NegotiatedOptionalCapabilities);
    }

    [Fact]
    public void TryCreateWelcome_RejectsUnknownClientsAndOversizedIdentityFields()
    {
        bool unknownAccepted = ControlProtocol.TryCreateWelcome(new ProtocolHello { ClientId = "DisplayMagician.Test" }, "ControlService", ControlProtocol.ControlServiceCapabilities, out _, out ControlErrorCode unknownError, out _);
        bool oversizedAccepted = ControlProtocol.TryCreateWelcome(new ProtocolHello { ClientKind = ControlClientKind.DesktopApplication, ClientId = new string('a', ControlProtocol.MaximumClientIdLength + 1) }, "ControlService", ControlProtocol.ControlServiceCapabilities, out _, out ControlErrorCode oversizedError, out _);

        Assert.False(unknownAccepted);
        Assert.Equal(ControlErrorCode.InvalidRequest, unknownError);
        Assert.False(oversizedAccepted);
        Assert.Equal(ControlErrorCode.InvalidRequest, oversizedError);
    }
}
