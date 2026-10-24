using System;
using System.IO;
using DisplayMagician.AppLibraries;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class WindowsScriptShellTests
{
    [Fact]
    public void CreateShortcut_RoundTripsThroughWindowsScriptHost()
    {
        string shortcutPath = Path.Combine(Path.GetTempPath(), $"DisplayMagician-{Guid.NewGuid():N}.lnk");
        string executablePath = Environment.ProcessPath ?? throw new InvalidOperationException("The test process has no executable path.");
        try
        {
            InstalledProgram.CreateShortcut(executablePath, "--displaymagician-test", string.Empty, shortcutPath);

            InstalledProgram shortcut = InstalledProgram.GetLnkShortcutData(shortcutPath);

            Assert.True(File.Exists(shortcutPath));
            Assert.Equal(executablePath, shortcut.Path, StringComparer.OrdinalIgnoreCase);
            Assert.Equal("--displaymagician-test", shortcut.Arguments);
        }
        finally
        {
            if (File.Exists(shortcutPath)) File.Delete(shortcutPath);
        }
    }
}
