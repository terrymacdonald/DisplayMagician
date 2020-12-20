using System.Runtime.InteropServices;

namespace DisplayMagicianShared
{
    public class ShellUtils
    {
        public static string AUMID = "LittleBitBig.DisplayMagician";

        // Add the ability to set an Application AUMID so that Windows 10+ recognises the
        // application and things like Toasts, Tasbar pinning and similar functionality
        // works as intended. This DLL import avoids the need to package the app as an MSIX
        // or moving to a WiX based installer at this stage, or the need to add this to the
        // Windows Store.
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

        public static void SetDefaultProcessExplicitAppUserModelID()
        {
            SetCurrentProcessExplicitAppUserModelID(AUMID);
        }

    }
}
