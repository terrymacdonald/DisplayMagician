using System;
using System.Linq;
using DisplayMagician.Gateway;
using Xunit;

namespace DisplayMagician.Gateway.Tests;

public sealed class GatewayCursorPageStoreTests
{
    [Fact]
    public void SubsequentPagesUseTheOriginalSnapshotAndBindTheCallerAndFilters()
    {
        GatewayCursorPageStore store = new GatewayCursorPageStore();
        DateTime now = DateTime.UtcNow;
        string[] original = { "first", "second", "third" };
        GatewayCursorPageResult<string> first = store.Create("owner/device", "/v1/shortcuts", "includeArtwork=false", original, 1, now);
        original[1] = "changed after capture";

        Assert.Equal("first", Assert.Single(first.Items));
        Assert.NotNull(first.NextCursor);
        Assert.Equal(GatewayCursorPageState.Invalid, store.Read<string>("other/device", "/v1/shortcuts", "includeArtwork=false", first.NextCursor!, 1, now).State);
        Assert.Equal(GatewayCursorPageState.Invalid, store.Read<string>("owner/device", "/v1/display-profiles", "includeArtwork=false", first.NextCursor!, 1, now).State);
        Assert.Equal(GatewayCursorPageState.Invalid, store.Read<string>("owner/device", "/v1/shortcuts", "includeArtwork=true", first.NextCursor!, 1, now).State);

        GatewayCursorPageResult<string> second = store.Read<string>("owner/device", "/v1/shortcuts", "includeArtwork=false", first.NextCursor!, 1, now);
        Assert.Equal("second", Assert.Single(second.Items));
        GatewayCursorPageResult<string> third = store.Read<string>("owner/device", "/v1/shortcuts", "includeArtwork=false", second.NextCursor!, 1, now);
        Assert.Equal("third", Assert.Single(third.Items));
        Assert.Null(third.NextCursor);
    }

    [Fact]
    public void InvalidAndExpiredCursorsHaveDistinctStates()
    {
        GatewayCursorPageStore store = new GatewayCursorPageStore();
        DateTime now = DateTime.UtcNow;
        GatewayCursorPageResult<int> first = store.Create("owner/device", "/v1/operations", "", new[] { 1, 2 }, 1, now);
        string cursor = first.NextCursor!;
        string tampered = cursor[..^1] + (cursor[^1] == 'A' ? 'B' : 'A');

        Assert.Equal(GatewayCursorPageState.Invalid, store.Read<int>("owner/device", "/v1/operations", "", "not-a-cursor", 1, now).State);
        Assert.Equal(GatewayCursorPageState.Invalid, store.Read<int>("owner/device", "/v1/operations", "", tampered, 1, now).State);
        Assert.Equal(GatewayCursorPageState.Invalid, store.Read<int>("owner/device", "/v1/operations", "", cursor, 101, now).State);
        Assert.Equal(GatewayCursorPageState.Success, store.Read<int>("owner/device", "/v1/operations", "", cursor, 1, now.AddMinutes(9)).State);
        Assert.Equal(GatewayCursorPageState.Expired, store.Read<int>("owner/device", "/v1/operations", "", cursor, 1, now.AddMinutes(10)).State);
    }

    [Fact]
    public void NinthSnapshotEvictsTheOldestForThatDevice()
    {
        GatewayCursorPageStore store = new GatewayCursorPageStore();
        DateTime now = DateTime.UtcNow;
        string oldestCursor = store.Create("owner/device", "/v1/shortcuts", "", new[] { "one", "two" }, 1, now).NextCursor!;

        for (int index = 1; index < 9; index++)
        {
            GatewayCursorPageResult<string> created = store.Create("owner/device", "/v1/shortcuts", "", new[] { "one", "two" }, 1, now.AddSeconds(index));
            Assert.Equal(GatewayCursorPageState.Success, created.State);
            Assert.NotNull(created.NextCursor);
        }

        Assert.Equal(GatewayCursorPageState.Expired, store.Read<string>("owner/device", "/v1/shortcuts", "", oldestCursor, 1, now.AddSeconds(9)).State);
    }
}
