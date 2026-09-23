using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;

namespace DisplayMagician.UIForms;

public partial class ServiceRecoveryForm : DisplayMagicianForm
{
    private readonly ControlServicePipeClient _controlServiceClient = new ControlServicePipeClient();
    private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public ServiceRecoveryForm()
    {
        InitializeComponent();
    }

    private async void ServiceRecoveryForm_Load(object sender, EventArgs e)
    {
        btn_force_release.Enabled = true;
        btn_restart_user_agent.Enabled = true;
        btn_restart_control_service.Enabled = true;
        lbl_administrator_notice.Text = "Restarting Control Service or releasing stuck display control requests Windows administrator approval.";

        await RefreshStatusAsync();
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
            logger.Error(ex, "ServiceRecoveryForm/btn_restart_user_agent_Click: Could not restart the User Agent.");
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
            bool restarted = await RunElevatedRecoveryActionAsync(Program.RestartControlServiceCommandLineOption, "Control Service restarted successfully.", "Control Service did not restart. Check the DisplayMagician log for details.");
            if (restarted)
            {
                await Task.Delay(1000);
                ControlResponse userAgentResponse = await _controlServiceClient.RestartUserAgentAsync(CancellationToken.None);
                lbl_result.Text = userAgentResponse.IsSuccessful
                    ? "Control Service and User Agent restarted successfully."
                    : $"Control Service restarted, but the User Agent could not restart: {userAgentResponse.Message}";
            }
            await RefreshStatusAsync();
        }
        finally
        {
            SetActionsEnabled(true);
        }
    }

    private async Task<bool> RunElevatedRecoveryActionAsync(string commandLineOption, string successMessage, string failureMessage)
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
            return succeeded;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            lbl_result.Text = "Administrator approval was cancelled. No recovery action was performed.";
            return false;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "ServiceRecoveryForm/RunElevatedRecoveryActionAsync: Could not start the elevated recovery action {0}.", commandLineOption);
            lbl_result.Text = $"Could not start the recovery action: {ex.Message}";
            return false;
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
            lbl_status.Text = lease == null
                ? $"Display control is not currently leased. {latestAdministration}"
                : $"Display control is leased by {lease.OwnerUserSid}, session {lease.OwnerSessionId}. Last heartbeat: {lease.LastHeartbeatUtc:u}. Recovery required: {lease.IsRecoveryRequired}. Active operation: {lease.ActiveOperationId?.ToString() ?? "none"}. {latestAdministration}";
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

}
