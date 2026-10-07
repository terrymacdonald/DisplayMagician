using Xunit;

namespace DisplayMagicianConsole.Tests;

public sealed class ConsoleExitCodeTests
{
    [Fact]
    public void Main_ReturnsCommandExitCodeForInvalidDecision()
    {
        int result = Program.Main(["AnswerDecision", "not-a-guid", "Continue"]);

        Assert.Equal((int)Program.ERRORLEVEL.ERROR_EXCEPTION, result);
    }
}
