using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
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

        //private static readonly Encoding CONSOLE_ENCODING = Encoding.UTF8;
        //private static readonly string CONSOLE_ENCODING_PREAMBLE = CONSOLE_ENCODING.GetString(CONSOLE_ENCODING.GetPreamble());

        //private const int INFINITE = -1;

        public static List<Process> StartProcess(string executable, string arguments, ProcessPriority processPriority, int startTimeout = 1, bool runAsAdministrator = false)
        {
            List<Process> returnedProcesses = new List<Process>();
            Process processCreated;
            //SafeAccessTokenHandle safeAccessTokenHandle;
            //bool returnValue = LogonUser(user, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeAccessTokenHandle);

            ProcessStartInfo psi = new ProcessStartInfo(executable, arguments)
            {
                CreateNoWindow = false,
                RedirectStandardOutput = false,
                WorkingDirectory = Path.GetDirectoryName(executable),
                LoadUserProfile = true
            };

            if (runAsAdministrator)
            {
                psi.Verb = "runas";
            }
            else
            {
                psi.Verb = "open";
            }

            if (executable.StartsWith("steam://", StringComparison.OrdinalIgnoreCase))
            {
                psi.UseShellExecute = true;
            }
            else
            {
                psi.UseShellExecute = false;
            }

            processCreated = new Process()
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            //var output = processCreated.StandardOutput.ReadToEnd();
            //var exitCode = processCreated.ExitCode;

            processCreated.Start();

            if (processCreated != null)
            {
                logger.Trace($"ProcessUtils/StartProcess: {executable} {arguments} has successfully been started by Process.Start");
                if (processCreated.Id > 0)
                {
                    try
                    {

                        processCreated.WaitForExit(4000);

                        if (processCreated.HasExited)
                        {

                            logger.Trace($"ProcessUtils/StartProcess: {executable} {arguments} has exited quickly. It is possible that it is a game or app launcher, so we'll try to see if it launched any child processes, and monitor them instead!");

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
                        }
                        else
                        {
                            ProcessPriorityClass wantedPriority = TranslatePriorityToClass(processPriority);
                            // If we're here then the process was created and hasn't exited!
                            try
                            {

                                if (processCreated.PriorityClass != wantedPriority)
                                {
                                    processCreated.PriorityClass = wantedPriority;
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
                        // Oops - something went wrong. We'll log it and have to move on :(
                        //process = null;
                        logger.Error(ex, $"ProcessUtils/StartProcess: Exception while trying to start {executable}. We were unable to start it.");
                    }

                }
            }
            else
            {
                logger.Warn($"ProcessUtils/StartProcess: {executable} {arguments} was unable to be started by Process.Start.");
            }    

            return returnedProcesses;
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
                    //logger.Trace($"ProcessUtils/ProcessExited: {process.Id} is still running as is has not exited yet.");
                    return false;
                }
            }
            catch (ArgumentException ex)
            {
                logger.Trace(ex, $"ProcessUtils/ProcessExited: {process.Id} is not running, and the process ID has expired. This means the process has finished!");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn(ex, $"ProcessUtils/ProcessExited: {process.Id} was not started by this process object. This likely means the process has finished!");
                return true;
            }
            catch (Exception ex)
            {
                logger.Trace(ex, $"ProcessUtils/ProcessExited: Exception when checking if {process.Id} is still running, so assuming the process has finished!");
                return true;
            }
        }

        public static bool ProcessExited(string executable)
        {            
            List<Process> wantedProcesses = Process.GetProcessesByName(GetProcessName(executable)).ToList();

            if (ProcessExited(wantedProcesses))
            {
                //logger.Trace($"ProcessUtils/ProcessExited4: All processes being monitored have exited, so no processes still running!");
                return true;
            }
            else
            {
                //logger.Trace($"ProcessUtils/ProcessExited4: At least one process is still running!");
                return false;
            }
        }

        public static bool ProcessExited(int processId)
        {
            Process process = Process.GetProcessById(processId);
            
            if (ProcessExited(process))
            {
                //logger.Trace($"ProcessUtils/ProcessExited3: Process with ID {processId} has exited, so no processes still running!");
                return true;
            }
            else
            {
                //logger.Trace($"ProcessUtils/ProcessExited3: Process with ID {processId} is still running!");
                return false;
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
                //logger.Trace($"ProcessUtils/ProcessExited2: All processes being monitored have exited, so no processes still running!");
                return true;
            }
            else
            {
                //logger.Trace($"ProcessUtils/ProcessExited2: {processClosedCount} processes out of {processes.Count} processes have exited. At least one process is still running!");
                return false;
            }
        }

        public static bool StopProcess(Process processToStop)
        {
            try
            {
                // Stop the process
                processToStop.CloseMainWindow();
                processToStop.WaitForExit(1000);
                if (!ProcessExited(processToStop))
                {
                    logger.Trace($"ProcessUtils/StopProcess: Process {processToStop.StartInfo.FileName} wouldn't stop cleanly. Forcing program close.");
                    processToStop.Kill();
                    processToStop.WaitForExit(5000);
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
                processToStop.WaitForExit(5000);
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
            finally
            {
                processToStop.Close();
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
                        string processFileName = processToStop.StartInfo.FileName;
                        logger.Debug($"ShortcutRepository/RunShortcut: Stopping process {processFileName }");
                        if (ProcessUtils.StopProcess(processToStop))
                        {
                            logger.Debug($"ShortcutRepository/RunShortcut: Successfully stopped process {processFileName }");
                        }
                        else
                        {
                            logger.Warn($"ShortcutRepository/RunShortcut: Failed to stop process {processFileName } after main executable or game was exited by the user.");
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
        /*public static bool TryExecute(string executable, string arguments, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal, int maxWaitMs = 1000)
        {
            List<Process> unused;
            return TryExecute(executable, arguments, out unused, priorityClass, maxWaitMs);
        }*/

        /*/// <summary>
        /// Executes the <paramref name="executable"/> and waits a maximum time of <paramref name="maxWaitMs"/> for completion. If the process doesn't end in 
        /// this time, it gets aborted. This helper method automatically decides if an impersonation should be done, depending on the current identity's 
        /// <see cref="TokenImpersonationLevel"/>.
        /// </summary>
        /// <param name="executable">Program to execute</param>
        /// <param name="arguments">Program arguments</param>
        /// <param name="priorityClass">Process priority</param>
        /// <param name="maxWaitMs">Maximum time to wait for completion</param>
        /// <returns><c>true</c> if process was executed and finished correctly</returns>
        public static bool TryExecute_AutoImpersonate(string executable, string arguments, out List<Process> processes, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal, int maxWaitMs = 1000)
        {
            bool result = false;
            if (IsImpersonated)
            {
                result = TryExecute_Impersonated(executable, arguments, out processes, priorityClass, maxWaitMs);
            }
            else
            {
                result = TryExecute(executable, arguments, out processes, priorityClass, maxWaitMs);
            }
            //process = processReturned;
            return result;
        }*/

        /*/// <summary>
        /// Executes the <paramref name="executable"/> and waits a maximum time of <paramref name="maxWaitMs"/> for completion. If the process doesn't end in 
        /// this time, it gets aborted. This method tries to impersonate the interactive user and run the process under its identity.
        /// </summary>
        /// <param name="executable">Program to execute</param>
        /// <param name="arguments">Program arguments</param>
        /// <param name="priorityClass">Process priority</param>
        /// <param name="maxWaitMs">Maximum time to wait for completion</param>
        /// <returns><c>true</c> if process was executed and finished correctly</returns>
        public static bool TryExecute_Impersonated(string executable, string arguments, out ImpersonationProcess processCreated, int maxWaitMs = 1000)
        {
            IntPtr userToken;
            if (!ImpersonationHelper.GetTokenByProcess(out userToken, true))
            {
                processCreated = null;
                return false;
            }
                
            try
            {
                //return TryExecute_Impersonated(executable, arguments, userToken, false, out unused, priorityClass, maxWaitMs);
                StringBuilder outputBuilder = new StringBuilder();
                //startedProcesses = new List<Process>();
                processCreated = new ImpersonationProcess { StartInfo = new ProcessStartInfo(executable, arguments) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = false } };
                *//*if (redirectInputOutput)
                {
                    // Set UTF-8 encoding for standard output.
                    process.StartInfo.StandardOutputEncoding = CONSOLE_ENCODING;
                    // Enable raising events because Process does not raise events by default.
                    process.EnableRaisingEvents = true;
                    // Attach the event handler for OutputDataReceived before starting the process.
                    process.OutputDataReceived += (sender, e) => outputBuilder.Append(e.Data);
                }*//*

                try
                {
                    processCreated.StartAsUser(userToken);
                    return true;
                }
                catch (ObjectDisposedException ex)
                {
                    logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. The process object has already been disposed.");
                    return false;
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
                    return false;
                }
                catch (Win32Exception ex)
                {
                    logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. There was an error in opening the associated file.");
                    return false;
                }
                catch (PlatformNotSupportedException ex)
                {
                    logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. Method not supported on operating systems without shell support such as Nano Server (.NET Core only).");
                    return false;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. Not sure what specific exception it is.");
                    return false;
                }

            }
            finally
            {
                ImpersonationHelper.SafeCloseHandle(userToken);
            }
        }

        /// <summary>
        /// Indicates if the current <see cref="WindowsIdentity"/> uses impersonation.
        /// </summary>
        private static bool IsImpersonated()
        {
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            return windowsIdentity != null && windowsIdentity.ImpersonationLevel == TokenImpersonationLevel.Impersonation;
        }
*/
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
        private static bool TryExecute(string executable, string arguments, out Process processCreated, bool runAsAdministrator = false, ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal, int maxWaitMs = 1000)
        {
            //StringBuilder outputBuilder = new StringBuilder();            
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
                        CreateNoWindow = false,
                        RedirectStandardOutput = false,
                        WorkingDirectory = Path.GetDirectoryName(executable)
                    };
                }
                else
                {
                    psi = new ProcessStartInfo(executable, arguments)
                    {
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        RedirectStandardOutput = false,
                        WorkingDirectory = Path.GetDirectoryName(executable)
                    };
                }
                
            }
            else
            {
                // Isn't a file (somethign like a url), or is a file but isn't an executable
                psi = new ProcessStartInfo(executable, arguments)
                {
                    UseShellExecute = true,
                    Verb = "Open",
                    CreateNoWindow = false,
                    RedirectStandardOutput = false,
                    WorkingDirectory = Path.GetDirectoryName(executable)
                };
            }

            processCreated = new Process { StartInfo = psi };
            
            /*if (redirectInputOutput)
            {
                // Set UTF-8 encoding for standard output.
                process.StartInfo.StandardOutputEncoding = CONSOLE_ENCODING;
                // Enable raising events because Process does not raise events by default.
                process.EnableRaisingEvents = true;
                // Attach the event handler for OutputDataReceived before starting the process.
                process.OutputDataReceived += (sender, e) => outputBuilder.Append(e.Data);
            }*/             

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
                return true;
            }
            catch (ObjectDisposedException ex)
            {
                logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. The process object has already been disposed.");
                return false;
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
                return false;
            }
            catch (Win32Exception ex)
            {
                if (ex.ErrorCode == -2147467259)
                {
                    if (runAsAdministrator)
                    {
                        logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable} for a second time with administrative rights. Giving up.");
                        return false;
                    }

                    logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. The process requires elevation. Attempting again with admin rights.");
                    if (TryExecute(executable, arguments, out processCreated, true, priorityClass, maxWaitMs))
                    {
                        logger.Trace($"ProcessUtils/TryExecute: Running the {executable} a second time with administrative rights worked!");
                        return true;
                    }
                    else
                    {
                        logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable} for a second time with administrative rights. Giving up.");
                        return false;
                    }

                }
                else
                {
                    logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. There was an error in opening the associated file.");
                }                
                return false;
            }
            catch (PlatformNotSupportedException ex)
            {
                logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. Method not supported on operating systems without shell support such as Nano Server (.NET Core only).");
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"ProcessUtils/TryExecute: Exception while trying to start {executable}. Not sure what specific exception it is.");
                return false;
            }

            /*if (redirectInputOutput)
                process.BeginOutputReadLine();*/

            //result = RemoveEncodingPreamble(outputBuilder.ToString());            

            //process = null;
            //return false;
        }
       
        public static bool IsExecutableFileType(string executable)
        {
            if (Path.GetExtension(executable).Equals(".exe", StringComparison.CurrentCultureIgnoreCase) ||
                    Path.GetExtension(executable).Equals(".com", StringComparison.CurrentCultureIgnoreCase) ||
                    Path.GetExtension(executable).Equals(".msi", StringComparison.CurrentCultureIgnoreCase) ||
                    Path.GetExtension(executable).Equals(".bat", StringComparison.CurrentCultureIgnoreCase) ||
                    Path.GetExtension(executable).Equals(".cmd", StringComparison.CurrentCultureIgnoreCase) ||
                    Path.GetExtension(executable).Equals(".ps1", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetProcessName(string executable)
        {
            if (executable.Contains(Path.DirectorySeparatorChar) || executable.Contains(Path.AltDirectorySeparatorChar) || executable.Contains(Path.VolumeSeparatorChar))
            {
                return Path.GetFileNameWithoutExtension(executable);
            }
            else
            {
                return executable;
            }
        }

        public static ProcessPriority TranslateNameToPriority(string processPriorityName)
        {
            ProcessPriority wantedPriority = ProcessPriority.Normal;
            switch (processPriorityName.ToLowerInvariant())
            {
                case "high":
                    wantedPriority = ProcessPriority.High;
                    break;
                case "abovenormal":
                    wantedPriority = ProcessPriority.AboveNormal;
                    break;
                case "normal":
                    wantedPriority = ProcessPriority.Normal;
                    break;
                case "belownormal":
                    wantedPriority = ProcessPriority.BelowNormal;
                    break;
                case "idle":
                    wantedPriority = ProcessPriority.Idle;
                    break;
                default:
                    wantedPriority = ProcessPriority.Normal;
                    break;
            }
            return wantedPriority;
        }

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
        
    }

}       

