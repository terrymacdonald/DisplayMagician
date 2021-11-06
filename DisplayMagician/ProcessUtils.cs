using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DisplayMagician
{
    public class ProcessUtils
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        [Flags]
        public enum PROCESS_CREATION_FLAGS : UInt32
        {
            ZERO_FLAG = 0x00000000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00001000,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
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
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
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
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }


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
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint SuspendThread(IntPtr hThread);

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
            try
            {
                if (CreateProcessWithPriority(executable, arguments, ProcessUtils.TranslatePriorityToClass(processPriority), out processInfo))
                {
                    if (processInfo.dwProcessId > 0)
                    {
                        process = Process.GetProcessById(processInfo.dwProcessId);
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
                        processInfo.dwProcessId = process.Id;
                        processInfo.dwThreadId = process.Threads[0].Id;
                        if (!process.HasExited)
                        {
                            // Change priority if we can (not always possible in this mode :(
                            try
                            {
                                // If this process is a protected process, then this will fail!
                                process.PriorityClass = ProcessUtils.TranslatePriorityToClass(processPriority);
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
                    psi.FileName = executable;
                    psi.Arguments = arguments;
                    psi.WorkingDirectory = Path.GetDirectoryName(executable);
                    process = Process.Start(psi);
                    processInfo.hProcess = process.Handle;
                    processInfo.dwProcessId = process.Id;
                    if (!process.HasExited)
                    {
                        processInfo.dwThreadId = process.Threads[0].Id;
                        // Change priority if we can (not always possible in this mode :(
                        try
                        {
                            // If this process is a protected process, then this will fail!
                            process.PriorityClass = ProcessUtils.TranslatePriorityToClass(processPriority);
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
                    processInfo.dwProcessId = process.Id;
                    processInfo.dwThreadId = process.Threads[0].Id;
                    //pInfo.dwThreadId = process.Threads[0].Id;
                    // Change priority
                    if (!process.HasExited)
                    {
                        runningProcesses.Add(process);
                    }
                }
                
            }


            // Check the launched exe hasn't exited within 2 secs
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

        public static bool CreateProcessWithPriority(string exeName, string cmdLine, ProcessPriorityClass priorityClass, out PROCESS_INFORMATION processInfo)
        {
            PROCESS_CREATION_FLAGS processFlags = TranslatePriorityClassToFlags(priorityClass);
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
                success = CreateProcess(exeName, cmdLine, ref pSec, ref tSec, false, processFlags, IntPtr.Zero, null, ref sInfoEx, out pInfo);
            }
            catch (Exception ex)
            {
                // This is a problem
            }
            if (!success)
            {
                try
                {
                    success = CreateProcess(exeName, cmdLine, IntPtr.Zero, IntPtr.Zero, false, processFlags, IntPtr.Zero, null, ref sInfoEx, out pInfo);
                }
                catch (Exception ex)
                {
                    // This is a problem too                    
                }
            }
            processInfo = pInfo;

            return success;
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
                if (!processToStop.WaitForExit(5000))
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

