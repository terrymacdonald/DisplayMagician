using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

public sealed class GatewayRequestAuthenticator
{
    private static readonly TimeSpan AllowedClockSkew = TimeSpan.FromMinutes(5);
    private readonly PairedClientRepository _pairedClients;
    private readonly ConcurrentDictionary<string, DateTime> _usedNonces = new ConcurrentDictionary<string, DateTime>(StringComparer.Ordinal);

    public GatewayRequestAuthenticator(PairedClientRepository pairedClients) => _pairedClients = pairedClients ?? throw new ArgumentNullException(nameof(pairedClients));

    public GatewayAuthenticationResult Authenticate(GatewayAuthenticationRequest request, DateTime utcNow)
    {
        if (request == null) return new GatewayAuthenticationResult { Message = "The signed request is invalid or expired." };
        SignedGatewayRequest? signedRequest = request.SignedRequest;
        if (signedRequest == null || string.IsNullOrWhiteSpace(signedRequest.DeviceId) || string.IsNullOrWhiteSpace(signedRequest.Nonce) || string.IsNullOrWhiteSpace(signedRequest.Signature) || Math.Abs((utcNow.ToUniversalTime() - signedRequest.TimestampUtc.ToUniversalTime()).TotalMinutes) > AllowedClockSkew.TotalMinutes)
            return new GatewayAuthenticationResult { Message = "The signed request is invalid or expired." };
        string nonceKey = signedRequest.DeviceId + "\n" + signedRequest.Nonce;
        foreach (var nonce in _usedNonces) if (nonce.Value <= utcNow) _usedNonces.TryRemove(nonce.Key, out _);
        if (!_usedNonces.TryAdd(nonceKey, utcNow.Add(AllowedClockSkew))) return new GatewayAuthenticationResult { Message = "The request nonce has already been used." };
        PairedClient? client = _pairedClients.FindActiveByDeviceId(signedRequest.DeviceId);
        if (client == null || !Verify(client.PublicKeyJwk, signedRequest, request.Method, request.Path, request.BodySha256)) return new GatewayAuthenticationResult { Message = "The request signature is invalid." };
        _pairedClients.RecordAuthentication(client.DeviceId, request.SourceIpAddress, utcNow);
        return new GatewayAuthenticationResult { IsAuthenticated = true, OwnerUserSid = client.OwnerUserSid, DeviceId = client.DeviceId, GrantedCapabilities = client.GrantedCapabilities, Message = "Authenticated." };
    }

    private static bool Verify(string publicKeyJwk, SignedGatewayRequest request, string method, string path, string bodySha256)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(publicKeyJwk);
            JsonElement root = document.RootElement;
            string? x = root.GetProperty("x").GetString();
            string? y = root.GetProperty("y").GetString();
            if (string.IsNullOrWhiteSpace(x) || string.IsNullOrWhiteSpace(y)) return false;
            using ECDsa key = ECDsa.Create(new ECParameters { Curve = ECCurve.NamedCurves.nistP256, Q = new ECPoint { X = FromBase64Url(x), Y = FromBase64Url(y) } });
            byte[] signature = FromBase64Url(request.Signature);
            byte[] payload = Encoding.UTF8.GetBytes(SignedGatewayRequest.CreateCanonicalPayload(method, path, bodySha256, request.TimestampUtc, request.Nonce));
            return signature.Length == 64 && key.VerifyData(payload, signature, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        }
        catch (Exception ex) when (ex is JsonException || ex is CryptographicException || ex is FormatException || ex is KeyNotFoundException)
        {
            return false;
        }
    }

    private static byte[] FromBase64Url(string value)
    {
        string padded = (value ?? string.Empty).Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }
}
