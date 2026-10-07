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

        public OperationDecisionForm(OperationDecision decision)
        {
            ArgumentNullException.ThrowIfNull(decision);
            InitializeComponent();
            Text = string.IsNullOrWhiteSpace(decision.Title) ? "DisplayMagician needs your decision" : decision.Title;
            lbl_message.Text = decision.Message;
            btn_continue.Visible = Array.IndexOf(decision.AllowedChoices, OperationDecisionChoice.Continue) >= 0;
            btn_stop_and_restore.Visible = Array.IndexOf(decision.AllowedChoices, OperationDecisionChoice.StopAndRestore) >= 0;
            AcceptButton = btn_continue.Visible ? btn_continue : btn_stop_and_restore;
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
