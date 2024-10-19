using DisplayMagician.Processes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public partial class UpgradeForm : Form
    {
        public UpgradeForm()
        {
            InitializeComponent();
            //wb_changelog.Navigate(_changelogWebsite);
            lnk_changelog.Text = ChangelogURL;
        }

        public bool Remind { get; set;  } = false;

        public string Message { get; set; } = "You have an upgrade available for DisplayMagician. Do you wish to upgrade now?";

        public string ChangelogURL { get; set; } = "https://github.com/terrymacdonald/DisplayMagician/releases";

        private void btn_skip_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Remind = false; 
            this.Close();
        }

        private void btn_upgrade_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Remind = false;
            this.Close();
        }

        private void btn_remind_later_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Remind = true;
            this.Close();
        }

        private void UpgradeForm_Load(object sender, EventArgs e)
        {
            rtb_message.Rtf = Message;
            Utils.CenterOnPrimaryScreen(this);
        }

        private void lnk_changelog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string targetURL = @"https://github.com/terrymacdonald/DisplayMagician/releases";
            ProcessUtils.StartProcess(targetURL, "", ProcessPriority.Normal);    
        }
    }
}
