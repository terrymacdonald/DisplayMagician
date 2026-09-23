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
            btn_force_release = new System.Windows.Forms.Button();
            btn_restart_user_agent = new System.Windows.Forms.Button();
            btn_restart_control_service = new System.Windows.Forms.Button();
            lbl_result = new System.Windows.Forms.Label();
            btn_back = new System.Windows.Forms.Button();
            gb_service_release = new System.Windows.Forms.GroupBox();
            gb_status.SuspendLayout();
            gb_service_release.SuspendLayout();
            SuspendLayout();
            // 
            // gb_status
            // 
            gb_status.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gb_status.Controls.Add(lbl_status);
            gb_status.Controls.Add(lbl_administrator_notice);
            gb_status.Controls.Add(btn_refresh);
            gb_status.ForeColor = System.Drawing.Color.White;
            gb_status.Location = new System.Drawing.Point(18, 18);
            gb_status.Name = "gb_status";
            gb_status.Size = new System.Drawing.Size(624, 126);
            gb_status.TabIndex = 0;
            gb_status.TabStop = false;
            gb_status.Text = "Control Service Status";
            // 
            // lbl_status
            // 
            lbl_status.ForeColor = System.Drawing.Color.White;
            lbl_status.Location = new System.Drawing.Point(18, 26);
            lbl_status.Name = "lbl_status";
            lbl_status.Size = new System.Drawing.Size(588, 40);
            lbl_status.TabIndex = 0;
            lbl_status.Text = "Loading Control Service status...";
            // 
            // lbl_administrator_notice
            // 
            lbl_administrator_notice.ForeColor = System.Drawing.Color.LightGray;
            lbl_administrator_notice.Location = new System.Drawing.Point(168, 84);
            lbl_administrator_notice.Name = "lbl_administrator_notice";
            lbl_administrator_notice.Size = new System.Drawing.Size(451, 36);
            lbl_administrator_notice.TabIndex = 1;
            lbl_administrator_notice.Text = "IMPORTANT: Emergency recovery actions are limited to elevated administrators.";
            lbl_administrator_notice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_refresh
            // 
            btn_refresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_refresh.ForeColor = System.Drawing.Color.White;
            btn_refresh.Location = new System.Drawing.Point(6, 88);
            btn_refresh.Name = "btn_refresh";
            btn_refresh.Size = new System.Drawing.Size(156, 32);
            btn_refresh.TabIndex = 2;
            btn_refresh.Text = "Refresh Status";
            btn_refresh.UseVisualStyleBackColor = true;
            btn_refresh.Click += btn_refresh_Click;
            // 
            // btn_force_release
            // 
            btn_force_release.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_force_release.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_force_release.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_force_release.ForeColor = System.Drawing.Color.White;
            btn_force_release.Location = new System.Drawing.Point(426, 24);
            btn_force_release.Name = "btn_force_release";
            btn_force_release.Size = new System.Drawing.Size(180, 32);
            btn_force_release.TabIndex = 3;
            btn_force_release.Text = "Clear Stuck Display Change";
            btn_force_release.UseVisualStyleBackColor = true;
            btn_force_release.Click += btn_force_release_Click;
            //
            // btn_restart_user_agent
            //
            btn_restart_user_agent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_restart_user_agent.ForeColor = System.Drawing.Color.White;
            btn_restart_user_agent.Location = new System.Drawing.Point(18, 24);
            btn_restart_user_agent.Name = "btn_restart_user_agent";
            btn_restart_user_agent.Size = new System.Drawing.Size(180, 32);
            btn_restart_user_agent.TabIndex = 4;
            btn_restart_user_agent.Text = "Restart User Agent";
            btn_restart_user_agent.UseVisualStyleBackColor = true;
            btn_restart_user_agent.Click += btn_restart_user_agent_Click;
            //
            // btn_restart_control_service
            //
            btn_restart_control_service.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_restart_control_service.ForeColor = System.Drawing.Color.White;
            btn_restart_control_service.Location = new System.Drawing.Point(222, 24);
            btn_restart_control_service.Name = "btn_restart_control_service";
            btn_restart_control_service.Size = new System.Drawing.Size(180, 32);
            btn_restart_control_service.TabIndex = 5;
            btn_restart_control_service.Text = "Restart Control Service";
            btn_restart_control_service.UseVisualStyleBackColor = true;
            btn_restart_control_service.Click += btn_restart_control_service_Click;
            // 
            // lbl_result
            // 
            lbl_result.ForeColor = System.Drawing.Color.White;
            lbl_result.Location = new System.Drawing.Point(18, 68);
            lbl_result.Name = "lbl_result";
            lbl_result.Size = new System.Drawing.Size(588, 25);
            lbl_result.TabIndex = 4;
            lbl_result.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_back
            // 
            btn_back.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(554, 295);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 28);
            btn_back.TabIndex = 5;
            btn_back.Text = "Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // gb_service_release
            // 
            gb_service_release.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            gb_service_release.Controls.Add(btn_force_release);
            gb_service_release.Controls.Add(btn_restart_user_agent);
            gb_service_release.Controls.Add(btn_restart_control_service);
            gb_service_release.Controls.Add(lbl_result);
            gb_service_release.ForeColor = System.Drawing.Color.White;
            gb_service_release.Location = new System.Drawing.Point(18, 163);
            gb_service_release.Name = "gb_service_release";
            gb_service_release.Size = new System.Drawing.Size(624, 112);
            gb_service_release.TabIndex = 6;
            gb_service_release.TabStop = false;
            gb_service_release.Text = "Recovery Actions";
            // 
            // ServiceRecoveryForm
            // 
            AcceptButton = btn_refresh;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            CancelButton = btn_back;
            ClientSize = new System.Drawing.Size(660, 346);
            Controls.Add(gb_service_release);
            Controls.Add(btn_back);
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
            gb_service_release.ResumeLayout(false);
            gb_service_release.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox gb_status;
        private System.Windows.Forms.Label lbl_status;
        private System.Windows.Forms.Label lbl_administrator_notice;
        private System.Windows.Forms.Button btn_refresh;
        private System.Windows.Forms.Button btn_force_release;
        private System.Windows.Forms.Button btn_restart_user_agent;
        private System.Windows.Forms.Button btn_restart_control_service;
        private System.Windows.Forms.Label lbl_result;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.GroupBox gb_service_release;
    }
}
