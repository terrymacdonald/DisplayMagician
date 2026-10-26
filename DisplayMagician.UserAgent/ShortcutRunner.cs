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
    private readonly object _windowPlacementLock = new object();
    private WindowPlacementRun? _activeWindowPlacementRun;

    public ShortcutRunner(ShortcutStore shortcutStore, AutomaticGameDetectionRegistry automaticGameDetectionRegistry, UserProfileOperationService profileOperationService, ShortcutRecoveryStore recoveryStore, AudioVolumeOverrideService? audioVolumeOverrideService = null)
    {
        _shortcutStore = shortcutStore ?? throw new ArgumentNullException(nameof(shortcutStore));
        _automaticGameDetectionRegistry = automaticGameDetectionRegistry ?? throw new ArgumentNullException(nameof(automaticGameDetectionRegistry));
        _profileOperationService = profileOperationService ?? throw new ArgumentNullException(nameof(profileOperationService));
        _recoveryStore = recoveryStore ?? throw new ArgumentNullException(nameof(recoveryStore));
        _audioVolumeOverrideService = audioVolumeOverrideService ?? new AudioVolumeOverrideService();
    }

    public bool IsRecoveryRequired => _recoveryStore.HasPendingRecovery();

    public WindowPlacementSessionView? GetWindowPlacementSession()
    {
        lock (_windowPlacementLock)
        {
            WindowPlacementRun? run = _activeWindowPlacementRun;
            return run == null ? null : new WindowPlacementSessionView
            {
                OperationId = run.OperationId,
                ShortcutId = run.Shortcut.Id,
                ShortcutName = run.Shortcut.Name,
                CanSave = run.CanSave,
                ExistingPlacementCount = _shortcutStore.TryGetShortcutDefinition(run.Shortcut.Id, out ShortcutDefinition? current)
                    ? current?.WindowPlacements.Count ?? 0 : 0
            };
        }
    }

    public SaveWindowPlacementsResult? SaveWindowPlacements(Guid operationId, string shortcutId)
    {
        lock (_windowPlacementLock)
        {
            WindowPlacementRun? run = _activeWindowPlacementRun;
            if (run == null || !run.CanSave || run.OperationId != operationId || !string.Equals(run.Shortcut.Id, shortcutId, StringComparison.OrdinalIgnoreCase))
                return null;
            List<WindowPlacementService.Source> sources = run.GetSources(out bool sourcesComplete);
            if (!sourcesComplete) throw new InvalidOperationException("A launched process could not be inspected, so the existing window snapshot was kept.");
            WindowPlacementService.CaptureResult capture = WindowPlacementService.Capture(sources);
            if (!capture.IsComplete) throw new InvalidOperationException("A window could not be inspected, so the existing window snapshot was kept.");
            ShortcutWindowPlacementDefinition[] placements = capture.Placements;
            if (placements.Length == 0) return new SaveWindowPlacementsResult();
            if (_shortcutStore.ReplaceWindowPlacements(shortcutId, placements) < 0) return null;
            return new SaveWindowPlacementsResult
            {
                SavedWindowCount = placements.Length,
                ProgramNames = placements.Select(item => Path.GetFileNameWithoutExtension(item.ExecutablePath)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            };
        }
    }

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

    public async Task<ShortcutRunResult> ApplyDetectedGameShortcutAsync(string shortcutId, ProcessIdentity detectedProcess, int audioDeviceWaitMilliseconds, CancellationToken cancellationToken, Func<OperationStatusUpdate, CancellationToken, Task>? publishStatusAsync = null, Guid? operationId = null, Func<RequestOperationDecisionRequest, CancellationToken, Task<OperationDecision>>? requestDecisionAsync = null)
    {
        return await RunShortcutAsync(shortcutId, audioDeviceWaitMilliseconds, shouldStartGame: false, isManualRun: false, cancellationToken, publishStatusAsync, operationId, requestDecisionAsync, detectedProcess).ConfigureAwait(false);
    }

    private async Task<ShortcutRunResult> RunShortcutAsync(string shortcutId, int audioDeviceWaitMilliseconds, bool shouldStartGame, bool isManualRun, CancellationToken cancellationToken, Func<OperationStatusUpdate, CancellationToken, Task>? publishStatusAsync, Guid? operationId = null, Func<RequestOperationDecisionRequest, CancellationToken, Task<OperationDecision>>? requestDecisionAsync = null, ProcessIdentity? detectedProcess = null)
    {
        // Await the whole lifecycle, including restoration, before reporting success.
        ShortcutRunResult result = await RunShortcutCoreAsync(shortcutId, audioDeviceWaitMilliseconds, shouldStartGame, isManualRun,
            cancellationToken, publishStatusAsync, operationId, requestDecisionAsync, detectedProcess).ConfigureAwait(false);
        if (!result.RequiresRecovery && _recoveryStore.HasPendingRecovery() && result.Shortcut != null)
        {
            return result with { Outcome = ShortcutRunOutcome.Failed, RequiresRecovery = true,
                Message = "The shortcut finished, but its original settings could not be fully restored. Recovery is required." };
        }
        return result;
    }

    private async Task<ShortcutRunResult> RunShortcutCoreAsync(string shortcutId, int audioDeviceWaitMilliseconds, bool shouldStartGame, bool isManualRun, CancellationToken cancellationToken, Func<OperationStatusUpdate, CancellationToken, Task>? publishStatusAsync, Guid? operationId, Func<RequestOperationDecisionRequest, CancellationToken, Task<OperationDecision>>? requestDecisionAsync, ProcessIdentity? detectedProcess)
    {
        ShortcutRunResult preparedRun = await PrepareRunAsync(shortcutId, cancellationToken, operationId).ConfigureAwait(false);
        if (preparedRun.Outcome != ShortcutRunOutcome.Prepared || preparedRun.Shortcut == null)
        {
            return preparedRun;
        }

        if (_recoveryStore.HasPendingRecovery())
            return preparedRun with { Outcome = ShortcutRunOutcome.Failed, RequiresRecovery = true,
                Message = "Original settings still require recovery before another shortcut can run." };

        ShortcutDefinition shortcut = preparedRun.Shortcut;
        WindowPlacementRun windowRun = new WindowPlacementRun(preparedRun.OperationId, shortcut);
        ProcessTreeMonitor? detectedGameMonitor = null;
        await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.Validating, "Validating shortcut.", cancellationToken).ConfigureAwait(false);
        if (!IsShortcutRunnable(shortcut))
        {
            return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
        }

        UserProfileOperationState originalState = _profileOperationService.CaptureCurrentProfileState();
        bool shouldApplyDisplayProfile = !string.IsNullOrWhiteSpace(shortcut.ProfileId) && !string.Equals(shortcut.ProfileId, originalState.DisplayProfileId, StringComparison.OrdinalIgnoreCase);
        bool shouldApplyAudioProfile = !string.IsNullOrWhiteSpace(shortcut.AudioProfileId) && !string.Equals(shortcut.AudioProfileId, originalState.AudioProfileId, StringComparison.OrdinalIgnoreCase);
        ShortcutRecoveryRecord? recoveryRecord = null;
        if ((shouldApplyDisplayProfile && shortcut.DisplayPermanence == ShortcutDefinitionPermanence.Temporary) ||
            (shouldApplyAudioProfile && shortcut.AudioPermanence == ShortcutDefinitionPermanence.Temporary))
        {
            originalState = _profileOperationService.CaptureCurrentProfileState(
                captureDisplaySnapshot: shouldApplyDisplayProfile && shortcut.DisplayPermanence == ShortcutDefinitionPermanence.Temporary,
                captureAudioSnapshot: shouldApplyAudioProfile && shortcut.AudioPermanence == ShortcutDefinitionPermanence.Temporary);
            recoveryRecord = new ShortcutRecoveryRecord
            {
                SchemaVersion = 2,
                OperationId = preparedRun.OperationId,
                ShortcutId = shortcut.Id,
                DisplayProfileId = originalState.DisplayProfileId,
                AudioProfileId = originalState.AudioProfileId,
                DisplayProfileSnapshotJson = originalState.DisplayProfileSnapshotJson,
                AudioProfileSnapshotJson = originalState.AudioProfileSnapshotJson,
                RequiresDisplayRestore = shouldApplyDisplayProfile && shortcut.DisplayPermanence == ShortcutDefinitionPermanence.Temporary,
                RequiresAudioRestore = shouldApplyAudioProfile && shortcut.AudioPermanence == ShortcutDefinitionPermanence.Temporary,
                CreatedUtc = DateTime.UtcNow
            };
            _recoveryStore.Save(recoveryRecord);
        }
        List<StartedProgram> startedPrograms = new List<StartedProgram>();
        List<StoppedProgram> stoppedProgramsToRestart = new List<StoppedProgram>();
        AudioVolumeOverrideState? audioVolumeOverrideState = null;
        bool automaticDetectionWasSuspended = isManualRun && shortcut.GameLaunchMode == GameLaunchMode.DetectGameRunning &&
            _automaticGameDetectionRegistry.SuspendAutomaticDetectionForManualRun(shortcut.Id);
        lock (_windowPlacementLock) _activeWindowPlacementRun = windowRun;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!shouldStartGame && shortcut.Category == ShortcutDefinitionCategory.Game)
            {
                // Attach before Start Programs run so a helper using the same executable
                // cannot be mistaken for the game that triggered automatic detection.
                if (!GameLibrary.GamesLoaded)
                    await Task.Run(GameLibrary.LoadGamesInBackground, cancellationToken).ConfigureAwait(false);
                Game? detectedGame = GameLibrary.GetAnyGameById(shortcut.GameAppId);
                string detectedExecutable = shortcut.MonitorDifferentGameExecutable
                    ? shortcut.DifferentGameExecutablePathToMonitor : detectedGame?.ExePath ?? string.Empty;
                if (detectedProcess.HasValue)
                    detectedGameMonitor = ProcessTreeMonitor.BeginWatchingExisting(detectedExecutable, detectedProcess.Value, shortcut.StartTimeoutSeconds);
                if (detectedGameMonitor == null)
                {
                    _logger.Warn("ShortcutRunner/RunShortcutAsync: Detected game PID {0} is no longer available for shortcut {1}.", detectedProcess?.ProcessId, shortcut.Id);
                    return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                }
                windowRun.Monitors.Add(("main", detectedGameMonitor));
                windowRun.OwnedHelperMonitors.Add(detectedGameMonitor);
                windowRun.RequireMainSource = true;
            }
            if (shouldApplyDisplayProfile)
            {
                await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.ApplyingDisplayProfile, "Applying display profile.", cancellationToken).ConfigureAwait(false);
                ApplyDisplayProfileOperationResult displayResult = await _profileOperationService.ApplyDisplayProfileAsync(shortcut.ProfileId, cancellationToken).ConfigureAwait(false);
                if (!displayResult.IsSuccessful)
                {
                    windowRun.DisplayProfileFailed = true;
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
                    SchemaVersion = 2,
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

            await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.StartingPrograms, "Starting shortcut programs.", cancellationToken).ConfigureAwait(false);
            if (!await RunPreGameProgramsAsync(shortcut, startedPrograms, stoppedProgramsToRestart, windowRun, requestDecisionAsync, preparedRun.OperationId, cancellationToken).ConfigureAwait(false))
            {
                return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
            }

            if ((shortcut.Category == ShortcutDefinitionCategory.Executable ||
                (shortcut.Category == ShortcutDefinitionCategory.Application && shortcut.ApplicationLibrary != 2)) &&
                !string.IsNullOrWhiteSpace(shortcut.ExecutablePath))
            {
                windowRun.RequireMainSource = true;
                ProcessTreeMonitor? monitor = ProcessTreeMonitor.BeginWatching(GetExecutableMonitorPath(shortcut), shortcut.StartTimeoutSeconds, continueTrackingDescendants: true);
                if (monitor == null) return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                List<Process> processes = new List<Process>();
                try
                {
                    await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.StartingGame, "Starting executable.", cancellationToken).ConfigureAwait(false);
                    processes = ProcessUtils.StartProcess(shortcut.ExecutablePath, shortcut.ExecutableArguments, (ProcessPriority)(int)shortcut.ProcessPriority, shortcut.StartTimeoutSeconds, shortcut.RunExecutableAsAdministrator);
                    if (processes.Count == 0)
                    {
                        return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                    }
                    monitor.RegisterLaunchedProcesses(processes, requireExpectedExecutable: !shortcut.MonitorLaunchedExecutable);
                    windowRun.Monitors.Add(("main", monitor));
                    if (!shortcut.MonitorLaunchedExecutable)
                    {
                        await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.WaitingForGameToStart, "Waiting for the alternative executable to start.", cancellationToken).ConfigureAwait(false);
                        DateTime deadlineUtc = DateTime.UtcNow.AddSeconds(Math.Clamp(shortcut.StartTimeoutSeconds, 1, 60));
                        while (!monitor.SelectNewestExpectedProcessTree() && DateTime.UtcNow < deadlineUtc)
                            await Task.Delay(250, cancellationToken).ConfigureAwait(false);
                        if (!monitor.SelectNewestExpectedProcessTree())
                            return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut)
                            { Message = "The alternative executable was not detected before the startup timeout." };
                    }
                    windowRun.CanSave = !windowRun.DisplayProfileFailed;
                    await RestoreWindowPlacementsAsync(windowRun, publishStatusAsync, cancellationToken).ConfigureAwait(false);
                    await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.WaitingForGameToClose, "Waiting for executable to close.", cancellationToken).ConfigureAwait(false);
                    while (monitor.IsRunning)
                    {
                        await Task.Delay(500, cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    monitor.Dispose();
                    ProcessUtils.DisposeProcesses(processes);
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

                string gameWindowExecutable = shortcut.MonitorDifferentGameExecutable
                    ? shortcut.DifferentGameExecutablePathToMonitor : game.ExePath;
                ProcessTreeMonitor? gameMonitor = detectedGameMonitor;
                if (shouldStartGame)
                {
                    if (!string.Equals(Path.GetExtension(gameWindowExecutable), ".exe", StringComparison.OrdinalIgnoreCase) || !File.Exists(gameWindowExecutable))
                    {
                        _logger.Warn("ShortcutRunner/RunShortcutAsync: Shortcut {0} needs a usable game executable; set Monitor different executable.", shortcut.Id);
                        return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut)
                        { Message = "The game library did not provide a usable executable. Set Monitor different executable for this shortcut." };
                    }
                    // Start before the library launch, so a short-lived launcher and its
                    // descendants are observed even if StartGame returns after it exits.
                    gameMonitor = ProcessTreeMonitor.BeginWatching(gameWindowExecutable, shortcut.StartTimeoutSeconds, continueTrackingDescendants: true);
                    if (gameMonitor == null) return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut);
                    windowRun.Monitors.Add(("main", gameMonitor));
                    windowRun.OwnedHelperMonitors.Add(gameMonitor);
                    windowRun.RequireMainSource = true;
                    await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.StartingGame, "Starting game.", cancellationToken).ConfigureAwait(false);
                    List<Process>? launchedProcesses = gameLibrary.StartGame(game, shortcut.GameArguments, (ProcessPriority)(int)shortcut.ProcessPriority);
                    try { gameMonitor.RegisterLaunchedProcesses(launchedProcesses, requireExpectedExecutable: true); }
                    finally { if (launchedProcesses != null) ProcessUtils.DisposeProcesses(launchedProcesses); }
                    DateTime gameStartDeadlineUtc = DateTime.UtcNow.AddSeconds(Math.Clamp(shortcut.StartTimeoutSeconds, 1, 60));
                    await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.WaitingForGameToStart, "Waiting for game to start.", cancellationToken).ConfigureAwait(false);
                    while (!gameMonitor.SelectNewestExpectedProcessTree() && DateTime.UtcNow < gameStartDeadlineUtc)
                    {
                        await Task.Delay(250, cancellationToken).ConfigureAwait(false);
                    }
                    if (!gameMonitor.SelectNewestExpectedProcessTree())
                    {
                        return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut)
                        { Message = "No new game process was detected before the startup timeout. Check Monitor different executable and the startup timeout." };
                    }
                }

                windowRun.CanSave = !windowRun.DisplayProfileFailed && gameMonitor != null;
                await RestoreWindowPlacementsAsync(windowRun, publishStatusAsync, cancellationToken).ConfigureAwait(false);

                await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.WaitingForGameToClose, "Waiting for game to close.", cancellationToken).ConfigureAwait(false);
                while (gameMonitor?.IsRunning == true)
                {
                    await Task.Delay(500, cancellationToken).ConfigureAwait(false);
                }
            }

            return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Completed, shortcut);
        }
        catch (ProcessTrackingException ex)
        {
            _logger.Warn(ex, "ShortcutRunner/RunShortcutAsync: Process tracking failed for shortcut {0}; restoring temporary state.", shortcut.Id);
            return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Failed, shortcut) { Message = ex.Message };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Cancelled, shortcut);
        }
        finally
        {
            lock (_windowPlacementLock)
            {
                if (ReferenceEquals(_activeWindowPlacementRun, windowRun)) _activeWindowPlacementRun = null;
            }
            foreach (ProcessTreeMonitor helperMonitor in windowRun.OwnedHelperMonitors) helperMonitor.Dispose();
            RunPostGameProgramCleanup(startedPrograms, stoppedProgramsToRestart);
            bool recoveryRestored = true;
            if (recoveryRecord?.RequiresDisplayRestore == true)
            {
                await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.RestoringDisplayProfile, "Restoring display profile.", CancellationToken.None).ConfigureAwait(false);
                recoveryRestored = (await _profileOperationService.ApplyDisplayProfileAsync(recoveryRecord.DisplayProfileId, CancellationToken.None, recoveryRecord.DisplayProfileSnapshotJson).ConfigureAwait(false)).IsSuccessful;
            }
            if (recoveryRecord?.RequiresAudioRestore == true)
            {
                await PublishStatusAsync(publishStatusAsync, preparedRun.OperationId, OperationPhase.RestoringAudioProfile, "Restoring audio profile.", CancellationToken.None).ConfigureAwait(false);
                recoveryRestored = _profileOperationService.ApplyAudioProfile(recoveryRecord.AudioProfileId, audioDeviceWaitMilliseconds, recoveryRecord.AudioProfileSnapshotJson).IsSuccessful && recoveryRestored;
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
            RunAfterPrograms(shortcut);
            if (automaticDetectionWasSuspended)
            {
                _automaticGameDetectionRegistry.RestoreAutomaticDetectionAfterManualRun(shortcut.Id);
            }
        }
    }

    internal static string GetExecutableMonitorPath(ShortcutDefinition shortcut) => shortcut.MonitorLaunchedExecutable
        ? shortcut.ExecutablePath : shortcut.DifferentExecutablePathToMonitor;

    private static bool IsShortcutRunnable(ShortcutDefinition shortcut)
    {
           if ((shortcut.Category == ShortcutDefinitionCategory.Executable ||
               (shortcut.Category == ShortcutDefinitionCategory.Application && shortcut.ApplicationLibrary != 2)) &&
            (string.IsNullOrWhiteSpace(shortcut.ExecutablePath) || !File.Exists(shortcut.ExecutablePath) ||
             (shortcut.ExecutableArgumentsRequired && string.IsNullOrWhiteSpace(shortcut.ExecutableArguments)) ||
             string.IsNullOrWhiteSpace(GetExecutableMonitorPath(shortcut)) || !File.Exists(GetExecutableMonitorPath(shortcut))))
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

    private static async Task<bool> RunPreGameProgramsAsync(ShortcutDefinition shortcut, List<StartedProgram> startedPrograms, List<StoppedProgram> stoppedProgramsToRestart, WindowPlacementRun windowRun, Func<RequestOperationDecisionRequest, CancellationToken, Task<OperationDecision>>? requestDecisionAsync, Guid operationId, CancellationToken cancellationToken)
    {
        IReadOnlyList<PreGameProgramAction> actions = BuildPreGameProgramActions(shortcut);

        int executionOrder = 0;
        foreach (PreGameProgramAction action in actions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? failure = null;
            if (action.StopProgram != null)
            {
                if (!StopProgram(action.StopProgram, executionOrder, stoppedProgramsToRestart))
                {
                    failure = $"Could not stop '{action.StopProgram.ExecutablePath}'.";
                }
            }
            else if (action.StartProgram == null || !StartProgram(action.StartProgram, executionOrder, startedPrograms, windowRun))
            {
                failure = $"Could not start '{action.StartProgram?.ExecutablePath ?? action.StartProgram?.ApplicationId ?? "the configured program"}'.";
            }

            if (failure != null && !await ShouldContinueAfterFailureAsync(requestDecisionAsync, operationId, "Shortcut program step failed", failure, cancellationToken).ConfigureAwait(false))
            {
                return false;
            }

            executionOrder++;
        }

        return true;
    }

    internal static IReadOnlyList<PreGameProgramAction> BuildPreGameProgramActions(ShortcutDefinition shortcut)
    {
        ArgumentNullException.ThrowIfNull(shortcut);
        List<PreGameProgramAction> actions = new List<PreGameProgramAction>();
        int sequence = 0;
        actions.AddRange(shortcut.StartPrograms.Where(program => !program.Disabled).Select(program => new PreGameProgramAction(program.Priority, sequence++, program, null)));
        actions.AddRange(shortcut.StopPrograms.Where(program => !program.Disabled).Select(program => new PreGameProgramAction(program.Priority, sequence++, null, program)));
        return actions.OrderBy(action => action.Priority).ThenBy(action => action.Sequence).ToArray();
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

    private static bool StartProgram(ShortcutStartProgramDefinition program, int sequence, List<StartedProgram> startedPrograms, WindowPlacementRun windowRun)
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
                startedPrograms.Add(new StartedProgram(program.Priority, sequence, new List<Process> { uwpProcess }, null));
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

        if (program.DoNotStartIfAlreadyRunning && ProcessTreeMonitor.IsExecutableRunning(program.ExecutablePath))
            return true;

        ProcessTreeMonitor? monitor = ProcessTreeMonitor.BeginWatching(program.ExecutablePath, 10, continueTrackingDescendants: true);
        List<Process> startedProcesses = ProcessUtils.StartProcess(program.ExecutablePath, program.Arguments, (ProcessPriority)(int)program.ProcessPriority, 10, program.RunAsAdministrator);
        monitor?.RegisterLaunchedProcesses(startedProcesses);
        if (startedProcesses.Count == 0)
        {
            monitor?.Dispose();
            return false;
        }
        if (monitor != null)
        {
            windowRun.Monitors.Add((program.Id, monitor));
            if (!program.CloseOnFinish) windowRun.OwnedHelperMonitors.Add(monitor);
        }

        if (program.CloseOnFinish)
        {
            startedPrograms.Add(new StartedProgram(program.Priority, sequence, startedProcesses, monitor));
        }
        else
        {
            ProcessUtils.DisposeProcesses(startedProcesses);
        }

        return true;
    }

    private static bool StopProgram(ShortcutStopProgramDefinition program, int sequence, List<StoppedProgram> stoppedProgramsToRestart)
    {
        if (string.IsNullOrWhiteSpace(program.ExecutablePath))
        {
            return false;
        }

        ProcessTreeMonitor.ProcessScan scan = ProcessTreeMonitor.FindRunningProcesses(program.ExecutablePath);
        if (!scan.IsComplete)
        {
            _logger.Warn("ShortcutRunner/StopProgram: Could not safely identify all processes for {0}; no processes were stopped.", program.ExecutablePath);
            return false;
        }
        List<Process> runningProcesses = new List<Process>();
        try
        {
            foreach (ProcessIdentity identity in scan.Identities)
            {
                try
                {
                    Process process = Process.GetProcessById(identity.ProcessId);
                    try
                    {
                        if (!process.HasExited && process.StartTime.ToUniversalTime() == identity.StartTimeUtc)
                        {
                            runningProcesses.Add(process);
                            process = null!;
                        }
                    }
                    finally { process?.Dispose(); }
                }
                catch (ArgumentException) { /* The selected process has already exited. */ }
            }
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
                stoppedProgramsToRestart.Add(new StoppedProgram(program, sequence));
            }

            return true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            _logger.Warn(ex, "ShortcutRunner/StopProgram: Could not verify selected process identities for {0}.", program.ExecutablePath);
            return false;
        }
        finally
        {
            ProcessUtils.DisposeProcesses(runningProcesses);
        }
    }

    private static void RunPostGameProgramCleanup(List<StartedProgram> startedPrograms, List<StoppedProgram> stoppedProgramsToRestart)
    {
        List<PreGameCleanupAction> cleanupActions = new List<PreGameCleanupAction>();
        cleanupActions.AddRange(startedPrograms.Select(program => new PreGameCleanupAction(program.Sequence, program, null)));
        cleanupActions.AddRange(stoppedProgramsToRestart.Select(program => new PreGameCleanupAction(program.Sequence, null, program)));

        foreach (PreGameCleanupAction action in cleanupActions.OrderByDescending(action => action.Sequence))
        {
            if (action.StartedProgram != null)
            {
                try
                {
                    List<Process> processesToStop = action.StartedProgram.Monitor?.GetTrackedProcesses(refresh: true) ?? new List<Process>();
                    foreach (Process process in action.StartedProgram.Processes)
                    {
                        if (action.StartedProgram.Monitor != null && !action.StartedProgram.Monitor.IsInDirectLaunchTree(process.Id)) continue;
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
                    ProcessUtils.DisposeProcesses(action.StartedProgram.Processes);
                    action.StartedProgram.Monitor?.Dispose();
                }
            }
            else if (action.StoppedProgram != null)
            {
                try
                {
                    ShortcutStopProgramDefinition stoppedProgram = action.StoppedProgram.Program;
                    ProcessUtils.DisposeProcesses(ProcessUtils.StartProcess(stoppedProgram.ExecutablePath, string.Empty, (ProcessPriority)(int)stoppedProgram.RestartProcessPriority, 10, stoppedProgram.RunAsAdministrator));
                }
                catch
                {
                    // Cleanup continues so other temporary shortcut state is still restored.
                }
            }
        }
        stoppedProgramsToRestart.Clear();
        startedPrograms.Clear();
    }

    private static void RunAfterPrograms(ShortcutDefinition shortcut)
    {
        foreach (ShortcutAfterProgramDefinition afterProgram in shortcut.AfterPrograms.Where(program => !program.Disabled).OrderBy(program => program.Priority))
        {
            if (string.IsNullOrWhiteSpace(afterProgram.ExecutablePath) || !File.Exists(afterProgram.ExecutablePath) || (afterProgram.ArgumentsRequired && string.IsNullOrWhiteSpace(afterProgram.Arguments)))
            {
                continue;
            }

            if (afterProgram.DoNotStartIfAlreadyRunning && ProcessTreeMonitor.IsExecutableRunning(afterProgram.ExecutablePath))
                continue;

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
            recoveryRestored = (await _profileOperationService.ApplyDisplayProfileAsync(recoveryRecord.DisplayProfileId, cancellationToken, recoveryRecord.DisplayProfileSnapshotJson).ConfigureAwait(false)).IsSuccessful;
        }
        if (recoveryRecord.RequiresAudioRestore)
        {
            recoveryRestored = _profileOperationService.ApplyAudioProfile(recoveryRecord.AudioProfileId, audioDeviceWaitMilliseconds, recoveryRecord.AudioProfileSnapshotJson).IsSuccessful && recoveryRestored;
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

    private static async Task RestoreWindowPlacementsAsync(WindowPlacementRun run, Func<OperationStatusUpdate, CancellationToken, Task>? publishStatusAsync, CancellationToken cancellationToken)
    {
        if (run.DisplayProfileFailed || run.Shortcut.WindowPlacements.Count == 0) return;
        HashSet<int> completed = new HashSet<int>();
        Dictionary<int, WindowPlacementService.PendingPlacement> pending = new Dictionary<int, WindowPlacementService.PendingPlacement>();
        List<string> programNames = new List<string>();
        int moved = 0;
        DateTime deadlineUtc = DateTime.UtcNow.AddSeconds(Math.Clamp(run.Shortcut.WindowPlacementTimeoutSeconds, 1, 120));
        while (DateTime.UtcNow < deadlineUtc && completed.Count < run.Shortcut.WindowPlacements.Count)
        {
            cancellationToken.ThrowIfCancellationRequested();
            (int count, string[] programs) = WindowPlacementService.Restore(run.GetSources(), run.Shortcut.WindowPlacements, completed, pending);
            moved += count;
            programNames.AddRange(programs);
            if (completed.Count < run.Shortcut.WindowPlacements.Count) await Task.Delay(500, cancellationToken).ConfigureAwait(false);
        }
        if (pending.Count > 0)
        {
            (int count, string[] programs) = WindowPlacementService.Restore(run.GetSources(), run.Shortcut.WindowPlacements, completed, pending, verifyOnly: true);
            moved += count;
            programNames.AddRange(programs);
        }
        string[] names = programNames.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        string message = $"Moved {moved} window{(moved == 1 ? string.Empty : "s")}";
        if (names.Length > 0) message += $": {string.Join(", ", names)}";
        if (completed.Count < run.Shortcut.WindowPlacements.Count)
        {
            _logger.Warn("ShortcutRunner/RestoreWindowPlacementsAsync: Placed {0} of {1} saved windows for shortcut {2}.", moved, run.Shortcut.WindowPlacements.Count, run.Shortcut.Id);
            for (int index = 0; index < run.Shortcut.WindowPlacements.Count; index++)
                if (!completed.Contains(index)) _logger.Warn("ShortcutRunner/RestoreWindowPlacementsAsync: Timed out matching window class {0} for {1} in shortcut {2}.", run.Shortcut.WindowPlacements[index].WindowClass, run.Shortcut.WindowPlacements[index].ExecutablePath, run.Shortcut.Id);
        }
        await PublishStatusAsync(publishStatusAsync, run.OperationId, OperationPhase.RestoringWindowPositions, message, cancellationToken).ConfigureAwait(false);
    }

    private sealed class WindowPlacementRun
    {
        public WindowPlacementRun(Guid operationId, ShortcutDefinition shortcut) { OperationId = operationId; Shortcut = shortcut; }
        public Guid OperationId { get; }
        public ShortcutDefinition Shortcut { get; }
        public bool DisplayProfileFailed { get; set; }
        public bool CanSave { get; set; }
        public bool RequireMainSource { get; set; }
        public List<(string ProgramId, ProcessTreeMonitor Monitor)> Monitors { get; } = new List<(string, ProcessTreeMonitor)>();
        public List<ProcessTreeMonitor> OwnedHelperMonitors { get; } = new List<ProcessTreeMonitor>();

        public List<WindowPlacementService.Source> GetSources()
            => GetSources(out _);

        public List<WindowPlacementService.Source> GetSources(out bool isComplete)
        {
            isComplete = true;
            List<WindowPlacementService.Source> sources = new List<WindowPlacementService.Source>();
            foreach ((string programId, ProcessTreeMonitor monitor) in Monitors)
            {
                foreach (Process process in monitor.GetTrackedProcesses())
                {
                    try
                    {
                        string path = process.MainModule?.FileName ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(path)) sources.Add(new WindowPlacementService.Source(programId, path, new HashSet<int> { process.Id }, monitor.IsInDirectLaunchTree(process.Id)));
                        else isComplete = false;
                    }
                    catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
                    {
                        isComplete = false;
                        _logger.Debug(ex, "ShortcutRunner/GetSources: Could not inspect tracked process for shortcut {0}.", Shortcut.Id);
                    }
                    finally { process.Dispose(); }
                }
                if (monitor.HadTrackingError) isComplete = false;
            }
            if (RequireMainSource && !sources.Any(item => item.ProgramId == "main")) isComplete = false;
            return sources;
        }
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

    private sealed record StartedProgram(int Priority, int Sequence, List<Process> Processes, ProcessTreeMonitor? Monitor);

    private sealed record StoppedProgram(ShortcutStopProgramDefinition Program, int Sequence);

    private sealed record PreGameCleanupAction(int Sequence, StartedProgram? StartedProgram, StoppedProgram? StoppedProgram);
}

internal sealed record PreGameProgramAction(int Priority, int Sequence, ShortcutStartProgramDefinition? StartProgram, ShortcutStopProgramDefinition? StopProgram);

public sealed record ShortcutRunResult(Guid OperationId, ShortcutRunOutcome Outcome, ShortcutDefinition? Shortcut = null)
{
    public string Message { get; init; } = string.Empty;
    public bool RequiresRecovery { get; init; }
}

public enum ShortcutRunOutcome
{
    Prepared = 0,
    ShortcutNotFound = 1,
    Cancelled = 2,
    Failed = 3,
    Completed = 4
}
