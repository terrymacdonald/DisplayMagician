using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace DisplayMagician.UserAgent;

/// <summary>Activates packaged applications from the interactive User Agent and returns their process when available.</summary>
internal static class UwpApplicationLauncher
{
    public static Process? Start(string appUserModelId, string arguments)
    {
        if (string.IsNullOrWhiteSpace(appUserModelId))
        {
            return null;
        }

        try
        {
            IApplicationActivationManager activationManager = (IApplicationActivationManager)new ApplicationActivationManager();
            int result = activationManager.ActivateApplication(appUserModelId, arguments ?? string.Empty, ActivateOptions.None, out uint processId);
            return result < 0 || processId == 0 ? null : Process.GetProcessById((int)processId);
        }
        catch (Exception ex) when (ex is COMException || ex is ArgumentException || ex is InvalidOperationException)
        {
            return null;
        }
    }

    public static bool IsRunning(string appUserModelId)
    {
        if (string.IsNullOrWhiteSpace(appUserModelId))
        {
            return false;
        }

        foreach (Process process in Process.GetProcesses())
        {
            try
            {
                uint length = 0;
                if (GetApplicationUserModelId(process.Handle, ref length, null) != ErrorInsufficientBuffer || length == 0)
                {
                    continue;
                }

                StringBuilder buffer = new StringBuilder((int)length);
                if (GetApplicationUserModelId(process.Handle, ref length, buffer) == 0 &&
                    string.Equals(buffer.ToString(), appUserModelId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception || ex is NotSupportedException)
            {
                // Some system and protected processes cannot be inspected from the interactive session.
            }
            finally
            {
                process.Dispose();
            }
        }

        return false;
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