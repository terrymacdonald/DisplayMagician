using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

// This file is taken from Soroush Falahati's amazing HeliosDisplayManagement software
// available at https://github.com/falahati/HeliosDisplayManagement

namespace DisplayMagicianShared.Windows
{
    /// <summary>
    ///     Represents a Restart Manager session. The Restart Manager enables all but the critical system services to be shut
    ///     down and restarted with aim of eliminate or reduce the number of system restarts that are required to complete an
    ///     installation or update.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class RestartManagerSession : IDisposable
    {
        public delegate void WriteStatusCallback(uint percentageCompleted);

        /// <summary>
        ///     Specifies the type of modification that is applied to restart or shutdown actions.
        /// </summary>
        public enum FilterAction : uint
        {
            /// <summary>
            ///     Prevents the restart of the specified application or service.
            /// </summary>
            RmNoRestart = 1,

            /// <summary>
            ///     Prevents the shut down and restart of the specified application or service.
            /// </summary>
            RmNoShutdown = 2
        }

        [Flags]
        public enum ShutdownType : uint
        {
            /// <summary>
            ///     Default behavior
            /// </summary>
            Normal = 0,

            /// <summary>
            ///     Force unresponsive applications and services to shut down after the timeout period. An application that does not
            ///     respond to a shutdown request is forced to shut down within 30 seconds. A service that does not respond to a
            ///     shutdown request is forced to shut down after 20 seconds.
            /// </summary>
            ForceShutdown = 0x1,

            /// <summary>
            ///     Shut down applications if and only if all the applications have been registered for restart using the
            ///     RegisterApplicationRestart function. If any processes or services cannot be restarted, then no processes or
            ///     services are shut down.
            /// </summary>
            ShutdownOnlyRegistered = 0x10
        }

        public RestartManagerSession()
        {
            SessionKey = Guid.NewGuid().ToString();
            var errorCode = StartSession(out var sessionHandle, 0, SessionKey);

            if (errorCode != 0)
            {
                throw new Win32Exception(errorCode);
            }

            Handle = sessionHandle;
        }

        public RestartManagerSession(string sessionKey)
        {
            SessionKey = sessionKey;
            var errorCode = JoinSession(out var sessionHandle, SessionKey);

            if (errorCode != 0)
            {
                throw new Win32Exception(errorCode);
            }

            Handle = sessionHandle;
        }

        private IntPtr Handle { get; }
        public string SessionKey { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        [DllImport("rstrtmgr", EntryPoint = "RmAddFilter", CharSet = CharSet.Auto)]
        // ReSharper disable once TooManyArguments
        private static extern int AddFilter(
            IntPtr sessionHandle,
            string fileName,
            UniqueProcess application,
            string serviceName,
            FilterAction filterAction);

        [DllImport("rstrtmgr", EntryPoint = "RmAddFilter", CharSet = CharSet.Auto)]
        // ReSharper disable once TooManyArguments
        private static extern int AddFilter(
            IntPtr sessionHandle,
            string fileName,
            IntPtr application,
            string serviceName,
            FilterAction filterAction);

        [DllImport("rstrtmgr", EntryPoint = "RmEndSession")]
        private static extern int EndSession(IntPtr sessionHandle);

        [DllImport("rstrtmgr", EntryPoint = "RmJoinSession", CharSet = CharSet.Auto)]
        private static extern int JoinSession(out IntPtr sessionHandle, string strSessionKey);

        [DllImport("rstrtmgr", EntryPoint = "RmRegisterResources", CharSet = CharSet.Auto)]
        // ReSharper disable once TooManyArguments
        private static extern int RegisterResources(
            IntPtr sessionHandle,
            uint numberOfFiles,
            string[] fileNames,
            uint numberOfApplications,
            UniqueProcess[] applications,
            uint numberOfServices,
            string[] serviceNames);

        [DllImport("rstrtmgr", EntryPoint = "RmRestart")]
        private static extern int Restart(IntPtr sessionHandle, int restartFlags, WriteStatusCallback statusCallback);

        [DllImport("rstrtmgr", EntryPoint = "RmShutdown")]
        private static extern int Shutdown(
            IntPtr sessionHandle,
            ShutdownType actionFlags,
            WriteStatusCallback statusCallback);

        [DllImport("rstrtmgr", EntryPoint = "RmStartSession", CharSet = CharSet.Auto)]
        private static extern int StartSession(out IntPtr sessionHandle, int sessionFlags, string strSessionKey);

        public void FilterProcess(Process process, FilterAction action)
        {
            var errorCode = AddFilter(Handle, null, UniqueProcess.FromProcess(process), null, action);

            if (errorCode != 0)
            {
                throw new Win32Exception(errorCode);
            }
        }

        public void FilterProcessFile(FileInfo file, FilterAction action)
        {
            var errorCode = AddFilter(Handle, file.FullName, IntPtr.Zero, null, action);

            if (errorCode != 0)
            {
                throw new Win32Exception(errorCode);
            }
        }

        public void FilterService(string serviceName, FilterAction action)
        {
            var errorCode = AddFilter(Handle, null, IntPtr.Zero, serviceName, action);

            if (errorCode != 0)
            {
                throw new Win32Exception(errorCode);
            }
        }

        public void RegisterProcess(params Process[] processes)
        {
            var errorCode = RegisterResources(Handle,
                0, new string[0],
                (uint)processes.Length, processes.Select(UniqueProcess.FromProcess).ToArray(),
                0, new string[0]);

            if (errorCode != 0)
            {
                throw new Win32Exception(errorCode);
            }
        }

        public void RegisterProcessFile(params FileInfo[] files)
        {
            var errorCode = RegisterResources(Handle,
                (uint)files.Length, files.Select(f => f.FullName).ToArray(),
                0, new UniqueProcess[0],
                0, new string[0]);

            if (errorCode != 0)
            {
                throw new Win32Exception(errorCode);
            }
        }

        public void RegisterService(params string[] serviceNames)
        {
            var errorCode = RegisterResources(Handle,
                0, new string[0],
                0, new UniqueProcess[0],
                (uint)serviceNames.Length, serviceNames);

            if (errorCode != 0)
            {
                throw new Win32Exception(errorCode);
            }
        }

        public void Restart(WriteStatusCallback statusCallback)
        {
            var errorCode = Restart(Handle, 0, statusCallback);

            if (errorCode != 0)
            {
                throw new Win32Exception(errorCode);
            }
        }

        public void Restart()
        {
            Restart(null);
        }

        public void Shutdown(ShutdownType shutdownType, WriteStatusCallback statusCallback)
        {
            var errorCode = Shutdown(Handle, shutdownType, statusCallback);

            if (errorCode != 0)
            {
                throw new Win32Exception(errorCode);
            }
        }

        public void Shutdown(ShutdownType shutdownType)
        {
            Shutdown(shutdownType, null);
        }

        private void ReleaseUnmanagedResources()
        {
            try
            {
                EndSession(Handle);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <inheritdoc />
        ~RestartManagerSession()
        {
            ReleaseUnmanagedResources();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileTime
        {
            private uint LowDateTime;
            private uint HighDateTime;

            public static FileTime FromDateTime(DateTime dateTime)
            {
                var ticks = dateTime.ToFileTime();

                return new FileTime
                {
                    HighDateTime = (uint)(ticks >> 32),
                    LowDateTime = (uint)(ticks & uint.MaxValue)
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UniqueProcess
        {
            private int ProcessId;
            private FileTime ProcessStartTime;

            public static UniqueProcess FromProcess(Process process)
            {
                return new UniqueProcess
                {
                    ProcessId = process.Id,
                    ProcessStartTime = FileTime.FromDateTime(process.StartTime)
                };
            }
        }

        public static bool RestartExplorer()
        {
            RestartManagerSession restartManager = new RestartManagerSession();
            try
            {                
                FileInfo explorerFileInfo = new FileInfo(@"C:\Windows\explorer.exe");
                restartManager.RegisterProcessFile(explorerFileInfo);
                restartManager.Shutdown(RestartManagerSession.ShutdownType.ForceShutdown);
                restartManager.Restart();
                restartManager.Dispose();
                return true;
            }
            catch (Win32Exception ex)
            {
                if (ex.ErrorCode == 776)
                {
                    SharedLogger.logger.Error(ex, $"WinLibrary/RestartExplorer: RestartManager was unable to restart Windows Explorer. ERROR_REQUEST_OUT_OF_SEQUENCE (779): This error value is returned if the RmRestart function is called with a valid session handle before calling the RmShutdown function.");
                }
                else if (ex.ErrorCode == 352)
                {
                    SharedLogger.logger.Error(ex, $"WinLibrary/RestartExplorer: RestartManager was unable to restart Windows Explorer. ERROR_FAIL_RESTART (352): One or more applications could not be restarted.");
                }
                else if (ex.ErrorCode == 121)
                {
                    SharedLogger.logger.Error(ex, $"WinLibrary/RestartExplorer: RestartManager was unable to restart Windows Explorer. ERROR_SEM_TIMEOUT (121): A Restart Manager function could not obtain a registry write mutex in the allotted time. A system restart is recommended because further use of the Restart Manager is likely to fail.");
                }
                else if (ex.ErrorCode == 1223)
                {
                    SharedLogger.logger.Error(ex, $"WinLibrary/RestartExplorer: RestartManager was unable to restart Windows Explorer. ERROR_CANCELLED (1223): This error value is returned by the RmRestart function when the request to cancel an operation is successful.");
                }
                else if (ex.ErrorCode == 160)
                {
                    SharedLogger.logger.Error(ex, $"WinLibrary/RestartExplorer: RestartManager was unable to restart Windows Explorer. ERROR_BAD_ARGUMENTS (160): One or more arguments are not correct. This error value is returned by the Restart Manager function if a NULL pointer or 0 is passed in a parameter that requires a non-null and non-zero value.");
                }
                else if (ex.ErrorCode == 29)
                {
                    SharedLogger.logger.Error(ex, $"WinLibrary/RestartExplorer: RestartManager was unable to restart Windows Explorer. ERROR_WRITE_FAULT (29): An operation was unable to read or write to the registry.");
                }
                else if (ex.ErrorCode == 14)
                {
                    SharedLogger.logger.Error(ex, $"WinLibrary/RestartExplorer: RestartManager was unable to restart Windows Explorer. ERROR_OUTOFMEMORY (14): A Restart Manager operation could not complete because not enough memory was available.");
                }
                else if (ex.ErrorCode == 6)
                {
                    SharedLogger.logger.Error(ex, $"WinLibrary/RestartExplorer: RestartManager was unable to restart Windows Explorer. ERROR_INVALID_HANDLE (6): No Restart Manager session exists for the handle supplied.");
                }
                else
                {
                    SharedLogger.logger.Error(ex, $"WinLibrary/RestartExplorer: RestartManager was unable to restart Windows Explorer. ErrorCode = {ex.ErrorCode}.");
                }
                restartManager.Restart();
                restartManager.Dispose();

                return false;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"WinLibrary/RestartExplorer: General exception when trying to restart Windows Explorer!");
                restartManager.Dispose();
                return false;
            }

        }
    }
}