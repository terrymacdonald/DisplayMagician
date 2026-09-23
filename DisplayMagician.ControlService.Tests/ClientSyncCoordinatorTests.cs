using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class ClientSyncCoordinatorTests
{
    [Fact]
    public async Task SyncAsync_DownloadsOneCombinedDocumentAndDeliversItsMessageSnapshotToRegisteredAgents()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            ControlStateCoordinator stateCoordinator = new ControlStateCoordinator();
            AgentRegistration agent = new AgentRegistration { UserSid = "S-1-5-21-100", SessionId = 10, ProcessId = 1000, CommandPipeName = "test-agent-command" };
            stateCoordinator.RegisterAgent(agent, DateTime.UtcNow);
            RecordingAgentCommandClient commandClient = new RecordingAgentCommandClient();
            using HttpClient httpClient = new HttpClient(new StaticDocumentHandler(CreateDocument()));
            ClientSyncCoordinator coordinator = new ClientSyncCoordinator(httpClient, new MachineScheduleCoordinator(new MachineScheduleStore(new StoragePaths(storageRoot))), stateCoordinator, commandClient);

            ClientSyncResult result = await coordinator.SyncAsync(new ClientSyncRequest { IsManual = true }, agent.UserSid, agent.SessionId, CancellationToken.None);

            Assert.True(result.WasDue);
            Assert.Equal("4.1.2.3", result.StableUpdate!.Version);
            Assert.Equal("4.2.0.0", result.PrereleaseUpdate!.Version);
            Assert.True(commandClient.WasCalled);
            Assert.Equal(agent.CommandPipeName, commandClient.Agent!.CommandPipeName);
            Assert.Equal(ControlMessageType.ApplyClientSyncMessages, commandClient.Request!.MessageType);
            ClientSyncMessageManifest? manifest = JsonSerializer.Deserialize<ClientSyncMessageManifest>(commandClient.Request.Payload);
            Assert.NotNull(manifest);
            Assert.Single(manifest.Messages);
            Assert.Equal("published", manifest.Messages[0].Status);
        }
        finally
        {
            if (Directory.Exists(storageRoot))
            {
                Directory.Delete(storageRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task SyncAsync_TreatsAnHttpTimeoutAsARecoverableFailure()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            using HttpClient httpClient = new HttpClient(new TimeoutHandler());
            ClientSyncCoordinator coordinator = new ClientSyncCoordinator(httpClient, new MachineScheduleCoordinator(new MachineScheduleStore(new StoragePaths(storageRoot))), new ControlStateCoordinator(), new RecordingAgentCommandClient());

            ClientSyncResult result = await coordinator.SyncAsync(new ClientSyncRequest { IsManual = true }, null, null, CancellationToken.None);

            Assert.False(result.WasDue);
        }
        finally
        {
            if (Directory.Exists(storageRoot))
            {
                Directory.Delete(storageRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task SyncAsync_TreatsAnUnavailableScheduleStoreAsARecoverableFailure()
    {
        string storageRoot = Path.GetTempFileName();
        try
        {
            using HttpClient httpClient = new HttpClient(new StaticDocumentHandler(CreateDocument()));
            ClientSyncCoordinator coordinator = new ClientSyncCoordinator(httpClient, new MachineScheduleCoordinator(new MachineScheduleStore(new StoragePaths(storageRoot))), new ControlStateCoordinator(), new RecordingAgentCommandClient());

            ClientSyncResult result = await coordinator.SyncAsync(new ClientSyncRequest(), null, null, CancellationToken.None);

            Assert.False(result.WasDue);
        }
        finally
        {
            File.Delete(storageRoot);
        }
    }

    private static string CreateDocument()
    {
        const string hash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
        return $$"""
            {
              "schemaVersion": 1,
              "publishedUtc": "2026-09-21T12:00:00Z",
              "updates": {
                "stable": { "version": "4.1.2.3", "url": "https://downloads.displaymagician.com/stable.exe", "changelog": "https://displaymagician.com/stable", "mandatory": { "value": false, "mode": 0 }, "checksum": { "value": "{{hash}}", "hashingAlgorithm": "SHA256" } },
                "prerelease": { "version": "4.2.0.0", "url": "https://downloads.displaymagician.com/prerelease.exe", "changelog": "https://displaymagician.com/prerelease", "mandatory": { "value": false, "mode": 0 }, "checksum": { "value": "{{hash}}", "hashingAlgorithm": "SHA256" } }
              },
              "messages": [ { "id": "11111111-1111-1111-1111-111111111111", "status": "published", "title": "Test", "url": "/sync/messages/test.md", "format": "md", "sha256": "{{hash}}" } ]
            }
            """;
    }

    private sealed class StaticDocumentHandler : HttpMessageHandler
    {
        private readonly string _document;

        public StaticDocumentHandler(string document)
        {
            _document = document;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(_document, Encoding.UTF8, "application/json") });
        }
    }

    private sealed class TimeoutHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromCanceled<HttpResponseMessage>(new CancellationToken(canceled: true));
        }
    }

    private sealed class RecordingAgentCommandClient : IAgentCommandClient
    {
        public bool WasCalled { get; private set; }
        public AgentRegistration? Agent { get; private set; }
        public ControlEnvelope? Request { get; private set; }

        public Task<ControlResponse> SendAsync(AgentRegistration agent, ControlEnvelope request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            Agent = agent;
            Request = request;
            return Task.FromResult(new ControlResponse { IsSuccessful = true, MessageSync = new MessageSyncResult { IsSuccessful = true } });
        }
    }
}
