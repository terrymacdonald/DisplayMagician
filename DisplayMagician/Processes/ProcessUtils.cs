using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

namespace DisplayMagician.Processes
{

    [Flags]
    public enum PROCESS_CREATION_FLAGS : UInt32
    {
        ZERO_FLAG = 0x00000000,
        DEBUG_PROCESS = 0x00000001,
        DEBUG_ONLY_THIS_PROCESS = 0x00000002,
        CREATE_SUSPENDED = 0x00000004,
        DETACHED_PROCESS = 0x00000008,
        CREATE_NEW_CONSOLE = 0x00000010,
        CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
        CREATE_DEFAULT_ERROR_MODE = 0x04000000,

        CREATE_NEW_PROCESS_GROUP = 0x00000200,
        CREATE_NO_WINDOW = 0x08000000,
        CREATE_PROTECTED_PROCESS = 0x00040000,
        CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
        CREATE_SEPARATE_WOW_VDM = 0x00000800,
        CREATE_SHARED_WOW_VDM = 0x00001000,
        CREATE_UNICODE_ENVIRONMENT = 0x00000400,
        EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
        INHERIT_PARENT_AFFINITY = 0x00010000,

        // Process creations flags
        ABOVE_NORMAL_PRIORITY_CLASS = 0x00008000,
        BELOW_NORMAL_PRIORITY_CLASS = 0x00004000,
        HIGH_PRIORITY_CLASS = 0x00000080,
        IDLE_PRIORITY_CLASS = 0x00000040,
        NORMAL_PRIORITY_CLASS = 0x00000020,
        REALTIME_PRIORITY_CLASS = 0x00000100,
    }


    public class ProcessUtils
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        //private static readonly Encoding CONSOLE_ENCODING = Encoding.UTF8;
        //private static readonly string CONSOLE_ENCODING_PREAMBLE = CONSOLE_ENCODING.GetString(CONSOLE_ENCODING.GetPreamble());

        //private const int INFINITE = -1;

        public static List<Process> StartProcess(string executable, string arguments, ProcessPriority processPriority, int startTimeout = 1, bool runAsAdministrator = false)
        {
            List<Process> returnedProcesses = new List<Process>();

            // TODO: startTimeout (seconds) is intended to gate how long DM waits for the process to be
            // detected as running, but that detection loop is not yet implemented here. The 4-second
            // launcher-detection WaitForExit below is a separate, unrelated mechanism.
            ProcessPriorityClass wantedPriority = TranslatePriorityToClass(processPriority);
            Process processCreated = TryExecute(executable, arguments, runAsAdministrator, wantedPriority);

            if (processCreated != null)
            {
                logger.Info($"ProcessUtils/StartProcess: {executable} {arguments} has successfully been started by Process.Start (Process ID: {processCreated.Id})");
                try
                {
                    processCreated.WaitForExit(4000);

                    if (processCreated.HasExited)
                    {
                        logger.Info($"ProcessUtils/StartProcess: {executable} {arguments} has exited within 4 seconds. It is probable that it is a game or app launcher, so we'll try to see if process ID {processCreated.Id} launched any child processes, and monitor them instead!");

                        // If the process has exited, then it's likely to be a launcher, so we try to find the children processes
                        List<Process> childProcesses = GetChildProcesses(processCreated);
                        if (childProcesses.Count > 0)
                        {
                            logger.Trace($"ProcessUtils/StartProcess: Yay! We found {childProcesses.Count} child processes were launched when we started {executable} {arguments}, so we'll monitor them instead!");
                            returnedProcesses.AddRange(childProcesses);
                        }
                        else
                        {
                            logger.Trace($"ProcessUtils/StartProcess: Oh no! We couldn't find any child processes after we started {executable} {arguments} and it closed itself. Nothing to monitor! It's possible that there is a problem with the {executable} program. Try running it yourself manually to see if you can see a problem with it.");
                            // We need to try and find if there were any child processes another way
                            // For example, this is where we land when Explorer launches a UWP program using ShellAppsFolder 
                            // Explorer runs the UWP program, and then closes the application process, as it seems to communicate with 
                            // svchost.exe thorough the backend.
                        }
                        // The launcher process has already exited; release its handle.
                        processCreated.Dispose();
                    }
                    else
                    {
                        // If we're here then the process was created and hasn't exited!
                        try
                        {
                            if (processCreated.PriorityClass != wantedPriority)
                            {
                                processCreated.PriorityClass = wantedPriority;
                                logger.Trace($"ProcessUtils/StartProcess: Successfully set the Priority Class to {wantedPriority.ToString("G")} for {executable}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex, $"ProcessUtils/StartProcess: Exception while trying to set the Priority Class to {wantedPriority.ToString("G")} for {executable}.");
                        }
                        returnedProcesses.Add(processCreated);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ProcessUtils/StartProcess: Exception while trying to get the process information from {executable} after we started it. It is possible there was a problem with the executable.");
                }
            }
            else
            {
                logger.Warn($"ProcessUtils/StartProcess: DisplayMagician was unable to start {executable} {arguments}.");
            }

            return returnedProcesses;
        }        

        public static List<Process> GetChildProcesses(Process process)
        {
            if (process == null)
            {
                logger.Warn("ProcessUtils/GetChildProcesses: Null process supplied — returning empty list.");
                return new List<Process>();
            }
            return GetChildProcesses(process.Id);
        }

        public static List<Process> GetChildProcesses(int processId)
        {
            List<Process> children = new List<Process>();
            using (ManagementObjectSearcher mos = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={processId}"))
            {
                foreach (ManagementObject mo in mos.Get())
                {
                    using (mo)
                    {
                        try
                        {
                            children.Add(Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])));
                        }
                        catch (ArgumentException)
                        {
                            logger.Trace($"ProcessUtils/GetChildProcesses: Child process {mo[\"ProcessID\"]} exited before we could retrieve it — skipping.");
                        }
                    }
                }
            }
            return children;
        }

        public static bool ProcessExited(Process process)
        {
            if (process == null)
            {
                logger.Trace("ProcessUtils/ProcessExited: Null process supplied — treating as exited.");
                return true;
            }
            int pid;
            try
            {
                pid = process.Id;
            }
            catch (InvalidOperationException)
            {
                logger.Trace("ProcessUtils/ProcessExited: Unstarted/unassociated process supplied — treating as exited.");
                return true;
            }
            if (pid <= 0)
            {
                logger.Trace("ProcessUtils/ProcessExited: Invalid process ID — treating as exited.");
                return true;
            }
            try
            {
                if (process.HasExited)
                {
                    logger.Trace($"ProcessUtils/ProcessExited: {pid} has exited and is not running. This means the process has finished!");
                    return true;
                }
                //logger.Trace($"ProcessUtils/ProcessExited: {pid} is still running as it has not exited yet.");
                return false;
            }
            catch (ArgumentException ex)
            {
                logger.Trace(ex, $"ProcessUtils/ProcessExited: {pid} is not running, and the process ID has expired. This means the process has finished!");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn(ex, $"ProcessUtils/ProcessExited: {pid} was not started by this process object. This likely means the process has finished!");
                return true;
            }
            catch (Exception ex)
            {
                logger.Trace(ex, $"ProcessUtils/ProcessExited: Exception when checking if {pid} is still running, so assuming the process has finished!");
                return true;
            }
        }

        public static bool ProcessExited(string executable)
        {
            return ProcessExited(Process.GetProcessesByName(GetProcessName(executable)).ToList());
        }

        public static bool ProcessExited(int processId)
        {
            Process process;
            try
            {
                process = Process.GetProcessById(processId);
            }
            catch (ArgumentException)
            {
                logger.Trace($"ProcessUtils/ProcessExited: Process with ID {processId} no longer exists — treating as exited.");
                return true;
            }

            return ProcessExited(process);
        }

        public static bool ProcessExited(List<Process> processes)
        {
            if (processes == null)
            {
                logger.Warn("ProcessUtils/ProcessExited: Null process list supplied — treating all as exited.");
                return true;
            }
            return processes.All(p => ProcessExited(p));
        }

        public static bool StopProcess(Process processToStop)
        {
            if (processToStop == null)
            {
                logger.Warn("ProcessUtils/StopProcess: Null process supplied, nothing to stop.");
                return false;
            }
            string procId = $"{processToStop.ProcessName} (PID {processToStop.Id})";
            try
            {
                // Stop the process
                processToStop.CloseMainWindow();
                processToStop.WaitForExit(1000);
                if (!ProcessExited(processToStop))
                {
                    logger.Trace($"ProcessUtils/StopProcess: Process {procId} wouldn't stop cleanly. Forcing program close.");
                    processToStop.Kill();
                    processToStop.WaitForExit(5000);
                    if (!ProcessExited(processToStop))
                    {
                        logger.Error($"ProcessUtils/StopProcess: Process {procId} couldn't be killed! It seems like something is actively preventing us from stopping the process");
                        return false;
                    }
                    logger.Trace($"ProcessUtils/StopProcess: Process {procId} was successfully killed.");
                }
                else
                {
                    logger.Trace($"ProcessUtils/StopProcess: Process {procId} was successfully stopped.");
                }
                return true;
            }
            catch (Win32Exception ex)
            {
                logger.Warn(ex, $"ProcessUtils/StopProcess: Win32Exception! Couldn't access the wait status for {procId} we're trying to stop. So now just killing the process.");
                try
                {
                    processToStop.Kill();
                    processToStop.WaitForExit(5000);
                    if (!ProcessExited(processToStop))
                    {
                        logger.Error($"ProcessUtils/StopProcess: Win32Exception! Process {procId} couldn't be killed! It seems like something is actively preventing us from stopping the process");
                        return false;
                    }
                    logger.Trace($"ProcessUtils/StopProcess: Win32Exception! Process {procId} was successfully killed.");
                    return true;
                }
                catch (InvalidOperationException)
                {
                    // Process exited on its own between the Win32Exception and the Kill() call — that's fine.
                    logger.Trace($"ProcessUtils/StopProcess: Win32Exception! Process {procId} exited on its own before Kill() could be called.");
                    return true;
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.Error(ex, $"ProcessUtils/StopProcess: Couldn't kill {procId} as the process appears to have closed already.");
                return true;
            }
            catch (SystemException ex)
            {
                logger.Error(ex, $"ProcessUtils/StopProcess: Couldn't WaitForExit the named process as there is no process associated with the Process object (or cannot get the ID from the named process handle).");
            }

            finally
            {
                processToStop.Close();
            }
            return false;
        }

        public static bool StopProcess(List<Process> processes)
        {
            if (processes == null)
            {
                logger.Warn("ProcessUtils/StopProcess: Null process list supplied, nothing to stop.");
                return false;
            }
            bool allStopped = true;
            // Stop the programs in the reverse order we started them
            foreach (Process processToStop in Enumerable.Reverse(processes))
            {
                // Stop the process if it hasn't stopped already
                try
                {
                    if (!ProcessExited(processToStop))
                    {
                        string procId = $"{processToStop.ProcessName} (PID {processToStop.Id})";
                        logger.Debug($"ProcessUtils/StopProcess: Stopping process {procId}");
                        if (StopProcess(processToStop))
                        {
                            logger.Debug($"ProcessUtils/StopProcess: Successfully stopped process {procId}");
                        }
                        else
                        {
                            logger.Warn($"ProcessUtils/StopProcess: Failed to stop process {procId} after main executable or game was exited by the user.");
                            allStopped = false;
                        }
                    }
                    else
                    {
                        logger.Debug($"ProcessUtils/StopProcess: Process {processToStop.ProcessName} (PID {processToStop.Id}) already stopped.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ProcessUtils/StopProcess: Exception while checking if processToStop has already exited");
                    allStopped = false;
                }

            }
            return allStopped;
        }

        /// <summary>
        /// Launches <paramref name="executable"/> and returns the created <see cref="Process"/>.
        /// Retries with elevated rights if the initial attempt fails with an access-denied error.
        /// </summary>
        /// <param name="executable">Program to execute.</param>
        /// <param name="arguments">Program arguments.</param>
        /// <param name="runAsAdministrator">Launch with elevated rights.</param>
        /// <param name="priorityClass">Process priority class to request.</param>
        /// <returns>The started <see cref="Process"/>, or <c>null</c> on failure.</returns>
        private static Process TryExecute(string executable, string arguments, bool runAsAdministrator = false, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal)
        {
            ProcessStartInfo psi;
            if (File.Exists(executable) && IsExecutableFileType(executable))
            {
                // Is exe file 
                if (runAsAdministrator)
                {
                    psi = new ProcessStartInfo(executable, arguments)
                    {
                        UseShellExecute = true,
                        Verb = "Runas",
                        WorkingDirectory = Path.GetDirectoryName(executable)
                    };
                }
                else
                {
                    psi = new ProcessStartInfo(executable, arguments)
                    {
                        UseShellExecute = false,
                        WorkingDirectory = Path.GetDirectoryName(executable)
                    };
                }
            }
            else
            {
                // Isn't a file (something like a URL) — do not derive WorkingDirectory from a
                // non-filesystem path; an invalid directory can cause shell-execute to fail.
                psi = new ProcessStartInfo(executable, arguments)
                {
                    UseShellExecute = true,
                    Verb = "Open",
                    WorkingDirectory = ""
                };
            }

            Process processCreated = new Process {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            try
            {
                if (processCreated.Start())
                {
                    logger.Trace($"ProcessUtils/TryExecute: {executable} was started successfully.");
                }
                else
                {
                    logger.Trace($"ProcessUtils/TryExecute: {executable} was reused successfully.");
                }
                return processCreated;
            }
            catch (ObjectDisposedException ex)
            {
                processCreated.Dispose();
                logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. The process object has already been disposed.");
                return null;
            }
            catch (InvalidOperationException ex)
            {
                if (processCreated.StartInfo.UseShellExecute && (processCreated.StartInfo.RedirectStandardInput || processCreated.StartInfo.RedirectStandardOutput || processCreated.StartInfo.RedirectStandardError))
                {
                    logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. The UseShellExecute member of the StartInfo property is true while RedirectStandardInput, RedirectStandardOutput, or RedirectStandardError is true.");
                }
                else
                {
                    logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. No file name was specified in the Process component's StartInfo.");
                }
                processCreated.Dispose();
                return null;
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == 740 || ex.NativeErrorCode == 5)
                {
                    if (runAsAdministrator)
                    {
                        processCreated.Dispose();
                        logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable} for a second time with administrative rights. Giving up.");
                        return null;
                    }

                    logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. The process requires elevation. Attempting again with admin rights.");
                    processCreated.Dispose();
                    processCreated = TryExecute(executable, arguments, true, priorityClass);
                    if (processCreated != null)
                    {
                        logger.Trace($"ProcessUtils/TryExecute: Running the {executable} a second time with administrative rights worked!");
                        return processCreated;
                    }
                    else
                    {
                        logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable} for a second time with administrative rights. Giving up.");
                        return null;
                    }

                }
                else
                {
                    logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. There was an error in opening the associated file.");
                }
                processCreated.Dispose();
                return null;
            }
            catch (PlatformNotSupportedException ex)
            {
                processCreated.Dispose();
                logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. Method not supported on operating systems without shell support such as Nano Server (.NET Core only).");
                return null;
            }
            catch (Exception ex)
            {
                processCreated.Dispose();
                logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. Not sure what specific exception it is.");
                return null;
            }
        }
       
        /// <summary>
        /// Returns true for PE-format binaries that can contain embedded icon resources
        /// (.exe, .com, .msi). Use this to decide whether TsudaKageyu.IconExtractor is appropriate.
        /// </summary>
        public static bool IsPEExecutable(string executable)
        {
            if (string.IsNullOrEmpty(executable)) return false;
            string ext = Path.GetExtension(executable);
            return ext.Equals(".exe", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".com", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".msi", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns true for file types that Windows Shell can launch but which do not contain
        /// embedded PE icon resources (.bat, .cmd, .ps1, .lnk, .url).
        /// Their icon comes from the Shell association or the shortcut target.
        /// Use WindowsThumbnailProvider to retrieve it.
        /// </summary>
        public static bool IsShellLaunchable(string executable)
        {
            if (string.IsNullOrEmpty(executable)) return false;
            string ext = Path.GetExtension(executable);
            return ext.Equals(".bat", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".cmd", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".ps1", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".lnk", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".url", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns true for any file type that DisplayMagician can launch as a shortcut target.
        /// This is the single source of truth for validating user-supplied executable paths.
        /// </summary>
        public static bool IsExecutableFileType(string executable)
        {
            return IsPEExecutable(executable) || IsShellLaunchable(executable);
        }

        public static string GetProcessName(string executable)
        {
            if (string.IsNullOrEmpty(executable))
                return "";
            return Path.GetFileNameWithoutExtension(executable);
        }

        public static ProcessPriority TranslateNameToPriority(string processPriorityName)
        {
            if (string.IsNullOrEmpty(processPriorityName)) return ProcessPriority.Normal;
            ProcessPriority wantedPriority = ProcessPriority.Normal;
            switch (processPriorityName.ToLowerInvariant())
            {
                case "high":        wantedPriority = ProcessPriority.High;        break;
                case "abovenormal": wantedPriority = ProcessPriority.AboveNormal; break;
                case "normal":      wantedPriority = ProcessPriority.Normal;      break;
                case "belownormal": wantedPriority = ProcessPriority.BelowNormal; break;
                case "idle":        wantedPriority = ProcessPriority.Idle;        break;
            }
            return wantedPriority;
        }

        public static ProcessPriorityClass TranslatePriorityToClass(ProcessPriority processPriorityClass)
        {
            ProcessPriorityClass wantedPriorityClass = ProcessPriorityClass.Normal;
            switch (processPriorityClass)
            {
                case ProcessPriority.High:        wantedPriorityClass = ProcessPriorityClass.High;        break;
                case ProcessPriority.AboveNormal: wantedPriorityClass = ProcessPriorityClass.AboveNormal; break;
                case ProcessPriority.Normal:      wantedPriorityClass = ProcessPriorityClass.Normal;      break;
                case ProcessPriority.BelowNormal: wantedPriorityClass = ProcessPriorityClass.BelowNormal; break;
                case ProcessPriority.Idle:        wantedPriorityClass = ProcessPriorityClass.Idle;        break;
            }
            return wantedPriorityClass;
        }

        public static PROCESS_CREATION_FLAGS TranslatePriorityClassToFlags(ProcessPriorityClass processPriorityClass)
        {
            PROCESS_CREATION_FLAGS wantedPriorityClass = PROCESS_CREATION_FLAGS.NORMAL_PRIORITY_CLASS;
            switch (processPriorityClass)
            {
                case ProcessPriorityClass.High:        wantedPriorityClass = PROCESS_CREATION_FLAGS.HIGH_PRIORITY_CLASS;         break;
                case ProcessPriorityClass.AboveNormal: wantedPriorityClass = PROCESS_CREATION_FLAGS.ABOVE_NORMAL_PRIORITY_CLASS; break;
                case ProcessPriorityClass.Normal:      wantedPriorityClass = PROCESS_CREATION_FLAGS.NORMAL_PRIORITY_CLASS;       break;
                case ProcessPriorityClass.BelowNormal: wantedPriorityClass = PROCESS_CREATION_FLAGS.BELOW_NORMAL_PRIORITY_CLASS; break;
                case ProcessPriorityClass.Idle:        wantedPriorityClass = PROCESS_CREATION_FLAGS.IDLE_PRIORITY_CLASS;         break;
            }
            return wantedPriorityClass;
        }
        
    }

}       

