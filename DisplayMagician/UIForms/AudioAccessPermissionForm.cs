using System;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public partial class AudioAccessPermissionForm : Form
    {
        public AudioAccessPermissionForm()
        {
            InitializeComponent();
        }

        private void btn_continue_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // The explanatory dialog has no opt-out path: close and Continue both lead
            // directly to the Windows consent prompt, where the user makes the choice.
            DialogResult = DialogResult.OK;
            base.OnFormClosing(e);
        }
    }
}
