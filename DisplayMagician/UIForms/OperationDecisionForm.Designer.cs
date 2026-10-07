namespace DisplayMagician.UIForms
{
    partial class OperationDecisionForm
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
            btn_continue = new System.Windows.Forms.Button();
            btn_stop_and_restore = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // lbl_message
            //
            lbl_message.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lbl_message.Location = new System.Drawing.Point(18, 18);
            lbl_message.Name = "lbl_message";
            lbl_message.Size = new System.Drawing.Size(528, 110);
            lbl_message.TabIndex = 0;
            //
            // btn_continue
            //
            btn_continue.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_continue.Location = new System.Drawing.Point(272, 145);
            btn_continue.Name = "btn_continue";
            btn_continue.Size = new System.Drawing.Size(132, 30);
            btn_continue.TabIndex = 1;
            btn_continue.Text = "&Continue";
            btn_continue.UseVisualStyleBackColor = true;
            btn_continue.Click += btn_continue_Click;
            //
            // btn_stop_and_restore
            //
            btn_stop_and_restore.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_stop_and_restore.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_stop_and_restore.Location = new System.Drawing.Point(410, 145);
            btn_stop_and_restore.Name = "btn_stop_and_restore";
            btn_stop_and_restore.Size = new System.Drawing.Size(136, 30);
            btn_stop_and_restore.TabIndex = 2;
            btn_stop_and_restore.Text = "&Stop and restore";
            btn_stop_and_restore.UseVisualStyleBackColor = true;
            btn_stop_and_restore.Click += btn_stop_and_restore_Click;
            //
            // OperationDecisionForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            CancelButton = btn_stop_and_restore;
            ClientSize = new System.Drawing.Size(564, 193);
            Controls.Add(btn_stop_and_restore);
            Controls.Add(btn_continue);
            Controls.Add(lbl_message);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "OperationDecisionForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "DisplayMagician needs your decision";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lbl_message;
        private System.Windows.Forms.Button btn_continue;
        private System.Windows.Forms.Button btn_stop_and_restore;
    }
}
