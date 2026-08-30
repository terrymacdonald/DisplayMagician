using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;

namespace DisplayMagician.Processes
{
    public sealed class ProcessTreeMonitor : IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly object _syncRoot = new object();
        private readonly string _expectedExecutablePath;
        private readonly string _expectedExecutableName;
        private readonly HashSet<int> _existingExpectedProcessIds = new HashSet<int>();
        private readonly HashSet<int> _trackedProcessIds = new HashSet<int>();
        private readonly DateTime _deadlineUtc;
        private ManagementEventWatcher _processStartWatcher;
        private Timer _fallbackTimer;
        private bool _hasObservedExpectedProcess;
        private bool _disposed;

        private ProcessTreeMonitor(string expectedExecutablePath, int startTimeout)
        {
            _expectedExecutablePath = Path.GetFullPath(expectedExecutablePath);
            _expectedExecutableName = Path.GetFileName(expectedExecutablePath);
            _deadlineUtc = DateTime.UtcNow.AddSeconds(Math.Clamp(startTimeout, 1, 30));
            foreach (Process process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_expectedExecutableName)))
            {
                try
                {
                    if (PathsMatch(process))
                        _existingExpectedProcessIds.Add(process.Id);
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

        public bool IsRunning
        {
            get
            {
                int[] processIds;
                lock (_syncRoot)
                    processIds = new List<int>(_trackedProcessIds).ToArray();

                foreach (int processId in processIds)
                {
                    try
                    {
                        using (Process process = Process.GetProcessById(processId))
                        {
                            if (!process.HasExited)
                                return true;
                        }
                    }
                    catch (ArgumentException)
                    {
                        // The process has exited.
                    }
                    catch (Exception ex)
                    {
                        logger.Trace(ex, $"ProcessTreeMonitor/IsRunning: Could not query tracked PID {processId}.");
                    }
                }
                return false;
            }
        }

        public static ProcessTreeMonitor BeginWatching(string expectedExecutablePath, int startTimeout)
        {
            if (string.IsNullOrWhiteSpace(expectedExecutablePath) || !File.Exists(expectedExecutablePath))
                return null;

            ProcessTreeMonitor monitor = new ProcessTreeMonitor(expectedExecutablePath, startTimeout);
            monitor.Start();
            return monitor;
        }

        private void Start()
        {
            try
            {
                _processStartWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
                _processStartWatcher.EventArrived += ProcessStartWatcherEventArrived;
                _processStartWatcher.Start();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ProcessTreeMonitor/Start: Could not start the process-start watcher for {_expectedExecutablePath}. Falling back to bounded process checks.");
                DisposeWatcher();
            }

            _fallbackTimer = new Timer(FallbackTimerElapsed, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(250));
            logger.Debug($"ProcessTreeMonitor/Start: Watching for a new {_expectedExecutablePath} process until {_deadlineUtc:O}.");
        }

        private void ProcessStartWatcherEventArrived(object sender, EventArrivedEventArgs args)
        {
            if (_disposed || DateTime.UtcNow > _deadlineUtc)
                return;

            try
            {
                int processId = Convert.ToInt32(args.NewEvent.Properties["ProcessID"].Value);
                int parentProcessId = Convert.ToInt32(args.NewEvent.Properties["ParentProcessID"].Value);
                string processName = args.NewEvent.Properties["ProcessName"].Value as string;
                if (string.Equals(processName, _expectedExecutableName, StringComparison.OrdinalIgnoreCase))
                    TryTrackExpectedProcess(processId);
                else
                    TryTrackDescendant(processId, parentProcessId);
            }
            catch (Exception ex)
            {
                logger.Trace(ex, $"ProcessTreeMonitor/ProcessStartWatcherEventArrived: Could not process a process-start event while watching {_expectedExecutablePath}.");
            }
        }

        private void FallbackTimerElapsed(object state)
        {
            if (_disposed)
                return;

            if (DateTime.UtcNow > _deadlineUtc)
            {
                Dispose();
                return;
            }

            if (!HasObservedExpectedProcess)
            {
                foreach (Process process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_expectedExecutableName)))
                {
                    try
                    {
                        if (PathsMatch(process))
                        {
                            TryTrackExpectedProcess(process.Id);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Trace(ex, $"ProcessTreeMonitor/FallbackTimerElapsed: Could not inspect a candidate process for {_expectedExecutablePath}.");
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
        }

        private void TryTrackExpectedProcess(int processId)
        {
            lock (_syncRoot)
            {
                if (_existingExpectedProcessIds.Contains(processId))
                    return;
            }
            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    if (PathsMatch(process))
                        TrackProcess(processId, true);
                }
            }
            catch (ArgumentException)
            {
                // The expected process was short-lived. The periodic exact-path check remains available.
            }
        }

        private void TryTrackDescendant(int processId, int parentProcessId)
        {
            lock (_syncRoot)
            {
                if (!_hasObservedExpectedProcess || !_trackedProcessIds.Contains(parentProcessId))
                    return;
            }
            TrackProcess(processId, false);
        }

        private void TrackProcess(int processId, bool expectedProcess)
        {
            lock (_syncRoot)
            {
                if (!_trackedProcessIds.Add(processId))
                    return;
                if (expectedProcess)
                    _hasObservedExpectedProcess = true;
            }
            logger.Debug($"ProcessTreeMonitor/TrackProcess: Tracking PID {processId} for {_expectedExecutablePath}{(expectedProcess ? " as the expected executable" : " as a descendant")}.");
            TrackExistingDescendants(processId);
        }

        private void TrackExistingDescendants(int parentProcessId)
        {
            foreach (Process childProcess in ProcessUtils.GetChildProcesses(parentProcessId))
            {
                try
                {
                    TrackProcess(childProcess.Id, false);
                }
                catch (Exception ex)
                {
                    logger.Trace(ex, $"ProcessTreeMonitor/TrackExistingDescendants: Could not track a descendant of PID {parentProcessId}.");
                }
                finally
                {
                    childProcess.Dispose();
                }
            }
        }

        private bool PathsMatch(Process process)
        {
            string processPath = process.MainModule?.FileName;
            return !string.IsNullOrWhiteSpace(processPath)
                && string.Equals(Path.GetFullPath(processPath), _expectedExecutablePath, StringComparison.OrdinalIgnoreCase);
        }

        private void DisposeWatcher()
        {
            if (_processStartWatcher == null)
                return;
            _processStartWatcher.EventArrived -= ProcessStartWatcherEventArrived;
            _processStartWatcher.Stop();
            _processStartWatcher.Dispose();
            _processStartWatcher = null;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _fallbackTimer?.Dispose();
            _fallbackTimer = null;
            try
            {
                DisposeWatcher();
            }
            catch (Exception ex)
            {
                logger.Trace(ex, $"ProcessTreeMonitor/Dispose: Could not dispose the process-start watcher for {_expectedExecutablePath}.");
            }
        }
    }
}
