namespace DisplayMagician.UIForms
{
    partial class ShortcutConflictForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lbl_message = new System.Windows.Forms.Label();
            btn_reopen = new System.Windows.Forms.Button();
            btn_save_new = new System.Windows.Forms.Button();
            btn_cancel = new System.Windows.Forms.Button();
            SuspendLayout();
            // lbl_message
            lbl_message.Location = new System.Drawing.Point(18, 18);
            lbl_message.Name = "lbl_message";
            lbl_message.Size = new System.Drawing.Size(564, 106);
            lbl_message.TabIndex = 0;
            lbl_message.Text = "This shortcut was changed or deleted elsewhere. Your draft has not been saved.\r\n\r\nReopen the latest version to replace your draft and continue editing, or save your draft as a new shortcut. Cancel keeps your draft open.";
            // btn_reopen
            btn_reopen.DialogResult = System.Windows.Forms.DialogResult.Yes;
            btn_reopen.Location = new System.Drawing.Point(18, 140);
            btn_reopen.Name = "btn_reopen";
            btn_reopen.Size = new System.Drawing.Size(168, 32);
            btn_reopen.TabIndex = 1;
            btn_reopen.Text = "&Reopen latest version";
            btn_reopen.UseVisualStyleBackColor = true;
            // btn_save_new
            btn_save_new.DialogResult = System.Windows.Forms.DialogResult.No;
            btn_save_new.Location = new System.Drawing.Point(198, 140);
            btn_save_new.Name = "btn_save_new";
            btn_save_new.Size = new System.Drawing.Size(230, 32);
            btn_save_new.TabIndex = 2;
            btn_save_new.Text = "&Save as new shortcut";
            btn_save_new.UseVisualStyleBackColor = true;
            // btn_cancel
            btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_cancel.Location = new System.Drawing.Point(444, 140);
            btn_cancel.Name = "btn_cancel";
            btn_cancel.Size = new System.Drawing.Size(138, 32);
            btn_cancel.TabIndex = 3;
            btn_cancel.Text = "&Cancel";
            btn_cancel.UseVisualStyleBackColor = true;
            // ShortcutConflictForm
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            AcceptButton = btn_cancel;
            CancelButton = btn_cancel;
            ClientSize = new System.Drawing.Size(600, 190);
            Controls.Add(lbl_message);
            Controls.Add(btn_reopen);
            Controls.Add(btn_save_new);
            Controls.Add(btn_cancel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ShortcutConflictForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Shortcut changed elsewhere";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lbl_message;
        private System.Windows.Forms.Button btn_reopen;
        private System.Windows.Forms.Button btn_save_new;
        private System.Windows.Forms.Button btn_cancel;
    }
}
