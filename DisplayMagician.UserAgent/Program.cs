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

    private static void ConfigureLogging(string userSid)
    {
        string legacyLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician", "Logs");
        string preferredLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DisplayMagician", "Users", userSid, "Logs");
        string logPath = TryPrepareLogPath(preferredLogPath, legacyLogPath) ? preferredLogPath : legacyLogPath;
        try
        {
            Directory.CreateDirectory(logPath);
            LoggingConfiguration configuration = new LoggingConfiguration();
            FileTarget fileTarget = new FileTarget("userAgentLog")
            {
                FileName = Path.Combine(logPath, $"UserAgent-{DateTime.UtcNow.ToString("yyyy-MM-dd-HHmm", CultureInfo.InvariantCulture)}.log"),
                MaxArchiveFiles = 4,
                ArchiveAboveSize = 41943040,
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}|${onexception:EXCEPTION OCCURRED \\:${exception::format=toString,Properties,Data}"
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
