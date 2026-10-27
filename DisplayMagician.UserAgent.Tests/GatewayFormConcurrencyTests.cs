using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;
using DisplayMagician.UIForms;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class GatewayFormConcurrencyTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OldQrReplyCannotReplaceOrClearTheNewEndpoint(bool oldFails) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using PairDeviceForm form = PairForm(client);
        Task old = Call(form, "RefreshQrCodeAsync");
        Field<RadioButton>(form, "rdo_remote_network").Checked = true;
        Task current = Call(form, "RefreshQrCodeAsync");
        client.Pending[1].Completion.SetResult(Qr("https://remote.invalid:9443"));
        await current;
        System.Drawing.Image? image = Field<PictureBox>(form, "pb_qr_code").Image;
        Assert.NotNull(image);
        if (oldFails) client.Pending[0].Completion.SetException(new IOException("Old failure"));
        else client.Pending[0].Completion.SetResult(Qr("https://lan.invalid:8443"));
        await old;
        Assert.Same(image, Field<PictureBox>(form, "pb_qr_code").Image);
        Assert.Equal("https://remote.invalid:9443", Field<Label>(form, "lbl_qr_endpoint").Text);
        Assert.Contains("Scan", Field<Label>(form, "lbl_qr_result").Text);
    });

    [Fact]
    public void SwitchingEndpointsImmediatelyRemovesThePreviousQr() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using PairDeviceForm form = PairForm(client);
        Task initial = Call(form, "RefreshQrCodeAsync");
        client.Pending[0].Completion.SetResult(Qr("https://lan.invalid:8443"));
        await initial;
        Assert.NotNull(Field<PictureBox>(form, "pb_qr_code").Image);
        Field<RadioButton>(form, "rdo_remote_network").Checked = true;
        Task next = Call(form, "RefreshQrCodeAsync");
        Assert.Null(Field<PictureBox>(form, "pb_qr_code").Image);
        Assert.Empty(Field<Label>(form, "lbl_qr_endpoint").Text);
        client.Pending[1].Completion.SetException(new IOException("Disconnected"));
        await next;
        Assert.Null(Field<PictureBox>(form, "pb_qr_code").Image);
    });

    [Fact]
    public void ExpiredQrRepliesAreNotPresentedForScanning() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using PairDeviceForm form = PairForm(client);
        Task loading = Call(form, "RefreshQrCodeAsync");
        ControlResponse response = Qr("https://lan.invalid:8443");
        response.DevicePairingQrCode!.ExpiresUtc = DateTime.UtcNow.AddMinutes(-1);
        client.Pending[0].Completion.SetResult(response);
        await loading;
        Assert.Null(Field<PictureBox>(form, "pb_qr_code").Image);
        Assert.Contains("expired", Field<Label>(form, "lbl_qr_result").Text);
    });

    [Theory]
    [InlineData("RefreshRequestsAsync")]
    [InlineData("RefreshPairedDevicesAsync")]
    public void OlderPairingListsCannotReplaceNewerLists(string method) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using PairDeviceForm form = PairForm(client);
        Task old = Call(form, method);
        Task current = Call(form, method);
        client.Pending[1].Completion.SetResult(ListReply(method, "New"));
        await current;
        client.Pending[0].Completion.SetResult(ListReply(method, "Old"));
        await old;
        if (method == "RefreshRequestsAsync") Assert.Equal("New", Assert.Single(Field<ListView>(form, "lv_pairing_requests").Items.Cast<ListViewItem>()).Text);
        else Assert.Equal("New", Field<DataGridView>(form, "dgv_paired_devices").Rows[0].Cells[0].Value);
    });

    [Theory]
    [InlineData("RefreshQrCodeAsync", false)]
    [InlineData("RefreshQrCodeAsync", true)]
    [InlineData("RefreshRequestsAsync", false)]
    [InlineData("RefreshRequestsAsync", true)]
    [InlineData("RefreshPairedDevicesAsync", false)]
    [InlineData("RefreshPairedDevicesAsync", true)]
    [InlineData("RefreshStatusAsync", false)]
    [InlineData("RefreshStatusAsync", true)]
    [InlineData("LoadGatewaySettingsAsync", false)]
    [InlineData("LoadGatewaySettingsAsync", true)]
    public void ClosingIgnoresLateReadRepliesAndCancelsWaits(string method, bool fail) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using Form form = method is "RefreshStatusAsync" or "LoadGatewaySettingsAsync" ? ServerForm(client) : PairForm(client);
        string labelName = method switch { "RefreshQrCodeAsync" => "lbl_qr_result", "RefreshRequestsAsync" => "lbl_request_result", "RefreshPairedDevicesAsync" => "lbl_paired_result", "RefreshStatusAsync" => "lbl_status", _ => "lbl_result" };
        Task pending = Call(form, method);
        string before = Field<Label>(form, labelName).Text;
        form.Dispose();
        Assert.True(client.Pending[0].Token.IsCancellationRequested);
        if (fail) client.Pending[0].Completion.SetException(new IOException("Disconnected after closing"));
        else client.Pending[0].Completion.SetResult(method switch { "RefreshQrCodeAsync" => Qr("https://lan.invalid:8443"), "RefreshStatusAsync" => Status("Late"), "LoadGatewaySettingsAsync" => Settings("Late"), _ => ListReply(method, "Late") });
        await pending;
        Assert.Equal(before, Field<Label>(form, labelName).Text);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OldServerStatusCannotReplaceTheNewestResult(bool oldFails) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using ServerSettingsForm form = ServerForm(client);
        Task old = Call(form, "RefreshStatusAsync");
        Task current = Call(form, "RefreshStatusAsync");
        client.Pending[1].Completion.SetResult(Status("New"));
        await current;
        if (oldFails) client.Pending[0].Completion.SetException(new IOException("Old failure"));
        else client.Pending[0].Completion.SetResult(Status("Old"));
        await old;
        Assert.Contains("New", Field<Label>(form, "lbl_status").Text);
        Assert.DoesNotContain("Old", Field<Label>(form, "lbl_status").Text);
    });

    [Theory]
    [InlineData("io")]
    [InlineData("timeout")]
    [InlineData("service")]
    public void FailedSettingsLoadBlocksSavingButKeepsRecoveryAndRetryAvailable(string kind) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using ServerSettingsForm form = ServerForm(client);
        Task loading = Call(form, "LoadGatewaySettingsAsync");
        client.Pending[0].Completion.SetException(kind == "io" ? new IOException("Disconnected") : kind == "timeout" ? new TimeoutException("Timeout") : new InvalidOperationException("Unavailable"));
        await loading;
        Assert.False(Field<Button>(form, "btn_apply_gateway_settings").Enabled);
        Assert.True(Field<Button>(form, "btn_force_release").Enabled);
        Click(form, "btn_apply_gateway_settings_Click");
        Assert.Single(client.Pending);
        Task retry = Call(form, "LoadGatewaySettingsAsync");
        client.Pending[1].Completion.SetResult(Settings("Loaded"));
        await retry;
        Assert.True(Field<Button>(form, "btn_apply_gateway_settings").Enabled);
        Assert.Equal("Loaded", Field<TextBox>(form, "txt_lan_host").Text);
        Assert.Equal("Gateway settings loaded.", Field<Label>(form, "lbl_result").Text);
    });

    [Fact]
    public void OlderSettingsLoadCannotOverwriteTheCurrentValues() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using ServerSettingsForm form = ServerForm(client);
        Task old = Call(form, "LoadGatewaySettingsAsync");
        Task current = Call(form, "LoadGatewaySettingsAsync");
        client.Pending[1].Completion.SetResult(Settings("New"));
        await current;
        client.Pending[0].Completion.SetResult(Settings("Old"));
        await old;
        Assert.Equal("New", Field<TextBox>(form, "txt_lan_host").Text);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GatewaySaveIsSerializedAndRestartsOnlyAfterSuccessfulSave(bool accepted) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        List<string> elevated = new List<string>();
        TaskCompletionSource<int> restart = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ServerSettingsForm form = new ServerSettingsForm(client, option => { elevated.Add(option); return restart.Task; }, (_, _) => true);
        await Load(form, client);
        Field<TextBox>(form, "txt_lan_host").Text = "Submitted";
        Click(form, "btn_apply_gateway_settings_Click");
        Click(form, "btn_apply_gateway_settings_Click");
        Assert.Equal(2, client.Pending.Count);
        Assert.False(Field<Button>(form, "btn_apply_gateway_settings").Enabled);
        Field<CheckBox>(form, "chk_allow_public_networks").Checked = true;
        using JsonDocument payload = JsonDocument.Parse(client.Pending[1].Envelope.Payload);
        Assert.Equal("Submitted", payload.RootElement.GetProperty("LanAdvertisedHost").GetString());
        Assert.False(payload.RootElement.GetProperty("AllowPublicNetworks").GetBoolean());
        client.Pending[1].Completion.SetResult(new ControlResponse { IsSuccessful = accepted, Message = "Rejected" });
        if (accepted)
        {
            await Until(() => elevated.Count == 1);
            Assert.Equal(global::DisplayMagician.Program.RestartGatewayCommandLineOption, elevated[0]);
            restart.SetResult(0);
        }
        await Until(() => !Field<bool>(form, "_actionInProgress"));
        Assert.Equal(accepted ? 1 : 0, elevated.Count);
        Assert.False(Field<bool>(form, "_originalAllowPublicNetworks"));
        Assert.True(Field<Button>(form, "btn_apply_gateway_settings").Enabled);
    });

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GatewayRestartFailureKeepsTheSavedSettingsOutcomeVisible(bool cancelled) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using ServerSettingsForm form = new ServerSettingsForm(client, _ => Task.FromException<int>(cancelled ? new Win32Exception(1223) : new IOException("Launch failed")), (_, _) => true);
        await Load(form, client);
        Click(form, "btn_apply_gateway_settings_Click");
        client.Pending[1].Completion.SetResult(new ControlResponse { IsSuccessful = true });
        await Until(() => !Field<bool>(form, "_actionInProgress"));
        string result = Field<Label>(form, "lbl_result").Text;
        Assert.Contains("settings saved", result);
        Assert.Contains(cancelled ? "cancelled" : "could not restart", result);
    });

    [Fact]
    public void ClosedServerStillFinishesAnAlreadySubmittedSaveAndRestart() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        int elevations = 0;
        using ServerSettingsForm form = new ServerSettingsForm(client, _ => { elevations++; return Task.FromResult(0); }, (_, _) => true);
        await Load(form, client);
        Click(form, "btn_apply_gateway_settings_Click");
        string before = Field<Label>(form, "lbl_result").Text;
        form.Dispose();
        Assert.False(client.Pending[1].Token.IsCancellationRequested);
        client.Pending[1].Completion.SetResult(new ControlResponse { IsSuccessful = true });
        await Until(() => !Field<bool>(form, "_actionInProgress"));
        Assert.Equal(1, elevations);
        Assert.Equal(before, Field<Label>(form, "lbl_result").Text);
    });

    [Theory]
    [InlineData("0", "8443")]
    [InlineData("8443", "65536")]
    public void InvalidPortsAreRejectedBeforeSaving(string lan, string remote) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using ServerSettingsForm form = ServerForm(client);
        await Load(form, client);
        Field<TextBox>(form, "txt_lan_port").Text = lan;
        Field<TextBox>(form, "txt_remote_port").Text = remote;
        Click(form, "btn_apply_gateway_settings_Click");
        Assert.Single(client.Pending);
        Assert.Contains("between 1 and 65535", Field<Label>(form, "lbl_result").Text);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RevokeContainsTransportErrorsAndRejectsOverlappingClicks(bool close) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using PairDeviceForm form = PairForm(client);
        Task loading = Call(form, "RefreshPairedDevicesAsync");
        client.Pending[0].Completion.SetResult(ListReply("RefreshPairedDevicesAsync", "Device"));
        await loading;
        DataGridViewCellEventArgs cell = new(Field<DataGridViewColumn>(form, "col_paired_remove").Index, 0);
        Event(form, "dgv_paired_devices_CellContentClick", cell);
        Event(form, "dgv_paired_devices_CellContentClick", cell);
        Assert.Equal(2, client.Pending.Count);
        string before = Field<Label>(form, "lbl_paired_result").Text;
        if (close) form.Dispose();
        client.Pending[1].Completion.SetException(new IOException("Disconnected"));
        await Until(() => !Field<bool>(form, "_actionInProgress"));
        Assert.Equal(2, client.Pending.Count);
        if (close) Assert.Equal(before, Field<Label>(form, "lbl_paired_result").Text);
        else Assert.Contains("Could not complete", Field<Label>(form, "lbl_paired_result").Text);
    });

    [Fact]
    public void RecoveryActionsCannotOverlap() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        TaskCompletionSource<int> release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int elevations = 0;
        using ServerSettingsForm form = new ServerSettingsForm(client, _ => { elevations++; return release.Task; }, (_, _) => true);
        Click(form, "btn_force_release_Click");
        Click(form, "btn_restart_user_agent_Click");
        Assert.Equal(1, elevations);
        Assert.Empty(client.Pending);
        release.SetResult(0);
        await Until(() => client.Pending.Count == 1);
        Assert.Equal(ControlMessageType.GetServiceStatus, client.Pending[0].Envelope.MessageType);
        client.Pending[0].Completion.SetResult(Status("Owner"));
        await Until(() => !Field<bool>(form, "_actionInProgress"));
        Assert.True(Field<Button>(form, "btn_force_release").Enabled);
    });

    private static PairDeviceForm PairForm(FakeClient client) => new(new GatewaySettings { LanAdvertisedHost = "lan.invalid", LanPort = 8443, RemoteHost = "remote.invalid", RemotePort = 9443 }, client, (_, _) => true);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ApproveAndRejectCannotOverlapAndKeepTheOriginalRequest(bool approveFirst) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using PairDeviceForm form = PairForm(client);
        ListView list = Field<ListView>(form, "lv_pairing_requests");
        _ = list.Handle;
        Guid id = Guid.NewGuid();
        ListViewItem item = new("Candidate") { Tag = new DevicePairingSessionView { PairingSessionId = id, RequestedCapabilities = new[] { "status.read" } } };
        list.Items.Add(item);
        item.Selected = true;
        Assert.Single(list.SelectedItems.Cast<ListViewItem>());
        Click(form, approveFirst ? "btn_approve_request_Click" : "btn_reject_request_Click");
        Click(form, approveFirst ? "btn_reject_request_Click" : "btn_approve_request_Click");
        Assert.Single(client.Pending);
        Assert.Equal(approveFirst ? ControlMessageType.ApproveDevicePairing : ControlMessageType.RejectDevicePairing, client.Pending[0].Envelope.MessageType);
        using JsonDocument payload = JsonDocument.Parse(client.Pending[0].Envelope.Payload);
        Assert.Equal(id, payload.RootElement.GetProperty("PairingSessionId").GetGuid());
        if (approveFirst) Assert.Equal("status.read", payload.RootElement.GetProperty("GrantedCapabilities")[0].GetString());
        client.Pending[0].Completion.SetException(new IOException("Disconnected"));
        await Until(() => !Field<bool>(form, "_actionInProgress"));
        Assert.Single(client.Pending);
        Assert.Contains("Could not complete", Field<Label>(form, "lbl_request_result").Text);
    });

    [Fact]
    public void DecliningPublicNetworkConfirmationDoesNotSaveOrRestart() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        int elevations = 0;
        using ServerSettingsForm form = new ServerSettingsForm(client, _ => { elevations++; return Task.FromResult(0); }, (_, _) => false);
        await Load(form, client);
        Field<CheckBox>(form, "chk_allow_public_networks").Checked = true;
        Click(form, "btn_apply_gateway_settings_Click");
        Assert.Single(client.Pending);
        Assert.Equal(0, elevations);
        Assert.False(Field<bool>(form, "_actionInProgress"));
        Assert.False(Field<bool>(form, "_originalAllowPublicNetworks"));
    });

    [Fact]
    public void AgentRestartFailureAfterServiceRestartIsContainedByTheEventHandler() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using ServerSettingsForm form = ServerForm(client);
        Click(form, "btn_restart_control_service_Click");
        await Until(() => client.Pending.Count == 1);
        Assert.Equal(ControlMessageType.RestartUserAgent, client.Pending[0].Envelope.MessageType);
        client.Pending[0].Completion.SetException(new IOException("Disconnected"));
        await Until(() => !Field<bool>(form, "_actionInProgress"));
        Assert.Contains("Could not complete", Field<Label>(form, "lbl_result").Text);
        Assert.True(Field<Button>(form, "btn_restart_control_service").Enabled);
    });

    [Fact]
    public void StartupContainsBothStatusAndSettingsFailures() => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using ServerSettingsForm form = ServerForm(client);
        Click(form, "ServerSettingsForm_Load");
        client.Pending[0].Completion.SetException(new IOException("Status unavailable"));
        await Until(() => client.Pending.Count == 2);
        Assert.Equal(ControlMessageType.GetGatewaySettings, client.Pending[1].Envelope.MessageType);
        client.Pending[1].Completion.SetException(new TimeoutException("Settings unavailable"));
        await Until(() => Field<Label>(form, "lbl_result").Text.Contains("Could not load Gateway settings"));
        Assert.False(Field<Button>(form, "btn_apply_gateway_settings").Enabled);
        Assert.True(Field<Button>(form, "btn_force_release").Enabled);
    });
    private static ServerSettingsForm ServerForm(FakeClient client) => new(client, _ => Task.FromResult(0), (_, _) => true);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BackgroundSettingsLoadDoesNotHideANewerRecoveryOutcome(bool loadFails) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using ServerSettingsForm form = ServerForm(client);
        Task loading = Call(form, "LoadGatewaySettingsAsync");
        Click(form, "btn_force_release_Click");
        Assert.Equal(2, client.Pending.Count);
        client.Pending[1].Completion.SetResult(Status("Owner"));
        await Until(() => !Field<bool>(form, "_actionInProgress"));
        string outcome = Field<Label>(form, "lbl_result").Text;
        Assert.Contains("emergency release request completed", outcome);
        if (loadFails) client.Pending[0].Completion.SetException(new IOException("Settings unavailable"));
        else client.Pending[0].Completion.SetResult(Settings("Loaded"));
        await loading;
        Assert.Equal(outcome, Field<Label>(form, "lbl_result").Text);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PairingListRefreshDoesNotHideARejectedDecision(bool approve) => WinFormsTestRunner.Run(async () =>
    {
        FakeClient client = new FakeClient();
        using PairDeviceForm form = PairForm(client);
        ListView list = Field<ListView>(form, "lv_pairing_requests");
        _ = list.Handle;
        ListViewItem item = new("Candidate") { Tag = new DevicePairingSessionView { PairingSessionId = Guid.NewGuid() } };
        list.Items.Add(item);
        item.Selected = true;
        Click(form, approve ? "btn_approve_request_Click" : "btn_reject_request_Click");
        Assert.Single(client.Pending);
        client.Pending[0].Completion.SetResult(new ControlResponse { IsSuccessful = false, Message = "The pairing request expired." });
        await Until(() => client.Pending.Count == 2);
        client.Pending[1].Completion.SetResult(new ControlResponse { IsSuccessful = true });
        await Until(() => !Field<bool>(form, "_actionInProgress"));
        Assert.Equal("The pairing request expired.", Field<Label>(form, "lbl_request_result").Text);
    });
    private static async Task Load(ServerSettingsForm form, FakeClient client)
    {
        Task loading = Call(form, "LoadGatewaySettingsAsync");
        client.Pending.Last().Completion.SetResult(Settings("Loaded"));
        await loading;
    }
    private static ControlResponse Settings(string host) => new() { IsSuccessful = true, GatewaySettings = new GatewaySettings { LanAdvertisedHost = host, LanPort = 8443, RemoteHost = "remote.invalid", RemotePort = 9443 } };
    private static ControlResponse Status(string owner) => new() { IsSuccessful = true, ServiceStatus = new ControlServiceStatus { DisplayControlLease = new DisplayControlLease { OwnerUserSid = owner } } };
    private static ControlResponse Qr(string uri) => new() { IsSuccessful = true, DevicePairingQrCode = new DevicePairingQrCode { PairingSessionId = Guid.NewGuid(), PairingSecret = "dummy-test-secret", ExpiresUtc = DateTime.UtcNow.AddMinutes(5), Gateway = new GatewayPairingIdentity { GatewayUri = uri, HostId = "test-host", HostIdentityPublicKeyJwk = "{}", TlsSpkiSha256 = "test-pin" } } };
    private static ControlResponse ListReply(string method, string name) => method == "RefreshRequestsAsync"
        ? new ControlResponse { IsSuccessful = true, DevicePairingRequests = new[] { new DevicePairingSessionView { PairingSessionId = Guid.NewGuid(), DeviceDisplayName = name } } }
        : new ControlResponse { IsSuccessful = true, PairedClients = new[] { new PairedClientView { DeviceId = name, DisplayName = name } } };
    private static T Field<T>(Form form, string name) => (T)form.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
    private static Task Call(Form form, string method) => (Task)form.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, null)!;
    private static void Click(Form form, string method) => Event(form, method, EventArgs.Empty);
    private static void Event(Form form, string method, EventArgs args) => form.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, new object[] { form, args });
    private static async Task Until(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition()) { Assert.True(DateTime.UtcNow < deadline, "The dialog action did not complete."); await Task.Delay(10); }
    }
    private sealed class FakeClient : global::DisplayMagician.ControlServicePipeClient
    {
        public List<(ControlEnvelope Envelope, CancellationToken Token, TaskCompletionSource<ControlResponse> Completion)> Pending { get; } = new();
        protected override Task<ControlResponse> SendAsync(ControlEnvelope request, CancellationToken cancellationToken)
        {
            TaskCompletionSource<ControlResponse> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Pending.Add((request, cancellationToken, completion));
            return completion.Task;
        }
    }
}
