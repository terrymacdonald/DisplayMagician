using System;

namespace DisplayMagician.UserAgent;

public enum UserAgentStartupAction
{
    Run = 0,
    RegisterOnce = 1,
    AcquireDisplayControl = 2,
    MigrateUserData = 3
}

public sealed class UserAgentStartupRequest
{
    public UserAgentStartupAction Action { get; init; }

    public string? ProfileId { get; init; }
}

public static class UserAgentCommandLine
{
    public static UserAgentStartupRequest Parse(string[]? args)
    {
        if (args?.Length == 1 && string.Equals(args[0], "--once", StringComparison.Ordinal))
        {
            return new UserAgentStartupRequest { Action = UserAgentStartupAction.RegisterOnce };
        }

        if (args?.Length == 1 && string.Equals(args[0], "--acquire-display-control", StringComparison.Ordinal))
        {
            return new UserAgentStartupRequest { Action = UserAgentStartupAction.AcquireDisplayControl };
        }

        if (args?.Length == 1 && string.Equals(args[0], "--migrate-user-data", StringComparison.Ordinal))
        {
            return new UserAgentStartupRequest { Action = UserAgentStartupAction.MigrateUserData };
        }

        return new UserAgentStartupRequest { Action = UserAgentStartupAction.Run };
    }
}
