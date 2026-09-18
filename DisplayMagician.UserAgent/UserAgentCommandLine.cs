using System;

namespace DisplayMagician.UserAgent;

public enum UserAgentStartupAction
{
    Run = 0,
    RegisterOnce = 1,
    ApplyDisplayProfile = 2,
    AcquireDisplayControl = 3,
    MigrateUserData = 4
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

        if (args?.Length == 2 && string.Equals(args[0], "--apply-profile", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(args[1]))
        {
            return new UserAgentStartupRequest { Action = UserAgentStartupAction.ApplyDisplayProfile, ProfileId = args[1] };
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
