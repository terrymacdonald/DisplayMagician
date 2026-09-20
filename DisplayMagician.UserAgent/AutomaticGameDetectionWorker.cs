using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.GameLibraries;
using DisplayMagician.Processes;

namespace DisplayMagician.UserAgent;

/// <summary>Detects game starts for registered automatic shortcuts in the interactive user session.</summary>
public sealed class AutomaticGameDetectionWorker
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly AutomaticGameDetectionRegistry _registry;
    private readonly ShortcutRunner _shortcutRunner;
    private readonly AutomaticGameDetectionStateTracker _stateTracker = new AutomaticGameDetectionStateTracker();

    public AutomaticGameDetectionWorker(AutomaticGameDetectionRegistry registry, ShortcutRunner shortcutRunner)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _shortcutRunner = shortcutRunner ?? throw new ArgumentNullException(nameof(shortcutRunner));
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
                        await _shortcutRunner.ApplyDetectedGameShortcutAsync(shortcut.Id, 0, cancellationToken).ConfigureAwait(false);
                        _stateTracker.HasNewlyStarted(shortcut.Id, false);
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
            ? !ProcessUtils.ProcessExited(shortcut.DifferentGameExecutablePathToMonitor)
            : game.IsRunning;
    }
}