using System;

namespace DisplayMagician.UserAgent;

internal static class WindowsScriptShell
{
    internal static dynamic Create()
    {
        Type shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new PlatformNotSupportedException("Windows Script Host is unavailable.");
        return Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException("Windows Script Host could not be started.");
    }
}
