using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace DisplayMagician.Processes
{
    public sealed class ProcessTreeMonitor : IDisposable
    {
        private const uint SnapshotProcesses = 0x00000002;
        private static readonly IntPtr InvalidHandle = new IntPtr(-1);
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly object _syncRoot = new object();
        private readonly string _expectedExecutablePath;
        private readonly string _expectedExecutableName;
        private readonly HashSet<int> _existingExpectedProcessIds = new HashSet<int>();
        private readonly HashSet<int> _trackedProcessIds = new HashSet<int>();
        private readonly DateTime _deadlineUtc;
        private Timer _snapshotTimer;
        private int _snapshotInProgress;
        private bool _hasObservedExpectedProcess;
        private bool _discoveryComplete;
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
                int[] processIds;
                lock (_syncRoot)
                    processIds = _trackedProcessIds.ToArray();

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

        public List<Process> GetTrackedProcesses()
        {
            List<Process> processes = new List<Process>();
            int[] processIds;
            lock (_syncRoot)
                processIds = _trackedProcessIds.ToArray();

            foreach (int processId in processIds)
            {
                try
                {
                    Process process = Process.GetProcessById(processId);
                    if (!process.HasExited)
                        processes.Add(process);
                    else
                        process.Dispose();
                }
                catch (ArgumentException)
                {
                    // The process has exited.
                }
                catch (Exception ex)
                {
                    logger.Trace(ex, $"ProcessTreeMonitor/GetTrackedProcesses: Could not query tracked PID {processId}.");
                }
            }

            return processes;
        }

        public static ProcessTreeMonitor BeginWatching(string expectedExecutablePath, int startTimeout)
        {
            if (string.IsNullOrWhiteSpace(expectedExecutablePath) || !File.Exists(expectedExecutablePath))
                return null;

            ProcessTreeMonitor monitor = new ProcessTreeMonitor(expectedExecutablePath, startTimeout);
            monitor.Start();
            return monitor;
        }

        public static List<Process> StartAndCapture(string executable, string arguments, ProcessPriority processPriority, int startTimeout = 1, bool runAsAdministrator = false, bool captureDescendantsForStartupWindow = false)
        {
            ProcessTreeMonitor monitor = BeginWatching(executable, startTimeout);
            List<Process> startedProcesses = ProcessUtils.StartProcess(executable, arguments, processPriority, startTimeout, runAsAdministrator);
            if (monitor == null)
                return startedProcesses;

            try
            {
                while (DateTime.UtcNow < monitor._deadlineUtc &&
                    (captureDescendantsForStartupWindow || !monitor.HasObservedExpectedProcess))
                {
                    Thread.Sleep(50);
                }

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
            _snapshotTimer = new Timer(CaptureProcessTree, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(250));
            logger.Debug($"ProcessTreeMonitor/Start: Capturing native process snapshots for {_expectedExecutablePath} until {_deadlineUtc:O}.");
        }

        private void CaptureProcessTree(object state)
        {
            if (_disposed || IsDiscoveryComplete || Interlocked.Exchange(ref _snapshotInProgress, 1) != 0)
                return;

            try
            {
                if (DateTime.UtcNow > _deadlineUtc)
                {
                    CompleteDiscovery();
                    return;
                }

                List<ProcessSnapshot> runningProcesses = CaptureProcessSnapshot();
                foreach (ProcessSnapshot process in runningProcesses)
                {
                    if (!string.Equals(process.ExecutableName, _expectedExecutableName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    TryTrackExpectedProcess(process.ProcessId);
                }

                if (HasObservedExpectedProcess)
                    ExpandTrackedDescendants(runningProcesses);
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ProcessTreeMonitor/CaptureProcessTree: Could not capture the process tree for {_expectedExecutablePath}.");
            }
            finally
            {
                Volatile.Write(ref _snapshotInProgress, 0);
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
                    if (!PathsMatch(process))
                        return;
                }

                lock (_syncRoot)
                {
                    if (_trackedProcessIds.Add(processId))
                    {
                        _hasObservedExpectedProcess = true;
                        logger.Debug($"ProcessTreeMonitor/TryTrackExpectedProcess: Tracking PID {processId} for {_expectedExecutablePath} as the expected executable.");
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

        private void ExpandTrackedDescendants(List<ProcessSnapshot> runningProcesses)
        {
            bool changed;
            do
            {
                changed = false;
                lock (_syncRoot)
                {
                    foreach (ProcessSnapshot process in runningProcesses)
                    {
                        if (_trackedProcessIds.Contains(process.ParentProcessId) && _trackedProcessIds.Add(process.ProcessId))
                        {
                            changed = true;
                            logger.Trace($"ProcessTreeMonitor/ExpandTrackedDescendants: Tracking descendant PID {process.ProcessId} of PID {process.ParentProcessId} for {_expectedExecutablePath}.");
                        }
                    }
                }
            }
            while (changed);
        }

        private static List<ProcessSnapshot> CaptureProcessSnapshot()
        {
            List<ProcessSnapshot> processes = new List<ProcessSnapshot>();
            IntPtr snapshot = CreateToolhelp32Snapshot(SnapshotProcesses, 0);
            if (snapshot == InvalidHandle)
            {
                logger.Warn($"ProcessTreeMonitor/CaptureProcessSnapshot: CreateToolhelp32Snapshot failed with Win32 error {Marshal.GetLastWin32Error()}.");
                return processes;
            }

            try
            {
                ProcessEntry32 entry = new ProcessEntry32
                {
                    Size = (uint)Marshal.SizeOf<ProcessEntry32>()
                };

                if (!Process32First(snapshot, ref entry))
                    return processes;

                do
                {
                    processes.Add(new ProcessSnapshot((int)entry.ProcessId, (int)entry.ParentProcessId, entry.ExecutableFile ?? string.Empty));
                    entry.Size = (uint)Marshal.SizeOf<ProcessEntry32>();
                }
                while (Process32Next(snapshot, ref entry));
            }
            finally
            {
                CloseHandle(snapshot);
            }

            return processes;
        }

        private bool PathsMatch(Process process)
        {
            string processPath = process.MainModule?.FileName;
            return !string.IsNullOrWhiteSpace(processPath)
                && string.Equals(Path.GetFullPath(processPath), _expectedExecutablePath, StringComparison.OrdinalIgnoreCase);
        }

        private void CompleteDiscovery()
        {
            lock (_syncRoot)
            {
                if (_discoveryComplete || _disposed)
                    return;
                _discoveryComplete = true;
            }

            _snapshotTimer?.Dispose();
            _snapshotTimer = null;
            logger.Debug($"ProcessTreeMonitor/CompleteDiscovery: Native startup discovery completed for {_expectedExecutablePath}. Retaining the captured process tree for lifetime tracking.");
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            CompleteDiscovery();
            _disposed = true;
        }

        private sealed record ProcessSnapshot(int ProcessId, int ParentProcessId, string ExecutableName);

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
            public string ExecutableFile;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processId);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool Process32First(IntPtr snapshot, ref ProcessEntry32 processEntry);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool Process32Next(IntPtr snapshot, ref ProcessEntry32 processEntry);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);
    }
}
