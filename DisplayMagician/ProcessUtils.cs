using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagician
{

    [Flags]
    public enum ProcessCreationFlags : uint
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

    public struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }

    public struct STARTUPINFO
    {
        public uint cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    public static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
                                 bool bInheritHandles, ProcessCreationFlags dwCreationFlags, IntPtr lpEnvironment,
                                string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern uint SuspendThread(IntPtr hThread);
    }

    class ProcessInfo : IComparable<ProcessInfo>
    {
        public Process TheProcess;
        public ProcessInfo Parent;
        public List<ProcessInfo> Children = new List<ProcessInfo>();

        public ProcessInfo(Process the_process)
        {
            TheProcess = the_process;
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]",
                TheProcess.ProcessName, TheProcess.Id);
        }

        public int CompareTo(ProcessInfo other)
        {
            return TheProcess.ProcessName.CompareTo(
                other.TheProcess.ProcessName);
        }
    }

    static class ProcessUtils
    {
        private static Dictionary<int, ProcessInfo> allProcessInfosDict;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static IntPtr ThreadHandle = IntPtr.Zero;

        public static void Initialise()
        {
            allProcessInfosDict = new Dictionary<int, ProcessInfo>();

            try
            {
                // Get the parent/child info.
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                   "SELECT ProcessId, ParentProcessId FROM Win32_Process");
                ManagementObjectCollection collection = searcher.Get();

                // Get the processes.
                foreach (Process process in Process.GetProcesses())
                {
                    allProcessInfosDict.Add(process.Id, new ProcessInfo(process));
                }

                // Create the child lists.
                foreach (var item in collection)
                {
                    // Find the parent and child in the dictionary.
                    int child_id = Convert.ToInt32(item["ProcessId"]);
                    int parent_id = Convert.ToInt32(item["ParentProcessId"]);

                    ProcessInfo child_info = null;
                    ProcessInfo parent_info = null;
                    if (allProcessInfosDict.ContainsKey(child_id))
                        child_info = allProcessInfosDict[child_id];
                    if (allProcessInfosDict.ContainsKey(parent_id))
                        parent_info = allProcessInfosDict[parent_id];

                    if (child_info == null)
                        Console.WriteLine(
                            "Cannot find child " + child_id.ToString() +
                            " for parent " + parent_id.ToString());

                    if (parent_info == null)
                        Console.WriteLine(
                            "Cannot find parent " + parent_id.ToString() +
                            " for child " + child_id.ToString());

                    if ((child_info != null) && (parent_info != null))
                    {
                        parent_info.Children.Add(child_info);
                        child_info.Parent = parent_info;
                    }
                }

            }
            catch(Exception ex)
            {
                logger.Error(ex,$"ProcessUtils/Initialise: Exception (re)initialising the process information to figure out process hierarchy");
            }

        }

        public static List<Process> FindChildProcesses(Process parentProcess)
        {
            List<Process> childProcesses = new List<Process>() { };

            try
            {
                int parentId = parentProcess.Id;
                // TODO: We *possibly* could walk the tree to find the program hierarchy, to get the full list of the 
                // process tree, but this seems like the best way at this stage. I'm expecting I'll find an edge case in the future
                // that requires some sort of modification, but this is working well in 2 days of testing so far!
                if (allProcessInfosDict.ContainsKey(parentId))
                {
                    foreach (ProcessInfo childProcess in allProcessInfosDict[parentId].Children)
                    {
                        childProcesses.Add(childProcess.TheProcess);
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"ProcessUtils/FindChildProcesses: Exception finding the child processes of the parentProcess");
            }
            
            return childProcesses;
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

        public static ProcessPriorityClass TranslatePriorityToClass(ProcessPriority  processPriorityClass)
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

        public static ProcessCreationFlags TranslatePriorityClassToFlags(ProcessPriorityClass processPriorityClass)
        {
            ProcessCreationFlags wantedPriorityClass = ProcessCreationFlags.NORMAL_PRIORITY_CLASS;
            switch (processPriorityClass)
            {
                case ProcessPriorityClass.High:
                    wantedPriorityClass = ProcessCreationFlags.HIGH_PRIORITY_CLASS;
                    break;
                case ProcessPriorityClass.AboveNormal:
                    wantedPriorityClass = ProcessCreationFlags.ABOVE_NORMAL_PRIORITY_CLASS;
                    break;
                case ProcessPriorityClass.Normal:
                    wantedPriorityClass = ProcessCreationFlags.NORMAL_PRIORITY_CLASS;
                    break;
                case ProcessPriorityClass.BelowNormal:
                    wantedPriorityClass = ProcessCreationFlags.BELOW_NORMAL_PRIORITY_CLASS;
                    break;
                case ProcessPriorityClass.Idle:
                    wantedPriorityClass = ProcessCreationFlags.IDLE_PRIORITY_CLASS;
                    break;
                default:
                    wantedPriorityClass = ProcessCreationFlags.NORMAL_PRIORITY_CLASS;
                    break;
            }
            return wantedPriorityClass;
        }

        public static bool LaunchProcessWithPriority(string exeName, string cmdLine, ProcessPriorityClass priorityClass, out uint PID)
        {
            ProcessCreationFlags processFlags = TranslatePriorityClassToFlags(priorityClass);

            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            bool success = NativeMethods.CreateProcess(exeName, cmdLine, IntPtr.Zero, IntPtr.Zero, false, processFlags, IntPtr.Zero, null, ref si, out pi);
            ThreadHandle = pi.hThread;
            PID = pi.dwProcessId;

            return success;
        }

        public static void ResumeProcess()
        {
            NativeMethods.ResumeThread(ThreadHandle);
        }
    }       
}
