using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;
using DisplayMagician.UIForms;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class MessagesFormFailureTests
{
    [Theory]
    [InlineData("io")]
    [InlineData("timeout")]
    [InlineData("service")]
    public void InitialServiceFailureKeepsTheFormUsable(string failureKind)
    {
        Exception? failure = null;
        Thread thread = new Thread(() =>
        {
            try
            {
                using MessagesForm form = new MessagesForm(false, new UnavailableMessageClient(failureKind));
                typeof(MessagesForm).GetMethod("OnLoad", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, new object[] { EventArgs.Empty });
                Label label = (Label)typeof(MessagesForm).GetField("lbl_count", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
                Assert.Contains("could not be loaded", label.Text);
                Assert.False(form.IsDisposed);
            }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)), "The message failure test did not complete.");
        if (failure != null) throw failure;
    }

    private sealed class UnavailableMessageClient : global::DisplayMagician.ControlServicePipeClient
    {
        private readonly string _failureKind;
        public UnavailableMessageClient(string failureKind) => _failureKind = failureKind;
        public override Task<MessageListResult> ListMessagesAsync(CancellationToken cancellationToken)
            => Task.FromException<MessageListResult>(_failureKind switch
            {
                "io" => new IOException("Disconnected test pipe"),
                "timeout" => new TimeoutException("Test timeout"),
                _ => new InvalidOperationException("Test service failure")
            });
    }
}
