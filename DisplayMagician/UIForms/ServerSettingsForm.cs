using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;

namespace DisplayMagician.UIForms;

public partial class ServerSettingsForm : DisplayMagicianForm
{
    private readonly ControlServicePipeClient _controlServiceClient = new ControlServicePipeClient();
    private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public ServerSettingsForm()
    {
        InitializeComponent();
    }

    private async void ServerSettingsForm_Load(object sender, EventArgs e)
    {
        btn_force_release.Enabled = true;
        btn_restart_user_agent.Enabled = true;
        btn_restart_control_service.Enabled = true;
        lbl_administrator_notice.Text = "Server settings and recovery actions require Windows administrator approval.";

        await RefreshStatusAsync();
        GatewaySettings settings = await _controlServiceClient.GetGatewaySettingsAsync(CancellationToken.None);
        LoadLanBindAddresses(settings.LanBindAddress);
        txt_lan_host.Text = settings.LanAdvertisedHost;
        txt_lan_port.Text = settings.LanPort.ToString();
        txt_remote_host.Text = settings.RemoteHost;
        txt_remote_port.Text = settings.RemotePort.ToString();
    }

    private async void btn_refresh_Click(object sender, EventArgs e)
    {
        await RefreshStatusAsync();
    }

    private async void btn_force_release_Click(object sender, EventArgs e)
    {
        DialogResult confirmation = MessageBox.Show(this, "Do you want to clear the stuck display profile change?", "Clear Stuck Display Profile Change?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        SetActionsEnabled(false);
        try
        {
            await RunElevatedRecoveryActionAsync(Program.ForceReleaseDisplayControlCommandLineOption, "The emergency release request completed.", "The emergency release request did not complete. Check the DisplayMagician log for details.");
            await RefreshStatusAsync();
        }
        finally
        {
            SetActionsEnabled(true);
        }
    }

    private async void btn_restart_user_agent_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show(this, "Restart the User Agent? This is available only when no display or shortcut operation is active.", "Restart User Agent", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
        {
            return;
        }

        SetActionsEnabled(false);
        try
        {
            ControlResponse response = await _controlServiceClient.RestartUserAgentAsync(CancellationToken.None);
            lbl_result.Text = response.Message;
            await RefreshStatusAsync();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "ServerSettingsForm/btn_restart_user_agent_Click: Could not restart the User Agent.");
            lbl_result.Text = $"Could not restart the User Agent: {ex.Message}";
        }
        finally
        {
            SetActionsEnabled(true);
        }
    }

    private async void btn_restart_control_service_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show(this, "Restart Control Service? This interrupts DisplayMagician operations for every user on this computer.", "Restart Control Service", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
        {
            return;
        }

        SetActionsEnabled(false);
        try
        {
            int restartExitCode = await RunElevatedRecoveryActionAsync(Program.RestartControlServiceCommandLineOption, "Control Service restarted successfully.", "Control Service did not restart. Check the DisplayMagician log for details.");
            bool restarted = restartExitCode == (int)Program.ERRORLEVEL.OK || restartExitCode == (int)Program.ERRORLEVEL.ERROR_RECOVERY_HISTORY_NOT_RECORDED;
            if (restarted)
            {
                await Task.Delay(1000);
                ControlResponse userAgentResponse = await _controlServiceClient.RestartUserAgentAsync(CancellationToken.None);
                if (userAgentResponse.IsSuccessful)
                {
                    lbl_result.Text = restartExitCode == (int)Program.ERRORLEVEL.OK
                        ? "Control Service and User Agent restarted successfully."
                        : "Control Service and User Agent restarted, but the recovery history could not be recorded.";
                }
                else
                {
                    lbl_result.Text = $"Control Service restarted, but the User Agent could not restart: {userAgentResponse.Message}";
                }
            }
            await RefreshStatusAsync();
        }
        finally
        {
            SetActionsEnabled(true);
        }
    }

    private async Task<int> RunElevatedRecoveryActionAsync(string commandLineOption, string successMessage, string failureMessage)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(Application.ExecutablePath, commandLineOption)
            {
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using Process elevatedProcess = Process.Start(startInfo) ?? throw new InvalidOperationException("DisplayMagician could not start the elevated recovery action.");
            await elevatedProcess.WaitForExitAsync();
            bool succeeded = elevatedProcess.ExitCode == (int)Program.ERRORLEVEL.OK;
            lbl_result.Text = succeeded ? successMessage : failureMessage;
            return elevatedProcess.ExitCode;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            lbl_result.Text = "Administrator approval was cancelled. No recovery action was performed.";
            return (int)Program.ERRORLEVEL.CANCELED_BY_USER;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "ServerSettingsForm/RunElevatedRecoveryActionAsync: Could not start the elevated recovery action {0}.", commandLineOption);
            lbl_result.Text = $"Could not start the recovery action: {ex.Message}";
            return (int)Program.ERRORLEVEL.ERROR_EXCEPTION;
        }
    }

    private async Task RefreshStatusAsync()
    {
        try
        {
            ControlServiceStatus status = await _controlServiceClient.GetServiceStatusAsync(CancellationToken.None);
            DisplayControlLease lease = status.DisplayControlLease;
            string latestAdministration = status.LatestRecoveryAdministration == null
                ? "No emergency recovery action has been recorded."
                : $"Last recovery administration: {status.LatestRecoveryAdministration.Action} at {status.LatestRecoveryAdministration.OccurredUtc:u}; outcome: {status.LatestRecoveryAdministration.Outcome}.";
            string recoveryHistory = status.RecoveryAdministrations.Length > 1
                ? $" Recovery history contains {status.RecoveryAdministrations.Length} actions."
                : string.Empty;
            lbl_status.Text = lease == null
                ? $"Display control is not currently leased. {latestAdministration}{recoveryHistory}"
                : $"Display control is leased by {lease.OwnerUserSid}, session {lease.OwnerSessionId}. Last heartbeat: {lease.LastHeartbeatUtc:u}. Recovery required: {lease.IsRecoveryRequired}. Active operation: {lease.ActiveOperationId?.ToString() ?? "none"}. {latestAdministration}{recoveryHistory}";
        }
        catch (Exception ex)
        {
            lbl_status.Text = $"The Control Service status is unavailable: {ex.Message}";
        }
    }

    private void SetActionsEnabled(bool enabled)
    {
        btn_refresh.Enabled = true;
        btn_force_release.Enabled = enabled;
        btn_restart_user_agent.Enabled = enabled;
        btn_restart_control_service.Enabled = enabled;
    }

    private void btn_back_Click(object sender, EventArgs e)
    {
        Close();
    }

    private async void btn_apply_gateway_settings_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(txt_lan_port.Text, out int lanPort) || !int.TryParse(txt_remote_port.Text, out int remotePort))
        {
            lbl_result.Text = "Gateway ports must be valid numbers.";
            return;
        }

        ControlResponse response = await _controlServiceClient.UpdateGatewaySettingsAsync(new GatewaySettings { LanBindAddress = cbo_lan_bind_address.SelectedItem?.ToString() ?? "*", LanAdvertisedHost = txt_lan_host.Text, LanPort = lanPort, RemoteHost = txt_remote_host.Text, RemotePort = remotePort }, CancellationToken.None);
        if (!response.IsSuccessful)
        {
            lbl_result.Text = response.Message;
            return;
        }

        using Process process = Process.Start(new ProcessStartInfo(Application.ExecutablePath, Program.RestartGatewayCommandLineOption) { UseShellExecute = true, Verb = "runas", WindowStyle = ProcessWindowStyle.Hidden })!;
        await process.WaitForExitAsync();
        lbl_result.Text = process.ExitCode == (int)Program.ERRORLEVEL.OK ? "Gateway settings saved and Gateway restarted." : "Gateway settings saved, but Gateway could not restart.";
    }

    private async void btn_pair_remote_device_Click(object sender, EventArgs e)
    {
        try
        {
            GatewaySettings settings = await _controlServiceClient.GetGatewaySettingsAsync(CancellationToken.None);
            using PairDeviceForm pairDeviceForm = new PairDeviceForm(settings);
            pairDeviceForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "ServerSettingsForm/btn_pair_remote_device_Click: Could not open the device pairing dialog.");
            lbl_result.Text = $"Could not open device pairing: {ex.Message}";
        }
    }

    private void LoadLanBindAddresses(string selectedAddress)
    {
        string[] addresses = NetworkInterface.GetAllNetworkInterfaces()
            .Where(networkInterface => networkInterface.OperationalStatus == OperationalStatus.Up)
            .SelectMany(networkInterface => networkInterface.GetIPProperties().UnicastAddresses)
            .Select(unicastAddress => unicastAddress.Address)
            .Where(address => address.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6 && !System.Net.IPAddress.IsLoopback(address))
            .Select(address => address.ToString())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(address => address, StringComparer.OrdinalIgnoreCase)
            .Prepend("*")
            .ToArray();

        cbo_lan_bind_address.Items.Clear();
        cbo_lan_bind_address.Items.AddRange(addresses);
        string selected = string.IsNullOrWhiteSpace(selectedAddress) ? "*" : selectedAddress;
        if (!cbo_lan_bind_address.Items.Contains(selected))
        {
            cbo_lan_bind_address.Items.Add(selected);
        }

        cbo_lan_bind_address.SelectedItem = selected;
    }

}
