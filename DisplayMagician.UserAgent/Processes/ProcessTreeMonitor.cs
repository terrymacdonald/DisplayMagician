using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace DisplayMagician.Processes
{
    public sealed class ProcessTreeMonitor : IDisposable
    {
        internal sealed record ProcessScan(IReadOnlyList<ProcessIdentity> Identities, bool IsComplete);
        internal sealed record ProcessSnapshot(int ProcessId, int ParentProcessId, string ExecutableName);
        internal sealed record ProcessSnapshotScan(List<ProcessSnapshot> Processes, DateTime CapturedUtc, bool IsComplete);
        private const int MaximumSnapshotFailures = 3;
        private static readonly TimeSpan SnapshotRetryInterval = TimeSpan.FromMilliseconds(250);
        private const uint SnapshotProcesses = 0x00000002;
        private const uint ProcessQueryLimitedInformation = 0x00001000;
        private const int MaximumPathLength = 32768;
        private static readonly IntPtr InvalidHandle = new IntPtr(-1);
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly object _syncRoot = new object();
        private readonly string _expectedExecutablePath;
        private readonly string _expectedExecutableName;
        private readonly Dictionary<int, DateTime> _existingExpectedProcessIds = new Dictionary<int, DateTime>();
        private readonly HashSet<int> _directLaunchTreeProcessIds = new HashSet<int>();
        private readonly Dictionary<int, DateTime> _trackedProcessStartTimes = new Dictionary<int, DateTime>();
        private readonly HashSet<ProcessIdentity> _observedExpectedProcesses = new HashSet<ProcessIdentity>();
        private readonly Dictionary<ProcessIdentity, ProcessIdentity> _trackedParents = new Dictionary<ProcessIdentity, ProcessIdentity>();
        private readonly DateTime _deadlineUtc;
        private readonly DateTime _watchStartedUtc;
        private readonly bool _continueTrackingDescendants;
        private readonly Func<ProcessSnapshotScan> _captureSnapshot;
        private Timer? _snapshotTimer;
        private int _snapshotInProgress;
        private int _completedSnapshotCount;
        private int _minimumSnapshotCountForDirectLaunch;
        private bool _hasObservedExpectedProcess;
        private bool _discoveryComplete;
        private volatile bool _disposed;
        private bool _usingLongInterval;
        private bool _trackOnlySelectedTree;
        private volatile bool _hadTrackingError;
        private int _consecutiveSnapshotFailures;
        private DateTime _nextSnapshotAttemptUtc;
        private bool _lastSnapshotSucceeded;
        private bool _trackingFailed;

        public bool IsTrackingFailed { get { lock (_syncRoot) return _trackingFailed; } }

        public bool HadTrackingError { get { lock (_syncRoot) return _hadTrackingError || _consecutiveSnapshotFailures > 0 || _trackingFailed; } }

        public bool IsInDirectLaunchTree(int processId)
        {
            lock (_syncRoot)
                return _directLaunchTreeProcessIds.Contains(processId);
        }

        internal ProcessTreeMonitor(string expectedExecutablePath, int startTimeout, bool continueTrackingDescendants, Func<ProcessSnapshotScan>? captureSnapshot = null)
        {
            _captureSnapshot = captureSnapshot ?? CaptureProcessSnapshot;
            _continueTrackingDescendants = continueTrackingDescendants;
            _expectedExecutablePath = Path.GetFullPath(expectedExecutablePath);
            _expectedExecutableName = Path.GetFileName(expectedExecutablePath);
            _watchStartedUtc = DateTime.UtcNow;
            _deadlineUtc = _watchStartedUtc.AddSeconds(Math.Clamp(startTimeout, 1, 60));

            foreach (Process process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_expectedExecutableName)))
            {
                try
                {
                    if (PathsMatch(process))
                        _existingExpectedProcessIds[process.Id] = process.StartTime.ToUniversalTime();
                }
                catch (Exception ex)
                {
                    logger.Trace(ex, $"ProcessTreeMonitor/ProcessTreeMonitor: Could not inspect an existing process for {_expectedExecutablePath}.");
                }
                finally
                {
                    process.Dispose();
                }
            }
        }

        public bool HasObservedExpectedProcess
        {
            get
            {
                lock (_syncRoot)
                    return _hasObservedExpectedProcess;
            }
        }

        public bool IsDiscoveryComplete
        {
            get
            {
                lock (_syncRoot)
                    return _discoveryComplete;
            }
        }

        public bool IsRunning
        {
            get
            {
                if (_continueTrackingDescendants) CaptureProcessTree(null);
                ThrowIfTrackingFailed();
                KeyValuePair<int, DateTime>[] processIds;
                bool waitingForDirectLaunchSnapshot;
                lock (_syncRoot)
                {
                    processIds = _trackedProcessStartTimes.ToArray();
                    waitingForDirectLaunchSnapshot = _hasObservedExpectedProcess &&
                        !_discoveryComplete &&
                        _completedSnapshotCount < _minimumSnapshotCountForDirectLaunch;
                }

                foreach (KeyValuePair<int, DateTime> tracked in processIds)
                {
                    try
                    {
                        using (Process process = Process.GetProcessById(tracked.Key))
                        {
                            if (!process.HasExited && process.StartTime.ToUniversalTime() == tracked.Value)
                                return true;
                        }
                    }
                    catch (ArgumentException)
                    {
                        // The process has exited.
                    }
                    catch (Exception ex)
                    {
                        logger.Trace(ex, $"ProcessTreeMonitor/IsRunning: Could not query tracked PID {tracked.Key}.");
                    }
                }

                // A launcher may already have exited while its child process is starting.
                // Do not report the tree as closed until a snapshot taken after the launch has
                // had an opportunity to add that child to the tracked process tree.
                if (_continueTrackingDescendants)
                {
                    // A parent can spawn a child and exit between the scan and the checks
                    // above. Take one final scan before declaring the selected tree closed.
                    CaptureProcessTree(null);
                    ThrowIfTrackingFailed();
                    lock (_syncRoot)
                        return !_lastSnapshotSucceeded || waitingForDirectLaunchSnapshot || Volatile.Read(ref _snapshotInProgress) != 0 ||
                            _trackedProcessStartTimes.Any(item => !processIds.Contains(item));
                }
                lock (_syncRoot) return (_consecutiveSnapshotFailures > 0 && !_discoveryComplete) || waitingForDirectLaunchSnapshot;
            }
        }

        internal bool SelectNewestExpectedProcessTree()
        {
            CaptureProcessTree(null);
            ThrowIfTrackingFailed();
            lock (_syncRoot)
            {
                if (_trackOnlySelectedTree) return true;
                if (_observedExpectedProcesses.Count == 0) return false;
                ProcessIdentity selected = _observedExpectedProcesses.OrderByDescending(item => item.StartTimeUtc).First();
                SelectProcessTrees(new HashSet<ProcessIdentity> { selected });
                logger.Info("ProcessTreeMonitor/SelectNewestExpectedProcessTree: Selected PID {0}, started {1:O}, for {2}.", selected.ProcessId, selected.StartTimeUtc, _expectedExecutablePath);
                return true;
            }
        }

        private void SelectProcessTrees(HashSet<ProcessIdentity> roots)
        {
            lock (_syncRoot)
            {
                _trackOnlySelectedTree = true;
                _hasObservedExpectedProcess = roots.Count > 0;
                _directLaunchTreeProcessIds.Clear();
                foreach (KeyValuePair<int, DateTime> tracked in _trackedProcessStartTimes.ToArray())
                {
                    ProcessIdentity identity = new ProcessIdentity(tracked.Key, tracked.Value);
                    HashSet<ProcessIdentity> visited = new HashSet<ProcessIdentity>();
                    while (!roots.Contains(identity) && visited.Add(identity) && _trackedParents.TryGetValue(identity, out ProcessIdentity parent))
                        identity = parent;
                    if (roots.Contains(identity)) _directLaunchTreeProcessIds.Add(tracked.Key);
                    else _trackedProcessStartTimes.Remove(tracked.Key);
                }
                _observedExpectedProcesses.Clear();
                _trackedParents.Clear();
            }
        }

        public List<Process> GetTrackedProcesses()
            => GetTrackedProcesses(refresh: false);

        internal List<Process> GetTrackedProcesses(bool refresh)
        {
            if (refresh) CaptureProcessTree(null);
            List<Process> processes = new List<Process>();
            KeyValuePair<int, DateTime>[] processIds;
            lock (_syncRoot)
                processIds = _trackedProcessStartTimes.ToArray();

            foreach (KeyValuePair<int, DateTime> tracked in processIds)
            {
                Process? process = null;
                try
                {
                    process = Process.GetProcessById(tracked.Key);
                    if (!process.HasExited && process.StartTime.ToUniversalTime() == tracked.Value)
                    {
                        processes.Add(process);
                        process = null;
                    }
                }
                catch (ArgumentException)
                {
                    // The process has exited.
                }
                catch (Exception ex)
                {
                    if (ex is System.ComponentModel.Win32Exception) _hadTrackingError = true;
                    logger.Trace(ex, $"ProcessTreeMonitor/GetTrackedProcesses: Could not query tracked PID {tracked.Key}.");
                }
                finally { process?.Dispose(); }
            }

            return processes;
        }

        public void RegisterLaunchedProcesses(IEnumerable<Process?>? launchedProcesses)
            => RegisterLaunchedProcesses(launchedProcesses, requireExpectedExecutable: false);

        internal void RegisterLaunchedProcesses(IEnumerable<Process?>? launchedProcesses, bool requireExpectedExecutable)
        {
            if (launchedProcesses == null)
                return;

            bool registeredLaunchedProcess = false;
            HashSet<ProcessIdentity> launchRoots = new HashSet<ProcessIdentity>();
            lock (_syncRoot)
                foreach (int processId in _directLaunchTreeProcessIds)
                    if (_trackedProcessStartTimes.TryGetValue(processId, out DateTime startedUtc))
                        launchRoots.Add(new ProcessIdentity(processId, startedUtc));
            foreach (Process? process in launchedProcesses)
            {
                try
                {
                    if (process == null)
                        continue;

                    if (requireExpectedExecutable)
                    {
                        // Libraries may return their persistent client (Steam, etc.) rather
                        // than the game. Accept only the configured monitoring executable.
                        string launchPath = process.StartInfo.FileName;
                        bool launchedExpected = Path.IsPathFullyQualified(launchPath) &&
                            string.Equals(Path.GetFullPath(launchPath), _expectedExecutablePath, StringComparison.OrdinalIgnoreCase);
                        if (!launchedExpected && !PathsMatch(process)) continue;
                    }

                    lock (_syncRoot)
                    {
                        if (_disposed) return;
                        DateTime startedUtc = process.StartTime.ToUniversalTime();
                        if (startedUtc < _watchStartedUtc) continue;
                        if (_existingExpectedProcessIds.TryGetValue(process.Id, out DateTime existingStartedUtc) && existingStartedUtc == startedUtc)
                            continue;
                        if (requireExpectedExecutable) _observedExpectedProcesses.Add(new ProcessIdentity(process.Id, startedUtc));
                        _directLaunchTreeProcessIds.Add(process.Id);
                        launchRoots.Add(new ProcessIdentity(process.Id, startedUtc));
                        if (!_trackedProcessStartTimes.TryGetValue(process.Id, out DateTime trackedStart) || trackedStart != startedUtc)
                        {
                            _trackedProcessStartTimes[process.Id] = startedUtc;
                            _hasObservedExpectedProcess = true;
                            int snapshotsRequired = _completedSnapshotCount +
                                (Volatile.Read(ref _snapshotInProgress) != 0 ? 2 : 1);
                            _minimumSnapshotCountForDirectLaunch = Math.Max(_minimumSnapshotCountForDirectLaunch, snapshotsRequired);
                            registeredLaunchedProcess = true;
                            // A bootstrapper can exit before Process.Start returns its handle. Keep its
                            // PID so the following native snapshots can still identify its live children.
                            logger.Debug($"ProcessTreeMonitor/RegisterLaunchedProcesses: Tracking launched PID {process.Id} for {_expectedExecutablePath} as the expected executable.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex is System.ComponentModel.Win32Exception) _hadTrackingError = true;
                    logger.Trace(ex, $"ProcessTreeMonitor/RegisterLaunchedProcesses: Could not register a launched process for {_expectedExecutablePath}.");
                }
            }

            // Direct launches have authoritative roots. Discovery remains open for game
            // library/alternative-executable callers until they explicitly select a root.
            if (!requireExpectedExecutable) SelectProcessTrees(launchRoots);
            if (registeredLaunchedProcess)
            {
                // Capture any child that was started before the launcher returned control.
                // If a timer snapshot is already in progress, IsRunning waits for the next one.
                CaptureProcessTree(null);
            }
        }

        public static ProcessTreeMonitor? BeginWatching(string expectedExecutablePath, int startTimeout)
            => BeginWatching(expectedExecutablePath, startTimeout, continueTrackingDescendants: false);

        internal static ProcessTreeMonitor? BeginWatching(string expectedExecutablePath, int startTimeout, bool continueTrackingDescendants)
        {
            if (string.IsNullOrWhiteSpace(expectedExecutablePath) || !File.Exists(expectedExecutablePath))
                return null;

            ProcessTreeMonitor monitor = new ProcessTreeMonitor(expectedExecutablePath, startTimeout, continueTrackingDescendants);
            monitor.Start();
            return monitor;
        }

        internal static ProcessScan FindRunningProcesses(string executablePath)
        {
            List<ProcessIdentity> identities = new List<ProcessIdentity>();
            if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath) ||
                !string.Equals(Path.GetExtension(executablePath), ".exe", StringComparison.OrdinalIgnoreCase))
                return new ProcessScan(identities, false);

            string expectedPath = Path.GetFullPath(executablePath);
            bool complete = true;
            foreach (Process process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(expectedPath)))
            {
                try
                {
                    string? actualPath = GetProcessImagePath(process.Id);
                    if (string.IsNullOrWhiteSpace(actualPath))
                    {
                        complete = false;
                        continue;
                    }
                    if (!process.HasExited &&
                        string.Equals(Path.GetFullPath(actualPath), expectedPath, StringComparison.OrdinalIgnoreCase))
                        identities.Add(new ProcessIdentity(process.Id, process.StartTime.ToUniversalTime()));
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
                {
                    if (ex is System.ComponentModel.Win32Exception) complete = false;
                    logger.Trace(ex, "ProcessTreeMonitor/FindRunningProcesses: Could not inspect a process for {0}.", executablePath);
                }
                finally { process.Dispose(); }
            }
            return new ProcessScan(identities, complete);
        }

        internal static ProcessTreeMonitor? BeginWatchingExisting(string expectedExecutablePath, ProcessIdentity identity, int startTimeout)
        {
            if (identity.ProcessId <= 0 || string.IsNullOrWhiteSpace(expectedExecutablePath) || !File.Exists(expectedExecutablePath))
                return null;

            ProcessTreeMonitor? monitor = null;
            try
            {
                monitor = new ProcessTreeMonitor(expectedExecutablePath, startTimeout, continueTrackingDescendants: true);
                using Process process = Process.GetProcessById(identity.ProcessId);
                if (process.HasExited || process.StartTime.ToUniversalTime() != identity.StartTimeUtc || !monitor.PathsMatch(process))
                {
                    monitor.Dispose();
                    return null;
                }

                monitor._trackOnlySelectedTree = true;
                monitor._trackedProcessStartTimes[identity.ProcessId] = identity.StartTimeUtc;
                monitor._directLaunchTreeProcessIds.Add(identity.ProcessId);
                monitor._hasObservedExpectedProcess = true;
                monitor.CaptureProcessTree(null);
                monitor.Start();
                return monitor;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
            {
                monitor?.Dispose();
                logger.Warn(ex, "ProcessTreeMonitor/BeginWatchingExisting: Could not attach to PID {0} for {1}.", identity.ProcessId, expectedExecutablePath);
                return null;
            }
        }

        public static bool IsExecutableRunning(string executablePath)
        {
            if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                return false;

            string expectedExecutablePath = Path.GetFullPath(executablePath);
            foreach (Process process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executablePath)))
            {
                try
                {
                    string? processPath = GetProcessImagePath(process.Id);
                    if (!string.IsNullOrWhiteSpace(processPath) &&
                        string.Equals(Path.GetFullPath(processPath), expectedExecutablePath, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Trace(ex, $"ProcessTreeMonitor/IsExecutableRunning: Could not inspect a process for {expectedExecutablePath}.");
                }
                finally
                {
                    process.Dispose();
                }
            }

            return false;
        }

        public static List<Process> StartAndCapture(string executable, string arguments, ProcessPriority processPriority, int startTimeout = 1, bool runAsAdministrator = false, bool captureDescendantsForStartupWindow = false)
        {
            ProcessTreeMonitor? monitor = BeginWatching(executable, startTimeout);
            List<Process> startedProcesses = ProcessUtils.StartProcess(executable, arguments, processPriority, startTimeout, runAsAdministrator);
            if (monitor == null)
                return startedProcesses;

            try
            {
                // Register the direct process immediately. Some applications replace this
                // short-lived launcher with a child process before the first timer snapshot.
                monitor.RegisterLaunchedProcesses(startedProcesses);

                while (DateTime.UtcNow < monitor._deadlineUtc &&
                    (captureDescendantsForStartupWindow || !monitor.HasObservedExpectedProcess))
                {
                    monitor.ThrowIfTrackingFailed();
                    Thread.Sleep(50);
                }

                monitor.ThrowIfTrackingFailed();
                List<Process> trackedProcesses = monitor.GetTrackedProcesses();
                if (trackedProcesses.Count == 0)
                    return startedProcesses;

                ProcessUtils.DisposeProcesses(startedProcesses);
                logger.Info($"ProcessTreeMonitor/StartAndCapture: Captured {trackedProcesses.Count} process(es) for {executable}{(captureDescendantsForStartupWindow ? " during the startup discovery window" : string.Empty)}.");
                return trackedProcesses;
            }
            finally
            {
                monitor.Dispose();
            }
        }

        private void Start()
        {
            lock (_syncRoot)
            {
                if (_disposed || _discoveryComplete) return;
                _snapshotTimer = new Timer(CaptureProcessTree, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                _snapshotTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(250));
            }
            logger.Debug($"ProcessTreeMonitor/Start: Capturing native process snapshots for {_expectedExecutablePath} until {_deadlineUtc:O}.");
        }

        private void CaptureProcessTree(object? state)
        {
            if (_disposed || IsDiscoveryComplete || Interlocked.Exchange(ref _snapshotInProgress, 1) != 0)
                return;

            try
            {
                lock (_syncRoot)
                    if (_consecutiveSnapshotFailures > 0 && DateTime.UtcNow < _nextSnapshotAttemptUtc) return;
                if (DateTime.UtcNow > _deadlineUtc && !_continueTrackingDescendants && _lastSnapshotSucceeded)
                {
                    CompleteDiscovery();
                    return;
                }

                ProcessSnapshotScan scan = _captureSnapshot();
                if (!scan.IsComplete || scan.Processes.Count == 0)
                {
                    RecordSnapshotFailure(null);
                    return;
                }
                List<ProcessSnapshot> runningProcesses = scan.Processes;
                if (_continueTrackingDescendants && DateTime.UtcNow > _deadlineUtc && !_usingLongInterval)
                {
                    _usingLongInterval = true;
                    _snapshotTimer?.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                }
                foreach (ProcessSnapshot process in runningProcesses)
                {
                    if (_trackOnlySelectedTree || DateTime.UtcNow > _deadlineUtc) break;
                    if (!string.Equals(process.ExecutableName, _expectedExecutableName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    TryTrackExpectedProcess(process.ProcessId, scan.CapturedUtc);
                }

                HashSet<int> deferredParents = new HashSet<int>();
                if (HasObservedExpectedProcess)
                    ExpandTrackedDescendants(runningProcesses, scan.CapturedUtc, deferredParents);
                // Discover children using the previous identities before retiring a parent
                // that exited after launching them, including after the startup deadline.
                RemoveExitedOrReusedProcesses(runningProcesses, scan.CapturedUtc, deferredParents);
                if (deferredParents.Count > 0)
                {
                    // Native enumeration completed, but a reused/new child identity could
                    // not be bound to that snapshot. Retry before confirming tree closure.
                    RecordSnapshotFailure(null);
                    return;
                }
                lock (_syncRoot)
                {
                    if (_disposed) return;
                    _completedSnapshotCount++;
                    _consecutiveSnapshotFailures = 0;
                    _lastSnapshotSucceeded = true;
                }
            }
            catch (Exception ex)
            {
                RecordSnapshotFailure(ex);
            }
            finally
            {
                Volatile.Write(ref _snapshotInProgress, 0);
            }
        }

        private void TryTrackExpectedProcess(int processId, DateTime capturedUtc)
        {
            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    if (!PathsMatch(process))
                        return;
                    DateTime startedUtc = process.StartTime.ToUniversalTime();
                    lock (_syncRoot)
                    {
                        if (_disposed || _trackOnlySelectedTree) return;
                        if (startedUtc < _watchStartedUtc || startedUtc > capturedUtc) return;
                        if (_existingExpectedProcessIds.TryGetValue(processId, out DateTime existingStartedUtc) && existingStartedUtc == startedUtc)
                            return;
                        _observedExpectedProcesses.Add(new ProcessIdentity(processId, startedUtc));
                        if (!_trackedProcessStartTimes.TryGetValue(processId, out DateTime trackedStart) || trackedStart != startedUtc)
                        {
                            _directLaunchTreeProcessIds.Remove(processId);
                            _trackedProcessStartTimes[processId] = startedUtc;
                            _hasObservedExpectedProcess = true;
                            logger.Debug($"ProcessTreeMonitor/TryTrackExpectedProcess: Tracking PID {processId} for {_expectedExecutablePath} as the expected executable.");
                        }
                    }
                }
            }
            catch (ArgumentException)
            {
                // The process exited before its full path could be verified.
            }
            catch (Exception ex)
            {
                logger.Trace(ex, $"ProcessTreeMonitor/TryTrackExpectedProcess: Could not inspect PID {processId} for {_expectedExecutablePath}.");
            }
        }

        private void ExpandTrackedDescendants(List<ProcessSnapshot> runningProcesses, DateTime capturedUtc, HashSet<int> deferredParents)
        {
            bool changed;
            do
            {
                changed = false;
                lock (_syncRoot)
                {
                    if (_disposed) return;
                    foreach (ProcessSnapshot process in runningProcesses)
                    {
                        if (!_trackedProcessStartTimes.TryGetValue(process.ParentProcessId, out DateTime parentStartedUtc))
                            continue;
                        try
                        {
                            using Process child = Process.GetProcessById(process.ProcessId);
                            if (child.HasExited) continue;
                            DateTime startedUtc = child.StartTime.ToUniversalTime();
                            // A fresh lookup of a reused child PID is not the process that
                            // appeared in this snapshot. Leave it for a later scan.
                            if (startedUtc < parentStartedUtc) continue;
                            if (startedUtc > capturedUtc)
                            {
                                deferredParents.Add(process.ParentProcessId);
                                continue;
                            }
                            try
                            {
                                using Process parent = Process.GetProcessById(process.ParentProcessId);
                                DateTime currentParentStartedUtc = parent.StartTime.ToUniversalTime();
                                // An older child may outlive a parent whose PID is now reused.
                                // A child born after the replacement parent belongs to that
                                // replacement, not to our original tracked identity.
                                if (currentParentStartedUtc != parentStartedUtc && startedUtc >= currentParentStartedUtc) continue;
                            }
                            catch (ArgumentException) { /* The original parent exited; its child remains valid. */ }
                            if (!_trackOnlySelectedTree)
                                _trackedParents[new ProcessIdentity(process.ProcessId, startedUtc)] = new ProcessIdentity(process.ParentProcessId, parentStartedUtc);
                            if (!_trackedProcessStartTimes.TryGetValue(process.ProcessId, out DateTime trackedStartedUtc) || trackedStartedUtc != startedUtc)
                            {
                                _trackedProcessStartTimes[process.ProcessId] = startedUtc;
                                changed = true;
                                logger.Trace($"ProcessTreeMonitor/ExpandTrackedDescendants: Tracking descendant PID {process.ProcessId} of PID {process.ParentProcessId} for {_expectedExecutablePath}.");
                            }
                            if (_directLaunchTreeProcessIds.Contains(process.ParentProcessId) && _directLaunchTreeProcessIds.Add(process.ProcessId))
                                changed = true;
                        }
                        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
                        {
                            if (ex is System.ComponentModel.Win32Exception) _hadTrackingError = true;
                            logger.Trace(ex, "ProcessTreeMonitor/ExpandTrackedDescendants: Could not verify descendant PID {0}.", process.ProcessId);
                        }
                    }
                }
            }
            while (changed);
        }

        private void RemoveExitedOrReusedProcesses(List<ProcessSnapshot> runningProcesses, DateTime capturedUtc, HashSet<int> deferredParents)
        {
            HashSet<int> runningIds = runningProcesses.Select(item => item.ProcessId).ToHashSet();
            lock (_syncRoot)
            {
                if (_disposed) return;
                foreach (KeyValuePair<int, DateTime> tracked in _trackedProcessStartTimes.ToArray())
                {
                    if (tracked.Value > capturedUtc || deferredParents.Contains(tracked.Key)) continue;
                    if (!runningIds.Contains(tracked.Key) && DateTime.UtcNow <= _deadlineUtc) continue;
                    bool sameProcess = false;
                    bool reused = false;
                    try
                    {
                        using Process process = Process.GetProcessById(tracked.Key);
                        reused = process.StartTime.ToUniversalTime() != tracked.Value;
                        sameProcess = !process.HasExited && !reused;
                    }
                    catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
                    {
                        if (runningIds.Contains(tracked.Key) && ex is System.ComponentModel.Win32Exception) _hadTrackingError = true;
                        logger.Trace(ex, "ProcessTreeMonitor/RemoveExitedOrReusedProcesses: Could not verify tracked PID {0}.", tracked.Key);
                    }
                    // If the parent was in this snapshot but exited while it was being
                    // processed, retain it until the next snapshot can include its child.
                    if (!sameProcess && runningIds.Contains(tracked.Key) && !reused) continue;
                    if (!sameProcess)
                    {
                        _trackedProcessStartTimes.Remove(tracked.Key);
                        _directLaunchTreeProcessIds.Remove(tracked.Key);
                    }
                }
            }
        }

        private ProcessSnapshotScan CaptureProcessSnapshot()
        {
            List<ProcessSnapshot> processes = new List<ProcessSnapshot>();
            // Bound lookups to identities that existed before snapshot creation. A child
            // created during collection is conservatively deferred to the next scan.
            DateTime capturedUtc = DateTime.UtcNow;
            IntPtr snapshot = CreateToolhelp32Snapshot(SnapshotProcesses, 0);
            if (snapshot == InvalidHandle)
            {
                logger.Warn($"ProcessTreeMonitor/CaptureProcessSnapshot: CreateToolhelp32Snapshot failed with Win32 error {Marshal.GetLastWin32Error()}.");
                return new ProcessSnapshotScan(processes, capturedUtc, false);
            }

            try
            {
                ProcessEntry32 entry = new ProcessEntry32
                {
                    Size = (uint)Marshal.SizeOf<ProcessEntry32>()
                };

                if (!Process32First(snapshot, ref entry))
                {
                    logger.Warn("ProcessTreeMonitor/CaptureProcessSnapshot: Process32First failed with Win32 error {0}.", Marshal.GetLastWin32Error());
                    return new ProcessSnapshotScan(processes, capturedUtc, false);
                }

                do
                {
                    processes.Add(new ProcessSnapshot((int)entry.ProcessId, (int)entry.ParentProcessId, entry.ExecutableFile ?? string.Empty));
                    entry.Size = (uint)Marshal.SizeOf<ProcessEntry32>();
                }
                while (Process32Next(snapshot, ref entry));
                int enumerationError = Marshal.GetLastWin32Error();
                if (enumerationError != 18) // ERROR_NO_MORE_FILES is the only successful end of enumeration.
                {
                    logger.Warn("ProcessTreeMonitor/CaptureProcessSnapshot: Process32Next failed with Win32 error {0}; the partial snapshot will be retried.", enumerationError);
                    return new ProcessSnapshotScan(processes, capturedUtc, false);
                }
            }
            finally
            {
                CloseHandle(snapshot);
            }

            return new ProcessSnapshotScan(processes, capturedUtc, true);
        }

        private bool PathsMatch(Process process)
        {
            string? processPath = GetProcessImagePath(process.Id);
            return !string.IsNullOrWhiteSpace(processPath)
                && string.Equals(Path.GetFullPath(processPath), _expectedExecutablePath, StringComparison.OrdinalIgnoreCase);
        }

        private static string? GetProcessImagePath(int processId)
        {
            IntPtr processHandle = OpenProcess(ProcessQueryLimitedInformation, false, processId);
            if (processHandle == IntPtr.Zero)
                return null;

            try
            {
                StringBuilder pathBuffer = new StringBuilder(MaximumPathLength);
                uint pathLength = (uint)pathBuffer.Capacity;
                if (!QueryFullProcessImageName(processHandle, 0, pathBuffer, ref pathLength))
                    return null;

                return pathBuffer.ToString();
            }
            finally
            {
                CloseHandle(processHandle);
            }
        }

        private void RecordSnapshotFailure(Exception? exception)
        {
            int failures;
            lock (_syncRoot)
            {
                if (_disposed || _discoveryComplete) return;
                _lastSnapshotSucceeded = false;
                failures = ++_consecutiveSnapshotFailures;
                _nextSnapshotAttemptUtc = DateTime.UtcNow.Add(SnapshotRetryInterval);
                _trackingFailed = failures >= MaximumSnapshotFailures;
            }
            logger.Warn(exception, "ProcessTreeMonitor/RecordSnapshotFailure: Snapshot attempt {0} of {1} failed for {2}.", failures, MaximumSnapshotFailures, _expectedExecutablePath);
            if (failures >= MaximumSnapshotFailures) CompleteDiscovery();
        }

        private void ThrowIfTrackingFailed()
        {
            lock (_syncRoot)
                if (_trackingFailed) throw new ProcessTrackingException($"Process tracking failed for '{_expectedExecutablePath}' after {MaximumSnapshotFailures} consecutive failed scans.");
        }

        private void CompleteDiscovery()
        {
            Timer? timer;
            bool failed;
            lock (_syncRoot)
            {
                if (_discoveryComplete)
                    return;
                _discoveryComplete = true;
                failed = _trackingFailed;
                timer = _snapshotTimer;
                _snapshotTimer = null;
            }

            timer?.Dispose();
            if (failed)
            {
                logger.Error("ProcessTreeMonitor/CompleteDiscovery: Stopped tracking {0} after {1} consecutive snapshot failures; process completion has not been confirmed.", _expectedExecutablePath, MaximumSnapshotFailures);
            }
            else if (!HasObservedExpectedProcess)
            {
                logger.Warn($"ProcessTreeMonitor/CompleteDiscovery: Native startup discovery ended without observing {_expectedExecutablePath}. The snapshot timer has stopped; the shortcut may need a different executable selected for monitoring.");
            }
            else
            {
                logger.Debug($"ProcessTreeMonitor/CompleteDiscovery: Native startup discovery completed for {_expectedExecutablePath}. Retaining the captured process tree for lifetime tracking.");
            }
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (_disposed) return;
                _disposed = true;
            }
            CompleteDiscovery();
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct ProcessEntry32
        {
            public uint Size;
            public uint Usage;
            public uint ProcessId;
            public IntPtr DefaultHeapId;
            public uint ModuleId;
            public uint Threads;
            public uint ParentProcessId;
            public int PriorityClassBase;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string? ExecutableFile;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint desiredAccess, bool inheritHandle, int processId);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool QueryFullProcessImageName(IntPtr processHandle, uint flags, StringBuilder executablePath, ref uint size);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool Process32First(IntPtr snapshot, ref ProcessEntry32 processEntry);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool Process32Next(IntPtr snapshot, ref ProcessEntry32 processEntry);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);
    }

    public sealed class ProcessTrackingException : InvalidOperationException
    {
        public ProcessTrackingException(string message) : base(message) { }
    }
}
