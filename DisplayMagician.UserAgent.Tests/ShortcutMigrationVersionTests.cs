using System;
using System.IO;
using System.Text;
using System.Text.Json;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ShortcutMigrationVersionTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-MigrationVersions", Guid.NewGuid().ToString("N"));
    private string DirectoryPath => Path.Combine(_root, "Shortcuts");
    private string FilePath => Path.Combine(DirectoryPath, "Shortcuts.json");

    [Theory]
    [InlineData("\"0\"")]
    [InlineData("\"-1\"")]
    [InlineData("\"1.5\"")]
    [InlineData("\"unknown\"")]
    [InlineData("\"2147483648\"")]
    [InlineData("1")]
    [InlineData("1.5")]
    [InlineData("true")]
    [InlineData("[]")]
    [InlineData("{}")]
    public void InvalidVersionCannotTriggerLegacyCleanupOrRewriteTheOriginalOrItsBackup(string versionJson)
    {
        Directory.CreateDirectory(DirectoryPath);
        string json = "{\"ShortcutFileVersion\":" + versionJson + ",\"Shortcuts\":[{\"UUID\":\"test\",\"Application\":{\"keep\":true},\"Custom\":\"original\"}]}";
        File.WriteAllText(FilePath, json, Encoding.Unicode);
        File.WriteAllText(FilePath + ".bak", "existing backup", Encoding.Unicode);
        byte[] original = File.ReadAllBytes(FilePath);
        byte[] backup = File.ReadAllBytes(FilePath + ".bak");
        Assert.Throws<InvalidDataException>(() => new ShortcutStore(_root));
        Assert.Equal(original, File.ReadAllBytes(FilePath));
        Assert.Equal(backup, File.ReadAllBytes(FilePath + ".bak"));
        Assert.Equal(2, Directory.GetFiles(DirectoryPath).Length);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    [InlineData("4")]
    [InlineData("5")]
    [InlineData("6")]
    [InlineData("7")]
    [InlineData("8")]
    [InlineData("9")]
    public void SupportedOlderVersionsMigrateToTheCurrentVersionOnceAndPreserveExistingChoices(string version)
    {
        Directory.CreateDirectory(DirectoryPath);
        string json = "{\"ShortcutFileVersion\":\"" + version + "\",\"Custom\":\"keep\",\"Shortcuts\":[{\"UUID\":\"test\",\"MonitorLaunchedExecutable\":false,\"StopPrograms\":[{\"RestartAfterwards\":true,\"WaitAfterStopSecs\":17}]}]}";
        File.WriteAllText(FilePath, json, Encoding.Unicode);
        byte[] original = File.ReadAllBytes(FilePath);
        ShortcutStore store = new(_root);
        using JsonDocument migrated = JsonDocument.Parse(store.GetSnapshot().Json);
        Assert.Equal("10", migrated.RootElement.GetProperty("ShortcutFileVersion").GetString());
        Assert.Equal("keep", migrated.RootElement.GetProperty("Custom").GetString());
        var shortcut = Assert.Single(store.GetShortcutDefinitions());
        Assert.False(shortcut.MonitorLaunchedExecutable);
        var program = Assert.Single(shortcut.StopPrograms);
        Assert.False(program.StopAllInstances);
        Assert.True(program.RestartAfterwards);
        Assert.Equal(17, program.WaitAfterStopSecs);
        Assert.Equal(original, File.ReadAllBytes(Assert.Single(Directory.GetFiles(DirectoryPath, "Shortcuts.json.v" + version + ".*.bak"))));
        byte[] saved = File.ReadAllBytes(FilePath);
        _ = new ShortcutStore(_root);
        Assert.Equal(saved, File.ReadAllBytes(FilePath));
        Assert.Single(Directory.GetFiles(DirectoryPath, "Shortcuts.json.v*.bak"));
    }

    [Fact]
    public void CurrentVersionDoesNotRewriteTheFileOrCreateMigrationBackups()
    {
        Directory.CreateDirectory(DirectoryPath);
        File.WriteAllText(FilePath, "{\"ShortcutFileVersion\":\"10\",\"Custom\":\"keep\",\"Shortcuts\":[{\"UUID\":\"test\",\"StopPrograms\":[{\"StopAllInstances\":true}]}]}", Encoding.Unicode);
        byte[] original = File.ReadAllBytes(FilePath);
        ShortcutStore store = new(_root);
        Assert.True(Assert.Single(Assert.Single(store.GetShortcutDefinitions()).StopPrograms).StopAllInstances);
        Assert.Equal(original, File.ReadAllBytes(FilePath));
        Assert.Single(Directory.GetFiles(DirectoryPath));
    }

    [Theory]
    [InlineData("11")]
    [InlineData("99")]
    public void NewerVersionsAreRejectedBeforeEitherMigrationChangesFiles(string version)
    {
        Directory.CreateDirectory(DirectoryPath);
        File.WriteAllText(FilePath, "{\"ShortcutFileVersion\":\"" + version + "\",\"Shortcuts\":[{\"UUID\":\"future\",\"Application\":{\"Future\":true},\"MonitorExecutablePath\":false}]}", Encoding.Unicode);
        File.WriteAllText(FilePath + ".bak", "existing valid backup", Encoding.Unicode);
        byte[] original = File.ReadAllBytes(FilePath);
        byte[] backup = File.ReadAllBytes(FilePath + ".bak");
        Assert.Throws<InvalidDataException>(() => new ShortcutStore(_root));
        Assert.Equal(original, File.ReadAllBytes(FilePath));
        Assert.Equal(backup, File.ReadAllBytes(FilePath + ".bak"));
        Assert.Equal(2, Directory.GetFiles(DirectoryPath).Length);
    }

    [Fact]
    public void LiveStoreCannotReadOrOverwriteANewerRepositoryOrDowngradeItsWindowSnapshot()
    {
        Directory.CreateDirectory(DirectoryPath);
        ShortcutStore store = new(_root);
        const string future = "{\"ShortcutFileVersion\":\"11\",\"Shortcuts\":[{\"UUID\":\"future\",\"Custom\":\"preserve\"}]}";
        File.WriteAllText(FilePath, future, Encoding.Unicode);
        byte[] original = File.ReadAllBytes(FilePath);
        Assert.Throws<JsonException>(() => store.GetSnapshot());
        Assert.Throws<JsonException>(() => store.GetShortcutDefinitions(future));
        Assert.Throws<JsonException>(() => store.ReplaceWindowPlacements("future", Array.Empty<DisplayMagician.ConfigurationDefinitions.ShortcutWindowPlacementDefinition>()));
        Assert.Throws<JsonException>(() => store.Commit(new DisplayMagician.Contracts.RepositoryCommitRequest
        {
            Repository = DisplayMagician.Contracts.RepositoryKind.Shortcuts, Json = "{\"Shortcuts\":[]}"
        }));
        Assert.Equal(original, File.ReadAllBytes(FilePath));
        Assert.Single(Directory.GetFiles(DirectoryPath));
    }

    [Fact]
    public void ANewerClientCannotCommitAnUnsupportedVersionIntoTheCurrentRepository()
    {
        ShortcutStore store = new(_root);
        Assert.Throws<JsonException>(() => store.Commit(new DisplayMagician.Contracts.RepositoryCommitRequest
        {
            Repository = DisplayMagician.Contracts.RepositoryKind.Shortcuts,
            Json = "{\"ShortcutFileVersion\":\"11\",\"Shortcuts\":[{\"UUID\":\"future\"}]}"
        }));
        Assert.False(File.Exists(FilePath));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("null")]
    [InlineData("\"\"")]
    public void UnversionedLegacyInputsRemainSupported(string? versionJson)
    {
        Directory.CreateDirectory(DirectoryPath);
        string prefix = versionJson == null ? "" : "\"ShortcutFileVersion\":" + versionJson + ",";
        File.WriteAllText(FilePath, "{" + prefix + "\"Shortcuts\":[{\"UUID\":\"legacy\"}]}", Encoding.Unicode);
        byte[] original = File.ReadAllBytes(FilePath);
        ShortcutStore store = new(_root);
        Assert.Equal("legacy", Assert.Single(store.GetShortcutDefinitions()).Id);
        using JsonDocument document = JsonDocument.Parse(store.GetSnapshot().Json);
        Assert.Equal("10", document.RootElement.GetProperty("ShortcutFileVersion").GetString());
        Assert.Equal(original, File.ReadAllBytes(Assert.Single(Directory.GetFiles(DirectoryPath, "Shortcuts.json.vunknown.*.bak"))));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}
