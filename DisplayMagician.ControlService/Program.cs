using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace DisplayMagician.ControlService;

internal static class Program
{
    private static async Task Main()
    {
        StoragePaths storagePaths = new StoragePaths();
        ConfigureLogging(storagePaths);
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Services.AddWindowsService(options => options.ServiceName = "DisplayMagician Control Service");
        builder.Services.AddSingleton(storagePaths);
        builder.Services.AddSingleton<MachineScheduleStore>();
        builder.Services.AddSingleton<MachineScheduleCoordinator>();
        builder.Services.AddSingleton<AnonymousMetricsSender>();
        builder.Services.AddSingleton<DisplayControlLeaseStore>();
        builder.Services.AddSingleton<AuditStore>();
        builder.Services.AddSingleton<RecoveryAdministrationStore>();
        builder.Services.AddSingleton<LegacyFileMigration>();
        builder.Services.AddSingleton<UserDataMigrationRunner>();
        builder.Services.AddSingleton<ControlStateCoordinator>();
        builder.Services.AddSingleton<OperationStatusStore>();
        builder.Services.AddSingleton<OperationDecisionStore>();
        builder.Services.AddSingleton<ControlRequestReplayStore>();
        builder.Services.AddSingleton<ControlClientEventHub>();
        builder.Services.AddSingleton<IAgentCommandClient, AgentCommandClient>();
        builder.Services.AddSingleton(provider => new ClientSyncCoordinator(new System.Net.Http.HttpClient { Timeout = System.TimeSpan.FromSeconds(30) }, provider.GetRequiredService<MachineScheduleCoordinator>(), provider.GetRequiredService<ControlStateCoordinator>(), provider.GetRequiredService<IAgentCommandClient>(), storagePaths, provider.GetRequiredService<ControlClientEventHub>()));
        builder.Services.AddSingleton<ISessionLauncherClient, SessionLauncherClient>();
        builder.Services.AddSingleton<ProfileOperationRouter>();
        builder.Services.AddSingleton<NamedPipeControlServer>();
        builder.Services.AddSingleton<ControlClientPipeServer>();
        builder.Services.AddSingleton<ControlClientEventPipeServer>();
        builder.Services.AddHostedService<ControlServiceWorker>();

        using IHost host = builder.Build();
        await host.RunAsync().ConfigureAwait(false);
    }

    private static void ConfigureLogging(StoragePaths storagePaths)
    {
        try
        {
            storagePaths.EnsureMachineDirectories();
            SupportLogLayout.Register();
            LoggingConfiguration configuration = new LoggingConfiguration();
            FileTarget logFile = new FileTarget("control-service-log")
            {
                FileName = System.IO.Path.Combine(storagePaths.MachineLogsPath, "ControlService-${shortdate}.log"),
                ArchiveAboveSize = 41943040,
                MaxArchiveFiles = 7,
                Layout = "${displaymagicianlog:component=ControlService}"
            };
            LoggingRule loggingRule = new LoggingRule("ControlServiceFileLog");
            loggingRule.EnableLoggingForLevels(LogLevel.Info, LogLevel.Fatal);
            loggingRule.Targets.Add(logFile);
            loggingRule.LoggerNamePattern = "*";
            configuration.LoggingRules.Add(loggingRule);
            LogManager.Configuration = configuration;
        }
        catch
        {
            // The service must still start to report an installer or ACL failure through the Service Control Manager.
        }
    }
}
