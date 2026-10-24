using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class WindowPlacementServiceTests
{
    [Fact]
    public void CaptureAndRestore_UsesNormalMinimizedAndMaximizedPlacement()
    {
        if (!Environment.UserInteractive || Screen.PrimaryScreen == null) return;
        Exception? failure = null;
        Thread thread = new Thread(() =>
        {
            try
            {
                string title = $"DisplayMagician window placement test {Guid.NewGuid():N}";
                using Form form = new Form { Text = title, StartPosition = FormStartPosition.Manual,
                    Bounds = new Rectangle(Screen.PrimaryScreen.WorkingArea.Left + 60, Screen.PrimaryScreen.WorkingArea.Top + 60, 320, 220) };
                form.Show();
                Application.DoEvents();
                using Process process = Process.GetCurrentProcess();
                WindowPlacementService.Source source = new WindowPlacementService.Source("main", process.MainModule!.FileName, new HashSet<int> { process.Id });
                WindowPlacementService.CaptureResult normal = WindowPlacementService.Capture(new[] { source });
                ShortcutWindowPlacementDefinition saved = Assert.Single(normal.Placements, item => item.WindowTitle == title);
                Assert.Equal(1, saved.ShowCommand);
                Assert.NotEmpty(saved.DisplayNames);
                WindowPlacementService.Source owningSource = new WindowPlacementService.Source("main", process.MainModule.FileName, new HashSet<int> { process.Id }, true);
                WindowPlacementService.Source overlappingSource = new WindowPlacementService.Source("helper", process.MainModule.FileName, new HashSet<int> { process.Id });
                WindowPlacementService.CaptureResult overlap = WindowPlacementService.Capture(new[] { owningSource, overlappingSource });
                Assert.Equal("main", Assert.Single(overlap.Placements, item => item.WindowTitle == title).ProgramId);

                form.WindowState = FormWindowState.Minimized;
                Application.DoEvents();
                WindowPlacementService.CaptureResult minimized = WindowPlacementService.Capture(new[] { source });
                ShortcutWindowPlacementDefinition min = Assert.Single(minimized.Placements, item => item.WindowTitle == title);
                Assert.Equal(2, min.ShowCommand);

                form.WindowState = FormWindowState.Maximized;
                Application.DoEvents();
                WindowPlacementService.CaptureResult maximized = WindowPlacementService.Capture(new[] { source });
                ShortcutWindowPlacementDefinition max = Assert.Single(maximized.Placements, item => item.WindowTitle == title);
                Assert.Equal(3, max.ShowCommand);
                Assert.NotNull(max.MaximizedLeft);
                Assert.NotNull(max.MaximizedTop);

                form.WindowState = FormWindowState.Normal;
                form.Left += 100;
                Application.DoEvents();
                HashSet<int> completed = new HashSet<int>();
                Dictionary<int, WindowPlacementService.PendingPlacement> pending = new Dictionary<int, WindowPlacementService.PendingPlacement>();
                WindowPlacementService.Restore(new[] { source }, new[] { saved }, completed, pending);
                Application.DoEvents();
                WindowPlacementService.Restore(new[] { source }, new[] { saved }, completed, pending);
                Assert.Contains(0, completed);

                completed.Clear();
                pending.Clear();
                WindowPlacementService.Restore(new[] { source }, new[] { max }, completed, pending);
                Application.DoEvents();
                WindowPlacementService.Restore(new[] { source }, new[] { max }, completed, pending);
                Assert.Contains(0, completed);

                completed.Clear();
                pending.Clear();
                WindowPlacementService.Restore(new[] { source }, new[] { min }, completed, pending);
                Application.DoEvents();
                WindowPlacementService.Restore(new[] { source }, new[] { min }, completed, pending);
                Assert.Contains(0, completed);

                completed.Clear();
                pending.Clear();
                pending[0] = new WindowPlacementService.PendingPlacement(form.Handle, DateTime.UtcNow.AddSeconds(-2), 3, true);
                WindowPlacementService.Restore(new[] { source }, new[] { saved }, completed, pending);
                Assert.Contains(0, completed);
                Assert.Empty(pending);

                form.WindowState = FormWindowState.Normal;
                form.Text = string.Empty;
                Application.DoEvents();
                WindowPlacementService.CaptureResult titleless = WindowPlacementService.Capture(new[] { source });
                Assert.Single(titleless.Placements, item => item.WindowTitle == string.Empty && item.WindowClass == saved.WindowClass);
            }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(15)), "The window placement test did not finish.");
        if (failure != null) throw failure;
    }
}
