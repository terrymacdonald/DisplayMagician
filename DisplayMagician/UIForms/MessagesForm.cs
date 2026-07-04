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
            lvMessages.Items.Clear();

            Font unreadFont = new Font(lvMessages.Font, FontStyle.Bold);
            Font readFont = new Font(lvMessages.Font, FontStyle.Regular);

            foreach (LocalMessage message in _messages)
            {
                ListViewItem item = new ListViewItem(message.Title)
                {
                    Name = message.Id,
                    Tag = message,
                    Font = message.IsRead ? readFont : unreadFont,
                };

                item.SubItems.Add(message.ReceivedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
                lvMessages.Items.Add(item);
            }

            int unreadCount = _messages.Count(m => !m.IsRead);
            lblCount.Text = $"{_messages.Count} messages ({unreadCount} unread)";
        }

        private void LvMessages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvMessages.SelectedItems.Count == 0)
            {
                return;
            }

            List<string> selectedIds = lvMessages.SelectedItems
                .Cast<ListViewItem>()
                .Select(i => i.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Program.SetMessageReadState(selectedIds, true);
            LoadMessagesIntoList();
            RestoreSelection(selectedIds);

            if (lvMessages.SelectedItems.Count > 0)
            {
                LocalMessage selectedMessage = lvMessages.SelectedItems[0].Tag as LocalMessage;
                RenderMessage(selectedMessage);
            }

            Program.RefreshMessageIndicators();
        }

        private void BtnMarkRead_Click(object sender, EventArgs e)
        {
            ApplyReadStateForSelection(isRead: true);
        }

        private void BtnMarkUnread_Click(object sender, EventArgs e)
        {
            ApplyReadStateForSelection(isRead: false);
        }

        private void ApplyReadStateForSelection(bool isRead)
        {
            List<string> selectedIds = lvMessages.SelectedItems
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

        private void RestoreSelection(List<string> selectedIds)
        {
            foreach (ListViewItem item in lvMessages.Items)
            {
                if (selectedIds.Contains(item.Name, StringComparer.OrdinalIgnoreCase))
                {
                    item.Selected = true;
                }
            }
        }

        private void SelectInitialMessageIfNeeded()
        {
            if (!_selectNewestUnreadOnLoad || lvMessages.Items.Count == 0)
            {
                return;
            }

            ListViewItem unreadItem = lvMessages.Items
                .Cast<ListViewItem>()
                .FirstOrDefault(i => (i.Tag as LocalMessage)?.IsRead == false);

            ListViewItem itemToSelect = unreadItem ?? lvMessages.Items[0];
            itemToSelect.Selected = true;
            itemToSelect.Focused = true;
            itemToSelect.EnsureVisible();
        }

        private void RenderMessage(LocalMessage message)
        {
            if (message == null)
            {
                lblFallback.Text = "Select a message to view its content.";
                lblFallback.Visible = true;
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
                lblFallback.Text = "This message content could not be found on disk.";
                lblFallback.Visible = true;
                if (webView != null)
                {
                    webView.Visible = false;
                }
                return;
            }

            string markdown;
            string htmlDoc;
            try
            {
                markdown = File.ReadAllText(fullPath);
                string htmlBody = Markdown.ToHtml(markdown, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
                htmlDoc = $"<!DOCTYPE html><html><head><meta charset='utf-8'><style>body{{font-family:'Segoe UI',sans-serif;padding:20px;line-height:1.45;color:#1a1a1a;}} pre{{background:#f4f4f4;padding:10px;overflow:auto;}} code{{font-family:Consolas,monospace;}} table{{border-collapse:collapse;}} th,td{{border:1px solid #ddd;padding:6px 8px;}}</style></head><body>{htmlBody}</body></html>";
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"MessagesForm/RenderMessage: Failed to read or parse markdown content (messageId={message.Id}, title={message.Title}, markdownFileName={message.MarkdownFileName}, fullPath={fullPath}).");
                lblFallback.Text = "This message content could not be loaded.";
                lblFallback.Visible = true;
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
                    lblFallback.Visible = false;
                    webView.NavigateToString(htmlDoc);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"MessagesForm/RenderMessage: Failed to render markdown in WebView2 (messageId={message.Id}, title={message.Title}, markdownFileName={message.MarkdownFileName}, fullPath={fullPath}).");
                    webView.Visible = false;
                    lblFallback.Text = markdown;
                    lblFallback.Visible = true;
                }
            }
            else
            {
                lblFallback.Text = markdown;
                lblFallback.Visible = true;
            }
        }
    }
}
