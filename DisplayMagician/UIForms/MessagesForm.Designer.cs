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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessagesForm));
            splitContainer = new System.Windows.Forms.SplitContainer();
            btn_back = new System.Windows.Forms.Button();
            btn_mark_unread = new System.Windows.Forms.Button();
            btn_mark_read = new System.Windows.Forms.Button();
            btn_upgrade = new System.Windows.Forms.Button();
            dgv_messages = new System.Windows.Forms.DataGridView();
            col_title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            col_published = new System.Windows.Forms.DataGridViewTextBoxColumn();
            panelListHeader = new System.Windows.Forms.Panel();
            lbl_count = new System.Windows.Forms.Label();
            rightPanel = new System.Windows.Forms.Panel();
            lbl_fallback = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgv_messages).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            panelListHeader.SuspendLayout();
            rightPanel.SuspendLayout();
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
            splitContainer.Panel1.Controls.Add(btn_upgrade);
            splitContainer.Panel1.Controls.Add(dgv_messages);
            splitContainer.Panel1.Controls.Add(panelListHeader);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(rightPanel);
            splitContainer.Size = new System.Drawing.Size(1120, 760);
            splitContainer.SplitterDistance = 350;
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
            btn_back.Location = new System.Drawing.Point(256, 709);
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
            btn_mark_unread.Location = new System.Drawing.Point(173, 709);
            btn_mark_unread.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_mark_unread.Name = "btn_mark_unread";
            btn_mark_unread.Size = new System.Drawing.Size(78, 27);
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
            btn_mark_read.Location = new System.Drawing.Point(90, 709);
            btn_mark_read.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_mark_read.Name = "btn_mark_read";
            btn_mark_read.Size = new System.Drawing.Size(78, 27);
            btn_mark_read.TabIndex = 9;
            btn_mark_read.Text = "Mark &Read";
            btn_mark_read.UseVisualStyleBackColor = true;
            btn_mark_read.Click += btn_mark_read_Click;
            //
            // btn_upgrade
            //
            btn_upgrade.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_upgrade.Enabled = false;
            btn_upgrade.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_upgrade.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_upgrade.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_upgrade.ForeColor = System.Drawing.Color.White;
            btn_upgrade.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_upgrade.Location = new System.Drawing.Point(7, 709);
            btn_upgrade.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_upgrade.Name = "btn_upgrade";
            btn_upgrade.Size = new System.Drawing.Size(78, 27);
            btn_upgrade.TabIndex = 11;
            btn_upgrade.Text = "&Upgrade";
            btn_upgrade.UseVisualStyleBackColor = true;
            btn_upgrade.Click += btn_upgrade_Click;
            //
            // dgv_messages
            // 
            dgv_messages.AllowUserToAddRows = false;
            dgv_messages.AllowUserToDeleteRows = false;
            dgv_messages.AllowUserToResizeRows = false;
            dgv_messages.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgv_messages.AutoGenerateColumns = false;
            dgv_messages.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            dgv_messages.BackgroundColor = System.Drawing.Color.White;
            dgv_messages.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            dgv_messages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv_messages.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { col_title, col_published });
            dgv_messages.Location = new System.Drawing.Point(11, 42);
            dgv_messages.MultiSelect = true;
            dgv_messages.Name = "dgv_messages";
            dgv_messages.ReadOnly = true;
            dgv_messages.RowHeadersVisible = false;
            dgv_messages.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgv_messages.Size = new System.Drawing.Size(321, 655);
            dgv_messages.TabIndex = 1;
            dgv_messages.SelectionChanged += dgv_messages_SelectionChanged;
            //
            // col_title
            //
            col_title.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            col_title.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle { WrapMode = System.Windows.Forms.DataGridViewTriState.True };
            col_title.HeaderText = "Title";
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
            panelListHeader.Size = new System.Drawing.Size(348, 36);
            panelListHeader.TabIndex = 0;
            // 
            // lbl_count
            // 
            lbl_count.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lbl_count.Location = new System.Drawing.Point(0, 0);
            lbl_count.Name = "lbl_count";
            lbl_count.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            lbl_count.Size = new System.Drawing.Size(384, 36);
            lbl_count.TabIndex = 0;
            lbl_count.Text = "0 messages";
            lbl_count.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rightPanel
            // 
            rightPanel.BackColor = System.Drawing.Color.White;
            rightPanel.Controls.Add(lbl_fallback);
            rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            rightPanel.Location = new System.Drawing.Point(0, 0);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new System.Drawing.Size(764, 758);
            rightPanel.TabIndex = 0;
            // 
            // lbl_fallback
            // 
            lbl_fallback.BackColor = System.Drawing.Color.White;
            lbl_fallback.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_fallback.ForeColor = System.Drawing.Color.Black;
            lbl_fallback.Location = new System.Drawing.Point(0, 0);
            lbl_fallback.Name = "lbl_fallback";
            lbl_fallback.Size = new System.Drawing.Size(764, 758);
            lbl_fallback.TabIndex = 0;
            lbl_fallback.Text = "Select a message to view its content.";
            lbl_fallback.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MessagesForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(1120, 760);
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
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel panelListHeader;
        private System.Windows.Forms.Label lbl_count;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.Label lbl_fallback;
        private System.Windows.Forms.DataGridView dgv_messages;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_title;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_published;
        private System.Windows.Forms.Button btn_mark_read;
        private System.Windows.Forms.Button btn_mark_unread;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.Button btn_upgrade;
    }
}
