using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagicianShared
{
    class Utils
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [Flags]
        public enum SendMessageTimeoutFlag : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
            SMTO_ERRORONEXIT = 0x20
        }


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, String lpWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr windowHandle, uint message, IntPtr wordParameter, IntPtr longParameter, SendMessageTimeoutFlag flag, uint timeout, out IntPtr resultHandle);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, string lParam);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);
       
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        public static bool IsWindows11()
        {
            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            var currentBuildStr = (string)reg.GetValue("CurrentBuild");
            var currentBuild = int.Parse(currentBuildStr);

            return currentBuild >= 22000;
        }

        public const int NULL = 0;
        public const int HWND_BROADCAST = 0xffff;
        public const int WM_SETTINGCHANGE = 0x001a;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int SPI_SETWORKAREA  = 0x002F;
        public const int WM_USER_REFRESHTASKBAR = 0x05CA;
        public const int wParam_SHELLTRAY = 0x00000006;
    }
}
