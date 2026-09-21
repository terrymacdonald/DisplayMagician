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
        btn_create_bundle.Enabled = isAdministrator;
        btn_force_release.Enabled = false;
        if (!isAdministrator)
        {
            lbl_administrator_notice.Text = "Run DisplayMagician elevated as an administrator to create a machine bundle or release display control.";
        }

        await RefreshStatusAsync();
    }

    private async void btn_refresh_Click(object sender, EventArgs e)
    {
        await RefreshStatusAsync();
    }

    private async void btn_create_bundle_Click(object sender, EventArgs e)
    {
        SetActionsEnabled(false);
        try
        {
            ControlResponse response = await _controlServiceClient.CreateDiagnosticBundleAsync(CancellationToken.None);
            lbl_result.Text = response.IsSuccessful
                ? $"Machine diagnostic bundle created: {response.DiagnosticBundlePath}"
                : response.Message;
        }
        catch (Exception ex)
        {
            lbl_result.Text = $"Could not create the diagnostic bundle: {ex.Message}";
        }
        finally
        {
            SetActionsEnabled(IsElevatedAdministrator());
        }
    }

    private async void btn_force_release_Click(object sender, EventArgs e)
    {
        DialogResult confirmation = MessageBox.Show(this, "Force release clears the machine display-control lease without restoring any interrupted display or shortcut operation. Continue only after checking the affected session.", "Force Release Display Control", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
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
            lbl_result.Text = $"Could not release display control: {ex.Message}";
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
        btn_refresh.Enabled = enabled;
        btn_create_bundle.Enabled = enabled;
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