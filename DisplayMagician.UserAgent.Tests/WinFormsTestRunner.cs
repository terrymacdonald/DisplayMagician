using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

internal static class WinFormsTestRunner
{
    internal static void Run(Func<Task> test)
    {
        Exception? failure = null;
        Thread thread = new Thread(() =>
        {
            using Control dispatcher = new Control();
            _ = dispatcher.Handle;
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            dispatcher.BeginInvoke((Action)(async () =>
            {
                try { await test(); }
                catch (Exception ex) { failure = ex; }
                finally { Application.ExitThread(); }
            }));
            Application.Run();
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)), "The form test did not complete.");
        if (failure != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }

}
