namespace DisplayMagician.UIForms
{
    partial class AudioProfilesForm
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
            btn_apply_audio_profile = new System.Windows.Forms.Button();
            gb_selected_audio_settings = new System.Windows.Forms.GroupBox();
            txt_audio_profile_settings = new System.Windows.Forms.TextBox();
            lbl_audio_profiles = new System.Windows.Forms.Label();
            lb_audio_profiles = new System.Windows.Forms.ListBox();
            btn_delete_audio_profile = new System.Windows.Forms.Button();
            btn_rename_audio_profile = new System.Windows.Forms.Button();
            btn_create_audio_profile = new System.Windows.Forms.Button();
            btn_update_audio_profile = new System.Windows.Forms.Button();
            btn_back = new System.Windows.Forms.Button();
            gb_selected_audio_settings.SuspendLayout();
            SuspendLayout();
            // 
            // btn_apply_audio_profile
            // 
            btn_apply_audio_profile.Enabled = false;
            btn_apply_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_apply_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_apply_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_apply_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_apply_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_apply_audio_profile.Location = new System.Drawing.Point(156, 1191);
            btn_apply_audio_profile.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            btn_apply_audio_profile.Name = "btn_apply_audio_profile";
            btn_apply_audio_profile.Size = new System.Drawing.Size(550, 66);
            btn_apply_audio_profile.TabIndex = 6;
            btn_apply_audio_profile.Text = "&Apply Selected Profile";
            btn_apply_audio_profile.UseVisualStyleBackColor = true;
            btn_apply_audio_profile.Click += btn_apply_audio_profile_Click;
            // 
            // gb_selected_audio_settings
            // 
            gb_selected_audio_settings.Anchor = System.Windows.Forms.AnchorStyles.Right;
            gb_selected_audio_settings.Controls.Add(txt_audio_profile_settings);
            gb_selected_audio_settings.Enabled = false;
            gb_selected_audio_settings.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_selected_audio_settings.ForeColor = System.Drawing.Color.White;
            gb_selected_audio_settings.Location = new System.Drawing.Point(879, 45);
            gb_selected_audio_settings.Margin = new System.Windows.Forms.Padding(6);
            gb_selected_audio_settings.Name = "gb_selected_audio_settings";
            gb_selected_audio_settings.Padding = new System.Windows.Forms.Padding(6);
            gb_selected_audio_settings.Size = new System.Drawing.Size(1411, 1280);
            gb_selected_audio_settings.TabIndex = 5;
            gb_selected_audio_settings.TabStop = false;
            gb_selected_audio_settings.Text = "Selected Audio Profile Settings";
            gb_selected_audio_settings.Paint += groupbox_Paint;
            // 
            // txt_audio_profile_settings
            // 
            txt_audio_profile_settings.AcceptsReturn = true;
            txt_audio_profile_settings.AcceptsTab = true;
            txt_audio_profile_settings.BackColor = System.Drawing.Color.White;
            txt_audio_profile_settings.ForeColor = System.Drawing.Color.Black;
            txt_audio_profile_settings.Location = new System.Drawing.Point(45, 93);
            txt_audio_profile_settings.Margin = new System.Windows.Forms.Padding(6);
            txt_audio_profile_settings.Multiline = true;
            txt_audio_profile_settings.Name = "txt_audio_profile_settings";
            txt_audio_profile_settings.ReadOnly = true;
            txt_audio_profile_settings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txt_audio_profile_settings.Size = new System.Drawing.Size(1319, 1160);
            txt_audio_profile_settings.TabIndex = 0;
            // 
            // lbl_audio_profiles
            // 
            lbl_audio_profiles.AutoSize = true;
            lbl_audio_profiles.Font = new System.Drawing.Font("Segoe UI", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_audio_profiles.ForeColor = System.Drawing.Color.White;
            lbl_audio_profiles.Location = new System.Drawing.Point(67, 61);
            lbl_audio_profiles.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            lbl_audio_profiles.Name = "lbl_audio_profiles";
            lbl_audio_profiles.Size = new System.Drawing.Size(425, 37);
            lbl_audio_profiles.TabIndex = 0;
            lbl_audio_profiles.Text = "Select an Audio Profile to manage:";
            lbl_audio_profiles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl_audio_profiles.Paint += label_Paint;
            // 
            // lb_audio_profiles
            // 
            lb_audio_profiles.BackColor = System.Drawing.Color.White;
            lb_audio_profiles.Font = new System.Drawing.Font("Segoe UI", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lb_audio_profiles.ForeColor = System.Drawing.Color.Black;
            lb_audio_profiles.FormattingEnabled = true;
            lb_audio_profiles.Location = new System.Drawing.Point(67, 114);
            lb_audio_profiles.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            lb_audio_profiles.Name = "lb_audio_profiles";
            lb_audio_profiles.Size = new System.Drawing.Size(725, 804);
            lb_audio_profiles.TabIndex = 1;
            lb_audio_profiles.SelectedIndexChanged += lb_audio_profiles_SelectedIndexChanged;
            // 
            // btn_delete_audio_profile
            // 
            btn_delete_audio_profile.Enabled = false;
            btn_delete_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_delete_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_delete_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_delete_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_delete_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_delete_audio_profile.Location = new System.Drawing.Point(156, 1373);
            btn_delete_audio_profile.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            btn_delete_audio_profile.Name = "btn_delete_audio_profile";
            btn_delete_audio_profile.Size = new System.Drawing.Size(550, 66);
            btn_delete_audio_profile.TabIndex = 4;
            btn_delete_audio_profile.Text = "&Delete Selected Profile";
            btn_delete_audio_profile.UseVisualStyleBackColor = true;
            btn_delete_audio_profile.Click += btn_delete_audio_profile_Click;
            // 
            // btn_rename_audio_profile
            // 
            btn_rename_audio_profile.Enabled = false;
            btn_rename_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_rename_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_rename_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_rename_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_rename_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_rename_audio_profile.Location = new System.Drawing.Point(156, 1284);
            btn_rename_audio_profile.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            btn_rename_audio_profile.Name = "btn_rename_audio_profile";
            btn_rename_audio_profile.Size = new System.Drawing.Size(550, 66);
            btn_rename_audio_profile.TabIndex = 5;
            btn_rename_audio_profile.Text = "&Rename Selected Profile";
            btn_rename_audio_profile.UseVisualStyleBackColor = true;
            btn_rename_audio_profile.Click += btn_rename_audio_profile_Click;
            // 
            // btn_create_audio_profile
            // 
            btn_create_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_create_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_create_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_create_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_create_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_create_audio_profile.Location = new System.Drawing.Point(156, 999);
            btn_create_audio_profile.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            btn_create_audio_profile.Name = "btn_create_audio_profile";
            btn_create_audio_profile.Size = new System.Drawing.Size(550, 66);
            btn_create_audio_profile.TabIndex = 2;
            btn_create_audio_profile.Text = "&Create New Profile from Current Audio";
            btn_create_audio_profile.UseVisualStyleBackColor = true;
            btn_create_audio_profile.Click += btn_create_audio_profile_Click;
            // 
            // btn_update_audio_profile
            // 
            btn_update_audio_profile.Enabled = false;
            btn_update_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_update_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_update_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_update_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_update_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_update_audio_profile.Location = new System.Drawing.Point(156, 1095);
            btn_update_audio_profile.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            btn_update_audio_profile.Name = "btn_update_audio_profile";
            btn_update_audio_profile.Size = new System.Drawing.Size(550, 66);
            btn_update_audio_profile.TabIndex = 3;
            btn_update_audio_profile.Text = "&Update Profile from Current Audio";
            btn_update_audio_profile.UseVisualStyleBackColor = true;
            btn_update_audio_profile.Click += btn_update_audio_profile_Click;
            // 
            // btn_back
            // 
            btn_back.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(2127, 1381);
            btn_back.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(163, 58);
            btn_back.TabIndex = 10;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // AudioProfilesForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(2347, 1503);
            Controls.Add(btn_back);
            Controls.Add(btn_apply_audio_profile);
            Controls.Add(lbl_audio_profiles);
            Controls.Add(gb_selected_audio_settings);
            Controls.Add(btn_update_audio_profile);
            Controls.Add(btn_create_audio_profile);
            Controls.Add(lb_audio_profiles);
            Controls.Add(btn_rename_audio_profile);
            Controls.Add(btn_delete_audio_profile);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            MaximizeBox = false;
            Name = "AudioProfilesForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Audio Profiles";
            Load += AudioProfilesForm_Load;
            gb_selected_audio_settings.ResumeLayout(false);
            gb_selected_audio_settings.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.GroupBox gb_selected_audio_settings;
        private System.Windows.Forms.TextBox txt_audio_profile_settings;
        private System.Windows.Forms.Label lbl_audio_profiles;
        private System.Windows.Forms.ListBox lb_audio_profiles;
        private System.Windows.Forms.Button btn_delete_audio_profile;
        private System.Windows.Forms.Button btn_create_audio_profile;
        private System.Windows.Forms.Button btn_update_audio_profile;
        private System.Windows.Forms.Button btn_rename_audio_profile;
        private System.Windows.Forms.Button btn_apply_audio_profile;
        private System.Windows.Forms.Button btn_back;
    }
}
