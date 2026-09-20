using System;
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
        AgentRegistration registration = AgentIdentity.CreateRegistration(AgentBuildVersion.Current, "manual");
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

        bool acquireDisplayControl = startupRequest.Action == UserAgentStartupAction.AcquireDisplayControl;
        bool migrateUserData = true;
        ProfileCommandHandler profileCommandHandler = new ProfileCommandHandler(registration);
        TaskCompletionSource<ControlResponse> migrationCompletion = new TaskCompletionSource<ControlResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        AgentCommandServer commandServer = new AgentCommandServer(registration.CommandPipeName);
        Task serviceConnection = serviceClient.RunAsync(registration, System.TimeSpan.FromSeconds(15), acquireDisplayControl, migrateUserData, migrationCompletion, cancellationTokenSource.Token);
        ControlResponse migrationResponse = await migrationCompletion.Task.ConfigureAwait(false);
        if (!migrationResponse.IsSuccessful)
        {
            cancellationTokenSource.Cancel();
            throw new InvalidOperationException(migrationResponse.Message);
        }

        await profileCommandHandler.RestorePendingShortcutRecoveryAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        await serviceClient.ReportAgentOperationStateAsync(registration, AgentOperationState.Idle, cancellationTokenSource.Token).ConfigureAwait(false);
        Task commandConnection = commandServer.RunAsync(profileCommandHandler.HandleAsync, () => profileCommandHandler.StopRequested, cancellationTokenSource.Token);
        AutomaticGameDetectionWorker automaticGameDetectionWorker = new AutomaticGameDetectionWorker(profileCommandHandler.AutomaticGameDetectionRegistry, profileCommandHandler.ShortcutRunner, serviceClient, registration);
        Task automaticGameDetection = automaticGameDetectionWorker.RunAsync(cancellationTokenSource.Token);

        await Task.WhenAny(serviceConnection, commandConnection, automaticGameDetection).ConfigureAwait(false);
        cancellationTokenSource.Cancel();
        await Task.WhenAll(serviceConnection, commandConnection, automaticGameDetection).ConfigureAwait(false);
    }
}
