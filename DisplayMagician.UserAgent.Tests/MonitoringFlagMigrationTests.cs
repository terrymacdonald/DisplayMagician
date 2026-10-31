using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class MonitoringFlagMigrationTests
{
    [Fact]
    public void FailedMigrationPreservesTheOriginalInsteadOfLoadingTheWrongDefaultFlag()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianMonitorMigration", Guid.NewGuid().ToString("N"));
        string directory = Path.Combine(root, "Shortcuts");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "Shortcuts.json");
        const string original = "{\"ShortcutFileVersion\":\"8\",\"Shortcuts\":[{\"UUID\":\"monitor-migration\",\"ProcessNameToMonitorUsesExecutable\":false}]}";
        try
        {
            File.WriteAllText(path, original, Encoding.Unicode);
            using FileStream held = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Assert.Throws<InvalidDataException>(() => new ShortcutStore(root));
            Assert.Equal(original, File.ReadAllText(path, Encoding.Unicode));
            Assert.Single(Directory.GetFiles(directory, "Shortcuts.json.v8.*.bak"));
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData("ProcessNameToMonitorUsesExecutable", "7", false)]
    [InlineData("ProcessNameToMonitorUsesExecutable", "7", true)]
    [InlineData("ProcessNameToMonitorUsesExecutable", "8", false)]
    [InlineData("ProcessNameToMonitorUsesExecutable", "8", true)]
    [InlineData("MonitorExecutablePath", "7", false)]
    [InlineData("MonitorExecutablePath", "7", true)]
    [InlineData("MonitorExecutablePath", "8", false)]
    [InlineData("MonitorExecutablePath", "8", true)]
    public void ShortcutMigrationRenamesOldFieldsAndPreservesTheSelection(string oldName, string version, bool selection)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianMonitorMigration", Guid.NewGuid().ToString("N"));
        string directory = Path.Combine(root, "Shortcuts");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "Shortcuts.json");
        try
        {
            JsonObject shortcut = new JsonObject
            {
                ["UUID"] = "monitor-migration", ["Name"] = "Original name", [oldName] = selection,
                ["DifferentExecutableToMonitor"] = "C:\\Tools\\target.exe", ["CustomChoice"] = "keep",
                ["StartPrograms"] = new JsonArray(new JsonObject { ["Id"] = "stable-program-id" }),
                ["WindowPlacements"] = new JsonArray(new JsonObject { ["ProgramId"] = "main", ["ShowCommand"] = 3 })
            };
            string original = new JsonObject { ["ShortcutFileVersion"] = version, ["Shortcuts"] = new JsonArray(shortcut) }.ToJsonString();
            File.WriteAllText(path, original, Encoding.Unicode);

            ShortcutStore store = new ShortcutStore(root);
            using JsonDocument document = JsonDocument.Parse(store.GetSnapshot().Json);
            JsonElement saved = document.RootElement.GetProperty("Shortcuts")[0];
            Assert.Equal("9", document.RootElement.GetProperty("ShortcutFileVersion").GetString());
            Assert.Equal(selection, saved.GetProperty("MonitorLaunchedExecutable").GetBoolean());
            Assert.False(saved.TryGetProperty("ProcessNameToMonitorUsesExecutable", out _));
            Assert.False(saved.TryGetProperty("MonitorExecutablePath", out _));
            Assert.Equal("Original name", saved.GetProperty("Name").GetString());
            Assert.Equal("keep", saved.GetProperty("CustomChoice").GetString());
            Assert.Equal("C:\\Tools\\target.exe", saved.GetProperty("DifferentExecutableToMonitor").GetString());
            Assert.Equal("stable-program-id", saved.GetProperty("StartPrograms")[0].GetProperty("Id").GetString());
            Assert.Equal(3, saved.GetProperty("WindowPlacements")[0].GetProperty("ShowCommand").GetInt32());
            Assert.True(store.TryGetShortcutDefinition("monitor-migration", out var definition));
            Assert.Equal(selection, definition!.MonitorLaunchedExecutable);
            string backup = Assert.Single(Directory.GetFiles(directory, "Shortcuts.json.v*.bak"));
            Assert.Equal(original, File.ReadAllText(backup, Encoding.Unicode));

            byte[] migrated = File.ReadAllBytes(path);
            _ = new ShortcutStore(root);
            Assert.Equal(migrated, File.ReadAllBytes(path));
            Assert.Single(Directory.GetFiles(directory, "Shortcuts.json.v*.bak"));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void ExistingCanonicalChoiceWinsOverAStaleLegacyField()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianMonitorMigration", Guid.NewGuid().ToString("N"));
        string directory = Path.Combine(root, "Shortcuts");
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "Shortcuts.json"),
                "{\"ShortcutFileVersion\":\"8\",\"Shortcuts\":[{\"UUID\":\"monitor-migration\",\"MonitorLaunchedExecutable\":true,\"ProcessNameToMonitorUsesExecutable\":false}]}", Encoding.Unicode);
            ShortcutStore store = new ShortcutStore(root);
            Assert.True(store.TryGetShortcutDefinition("monitor-migration", out var definition));
            Assert.True(definition!.MonitorLaunchedExecutable);
            Assert.DoesNotContain("ProcessNameToMonitorUsesExecutable", store.GetSnapshot().Json);
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ExistingLegacyDesktopMigrationAlsoWritesTheCanonicalField(bool selection)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianLegacyMonitorMigration", Guid.NewGuid().ToString("N"));
        string directory = Path.Combine(root, "Shortcuts");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "Shortcuts.json");
        FieldInfo appData = typeof(global::DisplayMagician.Program).GetField("AppDataPath", BindingFlags.NonPublic | BindingFlags.Static)!;
        object? previous = appData.GetValue(null);
        try
        {
            string original = JsonSerializer.Serialize(new { Shortcuts = new[] { new { UUID = "legacy-shortcut", ProcessNameToMonitorUsesExecutable = selection } } });
            File.WriteAllText(path, original, Encoding.Unicode);
            appData.SetValue(null, root);
            Type rule = typeof(global::DisplayMagician.Program).Assembly.GetType("DisplayMagician.ConfigMigrationRunner+SettingsV3ToV4LegacyDataMigration")!;
            rule.GetMethod("MigrateLegacyShortcuts", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null);

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path, Encoding.Unicode));
            Assert.Equal("5", document.RootElement.GetProperty("ShortcutFileVersion").GetString());
            JsonElement shortcut = document.RootElement.GetProperty("Shortcuts")[0];
            Assert.Equal(selection, shortcut.GetProperty("MonitorLaunchedExecutable").GetBoolean());
            Assert.False(shortcut.TryGetProperty("ProcessNameToMonitorUsesExecutable", out _));
            Assert.Equal(original, File.ReadAllText(path + ".v3-legacy.bak", Encoding.Unicode));
        }
        finally { appData.SetValue(null, previous); Directory.Delete(root, true); }
    }
}
