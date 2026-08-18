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
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public UpgradeForm()
        {
            InitializeComponent();
            //wb_changelog.Navigate(_changelogWebsite);
            lnk_changelog.Text = ChangelogURL;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Remind { get; set;  } = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Message { get; set; } = "You have an upgrade available for DisplayMagician. Do you wish to upgrade now?";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ChangelogURL { get; set; } = "https://github.com/terrymacdonald/DisplayMagician/releases";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ReleaseNotesHtml { get; set; } = string.Empty;

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

        private async void UpgradeForm_Load(object sender, EventArgs e)
        {
            lnk_changelog.Text = ChangelogURL;
            if (!string.IsNullOrWhiteSpace(ReleaseNotesHtml))
            {
                try
                {
                    await web_release_notes.EnsureCoreWebView2Async();
                    web_release_notes.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                    web_release_notes.CoreWebView2.Settings.AreDevToolsEnabled = false;
                    web_release_notes.NavigateToString(ReleaseNotesHtml);
                    web_release_notes.Show();
                    rtb_message.Hide();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "UpgradeForm/UpgradeForm_Load: WebView2 failed to render release notes; showing fallback text.");
                    rtb_message.Text = ReleaseNotesHtml;
                    rtb_message.Show();
                }
            }
            else if (Message.StartsWith(@"{\rtf", StringComparison.OrdinalIgnoreCase))
            {
                rtb_message.Rtf = Message;
                rtb_message.Show();
            }
            else
            {
                rtb_message.Text = Message;
                rtb_message.Show();
            }

            CenterToParent();
        }

        private void lnk_changelog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProcessUtils.StartProcess(ChangelogURL, "", ProcessPriority.Normal);
        }
    }
}
