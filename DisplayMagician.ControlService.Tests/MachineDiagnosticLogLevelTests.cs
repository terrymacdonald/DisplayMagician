using System;
using System.IO;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class MachineDiagnosticLogLevelTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-Diagnostics-{Guid.NewGuid():N}");
    private StoragePaths Paths => new StoragePaths(_root);
    private string StatePath => Path.Combine(Paths.MachineDiagnosticsPath, "LoggingSettings.json");

    [Fact]
    public void FailedSet_DoesNotReplaceCurrentLeaseOrReleaseOwner()
    {
        MachineDiagnosticLogLevelStore store = new MachineDiagnosticLogLevelStore(Paths);
        DateTime now = DateTime.UtcNow;
        Guid owner = Guid.NewGuid();
        store.Set(Request(owner), now);
        using (FileStream locked = new FileStream(StatePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Assert.ThrowsAny<IOException>(() => store.Set(Request(Guid.NewGuid(), "Trace"), now.AddSeconds(1)));
            Assert.Equal("Debug", store.GetActiveLevel(now.AddSeconds(2)));
        }

        Assert.Equal("Warn", store.Release(owner, now.AddSeconds(3)));
        Assert.Equal("Warn", new MachineDiagnosticLogLevelStore(Paths).GetActiveLevel(now.AddSeconds(3)));
    }

    [Fact]
    public void FailedRelease_PreservesLeaseUntilItCanBeReleased()
    {
        MachineDiagnosticLogLevelStore store = new MachineDiagnosticLogLevelStore(Paths);
        DateTime now = DateTime.UtcNow;
        Guid owner = Guid.NewGuid();
        store.Set(Request(owner), now);
        using (FileStream locked = new FileStream(StatePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Assert.ThrowsAny<IOException>(() => store.Release(owner, now.AddSeconds(1)));
            Assert.Equal("Debug", store.GetActiveLevel(now.AddSeconds(2)));
        }

        Assert.Equal("Warn", store.Release(owner, now.AddSeconds(3)));
        Assert.Equal("Warn", new MachineDiagnosticLogLevelStore(Paths).GetActiveLevel(now.AddSeconds(3)));
    }

    [Fact]
    public void AnotherOwnerCannotReleaseAnActiveLease()
    {
        MachineDiagnosticLogLevelStore store = new MachineDiagnosticLogLevelStore(Paths);
        DateTime now = DateTime.UtcNow;
        store.Set(Request(Guid.NewGuid()), now);
        Assert.Equal("Debug", store.Release(Guid.NewGuid(), now.AddSeconds(1)));
        Assert.Equal("Debug", new MachineDiagnosticLogLevelStore(Paths).GetActiveLevel(now.AddSeconds(1)));
    }

    private static TemporaryDiagnosticLogLevelRequest Request(Guid owner, string level = "Debug")
        => new TemporaryDiagnosticLogLevelRequest { OwnerId = owner, Level = level, DurationMinutes = 1 };

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}
