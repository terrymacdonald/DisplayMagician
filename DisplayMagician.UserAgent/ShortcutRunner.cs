using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Contracts;
using DisplayMagician.Processes;
using DisplayMagician.GameLibraries;

namespace DisplayMagician.UserAgent;

/// <summary>Agent-owned shortcut lifecycle. Execution stages are added while the legacy runner remains active.</summary>
public sealed class ShortcutRunner
{
    private readonly ShortcutStore _shortcutStore;
    private readonly AutomaticGameDetectionRegistry _automaticGameDetectionRegistry;
    private readonly UserProfileOperationService _profileOperationService;
    private readonly ShortcutRecoveryStore _recoveryStore;

    public ShortcutRunner(ShortcutStore shortcutStore, AutomaticGameDetectionRegistry automaticGameDetectionRegistry, UserProfileOperationService profileOperationService, ShortcutRecoveryStore recoveryStore)
    {
        _shortcutStore = shortcutStore ?? throw new ArgumentNullException(nameof(shortcutStore));
        _automaticGameDetectionRegistry = automaticGameDetectionRegistry ?? throw new ArgumentNullException(nameof(automaticGameDetectionRegistry));
        _profileOperationService = profileOperationService ?? throw new ArgumentNullException(nameof(profileOperationService));
        _recoveryStore = recoveryStore ?? throw new ArgumentNullException(nameof(recoveryStore));
    }

    public bool IsRecoveryRequired => _recoveryStore.HasPendingRecovery();

    public Task<ShortcutRunResult> PrepareRunAsync(string shortcutId, CancellationToken cancellationToken)
    {
        if (!_shortcutStore.TryGetShortcutDefinition(shortcutId, out ShortcutDefinition? shortcut))
        {
            return Task.FromResult(new ShortcutRunResult(Guid.NewGuid(), ShortcutRunOutcome.ShortcutNotFound));
        }

        return Task.FromResult(new ShortcutRunResult(Guid.NewGuid(), ShortcutRunOutcome.Prepared, shortcut));
    }

    public async Task<ShortcutRunResult> ApplyShortcutProfilesAsync(string shortcutId, int audioDeviceWaitMilliseconds, CancellationToken cancellationToken)
    {
        return await RunShortcutAsync(shortcutId, audioDeviceWaitMilliseconds, shouldStartGame: true, isManualRun: true, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ShortcutRunResult> ApplyDetectedGameShortcutAsync(string shortcutId, int audioDeviceWaitMilliseconds, CancellationToken cancellationToken)
    {
        return await RunShortcutAsync(shortcutId, audioDeviceWaitMilliseconds, shouldStartGame: false, isManualRun: false, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ShortcutRunResult> RunShortcutAsync(string shortcutId, int audioDeviceWaitMilliseconds, bool shouldStartGame, bool isManualRun, CancellationToken cancellationToken)
    {
        ShortcutRunResult preparedRun = await PrepareRunAsync(shortcutId, cancellationToken).ConfigureAwait(false);
        if (preparedRun.Outcome != ShortcutRunOutcome.Prepared || preparedRun.Shortcut == null)
        {
            return preparedRun;
        }

        ShortcutDefinition shortcut = preparedRun.Shortcut;
        if (!IsShortcutRunnable(shortcut))
        {
            return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
        }

        bool automaticDetectionWasSuspended = isManualRun && shortcut.GameLaunchMode == GameLaunchMode.DetectGameRunning &&
            _automaticGameDetectionRegistry.SuspendAutomaticDetectionForManualRun(shortcut.Id);
        UserProfileOperationState originalState = _profileOperationService.CaptureCurrentProfileState();
        bool shouldApplyDisplayProfile = !string.IsNullOrWhiteSpace(shortcut.ProfileId) && !string.Equals(shortcut.ProfileId, originalState.DisplayProfileId, StringComparison.OrdinalIgnoreCase);
        bool shouldApplyAudioProfile = !string.IsNullOrWhiteSpace(shortcut.AudioProfileId) && !string.Equals(shortcut.AudioProfileId, originalState.AudioProfileId, StringComparison.OrdinalIgnoreCase);
        ShortcutRecoveryRecord? recoveryRecord = null;
        if ((shouldApplyDisplayProfile && shortcut.DisplayPermanence == ShortcutDefinitionPermanence.Temporary) ||
            (shouldApplyAudioProfile && shortcut.AudioPermanence == ShortcutDefinitionPermanence.Temporary))
        {
            recoveryRecord = new ShortcutRecoveryRecord
            {
                OperationId = preparedRun.OperationId,
                ShortcutId = shortcut.Id,
                DisplayProfileId = originalState.DisplayProfileId,
                AudioProfileId = originalState.AudioProfileId,
                RequiresDisplayRestore = shouldApplyDisplayProfile && shortcut.DisplayPermanence == ShortcutDefinitionPermanence.Temporary,
                RequiresAudioRestore = shouldApplyAudioProfile && shortcut.AudioPermanence == ShortcutDefinitionPermanence.Temporary,
                CreatedUtc = DateTime.UtcNow
            };
            _recoveryStore.Save(recoveryRecord);
        }
        List<StartedProgram> startedPrograms = new List<StartedProgram>();
        List<ShortcutStopProgramDefinition> stoppedProgramsToRestart = new List<ShortcutStopProgramDefinition>();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!RunPreGamePrograms(shortcut, startedPrograms, stoppedProgramsToRestart, cancellationToken))
            {
                return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
            }
            if (shouldApplyDisplayProfile)
            {
                ApplyDisplayProfileOperationResult displayResult = await _profileOperationService.ApplyDisplayProfileAsync(shortcut.ProfileId, cancellationToken).ConfigureAwait(false);
                if (!displayResult.IsSuccessful)
                {
                    return new ShortcutRunResult(preparedRun.OperationId, displayResult.WasCancelled ? ShortcutRunOutcome.Cancelled : ShortcutRunOutcome.Failed, shortcut);
                }
            }

            if (shouldApplyAudioProfile && !_profileOperationService.ApplyAudioProfile(shortcut.AudioProfileId, audioDeviceWaitMilliseconds).IsSuccessful)
            {
                return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
            }

            if (shortcut.Category == ShortcutDefinitionCategory.Executable && !string.IsNullOrWhiteSpace(shortcut.ExecutablePath))
            {
                ProcessTreeMonitor? monitor = shortcut.MonitorExecutablePath
                    ? ProcessTreeMonitor.BeginWatching(string.IsNullOrWhiteSpace(shortcut.DifferentExecutablePathToMonitor) ? shortcut.ExecutablePath : shortcut.DifferentExecutablePathToMonitor, shortcut.StartTimeoutSeconds)
                    : null;
                try
                {
                    List<System.Diagnostics.Process> processes = ProcessUtils.StartProcess(shortcut.ExecutablePath, shortcut.ExecutableArguments, (ProcessPriority)(int)shortcut.ProcessPriority, shortcut.StartTimeoutSeconds, shortcut.RunExecutableAsAdministrator);
                    if (processes.Count == 0)
                    {
                        return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                    }
                    monitor?.RegisterLaunchedProcesses(processes);
                    while (monitor != null && monitor.IsRunning)
                    {
                        await Task.Delay(500, cancellationToken).ConfigureAwait(false);
                    }
                    ProcessUtils.DisposeProcesses(processes);
                }
                finally
                {
                    monitor?.Dispose();
                }
            }

            if (shortcut.Category == ShortcutDefinitionCategory.Game)
            {
                if (!GameLibrary.GamesLoaded)
                {
                    await Task.Run(GameLibrary.LoadGamesInBackground, cancellationToken).ConfigureAwait(false);
                }

                Game? game = GameLibrary.GetAnyGameById(shortcut.GameAppId);
                if (game == null)
                {
                    return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                }

                if (shouldStartGame)
                {
                    game.GameLibrary.StartGame(game, shortcut.GameArguments, (ProcessPriority)(int)shortcut.ProcessPriority);
                    DateTime gameStartDeadlineUtc = DateTime.UtcNow.AddSeconds(Math.Clamp(shortcut.StartTimeoutSeconds, 1, 60));
                    while (!IsGameRunning(shortcut, game) && DateTime.UtcNow < gameStartDeadlineUtc)
                    {
                        await Task.Delay(250, cancellationToken).ConfigureAwait(false);
                    }
                    if (!IsGameRunning(shortcut, game))
                    {
                        return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                    }
                }

                while (IsGameRunning(shortcut, game))
                {
                    await Task.Delay(500, cancellationToken).ConfigureAwait(false);
                }
            }

            return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Completed, shortcut);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Cancelled, shortcut);
        }
        finally
        {
            RunPostGameProgramCleanup(shortcut, startedPrograms, stoppedProgramsToRestart);
            bool recoveryRestored = true;
            if (recoveryRecord?.RequiresDisplayRestore == true)
            {
                recoveryRestored = !string.IsNullOrWhiteSpace(recoveryRecord.DisplayProfileId) &&
                    (await _profileOperationService.ApplyDisplayProfileAsync(recoveryRecord.DisplayProfileId, CancellationToken.None).ConfigureAwait(false)).IsSuccessful;
            }
            if (recoveryRecord?.RequiresAudioRestore == true)
            {
                recoveryRestored = !string.IsNullOrWhiteSpace(recoveryRecord.AudioProfileId) &&
                    _profileOperationService.ApplyAudioProfile(recoveryRecord.AudioProfileId, audioDeviceWaitMilliseconds).IsSuccessful && recoveryRestored;
            }
            if (recoveryRecord != null && recoveryRestored)
            {
                _recoveryStore.Clear();
            }
            if (automaticDetectionWasSuspended)
            {
                _automaticGameDetectionRegistry.RestoreAutomaticDetectionAfterManualRun(shortcut.Id);
            }
        }
    }

    private static bool IsShortcutRunnable(ShortcutDefinition shortcut)
    {
        if (shortcut.Category == ShortcutDefinitionCategory.Executable &&
            (string.IsNullOrWhiteSpace(shortcut.ExecutablePath) || !File.Exists(shortcut.ExecutablePath) ||
             (shortcut.ExecutableArgumentsRequired && string.IsNullOrWhiteSpace(shortcut.ExecutableArguments)) ||
             (shortcut.MonitorExecutablePath && !string.IsNullOrWhiteSpace(shortcut.DifferentExecutablePathToMonitor) && !File.Exists(shortcut.DifferentExecutablePathToMonitor))))
        {
            return false;
        }

        if (shortcut.Category == ShortcutDefinitionCategory.Game &&
            (string.IsNullOrWhiteSpace(shortcut.GameAppId) || (shortcut.GameArgumentsRequired && string.IsNullOrWhiteSpace(shortcut.GameArguments)) ||
             (shortcut.MonitorDifferentGameExecutable && (string.IsNullOrWhiteSpace(shortcut.DifferentGameExecutablePathToMonitor) || !File.Exists(shortcut.DifferentGameExecutablePathToMonitor)))))
        {
            return false;
        }

        if (shortcut.Category is ShortcutDefinitionCategory.Unknown or ShortcutDefinitionCategory.Application)
        {
            return false;
        }

        return shortcut.StartPrograms.Where(program => !program.Disabled).All(IsRunnable) &&
               shortcut.StopPrograms.Where(program => !program.Disabled).All(program => !string.IsNullOrWhiteSpace(program.ExecutablePath) && File.Exists(program.ExecutablePath)) &&
               shortcut.AfterPrograms.Where(program => !program.Disabled).All(IsRunnable);
    }

    private static bool IsRunnable(ShortcutStartProgramDefinition program)
    {
        return string.IsNullOrWhiteSpace(program.ApplicationId) && !string.IsNullOrWhiteSpace(program.ExecutablePath) &&
            File.Exists(program.ExecutablePath) && (!program.ArgumentsRequired || !string.IsNullOrWhiteSpace(program.Arguments));
    }

    private static bool IsRunnable(ShortcutAfterProgramDefinition program)
    {
        return !string.IsNullOrWhiteSpace(program.ExecutablePath) && File.Exists(program.ExecutablePath) &&
            (!program.ArgumentsRequired || !string.IsNullOrWhiteSpace(program.Arguments));
    }

    private static bool RunPreGamePrograms(ShortcutDefinition shortcut, List<StartedProgram> startedPrograms, List<ShortcutStopProgramDefinition> stoppedProgramsToRestart, CancellationToken cancellationToken)
    {
        List<(int Priority, ShortcutStartProgramDefinition? StartProgram, ShortcutStopProgramDefinition? StopProgram)> actions = new List<(int, ShortcutStartProgramDefinition?, ShortcutStopProgramDefinition?)>();
        actions.AddRange(shortcut.StartPrograms.Where(program => !program.Disabled).Select(program => (Priority: program.Priority, StartProgram: (ShortcutStartProgramDefinition?)program, StopProgram: (ShortcutStopProgramDefinition?)null)));
        actions.AddRange(shortcut.StopPrograms.Where(program => !program.Disabled).Select(program => (Priority: program.Priority, StartProgram: (ShortcutStartProgramDefinition?)null, StopProgram: (ShortcutStopProgramDefinition?)program)));

        foreach ((int _, ShortcutStartProgramDefinition? startProgram, ShortcutStopProgramDefinition? stopProgram) in actions.OrderBy(action => action.Priority))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (stopProgram != null)
            {
                if (!StopProgram(stopProgram, stoppedProgramsToRestart))
                {
                    return false;
                }
                continue;
            }

            if (startProgram == null || !StartProgram(startProgram, startedPrograms))
            {
                return false;
            }
        }

        return true;
    }

    private static bool StartProgram(ShortcutStartProgramDefinition program, List<StartedProgram> startedPrograms)
    {
        if (!string.IsNullOrWhiteSpace(program.ApplicationId) || string.IsNullOrWhiteSpace(program.ExecutablePath) || !File.Exists(program.ExecutablePath) || (program.ArgumentsRequired && string.IsNullOrWhiteSpace(program.Arguments)))
        {
            return false;
        }

        List<Process> existingProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(program.ExecutablePath)).ToList();
        try
        {
            if (program.DoNotStartIfAlreadyRunning && existingProcesses.Count > 0)
            {
                return true;
            }
        }
        finally
        {
            ProcessUtils.DisposeProcesses(existingProcesses);
        }

        ProcessTreeMonitor? monitor = ProcessTreeMonitor.BeginWatching(program.ExecutablePath, 10);
        List<Process> startedProcesses = ProcessUtils.StartProcess(program.ExecutablePath, program.Arguments, (ProcessPriority)(int)program.ProcessPriority, 10, program.RunAsAdministrator);
        monitor?.RegisterLaunchedProcesses(startedProcesses);
        if (startedProcesses.Count == 0)
        {
            monitor?.Dispose();
            return false;
        }

        if (program.CloseOnFinish)
        {
            startedPrograms.Add(new StartedProgram(program.Priority, startedProcesses, monitor));
        }
        else
        {
            ProcessUtils.DisposeProcesses(startedProcesses);
            monitor?.Dispose();
        }

        return true;
    }

    private static bool StopProgram(ShortcutStopProgramDefinition program, List<ShortcutStopProgramDefinition> stoppedProgramsToRestart)
    {
        if (string.IsNullOrWhiteSpace(program.ExecutablePath))
        {
            return false;
        }

        List<Process> runningProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(program.ExecutablePath)).ToList();
        try
        {
            if (runningProcesses.Count == 0)
            {
                return true;
            }

            if (!ProcessUtils.StopProcess(runningProcesses))
            {
                return false;
            }
            if (program.RestartAfterwards)
            {
                stoppedProgramsToRestart.Add(program);
            }

            return true;
        }
        finally
        {
            ProcessUtils.DisposeProcesses(runningProcesses);
        }
    }

    private static void RunPostGameProgramCleanup(ShortcutDefinition shortcut, List<StartedProgram> startedPrograms, List<ShortcutStopProgramDefinition> stoppedProgramsToRestart)
    {
        foreach (StartedProgram startedProgram in startedPrograms.OrderBy(program => program.Priority))
        {
            try
            {
                List<Process> processesToStop = startedProgram.Monitor?.GetTrackedProcesses() ?? new List<Process>();
                foreach (Process process in startedProgram.Processes)
                {
                    if (!ProcessUtils.ProcessExited(process) && processesToStop.All(trackedProcess => trackedProcess.Id != process.Id))
                    {
                        processesToStop.Add(process);
                    }
                }
                ProcessUtils.StopProcess(processesToStop);
                ProcessUtils.DisposeProcesses(processesToStop);
            }
            catch
            {
                // Cleanup continues so other temporary shortcut state is still restored.
            }
            finally
            {
                ProcessUtils.DisposeProcesses(startedProgram.Processes);
                startedProgram.Monitor?.Dispose();
            }
        }
        startedPrograms.Clear();

        foreach (ShortcutStopProgramDefinition stoppedProgram in stoppedProgramsToRestart.OrderBy(program => program.Priority))
        {
            try
            {
                ProcessUtils.DisposeProcesses(ProcessUtils.StartProcess(stoppedProgram.ExecutablePath, string.Empty, (ProcessPriority)(int)stoppedProgram.RestartProcessPriority, 10, stoppedProgram.RunAsAdministrator));
            }
            catch
            {
                // Cleanup continues so other temporary shortcut state is still restored.
            }
        }
        stoppedProgramsToRestart.Clear();

        foreach (ShortcutAfterProgramDefinition afterProgram in shortcut.AfterPrograms.Where(program => !program.Disabled).OrderBy(program => program.Priority))
        {
            if (string.IsNullOrWhiteSpace(afterProgram.ExecutablePath) || !File.Exists(afterProgram.ExecutablePath) || (afterProgram.ArgumentsRequired && string.IsNullOrWhiteSpace(afterProgram.Arguments)))
            {
                continue;
            }

            List<Process> existingProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(afterProgram.ExecutablePath)).ToList();
            try
            {
                if (afterProgram.DoNotStartIfAlreadyRunning && existingProcesses.Count > 0)
                {
                    continue;
                }
            }
            finally
            {
                ProcessUtils.DisposeProcesses(existingProcesses);
            }

            try
            {
                ProcessUtils.DisposeProcesses(ProcessUtils.StartProcess(afterProgram.ExecutablePath, afterProgram.Arguments, (ProcessPriority)(int)afterProgram.ProcessPriority, 10, afterProgram.RunAsAdministrator));
            }
            catch
            {
                // An after-program failure does not prevent restoration of temporary state.
            }
        }
    }

    public async Task<bool> RestorePendingRecoveryAsync(int audioDeviceWaitMilliseconds, CancellationToken cancellationToken)
    {
        ShortcutRecoveryRecord? recoveryRecord = _recoveryStore.GetPending();
        if (recoveryRecord == null)
        {
            return !_recoveryStore.HasPendingRecovery();
        }

        bool recoveryRestored = true;
        if (recoveryRecord.RequiresDisplayRestore)
        {
            recoveryRestored = !string.IsNullOrWhiteSpace(recoveryRecord.DisplayProfileId) &&
                (await _profileOperationService.ApplyDisplayProfileAsync(recoveryRecord.DisplayProfileId, cancellationToken).ConfigureAwait(false)).IsSuccessful;
        }
        if (recoveryRecord.RequiresAudioRestore)
        {
            recoveryRestored = !string.IsNullOrWhiteSpace(recoveryRecord.AudioProfileId) &&
                _profileOperationService.ApplyAudioProfile(recoveryRecord.AudioProfileId, audioDeviceWaitMilliseconds).IsSuccessful && recoveryRestored;
        }
        if (recoveryRestored)
        {
            _recoveryStore.Clear();
        }

        return recoveryRestored;
    }

    private static bool IsGameRunning(ShortcutDefinition shortcut, Game game)
    {
        return shortcut.MonitorDifferentGameExecutable && !string.IsNullOrWhiteSpace(shortcut.DifferentGameExecutablePathToMonitor)
            ? ProcessTreeMonitor.IsExecutableRunning(shortcut.DifferentGameExecutablePathToMonitor)
            : game.IsRunning;
    }

    private sealed record StartedProgram(int Priority, List<Process> Processes, ProcessTreeMonitor? Monitor);
}

public sealed record ShortcutRunResult(Guid OperationId, ShortcutRunOutcome Outcome, ShortcutDefinition? Shortcut = null);

public enum ShortcutRunOutcome
{
    Prepared = 0,
    ShortcutNotFound = 1,
    Cancelled = 2,
    Failed = 3,
    Completed = 4
}
