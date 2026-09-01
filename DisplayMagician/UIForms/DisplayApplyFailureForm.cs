using System;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    internal enum DisplayApplyFailureAction
    {
        Retry,
        RunWithoutChangingDisplays,
        Cancel
    }

    internal enum DisplayApplyFailureContext
    {
        Shortcut,
        DisplayProfile
    }

    internal partial class DisplayApplyFailureForm : Form
    {
        public DisplayApplyFailureAction SelectedAction { get; private set; } = DisplayApplyFailureAction.Cancel;

        public DisplayApplyFailureForm(string profileName, bool timedOut, int timeoutSeconds)
            : this(profileName, timedOut, timeoutSeconds, DisplayApplyFailureContext.Shortcut)
        {
        }

        public DisplayApplyFailureForm(string profileName, bool timedOut, int timeoutSeconds, DisplayApplyFailureContext context)
        {
            InitializeComponent();

            string result = timedOut ? $"did not finish within {timeoutSeconds} seconds" : "could not be applied";
            if (context == DisplayApplyFailureContext.DisplayProfile)
            {
                btn_run_without_display_change.Visible = false;
                btn_retry.Location = new System.Drawing.Point(btn_cancel.Left - btn_retry.Width - 6, btn_retry.Top);
                lbl_message.Text = $"DisplayMagician {result} for the '{profileName}' display profile.{Environment.NewLine}{Environment.NewLine}" +
                    "Check your displays, switchers, and inputs, then retry or cancel." +
                    (timedOut ? $"{Environment.NewLine}{Environment.NewLine}Windows does not provide a safe way to cancel an in-progress display change, so DisplayMagician will wait for it to finish before retrying." : String.Empty);
            }
            else
            {
                lbl_message.Text = $"DisplayMagician {result} for the '{profileName}' display profile.{Environment.NewLine}{Environment.NewLine}" +
                    "You can retry after changing a switch, input, or display power state; run the shortcut using your current display layout; or cancel." +
                    (timedOut ? $"{Environment.NewLine}{Environment.NewLine}Windows does not provide a safe way to cancel an in-progress display change, so DisplayMagician will wait for it to finish before retrying or restoring your layout." : String.Empty);
            }
        }

        private void btn_retry_Click(object sender, EventArgs e)
        {
            SelectedAction = DisplayApplyFailureAction.Retry;
            DialogResult = DialogResult.Retry;
            Close();
        }

        private void btn_run_without_display_change_Click(object sender, EventArgs e)
        {
            SelectedAction = DisplayApplyFailureAction.RunWithoutChangingDisplays;
            DialogResult = DialogResult.Ignore;
            Close();
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            SelectedAction = DisplayApplyFailureAction.Cancel;
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
