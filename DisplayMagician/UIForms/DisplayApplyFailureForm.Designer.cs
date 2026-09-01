namespace DisplayMagician.UIForms
{
    partial class DisplayApplyFailureForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lbl_message = new System.Windows.Forms.Label();
            btn_retry = new System.Windows.Forms.Button();
            btn_run_without_display_change = new System.Windows.Forms.Button();
            btn_cancel = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // lbl_message
            //
            lbl_message.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lbl_message.Location = new System.Drawing.Point(18, 18);
            lbl_message.Name = "lbl_message";
            lbl_message.Size = new System.Drawing.Size(528, 136);
            lbl_message.TabIndex = 0;
            //
            // btn_retry
            //
            btn_retry.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_retry.Location = new System.Drawing.Point(162, 170);
            btn_retry.Name = "btn_retry";
            btn_retry.Size = new System.Drawing.Size(90, 30);
            btn_retry.TabIndex = 1;
            btn_retry.Text = "&Retry";
            btn_retry.UseVisualStyleBackColor = true;
            btn_retry.Click += btn_retry_Click;
            //
            // btn_run_without_display_change
            //
            btn_run_without_display_change.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_run_without_display_change.Location = new System.Drawing.Point(258, 170);
            btn_run_without_display_change.Name = "btn_run_without_display_change";
            btn_run_without_display_change.Size = new System.Drawing.Size(190, 30);
            btn_run_without_display_change.TabIndex = 2;
            btn_run_without_display_change.Text = "Run without changing displays";
            btn_run_without_display_change.UseVisualStyleBackColor = true;
            btn_run_without_display_change.Click += btn_run_without_display_change_Click;
            //
            // btn_cancel
            //
            btn_cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_cancel.Location = new System.Drawing.Point(454, 170);
            btn_cancel.Name = "btn_cancel";
            btn_cancel.Size = new System.Drawing.Size(92, 30);
            btn_cancel.TabIndex = 3;
            btn_cancel.Text = "&Cancel";
            btn_cancel.UseVisualStyleBackColor = true;
            btn_cancel.Click += btn_cancel_Click;
            //
            // DisplayApplyFailureForm
            //
            AcceptButton = btn_retry;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            CancelButton = btn_cancel;
            ClientSize = new System.Drawing.Size(564, 218);
            Controls.Add(btn_cancel);
            Controls.Add(btn_run_without_display_change);
            Controls.Add(btn_retry);
            Controls.Add(lbl_message);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DisplayApplyFailureForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Display profile could not be applied";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lbl_message;
        private System.Windows.Forms.Button btn_retry;
        private System.Windows.Forms.Button btn_run_without_display_change;
        private System.Windows.Forms.Button btn_cancel;
    }
}
