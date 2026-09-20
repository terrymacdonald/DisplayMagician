using DisplayMagician.Contracts;
using DisplayMagician.UserAgent.Runtime;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class UserAgentRepositoryConnectionTests
{
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

        public RepositorySnapshot GetRepositorySnapshot(RepositoryKind repository)
        {
            RequestedRepository = repository;
            return _snapshot;
        }

        public RepositoryCommitResult CommitRepositorySnapshot(RepositoryCommitRequest request)
        {
            return new RepositoryCommitResult { Snapshot = _snapshot };
        }
    }
}
