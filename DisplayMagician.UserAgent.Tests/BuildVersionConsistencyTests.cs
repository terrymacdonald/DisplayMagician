using System.Reflection;
using DisplayMagician.Contracts;
using DisplayMagician.ControlService;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class BuildVersionConsistencyTests
{
    [Fact]
    public void ShippedV4Components_UseTheSameGeneratedFileVersion()
    {
        string contractsVersion = GetAssemblyFileVersion(typeof(ControlProtocol).Assembly);
        string serviceVersion = GetAssemblyFileVersion(typeof(ControlStateCoordinator).Assembly);
        string agentVersion = GetAssemblyFileVersion(typeof(AgentBuildVersion).Assembly);

        Assert.Equal(contractsVersion, serviceVersion);
        Assert.Equal(contractsVersion, agentVersion);
    }

    private static string GetAssemblyFileVersion(Assembly assembly)
    {
        return assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()!.Version;
    }
}
