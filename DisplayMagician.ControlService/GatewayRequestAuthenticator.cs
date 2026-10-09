using System;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

/// <summary>Validates opaque paired-device credentials against machine-owned hashes.</summary>
public sealed class GatewayRequestAuthenticator
{
    private readonly PairedClientRepository _pairedClients;

    public GatewayRequestAuthenticator(PairedClientRepository pairedClients) => _pairedClients = pairedClients ?? throw new ArgumentNullException(nameof(pairedClients));

    public GatewayAuthenticationResult Authenticate(GatewayAuthenticationRequest? request, DateTime utcNow)
    {
        PairedClient? client = request == null ? null : _pairedClients.FindActiveByCredential(request.BearerCredential);
        if (client == null)
        {
            return new GatewayAuthenticationResult { Message = "The paired-device credential is invalid." };
        }

        _pairedClients.RecordAuthentication(client.DeviceId, request?.SourceIpAddress ?? string.Empty, utcNow);
        return new GatewayAuthenticationResult
        {
            IsAuthenticated = true,
            OwnerUserSid = client.OwnerUserSid,
            DeviceId = client.DeviceId,
            PreferredSessionId = client.PreferredSessionId,
            GrantedCapabilities = client.GrantedCapabilities,
            Message = "Authenticated."
        };
    }
}
