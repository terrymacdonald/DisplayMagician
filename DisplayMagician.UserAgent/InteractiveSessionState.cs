using System;
using System.Runtime.InteropServices;

namespace DisplayMagician.UserAgent;

public enum InteractiveSessionState
{
    Unlocked = 0,
    Locked = 1,
    Unknown = 2
}

public interface IInteractiveSessionStateProvider
{
    InteractiveSessionState GetState(int sessionId);
}

public sealed class WtsInteractiveSessionStateProvider : IInteractiveSessionStateProvider
{
    private const int WtsInfoExInformationClass = 24;
    private const int WtsSessionStateLock = 0;
    private const int WtsSessionStateUnlock = 1;

    public InteractiveSessionState GetState(int sessionId)
    {
        if (sessionId < 0 || !WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoExInformationClass, out IntPtr buffer, out int bytesReturned))
        {
            return InteractiveSessionState.Unknown;
        }

        try
        {
            if (bytesReturned < Marshal.SizeOf<WtsInfoEx>())
            {
                return InteractiveSessionState.Unknown;
            }

            WtsInfoEx info = Marshal.PtrToStructure<WtsInfoEx>(buffer);
            return info.SessionFlags switch
            {
                WtsSessionStateLock => InteractiveSessionState.Locked,
                WtsSessionStateUnlock => InteractiveSessionState.Unlocked,
                _ => InteractiveSessionState.Unknown
            };
        }
        catch (ArgumentException)
        {
            return InteractiveSessionState.Unknown;
        }
        finally
        {
            WTSFreeMemory(buffer);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WtsInfoEx
    {
        public int Level;
        public int SessionId;
        public int SessionState;
        public int SessionFlags;
    }

    [DllImport("wtsapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WTSQuerySessionInformation(IntPtr server, int sessionId, int informationClass, out IntPtr buffer, out int bytesReturned);

    [DllImport("wtsapi32.dll")]
    private static extern void WTSFreeMemory(IntPtr memory);
}

public static class InteractiveSessionPolicy
{
    public static bool CanStartShortcut(InteractiveSessionState state)
    {
        return state == InteractiveSessionState.Unlocked;
    }
}