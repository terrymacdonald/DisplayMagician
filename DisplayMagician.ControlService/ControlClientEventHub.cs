using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

public sealed class ControlClientEventHub
{
    private readonly ConcurrentDictionary<Guid, Subscription> _subscriptions = new ConcurrentDictionary<Guid, Subscription>();

    public ControlClientEventSubscription Subscribe(string userSid, int sessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userSid);
        Subscription subscription = new Subscription(Guid.NewGuid(), userSid, sessionId, this);
        _subscriptions.TryAdd(subscription.Id, subscription);
        return subscription;
    }

    public void Publish(string userSid, int sessionId, ControlClientEvent clientEvent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userSid);
        ArgumentNullException.ThrowIfNull(clientEvent);
        foreach (Subscription subscription in _subscriptions.Values)
        {
            if (string.Equals(subscription.UserSid, userSid, StringComparison.OrdinalIgnoreCase) &&
                (clientEvent.Scope == ControlClientEventScope.User || subscription.SessionId == sessionId))
            {
                if (!subscription.Events.Writer.TryWrite(clientEvent))
                {
                    // A client that cannot keep up must reconnect and receive a fresh
                    // authoritative snapshot rather than silently missing an event.
                    subscription.Events.Writer.TryComplete();
                }
            }
        }
    }

    private void Unsubscribe(Guid subscriptionId)
    {
        if (_subscriptions.TryRemove(subscriptionId, out Subscription? subscription))
        {
            subscription.Events.Writer.TryComplete();
        }
    }

    private sealed class Subscription : ControlClientEventSubscription
    {
        private readonly ControlClientEventHub _owner;

        public Subscription(Guid id, string userSid, int sessionId, ControlClientEventHub owner)
        {
            Id = id;
            UserSid = userSid;
            SessionId = sessionId;
            _owner = owner;
            Events = Channel.CreateBounded<ControlClientEvent>(new BoundedChannelOptions(32) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = false });
        }

        public Guid Id { get; }
        public string UserSid { get; }
        public int SessionId { get; }
        public Channel<ControlClientEvent> Events { get; }

        public override ChannelReader<ControlClientEvent> Reader => Events.Reader;

        public override void Dispose()
        {
            _owner.Unsubscribe(Id);
        }
    }
}

public abstract class ControlClientEventSubscription : IDisposable
{
    public abstract ChannelReader<ControlClientEvent> Reader { get; }
    public abstract void Dispose();
}
