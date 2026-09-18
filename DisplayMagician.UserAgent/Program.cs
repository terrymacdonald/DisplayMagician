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
        UserAgentStartupRequest startupRequest = UserAgentCommandLine.Parse(args);
        if (startupRequest.Action == UserAgentStartupAction.RegisterOnce)
        {
            cancellationTokenSource.CancelAfter(10000);
            ControlResponse response = await serviceClient.RegisterOnceAsync(registration, cancellationTokenSource.Token).ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                throw new System.InvalidOperationException(response.Message);
            }

            return;
        }

        if (startupRequest.Action == UserAgentStartupAction.ApplyDisplayProfile)
        {
            string displayMagicianExecutablePath = Path.Combine(AppContext.BaseDirectory, "DisplayMagician.exe");
            if (!File.Exists(displayMagicianExecutablePath))
            {
                throw new FileNotFoundException("The User Agent could not find DisplayMagician.exe beside itself.", displayMagicianExecutablePath);
            }

            int exitCode = await serviceClient.ApplyDisplayProfileAsync(registration, startupRequest.ProfileId!, displayMagicianExecutablePath, cancellationTokenSource.Token).ConfigureAwait(false);
            Environment.ExitCode = exitCode;
            return;
        }

        bool acquireDisplayControl = startupRequest.Action == UserAgentStartupAction.AcquireDisplayControl;
        bool migrateUserData = startupRequest.Action == UserAgentStartupAction.MigrateUserData;
        await serviceClient.RunAsync(registration, System.TimeSpan.FromSeconds(15), acquireDisplayControl, migrateUserData, cancellationTokenSource.Token).ConfigureAwait(false);
    }
}
