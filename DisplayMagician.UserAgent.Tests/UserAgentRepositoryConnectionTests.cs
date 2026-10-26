using System;
using System.IO;
using System.Drawing;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent.Runtime;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class UserAgentRepositoryConnectionTests
{
    [Fact]
    public void AddShortcut_CommitsBeforeWritingItsIconAndPersistsTheDerivedIconPath()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianIconTests", Guid.NewGuid().ToString("N"));
        string previousIconPath = global::DisplayMagician.Program.AppIconPath;
        try
        {
            global::DisplayMagician.Program.AppIconPath = root;
            using Bitmap bitmap = new Bitmap(16, 16);
            ShortcutItem draft = new ShortcutItem { Name = "Saved shortcut", ShortcutBitmap = bitmap };
            string iconPath = Path.Combine(root, "ShortcutCache", draft.UUID + ".ico");
            FakeUserAgentRepositoryConnection connection = new FakeUserAgentRepositoryConnection(new RepositorySnapshot
            {
                Repository = RepositoryKind.Shortcuts, Revision = 29, Json = "{\"Shortcuts\":[]}"
            }) { BeforeCommit = request =>
            {
                Assert.False(File.Exists(iconPath));
                Assert.Contains(iconPath.Replace("\\", "\\\\"), request.Json);
            }};
            ShortcutRepository.ConnectToUserAgent(connection);

            Assert.True(ShortcutRepository.AddShortcut(draft));
            Assert.True(File.Exists(iconPath));
            Assert.Equal(draft.UUID, Assert.Single(ShortcutRepository.AllShortcuts).UUID);
        }
        finally
        {
            global::DisplayMagician.Program.AppIconPath = previousIconPath;
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void EditingCopyPreservesIdentityAndSeparatesMutableDraftCollections()
    {
        ShortcutItem original = new ShortcutItem { Name = "Original" };
        original.StartPrograms.Add(new StartProgram { Id = "helper", Executable = "original.exe" });
        ShortcutItem draft = original.CreateEditingCopy();
        draft.Name = "Edited";
        draft.StartPrograms.Clear();

        Assert.Equal(original.UUID, draft.UUID);
        Assert.Equal("Original", original.Name);
        Assert.Single(original.StartPrograms);
        Assert.Empty(draft.StartPrograms);
        Assert.NotSame(original.WindowPlacements, draft.WindowPlacements);
    }

    [Fact]
    public void AddShortcut_ReportsConflictAndKeepsTheAuthoritativeCache()
    {
        FakeUserAgentRepositoryConnection connection = new FakeUserAgentRepositoryConnection(new RepositorySnapshot
        {
            Repository = RepositoryKind.Shortcuts, Revision = 29,
            Json = "{\"Shortcuts\":[]}"
        }) { WasConflict = true };
        ShortcutRepository.ConnectToUserAgent(connection);
        ShortcutItem draft = new ShortcutItem { Name = "Unsaved draft" };

        Assert.False(ShortcutRepository.AddShortcut(draft));
        Assert.True(ShortcutRepository.LastSaveWasConflict);
        Assert.Contains("changed elsewhere", ShortcutRepository.LastSaveErrorMessage);
        Assert.Empty(ShortcutRepository.AllShortcuts);
        Assert.Equal("Unsaved draft", draft.Name);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RemoveShortcut_FailedCommitPreservesTheShortcutAndItsIcon(bool conflict)
    {
        string iconPath = Path.GetTempFileName();
        try
        {
            string id = Guid.NewGuid().ToString("D");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Shortcuts = new[] { new { UUID = id, Name = "Saved shortcut", SavedShortcutIconCacheFilename = iconPath, SelectedImage = new ShortcutBitmap() } }
            });
            FakeUserAgentRepositoryConnection connection = new FakeUserAgentRepositoryConnection(new RepositorySnapshot
            {
                Repository = RepositoryKind.Shortcuts, Revision = 29, Json = json
            }) { WasConflict = conflict, FailSave = !conflict };
            ShortcutRepository.ConnectToUserAgent(connection);

            Assert.False(ShortcutRepository.RemoveShortcut(id));
            Assert.Equal(id, Assert.Single(ShortcutRepository.AllShortcuts).UUID);
            Assert.True(File.Exists(iconPath));
        }
        finally { File.Delete(iconPath); }
    }

    [Fact]
    public void UpdateShortcut_FailedCommitRestoresSavedCacheAndRetainsTheDraft()
    {
        string id = Guid.NewGuid().ToString("D");
        FakeUserAgentRepositoryConnection connection = new FakeUserAgentRepositoryConnection(new RepositorySnapshot
        {
            Repository = RepositoryKind.Shortcuts, Revision = 29,
            Json = Newtonsoft.Json.JsonConvert.SerializeObject(new { Shortcuts = new[] { new { UUID = id, Name = "Original", SelectedImage = new ShortcutBitmap() } } })
        }) { FailSave = true };
        ShortcutRepository.ConnectToUserAgent(connection);
        ShortcutItem draft = new ShortcutItem { UUID = id, Name = "Proposed edit" };

        Assert.False(ShortcutRepository.UpdateShortcut(draft));
        Assert.Equal("Original", Assert.Single(ShortcutRepository.AllShortcuts).Name);
        Assert.Equal("Proposed edit", draft.Name);
    }

    [Fact]
    public void ConnectToUserAgent_LoadsAnEmptyDisplayProfileSnapshotIntoTheRepositoryCache()
    {
        FakeUserAgentRepositoryConnection connection = new FakeUserAgentRepositoryConnection(new RepositorySnapshot
        {
            Repository = RepositoryKind.DisplayProfiles,
            Revision = 17,
            Json = "{\"ProfileFileVersion\":\"4\",\"LastUpdated\":\"2026-01-01T00:00:00\",\"Profiles\":[]}"
        });

        ProfileRepository.ConnectToUserAgent(connection);

        Assert.Equal(RepositoryKind.DisplayProfiles, connection.RequestedRepository);
        Assert.True(ProfileRepository.ProfilesLoaded);
        Assert.Empty(ProfileRepository.AllProfiles);
    }

    [Fact]
    public void ConnectToUserAgent_LoadsAnEmptyAudioProfileSnapshotIntoTheRepositoryCache()
    {
        FakeUserAgentRepositoryConnection connection = new FakeUserAgentRepositoryConnection(new RepositorySnapshot
        {
            Repository = RepositoryKind.AudioProfiles,
            Revision = 23,
            Json = "{\"AudioProfileFileVersion\":\"1\",\"LastUpdated\":\"2026-01-01T00:00:00\",\"AudioProfiles\":[]}"
        });

        AudioProfileRepository.ConnectToUserAgent(connection);

        Assert.Equal(RepositoryKind.AudioProfiles, connection.RequestedRepository);
        Assert.True(AudioProfileRepository.AudioProfilesLoaded);
        Assert.Empty(AudioProfileRepository.AllAudioProfiles);
    }

    [Fact]
    public void ConnectToUserAgent_LoadsAnEmptyShortcutSnapshotIntoTheRepositoryCache()
    {
        FakeUserAgentRepositoryConnection connection = new FakeUserAgentRepositoryConnection(new RepositorySnapshot
        {
            Repository = RepositoryKind.Shortcuts,
            Revision = 29,
            Json = "{\"ShortcutFileVersion\":\"5\",\"LastUpdated\":\"2026-01-01T00:00:00\",\"Shortcuts\":[]}"
        });

        ShortcutRepository.ConnectToUserAgent(connection);

        Assert.Equal(RepositoryKind.Shortcuts, connection.RequestedRepository);
        Assert.Empty(ShortcutRepository.AllShortcuts);
    }

    private sealed class FakeUserAgentRepositoryConnection : IUserAgentRepositoryConnection
    {
        private readonly RepositorySnapshot _snapshot;

        public FakeUserAgentRepositoryConnection(RepositorySnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public RepositoryKind RequestedRepository { get; private set; }
        public bool WasConflict { get; set; }
        public bool FailSave { get; set; }
        public Action<RepositoryCommitRequest>? BeforeCommit { get; set; }

        public RepositorySnapshot GetRepositorySnapshot(RepositoryKind repository)
        {
            RequestedRepository = repository;
            return _snapshot;
        }

        public RepositoryCommitResult CommitRepositorySnapshot(RepositoryCommitRequest request)
        {
            if (FailSave) throw new IOException("Simulated unavailable storage.");
            BeforeCommit?.Invoke(request);
            return new RepositoryCommitResult
            {
                Snapshot = WasConflict ? _snapshot : new RepositorySnapshot
                {
                    Repository = request.Repository, Revision = _snapshot.Revision + 1, Json = request.Json
                },
                WasConflict = WasConflict
            };
        }
    }
}
