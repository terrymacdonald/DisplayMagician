using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.Extensions.Hosting;
using NLog;

namespace DisplayMagician.Gateway;

/// <summary>
/// Shares one Control Service status poll per Windows user between the Gateway's SSE subscribers.
/// Each subscriber owns a bounded channel, so a slow remote client cannot delay other clients.
/// </summary>
public sealed class GatewayStatusFeed : IHostedService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly GatewayControlServiceClient _client;
    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private int _isStopping;

    public GatewayStatusFeed(GatewayControlServiceClient client)
    {
        _client = client;
    }

    public ChannelReader<RemoteUserStatus> Subscribe(
        GatewayAuthenticationResult authentication,
        CancellationToken cancellationToken)
    {
        if (Volatile.Read(ref _isStopping) != 0)
        {
            return CreateCompletedReader();
        }

        string ownerUserSid = authentication.OwnerUserSid;
        while (true)
        {
            Entry entry = _entries.GetOrAdd(
                ownerUserSid,
                ownerSid => new Entry(
                    authentication,
                    _client,
                    retiredEntry => RemoveEntry(ownerSid, retiredEntry)));

            if (entry.TrySubscribe(cancellationToken, out ChannelReader<RemoteUserStatus>? reader))
            {
                return reader!;
            }

            RemoveEntry(ownerUserSid, entry);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Interlocked.Exchange(ref _isStopping, 1);
        Entry[] entries = _entries.Values.ToArray();
        _entries.Clear();

        await Task.WhenAll(entries.Select(entry => entry.StopAsync())).ConfigureAwait(false);
    }

    private void RemoveEntry(string ownerUserSid, Entry expectedEntry)
    {
        ((ICollection<KeyValuePair<string, Entry>>)_entries).Remove(
            new KeyValuePair<string, Entry>(ownerUserSid, expectedEntry));
    }

    private static ChannelReader<RemoteUserStatus> CreateCompletedReader()
    {
        Channel<RemoteUserStatus> channel = Channel.CreateUnbounded<RemoteUserStatus>();
        channel.Writer.TryComplete();
        return channel.Reader;
    }

    private sealed class Entry
    {
        private readonly GatewayAuthenticationResult _authentication;
        private readonly GatewayControlServiceClient _client;
        private readonly Action<Entry> _removeWhenRetired;
        private readonly Dictionary<Guid, Subscriber> _subscribers = new();
        private readonly Dictionary<Guid, OperationStatus> _operations = new();
        private readonly object _subscriberLock = new();
        private readonly object _stateLock = new();
        private readonly object _pollingLock = new();
        private readonly CancellationTokenSource _stopCancellation = new();
        private int _isPolling;
        private bool _retired;
        private RemoteUserStatus? _latest;
        private string _decisionFingerprint = string.Empty;
        private Task? _pollingTask;

        public Entry(
            GatewayAuthenticationResult authentication,
            GatewayControlServiceClient client,
            Action<Entry> removeWhenRetired)
        {
            _authentication = authentication;
            _client = client;
            _removeWhenRetired = removeWhenRetired;
        }

        public bool TrySubscribe(
            CancellationToken cancellationToken,
            out ChannelReader<RemoteUserStatus>? reader)
        {
            Guid subscriberId = Guid.NewGuid();
            Subscriber subscriber = new(subscriberId);
            RemoteUserStatus? latest;

            lock (_subscriberLock)
            {
                if (_retired)
                {
                    reader = null;
                    return false;
                }

                _subscribers.Add(subscriberId, subscriber);
            }

            lock (_stateLock)
            {
                latest = _latest;
            }

            CancellationTokenRegistration registration = cancellationToken.Register(
                () => RemoveSubscriber(subscriberId));
            subscriber.SetCancellationRegistration(registration);

            if (latest != null && !subscriber.Channel.Writer.TryWrite(latest))
            {
                RemoveSubscriber(subscriberId);
            }

            EnsurePolling();
            reader = subscriber.Channel.Reader;
            return true;
        }

        public async Task StopAsync()
        {
            Subscriber[] subscribers;

            lock (_subscriberLock)
            {
                _retired = true;
                subscribers = _subscribers.Values.ToArray();
                _subscribers.Clear();
            }

            foreach (Subscriber subscriber in subscribers)
            {
                subscriber.Complete();
            }

            CancelPolling();

            Task? pollingTask;
            lock (_pollingLock)
            {
                pollingTask = _pollingTask;
            }

            if (pollingTask != null)
            {
                await pollingTask.ConfigureAwait(false);
            }

            _stopCancellation.Dispose();
        }

        private void EnsurePolling()
        {
            if (Interlocked.CompareExchange(ref _isPolling, 1, 0) == 0)
            {
                Task pollingTask = RunAsync();
                lock (_pollingLock)
                {
                    _pollingTask = pollingTask;
                }
            }
        }

        private async Task RunAsync()
        {
            DateTime? changedSinceUtc = null;
            int retrySeconds = 1;

            try
            {
                while (HasSubscribers() && !_stopCancellation.IsCancellationRequested)
                {
                    int delaySeconds;

                    try
                    {
                        RemoteUserStatus status = await _client
                            .GetRemoteUserStatusAsync(
                                _authentication,
                                changedSinceUtc,
                                _stopCancellation.Token)
                            .ConfigureAwait(false);
                        changedSinceUtc = status.NextChangedSinceUtc;

                        if (UpdateSnapshot(status, out RemoteUserStatus snapshot))
                        {
                            Broadcast(snapshot);
                        }

                        retrySeconds = 1;
                        delaySeconds = retrySeconds;
                    }
                    catch (OperationCanceledException) when (_stopCancellation.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(
                            ex,
                            "GatewayStatusFeed/RunAsync: Could not refresh the cached remote status for user {0}. Retrying in {1} seconds.",
                            _authentication.OwnerUserSid,
                            retrySeconds);
                        delaySeconds = retrySeconds;
                        retrySeconds = Math.Min(retrySeconds * 2, 60);
                    }

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), _stopCancellation.Token)
                            .ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (_stopCancellation.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _isPolling, 0);
                if (HasSubscribers() && !_stopCancellation.IsCancellationRequested)
                {
                    EnsurePolling();
                }
            }
        }

        private bool UpdateSnapshot(RemoteUserStatus status, out RemoteUserStatus snapshot)
        {
            string decisionFingerprint = string.Join(
                "|",
                status.PendingDecisions.Select(decision =>
                    $"{decision.PromptId:N}:{decision.IsResolved}:{decision.ResolvedChoice}:{decision.ResolvedUtc:O}"));

            lock (_stateLock)
            {
                bool changed = _latest == null ||
                    status.Operations.Length > 0 ||
                    !string.Equals(_decisionFingerprint, decisionFingerprint, StringComparison.Ordinal);

                foreach (OperationStatus operation in status.Operations)
                {
                    _operations[operation.OperationId] = operation;
                }

                _decisionFingerprint = decisionFingerprint;
                RemoteUserStatus latest = new RemoteUserStatus
                {
                    Operations = _operations.Values
                        .OrderBy(operation => operation.UpdatedUtc)
                        .ToArray(),
                    PendingDecisions = status.PendingDecisions,
                    NextChangedSinceUtc = status.NextChangedSinceUtc
                };

                _latest = latest;
                snapshot = latest;
                return changed;
            }
        }

        private void Broadcast(RemoteUserStatus status)
        {
            Subscriber[] subscribers;
            lock (_subscriberLock)
            {
                subscribers = _subscribers.Values.ToArray();
            }

            foreach (Subscriber subscriber in subscribers)
            {
                if (!subscriber.Channel.Writer.TryWrite(status))
                {
                    RemoveSubscriber(subscriber.Id);
                }
            }
        }

        private void RemoveSubscriber(Guid subscriberId)
        {
            Subscriber? subscriber = null;
            bool retired = false;

            lock (_subscriberLock)
            {
                if (_subscribers.Remove(subscriberId, out subscriber) &&
                    _subscribers.Count == 0 &&
                    !_retired)
                {
                    _retired = true;
                    retired = true;
                }
            }

            subscriber?.Complete();

            if (retired)
            {
                CancelPolling();
                _removeWhenRetired(this);
            }
        }

        private void CancelPolling()
        {
            try
            {
                _stopCancellation.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // A cancellation callback can race with service shutdown after the poller has stopped.
            }
        }

        private bool HasSubscribers()
        {
            lock (_subscriberLock)
            {
                return _subscribers.Count > 0;
            }
        }

        private sealed class Subscriber
        {
            private readonly object _lock = new();
            private CancellationTokenRegistration _cancellationRegistration;
            private bool _isComplete;

            public Guid Id { get; }

            public Channel<RemoteUserStatus> Channel { get; } = System.Threading.Channels.Channel.CreateBounded<RemoteUserStatus>(
                new BoundedChannelOptions(8)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = false
                });

            public Subscriber(Guid id)
            {
                Id = id;
            }

            public void SetCancellationRegistration(CancellationTokenRegistration cancellationRegistration)
            {
                bool disposeRegistration;
                lock (_lock)
                {
                    _cancellationRegistration = cancellationRegistration;
                    disposeRegistration = _isComplete;
                }

                if (disposeRegistration)
                {
                    cancellationRegistration.Dispose();
                }
            }

            public void Complete()
            {
                CancellationTokenRegistration cancellationRegistration;
                lock (_lock)
                {
                    if (_isComplete)
                    {
                        return;
                    }

                    _isComplete = true;
                    cancellationRegistration = _cancellationRegistration;
                }

                Channel.Writer.TryComplete();
                cancellationRegistration.Dispose();
            }
        }
    }
}
