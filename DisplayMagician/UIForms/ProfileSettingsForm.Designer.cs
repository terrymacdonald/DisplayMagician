
namespace DisplayMagician.UIForms
{
    partial class ProfileSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfileSettingsForm));
            btn_back = new System.Windows.Forms.Button();
            gb_general = new System.Windows.Forms.GroupBox();
            lbl_wallpaper_mode = new System.Windows.Forms.Label();
            cmb_wallpaper_mode = new System.Windows.Forms.ComboBox();
            lbl_wallpaper_bg_type_label = new System.Windows.Forms.Label();
            lbl_wallpaper_bg_type = new System.Windows.Forms.Label();
            gb_multiple_applies = new System.Windows.Forms.GroupBox();
            label2 = new System.Windows.Forms.Label();
            cb_force_restart_explorer = new System.Windows.Forms.CheckBox();
            nud_apply_profile_delay = new System.Windows.Forms.NumericUpDown();
            lbl_apply_profile_delay = new System.Windows.Forms.Label();
            nud_apply_profile_count = new System.Windows.Forms.NumericUpDown();
            label1 = new System.Windows.Forms.Label();
            lbl_seconds = new System.Windows.Forms.Label();
            gb_general.SuspendLayout();
            gb_multiple_applies.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nud_apply_profile_delay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nud_apply_profile_count).BeginInit();
            SuspendLayout();
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
            btn_back.Location = new System.Drawing.Point(545, 402);
            btn_back.Margin = new System.Windows.Forms.Padding(55, 19, 55, 19);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 27);
            btn_back.TabIndex = 9;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // gb_general
            // 
            gb_general.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gb_general.Controls.Add(lbl_wallpaper_mode);
            gb_general.Controls.Add(cmb_wallpaper_mode);
            gb_general.Controls.Add(lbl_wallpaper_bg_type_label);
            gb_general.Controls.Add(lbl_wallpaper_bg_type);
            gb_general.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_general.ForeColor = System.Drawing.Color.White;
            gb_general.Location = new System.Drawing.Point(16, 13);
            gb_general.Margin = new System.Windows.Forms.Padding(55, 19, 55, 19);
            gb_general.Name = "gb_general";
            gb_general.Padding = new System.Windows.Forms.Padding(55, 19, 55, 19);
            gb_general.Size = new System.Drawing.Size(617, 144);
            gb_general.TabIndex = 11;
            gb_general.TabStop = false;
            gb_general.Text = "Wallpaper Settings";
            // 
            // lbl_wallpaper_mode
            // 
            lbl_wallpaper_mode.AutoSize = true;
            lbl_wallpaper_mode.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_wallpaper_mode.ForeColor = System.Drawing.Color.White;
            lbl_wallpaper_mode.Location = new System.Drawing.Point(46, 52);
            lbl_wallpaper_mode.Margin = new System.Windows.Forms.Padding(55, 0, 55, 0);
            lbl_wallpaper_mode.Name = "lbl_wallpaper_mode";
            lbl_wallpaper_mode.Size = new System.Drawing.Size(179, 16);
            lbl_wallpaper_mode.TabIndex = 0;
            lbl_wallpaper_mode.Text = "When switching to this profile:";
            // 
            // cmb_wallpaper_mode
            // 
            cmb_wallpaper_mode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmb_wallpaper_mode.DropDownWidth = 200;
            cmb_wallpaper_mode.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmb_wallpaper_mode.FormattingEnabled = true;
            cmb_wallpaper_mode.Items.AddRange(new object[] { "Apply saved wallpaper settings", "Do not apply saved wallpaper settings" });
            cmb_wallpaper_mode.Location = new System.Drawing.Point(244, 49);
            cmb_wallpaper_mode.Margin = new System.Windows.Forms.Padding(55, 19, 55, 19);
            cmb_wallpaper_mode.Name = "cmb_wallpaper_mode";
            cmb_wallpaper_mode.Size = new System.Drawing.Size(200, 24);
            cmb_wallpaper_mode.TabIndex = 1;
            cmb_wallpaper_mode.SelectedIndexChanged += cmb_wallpaper_mode_SelectedIndexChanged;
            // 
            // lbl_wallpaper_bg_type_label
            // 
            lbl_wallpaper_bg_type_label.AutoSize = true;
            lbl_wallpaper_bg_type_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_wallpaper_bg_type_label.ForeColor = System.Drawing.Color.White;
            lbl_wallpaper_bg_type_label.Location = new System.Drawing.Point(82, 96);
            lbl_wallpaper_bg_type_label.Margin = new System.Windows.Forms.Padding(55, 0, 55, 0);
            lbl_wallpaper_bg_type_label.Name = "lbl_wallpaper_bg_type_label";
            lbl_wallpaper_bg_type_label.Size = new System.Drawing.Size(145, 16);
            lbl_wallpaper_bg_type_label.TabIndex = 2;
            lbl_wallpaper_bg_type_label.Text = "Saved Wallpaper type:";
            // 
            // lbl_wallpaper_bg_type
            // 
            lbl_wallpaper_bg_type.AutoSize = true;
            lbl_wallpaper_bg_type.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_wallpaper_bg_type.ForeColor = System.Drawing.Color.LightGray;
            lbl_wallpaper_bg_type.Location = new System.Drawing.Point(246, 96);
            lbl_wallpaper_bg_type.Margin = new System.Windows.Forms.Padding(55, 0, 55, 0);
            lbl_wallpaper_bg_type.Name = "lbl_wallpaper_bg_type";
            lbl_wallpaper_bg_type.Size = new System.Drawing.Size(16, 16);
            lbl_wallpaper_bg_type.TabIndex = 3;
            lbl_wallpaper_bg_type.Text = "—";
            // 
            // gb_multiple_applies
            // 
            gb_multiple_applies.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gb_multiple_applies.Controls.Add(label2);
            gb_multiple_applies.Controls.Add(cb_force_restart_explorer);
            gb_multiple_applies.Controls.Add(nud_apply_profile_delay);
            gb_multiple_applies.Controls.Add(lbl_apply_profile_delay);
            gb_multiple_applies.Controls.Add(nud_apply_profile_count);
            gb_multiple_applies.Controls.Add(label1);
            gb_multiple_applies.Controls.Add(lbl_seconds);
            gb_multiple_applies.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_multiple_applies.ForeColor = System.Drawing.Color.White;
            gb_multiple_applies.Location = new System.Drawing.Point(16, 169);
            gb_multiple_applies.Margin = new System.Windows.Forms.Padding(41, 19, 41, 19);
            gb_multiple_applies.Name = "gb_multiple_applies";
            gb_multiple_applies.Padding = new System.Windows.Forms.Padding(41, 19, 41, 19);
            gb_multiple_applies.Size = new System.Drawing.Size(617, 221);
            gb_multiple_applies.TabIndex = 12;
            gb_multiple_applies.TabStop = false;
            gb_multiple_applies.Text = "Profile Settings";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label2.Location = new System.Drawing.Point(64, 184);
            label2.Margin = new System.Windows.Forms.Padding(41, 0, 41, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(462, 13);
            label2.TabIndex = 6;
            label2.Text = "(Note: This will close any Windows Explorer windows you have open when this profile is applied.)";
            // 
            // cb_force_restart_explorer
            // 
            cb_force_restart_explorer.AutoSize = true;
            cb_force_restart_explorer.Location = new System.Drawing.Point(46, 161);
            cb_force_restart_explorer.Margin = new System.Windows.Forms.Padding(41, 19, 41, 19);
            cb_force_restart_explorer.Name = "cb_force_restart_explorer";
            cb_force_restart_explorer.Size = new System.Drawing.Size(442, 20);
            cb_force_restart_explorer.TabIndex = 5;
            cb_force_restart_explorer.Text = "Force Windows Explorer to restart to redraw missing windows taskbars";
            cb_force_restart_explorer.UseVisualStyleBackColor = true;
            cb_force_restart_explorer.CheckedChanged += cb_force_restart_explorer_CheckedChanged;
            // 
            // nud_apply_profile_delay
            // 
            nud_apply_profile_delay.Location = new System.Drawing.Point(204, 103);
            nud_apply_profile_delay.Margin = new System.Windows.Forms.Padding(562, 122, 562, 122);
            nud_apply_profile_delay.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nud_apply_profile_delay.Name = "nud_apply_profile_delay";
            nud_apply_profile_delay.Size = new System.Drawing.Size(58, 22);
            nud_apply_profile_delay.TabIndex = 3;
            nud_apply_profile_delay.ValueChanged += nud_apply_profile_delay_ValueChanged;
            // 
            // lbl_apply_profile_delay
            // 
            lbl_apply_profile_delay.AutoSize = true;
            lbl_apply_profile_delay.Location = new System.Drawing.Point(46, 105);
            lbl_apply_profile_delay.Margin = new System.Windows.Forms.Padding(41, 0, 41, 0);
            lbl_apply_profile_delay.Name = "lbl_apply_profile_delay";
            lbl_apply_profile_delay.Size = new System.Drawing.Size(154, 16);
            lbl_apply_profile_delay.TabIndex = 2;
            lbl_apply_profile_delay.Text = "Delay between attempts:";
            // 
            // nud_apply_profile_count
            // 
            nud_apply_profile_count.Location = new System.Drawing.Point(300, 52);
            nud_apply_profile_count.Margin = new System.Windows.Forms.Padding(562, 122, 562, 122);
            nud_apply_profile_count.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            nud_apply_profile_count.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nud_apply_profile_count.Name = "nud_apply_profile_count";
            nud_apply_profile_count.Size = new System.Drawing.Size(50, 22);
            nud_apply_profile_count.TabIndex = 1;
            nud_apply_profile_count.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nud_apply_profile_count.ValueChanged += nud_apply_profile_count_ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(46, 54);
            label1.Margin = new System.Windows.Forms.Padding(41, 0, 41, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(248, 16);
            label1.TabIndex = 0;
            label1.Text = "Number of times to apply Display Profile:";
            // 
            // lbl_seconds
            // 
            lbl_seconds.AutoSize = true;
            lbl_seconds.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_seconds.Location = new System.Drawing.Point(262, 106);
            lbl_seconds.Margin = new System.Windows.Forms.Padding(41, 0, 41, 0);
            lbl_seconds.Name = "lbl_seconds";
            lbl_seconds.Size = new System.Drawing.Size(61, 15);
            lbl_seconds.TabIndex = 4;
            lbl_seconds.Text = "(seconds)";
            // 
            // ProfileSettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(650, 443);
            Controls.Add(gb_multiple_applies);
            Controls.Add(gb_general);
            Controls.Add(btn_back);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(55, 19, 55, 19);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ProfileSettingsForm";
            ShowIcon = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Profile Settings";
            TopMost = true;
            FormClosing += ProfileSettingsForm_FormClosing;
            Load += ProfileSettingsForm_Load;
            gb_general.ResumeLayout(false);
            gb_general.PerformLayout();
            gb_multiple_applies.ResumeLayout(false);
            gb_multiple_applies.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nud_apply_profile_delay).EndInit();
            ((System.ComponentModel.ISupportInitialize)nud_apply_profile_count).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.GroupBox gb_general;
        private System.Windows.Forms.Label lbl_wallpaper_mode;
        private System.Windows.Forms.ComboBox cmb_wallpaper_mode;
        private System.Windows.Forms.Label lbl_wallpaper_bg_type_label;
        private System.Windows.Forms.Label lbl_wallpaper_bg_type;
        private System.Windows.Forms.GroupBox gb_multiple_applies;
        private System.Windows.Forms.Label lbl_apply_profile_delay;
        private System.Windows.Forms.NumericUpDown nud_apply_profile_count;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nud_apply_profile_delay;
        private System.Windows.Forms.Label lbl_seconds;
        private System.Windows.Forms.CheckBox cb_force_restart_explorer;
        private System.Windows.Forms.Label label2;
    }
}
