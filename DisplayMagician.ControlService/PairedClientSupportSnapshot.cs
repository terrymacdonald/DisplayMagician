using System;
using System.Linq;
using System.Text.Json;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

/// <summary>Uses an explicit allowlist so new paired-client secrets never enter support bundles by default.</summary>
public static class PairedClientSupportSnapshot
{
    public static string Serialize(PairedClient[] clients)
    {
        ArgumentNullException.ThrowIfNull(clients);
        return JsonSerializer.Serialize(clients.Select(client => new
        {
            client.DeviceId,
            client.OwnerUserSid,
            client.DisplayName,
            client.ClientType,
            client.LastKnownIpAddress,
            client.GrantedCapabilities,
            client.PairedUtc,
            client.LastAuthenticatedUtc,
            client.RevokedUtc
        }).ToArray());
    }
}
