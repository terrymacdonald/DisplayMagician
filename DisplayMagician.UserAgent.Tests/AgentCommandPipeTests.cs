using System;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class AgentCommandPipeTests
{
    [Fact]
    public void CreateName_UsesTheVersionedCommandPipePrefixAndProcessId()
    {
        Assert.Equal($"{ControlProtocol.AgentCommandPipePrefix}1234", AgentCommandPipe.CreateName(1234));
    }

    [Fact]
    public void CreateName_RejectsInvalidProcessIds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AgentCommandPipe.CreateName(0));
    }
}
