using System.Reflection;

namespace DisplayMagician.UserAgent;

public static class AgentBuildVersion
{
    public static string Current { get; } = typeof(AgentBuildVersion).Assembly
        .GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
        ?? typeof(AgentBuildVersion).Assembly.GetName().Version?.ToString()
        ?? "0.0.0.0";
}
