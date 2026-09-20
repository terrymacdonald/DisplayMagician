using System;
using System.Collections.Generic;
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

    public ShortcutRunner(ShortcutStore shortcutStore, AutomaticGameDetectionRegistry automaticGameDetectionRegistry, UserProfileOperationService profileOperationService)
    {
        _shortcutStore = shortcutStore ?? throw new ArgumentNullException(nameof(shortcutStore));
        _automaticGameDetectionRegistry = automaticGameDetectionRegistry ?? throw new ArgumentNullException(nameof(automaticGameDetectionRegistry));
        _profileOperationService = profileOperationService ?? throw new ArgumentNullException(nameof(profileOperationService));
    }

    public Task<ShortcutRunResult> PrepareRunAsync(string shortcutId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
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
        bool automaticDetectionWasSuspended = isManualRun && shortcut.GameLaunchMode == GameLaunchMode.DetectGameRunning &&
            _automaticGameDetectionRegistry.SuspendAutomaticDetectionForManualRun(shortcut.Id);
        UserProfileOperationState originalState = _profileOperationService.CaptureCurrentProfileState();
        try
        {
            if (!string.IsNullOrWhiteSpace(shortcut.ProfileId))
            {
                ApplyDisplayProfileOperationResult displayResult = await _profileOperationService.ApplyDisplayProfileAsync(shortcut.ProfileId, cancellationToken).ConfigureAwait(false);
                if (!displayResult.IsSuccessful)
                {
                    return new ShortcutRunResult(preparedRun.OperationId, displayResult.WasCancelled ? ShortcutRunOutcome.Cancelled : ShortcutRunOutcome.Failed, shortcut);
                }
            }

            if (!string.IsNullOrWhiteSpace(shortcut.AudioProfileId) && !_profileOperationService.ApplyAudioProfile(shortcut.AudioProfileId, audioDeviceWaitMilliseconds).IsSuccessful)
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
                }

                while (IsGameRunning(shortcut, game))
                {
                    await Task.Delay(500, cancellationToken).ConfigureAwait(false);
                }
            }

            return new ShortcutRunResult(preparedRun.OperationId, ShortcutRunOutcome.Completed, shortcut);
        }
        finally
        {
            if (shortcut.DisplayPermanence == ShortcutDefinitionPermanence.Temporary && !string.IsNullOrWhiteSpace(originalState.DisplayProfileId))
            {
                await _profileOperationService.ApplyDisplayProfileAsync(originalState.DisplayProfileId, CancellationToken.None).ConfigureAwait(false);
            }
            if (shortcut.AudioPermanence == ShortcutDefinitionPermanence.Temporary && !string.IsNullOrWhiteSpace(originalState.AudioProfileId))
            {
                _profileOperationService.ApplyAudioProfile(originalState.AudioProfileId, audioDeviceWaitMilliseconds);
            }
            if (automaticDetectionWasSuspended)
            {
                _automaticGameDetectionRegistry.RestoreAutomaticDetectionAfterManualRun(shortcut.Id);
            }
        }
    }

    private static bool IsGameRunning(ShortcutDefinition shortcut, Game game)
    {
        return shortcut.MonitorDifferentGameExecutable && !string.IsNullOrWhiteSpace(shortcut.DifferentGameExecutablePathToMonitor)
            ? !ProcessUtils.ProcessExited(shortcut.DifferentGameExecutablePathToMonitor)
            : game.IsRunning;
    }
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
