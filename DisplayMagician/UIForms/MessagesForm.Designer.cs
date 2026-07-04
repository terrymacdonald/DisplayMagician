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
            lvMessages = new System.Windows.Forms.ListView();
            colTitle = new System.Windows.Forms.ColumnHeader();
            colReceived = new System.Windows.Forms.ColumnHeader();
            panelListHeader = new System.Windows.Forms.Panel();
            btnMarkUnread = new System.Windows.Forms.Button();
            btnMarkRead = new System.Windows.Forms.Button();
            lblCount = new System.Windows.Forms.Label();
            rightPanel = new System.Windows.Forms.Panel();
            lblFallback = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
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
            splitContainer.Panel1.Controls.Add(lvMessages);
            splitContainer.Panel1.Controls.Add(panelListHeader);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(rightPanel);
            splitContainer.Size = new System.Drawing.Size(1120, 760);
            splitContainer.SplitterDistance = 360;
            splitContainer.TabIndex = 0;
            // 
            // lvMessages
            // 
            lvMessages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colTitle, colReceived });
            lvMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            lvMessages.FullRowSelect = true;
            lvMessages.HideSelection = false;
            lvMessages.Location = new System.Drawing.Point(0, 36);
            lvMessages.MultiSelect = true;
            lvMessages.Name = "lvMessages";
            lvMessages.Size = new System.Drawing.Size(358, 722);
            lvMessages.TabIndex = 1;
            lvMessages.UseCompatibleStateImageBehavior = false;
            lvMessages.View = System.Windows.Forms.View.Details;
            lvMessages.SelectedIndexChanged += LvMessages_SelectedIndexChanged;
            // 
            // colTitle
            // 
            colTitle.Text = "Title";
            colTitle.Width = 230;
            // 
            // colReceived
            // 
            colReceived.Text = "Received";
            colReceived.Width = 120;
            // 
            // panelListHeader
            // 
            panelListHeader.BackColor = System.Drawing.Color.Black;
            panelListHeader.Controls.Add(btnMarkUnread);
            panelListHeader.Controls.Add(btnMarkRead);
            panelListHeader.Controls.Add(lblCount);
            panelListHeader.Dock = System.Windows.Forms.DockStyle.Top;
            panelListHeader.Location = new System.Drawing.Point(0, 0);
            panelListHeader.Name = "panelListHeader";
            panelListHeader.Size = new System.Drawing.Size(358, 36);
            panelListHeader.TabIndex = 0;
            // 
            // btnMarkUnread
            // 
            btnMarkUnread.Dock = System.Windows.Forms.DockStyle.Right;
            btnMarkUnread.Location = new System.Drawing.Point(248, 0);
            btnMarkUnread.Name = "btnMarkUnread";
            btnMarkUnread.Size = new System.Drawing.Size(110, 36);
            btnMarkUnread.TabIndex = 2;
            btnMarkUnread.Text = "Mark Unread";
            btnMarkUnread.UseVisualStyleBackColor = true;
            btnMarkUnread.Click += BtnMarkUnread_Click;
            // 
            // btnMarkRead
            // 
            btnMarkRead.Dock = System.Windows.Forms.DockStyle.Right;
            btnMarkRead.Location = new System.Drawing.Point(153, 0);
            btnMarkRead.Name = "btnMarkRead";
            btnMarkRead.Size = new System.Drawing.Size(95, 36);
            btnMarkRead.TabIndex = 1;
            btnMarkRead.Text = "Mark Read";
            btnMarkRead.UseVisualStyleBackColor = true;
            btnMarkRead.Click += BtnMarkRead_Click;
            // 
            // lblCount
            // 
            lblCount.Dock = System.Windows.Forms.DockStyle.Left;
            lblCount.Location = new System.Drawing.Point(0, 0);
            lblCount.Name = "lblCount";
            lblCount.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            lblCount.Size = new System.Drawing.Size(240, 36);
            lblCount.TabIndex = 0;
            lblCount.Text = "0 messages";
            lblCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rightPanel
            // 
            rightPanel.BackColor = System.Drawing.Color.White;
            rightPanel.Controls.Add(lblFallback);
            rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            rightPanel.Location = new System.Drawing.Point(0, 0);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new System.Drawing.Size(754, 758);
            rightPanel.TabIndex = 0;
            // 
            // lblFallback
            // 
            lblFallback.BackColor = System.Drawing.Color.White;
            lblFallback.Dock = System.Windows.Forms.DockStyle.Fill;
            lblFallback.ForeColor = System.Drawing.Color.Black;
            lblFallback.Location = new System.Drawing.Point(0, 0);
            lblFallback.Name = "lblFallback";
            lblFallback.Size = new System.Drawing.Size(754, 758);
            lblFallback.TabIndex = 0;
            lblFallback.Text = "Select a message to view its content.";
            lblFallback.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MessagesForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(1120, 760);
            Controls.Add(splitContainer);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            ForeColor = System.Drawing.Color.White;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(860, 560);
            MinimizeBox = false;
            Name = "MessagesForm";
            ShowIcon = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "DisplayMagician Messages";
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            panelListHeader.ResumeLayout(false);
            rightPanel.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ListView lvMessages;
        private System.Windows.Forms.ColumnHeader colTitle;
        private System.Windows.Forms.ColumnHeader colReceived;
        private System.Windows.Forms.Panel panelListHeader;
        private System.Windows.Forms.Button btnMarkUnread;
        private System.Windows.Forms.Button btnMarkRead;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.Label lblFallback;
    }
}
