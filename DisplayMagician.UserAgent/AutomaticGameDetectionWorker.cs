using System;
using System.Collections.Generic;
using System.IO;
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
    private readonly Func<string, ProcessIdentity, CancellationToken, Task<bool>> _runDetectedShortcutAsync;
    private readonly AutomaticGameDetectionStateTracker _stateTracker = new AutomaticGameDetectionStateTracker();
    private readonly HashSet<string> _invalidMonitorTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _monitorPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public AutomaticGameDetectionWorker(AutomaticGameDetectionRegistry registry, Func<string, ProcessIdentity, CancellationToken, Task<bool>> runDetectedShortcutAsync)
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
                    _monitorPaths.Remove(shortcutId);
                    _invalidMonitorTargets.Remove(shortcutId);
                }

                foreach (ShortcutDefinition shortcut in shortcuts)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Game? game = GameLibrary.GetAnyGameById(shortcut.GameAppId, (SupportedGameLibraryType)shortcut.GameLibrary);
                    if (game == null)
                    {
                        _stateTracker.Remove(shortcut.Id);
                        continue;
                    }

                    string executablePath = shortcut.MonitorDifferentGameExecutable
                        ? shortcut.DifferentGameExecutablePathToMonitor : game.ExePath;
                    if (string.IsNullOrWhiteSpace(executablePath) ||
                        !string.Equals(Path.GetExtension(executablePath), ".exe", StringComparison.OrdinalIgnoreCase) ||
                        !File.Exists(executablePath))
                    {
                        _stateTracker.Remove(shortcut.Id);
                        _monitorPaths.Remove(shortcut.Id);
                        if (_invalidMonitorTargets.Add(shortcut.Id))
                            Logger.Warn("AutomaticGameDetectionWorker/RunAsync: Shortcut {0} needs a running game .exe path; set Monitor different executable.", shortcut.Id);
                        continue;
                    }

                    _invalidMonitorTargets.Remove(shortcut.Id);
                    if (!_monitorPaths.TryGetValue(shortcut.Id, out string? previousPath) ||
                        !string.Equals(previousPath, executablePath, StringComparison.OrdinalIgnoreCase))
                    {
                        _stateTracker.Remove(shortcut.Id);
                        _monitorPaths[shortcut.Id] = executablePath;
                    }

                    ProcessTreeMonitor.ProcessScan scan = ProcessTreeMonitor.FindRunningProcesses(executablePath);
                    if (!scan.IsComplete) continue;
                    ProcessIdentity? newlyStarted = _stateTracker.Observe(shortcut.Id, scan.Identities);
                    if (!newlyStarted.HasValue) continue;
                    Logger.Info("AutomaticGameDetectionWorker/RunAsync: Detected new game PID {0} for shortcut {1}.", newlyStarted.Value.ProcessId, shortcut.Id);
                    bool wasStarted = await _runDetectedShortcutAsync(shortcut.Id, newlyStarted.Value, cancellationToken).ConfigureAwait(false);
                    if (wasStarted)
                        _stateTracker.MarkHandled(shortcut.Id, newlyStarted.Value);
                    else
                        Logger.Debug("AutomaticGameDetectionWorker/RunAsync: Automatic shortcut {0} was deferred because another operation is active or display control is unavailable.", shortcut.Id);
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

}
