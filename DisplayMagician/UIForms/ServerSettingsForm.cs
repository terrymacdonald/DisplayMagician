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
    private readonly ControlServicePipeClient _controlServiceClient;
    private readonly Func<string, Task<int>> _runElevatedAction;
    private readonly Func<string, string, bool> _confirmAction;
    private readonly CancellationTokenSource _lifetimeSource = new CancellationTokenSource();
    private readonly CancellationToken _lifetimeToken;
    private bool _lifetimeDisposed;
    private bool _settingsLoaded;
    private bool _actionInProgress;
    private int _statusRefreshVersion;
    private int _settingsRefreshVersion;
    private int _actionVersion;
    private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    private bool _originalAllowPublicNetworks;

    public ServerSettingsForm() : this(new ControlServicePipeClient()) { }

    internal ServerSettingsForm(ControlServicePipeClient client, Func<string, Task<int>> runElevatedAction = null, Func<string, string, bool> confirmAction = null)
    {
        _controlServiceClient = client ?? throw new ArgumentNullException(nameof(client));
        _runElevatedAction = runElevatedAction;
        _confirmAction = confirmAction ?? ((message, title) => MessageBox.Show(this, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes);
        _lifetimeToken = _lifetimeSource.Token;
        InitializeComponent();
        Disposed += (_, _) =>
        {
            if (_lifetimeDisposed) return;
            _lifetimeDisposed = true;
            _lifetimeSource.Cancel();
            _lifetimeSource.Dispose();
        };
        SetActionsEnabled(true);
    }

    private bool CanUpdateView => !IsDisposed && !Disposing && !_lifetimeToken.IsCancellationRequested;

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (!e.Cancel && !_lifetimeDisposed) _lifetimeSource.Cancel();
    }

    private async void ServerSettingsForm_Load(object sender, EventArgs e)
    {
        btn_force_release.Enabled = true;
        btn_restart_user_agent.Enabled = true;
        btn_restart_control_service.Enabled = true;
        lbl_administrator_notice.Text = "Server settings and recovery actions require Windows administrator approval.";

        await RefreshStatusAsync();
        await LoadGatewaySettingsAsync();
    }

    private async void btn_refresh_Click(object sender, EventArgs e)
    {
        await RefreshStatusAsync();
        if (!_settingsLoaded) await LoadGatewaySettingsAsync();
    }

    private async Task LoadGatewaySettingsAsync()
    {
        if (!CanUpdateView) return;
        int refreshVersion = ++_settingsRefreshVersion;
        int actionVersion = _actionVersion;
        try
        {
            GatewaySettings settings = await _controlServiceClient.GetGatewaySettingsAsync(_lifetimeToken);
            if (!CanUpdateView || refreshVersion != _settingsRefreshVersion) return;
            LoadLanBindAddresses(settings.LanBindAddress);
            txt_lan_host.Text = settings.LanAdvertisedHost;
            txt_lan_port.Text = settings.LanPort.ToString();
            txt_remote_host.Text = settings.RemoteHost;
            txt_remote_port.Text = settings.RemotePort.ToString();
            chk_allow_public_networks.Checked = settings.AllowPublicNetworks;
            _originalAllowPublicNetworks = settings.AllowPublicNetworks;
            _settingsLoaded = true;
            if (actionVersion == _actionVersion) lbl_result.Text = "Gateway settings loaded.";
        }
        catch (Exception ex)
        {
            if (!CanUpdateView || refreshVersion != _settingsRefreshVersion) return;
            logger.Warn(ex, "ServerSettingsForm/LoadGatewaySettingsAsync: Could not load Gateway settings.");
            if (actionVersion == _actionVersion) lbl_result.Text = $"Could not load Gateway settings. Use Refresh to try again: {ex.Message}";
        }
        finally { SetActionsEnabled(true); }
    }

    private async void btn_force_release_Click(object sender, EventArgs e)
    {
        await RunServerActionAsync(async () =>
        {
            if (!_confirmAction("Do you want to clear the stuck display profile change?", "Clear Stuck Display Profile Change?"))
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
        });
    }

    private async void btn_restart_user_agent_Click(object sender, EventArgs e)
    {
        await RunServerActionAsync(async () =>
        {
            if (!_confirmAction("Restart the User Agent? This is available only when no display or shortcut operation is active.", "Restart User Agent"))
            {
                return;
            }

            SetActionsEnabled(false);
            try
            {
                ControlResponse response = await _controlServiceClient.RestartUserAgentAsync(CancellationToken.None);
                if (CanUpdateView) lbl_result.Text = response.Message;
                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "ServerSettingsForm/btn_restart_user_agent_Click: Could not restart the User Agent.");
                if (CanUpdateView) lbl_result.Text = $"Could not restart the User Agent: {ex.Message}";
            }
            finally
            {
                SetActionsEnabled(true);
            }
        });
    }

    private async void btn_restart_control_service_Click(object sender, EventArgs e)
    {
        await RunServerActionAsync(async () =>
        {
            if (!_confirmAction("Restart Control Service? This interrupts DisplayMagician operations for every user on this computer.", "Restart Control Service"))
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
                        if (CanUpdateView) lbl_result.Text = restartExitCode == (int)Program.ERRORLEVEL.OK
                            ? "Control Service and User Agent restarted successfully."
                            : "Control Service and User Agent restarted, but the recovery history could not be recorded.";
                    }
                    else
                    {
                        if (CanUpdateView) lbl_result.Text = $"Control Service restarted, but the User Agent could not restart: {userAgentResponse.Message}";
                    }
                }
                await RefreshStatusAsync();
            }
            finally
            {
                SetActionsEnabled(true);
            }
        });
    }

    private async Task<int> RunElevatedRecoveryActionAsync(string commandLineOption, string successMessage, string failureMessage, string cancelledMessage = null)
    {
        try
        {
            int exitCode;
            if (_runElevatedAction != null)
            {
                exitCode = await _runElevatedAction(commandLineOption);
            }
            else
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(Application.ExecutablePath, commandLineOption)
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using Process elevatedProcess = Process.Start(startInfo) ?? throw new InvalidOperationException("DisplayMagician could not start the elevated recovery action.");
                await elevatedProcess.WaitForExitAsync();
                exitCode = elevatedProcess.ExitCode;
            }
            if (CanUpdateView) lbl_result.Text = exitCode == (int)Program.ERRORLEVEL.OK ? successMessage : failureMessage;
            return exitCode;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            if (CanUpdateView) lbl_result.Text = cancelledMessage ?? "Administrator approval was cancelled. No recovery action was performed.";
            return (int)Program.ERRORLEVEL.CANCELED_BY_USER;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "ServerSettingsForm/RunElevatedRecoveryActionAsync: Could not start the elevated recovery action {0}.", commandLineOption);
            if (CanUpdateView) lbl_result.Text = $"{failureMessage} {ex.Message}";
            return (int)Program.ERRORLEVEL.ERROR_EXCEPTION;
        }
    }

    private async Task RefreshStatusAsync()
    {
        if (!CanUpdateView) return;
        int refreshVersion = ++_statusRefreshVersion;
        try
        {
            ControlServiceStatus status = await _controlServiceClient.GetServiceStatusAsync(_lifetimeToken);
            if (!CanUpdateView || refreshVersion != _statusRefreshVersion) return;
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
            if (!CanUpdateView || refreshVersion != _statusRefreshVersion) return;
            logger.Warn(ex, "ServerSettingsForm/RefreshStatusAsync: Could not load Control Service status.");
            lbl_status.Text = $"The Control Service status is unavailable: {ex.Message}";
        }
    }

    private void SetActionsEnabled(bool enabled)
    {
        if (!CanUpdateView) return;
        enabled = enabled && !_actionInProgress;
        btn_refresh.Enabled = true;
        btn_force_release.Enabled = enabled;
        btn_restart_user_agent.Enabled = enabled;
        btn_restart_control_service.Enabled = enabled;
        btn_replace_gateway_tls_key.Enabled = enabled;
        btn_apply_gateway_settings.Enabled = enabled && _settingsLoaded;
        btn_pair_remote_device.Enabled = enabled && _settingsLoaded;
        cbo_lan_bind_address.Enabled = enabled && _settingsLoaded;
        txt_lan_host.Enabled = enabled && _settingsLoaded;
        txt_lan_port.Enabled = enabled && _settingsLoaded;
        txt_remote_host.Enabled = enabled && _settingsLoaded;
        txt_remote_port.Enabled = enabled && _settingsLoaded;
        chk_allow_public_networks.Enabled = enabled && _settingsLoaded;
    }

    private async Task RunServerActionAsync(Func<Task> action)
    {
        if (!CanUpdateView || _actionInProgress) return;
        ++_actionVersion;
        _actionInProgress = true;
        SetActionsEnabled(false);
        try { await action(); }
        catch (Exception ex)
        {
            logger.Error(ex, "ServerSettingsForm/RunServerActionAsync: Could not complete the requested server action.");
            if (CanUpdateView) lbl_result.Text = $"Could not complete the server action: {ex.Message}";
        }
        finally
        {
            _actionInProgress = false;
            SetActionsEnabled(true);
        }
    }

    private void btn_back_Click(object sender, EventArgs e)
    {
        Close();
    }

    private async void btn_apply_gateway_settings_Click(object sender, EventArgs e)
    {
        await RunServerActionAsync(async () =>
        {
            if (!_settingsLoaded || !CanUpdateView) return;
            if (!int.TryParse(txt_lan_port.Text, out int lanPort) || lanPort is < 1 or > 65535 ||
                !int.TryParse(txt_remote_port.Text, out int remotePort) || remotePort is < 1 or > 65535)
            {
                if (CanUpdateView) lbl_result.Text = "Gateway ports must be numbers between 1 and 65535.";
                return;
            }

            GatewaySettings requested = new GatewaySettings
            {
                LanBindAddress = cbo_lan_bind_address.SelectedItem?.ToString() ?? "*",
                LanAdvertisedHost = txt_lan_host.Text, LanPort = lanPort,
                AllowPublicNetworks = chk_allow_public_networks.Checked,
                RemoteHost = txt_remote_host.Text, RemotePort = remotePort
            };
            if (requested.AllowPublicNetworks && !_originalAllowPublicNetworks &&
                !_confirmAction("Allow Gateway access on Public Windows networks? Devices on those networks can reach its HTTPS port, but must still pair and authenticate. This setting does not configure router port forwarding.", "Allow Public Network Access?"))
                return;

            // Preserve the submitted save/restart sequence if this dialog closes; closure only stops UI updates.
            ControlResponse response = await _controlServiceClient.UpdateGatewaySettingsAsync(requested, CancellationToken.None);
            if (!response.IsSuccessful)
            {
                if (CanUpdateView) lbl_result.Text = response.Message;
                return;
            }
            _originalAllowPublicNetworks = requested.AllowPublicNetworks;
            await RunElevatedRecoveryActionAsync(Program.RestartGatewayCommandLineOption,
                "Gateway settings saved and Gateway restarted.", "Gateway settings saved, but Gateway could not restart.",
                "Gateway settings saved, but administrator approval to restart Gateway was cancelled.");
        });
    }

    private async void btn_pair_remote_device_Click(object sender, EventArgs e)
    {
        await RunServerActionAsync(async () =>
        {
            if (!_settingsLoaded || !CanUpdateView) return;
            try
            {
                GatewaySettings settings = await _controlServiceClient.GetGatewaySettingsAsync(_lifetimeToken);
                if (!CanUpdateView) return;
                using PairDeviceForm pairDeviceForm = new PairDeviceForm(settings);
                pairDeviceForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "ServerSettingsForm/btn_pair_remote_device_Click: Could not open the device pairing dialog.");
                if (CanUpdateView) lbl_result.Text = $"Could not open device pairing: {ex.Message}";
            }
        });
    }

    private async void btn_replace_gateway_tls_key_Click(object sender, EventArgs e)
    {
        await RunServerActionAsync(async () =>
        {
            if (!_confirmAction("Replace the Gateway TLS private key? Every paired phone will lose access and must be paired again. Use this only to recover from a lost or compromised key.", "Replace Gateway TLS Key?"))
            {
                return;
            }

            SetActionsEnabled(false);
            try
            {
                ControlResponse approval = await _controlServiceClient.ApproveGatewayTlsKeyReplacementAsync(CancellationToken.None);
                if (!approval.IsSuccessful)
                {
                    if (CanUpdateView) lbl_result.Text = approval.Message;
                    return;
                }

                await RunElevatedRecoveryActionAsync(Program.RestartGatewayCommandLineOption, "Gateway restarted after TLS key replacement approval. Paired devices must pair again once the new identity is active.", "Gateway TLS key replacement was approved, but Gateway did not restart. Restart it within ten minutes or approve the replacement again.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "ServerSettingsForm/btn_replace_gateway_tls_key_Click: Could not approve Gateway TLS key replacement.");
                if (CanUpdateView) lbl_result.Text = "Gateway TLS key replacement could not be approved.";
            }
            finally
            {
                SetActionsEnabled(true);
            }
        });
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
