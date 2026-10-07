using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class AutomaticGameDetectionStateTrackerTests
{
    [Fact]
    public void HasNewlyStarted_IgnoresTheInitialStateAndDetectsLaterStarts()
    {
        AutomaticGameDetectionStateTracker tracker = new AutomaticGameDetectionStateTracker();

        Assert.False(tracker.HasNewlyStarted("shortcut-id", true));
        Assert.False(tracker.HasNewlyStarted("shortcut-id", true));
        Assert.False(tracker.HasNewlyStarted("shortcut-id", false));
        Assert.True(tracker.HasNewlyStarted("shortcut-id", true));
        Assert.False(tracker.HasNewlyStarted("shortcut-id", true));
    }
}