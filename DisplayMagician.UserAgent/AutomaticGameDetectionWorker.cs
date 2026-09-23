using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Contracts;
using DisplayMagician.GameLibraries;
using DisplayMagician.Processes;

namespace DisplayMagician.UserAgent;

/// <summary>Detects game starts for registered automatic shortcuts in the interactive user session.</summary>
public sealed class AutomaticGameDetectionWorker
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly AutomaticGameDetectionRegistry _registry;
    private readonly Func<string, CancellationToken, Task<bool>> _runDetectedShortcutAsync;
    private readonly AutomaticGameDetectionStateTracker _stateTracker = new AutomaticGameDetectionStateTracker();

    public AutomaticGameDetectionWorker(AutomaticGameDetectionRegistry registry, Func<string, CancellationToken, Task<bool>> runDetectedShortcutAsync)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _runDetectedShortcutAsync = runDetectedShortcutAsync ?? throw new ArgumentNullException(nameof(runDetectedShortcutAsync));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (!GameLibrary.GamesLoaded)
        {
            await Task.Run(GameLibrary.LoadGamesInBackground, cancellationToken).ConfigureAwait(false);
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                IReadOnlyList<ShortcutDefinition> shortcuts = _registry.GetRegisteredShortcuts();
                HashSet<string> registeredShortcutIds = new HashSet<string>(shortcuts.Select(shortcut => shortcut.Id), StringComparer.OrdinalIgnoreCase);
                foreach (string shortcutId in _stateTracker.GetTrackedShortcutIds().Where(shortcutId => !registeredShortcutIds.Contains(shortcutId)))
                {
                    _stateTracker.Remove(shortcutId);
                }

                foreach (ShortcutDefinition shortcut in shortcuts)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Game? game = GameLibrary.GetAnyGameById(shortcut.GameAppId);
                    if (game == null)
                    {
                        continue;
                    }

                    bool isRunning = IsGameRunning(shortcut, game);
                    if (_stateTracker.HasNewlyStarted(shortcut.Id, isRunning))
                    {
                        Logger.Info($"AutomaticGameDetectionWorker/RunAsync: Detected game start for shortcut '{shortcut.Name}' ({shortcut.Id}).");
                        bool wasStarted = await _runDetectedShortcutAsync(shortcut.Id, cancellationToken).ConfigureAwait(false);
                        if (wasStarted)
                        {
                            _stateTracker.HasNewlyStarted(shortcut.Id, false);
                        }
                        else
                        {
                            Logger.Debug("AutomaticGameDetectionWorker/RunAsync: Automatic shortcut {0} was deferred because another operation is active or display control is unavailable.", shortcut.Id);
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "AutomaticGameDetectionWorker/RunAsync: Unable to evaluate automatic game detections.");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool IsGameRunning(ShortcutDefinition shortcut, Game game)
    {
        return shortcut.MonitorDifferentGameExecutable && !string.IsNullOrWhiteSpace(shortcut.DifferentGameExecutablePathToMonitor)
            ? ProcessTreeMonitor.IsExecutableRunning(shortcut.DifferentGameExecutablePathToMonitor)
            : game.IsRunning;
    }
}
