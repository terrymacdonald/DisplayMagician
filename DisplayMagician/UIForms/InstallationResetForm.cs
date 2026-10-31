using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;

namespace DisplayMagician.UIForms
{
    public partial class InstallationResetForm : Form
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Func<Task<ControlResponse>> _reset;
        private bool _pending;

        public InstallationResetForm() : this(InstallationResetClient.ResetAsync) { }

        internal InstallationResetForm(Func<Task<ControlResponse>> reset)
        {
            _reset = reset;
            InitializeComponent();
        }

        private async void btn_reset_Click(object sender, EventArgs e)
        {
            if (_pending) return;
            _pending = true;
            btn_reset.Enabled = false;
            btn_close.Enabled = false;
            lbl_status.Text = "Stopping DisplayMagician and restoring active shortcuts, then resetting all application data. Please wait...";
            try
            {
                ControlResponse result = await _reset();
                lbl_status.Text = result.Message;
                if (!result.IsSuccessful) btn_reset.Enabled = true;
                else btn_close.Text = "Close";
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "InstallationResetForm/btn_reset_Click: Installation reset did not complete successfully.");
                lbl_status.Text = "Reset could not be confirmed: " + ex.Message;
                btn_reset.Enabled = true;
            }
            finally { _pending = false; btn_close.Enabled = true; }
        }

        private void InstallationResetForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_pending) e.Cancel = true;
        }

        private void btn_close_Click(object sender, EventArgs e) => Close();
    }
}
