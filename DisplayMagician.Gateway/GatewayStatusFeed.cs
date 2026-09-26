using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagician.Gateway;

public sealed class GatewayStatusFeed
{
    private readonly GatewayControlServiceClient _client;
    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);
    public GatewayStatusFeed(GatewayControlServiceClient client) => _client = client;
    public ChannelReader<RemoteUserStatus> Subscribe(GatewayAuthenticationResult authentication, CancellationToken cancellationToken) => _entries.GetOrAdd(authentication.OwnerUserSid, _ => new Entry(authentication, _client)).Subscribe(cancellationToken);

    private sealed class Entry
    {
        private readonly GatewayAuthenticationResult _authentication;
        private readonly GatewayControlServiceClient _client;
        private readonly ConcurrentDictionary<Guid, Channel<RemoteUserStatus>> _subscribers = new();
        private int _started;
        private RemoteUserStatus? _latest;
        public Entry(GatewayAuthenticationResult authentication, GatewayControlServiceClient client) { _authentication = authentication; _client = client; }
        public ChannelReader<RemoteUserStatus> Subscribe(CancellationToken cancellationToken)
        {
            Guid id = Guid.NewGuid();
            Channel<RemoteUserStatus> channel = Channel.CreateBounded<RemoteUserStatus>(new BoundedChannelOptions(8) { SingleReader = true, SingleWriter = true });
            _subscribers.TryAdd(id, channel);
            if (_latest != null) channel.Writer.TryWrite(_latest);
            cancellationToken.Register(() => { if (_subscribers.TryRemove(id, out Channel<RemoteUserStatus>? removed)) removed.Writer.TryComplete(); });
            if (Interlocked.Exchange(ref _started, 1) == 0) _ = RunAsync();
            return channel.Reader;
        }
        private async Task RunAsync()
        {
            DateTime? cursor = null;
            while (true)
            {
                try
                {
                    RemoteUserStatus status = await _client.GetRemoteUserStatusAsync(_authentication, cursor, CancellationToken.None).ConfigureAwait(false);
                    cursor = status.NextChangedSinceUtc;
                    if (status.Operations.Length > 0 || _latest == null)
                    {
                        _latest = status;
                        foreach (var subscriber in _subscribers)
                            if (!subscriber.Value.Writer.TryWrite(status) && _subscribers.TryRemove(subscriber.Key, out Channel<RemoteUserStatus>? removed)) removed.Writer.TryComplete();
                    }
                }
                catch { }
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }
    }
}
