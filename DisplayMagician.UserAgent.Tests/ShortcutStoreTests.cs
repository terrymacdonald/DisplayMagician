using System;
using System.IO;
using System.Text;
using System.Text.Json;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ShortcutStoreTests
{
    [Fact]
    public void FutureShortcutSchemaIsUntouchedByBothMigrations()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagician-FutureShortcut", Guid.NewGuid().ToString("N"));
        string directory = Path.Combine(root, "Shortcuts");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "Shortcuts.json");
        try
        {
            string json = "{\"ShortcutFileVersion\":\"99\",\"Shortcuts\":[{\"UUID\":\"future\",\"Application\":{\"Future\":true},\"MonitorExecutablePath\":false}]}";
            File.WriteAllText(path, json, Encoding.Unicode);
            byte[] original = File.ReadAllBytes(path);
            _ = new ShortcutStore(root);
            Assert.Equal(original, File.ReadAllBytes(path));
            Assert.Single(Directory.GetFiles(directory));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void FailedShortcutMigrationKeepsOriginalAndAllowsImmediateRetryWithoutTemporaryFiles()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagician-LockedShortcut", Guid.NewGuid().ToString("N"));
        string directory = Path.Combine(root, "Shortcuts");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "Shortcuts.json");
        try
        {
            string json = "{\"ShortcutFileVersion\":\"6\",\"Shortcuts\":[{\"UUID\":\"old\",\"MonitorExecutablePath\":false}]}";
            File.WriteAllText(path, json, Encoding.Unicode);
            using (FileStream held = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Assert.Throws<InvalidDataException>(() => new ShortcutStore(root));
                Assert.Equal(json, File.ReadAllText(path, Encoding.Unicode));
                Assert.Empty(Directory.GetFiles(directory, "*.tmp"));
            }
            ShortcutStore retried = new ShortcutStore(root);
            Assert.True(retried.TryGetShortcutDefinition("old", out ShortcutDefinition? shortcut));
            Assert.False(shortcut!.MonitorLaunchedExecutable);
            Assert.All(Directory.GetFiles(directory, "Shortcuts.json.v6.*.bak"), backup => Assert.Equal(json, File.ReadAllText(backup, Encoding.Unicode)));
            Assert.Empty(Directory.GetFiles(directory, "*.tmp"));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void Constructor_MigratesWindowPlacementSchemaAndBacksUpShortcuts()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutStore-{Guid.NewGuid():N}");
        try
        {
            string directory = Path.Combine(root, "Shortcuts");
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "Shortcuts.json"),
                "{\"ShortcutFileVersion\":\"6\",\"Shortcuts\":[{\"UUID\":\"shortcut-one\",\"Name\":\"Original\",\"StartPrograms\":[{\"Executable\":\"C:\\\\Tools\\\\helper.exe\"}]}]}", Encoding.Unicode);

            ShortcutStore store = new ShortcutStore(root);
            using JsonDocument document = JsonDocument.Parse(store.GetSnapshot().Json);
            JsonElement shortcut = document.RootElement.GetProperty("Shortcuts")[0];
            Assert.Equal("9", document.RootElement.GetProperty("ShortcutFileVersion").GetString());
            Assert.Equal("Original", shortcut.GetProperty("Name").GetString());
            Assert.True(Guid.TryParse(shortcut.GetProperty("StartPrograms")[0].GetProperty("Id").GetString(), out _));
            Assert.Equal(20, shortcut.GetProperty("WindowPlacementTimeoutSeconds").GetInt32());
            Assert.Empty(shortcut.GetProperty("WindowPlacements").EnumerateArray());
            Assert.Single(Directory.GetFiles(directory, "Shortcuts.json.v6.*.bak"));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void Constructor_PreservesVersionSevenPlacementsWithoutMaximizedCoordinates()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutStore-{Guid.NewGuid():N}");
        try
        {
            string directory = Path.Combine(root, "Shortcuts");
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "Shortcuts.json"),
                "{\"ShortcutFileVersion\":\"7\",\"Shortcuts\":[{\"UUID\":\"shortcut-one\",\"WindowPlacements\":[{\"ProgramId\":\"main\",\"ShowCommand\":3}]}]}", Encoding.Unicode);

            ShortcutStore store = new ShortcutStore(root);
            using JsonDocument document = JsonDocument.Parse(store.GetSnapshot().Json);
            JsonElement placement = document.RootElement.GetProperty("Shortcuts")[0].GetProperty("WindowPlacements")[0];
            Assert.Equal("9", document.RootElement.GetProperty("ShortcutFileVersion").GetString());
            Assert.Equal(3, placement.GetProperty("ShowCommand").GetInt32());
            Assert.False(placement.TryGetProperty("MaximizedLeft", out _));
            Assert.Single(Directory.GetFiles(directory, "Shortcuts.json.v7.*.bak"));
            Assert.True(store.TryGetShortcutDefinition("shortcut-one", out ShortcutDefinition? migrated));
            Assert.Null(Assert.Single(migrated!.WindowPlacements).MaximizedLeft);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ReplaceWindowPlacements_ReplacesSnapshotAndPreservesOtherShortcutData()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutStore-{Guid.NewGuid():N}");
        try
        {
            ShortcutStore store = new ShortcutStore(root);
            RepositorySnapshot initial = store.GetSnapshot();
            store.Commit(new RepositoryCommitRequest
            {
                Repository = RepositoryKind.Shortcuts,
                ExpectedRevision = initial.Revision,
                Json = "{\"ShortcutFileVersion\":\"7\",\"Shortcuts\":[{\"UUID\":\"shortcut-one\",\"Name\":\"Original\",\"WindowPlacements\":[{\"ProgramId\":\"old\"}],\"CustomChoice\":true}]}"
            });
            int replaced = store.ReplaceWindowPlacements("shortcut-one", new[]
            {
                new ShortcutWindowPlacementDefinition { ProgramId = "main", ExecutablePath = "C:\\Game.exe", WindowClass = "Game", Left = 10, Right = 200, MaximizedLeft = 5, MaximizedTop = 7, ShowCommand = 3 }
            });

            Assert.Equal(1, replaced);
            using JsonDocument document = JsonDocument.Parse(store.GetSnapshot().Json);
            JsonElement shortcut = document.RootElement.GetProperty("Shortcuts")[0];
            Assert.True(shortcut.GetProperty("CustomChoice").GetBoolean());
            Assert.Equal("Original", shortcut.GetProperty("Name").GetString());
            Assert.Equal("main", shortcut.GetProperty("WindowPlacements")[0].GetProperty("ProgramId").GetString());
            Assert.Equal(3, shortcut.GetProperty("WindowPlacements")[0].GetProperty("ShowCommand").GetInt32());
            Assert.Equal(5, shortcut.GetProperty("WindowPlacements")[0].GetProperty("MaximizedLeft").GetInt32());
            Assert.Equal(7, shortcut.GetProperty("WindowPlacements")[0].GetProperty("MaximizedTop").GetInt32());

            int cleared = store.ReplaceWindowPlacements("shortcut-one", Array.Empty<ShortcutWindowPlacementDefinition>());
            Assert.Equal(1, cleared);
            using JsonDocument clearedDocument = JsonDocument.Parse(store.GetSnapshot().Json);
            JsonElement clearedShortcut = clearedDocument.RootElement.GetProperty("Shortcuts")[0];
            Assert.Empty(clearedShortcut.GetProperty("WindowPlacements").EnumerateArray());
            Assert.True(clearedShortcut.GetProperty("CustomChoice").GetBoolean());
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GetSavedShortcutBitmaps_ReturnsPersistedCompositeImage()
    {
        ShortcutStore store = new ShortcutStore(Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutStore-{Guid.NewGuid():N}"));
        string bitmap = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
        string json = JsonSerializer.Serialize(new
        {
            Shortcuts = new[]
            {
                new { UUID = "selected-game", ShortcutBitmap = (string?)bitmap },
                new { UUID = "no-image", ShortcutBitmap = (string?)null }
            }
        });

        var savedBitmaps = store.GetSavedShortcutBitmaps(json);

        Assert.Equal(bitmap, savedBitmaps["selected-game"]);
        Assert.False(savedBitmaps.ContainsKey("no-image"));
    }

    [Fact]
    public void Commit_WritesSnapshotAndRejectsStaleRevision()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutStore-{Guid.NewGuid():N}");
        try
        {
            ShortcutStore store = new ShortcutStore(root);
            RepositorySnapshot initial = store.GetSnapshot();

            RepositoryCommitResult committed = store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = initial.Revision, Json = "{\"Shortcuts\":[]}" });

            Assert.False(committed.WasConflict);
            Assert.NotNull(committed.Snapshot);
            Assert.NotEqual(initial.Revision, committed.Snapshot!.Revision);
            using JsonDocument document = JsonDocument.Parse(committed.Snapshot.Json);
            Assert.True(document.RootElement.TryGetProperty("Shortcuts", out _));

            RepositoryCommitResult conflict = store.Commit(new RepositoryCommitRequest { Repository = RepositoryKind.Shortcuts, ExpectedRevision = initial.Revision, Json = "{\"Shortcuts\":[{}]}" });
            Assert.True(conflict.WasConflict);
            Assert.Equal(committed.Snapshot.Revision, conflict.Snapshot!.Revision);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void Constructor_RemovesLegacyApplicationObjectAndCreatesBackup()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutStore-{Guid.NewGuid():N}");
        try
        {
            string shortcutDirectory = Path.Combine(root, "Shortcuts");
            Directory.CreateDirectory(shortcutDirectory);
            string shortcutPath = Path.Combine(shortcutDirectory, "Shortcuts.json");
            File.WriteAllText(shortcutPath, "{\"Shortcuts\":[{\"UUID\":\"application-shortcut\",\"ApplicationId\":\"Contoso.App_abc!App\",\"Application\":{\"$type\":\"DisplayMagician.AppLibraries.LocalApp, DisplayMagician\",\"Id\":\"Contoso.App_abc!App\"}}]}", Encoding.Unicode);

            ShortcutStore store = new ShortcutStore(root);

            using JsonDocument document = JsonDocument.Parse(store.GetSnapshot().Json);
            JsonElement shortcut = document.RootElement.GetProperty("Shortcuts")[0];
            Assert.Equal("Contoso.App_abc!App", shortcut.GetProperty("ApplicationId").GetString());
            Assert.False(shortcut.TryGetProperty("Application", out _));
            Assert.Single(Directory.GetFiles(shortcutDirectory, "Shortcuts.json.legacy-application.*.bak"));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void Commit_PreservesAutomaticallyDetectedGameLaunchMode()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutStore-{Guid.NewGuid():N}");
        try
        {
            ShortcutStore store = new ShortcutStore(root);
            RepositorySnapshot initial = store.GetSnapshot();
            string shortcutJson = "{\"ShortcutFileVersion\":\"6\",\"Shortcuts\":[{\"UUID\":\"shortcut-id\",\"GameLaunchMode\":1}]}";

            RepositoryCommitResult committed = store.Commit(new RepositoryCommitRequest
            {
                Repository = RepositoryKind.Shortcuts,
                ExpectedRevision = initial.Revision,
                Json = shortcutJson
            });

            Assert.False(committed.WasConflict);
            using JsonDocument document = JsonDocument.Parse(committed.Snapshot!.Json);
            Assert.Equal(1, document.RootElement.GetProperty("Shortcuts")[0].GetProperty("GameLaunchMode").GetInt32());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void TryGetShortcutDefinition_ReadsAutomaticallyDetectedGameShortcut()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutStore-{Guid.NewGuid():N}");
        try
        {
            ShortcutStore store = new ShortcutStore(root);
            RepositorySnapshot initial = store.GetSnapshot();
            store.Commit(new RepositoryCommitRequest
            {
                Repository = RepositoryKind.Shortcuts,
                ExpectedRevision = initial.Revision,
                Json = "{\"ShortcutFileVersion\":\"6\",\"Shortcuts\":[{\"UUID\":\"shortcut-id\",\"Name\":\"Test Game\",\"Category\":1,\"ProfileUUID\":\"display-profile\",\"AudioProfileUUID\":\"audio-profile\",\"DisplayPermanence\":1,\"AudioPermanence\":0,\"ProcessPriority\":1,\"GameAppId\":\"42\",\"GameName\":\"Test Game\",\"GameLibrary\":1,\"GameLaunchMode\":1,\"StartTimeout\":90,\"GameArguments\":\"-windowed\",\"GameArgumentsRequired\":true,\"MonitorDifferentGameExe\":true,\"DifferentGameExeToMonitor\":\"C:\\\\Games\\\\TestGame.exe\",\"StartPrograms\":[{\"Priority\":1,\"Executable\":\"C:\\\\Tools\\\\before.exe\",\"Arguments\":\"--ready\",\"ExecutableArgumentsRequired\":true,\"CloseOnFinish\":true}],\"AfterPrograms\":[{\"Priority\":2,\"Executable\":\"C:\\\\Tools\\\\after.exe\"}],\"StopPrograms\":[{\"Priority\":3,\"Executable\":\"C:\\\\Tools\\\\stop.exe\",\"RestartAfterwards\":true}]}]}"
            });

            bool wasFound = store.TryGetShortcutDefinition("shortcut-id", out ShortcutDefinition? shortcut);

            Assert.True(wasFound);
            Assert.NotNull(shortcut);
            Assert.Equal(ShortcutDefinitionCategory.Game, shortcut!.Category);
            Assert.Equal(GameLaunchMode.DetectGameRunning, shortcut.GameLaunchMode);
            Assert.Equal("42", shortcut.GameAppId);
            Assert.Equal("display-profile", shortcut.ProfileId);
            Assert.Equal("audio-profile", shortcut.AudioProfileId);
            Assert.Equal(ShortcutDefinitionPermanence.Permanent, shortcut.AudioPermanence);
            Assert.Equal(ShortcutDefinitionProcessPriority.AboveNormal, shortcut.ProcessPriority);
            Assert.Equal(90, shortcut.StartTimeoutSeconds);
            Assert.Equal("-windowed", shortcut.GameArguments);
            Assert.True(shortcut.MonitorDifferentGameExecutable);
            Assert.Single(shortcut.StartPrograms);
            Assert.Equal("C:\\Tools\\before.exe", shortcut.StartPrograms[0].ExecutablePath);
            Assert.True(shortcut.StartPrograms[0].CloseOnFinish);
            Assert.Single(shortcut.AfterPrograms);
            Assert.Equal("C:\\Tools\\after.exe", shortcut.AfterPrograms[0].ExecutablePath);
            Assert.Single(shortcut.StopPrograms);
            Assert.True(shortcut.StopPrograms[0].RestartAfterwards);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void TryGetShortcutDefinition_ReadsApplicationShortcutIdentity()
    {
        string root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-ShortcutStore-{Guid.NewGuid():N}");
        try
        {
            ShortcutStore store = new ShortcutStore(root);
            RepositorySnapshot initial = store.GetSnapshot();
            store.Commit(new RepositoryCommitRequest
            {
                Repository = RepositoryKind.Shortcuts,
                ExpectedRevision = initial.Revision,
                Json = "{\"Shortcuts\":[{\"UUID\":\"application-shortcut\",\"Category\":3,\"ApplicationId\":\"Contoso.App_abc!App\",\"ApplicationName\":\"Contoso App\",\"ApplicationLibrary\":2,\"ExecutableArguments\":\"--fullscreen\"}]}"
            });

            bool wasFound = store.TryGetShortcutDefinition("application-shortcut", out ShortcutDefinition? shortcut);

            Assert.True(wasFound);
            Assert.NotNull(shortcut);
            Assert.Equal(ShortcutDefinitionCategory.Application, shortcut!.Category);
            Assert.Equal("Contoso.App_abc!App", shortcut.ApplicationId);
            Assert.Equal("Contoso App", shortcut.ApplicationName);
            Assert.Equal(2, shortcut.ApplicationLibrary);
            Assert.Equal("--fullscreen", shortcut.ExecutableArguments);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}
