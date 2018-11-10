using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HeliosDisplayManagement.Shared
{
    public static class ShellHelper
    {
        private static readonly uint NotifyMessage_SettingChange = 0x001A;
        private static readonly uint NotifyMessage_ThemeChanged = 0x031A;
        private static readonly uint ShellChange_AllEvents = 0x7FFFFFFF;
        private static readonly uint ShellChange_FlushFlag = 0x1000;
        private static readonly uint ShellChange_NotifyRecursiveFlag = 0x10000;

        private static readonly UIntPtr WindowHandleBroadcast = (UIntPtr) 0xffff;

        public static Process GetShellProcess()
        {
            try
            {
                var shellWindowHandle = GetShellWindow();

                if (shellWindowHandle != IntPtr.Zero)
                {
                    GetWindowThreadProcessId(shellWindowHandle, out var shellPid);

                    if (shellPid > 0)
                    {
                        return Process.GetProcessById((int) shellPid);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public static async Task IntrigueShellToWriteSettings()
        {
            try
            {
                SendNotifyMessage(WindowHandleBroadcast, NotifyMessage_SettingChange, (UIntPtr) 0, "Policy");
                SendNotifyMessage(WindowHandleBroadcast, NotifyMessage_ThemeChanged, (UIntPtr) 0, null);
                ShellChangeNotify(ShellChange_AllEvents, ShellChange_FlushFlag | ShellChange_NotifyRecursiveFlag,
                    IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception)
            {
                // ignored
            }

            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        }

        [DllImport("user32")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr windowHandle, out uint processId);

        [DllImport("user32", EntryPoint = "SendNotifyMessage", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SendNotifyMessage(
            UIntPtr windowHandle,
            uint messageId,
            UIntPtr wParam,
            string lParam);

        [DllImport("shell32", EntryPoint = "SHChangeNotify", SetLastError = true)]
        private static extern int ShellChangeNotify(uint eventId, uint flags, IntPtr item1, IntPtr item2);
    }
}