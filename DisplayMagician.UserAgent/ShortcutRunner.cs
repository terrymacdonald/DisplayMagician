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
using NLog;

namespace DisplayMagician.UserAgent;

/// <summary>Agent-owned shortcut lifecycle and recovery execution.</summary>
public sealed class ShortcutRunner
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly ShortcutStore _shortcutStore;
    private readonly AutomaticGameDetectionRegistry _automaticGameDetectionRegistry;
    private readonly UserProfileOperationService _profileOperationService;
    private readonly ShortcutRecoveryStore _recoveryStore;
    private readonly AudioVolumeOverrideService _audioVolumeOverrideService;

    public ShortcutRunner(ShortcutStore shortcutStore, AutomaticGameDetectionRegistry automaticGameDetectionRegistry, UserProfileOperationService profileOperationService, ShortcutRecoveryStore recoveryStore, AudioVolumeOverrideService? audioVolumeOverrideService = null)
    {
        _shortcutStore = shortcutStore ?? throw new ArgumentNullException(nameof(shortcutStore));
        _automaticGameDetectionRegistry = automaticGameDetectionRegistry ?? throw new ArgumentNullException(nameof(automaticGameDetectionRegistry));
        _profileOperationService = profileOperationService ?? throw new ArgumentNullException(nameof(profileOperationService));
        _recoveryStore = recoveryStore ?? throw new ArgumentNullException(nameof(recoveryStore));
        _audioVolumeOverrideService = audioVolumeOverrideService ?? new AudioVolumeOverrideService();
    }

    public bool IsRecoveryRequired => _recoveryStore.HasPendingRecovery();

    public Task<ShortcutRunResult> PrepareRunAsync(string shortcutId, CancellationToken cancellationToken, Guid? operationId = null)
    {
        Guid resolvedOperationId = operationId ?? Guid.NewGuid();
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(new ShortcutRunResult(resolvedOperationId, ShortcutRunOutcome.Cancelled));
        }

        if (!_shortcutStore.TryGetShortcutDefinition(shortcutId, out ShortcutDefinition? shortcut))
        {
            return Task.FromResult(new ShortcutRunResult(resolvedOperationId, ShortcutRunOutcome.ShortcutNotFound));
        }

        return Task.FromResult(new ShortcutRunResult(resolvedOperationId, ShortcutRunOutcome.Prepared, shortcut));
    }

    public async Task<ShortcutRunResult> ApplyShortcutProfilesAsync(string shortcutId, int audioDeviceWaitMilliseconds, CancellationToken cancellationToken, Func<OperationStatusUpdate, CancellationToken, Task>? publishStatusAsync = null, Guid? operationId = null, Func<RequestOperationDecisionRequest, CancellationToken, Task<OperationDecision>>? requestDecisionAsync = null)
    {
        return await RunShortcutAsync(shortcutId, audioDeviceWaitMilliseconds, shouldStartGame: true, isManualRun: true, cancellationToken, publishStatusAsync, operationId, requestDecisionAsync).ConfigureAwait(false);
    }

    public async Task<ShortcutRunResult> ApplyDetectedGameShortcutAsync(string shortcutId, int audioDeviceWaitMilliseconds, CancellationToken cancellationToken, Func<OperationStatusUpdate, CancellationToken, Task>? publishStatusAsync = null)
    {
        return await RunShortcutAsync(shortcutId, audioDeviceWaitMilliseconds, shouldStartGame: false, isManualRun: false, cancellationToken, publishStatusAsync).ConfigureAwait(false);
    }

    private async Task<ShortcutRunResult> RunShortcutAsync(string shortcutId, int audioDeviceWaitMilliseconds, bool shouldStartGame, bool isManualRun, CancellationToken cancellationToken, Func<OperationStatusUpdate, CancellationToken, Task>? publishStatusAsync, Guid? operationId = null, Func<RequestOperationDecisionRequest, CancellationToken, Task<OperationDecision>>? requestDecisionAsync = null)
    {
        ShortcutRunResult preparedRun = await PrepareRunAsync(shortcutId, cancellationToken, operationId).ConfigureAwait(false);
        if (preparedRun.Outcome != ShortcutRunOutcome.Prepared || preparedRun.Shortcut == null)
        {
            return preparedRun;
        }

        ShortcutDefinition shortcut = preparedRun.Shortcut;
        await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.Validating, "Validating shortcut.", cancellationToken).ConfigureAwait(false);
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
        AudioVolumeOverrideState? audioVolumeOverrideState = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.StartingPrograms, "Starting shortcut programs.", cancellationToken).ConfigureAwait(false);
            string? preGameProgramFailure = RunPreGamePrograms(shortcut, startedPrograms, stoppedProgramsToRestart, cancellationToken);
            if (preGameProgramFailure != null && !await ShouldContinueAfterFailureAsync(requestDecisionAsync, preparedRun.OperationId, "Shortcut program step failed", preGameProgramFailure, cancellationToken).ConfigureAwait(false))
            {
                return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
            }
            if (shouldApplyDisplayProfile)
            {
                await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.ApplyingDisplayProfile, "Applying display profile.", cancellationToken).ConfigureAwait(false);
                ApplyDisplayProfileOperationResult displayResult = await _profileOperationService.ApplyDisplayProfileAsync(shortcut.ProfileId, cancellationToken).ConfigureAwait(false);
                if (!displayResult.IsSuccessful)
                {
                    if (displayResult.WasCancelled || !await ShouldContinueAfterFailureAsync(requestDecisionAsync, preparedRun.OperationId, "Display profile could not be applied", "Continue starting the shortcut without applying its display profile?", cancellationToken).ConfigureAwait(false))
                    {
                        return new ShortcutRunResult(preparedRun.OperationId, displayResult.WasCancelled ? ShortcutRunOutcome.Cancelled : ShortcutRunOutcome.Failed, shortcut);
                    }
                }
            }

            if (shouldApplyAudioProfile)
            {
                await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.ApplyingAudioProfile, "Applying audio profile.", cancellationToken).ConfigureAwait(false);
                if (!_profileOperationService.ApplyAudioProfile(shortcut.AudioProfileId, audioDeviceWaitMilliseconds).IsSuccessful &&
                    !await ShouldContinueAfterFailureAsync(requestDecisionAsync, preparedRun.OperationId, "Audio profile could not be applied", "Continue starting the shortcut without applying its audio profile?", cancellationToken).ConfigureAwait(false))
                {
                    return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                }
            }

            if (shortcut.OverrideAudioSpeakerVolume || shortcut.OverrideAudioMicrophoneVolume)
            {
                await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.ApplyingAudioProfile, "Applying audio volume overrides.", cancellationToken).ConfigureAwait(false);
                if (!_audioVolumeOverrideService.TryCapture(shortcut, out audioVolumeOverrideState))
                {
                    return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                }

                recoveryRecord ??= new ShortcutRecoveryRecord
                {
                    OperationId = preparedRun.OperationId,
                    ShortcutId = shortcut.Id,
                    CreatedUtc = DateTime.UtcNow
                };
                recoveryRecord.RequiresAudioVolumeRestore = true;
                recoveryRecord.AudioVolumeOverrides = audioVolumeOverrideState.Entries;
                _recoveryStore.Save(recoveryRecord);
                if (!_audioVolumeOverrideService.Apply(audioVolumeOverrideState))
                {
                    return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                }
            }

            if ((shortcut.Category == ShortcutDefinitionCategory.Executable ||
                (shortcut.Category == ShortcutDefinitionCategory.Application && shortcut.ApplicationLibrary != 2)) &&
                !string.IsNullOrWhiteSpace(shortcut.ExecutablePath))
            {
                ProcessTreeMonitor? monitor = shortcut.MonitorExecutablePath
                    ? ProcessTreeMonitor.BeginWatching(string.IsNullOrWhiteSpace(shortcut.DifferentExecutablePathToMonitor) ? shortcut.ExecutablePath : shortcut.DifferentExecutablePathToMonitor, shortcut.StartTimeoutSeconds)
                    : null;
                try
                {
                    await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.StartingGame, "Starting executable.", cancellationToken).ConfigureAwait(false);
                    List<System.Diagnostics.Process> processes = ProcessUtils.StartProcess(shortcut.ExecutablePath, shortcut.ExecutableArguments, (ProcessPriority)(int)shortcut.ProcessPriority, shortcut.StartTimeoutSeconds, shortcut.RunExecutableAsAdministrator);
                    if (processes.Count == 0)
                    {
                        return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                    }
                    monitor?.RegisterLaunchedProcesses(processes);
                    await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.WaitingForGameToClose, "Waiting for executable to close.", cancellationToken).ConfigureAwait(false);
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

            if (shortcut.Category == ShortcutDefinitionCategory.Application && shortcut.ApplicationLibrary == 2)
            {
                await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.StartingGame, "Starting packaged application.", cancellationToken).ConfigureAwait(false);
                using Process? process = UwpApplicationLauncher.Start(shortcut.ApplicationId, shortcut.ExecutableArguments);
                if (process == null)
                {
                    return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                }

                await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.WaitingForGameToClose, "Waiting for packaged application to close.", cancellationToken).ConfigureAwait(false);
                while (UwpApplicationLauncher.IsRunning(shortcut.ApplicationId) || !ProcessUtils.ProcessExited(process))
                {
                    await Task.Delay(500, cancellationToken).ConfigureAwait(false);
                }
            }

            if (shortcut.Category == ShortcutDefinitionCategory.Game)
            {
                if (!GameLibrary.GamesLoaded)
                {
                    await Task.Run(GameLibrary.LoadGamesInBackground, cancellationToken).ConfigureAwait(false);
                }

                if (string.IsNullOrWhiteSpace(shortcut.GameAppId))
                {
                    return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                }

                Game? game = GameLibrary.GetAnyGameById(shortcut.GameAppId);
                if (game == null)
                {
                    return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                }

                GameLibrary? gameLibrary = game.GameLibrary;
                if (gameLibrary == null)
                {
                    return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                }

                if (shouldStartGame)
                {
                    await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.StartingGame, "Starting game.", cancellationToken).ConfigureAwait(false);
                    gameLibrary.StartGame(game, shortcut.GameArguments, (ProcessPriority)(int)shortcut.ProcessPriority);
                    DateTime gameStartDeadlineUtc = DateTime.UtcNow.AddSeconds(Math.Clamp(shortcut.StartTimeoutSeconds, 1, 60));
                    await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.WaitingForGameToStart, "Waiting for game to start.", cancellationToken).ConfigureAwait(false);
                    while (!IsGameRunning(shortcut, game) && DateTime.UtcNow < gameStartDeadlineUtc)
                    {
                        await Task.Delay(250, cancellationToken).ConfigureAwait(false);
                    }
                    if (!IsGameRunning(shortcut, game))
                    {
                        return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                    }
                }

                await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.WaitingForGameToClose, "Waiting for game to close.", cancellationToken).ConfigureAwait(false);
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
                await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.RestoringDisplayProfile, "Restoring display profile.", CancellationToken.None).ConfigureAwait(false);
                recoveryRestored = !string.IsNullOrWhiteSpace(recoveryRecord.DisplayProfileId) &&
                    (await _profileOperationService.ApplyDisplayProfileAsync(recoveryRecord.DisplayProfileId, CancellationToken.None).ConfigureAwait(false)).IsSuccessful;
            }
            if (recoveryRecord?.RequiresAudioRestore == true)
            {
                await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.RestoringAudioProfile, "Restoring audio profile.", CancellationToken.None).ConfigureAwait(false);
                recoveryRestored = !string.IsNullOrWhiteSpace(recoveryRecord.AudioProfileId) &&
                    _profileOperationService.ApplyAudioProfile(recoveryRecord.AudioProfileId, audioDeviceWaitMilliseconds).IsSuccessful && recoveryRestored;
            }
            if (recoveryRecord?.RequiresAudioVolumeRestore == true)
            {
                await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.RestoringAudioProfile, "Restoring audio volume overrides.", CancellationToken.None).ConfigureAwait(false);
                recoveryRestored = _audioVolumeOverrideService.Restore(recoveryRecord.AudioVolumeOverrides) && recoveryRestored;
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
           if ((shortcut.Category == ShortcutDefinitionCategory.Executable ||
               (shortcut.Category == ShortcutDefinitionCategory.Application && shortcut.ApplicationLibrary != 2)) &&
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

        if (shortcut.Category == ShortcutDefinitionCategory.Application && shortcut.ApplicationLibrary == 2 &&
            (string.IsNullOrWhiteSpace(shortcut.ApplicationId) ||
             (shortcut.ExecutableArgumentsRequired && string.IsNullOrWhiteSpace(shortcut.ExecutableArguments))))
        {
            return false;
        }

        if (shortcut.Category == ShortcutDefinitionCategory.Unknown)
        {
            return false;
        }

        return shortcut.StartPrograms.Where(program => !program.Disabled).All(IsRunnable) &&
               shortcut.StopPrograms.Where(program => !program.Disabled).All(program => !string.IsNullOrWhiteSpace(program.ExecutablePath) && File.Exists(program.ExecutablePath)) &&
               shortcut.AfterPrograms.Where(program => !program.Disabled).All(IsRunnable);
    }

    private static bool IsRunnable(ShortcutStartProgramDefinition program)
    {
        return (!string.IsNullOrWhiteSpace(program.ApplicationId) && (!program.ArgumentsRequired || !string.IsNullOrWhiteSpace(program.Arguments))) ||
            (string.IsNullOrWhiteSpace(program.ApplicationId) && !string.IsNullOrWhiteSpace(program.ExecutablePath) &&
             File.Exists(program.ExecutablePath) && (!program.ArgumentsRequired || !string.IsNullOrWhiteSpace(program.Arguments)));
    }

    private static bool IsRunnable(ShortcutAfterProgramDefinition program)
    {
        return !string.IsNullOrWhiteSpace(program.ExecutablePath) && File.Exists(program.ExecutablePath) &&
            (!program.ArgumentsRequired || !string.IsNullOrWhiteSpace(program.Arguments));
    }

    private static string? RunPreGamePrograms(ShortcutDefinition shortcut, List<StartedProgram> startedPrograms, List<ShortcutStopProgramDefinition> stoppedProgramsToRestart, CancellationToken cancellationToken)
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
                    return $"Could not stop '{stopProgram.ExecutablePath}'.";
                }
                continue;
            }

            if (startProgram == null || !StartProgram(startProgram, startedPrograms))
            {
                return $"Could not start '{startProgram?.ExecutablePath ?? startProgram?.ApplicationId ?? "the configured program"}'.";
            }
        }

        return null;
    }

    private static async Task<bool> ShouldContinueAfterFailureAsync(Func<RequestOperationDecisionRequest, CancellationToken, Task<OperationDecision>>? requestDecisionAsync, Guid operationId, string title, string message, CancellationToken cancellationToken)
    {
        if (requestDecisionAsync == null)
        {
            return true;
        }

        try
        {
            OperationDecision decision = await requestDecisionAsync(new RequestOperationDecisionRequest
            {
                OperationId = operationId,
                OperationType = DisplayOperationType.StartShortcut,
                Title = title,
                Message = message,
                AllowedChoices = new[] { OperationDecisionChoice.Continue, OperationDecisionChoice.StopAndRestore },
                DefaultChoice = OperationDecisionChoice.Continue
            }, cancellationToken).ConfigureAwait(false);
            return decision.ResolvedChoice != OperationDecisionChoice.StopAndRestore;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // If the Control Service cannot present the prompt, preserve the product policy: continue toward launching the shortcut.
            _logger.Warn(ex, "ShortcutRunner/ShouldContinueAfterFailureAsync: Could not request an operation decision for {0}; continuing by default.", operationId);
            return true;
        }
    }

    private static bool StartProgram(ShortcutStartProgramDefinition program, List<StartedProgram> startedPrograms)
    {
        if (!string.IsNullOrWhiteSpace(program.ApplicationId))
        {
            if ((program.ArgumentsRequired && string.IsNullOrWhiteSpace(program.Arguments)) ||
                (program.DoNotStartIfAlreadyRunning && UwpApplicationLauncher.IsRunning(program.ApplicationId)))
            {
                return !program.ArgumentsRequired || !string.IsNullOrWhiteSpace(program.Arguments);
            }

            Process? uwpProcess = UwpApplicationLauncher.Start(program.ApplicationId, program.Arguments);
            if (uwpProcess == null)
            {
                return false;
            }

            if (program.CloseOnFinish)
            {
                startedPrograms.Add(new StartedProgram(program.Priority, new List<Process> { uwpProcess }, null));
            }
            else
            {
                uwpProcess.Dispose();
            }

            return true;
        }

        if (string.IsNullOrWhiteSpace(program.ExecutablePath) || !File.Exists(program.ExecutablePath) || (program.ArgumentsRequired && string.IsNullOrWhiteSpace(program.Arguments)))
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
        if (recoveryRecord.RequiresAudioVolumeRestore)
        {
            recoveryRestored = _audioVolumeOverrideService.Restore(recoveryRecord.AudioVolumeOverrides) && recoveryRestored;
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

    private static Task PublishStatusAsync(Func<OperationStatusUpdate, CancellationToken, Task>? publishStatusAsync, Guid operationId, OperationPhase phase, string message, CancellationToken cancellationToken)
    {
        return publishStatusAsync == null
            ? Task.CompletedTask
            : publishStatusAsync(new OperationStatusUpdate
            {
                OperationId = operationId,
                OperationType = DisplayOperationType.StartShortcut,
                Phase = phase,
                Message = message
            }, cancellationToken);
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
