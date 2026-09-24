using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using DisplayMagician.Contracts;

namespace DisplayMagician.Gateway;

/// <summary>Creates or opens the Gateway's separate machine P-256 host and TLS identities.</summary>
public sealed class GatewayIdentityProvider
{
    private const string HostKeyName = "DisplayMagician.Gateway.HostIdentity.v1";
    private const string TlsKeyName = "DisplayMagician.Gateway.Tls.v1";
    private const string TlsCertificateFriendlyName = "DisplayMagician Gateway TLS v1";
    private const string TlsCertificateSubject = "CN=DisplayMagician Gateway";

    public GatewayIdentity GetOrCreate()
    {
        using CngKey hostKey = OpenOrCreateKey(HostKeyName);
        string publicKeyJwk = CreatePublicJwk(hostKey);
        X509Certificate2 certificate = GetOrCreateTlsCertificate();
        string hostId = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(publicKeyJwk)));
        string certificateFingerprint = certificate.GetCertHashString(HashAlgorithmName.SHA256);
        return new GatewayIdentity(hostId, publicKeyJwk, certificateFingerprint, certificate);
    }

    private static X509Certificate2 GetOrCreateTlsCertificate()
    {
        using X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadWrite);
        X509Certificate2? existing = store.Certificates.Cast<X509Certificate2>().FirstOrDefault(certificate => certificate.FriendlyName == TlsCertificateFriendlyName && certificate.NotAfter.ToUniversalTime() > DateTime.UtcNow.AddDays(30) && certificate.HasPrivateKey);
        if (existing != null)
        {
            return existing;
        }

        using CngKey tlsKey = OpenOrCreateKey(TlsKeyName);
        using ECDsa tlsAlgorithm = new ECDsaCng(tlsKey);
        CertificateRequest request = new CertificateRequest(TlsCertificateSubject, tlsAlgorithm, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
        X509Certificate2 certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(5));
        certificate.FriendlyName = TlsCertificateFriendlyName;
        store.Add(certificate);
        return certificate;
    }

    private static CngKey OpenOrCreateKey(string keyName)
    {
        CngProvider provider = CngProvider.MicrosoftSoftwareKeyStorageProvider;
        CngKeyOpenOptions options = CngKeyOpenOptions.MachineKey;
        if (CngKey.Exists(keyName, provider, options))
        {
            return CngKey.Open(keyName, provider, options);
        }

        CngKeyCreationParameters parameters = new CngKeyCreationParameters
        {
            Provider = provider,
            KeyCreationOptions = CngKeyCreationOptions.MachineKey
        };
        return CngKey.Create(CngAlgorithm.ECDsaP256, keyName, parameters);
    }

    private static string CreatePublicJwk(CngKey key)
    {
        using ECDsa algorithm = new ECDsaCng(key);
        ECParameters parameters = algorithm.ExportParameters(false);
        return JsonSerializer.Serialize(new { kty = "EC", crv = "P-256", x = ToBase64Url(parameters.Q.X!), y = ToBase64Url(parameters.Q.Y!), alg = "ES256", use = "sig" });
    }

    private static string ToBase64Url(byte[] value) => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

/// <summary>In-memory Gateway identity; only public values are exposed to callers.</summary>
public sealed class GatewayIdentity
{
    public GatewayIdentity(string hostId, string hostIdentityPublicKeyJwk, string tlsCertificateSha256, X509Certificate2 tlsCertificate)
    {
        HostId = hostId;
        HostIdentityPublicKeyJwk = hostIdentityPublicKeyJwk;
        TlsCertificateSha256 = tlsCertificateSha256;
        TlsCertificate = tlsCertificate ?? throw new ArgumentNullException(nameof(tlsCertificate));
    }

    public string HostId { get; }
    public string HostIdentityPublicKeyJwk { get; }
    public string TlsCertificateSha256 { get; }
    public X509Certificate2 TlsCertificate { get; }

    public GatewayIdentityView ToView()
    {
        return new GatewayIdentityView { HostId = HostId, HostIdentityPublicKeyJwk = HostIdentityPublicKeyJwk, TlsCertificateSha256 = TlsCertificateSha256 };
    }
}
