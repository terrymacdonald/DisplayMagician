namespace DisplayMagician.UIForms
{
    partial class FovForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FovForm));
            this.btn_back = new System.Windows.Forms.Button();
            this.btn_update = new System.Windows.Forms.Button();
            this.btn_clear = new System.Windows.Forms.Button();
            this.pnl_fov = new System.Windows.Forms.Panel();
            this.split_fov = new System.Windows.Forms.SplitContainer();
            this.lbl_bezel_thickness = new System.Windows.Forms.Label();
            this.cmb_bezel_thickness = new System.Windows.Forms.ComboBox();
            this.txt_bezel_thickness = new System.Windows.Forms.TextBox();
            this.lbl_distance_to_screen = new System.Windows.Forms.Label();
            this.cmb_distance_to_screen = new System.Windows.Forms.ComboBox();
            this.txt_distance_to_screen = new System.Windows.Forms.TextBox();
            this.lbl_aspect_ratio = new System.Windows.Forms.Label();
            this.lbl_screen_size = new System.Windows.Forms.Label();
            this.lbl_screen_type = new System.Windows.Forms.Label();
            this.txt_aspect_ratio_y = new System.Windows.Forms.TextBox();
            this.txt_aspect_ratio_x = new System.Windows.Forms.TextBox();
            this.txt_screen_size = new System.Windows.Forms.TextBox();
            this.cmb_screen_size_units = new System.Windows.Forms.ComboBox();
            this.cmb_aspect_ratio = new System.Windows.Forms.ComboBox();
            this.btn_triple_screens = new System.Windows.Forms.Button();
            this.btn_single_screen = new System.Windows.Forms.Button();
            this.lbl_title = new System.Windows.Forms.Label();
            this.pnl_fov.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split_fov)).BeginInit();
            this.split_fov.Panel1.SuspendLayout();
            this.split_fov.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_back
            // 
            this.btn_back.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_back.ForeColor = System.Drawing.Color.White;
            this.btn_back.Location = new System.Drawing.Point(1214, 571);
            this.btn_back.Name = "btn_back";
            this.btn_back.Size = new System.Drawing.Size(75, 23);
            this.btn_back.TabIndex = 6;
            this.btn_back.Text = "&Back";
            this.btn_back.UseVisualStyleBackColor = true;
            this.btn_back.Click += new System.EventHandler(this.btn_back_Click);
            // 
            // btn_update
            // 
            this.btn_update.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_update.BackColor = System.Drawing.Color.Black;
            this.btn_update.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_update.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_update.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_update.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_update.ForeColor = System.Drawing.Color.White;
            this.btn_update.Location = new System.Drawing.Point(244, 555);
            this.btn_update.Name = "btn_update";
            this.btn_update.Size = new System.Drawing.Size(128, 40);
            this.btn_update.TabIndex = 7;
            this.btn_update.Text = "&Update";
            this.btn_update.UseVisualStyleBackColor = false;
            this.btn_update.Click += new System.EventHandler(this.btn_update_Click);
            // 
            // btn_clear
            // 
            this.btn_clear.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_clear.BackColor = System.Drawing.Color.Black;
            this.btn_clear.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_clear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_clear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_clear.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_clear.ForeColor = System.Drawing.Color.White;
            this.btn_clear.Location = new System.Drawing.Point(403, 554);
            this.btn_clear.Name = "btn_clear";
            this.btn_clear.Size = new System.Drawing.Size(128, 40);
            this.btn_clear.TabIndex = 8;
            this.btn_clear.Text = "&Clear";
            this.btn_clear.UseVisualStyleBackColor = false;
            // 
            // pnl_fov
            // 
            this.pnl_fov.Controls.Add(this.split_fov);
            this.pnl_fov.Location = new System.Drawing.Point(0, 55);
            this.pnl_fov.Name = "pnl_fov";
            this.pnl_fov.Size = new System.Drawing.Size(1301, 494);
            this.pnl_fov.TabIndex = 9;
            // 
            // split_fov
            // 
            this.split_fov.BackColor = System.Drawing.Color.White;
            this.split_fov.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split_fov.Location = new System.Drawing.Point(0, 0);
            this.split_fov.Name = "split_fov";
            // 
            // split_fov.Panel1
            // 
            this.split_fov.Panel1.BackColor = System.Drawing.Color.Black;
            this.split_fov.Panel1.Controls.Add(this.lbl_bezel_thickness);
            this.split_fov.Panel1.Controls.Add(this.cmb_bezel_thickness);
            this.split_fov.Panel1.Controls.Add(this.txt_bezel_thickness);
            this.split_fov.Panel1.Controls.Add(this.lbl_distance_to_screen);
            this.split_fov.Panel1.Controls.Add(this.cmb_distance_to_screen);
            this.split_fov.Panel1.Controls.Add(this.txt_distance_to_screen);
            this.split_fov.Panel1.Controls.Add(this.lbl_aspect_ratio);
            this.split_fov.Panel1.Controls.Add(this.lbl_screen_size);
            this.split_fov.Panel1.Controls.Add(this.lbl_screen_type);
            this.split_fov.Panel1.Controls.Add(this.txt_aspect_ratio_y);
            this.split_fov.Panel1.Controls.Add(this.txt_aspect_ratio_x);
            this.split_fov.Panel1.Controls.Add(this.txt_screen_size);
            this.split_fov.Panel1.Controls.Add(this.cmb_screen_size_units);
            this.split_fov.Panel1.Controls.Add(this.cmb_aspect_ratio);
            this.split_fov.Panel1.Controls.Add(this.btn_triple_screens);
            this.split_fov.Panel1.Controls.Add(this.btn_single_screen);
            // 
            // split_fov.Panel2
            // 
            this.split_fov.Panel2.BackColor = System.Drawing.Color.Black;
            this.split_fov.Size = new System.Drawing.Size(1301, 494);
            this.split_fov.SplitterDistance = 846;
            this.split_fov.TabIndex = 0;
            // 
            // lbl_bezel_thickness
            // 
            this.lbl_bezel_thickness.AutoSize = true;
            this.lbl_bezel_thickness.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_bezel_thickness.ForeColor = System.Drawing.Color.White;
            this.lbl_bezel_thickness.Location = new System.Drawing.Point(37, 319);
            this.lbl_bezel_thickness.Name = "lbl_bezel_thickness";
            this.lbl_bezel_thickness.Size = new System.Drawing.Size(133, 20);
            this.lbl_bezel_thickness.TabIndex = 15;
            this.lbl_bezel_thickness.Text = "Bezel Thickness?";
            // 
            // cmb_bezel_thickness
            // 
            this.cmb_bezel_thickness.FormattingEnabled = true;
            this.cmb_bezel_thickness.Location = new System.Drawing.Point(291, 321);
            this.cmb_bezel_thickness.Name = "cmb_bezel_thickness";
            this.cmb_bezel_thickness.Size = new System.Drawing.Size(133, 21);
            this.cmb_bezel_thickness.TabIndex = 14;
            // 
            // txt_bezel_thickness
            // 
            this.txt_bezel_thickness.Location = new System.Drawing.Point(185, 321);
            this.txt_bezel_thickness.Name = "txt_bezel_thickness";
            this.txt_bezel_thickness.Size = new System.Drawing.Size(100, 20);
            this.txt_bezel_thickness.TabIndex = 13;
            // 
            // lbl_distance_to_screen
            // 
            this.lbl_distance_to_screen.AutoSize = true;
            this.lbl_distance_to_screen.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_distance_to_screen.ForeColor = System.Drawing.Color.White;
            this.lbl_distance_to_screen.Location = new System.Drawing.Point(37, 244);
            this.lbl_distance_to_screen.Name = "lbl_distance_to_screen";
            this.lbl_distance_to_screen.Size = new System.Drawing.Size(261, 20);
            this.lbl_distance_to_screen.TabIndex = 12;
            this.lbl_distance_to_screen.Text = "Distance from your eyes to Screen?";
            // 
            // cmb_distance_to_screen
            // 
            this.cmb_distance_to_screen.FormattingEnabled = true;
            this.cmb_distance_to_screen.Location = new System.Drawing.Point(410, 246);
            this.cmb_distance_to_screen.Name = "cmb_distance_to_screen";
            this.cmb_distance_to_screen.Size = new System.Drawing.Size(133, 21);
            this.cmb_distance_to_screen.TabIndex = 11;
            // 
            // txt_distance_to_screen
            // 
            this.txt_distance_to_screen.Location = new System.Drawing.Point(304, 246);
            this.txt_distance_to_screen.Name = "txt_distance_to_screen";
            this.txt_distance_to_screen.Size = new System.Drawing.Size(100, 20);
            this.txt_distance_to_screen.TabIndex = 10;
            // 
            // lbl_aspect_ratio
            // 
            this.lbl_aspect_ratio.AutoSize = true;
            this.lbl_aspect_ratio.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_aspect_ratio.ForeColor = System.Drawing.Color.White;
            this.lbl_aspect_ratio.Location = new System.Drawing.Point(37, 174);
            this.lbl_aspect_ratio.Name = "lbl_aspect_ratio";
            this.lbl_aspect_ratio.Size = new System.Drawing.Size(165, 20);
            this.lbl_aspect_ratio.TabIndex = 9;
            this.lbl_aspect_ratio.Text = "Screen Aspect Ratio?";
            // 
            // lbl_screen_size
            // 
            this.lbl_screen_size.AutoSize = true;
            this.lbl_screen_size.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_screen_size.ForeColor = System.Drawing.Color.White;
            this.lbl_screen_size.Location = new System.Drawing.Point(37, 107);
            this.lbl_screen_size.Name = "lbl_screen_size";
            this.lbl_screen_size.Size = new System.Drawing.Size(104, 20);
            this.lbl_screen_size.TabIndex = 8;
            this.lbl_screen_size.Text = "Screen Size?";
            // 
            // lbl_screen_type
            // 
            this.lbl_screen_type.AutoSize = true;
            this.lbl_screen_type.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_screen_type.ForeColor = System.Drawing.Color.White;
            this.lbl_screen_type.Location = new System.Drawing.Point(37, 51);
            this.lbl_screen_type.Name = "lbl_screen_type";
            this.lbl_screen_type.Size = new System.Drawing.Size(278, 20);
            this.lbl_screen_type.TabIndex = 7;
            this.lbl_screen_type.Text = "Single Screen or Triple Screen layout?";
            // 
            // txt_aspect_ratio_y
            // 
            this.txt_aspect_ratio_y.Location = new System.Drawing.Point(568, 176);
            this.txt_aspect_ratio_y.Name = "txt_aspect_ratio_y";
            this.txt_aspect_ratio_y.Size = new System.Drawing.Size(69, 20);
            this.txt_aspect_ratio_y.TabIndex = 6;
            // 
            // txt_aspect_ratio_x
            // 
            this.txt_aspect_ratio_x.Location = new System.Drawing.Point(498, 176);
            this.txt_aspect_ratio_x.Name = "txt_aspect_ratio_x";
            this.txt_aspect_ratio_x.Size = new System.Drawing.Size(64, 20);
            this.txt_aspect_ratio_x.TabIndex = 5;
            // 
            // txt_screen_size
            // 
            this.txt_screen_size.Location = new System.Drawing.Point(161, 109);
            this.txt_screen_size.Name = "txt_screen_size";
            this.txt_screen_size.Size = new System.Drawing.Size(100, 20);
            this.txt_screen_size.TabIndex = 4;
            // 
            // cmb_screen_size_units
            // 
            this.cmb_screen_size_units.FormattingEnabled = true;
            this.cmb_screen_size_units.Location = new System.Drawing.Point(267, 107);
            this.cmb_screen_size_units.Name = "cmb_screen_size_units";
            this.cmb_screen_size_units.Size = new System.Drawing.Size(133, 21);
            this.cmb_screen_size_units.TabIndex = 3;
            // 
            // cmb_aspect_ratio
            // 
            this.cmb_aspect_ratio.FormattingEnabled = true;
            this.cmb_aspect_ratio.Location = new System.Drawing.Point(220, 176);
            this.cmb_aspect_ratio.Name = "cmb_aspect_ratio";
            this.cmb_aspect_ratio.Size = new System.Drawing.Size(259, 21);
            this.cmb_aspect_ratio.TabIndex = 2;
            // 
            // btn_triple_screens
            // 
            this.btn_triple_screens.Location = new System.Drawing.Point(486, 46);
            this.btn_triple_screens.Name = "btn_triple_screens";
            this.btn_triple_screens.Size = new System.Drawing.Size(133, 33);
            this.btn_triple_screens.TabIndex = 1;
            this.btn_triple_screens.Text = "Triple Screens";
            this.btn_triple_screens.UseVisualStyleBackColor = true;
            this.btn_triple_screens.Click += new System.EventHandler(this.btn_triple_screens_Click);
            // 
            // btn_single_screen
            // 
            this.btn_single_screen.Location = new System.Drawing.Point(326, 46);
            this.btn_single_screen.Name = "btn_single_screen";
            this.btn_single_screen.Size = new System.Drawing.Size(133, 33);
            this.btn_single_screen.TabIndex = 0;
            this.btn_single_screen.Text = "Single Screen";
            this.btn_single_screen.UseVisualStyleBackColor = true;
            this.btn_single_screen.Click += new System.EventHandler(this.btn_single_screen_Click);
            // 
            // lbl_title
            // 
            this.lbl_title.AutoSize = true;
            this.lbl_title.BackColor = System.Drawing.Color.Black;
            this.lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_title.ForeColor = System.Drawing.Color.White;
            this.lbl_title.Location = new System.Drawing.Point(481, 14);
            this.lbl_title.Name = "lbl_title";
            this.lbl_title.Size = new System.Drawing.Size(338, 29);
            this.lbl_title.TabIndex = 20;
            this.lbl_title.Text = "Field of View (FOV) Calculator";
            this.lbl_title.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // FovForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1301, 606);
            this.Controls.Add(this.lbl_title);
            this.Controls.Add(this.btn_clear);
            this.Controls.Add(this.btn_update);
            this.Controls.Add(this.pnl_fov);
            this.Controls.Add(this.btn_back);
            this.Name = "FovForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Field of View (FOV) Calculator";
            this.pnl_fov.ResumeLayout(false);
            this.split_fov.Panel1.ResumeLayout(false);
            this.split_fov.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split_fov)).EndInit();
            this.split_fov.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.Button btn_update;
        private System.Windows.Forms.Button btn_clear;
        private System.Windows.Forms.Panel pnl_fov;
        private System.Windows.Forms.SplitContainer split_fov;
        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Label lbl_bezel_thickness;
        private System.Windows.Forms.ComboBox cmb_bezel_thickness;
        private System.Windows.Forms.TextBox txt_bezel_thickness;
        private System.Windows.Forms.Label lbl_distance_to_screen;
        private System.Windows.Forms.ComboBox cmb_distance_to_screen;
        private System.Windows.Forms.TextBox txt_distance_to_screen;
        private System.Windows.Forms.Label lbl_aspect_ratio;
        private System.Windows.Forms.Label lbl_screen_size;
        private System.Windows.Forms.Label lbl_screen_type;
        private System.Windows.Forms.TextBox txt_aspect_ratio_y;
        private System.Windows.Forms.TextBox txt_aspect_ratio_x;
        private System.Windows.Forms.TextBox txt_screen_size;
        private System.Windows.Forms.ComboBox cmb_screen_size_units;
        private System.Windows.Forms.ComboBox cmb_aspect_ratio;
        private System.Windows.Forms.Button btn_triple_screens;
        private System.Windows.Forms.Button btn_single_screen;
    }
}