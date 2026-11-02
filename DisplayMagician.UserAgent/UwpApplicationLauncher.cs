using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DisplayMagician.UserAgent;

/// <summary>Activates packaged applications from the interactive User Agent and returns their process when available.</summary>
internal static class UwpApplicationLauncher
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public static Process? Start(string appUserModelId, string arguments)
        => Start(appUserModelId, arguments, out _);

    internal static Process? Start(string appUserModelId, string arguments, out bool ownsProcess,
        Func<string, string, (int Result, uint ProcessId)>? activate = null, Func<HashSet<int>>? snapshotProcessIds = null,
        Func<Process, string?>? readAppId = null, Func<Process, DateTime>? readStartTime = null)
    {
        ownsProcess = false;
        if (string.IsNullOrWhiteSpace(appUserModelId)) return null;
        HashSet<int>? existingIds = null;
        try
        {
            existingIds = (snapshotProcessIds ?? SnapshotProcessIds)();
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or UnauthorizedAccessException)
        {
            Logger.Warn(ex, "UwpApplicationLauncher/Start: Could not snapshot existing processes for {0}; automatic closing will be skipped.", appUserModelId);
        }
        DateTime activationStartedUtc = DateTime.UtcNow;
        try
        {
            (int result, uint processId) = (activate ?? Activate)(appUserModelId, arguments ?? string.Empty);
            if (result < 0 || processId == 0 || processId > int.MaxValue)
            {
                Logger.Warn("UwpApplicationLauncher/Start: Activation failed for {0}, HRESULT {1:X8}, PID {2}.", appUserModelId, result, processId);
                return null;
            }
            Process process = Process.GetProcessById((int)processId);
            try
            {
                // Activation can reuse an app. A new PID alone is insufficient: verify
                // its birth time and AUMID, binding its handle before retaining cleanup ownership.
                ownsProcess = existingIds != null && !existingIds.Contains(process.Id) &&
                    (readStartTime ?? (item => item.StartTime.ToUniversalTime()))(process) >= activationStartedUtc &&
                    string.Equals((readAppId ?? ReadAppId)(process), appUserModelId, StringComparison.OrdinalIgnoreCase);
                if (!ownsProcess)
                    Logger.Info("UwpApplicationLauncher/Start: PID {0} for {1} is existing or its new ownership is unverified; automatic closing will be skipped.", process.Id, appUserModelId);
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or ArgumentException or UnauthorizedAccessException)
            {
                Logger.Warn(ex, "UwpApplicationLauncher/Start: Could not verify new ownership of PID {0} for {1}; automatic closing will be skipped.", processId, appUserModelId);
            }
            return process;
        }
        catch (Exception ex) when (ex is COMException or ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            Logger.Warn(ex, "UwpApplicationLauncher/Start: Could not activate or acquire the process for {0}.", appUserModelId);
            return null;
        }
    }

    private static (int Result, uint ProcessId) Activate(string appUserModelId, string arguments)
    {
        IApplicationActivationManager? activationManager = null;
        try
        {
            activationManager = (IApplicationActivationManager)new ApplicationActivationManager();
            int result = activationManager.ActivateApplication(appUserModelId, arguments, ActivateOptions.None, out uint processId);
            return (result, processId);
        }
        finally
        {
            if (activationManager != null) Marshal.ReleaseComObject(activationManager);
        }
    }

    private static HashSet<int> SnapshotProcessIds()
    {
        HashSet<int> ids = new HashSet<int>();
        Process[] processes = Process.GetProcesses();
        try
        {
            foreach (Process process in processes) ids.Add(process.Id);
            return ids;
        }
        finally
        {
            foreach (Process process in processes) process.Dispose();
        }
    }

    public static bool IsRunning(string appUserModelId)
    {
        if (string.IsNullOrWhiteSpace(appUserModelId)) return false;
        Process[] processes = Process.GetProcesses();
        try
        {
            foreach (Process process in processes)
            {
                try
                {
                    if (string.Equals(ReadAppId(process), appUserModelId, StringComparison.OrdinalIgnoreCase)) return true;
                }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException)
                {
                    // Some system and protected processes cannot be inspected from this session.
                }
            }
            return false;
        }
        finally
        {
            foreach (Process process in processes) process.Dispose();
        }
    }

    private static string? ReadAppId(Process process)
    {
        uint length = 0;
        if (GetApplicationUserModelId(process.Handle, ref length, null) != ErrorInsufficientBuffer || length == 0) return null;
        StringBuilder buffer = new StringBuilder(checked((int)length));
        return GetApplicationUserModelId(process.Handle, ref length, buffer) == 0 ? buffer.ToString() : null;
    }

    private const int ErrorInsufficientBuffer = 122;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetApplicationUserModelId(IntPtr processHandle, ref uint applicationUserModelIdLength, StringBuilder? applicationUserModelId);

    [ComImport]
    [Guid("2e941141-7f97-4756-ba1d-9decde894a3d")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IApplicationActivationManager
    {
        int ActivateApplication(string appUserModelId, string arguments, ActivateOptions options, out uint processId);
    }

    [ComImport]
    [Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]
    private class ApplicationActivationManager
    {
    }

    private enum ActivateOptions
    {
        None = 0
    }
}