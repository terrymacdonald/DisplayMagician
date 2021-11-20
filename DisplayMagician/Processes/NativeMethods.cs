using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DisplayMagician.Processes
{
    /// <summary>
    /// This class is a static class containing definitions for system methods that are used in various places in MediaPortal.
    /// </summary>
    public static class NativeMethods
    {
        #region LoadLibraryEx Flags

        public const uint LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100;
        public const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;

        #endregion

        /// <summary>
        /// Retrieves a module handle for the specified module. The module must have been loaded by the calling process.
        /// </summary>
        /// <param name="lpModuleName">The name of the loaded module (either a .dll or .exe file).</param>
        /// <returns>If the function succeeds, the return value is a handle to the module.<br></br><br>If the function fails, the return value is NULL. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandle", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// The GetProcAddress function retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).
        /// </summary>
        /// <param name="hModule">Handle to the DLL module that contains the function or variable. The LoadLibrary or GetModuleHandle function returns this handle.</param>
        /// <param name="lpProcName">Pointer to a null-terminated string containing the function or variable name, or the function's ordinal value. If this parameter is an ordinal value, it must be in the low-order word; the high-order word must be zero.</param>
        /// <returns>If the function succeeds, the return value is the address of the exported function or variable.<br></br><br>If the function fails, the return value is NULL. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        /// <summary>
        /// Determines whether the specified process is running under WOW64.
        /// </summary>
        /// <param name="hProcess">A handle to the process. The handle must have the PROCESS_QUERY_INFORMATION or PROCESS_QUERY_LIMITED_INFORMATION access right.</param>
        /// <param name="isWow64Process">A pointer to a value that is set to TRUE if the process is running under WOW64. If the process is running under 32-bit Windows, the value is set to FALSE. If the process is a 64-bit application running under 64-bit Windows, the value is also set to FALSE.</param>
        /// <returns>If the function succeeds, the return value is nonzero.<br></br><br>If the function fails, the return value is zero. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
        [DllImport("kernel32.dll", EntryPoint = "IsWow64Process")]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool isWow64Process);

        /// <summary>
        /// The LoadLibrary function maps the specified executable module into the address space of the calling process.
        /// </summary>
        /// <param name="lpLibFileName">Pointer to a null-terminated string that names the executable module (either a .dll or .exe file). The name specified is the file name of the module and is not related to the name stored in the library module itself, as specified by the LIBRARY keyword in the module-definition (.def) file.</param>
        /// <returns>If the function succeeds, the return value is a handle to the module.<br></br><br>If the function fails, the return value is NULL. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryA", CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary(string lpLibFileName);

        /// <summary>
        /// Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded.
        /// </summary>
        /// <param name="lpFileName">Pointer to a null-terminated string that names the executable module (either a .dll or .exe file). The name specified is the file name of the module and is not related to the name stored in the library module itself, as specified by the LIBRARY keyword in the module-definition (.def) file.</param>
        /// <param name="hFile">This parameter is reserved for future use. It must be IntPtr.Zero.</param>
        /// <param name="dwFlags">The action to be taken when loading the module. If no flags are specified, the behavior of this function is identical to that of the <see cref="LoadLibrary"/> function.</param>
        /// <returns>If the function succeeds, the return value is a handle to the module.<br/>If the function fails, the return value is NULL. To get extended error information, call Marshal.GetLastWin32Error.</returns>
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        /// <summary>
        /// The FreeLibrary function decrements the reference count of the loaded dynamic-link library (DLL). When the reference count reaches zero, the module is unmapped from the address space of the calling process and the handle is no longer valid.
        /// </summary>
        /// <param name="hLibModule">Handle to the loaded DLL module. The LoadLibrary or GetModuleHandle function returns this handle.</param>
        /// <returns>If the function succeeds, the return value is nonzero.<br></br><br>If the function fails, the return value is zero. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary", CharSet = CharSet.Ansi)]
        public static extern int FreeLibrary(IntPtr hLibModule);

        // Group type enum
        public enum SecurityImpersonationLevel
        {
            Anonymous = 0,
            Identification = 1,
            Impersonation = 2,
            Delegation = 3
        }

        public enum LogonType
        {
            /// <summary>
            /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on  
            /// by a terminal server, remote shell, or similar process.
            /// This logon type has the additional expense of caching logon information for disconnected operations;
            /// therefore, it is inappropriate for some client/server applications,
            /// such as a mail server.
            /// </summary>
            Interactive = 2,

            /// <summary>
            /// This logon type is intended for high performance servers to authenticate plaintext passwords.

            /// The LogonUser function does not cache credentials for this logon type.
            /// </summary>
            Network = 3,

            /// <summary>
            /// This logon type is intended for batch servers, where processes may be executing on behalf of a user without
            /// their direct intervention. This type is also for higher performance servers that process many plaintext
            /// authentication attempts at a time, such as mail or Web servers.
            /// The LogonUser function does not cache credentials for this logon type.
            /// </summary>
            Batch = 4,

            /// <summary>
            /// Indicates a service-type logon. The account provided must have the service privilege enabled.
            /// </summary>
            Service = 5,

            /// <summary>
            /// This logon type is for GINA DLLs that log on users who will be interactively using the computer.
            /// This logon type can generate a unique audit record that shows when the workstation was unlocked.
            /// </summary>
            Unlock = 7,

            /// <summary>
            /// This logon type preserves the name and password in the authentication package, which allows the server to make
            /// connections to other network servers while impersonating the client. A server can accept plaintext credentials
            /// from a client, call LogonUser, verify that the user can access the system across the network, and still
            /// communicate with other servers.
            /// NOTE: Windows NT:  This value is not supported.
            /// </summary>
            NetworkCleartext = 8,

            /// <summary>
            /// This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
            /// The new logon session has the same local identifier but uses different credentials for other network connections.
            /// NOTE: This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
            /// NOTE: Windows NT:  This value is not supported.
            /// </summary>
            NewCredentials = 9,
        }

        public enum LogonProvider
        {
            /// <summary>
            /// Use the standard logon provider for the system.
            /// The default security provider is negotiate, unless you pass NULL for the domain name and the user name
            /// is not in UPN format. In this case, the default provider is NTLM.
            /// NOTE: Windows 2000/NT:   The default security provider is NTLM.
            /// </summary>
            Default = 0,
            WinNt35 = 1,
            WinNt40 = 2,
            WinNt50 = 3
        }

        [Flags]
        public enum TokenAccess
        {
            StandardRightsRequired = 0x000F0000,
            StandardRightsRead = 0x00020000,
            AssignPrimary = 0x0001,
            Duplicate = 0x0002,
            Impersonate = 0x0004,
            Query = 0x0008,
            QuerySource = 0x0010,
            AdjustPrivileges = 0x0020,
            AdjustGroups = 0x0040,
            AdjustDefault = 0x0080,
            AdjustSessionId = 0x0100,
            Read = (StandardRightsRead | Query),
            AllAccess = (StandardRightsRequired | AssignPrimary | Duplicate | Impersonate | Query | QuerySource | AdjustPrivileges | AdjustGroups | AdjustDefault | AdjustSessionId)
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessInformation
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class StartupInfo
        {
            public int cb;
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
            public SafeFileHandle hStdInput;
            public SafeFileHandle hStdOutput;
            public SafeFileHandle hStdError;

            public StartupInfo()
            {
                cb = Marshal.SizeOf(typeof(StartupInfo));
                hStdInput = new SafeFileHandle(IntPtr.Zero, false);
                hStdOutput = new SafeFileHandle(IntPtr.Zero, false);
                hStdError = new SafeFileHandle(IntPtr.Zero, false);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SecurityAttributes
        {
            public uint nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
            public SecurityAttributes()
            {
                nLength = (uint)Marshal.SizeOf(typeof(SecurityAttributes));
                lpSecurityDescriptor = IntPtr.Zero;
            }
        }

        public enum TokenType
        {
            Primary = 1,
            Impersonation
        }

        [Flags]
        public enum CreateProcessFlags : uint
        {
            None = 0,
            CreateBreakawayFromJob = 0x01000000,
            CreateDefaultErrorMode = 0x04000000,
            CreateNewConsole = 0x00000010,
            CreateNewProcessGroup = 0x00000200,
            CreateNoWindow = 0x08000000,
            CreateProtectedProcess = 0x00040000,
            CreatePreserveCodeAuthzLevel = 0x02000000,
            CreateSeparateWowVdm = 0x00000800,
            CreateSharedWowVdm = 0x00001000,
            CreateSuspended = 0x00000004,
            CreateUnicodeEnvironment = 0x00000400,
            DebugOnlyThisProcess = 0x00000002,
            DebugProcess = 0x00000001,
            DetachedProcess = 0x00000008,
            ExtendedStartupInfoPresent = 0x00080000,
            InheritParentAffinity = 0x00010000
        }

        [Flags]
        public enum ProcessAccess
        {
            AllAccess = CreateThread | DuplicateHandle | QueryInformation | SetInformation | Terminate | VmOperation | VmRead | VmWrite | Synchronize,
            CreateThread = 0x2,
            DuplicateHandle = 0x40,
            QueryInformation = 0x400,
            SetInformation = 0x200,
            Terminate = 0x1,
            VmOperation = 0x8,
            VmRead = 0x10,
            VmWrite = 0x20,
            Synchronize = 0x100000
        }

        // Obtains user token
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string username, string domain, string password, LogonType dwLogonType, LogonProvider dwLogonProvider, out IntPtr phToken);

        // Closes open handles returned by LogonUser
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetHandleInformation(SafeFileHandle hObject, int dwMask, uint dwFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeFileHandle GetStdHandle(int nStdHandle);

        // Creates duplicate token handle.
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr existingTokenHandle, SecurityImpersonationLevel impersonationLevel, ref IntPtr duplicateTokenHandle);

        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx", SetLastError = true)]
        public static extern bool DuplicateTokenEx(
            IntPtr hExistingToken,
            TokenAccess dwDesiredAccess,
            SecurityAttributes lpThreadAttributes,
            SecurityImpersonationLevel impersonationLevel,
            TokenType dwTokenType,
            out IntPtr phNewToken);

        [DllImport("advapi32")]
        public static extern bool OpenProcessToken(
            IntPtr processHandle, // handle to process
            TokenAccess desiredAccess, // desired access to process
            ref IntPtr tokenHandle // handle to open access token
        );

        [DllImport("advapi32.DLL")]
        public static extern bool ImpersonateLoggedOnUser(IntPtr hToken); // handle to token for logged-on user

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CreateProcessAsUserW(IntPtr token, [MarshalAs(UnmanagedType.LPTStr)] string lpApplicationName, [MarshalAs(UnmanagedType.LPTStr)] string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, CreateProcessFlags dwCreationFlags, IntPtr lpEnvironment, [MarshalAs(UnmanagedType.LPTStr)] string lpCurrentDirectory, [In] StartupInfo lpStartupInfo, out ProcessInformation lpProcessInformation);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetPriorityClass(IntPtr handle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetPriorityClass(IntPtr handle, uint priorityClass);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, SecurityAttributes lpPipeAttributes, int nSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, SafeFileHandle hSourceHandle, IntPtr hTargetProcess, out SafeFileHandle targetHandle, int dwDesiredAccess, bool bInheritHandle, int dwOptions);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [StructLayout(LayoutKind.Sequential)]
        public struct Message
        {
            public IntPtr hwnd;
            public int message;
            public IntPtr wParam;
            public IntPtr lParam;
            public int time;
            public int pt_x;
            public int pt_y;
        }

        public delegate int WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WindowClass
        {
            public uint style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
        }

        #region WndProc message constants

        public const int WM_QUIT = 0x0012;
        public const int WM_POWERBROADCAST = 0x0218;
        public const int BROADCAST_QUERY_DENY = 0x424D5144;

        #endregion

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int RegisterClass(ref WindowClass windowClass);

        [DllImport("user32.dll")]
        public static extern int DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern bool GetMessageA([In, Out] ref Message message, IntPtr hWnd, int uMsgFilterMin, int uMsgFilterMax);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool TranslateMessage([In, Out] ref Message message);

        [DllImport("user32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern IntPtr DispatchMessageA([In] ref Message message);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateWindowEx(uint dwExStyle, string className, string windowName, uint dwStyle, int x, int y,
                                                    int width, int height, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        #region FindWindow / BringToFront

        public enum ShowWindowFlags
        {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActivate = 7,
            ShowNotActivated = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        #region Interop

        [DllImport("user32")]
        private static extern bool AttachThreadInput(uint nThreadId, int nThreadIdTo, bool bAttach);

        [DllImport("user32")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int unused);

        [DllImport("user32")]
        private static extern bool SetFocus(IntPtr hWnd);

        [DllImport("user32", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr _hwnd);

        [DllImport("user32", SetLastError = true)]
        public static extern uint ShowWindow(IntPtr hwnd, ShowWindowFlags showCommand);

        #endregion

        public static bool SetForegroundWindow(IntPtr window, bool force)
        {
            IntPtr windowForeground = GetForegroundWindow();

            if (window == windowForeground)
            {
                return true;
            }

            bool setAttempt = SetForegroundWindow(window);

            if (force == false || setAttempt)
            {
                return setAttempt;
            }

            if (windowForeground == IntPtr.Zero)
            {
                return false;
            }

            // if we don't attach successfully to the windows thread then we're out of options
            int processId;
            int fgWindowPID = GetWindowThreadProcessId(windowForeground, out processId);

            if (fgWindowPID == -1)
            {
                return false;
            }

            // If we don't attach successfully to the windows thread then we're out of options
            uint curThreadID = GetCurrentThreadId();
            bool attached = AttachThreadInput(curThreadID, fgWindowPID, true);
            int lastError = Marshal.GetLastWin32Error();

            if (!attached)
            {
                return false;
            }

            SetForegroundWindow(window);
            BringWindowToTop(window);
            SetFocus(window);

            AttachThreadInput(curThreadID, fgWindowPID, false);

            // we've done all that we can so base our return value on whether we have succeeded or not
            return (GetForegroundWindow() == window);
        }

        #endregion FindWindow / BringToFront
    }
}
