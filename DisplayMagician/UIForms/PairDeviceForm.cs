using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;
using QRCoder;

namespace DisplayMagician.UIForms;

public partial class PairDeviceForm : DisplayMagicianForm
{
    private readonly ControlServicePipeClient _controlServiceClient = new ControlServicePipeClient();
    private readonly GatewaySettings _settings;

    public PairDeviceForm(GatewaySettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        InitializeComponent();
    }

    private async void PairDeviceForm_Load(object sender, EventArgs e)
    {
        await RefreshQrCodeAsync();
        await RefreshRequestsAsync();
        await RefreshPairedDevicesAsync();
    }

    private async void rdo_gateway_location_CheckedChanged(object sender, EventArgs e)
    {
        if (rdo_local_lan.Checked || rdo_remote_network.Checked)
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
        if (e.RowIndex < 0 || e.ColumnIndex != col_paired_remove.Index || dgv_paired_devices.Rows[e.RowIndex].Tag is not PairedClientView client)
        {
            return;
        }

        if (MessageBox.Show(this, $"Remove {client.DisplayName}? This device will need to pair again before it can use DisplayMagician.", "Remove Paired Device", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
        {
            return;
        }

        ControlResponse response = await _controlServiceClient.RevokePairedClientAsync(client.DeviceId, CancellationToken.None);
        lbl_paired_result.Text = response.Message;
        await RefreshPairedDevicesAsync();
    }

    private async void btn_approve_request_Click(object sender, EventArgs e)
    {
        if (lv_pairing_requests.SelectedItems.Count != 1 || lv_pairing_requests.SelectedItems[0].Tag is not DevicePairingSessionView request)
        {
            return;
        }

        btn_approve_request.Enabled = false;
        try
        {
            ControlResponse response = await _controlServiceClient.ApproveDevicePairingAsync(new ApproveDevicePairingRequest { PairingSessionId = request.PairingSessionId, GrantedCapabilities = request.RequestedCapabilities }, CancellationToken.None);
            lbl_request_result.Text = response.Message;
            await RefreshRequestsAsync();
        }
        catch (Exception ex)
        {
            lbl_request_result.Text = $"Could not approve the device: {ex.Message}";
        }
        finally
        {
            btn_approve_request.Enabled = true;
        }
    }

    private async void btn_reject_request_Click(object sender, EventArgs e)
    {
        if (lv_pairing_requests.SelectedItems.Count != 1 || lv_pairing_requests.SelectedItems[0].Tag is not DevicePairingSessionView request)
        {
            return;
        }

        if (MessageBox.Show(this, $"Reject pairing for {request.DeviceDisplayName}?", "Reject Device Pairing", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
        {
            return;
        }

        btn_reject_request.Enabled = false;
        try
        {
            ControlResponse response = await _controlServiceClient.RejectDevicePairingAsync(request.PairingSessionId, CancellationToken.None);
            lbl_request_result.Text = response.Message;
            await RefreshRequestsAsync();
        }
        catch (Exception ex)
        {
            lbl_request_result.Text = $"Could not reject the device: {ex.Message}";
        }
        finally
        {
            btn_reject_request.Enabled = true;
        }
    }

    private async Task RefreshQrCodeAsync()
    {
        try
        {
            string gatewayUri = GetSelectedGatewayUri();
            DevicePairingQrCode qrCode = await _controlServiceClient.CreateDevicePairingQrAsync(gatewayUri, CancellationToken.None);
            string payload = $"displaymagician://pair?session={Uri.EscapeDataString(qrCode.PairingSessionId.ToString("D"))}&secret={Uri.EscapeDataString(qrCode.PairingSecret)}&gateway={Uri.EscapeDataString(qrCode.Gateway.GatewayUri)}&hostId={Uri.EscapeDataString(qrCode.Gateway.HostId)}&hostKey={Uri.EscapeDataString(qrCode.Gateway.HostIdentityPublicKeyJwk)}&tlsFingerprint={Uri.EscapeDataString(qrCode.Gateway.TlsCertificateSha256)}&expires={Uri.EscapeDataString(qrCode.ExpiresUtc.ToUniversalTime().ToString("O"))}";
            using QRCodeGenerator generator = new QRCodeGenerator();
            using QRCodeData data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode pngQrCode = new PngByteQRCode(data);
            using MemoryStream pngStream = new MemoryStream(pngQrCode.GetGraphic(8));
            Image image = new Bitmap(pngStream);
            Image previousImage = pb_qr_code.Image;
            pb_qr_code.Image = image;
            previousImage?.Dispose();
            lbl_qr_endpoint.Text = gatewayUri;
            lbl_qr_expiry.Text = $"Expires {qrCode.ExpiresUtc.ToLocalTime():g}";
            lbl_qr_result.Text = "Scan this code from the DisplayMagician client you want to pair.";
        }
        catch (Exception ex)
        {
            lbl_qr_result.Text = $"Could not create a pairing QR code: {ex.Message}";
            pb_qr_code.Image?.Dispose();
            pb_qr_code.Image = null;
        }
    }

    private async Task RefreshRequestsAsync()
    {
        try
        {
            DevicePairingSessionView[] requests = await _controlServiceClient.ListDevicePairingRequestsAsync(CancellationToken.None);
            lv_pairing_requests.BeginUpdate();
            lv_pairing_requests.Items.Clear();
            foreach (DevicePairingSessionView request in requests)
            {
                ListViewItem item = new ListViewItem(request.DeviceDisplayName) { Tag = request };
                item.SubItems.Add(string.Join(", ", request.RequestedCapabilities));
                item.SubItems.Add(request.ExpiresUtc.ToLocalTime().ToString("g"));
                lv_pairing_requests.Items.Add(item);
            }
            lv_pairing_requests.EndUpdate();
            lbl_request_result.Text = requests.Length == 0 ? "No devices are waiting for approval." : $"{requests.Length} device(s) waiting for approval.";
        }
        catch (Exception ex)
        {
            lbl_request_result.Text = $"Could not load pairing requests: {ex.Message}";
        }
    }

    private async Task RefreshPairedDevicesAsync()
    {
        try
        {
            PairedClientView[] clients = await _controlServiceClient.ListPairedClientsAsync(CancellationToken.None);
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
            lbl_paired_result.Text = $"Could not load paired devices: {ex.Message}";
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
