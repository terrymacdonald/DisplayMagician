using DisplayMagician.Messaging;
using Markdig;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public partial class MessagesForm : Form
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private WebView2 webView;
        private readonly bool _selectNewestUnreadOnLoad;

        private List<LocalMessage> _messages = new List<LocalMessage>();
        private bool _isUpdatingList = false;

        public MessagesForm() : this(false)
        {
        }

        public MessagesForm(bool selectNewestUnreadOnLoad)
        {
            _selectNewestUnreadOnLoad = selectNewestUnreadOnLoad;
            InitializeComponent();
            dgv_messages.DefaultCellStyle.BackColor = Color.White;
            dgv_messages.DefaultCellStyle.ForeColor = Color.Black;
            dgv_messages.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
            dgv_messages.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            dgv_messages.ColumnHeadersDefaultCellStyle.BackColor = Color.Gainsboro;
            dgv_messages.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv_messages.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.Gainsboro;
            dgv_messages.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black;
            dgv_messages.ColumnHeadersDefaultCellStyle.Font = new Font(dgv_messages.Font, FontStyle.Bold);
            col_title.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _isUpdatingList = true;
            try
            {
                LoadMessagesIntoList();
                await InitializeWebViewIfNeededAsync();
            }
            finally
            {
                _isUpdatingList = false;
            }

            dgv_messages.ClearSelection();
            SelectInitialMessageIfNeeded();
        }

        private async Task InitializeWebViewIfNeededAsync()
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
                await webView.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                webView?.Dispose();
                webView = null;
                logger.Warn(ex, "MessagesForm/InitializeWebViewIfNeeded: WebView2 failed to initialise; fallback label will be used.");
            }
        }

        private void LoadMessagesIntoList()
        {
            _messages = Program.GetStoredMessages();
            dgv_messages.Rows.Clear();

            Font unreadFont = new Font(dgv_messages.Font, FontStyle.Bold);
            Font readFont = new Font(dgv_messages.Font, FontStyle.Regular);

            foreach (LocalMessage message in _messages)
            {
                bool isReleaseAnnouncement = string.Equals(message.Kind, "releaseAnnouncement", StringComparison.OrdinalIgnoreCase);
                DateTime displayUtc = message.PublishedUtc ?? message.ReceivedUtc;
                DataGridViewRow row = new DataGridViewRow
                {
                    Tag = message,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        BackColor = Color.White,
                        Font = message.IsRead ? readFont : unreadFont,
                        ForeColor = Color.Black,
                        SelectionBackColor = SystemColors.Highlight,
                        SelectionForeColor = SystemColors.HighlightText,
                    },
                };

                row.CreateCells(
                    dgv_messages,
                    isReleaseAnnouncement ? $"Update: {message.Title}" : message.Title,
                    displayUtc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture));
                row.Cells[0].ToolTipText = isReleaseAnnouncement
                    ? $"Release update {message.ReleaseVersion} ({message.ReleaseChannel})"
                    : message.Title;
                dgv_messages.Rows.Add(row);
            }

            int unreadCount = _messages.Count(m => !m.IsRead);
            lbl_count.Text = $"{_messages.Count} messages ({unreadCount} unread)";
        }

        private void dgv_messages_SelectionChanged(object sender, EventArgs e)
        {
            if (_isUpdatingList)
            {
                return;
            }

            if (dgv_messages.SelectedRows.Count == 0)
            {
                btn_upgrade.Enabled = false;
                return;
            }

            List<string> selectedIds = dgv_messages.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => (row.Tag as LocalMessage)?.Id)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            try
            {
                _isUpdatingList = true;
                Program.SetMessageReadState(selectedIds, true);
                LoadMessagesIntoList();
                RestoreSelection(selectedIds);
            }
            finally
            {
                _isUpdatingList = false;
            }

            if (dgv_messages.SelectedRows.Count > 0)
            {
                LocalMessage selectedMessage = dgv_messages.SelectedRows[0].Tag as LocalMessage;
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
            LocalMessage selectedMessage = dgv_messages.SelectedRows.Count == 1
                ? dgv_messages.SelectedRows[0].Tag as LocalMessage
                : null;

            if (!IsApplicableReleaseAnnouncement(selectedMessage))
            {
                return;
            }

            Program.CheckForUpdates(automatic: false);
        }

        private void ApplyReadStateForSelection(bool isRead)
        {
            if (_isUpdatingList)
            {
                return;
            }

            List<string> selectedIds = dgv_messages.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => (row.Tag as LocalMessage)?.Id)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (selectedIds.Count == 0)
            {
                return;
            }

            try
            {
                _isUpdatingList = true;
                Program.SetMessageReadState(selectedIds, isRead);
                LoadMessagesIntoList();
                RestoreSelection(selectedIds);
            }
            finally
            {
                _isUpdatingList = false;
            }

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
            foreach (DataGridViewRow row in dgv_messages.Rows)
            {
                LocalMessage message = row.Tag as LocalMessage;
                if (message != null && selectedIds.Contains(message.Id, StringComparer.OrdinalIgnoreCase))
                {
                    row.Selected = true;
                }
            }
        }

        private void SelectInitialMessageIfNeeded()
        {
            if (!_selectNewestUnreadOnLoad || dgv_messages.Rows.Count == 0)
            {
                return;
            }

            DataGridViewRow unreadRow = dgv_messages.Rows
                .Cast<DataGridViewRow>()
                .FirstOrDefault(row => (row.Tag as LocalMessage)?.IsRead == false);

            DataGridViewRow rowToSelect = unreadRow ?? dgv_messages.Rows[0];
            rowToSelect.Selected = true;
            dgv_messages.CurrentCell = rowToSelect.Cells[0];
            dgv_messages.FirstDisplayedScrollingRowIndex = rowToSelect.Index;
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
