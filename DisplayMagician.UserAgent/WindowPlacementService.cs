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

    internal sealed record Source(string ProgramId, string ExecutablePath, HashSet<int> ProcessIds, bool OwnsDirectLaunchTree = false);
    internal sealed record Window(IntPtr Handle, string ProgramId, string ExecutablePath, string ClassName, string Title, NativePlacement Placement);
    internal sealed record WindowScan(List<Window> Windows, bool IsComplete);
    internal sealed record CaptureResult(ShortcutWindowPlacementDefinition[] Placements, bool IsComplete);
    internal sealed record PendingPlacement(IntPtr Handle, DateTime LastAttemptUtc, int AttemptCount, bool WasAccepted);

    internal static WindowScan FindWindows(IReadOnlyList<Source> sources)
    {
        List<Window> windows = new List<Window>();
        bool complete = true;
        bool enumerated = EnumWindows((handle, _) =>
        {
            if (!IsWindowVisible(handle) || GetWindow(handle, OwnerWindow) != IntPtr.Zero || (GetWindowLongPtr(handle, -20).ToInt64() & ToolWindowStyle) != 0)
                return true;
            if (DwmGetWindowAttribute(handle, CloakedAttribute, out int cloaked, sizeof(int)) == 0 && cloaked != 0)
                return true;

            GetWindowThreadProcessId(handle, out uint processId);
            // Later launches may also appear in an earlier same-executable monitor. Prefer
            // the launch tree that owns this PID, then the later source if ownership is unknown.
            Source? source = sources.LastOrDefault(item => item.OwnsDirectLaunchTree && item.ProcessIds.Contains((int)processId))
                ?? sources.LastOrDefault(item => item.ProcessIds.Contains((int)processId));
            if (source == null) return true;
            if (string.Equals(System.IO.Path.GetFileName(source.ExecutablePath), "DisplayMagician.exe", StringComparison.OrdinalIgnoreCase)) return true;

            StringBuilder title = new StringBuilder(512);
            StringBuilder className = new StringBuilder(256);
            GetWindowText(handle, title, title.Capacity);
            GetClassName(handle, className, className.Capacity);
            if (string.IsNullOrWhiteSpace(className.ToString()))
            {
                complete = false;
                Logger.Warn("WindowPlacementService/FindWindows: Could not read the class for PID {0}.", processId);
                return true;
            }

            try
            {
                using Process process = Process.GetProcessById((int)processId);
                if (!string.Equals(process.MainModule?.FileName, source.ExecutablePath, StringComparison.OrdinalIgnoreCase))
                {
                    complete = false;
                    Logger.Warn("WindowPlacementService/FindWindows: Executable changed for PID {0} during window capture.", processId);
                    return true;
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception || ex is ArgumentException)
            {
                complete = false;
                Logger.Warn(ex, "WindowPlacementService/FindWindows: Could not verify executable for PID {0}.", processId);
                return true;
            }

            NativePlacement placement = new NativePlacement { Length = Marshal.SizeOf<NativePlacement>() };
            if (GetWindowPlacement(handle, ref placement))
                windows.Add(new Window(handle, source.ProgramId, source.ExecutablePath, className.ToString(), title.ToString(), placement));
            else
            {
                complete = false;
                Logger.Warn("WindowPlacementService/FindWindows: GetWindowPlacement failed for PID {0}, Win32 error {1}.", processId, Marshal.GetLastWin32Error());
            }
            return true;
        }, IntPtr.Zero);
        if (!enumerated)
        {
            complete = false;
            Logger.Warn("WindowPlacementService/FindWindows: EnumWindows failed with Win32 error {0}.", Marshal.GetLastWin32Error());
        }
        return new WindowScan(windows, complete);
    }

    internal static CaptureResult Capture(IReadOnlyList<Source> sources)
    {
        WindowScan scan = FindWindows(sources);
        List<ShortcutWindowPlacementDefinition> placements = new List<ShortcutWindowPlacementDefinition>();
        foreach (Window window in scan.Windows)
        {
            Rectangle normal = window.Placement.Normal.ToRectangle();
            if (string.IsNullOrWhiteSpace(window.Title) && scan.Windows.Count(item => item.ProgramId == window.ProgramId &&
                string.Equals(item.ExecutablePath, window.ExecutablePath, StringComparison.OrdinalIgnoreCase) && item.ClassName == window.ClassName) != 1)
            {
                Logger.Info("WindowPlacementService/Capture: Skipped ambiguous titleless window class {0} for {1}.", window.ClassName, window.ExecutablePath);
                continue;
            }
            string[] displays;
            if (window.Placement.ShowCommand == 2)
                displays = new[] { Screen.FromHandle(window.Handle).DeviceName };
            else
            {
                if (!GetWindowRect(window.Handle, out NativeRect currentRect))
                {
                    Logger.Warn("WindowPlacementService/Capture: GetWindowRect failed for {0}, Win32 error {1}.", window.ExecutablePath, Marshal.GetLastWin32Error());
                    return new CaptureResult(Array.Empty<ShortcutWindowPlacementDefinition>(), false);
                }
                Rectangle current = currentRect.ToRectangle();
                displays = Screen.AllScreens.Where(screen => screen.Bounds.IntersectsWith(current)).Select(screen => screen.DeviceName).ToArray();
            }
            if (displays.Length == 0) displays = new[] { Screen.FromHandle(window.Handle).DeviceName };
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
                MaximizedLeft = window.Placement.Max.X,
                MaximizedTop = window.Placement.Max.Y,
                ShowCommand = window.Placement.ShowCommand,
                DisplayNames = displays
            });
        }
        return new CaptureResult(placements.ToArray(), scan.IsComplete);
    }

    internal static (int Count, string[] Programs) Restore(IReadOnlyList<Source> sources, IReadOnlyList<ShortcutWindowPlacementDefinition> placements, HashSet<int> completed, Dictionary<int, PendingPlacement> pending, bool verifyOnly = false)
    {
        List<Window> windows = FindWindows(sources).Windows;
        HashSet<IntPtr> usedHandles = pending.Values.Select(item => item.Handle).ToHashSet();
        List<string> programs = new List<string>();
        int moved = 0;
        for (int index = 0; index < placements.Count; index++)
        {
            if (completed.Contains(index)) continue;
            ShortcutWindowPlacementDefinition saved = placements[index];
            int previousAttempts = 0;
            string[] requiredDisplays = saved.DisplayNames ?? Array.Empty<string>();
            Rectangle savedBounds = Rectangle.FromLTRB(saved.Left, saved.Top, saved.Right, saved.Bottom);
            if (savedBounds.Width <= 0 || savedBounds.Height <= 0 || requiredDisplays.Length == 0 ||
                requiredDisplays.Any(name => !Screen.AllScreens.Any(screen => string.Equals(screen.DeviceName, name, StringComparison.OrdinalIgnoreCase))))
            {
                Logger.Warn("WindowPlacementService/Restore: Skipped placement {0} for {1}: required display is missing.", index, saved.ExecutablePath);
                pending.Remove(index);
                completed.Add(index);
                continue;
            }

            if (pending.TryGetValue(index, out PendingPlacement? pendingPlacement))
            {
                Window? pendingWindow = windows.FirstOrDefault(item => item.Handle == pendingPlacement.Handle);
                if (pendingWindow != null && MatchesSavedPlacement(pendingWindow.Placement, saved))
                {
                    pending.Remove(index);
                    completed.Add(index);
                    if (pendingPlacement.WasAccepted)
                    {
                        moved++;
                        programs.Add(System.IO.Path.GetFileNameWithoutExtension(saved.ExecutablePath));
                        Logger.Info("WindowPlacementService/Restore: Verified window class {0} for {1} on {2}.", saved.WindowClass, saved.ExecutablePath, string.Join(",", requiredDisplays));
                    }
                    continue;
                }
                if (verifyOnly || DateTime.UtcNow - pendingPlacement.LastAttemptUtc < TimeSpan.FromSeconds(1)) continue;
                if (pendingPlacement.AttemptCount >= 3)
                {
                    pending.Remove(index);
                    completed.Add(index);
                    Logger.Warn("WindowPlacementService/Restore: Stopped placing window class {0} for {1} after {2} failed attempts.", saved.WindowClass, saved.ExecutablePath, pendingPlacement.AttemptCount);
                    continue;
                }
                previousAttempts = pendingPlacement.AttemptCount;
                pending.Remove(index);
                usedHandles.Remove(pendingPlacement.Handle);
            }

            if (verifyOnly) continue;

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
            if (MatchesSavedPlacement(match.Placement, saved))
            {
                usedHandles.Add(match.Handle);
                completed.Add(index);
                continue;
            }
            NativePlacement target = match.Placement;
            target.Normal = new NativeRect(saved.Left, saved.Top, saved.Right, saved.Bottom);
            if (saved.ShowCommand == 3 && saved.MaximizedLeft.HasValue && saved.MaximizedTop.HasValue)
                target.Max = new NativePoint { X = saved.MaximizedLeft.Value, Y = saved.MaximizedTop.Value };
            target.ShowCommand = saved.ShowCommand is 1 or 2 or 3 ? saved.ShowCommand : 1;
            target.Flags = 4;
            bool applied = SetWindowPlacement(match.Handle, ref target);
            usedHandles.Add(match.Handle);
            pending[index] = new PendingPlacement(match.Handle, DateTime.UtcNow, previousAttempts + 1, applied);
            if (!applied) Logger.Warn("WindowPlacementService/Restore: SetWindowPlacement failed for {0}, Win32 error {1}.", saved.ExecutablePath, Marshal.GetLastWin32Error());
        }
        return (moved, programs.ToArray());
    }

    private static bool MatchesSavedPlacement(NativePlacement current, ShortcutWindowPlacementDefinition saved)
    {
        int expectedShowCommand = saved.ShowCommand is 2 or 3 ? saved.ShowCommand : 1;
        return current.ShowCommand == expectedShowCommand &&
            current.Normal.Left == saved.Left && current.Normal.Top == saved.Top &&
            current.Normal.Right == saved.Right && current.Normal.Bottom == saved.Bottom &&
            (expectedShowCommand != 3 || !saved.MaximizedLeft.HasValue || !saved.MaximizedTop.HasValue ||
             (current.Max.X == saved.MaximizedLeft.Value && current.Max.Y == saved.MaximizedTop.Value));
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
    [DllImport("user32.dll", SetLastError = true)] private static extern bool GetWindowRect(IntPtr handle, out NativeRect rectangle);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool SetWindowPlacement(IntPtr handle, ref NativePlacement placement);
    [DllImport("dwmapi.dll")] private static extern int DwmGetWindowAttribute(IntPtr handle, int attribute, out int value, int size);
}
