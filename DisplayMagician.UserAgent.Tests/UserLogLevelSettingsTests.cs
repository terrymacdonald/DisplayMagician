using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class UserLogLevelSettingsTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-UserLogging-{Guid.NewGuid():N}");
    private string SettingsPath => Path.Combine(_root, "Settings.json");

    [Theory]
    [InlineData("Trace")]
    [InlineData("Debug")]
    [InlineData("Info")]
    [InlineData("Warn")]
    [InlineData("Error")]
    [InlineData("Fatal")]
    public void SavedLevelIsAppliedAtStartupAndAfterRestart(string level)
    {
        Save(level, Encoding.Unicode);
        List<string> applied = new List<string>();
        new UserLogLevelSettings(SettingsPath, applied.Add, "Info").Refresh();
        new UserLogLevelSettings(SettingsPath, applied.Add).Refresh();
        Assert.Equal(new[] { level, level }, applied);
    }

    [Fact]
    public void ChangedLevelIsAppliedToRunningAgentWithoutRepeatedReconfiguration()
    {
        Save("Debug", Encoding.UTF8);
        List<string> applied = new List<string>();
        UserLogLevelSettings settings = new UserLogLevelSettings(SettingsPath, applied.Add);
        settings.Refresh();
        settings.Refresh();
        Save("Error", Encoding.Unicode);
        settings.Refresh();
        settings.Refresh();
        Assert.Equal(new[] { "Debug", "Error" }, applied);
    }

    [Fact]
    public void StartupOverrideIsRetainedUntilSavedLevelChanges()
    {
        Save("Warn", Encoding.Unicode);
        List<string> applied = new List<string>();
        UserLogLevelSettings settings = new UserLogLevelSettings(SettingsPath, applied.Add, "Trace");
        settings.Refresh();
        Save("Warn", Encoding.UTF8);
        settings.Refresh();
        Save("Error", Encoding.Unicode);
        settings.Refresh();
        Assert.Equal(new[] { "Trace", "Error" }, applied);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"Settings\":null}")]
    [InlineData("{\"Settings\":{\"LogLevel\":5}}")]
    [InlineData("{\"Settings\":{\"LogLevel\":\"Off\"}}")]
    [InlineData("{\"Settings\":{\"LogLevel\":\"unknown\"}}")]
    public void InvalidSettingsPreserveCurrentLevelAndRecoverOnValidSave(string invalidJson)
    {
        Save("Warn", Encoding.Unicode);
        List<string> applied = new List<string>();
        UserLogLevelSettings settings = new UserLogLevelSettings(SettingsPath, applied.Add);
        settings.Refresh();
        File.WriteAllText(SettingsPath, invalidJson);
        settings.Refresh();
        Assert.Equal(new[] { "Warn" }, applied);
        Assert.Equal(invalidJson, File.ReadAllText(SettingsPath));
        Save("Debug", Encoding.Unicode);
        settings.Refresh();
        Assert.Equal(new[] { "Warn", "Debug" }, applied);
    }

    [Fact]
    public void MissingSettingsAndLockedFileDoNotPreventStartupOrLaterRefresh()
    {
        List<string> applied = new List<string>();
        UserLogLevelSettings settings = new UserLogLevelSettings(SettingsPath, applied.Add);
        settings.Refresh();
        Save("Warn", Encoding.Unicode);
        using (FileStream locked = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            settings.Refresh();
            Assert.Empty(applied);
        }
        settings.Refresh();
        Assert.Equal(new[] { "Warn" }, applied);
    }

    [Fact]
    public void MissingLogLevelUsesExistingDesktopDefaultWithoutLoadingTypeMetadata()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(SettingsPath, "{\"$type\":\"not-a-real-type\",\"Settings\":{\"$type\":\"not-a-real-type\"}}");
        List<string> applied = new List<string>();
        new UserLogLevelSettings(SettingsPath, applied.Add).Refresh();
        Assert.Equal(new[] { "Warn" }, applied);
    }

    private void Save(string level, Encoding encoding)
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(SettingsPath, $"{{\"SettingsFileVersion\":\"7\",\"Settings\":{{\"LogLevel\":\"{level}\"}}}}", encoding);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}
