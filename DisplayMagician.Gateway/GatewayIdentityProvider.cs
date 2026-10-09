using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using DisplayMagician.Contracts;

namespace DisplayMagician.Gateway;

/// <summary>Creates or opens the Gateway's separate machine P-256 host and TLS identities.</summary>
public sealed class GatewayIdentityProvider
{
    private const string HostKeyName = "DisplayMagician.Gateway.HostIdentity.v1";
    private const string TlsKeyName = "DisplayMagician.Gateway.Tls.v1";
    private const string TlsCertificateFriendlyName = "DisplayMagician Gateway TLS v1";
    private const string TlsCertificateSubject = "CN=DisplayMagician Gateway";

    public GatewayIdentity GetOrCreate(GatewaySettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        using CngKey hostKey = OpenOrCreateKey(HostKeyName);
        string publicKeyJwk = CreatePublicJwk(hostKey);
        X509Certificate2 certificate = GetOrCreateTlsCertificate(settings);
        string hostId = CalculateHostId(publicKeyJwk);
        return new GatewayIdentity(hostId, publicKeyJwk, certificate);
    }

    internal static string CalculateHostId(string publicKeyJwk)
    {
        // RFC 7638 hashes only the required members, in lexicographic member-name order.
        using JsonDocument publicKey = JsonDocument.Parse(publicKeyJwk);
        JsonElement root = publicKey.RootElement;
        string canonicalJwk = JsonSerializer.Serialize(new
        {
            crv = root.GetProperty("crv").GetString(),
            kty = root.GetProperty("kty").GetString(),
            x = root.GetProperty("x").GetString(),
            y = root.GetProperty("y").GetString()
        });
        return ToBase64Url(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalJwk)));
    }

    internal static string CalculateTlsSpkiSha256(X509Certificate2 certificate)
    {
        using ECDsa publicKey = certificate.GetECDsaPublicKey() ?? throw new InvalidOperationException("The Gateway TLS certificate does not have an ECDSA public key.");
        return Convert.ToBase64String(SHA256.HashData(publicKey.ExportSubjectPublicKeyInfo()));
    }

    private static X509Certificate2 GetOrCreateTlsCertificate(GatewaySettings settings)
    {
        ApplyApprovedTlsKeyReplacement();
        string[] certificateHosts = new[] { "localhost", "127.0.0.1", "::1", settings.LanAdvertisedHost, settings.RemoteHost }
            .Where(host => !string.IsNullOrWhiteSpace(host))
            .Select(host => host.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        foreach (string host in certificateHosts)
        {
            if (!IPAddress.TryParse(host, out _) && Uri.CheckHostName(host) != UriHostNameType.Dns)
            {
                throw new InvalidOperationException($"Gateway advertised host '{host}' is not a valid DNS name or IP address.");
            }
        }

        using X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);
        string namedTlsPin = string.Empty;
        if (CngKey.Exists(TlsKeyName, CngProvider.MicrosoftSoftwareKeyStorageProvider))
        {
            using CngKey namedKey = CngKey.Open(TlsKeyName, CngProvider.MicrosoftSoftwareKeyStorageProvider);
            using ECDsa namedAlgorithm = new ECDsaCng(namedKey);
            namedTlsPin = Convert.ToBase64String(SHA256.HashData(namedAlgorithm.ExportSubjectPublicKeyInfo()));
        }
        X509Certificate2? existing = store.Certificates.Cast<X509Certificate2>().FirstOrDefault(certificate =>
            certificate.FriendlyName == TlsCertificateFriendlyName &&
            CalculateTlsSpkiSha256(certificate) == namedTlsPin &&
            certificate.NotAfter.ToUniversalTime() > DateTime.UtcNow.AddDays(30) &&
            certificate.HasPrivateKey &&
            certificateHosts.All(host => certificate.MatchesHostname(host, allowWildcards: false, allowCommonName: false)));
        if (existing != null)
        {
            return existing;
        }

        if (!CngKey.Exists(TlsKeyName, CngProvider.MicrosoftSoftwareKeyStorageProvider) &&
            store.Certificates.Cast<X509Certificate2>().Any(certificate => certificate.FriendlyName == TlsCertificateFriendlyName))
        {
            throw new InvalidOperationException("The Gateway TLS private key is missing; local approval and re-pairing are required before replacing it.");
        }

        using CngKey tlsKey = OpenOrCreateKey(TlsKeyName);
        using ECDsa tlsAlgorithm = new ECDsaCng(tlsKey);
        CertificateRequest request = new CertificateRequest(TlsCertificateSubject, tlsAlgorithm, HashAlgorithmName.SHA256);
        SubjectAlternativeNameBuilder subjectAlternativeNames = new SubjectAlternativeNameBuilder();
        foreach (string host in certificateHosts)
        {
            if (IPAddress.TryParse(host, out IPAddress? address))
            {
                subjectAlternativeNames.AddIpAddress(address);
            }
            else
            {
                subjectAlternativeNames.AddDnsName(host);
            }
        }

        request.CertificateExtensions.Add(subjectAlternativeNames.Build());
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        OidCollection enhancedKeyUsages = new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") };
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(enhancedKeyUsages, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
        X509Certificate2 certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(5));
        certificate.FriendlyName = TlsCertificateFriendlyName;
        store.Add(certificate);
        return certificate;
    }

    private static void ApplyApprovedTlsKeyReplacement()
    {
        string approvalPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician", "Machine", "GatewayTlsResetApproval.json");
        if (!System.IO.File.Exists(approvalPath)) return;

        using JsonDocument approval = JsonDocument.Parse(System.IO.File.ReadAllText(approvalPath));
        string previousPin = approval.RootElement.GetProperty("PreviousTlsSpkiSha256").GetString() ?? string.Empty;
        DateTime approvedUtc = approval.RootElement.GetProperty("ApprovedUtc").GetDateTime().ToUniversalTime();
        if (string.IsNullOrWhiteSpace(previousPin) || approvedUtc > DateTime.UtcNow || approvedUtc.AddMinutes(10) < DateTime.UtcNow) return;

        CngProvider provider = CngProvider.MicrosoftSoftwareKeyStorageProvider;
        if (CngKey.Exists(TlsKeyName, provider))
        {
            using CngKey existingKey = CngKey.Open(TlsKeyName, provider);
            using ECDsa algorithm = new ECDsaCng(existingKey);
            string currentPin = Convert.ToBase64String(SHA256.HashData(algorithm.ExportSubjectPublicKeyInfo()));
            if (!string.Equals(currentPin, previousPin, StringComparison.Ordinal)) return;
        }

        using X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);
        foreach (X509Certificate2 certificate in store.Certificates.Cast<X509Certificate2>().Where(certificate => certificate.FriendlyName == TlsCertificateFriendlyName && CalculateTlsSpkiSha256(certificate) == previousPin).ToArray())
        {
            store.Remove(certificate);
        }

        if (CngKey.Exists(TlsKeyName, provider))
        {
            using CngKey existingKey = CngKey.Open(TlsKeyName, provider);
            existingKey.Delete();
        }
    }

    private static CngKey OpenOrCreateKey(string keyName)
    {
        CngProvider provider = CngProvider.MicrosoftSoftwareKeyStorageProvider;

        if (CngKey.Exists(keyName, provider))
        {
            return CngKey.Open(keyName, provider);
        }

        CngKeyCreationParameters parameters = new CngKeyCreationParameters
        {
            Provider = provider
        };

        return CngKey.Create(
            CngAlgorithm.ECDsaP256,
            keyName,
            parameters);
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
    private X509Certificate2 _tlsCertificate;

    public GatewayIdentity(string hostId, string hostIdentityPublicKeyJwk, X509Certificate2 tlsCertificate)
    {
        HostId = hostId;
        HostIdentityPublicKeyJwk = hostIdentityPublicKeyJwk;
        _tlsCertificate = tlsCertificate ?? throw new ArgumentNullException(nameof(tlsCertificate));
    }

    public string HostId { get; }
    public string HostIdentityPublicKeyJwk { get; }
    public string TlsSpkiSha256 => GatewayIdentityProvider.CalculateTlsSpkiSha256(TlsCertificate);
    public X509Certificate2 TlsCertificate => Volatile.Read(ref _tlsCertificate);

    public void ReplaceCertificate(X509Certificate2 certificate)
    {
        ArgumentNullException.ThrowIfNull(certificate);
        Interlocked.Exchange(ref _tlsCertificate, certificate);
    }

    public GatewayIdentityView ToView(int port)
    {
        return new GatewayIdentityView { Port = port, HostId = HostId, HostIdentityPublicKeyJwk = HostIdentityPublicKeyJwk, TlsSpkiSha256 = TlsSpkiSha256 };
    }
}
