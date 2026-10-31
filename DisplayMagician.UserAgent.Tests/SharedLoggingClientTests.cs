using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class SharedLoggingClientTests
{
    [Fact]
    public async Task AgentUsesServiceSnapshotIncludingGlobalDiagnosticSessions()
    {
        SharedLoggingSettings settings = new SharedLoggingSettings { SavedLevel = "Error", Sessions = new[]
        {
            new DiagnosticLoggingSession { OwnerId = Guid.NewGuid(), UserSid = "user", Level = "Trace", ExpiresUtc = DateTime.UtcNow.AddMinutes(1) }
        }};
        AgentClient client = new AgentClient(new ControlResponse { IsSuccessful = true, LoggingSettings = settings });
        SharedLoggingSettings received = await client.GetLoggingSettingsAsync(CancellationToken.None);
        Assert.Equal(ControlMessageType.GetLoggingSettings, client.Request!.MessageType);
        Assert.Equal("Trace", received.GetEffectiveLevel(DateTime.UtcNow));
        Assert.Equal("Error", received.GetEffectiveLevel(DateTime.UtcNow.AddMinutes(2)));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task AgentRejectsFailedMissingOrInvalidSnapshots(bool success, bool invalid)
    {
        AgentClient client = new AgentClient(new ControlResponse { IsSuccessful = success,
            LoggingSettings = invalid ? new SharedLoggingSettings { SchemaVersion = 2 } : null });
        await Assert.ThrowsAsync<InvalidDataException>(() => client.GetLoggingSettingsAsync(CancellationToken.None));
    }

    [Fact]
    public async Task DesktopUsesSeparateSavedLevelAndRenewalRequests()
    {
        DesktopClient client = new DesktopClient();
        await client.SetSharedLogLevelAsync("Warn", true, CancellationToken.None);
        SetSharedLogLevelRequest saved = JsonSerializer.Deserialize<SetSharedLogLevelRequest>(client.Request!.Payload)!;
        Assert.Equal(ControlMessageType.SetSharedLogLevel, client.Request.MessageType);
        Assert.True(saved.InitializeOnly);
        Guid owner = Guid.NewGuid();
        await client.SetTemporaryDiagnosticLogLevelAsync("Trace", owner, CancellationToken.None);
        TemporaryDiagnosticLogLevelRequest session = JsonSerializer.Deserialize<TemporaryDiagnosticLogLevelRequest>(client.Request!.Payload)!;
        Assert.Equal(ControlMessageType.SetTemporaryDiagnosticLogLevel, client.Request.MessageType);
        Assert.Equal(owner, session.OwnerId);
        Assert.Equal(1, session.DurationMinutes);
        await client.ReleaseTemporaryDiagnosticLogLevelAsync(owner, CancellationToken.None);
        Assert.Equal(owner, JsonSerializer.Deserialize<ReleaseTemporaryDiagnosticLogLevelRequest>(client.Request!.Payload)!.OwnerId);
    }

    private sealed class AgentClient : ControlServiceClient
    {
        private readonly ControlResponse _response;
        public ControlEnvelope? Request { get; private set; }
        public AgentClient(ControlResponse response) { _response = response; }
        protected override Task<ControlResponse> SendPersistentAsync(ControlEnvelope request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(_response);
        }
    }

    private sealed class DesktopClient : DisplayMagician.ControlServicePipeClient
    {
        public ControlEnvelope? Request { get; private set; }
        protected override Task<ControlResponse> SendAsync(ControlEnvelope request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new ControlResponse { IsSuccessful = true, LoggingSettings = new SharedLoggingSettings() });
        }
    }
}
