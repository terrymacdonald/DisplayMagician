using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace DisplayMagician.UserAgent;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        // The current WinForms application still owns desktop work. This executable becomes its interactive-session host
        // once the existing profile and shortcut lifecycle is moved behind the Agent boundary.
        AgentRegistration registration = AgentIdentity.CreateRegistration(AgentBuildVersion.Current, "manual");
        ConfigureLogging(registration.UserSid);
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
        Task serviceConnection = RunServiceConnectionWithRetryAsync(serviceClient, registration, acquireDisplayControl, migrateUserData, migrationCompletion, cancellationTokenSource.Token);
        ControlResponse migrationResponse = await migrationCompletion.Task.ConfigureAwait(false);
        if (!migrationResponse.IsSuccessful)
        {
            cancellationTokenSource.Cancel();
            throw new InvalidOperationException(migrationResponse.Message);
        }

        await profileCommandHandler.RestorePendingShortcutRecoveryAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        Task commandConnection = commandServer.RunAsync(profileCommandHandler.HandleAsync, () => profileCommandHandler.StopRequested, cancellationTokenSource.Token);
        registration.IsReady = true;
        ControlResponse readyResponse = await serviceClient.RegisterOnceAsync(registration, cancellationTokenSource.Token).ConfigureAwait(false);
        if (!readyResponse.IsSuccessful)
        {
            cancellationTokenSource.Cancel();
            throw new InvalidOperationException(readyResponse.Message);
        }

        await serviceClient.ReportAgentOperationStateAsync(registration, AgentOperationState.Idle, cancellationTokenSource.Token).ConfigureAwait(false);
        Task statusReconciliation = RunOperationStatusReconciliationAsync(profileCommandHandler, cancellationTokenSource.Token);
        AutomaticGameDetectionWorker automaticGameDetectionWorker = new AutomaticGameDetectionWorker(profileCommandHandler.AutomaticGameDetectionRegistry, profileCommandHandler.RunDetectedShortcutAsync);
        Task automaticGameDetection = automaticGameDetectionWorker.RunAsync(cancellationTokenSource.Token);

        await Task.WhenAny(commandConnection, automaticGameDetection, statusReconciliation).ConfigureAwait(false);
        cancellationTokenSource.Cancel();
        await Task.WhenAll(serviceConnection, commandConnection, automaticGameDetection, statusReconciliation).ConfigureAwait(false);
    }

    private static async Task RunOperationStatusReconciliationAsync(ProfileCommandHandler profileCommandHandler, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await profileCommandHandler.FlushPendingOperationStatusUpdatesAsync(cancellationToken).ConfigureAwait(false);
                await profileCommandHandler.ReconcileOperationStatusesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidOperationException || ex is TimeoutException || ex is OperationCanceledException)
            {
                LogManager.GetCurrentClassLogger().Debug(ex, "Program/RunOperationStatusReconciliationAsync: Control Service is unavailable for operation-status reconciliation.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private static async Task RunServiceConnectionWithRetryAsync(ControlServiceClient serviceClient, AgentRegistration registration, bool acquireDisplayControl, bool migrateUserData, TaskCompletionSource<ControlResponse> migrationCompletion, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                bool requiresInitialisation = !migrationCompletion.Task.IsCompleted;
                await serviceClient.RunAsync(registration, TimeSpan.FromSeconds(15), acquireDisplayControl && requiresInitialisation, migrateUserData && requiresInitialisation, migrationCompletion, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Warn(ex, "Program/RunServiceConnectionWithRetryAsync: Control Service connection ended. Retrying shortly without stopping the User Agent.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private static void ConfigureLogging(string userSid)
    {
        string legacyLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician", "Logs");
        string preferredLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician", "Users", userSid, "Logs");
        string logPath = TryPrepareLogPath(preferredLogPath, legacyLogPath) ? preferredLogPath : legacyLogPath;
        try
        {
            Directory.CreateDirectory(logPath);
            SupportLogLayout.Register();
            LoggingConfiguration configuration = new LoggingConfiguration();
            FileTarget fileTarget = new FileTarget("userAgentLog")
            {
                FileName = Path.Combine(logPath, $"UserAgent-{DateTime.UtcNow.ToString("yyyy-MM-dd-HHmm", CultureInfo.InvariantCulture)}.log"),
                MaxArchiveFiles = 4,
                ArchiveAboveSize = 41943040,
                Layout = "${displaymagicianlog:component=UserAgent}"
            };
            configuration.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);
            LogManager.Configuration = configuration;
            LogManager.GetCurrentClassLogger().Info("UserAgent/ConfigureLogging: User Agent logging started at {0}.", logPath);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException)
        {
            Console.WriteLine($"UserAgent/ConfigureLogging: Could not configure logging at {logPath}: {ex.Message}");
        }
    }

    private static bool TryPrepareLogPath(string preferredLogPath, string legacyLogPath)
    {
        try
        {
            Directory.CreateDirectory(preferredLogPath);
            string probePath = Path.Combine(preferredLogPath, $".write-probe-{Guid.NewGuid():N}.tmp");
            using (FileStream probe = new FileStream(probePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose))
            {
                probe.WriteByte(0);
            }

            if (Directory.Exists(legacyLogPath))
            {
                foreach (string legacyLogFile in Directory.EnumerateFiles(legacyLogPath, "UserAgent-*.log", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        string destinationPath = Path.Combine(preferredLogPath, Path.GetFileName(legacyLogFile));
                        if (File.Exists(destinationPath))
                        {
                            destinationPath = Path.Combine(preferredLogPath, $"{Path.GetFileNameWithoutExtension(legacyLogFile)}-{Guid.NewGuid():N}{Path.GetExtension(legacyLogFile)}");
                        }

                        File.Move(legacyLogFile, destinationPath);
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        Console.WriteLine($"UserAgent/TryPrepareLogPath: Could not move legacy log {legacyLogFile}: {ex.Message}");
                    }
                }
            }

            return true;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException)
        {
            Console.WriteLine($"UserAgent/TryPrepareLogPath: ProgramData log path is unavailable: {ex.Message}");
            return false;
        }
    }
}
