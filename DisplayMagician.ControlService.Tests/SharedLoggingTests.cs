using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using DisplayMagician.Contracts;
using NLog;
using NLog.Config;
using NLog.Targets;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class SharedLoggingTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"DisplayMagician-SharedLogging-{Guid.NewGuid():N}");
    private StoragePaths Paths => new StoragePaths(_root);
    private string StatePath => Path.Combine(Paths.MachineDiagnosticsPath, "LoggingSettings.json");
    private static readonly DateTime Now = new DateTime(2026, 10, 28, 0, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("Trace")]
    [InlineData("Debug")]
    [InlineData("Info")]
    [InlineData("Warn")]
    [InlineData("Error")]
    [InlineData("Fatal")]
    public void SavedBaselinePersistsAndLaterStartupCannotResetIt(string level)
    {
        MachineDiagnosticLogLevelStore store = new MachineDiagnosticLogLevelStore(Paths);
        store.SetSavedLevel(new SetSharedLogLevelRequest { Level = level });
        store = new MachineDiagnosticLogLevelStore(Paths);
        store.SetSavedLevel(new SetSharedLogLevelRequest { Level = "Warn", InitializeOnly = true });
        Assert.Equal(level, store.GetActiveLevel(Now));
        Assert.Equal(level, store.GetSettings().SavedLevel);
    }

    [Fact]
    public void InitialImportUsesExistingUserChoiceOnceAndExplicitChangesStillWin()
    {
        MachineDiagnosticLogLevelStore store = new MachineDiagnosticLogLevelStore(Paths);
        store.SetSavedLevel(new SetSharedLogLevelRequest { Level = "Error", InitializeOnly = true });
        store.SetSavedLevel(new SetSharedLogLevelRequest { Level = "Debug", InitializeOnly = true });
        Assert.Equal("Error", store.GetActiveLevel(Now));
        store.SetSavedLevel(new SetSharedLogLevelRequest { Level = "Info" });
        Assert.Equal("Info", new MachineDiagnosticLogLevelStore(Paths).GetActiveLevel(Now));
    }

    [Fact]
    public void OverlappingSessionsUseMostVerboseAndReleaseRestoresRemainingThenBaseline()
    {
        MachineDiagnosticLogLevelStore store = new MachineDiagnosticLogLevelStore(Paths);
        Guid debug = Guid.NewGuid(), trace = Guid.NewGuid();
        store.Set(Request(debug, "Debug"), Now, "user", 1);
        store.Set(Request(trace, "Trace"), Now, "user", 2);
        Assert.Equal("Trace", store.GetActiveLevel(Now));
        Assert.Equal("Debug", store.Release(trace, Now, "user", 2));
        Assert.Equal("Warn", store.Release(debug, Now, "user", 1));
    }

    [Fact]
    public void RenewalKeepsSessionAliveBeyondThirtyMinutesAndCrashExpiresAtDeadline()
    {
        MachineDiagnosticLogLevelStore store = new MachineDiagnosticLogLevelStore(Paths);
        Guid owner = Guid.NewGuid();
        for (int seconds = 0; seconds <= 3600; seconds += 15)
            store.Set(Request(owner, "Debug"), Now.AddSeconds(seconds), "user", 1);
        Assert.Single(store.GetSettings().Sessions);
        Assert.Equal("Debug", store.GetActiveLevel(Now.AddSeconds(3659)));
        Assert.Equal("Warn", store.GetActiveLevel(Now.AddSeconds(3660)));
    }

    [Fact]
    public void RestartAndDisconnectedCachedSnapshotRespectExpiryWithoutWriting()
    {
        MachineDiagnosticLogLevelStore store = new MachineDiagnosticLogLevelStore(Paths);
        store.Set(Request(Guid.NewGuid(), "Trace"), Now, "user", 1);
        SharedLoggingSettings cached = store.GetSettings();
        store = new MachineDiagnosticLogLevelStore(Paths);
        Assert.Equal("Trace", store.GetActiveLevel(Now.AddSeconds(59)));
        using FileStream locked = new FileStream(StatePath, FileMode.Open, FileAccess.Read, FileShare.None);
        Assert.Equal("Warn", store.GetActiveLevel(Now.AddMinutes(1)));
        Assert.Equal("Warn", cached.GetEffectiveLevel(Now.AddMinutes(1)));
    }

    [Fact]
    public void BaselineChangesDuringDiagnosticsApplyAfterSessionEnds()
    {
        MachineDiagnosticLogLevelStore store = new MachineDiagnosticLogLevelStore(Paths);
        Guid owner = Guid.NewGuid();
        store.Set(Request(owner, "Debug"), Now, "user", 1);
        store.SetSavedLevel(new SetSharedLogLevelRequest { Level = "Fatal" });
        Assert.Equal("Debug", store.GetActiveLevel(Now));
        Assert.Equal("Fatal", store.Release(owner, Now, "user", 1));
    }

    [Theory]
    [InlineData("other-user", 1)]
    [InlineData("user", 2)]
    public void OtherCallersCannotRenewOrReleaseSession(string user, int session)
    {
        MachineDiagnosticLogLevelStore store = new MachineDiagnosticLogLevelStore(Paths);
        Guid owner = Guid.NewGuid();
        store.Set(Request(owner, "Debug"), Now, "user", 1);
        Assert.Throws<UnauthorizedAccessException>(() => store.Set(Request(owner, "Trace"), Now, user, session));
        Assert.Throws<UnauthorizedAccessException>(() => store.Release(owner, Now, user, session));
        Assert.Equal("Debug", store.GetActiveLevel(Now));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{")]
    [InlineData("{\"SchemaVersion\":2,\"SavedLevel\":\"Warn\",\"HasSavedLevel\":true,\"Sessions\":[]}")]
    [InlineData("{\"SchemaVersion\":1,\"SavedLevel\":\"Off\",\"HasSavedLevel\":true,\"Sessions\":[]}")]
    public void InvalidStatePreservesEvidenceAndRejectsWritesWhileDefaultRemainsAvailable(string json)
    {
        Directory.CreateDirectory(Paths.MachineDiagnosticsPath);
        File.WriteAllText(StatePath, json);
        File.WriteAllText(StatePath + ".bak", "preserve backup");
        MachineDiagnosticLogLevelStore store = new MachineDiagnosticLogLevelStore(Paths);
        Assert.Equal("Warn", store.GetActiveLevel(Now));
        Assert.Throws<IOException>(() => store.SetSavedLevel(new SetSharedLogLevelRequest { Level = "Debug" }));
        Assert.Throws<IOException>(() => store.Set(Request(Guid.NewGuid(), "Debug"), Now));
        Assert.Equal(json, File.ReadAllText(StatePath));
        Assert.Equal("preserve backup", File.ReadAllText(StatePath + ".bak"));
    }

    [Fact]
    public void FailedSavedLevelAndRenewalRetainPriorBaselineAndExpiry()
    {
        MachineDiagnosticLogLevelStore store = new MachineDiagnosticLogLevelStore(Paths);
        Guid owner = Guid.NewGuid();
        store.Set(Request(owner, "Debug"), Now);
        using (FileStream locked = new FileStream(StatePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Assert.ThrowsAny<IOException>(() => store.SetSavedLevel(new SetSharedLogLevelRequest { Level = "Fatal" }));
            Assert.ThrowsAny<IOException>(() => store.Set(Request(owner, "Debug"), Now.AddSeconds(30)));
        }
        Assert.Equal("Warn", store.GetSettings().SavedLevel);
        Assert.Equal("Warn", store.GetActiveLevel(Now.AddSeconds(60)));
    }

    [Fact]
    public void ReadersReceiveIndependentSnapshots()
    {
        MachineDiagnosticLogLevelStore store = new MachineDiagnosticLogLevelStore(Paths);
        store.Set(Request(Guid.NewGuid(), "Debug"), Now);
        SharedLoggingSettings snapshot = store.GetSettings();
        snapshot.SavedLevel = "Trace";
        snapshot.Sessions[0].Level = "Trace";
        Assert.Equal("Debug", store.GetActiveLevel(Now));
    }

    [Theory]
    [InlineData("Trace", 6)]
    [InlineData("Debug", 5)]
    [InlineData("Info", 4)]
    [InlineData("Warn", 3)]
    [InlineData("Error", 2)]
    [InlineData("Fatal", 1)]
    public void ApplyingLevelFiltersTargetAndPreservesOtherTargets(string level, int count)
    {
        using LogFactory factory = new LogFactory();
        MemoryTarget target = new MemoryTarget("shared"), other = new MemoryTarget("other");
        LoggingConfiguration configuration = new LoggingConfiguration();
        configuration.AddRule(LogLevel.Trace, LogLevel.Fatal, target);
        configuration.AddRule(LogLevel.Trace, LogLevel.Fatal, other);
        factory.Configuration = configuration;
        SharedLoggingSettings.ApplyToTarget(factory, "shared", level);
        Logger logger = factory.GetLogger("test");
        foreach (LogLevel candidate in LogLevel.AllLoggingLevels) logger.Log(candidate, "SharedLoggingTests/ApplyingLevel: test event");
        Assert.Equal(count, target.Logs.Count);
        Assert.Equal(6, other.Logs.Count);
    }

    private static TemporaryDiagnosticLogLevelRequest Request(Guid owner, string level)
        => new TemporaryDiagnosticLogLevelRequest { OwnerId = owner, Level = level, DurationMinutes = 1 };

    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
}
