namespace DisplayMagician.UIForms
{
    partial class FovCalcForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FovCalcForm));
            btn_back = new System.Windows.Forms.Button();
            pnl_fov = new System.Windows.Forms.Panel();
            split_fov = new System.Windows.Forms.SplitContainer();
            llbl_markus_ewert = new System.Windows.Forms.LinkLabel();
            lbl_bezel_thickness_description = new System.Windows.Forms.Label();
            lbl_distinace_to_screen_tip = new System.Windows.Forms.Label();
            lbl_screen_type = new System.Windows.Forms.Label();
            lbl_distance_to_screen = new System.Windows.Forms.Label();
            lbl_distance_to_screen_description = new System.Windows.Forms.Label();
            lbl_aspect_ratio_description = new System.Windows.Forms.Label();
            lbl_screen_size_description = new System.Windows.Forms.Label();
            lbl_screen_type_description = new System.Windows.Forms.Label();
            btn_triple_screens = new System.Windows.Forms.Button();
            btn_single_screen = new System.Windows.Forms.Button();
            lbl_bezel_thickness = new System.Windows.Forms.Label();
            cmb_bezel_thickness = new System.Windows.Forms.ComboBox();
            txt_bezel_thickness = new System.Windows.Forms.TextBox();
            cmb_distance_to_screen = new System.Windows.Forms.ComboBox();
            txt_distance_to_screen = new System.Windows.Forms.TextBox();
            lbl_aspect_ratio = new System.Windows.Forms.Label();
            lbl_screen_size = new System.Windows.Forms.Label();
            txt_aspect_ratio_y = new System.Windows.Forms.TextBox();
            txt_aspect_ratio_x = new System.Windows.Forms.TextBox();
            txt_screen_size = new System.Windows.Forms.TextBox();
            cmb_screen_size_units = new System.Windows.Forms.ComboBox();
            cmb_aspect_ratio = new System.Windows.Forms.ComboBox();
            lbl_aspect_ratio_arrow = new System.Windows.Forms.Label();
            lbl_aspect_ratio_separator = new System.Windows.Forms.Label();
            lbl_vresult = new System.Windows.Forms.Label();
            lbl_vtitle = new System.Windows.Forms.Label();
            lbl_hresult = new System.Windows.Forms.Label();
            lbl_htitle = new System.Windows.Forms.Label();
            btn_save = new System.Windows.Forms.Button();
            lbl_results = new System.Windows.Forms.Label();
            rtb_results = new System.Windows.Forms.RichTextBox();
            lbl_title = new System.Windows.Forms.Label();
            lbl_about_fov = new System.Windows.Forms.Label();
            pnl_fov.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)split_fov).BeginInit();
            split_fov.Panel1.SuspendLayout();
            split_fov.Panel2.SuspendLayout();
            split_fov.SuspendLayout();
            SuspendLayout();
            // 
            // btn_back
            // 
            btn_back.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(1416, 848);
            btn_back.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 27);
            btn_back.TabIndex = 6;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // pnl_fov
            // 
            pnl_fov.Controls.Add(split_fov);
            pnl_fov.Location = new System.Drawing.Point(2, 148);
            pnl_fov.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pnl_fov.Name = "pnl_fov";
            pnl_fov.Size = new System.Drawing.Size(1518, 685);
            pnl_fov.TabIndex = 9;
            // 
            // split_fov
            // 
            split_fov.BackColor = System.Drawing.Color.White;
            split_fov.Dock = System.Windows.Forms.DockStyle.Fill;
            split_fov.Location = new System.Drawing.Point(0, 0);
            split_fov.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            split_fov.Name = "split_fov";
            // 
            // split_fov.Panel1
            // 
            split_fov.Panel1.BackColor = System.Drawing.Color.Black;
            split_fov.Panel1.Controls.Add(llbl_markus_ewert);
            split_fov.Panel1.Controls.Add(lbl_bezel_thickness_description);
            split_fov.Panel1.Controls.Add(lbl_distinace_to_screen_tip);
            split_fov.Panel1.Controls.Add(lbl_screen_type);
            split_fov.Panel1.Controls.Add(lbl_distance_to_screen);
            split_fov.Panel1.Controls.Add(lbl_distance_to_screen_description);
            split_fov.Panel1.Controls.Add(lbl_aspect_ratio_description);
            split_fov.Panel1.Controls.Add(lbl_screen_size_description);
            split_fov.Panel1.Controls.Add(lbl_screen_type_description);
            split_fov.Panel1.Controls.Add(btn_triple_screens);
            split_fov.Panel1.Controls.Add(btn_single_screen);
            split_fov.Panel1.Controls.Add(lbl_bezel_thickness);
            split_fov.Panel1.Controls.Add(cmb_bezel_thickness);
            split_fov.Panel1.Controls.Add(txt_bezel_thickness);
            split_fov.Panel1.Controls.Add(cmb_distance_to_screen);
            split_fov.Panel1.Controls.Add(txt_distance_to_screen);
            split_fov.Panel1.Controls.Add(lbl_aspect_ratio);
            split_fov.Panel1.Controls.Add(lbl_screen_size);
            split_fov.Panel1.Controls.Add(txt_aspect_ratio_y);
            split_fov.Panel1.Controls.Add(txt_aspect_ratio_x);
            split_fov.Panel1.Controls.Add(txt_screen_size);
            split_fov.Panel1.Controls.Add(cmb_screen_size_units);
            split_fov.Panel1.Controls.Add(cmb_aspect_ratio);
            split_fov.Panel1.Controls.Add(lbl_aspect_ratio_arrow);
            split_fov.Panel1.Controls.Add(lbl_aspect_ratio_separator);
            // 
            // split_fov.Panel2
            // 
            split_fov.Panel2.BackColor = System.Drawing.Color.Black;
            split_fov.Panel2.Controls.Add(lbl_vresult);
            split_fov.Panel2.Controls.Add(lbl_vtitle);
            split_fov.Panel2.Controls.Add(lbl_hresult);
            split_fov.Panel2.Controls.Add(lbl_htitle);
            split_fov.Panel2.Controls.Add(btn_save);
            split_fov.Panel2.Controls.Add(lbl_results);
            split_fov.Panel2.Controls.Add(rtb_results);
            split_fov.Size = new System.Drawing.Size(1518, 685);
            split_fov.SplitterDistance = 829;
            split_fov.SplitterWidth = 5;
            split_fov.TabIndex = 0;
            // 
            // llbl_markus_ewert
            // 
            llbl_markus_ewert.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            llbl_markus_ewert.AutoSize = true;
            llbl_markus_ewert.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            llbl_markus_ewert.LinkColor = System.Drawing.Color.LightSkyBlue;
            llbl_markus_ewert.Location = new System.Drawing.Point(251, 654);
            llbl_markus_ewert.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            llbl_markus_ewert.Name = "llbl_markus_ewert";
            llbl_markus_ewert.Size = new System.Drawing.Size(347, 15);
            llbl_markus_ewert.TabIndex = 54;
            llbl_markus_ewert.TabStop = true;
            llbl_markus_ewert.Text = "A massive thanks to Markus Ewert for permission to use his code";
            llbl_markus_ewert.LinkClicked += llbl_markus_ewert_LinkClicked;
            // 
            // lbl_bezel_thickness_description
            // 
            lbl_bezel_thickness_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_bezel_thickness_description.ForeColor = System.Drawing.Color.IndianRed;
            lbl_bezel_thickness_description.Location = new System.Drawing.Point(28, 542);
            lbl_bezel_thickness_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_bezel_thickness_description.Name = "lbl_bezel_thickness_description";
            lbl_bezel_thickness_description.Size = new System.Drawing.Size(407, 58);
            lbl_bezel_thickness_description.TabIndex = 51;
            lbl_bezel_thickness_description.Text = "Lastly, if you are using triple screens, you need to measure the width of your bezels at the sides of your screens. This is so that we can take account of how they affect the field of view.";
            // 
            // lbl_distinace_to_screen_tip
            // 
            lbl_distinace_to_screen_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_distinace_to_screen_tip.ForeColor = System.Drawing.Color.Gold;
            lbl_distinace_to_screen_tip.Location = new System.Drawing.Point(28, 432);
            lbl_distinace_to_screen_tip.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_distinace_to_screen_tip.Name = "lbl_distinace_to_screen_tip";
            lbl_distinace_to_screen_tip.Size = new System.Drawing.Size(408, 59);
            lbl_distinace_to_screen_tip.TabIndex = 50;
            lbl_distinace_to_screen_tip.Text = "(TIP: Try and place your screens as close to your eyes as your simracing rig will allow. This will let you see more of what is happening around you)";
            // 
            // lbl_screen_type
            // 
            lbl_screen_type.AutoSize = true;
            lbl_screen_type.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_screen_type.ForeColor = System.Drawing.Color.White;
            lbl_screen_type.Location = new System.Drawing.Point(27, 24);
            lbl_screen_type.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_screen_type.Name = "lbl_screen_type";
            lbl_screen_type.Size = new System.Drawing.Size(295, 20);
            lbl_screen_type.TabIndex = 7;
            lbl_screen_type.Text = "1. Single Screen or Triple Screen layout?";
            // 
            // lbl_distance_to_screen
            // 
            lbl_distance_to_screen.AutoSize = true;
            lbl_distance_to_screen.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_distance_to_screen.ForeColor = System.Drawing.Color.White;
            lbl_distance_to_screen.Location = new System.Drawing.Point(26, 367);
            lbl_distance_to_screen.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_distance_to_screen.Name = "lbl_distance_to_screen";
            lbl_distance_to_screen.Size = new System.Drawing.Size(314, 20);
            lbl_distance_to_screen.TabIndex = 12;
            lbl_distance_to_screen.Text = "4. How far are your eyes from the Screens?";
            // 
            // lbl_distance_to_screen_description
            // 
            lbl_distance_to_screen_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_distance_to_screen_description.ForeColor = System.Drawing.Color.IndianRed;
            lbl_distance_to_screen_description.Location = new System.Drawing.Point(27, 390);
            lbl_distance_to_screen_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_distance_to_screen_description.Name = "lbl_distance_to_screen_description";
            lbl_distance_to_screen_description.Size = new System.Drawing.Size(408, 42);
            lbl_distance_to_screen_description.TabIndex = 49;
            lbl_distance_to_screen_description.Text = "Now, you need to measure how far your eyes are away from the center of the middle screen. I use a tape measure for this.";
            // 
            // lbl_aspect_ratio_description
            // 
            lbl_aspect_ratio_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_aspect_ratio_description.ForeColor = System.Drawing.Color.IndianRed;
            lbl_aspect_ratio_description.Location = new System.Drawing.Point(28, 275);
            lbl_aspect_ratio_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_aspect_ratio_description.Name = "lbl_aspect_ratio_description";
            lbl_aspect_ratio_description.Size = new System.Drawing.Size(407, 60);
            lbl_aspect_ratio_description.TabIndex = 48;
            lbl_aspect_ratio_description.Text = "Next, select the aspect ratio your of your screens. Check your display manual if you are unsure, or visit https://whatismyresolution.com/";
            // 
            // lbl_screen_size_description
            // 
            lbl_screen_size_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_screen_size_description.ForeColor = System.Drawing.Color.IndianRed;
            lbl_screen_size_description.Location = new System.Drawing.Point(28, 157);
            lbl_screen_size_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_screen_size_description.Name = "lbl_screen_size_description";
            lbl_screen_size_description.Size = new System.Drawing.Size(407, 67);
            lbl_screen_size_description.TabIndex = 47;
            lbl_screen_size_description.Text = "Next, measure the visible diagonal area of a single screen, ignoring the bezels around the screen. Measure from the top left to the bottom right.";
            // 
            // lbl_screen_type_description
            // 
            lbl_screen_type_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_screen_type_description.ForeColor = System.Drawing.Color.IndianRed;
            lbl_screen_type_description.Location = new System.Drawing.Point(27, 45);
            lbl_screen_type_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_screen_type_description.Name = "lbl_screen_type_description";
            lbl_screen_type_description.Size = new System.Drawing.Size(408, 58);
            lbl_screen_type_description.TabIndex = 46;
            lbl_screen_type_description.Text = "First step is to choose whether you wish to use a Single Screen, or a Triple Screen. Choose Single Screen if the game requires NVIDIA Surround or AMD Eyefinity.";
            // 
            // btn_triple_screens
            // 
            btn_triple_screens.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_triple_screens.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_triple_screens.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_triple_screens.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_triple_screens.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_triple_screens.ForeColor = System.Drawing.Color.White;
            btn_triple_screens.Location = new System.Drawing.Point(639, 63);
            btn_triple_screens.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_triple_screens.Name = "btn_triple_screens";
            btn_triple_screens.Size = new System.Drawing.Size(162, 38);
            btn_triple_screens.TabIndex = 45;
            btn_triple_screens.Text = "Triple Screens";
            btn_triple_screens.UseVisualStyleBackColor = true;
            btn_triple_screens.Click += btn_triple_screens_Click;
            // 
            // btn_single_screen
            // 
            btn_single_screen.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_single_screen.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_single_screen.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_single_screen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_single_screen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_single_screen.ForeColor = System.Drawing.Color.White;
            btn_single_screen.Location = new System.Drawing.Point(453, 63);
            btn_single_screen.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_single_screen.Name = "btn_single_screen";
            btn_single_screen.Size = new System.Drawing.Size(162, 38);
            btn_single_screen.TabIndex = 44;
            btn_single_screen.Text = "Single Screen";
            btn_single_screen.UseVisualStyleBackColor = true;
            btn_single_screen.Click += btn_single_screen_Click;
            // 
            // lbl_bezel_thickness
            // 
            lbl_bezel_thickness.AutoSize = true;
            lbl_bezel_thickness.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_bezel_thickness.ForeColor = System.Drawing.Color.White;
            lbl_bezel_thickness.Location = new System.Drawing.Point(27, 519);
            lbl_bezel_thickness.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_bezel_thickness.Name = "lbl_bezel_thickness";
            lbl_bezel_thickness.Size = new System.Drawing.Size(150, 20);
            lbl_bezel_thickness.TabIndex = 15;
            lbl_bezel_thickness.Text = "5. Bezel Thickness?";
            // 
            // cmb_bezel_thickness
            // 
            cmb_bezel_thickness.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmb_bezel_thickness.FormattingEnabled = true;
            cmb_bezel_thickness.Location = new System.Drawing.Point(646, 542);
            cmb_bezel_thickness.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cmb_bezel_thickness.Name = "cmb_bezel_thickness";
            cmb_bezel_thickness.Size = new System.Drawing.Size(154, 28);
            cmb_bezel_thickness.TabIndex = 14;
            cmb_bezel_thickness.SelectedIndexChanged += cmb_bezel_thickness_SelectedIndexChanged;
            // 
            // txt_bezel_thickness
            // 
            txt_bezel_thickness.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_bezel_thickness.Location = new System.Drawing.Point(548, 542);
            txt_bezel_thickness.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_bezel_thickness.Name = "txt_bezel_thickness";
            txt_bezel_thickness.Size = new System.Drawing.Size(90, 26);
            txt_bezel_thickness.TabIndex = 13;
            txt_bezel_thickness.TextChanged += txt_bezel_thickness_TextChanged;
            // 
            // cmb_distance_to_screen
            // 
            cmb_distance_to_screen.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmb_distance_to_screen.FormattingEnabled = true;
            cmb_distance_to_screen.Location = new System.Drawing.Point(646, 390);
            cmb_distance_to_screen.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cmb_distance_to_screen.Name = "cmb_distance_to_screen";
            cmb_distance_to_screen.Size = new System.Drawing.Size(154, 28);
            cmb_distance_to_screen.TabIndex = 11;
            cmb_distance_to_screen.SelectedIndexChanged += cmb_distance_to_screen_SelectedIndexChanged;
            // 
            // txt_distance_to_screen
            // 
            txt_distance_to_screen.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_distance_to_screen.Location = new System.Drawing.Point(548, 390);
            txt_distance_to_screen.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_distance_to_screen.Name = "txt_distance_to_screen";
            txt_distance_to_screen.Size = new System.Drawing.Size(90, 26);
            txt_distance_to_screen.TabIndex = 10;
            txt_distance_to_screen.TextChanged += txt_distance_to_screen_TextChanged;
            // 
            // lbl_aspect_ratio
            // 
            lbl_aspect_ratio.AutoSize = true;
            lbl_aspect_ratio.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_aspect_ratio.ForeColor = System.Drawing.Color.White;
            lbl_aspect_ratio.Location = new System.Drawing.Point(27, 252);
            lbl_aspect_ratio.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_aspect_ratio.Name = "lbl_aspect_ratio";
            lbl_aspect_ratio.Size = new System.Drawing.Size(286, 20);
            lbl_aspect_ratio.TabIndex = 9;
            lbl_aspect_ratio.Text = "3. What Aspect Ratio are the Screens?";
            // 
            // lbl_screen_size
            // 
            lbl_screen_size.AutoSize = true;
            lbl_screen_size.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_screen_size.ForeColor = System.Drawing.Color.White;
            lbl_screen_size.Location = new System.Drawing.Point(27, 134);
            lbl_screen_size.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_screen_size.Name = "lbl_screen_size";
            lbl_screen_size.Size = new System.Drawing.Size(266, 20);
            lbl_screen_size.TabIndex = 8;
            lbl_screen_size.Text = "2. What sized Screens do you have?";
            // 
            // txt_aspect_ratio_y
            // 
            txt_aspect_ratio_y.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_aspect_ratio_y.Location = new System.Drawing.Point(548, 275);
            txt_aspect_ratio_y.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_aspect_ratio_y.Name = "txt_aspect_ratio_y";
            txt_aspect_ratio_y.Size = new System.Drawing.Size(80, 26);
            txt_aspect_ratio_y.TabIndex = 6;
            txt_aspect_ratio_y.TextChanged += txt_aspect_ratio_y_TextChanged;
            // 
            // txt_aspect_ratio_x
            // 
            txt_aspect_ratio_x.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_aspect_ratio_x.Location = new System.Drawing.Point(456, 275);
            txt_aspect_ratio_x.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_aspect_ratio_x.Name = "txt_aspect_ratio_x";
            txt_aspect_ratio_x.Size = new System.Drawing.Size(74, 26);
            txt_aspect_ratio_x.TabIndex = 5;
            txt_aspect_ratio_x.TextChanged += txt_aspect_ratio_x_TextChanged;
            // 
            // txt_screen_size
            // 
            txt_screen_size.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_screen_size.Location = new System.Drawing.Point(548, 157);
            txt_screen_size.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_screen_size.Name = "txt_screen_size";
            txt_screen_size.Size = new System.Drawing.Size(90, 26);
            txt_screen_size.TabIndex = 4;
            txt_screen_size.TextChanged += txt_screen_size_TextChanged;
            // 
            // cmb_screen_size_units
            // 
            cmb_screen_size_units.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmb_screen_size_units.FormattingEnabled = true;
            cmb_screen_size_units.Location = new System.Drawing.Point(646, 157);
            cmb_screen_size_units.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cmb_screen_size_units.Name = "cmb_screen_size_units";
            cmb_screen_size_units.Size = new System.Drawing.Size(154, 28);
            cmb_screen_size_units.TabIndex = 3;
            cmb_screen_size_units.SelectedIndexChanged += cmb_screen_size_units_SelectedIndexChanged;
            // 
            // cmb_aspect_ratio
            // 
            cmb_aspect_ratio.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmb_aspect_ratio.FormattingEnabled = true;
            cmb_aspect_ratio.Location = new System.Drawing.Point(662, 275);
            cmb_aspect_ratio.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cmb_aspect_ratio.Name = "cmb_aspect_ratio";
            cmb_aspect_ratio.Size = new System.Drawing.Size(139, 28);
            cmb_aspect_ratio.TabIndex = 2;
            cmb_aspect_ratio.SelectedIndexChanged += cmb_aspect_ratio_SelectedIndexChanged;
            // 
            // lbl_aspect_ratio_arrow
            // 
            lbl_aspect_ratio_arrow.AutoSize = true;
            lbl_aspect_ratio_arrow.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_aspect_ratio_arrow.ForeColor = System.Drawing.Color.White;
            lbl_aspect_ratio_arrow.Location = new System.Drawing.Point(632, 278);
            lbl_aspect_ratio_arrow.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_aspect_ratio_arrow.Name = "lbl_aspect_ratio_arrow";
            lbl_aspect_ratio_arrow.Size = new System.Drawing.Size(23, 20);
            lbl_aspect_ratio_arrow.TabIndex = 52;
            lbl_aspect_ratio_arrow.Text = "<-";
            // 
            // lbl_aspect_ratio_separator
            // 
            lbl_aspect_ratio_separator.AutoSize = true;
            lbl_aspect_ratio_separator.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_aspect_ratio_separator.ForeColor = System.Drawing.Color.White;
            lbl_aspect_ratio_separator.Location = new System.Drawing.Point(533, 276);
            lbl_aspect_ratio_separator.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_aspect_ratio_separator.Name = "lbl_aspect_ratio_separator";
            lbl_aspect_ratio_separator.Size = new System.Drawing.Size(13, 20);
            lbl_aspect_ratio_separator.TabIndex = 53;
            lbl_aspect_ratio_separator.Text = ":";
            // 
            // lbl_vresult
            // 
            lbl_vresult.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_vresult.ForeColor = System.Drawing.Color.White;
            lbl_vresult.Location = new System.Drawing.Point(436, 627);
            lbl_vresult.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_vresult.Name = "lbl_vresult";
            lbl_vresult.Size = new System.Drawing.Size(200, 40);
            lbl_vresult.TabIndex = 27;
            lbl_vresult.Text = "33.9";
            lbl_vresult.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_vtitle
            // 
            lbl_vtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_vtitle.ForeColor = System.Drawing.Color.White;
            lbl_vtitle.Location = new System.Drawing.Point(436, 607);
            lbl_vtitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_vtitle.Name = "lbl_vtitle";
            lbl_vtitle.Size = new System.Drawing.Size(200, 27);
            lbl_vtitle.TabIndex = 26;
            lbl_vtitle.Text = "Vertical FOV Degrees";
            lbl_vtitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_hresult
            // 
            lbl_hresult.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_hresult.ForeColor = System.Drawing.Color.White;
            lbl_hresult.Location = new System.Drawing.Point(46, 627);
            lbl_hresult.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hresult.Name = "lbl_hresult";
            lbl_hresult.Size = new System.Drawing.Size(200, 40);
            lbl_hresult.TabIndex = 25;
            lbl_hresult.Text = "160.5";
            lbl_hresult.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_htitle
            // 
            lbl_htitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_htitle.ForeColor = System.Drawing.Color.White;
            lbl_htitle.Location = new System.Drawing.Point(47, 607);
            lbl_htitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_htitle.Name = "lbl_htitle";
            lbl_htitle.Size = new System.Drawing.Size(200, 27);
            lbl_htitle.TabIndex = 24;
            lbl_htitle.Text = "Horizontal FOV Degrees";
            lbl_htitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btn_save
            // 
            btn_save.BackColor = System.Drawing.Color.Black;
            btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_save.ForeColor = System.Drawing.Color.White;
            btn_save.Location = new System.Drawing.Point(287, 621);
            btn_save.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_save.Name = "btn_save";
            btn_save.Size = new System.Drawing.Size(107, 38);
            btn_save.TabIndex = 23;
            btn_save.Text = "&Save";
            btn_save.UseVisualStyleBackColor = false;
            btn_save.Click += btn_save_Click;
            // 
            // lbl_results
            // 
            lbl_results.AutoSize = true;
            lbl_results.BackColor = System.Drawing.Color.Black;
            lbl_results.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_results.ForeColor = System.Drawing.Color.White;
            lbl_results.Location = new System.Drawing.Point(203, 14);
            lbl_results.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_results.Name = "lbl_results";
            lbl_results.Size = new System.Drawing.Size(226, 29);
            lbl_results.TabIndex = 21;
            lbl_results.Text = "Game FOV Settings";
            lbl_results.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // rtb_results
            // 
            rtb_results.Location = new System.Drawing.Point(14, 60);
            rtb_results.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rtb_results.Name = "rtb_results";
            rtb_results.Size = new System.Drawing.Size(653, 535);
            rtb_results.TabIndex = 0;
            rtb_results.Text = "";
            // 
            // lbl_title
            // 
            lbl_title.AutoSize = true;
            lbl_title.BackColor = System.Drawing.Color.Black;
            lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_title.ForeColor = System.Drawing.Color.White;
            lbl_title.Location = new System.Drawing.Point(561, 16);
            lbl_title.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_title.Name = "lbl_title";
            lbl_title.Size = new System.Drawing.Size(338, 29);
            lbl_title.TabIndex = 20;
            lbl_title.Text = "Field of View (FOV) Calculator";
            lbl_title.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_about_fov
            // 
            lbl_about_fov.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_about_fov.ForeColor = System.Drawing.Color.White;
            lbl_about_fov.Location = new System.Drawing.Point(68, 65);
            lbl_about_fov.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_about_fov.Name = "lbl_about_fov";
            lbl_about_fov.Size = new System.Drawing.Size(1392, 59);
            lbl_about_fov.TabIndex = 21;
            lbl_about_fov.Text = resources.GetString("lbl_about_fov.Text");
            lbl_about_fov.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FovCalcForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(1518, 888);
            Controls.Add(lbl_about_fov);
            Controls.Add(lbl_title);
            Controls.Add(pnl_fov);
            Controls.Add(btn_back);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "FovCalcForm";
            ShowIcon = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Field of View (FOV) Calculator";
            Load += FovCalcForm_Load;
            pnl_fov.ResumeLayout(false);
            split_fov.Panel1.ResumeLayout(false);
            split_fov.Panel1.PerformLayout();
            split_fov.Panel2.ResumeLayout(false);
            split_fov.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)split_fov).EndInit();
            split_fov.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btn_back;
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
        private System.Windows.Forms.Label lbl_distance_to_screen_description;
        private System.Windows.Forms.Label lbl_aspect_ratio_description;
        private System.Windows.Forms.Label lbl_screen_size_description;
        private System.Windows.Forms.Label lbl_screen_type_description;
        private System.Windows.Forms.Label lbl_about_fov;
        private System.Windows.Forms.Label lbl_bezel_thickness_description;
        private System.Windows.Forms.Label lbl_distinace_to_screen_tip;
        private System.Windows.Forms.Label lbl_aspect_ratio_arrow;
        private System.Windows.Forms.Label lbl_aspect_ratio_separator;
        private System.Windows.Forms.Label lbl_results;
        private System.Windows.Forms.RichTextBox rtb_results;
        private System.Windows.Forms.Label lbl_vresult;
        private System.Windows.Forms.Label lbl_vtitle;
        private System.Windows.Forms.Label lbl_hresult;
        private System.Windows.Forms.Label lbl_htitle;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.LinkLabel llbl_markus_ewert;
    }
}