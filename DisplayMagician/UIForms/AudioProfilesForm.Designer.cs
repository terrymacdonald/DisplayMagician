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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudioProfilesForm));
            lbl_audio_profiles = new System.Windows.Forms.Label();
            lb_audio_profiles = new System.Windows.Forms.ListBox();
            btn_create_audio_profile = new System.Windows.Forms.Button();
            btn_update_audio_profile = new System.Windows.Forms.Button();
            btn_apply_audio_profile = new System.Windows.Forms.Button();
            btn_rename_audio_profile = new System.Windows.Forms.Button();
            btn_delete_audio_profile = new System.Windows.Forms.Button();
            gb_selected_audio_settings = new System.Windows.Forms.GroupBox();
            txt_audio_profile_settings = new System.Windows.Forms.TextBox();
            p_audio_profile_advisory = new System.Windows.Forms.Panel();
            lbl_audio_profile_advisory = new System.Windows.Forms.Label();
            btn_back = new System.Windows.Forms.Button();
            lbl_heading_text = new System.Windows.Forms.Label();
            gb_selected_audio_settings.SuspendLayout();
            p_audio_profile_advisory.SuspendLayout();
            SuspendLayout();
            // 
            // lbl_audio_profiles
            // 
            lbl_audio_profiles.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lbl_audio_profiles.AutoSize = true;
            lbl_audio_profiles.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_audio_profiles.ForeColor = System.Drawing.Color.White;
            lbl_audio_profiles.Location = new System.Drawing.Point(107, 76);
            lbl_audio_profiles.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            lbl_audio_profiles.Name = "lbl_audio_profiles";
            lbl_audio_profiles.Size = new System.Drawing.Size(236, 20);
            lbl_audio_profiles.TabIndex = 0;
            lbl_audio_profiles.Text = "Select an Audio Profile to manage";
            lbl_audio_profiles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lbl_audio_profiles.Paint += label_Paint;
            // 
            // lb_audio_profiles
            // 
            lb_audio_profiles.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lb_audio_profiles.BackColor = System.Drawing.Color.White;
            lb_audio_profiles.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lb_audio_profiles.ForeColor = System.Drawing.Color.Black;
            lb_audio_profiles.FormattingEnabled = true;
            lb_audio_profiles.Location = new System.Drawing.Point(31, 98);
            lb_audio_profiles.Name = "lb_audio_profiles";
            lb_audio_profiles.Size = new System.Drawing.Size(386, 244);
            lb_audio_profiles.TabIndex = 1;
            lb_audio_profiles.SelectedIndexChanged += lb_audio_profiles_SelectedIndexChanged;
            // 
            // btn_create_audio_profile
            // 
            btn_create_audio_profile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btn_create_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_create_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_create_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_create_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_create_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_create_audio_profile.Location = new System.Drawing.Point(31, 350);
            btn_create_audio_profile.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btn_create_audio_profile.Name = "btn_create_audio_profile";
            btn_create_audio_profile.Size = new System.Drawing.Size(386, 46);
            btn_create_audio_profile.TabIndex = 2;
            btn_create_audio_profile.Text = "&Create New Profile from Current Audio";
            btn_create_audio_profile.UseVisualStyleBackColor = true;
            btn_create_audio_profile.Click += btn_create_audio_profile_Click;
            // 
            // btn_update_audio_profile
            // 
            btn_update_audio_profile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btn_update_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_update_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_update_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_update_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_update_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_update_audio_profile.Location = new System.Drawing.Point(31, 404);
            btn_update_audio_profile.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btn_update_audio_profile.Name = "btn_update_audio_profile";
            btn_update_audio_profile.Size = new System.Drawing.Size(386, 46);
            btn_update_audio_profile.TabIndex = 3;
            btn_update_audio_profile.Text = "&Update Profile from Current Audio";
            btn_update_audio_profile.UseVisualStyleBackColor = true;
            btn_update_audio_profile.Click += btn_update_audio_profile_Click;
            // 
            // btn_apply_audio_profile
            // 
            btn_apply_audio_profile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btn_apply_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_apply_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_apply_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_apply_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_apply_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_apply_audio_profile.Location = new System.Drawing.Point(31, 458);
            btn_apply_audio_profile.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btn_apply_audio_profile.Name = "btn_apply_audio_profile";
            btn_apply_audio_profile.Size = new System.Drawing.Size(386, 46);
            btn_apply_audio_profile.TabIndex = 4;
            btn_apply_audio_profile.Text = "&Apply Selected Profile Now";
            btn_apply_audio_profile.UseVisualStyleBackColor = true;
            btn_apply_audio_profile.Click += btn_apply_audio_profile_Click;
            // 
            // btn_rename_audio_profile
            // 
            btn_rename_audio_profile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btn_rename_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_rename_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_rename_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_rename_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_rename_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_rename_audio_profile.Location = new System.Drawing.Point(31, 512);
            btn_rename_audio_profile.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btn_rename_audio_profile.Name = "btn_rename_audio_profile";
            btn_rename_audio_profile.Size = new System.Drawing.Size(386, 46);
            btn_rename_audio_profile.TabIndex = 5;
            btn_rename_audio_profile.Text = "&Rename Selected Profile";
            btn_rename_audio_profile.UseVisualStyleBackColor = true;
            btn_rename_audio_profile.Click += btn_rename_audio_profile_Click;
            // 
            // btn_delete_audio_profile
            // 
            btn_delete_audio_profile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btn_delete_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_delete_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_delete_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_delete_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_delete_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_delete_audio_profile.Location = new System.Drawing.Point(31, 570);
            btn_delete_audio_profile.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btn_delete_audio_profile.Name = "btn_delete_audio_profile";
            btn_delete_audio_profile.Size = new System.Drawing.Size(386, 46);
            btn_delete_audio_profile.TabIndex = 6;
            btn_delete_audio_profile.Text = "&Delete Selected Profile";
            btn_delete_audio_profile.UseVisualStyleBackColor = true;
            btn_delete_audio_profile.Click += btn_delete_audio_profile_Click;
            // 
            // gb_selected_audio_settings
            // 
            gb_selected_audio_settings.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gb_selected_audio_settings.Controls.Add(txt_audio_profile_settings);
            gb_selected_audio_settings.Enabled = false;
            gb_selected_audio_settings.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_selected_audio_settings.ForeColor = System.Drawing.Color.White;
            gb_selected_audio_settings.Location = new System.Drawing.Point(453, 76);
            gb_selected_audio_settings.Name = "gb_selected_audio_settings";
            gb_selected_audio_settings.Padding = new System.Windows.Forms.Padding(6);
            gb_selected_audio_settings.Size = new System.Drawing.Size(734, 466);
            gb_selected_audio_settings.TabIndex = 0;
            gb_selected_audio_settings.TabStop = false;
            gb_selected_audio_settings.Text = "Selected Audio Profile Settings";
            gb_selected_audio_settings.Paint += groupbox_Paint;
            // 
            // txt_audio_profile_settings
            // 
            txt_audio_profile_settings.AcceptsReturn = true;
            txt_audio_profile_settings.AcceptsTab = true;
            txt_audio_profile_settings.BackColor = System.Drawing.Color.White;
            txt_audio_profile_settings.Dock = System.Windows.Forms.DockStyle.Fill;
            txt_audio_profile_settings.ForeColor = System.Drawing.Color.Black;
            txt_audio_profile_settings.Location = new System.Drawing.Point(6, 26);
            txt_audio_profile_settings.Margin = new System.Windows.Forms.Padding(6);
            txt_audio_profile_settings.Multiline = true;
            txt_audio_profile_settings.Name = "txt_audio_profile_settings";
            txt_audio_profile_settings.ReadOnly = true;
            txt_audio_profile_settings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txt_audio_profile_settings.Size = new System.Drawing.Size(722, 512);
            txt_audio_profile_settings.TabIndex = 0;
            // 
            // p_audio_profile_advisory
            //
            p_audio_profile_advisory.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            p_audio_profile_advisory.BackColor = System.Drawing.Color.FromArgb(255, 193, 7);
            p_audio_profile_advisory.Controls.Add(lbl_audio_profile_advisory);
            p_audio_profile_advisory.Location = new System.Drawing.Point(453, 550);
            p_audio_profile_advisory.Name = "p_audio_profile_advisory";
            p_audio_profile_advisory.Size = new System.Drawing.Size(734, 70);
            p_audio_profile_advisory.TabIndex = 8;
            p_audio_profile_advisory.Visible = false;
            //
            // lbl_audio_profile_advisory
            //
            lbl_audio_profile_advisory.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_audio_profile_advisory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_audio_profile_advisory.ForeColor = System.Drawing.Color.Black;
            lbl_audio_profile_advisory.Location = new System.Drawing.Point(0, 0);
            lbl_audio_profile_advisory.Name = "lbl_audio_profile_advisory";
            lbl_audio_profile_advisory.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            lbl_audio_profile_advisory.Size = new System.Drawing.Size(734, 70);
            lbl_audio_profile_advisory.TabIndex = 0;
            lbl_audio_profile_advisory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btn_back
            // 
            btn_back.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(1099, 626);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 27);
            btn_back.TabIndex = 7;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // lbl_heading_text
            // 
            lbl_heading_text.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lbl_heading_text.AutoSize = true;
            lbl_heading_text.BackColor = System.Drawing.Color.Black;
            lbl_heading_text.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_heading_text.ForeColor = System.Drawing.Color.White;
            lbl_heading_text.Location = new System.Drawing.Point(457, 26);
            lbl_heading_text.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_heading_text.Name = "lbl_heading_text";
            lbl_heading_text.Size = new System.Drawing.Size(308, 29);
            lbl_heading_text.TabIndex = 0;
            lbl_heading_text.Text = "Manage your Audio Profiles";
            lbl_heading_text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AudioProfilesForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(1222, 664);
            Controls.Add(lbl_heading_text);
            Controls.Add(lbl_audio_profiles);
            Controls.Add(p_audio_profile_advisory);
            Controls.Add(btn_back);
            Controls.Add(lb_audio_profiles);
            Controls.Add(gb_selected_audio_settings);
            Controls.Add(btn_create_audio_profile);
            Controls.Add(btn_update_audio_profile);
            Controls.Add(btn_delete_audio_profile);
            Controls.Add(btn_apply_audio_profile);
            Controls.Add(btn_rename_audio_profile);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            MinimumSize = new System.Drawing.Size(1238, 703);
            Name = "AudioProfilesForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Audio Profiles";
            Load += AudioProfilesForm_Load;
            gb_selected_audio_settings.ResumeLayout(false);
            gb_selected_audio_settings.PerformLayout();
            p_audio_profile_advisory.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.GroupBox gb_selected_audio_settings;
        private System.Windows.Forms.TextBox txt_audio_profile_settings;
        private System.Windows.Forms.Panel p_audio_profile_advisory;
        private System.Windows.Forms.Label lbl_audio_profile_advisory;
        private System.Windows.Forms.Label lbl_audio_profiles;
        private System.Windows.Forms.ListBox lb_audio_profiles;
        private System.Windows.Forms.Button btn_delete_audio_profile;
        private System.Windows.Forms.Button btn_create_audio_profile;
        private System.Windows.Forms.Button btn_update_audio_profile;
        private System.Windows.Forms.Button btn_rename_audio_profile;
        private System.Windows.Forms.Button btn_apply_audio_profile;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.Label lbl_heading_text;
    }
}

