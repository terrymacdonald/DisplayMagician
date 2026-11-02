using System;
using System.Runtime.InteropServices;

namespace DisplayMagician.UserAgent.Runtime.Windows;

/// <summary>Restores the caller's exact thread context without changing the process default.</summary>
internal sealed class ThreadDpiAwarenessScope : IDisposable
{
    private readonly Func<IntPtr, IntPtr> _setContext;
    private readonly IntPtr _previousContext;
    private bool _disposed;

    internal ThreadDpiAwarenessScope(DPI_AWARENESS_CONTEXT context, Func<IntPtr, IntPtr>? setContext = null)
    {
        _setContext = setContext ?? CCDImport.SetThreadDpiAwarenessContext;
        _previousContext = _setContext(new IntPtr((int)context));
        if (_previousContext == IntPtr.Zero)
            SharedLogger.logger.Warn("ThreadDpiAwarenessScope/ThreadDpiAwarenessScope: Could not set thread DPI context {0}; Windows error {1}. Continuing with the existing context.", context, Marshal.GetLastWin32Error());
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_previousContext != IntPtr.Zero && _setContext(_previousContext) == IntPtr.Zero)
            SharedLogger.logger.Warn("ThreadDpiAwarenessScope/Dispose: Could not restore thread DPI context {0}; Windows error {1}.", _previousContext, Marshal.GetLastWin32Error());
    }
}
