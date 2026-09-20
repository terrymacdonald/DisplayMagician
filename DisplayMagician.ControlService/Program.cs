using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DisplayMagician.ControlService;

internal static class Program
{
    private static async Task Main()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Services.AddWindowsService(options => options.ServiceName = "DisplayMagician Control Service");
        builder.Services.AddSingleton<StoragePaths>();
        builder.Services.AddSingleton<DisplayControlLeaseStore>();
        builder.Services.AddSingleton<LegacyFileMigration>();
        builder.Services.AddSingleton<UserDataMigrationRunner>();
        builder.Services.AddSingleton<ControlStateCoordinator>();
        builder.Services.AddSingleton<OperationStatusStore>();
        builder.Services.AddSingleton<IAgentCommandClient, AgentCommandClient>();
        builder.Services.AddSingleton<ISessionLauncherClient, SessionLauncherClient>();
        builder.Services.AddSingleton<ProfileOperationRouter>();
        builder.Services.AddSingleton<NamedPipeControlServer>();
        builder.Services.AddSingleton<ControlClientPipeServer>();
        builder.Services.AddHostedService<ControlServiceWorker>();

        using IHost host = builder.Build();
        await host.RunAsync().ConfigureAwait(false);
    }
}
