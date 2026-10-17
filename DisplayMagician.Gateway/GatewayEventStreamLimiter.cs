using System;
using System.Collections.Generic;
using DisplayMagician.Contracts;

namespace DisplayMagician.Gateway;

/// <summary>Bounds long-lived event streams independently of short HTTP requests.</summary>
public sealed class GatewayEventStreamLimiter
{
    private readonly object _syncRoot = new object();
    private readonly Dictionary<string, int> _userCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _devices = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public IDisposable? TryEnter(GatewayAuthenticationResult authentication)
    {
        string user = authentication.OwnerUserSid;
        string device = user + "\n" + authentication.DeviceId;
        lock (_syncRoot)
        {
            _userCounts.TryGetValue(user, out int count);
            if (count >= 4 || !_devices.Add(device)) return null;
            _userCounts[user] = count + 1;
        }

        return new Lease(this, user, device);
    }

    private void Leave(string user, string device)
    {
        lock (_syncRoot)
        {
            _devices.Remove(device);
            int count = _userCounts[user] - 1;
            if (count == 0) _userCounts.Remove(user); else _userCounts[user] = count;
        }
    }

    private sealed class Lease : IDisposable
    {
        private GatewayEventStreamLimiter? _owner;
        private readonly string _user;
        private readonly string _device;

        public Lease(GatewayEventStreamLimiter owner, string user, string device)
        {
            _owner = owner;
            _user = user;
            _device = device;
        }

        public void Dispose()
        {
            GatewayEventStreamLimiter? owner = System.Threading.Interlocked.Exchange(ref _owner, null);
            owner?.Leave(_user, _device);
        }
    }
}
