using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        CREATE_SEPARATE_WOW_VDM = 0x00001000,
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

        private static readonly Encoding CONSOLE_ENCODING = Encoding.UTF8;
        private static readonly string CONSOLE_ENCODING_PREAMBLE = CONSOLE_ENCODING.GetString(CONSOLE_ENCODING.GetPreamble());

        private const int INFINITE = -1;

        public static List<Process> StartProcess(string executable, string arguments, ProcessPriority processPriority, int startTimeout = 1)
        {
            List<Process> runningProcesses = new List<Process>();
            Process process = null;
            bool usingChildProcess = false;

            if (TryExecute_AutoImpersonate(executable, arguments, out process))
            {
                logger.Trace($"ProcessUtils/StartProcess: {executable} {arguments} has successfully been started (ID: {process.Id})");
                runningProcesses.Add(process);
            }
            return runningProcesses;
        }

        public static List<Process> GetChildProcesses(Process process)
        {
            List<Process> children = new List<Process>();
            ManagementObjectSearcher mos = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={process.Id}");
            foreach (ManagementObject mo in mos.Get())
            {
                children.Add(Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])));
            }
            return children;
        }

        public static List<Process> GetChildProcesses(int processId)
        {
            List<Process> children = new List<Process>();
            ManagementObjectSearcher mos = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={processId}");
            foreach (ManagementObject mo in mos.Get())
            {
                children.Add(Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])));
            }
            return children;
        }

        public static bool ProcessExited(Process process)
        {
            try
            {
                if (process == null || process.Id <= 0) 
                {
                    logger.Trace($"ProcessUtils/ProcessExited: {process.Id} is not currently running.");
                    return true;
                }
                else if (process.HasExited)
                {
                    logger.Trace($"ProcessUtils/ProcessExited: {process.Id} has exited and is not running. This means the process has finished!");
                    return true;
                }
                else
                {
                    logger.Trace($"ProcessUtils/ProcessExited: {process.Id} is still running as is has not exited yet.");
                    return false;
                }
            }
            catch (ArgumentException ex)
            {
                logger.Trace($"ProcessUtils/ProcessExited: {process.Id} is not running, and the process ID has expired. This means the process has finished!");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn($"ProcessUtils/ProcessExited: {process.Id} was not started by this process object. This likely means the process has finished!");
                return true;
            }
            catch (Exception ex)
            {
                logger.Trace($"ProcessUtils/ProcessExited: Exception when checking if {process.Id} is still running, so assuming the process has finished!");
                return true;
            }
        }

        public static bool ProcessExited(List<Process> processes)
        {
            int processClosedCount = 0;
            foreach (Process p in processes)
            {
                if (ProcessExited(p))
                {
                    processClosedCount++;
                }
            }
            if (processClosedCount == processes.Count)
            {
                logger.Trace($"ProcessUtils/ProcessExited2: All processes being monitored have exited, so no processes still running!");
                return true;
            }
            else
            {
                logger.Trace($"ProcessUtils/ProcessExited2: {processClosedCount} processes out of {processes.Count} processes have exited. At least one process is still running!");
                return false;
            }
        }

        public static bool StopProcess(Process processToStop)
        {
            try
            {
                // Stop the process
                processToStop.CloseMainWindow();
                if (!processToStop.WaitForExit(1000))
                {
                    logger.Trace($"ProcessUtils/StopProcess: Process {processToStop.StartInfo.FileName} wouldn't stop cleanly. Forcing program close.");
                    processToStop.Kill();
                    if (!processToStop.WaitForExit(5000))
                    {
                        logger.Error($"ProcessUtils/StopProcess: Process {processToStop.StartInfo.FileName} couldn't be killed! It seems like something is actively preventing us from stopping the process");
                        return false;
                    }
                    if (!ProcessExited(processToStop))
                    {
                        logger.Error($"ProcessUtils/StopProcess: Process {processToStop.StartInfo.FileName} couldn't be killed! It seems like something is actively preventing us from stopping the process");
                        return false;
                    }
                    logger.Trace($"ProcessUtils/StopProcess: Process {processToStop.StartInfo.FileName} was successfully killed.");
                }
                else
                {
                    logger.Trace($"ProcessUtils/StopProcess: Process {processToStop.StartInfo.FileName} was successfully stopped.");
                }
                return true;
            }
            catch (Win32Exception ex)
            {
                logger.Warn(ex, $"ProcessUtils/StopProcess: Win32Exception! Couldn't access the wait status for a named process we're trying to stop. So now just killing the process.");
                processToStop.Kill();
                if (!processToStop.WaitForExit(5000))
                {
                    logger.Error($"ProcessUtils/StopProcess: Win32Exception! Process {processToStop.StartInfo.FileName} couldn't be killed! It seems like something is actively preventing us from stopping the process");
                    return false;
                }
                if (!ProcessExited(processToStop))
                {
                    logger.Error($"ProcessUtils/StopProcess: Win32Exception! Process {processToStop.StartInfo.FileName} couldn't be killed! It seems like something is actively preventing us from stopping the process");
                    return false;
                }
                logger.Trace($"ProcessUtils/StopProcess: Win32Exception! Process {processToStop.StartInfo.FileName} was successfully killed.");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                logger.Error(ex, $"ProcessUtils/StopProcess: Couldn't kill the named process as the process appears to have closed already.");
                return true;
            }
            catch (SystemException ex)
            {
                logger.Error(ex, $"ProcessUtils/StopProcess: Couldn't WaitForExit the named process as there is no process associated with the Process object (or cannot get the ID from the named process handle).");
            }

            catch (AggregateException ae)
            {
                logger.Error(ae, $"ProcessUtils/StopProcess: Got an AggregateException.");
            }
            return false;
        }

        public static bool StopProcess(List<Process> processes)
        {
            // Stop the programs in the reverse order we started them
            foreach (Process processToStop in processes)
            {
                // Stop the process if it hasn't stopped already
                try
                {
                    if (!ProcessExited(processToStop))
                    {
                        logger.Debug($"ShortcutRepository/RunShortcut: Stopping process {processToStop.StartInfo.FileName}");
                        if (ProcessUtils.StopProcess(processToStop))
                        {
                            logger.Debug($"ShortcutRepository/RunShortcut: Successfully stopped process {processToStop.StartInfo.FileName}");
                        }
                        else
                        {
                            logger.Warn($"ShortcutRepository/RunShortcut: Failed to stop process {processToStop.StartInfo.FileName} after main executable or game was exited by the user.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ShortcutRepository/RunShortcut: Exception while checking if processToStop has already exited");
                }

            }
            return true;
        }

        /// <summary>
        /// Executes the <paramref name="executable"/> and waits a maximum time of <paramref name="maxWaitMs"/> for completion. If the process doesn't end in 
        /// this time, it gets aborted.
        /// </summary>
        /// <param name="executable">Program to execute</param>
        /// <param name="arguments">Program arguments</param>
        /// <param name="priorityClass">Process priority</param>
        /// <param name="maxWaitMs">Maximum time to wait for completion</param>
        /// <returns><c>true</c> if process was executed and finished correctly</returns>
        public static bool TryExecute(string executable, string arguments, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal, int maxWaitMs = 1000)
        {
            Process unused;
            return TryExecute(executable, arguments, out unused, priorityClass, maxWaitMs);
        }

        /// <summary>
        /// Executes the <paramref name="executable"/> and waits a maximum time of <paramref name="maxWaitMs"/> for completion. If the process doesn't end in 
        /// this time, it gets aborted. This helper method automatically decides if an impersonation should be done, depending on the current identity's 
        /// <see cref="TokenImpersonationLevel"/>.
        /// </summary>
        /// <param name="executable">Program to execute</param>
        /// <param name="arguments">Program arguments</param>
        /// <param name="priorityClass">Process priority</param>
        /// <param name="maxWaitMs">Maximum time to wait for completion</param>
        /// <returns><c>true</c> if process was executed and finished correctly</returns>
        public static bool TryExecute_AutoImpersonate(string executable, string arguments, out Process process, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal, int maxWaitMs = 1000)
        {
            return IsImpersonated ?
              TryExecute_Impersonated(executable, arguments, out process, priorityClass, maxWaitMs) :
              TryExecute(executable, arguments, out process, priorityClass, maxWaitMs);
        }

        /// <summary>
        /// Executes the <paramref name="executable"/> and waits a maximum time of <paramref name="maxWaitMs"/> for completion. If the process doesn't end in 
        /// this time, it gets aborted. This method tries to impersonate the interactive user and run the process under its identity.
        /// </summary>
        /// <param name="executable">Program to execute</param>
        /// <param name="arguments">Program arguments</param>
        /// <param name="priorityClass">Process priority</param>
        /// <param name="maxWaitMs">Maximum time to wait for completion</param>
        /// <returns><c>true</c> if process was executed and finished correctly</returns>
        public static bool TryExecute_Impersonated(string executable, string arguments, out Process process, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal, int maxWaitMs = 1000)
        {
            IntPtr userToken;
            process = null;
            if (!ImpersonationHelper.GetTokenByProcess(out userToken, true))
                return false;
            try
            {
                //return TryExecute_Impersonated(executable, arguments, userToken, false, out unused, priorityClass, maxWaitMs);
                return TryExecute_Impersonated(executable, arguments, userToken, out process, priorityClass, maxWaitMs);
            }
            finally
            {
                ImpersonationHelper.SafeCloseHandle(userToken);
            }
        }

        /// <summary>
        /// Executes the <paramref name="executable"/> and waits a maximum time of <paramref name="maxWaitMs"/> for completion and returns the contents of
        /// <see cref="Process.StandardOutput"/>. If the process doesn't end in this time, it gets aborted.
        /// </summary>
        /// <param name="executable">Program to execute</param>
        /// <param name="arguments">Program arguments</param>
        /// <param name="result">Returns the contents of standard output</param>
        /// <param name="priorityClass">Process priority</param>
        /// <param name="maxWaitMs">Maximum time to wait for completion</param>
        /// <returns></returns>
        public static bool TryExecuteReadString(string executable, string arguments, out Process process, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal, int maxWaitMs = 1000)
        {
            return TryExecute(executable, arguments, out process, priorityClass, maxWaitMs);
        }

        /// <summary>
        /// Executes the <paramref name="executable"/> and waits a maximum time of <paramref name="maxWaitMs"/> for completion and returns the contents of
        /// <see cref="Process.StandardOutput"/>. If the process doesn't end in this time, it gets aborted.
        /// </summary>
        /// <param name="executable">Program to execute</param>
        /// <param name="arguments">Program arguments</param>
        /// <param name="result">Returns the contents of standard output</param>
        /// <param name="priorityClass">Process priority</param>
        /// <param name="maxWaitMs">Maximum time to wait for completion</param>
        /// <returns></returns>
        public static bool TryExecuteReadString_AutoImpersonate(string executable, string arguments, out Process process, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal, int maxWaitMs = 1000)
        {
            return IsImpersonated ?
              TryExecuteReadString_Impersonated(executable, arguments, out process, priorityClass, maxWaitMs) :
              TryExecuteReadString(executable, arguments, out process, priorityClass, maxWaitMs);
        }

        /// <summary>
        /// Executes the <paramref name="executable"/> and waits a maximum time of <paramref name="maxWaitMs"/> for completion and returns the contents of
        /// <see cref="Process.StandardOutput"/>. If the process doesn't end in this time, it gets aborted. 
        /// This method tries to impersonate the interactive user and run the process under its identity.
        /// </summary>
        /// <param name="executable">Program to execute</param>
        /// <param name="arguments">Program arguments</param>
        /// <param name="result">Returns the contents of standard output</param>
        /// <param name="priorityClass">Process priority</param>
        /// <param name="maxWaitMs">Maximum time to wait for completion</param>
        /// <returns><c>true</c> if process was executed and finished correctly</returns>
        public static bool TryExecuteReadString_Impersonated(string executable, string arguments, out Process process, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal, int maxWaitMs = INFINITE)
        {
            IntPtr userToken;
            if (!ImpersonationHelper.GetTokenByProcess(out userToken, true))
            {
                process = null;
                return false;
            }
            try
            {
                return TryExecute_Impersonated(executable, arguments, userToken, out process, priorityClass, maxWaitMs);
            }
            finally
            {
                ImpersonationHelper.SafeCloseHandle(userToken);
            }
        }

        /// <summary>
        /// Indicates if the current <see cref="WindowsIdentity"/> uses impersonation.
        /// </summary>
        private static bool IsImpersonated
        {
            get
            {
                WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
                return windowsIdentity != null && windowsIdentity.ImpersonationLevel == TokenImpersonationLevel.Impersonation;
            }
        }

        /*/// <summary>
        /// Executes the <paramref name="executable"/> and waits a maximum time of <paramref name="maxWaitMs"/> for completion and returns the contents of
        /// <see cref="Process.StandardOutput"/>. If the process doesn't end in this time, it gets aborted.
        /// </summary>
        /// <param name="executable">Program to execute</param>
        /// <param name="arguments">Program arguments</param>
        /// <param name="redirectInputOutput"><c>true</c> to redirect standard streams.</param>
        /// <param name="result">Returns the contents of standard output</param>
        /// <param name="priorityClass">Process priority</param>
        /// <param name="maxWaitMs">Maximum time to wait for completion</param>
        /// <returns></returns>
        private static bool TryExecute(string executable, string arguments, bool redirectInputOutput, out string result, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal, int maxWaitMs = 1000)
        {
            StringBuilder outputBuilder = new StringBuilder();
            using (System.Diagnostics.Process process = new System.Diagnostics.Process { StartInfo = new ProcessStartInfo(executable, arguments) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = redirectInputOutput } })
            {
                *//*if (redirectInputOutput)
                {
                    // Set UTF-8 encoding for standard output.
                    process.StartInfo.StandardOutputEncoding = CONSOLE_ENCODING;
                    // Enable raising events because Process does not raise events by default.
                    process.EnableRaisingEvents = true;
                    // Attach the event handler for OutputDataReceived before starting the process.
                    process.OutputDataReceived += (sender, e) => outputBuilder.Append(e.Data);
                }*//*
                process.Start();
                process.PriorityClass = priorityClass;

                *//*if (redirectInputOutput)
                    process.BeginOutputReadLine();

                if (process.WaitForExit(maxWaitMs))
                {
                    result = RemoveEncodingPreamble(outputBuilder.ToString());
                    return process.ExitCode == 0;
                }
                if (!process.HasExited)
                    process.Kill();*//*
            }
            result = null;
            return false;
        }*/

        /// <summary>
        /// Executes the <paramref name="executable"/> and waits a maximum time of <paramref name="maxWaitMs"/> for completion and returns the contents of
        /// <see cref="Process.StandardOutput"/>. If the process doesn't end in this time, it gets aborted.
        /// </summary>
        /// <param name="executable">Program to execute</param>
        /// <param name="arguments">Program arguments</param>
        /// <param name="redirectInputOutput"><c>true</c> to redirect standard streams.</param>
        /// <param name="result">Returns the contents of standard output</param>
        /// <param name="priorityClass">Process priority</param>
        /// <param name="maxWaitMs">Maximum time to wait for completion</param>
        /// <returns></returns>
        private static bool TryExecute(string executable, string arguments, out Process process, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal, int maxWaitMs = 1000)
        {
            StringBuilder outputBuilder = new StringBuilder();
            using (Process processCreated = new Process { StartInfo = new ProcessStartInfo(executable, arguments) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = false} })
            {
                /*if (redirectInputOutput)
                {
                    // Set UTF-8 encoding for standard output.
                    process.StartInfo.StandardOutputEncoding = CONSOLE_ENCODING;
                    // Enable raising events because Process does not raise events by default.
                    process.EnableRaisingEvents = true;
                    // Attach the event handler for OutputDataReceived before starting the process.
                    process.OutputDataReceived += (sender, e) => outputBuilder.Append(e.Data);
                }*/
                processCreated.Start();
                
                /*if (redirectInputOutput)
                    process.BeginOutputReadLine();*/

                if (processCreated.WaitForExit(maxWaitMs))
                {
                    //result = RemoveEncodingPreamble(outputBuilder.ToString());
                    if (!processCreated.HasExited)
                    {
                        try 
                        {
                            processCreated.PriorityClass = priorityClass;
                        }
                        catch(Exception ex)
                        {
                            logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to set the Priority Class to {priorityClass.ToString("G")} for {executable}.");
                        }
                        process = processCreated;
                        return true;
                    }                        
                }
            }
            process = null;
            return false;
        }

        /*/// <summary>
        /// Executes the <paramref name="executable"/> and waits a maximum time of <paramref name="maxWaitMs"/> for completion. If the process doesn't end in 
        /// this time, it gets aborted. This method tries to impersonate the interactive user and run the process under its identity.
        /// </summary>
        /// <param name="executable">Program to execute</param>
        /// <param name="arguments">Program arguments</param>
        /// <param name="token">User token to run process</param>
        /// <param name="redirectInputOutput"><c>true</c> to redirect standard streams.</param>
        /// <param name="result">Returns the contents of standard output.</param>
        /// <param name="priorityClass">Process priority</param>
        /// <param name="maxWaitMs">Maximum time to wait for completion</param>
        /// <returns><c>true</c> if process was executed and finished correctly</returns>
        private static bool TryExecute_Impersonated(string executable, string arguments, IntPtr token, bool redirectInputOutput, out string result, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal, int maxWaitMs = INFINITE)
        {
            // TODO: code is 99% redundant to TryExecute, refactor Process/ImpersonationProcess and Start/StartAsUser!
            StringBuilder outputBuilder = new StringBuilder();
            using (ImpersonationProcess process = new ImpersonationProcess { StartInfo = new ProcessStartInfo(executable, arguments) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = redirectInputOutput } })
            {
                *//*if (redirectInputOutput)
                {
                    // Set UTF-8 encoding for standard output.
                    process.StartInfo.StandardOutputEncoding = CONSOLE_ENCODING;
                    // Enable raising events because Process does not raise events by default.
                    process.EnableRaisingEvents = true;
                    // Attach the event handler for OutputDataReceived before starting the process.
                    process.OutputDataReceived += (sender, e) => outputBuilder.Append(e.Data);
                }*//*
                process.StartAsUser(token);
                process.PriorityClass = priorityClass;

                *//*if (redirectInputOutput)
                    process.BeginOutputReadLine();*/

                /*if (process.WaitForExit(maxWaitMs))
                {
                    result = RemoveEncodingPreamble(outputBuilder.ToString());
                    return process.ExitCode == 0;
                }
                if (!process.HasExited)
                    process.Kill();*//*
            }
            result = null;
            return false;
        }*/

        /// <summary>
        /// Executes the <paramref name="executable"/> and waits a maximum time of <paramref name="maxWaitMs"/> for completion. If the process doesn't end in 
        /// this time, it gets aborted. This method tries to impersonate the interactive user and run the process under its identity.
        /// </summary>
        /// <param name="executable">Program to execute</param>
        /// <param name="arguments">Program arguments</param>
        /// <param name="token">User token to run process</param>
        /// <param name="redirectInputOutput"><c>true</c> to redirect standard streams.</param>
        /// <param name="result">Returns the contents of standard output.</param>
        /// <param name="priorityClass">Process priority</param>
        /// <param name="maxWaitMs">Maximum time to wait for completion</param>
        /// <returns><c>true</c> if process was executed and finished correctly</returns>
        private static bool TryExecute_Impersonated(string executable, string arguments, IntPtr token, out Process process, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal, int maxWaitMs = 1000)
        {
            // TODO: code is 99% redundant to TryExecute, refactor Process/ImpersonationProcess and Start/StartAsUser!
            StringBuilder outputBuilder = new StringBuilder();
            using (ImpersonationProcess processCreated = new ImpersonationProcess { StartInfo = new ProcessStartInfo(executable, arguments) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = false} })
            {
                /*if (redirectInputOutput)
                {
                    // Set UTF-8 encoding for standard output.
                    process.StartInfo.StandardOutputEncoding = CONSOLE_ENCODING;
                    // Enable raising events because Process does not raise events by default.
                    process.EnableRaisingEvents = true;
                    // Attach the event handler for OutputDataReceived before starting the process.
                    process.OutputDataReceived += (sender, e) => outputBuilder.Append(e.Data);
                }*/
                processCreated.StartAsUser(token);

                /*if (redirectInputOutput)
                    process.BeginOutputReadLine();*/

                if (processCreated.WaitForExit(maxWaitMs))
                {
                    //result = RemoveEncodingPreamble(outputBuilder.ToString());
                    if (!processCreated.HasExited)
                    {
                        try
                        {
                            processCreated.PriorityClass = priorityClass;
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, $"ProcessUtils/TryExecute_Impersonated: Exception while trying to set the Priority Class to {priorityClass.ToString("G")} for {executable}.");
                        }
                        process = processCreated;
                        return true;
                    }
                }
            }
            process = null;
            return false;
        }

        /// <summary>
        /// Helper method to remove an existing encoding preamble (<see cref="Encoding.GetPreamble"/>) from the given <paramref name="rawString"/>.
        /// </summary>
        /// <param name="rawString">Raw string that might include the preamble (BOM).</param>
        /// <returns>String without preamble.</returns>
        private static string RemoveEncodingPreamble(string rawString)
        {
            if (!string.IsNullOrWhiteSpace(rawString) && rawString.StartsWith(CONSOLE_ENCODING_PREAMBLE))
                return rawString.Substring(CONSOLE_ENCODING_PREAMBLE.Length);
            return rawString;
        }

        public static PROCESS_CREATION_FLAGS TranslatePriorityClassToFlags(ProcessPriorityClass processPriorityClass)
        {
            PROCESS_CREATION_FLAGS wantedPriorityClass = PROCESS_CREATION_FLAGS.NORMAL_PRIORITY_CLASS;
            switch (processPriorityClass)
            {
                case ProcessPriorityClass.High:
                    wantedPriorityClass = PROCESS_CREATION_FLAGS.HIGH_PRIORITY_CLASS;
                    break;
                case ProcessPriorityClass.AboveNormal:
                    wantedPriorityClass = PROCESS_CREATION_FLAGS.ABOVE_NORMAL_PRIORITY_CLASS;
                    break;
                case ProcessPriorityClass.Normal:
                    wantedPriorityClass = PROCESS_CREATION_FLAGS.NORMAL_PRIORITY_CLASS;
                    break;
                case ProcessPriorityClass.BelowNormal:
                    wantedPriorityClass = PROCESS_CREATION_FLAGS.BELOW_NORMAL_PRIORITY_CLASS;
                    break;
                case ProcessPriorityClass.Idle:
                    wantedPriorityClass = PROCESS_CREATION_FLAGS.IDLE_PRIORITY_CLASS;
                    break;
                default:
                    wantedPriorityClass = PROCESS_CREATION_FLAGS.NORMAL_PRIORITY_CLASS;
                    break;
            }
            return wantedPriorityClass;
        }

    }

    public class ProcessUtilsOld
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        const uint SE_GROUP_INTEGRITY = 0x00000020;
        private const uint INVALID_SESSION_ID = 0xFFFFFFFF;
        private static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

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
            CREATE_SEPARATE_WOW_VDM = 0x00001000,
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

        private enum SW
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10
        }

        private enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        private enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous = 0,
            SecurityIdentification = 1,
            SecurityImpersonation = 2,
            SecurityDelegation = 3,
        }

        public enum SaferLevel : uint
        {
            Disallowed = 0,
            Untrusted = 0x1000,
            Constrained = 0x10000,
            NormalUser = 0x20000,
            FullyTrusted = 0x40000
        }

        public enum SaferScope : uint
        {
            Machine = 1,
            User = 2
        }

        [Flags]
        public enum SaferOpenFlags : uint
        {
            Open = 1
        }

        private enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation = 2
        }

        public enum TOKEN_INFORMATION_CLASS
        {
            /// <summary>
            /// The buffer receives a TOKEN_USER structure that contains the user account of the token.
            /// </summary>
            TokenUser = 1,

            /// <summary>
            /// The buffer receives a TOKEN_GROUPS structure that contains the group accounts associated with the token.
            /// </summary>
            TokenGroups,

            /// <summary>
            /// The buffer receives a TOKEN_PRIVILEGES structure that contains the privileges of the token.
            /// </summary>
            TokenPrivileges,

            /// <summary>
            /// The buffer receives a TOKEN_OWNER structure that contains the default owner security identifier (SID) for newly created objects.
            /// </summary>
            TokenOwner,

            /// <summary>
            /// The buffer receives a TOKEN_PRIMARY_GROUP structure that contains the default primary group SID for newly created objects.
            /// </summary>
            TokenPrimaryGroup,

            /// <summary>
            /// The buffer receives a TOKEN_DEFAULT_DACL structure that contains the default DACL for newly created objects.
            /// </summary>
            TokenDefaultDacl,

            /// <summary>
            /// The buffer receives a TOKEN_SOURCE structure that contains the source of the token. TOKEN_QUERY_SOURCE access is needed to retrieve this information.
            /// </summary>
            TokenSource,

            /// <summary>
            /// The buffer receives a TOKEN_TYPE value that indicates whether the token is a primary or impersonation token.
            /// </summary>
            TokenType,

            /// <summary>
            /// The buffer receives a SECURITY_IMPERSONATION_LEVEL value that indicates the impersonation level of the token. If the access token is not an impersonation token, the function fails.
            /// </summary>
            TokenImpersonationLevel,

            /// <summary>
            /// The buffer receives a TOKEN_STATISTICS structure that contains various token statistics.
            /// </summary>
            TokenStatistics,

            /// <summary>
            /// The buffer receives a TOKEN_GROUPS structure that contains the list of restricting SIDs in a restricted token.
            /// </summary>
            TokenRestrictedSids,

            /// <summary>
            /// The buffer receives a DWORD value that indicates the Terminal Services session identifier that is associated with the token.
            /// </summary>
            TokenSessionId,

            /// <summary>
            /// The buffer receives a TOKEN_GROUPS_AND_PRIVILEGES structure that contains the user SID, the group accounts, the restricted SIDs, and the authentication ID associated with the token.
            /// </summary>
            TokenGroupsAndPrivileges,

            /// <summary>
            /// Reserved.
            /// </summary>
            TokenSessionReference,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if the token includes the SANDBOX_INERT flag.
            /// </summary>
            TokenSandBoxInert,

            /// <summary>
            /// Reserved.
            /// </summary>
            TokenAuditPolicy,

            /// <summary>
            /// The buffer receives a TOKEN_ORIGIN value.
            /// </summary>
            TokenOrigin,

            /// <summary>
            /// The buffer receives a TOKEN_ELEVATION_TYPE value that specifies the elevation level of the token.
            /// </summary>
            TokenElevationType,

            /// <summary>
            /// The buffer receives a TOKEN_LINKED_TOKEN structure that contains a handle to another token that is linked to this token.
            /// </summary>
            TokenLinkedToken,

            /// <summary>
            /// The buffer receives a TOKEN_ELEVATION structure that specifies whether the token is elevated.
            /// </summary>
            TokenElevation,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if the token has ever been filtered.
            /// </summary>
            TokenHasRestrictions,

            /// <summary>
            /// The buffer receives a TOKEN_ACCESS_INFORMATION structure that specifies security information contained in the token.
            /// </summary>
            TokenAccessInformation,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if virtualization is allowed for the token.
            /// </summary>
            TokenVirtualizationAllowed,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if virtualization is enabled for the token.
            /// </summary>
            TokenVirtualizationEnabled,

            /// <summary>
            /// The buffer receives a TOKEN_MANDATORY_LABEL structure that specifies the token's integrity level.
            /// </summary>
            TokenIntegrityLevel,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if the token has the UIAccess flag set.
            /// </summary>
            TokenUIAccess,

            /// <summary>
            /// The buffer receives a TOKEN_MANDATORY_POLICY structure that specifies the token's mandatory integrity policy.
            /// </summary>
            TokenMandatoryPolicy,

            /// <summary>
            /// The buffer receives the token's logon security identifier (SID).
            /// </summary>
            TokenLogonSid,

            /// <summary>
            /// The maximum value for this enumeration
            /// </summary>
            MaxTokenInfoClass
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public UInt32 dwX;
            public UInt32 dwY;
            public UInt32 dwXSize;
            public UInt32 dwYSize;
            public UInt32 dwXCountChars;
            public UInt32 dwYCountChars;
            public UInt32 dwFillAttribute;
            public UInt32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public  struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SID_AND_ATTRIBUTES
        {
            public IntPtr Sid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_MANDATORY_LABEL
        {
            public SID_AND_ATTRIBUTES Label;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public readonly UInt32 SessionID;

            [MarshalAs(UnmanagedType.LPStr)]
            public readonly String pWinStationName;

            public readonly WTS_CONNECTSTATE_CLASS State;
        }




        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern bool SaferCreateLevel(SaferScope scope, SaferLevel level, SaferOpenFlags openFlags, out IntPtr pLevelHandle, IntPtr lpReserved);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern bool SaferComputeTokenFromLevel(IntPtr LevelHandle, IntPtr InAccessToken, out IntPtr OutAccessToken, int dwFlags, IntPtr lpReserved);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SaferCloseLevel(IntPtr hLevelHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool ConvertStringSidToSid(string StringSid, out IntPtr ptrSid);

        private static bool SafeCloseHandle(IntPtr hObject)
        {
            return (hObject == IntPtr.Zero) ? true : CloseHandle(hObject);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateProcess(
            string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, PROCESS_CREATION_FLAGS dwCreationFlags,
            IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFOEX lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateProcess(
            string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes, bool bInheritHandles, PROCESS_CREATION_FLAGS dwCreationFlags,
            IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFOEX lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern Boolean SetTokenInformation(
            IntPtr TokenHandle,
            TOKEN_INFORMATION_CLASS TokenInformationClass,
            IntPtr TokenInformation,
            UInt32 TokenInformationLength);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            PROCESS_CREATION_FLAGS dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UpdateProcThreadAttribute(
            IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute, IntPtr lpValue,
            IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InitializeProcThreadAttributeList(
            IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref IntPtr lpSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteProcThreadAttributeList(IntPtr lpAttributeList);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint SuspendThread(IntPtr hThread);

        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
        private static extern bool DuplicateTokenEx(
            IntPtr ExistingTokenHandle,
            uint dwDesiredAccess,
            IntPtr lpThreadAttributes,
            int TokenType,
            int ImpersonationLevel,
            ref IntPtr DuplicateTokenHandle);

        [DllImport("userenv.dll", SetLastError = true)]
        private static extern bool CreateEnvironmentBlock(ref IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

        [DllImport("userenv.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hSnapshot);

        [DllImport("kernel32.dll")]
        private static extern uint WTSGetActiveConsoleSessionId();

        [DllImport("Wtsapi32.dll")]
        private static extern uint WTSQueryUserToken(uint SessionId, ref IntPtr phToken);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern int WTSEnumerateSessions(
            IntPtr hServer,
            int Reserved,
            int Version,
            ref IntPtr ppSessionInfo,
            ref int pCount);



        public static ProcessPriorityClass TranslatePriorityToClass(ProcessPriority processPriorityClass)
        {
            ProcessPriorityClass wantedPriorityClass = ProcessPriorityClass.Normal;
            switch (processPriorityClass)
            {
                case ProcessPriority.High:
                    wantedPriorityClass = ProcessPriorityClass.High;
                    break;
                case ProcessPriority.AboveNormal:
                    wantedPriorityClass = ProcessPriorityClass.AboveNormal;
                    break;
                case ProcessPriority.Normal:
                    wantedPriorityClass = ProcessPriorityClass.Normal;
                    break;
                case ProcessPriority.BelowNormal:
                    wantedPriorityClass = ProcessPriorityClass.BelowNormal;
                    break;
                case ProcessPriority.Idle:
                    wantedPriorityClass = ProcessPriorityClass.Idle;
                    break;
                default:
                    wantedPriorityClass = ProcessPriorityClass.Normal;
                    break;
            }
            return wantedPriorityClass;
        }

        public static PROCESS_CREATION_FLAGS TranslatePriorityClassToFlags(ProcessPriorityClass processPriorityClass)
        {
            PROCESS_CREATION_FLAGS wantedPriorityClass = PROCESS_CREATION_FLAGS.NORMAL_PRIORITY_CLASS;
            switch (processPriorityClass)
            {
                case ProcessPriorityClass.High:
                    wantedPriorityClass = PROCESS_CREATION_FLAGS.HIGH_PRIORITY_CLASS;
                    break;
                case ProcessPriorityClass.AboveNormal:
                    wantedPriorityClass = PROCESS_CREATION_FLAGS.ABOVE_NORMAL_PRIORITY_CLASS;
                    break;
                case ProcessPriorityClass.Normal:
                    wantedPriorityClass = PROCESS_CREATION_FLAGS.NORMAL_PRIORITY_CLASS;
                    break;
                case ProcessPriorityClass.BelowNormal:
                    wantedPriorityClass = PROCESS_CREATION_FLAGS.BELOW_NORMAL_PRIORITY_CLASS;
                    break;
                case ProcessPriorityClass.Idle:
                    wantedPriorityClass = PROCESS_CREATION_FLAGS.IDLE_PRIORITY_CLASS;
                    break;
                default:
                    wantedPriorityClass = PROCESS_CREATION_FLAGS.NORMAL_PRIORITY_CLASS;
                    break;
            }
            return wantedPriorityClass;
        }

        public static List<Process> StartProcess(string executable, string arguments, ProcessPriority processPriority, int startTimeout = 1)
        {
            List<Process> runningProcesses = new List<Process>();
            Process process = null;
            PROCESS_INFORMATION processInfo;
            bool usingChildProcess = false;
            try
            {
                //if (CreateProcessWithPriorityAsRestrictedUser(executable, arguments, ProcessUtils.TranslatePriorityToClass(processPriority), out processInfo))                    
                //if (CreateProcessWithPriority(executable, arguments, ProcessUtils.TranslatePriorityToClass(processPriority), out processInfo))
                if (StartProcessAsCurrentUser(out processInfo, executable))
                {
                    if (processInfo.dwProcessId > 0)
                    {
                        try
                        {
                            process = Process.GetProcessById((int)processInfo.dwProcessId);
                            Task.Delay(500);
                            if (process.HasExited)
                            {
                                // it's a launcher! We need to look for children
                                List<Process> childProcesses = GetChildProcesses(process);
                                runningProcesses.AddRange(childProcesses);
                                usingChildProcess = true;
                            }
                            else
                            {
                                runningProcesses.Add(process);
                            }
                        }
                        catch (Exception ex)
                        {
                            // it's a launcher! We need to look for children
                            List<Process> childProcesses = GetChildProcesses((int)processInfo.dwProcessId);
                            runningProcesses.AddRange(childProcesses);
                            usingChildProcess = true;
                        }

                    }
                    else
                    {
                        logger.Warn($"ProcessUtils/StartProcess: CreateProcessWithPriority returned a process with PID 0 when trying to start process {executable}. This indicates that the process was not started, so we'll try it a different way.");
                        // Start the process using built in process library
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.FileName = executable;
                        psi.Arguments = arguments;
                        psi.WorkingDirectory = Path.GetDirectoryName(executable);
                        process = Process.Start(psi);
                        processInfo.hProcess = process.Handle;
                        processInfo.dwProcessId = (uint)process.Id;
                        if (!process.HasExited)
                        {
                            processInfo.dwThreadId = (uint)process.Threads[0].Id;

                            // Change priority if we can (not always possible in this mode :(
                            try
                            {
                                // If this process is a protected process, then this will fail!
                                process.PriorityClass = TranslatePriorityToClass(processPriority);
                            }
                            catch (Exception ex)
                            {
                                // We would need need higher rights for this processto set the priority
                                // https://docs.microsoft.com/en-us/windows/win32/procthread/process-security-and-access-rights
                                // At this stage I am not writing this, as it is a lot of work for a niche issue.
                            }
                            runningProcesses.Add(process);
                        }
                    }
                }
                else
                {
                    // Start the process using built in process library
                    ProcessStartInfo psi = new ProcessStartInfo();
                    string extension = Path.GetExtension(executable);
                    if (extension.Equals("com", StringComparison.CurrentCultureIgnoreCase)
                        || extension.Equals("exe", StringComparison.CurrentCultureIgnoreCase)
                        || extension.Equals("msi", StringComparison.CurrentCultureIgnoreCase))
                    {
                        psi.UseShellExecute = false;
                    }
                    else
                    {
                        psi.Verb = "Open";                        
                    }
                    psi.FileName = executable;
                    psi.Arguments = arguments;
                    psi.WorkingDirectory = Path.GetDirectoryName(executable);
                    process = Process.Start(psi);
                    processInfo.hProcess = process.Handle;
                    processInfo.dwProcessId = (uint)process.Id;
                    if (!process.HasExited)
                    {
                        processInfo.dwThreadId = (uint)process.Threads[0].Id;
                        // Change priority if we can (not always possible in this mode :(
                        try
                        {
                            // If this process is a protected process, then this will fail!
                            process.PriorityClass = TranslatePriorityToClass(processPriority);
                        }
                        catch(Exception ex)
                        {
                            // We would need need higher rights for this processto set the priority
                            // https://docs.microsoft.com/en-us/windows/win32/procthread/process-security-and-access-rights
                            // At this stage I am not writing this, as it is a lot of work for a niche issue.
                        }
                        runningProcesses.Add(process);
                    }

                }

            }
            catch (Exception ex)
            {
                if (!process.HasExited)
                {
                    // Start the process using built in process library
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = executable;
                    psi.Arguments = arguments;
                    psi.WorkingDirectory = Path.GetDirectoryName(executable);
                    process = Process.Start(psi);
                    processInfo.hProcess = process.Handle;
                    processInfo.dwProcessId = (uint)process.Id;
                    processInfo.dwThreadId = (uint)process.Threads[0].Id;
                    //pInfo.dwThreadId = process.Threads[0].Id;
                    // Change priority
                    if (!process.HasExited)
                    {
                        runningProcesses.Add(process);
                    }
                }
                
            }


            // Check the launched exe hasn't exited within 2 secs
            if (!usingChildProcess)
            {
                for (int secs = 0; secs <= (startTimeout * 1000); secs += 500)
                {
                    // If we have no more processes left then we're done!
                    if (process.HasExited)
                    {
                        logger.Trace($"ProcessUtils/StartProcess: {executable} has exited early! It's likely to be a launcher! Trying to detect it's children.");
                        // As the original process has left the building, we'll overwrite it with the children processes
                        runningProcesses = GetChildProcesses(process);
                        break;
                    }
                    // Send a message to windows so that it doesn't think
                    // we're locked and try to kill us
                    System.Threading.Thread.CurrentThread.Join(0);
                    Thread.Sleep(500);
                }

            }

            return runningProcesses;
        }

        public static List<Process> GetChildProcesses(Process process)
        {
            List<Process> children = new List<Process>();
            ManagementObjectSearcher mos = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={process.Id}");
            foreach (ManagementObject mo in mos.Get())
            {
                children.Add(Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])));
            }
            return children;
        }

        public static List<Process> GetChildProcesses(int processId)
        {
            List<Process> children = new List<Process>();
            ManagementObjectSearcher mos = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={processId}");
            foreach (ManagementObject mo in mos.Get())
            {
                children.Add(Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])));
            }
            return children;
        }

        public static bool CreateProcessWithPriority(string fileName, string args, ProcessPriorityClass priorityClass, out PROCESS_INFORMATION processInfo)
        {
            PROCESS_CREATION_FLAGS processFlags = TranslatePriorityClassToFlags(priorityClass);
            var cmd = new StringBuilder();
            cmd.Append('"').Append(fileName).Append('"');
            if (!string.IsNullOrWhiteSpace(args))
            {
                cmd.Append(' ').Append(args);
            }
            bool success = false;
            PROCESS_INFORMATION pInfo = new PROCESS_INFORMATION();
            var pSec = new SECURITY_ATTRIBUTES();
            var tSec = new SECURITY_ATTRIBUTES();
            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);
            var sInfoEx = new STARTUPINFOEX();
            sInfoEx.StartupInfo.cb = Marshal.SizeOf(sInfoEx);
            try
            {
                success = CreateProcess(null, cmd.ToString(), ref pSec, ref tSec, false, processFlags, IntPtr.Zero, null, ref sInfoEx, out pInfo);
            }
            catch (Exception ex)
            {
                // This is a problem
            }
            if (!success)
            {
                try
                {
                    success = CreateProcess(null, cmd.ToString(), IntPtr.Zero, IntPtr.Zero, false, processFlags, IntPtr.Zero, null, ref sInfoEx, out pInfo);
                }
                catch (Exception ex)
                {
                    // This is a problem too                    
                }
            }
            processInfo = pInfo;

            return success;
        }

        /// Runs a process as a non-elevated version of the current user.
        public static bool CreateProcessWithPriorityAsRestrictedUser(string fileName, string args, ProcessPriorityClass priorityClass, out PROCESS_INFORMATION processInfo)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));

            var pi = new PROCESS_INFORMATION();
            if (GetRestrictedSessionUserToken(out var hRestrictedToken))
            {
                try
                {
                    var si = new STARTUPINFO();
                    var cmd = new StringBuilder();
                    cmd.Append('"').Append(fileName).Append('"');
                    if (!string.IsNullOrWhiteSpace(args))
                    {
                        cmd.Append(' ').Append(args);
                    }

                    if (!CreateProcessAsUser(
                        hRestrictedToken,
                        null,
                        cmd.ToString(),
                        IntPtr.Zero,
                        IntPtr.Zero,
                        true, // inherit handle
                        0,
                        IntPtr.Zero,
                        null,
                        ref si,
                        out pi))
                    {
                        processInfo = pi;
                        return false;
                    }

                    
                }
                finally
                {
                    CloseHandle(hRestrictedToken);
                }
                processInfo = pi;
                return true;
            }
            else
            {
                processInfo = pi;
                return false;
            }
            
        }

        // based on https://stackoverflow.com/a/16110126/862099
        private static bool GetRestrictedSessionUserToken(out IntPtr token)
        {
            token = IntPtr.Zero;
            if (!SaferCreateLevel(SaferScope.User, SaferLevel.NormalUser, SaferOpenFlags.Open, out var hLevel, IntPtr.Zero))
            {
                return false;
            }

            IntPtr hRestrictedToken = IntPtr.Zero;
            TOKEN_MANDATORY_LABEL tml = default;
            tml.Label.Sid = IntPtr.Zero;
            IntPtr tmlPtr = IntPtr.Zero;

            try
            {
                if (!SaferComputeTokenFromLevel(hLevel, IntPtr.Zero, out hRestrictedToken, 0, IntPtr.Zero))
                {
                    return false;
                }

                // Set the token to medium integrity.
                tml.Label.Attributes = SE_GROUP_INTEGRITY;
                tml.Label.Sid = IntPtr.Zero;
                if (!ConvertStringSidToSid("S-1-16-8192", out tml.Label.Sid))
                {
                    return false;
                }

                tmlPtr = Marshal.AllocHGlobal(Marshal.SizeOf(tml));
                Marshal.StructureToPtr(tml, tmlPtr, false);
                if (!SetTokenInformation(hRestrictedToken,
                    TOKEN_INFORMATION_CLASS.TokenIntegrityLevel,
                    tmlPtr, (uint)Marshal.SizeOf(tml)))
                {
                    return false;
                }

                token = hRestrictedToken;
                hRestrictedToken = IntPtr.Zero; // make sure finally() doesn't close the handle
            }
            finally
            {
                SaferCloseLevel(hLevel);
                SafeCloseHandle(hRestrictedToken);
                if (tml.Label.Sid != IntPtr.Zero)
                {
                    LocalFree(tml.Label.Sid);
                }
                if (tmlPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(tmlPtr);
                }
            }

            return true;
        }

        public static void ResumeProcess(PROCESS_INFORMATION processInfo)
        {
            ResumeThread(processInfo.hThread);
        }

        public static bool CreateProcessWithParent(int parentProcessId)
        {
            const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;

            var pInfo = new PROCESS_INFORMATION();
            var sInfoEx = new STARTUPINFOEX();
            sInfoEx.StartupInfo.cb = Marshal.SizeOf(sInfoEx);
            IntPtr lpValue = IntPtr.Zero;

            try
            {
                if (parentProcessId > 0)
                {
                    var lpSize = IntPtr.Zero;
                    var success = InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
                    if (success || lpSize == IntPtr.Zero)
                    {
                        return false;
                    }

                    sInfoEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
                    success = InitializeProcThreadAttributeList(sInfoEx.lpAttributeList, 1, 0, ref lpSize);
                    if (!success)
                    {
                        return false;
                    }

                    var parentHandle = Process.GetProcessById(parentProcessId).Handle;
                    // This value should persist until the attribute list is destroyed using the DeleteProcThreadAttributeList function
                    lpValue = Marshal.AllocHGlobal(IntPtr.Size);
                    Marshal.WriteIntPtr(lpValue, parentHandle);

                    success = UpdateProcThreadAttribute(
                        sInfoEx.lpAttributeList,
                        0,
                        (IntPtr)PROC_THREAD_ATTRIBUTE_PARENT_PROCESS,
                        lpValue,
                        (IntPtr)IntPtr.Size,
                        IntPtr.Zero,
                        IntPtr.Zero);
                    if (!success)
                    {
                        return false;
                    }
                }

                var pSec = new SECURITY_ATTRIBUTES();
                var tSec = new SECURITY_ATTRIBUTES();
                pSec.nLength = Marshal.SizeOf(pSec);
                tSec.nLength = Marshal.SizeOf(tSec);
                var lpApplicationName = Path.Combine(Environment.SystemDirectory, "notepad.exe");
                return CreateProcess(lpApplicationName, null, ref pSec, ref tSec, false, PROCESS_CREATION_FLAGS.EXTENDED_STARTUPINFO_PRESENT, IntPtr.Zero, null, ref sInfoEx, out pInfo);
            }
            finally
            {
                // Free the attribute list
                if (sInfoEx.lpAttributeList != IntPtr.Zero)
                {
                    DeleteProcThreadAttributeList(sInfoEx.lpAttributeList);
                    Marshal.FreeHGlobal(sInfoEx.lpAttributeList);
                }
                Marshal.FreeHGlobal(lpValue);

                // Close process and thread handles
                if (pInfo.hProcess != IntPtr.Zero)
                {
                    CloseHandle(pInfo.hProcess);
                }
                if (pInfo.hThread != IntPtr.Zero)
                {
                    CloseHandle(pInfo.hThread);
                }
            }
        }

        // Gets the user token from the currently active session
        private static bool GetSessionUserToken(ref IntPtr phUserToken)
        {
            var bResult = false;
            var hImpersonationToken = IntPtr.Zero;
            var activeSessionId = INVALID_SESSION_ID;
            var pSessionInfo = IntPtr.Zero;
            var sessionCount = 0;

            // Get a handle to the user access token for the current active session.
            if (WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, ref pSessionInfo, ref sessionCount) != 0)
            {
                var arrayElementSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                var current = pSessionInfo;

                for (var i = 0; i < sessionCount; i++)
                {
                    var si = (WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)current, typeof(WTS_SESSION_INFO));
                    current += arrayElementSize;

                    if (si.State == WTS_CONNECTSTATE_CLASS.WTSActive)
                    {
                        activeSessionId = si.SessionID;
                    }
                }
            }

            // If enumerating did not work, fall back to the old method
            if (activeSessionId == INVALID_SESSION_ID)
            {
                activeSessionId = WTSGetActiveConsoleSessionId();
            }

            if (WTSQueryUserToken(activeSessionId, ref hImpersonationToken) != 0)
            {
                // Convert the impersonation token to a primary token
                bResult = DuplicateTokenEx(hImpersonationToken, 0, IntPtr.Zero,
                    (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, (int)TOKEN_TYPE.TokenPrimary,
                    ref phUserToken);

                CloseHandle(hImpersonationToken);
            }

            return bResult;
        }

        public static bool StartProcessAsCurrentUser(out PROCESS_INFORMATION processInfo, string appPath, string cmdLine = null, string workDir = null, bool visible = true )
        {
            var hUserToken = IntPtr.Zero;
            var startInfo = new STARTUPINFO();
            var procInfo = new PROCESS_INFORMATION();
            var pEnv = IntPtr.Zero;
            int iResultOfCreateProcessAsUser;

            startInfo.cb = Marshal.SizeOf(typeof(STARTUPINFO));

            try
            {
                if (!GetSessionUserToken(ref hUserToken))
                {
                    throw new Exception("StartProcessAsCurrentUser: GetSessionUserToken failed.");
                }

                PROCESS_CREATION_FLAGS dwCreationFlags = PROCESS_CREATION_FLAGS.CREATE_UNICODE_ENVIRONMENT | (visible ? PROCESS_CREATION_FLAGS.CREATE_NEW_CONSOLE : PROCESS_CREATION_FLAGS.CREATE_NO_WINDOW);
                startInfo.wShowWindow = (short)(visible ? SW.SW_SHOW : SW.SW_HIDE);
                startInfo.lpDesktop = "winsta0\\default";

                if (!CreateEnvironmentBlock(ref pEnv, hUserToken, false))
                {
                    throw new Exception("StartProcessAsCurrentUser: CreateEnvironmentBlock failed.");
                }

                if (!CreateProcessAsUser(hUserToken,
                    appPath, // Application Name
                    cmdLine, // Command Line
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    dwCreationFlags,
                    pEnv,
                    workDir, // Working directory
                    ref startInfo,
                    out procInfo))
                {
                    iResultOfCreateProcessAsUser = Marshal.GetLastWin32Error();
                    throw new Exception("StartProcessAsCurrentUser: CreateProcessAsUser failed.  Error Code -" + iResultOfCreateProcessAsUser);
                }

                iResultOfCreateProcessAsUser = Marshal.GetLastWin32Error();
            }
            finally
            {
                CloseHandle(hUserToken);
                if (pEnv != IntPtr.Zero)
                {
                    DestroyEnvironmentBlock(pEnv);
                }
                CloseHandle(procInfo.hThread);
                CloseHandle(procInfo.hProcess);
            }
            processInfo = procInfo;
            return true;
        }

        public static bool ProcessExited(Process process)
        {
            try
            {
                Process processToTest = Process.GetProcessById(process.Id);
                if (processToTest.HasExited)
                {
                    logger.Trace($"ProcessUtils/ProcessExited: {process.Id} has exited and is not running. This means the process has finished!");
                    return true;
                }
                else
                {
                    logger.Trace($"ProcessUtils/ProcessExited: {process.Id} is still running as is has not exited yet.");
                    return false;
                }
            }
            catch (ArgumentException ex)
            {
                logger.Trace($"ProcessUtils/ProcessExited: {process.Id} is not running, and the process ID has expired. This means the process has finished!");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn($"ProcessUtils/ProcessExited: {process.Id} was not started by this process object. This likely means the process has finished!");
                return true;
            }
            catch (Exception ex)
            {
                logger.Trace($"ProcessUtils/ProcessExited: Exception when checking if {process.Id} is still running, so assuming the process has finished!");
                return true;
            }
        }

        public static bool ProcessExited(List<Process> processes)
        {
            int processClosedCount = 0;
            foreach (Process p in processes)
            {
                if (ProcessExited(p))
                {
                    processClosedCount++;
                }
            }
            if (processClosedCount == processes.Count)
            {
                logger.Trace($"ProcessUtils/ProcessExited2: All processes being monitored have exited, so no processes still running!");
                return true;
            }
            else
            {
                logger.Trace($"ProcessUtils/ProcessExited2: {processClosedCount} processes out of {processes.Count} processes have exited. At least one process is still running!");
                return false;
            }
        }

        public static bool StopProcess(Process processToStop)
        {
            try
            {
                // Stop the process
                processToStop.CloseMainWindow();
                if (!processToStop.WaitForExit(1000))
                {
                    logger.Trace($"ProcessUtils/StopProcess: Process {processToStop.StartInfo.FileName} wouldn't stop cleanly. Forcing program close.");
                    processToStop.Kill();
                    if (!processToStop.WaitForExit(5000))
                    {
                        logger.Error($"ProcessUtils/StopProcess: Process {processToStop.StartInfo.FileName} couldn't be killed! It seems like something is actively preventing us from stopping the process");
                        return false;
                    }
                    logger.Trace($"ProcessUtils/StopProcess: Process {processToStop.StartInfo.FileName} was successfully killed.");
                }
                processToStop.Close();
                return true;
            }
            catch (Win32Exception ex)
            {
                logger.Warn(ex, $"ProcessUtils/StopProcess: Win32Exception! Couldn't access the wait status for a named process we're trying to stop. So now just killing the process.");
                processToStop.Kill();
                if (!processToStop.WaitForExit(5000))
                {
                    logger.Error($"ProcessUtils/StopProcess: Win32Exception! Process {processToStop.StartInfo.FileName} couldn't be killed! It seems like something is actively preventing us from stopping the process");
                    return false;
                }
                logger.Trace($"ProcessUtils/StopProcess: Win32Exception! Process {processToStop.StartInfo.FileName} was successfully killed.");
                processToStop.Close();
                return true;
            }
            catch (InvalidOperationException ex)
            {
                logger.Error(ex, $"ProcessUtils/StopProcess: Couldn't kill the named process as the process appears to have closed already.");
            }
            catch (SystemException ex)
            {
                logger.Error(ex, $"ProcessUtils/StopProcess: Couldn't WaitForExit the named process as there is no process associated with the Process object (or cannot get the ID from the named process handle).");
            }

            catch (AggregateException ae)
            {
                logger.Error(ae, $"ProcessUtils/StopProcess: Got an AggregateException.");
            }
            return false;
        }

        public static bool StopProcess(List<Process> processes)
        {
            // Stop the programs in the reverse order we started them
            foreach (Process processToStop in processes)
            {
                // Stop the process if it hasn't stopped already
                try
                {
                    if (!processToStop.HasExited)
                    {
                        logger.Debug($"ShortcutRepository/RunShortcut: Stopping process {processToStop.StartInfo.FileName}");
                        if (ProcessUtils.StopProcess(processToStop))
                        {
                            logger.Debug($"ShortcutRepository/RunShortcut: Successfully stopped process {processToStop.StartInfo.FileName}");
                        }
                        else
                        {
                            logger.Warn($"ShortcutRepository/RunShortcut: Failed to stop process {processToStop.StartInfo.FileName} after main executable or game was exited by the user.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ShortcutRepository/RunShortcut: Exception while checking if processToStop has already exited");
                }

            }
            return true;
        }
    }
}       

