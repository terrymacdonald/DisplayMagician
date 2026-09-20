using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DisplayMagician.Processes
{
    /// <summary>
    /// For calls to the Windows API, this class should be used instead of directly using
    /// the underlaying system's API. This class hides the concrete underlaying system and will
    /// use different system functions depending on the underlaying system.
    /// </summary>
    public static class WindowsAPI
    {
        #region Windows API

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        /// <summary>
        /// Handle to send a message to all top-level windows in the system, including disabled or invisible unowned windows,
        /// overlapped windows, and pop-up windows; but the message is not sent to child windows.
        /// </summary>
        public const int HWND_BROADCAST = 0xffff;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [DllImport("powrprof.dll", SetLastError = true)]
        public static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, bool uiParam, IntPtr pvParam, uint fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref bool result, uint fWinIni);

        #region ExitWindowsEx

        // Source: http://ithoughthecamewithyou.com/post/Reboot-computer-in-C-NET.aspx
        public static void ExitWindowsEx(EXIT_WINDOWS exitMode)
        {
            IntPtr tokenHandle = IntPtr.Zero;

            try
            {
                // Get process token
                if (!OpenProcessToken(System.Diagnostics.Process.GetCurrentProcess().Handle,
                    TOKEN_QUERY | TOKEN_ADJUST_PRIVILEGES,
                    out tokenHandle))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(),
                        "Failed to open process token handle");
                }

                // Lookup the shutdown privilege
                TOKEN_PRIVILEGES tokenPrivs = new TOKEN_PRIVILEGES();
                tokenPrivs.PrivilegeCount = 1;
                tokenPrivs.Privileges = new LUID_AND_ATTRIBUTES[1];
                tokenPrivs.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

                if (!LookupPrivilegeValue(null,
                    SE_SHUTDOWN_NAME,
                    out tokenPrivs.Privileges[0].Luid))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(),
                        "Failed to open lookup shutdown privilege");
                }

                // Add the shutdown privilege to the process token
                if (!AdjustTokenPrivileges(tokenHandle,
                    false,
                    ref tokenPrivs,
                    0,
                    IntPtr.Zero,
                    IntPtr.Zero))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(),
                        "Failed to adjust process token privileges");
                }

                // Perform the exit operation
                if (!ExitWindowsEx(exitMode,
                        ShutdownReason.MajorApplication |
                ShutdownReason.MinorInstallation |
                ShutdownReason.FlagPlanned))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(),
                        String.Format("Failed to exit the system ({0})", exitMode));
                }
            }
            finally
            {
                // Close the process token
                if (tokenHandle != IntPtr.Zero)
                {
                    CloseHandle(tokenHandle);
                }
            }
        }

        // http://msdn.microsoft.com/library/windows/desktop/aa376868

        [FlagsAttribute]
        public enum EXIT_WINDOWS : uint
        {
            // ONE of the following is required

            EWX_LOGOFF = 0,
            EWX_POWEROFF = 0x00000008,
            EWX_REBOOT = 0x00000002,
            EWX_RESTARTAPPS = 0x00000040,
            EWX_SHUTDOWN = 0x00000001,

            // ONE of the following is optional

            /// <summary>
            /// Beginning with Windows 8: You can prepare the system for a faster startup by combining the EWX_HYBRID_SHUTDOWN flag with the EWX_SHUTDOWN flag.
            /// </summary>
            EWX_HYBRID_SHUTDOWN = 0x00400000,

            /// <summary>
            /// This flag has no effect if terminal services is enabled. Otherwise, the system does not send the WM_QUERYENDSESSION message.
            /// This can cause applications to lose data. Therefore, you should only use this flag in an emergency.
            /// </summary>
            EWX_FORCE = 0x00000004,

            /// <summary>
            /// Forces processes to terminate if they do not respond to the WM_QUERYENDSESSION or WM_ENDSESSION message within the timeout interval.
            /// For more information, see the http://msdn.microsoft.com/library/windows/desktop/aa376868.
            /// </summary>
            EWX_FORCEIFHUNG = 0x00000010,
        }

        [FlagsAttribute]
        private enum ShutdownReason : uint
        {
            MajorApplication = 0x00040000,
            MajorHardware = 0x00010000,
            MajorLegacyApi = 0x00070000,
            MajorOperatingSystem = 0x00020000,
            MajorOther = 0x00000000,
            MajorPower = 0x00060000,
            MajorSoftware = 0x00030000,
            MajorSystem = 0x00050000,

            MinorBlueScreen = 0x0000000F,
            MinorCordUnplugged = 0x0000000b,
            MinorDisk = 0x00000007,
            MinorEnvironment = 0x0000000c,
            MinorHardwareDriver = 0x0000000d,
            MinorHotfix = 0x00000011,
            MinorHung = 0x00000005,
            MinorInstallation = 0x00000002,
            MinorMaintenance = 0x00000001,
            MinorMMC = 0x00000019,
            MinorNetworkConnectivity = 0x00000014,
            MinorNetworkCard = 0x00000009,
            MinorOther = 0x00000000,
            MinorOtherDriver = 0x0000000e,
            MinorPowerSupply = 0x0000000a,
            MinorProcessor = 0x00000008,
            MinorReconfig = 0x00000004,
            MinorSecurity = 0x00000013,
            MinorSecurityFix = 0x00000012,
            MinorSecurityFixUninstall = 0x00000018,
            MinorServicePack = 0x00000010,
            MinorServicePackUninstall = 0x00000016,
            MinorTermSrv = 0x00000020,
            MinorUnstable = 0x00000006,
            MinorUpgrade = 0x00000003,
            MinorWMI = 0x00000015,

            FlagUserDefined = 0x40000000,
            FlagPlanned = 0x80000000
        }

        // Everything from here on is from pinvoke.net & 

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public UInt32 Attributes;
        }

        private struct TOKEN_PRIVILEGES
        {
            public UInt32 PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public LUID_AND_ATTRIBUTES[] Privileges;
        }

        private const UInt32 TOKEN_QUERY = 0x0008;
        private const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const UInt32 SE_PRIVILEGE_ENABLED = 0x00000002;
        private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

        // Note: http://msdn.microsoft.com/library/windows/desktop/aa376873
        // The ExitWindowsEx function returns as soon as it has initiated the shutdown process.
        // The shutdown or logoff then proceeds asynchronously.
        // The function is designed to stop all processes in the caller's logon session.
        // Therefore, if you are not the interactive user, the function can succeed without actually shutting down the computer.
        // If you are not the interactive user, use the InitiateSystemShutdown or InitiateSystemShutdownEx function.
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ExitWindowsEx(EXIT_WINDOWS uFlags, ShutdownReason dwReason);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle,
                                                    UInt32 DesiredAccess,
                                                    out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LookupPrivilegeValue(string lpSystemName,
                                                        string lpName,
                                                        out LUID lpLuid);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
                                                         [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
                                                         ref TOKEN_PRIVILEGES NewState,
                                                         UInt32 Zero,
                                                         IntPtr Null1,
                                                         IntPtr Null2);

        #endregion

        /// <summary>
        /// Places (posts) a message in the message queue associated with the thread that created the specified window and returns
        /// without waiting for the thread to process the message.
        /// </summary>
        /// <param name="hwnd">A handle to the window whose window procedure is to receive the message.</param>
        /// <param name="msg">The message to be posted.</param>
        /// <param name="wparam">Additional message-specific information.</param>
        /// <param name="lparam">Additional message-specific information.</param>
        /// <returns><c>true</c>, if the function succeeds, else <c>false</c>.</returns>
        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        /// <summary>
        /// Registers a window message.
        /// </summary>
        /// <param name="lpString">Name of the message to be registered.</param>
        /// <returns>If the message is successfully registered, the return value is a message identifier in the range 0xC000 through 0xFFFF.
        /// If the function fails, the return value is zero. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</returns>
        [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageW")]
        public static extern uint RegisterWindowMessage([MarshalAs(UnmanagedType.LPWStr)] string lpString);

        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", SetLastError = true)]
        public static extern IntPtr CreateWindowEx(int dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
                               [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, int dwStyle, int x, int y,
                               int nWidth, int nHeight, uint hWndParent, int hMenu, int hInstance,
                               int lpParam);

        #endregion

        public const string AUTOSTART_REGISTRY_KEY = @"Software\Microsoft\Windows\Currentversion\Run";

        public const int S_OK = 0x0;
        public const int S_FALSE = 0x1;

        public const int MAX_PATH = 260;

        public const uint SPI_GETSCREENSAVEACTIVE = 0x0010;
        public const uint SPI_SETSCREENSAVEACTIVE = 0x0011;

        public static bool ScreenSaverEnabled
        {
            get
            {
                bool result = false;
                SystemParametersInfo(SPI_GETSCREENSAVEACTIVE, 0, ref result, 0);
                return result;
            }
            set { SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, value, IntPtr.Zero, 0); }
        }

        /// <summary>
        /// Returns a string which contains the name and version of the operating system.
        /// </summary>
        /// <returns>Operating system name and version.</returns>
        public static string GetOsVersionString()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Platform + "/" + os.Version;
        }

        /// <summary>
        /// Returns the path of the given system's special folder.
        /// <remarks>
        /// If the requested special folder is an user folder, and the caller operates as local service, this method won't resolve the folder.
        /// </remarks>
        /// </summary>
        /// <param name="folder">Folder to retrieve.</param>
        /// <param name="folderPath">Will be set to the folder path if the result value is <c>true</c>.</param>
        /// <returns><c>true</c>, if the specified special folder could be retrieved. Else <c>false</c>
        /// will be returned.</returns>
        public static bool GetSpecialFolder(Environment.SpecialFolder folder, out string folderPath)
        {
            folderPath = null;
            switch (folder)
            {
                case Environment.SpecialFolder.MyMusic:
                case Environment.SpecialFolder.MyPictures:
                case Environment.SpecialFolder.MyVideos:
                    folderPath = Environment.GetFolderPath(folder);
                    return !string.IsNullOrWhiteSpace(folderPath);
                default:
                    throw new NotImplementedException(string.Format("The handling for special folder '{0}' isn't implemented yet", folder));
            }
        }

       /* /// <summary>
        /// Adds the application with the specified <paramref name="applicationPath"/> to the autostart
        /// registry key. The application will be automatically started the next system startup.
        /// </summary>
        /// <param name="applicationPath">Path of the application to be auto-started.</param>
        /// <param name="registerName">The name used in the registry as key for the autostart value.</param>
        /// <param name="user">If set to <c>true</c>, the autostart application will be added to the HCKU
        /// registry hive, else it will be added to the HKLM hive.</param>
        /// <exception cref="EnvironmentException">If the appropriate registry key cannot accessed.</exception>
        public static void AddAutostartApplication(string applicationPath, string registerName, bool user)
        {
            RegistryKey root = user ? Registry.CurrentUser : Registry.LocalMachine;
            // open RegisteryKey with write access
            using (RegistryKey key = root.OpenSubKey(AUTOSTART_REGISTRY_KEY, true))
            {
                if (key == null)
                    throw new EnvironmentException(@"Unable to access/create registry key '{0}\{1}'",
                        user ? "HKCU" : "HKLM", AUTOSTART_REGISTRY_KEY);
                key.SetValue(registerName, applicationPath, RegistryValueKind.ExpandString);
            }
        }

        /// <summary>
        /// Removes an application from the autostart registry key.
        /// </summary>
        /// <param name="registerName">The name used in the registry as key for the autostart value.</param>
        /// <param name="user">If set to <c>true</c>, the autostart application will be removed from the HCKU
        /// registry hive, else it will be removed from the HKLM hive.</param>
        /// <exception cref="EnvironmentException">If the appropriate registry key cannot accessed.</exception>
        public static void RemoveAutostartApplication(string registerName, bool user)
        {
            RegistryKey root = user ? Registry.CurrentUser : Registry.LocalMachine;
            // open RegisteryKey with write access
            using (RegistryKey key = root.OpenSubKey(AUTOSTART_REGISTRY_KEY, true))
            {
                if (key == null)
                    throw new EnvironmentException(@"Unable to access registry key '{0}\{1}'",
                        user ? "HKCU" : "HKLM", AUTOSTART_REGISTRY_KEY);
                key.DeleteValue(registerName, false);
            }
        }

        /// <summary>
        /// Returns the application path for the application registered to be autostarted with the
        /// specified <paramref name="registerName"/>.
        /// </summary>
        /// <param name="registerName">The name used in the registry as key for the autostart value.</param>
        /// <param name="user">If set to <c>true</c>, the autostart application path will be searched in the HCKU
        /// registry hive, else it will be searched in the HKLM hive.</param>
        /// <returns>Application path registered to be autostarted with the specified
        /// <paramref name="registerName"/>.</returns>
        public static string GetAutostartApplicationPath(string registerName, bool user)
        {
            RegistryKey root = user ? Registry.CurrentUser : Registry.LocalMachine;
            using (RegistryKey key = root.OpenSubKey(AUTOSTART_REGISTRY_KEY))
                return key == null ? null : key.GetValue(registerName) as string;
        }*/
    }
}
