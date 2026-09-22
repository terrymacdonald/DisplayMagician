namespace DisplayMagician.UIForms
{
    partial class ServiceRecoveryForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            gb_status = new System.Windows.Forms.GroupBox();
            lbl_status = new System.Windows.Forms.Label();
            lbl_administrator_notice = new System.Windows.Forms.Label();
            btn_refresh = new System.Windows.Forms.Button();
            btn_create_bundle = new System.Windows.Forms.Button();
            btn_force_release = new System.Windows.Forms.Button();
            lbl_confirmation = new System.Windows.Forms.Label();
            txt_confirmation = new System.Windows.Forms.TextBox();
            lbl_result = new System.Windows.Forms.Label();
            btn_back = new System.Windows.Forms.Button();
            gb_status.SuspendLayout();
            SuspendLayout();
            // 
            // gb_status
            // 
            gb_status.Controls.Add(lbl_status);
            gb_status.ForeColor = System.Drawing.Color.White;
            gb_status.Location = new System.Drawing.Point(18, 18);
            gb_status.Name = "gb_status";
            gb_status.Size = new System.Drawing.Size(624, 112);
            gb_status.TabIndex = 0;
            gb_status.TabStop = false;
            gb_status.Text = "Control Service Status";
            // 
            // lbl_status
            // 
            lbl_status.ForeColor = System.Drawing.Color.White;
            lbl_status.Location = new System.Drawing.Point(18, 26);
            lbl_status.Name = "lbl_status";
            lbl_status.Size = new System.Drawing.Size(588, 70);
            lbl_status.TabIndex = 0;
            lbl_status.Text = "Loading Control Service status...";
            // 
            // lbl_administrator_notice
            // 
            lbl_administrator_notice.ForeColor = System.Drawing.Color.LightGray;
            lbl_administrator_notice.Location = new System.Drawing.Point(36, 142);
            lbl_administrator_notice.Name = "lbl_administrator_notice";
            lbl_administrator_notice.Size = new System.Drawing.Size(588, 36);
            lbl_administrator_notice.TabIndex = 1;
            lbl_administrator_notice.Text = "Machine support actions are limited to elevated administrators.";
            lbl_administrator_notice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_refresh
            // 
            btn_refresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_refresh.ForeColor = System.Drawing.Color.White;
            btn_refresh.Location = new System.Drawing.Point(36, 196);
            btn_refresh.Name = "btn_refresh";
            btn_refresh.Size = new System.Drawing.Size(156, 32);
            btn_refresh.TabIndex = 2;
            btn_refresh.Text = "Refresh Status";
            btn_refresh.UseVisualStyleBackColor = true;
            btn_refresh.Click += btn_refresh_Click;
            // 
            // btn_create_bundle
            // 
            btn_create_bundle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_create_bundle.ForeColor = System.Drawing.Color.White;
            btn_create_bundle.Location = new System.Drawing.Point(252, 196);
            btn_create_bundle.Name = "btn_create_bundle";
            btn_create_bundle.Size = new System.Drawing.Size(156, 32);
            btn_create_bundle.TabIndex = 3;
            btn_create_bundle.Text = "Create Machine Bundle";
            btn_create_bundle.UseVisualStyleBackColor = true;
            btn_create_bundle.Click += btn_create_bundle_Click;
            // 
            // btn_force_release
            // 
            btn_force_release.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_force_release.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_force_release.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_force_release.ForeColor = System.Drawing.Color.White;
            btn_force_release.Location = new System.Drawing.Point(468, 196);
            btn_force_release.Name = "btn_force_release";
            btn_force_release.Size = new System.Drawing.Size(156, 32);
            btn_force_release.TabIndex = 4;
            btn_force_release.Text = "Force Release Control";
            btn_force_release.UseVisualStyleBackColor = true;
            btn_force_release.Click += btn_force_release_Click;
            // 
            // lbl_confirmation
            // 
            lbl_confirmation.ForeColor = System.Drawing.Color.White;
            lbl_confirmation.Location = new System.Drawing.Point(36, 242);
            lbl_confirmation.Name = "lbl_confirmation";
            lbl_confirmation.Size = new System.Drawing.Size(260, 24);
            lbl_confirmation.TabIndex = 5;
            lbl_confirmation.Text = "Type FORCE RELEASE to enable emergency release:";
            lbl_confirmation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txt_confirmation
            // 
            txt_confirmation.Location = new System.Drawing.Point(310, 243);
            txt_confirmation.Name = "txt_confirmation";
            txt_confirmation.Size = new System.Drawing.Size(314, 23);
            txt_confirmation.TabIndex = 5;
            txt_confirmation.TextChanged += txt_confirmation_TextChanged;
            // 
            // lbl_result
            // 
            lbl_result.ForeColor = System.Drawing.Color.White;
            lbl_result.Location = new System.Drawing.Point(36, 282);
            lbl_result.Name = "lbl_result";
            lbl_result.Size = new System.Drawing.Size(588, 60);
            lbl_result.TabIndex = 5;
            lbl_result.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_back
            // 
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(536, 360);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 28);
            btn_back.TabIndex = 6;
            btn_back.Text = "Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // ServiceRecoveryForm
            // 
            AcceptButton = btn_refresh;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            CancelButton = btn_back;
            ClientSize = new System.Drawing.Size(660, 406);
            Controls.Add(btn_back);
            Controls.Add(lbl_result);
            Controls.Add(txt_confirmation);
            Controls.Add(lbl_confirmation);
            Controls.Add(btn_force_release);
            Controls.Add(btn_create_bundle);
            Controls.Add(btn_refresh);
            Controls.Add(lbl_administrator_notice);
            Controls.Add(gb_status);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ServiceRecoveryForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Service Recovery";
            Load += ServiceRecoveryForm_Load;
            gb_status.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox gb_status;
        private System.Windows.Forms.Label lbl_status;
        private System.Windows.Forms.Label lbl_administrator_notice;
        private System.Windows.Forms.Button btn_refresh;
        private System.Windows.Forms.Button btn_create_bundle;
        private System.Windows.Forms.Button btn_force_release;
        private System.Windows.Forms.Label lbl_confirmation;
        private System.Windows.Forms.TextBox txt_confirmation;
        private System.Windows.Forms.Label lbl_result;
        private System.Windows.Forms.Button btn_back;
    }
}
