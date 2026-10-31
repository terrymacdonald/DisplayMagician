using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;
using DisplayMagician.UIForms;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class SharedLoggingSettingsFormTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedSaveKeepsDraftOpenAndPreventsDuplicateRequests(bool transportFails) => WinFormsTestRunner.Run(async () =>
    {
        Client client = new Client();
        List<string> errors = new List<string>();
        using SettingsForm form = Create(client, errors);
        FormClosingEventArgs closing = CloseEvent(form);
        Assert.True(closing.Cancel);
        Assert.False(form.Enabled);
        Assert.True(CloseEvent(form).Cancel);
        Assert.Equal(1, client.Requests);
        if (transportFails) client.Reply.SetException(new System.IO.IOException("disconnected"));
        else client.Reply.SetResult(new ControlResponse { Message = "write failed" });
        await Until(() => form.Enabled);
        Assert.Single(errors);
        Assert.False(form.IsDisposed);
        Assert.Equal("Full Application Trace (very large)", Field<ComboBox>(form, "cmb_loglevel").SelectedItem);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DisposedFormIgnoresLateLoggingReply(bool successful) => WinFormsTestRunner.Run(async () =>
    {
        Client client = new Client();
        List<string> errors = new List<string>();
        using SettingsForm form = Create(client, errors);
        Assert.True(CloseEvent(form).Cancel);
        form.Dispose();
        client.Reply.SetResult(new ControlResponse { IsSuccessful = successful, LoggingSettings = new SharedLoggingSettings() });
        await Until(() => !Field<bool>(form, "_loggingSavePending"));
        Assert.Empty(errors);
    });

    private static SettingsForm Create(Client client, List<string> errors)
    {
        SettingsForm form = new SettingsForm(client, errors.Add);
        typeof(SettingsForm).GetField("_loadedLogLevel", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(form, "Warn");
        Field<ComboBox>(form, "cmb_loglevel").SelectedIndex = 0;
        return form;
    }

    private static FormClosingEventArgs CloseEvent(SettingsForm form)
    {
        FormClosingEventArgs args = new FormClosingEventArgs(CloseReason.UserClosing, false);
        typeof(SettingsForm).GetMethod("SettingsForm_FormClosing", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, new object[] { form, args });
        return args;
    }

    private static T Field<T>(SettingsForm form, string name) => (T)typeof(SettingsForm).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
    private static async Task Until(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition()) { Assert.True(DateTime.UtcNow < deadline); await Task.Delay(10); }
    }

    private sealed class Client : DisplayMagician.ControlServicePipeClient
    {
        public int Requests { get; private set; }
        public TaskCompletionSource<ControlResponse> Reply { get; } = new TaskCompletionSource<ControlResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override Task<ControlResponse> SendAsync(ControlEnvelope request, CancellationToken cancellationToken)
        {
            Assert.Equal(ControlMessageType.SetSharedLogLevel, request.MessageType);
            Requests++;
            return Reply.Task;
        }
    }
}
