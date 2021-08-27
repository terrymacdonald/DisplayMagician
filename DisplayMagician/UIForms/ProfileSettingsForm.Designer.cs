
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
            this.btn_select_wallpaper = new System.Windows.Forms.Button();
            this.txt_wallpaper_filename = new System.Windows.Forms.TextBox();
            this.cb_set_wallpaper = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmb_wallpaper_display_mode = new System.Windows.Forms.ComboBox();
            this.gb_general.SuspendLayout();
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
            this.btn_back.Location = new System.Drawing.Point(476, 202);
            this.btn_back.Name = "btn_back";
            this.btn_back.Size = new System.Drawing.Size(75, 23);
            this.btn_back.TabIndex = 9;
            this.btn_back.Text = "&Back";
            this.btn_back.UseVisualStyleBackColor = true;
            this.btn_back.Click += new System.EventHandler(this.btn_back_Click);
            // 
            // gb_general
            // 
            this.gb_general.Controls.Add(this.btn_select_wallpaper);
            this.gb_general.Controls.Add(this.txt_wallpaper_filename);
            this.gb_general.Controls.Add(this.cb_set_wallpaper);
            this.gb_general.Controls.Add(this.label1);
            this.gb_general.Controls.Add(this.cmb_wallpaper_display_mode);
            this.gb_general.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gb_general.ForeColor = System.Drawing.Color.White;
            this.gb_general.Location = new System.Drawing.Point(27, 21);
            this.gb_general.Name = "gb_general";
            this.gb_general.Size = new System.Drawing.Size(525, 157);
            this.gb_general.TabIndex = 11;
            this.gb_general.TabStop = false;
            this.gb_general.Text = "Wallpaper Settings";
            // 
            // btn_select_wallpaper
            // 
            this.btn_select_wallpaper.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_select_wallpaper.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_select_wallpaper.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_select_wallpaper.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_select_wallpaper.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_select_wallpaper.ForeColor = System.Drawing.Color.White;
            this.btn_select_wallpaper.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_select_wallpaper.Location = new System.Drawing.Point(428, 68);
            this.btn_select_wallpaper.Name = "btn_select_wallpaper";
            this.btn_select_wallpaper.Size = new System.Drawing.Size(75, 23);
            this.btn_select_wallpaper.TabIndex = 16;
            this.btn_select_wallpaper.Text = "&Select";
            this.btn_select_wallpaper.UseVisualStyleBackColor = true;
            // 
            // txt_wallpaper_filename
            // 
            this.txt_wallpaper_filename.Location = new System.Drawing.Point(28, 68);
            this.txt_wallpaper_filename.Name = "txt_wallpaper_filename";
            this.txt_wallpaper_filename.Size = new System.Drawing.Size(394, 22);
            this.txt_wallpaper_filename.TabIndex = 15;
            // 
            // cb_set_wallpaper
            // 
            this.cb_set_wallpaper.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cb_set_wallpaper.AutoSize = true;
            this.cb_set_wallpaper.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.cb_set_wallpaper.ForeColor = System.Drawing.Color.White;
            this.cb_set_wallpaper.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cb_set_wallpaper.Location = new System.Drawing.Point(28, 36);
            this.cb_set_wallpaper.Name = "cb_set_wallpaper";
            this.cb_set_wallpaper.Size = new System.Drawing.Size(261, 20);
            this.cb_set_wallpaper.TabIndex = 14;
            this.cb_set_wallpaper.Text = "Set wallpaper when applying this profile";
            this.cb_set_wallpaper.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(42, 111);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(155, 16);
            this.label1.TabIndex = 13;
            this.label1.Text = "Wallpaper display style: ";
            // 
            // cmb_wallpaper_display_mode
            // 
            this.cmb_wallpaper_display_mode.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_wallpaper_display_mode.FormattingEnabled = true;
            this.cmb_wallpaper_display_mode.Items.AddRange(new object[] {
            "Centered",
            "Stretched",
            "Tiled"});
            this.cmb_wallpaper_display_mode.Location = new System.Drawing.Point(203, 108);
            this.cmb_wallpaper_display_mode.Name = "cmb_wallpaper_display_mode";
            this.cmb_wallpaper_display_mode.Size = new System.Drawing.Size(219, 24);
            this.cmb_wallpaper_display_mode.TabIndex = 12;
            this.cmb_wallpaper_display_mode.SelectedIndexChanged += new System.EventHandler(this.cmb_wallpaper_display_mode_SelectedIndexChanged);
            // 
            // ProfileSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(580, 249);
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
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.GroupBox gb_general;
        private System.Windows.Forms.CheckBox cb_set_wallpaper;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmb_wallpaper_display_mode;
        private System.Windows.Forms.TextBox txt_wallpaper_filename;
        private System.Windows.Forms.Button btn_select_wallpaper;
    }
}