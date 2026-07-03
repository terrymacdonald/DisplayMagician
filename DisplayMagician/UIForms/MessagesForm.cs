using DisplayMagician.Messaging;
using DisplayMagician.Processes;
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
    public class MessagesForm : Form
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SplitContainer splitContainer = new SplitContainer();
        private readonly ListView lvMessages = new ListView();
        private readonly Button btnMarkRead = new Button();
        private readonly Button btnMarkUnread = new Button();
        private readonly Label lblCount = new Label();
        private readonly Panel rightPanel = new Panel();
        private WebView2 webView;
        private readonly Label lblFallback = new Label();
        private readonly bool _selectNewestUnreadOnLoad;

        private List<LocalMessage> _messages = new List<LocalMessage>();

        public MessagesForm() : this(false)
        {
        }

        public MessagesForm(bool selectNewestUnreadOnLoad)
        {
            _selectNewestUnreadOnLoad = selectNewestUnreadOnLoad;
            InitializeUi();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadMessagesIntoList();
            InitializeWebViewIfNeeded();
            SelectInitialMessageIfNeeded();
        }

        private void InitializeUi()
        {
            Text = "DisplayMagician Messages";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1120;
            Height = 760;
            MinimumSize = new Size(860, 560);
            BackColor = Color.Black;
            ForeColor = Color.White;

            splitContainer.Dock = DockStyle.Fill;
            splitContainer.SplitterDistance = 360;
            splitContainer.BorderStyle = BorderStyle.FixedSingle;

            lvMessages.Dock = DockStyle.Fill;
            lvMessages.View = View.Details;
            lvMessages.FullRowSelect = true;
            lvMessages.MultiSelect = true;
            lvMessages.HideSelection = false;
            lvMessages.Columns.Add("Title", 230);
            lvMessages.Columns.Add("Received", 120);
            lvMessages.SelectedIndexChanged += LvMessages_SelectedIndexChanged;

            Panel leftTopPanel = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.Black };
            lblCount.Dock = DockStyle.Left;
            lblCount.TextAlign = ContentAlignment.MiddleLeft;
            lblCount.Padding = new Padding(8, 0, 0, 0);
            lblCount.Text = "0 messages";

            btnMarkUnread.Text = "Mark Unread";
            btnMarkUnread.Dock = DockStyle.Right;
            btnMarkUnread.Width = 110;
            btnMarkUnread.Click += BtnMarkUnread_Click;

            btnMarkRead.Text = "Mark Read";
            btnMarkRead.Dock = DockStyle.Right;
            btnMarkRead.Width = 95;
            btnMarkRead.Click += BtnMarkRead_Click;

            leftTopPanel.Controls.Add(btnMarkUnread);
            leftTopPanel.Controls.Add(btnMarkRead);
            leftTopPanel.Controls.Add(lblCount);

            splitContainer.Panel1.Controls.Add(lvMessages);
            splitContainer.Panel1.Controls.Add(leftTopPanel);

            rightPanel.Dock = DockStyle.Fill;
            rightPanel.BackColor = Color.White;

            lblFallback.Dock = DockStyle.Fill;
            lblFallback.Text = "Select a message to view its content.";
            lblFallback.TextAlign = ContentAlignment.MiddleCenter;
            lblFallback.BackColor = Color.White;
            lblFallback.ForeColor = Color.Black;

            rightPanel.Controls.Add(lblFallback);
            splitContainer.Panel2.Controls.Add(rightPanel);

            Controls.Add(splitContainer);
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
