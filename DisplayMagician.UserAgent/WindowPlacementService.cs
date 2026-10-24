using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DisplayMagician.ConfigurationDefinitions;

namespace DisplayMagician.UserAgent;

/// <summary>Captures and restores windows in the interactive user's desktop.</summary>
internal static class WindowPlacementService
{
    private const int ToolWindowStyle = 0x00000080;
    private const uint OwnerWindow = 4;
    private const int CloakedAttribute = 14;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    internal sealed record Source(string ProgramId, string ExecutablePath, HashSet<int> ProcessIds);
    internal sealed record Window(IntPtr Handle, string ProgramId, string ExecutablePath, string ClassName, string Title, NativePlacement Placement);

    internal static List<Window> FindWindows(IReadOnlyList<Source> sources)
    {
        List<Window> windows = new List<Window>();
        EnumWindows((handle, _) =>
        {
            if (!IsWindowVisible(handle) || GetWindow(handle, OwnerWindow) != IntPtr.Zero || (GetWindowLongPtr(handle, -20).ToInt64() & ToolWindowStyle) != 0)
                return true;
            if (DwmGetWindowAttribute(handle, CloakedAttribute, out int cloaked, sizeof(int)) == 0 && cloaked != 0)
                return true;

            GetWindowThreadProcessId(handle, out uint processId);
            Source? source = sources.FirstOrDefault(item => item.ProcessIds.Contains((int)processId));
            if (source == null) return true;
            if (string.Equals(System.IO.Path.GetFileName(source.ExecutablePath), "DisplayMagician.exe", StringComparison.OrdinalIgnoreCase)) return true;

            StringBuilder title = new StringBuilder(512);
            StringBuilder className = new StringBuilder(256);
            GetWindowText(handle, title, title.Capacity);
            GetClassName(handle, className, className.Capacity);
            if (string.IsNullOrWhiteSpace(title.ToString()) || string.IsNullOrWhiteSpace(className.ToString())) return true;

            try
            {
                using Process process = Process.GetProcessById((int)processId);
                if (!string.Equals(process.MainModule?.FileName, source.ExecutablePath, StringComparison.OrdinalIgnoreCase)) return true;
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception || ex is ArgumentException)
            {
                Logger.Debug(ex, "WindowPlacementService/FindWindows: Could not verify executable for PID {0}.", processId);
                return true;
            }

            NativePlacement placement = new NativePlacement { Length = Marshal.SizeOf<NativePlacement>() };
            if (GetWindowPlacement(handle, ref placement))
                windows.Add(new Window(handle, source.ProgramId, source.ExecutablePath, className.ToString(), title.ToString(), placement));
            return true;
        }, IntPtr.Zero);
        return windows;
    }

    internal static ShortcutWindowPlacementDefinition[] Capture(IReadOnlyList<Source> sources)
    {
        List<ShortcutWindowPlacementDefinition> placements = new List<ShortcutWindowPlacementDefinition>();
        foreach (Window window in FindWindows(sources))
        {
            Rectangle normal = window.Placement.Normal.ToRectangle();
            string[] displays = Screen.AllScreens.Where(screen => screen.Bounds.IntersectsWith(normal)).Select(screen => screen.DeviceName).ToArray();
            int occurrence = placements.Count(item => item.ProgramId == window.ProgramId && item.WindowClass == window.ClassName);
            placements.Add(new ShortcutWindowPlacementDefinition
            {
                ProgramId = window.ProgramId,
                ExecutablePath = window.ExecutablePath,
                WindowClass = window.ClassName,
                WindowTitle = window.Title,
                OccurrenceIndex = occurrence,
                Left = normal.Left,
                Top = normal.Top,
                Right = normal.Right,
                Bottom = normal.Bottom,
                ShowCommand = window.Placement.ShowCommand,
                DisplayNames = displays
            });
        }
        return placements.ToArray();
    }

    internal static (int Count, string[] Programs) Restore(IReadOnlyList<Source> sources, IReadOnlyList<ShortcutWindowPlacementDefinition> placements, HashSet<int> completed)
    {
        List<Window> windows = FindWindows(sources);
        HashSet<IntPtr> usedHandles = new HashSet<IntPtr>();
        List<string> programs = new List<string>();
        int moved = 0;
        for (int index = 0; index < placements.Count; index++)
        {
            if (completed.Contains(index)) continue;
            ShortcutWindowPlacementDefinition saved = placements[index];
            Rectangle savedBounds = Rectangle.FromLTRB(saved.Left, saved.Top, saved.Right, saved.Bottom);
            if (savedBounds.Width <= 0 || savedBounds.Height <= 0 || saved.DisplayNames.Length == 0 ||
                saved.DisplayNames.Any(name => !Screen.AllScreens.Any(screen => string.Equals(screen.DeviceName, name, StringComparison.OrdinalIgnoreCase) && screen.Bounds.IntersectsWith(savedBounds))))
            {
                Logger.Warn("WindowPlacementService/Restore: Skipped placement {0} for {1}: required display is missing.", index, saved.ExecutablePath);
                completed.Add(index);
                continue;
            }

            List<Window> matches = windows.Where(window => window.ProgramId == saved.ProgramId &&
                string.Equals(window.ExecutablePath, saved.ExecutablePath, StringComparison.OrdinalIgnoreCase) &&
                window.ClassName == saved.WindowClass && !usedHandles.Contains(window.Handle)).ToList();
            int savedClassCount = placements.Count(item => item.ProgramId == saved.ProgramId &&
                string.Equals(item.ExecutablePath, saved.ExecutablePath, StringComparison.OrdinalIgnoreCase) && item.WindowClass == saved.WindowClass);
            List<Window> titleMatches = matches.Where(window => window.Title == saved.WindowTitle).ToList();
            int savedTitleCount = placements.Count(item => item.ProgramId == saved.ProgramId &&
                string.Equals(item.ExecutablePath, saved.ExecutablePath, StringComparison.OrdinalIgnoreCase) &&
                item.WindowClass == saved.WindowClass && item.WindowTitle == saved.WindowTitle);
            if (titleMatches.Count == 1 && savedTitleCount == 1) matches = titleMatches;
            else if (savedClassCount != 1 || matches.Count != 1) continue;
            if (matches.Count != 1) continue;

            Window match = matches[0];
            NativePlacement target = match.Placement;
            target.Normal = new NativeRect(saved.Left, saved.Top, saved.Right, saved.Bottom);
            target.ShowCommand = 1;
            target.Flags = 4;
            bool applied = SetWindowPlacement(match.Handle, ref target);
            if (applied && saved.ShowCommand is 2 or 3)
            {
                target.ShowCommand = saved.ShowCommand;
                applied = SetWindowPlacement(match.Handle, ref target);
            }
            if (applied)
            {
                usedHandles.Add(match.Handle);
                completed.Add(index);
                moved++;
                programs.Add(System.IO.Path.GetFileNameWithoutExtension(saved.ExecutablePath));
                Logger.Info("WindowPlacementService/Restore: Placed window class {0} for {1} on {2}.", saved.WindowClass, saved.ExecutablePath, string.Join(",", saved.DisplayNames));
            }
            else Logger.Warn("WindowPlacementService/Restore: SetWindowPlacement failed for {0}, Win32 error {1}.", saved.ExecutablePath, Marshal.GetLastWin32Error());
        }
        return (moved, programs.ToArray());
    }

    [StructLayout(LayoutKind.Sequential)] internal struct NativeRect
    {
        public int Left, Top, Right, Bottom;
        public NativeRect(int left, int top, int right, int bottom) { Left = left; Top = top; Right = right; Bottom = bottom; }
        public Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);
    }
    [StructLayout(LayoutKind.Sequential)] internal struct NativePoint { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)] internal struct NativePlacement
    {
        public int Length;
        public int Flags;
        public int ShowCommand;
        public NativePoint Min;
        public NativePoint Max;
        public NativeRect Normal;
    }
    private delegate bool EnumWindowsCallback(IntPtr handle, IntPtr parameter);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsCallback callback, IntPtr parameter);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr handle);
    [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr handle, uint command);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")] private static extern IntPtr GetWindowLongPtr(IntPtr handle, int index);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr handle, out uint processId);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr handle, StringBuilder text, int count);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr handle, StringBuilder text, int count);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool GetWindowPlacement(IntPtr handle, ref NativePlacement placement);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool SetWindowPlacement(IntPtr handle, ref NativePlacement placement);
    [DllImport("dwmapi.dll")] private static extern int DwmGetWindowAttribute(IntPtr handle, int attribute, out int value, int size);
}
