using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;
using QRCoder;

namespace DisplayMagician.UIForms;

public partial class PairDeviceForm : DisplayMagicianForm
{
    private readonly ControlServicePipeClient _controlServiceClient;
    private readonly GatewaySettings _settings;
    private readonly Func<string, string, bool> _confirmAction;
    private readonly CancellationTokenSource _lifetimeSource = new CancellationTokenSource();
    private readonly CancellationToken _lifetimeToken;
    private bool _lifetimeDisposed;
    private bool _loaded;
    private bool _actionInProgress;
    private int _qrRefreshVersion;
    private int _requestsRefreshVersion;
    private int _clientsRefreshVersion;
    private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public PairDeviceForm(GatewaySettings settings) : this(settings, new ControlServicePipeClient()) { }

    internal PairDeviceForm(GatewaySettings settings, ControlServicePipeClient client, Func<string, string, bool> confirmAction = null)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _controlServiceClient = client ?? throw new ArgumentNullException(nameof(client));
        _confirmAction = confirmAction ?? ((message, title) => MessageBox.Show(this, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes);
        _lifetimeToken = _lifetimeSource.Token;
        InitializeComponent();
        lv_pairing_requests.SelectedIndexChanged += (_, _) => SetActionsEnabled();
        Disposed += (_, _) =>
        {
            if (_lifetimeDisposed) return;
            _lifetimeDisposed = true;
            _lifetimeSource.Cancel();
            _lifetimeSource.Dispose();
        };
        SetActionsEnabled();
    }

    private bool CanUpdateView => !IsDisposed && !Disposing && !_lifetimeToken.IsCancellationRequested;

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (!e.Cancel && !_lifetimeDisposed) _lifetimeSource.Cancel();
    }

    private async void PairDeviceForm_Load(object sender, EventArgs e)
    {
        _loaded = true;
        await RefreshQrCodeAsync();
        await RefreshRequestsAsync();
        await RefreshPairedDevicesAsync();
    }

    private async void rdo_gateway_location_CheckedChanged(object sender, EventArgs e)
    {
        if (_loaded && (rdo_local_lan.Checked || rdo_remote_network.Checked))
        {
            await RefreshQrCodeAsync();
        }
    }

    private async void btn_refresh_requests_Click(object sender, EventArgs e)
    {
        await RefreshRequestsAsync();
    }

    private async void btn_refresh_paired_devices_Click(object sender, EventArgs e)
    {
        await RefreshPairedDevicesAsync();
    }

    private async void dgv_paired_devices_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        if (_actionInProgress || !CanUpdateView || e.RowIndex < 0 || e.RowIndex >= dgv_paired_devices.Rows.Count || e.ColumnIndex != col_paired_remove.Index || dgv_paired_devices.Rows[e.RowIndex].Tag is not PairedClientView client)
        {
            return;
        }

        if (!_confirmAction($"Remove {client.DisplayName}? This device will need to pair again before it can use DisplayMagician.", "Remove Paired Device"))
        {
            return;
        }

        await RunPairingActionAsync(async () =>
        {
            ControlResponse response = await _controlServiceClient.RevokePairedClientAsync(client.DeviceId, _lifetimeToken);
            if (!CanUpdateView) return;
            lbl_paired_result.Text = response.Message;
            await RefreshPairedDevicesAsync();
            if (CanUpdateView && !response.IsSuccessful) lbl_paired_result.Text = response.Message;
        }, lbl_paired_result);
    }

    private async void btn_approve_request_Click(object sender, EventArgs e)
    {
        if (_actionInProgress || !CanUpdateView || lv_pairing_requests.SelectedItems.Count != 1 || lv_pairing_requests.SelectedItems[0].Tag is not DevicePairingSessionView request)
        {
            return;
        }

        await RunPairingActionAsync(async () =>
        {
            ControlResponse response = await _controlServiceClient.ApproveDevicePairingAsync(new ApproveDevicePairingRequest { PairingSessionId = request.PairingSessionId, GrantedCapabilities = request.RequestedCapabilities }, _lifetimeToken);
            if (!CanUpdateView) return;
            lbl_request_result.Text = response.Message;
            await RefreshRequestsAsync();
            if (CanUpdateView && !response.IsSuccessful) lbl_request_result.Text = response.Message;
        }, lbl_request_result);
    }

    private async void btn_reject_request_Click(object sender, EventArgs e)
    {
        if (_actionInProgress || !CanUpdateView || lv_pairing_requests.SelectedItems.Count != 1 || lv_pairing_requests.SelectedItems[0].Tag is not DevicePairingSessionView request)
        {
            return;
        }

        if (!_confirmAction($"Reject pairing for {request.DeviceDisplayName}?", "Reject Device Pairing"))
        {
            return;
        }

        await RunPairingActionAsync(async () =>
        {
            ControlResponse response = await _controlServiceClient.RejectDevicePairingAsync(request.PairingSessionId, _lifetimeToken);
            if (!CanUpdateView) return;
            lbl_request_result.Text = response.Message;
            await RefreshRequestsAsync();
            if (CanUpdateView && !response.IsSuccessful) lbl_request_result.Text = response.Message;
        }, lbl_request_result);
    }

    private async Task RefreshQrCodeAsync()
    {
        if (!CanUpdateView) return;
        int refreshVersion = ++_qrRefreshVersion;
        Image previous = pb_qr_code.Image;
        pb_qr_code.Image = null;
        previous?.Dispose();
        lbl_qr_endpoint.Text = string.Empty;
        lbl_qr_expiry.Text = string.Empty;
        lbl_qr_result.Text = "Creating a pairing QR code...";
        try
        {
            string gatewayUri = GetSelectedGatewayUri();
            DevicePairingQrCode qrCode = await _controlServiceClient.CreateDevicePairingQrAsync(gatewayUri, _lifetimeToken);
            if (!CanUpdateView || refreshVersion != _qrRefreshVersion) return;
            if (qrCode.ExpiresUtc <= DateTime.UtcNow)
            {
                lbl_qr_result.Text = "This pairing QR code has expired. Select the gateway location again to request another code.";
                return;
            }
            byte[] pairingPayload = JsonSerializer.SerializeToUtf8Bytes(new
            {
                version = 1,
                gatewayUri = qrCode.Gateway.GatewayUri,
                hostId = qrCode.Gateway.HostId,
                hostIdentityPublicKeyJwk = qrCode.Gateway.HostIdentityPublicKeyJwk,
                tlsSpkiSha256 = qrCode.Gateway.TlsSpkiSha256,
                pairingRequestId = qrCode.PairingSessionId,
                pairingSecret = qrCode.PairingSecret,
                expiresAt = qrCode.ExpiresUtc.ToUniversalTime()
            });
            string payload = "displaymagician://pair?payload=" + Convert.ToBase64String(pairingPayload).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            using QRCodeGenerator generator = new QRCodeGenerator();
            using QRCodeData data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode pngQrCode = new PngByteQRCode(data);
            using MemoryStream pngStream = new MemoryStream(pngQrCode.GetGraphic(8));
            using Image decoded = Image.FromStream(pngStream);
            Image image = new Bitmap(decoded);
            Image previousImage = pb_qr_code.Image;
            pb_qr_code.Image = image;
            previousImage?.Dispose();
            lbl_qr_endpoint.Text = gatewayUri;
            lbl_qr_expiry.Text = $"Expires {qrCode.ExpiresUtc.ToLocalTime():g}";
            lbl_qr_result.Text = "Scan this code from the DisplayMagician client you want to pair.";
        }
        catch (Exception ex)
        {
            if (!CanUpdateView || refreshVersion != _qrRefreshVersion) return;
            logger.Warn(ex, "PairDeviceForm/RefreshQrCodeAsync: Could not create a pairing QR code.");
            lbl_qr_result.Text = $"Could not create a pairing QR code: {ex.Message}";
            pb_qr_code.Image?.Dispose();
            pb_qr_code.Image = null;
        }
    }

    private async Task RefreshRequestsAsync()
    {
        if (!CanUpdateView) return;
        int refreshVersion = ++_requestsRefreshVersion;
        try
        {
            DevicePairingSessionView[] requests = await _controlServiceClient.ListDevicePairingRequestsAsync(_lifetimeToken);
            if (!CanUpdateView || refreshVersion != _requestsRefreshVersion) return;
            lv_pairing_requests.BeginUpdate();
            try
            {
                lv_pairing_requests.Items.Clear();
                foreach (DevicePairingSessionView request in requests)
                {
                    ListViewItem item = new ListViewItem(request.DeviceDisplayName) { Tag = request };
                    item.SubItems.Add(string.Join(", ", request.RequestedCapabilities));
                    item.SubItems.Add(request.ExpiresUtc.ToLocalTime().ToString("g"));
                    lv_pairing_requests.Items.Add(item);
                }
            }
            finally { lv_pairing_requests.EndUpdate(); }
            lbl_request_result.Text = requests.Length == 0 ? "No devices are waiting for approval." : $"{requests.Length} device(s) waiting for approval.";
        }
        catch (Exception ex)
        {
            if (!CanUpdateView || refreshVersion != _requestsRefreshVersion) return;
            logger.Warn(ex, "PairDeviceForm/RefreshRequestsAsync: Could not load pending pairing requests.");
            lbl_request_result.Text = $"Could not load pairing requests: {ex.Message}";
        }
    }

    private async Task RefreshPairedDevicesAsync()
    {
        if (!CanUpdateView) return;
        int refreshVersion = ++_clientsRefreshVersion;
        try
        {
            PairedClientView[] clients = await _controlServiceClient.ListPairedClientsAsync(_lifetimeToken);
            if (!CanUpdateView || refreshVersion != _clientsRefreshVersion) return;
            dgv_paired_devices.Rows.Clear();
            foreach (PairedClientView client in clients)
            {
                int rowIndex = dgv_paired_devices.Rows.Add(client.DisplayName, client.ClientType, client.LastKnownIpAddress, client.ConnectedSinceUtc.ToLocalTime().ToString("g"));
                dgv_paired_devices.Rows[rowIndex].Tag = client;
            }
            lbl_paired_result.Text = clients.Length == 0 ? "No devices are currently paired." : $"{clients.Length} paired device(s).";
        }
        catch (Exception ex)
        {
            if (!CanUpdateView || refreshVersion != _clientsRefreshVersion) return;
            logger.Warn(ex, "PairDeviceForm/RefreshPairedDevicesAsync: Could not load paired devices.");
            lbl_paired_result.Text = $"Could not load paired devices: {ex.Message}";
        }
    }

    private void SetActionsEnabled()
    {
        if (!CanUpdateView) return;
        bool hasRequest = lv_pairing_requests.SelectedItems.Count == 1;
        btn_approve_request.Enabled = !_actionInProgress && hasRequest;
        btn_reject_request.Enabled = !_actionInProgress && hasRequest;
        btn_refresh_requests.Enabled = !_actionInProgress;
        btn_refresh_paired_devices.Enabled = !_actionInProgress;
        dgv_paired_devices.Enabled = !_actionInProgress;
    }

    private async Task RunPairingActionAsync(Func<Task> action, Label resultLabel)
    {
        if (_actionInProgress || !CanUpdateView) return;
        _actionInProgress = true;
        SetActionsEnabled();
        try { await action(); }
        catch (OperationCanceledException) when (_lifetimeToken.IsCancellationRequested) { /* The dialog closed while waiting. */ }
        catch (Exception ex)
        {
            logger.Warn(ex, "PairDeviceForm/RunPairingActionAsync: Could not complete the pairing action.");
            if (CanUpdateView) resultLabel.Text = $"Could not complete the pairing action: {ex.Message}";
        }
        finally
        {
            _actionInProgress = false;
            SetActionsEnabled();
        }
    }

    private string GetSelectedGatewayUri()
    {
        string host = rdo_remote_network.Checked ? _settings.RemoteHost : _settings.LanAdvertisedHost;
        int port = rdo_remote_network.Checked ? _settings.RemotePort : _settings.LanPort;
        if (string.IsNullOrWhiteSpace(host) || port is < 1 or > 65535)
        {
            throw new InvalidOperationException(rdo_remote_network.Checked ? "Set a Remote Network host and port in Server Settings first." : "Set a LAN host and port in Server Settings first.");
        }

        return new UriBuilder(Uri.UriSchemeHttps, host.Trim(), port).Uri.AbsoluteUri.TrimEnd('/');
    }

    private void btn_close_Click(object sender, EventArgs e)
    {
        Close();
    }
}
