using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DisplayMagician.Gateway;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class GatewayIdentityProviderTests
{
    [Fact]
    public void HostIdUsesRfc7638CanonicalMembersOnly()
    {
        const string jwk = "{\"y\":\"AgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI\",\"alg\":\"ES256\",\"x\":\"AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQE\",\"use\":\"sig\",\"kty\":\"EC\",\"crv\":\"P-256\"}";

        Assert.Equal("kOFKxjJdOqJD5G4Yuw-cxHe64VGyxKEO_hoV83QfGj0", GatewayIdentityProvider.CalculateHostId(jwk));
    }

    [Fact]
    public void TlsSpkiPinSurvivesCertificateRenewalWithSameKey()
    {
        using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using X509Certificate2 first = new CertificateRequest("CN=first", key, HashAlgorithmName.SHA256)
            .CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        using X509Certificate2 renewed = new CertificateRequest("CN=renewed", key, HashAlgorithmName.SHA256)
            .CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

        string firstPin = GatewayIdentityProvider.CalculateTlsSpkiSha256(first);
        Assert.Equal(firstPin, GatewayIdentityProvider.CalculateTlsSpkiSha256(renewed));
        Assert.NotEqual(first.GetCertHashString(HashAlgorithmName.SHA256), renewed.GetCertHashString(HashAlgorithmName.SHA256));
        Assert.Equal(44, firstPin.Length);
    }
}
