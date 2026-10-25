using System;
using DisplayMagician.Processes;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class AutomaticGameDetectionStateTrackerTests
{
    [Fact]
    public void Observe_IgnoresExistingCopyAndRetainsNewestPendingIdentity()
    {
        AutomaticGameDetectionStateTracker tracker = new AutomaticGameDetectionStateTracker();
        ProcessIdentity old = new ProcessIdentity(10, DateTime.UtcNow.AddMinutes(-10));
        ProcessIdentity newer = new ProcessIdentity(11, DateTime.UtcNow.AddMinutes(-1));
        ProcessIdentity newest = new ProcessIdentity(12, DateTime.UtcNow);

        Assert.Null(tracker.Observe("shortcut-id", new[] { old }));
        Assert.Equal(newest, tracker.Observe("shortcut-id", new[] { old, newer, newest }));
        Assert.Equal(newest, tracker.Observe("shortcut-id", new[] { old, newer, newest }));
        tracker.MarkHandled("shortcut-id", newest);
        Assert.Null(tracker.Observe("shortcut-id", new[] { old, newer }));
        Assert.Null(tracker.Observe("shortcut-id", new[] { old }));
        ProcessIdentity next = new ProcessIdentity(13, DateTime.UtcNow.AddMinutes(1));
        Assert.Equal(next, tracker.Observe("shortcut-id", new[] { old, next }));
    }

    [Fact]
    public void Observe_KeepsDeferredProcessPendingUntilItExitsOrIsHandled()
    {
        AutomaticGameDetectionStateTracker tracker = new AutomaticGameDetectionStateTracker();
        ProcessIdentity detected = new ProcessIdentity(20, DateTime.UtcNow);
        Assert.Null(tracker.Observe("shortcut-id", Array.Empty<ProcessIdentity>()));
        Assert.Equal(detected, tracker.Observe("shortcut-id", new[] { detected }));
        Assert.Equal(detected, tracker.Observe("shortcut-id", new[] { detected }));
        Assert.Null(tracker.Observe("shortcut-id", Array.Empty<ProcessIdentity>()));
    }
}
