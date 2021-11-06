using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DisplayMagician
{
    public class ProcessCreator
    {

        [Flags]
        public enum PROCESS_CREATION_FLAGS : uint
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

        public static bool CreateProcessWithPriority(string exeName, string cmdLine, ProcessPriorityClass priorityClass, out PROCESS_INFORMATION processInfo)
        {
            PROCESS_CREATION_FLAGS processFlags = TranslatePriorityClassToFlags(priorityClass) | PROCESS_CREATION_FLAGS.CREATE_SUSPENDED;
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
                // This is 
            }
            processInfo = pInfo;

            return true;
        }

        public static void ResumeProcess(IntPtr threadHandle)
        {
            ResumeThread(threadHandle);
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

    }
}       

