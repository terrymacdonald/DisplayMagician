using System;
using System.Windows.Forms;
using DisplayMagician.Contracts;

namespace DisplayMagician.UIForms
{
    /// <summary>Displays an operation decision which may also be answered by another associated client.</summary>
    internal partial class OperationDecisionForm : DisplayMagicianForm
    {
        public OperationDecisionChoice SelectedChoice { get; private set; } = OperationDecisionChoice.Continue;
        public bool WasResolvedByAnotherClient { get; private set; }
        public bool HasExpired { get; private set; }
        public DateTime ExpiresUtc { get; }

        public OperationDecisionForm(OperationDecision decision)
        {
            ArgumentNullException.ThrowIfNull(decision);
            InitializeComponent();
            ExpiresUtc = decision.ExpiresUtc;
            Text = string.IsNullOrWhiteSpace(decision.Title) ? "DisplayMagician needs your decision" : decision.Title;
            lbl_message.Text = decision.Message;
            btn_continue.Visible = Array.IndexOf(decision.AllowedChoices, OperationDecisionChoice.Continue) >= 0;
            btn_stop_and_restore.Visible = Array.IndexOf(decision.AllowedChoices, OperationDecisionChoice.StopAndRestore) >= 0;
            AcceptButton = btn_continue.Visible ? btn_continue : btn_stop_and_restore;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (ExpiresUtc <= DateTime.UtcNow) deadline_timer_Tick(this, EventArgs.Empty);
            else deadline_timer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            HasExpired |= ExpiresUtc <= DateTime.UtcNow;
            base.OnFormClosing(e);
            if (!e.Cancel) deadline_timer.Stop();
        }

        private void deadline_timer_Tick(object sender, EventArgs e)
        {
            if (ExpiresUtc > DateTime.UtcNow) return;
            HasExpired = true;
            deadline_timer.Stop();
            Close();
        }

        private void btn_continue_Click(object sender, EventArgs e)
        {
            SelectedChoice = OperationDecisionChoice.Continue;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btn_stop_and_restore_Click(object sender, EventArgs e)
        {
            SelectedChoice = OperationDecisionChoice.StopAndRestore;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public void CloseBecauseAnotherClientResponded()
        {
            WasResolvedByAnotherClient = true;
            Close();
        }
    }
}
