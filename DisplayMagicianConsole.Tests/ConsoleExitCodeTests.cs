using Xunit;

namespace DisplayMagicianConsole.Tests;

public sealed class ConsoleExitCodeTests
{
    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "Continue")]
    [InlineData("7fe7d61d-06c8-454e-af5b-846ea8da1cf1", "999")]
    [InlineData("7fe7d61d-06c8-454e-af5b-846ea8da1cf1", "Unknown")]
    public void InvalidDecisionsAreRejectedBeforeConnectingToTheService(string id, string choice)
    {
        Assert.Equal(Program.ERRORLEVEL.ERROR_EXCEPTION, Program.ResolveOperationDecision(id, choice));
    }

    [Fact]
    public void Main_ReturnsCommandExitCodeForInvalidDecision()
    {
        int result = Program.Main(["AnswerDecision", "not-a-guid", "Continue"]);

        Assert.Equal((int)Program.ERRORLEVEL.ERROR_EXCEPTION, result);
    }
}
