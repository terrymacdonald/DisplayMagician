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
using Markdig;

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
        public string ChangelogURL { get; set; } = "https://github.com/terrymacdonald/DisplayMagician/releases";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ReleaseNotesHtml { get; set; } = string.Empty;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ReleaseNotesFormat { get; set; } = "md";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ReleaseHeading { get; set; } = "DisplayMagician update available";

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
            lbl_release_heading.Text = ReleaseHeading;
            if (!string.IsNullOrWhiteSpace(ReleaseNotesHtml))
            {
                try
                {
                    await web_release_notes.EnsureCoreWebView2Async();
                    web_release_notes.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                    web_release_notes.CoreWebView2.Settings.AreDevToolsEnabled = false;
                    string htmlBody = string.Equals(ReleaseNotesFormat, "html", StringComparison.OrdinalIgnoreCase)
                        ? ReleaseNotesHtml
                        : Markdown.ToHtml(ReleaseNotesHtml, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
                    string htmlDocument = $"<!DOCTYPE html><html><head><meta charset='utf-8'><style>body{{font-family:'Segoe UI',sans-serif;padding:20px;line-height:1.45;color:#1a1a1a;}} pre{{background:#f4f4f4;padding:10px;overflow:auto;}} code{{font-family:Consolas,monospace;}} table{{border-collapse:collapse;}} th,td{{border:1px solid #ddd;padding:6px 8px;}}</style></head><body>{htmlBody}</body></html>";
                    web_release_notes.NavigateToString(htmlDocument);
                    web_release_notes.Show();
                    rtb_message.Hide();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "UpgradeForm/UpgradeForm_Load: WebView2 failed to render release notes; showing fallback text.");
                    rtb_message.Text = "The release notes could not be displayed. You can use the changelog link below for more information.";
                    rtb_message.Show();
                }
            }
            else
            {
                rtb_message.Text = "Release notes for this update are not available locally. You can use the changelog link below for more information.";
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
