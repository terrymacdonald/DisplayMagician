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
            this.lbl_title = new System.Windows.Forms.Label();
            this.btn_upgrade = new System.Windows.Forms.Button();
            this.btn_remind_later = new System.Windows.Forms.Button();
            this.btn_skip = new System.Windows.Forms.Button();
            this.lbl_changelog = new System.Windows.Forms.Label();
            this.lnk_changelog = new System.Windows.Forms.LinkLabel();
            this.rtb_message = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // lbl_title
            // 
            this.lbl_title.AutoSize = true;
            this.lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_title.ForeColor = System.Drawing.Color.White;
            this.lbl_title.Location = new System.Drawing.Point(188, 9);
            this.lbl_title.Name = "lbl_title";
            this.lbl_title.Size = new System.Drawing.Size(475, 33);
            this.lbl_title.TabIndex = 31;
            this.lbl_title.Text = "DisplayMagician Upgrade Available";
            // 
            // btn_upgrade
            // 
            this.btn_upgrade.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_upgrade.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_upgrade.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_upgrade.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_upgrade.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_upgrade.ForeColor = System.Drawing.Color.White;
            this.btn_upgrade.Location = new System.Drawing.Point(183, 338);
            this.btn_upgrade.Name = "btn_upgrade";
            this.btn_upgrade.Size = new System.Drawing.Size(206, 40);
            this.btn_upgrade.TabIndex = 32;
            this.btn_upgrade.Text = "&Upgrade now";
            this.btn_upgrade.UseVisualStyleBackColor = true;
            this.btn_upgrade.Click += new System.EventHandler(this.btn_upgrade_Click);
            // 
            // btn_remind_later
            // 
            this.btn_remind_later.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_remind_later.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_remind_later.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_remind_later.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_remind_later.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_remind_later.ForeColor = System.Drawing.Color.White;
            this.btn_remind_later.Location = new System.Drawing.Point(448, 338);
            this.btn_remind_later.Name = "btn_remind_later";
            this.btn_remind_later.Size = new System.Drawing.Size(220, 40);
            this.btn_remind_later.TabIndex = 33;
            this.btn_remind_later.Text = "&Remind me later";
            this.btn_remind_later.UseVisualStyleBackColor = true;
            this.btn_remind_later.Click += new System.EventHandler(this.btn_remind_later_Click);
            // 
            // btn_skip
            // 
            this.btn_skip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_skip.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_skip.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_skip.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_skip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_skip.ForeColor = System.Drawing.Color.White;
            this.btn_skip.Location = new System.Drawing.Point(744, 369);
            this.btn_skip.Name = "btn_skip";
            this.btn_skip.Size = new System.Drawing.Size(94, 25);
            this.btn_skip.TabIndex = 34;
            this.btn_skip.Text = "&Skip";
            this.btn_skip.UseVisualStyleBackColor = true;
            this.btn_skip.Click += new System.EventHandler(this.btn_skip_Click);
            // 
            // lbl_changelog
            // 
            this.lbl_changelog.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lbl_changelog.BackColor = System.Drawing.Color.Transparent;
            this.lbl_changelog.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_changelog.ForeColor = System.Drawing.Color.White;
            this.lbl_changelog.Location = new System.Drawing.Point(74, 261);
            this.lbl_changelog.Name = "lbl_changelog";
            this.lbl_changelog.Size = new System.Drawing.Size(702, 32);
            this.lbl_changelog.TabIndex = 39;
            this.lbl_changelog.Text = "For more information on what has changed please see the changelog : ";
            this.lbl_changelog.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lnk_changelog
            // 
            this.lnk_changelog.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lnk_changelog.AutoSize = true;
            this.lnk_changelog.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnk_changelog.LinkColor = System.Drawing.Color.LightSkyBlue;
            this.lnk_changelog.Location = new System.Drawing.Point(233, 293);
            this.lnk_changelog.Name = "lnk_changelog";
            this.lnk_changelog.Size = new System.Drawing.Size(371, 16);
            this.lnk_changelog.TabIndex = 40;
            this.lnk_changelog.TabStop = true;
            this.lnk_changelog.Text = "https://github.com/terrymacdonald/DisplayMagician/releases";
            this.lnk_changelog.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnk_changelog_LinkClicked);
            // 
            // rtb_message
            // 
            this.rtb_message.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtb_message.Location = new System.Drawing.Point(12, 52);
            this.rtb_message.Name = "rtb_message";
            this.rtb_message.Size = new System.Drawing.Size(826, 206);
            this.rtb_message.TabIndex = 41;
            this.rtb_message.Text = "";
            // 
            // UpgradeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(850, 406);
            this.Controls.Add(this.rtb_message);
            this.Controls.Add(this.lnk_changelog);
            this.Controls.Add(this.lbl_changelog);
            this.Controls.Add(this.btn_skip);
            this.Controls.Add(this.btn_remind_later);
            this.Controls.Add(this.btn_upgrade);
            this.Controls.Add(this.lbl_title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpgradeForm";
            this.ShowIcon = false;
            this.Text = "Upgrade DisplayMagician";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.UpgradeForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Button btn_upgrade;
        private System.Windows.Forms.Button btn_remind_later;
        private System.Windows.Forms.Button btn_skip;
        private System.Windows.Forms.Label lbl_changelog;
        private System.Windows.Forms.LinkLabel lnk_changelog;
        private System.Windows.Forms.RichTextBox rtb_message;
    }
}