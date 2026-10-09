using System.Threading;

namespace DisplayMagician.Gateway;

/// <summary>Prevents remote work until ControlService has accepted the current Gateway identity.</summary>
public sealed class GatewayRegistrationState
{
    private int _registered;

    public bool IsRegistered => Volatile.Read(ref _registered) == 1;

    public void SetRegistered(bool registered) => Volatile.Write(ref _registered, registered ? 1 : 0);
}
