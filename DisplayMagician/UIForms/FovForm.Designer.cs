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
            this.btn_calculate = new System.Windows.Forms.Button();
            this.btn_clear = new System.Windows.Forms.Button();
            this.pnl_fov = new System.Windows.Forms.Panel();
            this.split_fov = new System.Windows.Forms.SplitContainer();
            this.lbl_title = new System.Windows.Forms.Label();
            this.btn_single_screen = new System.Windows.Forms.Button();
            this.btn_triple_screens = new System.Windows.Forms.Button();
            this.cmb_aspect_ratio = new System.Windows.Forms.ComboBox();
            this.cmb_screen_size_units = new System.Windows.Forms.ComboBox();
            this.txt_screen_size = new System.Windows.Forms.TextBox();
            this.txt_aspect_ratio_x = new System.Windows.Forms.TextBox();
            this.txt_aspect_ratio_y = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_distance_to_screen = new System.Windows.Forms.TextBox();
            this.cmb_distance_to_screen = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txt_bezel_thickness = new System.Windows.Forms.TextBox();
            this.cmb_bezel_thickness = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
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
            // btn_calculate
            // 
            this.btn_calculate.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_calculate.BackColor = System.Drawing.Color.Black;
            this.btn_calculate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_calculate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_calculate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_calculate.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_calculate.ForeColor = System.Drawing.Color.White;
            this.btn_calculate.Location = new System.Drawing.Point(244, 555);
            this.btn_calculate.Name = "btn_calculate";
            this.btn_calculate.Size = new System.Drawing.Size(128, 40);
            this.btn_calculate.TabIndex = 7;
            this.btn_calculate.Text = "&Update";
            this.btn_calculate.UseVisualStyleBackColor = false;
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
            this.split_fov.Panel1.Controls.Add(this.label5);
            this.split_fov.Panel1.Controls.Add(this.cmb_bezel_thickness);
            this.split_fov.Panel1.Controls.Add(this.txt_bezel_thickness);
            this.split_fov.Panel1.Controls.Add(this.label4);
            this.split_fov.Panel1.Controls.Add(this.cmb_distance_to_screen);
            this.split_fov.Panel1.Controls.Add(this.txt_distance_to_screen);
            this.split_fov.Panel1.Controls.Add(this.label3);
            this.split_fov.Panel1.Controls.Add(this.label2);
            this.split_fov.Panel1.Controls.Add(this.label1);
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
            // btn_single_screen
            // 
            this.btn_single_screen.Location = new System.Drawing.Point(359, 38);
            this.btn_single_screen.Name = "btn_single_screen";
            this.btn_single_screen.Size = new System.Drawing.Size(133, 33);
            this.btn_single_screen.TabIndex = 0;
            this.btn_single_screen.Text = "Single Screen";
            this.btn_single_screen.UseVisualStyleBackColor = true;
            // 
            // btn_triple_screens
            // 
            this.btn_triple_screens.Location = new System.Drawing.Point(527, 38);
            this.btn_triple_screens.Name = "btn_triple_screens";
            this.btn_triple_screens.Size = new System.Drawing.Size(133, 33);
            this.btn_triple_screens.TabIndex = 1;
            this.btn_triple_screens.Text = "Triple Screens";
            this.btn_triple_screens.UseVisualStyleBackColor = true;
            // 
            // cmb_aspect_ratio
            // 
            this.cmb_aspect_ratio.FormattingEnabled = true;
            this.cmb_aspect_ratio.Location = new System.Drawing.Point(359, 163);
            this.cmb_aspect_ratio.Name = "cmb_aspect_ratio";
            this.cmb_aspect_ratio.Size = new System.Drawing.Size(259, 21);
            this.cmb_aspect_ratio.TabIndex = 2;
            // 
            // cmb_screen_size_units
            // 
            this.cmb_screen_size_units.FormattingEnabled = true;
            this.cmb_screen_size_units.Location = new System.Drawing.Point(465, 102);
            this.cmb_screen_size_units.Name = "cmb_screen_size_units";
            this.cmb_screen_size_units.Size = new System.Drawing.Size(133, 21);
            this.cmb_screen_size_units.TabIndex = 3;
            // 
            // txt_screen_size
            // 
            this.txt_screen_size.Location = new System.Drawing.Point(359, 102);
            this.txt_screen_size.Name = "txt_screen_size";
            this.txt_screen_size.Size = new System.Drawing.Size(100, 20);
            this.txt_screen_size.TabIndex = 4;
            // 
            // txt_aspect_ratio_x
            // 
            this.txt_aspect_ratio_x.Location = new System.Drawing.Point(358, 210);
            this.txt_aspect_ratio_x.Name = "txt_aspect_ratio_x";
            this.txt_aspect_ratio_x.Size = new System.Drawing.Size(100, 20);
            this.txt_aspect_ratio_x.TabIndex = 5;
            // 
            // txt_aspect_ratio_y
            // 
            this.txt_aspect_ratio_y.Location = new System.Drawing.Point(486, 210);
            this.txt_aspect_ratio_y.Name = "txt_aspect_ratio_y";
            this.txt_aspect_ratio_y.Size = new System.Drawing.Size(100, 20);
            this.txt_aspect_ratio_y.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(132, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(188, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Single Screen or Triple Screen layout?";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(132, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Screen Size?";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(132, 171);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Screen Aspect Ratio?";
            // 
            // txt_distance_to_screen
            // 
            this.txt_distance_to_screen.Location = new System.Drawing.Point(359, 277);
            this.txt_distance_to_screen.Name = "txt_distance_to_screen";
            this.txt_distance_to_screen.Size = new System.Drawing.Size(100, 20);
            this.txt_distance_to_screen.TabIndex = 10;
            // 
            // cmb_distance_to_screen
            // 
            this.cmb_distance_to_screen.FormattingEnabled = true;
            this.cmb_distance_to_screen.Location = new System.Drawing.Point(465, 277);
            this.cmb_distance_to_screen.Name = "cmb_distance_to_screen";
            this.cmb_distance_to_screen.Size = new System.Drawing.Size(133, 21);
            this.cmb_distance_to_screen.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(132, 280);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(175, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Distance from your eyes to Screen?";
            // 
            // txt_bezel_thickness
            // 
            this.txt_bezel_thickness.Location = new System.Drawing.Point(358, 344);
            this.txt_bezel_thickness.Name = "txt_bezel_thickness";
            this.txt_bezel_thickness.Size = new System.Drawing.Size(100, 20);
            this.txt_bezel_thickness.TabIndex = 13;
            // 
            // cmb_bezel_thickness
            // 
            this.cmb_bezel_thickness.FormattingEnabled = true;
            this.cmb_bezel_thickness.Location = new System.Drawing.Point(465, 343);
            this.cmb_bezel_thickness.Name = "cmb_bezel_thickness";
            this.cmb_bezel_thickness.Size = new System.Drawing.Size(133, 21);
            this.cmb_bezel_thickness.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(132, 346);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Bezel Thickness?";
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
            this.Controls.Add(this.btn_calculate);
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
        private System.Windows.Forms.Button btn_calculate;
        private System.Windows.Forms.Button btn_clear;
        private System.Windows.Forms.Panel pnl_fov;
        private System.Windows.Forms.SplitContainer split_fov;
        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmb_bezel_thickness;
        private System.Windows.Forms.TextBox txt_bezel_thickness;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmb_distance_to_screen;
        private System.Windows.Forms.TextBox txt_distance_to_screen;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_aspect_ratio_y;
        private System.Windows.Forms.TextBox txt_aspect_ratio_x;
        private System.Windows.Forms.TextBox txt_screen_size;
        private System.Windows.Forms.ComboBox cmb_screen_size_units;
        private System.Windows.Forms.ComboBox cmb_aspect_ratio;
        private System.Windows.Forms.Button btn_triple_screens;
        private System.Windows.Forms.Button btn_single_screen;
    }
}