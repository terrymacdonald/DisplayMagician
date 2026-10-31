namespace DisplayMagician.UIForms
{
    partial class InstallationResetForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lbl_warning = new System.Windows.Forms.Label();
            lbl_status = new System.Windows.Forms.Label();
            btn_reset = new System.Windows.Forms.Button();
            btn_close = new System.Windows.Forms.Button();
            SuspendLayout();
            lbl_warning.Location = new System.Drawing.Point(20, 20);
            lbl_warning.Size = new System.Drawing.Size(580, 160);
            lbl_warning.Text = "Reset DisplayMagician for ALL Windows users?\r\n\r\nThis permanently deletes all settings, display/audio profiles, shortcuts, caches, backups and paired-device configuration. No reset backup will be created. Log files will be kept.\r\n\r\nCreate a Support ZIP before continuing if you need troubleshooting evidence. All other DisplayMagician windows will close. Active shortcuts must finish display/audio restoration before deletion can proceed. Installed games and applications will remain installed.";
            lbl_status.Location = new System.Drawing.Point(20, 190);
            lbl_status.Size = new System.Drawing.Size(580, 90);
            lbl_status.Text = "Select Reset DisplayMagician to confirm, or Cancel to keep your data.";
            btn_reset.Location = new System.Drawing.Point(238, 300);
            btn_reset.Size = new System.Drawing.Size(220, 38);
            btn_reset.Text = "Reset DisplayMagician";
            btn_reset.Click += btn_reset_Click;
            btn_close.Location = new System.Drawing.Point(474, 300);
            btn_close.Size = new System.Drawing.Size(126, 38);
            btn_close.Text = "Cancel";
            btn_close.TabIndex = 0;
            btn_reset.TabIndex = 1;
            btn_close.Click += btn_close_Click;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(620, 360);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Reset DisplayMagician — permanent data deletion";
            Name = "InstallationResetForm";
            Controls.Add(lbl_warning);
            Controls.Add(lbl_status);
            Controls.Add(btn_reset);
            Controls.Add(btn_close);
            CancelButton = btn_close;
            ActiveControl = btn_close;
            FormClosing += InstallationResetForm_FormClosing;
            ResumeLayout(false);
        }
        private System.Windows.Forms.Label lbl_warning;
        private System.Windows.Forms.Label lbl_status;
        private System.Windows.Forms.Button btn_reset;
        private System.Windows.Forms.Button btn_close;
    }
}
