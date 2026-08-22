namespace DisplayMagician.UIForms
{
    partial class MessagesForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessagesForm));
            splitContainer = new System.Windows.Forms.SplitContainer();
            btn_back = new System.Windows.Forms.Button();
            btn_mark_unread = new System.Windows.Forms.Button();
            btn_mark_read = new System.Windows.Forms.Button();
            dgv_messages = new System.Windows.Forms.DataGridView();
            col_title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            col_published = new System.Windows.Forms.DataGridViewTextBoxColumn();
            panelListHeader = new System.Windows.Forms.Panel();
            lbl_count = new System.Windows.Forms.Label();
            rightPanel = new System.Windows.Forms.Panel();
            message_content_panel = new System.Windows.Forms.Panel();
            lbl_fallback = new System.Windows.Forms.Label();
            panel_release_header = new System.Windows.Forms.Panel();
            lbl_release_heading = new System.Windows.Forms.Label();
            btn_update_now = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgv_messages).BeginInit();
            panelListHeader.SuspendLayout();
            rightPanel.SuspendLayout();
            message_content_panel.SuspendLayout();
            panel_release_header.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer
            // 
            splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Location = new System.Drawing.Point(0, 0);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(btn_back);
            splitContainer.Panel1.Controls.Add(btn_mark_unread);
            splitContainer.Panel1.Controls.Add(btn_mark_read);
            splitContainer.Panel1.Controls.Add(dgv_messages);
            splitContainer.Panel1.Controls.Add(panelListHeader);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(rightPanel);
            splitContainer.Size = new System.Drawing.Size(1295, 760);
            splitContainer.SplitterDistance = 490;
            splitContainer.TabIndex = 0;
            // 
            // btn_back
            // 
            btn_back.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_back.Location = new System.Drawing.Point(394, 709);
            btn_back.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(78, 27);
            btn_back.TabIndex = 10;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // btn_mark_unread
            // 
            btn_mark_unread.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_mark_unread.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_mark_unread.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_mark_unread.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_mark_unread.ForeColor = System.Drawing.Color.White;
            btn_mark_unread.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_mark_unread.Location = new System.Drawing.Point(243, 709);
            btn_mark_unread.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_mark_unread.Name = "btn_mark_unread";
            btn_mark_unread.Size = new System.Drawing.Size(90, 27);
            btn_mark_unread.TabIndex = 8;
            btn_mark_unread.Text = "Mark &Unread";
            btn_mark_unread.UseVisualStyleBackColor = true;
            btn_mark_unread.Click += btn_mark_unread_Click;
            // 
            // btn_mark_read
            // 
            btn_mark_read.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_mark_read.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_mark_read.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_mark_read.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_mark_read.ForeColor = System.Drawing.Color.White;
            btn_mark_read.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_mark_read.Location = new System.Drawing.Point(160, 709);
            btn_mark_read.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_mark_read.Name = "btn_mark_read";
            btn_mark_read.Size = new System.Drawing.Size(78, 27);
            btn_mark_read.TabIndex = 9;
            btn_mark_read.Text = "Mark &Read";
            btn_mark_read.UseVisualStyleBackColor = true;
            btn_mark_read.Click += btn_mark_read_Click;
            // 
            // dgv_messages
            // 
            dgv_messages.AllowUserToAddRows = false;
            dgv_messages.AllowUserToDeleteRows = false;
            dgv_messages.AllowUserToResizeRows = false;
            dgv_messages.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgv_messages.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            dgv_messages.BackgroundColor = System.Drawing.Color.White;
            dgv_messages.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgv_messages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv_messages.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { col_title, col_published });
            dgv_messages.EnableHeadersVisualStyles = false;
            dgv_messages.Location = new System.Drawing.Point(11, 42);
            dgv_messages.MultiSelect = false;
            dgv_messages.Name = "dgv_messages";
            dgv_messages.ReadOnly = true;
            dgv_messages.RowHeadersVisible = false;
            dgv_messages.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgv_messages.Size = new System.Drawing.Size(461, 655);
            dgv_messages.TabIndex = 1;
            dgv_messages.SelectionChanged += dgv_messages_SelectionChanged;
            // 
            // col_title
            // 
            col_title.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            col_title.HeaderText = "Message";
            col_title.Name = "col_title";
            col_title.ReadOnly = true;
            // 
            // col_published
            // 
            col_published.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            col_published.HeaderText = "Published";
            col_published.Name = "col_published";
            col_published.ReadOnly = true;
            col_published.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            col_published.Width = 125;
            // 
            // panelListHeader
            // 
            panelListHeader.BackColor = System.Drawing.Color.Black;
            panelListHeader.Controls.Add(lbl_count);
            panelListHeader.Dock = System.Windows.Forms.DockStyle.Top;
            panelListHeader.Location = new System.Drawing.Point(0, 0);
            panelListHeader.Name = "panelListHeader";
            panelListHeader.Size = new System.Drawing.Size(488, 36);
            panelListHeader.TabIndex = 0;
            // 
            // lbl_count
            // 
            lbl_count.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lbl_count.Location = new System.Drawing.Point(70, 0);
            lbl_count.Name = "lbl_count";
            lbl_count.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            lbl_count.Size = new System.Drawing.Size(350, 36);
            lbl_count.TabIndex = 0;
            lbl_count.Text = "0 messages";
            lbl_count.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rightPanel
            // 
            rightPanel.BackColor = System.Drawing.Color.White;
            rightPanel.Controls.Add(message_content_panel);
            rightPanel.Controls.Add(panel_release_header);
            rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            rightPanel.Location = new System.Drawing.Point(0, 0);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new System.Drawing.Size(799, 758);
            rightPanel.TabIndex = 0;
            // 
            // message_content_panel
            // 
            message_content_panel.BackColor = System.Drawing.Color.White;
            message_content_panel.Controls.Add(lbl_fallback);
            message_content_panel.Dock = System.Windows.Forms.DockStyle.Fill;
            message_content_panel.Location = new System.Drawing.Point(0, 52);
            message_content_panel.Name = "message_content_panel";
            message_content_panel.Size = new System.Drawing.Size(799, 706);
            message_content_panel.TabIndex = 0;
            // 
            // lbl_fallback
            // 
            lbl_fallback.BackColor = System.Drawing.Color.White;
            lbl_fallback.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_fallback.ForeColor = System.Drawing.Color.Black;
            lbl_fallback.Location = new System.Drawing.Point(0, 0);
            lbl_fallback.Name = "lbl_fallback";
            lbl_fallback.Size = new System.Drawing.Size(799, 706);
            lbl_fallback.TabIndex = 0;
            lbl_fallback.Text = "Select a message to view its content.";
            lbl_fallback.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel_release_header
            // 
            panel_release_header.BackColor = System.Drawing.Color.FromArgb(238, 242, 255);
            panel_release_header.Controls.Add(lbl_release_heading);
            panel_release_header.Controls.Add(btn_update_now);
            panel_release_header.Dock = System.Windows.Forms.DockStyle.Top;
            panel_release_header.Location = new System.Drawing.Point(0, 0);
            panel_release_header.Name = "panel_release_header";
            panel_release_header.Size = new System.Drawing.Size(799, 52);
            panel_release_header.TabIndex = 1;
            panel_release_header.Visible = false;
            // 
            // lbl_release_heading
            // 
            lbl_release_heading.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lbl_release_heading.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_release_heading.ForeColor = System.Drawing.Color.FromArgb(49, 46, 129);
            lbl_release_heading.Location = new System.Drawing.Point(14, 5);
            lbl_release_heading.Name = "lbl_release_heading";
            lbl_release_heading.Size = new System.Drawing.Size(614, 39);
            lbl_release_heading.TabIndex = 0;
            lbl_release_heading.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btn_update_now
            // 
            btn_update_now.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_update_now.BackColor = System.Drawing.Color.FromArgb(79, 70, 229);
            btn_update_now.FlatAppearance.BorderSize = 0;
            btn_update_now.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_update_now.ForeColor = System.Drawing.Color.White;
            btn_update_now.Location = new System.Drawing.Point(662, 11);
            btn_update_now.Name = "btn_update_now";
            btn_update_now.Size = new System.Drawing.Size(126, 28);
            btn_update_now.TabIndex = 1;
            btn_update_now.Text = "&Update Now";
            btn_update_now.UseVisualStyleBackColor = false;
            btn_update_now.Click += btn_update_now_Click;
            // 
            // MessagesForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(1295, 760);
            Controls.Add(splitContainer);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            ForeColor = System.Drawing.Color.White;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(860, 560);
            Name = "MessagesForm";
            ShowIcon = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "DisplayMagician Messages";
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgv_messages).EndInit();
            panelListHeader.ResumeLayout(false);
            rightPanel.ResumeLayout(false);
            message_content_panel.ResumeLayout(false);
            panel_release_header.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel panelListHeader;
        private System.Windows.Forms.Label lbl_count;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.Panel message_content_panel;
        private System.Windows.Forms.Label lbl_fallback;
        private System.Windows.Forms.DataGridView dgv_messages;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_title;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_published;
        private System.Windows.Forms.Button btn_mark_read;
        private System.Windows.Forms.Button btn_mark_unread;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.Panel panel_release_header;
        private System.Windows.Forms.Label lbl_release_heading;
        private System.Windows.Forms.Button btn_update_now;
    }
}
