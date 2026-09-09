namespace DisplayMagician.UIForms
{
    partial class AudioAccessPermissionForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lbl_heading;
        private System.Windows.Forms.Label lbl_message;
        private System.Windows.Forms.Button btn_continue;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudioAccessPermissionForm));
            lbl_heading = new System.Windows.Forms.Label();
            lbl_message = new System.Windows.Forms.Label();
            btn_continue = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // lbl_heading
            // 
            lbl_heading.AutoSize = true;
            lbl_heading.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_heading.Location = new System.Drawing.Point(24, 22);
            lbl_heading.Name = "lbl_heading";
            lbl_heading.Size = new System.Drawing.Size(281, 21);
            lbl_heading.TabIndex = 0;
            lbl_heading.Text = "Audio access permission is required";
            // 
            // lbl_message
            // 
            lbl_message.Location = new System.Drawing.Point(27, 61);
            lbl_message.Name = "lbl_message";
            lbl_message.Size = new System.Drawing.Size(546, 61);
            lbl_message.TabIndex = 1;
            lbl_message.Text = resources.GetString("lbl_message.Text");
            // 
            // btn_continue
            // 
            btn_continue.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_continue.DialogResult = System.Windows.Forms.DialogResult.OK;
            btn_continue.Location = new System.Drawing.Point(462, 137);
            btn_continue.Name = "btn_continue";
            btn_continue.Size = new System.Drawing.Size(111, 31);
            btn_continue.TabIndex = 2;
            btn_continue.Text = "Continue";
            btn_continue.UseVisualStyleBackColor = true;
            btn_continue.Click += btn_continue_Click;
            // 
            // AudioAccessPermissionForm
            // 
            AcceptButton = btn_continue;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(601, 187);
            Controls.Add(btn_continue);
            Controls.Add(lbl_message);
            Controls.Add(lbl_heading);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AudioAccessPermissionForm";
            ShowIcon = true;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "DisplayMagician - Audio Access";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
