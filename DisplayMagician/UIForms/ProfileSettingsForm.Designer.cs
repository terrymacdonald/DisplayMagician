
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
            this.btn_back = new System.Windows.Forms.Button();
            this.gb_general = new System.Windows.Forms.GroupBox();
            this.rb_leave_wallpaper = new System.Windows.Forms.RadioButton();
            this.rb_clear_wallpaper = new System.Windows.Forms.RadioButton();
            this.rb_apply_wallpaper = new System.Windows.Forms.RadioButton();
            this.btn_current = new System.Windows.Forms.Button();
            this.btn_clear = new System.Windows.Forms.Button();
            this.pb_wallpaper = new System.Windows.Forms.PictureBox();
            this.btn_select = new System.Windows.Forms.Button();
            this.lbl_style = new System.Windows.Forms.Label();
            this.cmb_wallpaper_display_mode = new System.Windows.Forms.ComboBox();
            this.gb_general.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_wallpaper)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_back
            // 
            this.btn_back.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_back.ForeColor = System.Drawing.Color.White;
            this.btn_back.Location = new System.Drawing.Point(476, 426);
            this.btn_back.Name = "btn_back";
            this.btn_back.Size = new System.Drawing.Size(75, 23);
            this.btn_back.TabIndex = 9;
            this.btn_back.Text = "&Back";
            this.btn_back.UseVisualStyleBackColor = true;
            this.btn_back.Click += new System.EventHandler(this.btn_back_Click);
            // 
            // gb_general
            // 
            this.gb_general.Controls.Add(this.rb_leave_wallpaper);
            this.gb_general.Controls.Add(this.rb_clear_wallpaper);
            this.gb_general.Controls.Add(this.rb_apply_wallpaper);
            this.gb_general.Controls.Add(this.btn_current);
            this.gb_general.Controls.Add(this.btn_clear);
            this.gb_general.Controls.Add(this.pb_wallpaper);
            this.gb_general.Controls.Add(this.btn_select);
            this.gb_general.Controls.Add(this.lbl_style);
            this.gb_general.Controls.Add(this.cmb_wallpaper_display_mode);
            this.gb_general.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gb_general.ForeColor = System.Drawing.Color.White;
            this.gb_general.Location = new System.Drawing.Point(27, 23);
            this.gb_general.Name = "gb_general";
            this.gb_general.Size = new System.Drawing.Size(525, 381);
            this.gb_general.TabIndex = 11;
            this.gb_general.TabStop = false;
            this.gb_general.Text = "Wallpaper Settings";
            // 
            // rb_leave_wallpaper
            // 
            this.rb_leave_wallpaper.AutoSize = true;
            this.rb_leave_wallpaper.Checked = true;
            this.rb_leave_wallpaper.Location = new System.Drawing.Point(28, 30);
            this.rb_leave_wallpaper.Name = "rb_leave_wallpaper";
            this.rb_leave_wallpaper.Size = new System.Drawing.Size(146, 20);
            this.rb_leave_wallpaper.TabIndex = 22;
            this.rb_leave_wallpaper.TabStop = true;
            this.rb_leave_wallpaper.Text = "Do Nothing (Default)";
            this.rb_leave_wallpaper.UseVisualStyleBackColor = true;
            this.rb_leave_wallpaper.CheckedChanged += new System.EventHandler(this.rb_leave_wallpaper_CheckedChanged);
            // 
            // rb_clear_wallpaper
            // 
            this.rb_clear_wallpaper.AutoSize = true;
            this.rb_clear_wallpaper.Location = new System.Drawing.Point(28, 56);
            this.rb_clear_wallpaper.Name = "rb_clear_wallpaper";
            this.rb_clear_wallpaper.Size = new System.Drawing.Size(381, 20);
            this.rb_clear_wallpaper.TabIndex = 21;
            this.rb_clear_wallpaper.Text = "Clear the Desktop Wallpaper when using this Display Profile";
            this.rb_clear_wallpaper.UseVisualStyleBackColor = true;
            this.rb_clear_wallpaper.CheckedChanged += new System.EventHandler(this.rb_clear_wallpaper_CheckedChanged);
            // 
            // rb_apply_wallpaper
            // 
            this.rb_apply_wallpaper.AutoSize = true;
            this.rb_apply_wallpaper.Location = new System.Drawing.Point(28, 82);
            this.rb_apply_wallpaper.Name = "rb_apply_wallpaper";
            this.rb_apply_wallpaper.Size = new System.Drawing.Size(386, 20);
            this.rb_apply_wallpaper.TabIndex = 20;
            this.rb_apply_wallpaper.Text = "Apply this Desktop Wallpaper when using this Display Profile";
            this.rb_apply_wallpaper.UseVisualStyleBackColor = true;
            this.rb_apply_wallpaper.CheckedChanged += new System.EventHandler(this.rb_apply_wallpaper_CheckedChanged);
            // 
            // btn_current
            // 
            this.btn_current.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_current.Enabled = false;
            this.btn_current.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_current.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_current.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_current.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_current.ForeColor = System.Drawing.Color.White;
            this.btn_current.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_current.Location = new System.Drawing.Point(417, 201);
            this.btn_current.Name = "btn_current";
            this.btn_current.Size = new System.Drawing.Size(75, 23);
            this.btn_current.TabIndex = 19;
            this.btn_current.Text = "&Use Current";
            this.btn_current.UseVisualStyleBackColor = true;
            this.btn_current.Click += new System.EventHandler(this.btn_current_Click);
            // 
            // btn_clear
            // 
            this.btn_clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_clear.Enabled = false;
            this.btn_clear.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_clear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_clear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_clear.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_clear.ForeColor = System.Drawing.Color.White;
            this.btn_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_clear.Location = new System.Drawing.Point(417, 230);
            this.btn_clear.Name = "btn_clear";
            this.btn_clear.Size = new System.Drawing.Size(75, 23);
            this.btn_clear.TabIndex = 18;
            this.btn_clear.Text = "&Clear";
            this.btn_clear.UseVisualStyleBackColor = true;
            this.btn_clear.Click += new System.EventHandler(this.btn_clear_Click);
            // 
            // pb_wallpaper
            // 
            this.pb_wallpaper.BackColor = System.Drawing.Color.White;
            this.pb_wallpaper.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pb_wallpaper.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pb_wallpaper.Enabled = false;
            this.pb_wallpaper.Location = new System.Drawing.Point(28, 111);
            this.pb_wallpaper.Name = "pb_wallpaper";
            this.pb_wallpaper.Size = new System.Drawing.Size(381, 212);
            this.pb_wallpaper.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_wallpaper.TabIndex = 17;
            this.pb_wallpaper.TabStop = false;
            // 
            // btn_select
            // 
            this.btn_select.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_select.Enabled = false;
            this.btn_select.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_select.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_select.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_select.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_select.ForeColor = System.Drawing.Color.White;
            this.btn_select.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_select.Location = new System.Drawing.Point(417, 172);
            this.btn_select.Name = "btn_select";
            this.btn_select.Size = new System.Drawing.Size(75, 23);
            this.btn_select.TabIndex = 16;
            this.btn_select.Text = "&Select";
            this.btn_select.UseVisualStyleBackColor = true;
            this.btn_select.Click += new System.EventHandler(this.btn_select_wallpaper_Click);
            // 
            // lbl_style
            // 
            this.lbl_style.AutoSize = true;
            this.lbl_style.Enabled = false;
            this.lbl_style.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_style.ForeColor = System.Drawing.Color.Transparent;
            this.lbl_style.Location = new System.Drawing.Point(176, 334);
            this.lbl_style.Name = "lbl_style";
            this.lbl_style.Size = new System.Drawing.Size(44, 16);
            this.lbl_style.TabIndex = 13;
            this.lbl_style.Text = "Style: ";
            // 
            // cmb_wallpaper_display_mode
            // 
            this.cmb_wallpaper_display_mode.Enabled = false;
            this.cmb_wallpaper_display_mode.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_wallpaper_display_mode.FormattingEnabled = true;
            this.cmb_wallpaper_display_mode.Location = new System.Drawing.Point(226, 329);
            this.cmb_wallpaper_display_mode.Name = "cmb_wallpaper_display_mode";
            this.cmb_wallpaper_display_mode.Size = new System.Drawing.Size(183, 24);
            this.cmb_wallpaper_display_mode.TabIndex = 12;
            this.cmb_wallpaper_display_mode.SelectedIndexChanged += new System.EventHandler(this.cmb_wallpaper_display_mode_SelectedIndexChanged);
            // 
            // ProfileSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(580, 466);
            this.Controls.Add(this.gb_general);
            this.Controls.Add(this.btn_back);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProfileSettingsForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Profile Settings";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProfileSettingsForm_FormClosing);
            this.Load += new System.EventHandler(this.ProfileSettingsForm_Load);
            this.gb_general.ResumeLayout(false);
            this.gb_general.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_wallpaper)).EndInit();
            this.ResumeLayout(false);

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
    }
}