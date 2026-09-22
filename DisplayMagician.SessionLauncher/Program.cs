using System;
using System.IO;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace DisplayMagician.SessionLauncher;

internal static class Program
{
    private static async Task Main()
    {
        ConfigureLogging();
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Services.AddWindowsService(options => options.ServiceName = "DisplayMagician Session Launcher");
        builder.Services.AddSingleton<InteractiveUserProcessLauncher>();
        builder.Services.AddSingleton<SessionLauncherPipeServer>();
        builder.Services.AddHostedService<SessionLauncherWorker>();

        using IHost host = builder.Build();
        await host.RunAsync().ConfigureAwait(false);
    }

    private static void ConfigureLogging()
    {
        try
        {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician", "Machine", "Logs");
            Directory.CreateDirectory(logPath);
            SupportLogLayout.Register();
            LoggingConfiguration configuration = new LoggingConfiguration();
            FileTarget logFile = new FileTarget("session-launcher-log")
            {
                FileName = Path.Combine(logPath, "SessionLauncher-${shortdate}.log"),
                ArchiveAboveSize = 41943040,
                MaxArchiveFiles = 7,
                Layout = "${displaymagicianlog:component=SessionLauncher}"
            };
            configuration.AddRule(LogLevel.Info, LogLevel.Fatal, logFile);
            LogManager.Configuration = configuration;
            LogManager.GetCurrentClassLogger().Info("SessionLauncher/ConfigureLogging: Session Launcher logging started at {0}.", logPath);
        }
        catch
        {
            // The service must still start to report installer or ACL failures through the Service Control Manager.
        }
    }
}
