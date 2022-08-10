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
            this.btn_back = new System.Windows.Forms.Button();
            this.pnl_fov = new System.Windows.Forms.Panel();
            this.split_fov = new System.Windows.Forms.SplitContainer();
            this.lbl_bezel_thickness_description = new System.Windows.Forms.Label();
            this.lbl_distinace_to_screen_tip = new System.Windows.Forms.Label();
            this.lbl_screen_type = new System.Windows.Forms.Label();
            this.lbl_distance_to_screen = new System.Windows.Forms.Label();
            this.lbl_distance_to_screen_description = new System.Windows.Forms.Label();
            this.lbl_aspect_ratio_description = new System.Windows.Forms.Label();
            this.lbl_screen_size_description = new System.Windows.Forms.Label();
            this.lbl_screen_type_description = new System.Windows.Forms.Label();
            this.btn_triple_screens = new System.Windows.Forms.Button();
            this.btn_single_screen = new System.Windows.Forms.Button();
            this.lbl_bezel_thickness = new System.Windows.Forms.Label();
            this.cmb_bezel_thickness = new System.Windows.Forms.ComboBox();
            this.txt_bezel_thickness = new System.Windows.Forms.TextBox();
            this.cmb_distance_to_screen = new System.Windows.Forms.ComboBox();
            this.txt_distance_to_screen = new System.Windows.Forms.TextBox();
            this.lbl_aspect_ratio = new System.Windows.Forms.Label();
            this.lbl_screen_size = new System.Windows.Forms.Label();
            this.txt_aspect_ratio_y = new System.Windows.Forms.TextBox();
            this.txt_aspect_ratio_x = new System.Windows.Forms.TextBox();
            this.txt_screen_size = new System.Windows.Forms.TextBox();
            this.cmb_screen_size_units = new System.Windows.Forms.ComboBox();
            this.cmb_aspect_ratio = new System.Windows.Forms.ComboBox();
            this.lbl_aspect_ratio_arrow = new System.Windows.Forms.Label();
            this.lbl_aspect_ratio_separator = new System.Windows.Forms.Label();
            this.lbl_vresult = new System.Windows.Forms.Label();
            this.lbl_vtitle = new System.Windows.Forms.Label();
            this.lbl_hresult = new System.Windows.Forms.Label();
            this.lbl_htitle = new System.Windows.Forms.Label();
            this.btn_save = new System.Windows.Forms.Button();
            this.lbl_results = new System.Windows.Forms.Label();
            this.rtb_results = new System.Windows.Forms.RichTextBox();
            this.lbl_title = new System.Windows.Forms.Label();
            this.lbl_about_fov = new System.Windows.Forms.Label();
            this.llbl_markus_ewert = new System.Windows.Forms.LinkLabel();
            this.pnl_fov.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split_fov)).BeginInit();
            this.split_fov.Panel1.SuspendLayout();
            this.split_fov.Panel2.SuspendLayout();
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
            this.btn_back.Location = new System.Drawing.Point(1214, 735);
            this.btn_back.Name = "btn_back";
            this.btn_back.Size = new System.Drawing.Size(75, 23);
            this.btn_back.TabIndex = 6;
            this.btn_back.Text = "&Back";
            this.btn_back.UseVisualStyleBackColor = true;
            this.btn_back.Click += new System.EventHandler(this.btn_back_Click);
            // 
            // pnl_fov
            // 
            this.pnl_fov.Controls.Add(this.split_fov);
            this.pnl_fov.Location = new System.Drawing.Point(2, 128);
            this.pnl_fov.Name = "pnl_fov";
            this.pnl_fov.Size = new System.Drawing.Size(1301, 594);
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
            this.split_fov.Panel1.Controls.Add(this.llbl_markus_ewert);
            this.split_fov.Panel1.Controls.Add(this.lbl_bezel_thickness_description);
            this.split_fov.Panel1.Controls.Add(this.lbl_distinace_to_screen_tip);
            this.split_fov.Panel1.Controls.Add(this.lbl_screen_type);
            this.split_fov.Panel1.Controls.Add(this.lbl_distance_to_screen);
            this.split_fov.Panel1.Controls.Add(this.lbl_distance_to_screen_description);
            this.split_fov.Panel1.Controls.Add(this.lbl_aspect_ratio_description);
            this.split_fov.Panel1.Controls.Add(this.lbl_screen_size_description);
            this.split_fov.Panel1.Controls.Add(this.lbl_screen_type_description);
            this.split_fov.Panel1.Controls.Add(this.btn_triple_screens);
            this.split_fov.Panel1.Controls.Add(this.btn_single_screen);
            this.split_fov.Panel1.Controls.Add(this.lbl_bezel_thickness);
            this.split_fov.Panel1.Controls.Add(this.cmb_bezel_thickness);
            this.split_fov.Panel1.Controls.Add(this.txt_bezel_thickness);
            this.split_fov.Panel1.Controls.Add(this.cmb_distance_to_screen);
            this.split_fov.Panel1.Controls.Add(this.txt_distance_to_screen);
            this.split_fov.Panel1.Controls.Add(this.lbl_aspect_ratio);
            this.split_fov.Panel1.Controls.Add(this.lbl_screen_size);
            this.split_fov.Panel1.Controls.Add(this.txt_aspect_ratio_y);
            this.split_fov.Panel1.Controls.Add(this.txt_aspect_ratio_x);
            this.split_fov.Panel1.Controls.Add(this.txt_screen_size);
            this.split_fov.Panel1.Controls.Add(this.cmb_screen_size_units);
            this.split_fov.Panel1.Controls.Add(this.cmb_aspect_ratio);
            this.split_fov.Panel1.Controls.Add(this.lbl_aspect_ratio_arrow);
            this.split_fov.Panel1.Controls.Add(this.lbl_aspect_ratio_separator);
            // 
            // split_fov.Panel2
            // 
            this.split_fov.Panel2.BackColor = System.Drawing.Color.Black;
            this.split_fov.Panel2.Controls.Add(this.lbl_vresult);
            this.split_fov.Panel2.Controls.Add(this.lbl_vtitle);
            this.split_fov.Panel2.Controls.Add(this.lbl_hresult);
            this.split_fov.Panel2.Controls.Add(this.lbl_htitle);
            this.split_fov.Panel2.Controls.Add(this.btn_save);
            this.split_fov.Panel2.Controls.Add(this.lbl_results);
            this.split_fov.Panel2.Controls.Add(this.rtb_results);
            this.split_fov.Size = new System.Drawing.Size(1301, 594);
            this.split_fov.SplitterDistance = 711;
            this.split_fov.TabIndex = 0;
            // 
            // lbl_bezel_thickness_description
            // 
            this.lbl_bezel_thickness_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_bezel_thickness_description.ForeColor = System.Drawing.Color.IndianRed;
            this.lbl_bezel_thickness_description.Location = new System.Drawing.Point(24, 470);
            this.lbl_bezel_thickness_description.Name = "lbl_bezel_thickness_description";
            this.lbl_bezel_thickness_description.Size = new System.Drawing.Size(349, 50);
            this.lbl_bezel_thickness_description.TabIndex = 51;
            this.lbl_bezel_thickness_description.Text = "Lastly, if you are using triple screens, you need to measure the width of your be" +
    "zels at the sides of your screens. This is so that we can take account of how th" +
    "ey affect the field of view.";
            // 
            // lbl_distinace_to_screen_tip
            // 
            this.lbl_distinace_to_screen_tip.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_distinace_to_screen_tip.ForeColor = System.Drawing.Color.Gold;
            this.lbl_distinace_to_screen_tip.Location = new System.Drawing.Point(24, 374);
            this.lbl_distinace_to_screen_tip.Name = "lbl_distinace_to_screen_tip";
            this.lbl_distinace_to_screen_tip.Size = new System.Drawing.Size(350, 51);
            this.lbl_distinace_to_screen_tip.TabIndex = 50;
            this.lbl_distinace_to_screen_tip.Text = "(TIP: Try and place your screens as close to your eyes as your simracing rig will" +
    " allow. This will let you see more of what is happening around you)";
            // 
            // lbl_screen_type
            // 
            this.lbl_screen_type.AutoSize = true;
            this.lbl_screen_type.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_screen_type.ForeColor = System.Drawing.Color.White;
            this.lbl_screen_type.Location = new System.Drawing.Point(23, 21);
            this.lbl_screen_type.Name = "lbl_screen_type";
            this.lbl_screen_type.Size = new System.Drawing.Size(295, 20);
            this.lbl_screen_type.TabIndex = 7;
            this.lbl_screen_type.Text = "1. Single Screen or Triple Screen layout?";
            // 
            // lbl_distance_to_screen
            // 
            this.lbl_distance_to_screen.AutoSize = true;
            this.lbl_distance_to_screen.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_distance_to_screen.ForeColor = System.Drawing.Color.White;
            this.lbl_distance_to_screen.Location = new System.Drawing.Point(22, 318);
            this.lbl_distance_to_screen.Name = "lbl_distance_to_screen";
            this.lbl_distance_to_screen.Size = new System.Drawing.Size(314, 20);
            this.lbl_distance_to_screen.TabIndex = 12;
            this.lbl_distance_to_screen.Text = "4. How far are your eyes from the Screens?";
            // 
            // lbl_distance_to_screen_description
            // 
            this.lbl_distance_to_screen_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_distance_to_screen_description.ForeColor = System.Drawing.Color.IndianRed;
            this.lbl_distance_to_screen_description.Location = new System.Drawing.Point(23, 338);
            this.lbl_distance_to_screen_description.Name = "lbl_distance_to_screen_description";
            this.lbl_distance_to_screen_description.Size = new System.Drawing.Size(350, 36);
            this.lbl_distance_to_screen_description.TabIndex = 49;
            this.lbl_distance_to_screen_description.Text = "Now, you need to measure how far your eyes are away from the center of the middle" +
    " screen. I use a tape measure for this.";
            // 
            // lbl_aspect_ratio_description
            // 
            this.lbl_aspect_ratio_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_aspect_ratio_description.ForeColor = System.Drawing.Color.IndianRed;
            this.lbl_aspect_ratio_description.Location = new System.Drawing.Point(24, 238);
            this.lbl_aspect_ratio_description.Name = "lbl_aspect_ratio_description";
            this.lbl_aspect_ratio_description.Size = new System.Drawing.Size(349, 52);
            this.lbl_aspect_ratio_description.TabIndex = 48;
            this.lbl_aspect_ratio_description.Text = "Next, select the aspect ratio your of your screens. Check your display manual if " +
    "you are unsure, or visit https://whatismyresolution.com/";
            // 
            // lbl_screen_size_description
            // 
            this.lbl_screen_size_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_screen_size_description.ForeColor = System.Drawing.Color.IndianRed;
            this.lbl_screen_size_description.Location = new System.Drawing.Point(24, 136);
            this.lbl_screen_size_description.Name = "lbl_screen_size_description";
            this.lbl_screen_size_description.Size = new System.Drawing.Size(349, 58);
            this.lbl_screen_size_description.TabIndex = 47;
            this.lbl_screen_size_description.Text = "Next, measure the visible diagonal area of a single screen, ignoring the bezels. " +
    "Measure from the top left to the bottom right. Only measure the visible screen a" +
    "rea.";
            // 
            // lbl_screen_type_description
            // 
            this.lbl_screen_type_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_screen_type_description.ForeColor = System.Drawing.Color.IndianRed;
            this.lbl_screen_type_description.Location = new System.Drawing.Point(23, 39);
            this.lbl_screen_type_description.Name = "lbl_screen_type_description";
            this.lbl_screen_type_description.Size = new System.Drawing.Size(350, 50);
            this.lbl_screen_type_description.TabIndex = 46;
            this.lbl_screen_type_description.Text = "First step is to choose whether you wish to use a Single Screen, or a Triple Scre" +
    "en. Choose Single Screen if the game requires NVIDIA Surround or AMD Eyefinity.";
            // 
            // btn_triple_screens
            // 
            this.btn_triple_screens.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_triple_screens.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_triple_screens.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_triple_screens.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_triple_screens.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_triple_screens.ForeColor = System.Drawing.Color.White;
            this.btn_triple_screens.Location = new System.Drawing.Point(549, 55);
            this.btn_triple_screens.Name = "btn_triple_screens";
            this.btn_triple_screens.Size = new System.Drawing.Size(139, 33);
            this.btn_triple_screens.TabIndex = 45;
            this.btn_triple_screens.Text = "Triple Screens";
            this.btn_triple_screens.UseVisualStyleBackColor = true;
            this.btn_triple_screens.Click += new System.EventHandler(this.btn_triple_screens_Click);
            // 
            // btn_single_screen
            // 
            this.btn_single_screen.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_single_screen.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_single_screen.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_single_screen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_single_screen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_single_screen.ForeColor = System.Drawing.Color.White;
            this.btn_single_screen.Location = new System.Drawing.Point(389, 55);
            this.btn_single_screen.Name = "btn_single_screen";
            this.btn_single_screen.Size = new System.Drawing.Size(139, 33);
            this.btn_single_screen.TabIndex = 44;
            this.btn_single_screen.Text = "Single Screen";
            this.btn_single_screen.UseVisualStyleBackColor = true;
            this.btn_single_screen.Click += new System.EventHandler(this.btn_single_screen_Click);
            // 
            // lbl_bezel_thickness
            // 
            this.lbl_bezel_thickness.AutoSize = true;
            this.lbl_bezel_thickness.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_bezel_thickness.ForeColor = System.Drawing.Color.White;
            this.lbl_bezel_thickness.Location = new System.Drawing.Point(23, 450);
            this.lbl_bezel_thickness.Name = "lbl_bezel_thickness";
            this.lbl_bezel_thickness.Size = new System.Drawing.Size(150, 20);
            this.lbl_bezel_thickness.TabIndex = 15;
            this.lbl_bezel_thickness.Text = "5. Bezel Thickness?";
            // 
            // cmb_bezel_thickness
            // 
            this.cmb_bezel_thickness.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_bezel_thickness.FormattingEnabled = true;
            this.cmb_bezel_thickness.Location = new System.Drawing.Point(554, 470);
            this.cmb_bezel_thickness.Name = "cmb_bezel_thickness";
            this.cmb_bezel_thickness.Size = new System.Drawing.Size(133, 28);
            this.cmb_bezel_thickness.TabIndex = 14;
            this.cmb_bezel_thickness.SelectedIndexChanged += new System.EventHandler(this.cmb_bezel_thickness_SelectedIndexChanged);
            // 
            // txt_bezel_thickness
            // 
            this.txt_bezel_thickness.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_bezel_thickness.Location = new System.Drawing.Point(470, 470);
            this.txt_bezel_thickness.Name = "txt_bezel_thickness";
            this.txt_bezel_thickness.Size = new System.Drawing.Size(78, 26);
            this.txt_bezel_thickness.TabIndex = 13;
            this.txt_bezel_thickness.TextChanged += new System.EventHandler(this.txt_bezel_thickness_TextChanged);
            // 
            // cmb_distance_to_screen
            // 
            this.cmb_distance_to_screen.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_distance_to_screen.FormattingEnabled = true;
            this.cmb_distance_to_screen.Location = new System.Drawing.Point(554, 338);
            this.cmb_distance_to_screen.Name = "cmb_distance_to_screen";
            this.cmb_distance_to_screen.Size = new System.Drawing.Size(133, 28);
            this.cmb_distance_to_screen.TabIndex = 11;
            this.cmb_distance_to_screen.SelectedIndexChanged += new System.EventHandler(this.cmb_distance_to_screen_SelectedIndexChanged);
            // 
            // txt_distance_to_screen
            // 
            this.txt_distance_to_screen.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_distance_to_screen.Location = new System.Drawing.Point(470, 338);
            this.txt_distance_to_screen.Name = "txt_distance_to_screen";
            this.txt_distance_to_screen.Size = new System.Drawing.Size(78, 26);
            this.txt_distance_to_screen.TabIndex = 10;
            this.txt_distance_to_screen.TextChanged += new System.EventHandler(this.txt_distance_to_screen_TextChanged);
            // 
            // lbl_aspect_ratio
            // 
            this.lbl_aspect_ratio.AutoSize = true;
            this.lbl_aspect_ratio.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_aspect_ratio.ForeColor = System.Drawing.Color.White;
            this.lbl_aspect_ratio.Location = new System.Drawing.Point(23, 218);
            this.lbl_aspect_ratio.Name = "lbl_aspect_ratio";
            this.lbl_aspect_ratio.Size = new System.Drawing.Size(286, 20);
            this.lbl_aspect_ratio.TabIndex = 9;
            this.lbl_aspect_ratio.Text = "3. What Aspect Ratio are the Screens?";
            // 
            // lbl_screen_size
            // 
            this.lbl_screen_size.AutoSize = true;
            this.lbl_screen_size.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_screen_size.ForeColor = System.Drawing.Color.White;
            this.lbl_screen_size.Location = new System.Drawing.Point(23, 116);
            this.lbl_screen_size.Name = "lbl_screen_size";
            this.lbl_screen_size.Size = new System.Drawing.Size(266, 20);
            this.lbl_screen_size.TabIndex = 8;
            this.lbl_screen_size.Text = "2. What sized Screens do you have?";
            // 
            // txt_aspect_ratio_y
            // 
            this.txt_aspect_ratio_y.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_aspect_ratio_y.Location = new System.Drawing.Point(470, 238);
            this.txt_aspect_ratio_y.Name = "txt_aspect_ratio_y";
            this.txt_aspect_ratio_y.Size = new System.Drawing.Size(69, 26);
            this.txt_aspect_ratio_y.TabIndex = 6;
            this.txt_aspect_ratio_y.TextChanged += new System.EventHandler(this.txt_aspect_ratio_y_TextChanged);
            // 
            // txt_aspect_ratio_x
            // 
            this.txt_aspect_ratio_x.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_aspect_ratio_x.Location = new System.Drawing.Point(391, 238);
            this.txt_aspect_ratio_x.Name = "txt_aspect_ratio_x";
            this.txt_aspect_ratio_x.Size = new System.Drawing.Size(64, 26);
            this.txt_aspect_ratio_x.TabIndex = 5;
            this.txt_aspect_ratio_x.TextChanged += new System.EventHandler(this.txt_aspect_ratio_x_TextChanged);
            // 
            // txt_screen_size
            // 
            this.txt_screen_size.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_screen_size.Location = new System.Drawing.Point(470, 136);
            this.txt_screen_size.Name = "txt_screen_size";
            this.txt_screen_size.Size = new System.Drawing.Size(78, 26);
            this.txt_screen_size.TabIndex = 4;
            this.txt_screen_size.TextChanged += new System.EventHandler(this.txt_screen_size_TextChanged);
            // 
            // cmb_screen_size_units
            // 
            this.cmb_screen_size_units.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_screen_size_units.FormattingEnabled = true;
            this.cmb_screen_size_units.Location = new System.Drawing.Point(554, 136);
            this.cmb_screen_size_units.Name = "cmb_screen_size_units";
            this.cmb_screen_size_units.Size = new System.Drawing.Size(133, 28);
            this.cmb_screen_size_units.TabIndex = 3;
            this.cmb_screen_size_units.SelectedIndexChanged += new System.EventHandler(this.cmb_screen_size_units_SelectedIndexChanged);
            // 
            // cmb_aspect_ratio
            // 
            this.cmb_aspect_ratio.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_aspect_ratio.FormattingEnabled = true;
            this.cmb_aspect_ratio.Location = new System.Drawing.Point(567, 238);
            this.cmb_aspect_ratio.Name = "cmb_aspect_ratio";
            this.cmb_aspect_ratio.Size = new System.Drawing.Size(120, 28);
            this.cmb_aspect_ratio.TabIndex = 2;
            this.cmb_aspect_ratio.SelectedIndexChanged += new System.EventHandler(this.cmb_aspect_ratio_SelectedIndexChanged);
            // 
            // lbl_aspect_ratio_arrow
            // 
            this.lbl_aspect_ratio_arrow.AutoSize = true;
            this.lbl_aspect_ratio_arrow.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_aspect_ratio_arrow.ForeColor = System.Drawing.Color.White;
            this.lbl_aspect_ratio_arrow.Location = new System.Drawing.Point(542, 241);
            this.lbl_aspect_ratio_arrow.Name = "lbl_aspect_ratio_arrow";
            this.lbl_aspect_ratio_arrow.Size = new System.Drawing.Size(23, 20);
            this.lbl_aspect_ratio_arrow.TabIndex = 52;
            this.lbl_aspect_ratio_arrow.Text = "<-";
            // 
            // lbl_aspect_ratio_separator
            // 
            this.lbl_aspect_ratio_separator.AutoSize = true;
            this.lbl_aspect_ratio_separator.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_aspect_ratio_separator.ForeColor = System.Drawing.Color.White;
            this.lbl_aspect_ratio_separator.Location = new System.Drawing.Point(457, 239);
            this.lbl_aspect_ratio_separator.Name = "lbl_aspect_ratio_separator";
            this.lbl_aspect_ratio_separator.Size = new System.Drawing.Size(13, 20);
            this.lbl_aspect_ratio_separator.TabIndex = 53;
            this.lbl_aspect_ratio_separator.Text = ":";
            // 
            // lbl_vresult
            // 
            this.lbl_vresult.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_vresult.ForeColor = System.Drawing.Color.White;
            this.lbl_vresult.Location = new System.Drawing.Point(374, 543);
            this.lbl_vresult.Name = "lbl_vresult";
            this.lbl_vresult.Size = new System.Drawing.Size(171, 35);
            this.lbl_vresult.TabIndex = 27;
            this.lbl_vresult.Text = "33.9";
            this.lbl_vresult.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_vtitle
            // 
            this.lbl_vtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_vtitle.ForeColor = System.Drawing.Color.White;
            this.lbl_vtitle.Location = new System.Drawing.Point(374, 526);
            this.lbl_vtitle.Name = "lbl_vtitle";
            this.lbl_vtitle.Size = new System.Drawing.Size(171, 23);
            this.lbl_vtitle.TabIndex = 26;
            this.lbl_vtitle.Text = "Vertical FOV Degrees";
            this.lbl_vtitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_hresult
            // 
            this.lbl_hresult.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_hresult.ForeColor = System.Drawing.Color.White;
            this.lbl_hresult.Location = new System.Drawing.Point(39, 543);
            this.lbl_hresult.Name = "lbl_hresult";
            this.lbl_hresult.Size = new System.Drawing.Size(171, 35);
            this.lbl_hresult.TabIndex = 25;
            this.lbl_hresult.Text = "160.5";
            this.lbl_hresult.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_htitle
            // 
            this.lbl_htitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_htitle.ForeColor = System.Drawing.Color.White;
            this.lbl_htitle.Location = new System.Drawing.Point(40, 526);
            this.lbl_htitle.Name = "lbl_htitle";
            this.lbl_htitle.Size = new System.Drawing.Size(171, 23);
            this.lbl_htitle.TabIndex = 24;
            this.lbl_htitle.Text = "Horizontal FOV Degrees";
            this.lbl_htitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btn_save
            // 
            this.btn_save.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_save.BackColor = System.Drawing.Color.Black;
            this.btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_save.ForeColor = System.Drawing.Color.White;
            this.btn_save.Location = new System.Drawing.Point(246, 538);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(92, 33);
            this.btn_save.TabIndex = 23;
            this.btn_save.Text = "&Save";
            this.btn_save.UseVisualStyleBackColor = false;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // lbl_results
            // 
            this.lbl_results.AutoSize = true;
            this.lbl_results.BackColor = System.Drawing.Color.Black;
            this.lbl_results.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_results.ForeColor = System.Drawing.Color.White;
            this.lbl_results.Location = new System.Drawing.Point(174, 12);
            this.lbl_results.Name = "lbl_results";
            this.lbl_results.Size = new System.Drawing.Size(226, 29);
            this.lbl_results.TabIndex = 21;
            this.lbl_results.Text = "Game FOV Settings";
            this.lbl_results.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // rtb_results
            // 
            this.rtb_results.Location = new System.Drawing.Point(12, 52);
            this.rtb_results.Name = "rtb_results";
            this.rtb_results.Size = new System.Drawing.Size(560, 464);
            this.rtb_results.TabIndex = 0;
            this.rtb_results.Text = "";
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
            // lbl_about_fov
            // 
            this.lbl_about_fov.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_about_fov.ForeColor = System.Drawing.Color.White;
            this.lbl_about_fov.Location = new System.Drawing.Point(58, 56);
            this.lbl_about_fov.Name = "lbl_about_fov";
            this.lbl_about_fov.Size = new System.Drawing.Size(1193, 51);
            this.lbl_about_fov.TabIndex = 21;
            this.lbl_about_fov.Text = resources.GetString("lbl_about_fov.Text");
            this.lbl_about_fov.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // llbl_markus_ewert
            // 
            this.llbl_markus_ewert.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.llbl_markus_ewert.AutoSize = true;
            this.llbl_markus_ewert.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.llbl_markus_ewert.LinkColor = System.Drawing.Color.LightSkyBlue;
            this.llbl_markus_ewert.Location = new System.Drawing.Point(216, 567);
            this.llbl_markus_ewert.Name = "llbl_markus_ewert";
            this.llbl_markus_ewert.Size = new System.Drawing.Size(312, 13);
            this.llbl_markus_ewert.TabIndex = 54;
            this.llbl_markus_ewert.TabStop = true;
            this.llbl_markus_ewert.Text = "A massive thanks to Markus Ewert for permission to use his code";
            this.llbl_markus_ewert.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llbl_markus_ewert_LinkClicked);
            // 
            // FovCalcForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1301, 770);
            this.Controls.Add(this.lbl_about_fov);
            this.Controls.Add(this.lbl_title);
            this.Controls.Add(this.pnl_fov);
            this.Controls.Add(this.btn_back);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FovCalcForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Field of View (FOV) Calculator";
            this.Load += new System.EventHandler(this.FovCalcForm_Load);
            this.pnl_fov.ResumeLayout(false);
            this.split_fov.Panel1.ResumeLayout(false);
            this.split_fov.Panel1.PerformLayout();
            this.split_fov.Panel2.ResumeLayout(false);
            this.split_fov.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split_fov)).EndInit();
            this.split_fov.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

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