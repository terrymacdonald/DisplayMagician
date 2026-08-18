using DisplayMagician.Messaging;
using Markdig;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public partial class MessagesForm : Form
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private WebView2 webView;
        private readonly bool _selectNewestUnreadOnLoad;

        private List<LocalMessage> _messages = new List<LocalMessage>();

        public MessagesForm() : this(false)
        {
        }

        public MessagesForm(bool selectNewestUnreadOnLoad)
        {
            _selectNewestUnreadOnLoad = selectNewestUnreadOnLoad;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadMessagesIntoList();
            InitializeWebViewIfNeeded();
            SelectInitialMessageIfNeeded();
        }

        private void InitializeWebViewIfNeeded()
        {
            if (webView != null)
            {
                return;
            }

            try
            {
                webView = new WebView2
                {
                    Dock = DockStyle.Fill,
                    Visible = false,
                };
                rightPanel.Controls.Add(webView);
                webView.BringToFront();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "MessagesForm/InitializeWebViewIfNeeded: WebView2 failed to initialise; fallback label will be used.");
            }
        }

        private void LoadMessagesIntoList()
        {
            _messages = Program.GetStoredMessages();
            lv_messages.Items.Clear();

            Font unreadFont = new Font(lv_messages.Font, FontStyle.Bold);
            Font readFont = new Font(lv_messages.Font, FontStyle.Regular);

            foreach (LocalMessage message in _messages)
            {
                bool isReleaseAnnouncement = string.Equals(message.Kind, "releaseAnnouncement", StringComparison.OrdinalIgnoreCase);
                ListViewItem item = new ListViewItem(isReleaseAnnouncement ? $"Update: {message.Title}" : message.Title)
                {
                    Name = message.Id,
                    Tag = message,
                    Font = message.IsRead ? readFont : unreadFont,
                    ToolTipText = isReleaseAnnouncement
                        ? $"Release update {message.ReleaseVersion} ({message.ReleaseChannel})"
                        : message.Title,
                };

                item.SubItems.Add(message.ReceivedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
                lv_messages.Items.Add(item);
            }

            int unreadCount = _messages.Count(m => !m.IsRead);
            lbl_count.Text = $"{_messages.Count} messages ({unreadCount} unread)";
        }

        private void lv_messages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv_messages.SelectedItems.Count == 0)
            {
                btn_upgrade.Enabled = false;
                return;
            }

            List<string> selectedIds = lv_messages.SelectedItems
                .Cast<ListViewItem>()
                .Select(i => i.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Program.SetMessageReadState(selectedIds, true);
            LoadMessagesIntoList();
            RestoreSelection(selectedIds);

            if (lv_messages.SelectedItems.Count > 0)
            {
                LocalMessage selectedMessage = lv_messages.SelectedItems[0].Tag as LocalMessage;
                RenderMessage(selectedMessage);
                btn_upgrade.Enabled = IsApplicableReleaseAnnouncement(selectedMessage);
            }

            Program.RefreshMessageIndicators();
        }

        private void btn_mark_read_Click(object sender, EventArgs e)
        {
            ApplyReadStateForSelection(isRead: true);
        }

        private void btn_mark_unread_Click(object sender, EventArgs e)
        {
            ApplyReadStateForSelection(isRead: false);
        }

        private void btn_upgrade_Click(object sender, EventArgs e)
        {
            LocalMessage selectedMessage = lv_messages.SelectedItems.Count == 1
                ? lv_messages.SelectedItems[0].Tag as LocalMessage
                : null;

            if (!IsApplicableReleaseAnnouncement(selectedMessage))
            {
                return;
            }

            Program.CheckForUpdates(automatic: false);
        }

        private void ApplyReadStateForSelection(bool isRead)
        {
            List<string> selectedIds = lv_messages.SelectedItems
                .Cast<ListViewItem>()
                .Select(i => i.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (selectedIds.Count == 0)
            {
                return;
            }

            Program.SetMessageReadState(selectedIds, isRead);
            LoadMessagesIntoList();
            RestoreSelection(selectedIds);
            Program.RefreshMessageIndicators();
        }

        private bool IsApplicableReleaseAnnouncement(LocalMessage message)
        {
            if (message == null
                || !string.Equals(message.Kind, "releaseAnnouncement", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(message.UpdateAction, "installIfAvailable", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(message.ReleaseChannel, Program.AppProgramSettings.UpgradeToPreReleases ? "prerelease" : "stable", StringComparison.OrdinalIgnoreCase)
                || !Version.TryParse(message.ReleaseVersion, out Version releaseVersion)
                || !Version.TryParse(Program.AppVersion, out Version currentVersion))
            {
                return false;
            }

            return releaseVersion > currentVersion;
        }

        private void RestoreSelection(List<string> selectedIds)
        {
            foreach (ListViewItem item in lv_messages.Items)
            {
                if (selectedIds.Contains(item.Name, StringComparer.OrdinalIgnoreCase))
                {
                    item.Selected = true;
                }
            }
        }

        private void SelectInitialMessageIfNeeded()
        {
            if (!_selectNewestUnreadOnLoad || lv_messages.Items.Count == 0)
            {
                return;
            }

            ListViewItem unreadItem = lv_messages.Items
                .Cast<ListViewItem>()
                .FirstOrDefault(i => (i.Tag as LocalMessage)?.IsRead == false);

            ListViewItem itemToSelect = unreadItem ?? lv_messages.Items[0];
            itemToSelect.Selected = true;
            itemToSelect.Focused = true;
            itemToSelect.EnsureVisible();
        }

        private void RenderMessage(LocalMessage message)
        {
            if (message == null)
            {
                lbl_fallback.Text = "Select a message to view its content.";
                lbl_fallback.Visible = true;
                if (webView != null)
                {
                    webView.Visible = false;
                }
                return;
            }

            string fullPath = Path.Combine(Program.AppMessagesPath, message.MarkdownFileName ?? string.Empty);
            if (!File.Exists(fullPath))
            {
                logger.Warn($"MessagesForm/RenderMessage: Markdown file is missing (messageId={message.Id}, title={message.Title}, markdownFileName={message.MarkdownFileName}, fullPath={fullPath}).");
                lbl_fallback.Text = "This message content could not be found on disk.";
                lbl_fallback.Visible = true;
                if (webView != null)
                {
                    webView.Visible = false;
                }
                return;
            }

            string rawContent;
            string htmlDoc;
            try
            {
                rawContent = File.ReadAllText(fullPath);
                string releaseBanner = string.Equals(message.Kind, "releaseAnnouncement", StringComparison.OrdinalIgnoreCase)
                    ? $"<div style='margin:0 0 18px;padding:12px 16px;border:1px solid #4f46e5;background:#eef2ff;color:#312e81;border-radius:6px;'><strong>DisplayMagician update {System.Net.WebUtility.HtmlEncode(message.ReleaseVersion)}</strong><br />Release channel: {System.Net.WebUtility.HtmlEncode(message.ReleaseChannel)}</div>"
                    : string.Empty;
                if (message.Format != null && message.Format.Equals("html", StringComparison.OrdinalIgnoreCase))
                {
                    htmlDoc = $"<!DOCTYPE html><html><head><meta charset='utf-8'><style>body{{font-family:'Segoe UI',sans-serif;padding:20px;line-height:1.45;color:#1a1a1a;}} pre{{background:#f4f4f4;padding:10px;overflow:auto;}} code{{font-family:Consolas,monospace;}} table{{border-collapse:collapse;}} th,td{{border:1px solid #ddd;padding:6px 8px;}}</style></head><body>{releaseBanner}{rawContent}</body></html>";
                }
                else
                {
                    string htmlBody = Markdown.ToHtml(rawContent, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
                    htmlDoc = $"<!DOCTYPE html><html><head><meta charset='utf-8'><style>body{{font-family:'Segoe UI',sans-serif;padding:20px;line-height:1.45;color:#1a1a1a;}} pre{{background:#f4f4f4;padding:10px;overflow:auto;}} code{{font-family:Consolas,monospace;}} table{{border-collapse:collapse;}} th,td{{border:1px solid #ddd;padding:6px 8px;}}</style></head><body>{releaseBanner}{htmlBody}</body></html>";
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"MessagesForm/RenderMessage: Failed to read or parse content (messageId={message.Id}, title={message.Title}, markdownFileName={message.MarkdownFileName}, fullPath={fullPath}).");
                lbl_fallback.Text = "This message content could not be loaded.";
                lbl_fallback.Visible = true;
                if (webView != null)
                {
                    webView.Visible = false;
                }
                return;
            }

            if (webView != null)
            {
                try
                {
                    webView.Visible = true;
                    lbl_fallback.Visible = false;
                    webView.NavigateToString(htmlDoc);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"MessagesForm/RenderMessage: Failed to render HTML in WebView2 (messageId={message.Id}, title={message.Title}, markdownFileName={message.MarkdownFileName}, fullPath={fullPath}).");
                    webView.Visible = false;
                    lbl_fallback.Text = rawContent;
                    lbl_fallback.Visible = true;
                }
            }
            else
            {
                lbl_fallback.Text = rawContent;
                lbl_fallback.Visible = true;
            }
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
