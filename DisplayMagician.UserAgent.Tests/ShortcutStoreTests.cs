using System;
using System.IO;
using System.Text.Json;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ShortcutStoreTests
{
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
