using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;
using DisplayMagician.UIForms;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ProfileFormConcurrencyTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LateRefreshCannotReplaceANewerSelection(bool audio) => RunSta(async () =>
    {
        FakeClient client = new FakeClient();
        using Form form = CreateForm(audio, client, new List<string>());
        Task first = Refresh(form, audio, "old");
        Task second = Refresh(form, audio, "new");
        Assert.Equal(2, client.Pending.Count);
        client.Pending[1].Completion.SetResult(ListResponse(audio, "new"));
        await second;
        client.Pending[0].Completion.SetResult(ListResponse(audio, "old"));
        await first;
        Assert.Equal("new", SelectedId(form, audio));
    });

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ClosingWhileWaitingCancelsTheRequestAndIgnoresLateReplies(bool audio, bool fail) => RunSta(async () =>
    {
        FakeClient client = new FakeClient();
        List<string> errors = new List<string>();
        using Form form = CreateForm(audio, client, errors);
        Task pending = InvokeTask(form, "RunProfileActionAsync", (Func<Task>)(() => Refresh(form, audio, "late")));
        Assert.Single(client.Pending);
        form.Dispose();
        Assert.True(client.Pending[0].Token.IsCancellationRequested);
        if (fail) client.Pending[0].Completion.SetException(new IOException("Disconnected after closing."));
        else client.Pending[0].Completion.SetResult(ListResponse(audio, "late"));
        await pending;
        Assert.Empty(errors);
        Assert.Null(SelectedId(form, audio));
    });

    [Theory]
    [InlineData(false, "io")]
    [InlineData(false, "timeout")]
    [InlineData(false, "service")]
    [InlineData(true, "io")]
    [InlineData(true, "timeout")]
    [InlineData(true, "service")]
    public void ServiceFailuresAreReportedAndActionsRecover(bool audio, string kind) => RunSta(async () =>
    {
        FakeClient client = new FakeClient();
        List<string> errors = new List<string>();
        using Form form = CreateForm(audio, client, errors);
        Task loading = Refresh(form, audio, "saved");
        client.Pending[0].Completion.SetResult(ListResponse(audio, "saved"));
        await loading;
        Task action = InvokeTask(form, "RunProfileActionAsync", (Func<Task>)(() => Refresh(form, audio, "saved")));
        client.Pending[1].Completion.SetException(kind == "io" ? new IOException("Disconnected") : kind == "timeout" ? new TimeoutException("Timed out") : new InvalidOperationException("Service refused"));
        await action;
        Assert.Single(errors);
        Assert.Equal("saved", SelectedId(form, audio));
        Assert.True(Field<Button>(form, audio ? "btn_apply_audio_profile" : "btn_apply").Enabled);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RefreshWithNoProfilesClearsStaleSelectionAndDisablesActions(bool audio) => RunSta(async () =>
    {
        FakeClient client = new FakeClient();
        using Form form = CreateForm(audio, client, new List<string>());
        Task loading = Refresh(form, audio, "saved");
        client.Pending[0].Completion.SetResult(ListResponse(audio, "saved"));
        await loading;
        Task empty = Refresh(form, audio, null);
        client.Pending[1].Completion.SetResult(audio
            ? new ControlResponse { IsSuccessful = true, AudioProfileList = new AudioProfileListResult { CanAccessAudioSettings = true } }
            : new ControlResponse { IsSuccessful = true, ProfileList = new ProfileListResult() });
        await empty;
        Assert.Null(SelectedId(form, audio));
        Assert.False(Field<Button>(form, audio ? "btn_delete_audio_profile" : "btn_delete").Enabled);
        Assert.False(Field<Button>(form, audio ? "btn_apply_audio_profile" : "btn_apply").Enabled);
    });

    [Fact]
    public void UnsavedCurrentLayoutDoesNotEnableSavedProfileActions() => RunSta(async () =>
    {
        FakeClient client = new FakeClient();
        using DisplayProfileForm form = new DisplayProfileForm(client, (_, _) => { });
        Task loading = Refresh(form, false, null);
        client.Pending[0].Completion.SetResult(new ControlResponse { IsSuccessful = true, ProfileList = new ProfileListResult { CurrentLayout = new DisplayProfileView { Id = "current", Name = "Current", IsSaved = false } } });
        await loading;
        typeof(DisplayProfileForm).GetMethod("SetProfileActionsEnabled", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, new object[] { true });
        Assert.False(Field<Button>(form, "btn_delete").Enabled);
        Assert.False(Field<Button>(form, "btn_profile_settings").Enabled);
        Assert.False(Field<Button>(form, "btn_hotkey").Enabled);
        Assert.True(Field<Button>(form, "btn_save_or_rename").Enabled);
    });

    [Fact]
    public void AudioRefreshPreservesTheRemainingActiveSelection() => RunSta(async () =>
    {
        FakeClient client = new FakeClient();
        using Form form = CreateForm(true, client, new List<string>());
        Task refresh = Refresh(form, true, null);
        client.Pending[0].Completion.SetResult(new ControlResponse { IsSuccessful = true, AudioProfileList = new AudioProfileListResult
            { CanAccessAudioSettings = true, SavedProfiles = new[] { new AudioProfileView { Id = "remaining", Name = "Remaining", IsActive = true } } } });
        await refresh;
        Assert.Equal("remaining", SelectedId(form, true));
        Assert.True(Field<Button>(form, "btn_apply_audio_profile").Enabled);
    });

    [Fact]
    public void ApplyUsesTheOriginalProfileAndRejectsOverlappingClicks() => RunSta(async () =>
    {
        FakeClient client = new FakeClient();
        using DisplayProfileForm form = new DisplayProfileForm(client, (_, _) => { });
        Task loading = Refresh(form, false, "original");
        client.Pending[0].Completion.SetResult(ListResponse(false, "original"));
        await loading;
        MethodInfo apply = typeof(DisplayProfileForm).GetMethod("Apply_Click", BindingFlags.Instance | BindingFlags.NonPublic)!;
        apply.Invoke(form, new object[] { form, EventArgs.Empty });
        apply.Invoke(form, new object[] { form, EventArgs.Empty });
        Assert.Equal(2, client.Pending.Count);
        using JsonDocument command = JsonDocument.Parse(client.Pending[1].Envelope.Payload);
        Assert.Equal("original", command.RootElement.GetProperty("ProfileId").GetString());
        typeof(DisplayProfileForm).GetMethod("ChangeSelectedProfile", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, new object[] { new DisplayProfileView { Id = "different", Name = "Different", IsSaved = true, IsValid = true } });
        client.Pending[1].Completion.SetResult(new ControlResponse { IsSuccessful = true });
        await WaitUntil(() => client.Pending.Count == 3);
        using JsonDocument refresh = JsonDocument.Parse(client.Pending[2].Envelope.Payload);
        Assert.Equal("original", refresh.RootElement.GetProperty("DetailedProfileId").GetString());
        client.Pending[2].Completion.SetResult(ListResponse(false, "original"));
        await WaitUntil(() => !Field<bool>(form, "_commandInProgress"));
        Assert.Equal("original", SelectedId(form, false));
    });

    private static Form CreateForm(bool audio, FakeClient client, List<string> errors)
        => audio ? new AudioProfilesForm(client, (message, _) => errors.Add(message)) : new DisplayProfileForm(client, (message, _) => errors.Add(message));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ApplyClickContainsTransportFailuresEvenAfterClosing(bool close) => RunSta(async () =>
    {
        FakeClient client = new FakeClient();
        List<string> errors = new List<string>();
        using DisplayProfileForm form = new DisplayProfileForm(client, (message, _) => errors.Add(message));
        Task loading = Refresh(form, false, "saved");
        client.Pending[0].Completion.SetResult(ListResponse(false, "saved"));
        await loading;
        typeof(DisplayProfileForm).GetMethod("Apply_Click", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, new object[] { form, EventArgs.Empty });
        if (close) form.Dispose();
        client.Pending[1].Completion.SetException(new IOException("Disconnected"));
        await WaitUntil(() => !Field<bool>(form, "_commandInProgress"));
        Assert.Equal(close ? 0 : 1, errors.Count);
        if (!close) Assert.True(Field<Button>(form, "btn_apply").Enabled);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellingFormClosureKeepsPendingRefreshUsable(bool audio) => RunSta(async () =>
    {
        FakeClient client = new FakeClient();
        using Form form = CreateForm(audio, client, new List<string>());
        Task loading = Refresh(form, audio, "saved");
        form.GetType().GetMethod("OnFormClosing", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, new object[] { new FormClosingEventArgs(CloseReason.UserClosing, true) });
        Assert.False(client.Pending[0].Token.IsCancellationRequested);
        client.Pending[0].Completion.SetResult(ListResponse(audio, "saved"));
        await loading;
        Assert.Equal("saved", SelectedId(form, audio));
    });

    private static Task Refresh(Form form, bool audio, string? id) => audio
        ? InvokeTask(form, "RefreshAudioProfilesAsync", id!)
        : InvokeTask(form, "RefreshProfilesAsync", id!, Field<CancellationToken>(form, "_lifetimeToken"), false);

    [Fact]
    public void CurrentViewRefreshFromAnotherThreadReturnsToTheUiThread() => RunSta(async () =>
    {
        FakeClient client = new FakeClient();
        using DisplayProfileForm form = new DisplayProfileForm(client, (_, _) => { });
        _ = form.Handle;
        int uiThreadId = Environment.CurrentManagedThreadId;
        await Task.Run(() => form.RefreshCurrentView());
        await WaitUntil(() => client.Pending.Count == 1);
        Assert.Equal(uiThreadId, client.LastRequestThreadId);
        using JsonDocument payload = JsonDocument.Parse(client.Pending[0].Envelope.Payload);
        Assert.True(payload.RootElement.GetProperty("IncludeCurrentLayoutDetail").GetBoolean());
        client.Pending[0].Completion.SetResult(new ControlResponse { IsSuccessful = true, ProfileList = new ProfileListResult { CurrentLayout = new DisplayProfileView { Id = "current", Name = "Current" } } });
        await WaitUntil(() => !Field<bool>(form, "_commandInProgress"));
        Assert.Equal("current", SelectedId(form, false));
        form.Dispose();
        await Task.Run(() => form.RefreshCurrentView());
        Assert.Single(client.Pending);
    });

    private static Task InvokeTask(Form form, string method, params object[] arguments)
        => (Task)form.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, arguments)!;

    private static T Field<T>(Form form, string name) => (T)form.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
    private static string? SelectedId(Form form, bool audio) => audio ? Field<AudioProfileView?>(form, "_selectedAudioProfile")?.Id : Field<DisplayProfileView?>(form, "_selectedProfile")?.Id;

    private static ControlResponse ListResponse(bool audio, string id) => audio
        ? new ControlResponse { IsSuccessful = true, AudioProfileList = new AudioProfileListResult { CanAccessAudioSettings = true, SavedProfiles = new[] { new AudioProfileView { Id = id, Name = id } } } }
        : new ControlResponse { IsSuccessful = true, ProfileList = new ProfileListResult { SavedProfiles = new[] { new DisplayProfileView { Id = id, Name = id, IsSaved = true, IsValid = true } } } };

    private static async Task WaitUntil(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition()) { Assert.True(DateTime.UtcNow < deadline, "The form action did not complete."); await Task.Delay(10); }
    }

    private static void RunSta(Func<Task> test) => WinFormsTestRunner.Run(test);

    private sealed class FakeClient : global::DisplayMagician.ControlServicePipeClient
    {
        public int LastRequestThreadId { get; private set; }
        public List<(ControlEnvelope Envelope, CancellationToken Token, TaskCompletionSource<ControlResponse> Completion)> Pending { get; } = new();
        protected override Task<ControlResponse> SendAsync(ControlEnvelope request, CancellationToken cancellationToken)
        {
            LastRequestThreadId = Environment.CurrentManagedThreadId;
            TaskCompletionSource<ControlResponse> completion = new TaskCompletionSource<ControlResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            Pending.Add((request, cancellationToken, completion));
            return completion.Task;
        }
    }
}
