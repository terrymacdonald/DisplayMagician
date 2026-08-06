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
            tlpMain = new System.Windows.Forms.TableLayoutPanel();
            tlpContent = new System.Windows.Forms.TableLayoutPanel();
            tlpLeft = new System.Windows.Forms.TableLayoutPanel();
            lbl_audio_profiles = new System.Windows.Forms.Label();
            lb_audio_profiles = new System.Windows.Forms.ListBox();
            btn_create_audio_profile = new System.Windows.Forms.Button();
            btn_update_audio_profile = new System.Windows.Forms.Button();
            btn_apply_audio_profile = new System.Windows.Forms.Button();
            btn_rename_audio_profile = new System.Windows.Forms.Button();
            btn_delete_audio_profile = new System.Windows.Forms.Button();
            gb_selected_audio_settings = new System.Windows.Forms.GroupBox();
            txt_audio_profile_settings = new System.Windows.Forms.TextBox();
            flpBottom = new System.Windows.Forms.FlowLayoutPanel();
            btn_back = new System.Windows.Forms.Button();
            tlpMain.SuspendLayout();
            tlpContent.SuspendLayout();
            tlpLeft.SuspendLayout();
            gb_selected_audio_settings.SuspendLayout();
            flpBottom.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpMain.Controls.Add(tlpContent, 0, 0);
            tlpMain.Controls.Add(flpBottom, 0, 1);
            tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpMain.Location = new System.Drawing.Point(0, 0);
            tlpMain.Margin = new System.Windows.Forms.Padding(0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 2;
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpMain.Size = new System.Drawing.Size(1144, 601);
            tlpMain.TabIndex = 0;
            // 
            // tlpContent
            // 
            tlpContent.ColumnCount = 2;
            tlpContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            tlpContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            tlpContent.Controls.Add(tlpLeft, 0, 0);
            tlpContent.Controls.Add(gb_selected_audio_settings, 1, 0);
            tlpContent.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpContent.Location = new System.Drawing.Point(3, 3);
            tlpContent.Name = "tlpContent";
            tlpContent.RowCount = 1;
            tlpContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpContent.Size = new System.Drawing.Size(1138, 550);
            tlpContent.TabIndex = 0;
            // 
            // tlpLeft
            // 
            tlpLeft.ColumnCount = 1;
            tlpLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpLeft.Controls.Add(lbl_audio_profiles, 0, 0);
            tlpLeft.Controls.Add(lb_audio_profiles, 0, 1);
            tlpLeft.Controls.Add(btn_create_audio_profile, 0, 2);
            tlpLeft.Controls.Add(btn_update_audio_profile, 0, 3);
            tlpLeft.Controls.Add(btn_apply_audio_profile, 0, 4);
            tlpLeft.Controls.Add(btn_rename_audio_profile, 0, 5);
            tlpLeft.Controls.Add(btn_delete_audio_profile, 0, 6);
            tlpLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpLeft.Location = new System.Drawing.Point(3, 3);
            tlpLeft.Name = "tlpLeft";
            tlpLeft.RowCount = 7;
            tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpLeft.Size = new System.Drawing.Size(392, 544);
            tlpLeft.TabIndex = 0;
            // 
            // lbl_audio_profiles
            // 
            lbl_audio_profiles.AutoSize = true;
            lbl_audio_profiles.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_audio_profiles.Font = new System.Drawing.Font("Segoe UI", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_audio_profiles.ForeColor = System.Drawing.Color.White;
            lbl_audio_profiles.Location = new System.Drawing.Point(3, 3);
            lbl_audio_profiles.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            lbl_audio_profiles.Name = "lbl_audio_profiles";
            lbl_audio_profiles.Size = new System.Drawing.Size(386, 19);
            lbl_audio_profiles.TabIndex = 0;
            lbl_audio_profiles.Text = "Select an Audio Profile to manage:";
            lbl_audio_profiles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lbl_audio_profiles.Paint += label_Paint;
            // 
            // lb_audio_profiles
            // 
            lb_audio_profiles.BackColor = System.Drawing.Color.White;
            lb_audio_profiles.Dock = System.Windows.Forms.DockStyle.Fill;
            lb_audio_profiles.Font = new System.Drawing.Font("Segoe UI", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lb_audio_profiles.ForeColor = System.Drawing.Color.Black;
            lb_audio_profiles.FormattingEnabled = true;
            lb_audio_profiles.Location = new System.Drawing.Point(3, 31);
            lb_audio_profiles.Name = "lb_audio_profiles";
            lb_audio_profiles.Size = new System.Drawing.Size(386, 240);
            lb_audio_profiles.TabIndex = 1;
            lb_audio_profiles.SelectedIndexChanged += lb_audio_profiles_SelectedIndexChanged;
            // 
            // btn_create_audio_profile
            // 
            btn_create_audio_profile.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            btn_create_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_create_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_create_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_create_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_create_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_create_audio_profile.Location = new System.Drawing.Point(3, 278);
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
            btn_update_audio_profile.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            btn_update_audio_profile.Enabled = false;
            btn_update_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_update_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_update_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_update_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_update_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_update_audio_profile.Location = new System.Drawing.Point(3, 332);
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
            btn_apply_audio_profile.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            btn_apply_audio_profile.Enabled = false;
            btn_apply_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_apply_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_apply_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_apply_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_apply_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_apply_audio_profile.Location = new System.Drawing.Point(3, 386);
            btn_apply_audio_profile.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btn_apply_audio_profile.Name = "btn_apply_audio_profile";
            btn_apply_audio_profile.Size = new System.Drawing.Size(386, 46);
            btn_apply_audio_profile.TabIndex = 4;
            btn_apply_audio_profile.Text = "&Apply Selected Profile";
            btn_apply_audio_profile.UseVisualStyleBackColor = true;
            btn_apply_audio_profile.Click += btn_apply_audio_profile_Click;
            // 
            // btn_rename_audio_profile
            // 
            btn_rename_audio_profile.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            btn_rename_audio_profile.Enabled = false;
            btn_rename_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_rename_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_rename_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_rename_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_rename_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_rename_audio_profile.Location = new System.Drawing.Point(3, 440);
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
            btn_delete_audio_profile.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            btn_delete_audio_profile.Enabled = false;
            btn_delete_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_delete_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_delete_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_delete_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_delete_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_delete_audio_profile.Location = new System.Drawing.Point(3, 494);
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
            gb_selected_audio_settings.Controls.Add(txt_audio_profile_settings);
            gb_selected_audio_settings.Dock = System.Windows.Forms.DockStyle.Fill;
            gb_selected_audio_settings.Enabled = false;
            gb_selected_audio_settings.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_selected_audio_settings.ForeColor = System.Drawing.Color.White;
            gb_selected_audio_settings.Location = new System.Drawing.Point(401, 3);
            gb_selected_audio_settings.Name = "gb_selected_audio_settings";
            gb_selected_audio_settings.Padding = new System.Windows.Forms.Padding(6);
            gb_selected_audio_settings.Size = new System.Drawing.Size(734, 544);
            gb_selected_audio_settings.TabIndex = 1;
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
            txt_audio_profile_settings.Location = new System.Drawing.Point(6, 22);
            txt_audio_profile_settings.Margin = new System.Windows.Forms.Padding(6);
            txt_audio_profile_settings.Multiline = true;
            txt_audio_profile_settings.Name = "txt_audio_profile_settings";
            txt_audio_profile_settings.ReadOnly = true;
            txt_audio_profile_settings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txt_audio_profile_settings.Size = new System.Drawing.Size(722, 516);
            txt_audio_profile_settings.TabIndex = 0;
            // 
            // flpBottom
            // 
            flpBottom.AutoSize = true;
            flpBottom.Controls.Add(btn_back);
            flpBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            flpBottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flpBottom.Location = new System.Drawing.Point(3, 559);
            flpBottom.Name = "flpBottom";
            flpBottom.Padding = new System.Windows.Forms.Padding(3);
            flpBottom.Size = new System.Drawing.Size(1138, 39);
            flpBottom.TabIndex = 1;
            // 
            // btn_back
            // 
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(1041, 6);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 27);
            btn_back.TabIndex = 7;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // AudioProfilesForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(1144, 601);
            Controls.Add(tlpMain);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            MinimumSize = new System.Drawing.Size(1160, 640);
            Name = "AudioProfilesForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Audio Profiles";
            Load += AudioProfilesForm_Load;
            tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
            tlpContent.ResumeLayout(false);
            tlpLeft.ResumeLayout(false);
            tlpLeft.PerformLayout();
            gb_selected_audio_settings.ResumeLayout(false);
            gb_selected_audio_settings.PerformLayout();
            flpBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tlpContent;
        private System.Windows.Forms.TableLayoutPanel tlpLeft;
        private System.Windows.Forms.FlowLayoutPanel flpBottom;
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

