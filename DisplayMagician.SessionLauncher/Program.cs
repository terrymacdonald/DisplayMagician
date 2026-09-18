using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DisplayMagician.SessionLauncher;

internal static class Program
{
    private static async Task Main()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Services.AddWindowsService(options => options.ServiceName = "DisplayMagician Session Launcher");
        builder.Services.AddSingleton<InteractiveUserProcessLauncher>();
        builder.Services.AddSingleton<SessionLauncherPipeServer>();
        builder.Services.AddHostedService<SessionLauncherWorker>();

        using IHost host = builder.Build();
        await host.RunAsync().ConfigureAwait(false);
    }
}
