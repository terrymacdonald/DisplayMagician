using System;

namespace DisplayMagician.ControlService;

public static class ConsoleSessionLocator
{
    public static int GetActiveConsoleSessionId()
    {
        uint sessionId = WTSGetActiveConsoleSessionId();
        if (sessionId == uint.MaxValue)
        {
            throw new InvalidOperationException("There is no active physical console session.");
        }

        return checked((int)sessionId);
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern uint WTSGetActiveConsoleSessionId();
}
