using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DisplayMagician.ControlService;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class MigrationFailureTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-MigrationFailures", Guid.NewGuid().ToString("N"));

    [Theory]
    [InlineData(false, "{")]
    [InlineData(false, "")]
    [InlineData(true, "{\"Profiles\":null}")]
    [InlineData(true, "[]")]
    public void InvalidSourceDoesNotReplaceAnExistingDestination(bool transform, string json)
    {
        UserStoragePaths paths = new StoragePaths(Path.Combine(_root, "storage")).GetUserPaths("S-1-5-18");
        paths.EnsureDirectories();
        string source = Path.Combine(_root, "source.json");
        string target = Path.Combine(paths.ProfilesPath, "DisplayProfiles.json");
        File.WriteAllText(source, json, Encoding.Unicode);
        File.WriteAllText(target, "{\"Profiles\":[],\"Choice\":\"keep\"}", Encoding.Unicode);
        byte[] original = File.ReadAllBytes(target);
        LegacyFileMigrationResult result = new LegacyFileMigration().MigrateFile(source, target, paths, transform ? value => value : null);
        Assert.False(result.IsSuccessful);
        Assert.Equal(original, File.ReadAllBytes(target));
        Assert.Equal(json, File.ReadAllText(source, Encoding.Unicode));
        Assert.False(File.Exists(source + ".old"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingSourceWithInvalidDestinationReturnsFailure(bool locked)
    {
        UserStoragePaths paths = new StoragePaths(Path.Combine(_root, "storage")).GetUserPaths("S-1-5-18");
        paths.EnsureDirectories();
        string target = Path.Combine(paths.ProfilesPath, "DisplayProfiles.json");
        File.WriteAllText(target, "invalid-json");
        using FileStream? held = locked ? new FileStream(target, FileMode.Open, FileAccess.ReadWrite, FileShare.None) : null;
        LegacyFileMigrationResult result = new LegacyFileMigration().MigrateFile(Path.Combine(_root, "missing.json"), target, paths, value => value);
        Assert.False(result.IsSuccessful);
    }

    [Fact]
    public void RetiredSourcesInSubdirectoriesAreNotImportedAgain()
    {
        string legacy = Path.Combine(_root, "legacy");
        Directory.CreateDirectory(Path.Combine(legacy, "Shortcuts"));
        string retired = Path.Combine(legacy, "Shortcuts", "Shortcuts.json.old");
        File.WriteAllText(retired, "{\"Shortcuts\":[]}", Encoding.Unicode);
        UserStoragePaths paths = new StoragePaths(Path.Combine(_root, "storage")).GetUserPaths("S-1-5-18");
        UserDataMigrationResult result = new UserDataMigrationRunner(new LegacyFileMigration()).Migrate(legacy, paths);
        Assert.True(result.IsSuccessful);
        Assert.Empty(result.Files);
        Assert.True(File.Exists(retired));
        Assert.False(File.Exists(Path.Combine(paths.ShortcutsPath, "Shortcuts.json.old")));
        Assert.False(File.Exists(retired + ".old"));
    }

    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"SchemaVersion\":1,\"MigratedUtc\":\"2026-10-27T00:00:00Z\",\"LegacyAppDataPath\":\"old\"}")]
    [InlineData("{\"SchemaVersion\":2,\"MigratedUtc\":\"2026-10-27T00:00:00Z\",\"LegacyAppDataPath\":\"old\",\"Files\":[]}")]
    [InlineData("{\"SchemaVersion\":1,\"MigratedUtc\":\"2026-10-27T00:00:00Z\",\"LegacyAppDataPath\":\"old\",\"Files\":null}")]
    [InlineData("{\"SchemaVersion\":1,\"MigratedUtc\":\"2026-10-27T00:00:00Z\",\"LegacyAppDataPath\":\"old\",\"Files\":[null]}")]
    [InlineData("{\"SchemaVersion\":1,\"MigratedUtc\":\"2026-10-27T00:00:00Z\",\"LegacyAppDataPath\":\"old\",\"Files\":[{}]}")]
    public void InvalidMarkerBlocksImportAndPreservesBothSides(string marker)
    {
        UserStoragePaths paths = new StoragePaths(Path.Combine(_root, "storage")).GetUserPaths("S-1-5-18");
        paths.EnsureDirectories();
        string legacy = Path.Combine(_root, "legacy");
        Directory.CreateDirectory(legacy);
        string source = Path.Combine(legacy, "Settings.json");
        string target = Path.Combine(paths.SettingsPath, "Settings.json");
        File.WriteAllText(source, "{\"Value\":\"old\"}");
        File.WriteAllText(target, "{\"Value\":\"new\"}");
        File.WriteAllText(paths.MigrationMarkerPath, marker);
        UserDataMigrationResult result = new UserDataMigrationRunner(new LegacyFileMigration()).Migrate(legacy, paths);
        Assert.False(result.IsSuccessful);
        Assert.False(result.WasAlreadyMigrated);
        Assert.Contains(paths.MigrationMarkerPath, result.Message);
        Assert.Empty(result.Files);
        Assert.Equal(marker, File.ReadAllText(paths.MigrationMarkerPath));
        Assert.Equal("{\"Value\":\"old\"}", File.ReadAllText(source));
        Assert.Equal("{\"Value\":\"new\"}", File.ReadAllText(target));
        Assert.Empty(Directory.GetFiles(paths.BackupsPath));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnreadableMarkerBlocksImport(bool directory)
    {
        UserStoragePaths paths = new StoragePaths(Path.Combine(_root, "storage")).GetUserPaths("S-1-5-18");
        paths.EnsureDirectories();
        if (directory) Directory.CreateDirectory(paths.MigrationMarkerPath);
        else File.WriteAllText(paths.MigrationMarkerPath, JsonSerializer.Serialize(new MigrationMarker { SchemaVersion = 1, MigratedUtc = DateTime.UtcNow, LegacyAppDataPath = "old" }));
        using FileStream? held = directory ? null : new FileStream(paths.MigrationMarkerPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        UserDataMigrationResult result = new UserDataMigrationRunner(new LegacyFileMigration()).Migrate(Path.Combine(_root, "legacy"), paths);
        Assert.False(result.IsSuccessful);
        Assert.Empty(result.Files);
    }

    [Fact]
    public async Task ConcurrentMigrationsImportOnceAndThenValidateTheCompletedMarker()
    {
        UserStoragePaths paths = new StoragePaths(Path.Combine(_root, "storage")).GetUserPaths("S-1-5-18");
        string legacy = Path.Combine(_root, "legacy");
        Directory.CreateDirectory(legacy);
        File.WriteAllText(Path.Combine(legacy, "Settings.json"), "{\"Settings\":{}}");
        UserDataMigrationRunner runner = new UserDataMigrationRunner(new LegacyFileMigration());
        TaskCompletionSource<bool> start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<UserDataMigrationResult>[] requests = Enumerable.Range(0, 8).Select(_ => Task.Run(async () => { await start.Task; return runner.Migrate(legacy, paths); })).ToArray();
        start.TrySetResult(true);
        UserDataMigrationResult[] results = await Task.WhenAll(requests).WaitAsync(TimeSpan.FromSeconds(15));
        Assert.All(results, result => Assert.True(result.IsSuccessful, result.Message));
        Assert.Single(results, result => !result.WasAlreadyMigrated);
        Assert.True(File.Exists(Path.Combine(legacy, "Settings.json.old")));
        Assert.Single(Directory.GetFiles(paths.BackupsPath, "*.pre-v4.bak"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}
