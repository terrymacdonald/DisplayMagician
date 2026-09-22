using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;

namespace DisplayMagician.UIForms;

public partial class ServiceRecoveryForm : DisplayMagicianForm
{
    private readonly ControlServicePipeClient _controlServiceClient = new ControlServicePipeClient();

    public ServiceRecoveryForm()
    {
        InitializeComponent();
    }

    private async void ServiceRecoveryForm_Load(object sender, EventArgs e)
    {
        bool isAdministrator = IsElevatedAdministrator();
        btn_force_release.Enabled = false;
        if (!isAdministrator)
        {
            lbl_administrator_notice.Text = "Run DisplayMagician elevated as an administrator to release display control.";
        }

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
            ControlResponse response = await _controlServiceClient.ForceReleaseDisplayControlAsync(txt_confirmation.Text, CancellationToken.None);
            lbl_result.Text = response.Message;
            await RefreshStatusAsync();
        }
        catch (Exception ex)
        {
            lbl_result.Text = $"Could not clear the stuck display profile change: {ex.Message}";
        }
        finally
        {
            SetActionsEnabled(IsElevatedAdministrator());
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
        btn_force_release.Enabled = enabled && string.Equals(txt_confirmation.Text, "FORCE RELEASE", StringComparison.Ordinal);
    }

    private static bool IsElevatedAdministrator()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }

    private void btn_back_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void txt_confirmation_TextChanged(object sender, EventArgs e)
    {
        btn_force_release.Enabled = IsElevatedAdministrator() && string.Equals(txt_confirmation.Text, "FORCE RELEASE", StringComparison.Ordinal);
    }
}
