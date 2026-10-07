using System;
using System.Diagnostics;
using System.Security.Principal;
using DisplayMagician.Contracts;

namespace DisplayMagician.UserAgent;

public static class AgentIdentity
{
    public static AgentRegistration CreateRegistration(string version, string startupMode)
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();

        return new AgentRegistration
        {
            UserSid = identity.User?.Value ?? throw new InvalidOperationException("The current Windows user has no SID."),
            SessionId = Process.GetCurrentProcess().SessionId,
            ProcessId = Environment.ProcessId,
            Version = version,
            StartupMode = startupMode,
            OperationState = AgentOperationState.Idle,
            CommandPipeName = AgentCommandPipe.CreateName(Environment.ProcessId)
        };
    }
}
