using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagician.UserAgent;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        // The current WinForms application still owns desktop work. This executable becomes its interactive-session host
        // once the existing profile and shortcut lifecycle is moved behind the Agent boundary.
        AgentRegistration registration = AgentIdentity.CreateRegistration("4.0.0-development", "manual");
        ControlServiceClient serviceClient = new ControlServiceClient();
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        if (args.Length == 1 && args[0] == "--once")
        {
            cancellationTokenSource.CancelAfter(10000);
            ControlResponse response = await serviceClient.RegisterOnceAsync(registration, cancellationTokenSource.Token).ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                throw new System.InvalidOperationException(response.Message);
            }

            return;
        }

        if (args.Length == 2 && args[0] == "--apply-profile")
        {
            string displayMagicianExecutablePath = Path.Combine(AppContext.BaseDirectory, "DisplayMagician.exe");
            if (!File.Exists(displayMagicianExecutablePath))
            {
                throw new FileNotFoundException("The User Agent could not find DisplayMagician.exe beside itself.", displayMagicianExecutablePath);
            }

            int exitCode = await serviceClient.ApplyDisplayProfileAsync(registration, args[1], displayMagicianExecutablePath, cancellationTokenSource.Token).ConfigureAwait(false);
            Environment.ExitCode = exitCode;
            return;
        }

        bool acquireDisplayControl = args.Length == 1 && args[0] == "--acquire-display-control";
        bool migrateUserData = args.Length == 1 && args[0] == "--migrate-user-data";
        await serviceClient.RunAsync(registration, System.TimeSpan.FromSeconds(15), acquireDisplayControl, migrateUserData, cancellationTokenSource.Token).ConfigureAwait(false);
    }
}
