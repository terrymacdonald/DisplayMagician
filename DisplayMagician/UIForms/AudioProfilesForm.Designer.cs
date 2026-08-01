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
            gb_audio_profile = new System.Windows.Forms.GroupBox();
            gb_selected_audio_settings = new System.Windows.Forms.GroupBox();
            cb_system_audio_enabled = new System.Windows.Forms.CheckBox();
            cb_mono_audio = new System.Windows.Forms.CheckBox();
            cb_microphone_mute = new System.Windows.Forms.CheckBox();
            cb_speaker_mute = new System.Windows.Forms.CheckBox();
            lbl_microphone_volume = new System.Windows.Forms.Label();
            nud_microphone_volume = new System.Windows.Forms.NumericUpDown();
            lbl_speaker_volume = new System.Windows.Forms.Label();
            nud_speaker_volume = new System.Windows.Forms.NumericUpDown();
            txt_audio_profile_settings = new System.Windows.Forms.TextBox();
            lbl_audio_profiles = new System.Windows.Forms.Label();
            lb_audio_profiles = new System.Windows.Forms.ListBox();
            btn_delete_audio_profile = new System.Windows.Forms.Button();
            btn_create_audio_profile = new System.Windows.Forms.Button();
            btn_update_audio_profile = new System.Windows.Forms.Button();
            btn_rename_audio_profile = new System.Windows.Forms.Button();
            btn_apply_audio_profile = new System.Windows.Forms.Button();
            btn_close = new System.Windows.Forms.Button();
            gb_audio_profile.SuspendLayout();
            gb_selected_audio_settings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nud_microphone_volume).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nud_speaker_volume).BeginInit();
            SuspendLayout();
            // 
            // gb_audio_profile
            // 
            gb_audio_profile.Controls.Add(btn_close);
            gb_audio_profile.Controls.Add(btn_apply_audio_profile);
            gb_audio_profile.Controls.Add(gb_selected_audio_settings);
            gb_audio_profile.Controls.Add(lbl_audio_profiles);
            gb_audio_profile.Controls.Add(lb_audio_profiles);
            gb_audio_profile.Controls.Add(btn_delete_audio_profile);
            gb_audio_profile.Controls.Add(btn_rename_audio_profile);
            gb_audio_profile.Controls.Add(btn_create_audio_profile);
            gb_audio_profile.Controls.Add(btn_update_audio_profile);
            gb_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_audio_profile.ForeColor = System.Drawing.Color.White;
            gb_audio_profile.Location = new System.Drawing.Point(12, 12);
            gb_audio_profile.Name = "gb_audio_profile";
            gb_audio_profile.Size = new System.Drawing.Size(1240, 720);
            gb_audio_profile.TabIndex = 0;
            gb_audio_profile.TabStop = false;
            gb_audio_profile.Text = "Audio Profiles";
            gb_audio_profile.Paint += groupbox_Paint;
            // 
            // gb_selected_audio_settings
            // 
            gb_selected_audio_settings.Controls.Add(cb_system_audio_enabled);
            gb_selected_audio_settings.Controls.Add(cb_mono_audio);
            gb_selected_audio_settings.Controls.Add(cb_microphone_mute);
            gb_selected_audio_settings.Controls.Add(cb_speaker_mute);
            gb_selected_audio_settings.Controls.Add(lbl_microphone_volume);
            gb_selected_audio_settings.Controls.Add(nud_microphone_volume);
            gb_selected_audio_settings.Controls.Add(lbl_speaker_volume);
            gb_selected_audio_settings.Controls.Add(nud_speaker_volume);
            gb_selected_audio_settings.Controls.Add(txt_audio_profile_settings);
            gb_selected_audio_settings.Enabled = false;
            gb_selected_audio_settings.ForeColor = System.Drawing.Color.White;
            gb_selected_audio_settings.Location = new System.Drawing.Point(463, 55);
            gb_selected_audio_settings.Name = "gb_selected_audio_settings";
            gb_selected_audio_settings.Size = new System.Drawing.Size(760, 600);
            gb_selected_audio_settings.TabIndex = 5;
            gb_selected_audio_settings.TabStop = false;
            gb_selected_audio_settings.Text = "Selected Audio Profile Settings";
            gb_selected_audio_settings.Paint += groupbox_Paint;
            // 
            // cb_system_audio_enabled
            // 
            cb_system_audio_enabled.AutoSize = true;
            cb_system_audio_enabled.ForeColor = System.Drawing.Color.White;
            cb_system_audio_enabled.Location = new System.Drawing.Point(24, 551);
            cb_system_audio_enabled.Name = "cb_system_audio_enabled";
            cb_system_audio_enabled.Size = new System.Drawing.Size(180, 24);
            cb_system_audio_enabled.TabIndex = 8;
            cb_system_audio_enabled.Text = "System Audio Enabled";
            cb_system_audio_enabled.UseVisualStyleBackColor = true;
            cb_system_audio_enabled.CheckedChanged += AudioValueChanged;
            cb_system_audio_enabled.Paint += checkbox_Paint;
            // 
            // cb_mono_audio
            // 
            cb_mono_audio.AutoSize = true;
            cb_mono_audio.ForeColor = System.Drawing.Color.White;
            cb_mono_audio.Location = new System.Drawing.Point(24, 521);
            cb_mono_audio.Name = "cb_mono_audio";
            cb_mono_audio.Size = new System.Drawing.Size(149, 24);
            cb_mono_audio.TabIndex = 7;
            cb_mono_audio.Text = "Mono Audio";
            cb_mono_audio.UseVisualStyleBackColor = true;
            cb_mono_audio.CheckedChanged += AudioValueChanged;
            cb_mono_audio.Paint += checkbox_Paint;
            // 
            // cb_microphone_mute
            // 
            cb_microphone_mute.AutoSize = true;
            cb_microphone_mute.ForeColor = System.Drawing.Color.White;
            cb_microphone_mute.Location = new System.Drawing.Point(560, 521);
            cb_microphone_mute.Name = "cb_microphone_mute";
            cb_microphone_mute.Size = new System.Drawing.Size(144, 24);
            cb_microphone_mute.TabIndex = 6;
            cb_microphone_mute.Text = "Microphone Mute";
            cb_microphone_mute.UseVisualStyleBackColor = true;
            cb_microphone_mute.CheckedChanged += AudioValueChanged;
            cb_microphone_mute.Paint += checkbox_Paint;
            // 
            // cb_speaker_mute
            // 
            cb_speaker_mute.AutoSize = true;
            cb_speaker_mute.ForeColor = System.Drawing.Color.White;
            cb_speaker_mute.Location = new System.Drawing.Point(560, 491);
            cb_speaker_mute.Name = "cb_speaker_mute";
            cb_speaker_mute.Size = new System.Drawing.Size(122, 24);
            cb_speaker_mute.TabIndex = 5;
            cb_speaker_mute.Text = "Speaker Mute";
            cb_speaker_mute.UseVisualStyleBackColor = true;
            cb_speaker_mute.CheckedChanged += AudioValueChanged;
            cb_speaker_mute.Paint += checkbox_Paint;
            // 
            // lbl_microphone_volume
            // 
            lbl_microphone_volume.AutoSize = true;
            lbl_microphone_volume.ForeColor = System.Drawing.Color.White;
            lbl_microphone_volume.Location = new System.Drawing.Point(360, 558);
            lbl_microphone_volume.Name = "lbl_microphone_volume";
            lbl_microphone_volume.Size = new System.Drawing.Size(150, 20);
            lbl_microphone_volume.TabIndex = 4;
            lbl_microphone_volume.Text = "Microphone Volume:";
            lbl_microphone_volume.Paint += label_Paint;
            // 
            // nud_microphone_volume
            // 
            nud_microphone_volume.Location = new System.Drawing.Point(516, 556);
            nud_microphone_volume.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            nud_microphone_volume.Name = "nud_microphone_volume";
            nud_microphone_volume.Size = new System.Drawing.Size(50, 26);
            nud_microphone_volume.TabIndex = 3;
            nud_microphone_volume.Value = new decimal(new int[] { 50, 0, 0, 0 });
            nud_microphone_volume.ValueChanged += AudioValueChanged;
            // 
            // lbl_speaker_volume
            // 
            lbl_speaker_volume.AutoSize = true;
            lbl_speaker_volume.ForeColor = System.Drawing.Color.White;
            lbl_speaker_volume.Location = new System.Drawing.Point(24, 558);
            lbl_speaker_volume.Name = "lbl_speaker_volume";
            lbl_speaker_volume.Size = new System.Drawing.Size(124, 20);
            lbl_speaker_volume.TabIndex = 2;
            lbl_speaker_volume.Text = "Speaker Volume:";
            lbl_speaker_volume.Paint += label_Paint;
            // 
            // nud_speaker_volume
            // 
            nud_speaker_volume.Location = new System.Drawing.Point(154, 556);
            nud_speaker_volume.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            nud_speaker_volume.Name = "nud_speaker_volume";
            nud_speaker_volume.Size = new System.Drawing.Size(50, 26);
            nud_speaker_volume.TabIndex = 1;
            nud_speaker_volume.Value = new decimal(new int[] { 50, 0, 0, 0 });
            nud_speaker_volume.ValueChanged += AudioValueChanged;
            // 
            // txt_audio_profile_settings
            // 
            txt_audio_profile_settings.AcceptsReturn = true;
            txt_audio_profile_settings.AcceptsTab = true;
            txt_audio_profile_settings.BackColor = System.Drawing.Color.White;
            txt_audio_profile_settings.ForeColor = System.Drawing.Color.Black;
            txt_audio_profile_settings.Location = new System.Drawing.Point(24, 32);
            txt_audio_profile_settings.Multiline = true;
            txt_audio_profile_settings.Name = "txt_audio_profile_settings";
            txt_audio_profile_settings.ReadOnly = true;
            txt_audio_profile_settings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txt_audio_profile_settings.Size = new System.Drawing.Size(712, 440);
            txt_audio_profile_settings.TabIndex = 0;
            // 
            // lbl_audio_profiles
            // 
            lbl_audio_profiles.AutoSize = true;
            lbl_audio_profiles.ForeColor = System.Drawing.Color.White;
            lbl_audio_profiles.Location = new System.Drawing.Point(57, 55);
            lbl_audio_profiles.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_audio_profiles.Name = "lbl_audio_profiles";
            lbl_audio_profiles.Size = new System.Drawing.Size(250, 20);
            lbl_audio_profiles.TabIndex = 0;
            lbl_audio_profiles.Text = "Select an Audio Profile to manage:";
            lbl_audio_profiles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl_audio_profiles.Paint += label_Paint;
            // 
            // lb_audio_profiles
            // 
            lb_audio_profiles.BackColor = System.Drawing.Color.White;
            lb_audio_profiles.ForeColor = System.Drawing.Color.Black;
            lb_audio_profiles.FormattingEnabled = true;
            lb_audio_profiles.Location = new System.Drawing.Point(26, 76);
            lb_audio_profiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_audio_profiles.Name = "lb_audio_profiles";
            lb_audio_profiles.Size = new System.Drawing.Size(392, 394);
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
            btn_delete_audio_profile.Location = new System.Drawing.Point(74, 581);
            btn_delete_audio_profile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_delete_audio_profile.Name = "btn_delete_audio_profile";
            btn_delete_audio_profile.Size = new System.Drawing.Size(296, 31);
            btn_delete_audio_profile.TabIndex = 4;
            btn_delete_audio_profile.Text = "Delete Selected Profile";
            btn_delete_audio_profile.UseVisualStyleBackColor = true;
            btn_delete_audio_profile.Click += btn_delete_audio_profile_Click;
            // 
            // btn_create_audio_profile
            // 
            btn_create_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_create_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_create_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_create_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_create_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_create_audio_profile.Location = new System.Drawing.Point(74, 491);
            btn_create_audio_profile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_create_audio_profile.Name = "btn_create_audio_profile";
            btn_create_audio_profile.Size = new System.Drawing.Size(296, 31);
            btn_create_audio_profile.TabIndex = 2;
            btn_create_audio_profile.Text = "Create New Profile from Current Audio";
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
            btn_update_audio_profile.Location = new System.Drawing.Point(74, 536);
            btn_update_audio_profile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_update_audio_profile.Name = "btn_update_audio_profile";
            btn_update_audio_profile.Size = new System.Drawing.Size(296, 31);
            btn_update_audio_profile.TabIndex = 3;
            btn_update_audio_profile.Text = "Update Profile from Current Audio";
            btn_update_audio_profile.UseVisualStyleBackColor = true;
            btn_update_audio_profile.Click += btn_update_audio_profile_Click;
            // 
            // btn_rename_audio_profile
            // 
            btn_rename_audio_profile.Enabled = false;
            btn_rename_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_rename_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_rename_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_rename_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_rename_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_rename_audio_profile.Location = new System.Drawing.Point(74, 626);
            btn_rename_audio_profile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_rename_audio_profile.Name = "btn_rename_audio_profile";
            btn_rename_audio_profile.Size = new System.Drawing.Size(296, 31);
            btn_rename_audio_profile.TabIndex = 5;
            btn_rename_audio_profile.Text = "Rename Selected Profile";
            btn_rename_audio_profile.UseVisualStyleBackColor = true;
            btn_rename_audio_profile.Click += btn_rename_audio_profile_Click;
            // 
            // btn_apply_audio_profile
            // 
            btn_apply_audio_profile.Enabled = false;
            btn_apply_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_apply_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_apply_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_apply_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_apply_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_apply_audio_profile.Location = new System.Drawing.Point(915, 661);
            btn_apply_audio_profile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_apply_audio_profile.Name = "btn_apply_audio_profile";
            btn_apply_audio_profile.Size = new System.Drawing.Size(140, 31);
            btn_apply_audio_profile.TabIndex = 6;
            btn_apply_audio_profile.Text = "&Apply Profile";
            btn_apply_audio_profile.UseVisualStyleBackColor = true;
            btn_apply_audio_profile.Click += btn_apply_audio_profile_Click;
            // 
            // btn_close
            // 
            btn_close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_close.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_close.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_close.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_close.ForeColor = System.Drawing.Color.White;
            btn_close.Location = new System.Drawing.Point(1073, 661);
            btn_close.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_close.Name = "btn_close";
            btn_close.Size = new System.Drawing.Size(88, 31);
            btn_close.TabIndex = 7;
            btn_close.Text = "&Close";
            btn_close.UseVisualStyleBackColor = true;
            btn_close.Click += btn_close_Click;
            // 
            // AudioProfilesForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(1264, 744);
            Controls.Add(gb_audio_profile);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            Name = "AudioProfilesForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Audio Profiles";
            Load += AudioProfilesForm_Load;
            gb_audio_profile.ResumeLayout(false);
            gb_audio_profile.PerformLayout();
            gb_selected_audio_settings.ResumeLayout(false);
            gb_selected_audio_settings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nud_microphone_volume).EndInit();
            ((System.ComponentModel.ISupportInitialize)nud_speaker_volume).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox gb_audio_profile;
        private System.Windows.Forms.GroupBox gb_selected_audio_settings;
        private System.Windows.Forms.TextBox txt_audio_profile_settings;
        private System.Windows.Forms.Label lbl_audio_profiles;
        private System.Windows.Forms.ListBox lb_audio_profiles;
        private System.Windows.Forms.Button btn_delete_audio_profile;
        private System.Windows.Forms.Button btn_create_audio_profile;
        private System.Windows.Forms.Button btn_update_audio_profile;
        private System.Windows.Forms.Button btn_rename_audio_profile;
        private System.Windows.Forms.Button btn_apply_audio_profile;
        private System.Windows.Forms.Button btn_close;
        private System.Windows.Forms.CheckBox cb_system_audio_enabled;
        private System.Windows.Forms.CheckBox cb_mono_audio;
        private System.Windows.Forms.CheckBox cb_microphone_mute;
        private System.Windows.Forms.CheckBox cb_speaker_mute;
        private System.Windows.Forms.Label lbl_microphone_volume;
        private System.Windows.Forms.NumericUpDown nud_microphone_volume;
        private System.Windows.Forms.Label lbl_speaker_volume;
        private System.Windows.Forms.NumericUpDown nud_speaker_volume;
    }
}
