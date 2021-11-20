using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace DisplayMagician.Processes
{
    public class ImpersonationProcess : System.Diagnostics.Process
    {
        private const int HANDLE_FLAG_INHERIT = 1;

        private const uint STARTF_USESHOWWINDOW = 0x00000001;
        private const uint STARTF_USESTDHANDLES = 0x00000100;

        private const int STD_INPUT_HANDLE = -10;
        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;

        private const int SW_HIDE = 0;
        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;
        private const int SW_SHOW = 5;

        private SafeFileHandle _stdinReadHandle;
        private SafeFileHandle _stdinWriteHandle;
        private SafeFileHandle _stdoutWriteHandle;
        private SafeFileHandle _stderrWriteHandle;
        private SafeFileHandle _stdoutReadHandle;
        private SafeFileHandle _stderrReadHandle;
        private NativeMethods.ProcessInformation _processInformation;

        private string GetCommandLine()
        {
            StringBuilder result = new StringBuilder();
            string applicationName = StartInfo.FileName.Trim();
            string arguments = StartInfo.Arguments;

            bool applicationNameIsQuoted = applicationName.StartsWith("\"") && applicationName.EndsWith("\"");

            if (!applicationNameIsQuoted)
                result.Append("\"");

            result.Append(applicationName);

            if (!applicationNameIsQuoted)
                result.Append("\"");

            if (arguments.Length > 0)
            {
                result.Append(" ");
                result.Append(arguments);
            }
            return result.ToString();
        }

        public new void Kill()
        {
            IntPtr hProcess = IntPtr.Zero;
            try
            {
                int id = Id;
                hProcess = NativeMethods.OpenProcess(NativeMethods.ProcessAccess.Terminate, false, id);
                if (hProcess == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "ImpersonationProcess: OpenProcess failed");
                NativeMethods.TerminateProcess(hProcess, 0);
            }
            finally
            {
                ImpersonationHelper.SafeCloseHandle(hProcess);
            }
        }

        public new ProcessPriorityClass PriorityClass
        {
            get { return (ProcessPriorityClass)NativeMethods.GetPriorityClass(_processInformation.hProcess); }
            set { NativeMethods.SetPriorityClass(_processInformation.hProcess, (uint)value); }
        }

        public new int ExitCode
        {
            get
            {
                uint exitCode;
                if (NativeMethods.GetExitCodeProcess(_processInformation.hProcess, out exitCode))
                    return (int)exitCode;
                return -1;
            }
        }

        public bool StartAsUser(string domain, string username, string password)
        {
            IntPtr userToken = IntPtr.Zero;
            IntPtr token = IntPtr.Zero;
            try
            {
                if (!NativeMethods.LogonUser(username, domain, password, NativeMethods.LogonType.Interactive, NativeMethods.LogonProvider.Default, out token))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), String.Format("ImpersonationProcess: LogonUser {0}\\{1} failed", domain, username));

                if (!NativeMethods.DuplicateTokenEx(token, NativeMethods.TokenAccess.AssignPrimary | NativeMethods.TokenAccess.Duplicate | NativeMethods.TokenAccess.Query, null, NativeMethods.SecurityImpersonationLevel.Impersonation, NativeMethods.TokenType.Primary, out userToken))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "ImpersonationProcess: DuplicateToken failed");

                return StartAsUser(userToken);
            }
            finally
            {
                ImpersonationHelper.SafeCloseHandle(token);
                ImpersonationHelper.SafeCloseHandle(userToken);
            }
        }

        public bool StartAsUser(IntPtr userToken)
        {
            _processInformation = new NativeMethods.ProcessInformation();
            NativeMethods.StartupInfo startupInfo = new NativeMethods.StartupInfo();
            switch (StartInfo.WindowStyle)
            {
                case ProcessWindowStyle.Hidden:
                    startupInfo.wShowWindow = SW_HIDE;
                    break;
                case ProcessWindowStyle.Maximized:
                    startupInfo.wShowWindow = SW_MAXIMIZE;
                    break;
                case ProcessWindowStyle.Minimized:
                    startupInfo.wShowWindow = SW_MINIMIZE;
                    break;
                case ProcessWindowStyle.Normal:
                    startupInfo.wShowWindow = SW_SHOW;
                    break;
            }
            CreateStandardPipe(out _stdinReadHandle, out _stdinWriteHandle, STD_INPUT_HANDLE, true, StartInfo.RedirectStandardInput);
            CreateStandardPipe(out _stdoutReadHandle, out _stdoutWriteHandle, STD_OUTPUT_HANDLE, false, StartInfo.RedirectStandardOutput);
            CreateStandardPipe(out _stderrReadHandle, out _stderrWriteHandle, STD_ERROR_HANDLE, false, StartInfo.RedirectStandardError);

            startupInfo.dwFlags = STARTF_USESTDHANDLES | STARTF_USESHOWWINDOW;
            startupInfo.hStdInput = _stdinReadHandle;
            startupInfo.hStdOutput = _stdoutWriteHandle;
            startupInfo.hStdError = _stderrWriteHandle;

            NativeMethods.CreateProcessFlags createFlags = NativeMethods.CreateProcessFlags.CreateNewConsole | NativeMethods.CreateProcessFlags.CreateNewProcessGroup | NativeMethods.CreateProcessFlags.CreateDefaultErrorMode;
            if (StartInfo.CreateNoWindow)
            {
                startupInfo.wShowWindow = SW_HIDE;
                createFlags |= NativeMethods.CreateProcessFlags.CreateNoWindow;
            }

            // Create process as user, fail hard if this is unsuccessful so it can be caught in EncoderUnit
            if (!NativeMethods.CreateProcessAsUserW(userToken, null, GetCommandLine(), IntPtr.Zero, IntPtr.Zero, true, createFlags, IntPtr.Zero, null, startupInfo, out _processInformation))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "ImpersonationProcess: CreateProcessAsUser failed");

            if (_processInformation.hThread != (IntPtr)(-1))
            {
                ImpersonationHelper.SafeCloseHandle(ref _processInformation.hThread);
                _processInformation.hThread = IntPtr.Zero;
            }

            if (StartInfo.RedirectStandardInput)
            {
                ImpersonationHelper.SafeCloseHandle(ref _stdinReadHandle);
                StreamWriter standardInput = new StreamWriter(new FileStream(_stdinWriteHandle, FileAccess.Write, 4096), Console.Out.Encoding) { AutoFlush = true };
                SetField("standardInput", standardInput);
            }

            if (StartInfo.RedirectStandardOutput)
            {
                ImpersonationHelper.SafeCloseHandle(ref _stdoutWriteHandle);
                StreamReader standardOutput = new StreamReader(new FileStream(_stdoutReadHandle, FileAccess.Read, 4096), StartInfo.StandardOutputEncoding);
                SetField("standardOutput", standardOutput);
            }

            if (StartInfo.RedirectStandardError)
            {
                ImpersonationHelper.SafeCloseHandle(ref _stderrWriteHandle);
                StreamReader standardError = new StreamReader(new FileStream(_stderrReadHandle, FileAccess.Read, 4096), StartInfo.StandardErrorEncoding);
                SetField("standardError", standardError);
            }

            // Workaround to get process handle as non-public SafeProcessHandle
            Assembly processAssembly = typeof(System.Diagnostics.Process).Assembly;
            Type processManager = processAssembly.GetType("System.Diagnostics.ProcessManager");
            object safeProcessHandle = processManager.InvokeMember("OpenProcess", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, this, new object[] { _processInformation.dwProcessId, 0x100000, false });

            InvokeMethod("SetProcessHandle", safeProcessHandle);
            InvokeMethod("SetProcessId", _processInformation.dwProcessId);

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            ImpersonationHelper.SafeCloseHandle(ref _processInformation.hProcess);
            ImpersonationHelper.SafeCloseHandle(ref _processInformation.hThread);
            base.Dispose(disposing);
        }

        private void CreateStandardPipe(out SafeFileHandle readHandle, out SafeFileHandle writeHandle, int standardHandle, bool isInput, bool redirect)
        {
            if (redirect)
            {
                NativeMethods.SecurityAttributes security = new NativeMethods.SecurityAttributes { bInheritHandle = true };

                bool success = NativeMethods.CreatePipe(out readHandle, out writeHandle, security, 4096);

                if (success)
                    success = NativeMethods.SetHandleInformation(isInput ? writeHandle : readHandle, HANDLE_FLAG_INHERIT, 0);

                if (!success)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "ImpersonationProcess: could not create standard pipe");
            }
            else
            {
                if (isInput)
                {
                    writeHandle = new SafeFileHandle(IntPtr.Zero, false);
                    readHandle = NativeMethods.GetStdHandle(standardHandle);
                }
                else
                {
                    readHandle = new SafeFileHandle(IntPtr.Zero, false);
                    writeHandle = NativeMethods.GetStdHandle(standardHandle);
                }
            }
        }

        private object InvokeMethod(string member, params object[] args)
        {
            return typeof(System.Diagnostics.Process).InvokeMember(member, BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, this, args);
        }

        private object SetField(string member, params object[] args)
        {
            return typeof(System.Diagnostics.Process).InvokeMember(member, BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance, null, this, args);
        }
    }
}
