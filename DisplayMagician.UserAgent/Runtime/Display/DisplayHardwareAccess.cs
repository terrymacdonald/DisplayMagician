using System;
using System.Threading;

namespace DisplayMagician.UserAgent.Runtime;

/// <summary>Serializes synchronous display hardware phases within this UserAgent.</summary>
internal static class DisplayHardwareAccess
{
    // Repository operations that capture or patch native configuration enter here
    // before taking their metadata lock. Pure metadata/list reads need no hardware access.
    internal static object SyncRoot { get; } = new object();

    internal static IDisposable Enter(CancellationToken cancellationToken = default)
    {
        while (!Monitor.TryEnter(SyncRoot, 50)) cancellationToken.ThrowIfCancellationRequested();
        if (cancellationToken.IsCancellationRequested)
        {
            Monitor.Exit(SyncRoot);
            cancellationToken.ThrowIfCancellationRequested();
        }
        return new AccessScope();
    }

    private sealed class AccessScope : IDisposable
    {
        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            // All hardware phases are synchronous. Do not hold this scope across await.
            Monitor.Exit(SyncRoot);
            _disposed = true;
        }
    }
}
