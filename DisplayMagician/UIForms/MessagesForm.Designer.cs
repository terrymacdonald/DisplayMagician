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
            tlpLeft = new System.Windows.Forms.TableLayoutPanel();
            panelListHeader = new System.Windows.Forms.Panel();
            lbl_count = new System.Windows.Forms.Label();
            lv_messages = new System.Windows.Forms.ListView();
            flpLeftButtons = new System.Windows.Forms.FlowLayoutPanel();
            btn_mark_read = new System.Windows.Forms.Button();
            btn_mark_unread = new System.Windows.Forms.Button();
            btn_back = new System.Windows.Forms.Button();
            rightPanel = new System.Windows.Forms.Panel();
            lbl_fallback = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            tlpLeft.SuspendLayout();
            panelListHeader.SuspendLayout();
            flpLeftButtons.SuspendLayout();
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
            splitContainer.Panel1.Controls.Add(tlpLeft);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(rightPanel);
            splitContainer.Size = new System.Drawing.Size(1120, 760);
            splitContainer.SplitterDistance = 386;
            splitContainer.TabIndex = 0;
            // 
            // tlpLeft
            // 
            tlpLeft.ColumnCount = 1;
            tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpLeft.Controls.Add(panelListHeader, 0, 0);
            tlpLeft.Controls.Add(lv_messages, 0, 1);
            tlpLeft.Controls.Add(flpLeftButtons, 0, 2);
            tlpLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpLeft.Location = new System.Drawing.Point(0, 0);
            tlpLeft.Name = "tlpLeft";
            tlpLeft.RowCount = 3;
            tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpLeft.Size = new System.Drawing.Size(384, 758);
            tlpLeft.TabIndex = 0;
            // 
            // panelListHeader
            // 
            panelListHeader.BackColor = System.Drawing.Color.Black;
            panelListHeader.Controls.Add(lbl_count);
            panelListHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            panelListHeader.Location = new System.Drawing.Point(3, 3);
            panelListHeader.Name = "panelListHeader";
            panelListHeader.Size = new System.Drawing.Size(378, 36);
            panelListHeader.TabIndex = 0;
            // 
            // lbl_count
            // 
            lbl_count.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_count.Location = new System.Drawing.Point(0, 0);
            lbl_count.Name = "lbl_count";
            lbl_count.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            lbl_count.Size = new System.Drawing.Size(378, 36);
            lbl_count.TabIndex = 0;
            lbl_count.Text = "0 messages";
            lbl_count.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lv_messages
            // 
            lv_messages.Dock = System.Windows.Forms.DockStyle.Fill;
            lv_messages.Location = new System.Drawing.Point(3, 45);
            lv_messages.Name = "lv_messages";
            lv_messages.Size = new System.Drawing.Size(378, 659);
            lv_messages.TabIndex = 1;
            lv_messages.UseCompatibleStateImageBehavior = false;
            lv_messages.SelectedIndexChanged += lv_messages_SelectedIndexChanged;
            // 
            // flpLeftButtons
            // 
            flpLeftButtons.Controls.Add(btn_back);
            flpLeftButtons.Controls.Add(btn_mark_unread);
            flpLeftButtons.Controls.Add(btn_mark_read);
            flpLeftButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            flpLeftButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flpLeftButtons.Location = new System.Drawing.Point(3, 710);
            flpLeftButtons.Name = "flpLeftButtons";
            flpLeftButtons.Size = new System.Drawing.Size(378, 45);
            flpLeftButtons.TabIndex = 2;
            // 
            // btn_mark_read
            // 
            btn_mark_read.AutoSize = true;
            btn_mark_read.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_mark_read.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_mark_read.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_mark_read.ForeColor = System.Drawing.Color.White;
            btn_mark_read.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_mark_read.Location = new System.Drawing.Point(3, 3);
            btn_mark_read.Name = "btn_mark_read";
            btn_mark_read.Size = new System.Drawing.Size(94, 39);
            btn_mark_read.TabIndex = 9;
            btn_mark_read.Text = "Mark &Read";
            btn_mark_read.UseVisualStyleBackColor = true;
            btn_mark_read.Click += btn_mark_read_Click;
            // 
            // btn_mark_unread
            // 
            btn_mark_unread.AutoSize = true;
            btn_mark_unread.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_mark_unread.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_mark_unread.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_mark_unread.ForeColor = System.Drawing.Color.White;
            btn_mark_unread.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_mark_unread.Location = new System.Drawing.Point(103, 3);
            btn_mark_unread.Name = "btn_mark_unread";
            btn_mark_unread.Size = new System.Drawing.Size(102, 39);
            btn_mark_unread.TabIndex = 8;
            btn_mark_unread.Text = "Mark &Unread";
            btn_mark_unread.UseVisualStyleBackColor = true;
            btn_mark_unread.Click += btn_mark_unread_Click;
            // 
            // btn_back
            // 
            btn_back.AutoSize = true;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_back.Location = new System.Drawing.Point(211, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(73, 39);
            btn_back.TabIndex = 10;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // rightPanel
            // 
            rightPanel.BackColor = System.Drawing.Color.White;
            rightPanel.Controls.Add(lbl_fallback);
            rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            rightPanel.Location = new System.Drawing.Point(0, 0);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new System.Drawing.Size(728, 758);
            rightPanel.TabIndex = 0;
            // 
            // lbl_fallback
            // 
            lbl_fallback.BackColor = System.Drawing.Color.White;
            lbl_fallback.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_fallback.ForeColor = System.Drawing.Color.Black;
            lbl_fallback.Location = new System.Drawing.Point(0, 0);
            lbl_fallback.Name = "lbl_fallback";
            lbl_fallback.Size = new System.Drawing.Size(728, 758);
            lbl_fallback.TabIndex = 0;
            lbl_fallback.Text = "Select a message to view its content.";
            lbl_fallback.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MessagesForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
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
            tlpLeft.ResumeLayout(false);
            panelListHeader.ResumeLayout(false);
            flpLeftButtons.ResumeLayout(false);
            flpLeftButtons.PerformLayout();
            rightPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TableLayoutPanel tlpLeft;
        private System.Windows.Forms.Panel panelListHeader;
        private System.Windows.Forms.Label lbl_count;
        private System.Windows.Forms.ListView lv_messages;
        private System.Windows.Forms.FlowLayoutPanel flpLeftButtons;
        private System.Windows.Forms.Button btn_mark_read;
        private System.Windows.Forms.Button btn_mark_unread;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.Label lbl_fallback;
    }
}

