using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;
using DisplayMagician.UIForms;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class InstallationResetFormTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConfirmationStartsOnlyOneResetAndPreventsClosingUntilResult(bool success) => WinFormsTestRunner.Run(async () =>
    {
        int calls = 0;
        TaskCompletionSource<ControlResponse> reply = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using InstallationResetForm form = new(() => { calls++; return reply.Task; });
        Invoke(form, "btn_reset_Click", form, EventArgs.Empty);
        Invoke(form, "btn_reset_Click", form, EventArgs.Empty);
        Assert.Equal(1, calls);
        FormClosingEventArgs closing = new(CloseReason.UserClosing, false);
        Invoke(form, "InstallationResetForm_FormClosing", form, closing);
        Assert.True(closing.Cancel);
        reply.SetResult(new ControlResponse { IsSuccessful = success, Message = success ? "Reset complete" : "Reset failed" });
        for (int i = 0; i < 100 && !Field<Button>(form, "btn_close").Enabled; i++) await Task.Delay(10);
        Assert.True(Field<Button>(form, "btn_close").Enabled);
        Assert.Equal(!success, Field<Button>(form, "btn_reset").Enabled);
        Assert.Equal(success ? "Reset complete" : "Reset failed", Field<Label>(form, "lbl_status").Text);
    });

    private static object? Invoke(object instance, string name, params object[] arguments)
        => instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(instance, arguments);

    private static T Field<T>(object instance, string name) => (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance)!;
}
