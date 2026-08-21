namespace DisplayMagician.UIForms
{
    partial class UpgradeForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpgradeForm));
            lbl_title = new System.Windows.Forms.Label();
            btn_upgrade = new System.Windows.Forms.Button();
            btn_remind_later = new System.Windows.Forms.Button();
            btn_skip = new System.Windows.Forms.Button();
            lbl_changelog = new System.Windows.Forms.Label();
            lnk_changelog = new System.Windows.Forms.LinkLabel();
            rtb_message = new System.Windows.Forms.RichTextBox();
            web_release_notes = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)web_release_notes).BeginInit();
            SuspendLayout();
            // 
            // lbl_title
            // 
            lbl_title.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lbl_title.AutoSize = true;
            lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_title.ForeColor = System.Drawing.Color.White;
            lbl_title.Location = new System.Drawing.Point(476, 26);
            lbl_title.Margin = new System.Windows.Forms.Padding(55, 0, 55, 0);
            lbl_title.Name = "lbl_title";
            lbl_title.Size = new System.Drawing.Size(393, 29);
            lbl_title.TabIndex = 31;
            lbl_title.Text = "DisplayMagician Upgrade Available";
            // 
            // btn_upgrade
            // 
            btn_upgrade.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_upgrade.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_upgrade.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_upgrade.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_upgrade.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_upgrade.ForeColor = System.Drawing.Color.White;
            btn_upgrade.Location = new System.Drawing.Point(1074, 871);
            btn_upgrade.Margin = new System.Windows.Forms.Padding(55, 19, 55, 19);
            btn_upgrade.Name = "btn_upgrade";
            btn_upgrade.Size = new System.Drawing.Size(88, 27);
            btn_upgrade.TabIndex = 32;
            btn_upgrade.Text = "&Upgrade now";
            btn_upgrade.UseVisualStyleBackColor = true;
            btn_upgrade.Click += btn_upgrade_Click;
            // 
            // btn_remind_later
            // 
            btn_remind_later.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_remind_later.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_remind_later.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_remind_later.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_remind_later.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_remind_later.ForeColor = System.Drawing.Color.White;
            btn_remind_later.Location = new System.Drawing.Point(1170, 871);
            btn_remind_later.Margin = new System.Windows.Forms.Padding(55, 19, 55, 19);
            btn_remind_later.Name = "btn_remind_later";
            btn_remind_later.Size = new System.Drawing.Size(121, 27);
            btn_remind_later.TabIndex = 33;
            btn_remind_later.Text = "&Remind me later";
            btn_remind_later.UseVisualStyleBackColor = true;
            btn_remind_later.Click += btn_remind_later_Click;
            // 
            // btn_skip
            // 
            btn_skip.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_skip.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_skip.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_skip.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_skip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_skip.ForeColor = System.Drawing.Color.White;
            btn_skip.Location = new System.Drawing.Point(1300, 871);
            btn_skip.Margin = new System.Windows.Forms.Padding(55, 19, 55, 19);
            btn_skip.Name = "btn_skip";
            btn_skip.Size = new System.Drawing.Size(88, 27);
            btn_skip.TabIndex = 34;
            btn_skip.Text = "&Skip";
            btn_skip.UseVisualStyleBackColor = true;
            btn_skip.Click += btn_skip_Click;
            // 
            // lbl_changelog
            // 
            lbl_changelog.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lbl_changelog.BackColor = System.Drawing.Color.Black;
            lbl_changelog.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_changelog.ForeColor = System.Drawing.Color.White;
            lbl_changelog.Location = new System.Drawing.Point(15, 868);
            lbl_changelog.Margin = new System.Windows.Forms.Padding(55, 0, 55, 0);
            lbl_changelog.Name = "lbl_changelog";
            lbl_changelog.Size = new System.Drawing.Size(928, 34);
            lbl_changelog.TabIndex = 39;
            lbl_changelog.Text = "For more information on what has changed in the new version please visit the changelog: ";
            lbl_changelog.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lnk_changelog
            // 
            lnk_changelog.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            lnk_changelog.AutoSize = true;
            lnk_changelog.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lnk_changelog.LinkColor = System.Drawing.Color.LightSkyBlue;
            lnk_changelog.Location = new System.Drawing.Point(552, 877);
            lnk_changelog.Margin = new System.Windows.Forms.Padding(55, 0, 55, 0);
            lnk_changelog.Name = "lnk_changelog";
            lnk_changelog.Size = new System.Drawing.Size(371, 16);
            lnk_changelog.TabIndex = 40;
            lnk_changelog.TabStop = true;
            lnk_changelog.Text = "https://github.com/terrymacdonald/DisplayMagician/releases";
            lnk_changelog.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lnk_changelog.LinkClicked += lnk_changelog_LinkClicked;
            // 
            // rtb_message
            // 
            rtb_message.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            rtb_message.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            rtb_message.Location = new System.Drawing.Point(15, 73);
            rtb_message.Margin = new System.Windows.Forms.Padding(55, 19, 55, 19);
            rtb_message.Name = "rtb_message";
            rtb_message.Size = new System.Drawing.Size(1373, 780);
            rtb_message.TabIndex = 41;
            rtb_message.Text = "";
            // 
            // web_release_notes
            // 
            web_release_notes.AllowExternalDrop = false;
            web_release_notes.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            web_release_notes.BackColor = System.Drawing.Color.White;
            web_release_notes.CreationProperties = null;
            web_release_notes.DefaultBackgroundColor = System.Drawing.Color.White;
            web_release_notes.Location = new System.Drawing.Point(15, 74);
            web_release_notes.Margin = new System.Windows.Forms.Padding(55, 19, 55, 19);
            web_release_notes.Name = "web_release_notes";
            web_release_notes.Size = new System.Drawing.Size(1373, 779);
            web_release_notes.TabIndex = 42;
            web_release_notes.Visible = false;
            web_release_notes.ZoomFactor = 1D;
            // 
            // UpgradeForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(1403, 915);
            Controls.Add(web_release_notes);
            Controls.Add(rtb_message);
            Controls.Add(lnk_changelog);
            Controls.Add(lbl_changelog);
            Controls.Add(btn_skip);
            Controls.Add(btn_remind_later);
            Controls.Add(btn_upgrade);
            Controls.Add(lbl_title);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(55, 19, 55, 19);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(1419, 954);
            Name = "UpgradeForm";
            ShowIcon = false;
            Text = "Upgrade DisplayMagician";
            TopMost = true;
            Load += UpgradeForm_Load;
            ((System.ComponentModel.ISupportInitialize)web_release_notes).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Button btn_upgrade;
        private System.Windows.Forms.Button btn_remind_later;
        private System.Windows.Forms.Button btn_skip;
        private System.Windows.Forms.Label lbl_changelog;
        private System.Windows.Forms.LinkLabel lnk_changelog;
        private System.Windows.Forms.RichTextBox rtb_message;
        private Microsoft.Web.WebView2.WinForms.WebView2 web_release_notes;
    }
}
