
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
            rb_leave_wallpaper = new System.Windows.Forms.RadioButton();
            rb_clear_wallpaper = new System.Windows.Forms.RadioButton();
            rb_apply_wallpaper = new System.Windows.Forms.RadioButton();
            btn_current = new System.Windows.Forms.Button();
            btn_clear = new System.Windows.Forms.Button();
            pb_wallpaper = new System.Windows.Forms.PictureBox();
            btn_select = new System.Windows.Forms.Button();
            lbl_style = new System.Windows.Forms.Label();
            cmb_wallpaper_display_mode = new System.Windows.Forms.ComboBox();
            gb_multiple_applies = new System.Windows.Forms.GroupBox();
            nud_apply_profile_delay = new System.Windows.Forms.NumericUpDown();
            lbl_apply_profile_delay = new System.Windows.Forms.Label();
            nud_apply_profile_count = new System.Windows.Forms.NumericUpDown();
            label1 = new System.Windows.Forms.Label();
            lbl_seconds = new System.Windows.Forms.Label();
            cb_force_restart_explorer = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            gb_general.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_wallpaper).BeginInit();
            gb_multiple_applies.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nud_apply_profile_delay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nud_apply_profile_count).BeginInit();
            SuspendLayout();
            // 
            // btn_back
            // 
            btn_back.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(555, 640);
            btn_back.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 27);
            btn_back.TabIndex = 9;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // gb_general
            // 
            gb_general.Controls.Add(rb_leave_wallpaper);
            gb_general.Controls.Add(rb_clear_wallpaper);
            gb_general.Controls.Add(rb_apply_wallpaper);
            gb_general.Controls.Add(btn_current);
            gb_general.Controls.Add(btn_clear);
            gb_general.Controls.Add(pb_wallpaper);
            gb_general.Controls.Add(btn_select);
            gb_general.Controls.Add(lbl_style);
            gb_general.Controls.Add(cmb_wallpaper_display_mode);
            gb_general.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_general.ForeColor = System.Drawing.Color.White;
            gb_general.Location = new System.Drawing.Point(30, 27);
            gb_general.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_general.Name = "gb_general";
            gb_general.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_general.Size = new System.Drawing.Size(612, 440);
            gb_general.TabIndex = 11;
            gb_general.TabStop = false;
            gb_general.Text = "Wallpaper Settings";
            // 
            // rb_leave_wallpaper
            // 
            rb_leave_wallpaper.AutoSize = true;
            rb_leave_wallpaper.Checked = true;
            rb_leave_wallpaper.Location = new System.Drawing.Point(33, 35);
            rb_leave_wallpaper.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_leave_wallpaper.Name = "rb_leave_wallpaper";
            rb_leave_wallpaper.Size = new System.Drawing.Size(145, 20);
            rb_leave_wallpaper.TabIndex = 22;
            rb_leave_wallpaper.TabStop = true;
            rb_leave_wallpaper.Text = "Do Nothing (Default)";
            rb_leave_wallpaper.UseVisualStyleBackColor = true;
            rb_leave_wallpaper.CheckedChanged += rb_leave_wallpaper_CheckedChanged;
            // 
            // rb_clear_wallpaper
            // 
            rb_clear_wallpaper.AutoSize = true;
            rb_clear_wallpaper.Location = new System.Drawing.Point(33, 65);
            rb_clear_wallpaper.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_clear_wallpaper.Name = "rb_clear_wallpaper";
            rb_clear_wallpaper.Size = new System.Drawing.Size(380, 20);
            rb_clear_wallpaper.TabIndex = 21;
            rb_clear_wallpaper.Text = "Clear the Desktop Wallpaper when using this Display Profile";
            rb_clear_wallpaper.UseVisualStyleBackColor = true;
            rb_clear_wallpaper.CheckedChanged += rb_clear_wallpaper_CheckedChanged;
            // 
            // rb_apply_wallpaper
            // 
            rb_apply_wallpaper.AutoSize = true;
            rb_apply_wallpaper.Location = new System.Drawing.Point(33, 95);
            rb_apply_wallpaper.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_apply_wallpaper.Name = "rb_apply_wallpaper";
            rb_apply_wallpaper.Size = new System.Drawing.Size(385, 20);
            rb_apply_wallpaper.TabIndex = 20;
            rb_apply_wallpaper.Text = "Apply this Desktop Wallpaper when using this Display Profile";
            rb_apply_wallpaper.UseVisualStyleBackColor = true;
            rb_apply_wallpaper.CheckedChanged += rb_apply_wallpaper_CheckedChanged;
            // 
            // btn_current
            // 
            btn_current.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_current.Enabled = false;
            btn_current.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_current.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_current.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_current.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_current.ForeColor = System.Drawing.Color.White;
            btn_current.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_current.Location = new System.Drawing.Point(486, 232);
            btn_current.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_current.Name = "btn_current";
            btn_current.Size = new System.Drawing.Size(88, 27);
            btn_current.TabIndex = 19;
            btn_current.Text = "&Use Current";
            btn_current.UseVisualStyleBackColor = true;
            btn_current.Click += btn_current_Click;
            // 
            // btn_clear
            // 
            btn_clear.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_clear.Enabled = false;
            btn_clear.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_clear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_clear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_clear.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_clear.ForeColor = System.Drawing.Color.White;
            btn_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_clear.Location = new System.Drawing.Point(486, 265);
            btn_clear.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_clear.Name = "btn_clear";
            btn_clear.Size = new System.Drawing.Size(88, 27);
            btn_clear.TabIndex = 18;
            btn_clear.Text = "&Clear";
            btn_clear.UseVisualStyleBackColor = true;
            btn_clear.Click += btn_clear_Click;
            // 
            // pb_wallpaper
            // 
            pb_wallpaper.BackColor = System.Drawing.Color.White;
            pb_wallpaper.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            pb_wallpaper.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pb_wallpaper.Enabled = false;
            pb_wallpaper.Location = new System.Drawing.Point(33, 128);
            pb_wallpaper.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pb_wallpaper.Name = "pb_wallpaper";
            pb_wallpaper.Size = new System.Drawing.Size(444, 244);
            pb_wallpaper.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pb_wallpaper.TabIndex = 17;
            pb_wallpaper.TabStop = false;
            // 
            // btn_select
            // 
            btn_select.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_select.Enabled = false;
            btn_select.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_select.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_select.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_select.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_select.ForeColor = System.Drawing.Color.White;
            btn_select.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_select.Location = new System.Drawing.Point(486, 198);
            btn_select.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_select.Name = "btn_select";
            btn_select.Size = new System.Drawing.Size(88, 27);
            btn_select.TabIndex = 16;
            btn_select.Text = "&Select";
            btn_select.UseVisualStyleBackColor = true;
            btn_select.Click += btn_select_wallpaper_Click;
            // 
            // lbl_style
            // 
            lbl_style.AutoSize = true;
            lbl_style.Enabled = false;
            lbl_style.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_style.ForeColor = System.Drawing.Color.Transparent;
            lbl_style.Location = new System.Drawing.Point(205, 385);
            lbl_style.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_style.Name = "lbl_style";
            lbl_style.Size = new System.Drawing.Size(43, 16);
            lbl_style.TabIndex = 13;
            lbl_style.Text = "Style: ";
            // 
            // cmb_wallpaper_display_mode
            // 
            cmb_wallpaper_display_mode.Enabled = false;
            cmb_wallpaper_display_mode.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmb_wallpaper_display_mode.FormattingEnabled = true;
            cmb_wallpaper_display_mode.Location = new System.Drawing.Point(264, 380);
            cmb_wallpaper_display_mode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cmb_wallpaper_display_mode.Name = "cmb_wallpaper_display_mode";
            cmb_wallpaper_display_mode.Size = new System.Drawing.Size(213, 24);
            cmb_wallpaper_display_mode.TabIndex = 12;
            cmb_wallpaper_display_mode.SelectedIndexChanged += cmb_wallpaper_display_mode_SelectedIndexChanged;
            // 
            // gb_multiple_applies
            // 
            gb_multiple_applies.Controls.Add(label2);
            gb_multiple_applies.Controls.Add(cb_force_restart_explorer);
            gb_multiple_applies.Controls.Add(nud_apply_profile_delay);
            gb_multiple_applies.Controls.Add(lbl_apply_profile_delay);
            gb_multiple_applies.Controls.Add(nud_apply_profile_count);
            gb_multiple_applies.Controls.Add(label1);
            gb_multiple_applies.Controls.Add(lbl_seconds);
            gb_multiple_applies.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_multiple_applies.ForeColor = System.Drawing.Color.White;
            gb_multiple_applies.Location = new System.Drawing.Point(31, 494);
            gb_multiple_applies.Name = "gb_multiple_applies";
            gb_multiple_applies.Size = new System.Drawing.Size(612, 124);
            gb_multiple_applies.TabIndex = 12;
            gb_multiple_applies.TabStop = false;
            gb_multiple_applies.Text = "Profile Settings";
            // 
            // nud_apply_profile_delay
            // 
            nud_apply_profile_delay.Location = new System.Drawing.Point(535, 32);
            nud_apply_profile_delay.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nud_apply_profile_delay.Name = "nud_apply_profile_delay";
            nud_apply_profile_delay.Size = new System.Drawing.Size(42, 22);
            nud_apply_profile_delay.TabIndex = 3;
            nud_apply_profile_delay.ValueChanged += nud_apply_profile_delay_ValueChanged;
            // 
            // lbl_apply_profile_delay
            // 
            lbl_apply_profile_delay.AutoSize = true;
            lbl_apply_profile_delay.Location = new System.Drawing.Point(375, 34);
            lbl_apply_profile_delay.Name = "lbl_apply_profile_delay";
            lbl_apply_profile_delay.Size = new System.Drawing.Size(154, 16);
            lbl_apply_profile_delay.TabIndex = 2;
            lbl_apply_profile_delay.Text = "Delay between attempts:";
            // 
            // nud_apply_profile_count
            // 
            nud_apply_profile_count.Location = new System.Drawing.Point(287, 32);
            nud_apply_profile_count.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            nud_apply_profile_count.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nud_apply_profile_count.Name = "nud_apply_profile_count";
            nud_apply_profile_count.Size = new System.Drawing.Size(42, 22);
            nud_apply_profile_count.TabIndex = 1;
            nud_apply_profile_count.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nud_apply_profile_count.ValueChanged += nud_apply_profile_count_ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(33, 34);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(248, 16);
            label1.TabIndex = 0;
            label1.Text = "Number of times to apply Display Profile:";
            // 
            // lbl_seconds
            // 
            lbl_seconds.AutoSize = true;
            lbl_seconds.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_seconds.Location = new System.Drawing.Point(526, 53);
            lbl_seconds.Name = "lbl_seconds";
            lbl_seconds.Size = new System.Drawing.Size(61, 15);
            lbl_seconds.TabIndex = 4;
            lbl_seconds.Text = "(seconds)";
            // 
            // cb_force_restart_explorer
            // 
            cb_force_restart_explorer.AutoSize = true;
            cb_force_restart_explorer.Location = new System.Drawing.Point(36, 72);
            cb_force_restart_explorer.Name = "cb_force_restart_explorer";
            cb_force_restart_explorer.Size = new System.Drawing.Size(442, 20);
            cb_force_restart_explorer.TabIndex = 5;
            cb_force_restart_explorer.Text = "Force Windows Explorer to restart to redraw missing windows taskbars";
            cb_force_restart_explorer.UseVisualStyleBackColor = true;
            cb_force_restart_explorer.CheckedChanged += cb_force_restart_explorer_CheckedChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label2.Location = new System.Drawing.Point(52, 94);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(462, 13);
            label2.TabIndex = 6;
            label2.Text = "(Note: This will close any Windows Explorer windows you have open when this profile is applied.)";
            // 
            // ProfileSettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(677, 693);
            Controls.Add(gb_multiple_applies);
            Controls.Add(gb_general);
            Controls.Add(btn_back);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
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
            ((System.ComponentModel.ISupportInitialize)pb_wallpaper).EndInit();
            gb_multiple_applies.ResumeLayout(false);
            gb_multiple_applies.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nud_apply_profile_delay).EndInit();
            ((System.ComponentModel.ISupportInitialize)nud_apply_profile_count).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.GroupBox gb_general;
        private System.Windows.Forms.Label lbl_style;
        private System.Windows.Forms.ComboBox cmb_wallpaper_display_mode;
        private System.Windows.Forms.Button btn_select;
        private System.Windows.Forms.Button btn_clear;
        private System.Windows.Forms.PictureBox pb_wallpaper;
        private System.Windows.Forms.Button btn_current;
        private System.Windows.Forms.RadioButton rb_leave_wallpaper;
        private System.Windows.Forms.RadioButton rb_clear_wallpaper;
        private System.Windows.Forms.RadioButton rb_apply_wallpaper;
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