using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class AnonymousMetricsSenderTests
{
    [Fact]
    public async Task TrySendAsync_SendsMachineOwnedMetricsAndSchedulesTheNextHeartbeat()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            MachineScheduleCoordinator coordinator = new MachineScheduleCoordinator(new MachineScheduleStore(new StoragePaths(storageRoot)));
            coordinator.InitializeAnonymousMetrics(new InitializeAnonymousMetricsRequest { InstallId = Guid.NewGuid().ToString(), ShareAnonymousUsageMetrics = true });
            coordinator.RecordAnonymousMetricsUsage(new AnonymousMetricsUsageReport { AppVersion = "4.0.0.1", UpdateChannel = "stable", IsLaunch = true, ActiveMinutes = 12 });
            RecordingHandler handler = new RecordingHandler();
            using HttpClient httpClient = new HttpClient(handler);
            AnonymousMetricsSender sender = new AnonymousMetricsSender(httpClient, coordinator);

            await sender.TrySendAsync(CancellationToken.None);

            Assert.NotNull(handler.Payload);
            Assert.Equal(1, handler.Payload!.RootElement.GetProperty("schemaVersion").GetInt32());
            Assert.Equal("4.0.0.1", handler.Payload.RootElement.GetProperty("appVersion").GetString());
            Assert.Equal(1, handler.Payload.RootElement.GetProperty("launches").GetInt64());
            Assert.Equal(12, handler.Payload.RootElement.GetProperty("activeMinutes").GetInt64());
            MachineScheduleState state = coordinator.GetState();
            Assert.Equal("4.0.0.1", state.LastMetricsReportedVersion);
            Assert.NotNull(state.NextMetricsHeartbeatUtc);
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
    public void InitializeAnonymousMetrics_ImportsLegacyStateOnlyOnce()
    {
        string storageRoot = Path.Combine(Path.GetTempPath(), "DisplayMagicianTests", Guid.NewGuid().ToString("N"));
        try
        {
            MachineScheduleCoordinator coordinator = new MachineScheduleCoordinator(new MachineScheduleStore(new StoragePaths(storageRoot)));
            string installId = Guid.NewGuid().ToString();
            coordinator.InitializeAnonymousMetrics(new InitializeAnonymousMetricsRequest { InstallId = installId, ShareAnonymousUsageMetrics = false, Launches = 5, ActiveMinutes = 9 });
            coordinator.InitializeAnonymousMetrics(new InitializeAnonymousMetricsRequest { InstallId = Guid.NewGuid().ToString(), ShareAnonymousUsageMetrics = true, Launches = 1, ActiveMinutes = 1 });

            MachineScheduleState state = coordinator.GetState();
            Assert.Equal(installId, state.InstallId);
            Assert.False(state.ShareAnonymousUsageMetrics);
            Assert.Equal(5, state.TotalAnonymousMetricLaunches);
            Assert.Equal(9, state.TotalAnonymousMetricActiveMinutes);
        }
        finally
        {
            if (Directory.Exists(storageRoot))
            {
                Directory.Delete(storageRoot, recursive: true);
            }
        }
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public JsonDocument? Payload { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Payload = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}